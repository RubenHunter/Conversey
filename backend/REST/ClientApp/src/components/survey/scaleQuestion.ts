import type { Question } from '../../models/question.ts'
import type { QuestionComponent } from './singleChoiceQuestion.ts'

export function renderScaleQuestion(question: Question, index: number): QuestionComponent {
    let scaleValue: number | null = null
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
                    <span class="survey-answer-hint">Choose a number</span>
                </div>
            </div>
        </div>

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

    input.addEventListener('input', () => {
        if (isLocked) return
        const parsed = Number(input.value)
        scaleValue = Number.isFinite(parsed) && input.value.trim().length > 0 ? parsed : null
        const errorEl = wrapper.querySelector(`#error-${question.id}`)
        if (scaleValue !== null) {
            errorEl?.classList.remove('show')
        }
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
        getElement: () => wrapper,
    }
}

