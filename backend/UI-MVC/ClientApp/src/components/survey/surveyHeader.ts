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

interface RenderSurveyHeaderParams {
    organizationName: string
    organizationSlug: string
}

export function renderSurveyHeader({ organizationName, organizationSlug }: RenderSurveyHeaderParams): string {
    const badge = getOrganizationBadge(organizationName, organizationSlug)
    const displayName = organizationName.trim() || formatOrganizationName(organizationSlug)

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

