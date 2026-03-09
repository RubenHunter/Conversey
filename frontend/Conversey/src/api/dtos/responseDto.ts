import type { ResponseValue } from '../../models/response.ts'

export interface ApiResponseAnswerDto {
    questionId: number
    answerValue: ResponseValue
    selectedOptionId?: number
    openTextValue?: string
}

export interface ApiSurveyResponseRequestDto {
    projectId: number
    answers: ApiResponseAnswerDto[]
}

