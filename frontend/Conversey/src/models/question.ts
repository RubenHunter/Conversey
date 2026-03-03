export const QuestionType = {
    SingleChoice: 'SINGLE_CHOICE',
    OpenText: 'OPEN_TEXT',
} as const

export type QuestionType = (typeof QuestionType)[keyof typeof QuestionType]

export interface AnswerOption {
    id: number
    questionId: number
    text: string
}

export interface Question {
    id: number
    projectId: number
    text: string
    type: QuestionType
    isRequired: boolean
    options?: AnswerOption[]
}