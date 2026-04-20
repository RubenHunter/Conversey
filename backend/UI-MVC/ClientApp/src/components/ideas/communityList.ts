import type { Idea, IdeaTopic } from '../../models/idea.ts'
import type { ActiveView } from './types.ts'

interface RenderCommunityListParams {
    list: HTMLDivElement
    ideas: Idea[]
    activeView: ActiveView
    topics: IdeaTopic[]
    upBtn: HTMLButtonElement
    downBtn: HTMLButtonElement
    flaggedIdeaIds: ReadonlySet<number>
}

export function renderCommunityIdeasList({ list, ideas, activeView, topics, upBtn, downBtn, flaggedIdeaIds }: RenderCommunityListParams): void {
    list.innerHTML = ''

    if (ideas.length === 0) {
        list.innerHTML = `<p class="ideas-empty">${activeView.type === 'my-ideas' ? 'You have not submitted any ideas yet.' : 'No ideas yet for this view.'}</p>`
        upBtn.disabled = true
        downBtn.disabled = true
        return
    }

    ideas.forEach((idea, index) => {
        const card = document.createElement('article')
        card.className = 'ideas-card'

        if (activeView.type === 'my-ideas') {
            card.classList.add('ideas-card--my-idea')

            const topicLabel = document.createElement('p')
            topicLabel.className = 'ideas-card-topic'
            topicLabel.textContent = topics.find((topic) => topic.id === idea.topicId)?.title ?? 'Unknown topic'

            if (idea.pendingReview || flaggedIdeaIds.has(idea.id)) {
                const flagged = document.createElement('span')
                flagged.className = 'ideas-review-flag'
                flagged.textContent = 'Marked for review'
                card.appendChild(flagged)
            }

            const body = document.createElement('p')
            body.className = 'ideas-card-body'
            body.textContent = idea.body

            card.append(topicLabel, body)
        } else {
            if (idea.authorType === 'self') {
                const yoursBadge = document.createElement('span')
                yoursBadge.className = 'ideas-card-yours-badge'
                yoursBadge.textContent = 'Your idea'
                card.appendChild(yoursBadge)
            }

            if (idea.pendingReview || flaggedIdeaIds.has(idea.id)) {
                const flagged = document.createElement('span')
                flagged.className = 'ideas-review-flag'
                flagged.textContent = 'Marked for review'
                card.appendChild(flagged)
            }

            const body = document.createElement('p')
            body.className = 'ideas-card-body'
            body.textContent = idea.body
            card.appendChild(body)
        }

        card.setAttribute('data-idea-index', String(index))
        list.appendChild(card)
    })
}
