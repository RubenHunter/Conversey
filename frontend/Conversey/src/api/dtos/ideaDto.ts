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
    reactions?: Array<{ emoji?: string; Emoji?: string; count?: number; Count?: number }>
    Reactions?: Array<{ emoji?: string; Emoji?: string; count?: number; Count?: number }>
}

export interface ApiSubmitIdeaRequestDto {
    projectId: number
    topicId: number
    content: string
    youthToken: string
}
