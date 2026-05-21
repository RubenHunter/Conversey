/**
 * Bubble List - Manages Brainstorm Mode phrase bubbles
 * 
 * Handles creation, display, and management of phrase bubbles that appear
 * in the Brainstorm Mode modal. Supports both permanent (AI-validated) and temporary
 * bubbles, with animations for adding/removing.
 */
import { getSurveyStrings } from '../../../i18n/survey'

/**
 * Represents a bubble in the Brainstorm Mode UI.
 * Contains the phrase text and its DOM element.
 */
// Bubble interface - all bubbles are permanent (AI validated)
interface Bubble {
    text: string;
    element: HTMLElement;
}

/**
 * Controller interface for the bubble list.
 * Provides methods to add, remove, and query bubbles, as well as manage recording state.
 */
export interface BubbleListController {
    addBubbles(phrases: string[]): void;
    addTemporaryBubbles(phrases: string[]): void;
    removeBubble(index: number): void;
    getBubbles(): string[];
    getRejectedPhrases(): string[];
    reset(): void;
    setRecordingState(isRecording: boolean): void;
    readonly element: HTMLElement;
}



/**
 * Creates a bubble list controller.
 * 
 * Initializes the container element and sets up event handlers for bubble interactions.
 * 
 * @returns BubbleListController instance
 */
export function createBubbleList(): BubbleListController {
    const t = getSurveyStrings()
    const activeBubbles: Bubble[] = [];
    const rejectedPhrases = new Set<string>();
    const removingBubbles = new Set<HTMLElement>(); // Track bubbles being animated out
    const container = document.createElement('div');
    container.className = 'brainstorm-bubble-area';
    container.setAttribute('role', 'list');

    const normalize = (s: string): string => s.trim().toLowerCase();

    function scatterPosition(index: number): { left: string; top: string } {
        const slots = [
            { l: 15, t: 60 }, { l: 62, t: 50 },
            { l: 8,  t: 170 }, { l: 55, t: 155 },
            { l: 22, t: 280 }, { l: 68, t: 265 },
            { l: 12, t: 390 }, { l: 58, t: 375 },
            { l: 26, t: 500 }, { l: 72, t: 485 },
        ];
        const cycle = Math.floor(index / slots.length);
        const slot = slots[index % slots.length];
        const jitterX = (Math.random() - 0.5) * 6;
        const jitterY = (Math.random() - 0.5) * 5;
        const left = Math.max(4, Math.min(66, slot.l + jitterX));
        const top = slot.t + cycle * 580 + jitterY;
        return { left: `${left}%`, top: `${top}px` };
    }

    function updateContainerHeight(): void {
        let maxTop = 0;
        activeBubbles.forEach(b => {
            const t = parseFloat(b.element.style.top);
            if (!isNaN(t) && t > maxTop) maxTop = t;
        });
        const needed = Math.max(420, maxTop + 280);
        container.style.minHeight = `${needed}px`;
    }

    /**
     * Creates a bubble DOM element with text and close button.
     * Applies randomized phase for staggered animation and sets up event handlers.
     * 
     * @param text - The phrase text to display in the bubble
     * @param index - The bubble index for animation ordering
     * @returns The created bubble HTMLElement
     */
    function createBubbleElement(text: string, index: number): HTMLElement {
        const bubbleEl = document.createElement('div');
        const floatClasses = ['brainstorm-float-1', 'brainstorm-float-2', 'brainstorm-float-3'];
        bubbleEl.className = `brainstorm-bubble ${floatClasses[index % 3]}`;
        bubbleEl.setAttribute('role', 'listitem');

        const pos = scatterPosition(index);
        bubbleEl.style.left = pos.left;
        bubbleEl.style.top = pos.top;
        bubbleEl.style.setProperty('--bubble-index', index.toString());

        // Text element
        const label = document.createElement('span');
        label.className = 'brainstorm-bubble-text';
        label.textContent = text;

        // Close button - on the LEFT (using text "\u00d7" instead of icon)
        const closeBtn = document.createElement('button');
        closeBtn.className = 'brainstorm-bubble-close';
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
            if (target.closest('.brainstorm-bubble-close')) {
                return;
            }
            bubbleEl.classList.toggle('active');
        });

        // Handle touch end - toggle active state (consistent with click)
        bubbleEl.addEventListener('touchend', (e) => {
            const target = e.target as HTMLElement;
            if (target.closest('.brainstorm-bubble-close')) return;
            e.preventDefault(); // Prevent browser scrolling
            bubbleEl.classList.toggle('active');
        });

        return bubbleEl;
    }

    /**
     * Renders all active bubbles into the container.
     * Clears existing content and recreates bubble elements with proper animations.
     */
    function render(): void {
        // Clear everything
        container.innerHTML = '';
        
        // Add placeholder or active bubbles
        if (activeBubbles.length === 0) {
            const placeholder = document.createElement('p');
            placeholder.className = 'text-base-content/50 text-sm brainstorm-placeholder';
            placeholder.textContent = t.brainstormModeEmptyState;
            container.appendChild(placeholder);
        } else {
            activeBubbles.forEach((bubble) => {
                container.appendChild(bubble.element);
            });
        }
        // Note: removingBubbles stay in their original position until animation completes
    }

    /**
     * Adds permanent bubbles for the given phrases.
     * Normalizes phrases, removes duplicates, updates active bubbles, and re-renders.
     * 
     * @param phrases - Array of phrase texts to add as permanent bubbles
     */
    function addBubbles(phrases: string[]): void {
        let needsPlaceholderUpdate = false;
        
        // Deduplicate input phrases first
        const seen = new Set<string>();
        const uniquePhrases: string[] = [];
        for (const p of phrases) {
            const trimmed = p.trim();
            if (!trimmed) continue;
            const key = normalize(trimmed);
            if (seen.has(key)) continue;
            seen.add(key);
            uniquePhrases.push(trimmed);
        }
        
        for (const trimmed of uniquePhrases) {
            const key = normalize(trimmed);
            if (rejectedPhrases.has(key)) continue;
            if (activeBubbles.some(b => normalize(b.text) === key)) continue;
            // Also check removingBubbles to prevent re-adding during removal animation
            if (Array.from(removingBubbles).some(el => {
                const labelEl = el.querySelector('.brainstorm-bubble-text');
                return labelEl && normalize(labelEl.textContent) === key;
            })) continue;
            
            const bubbleEl = createBubbleElement(trimmed, activeBubbles.length);
            activeBubbles.push({ text: trimmed, element: bubbleEl });
            container.appendChild(bubbleEl);
            needsPlaceholderUpdate = true;
        }
        
        // Handle placeholder: remove if we just added first bubble
        if (needsPlaceholderUpdate && activeBubbles.length > 0) {
            const placeholder = container.querySelector('.brainstorm-placeholder');
            if (placeholder) placeholder.remove();
        }
        updateContainerHeight();
    }

    /**
     * Adds temporary bubbles for the given phrases.
     * These bubbles are styled differently and are used as a fallback for errors.
     * 
     * @param phrases - Array of phrase texts to add as temporary bubbles
     */
    function addTemporaryBubbles(phrases: string[]): void {
        // Fallback method - adds as permanent for now
        // In future: can be removed when AI validation is fully reliable
        addBubbles(phrases);
    }

    /**
     * Removes a bubble at the specified index with animation.
     * 
     * @param index - The index of the bubble to remove
     */
    function removeBubble(index: number): void {
        const removed = activeBubbles.splice(index, 1)[0];
        if (removed) {
            rejectedPhrases.add(normalize(removed.text));
            removingBubbles.add(removed.element);
            
            // If this was the last active bubble, show placeholder immediately
            if (activeBubbles.length === 0) {
                const placeholder = document.createElement('p');
                placeholder.className = 'text-base-content/50 text-sm brainstorm-placeholder';
                placeholder.textContent = t.brainstormModeEmptyState;
                container.appendChild(placeholder);
                container.style.minHeight = '';
            } else {
                updateContainerHeight();
            }
            
            // Trigger removal animation
            removed.element.classList.add('brainstorm-bubble-remove');
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

    /**
     * Gets the text of all active bubbles.
     * 
     * @returns Array of all bubble texts
     */
    function getBubbles(): string[] {
        return activeBubbles.map(b => b.text);
    }

    /**
     * Gets all rejected phrases as an array.
     * 
     * @returns Array of all rejected phrase texts
     */
    function getRejectedPhrases(): string[] {
        return Array.from(rejectedPhrases);
    }

    /**
     * Resets the bubble list to its initial state.
     * Clears all bubbles, rejected phrases, and re-renders.
     */
    function reset(): void {
        // Clear all removing bubbles from DOM immediately to prevent memory leaks
        removingBubbles.forEach(el => el.remove());
        removingBubbles.clear();
        
        activeBubbles.length = 0;
        rejectedPhrases.clear();
        container.style.minHeight = '';
        render();
    }

    /**
     * Sets the recording state which affects bubble styling.
     * 
     * @param recording - Whether recording is currently active
     */
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
        removeBubble, 
        getBubbles, 
        getRejectedPhrases, 
        reset, 
        setRecordingState,
        element: container 
    };
}
