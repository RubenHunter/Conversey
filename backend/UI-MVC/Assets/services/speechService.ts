/**
 * Speech Service
 * Main entry point for speech recognition and synthesis services.
 * 
 * Re-exports all speech-related functionality from specialized modules:
 * - STTManager and TTSManager classes with dependency injection
 * - Factory functions for creating and managing instances
 * - Type definitions and configuration
 * - Buffer management for STT dual buffer system
 * - Dependency injection interfaces for testability
 * 
 * Note: First chunk (index 0) in STT temporary buffer contains encryption metadata
 * for Mistral Voxtral and MUST NOT be removed.
 */

// Re-export dependency injection interfaces
export type {
    IAudioRecorder,
    IAudioRecorderFactory,
    IAudioPlayer,
    IAudioPlayerFactory,
    IAudioContext,
    IAudioContextFactory,
    IAnalyserNode,
    IMediaStreamAudioSourceNode,
    IStreamService,
    ITranscribeService,
    ISynthesizeService,
} from './speech/interfaces';

// Re-export default implementations
export {
    defaultAudioRecorderFactory,
    defaultAudioPlayerFactory,
    defaultAudioContextFactory,
    defaultStreamService,
} from './speech/interfaces';

// Re-export dependency injection types for managers
export type { STTManagerDependencies } from './speech/sttManager';
export type { TTSManagerDependencies } from './speech/ttsManager';

// Re-export pure functions for testability
export {
    generateListeningPlaceholder,
    generateProcessingPlaceholder,
    getRequiredRecordingDuration,
    getRecorderMimeType,
    meetsMinimumAudioSize,
} from './speech/sttManager';

export { calculateAudioTimeout } from './speech/ttsManager';

// Re-export types and constants
export {
  PRIORITY_MIME_TYPES,
  SpeechError,
} from './speech/speechTypes';

export type {
  SpeechState,
  SpeechCallbacks,
} from './speech/speechTypes';

// Re-export configuration
export { SPEECH_CONFIG, getSpeechLanguage } from '../config/speechConfig';

// Re-export utilities and API types
export { toBase64, getBestMimeType, transcribe, synthesize } from './speech/speechUtils';
export type { SynthesizeRequestBody, TranscribeResponse } from './speech/speechUtils';

// Re-export buffer manager
export { createBufferManager, type BufferManager } from './speech/bufferManager';

// Re-export managers
export { STTManager } from './speech/sttManager';
export { TTSManager } from './speech/ttsManager';

// Re-export factories
export {
  getSTTManager,
  getTTSManager,
  createSpeakerButton,
  bindMicButton,
  type SpeakerButtonController,
} from './speech/speechFactories';
