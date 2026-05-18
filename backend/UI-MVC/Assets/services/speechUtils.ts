/**
 * Speech Service Utilities
 * Utility functions for speech recognition and synthesis.
 */
import { apiFetch } from './apiService';
import { PRIORITY_MIME_TYPES } from './speechTypes';
import { SpeechError } from './speechTypes';

/**
 * Converts a Blob to a base64-encoded string.
 * @param blob - The blob to convert
 * @returns Promise resolving to the base64 string
 */
export function toBase64(blob: Blob): Promise<string> {
  return new Promise((resolve) => {
    const reader = new FileReader();
    reader.onload = () => resolve(reader.result as string);
    reader.readAsDataURL(blob);
  });
}

/**
 * Gets the best supported audio MIME type for recording.
 * Forces WAV for Mistral Voxtral compatibility.
 * Falls back to first supported type if WAV is not available.
 * @returns The best MIME type or undefined
 */
export function getBestMimeType(): string | undefined {
  // Force WAV for Mistral Voxtral compatibility
  const wavType = PRIORITY_MIME_TYPES.find(t => t.includes('audio/wav'));
  if (wavType && MediaRecorder.isTypeSupported(wavType)) {
    return wavType;
  }
  // Fallback to first supported type
  return PRIORITY_MIME_TYPES.find(MediaRecorder.isTypeSupported);
}

// ============================================================================
// API Types
// ============================================================================

/** Response from transcribe API */
export interface TranscribeResponse {
  text: string;
}

/** Request body for synthesize API call */
export interface SynthesizeRequestBody {
  Input: string;
  Language: string;
}

// ============================================================================
// API Functions
// ============================================================================

/**
 * Sends audio to the Mistral Voxtral transcription API.
 * Forces WAV mime type for compatibility.
 * @param audio - Audio blob to transcribe
 * @param language - Language code for transcription
 * @param contextBias - Optional context phrases to bias transcription
 * @returns Promise resolving to transcribed text
 */
export async function transcribe(audio: Blob, language: string, contextBias: string[] = []): Promise<string> {
  const audioBase64 = (await toBase64(audio)).split(',')[1];
  // Force WAV mime type for Mistral Voxtral API compatibility
  const mimeType = 'audio/wav';
  try {
    const response = await apiFetch<TranscribeResponse>('/speech/transcribe', {
      method: 'POST',
      body: JSON.stringify({ AudioBase64: audioBase64, Language: language, ContextBias: contextBias, MimeType: mimeType })
    });
    return response.text || '';
  } catch (err) {
    const message = err instanceof Error ? err.message : 'Unknown error';
    throw new SpeechError(`Transcription failed: ${message}`, 'TRANSCRIBE_ERROR', message);
  }
}

/**
 * Sends text to the TTS API for speech synthesis.
 * @param text - Text to synthesize
 * @param language - Language code for synthesis
 * @returns Promise resolving to audio blob
 */
export async function synthesize(text: string, language: string): Promise<Blob> {
  try {
    const response = await fetch('/api/speech/synthesize', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ Input: text, Language: language } as SynthesizeRequestBody)
    });
    if (!response.ok) {
      const errorData = await response.json().catch(() => ({}));
      const errorMessage = errorData.Error || errorData.error || `TTS API error: ${response.status}`;
      throw new SpeechError(errorMessage, 'TTS_ERROR', JSON.stringify(errorData));
    }
    return await response.blob();
  } catch (err) {
    const message = err instanceof Error ? err.message : 'Unknown error';
    throw new SpeechError(`Synthesis failed: ${message}`, 'SYNTHESIZE_ERROR', message);
  }
}
