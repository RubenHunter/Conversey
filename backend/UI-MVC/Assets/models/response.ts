

export interface ResponseAnswer {
    questionId: number
    selectedOptionId?: number
    openTextValue?: string
}

export interface SurveyResponse {
    projectId: string
    answers: ResponseAnswer[]
}
