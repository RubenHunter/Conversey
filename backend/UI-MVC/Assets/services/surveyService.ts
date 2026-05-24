import { mapSurveyResponseToApiResponseDto } from '../mappers/responseMapper'
import type { Question } from '../models/question.ts'
import type { SurveyResponse } from '../models/response'
import { apiFetch } from './apiService.ts'
import { getOrCreateProjectYouthId, normalizeSlugForClient } from './youthIdService.ts'
import {QuestionDto} from "../api/dtos/questionDto.ts";
import {mapQuestionDtosToQuestions} from "../mappers/questionMapper.ts";

function getOrCreateProjectScopedYouthId(projectSlug: string): string {
    return getOrCreateProjectYouthId(projectSlug)
}

export async function getQuestions(workspaceSlug: string, projectSlug: string): Promise<Question[]> {
    const normalizedProjectSlug = normalizeSlugForClient(projectSlug)
    const endpoint = `/workspaces/${workspaceSlug}/projects/${normalizedProjectSlug}/questions`
    const questionDtos = await apiFetch<QuestionDto[]>(endpoint)

    if (import.meta.env.DEV) {
        console.info(`[backend api] loaded questions from ${endpoint}`, questionDtos)
    }

    return mapQuestionDtosToQuestions(questionDtos)
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
