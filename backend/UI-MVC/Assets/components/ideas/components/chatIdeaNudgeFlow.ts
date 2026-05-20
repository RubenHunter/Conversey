import { assessIdeaNudging } from '../../../services/ideaService'
import type { ActiveView } from '../types'
import type { IdeaNudgingContext, IdeaNudgingTurn } from '../../../services/ideaService'
import type { IdeaNudgingResult } from './ideaNudgeDialog'

interface CreateChatIdeaNudgeFlowParams {
    workspaceSlug: string
    projectSlug: string
    getActiveView: () => ActiveView
    getContext: (activeView: ActiveView) => IdeaNudgingContext | null
    appendAssistantBubble: (text: string) => Promise<void>
    appendUserBubble: (text: string) => void
    setInputDisabled: (disabled: boolean) => void
    setInputPlaceholder: (text: string) => void
    clearInput: () => void
    focusInput: () => void
    showTyping?: () => void
    hideTyping?: () => void
}

export interface ChatIdeaNudgeFlowController {
    start(initialText: string, activeView: ActiveView): Promise<IdeaNudgingResult>
    submitAnswer(answer: string): void
    isActive(): boolean
    cancelIfTopicChanged(): void
}

function composeDraft(initialText: string, answers: string[]): string {
    return [initialText.trim(), ...answers.map((answer) => answer.trim()).filter((answer) => answer.length > 0)]
        .join(' ')
        .replace(/\s+/g, ' ')
        .trim()
}

export function createChatIdeaNudgeFlow({
    workspaceSlug,
    projectSlug,
    getActiveView,
    getContext,
    appendAssistantBubble,
    appendUserBubble,
    setInputDisabled,
    setInputPlaceholder,
    clearInput,
    focusInput,
    showTyping,
    hideTyping,
}: CreateChatIdeaNudgeFlowParams): ChatIdeaNudgeFlowController {
    let active = false
    let initialText = ''
    let topicId: number | null = null
    let answers: string[] = []
    let conversation: IdeaNudgingTurn[] = []
    let pendingAnswerResolver: ((answer: string) => void) | null = null
    let finalResolver: ((result: IdeaNudgingResult) => void) | null = null
    let lastQuestion = ''

    function resolve(result: IdeaNudgingResult): void {
        active = false
        pendingAnswerResolver = null
        const resolver = finalResolver
        finalResolver = null
        resolver?.(result)
    }

    function cancelIfTopicChanged(): void {
        if (!active) return
        const currentView = getActiveView()
        if (currentView.type !== 'topic' || topicId !== currentView.topicId) {
            resolve({
                proceed: false,
                finalText: '',
                bypassQualityNudging: true,
            })
        }
    }

    async function askNextQuestion(): Promise<void> {
        cancelIfTopicChanged()
        if (!active) return

        const currentView = getActiveView()
        if (currentView.type !== 'topic' || topicId !== currentView.topicId) {
            resolve({ proceed: false, finalText: '', bypassQualityNudging: true })
            return
        }

        const context = getContext(currentView)
        if (!context) {
            resolve({ proceed: false, finalText: '', bypassQualityNudging: true })
            return
        }

        showTyping?.()
        const decision = await assessIdeaNudging(
            workspaceSlug,
            projectSlug,
            currentView.topicId,
            composeDraft(initialText, answers),
            context,
            conversation,
        )
        hideTyping?.()

        if (!active) return

        if (decision.isApproved) {
            setInputDisabled(true)
            setInputPlaceholder('AI approved this idea.')
            clearInput()
            resolve({
                proceed: true,
                finalText: composeDraft(initialText, answers),
                bypassQualityNudging: false,
            })
            return
        }

        lastQuestion = decision.question ?? 'Could you make this idea more specific for this topic?'
        await appendAssistantBubble(lastQuestion)
        setInputDisabled(false)
        setInputPlaceholder('Answer the AI question and continue...')
        focusInput()

        await new Promise<void>((resolveAnswer) => {
            pendingAnswerResolver = (answer: string) => {
                answers.push(answer)
                conversation.push({ question: lastQuestion, answer })
                appendUserBubble(answer)
                pendingAnswerResolver = null
                resolveAnswer()
            }
        })

        if (active) {
            await askNextQuestion()
        }
    }

    async function start(nextInitialText: string, nextActiveView: ActiveView): Promise<IdeaNudgingResult> {
        active = true
        initialText = nextInitialText.trim()
        topicId = nextActiveView.type === 'topic' ? nextActiveView.topicId : null
        answers = []
        conversation = []
        lastQuestion = ''
        setInputDisabled(true)
        setInputPlaceholder('AI is checking your idea...')

        if (initialText.length === 0 || nextActiveView.type !== 'topic') {
            resolve({ proceed: false, finalText: '', bypassQualityNudging: true })
            return { proceed: false, finalText: '', bypassQualityNudging: true }
        }

        finalResolver = null
        const resultPromise = new Promise<IdeaNudgingResult>((resolveResult) => {
            finalResolver = resolveResult
        })

        void askNextQuestion()
        return resultPromise
    }

    function submitAnswer(answer: string): void {
        if (!active || !pendingAnswerResolver) return
        const trimmed = answer.trim()
        if (trimmed.length === 0) return
        setInputDisabled(true)
        setInputPlaceholder('Checking your answer...')
        pendingAnswerResolver(trimmed)
    }

    return {
        start,
        submitAnswer,
        isActive: () => active,
        cancelIfTopicChanged,
    }
}

