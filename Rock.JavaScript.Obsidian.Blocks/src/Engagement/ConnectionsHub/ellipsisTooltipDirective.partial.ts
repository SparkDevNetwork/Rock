/* eslint-disable @typescript-eslint/no-explicit-any */
import { Directive } from "vue";
import { destroyTooltip, tooltip } from "@Obsidian/Utility/tooltip";

function updateTooltip(el: HTMLElement): void {
    const isOverflowing = el.scrollWidth > el.clientWidth;
    const text = el.textContent ?? "";

    const currentTitle = el.getAttribute("data-original-title") ?? "";
    const hasTooltip = currentTitle.length > 0;

    if (isOverflowing) {
        if (!hasTooltip) {
            el.setAttribute("data-original-title", text);
            tooltip(el);
        }
        else if (currentTitle !== text) {
            el.setAttribute("data-original-title", text);
            tooltip(el);
        }
    }
    else if (hasTooltip) {
        el.removeAttribute("data-original-title");
        destroyTooltip(el);
    }
}

export const ellipsisTooltip: Directive = {
    mounted(el) {
        updateTooltip(el);

        const resizeObserver = new ResizeObserver(() => {
            updateTooltip(el);
        });

        resizeObserver.observe(el);

        const mutationObserver = new MutationObserver(() => {
            updateTooltip(el);
        });

        mutationObserver.observe(el, {
            characterData: true,
            childList: true,
            subtree: true
        });

        (el as any)._ellipsisResizeObserver = resizeObserver;
        (el as any)._ellipsisMutationObserver = mutationObserver;
    },

    updated(el) {
        updateTooltip(el);
    },

    unmounted(el) {
        (el as any)._ellipsisResizeObserver?.disconnect();
        (el as any)._ellipsisMutationObserver?.disconnect();
    }
};
