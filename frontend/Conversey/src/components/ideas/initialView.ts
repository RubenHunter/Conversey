import type { Idea, IdeaTopic } from '../../models/idea.ts'
import type { ActiveView } from './types.ts'

export function resolveInitialIdeasView(topics: IdeaTopic[], allIdeas: Idea[]): ActiveView {
    if (topics.length === 0) {
        return { type: 'my-ideas' }
    }

    const selfIdeaTopicIds = new Set(allIdeas.filter((idea) => idea.authorType === 'self').map((idea) => idea.topicId))
    const firstUnansweredTopic = topics.find((topic) => !selfIdeaTopicIds.has(topic.id))

    if (firstUnansweredTopic) {
        return { type: 'topic', topicId: firstUnansweredTopic.id }
    }

    return { type: 'my-ideas' }
}

