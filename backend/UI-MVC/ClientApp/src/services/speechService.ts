/** Speech Service - STT & TTS with Voxtral via Mistral API */
import { apiFetch } from './apiService';

// ============================================================================
// Configuration Constants
// ============================================================================

const SPEECH_CONFIG = {
  // Recording settings
  CHUNK_INTERVAL_MS: 2000,
  TRANSCRIBE_INTERVAL_MS: 2000,
  MIN_AUDIO_SIZE: 60000, // Bytes - Mistral needs sufficient context
  DOT_ANIMATION_INTERVAL_MS: 500, // Interval for loading dots animation
  
  // Mistral API models
  TRANSCRIBE_MODEL: 'voxtral-mini-transcribe-2507',
  TTS_MODEL: 'voxtral-mini-tts-latest',
  
  // Default language
  DEFAULT_LANGUAGE: 'nl',
  
  // Audio settings
  AUDIO_TIMEOUT_MS: 200, // Timeout for audio playback per character
  
  // UX settings - progressive buffer
  MIN_INITIAL_DURATION_MS: 5000, // 5s for first transcription
  MIN_CONTINUE_DURATION_MS: 2000, // 2s for subsequent transcriptions
} as const;

// ============================================================================
// Types & Interfaces
// ============================================================================

/** Supported MIME types for MediaRecorder, in priority order */
const PRIORITY_MIME_TYPES = [
  'audio/wav',
  'audio/wav;codecs=pcm',
  'audio/mp3',
  'audio/ogg',
  'audio/webm;codecs=opus',
  'audio/webm'
] as const;

/** State types for speech managers */
export type SpeechState = 'idle' | 'listening' | 'speaking' | 'error' | 'processing';

/** Error with context */
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

/** Callback types */
export interface SpeechCallbacks {
  onStateChange?: (state: SpeechState) => void;
  onError?: (error: SpeechError) => void;
  onText?: (text: string) => void;
}

// ============================================================================
// API Request/Response Types
// ============================================================================

interface TranscribeRequestBody {
  AudioBase64: string;
  Language: string;
  Prompt?: string;
}

interface TranscribeResponse {
  text: string;
}

interface SynthesizeRequestBody {
  Text: string;
  Language: string;
}

// ============================================================================
// Singleton Managers
// ============================================================================

let sttManagerInstance: STTManager | null = null;
let ttsManagerInstance: TTSManager | null = null;

/** Get or create the global STT manager (batch mode) */
export function getSTTManager(): STTManager {
  if (!sttManagerInstance) {
    sttManagerInstance = new STTManager();
    window.addEventListener('unload', () => sttManagerInstance?.destroy());
  }
  return sttManagerInstance;
}

/** Get or create the global TTS manager */
export function getTTSManager(): TTSManager {
  if (!ttsManagerInstance) {
    ttsManagerInstance = new TTSManager();
    window.addEventListener('unload', () => ttsManagerInstance?.destroy());
  }
  return ttsManagerInstance;
}

/** Reset all speech managers (useful for testing) */
export function resetSpeechManagers(): void {
  sttManagerInstance?.destroy();
  ttsManagerInstance?.destroy();
  sttManagerInstance = null;
  ttsManagerInstance = null;
}

// ============================================================================
// Utility Functions
// ============================================================================

/** Convert blob to base64 string */
function toBase64(blob: Blob): Promise<string> {
  return new Promise((resolve) => {
    const reader = new FileReader();
    reader.onload = () => resolve(reader.result as string);
    reader.readAsDataURL(blob);
  });
}

/** Get the best supported MIME type for MediaRecorder */
function getBestMimeType(): string | undefined {
  for (const mimeType of PRIORITY_MIME_TYPES) {
    if (MediaRecorder.isTypeSupported(mimeType)) {
      return mimeType;
    }
  }
  return undefined;
}

// ============================================================================
// API Service Functions
// ============================================================================

/** Send audio to backend for transcription */
async function transcribe(
  audio: Blob,
  language: string = SPEECH_CONFIG.DEFAULT_LANGUAGE,
  prompt?: string
): Promise<string> {
  const audioBase64 = await toBase64(audio);
  
  const body: TranscribeRequestBody = {
    AudioBase64: audioBase64.split(',')[1],
    Language: language,
  };
  if (prompt) body.Prompt = prompt;

  try {
    const response = await apiFetch<TranscribeResponse>('/speech/transcribe', {
      method: 'POST',
      body: JSON.stringify(body)
    });
    return response.text || '';
  } catch (err) {
    const message = err instanceof Error ? err.message : 'Unknown error';
    throw new SpeechError(
      `Transcription failed: ${message}`,
      'TRANSCRIBE_ERROR',
      message
    );
  }
}

/** Synthesize text to speech */
async function synthesize(
  text: string,
  language: string = SPEECH_CONFIG.DEFAULT_LANGUAGE
): Promise<Blob> {
  try {
    const response = await fetch('/api/speech/synthesize', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        Text: text,
        Language: language
      } as SynthesizeRequestBody)
    });

    if (!response.ok) {
      const errorData = await response.json().catch(() => ({}));
      const errorMessage = errorData.Error || errorData.error || `TTS API error: ${response.status}`;
      throw new SpeechError(
        errorMessage,
        'TTS_ERROR',
        JSON.stringify(errorData)
      );
    }
    return await response.blob();
  } catch (err) {
    const message = err instanceof Error ? err.message : 'Unknown error';
    throw new SpeechError(
      `Synthesis failed: ${message}`,
      'SYNTHESIZE_ERROR',
      message
    );
  }
}

// ============================================================================
// STT Manager (Batch Mode)
// ============================================================================

export class STTManager {
  // --------------------------------------------------------------------------
  // Private Properties
  // --------------------------------------------------------------------------
  
  // Recording state
  private recorder: MediaRecorder | null = null;
  private stream: MediaStream | null = null;
  private chunks: Blob[] = [];
  
  // References
  private language: string = SPEECH_CONFIG.DEFAULT_LANGUAGE;
  private callbacks: SpeechCallbacks = {};
  private textareaRef: HTMLTextAreaElement | HTMLInputElement | null = null;
  
  // State flags
  private isStopping = false;
  private isTranscribing = false;
  private hasFirstTranscription = false;
  private totalRecordedMs = 0;
  
  // Animation state
  private dotCount = 0;
  private lastDotUpdate = 0;
  private transcribeInterval: number | null = null;
  private progressInterval: number | null = null;
  
  // --------------------------------------------------------------------------
  // Public API
  // --------------------------------------------------------------------------
  
  /** Register callbacks for state changes and errors */
  setupCallbacks(callbacks: SpeechCallbacks): void {
    this.callbacks = { ...this.callbacks, ...callbacks };
  }

  /** Set default language for transcription */
  setLanguage(language: string): void {
    this.language = language;
  }

  /** Set the textarea to update with transcription results */
  setTextarea(textarea: HTMLTextAreaElement | HTMLInputElement): void {
    this.textareaRef = textarea;
  }

  /** Start recording and transcription */
  async start(
    textarea: HTMLTextAreaElement | HTMLInputElement,
    language: string = SPEECH_CONFIG.DEFAULT_LANGUAGE,
    onText?: (text: string) => void
  ): Promise<void> {
    this.stop(); // Stop any existing recording
    
    // Initialize state
    this.language = language;
    this.textareaRef = textarea;
    this.totalRecordedMs = 0;
    this.hasFirstTranscription = false;
    this.dotCount = 0;
    this.lastDotUpdate = 0;
    
    if (onText) {
      this.callbacks.onText = onText;
    }

    this.setState('listening');
    this.chunks = [];
    this.isStopping = false;
    this.isTranscribing = false;

    try {
      // Request microphone access
      this.stream = await navigator.mediaDevices.getUserMedia({ 
        audio: true, 
        video: false 
      });

      // Block textarea during recording
      this.blockTextarea();

      const mimeType = getBestMimeType();
      this.recorder = new MediaRecorder(this.stream, mimeType ? { mimeType } : undefined);

      // Set up periodic transcription
      this.transcribeInterval = window.setInterval(
        () => this.transcribeWindow(),
        SPEECH_CONFIG.TRANSCRIBE_INTERVAL_MS
      );

      // Handle audio data chunks
      this.recorder.ondataavailable = (e) => {
        if (e.data.size > 0) {
          this.chunks.push(e.data);
          this.totalRecordedMs += SPEECH_CONFIG.CHUNK_INTERVAL_MS;
          this.updateTextareaFeedback();
        }
      };

      // Handle stop event
      this.recorder.onstop = async () => {
        try {
          await this.processFinalTranscription();
          this.setState('idle');
        } catch (err) {
          this.notifyError(err as SpeechError);
          this.setState('error');
        } finally {
          this.isStopping = false;
          this.cleanup();
        }
      };

      // Handle recorder errors
      this.recorder.onerror = () => {
        this.notifyError(new SpeechError(
          'Recording error. Check microphone permissions.',
          'RECORDER_ERROR'
        ));
        this.setState('error');
        this.cleanup();
      };

      // Start recording
      this.recorder.start(SPEECH_CONFIG.CHUNK_INTERVAL_MS);
      
      // Start progress updates (5x per second for dots animation)
      this.progressInterval = window.setInterval(
        () => this.updateTextareaFeedback(),
        200
      );
      
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Microphone not available';
      this.notifyError(new SpeechError(message, 'MICROPHONE_ERROR'));
      this.setState('error');
      this.cleanup();
    }
  }

  /** Stop recording */
  stop(): void {
    this.isStopping = true;
    if (this.recorder && this.recorder.state === 'recording') {
      this.recorder.requestData();
    }
    this.recorder?.stop();
  }

  /** Toggle recording state */
  toggle(
    textarea: HTMLTextAreaElement | HTMLInputElement,
    language: string = SPEECH_CONFIG.DEFAULT_LANGUAGE
  ): void {
    if (this.recorder && this.recorder.state !== 'inactive') {
      this.stop();
    } else {
      this.start(textarea, language);
    }
  }

  /** Destroy manager and clean up resources */
  destroy(): void {
    this.stop();
    this.callbacks = {};
  }

  // --------------------------------------------------------------------------
  // Private Methods
  // --------------------------------------------------------------------------
  
  /** Block textarea during recording */
  private blockTextarea(): void {
    if (!this.textareaRef) return;
    (this.textareaRef as HTMLTextAreaElement).readOnly = true;
    this.textareaRef.style.pointerEvents = 'none';
    this.updateTextareaFeedback();
  }

  /** Update state and notify callbacks */
  private setState(state: SpeechState): void {
    this.callbacks.onStateChange?.(state);
  }

  /** Notify error to callbacks */
  private notifyError(error: SpeechError): void {
    console.error('[STT] Error:', error);
    this.callbacks.onError?.(error);
  }

  /** Update textarea with transcribed text */
  private notifyText(text: string): void {
    if (this.callbacks.onText) {
      this.callbacks.onText(text);
    } else if (this.textareaRef) {
      this.textareaRef.value = text;
      this.textareaRef.dispatchEvent(new Event('change', { bubbles: true }));
      this.textareaRef.dispatchEvent(new Event('input', { bubbles: true }));
    }
  }

  /** Get required duration based on transcription state */
  private getRequiredDurationMs(): number {
    return this.hasFirstTranscription 
      ? SPEECH_CONFIG.MIN_CONTINUE_DURATION_MS
      : SPEECH_CONFIG.MIN_INITIAL_DURATION_MS;
  }

  /** Update textarea loading dots animation */
  private updateTextareaFeedback(): void {
    if (!this.textareaRef) return;

    const requiredMs = this.getRequiredDurationMs();
    const seconds = Math.floor(this.totalRecordedMs / 1000);
    const now = Date.now();

    if (seconds < requiredMs / 1000) {
      // Animate dots every DOT_ANIMATION_INTERVAL_MS
      if (now - this.lastDotUpdate > SPEECH_CONFIG.DOT_ANIMATION_INTERVAL_MS) {
        this.dotCount = (this.dotCount + 1) % 4;
        this.lastDotUpdate = now;
      }
      const dots = '.'.repeat(this.dotCount);
      this.textareaRef.placeholder = `Ik luister${dots}`;
      this.textareaRef.classList.add('speech-listening');
      this.textareaRef.classList.remove('speech-processing');
    } else {
      this.textareaRef.placeholder = 'Verwerken...';
      this.textareaRef.classList.remove('speech-listening');
      this.textareaRef.classList.add('speech-processing');
    }
  }

  /** Process final transcription on stop */
  private async processFinalTranscription(): Promise<void> {
    if (this.chunks.length > 0) {
      const mimeType = this.recorder?.mimeType || 'audio/webm';
      const allBlob = new Blob(this.chunks, { type: mimeType });
      const finalText = await transcribe(allBlob, this.language);
      if (finalText?.trim()) {
        this.notifyText(finalText);
      }
    }
  }

  /** Send current chunks to backend for transcription */
  private async transcribeWindow(): Promise<void> {
    if (this.isTranscribing || this.isStopping) return;
    if (this.chunks.length === 0 || !this.textareaRef) return;

    const requiredMs = this.getRequiredDurationMs();
    if (this.totalRecordedMs < requiredMs) return;

    this.isTranscribing = true;
    this.setState('processing');
    this.hasFirstTranscription = true;
    
    try {
      const mimeType = this.recorder?.mimeType || 'audio/webm';
      const blob = new Blob(this.chunks, { type: mimeType });
      
      // Skip if blob too small for Mistral
      if (blob.size < SPEECH_CONFIG.MIN_AUDIO_SIZE) return;

      const text = await transcribe(blob, this.language);
      if (text?.trim()) {
        this.notifyText(text);
      }
    } catch (err) {
      this.notifyError(err as SpeechError);
    } finally {
      this.isTranscribing = false;
    }
  }

  /** Clean up all resources */
  private cleanup(): void {
    // Clear intervals and timeouts
    if (this.transcribeInterval) {
      clearInterval(this.transcribeInterval);
      this.transcribeInterval = null;
    }
    
    if (this.progressInterval) {
      clearInterval(this.progressInterval);
      this.progressInterval = null;
    }

    // Stop and clean up media stream
    this.stream?.getTracks().forEach(t => t.stop());
    this.stream = null;
    
    this.recorder?.stop();
    this.recorder = null;
    
    // Reset state
    this.chunks = [];
    this.totalRecordedMs = 0;
    this.isStopping = false;
    this.isTranscribing = false;
    this.dotCount = 0;
    this.lastDotUpdate = 0;

    // Re-enable textarea
    this.unblockTextarea();
  }

  /** Unblock textarea after recording stops */
  private unblockTextarea(): void {
    if (!this.textareaRef) return;
    (this.textareaRef as HTMLTextAreaElement).readOnly = false;
    this.textareaRef.style.pointerEvents = '';
    this.textareaRef.placeholder = '';
    this.textareaRef.classList.remove('speech-listening', 'speech-processing');
  }
}

// ============================================================================
// TTS Manager
// ============================================================================

export class TTSManager {
  // --------------------------------------------------------------------------
  // Private Properties
  // --------------------------------------------------------------------------
  
  private player: HTMLAudioElement | null = null;
  private audioUrl: string | null = null;
  private callbacks: SpeechCallbacks = {};
  
  // --------------------------------------------------------------------------
  // Public API
  // --------------------------------------------------------------------------
  
  /** Register callbacks for state changes and errors */
  setupCallbacks(callbacks: SpeechCallbacks): void {
    this.callbacks = { ...this.callbacks, ...callbacks };
  }

  /** Start text-to-speech */
  async start(
    text: string,
    language: string = SPEECH_CONFIG.DEFAULT_LANGUAGE
  ): Promise<void> {
    this.stop();
    this.setState('speaking');

    try {
      const audioBlob = await synthesize(text, language);
      this.setupPlayer(audioBlob);
      await this.playAudio();
      
      // Fallback cleanup if audio doesn't end
      this.setupFallbackCleanup(text);

    } catch (err) {
      this.notifyError(err as SpeechError);
      this.setState('error');
    }
  }

  /** Stop playback */
  stop(): void {
    this.player?.pause();
    this.cleanup();
    this.setState('idle');
  }

  /** Destroy manager and clean up resources */
  destroy(): void {
    this.stop();
    this.callbacks = {};
  }

  /** Synthesize speech and return audio blob without playback */
  synthesizeSpeech(
    text: string,
    language: string = SPEECH_CONFIG.DEFAULT_LANGUAGE
  ): Promise<Blob> {
    return synthesize(text, language);
  }

  // --------------------------------------------------------------------------
  // Private Methods
  // --------------------------------------------------------------------------
  
  /** Set up audio player with blob */
  private setupPlayer(blob: Blob): void {
    this.player = new Audio();
    this.audioUrl = URL.createObjectURL(blob);
    this.player.src = this.audioUrl;

    this.player.onended = () => {
      this.cleanup();
      this.setState('idle');
    };

    this.player.onerror = () => {
      this.notifyError(new SpeechError(
        'Playback error. Try again.',
        'PLAYBACK_ERROR'
      ));
      this.setState('error');
      this.cleanup();
    };
  }

  /** Play audio with error handling */
  private async playAudio(): Promise<void> {
    const playPromise = this.player?.play();
    if (playPromise !== undefined) {
      await playPromise.catch(err => {
        this.notifyError(new SpeechError(
          err instanceof Error ? err.message : 'Playback error',
          'PLAYBACK_ERROR'
        ));
        this.setState('error');
        this.cleanup();
      });
    }
  }

  /** Set up fallback cleanup for audio that doesn't end */
  private setupFallbackCleanup(text: string): void {
    setTimeout(() => {
      if (this.player && !this.player.ended) {
        this.player.onended = null;
        this.cleanup();
      }
    }, text.length * SPEECH_CONFIG.AUDIO_TIMEOUT_MS);
  }

  /** Update state and notify callbacks */
  private setState(state: SpeechState): void {
    this.callbacks.onStateChange?.(state);
  }

  /** Notify error to callbacks */
  private notifyError(error: SpeechError): void {
    console.error('[TTS] Error:', error);
    this.callbacks.onError?.(error);
  }

  /** Clean up audio resources */
  private cleanup(): void {
    if (this.audioUrl) {
      URL.revokeObjectURL(this.audioUrl);
      this.audioUrl = null;
    }
    
    if (this.player) {
      this.player.onended = null;
      this.player.onerror = null;
      this.player = null;
    }
  }
}

// ============================================================================
// Helper Function: Bind Mic Button
// ============================================================================

/**
 * Bind a mic button to the STT manager.
 * Wraps the button in an .stt-aura container for CSS styling.
 * Returns a cleanup function.
 */
export function bindMicButton(
  btn: HTMLElement,
  textarea: HTMLTextAreaElement | HTMLInputElement,
  getLanguage: () => string,
  onText: (text: string) => void
): () => void {
  const stt = getSTTManager();
  let isRecording = false;

  // Create aura wrapper for CSS isolation
  const aura = document.createElement('span');
  aura.className = 'stt-aura';
  btn.parentNode?.insertBefore(aura, btn);
  aura.appendChild(btn);

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
        await stt.start(textarea, getLanguage(), onText);
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
