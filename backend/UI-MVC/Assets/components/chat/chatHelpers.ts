import { QuestionType } from '../../models/question'
import type { Question } from '../../models/question'
import type { QuestionAnswer } from '../survey/singleChoiceQuestion'

export function esc(text: string): string {
    return text
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
}

export function wait(ms: number): Promise<void> {
    return new Promise((resolve) => setTimeout(resolve, ms))
}

export function hasAnswer(answer: QuestionAnswer): boolean {
    return Array.isArray(answer) ? answer.length > 0 : answer !== null && answer !== ''
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
