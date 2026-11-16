export type ListKeys<K> = { itemAttrs: (key: K) => Record<string, unknown>, itemRef: (key: K) => (element: unknown) => void };
export function useListKeys<K>(_items: () => readonly K[], _options: { onPick?: (key: K) => void } = {}): ListKeys<K> {
    return { itemAttrs: () => ({}), itemRef: () => () => undefined };
}
