import '../../../styles/pages/chat-survey.css'
import '../../../styles/pages/ideas.css'
import type { Project } from '../../../models/project'
import type { ProjectContext } from '../../../main'
import { getQuestions, submitAnswers } from '../../../services/surveyService'
import { clearSurveyProgress, loadSurveyProgress, saveSurveyProgress } from '../../../services/surveyProgressService'
import {FixedQuestion, OpenQuestion, QuestionType, RangeQuestion} from '../../../models/question.ts'
import type { QuestionAnswer, QuestionComponent } from '../../survey/components/singleChoiceQuestion'
import { renderSingleChoiceQuestion } from '../../survey/components/singleChoiceQuestion'
import { renderMultipleChoiceQuestion } from '../../survey/components/multipleChoiceQuestion'
import { renderOpenTextQuestion } from '../../survey/components/openTextQuestion'
import { renderScaleQuestion } from '../../survey/components/scaleQuestion'
import { renderSurveyHeader, createSurveyHeaderController } from '../../survey/components/surveyHeader'
import {
    updateIdeaAfterSafetyReview, saveYouthContactEmail,
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
import { createIdeasListController } from '../../ideas/components/ideasListController'
import { createSafetyReviewDialogController } from '../../ideas/components/safetyReviewDialog'
import { createIdeaPanelController } from '../../ideas/components/ideaPanel'
import { createIdeasSubmitHandler } from '../../ideas/components/ideasSubmitHandler'
import { createFirstIdeaContactDialogController } from '../../ideas/components/firstIdeaContactDialog'
import { createTopicModalController } from '../../ideas/components/topicModal'
import { createChatIdeaNudgeFlow } from '../../ideas/components/chatIdeaNudgeFlow'
import {Idea, IdeaAuthorType} from '../../../models/idea'
import type {IdeaNudgingContext} from '../../../services/ideaService'
import type { ActiveView, DiscoveryFeed } from '../../ideas/types'
import { DiscoveryMode, DiscoveryBadgeType } from '../../ideas/types'
import { createDiscoveryFeed, getTopicSemanticCategories as getTopicSemanticCats } from '../../ideas/utils/ideasDiscovery'
import { getVisibleIdeas, type DiscoveryOptions } from '../../ideas/utils/discoveryApi'
import { initIdeasContext, type IdeasInitResult } from '../../ideas/utils/ideasInit'
import { getOrganizationBadge } from '../../shared/organizationBranding.ts'
import { getSurveyStrings } from '../../../i18n/survey'
import { bindChatIdeasDesktopLayout, formatAnswerForDisplay, hasAnswer, mapAnswersToResponse, wait, esc } from '../utils/chatHelpers.ts'
import {
    CHECKMARK_SVG,
    SPEAKER_SVG,
    renderChatShellTemplate,
    avatarHTML,
} from '../utils/chatTemplates.ts'
import { getSTTManager, createSpeakerButton, getSpeechLanguage, type SpeakerButtonController } from '../../../services/speechService'
import { wireBrainstormButton, type BrainstormModalController } from '../../shared/brainstormMode'

interface OpenTextState {
    questionIndex: number
    messages: string[]
    floatingConfirmRow: HTMLElement | null
}

const IDEAS_BATCH_SIZE = 7
const LOAD_MORE_SCROLL_THRESHOLD = 150

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
        // Clear saved history HTML so it gets rebuilt with current locale strings
        const surveyHistoryKey = `survey-history-${projectSlugKey}`
        localStorage.removeItem(surveyHistoryKey)
    } else {
        // Clear saved survey history when starting a new survey
        const surveyHistoryKey = `survey-history-${projectSlugKey}`
        localStorage.removeItem(surveyHistoryKey)
    }

    const questions = await getQuestions(params.organizationSlug, params.projectSlug)
    const orgName = project.organizationName?.trim() || project.organizationSlug
    const workspaceBadge = getOrganizationBadge(orgName, project.organizationSlug)
    const workspaceLogo = project.organizationLogo
    const headerHTML = renderSurveyHeader({ organizationName: orgName, organizationSlug: project.organizationSlug, organizationLogo: workspaceLogo })

    let inIdeasPhase = false
    let activeViewForBrainstorm: ActiveView | null = null
    let topicsForBrainstorm: { id: number; prompt?: string | null }[] = []

    container.innerHTML = renderChatShellTemplate({
        projectTitle: project.title,
        questionsCount: questions.length,
        headerHTML,
        t,
    })

    const chatShell = container.querySelector<HTMLDivElement>('#chat-shell')!
    const cleanupDesktopLayout = bindChatIdeasDesktopLayout(chatShell)
    if (project.imageUrl) {
        chatShell.style.setProperty('--project-bg', `url(${project.imageUrl})`);
    } else {
        chatShell.style.setProperty('--project-bg', 'none');
        chatShell.style.backgroundColor = 'white';
    }
    const scrollAreaEl = container.querySelector<HTMLDivElement>('#chat-scroll-area')!
    const messagesEl = container.querySelector<HTMLDivElement>('#chat-messages')!
    const headerController = createSurveyHeaderController({ root: container })
    const chatInput = container.querySelector<HTMLTextAreaElement>('#chat-input')!
    const brainstormBtn = container.querySelector<HTMLButtonElement>('#chat-brainstorm-btn')!
    const confirmInlineBtn = container.querySelector<HTMLButtonElement>('#chat-confirm-inline-btn')!
    const sendBtn = container.querySelector<HTMLButtonElement>('#chat-send-btn')!
    const micIcon = sendBtn.querySelector<SVGElement>('.chat-mic-icon')!
    const sendIcon = sendBtn.querySelector<SVGElement>('.chat-send-icon')!
    const scrollBottomBtn = container.querySelector<HTMLButtonElement>('#chat-scroll-bottom')!

    const components: QuestionComponent[] = questions.map((q, i) =>
        q.type === QuestionType.SingleChoice
            ? renderSingleChoiceQuestion(q as FixedQuestion, i)
            : q.type === QuestionType.MultipleChoice
            ? renderMultipleChoiceQuestion(q as FixedQuestion, i)
            : q.type === QuestionType.Scale
            ? renderScaleQuestion(q as RangeQuestion, i)
            : renderOpenTextQuestion(q as OpenQuestion, i),
    )

    const answeredState = new Array<boolean>(questions.length).fill(false)
    const openTextDraftsByQuestionId = new Map<number, string[]>()
    let confirmedUpToIndex = 0
    let openTextState: OpenTextState | null = null
    let activeConfirmIndex: number | null = null
    let activeSendHandler: (() => void | Promise<void>) | null = null
    let editingBubble: HTMLElement | null = null
    let editingPrevSendHandler: (() => void | Promise<void>) | null = null    
    let handleIdeaSubmit: () => Promise<void> = async () => { return }

    const brainstormController: BrainstormModalController = wireBrainstormButton(brainstormBtn, {
        getQuestionText: () => {
            const av = activeViewForBrainstorm
            if (inIdeasPhase && av && av.type === 'topic') {
                const topic = topicsForBrainstorm?.find((item) => item.id === av.topicId)
                if (topic?.prompt?.trim()) return topic.prompt.trim()
                return chatInput.placeholder
            }
            if (openTextState && openTextState.questionIndex < questions.length) {
                return questions[openTextState.questionIndex].text ?? ''
            }
            return chatInput.placeholder
        },
        onResult: (finalText: string) => {
            if (finalText.trim()) {
                chatInput.value = finalText
                chatInput.style.height = 'auto'
                chatInput.style.height = chatInput.scrollHeight + 'px'
                chatInput.dispatchEvent(new Event('input', { bubbles: true }))
                if (activeSendHandler) activeSendHandler()
            }
        }
    })

    // ===== Edit handler (must be defined early for use in sendOpenTextMessage) =====
    const createEditHandler = (questionIndex: number, bubbleElement: HTMLElement) => () => {
        
        // Cancel any current edit before starting new one
        cancelCurrentEdit()

        // Populate chat input with bubble text
        const currentText = bubbleElement.textContent?.replace('\u200B', '').trim() || ''
        chatInput.value = currentText
        chatInput.style.height = 'auto'
        chatInput.style.height = `${Math.min(chatInput.scrollHeight, 120)}px`
        
        // Visual: mark bubble as being edited
        bubbleElement.classList.add('chat-bubble--editing')
        
        // Track editing state
        editingBubble = bubbleElement
        const prevSendHandler = activeSendHandler
        editingPrevSendHandler = prevSendHandler
        
        // Set send handler to save edit back to bubble (no AI response)
        activeSendHandler = () => {
            const newText = chatInput.value.trim()
            if (!newText) {
                cancelBubbleEdit(bubbleElement, prevSendHandler)
                return
            }
            
            // Update bubble display
            bubbleElement.textContent = newText
            bubbleElement.classList.remove('chat-bubble--editing')
            editingBubble = null
            editingPrevSendHandler = null
            // Update component answer
            components[questionIndex].setAnswer(newText || null)
            answeredState[questionIndex] = true
            const qId = questions[questionIndex].id!
            openTextDraftsByQuestionId.set(qId, [newText])
            
            // Update openTextState.messages if editing current active question
            if (openTextState && openTextState.questionIndex === questionIndex) {
                openTextState.messages = [newText]
            }
            
            updateProgress()
            persistProgress()
            
            // Clear input
            chatInput.value = ''
            chatInput.style.height = 'auto'
            updateSendIcon()
            
            // Restore send handler
            activeSendHandler = prevSendHandler
            if (!prevSendHandler) {
                deactivateInput()
            }
        }
        
        chatInput.disabled = false
        sendBtn.disabled = false
        brainstormBtn.hidden = false
        updateSendIcon()
        
        // Only show inline confirm for the current active question
        if (openTextState && openTextState.questionIndex === questionIndex) {
            showInlineConfirm(questionIndex)
        }
        
        // Ensure confirm row is not in confirmed state
        const confirmRow = messagesEl.querySelector<HTMLElement>(`[data-confirm-for="${questionIndex}"]`)
        if (confirmRow) {
            confirmRow.classList.remove('chat-confirm-row--confirmed')
        }
        
        setTimeout(() => chatInput.focus(), 50)
    }
    const bubbleSpeakerControllers: SpeakerButtonController[] = []
    let isChatRecording = false
    
    function cancelBubbleEdit(bubbleElement: HTMLElement, prevSendHandler: (() => void | Promise<void>) | null): void {
        bubbleElement.classList.remove('chat-bubble--editing')
        editingBubble = null
        editingPrevSendHandler = null
        chatInput.value = ''
        chatInput.style.height = 'auto'
        updateSendIcon()
        activeSendHandler = prevSendHandler
        if (!prevSendHandler) {
            deactivateInput()
        }
    }

    function cancelCurrentEdit(): void {
        if (!editingBubble) return
        const bubble = editingBubble
        const prev = editingPrevSendHandler
        cancelBubbleEdit(bubble, prev)
    }

    scrollAreaEl.addEventListener('mousedown', (e) => {
        if (!editingBubble) return
        if (editingBubble.contains(e.target as Node)) return
        cancelCurrentEdit()
    })

    // ===== Progress =====
    function updateProgress(): void {
        const count = answeredState.filter(Boolean).length
        headerController.updateProgress(count, questions.length)
    }

    function persistProgress(nextConfirmedUpToIndex: number = confirmedUpToIndex): void {
        confirmedUpToIndex = nextConfirmedUpToIndex
        const byId = new Map<number, QuestionAnswer>(
            questions.map((q, i) => [q.id!, components[i].getAnswer()] as const),
        )
        saveSurveyProgress(projectSlugKey, questions, confirmedUpToIndex, byId, {
            openTextDraftsByQuestionId,
        })
    }

    // ===== Scroll =====
    function scrollToBottom(): void {
        scrollAreaEl.scrollTo({ top: scrollAreaEl.scrollHeight, behavior: 'smooth' })
    }

    function updateScrollBottomButton(): void {
        const threshold = scrollAreaEl.clientHeight * 0.5
        const distFromBottom = scrollAreaEl.scrollHeight - scrollAreaEl.clientHeight - scrollAreaEl.scrollTop
        scrollBottomBtn.classList.toggle('chat-scroll-bottom--visible', distFromBottom > threshold)
    }

    scrollAreaEl.addEventListener('scroll', updateScrollBottomButton, { passive: true })
    scrollBottomBtn.addEventListener('click', () => scrollToBottom())

    // ===== Typing indicator =====
    function showTyping(): void {
        if (messagesEl.querySelector('#chat-typing-indicator')) return
        const row = document.createElement('div')
        row.id = 'chat-typing-indicator'
        row.className = 'chat-row chat-row--ai'
        row.innerHTML = `
            <div class="chat-avatar">
                ${avatarHTML(workspaceBadge, inIdeasPhase, workspaceLogo)}
            </div>
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
        prefix?: string
    }

    async function appendAiBubble(text: string, options: AiBubbleOptions = {}): Promise<void> {
        const { animated = true, bubbleClass, questionNum, required, prefix } = options
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

        if (prefix) {
            const prefixEl = document.createElement('strong')
            prefixEl.className = 'chat-bubble-prefix'
            prefixEl.textContent = prefix
            bubbleEl.appendChild(prefixEl)
        }

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
        speakerBtn.setAttribute('aria-label', t.readAloud)
        speakerBtn.innerHTML = SPEAKER_SVG
        const speakerController = createSpeakerButton(speakerBtn, () => text, getSpeechLanguage)
        bubbleSpeakerControllers.push(speakerController)

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
        avatarDiv.innerHTML = avatarHTML(workspaceBadge, inIdeasPhase, workspaceLogo)

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
        if (isChatRecording) {
            micIcon.classList.remove('chat-icon-hidden')
            sendIcon.classList.add('chat-icon-hidden')
            return
        }
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
 
         // Append bubble with click handler to allow editing
         const row = document.createElement('div')
         row.className = 'chat-row chat-row--user'
         const bubble = document.createElement('div')
         bubble.className = 'chat-bubble chat-bubble--user chat-bubble--editable'
         bubble.textContent = text
         
         // Add click handler for inline editing
         bubble.addEventListener('click', createEditHandler(openTextState.questionIndex, bubble))
         
         row.appendChild(bubble)
         messagesEl.appendChild(row)
 
         openTextState.floatingConfirmRow?.remove()
         const newRow = createFloatingConfirmRow(openTextState.questionIndex)
         messagesEl.appendChild(newRow)
         openTextState.floatingConfirmRow = newRow
         scrollToBottom()
 
         const bundled = openTextState.messages.join('\n\n')
         components[openTextState.questionIndex].setAnswer(bundled)
         answeredState[openTextState.questionIndex] = true
         const questionId = questions[openTextState.questionIndex].id!
         openTextDraftsByQuestionId.set(questionId, [...openTextState.messages])
         updateProgress()
         persistProgress()
     }

    async function confirmOpenText(index: number): Promise<void> {
        if (!openTextState || openTextState.questionIndex !== index) return

        // Handle edit mode — save pending edits to bubble before confirming
        if (editingBubble) {
            let editedText: string

            if (editingBubble.contentEditable === 'true') {
                editedText = editingBubble.textContent?.trim() || ''
                editingBubble.contentEditable = 'false'
            } else {
                editedText = chatInput.value.trim()
            }

            editingBubble.classList.remove('chat-bubble--editing')

            if (editedText) {
                editingBubble.textContent = editedText
            }

            components[index].setAnswer(editedText || null)
            answeredState[index] = editedText.length > 0
            const questionId = questions[index].id!
            if (editedText) {
                openTextDraftsByQuestionId.set(questionId, [editedText])
            } else {
                openTextDraftsByQuestionId.delete(questionId)
            }
            
            openTextState.floatingConfirmRow?.classList.add('chat-confirm-row--confirmed')
            updateProgress()
            hideInlineConfirm()
            editingBubble = null
            openTextState = null
            deactivateInput()
            persistProgress(index + 1)
            
            await wait(350)
            if (index < questions.length - 1) {
                await revealQuestion(index + 1)
            } else {
                await showSubmitSection()
            }
            return
        }

        // Original behavior for multi-message open text
        const inputText = chatInput.value.trim()
        if (inputText) {
            sendOpenTextMessage()
        }

        if (openTextState.messages.length === 0 && questions[index].required) {
            await appendAiBubble(t.pleaseFill, { animated: false })
            return
        }

        // If no messages but question is not required, show an empty bubble so user can click to edit
        // Place empty bubble BEFORE the confirm row (above the checkmark)
        let emptyBubble: HTMLElement | null = null
        if (openTextState.messages.length === 0 && !questions[index].required) {
            const row = document.createElement('div')
            row.className = 'chat-row chat-row--user'
            const bubble = document.createElement('div')
            bubble.className = 'chat-bubble chat-bubble--user chat-bubble--editable'
            bubble.textContent = '\u200B' // Zero-width space to maintain bubble height
            bubble.addEventListener('click', createEditHandler(index, bubble))
            row.appendChild(bubble)
            emptyBubble = row
        }

        const bundled = openTextState.messages.join('\n\n')
        components[index].setAnswer(bundled || null)
        openTextDraftsByQuestionId.delete(questions[index].id!)

        // Insert empty bubble before the confirm row, not after
        if (emptyBubble && openTextState.floatingConfirmRow) {
            openTextState.floatingConfirmRow.before(emptyBubble)
        } else if (emptyBubble) {
            messagesEl.appendChild(emptyBubble)
        }

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
         const questionId = questions[questionIndex].id!
         const restoredMessages = openTextDraftsByQuestionId.get(questionId) ?? []
         openTextState = { questionIndex, messages: [...restoredMessages], floatingConfirmRow: null }
 
         if (restoredMessages.length > 0) {
             // Append restored bubbles with click handlers to allow re-editing
             restoredMessages.forEach((message) => {
                 const row = document.createElement('div')
                 row.className = 'chat-row chat-row--user'
                 const bubble = document.createElement('div')
                 bubble.className = 'chat-bubble chat-bubble--user chat-bubble--editable'
                 bubble.textContent = message
                 
                 // Add click handler for inline editing
                 bubble.addEventListener('click', createEditHandler(questionIndex, bubble))
                 
                 row.appendChild(bubble)
                 messagesEl.appendChild(row)
             })
             
             const restoredRow = createFloatingConfirmRow(questionIndex)
             messagesEl.appendChild(restoredRow)
             openTextState.floatingConfirmRow = restoredRow
             components[questionIndex].setAnswer(restoredMessages.join('\n\n'))
             answeredState[questionIndex] = true
             updateProgress()
             scrollToBottom()
         } else {
             // Show floating confirm row even when empty, for non-required questions
             const emptyRow = createFloatingConfirmRow(questionIndex)
             messagesEl.appendChild(emptyRow)
             openTextState.floatingConfirmRow = emptyRow
             scrollToBottom()
         }
 
         chatInput.disabled = false
         chatInput.placeholder = questions[questionIndex].hint?.trim() || t.typeHere
         chatInput.value = ''
         chatInput.style.height = 'auto'
         sendBtn.disabled = false
         brainstormBtn.hidden = false
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
        brainstormBtn.hidden = false
        hideInlineConfirm()
        activeSendHandler = handler
        updateSendIcon()
        setTimeout(() => chatInput.focus(), 50)
    }

    function deactivateInput(placeholder = ''): void {
        stopChatRecording()
        activeSendHandler = null
        chatInput.disabled = true
        chatInput.value = ''
        chatInput.style.height = 'auto'
        chatInput.placeholder = placeholder || t.selectAbove
        sendBtn.disabled = true
        brainstormBtn.hidden = true
        updateSendIcon()
    }

    function startChatRecording(): void {
        if (isChatRecording) return
        isChatRecording = true
        sendBtn.classList.add('recording')
        const stt = getSTTManager()
        const language = getSpeechLanguage()
        stt.setupCallbacks({
            onStateChange: (state) => {
                if (state === 'idle' || state === 'error') {
                    isChatRecording = false
                    sendBtn.classList.remove('recording')
                    updateSendIcon()
                }
            },
        })
        stt.start(chatInput, language, () => { updateSendIcon() }).catch(() => {
            isChatRecording = false
            sendBtn.classList.remove('recording')
        })
    }

    function stopChatRecording(): void {
        if (!isChatRecording) return
        isChatRecording = false
        sendBtn.classList.remove('recording')
        getSTTManager().stop()
    }

    chatInput.addEventListener('input', () => {
        updateSendIcon()
        chatInput.style.height = 'auto'
        chatInput.style.height = `${Math.min(chatInput.scrollHeight, 120)}px`
    })

    sendBtn.addEventListener('click', () => {
        if (isChatRecording) {
            stopChatRecording()
            return
        }
        if (chatInput.value.trim().length > 0) {
            void activeSendHandler?.()
            return
        }
        startChatRecording()
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
        if (!q) return

        await appendAiBubble(q.text, {
            bubbleClass: 'chat-bubble--question-title',
            questionNum: index + 1,
            required: q.required,
        })

        if (q.hint?.trim()) {
            await wait(150)
            await appendAiBubble(q.hint.trim())
        }

        if (q.type === QuestionType.Open) {
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

            // Validate required questions before submitting
            for (let i = 0; i < questions.length; i++) {
                if (questions[i].required && !hasAnswer(components[i].getAnswer())) {
                    await appendAiBubble(t.pleaseFill, { animated: false })
                    scrollToBottom()
                    return
                }
            }

            btn.disabled = true
            btn.textContent = t.submitting

            const answers = mapAnswersToResponse(questions, components)

            try {
                await submitAnswers(params.organizationSlug, params.projectSlug, {
                    projectId: params.projectSlug,
                    answers,
                })
                 localStorage.setItem(completedKey, 'true')
                 clearSurveyProgress(projectSlugKey)
  
                 // Lock all survey answers to prevent further editing
                 for (let i = 0; i <= confirmedUpToIndex && i < components.length; i++) {
                     components[i].lock()
                 }
                 deactivateInput()
               
                 submitRow.remove()
 
                 const successRow = document.createElement('div')
                 successRow.className = 'chat-submit-success'
                 successRow.innerHTML = `
                     <div class="chat-submit-success-icon">${CHECKMARK_SVG}</div>
                     <p class="chat-submit-success-title">${esc(t.submittedTitle)}</p>
                     <p class="chat-submit-success-sub">${esc(t.submittedSub)}</p>`
                 messagesEl.appendChild(successRow)
                 scrollToBottom()
 
                 // Save survey history after success message is added and submit button removed
                 lockSurveyHistory()
 
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
        // Save survey history HTML before locking
        const surveyHistoryKey = `survey-history-${projectSlugKey}`
        const historyHTML = messagesEl.innerHTML
        localStorage.setItem(surveyHistoryKey, historyHTML)

        // Save answers snapshot for language-independent history reconstruction
        const snapshot: Record<number, QuestionAnswer> = {}
        for (let i = 0; i < questions.length; i++) {
            snapshot[questions[i].id!] = components[i].getAnswer()
        }
        localStorage.setItem(`survey-answers-snapshot-${projectSlugKey}`, JSON.stringify(snapshot))

        Array.from(messagesEl.children).forEach((el) => {
            if (!(el instanceof HTMLElement)) return
            el.classList.add('chat-survey-history-locked')
            // Keep history scrollable while preventing edits on old answer controls.
            el.querySelectorAll<HTMLElement>('button, input, textarea, select, [role="button"], [contenteditable="true"]').forEach((control) => {
                control.setAttribute('tabindex', '-1')
                control.setAttribute('aria-disabled', 'true')
                if (control instanceof HTMLButtonElement || control instanceof HTMLInputElement || control instanceof HTMLTextAreaElement || control instanceof HTMLSelectElement) {
                    control.disabled = true
                }
            })

            // Explicitly force contenteditable to false on bubbles
            el.querySelectorAll<HTMLElement>('[contenteditable="true"]').forEach((bubble) => {
                bubble.contentEditable = 'false'
                bubble.removeAttribute('contenteditable')
            })

            // Remove click handlers from editable bubbles to prevent editing
            el.querySelectorAll<HTMLElement>('.chat-bubble--editable').forEach((bubble) => {
                // Clone and replace to remove all event listeners
                const newBubble = bubble.cloneNode(true) as HTMLElement
                bubble.parentNode?.replaceChild(newBubble, bubble)
            })
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

    // ===== Restore survey history =====
    function restoreSurveyHistory(): boolean {
        const surveyHistoryKey = `survey-history-${projectSlugKey}`
        const savedHistory = localStorage.getItem(surveyHistoryKey)
        if (savedHistory) {
            messagesEl.innerHTML = savedHistory
            // Re-apply the locked class to all children
            Array.from(messagesEl.children).forEach((el) => {
                if (!(el instanceof HTMLElement)) return
                el.classList.add('chat-survey-history-locked')
            })
            return true
        }
        return false
    }

    // ===== Rebuild survey history from answers snapshot =====
    function rebuildHistoryFromSnapshot(): void {
        const snapshotKey = `survey-answers-snapshot-${projectSlugKey}`
        const raw = localStorage.getItem(snapshotKey)
        if (!raw) {
            messagesEl.innerHTML = ''
            return
        }

        let snapshot: Record<number, QuestionAnswer>
        try {
            snapshot = JSON.parse(raw) as Record<number, QuestionAnswer>
        } catch {
            messagesEl.innerHTML = ''
            return
        }

        messagesEl.innerHTML = ''

        // Project title
        const titleRow = document.createElement('div')
        titleRow.className = 'chat-row chat-row--ai'
        const titleAvatar = document.createElement('div')
        titleAvatar.className = 'chat-avatar'
        titleAvatar.innerHTML = avatarHTML(workspaceBadge, inIdeasPhase, workspaceLogo)
        const titleBubbleGroup = document.createElement('div')
        titleBubbleGroup.className = 'chat-bubble-group'
        const titleBubble = document.createElement('div')
        titleBubble.className = 'chat-bubble chat-bubble--ai chat-bubble--project-title'
        titleBubble.textContent = project.title
        titleBubbleGroup.appendChild(titleBubble)
        titleRow.appendChild(titleAvatar)
        titleRow.appendChild(titleBubbleGroup)
        messagesEl.appendChild(titleRow)

        // Project description
        if (project.description) {
            const descRow = document.createElement('div')
            descRow.className = 'chat-row chat-row--ai'
            const descAvatar = document.createElement('div')
            descAvatar.className = 'chat-avatar'
            descAvatar.innerHTML = avatarHTML(workspaceBadge, inIdeasPhase, workspaceLogo)
            const descBubbleGroup = document.createElement('div')
            descBubbleGroup.className = 'chat-bubble-group'
            const descBubble = document.createElement('div')
            descBubble.className = 'chat-bubble chat-bubble--ai'
            descBubble.textContent = project.description
            descBubbleGroup.appendChild(descBubble)
            descRow.appendChild(descAvatar)
            descRow.appendChild(descBubbleGroup)
            messagesEl.appendChild(descRow)
        }

        // Questions
        for (let i = 0; i < questions.length; i++) {
            const q = questions[i]
            const answer = snapshot[q.id!]
            if (answer === undefined || answer === null) continue

            // AI bubble: question text
            const qRow = document.createElement('div')
            qRow.className = 'chat-row chat-row--ai'
            const qAvatar = document.createElement('div')
            qAvatar.className = 'chat-avatar'
            qAvatar.innerHTML = avatarHTML(workspaceBadge, inIdeasPhase, workspaceLogo)
            const qBubbleGroup = document.createElement('div')
            qBubbleGroup.className = 'chat-bubble-group'
            const qBubble = document.createElement('div')
            qBubble.className = 'chat-bubble chat-bubble--ai chat-bubble--question-title'
            const numEl = document.createElement('span')
            numEl.className = 'chat-question-num'
            numEl.textContent = `${i + 1}.`
            qBubble.appendChild(numEl)
            qBubble.appendChild(document.createTextNode(' ' + q.text))

            const speakerBtn = document.createElement('button')
            speakerBtn.className = 'chat-speaker-btn'
            speakerBtn.type = 'button'
            speakerBtn.setAttribute('aria-label', t.readAloud)
            speakerBtn.innerHTML = SPEAKER_SVG

            let bubbleOrWrapper: HTMLElement = qBubble
            if (q.required) {
                const wrapper = document.createElement('div')
                wrapper.className = 'chat-bubble-wrapper'
                wrapper.appendChild(qBubble)
                const badge = document.createElement('span')
                badge.className = 'chat-required-float'
                badge.textContent = t.requiredLabel
                wrapper.appendChild(badge)
                bubbleOrWrapper = wrapper
            }

            qBubbleGroup.appendChild(bubbleOrWrapper)
            qBubbleGroup.appendChild(speakerBtn)
            qRow.appendChild(qAvatar)
            qRow.appendChild(qBubbleGroup)
            messagesEl.appendChild(qRow)

            // User answer
            if (q.type === QuestionType.Open) {
                const text = (answer ?? '') as string
                const userRow = document.createElement('div')
                userRow.className = 'chat-row chat-row--user'
                const userBubble = document.createElement('div')
                userBubble.className = 'chat-bubble chat-bubble--user'
                userBubble.textContent = text || '\u200B'
                userRow.appendChild(userBubble)
                messagesEl.appendChild(userRow)
            } else {
                const userRow = document.createElement('div')
                userRow.className = 'chat-row chat-row--user'
                const userBubble = document.createElement('div')
                userBubble.className = 'chat-bubble chat-bubble--user'
                userBubble.textContent = formatAnswerForDisplay(q, answer)
                userRow.appendChild(userBubble)
                messagesEl.appendChild(userRow)
            }

            // Confirm checkmark row
            const confirmRow = document.createElement('div')
            confirmRow.className = 'chat-confirm-row chat-confirm-row--confirmed'
            confirmRow.setAttribute('data-confirm-for', String(i))
            confirmRow.innerHTML = `
                <div class="chat-confirm-line"></div>
                <div class="chat-confirm-btn" aria-hidden="true">${CHECKMARK_SVG}</div>
                <div class="chat-confirm-line"></div>`
            messagesEl.appendChild(confirmRow)
        }

        // Success message
        const successRow = document.createElement('div')
        successRow.className = 'chat-submit-success'
        successRow.innerHTML = `
            <div class="chat-submit-success-icon">${CHECKMARK_SVG}</div>
            <p class="chat-submit-success-title">${esc(t.submittedTitle)}</p>
            <p class="chat-submit-success-sub">${esc(t.submittedSub)}</p>`
        messagesEl.appendChild(successRow)
    }

    // ===== Ideation phase =====
    async function enterIdeasPhase(): Promise<void> {
        inIdeasPhase = true

        // Restore survey history: try saved HTML first, fallback to rebuilding from answers snapshot
        const hasRestoredHTML = restoreSurveyHistory()
        if (!hasRestoredHTML) {
            rebuildHistoryFromSnapshot()
        }
        lockSurveyHistory()

        // Keep organization branding anchored at the top of the full page in ideation mode.
        const topbar = scrollAreaEl.querySelector<HTMLElement>('.survey-topbar')
        if (topbar && topbar.parentElement !== chatShell) {
            chatShell.insertBefore(topbar, chatShell.firstChild)
        }

        const initResult: IdeasInitResult = await initIdeasContext(params.organizationSlug, params.projectSlug, project)
        const { allIdeas, topics, youthToken, firstTopic } = initResult
        topicsForBrainstorm = topics

        if (!firstTopic) {
            await appendAiBubble(t.surveyCompleted)
            return
        }

        const surveyHeaderEl = container.querySelector<HTMLElement>('#chat-survey-header')!
        surveyHeaderEl.hidden = true

        let activeView: ActiveView = { type: 'topic', topicId: firstTopic.id }
        activeViewForBrainstorm = activeView
        let discoveryMode: DiscoveryMode = DiscoveryMode.All
        let selectedSemanticCategory: string | null = null
        let showPostPreviewPair = false
        let latestSubmittedIdea: Idea | null = null
        let discoveryRequestToken = 0
        let discoveryBadgeByIdeaId: ReadonlyMap<number, DiscoveryBadgeType> = new Map()
        let visibleIdeasCache: Idea[] = []
        let extraLoadsUsed = 0
        let isLoadingMoreIdeas = false
        let autoLoadArmed = true
        let lastScrollTop = 0
        let suppressListScrollSyncUntil = 0

        function suppressListScrollSync(durationMs: number): void {
            suppressListScrollSyncUntil = performance.now() + durationMs
        }

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
                        <span class="ideas-compose-topic-kicker">${esc(t.chooseTopic)}</span>
                        <span id="ideas-topic-trigger-value" class="ideas-compose-topic-value"></span>
                        <span class="ideas-compose-topic-chevron" aria-hidden="true">▾</span>
                    </span>
                </button>
                <button id="ideas-topic-trigger-floating" class="ideas-compose-topic-button ideas-compose-topic-button--floating" aria-haspopup="dialog" aria-expanded="false" aria-controls="topic-modal" aria-label="Switch topic" hidden>
                    <span class="ideas-compose-topic-text">
                        <span class="ideas-compose-topic-kicker">${esc(t.chooseTopic)}</span>
                        <span id="ideas-topic-trigger-floating-value" class="ideas-compose-topic-value"></span>
                        <span class="ideas-compose-topic-chevron" aria-hidden="true">▾</span>
                    </span>
                </button>
            </div>
            <section class="ideas-community min-h-0 flex flex-col overflow-hidden overscroll-contain relative" aria-label="Ideas list">
                <div id="ideas-discovery" class="ideas-discovery" hidden>
                    <button id="ideas-discovery-trigger" class="ideas-discovery-trigger" type="button" aria-haspopup="menu" aria-expanded="false">
                        <span id="ideas-discovery-label">${esc(t.exploreIdeas)}</span>
                        <span class="ideas-discovery-chevron" aria-hidden="true">▾</span>
                    </button>
                    <div id="ideas-discovery-menu" class="ideas-discovery-menu" role="menu" hidden></div>
                </div>
                <div class="ideas-list flex-1 min-h-0 overflow-y-auto py-[var(--spacing-sm)] px-[var(--spacing-md)] flex flex-col gap-[var(--spacing-xs)] overscroll-contain snap-none" id="chat-ideas-list" aria-live="polite"></div>
            </section>`

        chatShell.classList.add('chat-shell--ideas')
        chatShell.insertBefore(ideasArea, scrollAreaEl)

        const ideasListEl = container.querySelector<HTMLDivElement>('#chat-ideas-list')!

        // Create load more button dynamically (will be appended to list)
        const loadMoreBtn = document.createElement('button')
        loadMoreBtn.id = 'chat-ideas-load-more'
        loadMoreBtn.className = 'ideas-load-more'
        loadMoreBtn.type = 'button'
        loadMoreBtn.hidden = true
        loadMoreBtn.innerHTML = `
            <span class="ideas-load-more-icon" aria-hidden="true">
                <svg class="ideas-load-more-ring" viewBox="0 0 36 36" xmlns="http://www.w3.org/2000/svg" aria-hidden="true">
                    <circle class="ideas-load-more-ring-track" cx="18" cy="18" r="14"/>
                    <circle class="ideas-load-more-ring-fill" cx="18" cy="18" r="14"/>
                </svg>
                <span class="ideas-load-more-arrow">↓</span>
            </span>
            <span class="ideas-load-more-text">${esc(t.loadMoreIdeas)}</span>
        `
        const loadMoreText = loadMoreBtn.querySelector<HTMLSpanElement>('.ideas-load-more-text')!
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

        const chatNudgeFlow = createChatIdeaNudgeFlow({
            workspaceSlug: params.organizationSlug,
            projectSlug: params.projectSlug,
            getActiveView: () => activeView,
            getContext: (view) => {
                if (view.type !== 'topic') return null
                const topic = topics.find((item) => item.id === view.topicId)
                if (!topic) return null
                return {
                    projectTitle: project.title,
                    projectDescription: project.description,
                    topicTitle: topic.title,
                    topicPrompt: topic.prompt,
                }
            },
            appendAssistantBubble: (text) => appendAiBubble(text),
            appendUserBubble: (text) => appendUserBubble(text),
            setInputDisabled: (disabled) => {
                chatInput.disabled = disabled
                sendBtn.disabled = disabled
                updateSendIcon()
            },
            setInputPlaceholder: (text) => { chatInput.placeholder = text },
            clearInput: () => {
                chatInput.value = ''
                chatInput.dispatchEvent(new Event('input', { bubbles: true }))
            },
            focusInput: () => chatInput.focus(),
            showTyping,
            hideTyping,
        })

        let listController: ReturnType<typeof createIdeasListController> | null = null

        const ideaPanel = createIdeaPanelController({
            root: container,
            reviewBeforePost: (input) => safetyDialog.reviewBeforePost(input),
            reviewWithSuggestion,
            getNudgingContext: (view: ActiveView) => {
                if (view.type !== 'topic') return null
                const topic = topics.find((item) => item.id === view.topicId)
                if (!topic) return null
                return {
                    projectTitle: project.title,
                    projectDescription: project.description,
                    topicTitle: topic.title,
                    topicPrompt: topic.prompt,
                }
            },
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
            onSelect: async (nextView) => {
                chatNudgeFlow.cancelIfTopicChanged()
                activeView = nextView
                activeViewForBrainstorm = nextView
                if (nextView.type === 'topic') {
                    discoveryMode = DiscoveryMode.All
                    selectedSemanticCategory = null
                    showPostPreviewPair = false
                }
                resetPaging()
                closeDiscoveryMenu()
                void renderIdeasList()
                topicModal.renderTopics(activeView)
                updateTopicLabels()
                if (activeView.type === 'topic') {
                    const view = activeView as { type: 'topic'; topicId: number }
                    const topic = topics.find((item) => item.id === view.topicId)
                    if (topic) {
                        const topicPrompt = topic.prompt?.trim()
                        const switchMsg = t.switchedTopic.replace('{topicTitle}', topic.title)
                        await appendAiBubble(switchMsg, { bubbleClass: 'chat-bubble--topic-switch' })
                        await appendAiBubble(topicPrompt || t.thoughtsOnTopic.replace('{topicTitle}', topic.title), { prefix: t.topicQuestionLabel })
                        activateIdeaInput(t.shareIdea, handleIdeaSubmit)
                    }
                } else {
                    deactivateInput(t.selectTopicToShare)
                }
            },
        })

        // Wire up topic trigger → toggle topic modal
        topicTrigger.addEventListener('click', (e) => {
            e.stopPropagation()
            if (topicModal.isOpen()) {
                topicModal.close()
            } else {
                topicModal.open(topicTrigger)
            }
        })

        // Wire up discovery trigger → toggle discovery menu
        discoveryTrigger.addEventListener('click', (e) => {
            e.stopPropagation()
            const opening = discoveryMenu.hidden
            if (opening) {
                updateDiscoveryUi()
            }
            discoveryMenu.hidden = !opening
            discoveryTrigger.setAttribute('aria-expanded', String(opening))
        })

        // Wire up discovery menu option clicks
        discoveryMenu.addEventListener('click', (e) => {
            const opt = (e.target as HTMLElement).closest<HTMLElement>('[data-discovery-mode], [data-semantic-category]')
            if (!opt) return

            const mode = opt.dataset.discoveryMode
            const category = opt.dataset.semanticCategory

            if (mode) {
                if (mode === 'all') discoveryMode = DiscoveryMode.All
                else if (mode === 'similar') discoveryMode = DiscoveryMode.Similar
                else if (mode === 'different') discoveryMode = DiscoveryMode.Different
                selectedSemanticCategory = null
                showPostPreviewPair = false
            } else if (category) {
                selectedSemanticCategory = category
                showPostPreviewPair = false
            }

            discoveryMenu.hidden = true
            discoveryTrigger.setAttribute('aria-expanded', 'false')
            updateDiscoveryUi()
            void renderIdeasList()
        })

        function hasOwnIdeaInTopic(topicId: number): boolean {
            return allIdeas.some((idea) => idea.authorType === IdeaAuthorType.Self && idea.topicId === topicId)
        }

        function resetPaging(): void {
            extraLoadsUsed = 0
            isLoadingMoreIdeas = false
            autoLoadArmed = true
        }

        function getMaxExtraLoads(): number {
            if (activeView.type !== 'topic') return 3
            const topicId = (activeView as { type: 'topic'; topicId: number }).topicId
            const topic = topics.find((item) => item.id === topicId)
            return topic?.maxBroadSelectionLoads ?? 3
        }

        function getVisibleLimit(): number {
            return IDEAS_BATCH_SIZE * (1 + extraLoadsUsed)
        }

        function hasMoreIdeasToLoad(): boolean {
            return visibleIdeasCache.length > getVisibleLimit() && extraLoadsUsed < getMaxExtraLoads()
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
            ideasListEl.classList.toggle('ideas-list--has-more', hasMoreIdeas)

            // Force SVG animation restart each time loading begins
            if (isLoadingMoreIdeas && !wasLoading) {
                const ringFill = loadMoreBtn.querySelector<SVGCircleElement>('.ideas-load-more-ring-fill')
                if (ringFill) {
                    ringFill.style.animation = 'none'
                    void ringFill.getBoundingClientRect()
                    ringFill.style.animation = ''
                }
            }

            // Append button to list so it scrolls with content (like vertical scroll mode)
            if (loadMoreBtn.parentElement !== ideasListEl) {
                ideasListEl.appendChild(loadMoreBtn)
            }
        }

        async function loadMoreIdeas(): Promise<void> {
            if (isLoadingMoreIdeas || !hasMoreIdeasToLoad()) return

            isLoadingMoreIdeas = true
            updateLoadMoreButton()
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
                await renderIdeasList()
                listController?.setActive(firstNewIndex, true)
            } finally {
                isLoadingMoreIdeas = false
                updateLoadMoreButton()
            }
        }

        function getTopicSemanticCategories(topicId: number): string[] {
            return getTopicSemanticCats(allIdeas, topicId)
        }

        function renderDiscoveryMenuOptions(): void {
            if (activeView.type !== 'topic') {
                discoveryMenu.innerHTML = ''
                return
            }

            const categories = getTopicSemanticCategories(activeView.topicId)
            const semanticButtons = categories
                .map((category) => `<button class="ideas-discovery-option" data-semantic-category="${category.replace(/"/g, '&quot;')}" role="menuitem" type="button">${category}</button>`)
                .join('')
            const categoriesSection = categories.length > 0
                ? `<hr class="ideas-discovery-separator" role="separator"><p class="ideas-discovery-section-label">${esc(t.ideaCategories)}</p>${semanticButtons}`
                : ''

            if (!hasOwnIdeaInTopic(activeView.topicId)) {
                discoveryMenu.innerHTML = `<button class="ideas-discovery-option" data-discovery-mode="all" role="menuitem" type="button">${esc(t.broadSelection)}</button>${categoriesSection}`
                return
            }

            discoveryMenu.innerHTML = `
                <button class="ideas-discovery-option" data-discovery-mode="similar" role="menuitem" type="button">${esc(t.similarIdeas)}</button>
                <button class="ideas-discovery-option" data-discovery-mode="different" role="menuitem" type="button">${esc(t.differingIdeas)}</button>
                <button class="ideas-discovery-option" data-discovery-mode="all" role="menuitem" type="button">${esc(t.allIdeas)}</button>
                ${categoriesSection}`
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
            const view = activeView as { type: 'topic'; topicId: number }
            const ownIdeaExists = hasOwnIdeaInTopic(view.topicId)
            const labelMap: Record<DiscoveryMode, string> = { all: t.allIdeas, similar: t.similarIdeas, different: t.differingIdeas, random: '' }
            discoveryLabel.textContent = ownIdeaExists ? labelMap[discoveryMode] : (selectedSemanticCategory ?? t.broadSelection)

            discoveryMenu.querySelectorAll<HTMLButtonElement>('.ideas-discovery-option').forEach((option) => {
                const mode = option.dataset.discoveryMode
                const semanticCategory = option.dataset.semanticCategory
                const isOwnMode = ownIdeaExists && mode === discoveryMode
                const isBroadSelection = !ownIdeaExists && !selectedSemanticCategory && mode === DiscoveryMode.All
                const isSemanticSelection = !ownIdeaExists && !!selectedSemanticCategory && semanticCategory === selectedSemanticCategory
                option.classList.toggle('selected', isOwnMode || isBroadSelection || isSemanticSelection)
            })
        }

        async function getVisibleIdeasForCurrentMode(): Promise<DiscoveryFeed> {
            if (activeView.type === 'my-ideas') {
                return createDiscoveryFeed(allIdeas.filter((idea) => idea.authorType === IdeaAuthorType.Self), new Map())
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

        function getActiveIdeasLabel(view: ActiveView): string {
            if (view.type === 'my-ideas') return t.myIdeas
            const topic = topics.find((item) => item.id === view.topicId)
            return topic ? topic.title : t.selectTopic
        }

        function updateTopicLabels(): void {
            const label = getActiveIdeasLabel(activeView)
            topicTriggerValue.textContent = label
            topicFloatingTriggerValue.textContent = label
            // Floating switch-topic button is for vertical ideas page UX only.
            topicFloatingTrigger.hidden = true

            const inputWrap = chatShell.querySelector<HTMLElement>('.chat-input-wrap')
            if (!inputWrap) return

            const isMyIdeasView = activeView.type === 'my-ideas'
            inputWrap.classList.toggle('chat-input-wrap--muted', isMyIdeasView)
            if (isMyIdeasView) {
                deactivateInput(t.selectTopicToShare)
            }
        }

        async function renderIdeasList(): Promise<void> {
            const renderToken = ++discoveryRequestToken
            updateTopicLabels()
            updateDiscoveryUi()

            const discoveryFeed = await getVisibleIdeasForCurrentMode()
            if (renderToken !== discoveryRequestToken) return

            visibleIdeasCache = discoveryFeed.ideas
            discoveryBadgeByIdeaId = discoveryFeed.badgesByIdeaId
            const pagedIdeas = visibleIdeasCache.slice(0, getVisibleLimit())

            if (listController) {
                listController.cleanup()
            }

            if (pagedIdeas.length === 0) {
                ideasListEl.classList.remove('ideas-list--preview')
                ideasListEl.innerHTML = `<p class="ideas-empty-state">${esc(t.noIdeas)}</p>`
                updateLoadMoreButton()
                listController = null
                return
            }

            ideasListEl.classList.toggle('ideas-list--preview', showPostPreviewPair)

            listController = createIdeasListController({
                list: ideasListEl,
                ideas: pagedIdeas,
                activeView,
                topics,
                flaggedIdeaIds,
                discoveryBadgeByIdeaId,
                onDiscoveryBadgeClick: (badge: DiscoveryBadgeType) => {
                    discoveryMode = badge === DiscoveryBadgeType.Similar ? DiscoveryMode.Similar : DiscoveryMode.Different
                    showPostPreviewPair = false
                    resetPaging()
                    closeDiscoveryMenu()
                    void renderIdeasList()
                },
            })
            listController.setActive(0, false)
            listController.startRotation()
            topicModal.renderTopics(activeView)
            updateLoadMoreButton()
        }

        const submitHandler = createIdeasSubmitHandler({
            organizationSlug: params.organizationSlug,
            projectSlug: params.projectSlug,
            projectId: project.id,
            reviewBeforePost: (input) => safetyDialog.reviewBeforePost(input),
            reviewWithSuggestion,
            getNudgingContext: (view) => {
                if (view.type !== 'topic') return null
                const topic = topics.find((item) => item.id === view.topicId)
                if (!topic) return null
                return {
                    projectTitle: project.title,
                    projectDescription: project.description,
                    topicTitle: topic.title,
                    topicPrompt: topic.prompt,
                }
            },
            runNudgingFlow: async (input: string, activeView: ActiveView, _context: IdeaNudgingContext) => {
                return chatNudgeFlow.start(input, activeView)
            },
            onIdeaSubmitted: (idea: Idea) => {
                allIdeas.unshift(idea)
                latestSubmittedIdea = idea
                showPostPreviewPair = false
                resetPaging()
                closeDiscoveryMenu()
                void renderIdeasList()

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

         handleIdeaSubmit = async (): Promise<void> => {
             const text = chatInput.value.trim()
             if (!text) return
             if (activeView.type !== 'topic') return

             if (chatNudgeFlow.isActive()) {
                 chatNudgeFlow.submitAnswer(text)
                 chatInput.value = ''
                 chatInput.dispatchEvent(new Event('input', { bubbles: true }))
                 return
             }

             chatInput.disabled = true
             const userBubble = document.createElement('div')
             userBubble.className = 'chat-row chat-row--user'
             userBubble.innerHTML = `<div class="chat-bubble chat-bubble--user">${esc(text)}</div>`
             messagesEl.appendChild(userBubble)
             scrollToBottom()

             chatInput.value = ''
             chatInput.dispatchEvent(new Event('input', { bubbles: true }))

              try {
                  await submitHandler.submit(text, activeView)
                  await appendAiBubble(t.thanksForIdea)
                  await wait(400)
                  // Tell user they can post another or switch topic
                  await appendAiBubble(t.postIdeaNext, { bubbleClass: 'chat-bubble--topic-switch' })
                  if (activeView.type === 'topic') {
                      const currentTopic = topics.find((item) => item.id === (activeView as { type: 'topic'; topicId: number }).topicId)
                      if (currentTopic) {
                          const prompt = currentTopic.prompt?.trim()
                          await appendAiBubble(prompt || t.thoughtsOnTopic.replace('{topicTitle}', currentTopic.title), { prefix: t.topicQuestionLabel })
                      }
                  }
              } catch {
                  await appendAiBubble(t.somethingWrong)
              } finally {
                  chatInput.disabled = false
                  chatInput.placeholder = t.shareAnother
                  chatInput.focus()
              }
         }

        ideasListEl.addEventListener('scroll', () => {
            const isSuppressed = performance.now() < suppressListScrollSyncUntil
            listController?.updateFromScroll()

            const currentScrollTop = ideasListEl.scrollTop
            const isScrollingUp = currentScrollTop < lastScrollTop
            lastScrollTop = currentScrollTop

            if (isScrollingUp && isLoadingMoreIdeas) {
                isLoadingMoreIdeas = false
                updateLoadMoreButton()
                if (!isSuppressed) return
            }

            if (isSuppressed) return

            const distanceFromBottom = ideasListEl.scrollHeight - ideasListEl.clientHeight - ideasListEl.scrollTop
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

        loadMoreBtn.addEventListener('click', () => {
            void loadMoreIdeas()
        })

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
        const topicPrompt = firstTopic.prompt?.trim()
        await appendAiBubble(topicPrompt || t.thoughtsOnTopic.replace('{topicTitle}', firstTopic.title), { prefix: t.topicQuestionLabel })
        activateIdeaInput(t.shareIdea, handleIdeaSubmit)
    }

    window.addEventListener('app:before-navigate', () => {
        stopChatRecording()
        bubbleSpeakerControllers.forEach(c => c.stop())
        brainstormController.destroy()
        cleanupDesktopLayout()
    }, { once: true })

    // ===== Start conversation =====
    if (startInIdeasMode) {
        await enterIdeasPhase()
        return
    }

    const savedProgress = loadSurveyProgress(projectSlugKey, questions)

    // Check if there's meaningful progress to resume
    const hasAnsweredQuestions = savedProgress && savedProgress.answersByQuestionId && savedProgress.answersByQuestionId.size > 0
    const shouldResume = savedProgress && (savedProgress.currentQuestionIndex > 0 || hasAnsweredQuestions) && questions.length > 0

    if (shouldResume) {
        const answeredQuestionCount = Array.from(savedProgress!.answersByQuestionId.values()).filter((answer) => hasAnswer(answer)).length
        const resumeAt = Math.min(Math.max(savedProgress!.currentQuestionIndex, answeredQuestionCount), questions.length)
        confirmedUpToIndex = resumeAt
        const savedOpenTextDrafts = savedProgress!.openTextDraftsByQuestionId ?? new Map<number, string[]>()
        savedOpenTextDrafts.forEach((messages, questionId) => {
            openTextDraftsByQuestionId.set(questionId, [...messages])
        })

        // Restore answers to components
        for (let i = 0; i < questions.length; i++) {
            const saved = savedProgress!.answersByQuestionId.get(questions[i].id!)
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
                 required: q.required,
             })

             if (q.type === QuestionType.Open) {
                 // For open text questions, show as clickable editable bubble(s)
                 const questionId = questions[i].id!
                 const drafts = openTextDraftsByQuestionId.get(questionId)
                 const answer = components[i].getAnswer()

                 if (drafts && drafts.length > 0) {
                     drafts.forEach((message) => {
                         const row = document.createElement('div')
                         row.className = 'chat-row chat-row--user'
                         const bubble = document.createElement('div')
                         bubble.className = 'chat-bubble chat-bubble--user chat-bubble--editable'
                         bubble.textContent = message
                         bubble.addEventListener('click', createEditHandler(i, bubble))
                         row.appendChild(bubble)
                         messagesEl.appendChild(row)
                     })
                 } else {
                     const displayText = formatAnswerForDisplay(q, answer)
                     const row = document.createElement('div')
                     row.className = 'chat-row chat-row--user'
                     const bubble = document.createElement('div')
                     bubble.className = 'chat-bubble chat-bubble--user chat-bubble--editable'
                     bubble.textContent = displayText || '\u200B'
                     bubble.addEventListener('click', createEditHandler(i, bubble))
                     row.appendChild(bubble)
                     messagesEl.appendChild(row)
                 }

                 // Show confirmed checkmark row for open text
                 const confirmRow = document.createElement('div')
                 confirmRow.className = 'chat-confirm-row chat-confirm-row--confirmed'
                 confirmRow.setAttribute('data-confirm-for', String(i))
                 confirmRow.innerHTML = `
                 <div class="chat-confirm-line"></div>
                 <div class="chat-confirm-btn" aria-hidden="true">${CHECKMARK_SVG}</div>
                 <div class="chat-confirm-line"></div>`
                 messagesEl.appendChild(confirmRow)
             } else {
                 // For other question types, show the actual question component with answer pre-selected
                 const block = document.createElement('div')
                 block.className = 'chat-question-block'
                 block.setAttribute('data-question-index', String(i))

                 const answerRegion = document.createElement('div')
                 answerRegion.className = 'chat-answer-region'

                 const el = components[i].getElement()
                 el.classList.add('chat-question-component')
                 answerRegion.appendChild(el)

                 const confirmRow = document.createElement('div')
                 confirmRow.className = 'chat-confirm-row chat-confirm-row--confirmed'
                 confirmRow.setAttribute('data-confirm-for', String(i))
                 confirmRow.innerHTML = `
                 <div class="chat-confirm-line"></div>
                 <div class="chat-confirm-btn" aria-hidden="true">${CHECKMARK_SVG}</div>
                 <div class="chat-confirm-line"></div>`

                 block.appendChild(answerRegion)
                 block.appendChild(confirmRow)
                 messagesEl.appendChild(block)

                 // Re-apply answer after DOM append so range slider positions correctly
                 const saved = savedProgress!.answersByQuestionId.get(q.id!)
                 if (saved !== undefined && saved !== null) {
                     components[i].setAnswer(saved)
                 }
             }

             scrollToBottom()
         }

         // Show resume message then continue (outside the replay loop)
         await appendAiBubble(t.resuming)

         if (resumeAt < questions.length) {
             await revealQuestion(resumeAt)
         } else {
             await showSubmitSection()
         }
    } else {
        if (savedProgress) {
            confirmedUpToIndex = Math.max(0, Math.min(savedProgress.currentQuestionIndex, questions.length))
            const savedOpenTextDrafts = savedProgress.openTextDraftsByQuestionId ?? new Map<number, string[]>()
            savedOpenTextDrafts.forEach((messages, questionId) => {
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
