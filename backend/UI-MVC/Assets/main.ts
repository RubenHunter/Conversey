import "./main.css";
import { renderLandingPage } from "./components/landingPage";
import { renderSurveyPage } from "./components/survey/surveyPage";
import { renderIdeasPage } from "./components/ideas/ideasPage";
import { renderCompletedPage } from "./components/completedPage";
import { ProjectContext, navigate, render, parseRoute } from "./shared";

function init(): void {
	const app = document.querySelector<HTMLDivElement>('#app')
	if (!app) {
		console.error('App container #app not found')
		return
	}
	
	const route = parseRoute();
	const path = window.location.pathname.split('/').filter(Boolean);
	
	// Render based on first path segment
	if (!route.projectSlug || path.length === 0) {
		// No project in URL - redirect to a default project or show error
		// For development, you might want to hardcode a test project
		console.warn('No project slug in URL. Use: http://localhost:4180/<organization>/<project>')
		app.innerHTML = `
			<div class="min-h-screen flex flex-col items-center justify-center gap-4">
				<h1 class="text-2xl font-bold">Welcome to Conversey</h1>
				<p class="text-gray-600">Select a project from the list or navigate to:</p>
				<code class="bg-gray-100 p-2 rounded">http://localhost:4180/{organizationSlug}/{projectSlug}</code>
			</div>
		`
		return
	}
	
	// Route to the appropriate page based on the second path segment
	const page = path[1] || 'landing';
	
	switch (page) {
		case 'survey':
			render(renderSurveyPage, route);
			break;
		case 'ideas':
			render(renderIdeasPage, route);
			break;
		case 'completed':
			render(renderCompletedPage, route);
			break;
		default:
			render(renderLandingPage, route);
			break;
	}
}

export { navigate, render, parseRoute } from "./shared";

// Start the app
init()

// Handle browser navigation
window.addEventListener('popstate', () => {
	init();
});
