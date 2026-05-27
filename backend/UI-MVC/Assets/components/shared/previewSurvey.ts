import { renderSingleChoiceQuestion } from '../survey/components/singleChoiceQuestion'
import { renderMultipleChoiceQuestion } from '../survey/components/multipleChoiceQuestion'
import { renderScaleQuestion } from '../survey/components/scaleQuestion'
import { renderSurveyHeader, createSurveyHeaderController } from '../survey/components/surveyHeader'
import { applyTheme } from '../../utils/theme'
import { generateQuestionHeader } from '../survey/utils/surveyUtils'
import { getSurveyStrings } from '../../i18n/survey'
import { QuestionType } from '../../models/question'
import type { FixedQuestion, OpenQuestion, RangeQuestion, Question } from '../../models/question'
import type { QuestionComponent, QuestionAnswer } from '../survey/components/singleChoiceQuestion'

// ── Draft field name constants (mirrors createProjectStepper.ts STEP1_FIELD_MAP) ──

const STEP1_FIELDS = {
    name: 'CreateStep1ViewModel.Name',
    description: 'CreateStep1ViewModel.Description',
    interactionForm: 'CreateStep1ViewModel.InteractionForm',
    imageUrl: 'CreateStep1ViewModel.ImageUrl',
    themePrimary: 'CreateStep1ViewModel.ThemePrimary',
    themeSecondary: 'CreateStep1ViewModel.ThemeSecondary',
    themeAccent: 'CreateStep1ViewModel.ThemeAccent',
    themePreset: 'CreateStep1ViewModel.ThemePreset',
    themeFont: 'CreateStep1ViewModel.ThemeFont',
} as const

const STEP2_QUESTIONS_JSON = 'CreateStep2ViewModel.QuestionsJson'

let draftPrefix = ''

interface DraftQuestion {
    type: string
    text: string
    required: boolean
    possibleAnswers?: { text: string }[]
    min?: number
    max?: number
}

// ── Project preview data shape ──

interface ProjectPreviewData {
    title: string
    description: string
    imageUrl: string
    primary: string
    secondary: string
    accent: string
    preset: string
    font: string
    interactionType: string
}

// ── Map C# InteractionType enum values to TS expected values ──

function mapInteractionType(csharpValue: string): string {
    if (csharpValue === 'VerticalScroll' || csharpValue === '1') return 'Vertical_Scroll'
    if (csharpValue === 'Chat' || csharpValue === '0') return 'Chat'
    if (csharpValue === 'UserDefined' || csharpValue === '2') return 'UserDefined'
    return 'Vertical_Scroll'
}

// ── Read step 1 draft from localStorage ──

function readDraftData(draftPrefix: string): ProjectPreviewData | null {
    const raw = localStorage.getItem(`${draftPrefix}:step:1`)
    if (!raw) return null

    try {
        const parsed = JSON.parse(raw)
        const fields: Record<string, string> = parsed.fields ?? {}
        return {
            title: fields[STEP1_FIELDS.name] ?? 'Untitled Project',
            description: fields[STEP1_FIELDS.description] ?? '',
            imageUrl: fields[STEP1_FIELDS.imageUrl] ?? '',
            primary: fields[STEP1_FIELDS.themePrimary] ?? '#6c5ce7',
            secondary: fields[STEP1_FIELDS.themeSecondary] ?? '#db99c8',
            accent: fields[STEP1_FIELDS.themeAccent] ?? '#cd6f88',
            preset: fields[STEP1_FIELDS.themePreset] ?? 'default',
            font: fields[STEP1_FIELDS.themeFont] ?? 'Helvetica',
            interactionType: fields[STEP1_FIELDS.interactionForm] ?? 'VerticalScroll',
        }
    } catch {
        return null
    }
}

function readQuestions(): (RangeQuestion | FixedQuestion | OpenQuestion)[] {
    const raw = localStorage.getItem(`${draftPrefix}:step:2`)
    if (!raw) return []

    try {
        const parsed = JSON.parse(raw)
        const fields: Record<string, string> = parsed.fields ?? {}
        const jsonStr = fields[STEP2_QUESTIONS_JSON]
        if (!jsonStr || jsonStr === '[]') return []

        const dtos: DraftQuestion[] = JSON.parse(jsonStr)
        if (!Array.isArray(dtos) || dtos.length === 0) return []

        return dtos.map((dto, index) => {
            if (dto.type === 'Scale') {
                return {
                    id: index + 1,
                    type: QuestionType.Scale,
                    text: dto.text,
                    required: dto.required,
                    min: dto.min ?? 1,
                    max: dto.max ?? 5,
                }
            }
            if (dto.type === 'SingleChoice') {
                return {
                    id: index + 1,
                    type: QuestionType.SingleChoice,
                    text: dto.text,
                    required: dto.required,
                    possibleAnswers: (dto.possibleAnswers ?? []).map((a, ai) => ({ id: ai + 1, text: a.text })),
                }
            }
            if (dto.type === 'MultipleChoice') {
                return {
                    id: index + 1,
                    type: QuestionType.MultipleChoice,
                    text: dto.text,
                    required: dto.required,
                    possibleAnswers: (dto.possibleAnswers ?? []).map((a, ai) => ({ id: ai + 1, text: a.text })),
                }
            }
            return {
                id: index + 1,
                type: QuestionType.Open,
                text: dto.text,
                required: dto.required,
            }
        })
    } catch {
        return []
    }
}

// ── Simplified open-text component (no speech/brainstorm deps) ──

function renderOpenTextPreview(question: OpenQuestion, index: number): QuestionComponent {
    let textValue = ''
    let answerCallback: (() => void) | null = null
    let isLocked = false

    const wrapper = document.createElement('div')
    wrapper.setAttribute('data-question-index', String(index))
    wrapper.className = 'survey-question-group'

    wrapper.innerHTML = `
        ${generateQuestionHeader(question, index + 1)}
        <div class="survey-textarea-wrapper">
            <div class="relative">
                <textarea class="survey-textarea" placeholder="Type your answer..." rows="4"></textarea>
            </div>
        </div>
    `

    const textarea = wrapper.querySelector<HTMLTextAreaElement>('textarea')!

    textarea.addEventListener('input', () => {
        if (isLocked) return
        textValue = textarea.value
        answerCallback?.()
    })

    return {
        getAnswer: (): QuestionAnswer => (textValue.trim().length > 0 ? textValue.trim() : null),
        validate: () => true,
        lock: () => { isLocked = true; wrapper.style.opacity = '0.4'; wrapper.style.pointerEvents = 'none' },
        unlock: () => { isLocked = false; wrapper.style.opacity = '1'; wrapper.style.pointerEvents = 'auto' },
        onAnswer: (cb) => { answerCallback = cb },
        setAnswer: (answer) => { textValue = typeof answer === 'string' ? answer : ''; textarea.value = textValue },
        getElement: () => wrapper,
    }
}

// ── Create a question component for any question type ──

function createQuestionComponent(question: Question, index: number): QuestionComponent {
    if (question.type === QuestionType.SingleChoice || question.type === QuestionType.MultipleChoice) {
        if (question.type === QuestionType.MultipleChoice) {
            return renderMultipleChoiceQuestion(question as FixedQuestion, index)
        }
        return renderSingleChoiceQuestion(question as FixedQuestion, index)
    }
    if (question.type === QuestionType.Scale) {
        return renderScaleQuestion(question as RangeQuestion, index)
    }
    return renderOpenTextPreview(question as OpenQuestion, index)
}

// ── HTML escape ──

function esc(value: string): string {
    return value
        .replaceAll('&', '&amp;')
        .replaceAll('<', '&lt;')
        .replaceAll('>', '&gt;')
        .replaceAll('"', '&quot;')
        .replaceAll("'", '&#39;')
}

// ── Minimal chat avatar ──

const AVATAR_SVG = `<svg viewBox="0 0 36 36" fill="none" xmlns="http://www.w3.org/2000/svg" aria-hidden="true">
    <circle cx="18" cy="18" r="17" fill="var(--color-primary)" stroke="color-mix(in srgb, var(--color-primary) 85%, white)" stroke-width="2"/>
    <rect x="9" y="12" width="18" height="12" rx="2" fill="white" fill-opacity="0.95"/>
    <circle cx="14" cy="18" r="1.8" fill="var(--color-primary)"/>
    <circle cx="22" cy="18" r="1.8" fill="var(--color-primary)"/>
</svg>`

// ── Generate hero HTML (image + title + description) ──

function heroHTML(data: ProjectPreviewData): string {
    const imageSection = data.imageUrl
        ? `<img src="${esc(data.imageUrl)}" alt="${esc(data.title)}" class="survey-hero-image" />`
        : ''

    return `
        <section class="survey-hero" id="survey-hero">
            ${imageSection}
            <div class="survey-hero-content lg:top-[28%]">
                <h1 class="survey-hero-title">${esc(data.title)}</h1>
                <p class="survey-hero-description">${esc(data.description)}</p>
            </div>
        </section>
    `
}

// ── Render VerticalScroll mode ──

function renderVerticalScrollPreview(container: HTMLElement, data: ProjectPreviewData, questions: Question[]): void {
    container.innerHTML = `
        <div class="survey-shell">
            ${renderSurveyHeader({ organizationName: 'Preview', organizationSlug: 'preview' })}
            ${heroHTML(data)}
            <div class="survey-header" id="survey-header">
                <div class="survey-header-content">
                    <h2 class="survey-title">${esc(data.title)}</h2>
                    <div class="survey-progress-container">
                        <div class="survey-progress-bar">
                            <div class="survey-progress-fill" id="progress-bar"></div>
                        </div>
                        <span class="survey-progress-badge" id="progress-badge">0 / ${questions.length}</span>
                    </div>
                </div>
            </div>
            <div class="survey-content">
                <div id="questions-container"></div>
            </div>
        </div>
    `

    const questionsContainer = container.querySelector<HTMLDivElement>('#questions-container')!
    const headerController = createSurveyHeaderController({ root: container })

    if (questions.length === 0) {
        questionsContainer.innerHTML = '<p class="survey-empty-questions">No questions added yet. Add questions in Step 2 to see them here.</p>'
        headerController.updateProgress(0, 1)
        return
    }

    let answeredCount = 0
    const updateProgress = () => {
        headerController.updateProgress(answeredCount, questions.length)
    }

    questions.forEach((question, index) => {
        const component = createQuestionComponent(question, index)
        let wasAnswered = false
        component.onAnswer(() => {
            const nowAnswered = component.getAnswer() !== null
            if (!wasAnswered && nowAnswered) answeredCount++
            else if (wasAnswered && !nowAnswered) answeredCount--
            wasAnswered = nowAnswered
            updateProgress()
        })
        questionsContainer.appendChild(component.getElement())
    })

    headerController.updateProgress(0, questions.length || 1)
}

// ── Render Chat mode (sequential question-by-question flow) ──

function wait(ms: number): Promise<void> {
    return new Promise(resolve => setTimeout(resolve, ms))
}

interface ChatHistoryEntry {
    signature: string
    answer: QuestionAnswer              // raw answer value (string for open, number for choice, etc.)
}

interface ChatHistory {
    entries: (ChatHistoryEntry | null)[]  // null = unanswered
}

function questionSignature(q: Question): string {
    const opts = q.type === QuestionType.Scale
        ? `${(q as RangeQuestion).min}:${(q as RangeQuestion).max}`
        : q.type === QuestionType.SingleChoice || q.type === QuestionType.MultipleChoice
            ? (q as FixedQuestion).possibleAnswers.map(a => a.text).join('|')
            : ''
    return `${q.text}|${q.type}|${opts}`
}

function answerDisplayText(q: Question, answer: QuestionAnswer): string {
    if (answer === null || answer === undefined) return ''
    if (q.type === QuestionType.SingleChoice) {
        const ans = (q as FixedQuestion).possibleAnswers.find(a => a.id === answer as number)
        return ans?.text ?? String(answer)
    }
    if (q.type === QuestionType.MultipleChoice) {
        const ids = answer as number[]
        return (q as FixedQuestion).possibleAnswers
            .filter(a => ids.includes(a.id!))
            .map(a => a.text)
            .join(', ')
    }
    return String(answer)
}

async function renderChatPreview(container: HTMLElement, data: ProjectPreviewData, questions: Question[]): Promise<void> {
    const t = getSurveyStrings()
    const historyKey = `${draftPrefix}:chat-history`

    function loadHistory(): ChatHistory {
        try {
            const raw = localStorage.getItem(historyKey)
            if (!raw) return { entries: [] }
            return JSON.parse(raw) as ChatHistory
        } catch {
            return { entries: [] }
        }
    }

    function saveHistory(h: ChatHistory): void {
        try {
            localStorage.setItem(historyKey, JSON.stringify(h))
        } catch { /* ignore */ }
    }

    // Build simplified chat shell
    container.innerHTML = `
        <div class="chat-shell" id="chat-shell">
            <div class="chat-scroll-area" id="chat-scroll-area">
                ${renderSurveyHeader({ organizationName: 'Preview', organizationSlug: 'preview' })}
                <div class="survey-header" id="chat-survey-header">
                    <div class="survey-header-content">
                        <h2 class="survey-title">${esc(data.title)}</h2>
                        <div class="survey-progress-container">
                            <div class="survey-progress-bar">
                                <div class="survey-progress-fill" id="progress-bar"></div>
                            </div>
                            <span class="survey-progress-badge" id="progress-badge">0 / ${questions.length}</span>
                        </div>
                    </div>
                </div>
                <div class="chat-messages" id="chat-messages"></div>
            </div>
            <div class="chat-input-wrap">
                <div class="chat-input-bar">
                    <textarea id="chat-input" class="chat-input"
                        placeholder="${esc(t.selectAbove)}" rows="1" disabled></textarea>
                    <button id="chat-send-btn" class="chat-send-btn" type="button" disabled>
                        <svg class="chat-send-icon" viewBox="0 0 24 24" fill="currentColor">
                            <path d="M2.01 21L23 12 2.01 3 2 10l15 2-15 2z"/>
                        </svg>
                    </button>
                </div>
            </div>
        </div>`

    const messagesEl = container.querySelector<HTMLDivElement>('#chat-messages')!
    const chatInput = container.querySelector<HTMLTextAreaElement>('#chat-input')! as HTMLTextAreaElement
    const sendBtn = container.querySelector<HTMLButtonElement>('#chat-send-btn')!
    const headerController = createSurveyHeaderController({ root: container })

    let answeredCount = 0
    let activeOpenIndex: number | null = null
    let openSendHandler: (() => void) | null = null

    const scrollToBottom = () => {
        const scrollArea = container.querySelector<HTMLElement>('#chat-scroll-area')
        if (scrollArea) scrollArea.scrollTop = scrollArea.scrollHeight
    }

    const updateProgress = () => {
        headerController.updateProgress(answeredCount, questions.length)
    }

    function appendAiBubbleText(text: string, bubbleClass?: string): void {
        const row = document.createElement('div')
        row.className = 'chat-row chat-row--ai'
        const extraClass = bubbleClass ? ` ${bubbleClass}` : ''
        row.innerHTML = `
            <div class="chat-avatar">${AVATAR_SVG}</div>
            <div class="chat-bubble-wrapper">
                <div class="chat-bubble-group">
                    <div class="chat-bubble chat-bubble--ai${extraClass}"></div>
                </div>
            </div>`
        const bubble = row.querySelector('.chat-bubble')!
        bubble.textContent = text
        messagesEl.appendChild(row)
        scrollToBottom()
    }

    function appendUserBubble(text: string): void {
        const row = document.createElement('div')
        row.className = 'chat-row chat-row--user'
        const bubble = document.createElement('div')
        bubble.className = 'chat-bubble chat-bubble--user'
        bubble.textContent = text
        row.appendChild(bubble)
        messagesEl.appendChild(row)
        scrollToBottom()
    }

    function deactivateInput(): void {
        activeOpenIndex = null
        openSendHandler = null
        chatInput.disabled = true
        chatInput.value = ''
        chatInput.style.height = 'auto'
        chatInput.placeholder = t.selectAbove
        sendBtn.disabled = true
    }

    function activateOpenTextInput(index: number, onSend: (text: string) => void): void {
        deactivateInput()
        activeOpenIndex = index
        chatInput.disabled = false
        chatInput.value = ''
        chatInput.style.height = 'auto'
        chatInput.placeholder = t.typeHere
        sendBtn.disabled = false

        const handler = () => {
            const text = chatInput.value.trim()
            if (!text) return
            openSendHandler = null
            onSend(text)
        }
        openSendHandler = handler
        setTimeout(() => chatInput.focus(), 100)
    }

    // Global send / Enter handler
    sendBtn.addEventListener('click', () => openSendHandler?.())
    chatInput.addEventListener('keydown', (e) => {
        if (e.key === 'Enter' && !e.shiftKey) {
            e.preventDefault()
            openSendHandler?.()
        }
    })

    async function revealQuestion(index: number): Promise<void> {
        const q = questions[index]
        if (!q) return

        const questionNum = index + 1
        await wait(400)
        appendAiBubbleText(`${questionNum}. ${q.text}`, 'chat-bubble--question-title')
        await wait(500)

        if (q.type === QuestionType.Open) {
            activateOpenTextInput(index, (text) => {
                appendUserBubble(text)
                deactivateInput()
                confirmQuestion(index, text)
            })
            return
        }

        // Non-open: answer region + confirm button
        const block = document.createElement('div')
        block.className = 'chat-question-block'
        block.setAttribute('data-question-index', String(index))

        const answerRegion = document.createElement('div')
        answerRegion.className = 'chat-answer-region'

        const component = createQuestionComponent(q, index)
        const el = component.getElement()
        el.classList.add('chat-question-component')
        answerRegion.appendChild(el)

        const confirmRow = document.createElement('div')
        confirmRow.className = 'chat-confirm-row'
        confirmRow.innerHTML = `
            <div class="chat-confirm-line"></div>
            <button class="chat-confirm-btn" type="button">Confirm</button>
            <div class="chat-confirm-line"></div>`
        confirmRow.querySelector<HTMLButtonElement>('.chat-confirm-btn')!.addEventListener('click', () => {
            const answer = component.getAnswer()
            confirmRow.classList.add('chat-confirm-row--confirmed')
            confirmQuestion(index, answer)
        })

        block.appendChild(answerRegion)
        block.appendChild(confirmRow)
        messagesEl.appendChild(block)
        scrollToBottom()
    }

    function confirmQuestion(index: number, answer: QuestionAnswer): void {
        if (questions.length === 0) return

        // Show user answer bubble
        const q = questions[index]
        const display = answerDisplayText(q, answer)
        if (display) appendUserBubble(display)

        // Save to history
        const history = loadHistory()
        const sig = questionSignature(questions[index])
        history.entries[index] = { signature: sig, answer }
        saveHistory(history)

        answeredCount = Math.max(answeredCount, index + 1)
        updateProgress()

        if (index < questions.length - 1) {
            setTimeout(() => revealQuestion(index + 1), 400)
        } else {
            setTimeout(() => showSubmitSection(), 400)
        }
    }

    function showSubmitSection(): void {
        deactivateInput()
        appendAiBubbleText('All questions answered.', undefined)
        const submitRow = document.createElement('div')
        submitRow.className = 'survey-action-bar survey-ready'
        submitRow.innerHTML = `<button class="survey-submit-btn" disabled>${t.submitSurvey}</button>`
        messagesEl.appendChild(submitRow)
        scrollToBottom()
    }

    // ── Start the chat flow ──

    headerController.updateProgress(0, questions.length || 1)

    // Hero bubble
    if (data.imageUrl || data.title) {
        let content = ''
        if (data.imageUrl) {
            content += `<img src="${esc(data.imageUrl)}" alt="${esc(data.title)}"
                style="max-width:100%; border-radius:12px; margin-bottom:0.75rem;" />`
        }
        content += `<strong>${esc(data.title)}</strong>`
        if (data.description) {
            content += `<p style="margin:0.5rem 0 0 0; font-size:0.9em; opacity:0.85;">${esc(data.description)}</p>`
        }
        const heroRow = document.createElement('div')
        heroRow.className = 'chat-row chat-row--ai'
        heroRow.innerHTML = `
            <div class="chat-avatar">${AVATAR_SVG}</div>
            <div class="chat-bubble-wrapper">
                <div class="chat-bubble-group">
                    <div class="chat-bubble chat-bubble--ai chat-bubble--project-title">${content}</div>
                </div>
            </div>`
        messagesEl.appendChild(heroRow)
        scrollToBottom()
        await wait(500)
    }

    if (data.description) {
        appendAiBubbleText(data.description)
        await wait(800)
    }

    if (questions.length === 0) {
        appendAiBubbleText('No questions added yet. Add questions in Step 2 to see them here.')
        headerController.updateProgress(0, 1)
        return
    }

    // ── Replay previously answered questions with full UI state ──
    const history = loadHistory()
    let resumeAtIndex = 0

    function replayAnswered(index: number, entry: ChatHistoryEntry): void {
        const q = questions[index]
        const num = index + 1
        appendAiBubbleText(`${num}. ${q.text}`, 'chat-bubble--question-title')

        if (q.type === QuestionType.Open) {
            // Show user answer bubble
            const text = typeof entry.answer === 'string' ? entry.answer : ''
            if (text) appendUserBubble(text)
        } else {
            // Non-open: restore component with answer, locked, confirmed
            const block = document.createElement('div')
            block.className = 'chat-question-block'
            block.setAttribute('data-question-index', String(index))

            const answerRegion = document.createElement('div')
            answerRegion.className = 'chat-answer-region'

            const component = createQuestionComponent(q, index)
            component.setAnswer(entry.answer)
            component.lock()
            const el = component.getElement()
            el.classList.add('chat-question-component')
            answerRegion.appendChild(el)

            const confirmRow = document.createElement('div')
            confirmRow.className = 'chat-confirm-row chat-confirm-row--confirmed'
            confirmRow.innerHTML = `
                <div class="chat-confirm-line"></div>
                <button class="chat-confirm-btn" type="button" disabled>Confirmed</button>
                <div class="chat-confirm-line"></div>`

            block.appendChild(answerRegion)
            block.appendChild(confirmRow)
            messagesEl.appendChild(block)

            // Show user answer bubble
            const display = answerDisplayText(q, entry.answer)
            if (display) appendUserBubble(display)
        }

        resumeAtIndex = index + 1
        answeredCount = index + 1
    }

    for (let i = 0; i < questions.length; i++) {
        const entry = history.entries[i]
        if (!entry) break
        const currentSig = questionSignature(questions[i])
        if (entry.signature !== currentSig) break
        replayAnswered(i, entry)
    }

    // Trim history entries beyond current question list
    if (history.entries.length > questions.length) {
        history.entries.length = questions.length
        saveHistory(history)
    }
    // Invalidate entries where signature changed (already handled by break above)
    // Clean entries beyond resume point
    for (let i = resumeAtIndex; i < history.entries.length; i++) {
        history.entries[i] = null
    }
    saveHistory(history)

    updateProgress()

    // Reveal next unanswered question
    await wait(600)
    if (resumeAtIndex < questions.length) {
        await revealQuestion(resumeAtIndex)
    } else {
        showSubmitSection()
    }
}

// ── Init ──

function init(): void {
    const root = document.getElementById('preview-root')
    if (!root) return

    draftPrefix = root.dataset.draftPrefix || ''
    if (!draftPrefix) {
        root.innerHTML = '<p style="padding:2rem;color:#999;">No draft prefix configured.</p>'
        return
    }

    let data: ProjectPreviewData | null = readDraftData(draftPrefix)
    let currentMode = 'Vertical_Scroll'
    let isUserDefined = false

    function resolveUserDefinedMode(): string {
        return localStorage.getItem(`${draftPrefix}:preview-mode`) ?? 'Vertical_Scroll'
    }

    function render(mode?: string): void {
        if (!data) return
        if (!root) return

        try {
            currentMode = mode ?? currentMode

            root!.innerHTML = ''
            root!.style.position = 'relative'

            applyTheme({
                primary: data.primary,
                secondary: data.secondary,
                accent: data.accent,
                preset: data.preset,
                font: data.font,
            })

            document.body.style.fontFamily = 'var(--font-primary)'

            const questions = readQuestions()

            if (currentMode === 'Chat') {
                renderChatPreview(root, data, questions)
            } else {
                renderVerticalScrollPreview(root, data, questions)
            }
        } catch (err) {
            console.error('[preview] render failed', err)
            root.innerHTML = '<p style="padding:2rem;color:#c33;">Preview render error. Check console.</p>'
        }
    }

    function tryUpdate(force: boolean = false): void {
        if (!root) return
        const newData = readDraftData(draftPrefix)
        if (!newData && !data) return

        const changed = force || JSON.stringify(newData) !== JSON.stringify(data)
        if (!changed) return

        data = newData
        if (data) {
            isUserDefined = data.interactionType === 'UserDefined' || data.interactionType === '2'
            if (isUserDefined) {
                currentMode = resolveUserDefinedMode()
            } else {
                currentMode = mapInteractionType(data.interactionType)
            }
            render()
        } else {
            root.innerHTML = '<p style="padding:2rem;color:#999;">No draft data found. Fill in step 1 fields to see preview.</p>'
        }
    }

    if (data) {
        isUserDefined = data.interactionType === 'UserDefined'
        if (isUserDefined) {
            currentMode = resolveUserDefinedMode()
        } else {
            currentMode = mapInteractionType(data.interactionType)
        }
        render()
    } else {
        root.innerHTML = '<p style="padding:2rem;color:#999;">No draft data found. Fill in step 1 fields to see preview.</p>'
    }

    window.addEventListener('message', (e) => {
        if (e.origin !== window.location.origin) return
        if (e.data?.type === 'draft-changed') {
            const key: string = e.data?.storageKey ?? ''
            if (key === `${draftPrefix}:step:1`) {
                tryUpdate()
            } else if (key === `${draftPrefix}:step:2`) {
                tryUpdate(true)
            }
        }
    })

    window.addEventListener('storage', (e) => {
        if (e.key === `${draftPrefix}:step:1`) {
            tryUpdate()
        } else if (e.key === `${draftPrefix}:step:2`) {
            tryUpdate(true)
        }
    })

    let lastStep1Sig = ''
    let lastStep2Sig = ''
    let lastPreviewMode = ''
    setInterval(() => {
        const raw1 = localStorage.getItem(`${draftPrefix}:step:1`)
        const raw2 = localStorage.getItem(`${draftPrefix}:step:2`)
        const sig1 = raw1 ?? ''
        const sig2 = raw2 ?? ''
        if (sig1 !== lastStep1Sig) {
            lastStep1Sig = sig1
            tryUpdate()
        }
        if (sig2 !== lastStep2Sig) {
            lastStep2Sig = sig2
            tryUpdate(true)
        }
        if (isUserDefined) {
            const previewMode = localStorage.getItem(`${draftPrefix}:preview-mode`) ?? 'Vertical_Scroll'
            if (previewMode !== lastPreviewMode) {
                lastPreviewMode = previewMode
                if (currentMode !== previewMode) {
                    currentMode = previewMode
                    render()
                }
            }
        }
    }, 500)
}

if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', init)
} else {
    init()
}
