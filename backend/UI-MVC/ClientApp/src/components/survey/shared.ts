import { QuestionType, type Question } from '../../models/question.ts'
import { getTTSManager } from '../../services/speechService'

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
                    <button class="survey-speaker-btn" title="Lees voor" aria-label="Lees vraag voor" data-question-id="${question.id}">
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

export function initQuestionTTS(wrapper: HTMLElement, text: string, questionId: string, language: string = 'nl'): void {
  const btn = wrapper.querySelector<HTMLElement>(`.survey-speaker-btn[data-question-id="${questionId}"]`);
  if (!btn) return;

  const tts = getTTSManager();
  let isSpeaking = false;
  let player: HTMLAudioElement | null = null;
  let audioUrl: string | null = null;

  function cleanupAudio(): void {
    if (audioUrl) {
      URL.revokeObjectURL(audioUrl);
      audioUrl = null;
    }
    if (player) {
      player.onended = null;
      player.onerror = null;
      player.pause();
      player = null;
    }
  }

  btn.addEventListener('click', async (e) => {
    e.preventDefault();
    const wasSpeaking = isSpeaking;
    isSpeaking = !isSpeaking;

    if (wasSpeaking) {
      cleanupAudio();
      btn.classList.remove('active');
    } else {
      // Show active state on button only (no text status)
      btn.classList.add('active');
      
      try {
        // Fetch audio inside click handler to maintain user gesture context
        cleanupAudio();
        const audioBlob = await tts.synthesizeSpeech(text, language);
        player = new Audio();
        audioUrl = URL.createObjectURL(audioBlob);
        player.src = audioUrl;
        
        player.onended = () => {
          cleanupAudio();
          btn.classList.remove('active');
          isSpeaking = false;
        };
        player.onerror = () => {
          cleanupAudio();
          btn.classList.remove('active');
          isSpeaking = false;
        };
        
        // Play must happen in direct response to user gesture
        const playPromise = player.play();
        
        if (playPromise !== undefined) {
          await playPromise.catch(() => {
            cleanupAudio();
            btn.classList.remove('active');
            isSpeaking = false;
          });
        }
        
        // Fallback cleanup
        setTimeout(() => {
          if (player && !player.ended) {
            cleanupAudio();
            btn.classList.remove('active');
            isSpeaking = false;
          }
        }, text.length * 200);
      } catch (err) {
        cleanupAudio();
        btn.classList.remove('active');
        isSpeaking = false;
      }
    }
  });
}
