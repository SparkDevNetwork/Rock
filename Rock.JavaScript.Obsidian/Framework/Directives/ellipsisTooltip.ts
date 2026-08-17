// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//

/**
 * Directive that adds a tooltip to an element only when its content is truncated
 * with an ellipsis, and removes it again when the content fits.
 *
 * @example
 * <div v-ellipsis-tooltip>
 *     This is some long text that may be truncated.
 * </div>
 */
import { Directive } from "vue";
import { destroyTooltip, tooltip } from "@Obsidian/Utility/tooltip";

/**
 * The observers watching an element, kept off the element itself so the element's
 * type is not augmented. Entries are removed when the element unmounts.
 */
type ElementObservers = {
    resizeObserver: ResizeObserver;
    mutationObserver: MutationObserver;
};

const observersByElement = new WeakMap<HTMLElement, ElementObservers>();

/**
 * Shows or hides the element's tooltip based on whether its text currently overflows.
 *
 * @param el The element to evaluate.
 */
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

/**
 * Applies the single-line truncation styles the directive relies on, without
 * overriding any the element already sets itself.
 *
 * @param el The element to style.
 */
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

export const vEllipsisTooltip: Directive<HTMLElement> = {
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

        observersByElement.set(el, { resizeObserver, mutationObserver });
    },

    updated(el) {
        updateTooltip(el);
    },

    unmounted(el) {
        const observers = observersByElement.get(el);

        if (observers) {
            observers.resizeObserver.disconnect();
            observers.mutationObserver.disconnect();
            observersByElement.delete(el);
        }
    }
};
