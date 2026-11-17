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
// A turn is one motion: the pages slide while the surface grows or shrinks from the height of the
// page it leaves to the height of the page it turns to.

import { computed, ComputedRef, inject, InjectionKey, nextTick, provide, shallowReactive } from "vue";
import { focusQuietly } from "./focusTrap.partial";
import { isMotionReduced } from "./viewport.partial";
import { SheetHeight } from "../types.partial";

/** One page a surface can turn to, as its page part registers it. */
export type SurfacePageEntry = {
    /** What the surface's head calls the page while it is on top. */
    title: () => string;

    /** How tall the page asks the surface to be: its content's height, or the one picker height. */
    height: () => SheetHeight;

    /** The drawn page, where focus starts as it is turned to. */
    element: () => HTMLElement | undefined;

    /** Asks the page's owner to close it, for Back and Escape. */
    back: () => void;
};

/** The parts of a surface its pages move. */
export type SurfaceParts = {
    /** Whether the surface is open now. */
    isOpen: () => boolean;

    /** The surface's own element, whose height follows the page on top. */
    surface: () => HTMLElement | undefined;

    /** The surface's scrolling body, where each page keeps its own place; none where pages do not scroll. */
    scroller?: () => HTMLElement | undefined;

    /** The head's Back, drawn while a page is turned to. */
    backElement: () => HTMLElement | undefined;
};

/** A surface's pages, the first one its own body and the rest turned to over it in order. */
export type SurfacePages = {
    /** Whether a page is turned to over the surface's own body. */
    isTurned: ComputedRef<boolean>;

    /** The top page's title, which the head shows and names the surface by while it is turned. */
    title: ComputedRef<string>;

    /** The top page's height while a page is turned to; null while the surface's own body is on top. */
    height: ComputedRef<SheetHeight | null>;

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
 * @param parts The surface's parts the pages move.
 *
 * @returns The pages.
 */
export function provideSurfacePages(parts: SurfaceParts): SurfacePages {
    const stack = shallowReactive<SurfacePageEntry[]>([]);

    /** What had focus as each page was turned to, which focus returns to as it closes. */
    const openers = new Map<SurfacePageEntry, HTMLElement | null>();

    /** Where the page under each page was scrolled to as it was turned to, which Back gives back. */
    const places = new Map<SurfacePageEntry, number>();

    /** The surface's height change still playing, which the next turn takes over from. */
    let resizing: Animation | null = null;

    const top = computed((): SurfacePageEntry | null => stack[stack.length - 1] ?? null);

    /**
     * The surface's height as the person sees it before a turn is drawn, mid-change included, so
     * the next change starts from there; null where the surface is not open to move.
     */
    function heightBefore(): number | null {
        const surface = parts.surface();
        return surface && parts.isOpen() ? surface.getBoundingClientRect().height : null;
    }

    /**
     * After the patch, puts the page on top at its place, then grows or shrinks the surface from
     * its old height to the one that page gives it, over the panel's own duration as the pages
     * slide. Where less motion is asked for the height changes at once and the pages only fade.
     */
    function settleTurn(before: number | null, place: number): void {
        const surface = parts.surface();
        if (!surface || !parts.isOpen()) {
            return;
        }

        const scroller = parts.scroller?.();
        if (scroller) {
            scroller.scrollTop = place;
        }

        // a change still playing gives way first, so the new height is read at its true end
        resizing?.cancel();
        resizing = null;
        if (before === null || typeof surface.animate !== "function" || isMotionReduced()) {
            return;
        }

        const after = surface.getBoundingClientRect().height;
        const style = getComputedStyle(surface);
        const duration = parseFloat(style.getPropertyValue("--chat-t-panel"));
        if (after === before || !duration) {
            return;
        }

        resizing = surface.animate([{ height: `${before}px` }, { height: `${after}px` }], {
            duration,
            easing: style.getPropertyValue("--chat-ease-in").trim()
        });
    }

    function turnTo(entry: SurfacePageEntry): void {
        if (stack.includes(entry)) {
            return;
        }

        openers.set(entry, document.activeElement instanceof HTMLElement ? document.activeElement : null);
        places.set(entry, parts.scroller?.()?.scrollTop ?? 0);
        const before = heightBefore();
        stack.push(entry);

        // After the patch, when the page and the head's Back are drawn; the page starts at its top.
        void nextTick(() => {
            settleTurn(before, 0);
            const start = entry.element()?.querySelector<HTMLElement>("[data-autofocus]") ?? parts.backElement();
            if (parts.isOpen() && top.value === entry && start) {
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
        const place = places.get(entry) ?? 0;
        const before = wasTop ? heightBefore() : null;
        stack.splice(at, 1);
        openers.delete(entry);
        places.delete(entry);

        // After the patch, when the page under is in reach again, back at its place; not when the
        // surface closed with it, whose own trap has already sent focus back to what opened it.
        void nextTick(() => {
            if (!wasTop) {
                return;
            }

            settleTurn(before, place);
            if (parts.isOpen() && opener?.isConnected) {
                focusQuietly(opener, { preventScroll: true });
            }
        });
    }

    const pages: SurfacePages = {
        isTurned: computed(() => stack.length > 0),
        title: computed(() => top.value?.title() ?? ""),
        height: computed(() => top.value?.height() ?? null),
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
