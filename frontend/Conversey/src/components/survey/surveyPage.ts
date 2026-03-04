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
    const organizationName = params.organizationSlug
        .split('-')
        .map((part) => (part.length <= 3 ? part.toUpperCase() : `${part.charAt(0).toUpperCase()}${part.slice(1)}`))
        .join(' ')

    let currentQuestionIndex = 0
    let scrollNav: ScrollNav | null = null

    // Build page structure with sticky topbar
    container.innerHTML = `
        <div class="survey-topbar">
            <div class="survey-topbar-logo">Conversey</div>
            <div class="survey-topbar-brand">
                <div class="survey-topbar-logo-badge">AXA</div>
                <div class="survey-topbar-name">${organizationName}</div>
            </div>
        </div>

        <div class="survey-header">
            <div class="survey-header-content">
                <h2 class="survey-title">${project.title}</h2>
                <div class="survey-progress-container">
                    <div class="survey-progress-bar">
                        <div class="survey-progress-fill" id="progress-bar" style="width: 0%"></div>
                    </div>
                    <span class="survey-progress-badge" id="progress-badge">0 / ${questions.length}</span>
                </div>
            </div>
        </div>

        <div class="survey-content">
            <div id="questions-container"></div>
            <button
                id="btn-submit"
                class="survey-submit-btn"
                disabled
            >
                Submit Survey
            </button>
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
        if (allRequiredAnswered && answeredCount === questions.length) {
            submitBtn.disabled = false
            submitBtn.classList.remove('disabled')
        } else {
            submitBtn.disabled = true
            submitBtn.classList.add('disabled')
        }
    }

    // Wire up answer callbacks for progressive unlock
    components.forEach((component, index) => {
        component.onAnswer(() => {
            const answer = component.getAnswer()
            const hasAnswer = answer !== null && answer !== ''
            answeredState[index] = hasAnswer

            // Only lock/unlock the immediate next question if we haven't progressed past it
            if (index + 1 < components.length) {
                const nextQuestion = components[index + 1]
                const nextIndex = index + 1

                // Only re-lock if next question is unanswered and we haven't reached beyond it
                if (!hasAnswer && !answeredState[nextIndex]) {
                    nextQuestion.lock()
                } else if (hasAnswer) {
                    nextQuestion.unlock()
                }
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

    // Track current question index based on scroll position
    function updateCurrentQuestionFromScroll(): void {
        const elements = questionsContainer.querySelectorAll<HTMLElement>('[data-question-index]')
        let closestIndex = 0
        let closestDistance = Number.POSITIVE_INFINITY

        elements.forEach((el) => {
            const rect = el.getBoundingClientRect()
            const distance = Math.abs(rect.top + rect.height / 2 - window.innerHeight / 2)
            if (distance < closestDistance) {
                closestDistance = distance
                closestIndex = Number(el.getAttribute('data-question-index'))
            }
        })

        currentQuestionIndex = closestIndex
        scrollNav?.update(currentQuestionIndex, questions.length)
    }

    // Listen for scroll events
    window.addEventListener('scroll', updateCurrentQuestionFromScroll, { passive: true })
    updateCurrentQuestionFromScroll() // Initial update

    function cleanupSurveyPage(): void {
        window.removeEventListener('scroll', updateCurrentQuestionFromScroll)
        window.removeEventListener('app:before-navigate', cleanupSurveyPage as EventListener)
        scrollNav?.destroy()
        scrollNav = null
    }

    window.addEventListener('app:before-navigate', cleanupSurveyPage as EventListener)

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
            cleanupSurveyPage()
            navigate('completed')
        } catch {
            submitBtn.disabled = false
            submitBtn.textContent = 'Submit Survey'
            alert('Failed to submit survey. Please try again.')
        }
    })
}

