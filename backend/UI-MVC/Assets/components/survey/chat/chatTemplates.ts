import { esc } from './chatHelpers'

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

export const IDEATION_MODALS_HTML = `
<div id="topic-modal-backdrop" class="modal-backdrop" hidden aria-hidden="true"></div>
<div id="topic-modal" class="modal" role="dialog" aria-modal="true" aria-labelledby="topic-modal-title" hidden>
    <div class="modal-header">
        <h3 id="topic-modal-title">Select a Topic</h3>
        <button id="topic-modal-close" class="modal-close" aria-label="Close">&times;</button>
    </div>
    <div class="modal-body">
        <div id="topic-modal-list" class="modal-list"></div>
    </div>
</div>
<div id="idea-panel-backdrop" class="idea-panel-backdrop" hidden aria-hidden="true"></div>
<div id="idea-panel" class="idea-panel" role="dialog" aria-modal="true" aria-label="Idea detail" hidden>
    <div class="idea-panel-header">
        <h3 class="idea-panel-title">Idea</h3>
        <button id="idea-panel-close" class="idea-panel-close" aria-label="Close">&times;</button>
    </div>
    <div class="idea-panel-body">
        <div id="idea-panel-pinned" class="idea-panel-pinned" hidden></div>
        <div class="idea-panel-section idea-panel-section--idea">
            <p class="idea-panel-section-label">Original idea</p>
            <div id="idea-panel-post" class="idea-panel-post">
                <div id="idea-panel-badges" class="idea-panel-badges"></div>
                <p id="idea-panel-text" class="idea-panel-text"></p>
                <div id="idea-panel-edit-region" hidden>
                    <textarea id="idea-panel-edit-input" class="idea-panel-input idea-panel-edit-input" rows="4" placeholder="Edit your idea..."></textarea>
                    <div class="idea-panel-edit-actions">
                        <button id="idea-panel-edit-cancel" class="idea-panel-send idea-panel-send--secondary" type="button">Cancel</button>
                        <button id="idea-panel-edit-save" class="idea-panel-send" type="button" disabled>Save changes</button>
                    </div>
                </div>
                <div class="idea-panel-post-actions">
                    <button id="idea-panel-emoji" class="idea-panel-emoji-btn" type="button" title="Add reaction">
                        <span aria-hidden="true">+</span><span aria-hidden="true">:)</span>
                    </button>
                    <button id="idea-panel-copy" class="idea-panel-copy-btn" type="button" aria-label="Use this idea as a starting point" title="Use this idea as a starting point" hidden>
                        <svg class="idea-panel-copy-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                            <rect x="9" y="9" width="13" height="13" rx="2" ry="2"></rect>
                            <path d="M5 15H4a2 2 0 0 1-2-2V4a2 2 0 0 1 2-2h9a2 2 0 0 1 2 2v1"></path>
                        </svg>
                        <span>Use as starter</span>
                    </button>
                    <button id="idea-panel-edit-toggle" class="survey-magic-btn idea-panel-edit-cta" type="button" aria-label="Edit idea before publish" hidden>
                        <svg class="w-4 h-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                            <path stroke-linecap="round" stroke-linejoin="round" d="M12 20h9"/>
                            <path stroke-linecap="round" stroke-linejoin="round" d="M16.5 3.5a2.121 2.121 0 113 3L7 19l-4 1 1-4 12.5-12.5z"/>
                        </svg>
                        <span class="survey-magic-btn-text">Edit idea before publish</span>
                    </button>
                </div>
            </div>
        </div>
        <div class="idea-panel-section idea-panel-section--responses">
            <p class="idea-panel-section-label">Responses</p>
            <div id="idea-panel-comments" class="idea-panel-comments"></div>
        </div>
    </div>
    <div class="idea-panel-footer">
        <textarea id="idea-panel-input" class="idea-panel-input" placeholder="Write a comment..." rows="2"></textarea>
        <button id="idea-panel-send" class="idea-panel-send" type="button" disabled>Post</button>
    </div>
</div>
<div id="safety-review-backdrop" class="safety-review-backdrop" hidden aria-hidden="true"></div>
<div id="safety-review-dialog" class="safety-review-dialog" role="dialog" aria-modal="true" aria-label="Content safety review" hidden>
    <div class="safety-review-header"><h3>Let's keep this space safe</h3></div>
    <div class="safety-review-body">
        <p class="safety-review-copy">Our AI flagged your text as potentially offensive. You can use the suggestion, edit it, or continue with your original text.</p>
        <div class="safety-review-block">
            <div class="safety-review-block-head">
                <span class="safety-review-label">Your original message</span>
                <button id="safety-review-edit-original" class="safety-review-edit-icon" type="button" aria-label="Edit your response">
                    <svg class="safety-review-edit-glyph" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                        <path stroke-linecap="round" stroke-linejoin="round" d="M12 20h9"/>
                        <path stroke-linecap="round" stroke-linejoin="round" d="M16.5 3.5a2.121 2.121 0 113 3L7 19l-4 1 1-4 12.5-12.5z"/>
                    </svg>
                    <span>Edit your response</span>
                </button>
            </div>
            <textarea id="safety-review-original" class="safety-review-original" rows="4" readonly></textarea>
        </div>
        <div class="safety-review-block">
            <div class="safety-review-block-head">
                <span class="safety-review-label">AI suggestion</span>
                <button id="safety-review-edit-suggestion" class="safety-review-edit-icon" type="button" aria-label="Edit the AI suggestion">
                    <svg class="safety-review-edit-glyph" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                        <path stroke-linecap="round" stroke-linejoin="round" d="M12 20h9"/>
                        <path stroke-linecap="round" stroke-linejoin="round" d="M16.5 3.5a2.121 2.121 0 113 3L7 19l-4 1 1-4 12.5-12.5z"/>
                    </svg>
                    <span>Edit the AI suggestion</span>
                </button>
            </div>
            <textarea id="safety-review-suggestion" class="safety-review-suggestion" rows="4" readonly></textarea>
        </div>
    </div>
    <div class="safety-review-actions">
        <button id="safety-review-accept-suggestion" class="safety-review-btn safety-review-btn--primary" type="button">Accept suggestion</button>
        <button id="safety-review-post-anyway" class="safety-review-btn safety-review-btn--warn" type="button">Post original anyway</button>
    </div>
</div>
<div id="first-idea-contact-backdrop" class="modal-backdrop first-idea-contact-backdrop" hidden aria-hidden="true"></div>
<div id="first-idea-contact-dialog" class="modal first-idea-contact-dialog" role="dialog" aria-modal="true" aria-labelledby="first-idea-contact-title" hidden>
    <div class="modal-header">
        <h3 id="first-idea-contact-title">Stay in touch about your idea</h3>
    </div>
    <div class="modal-body first-idea-contact-body">
        <p class="first-idea-contact-copy">You can leave your email if you want us to contact you about your ideas.</p>
        <label class="first-idea-contact-field" for="first-idea-contact-email">
            <span class="first-idea-contact-label">Email address</span>
            <input id="first-idea-contact-email" class="first-idea-contact-input" type="email" autocomplete="email" placeholder="you@example.com" />
        </label>
        <label class="first-idea-contact-check">
            <input id="first-idea-contact-permission" class="first-idea-contact-checkbox" type="checkbox" />
            <span>I agree to be contacted about this idea.</span>
        </label>
        <a class="first-idea-contact-privacy-link" href="https://treecompany.be/privacyverklaring/" target="_blank" rel="noopener noreferrer">Privacy Policy</a>
        <label class="first-idea-contact-check first-idea-contact-check--remember">
            <input id="first-idea-contact-remember" class="first-idea-contact-checkbox" type="checkbox" />
            <span>Remember my choice</span>
        </label>
    </div>
    <div class="first-idea-contact-actions">
        <button id="first-idea-contact-deny" class="safety-review-btn first-idea-contact-deny" type="button">Deny</button>
        <button id="first-idea-contact-accept" class="safety-review-btn safety-review-btn--primary first-idea-contact-accept" type="button" disabled>Allow contact</button>
    </div>
</div>`

export interface ChatShellStrings {
    selectAbove: string
    magicMode: string
}

interface RenderChatShellParams {
    projectTitle: string
    questionsCount: number
    headerHTML: string
    strings: ChatShellStrings
}

export function renderChatShellTemplate({
    projectTitle,
    questionsCount,
    headerHTML,
    strings,
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
                    <button id="chat-magic-btn" class="survey-magic-btn chat-magic-btn" type="button" aria-label="${esc(strings.magicMode)}" hidden>
                        ${MAGIC_SVG}
                    </button>
                    <textarea
                        id="chat-input"
                        class="chat-input"
                        placeholder="${esc(strings.selectAbove)}"
                        rows="1"
                        disabled
                    ></textarea>
                    <button id="chat-confirm-inline-btn" class="chat-confirm-inline-btn" type="button" aria-label="Confirm answer and continue" hidden>
                        ${CHECKMARK_SVG}
                    </button>
                    <button id="chat-send-btn" class="chat-send-btn" type="button" aria-label="Send" disabled>
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
        ${IDEATION_MODALS_HTML}`
}

