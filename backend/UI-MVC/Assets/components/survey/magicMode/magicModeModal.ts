import { getSTTManager } from '../../../services/speechService';
import { detectLocale, getSurveyStrings } from '../../../i18n/survey';
import { createBubbleList } from './bubbleList';

// ============================================================================
// Constants
// ============================================================================

const MAX_PHRASES = 2
const CACHE_MAX_SIZE = 100
const FEEDBACK_DURATION_MS = 3000

// ============================================================================
// ============================================================================

export interface MagicModeModalController {
    open(questionText: string, onClose: (text: string) => void): void;
    destroy(): void;
}

export function createMagicModeModal(): MagicModeModalController {
    const stt = getSTTManager();
    const t = getSurveyStrings();
    let isRecording = false;
    let onCloseCallback: ((text: string) => void) | null = null;
    const bubbleList = createBubbleList();
    
    // Mic volume animation
    let micContainer: HTMLElement | null = null;
    let unsubscribeVolume: (() => void) | null = null;
    
    // Status ring state
    let transcriptInProgress = false;
    let aiInProgress = false;
    let transcriptWrapperEl: HTMLElement | null = null;
    let aiWrapperEl: HTMLElement | null = null;
    
    // Update ring state classes on individual wrappers
    function updateRingState(): void {
        if (transcriptWrapperEl) {
            transcriptWrapperEl.classList.toggle('transcribing', transcriptInProgress);
        }
        if (aiWrapperEl) {
            aiWrapperEl.classList.toggle('thinking', aiInProgress);
        }
    }
    
    // AI validation state
    let pendingValidation = false;
    const phraseCache = new Map<string, {phrases: string[], rejected: Array<{phrase: string, reason: string}>}>();

    function showRejectedFeedback(rejected: Array<{phrase: string, reason: string}>): void {
      if (rejected.length === 0) return;
      
      const feedbackElement = document.createElement('div');
      feedbackElement.className = 'magic-mode-feedback';
      
      // Group by reason and show specific phrases
      const reasonGroups: Record<string, string[]> = {};
      rejected.forEach(r => {
        if (!reasonGroups[r.reason]) reasonGroups[r.reason] = [];
        reasonGroups[r.reason].push(r.phrase);
      });

      const reasonText = Object.entries(reasonGroups)
        .map(([reason, phrases]) => {
          const reasonLabel = getReasonText(reason);
          const phraseList = phrases.map(p => `"${p}"`).join(', ');
          return phrases.length === 1 
            ? `"${phrases[0]}" (${reasonLabel})`
            : `${phrases.length}x ${reasonLabel}: ${phraseList}`;
        })
        .join('; ');

      feedbackElement.textContent = `${t.rejectedFeedbackPrefix}${reasonText}`;
      document.body.appendChild(feedbackElement);

      setTimeout(() => feedbackElement.remove(), FEEDBACK_DURATION_MS);
    }

    function getReasonText(reason: string): string {
      const reasonMap: Record<string, string> = {
        'WordCountTooLow': t.rejectionWordCountTooLow,
        'WordCountExceeded': t.rejectionWordCountExceeded,
        'DuplicateExact': t.rejectionDuplicateExact,
        'DuplicateSemantic': t.rejectionDuplicateSemantic,
        'SubsetOfExisting': t.rejectionSubsetOfExisting,
        'FillerContent': t.rejectionFillerContent,
        'TooGeneric': t.rejectionTooGeneric
      };
      return reasonMap[reason] || reason;
    }

    function buildDOM(): { backdrop: HTMLElement; dialog: HTMLElement } {
        const backdrop = document.createElement('div');
        backdrop.className = 'magic-mode-backdrop';

        const dialog = document.createElement('div');
        dialog.className = 'magic-mode-dialog';
        dialog.setAttribute('role', 'dialog');
        dialog.setAttribute('aria-modal', 'true');
        dialog.setAttribute('aria-label', t.magicModeAriaLabel);

        // Header
        const header = document.createElement('div');
        header.className = 'magic-mode-dialog-header';

        const title = document.createElement('h3');
        title.className = 'text-xs flex items-center';

        const icon = document.createElement('div');
        icon.className = 'w-6 h-6';
        icon.innerHTML = '<svg xmlns="http://www.w3.org/2000/svg" width="70%" height="100%" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="lucide lucide-wand-sparkles-icon lucide-wand-sparkles"><path d="m21.64 3.64-1.28-1.28a1.21 1.21 0 0 0-1.72 0L2.36 18.64a1.21 1.21 0 0 0 0 1.72l1.28 1.28a1.2 1.2 0 0 0 1.72 0L21.64 5.36a1.2 1.2 0 0 0 0-1.72"/><path d="m14 7 3 3"/><path d="M5 6v4"/><path d="M19 14v4"/><path d="M10 2v2"/><path d="M7 8H3"/><path d="M21 16h-4"/><path d="M11 3H9"/></svg>';

        const UpperMagicModeText = document.createElement('span');
        UpperMagicModeText.innerHTML = t.magicModeActivated;

        title.appendChild(icon);
        title.appendChild(UpperMagicModeText);

        const closeBtn = document.createElement('button');
        closeBtn.className = 'btn btn-ghost btn-sm btn-circle';
        closeBtn.setAttribute('aria-label', t.close);
        closeBtn.innerHTML = '<svg xmlns="http://www.w3.org/2000/svg" fill="none" width="24" height="24" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" class="w-5 h-5"><path stroke-linecap="round" stroke-linejoin="round" d="M6 18 18 6M6 6l12 12" /></svg>';
        closeBtn.addEventListener('click', closeModal);

        header.appendChild(title);
        header.appendChild(closeBtn);

        // Body
        const body = document.createElement('div');
        body.className = 'magic-mode-dialog-body';

        const questionEl = document.createElement('h4');
        questionEl.className = 'magic-mode-question text-base font-medium text-base-content';

        body.appendChild(questionEl);
        body.appendChild(bubbleList.element);

        // Ring container - dedicated space for status rings
        const ringContainer = document.createElement('div');
        ringContainer.className = 'magic-mode-ring-container';

        // Transcript wrapper - slower orbit
        const transcriptWrapper = document.createElement('div');
        transcriptWrapper.className = 'magic-mode-ring-wrapper magic-mode-transcript-wrapper';

        // AI wrapper - faster orbit
        const aiWrapper = document.createElement('div');
        aiWrapper.className = 'magic-mode-ring-wrapper magic-mode-ai-wrapper';

        const transcriptRing = document.createElement('div');
        transcriptRing.className = 'magic-mode-status-ring magic-mode-transcript-ring';

        const aiRing = document.createElement('div');
        aiRing.className = 'magic-mode-status-ring magic-mode-ai-ring';

        // Assemble: both wrappers at same center point
        transcriptWrapper.appendChild(transcriptRing);
        aiWrapper.appendChild(aiRing);
        ringContainer.append(transcriptWrapper, aiWrapper);
        body.appendChild(ringContainer);

        // Store references for state management
        transcriptWrapperEl = transcriptWrapper;
        aiWrapperEl = aiWrapper;

        // Footer
        const footer = document.createElement('div');
        footer.className = 'magic-mode-dialog-footer';

        // Mic container for volume animation
        const micContainerEl = document.createElement('div');
        micContainerEl.className = 'magic-mode-mic-container';

        // Filled circle that expands with volume
        const micFill = document.createElement('div');
        micFill.className = 'magic-mode-mic-fill';

        const micBtn = document.createElement('button');
        micBtn.className = 'btn btn-circle btn-lg magic-mode-mic-btn';
        micBtn.setAttribute('aria-label', t.micStartRecording);
        micBtn.setAttribute('aria-pressed', 'false');
        micBtn.innerHTML = '<svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" class="w-7 h-7"><path stroke-linecap="round" stroke-linejoin="round" d="M12 18.75a6 6 0 0 0 6-6v-1.5m-6 7.5a6 6 0 0 1-6-6v-1.5m6 7.5v3.75m-3.75 0h7.5M12 15.75a3 3 0 0 1-3-3V4.5a3 3 0 1 1 6 0v8.25a3 3 0 0 1-3 3Z" /></svg>';

        micContainerEl.appendChild(micFill);
        micContainerEl.appendChild(micBtn);

        const micHint = document.createElement('p');
        micHint.className = 'text-xs text-base-content/50 magic-mode-mic-hint';
        micHint.textContent = t.micClickToSpeak;

        micBtn.addEventListener('click', () => {
            isRecording = !isRecording;
            if (isRecording) {
                transcriptInProgress = true;
                aiInProgress = false;
                updateRingState();
                stt.start(null, detectLocale(), onTranscript);
                // Setup volume callback
                unsubscribeVolume = stt.onVolume((volume: number) => {
                    if (micContainerEl) {
                        micContainerEl.style.setProperty('--mic-volume', String(volume));
                    }
                });
            } else {
                stt.stop();
                transcriptInProgress = false;
                aiInProgress = false;
                updateRingState();
                // Clean up volume callback
                if (unsubscribeVolume) {
                    unsubscribeVolume();
                    unsubscribeVolume = null;
                }
                // Reset volume to 0 when stopping
                if (micContainerEl) {
                    micContainerEl.style.setProperty('--mic-volume', '0');
                }
            }
            // Toggle recording classes
            micContainerEl.classList.toggle('recording', isRecording);
            micBtn.classList.toggle('recording', isRecording);
            micHint.classList.toggle('hidden', isRecording);
            transcriptWrapperEl?.classList.toggle('recording', isRecording);
            aiWrapperEl?.classList.toggle('recording', isRecording);
            micBtn.setAttribute('aria-pressed', String(isRecording));
            micBtn.setAttribute('aria-label', isRecording ? t.micStopRecording : t.micStartRecording);
        });

        micContainerEl.appendChild(micHint);
        footer.appendChild(micContainerEl);
        
        // Store reference for cleanup
        micContainer = micContainerEl;

        dialog.appendChild(header);
        dialog.appendChild(body);
        dialog.appendChild(footer);

        backdrop.addEventListener('click', closeModal);

        return { backdrop, dialog };
    }

    let activeBackdrop: HTMLElement | null = null;
    let activeDialog: HTMLElement | null = null;

    function generateCacheKey(
        transcript: string,
        language: string,
        maxPhrases: number,
        existingPhrases: string[],
        rejectedPhrases: string[]
    ): string {
        const normalizedTranscript = transcript.trim().toLowerCase();
        const normalizedExisting = existingPhrases
            .map(p => p.trim().toLowerCase())
            .sort()
            .join('|');
        const normalizedRejected = rejectedPhrases
            .map(p => p.trim().toLowerCase())
            .sort()
            .join('|');
        return `${normalizedTranscript}|${language}|${maxPhrases}|${normalizedExisting}|${normalizedRejected}`;
    }

    function cleanupCache(): void {
        if (phraseCache.size > CACHE_MAX_SIZE) {
            const keys = Array.from(phraseCache.keys());
            for (let i = 0; i < keys.length - (CACHE_MAX_SIZE / 2); i++) {
                phraseCache.delete(keys[i]);
            }
        }
    }

    async function fetchAndCachePhrases(text: string, cacheKey: string): Promise<{phrases: string[], rejected: Array<{phrase: string, reason: string}>}> {
        const result = await fetchKeyPhrases(text, bubbleList.getBubbles(), bubbleList.getRejectedPhrases());
        
        if (result.phrases.length > 0) {
            phraseCache.set(cacheKey, { phrases: result.phrases, rejected: result.rejectedPhrasesWithReasons || [] });
            cleanupCache();
        }
        
        return {
            phrases: result.phrases,
            rejected: result.rejectedPhrasesWithReasons || []
        };
    }

    async function onTranscript(text: string): Promise<void> {
        if (!text.trim()) return;

        // Debouncing: if validation is already in progress, skip
        if (pendingValidation) return;
        pendingValidation = true;
        
        // STT is done, now starting AI processing
        transcriptInProgress = false;
        aiInProgress = true;
        updateRingState();

        // Generate cache key
        const cacheKey = generateCacheKey(
            text,
            detectLocale(),
            MAX_PHRASES,
            bubbleList.getBubbles(),
            bubbleList.getRejectedPhrases()
        );

        // Check cache
        if (phraseCache.has(cacheKey)) {
            const cached = phraseCache.get(cacheKey)!;
            if (cached.phrases.length > 0) {
                // Add accepted phrases directly as permanent bubbles
                bubbleList.addBubbles(cached.phrases);
            }
            aiInProgress = false;
            pendingValidation = false;
            updateRingState();
            return;
        }

        try {
            const result = await fetchAndCachePhrases(text, cacheKey);

            // Add ONLY accepted phrases as PERMANENT bubbles
            if (result.phrases.length > 0) {
                bubbleList.addBubbles(result.phrases);
            }

            // Optional: show rejected phrases as feedback
            if (result.rejected.length > 0) {
                showRejectedFeedback(result.rejected);
            }
        } catch (error) {
            console.error('[MagicMode] Validation failed:', error);
            // Fallback: add as temporary bubble (for emergencies)
            bubbleList.addTemporaryBubbles([text]);
        } finally {
            aiInProgress = false;
            pendingValidation = false;
            updateRingState();
            
            // Trigger AI completion animation (360° orbit + glow in 1s)
            if (aiWrapperEl) {
                aiWrapperEl.classList.add('ai-complete');
                setTimeout(() => aiWrapperEl?.classList.remove('ai-complete'), 1000);
            }
        }
    }

    function closeModal(): void {
        if (isRecording) {
            stt.stop();
            isRecording = false;
        }
        // Clean up volume callback
        if (unsubscribeVolume) {
            unsubscribeVolume();
            unsubscribeVolume = null;
        }
        if (micContainer) {
            micContainer.style.setProperty('--mic-volume', '0');
        }
        micContainer = null;
        
        // Reset ring states
        transcriptInProgress = false;
        aiInProgress = false;
        updateRingState();
        transcriptWrapperEl?.classList.remove('recording');
        aiWrapperEl?.classList.remove('recording');
        transcriptWrapperEl = null;
        aiWrapperEl = null;
        
        const text = bubbleList.getBubbles().join(', ');
        
        activeBackdrop?.remove();
        activeDialog?.remove();
        activeBackdrop = null;
        activeDialog = null;
        
        // Reset state
        pendingValidation = false;
        phraseCache.clear();
        
        onCloseCallback?.(text);
    }

    return {
        open(questionText: string, onClose: (text: string) => void): void {
            onCloseCallback = onClose;
            bubbleList.reset();
            
            // Reset state for new modal
            pendingValidation = false;
            phraseCache.clear();
            transcriptWrapperEl = null;
            aiWrapperEl = null;
            transcriptInProgress = false;
            aiInProgress = false;

            const { backdrop, dialog } = buildDOM();
            activeBackdrop = backdrop;
            activeDialog = dialog;

            const questionEl = dialog.querySelector('h4') ?? dialog.querySelector('p');
            if (questionEl) questionEl.textContent = questionText;

            document.body.appendChild(backdrop);
            document.body.appendChild(dialog);
        },
        destroy(): void {
            if (isRecording) stt.stop();
            // Clean up volume callback
            if (unsubscribeVolume) {
                unsubscribeVolume();
                unsubscribeVolume = null;
            }
            if (micContainer) {
                micContainer.style.setProperty('--mic-volume', '0');
            }
            micContainer = null;
            
            // Reset ring states
            transcriptInProgress = false;
            aiInProgress = false;
            updateRingState();
            transcriptWrapperEl?.classList.remove('recording');
            aiWrapperEl?.classList.remove('recording');
            transcriptWrapperEl = null;
            aiWrapperEl = null;
            
            activeBackdrop?.remove();
            activeDialog?.remove();
            activeBackdrop = null;
            activeDialog = null;
            
            // Clean up bubble list to prevent memory leaks
            bubbleList.reset();
            
            // Reset state
            pendingValidation = false;
            phraseCache.clear();
        }
    };
}

async function fetchKeyPhrases(
    transcript: string,
    existingPhrases: string[],
    rejectedPhrases: string[]
): Promise<{phrases: string[], rejectedPhrasesWithReasons: Array<{phrase: string, reason: string}>}> {
    try {
        const response = await fetch('/api/magic-mode/key-phrases', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                transcript,
                language: detectLocale(),
                maxPhrases: MAX_PHRASES,
                existingPhrases,
                rejectedPhrases
            })
        });
        if (!response.ok) return { phrases: [], rejectedPhrasesWithReasons: [] };
        const data = await response.json() as {
            phrases: string[],
            rejectedPhrasesWithReasons?: Array<{phrase: string, reason: number}>,
        };
        
        // Convert numeric reason codes to human-readable strings
        const reasonMap: Record<number, string> = {
            0: 'None',
            1: 'WordCountTooLow',
            2: 'WordCountExceeded',
            3: 'DuplicateExact',
            4: 'DuplicateSemantic',
            5: 'SubsetOfExisting',
            6: 'FillerContent',
            7: 'TooGeneric'
        };
        
        const convertedRejected = (data.rejectedPhrasesWithReasons || []).map(r => ({
            phrase: r.phrase,
            reason: reasonMap[r.reason] || 'Unknown'
        }));
        
        return { phrases: data.phrases ?? [], rejectedPhrasesWithReasons: convertedRejected };
    } catch {
        return { phrases: [], rejectedPhrasesWithReasons: [] };
    }
}
