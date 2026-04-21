import "./main.css";

let app: HTMLDivElement;

function init(): void {
	app = document.querySelector<HTMLDivElement>('#app')
}

export interface ProjectContext {
	organizationSlug: string
	projectSlug: string
}

function parseRoute(): ProjectContext {
	const path = window.location.pathname
	const domain = window.location.hostname

	const organizationSlug = domain.split(".")[0]
	const projectSlug = path.split('/')[1];

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
