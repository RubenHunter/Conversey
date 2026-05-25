const BASE_URL = '/api'

export async function apiFetch<T>(endpoint: string, options: RequestInit = {}): Promise<T> {
    const url = `${BASE_URL}${endpoint}`

    const response = await fetch(url, {
        headers: {
            'Content-Type': 'application/json',
            ...options.headers,
        },
        ...options,
    })

    if (!response.ok) {
        const errorBody = await response.text().catch(() => '')
        throw new Error(`API error ${response.status}: ${response.statusText} at ${url}${errorBody ? ` — ${errorBody.slice(0, 300)}` : ''}`)
    }

    if (response.status === 204) {
        return undefined as T
    }

    const contentType = response.headers.get('content-type') ?? ''
    const rawBody = await response.text()

    if (rawBody.length === 0) {
        return undefined as T
    }

    if (!contentType.toLowerCase().includes('application/json')) {
        const preview = rawBody.slice(0, 120).replace(/\s+/g, ' ').trim()
        throw new Error(`Expected JSON from ${url} but received '${contentType || 'unknown'}'. Body starts with: ${preview}`)
    }

    try {
        return JSON.parse(rawBody) as T
    } catch (error) {
        throw new Error(`Failed to parse JSON from ${url}: ${error instanceof Error ? error.message : String(error)}`)
    }
}
