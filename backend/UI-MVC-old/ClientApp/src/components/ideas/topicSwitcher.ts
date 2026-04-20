import type { IdeaTopic } from '../../models/idea.ts'
import type { ActiveView } from './types.ts'

interface RenderTopicMenuParams {
    menu: HTMLDivElement
    topics: IdeaTopic[]
    activeView: ActiveView
    onSelectMyIdeas: () => void
    onSelectTopic: (topicId: number) => void
}

export function getActiveIdeasLabel(activeView: ActiveView, topics: IdeaTopic[]): string {
    if (activeView.type === 'my-ideas') return 'My ideas'
    const topic = topics.find((item) => item.id === activeView.topicId)
    return topic ? topic.title : 'Select a topic'
}

export function renderTopicMenu({ menu, topics, activeView, onSelectMyIdeas, onSelectTopic }: RenderTopicMenuParams): void {
    menu.innerHTML = ''

    const myIdeasBtn = document.createElement('button')
    myIdeasBtn.type = 'button'
    myIdeasBtn.className = 'ideas-topic-option ideas-topic-option--my-ideas'
    if (activeView.type === 'my-ideas') myIdeasBtn.classList.add('active')
    myIdeasBtn.textContent = 'My ideas'
    myIdeasBtn.addEventListener('click', onSelectMyIdeas)
    menu.appendChild(myIdeasBtn)

    topics.forEach((topic) => {
        const btn = document.createElement('button')
        btn.type = 'button'
        btn.className = 'ideas-topic-option'
        if (activeView.type === 'topic' && activeView.topicId === topic.id) {
            btn.classList.add('active')
        }
        btn.textContent = topic.title
        btn.addEventListener('click', () => onSelectTopic(topic.id))
        menu.appendChild(btn)
    })
}

