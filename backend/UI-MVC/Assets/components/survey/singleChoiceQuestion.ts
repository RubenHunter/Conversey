import type { Question } from '../../models/question.ts'
import { generateQuestionHeader, initQuestionSpeakerForWrapper } from './shared.ts'

export type QuestionAnswer = number | string | number[] | null

export interface QuestionComponent {
    getAnswer(): QuestionAnswer
    validate(): boolean
    lock(): void
    unlock(): void
    onAnswer(callback: () => void): void
    setAnswer(answer: QuestionAnswer): void
    getElement(): HTMLElement
    destroy?(): void
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

    // Initialize TTS for speaker button in question header
    initQuestionSpeakerForWrapper(wrapper)

    const optionsContainer = wrapper.querySelector(`#options-${question.id}`)!
    const labels = optionsContainer.querySelectorAll<HTMLLabelElement>('.survey-option-label')

    function applySelectedState(nextSelectedOptionId: number | null): void {
        selectedOptionId = nextSelectedOptionId

        if (selectedOptionId === null) {
            labels.forEach((label) => {
                label.classList.remove('selected')
                label.classList.remove('disabled')
            })
        } else {
            labels.forEach((label) => {
                const optionId = Number(label.getAttribute('data-option-id'))
                const isSelected = optionId === selectedOptionId
                label.classList.toggle('selected', isSelected)
                label.classList.toggle('disabled', !isSelected)
            })
        }

        const errorEl = wrapper.querySelector(`#error-${question.id}`)
        errorEl?.classList.remove('show')
    }

    labels.forEach((label) => {
        label.addEventListener('click', () => {
            if (isLocked) return

            const optionId = Number(label.getAttribute('data-option-id'))
            applySelectedState(selectedOptionId === optionId ? null : optionId)

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
        setAnswer: (answer) => {
            const nextSelected = typeof answer === 'number' && Number.isFinite(answer) ? answer : null
            const hasMatchingOption = [...labels].some(
                (label) => Number(label.getAttribute('data-option-id')) === nextSelected,
            )
            applySelectedState(hasMatchingOption ? nextSelected : null)
        },
        getElement: () => wrapper,
    }
}



