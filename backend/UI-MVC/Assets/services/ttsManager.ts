/**
 * TTS Manager
 * Manages text-to-speech synthesis and audio playback.
 * 
 * Converts text to speech using the Mistral Voxtral API and plays the resulting
 * audio through an HTMLAudioElement. Handles playback state, errors, and cleanup.
 * 
 * Features:
 * - Text-to-speech synthesis via API
 * - Audio playback with event handling
 * - Automatic cleanup on stop/destroy
 * - Dependency injection for improved testability
 */
import { SPEECH_CONFIG } from '../config/speechConfig';
import { getSpeechLanguage } from '../config/speechConfig';
import { synthesize } from './speechUtils';
import { SpeechError } from './speechTypes';
import type { SpeechState, SpeechCallbacks } from './speechTypes';
import type {
    IAudioPlayer,
    IAudioPlayerFactory,
    ISynthesizeService,
} from './interfaces';
import {
    defaultAudioPlayerFactory,
} from './interfaces';

/**
 * Dependencies for TTSManager that can be injected for testing.
 */
export interface TTSManagerDependencies {
    /** Factory for creating audio players (default: browser Audio) */
    audioPlayerFactory?: IAudioPlayerFactory;
    /** Service for synthesis (default: uses synthesize from speechUtils) */
    synthesizeService?: ISynthesizeService;
}

/**
 * Pure function: calculates audio timeout based on text length.
 * Extracted for testability.
 */
export function calculateAudioTimeout(textLength: number): number {
    return textLength * SPEECH_CONFIG.AUDIO_TIMEOUT_MS;
}

/**
 * Manages text-to-speech synthesis and audio playback.
 */
export class TTSManager {
  private player: IAudioPlayer | null = null;
  private audioUrl: string | null = null;
  private callbacks: SpeechCallbacks = {};

  // Injected dependencies
  private readonly audioPlayerFactory: IAudioPlayerFactory;
  private readonly synthesizeService: ISynthesizeService;

  /**
   * Creates a new TTSManager instance.
   * 
   * @param dependencies - Optional dependencies for testing. All have backward-compatible defaults.
   */
  constructor(dependencies?: TTSManagerDependencies) {
    // Use injected dependencies or defaults
    this.audioPlayerFactory = dependencies?.audioPlayerFactory ?? defaultAudioPlayerFactory;
    this.synthesizeService = dependencies?.synthesizeService ?? { synthesize: synthesize };
  }

  /**
   * Sets up event callbacks for the TTS manager.
   * Merges with existing callbacks.
   * @param callbacks - Callback functions for state changes and errors
   */
  setupCallbacks(callbacks: SpeechCallbacks): void {
    this.callbacks = { ...this.callbacks, ...callbacks };
  }

  /**
   * Converts text to speech and plays the audio.
   * 
   * @param text - The text to synthesize into speech
   * @param language - Language code for synthesis (default: detected from browser)
   */
  async start(text: string, language: string = getSpeechLanguage()): Promise<void> {
    this.stop();
    this.setState('speaking');

    try {
      const audioBlob = await this.synthesizeService.synthesize(text, language);
      this.setupPlayer(audioBlob);
      await this.playAudio();
      this.setupFallbackCleanup(text);
    } catch (err) {
      this.notifyError(err as SpeechError);
      this.setState('error');
    }
  }

  /**
   * Stops audio playback immediately.
   * Pauses the player, cleans up resources, and sets state to idle.
   */
  stop(): void {
    (this.player as any)?.pause?.();
    this.cleanup();
    this.setState('idle');
  }

  /**
   * Destroys the TTS manager and cleans up all resources.
   * Stops playback and removes callbacks.
   */
  destroy(): void {
    this.stop();
    this.callbacks = {};
  }

  /**
   * Synthesizes text to speech and returns the audio blob.
   * Does not play the audio - use start() for playback.
   * 
   * @param text - The text to synthesize
   * @param language - Language code for synthesis (default: detected from browser)
   * @returns Promise resolving to the synthesized audio Blob
   */
  synthesizeSpeech(text: string, language: string = getSpeechLanguage()): Promise<Blob> {
    return this.synthesizeService.synthesize(text, language);
  }

  // --------------------------------------------------------------------------
  // Private Methods
  // --------------------------------------------------------------------------

  private setupPlayer(blob: Blob): void {
    this.player = this.audioPlayerFactory();
    this.audioUrl = URL.createObjectURL(blob);
    (this.player as any).src = this.audioUrl;

    (this.player as any).onended = () => { this.cleanup(); this.setState('idle'); };
    (this.player as any).onerror = () => {
      this.notifyError(new SpeechError('Playback error. Try again.', 'PLAYBACK_ERROR'));
      this.setState('error');
      this.cleanup();
    };
  }

  private async playAudio(): Promise<void> {
    const playPromise = (this.player as any)?.play?.();
    if (playPromise) {
      await playPromise.catch((err: unknown) => {
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
      if (this.player && !(this.player as any).ended) {
        (this.player as any).onended = null;
        this.cleanup();
      }
    }, calculateAudioTimeout(text.length));
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
      (this.player as any).onended = null;
      (this.player as any).onerror = null;
      this.player = null;
    }
  }
}
