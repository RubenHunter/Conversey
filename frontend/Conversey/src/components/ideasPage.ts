import type { RouteParams } from '../utils/router.ts'
import { getProject } from '../services/projectService.ts'

export async function renderIdeasPage(container: HTMLElement, params: RouteParams): Promise<void> {
    const project = await getProject(params.organizationSlug, params.projectSlug)

    container.innerHTML = `
        <div class="screen-height px-6 py-8">
            <h1 class="text-2xl font-bold mb-3">Ideas Phase</h1>
            <p class="mb-3">Welcome to the ideas page for <strong>${project.title}</strong>.</p>
            <p class="completed-text">This is a placeholder for Sprint 1.</p>
        </div>
    `
}

