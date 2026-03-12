export type IdeaAuthorType = 'self' | 'other'

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
}

export interface SubmitIdeaRequest {
    projectId: number
    topicId: number
    body: string
    authorType: IdeaAuthorType
}

