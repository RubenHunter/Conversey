/**
 * Brainstorm Mode Index
 * Exports all Brainstorm Mode related components and types
 */

import { createBrainstormModal } from './brainstormModal';
import type { BrainstormModalController } from './brainstormModal';
import { createBubbleList } from './bubbleList';
import type { BubbleListController } from './bubbleList';
import { createRingController } from './ringController';
import type { RingController } from './ringController';
import { createPhraseCache, generateCacheKey } from './phraseCache';
import type { PhraseCache, PhraseCacheEntry, PhraseCacheOptions } from './phraseCache';
import { createFeedbackController } from './feedbackController';
import type { FeedbackController, FeedbackControllerOptions } from './feedbackController';
import type { BrainstormWiringOptions } from './types';

export { createBrainstormModal };
export type { BrainstormModalController };
export { createBubbleList };
export type { BubbleListController };
export { createRingController };
export type { RingController };
export { createPhraseCache, generateCacheKey };
export type { PhraseCache, PhraseCacheEntry, PhraseCacheOptions };
export { createFeedbackController };
export type { FeedbackController, FeedbackControllerOptions };

// Re-export types from types.ts
export type {
    BrainstormConfig,
    Bubble,
    RejectedPhrase,
    PhraseRejectionReason,
    ExtractKeyPhrasesRequest,
    ExtractKeyPhrasesResponse,
    BrainstormWiringOptions,
} from './types';

export {
    DEFAULT_BRAINSTORM_CONFIG,
} from './types';

/**
 * Wires a button element to open the Brainstorm Mode modal.
 *
 * Sets up a click handler that opens the modal with the question text
 * from options.getQuestionText() and invokes options.onResult() when done.
 *
 * @param button - The button element to wire up
 * @param options - Wiring options with getQuestionText and onResult callbacks
 * @returns The BrainstormModeModalController instance for programmatic control
 */
export function wireBrainstormButton(
    button: HTMLElement,
    options: BrainstormWiringOptions
): BrainstormModalController {
    const modal = createBrainstormModal();
    button.addEventListener('click', () => {
        modal.open(options.getQuestionText(), options.onResult);
    });
    return modal;
}
