export const QuestionType = {
    SingleChoice: 'SINGLE_CHOICE',
    MultipleChoice: 'MULTIPLE_CHOICE',
    OpenText: 'OPEN_TEXT',
    Scale: 'SCALE',
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
    hint?: string
    order?: number
    backendType?: string
    options?: AnswerOption[]
    // Scale question properties
    lowerBound?: number
    upperBound?: number
}