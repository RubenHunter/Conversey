import type { ApiQuestionDto } from '../api/dtos/questionDto.ts'
import { mapApiQuestionsToQuestions } from '../mappers/questionMapper.ts'
import { mapSurveyResponseToApiResponseDto } from '../mappers/responseMapper.ts'
import type { Question } from '../models/question.ts'
import type { SurveyResponse } from '../models/response.ts'
import { apiFetch } from './apiService.ts'
import { getOrCreateProjectYouthId, normalizeSlugForClient } from './youthIdService.ts'

export function getOrCreateProjectScopedYouthId(projectSlug: string): string {
    return getOrCreateProjectYouthId(projectSlug)
}

export async function getQuestions(workspaceSlug: string, projectSlug: string): Promise<Question[]> {
    const normalizedProjectSlug = normalizeSlugForClient(projectSlug)
    const endpoint = `/workspaces/${workspaceSlug}/projects/${normalizedProjectSlug}/questions`
    const questionDtos = await apiFetch<ApiQuestionDto[]>(endpoint)

    if (import.meta.env.DEV) {
        console.info(`[backend api] loaded questions from ${endpoint}`, questionDtos)
    }

    return mapApiQuestionsToQuestions(questionDtos)
}

export async function submitAnswers(workspaceSlug: string, projectSlug: string, response: SurveyResponse): Promise<void> {
    const normalizedProjectSlug = normalizeSlugForClient(projectSlug)
    const normalizedBodyProjectId = normalizeSlugForClient(response.projectId)
    const youthId = getOrCreateProjectScopedYouthId(normalizedBodyProjectId)
    const normalizedResponse: SurveyResponse = {
        ...response,
        projectId: normalizedBodyProjectId,
    }
    const requestDto = mapSurveyResponseToApiResponseDto(normalizedResponse, youthId)

    const endpoint = `/workspaces/${workspaceSlug}/projects/${normalizedProjectSlug}/answers`
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
