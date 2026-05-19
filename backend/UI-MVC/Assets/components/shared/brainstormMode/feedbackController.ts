/**
 * Brainstorm Mode Feedback Controller
 * Manages display of feedback for rejected phrases.
 *
 * Provides visual feedback when the AI rejects phrases, showing the reason
 * for rejection with localized messages. Feedback auto-dismisses after
 * a configurable duration.
 */
import { getSurveyStrings } from '../../../i18n/survey';
import type { PhraseRejectionReason } from './types';

/**
 * Manages display of feedback for rejected phrases.
 * Handles DOM element creation, grouping by reason, and auto-dismissal.
 */
export interface FeedbackController {
    /**
     * Show feedback for rejected phrases.
     * Groups phrases by rejection reason and displays localized messages.
     * Auto-dismisses after the configured duration.
     * 
     * @param rejected - Array of rejected phrases with their reasons
     */
    showRejected(rejected: Array<{ phrase: string; reason: PhraseRejectionReason }>): void;
    
    /**
     * Clean up any active feedback elements.
     * Removes the feedback element from the DOM if it exists.
     */
    destroy(): void;
}

/**
 * Options for creating a feedback controller.
 */
export interface FeedbackControllerOptions {
    /** Duration in milliseconds to show feedback before auto-dismissal. Default: 3000 */
    durationMs?: number;
}

/**
 * Creates a new feedback controller.
 * 
 * @param options - Configuration options for the feedback controller
 * @returns A new FeedbackController instance
 */
export function createFeedbackController(options: FeedbackControllerOptions = {}): FeedbackController {
    const { durationMs = 3000 } = options;
    const t = getSurveyStrings();
    let activeTimeout: ReturnType<typeof setTimeout> | null = null;
    let feedbackElement: HTMLElement | null = null;
    
    /**
     * Maps rejection reason codes to localized strings.
     * 
     * @param reason - The rejection reason code
     * @returns Localized string for the rejection reason
     */
    function getReasonText(reason: PhraseRejectionReason): string {
        const reasonMap: Record<PhraseRejectionReason, string> = {
            'None': t.rejectionNone,
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
    
    return {
        showRejected(rejected: Array<{ phrase: string; reason: PhraseRejectionReason }>): void {
            // Clear any existing timeout
            if (activeTimeout) {
                clearTimeout(activeTimeout);
                activeTimeout = null;
            }
            
            // Remove any existing feedback element
            if (feedbackElement) {
                feedbackElement.remove();
                feedbackElement = null;
            }
            
            if (rejected.length === 0) return;
            
            const element = document.createElement('div');
            element.className = 'brainstorm-feedback';
            
            // Group by reason and show specific phrases
            const reasonGroups: Record<string, string[]> = {};
            rejected.forEach(r => {
                if (!reasonGroups[r.reason]) reasonGroups[r.reason] = [];
                reasonGroups[r.reason].push(r.phrase);
            });

            const reasonText = Object.entries(reasonGroups)
                .map(([reason, phrases]) => {
                    const reasonLabel = getReasonText(reason as PhraseRejectionReason);
                    const phraseList = phrases.map(p => `"${p}"`).join(', ');
                    return phrases.length === 1
                        ? `"${phrases[0]}" (${reasonLabel})`
                        : `${phrases.length}x ${reasonLabel}: ${phraseList}`;
                })
                .join('; ');

            element.textContent = `${t.rejectedFeedbackPrefix}${reasonText}`;
            document.body.appendChild(element);
            
            // Store reference for cleanup
            feedbackElement = element;
            
            // Auto-dismiss after duration
            activeTimeout = setTimeout(() => {
                element.remove();
                feedbackElement = null;
                activeTimeout = null;
            }, durationMs);
        },
        
        destroy(): void {
            if (activeTimeout) {
                clearTimeout(activeTimeout);
                activeTimeout = null;
            }
            if (feedbackElement) {
                feedbackElement.remove();
                feedbackElement = null;
            }
        }
    };
}
