import type { ApiIdeaDto, ApiIdeaTopicDto, ApiSubmitIdeaRequestDto } from '../api/dtos/ideaDto.ts'
import type { Idea, IdeaTopic, SubmitIdeaRequest } from '../models/idea.ts'

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

export function mapApiIdeaToIdea(dto: ApiIdeaDto): Idea {
    return {
        id: pickNumber(dto.id, dto.Id) ?? 0,
        projectId: pickNumber(dto.projectId, dto.ProjectId) ?? 0,
        topicId: pickNumber(dto.topicId, dto.TopicId) ?? 0,
        body: pickString(dto.body, dto.Body) ?? '',
        authorType: dto.authorType ?? dto.AuthorType ?? 'other',
        createdAt: pickString(dto.createdAt, dto.CreatedAt) ?? new Date().toISOString(),
    }
}

export function mapSubmitIdeaRequestToApiSubmitIdeaRequest(request: SubmitIdeaRequest): ApiSubmitIdeaRequestDto {
    return {
        projectId: request.projectId,
        topicId: request.topicId,
        body: request.body,
        authorType: request.authorType,
    }
}

