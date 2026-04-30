import { QuestionType, type Question } from '../../models/question'
import { createSpeakerButton, getSpeechLanguage } from '../../services/speechService'

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
                    <button class="survey-speaker-btn" title="Lees voor" aria-label="Lees vraag voor"
                    data-question-id="${question.id}"
                    data-question-text="${escapeHtml(question.text)}">
                    <svg class="survey-speaker-icon" fill="currentColor" viewBox="0 0 24 24" aria-hidden="true">
                    <path d="M3 9v6h4l5 4V5L7 9H3zm13.5 3a4.5 4.5 0 00-2.5-4.03v8.06A4.5 4.5 0 0016.5 12zm-2.5-9.5v2.06a7 7 0 010 13.88v2.06c4.01-.91 7-4.49 7-8.99s-2.99-8.08-7-8.99z"/>
                    </svg>
                    </button>
                </div>
                <div class="survey-question-meta">${answerHintMarkup}${requiredBadge}</div>
            </div>
        </div>
    `
}

// Helper to setup TTS for question speaker buttons
export function initQuestionSpeakerForWrapper(wrapper: HTMLElement): void {
    const speakerBtn = wrapper.querySelector<HTMLButtonElement>('.survey-speaker-btn');
    if (!speakerBtn) return;

    const questionText = speakerBtn.dataset.questionText || '';
    const questionId = speakerBtn.dataset.questionId || '';
    if (!questionText || !questionId) return;

    createSpeakerButton(speakerBtn, () => questionText, getSpeechLanguage);
}
