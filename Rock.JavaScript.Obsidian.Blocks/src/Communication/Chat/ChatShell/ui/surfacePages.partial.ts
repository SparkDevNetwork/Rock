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

// A modal surface's pages, in one place for the sheet and the panel dialog: a choice that leads to
// another, as the plus menu's GIF or settings' Blocked people, turns the same surface into the
// next page instead of stacking a second surface over it. The surface holds the stack and draws
// the head (Back and the top page's title); each page registers itself as it opens and closes.

import { computed, ComputedRef, inject, InjectionKey, nextTick, provide, shallowReactive } from "vue";
import { focusQuietly } from "./focusTrap.partial";

/** One page a surface can turn to, as its page part registers it. */
export type SurfacePageEntry = {
    /** What the surface's head calls the page while it is on top. */
    title: () => string;

    /** The drawn page, where focus starts as it is turned to. */
    element: () => HTMLElement | undefined;

    /** Asks the page's owner to close it, for Back and Escape. */
    back: () => void;
};

/** A surface's pages, the first one its own body and the rest turned to over it in order. */
export type SurfacePages = {
    /** Whether a page is turned to over the surface's own body. */
    isTurned: ComputedRef<boolean>;

    /** The top page's title, which the head shows and names the surface by while it is turned. */
    title: ComputedRef<string>;

    /**
     * Whether a page lies under another, set aside and out of reach.
     *
     * @param entry The page; null for the surface's own first page.
     */
    isCovered: (entry: SurfacePageEntry | null) => boolean;

    /** Goes back one page: the top page's owner is asked to close it. */
    back: () => void;

    /** A page opened: it goes on top, and focus moves into it. */
    turnTo: (entry: SurfacePageEntry) => void;

    /** A page closed: it leaves the stack, and focus goes back to what turned to it. */
    turnFrom: (entry: SurfacePageEntry) => void;
};

const surfacePagesKey: InjectionKey<SurfacePages> = Symbol("chat-surface-pages");

/**
 * Gives a surface its pages, for the pages drawn inside it. Focus as a page opens goes to the
 * control the page marks data-autofocus, else Back, quietly, since the person reached for the row
 * that turned it, not for Back; as the top page closes it goes back to the control that turned it.
 * A surface closing with its pages is left to its own trap, which returns focus to its opener.
 *
 * @param isOpen Whether the surface is open now.
 * @param backElement The head's Back, drawn while a page is turned to.
 *
 * @returns The pages.
 */
export function provideSurfacePages(isOpen: () => boolean, backElement: () => HTMLElement | undefined): SurfacePages {
    const stack = shallowReactive<SurfacePageEntry[]>([]);

    /** What had focus as each page was turned to, which focus returns to as it closes. */
    const openers = new Map<SurfacePageEntry, HTMLElement | null>();

    const top = computed((): SurfacePageEntry | null => stack[stack.length - 1] ?? null);

    function turnTo(entry: SurfacePageEntry): void {
        if (stack.includes(entry)) {
            return;
        }

        openers.set(entry, document.activeElement instanceof HTMLElement ? document.activeElement : null);
        stack.push(entry);

        // After the patch, when the page and the head's Back are drawn.
        void nextTick(() => {
            const start = entry.element()?.querySelector<HTMLElement>("[data-autofocus]") ?? backElement();
            if (isOpen() && top.value === entry && start) {
                focusQuietly(start, { preventScroll: true });
            }
        });
    }

    function turnFrom(entry: SurfacePageEntry): void {
        const at = stack.indexOf(entry);
        if (at < 0) {
            return;
        }

        const wasTop = at === stack.length - 1;
        const opener = openers.get(entry) ?? null;
        stack.splice(at, 1);
        openers.delete(entry);

        // After the patch, when the page under is in reach again; not when the surface closed
        // with it, whose own trap has already sent focus back to what opened the surface.
        void nextTick(() => {
            if (wasTop && isOpen() && opener?.isConnected) {
                focusQuietly(opener, { preventScroll: true });
            }
        });
    }

    const pages: SurfacePages = {
        isTurned: computed(() => stack.length > 0),
        title: computed(() => top.value?.title() ?? ""),
        isCovered: entry => entry === null ? stack.length > 0 : stack.includes(entry) && top.value !== entry,
        back: () => top.value?.back(),
        turnTo,
        turnFrom
    };

    provide(surfacePagesKey, pages);
    return pages;
}

/**
 * The pages of the surface a page is drawn in; null outside one.
 *
 * @returns The pages, or null.
 */
export function useSurfacePages(): SurfacePages | null {
    return inject(surfacePagesKey, null);
}
