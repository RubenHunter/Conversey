import { QuestionType } from '../../../models/question'
import type { Question } from '../../../models/question'
import type { ResponseAnswer } from '../../../models/response'
import type { QuestionAnswer, QuestionComponent } from '../../survey/components/singleChoiceQuestion'
import { esc } from '../../survey/utils/surveyUtils'

export { esc }

export function wait(ms: number): Promise<void> {
    return new Promise((resolve) => setTimeout(resolve, ms))
}

export function hasAnswer(answer: QuestionAnswer): boolean {
    return Array.isArray(answer) ? answer.length > 0 : answer !== null && answer !== ''
}

export function mapAnswersToResponse(questions: Question[], components: QuestionComponent[]): ResponseAnswer[] {
    return questions.reduce<ResponseAnswer[]>((acc, q, i) => {
        const answer = components[i].getAnswer()
        if (q.type === QuestionType.SingleChoice) {
            const id = answer as number
            if (id != null) {
                acc.push({ questionId: q.id, selectedOptionId: id })
            }
            return acc
        }
        if (q.type === QuestionType.MultipleChoice) {
            const ids = Array.isArray(answer) ? answer : []
            ids.forEach((id) => {
                acc.push({ questionId: q.id, selectedOptionId: id })
            })
            return acc
        }
        if (q.type === QuestionType.Scale) {
            const val = answer as number
            if (val != null) {
                acc.push({ questionId: q.id, selectedOptionId: val })
            }
            return acc
        }
        const text = answer as string
        if (text?.trim()) {
            acc.push({ questionId: q.id, openTextValue: text })
        }
        return acc
    }, [])
}

export function formatAnswerForDisplay(question: Question, answer: QuestionAnswer): string {
    if (answer === null || answer === '') return ''
    if (typeof answer === 'string') return answer
    if (typeof answer === 'number') {
        if (question.type === QuestionType.SingleChoice && question.options) {
            return question.options.find((option) => option.id === answer)?.text ?? String(answer)
        }
        return String(answer)
    }
    if (Array.isArray(answer) && question.options) {
        return answer.map((id) => question.options?.find((option) => option.id === id)?.text ?? String(id)).join(', ')
    }
    return ''
}

export function bindChatIdeasDesktopLayout(root: HTMLElement): () => void {
    const sync = (): void => {
        root.classList.toggle('chat-shell--ideas-desktop', window.innerWidth >= 1024)
    }

    sync()
    window.addEventListener('resize', sync)

    return () => {
        window.removeEventListener('resize', sync)
    }
}