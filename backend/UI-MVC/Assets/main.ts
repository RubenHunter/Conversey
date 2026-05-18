import "./main.css";

let app: HTMLDivElement | null = null;

function init(): void {
	app = document.querySelector<HTMLDivElement>('#app')!
}

export interface ProjectContext {
	organizationSlug: string
	projectSlug: string
}

export function navigate(to: string) {
	window.location.href = `/${parseProject()}/${to}`;
}

function parseProject() {
	return window.location.pathname.split('/')[1];
}

function parseRoute(): ProjectContext {
	const domain = window.location.hostname

	const organizationSlug = domain.split(".")[0]
	const projectSlug = parseProject();

	return { organizationSlug, projectSlug }
}

function getApp():HTMLDivElement {
	if (!app) {
		throw new Error('App container #app not found')
	}
	return app;
}

type ViewRenderer = (container: HTMLElement, params: ProjectContext) => void | Promise<void>
export function render(renderer: ViewRenderer): void {
	renderer(getApp(), parseRoute());
}

init()
