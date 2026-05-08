import type { Idea, IdeaTopic } from '../../models/idea'
import type { Project } from '../../models/project'
import { getIdeasContext, getOrCreateProjectScopedYouthId } from '../../services/ideaService'

export interface IdeasInitResult {
    allIdeas: Idea[]
    topics: IdeaTopic[]
    youthToken: string
    firstTopic: IdeaTopic | undefined
}

export async function initIdeasContext(
    organizationSlug: string,
    projectSlug: string,
    project: Project,
): Promise<IdeasInitResult> {
    const ideasContext = await getIdeasContext(organizationSlug, projectSlug, project)
    const youthToken = getOrCreateProjectScopedYouthId(project.slug)
    const allIdeas: Idea[] = [...ideasContext.ideas]
    const topics: IdeaTopic[] = ideasContext.topics
    const firstTopic = topics[0]
    return { allIdeas, topics, youthToken, firstTopic }
}
