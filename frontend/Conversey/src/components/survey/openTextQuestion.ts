import type { Question } from '../../models/question.ts'
import type { QuestionComponent } from './singleChoiceQuestion.ts'

export function renderOpenTextQuestion(question: Question, index: number): QuestionComponent {
    let textValue = ''
    let answerCallback: (() => void) | null = null
    let isLocked = false

    const wrapper = document.createElement('div')
    wrapper.setAttribute('data-question-index', String(index))
    wrapper.className = 'transition-all duration-300'

    const questionNumber = index + 1
    const requiredBadge = question.isRequired
        ? `<span class="ml-2 text-xs font-medium px-2 py-0.5 rounded-full"
                 style="background-color: var(--color-error-bg); color: var(--color-error);">Required</span>`
        : ''

    wrapper.innerHTML = `
        <div class="mb-2 flex items-center flex-wrap">
            <h3 class="font-semibold" style="font-size: var(--font-size-lg); color: var(--color-text);">
                ${questionNumber}. ${question.text}
            </h3>
            ${requiredBadge}
        </div>

        <div class="relative mt-3">
            <div class="flex items-center justify-end mb-2">
                <button
                    class="flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-sm font-medium transition-all"
                    style="background-color: var(--color-primary-light)20; color: var(--color-primary); border: none; cursor: pointer;"
                    title="Answer in Magic Mode (coming soon)"
                    id="magic-btn-${question.id}"
                >
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
                    class="w-full p-4 pr-12 rounded-xl border-2 resize-none transition-all focus:outline-none"
                    style="background-color: var(--color-surface); border-color: var(--color-border); color: var(--color-text); font-size: var(--font-size-base); font-family: var(--font-primary); min-height: 120px;"
                    placeholder="Type your answer here..."
                    rows="4"
                ></textarea>

                <button
                    class="absolute bottom-3 right-3 w-10 h-10 rounded-full flex items-center justify-center transition-all"
                    style="background-color: var(--color-disabled-bg); border: none; cursor: pointer; color: var(--color-text-muted);"
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

        <p class="mt-2 text-sm hidden" id="error-${question.id}"
           style="color: var(--color-error);">
            Please provide an answer to continue.
        </p>
    `

    const textarea = wrapper.querySelector<HTMLTextAreaElement>(`#textarea-${question.id}`)!

    textarea.addEventListener('focus', () => {
        if (isLocked) return
        textarea.style.borderColor = 'var(--color-border-focus)'
    })

    textarea.addEventListener('blur', () => {
        textarea.style.borderColor = 'var(--color-border)'
    })

    textarea.addEventListener('input', () => {
        if (isLocked) return
        textValue = textarea.value.trim()

        // Hide error when user starts typing
        const errorEl = wrapper.querySelector(`#error-${question.id}`)
        if (textValue.length > 0) {
            errorEl?.classList.add('hidden')
        }

        answerCallback?.()
    })

    return {
        getAnswer: () => (textValue.length > 0 ? textValue : null),
        validate: () => {
            if (question.isRequired && textValue.length === 0) {
                const errorEl = wrapper.querySelector(`#error-${question.id}`)
                errorEl?.classList.remove('hidden')
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

