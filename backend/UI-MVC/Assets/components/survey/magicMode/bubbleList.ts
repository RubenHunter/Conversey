import { getSurveyStrings } from '../../../i18n/survey'

// Bubble interface - all bubbles are permanent (AI validated)
interface Bubble {
    text: string;
    element: HTMLElement;
}

export interface BubbleListController {
    addBubbles(phrases: string[]): void;
    addTemporaryBubbles(phrases: string[]): void;
    removeBubble(index: number): void;
    getBubbles(): string[];
    getRejectedPhrases(): string[];
    convertTemporaryToPermanent(): void;
    reset(): void;
    setRecordingState(isRecording: boolean): void;
    readonly element: HTMLElement;
}



export function createBubbleList(): BubbleListController {
    const t = getSurveyStrings()
    const activeBubbles: Bubble[] = [];
    const rejectedPhrases = new Set<string>();
    const removingBubbles = new Set<HTMLElement>(); // Track bubbles being animated out
    const container = document.createElement('div');
    container.className = 'magic-mode-bubble-area';
    container.setAttribute('role', 'list');

    const normalize = (s: string): string => s.trim().toLowerCase();

    function createBubbleElement(text: string, index: number): HTMLElement {
        const bubbleEl = document.createElement('div');
        bubbleEl.className = 'magic-mode-bubble magic-mode-badge-enter';
        bubbleEl.setAttribute('role', 'listitem');
        
        // Random phase (0-100%) for natural staggered animation start
        const randomPhase = Math.random();
        bubbleEl.style.setProperty('--bubble-phase', (randomPhase * 100).toString());
        
        bubbleEl.style.setProperty('--bubble-index', index.toString());

        // Text element
        const label = document.createElement('span');
        label.className = 'magic-mode-bubble-text';
        label.textContent = text;

        // Close button - on the LEFT (using text "\u00d7" instead of icon)
        const closeBtn = document.createElement('button');
        closeBtn.className = 'magic-mode-bubble-close';
        closeBtn.setAttribute('aria-label', `${t.remove} "${text}"`);
        closeBtn.textContent = '\u00d7';

        bubbleEl.appendChild(label);
        bubbleEl.appendChild(closeBtn);

        // Handle close button click
        closeBtn.addEventListener('click', (e) => {
            e.stopPropagation();
            const idx = activeBubbles.findIndex(b => b.element === bubbleEl);
            if (idx !== -1) {
                removeBubble(idx);
            }
        });

        // Handle close button touch (mobile) - remove passive for preventDefault
        closeBtn.addEventListener('touchend', (e) => {
            e.preventDefault();
            e.stopPropagation();
            const idx = activeBubbles.findIndex(b => b.element === bubbleEl);
            if (idx !== -1) {
                removeBubble(idx);
            }
        });

        // Handle bubble click - show close button on click (universal for desktop & mobile)
        bubbleEl.addEventListener('click', (e) => {
            const target = e.target as HTMLElement;
            if (target.closest('.magic-mode-bubble-close')) {
                return;
            }
            bubbleEl.classList.toggle('active');
        });

        // Handle touch end - toggle active state (consistent with click)
        bubbleEl.addEventListener('touchend', (e) => {
            const target = e.target as HTMLElement;
            if (target.closest('.magic-mode-bubble-close')) return;
            e.preventDefault(); // Prevent browser scrolling
            bubbleEl.classList.toggle('active');
        });

        return bubbleEl;
    }

    function render(): void {
        // Clear everything
        container.innerHTML = '';
        
        // Add placeholder or active bubbles
        if (activeBubbles.length === 0) {
            const placeholder = document.createElement('p');
            placeholder.className = 'text-base-content/50 text-sm magic-mode-placeholder';
            placeholder.textContent = t.magicModeEmptyState;
            container.appendChild(placeholder);
        } else {
            activeBubbles.forEach((bubble) => {
                container.appendChild(bubble.element);
            });
        }
        // Note: removingBubbles stay in their original position until animation completes
    }

    function addBubbles(phrases: string[]): void {
        let needsPlaceholderUpdate = false;
        
        for (const p of phrases) {
            const trimmed = p.trim();
            if (!trimmed) continue;
            const key = normalize(trimmed);
            if (rejectedPhrases.has(key)) continue;
            if (activeBubbles.some(b => normalize(b.text) === key)) continue;
            // Also check removingBubbles to prevent re-adding during removal animation
            if (Array.from(removingBubbles).some(el => el.textContent.trim().toLowerCase() === key)) continue;
            
            const bubbleEl = createBubbleElement(trimmed, activeBubbles.length);
            activeBubbles.push({ text: trimmed, element: bubbleEl });
            container.appendChild(bubbleEl);
            needsPlaceholderUpdate = true;
        }
        
        // Handle placeholder: remove if we just added first bubble
        if (needsPlaceholderUpdate && activeBubbles.length > 0) {
            const placeholder = container.querySelector('.magic-mode-placeholder');
            if (placeholder) placeholder.remove();
        }
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
        if (removed) {
            rejectedPhrases.add(normalize(removed.text));
            removingBubbles.add(removed.element);
            
            // If this was the last active bubble, show placeholder immediately
            if (activeBubbles.length === 0) {
                const placeholder = document.createElement('p');
                placeholder.className = 'text-base-content/50 text-sm magic-mode-placeholder';
                placeholder.textContent = t.magicModeEmptyState;
                container.appendChild(placeholder);
            }
            
            // Trigger removal animation
            removed.element.classList.add('magic-mode-bubble-remove');
            removed.element.addEventListener('animationend', () => {
                removingBubbles.delete(removed.element);
                removed.element.remove();
                // No render() needed - element is gone from DOM
            }, { once: true });
            // DON'T call render() immediately - let animation complete
        } else {
            render();
        }
    }

    function getBubbles(): string[] {
        return activeBubbles.map(b => b.text);
    }

    function getRejectedPhrases(): string[] {
        return Array.from(rejectedPhrases);
    }

    function reset(): void {
        // Clear all removing bubbles from DOM immediately to prevent memory leaks
        removingBubbles.forEach(el => el.remove());
        removingBubbles.clear();
        
        activeBubbles.length = 0;
        rejectedPhrases.clear();
        render();
    }

    function setRecordingState(recording: boolean): void {
        if (recording) {
            container.classList.add('recording');
        } else {
            container.classList.remove('recording');
        }
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
        setRecordingState,
        element: container 
    };
}
