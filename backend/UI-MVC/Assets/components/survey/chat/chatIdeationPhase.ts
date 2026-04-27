/**
 * Chat Ideation Phase Controller
 * Handles the ideas browsing, filtering, submission, and discovery in chat survey mode
 */
import type { Project } from '../../../models/project'
import type { ProjectContext } from '../../../main'
import {
    getIdeasContext,
    getOrCreateProjectScopedYouthId,
    saveYouthContactEmail,
    updateIdeaAfterSafetyReview,
    getDiscoveredIdeasForTopic,
    IDEA_DISCOVERY_MAX_RESULTS,
    type IdeaDiscoveryCategory,
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
import { createIdeasListController } from '../../ideas/ideasListController'
import { createSafetyReviewDialogController } from '../../ideas/safetyReviewDialog'
import { createIdeaPanelController } from '../../ideas/ideaPanel'
import { createIdeasSubmitHandler } from '../../ideas/ideasSubmitHandler'
import { createFirstIdeaContactDialogController } from '../../ideas/firstIdeaContactDialog'
import type { Idea, IdeaTopic } from '../../../models/idea'
import type { ActiveView } from '../../ideas/types'
import { getSurveyStrings } from '../../../i18n/survey'
import { wait, esc } from './chatHelpers'
import { AI_AVATAR, CHECKMARK_SVG, MAGIC_SVG } from './chatTemplates'

interface ChatIdeationOptions {
    container: HTMLElement
    params: ProjectContext
    project: Project
    appendAiBubble: (text: string, options?: { animated?: boolean }) => Promise<void>
    chatInput: HTMLTextAreaElement
    chatShell: HTMLDivElement
    scrollAreaEl: HTMLDivElement
    messagesEl: HTMLDivElement
}

interface DiscoveryCache {
    ideas: Idea[]
    badgesByIdeaId: Map<number, 'similar' | 'different'>
}

/**
 * Initiates the chat ideation phase with proper async idea discovery and filtering
 * IMPORTANT: Call this AFTER survey questions are complete to transition from survey -> ideas
 */
export async function initiateChatIdeationPhase(options: ChatIdeationOptions): Promise<void> {
    const t = getSurveyStrings()
    const { container, params, project, appendAiBubble, chatInput, chatShell, scrollAreaEl, messagesEl } = options

    const ideasContext = await getIdeasContext(params.organizationSlug, params.projectSlug, project)
    const youthToken = getOrCreateProjectScopedYouthId(project.slug)
    const allIdeas: Idea[] = [...ideasContext.ideas]
    const topics: IdeaTopic[] = ideasContext.topics
    const firstTopic = topics[0]

    if (!firstTopic) {
        await appendAiBubble('Thank you for completing the survey! Your responses have been recorded.')
        return
    }

    const surveyHeaderEl = container.querySelector<HTMLElement>('#chat-survey-header')!
    surveyHeaderEl.hidden = true

    let currentView: ActiveView = { type: 'topic', topicId: firstTopic.id }
    let currentDiscoveryLabel = t.broadSelection
    let currentSemanticCategory: string | null = null
    let currentDiscoveryMode: 'broad' | 'similar' | 'different' | 'all' = 'broad'

    const firstIdeaContactStorageKey = `ideas-contact-consent:${params.organizationSlug}:${params.projectSlug}`
    const firstIdeaContactDialog = createFirstIdeaContactDialogController({
        root: container,
        storageKey: firstIdeaContactStorageKey,
    })

    const discoveryCache = new Map<string, DiscoveryCache>()

    // ===== Helpers =====
    function hasOwnIdeaInTopic(topicId: number): boolean {
        return allIdeas.some((idea) => idea.authorType === 'self' && idea.topicId === topicId)
    }

    function getSemanticCategoriesForTopic(topicId: number): string[] {
        const cats = new Set<string>()
        allIdeas
            .filter((idea) => idea.topicId === topicId)
            .forEach((idea) => idea.semanticCategories.forEach((c) => { if (c.trim()) cats.add(c) }))
        return [...cats].sort((a, b) => a.localeCompare(b))
    }

    // ===== Topic selector =====
    const topicOptions = topics
        .map(
            (topic) =>
                `<li class="chat-topic-option${topic.id === firstTopic.id ? ' chat-topic-option--active' : ''}" data-topic-id="${topic.id}" role="option" aria-selected="${topic.id === firstTopic.id}">${esc(topic.title)}</li>`,
        )
        .join('')

    const topicSelectorEl = document.createElement('div')
    topicSelectorEl.className = 'chat-topic-selector'
    topicSelectorEl.id = 'chat-topic-selector'
    topicSelectorEl.innerHTML = `
        <button class="chat-topic-trigger" id="chat-topic-trigger" type="button" aria-expanded="false" aria-haspopup="listbox">
            <span class="chat-topic-trigger-label">${esc(t.topicLabel)}</span>
            <span class="chat-topic-trigger-name" id="chat-topic-trigger-name">${esc(firstTopic.title)}</span>
            <svg class="chat-topic-trigger-chevron" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <polyline points="6 9 12 15 18 9"/>
            </svg>
        </button>
        <ul class="chat-topic-dropdown" id="chat-topic-dropdown" role="listbox" hidden>
            ${topicOptions}
            <li class="chat-topic-separator" role="separator" aria-hidden="true"></li>
            <li class="chat-topic-option chat-topic-option--my-ideas" data-view="my-ideas" role="option" aria-selected="false">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" width="14" height="14"><path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"/><circle cx="12" cy="7" r="4"/></svg>
                ${esc(t.myIdeas)}
            </li>
        </ul>`

    // ===== Ideas area =====
    const ideasArea = document.createElement('div')
    ideasArea.className = 'chat-ideas-area'
    ideasArea.innerHTML = `
        <div class="chat-ideas-area-header">
            <span class="chat-ideas-area-title">${esc(t.communityIdeas)}</span>
            <div class="ideas-discovery chat-discovery-wrap" id="chat-discovery">
                <button class="ideas-discovery-trigger" id="chat-discovery-trigger" type="button" aria-expanded="false" aria-haspopup="menu">
                    <span id="chat-discovery-label">${esc(t.broadSelection)}</span>
                    <span class="ideas-discovery-chevron" aria-hidden="true">▾</span>
                </button>
                <div class="ideas-discovery-menu chat-discovery-menu" id="chat-discovery-menu" role="menu" hidden></div>
            </div>
        </div>
        <div class="ideas-list" id="chat-ideas-list" aria-live="polite"></div>`

    chatShell.classList.add('chat-shell--ideas')
    chatShell.insertBefore(topicSelectorEl, scrollAreaEl)
    chatShell.insertBefore(ideasArea, scrollAreaEl)

    const ideasListEl = container.querySelector<HTMLDivElement>('#chat-ideas-list')!
    const flaggedIdeaIds = new Set<number>()

    const discoveryEl = ideasArea.querySelector<HTMLElement>('#chat-discovery')!
    const discoveryTrigger = ideasArea.querySelector<HTMLButtonElement>('#chat-discovery-trigger')!
    const discoveryLabel = ideasArea.querySelector<HTMLSpanElement>('#chat-discovery-label')!
    const discoveryMenu = ideasArea.querySelector<HTMLElement>('#chat-discovery-menu')!

    const safetyDialog = createSafetyReviewDialogController({ root: container })

    const reviewWithSuggestion = async (orig: string, sugg: string) => {
        return safetyDialog.reviewWithSuggestion(orig, sugg)
    }

    let listController: ReturnType<typeof createIdeasListController> | null = null

    const ideaPanel = createIdeaPanelController({
        root: container,
        reviewBeforePost: (input) => safetyDialog.reviewBeforePost(input),
        reviewWithSuggestion,
        updateIdeaAfterSafetyReview: (idea, text, mark) =>
            updateIdeaAfterSafetyReview(
                params.organizationSlug,
                params.projectSlug,
                idea.topicId,
                idea.id,
                text,
                mark,
            ),
        loadResponses: (idea) =>
            getIdeaResponses(params.organizationSlug, params.projectSlug, idea, youthToken),
        submitResponse: (idea, text) =>
            addIdeaResponse(params.organizationSlug, params.projectSlug, idea, youthToken, text),
        updateResponseAfterSafetyReview: (idea, rid, text, mark) =>
            updateIdeaResponseAfterSafetyReview(
                params.organizationSlug,
                params.projectSlug,
                idea,
                rid,
                youthToken,
                text,
                mark,
            ),
        reactToResponse: (idea, rid, emoji) =>
            addResponseReaction(params.organizationSlug, params.projectSlug, idea, rid, youthToken, emoji),
        unreactToResponse: (idea, rid, emoji) =>
            removeResponseReaction(params.organizationSlug, params.projectSlug, idea, rid, youthToken, emoji),
        reactToIdea: (idea, emoji) =>
            addIdeaReaction(params.organizationSlug, params.projectSlug, idea, youthToken, emoji),
        unreactToIdea: (idea, emoji) =>
            removeIdeaReaction(params.organizationSlug, params.projectSlug, idea, youthToken, emoji),
        onCopyIdea: (idea) => {
            chatInput.value = idea.body
            chatInput.dispatchEvent(new Event('input', { bubbles: true }))
            chatInput.focus()
            listController?.startRotation()
        },
        onIdeaReactionsUpdated: (ideaId, reactions) => {
            const idx = allIdeas.findIndex((x) => x.id === ideaId)
            if (idx >= 0) allIdeas[idx] = { ...allIdeas[idx], reactions }
        },
    })

    // ===== Get visible ideas with AI discovery =====
    async function getVisibleIdeasForCurrentMode(): Promise<Idea[]> {
        if (currentView.type === 'my-ideas') {
            return allIdeas.filter((x) => x.authorType === 'self')
        }

        const topicId = currentView.topicId
        const ownIdeaExists = hasOwnIdeaInTopic(topicId)
        const categorySuffix = ownIdeaExists ? 'own' : (currentSemanticCategory ?? 'broad')
        const cacheKey = `${topicId}:${currentDiscoveryMode}:${categorySuffix}`
        
        const cached = discoveryCache.get(cacheKey)
        if (cached) {
            return cached.ideas
        }

        try {
            let ideas: Idea[] = []

            if (!ownIdeaExists) {
                const topicIdeas = allIdeas.filter((idea) => idea.topicId === topicId)

                if (!currentSemanticCategory) {
                    // Broad selection - just return all ideas
                    ideas = topicIdeas
                } else {
                    // Filter by semantic category
                    const categoryFilter = currentSemanticCategory.toLowerCase()
                    ideas = topicIdeas.filter((idea) =>
                        idea.semanticCategories.some((category) => category.toLowerCase() === categoryFilter),
                    )
                }
            } else if (currentDiscoveryMode === 'all') {
                // User has own idea, fetch both similar and different
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

                // Combine with user's own idea pinned first
                const userIdea = allIdeas.find((idea) => idea.authorType === 'self' && idea.topicId === topicId)
                const combined = [
                    ...(userIdea ? [userIdea] : []),
                    ...similarIdeas.slice(0, 3),
                    ...oppositeIdeas.slice(0, 3),
                    ...allIdeas.filter(
                        (idea) =>
                            idea.topicId === topicId &&
                            idea.authorType !== 'self' &&
                            !similarIds.has(idea.id) &&
                            !oppositeIdeas.some((x) => x.id === idea.id),
                    ),
                ]
                ideas = combined
            } else {
                // Fetch specific mode
                const otherMode: IdeaDiscoveryCategory = currentDiscoveryMode === 'similar' ? 'different' : 'similar'
                const [modeIdeas, otherIdeas] = await Promise.all([
                    getDiscoveredIdeasForTopic(
                        params.organizationSlug,
                        params.projectSlug,
                        topicId,
                        youthToken,
                        currentDiscoveryMode as IdeaDiscoveryCategory,
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

                const similarList = currentDiscoveryMode === 'similar' ? modeIdeas : otherIdeas
                const simIds = new Set(similarList.map((idea) => idea.id))
                const filtered = (currentDiscoveryMode === 'similar' ? modeIdeas : otherIdeas).filter(
                    (idea) => !simIds.has(idea.id),
                )
                ideas = currentDiscoveryMode === 'similar' ? similarList : filtered
            }

            discoveryCache.set(cacheKey, { ideas, badgesByIdeaId: new Map() })
            return ideas
        } catch (error) {
            console.warn('Could not load idea discovery suggestions, falling back to all ideas.', error)
            const topicIdeas = allIdeas.filter((idea) => idea.topicId === topicId)
            return topicIdeas.slice(0, 20)
        }
    }

    function renderDiscoveryMenuOptions(): void {
        if (currentView.type !== 'topic') {
            discoveryMenu.innerHTML = ''
            return
        }

        const topicId = currentView.topicId
        const hasOwn = hasOwnIdeaInTopic(topicId)

        if (!hasOwn) {
            const categories = getSemanticCategoriesForTopic(topicId)
            const catButtons = categories
                .map(
                    (cat) =>
                        `<button class="ideas-discovery-option${currentSemanticCategory === cat ? ' selected' : ''}" data-chat-category="${esc(cat)}" role="menuitem" type="button">${esc(cat)}</button>`,
                )
                .join('')
            const catSection =
                categories.length > 0
                    ? `<hr class="ideas-discovery-separator" role="separator">
                       <p class="ideas-discovery-section-label">${esc(t.ideaCategories)}</p>
                       ${catButtons}`
                    : ''

            discoveryMenu.innerHTML = `
                <button class="ideas-discovery-option${!currentSemanticCategory ? ' selected' : ''}" data-chat-sort="broad" role="menuitem" type="button">${esc(t.broadSelection)}</button>
                ${catSection}`
        } else {
            discoveryMenu.innerHTML = `
                <button class="ideas-discovery-option${currentDiscoveryMode === 'similar' ? ' selected' : ''}" data-chat-sort="similar" role="menuitem" type="button">${esc(t.similarIdeas)}</button>
                <button class="ideas-discovery-option${currentDiscoveryMode === 'different' ? ' selected' : ''}" data-chat-sort="different" role="menuitem" type="button">${esc(t.differingIdeas)}</button>
                <button class="ideas-discovery-option${currentDiscoveryMode === 'all' ? ' selected' : ''}" data-chat-sort="all" role="menuitem" type="button">${esc(t.allIdeas)}</button>`
        }
    }

    function updateDiscoveryLabel(): void {
        discoveryLabel.textContent = currentDiscoveryLabel
    }

    async function renderIdeasList(): Promise<void> {
        if (listController) {
            listController.cleanup()
            listController = null
        }
        const displayIdeas = await getVisibleIdeasForCurrentMode()
        if (displayIdeas.length === 0) {
            ideasListEl.innerHTML = `<p class="ideas-empty-state">${esc(t.noIdeas)}</p>`
            return
        }
        listController = createIdeasListController({
            list: ideasListEl,
            ideas: displayIdeas,
            activeView: currentView,
            topics,
            flaggedIdeaIds,
        })
        listController.startRotation()
    }

    ideasListEl.addEventListener('scroll', () => {
        listController?.updateFromScroll()
    }, { passive: true })

    // Topic selector events
    const topicTrigger = topicSelectorEl.querySelector<HTMLButtonElement>('#chat-topic-trigger')!
    const topicDropdown = topicSelectorEl.querySelector<HTMLElement>('#chat-topic-dropdown')!
    const topicTriggerName = topicSelectorEl.querySelector<HTMLElement>('#chat-topic-trigger-name')!

    topicTrigger.addEventListener('click', (e) => {
        e.stopPropagation()
        const opening = topicDropdown.hidden
        topicDropdown.hidden = !opening
        topicTrigger.setAttribute('aria-expanded', String(opening))
    })

    topicDropdown.addEventListener('click', (e) => {
        const opt = (e.target as HTMLElement).closest<HTMLElement>('[data-topic-id], [data-view="my-ideas"]')
        if (!opt) return

        topicDropdown.hidden = true
        topicTrigger.setAttribute('aria-expanded', 'false')

        if (opt.getAttribute('data-view') === 'my-ideas') {
            if (currentView.type === 'my-ideas') return
            currentView = { type: 'my-ideas' }
            topicTriggerName.textContent = t.myIdeas
            topicDropdown.querySelectorAll('[data-topic-id]').forEach((el) => {
                el.classList.remove('chat-topic-option--active')
                el.setAttribute('aria-selected', 'false')
            })
            opt.classList.add('chat-topic-option--active')
            opt.setAttribute('aria-selected', 'true')
            renderDiscoveryMenuOptions()
            void renderIdeasList()
            return
        }

        const newTopicId = Number(opt.getAttribute('data-topic-id'))
        const newTopic = topics.find((topic) => topic.id === newTopicId)
        if (!newTopic) return
        if (currentView.type === 'topic' && currentView.topicId === newTopicId) return

        currentView = { type: 'topic', topicId: newTopicId }
        topicTriggerName.textContent = newTopic.title
        currentSemanticCategory = null
        currentDiscoveryMode = 'broad'
        currentDiscoveryLabel = hasOwnIdeaInTopic(newTopicId) ? t.similarIdeas : t.broadSelection
        updateDiscoveryLabel()

        topicDropdown.querySelectorAll('[data-topic-id], [data-view="my-ideas"]').forEach((el) => {
            const isActive = el.getAttribute('data-topic-id') === String(newTopicId)
            el.classList.toggle('chat-topic-option--active', isActive)
            el.setAttribute('aria-selected', String(isActive))
        })

        renderDiscoveryMenuOptions()
        void renderIdeasList()
        void appendAiBubble(newTopic.prompt?.trim() || `What are your thoughts on: "${newTopic.title}"?`)
    })

    // Discovery dropdown events
    discoveryTrigger.addEventListener('click', (e) => {
        e.stopPropagation()
        renderDiscoveryMenuOptions()
        const opening = discoveryMenu.hidden
        discoveryMenu.hidden = !opening
        discoveryTrigger.setAttribute('aria-expanded', String(opening))
    })

    discoveryMenu.addEventListener('click', (e) => {
        const opt = (e.target as HTMLElement).closest<HTMLElement>('[data-chat-sort], [data-chat-category]')
        if (!opt) return

        const sort = opt.getAttribute('data-chat-sort')
        const category = opt.getAttribute('data-chat-category')

        if (sort) {
            currentDiscoveryMode = sort as 'broad' | 'similar' | 'different' | 'all'
            currentSemanticCategory = null
            const sortLabels: Record<string, string> = {
                broad: t.broadSelection,
                similar: t.similarIdeas,
                different: t.differingIdeas,
                all: t.allIdeas,
            }
            currentDiscoveryLabel = sortLabels[sort] ?? t.broadSelection
        } else if (category) {
            currentSemanticCategory = category
            currentDiscoveryLabel = category
        }

        discoveryMenu.hidden = true
        discoveryTrigger.setAttribute('aria-expanded', 'false')
        updateDiscoveryLabel()
        renderDiscoveryMenuOptions()
        void renderIdeasList()
    })

    const closeMenusOnOutsideClick = (e: MouseEvent): void => {
        if (!topicSelectorEl.contains(e.target as Node)) {
            topicDropdown.hidden = true
            topicTrigger.setAttribute('aria-expanded', 'false')
        }
        if (!discoveryEl.contains(e.target as Node)) {
            discoveryMenu.hidden = true
            discoveryTrigger.setAttribute('aria-expanded', 'false')
        }
    }
    document.addEventListener('click', closeMenusOnOutsideClick)

    // Ideas list click → open panel
    ideasListEl.addEventListener('click', async (e) => {
        const card = (e.target as HTMLElement).closest<HTMLElement>('.ideas-card')
        if (!card || !listController) return
        const idx = Number(card.getAttribute('data-original-index'))
        const displayIdeas = await getVisibleIdeasForCurrentMode()
        if (!Number.isFinite(idx) || idx < 0 || idx >= displayIdeas.length) return
        listController.setActive(idx, true)
        ideaPanel.open(displayIdeas[idx])
    })

    container.querySelector<HTMLElement>('#idea-panel-backdrop')?.addEventListener('click', () => {
        listController?.startRotation()
    })
    container.querySelector<HTMLElement>('#idea-panel-close')?.addEventListener('click', () => {
        listController?.startRotation()
    })

    window.addEventListener(
        'app:before-navigate',
        () => {
            listController?.cleanup()
            document.removeEventListener('click', closeMenusOnOutsideClick)
        },
        { once: true },
    )

    const submitHandler = createIdeasSubmitHandler({
        organizationSlug: params.organizationSlug,
        projectSlug: params.projectSlug,
        projectId: project.id,
        reviewBeforePost: (input) => safetyDialog.reviewBeforePost(input),
        reviewWithSuggestion,
        onIdeaSubmitted: (idea: Idea) => {
            const wasFirstOwnInTopic =
                currentView.type === 'topic' && !hasOwnIdeaInTopic(currentView.topicId)
            allIdeas.unshift(idea)
            if (wasFirstOwnInTopic) {
                currentDiscoveryMode = 'similar'
                currentDiscoveryLabel = t.similarIdeas
                currentSemanticCategory = null
                updateDiscoveryLabel()
            }
            renderDiscoveryMenuOptions()
            void renderIdeasList()
            void appendAiBubble(t.ideaShared)

            if (!firstIdeaContactDialog.hasStoredDecision()) {
                void firstIdeaContactDialog.open().then((choice) => {
                    if (choice?.permissionGranted && choice.email) {
                        void saveYouthContactEmail(
                            params.organizationSlug,
                            params.projectSlug,
                            youthToken,
                            choice.email,
                        )
                    }
                })
            }
        },
    })

    async function handleIdeaSubmit(): Promise<void> {
        const text = chatInput.value.trim()
        if (!text) return
        if (currentView.type !== 'topic') return

        chatInput.disabled = true
        const userBubble = document.createElement('div')
        userBubble.className = 'chat-row chat-row--user'
        userBubble.innerHTML = `<div class="chat-bubble chat-bubble--user">${esc(text)}</div>`
        messagesEl.appendChild(userBubble)
        scrollToBottom()

        chatInput.value = ''
        chatInput.dispatchEvent(new Event('input', { bubbles: true }))

        try {
            await submitHandler.submit(text, currentView)
        } catch {
            await appendAiBubble(t.somethingWrong)
        }
    }

    // Helper function
    function scrollToBottom(): void {
        scrollAreaEl.scrollTo({ top: scrollAreaEl.scrollHeight, behavior: 'smooth' })
    }

    // Initial render
    renderDiscoveryMenuOptions()
    await renderIdeasList()

    await appendAiBubble(t.ideationIntro)
    await wait(200)
    await appendAiBubble(firstTopic.prompt?.trim() || `What are your thoughts on: "${firstTopic.title}"?`)
}



