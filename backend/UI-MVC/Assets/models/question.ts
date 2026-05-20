export enum QuestionType {
    SingleChoice,
    MultipleChoice,
    OpenText,
    Scale,
}

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
    lowerBound?: number
    upperBound?: number
}