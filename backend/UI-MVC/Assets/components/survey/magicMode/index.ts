/**
 * Magic Mode Index
 * Exports all Magic Mode related components and types
 */

import { createMagicModeModal } from './magicModeModal';
import type { MagicModeModalController } from './magicModeModal';
import { createBubbleList } from './bubbleList';
import type { BubbleListController } from './bubbleList';
import { createRingController } from './ringController';
import type { RingController } from './ringController';
import { createPhraseCache, generateCacheKey } from './phraseCache';
import type { PhraseCache, PhraseCacheEntry, PhraseCacheOptions } from './phraseCache';
import { createFeedbackController } from './feedbackController';
import type { FeedbackController, FeedbackControllerOptions } from './feedbackController';
import type { MagicModeWiringOptions } from './types';

export { createMagicModeModal };
export type { MagicModeModalController };
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
    MagicModeConfig,
    Bubble,
    RejectedPhrase,
    PhraseRejectionReason,
    ExtractKeyPhrasesRequest,
    ExtractKeyPhrasesResponse,
    MagicModeWiringOptions,
} from './types';

export {
    DEFAULT_MAGIC_MODE_CONFIG,
} from './types';

/**
 * Wires a button element to open the Magic Mode modal.
 *
 * Sets up a click handler that opens the modal with the question text
 * from options.getQuestionText() and invokes options.onResult() when done.
 *
 * @param button - The button element to wire up
 * @param options - Wiring options with getQuestionText and onResult callbacks
 * @returns The MagicModeModalController instance for programmatic control
 */
export function wireMagicModeButton(
    button: HTMLElement,
    options: MagicModeWiringOptions
): MagicModeModalController {
    const modal = createMagicModeModal();
    button.addEventListener('click', () => {
        modal.open(options.getQuestionText(), options.onResult);
    });
    return modal;
}
