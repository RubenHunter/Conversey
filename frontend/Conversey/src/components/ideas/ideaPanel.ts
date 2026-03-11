import type { Idea } from '../../models/idea.ts'
import type { IdeaComment, IdeaPanelController, ReviewBeforePost } from './types.ts'

interface CreateIdeaPanelControllerParams {
    root: ParentNode
    reviewBeforePost: ReviewBeforePost
}

export function createIdeaPanelController({ root, reviewBeforePost }: CreateIdeaPanelControllerParams): IdeaPanelController {
    const panelBackdrop = root.querySelector<HTMLDivElement>('#idea-panel-backdrop')!
    const panel = root.querySelector<HTMLDivElement>('#idea-panel')!
    const panelClose = root.querySelector<HTMLButtonElement>('#idea-panel-close')!
    const panelPinned = root.querySelector<HTMLDivElement>('#idea-panel-pinned')!
    const panelBadges = root.querySelector<HTMLDivElement>('#idea-panel-badges')!
    const panelText = root.querySelector<HTMLParagraphElement>('#idea-panel-text')!
    const panelComments = root.querySelector<HTMLDivElement>('#idea-panel-comments')!
    const panelInput = root.querySelector<HTMLTextAreaElement>('#idea-panel-input')!
    const panelSend = root.querySelector<HTMLButtonElement>('#idea-panel-send')!

    const commentStore = new Map<number, IdeaComment[]>()
    let currentIdea: Idea | null = null

    function renderComments(idea: Idea): void {
        const comments = commentStore.get(idea.id) ?? []
        panelComments.innerHTML = ''

        if (comments.length === 0) {
            panelComments.innerHTML = `<p class="idea-panel-no-comments">No comments yet. Be the first!</p>`
            return
        }

        const isOwnIdea = idea.authorType === 'self'
        const pinnedComments = isOwnIdea ? comments.filter((comment) => comment.author === 'self') : []
        const regularComments = isOwnIdea ? comments.filter((comment) => comment.author !== 'self') : comments

        if (pinnedComments.length > 0) {
            const pinnedLabel = document.createElement('p')
            pinnedLabel.className = 'idea-panel-comments-label'
            pinnedLabel.textContent = 'Pinned author comments'
            panelComments.appendChild(pinnedLabel)

            pinnedComments.forEach((comment) => {
                const el = document.createElement('div')
                el.className = 'idea-panel-comment idea-panel-comment--self idea-panel-comment--pinned'
                el.textContent = comment.text
                panelComments.appendChild(el)
            })
        }

        regularComments.forEach((comment) => {
            const el = document.createElement('div')
            el.className = `idea-panel-comment${comment.author === 'self' ? ' idea-panel-comment--self' : ''}`
            el.textContent = comment.text

            if (comment.offensiveContentDetected) {
                const flagged = document.createElement('span')
                flagged.className = 'ideas-review-flag'
                flagged.textContent = 'Marked for review'
                el.prepend(flagged)
            }

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

        // Keep note contextual to the post while pinning author's own comments in the comments list.
        if (idea.authorType === 'self') {
            panelPinned.hidden = false
            panelPinned.innerHTML = `
                <span class="idea-panel-pinned-label">📌 Author's note</span>
                <p class="idea-panel-pinned-text">This is your original idea. Your comments appear pinned below.</p>
            `
        } else {
            panelPinned.hidden = true
            panelPinned.innerHTML = ''
        }

        renderComments(idea)
        panelInput.value = ''
        panelSend.disabled = true
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
            },
            { once: true },
        )
    }

    function open(idea: Idea): void {
        currentIdea = idea
        renderPanel(idea)
        panel.hidden = false
        panelBackdrop.hidden = false

        requestAnimationFrame(() => {
            panel.classList.add('open')
            panelBackdrop.classList.add('open')
        })

        panelInput.focus()
    }

    panelClose.addEventListener('click', close)
    panelBackdrop.addEventListener('click', close)

    panelInput.addEventListener('input', () => {
        panelSend.disabled = panelInput.value.trim().length === 0
    })

    panelSend.addEventListener('click', async () => {
        const text = panelInput.value.trim()
        if (!currentIdea || text.length === 0) return

        const decision = await reviewBeforePost(text)
        if (!decision.proceed) {
            panelInput.value = text
            panelInput.focus()
            panelSend.disabled = panelInput.value.trim().length === 0
            return
        }

        const existing = commentStore.get(currentIdea.id) ?? []
        commentStore.set(currentIdea.id, [
            ...existing,
            { author: 'self', text: decision.text, offensiveContentDetected: decision.offensiveContentDetected },
        ])
        renderPanel(currentIdea)
    })

    return {
        open,
        close,
    }
}
