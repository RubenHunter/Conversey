import type {
    ApiCreatedReactionDto,
    ApiCreateIdeaResponseRequestDto,
    ApiCreateResponseReactionRequestDto,
    ApiIdeaResponseDto,
    ApiIdeaThreadDto,
    ApiResponseReactionSummaryDto,
    ApiResponseSubmissionResultDto,
    ApiUpdateResponseAfterSafetyReviewRequestDto,
} from '../api/dtos/ideaResponseDto'
import { mapApiIdeaResponseToIdeaResponse, mapApiReactionSummaryToReactionSummary } from '../mappers/ideaResponseMapper'
import type { Idea } from '../models/idea'
import type { IdeaResponse, ResponseReactionSummary } from '../models/ideaResponse'
import { apiFetch } from './apiService'

const responseReactionIds = new Map<string, number>()
const ideaReactionIds = new Map<string, number>()

function pickNumber(...values: Array<number | undefined>): number | undefined {
    return values.find((value) => typeof value === 'number' && Number.isFinite(value))
}

function getResponseReactionKey(workspaceSlug: string, projectSlug: string, idea: Idea, responseId: number, youthId: string, emoji: string): string {
    return `${workspaceSlug}|${projectSlug}|${idea.id}|${responseId}|${youthId}|${emoji}`
}

function getIdeaReactionKey(workspaceSlug: string, projectSlug: string, idea: Idea, youthId: string, emoji: string): string {
    return `${workspaceSlug}|${projectSlug}|${idea.id}|${youthId}|${emoji}`
}

function getReactionId(dto: ApiCreatedReactionDto): number | undefined {
    return pickNumber(dto.id, dto.Id, dto.reactionId, dto.ReactionId)
}

function getResponsesEndpoint(workspaceSlug: string, projectSlug: string, idea: Idea): string {
    return `/workspaces/${workspaceSlug}/projects/${projectSlug}/topics/${idea.topicId}/ideas/${idea.id}/responses`
}

function getIdeaThreadEndpoint(workspaceSlug: string, projectSlug: string, idea: Idea): string {
    return `/workspaces/${workspaceSlug}/projects/${projectSlug}/topics/${idea.topicId}/ideas/${idea.id}/thread`
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
    const endpoint = getIdeaThreadEndpoint(workspaceSlug, projectSlug, idea)
    const thread = await apiFetch<ApiIdeaThreadDto>(endpoint)
    const dtos = thread.responses ?? thread.Responses ?? []
    return dtos.map((dto) => mapApiIdeaResponseToIdeaResponse(dto, youthToken))
}

export async function addIdeaResponse(workspaceSlug: string, projectSlug: string, idea: Idea, youthToken: string, text: string): Promise<IdeaResponseSubmitResult> {
    const endpoint = getResponsesEndpoint(workspaceSlug, projectSlug, idea)
    const requestDto: ApiCreateIdeaResponseRequestDto = {
        text,
        youthId: youthToken,
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
        youthId: youthToken,
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
        youthId: youthToken,
    }

    const result = await apiFetch<ApiResponseReactionSummaryDto[] | ApiCreatedReactionDto>(endpoint, {
        method: 'POST',
        body: JSON.stringify(requestDto),
    })

    if (!Array.isArray(result)) {
        const reactionId = getReactionId(result)
        if (reactionId !== undefined) {
            const key = getResponseReactionKey(workspaceSlug, projectSlug, idea, responseId, youthToken, emoji)
            responseReactionIds.set(key, reactionId)
        }
    }

    return getResponseReactionSummary(workspaceSlug, projectSlug, idea, responseId)
}

export async function removeResponseReaction(workspaceSlug: string, projectSlug: string, idea: Idea, responseId: number, youthToken: string, emoji: string): Promise<ResponseReactionSummary[]> {
    const key = getResponseReactionKey(workspaceSlug, projectSlug, idea, responseId, youthToken, emoji)
    const reactionId = responseReactionIds.get(key)

    if (reactionId === undefined) {
        throw new Error('Cannot remove this response reaction yet because no reaction id is available in the current session.')
    }

    const endpoint = `${getResponsesEndpoint(workspaceSlug, projectSlug, idea)}/${responseId}/reactions/${reactionId}?youthId=${encodeURIComponent(youthToken)}`
    await apiFetch<void>(endpoint, {
        method: 'DELETE',
    })

    responseReactionIds.delete(key)

    return getResponseReactionSummary(workspaceSlug, projectSlug, idea, responseId)
}

export async function addIdeaReaction(workspaceSlug: string, projectSlug: string, idea: Idea, youthToken: string, emoji: string): Promise<ResponseReactionSummary[]> {
    const endpoint = getIdeaReactionsEndpoint(workspaceSlug, projectSlug, idea)
    const requestDto: ApiCreateResponseReactionRequestDto = {
        emoji,
        youthId: youthToken,
    }

    const result = await apiFetch<ApiResponseReactionSummaryDto[] | ApiCreatedReactionDto>(endpoint, {
        method: 'POST',
        body: JSON.stringify(requestDto),
    })

    if (!Array.isArray(result)) {
        const reactionId = getReactionId(result)
        if (reactionId !== undefined) {
            const key = getIdeaReactionKey(workspaceSlug, projectSlug, idea, youthToken, emoji)
            ideaReactionIds.set(key, reactionId)
        }
    }

    if (Array.isArray(result)) {
        return result.map(mapApiReactionSummaryToReactionSummary)
    }

    return getIdeaReactionSummary(workspaceSlug, projectSlug, idea)
}

export async function removeIdeaReaction(workspaceSlug: string, projectSlug: string, idea: Idea, youthToken: string, emoji: string): Promise<ResponseReactionSummary[]> {
    const key = getIdeaReactionKey(workspaceSlug, projectSlug, idea, youthToken, emoji)
    const reactionId = ideaReactionIds.get(key)

    if (reactionId === undefined) {
        throw new Error('Cannot remove this idea reaction yet because no reaction id is available in the current session.')
    }

    const endpoint = `${getIdeaReactionsEndpoint(workspaceSlug, projectSlug, idea)}/${reactionId}?youthId=${encodeURIComponent(youthToken)}`
    await apiFetch<void>(endpoint, {
        method: 'DELETE',
    })

    ideaReactionIds.delete(key)

    return getIdeaReactionSummary(workspaceSlug, projectSlug, idea)
}
