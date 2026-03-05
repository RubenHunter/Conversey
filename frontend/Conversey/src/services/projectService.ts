import type { ApiProjectDto } from '../api/dtos/projectDto.ts'
import { mapApiProjectToProject } from '../mappers/projectMapper.ts'
import type { Project } from '../models/project.ts'
import { apiFetch } from './apiService.ts'

const USE_MOCK = true

const MOCK_PROJECTS: Record<string, ApiProjectDto> = {
    'axa-bank/mental-wellbeing-2026': {
        Id: 1,
        Title: 'Mental Wellbeing 2026',
        Description:
            'Help us understand how young people experience stress, wellbeing, and support in their daily lives. Your answers are anonymous and will shape future initiatives.',
        ImageUrl:
            'https://images.unsplash.com/photo-1544027993-37dbfe43562a?auto=format&fit=crop&w=1648&h=3660&q=90&dpr=2',
        Status: 'Active',
    },
}

function getMockProject(orgSlug: string, projectSlug: string): ApiProjectDto | undefined {
    const key = `${orgSlug}/${projectSlug}`
    return MOCK_PROJECTS[key]
}

export async function getProject(orgSlug: string, projectSlug: string): Promise<Project> {
    if (USE_MOCK) {
        const projectDto = getMockProject(orgSlug, projectSlug)
        if (!projectDto) {
            throw new Error(`Project not found: ${orgSlug}/${projectSlug}`)
        }

        return Promise.resolve(mapApiProjectToProject(projectDto, orgSlug, projectSlug))
    }

    const projectDto = await apiFetch<ApiProjectDto>(`/organizations/${orgSlug}/projects/${projectSlug}`)
    return mapApiProjectToProject(projectDto, orgSlug, projectSlug)
}
