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

import { Directive } from "vue";
import { arrow, computePosition, flip, offset, shift } from "@Obsidian/Libs/floating-ui";

// This needs to match the CSS transition duration.
const animationDuration = 200;

const defaultShowDelay = 250;
const defaultHideDelay = 50;
const tooltipDataSymbol = Symbol("tooltipData");

/** Data associated with a tooltip instance. */
type TooltipData = {
    /** The element the tooltip is attached to. */
    element: Element;

    /** The floating tooltip element. */
    floatingElement: HTMLElement;

    /** The content element within the tooltip. */
    contentElement: HTMLElement;

    /** The arrow element within the tooltip. */
    arrowElement: HTMLElement;

    /** The mutation observer for content changes. */
    observer: MutationObserver;

    /**
     * The state of the hover. Bit 0 = target element, Bit 1 = tooltip element.
     */
    hoverState: number;

    /** The current state of the tooltip. */
    state: "none" | "enter-waiting" | "entering" | "entered" | "leave-waiting" | "leaving";

    /** The timer for show/hide delays and animations. */
    timer?: number;

    /** The configuration provided to the tooltip. */
    config: TooltipConfiguration;

    /** Whether the content contains HTML or plain text. */
    html: boolean;

    /**
     * Whether the popup will show right away or the configured
     * delay value will be used.
     */
    immediate: boolean;

    /**
     * Whether the tooltip should only be shown when the content of the element
     * it is attached to overflows. This is useful for showing tooltips on
     * truncated text.
     */
    overflow: boolean;
};

/** The content that can be displayed in a tooltip. */
type TooltipContent = string | HTMLElement | null | undefined;

/** Configuration options for a tooltip. */
export type TooltipConfiguration = {
    /** The content to display in the tooltip. */
    content: TooltipContent | (() => TooltipContent | Promise<TooltipContent>);

    /** Whether the content contains HTML or plain text. */
    html?: boolean;

    /**
     * Whether the popup will show right away or the configured
     * delay value will be used.
     */
    immediate?: boolean;

    /** Whether the tooltip should be displayed in a wide format. */
    wide?: boolean;

    /** Overrides the default show delay in milliseconds. */
    showDelay?: number;

    /** Overrides the default hide delay in milliseconds. */
    hideDelay?: number;

    /**
     * Whether the tooltip should only be shown when the content of the element
     * it is attached to overflows. This is useful for showing tooltips on
     * truncated text.
     */
    overflow?: boolean;
};

/**
 * Adds the tooltip element to the DOM and positions it relative to the target
 * element.
 *
 * @param data The tooltip data.
 */
function addAndPositionTooltip(data: TooltipData): void {
    if (!data.floatingElement.parentElement) {
        document.body.appendChild(data.floatingElement);
    }

    positionTooltip(data);
}

/**
 * Positions the tooltip element relative to the target element. This handles
 * automatic positioning to keep the tooltip within the viewport.
 *
 * @param data The tooltip data.
 */
function positionTooltip(data: TooltipData): void {
    computePosition(data.element, data.floatingElement, {
        placement: "top",
        middleware: [
            offset(0),
            flip(),
            shift({ padding: 8 }),
            arrow({ element: data.arrowElement })
        ],
    }).then(({ x, y, placement, middlewareData }) => {
        if (data.floatingElement) {
            Object.assign(data.floatingElement.style, {
                left: `${x}px`,
                top: `${y}px`,
            });
        }

        if (placement === "top") {
            data.floatingElement.classList.add("top");
            data.floatingElement.classList.remove("bottom");
        }
        else {
            data.floatingElement.classList.add("bottom");
            data.floatingElement.classList.remove("top");
        }

        if (data.arrowElement && middlewareData.arrow) {
            const { x: arrowX, y: arrowY } = middlewareData.arrow;

            Object.assign(data.arrowElement.style, {
                marginLeft: "initial", // Override legacy bootstrap styles that interfere with positioning.
                left: arrowX != null ? `${arrowX}px` : "",
                top: arrowY != null ? `${arrowY}px` : "",
            });
        }
    });
}

/**
 * Sets the content of the tooltip.
 *
 * @param data The tooltip data.
 * @param content The content to set.
 */
function setTooltipContent(data: TooltipData, content: TooltipContent): void {
    if (!content) {
        data.contentElement.innerHTML = "";
    }
    else if (content instanceof HTMLElement) {
        data.contentElement.innerHTML = "";
        data.contentElement.appendChild(content);
    }
    else if (data.html) {
        data.contentElement.innerHTML = content;
    }
    else {
        data.contentElement.textContent = content;
    }
}

/**
 * Updates the tooltip content, handling asynchronous content if needed.
 *
 * @param data The tooltip data.
 *
 * @returns True if content was set or is being loaded, false if no content
 * is available.
 */
function updateTooltipContent(data: TooltipData): boolean {
    let content: TooltipContent | Promise<TooltipContent>;

    if (typeof data.config.content === "function") {
        content = data.config.content();
    }
    else {
        content = data.config.content;
    }

    if (!!content && typeof content === "object" && "then" in content) {
        setTooltipContent(data, "Loading...");

        content.then(resolvedContent => {
            setTooltipContent(data, resolvedContent);
        });

        return true;
    }
    else {
        setTooltipContent(data, content);

        return !!content;
    }
}

/**
 * Handles mouse entering the tooltip or target element.
 *
 * @param event The mouse event.
 */
function onMouseEnter(event: MouseEvent): void {
    if (!event.currentTarget || !(event.currentTarget instanceof HTMLElement)) {
        return;
    }

    const data = event.currentTarget[tooltipDataSymbol] as TooltipData;

    if (data.overflow) {
        if (event.currentTarget.scrollWidth <= event.currentTarget.clientWidth) {
            return;
        }
    }

    const oldHoverState = data.hoverState;
    if (event.currentTarget === data.element) {
        data.hoverState |= 1;
    }
    else {
        data.hoverState |= 2;
    }

    if (oldHoverState !== 0) {
        return;
    }

    if (data.state === "none") {
        if (!updateTooltipContent(data)) {
            return;
        }

        data.observer.observe(data.contentElement, {
            childList: true,
            subtree: true,
            attributes: true,
            characterData: true,
        });

        data.state = "enter-waiting";

        addAndPositionTooltip(data);

        data.timer = window.setTimeout(() => {
            data.state = "entering";
            data.floatingElement.classList.add("in");

            data.timer = window.setTimeout(() => {
                data.state = "entered";
                data.timer = undefined;
            }, animationDuration);
        }, data.immediate ? 0 : data.config.showDelay ?? defaultShowDelay);
    }
    else if (data.state === "leave-waiting" || data.state === "leaving") {
        data.floatingElement.classList.add("in");
        data.state = "entered";

        if (data.timer !== undefined) {
            window.clearTimeout(data.timer);
            data.timer = undefined;
        }
    }
}

/**
 * Handles mouse leaving the tooltip or target element.
 *
 * @param event The mouse event.
 */
function onMouseLeave(event: MouseEvent): void {
    if (!event.currentTarget || !(event.currentTarget instanceof HTMLElement)) {
        return;
    }

    const data = event.currentTarget[tooltipDataSymbol] as TooltipData;

    // Don't bother checking for overflow here since if the tooltip was shown,
    // then the content must have been overflowing.

    const oldHoverState = data.hoverState;
    if (event.currentTarget === data.element) {
        data.hoverState &= ~1;
    }
    else {
        data.hoverState &= ~2;
    }

    if (oldHoverState === 0) {
        return;
    }

    if (data.state === "enter-waiting") {
        if (data.timer !== undefined) {
            window.clearTimeout(data.timer);
            data.timer = undefined;
            data.state = "none";
            data.observer.disconnect();

            data.floatingElement.remove();
            data.hoverState = 0;
        }
    }
    else if (data.state === "entering") {
        data.state = "leaving";
        data.floatingElement.classList.remove("in");

        if (data.timer !== undefined) {
            window.clearTimeout(data.timer);
        }

        data.timer = window.setTimeout(() => {
            data.state = "none";
            data.observer.disconnect();
            data.floatingElement.remove();
            data.hoverState = 0;
            data.timer = undefined;
        }, animationDuration);
    }
    else if (data.state === "entered") {
        data.state = "leave-waiting";

        data.timer = window.setTimeout(() => {
            data.state = "leaving";
            data.floatingElement.classList.remove("in");

            data.timer = window.setTimeout(() => {
                data.state = "none";
                data.observer.disconnect();
                data.floatingElement.remove();
                data.hoverState = 0;
                data.timer = undefined;
            }, animationDuration);
        }, data.immediate ? 0 : data.config.hideDelay ?? defaultHideDelay);

    }
}

/**
 * Handles content mutations within the tooltip.
 *
 * @param data The tooltip data.
 */
function onContentMutated(data: TooltipData): void {
    if (data.state !== "none") {
        positionTooltip(data);
    }
}

/**
 * Converts the provided value into a full tooltip configuration.
 *
 * @param value The value provided to the directive.
 *
 * @returns The tooltip configuration.
 */
function getConfiguration(value: TooltipContent | (() => TooltipContent | Promise<TooltipContent>) | TooltipConfiguration): TooltipConfiguration {
    if (value) {
        if (value instanceof HTMLElement) {
            return { content: value };
        }
        else if (typeof value === "object") {
            return value;
        }
        else {
            return { content: value };
        }
    }
    else {
        return { content: undefined };
    }
}

/**
 * Directive that adds a tooltip to an element.
 *
 * The directive value can be either a string/HTMLElement for simple content or an object for full configuration.
 *
 * The following modifiers are supported:
 * - `html`: Treat the content as HTML instead of plain text.
 * - `immediate`: Show and hide the tooltip immediately on hover without any delay.
 * - `wide`: Use a wider style for the tooltip, allowing more content to fit on a single line.
 */
// Once we update to Vue 3.5, we can add the modifiers to the type parameters for
// better type safety.
export const vTooltip: Directive<HTMLElement, TooltipContent | (() => TooltipContent | Promise<TooltipContent>) | TooltipConfiguration> = {
    beforeMount(el, binding) {
        const config = getConfiguration(binding.value);

        const floatingElement = document.createElement("div");
        floatingElement.classList.add("tooltip", "fade", "top");

        if (binding.modifiers.wide ?? config.wide) {
            floatingElement.classList.add("tooltip-wide");
        }

        const arrowElement = document.createElement("div");
        arrowElement.classList.add("tooltip-arrow");
        floatingElement.appendChild(arrowElement);

        const contentElement = document.createElement("div");
        contentElement.classList.add("tooltip-inner");
        floatingElement.appendChild(contentElement);

        const data: TooltipData = {
            element: el,
            floatingElement,
            contentElement,
            arrowElement,
            observer: new MutationObserver(() => onContentMutated(data)),
            hoverState: 0,
            state: "none",
            config,
            html: binding.modifiers.html ?? config.html ?? false,
            immediate: binding.modifiers.immediate ?? config.immediate ?? false,
            overflow: binding.modifiers.overflow ?? config.overflow ?? false,
        };

        floatingElement[tooltipDataSymbol] = data;
        floatingElement.addEventListener("mouseenter", onMouseEnter);
        floatingElement.addEventListener("mouseleave", onMouseLeave);

        el[tooltipDataSymbol] = data;
        el.addEventListener("mouseenter", onMouseEnter);
        el.addEventListener("mouseleave", onMouseLeave);
    },

    updated(el, binding) {
        const data = el[tooltipDataSymbol] as TooltipData;

        data.config = getConfiguration(binding.value);

        if (binding.modifiers.wide ?? data.config.wide) {
            data.floatingElement.classList.add("tooltip-wide");
        }
        else {
            data.floatingElement.classList.remove("tooltip-wide");
        }

        data.html = binding.modifiers.html ?? data.config.html ?? false;
        data.immediate = binding.modifiers.immediate ?? data.config.immediate ?? false;
        data.overflow = binding.modifiers.overflow ?? data.config.overflow ?? false;

        if (data.state !== "none") {
            if (!updateTooltipContent(data)) {
                if (data.timer !== undefined) {
                    window.clearTimeout(data.timer);
                    data.timer = undefined;
                }

                data.floatingElement.remove();
                data.hoverState = 0;
                data.state = "none";
            }
        }
    },

    beforeUnmount(el) {
        const data = el[tooltipDataSymbol] as TooltipData;

        data.floatingElement.remove();
        data.hoverState = 0;

        if (data.timer !== undefined) {
            window.clearTimeout(data.timer);
            data.timer = undefined;
        }

        data.observer.disconnect();

        el.removeEventListener("mouseenter", onMouseEnter);
        el.removeEventListener("mouseleave", onMouseLeave);
        delete el[tooltipDataSymbol];

        data.floatingElement.removeEventListener("mouseenter", onMouseEnter);
        data.floatingElement.removeEventListener("mouseleave", onMouseLeave);
        delete data.floatingElement[tooltipDataSymbol];
    },
};
