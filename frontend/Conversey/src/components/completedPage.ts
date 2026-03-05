import type { RouteParams } from '../utils/router.ts'
import { navigate } from '../utils/router.ts'
import { getProject } from '../services/projectService.ts'

function isSurveyCompleted(projectId: number): boolean {
    return localStorage.getItem(`survey-completed-${projectId}`) === 'true'
}

export async function renderCompletedPage(container: HTMLElement, params: RouteParams): Promise<void> {
    const project = await getProject(params.organizationSlug, params.projectSlug)

    if (!isSurveyCompleted(project.id)) {
        await navigate('survey', { replace: true })
        return
    }

    container.innerHTML = `
        <div class="flex flex-col items-center justify-center screen-height px-6 py-10">
            <div class="w-20 h-20 rounded-full flex items-center justify-center mb-6 completed-icon-badge">
                <svg class="w-10 h-10 completed-icon" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7"/>
                </svg>
            </div>
            <h1 class="text-2xl font-bold text-center mb-3 completed-heading">
                Thank You!
            </h1>
            <p class="text-center mb-8 leading-relaxed completed-text">
                Your survey responses have been submitted successfully. Your input helps us make a real difference.
            </p>
            <button
                id="btn-to-ideas"
                class="w-full py-4 px-6 rounded-xl font-semibold text-lg transition-all active:scale-[0.98] completed-cta">
                Continue to Ideas
            </button>
        </div>
    `

    const ideasBtn = container.querySelector<HTMLButtonElement>('#btn-to-ideas')
    ideasBtn?.addEventListener('click', () => {
        void navigate('ideas')
    })
}
