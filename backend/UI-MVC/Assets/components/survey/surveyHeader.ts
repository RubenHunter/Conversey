import { getOrganizationBranding } from '../organizationBranding'

interface RenderSurveyHeaderParams {
    organizationName: string
    organizationSlug: string
}

export function renderSurveyHeader({ organizationName, organizationSlug }: RenderSurveyHeaderParams): string {
    const { badge, displayName } = getOrganizationBranding(organizationName, organizationSlug)

    return `
        <div class="survey-topbar">
            <div class="survey-topbar-left">
                <div class="survey-topbar-logo"><img src="/Conversey_logo.png" alt="Conversey" /></div>
                <div class="survey-topbar-logo-title">CONVERSEY</div>
            </div>
            <div class="survey-topbar-brand">
                <div class="survey-topbar-logo-badge">${badge}</div>
                <div class="survey-topbar-name">${displayName}</div>
            </div>
        </div>
    `
}

interface CreateSurveyHeaderControllerParams {
    root: HTMLElement
}

interface SurveyHeaderController {
    updateProgress(current: number, total: number): void
    getProgressBar(): HTMLElement | null
}

export function createSurveyHeaderController({
    root,
}: CreateSurveyHeaderControllerParams): SurveyHeaderController {
    const progressBar = root.querySelector<HTMLElement>('#progress-bar')
    const progressBadge = root.querySelector<HTMLElement>('#progress-badge')

    function updateProgress(current: number, total: number): void {
        if (progressBar) {
            const percentage = total > 0 ? (current / total) * 100 : 0
            progressBar.style.width = `${percentage}%`
        }
        if (progressBadge) {
            progressBadge.textContent = `${current} / ${total}`
        }
    }

    return {
        updateProgress,
        getProgressBar: () => progressBar || null,
    }
}

