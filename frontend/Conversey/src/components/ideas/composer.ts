import type { IdeaTopic } from '../../models/idea.ts'
import type { ActiveView } from './types.ts'

interface RenderComposerParams {
    activeView: ActiveView
    topics: IdeaTopic[]
    ideasGrid: HTMLDivElement
    ideasCompose: HTMLElement
    composeTopic: HTMLParagraphElement
    prompt: HTMLParagraphElement
    textarea: HTMLTextAreaElement
    submitBtn: HTMLButtonElement
    magicBtn: HTMLButtonElement
    speakBtn: HTMLButtonElement
}

export function renderIdeasComposer({
    activeView,
    topics,
    ideasGrid,
    ideasCompose,
    composeTopic,
    prompt,
    textarea,
    submitBtn,
    magicBtn,
    speakBtn,
}: RenderComposerParams): void {
    const isMyIdeasView = activeView.type === 'my-ideas'
    const topic = activeView.type === 'topic' ? topics.find((item) => item.id === activeView.topicId) : undefined

    ideasGrid.classList.toggle('ideas-grid--my-ideas', isMyIdeasView)
    ideasCompose.hidden = isMyIdeasView

    if (!topic) {
        composeTopic.textContent = 'Current view: My ideas'
        prompt.textContent = 'Viewing all your ideas. Pick a topic to submit a new one.'
        textarea.value = ''
        textarea.disabled = true
        submitBtn.disabled = true
        magicBtn.disabled = true
        speakBtn.disabled = true
        return
    }

    composeTopic.textContent = `Topic question: ${topic.title}`
    prompt.textContent = topic.prompt
    textarea.disabled = false
    submitBtn.disabled = textarea.value.trim().length === 0
    magicBtn.disabled = false
    speakBtn.disabled = false
}

