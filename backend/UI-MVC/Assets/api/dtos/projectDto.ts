type ApiSlugValue = string | { text?: string; Text?: string }

export interface ApiTopicDto {
    id?: number
    Id?: number
    name?: string
    Name?: string
    context?: string
    Context?: string
    maxBroadSelectionLoads?: number
    MaxBroadSelectionLoads?: number
}

export interface ApiProjectStyleDto {
    primaryColor?: string[]
    PrimaryColor?: string[]
}

export type ApiProjectStatusDto = 'Draft' | 'Active' | 'Archived' | string | number
export type ApiInteractionTypeDto = 'Chat' | 'Vertical_Scroll' | 'VerticalScroll' | string | number

export interface ApiProjectDto {
    id?: ApiSlugValue
    Id?: ApiSlugValue
    organizationId?: ApiSlugValue
    OrganizationId?: ApiSlugValue
    slug?: string
    Slug?: string
    organizationSlug?: string
    OrganizationSlug?: string
    organizationName?: string
    OrganizationName?: string
    title?: string
    Title?: string
    description?: string
    Description?: string
    imageUrl?: string
    ImageUrl?: string
    status?: ApiProjectStatusDto
    Status?: ApiProjectStatusDto
    startDate?: string
    StartDate?: string
    endDate?: string
    EndDate?: string
    interactionType?: ApiInteractionTypeDto
    InteractionType?: ApiInteractionTypeDto
    interactionForm?: ApiInteractionTypeDto
    InteractionForm?: ApiInteractionTypeDto
    language?: string
    Language?: string
    topic?: ApiTopicDto
    Topic?: ApiTopicDto
    topics?: ApiTopicDto[]
    Topics?: ApiTopicDto[]
    style?: ApiProjectStyleDto
    Style?: ApiProjectStyleDto
}
