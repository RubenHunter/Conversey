export const ProjectStatus = {
    Draft: 'Draft',
    Active: 'Active',
    Archived: 'Archived',
} as const

export type ProjectStatus = (typeof ProjectStatus)[keyof typeof ProjectStatus]

export const InteractionType = {
    Chat: 'Chat',
    VerticalScroll: 'Vertical_Scroll',
} as const

export type InteractionType = (typeof InteractionType)[keyof typeof InteractionType]

export interface ProjectTopic {
    id: number
    name: string
    context: string
    maxBroadSelectionLoads: number
}

export interface ProjectStyle {
    // Backend currently exposes Color[]; string tokens keep frontend transport-safe.
    primaryColors: string[]
}

export interface Project {
    id: string
    slug: string
    organizationSlug: string
    organizationName?: string
    title: string
    description: string
    imageUrl: string
    status?: ProjectStatus
    startDate?: string
    endDate?: string
    interactionType?: InteractionType
    language?: string
    topic?: ProjectTopic
    topics?: ProjectTopic[]
    style?: ProjectStyle
}

/* will need changes: like organizationslug and projectslug are just edited version of the titles.
could be calculated instead of stored.

maybe zstart date and end date, and maybe a status (active, archived, etc.)

topic and style isnt needed yet but will be in the future to load every project according to admin settings



*/