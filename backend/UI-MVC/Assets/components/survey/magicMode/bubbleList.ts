export interface BubbleListController {
    addBubbles(phrases: string[]): void;
    removeBubble(index: number): void;
    getBubbles(): string[];
    getRejectedPhrases(): string[];
    reset(): void;
    readonly element: HTMLElement;
}

export function createBubbleList(): BubbleListController {
    const activeBubbles: string[] = [];
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
        activeBubbles.forEach((text, i) => {
            const bubble = document.createElement('div');
            bubble.className = 'badge badge-primary gap-1 cursor-default magic-mode-badge-enter';
            bubble.setAttribute('role', 'listitem');

            const label = document.createElement('span');
            label.textContent = text;

            const closeBtn = document.createElement('button');
            closeBtn.className = 'btn btn-ghost btn-xs p-0';
            closeBtn.setAttribute('aria-label', `Verwijder "${text}"`);
            closeBtn.innerHTML = '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 16 16" fill="currentColor" class="w-3 h-3"><path d="M5.28 4.22a.75.75 0 0 0-1.06 1.06L6.94 8l-2.72 2.72a.75.75 0 1 0 1.06 1.06L8 9.06l2.72 2.72a.75.75 0 1 0 1.06-1.06L9.06 8l2.72-2.72a.75.75 0 0 0-1.06-1.06L8 6.94 5.28 4.22Z"/></svg>';
            closeBtn.addEventListener('click', (e) => {
                e.stopPropagation();
                removeBubble(i);
            });

            bubble.appendChild(label);
            bubble.appendChild(closeBtn);
            container.appendChild(bubble);
        });
    }

    function addBubbles(phrases: string[]): void {
        for (const p of phrases) {
            const trimmed = p.trim();
            if (!trimmed) continue;
            const key = normalize(trimmed);
            if (rejectedPhrases.has(key)) continue;
            if (activeBubbles.some(b => normalize(b) === key)) continue;
            activeBubbles.push(trimmed);
        }
        render();
    }

    function removeBubble(index: number): void {
        const removed = activeBubbles.splice(index, 1)[0];
        if (removed) rejectedPhrases.add(normalize(removed));
        render();
    }

    function getBubbles(): string[] {
        return [...activeBubbles];
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
    return { addBubbles, removeBubble, getBubbles, getRejectedPhrases, reset, element: container };
}
