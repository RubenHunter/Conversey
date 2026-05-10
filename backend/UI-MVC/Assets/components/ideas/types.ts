import type { Idea, IdeaAuthorType } from '../../models/idea'
import type { PostSafetyDecision } from './components/safetyReviewDialog'

export interface DiscoveryFeed {
    ideas: Idea[]
    badgesByIdeaId: ReadonlyMap<number, DiscoveryBadgeType>
}

export enum DiscoveryMode {
    All = 'all',
    Similar = 'similar',
    Different = 'different',
    Random = 'random',
}

export enum DiscoveryBadgeType {
    Similar = 'similar',
    Different = 'different',
}

export type ActiveView = { type: 'topic'; topicId: number } | { type: 'my-ideas' }

export interface IdeaComment {
    author: IdeaAuthorType
    text: string
    offensiveContentDetected?: boolean
}

export type ReviewBeforePost = (input: string) => Promise<PostSafetyDecision>

export interface IdeaPanelController {
    open: (idea: Idea) => void
    close: () => void
    isOpen: () => boolean
    onDiscoveryBadgeClick?: (badge: DiscoveryBadgeType) => void
    runNudgingFlow?: (input: string, view: ActiveView) => void
}
