/**
 * Magic Mode Types
 * Type definitions specific to Magic Mode feature
 * 
 * Contains interfaces and types for:
 * - Modal controllers (MagicModeModalController, MagicModeWiringOptions)
 * - Bubble management (BubbleListController, Bubble)
 * - Configuration (MagicModeConfig)
 * - AI phrase extraction types are imported from shared API types
 */

// Import shared API types from central location to avoid duplication
// These types mirror the C# DTOs in Domain/DTOs/MagicMode/
export type {
    PhraseRejectionReason,
    RejectedPhrase,
    ExtractKeyPhrasesRequest,
    ExtractKeyPhrasesResponse,
} from '../../../services/api/magicModeTypes';

// ============================================================================
// Modal Types
// ============================================================================

/**
 * Controller interface for Magic Mode modal.
 * Provides methods to open the modal with question text and handle the result.
 */
export interface MagicModeModalController {
    open(questionText: string, onClose: (text: string) => void): void;
    destroy(): void;
}

/**
 * Options for wiring Magic Mode button.
 * Provides callbacks for getting the question text and handling the result.
 */
export interface MagicModeWiringOptions {
    getQuestionText: () => string;
    onResult: (finalText: string) => void;
}

/**
 * Controller for bubble list.
 * Manages adding, removing, and querying bubbles, as well as recording state.
 * Note: This is a duplicate of the interface in bubbleList.ts for convenience.
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

// ============================================================================
// Bubble Types
// ============================================================================

/**
 * Internal bubble representation.
 * Contains the phrase text and its DOM element.
 * Note: This is a duplicate of the interface in bubbleList.ts for convenience.
 */
export interface Bubble {
    text: string;
    element: HTMLElement;
}

// ============================================================================
// Configuration
// ============================================================================

/**
 * Configuration for Magic Mode feature.
 * Defines limits and timing for phrase extraction and caching.
 */
export interface MagicModeConfig {
    maxPhrases: number;
    cacheMaxSize: number;
    feedbackDurationMs: number;
    stabilityThreshold: number;
}

/**
 * Default Magic Mode configuration.
 * Used as fallback when no custom configuration is provided.
 */
export const DEFAULT_MAGIC_MODE_CONFIG: MagicModeConfig = {
    maxPhrases: 2,
    cacheMaxSize: 100,
    feedbackDurationMs: 3000,
    stabilityThreshold: 2,
} as const;
