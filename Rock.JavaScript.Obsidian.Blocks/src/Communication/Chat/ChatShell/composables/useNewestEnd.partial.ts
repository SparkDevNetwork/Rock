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
// Whether the reader of a view is at its newest message: the conversation's feed or the thread's
// drawer. It is theirs to change by scrolling, never measured again whenever the view happens to
// change, so a composer growing under the scroller, a photo decoding or a page replacing the held
// one keeps a reader at the newest message there and leaves a reader scrolled up alone. The view's
// tail asks it whether the reader follows, so an arrival plays the same way in both views.

import { onBeforeUnmount, onMounted, readonly, Ref, ref } from "vue";
import { nearBottomPx, endSlackPx } from "../feedLayout.partial";

/**
 * Where the newest end of a scroller is, read from layout so no motion in flight moves it: a
 * transform on the content grows the scroller's whole height while it plays, so a follow to that
 * height scrolls into the motion and the browser pulls it back frame by frame. The scroller is
 * its content's offset parent, so the content's offset is from the scroller's own top.
 *
 * @param scroller The view's scroller.
 * @param content The box that holds what it draws.
 *
 * @returns The scroll top at the newest end.
 */
export function endOf(scroller: HTMLElement, content: HTMLElement): number {
    const padding = parseFloat(getComputedStyle(scroller).paddingBottom);
    return Math.max(0, content.offsetTop + content.offsetHeight + (Number.isNaN(padding) ? 0 : padding) - scroller.clientHeight);
}

/** Where a view's reader is, and the ways the view moves them. */
export type NewestEnd = {
    /** Whether the reader is at the newest message. */
    isAtNewest: Readonly<Ref<boolean>>;

    /** Reads the reader's place from where the scroller is scrolled to, on a scroll and before a change lands. */
    read: () => void;

    /** The scroll top at the newest end, read from layout so a glide in flight does not move it. */
    endOf: () => number;

    /** Scrolls to the newest message, where the reader is then. */
    toNewest: () => void;

    /**
     * The reader is going to a place, as a jump or a landing takes them: they are where it goes from
     * the moment it starts, so its first frames, still near the bottom, and rows settling on the way
     * neither read as the reader at the newest message nor pull them back to it.
     */
    goingTo: (top: number) => void;

    /** Takes the reader to a place, smoothly unless the person asked for less motion. */
    moveTo: (top: number) => void;
};

/**
 * A view's reader at its newest end, from the moment it mounts: the scroller and the content are
 * watched for a change of size, which keeps a reader at the newest message there.
 *
 * @param scroller The view's scroller.
 * @param content The box that holds what it draws.
 * @param onResized What else the view places once a size changed, after the reader is kept.
 *
 * @returns The reader's place and the ways to move them.
 */
export function useNewestEnd(scroller: () => HTMLElement | null, content: Ref<HTMLElement | null>, onResized?: () => void): NewestEnd {
    const isAtNewest = ref(true);
    // where the last read found the scroller, so a read can tell a move down from a move up
    let readerTop = 0;
    let sizeObserver: ResizeObserver | null = null;

    function endOfScroller(element: HTMLElement): number {
        return content.value ? endOf(element, content.value) : element.scrollHeight - element.clientHeight;
    }

    function toNewest(): void {
        const element = scroller();
        if (element) {
            element.scrollTop = endOfScroller(element);
            isAtNewest.value = true;
            readerTop = element.scrollTop;
        }
    }

    function goingTo(top: number): void {
        const element = scroller();
        if (element) {
            const end = endOfScroller(element);
            isAtNewest.value = end - Math.min(Math.max(0, top), end) < nearBottomPx;
            readerTop = element.scrollTop;
        }
    }

    onMounted(() => {
        const element = scroller();
        if (typeof ResizeObserver === "undefined" || !element) {
            return;
        }
        // The browser keeps a scroll's top as a box changes size, which would leave the newest
        // rows under the composer, or above the foot of a row that grew.
        sizeObserver = new ResizeObserver(() => {
            if (isAtNewest.value) {
                toNewest();
            }
            onResized?.();
        });
        sizeObserver.observe(element);
        if (content.value) {
            sizeObserver.observe(content.value);
        }
    });

    onBeforeUnmount(() => {
        sizeObserver?.disconnect();
        sizeObserver = null;
    });

    return {
        isAtNewest: readonly(isAtNewest),

        // A move down into the newest message's reach, or a scroll that lands at the very end whoever
        // moved it (the browser clamps a reader to the end when content shrinks), puts them at it,
        // and only a move up out of the reach takes them from it. With no move, nothing changes,
        // whatever the content did meanwhile. The first frames of a smooth scroll up are still in
        // reach but not at the end, so they stay a move up.
        read: () => {
            const element = scroller();
            if (!element) {
                return;
            }
            const top = element.scrollTop;
            const fromEnd = endOfScroller(element) - top;
            const isNear = fromEnd < nearBottomPx;
            if (isNear && (top > readerTop || fromEnd <= endSlackPx)) {
                isAtNewest.value = true;
            }
            else if (!isNear && top < readerTop) {
                isAtNewest.value = false;
            }
            readerTop = top;
        },

        endOf: () => {
            const element = scroller();
            return element ? endOfScroller(element) : 0;
        },

        toNewest,

        goingTo,

        moveTo: top => {
            const element = scroller();
            if (!element) {
                return;
            }
            const target = Math.min(Math.max(0, top), endOfScroller(element));
            goingTo(target);
            const isReduced = window.matchMedia?.("(prefers-reduced-motion: reduce)").matches;
            element.scrollTo?.({ top: target, behavior: isReduced ? "auto" : "smooth" });
        }
    };
}
