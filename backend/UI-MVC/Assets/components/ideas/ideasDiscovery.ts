import type { Idea } from '../../models/idea'
import type { DiscoveryFeed } from './types'
import { DiscoveryBadgeType } from './types'

let suppressListScrollSyncUntil: number = 0

export function suppressListScrollSync(durationMs: number): void {
    suppressListScrollSyncUntil = performance.now() + durationMs
}

export function isScrollSyncSuppressed(): boolean {
    return performance.now() < suppressListScrollSyncUntil
}

import { IdeaAuthorType } from '../../models/idea'

export function hasOwnIdeaInTopic(allIdeas: Idea[], topicId: number): boolean {
    return allIdeas.some((idea) => idea.authorType === IdeaAuthorType.Self && idea.topicId === topicId)
}

export function getTopicSemanticCategories(allIdeas: Idea[], topicId: number): string[] {
    const categories = new Set<string>()
    allIdeas
        .filter((idea) => idea.topicId === topicId)
        .forEach((idea) => {
            idea.semanticCategories.forEach((category) => {
                if (category.trim().length > 0) {
                    categories.add(category)
                }
            })
        })

    return [...categories].sort((a, b) => a.localeCompare(b))
}

export function buildBroadFeed(topicIdeas: Idea[]): Idea[] {
    const byCategory = new Map<string, Idea[]>()
    const withoutCategories: Idea[] = []

    topicIdeas.forEach((idea) => {
        if (idea.semanticCategories.length === 0) {
            withoutCategories.push(idea)
            return
        }
        idea.semanticCategories.forEach((category) => {
            const bucket = byCategory.get(category) ?? []
            bucket.push(idea)
            byCategory.set(category, bucket)
        })
    })

    const broad: Idea[] = []
    const seen = new Set<number>()
    const categories = [...byCategory.keys()]
    let added = true
    while (added) {
        added = false
        categories.forEach((category) => {
            const bucket = byCategory.get(category)
            if (!bucket || bucket.length === 0) return
            const nextIdea = bucket.shift()!
            if (seen.has(nextIdea.id)) return
            seen.add(nextIdea.id)
            broad.push(nextIdea)
            added = true
        })
    }
    withoutCategories.forEach((idea) => {
        if (seen.has(idea.id)) return
        seen.add(idea.id)
        broad.push(idea)
    })
    topicIdeas.forEach((idea) => {
        if (seen.has(idea.id)) return
        seen.add(idea.id)
        broad.push(idea)
    })
    return broad
}

export function createDiscoveryFeed(ideas: Idea[], badgesByIdeaId: Map<number, DiscoveryBadgeType>): DiscoveryFeed {
    return { ideas, badgesByIdeaId }
}

export function createPostPreviewFeed(
    similarIdeas: Idea[],
    differentIdeas: Idea[],
    submittedIdea: Idea | null,
    _badgesByIdeaId: Map<number, DiscoveryBadgeType>,
): DiscoveryFeed {
    const previewIdeas: Idea[] = []
    const previewBadges = new Map<number, DiscoveryBadgeType>()
    const seen = new Set<number>()

    const addIdea = (idea: Idea | null | undefined, badge?: DiscoveryBadgeType): boolean => {
        if (!idea || seen.has(idea.id)) return false
        seen.add(idea.id)
        previewIdeas.push(idea)
        if (badge && previewBadges.size < 2) {
            previewBadges.set(idea.id, badge)
        }
        return true
    }

    addIdea(similarIdeas[0], DiscoveryBadgeType.Similar)
    for (const idea of differentIdeas) {
        if (addIdea(idea, DiscoveryBadgeType.Different)) break
    }
    addIdea(submittedIdea)

    return createDiscoveryFeed(previewIdeas, previewBadges)
}
