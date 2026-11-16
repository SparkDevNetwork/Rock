export type SeenObserver = { observeAll: (container: HTMLElement, selector: string) => void };
export function useSeenObserver(_root: () => HTMLElement | null, _onSeen: (element: HTMLElement) => void): SeenObserver {
    return { observeAll: () => undefined };
}
