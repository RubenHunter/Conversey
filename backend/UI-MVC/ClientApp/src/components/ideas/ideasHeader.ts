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

interface RenderIdeasHeaderParams {
    organizationName: string
    organizationSlug: string
}

export function renderIdeasHeader({ organizationName, organizationSlug }: RenderIdeasHeaderParams): string {
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

export function formatOrgName(organizationSlug: string): string {
    return formatOrganizationName(organizationSlug)
}

export function getOrgBadge(organizationName: string, organizationSlug: string): string {
    return getOrganizationBadge(organizationName, organizationSlug)
}

