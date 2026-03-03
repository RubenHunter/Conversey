export interface ResponseAnswer {
    questionId: number
    selectedOptionId?: number
    openTextValue?: string
}

export interface SurveyResponse {
    projectId: number
    answers: ResponseAnswer[]
}

