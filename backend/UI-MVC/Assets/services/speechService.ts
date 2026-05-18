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
} from './interfaces';

// Re-export default implementations
export {
    defaultAudioRecorderFactory,
    defaultAudioPlayerFactory,
    defaultAudioContextFactory,
    defaultStreamService,
} from './interfaces';

// Re-export dependency injection types for managers
export type { STTManagerDependencies } from './sttManager';
export type { TTSManagerDependencies } from './ttsManager';

// Re-export pure functions for testability
export {
    generateListeningPlaceholder,
    generateProcessingPlaceholder,
    getRequiredRecordingDuration,
    getRecorderMimeType,
    meetsMinimumAudioSize,
} from './sttManager';

export { calculateAudioTimeout } from './ttsManager';

// Re-export types and constants
export {
  PRIORITY_MIME_TYPES,
  SpeechError,
} from './speechTypes';

export type {
  SpeechState,
  SpeechCallbacks,
} from './speechTypes';

// Re-export configuration
export { SPEECH_CONFIG, getSpeechLanguage } from './speechConfig';

// Re-export utilities and API types
export { toBase64, getBestMimeType, transcribe, synthesize } from './speechUtils';
export type { SynthesizeRequestBody, TranscribeResponse } from './speechUtils';

// Re-export buffer manager
export { createBufferManager, type BufferManager } from './bufferManager';

// Re-export managers
export { STTManager } from './sttManager';
export { TTSManager } from './ttsManager';

// Re-export factories
export {
  getSTTManager,
  getTTSManager,
  createSpeakerButton,
  bindMicButton,
  type SpeakerButtonController,
} from './speechFactories';
