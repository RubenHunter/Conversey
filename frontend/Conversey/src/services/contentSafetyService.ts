export interface ContentSafetyReviewResult {
    flagged: boolean
    suggestion: string | null
}

// Real moderation is done by the backend (Mistral AI).
// This module is kept as a thin pass-through so safetyReviewDialog
// can still call reviewContentForSafety — it always returns not-flagged
// because the backend POST is the authoritative check.
export async function reviewContentForSafety(_input: string): Promise<ContentSafetyReviewResult> {
    return { flagged: false, suggestion: null }
}

