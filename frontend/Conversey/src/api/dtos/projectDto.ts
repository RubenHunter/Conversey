export interface ApiTopicDto {
    name?: string
    Name?: string
    context?: string
    Context?: string
}

export interface ApiProjectStyleDto {
    primaryColor?: string[]
    PrimaryColor?: string[]
}

export type ApiProjectStatusDto = 'Draft' | 'Active' | 'Archived' | string | number
export type ApiInteractionTypeDto = 'Chat' | 'Vertical_Scroll' | 'VerticalScroll' | string | number

export interface ApiProjectDto {
    id?: number
    Id?: number
    slug?: string
    Slug?: string
    organizationSlug?: string
    OrganizationSlug?: string
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
    topic?: ApiTopicDto
    Topic?: ApiTopicDto
    style?: ApiProjectStyleDto
    Style?: ApiProjectStyleDto
}

