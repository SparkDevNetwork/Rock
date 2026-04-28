/**
 * Directive that adds a tooltip to an element when its content is truncated with ellipsis.
 *
 * @example
 * <div v-ellipsis-tooltip>
 *     This is some long text that may be truncated.
 * </div>
 */
import { Directive } from "vue";
import { destroyTooltip, tooltip } from "@Obsidian/Utility/tooltip";

function updateTooltip(el: HTMLElement): void {
    ensureEllipsisStyles(el);

    const isOverflowing = el.scrollWidth > el.clientWidth;
    const text = el.textContent ?? "";

    const currentTitle = el.getAttribute("data-original-title") ?? "";
    const hasTooltip = currentTitle.length > 0;

    if (isOverflowing) {
        if (!hasTooltip || currentTitle !== text) {
            el.setAttribute("data-original-title", text);
            tooltip(el);
        }
    }
    else if (hasTooltip) {
        el.removeAttribute("data-original-title");
        destroyTooltip(el);
    }
}

function ensureEllipsisStyles(el: HTMLElement): void {
    const style = el.style;

    if (!style.whiteSpace) {
        style.whiteSpace = "nowrap";
    }

    if (!style.overflow) {
        style.overflow = "hidden";
    }

    if (!style.textOverflow) {
        style.textOverflow = "ellipsis";
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

        el._ellipsisResizeObserver = resizeObserver;
        el._ellipsisMutationObserver = mutationObserver;
    },

    updated(el) {
        updateTooltip(el);
    },

    unmounted(el) {
        el._ellipsisResizeObserver?.disconnect();
        el._ellipsisMutationObserver?.disconnect();
    }
};
