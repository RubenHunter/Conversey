import { createWorkspace, getWorkspaces } from '../../services/workspaceService'

export async function renderWorkspaceTestPage(container: HTMLElement): Promise<void> {
    container.innerHTML = `
        <div style="font-family: monospace; max-width: 640px; margin: 40px auto; padding: 0 20px;">
            <h1 style="font-size: 1.4rem; margin-bottom: 4px;">Workspace API Test</h1>
            <p style="color: #888; margin-bottom: 32px; font-size: 0.85rem;">
                Hits the real backend — open DevTools Network tab to inspect requests.
            </p>

            <section style="margin-bottom: 40px;">
                <h2 style="font-size: 1rem; margin-bottom: 12px;">POST /api/Workspaces — create</h2>
                <div style="display: flex; gap: 8px; margin-bottom: 12px;">
                    <input
                        id="workspace-name-input"
                        type="text"
                        placeholder="Workspace name"
                        style="flex: 1; padding: 8px 12px; border: 1px solid #ccc; border-radius: 6px; font-size: 0.9rem;"
                    />
                    <button
                        id="btn-create"
                        style="padding: 8px 16px; background: #2563eb; color: white; border: none; border-radius: 6px; cursor: pointer; font-size: 0.9rem;">
                        Create
                    </button>
                </div>
                <div id="slug-preview" style="color: #888; font-size: 0.8rem; margin-bottom: 8px;"></div>
                <pre id="create-result" style="background: #f4f4f4; padding: 12px; border-radius: 6px; min-height: 48px; white-space: pre-wrap; font-size: 0.8rem;"></pre>
            </section>

            <section>
                <h2 style="font-size: 1rem; margin-bottom: 12px;">GET /api/Workspaces — list all</h2>
                <button
                    id="btn-list"
                    style="padding: 8px 16px; background: #059669; color: white; border: none; border-radius: 6px; cursor: pointer; font-size: 0.9rem; margin-bottom: 12px;">
                    Fetch workspaces
                </button>
                <pre id="list-result" style="background: #f4f4f4; padding: 12px; border-radius: 6px; min-height: 48px; white-space: pre-wrap; font-size: 0.8rem;"></pre>
            </section>
        </div>
    `

    const nameInput = container.querySelector<HTMLInputElement>('#workspace-name-input')!
    const slugPreview = container.querySelector<HTMLElement>('#slug-preview')!
    const createResult = container.querySelector<HTMLElement>('#create-result')!
    const listResult = container.querySelector<HTMLElement>('#list-result')!

    nameInput.addEventListener('input', () => {
        const slug = toSlugPreview(nameInput.value)
        slugPreview.textContent = slug ? `slug: "${slug}"` : ''
    })

    container.querySelector('#btn-create')!.addEventListener('click', async () => {
        const name = nameInput.value.trim()
        if (!name) {
            createResult.textContent = '⚠ Enter a workspace name first.'
            return
        }
        createResult.textContent = 'Sending request...'
        try {
            const workspace = await createWorkspace(name)
            createResult.textContent = `Created:\n${JSON.stringify(workspace, null, 2)}`
        } catch (err) {
            createResult.textContent = `Error:\n${err instanceof Error ? err.message : String(err)}`
        }
    })

    container.querySelector('#btn-list')!.addEventListener('click', async () => {
        listResult.textContent = 'Fetching...'
        try {
            const workspaces = await getWorkspaces()
            if (!workspaces || (Array.isArray(workspaces) && workspaces.length === 0)) {
                listResult.textContent = '(no workspaces found — 204 No Content)'
            } else {
                listResult.textContent = JSON.stringify(workspaces, null, 2)
            }
        } catch (err) {
            listResult.textContent = `Error:\n${err instanceof Error ? err.message : String(err)}`
        }
    })
}

function toSlugPreview(value: string): string {
    return value
        .trim()
        .toLowerCase()
        .replace(/[^a-z0-9]+/g, '-')
        .replace(/^-+|-+$/g, '')
}

