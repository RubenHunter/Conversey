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
        <div class="flex flex-col gap-2 mt-3" id="options-${question.id}">
            ${(question.options ?? [])
                .map(
                    (option) => `
                <label
                    class="flex items-center gap-3 p-4 rounded-xl cursor-pointer transition-all border-2"
                    style="background-color: var(--color-surface); border-color: var(--color-border);"
                    data-option-id="${option.id}"
                >
                    <div class="relative flex-shrink-0 w-5 h-5 rounded-full border-2 transition-all"
                         style="border-color: var(--color-border);"
                         data-radio="${option.id}">
                        <div class="absolute inset-1 rounded-full transition-all scale-0"
                             style="background-color: var(--color-primary);"
                             data-radio-dot="${option.id}">
                        </div>
                    </div>
                    <span style="color: var(--color-text); font-size: var(--font-size-base);">
                        ${option.text}
                    </span>
                </label>
            `,
                )
                .join('')}
        </div>
        <p class="mt-2 text-sm hidden" id="error-${question.id}"
           style="color: var(--color-error);">
            Please select an option to continue.
        </p>
    `

    const optionsContainer = wrapper.querySelector(`#options-${question.id}`)!
    const labels = optionsContainer.querySelectorAll<HTMLLabelElement>('label')

    labels.forEach((label) => {
        label.addEventListener('click', () => {
            if (isLocked) return

            const optionId = Number(label.getAttribute('data-option-id'))
            selectedOptionId = optionId

            // Reset all
            labels.forEach((l) => {
                l.style.borderColor = 'var(--color-border)'
                l.style.backgroundColor = 'var(--color-surface)'
                const radio = l.querySelector<HTMLElement>(`[data-radio]`)
                if (radio) radio.style.borderColor = 'var(--color-border)'
                const dot = l.querySelector<HTMLElement>(`[data-radio-dot]`)
                if (dot) dot.style.transform = 'scale(0)'
            })

            // Highlight selected
            label.style.borderColor = 'var(--color-primary)'
            label.style.backgroundColor = 'var(--color-primary-light)15'
            const radio = label.querySelector<HTMLElement>(`[data-radio]`)
            if (radio) radio.style.borderColor = 'var(--color-primary)'
            const dot = label.querySelector<HTMLElement>(`[data-radio-dot]`)
            if (dot) dot.style.transform = 'scale(1)'

            // Hide error
            const errorEl = wrapper.querySelector(`#error-${question.id}`)
            errorEl?.classList.add('hidden')

            answerCallback?.()
        })
    })

    return {
        getAnswer: () => selectedOptionId,
        validate: () => {
            if (question.isRequired && selectedOptionId === null) {
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

