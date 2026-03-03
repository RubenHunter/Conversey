export enum QuestionType {
    SingleChoice = "SINGLE_CHOICE",
    OpenText = "OPEN_TEXT"
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
    options?: AnswerOption[]
}