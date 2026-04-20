export interface ApiResponseAnswerDto {
    questionId: number
    selectedOptionId?: number
    openTextValue?: string
}

export interface ApiSurveyResponseRequestDto {
    projectId: number
    youthId: string
    answers: ApiResponseAnswerDto[]
}
