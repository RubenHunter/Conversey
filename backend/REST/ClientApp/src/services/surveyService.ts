import type { ApiQuestionDto } from '../api/dtos/questionDto.ts'
import { mapApiQuestionsToQuestions } from '../mappers/questionMapper.ts'
import { mapSurveyResponseToApiResponseDto } from '../mappers/responseMapper.ts'
import type { Question } from '../models/question.ts'
import type { SurveyResponse } from '../models/response.ts'
import { apiFetch } from './apiService.ts'

const SURVEY_YOUTH_ID_KEY_PREFIX = 'conversey-survey-youth-id'

function createYouthId(projectId: number): string {
    const randomPart =
        typeof crypto !== 'undefined' && typeof crypto.randomUUID === 'function'
            ? crypto.randomUUID()
            : `${Date.now()}-${Math.random().toString(36).slice(2, 10)}`

    return `anon-p${projectId}-${randomPart}`
}

export function getOrCreateSurveyYouthId(projectId: number): string {
    const key = `${SURVEY_YOUTH_ID_KEY_PREFIX}-${projectId}`
    const existing = localStorage.getItem(key)
    if (existing && existing.length > 0) {
        return existing
    }

    const youthId = createYouthId(projectId)
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
