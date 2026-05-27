export interface OrganizationBranding {
    displayName: string
    badge: string
    logoUrl?: string
}

export function formatOrganizationName(organizationSlug: string): string {
    return organizationSlug
        .split('-')
        .filter((part) => part.length > 0)
        .map((part) => (part.length <= 3 ? part.toUpperCase() : `${part.charAt(0).toUpperCase()}${part.slice(1)}`))
        .join(' ')
}

export function getOrganizationBadge(organizationName: string, organizationSlug: string): string {
    const clean = organizationName.replace(/[^a-z0-9]/gi, '') || organizationSlug.replace(/[^a-z0-9]/gi, '')
    return clean.slice(0, 3).toUpperCase() || 'ORG'
}

export function getOrganizationBranding(organizationName: string, organizationSlug: string, organizationLogo?: string): OrganizationBranding {
    const displayName = organizationName.trim() || formatOrganizationName(organizationSlug)
    return {
        displayName,
        badge: getOrganizationBadge(displayName, organizationSlug),
        logoUrl: organizationLogo || undefined,
    }
}

export function renderOrganizationBadge(badge: string, logoUrl?: string): string {
    if (logoUrl) {
        return `<img src="${logoUrl}" alt="" class="survey-topbar-logo-img" />`
    }
    return `<div class="survey-topbar-logo-badge">${badge}</div>`
}

