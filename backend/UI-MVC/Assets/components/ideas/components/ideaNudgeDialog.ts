import { assessIdeaNudging } from '../../../services/ideaService'
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

export function createIdeaNudgeDialogController({
    root,
    workspaceSlug,
    projectSlug,
    isCurrentView,
    getContext,
}: CreateIdeaNudgeDialogParams): IdeaNudgeDialogController {
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

    // Non-null assertions after guard clause
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
        row.innerHTML = `<div class="idea-nudge-thread-bubble idea-nudge-thread-bubble--${role}">${text.replace(/</g, '&lt;').replace(/>/g, '&gt;')}</div>`
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
        contextElEl.textContent = context
            ? `${context.topicTitle}`
            : ''
        inputEl.value = ''
        inputEl.disabled = false
        inputEl.placeholder = 'Answer the AI question here...'
        setActionState('Answer & continue', true)
        setStatus('The AI checks your idea quality. Close to post as-is.')
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

        const decision = await assessIdeaNudging(
            workspaceSlug,
            projectSlug,
            activeView.topicId,
            composeDraft(initialIdea, conversation),
            context,
            conversation,
        )

        if (resolved) return

        if (decision.isApproved) {
            approved = true
            inputEl.disabled = true
            inputEl.placeholder = 'AI approved this idea.'
            setActionState('Post final idea', true)
            setStatus('The AI is happy with this idea. Click to post the final version.')
            return
        }

        const question = decision.question ?? 'Could you make this idea more specific for this topic?'
        lastQuestion = question
        appendThread('assistant', question)
        setStatus('Answer the question below to continue.')
        inputEl.value = ''
        inputEl.disabled = false
        inputEl.focus()
    }

    async function submitAnswer(activeView: ActiveView): Promise<void> {
        if (resolved || approved) return
        const answer = inputEl.value.trim()
        if (answer.length === 0) return

        appendThread('user', answer)
        conversation.push({ question: lastQuestion, answer })
        inputEl.value = ''
        setActionState('Checking...', false)
        await askNextQuestion(activeView)
        if (!resolved && !approved) {
            setActionState('Answer & continue', true)
        }
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


