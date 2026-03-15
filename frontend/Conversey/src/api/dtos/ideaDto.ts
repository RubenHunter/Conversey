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
    content?: string
    Content?: string
    authorType?: 'self' | 'other'
    AuthorType?: 'self' | 'other'
    youthToken?: string
    YouthToken?: string
    createdAt?: string
    CreatedAt?: string
    submissionDate?: string
    SubmissionDate?: string
}

export interface ApiSubmitIdeaRequestDto {
    projectId: number
    topicId: number
    content: string
    youthToken: string
}
