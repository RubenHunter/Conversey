import { navigate } from '../utils/router.ts'
import { getProject } from '../services/projectService.ts'
import {ProjectContext, render} from "../main";

function isSurveyCompleted(projectId: number): boolean {
    return localStorage.getItem(`survey-completed-${projectId}`) === 'true'
}

async function renderCompletedPage(container: HTMLElement, params: ProjectContext): Promise<void> {
    const project = await getProject(params.organizationSlug, params.projectSlug)

    if (!isSurveyCompleted(project.id)) {
        await navigate('survey', { replace: true })
        return
    }

    container.innerHTML = `
        <div class="survey-redirect-wrap screen-height">
            <div class="survey-redirect-card">
                <div class="survey-redirect-check">✓</div>
                <h2>Thank you for filling out this survey!</h2>
                <p>Could you also help us by sharing your ideas?</p>
                <button id="btn-to-ideas" class="survey-redirect-cta completed-cta">Continue to Ideas</button>
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

    const ideasBtn = container.querySelector<HTMLButtonElement>('#btn-to-ideas')
    ideasBtn?.addEventListener('click', () => {
        void navigate('ideas')
    })
}

render(renderCompletedPage)
