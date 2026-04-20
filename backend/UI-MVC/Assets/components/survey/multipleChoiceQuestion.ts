import type { Question } from '../../models/question.ts'
import type { QuestionComponent } from './singleChoiceQuestion.ts'
import { generateQuestionHeader } from './shared.ts'

export function renderMultipleChoiceQuestion(question: Question, index: number): QuestionComponent {
    let selectedOptionIds: number[] = []
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
                <label class="survey-option-label multiple-choice" data-option-id="${option.id}">
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

            if (selectedOptionIds.includes(optionId)) {
                selectedOptionIds = selectedOptionIds.filter((id) => id !== optionId)
                label.classList.remove('selected')
            } else {
                selectedOptionIds = [...selectedOptionIds, optionId]
                label.classList.add('selected')
            }

            labels.forEach((l) => {
                l.classList.toggle('disabled', false)
            })

            const errorEl = wrapper.querySelector(`#error-${question.id}`)
            errorEl?.classList.remove('show')

            answerCallback?.()
        })
    })

    return {
        getAnswer: () => selectedOptionIds,
        validate: () => {
            if (question.isRequired && selectedOptionIds.length === 0) {
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


