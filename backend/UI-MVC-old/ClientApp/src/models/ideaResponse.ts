export interface ResponseReactionSummary {
    emoji: string
    count: number
}

export interface IdeaResponse {
    id: number
    ideaId: number
    text: string
    createdAt: string
    youthToken: string
    author: 'self' | 'other'
    offensiveContentDetected?: boolean
    reactions: ResponseReactionSummary[]
}
