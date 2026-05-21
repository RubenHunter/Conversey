import { assessIdeaNudging } from '../../../services/ideaService'
import { getSurveyStrings } from '../../../i18n/survey'
import type { ActiveView } from '../types'
import type { IdeaNudgingContext, IdeaNudgingTurn } from '../../../services/ideaService'

export interface IdeaNudgingResult {
    proceed: boolean
    finalText: string
    bypassQualityNudging: boolean
}

interface CreateIdeaNudgeDialogParams {
    root: ParentNode
    workspaceSlug: string
    projectSlug: string
    isCurrentView: () => ActiveView
    getContext: (activeView: ActiveView) => IdeaNudgingContext | null
}

interface IdeaNudgeDialogController {
    run(initialText: string, activeView: ActiveView): Promise<IdeaNudgingResult>
}

function composeDraft(initialText: string, conversation: IdeaNudgingTurn[]): string {
    const fragments = [initialText.trim()]
    conversation.forEach((turn) => {
        const answer = turn.answer.trim()
        if (answer.length > 0) fragments.push(answer)
    })
    return fragments.join(' ').replace(/\s+/g, ' ').trim()
}

function truncate(desc: string | null | undefined, maxChars: number): string {
    if (!desc) return ''
    return desc.length > maxChars ? desc.slice(0, maxChars) + '…' : desc
}

export function createIdeaNudgeDialogController({
    root,
    workspaceSlug,
    projectSlug,
    isCurrentView,
    getContext,
}: CreateIdeaNudgeDialogParams): IdeaNudgeDialogController {
    const t = getSurveyStrings()
    const backdrop = root.querySelector<HTMLDivElement>('#idea-nudge-backdrop')
    const dialog = root.querySelector<HTMLDivElement>('#idea-nudge-dialog')
    const closeBtnEl = root.querySelector<HTMLButtonElement>('#idea-nudge-close')
    const thread = root.querySelector<HTMLDivElement>('#idea-nudge-thread')
    const contextEl = root.querySelector<HTMLParagraphElement>('#idea-nudge-context')
    const input = root.querySelector<HTMLTextAreaElement>('#idea-nudge-input')
    const actionBtn = root.querySelector<HTMLButtonElement>('#idea-nudge-action')
    const statusEl = root.querySelector<HTMLParagraphElement>('#idea-nudge-status')

    if (!backdrop || !dialog || !closeBtnEl || !thread || !contextEl || !input || !actionBtn || !statusEl) {
        console.warn('[ideaNudgeDialog] Required modal elements are missing. Nudge dialog is disabled for this view.')
        return {
            run: async (initialText: string) => ({ proceed: true, finalText: initialText.trim(), bypassQualityNudging: true }),
        }
    }

    const backdropEl = backdrop!
    const dialogEl = dialog!
    const threadEl = thread!
    const contextElEl = contextEl!
    const inputEl = input!
    const actionBtnEl = actionBtn!
    const statusElEl = statusEl!

    let activeResolver: ((result: IdeaNudgingResult) => void) | null = null
    let conversation: IdeaNudgingTurn[] = []
    let initialIdea = ''
    let currentTopicId: number | null = null
    let approved = false
    let resolved = false
    let lastQuestion = ''

    function close(result?: IdeaNudgingResult): void {
        if (resolved) return
        resolved = true
        dialogEl.hidden = true
        backdropEl.hidden = true
        dialogEl.classList.remove('open')
        backdropEl.classList.remove('open')
        const resolver = activeResolver
        activeResolver = null
        resolver?.(result ?? { proceed: true, finalText: composeDraft(initialIdea, conversation), bypassQualityNudging: true })
    }

    function appendThread(role: 'assistant' | 'user', text: string): void {
        const row = document.createElement('div')
        row.className = `idea-nudge-thread-row idea-nudge-thread-row--${role}`
        const bubble = document.createElement('div')
        bubble.className = `idea-nudge-thread-bubble idea-nudge-thread-bubble--${role}`
        bubble.textContent = text

        if (role === 'assistant') {
            const avatar = document.createElement('span')
            avatar.className = 'idea-nudge-avatar'
            avatar.setAttribute('aria-hidden', 'true')
            avatar.innerHTML = '<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="m21.64 3.64-1.28-1.28a1.21 1.21 0 0 0-1.72 0L2.36 18.64a1.21 1.21 0 0 0 0 1.72l1.28 1.28a1.2 1.2 0 0 0 1.72 0L21.64 5.36a1.2 1.2 0 0 0 0-1.72"/><path d="m14 7 3 3"/><path d="M5 6v4"/><path d="M19 14v4"/><path d="M10 2v2"/><path d="M7 8H3"/><path d="M21 16h-4"/><path d="M11 3H9"/></svg>'
            row.prepend(avatar)
        }

        row.appendChild(bubble)
        threadEl.appendChild(row)
        threadEl.scrollTop = threadEl.scrollHeight
    }

    function appendStatus(text: string, className: string = ''): void {
        const row = document.createElement('div')
        row.className = `idea-nudge-thread-row idea-nudge-thread-row--status${className ? ' ' + className : ''}`
        row.textContent = text
        threadEl.appendChild(row)
        threadEl.scrollTop = threadEl.scrollHeight
    }

    function setStatus(text: string): void {
        statusElEl.textContent = text
    }

    function setActionState(nextLabel: string, enabled: boolean): void {
        actionBtnEl.textContent = nextLabel
        actionBtnEl.disabled = !enabled
    }

    function resetDialog(text: string, activeView: ActiveView): void {
        threadEl.innerHTML = ''
        conversation = []
        initialIdea = text.trim()
        currentTopicId = activeView.type === 'topic' ? activeView.topicId : null
        approved = false
        resolved = false
        const context = getContext(activeView)
        if (context) {
            const truncatedDesc = truncate(context.projectDescription, 500)
            contextElEl.innerHTML = `<span class="idea-nudge-context-project">${escapeHtml(context.projectTitle)}</span> · <span class="idea-nudge-context-topic">${escapeHtml(context.topicTitle)}</span>`
            if (truncatedDesc) {
                contextElEl.innerHTML += `<span class="idea-nudge-context-desc">${escapeHtml(truncatedDesc)}</span>`
            }
        } else {
            contextElEl.innerHTML = ''
        }
        inputEl.value = ''
        inputEl.disabled = true
        inputEl.placeholder = ''
        setActionState('', false)

        // Show the user's draft as the first message
        if (initialIdea) {
            appendThread('user', initialIdea)
        }
        appendStatus(t.nudgeThinking, 'idea-nudge-thread-row--thinking')
        setStatus(t.nudgeThinking)
        actionBtnEl.style.display = 'none'
    }

    async function askNextQuestion(activeView: ActiveView): Promise<void> {
        if (resolved) return
        if (activeView.type !== 'topic' || currentTopicId !== activeView.topicId || !isCurrentView()) {
            close()
            return
        }

        const context = getContext(activeView)
        if (!context) {
            close({ proceed: false, finalText: '', bypassQualityNudging: true })
            return
        }

        // Truncate project description to prevent dominating the AI prompt
        const trimmedContext: IdeaNudgingContext = {
            ...context,
            projectDescription: truncate(context.projectDescription, 500),
        }

        const decision = await assessIdeaNudging(
            workspaceSlug,
            projectSlug,
            activeView.topicId,
            composeDraft(initialIdea, conversation),
            trimmedContext,
            conversation,
        )

        if (resolved) return

        // Remove thinking status
        const thinkingRow = threadEl.querySelector('.idea-nudge-thread-row--thinking')
        thinkingRow?.remove()

        if (decision.isApproved) {
            approved = true
            inputEl.disabled = true
            inputEl.placeholder = ''
            actionBtnEl.style.display = ''
            setActionState(t.post, true)
            setStatus(t.nudgeApproved)
            appendStatus(t.nudgeApproved, 'idea-nudge-thread-row--approved')
            return
        }

        const question = decision.question ?? t.pleaseFill
        lastQuestion = question
        appendThread('assistant', question)
        setStatus(t.nudgeQuestionStatus)
        inputEl.disabled = false
        inputEl.placeholder = question
        inputEl.focus()
        actionBtnEl.style.display = ''
        setActionState(t.answerContinue, true)
    }

    async function submitAnswer(activeView: ActiveView): Promise<void> {
        if (resolved || approved) return
        const answer = inputEl.value.trim()
        if (answer.length === 0) return

        appendThread('user', answer)
        conversation.push({ question: lastQuestion, answer })
        inputEl.value = ''
        inputEl.disabled = true
        setActionState('', false)
        actionBtnEl.style.display = 'none'
        appendStatus(t.nudgeThinking, 'idea-nudge-thread-row--thinking')
        setStatus(t.nudgeThinking)
        await askNextQuestion(activeView)
    }

    function open(text: string, activeView: ActiveView): Promise<IdeaNudgingResult> {
        resetDialog(text, activeView)
        dialogEl.hidden = false
        backdropEl.hidden = false
        requestAnimationFrame(() => {
            dialogEl.classList.add('open')
            backdropEl.classList.add('open')
        })

        return new Promise<IdeaNudgingResult>((resolve) => {
            activeResolver = resolve
            void askNextQuestion(activeView)
        })
    }

    backdropEl.addEventListener('click', () => {
        close({ proceed: true, finalText: composeDraft(initialIdea, conversation), bypassQualityNudging: true })
    })

    closeBtnEl.addEventListener('click', () => {
        close({ proceed: true, finalText: composeDraft(initialIdea, conversation), bypassQualityNudging: true })
    })

    actionBtnEl.addEventListener('click', () => {
        const activeView = isCurrentView()
        if (approved) {
            close({ proceed: true, finalText: composeDraft(initialIdea, conversation), bypassQualityNudging: false })
            return
        }
        void submitAnswer(activeView)
    })

    inputEl.addEventListener('keydown', (event) => {
        if (event.key === 'Enter' && !event.shiftKey) {
            event.preventDefault()
            const activeView = isCurrentView()
            if (approved) {
                close({ proceed: true, finalText: composeDraft(initialIdea, conversation), bypassQualityNudging: false })
                return
            }
            void submitAnswer(activeView)
        }
    })

    return {
        run: open,
    }
}

function escapeHtml(text: string): string {
    return text.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;')
}
