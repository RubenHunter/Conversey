import type { ApiSurveyResponseRequestDto } from '../api/dtos/responseDto'
import type { SurveyResponse } from '../models/response'

export function mapSurveyResponseToApiResponseDto(response: SurveyResponse, youthId: string): ApiSurveyResponseRequestDto {
    return {
        projectId: { Text: response.projectId },
        youthId,
        answers: response.answers.map((answer) => ({
            questionId: answer.questionId,
            selectedOptionId: answer.selectedOptionId,
            openTextValue: answer.openTextValue,
        })),
    }
}
