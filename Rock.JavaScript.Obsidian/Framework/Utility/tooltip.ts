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

import { getUniqueCssSelector } from "./dom";
import { getFullscreenElement, isFullscreen } from "./fullscreen";

// NOTE: Do not make this public yet. This is essentially temporary and
// will likely move to a different place and be merged with the popover
// concept code as well.
type TooltipOptions = {
    /** The container element to place the tooltip in. */
    container?: string;

    /** Allow HTML content in the tooltip. */
    html?: boolean;

    /** Enables santization of HTML content. */
    sanitize?: boolean;

    /** The delay before showing/hiding the tooltip. */
    delay?: number | {
        /** The delay before showing the tooltip. */
        show?: number;

        /** The delay before hiding the tooltip. */
        hide?: number;
    };
};

// eslint-disable-next-line @typescript-eslint/no-explicit-any
declare const $: any;

/**
 * Configure a tooltip for the specified node or nodes to show on hover. This
 * currently uses Bootstrap tooltips but may be changed to use a different
 * method later.
 *
 * @param node The node or nodes to have tooltips configured on.
 * @param options The options that describe how the tooltips should behave.
 */
export function tooltip(node: Element | Element[], options?: TooltipOptions): void {
    // If we got an array of elements then activate each one.
    if (Array.isArray(node)) {
        for (const n of node) {
            tooltip(n, options);
        }

        return;
    }

    if (typeof $ !== "function") {
        return;
    }

    const $node = $(node);
    let appliedContainer: string | undefined = undefined;
    let fsElement: HTMLElement | undefined = undefined;

    const getContainer = (): string | undefined => {
        if (!isFullscreen() && fsElement) {
            // No long in fullscreen mode, so reset everything.
            fsElement.style.position = "";
            fsElement = undefined;

            return options?.container ?? "body";
        }

        const newFsElement = getFullscreenElement();

        if (newFsElement && newFsElement === fsElement) {
            // No change
            return undefined;
        }

        if (newFsElement && newFsElement.contains(node)) {
            // Newly in fullscreen mode, use the fullscreen element
            fsElement = newFsElement as HTMLElement;
            const fsElementPosition = getComputedStyle(fsElement).position;

            if (fsElementPosition === "static") {
                fsElement.style.position = "relative";
            }

            return getUniqueCssSelector(fsElement);
        }

        return options?.container ?? "body";
    };

    const applyTooltip = (container: string | undefined): void => {
        // Already applied to this container, or no container provided so don't do anything.
        if (appliedContainer === container || container === undefined) {
            return;
        }

        if (appliedContainer) {
            $node?.tooltip("destroy");

            setTimeout(() => {
                $node?.tooltip({
                    container: container,
                    html: options?.html,
                    sanitize: options?.sanitize ?? true,
                    delay: options?.delay,
                    trigger: "hover focus"
                });

            }, 151);
        }
        else {
            $node?.tooltip({
                container: container,
                html: options?.html,
                sanitize: options?.sanitize ?? true,
                delay: options?.delay,
                trigger: "hover focus"
            });
        }

        appliedContainer = container;
    };

    // When we attach to the body/html element, we need to change the container when we're in fullscreen mode.
    if (!options?.container || options.container === "body" || options.container === "html") {
        document.addEventListener("fullscreenchange", () => applyTooltip(getContainer()));
    }

    applyTooltip(getContainer());

    $(document).on("click", ".tooltip", function(e: Event) {
        e.stopPropagation();
    });

    $node?.on("mouseleave", function(this: Element) {
        if (!$(this).data("tooltip-pinned")) {
            $node?.tooltip("hide");
        }
    });
}

/**
 * Manually show a previously-configured tooltip for the specified node.
 *
 * @param node The node for which to show a tooltip
 */
export function showTooltip(node: Element): void {
    if (typeof $ === "function") {
        $(node).tooltip("show");
        $(node).data("tooltip-pinned", true);
    }
}

/**
 * Manually hide a previously-configured tooltip for the specified node.
 *
 * @param node The node for which to hide a tooltip
 */
export function hideTooltip(node: Element): void {
    if (typeof $ === "function") {
        $(node).data("tooltip-pinned", false);
        $(node).tooltip("hide");
    }
}

/**
 * Manually destroy a previously-configured tooltip for the specified node.
 *
 * @param node The node for which to destroy a tooltip.
 */
export function destroyTooltip(node: Element | Element[]): void {
    if (typeof $ === "function") {
        $(node).tooltip("destroy");
    }
}

/**
 * Reset a tooltip by destroying the existing one and adding a new tooltip on
 *
 * @param node The node or nodes to have tooltips configured on.
 * @param options The options that describe how the tooltips should behave.
 */
export function resetTooltip(node: Element | Element[], options?: TooltipOptions): Promise<void> {
    return new Promise(res => {
        destroyTooltip(node);

        setTimeout(() => {
            tooltip(node, options);
            res();
        }, 151);
    });
}