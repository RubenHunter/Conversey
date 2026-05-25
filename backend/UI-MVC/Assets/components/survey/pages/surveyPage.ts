import {getProject} from '../../../services/projectService'
import {getQuestions, submitAnswers} from '../../../services/surveyService'
import type {QuestionAnswer, QuestionComponent} from '../components/singleChoiceQuestion'
import {renderSingleChoiceQuestion} from '../components/singleChoiceQuestion'
import {renderMultipleChoiceQuestion} from '../components/multipleChoiceQuestion'
import {renderOpenTextQuestion} from '../components/openTextQuestion'
import {renderScaleQuestion} from '../components/scaleQuestion'
import type {ScrollNav} from '../../shared/scrollNav'
import {clearSurveyProgress, loadSurveyProgress, saveSurveyProgress} from '../../../services/surveyProgressService'
import {renderSurveyHeader, createSurveyHeaderController} from '../components/surveyHeader'
import {navigate, ProjectContext, render} from "../../../main";
import {InteractionType} from '../../../models/project'
import {showLayoutPicker} from '../components/layoutPicker'
import {getSurveyStrings} from '../../../i18n/survey'
import {renderScrollNav} from '../../shared/scrollNav'
import {hasAnswer} from '../../chat/utils/chatHelpers'
import {FixedQuestion, OpenQuestion, QuestionType, RangeQuestion} from "../../../models/question.ts";
import {ResponseAnswer} from "../../../models/response.ts";

const sessionLayoutCache = new Map<string, typeof InteractionType.Chat | typeof InteractionType.VerticalScroll>()

export async function renderSurveyPage(container: HTMLElement, params: ProjectContext): Promise<void> {
    const t = getSurveyStrings()
    const project = await getProject(params.organizationSlug, params.projectSlug)
    const projectSlugKey = params.projectSlug
    const completedKey = `survey-completed-${projectSlugKey}`

    if (localStorage.getItem(completedKey) === 'true') {
        clearSurveyProgress(projectSlugKey)
        container.innerHTML = `
            <div class="survey-redirect-wrap screen-height">
                <div class="survey-redirect-card">
                    <div class="survey-redirect-check">✓</div>
                    <h2>${t.surveyAlreadyCompleted}</h2>
                    <p>${t.redirectingToIdeas}</p>
                    <div class="survey-confetti" aria-hidden="true"></div>
                </div>
            </div>
        `

        const redirectTimer = window.setTimeout(() => {
            void navigate('ideas')
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

    const organizationName = project.organizationName?.trim() || project.organizationSlug
    const headerHTML = renderSurveyHeader({ organizationName, organizationSlug: project.organizationSlug })

    let currentQuestionIndex = -1 // Start at -1 to indicate we're at landing page section, not at any question yet
    let scrollNav: ScrollNav | null = null
    let isUserScroll = false // Track whether the scroll was from user or from programmatic navigation
    let scrollTimeoutId: number | null = null // Track pending scroll timeout to cancel if needed

    container.innerHTML = `
        <div class="survey-shell" id="survey-shell">
            ${headerHTML}

            <section class="survey-hero" id="survey-hero">
                <img src="${project.imageUrl}" alt="${project.title}" class="survey-hero-image" />
                <div class="survey-hero-content lg:top-[28%]">
                    <h1 class="survey-hero-title">${project.title}</h1>
                    <p class="survey-hero-description" id="survey-hero-description">${project.description}</p>
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
                    <button id="btn-submit" class="survey-submit-btn">${t.submitSurvey}</button>
                </div>
            </div>
        </div>
    `

    const surveyShell = container.querySelector<HTMLDivElement>('#survey-shell')!
    const headerEl = container.querySelector<HTMLDivElement>('#survey-header')!
    const questionsContainer = container.querySelector<HTMLDivElement>('#questions-container')!
    const submitBtn = container.querySelector<HTMLButtonElement>('#btn-submit')!
    const actionBar = container.querySelector<HTMLDivElement>('#survey-action-bar')!

    const headerController = createSurveyHeaderController({ root: container })

    function hasRequiredQuestionBefore(index: number): boolean {
        for (let i = index - 1; i >= 0; i--) {
            if (questions[i].required) return true
        }
        return false
    }

    function hasUnansweredRequiredBefore(index: number): boolean {
        for (let i = index - 1; i >= 0; i--) {
            if (questions[i].required && !answeredState[i]) return true
        }
        return false
    }

    const components: QuestionComponent[] = questions.map((question, index) => {
        const component =
            question.type === QuestionType.SingleChoice
                ? renderSingleChoiceQuestion(question as FixedQuestion, index)
                : question.type === QuestionType.MultipleChoice
                    ? renderMultipleChoiceQuestion(question as FixedQuestion, index)
                : question.type === QuestionType.Scale
                    ? renderScaleQuestion(question as RangeQuestion, index)
                    : renderOpenTextQuestion(question as OpenQuestion, index)

        questionsContainer.appendChild(component.getElement())

        // Lock by required-gate: a question is blocked only while the last required question before it is unanswered.
        if (hasRequiredQuestionBefore(index)) {
            component.lock()
        }

        return component
    })

    const answeredState = new Array<boolean>(questions.length).fill(false)

    function syncAnsweredState(): void {
        components.forEach((component, index) => {
            answeredState[index] = hasAnswer(component.getAnswer())
        })
    }

    function syncQuestionLocks(): void {
        components.forEach((component, index) => {
            if (index === 0) {
                component.unlock()
                return
            }

            const shouldLockByRequiredGate = hasUnansweredRequiredBefore(index)
            if (shouldLockByRequiredGate) {
                component.lock()
            } else {
                component.unlock()
            }
        })
    }

    function collectAnswersByQuestionId(): Map<number, QuestionAnswer> {
        return new Map<number, QuestionAnswer>(
            questions.map((question, index) => [question.id!, components[index].getAnswer()] as const),
        )
    }

    function persistProgress(): void {
        saveSurveyProgress(projectSlugKey, questions, currentQuestionIndex, collectAnswersByQuestionId())
    }

    // Auto-scroll textarea into view on mobile when focused
    document.querySelectorAll('.survey-textarea').forEach(textarea => {
        textarea.addEventListener('focus', () => {
            setTimeout(() => {
                (textarea as HTMLElement).scrollIntoView({ behavior: 'smooth', block: 'center' })
            }, 300)
        })
    })

    function updateProgress(): void {
        const answeredCount = answeredState.filter(Boolean).length
        headerController.updateProgress(answeredCount, questions.length)

        const isReady = questions.every((q, i) => !q.required || answeredState[i])
        
        actionBar.classList.toggle('survey-ready', isReady)
    }

    function syncSurveyState(): void {
        syncAnsweredState()
        syncQuestionLocks()
        updateProgress()
    }

    components.forEach((component) => {
        component.onAnswer(() => {
            syncSurveyState()
            persistProgress()
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
        syncQuestionLocks()
        scrollNav?.update(currentQuestionIndex, questions.length)
        persistProgress()

        // Re-enable user scroll detection after scroll completes
        scrollTimeoutId = window.setTimeout(() => {
            isUserScroll = true
            scrollTimeoutId = null
        }, 700)
    }

    scrollNav = renderScrollNav((direction) => {
        // If at hero (index = -1), first down click goes to question 0
        if (currentQuestionIndex === -1 && direction === 'down' && questions.length > 0) {
            scrollToQuestion(0)
            return
        }
        if (direction === 'up' && currentQuestionIndex > 0) {
            scrollToQuestion(currentQuestionIndex - 1)
        } else if (direction === 'down' && currentQuestionIndex >= 0 && currentQuestionIndex < questions.length - 1) {
            scrollToQuestion(currentQuestionIndex + 1)
        }
    })
    scrollNav.update(currentQuestionIndex, questions.length)

    const savedProgress = loadSurveyProgress(projectSlugKey, questions)
    if (savedProgress) {
        components.forEach((component, index) => {
            const questionId = questions[index].id
            if (!savedProgress.answersByQuestionId.has(questionId!)) {
                return
            }

            const savedAnswer = savedProgress.answersByQuestionId.get(questionId!)
            component.setAnswer(savedAnswer ?? null)
        })

        currentQuestionIndex = Math.min(
            Math.max(savedProgress.currentQuestionIndex, -1),
            questions.length - 1,
        )
    }

    syncSurveyState()

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
            syncQuestionLocks()
            scrollNav?.update(currentQuestionIndex, questions.length)
            persistProgress()
        }
    }

    function cleanupSurveyPage(): void {
        window.removeEventListener('scroll', updateCurrentQuestionFromScroll)
        window.removeEventListener('app:before-navigate', cleanupSurveyPage as EventListener)
        if (scrollTimeoutId !== null) {
            clearTimeout(scrollTimeoutId)
            scrollTimeoutId = null
        }
        components.forEach(c => c.destroy?.())
        scrollNav?.destroy()
        scrollNav = null
    }

    window.addEventListener('scroll', updateCurrentQuestionFromScroll, { passive: true })
    window.addEventListener('app:before-navigate', cleanupSurveyPage as EventListener)
    // Keep currentQuestionIndex at -1 initially (hero section)
    // updateCurrentQuestionFromScroll() is called by scroll events
    isUserScroll = true

    if (currentQuestionIndex >= 0) {
        requestAnimationFrame(() => {
            scrollToQuestion(currentQuestionIndex)
        })
    } else {
        persistProgress()
    }

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
                if (selectedOptionId == null) return []
                return { questionId: question.id!, selectedOptionId }
            }
            if (question.type === QuestionType.MultipleChoice) {
                const selectedOptionIds = Array.isArray(answer) ? answer : []
                if (selectedOptionIds.length === 0) return []
                return selectedOptionIds.map((selectedOptionId) => ({
                    questionId: question.id!,
                    selectedOptionId,
                }))
            }
            if (question.type === QuestionType.Scale) {
                const scaleValue = answer as number
                if (scaleValue == null) return []
                return { questionId: question.id!, selectedOptionId: scaleValue }
            }
            const openTextValue = answer as string
            if (openTextValue == null || openTextValue === '') return []
            return { questionId: question.id!, openTextValue }
        }).flat()
        
        submitBtn.textContent = t.submitting
        
        try {
            await submitAnswers(params.organizationSlug, params.projectSlug, { projectId: params.projectSlug, answers })
            localStorage.setItem(completedKey, 'true')
            clearSurveyProgress(params.projectSlug)
            cleanupSurveyPage()
            navigate("completed");
        } catch {
            submitBtn.textContent = t.submitSurvey
            alert(t.submitFailed)
        }
    })
}


render(async (container, params) => {
    const project = await getProject(params.organizationSlug, params.projectSlug)

    if (project.interactionType === InteractionType.Chat) {
        const { renderChatSurveyPage } = await import("../../chat/pages/chatSurveyPage")
        await renderChatSurveyPage(container, params, project)
        return
    }

    if (project.interactionType === InteractionType.UserDefined) {
        const layoutKey = `survey-layout-${params.projectSlug}`
        const savedLayout = sessionLayoutCache.get(layoutKey) || localStorage.getItem(layoutKey)

        if (savedLayout === InteractionType.Chat) {
            const { renderChatSurveyPage } = await import('../../chat/pages/chatSurveyPage')
            await renderChatSurveyPage(container, params, project)
            return
        }

        if (savedLayout === InteractionType.VerticalScroll) {
            await renderSurveyPage(container, params)
            return
        }

        const organizationName = project.organizationName?.trim() || project.organizationSlug
        const choice = await showLayoutPicker({
            container,
            storageKey: layoutKey,
            organizationName,
            organizationSlug: project.organizationSlug,
        })
        sessionLayoutCache.set(layoutKey, choice)
        if (choice === InteractionType.Chat) {
            const { renderChatSurveyPage } = await import('../../chat/pages/chatSurveyPage')
            await renderChatSurveyPage(container, params, project)
        } else {
            await renderSurveyPage(container, params)
        }
        return
    }

    await renderSurveyPage(container, params)
})
