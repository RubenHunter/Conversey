/*export async function navigate(
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
}*/
