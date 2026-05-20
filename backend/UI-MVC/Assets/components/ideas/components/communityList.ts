import type { Idea, IdeaTopic } from '../../../models/idea'
import type { ActiveView } from '../types'
import { DiscoveryBadgeType } from '../types'
import { getSurveyStrings } from '../../../i18n/survey'

interface RenderCommunityListParams {
    list: HTMLDivElement
    ideas: Idea[]
    activeView: ActiveView
    topics: IdeaTopic[]
    flaggedIdeaIds: ReadonlySet<number>
    activeIndex: number
    discoveryBadgeByIdeaId?: ReadonlyMap<number, DiscoveryBadgeType>
    onDiscoveryBadgeClick?: (badge: DiscoveryBadgeType) => void
}

function formatDate(isoString: string): string {
    const t = getSurveyStrings()
    const date = new Date(isoString)
    const now = new Date()
    const diffInMinutes = Math.floor((now.getTime() - date.getTime()) / (1000 * 60))
    
    if (diffInMinutes < 1) return t.justNow
    if (diffInMinutes < 60) return t.minutesAgo.replace('{n}', String(diffInMinutes))
    
    const diffInHours = Math.floor(diffInMinutes / 60)
    if (diffInHours < 24) return t.hoursAgo.replace('{n}', String(diffInHours))
    
    const diffInDays = Math.floor(diffInHours / 24)
    if (diffInDays < 7) return t.daysAgo.replace('{n}', String(diffInDays))
    
    return date.toLocaleDateString('en-US', { month: 'short', day: 'numeric' })
}

function createUserInfoElement(_authorType: Idea['authorType'], createdAt: string): HTMLElement {
    const userInfo = document.createElement('div')
    userInfo.className = 'ideas-card-user'
    
    const icon = document.createElementNS('http://www.w3.org/2000/svg', 'svg')
    icon.setAttribute('class', 'ideas-card-user-icon')
    icon.setAttribute('viewBox', '0 0 24 24')
    icon.setAttribute('fill', 'currentColor')
    icon.setAttribute('aria-hidden', 'true')
    
    const path = document.createElementNS('http://www.w3.org/2000/svg', 'path')
    path.setAttribute('d', 'M12 12c2.21 0 4-1.79 4-4s-1.79-4-4-4-4 1.79-4 4 1.79 4 4 4zm0 2c-2.67 0-8 1.34-8 4v2h16v-2c0-2.66-5.33-4-8-4z')
    icon.appendChild(path)
    
    const date = document.createElement('span')
    date.className = 'ideas-card-user-date'
    date.textContent = formatDate(createdAt)
    
    userInfo.append(icon, date)
    return userInfo
}

function getDiscoveryBadgeLabel(badge: DiscoveryBadgeType): string {
    const t = getSurveyStrings()
    return badge === 'similar' ? t.mostSimilar : t.leastSimilar
}

function createDiscoveryBadgeElement(
    ideaId: number,
    badge: DiscoveryBadgeType,
    onDiscoveryBadgeClick?: (badge: DiscoveryBadgeType) => void,
): HTMLButtonElement {
    const t = getSurveyStrings()
    const button = document.createElement('button')
    button.type = 'button'
    button.className = `ideas-discovery-badge ideas-discovery-badge--${badge}`
    button.textContent = getDiscoveryBadgeLabel(badge)
    button.title = `Show ${badge === 'similar' ? t.mostSimilarIdeas.toLowerCase() : t.leastSimilarIdeas.toLowerCase()} ideas`
    button.setAttribute('data-discovery-badge', badge)
    button.setAttribute('data-idea-id', String(ideaId))
    button.addEventListener('click', (event) => {
        event.stopPropagation()
        onDiscoveryBadgeClick?.(badge)
    })
    return button
}

export function renderCommunityIdeasList({
    list,
    ideas,
    activeView,
    topics,
    flaggedIdeaIds,
    activeIndex,
    discoveryBadgeByIdeaId,
    onDiscoveryBadgeClick,
}: RenderCommunityListParams): void {
    const t = getSurveyStrings()
    list.innerHTML = ''

    if (ideas.length === 0) {
        list.innerHTML = `<p class="ideas-empty">${activeView.type === 'my-ideas' ? t.noIdeasMyIdeas : t.noIdeasForView}</p>`
        return
    }

    ideas.forEach((idea, index) => {
        const card = document.createElement('article')
        card.className = 'ideas-card'
        if (index === activeIndex) {
            card.classList.add('active')
        } else if (Math.abs(index - activeIndex) === 1) {
            card.classList.add('near')
        } else {
            card.classList.add('far')
        }
        card.setAttribute('data-idea-id', String(idea.id))
        card.setAttribute('aria-label', `Idea ${index + 1}: ${idea.body.substring(0, 50)}${idea.body.length > 50 ? '...' : ''}`)

        // Add user info (icon + date) for all cards
        const userInfo = createUserInfoElement(idea.authorType, idea.createdAt)
        card.appendChild(userInfo)

        if (activeView.type === 'my-ideas') {
            card.classList.add('ideas-card--my-idea')

            const topicLabel = document.createElement('p')
            topicLabel.className = 'ideas-card-topic'
            topicLabel.textContent = topics.find((topic) => topic.id === idea.topicId)?.title ?? 'Unknown topic'

            const badgesWrapper = document.createElement('div')
            badgesWrapper.className = 'ideas-card-badges'
            badgesWrapper.style.display = 'flex'
            badgesWrapper.style.gap = '0.25rem'
            badgesWrapper.style.alignItems = 'center'
            
            if (idea.pendingReview || idea.qualityNudgeBypassed || flaggedIdeaIds.has(idea.id)) {
                const flagged = document.createElement('span')
                flagged.className = 'ideas-review-flag'
                flagged.textContent = 'Marked for review'
                badgesWrapper.appendChild(flagged)
            }
            
            if (badgesWrapper.children.length > 0) {
                card.appendChild(badgesWrapper)
            }

            const body = document.createElement('p')
            body.className = 'ideas-card-body'
            body.textContent = idea.body

            card.append(topicLabel, body)
        } else {
            const badgesWrapper = document.createElement('div')
            badgesWrapper.className = 'ideas-card-badges'
            badgesWrapper.style.display = 'flex'
            badgesWrapper.style.gap = '0.25rem'
            badgesWrapper.style.alignItems = 'center'
            
            if (idea.authorType === 'self') {
                const yoursBadge = document.createElement('span')
                yoursBadge.className = 'ideas-card-yours-badge'
                yoursBadge.textContent = 'Your idea'
                badgesWrapper.appendChild(yoursBadge)
            }

            if (idea.pendingReview || idea.qualityNudgeBypassed || flaggedIdeaIds.has(idea.id)) {
                const flagged = document.createElement('span')
                flagged.className = 'ideas-review-flag'
                flagged.textContent = 'Marked for review'
                badgesWrapper.appendChild(flagged)
            }

            const discoveryBadge = discoveryBadgeByIdeaId?.get(idea.id)
            if (discoveryBadge) {
                badgesWrapper.appendChild(createDiscoveryBadgeElement(idea.id, discoveryBadge, onDiscoveryBadgeClick))
            }
            
            if (badgesWrapper.children.length > 0) {
                card.appendChild(badgesWrapper)
            }

            const body = document.createElement('p')
            body.className = 'ideas-card-body'
            body.textContent = idea.body
            card.appendChild(body)
        }

        // Add reactions summary + emoji button as click affordance at bottom of card
        const totalReactions = idea.reactions.reduce((sum, r) => sum + r.count, 0)
        const firstEmoji = idea.reactions.length > 0 ? idea.reactions[0].emoji : null
        const reactionsRow = document.createElement('div')
        reactionsRow.className = 'ideas-card-reactions'
        if (totalReactions > 0 && firstEmoji) {
            const emojiSpan = document.createElement('span')
            emojiSpan.className = 'ideas-card-reactions-emoji'
            emojiSpan.textContent = firstEmoji
            const countSpan = document.createElement('span')
            countSpan.className = 'ideas-card-reactions-count'
            countSpan.textContent = String(totalReactions)
            reactionsRow.append(emojiSpan, countSpan)
        }
        // Emoji add button (matching ideaPanel's +:) button)
        const addReactionBtn = document.createElement('span')
        addReactionBtn.className = 'ideas-card-reaction-add-btn'
        addReactionBtn.textContent = '+ :)'
        addReactionBtn.setAttribute('aria-label', 'Add reaction')
        reactionsRow.appendChild(addReactionBtn)
        card.appendChild(reactionsRow)

        // Keep stable index mapping for controller and click handlers.
        card.setAttribute('data-original-index', String(index))
        card.setAttribute('data-idea-index', String(index))
        card.style.setProperty('--card-index', String(index))
        list.appendChild(card)
    })
}
