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
    if (csharpValue === 'VerticalScroll') return 'Vertical_Scroll'
    if (csharpValue === 'Chat') return 'Chat'
    if (csharpValue === 'UserDefined') return 'UserDefined'
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

// ── Minimal chat shell (no ideation modals — preview only) ──

const AVATAR_SVG = `<svg viewBox="0 0 36 36" fill="none" xmlns="http://www.w3.org/2000/svg" aria-hidden="true">
    <circle cx="18" cy="18" r="17" fill="var(--color-primary)" stroke="color-mix(in srgb, var(--color-primary) 85%, white)" stroke-width="2"/>
    <rect x="9" y="12" width="18" height="12" rx="2" fill="white" fill-opacity="0.95"/>
    <circle cx="14" cy="18" r="1.8" fill="var(--color-primary)"/>
    <circle cx="22" cy="18" r="1.8" fill="var(--color-primary)"/>
</svg>`

function renderChatShell(data: ProjectPreviewData, questionCount: number): string {
    const t = getSurveyStrings()
    return `
        <div class="chat-shell">
            <div class="chat-scroll-area">
                ${renderSurveyHeader({ organizationName: 'Preview', organizationSlug: 'preview' })}
                <div class="survey-header">
                    <div class="survey-header-content">
                        <h2 class="survey-title">${esc(data.title)}</h2>
                        <div class="survey-progress-container">
                            <div class="survey-progress-bar">
                                <div class="survey-progress-fill" id="progress-bar"></div>
                            </div>
                            <span class="survey-progress-badge" id="progress-badge">0 / ${questionCount}</span>
                        </div>
                    </div>
                </div>
                <div class="chat-messages" id="chat-messages"></div>
            </div>
            <div class="chat-input-wrap">
                <div class="chat-input-bar">
                    <textarea id="chat-input" class="chat-input" placeholder="${esc(t.selectAbove)}" rows="1" disabled></textarea>
                    <button id="chat-send-btn" class="chat-send-btn" type="button" disabled>
                        <svg class="chat-send-icon" viewBox="0 0 24 24" fill="currentColor" aria-hidden="true">
                            <path d="M2.01 21L23 12 2.01 3 2 10l15 2-15 2z"/>
                        </svg>
                    </button>
                </div>
            </div>
        </div>`
}

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
    } else {
        questions.forEach((question, index) => {
            const component = createQuestionComponent(question, index)
            questionsContainer.appendChild(component.getElement())
        })
    }

    headerController.updateProgress(0, questions.length || 1)
}

// ── Render Chat mode ──

function renderChatPreview(container: HTMLElement, data: ProjectPreviewData, questions: Question[]): void {
    container.innerHTML = renderChatShell(data, questions.length)

    const messagesContainer = container.querySelector<HTMLDivElement>('#chat-messages')!
    const headerController = createSurveyHeaderController({ root: container })

    let bubblesHTML = ''

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
        bubblesHTML += `
            <div class="chat-row chat-row--ai">
                <div class="chat-avatar">${AVATAR_SVG}</div>
                <div class="chat-bubble-wrapper">
                    <div class="chat-bubble-group">
                        <div class="chat-bubble chat-bubble--ai chat-bubble--project-title">${content}</div>
                    </div>
                </div>
            </div>`
    }

    questions.forEach((question, index) => {
        const component = createQuestionComponent(question, index)
        const element = component.getElement()

        const headerEl = element.querySelector('.survey-question-header')
        const optionsEl = element.querySelector('.survey-options')
        const textareaEl = element.querySelector('.survey-textarea-wrapper')
        const scaleEl = element.querySelector('.scale-row')

        const bubbleContent = [
            headerEl?.outerHTML ?? '',
            optionsEl?.outerHTML ?? '',
            textareaEl?.outerHTML ?? '',
            scaleEl?.outerHTML ?? '',
        ].join('')

        bubblesHTML += `
            <div class="chat-row chat-row--ai">
                <div class="chat-avatar">${AVATAR_SVG}</div>
                <div class="chat-bubble-wrapper">
                    <div class="chat-bubble-group">
                        <div class="chat-bubble chat-bubble--ai chat-bubble--question-title">${bubbleContent}</div>
                    </div>
                </div>
            </div>`
    })

    if (questions.length === 0) {
        bubblesHTML += `
            <div class="chat-row chat-row--ai">
                <div class="chat-avatar">${AVATAR_SVG}</div>
                <div class="chat-bubble-wrapper">
                    <div class="chat-bubble-group">
                        <div class="chat-bubble chat-bubble--ai">No questions added yet. Add questions in Step 2 to see them here.</div>
                    </div>
                </div>
            </div>`
    }

    messagesContainer.innerHTML = bubblesHTML
    headerController.updateProgress(0, questions.length || 1)
}

// ── Render UserDefined layout picker ──

async function renderUserDefinedPicker(
    container: HTMLElement,
    data: ProjectPreviewData,
    questions: Question[],
    onChoice: (mode: string) => void,
): Promise<void> {
    try {
        const { showLayoutPicker } = await import('../survey/components/layoutPicker')
        const storageKey = `preview-layout-${Math.random().toString(36).slice(2, 8)}`
        const choice = await showLayoutPicker({
            container,
            storageKey,
            organizationName: 'Preview',
            organizationSlug: 'preview',
        })
        onChoice(choice)
    } catch {
        renderVerticalScrollPreview(container, data, questions)
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
    let layoutPicked = false

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

            if (isUserDefined && !layoutPicked) {
                renderUserDefinedPicker(root!, data, questions, (choice) => {
                    layoutPicked = true
                    currentMode = choice
                    render()
                })
                return
            }

            if (currentMode === 'Chat') {
                renderChatPreview(root, data, questions)
            } else {
                renderVerticalScrollPreview(root, data, questions)
            }

            if (isUserDefined && layoutPicked) {
                const resetBar = document.createElement('div')
                resetBar.className = 'fixed bottom-4 right-4 z-50'
                resetBar.innerHTML = `<button id="preview-layout-reset" class="rounded-lg border border-text/30 bg-background px-3 py-1.5 text-xs text-text/70 hover:border-text/50 hover:text-text transition-colors">Change layout</button>`
                root!.appendChild(resetBar)
                const resetBtn = root!.querySelector<HTMLButtonElement>('#preview-layout-reset')
                resetBtn?.addEventListener('click', () => {
                    layoutPicked = false
                    render()
                })
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
            currentMode = mapInteractionType(data.interactionType)
            isUserDefined = data.interactionType === 'UserDefined'
            if (isUserDefined) { currentMode = 'Vertical_Scroll'; layoutPicked = false }
            render()
        } else {
            root.innerHTML = '<p style="padding:2rem;color:#999;">No draft data found. Fill in step 1 fields to see preview.</p>'
        }
    }

    if (data) {
        currentMode = mapInteractionType(data.interactionType)
        isUserDefined = data.interactionType === 'UserDefined'
        if (isUserDefined) { currentMode = 'Vertical_Scroll'; layoutPicked = false }
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
}

if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', init)
} else {
    init()
}
