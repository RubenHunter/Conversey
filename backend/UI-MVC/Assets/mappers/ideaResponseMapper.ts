import type { ApiIdeaResponseDto, ApiResponseReactionSummaryDto } from '../api/dtos/ideaResponseDto'
import type { IdeaResponse, ResponseReactionSummary } from '../models/ideaResponse'

function pickNumber(...values: Array<number | undefined>): number | undefined {
    return values.find((value) => typeof value === 'number' && Number.isFinite(value))
}

function pickString(...values: Array<string | undefined>): string | undefined {
    return values.find((value) => typeof value === 'string' && value.length > 0)
}

export function mapApiReactionSummaryToReactionSummary(dto: ApiResponseReactionSummaryDto): ResponseReactionSummary {
    return {
        emoji: pickString(dto.emoji, dto.Emoji) ?? '',
        count: pickNumber(dto.count, dto.Count) ?? 0,
    }
}

function isPendingStatus(status: unknown): boolean {
    if (typeof status === 'number') {
        return status === 0
    }

    if (typeof status === 'string') {
        const normalized = status.trim().toLowerCase()
        return normalized === 'pending' || normalized === '0'
    }

    return false
}

export function mapApiIdeaResponseToIdeaResponse(dto: ApiIdeaResponseDto, youthToken: string): IdeaResponse {
    const authorToken = pickString(dto.youthId, dto.YouthId) ?? ''
    const rawReactions = dto.reactions ?? dto.Reactions ?? []

    return {
        id: pickNumber(dto.id, dto.Id) ?? 0,
        ideaId: pickNumber(dto.ideaId, dto.IdeaId) ?? 0,
        text: pickString(dto.text, dto.Text) ?? '',
        createdAt: pickString(dto.createdAt, dto.CreatedAt) ?? new Date().toISOString(),
        youthToken: authorToken,
        author: authorToken.length > 0 && authorToken === youthToken ? 'self' : 'other',
        offensiveContentDetected: isPendingStatus(dto.status ?? dto.Status),
        reactions: rawReactions.map(mapApiReactionSummaryToReactionSummary),
    }
}
