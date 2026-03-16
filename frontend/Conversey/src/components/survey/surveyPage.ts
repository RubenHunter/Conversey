import type { RouteParams } from '../../utils/router.ts'
import { navigate } from '../../utils/router.ts'
import { getProject } from '../../services/projectService.ts'
import { getQuestions, submitAnswers } from '../../services/surveyService.ts'
import { QuestionType } from '../../models/question.ts'
import type { ResponseAnswer } from '../../models/response.ts'
import { renderSingleChoiceQuestion } from './singleChoiceQuestion.ts'
import type { QuestionComponent } from './singleChoiceQuestion.ts'
import { renderOpenTextQuestion } from './openTextQuestion.ts'
import { renderScrollNav } from '../scrollNav.ts'
import type { ScrollNav } from '../scrollNav.ts'

function formatOrganizationName(organizationSlug: string): string {
    return organizationSlug
        .split('-')
        .filter((part) => part.length > 0)
        .map((part) => (part.length <= 3 ? part.toUpperCase() : `${part.charAt(0).toUpperCase()}${part.slice(1)}`))
        .join(' ')
}

function getOrganizationBadge(organizationName: string, organizationSlug: string): string {
    const clean = organizationName.replace(/[^a-z0-9]/gi, '') || organizationSlug.replace(/[^a-z0-9]/gi, '')
    return clean.slice(0, 3).toUpperCase() || 'ORG'
}

export async function renderSurveyPage(container: HTMLElement, params: RouteParams): Promise<void> {
    const project = await getProject(params.organizationSlug, params.projectSlug)
    const completedKey = `survey-completed-${project.id}`

    if (localStorage.getItem(completedKey) === 'true') {
        container.innerHTML = `
            <div class="survey-redirect-wrap screen-height">
                <div class="survey-redirect-card">
                    <div class="survey-redirect-check">✓</div>
                    <h2>Survey already completed</h2>
                    <p>Redirecting you to ideas...</p>
                    <div class="survey-confetti" aria-hidden="true"></div>
                </div>
            </div>
        `

        const redirectTimer = window.setTimeout(() => {
            void navigate('ideas', { replace: true })
        }, 3200)

        window.addEventListener(
            'app:before-navigate',
            () => {
                window.clearTimeout(redirectTimer)
            },
            { once: true },
        )

        return
    }

    const questions = await getQuestions(params.organizationSlug, params.projectSlug)

    const organizationName = project.organizationName?.trim() || formatOrganizationName(project.organizationSlug)
    const organizationBadge = getOrganizationBadge(organizationName, project.organizationSlug)

    let currentQuestionIndex = -1 // Start at -1 to indicate we're at landing page section, not at any question yet
    let scrollNav: ScrollNav | null = null
    let isUserScroll = false // Track whether the scroll was from user or from programmatic navigation
    let scrollTimeoutId: number | null = null // Track pending scroll timeout to cancel if needed

    container.innerHTML = `
        <div class="survey-shell" id="survey-shell">
            <div class="survey-topbar">
                <div class="survey-topbar-left">
                    <div class="survey-topbar-logo"><img src="/Conversey_logo.png" alt="Conversey" /></div>
                    <div class="survey-topbar-logo-title">CONVERSEY</div>
                </div>
                <div class="survey-topbar-brand">
                    <div class="survey-topbar-logo-badge">${organizationBadge}</div>
                    <div class="survey-topbar-name">${organizationName}</div>
                </div>
            </div>

            <section class="survey-hero" id="survey-hero">
                <img src="${project.imageUrl}" alt="${project.title}" class="survey-hero-image" />
                <div class="survey-hero-content">
                    <h1 class="survey-hero-title">${project.title}</h1>
                    <p class="survey-hero-description" id="survey-hero-description">${project.description}</p>
                    <p class="mt-3 text-xs uppercase tracking-[0.18em] opacity-80">Loaded from backend API</p>
                </div>
            </section>

            <div class="survey-header" id="survey-header">
                <div class="survey-header-content">
                    <h2 class="survey-title">${project.title}</h2>
                    <div class="survey-progress-container">
                        <div class="survey-progress-bar">
                            <div class="survey-progress-fill" id="progress-bar"></div>
                        </div>
                        <span class="survey-progress-badge" id="progress-badge">0 / ${questions.length}</span>
                    </div>
                </div>
            </div>

            <div class="survey-content">
                <div id="questions-container"></div>
                <div class="survey-action-bar" id="survey-action-bar">
                    <button id="btn-stop-early" class="survey-stop-early-btn" aria-label="Stop survey early">Stop Survey Early</button>
                    <button id="btn-submit" class="survey-submit-btn" disabled>Submit Survey</button>
                </div>
            </div>
        </div>
    `

    const surveyShell = container.querySelector<HTMLDivElement>('#survey-shell')!
    const headerEl = container.querySelector<HTMLDivElement>('#survey-header')!
    const questionsContainer = container.querySelector<HTMLDivElement>('#questions-container')!
    const submitBtn = container.querySelector<HTMLButtonElement>('#btn-submit')!
    const stopEarlyBtn = container.querySelector<HTMLButtonElement>('#btn-stop-early')!
    const actionBar = container.querySelector<HTMLDivElement>('#survey-action-bar')!
    const progressBar = container.querySelector<HTMLDivElement>('#progress-bar')!
    const progressBadge = container.querySelector<HTMLSpanElement>('#progress-badge')!

    const components: QuestionComponent[] = questions.map((question, index) => {
        const component =
            question.type === QuestionType.SingleChoice
                ? renderSingleChoiceQuestion(question, index)
                : renderOpenTextQuestion(question, index)

        questionsContainer.appendChild(component.getElement())

        if (index > 0) {
            component.lock()
        }

        return component
    })

    const answeredState = new Array<boolean>(questions.length).fill(false)

    function updateProgress(): void {
        const answeredCount = answeredState.filter(Boolean).length
        const percentage = (answeredCount / questions.length) * 100
        progressBar.style.width = `${percentage}%`
        progressBadge.textContent = `${answeredCount} / ${questions.length}`

        const allRequiredAnswered = questions.every((q, i) => !q.isRequired || answeredState[i])
        const isReady = allRequiredAnswered && answeredCount === questions.length

        submitBtn.disabled = !isReady
        actionBar.classList.toggle('survey-ready', isReady)
    }

    components.forEach((component, index) => {
        component.onAnswer(() => {
            const answer = component.getAnswer()
            const hasAnswer = answer !== null && answer !== ''
            answeredState[index] = hasAnswer

            if (index + 1 < components.length) {
                const nextQuestion = components[index + 1]
                const nextIndex = index + 1

                if (!hasAnswer && !answeredState[nextIndex]) {
                    nextQuestion.lock()
                } else if (hasAnswer) {
                    nextQuestion.unlock()
                }
            }

            updateProgress()
        })
    })

    function scrollToQuestion(index: number): void {
        const el = questionsContainer.querySelector<HTMLElement>(`[data-question-index="${index}"]`)
        if (!el) return

        // Cancel any pending scroll timeout from previous navigation
        if (scrollTimeoutId !== null) {
            clearTimeout(scrollTimeoutId)
            scrollTimeoutId = null
        }

        // If navigating to question 0, allow hero to expand, otherwise keep it collapsed
        if (index !== 0) {
            surveyShell.classList.add('survey-hero-collapsed')
        } else {
            surveyShell.classList.remove('survey-hero-collapsed')
        }

        // Calculate scroll position accounting for header
        const headerHeight = headerEl.getBoundingClientRect().height
        const elementTop = el.getBoundingClientRect().top + window.scrollY
        const offset = headerHeight + 80 // Increased padding to account for progress bar

        // Set flag to indicate this is a programmatic scroll
        isUserScroll = false

        window.scrollTo({
            top: elementTop - offset,
            behavior: 'smooth'
        })

        currentQuestionIndex = index
        scrollNav?.update(currentQuestionIndex, questions.length)

        // Re-enable user scroll detection after scroll completes
        scrollTimeoutId = window.setTimeout(() => {
            isUserScroll = true
            scrollTimeoutId = null
        }, 700)
    }

    scrollNav = renderScrollNav((direction) => {
        if (direction === 'up' && currentQuestionIndex > 0) {
            scrollToQuestion(currentQuestionIndex - 1)
        } else if (direction === 'down' && currentQuestionIndex < questions.length - 1) {
            scrollToQuestion(currentQuestionIndex + 1)
        }
    })
    scrollNav.update(currentQuestionIndex, questions.length)

    function updateCurrentQuestionFromScroll(): void {
        // Only update from scroll if this was a user scroll, not programmatic navigation
        if (!isUserScroll) return

        const elements = questionsContainer.querySelectorAll<HTMLElement>('[data-question-index]')
        const headerBottom = headerEl.getBoundingClientRect().bottom

        let closestIndex = -1
        let closestDistance = Number.POSITIVE_INFINITY

        elements.forEach((el) => {
            const rect = el.getBoundingClientRect()
            // Calculate distance from element top to position just below header
            const targetY = headerBottom + 100 // Consider element "current" when it's ~100px below header
            const distance = Math.abs(rect.top - targetY)

            // Only consider elements that are visible and not scrolled past
            if (rect.top < window.innerHeight && rect.bottom > headerBottom) {
                if (distance < closestDistance) {
                    closestDistance = distance
                    closestIndex = Number(el.getAttribute('data-question-index'))
                }
            }
        })

        const firstQuestion = questionsContainer.querySelector<HTMLElement>('[data-question-index="0"]')
        if (!firstQuestion) return

        const firstTop = firstQuestion.getBoundingClientRect().top
        const shouldCollapseHero = firstTop <= headerBottom + 8
        surveyShell.classList.toggle('survey-hero-collapsed', shouldCollapseHero)

        // If no question is visible near the header (scrolled back to hero), set to -1
        if (closestIndex === -1) {
            // Check if first question is above header (meaning we're at hero)
            if (firstTop <= headerBottom) {
                closestIndex = -1
            } else {
                // If we somehow have no closest index but first question is visible, use it
                closestIndex = 0
            }
        }

        // Update only if the index has actually changed to avoid unnecessary updates
        if (currentQuestionIndex !== closestIndex) {
            currentQuestionIndex = closestIndex
            scrollNav?.update(currentQuestionIndex, questions.length)
        }
    }

    function cleanupSurveyPage(): void {
        window.removeEventListener('scroll', updateCurrentQuestionFromScroll)
        window.removeEventListener('app:before-navigate', cleanupSurveyPage as EventListener)
        if (scrollTimeoutId !== null) {
            clearTimeout(scrollTimeoutId)
            scrollTimeoutId = null
        }
        scrollNav?.destroy()
        scrollNav = null
    }

    window.addEventListener('scroll', updateCurrentQuestionFromScroll, { passive: true })
    window.addEventListener('app:before-navigate', cleanupSurveyPage as EventListener)
    // Don't call updateCurrentQuestionFromScroll() on initial load - keep currentQuestionIndex at -1

    stopEarlyBtn.addEventListener('click', () => {
        void navigate('ideas')
    })

    submitBtn.addEventListener('click', async () => {
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

        const answers: ResponseAnswer[] = questions.map((question, index) => {
            const answer = components[index].getAnswer()
            if (question.type === QuestionType.SingleChoice) {
                const selectedOptionId = answer as number
                return { questionId: question.id, selectedOptionId, value: selectedOptionId }
            }
            const openTextValue = answer as string
            return { questionId: question.id, openTextValue, value: openTextValue }
        })

        submitBtn.disabled = true
        submitBtn.textContent = 'Submitting...'

        try {
            await submitAnswers(params.organizationSlug, params.projectSlug, { projectId: project.id, answers })
            localStorage.setItem(completedKey, 'true')
            cleanupSurveyPage()
            await navigate('completed')
        } catch {
            submitBtn.disabled = false
            submitBtn.textContent = 'Submit Survey'
            alert('Failed to submit survey. Please try again.')
        }
    })
}
