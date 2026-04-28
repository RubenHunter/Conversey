export interface ProjectContext {
	organizationSlug: string
	projectSlug: string
}

export function parseRoute(): ProjectContext {
	const path = window.location.pathname
	const domain = window.location.hostname

	const organizationSlug = domain.split(".")[0]
	const projectSlug = window.location.pathname.split('/').filter(Boolean)[0] || ''

	return { organizationSlug, projectSlug }
}

export function navigate(to: string): void {
	const route = parseRoute();
	window.location.href = `/${route.organizationSlug}/${route.projectSlug}/${to}`;
}

let app: HTMLDivElement | null = null;

export function getApp(): HTMLDivElement {
	if (!app) {
		app = document.querySelector<HTMLDivElement>('#app');
		if (!app) {
			throw new Error('App container #app not found');
		}
	}
	return app;
}

export type ViewRenderer = (container: HTMLElement, params: ProjectContext) => void | Promise<void>;

export function render(renderer: ViewRenderer, params: ProjectContext): void {
	renderer(getApp(), params);
}
