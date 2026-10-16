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
// Which message of a scrolling column of messages is hot: the one the pointer is over, whose
// action bar may show. One at a time, so a column never shows two bars. The bar keeps its own
// rest and grace; this says only where the pointer is and whether a move between messages is a
// swap, which shows the new bar and hides the old one at once.
//
// The conversation's feed and the thread drawer each keep one, binding the handlers below on
// their scroller, whose rows each mark themselves with data-hot-id.

import { ref, Ref } from "vue";

/** The hot message of one column, and the handlers its scroller binds. */
export type HotMessage = {
    /** The message the pointer is over, or null. */
    hotId: Ref<number | null>;

    /** Whether the last change of hotId was a swap between bars, shown and hidden without waiting. */
    isSwapInstant: Ref<boolean>;

    onPointerOver: (event: PointerEvent) => void;
    onPointerLeave: () => void;
    onPointerMove: (event: PointerEvent) => void;
    onScroll: () => void;
    onFocusIn: (event: FocusEvent) => void;
    onFocusOut: (event: FocusEvent) => void;

    /** A row's bar reports showing and holding, so the column knows which bar is up. */
    onBarChange: (messageId: number, isShown: boolean, isHeld: boolean) => void;
};

/**
 * The message a row of the column stands for, from the element an event reached.
 *
 * @param target The event's target.
 *
 * @returns The message's id, or null outside any row that offers a bar.
 */
function hotIdAt(target: EventTarget | null): number | null {
    const row = target instanceof Element ? target.closest<HTMLElement>("[data-hot-id]") : null;
    const id = Number(row?.dataset.hotId);
    return row && Number.isSafeInteger(id) ? id : null;
}

/**
 * Whether a focus came from the keyboard, which shows a bar the way a resting pointer does. The
 * oldest Safari Rock supports cannot tell and throws, and is taken as the keyboard.
 *
 * @param target The focused element.
 *
 * @returns True for a keyboard's focus.
 */
function isKeyboardFocus(target: EventTarget | null): boolean {
    try {
        return target instanceof Element && target.matches(":focus-visible");
    }
    catch {
        return true;
    }
}

/**
 * Creates one column's hot message.
 *
 * @returns The hot message and its handlers.
 */
export function useHotMessage(): HotMessage {
    const hotId = ref<number | null>(null);
    const isSwapInstant = ref(false);

    /** The message whose bar shows now, which may be the one just left, still in its grace. */
    let shownId: number | null = null;

    /** The message whose bar a menu or a keyboard's focus holds; the column stays on it. */
    let heldId: number | null = null;

    /** Set by a scroll and cleared by the next move, so the rows passing under a still pointer stay cold. */
    let isScrollMuted = false;

    /** Whether the hot message came from a keyboard's focus, which leaving the row lets go of. */
    let isHotFromFocus = false;

    function setHot(id: number | null, isInstant: boolean): void {
        isSwapInstant.value = isInstant;
        hotId.value = id;
    }

    /** The pointer reached a row, or a gap between rows. */
    function onPointerOver(event: PointerEvent): void {
        if (event.pointerType === "touch" || isScrollMuted) {
            return;
        }

        const id = hotIdAt(event.target);
        if (id === hotId.value) {
            return;
        }

        isHotFromFocus = false;

        // A held bar keeps the column on its message until the menu closes or focus leaves.
        if (heldId !== null && heldId !== id) {
            return;
        }

        // Straight from one bar to another is a swap; from nothing, the rest applies.
        setHot(id, id !== null && shownId !== null && shownId !== id);
    }

    function onPointerLeave(): void {
        if (hotId.value !== null && !isHotFromFocus) {
            setHot(null, false);
        }
    }

    /** The first move after a scroll wakes the row under the pointer, which gets no new pointerover. */
    function onPointerMove(event: PointerEvent): void {
        if (!isScrollMuted || event.pointerType === "touch") {
            return;
        }

        isScrollMuted = false;
        const id = hotIdAt(event.target);
        if (id !== null && heldId === null) {
            setHot(id, false);
        }
    }

    /** A scroll hides the bar at once, since its message is sliding away under it. */
    function onScroll(): void {
        isScrollMuted = true;
        if (hotId.value !== null && heldId === null) {
            isHotFromFocus = false;
            setHot(null, true);
        }
    }

    /** A keyboard reaching a message's links or buttons shows its bar, so Tab can go on into it. */
    function onFocusIn(event: FocusEvent): void {
        const id = hotIdAt(event.target);
        if (id === null || id === hotId.value || !isKeyboardFocus(event.target)) {
            return;
        }

        if (heldId !== null && heldId !== id) {
            return;
        }

        isHotFromFocus = true;
        setHot(id, true);
    }

    function onFocusOut(event: FocusEvent): void {
        if (!isHotFromFocus || hotIdAt(event.relatedTarget) === hotId.value) {
            return;
        }

        isHotFromFocus = false;
        setHot(null, true);
    }

    function onBarChange(messageId: number, isShown: boolean, isHeld: boolean): void {
        if (isShown) {
            shownId = messageId;
        }
        else if (shownId === messageId) {
            shownId = null;
        }

        if (isHeld) {
            heldId = messageId;
        }
        else if (heldId === messageId) {
            heldId = null;
        }
    }

    return { hotId, isSwapInstant, onPointerOver, onPointerLeave, onPointerMove, onScroll, onFocusIn, onFocusOut, onBarChange };
}
