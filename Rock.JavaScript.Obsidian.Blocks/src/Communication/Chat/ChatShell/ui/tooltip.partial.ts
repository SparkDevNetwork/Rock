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

import { onBeforeUnmount, Ref, ref } from "vue";
import { autoUpdate, flip, offset, shift, useFloating } from "@Obsidian/Libs/floating-ui";
import { tokenDuration } from "./brand.partial";

/** A tooltip's state and the handlers its anchor binds, and an interactive tip's own. */
export type ChatTooltip = {
    /** The drawn tip, for the placement to measure. */
    tipElement: Ref<HTMLElement | undefined>;
    isTipShown: Ref<boolean>;
    floatingStyles: ReturnType<typeof useFloating>["floatingStyles"];
    hideTip: () => void;
    onPointerEnter: () => void;
    onPointerLeave: () => void;
    onPointerDown: () => void;
    onFocus: () => void;
    onBlur: (event?: FocusEvent) => void;

    /** The anchor's keys: Tab into an interactive tip that shows, Escape to close it. */
    onAnchorKeydown: (event: KeyboardEvent) => void;

    /** The tip's own handlers, bound only on an interactive tip. */
    onTipPointerEnter: () => void;
    onTipPointerLeave: () => void;
    onTipFocusOut: (event: FocusEvent) => void;
    onTipKeydown: (event: KeyboardEvent) => void;
};

/**
 * The chat's root an element sits in, where a tip or a menu is drawn so the feed's faded edges
 * neither fade nor cut it: a mask paints over everything inside its element, a fixed child too.
 * Null outside a chat root, as when a part is drawn alone, which then draws in place.
 *
 * @param element The anchor.
 *
 * @returns The root, or null.
 */
export function overlayHostOf(element: Element | null | undefined): HTMLElement | null {
    return element?.closest<HTMLElement>(".chat-shell") ?? null;
}

/** The design's tip delay, used when the sheet is not on the page. */
const fallbackTipDelayMs = 500;

/** Where a tip's rest time is read: a timing token in the sheet, and the value when the sheet is absent. */
export type TooltipDelay = {
    token: string;
    fallbackMs: number;
};

const defaultTipDelay: TooltipDelay = { token: "--chat-d-tip-delay", fallbackMs: fallbackTipDelayMs };

/** How long an interactive tip outlives the pointer leaving, so the pointer can cross the gap into it; shared with the action bar's. */
const hoverGrace: TooltipDelay = { token: "--chat-d-hover-grace", fallbackMs: 220 };

/** What the keyboard can reach, in the page's order. */
const tabbableSelector = "button:not([disabled]), [href], input:not([disabled]), select:not([disabled]), textarea:not([disabled]), [contenteditable='true'], [tabindex='0']";

/**
 * Whether an element is drawn for the keyboard, though it still matches the keyboard's selector:
 * not one under display:none, or hidden itself, nor one in an inert part, as a phone's panes
 * behind the front one are, or in a part hidden from assistive technology.
 *
 * @param element The element.
 *
 * @returns Whether it is drawn.
 */
function isDrawn(element: HTMLElement): boolean {
    if (getComputedStyle(element).visibility === "hidden" || element.closest("[inert], [aria-hidden='true']")) {
        return false;
    }

    for (let at: HTMLElement | null = element; at; at = at.parentElement) {
        if (at.hidden || getComputedStyle(at).display === "none") {
            return false;
        }
    }

    return true;
}

/** Set only while focusQuietly moves focus, which a browser reports at once. */
let isQuietFocus = false;

/**
 * Focuses an element without showing its tooltip: a panel opening puts focus on its back chevron
 * or X only so the keyboard starts inside it, which is not the person reaching for that button.
 *
 * @param element The element to focus.
 * @param options How to focus it, such as without scrolling a panel that is still opening.
 */
export function focusQuietly(element: HTMLElement, options?: FocusOptions): void {
    isQuietFocus = true;
    try {
        element.focus(options);
    }
    finally {
        isQuietFocus = false;
    }
}

/**
 * The chat's tooltip for one anchor. The caller draws the tip, class chat-tip, role tooltip, at
 * the floating styles while it is shown, and binds the anchor's handlers. A tip shows once the
 * pointer has rested or a keyboard focus has arrived, never under a press, and hides on a click.
 *
 * An interactive tip holds something to press, so it is a popover rather than a tooltip: drawn
 * below the anchor, it outlives the pointer leaving by a grace the pointer entering it cancels,
 * and the keyboard moves into it with Tab and back with Shift+Tab or Escape. The caller binds the
 * tip's handlers too.
 *
 * @param anchor The element the tip belongs to.
 * @param isDisabled Whether the tip is kept away for now, such as while a menu covers it.
 * @param delay The rest time's token, for a tip that shows sooner than the rest, as on a reaction.
 * @param isInteractive Whether the tip holds something to press.
 */
export function useTooltip(anchor: Ref<HTMLElement | undefined>, isDisabled: () => boolean = () => false, delay: TooltipDelay = defaultTipDelay, isInteractive: boolean = false): ChatTooltip {
    const tipElement = ref<HTMLElement>();
    const isTipShown = ref(false);

    // One timer: the rest before showing, or an interactive tip's grace before hiding.
    let tipTimer: number | undefined;

    /** Whether the focus that is coming was a press, since a tip under the pointer is noise. */
    let isPointerFocus = false;

    // Above the anchor, or below it when there is no room, kept 8px inside the window. An
    // interactive tip hangs below, from the anchor's start, as a card opened from it does.
    const { floatingStyles } = useFloating(anchor, tipElement, {
        placement: isInteractive ? "bottom-start" : "top",
        strategy: "fixed",
        middleware: [offset(isInteractive ? 6 : 8), flip(), shift({ padding: 8 })],
        whileElementsMounted: autoUpdate
    });

    /** Shows the tip once the pointer has rested, so passing over a row of buttons does not flicker tips. */
    function scheduleTip(): void {
        if (isDisabled()) {
            return;
        }

        window.clearTimeout(tipTimer);
        const restMs = anchor.value ? tokenDuration(anchor.value, delay.token, delay.fallbackMs) : delay.fallbackMs;
        tipTimer = window.setTimeout(() => {
            isTipShown.value = true;
        }, restMs);
    }

    function hideTip(): void {
        window.clearTimeout(tipTimer);
        isTipShown.value = false;
    }

    /**
     * The pointer left the anchor or the tip. A tip that is not showing yet never will; an
     * interactive one that is showing waits out the grace, so the pointer can reach it.
     */
    function leave(): void {
        if (!isInteractive || !isTipShown.value) {
            hideTip();
            return;
        }

        window.clearTimeout(tipTimer);
        const graceMs = anchor.value ? tokenDuration(anchor.value, hoverGrace.token, hoverGrace.fallbackMs) : hoverGrace.fallbackMs;
        tipTimer = window.setTimeout(hideTip, graceMs);
    }

    /** Whether an element is inside the tip. */
    function isInTip(element: EventTarget | null | undefined): boolean {
        return element instanceof Node && !!tipElement.value?.contains(element);
    }

    function tipControls(): HTMLElement[] {
        return Array.from(tipElement.value?.querySelectorAll<HTMLElement>(tabbableSelector) ?? []);
    }

    /**
     * Focus on what follows the anchor, as Tab from the anchor would have reached it: the tip is
     * drawn in the chat's root, out of the page's order, so leaving it forward goes back there.
     */
    function focusAfterAnchor(): void {
        const from = anchor.value;
        if (!from) {
            return;
        }

        const next = Array.from(document.querySelectorAll<HTMLElement>(tabbableSelector))
            .find(element => !isInTip(element) && !!(from.compareDocumentPosition(element) & Node.DOCUMENT_POSITION_FOLLOWING) && !from.contains(element) && isDrawn(element));
        next?.focus();
    }

    /**
     * Only a keyboard focus shows the tip. Told apart by the press before it rather than by
     * :focus-visible, which the oldest Safari Rock supports cannot match from script.
     */
    function onFocus(): void {
        if (!isPointerFocus && !isQuietFocus) {
            scheduleTip();
        }
    }

    function onPointerDown(): void {
        isPointerFocus = true;
    }

    /** Focus leaving the anchor hides the tip, unless it went into an interactive tip. */
    function onBlur(event?: FocusEvent): void {
        isPointerFocus = false;
        if (!isInteractive || !isInTip(event?.relatedTarget)) {
            hideTip();
        }
    }

    function onAnchorKeydown(event: KeyboardEvent): void {
        if (!isInteractive || !isTipShown.value) {
            return;
        }

        if (event.key === "Escape") {
            event.stopPropagation();
            hideTip();
        }
        else if (event.key === "Tab" && !event.shiftKey && tipControls()[0]) {
            event.preventDefault();
            tipControls()[0].focus();
        }
    }

    /** Focus leaving the tip for anything but its anchor hides it. */
    function onTipFocusOut(event: FocusEvent): void {
        const to = event.relatedTarget;
        if (!isInTip(to) && !(to instanceof Node && anchor.value?.contains(to))) {
            hideTip();
        }
    }

    function onTipKeydown(event: KeyboardEvent): void {
        const controls = tipControls();
        const isFirst = document.activeElement === controls[0];
        const isLast = document.activeElement === controls[controls.length - 1];

        if (event.key === "Escape") {
            event.stopPropagation();
            hideTip();
            if (anchor.value) {
                focusQuietly(anchor.value);
            }
        }
        else if (event.key === "Tab" && event.shiftKey && isFirst && anchor.value) {
            event.preventDefault();
            focusQuietly(anchor.value);
        }
        else if (event.key === "Tab" && !event.shiftKey && isLast) {
            event.preventDefault();
            hideTip();
            focusAfterAnchor();
        }
    }

    onBeforeUnmount(() => {
        window.clearTimeout(tipTimer);
    });

    return {
        tipElement,
        isTipShown,
        floatingStyles,
        hideTip,
        onPointerEnter: scheduleTip,
        onPointerLeave: leave,
        onPointerDown,
        onFocus,
        onBlur,
        onAnchorKeydown,
        onTipPointerEnter: (): void => {
            window.clearTimeout(tipTimer);
        },
        onTipPointerLeave: leave,
        onTipFocusOut,
        onTipKeydown
    };
}
