import { getOrganizationBranding } from '../../shared/organizationBranding'

interface RenderIdeasHeaderParams {
    organizationName: string
    organizationSlug: string
}

export function renderIdeasHeader({ organizationName, organizationSlug }: RenderIdeasHeaderParams): string {
    const { badge, displayName } = getOrganizationBranding(organizationName, organizationSlug)

    return `
        <div class="survey-topbar">
            <div class="survey-topbar-left">
                <div class="survey-topbar-logo"><img src="/images/Conversey_logo.png" alt="Conversey" /></div>
                <div class="survey-topbar-logo-title">CONVERSEY</div>
            </div>
            <div class="survey-topbar-brand">
                <div class="survey-topbar-logo-badge">${badge}</div>
                <div class="survey-topbar-name">${displayName}</div>
            </div>
        </div>
    `
}

