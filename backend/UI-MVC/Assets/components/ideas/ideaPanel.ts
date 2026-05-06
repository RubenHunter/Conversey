import type { Idea, IdeaReactionSummary } from '../../models/idea'
import type { IdeaResponse, ResponseReactionSummary } from '../../models/ideaResponse'
import type { IdeaPanelController, ReviewBeforePost } from './types'
import type { PostSafetyDecision } from './safetyReviewDialog'
import { getSurveyStrings } from '../../i18n/survey'

interface ResponseSubmitResult {
    response: IdeaResponse
    aiSuggestion: string | null
    requiresSafetyReview: boolean
}

interface CreateIdeaPanelControllerParams {
    root: ParentNode
    reviewBeforePost: ReviewBeforePost
    reviewWithSuggestion: (original: string, suggestion: string) => Promise<PostSafetyDecision>
    updateIdeaAfterSafetyReview: (idea: Idea, text: string, markForReview: boolean) => Promise<Idea>
    loadResponses: (idea: Idea) => Promise<IdeaResponse[]>
    submitResponse: (idea: Idea, text: string) => Promise<ResponseSubmitResult>
    updateResponseAfterSafetyReview: (
        idea: Idea,
        responseId: number,
        text: string,
        markForReview: boolean,
    ) => Promise<IdeaResponse>
    reactToResponse: (idea: Idea, responseId: number, emoji: string) => Promise<ResponseReactionSummary[]>
    unreactToResponse: (idea: Idea, responseId: number, emoji: string) => Promise<ResponseReactionSummary[]>
    reactToIdea: (idea: Idea, emoji: string) => Promise<IdeaReactionSummary[]>
    unreactToIdea: (idea: Idea, emoji: string) => Promise<IdeaReactionSummary[]>
    onCopyIdea: (idea: Idea) => void
    onIdeaReactionsUpdated: (ideaId: number, reactions: IdeaReactionSummary[]) => void
}

type PickerTarget = { kind: 'idea' } | { kind: 'response'; responseId: number }

const REACTION_PICK_OPTIONS = ['❤️', '👍', '💡', '🔥', '😂', '👏']

export function createIdeaPanelController({
    root,
    reviewBeforePost,
    reviewWithSuggestion,
    updateIdeaAfterSafetyReview,
    loadResponses,
    submitResponse,
    updateResponseAfterSafetyReview,
    reactToResponse,
    unreactToResponse,
    reactToIdea,
    unreactToIdea,
    onCopyIdea,
    onIdeaReactionsUpdated,
}: CreateIdeaPanelControllerParams): IdeaPanelController {
    const t = getSurveyStrings()
    const panelBackdrop = root.querySelector<HTMLDivElement>('#idea-panel-backdrop')!
    const panel = root.querySelector<HTMLDivElement>('#idea-panel')!
    const panelClose = root.querySelector<HTMLButtonElement>('#idea-panel-close')!
    const panelBadges = root.querySelector<HTMLDivElement>('#idea-panel-badges')!
    const panelEditToggle = root.querySelector<HTMLButtonElement>('#idea-panel-edit-toggle')!
    const panelCopyButton = root.querySelector<HTMLButtonElement>('#idea-panel-copy')!
    const panelText = root.querySelector<HTMLParagraphElement>('#idea-panel-text')!
    const panelPost = root.querySelector<HTMLDivElement>('#idea-panel-post')!
    const panelIdeaEmoji = root.querySelector<HTMLButtonElement>('#idea-panel-emoji')!
    const panelEditRegion = root.querySelector<HTMLDivElement>('#idea-panel-edit-region')!
    const panelEditInput = root.querySelector<HTMLTextAreaElement>('#idea-panel-edit-input')!
    const panelEditCancel = root.querySelector<HTMLButtonElement>('#idea-panel-edit-cancel')!
    const panelEditSave = root.querySelector<HTMLButtonElement>('#idea-panel-edit-save')!
    const panelComments = root.querySelector<HTMLDivElement>('#idea-panel-comments')!
    const panelInput = root.querySelector<HTMLTextAreaElement>('#idea-panel-input')!
    const panelSend = root.querySelector<HTMLButtonElement>('#idea-panel-send')!

    const responseStore = new Map<number, IdeaResponse[]>()
    const ideaReactionStore = new Map<number, IdeaReactionSummary[]>()
    const selectedIdeaReactions = new Map<number, Set<string>>()
    const selectedResponseReactions = new Map<number, Set<string>>()
    const animatedReactionKeys = new Set<string>()
    const loadingIdeas = new Set<number>()
    const failedIdeas = new Set<number>()
    const pendingReactions = new Set<string>()
    let currentIdea: Idea | null = null
    let isSubmittingResponse = false
    let isEditingIdea = false
    let editBaseline = ''
    let pickerTarget: PickerTarget | null = null

    const ideaReactionRow = document.createElement('div')
    ideaReactionRow.className = 'idea-panel-reactions-row'
    const ideaReactionSummary = document.createElement('div')
    ideaReactionSummary.className = 'idea-panel-reactions-summary'
    panelPost.append(ideaReactionRow)

    function getResponses(ideaId: number): IdeaResponse[] {
        return responseStore.get(ideaId) ?? []
    }

    function getIdeaReactions(idea: Idea): IdeaReactionSummary[] {
        return ideaReactionStore.get(idea.id) ?? idea.reactions
    }

    function getSelectedIdeaReactions(ideaId: number): Set<string> {
        return selectedIdeaReactions.get(ideaId) ?? new Set<string>()
    }

    function getSelectedResponseReactions(responseId: number): Set<string> {
        return selectedResponseReactions.get(responseId) ?? new Set<string>()
    }

    function markIdeaReactionSelected(ideaId: number, emoji: string): void {
        const next = new Set(getSelectedIdeaReactions(ideaId))
        next.add(emoji)
        selectedIdeaReactions.set(ideaId, next)
    }

    function unmarkIdeaReactionSelected(ideaId: number, emoji: string): void {
        const next = new Set(getSelectedIdeaReactions(ideaId))
        next.delete(emoji)
        selectedIdeaReactions.set(ideaId, next)
    }

    function markResponseReactionSelected(responseId: number, emoji: string): void {
        const next = new Set(getSelectedResponseReactions(responseId))
        next.add(emoji)
        selectedResponseReactions.set(responseId, next)
    }

    function unmarkResponseReactionSelected(responseId: number, emoji: string): void {
        const next = new Set(getSelectedResponseReactions(responseId))
        next.delete(emoji)
        selectedResponseReactions.set(responseId, next)
    }

    function mapReactionCounts(reactions: ReadonlyArray<IdeaReactionSummary | ResponseReactionSummary>): Map<string, number> {
        return new Map(reactions.map((reaction) => [reaction.emoji, reaction.count]))
    }

    function pulseReactionKey(reactionKey: string): void {
        animatedReactionKeys.add(reactionKey)
        window.setTimeout(() => {
            animatedReactionKeys.delete(reactionKey)
            if (currentIdea) {
                renderIdeaReactionRow(currentIdea)
                renderComments(currentIdea)
            }
        }, 420)
    }

    function updateIdeaReactions(idea: Idea, reactions: IdeaReactionSummary[]): void {
        const previousCounts = mapReactionCounts(getIdeaReactions(idea))
        const normalized = reactions.filter((reaction) => reaction.emoji.trim().length > 0)
        ideaReactionStore.set(idea.id, normalized)
        onIdeaReactionsUpdated(idea.id, normalized)

        normalized.forEach((reaction) => {
            if ((previousCounts.get(reaction.emoji) ?? 0) !== reaction.count) {
                pulseReactionKey(`idea:${idea.id}:${reaction.emoji}`)
            }
        })
    }

    function isEditableIdea(idea: Idea | null): boolean {
        return Boolean(idea && idea.authorType === 'self' && idea.pendingReview)
    }

    function syncEditedIdeaCard(idea: Idea): void {
        const card = root.querySelector<HTMLElement>(`.ideas-card[data-idea-id="${idea.id}"]`)
        if (!card) return

        const body = card.querySelector<HTMLElement>('.ideas-card-body')
        if (body) {
            body.textContent = idea.body
        }

        card.setAttribute(
            'aria-label',
            `Idea: ${idea.body.substring(0, 50)}${idea.body.length > 50 ? '...' : ''}`,
        )
    }

    function updateEditSaveState(): void {
        const trimmed = panelEditInput.value.trim()
        panelEditSave.disabled = !currentIdea || trimmed.length === 0 || trimmed === editBaseline
    }

    function enterEditMode(): void {
        if (!currentIdea || !isEditableIdea(currentIdea)) return

        isEditingIdea = true
        editBaseline = currentIdea.body.trim()
        panelEditInput.value = currentIdea.body
        panelText.hidden = true
        panelEditRegion.hidden = false
        panelEditToggle.hidden = true
        updateEditSaveState()
        panelEditInput.focus()
        panelEditInput.setSelectionRange(panelEditInput.value.length, panelEditInput.value.length)
    }

    function exitEditMode(): void {
        isEditingIdea = false
        panelEditRegion.hidden = true
        panelText.hidden = false
        panelEditToggle.hidden = !isEditableIdea(currentIdea)
        updateEditSaveState()
    }

    function updateResponseReactions(ideaId: number, responseId: number, reactions: ResponseReactionSummary[]): void {
        const existing = getResponses(ideaId)
        const response = existing.find((item) => item.id === responseId)
        const previousCounts = mapReactionCounts(response?.reactions ?? [])
        responseStore.set(
            ideaId,
            existing.map((response) => (response.id === responseId ? { ...response, reactions } : response)),
        )

        reactions.forEach((reaction) => {
            if ((previousCounts.get(reaction.emoji) ?? 0) !== reaction.count) {
                pulseReactionKey(`response:${responseId}:${reaction.emoji}`)
            }
        })
    }

    function createReactionPicker(target: PickerTarget, onPick: (emoji: string) => void): HTMLDivElement {
        const picker = document.createElement('div')
        picker.className = 'idea-panel-reaction-picker'

        const selected = target.kind === 'idea'
            ? getSelectedIdeaReactions(currentIdea!.id)
            : getSelectedResponseReactions(target.responseId)

        const availableEmojis = REACTION_PICK_OPTIONS.filter((emoji) => !selected.has(emoji))

        availableEmojis.forEach((emoji, index) => {
            const button = document.createElement('button')
            button.type = 'button'
            button.className = 'idea-panel-reaction-picker-btn'
            button.textContent = emoji
            button.title = `React with ${emoji}`
            button.style.setProperty('--reaction-index', String(index))
            const key = target.kind === 'idea' ? `idea:${currentIdea!.id}:${emoji}` : `response:${target.responseId}:${emoji}`
            button.disabled = pendingReactions.has(key)
            button.addEventListener('click', (event) => {
                event.stopPropagation()
                onPick(emoji)
            })
            picker.appendChild(button)
        })

        if (availableEmojis.length === 0) {
            const doneLabel = document.createElement('span')
            doneLabel.className = 'idea-panel-reaction-picker-empty'
            doneLabel.textContent = 'You reacted with all options'
            picker.appendChild(doneLabel)
        }

        return picker
    }

    function renderIdeaReactionRow(idea: Idea): void {
        const reactions = getIdeaReactions(idea)
        ideaReactionRow.innerHTML = ''
        ideaReactionSummary.innerHTML = ''

        reactions.forEach((reaction) => {
            const summaryChip = document.createElement('button')
            summaryChip.className = 'idea-panel-reaction-chip'
            summaryChip.type = 'button'
            summaryChip.textContent = `${reaction.emoji} ${reaction.count}`
            summaryChip.title = getSelectedIdeaReactions(idea.id).has(reaction.emoji) ? 'Remove your reaction' : 'Add +1'
            summaryChip.disabled = pendingReactions.has(`idea:${idea.id}:${reaction.emoji}`)
            summaryChip.classList.toggle('is-selected-reaction', getSelectedIdeaReactions(idea.id).has(reaction.emoji))
            summaryChip.classList.toggle('reaction-count-pop', animatedReactionKeys.has(`idea:${idea.id}:${reaction.emoji}`))
            summaryChip.addEventListener('click', (event) => {
                event.stopPropagation()
                void handleIdeaReaction(idea, reaction.emoji)
            })
            ideaReactionSummary.appendChild(summaryChip)
        })

        panelIdeaEmoji.disabled = loadingIdeas.has(idea.id)
        panelIdeaEmoji.classList.toggle('open', pickerTarget?.kind === 'idea')

        if (pickerTarget?.kind === 'idea') {
            ideaReactionRow.append(
                ideaReactionSummary,
                panelIdeaEmoji,
                createReactionPicker({ kind: 'idea' }, (emoji) => {
                    pickerTarget = null
                    void handleIdeaReaction(idea, emoji)
                }),
            )
            return
        }

        ideaReactionRow.append(ideaReactionSummary, panelIdeaEmoji)
    }

    function createResponseReactionRow(idea: Idea, response: IdeaResponse): HTMLElement {
        const row = document.createElement('div')
        row.className = 'idea-panel-reactions-row'

        const reactionSummary = document.createElement('div')
        reactionSummary.className = 'idea-panel-reactions-summary'

        response.reactions.forEach((reaction) => {
            const summaryChip = document.createElement('button')
            summaryChip.className = 'idea-panel-reaction-chip'
            summaryChip.type = 'button'
            summaryChip.textContent = `${reaction.emoji} ${reaction.count}`
            summaryChip.title = getSelectedResponseReactions(response.id).has(reaction.emoji) ? 'Remove your reaction' : 'Add +1'
            summaryChip.disabled = pendingReactions.has(`response:${response.id}:${reaction.emoji}`)
            summaryChip.classList.toggle('is-selected-reaction', getSelectedResponseReactions(response.id).has(reaction.emoji))
            summaryChip.classList.toggle('reaction-count-pop', animatedReactionKeys.has(`response:${response.id}:${reaction.emoji}`))
            summaryChip.addEventListener('click', (event) => {
                event.stopPropagation()
                void handleResponseReaction(idea, response.id, reaction.emoji)
            })
            reactionSummary.appendChild(summaryChip)
        })

        const addButton = document.createElement('button')
        addButton.className = 'idea-panel-reaction-add-btn'
        addButton.type = 'button'
        addButton.textContent = '+ :)'
        addButton.title = 'Add reaction'
        addButton.classList.toggle('open', pickerTarget?.kind === 'response' && pickerTarget.responseId === response.id)
        addButton.addEventListener('click', (event) => {
            event.stopPropagation()
            const sameTarget = pickerTarget?.kind === 'response' && pickerTarget.responseId === response.id
            pickerTarget = sameTarget ? null : { kind: 'response', responseId: response.id }
            if (currentIdea?.id === idea.id) {
                renderComments(idea)
                renderIdeaReactionRow(idea)
            }
        })

        row.append(reactionSummary, addButton)

        if (pickerTarget?.kind === 'response' && pickerTarget.responseId === response.id) {
            row.appendChild(
                createReactionPicker({ kind: 'response', responseId: response.id }, (emoji) => {
                    pickerTarget = null
                    void handleResponseReaction(idea, response.id, emoji)
                }),
            )
        }

        return row
    }

    function renderComments(idea: Idea): void {
        const comments = getResponses(idea.id)
        panelComments.innerHTML = ''

        if (loadingIdeas.has(idea.id)) {
            panelComments.innerHTML = `<p class="idea-panel-no-comments">${t.loadingResponses}</p>`
            return
        }

        if (failedIdeas.has(idea.id) && comments.length === 0) {
            panelComments.innerHTML = `<p class="idea-panel-no-comments">${t.couldNotLoadResponses}</p>`
            return
        }

        if (comments.length === 0) {
            panelComments.innerHTML = `<p class="idea-panel-no-comments">${t.noResponsesYet}</p>`
            return
        }

        const isOwnIdea = idea.authorType === 'self'
        const pinnedComments = isOwnIdea ? comments.filter((comment) => comment.author === 'self') : []
        const regularComments = isOwnIdea ? comments.filter((comment) => comment.author !== 'self') : comments

        if (pinnedComments.length > 0) {
            const pinnedLabel = document.createElement('p')
            pinnedLabel.className = 'idea-panel-comments-label'
            pinnedLabel.textContent = 'Pinned author responses'
            panelComments.appendChild(pinnedLabel)

            pinnedComments.forEach((comment) => {
                const el = document.createElement('div')
                el.className = 'idea-panel-comment idea-panel-comment--self idea-panel-comment--pinned'

                if (comment.offensiveContentDetected) {
                    const flagged = document.createElement('span')
                    flagged.className = 'ideas-review-flag'
                    flagged.textContent = 'Marked for review'
                    el.appendChild(flagged)
                }

                const copy = document.createElement('p')
                copy.className = 'idea-panel-comment-text'
                copy.textContent = comment.text
                el.append(copy, createResponseReactionRow(idea, comment))
                panelComments.appendChild(el)
            })
        }

        regularComments.forEach((comment) => {
            const el = document.createElement('div')
            el.className = `idea-panel-comment${comment.author === 'self' ? ' idea-panel-comment--self' : ''}`

            const copy = document.createElement('p')
            copy.className = 'idea-panel-comment-text'
            copy.textContent = comment.text

            if (comment.offensiveContentDetected) {
                const flagged = document.createElement('span')
                flagged.className = 'ideas-review-flag'
                flagged.textContent = 'Marked for review'
                el.appendChild(flagged)
            }

            el.append(copy, createResponseReactionRow(idea, comment))
            panelComments.appendChild(el)
        })
    }

    function renderPanel(idea: Idea): void {
        panelBadges.innerHTML = ''
        if (idea.authorType === 'self') {
            const badge = document.createElement('span')
            badge.className = 'ideas-card-yours-badge'
            badge.textContent = 'Your idea'
            panelBadges.appendChild(badge)
        }
        if (idea.pendingReview) {
            const reviewBadge = document.createElement('span')
            reviewBadge.className = 'ideas-review-flag'
            reviewBadge.textContent = 'Marked for review'
            panelBadges.appendChild(reviewBadge)
        }

        panelEditToggle.hidden = !isEditableIdea(idea)
        panelCopyButton.hidden = idea.authorType === 'self'
        if (!isEditingIdea || !isEditableIdea(idea)) {
            panelText.textContent = idea.body
            panelEditInput.value = idea.body
            editBaseline = idea.body.trim()
        }
        panelText.hidden = isEditingIdea && isEditableIdea(idea)
        panelEditRegion.hidden = !isEditingIdea || !isEditableIdea(idea)

        renderIdeaReactionRow(idea)
        renderComments(idea)
        if (!isEditingIdea) {
            panelInput.value = ''
        }
        panelSend.disabled = true
        updateEditSaveState()
    }

    async function refreshResponses(idea: Idea): Promise<void> {
        loadingIdeas.add(idea.id)
        failedIdeas.delete(idea.id)
        renderPanel(idea)

        try {
            const responses = await loadResponses(idea)
            responseStore.set(idea.id, responses)
        } catch {
            failedIdeas.add(idea.id)
        } finally {
            loadingIdeas.delete(idea.id)
            if (currentIdea?.id === idea.id) {
                renderPanel(idea)
            }
        }
    }

    async function handleIdeaReaction(idea: Idea, emoji: string): Promise<void> {
        const key = `idea:${idea.id}:${emoji}`
        if (pendingReactions.has(key)) return

        pendingReactions.add(key)
        renderIdeaReactionRow(idea)

        try {
            const isSelected = getSelectedIdeaReactions(idea.id).has(emoji)
            const updated = isSelected ? await unreactToIdea(idea, emoji) : await reactToIdea(idea, emoji)
            if (isSelected) {
                unmarkIdeaReactionSelected(idea.id, emoji)
            } else {
                markIdeaReactionSelected(idea.id, emoji)
            }
            updateIdeaReactions(idea, updated)
        } finally {
            pendingReactions.delete(key)
            if (currentIdea?.id === idea.id) {
                renderIdeaReactionRow(idea)
            }
        }
    }

    async function handleResponseReaction(idea: Idea, responseId: number, emoji: string): Promise<void> {
        const key = `response:${responseId}:${emoji}`
        if (pendingReactions.has(key)) return

        pendingReactions.add(key)
        renderComments(idea)

        try {
            const isSelected = getSelectedResponseReactions(responseId).has(emoji)
            const updated = isSelected ? await unreactToResponse(idea, responseId, emoji) : await reactToResponse(idea, responseId, emoji)
            if (isSelected) {
                unmarkResponseReactionSelected(responseId, emoji)
            } else {
                markResponseReactionSelected(responseId, emoji)
            }
            updateResponseReactions(idea.id, responseId, updated)
        } finally {
            pendingReactions.delete(key)
            if (currentIdea?.id === idea.id) {
                renderComments(idea)
            }
        }
    }

    function close(): void {
        panel.classList.remove('open')
        panelBackdrop.classList.remove('open')
        panel.addEventListener(
            'transitionend',
            () => {
                panel.hidden = true
                panelBackdrop.hidden = true
                currentIdea = null
                pickerTarget = null
            },
            { once: true },
        )
    }

    function isOpen(): boolean {
        return !panel.hidden && panel.classList.contains('open')
    }

    function open(idea: Idea): void {
        currentIdea = idea
        pickerTarget = null
        isEditingIdea = false
        if (!ideaReactionStore.has(idea.id)) {
            ideaReactionStore.set(idea.id, idea.reactions)
        }

        renderPanel(idea)
        panel.hidden = false
        panelBackdrop.hidden = false

        requestAnimationFrame(() => {
            panel.classList.add('open')
            panelBackdrop.classList.add('open')
        })

        panelInput.focus()
        void refreshResponses(idea)
    }

    panelClose.addEventListener('click', close)
    panelBackdrop.addEventListener('click', close)

    panel.addEventListener('click', () => {
        if (!currentIdea || pickerTarget === null) return
        pickerTarget = null
        renderIdeaReactionRow(currentIdea)
        renderComments(currentIdea)
    })

    panelIdeaEmoji.addEventListener('click', (event) => {
        event.stopPropagation()
        if (!currentIdea) return

        pickerTarget = pickerTarget?.kind === 'idea' ? null : { kind: 'idea' }
        renderIdeaReactionRow(currentIdea)
    })

    panelEditToggle.addEventListener('click', (event) => {
        event.stopPropagation()
        enterEditMode()
    })

    panelCopyButton.addEventListener('click', (event) => {
        event.stopPropagation()
        if (!currentIdea || currentIdea.authorType === 'self') return
        const ideaToCopy = currentIdea
        close()
        window.setTimeout(() => {
            onCopyIdea(ideaToCopy)
        }, 220)
    })

    panelEditCancel.addEventListener('click', () => {
        if (!currentIdea) return
        exitEditMode()
        renderPanel(currentIdea)
    })

    panelEditInput.addEventListener('input', updateEditSaveState)

    panelEditSave.addEventListener('click', async () => {
        if (!currentIdea || !isEditableIdea(currentIdea) || !isEditingIdea) return

        const nextBody = panelEditInput.value.trim()
        if (nextBody.length === 0 || nextBody === editBaseline) return

        panelEditSave.disabled = true
        panelEditSave.textContent = 'Saving...'

        try {
            const updatedIdea = await updateIdeaAfterSafetyReview(currentIdea, nextBody, true)
            Object.assign(currentIdea, updatedIdea, { pendingReview: true })
            syncEditedIdeaCard(currentIdea)
            exitEditMode()
            renderPanel(currentIdea)
        } finally {
            panelEditSave.textContent = t.saveChanges
            updateEditSaveState()
        }
    })

    panelInput.addEventListener('input', () => {
        panelSend.disabled = panelInput.value.trim().length === 0 || isSubmittingResponse
    })

    panelSend.addEventListener('click', async () => {
        const text = panelInput.value.trim()
        if (!currentIdea || text.length === 0 || isSubmittingResponse) return

        // Keep existing local pre-check behavior if enabled in future.
        const localDecision = await reviewBeforePost(text)
        if (!localDecision.proceed) {
            panelInput.value = text
            panelInput.focus()
            panelSend.disabled = panelInput.value.trim().length === 0 || isSubmittingResponse
            return
        }

        isSubmittingResponse = true
        panelSend.disabled = true
        panelSend.textContent = 'Posting...'

        try {
            const submitResult = await submitResponse(currentIdea, localDecision.text)
            let finalResponse = submitResult.response

            if (submitResult.requiresSafetyReview) {
                const suggestion = submitResult.aiSuggestion ?? 'Please rephrase your message in a respectful and constructive way.'
                const decision = await reviewWithSuggestion(localDecision.text, suggestion)

                if (!decision.proceed) {
                    panelInput.value = text
                    panelInput.focus()
                    return
                }

                if (decision.useOriginal) {
                    if (decision.edited) {
                        finalResponse = await updateResponseAfterSafetyReview(currentIdea, finalResponse.id, decision.text, true)
                    }
                    finalResponse = {
                        ...finalResponse,
                        text: decision.text,
                        offensiveContentDetected: true,
                    }
                } else if (decision.edited) {
                    finalResponse = await updateResponseAfterSafetyReview(currentIdea, finalResponse.id, decision.text, true)
                    finalResponse = {
                        ...finalResponse,
                        offensiveContentDetected: true,
                    }
                } else {
                    finalResponse = await updateResponseAfterSafetyReview(currentIdea, finalResponse.id, decision.text, false)
                }
            }

            const existing = getResponses(currentIdea.id)
            responseStore.set(currentIdea.id, [...existing, finalResponse])
            renderPanel(currentIdea)
        } finally {
            isSubmittingResponse = false
            panelSend.textContent = 'Post'
            panelSend.disabled = panelInput.value.trim().length === 0
        }
    })

    return {
        open,
        close,
        isOpen,
    }
}
