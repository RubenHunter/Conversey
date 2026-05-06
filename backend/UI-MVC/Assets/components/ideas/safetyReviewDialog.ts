import { reviewContentForSafety } from '../../services/contentSafetyService'

export interface PostSafetyDecision {
    proceed: boolean
    text: string
    offensiveContentDetected: boolean
    /** true when the user chose to post the original (flagged) text */
    useOriginal?: boolean
    /** true when the user edited the text in the dialog before posting */
    edited?: boolean
}

interface CreateSafetyReviewDialogParams {
    root: ParentNode
}

export interface SafetyReviewDialogController {
    reviewBeforePost: (input: string) => Promise<PostSafetyDecision>
    reviewWithSuggestion: (original: string, suggestion: string) => Promise<PostSafetyDecision>
}

export function createSafetyReviewDialogController({ root }: CreateSafetyReviewDialogParams): SafetyReviewDialogController {
    const backdrop = root.querySelector<HTMLDivElement>('#safety-review-backdrop')!
    const dialog = root.querySelector<HTMLDivElement>('#safety-review-dialog')!
    const originalInput = root.querySelector<HTMLTextAreaElement>('#safety-review-original')!
    const suggestionInput = root.querySelector<HTMLTextAreaElement>('#safety-review-suggestion')!
    const editOriginalBtn = root.querySelector<HTMLButtonElement>('#safety-review-edit-original')!
    const editSuggestionBtn = root.querySelector<HTMLButtonElement>('#safety-review-edit-suggestion')!
    const acceptSuggestionBtn = root.querySelector<HTMLButtonElement>('#safety-review-accept-suggestion')!
    const postAnywayBtn = root.querySelector<HTMLButtonElement>('#safety-review-post-anyway')!

    let activeResolver: ((decision: PostSafetyDecision) => void) | null = null
    let baselineOriginal = ''
    let baselineSuggestion = ''

    function closeDialog(): void {
        dialog.classList.remove('open')
        backdrop.classList.remove('open')
        dialog.hidden = true
        backdrop.hidden = true
        activeResolver = null
    }

    function resolve(decision: PostSafetyDecision): void {
        const resolver = activeResolver
        closeDialog()
        resolver?.(decision)
    }

    backdrop.addEventListener('click', () => {
        resolve({ proceed: false, text: '', offensiveContentDetected: false })
    })

    editOriginalBtn.addEventListener('click', () => {
        originalInput.readOnly = false
        originalInput.focus()
        originalInput.setSelectionRange(originalInput.value.length, originalInput.value.length)
    })

    editSuggestionBtn.addEventListener('click', () => {
        suggestionInput.readOnly = false
        suggestionInput.focus()
        suggestionInput.setSelectionRange(suggestionInput.value.length, suggestionInput.value.length)
    })

    acceptSuggestionBtn.addEventListener('click', () => {
        const text = suggestionInput.value.trim()
        if (text.length === 0) return
        resolve({
            proceed: true,
            text,
            offensiveContentDetected: false,
            useOriginal: false,
            edited: text !== baselineSuggestion,
        })
    })

    postAnywayBtn.addEventListener('click', () => {
        const original = originalInput.value.trim()
        if (original.length === 0) return
        resolve({
            proceed: true,
            text: original,
            offensiveContentDetected: true,
            useOriginal: true,
            edited: original !== baselineOriginal,
        })
    })

    function openDialog(originalText: string, suggestionText: string): Promise<PostSafetyDecision> {
        return new Promise<PostSafetyDecision>((resolveDecision) => {
            activeResolver = resolveDecision
            baselineOriginal = originalText.trim()
            baselineSuggestion = suggestionText.trim()
            originalInput.value = originalText
            originalInput.readOnly = true
            suggestionInput.value = suggestionText
            suggestionInput.readOnly = true

            dialog.hidden = false
            backdrop.hidden = false
            requestAnimationFrame(() => {
                dialog.classList.add('open')
                backdrop.classList.add('open')
            })
        })
    }

    async function reviewBeforePost(input: string): Promise<PostSafetyDecision> {
        const result = await reviewContentForSafety(input)
        if (!result.flagged || !result.suggestion) {
            return { proceed: true, text: input, offensiveContentDetected: false }
        }
        return openDialog(input, result.suggestion)
    }

    async function reviewWithSuggestion(original: string, suggestion: string): Promise<PostSafetyDecision> {
        return openDialog(original, suggestion)
    }

    return {
        reviewBeforePost,
        reviewWithSuggestion,
    }
}
