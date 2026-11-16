export const searchPause = 0;
export type LatestSearch<T> = { query: string, results: T | null, busy: boolean, error: unknown, set: (query: string, options?: { isAtOnce?: boolean }) => void, cancel: () => void, stop: () => void };
export function useLatestSearch<T>(_fetch: (query: string, signal: AbortSignal) => Promise<T>, _options: { pause?: number, isSearchable?: (query: string) => boolean, onAnswer?: (answer: T, query: string) => void } = {}): LatestSearch<T> {
    return { query: "", results: null, busy: false, error: null, set: () => undefined, cancel: () => undefined, stop: () => undefined };
}
