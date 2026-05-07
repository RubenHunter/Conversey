import type { IdeaTopic } from '../../models/idea'
import type { ActiveView } from './types'

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
    const modal = root.querySelector<HTMLDivElement>('#topic-modal')
    const backdrop = root.querySelector<HTMLDivElement>('#topic-modal-backdrop')
    const list = root.querySelector<HTMLDivElement>('#topic-modal-list')
    const closeBtn = root.querySelector<HTMLButtonElement>('#topic-modal-close')
    const topicTrigger = root.querySelector<HTMLButtonElement>('#ideas-topic-trigger')
    const topicFloatingTrigger = root.querySelector<HTMLButtonElement>('#ideas-topic-trigger-floating')

    if (!modal || !backdrop || !list || !closeBtn || !topicTrigger || !topicFloatingTrigger) {
        console.warn('[topicModal] Required modal elements are missing. Topic modal is disabled for this view.')
        return {
            open: () => {},
            close: () => {},
            isOpen: () => false,
            renderTopics: () => {},
        }
    }

    // Non-null assertions after guard clause
    const modalEl = modal!
    const backdropEl = backdrop!
    const listEl = list!
    const closeBtnEl = closeBtn!
    const topicTriggerEl = topicTrigger!
    const topicFloatingTriggerEl = topicFloatingTrigger!

    let isOpen = false
    let currentInvoker: HTMLButtonElement = topicTriggerEl

    function setExpanded(value: boolean): void {
        const ariaValue = value ? 'true' : 'false'
        topicTriggerEl.setAttribute('aria-expanded', ariaValue)
        topicFloatingTriggerEl.setAttribute('aria-expanded', ariaValue)
    }

    function focusTrap(): void {
        const focusableElements = modalEl.querySelectorAll<HTMLElement>(
            'button, [tabindex]:not([tabindex="-1"])'
        )
        if (focusableElements.length > 0) {
            focusableElements[0].focus()
        }
    }

    function open(invoker: HTMLButtonElement = topicTriggerEl): void {
        if (isOpen) return

        isOpen = true
        currentInvoker = invoker
        modalEl.hidden = false
        backdropEl.hidden = false
        setExpanded(true)

        focusTrap()
    }

    function close(): void {
        if (!isOpen) return

        isOpen = false
        modalEl.hidden = true
        backdropEl.hidden = true
        setExpanded(false)

        // Restore focus to the trigger that opened the modal
        if (!currentInvoker.hidden) {
            currentInvoker.focus()
        } else {
            topicTriggerEl.focus()
        }
    }

    function renderTopics(activeView: ActiveView): void {
        listEl.innerHTML = ''

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
            listEl.appendChild(option)
        })

        const divider = document.createElement('div')
        divider.className = 'modal-list-divider'
        divider.setAttribute('role', 'separator')
        //divider.textContent = 'Other views'
        listEl.appendChild(divider)

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
        listEl.appendChild(myIdeasOption)
    }

    // Wire up event listeners
    closeBtnEl.addEventListener('click', close)
    backdropEl.addEventListener('click', close)

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
