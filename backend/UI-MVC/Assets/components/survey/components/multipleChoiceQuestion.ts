import type { QuestionComponent } from './singleChoiceQuestion'
import { generateQuestionHeader, initQuestionSpeakerForWrapper } from '../utils/surveyUtils'
import {FixedQuestion} from "../../../models/question.ts";

export function renderMultipleChoiceQuestion(question: FixedQuestion, index: number): QuestionComponent {
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
            ${(question.possibleAnswers ?? [])
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

    initQuestionSpeakerForWrapper(wrapper)

    const optionsContainer = wrapper.querySelector(`#options-${question.id}`)!
    const labels = optionsContainer.querySelectorAll<HTMLLabelElement>('.survey-option-label')

    const validOptionIds = new Set<number>(
        [...labels].map((label) => Number(label.getAttribute('data-option-id'))),
    )

    function applySelectedState(nextSelectedOptionIds: number[]): void {
        selectedOptionIds = nextSelectedOptionIds

        labels.forEach((label) => {
            const optionId = Number(label.getAttribute('data-option-id'))
            label.classList.toggle('selected', selectedOptionIds.includes(optionId))
            label.classList.remove('disabled')
        })

        const errorEl = wrapper.querySelector(`#error-${question.id}`)
        errorEl?.classList.remove('show')
    }

    labels.forEach((label) => {
        label.addEventListener('click', () => {
            if (isLocked) return

            const optionId = Number(label.getAttribute('data-option-id'))

            if (selectedOptionIds.includes(optionId)) {
                applySelectedState(selectedOptionIds.filter((id) => id !== optionId))
            } else {
                applySelectedState([...selectedOptionIds, optionId])
            }

            answerCallback?.()
        })
    })

    return {
        getAnswer: () => selectedOptionIds,
        validate: () => {
            if (question.required && selectedOptionIds.length === 0) {
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
            const restoredIds = Array.isArray(answer)
                ? [...new Set(answer.filter((value): value is number => Number.isFinite(value) && validOptionIds.has(value)))]
                : []
            applySelectedState(restoredIds)
        },
        getElement: () => wrapper,
    }
}


