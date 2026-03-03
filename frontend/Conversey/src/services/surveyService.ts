import type { Question } from '../models/question.ts'
import { QuestionType } from '../models/question.ts'
import type { SurveyResponse } from '../models/response.ts'

const USE_MOCK = true

const MOCK_QUESTIONS: Record<number, Question[]> = {
    1: [
        {
            id: 1,
            projectId: 1,
            text: 'What is your main source of stress?',
            type: QuestionType.SingleChoice,
            isRequired: true,
            options: [
                { id: 1, questionId: 1, text: 'Exams' },
                { id: 2, questionId: 1, text: 'Financial situation' },
                { id: 3, questionId: 1, text: 'Family' },
                { id: 4, questionId: 1, text: 'Social pressure' },
            ],
        },
        {
            id: 2,
            projectId: 1,
            text: 'How often do you feel overwhelmed during a typical week?',
            type: QuestionType.SingleChoice,
            isRequired: true,
            options: [
                { id: 5, questionId: 2, text: 'Never' },
                { id: 6, questionId: 2, text: '1-2 times' },
                { id: 7, questionId: 2, text: '3-4 times' },
                { id: 8, questionId: 2, text: 'Almost every day' },
            ],
        },
        {
            id: 3,
            projectId: 1,
            text: 'Describe a situation where you felt supported by someone around you.',
            type: QuestionType.OpenText,
            isRequired: true,
        },
        {
            id: 4,
            projectId: 1,
            text: 'What would help you manage stress better? Share your ideas.',
            type: QuestionType.OpenText,
            isRequired: false,
        },
    ],
}

export async function getQuestions(projectId: number): Promise<Question[]> {
    if (USE_MOCK) {
        const questions = MOCK_QUESTIONS[projectId]
        if (!questions) {
            throw new Error(`No questions found for project ${projectId}`)
        }
        return Promise.resolve(questions)
    }

    // Future: real API call
    // return apiFetch<Question[]>(`/projects/${projectId}/questions`)
    throw new Error('Real API not yet implemented')
}

export async function submitResponse(response: SurveyResponse): Promise<void> {
    if (USE_MOCK) {
        console.log('Survey response submitted (mock):', response)
        return Promise.resolve()
    }

    // Future: real API call
    // return apiFetch<void>(`/projects/${response.projectId}/responses`, {
    //     method: 'POST',
    //     body: JSON.stringify(response),
    // })
    throw new Error('Real API not yet implemented')
}

