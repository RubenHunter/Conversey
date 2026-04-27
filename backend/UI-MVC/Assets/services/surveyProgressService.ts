import type { Question } from '../models/question.ts'
import type { QuestionAnswer } from '../components/survey/singleChoiceQuestion.ts'

const SURVEY_PROGRESS_KEY_PREFIX = 'conversey-survey-progress'
const SURVEY_PROGRESS_VERSION = 1

interface SurveyProgressSnapshot {
    version: number
    questionSignature: string
    currentQuestionIndex: number
    answersByQuestionId: Record<string, QuestionAnswer>
}

export interface LoadedSurveyProgress {
    currentQuestionIndex: number
    answersByQuestionId: Map<number, QuestionAnswer>
}

function getSurveyProgressKey(projectId: string): string {
    return `${SURVEY_PROGRESS_KEY_PREFIX}-${projectId}`
}

function getQuestionSignature(questions: Question[]): string {
    return questions.map((question) => `${question.id}:${question.type}`).join('|')
}

export function loadSurveyProgress(projectId: string, questions: Question[]): LoadedSurveyProgress | null {
    const raw = localStorage.getItem(getSurveyProgressKey(projectId))
    if (!raw) {
        return null
    }

    try {
        const parsed = JSON.parse(raw) as SurveyProgressSnapshot
        if (
            parsed.version !== SURVEY_PROGRESS_VERSION
            || parsed.questionSignature !== getQuestionSignature(questions)
            || typeof parsed.currentQuestionIndex !== 'number'
            || typeof parsed.answersByQuestionId !== 'object'
            || parsed.answersByQuestionId === null
        ) {
            return null
        }

        const answersByQuestionId = new Map<number, QuestionAnswer>()
        Object.entries(parsed.answersByQuestionId).forEach(([questionId, answer]) => {
            const id = Number(questionId)
            if (Number.isInteger(id)) {
                answersByQuestionId.set(id, answer)
            }
        })

        return {
            currentQuestionIndex: parsed.currentQuestionIndex,
            answersByQuestionId,
        }
    } catch {
        return null
    }
}

export function saveSurveyProgress(
    projectId: string,
    questions: Question[],
    currentQuestionIndex: number,
    answersByQuestionId: Map<number, QuestionAnswer>,
): void {
    const serializedAnswers: Record<string, QuestionAnswer> = {}
    answersByQuestionId.forEach((answer, questionId) => {
        serializedAnswers[String(questionId)] = answer
    })

    const snapshot: SurveyProgressSnapshot = {
        version: SURVEY_PROGRESS_VERSION,
        questionSignature: getQuestionSignature(questions),
        currentQuestionIndex,
        answersByQuestionId: serializedAnswers,
    }

    localStorage.setItem(getSurveyProgressKey(projectId), JSON.stringify(snapshot))
}

export function clearSurveyProgress(projectId: string): void {
    localStorage.removeItem(getSurveyProgressKey(projectId))
}

