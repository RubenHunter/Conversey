import type { RouteParams } from '../../utils/router.ts'
import { navigate } from '../../utils/router.ts'
import { getProject } from '../../services/projectService.ts'
import { getQuestions, submitResponse } from '../../services/surveyService.ts'
import { QuestionType } from '../../models/question.ts'
import type { ResponseAnswer } from '../../models/response.ts'
import { renderSingleChoiceQuestion } from './singleChoiceQuestion.ts'
import type { QuestionComponent } from './singleChoiceQuestion.ts'
import { renderOpenTextQuestion } from './openTextQuestion.ts'
import { renderScrollNav } from '../scrollNav.ts'
import type { ScrollNav } from '../scrollNav.ts'

export async function renderSurveyPage(container: HTMLElement, params: RouteParams): Promise<void> {
    const project = await getProject(params.organizationSlug, params.projectSlug)
    const questions = await getQuestions(project.id)

    let currentQuestionIndex = 0
    let scrollNav: ScrollNav | null = null

    // Build page structure
    container.innerHTML = `
        <div class="px-6 py-6 pb-32">
            <div class="mb-6">
                <p class="text-sm font-medium mb-1" style="color: var(--color-primary);">
                    ${project.title}
                </p>
                <div class="flex items-center justify-between">
                    <h2 class="text-xl font-bold" style="color: var(--color-text);">
                        Survey
                    </h2>
                    <span class="text-sm font-medium px-3 py-1 rounded-full"
                          style="background-color: var(--color-primary-light)20; color: var(--color-primary);"
                          id="progress-badge">
                        0 / ${questions.length}
                    </span>
                </div>
                <div class="w-full h-2 rounded-full mt-3" style="background-color: var(--color-disabled-bg);">
                    <div class="h-2 rounded-full transition-all duration-500"
                         style="background-color: var(--color-primary); width: 0%;"
                         id="progress-bar">
                    </div>
                </div>
            </div>

            <div class="flex flex-col gap-8" id="questions-container"></div>

            <div class="mt-8">
                <button
                    id="btn-submit"
                    class="w-full py-4 px-6 rounded-xl font-semibold text-lg transition-all"
                    style="background-color: var(--color-disabled-bg); color: var(--color-disabled-text); border: none; cursor: not-allowed;"
                    disabled
                >
                    Submit Survey
                </button>
            </div>
        </div>
    `

    const questionsContainer = container.querySelector<HTMLDivElement>('#questions-container')!
    const submitBtn = container.querySelector<HTMLButtonElement>('#btn-submit')!
    const progressBar = container.querySelector<HTMLDivElement>('#progress-bar')!
    const progressBadge = container.querySelector<HTMLSpanElement>('#progress-badge')!

    // Render all question components
    const components: QuestionComponent[] = questions.map((question, index) => {
        const component =
            question.type === QuestionType.SingleChoice
                ? renderSingleChoiceQuestion(question, index)
                : renderOpenTextQuestion(question, index)

        questionsContainer.appendChild(component.getElement())

        // Lock all questions except the first
        if (index > 0) {
            component.lock()
        }

        return component
    })

    // Track answered state for progressive unlock
    const answeredState = new Array<boolean>(questions.length).fill(false)

    function updateProgress(): void {
        const answeredCount = answeredState.filter(Boolean).length
        const percentage = (answeredCount / questions.length) * 100
        progressBar.style.width = `${percentage}%`
        progressBadge.textContent = `${answeredCount} / ${questions.length}`

        // Enable submit if all required questions are answered
        const allRequiredAnswered = questions.every((q, i) => !q.isRequired || answeredState[i])
        if (allRequiredAnswered) {
            submitBtn.disabled = false
            submitBtn.style.backgroundColor = 'var(--color-primary)'
            submitBtn.style.color = 'var(--color-text-on-primary)'
            submitBtn.style.cursor = 'pointer'
            submitBtn.style.boxShadow = 'var(--shadow-md)'
        } else {
            submitBtn.disabled = true
            submitBtn.style.backgroundColor = 'var(--color-disabled-bg)'
            submitBtn.style.color = 'var(--color-disabled-text)'
            submitBtn.style.cursor = 'not-allowed'
            submitBtn.style.boxShadow = 'none'
        }
    }

    // Wire up answer callbacks for progressive unlock
    components.forEach((component, index) => {
        component.onAnswer(() => {
            const answer = component.getAnswer()
            const hasAnswer = answer !== null && answer !== ''
            answeredState[index] = hasAnswer

            // Unlock the next question when current is answered
            if (hasAnswer && index + 1 < components.length) {
                components[index + 1].unlock()
            }

            updateProgress()
        })
    })

    // Scroll navigation
    function scrollToQuestion(index: number): void {
        const el = questionsContainer.querySelector<HTMLElement>(`[data-question-index="${index}"]`)
        if (el) {
            el.scrollIntoView({ behavior: 'smooth', block: 'center' })
            currentQuestionIndex = index
            scrollNav?.update(currentQuestionIndex, questions.length)
        }
    }

    scrollNav = renderScrollNav((direction) => {
        if (direction === 'up' && currentQuestionIndex > 0) {
            scrollToQuestion(currentQuestionIndex - 1)
        } else if (direction === 'down' && currentQuestionIndex < questions.length - 1) {
            scrollToQuestion(currentQuestionIndex + 1)
        }
    })
    scrollNav.update(0, questions.length)

    // Submit handler
    submitBtn.addEventListener('click', async () => {
        // Validate all questions
        let allValid = true
        let firstInvalidIndex = -1

        for (let i = 0; i < components.length; i++) {
            if (!components[i].validate()) {
                allValid = false
                if (firstInvalidIndex === -1) firstInvalidIndex = i
            }
        }

        if (!allValid) {
            scrollToQuestion(firstInvalidIndex)
            return
        }

        // Build response
        const answers: ResponseAnswer[] = questions.map((question, index) => {
            const answer = components[index].getAnswer()
            if (question.type === QuestionType.SingleChoice) {
                return { questionId: question.id, selectedOptionId: answer as number }
            }
            return { questionId: question.id, openTextValue: answer as string }
        })

        submitBtn.disabled = true
        submitBtn.textContent = 'Submitting...'

        try {
            await submitResponse({ projectId: project.id, answers })
            localStorage.setItem(`survey-completed-${project.id}`, 'true')
            scrollNav?.destroy()
            navigate('completed')
        } catch {
            submitBtn.disabled = false
            submitBtn.textContent = 'Submit Survey'
            alert('Failed to submit survey. Please try again.')
        }
    })
}

