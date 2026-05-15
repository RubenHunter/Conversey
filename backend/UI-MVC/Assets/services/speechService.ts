/** Speech Service - STT & TTS with Mistral Voxtral API */
import {apiFetch} from './apiService';

// ============================================================================
// Configuration
// ============================================================================

const SPEECH_CONFIG = {
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

export function getSpeechLanguage(): string {
  const lang = navigator.language.toLowerCase()
  if (lang.startsWith('en')) return 'en'
  if (lang.startsWith('fr')) return 'fr'
  return 'nl'
}

const PRIORITY_MIME_TYPES = [
  'audio/wav',
  'audio/wav;codecs=pcm',
  'audio/mp3',
  'audio/ogg',
  'audio/webm;codecs=opus',
  'audio/webm',
  'audio/mp4'
] as const;

export type SpeechState = 'idle' | 'listening' | 'speaking' | 'error' | 'processing';

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

export interface SpeechCallbacks {
  onStateChange?: (state: SpeechState) => void;
  onError?: (error: SpeechError) => void;
  onText?: (text: string) => void;
}

interface TranscribeResponse { text: string; }
interface SynthesizeRequestBody { Input: string; Language: string; }

// ============================================================================
// Singletons
// ============================================================================

let sttManagerInstance: STTManager | null = null;
let ttsManagerInstance: TTSManager | null = null;

export function getSTTManager(): STTManager {
  if (!sttManagerInstance) {
    sttManagerInstance = new STTManager();
    window.addEventListener('unload', () => sttManagerInstance?.destroy());
  }
  return sttManagerInstance;
}

export function getTTSManager(): TTSManager {
  if (!ttsManagerInstance) {
    ttsManagerInstance = new TTSManager();
    window.addEventListener('unload', () => ttsManagerInstance?.destroy());
  }
  return ttsManagerInstance;
}

// ============================================================================
// Utilities
// ============================================================================

function toBase64(blob: Blob): Promise<string> {
  return new Promise((resolve) => {
    const reader = new FileReader();
    reader.onload = () => resolve(reader.result as string);
    reader.readAsDataURL(blob);
  });
}

function getBestMimeType(): string | undefined {
  // Force WAV for Mistral Voxtral compatibility
  const wavType = PRIORITY_MIME_TYPES.find(t => t.includes('audio/wav'));
  if (wavType && MediaRecorder.isTypeSupported(wavType)) {
    return wavType;
  }
  // Fallback to first supported type
  return PRIORITY_MIME_TYPES.find(MediaRecorder.isTypeSupported);
}

// ============================================================================
// 
// ============================================================================

async function transcribe(audio: Blob, language: string, contextBias: string[] = []): Promise<string> {
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

async function synthesize(text: string, language: string): Promise<Blob> {
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

// ============================================================================
// STT Manager
// ============================================================================

export class STTManager {
  private recorder: MediaRecorder | null = null;
  private stream: MediaStream | null = null;
  private chunks: Blob[] = [];
  private completeChunks: Blob[] = [];        // Full recording for final transcription
  private temporaryChunks: Blob[] = [];     // Last  chunks for real-time feedback
  private readonly TEMPORARY_WINDOW_SIZE = 5; // 5 recent chunks + 1 permanent header chunk
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
  private transcribeInterval: number | null = null;
  private progressInterval: number | null = null;
  private timerInterval: number | null = null;

  // Volume analysis
  private audioContext: AudioContext | null = null;
  private analyser: AnalyserNode | null = null;
  private volumeCallbacks: Array<(volume: number) => void> = [];
  private lastVolume = 0;

  setupCallbacks(callbacks: SpeechCallbacks): void {
    this.callbacks = { ...this.callbacks, ...callbacks };
  }

  setTimerElement(element: HTMLElement): void {
    this.timerTextElement = element;
  }

  onVolume(callback: (volume: number) => void): () => void {
    this.volumeCallbacks.push(callback);
    return () => {
      this.volumeCallbacks = this.volumeCallbacks.filter(c => c !== callback);
    };
  }

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
    this.completeChunks = [];        // Reset full recording buffer
    this.temporaryChunks = [];     // Reset temporary buffer
    this.isStopping = false;
    this.isTranscribing = false;
    this.startTimeMs = Date.now();

    try {
      this.stream = await navigator.mediaDevices.getUserMedia({ audio: true, video: false });
      this.blockTextarea();
      this.setupVolumeAnalysis(this.stream);

      const mimeType = getBestMimeType();
      this.recorder = new MediaRecorder(this.stream, mimeType ? { mimeType } : undefined);

      // Force early first chunk to minimize audio data in header chunk
      setTimeout(() => {
        this.recorder?.requestData();
      }, SPEECH_CONFIG.EARLY_CHUNK_DELAY_MS);

      this.recorder.ondataavailable = (e) => {
        if (e.data.size > 0) {
          // Add to ALL buffers
          this.chunks.push(e.data);
          this.completeChunks.push(e.data);
          this.temporaryChunks.push(e.data);

          // Keep first chunk (contains headers), trim from index 1
          if (this.temporaryChunks.length > this.TEMPORARY_WINDOW_SIZE + 1) {
            this.temporaryChunks.splice(1, 1);
          }

          this.totalRecordedMs += SPEECH_CONFIG.CHUNK_INTERVAL_MS;
          this.updateTextareaFeedback();
          
          // Trigger debounced transcription on new chunk
          this.triggerTranscribeWindow();
        }
      };

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

      this.recorder.onerror = () => {
        this.notifyError(new SpeechError('Recording error. Check microphone permissions.', 'RECORDER_ERROR'));
        this.setState('error');
        this.cleanup();
      };

      this.recorder.start(SPEECH_CONFIG.CHUNK_INTERVAL_MS);
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

  stop(): void {
    this.isStopping = true;
    if (this.timerInterval) {
      clearInterval(this.timerInterval);
      this.timerInterval = null;
    }
    if (this.recorder?.state === 'recording') {
      this.recorder.requestData();
      setTimeout(() => this.recorder?.stop(), SPEECH_CONFIG.STOP_DELAY_MS);
    } else {
      this.recorder?.stop();
    }
  }

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
      const audioContext = new (window.AudioContext || 
        (window as any).webkitAudioContext)();
      const source = audioContext.createMediaStreamSource(stream);
      const analyser = audioContext.createAnalyser();
      analyser.fftSize = 64;
      source.connect(analyser);

      this.audioContext = audioContext;
      this.analyser = analyser;

      const data = new Uint8Array(analyser.frequencyBinCount);

      const update = () => {
        if (!this.analyser) return;
        this.analyser.getByteFrequencyData(data);
        const rawVolume = data.reduce((a, b) => a + b) / data.length / 255;
        // Smooth: 70% old value, 30% new value
        this.lastVolume = this.lastVolume * 0.7 + rawVolume * 0.3;
        
        this.volumeCallbacks.forEach(cb => cb(this.lastVolume));
        
        if (this.audioContext?.state === 'running') {
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

  private getRequiredDurationMs(): number {
    return this.hasFirstTranscription
      ? SPEECH_CONFIG.MIN_CONTINUE_DURATION_MS
      : SPEECH_CONFIG.MIN_INITIAL_DURATION_MS;
  }

  private getListeningPlaceholder(): string {
    const dots = '.'.repeat(this.dotCount)
    switch (this.language) {
      case 'fr': return `J'écoute${dots}`
      case 'en': return `Listening${dots}`
      default:   return `Luister${dots}`
    }
  }

  private getProcessingPlaceholder(): string {
    switch (this.language) {
      case 'fr': return 'Traitement...'
      case 'en': return 'Processing...'
      default:   return 'Verwerken...'
    }
  }

  private updateTextareaFeedback(): void {
    if (!this.textareaRef) return;

    const requiredMs = this.getRequiredDurationMs();
    const seconds = Math.floor(this.totalRecordedMs / 1000);
    const now = Date.now();

    if (seconds < requiredMs / 1000) {
      if (now - this.lastDotUpdate > SPEECH_CONFIG.DOT_ANIMATION_INTERVAL_MS) {
        this.dotCount = (this.dotCount + 1) % 4;
        this.lastDotUpdate = now;
      }
      this.textareaRef.placeholder = this.getListeningPlaceholder();
      this.textareaRef.classList.add('speech-listening');
      this.textareaRef.classList.remove('speech-processing');
    } else {
      this.textareaRef.placeholder = this.getProcessingPlaceholder();
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
    if (this.completeChunks.length > 0) {
      const mimeType = this.recorder?.mimeType || 'audio/webm';
      // Use ALL completeChunks for full transcription
      const finalText = await transcribe(new Blob(this.completeChunks, { type: mimeType }), this.language, this.contextBias);
      if (finalText?.trim()) {
        this.notifyText(finalText, this.hadExistingText);
      }
    }
  }

  private async transcribeWindow(): Promise<void> {
    if (this.isTranscribing || this.isStopping || this.temporaryChunks.length === 0) return;

    const requiredMs = this.getRequiredDurationMs();
    if (this.totalRecordedMs < requiredMs) return;

    this.isTranscribing = true;
    this.setState('processing');
    this.hasFirstTranscription = true;

    try {
      // Use ONLY temporaryChunks (last 10s audio) for real-time feedback
      const blob = new Blob(this.temporaryChunks, { type: this.recorder?.mimeType || 'audio/webm' });
      
      // Use lower threshold for temporary chunks (real-time feedback)
      if (blob.size >= SPEECH_CONFIG.MIN_TEMPORARY_AUDIO_SIZE) {
        const text = await transcribe(blob, this.language, this.contextBias);
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
    if (this.audioContext && this.audioContext.state !== 'closed') {
      this.audioContext.close().catch(() => {});
    }
    this.audioContext = null;
    this.analyser = null;
    this.volumeCallbacks = [];
    this.lastVolume = 0;

    this.stream?.getTracks().forEach(t => t.stop());
    this.stream = null;
    this.recorder?.stop();
    this.recorder = null;

    this.chunks = [];
    this.completeChunks = [];        // Reset full recording buffer
    this.temporaryChunks = [];     // Reset temporary buffer
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

// ============================================================================
// TTS Manager
// ============================================================================

export class TTSManager {
  private player: HTMLAudioElement | null = null;
  private audioUrl: string | null = null;
  private callbacks: SpeechCallbacks = {};

  async start(text: string, language: string = getSpeechLanguage()): Promise<void> {
    this.stop();
    this.setState('speaking');

    try {
      const audioBlob = await synthesize(text, language);
      this.setupPlayer(audioBlob);
      await this.playAudio();
      this.setupFallbackCleanup(text);
    } catch (err) {
      this.notifyError(err as SpeechError);
      this.setState('error');
    }
  }

  stop(): void {
    this.player?.pause();
    this.cleanup();
    this.setState('idle');
  }

  destroy(): void {
    this.stop();
    this.callbacks = {};
  }

  synthesizeSpeech(text: string, language: string = getSpeechLanguage()): Promise<Blob> {
    return synthesize(text, language);
  }

  // --------------------------------------------------------------------------
  // Private Methods
  // --------------------------------------------------------------------------

  private setupPlayer(blob: Blob): void {
    this.player = new Audio();
    this.audioUrl = URL.createObjectURL(blob);
    this.player.src = this.audioUrl;

    this.player.onended = () => { this.cleanup(); this.setState('idle'); };
    this.player.onerror = () => {
      this.notifyError(new SpeechError('Playback error. Try again.', 'PLAYBACK_ERROR'));
      this.setState('error');
      this.cleanup();
    };
  }

  private async playAudio(): Promise<void> {
    const playPromise = this.player?.play();
    if (playPromise) {
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

  private setupFallbackCleanup(text: string): void {
    setTimeout(() => {
      if (this.player && !this.player.ended) {
        this.player.onended = null;
        this.cleanup();
      }
    }, text.length * SPEECH_CONFIG.AUDIO_TIMEOUT_MS);
  }

  private setState(state: SpeechState): void {
    this.callbacks.onStateChange?.(state);
  }

  private notifyError(error: SpeechError): void {
    console.error('[TTS] Error:', error);
    this.callbacks.onError?.(error);
  }

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
// Speaker Button
// ============================================================================

export interface SpeakerButtonController {
  stop(): void
  setDisabled(disabled: boolean): void
}

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
