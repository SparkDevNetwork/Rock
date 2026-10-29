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
// When a hover-only control of a list of rows shows, shared by the message action bar and the
// members list's Message button. It first shows once the pointer has rested on a row, with a
// fade; from then on it follows the pointer from row to row at once, with none; and it hides,
// with a fade, a moment after the pointer is on no row, so slipping off for an instant does not
// lose it. The owner decides which row the pointer is on and what holds the control in place; this
// decides only the timing and whether the change fades.

import { Ref, ref } from "vue";
import { tokenDuration } from "../ui/brand.partial";

/** The design's rest and grace, used when the sheet is not on the page. */
const fallbackRestMs = 150;
const fallbackGraceMs = 220;

/** One list's hover control: the row it is on, whether it shows, and whether the last change skips the fade. */
export type HoverIntent<TKey> = {
    /** The row the control is on; kept after it hides, so it fades where it was. */
    shownKey: Ref<TKey | null>;

    /** Whether the control shows. */
    isShown: Ref<boolean>;

    /**
     * Whether the last show, move or hide skips the fade. A move does; the pointer withdrawing
     * always fades, however the control got to where it is, and only an owner's own hide can ask
     * for none.
     */
    isInstant: Ref<boolean>;

    /**
     * The pointer, or a held control letting go, is now on a row or on none. A shown control moves
     * there at once; a hidden one waits for the pointer to rest; on none, a shown one starts its grace.
     */
    follow: (key: TKey | null) => void;

    /** Shows the control on a row now, with the fade. */
    show: (key: TKey) => void;

    /** Hides the control now, with the fade unless asked to skip it. */
    hide: (isWithoutFade: boolean) => void;

    /** Stops the rest and the grace, as the list goes. */
    stop: () => void;
};

/**
 * Creates one list's hover intent.
 *
 * @param timingElement The element the rest and grace tokens are read on, once the list is drawn.
 *
 * @returns The intent.
 */
export function useHoverIntent<TKey>(timingElement: () => Element | null | undefined): HoverIntent<TKey> {
    const shownKey = ref<TKey | null>(null) as Ref<TKey | null>;
    const isShown = ref(false);
    const isInstant = ref(false);

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

    function show(key: TKey): void {
        stop();
        shownKey.value = key;
        isInstant.value = false;
        isShown.value = true;
    }

    function hide(isWithoutFade: boolean): void {
        stop();
        isInstant.value = isWithoutFade;
        isShown.value = false;
    }

    function follow(key: TKey | null): void {
        if (restTimer) {
            clearTimeout(restTimer);
            restTimer = null;
        }

        if (key === null) {
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
            // A move is no show or hide, so it has no fade.
            shownKey.value = key;
            isInstant.value = true;
            return;
        }

        restTimer = setTimeout(() => show(key), hoverMs("--chat-d-hover-rest", fallbackRestMs));
    }

    return { shownKey, isShown, isInstant, follow, show, hide, stop };
}
