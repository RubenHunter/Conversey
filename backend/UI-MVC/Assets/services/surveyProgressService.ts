import type { Question } from '../models/question'
import type { QuestionAnswer } from '../components/survey/singleChoiceQuestion'

const SURVEY_PROGRESS_KEY_PREFIX = 'conversey-survey-progress'
const SURVEY_PROGRESS_VERSION = 1

interface SurveyProgressSnapshot {
    version: number
    questionSignature: string
    currentQuestionIndex: number
    answersByQuestionId: Record<string, QuestionAnswer>
    openTextDraftsByQuestionId?: Record<string, string[]>
}

export interface LoadedSurveyProgress {
    currentQuestionIndex: number
    answersByQuestionId: Map<number, QuestionAnswer>
    openTextDraftsByQuestionId: Map<number, string[]>
}

interface SaveSurveyProgressOptions {
    openTextDraftsByQuestionId?: Map<number, string[]>
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

        const openTextDraftsByQuestionId = new Map<number, string[]>()
        if (parsed.openTextDraftsByQuestionId && typeof parsed.openTextDraftsByQuestionId === 'object') {
            Object.entries(parsed.openTextDraftsByQuestionId).forEach(([questionId, drafts]) => {
                const id = Number(questionId)
                if (!Number.isInteger(id) || !Array.isArray(drafts)) return
                const sanitizedDrafts = drafts.filter((draft): draft is string => typeof draft === 'string')
                openTextDraftsByQuestionId.set(id, sanitizedDrafts)
            })
        }

        return {
            currentQuestionIndex: parsed.currentQuestionIndex,
            answersByQuestionId,
            openTextDraftsByQuestionId,
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
    options?: SaveSurveyProgressOptions,
): void {
    const serializedAnswers: Record<string, QuestionAnswer> = {}
    answersByQuestionId.forEach((answer, questionId) => {
        serializedAnswers[String(questionId)] = answer
    })

    const serializedOpenTextDrafts: Record<string, string[]> = {}
    options?.openTextDraftsByQuestionId?.forEach((drafts, questionId) => {
        serializedOpenTextDrafts[String(questionId)] = drafts
    })

    const snapshot: SurveyProgressSnapshot = {
        version: SURVEY_PROGRESS_VERSION,
        questionSignature: getQuestionSignature(questions),
        currentQuestionIndex,
        answersByQuestionId: serializedAnswers,
        ...(Object.keys(serializedOpenTextDrafts).length > 0 ? { openTextDraftsByQuestionId: serializedOpenTextDrafts } : {}),
    }

    localStorage.setItem(getSurveyProgressKey(projectId), JSON.stringify(snapshot))
}

export function clearSurveyProgress(projectId: string): void {
    localStorage.removeItem(getSurveyProgressKey(projectId))
}
