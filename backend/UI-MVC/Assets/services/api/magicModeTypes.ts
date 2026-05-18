/**
 * Magic Mode API Types
 * 
 * Shared TypeScript type definitions for Magic Mode API DTOs.
 * These types mirror the C# DTOs in Domain/DTOs/MagicMode/ to eliminate duplication.
 * 
 * Source of truth: C# backend DTOs in Domain/DTOs/MagicMode/
 * - ExtractKeyPhrasesRequest.cs
 * - ExtractKeyPhrasesResponse.cs
 * 
 * This file should be kept in sync with the backend DTOs.
 */

// ============================================================================
// API DTO Types
// ============================================================================

/**
 * Reasons for phrase rejection.
 * Matches the C# enum PhraseRejectionReason in ExtractKeyPhrasesResponse.cs
 * 
 * Numeric values correspond to the C# enum order:
 * 0 = None
 * 1 = WordCountTooLow
 * 2 = WordCountExceeded
 * 3 = DuplicateExact
 * 4 = DuplicateSemantic
 * 5 = SubsetOfExisting
 * 6 = FillerContent
 * 7 = TooGeneric
 */
export type PhraseRejectionReason =
    | 'None'
    | 'WordCountTooLow'
    | 'WordCountExceeded'
    | 'DuplicateExact'
    | 'DuplicateSemantic'
    | 'SubsetOfExisting'
    | 'FillerContent'
    | 'TooGeneric';

/**
 * A rejected phrase with reason.
 * Matches the C# record RejectedPhrase in ExtractKeyPhrasesResponse.cs
 */
export interface RejectedPhrase {
    phrase: string;
    reason: PhraseRejectionReason;
    similarTo?: string | null;
}

/**
 * API request for extracting key phrases.
 * Matches the C# record ExtractKeyPhrasesRequest in ExtractKeyPhrasesRequest.cs
 */
export interface ExtractKeyPhrasesRequest {
    transcript: string;
    language: string;
    maxPhrases: number;
    existingPhrases: string[];
    rejectedPhrases: string[];
}

/**
 * API response for extracting key phrases.
 * Matches the C# record ExtractKeyPhrasesResponse in ExtractKeyPhrasesResponse.cs
 */
export interface ExtractKeyPhrasesResponse {
    phrases: string[];
    rejectedPhrasesWithReasons: RejectedPhrase[];
}
