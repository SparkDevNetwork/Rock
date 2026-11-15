// The focus trap: the one home for keeping Tab inside a surface.
export function onTrapKeydown(event: KeyboardEvent, keep: () => void): void {
    if (event.key === "Tab") {
        keep();
    }
}
