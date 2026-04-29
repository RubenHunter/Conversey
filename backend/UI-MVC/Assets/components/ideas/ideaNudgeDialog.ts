import { assessIdeaNudging } from '../../services/ideaService.ts'
import type { ActiveView } from './types.ts'
import type { IdeaNudgingContext, IdeaNudgingTurn } from '../../services/ideaService.ts'

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
    const closeBtn = root.querySelector<HTMLButtonElement>('#idea-nudge-close')
    const thread = root.querySelector<HTMLDivElement>('#idea-nudge-thread')
    const contextEl = root.querySelector<HTMLParagraphElement>('#idea-nudge-context')
    const input = root.querySelector<HTMLTextAreaElement>('#idea-nudge-input')
    const actionBtn = root.querySelector<HTMLButtonElement>('#idea-nudge-action')
    const statusEl = root.querySelector<HTMLParagraphElement>('#idea-nudge-status')

    if (!backdrop || !dialog || !closeBtn || !thread || !contextEl || !input || !actionBtn || !statusEl) {
        console.warn('[ideaNudgeDialog] Required modal elements are missing. Nudge dialog is disabled for this view.')
        return {
            run: async (initialText: string) => ({ proceed: true, finalText: initialText.trim(), bypassQualityNudging: true }),
        }
    }

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
        dialog.hidden = true
        backdrop.hidden = true
        dialog.classList.remove('open')
        backdrop.classList.remove('open')
        const resolver = activeResolver
        activeResolver = null
        resolver?.(result ?? { proceed: true, finalText: composeDraft(initialIdea, conversation), bypassQualityNudging: true })
    }

    function appendThread(role: 'assistant' | 'user', text: string): void {
        const row = document.createElement('div')
        row.className = `idea-nudge-thread-row idea-nudge-thread-row--${role}`
        row.innerHTML = `<div class="idea-nudge-thread-bubble idea-nudge-thread-bubble--${role}">${text.replace(/</g, '&lt;').replace(/>/g, '&gt;')}</div>`
        thread.appendChild(row)
        thread.scrollTop = thread.scrollHeight
    }

    function setStatus(text: string): void {
        statusEl.textContent = text
    }

    function setActionState(nextLabel: string, enabled: boolean): void {
        actionBtn.textContent = nextLabel
        actionBtn.disabled = !enabled
    }

    function resetDialog(text: string, activeView: ActiveView): void {
        thread.innerHTML = ''
        conversation = []
        initialIdea = text.trim()
        currentTopicId = activeView.type === 'topic' ? activeView.topicId : null
        approved = false
        resolved = false
        const context = getContext(activeView)
        contextEl.textContent = context
            ? `${context.projectTitle} · ${context.topicTitle}${context.topicPrompt ? ` — ${context.topicPrompt}` : ''}`
            : ''
        input.value = ''
        input.disabled = false
        input.placeholder = 'Type your answer here...'
        setActionState('Answer & continue', true)
        setStatus('The AI will ask one question at a time. Close the dialog to post the current version as pending review.')
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
            input.disabled = true
            input.placeholder = 'AI approved this idea.'
            setActionState('Post final idea', true)
            setStatus('The AI is happy with this idea. Click to post the final version.')
            return
        }

        const question = decision.question ?? 'Could you make this idea more specific for this topic?'
        lastQuestion = question
        appendThread('assistant', question)
        setStatus('Answer the question below to continue.')
        input.value = ''
        input.disabled = false
        input.focus()
    }

    async function submitAnswer(activeView: ActiveView): Promise<void> {
        if (resolved || approved) return
        const answer = input.value.trim()
        if (answer.length === 0) return

        appendThread('user', answer)
        conversation.push({ question: lastQuestion, answer })
        input.value = ''
        setActionState('Checking...', false)
        await askNextQuestion(activeView)
        if (!resolved && !approved) {
            setActionState('Answer & continue', true)
        }
    }

    function open(text: string, activeView: ActiveView): Promise<IdeaNudgingResult> {
        resetDialog(text, activeView)
        dialog.hidden = false
        backdrop.hidden = false
        requestAnimationFrame(() => {
            dialog.classList.add('open')
            backdrop.classList.add('open')
        })

        return new Promise<IdeaNudgingResult>((resolve) => {
            activeResolver = resolve
            void askNextQuestion(activeView)
        })
    }

    backdrop.addEventListener('click', () => {
        close({ proceed: true, finalText: composeDraft(initialIdea, conversation), bypassQualityNudging: true })
    })

    closeBtn.addEventListener('click', () => {
        close({ proceed: true, finalText: composeDraft(initialIdea, conversation), bypassQualityNudging: true })
    })

    actionBtn.addEventListener('click', () => {
        const activeView = isCurrentView()
        if (approved) {
            close({ proceed: true, finalText: composeDraft(initialIdea, conversation), bypassQualityNudging: false })
            return
        }
        void submitAnswer(activeView)
    })

    input.addEventListener('keydown', (event) => {
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


