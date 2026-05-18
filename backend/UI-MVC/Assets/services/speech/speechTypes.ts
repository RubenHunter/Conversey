/**
 * Speech Service Types
 * Type definitions for speech recognition and synthesis services.
 */

/**
 * Priority order for audio MIME types.
 * WAV is forced for Mistral Voxtral compatibility.
 * Falls back to first supported type if WAV is not available.
 */
export const PRIORITY_MIME_TYPES = [
  'audio/wav',
  'audio/wav;codecs=pcm',
  'audio/mp3',
  'audio/ogg',
  'audio/webm;codecs=opus',
  'audio/webm',
  'audio/mp4'
] as const;

/**
 * Possible states for speech recognition and synthesis.
 */
export type SpeechState = 'idle' | 'listening' | 'speaking' | 'error' | 'processing';

/**
 * Custom error class for speech service errors.
 * Contains error message, code, and optional details.
 */
export class SpeechError extends Error {
  constructor(
    message: string,
    public readonly code: string,
    public readonly details?: string
  ) {
    super(message);
    this.name = 'SpeechError';
  }
}

/**
 * Callback interface for speech service events.
 * All callbacks are optional.
 */
export interface SpeechCallbacks {
  onStateChange?: (state: SpeechState) => void;
  onError?: (error: SpeechError) => void;
  onText?: (text: string) => void;
}
