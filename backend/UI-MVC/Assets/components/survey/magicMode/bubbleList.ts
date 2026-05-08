// Bubble interface - all bubbles are permanent (AI validated)
interface Bubble {
    text: string;
}

export interface BubbleListController {
    addBubbles(phrases: string[]): void;
    addTemporaryBubbles(phrases: string[]): void;
    removeBubble(index: number): void;
    getBubbles(): string[];
    getRejectedPhrases(): string[];
    convertTemporaryToPermanent(): void;
    reset(): void;
    readonly element: HTMLElement;
}

export function createBubbleList(): BubbleListController {
    const activeBubbles: Bubble[] = [];
    const rejectedPhrases = new Set<string>();
    const container = document.createElement('div');
    container.className = 'flex flex-wrap gap-2 p-4 min-h-24 overflow-y-auto';

    const normalize = (s: string): string => s.trim().toLowerCase();

    function render(): void {
        container.innerHTML = '';
        if (activeBubbles.length === 0) {
            const placeholder = document.createElement('p');
            placeholder.className = 'text-base-content/50 text-sm';
            placeholder.textContent = 'Spreek om sleutelwoorden te genereren...';
            container.appendChild(placeholder);
            return;
        }
        activeBubbles.forEach((bubble, i) => {
            const bubbleEl = document.createElement('div');
            
            // All bubbles are permanent (AI validated)
            bubbleEl.className = 'badge badge-primary gap-1 cursor-default magic-mode-badge-enter';
            
            bubbleEl.setAttribute('role', 'listitem');

            const label = document.createElement('span');
            label.textContent = bubble.text;

            const closeBtn = document.createElement('button');
            closeBtn.className = 'btn btn-ghost btn-xs p-0';
            closeBtn.setAttribute('aria-label', `Remove "${bubble.text}"`);
            closeBtn.innerHTML = '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 16 16" fill="currentColor" class="w-3 h-3"><path d="M5.28 4.22a.75.75 0 0 0-1.06 1.06L6.94 8l-2.72 2.72a.75.75 0 1 0 1.06 1.06L8 9.06l2.72 2.72a.75.75 0 1 0 1.06-1.06L9.06 8l2.72-2.72a.75.75 0 0 0-1.06-1.06L8 6.94 5.28 4.22Z"/></svg>';
            closeBtn.addEventListener('click', (e) => {
                e.stopPropagation();
                removeBubble(i);
            });

            bubbleEl.appendChild(label);
            bubbleEl.appendChild(closeBtn);
            container.appendChild(bubbleEl);
        });
    }

    function addBubbles(phrases: string[]): void {
        for (const p of phrases) {
            const trimmed = p.trim();
            if (!trimmed) continue;
            const key = normalize(trimmed);
            if (rejectedPhrases.has(key)) continue;
            if (activeBubbles.some(b => normalize(b.text) === key)) continue;
            activeBubbles.push({ text: trimmed });
        }
        render();
    }

    function addTemporaryBubbles(phrases: string[]): void {
        // Fallback method - adds as permanent for now
        // In future: can be removed when AI validation is fully reliable
        this.addBubbles(phrases);
    }

    function convertTemporaryToPermanent(): void {
        // No-op: all bubbles are already permanent (AI validated)
        render();
    }

    function removeBubble(index: number): void {
        const removed = activeBubbles.splice(index, 1)[0];
        if (removed) rejectedPhrases.add(normalize(removed.text));
        render();
    }

    function getBubbles(): string[] {
        return activeBubbles.map(b => b.text);
    }

    function getRejectedPhrases(): string[] {
        return [...rejectedPhrases];
    }

    function reset(): void {
        activeBubbles.length = 0;
        rejectedPhrases.clear();
        render();
    }

    render();
    return { 
        addBubbles, 
        addTemporaryBubbles, 
        convertTemporaryToPermanent,
        removeBubble, 
        getBubbles, 
        getRejectedPhrases, 
        reset, 
        element: container 
    };
}
