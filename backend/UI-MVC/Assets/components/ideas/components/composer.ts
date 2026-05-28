import { getSurveyStrings } from '../../../i18n/survey'
import type { IdeaTopic } from '../../../models/idea'
import type { ActiveView } from '../types'

interface RenderComposerParams {
    activeView: ActiveView
    topics: IdeaTopic[]
    ideasGrid: HTMLDivElement
    ideasCompose: HTMLElement
    composeTopic: HTMLSpanElement
    prompt: HTMLParagraphElement
    promptSpeakerBtn: HTMLButtonElement
    textarea: HTMLTextAreaElement
    submitBtn: HTMLButtonElement
    brainstormBtn: HTMLButtonElement
    speakBtn: HTMLButtonElement
}

export function renderIdeasComposer({
    activeView,
    topics,
    ideasGrid,
    ideasCompose,
    composeTopic,
    prompt,
    promptSpeakerBtn,
    textarea,
    submitBtn,
    brainstormBtn,
    speakBtn,
}: RenderComposerParams): void {
    const isMyIdeasView = activeView.type === 'my-ideas'
    const topic = activeView.type === 'topic' ? topics.find((item) => item.id === activeView.topicId) : undefined

    ideasGrid.classList.toggle('ideas-grid--my-ideas', isMyIdeasView)
    ideasCompose.hidden = isMyIdeasView

    if (!topic) {
        composeTopic.textContent = 'My ideas'
        prompt.textContent = 'Viewing all your ideas. Pick a topic to submit a new one.'
        textarea.value = ''
        textarea.disabled = true
        submitBtn.disabled = true
        brainstormBtn.disabled = true
        speakBtn.disabled = true
        promptSpeakerBtn.disabled = true
        return
    }

    composeTopic.textContent = topic.title
    const t = getSurveyStrings()
    prompt.innerHTML = `<span class="ideas-prompt-prefix">${t.topicQuestionLabel}</span> ${topic.prompt}`
    textarea.disabled = false
    submitBtn.disabled = textarea.value.trim().length === 0
    brainstormBtn.disabled = false
    speakBtn.disabled = false
    promptSpeakerBtn.disabled = false
}

