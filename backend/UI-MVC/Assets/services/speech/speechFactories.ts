/**
 * Speech Service Factories
 * Factory functions for creating and managing speech service instances.
 */
import { STTManager } from './sttManager';
import { TTSManager } from './ttsManager';
import { getSpeechLanguage } from '../../config/speechConfig';
import type { SpeechState } from './speechTypes';

// ============================================================================
// Singletons
// ============================================================================

let sttManagerInstance: STTManager | null = null;
let ttsManagerInstance: TTSManager | null = null;

/**
 * Gets the singleton STTManager instance.
 * Creates a new instance on first call and sets up cleanup on page unload.
 * @returns The STTManager singleton instance
 */
export function getSTTManager(): STTManager {
  if (!sttManagerInstance) {
    sttManagerInstance = new STTManager();
    window.addEventListener('unload', () => sttManagerInstance?.destroy());
  }
  return sttManagerInstance;
}

/**
 * Gets the singleton TTSManager instance.
 * Creates a new instance on first call and sets up cleanup on page unload.
 * @returns The TTSManager singleton instance
 */
export function getTTSManager(): TTSManager {
  if (!ttsManagerInstance) {
    ttsManagerInstance = new TTSManager();
    window.addEventListener('unload', () => ttsManagerInstance?.destroy());
  }
  return ttsManagerInstance;
}

// ============================================================================
// Speaker Button
// ============================================================================

/**
 * Controller interface for speaker button.
 * Provides methods to control the speaker button programmatically.
 */
export interface SpeakerButtonController {
  stop(): void
  setDisabled(disabled: boolean): void
}

/**
 * Creates a speaker button that reads text aloud when clicked.
 * 
 * Binds click handler to the button that synthesizes and plays the text.
 * Manages button active state during playback.
 * 
 * @param btn - The button element to bind the speaker functionality to
 * @param getText - Function that returns the current text to speak
 * @param getLanguage - Function that returns the language code (default: browser detection)
 * @returns SpeakerButtonController with stop() and setDisabled() methods
 */
export function createSpeakerButton(
  btn: HTMLButtonElement,
  getText: () => string,
  getLanguage: () => string = getSpeechLanguage
): SpeakerButtonController {
  const tts = getTTSManager();
  let playing = false;
  let player: HTMLAudioElement | null = null;
  let audioUrl: string | null = null;

  function cleanup(): void {
    if (audioUrl) { URL.revokeObjectURL(audioUrl); audioUrl = null; }
    if (player) { player.pause(); player = null; }
    btn.classList.remove('active');
    playing = false;
  }

  async function handleClick(e: Event): Promise<void> {
    e.preventDefault();
    if (playing) { cleanup(); return; }
    const text = getText().trim();
    if (!text) return;

    btn.classList.add('active');
    playing = true;
    try {
      const blob = await tts.synthesizeSpeech(text, getLanguage());
      audioUrl = URL.createObjectURL(blob);
      player = new Audio(audioUrl);
      player.addEventListener('ended', cleanup, { once: true });
      player.addEventListener('error', cleanup, { once: true });
      await player.play();
    } catch { cleanup(); }
  }

  btn.addEventListener('click', handleClick);

  return {
    stop: cleanup,
    setDisabled: (disabled: boolean) => { btn.disabled = disabled; },
  };
}

// ============================================================================
// Bind Mic Button
// ============================================================================

/**
 * Binds microphone button to STT recording functionality.
 * 
 * Creates an aura effect around the button, manages recording state, and
 * connects the button to the STT manager for speech-to-text transcription.
 * 
 * @param btn - The button element to bind microphone functionality to
 * @param textarea - The textarea or input element to populate with transcribed text
 * @param getLanguage - Function that returns the language code for transcription
 * @param onText - Callback function invoked when text is transcribed
 * @param getContextBias - Optional function that returns context bias terms for transcription
 * @returns Unbind function to remove the click handler and stop recording
 */
export function bindMicButton(
  btn: HTMLElement,
  textarea: HTMLTextAreaElement | HTMLInputElement,
  getLanguage: () => string,
  onText: (text: string) => void,
  getContextBias?: () => string[]
): () => void {
  const stt = getSTTManager();
  let isRecording = false;

  const aura = document.createElement('span');
  aura.className = 'stt-aura';
  btn.parentNode?.insertBefore(aura, btn);
  aura.appendChild(btn);

  const existingContent = btn.innerHTML;
  btn.innerHTML = `${existingContent}<span class="timer-text"></span>`;
  const timerElement = btn.querySelector('.timer-text') as HTMLElement | null;
  if (timerElement) stt.setTimerElement(timerElement);

  function setActive(active: boolean): void {
    aura.classList.toggle('stt-aura--active', active);
    btn.classList.toggle('active', active);
  }

  async function handleClick(e: Event): Promise<void> {
    e.preventDefault();
    const wasRecording = isRecording;
    isRecording = !isRecording;

    if (wasRecording) {
      stt.stop();
      setActive(false);
    } else {
      setActive(true);
      stt.setupCallbacks({
        onStateChange: (state: SpeechState) => {
          if (state === 'idle' || state === 'error') {
            isRecording = false;
            setActive(false);
          }
        }
      });

      try {
        await stt.start(textarea, getLanguage(), onText, getContextBias?.() ?? []);
      } catch (err) {
        console.error('[STT] Failed to start:', err);
        isRecording = false;
        setActive(false);
      }
    }
  }

  btn.addEventListener('click', handleClick);

  return () => {
    btn.removeEventListener('click', handleClick);
    stt.stop();
    setActive(false);
  };
}
