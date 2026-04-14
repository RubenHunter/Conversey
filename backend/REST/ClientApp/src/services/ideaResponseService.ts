import type {
    ApiCreateIdeaResponseRequestDto,
    ApiCreateResponseReactionRequestDto,
    ApiIdeaResponseDto,
    ApiResponseReactionSummaryDto,
    ApiResponseSubmissionResultDto,
    ApiUpdateResponseAfterSafetyReviewRequestDto,
} from '../api/dtos/ideaResponseDto.ts'
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

export interface IdeaResponseSubmitResult {
    response: IdeaResponse
    aiSuggestion: string | null
    requiresSafetyReview: boolean
}

export async function getIdeaResponses(workspaceSlug: string, projectSlug: string, idea: Idea, youthToken: string): Promise<IdeaResponse[]> {
    const endpoint = `${getResponsesEndpoint(workspaceSlug, projectSlug, idea)}?youthToken=${encodeURIComponent(youthToken)}`
    const dtos = await apiFetch<ApiIdeaResponseDto[]>(endpoint)
    return dtos.map((dto) => mapApiIdeaResponseToIdeaResponse(dto, youthToken))
}

export async function addIdeaResponse(workspaceSlug: string, projectSlug: string, idea: Idea, youthToken: string, text: string): Promise<IdeaResponseSubmitResult> {
    const endpoint = getResponsesEndpoint(workspaceSlug, projectSlug, idea)
    const requestDto: ApiCreateIdeaResponseRequestDto = {
        text,
        youthToken,
    }

    const result = await apiFetch<ApiResponseSubmissionResultDto>(endpoint, {
        method: 'POST',
        body: JSON.stringify(requestDto),
    })

    const responseDto = result.response ?? result.Response
    if (!responseDto) {
        throw new Error('Unexpected response submission payload: missing response')
    }

    const mappedResponse = mapApiIdeaResponseToIdeaResponse(responseDto, youthToken)
    const decision = result.decision ?? result.Decision
    const suggestion = result.suggestion
        ?? result.Suggestion
        ?? decision?.suggestion
        ?? decision?.Suggestion
        ?? null
    const isAllowed = decision?.isAllowed ?? decision?.IsAllowed

    return {
        response: mappedResponse,
        aiSuggestion: suggestion && suggestion.trim().length > 0 ? suggestion.trim() : null,
        requiresSafetyReview: isAllowed === false || mappedResponse.offensiveContentDetected === true,
    }
}

export async function updateIdeaResponseAfterSafetyReview(
    workspaceSlug: string,
    projectSlug: string,
    idea: Idea,
    responseId: number,
    youthToken: string,
    text: string,
    markForReview: boolean,
): Promise<IdeaResponse> {
    const endpoint = `${getResponsesEndpoint(workspaceSlug, projectSlug, idea)}/${responseId}`
    const requestDto: ApiUpdateResponseAfterSafetyReviewRequestDto = {
        text,
        youthToken,
        markForReview,
    }

    const dto = await apiFetch<ApiIdeaResponseDto>(endpoint, {
        method: 'PUT',
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
