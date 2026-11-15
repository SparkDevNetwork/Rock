// The popover part: the one home for placing a floating surface and closing it on a press outside.
import { autoUpdate, flip, offset, shift, useFloating } from "@Obsidian/Libs/floating-ui";

export function usePopover(onOutside: (event: PointerEvent) => void): () => void {
    document.addEventListener("pointerdown", onOutside, true);
    return () => document.removeEventListener("pointerdown", onOutside, true);
}
