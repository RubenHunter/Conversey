import type { Question } from '../../models/question.ts'
import type { QuestionComponent } from './singleChoiceQuestion.ts'
import { generateQuestionHeader } from './shared'

export function renderScaleQuestion(question: Question, index: number): QuestionComponent {
    let scaleValue: number | null = null
    let answerCallback: (() => void) | null = null
    let isLocked = false

    const wrapper = document.createElement('div')
    wrapper.setAttribute('data-question-index', String(index))
    wrapper.className = 'survey-question-group'

    const questionNumber = index + 1

    wrapper.innerHTML = `
        ${generateQuestionHeader(question, questionNumber)}

        <div class="survey-textarea-wrapper">
            <div class="relative">
                <input
                    id="scale-${question.id}"
                    class="survey-textarea"
                    type="number"
                    inputmode="numeric"
                    placeholder="Enter a number"
                />
            </div>
        </div>

        <p class="survey-error" id="error-${question.id}">
            Please provide a numeric answer to continue.
        </p>
    `

    const input = wrapper.querySelector<HTMLInputElement>(`#scale-${question.id}`)!

    function applyScaleValue(nextValue: number | null): void {
        scaleValue = nextValue
        input.value = scaleValue === null ? '' : String(scaleValue)
        const errorEl = wrapper.querySelector(`#error-${question.id}`)
        if (scaleValue !== null) {
            errorEl?.classList.remove('show')
        }
    }

    input.addEventListener('input', () => {
        if (isLocked) return
        const parsed = Number(input.value)
        applyScaleValue(Number.isFinite(parsed) && input.value.trim().length > 0 ? parsed : null)
        answerCallback?.()
    })

    return {
        getAnswer: () => scaleValue,
        validate: () => {
            if (question.isRequired && scaleValue === null) {
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
            applyScaleValue(typeof answer === 'number' && Number.isFinite(answer) ? answer : null)
        },
        getElement: () => wrapper,
    }
}

