import type { Question } from '../../models/question.ts'

export interface QuestionComponent {
    getAnswer(): number | string | null
    validate(): boolean
    lock(): void
    unlock(): void
    onAnswer(callback: () => void): void
    getElement(): HTMLElement
}

export function renderSingleChoiceQuestion(question: Question, index: number): QuestionComponent {
    let selectedOptionId: number | null = null
    let answerCallback: (() => void) | null = null
    let isLocked = false

    const wrapper = document.createElement('div')
    wrapper.setAttribute('data-question-index', String(index))
    wrapper.className = 'survey-question-group'

    const questionNumber = index + 1
    const requiredBadge = question.isRequired ? '<span class="survey-required-badge">Required</span>' : ''

    wrapper.innerHTML = `
        <div class="survey-question-header">
            <span class="survey-question-number">${questionNumber}</span>
            <div class="flex-1">
                <div class="survey-question-title">
                    <span>${question.text}</span>
                    <svg class="survey-speaker-icon" fill="currentColor" viewBox="0 0 24 24" aria-label="Read question aloud">
                        <path d="M3 9v6h4l5 5V4L7 9H3zm13.5 3c0-1.77-1.02-3.29-2.5-4.03v8.05c1.48-.73 2.5-2.26 2.5-4.02zM14 3.23v2.06c2.89.86 5 3.54 5 6.71s-2.11 5.85-5 6.71v2.06c4.01-.91 7-4.49 7-8.77s-2.99-7.86-7-8.77z"/>
                    </svg>
                </div>
                <div class="survey-question-meta">
                    ${requiredBadge}
                    <span class="survey-answer-hint">Only 1 answer possible</span>
                </div>
            </div>
        </div>

        <div class="survey-options" id="options-${question.id}">
            ${(question.options ?? [])
                .map(
                    (option) => `
                <label class="survey-option-label" data-option-id="${option.id}">
                    <div class="survey-radio-circle">
                        <div class="survey-radio-dot"></div>
                    </div>
                    <span class="survey-option-text">${option.text}</span>
                </label>
            `,
                )
                .join('')}
        </div>

        <p class="survey-error" id="error-${question.id}">
            Please select an option to continue.
        </p>
    `

    const optionsContainer = wrapper.querySelector(`#options-${question.id}`)!
    const labels = optionsContainer.querySelectorAll<HTMLLabelElement>('.survey-option-label')

    labels.forEach((label) => {
        label.addEventListener('click', () => {
            if (isLocked) return

            const optionId = Number(label.getAttribute('data-option-id'))

            // If clicking the same option, deselect it
            if (selectedOptionId === optionId) {
                selectedOptionId = null
                label.classList.remove('selected')

                // Re-enable all options
                labels.forEach((l) => {
                    l.classList.remove('disabled')
                })

                // Hide error
                const errorEl = wrapper.querySelector(`#error-${question.id}`)
                errorEl?.classList.remove('show')

                answerCallback?.()
                return
            }

            // Otherwise, select the new option
            selectedOptionId = optionId

            // Reset all
            labels.forEach((l) => {
                l.classList.remove('selected')
                l.classList.add('disabled')
            })

            // Highlight selected and enable it
            label.classList.add('selected')
            label.classList.remove('disabled')

            // Hide error
            const errorEl = wrapper.querySelector(`#error-${question.id}`)
            errorEl?.classList.remove('show')

            answerCallback?.()
        })
    })

    return {
        getAnswer: () => selectedOptionId,
        validate: () => {
            if (question.isRequired && selectedOptionId === null) {
                const errorEl = wrapper.querySelector(`#error-${question.id}`)
                errorEl?.classList.add('show')
                return false
            }
            return true
        },
        lock: () => {
            isLocked = true
            wrapper.style.opacity = '0.4'
            wrapper.style.pointerEvents = 'none'
        },
        unlock: () => {
            isLocked = false
            wrapper.style.opacity = '1'
            wrapper.style.pointerEvents = 'auto'
        },
        onAnswer: (cb) => {
            answerCallback = cb
        },
        getElement: () => wrapper,
    }
}



