import {navigate, ProjectContext, render} from "../../main";
import { getSurveyStrings } from "../../i18n/survey";

function isSurveyCompleted(projectSlug: string): boolean {
    return localStorage.getItem(`survey-completed-${projectSlug}`) === 'true';
}

async function renderCompletedPage(container: HTMLElement, params: ProjectContext): Promise<void> {
    const t = getSurveyStrings()
    if (!isSurveyCompleted(params.projectSlug)) {
        navigate('survey')
        return
    }

    container.innerHTML = `
        <div class="survey-redirect-wrap screen-height">
            <div class="survey-redirect-card">
                <div class="survey-redirect-check">✓</div>
                <h2>${t.thankYouSurvey}</h2>
                <p>${t.helpShareIdeas}</p>
                <a id="btn-to-ideas" class="survey-redirect-cta completed-cta" href="ideas">${t.continueToIdeas}</a>
                <div class="survey-confetti" aria-hidden="true"></div>
            </div>
        </div>
    `

    const redirectTimer = window.setTimeout(() => {
        navigate("ideas");
    }, 3200)

    window.addEventListener(
        'app:before-navigate',
        () => {
            window.clearTimeout(redirectTimer)
        },
        { once: true },
    )
}

render(renderCompletedPage)
