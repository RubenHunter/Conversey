type ApiSlugValue = string | { text?: string; Text?: string }

export interface ApiProjectThemeDto {
    primary?: string
    Primary?: string
    secondary?: string
    Secondary?: string
    accent?: string
    Accent?: string
    preset?: string
    Preset?: string
    font?: string
    Font?: string
}

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

export type ApiProjectStatusDto = 'Draft' | 'Active' | 'Archived' | string | number
export type ApiInteractionTypeDto = 'Chat' | 'Vertical_Scroll' | 'VerticalScroll' | string | number

export interface ApiProjectDto {
    id?: ApiSlugValue
    Id?: ApiSlugValue
    organizationId?: ApiSlugValue
    OrganizationId?: ApiSlugValue
    organizationName?: string
    OrganizationName?: string
    name?: string
    Name?: string
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
    interactionForm?: ApiInteractionTypeDto
    InteractionForm?: ApiInteractionTypeDto
    nudgingStrength?: number
    NudgingStrength?: number
    topic?: ApiTopicDto
    Topic?: ApiTopicDto
    topics?: ApiTopicDto[]
    Topics?: ApiTopicDto[]
    theme?: ApiProjectThemeDto
    Theme?: ApiProjectThemeDto
}
