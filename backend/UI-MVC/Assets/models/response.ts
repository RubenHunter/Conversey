export type ResponseValue = string | number | boolean | string[]

export interface ResponseAnswer {
    questionId: number
    selectedOptionId?: number
    openTextValue?: string
    value?: ResponseValue
}

export interface SurveyResponse {
    projectId: string
    answers: ResponseAnswer[]
}
