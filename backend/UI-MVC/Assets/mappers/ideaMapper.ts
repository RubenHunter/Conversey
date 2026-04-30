import type { ApiIdeaDto, ApiIdeaTopicDto, ApiSubmitIdeaRequestDto } from '../api/dtos/ideaDto.ts'
import type { Idea, IdeaReactionSummary, IdeaTopic, SubmitIdeaRequest } from '../models/idea.ts'

function pickNumber(...values: Array<number | undefined>): number | undefined {
    return values.find((value) => typeof value === 'number' && Number.isFinite(value))
}

function pickString(...values: Array<string | undefined>): string | undefined {
    return values.find((value) => typeof value === 'string' && value.length > 0)
}

function getSlugText(value: unknown): string | undefined {
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

function slugToStableNumber(slug: string): number {
    // FNV-1a hash, constrained to a positive 31-bit integer for predictable local keys.
    let hash = 2166136261
    for (let index = 0; index < slug.length; index += 1) {
        hash ^= slug.charCodeAt(index)
        hash = Math.imul(hash, 16777619)
    }

    return (hash >>> 0) & 0x7fffffff
}

function mapProjectId(value: unknown): number {
    if (typeof value === 'number' && Number.isFinite(value)) {
        return value
    }

    const slug = getSlugText(value)
    return slug ? slugToStableNumber(slug) : 0
}

export function mapApiIdeaTopicToIdeaTopic(dto: ApiIdeaTopicDto): IdeaTopic {
    const id = pickNumber(dto.id, dto.Id) ?? 0

    return {
        id,
        projectId: mapProjectId(dto.projectId ?? dto.ProjectId),
        title: pickString(dto.title, dto.Title) ?? `Topic ${id}`,
        prompt: pickString(dto.prompt, dto.Prompt) ?? '',
        order: pickNumber(dto.order, dto.Order),
        maxBroadSelectionLoads: 3,
    }
}

function isPendingStatus(status: unknown): boolean {
    if (typeof status === 'number') {
        // C# enum IdeaStatus: Pending=0, Approved=1, Rejected=2
        return status === 0
    }

    if (typeof status === 'string') {
        const normalized = status.trim().toLowerCase()
        return normalized === 'pending' || normalized === '0'
    }

    return false
}

export function mapApiIdeaToIdea(dto: ApiIdeaDto, currentYouthToken?: string): Idea {
    const youthToken = pickString(dto.youthId, dto.YouthId, dto.youthToken, dto.YouthToken)
    const rawReactions = dto.reactions ?? dto.Reactions ?? []
    const reactions: IdeaReactionSummary[] = rawReactions
        .map((reaction) => ({
            emoji: pickString(reaction.emoji, reaction.Emoji) ?? '',
            count: pickNumber(reaction.count, reaction.Count) ?? 0,
        }))
        .filter((reaction) => reaction.emoji.length > 0)

    return {
        id: pickNumber(dto.id, dto.Id) ?? 0,
        projectId: mapProjectId(dto.projectId ?? dto.ProjectId),
        topicId: pickNumber(dto.topicId, dto.TopicId) ?? 0,
        body: pickString(dto.body, dto.Body, dto.content, dto.Content) ?? '',
        authorType: youthToken && currentYouthToken && youthToken === currentYouthToken ? 'self' : dto.authorType ?? dto.AuthorType ?? 'other',
        createdAt: pickString(dto.createdAt, dto.CreatedAt, dto.submissionDate, dto.SubmissionDate) ?? new Date().toISOString(),
        reactions,
        pendingReview: isPendingStatus(dto.status ?? dto.Status),
        qualityNudgeBypassed: Boolean(dto.qualityNudgeBypassed ?? dto.QualityNudgeBypassed),
        semanticCategories: (dto.semanticCategories ?? dto.SemanticCategories ?? [])
            .filter((category): category is string => typeof category === 'string')
            .map((category) => category.trim())
            .filter((category) => category.length > 0),
    }
}

export function mapSubmitIdeaRequestToApiSubmitIdeaRequest(request: SubmitIdeaRequest, youthToken: string): ApiSubmitIdeaRequestDto {
    return {
        content: request.body,
        youthId: youthToken,
        qualityNudgeBypassed: request.qualityNudgeBypassed ?? false,
    }
}
