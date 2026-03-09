import type { Question } from '../../models/question.ts'
import type { QuestionComponent } from './singleChoiceQuestion.ts'

export function renderOpenTextQuestion(question: Question, index: number): QuestionComponent {
    let textValue = ''
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
                    <span class="survey-answer-hint">Free text answer</span>
                </div>
            </div>
        </div>

        <div class="survey-textarea-wrapper">
            <div class="survey-magic-row">
                <button class="survey-magic-btn" title="Answer in Magic Mode (coming soon)">
                    <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                              d="M5 3v4M3 5h4M6 17v4m-2-2h4m5-16l2.286 6.857L21 12l-5.714 2.143L13 21l-2.286-6.857L5 12l5.714-2.143L13 3z"/>
                    </svg>
                    Magic Mode
                </button>
            </div>

            <div class="relative">
                <textarea
                    id="textarea-${question.id}"
                    class="survey-textarea"
                    placeholder="Type your answer here..."
                    rows="4"
                ></textarea>

                <button
                    class="survey-mic-btn"
                    title="Voice input (coming soon)"
                    id="mic-btn-${question.id}"
                >
                    <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                              d="M19 11a7 7 0 01-14 0m7 7v4m-4 0h8M12 1a3 3 0 00-3 3v7a3 3 0 006 0V4a3 3 0 00-3-3z"/>
                    </svg>
                </button>
            </div>
        </div>

        <p class="survey-error" id="error-${question.id}">
            Please provide an answer to continue.
        </p>
    `

    const textarea = wrapper.querySelector<HTMLTextAreaElement>(`#textarea-${question.id}`)!

    textarea.addEventListener('focus', () => {
        if (isLocked) return
    })

    textarea.addEventListener('blur', () => {
    })

    textarea.addEventListener('input', () => {
        if (isLocked) return
        textValue = textarea.value.trim()

        // Hide error when user starts typing
        const errorEl = wrapper.querySelector(`#error-${question.id}`)
        if (textValue.length > 0) {
            errorEl?.classList.remove('show')
        }

        answerCallback?.()
    })

    return {
        getAnswer: () => (textValue.length > 0 ? textValue : null),
        validate: () => {
            if (question.isRequired && textValue.length === 0) {
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





