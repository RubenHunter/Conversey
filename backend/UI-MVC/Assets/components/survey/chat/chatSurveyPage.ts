import '../../../styles/pages/chat-survey.css'
import '../../../styles/pages/ideas.css'
import type { Project } from '../../../models/project'
import type { ProjectContext } from '../../../main'
import { getQuestions, submitAnswers } from '../../../services/surveyService'
import { clearSurveyProgress, loadSurveyProgress, saveSurveyProgress } from '../../../services/surveyProgressService'
import { QuestionType } from '../../../models/question'
import type { Question } from '../../../models/question'
import type { ResponseAnswer } from '../../../models/response'
import type { QuestionAnswer, QuestionComponent } from '../singleChoiceQuestion'
import { renderSingleChoiceQuestion } from '../singleChoiceQuestion'
import { renderMultipleChoiceQuestion } from '../multipleChoiceQuestion'
import { renderOpenTextQuestion } from '../openTextQuestion'
import { renderScaleQuestion } from '../scaleQuestion'
import { renderSurveyHeader, createSurveyHeaderController } from '../surveyHeader'
import {
    getIdeasContext,
    getDiscoveredIdeasForTopic,
    IDEA_DISCOVERY_MAX_RESULTS,
    getOrCreateProjectScopedYouthId,
    saveYouthContactEmail,
    type IdeaDiscoveryCategory,
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
import { createIdeasListController } from '../../ideas/ideasListController'
import { createSafetyReviewDialogController } from '../../ideas/safetyReviewDialog'
import { createIdeaPanelController } from '../../ideas/ideaPanel'
import { createIdeasSubmitHandler } from '../../ideas/ideasSubmitHandler'
import { createFirstIdeaContactDialogController } from '../../ideas/firstIdeaContactDialog'
import { createTopicModalController } from '../../ideas/topicModal'
import type { Idea, IdeaTopic } from '../../../models/idea'
import type { ActiveView } from '../../ideas/types'
import { getSurveyStrings } from '../../../i18n/survey'
import { formatAnswerForDisplay, hasAnswer, wait, esc } from './chatHelpers'
import {
    AI_AVATAR,
    CHECKMARK_SVG,
    IDEATION_MODALS_HTML,
    SPEAKER_SVG,
    renderChatShellTemplate,
} from './chatTemplates'

interface OpenTextState {
    questionIndex: number
    messages: string[]
    floatingConfirmRow: HTMLElement | null
}

type DiscoveryBadgeType = 'similar' | 'different'
type DiscoveryMode = 'all' | 'similar' | 'different'

interface DiscoveryFeed {
    ideas: Idea[]
    badgesByIdeaId: ReadonlyMap<number, DiscoveryBadgeType>
}

function createDiscoveryFeed(ideas: Idea[], badgesByIdeaId: ReadonlyMap<number, DiscoveryBadgeType>): DiscoveryFeed {
    return {
        ideas,
        badgesByIdeaId,
    }
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


export async function renderChatSurveyPage(
    container: HTMLElement,
    params: ProjectContext,
    project: Project,
): Promise<void> {
    const t = getSurveyStrings()
    const projectSlugKey = params.projectSlug
    const completedKey = `survey-completed-${projectSlugKey}`
    const startInIdeasMode = localStorage.getItem(completedKey) === 'true'

    if (startInIdeasMode) {
        clearSurveyProgress(projectSlugKey)
    }

    const questions = await getQuestions(params.organizationSlug, params.projectSlug)
    const orgName = project.organizationName?.trim() || project.organizationSlug
    const headerHTML = renderSurveyHeader({ organizationName: orgName, organizationSlug: project.organizationSlug })

    container.innerHTML = renderChatShellTemplate({
        projectTitle: project.title,
        questionsCount: questions.length,
        headerHTML,
        strings: { selectAbove: t.selectAbove, magicMode: t.magicMode },
    })

    const chatShell = container.querySelector<HTMLDivElement>('#chat-shell')!
    const scrollAreaEl = container.querySelector<HTMLDivElement>('#chat-scroll-area')!
    const messagesEl = container.querySelector<HTMLDivElement>('#chat-messages')!
    const headerController = createSurveyHeaderController({ root: container })
    const chatInput = container.querySelector<HTMLTextAreaElement>('#chat-input')!
    const magicBtn = container.querySelector<HTMLButtonElement>('#chat-magic-btn')!
    const confirmInlineBtn = container.querySelector<HTMLButtonElement>('#chat-confirm-inline-btn')!
    const sendBtn = container.querySelector<HTMLButtonElement>('#chat-send-btn')!
    const micIcon = sendBtn.querySelector<SVGElement>('.chat-mic-icon')!
    const sendIcon = sendBtn.querySelector<SVGElement>('.chat-send-icon')!

    const components: QuestionComponent[] = questions.map((q, i) =>
        q.type === QuestionType.SingleChoice
            ? renderSingleChoiceQuestion(q, i)
            : q.type === QuestionType.MultipleChoice
            ? renderMultipleChoiceQuestion(q, i)
            : q.type === QuestionType.Scale
            ? renderScaleQuestion(q, i)
            : renderOpenTextQuestion(q, i),
    )

    const answeredState = new Array<boolean>(questions.length).fill(false)
    const openTextDraftsByQuestionId = new Map<number, string[]>()
    let confirmedUpToIndex = 0
    let openTextState: OpenTextState | null = null
    let activeConfirmIndex: number | null = null
    let activeSendHandler: (() => void | Promise<void>) | null = null

    // ===== Progress =====
    function updateProgress(): void {
        const count = answeredState.filter(Boolean).length
        headerController.updateProgress(count, questions.length)
    }

    function persistProgress(nextConfirmedUpToIndex: number = confirmedUpToIndex): void {
        confirmedUpToIndex = nextConfirmedUpToIndex
        const byId = new Map<number, QuestionAnswer>(
            questions.map((q, i) => [q.id, components[i].getAnswer()] as const),
        )
        saveSurveyProgress(projectSlugKey, questions, confirmedUpToIndex, byId, {
            openTextDraftsByQuestionId,
        })
    }

    // ===== Scroll =====
    function scrollToBottom(): void {
        scrollAreaEl.scrollTo({ top: scrollAreaEl.scrollHeight, behavior: 'smooth' })
    }

    // ===== Typing indicator =====
    function showTyping(): void {
        if (messagesEl.querySelector('#chat-typing-indicator')) return
        const row = document.createElement('div')
        row.id = 'chat-typing-indicator'
        row.className = 'chat-row chat-row--ai'
        row.innerHTML = `
            <div class="chat-avatar">${AI_AVATAR}</div>
            <div class="chat-bubble-group">
                <div class="chat-bubble chat-bubble--ai chat-bubble--typing">
                    <span class="chat-dot"></span>
                    <span class="chat-dot"></span>
                    <span class="chat-dot"></span>
                </div>
            </div>`
        messagesEl.appendChild(row)
        scrollToBottom()
    }

    function hideTyping(): void {
        messagesEl.querySelector('#chat-typing-indicator')?.remove()
    }

    // ===== AI bubble =====
    interface AiBubbleOptions {
        animated?: boolean
        bubbleClass?: string
        questionNum?: number
        required?: boolean
    }

    async function appendAiBubble(text: string, options: AiBubbleOptions = {}): Promise<void> {
        const { animated = true, bubbleClass, questionNum, required } = options
        if (animated) {
            showTyping()
            await wait(650 + Math.min(text.length * 7, 850))
            hideTyping()
        }

        const row = document.createElement('div')
        row.className = 'chat-row chat-row--ai'

        const bubbleEl = document.createElement('div')
        const extraClass = bubbleClass ? ` ${bubbleClass}` : ''
        bubbleEl.className = `chat-bubble chat-bubble--ai${extraClass}`

        if (questionNum != null) {
            const numEl = document.createElement('span')
            numEl.className = 'chat-question-num'
            numEl.textContent = `${questionNum}.`
            bubbleEl.appendChild(numEl)
            bubbleEl.appendChild(document.createTextNode(' '))
        }

        bubbleEl.appendChild(document.createTextNode(text))

        const speakerBtn = document.createElement('button')
        speakerBtn.className = 'chat-speaker-btn'
        speakerBtn.type = 'button'
        speakerBtn.disabled = true
        speakerBtn.setAttribute('aria-label', t.readAloud)
        speakerBtn.innerHTML = SPEAKER_SVG

        let bubbleOrWrapper: HTMLElement = bubbleEl

        if (required) {
            const wrapper = document.createElement('div')
            wrapper.className = 'chat-bubble-wrapper'
            wrapper.appendChild(bubbleEl)
            const badge = document.createElement('span')
            badge.className = 'chat-required-float'
            badge.textContent = t.requiredLabel
            wrapper.appendChild(badge)
            bubbleOrWrapper = wrapper
        }

        const group = document.createElement('div')
        group.className = 'chat-bubble-group'
        group.appendChild(bubbleOrWrapper)
        group.appendChild(speakerBtn)

        const avatarDiv = document.createElement('div')
        avatarDiv.className = 'chat-avatar'
        avatarDiv.innerHTML = AI_AVATAR

        row.appendChild(avatarDiv)
        row.appendChild(group)
        messagesEl.appendChild(row)
        scrollToBottom()
    }

    // ===== User bubble =====
    function appendUserBubble(text: string): void {
        const row = document.createElement('div')
        row.className = 'chat-row chat-row--user'
        row.innerHTML = `<div class="chat-bubble chat-bubble--user">${esc(text)}</div>`
        messagesEl.appendChild(row)
        scrollToBottom()
    }

    // ===== Input icon =====
    function updateSendIcon(): void {
        const hasText = chatInput.value.trim().length > 0
        micIcon.classList.toggle('chat-icon-hidden', hasText)
        sendIcon.classList.toggle('chat-icon-hidden', !hasText)
    }

    // ===== Inline confirm =====
    function showInlineConfirm(index: number): void {
        activeConfirmIndex = index
        confirmInlineBtn.hidden = false
    }

    function hideInlineConfirm(): void {
        activeConfirmIndex = null
        confirmInlineBtn.hidden = true
    }

    // ===== Open text: multi-message =====
    function createFloatingConfirmRow(index: number): HTMLElement {
        const row = document.createElement('div')
        row.className = 'chat-confirm-row'
        row.setAttribute('data-confirm-for', String(index))
        row.innerHTML = `
            <div class="chat-confirm-line"></div>
            <button class="chat-confirm-btn" type="button" aria-label="Confirm answer and continue">
                ${CHECKMARK_SVG}
            </button>
            <div class="chat-confirm-line"></div>`
        row.querySelector<HTMLButtonElement>('.chat-confirm-btn')!.addEventListener('click', () => {
            void confirmOpenText(index)
        })
        return row
    }

    function sendOpenTextMessage(): void {
        if (!openTextState) return
        const text = chatInput.value.trim()
        if (!text) return

        openTextState.messages.push(text)
        chatInput.value = ''
        chatInput.style.height = 'auto'
        updateSendIcon()

        appendUserBubble(text)

        openTextState.floatingConfirmRow?.remove()
        const newRow = createFloatingConfirmRow(openTextState.questionIndex)
        messagesEl.appendChild(newRow)
        openTextState.floatingConfirmRow = newRow
        scrollToBottom()

        const bundled = openTextState.messages.join('\n\n')
        components[openTextState.questionIndex].setAnswer(bundled)
        answeredState[openTextState.questionIndex] = true
        const questionId = questions[openTextState.questionIndex].id
        openTextDraftsByQuestionId.set(questionId, [...openTextState.messages])
        updateProgress()
        persistProgress()
    }

    async function confirmOpenText(index: number): Promise<void> {
        if (!openTextState || openTextState.questionIndex !== index) return

        const inputText = chatInput.value.trim()
        if (inputText) {
            sendOpenTextMessage()
        }

        if (openTextState.messages.length === 0 && questions[index].isRequired) {
            await appendAiBubble(t.pleaseFill, { animated: false })
            return
        }

        const bundled = openTextState.messages.join('\n\n')
        components[index].setAnswer(bundled || null)
        openTextDraftsByQuestionId.delete(questions[index].id)

        openTextState.floatingConfirmRow?.classList.add('chat-confirm-row--confirmed')
        answeredState[index] = bundled.trim().length > 0
        updateProgress()
        openTextState = null
        hideInlineConfirm()
        deactivateInput()

        persistProgress(index + 1)

        await wait(350)
        if (index < questions.length - 1) {
            await revealQuestion(index + 1)
        } else {
            await showSubmitSection()
        }
    }

    // ===== Input state management =====
    function activateOpenTextInput(questionIndex: number): void {
        const questionId = questions[questionIndex].id
        const restoredMessages = openTextDraftsByQuestionId.get(questionId) ?? []
        openTextState = { questionIndex, messages: [...restoredMessages], floatingConfirmRow: null }

        if (restoredMessages.length > 0) {
            restoredMessages.forEach((message) => appendUserBubble(message))
            const restoredRow = createFloatingConfirmRow(questionIndex)
            messagesEl.appendChild(restoredRow)
            openTextState.floatingConfirmRow = restoredRow
            components[questionIndex].setAnswer(restoredMessages.join('\n\n'))
            answeredState[questionIndex] = true
            updateProgress()
        }

        chatInput.disabled = false
        chatInput.placeholder = questions[questionIndex].hint?.trim() || t.typeHere
        chatInput.value = ''
        chatInput.style.height = 'auto'
        sendBtn.disabled = false
        magicBtn.hidden = false
        showInlineConfirm(questionIndex)
        activeSendHandler = sendOpenTextMessage
        updateSendIcon()
        setTimeout(() => chatInput.focus(), 50)
    }

    function activateIdeaInput(placeholder: string, handler: () => void | Promise<void>): void {
        chatInput.disabled = false
        chatInput.placeholder = placeholder
        chatInput.value = ''
        chatInput.style.height = 'auto'
        sendBtn.disabled = false
        magicBtn.hidden = false
        hideInlineConfirm()
        activeSendHandler = handler
        updateSendIcon()
        setTimeout(() => chatInput.focus(), 50)
    }

    function deactivateInput(placeholder = ''): void {
        activeSendHandler = null
        chatInput.disabled = true
        chatInput.value = ''
        chatInput.style.height = 'auto'
        chatInput.placeholder = placeholder || t.selectAbove
        sendBtn.disabled = true
        magicBtn.hidden = true
        updateSendIcon()
    }

    chatInput.addEventListener('input', () => {
        updateSendIcon()
        chatInput.style.height = 'auto'
        chatInput.style.height = `${Math.min(chatInput.scrollHeight, 120)}px`
    })

    sendBtn.addEventListener('click', () => {
        void activeSendHandler?.()
    })

    chatInput.addEventListener('keydown', (e) => {
        if (e.key === 'Enter' && !e.shiftKey) {
            e.preventDefault()
            void activeSendHandler?.()
        }
    })

    confirmInlineBtn.addEventListener('click', () => {
        if (openTextState !== null) {
            void confirmOpenText(openTextState.questionIndex)
        } else if (activeConfirmIndex !== null) {
            void confirmQuestion(activeConfirmIndex)
        }
    })

    // ===== Confirm question (non-open-text) =====
    async function confirmQuestion(index: number): Promise<void> {
        if (!components[index].validate()) {
            await appendAiBubble(t.pleaseFillChoice)
            return
        }

        const confirmRow = messagesEl.querySelector<HTMLElement>(`[data-confirm-for="${index}"]`)
        confirmRow?.classList.add('chat-confirm-row--confirmed')

        answeredState[index] = true
        updateProgress()
        hideInlineConfirm()

        persistProgress(index + 1)

        await wait(350)
        if (index < questions.length - 1) {
            await revealQuestion(index + 1)
        } else {
            await showSubmitSection()
        }
    }

    // ===== Reveal question =====
    async function revealQuestion(index: number): Promise<void> {
        const q = questions[index]

        await appendAiBubble(q.text, {
            bubbleClass: 'chat-bubble--question-title',
            questionNum: index + 1,
            required: q.isRequired,
        })

        if (q.hint?.trim()) {
            await wait(150)
            await appendAiBubble(q.hint.trim())
        }

        if (q.type === QuestionType.OpenText) {
            const hintRow = document.createElement('div')
            hintRow.className = 'chat-row chat-row--hint'
            hintRow.innerHTML = `
                <div class="chat-avatar chat-avatar--spacer"></div>
                <p class="chat-open-text-hint">${esc(t.typeBelow)}</p>`
            messagesEl.appendChild(hintRow)
            scrollToBottom()
            activateOpenTextInput(index)
            return
        }

        const block = document.createElement('div')
        block.className = 'chat-question-block'
        block.setAttribute('data-question-index', String(index))

        const answerRegion = document.createElement('div')
        answerRegion.className = 'chat-answer-region'

        const el = components[index].getElement()
        el.classList.add('chat-question-component')
        answerRegion.appendChild(el)

        components[index].onAnswer(() => {
            answeredState[index] = hasAnswer(components[index].getAnswer())
            updateProgress()
        })

        const confirmRow = document.createElement('div')
        confirmRow.className = 'chat-confirm-row'
        confirmRow.setAttribute('data-confirm-for', String(index))
        confirmRow.innerHTML = `
            <div class="chat-confirm-line"></div>
            <button class="chat-confirm-btn" type="button" aria-label="Confirm answer and continue">
                ${CHECKMARK_SVG}
            </button>
            <div class="chat-confirm-line"></div>`
        confirmRow.querySelector<HTMLButtonElement>('.chat-confirm-btn')!.addEventListener('click', () => {
            void confirmQuestion(index)
        })

        block.appendChild(answerRegion)
        block.appendChild(confirmRow)
        messagesEl.appendChild(block)
        scrollToBottom()

        deactivateInput()
        showInlineConfirm(index)
    }

    // ===== Submit section =====
    async function showSubmitSection(): Promise<void> {
        await appendAiBubble(t.allDone)

        const submitRow = document.createElement('div')
        submitRow.className = 'chat-submit-row'
        submitRow.innerHTML = `<button class="chat-submit-btn" id="chat-survey-submit" type="button">${esc(t.submitSurvey)}</button>`
        messagesEl.appendChild(submitRow)
        scrollToBottom()

        submitRow.querySelector<HTMLButtonElement>('#chat-survey-submit')!.addEventListener('click', async () => {
            const btn = submitRow.querySelector<HTMLButtonElement>('#chat-survey-submit')!
            btn.disabled = true
            btn.textContent = t.submitting

            const answers = questions.reduce<ResponseAnswer[]>((acc, q, i) => {
                const answer = components[i].getAnswer()
                if (q.type === QuestionType.SingleChoice) {
                    const id = answer as number
                    if (id != null) {
                        acc.push({ questionId: q.id, selectedOptionId: id, value: id })
                    }
                    return acc
                }
                if (q.type === QuestionType.MultipleChoice) {
                    const ids = Array.isArray(answer) ? answer : []
                    ids.forEach((id) => {
                        acc.push({ questionId: q.id, selectedOptionId: id, value: id })
                    })
                    return acc
                }
                if (q.type === QuestionType.Scale) {
                    const val = answer as number
                    if (val != null) {
                        acc.push({ questionId: q.id, selectedOptionId: val, value: val })
                    }
                    return acc
                }
                const text = answer as string
                if (text?.trim()) {
                    acc.push({ questionId: q.id, openTextValue: text, value: text })
                }
                return acc
            }, [])

            try {
                await submitAnswers(params.organizationSlug, params.projectSlug, {
                    projectId: params.projectSlug,
                    answers,
                })
                localStorage.setItem(completedKey, 'true')
                clearSurveyProgress(projectSlugKey)
                submitRow.remove()

                const successRow = document.createElement('div')
                successRow.className = 'chat-submit-success'
                successRow.innerHTML = `
                    <div class="chat-submit-success-icon">${CHECKMARK_SVG}</div>
                    <p class="chat-submit-success-title">${esc(t.submittedTitle)}</p>
                    <p class="chat-submit-success-sub">${esc(t.submittedSub)}</p>`
                messagesEl.appendChild(successRow)
                scrollToBottom()

                await wait(2200)
                await enterIdeasPhase()
            } catch {
                btn.disabled = false
                btn.textContent = t.submitSurvey
                await appendAiBubble(t.somethingWrong)
            }
        })
    }

    // ===== Lock survey history =====
    function lockSurveyHistory(): void {
        Array.from(messagesEl.children).forEach((el) => {
            if (el instanceof HTMLElement) {
                el.classList.add('chat-survey-history-locked')
            }
        })
    }

    // ===== Offensive content notification =====
    function showOffensiveNotice(): void {
        if (chatShell.querySelector('#chat-offensive-notice')) return
        const notice = document.createElement('div')
        notice.id = 'chat-offensive-notice'
        notice.className = 'chat-offensive-notice'
        notice.innerHTML = `
            <svg viewBox="0 0 24 24" fill="currentColor" aria-hidden="true">
                <path d="M1 21h22L12 2 1 21zm12-3h-2v-2h2v2zm0-4h-2v-4h2v4z"/>
            </svg>
            <span>${esc(t.offensiveLanguage)}</span>`
        const inputWrap = chatShell.querySelector('.chat-input-wrap')
        chatShell.insertBefore(notice, inputWrap)
    }

    function hideOffensiveNotice(): void {
        chatShell.querySelector('#chat-offensive-notice')?.remove()
    }

    // ===== Ideation phase =====
    async function enterIdeasPhase(): Promise<void> {
        lockSurveyHistory()

        // Keep organization branding anchored at the top of the full page in ideation mode.
        const topbar = scrollAreaEl.querySelector<HTMLElement>('.survey-topbar')
        if (topbar && topbar.parentElement !== chatShell) {
            chatShell.insertBefore(topbar, chatShell.firstChild)
        }

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

        let activeView: ActiveView = { type: 'topic', topicId: firstTopic.id }
        let discoveryMode: DiscoveryMode = 'all'
        let selectedSemanticCategory: string | null = null
        let showPostPreviewPair = false
        let latestSubmittedIdea: Idea | null = null
        let discoveryRequestToken = 0
        let discoveryBadgeByIdeaId: ReadonlyMap<number, DiscoveryBadgeType> = new Map()
        let visibleIdeasCache: Idea[] = []
        const discoveryCache = new Map<string, DiscoveryFeed>()

        const firstIdeaContactStorageKey = `ideas-contact-consent:${params.organizationSlug}:${params.projectSlug}`
        const firstIdeaContactDialog = createFirstIdeaContactDialogController({
            root: container,
            storageKey: firstIdeaContactStorageKey,
        })

        const ideasArea = document.createElement('div')
        ideasArea.className = 'chat-ideas-area'
        ideasArea.innerHTML = `
            <div class="chat-ideas-top-row">
                <button id="ideas-topic-trigger" class="ideas-compose-topic-button" aria-haspopup="dialog" aria-expanded="false" aria-controls="topic-modal" aria-label="Select topic">
                    <span class="ideas-compose-topic-text">
                        <span class="ideas-compose-topic-kicker">Topic:</span>
                        <span id="ideas-topic-trigger-value" class="ideas-compose-topic-value"></span>
                        <span class="ideas-compose-topic-chevron" aria-hidden="true">▾</span>
                    </span>
                </button>
                <button id="ideas-topic-trigger-floating" class="ideas-compose-topic-button ideas-compose-topic-button--floating" aria-haspopup="dialog" aria-expanded="false" aria-controls="topic-modal" aria-label="Switch topic" hidden>
                    <span class="ideas-compose-topic-text">
                        <span class="ideas-compose-topic-kicker">Switch to topic</span>
                        <span id="ideas-topic-trigger-floating-value" class="ideas-compose-topic-value"></span>
                        <span class="ideas-compose-topic-chevron" aria-hidden="true">▾</span>
                    </span>
                </button>
            </div>
            <section class="ideas-community" aria-label="Ideas list">
                <div id="ideas-discovery" class="ideas-discovery" hidden>
                    <button id="ideas-discovery-trigger" class="ideas-discovery-trigger" type="button" aria-haspopup="menu" aria-expanded="false">
                        <span id="ideas-discovery-label">Explore ideas</span>
                        <span class="ideas-discovery-chevron" aria-hidden="true">▾</span>
                    </button>
                    <div id="ideas-discovery-menu" class="ideas-discovery-menu" role="menu" hidden></div>
                </div>
                <div class="ideas-list" id="chat-ideas-list" aria-live="polite"></div>
            </section>`

        chatShell.classList.add('chat-shell--ideas')
        chatShell.insertBefore(ideasArea, scrollAreaEl)

        const ideasListEl = container.querySelector<HTMLDivElement>('#chat-ideas-list')!
        const topicTrigger = container.querySelector<HTMLButtonElement>('#ideas-topic-trigger')!
        const topicTriggerValue = container.querySelector<HTMLSpanElement>('#ideas-topic-trigger-value')!
        const topicFloatingTrigger = container.querySelector<HTMLButtonElement>('#ideas-topic-trigger-floating')!
        const topicFloatingTriggerValue = container.querySelector<HTMLSpanElement>('#ideas-topic-trigger-floating-value')!
        const discoveryRoot = container.querySelector<HTMLDivElement>('#ideas-discovery')!
        const discoveryTrigger = container.querySelector<HTMLButtonElement>('#ideas-discovery-trigger')!
        const discoveryLabel = container.querySelector<HTMLSpanElement>('#ideas-discovery-label')!
        const discoveryMenu = container.querySelector<HTMLDivElement>('#ideas-discovery-menu')!
        const flaggedIdeaIds = new Set<number>()

        const safetyDialog = createSafetyReviewDialogController({ root: container })
        const reviewWithSuggestion = async (orig: string, sugg: string) => {
            showOffensiveNotice()
            await wait(1000)
            hideOffensiveNotice()
            return safetyDialog.reviewWithSuggestion(orig, sugg)
        }

        let listController: ReturnType<typeof createIdeasListController> | null = null

        const ideaPanel = createIdeaPanelController({
            root: container,
            reviewBeforePost: (input) => safetyDialog.reviewBeforePost(input),
            reviewWithSuggestion,
            updateIdeaAfterSafetyReview: (idea, text, mark) =>
                updateIdeaAfterSafetyReview(params.organizationSlug, params.projectSlug, idea.topicId, idea.id, text, mark),
            loadResponses: (idea) => getIdeaResponses(params.organizationSlug, params.projectSlug, idea, youthToken),
            submitResponse: (idea, text) => addIdeaResponse(params.organizationSlug, params.projectSlug, idea, youthToken, text),
            updateResponseAfterSafetyReview: (idea, rid, text, mark) =>
                updateIdeaResponseAfterSafetyReview(params.organizationSlug, params.projectSlug, idea, rid, youthToken, text, mark),
            reactToResponse: (idea, rid, emoji) => addResponseReaction(params.organizationSlug, params.projectSlug, idea, rid, youthToken, emoji),
            unreactToResponse: (idea, rid, emoji) => removeResponseReaction(params.organizationSlug, params.projectSlug, idea, rid, youthToken, emoji),
            reactToIdea: (idea, emoji) => addIdeaReaction(params.organizationSlug, params.projectSlug, idea, youthToken, emoji),
            unreactToIdea: (idea, emoji) => removeIdeaReaction(params.organizationSlug, params.projectSlug, idea, youthToken, emoji),
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
                closeDiscoveryMenu()
                void renderIdeasList()
                topicModal.renderTopics(activeView)
                updateTopicLabels()
                if (activeView.type === 'topic') {
                    const topic = topics.find((item) => item.id === activeView.topicId)
                    if (topic) {
                        void appendAiBubble(topic.prompt?.trim() || `What are your thoughts on: "${topic.title}"?`)
                        activateIdeaInput(t.shareIdea, handleIdeaSubmit)
                    }
                } else {
                    deactivateInput(t.selectTopicToShare)
                }
            },
        })

        function hasOwnIdeaInTopic(topicId: number): boolean {
            return allIdeas.some((idea) => idea.authorType === 'self' && idea.topicId === topicId)
        }

        function getTopicSemanticCategories(topicId: number): string[] {
            const categories = new Set<string>()
            allIdeas
                .filter((idea) => idea.topicId === topicId)
                .forEach((idea) => {
                    idea.semanticCategories.forEach((category) => {
                        if (category.trim().length > 0) categories.add(category)
                    })
                })
            return [...categories].sort((a, b) => a.localeCompare(b))
        }

        function createPostPreviewFeed(similarIdeas: Idea[], differentIdeas: Idea[], submittedIdea: Idea | null): DiscoveryFeed {
            const previewIdeas: Idea[] = []
            const previewBadges = new Map<number, DiscoveryBadgeType>()
            const seen = new Set<number>()

            const addIdea = (idea: Idea | null | undefined, badge?: DiscoveryBadgeType): boolean => {
                if (!idea || seen.has(idea.id)) return false
                seen.add(idea.id)
                previewIdeas.push(idea)
                if (badge && previewBadges.size < 2) previewBadges.set(idea.id, badge)
                return true
            }

            addIdea(similarIdeas[0], 'similar')
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

            if (!hasOwnIdeaInTopic(activeView.topicId)) {
                const categories = getTopicSemanticCategories(activeView.topicId)
                const semanticButtons = categories
                    .map((category) => `<button class="ideas-discovery-option" data-semantic-category="${category.replace(/"/g, '&quot;')}" role="menuitem" type="button">${category}</button>`)
                    .join('')
                const categoriesSection = categories.length > 0
                    ? `<hr class="ideas-discovery-separator" role="separator"><p class="ideas-discovery-section-label">Idea categories</p>${semanticButtons}`
                    : ''

                discoveryMenu.innerHTML = `<button class="ideas-discovery-option" data-discovery-mode="all" role="menuitem" type="button">Broad selection</button>${categoriesSection}`
                return
            }

            discoveryMenu.innerHTML = `
                <button class="ideas-discovery-option" data-discovery-mode="similar" role="menuitem" type="button">Similar ideas</button>
                <button class="ideas-discovery-option" data-discovery-mode="different" role="menuitem" type="button">Differing ideas</button>
                <button class="ideas-discovery-option" data-discovery-mode="all" role="menuitem" type="button">All ideas</button>`
        }

        function closeDiscoveryMenu(): void {
            discoveryMenu.hidden = true
            discoveryTrigger.setAttribute('aria-expanded', 'false')
        }

        function updateDiscoveryUi(): void {
            if (activeView.type !== 'topic') {
                discoveryRoot.hidden = true
                closeDiscoveryMenu()
                return
            }

            discoveryRoot.hidden = false
            renderDiscoveryMenuOptions()
            const ownIdeaExists = hasOwnIdeaInTopic(activeView.topicId)
            const labelMap: Record<DiscoveryMode, string> = { all: 'All ideas', similar: 'Similar ideas', different: 'Differing ideas' }
            discoveryLabel.textContent = ownIdeaExists ? labelMap[discoveryMode] : (selectedSemanticCategory ?? 'Broad selection')

            discoveryMenu.querySelectorAll<HTMLButtonElement>('.ideas-discovery-option').forEach((option) => {
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
                return createDiscoveryFeed(allIdeas.filter((idea) => idea.authorType === 'self'), new Map())
            }

            const topicId = activeView.topicId
            const ownIdeaExists = hasOwnIdeaInTopic(topicId)
            const categorySuffix = ownIdeaExists ? 'own' : (selectedSemanticCategory ?? 'broad')
            const cacheSuffix = discoveryMode === 'all' && showPostPreviewPair ? 'preview' : 'full'
            const cacheKey = `${topicId}:${discoveryMode}:${categorySuffix}:${cacheSuffix}`
            const cached = discoveryCache.get(cacheKey)
            if (cached) return cached

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
                        getDiscoveredIdeasForTopic(params.organizationSlug, params.projectSlug, topicId, youthToken, 'similar', IDEA_DISCOVERY_MAX_RESULTS),
                        getDiscoveredIdeasForTopic(params.organizationSlug, params.projectSlug, topicId, youthToken, 'different', IDEA_DISCOVERY_MAX_RESULTS),
                    ])
                    const similarIds = new Set(similarIdeas.map((idea) => idea.id))
                    const oppositeIdeas = rawDifferentIdeas.filter((idea) => !similarIds.has(idea.id))

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
                        discovered = createPostPreviewFeed(similarIdeas, rawDifferentIdeas, submittedIdea)
                    } else {
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
                                addPinned(idea, 'different')
                                break
                            }
                        }
                        const userIdea = latestSubmittedIdea ?? allIdeas.find((idea) => idea.authorType === 'self' && idea.topicId === topicId) ?? null
                        addPinned(userIdea)
                        const broadRemainder = buildBroadFeed(allIdeas.filter((idea) => idea.topicId === topicId)).filter((idea) => !pinnedIds.has(idea.id))
                        discovered = createDiscoveryFeed([...pinnedIdeas, ...broadRemainder], pinnedBadges)
                    }
                } else {
                    const otherMode: IdeaDiscoveryCategory = discoveryMode === 'similar' ? 'different' : 'similar'
                    const [modeIdeas, otherIdeas] = await Promise.all([
                        getDiscoveredIdeasForTopic(params.organizationSlug, params.projectSlug, topicId, youthToken, discoveryMode, IDEA_DISCOVERY_MAX_RESULTS),
                        getDiscoveredIdeasForTopic(params.organizationSlug, params.projectSlug, topicId, youthToken, otherMode, IDEA_DISCOVERY_MAX_RESULTS),
                    ])

                    const similarList = discoveryMode === 'similar' ? modeIdeas : otherIdeas
                    const rawDifferentList = discoveryMode === 'different' ? modeIdeas : otherIdeas
                    const simIds = new Set(similarList.map((idea) => idea.id))
                    const deduplicatedDifferent = rawDifferentList.filter((idea) => !simIds.has(idea.id))

                    const otherCacheKey = `${topicId}:${otherMode}:${categorySuffix}:full`
                    if (!discoveryCache.has(otherCacheKey)) {
                        discoveryCache.set(otherCacheKey, createDiscoveryFeed(discoveryMode === 'similar' ? deduplicatedDifferent : similarList, new Map()))
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

        function getActiveIdeasLabel(view: ActiveView): string {
            if (view.type === 'my-ideas') return 'My ideas'
            const topic = topics.find((item) => item.id === view.topicId)
            return topic ? topic.title : 'Select a topic'
        }

        function updateTopicLabels(): void {
            const label = getActiveIdeasLabel(activeView)
            topicTriggerValue.textContent = label
            topicFloatingTriggerValue.textContent = label
            topicFloatingTrigger.hidden = activeView.type !== 'my-ideas'
        }

        async function renderIdeasList(): Promise<void> {
            const renderToken = ++discoveryRequestToken
            updateTopicLabels()
            updateDiscoveryUi()

            const discoveryFeed = await getVisibleIdeasForCurrentMode()
            if (renderToken !== discoveryRequestToken) return

            visibleIdeasCache = discoveryFeed.ideas
            discoveryBadgeByIdeaId = discoveryFeed.badgesByIdeaId

            if (listController) {
                listController.cleanup()
            }

            if (visibleIdeasCache.length === 0) {
                ideasListEl.innerHTML = `<p class="ideas-empty-state">${esc(t.noIdeas)}</p>`
                listController = null
                return
            }

            listController = createIdeasListController({
                list: ideasListEl,
                ideas: visibleIdeasCache,
                activeView,
                topics,
                flaggedIdeaIds,
                discoveryBadgeByIdeaId,
                onDiscoveryBadgeClick: (badge) => {
                    discoveryMode = badge === 'similar' ? 'similar' : 'different'
                    showPostPreviewPair = false
                    closeDiscoveryMenu()
                    void renderIdeasList()
                },
            })
            listController.setActive(0, false)
            listController.startRotation()
            topicModal.renderTopics(activeView)
        }

        const submitHandler = createIdeasSubmitHandler({
            organizationSlug: params.organizationSlug,
            projectSlug: params.projectSlug,
            projectId: project.id,
            reviewBeforePost: (input) => safetyDialog.reviewBeforePost(input),
            reviewWithSuggestion,
            onIdeaSubmitted: (idea: Idea) => {
                allIdeas.unshift(idea)
                latestSubmittedIdea = idea
                discoveryMode = 'all'
                selectedSemanticCategory = null
                showPostPreviewPair = true
                discoveryCache.clear()
                void renderIdeasList()
                void appendAiBubble(t.ideaShared)

                if (!firstIdeaContactDialog.hasStoredDecision()) {
                    void firstIdeaContactDialog.open().then((choice) => {
                        if (choice?.permissionGranted && choice.email) {
                            void saveYouthContactEmail(params.organizationSlug, params.projectSlug, youthToken, choice.email)
                        }
                    })
                }
            },
        })

        async function handleIdeaSubmit(): Promise<void> {
            const text = chatInput.value.trim()
            if (!text || activeView.type !== 'topic') return

            deactivateInput(t.submitting)
            appendUserBubble(text)

            try {
                await submitHandler.submit(text, activeView)
            } catch {
                await appendAiBubble(t.somethingWrong)
            } finally {
                if (activeView.type === 'topic') {
                    activateIdeaInput(t.shareAnother, handleIdeaSubmit)
                }
            }
        }

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
                discoveryMenu.hidden = false
                discoveryTrigger.setAttribute('aria-expanded', 'true')
            } else {
                closeDiscoveryMenu()
            }
        })

        discoveryMenu.addEventListener('click', (event) => {
            const option = (event.target as HTMLElement).closest<HTMLButtonElement>('.ideas-discovery-option')
            if (!option || activeView.type !== 'topic') return

            const selectedMode = option.dataset.discoveryMode as DiscoveryMode | undefined
            const semanticCategory = option.dataset.semanticCategory

            if (hasOwnIdeaInTopic(activeView.topicId)) {
                if (!selectedMode) return
                discoveryMode = selectedMode
                selectedSemanticCategory = null
            } else {
                discoveryMode = 'all'
                selectedSemanticCategory = semanticCategory ?? null
            }

            showPostPreviewPair = false
            closeDiscoveryMenu()
            void renderIdeasList()
        })

        ideasListEl.addEventListener('scroll', () => {
            listController?.updateFromScroll()
        }, { passive: true })

        ideasListEl.addEventListener('click', (event) => {
            const card = (event.target as HTMLElement).closest<HTMLElement>('.ideas-card')
            if (!card || !listController) return
            const idx = Number(card.getAttribute('data-original-index'))
            if (!Number.isFinite(idx) || idx < 0 || idx >= visibleIdeasCache.length) return
            listController.setActive(idx, true)
            ideaPanel.open(visibleIdeasCache[idx])
        })

        document.addEventListener('click', (event) => {
            if (!(event.target instanceof Node)) return
            if (!discoveryRoot.contains(event.target)) {
                closeDiscoveryMenu()
            }
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
            },
            { once: true },
        )

        await renderIdeasList()
        await appendAiBubble(t.ideationIntro)
        await wait(200)
        await appendAiBubble(firstTopic.prompt?.trim() || `What are your thoughts on: "${firstTopic.title}"?`)
        activateIdeaInput(t.shareIdea, handleIdeaSubmit)
    }

    // ===== Start conversation =====
    if (startInIdeasMode) {
        await enterIdeasPhase()
        return
    }

    const savedProgress = loadSurveyProgress(projectSlugKey, questions)

    if (savedProgress && savedProgress.currentQuestionIndex > 0 && questions.length > 0) {
        const resumeAt = Math.min(savedProgress.currentQuestionIndex, questions.length)
        confirmedUpToIndex = resumeAt
        savedProgress.openTextDraftsByQuestionId.forEach((messages, questionId) => {
            openTextDraftsByQuestionId.set(questionId, [...messages])
        })

        // Restore answers to components
        for (let i = 0; i < questions.length; i++) {
            const saved = savedProgress.answersByQuestionId.get(questions[i].id)
            if (saved !== undefined && saved !== null) {
                components[i].setAnswer(saved)
                answeredState[i] = hasAnswer(saved)
            }
        }
        updateProgress()

        // Show project title and description instantly
        await appendAiBubble(project.title, { animated: false, bubbleClass: 'chat-bubble--project-title' })
        if (project.description) {
            await appendAiBubble(project.description, { animated: false })
        }

        // Quick replay of answered questions (no animation)
        for (let i = 0; i < resumeAt && i < questions.length; i++) {
            const q = questions[i]
            await appendAiBubble(q.text, {
                animated: false,
                bubbleClass: 'chat-bubble--question-title',
                questionNum: i + 1,
                required: q.isRequired,
            })

            const answer = components[i].getAnswer()
            const displayText = formatAnswerForDisplay(q, answer)
            if (displayText) {
                appendUserBubble(displayText)
            }

            const confirmRow = document.createElement('div')
            confirmRow.className = 'chat-confirm-row chat-confirm-row--confirmed'
            confirmRow.setAttribute('data-confirm-for', String(i))
            confirmRow.innerHTML = `
                <div class="chat-confirm-line"></div>
                <div class="chat-confirm-btn" aria-hidden="true">${CHECKMARK_SVG}</div>
                <div class="chat-confirm-line"></div>`
            messagesEl.appendChild(confirmRow)
        }

        scrollToBottom()

        // Show resume message then continue
        await appendAiBubble(t.resuming)

        if (resumeAt < questions.length) {
            await revealQuestion(resumeAt)
        } else {
            await showSubmitSection()
        }
    } else {
        if (savedProgress) {
            confirmedUpToIndex = Math.max(0, Math.min(savedProgress.currentQuestionIndex, questions.length))
            savedProgress.openTextDraftsByQuestionId.forEach((messages, questionId) => {
                openTextDraftsByQuestionId.set(questionId, [...messages])
            })
        }

        // Normal start
        await appendAiBubble(project.title, { animated: false, bubbleClass: 'chat-bubble--project-title' })
        await wait(300)
        await appendAiBubble(project.description)
        await wait(1500)

        if (questions.length > 0) {
            await revealQuestion(0)
        } else {
            await appendAiBubble(t.noQuestions)
        }
    }
}
