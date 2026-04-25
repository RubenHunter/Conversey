const PROJECT_YOUTH_ID_KEY_PREFIX = 'conversey-project-youth-id'
const LEGACY_SURVEY_YOUTH_ID_KEY_PREFIX = 'conversey-survey-youth-id'
const LEGACY_IDEAS_YOUTH_ID_KEY_PREFIX = 'conversey-ideas-user-id'
const BROKEN_OBJECT_KEY_SUFFIX = '[object Object]'

function isGuid(value: string): boolean {
    return /^[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i.test(value)
}

function createGuidToken(): string {
    if (typeof crypto !== 'undefined' && typeof crypto.randomUUID === 'function') {
        return crypto.randomUUID()
    }

    const seed = `${Date.now()}-${Math.random().toString(16).slice(2)}`
    return `00000000-0000-4000-8000-${seed.padEnd(12, '0').slice(0, 12)}`
}

function extractSlugText(value: unknown): string | undefined {
    if (typeof value === 'string' && value.length > 0) {
        return value
    }

    if (value && typeof value === 'object') {
        const maybeSlug = value as { text?: unknown; Text?: unknown }
        if (typeof maybeSlug.text === 'string' && maybeSlug.text.length > 0) {
            return maybeSlug.text
        }
        if (typeof maybeSlug.Text === 'string' && maybeSlug.Text.length > 0) {
            return maybeSlug.Text
        }
    }

    return undefined
}

export function normalizeSlugForClient(value: unknown): string {
    return extractSlugText(value) ?? String(value ?? '').trim()
}

function readYouthIdFromKey(key: string): string | null {
    const value = localStorage.getItem(key)
    if (value && isGuid(value)) {
        return value
    }

    return null
}

function findLegacyYouthId(projectSlug: string): string | null {
    const candidateKeys = [
        `${LEGACY_SURVEY_YOUTH_ID_KEY_PREFIX}-${projectSlug}`,
        `${LEGACY_IDEAS_YOUTH_ID_KEY_PREFIX}-${projectSlug}`,
        `${PROJECT_YOUTH_ID_KEY_PREFIX}-${BROKEN_OBJECT_KEY_SUFFIX}`,
        `${LEGACY_SURVEY_YOUTH_ID_KEY_PREFIX}-${BROKEN_OBJECT_KEY_SUFFIX}`,
        `${LEGACY_IDEAS_YOUTH_ID_KEY_PREFIX}-${BROKEN_OBJECT_KEY_SUFFIX}`,
    ]

    for (const key of candidateKeys) {
        const candidate = readYouthIdFromKey(key)
        if (candidate) {
            return candidate
        }
    }

    return null
}

export function getOrCreateProjectYouthId(projectSlugInput: unknown): string {
    const projectSlug = normalizeSlugForClient(projectSlugInput)
    const key = `${PROJECT_YOUTH_ID_KEY_PREFIX}-${projectSlug}`

    const existing = readYouthIdFromKey(key)
    if (existing) {
        return existing
    }

    const migrated = findLegacyYouthId(projectSlug)
    if (migrated) {
        localStorage.setItem(key, migrated)
        return migrated
    }

    const youthId = createGuidToken()
    localStorage.setItem(key, youthId)
    return youthId
}
