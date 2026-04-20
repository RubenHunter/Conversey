import type { IdeaTopic } from '../../models/idea.ts'
import type { ActiveView } from './types.ts'

interface CreateTopicModalParams {
    root: HTMLElement
    topics: IdeaTopic[]
    onSelect: (activeView: ActiveView) => void
}

interface TopicModalController {
    open(invoker: HTMLButtonElement): void
    close(): void
    isOpen(): boolean
    renderTopics(activeView: ActiveView): void
}

export function createTopicModalController({
    root,
    topics,
    onSelect,
}: CreateTopicModalParams): TopicModalController {
    const modal = root.querySelector<HTMLDivElement>('#topic-modal')!
    const backdrop = root.querySelector<HTMLDivElement>('#topic-modal-backdrop')!
    const list = root.querySelector<HTMLDivElement>('#topic-modal-list')!
    const closeBtn = root.querySelector<HTMLButtonElement>('#topic-modal-close')!
    const topicTrigger = root.querySelector<HTMLButtonElement>('#ideas-topic-trigger')!
    const topicFloatingTrigger = root.querySelector<HTMLButtonElement>('#ideas-topic-trigger-floating')!

    let isOpen = false
    let currentInvoker: HTMLButtonElement = topicTrigger

    function setExpanded(value: boolean): void {
        const ariaValue = value ? 'true' : 'false'
        topicTrigger.setAttribute('aria-expanded', ariaValue)
        topicFloatingTrigger.setAttribute('aria-expanded', ariaValue)
    }

    function focusTrap(): void {
        const focusableElements = modal.querySelectorAll<HTMLElement>(
            'button, [tabindex]:not([tabindex="-1"])'
        )
        if (focusableElements.length > 0) {
            focusableElements[0].focus()
        }
    }

    function open(invoker: HTMLButtonElement = topicTrigger): void {
        if (isOpen) return

        isOpen = true
        currentInvoker = invoker
        modal.hidden = false
        backdrop.hidden = false
        setExpanded(true)

        focusTrap()
    }

    function close(): void {
        if (!isOpen) return

        isOpen = false
        modal.hidden = true
        backdrop.hidden = true
        setExpanded(false)

        // Restore focus to the trigger that opened the modal
        if (!currentInvoker.hidden) {
            currentInvoker.focus()
        } else {
            topicTrigger.focus()
        }
    }

    function renderTopics(activeView: ActiveView): void {
        list.innerHTML = ''

        // Topic options
        topics.forEach((topic) => {
            const option = document.createElement('button')
            option.className = 'modal-option'
            option.textContent = topic.title
            option.setAttribute('data-topic-id', String(topic.id))
            if (activeView.type === 'topic' && activeView.topicId === topic.id) {
                option.classList.add('selected')
            }
            option.addEventListener('click', () => {
                onSelect({ type: 'topic', topicId: topic.id })
                close()
            })
            list.appendChild(option)
        })

        const divider = document.createElement('div')
        divider.className = 'modal-list-divider'
        divider.setAttribute('role', 'separator')
        //divider.textContent = 'Other views'
        list.appendChild(divider)

        const myIdeasOption = document.createElement('button')
        myIdeasOption.className = 'modal-option modal-option--my-ideas'
        myIdeasOption.textContent = 'My ideas'
        myIdeasOption.setAttribute('data-view-type', 'my-ideas')
        if (activeView.type === 'my-ideas') {
            myIdeasOption.classList.add('selected')
        }
        myIdeasOption.addEventListener('click', () => {
            onSelect({ type: 'my-ideas' })
            close()
        })
        list.appendChild(myIdeasOption)
    }

    // Wire up event listeners
    closeBtn.addEventListener('click', close)
    backdrop.addEventListener('click', close)

    document.addEventListener('keydown', (event) => {
        if (event.key === 'Escape' && isOpen) {
            close()
        }
    })

    return {
        open,
        close,
        isOpen: () => isOpen,
        renderTopics,
    }
}

