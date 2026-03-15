import '../../styles/pages/ideas.css'
import type { RouteParams } from '../../utils/router.ts'
import { getProject } from '../../services/projectService.ts'
import { getIdeasContext, getIdeasYouthToken, submitIdea, updateIdeaAfterSafetyReview } from '../../services/ideaService.ts'
import { addIdeaReaction, addIdeaResponse, addResponseReaction, getIdeaResponses, removeIdeaReaction, removeResponseReaction } from '../../services/ideaResponseService.ts'
import type { Idea } from '../../models/idea.ts'
import { resolveInitialIdeasView } from './initialView.ts'
import { createIdeaPanelController } from './ideaPanel.ts'
import { createSafetyReviewDialogController } from './safetyReviewDialog.ts'
import { renderTopicMenu, getActiveIdeasLabel } from './topicSwitcher.ts'
import { renderCommunityIdeasList } from './communityList.ts'
import { renderIdeasComposer } from './composer.ts'
import type { ActiveView } from './types.ts'

function formatOrganizationName(organizationSlug: string): string {
    return organizationSlug
        .split('-')
        .filter((part) => part.length > 0)
        .map((part) => (part.length <= 3 ? part.toUpperCase() : `${part.charAt(0).toUpperCase()}${part.slice(1)}`))
        .join(' ')
}

function getOrganizationBadge(organizationName: string, organizationSlug: string): string {
    const clean = organizationName.replace(/[^a-z0-9]/gi, '') || organizationSlug.replace(/[^a-z0-9]/gi, '')
    return clean.slice(0, 3).toUpperCase() || 'ORG'
}

export async function renderIdeasPage(container: HTMLElement, params: RouteParams): Promise<void> {
    const project = await getProject(params.organizationSlug, params.projectSlug)
    const context = await getIdeasContext(params.organizationSlug, params.projectSlug, project)
    const youthToken = getIdeasYouthToken(project.id)

    const organizationName = project.organizationName?.trim() || formatOrganizationName(project.organizationSlug)
    const organizationBadge = getOrganizationBadge(organizationName, project.organizationSlug)

    const topics = context.topics
    const allIdeas = [...context.ideas]

    let activeView: ActiveView = resolveInitialIdeasView(topics, allIdeas)
    let activeIdeaIndex = 0
    let isTopicMenuOpen = false
    let visibleIdeasCache: Idea[] = []
    let listSyncFrame: number | null = null
    let focusTurnTimeout: number | null = null
    let listScrollUnlockTimeout: number | null = null
    let isProgrammaticListScroll = false
    const flaggedIdeaIds = new Set<number>()

    container.innerHTML = `
        <div class="ideas-shell">
            <div class="survey-topbar">
                <div class="survey-topbar-left">
                    <div class="survey-topbar-logo"><img src="/Conversey_logo.png" alt="Conversey" /></div>
                    <div class="survey-topbar-logo-title">CONVERSEY</div>
                </div>
                <div class="survey-topbar-brand">
                    <div class="survey-topbar-logo-badge">${organizationBadge}</div>
                    <div class="survey-topbar-name">${organizationName}</div>
                </div>
            </div>

            <div class="ideas-body">
                <section class="ideas-topic-switcher" aria-label="Idea topic switcher">
                    <button id="ideas-topic-trigger" class="ideas-topic-trigger" aria-haspopup="listbox" aria-expanded="false" aria-label="Select topic or view">
                        <span class="ideas-topic-trigger-copy">
                            <span class="ideas-topic-trigger-kicker">Selected topic</span>
                            <span id="ideas-topic-trigger-value" class="ideas-topic-trigger-value"></span>
                        </span>
                        <span class="ideas-topic-chevron" aria-hidden="true">▾</span>
                    </button>
                    <div id="ideas-topic-menu" class="ideas-topic-menu" role="listbox"></div>
                </section>

                <div class="ideas-grid">
                    <section class="ideas-community" aria-label="Ideas list">
                        <div class="ideas-community-head">
                            <h2 id="ideas-list-title" class="ideas-community-title"></h2>
                            <div class="ideas-list-controls">
                                <button id="ideas-up" class="ideas-control-btn" aria-label="Previous idea">↑</button>
                                <button id="ideas-down" class="ideas-control-btn" aria-label="Next idea">↓</button>
                            </div>
                        </div>
                        <div id="ideas-list" class="ideas-list"></div>
                    </section>

                    <section class="ideas-compose" aria-label="Create idea">
                        <div class="ideas-compose-head">
                            <p id="ideas-compose-topic" class="ideas-compose-topic"></p>
                            <div class="survey-question-title ideas-prompt-title-row">
                                <span id="ideas-prompt" class="ideas-prompt"></span>
                                <button id="ideas-speak" class="ideas-speaker-btn" type="button" aria-label="Voice input" title="Voice input (coming soon)">
                                    <svg class="survey-speaker-icon" fill="currentColor" viewBox="0 0 24 24" aria-hidden="true">
                                        <path d="M3 9v6h4l5 5V4L7 9H3zm13.5 3c0-1.77-1.02-3.29-2.5-4.03v8.05c1.48-.73 2.5-2.26 2.5-4.02zM14 3.23v2.06c2.89.86 5 3.54 5 6.71s-2.11 5.85-5 6.71v2.06c4.01-.91 7-4.49 7-8.77s-2.99-7.86-7-8.77z"/>
                                    </svg>
                                </button>
                            </div>
                            <div class="survey-magic-row ideas-magic-row">
                                <button id="ideas-magic" class="survey-magic-btn" type="button" title="Answer in Magic Mode (coming soon)">
                                    <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24" aria-hidden="true">
                                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 3v4M3 5h4M6 17v4m-2-2h4m5-16l2.286 6.857L21 12l-5.714 2.143L13 21l-2.286-6.857L5 12l5.714-2.143L13 3z"/>
                                    </svg>
                                    Magic Mode
                                </button>
                            </div>
                        </div>
                        <textarea id="ideas-textarea" class="ideas-textarea" placeholder="Share your idea for this topic..."></textarea>
                        <button id="ideas-submit" class="ideas-submit" type="button">Submit Idea</button>
                    </section>
                </div>
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
                        <button id="idea-panel-emoji" class="idea-panel-emoji-btn" type="button" title="Add reaction">
                            <span aria-hidden="true">+</span>
                            <span aria-hidden="true">:)</span>
                        </button>
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
    const topicMenu = container.querySelector<HTMLDivElement>('#ideas-topic-menu')!
    const listTitle = container.querySelector<HTMLHeadingElement>('#ideas-list-title')!
    const list = container.querySelector<HTMLDivElement>('#ideas-list')!
    const prompt = container.querySelector<HTMLParagraphElement>('#ideas-prompt')!
    const composeTopic = container.querySelector<HTMLParagraphElement>('#ideas-compose-topic')!
    const ideasGrid = container.querySelector<HTMLDivElement>('.ideas-grid')!
    const ideasCompose = container.querySelector<HTMLElement>('.ideas-compose')!
    const textarea = container.querySelector<HTMLTextAreaElement>('#ideas-textarea')!
    const submitBtn = container.querySelector<HTMLButtonElement>('#ideas-submit')!
    const magicBtn = container.querySelector<HTMLButtonElement>('#ideas-magic')!
    const speakBtn = container.querySelector<HTMLButtonElement>('#ideas-speak')!
    const upBtn = container.querySelector<HTMLButtonElement>('#ideas-up')!
    const downBtn = container.querySelector<HTMLButtonElement>('#ideas-down')!

    const safetyReviewDialog = createSafetyReviewDialogController({ root: container })
    const ideaPanel = createIdeaPanelController({
        root: container,
        reviewBeforePost: (input) => safetyReviewDialog.reviewBeforePost(input),
        loadResponses: (idea) => getIdeaResponses(params.organizationSlug, params.projectSlug, idea, youthToken),
        submitResponse: async (idea, text, offensiveContentDetected) => {
            const response = await addIdeaResponse(params.organizationSlug, params.projectSlug, idea, youthToken, text)
            return {
                ...response,
                offensiveContentDetected,
            }
        },
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

    function getVisibleIdeas(): Idea[] {
        if (activeView.type === 'my-ideas') {
            return allIdeas.filter((idea) => idea.authorType === 'self')
        }

        const topicId = activeView.topicId
        return allIdeas.filter((idea) => idea.topicId === topicId)
    }

    function openTopicMenu(): void {
        isTopicMenuOpen = true
        topicMenu.classList.add('open')
        topicTrigger.classList.add('open')
        topicTrigger.setAttribute('aria-expanded', 'true')
    }

    function closeTopicMenu(): void {
        isTopicMenuOpen = false
        topicMenu.classList.remove('open')
        topicTrigger.classList.remove('open')
        topicTrigger.setAttribute('aria-expanded', 'false')
    }

    function renderTopicMenuBlock(): void {
        renderTopicMenu({
            menu: topicMenu,
            topics,
            activeView,
            onSelectMyIdeas: () => {
                activeView = { type: 'my-ideas' }
                activeIdeaIndex = 0
                closeTopicMenu()
                render()
            },
            onSelectTopic: (topicId) => {
                activeView = { type: 'topic', topicId }
                activeIdeaIndex = 0
                closeTopicMenu()
                render()
            },
        })
    }

    function updateArrowState(totalIdeas: number): void {
        if (totalIdeas === 0) {
            upBtn.disabled = true
            downBtn.disabled = true
            return
        }

        upBtn.disabled = activeIdeaIndex <= 0
        downBtn.disabled = activeIdeaIndex >= totalIdeas - 1
    }

    function applyWheelStyles(): void {
        const cards = list.querySelectorAll<HTMLElement>('.ideas-card')
        cards.forEach((card, index) => {
            const distance = Math.abs(index - activeIdeaIndex)
            card.classList.toggle('active', distance === 0)
            card.classList.toggle('near', distance === 1)
            card.classList.toggle('far', distance >= 2)
        })
    }

    function setActiveIdea(nextIndex: number, shouldScroll: boolean): void {
        const totalIdeas = visibleIdeasCache.length
        if (totalIdeas === 0) {
            activeIdeaIndex = 0
            updateArrowState(0)
            return
        }

        activeIdeaIndex = Math.max(0, Math.min(nextIndex, totalIdeas - 1))
        applyWheelStyles()
        updateArrowState(totalIdeas)

        const activeCard = list.querySelector<HTMLElement>(`[data-idea-index="${activeIdeaIndex}"]`)
        if (activeCard) {
            activeCard.classList.remove('turn-focus')
            if (focusTurnTimeout !== null) {
                window.clearTimeout(focusTurnTimeout)
            }
            void activeCard.offsetWidth
            activeCard.classList.add('turn-focus')
            focusTurnTimeout = window.setTimeout(() => {
                activeCard.classList.remove('turn-focus')
                focusTurnTimeout = null
            }, 320)
        }

        if (!shouldScroll) return

        isProgrammaticListScroll = true
        if (listScrollUnlockTimeout !== null) {
            window.clearTimeout(listScrollUnlockTimeout)
        }

        activeCard?.scrollIntoView({ behavior: 'smooth', block: 'center' })

        listScrollUnlockTimeout = window.setTimeout(() => {
            isProgrammaticListScroll = false
            listScrollUnlockTimeout = null
        }, 360)
    }

    function updateActiveIdeaFromScroll(): void {
        if (visibleIdeasCache.length === 0 || isProgrammaticListScroll) return

        if (listSyncFrame !== null) {
            window.cancelAnimationFrame(listSyncFrame)
        }

        listSyncFrame = window.requestAnimationFrame(() => {
            const cards = list.querySelectorAll<HTMLElement>('.ideas-card')
            if (cards.length === 0) return

            const listRect = list.getBoundingClientRect()
            const centerY = listRect.top + listRect.height / 2

            let closestIndex = activeIdeaIndex
            let closestDistance = Number.POSITIVE_INFINITY

            cards.forEach((card) => {
                const rect = card.getBoundingClientRect()
                const cardCenterY = rect.top + rect.height / 2
                const distance = Math.abs(cardCenterY - centerY)
                const index = Number(card.getAttribute('data-idea-index'))

                if (distance < closestDistance) {
                    closestDistance = distance
                    closestIndex = index
                }
            })

            if (closestIndex !== activeIdeaIndex) {
                setActiveIdea(closestIndex, false)
            }

            listSyncFrame = null
        })
    }

    function render(): void {
        topicTriggerValue.textContent = getActiveIdeasLabel(activeView, topics)
        listTitle.textContent = activeView.type === 'my-ideas' ? 'Your ideas by topic' : 'Community ideas on this topic'

        renderTopicMenuBlock()
        visibleIdeasCache = getVisibleIdeas()

        renderCommunityIdeasList({
            list,
            ideas: visibleIdeasCache,
            activeView,
            topics,
            upBtn,
            downBtn,
            flaggedIdeaIds,
        })

        if (visibleIdeasCache.length > 0) {
            setActiveIdea(activeIdeaIndex, false)
        }

        renderIdeasComposer({
            activeView,
            topics,
            ideasGrid,
            ideasCompose,
            composeTopic,
            prompt,
            textarea,
            submitBtn,
            magicBtn,
            speakBtn,
        })
    }

    topicTrigger.addEventListener('click', () => {
        if (isTopicMenuOpen) {
            closeTopicMenu()
        } else {
            openTopicMenu()
        }
    })

    document.addEventListener('click', (event) => {
        if (!container.contains(event.target as Node)) return
        const target = event.target as Node
        if (!topicMenu.contains(target) && !topicTrigger.contains(target)) {
            closeTopicMenu()
        }
    })

    list.addEventListener('scroll', updateActiveIdeaFromScroll, { passive: true })

    list.addEventListener('click', (event) => {
        const target = event.target as HTMLElement
        const card = target.closest<HTMLElement>('.ideas-card')
        if (!card) return

        const index = Number(card.getAttribute('data-idea-index'))
        if (!Number.isFinite(index) || index < 0 || index >= visibleIdeasCache.length) return

        setActiveIdea(index, true)
        ideaPanel.open(visibleIdeasCache[index])
    })

    upBtn.addEventListener('click', () => {
        if (visibleIdeasCache.length === 0) return
        setActiveIdea(activeIdeaIndex - 1, true)
    })

    downBtn.addEventListener('click', () => {
        if (visibleIdeasCache.length === 0) return
        setActiveIdea(activeIdeaIndex + 1, true)
    })

    textarea.addEventListener('input', () => {
        submitBtn.disabled = textarea.value.trim().length === 0 || activeView.type !== 'topic'
    })

    submitBtn.addEventListener('click', async () => {
        if (activeView.type !== 'topic') return

        const body = textarea.value.trim()
        if (body.length === 0) return

        const decision = await safetyReviewDialog.reviewBeforePost(body)
        if (!decision.proceed) {
            textarea.value = body
            textarea.focus()
            submitBtn.disabled = textarea.value.trim().length === 0
            return
        }

        submitBtn.disabled = true
        submitBtn.textContent = 'Checking...'

        console.log('[ideas] submitting idea, waiting for AI moderation...')

        try {
            // First submit — backend runs Mistral moderation
            const result = await submitIdea(params.organizationSlug, params.projectSlug, {
                projectId: project.id,
                topicId: activeView.topicId,
                body,
                authorType: 'self',
            })

            if (result.aiSuggestion) {
                // Backend flagged it — show dialog with real AI suggestion
                // The idea is already saved as Pending in the DB
                console.log('[ideas] showing safety review dialog to user')
                submitBtn.textContent = 'Submit Idea'
                submitBtn.disabled = false

                const decision = await safetyReviewDialog.reviewWithSuggestion(body, result.aiSuggestion)

                if (!decision.proceed) {
                    // User cancelled — leave textarea as-is
                    console.log('[ideas] user dismissed safety dialog — idea stays Pending in DB')
                    return
                }

                if (decision.useOriginal) {
                    // Keep original idea pending; if edited, persist the edited text as pending.
                    if (decision.edited) {
                        await updateIdeaAfterSafetyReview(
                            params.organizationSlug,
                            params.projectSlug,
                            activeView.topicId,
                            result.idea.id,
                            project.id,
                            decision.text,
                            true,
                        )
                        result.idea.body = decision.text
                    }

                    console.log('[ideas] user chose original text — idea stays Pending in DB')
                    allIdeas.unshift({ ...result.idea, authorType: 'self' })
                    flaggedIdeaIds.add(result.idea.id)
                    textarea.value = ''
                    activeIdeaIndex = 0
                    render()
                    return
                }

                // User accepted AI suggestion. If edited, keep it pending for review.
                if (decision.edited) {
                    await updateIdeaAfterSafetyReview(
                        params.organizationSlug,
                        params.projectSlug,
                        activeView.topicId,
                        result.idea.id,
                        project.id,
                        decision.text,
                        true,
                    )

                    console.log('[ideas] edited AI suggestion saved as Pending for review')
                    allIdeas.unshift({ ...result.idea, body: decision.text, authorType: 'self' })
                    flaggedIdeaIds.add(result.idea.id)
                } else {
                    // Unedited AI suggestion can be approved immediately.
                    await updateIdeaAfterSafetyReview(
                        params.organizationSlug,
                        params.projectSlug,
                        activeView.topicId,
                        result.idea.id,
                        project.id,
                        decision.text,
                        false,
                    )

                    console.log('[ideas] unedited AI suggestion accepted and approved')
                    allIdeas.unshift({ ...result.idea, body: decision.text, authorType: 'self' })
                }
            } else {
                // Approved — add directly
                console.log('[ideas] idea approved and added to list')
                allIdeas.unshift({ ...result.idea, authorType: 'self' })
            }

            textarea.value = ''
            activeIdeaIndex = 0
            render()
        } catch (err) {
            console.error('[ideas] submit failed', err)
        } finally {
            submitBtn.textContent = 'Submit Idea'
            submitBtn.disabled = textarea.value.trim().length === 0 || activeView.type !== 'topic'
        }
    })

    render()
}
