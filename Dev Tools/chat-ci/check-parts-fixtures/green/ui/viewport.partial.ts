// The viewport part: the one home for reading a media query.
export function useMediaFlag(query: string): boolean {
    return window.matchMedia?.(query).matches ?? false;
}
