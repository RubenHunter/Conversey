export type ApiQuestionTypeDto = string | number

export interface ApiAnswerOptionDto {
    id?: number
    Id?: number
    questionId?: number
    QuestionId?: number
    text?: string
    Text?: string
}

export interface ApiQuestionDto {
    id?: number
    Id?: number
    projectId?: number
    ProjectId?: number
    text?: string
    Text?: string
    order?: number
    Order?: number
    isRequired?: boolean
    IsRequired?: boolean
    type?: ApiQuestionTypeDto
    Type?: ApiQuestionTypeDto
    questionType?: ApiQuestionTypeDto
    QuestionType?: ApiQuestionTypeDto
    options?: ApiAnswerOptionDto[]
    Options?: ApiAnswerOptionDto[]
}

