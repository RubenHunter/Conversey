import type { ApiIdeaDto, ApiIdeaTopicDto, ApiSubmitIdeaRequestDto } from '../api/dtos/ideaDto.ts'
import type { Idea, IdeaReactionSummary, IdeaTopic, SubmitIdeaRequest } from '../models/idea.ts'

function pickNumber(...values: Array<number | undefined>): number | undefined {
    return values.find((value) => typeof value === 'number' && Number.isFinite(value))
}

function pickString(...values: Array<string | undefined>): string | undefined {
    return values.find((value) => typeof value === 'string' && value.length > 0)
}

export function mapApiIdeaTopicToIdeaTopic(dto: ApiIdeaTopicDto): IdeaTopic {
    const id = pickNumber(dto.id, dto.Id) ?? 0

    return {
        id,
        projectId: pickNumber(dto.projectId, dto.ProjectId) ?? 0,
        title: pickString(dto.title, dto.Title) ?? `Topic ${id}`,
        prompt: pickString(dto.prompt, dto.Prompt) ?? '',
        order: pickNumber(dto.order, dto.Order),
    }
}

export function mapApiIdeaToIdea(dto: ApiIdeaDto, currentYouthToken?: string): Idea {
    const youthToken = pickString(dto.youthToken, dto.YouthToken)
    const rawReactions = dto.reactions ?? dto.Reactions ?? []
    const reactions: IdeaReactionSummary[] = rawReactions
        .map((reaction) => ({
            emoji: pickString(reaction.emoji, reaction.Emoji) ?? '',
            count: pickNumber(reaction.count, reaction.Count) ?? 0,
        }))
        .filter((reaction) => reaction.emoji.length > 0)

    return {
        id: pickNumber(dto.id, dto.Id) ?? 0,
        projectId: pickNumber(dto.projectId, dto.ProjectId) ?? 0,
        topicId: pickNumber(dto.topicId, dto.TopicId) ?? 0,
        body: pickString(dto.body, dto.Body, dto.content, dto.Content) ?? '',
        authorType: youthToken && currentYouthToken && youthToken === currentYouthToken ? 'self' : dto.authorType ?? dto.AuthorType ?? 'other',
        createdAt: pickString(dto.createdAt, dto.CreatedAt, dto.submissionDate, dto.SubmissionDate) ?? new Date().toISOString(),
        reactions,
    }
}

export function mapSubmitIdeaRequestToApiSubmitIdeaRequest(request: SubmitIdeaRequest, youthToken: string): ApiSubmitIdeaRequestDto {
    return {
        projectId: request.projectId,
        topicId: request.topicId,
        content: request.body,
        youthToken,
    }
}
