import '../../../styles/pages/chat-survey.css'
import '../../../styles/pages/ideas.css'
import type { Project } from '../../../models/project'
import type { ProjectContext } from '../../../main'
import { navigate } from '../../../main'
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
import { createIdeasListController } from '../../ideas/ideasListController'
import { createSafetyReviewDialogController } from '../../ideas/safetyReviewDialog'
import { createIdeaPanelController } from '../../ideas/ideaPanel'
import { createIdeasSubmitHandler } from '../../ideas/ideasSubmitHandler'
import { createFirstIdeaContactDialogController } from '../../ideas/firstIdeaContactDialog'
import type { Idea, IdeaTopic } from '../../../models/idea'
import type { ActiveView } from '../../ideas/types'
import { getSurveyStrings } from '../../../i18n/survey'

interface OpenTextState {
    questionIndex: number
    messages: string[]
    floatingConfirmRow: HTMLElement | null
}

// Matches the speaker icon used in vertical scroll mode
const SPEAKER_SVG = `<svg class="chat-speaker-icon" fill="currentColor" viewBox="0 0 24 24" aria-hidden="true">
  <path d="M3 9v6h4l5 4V5L7 9H3zm13.5 3a4.5 4.5 0 00-2.5-4.03v8.06A4.5 4.5 0 0016.5 12zm-2.5-9.5v2.06a7 7 0 010 13.88v2.06c4.01-.91 7-4.49 7-8.99s-2.99-8.08-7-8.99z"/>
</svg>`

const AI_AVATAR = `<svg viewBox="0 0 36 36" fill="none" xmlns="http://www.w3.org/2000/svg">
  <circle cx="18" cy="18" r="18" fill="var(--color-primary)"/>
  <circle cx="18" cy="14" r="5" fill="white" fill-opacity="0.9"/>
  <path d="M6 32c0-5.523 5.373-9 12-9s12 3.477 12 9" fill="white" fill-opacity="0.9"/>
</svg>`

const MAGIC_SVG = `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
  <path d="M9.937 15.5A2 2 0 0 0 8.5 14.063l-6.135-1.582a.5.5 0 0 1 0-.962L8.5 9.936A2 2 0 0 0 9.937 8.5l1.582-6.135a.5.5 0 0 1 .963 0L14.063 8.5A2 2 0 0 0 15.5 9.937l6.135 1.581a.5.5 0 0 1 0 .964L15.5 14.063a2 2 0 0 0-1.437 1.437l-1.582 6.135a.5.5 0 0 1-.963 0z"/>
  <path d="M20 3v4m2-2h-4M4 17v2m1-1H3"/>
</svg>`

const CHECKMARK_SVG = `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
  <polyline points="20 6 9 17 4 12"/>
</svg>`

function esc(text: string): string {
    return text
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
}

function wait(ms: number): Promise<void> {
    return new Promise((resolve) => setTimeout(resolve, ms))
}

function hasAnswer(answer: QuestionAnswer): boolean {
    return Array.isArray(answer) ? answer.length > 0 : answer !== null && answer !== ''
}

function formatAnswerForDisplay(q: Question, answer: QuestionAnswer): string {
    if (answer === null || answer === '') return ''
    if (typeof answer === 'string') return answer
    if (typeof answer === 'number') {
        if (q.type === QuestionType.SingleChoice && q.options) {
            return q.options.find((o) => o.id === answer)?.text ?? String(answer)
        }
        return String(answer)
    }
    if (Array.isArray(answer) && q.options) {
        return answer.map((id) => q.options?.find((o) => o.id === id)?.text ?? String(id)).join(', ')
    }
    return ''
}

const IDEATION_MODALS_HTML = `
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
                        <span aria-hidden="true">+</span><span aria-hidden="true">:)</span>
                    </button>
                    <button id="idea-panel-copy" class="idea-panel-copy-btn" type="button" aria-label="Use this idea as a starting point" title="Use this idea as a starting point" hidden>
                        <svg class="idea-panel-copy-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                            <rect x="9" y="9" width="13" height="13" rx="2" ry="2"></rect>
                            <path d="M5 15H4a2 2 0 0 1-2-2V4a2 2 0 0 1 2-2h9a2 2 0 0 1 2 2v1"></path>
                        </svg>
                        <span>Use as starter</span>
                    </button>
                    <button id="idea-panel-edit-toggle" class="survey-magic-btn idea-panel-edit-cta" type="button" aria-label="Edit idea before publish" hidden>
                        <svg class="w-4 h-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
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
    <div class="safety-review-header"><h3>Let's keep this space safe</h3></div>
    <div class="safety-review-body">
        <p class="safety-review-copy">Our AI flagged your text as potentially offensive. You can use the suggestion, edit it, or continue with your original text.</p>
        <div class="safety-review-block">
            <div class="safety-review-block-head">
                <span class="safety-review-label">Your original message</span>
                <button id="safety-review-edit-original" class="safety-review-edit-icon" type="button" aria-label="Edit your response">
                    <svg class="safety-review-edit-glyph" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
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
                <button id="safety-review-edit-suggestion" class="safety-review-edit-icon" type="button" aria-label="Edit the AI suggestion">
                    <svg class="safety-review-edit-glyph" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
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
</div>`

export async function renderChatSurveyPage(
    container: HTMLElement,
    params: ProjectContext,
    project: Project,
): Promise<void> {
    const t = getSurveyStrings()
    const projectSlugKey = params.projectSlug
    const completedKey = `survey-completed-${projectSlugKey}`

    if (localStorage.getItem(completedKey) === 'true') {
        clearSurveyProgress(projectSlugKey)
        container.innerHTML = `
            <div class="survey-redirect-wrap screen-height">
                <div class="survey-redirect-card">
                    <div class="survey-redirect-check">✓</div>
                    <h2>Survey already completed</h2>
                    <p>Redirecting you to ideas...</p>
                    <div class="survey-confetti" aria-hidden="true"></div>
                </div>
            </div>`
        const timer = window.setTimeout(() => void navigate('ideas'), 3200)
        window.addEventListener('app:before-navigate', () => window.clearTimeout(timer), { once: true })
        return
    }

    const questions = await getQuestions(params.organizationSlug, params.projectSlug)
    const orgName = project.organizationName?.trim() || project.organizationSlug
    const headerHTML = renderSurveyHeader({ organizationName: orgName, organizationSlug: project.organizationSlug })

    container.innerHTML = `
        <div class="chat-shell" id="chat-shell">
            <div class="survey-header" id="chat-survey-header">
                <div class="survey-header-content">
                    <h2 class="survey-title">${esc(project.title)}</h2>
                    <div class="survey-progress-container">
                        <div class="survey-progress-bar">
                            <div class="survey-progress-fill" id="progress-bar"></div>
                        </div>
                        <span class="survey-progress-badge" id="progress-badge">0 / ${questions.length}</span>
                    </div>
                </div>
            </div>
            <div class="chat-scroll-area" id="chat-scroll-area">
                ${headerHTML}
                <div class="chat-messages" id="chat-messages"></div>
            </div>
            <div class="chat-input-wrap">
                <div class="chat-input-bar">
                    <textarea
                        id="chat-input"
                        class="chat-input"
                        placeholder="${esc(t.selectAbove)}"
                        rows="1"
                        disabled
                    ></textarea>
                    <button id="chat-magic-btn" class="survey-magic-btn chat-magic-btn" type="button" aria-label="${esc(t.magicMode)}" hidden>
                        ${MAGIC_SVG}
                        <span class="survey-magic-btn-text">${esc(t.magicMode)}</span>
                    </button>
                    <button id="chat-confirm-inline-btn" class="chat-confirm-inline-btn" type="button" aria-label="Confirm answer and continue" hidden>
                        ${CHECKMARK_SVG}
                    </button>
                    <button id="chat-send-btn" class="chat-send-btn" type="button" aria-label="Send" disabled>
                        <svg class="chat-mic-icon" viewBox="0 0 24 24" fill="currentColor" aria-hidden="true">
                            <path d="M12 14c1.66 0 2.99-1.34 2.99-3L15 5c0-1.66-1.34-3-3-3S9 3.34 9 5v6c0 1.66 1.34 3 3 3zm5.3-3c0 3-2.54 5.1-5.3 5.1S6.7 14 6.7 11H5c0 3.41 2.72 6.23 6 6.72V21h2v-3.28c3.28-.48 6-3.3 6-6.72h-1.7z"/>
                        </svg>
                        <svg class="chat-send-icon chat-icon-hidden" viewBox="0 0 24 24" fill="currentColor" aria-hidden="true">
                            <path d="M2.01 21L23 12 2.01 3 2 10l15 2-15 2z"/>
                        </svg>
                    </button>
                </div>
            </div>
        </div>
        ${IDEATION_MODALS_HTML}`

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
    let openTextState: OpenTextState | null = null
    let activeConfirmIndex: number | null = null
    let activeSendHandler: (() => void | Promise<void>) | null = null

    // ===== Progress =====
    function updateProgress(): void {
        const count = answeredState.filter(Boolean).length
        headerController.updateProgress(count, questions.length)
    }

    function persistProgress(confirmedUpToIndex: number): void {
        const byId = new Map<number, QuestionAnswer>(
            questions.map((q, i) => [q.id, components[i].getAnswer()] as const),
        )
        saveSurveyProgress(projectSlugKey, questions, confirmedUpToIndex, byId)
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
        updateProgress()
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
        openTextState = { questionIndex, messages: [], floatingConfirmRow: null }
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

            const answers: ResponseAnswer[] = questions.flatMap((q, i) => {
                const answer = components[i].getAnswer()
                if (q.type === QuestionType.SingleChoice) {
                    const id = answer as number
                    return id == null ? [] : [{ questionId: q.id, selectedOptionId: id, value: id }]
                }
                if (q.type === QuestionType.MultipleChoice) {
                    const ids = Array.isArray(answer) ? answer : []
                    return ids.map((id) => ({ questionId: q.id, selectedOptionId: id, value: id }))
                }
                if (q.type === QuestionType.Scale) {
                    const val = answer as number
                    return val == null ? [] : [{ questionId: q.id, selectedOptionId: val, value: val }]
                }
                const text = answer as string
                return text?.trim() ? [{ questionId: q.id, openTextValue: text, value: text }] : []
            })

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

        function getCurrentIdeas(): Idea[] {
            if (currentView.type === 'my-ideas') {
                return allIdeas.filter((x) => x.authorType === 'self')
            }
            const topicId = currentView.topicId
            const hasOwn = hasOwnIdeaInTopic(topicId)
            let filtered = allIdeas.filter((x) => x.topicId === topicId)

            if (!hasOwn) {
                if (currentSemanticCategory) {
                    filtered = filtered.filter((x) => x.semanticCategories.includes(currentSemanticCategory!))
                }
            } else {
                if (currentDiscoveryMode === 'different') {
                    filtered = [...filtered].reverse()
                }
            }
            return filtered.slice(0, 20)
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
                    <button class="ideas-discovery-option${currentDiscoveryMode === 'similar' || currentDiscoveryMode === 'broad' ? ' selected' : ''}" data-chat-sort="similar" role="menuitem" type="button">${esc(t.similarIdeas)}</button>
                    <button class="ideas-discovery-option${currentDiscoveryMode === 'different' ? ' selected' : ''}" data-chat-sort="different" role="menuitem" type="button">${esc(t.differingIdeas)}</button>
                    <button class="ideas-discovery-option${currentDiscoveryMode === 'all' ? ' selected' : ''}" data-chat-sort="all" role="menuitem" type="button">${esc(t.allIdeas)}</button>`
            }
        }

        function updateDiscoveryLabel(): void {
            discoveryLabel.textContent = currentDiscoveryLabel
        }

        function renderIdeasList(): void {
            if (listController) {
                listController.cleanup()
                listController = null
            }
            const displayIdeas = getCurrentIdeas()
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
                renderIdeasList()
                deactivateInput(t.selectTopicToShare)
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
            renderIdeasList()
            void appendAiBubble(newTopic.prompt?.trim() || `What are your thoughts on: "${newTopic.title}"?`)
            activateIdeaInput(t.shareIdea, handleIdeaSubmit)
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
            renderIdeasList()
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
        ideasListEl.addEventListener('click', (e) => {
            const card = (e.target as HTMLElement).closest<HTMLElement>('.ideas-card')
            if (!card || !listController) return
            const idx = Number(card.getAttribute('data-original-index'))
            const displayIdeas = getCurrentIdeas()
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
                renderIdeasList()
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

            deactivateInput(t.submitting)
            appendUserBubble(text)

            try {
                await submitHandler.submit(text, currentView)
            } catch {
                await appendAiBubble(t.somethingWrong)
            } finally {
                if (currentView.type === 'topic') {
                    activateIdeaInput(t.shareAnother, handleIdeaSubmit)
                }
            }
        }

        // Initial render
        renderDiscoveryMenuOptions()
        renderIdeasList()

        await appendAiBubble(t.ideationIntro)
        await wait(200)
        await appendAiBubble(firstTopic.prompt?.trim() || `What are your thoughts on: "${firstTopic.title}"?`)

        activateIdeaInput(t.shareIdea, handleIdeaSubmit)
    }

    // ===== Start conversation =====
    const savedProgress = loadSurveyProgress(projectSlugKey, questions)

    if (savedProgress && savedProgress.currentQuestionIndex > 0 && questions.length > 0) {
        const resumeAt = Math.min(savedProgress.currentQuestionIndex, questions.length)

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
