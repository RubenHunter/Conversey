export type IdeaAuthorType = 'self' | 'other'

export interface IdeaReactionSummary {
    emoji: string
    count: number
}

export interface IdeaTopic {
    id: number
    projectId: number
    title: string
    prompt: string
    order?: number
}

export interface Idea {
    id: number
    projectId: number
    topicId: number
    body: string
    authorType: IdeaAuthorType
    createdAt: string
    reactions: IdeaReactionSummary[]
    pendingReview: boolean
    semanticCategories: string[]
}

export interface SubmitIdeaRequest {
    projectId: number
    topicId: number
    body: string
    authorType: IdeaAuthorType
}
