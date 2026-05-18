/**
 * STT Manager
 * Manages audio recording and speech-to-text transcription.
 * 
 * Uses a dual buffer system via BufferManager:
 * - complete buffer: Stores all audio chunks for final transcription
 * - temporary buffer: Stores last N chunks + header chunk for real-time feedback
 * 
 * Note: First chunk (index 0) in temporary buffer contains encryption metadata for Mistral Voxtral
 * and MUST NOT be removed. The buffer trims by removing from index 1 when exceeding window size.
 * 
 * Features:
 * - Real-time transcription with periodic updates
 * - Volume analysis for microphone input visualization
 * - Automatic cleanup on stop/destroy
 * - Configurable audio quality and timing
 * - Dependency injection for improved testability
 */
import { SPEECH_CONFIG } from '../../config/speechConfig';
import { getSpeechLanguage } from '../../config/speechConfig';
import { getBestMimeType } from './speechUtils';
import { transcribe } from './speechUtils';
import { createBufferManager, type BufferManager } from './bufferManager';
import { SpeechError } from './speechTypes';
import type { SpeechState, SpeechCallbacks } from './speechTypes';
import type {
    IAudioRecorder,
    IAudioRecorderFactory,
    IAudioContext,
    IAudioContextFactory,
    IStreamService,
    ITranscribeService,
} from './interfaces';
import {
    defaultAudioRecorderFactory,
    defaultAudioContextFactory,
    defaultStreamService,
} from './interfaces';

/**
 * Dependencies for STTManager that can be injected for testing.
 */
export interface STTManagerDependencies {
    /** Factory for creating audio recorders (default: browser MediaRecorder) */
    audioRecorderFactory?: IAudioRecorderFactory;
    /** Factory for creating audio contexts (default: browser AudioContext) */
    audioContextFactory?: IAudioContextFactory;
    /** Service for getting user media streams (default: navigator.mediaDevices) */
    streamService?: IStreamService;
    /** Service for transcription (default: uses transcribe from speechUtils) */
    transcribeService?: ITranscribeService;
}

/**
 * Pure function: generates a placeholder text based on language and dot count.
 * Extracted for testability.
 */
export function generateListeningPlaceholder(language: string, dotCount: number): string {
    const dots = '.'.repeat(dotCount);
    switch (language) {
        case 'fr': return `J'écoute${dots}`;
        case 'en': return `Listening${dots}`;
        default:   return `Luister${dots}`;
    }
}

/**
 * Pure function: generates a processing placeholder text based on language.
 * Extracted for testability.
 */
export function generateProcessingPlaceholder(language: string): string {
    switch (language) {
        case 'fr': return 'Traitement...';
        case 'en': return 'Processing...';
        default:   return 'Verwerken...';
    }
}

/**
 * Pure function: calculates the required recording duration in milliseconds.
 * Extracted for testability.
 */
export function getRequiredRecordingDuration(
    hasFirstTranscription: boolean
): number {
    return hasFirstTranscription
        ? SPEECH_CONFIG.MIN_CONTINUE_DURATION_MS
        : SPEECH_CONFIG.MIN_INITIAL_DURATION_MS;
}

/**
 * Pure function: extracts audio mime type from recorder or uses fallback.
 * Extracted for testability.
 */
export function getRecorderMimeType(
    recorder: IAudioRecorder | null,
    fallback: string = 'audio/webm'
): string {
    return recorder?.mimeType || fallback;
}

/**
 * Pure function: determines if audio buffer meets minimum size requirement.
 * Extracted for testability.
 */
export function meetsMinimumAudioSize(
    blob: Blob,
    minSize: number = SPEECH_CONFIG.MIN_TEMPORARY_AUDIO_SIZE
): boolean {
    return blob.size >= minSize;
}

/**
 * Manages audio recording and speech-to-text transcription.
 */
export class STTManager {
  private recorder: IAudioRecorder | null = null;
  private stream: MediaStream | null = null;
  private chunks: Blob[] = [];
  private bufferManager: BufferManager;
  private language: string = getSpeechLanguage();
  private contextBias: string[] = [];
  private callbacks: SpeechCallbacks = {};
  private textareaRef: HTMLTextAreaElement | HTMLInputElement | null = null;
  private timerTextElement: HTMLElement | null = null;

  private isStopping = false;
  private isTranscribing = false;
  private transcribeDebounce: number | null = null;
  private hasFirstTranscription = false;
  private totalRecordedMs = 0;
  private startTimeMs: number | null = null;
  private hadExistingText = false;
  private originalText = '';

  private dotCount = 0;
  private lastDotUpdate = 0;
  private progressInterval: number | null = null;
  private timerInterval: number | null = null;

  // Volume analysis
  private audioContext: IAudioContext | null = null;
  private analyser: AnalyserNode | null = null;
  private volumeCallbacks: Array<(volume: number) => void> = [];
  private lastVolume = 0;

  // Injected dependencies
  private readonly audioRecorderFactory: IAudioRecorderFactory;
  private readonly audioContextFactory: IAudioContextFactory;
  private readonly streamService: IStreamService;
  private readonly transcribeService: ITranscribeService;

  /**
   * Creates a new STTManager instance.
   *
   * @param dependencies - Optional dependencies for testing. All have backward-compatible defaults.
   */
  constructor(dependencies?: STTManagerDependencies) {
    this.bufferManager = createBufferManager(5);
    this.language = getSpeechLanguage();

    // Use injected dependencies or defaults
    this.audioRecorderFactory = dependencies?.audioRecorderFactory ?? defaultAudioRecorderFactory;
    this.audioContextFactory = dependencies?.audioContextFactory ?? defaultAudioContextFactory;
    this.streamService = dependencies?.streamService ?? defaultStreamService;
    this.transcribeService = dependencies?.transcribeService ?? {transcribe: transcribe};
  }

  /**
   * Sets up event callbacks for the STT manager.
   * Merges with existing callbacks.
   * @param callbacks - Callback functions for state changes, errors, and text
   */
  setupCallbacks(callbacks: SpeechCallbacks): void {
    this.callbacks = {...this.callbacks, ...callbacks};
  }

  /**
   * Sets the DOM element for displaying timer text.
   * @param element - HTMLElement to display recording timer
   */
  setTimerElement(element: HTMLElement): void {
    this.timerTextElement = element;
  }

  /**
   * Registers a callback for volume level updates.
   * Returns an unsubscribe function to remove the callback.
   * @param callback - Function called with volume level (0-1)
   * @returns Function to unsubscribe the callback
   */
  onVolume(callback: (volume: number) => void): () => void {
    this.volumeCallbacks.push(callback);
    return () => {
      this.volumeCallbacks = this.volumeCallbacks.filter(c => c !== callback);
    };
  }

  /**
   * Starts audio recording and transcription.
   *
   * @param textarea - Optional textarea to update with transcribed text
   * @param language - Language code for transcription (default: detected from browser)
   * @param onText - Optional callback for transcribed text
   * @param contextBias - Optional array of bias terms for transcription context
   */
  async start(
      textarea: HTMLTextAreaElement | HTMLInputElement | null,
      language: string = getSpeechLanguage(),
      onText?: (text: string) => void,
      contextBias: string[] = []
  ): Promise<void> {
    this.stop();

    this.language = language;
    this.contextBias = contextBias;
    this.textareaRef = textarea;
    this.originalText = this.textareaRef?.value || '';
    this.hadExistingText = !!this.originalText.trim();
    this.totalRecordedMs = 0;
    this.hasFirstTranscription = false;
    this.dotCount = 0;
    this.lastDotUpdate = 0;

    if (onText) this.callbacks.onText = onText;

    this.setState('listening');
    this.chunks = [];
    this.bufferManager.reset();
    this.isStopping = false;
    this.isTranscribing = false;
    this.startTimeMs = Date.now();

    try {
      this.stream = await this.streamService.getUserMedia({ audio: true, video: false });
      this.blockTextarea();
      this.setupVolumeAnalysis(this.stream);

      const mimeType = getBestMimeType();
      this.recorder = this.audioRecorderFactory(this.stream, mimeType ? { mimeType } : undefined);

      // Force early first chunk to minimize audio data in header chunk
      setTimeout(() => {
        this.recorder?.requestData?.();
      }, SPEECH_CONFIG.EARLY_CHUNK_DELAY_MS);

      // Note: Type assertion for ondataavailable due to browser API differences
      (this.recorder as any).ondataavailable = (e: BlobEvent) => {
        if (e.data.size > 0) {
          // Add to buffers via BufferManager
          this.chunks.push(e.data);
          this.bufferManager.addChunk(e.data);

          this.totalRecordedMs += SPEECH_CONFIG.CHUNK_INTERVAL_MS;
          this.updateTextareaFeedback();
          
          // Trigger debounced transcription on new chunk
          this.triggerTranscribeWindow();
        }
      };

      (this.recorder as any).onstop = async () => {
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

      (this.recorder as any).onerror = () => {
        this.notifyError(new SpeechError('Recording error. Check microphone permissions.', 'RECORDER_ERROR'));
        this.setState('error');
        this.cleanup();
      };

      (this.recorder as any).start(SPEECH_CONFIG.CHUNK_INTERVAL_MS);
      this.progressInterval = window.setInterval(() => this.updateTextareaFeedback(), 200);
      this.timerInterval = window.setInterval(() => this.updateTimerText(), SPEECH_CONFIG.TIMER_UPDATE_INTERVAL_MS);
      this.updateTimerText();
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Microphone not available';
      this.notifyError(new SpeechError(message, 'MICROPHONE_ERROR'));
      this.setState('error');
      this.cleanup();
    }
  }

  /**
   * Stops audio recording.
   * Requests final data from the recorder before stopping.
   */
  stop(): void {
    this.isStopping = true;
    if (this.timerInterval) {
      clearInterval(this.timerInterval);
      this.timerInterval = null;
    }
    if (this.recorder) {
      if ((this.recorder as any).state === 'recording') {
        (this.recorder as any).requestData?.();
        setTimeout(() => (this.recorder as any)?.stop?.(), SPEECH_CONFIG.STOP_DELAY_MS);
      } else {
        (this.recorder as any)?.stop?.();
      }
    }
  }

  /**
   * Destroys the STT manager and cleans up all resources.
   * Stops recording, closes audio streams, and removes callbacks.
   */
  destroy(): void {
    this.stop();
    this.callbacks = {};
  }

  // --------------------------------------------------------------------------
  // Private Methods
  // --------------------------------------------------------------------------

  private setupVolumeAnalysis(stream: MediaStream): void {
    // Reset previous analysis
    this.lastVolume = 0;
    
    try {
      const audioContext = this.audioContextFactory();
      // Type assertions for browser-specific types
      const source = (audioContext as any).createMediaStreamSource(stream);
      const analyser = (audioContext as any).createAnalyser();
      analyser.fftSize = 64;
      source.connect(analyser);

      this.audioContext = audioContext as unknown as IAudioContext;
      this.analyser = analyser as unknown as AnalyserNode;

      const data = new Uint8Array(analyser.frequencyBinCount);

      const update = () => {
        if (!this.analyser) return;
        (this.analyser as any).getByteFrequencyData(data);
        const rawVolume = data.reduce((a, b) => a + b) / data.length / 255;
        // Smooth: 70% old value, 30% new value
        this.lastVolume = this.lastVolume * 0.7 + rawVolume * 0.3;
        
        this.volumeCallbacks.forEach(cb => cb(this.lastVolume));
        
        if (this.audioContext && (this.audioContext as any).state === 'running') {
          requestAnimationFrame(update);
        }
      };
      update();
    } catch (err) {
      console.warn('[STT] Volume analysis not available:', err);
      // Non-fatal: continue without volume visualization
    }
  }

  private blockTextarea(): void {
    if (!this.textareaRef) return;
    (this.textareaRef as HTMLTextAreaElement).readOnly = true;
    this.textareaRef.style.pointerEvents = 'none';
    this.updateTextareaFeedback();
  }

  private setState(state: SpeechState): void {
    this.callbacks.onStateChange?.(state);
  }

  private notifyError(error: SpeechError): void {
    console.error('[STT] Error:', error);
    this.callbacks.onError?.(error);
  }

  private notifyText(text: string, isFinal: boolean = false): void {
    if (this.textareaRef) {
      this.textareaRef.value = isFinal && this.hadExistingText
          ? this.originalText + ' ' + text
          : text;
      this.textareaRef.dispatchEvent(new Event('change', { bubbles: true }));
      this.textareaRef.dispatchEvent(new Event('input', { bubbles: true }));
      this.callbacks.onText?.(this.textareaRef.value);
    } else {
      this.callbacks.onText?.(text);
    }
  }

  private updateTextareaFeedback(): void {
    if (!this.textareaRef) return;

    const requiredMs = getRequiredRecordingDuration(this.hasFirstTranscription);
    const seconds = Math.floor(this.totalRecordedMs / 1000);
    const now = Date.now();

    if (seconds < requiredMs / 1000) {
      if (now - this.lastDotUpdate > SPEECH_CONFIG.DOT_ANIMATION_INTERVAL_MS) {
        this.dotCount = (this.dotCount + 1) % 4;
        this.lastDotUpdate = now;
      }
      this.textareaRef.placeholder = generateListeningPlaceholder(this.language, this.dotCount);
      this.textareaRef.classList.add('speech-listening');
      this.textareaRef.classList.remove('speech-processing');
    } else {
      this.textareaRef.placeholder = generateProcessingPlaceholder(this.language);
      this.textareaRef.classList.remove('speech-listening');
      this.textareaRef.classList.add('speech-processing');
    }
  }

  private triggerTranscribeWindow(): void {
    if (this.transcribeDebounce) clearTimeout(this.transcribeDebounce);
    this.transcribeDebounce = window.setTimeout(() => {
      this.transcribeWindow();
    }, SPEECH_CONFIG.TRANSCRIBE_DEBOUNCE_MS);
  }

  private async processFinalTranscription(): Promise<void> {
    const completeBuffer = this.bufferManager.getCompleteBuffer();
    if (completeBuffer.length > 0) {
      const mimeType = getRecorderMimeType(this.recorder, 'audio/webm');
      // Use ALL complete buffer for full transcription
      const finalText = await this.transcribeService.transcribe(
          new Blob(completeBuffer, { type: mimeType }),
          this.language,
          this.contextBias
      );
      if (finalText?.trim()) {
        this.notifyText(finalText, this.hadExistingText);
      }
    }
  }

  private async transcribeWindow(): Promise<void> {
    const temporaryBuffer = this.bufferManager.getTemporaryBuffer();
    if (this.isTranscribing || this.isStopping || temporaryBuffer.length === 0) return;

    const requiredMs = getRequiredRecordingDuration(this.hasFirstTranscription);

    if (this.totalRecordedMs < requiredMs) return;

    this.isTranscribing = true;
    this.setState('processing');
    this.hasFirstTranscription = true;

    try {
      // Use ONLY temporary buffer (last N chunks + header) for real-time feedback
      const mimeType = getRecorderMimeType(this.recorder, 'audio/webm');
      const blob = new Blob(temporaryBuffer, { type: mimeType });
      
      // Use lower threshold for temporary chunks (real-time feedback)
      if (meetsMinimumAudioSize(blob)) {
        const text = await this.transcribeService.transcribe(
            blob,
            this.language,
            this.contextBias
        );
        if (text?.trim()) this.notifyText(text, false);
      }
    } catch (err) {
      this.notifyError(err as SpeechError);
    } finally {
      this.isTranscribing = false;
    }
  }

  private cleanup(): void {
    if (this.transcribeDebounce) {
      clearTimeout(this.transcribeDebounce);
      this.transcribeDebounce = null;
    }
    this.progressInterval && clearInterval(this.progressInterval);
    this.timerInterval && clearInterval(this.timerInterval);

    // Close audio context for volume analysis
    if (this.audioContext && (this.audioContext as any).state !== 'closed') {
      (this.audioContext as any).close().catch(() => {});
    }
    this.audioContext = null;
    this.analyser = null;
    this.volumeCallbacks = [];
    this.lastVolume = 0;

    this.stream?.getTracks().forEach(t => t.stop());
    this.stream = null;
    (this.recorder as any)?.stop?.();
    this.recorder = null;

    this.chunks = [];
    this.bufferManager.reset();
    this.totalRecordedMs = 0;
    this.isStopping = false;
    this.isTranscribing = false;
    this.startTimeMs = null;
    this.hadExistingText = false;
    this.originalText = '';
    this.dotCount = 0;
    this.lastDotUpdate = 0;

    if (this.timerTextElement) {
      this.timerTextElement.textContent = '';
      this.timerTextElement.classList.remove('timer-blink');
      this.timerTextElement = null;
    }

    this.unblockTextarea();
  }

  private unblockTextarea(): void {
    if (!this.textareaRef) return;
    (this.textareaRef as HTMLTextAreaElement).readOnly = false;
    this.textareaRef.style.pointerEvents = '';
    this.textareaRef.placeholder = '';
    this.textareaRef.classList.remove('speech-listening', 'speech-processing');
  }

  private updateTimerText(): void {
    if (!this.startTimeMs || !this.timerTextElement) return;

    const elapsedMs = Date.now() - this.startTimeMs;
    const remainingMs = Math.max(0, SPEECH_CONFIG.MAX_RECORDING_DURATION_MS - elapsedMs);
    const seconds = Math.ceil(remainingMs / 1000);

    this.timerTextElement.textContent = `${seconds}s`;
    this.timerTextElement.classList.toggle('timer-blink', seconds <= 10);

    if (remainingMs <= 0 && !this.isStopping) this.stop();
  }
}
