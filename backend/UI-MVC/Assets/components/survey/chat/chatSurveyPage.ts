import '../../../styles/pages/chat-survey.css'
import '../../../styles/pages/ideas.css'
import type { Project } from '../../../models/project'
import type { ProjectContext } from '../../../main'
import { navigate } from '../../../main'
import { getQuestions, submitAnswers } from '../../../services/surveyService'
import { clearSurveyProgress } from '../../../services/surveyProgressService'
import { QuestionType } from '../../../models/question'
import type { ResponseAnswer } from '../../../models/response'
import type { QuestionAnswer, QuestionComponent } from '../singleChoiceQuestion'
import { renderSingleChoiceQuestion } from '../singleChoiceQuestion'
import { renderMultipleChoiceQuestion } from '../multipleChoiceQuestion'
import { renderOpenTextQuestion } from '../openTextQuestion'
import { renderScaleQuestion } from '../scaleQuestion'
import { renderSurveyHeader } from '../surveyHeader'
import {
    getIdeasContext,
    getOrCreateProjectScopedYouthId,
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
import type { Idea, IdeaTopic } from '../../../models/idea'
import type { ActiveView } from '../../ideas/types'

interface OpenTextState {
    questionIndex: number
    messages: string[]
    floatingConfirmRow: HTMLElement | null
}

const AI_AVATAR = `<svg viewBox="0 0 36 36" fill="none" xmlns="http://www.w3.org/2000/svg">
  <circle cx="18" cy="18" r="18" fill="var(--color-primary)"/>
  <circle cx="18" cy="14" r="5" fill="white" fill-opacity="0.9"/>
  <path d="M6 32c0-5.523 5.373-9 12-9s12 3.477 12 9" fill="white" fill-opacity="0.9"/>
</svg>`

const SPEAKER_SVG = `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
  <polygon points="11 5 6 9 2 9 2 15 6 15 11 19 11 5"/>
  <path d="M15.54 8.46a5 5 0 0 1 0 7.07"/>
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
</div>`

export async function renderChatSurveyPage(
    container: HTMLElement,
    params: ProjectContext,
    project: Project,
): Promise<void> {
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
        const t = window.setTimeout(() => void navigate('ideas'), 3200)
        window.addEventListener('app:before-navigate', () => window.clearTimeout(t), { once: true })
        return
    }

    const questions = await getQuestions(params.organizationSlug, params.projectSlug)
    const orgName = project.organizationName?.trim() || project.organizationSlug
    const headerHTML = renderSurveyHeader({ organizationName: orgName, organizationSlug: project.organizationSlug })

    container.innerHTML = `
        <div class="chat-shell" id="chat-shell">
            ${headerHTML}
            <div class="chat-progress-strip" id="chat-progress-strip">
                <div class="chat-progress-track">
                    <div class="chat-progress-fill" id="chat-progress-fill"></div>
                </div>
                <span class="chat-progress-label" id="chat-progress-label">0 / ${questions.length}</span>
            </div>
            <div class="chat-messages" id="chat-messages"></div>
            <div class="chat-input-wrap">
                <div class="chat-input-bar">
                    <textarea
                        id="chat-input"
                        class="chat-input"
                        placeholder="Select your answer above..."
                        rows="1"
                        disabled
                    ></textarea>
                    <button id="chat-magic-btn" class="survey-magic-btn chat-magic-btn" type="button" aria-label="Answer in Magic Mode" hidden>
                        ${MAGIC_SVG}
                        <span class="survey-magic-btn-text">Magic</span>
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
    const messagesEl = container.querySelector<HTMLDivElement>('#chat-messages')!
    const progressFill = container.querySelector<HTMLDivElement>('#chat-progress-fill')!
    const progressLabel = container.querySelector<HTMLSpanElement>('#chat-progress-label')!
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
    let activeSendHandler: (() => void | Promise<void>) | null = null

    // ===== Progress =====
    function updateProgress(): void {
        const count = answeredState.filter(Boolean).length
        const pct = questions.length > 0 ? (count / questions.length) * 100 : 0
        progressFill.style.width = `${pct}%`
        progressLabel.textContent = `${count} / ${questions.length}`
    }

    // ===== Scroll =====
    function scrollToBottom(): void {
        messagesEl.scrollTo({ top: messagesEl.scrollHeight, behavior: 'smooth' })
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
    }

    async function appendAiBubble(text: string, options: AiBubbleOptions = {}): Promise<void> {
        const { animated = true, bubbleClass, questionNum } = options
        if (animated) {
            showTyping()
            await wait(650 + Math.min(text.length * 7, 850))
            hideTyping()
        }
        const numHtml = questionNum != null
            ? `<span class="chat-question-num">${questionNum}.</span> `
            : ''
        const extraClass = bubbleClass ? ` ${bubbleClass}` : ''
        const row = document.createElement('div')
        row.className = 'chat-row chat-row--ai'
        row.innerHTML = `
            <div class="chat-avatar">${AI_AVATAR}</div>
            <div class="chat-bubble-group">
                <div class="chat-bubble chat-bubble--ai${extraClass}">${numHtml}${esc(text)}</div>
                <button class="chat-speaker-btn" type="button" disabled aria-label="Read aloud">${SPEAKER_SVG}</button>
            </div>`
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
            await appendAiBubble('Please type your answer before continuing.', { animated: false })
            return
        }

        const bundled = openTextState.messages.join('\n\n')
        components[index].setAnswer(bundled || null)

        openTextState.floatingConfirmRow?.classList.add('chat-confirm-row--confirmed')
        answeredState[index] = bundled.trim().length > 0
        updateProgress()
        openTextState = null
        deactivateInput()

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
        chatInput.placeholder = questions[questionIndex].hint?.trim() || 'Type your answer here...'
        chatInput.value = ''
        chatInput.style.height = 'auto'
        sendBtn.disabled = false
        magicBtn.hidden = false
        confirmInlineBtn.hidden = false
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
        confirmInlineBtn.hidden = true
        activeSendHandler = handler
        updateSendIcon()
        setTimeout(() => chatInput.focus(), 50)
    }

    function deactivateInput(placeholder = 'Select your answer above...'): void {
        activeSendHandler = null
        chatInput.disabled = true
        chatInput.value = ''
        chatInput.style.height = 'auto'
        chatInput.placeholder = placeholder
        sendBtn.disabled = true
        magicBtn.hidden = true
        confirmInlineBtn.hidden = true
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
        }
    })

    // ===== Confirm question (non-open-text) =====
    async function confirmQuestion(index: number): Promise<void> {
        if (!components[index].validate()) {
            await appendAiBubble('Please fill in your answer before continuing.')
            return
        }

        const confirmRow = messagesEl.querySelector<HTMLElement>(`[data-confirm-for="${index}"]`)
        confirmRow?.classList.add('chat-confirm-row--confirmed')

        answeredState[index] = true
        updateProgress()

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
        })

        if (q.hint?.trim()) {
            await wait(150)
            await appendAiBubble(q.hint.trim())
        }

        if (q.type === QuestionType.OpenText) {
            const hintRow = document.createElement('div')
            hintRow.className = 'chat-row chat-row--ai chat-row--hint'
            hintRow.innerHTML = `
                <div class="chat-avatar chat-avatar--spacer"></div>
                <p class="chat-open-text-hint">Type your answer in the chat bar below</p>`
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

        deactivateInput('Select your answer above...')
    }

    // ===== Submit section =====
    async function showSubmitSection(): Promise<void> {
        await appendAiBubble("You've answered all the questions — well done! Ready to submit your responses?")

        const submitRow = document.createElement('div')
        submitRow.className = 'chat-submit-row'
        submitRow.innerHTML = `<button class="chat-submit-btn" id="chat-survey-submit" type="button">Submit Survey</button>`
        messagesEl.appendChild(submitRow)
        scrollToBottom()

        submitRow.querySelector<HTMLButtonElement>('#chat-survey-submit')!.addEventListener('click', async () => {
            const btn = submitRow.querySelector<HTMLButtonElement>('#chat-survey-submit')!
            btn.disabled = true
            btn.textContent = 'Submitting...'

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
                await enterIdeasPhase()
            } catch {
                btn.disabled = false
                btn.textContent = 'Submit Survey'
                await appendAiBubble('Sorry, something went wrong. Please try again.')
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

        const progressStrip = container.querySelector<HTMLElement>('#chat-progress-strip')!
        progressStrip.hidden = true

        let currentTopicId = firstTopic.id
        let currentSort: 'all' | 'similar' | 'different' = 'all'

        // Build topic selector HTML
        const topicOptions = topics
            .map(
                (t) =>
                    `<li class="chat-topic-option${t.id === firstTopic.id ? ' chat-topic-option--active' : ''}" data-topic-id="${t.id}" role="option" aria-selected="${t.id === firstTopic.id}">${esc(t.title)}</li>`,
            )
            .join('')

        const topicSelectorEl = document.createElement('div')
        topicSelectorEl.className = 'chat-topic-selector'
        topicSelectorEl.id = 'chat-topic-selector'
        topicSelectorEl.innerHTML = `
            <button class="chat-topic-trigger" id="chat-topic-trigger" type="button" aria-expanded="false" aria-haspopup="listbox">
                <span class="chat-topic-trigger-label">Topic</span>
                <span class="chat-topic-trigger-name" id="chat-topic-trigger-name">${esc(firstTopic.title)}</span>
                <svg class="chat-topic-trigger-chevron" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                    <polyline points="6 9 12 15 18 9"/>
                </svg>
            </button>
            <ul class="chat-topic-dropdown" id="chat-topic-dropdown" role="listbox" hidden>
                ${topicOptions}
            </ul>`

        const ideasArea = document.createElement('div')
        ideasArea.className = 'chat-ideas-area'
        ideasArea.innerHTML = `
            <div class="chat-ideas-area-header">
                <span class="chat-ideas-area-title">Community ideas</span>
                <div class="chat-sort-tabs" id="chat-sort-tabs">
                    <button class="chat-sort-tab chat-sort-tab--active" data-sort="all" type="button">All</button>
                    <button class="chat-sort-tab" data-sort="similar" type="button">Similar</button>
                    <button class="chat-sort-tab" data-sort="different" type="button">Different</button>
                </div>
            </div>
            <div class="ideas-list" id="chat-ideas-list" aria-live="polite"></div>`

        chatShell.classList.add('chat-shell--ideas')
        chatShell.insertBefore(topicSelectorEl, messagesEl)
        chatShell.insertBefore(ideasArea, messagesEl)

        const ideasListEl = container.querySelector<HTMLDivElement>('#chat-ideas-list')!
        const flaggedIdeaIds = new Set<number>()

        const safetyDialog = createSafetyReviewDialogController({ root: container })

        let listController: ReturnType<typeof createIdeasListController> | null = null

        const ideaPanel = createIdeaPanelController({
            root: container,
            reviewBeforePost: (input) => safetyDialog.reviewBeforePost(input),
            reviewWithSuggestion: (orig, sugg) => safetyDialog.reviewWithSuggestion(orig, sugg),
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
                removeResponseReaction(
                    params.organizationSlug,
                    params.projectSlug,
                    idea,
                    rid,
                    youthToken,
                    emoji,
                ),
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

        function renderIdeasList(): void {
            if (listController) {
                listController.cleanup()
                listController = null
            }
            const currentActiveView: ActiveView = { type: 'topic', topicId: currentTopicId }
            let topicIdeas = allIdeas.filter((x) => x.topicId === currentTopicId).slice(0, 20)
            // Placeholder ordering — Similar/Different will use AI ranking in the future
            if (currentSort === 'different') {
                topicIdeas = topicIdeas.slice().reverse()
            }
            if (topicIdeas.length === 0) {
                ideasListEl.innerHTML = `<p class="ideas-empty-state">No ideas shared yet. Be the first!</p>`
                return
            }
            listController = createIdeasListController({
                list: ideasListEl,
                ideas: topicIdeas,
                activeView: currentActiveView,
                topics,
                flaggedIdeaIds,
            })
            listController.startRotation()
        }

        // Topic selector interactions
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
            const opt = (e.target as HTMLElement).closest<HTMLElement>('[data-topic-id]')
            if (!opt) return

            const newTopicId = Number(opt.getAttribute('data-topic-id'))
            if (newTopicId === currentTopicId) {
                topicDropdown.hidden = true
                topicTrigger.setAttribute('aria-expanded', 'false')
                return
            }

            const newTopic = topics.find((t) => t.id === newTopicId)
            if (!newTopic) return

            currentTopicId = newTopicId
            topicTriggerName.textContent = newTopic.title
            topicDropdown.hidden = true
            topicTrigger.setAttribute('aria-expanded', 'false')

            topicDropdown.querySelectorAll('[data-topic-id]').forEach((el) => {
                const isActive = Number(el.getAttribute('data-topic-id')) === newTopicId
                el.classList.toggle('chat-topic-option--active', isActive)
                el.setAttribute('aria-selected', String(isActive))
            })

            renderIdeasList()
            void appendAiBubble(newTopic.prompt?.trim() || `What are your thoughts on: "${newTopic.title}"?`)
        })

        const closeDropdownOnOutsideClick = (e: MouseEvent): void => {
            if (!topicSelectorEl.contains(e.target as Node)) {
                topicDropdown.hidden = true
                topicTrigger.setAttribute('aria-expanded', 'false')
            }
        }
        document.addEventListener('click', closeDropdownOnOutsideClick)

        // Sort tabs
        const sortTabsEl = ideasArea.querySelector<HTMLElement>('#chat-sort-tabs')!
        sortTabsEl.addEventListener('click', (e) => {
            const btn = (e.target as HTMLElement).closest<HTMLElement>('[data-sort]')
            if (!btn) return
            currentSort = btn.getAttribute('data-sort') as 'all' | 'similar' | 'different'
            sortTabsEl.querySelectorAll('.chat-sort-tab').forEach((el) => {
                el.classList.toggle('chat-sort-tab--active', el.getAttribute('data-sort') === currentSort)
            })
            renderIdeasList()
        })

        // Ideas list click → open panel
        ideasListEl.addEventListener('click', (e) => {
            const card = (e.target as HTMLElement).closest<HTMLElement>('.ideas-card')
            if (!card || !listController) return
            const idx = Number(card.getAttribute('data-original-index'))
            const topicIdeas = allIdeas.filter((x) => x.topicId === currentTopicId).slice(0, 20)
            if (!Number.isFinite(idx) || idx < 0 || idx >= topicIdeas.length) return
            listController.setActive(idx, true)
            ideaPanel.open(topicIdeas[idx])
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
                document.removeEventListener('click', closeDropdownOnOutsideClick)
            },
            { once: true },
        )

        renderIdeasList()

        await appendAiBubble("You've completed the survey — thank you! Now let's share ideas with the community.")
        await wait(200)
        await appendAiBubble(firstTopic.prompt?.trim() || `What are your thoughts on: "${firstTopic.title}"?`)

        const submitHandler = createIdeasSubmitHandler({
            organizationSlug: params.organizationSlug,
            projectSlug: params.projectSlug,
            projectId: project.id,
            reviewBeforePost: (input) => safetyDialog.reviewBeforePost(input),
            reviewWithSuggestion: (orig, sugg) => safetyDialog.reviewWithSuggestion(orig, sugg),
            onIdeaSubmitted: (idea: Idea) => {
                allIdeas.unshift(idea)
                renderIdeasList()
                void appendAiBubble('Your idea has been shared with the community!')
            },
        })

        async function handleIdeaSubmit(): Promise<void> {
            const text = chatInput.value.trim()
            if (!text) return

            deactivateInput('Submitting...')
            appendUserBubble(text)

            try {
                await submitHandler.submit(text, { type: 'topic', topicId: currentTopicId })
            } catch {
                await appendAiBubble('Sorry, something went wrong. Please try again.')
            } finally {
                activateIdeaInput('Share another idea...', handleIdeaSubmit)
            }
        }

        activateIdeaInput('Share your idea...', handleIdeaSubmit)
    }

    // ===== Start conversation =====
    await appendAiBubble(project.title, { animated: false, bubbleClass: 'chat-bubble--project-title' })
    await wait(300)
    await appendAiBubble(project.description)
    await wait(1500)

    if (questions.length > 0) {
        await revealQuestion(0)
    } else {
        await appendAiBubble('There are no questions for this survey yet.')
    }
}
