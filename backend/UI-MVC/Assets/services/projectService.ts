import type { ApiProjectDto } from '../api/dtos/projectDto'
import { mapApiProjectToProject } from '../mappers/projectMapper'
import type { Project } from '../models/project'
import { apiFetch } from './apiService'

export async function getProject(orgSlug: string, projectSlug: string): Promise<Project> {
    const endpoint = `/workspaces/${orgSlug}/projects/${projectSlug}`
    const projectDto = await apiFetch<ApiProjectDto>(endpoint)

    if (import.meta.env.DEV) {
        console.info(`[backend api] loaded project from ${endpoint}`, projectDto)
    }

    return mapApiProjectToProject(projectDto, orgSlug, projectSlug)
}
