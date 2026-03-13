export interface RouteParams {
    organizationSlug: string
    projectSlug: string
}

export type ViewName = 'landing' | 'survey' | 'completed' | 'ideas' | 'workspace-test'

type ViewRenderer = (container: HTMLElement, params: RouteParams) => void | Promise<void>

const views = new Map<ViewName, ViewRenderer>()

let currentParams: RouteParams = { organizationSlug: '', projectSlug: '' }

export function registerView(name: ViewName, renderer: ViewRenderer): void {
    views.set(name, renderer)
}

function getViewFromPath(pathname: string): ViewName {
    const normalizedPath = pathname.replace(/^\/+|\/+$/g, '')
    const segments = normalizedPath.split('/').filter(Boolean)
    const viewSegment = segments[2]

    if (viewSegment === 'landing') return 'landing'
    if (viewSegment === 'completed') return 'completed'
    if (viewSegment === 'ideas') return 'ideas'
    if (viewSegment === 'workspace-test') return 'workspace-test'
    return 'survey'
}

function buildPath(view: ViewName, params: RouteParams): string {
    const basePath = `/${params.organizationSlug}/${params.projectSlug}`

    if (view === 'landing') return `${basePath}/landing`
    if (view === 'survey') return `${basePath}/survey`
    if (view === 'completed') return `${basePath}/completed`
    if (view === 'workspace-test') return `${basePath}/workspace-test`
    return `${basePath}/ideas`
}

export function getInitialView(): ViewName {
    return getViewFromPath(window.location.pathname)
}

export function parseRoute(): RouteParams {
    const path = window.location.pathname.replace(/^\/+|\/+$/g, '')
    const segments = path.split('/').filter(Boolean)

    const organizationSlug = segments[0] || 'axa-bank'
    const projectSlug = segments[1] || 'mental-wellbeing-2026'

    return { organizationSlug, projectSlug }
}

export async function navigate(
    view: ViewName,
    options: { updateHistory?: boolean; replace?: boolean } = {},
): Promise<void> {
    const { updateHistory = true, replace = false } = options

    const app = document.querySelector<HTMLDivElement>('#app')
    if (!app) {
        throw new Error('App container #app not found')
    }

    const renderer = views.get(view)
    if (!renderer) {
        throw new Error(`View "${view}" is not registered`)
    }

    const targetPath = buildPath(view, currentParams)

    if (updateHistory) {
        const state = { view }
        if (replace) {
            window.history.replaceState(state, '', targetPath)
        } else if (window.location.pathname !== targetPath) {
            window.history.pushState(state, '', targetPath)
        }
    }

    window.dispatchEvent(new CustomEvent('app:before-navigate'))

    app.innerHTML = ''
    await renderer(app, currentParams)
}

export function initRouter(): void {
    currentParams = parseRoute()

    window.addEventListener('popstate', () => {
        currentParams = parseRoute()
        const view = getViewFromPath(window.location.pathname)
        void navigate(view, { updateHistory: false })
    })
}
