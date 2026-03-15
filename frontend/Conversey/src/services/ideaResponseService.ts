import type { ApiCreateIdeaResponseRequestDto, ApiCreateResponseReactionRequestDto, ApiIdeaResponseDto, ApiResponseReactionSummaryDto } from '../api/dtos/ideaResponseDto.ts'
import { mapApiIdeaResponseToIdeaResponse, mapApiReactionSummaryToReactionSummary } from '../mappers/ideaResponseMapper.ts'
import type { Idea } from '../models/idea.ts'
import type { IdeaResponse, ResponseReactionSummary } from '../models/ideaResponse.ts'
import { apiFetch } from './apiService.ts'

function getResponsesEndpoint(workspaceSlug: string, projectSlug: string, idea: Idea): string {
    return `/workspaces/${workspaceSlug}/projects/${projectSlug}/topics/${idea.topicId}/ideas/${idea.id}/responses`
}

function getIdeaReactionsEndpoint(workspaceSlug: string, projectSlug: string, idea: Idea): string {
    return `/workspaces/${workspaceSlug}/projects/${projectSlug}/topics/${idea.topicId}/ideas/${idea.id}/reactions`
}

async function getResponseReactionSummary(workspaceSlug: string, projectSlug: string, idea: Idea, responseId: number): Promise<ResponseReactionSummary[]> {
    const endpoint = `${getResponsesEndpoint(workspaceSlug, projectSlug, idea)}/${responseId}/reactions`
    const dtos = await apiFetch<ApiResponseReactionSummaryDto[]>(endpoint)
    return dtos.map(mapApiReactionSummaryToReactionSummary)
}

async function getIdeaReactionSummary(workspaceSlug: string, projectSlug: string, idea: Idea): Promise<ResponseReactionSummary[]> {
    const endpoint = getIdeaReactionsEndpoint(workspaceSlug, projectSlug, idea)
    const dtos = await apiFetch<ApiResponseReactionSummaryDto[]>(endpoint)
    return dtos.map(mapApiReactionSummaryToReactionSummary)
}

export async function getIdeaResponses(workspaceSlug: string, projectSlug: string, idea: Idea, youthToken: string): Promise<IdeaResponse[]> {
    const endpoint = getResponsesEndpoint(workspaceSlug, projectSlug, idea)
    const dtos = await apiFetch<ApiIdeaResponseDto[]>(endpoint)
    return dtos.map((dto) => mapApiIdeaResponseToIdeaResponse(dto, youthToken))
}

export async function addIdeaResponse(workspaceSlug: string, projectSlug: string, idea: Idea, youthToken: string, text: string): Promise<IdeaResponse> {
    const endpoint = getResponsesEndpoint(workspaceSlug, projectSlug, idea)
    const requestDto: ApiCreateIdeaResponseRequestDto = {
        text,
        youthToken,
    }

    const dto = await apiFetch<ApiIdeaResponseDto>(endpoint, {
        method: 'POST',
        body: JSON.stringify(requestDto),
    })

    return mapApiIdeaResponseToIdeaResponse(dto, youthToken)
}

export async function addResponseReaction(workspaceSlug: string, projectSlug: string, idea: Idea, responseId: number, youthToken: string, emoji: string): Promise<ResponseReactionSummary[]> {
    const endpoint = `${getResponsesEndpoint(workspaceSlug, projectSlug, idea)}/${responseId}/reactions`
    const requestDto: ApiCreateResponseReactionRequestDto = {
        emoji,
        youthToken,
    }

    const dtos = await apiFetch<ApiResponseReactionSummaryDto[]>(endpoint, {
        method: 'POST',
        body: JSON.stringify(requestDto),
    })

    return dtos.map(mapApiReactionSummaryToReactionSummary)
}

export async function removeResponseReaction(workspaceSlug: string, projectSlug: string, idea: Idea, responseId: number, youthToken: string, emoji: string): Promise<ResponseReactionSummary[]> {
    const endpoint = `${getResponsesEndpoint(workspaceSlug, projectSlug, idea)}/${responseId}/reactions?youthToken=${encodeURIComponent(youthToken)}&emoji=${encodeURIComponent(emoji)}`
    await apiFetch<void>(endpoint, {
        method: 'DELETE',
    })

    return getResponseReactionSummary(workspaceSlug, projectSlug, idea, responseId)
}

export async function addIdeaReaction(workspaceSlug: string, projectSlug: string, idea: Idea, youthToken: string, emoji: string): Promise<ResponseReactionSummary[]> {
    const endpoint = getIdeaReactionsEndpoint(workspaceSlug, projectSlug, idea)
    const requestDto: ApiCreateResponseReactionRequestDto = {
        emoji,
        youthToken,
    }

    const dtos = await apiFetch<ApiResponseReactionSummaryDto[]>(endpoint, {
        method: 'POST',
        body: JSON.stringify(requestDto),
    })

    return dtos.map(mapApiReactionSummaryToReactionSummary)
}

export async function removeIdeaReaction(workspaceSlug: string, projectSlug: string, idea: Idea, youthToken: string, emoji: string): Promise<ResponseReactionSummary[]> {
    const endpoint = `${getIdeaReactionsEndpoint(workspaceSlug, projectSlug, idea)}?youthToken=${encodeURIComponent(youthToken)}&emoji=${encodeURIComponent(emoji)}`
    await apiFetch<void>(endpoint, {
        method: 'DELETE',
    })

    return getIdeaReactionSummary(workspaceSlug, projectSlug, idea)
}
