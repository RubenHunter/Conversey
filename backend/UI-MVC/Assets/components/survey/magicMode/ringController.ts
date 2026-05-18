/**
 * Magic Mode Ring Controller
 * Manages status ring animations for STT and AI processing states.
 *
 * Provides visual feedback through animated CSS rings that indicate:
 * - Transcription in progress (slower orbit)
 * - AI thinking in progress (faster orbit)
 * - AI completion (360° orbit + glow animation)
 * - Recording state (styling changes)
 */

/**
 * Controller for managing status ring animations.
 * Handles DOM element creation, state management, and CSS class toggling
 * for transcription and AI processing visual feedback.
 */
export interface RingController {
    /** The root ring container element */
    readonly element: HTMLElement;
    
    /**
     * Start the transcription ring animation.
     * Adds 'transcribing' class to the transcript wrapper.
     */
    startTranscribing(): void;
    
    /**
     * Start the AI thinking ring animation.
     * Adds 'thinking' class to the AI wrapper.
     */
    startThinking(): void;
    
    /**
     * Trigger AI completion animation.
     * Adds 'ai-complete' class for a brief 360° orbit + glow effect,
     * then removes it after 1000ms.
     */
    completeAI(): void;
    
    /**
     * Stop all ring animations.
     * Removes all state classes from both wrappers.
     */
    stopAll(): void;
    
    /**
     * Set recording state for both rings.
     * Adds/removes 'recording' class based on the state.
     * @param isRecording - Whether recording is currently active
     */
    setRecordingState(isRecording: boolean): void;
    
    /**
     * Clean up DOM elements and internal references.
     * Removes all classes and nullifies element references.
     */
    destroy(): void;
}

/**
 * Creates a new RingController instance.
 * Initializes the DOM structure for the status rings with transcript
 * (slower orbit) and AI (faster orbit) wrappers.
 *
 * @returns A new RingController instance
 */
export function createRingController(): RingController {
    // DOM elements
    let transcriptWrapperEl: HTMLElement | null = null;
    let aiWrapperEl: HTMLElement | null = null;
    
    // State
    let transcriptInProgress = false;
    let aiInProgress = false;
    let isRecording = false;
    
    // Create the ring container with both wrappers
    const ringContainer = document.createElement('div');
    ringContainer.className = 'magic-mode-ring-container';
    
    // Transcript wrapper - slower orbit
    const transcriptWrapper = document.createElement('div');
    transcriptWrapper.className = 'magic-mode-ring-wrapper magic-mode-transcript-wrapper';
    
    // AI wrapper - faster orbit
    const aiWrapper = document.createElement('div');
    aiWrapper.className = 'magic-mode-ring-wrapper magic-mode-ai-wrapper';
    
    // Ring elements
    const transcriptRing = document.createElement('div');
    transcriptRing.className = 'magic-mode-status-ring magic-mode-transcript-ring';
    
    const aiRing = document.createElement('div');
    aiRing.className = 'magic-mode-status-ring magic-mode-ai-ring';
    
    // Assemble hierarchy
    transcriptWrapper.appendChild(transcriptRing);
    aiWrapper.appendChild(aiRing);
    ringContainer.append(transcriptWrapper, aiWrapper);
    
    // Store references
    transcriptWrapperEl = transcriptWrapper;
    aiWrapperEl = aiWrapper;
    
    /**
     * Updates CSS classes on ring wrappers based on current state.
     * Called internally by state-changing methods.
     */
    function updateRingState(): void {
        if (transcriptWrapperEl) {
            transcriptWrapperEl.classList.toggle('transcribing', transcriptInProgress);
            transcriptWrapperEl.classList.toggle('recording', isRecording);
        }
        if (aiWrapperEl) {
            aiWrapperEl.classList.toggle('thinking', aiInProgress);
            aiWrapperEl.classList.toggle('recording', isRecording);
        }
    }
    
    return {
        element: ringContainer,
        
        startTranscribing(): void {
            transcriptInProgress = true;
            aiInProgress = false;
            updateRingState();
        },
        
        startThinking(): void {
            transcriptInProgress = false;
            aiInProgress = true;
            updateRingState();
        },
        
        completeAI(): void {
            // Trigger completion animation on AI wrapper
            if (aiWrapperEl) {
                aiWrapperEl.classList.add('ai-complete');
                setTimeout(() => aiWrapperEl?.classList.remove('ai-complete'), 1000);
            }
            aiInProgress = false;
            updateRingState();
        },
        
        stopAll(): void {
            transcriptInProgress = false;
            aiInProgress = false;
            updateRingState();
        },
        
        setRecordingState(isRecordingState: boolean): void {
            isRecording = isRecordingState;
            updateRingState();
        },
        
        destroy(): void {
            transcriptInProgress = false;
            aiInProgress = false;
            isRecording = false;
            updateRingState();
            
            // Remove recording class from wrappers
            transcriptWrapperEl?.classList.remove('recording');
            aiWrapperEl?.classList.remove('recording');
            
            // Clear references
            transcriptWrapperEl = null;
            aiWrapperEl = null;
        }
    };
}
