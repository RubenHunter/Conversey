import type { ApiIdeaDto } from '../api/dtos/ideaDto.ts'
import { mapApiIdeaToIdea, mapSubmitIdeaRequestToApiSubmitIdeaRequest } from '../mappers/ideaMapper.ts'
import type { Idea, IdeaTopic, SubmitIdeaRequest } from '../models/idea.ts'
import type { Project } from '../models/project.ts'
import { apiFetch } from './apiService.ts'

const IDEAS_USER_KEY = 'conversey-ideas-user-id'

function isGuid(value: string): boolean {
    return /^[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i.test(value)
}

function createGuidToken(): string {
    if (typeof crypto !== 'undefined' && typeof crypto.randomUUID === 'function') {
        return crypto.randomUUID()
    }

    // Fallback for older environments where randomUUID is unavailable.
    const seed = `${Date.now()}-${Math.random().toString(16).slice(2)}`
    return `00000000-0000-4000-8000-${seed.padEnd(12, '0').slice(0, 12)}`
}

interface IdeasContext {
    topics: IdeaTopic[]
    ideas: Idea[]
}

export function getIdeasYouthToken(projectId: number): string {
    const key = `${IDEAS_USER_KEY}-${projectId}`
    const existing = localStorage.getItem(key)
    if (existing && isGuid(existing)) return existing

    const userId = createGuidToken()

    localStorage.setItem(key, userId)
    return userId
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
        }))
        .sort((a, b) => (a.order ?? a.id) - (b.order ?? b.id))
}

async function getCommunityIdeasForTopic(workspaceSlug: string, projectSlug: string, topicId: number, youthToken: string): Promise<Idea[]> {
    const endpoint = `/workspaces/${workspaceSlug}/projects/${projectSlug}/topics/${topicId}/ideas`
    const dtos = await apiFetch<ApiIdeaDto[]>(endpoint)
    return dtos.map((dto) => mapApiIdeaToIdea(dto, youthToken))
}

async function getMyIdeas(workspaceSlug: string, projectSlug: string, youthToken: string): Promise<Idea[]> {
    const endpoint = `/workspaces/${workspaceSlug}/projects/${projectSlug}/ideas/by-youth/${encodeURIComponent(youthToken)}`
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
    const youthToken = getIdeasYouthToken(project.id)

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

export async function submitIdea(workspaceSlug: string, projectSlug: string, request: SubmitIdeaRequest): Promise<IdeaSubmitResult> {
    const youthToken = getIdeasYouthToken(request.projectId)
    const requestDto = mapSubmitIdeaRequestToApiSubmitIdeaRequest(request, youthToken)
    const endpoint = `/workspaces/${workspaceSlug}/projects/${projectSlug}/topics/${request.topicId}/ideas`

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
    const requiresSafetyReview = isAllowed === false || mappedIdea.pendingReview

    if (trimmedSuggestion) {
        console.log('[AI moderation] ⚠️ content flagged by AI moderation')
        console.log('[AI moderation] AI suggestion:', trimmedSuggestion)
        console.log('[AI moderation] idea saved as Pending in DB, awaiting user decision')
    } else if (isAllowed === false) {
        console.log('[AI moderation] ⚠️ content flagged by moderation and saved as Pending')
    } else {
        console.log('[AI moderation] ✅ content approved by AI moderation — idea saved as Approved')
    }

    return {
        idea: mappedIdea,
        aiSuggestion: trimmedSuggestion,
        requiresSafetyReview,
    }
}

interface UpdateIdeaAfterSafetyReviewRequest {
    projectId: number
    content: string
    youthToken: string
    markForReview: boolean
}

export async function updateIdeaAfterSafetyReview(
    workspaceSlug: string,
    projectSlug: string,
    topicId: number,
    ideaId: number,
    projectId: number,
    content: string,
    markForReview: boolean,
): Promise<Idea> {
    const youthToken = getIdeasYouthToken(projectId)
    const endpoint = `/workspaces/${workspaceSlug}/projects/${projectSlug}/topics/${topicId}/ideas/${ideaId}`

    const payload: UpdateIdeaAfterSafetyReviewRequest = {
        projectId,
        content,
        youthToken,
        markForReview,
    }

    const dto = await apiFetch<ApiIdeaDto>(endpoint, {
        method: 'PUT',
        body: JSON.stringify(payload),
    })

    console.log(
        `[AI moderation] updated idea ${ideaId} after safety dialog; status=${markForReview ? 'Pending' : 'Approved'}`,
    )

    return mapApiIdeaToIdea(dto, youthToken)
}
