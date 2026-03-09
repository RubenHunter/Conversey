import type { ApiQuestionDto } from '../api/dtos/questionDto.ts'
import { mapApiQuestionsToQuestions } from '../mappers/questionMapper.ts'
import { mapSurveyResponseToApiResponseDto } from '../mappers/responseMapper.ts'
import type { Question } from '../models/question.ts'
import type { SurveyResponse } from '../models/response.ts'
import { apiFetch } from './apiService.ts'

// TODO: Remove mock data once backend question endpoints are implemented
const MOCK_QUESTIONS: Record<number, ApiQuestionDto[]> = {
    1: [
        {
            Id: 1,
            ProjectId: 1,
            Text: 'What is your main source of stress?',
            Type: 'SingleChoice',
            IsRequired: true,
            Order: 1,
            Options: [
                { Id: 1, QuestionId: 1, Text: 'Exams' },
                { Id: 2, QuestionId: 1, Text: 'Financial situation' },
                { Id: 3, QuestionId: 1, Text: 'Family' },
                { Id: 4, QuestionId: 1, Text: 'Social pressure' },
            ],
        },
        {
            Id: 2,
            ProjectId: 1,
            Text: 'How often do you feel overwhelmed during a typical week?',
            Type: 'SingleChoice',
            IsRequired: true,
            Order: 2,
            Options: [
                { Id: 5, QuestionId: 2, Text: 'Never' },
                { Id: 6, QuestionId: 2, Text: '1-2 times' },
                { Id: 7, QuestionId: 2, Text: '3-4 times' },
                { Id: 8, QuestionId: 2, Text: 'Almost every day' },
            ],
        },
        {
            Id: 3,
            ProjectId: 1,
            Text: 'What is your preferred way to relax after a stressful day?',
            Type: 'SingleChoice',
            IsRequired: true,
            Order: 3,
            Options: [
                { Id: 9, QuestionId: 3, Text: 'Spending time with friends' },
                { Id: 10, QuestionId: 3, Text: 'Physical exercise' },
                { Id: 11, QuestionId: 3, Text: 'Creative activities' },
                { Id: 12, QuestionId: 3, Text: 'Sleeping or resting' },
            ],
        },
        {
            Id: 4,
            ProjectId: 1,
            Text: 'How do you rate your current mental health on a scale of 1-10?',
            Type: 'SingleChoice',
            IsRequired: true,
            Order: 4,
            Options: [
                { Id: 13, QuestionId: 4, Text: '1-3 (Poor)' },
                { Id: 14, QuestionId: 4, Text: '4-6 (Fair)' },
                { Id: 15, QuestionId: 4, Text: '7-8 (Good)' },
                { Id: 16, QuestionId: 4, Text: '9-10 (Excellent)' },
            ],
        },
        {
            Id: 5,
            ProjectId: 1,
            Text: 'Describe a situation where you felt supported by someone around you.',
            Type: 'OpenText',
            IsRequired: true,
            Order: 5,
        },
        {
            Id: 6,
            ProjectId: 1,
            Text: 'What would help you manage stress better? Share your ideas.',
            Type: 'OpenText',
            IsRequired: false,
            Order: 6,
        },
        {
            Id: 7,
            ProjectId: 1,
            Text: 'Do you have access to mental health resources or counseling?',
            Type: 'SingleChoice',
            IsRequired: true,
            Order: 7,
            Options: [
                { Id: 17, QuestionId: 7, Text: 'Yes, easily accessible' },
                { Id: 18, QuestionId: 7, Text: 'Yes, but difficult to access' },
                { Id: 19, QuestionId: 7, Text: 'No, not available' },
                { Id: 20, QuestionId: 7, Text: 'Not sure' },
            ],
        },
        {
            Id: 8,
            ProjectId: 1,
            Text: 'What changes would you like to see in your school or workplace to better support mental health?',
            Type: 'OpenText',
            IsRequired: false,
            Order: 8,
        },
    ],
}

export async function getQuestions(projectId: number): Promise<Question[]> {
    // TODO: Remove this mock fallback once /api/projects/{projectId}/questions is implemented
    const mockData = MOCK_QUESTIONS[projectId]
    
    if (mockData) {
        console.log('⚠️ Using mock questions - real API endpoint not yet implemented')
        return Promise.resolve(mapApiQuestionsToQuestions(mockData))
    }

    // When real API is ready, uncomment this:
    // const questionDtos = await apiFetch<ApiQuestionDto[]>(`/projects/${projectId}/questions`)
    // return mapApiQuestionsToQuestions(questionDtos)
    
    throw new Error(`No questions found for project ${projectId}`)
}

export async function submitResponse(response: SurveyResponse): Promise<void> {
    const requestDto = mapSurveyResponseToApiResponseDto(response)

    // TODO: Implement real API endpoint once ready
    const bundledData = {
        timestamp: new Date().toISOString(),
        projectId: requestDto.projectId,
        totalAnswers: requestDto.answers.length,
        answers: requestDto.answers,
        summary: {
            singleChoiceAnswers: requestDto.answers.filter((a) => typeof a.answerValue === 'number').length,
            openTextAnswers: requestDto.answers.filter((a) => typeof a.answerValue === 'string').length,
        },
    }

    console.log('========== SURVEY RESPONSE BUNDLE ==========' )
    console.log(JSON.stringify(bundledData, null, 2))
    console.log('========== END SURVEY RESPONSE ==========' )

    // When real API is ready, uncomment this and remove mock logging:
    // await apiFetch<void>(`/projects/${response.projectId}/responses`, {
    //     method: 'POST',
    //     body: JSON.stringify(requestDto),
    // })
}
