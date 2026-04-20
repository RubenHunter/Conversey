import { QuestionType, type Question } from '../../models/question.ts'

function escapeHtml(value: string): string {
    return value
        .replaceAll('&', '&amp;')
        .replaceAll('<', '&lt;')
        .replaceAll('>', '&gt;')
        .replaceAll('"', '&quot;')
        .replaceAll("'", '&#39;')
}

function getAnswerHint(question: Question): string {
    const customHint = question.hint?.trim()
    if (customHint) {
        return customHint
    }

    switch (question.type) {
        case QuestionType.SingleChoice:
            return 'Choose one option.'
        case QuestionType.MultipleChoice:
            return 'Choose one or more options.'
        case QuestionType.Scale:
            return 'Enter a numeric value.'
        case QuestionType.OpenText:
            return 'Write your answer in your own words.'
        default:
            return ''
    }
}

export function generateQuestionHeader(question: Question, questionNumber: number): string {
    const requiredBadge = question.isRequired
        ? '<span class="survey-required-badge">Required</span>'
        : ''
    const answerHint = getAnswerHint(question)
    const answerHintMarkup = answerHint
        ? `<span class="survey-answer-hint">${escapeHtml(answerHint)}</span>`
        : ''

    return `
        <div class="survey-question-header">
            <span class="survey-question-number">${questionNumber}.</span>
            <div>
                <div class="survey-question-title">
                    <span>${escapeHtml(question.text)}</span>
                    <svg class="survey-speaker-icon" fill="currentColor" viewBox="0 0 24 24" aria-hidden="true">
                        <path d="M3 9v6h4l5 4V5L7 9H3zm13.5 3a4.5 4.5 0 00-2.5-4.03v8.06A4.5 4.5 0 0016.5 12zm-2.5-9.5v2.06a7 7 0 010 13.88v2.06c4.01-.91 7-4.49 7-8.99s-2.99-8.08-7-8.99z"/>
                    </svg>
                </div>
                <div class="survey-question-meta">${requiredBadge}${answerHintMarkup}</div>
            </div>
        </div>
    `
}

