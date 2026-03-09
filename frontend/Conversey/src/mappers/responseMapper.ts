import type { ApiResponseAnswerDto, ApiSurveyResponseRequestDto } from '../api/dtos/responseDto.ts'
import type { ResponseAnswer, SurveyResponse } from '../models/response.ts'

function normalizeAnswerValue(answer: ResponseAnswer): ApiResponseAnswerDto['answerValue'] {
    if (answer.value !== undefined) return answer.value
    if (answer.selectedOptionId !== undefined) return answer.selectedOptionId
    if (answer.openTextValue !== undefined) return answer.openTextValue
    return ''
}

export function mapSurveyResponseToApiResponseDto(response: SurveyResponse): ApiSurveyResponseRequestDto {
    return {
        projectId: response.projectId,
        answers: response.answers.map((answer) => ({
            questionId: answer.questionId,
            answerValue: normalizeAnswerValue(answer),
            selectedOptionId: answer.selectedOptionId,
            openTextValue: answer.openTextValue,
        })),
    }
}

