export interface ApiSlugDto {
    Text: string
}

export interface ApiResponseAnswerDto {
    questionId: number
    selectedOptionId?: number
    openTextValue?: string
}

export interface ApiSurveyResponseRequestDto {
    projectId: ApiSlugDto
    youthId: string
    answers: ApiResponseAnswerDto[]
}
