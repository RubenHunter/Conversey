import { Idea, IdeaAuthorType } from '../../../models/idea'
import { DiscoveryBadgeType, DiscoveryFeed } from '../types'
import { DiscoveryMode } from '../types'
import { getDiscoveredIdeasForTopic, IDEA_DISCOVERY_MAX_RESULTS } from '../../../services/ideaService'
import { buildBroadFeed, createDiscoveryFeed, createPostPreviewFeed } from './ideasDiscovery'

export interface DiscoveryOptions {
    allIdeas: Idea[]
    topicId: number
    discoveryMode: DiscoveryMode
    showPostPreviewPair: boolean
    youthToken: string
    organizationSlug: string
    projectSlug: string
    selectedSemanticCategory: string | null
    latestSubmittedIdea: Idea | null
    discoveryCache: Map<string, DiscoveryFeed>
}

export function hasOwnIdeaInTopic(allIdeas: Idea[], topicId: number): boolean {
    return allIdeas.some((idea) => idea.authorType === IdeaAuthorType.Self && idea.topicId === topicId)
}

export function getCacheSuffix(_ownIdeaExists: boolean, _selectedSemanticCategory: string | null, discoveryMode: DiscoveryMode, showPostPreviewPair: boolean): string {
    return discoveryMode === DiscoveryMode.All && showPostPreviewPair ? 'preview' : 'full'
}

export function buildCacheKey(topicId: number, discoveryMode: DiscoveryMode, categorySuffix: string, cacheSuffix: string): string {
    return `${topicId}:${discoveryMode}:${categorySuffix}:${cacheSuffix}`
}

export async function fetchAndDeduplicateSimilarDifferent(
    organizationSlug: string,
    projectSlug: string,
    topicId: number,
    youthToken: string,
    primaryMode: DiscoveryMode,
): Promise<{ primaryIdeas: Idea[]; oppositeIdeas: Idea[] }> {
    const oppositeMode = primaryMode === DiscoveryMode.Similar ? DiscoveryMode.Different : DiscoveryMode.Similar
    const [primaryIdeas, oppositeIdeas] = await Promise.all([
        getDiscoveredIdeasForTopic(organizationSlug, projectSlug, topicId, youthToken, primaryMode, IDEA_DISCOVERY_MAX_RESULTS),
        getDiscoveredIdeasForTopic(organizationSlug, projectSlug, topicId, youthToken, oppositeMode, IDEA_DISCOVERY_MAX_RESULTS),
    ])
    return { primaryIdeas, oppositeIdeas }
}

export function deduplicateIdeas(primaryList: Idea[], oppositeList: Idea[], primaryMode: DiscoveryMode): Idea[] {
    const simIds = new Set(primaryList.map((idea) => idea.id))
    const deduplicated = oppositeList.filter((idea) => !simIds.has(idea.id))
    return primaryMode === DiscoveryMode.Similar ? primaryList : deduplicated
}

export function buildPinnedIdeas(
    similarIdeas: Idea[],
    oppositeIdeas: Idea[],
    allIdeas: Idea[],
    topicId: number,
    latestSubmittedIdea: Idea | null,
): { pinnedIdeas: Idea[]; pinnedBadges: Map<number, DiscoveryBadgeType>; pinnedIds: Set<number> } {
    const pinnedIdeas: Idea[] = []
    const pinnedBadges = new Map<number, DiscoveryBadgeType>()
    const pinnedIds = new Set<number>()

    const addPinned = (idea: Idea | null | undefined, badge?: DiscoveryBadgeType): void => {
        if (!idea || pinnedIds.has(idea.id)) return
        pinnedIds.add(idea.id)
        pinnedIdeas.push(idea)
        if (badge) pinnedBadges.set(idea.id, badge)
    }

    addPinned(similarIdeas[0], DiscoveryBadgeType.Similar)
    for (const idea of oppositeIdeas) {
        if (!pinnedIds.has(idea.id)) {
            addPinned(idea, DiscoveryBadgeType.Different)
            break
        }
    }
    const userIdea = latestSubmittedIdea ?? allIdeas.find((idea) => idea.authorType === IdeaAuthorType.Self && idea.topicId === topicId) ?? null
    addPinned(userIdea)

    return { pinnedIdeas, pinnedBadges, pinnedIds }
}

export async function getVisibleIdeas(options: DiscoveryOptions): Promise<DiscoveryFeed> {
    const { allIdeas, topicId, discoveryMode, showPostPreviewPair, youthToken, organizationSlug, projectSlug, selectedSemanticCategory, latestSubmittedIdea, discoveryCache } = options

    if (discoveryMode === DiscoveryMode.All && !hasOwnIdeaInTopic(allIdeas, topicId)) {
        const topicIdeas = allIdeas.filter((idea) => idea.topicId === topicId)
        const cacheSuffix = getCacheSuffix(false, selectedSemanticCategory, discoveryMode, showPostPreviewPair)
        const cacheKey = buildCacheKey(topicId, discoveryMode, selectedSemanticCategory ?? 'broad', cacheSuffix)
        const cached = discoveryCache.get(cacheKey)
        if (cached) return cached

        let discovered: DiscoveryFeed
        if (!selectedSemanticCategory) {
            discovered = createDiscoveryFeed(buildBroadFeed(topicIdeas), new Map())
        } else {
            const categoryFilter = selectedSemanticCategory.toLowerCase()
            const filtered = topicIdeas.filter((idea) =>
                idea.semanticCategories.some((category) => category.toLowerCase() === categoryFilter),
            )
            discovered = createDiscoveryFeed(filtered, new Map())
        }
        discoveryCache.set(cacheKey, discovered)
        return discovered
    }

    const ownIdeaExists = hasOwnIdeaInTopic(allIdeas, topicId)
    const categorySuffix = ownIdeaExists ? 'own' : (selectedSemanticCategory ?? 'broad')
    const cacheSuffix = getCacheSuffix(ownIdeaExists, selectedSemanticCategory, discoveryMode, showPostPreviewPair)
    const cacheKey = buildCacheKey(topicId, discoveryMode, categorySuffix, cacheSuffix)
    const cached = discoveryCache.get(cacheKey)
    if (cached) return cached

    try {
        if (discoveryMode === DiscoveryMode.All && ownIdeaExists) {
            const { primaryIdeas: similarIdeas, oppositeIdeas: rawDifferentIdeas } = await fetchAndDeduplicateSimilarDifferent(
                organizationSlug, projectSlug, topicId, youthToken, DiscoveryMode.Similar
            )

            const similarIds = new Set(similarIdeas.map((idea) => idea.id))
            const oppositeIdeas = rawDifferentIdeas.filter((idea) => !similarIds.has(idea.id))

            // Pre-populate individual mode caches
            const similarCacheKey = buildCacheKey(topicId, DiscoveryMode.Similar, categorySuffix, 'full')
            const differentCacheKey = buildCacheKey(topicId, DiscoveryMode.Different, categorySuffix, 'full')
            if (!discoveryCache.has(similarCacheKey)) {
                discoveryCache.set(similarCacheKey, createDiscoveryFeed(similarIdeas, new Map()))
            }
            if (!discoveryCache.has(differentCacheKey)) {
                discoveryCache.set(differentCacheKey, createDiscoveryFeed(oppositeIdeas, new Map()))
            }

            if (showPostPreviewPair) {
                const submittedIdea = latestSubmittedIdea ?? allIdeas.find((idea) => idea.authorType === IdeaAuthorType.Self && idea.topicId === topicId) ?? null
                const discovered = createPostPreviewFeed(similarIdeas, rawDifferentIdeas, submittedIdea, new Map())
                discoveryCache.set(cacheKey, discovered)
                return discovered
            }

            const { pinnedIdeas, pinnedBadges, pinnedIds } = buildPinnedIdeas(similarIdeas, oppositeIdeas, allIdeas, topicId, latestSubmittedIdea)
            const topicIdeas = allIdeas.filter((idea) => idea.topicId === topicId)
            const broadRemainder = buildBroadFeed(topicIdeas).filter((idea) => !pinnedIds.has(idea.id))
            const discovered = createDiscoveryFeed([...pinnedIdeas, ...broadRemainder], pinnedBadges)
            discoveryCache.set(cacheKey, discovered)
            return discovered
        }

        // Single mode (Similar or Different)
        const otherMode = discoveryMode === DiscoveryMode.Similar ? DiscoveryMode.Different : DiscoveryMode.Similar
        const [modeIdeas, otherIdeas] = await Promise.all([
            getDiscoveredIdeasForTopic(organizationSlug, projectSlug, topicId, youthToken, discoveryMode, IDEA_DISCOVERY_MAX_RESULTS),
            getDiscoveredIdeasForTopic(organizationSlug, projectSlug, topicId, youthToken, otherMode, IDEA_DISCOVERY_MAX_RESULTS),
        ])

        const similarList = discoveryMode === DiscoveryMode.Similar ? modeIdeas : otherIdeas
        const rawDifferentList = discoveryMode === DiscoveryMode.Different ? modeIdeas : otherIdeas
        const simIds = new Set(similarList.map((idea) => idea.id))
        const deduplicatedDifferent = rawDifferentList.filter((idea) => !simIds.has(idea.id))

        const otherCacheKey = buildCacheKey(topicId, otherMode, categorySuffix, 'full')
        if (!discoveryCache.has(otherCacheKey)) {
            discoveryCache.set(
                otherCacheKey,
                createDiscoveryFeed(discoveryMode === DiscoveryMode.Similar ? deduplicatedDifferent : similarList, new Map()),
            )
        }

        const discovered = createDiscoveryFeed(discoveryMode === DiscoveryMode.Similar ? similarList : deduplicatedDifferent, new Map())
        discoveryCache.set(cacheKey, discovered)
        return discovered
    } catch (error) {
        console.warn('Could not load idea discovery suggestions, falling back to all ideas.', error)
        const topicIdeas = allIdeas.filter((idea) => idea.topicId === topicId)
        return createDiscoveryFeed(topicIdeas.slice(0, IDEA_DISCOVERY_MAX_RESULTS), new Map())
    }
}
