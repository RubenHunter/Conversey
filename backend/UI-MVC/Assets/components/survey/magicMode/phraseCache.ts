/**
 * Magic Mode Phrase Cache
 * Manages client-side caching of AI phrase extraction results.
 *
 * Provides LRU-like eviction when the cache exceeds its maximum size,
 * removing oldest entries first to maintain performance.
 */
import type { RejectedPhrase } from './types';

/**
 * Entry stored in the phrase cache.
 * Contains the extracted phrases and any rejected phrases with reasons.
 */
export interface PhraseCacheEntry {
    phrases: string[];
    rejected: RejectedPhrase[];
}

/**
 * Manages caching of AI phrase extraction results.
 * Provides get/set/clear operations with automatic cleanup when max size is exceeded.
 */
export interface PhraseCache {
    /**
     * Get cached result for a key.
     * @param key - The cache key to look up
     * @returns The cached entry or undefined if not found
     */
    get(key: string): PhraseCacheEntry | undefined;
    
    /**
     * Store a result in cache.
     * @param key - The cache key
     * @param entry - The entry to store
     */
    set(key: string, entry: PhraseCacheEntry): void;
    
    /**
     * Check if a key exists in the cache.
     * @param key - The cache key to check
     * @returns true if the key exists in the cache
     */
    has(key: string): boolean;
    
    /**
     * Remove oldest entries if cache exceeds max size.
     * Uses FIFO eviction, removing entries from the oldest half.
     */
    cleanup(): void;
    
    /**
     * Clear all cached entries.
     */
    clear(): void;
    
    /**
     * Get the current number of entries in the cache.
     */
    readonly size: number;
    
    /**
     * Maximum number of entries the cache can hold.
     */
    readonly maxSize: number;
}

/**
 * Options for creating a phrase cache.
 */
export interface PhraseCacheOptions {
    /** Maximum number of entries to keep in the cache. Default: 100 */
    maxSize?: number;
}

/**
 * Creates a new phrase cache with LRU-like eviction.
 * 
 * @param options - Cache configuration options
 * @returns A new PhraseCache instance
 */
export function createPhraseCache(options: PhraseCacheOptions = {}): PhraseCache {
    const { maxSize = 100 } = options;
    const cache = new Map<string, PhraseCacheEntry>();
    
    return {
        get(key: string): PhraseCacheEntry | undefined {
            return cache.get(key);
        },
        
        set(key: string, entry: PhraseCacheEntry): void {
            cache.set(key, entry);
        },
        
        has(key: string): boolean {
            return cache.has(key);
        },
        
        cleanup(): void {
            if (cache.size > maxSize) {
                const keys = Array.from(cache.keys());
                // Remove oldest entries (first half of excess)
                for (let i = 0; i < keys.length - (maxSize / 2); i++) {
                    cache.delete(keys[i]);
                }
            }
        },
        
        clear(): void {
            cache.clear();
        },
        
        get size(): number {
            return cache.size;
        },
        
        get maxSize(): number {
            return maxSize;
        }
    };
}

/**
 * Generates a cache key from phrase extraction parameters.
 * Normalizes all inputs by trimming, lowercasing, and sorting arrays
 * to ensure consistent cache hits regardless of input formatting.
 * 
 * @param transcript - The transcribed text
 * @param language - The language code
 * @param maxPhrases - Maximum number of phrases to extract
 * @param existingPhrases - Phrases already displayed in the modal
 * @param rejectedPhrases - Phrases that were previously rejected
 * @returns A normalized cache key string
 */
export function generateCacheKey(
    transcript: string,
    language: string,
    maxPhrases: number,
    existingPhrases: string[],
    rejectedPhrases: string[]
): string {
    const normalizedTranscript = transcript.trim().toLowerCase();
    const normalizedExisting = existingPhrases
        .map(p => p.trim().toLowerCase())
        .sort()
        .join('|');
    const normalizedRejected = rejectedPhrases
        .map(p => p.trim().toLowerCase())
        .sort()
        .join('|');
    return `${normalizedTranscript}|${language}|${maxPhrases}|${normalizedExisting}|${normalizedRejected}`;
}
