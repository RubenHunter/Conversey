import { renderCommunityIdeasList } from './communityList'
import type { Idea, IdeaTopic } from '../../../models/idea'
import type { ActiveView } from '../types'
import { DiscoveryBadgeType } from '../types'

interface CreateIdeasListControllerParams {
    list: HTMLElement
    ideas: Idea[]
    activeView: ActiveView
    topics: IdeaTopic[]
    flaggedIdeaIds: Set<number>
    discoveryBadgeByIdeaId?: ReadonlyMap<number, DiscoveryBadgeType>
    onDiscoveryBadgeClick?: (badge: DiscoveryBadgeType) => void
    onActiveIdeasChanged?: (nextIdea: Idea, originalIndex: number) => void
}

interface IdeasListController {
    setActive(nextIndex: number, shouldScroll: boolean): void
    startRotation(): void
    stopRotation(): void
    updateFromScroll(): void
    cleanup(): void
    setIdeas(ideas: Idea[]): void
    getActiveIndex(): number
}

export function createIdeasListController({
    list,
    ideas: initialIdeas,
    activeView,
    topics,
    flaggedIdeaIds,
    discoveryBadgeByIdeaId,
    onDiscoveryBadgeClick,
    onActiveIdeasChanged,
}: CreateIdeasListControllerParams): IdeasListController {
    let ideas = initialIdeas
    let activeIdeaOriginalIndex = 0
    let rotationTimer: number | null = null
    let focusTurnTimeout: number | null = null
    let listScrollUnlockTimeout: number | null = null
    let isProgrammaticListScroll = false
    let listSyncFrame: number | null = null

    function centerActiveCard(behavior: ScrollBehavior): void {
        const activeCard = list.querySelector<HTMLElement>(`.ideas-card[data-original-index="${activeIdeaOriginalIndex}"]`)
        if (!activeCard) return

        isProgrammaticListScroll = true
        if (listScrollUnlockTimeout !== null) {
            window.clearTimeout(listScrollUnlockTimeout)
        }

        activeCard.scrollIntoView({ behavior, block: 'center' })

        listScrollUnlockTimeout = window.setTimeout(() => {
            isProgrammaticListScroll = false
            listScrollUnlockTimeout = null
            startRotation()
        }, behavior === 'smooth' ? 380 : 120)
    }

    function startRotation(): void {
        if (rotationTimer !== null) {
            stopRotation()
        }
        if (ideas.length <= 1) return

        rotationTimer = window.setInterval(() => {
            const nextIndex = (activeIdeaOriginalIndex + 1) % ideas.length
            setActive(nextIndex, true)
        }, 5000)
    }

    function stopRotation(): void {
        if (rotationTimer !== null) {
            window.clearInterval(rotationTimer)
            rotationTimer = null
        }
    }

    function setActive(nextIndex: number, shouldScroll: boolean): void {
        const totalIdeas = ideas.length
        if (totalIdeas === 0) {
            activeIdeaOriginalIndex = 0
            return
        }

        if (shouldScroll) {
            stopRotation()
        }
        const newActiveIndex = Math.max(0, Math.min(nextIndex, totalIdeas - 1))

        renderCommunityIdeasList({
            list: list as HTMLDivElement,
            ideas,
            activeView,
            topics,
            flaggedIdeaIds,
            activeIndex: newActiveIndex,
            discoveryBadgeByIdeaId,
            onDiscoveryBadgeClick,
        })

        activeIdeaOriginalIndex = newActiveIndex

        if (onActiveIdeasChanged) {
            onActiveIdeasChanged(ideas[newActiveIndex], newActiveIndex)
        }

        const activeCard = list.querySelector<HTMLElement>(`.ideas-card[data-original-index="${newActiveIndex}"]`)
        if (activeCard) {
            activeCard.classList.remove('turn-focus')
            if (focusTurnTimeout !== null) {
                window.clearTimeout(focusTurnTimeout)
            }
            void activeCard.offsetWidth
            activeCard.classList.add('turn-focus')
            focusTurnTimeout = window.setTimeout(() => {
                activeCard.classList.remove('turn-focus')
                focusTurnTimeout = null
            }, 320)
        }

        if (!shouldScroll) return
        centerActiveCard('smooth')
    }

    function updateFromScroll(): void {
        if (ideas.length === 0 || isProgrammaticListScroll) return

        // Boundary snap: at extreme top → first card; at extreme bottom → last card
        if (list.scrollTop <= 6) {
            if (activeIdeaOriginalIndex !== 0) setActive(0, false)
            return
        }
        const distToBottom = list.scrollHeight - list.clientHeight - list.scrollTop
        if (distToBottom <= 6) {
            const lastIndex = ideas.length - 1
            if (activeIdeaOriginalIndex !== lastIndex) setActive(lastIndex, false)
            return
        }

        if (listSyncFrame !== null) {
            window.cancelAnimationFrame(listSyncFrame)
        }

        listSyncFrame = window.requestAnimationFrame(() => {
            const cards = list.querySelectorAll<HTMLElement>('.ideas-card')
            if (cards.length === 0) return

            const listRect = list.getBoundingClientRect()
            const centerY = listRect.top + listRect.height / 2
            let closestOriginalIndex = activeIdeaOriginalIndex
            let closestDistance = Number.POSITIVE_INFINITY

            cards.forEach((card) => {
                const rect = card.getBoundingClientRect()
                const cardCenterY = rect.top + rect.height / 2
                const distance = Math.abs(cardCenterY - centerY)
                const originalIndex = Number(card.getAttribute('data-original-index'))

                if (distance < closestDistance) {
                    closestDistance = distance
                    closestOriginalIndex = originalIndex
                }
            })

            if (closestOriginalIndex !== activeIdeaOriginalIndex) {
                setActive(closestOriginalIndex, false)
            }

            listSyncFrame = null
        })
    }

    function cleanup(): void {
        stopRotation()
        if (focusTurnTimeout !== null) {
            window.clearTimeout(focusTurnTimeout)
        }
        if (listScrollUnlockTimeout !== null) {
            window.clearTimeout(listScrollUnlockTimeout)
        }
        if (listSyncFrame !== null) {
            window.cancelAnimationFrame(listSyncFrame)
        }
    }

    return {
        setActive,
        startRotation,
        stopRotation,
        updateFromScroll,
        cleanup,
        setIdeas: (nextIdeas: Idea[]) => {
            ideas = nextIdeas
            if (activeIdeaOriginalIndex >= ideas.length) {
                activeIdeaOriginalIndex = 0
            }
            if (ideas.length > 0) {
                setActive(activeIdeaOriginalIndex, false)
                centerActiveCard('auto')
            }
        },
        getActiveIndex: () => activeIdeaOriginalIndex,
    }
}


