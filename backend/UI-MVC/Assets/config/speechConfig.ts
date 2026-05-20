/**
 * Speech Service Configuration
 * Configuration constants for STT and TTS services.
 */

/**
 * Speech service configuration constants.
 * 
 * @property CHUNK_INTERVAL_MS - Interval in ms for MediaRecorder dataavailable events
 * @property MIN_AUDIO_SIZE - Minimum audio size in bytes for final transcription
 * @property MIN_TEMPORARY_AUDIO_SIZE - Minimum audio size in bytes for real-time feedback (5KB)
 * @property DOT_ANIMATION_INTERVAL_MS - Interval for listening placeholder dot animation
 * @property AUDIO_TIMEOUT_MS - Timeout per character for TTS fallback cleanup
 * @property MIN_INITIAL_DURATION_MS - Minimum recording duration in ms for first transcription
 * @property MIN_CONTINUE_DURATION_MS - Minimum recording duration in ms for subsequent transcriptions
 * @property MAX_RECORDING_DURATION_MS - Maximum recording duration in ms (60 seconds)
 * @property TIMER_UPDATE_INTERVAL_MS - Interval for timer display updates
 * @property EARLY_CHUNK_DELAY_MS - Delay before first chunk request to minimize header audio data
 * @property TRANSCRIBE_DEBOUNCE_MS - Debounce delay for transcription requests
 * @property STOP_DELAY_MS - Delay before stop after final requestData call
 */
export const SPEECH_CONFIG = {
  CHUNK_INTERVAL_MS: 2000,
  MIN_AUDIO_SIZE: 60000,           // For final transcription
  MIN_TEMPORARY_AUDIO_SIZE: 5000,  // For real-time feedback (5KB)
  DOT_ANIMATION_INTERVAL_MS: 500,
  AUDIO_TIMEOUT_MS: 200,
  MIN_INITIAL_DURATION_MS: 2000,
  MIN_CONTINUE_DURATION_MS: 1000,
  MAX_RECORDING_DURATION_MS: 60000,
  TIMER_UPDATE_INTERVAL_MS: 100,
  EARLY_CHUNK_DELAY_MS: 100,       // Delay for first chunk request
  TRANSCRIBE_DEBOUNCE_MS: 150,    // Debounce delay for transcription
  STOP_DELAY_MS: 300,             // Delay before stop after requestData
} as const;

/**
 * Detects the browser language and returns the appropriate speech language code.
 * @returns 'en' for English, 'fr' for French, 'nl' for Dutch (default)
 */
export function getSpeechLanguage(): string {
  const lang = navigator.language.toLowerCase()
  if (lang.startsWith('en')) return 'en'
  if (lang.startsWith('fr')) return 'fr'
  return 'nl'
}
