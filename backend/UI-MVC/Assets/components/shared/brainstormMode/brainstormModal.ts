/**
 * Brainstorm Mode Modal - AI-powered phrase extraction and suggestion modal
 * 
 * Provides a modal interface for users to speak their ideas, which are then
 * transcribed and sent to the AI for key phrase extraction. Displays extracted
 * phrases as interactive bubbles that can be selected to populate form fields.
 * 
 * Features:
 * - STT integration for voice input
 * - AI-powered key phrase extraction
 * - Phrase caching for performance
 * - Feedback display for rejected phrases
 * - Animated status rings for STT/AI state
 * - Volume visualization for microphone input
 */
import { getSTTManager } from '../../../services/speechService';
import { detectLocale, getSurveyStrings } from '../../../i18n/survey';
import { createBubbleList } from './bubbleList';
import { createRingController } from './ringController';
import { createPhraseCache, generateCacheKey } from './phraseCache';
import { createFeedbackController } from './feedbackController';
import type { RejectedPhrase, PhraseRejectionReason } from './types';

// ============================================================================
// Constants
// ============================================================================

const MAX_PHRASES = 2;

// ============================================================================
// ============================================================================

export interface BrainstormModalController {
    open(questionText: string, onClose: (text: string) => void): void;
    destroy(): void;
}

export function createBrainstormModal(): BrainstormModalController {
    const stt = getSTTManager();
    const t = getSurveyStrings();
    let isRecording = false;
    let onCloseCallback: ((text: string) => void) | null = null;
    const bubbleList = createBubbleList();
    const ringController = createRingController();
    const phraseCache = createPhraseCache({ maxSize: 100 });
    const feedbackController = createFeedbackController({ durationMs: 3000 });
    
    // Mic volume animation
    let micContainer: HTMLElement | null = null;
    let micBtn: HTMLButtonElement | null = null;
    let micHint: HTMLElement | null = null;
    let unsubscribeVolume: (() => void) | null = null;
    
    // AI validation state
    let pendingValidation = false;
    
    // Track full transcript for final text generation
    let fullTranscript = '';

    // Sync UI state with STT state changes
    function handleSTTStateChange(state: string): void {
        if (state === 'idle' && isRecording) {
            // STT stopped automatically (e.g., 60s timeout), update UI
            isRecording = false;
            ringController.stopAll();
            ringController.setRecordingState(false);
            
            // Clean up volume callback
            if (unsubscribeVolume) {
                unsubscribeVolume();
                unsubscribeVolume = null;
            }
            
            // Reset volume and recording classes
            if (micContainer) {
                micContainer.style.setProperty('--mic-volume', '0');
                micContainer.classList.remove('recording');
            }
            if (micBtn) {
                micBtn.classList.remove('recording');
                micBtn.setAttribute('aria-pressed', 'false');
                micBtn.setAttribute('aria-label', t.micStartRecording);
            }
            if (micHint) {
                micHint.classList.remove('hidden');
            }
        }
    }

    function buildDOM(): { backdrop: HTMLElement; dialog: HTMLElement } {
        const backdrop = document.createElement('div');
        backdrop.className = 'brainstorm-backdrop';

        const dialog = document.createElement('div');
        dialog.className = 'brainstorm-dialog';
        dialog.setAttribute('role', 'dialog');
        dialog.setAttribute('aria-modal', 'true');
        dialog.setAttribute('aria-label', t.brainstormModeAriaLabel);

        // Header
        const header = document.createElement('div');
        header.className = 'brainstorm-dialog-header';

        const title = document.createElement('h3');
        title.className = 'text-xs flex items-center';

        const icon = document.createElement('div');
        icon.className = 'w-6 h-6';
        icon.innerHTML = '<svg xmlns="http://www.w3.org/2000/svg" width="70%" height="100%" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="lucide lucide-wand-sparkles-icon lucide-wand-sparkles"><path d="m21.64 3.64-1.28-1.28a1.21 1.21 0 0 0-1.72 0L2.36 18.64a1.21 1.21 0 0 0 0 1.72l1.28 1.28a1.2 1.2 0 0 0 1.72 0L21.64 5.36a1.2 1.2 0 0 0 0-1.72"/><path d="m14 7 3 3"/><path d="M5 6v4"/><path d="M19 14v4"/><path d="M10 2v2"/><path d="M7 8H3"/><path d="M21 16h-4"/><path d="M11 3H9"/></svg>';

        const UpperBrainstormModeText = document.createElement('span');
        UpperBrainstormModeText.innerHTML = t.brainstormModeActivated;

        title.appendChild(icon);
        title.appendChild(UpperBrainstormModeText);

        const closeBtn = document.createElement('button');
        closeBtn.className = 'btn btn-ghost btn-sm btn-circle';
        closeBtn.setAttribute('aria-label', t.close);
        closeBtn.innerHTML = '<svg xmlns="http://www.w3.org/2000/svg" fill="none" width="24" height="24" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" class="w-5 h-5"><path stroke-linecap="round" stroke-linejoin="round" d="M6 18 18 6M6 6l12 12" /></svg>';
        closeBtn.addEventListener('click', async () => {
            await closeModal();
        });

        header.appendChild(title);
        header.appendChild(closeBtn);

        // Body
        const body = document.createElement('div');
        body.className = 'brainstorm-dialog-body';

        const questionEl = document.createElement('h4');
        questionEl.className = 'brainstorm-question text-base font-medium text-base-content';

        const instructionEl = document.createElement('p');
        instructionEl.className = 'brainstorm-instruction text-sm text-base-content/60 text-center mt-2';
        instructionEl.textContent = t.brainstormInstruction;

        body.appendChild(questionEl);
        body.appendChild(bubbleList.element);
        body.appendChild(instructionEl);

        // Ring container - use RingController for status ring animations
        body.appendChild(ringController.element);

        // Footer
        const footer = document.createElement('div');
        footer.className = 'brainstorm-dialog-footer';

        // Mic container for volume animation
        const micContainerEl = document.createElement('div');
        micContainerEl.className = 'brainstorm-mic-container';

        // Filled circle that expands with volume
        const micFill = document.createElement('div');
        micFill.className = 'brainstorm-mic-fill';

        const micBtnEl = document.createElement('button');
        micBtnEl.className = 'btn btn-circle btn-lg brainstorm-mic-btn';
        micBtnEl.setAttribute('aria-label', t.micStartRecording);
        micBtnEl.setAttribute('aria-pressed', 'false');
        micBtnEl.innerHTML = '<svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" class="w-7 h-7"><path stroke-linecap="round" stroke-linejoin="round" d="M12 18.75a6 6 0 0 0 6-6v-1.5m-6 7.5a6 6 0 0 1-6-6v-1.5m6 7.5v3.75m-3.75 0h7.5M12 15.75a3 3 0 0 1-3-3V4.5a3 3 0 1 1 6 0v8.25a3 3 0 0 1-3 3Z" /></svg>';

        micContainerEl.appendChild(micFill);
        micContainerEl.appendChild(micBtnEl);

        const micHintEl = document.createElement('p');
        micHintEl.className = 'text-xs text-base-content/50 brainstorm-mic-hint';
        micHintEl.textContent = t.micClickToSpeak;

        micBtnEl.addEventListener('click', () => {
            isRecording = !isRecording;
            if (isRecording) {
                ringController.startTranscribing();
                stt.start(null, detectLocale(), onTranscript);
                // Setup volume callback
                unsubscribeVolume = stt.onVolume((volume: number) => {
                    if (micContainerEl) {
                        micContainerEl.style.setProperty('--mic-volume', String(volume));
                    }
                });
            } else {
                stt.stop();
                ringController.stopAll();
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
            ringController.setRecordingState(isRecording);
            micContainerEl.classList.toggle('recording', isRecording);
            micBtnEl.classList.toggle('recording', isRecording);
            micHintEl.classList.toggle('hidden', isRecording);
            micBtnEl.setAttribute('aria-pressed', String(isRecording));
            micBtnEl.setAttribute('aria-label', isRecording ? t.micStopRecording : t.micStartRecording);
        });

        micContainerEl.appendChild(micHintEl);
        footer.appendChild(micContainerEl);
        
        // Store references for cleanup
        micContainer = micContainerEl;
        micBtn = micBtnEl;
        micHint = micHintEl;
        
        // Setup STT state change callback for automatic stops (e.g., 60s timeout)
        stt.setupCallbacks({ onStateChange: handleSTTStateChange });

        dialog.appendChild(header);
        dialog.appendChild(body);
        dialog.appendChild(footer);

        backdrop.addEventListener('click', closeModal);

        return { backdrop, dialog };
    }

    let activeBackdrop: HTMLElement | null = null;
    let activeDialog: HTMLElement | null = null;

    async function fetchAndCachePhrases(text: string, cacheKey: string): Promise<{phrases: string[], rejected: RejectedPhrase[]}> {
        const result = await fetchKeyPhrases(text, bubbleList.getBubbles(), bubbleList.getRejectedPhrases());
        
        if (result.phrases.length > 0) {
            phraseCache.set(cacheKey, { phrases: result.phrases, rejected: result.rejectedPhrasesWithReasons });
            phraseCache.cleanup();
        }
        
        return {
            phrases: result.phrases,
            rejected: result.rejectedPhrasesWithReasons
        };
    }

    async function onTranscript(text: string): Promise<void> {
        if (!text.trim()) return;

        // Accumulate full transcript
        fullTranscript = text;

        // Debouncing: if validation is already in progress, skip
        if (pendingValidation) return;
        pendingValidation = true;
        
        // STT is done, now starting AI processing
        ringController.startThinking();

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
            ringController.stopAll();
            pendingValidation = false;
            return;
        }

        try {
            const result = await fetchAndCachePhrases(text, cacheKey);

            // Add ONLY accepted phrases as PERMANENT bubbles
            if (result.phrases.length > 0) {
                bubbleList.addBubbles(result.phrases);
            }
        } catch (error) {
            console.error('[BrainstormMode] Validation failed:', error);
            // Fallback: add as temporary bubble (for emergencies)
            bubbleList.addTemporaryBubbles([text]);
        } finally {
            pendingValidation = false;
            ringController.completeAI();
        }
    }

    async function generateFinalText(): Promise<string> {
        const bubbles = bubbleList.getBubbles();
        
        // If we have bubbles and a transcript, try to generate a polished text
        if (bubbles.length > 0 && fullTranscript.trim()) {
            try {
                const response = await fetch('/api/brainstorm/generate-text', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({
                        transcript: fullTranscript,
                        bubbles: bubbles,
                        language: detectLocale(),
                        rejectedPhrases: bubbleList.getRejectedPhrases()
                    })
                });
                if (response.ok) {
                    const data = await response.json() as { text: string };
                    if (data?.text?.trim()) {
                        return data.text;
                    }
                }
            } catch (error) {
                console.error('[BrainstormMode] Failed to generate final text:', error);
            }
        }
        
        // Fallback: join bubbles with commas
        return bubbles.join(', ');
    }

    async function closeModal(): Promise<void> {
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
            micContainer = null;
        }
        micBtn = null;
        micHint = null;
        
        // Reset ring states
        ringController.stopAll();
        ringController.setRecordingState(false);
        feedbackController.destroy();
        
        // Generate final text from bubbles + transcript
        const text = await generateFinalText();
        
        activeBackdrop?.remove();
        activeDialog?.remove();
        activeBackdrop = null;
        activeDialog = null;
        
        // Reset state
        pendingValidation = false;
        fullTranscript = '';
        phraseCache.clear();
        
        onCloseCallback?.(text);
    }

    return {
        open(questionText: string, onClose: (text: string) => void): void {
            onCloseCallback = onClose;
            bubbleList.reset();
            
            // Reset state for new modal
            pendingValidation = false;
            fullTranscript = '';
            phraseCache.clear();
            ringController.stopAll();
            ringController.setRecordingState(false);
            feedbackController.destroy();

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
            micBtn = null;
            micHint = null;
            
            // Reset ring states
            ringController.destroy();
            feedbackController.destroy();
            
            activeBackdrop?.remove();
            activeDialog?.remove();
            activeBackdrop = null;
            activeDialog = null;
            
            // Clean up bubble list to prevent memory leaks
            bubbleList.reset();
            
            // Reset state
            pendingValidation = false;
            isRecording = false;
            phraseCache.clear();
        }
    };
}

async function fetchKeyPhrases(
    transcript: string,
    existingPhrases: string[],
    rejectedPhrases: string[]
): Promise<{phrases: string[], rejectedPhrasesWithReasons: RejectedPhrase[]}> {
    try {
        const response = await fetch('/api/brainstorm/key-phrases', {
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
        
        const convertedRejected: RejectedPhrase[] = (data.rejectedPhrasesWithReasons || []).map(r => ({
            phrase: r.phrase,
            reason: (reasonMap[r.reason] || 'None') as PhraseRejectionReason
        }));
        
        return { phrases: data.phrases ?? [], rejectedPhrasesWithReasons: convertedRejected };
    } catch {
        return { phrases: [], rejectedPhrasesWithReasons: [] };
    }
}
