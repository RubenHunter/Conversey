/**
 * Brainstorm Mode Types
 * Type definitions specific to Brainstorm Mode feature
 * 
 * Contains interfaces and types for:
 * - Modal controllers (BrainstormModalController, BrainstormWiringOptions)
 * - Bubble management (BubbleListController, Bubble)
 * - Configuration (BrainstormConfig)
 * - AI phrase extraction types are imported from shared API types
 */

// Import shared API types from central location to avoid duplication
// These types mirror the C# DTOs in Domain/DTOs/BrainstormMode/
export type {
    PhraseRejectionReason,
    RejectedPhrase,
    ExtractKeyPhrasesRequest,
    ExtractKeyPhrasesResponse,
} from '../../../services/api/brainstormTypes';

// ============================================================================
// Modal Types
// ============================================================================

/**
 * Controller interface for Brainstorm Mode modal.
 * Provides methods to open the modal with question text and handle the result.
 */
export interface BrainstormModalController {
    open(questionText: string, onClose: (text: string) => void): void;
    destroy(): void;
}

/**
 * Options for wiring Brainstorm Mode button.
 * Provides callbacks for getting the question text and handling the result.
 */
export interface BrainstormWiringOptions {
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
 * Configuration for Brainstorm Mode feature.
 * Defines limits and timing for phrase extraction and caching.
 */
export interface BrainstormConfig {
    maxPhrases: number;
    cacheMaxSize: number;
    feedbackDurationMs: number;
    stabilityThreshold: number;
}

/**
 * Default Brainstorm Mode configuration.
 * Used as fallback when no custom configuration is provided.
 */
export const DEFAULT_BRAINSTORM_CONFIG: BrainstormConfig = {
    maxPhrases: 2,
    cacheMaxSize: 100,
    feedbackDurationMs: 3000,
    stabilityThreshold: 2,
} as const;
