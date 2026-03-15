import type { Idea, IdeaReactionSummary } from '../../models/idea.ts'
import type { IdeaResponse, ResponseReactionSummary } from '../../models/ideaResponse.ts'
import type { IdeaPanelController, ReviewBeforePost } from './types.ts'

interface CreateIdeaPanelControllerParams {
    root: ParentNode
    reviewBeforePost: ReviewBeforePost
    loadResponses: (idea: Idea) => Promise<IdeaResponse[]>
    submitResponse: (idea: Idea, text: string, offensiveContentDetected: boolean) => Promise<IdeaResponse>
    reactToResponse: (idea: Idea, responseId: number, emoji: string) => Promise<ResponseReactionSummary[]>
    unreactToResponse: (idea: Idea, responseId: number, emoji: string) => Promise<ResponseReactionSummary[]>
    reactToIdea: (idea: Idea, emoji: string) => Promise<IdeaReactionSummary[]>
    unreactToIdea: (idea: Idea, emoji: string) => Promise<IdeaReactionSummary[]>
    onIdeaReactionsUpdated: (ideaId: number, reactions: IdeaReactionSummary[]) => void
}

type PickerTarget = { kind: 'idea' } | { kind: 'response'; responseId: number }

const REACTION_PICK_OPTIONS = ['❤️', '👍', '💡', '🔥', '😂', '👏']

export function createIdeaPanelController({
    root,
    reviewBeforePost,
    loadResponses,
    submitResponse,
    reactToResponse,
    unreactToResponse,
    reactToIdea,
    unreactToIdea,
    onIdeaReactionsUpdated,
}: CreateIdeaPanelControllerParams): IdeaPanelController {
    const panelBackdrop = root.querySelector<HTMLDivElement>('#idea-panel-backdrop')!
    const panel = root.querySelector<HTMLDivElement>('#idea-panel')!
    const panelClose = root.querySelector<HTMLButtonElement>('#idea-panel-close')!
    const panelPinned = root.querySelector<HTMLDivElement>('#idea-panel-pinned')!
    const panelBadges = root.querySelector<HTMLDivElement>('#idea-panel-badges')!
    const panelText = root.querySelector<HTMLParagraphElement>('#idea-panel-text')!
    const panelPost = root.querySelector<HTMLDivElement>('#idea-panel-post')!
    const panelIdeaEmoji = root.querySelector<HTMLButtonElement>('#idea-panel-emoji')!
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
    let pickerTarget: PickerTarget | null = null

    const ideaReactionRow = document.createElement('div')
    ideaReactionRow.className = 'idea-panel-reactions-row'
    const ideaReactionSummary = document.createElement('div')
    ideaReactionSummary.className = 'idea-panel-reactions-summary'
    const ideaReactionPicker = document.createElement('div')
    ideaReactionPicker.className = 'idea-panel-reaction-picker'
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
        ideaReactionPicker.innerHTML = ''

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
            ideaReactionPicker.replaceChildren(
                createReactionPicker({ kind: 'idea' }, (emoji) => {
                    pickerTarget = null
                    void handleIdeaReaction(idea, emoji)
                }),
            )
        }

        ideaReactionRow.append(ideaReactionSummary, panelIdeaEmoji, ideaReactionPicker)
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
            panelComments.innerHTML = `<p class="idea-panel-no-comments">Loading responses...</p>`
            return
        }

        if (failedIdeas.has(idea.id) && comments.length === 0) {
            panelComments.innerHTML = `<p class="idea-panel-no-comments">Could not load responses right now. Try reopening this idea.</p>`
            return
        }

        if (comments.length === 0) {
            panelComments.innerHTML = `<p class="idea-panel-no-comments">No responses yet. Be the first!</p>`
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
        panelText.textContent = idea.body

        if (idea.authorType === 'self') {
            panelPinned.hidden = false
            panelPinned.innerHTML = `
                <span class="idea-panel-pinned-label">📌 Author's note</span>
                <p class="idea-panel-pinned-text">This is your original idea. Your responses appear pinned below.</p>
            `
        } else {
            panelPinned.hidden = true
            panelPinned.innerHTML = ''
        }

        renderIdeaReactionRow(idea)
        renderComments(idea)
        panelInput.value = ''
        panelSend.disabled = true
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

    function open(idea: Idea): void {
        currentIdea = idea
        pickerTarget = null
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

    panelInput.addEventListener('input', () => {
        panelSend.disabled = panelInput.value.trim().length === 0 || isSubmittingResponse
    })

    panelSend.addEventListener('click', async () => {
        const text = panelInput.value.trim()
        if (!currentIdea || text.length === 0 || isSubmittingResponse) return

        const decision = await reviewBeforePost(text)
        if (!decision.proceed) {
            panelInput.value = text
            panelInput.focus()
            panelSend.disabled = panelInput.value.trim().length === 0 || isSubmittingResponse
            return
        }

        isSubmittingResponse = true
        panelSend.disabled = true
        panelSend.textContent = 'Posting...'

        try {
            const response = await submitResponse(currentIdea, decision.text, decision.offensiveContentDetected)
            const existing = getResponses(currentIdea.id)
            responseStore.set(currentIdea.id, [...existing, response])
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
    }
}
