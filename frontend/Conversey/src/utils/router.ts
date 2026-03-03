export interface RouteParams {
    organizationSlug: string
    projectSlug: string
}

export type ViewName = 'landing' | 'survey' | 'completed'

type ViewRenderer = (container: HTMLElement, params: RouteParams) => void | Promise<void>

const views = new Map<ViewName, ViewRenderer>()

let currentParams: RouteParams = { organizationSlug: '', projectSlug: '' }

export function registerView(name: ViewName, renderer: ViewRenderer): void {
    views.set(name, renderer)
}

export function getRouteParams(): RouteParams {
    return { ...currentParams }
}

export function parseRoute(): RouteParams {
    const path = window.location.pathname.replace(/^\/+|\/+$/g, '')
    const segments = path.split('/')

    // URL pattern: /:organization/:project
    // Default to mock data slugs for development
    const organizationSlug = segments[0] || 'axa-bank'
    const projectSlug = segments[1] || 'mental-wellbeing-2026'

    return { organizationSlug, projectSlug }
}

export async function navigate(view: ViewName): Promise<void> {
    const app = document.querySelector<HTMLDivElement>('#app')
    if (!app) {
        throw new Error('App container #app not found')
    }

    const renderer = views.get(view)
    if (!renderer) {
        throw new Error(`View "${view}" is not registered`)
    }

    app.innerHTML = ''
    await renderer(app, currentParams)
}

export function initRouter(): void {
    currentParams = parseRoute()
}

