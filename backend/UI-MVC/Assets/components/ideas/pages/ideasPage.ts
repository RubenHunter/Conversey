import '../../../styles/pages/ideas.css'
//import type { RouteParams } from '../../utils/router'
import { getProject } from '../../../services/projectService'
import { applyTheme } from '../../../utils/theme'
import {
    getIdeasContext,
    getOrCreateProjectScopedYouthId,
    saveYouthContactEmail,
    updateIdeaAfterSafetyReview,
} from '../../../services/ideaService'
import {
    addIdeaReaction,
    addIdeaResponse,
    addResponseReaction,
    getIdeaResponses,
    removeIdeaReaction,
    removeResponseReaction,
    updateIdeaResponseAfterSafetyReview,
} from '../../../services/ideaResponseService'
import { bindMicButton, createSpeakerButton, getSpeechLanguage, type SpeakerButtonController } from '../../../services/speechService'
import type { Idea, IdeaTopic } from '../../../models/idea'
import { resolveInitialIdeasView } from '../utils/initialView'
import { createIdeaPanelController } from '../components/ideaPanel'
import { createSafetyReviewDialogController } from '../components/safetyReviewDialog'
import {createFirstIdeaContactDialogController} from '../components/firstIdeaContactDialog'
import {createIdeaNudgeDialogController} from '../components/ideaNudgeDialog'
import { renderIdeasComposer } from '../components/composer'
import { renderIdeasHeader } from "../utils/ideasHeader"
import {createTopicModalController} from "../components/topicModal";
import {createIdeasListController} from "../components/ideasListController";
import {createIdeasSubmitHandler} from "../components/ideasSubmitHandler";
import { wireBrainstormButton, type BrainstormModalController } from '../../shared/brainstormMode'
import type { ActiveView } from '../types'
import { DiscoveryMode, DiscoveryBadgeType, DiscoveryFeed } from '../types'
import { IdeaAuthorType } from '../../../models/idea'
import { getSurveyStrings } from '../../../i18n/survey'
import { hasOwnIdeaInTopic, getTopicSemanticCategories, createDiscoveryFeed } from '../utils/ideasDiscovery'
import { getVisibleIdeas, type DiscoveryOptions } from '../utils/discoveryApi'
import {ProjectContext, render} from "../../../main";


// Get label for active ideas view
function getActiveIdeasLabel(activeView: ActiveView, topics: IdeaTopic[], t: any): string {
    if (activeView.type === 'my-ideas') return t.myIdeas
    const topic = topics.find((item) => item.id === activeView.topicId)
    return topic ? topic.title : t.selectTopic
}

const IDEAS_BATCH_SIZE = 7
const LOAD_MORE_SCROLL_THRESHOLD = 150

const getDiscoveryLabels = (t: any): Record<DiscoveryMode, string> => ({
    all: t.allIdeas,
    similar: t.similarIdeas,
    different: t.differingIdeas,
    random: '',
})





export async function renderIdeasPage(container: HTMLElement, params: ProjectContext): Promise<void> {
    const t = getSurveyStrings()
    let discoveryRequestToken = 0
    const project = await getProject(params.organizationSlug, params.projectSlug)
    applyTheme(project.theme)
    const context = await getIdeasContext(params.organizationSlug, params.projectSlug, project)
    const youthToken = getOrCreateProjectScopedYouthId(project.slug)

    const organizationName = project.organizationName?.trim() || project.organizationSlug
    const topics = context.topics
    const allIdeas = [...context.ideas]
    let suppressListScrollSyncUntil: number = 0

    let activeView: ActiveView = resolveInitialIdeasView(topics, allIdeas)
    const flaggedIdeaIds = new Set<number>()

    let extraLoadsUsed: number = 0
    let isLoadingMoreIdeas: boolean = false
    let autoLoadArmed: boolean = true
    let discoveryMode: DiscoveryMode = DiscoveryMode.All
    let selectedSemanticCategory: string | null = null
    let showPostPreviewPair: boolean = false
    let discoveryCache: Map<string, DiscoveryFeed> = new Map()
    let latestSubmittedIdea: Idea | null = null
    let visibleIdeasCache: Idea[] = []
    let discoveryBadgeByIdeaId: ReadonlyMap<number, DiscoveryBadgeType> = new Map()
    let lastScrollTop: number = 0

    const headerHTML = renderIdeasHeader({ organizationName, organizationSlug: project.organizationSlug, organizationLogo: project.organizationLogo })

    container.innerHTML = `
        <div class="ideas-shell h-svh w-full self-stretch overflow-hidden flex flex-col bg-[var(--color-bg)]">
            ${headerHTML}

            <div class="ideas-body flex-1 min-h-0 w-[min(100%,1000px)] mx-auto flex flex-col overflow-hidden px-[var(--spacing-sm)] pb-[var(--spacing-md)] gap-[var(--spacing-sm)]">
                <div class="ideas-grid flex-1 min-h-0 min-w-0 w-full grid grid-rows-[minmax(0,1fr)_auto] gap-[var(--spacing-md)]">
                    <section class="ideas-community min-h-0 flex flex-col overflow-hidden overscroll-contain relative" aria-label="Ideas list">
                        <div id="ideas-discovery" class="ideas-discovery" hidden>
                            <button
                                id="ideas-discovery-trigger"
                                class="ideas-discovery-trigger"
                                type="button"
                                aria-haspopup="menu"
                                aria-expanded="false"
                            >
                                <span id="ideas-discovery-label">${t.exploreIdeas}</span>
                                <span class="ideas-discovery-chevron" aria-hidden="true">▾</span>
                            </button>
                            <div id="ideas-discovery-menu" class="ideas-discovery-menu" role="menu" hidden>
                                <button class="ideas-discovery-option" data-discovery-mode="similar" role="menuitem" type="button">${t.similarIdeas}</button>
                                <button class="ideas-discovery-option" data-discovery-mode="different" role="menuitem" type="button">${t.differingIdeas}</button>
                                <button class="ideas-discovery-option" data-discovery-mode="all" role="menuitem" type="button">${t.allIdeas}</button>
                            </div>
                        </div>
                        <div id="ideas-list" class="ideas-list flex-1 min-h-0 overflow-y-auto py-[var(--spacing-sm)] px-[var(--spacing-md)] flex flex-col gap-[var(--spacing-xs)] overscroll-contain snap-none" aria-live="polite"></div>
                        <button id="ideas-load-more" class="ideas-load-more" type="button" hidden>
                            <span class="ideas-load-more-icon" aria-hidden="true">
                                <svg class="ideas-load-more-ring" viewBox="0 0 36 36" xmlns="http://www.w3.org/2000/svg" aria-hidden="true">
                                    <circle class="ideas-load-more-ring-track" cx="18" cy="18" r="14"/>
                                    <circle class="ideas-load-more-ring-fill" cx="18" cy="18" r="14"/>
                                </svg>
                                <span class="ideas-load-more-arrow">↓</span>
                            </span>
                            <span id="ideas-load-more-text" class="ideas-load-more-text">${t.loadMoreIdeas}</span>
                        </button>
                    </section>

                    <section class="ideas-compose" aria-label="Create idea">
                        <div class="ideas-compose-head max-[450px]:flex-row max-[450px]:items-center max-[450px]:flex-wrap max-[450px]:gap-0 max-[450px]:mb-0">
                            <button id="ideas-topic-trigger" class="ideas-compose-topic-button" aria-haspopup="dialog" aria-expanded="false" aria-controls="topic-modal" aria-label="Select topic">
                                <span class="ideas-compose-topic-text">
                                    <span class="ideas-compose-topic-kicker">${t.chooseTopic}</span>
                                    <span id="ideas-topic-trigger-value" class="ideas-compose-topic-value"></span>
                                    <span class="ideas-compose-topic-chevron" aria-hidden="true">▾</span>
                                </span>
                            </button>
                            <div class="survey-question-title ideas-prompt-title-row max-[450px]:text-xs">
                                <span id="ideas-prompt" class="ideas-prompt max-[450px]:text-xs"></span>
                                <button id="ideas-prompt-speaker" class="survey-speaker-btn"
                                        title="${t.readAloud}" aria-label="${t.readAloud}" disabled>
                                    <svg class="survey-speaker-icon" fill="currentColor" viewBox="0 0 24 24" aria-hidden="true">
                                        <path d="M3 9v6h4l5 4V5L7 9H3zm13.5 3a4.5 4.5 0 00-2.5-4.03v8.06A4.5 4.5 0 0016.5 12zm-2.5-9.5v2.06a7 7 0 010 13.88v2.06c4.01-.91 7-4.49 7-8.99s-2.99-8.08-7-8.99z"/>
                                    </svg>
                                </button>
                            </div>
                        </div>
                        <div class="survey-textarea-wrapper">
                            <textarea id="ideas-textarea" class="survey-textarea max-[370px]:min-h-[calc(var(--spacing-xl)*3.4)] max-[450px]:h-10 max-[450px]:min-h-0 max-[450px]:max-h-10" placeholder="${t.shareIdea}"></textarea>
                            <div class="survey-textarea-actions">
                                <button id="ideas-brainstorm" class="survey-brainstorm-btn" type="button" title="${t.brainstormModeTitle}">
                                    <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24" aria-hidden="true">
                                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 3v4M3 5h4M6 17v4m-2-2h4m5-16l2.286 6.857L21 12l-5.714 2.143L13 21l-2.286-6.857L5 12l5.714-2.143L13 3z"/>
                                    </svg>
                                    <span class="survey-brainstorm-btn-text">${t.brainstormModeButton}</span>
                                </button>
                                <button id="ideas-speak" class="survey-mic-btn" type="button" aria-label="${t.voiceInput}" title="${t.voiceInput}">
                                    <svg class="survey-speaker-icon" fill="currentColor" viewBox="0 0 24 24" aria-hidden="true">
                                        <path d="M12 14c1.66 0 2.99-1.34 2.99-3L15 5c0-1.66-1.34-3-3-3S9 3.34 9 5v6c0 1.66 1.34 3 3 3zm5.3-3c0 3-2.54 5.1-5.3 5.1S6.7 14 6.7 11H5c0 3.41 2.72 6.23 6 6.72V21h2v-3.28c3.28-.48 6-3.3 6-6.72h-1.7z"/>
                                    </svg>
                                </button>
                            </div>
                        </div>
                        <button id="ideas-submit" class="ideas-submit max-[450px]:py-1 max-[450px]:text-xs" type="button">Submit Idea</button>
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
                        <span class="ideas-compose-topic-kicker">${t.chooseTopic}</span>
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
                <h3 id="topic-modal-title">${t.selectTopicTitle}</h3>
                <button id="topic-modal-close" class="modal-close" aria-label="${t.cancel}">&times;</button>
            </div>
            <div class="modal-body">
                <div id="topic-modal-list" class="modal-list"></div>
            </div>
        </div>

        <div id="idea-panel-backdrop" class="idea-panel-backdrop" hidden aria-hidden="true"></div>
        <div id="idea-panel" class="idea-panel" role="dialog" aria-modal="true" aria-label="${t.ideaDetail}" hidden>
            <div class="idea-panel-header">
                <h3 class="idea-panel-title">${t.ideaDetail}</h3>
                <button id="idea-panel-close" class="idea-panel-close" aria-label="${t.cancel}">&times;</button>
            </div>
            <div class="idea-panel-body">
                <div id="idea-panel-pinned" class="idea-panel-pinned" hidden></div>
                <div class="idea-panel-section idea-panel-section--idea">
                    <p class="idea-panel-section-label">${t.originalIdea}</p>
                    <div id="idea-panel-post" class="idea-panel-post">
                        <div id="idea-panel-badges" class="idea-panel-badges"></div>
                        <p id="idea-panel-text" class="idea-panel-text"></p>
                        <div id="idea-panel-edit-region" hidden>
                            <textarea id="idea-panel-edit-input" class="idea-panel-input idea-panel-edit-input" rows="4" placeholder="${t.editIdea}..."></textarea>
                            <div class="idea-panel-edit-actions">
                                <button id="idea-panel-edit-cancel" class="idea-panel-send idea-panel-send--secondary" type="button">${t.cancel}</button>
                                <button id="idea-panel-edit-save" class="idea-panel-send" type="button" disabled>${t.saveChanges}</button>
                            </div>
                        </div>
                        <div class="idea-panel-post-actions">
                            <button id="idea-panel-emoji" class="idea-panel-emoji-btn" type="button" title="${t.addReaction}">
                                <span aria-hidden="true">+</span>
                                <span aria-hidden="true">:)</span>
                            </button>
                            <button
                                id="idea-panel-copy"
                                class="idea-panel-copy-btn"
                                type="button"
                                aria-label="${t.useAsStartingPoint}"
                                title="${t.useAsStartingPoint}"
                                hidden
                            >
                                <svg class="idea-panel-copy-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" aria-hidden="true">
                                    <rect x="9" y="9" width="13" height="13" rx="2" ry="2"></rect>
                                    <path d="M5 15H4a2 2 0 0 1-2-2V4a2 2 0 0 1 2-2h9a2 2 0 0 1 2 2v1"></path>
                                </svg>
                                <span>${t.useAsStartingPoint}</span>
                            </button>
                            <button
                                id="idea-panel-edit-toggle"
                                class="survey-brainstorm-btn idea-panel-edit-cta"
                                type="button"
                                aria-label="${t.editIdea}"
                                title="${t.editIdea}"
                                hidden
                            >
                                <svg class="w-4 h-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" aria-hidden="true">
                                    <path stroke-linecap="round" stroke-linejoin="round" d="M12 20h9"/>
                                    <path stroke-linecap="round" stroke-linejoin="round" d="M16.5 3.5a2.121 2.121 0 1 1 3L7 19l-4 1 1-4 12.5-12.5z"/>
                                </svg>
                                <span class="survey-brainstorm-btn-text">${t.editIdea}</span>
                            </button>
                        </div>
                    </div>
                </div>
                <div class="idea-panel-section idea-panel-section--responses">
                    <p class="idea-panel-section-label">${t.responses}</p>
                    <div id="idea-panel-comments" class="idea-panel-comments"></div>
                </div>
            </div>
            <div class="idea-panel-footer">
                <textarea id="idea-panel-input" class="idea-panel-input" placeholder="${t.writeComment}" rows="2"></textarea>
                <button id="idea-panel-send" class="idea-panel-send" type="button" disabled aria-label="${t.post}">
                    <svg viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><path d="M2.01 21L23 12 2.01 3 2 10l15 2-15 2z"/></svg>
                </button>
            </div>
        </div>

        <div id="safety-review-backdrop" class="safety-review-backdrop" hidden aria-hidden="true"></div>
        <div id="safety-review-dialog" class="safety-review-dialog" role="dialog" aria-modal="true" aria-label="${t.safetyReviewTitle}" hidden>
            <div class="safety-review-header">
                <h3>${t.safetyReviewTitle}</h3>
            </div>
            <div class="safety-review-body">
                <p class="safety-review-copy">${t.safetyReviewCopy}</p>
                <div class="safety-review-block">
                    <div class="safety-review-block-head">
                        <span class="safety-review-label">${t.yourOriginalMessage}</span>
                        <button id="safety-review-edit-original" class="safety-review-edit-icon" type="button" aria-label="${t.editYourResponse}" title="${t.editYourResponse}">
                            <svg class="safety-review-edit-glyph" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" aria-hidden="true">
                                <path stroke-linecap="round" stroke-linejoin="round" d="M12 20h9"/>
                                <path stroke-linecap="round" stroke-linejoin="round" d="M16.5 3.5a2.121 2.121 0 113 3L7 19l-4 1 1-4 12.5-12.5z"/>
                            </svg>
                            <span>${t.editYourResponse}</span>
                        </button>
                    </div>
                    <textarea id="safety-review-original" class="safety-review-original" rows="4" readonly></textarea>
                </div>
                <div class="safety-review-block">
                    <div class="safety-review-block-head">
                        <span class="safety-review-label">${t.aiSuggestion}</span>
                        <button id="safety-review-edit-suggestion" class="safety-review-edit-icon" type="button" aria-label="${t.editAiSuggestion}" title="${t.editAiSuggestion}">
                            <svg class="safety-review-edit-glyph" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" aria-hidden="true">
                                <path stroke-linecap="round" stroke-linejoin="round" d="M12 20h9"/>
                                <path stroke-linecap="round" stroke-linejoin="round" d="M16.5 3.5a2.121 2.121 0 113 3L7 19l-4 1 1-4 12.5-12.5z"/>
                            </svg>
                            <span>${t.editAiSuggestion}</span>
                        </button>
                    </div>
                    <textarea id="safety-review-suggestion" class="safety-review-suggestion" rows="4" readonly></textarea>
                </div>
            </div>
            <div class="safety-review-actions">
                <button id="safety-review-accept-suggestion" class="safety-review-btn safety-review-btn--primary" type="button">${t.acceptSuggestion}</button>
                <button id="safety-review-post-anyway" class="safety-review-btn safety-review-btn--warn" type="button">${t.postOriginalAnyway}</button>
            </div>
        </div>

        <div id="idea-nudge-backdrop" class="modal-backdrop idea-nudge-backdrop" hidden aria-hidden="true"></div>
        <div id="idea-nudge-dialog" class="modal idea-nudge-dialog max-[720px]:w-[calc(100vw-1rem)]" role="dialog" aria-modal="true" aria-labelledby="idea-nudge-title" hidden>
            <div class="modal-header">
                <h3 id="idea-nudge-title">${t.nudgeTitle}</h3>
                <button id="idea-nudge-close" class="modal-close" aria-label="${t.cancel}">&times;</button>
            </div>
            <div class="modal-body idea-nudge-body">
                <p id="idea-nudge-context" class="idea-nudge-context"></p>
                <p id="idea-nudge-status" class="idea-nudge-status">${t.nudgeThinking}</p>
                <div id="idea-nudge-thread" class="idea-nudge-thread max-[720px]:max-h-[220px]" aria-live="polite"></div>
                <label class="idea-nudge-input-wrap" for="idea-nudge-input">
                    <span class="idea-nudge-input-label">${t.yourAnswer}</span>
                    <textarea id="idea-nudge-input" class="idea-nudge-input" rows="2" placeholder=""></textarea>
                </label>
            </div>
            <div class="idea-nudge-actions-wrap">
                <button id="idea-nudge-action" class="safety-review-btn safety-review-btn--primary" type="button">${t.answerContinue}</button>
            </div>
        </div>
        
        <div id="first-idea-contact-gate-backdrop" class="modal-backdrop first-idea-contact-backdrop" hidden aria-hidden="true"></div>
        <div id="first-idea-contact-gate-dialog" class="modal first-idea-contact-gate-dialog" role="dialog" aria-modal="true" aria-labelledby="first-idea-contact-gate-title" hidden>
            <div class="modal-header">
                <h3 id="first-idea-contact-gate-title">${t.wantStayInTouch}</h3>
            </div>
            <div class="modal-body">
                <p class="first-idea-contact-copy">${t.stayInTouchCopy}</p>
                <label class="first-idea-contact-check first-idea-contact-check--remember">
                    <input id="first-idea-contact-gate-remember" class="first-idea-contact-checkbox" type="checkbox" />
                    <span>${t.dontAskAgain}</span>
                </label>
            </div>
            <div class="first-idea-contact-actions">
                <button id="first-idea-contact-gate-deny" class="safety-review-btn first-idea-contact-deny" type="button">${t.noThanks}</button>
                <button id="first-idea-contact-gate-accept" class="safety-review-btn safety-review-btn--primary" type="button">${t.leaveMyEmail}</button>
            </div>
        </div>

        <div id="first-idea-contact-backdrop" class="modal-backdrop first-idea-contact-backdrop" hidden aria-hidden="true"></div>
        <div id="first-idea-contact-dialog" class="modal first-idea-contact-dialog" role="dialog" aria-modal="true" aria-labelledby="first-idea-contact-title" hidden>
            <div class="modal-header">
                <h3 id="first-idea-contact-title">${t.stayInTouchTitle}</h3>
            </div>
            <div class="modal-body first-idea-contact-body">
                <p class="first-idea-contact-copy">${t.leaveEmailCopy} <a class="first-idea-contact-privacy-link" href="https://treecompany.be/privacyverklaring/" target="_blank" rel="noopener noreferrer">${t.leaveEmailCopy}</a></p>
                <label class="first-idea-contact-field" for="first-idea-contact-email">
                    <span class="first-idea-contact-label">${t.emailAddress}</span>
                    <input id="first-idea-contact-email" class="first-idea-contact-input" type="email" autocomplete="email" placeholder="you@example.com" />
                </label>
                <label class="first-idea-contact-check">
                    <input id="first-idea-contact-permission" class="first-idea-contact-checkbox" type="checkbox" />
                    <span>${t.agreeContact}</span>
                </label>
                <label class="first-idea-contact-check first-idea-contact-check--remember">
                    <input id="first-idea-contact-remember" class="first-idea-contact-checkbox" type="checkbox" />
                    <span>${t.rememberChoice}</span>
                </label>
            </div>
            <div class="first-idea-contact-actions">
                <button id="first-idea-contact-deny" class="safety-review-btn first-idea-contact-deny" type="button">${t.deny}</button>
                <button id="first-idea-contact-accept" class="safety-review-btn safety-review-btn--primary first-idea-contact-accept" type="button" disabled>${t.allowContact}</button>
            </div>
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
    const brainstormBtn = container.querySelector<HTMLButtonElement>('#ideas-brainstorm')!
    const speakBtn = container.querySelector<HTMLButtonElement>('#ideas-speak')!
    const promptSpeakerBtn = container.querySelector<HTMLButtonElement>('#ideas-prompt-speaker')!
    const unbindMic = bindMicButton(speakBtn, textarea, getSpeechLanguage, (text) => {
        textarea.value = text
        textarea.dispatchEvent(new Event('input', { bubbles: true }))
        submitBtn.disabled = textarea.value.trim().length === 0
    })
    const promptSpeaker: SpeakerButtonController = createSpeakerButton(
        promptSpeakerBtn,
        () => prompt.textContent ?? '',
        getSpeechLanguage
    )
    const panelBackdrop = container.querySelector<HTMLDivElement>('#idea-panel-backdrop')!
    const panelClose = container.querySelector<HTMLButtonElement>('#idea-panel-close')!
    const ideasShell = container.querySelector<HTMLDivElement>('.ideas-shell')!
    const discoveryRoot = container.querySelector<HTMLDivElement>('#ideas-discovery')!
    const discoveryTrigger = container.querySelector<HTMLButtonElement>('#ideas-discovery-trigger')!
    const discoveryLabel = container.querySelector<HTMLSpanElement>('#ideas-discovery-label')!
    const discoveryMenu = container.querySelector<HTMLDivElement>('#ideas-discovery-menu')!
    const firstIdeaContactStorageKey = `ideas-contact-consent:${params.organizationSlug}:${params.projectSlug}`
    let firstIdeaContactDialog = createFirstIdeaContactDialogController({
        root: container,
        storageKey: firstIdeaContactStorageKey,
    })

    const brainstormModal: BrainstormModalController = wireBrainstormButton(brainstormBtn, {
        getQuestionText: () => prompt.textContent ?? '',
        onResult: (finalText: string) => {
            if (finalText.trim()) {
                textarea.value = finalText
                textarea.dispatchEvent(new Event('input', { bubbles: true }))
                submitBtn.disabled = textarea.value.trim().length === 0 || activeView.type !== 'topic'
            }
        }
    })

    function getNudgingContext(view: ActiveView) {
        if (view.type !== 'topic') return null
        const topic = topics.find((item) => item.id === view.topicId)
        if (!topic) return null
        return {
            projectTitle: project.title,
            projectDescription: project.description,
            topicTitle: topic.title,
            topicPrompt: topic.prompt,
        }
    }

    function resetIdeasListToTop(): void {
        list.scrollTop = 0
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



    function renderDiscoveryMenuOptions(): void {
        if (activeView.type !== 'topic') {
            discoveryMenu.innerHTML = ''
            return
        }

        const categories = getTopicSemanticCategories(allIdeas, activeView.topicId)
        const semanticButtons = categories
            .map((category) => `<button class="ideas-discovery-option" data-semantic-category="${category.replace(/"/g, '&quot;')}" role="menuitem" type="button">${category}</button>`)
            .join('')
        const categoriesSection = categories.length > 0
            ? `<hr class="ideas-discovery-separator" role="separator">
               <p class="ideas-discovery-section-label">${t.ideaCategories}</p>
               ${semanticButtons}`
            : ''

        if (!hasOwnIdeaInTopic(allIdeas, activeView.topicId)) {
            discoveryMenu.innerHTML = `
                <button class="ideas-discovery-option" data-discovery-mode="all" role="menuitem" type="button">${t.broadSelection}</button>
                ${categoriesSection}
            `
            return
        }

        discoveryMenu.innerHTML = `
            <button class="ideas-discovery-option" data-discovery-mode="similar" role="menuitem" type="button">${t.similarIdeas}</button>
            <button class="ideas-discovery-option" data-discovery-mode="different" role="menuitem" type="button">${t.differingIdeas}</button>
            <button class="ideas-discovery-option" data-discovery-mode="all" role="menuitem" type="button">${t.allIdeas}</button>
            ${categoriesSection}
        `
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

        const discoveryLabels = getDiscoveryLabels(t)
        discoveryLabel.textContent = ownIdeaExists ? discoveryLabels[discoveryMode] : (selectedSemanticCategory ?? t.broadSelection)

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
            const myIdeas = allIdeas.filter((idea) => idea.authorType === IdeaAuthorType.Self)
            return createDiscoveryFeed(myIdeas, new Map())
        }

        const options: DiscoveryOptions = {
            allIdeas,
            topicId: activeView.topicId,
            discoveryMode,
            showPostPreviewPair,
            youthToken,
            organizationSlug: params.organizationSlug,
            projectSlug: params.projectSlug,
            selectedSemanticCategory,
            latestSubmittedIdea,
            discoveryCache,
        }
        return getVisibleIdeas(options)
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
    const ideaNudgeDialog = createIdeaNudgeDialogController({
        root: container,
        workspaceSlug: params.organizationSlug,
        projectSlug: params.projectSlug,
        isCurrentView: () => activeView,
        getContext: getNudgingContext,
    })
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

    const topicModal = createTopicModalController({
        root: container,
        topics,
        onSelect: (nextView) => {
            promptSpeaker.stop()
            activeView = nextView
            if (nextView.type === 'topic') {
                discoveryMode = DiscoveryMode.All
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
        getNudgingContext,
        runNudgingFlow: (input, view) => ideaNudgeDialog.run(input, view),
        onIdeaSubmitted: (idea, isFlagged) => {
            allIdeas.unshift(idea)
            latestSubmittedIdea = idea
            if (isFlagged) {
                flaggedIdeaIds.add(idea.id)
            }
            showPostPreviewPair = true
            resetPaging()
            discoveryCache.clear()
            textarea.value = ''
            void render({resetListPosition: true})
        },
    })

    function updateTopicLabels(): void {
        const label = getActiveIdeasLabel(activeView, topics, t)
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

        list.classList.toggle('ideas-list--preview', showPostPreviewPair)

        // Create new list controller
        listController = createIdeasListController({
            list,
            ideas: pagedIdeas,
            activeView,
            topics,
            flaggedIdeaIds,
            discoveryBadgeByIdeaId,
            onDiscoveryBadgeClick: (badge) => {
                discoveryMode = badge === 'similar' ? DiscoveryMode.Similar : DiscoveryMode.Different
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
            promptSpeakerBtn,
            textarea,
            submitBtn,
            brainstormBtn,
            speakBtn,
        })

        topicModal.renderTopics(activeView)
    }

    function hasMoreIdeasToLoad(): boolean {
        return visibleIdeasCache.length > getVisibleLimit() && extraLoadsUsed < getMaxExtraLoads()
    }

    function getEmptyStateMessage(): string {
        if (activeView.type !== 'topic') return t.noIdeasHere
        const ownIdeaExists = hasOwnIdeaInTopic(allIdeas, activeView.topicId)
        if (!ownIdeaExists) return t.noIdeasYetBeFirst
        if (discoveryMode === DiscoveryMode.Similar) return t.noSimilarIdeasFound
        if (discoveryMode === DiscoveryMode.Different) return t.noContrastingIdeasFound
        return t.noIdeasHere
    }

    function updateLoadMoreButton(): void {
        const wasLoading = loadMoreBtn.classList.contains('ideas-load-more--loading')
        const hasMoreIdeas = hasMoreIdeasToLoad()
        loadMoreBtn.hidden = !hasMoreIdeas
        loadMoreBtn.disabled = isLoadingMoreIdeas || !hasMoreIdeas
        loadMoreBtn.classList.toggle('ideas-load-more--loading', isLoadingMoreIdeas)
        loadMoreBtn.setAttribute('aria-busy', String(isLoadingMoreIdeas))
        loadMoreText.textContent = isLoadingMoreIdeas
            ? t.loadingMoreIdeas
            : t.loadMoreIdeas

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

        if (loadMoreBtn.parentElement !== list) {
            list.appendChild(loadMoreBtn)
        }
        suppressListScrollSync(2500)
        loadMoreBtn.scrollIntoView({ behavior: 'smooth', block: 'center' })

        const firstNewIndex = getVisibleLimit()

        try {
            await new Promise<void>((resolve) => {
                const timeout = window.setTimeout(resolve, 2000)
                const checkCancel = () => {
                    if (!isLoadingMoreIdeas) {
                        window.clearTimeout(timeout)
                        resolve()
                    } else {
                        requestAnimationFrame(checkCancel)
                    }
                }
                requestAnimationFrame(checkCancel)
            })

            if (!isLoadingMoreIdeas) return

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
            discoveryMode = DiscoveryMode.All
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
        const isSuppressed = performance.now() < suppressListScrollSyncUntil
        listController?.updateFromScroll()

        const currentScrollTop = list.scrollTop
        const isScrollingUp = currentScrollTop < lastScrollTop
        lastScrollTop = currentScrollTop

        if (isScrollingUp && isLoadingMoreIdeas) {
            isLoadingMoreIdeas = false
            updateLoadMoreButton()
            if (!isSuppressed) return
        }

        if (isSuppressed) return

        const distanceFromBottom = list.scrollHeight - list.clientHeight - list.scrollTop
        if (distanceFromBottom <= LOAD_MORE_SCROLL_THRESHOLD) {
            if (autoLoadArmed && !isLoadingMoreIdeas && hasMoreIdeasToLoad()) {
                autoLoadArmed = false
                void loadMoreIdeas()
            }
        } else {
            autoLoadArmed = true
        }
        if (!isLoadingMoreIdeas) {
            updateLoadMoreButton()
        }
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
        unbindMic()
        promptSpeaker.stop()
        listController?.cleanup()
        resizeObserver.disconnect()
        discoveryRequestToken += 1
        document.removeEventListener('keydown', handleKeyDown)
        brainstormModal.destroy()
        if (copyPulseTimeout !== null) {
            window.clearTimeout(copyPulseTimeout)
        }
    }, { once: false })

    // Brainstorm button focus behavior
    textarea.addEventListener('focus', () => {
        brainstormBtn?.classList.add('survey-brainstorm-btn-focused')
    })

    textarea.addEventListener('blur', () => {
        brainstormBtn?.classList.remove('survey-brainstorm-btn-focused')
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

        const choice = await firstIdeaContactDialog.open()
        persistContactEmailIfGranted(choice)

        try {
            await submitHandler.submit(body, activeView)
        } finally {
            submitBtn.textContent = t.submitIdea
            submitBtn.disabled = textarea.value.trim().length === 0 || activeView.type !== 'topic'
        }
    })

    void render()
}

render(renderIdeasPage)
