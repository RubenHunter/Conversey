import "./main.css";

let app: HTMLDivElement | null = null;

function init(): void {
	app = document.querySelector<HTMLDivElement>('#app')!
	
	// Initialize admin dashboard on admin pages
	if (isAdminPage()) {
		initAdminDashboard();
	}
}

/**
 * Initialize admin dashboard components
 */
async function initAdminDashboard(): Promise<void> {
	// Dynamically import dashboard module
	const { initDashboard } = await import('./components/admin/dashboard/index.js');
	
	// Run dashboard initialization when DOM is ready
	if (document.readyState === 'loading') {
		document.addEventListener('DOMContentLoaded', () => initDashboard());
	} else {
		await initDashboard();
	}
}

export interface ProjectContext {
	organizationSlug: string
	projectSlug: string
}

// Check if current path is an admin page
export function isAdminPage(): boolean {
	return window.location.pathname.startsWith('/admin');
}

export function navigate(to: string) {
	// Don't use project-based navigation for admin pages
	if (isAdminPage()) {
		window.location.href = `/admin/${to}`;
		return;
	}
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
	// Don't parse route for admin pages - they don't use project context
	if (isAdminPage()) {
		// For admin pages, create a minimal context
		const adminContext = { organizationSlug: '', projectSlug: '' };
		renderer(getApp(), adminContext);
		return;
	}
	renderer(getApp(), parseRoute());
}

init()
