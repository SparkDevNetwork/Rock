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

// A long press, the one way a touch screen asks for more on a message or a reaction chip: a
// finger held still. Its two timers are the gesture itself, the press shown and then fired, not a
// delay hiding anything.

import { onScopeDispose, ref, Ref, watch } from "vue";
import { tokenDuration } from "../ui/brand.partial";

/** What a long press's owner tells it. */
export type LongPressOptions = {
    /** Whether presses are held now; while false nothing is bound at all. */
    isOn: () => boolean;

    /** The hold fired, with the element the finger was on when it went down. */
    onPress: (target: Element) => void;

    /** Whether a pointer going down may start a hold; any primary pointer's main button when not said. */
    isPointerTaken?: (event: PointerEvent) => boolean;

    /**
     * The element a press is held on, inside the bound element, for an owner that holds many, as a
     * row of chips does; the bound element itself when not said. That element carries the mark.
     */
    heldOf?: (target: Element) => Element | null;
};

/** How far, in pixels, a finger may move before a press stops being a long press. */
const slopPx = 8;

/** The mark of an element that holds its own presses, so a press there is never also its container's. */
const markAttribute = "data-long-press";

/**
 * A long press on an element. The press shows from the fast duration and fires at the long-press
 * duration, both read from the sheet. A move past 8px, the browser taking the touch to scroll, an
 * early release, a second finger, a scroll or the page going away ends it unfired. The phone's own
 * menu is held off from the press's start to its end. The click a fired hold's release makes is
 * swallowed wherever it lands, so a photo under the finger opens no viewer and a link is not
 * followed; the next press anywhere disarms that, so a hold the browser ended with no click never
 * eats the next tap, in the sheet the hold opened above all. A press nearer another mark is that
 * mark's own, so a chip held never also opens its message's sheet. While idle the element holds
 * one listener, the press's start.
 *
 * @param element The element pressed.
 * @param options What the owner tells it.
 *
 * @returns Whether the press is showing now.
 */
export function useLongPress(element: Ref<HTMLElement | undefined | null>, options: LongPressOptions): { isPressing: Ref<boolean> } {
    const isPressing = ref(false);

    /** The press under way, with every listener and timer it holds, ended by calling its end. */
    let press: { pointerId: number, x: number, y: number, end: () => void } | null = null;

    /** Takes the swallow down, from the click it ate or the press that came first. */
    let disarm: (() => void) | null = null;

    function isTaken(event: PointerEvent): boolean {
        return options.isPointerTaken
            ? options.isPointerTaken(event)
            : event.isPrimary !== false && (event.pointerType !== "mouse" || event.button === 0);
    }

    /**
     * Takes the release of a fired hold from being a tap, unless a press comes first. A finger's
     * release is cancelled, so the browser makes no mouse press of it: that press would land where
     * the finger was, by now the scrim of the sheet the hold opened, and take focus out of the
     * sheet. The next click is swallowed once, for a pen or mouse hold and any browser that clicks.
     */
    function armSwallow(): void {
        disarm?.();
        const onClick = (event: Event): void => {
            event.preventDefault();
            event.stopImmediatePropagation();
            disarm?.();
        };
        const onDown = (): void => disarm?.();
        const onTouchEnd = (event: Event): void => {
            event.preventDefault();
            window.removeEventListener("touchend", onTouchEnd, true);
        };
        document.addEventListener("click", onClick, true);
        document.addEventListener("pointerdown", onDown, true);
        // Not passive, or the browser ignores the cancel.
        window.addEventListener("touchend", onTouchEnd, { capture: true, passive: false });
        disarm = (): void => {
            document.removeEventListener("click", onClick, true);
            document.removeEventListener("pointerdown", onDown, true);
            window.removeEventListener("touchend", onTouchEnd, true);
            disarm = null;
        };
    }

    function start(bound: HTMLElement, event: PointerEvent): void {
        const target = event.target instanceof Element ? event.target : null;
        const held = target ? (options.heldOf ? options.heldOf(target) : bound) : null;
        if (!target || !held || target.closest(`[${markAttribute}]`) !== held || !isTaken(event)) {
            return;
        }

        let isFired = false;
        const pressingTimer = window.setTimeout(() => {
            isPressing.value = true;
        }, tokenDuration(bound, "--chat-t-fast", 120));
        const fireTimer = window.setTimeout(() => {
            isFired = true;
            isPressing.value = false;
            armSwallow();
            options.onPress(target);
        }, tokenDuration(bound, "--chat-d-long-press", 450));

        // Before the fire each of these ends the press unfired; after it, only the finger lifting
        // or the browser taking it ends the press, which holds the menu off until then.
        const onMove = (move: PointerEvent): void => {
            if (!isFired && press && move.pointerId === press.pointerId && Math.hypot(move.clientX - press.x, move.clientY - press.y) > slopPx) {
                end();
            }
        };
        const onUnfiredEnd = (): void => {
            if (!isFired) {
                end();
            }
        };
        const onSecond = (down: PointerEvent): void => {
            if (press && down.pointerId !== press.pointerId) {
                end();
            }
        };
        const onMenu = (menu: Event): void => menu.preventDefault();

        const listeners: [EventTarget, string, EventListener, AddEventListenerOptions][] = [
            [window, "pointermove", onMove as EventListener, { passive: true }],
            [window, "pointerup", end, {}],
            [window, "pointercancel", end, {}],
            [window, "pointerdown", onSecond as EventListener, { capture: true }],
            [document, "scroll", onUnfiredEnd, { capture: true, passive: true }],
            [document, "visibilitychange", end, {}],
            [window, "contextmenu", onMenu, { capture: true }]
        ];

        function end(): void {
            window.clearTimeout(pressingTimer);
            window.clearTimeout(fireTimer);
            listeners.forEach(([on, type, listener, listenerOptions]) => on.removeEventListener(type, listener, listenerOptions));
            isPressing.value = false;
            press = null;
        }

        press = { pointerId: event.pointerId, x: event.clientX, y: event.clientY, end };
        listeners.forEach(([on, type, listener, listenerOptions]) => on.addEventListener(type, listener, listenerOptions));
    }

    function onPointerDown(event: Event): void {
        const bound = element.value;
        if (!bound || press) {
            return;
        }

        start(bound, event as PointerEvent);
    }

    watch([() => element.value, () => options.isOn()], ([bound, isOn], _before, onCleanup) => {
        if (!bound || !isOn) {
            return;
        }

        // An owner of many marks each one it holds; one holding the element alone marks the element.
        if (!options.heldOf) {
            bound.setAttribute(markAttribute, "");
        }
        bound.addEventListener("pointerdown", onPointerDown);
        onCleanup(() => {
            bound.removeEventListener("pointerdown", onPointerDown);
            if (!options.heldOf) {
                bound.removeAttribute(markAttribute);
            }
            press?.end();
        });
    }, { immediate: true, flush: "sync" });

    // A press held as its element goes, as when the feed switches room, fires nothing. A swallow
    // already armed stays, since the release it waits for still comes, and goes on its own.
    onScopeDispose(() => press?.end());

    return { isPressing };
}
