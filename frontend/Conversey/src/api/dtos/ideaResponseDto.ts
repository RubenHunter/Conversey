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
