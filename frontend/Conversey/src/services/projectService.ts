import type { Project } from '../models/project.ts'

const USE_MOCK = true

const MOCK_PROJECTS: Record<string, Project> = {
    'axa-bank/mental-wellbeing-2026': {
        id: 1,
        slug: 'mental-wellbeing-2026',
        organizationSlug: 'axa-bank',
        title: 'Mental Wellbeing 2026',
        description:
            'Help us understand how young people experience stress, wellbeing, and support in their daily lives. Your answers are anonymous and will shape future initiatives.',
        imageUrl: 'https://images.unsplash.com/photo-1544027993-37dbfe43562a?w=800&h=400&fit=crop',
    },
}

function getMockProject(orgSlug: string, projectSlug: string): Project | undefined {
    const key = `${orgSlug}/${projectSlug}`
    return MOCK_PROJECTS[key]
}

export async function getProject(orgSlug: string, projectSlug: string): Promise<Project> {
    if (USE_MOCK) {
        const project = getMockProject(orgSlug, projectSlug)
        if (!project) {
            throw new Error(`Project not found: ${orgSlug}/${projectSlug}`)
        }
        return Promise.resolve(project)
    }

    // Future: real API call
    // return apiFetch<Project>(`/organizations/${orgSlug}/projects/${projectSlug}`)
    throw new Error('Real API not yet implemented')
}

