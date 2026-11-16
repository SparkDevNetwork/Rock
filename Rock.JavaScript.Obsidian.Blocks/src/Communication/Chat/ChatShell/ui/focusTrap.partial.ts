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

// Where the keyboard can go, in one place: what can take focus, how a modal surface keeps it, and
// how focus is moved without showing a tip.

import { nextTick, onScopeDispose, Ref, watch } from "vue";

/** A surface's element while it is drawn, or while it is open where its way out outlasts its close. */
export type SurfaceElement = Ref<HTMLElement | undefined> | (() => HTMLElement | null | undefined);

/** What can take focus, in the page's order: one list, so every surface agrees on it. */
export const focusableSelector = "button:not([disabled]), [href], input:not([disabled]), select:not([disabled]), textarea:not([disabled]), [contenteditable='true'], [tabindex='0']";

/**
 * Whether an element is drawn for the keyboard, though it still matches the selector: not one
 * under display:none, or hidden itself, nor one in an inert part, as a phone's panes behind the
 * front one are, or in a part hidden from assistive technology.
 *
 * @param element The element.
 *
 * @returns Whether it is drawn.
 */
export function isDrawn(element: HTMLElement): boolean {
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
 * Whether the focus arriving now was moved by focusQuietly, which shows no tip.
 *
 * @returns True during a quiet focus.
 */
export function isFocusQuiet(): boolean {
    return isQuietFocus;
}

/**
 * A surface Escape closes. A popover takes the key before anything under it sees it, since focus
 * may stay outside it, in the box that opened it. A modal surface holds focus, so the key goes on
 * to it: its own parts answer first, as a search clears its words, and the surface last.
 */
export type EscapeLayer = {
    /** Closes a popover; none for a modal surface, which answers the key itself. */
    takeEscape?: () => void;
};

/**
 * Every open surface Escape closes, popovers and modal surfaces in one stack, oldest first, so the
 * newest takes the key whichever kind it is and the one under it keeps it for the next press.
 */
const escapeStack: EscapeLayer[] = [];

/** Captured on its way down, so a part under the newest popover never sees the key. */
function onDocumentEscape(event: KeyboardEvent): void {
    const top = escapeStack[escapeStack.length - 1];
    if (event.key !== "Escape" || event.isComposing || !top?.takeEscape) {
        return;
    }

    event.preventDefault();
    event.stopPropagation();
    top.takeEscape();
}

/**
 * Puts an open surface on top of the Escape stack.
 *
 * @param layer The surface.
 */
export function pushEscapeLayer(layer: EscapeLayer): void {
    if (escapeStack.length === 0) {
        document.addEventListener("keydown", onDocumentEscape, true);
    }

    escapeStack.push(layer);
}

/**
 * Takes a surface off the Escape stack as it closes; one not on it is left alone.
 *
 * @param layer The surface.
 */
export function pullEscapeLayer(layer: EscapeLayer): void {
    const at = escapeStack.indexOf(layer);
    if (at < 0) {
        return;
    }

    escapeStack.splice(at, 1);
    if (escapeStack.length === 0) {
        document.removeEventListener("keydown", onDocumentEscape, true);
    }
}

/** How a trap starts and ends. */
export type FocusTrapOptions = {
    /** What focus goes back to as the trap ends, when not what had focus as it began. */
    returnTo?: () => HTMLElement | null;

    /**
     * What focus starts on as the trap begins; by default the control marked data-autofocus, else
     * the first. Null leaves focus where it is, for a surface that places it itself.
     */
    initialFocus?: (controls: HTMLElement[]) => HTMLElement | null;
};

/**
 * Keeps the keyboard inside a modal surface for as long as its element is given: focus starts
 * inside, quietly, Tab wraps from the last control to the first and Shift+Tab back, and as the
 * element goes focus returns to what opened it, quietly, when that is still on the page. Focus the
 * page moves is not the person reaching for a control, so neither shows that control's tip.
 *
 * Rows that are all out of the tab order, as a sheet's menu rows are, are moved through one by one,
 * since Tab would otherwise leave from any of them.
 *
 * @param element The surface's element; a getter that answers nothing once the surface closes
 *     ends the trap then, before a way out has finished playing.
 * @param options How the trap starts and ends.
 */
export function useFocusTrap(element: SurfaceElement, options: FocusTrapOptions = {}): void {
    let opener: HTMLElement | null = null;
    let trapped: HTMLElement | null = null;

    // A modal surface is on the Escape stack while it holds the keyboard.
    const layer: EscapeLayer = {};

    function controls(): HTMLElement[] {
        return Array.from(trapped?.querySelectorAll<HTMLElement>(focusableSelector) ?? []).filter(isDrawn);
    }

    function onKeydown(event: KeyboardEvent): void {
        if (event.key !== "Tab" || event.defaultPrevented) {
            return;
        }

        const all = controls();
        const inOrder = all.filter(control => control.tabIndex >= 0);
        const ring = inOrder.length > 0 ? inOrder : all;
        if (ring.length === 0) {
            event.preventDefault();
            return;
        }

        const at = ring.indexOf(document.activeElement as HTMLElement);
        const isRoving = inOrder.length === 0;
        const isAtEdge = event.shiftKey ? at <= 0 : at === ring.length - 1 || at < 0;

        // between two controls in the tab order the browser's own Tab is right
        if (!isRoving && !isAtEdge) {
            return;
        }

        event.preventDefault();
        const next = at < 0
            ? (event.shiftKey ? ring.length - 1 : 0)
            : (at + (event.shiftKey ? -1 : 1) + ring.length) % ring.length;
        ring[next].focus();
    }

    function begin(surface: HTMLElement): void {
        opener = document.activeElement instanceof HTMLElement && document.activeElement !== document.body ? document.activeElement : null;
        trapped = surface;
        surface.addEventListener("keydown", onKeydown);
        pushEscapeLayer(layer);
        void nextTick(() => {
            if (trapped !== surface || surface.contains(document.activeElement)) {
                return;
            }

            const all = controls();
            const start = options.initialFocus
                ? options.initialFocus(all)
                : surface.querySelector<HTMLElement>("[data-autofocus]") ?? all[0] ?? surface;
            if (start) {
                focusQuietly(start, { preventScroll: true });
            }
        });
    }

    function end(): void {
        trapped?.removeEventListener("keydown", onKeydown);
        trapped = null;
        pullEscapeLayer(layer);
        const target = options.returnTo?.() ?? opener;
        opener = null;
        if (target?.isConnected) {
            focusQuietly(target, { preventScroll: true });
        }
    }

    // At once as it goes, not after its way out, so a keyboard user is never left on a vanishing control.
    watch(() => (typeof element === "function" ? element() : element.value) ?? undefined, (now, was) => {
        if (was && was !== now) {
            end();
        }

        if (now && now !== was) {
            begin(now);
        }
    }, { immediate: true, flush: "post" });

    onScopeDispose(() => {
        if (trapped) {
            end();
        }
    });
}
