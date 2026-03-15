import type { ApiIdeaDto } from '../api/dtos/ideaDto.ts'
import { mapApiIdeaToIdea, mapSubmitIdeaRequestToApiSubmitIdeaRequest } from '../mappers/ideaMapper.ts'
import type { Idea, IdeaTopic, SubmitIdeaRequest } from '../models/idea.ts'
import type { Project } from '../models/project.ts'
import { apiFetch } from './apiService.ts'

const IDEAS_USER_KEY = 'conversey-ideas-user-id'

interface IdeasContext {
    topics: IdeaTopic[]
    ideas: Idea[]
}

export function getIdeasYouthToken(projectId: number): string {
    const key = `${IDEAS_USER_KEY}-${projectId}`
    const existing = localStorage.getItem(key)
    if (existing) return existing

    const userId =
        typeof crypto !== 'undefined' && typeof crypto.randomUUID === 'function'
            ? `anon-p${projectId}-${crypto.randomUUID()}`
            : `anon-p${projectId}-${Date.now()}-${Math.random().toString(36).slice(2, 10)}`

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

export async function submitIdea(workspaceSlug: string, projectSlug: string, request: SubmitIdeaRequest): Promise<Idea> {
    const youthToken = getIdeasYouthToken(request.projectId)
    const requestDto = mapSubmitIdeaRequestToApiSubmitIdeaRequest(request, youthToken)
    const endpoint = `/workspaces/${workspaceSlug}/projects/${projectSlug}/topics/${request.topicId}/ideas`

    const result = await apiFetch<{ idea?: ApiIdeaDto; Idea?: ApiIdeaDto; suggestion?: string; Suggestion?: string }>(endpoint, {
        method: 'POST',
        body: JSON.stringify(requestDto),
    })

    const ideaDto = result.idea ?? result.Idea
    if (!ideaDto) {
        throw new Error('Unexpected idea submission response: missing idea payload')
    }

    return mapApiIdeaToIdea(ideaDto, youthToken)
}
