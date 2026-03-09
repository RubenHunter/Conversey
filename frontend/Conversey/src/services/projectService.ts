import type { ApiProjectDto } from '../api/dtos/projectDto.ts'
import { mapApiProjectToProject } from '../mappers/projectMapper.ts'
import type { Project } from '../models/project.ts'
import { apiFetch } from './apiService.ts'

// TODO: Remove mock data once backend project endpoints are implemented
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

export async function getProject(orgSlug: string, projectSlug: string): Promise<Project> {
    // TODO: Remove this mock fallback once /api/organizations/{orgSlug}/projects/{projectSlug} is implemented
    const mockKey = `${orgSlug}/${projectSlug}`
    const mockData = MOCK_PROJECTS[mockKey]
    
    if (mockData) {
        console.log('⚠️ Using mock project data - real API endpoint not yet implemented')
        return Promise.resolve(mapApiProjectToProject(mockData, orgSlug, projectSlug))
    }

    // When real API is ready, uncomment this:
    // const projectDto = await apiFetch<ApiProjectDto>(`/organizations/${orgSlug}/projects/${projectSlug}`)
    // return mapApiProjectToProject(projectDto, orgSlug, projectSlug)
    
    throw new Error(`Project not found: ${orgSlug}/${projectSlug}`)
}

