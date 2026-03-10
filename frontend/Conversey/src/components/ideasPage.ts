import type { RouteParams } from '../utils/router.ts'
import { getProject } from '../services/projectService.ts'
import { getWorkspaceByName } from '../services/workspaceService.ts'
import { getIdeasContext, submitIdea } from '../services/ideaService.ts'
import type { Idea, IdeaTopic } from '../models/idea.ts'

type ActiveView = { type: 'topic'; topicId: number } | { type: 'my-ideas' }

function formatOrganizationName(organizationSlug: string): string {
    return organizationSlug
        .split('-')
        .map((part) => (part.length <= 3 ? part.toUpperCase() : `${part.charAt(0).toUpperCase()}${part.slice(1)}`))
        .join(' ')
}

function getRandomTopic(topics: IdeaTopic[]): IdeaTopic | null {
    if (topics.length === 0) return null
    const index = Math.floor(Math.random() * topics.length)
    return topics[index]
}

export async function renderIdeasPage(container: HTMLElement, params: RouteParams): Promise<void> {
    const project = await getProject(params.organizationSlug, params.projectSlug)
    const context = await getIdeasContext(project.id)

    let organizationName = formatOrganizationName(project.organizationSlug)
    try {
        const workspace = await getWorkspaceByName(organizationName)
        if (workspace) organizationName = workspace.name
    } catch {
        // Keep formatted organization fallback.
    }

    const topics = context.topics
    const allIdeas = [...context.ideas]
    const defaultTopic = getRandomTopic(topics)

    let activeView: ActiveView = defaultTopic ? { type: 'topic', topicId: defaultTopic.id } : { type: 'my-ideas' }
    let activeIdeaIndex = 0
    let isTopicMenuOpen = false
    let visibleIdeasCache: Idea[] = []
    let listSyncFrame: number | null = null
    let focusTurnTimeout: number | null = null
    let listScrollUnlockTimeout: number | null = null
    let isProgrammaticListScroll = false

    container.innerHTML = `
        <div class="ideas-shell">
            <div class="survey-topbar">
                <div class="survey-topbar-left">
                    <div class="survey-topbar-logo"><img src="/Conversey_logo.png" alt="Conversey" /></div>
                    <div class="survey-topbar-logo-title">CONVERSEY</div>
                </div>
                <div class="survey-topbar-brand">
                    <div class="survey-topbar-logo-badge">AXA</div>
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
                <div id="idea-panel-post" class="idea-panel-post">
                    <div id="idea-panel-badges" class="idea-panel-badges"></div>
                    <p id="idea-panel-text" class="idea-panel-text"></p>
                    <button id="idea-panel-emoji" class="idea-panel-emoji-btn" type="button" title="React with emoji (coming soon)">
                        <span aria-hidden="true">＋</span>
                        <span aria-hidden="true">😊</span>
                    </button>
                </div>
                <div id="idea-panel-comments" class="idea-panel-comments"></div>
            </div>
            <div class="idea-panel-footer">
                <textarea id="idea-panel-input" class="idea-panel-input" placeholder="Write a comment…" rows="2"></textarea>
                <button id="idea-panel-send" class="idea-panel-send" type="button" disabled>Post</button>
            </div>
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

    // Panel elements
    const panelBackdrop = container.querySelector<HTMLDivElement>('#idea-panel-backdrop')!
    const panel = container.querySelector<HTMLDivElement>('#idea-panel')!
    const panelClose = container.querySelector<HTMLButtonElement>('#idea-panel-close')!
    const panelPinned = container.querySelector<HTMLDivElement>('#idea-panel-pinned')!
    const panelBadges = container.querySelector<HTMLDivElement>('#idea-panel-badges')!
    const panelText = container.querySelector<HTMLParagraphElement>('#idea-panel-text')!
    const panelComments = container.querySelector<HTMLDivElement>('#idea-panel-comments')!
    const panelInput = container.querySelector<HTMLTextAreaElement>('#idea-panel-input')!
    const panelSend = container.querySelector<HTMLButtonElement>('#idea-panel-send')!

    // Per-idea in-memory comment store (keyed by idea id)
    const commentStore = new Map<number, { author: 'self' | 'other'; text: string }[]>()

    function getTopicById(topicId: number): IdeaTopic | undefined {
        return topics.find((topic) => topic.id === topicId)
    }

    function getVisibleIdeas(): Idea[] {
        if (activeView.type === 'my-ideas') {
            return allIdeas.filter((idea) => idea.authorType === 'self')
        }

        const topicId = activeView.topicId
        return allIdeas.filter((idea) => idea.topicId === topicId)
    }

    function getActiveLabel(): string {
        if (activeView.type === 'my-ideas') return 'My ideas'
        const topic = getTopicById(activeView.topicId)
        return topic ? topic.title : 'Select a topic'
    }

    function isSameActiveOption(topicId: number): boolean {
        return activeView.type === 'topic' && activeView.topicId === topicId
    }

    function renderTopicMenu(): void {
        topicMenu.innerHTML = ''

        const myIdeasBtn = document.createElement('button')
        myIdeasBtn.type = 'button'
        myIdeasBtn.className = 'ideas-topic-option ideas-topic-option--my-ideas'
        if (activeView.type === 'my-ideas') myIdeasBtn.classList.add('active')
        myIdeasBtn.textContent = 'My ideas'
        myIdeasBtn.addEventListener('click', () => {
            activeView = { type: 'my-ideas' }
            activeIdeaIndex = 0
            closeTopicMenu()
            render()
        })
        topicMenu.appendChild(myIdeasBtn)

        topics.forEach((topic) => {
            const btn = document.createElement('button')
            btn.type = 'button'
            btn.className = 'ideas-topic-option'
            if (isSameActiveOption(topic.id)) {
                btn.classList.add('active')
            }
            btn.textContent = topic.title
            btn.addEventListener('click', () => {
                activeView = { type: 'topic', topicId: topic.id }
                activeIdeaIndex = 0
                closeTopicMenu()
                render()
            })
            topicMenu.appendChild(btn)
        })
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

    function renderIdeasList(ideas: Idea[]): void {
        list.innerHTML = ''

        if (ideas.length === 0) {
            list.innerHTML = `<p class="ideas-empty">${activeView.type === 'my-ideas' ? 'You have not submitted any ideas yet.' : 'No ideas yet for this view.'}</p>`
            upBtn.disabled = true
            downBtn.disabled = true
            return
        }

        ideas.forEach((idea, index) => {
            const card = document.createElement('article')
            card.className = 'ideas-card'

            if (activeView.type === 'my-ideas') {
                card.classList.add('ideas-card--my-idea')

                const topicLabel = document.createElement('p')
                topicLabel.className = 'ideas-card-topic'
                topicLabel.textContent = getTopicById(idea.topicId)?.title ?? 'Unknown topic'

                const body = document.createElement('p')
                body.className = 'ideas-card-body'
                body.textContent = idea.body

                card.append(topicLabel, body)
            } else {
                if (idea.authorType === 'self') {
                    const yoursBadge = document.createElement('span')
                    yoursBadge.className = 'ideas-card-yours-badge'
                    yoursBadge.textContent = 'Your idea'
                    card.appendChild(yoursBadge)
                }

                const body = document.createElement('p')
                body.className = 'ideas-card-body'
                body.textContent = idea.body
                card.appendChild(body)
            }

            card.setAttribute('data-idea-index', String(index))
            list.appendChild(card)
        })

        setActiveIdea(activeIdeaIndex, false)
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
            // Reflow lets the animation replay on repeated selections.
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

        // Keep scroll-sync paused until smooth scrolling settles.
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

    function renderComposer(): void {
        const isMyIdeasView = activeView.type === 'my-ideas'
        const topic = activeView.type === 'topic' ? getTopicById(activeView.topicId) : undefined

        ideasGrid.classList.toggle('ideas-grid--my-ideas', isMyIdeasView)
        ideasCompose.hidden = isMyIdeasView

        if (!topic) {
            composeTopic.textContent = 'Current view: My ideas'
            prompt.textContent = 'Viewing all your ideas. Pick a topic to submit a new one.'
            textarea.value = ''
            textarea.disabled = true
            submitBtn.disabled = true
            magicBtn.disabled = true
            speakBtn.disabled = true
            return
        }

        composeTopic.textContent = `Topic question: ${topic.title}`
        prompt.textContent = topic.prompt
        textarea.disabled = false
        submitBtn.disabled = textarea.value.trim().length === 0
        magicBtn.disabled = false
        speakBtn.disabled = false
    }

    function render(): void {
        topicTriggerValue.textContent = getActiveLabel()
        listTitle.textContent = activeView.type === 'my-ideas' ? 'Your ideas by topic' : 'Community ideas on this topic'

        renderTopicMenu()
        visibleIdeasCache = getVisibleIdeas()
        renderIdeasList(visibleIdeasCache)
        renderComposer()
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

    textarea.addEventListener('input', () => {
        submitBtn.disabled = textarea.value.trim().length === 0 || activeView.type !== 'topic'
    })

    submitBtn.addEventListener('click', async () => {
        if (activeView.type !== 'topic') return

        const body = textarea.value.trim()
        if (body.length === 0) return

        submitBtn.disabled = true
        submitBtn.textContent = 'Saving...'

        const createdIdea = await submitIdea({
            projectId: project.id,
            topicId: activeView.topicId,
            body,
            authorType: 'self',
        })

        allIdeas.unshift(createdIdea)
        textarea.value = ''
        submitBtn.textContent = 'Submit Idea'
        render()
    })

    list.addEventListener('scroll', updateActiveIdeaFromScroll, { passive: true })

    // ── Panel ────────────────────────────────────────────────────────────────────

    function renderPanel(idea: Idea): void {
        panelBadges.innerHTML = ''
        if (idea.authorType === 'self') {
            const b = document.createElement('span')
            b.className = 'ideas-card-yours-badge'
            b.textContent = 'Your idea'
            panelBadges.appendChild(b)
        }
        panelText.textContent = idea.body

        if (idea.authorType === 'self') {
            panelPinned.hidden = false
            panelPinned.innerHTML = `
                <span class="idea-panel-pinned-label">📌 Author's note</span>
                <p class="idea-panel-pinned-text">This is your original idea. Others can comment and react below.</p>
            `
        } else {
            panelPinned.hidden = true
            panelPinned.innerHTML = ''
        }

        const comments = commentStore.get(idea.id) ?? []
        panelComments.innerHTML = ''

        if (comments.length === 0) {
            panelComments.innerHTML = `<p class="idea-panel-no-comments">No comments yet. Be the first!</p>`
        } else {
            const isOwnIdea = idea.authorType === 'self'
            const pinnedComments = isOwnIdea ? comments.filter((c) => c.author === 'self') : []
            const regularComments = isOwnIdea ? comments.filter((c) => c.author !== 'self') : comments

            if (pinnedComments.length > 0) {
                const pinnedLabel = document.createElement('p')
                pinnedLabel.className = 'idea-panel-comments-label'
                pinnedLabel.textContent = 'Pinned author comments'
                panelComments.appendChild(pinnedLabel)

                pinnedComments.forEach((c) => {
                    const el = document.createElement('div')
                    el.className = 'idea-panel-comment idea-panel-comment--self idea-panel-comment--pinned'
                    el.textContent = c.text
                    panelComments.appendChild(el)
                })
            }

            regularComments.forEach((c) => {
                const el = document.createElement('div')
                el.className = `idea-panel-comment${c.author === 'self' ? ' idea-panel-comment--self' : ''}`
                el.textContent = c.text
                panelComments.appendChild(el)
            })
        }

        panelInput.value = ''
        panelSend.disabled = true
    }

    function closePanel(): void {
        panel.classList.remove('open')
        panelBackdrop.classList.remove('open')
        panel.addEventListener('transitionend', () => {
            panel.hidden = true
            panelBackdrop.hidden = true
        }, { once: true })
    }

    panelClose.addEventListener('click', closePanel)
    panelBackdrop.addEventListener('click', closePanel)

    panelInput.addEventListener('input', () => {
        panelSend.disabled = panelInput.value.trim().length === 0
    })

    panelSend.addEventListener('click', () => {
        const text = panelInput.value.trim()
        if (text.length === 0) return

        const idea = visibleIdeasCache[activeIdeaIndex]
        if (!idea) return

        const existing = commentStore.get(idea.id) ?? []
        commentStore.set(idea.id, [...existing, { author: 'self', text }])
        renderPanel(idea)
    })

    // ── Card click ───────────────────────────────────────────────────────────────

    list.addEventListener('click', (event) => {
        const target = event.target as HTMLElement
        const card = target.closest<HTMLElement>('.ideas-card')
        if (!card) return

        const index = Number(card.getAttribute('data-idea-index'))
        if (!Number.isFinite(index)) return

        if (index === activeIdeaIndex) {
            if (!visibleIdeasCache[index]) return
            renderPanel(visibleIdeasCache[index])
            panel.hidden = false
            panelBackdrop.hidden = false
            requestAnimationFrame(() => {
                panel.classList.add('open')
                panelBackdrop.classList.add('open')
            })
            panelInput.focus()
        } else {
            setActiveIdea(index, true)
        }
    })

    upBtn.addEventListener('click', () => {
        if (visibleIdeasCache.length === 0 || activeIdeaIndex <= 0) return
        setActiveIdea(activeIdeaIndex - 1, true)
    })

    downBtn.addEventListener('click', () => {
        if (visibleIdeasCache.length === 0 || activeIdeaIndex >= visibleIdeasCache.length - 1) return
        setActiveIdea(activeIdeaIndex + 1, true)
    })

    render()
}

