import type { ApiQuestionDto } from '../api/dtos/questionDto.ts'
import { mapApiQuestionsToQuestions } from '../mappers/questionMapper.ts'
import { mapSurveyResponseToApiResponseDto } from '../mappers/responseMapper.ts'
import type { Question } from '../models/question.ts'
import type { SurveyResponse } from '../models/response.ts'
import { apiFetch } from './apiService.ts'

const SURVEY_YOUTH_ID_KEY_PREFIX = 'conversey-survey-youth-id'

function isGuid(value: string): boolean {
    return /^[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i.test(value)
}

function createYouthId(): string {
    if (typeof crypto !== 'undefined' && typeof crypto.randomUUID === 'function') {
        return crypto.randomUUID()
    }

    const seed = `${Date.now()}-${Math.random().toString(16).slice(2)}`
    return `00000000-0000-4000-8000-${seed.padEnd(12, '0').slice(0, 12)}`
}

export function getOrCreateSurveyYouthId(projectId: number): string {
    const key = `${SURVEY_YOUTH_ID_KEY_PREFIX}-${projectId}`
    const existing = localStorage.getItem(key)
    if (existing && isGuid(existing)) {
        return existing
    }

    const youthId = createYouthId()
    localStorage.setItem(key, youthId)
    return youthId
}

export async function getQuestions(workspaceSlug: string, projectSlug: string): Promise<Question[]> {
    const endpoint = `/workspaces/${workspaceSlug}/projects/${projectSlug}/questions`
    const questionDtos = await apiFetch<ApiQuestionDto[]>(endpoint)

    if (import.meta.env.DEV) {
        console.info(`[backend api] loaded questions from ${endpoint}`, questionDtos)
    }

    return mapApiQuestionsToQuestions(questionDtos)
}

export async function submitAnswers(workspaceSlug: string, projectSlug: string, response: SurveyResponse): Promise<void> {
    const youthId = getOrCreateSurveyYouthId(response.projectId)
    const requestDto = mapSurveyResponseToApiResponseDto(response, youthId)

    const endpoint = `/workspaces/${workspaceSlug}/projects/${projectSlug}/answers`
    await apiFetch<void>(endpoint, {
        method: 'POST',
        body: JSON.stringify(requestDto),
    })

    if (import.meta.env.DEV) {
        console.info(`[backend api] submitted survey answers to ${endpoint}`, {
            youthId,
            projectId: requestDto.projectId,
            totalAnswers: requestDto.answers.length,
        })
    }
}
