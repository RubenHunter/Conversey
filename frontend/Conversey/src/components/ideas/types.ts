import type { Idea } from '../../models/idea.ts'

export type ActiveView = { type: 'topic'; topicId: number } | { type: 'my-ideas' }

export interface IdeaComment {
    author: 'self' | 'other'
    text: string
}

export interface IdeaPanelController {
    open: (idea: Idea) => void
    close: () => void
}

