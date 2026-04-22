import type { RouteParams } from '../utils/router.ts'
import { navigate } from '../utils/router.ts'
import { getProject } from '../services/projectService.ts'
import { formatOrganizationName, getOrganizationBadge } from '../utils/project.ts'

function isSurveyCompleted(projectId: number): boolean {
    return localStorage.getItem(`survey-completed-${projectId}`) === 'true'
}

// Debug: Clear survey completion flag (remove in production)
function clearSurveyCompletion(): void {
    const keys = Object.keys(localStorage).filter(
        (key) => key.startsWith('survey-completed-') || key.startsWith('conversey-survey-progress-'),
    )
    keys.forEach((key) => localStorage.removeItem(key))
    console.log('Survey completion/progress flags cleared. Reload the page to retake the survey.')
}

// Expose to window for easy testing
if (typeof window !== 'undefined') {
    ;(window as any).clearSurvey = clearSurveyCompletion
}

export async function renderLandingPage(container: HTMLElement, params: RouteParams): Promise<void> {
    const project = await getProject(params.organizationSlug, params.projectSlug)
    const organizationName = project.organizationName?.trim() || formatOrganizationName(project.organizationSlug)
    const organizationBadge = getOrganizationBadge(organizationName, project.organizationSlug)

    if (isSurveyCompleted(project.id)) {
        container.innerHTML = `
            <div class="flex flex-col items-center justify-center screen-height px-6 py-10">
                <div class="w-16 h-16 rounded-full flex items-center justify-center mb-6 completed-icon-badge">
                    <svg class="w-8 h-8 completed-icon" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7"/>
                    </svg>
                </div>
                <h1 class="text-2xl font-bold text-center mb-3 completed-heading">
                    Survey Already Completed
                </h1>
                <p class="text-center mb-8 completed-text">
                    You have already filled in this survey. Thank you for your participation!
                </p>
                <button
                    id="btn-ideas"
                    class="w-full py-3 px-6 rounded-xl font-semibold text-base transition-all completed-cta">
                    Continue to Ideas
                </button>
            </div>
        `

        const ideasButton = container.querySelector<HTMLButtonElement>('#btn-ideas')
        ideasButton?.addEventListener('click', () => {
            void navigate('ideas')
        })
        return
    }

    container.innerHTML = `
        <div class="relative flex flex-col screen-height-fixed overflow-hidden">
            <img
                id="project-image"
                src="${project.imageUrl}"
                alt="${project.title}"
                class="absolute inset-0 w-full h-full object-cover landing-bg-image"
            />

            <div class="relative flex flex-col justify-between flex-1 px-6 pt-10 pb-12">
                <div class="landing-topbar" aria-label="Survey branding">
                    <div class="landing-logo">Conversey</div>
                    <div class="landing-owner-brand" aria-label="Organization branding">
                        <div class="landing-owner-logo" aria-hidden="true">${organizationBadge}</div>
                        <div class="landing-owner-name">${organizationName}</div>
                    </div>
                </div>

                <div class="absolute overflow-hidden landing-circle-wrap">
                    <div class="landing-circle"></div>
                </div>
                <div class="absolute overflow-hidden landing-accent-circle-wrap" aria-hidden="true">
                    <div class="landing-accent-circle"></div>
                </div>

                <div class="text-left relative landing-copy">
                    <h1 class="font-bold leading-tight mb-4 landing-title">
                        ${project.title}
                    </h1>
                    <p class="leading-relaxed landing-description">
                        ${project.description}
                    </p>
                </div>

                <div class="flex justify-center relative landing-start-wrap">
                    <button
                        id="btn-start-survey"
                        class="font-bold transition-all active:scale-[0.95] landing-start-btn">
                        Start Survey
                    </button>
                </div>
            </div>
        </div>
    `

    const startButton = container.querySelector<HTMLButtonElement>('#btn-start-survey')
    startButton?.addEventListener('click', () => {
        navigate('survey')
    })
}
