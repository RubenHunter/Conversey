export interface ApiResponseReactionSummaryDto {
    emoji?: string
    Emoji?: string
    count?: number
    Count?: number
}

export interface ApiIdeaResponseDto {
    id?: number
    Id?: number
    ideaId?: number
    IdeaId?: number
    text?: string
    Text?: string
    createdAt?: string
    CreatedAt?: string
    youthToken?: string
    YouthToken?: string
    status?: string | number
    Status?: string | number
    reactions?: ApiResponseReactionSummaryDto[]
    Reactions?: ApiResponseReactionSummaryDto[]
}

export interface ApiCreateIdeaResponseRequestDto {
    text: string
    youthToken: string
}

export interface ApiCreateResponseReactionRequestDto {
    emoji: string
    youthToken: string
}

export interface ApiResponseSubmissionResultDto {
    response?: ApiIdeaResponseDto
    Response?: ApiIdeaResponseDto
    suggestion?: string
    Suggestion?: string
}

export interface ApiUpdateResponseAfterSafetyReviewRequestDto {
    text: string
    youthToken: string
    markForReview: boolean
}
