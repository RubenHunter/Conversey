import { getSTTManager } from '../../../services/speechService';
import { detectLocale } from '../../../i18n/survey';
import { createBubbleList } from './bubbleList';

export interface MagicModeModalController {
    open(questionText: string, onClose: (text: string) => void): void;
    destroy(): void;
}

export function createMagicModeModal(): MagicModeModalController {
    const stt = getSTTManager();
    let isRecording = false;
    let onCloseCallback: ((text: string) => void) | null = null;
    const bubbleList = createBubbleList();
    
    // Stabiliteitscheck state
    let lastTranscript: string = '';
    let stabilityCount: number = 0;
    let isFinalized: boolean = false;
    const STABILITY_THRESHOLD = 2; // 2x dezelfde transcriptie = stabiel

    // Optimizatie state
    let isProcessing = false;
    const phraseCache = new Map<string, string[]>();
    const MAX_CACHE_SIZE = 100;

    function buildDOM(): { backdrop: HTMLElement; dialog: HTMLElement } {
        const backdrop = document.createElement('div');
        backdrop.className = 'magic-mode-backdrop';

        const dialog = document.createElement('div');
        dialog.className = 'magic-mode-dialog';
        dialog.setAttribute('role', 'dialog');
        dialog.setAttribute('aria-modal', 'true');
        dialog.setAttribute('aria-label', 'Magic Mode');

        // Header
        const header = document.createElement('div');
        header.className = 'magic-mode-dialog-header';

        const title = document.createElement('h3');
        title.className = 'font-bold text-lg';
        title.textContent = 'Magic Mode';

        const closeBtn = document.createElement('button');
        closeBtn.className = 'btn btn-ghost btn-sm btn-circle';
        closeBtn.setAttribute('aria-label', 'Sluiten');
        closeBtn.innerHTML = '<svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" class="w-5 h-5"><path stroke-linecap="round" stroke-linejoin="round" d="M6 18 18 6M6 6l12 12" /></svg>';
        closeBtn.addEventListener('click', closeModal);

        header.appendChild(title);
        header.appendChild(closeBtn);

        // Body
        const body = document.createElement('div');
        body.className = 'magic-mode-dialog-body';

        const questionEl = document.createElement('p');
        questionEl.className = 'text-base-content/70 text-sm';

        const bubbleArea = document.createElement('div');
        bubbleArea.className = 'magic-mode-bubble-area';
        bubbleArea.setAttribute('role', 'list');
        bubbleArea.appendChild(bubbleList.element);

        body.appendChild(questionEl);
        body.appendChild(bubbleArea);

        // Footer
        const footer = document.createElement('div');
        footer.className = 'magic-mode-dialog-footer';

        const micBtn = document.createElement('button');
        micBtn.className = 'btn btn-primary btn-circle btn-lg';
        micBtn.setAttribute('aria-label', 'Start opname');
        micBtn.setAttribute('aria-pressed', 'false');
        micBtn.innerHTML = '<svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" class="w-7 h-7"><path stroke-linecap="round" stroke-linejoin="round" d="M12 18.75a6 6 0 0 0 6-6v-1.5m-6 7.5a6 6 0 0 1-6-6v-1.5m6 7.5v3.75m-3.75 0h7.5M12 15.75a3 3 0 0 1-3-3V4.5a3 3 0 1 1 6 0v8.25a3 3 0 0 1-3 3Z" /></svg>';

        const micHint = document.createElement('p');
        micHint.className = 'text-xs text-base-content/50';
        micHint.textContent = 'Klik om te spreken';

        micBtn.addEventListener('click', () => {
            isRecording = !isRecording;
            if (isRecording) {
                stt.start(null, detectLocale(), onTranscript);
                micHint.textContent = 'Klik om te stoppen';
            } else {
                stt.stop();
                micHint.textContent = 'Klik om te spreken';
            }
            micBtn.classList.toggle('btn-error', isRecording);
            micBtn.classList.toggle('animate-pulse', isRecording);
            micBtn.setAttribute('aria-pressed', String(isRecording));
            micBtn.setAttribute('aria-label', isRecording ? 'Stop opname' : 'Start opname');
        });

        footer.appendChild(micBtn);
        footer.appendChild(micHint);

        dialog.appendChild(header);
        dialog.appendChild(body);
        dialog.appendChild(footer);

        backdrop.addEventListener('click', closeModal);

        return { backdrop, dialog };
    }

    let activeBackdrop: HTMLElement | null = null;
    let activeDialog: HTMLElement | null = null;

    function getQuestionEl(): HTMLElement | null {
        return activeDialog?.querySelector('p.text-base-content\\/70') ?? null;
    }

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
        if (phraseCache.size > MAX_CACHE_SIZE) {
            const keys = Array.from(phraseCache.keys());
            for (let i = 0; i < keys.length - (MAX_CACHE_SIZE / 2); i++) {
                phraseCache.delete(keys[i]);
            }
        }
    }

    async function fetchAndCachePhrases(text: string, cacheKey: string): Promise<{phrases: string[], rejectedPhrases: Array<{phrase: string, reason: string, similarTo?: string}>}> {
        const result = await fetchKeyPhrases(text, bubbleList.getBubbles(), bubbleList.getRejectedPhrases());
        
        if (result.phrases.length > 0) {
            phraseCache.set(cacheKey, result.phrases);
            cleanupCache();
        }
        
        return result;
    }

    async function onTranscript(text: string): Promise<void> {
        if (!text.trim()) return;

        // Stabiliteitscheck: track hoevaak dezelfde transcriptie achter elkaar komt
        // Dit doe je VOORDAT je beslist of je de transcriptie verwerkt
        if (text === lastTranscript) {
            stabilityCount++;
        } else {
            // Nieuwe transcriptie: reset counter
            stabilityCount = 1;  // 1 omdat dit de eerste keer is dat we deze text zien
            lastTranscript = text;
            isFinalized = false;
        }

        // Skip als er al een Mistral call bezig is
        // We willen alleen de LAATSTE transcriptie verwerken als processing klaar is
        if (isProcessing) return;

        // Genereer cache key
        const cacheKey = generateCacheKey(
            text,
            detectLocale(),
            2,
            bubbleList.getBubbles(),
            bubbleList.getRejectedPhrases()
        );

        // Check cache
        if (phraseCache.has(cacheKey)) {
            const cachedPhrases = phraseCache.get(cacheKey)!;
            if (cachedPhrases.length > 0) {
                bubbleList.addTemporaryBubbles(cachedPhrases);
            }

            // Stabiliteitscheck: als transcriptie 2x achter elkaar hetzelfde is, maak bubbels final
            if (stabilityCount >= STABILITY_THRESHOLD && !isFinalized) {
                bubbleList.convertTemporaryToPermanent();
                isFinalized = true;
            }
            return;
        }

        // Markeer als processing
        isProcessing = true;

        try {
            const result = await fetchAndCachePhrases(text, cacheKey);

            // Voeg toe als tijdelijk
            if (result.phrases.length > 0) {
                bubbleList.addTemporaryBubbles(result.phrases);
            }

            // Stabiliteitscheck: als transcriptie 2x achter elkaar hetzelfde is, maak bubbels final
            if (stabilityCount >= STABILITY_THRESHOLD && !isFinalized) {
                bubbleList.convertTemporaryToPermanent();
                isFinalized = true;
            }
        } finally {
            isProcessing = false;
        }
    }

    function closeModal(): void {
        if (isRecording) {
            stt.stop();
            isRecording = false;
        }
        const text = bubbleList.getBubbles().join(', ');
        activeBackdrop?.remove();
        activeDialog?.remove();
        activeBackdrop = null;
        activeDialog = null;
        
        // Reset state
        lastTranscript = '';
        stabilityCount = 0;
        isFinalized = false;
        isProcessing = false;
        phraseCache.clear();
        
        onCloseCallback?.(text);
    }

    return {
        open(questionText: string, onClose: (text: string) => void): void {
            onCloseCallback = onClose;
            bubbleList.reset();
            
            // Reset state bij nieuwe modal
            lastTranscript = '';
            stabilityCount = 0;
            isFinalized = false;
            isProcessing = false;
            phraseCache.clear();

            const { backdrop, dialog } = buildDOM();
            activeBackdrop = backdrop;
            activeDialog = dialog;

            const questionEl = dialog.querySelector('p.text-base-content\\/70') ?? dialog.querySelector('p');
            if (questionEl) questionEl.textContent = questionText;

            document.body.appendChild(backdrop);
            document.body.appendChild(dialog);
        },
        destroy(): void {
            if (isRecording) stt.stop();
            activeBackdrop?.remove();
            activeDialog?.remove();
            activeBackdrop = null;
            activeDialog = null;
            
            // Reset state
            lastTranscript = '';
            stabilityCount = 0;
            isFinalized = false;
            isProcessing = false;
            phraseCache.clear();
        }
    };
}

async function fetchKeyPhrases(
    transcript: string,
    existingPhrases: string[],
    rejectedPhrases: string[]
): Promise<{phrases: string[], rejectedPhrases: Array<{phrase: string, reason: string, similarTo?: string}>}> {
    try {
        const response = await fetch('/api/magic-mode/key-phrases', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                transcript,
                language: detectLocale(),
                maxPhrases: 2,
                existingPhrases,
                rejectedPhrases
            })
        });
        if (!response.ok) return { phrases: [], rejectedPhrases: [] };
        const data = await response.json() as {
            phrases: string[],
            rejectedPhrasesWithReasons?: Array<{phrase: string, reason: number, similarTo?: string}>
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
            reason: reasonMap[r.reason] || 'Unknown',
            similarTo: r.similarTo
        }));
        
        return { phrases: data.phrases ?? [], rejectedPhrases: convertedRejected };
    } catch {
        return { phrases: [], rejectedPhrases: [] };
    }
}
