import type { ApiIdeaDto } from '../api/dtos/ideaDto.ts'
import { mapApiIdeaToIdea, mapSubmitIdeaRequestToApiSubmitIdeaRequest } from '../mappers/ideaMapper.ts'
import type { Idea, IdeaTopic, SubmitIdeaRequest } from '../models/idea.ts'
import type { Project } from '../models/project.ts'
import { apiFetch } from './apiService.ts'
import { getOrCreateProjectYouthId, normalizeSlugForClient } from './youthIdService.ts'

interface IdeasContext {
    topics: IdeaTopic[]
    ideas: Idea[]
}

export type IdeaDiscoveryCategory = 'similar' | 'different' | 'random'
export const IDEA_DISCOVERY_MAX_RESULTS = 30

export function getOrCreateProjectScopedYouthId(projectSlug: string): string {
    return getOrCreateProjectYouthId(projectSlug)
}
function mapProjectTopicsToIdeaTopics(project: Project): IdeaTopic[] {
    const sourceTopics = project.topics && project.topics.length > 0
        ? project.topics
        : project.topic
            ? [project.topic]
            : []

    return sourceTopics
        .map((topic, index) => ({
            id: topic.id,
            projectId: project.id,
            title: topic.name,
            prompt: topic.context,
            order: index + 1,
            maxBroadSelectionLoads: topic.maxBroadSelectionLoads ?? 3,
        }))
        .sort((a, b) => (a.order ?? a.id) - (b.order ?? b.id))
}

async function getCommunityIdeasForTopic(workspaceSlug: string, projectSlug: string, topicId: number, youthToken: string): Promise<Idea[]> {
    const endpoint = `/workspaces/${workspaceSlug}/projects/${normalizeSlugForClient(projectSlug)}/topics/${topicId}/ideas`
    const dtos = await apiFetch<ApiIdeaDto[]>(endpoint)
    return dtos.map((dto) => mapApiIdeaToIdea(dto, youthToken))
}

async function getMyIdeas(workspaceSlug: string, projectSlug: string, youthToken: string): Promise<Idea[]> {
    const endpoint = `/workspaces/${workspaceSlug}/projects/${normalizeSlugForClient(projectSlug)}/youth/${encodeURIComponent(youthToken)}/ideas`
    const dtos = await apiFetch<ApiIdeaDto[]>(endpoint)
    return dtos.map((dto) => mapApiIdeaToIdea(dto, youthToken))
}

function mergeIdeas(communityIdeas: Idea[], myIdeas: Idea[]): Idea[] {
    const uniqueById = new Map<number, Idea>()
    for (const idea of [...communityIdeas, ...myIdeas]) {
        uniqueById.set(idea.id, idea)
    }

    return [...uniqueById.values()].sort((a, b) => Date.parse(b.createdAt) - Date.parse(a.createdAt))
}

export async function getIdeasContext(workspaceSlug: string, projectSlug: string, project: Project): Promise<IdeasContext> {
    const topics = mapProjectTopicsToIdeaTopics(project)
    const youthToken = getOrCreateProjectScopedYouthId(projectSlug)

    const communityPerTopic = await Promise.all(
        topics.map((topic) => getCommunityIdeasForTopic(workspaceSlug, projectSlug, topic.id, youthToken)),
    )

    const communityIdeas = communityPerTopic.flat()
    const myIdeas = await getMyIdeas(workspaceSlug, projectSlug, youthToken)

    return {
        topics,
        ideas: mergeIdeas(communityIdeas, myIdeas),
    }
}

export interface IdeaSubmitResult {
    idea: Idea
    /** Present when backend flagged the content and generated an AI alternative */
    aiSuggestion: string | null
    /** True when moderation marked content as pending review */
    requiresSafetyReview: boolean
}

export interface IdeaNudgingTurn {
    question: string
    answer: string
}

export interface IdeaNudgingContext {
    projectTitle: string
    projectDescription: string
    topicTitle: string
    topicPrompt: string
}

export interface IdeaNudgingDecision {
    isApproved: boolean
    question: string | null
}

export async function submitIdea(workspaceSlug: string, projectSlug: string, request: SubmitIdeaRequest): Promise<IdeaSubmitResult> {
    const normalizedProjectSlug = normalizeSlugForClient(projectSlug)
    const youthToken = getOrCreateProjectScopedYouthId(normalizedProjectSlug)
    const requestDto = mapSubmitIdeaRequestToApiSubmitIdeaRequest(request, youthToken)
    const endpoint = `/workspaces/${workspaceSlug}/projects/${normalizedProjectSlug}/topics/${request.topicId}/ideas`

    console.log('[AI moderation] sending idea to backend for moderation...', { content: request.body })

    const result = await apiFetch<{
        idea?: ApiIdeaDto
        Idea?: ApiIdeaDto
        suggestion?: string
        Suggestion?: string
        decision?: {
            isAllowed?: boolean
            IsAllowed?: boolean
            suggestion?: string
            Suggestion?: string
        }
        Decision?: {
            isAllowed?: boolean
            IsAllowed?: boolean
            suggestion?: string
            Suggestion?: string
        }
    }>(endpoint, {
        method: 'POST',
        body: JSON.stringify(requestDto),
    })

    const ideaDto = result.idea ?? result.Idea
    if (!ideaDto) {
        throw new Error('Unexpected idea submission response: missing idea payload')
    }

    const decision = result.decision ?? result.Decision
    const aiSuggestion = result.suggestion
        ?? result.Suggestion
        ?? decision?.suggestion
        ?? decision?.Suggestion
        ?? null
    const trimmedSuggestion = aiSuggestion && aiSuggestion.trim().length > 0 ? aiSuggestion.trim() : null
    const isAllowed = decision?.isAllowed ?? decision?.IsAllowed
    const mappedIdea = mapApiIdeaToIdea(ideaDto, youthToken)
    const requiresSafetyReview = isAllowed === false || Boolean(trimmedSuggestion)

    if (trimmedSuggestion) {
        console.log('[AI moderation] ⚠️ content flagged by AI moderation')
        console.log('[AI moderation] AI suggestion:', trimmedSuggestion)
        console.log('[AI moderation] idea saved as Pending in DB, awaiting user decision')
    } else if (isAllowed === false) {
        console.log('[AI moderation] ⚠️ content flagged by moderation and saved as Pending')
    } else if (mappedIdea.pendingReview && mappedIdea.qualityNudgeBypassed) {
        console.log('[AI nudging] idea posted without completing nudging — saved as Pending for moderation')
    } else {
        console.log('[AI moderation] ✅ content approved by AI moderation — idea saved as Approved')
    }

    return {
        idea: mappedIdea,
        aiSuggestion: trimmedSuggestion,
        requiresSafetyReview,
    }
}

export async function assessIdeaNudging(
    workspaceSlug: string,
    projectSlug: string,
    topicId: number,
    ideaText: string,
    context: IdeaNudgingContext,
    conversation: IdeaNudgingTurn[] = [],
): Promise<IdeaNudgingDecision> {
    const normalizedProjectSlug = normalizeSlugForClient(projectSlug)
    const endpoint = `/workspaces/${workspaceSlug}/projects/${normalizedProjectSlug}/topics/${topicId}/ideas/nudge`

    const result = await apiFetch<{ isApproved?: boolean; IsApproved?: boolean; question?: string; Question?: string }>(endpoint, {
        method: 'POST',
        body: JSON.stringify({
            ideaText,
            projectTitle: context.projectTitle,
            projectDescription: context.projectDescription,
            topicTitle: context.topicTitle,
            topicPrompt: context.topicPrompt,
            conversation,
        }),
    })

    const isApproved = result.isApproved ?? result.IsApproved ?? true
    const question = result.question ?? result.Question ?? null

    return {
        isApproved,
        question: question && question.trim().length > 0 ? question.trim() : null,
    }
}

interface UpdateIdeaAfterSafetyReviewRequest {
    content: string
    markForReview: boolean
}

export async function updateIdeaAfterSafetyReview(
    workspaceSlug: string,
    projectSlug: string,
    topicId: number,
    ideaId: number,
    content: string,
    markForReview: boolean,
): Promise<Idea> {
    const normalizedProjectSlug = normalizeSlugForClient(projectSlug)
    const endpoint = `/workspaces/${workspaceSlug}/projects/${normalizedProjectSlug}/topics/${topicId}/ideas/${ideaId}`

    const payload: UpdateIdeaAfterSafetyReviewRequest = {
        content,
        markForReview,
    }

    const dto = await apiFetch<ApiIdeaDto>(endpoint, {
        method: 'PUT',
        body: JSON.stringify(payload),
    })

    console.log(
        `[AI moderation] updated idea ${ideaId} after safety dialog; status=${markForReview ? 'Pending' : 'Approved'}`,
    )

    return mapApiIdeaToIdea(dto, getOrCreateProjectScopedYouthId(normalizedProjectSlug))
}

export async function getDiscoveredIdeasForTopic(
    workspaceSlug: string,
    projectSlug: string,
    topicId: number,
    youthToken: string,
    category: IdeaDiscoveryCategory,
    limit = IDEA_DISCOVERY_MAX_RESULTS,
): Promise<Idea[]> {
    const normalizedProjectSlug = normalizeSlugForClient(projectSlug)
    const endpoint = `/workspaces/${workspaceSlug}/projects/${normalizedProjectSlug}/topics/${topicId}/ideas/discover?youthId=${encodeURIComponent(youthToken)}&category=${encodeURIComponent(category)}&limit=${limit}`
    const dtos = await apiFetch<ApiIdeaDto[]>(endpoint)
    return dtos.map((dto) => mapApiIdeaToIdea(dto, youthToken))
}

export async function saveYouthContactEmail(
    workspaceSlug: string,
    projectSlug: string,
    youthToken: string,
    email: string,
): Promise<void> {
    const normalizedProjectSlug = normalizeSlugForClient(projectSlug)
    await apiFetch<void>(`/workspaces/${workspaceSlug}/projects/${normalizedProjectSlug}/youth/${encodeURIComponent(youthToken)}`, {
        method: 'PUT',
        body: JSON.stringify({ email }),
    })
}
