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
// The one action bar of a scrolling column of messages, and when it shows. A column owns a single
// bar, so two can never be on screen: each row that offers a bar only lends it a dock, and the bar
// is drawn in the dock of the message it is on. It first shows once the pointer rests on a message,
// then follows the pointer from message to message at once, with no fade, and hides a moment after
// the pointer leaves the messages. A menu or the picker open from it, or a keyboard's focus inside
// it, holds it where it is; a keyboard reaching a message shows it there at once.
//
// The conversation's feed and the thread drawer each keep one, binding the handlers below on
// their scroller, whose rows each mark themselves with data-hot-id.

import { computed, ComputedRef, inject, InjectionKey, provide, Ref, ref, shallowReactive } from "vue";
import { tokenDuration } from "../ui/brand.partial";

/** What a row lends the column's bar: where it is drawn, what it shows and where its choices go. */
export type ActionBarDock = {
    /** The element the bar is drawn in, placed against the message's bubble or a channel's row. */
    element: HTMLElement;

    /** The bar's props for the row's message. */
    props: ComputedRef<Record<string, unknown>>;

    /** The bar's handlers for the row's message. */
    handlers: Record<string, (...args: never[]) => void>;
};

/** One column's action bar, and the handlers its scroller binds. */
export type ColumnActionBar = {
    /** The dock the bar is in: the message it shows on, or the one it last showed on while it fades. */
    dock: ComputedRef<ActionBarDock | null>;

    /** Whether the bar shows. */
    isShown: Ref<boolean>;

    /** Whether the last show or hide skips the fade, as a scroll's or a keyboard leaving does. */
    isInstant: Ref<boolean>;

    /** A row lends its dock while its message offers a bar, and takes it back when it no longer does. */
    offer: (messageId: number, dock: ActionBarDock) => void;
    withdraw: (messageId: number, dock: ActionBarDock) => void;

    onPointerOver: (event: PointerEvent) => void;
    onPointerLeave: () => void;
    onPointerMove: (event: PointerEvent) => void;
    onScroll: () => void;
    onFocusIn: (event: FocusEvent) => void;
    onFocusOut: (event: FocusEvent) => void;

    /** The bar reports a menu, the picker or a keyboard's focus holding it, and letting go. */
    onHoldChange: (isHeld: boolean) => void;

    /** Stops every timer, as the column goes. */
    stop: () => void;
};

const actionBarKey: InjectionKey<ColumnActionBar> = Symbol("chat-action-bar");

/** The design's rest and grace, used when the sheet is not on the page. */
const fallbackRestMs = 150;
const fallbackGraceMs = 220;

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
 * Creates one column's action bar and hands it to the column's rows.
 *
 * @param timingElement The element the rest and grace tokens are read on, once the column is drawn.
 *
 * @returns The bar and its handlers.
 */
export function useActionBar(timingElement: () => Element | null | undefined): ColumnActionBar {
    const docks = shallowReactive(new Map<number, ActionBarDock>());

    /** The message the bar is on; kept after it hides, so it fades where it was. */
    const barId = ref<number | null>(null);
    const isShown = ref(false);
    const isInstant = ref(false);

    const dock = computed((): ActionBarDock | null => barId.value === null ? null : docks.get(barId.value) ?? null);

    /** The message the pointer is over, or the one a keyboard reached. */
    let hotId: number | null = null;

    /** The hot message came from a keyboard's focus, which the pointer leaving does not let go of. */
    let isHotFromFocus = false;

    /** A menu, the picker or a keyboard's focus inside keeps the bar on its message. */
    let isHeld = false;

    /** Set by a scroll and cleared by the next move, so the rows passing under a still pointer stay cold. */
    let isScrollMuted = false;

    let restTimer: ReturnType<typeof setTimeout> | null = null;
    let graceTimer: ReturnType<typeof setTimeout> | null = null;

    function hoverMs(token: string, fallbackMs: number): number {
        const element = timingElement();
        return element ? tokenDuration(element, token, fallbackMs) : fallbackMs;
    }

    function stop(): void {
        if (restTimer) {
            clearTimeout(restTimer);
            restTimer = null;
        }

        if (graceTimer) {
            clearTimeout(graceTimer);
            graceTimer = null;
        }
    }

    function show(id: number): void {
        stop();
        barId.value = id;
        isInstant.value = false;
        isShown.value = true;
    }

    function hide(isWithoutFade: boolean): void {
        stop();
        isInstant.value = isWithoutFade;
        isShown.value = false;
    }

    /**
     * The pointer, or a held bar letting go, leaves the bar where the pointer now is. A shown bar
     * moves there at once; a hidden one waits for the pointer to rest; away from every message, a
     * shown bar stays for its grace, so slipping off a message for a moment does not lose it.
     */
    function follow(id: number | null): void {
        if (restTimer) {
            clearTimeout(restTimer);
            restTimer = null;
        }

        if (id === null) {
            if (isShown.value && !graceTimer) {
                graceTimer = setTimeout(() => hide(false), hoverMs("--chat-d-hover-grace", fallbackGraceMs));
            }

            return;
        }

        if (graceTimer) {
            clearTimeout(graceTimer);
            graceTimer = null;
        }

        if (isShown.value) {
            barId.value = id;
            return;
        }

        restTimer = setTimeout(() => show(id), hoverMs("--chat-d-hover-rest", fallbackRestMs));
    }

    /** The pointer reached a row, or a gap between rows. */
    function onPointerOver(event: PointerEvent): void {
        if (event.pointerType === "touch" || isScrollMuted) {
            return;
        }

        const id = hotIdAt(event.target);
        if (id === hotId) {
            return;
        }

        hotId = id;
        isHotFromFocus = false;
        if (!isHeld) {
            follow(id);
        }
    }

    function onPointerLeave(): void {
        if (isHotFromFocus || hotId === null) {
            return;
        }

        hotId = null;
        if (!isHeld) {
            follow(null);
        }
    }

    /** The first move after a scroll wakes the row under the pointer, which gets no new pointerover. */
    function onPointerMove(event: PointerEvent): void {
        if (!isScrollMuted || event.pointerType === "touch") {
            return;
        }

        isScrollMuted = false;
        hotId = hotIdAt(event.target);
        if (!isHeld) {
            follow(hotId);
        }
    }

    /** A scroll hides the bar at once, since its message is sliding away under it. */
    function onScroll(): void {
        isScrollMuted = true;
        hotId = null;
        isHotFromFocus = false;
        if (!isHeld) {
            hide(true);
        }
    }

    /** A keyboard reaching a message's links or buttons shows its bar at once, so Tab can go on into it. */
    function onFocusIn(event: FocusEvent): void {
        const id = hotIdAt(event.target);
        if (id === null || !isKeyboardFocus(event.target) || (isHeld && id !== barId.value)) {
            return;
        }

        hotId = id;
        isHotFromFocus = true;
        if (isShown.value) {
            follow(id);
        }
        else {
            show(id);
        }
    }

    function onFocusOut(event: FocusEvent): void {
        if (!isHotFromFocus || hotIdAt(event.relatedTarget) === hotId) {
            return;
        }

        hotId = null;
        isHotFromFocus = false;
        if (!isHeld) {
            hide(true);
        }
    }

    /** Let go, the bar goes where the pointer is now, or starts its grace if it is on none. */
    function onHoldChange(isHolding: boolean): void {
        isHeld = isHolding;
        if (!isHolding && hotId !== barId.value) {
            follow(hotId);
        }
    }

    function offer(messageId: number, offered: ActionBarDock): void {
        docks.set(messageId, offered);
    }

    /** A message that no longer offers a bar takes it away at once, held or not. */
    function withdraw(messageId: number, offered: ActionBarDock): void {
        if (docks.get(messageId) !== offered) {
            return;
        }

        docks.delete(messageId);
        if (hotId === messageId) {
            hotId = null;
        }

        if (barId.value === messageId) {
            isHeld = false;
            hide(true);
        }
    }

    const bar: ColumnActionBar = { dock, isShown, isInstant, offer, withdraw, onPointerOver, onPointerLeave, onPointerMove, onScroll, onFocusIn, onFocusOut, onHoldChange, stop };
    provide(actionBarKey, bar);
    return bar;
}

/**
 * The action bar of the column a row is drawn in.
 *
 * @returns The column's bar, or null for a row drawn outside any column.
 */
export function useColumnActionBar(): ColumnActionBar | null {
    return inject(actionBarKey, null);
}
