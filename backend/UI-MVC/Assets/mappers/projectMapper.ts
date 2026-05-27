import type { ApiProjectDto, ApiInteractionTypeDto, ApiProjectStatusDto, ApiTopicDto, ApiProjectThemeDto } from '../api/dtos/projectDto'
import { InteractionType, ProjectStatus, type Project, type ProjectTopic, type ProjectTheme } from '../models/project'

function pickString(...values: Array<string | undefined>): string | undefined {
    return values.find((value) => typeof value === 'string' && value.length > 0)
}

function extractSlugText(value: unknown): string | undefined {
    if (typeof value === 'string' && value.length > 0) {
        return value
    }

    if (value && typeof value === 'object') {
        const maybeSlug = value as { text?: unknown; Text?: unknown }
        if (typeof maybeSlug.text === 'string' && maybeSlug.text.length > 0) {
            return maybeSlug.text
        }
        if (typeof maybeSlug.Text === 'string' && maybeSlug.Text.length > 0) {
            return maybeSlug.Text
        }
    }

    return undefined
}

function toSlug(value: string): string {
    return value
        .trim()
        .toLowerCase()
        .replace(/[^a-z0-9]+/g, '-')
        .replace(/^-+|-+$/g, '')
}

function mapStatus(rawStatus: ApiProjectStatusDto | undefined): Project['status'] {
    if (rawStatus === undefined) return undefined

    if (typeof rawStatus === 'number') {
        if (rawStatus === 0) return ProjectStatus.Draft
        if (rawStatus === 1) return ProjectStatus.Active
        if (rawStatus === 2) return ProjectStatus.Archived
        return undefined
    }

    if (rawStatus === ProjectStatus.Draft || rawStatus.toLowerCase() === 'draft') return ProjectStatus.Draft
    if (rawStatus === ProjectStatus.Active || rawStatus.toLowerCase() === 'active') return ProjectStatus.Active
    if (rawStatus === ProjectStatus.Archived || rawStatus.toLowerCase() === 'archived') return ProjectStatus.Archived

    return undefined
}

function mapInteractionType(rawType: ApiInteractionTypeDto | undefined): Project['interactionType'] {
    if (rawType === undefined) return undefined

    if (typeof rawType === 'number') {
        if (rawType === 0) return InteractionType.UserDefined
        if (rawType === 1) return InteractionType.Chat
        if (rawType === 2) return InteractionType.VerticalScroll
        return undefined
    }

    if (rawType === InteractionType.Chat || rawType.toLowerCase() === 'chat') return InteractionType.Chat
    if (rawType === InteractionType.UserDefined || rawType.toLowerCase() === 'userdefined') return InteractionType.UserDefined

    const normalized = rawType.replace(/\s|-/g, '_').toLowerCase()
    if (normalized === 'vertical_scroll' || normalized === 'verticalscroll') {
        return InteractionType.VerticalScroll
    }

    return undefined
}

function mapTopic(topicDto: ApiTopicDto | undefined): ProjectTopic | undefined {
    if (!topicDto) return undefined

    const id = (topicDto.id ?? topicDto.Id) ?? 0
    const name = pickString(topicDto.name, topicDto.Name)
    const context = pickString(topicDto.context, topicDto.Context) ?? ''

    if (!name) return undefined

    const maxBroadSelectionLoads = topicDto.maxBroadSelectionLoads ?? topicDto.MaxBroadSelectionLoads ?? 3
    return { id, name, context, maxBroadSelectionLoads }
}

function mapTopics(topicDtos: ApiTopicDto[] | undefined): ProjectTopic[] | undefined {
    if (!topicDtos || topicDtos.length === 0) return undefined

    const topics = topicDtos
        .map(mapTopic)
        .filter((topic): topic is ProjectTopic => topic !== undefined)

    return topics.length > 0 ? topics : undefined
}

function mapTheme(dto: ApiProjectThemeDto | undefined): ProjectTheme {
    return {
        primary: pickString(dto?.primary, dto?.Primary) ?? '#6c5ce7',
        secondary: pickString(dto?.secondary, dto?.Secondary) ?? '#db99c8',
        accent: pickString(dto?.accent, dto?.Accent) ?? '#cd6f88',
        preset: pickString(dto?.preset, dto?.Preset) ?? 'default',
        font: pickString(dto?.font, dto?.Font) ?? 'Helvetica',
    }
}

function mapNudgingStrength(rawValue: number | undefined): number {
    if (typeof rawValue !== 'number' || !Number.isFinite(rawValue)) return 3
    return Math.min(5, Math.max(1, Math.trunc(rawValue)))
}

export function mapApiProjectToProject(dto: ApiProjectDto, organizationSlugHint: string, projectSlugHint: string): Project {
    const name = pickString(dto.name, dto.Name) ?? projectSlugHint
    const idSlug = extractSlugText(dto.id ?? dto.Id)
    const slug = idSlug ?? toSlug(name)
    const organizationSlug = extractSlugText(dto.organizationId ?? dto.OrganizationId) ?? organizationSlugHint
    const topics = mapTopics(dto.topics ?? dto.Topics)

    return {
        id: idSlug ?? slug,
        slug,
        organizationSlug,
        organizationName: pickString(dto.organizationName, dto.OrganizationName),
        title: name,
        description: pickString(dto.description, dto.Description) ?? '',
        imageUrl: pickString(dto.imageUrl, dto.ImageUrl) ?? '',
        status: mapStatus(dto.status ?? dto.Status),
        startDate: pickString(dto.startDate, dto.StartDate),
        endDate: pickString(dto.endDate, dto.EndDate),
        interactionType: mapInteractionType(dto.interactionForm ?? dto.InteractionForm),
        nudgingStrength: mapNudgingStrength(dto.nudgingStrength ?? dto.NudgingStrength),
        topic: mapTopic(dto.topic ?? dto.Topic) ?? topics?.[0],
        topics,
        theme: mapTheme(dto.theme ?? dto.Theme),
    }
}
