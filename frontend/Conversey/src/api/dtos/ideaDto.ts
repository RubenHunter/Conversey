export interface ApiIdeaTopicDto {
    id?: number
    Id?: number
    projectId?: number
    ProjectId?: number
    title?: string
    Title?: string
    prompt?: string
    Prompt?: string
    order?: number
    Order?: number
}

export interface ApiIdeaDto {
    id?: number
    Id?: number
    projectId?: number
    ProjectId?: number
    topicId?: number
    TopicId?: number
    body?: string
    Body?: string
    authorType?: 'self' | 'other'
    AuthorType?: 'self' | 'other'
    createdAt?: string
    CreatedAt?: string
}

export interface ApiSubmitIdeaRequestDto {
    projectId: number
    topicId: number
    body: string
    authorType: 'self' | 'other'
}

