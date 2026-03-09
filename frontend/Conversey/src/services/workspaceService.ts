import { apiFetch } from './apiService.ts'

export interface Workspace {
    name: string
}

export async function getWorkspaces(): Promise<Workspace[]> {
    return apiFetch<Workspace[]>('/Workspaces')
}

export async function getWorkspaceByName(name: string): Promise<Workspace | undefined> {
    const workspaces = await getWorkspaces()
    return workspaces.find((w) => w.name.toLowerCase() === name.toLowerCase())
}

