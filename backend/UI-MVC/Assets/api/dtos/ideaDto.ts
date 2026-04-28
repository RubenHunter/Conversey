type ApiSlugValue = string | { text?: string; Text?: string }

export interface ApiIdeaTopicDto {
    id?: number
    Id?: number
    projectId?: number | ApiSlugValue
    ProjectId?: number | ApiSlugValue
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
    projectId?: number | ApiSlugValue
    ProjectId?: number | ApiSlugValue
    topicId?: number
    TopicId?: number
    body?: string
    Body?: string
    content?: string
    Content?: string
    status?: string | number
    Status?: string | number
    authorType?: 'self' | 'other'
    AuthorType?: 'self' | 'other'
    youthId?: string
    YouthId?: string
    youthToken?: string
    YouthToken?: string
    createdAt?: string
    CreatedAt?: string
    submissionDate?: string
    SubmissionDate?: string
    semanticCategories?: string[]
    SemanticCategories?: string[]
    qualityNudgeBypassed?: boolean
    QualityNudgeBypassed?: boolean
    reactions?: Array<{ emoji?: string; Emoji?: string; count?: number; Count?: number }>
    Reactions?: Array<{ emoji?: string; Emoji?: string; count?: number; Count?: number }>
}

export interface ApiSubmitIdeaRequestDto {
    content: string
    youthId: string
    qualityNudgeBypassed?: boolean
}
