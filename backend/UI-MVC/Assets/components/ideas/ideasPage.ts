import '../../styles/pages/ideas.css'
import type { RouteParams } from '../../utils/router.ts'
import { getProject } from '../../services/projectService.ts'
import {
    getDiscoveredIdeasForTopic,
    IDEA_DISCOVERY_MAX_RESULTS,
    getIdeasContext,
    getIdeasYouthToken,
    type IdeaDiscoveryCategory,
    updateIdeaAfterSafetyReview,
} from '../../services/ideaService.ts'
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

type DiscoveryBadgeType = 'similar' | 'opposite'

// Get label for active ideas view
function getActiveIdeasLabel(activeView: ActiveView, topics: IdeaTopic[]): string {
    if (activeView.type === 'my-ideas') return 'My ideas'
    const topic = topics.find((item) => item.id === activeView.topicId)
    return topic ? topic.title : 'Select a topic'
}

type DiscoveryMode = 'all' | IdeaDiscoveryCategory

const IDEAS_BATCH_SIZE = 10
const MAX_EXTRA_LOADS = 2

const DISCOVERY_LABELS: Record<IdeaDiscoveryCategory, string> = {
    similar: 'Similar ideas',
    different: 'Opposite ideas',
    random: 'Random ideas',
}

interface DiscoveryFeed {
    ideas: Idea[]
    badgesByIdeaId: ReadonlyMap<number, DiscoveryBadgeType>
}

function shuffleIdeas<T>(items: T[]): T[] {
    const next = [...items]
    for (let index = next.length - 1; index > 0; index -= 1) {
        const swapIndex = Math.floor(Math.random() * (index + 1))
        ;[next[index], next[swapIndex]] = [next[swapIndex], next[index]]
    }
    return next
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
                        <div id="ideas-discovery" class="ideas-discovery" hidden>
                            <button
                                id="ideas-discovery-trigger"
                                class="ideas-discovery-trigger"
                                type="button"
                                aria-haspopup="menu"
                                aria-expanded="false"
                            >
                                <span id="ideas-discovery-label">Explore ideas</span>
                                <span class="ideas-discovery-chevron" aria-hidden="true">▾</span>
                            </button>
                            <div id="ideas-discovery-menu" class="ideas-discovery-menu" role="menu" hidden>
                                <button class="ideas-discovery-option" data-discovery-mode="similar" role="menuitem" type="button">Similar ideas</button>
                                <button class="ideas-discovery-option" data-discovery-mode="different" role="menuitem" type="button">Opposite ideas</button>
                                <button class="ideas-discovery-option" data-discovery-mode="random" role="menuitem" type="button">Random ideas</button>
                                <button class="ideas-discovery-option" data-discovery-mode="all" role="menuitem" type="button">All ideas</button>
                            </div>
                        </div>
                        <div id="ideas-list" class="ideas-list" aria-live="polite"></div>
                        <button id="ideas-load-more" class="ideas-load-more" type="button" hidden>Show 10 more ideas</button>
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
    const loadMoreBtn = container.querySelector<HTMLButtonElement>('#ideas-load-more')!
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
    const discoveryRoot = container.querySelector<HTMLDivElement>('#ideas-discovery')!
    const discoveryTrigger = container.querySelector<HTMLButtonElement>('#ideas-discovery-trigger')!
    const discoveryLabel = container.querySelector<HTMLSpanElement>('#ideas-discovery-label')!
    const discoveryMenu = container.querySelector<HTMLDivElement>('#ideas-discovery-menu')!

    let discoveryMode: DiscoveryMode = 'all'
    let selectedSemanticCategory: string | null = null
    let discoveryRequestToken = 0
    const discoveryCache = new Map<string, DiscoveryFeed>()
    let discoveryBadgeByIdeaId: ReadonlyMap<number, DiscoveryBadgeType> = new Map()
    let suppressListScrollSyncUntil = 0
    let extraLoadsUsed = 0
    let showPostPreviewPair = false

    function resetIdeasListToTop(): void {
        list.scrollTo({ top: 0, behavior: 'auto' })
    }

    function suppressListScrollSync(durationMs: number): void {
        suppressListScrollSyncUntil = performance.now() + durationMs
    }

    function resetPaging(): void {
        extraLoadsUsed = 0
    }

    function getVisibleLimit(): number {
        return IDEAS_BATCH_SIZE * (1 + extraLoadsUsed)
    }

    function getTopicSemanticCategories(topicId: number): string[] {
        const categories = new Set<string>()
        allIdeas
            .filter((idea) => idea.topicId === topicId)
            .forEach((idea) => {
                idea.semanticCategories.forEach((category) => {
                    if (category.trim().length > 0) {
                        categories.add(category)
                    }
                })
            })

        return [...categories].sort((a, b) => a.localeCompare(b))
    }

    function renderDiscoveryMenuOptions(): void {
        if (activeView.type !== 'topic') {
            discoveryMenu.innerHTML = ''
            return
        }

        const topicId = activeView.topicId
        if (hasOwnIdeaInTopic(topicId)) {
            discoveryMenu.innerHTML = `
                <button class="ideas-discovery-option" data-discovery-mode="similar" role="menuitem" type="button">Similar ideas</button>
                <button class="ideas-discovery-option" data-discovery-mode="different" role="menuitem" type="button">Opposite ideas</button>
                <button class="ideas-discovery-option" data-discovery-mode="random" role="menuitem" type="button">Random ideas</button>
                <button class="ideas-discovery-option" data-discovery-mode="all" role="menuitem" type="button">All ideas</button>
            `
            return
        }

        const categories = getTopicSemanticCategories(topicId)
        const semanticButtons = categories
            .map((category) => `<button class="ideas-discovery-option" data-semantic-category="${category.replace(/"/g, '&quot;')}" role="menuitem" type="button">${category}</button>`)
            .join('')

        discoveryMenu.innerHTML = `
            <button class="ideas-discovery-option" data-discovery-mode="all" role="menuitem" type="button">Broad selection</button>
            ${semanticButtons}
        `
    }

    function createDiscoveryFeed(ideas: Idea[], badgesByIdeaId: ReadonlyMap<number, DiscoveryBadgeType>): DiscoveryFeed {
        return {
            ideas,
            badgesByIdeaId,
        }
    }

    function createMixedDiscoveryFeed(similarIdeas: Idea[], oppositeIdeas: Idea[]): DiscoveryFeed {
        const mixedIdeas: Idea[] = []
        const badgeByIdeaId = new Map<number, DiscoveryBadgeType>()
        const seen = new Set<number>()

        const addIdea = (idea: Idea, badge: DiscoveryBadgeType): void => {
            if (seen.has(idea.id) || mixedIdeas.length >= IDEA_DISCOVERY_MAX_RESULTS) return
            seen.add(idea.id)
            mixedIdeas.push(idea)
            badgeByIdeaId.set(idea.id, badge)
        }

        if (similarIdeas[0]) addIdea(similarIdeas[0], 'similar')
        if (oppositeIdeas[0]) addIdea(oppositeIdeas[0], 'opposite')

        const remaining = shuffleIdeas([
            ...similarIdeas.slice(1).map((idea) => ({ idea, badge: 'similar' as DiscoveryBadgeType })),
            ...oppositeIdeas.slice(1).map((idea) => ({ idea, badge: 'opposite' as DiscoveryBadgeType })),
        ])

        remaining.forEach(({ idea, badge }) => addIdea(idea, badge))

        if (mixedIdeas.length === 0) {
            return createDiscoveryFeed([], new Map())
        }

        return createDiscoveryFeed(mixedIdeas.slice(0, IDEA_DISCOVERY_MAX_RESULTS), badgeByIdeaId)
    }

    function hasOwnIdeaInTopic(topicId: number): boolean {
        return allIdeas.some((idea) => idea.authorType === 'self' && idea.topicId === topicId)
    }

    function closeDiscoveryMenu(): void {
        discoveryMenu.hidden = true
        discoveryTrigger.setAttribute('aria-expanded', 'false')
    }

    function openDiscoveryMenu(): void {
        discoveryMenu.hidden = false
        discoveryTrigger.setAttribute('aria-expanded', 'true')
    }

    function updateDiscoveryUi(): void {
        if (activeView.type !== 'topic') {
            discoveryRoot.hidden = true
            closeDiscoveryMenu()
            return
        }

        const enabled = hasOwnIdeaInTopic(activeView.topicId)
        discoveryRoot.hidden = false
        renderDiscoveryMenuOptions()

        if (!enabled) {
            discoveryLabel.textContent = selectedSemanticCategory ?? 'Broad selection'
        } else {
            discoveryLabel.textContent = discoveryMode === 'all' ? 'Explore ideas' : DISCOVERY_LABELS[discoveryMode]
        }

        const options = discoveryMenu.querySelectorAll<HTMLButtonElement>('.ideas-discovery-option')
        options.forEach((option) => {
            const mode = option.dataset.discoveryMode
            const semanticCategory = option.dataset.semanticCategory
            const isOwnMode = enabled && mode === discoveryMode
            const isBroadSelection = !enabled && !selectedSemanticCategory && mode === 'all'
            const isSemanticSelection = !enabled && !!selectedSemanticCategory && semanticCategory === selectedSemanticCategory
            option.classList.toggle('selected', isOwnMode || isBroadSelection || isSemanticSelection)
        })
    }

    async function getVisibleIdeasForCurrentMode(): Promise<DiscoveryFeed> {
        if (activeView.type === 'my-ideas') {
            const myIdeas = allIdeas.filter((idea) => idea.authorType === 'self')
            return createDiscoveryFeed(myIdeas, new Map())
        }

        const topicId = activeView.topicId
        const topicIdeas = allIdeas.filter((idea) => idea.topicId === topicId)
        const hasOwnIdea = hasOwnIdeaInTopic(topicId)

        if (!hasOwnIdea) {
            if (!selectedSemanticCategory) {
                // Broad selection: prioritize semantic variety, then fill with remaining ideas.
                const byCategory = new Map<string, Idea[]>()
                const withoutCategories: Idea[] = []

                topicIdeas.forEach((idea) => {
                    if (idea.semanticCategories.length === 0) {
                        withoutCategories.push(idea)
                        return
                    }

                    idea.semanticCategories.forEach((category) => {
                        const bucket = byCategory.get(category) ?? []
                        bucket.push(idea)
                        byCategory.set(category, bucket)
                    })
                })

                const broad: Idea[] = []
                const seen = new Set<number>()
                const categories = [...byCategory.keys()]
                let added = true
                while (added) {
                    added = false
                    categories.forEach((category) => {
                        const bucket = byCategory.get(category)
                        if (!bucket || bucket.length === 0) return
                        const nextIdea = bucket.shift()!
                        if (seen.has(nextIdea.id)) return
                        seen.add(nextIdea.id)
                        broad.push(nextIdea)
                        added = true
                    })
                }

                withoutCategories.forEach((idea) => {
                    if (seen.has(idea.id)) return
                    seen.add(idea.id)
                    broad.push(idea)
                })

                topicIdeas.forEach((idea) => {
                    if (seen.has(idea.id)) return
                    seen.add(idea.id)
                    broad.push(idea)
                })

                return createDiscoveryFeed(broad, new Map())
            }

            const categoryFilter = selectedSemanticCategory.toLowerCase()
            const filtered = topicIdeas.filter((idea) =>
                idea.semanticCategories.some((category) => category.toLowerCase() === categoryFilter),
            )
            return createDiscoveryFeed(filtered, new Map())
        }

        if (discoveryMode === 'all') {
            return createDiscoveryFeed(topicIdeas, new Map())
        }

        const cacheSuffix = discoveryMode === 'random' && showPostPreviewPair ? 'preview' : 'full'
        const cacheKey = `${topicId}:${discoveryMode}:${cacheSuffix}`
        const cached = discoveryCache.get(cacheKey)
        if (cached) {
            return cached
        }

        try {
            let discovered: DiscoveryFeed

            if (discoveryMode === 'random') {
                const [similarIdeas, oppositeIdeas] = await Promise.all([
                    getDiscoveredIdeasForTopic(
                        params.organizationSlug,
                        params.projectSlug,
                        topicId,
                        youthToken,
                        'similar',
                        IDEA_DISCOVERY_MAX_RESULTS,
                    ),
                    getDiscoveredIdeasForTopic(
                        params.organizationSlug,
                        params.projectSlug,
                        topicId,
                        youthToken,
                        'different',
                        IDEA_DISCOVERY_MAX_RESULTS,
                    ),
                ])

                if (showPostPreviewPair) {
                    const previewIdeas: Idea[] = []
                    const previewBadges = new Map<number, DiscoveryBadgeType>()
                    if (similarIdeas[0]) {
                        previewIdeas.push(similarIdeas[0])
                        previewBadges.set(similarIdeas[0].id, 'similar')
                    }
                    if (oppositeIdeas[0] && !previewBadges.has(oppositeIdeas[0].id)) {
                        previewIdeas.push(oppositeIdeas[0])
                        previewBadges.set(oppositeIdeas[0].id, 'opposite')
                    }
                    discovered = createDiscoveryFeed(previewIdeas, previewBadges)
                } else {
                    discovered = createMixedDiscoveryFeed(similarIdeas, oppositeIdeas)
                }
            } else {
                const sourceIdeas = await getDiscoveredIdeasForTopic(
                    params.organizationSlug,
                    params.projectSlug,
                    topicId,
                    youthToken,
                    discoveryMode,
                    IDEA_DISCOVERY_MAX_RESULTS,
                )
                discovered = createDiscoveryFeed(
                    sourceIdeas,
                    new Map(),
                )
            }

            discoveryCache.set(cacheKey, discovered)
            return discovered
        } catch (error) {
            console.warn('Could not load idea discovery suggestions, falling back to all ideas.', error)
            return createDiscoveryFeed(topicIdeas.slice(0, IDEA_DISCOVERY_MAX_RESULTS), new Map())
        }
    }

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
            if (nextView.type === 'topic') {
                discoveryMode = 'all'
                selectedSemanticCategory = null
                showPostPreviewPair = false
            }
            resetPaging()
            closeDiscoveryMenu()
            void render({ resetListPosition: true })
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
            discoveryMode = 'random'
            selectedSemanticCategory = null
            showPostPreviewPair = true
            resetPaging()
            discoveryCache.clear()
            textarea.value = ''
            void render({ resetListPosition: true })
        },
    })

    function updateTopicLabels(): void {
        const label = getActiveIdeasLabel(activeView, topics)
        topicTriggerValue.textContent = label
        topicFloatingTriggerValue.textContent = label
        topicFloatingTrigger.hidden = activeView.type !== 'my-ideas'
        ideasShell.classList.toggle('ideas-shell--my-ideas', activeView.type === 'my-ideas')
    }

    async function render(options?: { resetListPosition?: boolean; preserveScroll?: boolean; preserveActive?: boolean }): Promise<void> {
        const renderToken = ++discoveryRequestToken
        const previousScrollTop = options?.preserveScroll ? list.scrollTop : 0
        const previousActiveIndex = options?.preserveActive ? (listController?.getActiveIndex() ?? 0) : 0
        if (options?.resetListPosition) {
            suppressListScrollSync(350)
            resetIdeasListToTop()
        }
        updateTopicLabels()
        updateDiscoveryUi()

        const discoveryFeed = await getVisibleIdeasForCurrentMode()
        if (renderToken !== discoveryRequestToken) {
            return
        }
        visibleIdeasCache = discoveryFeed.ideas
        discoveryBadgeByIdeaId = discoveryFeed.badgesByIdeaId
        const pagedIdeas = visibleIdeasCache.slice(0, getVisibleLimit())

        // Cleanup old list controller
        if (listController) {
            listController.cleanup()
        }

        // Create new list controller
        listController = createIdeasListController({
            list,
            ideas: pagedIdeas,
            activeView,
            topics,
            flaggedIdeaIds,
            discoveryBadgeByIdeaId,
            onDiscoveryBadgeClick: (badge) => {
                discoveryMode = badge === 'similar' ? 'similar' : 'different'
                showPostPreviewPair = false
                resetPaging()
                closeDiscoveryMenu()
                void render({ resetListPosition: true })
            },
        })

        if (pagedIdeas.length > 0) {
            const nextActiveIndex = Math.max(0, Math.min(previousActiveIndex, pagedIdeas.length - 1))
            listController.setActive(nextActiveIndex, false)
            if (options?.resetListPosition) {
                suppressListScrollSync(350)
                resetIdeasListToTop()
            } else if (options?.preserveScroll) {
                list.scrollTop = previousScrollTop
            }
            listController.startRotation()
        }

        const hasMoreIdeas = visibleIdeasCache.length > pagedIdeas.length
        loadMoreBtn.hidden = !(hasMoreIdeas && extraLoadsUsed < MAX_EXTRA_LOADS)

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

    discoveryTrigger.addEventListener('click', (event) => {
        event.stopPropagation()
        if (discoveryRoot.hidden) return
        if (discoveryMenu.hidden) {
            openDiscoveryMenu()
        } else {
            closeDiscoveryMenu()
        }
    })

    discoveryMenu.addEventListener('click', (event) => {
        const target = event.target as HTMLElement
        const option = target.closest<HTMLButtonElement>('.ideas-discovery-option')
        if (!option || activeView.type !== 'topic') return

        const selectedMode = option.dataset.discoveryMode as DiscoveryMode | undefined
        const semanticCategory = option.dataset.semanticCategory

        if (hasOwnIdeaInTopic(activeView.topicId)) {
            if (!selectedMode) return
            discoveryMode = selectedMode
            selectedSemanticCategory = null
            showPostPreviewPair = false
        } else {
            discoveryMode = 'all'
            selectedSemanticCategory = semanticCategory ?? null
            showPostPreviewPair = false
        }

        resetPaging()
        closeDiscoveryMenu()
        void render({ resetListPosition: true })
    })

    loadMoreBtn.addEventListener('click', () => {
        if (extraLoadsUsed >= MAX_EXTRA_LOADS) return
        extraLoadsUsed += 1
        showPostPreviewPair = false
        void render({ preserveScroll: true, preserveActive: true })
    })

    document.addEventListener('click', (event) => {
        if (!(event.target instanceof Node)) return
        if (!discoveryRoot.contains(event.target)) {
            closeDiscoveryMenu()
        }
    })

    list.addEventListener('scroll', () => {
        if (performance.now() < suppressListScrollSyncUntil) return
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
        discoveryRequestToken += 1
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

    void render()
}
