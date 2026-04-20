import { renderCommunityIdeasList } from './communityList.ts'
import type { Idea, IdeaTopic } from '../../models/idea.ts'
import type { ActiveView } from './types.ts'

interface CreateIdeasListControllerParams {
    list: HTMLElement
    ideas: Idea[]
    activeView: ActiveView
    topics: IdeaTopic[]
    flaggedIdeaIds: Set<number>
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
    onActiveIdeasChanged,
}: CreateIdeasListControllerParams): IdeasListController {
    let ideas = initialIdeas
    let activeIdeaOriginalIndex = 0
    let rotationTimer: number | null = null
    let focusTurnTimeout: number | null = null
    let listScrollUnlockTimeout: number | null = null
    let isProgrammaticListScroll = false
    let listSyncFrame: number | null = null

    function applyWheelStyles(): void {
        const cards = list.querySelectorAll<HTMLElement>('.ideas-card')
        cards.forEach((card, index) => {
            card.classList.remove('active', 'near', 'far')
            if (index === 0) {
                card.classList.add('active')
            } else if (index === 1) {
                card.classList.add('near')
            } else if (index >= 2) {
                card.classList.add('far')
            }
        })
    }

    function calculateVisibleCardCount(): number {
        const card = list.querySelector<HTMLElement>('.ideas-card')
        if (!card) return 3

        const containerHeight = list.getBoundingClientRect().height
        const cardHeight = card.getBoundingClientRect().height
        const gapHeight = 7 // pixels - from gap: 0.45rem

        const maxCardsByHeight = Math.max(1, Math.floor(containerHeight / (cardHeight + gapHeight)))
        return Math.min(5, maxCardsByHeight)
    }

    function updateVisibleCards(): void {
        const cards = list.querySelectorAll<HTMLElement>('.ideas-card')
        const visibleCount = calculateVisibleCardCount()

        cards.forEach((card, index) => {
            card.style.display = index < visibleCount ? '' : 'none'
        })
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

        stopRotation()
        const newActiveIndex = Math.max(0, Math.min(nextIndex, totalIdeas - 1))

        renderCommunityIdeasList({
            list: list as HTMLDivElement,
            ideas,
            activeView,
            topics,
            flaggedIdeaIds,
            activeIndex: newActiveIndex,
        })

        activeIdeaOriginalIndex = newActiveIndex
        applyWheelStyles()
        updateVisibleCards()

        if (onActiveIdeasChanged) {
            onActiveIdeasChanged(ideas[newActiveIndex], newActiveIndex)
        }

        const activeCard = list.querySelector<HTMLElement>('.ideas-card:first-child')
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

        isProgrammaticListScroll = true
        if (listScrollUnlockTimeout !== null) {
            window.clearTimeout(listScrollUnlockTimeout)
        }

        activeCard?.scrollIntoView({ behavior: 'smooth', block: 'center' })

        listScrollUnlockTimeout = window.setTimeout(() => {
            isProgrammaticListScroll = false
            listScrollUnlockTimeout = null
            startRotation()
        }, 360)
    }

    function updateFromScroll(): void {
        if (ideas.length === 0 || isProgrammaticListScroll) return

        if (listSyncFrame !== null) {
            window.cancelAnimationFrame(listSyncFrame)
        }

        listSyncFrame = window.requestAnimationFrame(() => {
            const cards = list.querySelectorAll<HTMLElement>('.ideas-card')
            if (cards.length === 0) return

            const centerY = window.innerHeight / 2
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
        },
        getActiveIndex: () => activeIdeaOriginalIndex,
    }
}


