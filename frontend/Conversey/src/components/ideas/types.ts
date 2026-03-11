import type { Idea } from '../../models/idea.ts'
import type { PostSafetyDecision } from './safetyReviewDialog.ts'

export type ActiveView = { type: 'topic'; topicId: number } | { type: 'my-ideas' }

export interface IdeaComment {
    author: 'self' | 'other'
    text: string
    offensiveContentDetected?: boolean
}

export type ReviewBeforePost = (input: string) => Promise<PostSafetyDecision>

export interface IdeaPanelController {
    open: (idea: Idea) => void
    close: () => void
}
