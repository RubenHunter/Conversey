const BASE_URL = import.meta.env.VITE_API_BASE_URL ?? '/api'

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
        throw new Error(`API error ${response.status}: ${response.statusText}`)
    }

    if (response.status === 204) {
        return undefined as T
    }

    return response.json() as Promise<T>
}

