export type IdeaAuthorType = 'self' | 'other'

export interface IdeaReactionSummary {
    emoji: string
    count: number
}

export interface IdeaTopic {
    id: number
    projectId: string | number
    title: string
    prompt: string
    order?: number
    maxBroadSelectionLoads: number
}

export interface Idea {
    id: number
    projectId: string | number
    topicId: number
    body: string
    authorType: IdeaAuthorType
    createdAt: string
    reactions: IdeaReactionSummary[]
    pendingReview: boolean
    qualityNudgeBypassed: boolean
    semanticCategories: string[]
}

export interface SubmitIdeaRequest {
    projectId: string | number
    topicId: number
    body: string
    authorType: IdeaAuthorType
    qualityNudgeBypassed?: boolean
}
