/**
 * Buffer Manager for STT Dual Buffer System
 * 
 * Manages two audio buffers for speech-to-text:
 * - completeChunks: All audio chunks for final transcription
 * - temporaryChunks: Last N chunks + header chunk for real-time feedback
 * 
 * NOTE: First chunk (index 0) in temporaryChunks contains encryption metadata
 * for Mistral Voxtral and MUST NOT be removed.
 */

/**
 * Manages dual audio buffers for STT.
 * 
 * The dual buffer system allows for:
 * - completeChunks: Stores ALL audio chunks for final, accurate transcription
 * - temporaryChunks: Stores header chunk + last N chunks for real-time feedback
 * 
 * The header chunk (index 0) in temporaryChunks contains encryption metadata
 * for Mistral Voxtral and MUST NOT be removed during trimming.
 */
export interface BufferManager {
    /**
     * Add a new audio chunk to both buffers.
     * Complete chunks are stored for final transcription.
     * Temporary chunks are stored for real-time feedback, with automatic trimming
     * to maintain the window size while preserving the header chunk at index 0.
     * 
     * @param chunk - The audio blob chunk to add
     */
    addChunk(chunk: Blob): void;
    
    /**
     * Get all chunks for final transcription.
     * Returns the complete buffer containing all audio recorded.
     * 
     * @returns Array of all audio chunks
     */
    getCompleteBuffer(): Blob[];
    
    /**
     * Get temporary chunks for real-time feedback.
     * Returns the temporary buffer including the header chunk at index 0
     * and the most recent N chunks.
     * 
     * @returns Array of temporary audio chunks (header + last N chunks)
     */
    getTemporaryBuffer(): Blob[];
    
    /**
     * Reset both buffers.
     * Clears all chunks from both complete and temporary buffers.
     */
    reset(): void;
    
    /**
     * Header chunk is always at this index in temporaryChunks.
     * This chunk contains encryption metadata for Mistral Voxtral.
     */
    readonly headerChunkIndex: number;
    
    /**
     * Number of recent chunks to keep in temporary buffer (excluding header).
     */
    readonly windowSize: number;
}

/**
 * Creates a buffer manager for STT dual buffer system.
 * 
 * @param windowSize - Number of recent chunks to keep for real-time feedback
 * @returns A new BufferManager instance
 */
export function createBufferManager(windowSize: number): BufferManager {
    // Complete buffer: all chunks for final transcription
    const completeChunks: Blob[] = [];
    
    // Temporary buffer: header chunk + last N chunks for real-time feedback
    // Index 0 contains encryption metadata and MUST NOT BE REMOVED
    const temporaryChunks: Blob[] = [];
    
    return {
        headerChunkIndex: 0,
        windowSize,
        
        addChunk(chunk: Blob): void {
            // Add to both buffers
            completeChunks.push(chunk);
            temporaryChunks.push(chunk);
            
            // Keep header chunk at index 0 (contains encryption metadata for Mistral Voxtral)
            // Remove oldest non-header chunk to maintain window of N recent chunks
            if (temporaryChunks.length > windowSize + 1) {
                temporaryChunks.splice(1, 1);
            }
        },
        
        getCompleteBuffer(): Blob[] {
            return [...completeChunks];
        },
        
        getTemporaryBuffer(): Blob[] {
            return [...temporaryChunks];
        },
        
        reset(): void {
            completeChunks.length = 0;
            temporaryChunks.length = 0;
        }
    };
}
