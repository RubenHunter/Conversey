import '../../styles/pages/ideas.css'
//import type { RouteParams } from '../../utils/router.ts'
import { getProject } from '../../services/projectService'
import {
    getDiscoveredIdeasForTopic,
    IDEA_DISCOVERY_MAX_RESULTS,
    getIdeasContext,
    getOrCreateProjectScopedYouthId,
    saveYouthContactEmail,
    updateIdeaAfterSafetyReview,
    type IdeaDiscoveryCategory,
} from '../../services/ideaService'
import {
    addIdeaReaction,
    addIdeaResponse,
    addResponseReaction,
    getIdeaResponses,
    removeIdeaReaction,
    removeResponseReaction,
    updateIdeaResponseAfterSafetyReview,
} from '../../services/ideaResponseService'
import type { Idea, IdeaTopic } from '../../models/idea'
import { resolveInitialIdeasView } from './initialView'
import { createIdeaPanelController } from './ideaPanel'
import { createSafetyReviewDialogController } from './safetyReviewDialog'
import {createFirstIdeaContactDialogController} from './firstIdeaContactDialog'
import { renderIdeasComposer } from './composer'
import type { ActiveView } from './types.ts'
import { renderIdeasHeader } from './ideasHeader'
import { createTopicModalController } from './topicModal'
import { createIdeasListController } from './ideasListController'
import { createIdeasSubmitHandler } from './ideasSubmitHandler'
import {ProjectContext, render} from "../../main";

type DiscoveryBadgeType = 'similar' | 'different'

// Get label for active ideas view
function getActiveIdeasLabel(activeView: ActiveView, topics: IdeaTopic[]): string {
    if (activeView.type === 'my-ideas') return 'My ideas'
    const topic = topics.find((item) => item.id === activeView.topicId)
    return topic ? topic.title : 'Select a topic'
}

type DiscoveryMode = 'all' | 'similar' | 'different'

const IDEAS_BATCH_SIZE = 7
const LOAD_MORE_SCROLL_THRESHOLD = 24

const DISCOVERY_LABELS: Record<DiscoveryMode, string> = {
    all: 'All ideas',
    similar: 'Similar ideas',
    different: 'Differing ideas',
}

interface DiscoveryFeed {
    ideas: Idea[]
    badgesByIdeaId: ReadonlyMap<number, DiscoveryBadgeType>
}

function hasOwnIdeaInTopic(allIdeas: Idea[], topicId: number): boolean {
    return allIdeas.some((idea) => idea.authorType === 'self' && idea.topicId === topicId)
}

function getTopicSemanticCategories(allIdeas: Idea[], topicId: number): string[] {
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

function buildBroadFeed(topicIdeas: Idea[]): Idea[] {
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
    return broad
}


export async function renderIdeasPage(container: HTMLElement, params: ProjectContext): Promise<void> {
    const project = await getProject(params.organizationSlug, params.projectSlug)
    const context = await getIdeasContext(params.organizationSlug, params.projectSlug, project)
    const youthToken = getOrCreateProjectScopedYouthId(project.slug)

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
                                <button class="ideas-discovery-option" data-discovery-mode="different" role="menuitem" type="button">Differing ideas</button>
                                <button class="ideas-discovery-option" data-discovery-mode="all" role="menuitem" type="button">All ideas</button>
                            </div>
                        </div>
                        <div id="ideas-list" class="ideas-list" aria-live="polite"></div>
                        <button id="ideas-load-more" class="ideas-load-more" type="button" hidden>
                            <span class="ideas-load-more-icon" aria-hidden="true">
                                <svg class="ideas-load-more-ring" viewBox="0 0 36 36" xmlns="http://www.w3.org/2000/svg" aria-hidden="true">
                                    <circle class="ideas-load-more-ring-track" cx="18" cy="18" r="14"/>
                                    <circle class="ideas-load-more-ring-fill" cx="18" cy="18" r="14"/>
                                </svg>
                                <span class="ideas-load-more-arrow">↓</span>
                            </span>
                            <span id="ideas-load-more-text" class="ideas-load-more-text">Click or scroll down to load 7 more ideas</span>
                        </button>
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
                                id="idea-panel-copy"
                                class="idea-panel-copy-btn"
                                type="button"
                                aria-label="Use this idea as a starting point"
                                title="Use this idea as a starting point"
                                hidden
                            >
                                <svg class="idea-panel-copy-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" aria-hidden="true">
                                    <rect x="9" y="9" width="13" height="13" rx="2" ry="2"></rect>
                                    <path d="M5 15H4a2 2 0 0 1-2-2V4a2 2 0 0 1 2-2h9a2 2 0 0 1 2 2v1"></path>
                                </svg>
                                <span>Use as starter</span>
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
        
        <div id="first-idea-contact-gate-backdrop" class="modal-backdrop first-idea-contact-backdrop" hidden aria-hidden="true"></div>
        <div id="first-idea-contact-gate-dialog" class="modal first-idea-contact-gate-dialog" role="dialog" aria-modal="true" aria-labelledby="first-idea-contact-gate-title" hidden>
            <div class="modal-header">
                <h3 id="first-idea-contact-gate-title">Want to stay in touch?</h3>
            </div>
            <div class="modal-body">
                <p class="first-idea-contact-copy">We can let you know if anything happens with your idea.</p>
                <label class="first-idea-contact-check first-idea-contact-check--remember">
                    <input id="first-idea-contact-gate-remember" class="first-idea-contact-checkbox" type="checkbox" />
                    <span>Don't ask me again</span>
                </label>
            </div>
            <div class="first-idea-contact-actions">
                <button id="first-idea-contact-gate-deny" class="safety-review-btn first-idea-contact-deny" type="button">No thanks</button>
                <button id="first-idea-contact-gate-accept" class="safety-review-btn safety-review-btn--primary" type="button">Leave my email</button>
            </div>
        </div>

        <div id="first-idea-contact-backdrop" class="modal-backdrop first-idea-contact-backdrop" hidden aria-hidden="true"></div>
        <div id="first-idea-contact-dialog" class="modal first-idea-contact-dialog" role="dialog" aria-modal="true" aria-labelledby="first-idea-contact-title" hidden>
            <div class="modal-header">
                <h3 id="first-idea-contact-title">Stay in touch about your idea</h3>
            </div>
            <div class="modal-body first-idea-contact-body">
                <p class="first-idea-contact-copy">You can leave your email if you want us to contact you about your ideas.</p>
                <label class="first-idea-contact-field" for="first-idea-contact-email">
                    <span class="first-idea-contact-label">Email address</span>
                    <input id="first-idea-contact-email" class="first-idea-contact-input" type="email" autocomplete="email" placeholder="you@example.com" />
                </label>
                <label class="first-idea-contact-check">
                    <input id="first-idea-contact-permission" class="first-idea-contact-checkbox" type="checkbox" />
                    <span>I agree to be contacted about this idea.</span>
                </label>
                <a class="first-idea-contact-privacy-link" href="https://treecompany.be/privacyverklaring/" target="_blank" rel="noopener noreferrer">Privacy Policy</a>
                <label class="first-idea-contact-check first-idea-contact-check--remember">
                    <input id="first-idea-contact-remember" class="first-idea-contact-checkbox" type="checkbox" />
                    <span>Remember my choice</span>
                </label>
            </div>
            <div class="first-idea-contact-actions">
                <button id="first-idea-contact-deny" class="safety-review-btn first-idea-contact-deny" type="button">Deny</button>
                <button id="first-idea-contact-accept" class="safety-review-btn safety-review-btn--primary first-idea-contact-accept" type="button" disabled>Allow contact</button>
            </div>
        </div>
    `

    const topicTrigger = container.querySelector<HTMLButtonElement>('#ideas-topic-trigger')!
    const topicTriggerValue = container.querySelector<HTMLSpanElement>('#ideas-topic-trigger-value')!
    const topicFloatingTrigger = container.querySelector<HTMLButtonElement>('#ideas-topic-trigger-floating')!
    const topicFloatingTriggerValue = container.querySelector<HTMLSpanElement>('#ideas-topic-trigger-floating-value')!
    const list = container.querySelector<HTMLDivElement>('#ideas-list')!
    const loadMoreBtn = container.querySelector<HTMLButtonElement>('#ideas-load-more')!
    const loadMoreText = container.querySelector<HTMLSpanElement>('#ideas-load-more-text')!
    const prompt = container.querySelector<HTMLParagraphElement>('#ideas-prompt')!
    const ideasGrid = container.querySelector<HTMLDivElement>('.ideas-grid')!
    const ideasCompose = container.querySelector<HTMLElement>('.ideas-compose')!
    const textareaWrapper = container.querySelector<HTMLDivElement>('.survey-textarea-wrapper')!
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
    const firstIdeaContactStorageKey = `ideas-contact-consent:${params.organizationSlug}:${params.projectSlug}`

    let discoveryMode: DiscoveryMode = 'all'
    let selectedSemanticCategory: string | null = null
    let discoveryRequestToken = 0
    const discoveryCache = new Map<string, DiscoveryFeed>()
    let discoveryBadgeByIdeaId: ReadonlyMap<number, DiscoveryBadgeType> = new Map()
    let suppressListScrollSyncUntil = 0
    let extraLoadsUsed = 0
    let showPostPreviewPair = false
    let latestSubmittedIdea: Idea | null = null
    let isLoadingMoreIdeas = false
    let autoLoadArmed = true

    const firstIdeaContactDialog = createFirstIdeaContactDialogController({
        root: container,
        storageKey: firstIdeaContactStorageKey,
    })

    function resetIdeasListToTop(): void {
        list.scrollTo({top: 0, behavior: 'auto'})
    }

    function suppressListScrollSync(durationMs: number): void {
        suppressListScrollSyncUntil = performance.now() + durationMs
    }

    function resetPaging(): void {
        extraLoadsUsed = 0
        isLoadingMoreIdeas = false
        autoLoadArmed = true
    }

    function getMaxExtraLoads(): number {
        const view = activeView
        if (view.type !== 'topic') return 3
        const topic = topics.find((t) => t.id === view.topicId)
        return topic?.maxBroadSelectionLoads ?? 3
    }

    function getVisibleLimit(): number {
        return IDEAS_BATCH_SIZE * (1 + extraLoadsUsed)
    }

    function persistContactEmailIfGranted(choice: { email: string; permissionGranted: boolean; remembered: boolean } | null): void {
        if (!choice?.permissionGranted || choice.email.trim().length === 0) return

        void saveYouthContactEmail(params.organizationSlug, params.projectSlug, youthToken, choice.email.trim())
            .catch((error) => {
                console.warn('[ideas] failed to persist contact email', error)
            })
    }

    function createPostPreviewFeed(similarIdeas: Idea[], differentIdeas: Idea[], submittedIdea: Idea | null): DiscoveryFeed {
        const previewIdeas: Idea[] = []
        const previewBadges = new Map<number, DiscoveryBadgeType>()
        const seen = new Set<number>()

        const addIdea = (idea: Idea | null | undefined, badge?: DiscoveryBadgeType): boolean => {
            if (!idea || seen.has(idea.id)) return false
            seen.add(idea.id)
            previewIdeas.push(idea)
            if (badge && previewBadges.size < 2) {
                previewBadges.set(idea.id, badge)
            }
            return true
        }

        addIdea(similarIdeas[0], 'similar')
        // Find first different idea that hasn't already been shown as similar
        for (const idea of differentIdeas) {
            if (addIdea(idea, 'different')) break
        }
        addIdea(submittedIdea)

        return createDiscoveryFeed(previewIdeas, previewBadges)
    }

    function renderDiscoveryMenuOptions(): void {
        if (activeView.type !== 'topic') {
            discoveryMenu.innerHTML = ''
            return
        }

        if (!hasOwnIdeaInTopic(allIdeas, activeView.topicId)) {
            const categories = getTopicSemanticCategories(allIdeas, activeView.topicId)
            const semanticButtons = categories
                .map((category) => `<button class="ideas-discovery-option" data-semantic-category="${category.replace(/"/g, '&quot;')}" role="menuitem" type="button">${category}</button>`)
                .join('')
            const categoriesSection = categories.length > 0
                ? `<hr class="ideas-discovery-separator" role="separator">
                   <p class="ideas-discovery-section-label">Idea categories</p>
                   ${semanticButtons}`
                : ''

            discoveryMenu.innerHTML = `
                <button class="ideas-discovery-option" data-discovery-mode="all" role="menuitem" type="button">Broad selection</button>
                ${categoriesSection}
            `
            return
        }

        discoveryMenu.innerHTML = `
            <button class="ideas-discovery-option" data-discovery-mode="similar" role="menuitem" type="button">Similar ideas</button>
            <button class="ideas-discovery-option" data-discovery-mode="different" role="menuitem" type="button">Differing ideas</button>
            <button class="ideas-discovery-option" data-discovery-mode="all" role="menuitem" type="button">All ideas</button>
        `
    }

    function createDiscoveryFeed(ideas: Idea[], badgesByIdeaId: ReadonlyMap<number, DiscoveryBadgeType>): DiscoveryFeed {
        return {
            ideas,
            badgesByIdeaId,
        }
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

        const ownIdeaExists = hasOwnIdeaInTopic(allIdeas, activeView.topicId)
        discoveryRoot.hidden = false
        renderDiscoveryMenuOptions()

        discoveryLabel.textContent = ownIdeaExists ? DISCOVERY_LABELS[discoveryMode] : (selectedSemanticCategory ?? 'Broad selection')

        const options = discoveryMenu.querySelectorAll<HTMLButtonElement>('.ideas-discovery-option')
        options.forEach((option) => {
            const mode = option.dataset.discoveryMode
            const semanticCategory = option.dataset.semanticCategory
            const isOwnMode = ownIdeaExists && mode === discoveryMode
            const isBroadSelection = !ownIdeaExists && !selectedSemanticCategory && mode === 'all'
            const isSemanticSelection = !ownIdeaExists && !!selectedSemanticCategory && semanticCategory === selectedSemanticCategory
            option.classList.toggle('selected', isOwnMode || isBroadSelection || isSemanticSelection)
        })
    }

    async function getVisibleIdeasForCurrentMode(): Promise<DiscoveryFeed> {
        if (activeView.type === 'my-ideas') {
            const myIdeas = allIdeas.filter((idea) => idea.authorType === 'self')
            return createDiscoveryFeed(myIdeas, new Map())
        }

        const topicId = activeView.topicId
        const ownIdeaExists = hasOwnIdeaInTopic(allIdeas, topicId)
        const categorySuffix = ownIdeaExists ? 'own' : (selectedSemanticCategory ?? 'broad')
        const cacheSuffix = discoveryMode === 'all' && showPostPreviewPair ? 'preview' : 'full'
        const cacheKey = `${topicId}:${discoveryMode}:${categorySuffix}:${cacheSuffix}`
        const cached = discoveryCache.get(cacheKey)
        if (cached) {
            return cached
        }

        try {
            let discovered: DiscoveryFeed

            if (!ownIdeaExists) {
                const topicIdeas = allIdeas.filter((idea) => idea.topicId === topicId)

                if (!selectedSemanticCategory) {
                    discovered = createDiscoveryFeed(buildBroadFeed(topicIdeas), new Map())
                } else {
                    const categoryFilter = selectedSemanticCategory.toLowerCase()
                    const filtered = topicIdeas.filter((idea) =>
                        idea.semanticCategories.some((category) => category.toLowerCase() === categoryFilter),
                    )
                    discovered = createDiscoveryFeed(filtered, new Map())
                }
            } else if (discoveryMode === 'all') {
                const [similarIdeas, rawDifferentIdeas] = await Promise.all([
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
                const similarIds = new Set(similarIdeas.map((idea) => idea.id))
                const oppositeIdeas = rawDifferentIdeas.filter((idea) => !similarIds.has(idea.id))

                // Pre-populate individual mode caches so switching modes uses consistent deduplicated data
                const similarCacheKey = `${topicId}:similar:${categorySuffix}:full`
                const differentCacheKey = `${topicId}:different:${categorySuffix}:full`
                if (!discoveryCache.has(similarCacheKey)) {
                    discoveryCache.set(similarCacheKey, createDiscoveryFeed(similarIdeas, new Map()))
                }
                if (!discoveryCache.has(differentCacheKey)) {
                    discoveryCache.set(differentCacheKey, createDiscoveryFeed(oppositeIdeas, new Map()))
                }

                if (showPostPreviewPair) {
                    const submittedIdea = latestSubmittedIdea ?? allIdeas.find((idea) => idea.authorType === 'self' && idea.topicId === topicId) ?? null
                    // Use rawDifferentIdeas for the preview so we always find a unique "differing" pick
                    discovered = createPostPreviewFeed(similarIdeas, rawDifferentIdeas, submittedIdea)
                } else {
                    // Pinned top 3: most similar, most different, then user's own idea
                    const pinnedIdeas: Idea[] = []
                    const pinnedBadges = new Map<number, DiscoveryBadgeType>()
                    const pinnedIds = new Set<number>()

                    const addPinned = (idea: Idea | null | undefined, badge?: DiscoveryBadgeType): void => {
                        if (!idea || pinnedIds.has(idea.id)) return
                        pinnedIds.add(idea.id)
                        pinnedIdeas.push(idea)
                        if (badge) pinnedBadges.set(idea.id, badge)
                    }

                    addPinned(similarIdeas[0], 'similar')
                    for (const idea of oppositeIdeas) {
                        if (!pinnedIds.has(idea.id)) {
                            addPinned(idea, 'different');
                            break
                        }
                    }
                    const userIdea = latestSubmittedIdea ?? allIdeas.find((idea) => idea.authorType === 'self' && idea.topicId === topicId) ?? null
                    addPinned(userIdea)

                    // Remaining topic ideas in broad category-interleaved order
                    const topicIdeas = allIdeas.filter((idea) => idea.topicId === topicId)
                    const broadRemainder = buildBroadFeed(topicIdeas).filter((idea) => !pinnedIds.has(idea.id))
                    discovered = createDiscoveryFeed([...pinnedIdeas, ...broadRemainder], pinnedBadges)
                }
            } else {
                // Fetch both modes in parallel and deduplicate to prevent overlap between lists
                const otherMode: IdeaDiscoveryCategory = discoveryMode === 'similar' ? 'different' : 'similar'
                const [modeIdeas, otherIdeas] = await Promise.all([
                    getDiscoveredIdeasForTopic(
                        params.organizationSlug,
                        params.projectSlug,
                        topicId,
                        youthToken,
                        discoveryMode,
                        IDEA_DISCOVERY_MAX_RESULTS,
                    ),
                    getDiscoveredIdeasForTopic(
                        params.organizationSlug,
                        params.projectSlug,
                        topicId,
                        youthToken,
                        otherMode,
                        IDEA_DISCOVERY_MAX_RESULTS,
                    ),
                ])

                const similarList = discoveryMode === 'similar' ? modeIdeas : otherIdeas
                const rawDifferentList = discoveryMode === 'different' ? modeIdeas : otherIdeas
                const simIds = new Set(similarList.map((idea) => idea.id))
                const deduplicatedDifferent = rawDifferentList.filter((idea) => !simIds.has(idea.id))

                // Cache the other mode so switching back uses consistent data
                const otherCacheKey = `${topicId}:${otherMode}:${categorySuffix}:full`
                if (!discoveryCache.has(otherCacheKey)) {
                    discoveryCache.set(
                        otherCacheKey,
                        createDiscoveryFeed(discoveryMode === 'similar' ? deduplicatedDifferent : similarList, new Map()),
                    )
                }

                discovered = createDiscoveryFeed(discoveryMode === 'similar' ? similarList : deduplicatedDifferent, new Map())
            }

            discoveryCache.set(cacheKey, discovered)
            return discovered
        } catch (error) {
            console.warn('Could not load idea discovery suggestions, falling back to all ideas.', error)
            return createDiscoveryFeed(allIdeas.filter((idea) => idea.topicId === topicId).slice(0, IDEA_DISCOVERY_MAX_RESULTS), new Map())
        }
    }

    let copyPulseTimeout: number | null = null

    function pulseComposerWithCopiedIdea(ideaBody: string): void {
        textarea.value = ideaBody
        textarea.dispatchEvent(new Event('input', {bubbles: true}))
        textarea.focus()
        textarea.setSelectionRange(textarea.value.length, textarea.value.length)

        textareaWrapper.classList.remove('ideas-compose-copied')
        void textareaWrapper.offsetWidth
        textareaWrapper.classList.add('ideas-compose-copied')

        if (copyPulseTimeout !== null) {
            window.clearTimeout(copyPulseTimeout)
        }
        copyPulseTimeout = window.setTimeout(() => {
            textareaWrapper.classList.remove('ideas-compose-copied')
            copyPulseTimeout = null
        }, 850)
    }

    // Create controllers
    const safetyReviewDialog = createSafetyReviewDialogController({root: container})
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
        onCopyIdea: (idea) => {
            pulseComposerWithCopiedIdea(idea.body)
            listController?.startRotation()
        },
        onIdeaReactionsUpdated: (ideaId, reactions) => {
            const ideaIndex = allIdeas.findIndex((item) => item.id === ideaId)
            if (ideaIndex < 0) return
            allIdeas[ideaIndex] = {...allIdeas[ideaIndex], reactions}
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
            void render({resetListPosition: true})
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
            latestSubmittedIdea = idea
            if (isFlagged) {
                flaggedIdeaIds.add(idea.id)
            }
            discoveryMode = 'all'
            selectedSemanticCategory = null
            showPostPreviewPair = true
            resetPaging()
            discoveryCache.clear()
            textarea.value = ''
            void render({resetListPosition: true})
        },
    })

    function updateTopicLabels(): void {
        const label = getActiveIdeasLabel(activeView, topics)
        topicTriggerValue.textContent = label
        topicFloatingTriggerValue.textContent = label
        topicFloatingTrigger.hidden = activeView.type !== 'my-ideas'
        ideasShell.classList.toggle('ideas-shell--my-ideas', activeView.type === 'my-ideas')
    }

    async function render(options?: {
        resetListPosition?: boolean;
        preserveScroll?: boolean;
        preserveActive?: boolean;
        stickToBottom?: boolean
    }): Promise<void> {
        const renderToken = ++discoveryRequestToken
        const previousScrollTop = options?.preserveScroll ? list.scrollTop : 0
        const previousBottomOffset = options?.preserveScroll ? Math.max(0, list.scrollHeight - (list.scrollTop + list.clientHeight)) : 0
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
                void render({resetListPosition: true})
            },
        })

        if (pagedIdeas.length > 0) {
            const nextActiveIndex = Math.max(0, Math.min(previousActiveIndex, pagedIdeas.length - 1))
            listController.setActive(nextActiveIndex, false)
            if (options?.resetListPosition) {
                suppressListScrollSync(350)
                resetIdeasListToTop()
            } else if (options?.preserveScroll) {
                if (options.stickToBottom) {
                    list.scrollTop = Math.max(0, list.scrollHeight - list.clientHeight - previousBottomOffset)
                } else {
                    list.scrollTop = previousScrollTop
                }
            }
            listController.startRotation()
        } else {
            list.innerHTML = `<p class="ideas-empty-state">${getEmptyStateMessage()}</p>`
        }

        updateLoadMoreButton()

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

    function hasMoreIdeasToLoad(): boolean {
        return visibleIdeasCache.length > getVisibleLimit() && extraLoadsUsed < getMaxExtraLoads()
    }

    function getEmptyStateMessage(): string {
        if (activeView.type !== 'topic') return 'No ideas here yet.'
        const ownIdeaExists = hasOwnIdeaInTopic(allIdeas, activeView.topicId)
        if (!ownIdeaExists) return 'No ideas have been shared yet. Be the first!'
        if (discoveryMode === 'similar') return 'Your idea seems super original — no similar ideas found yet.'
        if (discoveryMode === 'different') return 'No clearly contrasting ideas found yet.'
        return 'No ideas here yet.'
    }

    function updateLoadMoreButton(): void {
        const wasLoading = loadMoreBtn.classList.contains('ideas-load-more--loading')
        const hasMoreIdeas = hasMoreIdeasToLoad()
        loadMoreBtn.hidden = !hasMoreIdeas
        loadMoreBtn.disabled = isLoadingMoreIdeas || !hasMoreIdeas
        loadMoreBtn.classList.toggle('ideas-load-more--loading', isLoadingMoreIdeas)
        loadMoreBtn.setAttribute('aria-busy', String(isLoadingMoreIdeas))
        loadMoreText.textContent = isLoadingMoreIdeas
            ? 'Loading 7 more ideas...'
            : 'Click or scroll down to load 7 more ideas'

        // Extra bottom space so the button is visible before the load triggers
        list.classList.toggle('ideas-list--has-more', hasMoreIdeas)

        // Force SVG animation restart each time loading begins
        if (isLoadingMoreIdeas && !wasLoading) {
            const ringFill = loadMoreBtn.querySelector<SVGCircleElement>('.ideas-load-more-ring-fill')
            if (ringFill) {
                ringFill.style.animation = 'none'
                void ringFill.getBoundingClientRect()
                ringFill.style.animation = ''
            }
        }

        if (loadMoreBtn.parentElement !== list) {
            list.appendChild(loadMoreBtn)
        }
    }

    async function loadMoreIdeas(): Promise<void> {
        if (isLoadingMoreIdeas || !hasMoreIdeasToLoad()) return

        isLoadingMoreIdeas = true
        updateLoadMoreButton()

        // Scroll the button into center view and briefly pause auto-scroll while it activates
        suppressListScrollSync(1000)
        loadMoreBtn.scrollIntoView({ behavior: 'smooth', block: 'center' })

        const firstNewIndex = getVisibleLimit()

        try {
            await new Promise<void>((resolve) => {
                window.setTimeout(resolve, 2000)
            })

            extraLoadsUsed += 1
            showPostPreviewPair = false
            await render({})
            suppressListScrollSync(500)
            listController?.setActive(firstNewIndex, true)
        } finally {
            isLoadingMoreIdeas = false
            updateLoadMoreButton()
        }
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

        if (hasOwnIdeaInTopic(allIdeas, activeView.topicId)) {
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
        void loadMoreIdeas()
    })

    document.addEventListener('click', (event) => {
        if (!(event.target instanceof Node)) return
        if (!discoveryRoot.contains(event.target)) {
            closeDiscoveryMenu()
        }
    })

    function handleKeyDown(event: KeyboardEvent): void {
        if (event.key !== 'ArrowUp' && event.key !== 'ArrowDown') return
        if (!listController) return

        const active = document.activeElement
        if (active instanceof HTMLTextAreaElement || active instanceof HTMLInputElement) return

        const ideaPanelEl = container.querySelector('#idea-panel')
        if (ideaPanelEl && !ideaPanelEl.hasAttribute('hidden')) return

        event.preventDefault()

        const currentIndex = listController.getActiveIndex()
        const pagedCount = Math.min(getVisibleLimit(), visibleIdeasCache.length)
        if (pagedCount === 0) return

        const delta = event.key === 'ArrowUp' ? -1 : 1
        const nextIndex = Math.max(0, Math.min(pagedCount - 1, currentIndex + delta))

        if (nextIndex !== currentIndex) {
            suppressListScrollSync(500)
            listController.setActive(nextIndex, true)
        }
    }

    document.addEventListener('keydown', handleKeyDown)

    list.addEventListener('scroll', () => {
        if (performance.now() < suppressListScrollSyncUntil) return
        listController?.updateFromScroll()
        const distanceFromBottom = list.scrollHeight - list.clientHeight - list.scrollTop
        if (distanceFromBottom <= LOAD_MORE_SCROLL_THRESHOLD) {
            if (autoLoadArmed && hasMoreIdeasToLoad()) {
                autoLoadArmed = false
                void loadMoreIdeas()
            }
        } else {
            autoLoadArmed = true
        }
        updateLoadMoreButton()
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
        document.removeEventListener('keydown', handleKeyDown)
        if (copyPulseTimeout !== null) {
            window.clearTimeout(copyPulseTimeout)
        }
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

        void firstIdeaContactDialog.open().then((choice) => {
            persistContactEmailIfGranted(choice)
        })

        try {
            await submitHandler.submit(body, activeView)
        } finally {
            submitBtn.textContent = 'Submit Idea'
            submitBtn.disabled = textarea.value.trim().length === 0 || activeView.type !== 'topic'
        }
    })

    void render()
}

render(renderIdeasPage)