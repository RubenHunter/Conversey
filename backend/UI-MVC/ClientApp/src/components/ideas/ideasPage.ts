import '../../styles/pages/ideas.css'
import type { RouteParams } from '../../utils/router.ts'
import { getProject } from '../../services/projectService.ts'
import { getIdeasContext, getIdeasYouthToken, updateIdeaAfterSafetyReview } from '../../services/ideaService.ts'
import {
    addIdeaReaction,
    addIdeaResponse,
    addResponseReaction,
    getIdeaResponses,
    removeIdeaReaction,
    removeResponseReaction,
    updateIdeaResponseAfterSafetyReview,
} from '../../services/ideaResponseService.ts'
import type { Idea, IdeaTopic } from '../../models/idea.ts'
import { resolveInitialIdeasView } from './initialView.ts'
import { createIdeaPanelController } from './ideaPanel.ts'
import { createSafetyReviewDialogController } from './safetyReviewDialog.ts'
import { renderIdeasComposer } from './composer.ts'
import type { ActiveView } from './types.ts'
import { renderIdeasHeader } from './ideasHeader.ts'
import { createTopicModalController } from './topicModal.ts'
import { createIdeasListController } from './ideasListController.ts'
import { createIdeasSubmitHandler } from './ideasSubmitHandler.ts'

// Get label for active ideas view
function getActiveIdeasLabel(activeView: ActiveView, topics: IdeaTopic[]): string {
    if (activeView.type === 'my-ideas') return 'My ideas'
    const topic = topics.find((item) => item.id === activeView.topicId)
    return topic ? topic.title : 'Select a topic'
}


export async function renderIdeasPage(container: HTMLElement, params: RouteParams): Promise<void> {
    const project = await getProject(params.organizationSlug, params.projectSlug)
    const context = await getIdeasContext(params.organizationSlug, params.projectSlug, project)
    const youthToken = getIdeasYouthToken(project.slug)

    const organizationName = project.organizationName?.trim() || project.organizationSlug
    const topics = context.topics
    const allIdeas = [...context.ideas]

    let activeView: ActiveView = resolveInitialIdeasView(topics, allIdeas)
    const flaggedIdeaIds = new Set<number>()

    const headerHTML = renderIdeasHeader({ organizationName, organizationSlug: project.organizationSlug })

    container.innerHTML = `
        <div class="ideas-shell">
            ${headerHTML}

            <div class="ideas-body">
                <div class="ideas-grid">
                    <section class="ideas-community" aria-label="Ideas list">
                        <div id="ideas-list" class="ideas-list" aria-live="polite"></div>
                    </section>

                    <section class="ideas-compose" aria-label="Create idea">
                        <div class="ideas-compose-head">
                            <button id="ideas-topic-trigger" class="ideas-compose-topic-button" aria-haspopup="dialog" aria-expanded="false" aria-controls="topic-modal" aria-label="Select topic">
                                <span class="ideas-compose-topic-text">
                                    <span class="ideas-compose-topic-kicker">Topic:</span>
                                    <span id="ideas-topic-trigger-value" class="ideas-compose-topic-value"></span>
                                    <span class="ideas-compose-topic-chevron" aria-hidden="true">▾</span>
                                </span>
                            </button>
                            <div class="survey-question-title ideas-prompt-title-row">
                                <span id="ideas-prompt" class="ideas-prompt"></span>
                            </div>
                        </div>
                        <div class="survey-textarea-wrapper">
                            <textarea id="ideas-textarea" class="survey-textarea" placeholder="Share your idea for this topic..."></textarea>
                            <div class="survey-textarea-actions">
                                <button id="ideas-magic" class="survey-magic-btn" type="button" title="Answer in Magic Mode (coming soon)">
                                    <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24" aria-hidden="true">
                                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 3v4M3 5h4M6 17v4m-2-2h4m5-16l2.286 6.857L21 12l-5.714 2.143L13 21l-2.286-6.857L5 12l5.714-2.143L13 3z"/>
                                    </svg>
                                    <span class="survey-magic-btn-text">Magic Mode</span>
                                </button>
                                <button id="ideas-speak" class="survey-mic-btn" type="button" aria-label="Voice input" title="Voice input (coming soon)">
                                    <svg class="survey-speaker-icon" fill="currentColor" viewBox="0 0 24 24" aria-hidden="true">
                                        <path d="M12 14c1.66 0 2.99-1.34 2.99-3L15 5c0-1.66-1.34-3-3-3S9 3.34 9 5v6c0 1.66 1.34 3 3 3zm5.3-3c0 3-2.54 5.1-5.3 5.1S6.7 14 6.7 11H5c0 3.41 2.72 6.23 6 6.72V21h2v-3.28c3.28-.48 6-3.3 6-6.72h-1.7z"/>
                                    </svg>
                                </button>
                            </div>
                        </div>
                        <button id="ideas-submit" class="ideas-submit" type="button">Submit Idea</button>
                    </section>
                </div>
                <button
                    id="ideas-topic-trigger-floating"
                    class="ideas-compose-topic-button ideas-compose-topic-button--floating"
                    aria-haspopup="dialog"
                    aria-expanded="false"
                    aria-controls="topic-modal"
                    aria-label="Switch topic"
                    hidden
                >
                    <span class="ideas-compose-topic-text">
                        <span class="ideas-compose-topic-kicker">Switch to topic</span>
                        <span id="ideas-topic-trigger-floating-value" class="ideas-compose-topic-value"></span>
                        <span class="ideas-compose-topic-chevron" aria-hidden="true">▾</span>
                    </span>
                </button>
            </div>
        </div>

        <!-- Topic Selection Modal -->
        <div id="topic-modal-backdrop" class="modal-backdrop" hidden aria-hidden="true"></div>
        <div id="topic-modal" class="modal" role="dialog" aria-modal="true" aria-labelledby="topic-modal-title" hidden>
            <div class="modal-header">
                <h3 id="topic-modal-title">Select a Topic</h3>
                <button id="topic-modal-close" class="modal-close" aria-label="Close">&times;</button>
            </div>
            <div class="modal-body">
                <div id="topic-modal-list" class="modal-list"></div>
            </div>
        </div>

        <div id="idea-panel-backdrop" class="idea-panel-backdrop" hidden aria-hidden="true"></div>
        <div id="idea-panel" class="idea-panel" role="dialog" aria-modal="true" aria-label="Idea detail" hidden>
            <div class="idea-panel-header">
                <h3 class="idea-panel-title">Idea</h3>
                <button id="idea-panel-close" class="idea-panel-close" aria-label="Close">&times;</button>
            </div>
            <div class="idea-panel-body">
                <div id="idea-panel-pinned" class="idea-panel-pinned" hidden></div>
                <div class="idea-panel-section idea-panel-section--idea">
                    <p class="idea-panel-section-label">Original idea</p>
                    <div id="idea-panel-post" class="idea-panel-post">
                        <div id="idea-panel-badges" class="idea-panel-badges"></div>
                        <p id="idea-panel-text" class="idea-panel-text"></p>
                        <div id="idea-panel-edit-region" hidden>
                            <textarea id="idea-panel-edit-input" class="idea-panel-input idea-panel-edit-input" rows="4" placeholder="Edit your idea..."></textarea>
                            <div class="idea-panel-edit-actions">
                                <button id="idea-panel-edit-cancel" class="idea-panel-send idea-panel-send--secondary" type="button">Cancel</button>
                                <button id="idea-panel-edit-save" class="idea-panel-send" type="button" disabled>Save changes</button>
                            </div>
                        </div>
                        <div class="idea-panel-post-actions">
                            <button id="idea-panel-emoji" class="idea-panel-emoji-btn" type="button" title="Add reaction">
                                <span aria-hidden="true">+</span>
                                <span aria-hidden="true">:)</span>
                            </button>
                            <button
                                id="idea-panel-edit-toggle"
                                class="survey-magic-btn idea-panel-edit-cta"
                                type="button"
                                aria-label="Edit idea before publish"
                                title="Edit idea before publish"
                                hidden
                            >
                                <svg class="w-4 h-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" aria-hidden="true">
                                    <path stroke-linecap="round" stroke-linejoin="round" d="M12 20h9"/>
                                    <path stroke-linecap="round" stroke-linejoin="round" d="M16.5 3.5a2.121 2.121 0 113 3L7 19l-4 1 1-4 12.5-12.5z"/>
                                </svg>
                                <span class="survey-magic-btn-text">Edit idea before publish</span>
                            </button>
                        </div>
                    </div>
                </div>
                <div class="idea-panel-section idea-panel-section--responses">
                    <p class="idea-panel-section-label">Responses</p>
                    <div id="idea-panel-comments" class="idea-panel-comments"></div>
                </div>
            </div>
            <div class="idea-panel-footer">
                <textarea id="idea-panel-input" class="idea-panel-input" placeholder="Write a comment..." rows="2"></textarea>
                <button id="idea-panel-send" class="idea-panel-send" type="button" disabled>Post</button>
            </div>
        </div>

        <div id="safety-review-backdrop" class="safety-review-backdrop" hidden aria-hidden="true"></div>
        <div id="safety-review-dialog" class="safety-review-dialog" role="dialog" aria-modal="true" aria-label="Content safety review" hidden>
            <div class="safety-review-header">
                <h3>Let's keep this space safe</h3>
            </div>
            <div class="safety-review-body">
                <p class="safety-review-copy">Our AI flagged your text as potentially offensive. You can use the suggestion, edit it, or continue with your original text.</p>
                <div class="safety-review-block">
                    <div class="safety-review-block-head">
                        <span class="safety-review-label">Your original message</span>
                        <button id="safety-review-edit-original" class="safety-review-edit-icon" type="button" aria-label="Edit your response" title="Edit your response">
                            <svg class="safety-review-edit-glyph" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" aria-hidden="true">
                                <path stroke-linecap="round" stroke-linejoin="round" d="M12 20h9"/>
                                <path stroke-linecap="round" stroke-linejoin="round" d="M16.5 3.5a2.121 2.121 0 113 3L7 19l-4 1 1-4 12.5-12.5z"/>
                            </svg>
                            <span>Edit your response</span>
                        </button>
                    </div>
                    <textarea id="safety-review-original" class="safety-review-original" rows="4" readonly></textarea>
                </div>
                <div class="safety-review-block">
                    <div class="safety-review-block-head">
                        <span class="safety-review-label">AI suggestion</span>
                        <button id="safety-review-edit-suggestion" class="safety-review-edit-icon" type="button" aria-label="Edit the AI suggestion" title="Edit the AI suggestion">
                            <svg class="safety-review-edit-glyph" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" aria-hidden="true">
                                <path stroke-linecap="round" stroke-linejoin="round" d="M12 20h9"/>
                                <path stroke-linecap="round" stroke-linejoin="round" d="M16.5 3.5a2.121 2.121 0 113 3L7 19l-4 1 1-4 12.5-12.5z"/>
                            </svg>
                            <span>Edit the AI suggestion</span>
                        </button>
                    </div>
                    <textarea id="safety-review-suggestion" class="safety-review-suggestion" rows="4" readonly></textarea>
                </div>
            </div>
            <div class="safety-review-actions">
                <button id="safety-review-accept-suggestion" class="safety-review-btn safety-review-btn--primary" type="button">Accept suggestion</button>
                <button id="safety-review-post-anyway" class="safety-review-btn safety-review-btn--warn" type="button">Post original anyway</button>
            </div>
        </div>
    `

    const topicTrigger = container.querySelector<HTMLButtonElement>('#ideas-topic-trigger')!
    const topicTriggerValue = container.querySelector<HTMLSpanElement>('#ideas-topic-trigger-value')!
    const topicFloatingTrigger = container.querySelector<HTMLButtonElement>('#ideas-topic-trigger-floating')!
    const topicFloatingTriggerValue = container.querySelector<HTMLSpanElement>('#ideas-topic-trigger-floating-value')!
    const list = container.querySelector<HTMLDivElement>('#ideas-list')!
    const prompt = container.querySelector<HTMLParagraphElement>('#ideas-prompt')!
    const ideasGrid = container.querySelector<HTMLDivElement>('.ideas-grid')!
    const ideasCompose = container.querySelector<HTMLElement>('.ideas-compose')!
    const textarea = container.querySelector<HTMLTextAreaElement>('#ideas-textarea')!
    const submitBtn = container.querySelector<HTMLButtonElement>('#ideas-submit')!
    const magicBtn = container.querySelector<HTMLButtonElement>('#ideas-magic')!
    const speakBtn = container.querySelector<HTMLButtonElement>('#ideas-speak')!
    const panelBackdrop = container.querySelector<HTMLDivElement>('#idea-panel-backdrop')!
    const panelClose = container.querySelector<HTMLButtonElement>('#idea-panel-close')!
    const ideasShell = container.querySelector<HTMLDivElement>('.ideas-shell')!

    // Create controllers
    const safetyReviewDialog = createSafetyReviewDialogController({ root: container })
    const ideaPanel = createIdeaPanelController({
        root: container,
        reviewBeforePost: (input) => safetyReviewDialog.reviewBeforePost(input),
        reviewWithSuggestion: (original, suggestion) => safetyReviewDialog.reviewWithSuggestion(original, suggestion),
        updateIdeaAfterSafetyReview: (idea, text, markForReview) =>
            updateIdeaAfterSafetyReview(
                params.organizationSlug,
                params.projectSlug,
                idea.topicId,
                idea.id,
                text,
                markForReview,
            ),
        loadResponses: (idea) => getIdeaResponses(params.organizationSlug, params.projectSlug, idea, youthToken),
        submitResponse: (idea, text) => addIdeaResponse(params.organizationSlug, params.projectSlug, idea, youthToken, text),
        updateResponseAfterSafetyReview: (idea, responseId, text, markForReview) =>
            updateIdeaResponseAfterSafetyReview(
                params.organizationSlug,
                params.projectSlug,
                idea,
                responseId,
                youthToken,
                text,
                markForReview,
            ),
        reactToResponse: (idea, responseId, emoji) =>
            addResponseReaction(params.organizationSlug, params.projectSlug, idea, responseId, youthToken, emoji),
        unreactToResponse: (idea, responseId, emoji) =>
            removeResponseReaction(params.organizationSlug, params.projectSlug, idea, responseId, youthToken, emoji),
        reactToIdea: (idea, emoji) => addIdeaReaction(params.organizationSlug, params.projectSlug, idea, youthToken, emoji),
        unreactToIdea: (idea, emoji) => removeIdeaReaction(params.organizationSlug, params.projectSlug, idea, youthToken, emoji),
        onIdeaReactionsUpdated: (ideaId, reactions) => {
            const ideaIndex = allIdeas.findIndex((item) => item.id === ideaId)
            if (ideaIndex < 0) return
            allIdeas[ideaIndex] = { ...allIdeas[ideaIndex], reactions }
        },
    })

    let visibleIdeasCache: Idea[] = []

    const topicModal = createTopicModalController({
        root: container,
        topics,
        onSelect: (nextView) => {
            activeView = nextView
            render()
        },
    })

    let listController: ReturnType<typeof createIdeasListController> | null = null

    const submitHandler = createIdeasSubmitHandler({
        organizationSlug: params.organizationSlug,
        projectSlug: params.projectSlug,
        projectId: project.id,
        reviewBeforePost: (input) => safetyReviewDialog.reviewBeforePost(input),
        reviewWithSuggestion: (original, suggestion) => safetyReviewDialog.reviewWithSuggestion(original, suggestion),
        onIdeaSubmitted: (idea, isFlagged) => {
            allIdeas.unshift(idea)
            if (isFlagged) {
                flaggedIdeaIds.add(idea.id)
            }
            textarea.value = ''
            render()
        },
    })

    function getVisibleIdeas(): Idea[] {
        if (activeView.type === 'my-ideas') {
            return allIdeas.filter((idea) => idea.authorType === 'self')
        }

        const topicId = activeView.topicId
        return allIdeas.filter((idea) => idea.topicId === topicId)
    }

    function updateTopicLabels(): void {
        const label = getActiveIdeasLabel(activeView, topics)
        topicTriggerValue.textContent = label
        topicFloatingTriggerValue.textContent = label
        topicFloatingTrigger.hidden = activeView.type !== 'my-ideas'
        ideasShell.classList.toggle('ideas-shell--my-ideas', activeView.type === 'my-ideas')
    }

    function render(): void {
        visibleIdeasCache = getVisibleIdeas()
        updateTopicLabels()

        // Cleanup old list controller
        if (listController) {
            listController.cleanup()
        }

        // Create new list controller
        listController = createIdeasListController({
            list,
            ideas: visibleIdeasCache,
            activeView,
            topics,
            flaggedIdeaIds,
        })

        if (visibleIdeasCache.length > 0) {
            listController.setActive(0, false)
            listController.startRotation()
        }

        renderIdeasComposer({
            activeView,
            topics,
            ideasGrid,
            ideasCompose,
            composeTopic: topicTriggerValue,
            prompt,
            textarea,
            submitBtn,
            magicBtn,
            speakBtn,
        })

        topicModal.renderTopics(activeView)
    }

    // Wire up event listeners
    topicTrigger.addEventListener('click', () => {
        topicModal.open(topicTrigger)
    })

    topicFloatingTrigger.addEventListener('click', () => {
        topicModal.open(topicFloatingTrigger)
    })

    list.addEventListener('scroll', () => {
        listController?.updateFromScroll()
    }, { passive: true })

    list.addEventListener('click', (event) => {
        const target = event.target as HTMLElement
        const card = target.closest<HTMLElement>('.ideas-card')
        if (!card || !listController) return

        const index = Number(card.getAttribute('data-original-index'))
        if (!Number.isFinite(index) || index < 0 || index >= visibleIdeasCache.length) return

        listController.setActive(index, true)
        ideaPanel.open(visibleIdeasCache[index])
    })

    // Resume animation when panel closes
    panelClose.addEventListener('click', () => {
        listController?.startRotation()
    })

    panelBackdrop.addEventListener('click', () => {
        listController?.startRotation()
    })

    // Dynamically show/hide cards based on available space
    const resizeObserver = new ResizeObserver(() => {
        // List controller handles card visibility internally
    })
    resizeObserver.observe(list)

    // Cleanup on navigation
    window.addEventListener('app:before-navigate', () => {
        listController?.cleanup()
        resizeObserver.disconnect()
    }, { once: false })

    // Magic button focus behavior
    textarea.addEventListener('focus', () => {
        magicBtn?.classList.add('survey-magic-btn-focused')
    })

    textarea.addEventListener('blur', () => {
        magicBtn?.classList.remove('survey-magic-btn-focused')
    })

    textarea.addEventListener('input', () => {
        submitBtn.disabled = textarea.value.trim().length === 0 || activeView.type !== 'topic'
    })

    submitBtn.addEventListener('click', async () => {
        if (activeView.type !== 'topic') return

        const body = textarea.value.trim()
        if (body.length === 0) return

        submitBtn.disabled = true
        submitBtn.textContent = 'Checking...'

        try {
            await submitHandler.submit(body, activeView)
        } finally {
            submitBtn.textContent = 'Submit Idea'
            submitBtn.disabled = textarea.value.trim().length === 0 || activeView.type !== 'topic'
        }
    })

    render()
}
