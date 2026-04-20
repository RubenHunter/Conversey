import { apiFetch } from './apiService.ts'

export interface Workspace {
    name: string
    slug: string
}

function toSlug(value: string): string {
    return value
        .trim()
        .toLowerCase()
        .replace(/[^a-z0-9]+/g, '-')
        .replace(/^-+|-+$/g, '')
}

export async function getWorkspaces(): Promise<Workspace[]> {
    return apiFetch<Workspace[]>('/Workspaces')
}

export async function getWorkspaceBySlug(slug: string): Promise<Workspace> {
    return apiFetch<Workspace>(`/Workspaces/${slug}`)
}

export async function getWorkspaceByName(name: string): Promise<Workspace | undefined> {
    const workspaces = await getWorkspaces()
    return workspaces.find((w) => w.name.toLowerCase() === name.toLowerCase())
}

export async function createWorkspace(name: string): Promise<Workspace> {
    const slug = toSlug(name)
    return apiFetch<Workspace>('/Workspaces', {
        method: 'POST',
        body: JSON.stringify({ name, slug }),
    })
}

