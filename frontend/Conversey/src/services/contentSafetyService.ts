export interface ContentSafetyReviewResult {
    flagged: boolean
    suggestion: string | null
}

const TOXIC_TERMS: ReadonlyArray<string> = [
    'idiot',
    'stupid',
    'dumb',
    'hate you',
    'moron',
]

function createSuggestion(input: string): string {
    let suggestion = input

    for (const term of TOXIC_TERMS) {
        const pattern = new RegExp(term.replace(/[.*+?^${}()|[\]\\]/g, '\\$&'), 'gi')
        suggestion = suggestion.replace(pattern, 'that')
    }

    return suggestion
        .replace(/\s{2,}/g, ' ')
        .trim()
}

export async function reviewContentForSafety(input: string): Promise<ContentSafetyReviewResult> {
    const normalized = input.toLowerCase()
    const containsToxicTerm = TOXIC_TERMS.some((term) => normalized.includes(term))

    if (!containsToxicTerm) {
        return { flagged: false, suggestion: null }
    }

    const suggestion = createSuggestion(input)
    return {
        flagged: true,
        suggestion: suggestion.length > 0 ? suggestion : 'I want to share this respectfully.',
    }
}

