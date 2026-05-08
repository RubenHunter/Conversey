import { esc } from './chatHelpers'
import {SurveyStrings} from "../../../i18n/survey.ts";

export const SPEAKER_SVG = `<svg class="chat-speaker-icon" fill="currentColor" viewBox="0 0 24 24" aria-hidden="true">
  <path d="M3 9v6h4l5 4V5L7 9H3zm13.5 3a4.5 4.5 0 00-2.5-4.03v8.06A4.5 4.5 0 0016.5 12zm-2.5-9.5v2.06a7 7 0 010 13.88v2.06c4.01-.91 7-4.49 7-8.99s-2.99-8.08-7-8.99z"/>
</svg>`

export const AI_AVATAR = `<svg viewBox="0 0 36 36" fill="none" xmlns="http://www.w3.org/2000/svg">
  <circle cx="18" cy="18" r="18" fill="var(--color-primary)"/>
  <circle cx="18" cy="14" r="5" fill="white" fill-opacity="0.9"/>
  <path d="M6 32c0-5.523 5.373-9 12-9s12 3.477 12 9" fill="white" fill-opacity="0.9"/>
</svg>`

export const MAGIC_SVG = `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
  <path d="M9.937 15.5A2 2 0 0 0 8.5 14.063l-6.135-1.582a.5.5 0 0 1 0-.962L8.5 9.936A2 2 0 0 0 9.937 8.5l1.582-6.135a.5.5 0 0 1 .963 0L14.063 8.5A2 2 0 0 0 15.5 9.937l6.135 1.581a.5.5 0 0 1 0 .964L15.5 14.063a2 2 0 0 0-1.437 1.437l-1.582 6.135a.5.5 0 0 1-.963 0z"/>
  <path d="M20 3v4m2-2h-4M4 17v2m1-1H3"/>
</svg>`

export const CHECKMARK_SVG = `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
  <polyline points="20 6 9 17 4 12"/>
</svg>`

export function getIdeationModalsHtml(t: SurveyStrings): string {
    return `
<div id="topic-modal-backdrop" class="modal-backdrop" hidden aria-hidden="true"></div>
<div id="topic-modal" class="modal" role="dialog" aria-modal="true" aria-labelledby="topic-modal-title" hidden>
    <div class="modal-header">
        <h3 id="topic-modal-title">${t.selectTopicTitle}</h3>
        <button id="topic-modal-close" class="modal-close" aria-label="${t.cancel}">&times;</button>
    </div>
    <div class="modal-body">
        <div id="topic-modal-list" class="modal-list"></div>
    </div>
</div>
<div id="idea-panel-backdrop" class="idea-panel-backdrop" hidden aria-hidden="true"></div>
<div id="idea-panel" class="idea-panel" role="dialog" aria-modal="true" aria-label="${t.ideaDetail}" hidden>
    <div class="idea-panel-header">
        <h3 class="idea-panel-title">${t.ideaDetail}</h3>
        <button id="idea-panel-close" class="idea-panel-close" aria-label="${t.cancel}">&times;</button>
    </div>
    <div class="idea-panel-body">
        <div id="idea-panel-pinned" class="idea-panel-pinned" hidden></div>
        <div class="idea-panel-section idea-panel-section--idea">
            <p class="idea-panel-section-label">${t.originalIdea}</p>
            <div id="idea-panel-post" class="idea-panel-post">
                <div id="idea-panel-badges" class="idea-panel-badges"></div>
                <p id="idea-panel-text" class="idea-panel-text"></p>
                <div id="idea-panel-edit-region" hidden>
                    <textarea id="idea-panel-edit-input" class="idea-panel-input idea-panel-edit-input" rows="4" placeholder="${t.editIdea}..."></textarea>
                    <div class="idea-panel-edit-actions">
                        <button id="idea-panel-edit-cancel" class="idea-panel-send idea-panel-send--secondary" type="button">${t.cancel}</button>
                        <button id="idea-panel-edit-save" class="idea-panel-send" type="button" disabled>${t.saveChanges}</button>
                    </div>
                </div>
                <div class="idea-panel-post-actions">
                    <button id="idea-panel-emoji" class="idea-panel-emoji-btn" type="button" title="${t.addReaction}">
                        <span aria-hidden="true">+</span><span aria-hidden="true">:)</span>
                    </button>
                    <button id="idea-panel-copy" class="idea-panel-copy-btn" type="button" aria-label="${t.useAsStartingPoint}" title="${t.useAsStartingPoint}" hidden>
                        <svg class="idea-panel-copy-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                            <rect x="9" y="9" width="13" height="13" rx="2" ry="2"></rect>
                            <path d="M5 15H4a2 2 0 0 1-2-2V4a2 2 0 0 1 2-2h9a2 2 0 0 1 2 2v1"></path>
                        </svg>
                        <span>${t.useAsStartingPoint}</span>
                    </button>
                    <button id="idea-panel-edit-toggle" class="survey-magic-btn idea-panel-edit-cta" type="button" aria-label="${t.editIdea}" hidden>
                        <svg class="w-4 h-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                            <path stroke-linecap="round" stroke-linejoin="round" d="M12 20h9"/>
                            <path stroke-linecap="round" stroke-linejoin="round" d="M16.5 3.5a2.121 2.121 0 1 1 3L7 19l-4 1 1-4 12.5-12.5z"/>
                        </svg>
                        <span class="survey-magic-btn-text">${t.editIdea}</span>
                    </button>
                </div>
            </div>
        </div>
        <div class="idea-panel-section idea-panel-section--responses">
            <p class="idea-panel-section-label">${t.responses}</p>
            <div id="idea-panel-comments" class="idea-panel-comments"></div>
        </div>
    </div>
    <div class="idea-panel-footer">
        <textarea id="idea-panel-input" class="idea-panel-input" placeholder="${t.writeComment}" rows="2"></textarea>
        <button id="idea-panel-send" class="idea-panel-send" type="button" disabled>${t.post}</button>
    </div>
</div>
<div id="safety-review-backdrop" class="safety-review-backdrop" hidden aria-hidden="true"></div>
<div id="safety-review-dialog" class="safety-review-dialog" role="dialog" aria-modal="true" aria-label="${t.safetyReviewTitle}" hidden>
    <div class="safety-review-header"><h3>${t.safetyReviewTitle}</h3></div>
    <div class="safety-review-body">
        <p class="safety-review-copy">${t.safetyReviewCopy}</p>
        <div class="safety-review-block">
            <div class="safety-review-block-head">
                <span class="safety-review-label">${t.yourOriginalMessage}</span>
                <button id="safety-review-edit-original" class="safety-review-edit-icon" type="button" aria-label="${t.editYourResponse}">
                    <svg class="safety-review-edit-glyph" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                        <path stroke-linecap="round" stroke-linejoin="round" d="M12 20h9"/>
                        <path stroke-linecap="round" stroke-linejoin="round" d="M16.5 3.5a2.121 2.121 0 1 1 3L7 19l-4 1 1-4 12.5-12.5z"/>
                    </svg>
                    <span>${t.editYourResponse}</span>
                </button>
            </div>
            <textarea id="safety-review-original" class="safety-review-original" rows="4" readonly></textarea>
        </div>
        <div class="safety-review-block">
            <div class="safety-review-block-head">
                <span class="safety-review-label">${t.aiSuggestion}</span>
                <button id="safety-review-edit-suggestion" class="safety-review-edit-icon" type="button" aria-label="${t.editAiSuggestion}">
                    <svg class="safety-review-edit-glyph" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                        <path stroke-linecap="round" stroke-linejoin="round" d="M12 20h9"/>
                        <path stroke-linecap="round" stroke-linejoin="round" d="M16.5 3.5a2.121 2.121 0 1 1 3L7 19l-4 1 1-4 12.5-12.5z"/>
                    </svg>
                    <span>${t.editAiSuggestion}</span>
                </button>
            </div>
            <textarea id="safety-review-suggestion" class="safety-review-suggestion" rows="4" readonly></textarea>
        </div>
    </div>
    <div class="safety-review-actions">
        <button id="safety-review-accept-suggestion" class="safety-review-btn safety-review-btn--primary" type="button">${t.acceptSuggestion}</button>
        <button id="safety-review-post-anyway" class="safety-review-btn safety-review-btn--warn" type="button">${t.postOriginalAnyway}</button>
    </div>
</div>
<div id="first-idea-contact-gate-backdrop" class="modal-backdrop first-idea-contact-backdrop" hidden aria-hidden="true"></div>
<div id="first-idea-contact-gate-dialog" class="modal first-idea-contact-gate-dialog" role="dialog" aria-modal="true" aria-labelledby="first-idea-contact-gate-title" hidden>
    <div class="modal-header">
        <h3 id="first-idea-contact-gate-title">${t.wantStayInTouch}</h3>
    </div>
    <div class="modal-body">
        <p class="first-idea-contact-copy">${t.stayInTouchCopy}</p>
        <label class="first-idea-contact-check first-idea-contact-check--remember">
            <input id="first-idea-contact-gate-remember" class="first-idea-contact-checkbox" type="checkbox" />
            <span>${t.dontAskAgain}</span>
        </label>
    </div>
    <div class="first-idea-contact-actions">
        <button id="first-idea-contact-gate-deny" class="safety-review-btn first-idea-contact-deny" type="button">${t.noThanks}</button>
        <button id="first-idea-contact-gate-accept" class="safety-review-btn safety-review-btn--primary" type="button">${t.leaveMyEmail}</button>
    </div>
</div>
<div id="first-idea-contact-backdrop" class="modal-backdrop first-idea-contact-backdrop" hidden aria-hidden="true"></div>
<div id="first-idea-contact-dialog" class="modal first-idea-contact-dialog" role="dialog" aria-modal="true" aria-labelledby="first-idea-contact-title" hidden>
    <div class="modal-header">
        <h3 id="first-idea-contact-title">${t.stayInTouchTitle}</h3>
    </div>
    <div class="modal-body first-idea-contact-body">
        <p class="first-idea-contact-copy">${t.leaveEmailCopy} <a class="first-idea-contact-privacy-link" href="https://treecompany.be/privacyverklaring/" target="_blank" rel="noopener noreferrer">${t.leaveEmailCopy}</a></p>
        <label class="first-idea-contact-field" for="first-idea-contact-email">
            <span class="first-idea-contact-label">${t.emailAddress}</span>
            <input id="first-idea-contact-email" class="first-idea-contact-input" type="email" autocomplete="email" placeholder="you@example.com" />
        </label>
        <label class="first-idea-contact-check">
            <input id="first-idea-contact-permission" class="first-idea-contact-checkbox" type="checkbox" />
            <span>${t.agreeContact}</span>
        </label>
        <label class="first-idea-contact-check first-idea-contact-check--remember">
            <input id="first-idea-contact-remember" class="first-idea-contact-checkbox" type="checkbox" />
            <span>${t.rememberChoice}</span>
        </label>
    </div>
    <div class="first-idea-contact-actions">
        <button id="first-idea-contact-deny" class="safety-review-btn first-idea-contact-deny" type="button">${t.deny}</button>
        <button id="first-idea-contact-accept" class="safety-review-btn safety-review-btn--primary first-idea-contact-accept" type="button" disabled>${t.allowContact}</button>
    </div>
</div>`

    + `
<div id="idea-nudge-backdrop" class="modal-backdrop idea-nudge-backdrop" hidden aria-hidden="true"></div>
<div id="idea-nudge-dialog" class="modal idea-nudge-dialog" role="dialog" aria-modal="true" aria-labelledby="idea-nudge-title" hidden>
    <div class="modal-header">
        <h3 id="idea-nudge-title">${t.nudgeTitle}</h3>
        <button id="idea-nudge-close" class="modal-close" aria-label="${t.cancel}">&times;</button>
    </div>
    <div class="modal-body idea-nudge-body">
        <p id="idea-nudge-context" class="idea-nudge-context"></p>
        <p id="idea-nudge-status" class="idea-nudge-status">${t.nudgeStatus}</p>
        <div id="idea-nudge-thread" class="idea-nudge-thread" aria-live="polite"></div>
        <label class="idea-nudge-input-wrap" for="idea-nudge-input">
            <span class="idea-nudge-input-label">${t.yourAnswer}</span>
            <textarea id="idea-nudge-input" class="idea-nudge-input" rows="3" placeholder="${t.answerContinue}..."></textarea>
        </label>
    </div>
    <div class="first-idea-contact-actions idea-nudge-actions">
        <button id="idea-nudge-action" class="safety-review-btn safety-review-btn--primary" type="button">${t.answerContinue}</button>
    </div>
</div>`
}

export interface ChatShellStrings {
    selectAbove: string
    magicMode: string
}

interface RenderChatShellParams {
    projectTitle: string
    questionsCount: number
    headerHTML: string
    t: SurveyStrings
}

export function renderChatShellTemplate({
    projectTitle,
    questionsCount,
    headerHTML,
    t,
}: RenderChatShellParams): string {
    return `
        <div class="chat-shell" id="chat-shell">
            <div class="chat-scroll-area" id="chat-scroll-area">
                ${headerHTML}
                <div class="survey-header" id="chat-survey-header">
                    <div class="survey-header-content">
                        <h2 class="survey-title">${esc(projectTitle)}</h2>
                        <div class="survey-progress-container">
                            <div class="survey-progress-bar">
                                <div class="survey-progress-fill" id="progress-bar"></div>
                            </div>
                            <span class="survey-progress-badge" id="progress-badge">0 / ${questionsCount}</span>
                        </div>
                    </div>
                </div>
                <div class="chat-messages" id="chat-messages"></div>
            </div>
            <div class="chat-input-wrap">
                <div class="chat-input-bar">
                    <button id="chat-magic-btn" class="survey-magic-btn chat-magic-btn" type="button" aria-label="${esc(t.magicMode)}" hidden>
                        ${MAGIC_SVG}
                    </button>
                    <textarea
                        id="chat-input"
                        class="chat-input"
                        placeholder="${esc(t.selectAbove)}"
                        rows="1"
                        disabled
                    ></textarea>
                    <button id="chat-confirm-inline-btn" class="chat-confirm-inline-btn" type="button" aria-label="${esc(t.checkmarkConfirm)}" hidden>
                        ${CHECKMARK_SVG}
                    </button>
                    <button id="chat-send-btn" class="chat-send-btn" type="button" aria-label="${esc(t.chatSend)}" disabled>
                        <svg class="chat-mic-icon" viewBox="0 0 24 24" fill="currentColor" aria-hidden="true">
                            <path d="M12 14c1.66 0 2.99-1.34 2.99-3L15 5c0-1.66-1.34-3-3-3S9 3.34 9 5v6c0 1.66 1.34 3 3 3zm5.3-3c0 3-2.54 5.1-5.3 5.1S6.7 14 6.7 11H5c0 3.41 2.72 6.23 6 6.72V21h2v-3.28c3.28-.48 6-3.3 6-6.72h-1.7z"/>
                        </svg>
                        <svg class="chat-send-icon chat-icon-hidden" viewBox="0 0 24 24" fill="currentColor" aria-hidden="true">
                            <path d="M2.01 21L23 12 2.01 3 2 10l15 2-15 2z"/>
                        </svg>
                    </button>
                </div>
            </div>
        </div>
        ${getIdeationModalsHtml(t)}` 
}

