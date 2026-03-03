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
            text: 'What is your preferred way to relax after a stressful day?',
            type: QuestionType.SingleChoice,
            isRequired: true,
            options: [
                { id: 9, questionId: 3, text: 'Spending time with friends' },
                { id: 10, questionId: 3, text: 'Physical exercise' },
                { id: 11, questionId: 3, text: 'Creative activities' },
                { id: 12, questionId: 3, text: 'Sleeping or resting' },
            ],
        },
        {
            id: 4,
            projectId: 1,
            text: 'How do you rate your current mental health on a scale of 1-10?',
            type: QuestionType.SingleChoice,
            isRequired: true,
            options: [
                { id: 13, questionId: 4, text: '1-3 (Poor)' },
                { id: 14, questionId: 4, text: '4-6 (Fair)' },
                { id: 15, questionId: 4, text: '7-8 (Good)' },
                { id: 16, questionId: 4, text: '9-10 (Excellent)' },
            ],
        },
        {
            id: 5,
            projectId: 1,
            text: 'Describe a situation where you felt supported by someone around you.',
            type: QuestionType.OpenText,
            isRequired: true,
        },
        {
            id: 6,
            projectId: 1,
            text: 'What would help you manage stress better? Share your ideas.',
            type: QuestionType.OpenText,
            isRequired: false,
        },
        {
            id: 7,
            projectId: 1,
            text: 'Do you have access to mental health resources or counseling?',
            type: QuestionType.SingleChoice,
            isRequired: true,
            options: [
                { id: 17, questionId: 7, text: 'Yes, easily accessible' },
                { id: 18, questionId: 7, text: 'Yes, but difficult to access' },
                { id: 19, questionId: 7, text: 'No, not available' },
                { id: 20, questionId: 7, text: 'Not sure' },
            ],
        },
        {
            id: 8,
            projectId: 1,
            text: 'What changes would you like to see in your school or workplace to better support mental health?',
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
        // Bundle and log all survey data
        const bundledData = {
            timestamp: new Date().toISOString(),
            projectId: response.projectId,
            totalAnswers: response.answers.length,
            answers: response.answers,
            summary: {
                singleChoiceAnswers: response.answers.filter((a) => 'selectedOptionId' in a).length,
                openTextAnswers: response.answers.filter((a) => 'openTextValue' in a).length,
            },
        }

        console.log('========== SURVEY RESPONSE BUNDLE ==========')
        console.log(JSON.stringify(bundledData, null, 2))
        console.log('========== END SURVEY RESPONSE ==========')
        console.log('Bundle ready to be sent to API:', bundledData)

        return Promise.resolve()
    }

    // Future: real API call
    // return apiFetch<void>(`/projects/${response.projectId}/responses`, {
    //     method: 'POST',
    //     body: JSON.stringify(response),
    // })
    throw new Error('Real API not yet implemented')
}

