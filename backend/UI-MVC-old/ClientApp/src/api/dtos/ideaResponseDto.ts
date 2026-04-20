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
    youthId?: string
    YouthId?: string
    youthToken?: string
    YouthToken?: string
    status?: string | number
    Status?: string | number
    reactions?: ApiResponseReactionSummaryDto[]
    Reactions?: ApiResponseReactionSummaryDto[]
}

export interface ApiIdeaThreadDto {
    responses?: ApiIdeaResponseDto[]
    Responses?: ApiIdeaResponseDto[]
}

export interface ApiCreateIdeaResponseRequestDto {
    text: string
    youthId: string
}

export interface ApiCreateResponseReactionRequestDto {
    emoji: string
    youthId: string
}

export interface ApiCreatedReactionDto {
    id?: number
    Id?: number
    reactionId?: number
    ReactionId?: number
    emoji?: string
    Emoji?: string
}

export interface ApiResponseSubmissionResultDto {
    response?: ApiIdeaResponseDto
    Response?: ApiIdeaResponseDto
    suggestion?: string
    Suggestion?: string
    decision?: {
        isAllowed?: boolean
        IsAllowed?: boolean
        suggestion?: string
        Suggestion?: string
    }
    Decision?: {
        isAllowed?: boolean
        IsAllowed?: boolean
        suggestion?: string
        Suggestion?: string
    }
}

export interface ApiUpdateResponseAfterSafetyReviewRequestDto {
    text: string
    youthId: string
    markForReview: boolean
}
