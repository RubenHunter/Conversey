import type { Question } from '../../models/question.ts'
import { generateQuestionHeader } from './shared.ts'

export interface QuestionComponent {
    getAnswer(): number | string | number[] | null
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

    wrapper.innerHTML = `
        ${generateQuestionHeader(question, questionNumber)}

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



