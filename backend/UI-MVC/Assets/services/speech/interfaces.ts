/**
 * Speech Service Interfaces
 * Dependency injection interfaces for improved testability.
 * 
 * These interfaces allow mocking browser APIs and external services
 * for unit testing without requiring actual browser environments.
 */

// ============================================================================
// Browser API Interfaces
// ============================================================================

/**
 * Interface for media recording capabilities.
 * Allows mocking MediaRecorder for testing.
 */
export interface IAudioRecorder {
    /** Start recording with the given timeslice */
    start(timeslice?: number): void;
    /** Stop recording */
    stop(): void;
    /** Pause recording */
    pause(): void;
    /** Resume recording */
    resume(): void;
    /** Request data from the recorder */
    requestData(): void;
    /** Event handler for dataavailable */
    ondataavailable: ((this: IAudioRecorder, ev: BlobEvent) => void) | null;
    /** Event handler for stop */
    onstop: ((this: IAudioRecorder, ev: Event) => void) | null;
    /** Event handler for error */
    onerror: ((this: IAudioRecorder, ev: Event) => void) | null;
    /** Event handler for start */
    onstart: ((this: IAudioRecorder, ev: Event) => void) | null;
    /** MIME type of the recording */
    readonly mimeType: string;
    /** State of the recorder */
    readonly state: 'inactive' | 'recording' | 'paused';
    /** Stream being recorded */
    readonly stream: MediaStream | null;
}

/**
 * Interface for audio playback capabilities.
 * Allows mocking HTMLAudioElement for testing.
 */
export interface IAudioPlayer {
    /** Source URL for the audio */
    src: string;
    /** Play the audio */
    play(): Promise<void>;
    /** Pause the audio */
    pause(): void;
    /** Stop the audio */
    stop?(): void;
    /** Event handler for ended */
    onended: (() => void) | null;
    /** Event handler for error */
    onerror: ((this: IAudioPlayer, ev: Event) => void) | null;
}

/**
 * Factory interface for creating audio recorders.
 * Allows mocking the MediaRecorder constructor.
 */
export interface IAudioRecorderFactory {
    (stream: MediaStream, options?: MediaRecorderOptions): IAudioRecorder;
}

/**
 * Factory interface for creating audio players.
 * Allows mocking the Audio constructor.
 */
export interface IAudioPlayerFactory {
    (src?: string): IAudioPlayer;
}

// ============================================================================
// Audio Analysis Interfaces
// ============================================================================

/**
 * Interface for audio context capabilities.
 * Allows mocking AudioContext for testing.
 */
export interface IAudioContext {
    /** Create an analyser node */
    createAnalyser(): IAnalyserNode;
    /** Create a media stream source node */
    createMediaStreamSource(stream: MediaStream): IMediaStreamAudioSourceNode;
    /** Close the context */
    close(): Promise<void>;
    /** Current time in seconds */
    readonly currentTime: number;
    /** State of the context */
    readonly state: 'suspended' | 'running' | 'closed';
}

/**
 * Interface for analyser node capabilities.
 * Allows mocking AnalyserNode for testing.
 */
export interface IAnalyserNode {
    /** Frequency data array */
    frequencyBinCount: number;
    /** Get frequency data */
    getByteFrequencyData(array: Uint8Array): void;
    /** Get time domain data */
    getByteTimeDomainData(array: Uint8Array): void;
    /** FFT size */
    fftSize: number;
    /** Min decimals */
    minDecibels: number;
    /** Max decimals */
    maxDecibels: number;
}

/**
 * Interface for media stream source node capabilities.
 */
export interface IMediaStreamAudioSourceNode {
    /** Connect to another node */
    connect(destination: IAnalyserNode): void;
    /** Disconnect */
    disconnect(): void;
}

/**
 * Factory interface for creating audio contexts.
 * Allows mocking AudioContext constructor.
 */
export interface IAudioContextFactory {
    (): IAudioContext;
}

// ============================================================================
// Transcription Service Interface
// ============================================================================

/**
 * Interface for transcription service.
 * Allows mocking the Mistral Voxtral transcription API.
 */
export interface ITranscribeService {
    /** Transcribe audio blob to text */
    transcribe(
        audio: Blob,
        language: string,
        contextBias?: string[]
    ): Promise<string>;
}

// ============================================================================
// Synthesis Service Interface
// ============================================================================

/**
 * Interface for text-to-speech synthesis service.
 * Allows mocking the Mistral Voxtral synthesis API.
 */
export interface ISynthesizeService {
    /** Synthesize text to speech audio */
    synthesize(text: string, language: string): Promise<Blob>;
}

// ============================================================================
// Stream Service Interface
// ============================================================================

/**
 * Interface for media stream service.
 * Allows mocking navigator.mediaDevices.getUserMedia.
 */
export interface IStreamService {
    /** Get user media stream */
    getUserMedia(constraints: MediaStreamConstraints): Promise<MediaStream>;
}

// ============================================================================
// Default Implementations (Browser)
// ============================================================================

/** Default audio recorder factory using browser MediaRecorder */
export const defaultAudioRecorderFactory: IAudioRecorderFactory = (
    stream: MediaStream,
    options?: MediaRecorderOptions
) => new MediaRecorder(stream, options) as unknown as IAudioRecorder;

/** Default audio player factory using browser Audio */
export const defaultAudioPlayerFactory: IAudioPlayerFactory = (src?: string) => {
    const audio = new Audio(src);
    return {
        src: audio.src,
        play: audio.play.bind(audio),
        pause: audio.pause.bind(audio),
        onended: null,
        onerror: null,
    } as IAudioPlayer;
};

/** Default audio context factory using browser AudioContext */
export const defaultAudioContextFactory: IAudioContextFactory = () => {
    return new (window.AudioContext || (window as any).webkitAudioContext)() as unknown as IAudioContext;
};

/** Default stream service using browser navigator.mediaDevices */
export const defaultStreamService: IStreamService = {
    getUserMedia: (constraints: MediaStreamConstraints) => {
        return navigator.mediaDevices.getUserMedia(constraints);
    },
};
