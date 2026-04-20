import type { ApiProjectDto } from '../api/dtos/projectDto.ts'
import { mapApiProjectToProject } from '../mappers/projectMapper.ts'
import type { Project } from '../models/project.ts'
import { apiFetch } from './apiService.ts'

export async function getProject(orgSlug: string, projectSlug: string): Promise<Project> {
    const endpoint = `/workspaces/${orgSlug}/projects/${projectSlug}`
    const projectDto = await apiFetch<ApiProjectDto>(endpoint)

    if (import.meta.env.DEV) {
        console.info(`[backend api] loaded project from ${endpoint}`, projectDto)
    }

    return mapApiProjectToProject(projectDto, orgSlug, projectSlug)
}
