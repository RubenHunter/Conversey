import type { RouteParams } from '../utils/router.ts'
import { navigate } from '../utils/router.ts'
import { getProject } from '../services/projectService.ts'

function isSurveyCompleted(projectId: number): boolean {
    return localStorage.getItem(`survey-completed-${projectId}`) === 'true'
}

export async function renderLandingPage(container: HTMLElement, params: RouteParams): Promise<void> {
    const project = await getProject(params.organizationSlug, params.projectSlug)

    if (isSurveyCompleted(project.id)) {
        container.innerHTML = `
            <div class="flex flex-col items-center justify-center min-h-dvh px-6 py-10">
                <div class="w-16 h-16 rounded-full flex items-center justify-center mb-6"
                     style="background-color: var(--color-success-bg);">
                    <svg class="w-8 h-8" style="color: var(--color-success);" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7"/>
                    </svg>
                </div>
                <h1 class="text-2xl font-bold text-center mb-3" style="color: var(--color-text);">
                    Survey Already Completed
                </h1>
                <p class="text-center mb-8" style="color: var(--color-text-secondary); font-size: var(--font-size-base);">
                    You have already filled in this survey. Thank you for your participation!
                </p>
                <button
                    id="btn-ideas"
                    class="w-full py-3 px-6 rounded-xl font-semibold text-base transition-all"
                    style="background-color: var(--color-secondary); color: var(--color-text-on-primary); border: none; cursor: pointer;">
                    Continue to Ideas
                </button>
            </div>
        `
        return
    }

    container.innerHTML = `
        <div class="relative flex flex-col min-h-dvh">
            <!-- Fullscreen background image -->
            <img
                id="project-image"
                src="${project.imageUrl}"
                alt="${project.title}"
                class="absolute inset-0 w-full h-full object-cover"
            />
            <!-- Dark overlay for readability -->
            <div class="absolute inset-0"
                 style="background: linear-gradient(to top, rgba(0,0,0,0.75) 0%, rgba(0,0,0,0.3) 50%, rgba(0,0,0,0.1) 100%);">
            </div>

            <!-- Content overlay -->
            <div class="relative flex flex-col flex-1 px-6 pt-10">
                <!-- Title & description: positioned just above vertical center -->
                <div class="text-left" style="margin-top: 38dvh;">
                    <h1 class="font-bold leading-tight mb-2"
                        style="font-size: 2.25rem; color: #FFFFFF;">
                        ${project.title}
                    </h1>
                    <p class="leading-relaxed"
                       style="font-size: var(--font-size-sm); color: rgba(255,255,255,0.75); max-width: 85%;">
                        ${project.description}
                    </p>
                </div>

                <!-- Start button: floating above bottom -->
                <div class="flex justify-center" style="margin-top: auto; padding-bottom: 15dvh;">
                    <button
                        id="btn-start-survey"
                        class="font-bold transition-all active:scale-[0.95]"
                        style="padding: 1.1rem 3rem; font-size: 1.2rem; background-color: var(--color-primary); color: #FFFFFF; border: none; cursor: pointer; border-radius: var(--radius-full); box-shadow: 0 8px 30px rgba(108, 92, 231, 0.5), 0 2px 8px rgba(0,0,0,0.3);">
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

