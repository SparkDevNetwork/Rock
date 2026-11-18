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
// page it leaves to the height of the page it turns to. Where the pages share one scroller, as a
// panel's body, the page sliding out of front is drawn as the view the person last saw of it, so
// the slide never shows its top or blank space below its end.

import { computed, ComputedRef, inject, InjectionKey, nextTick, provide, shallowReactive, watch } from "vue";
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

    /** Asks the page's owner to close it, for Back, Escape and the surface closing. */
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
};

/** A surface's height and side inset as drawn, which a turn moves from one page's to the next's. */
type SurfaceBox = { height: number, paddingLeft: string, paddingRight: string };

/**
 * The view a page was last seen in, which it is drawn as while it is out of front, set aside or
 * going back, so its slide shows what the person saw rather than its own top.
 */
export type SurfacePageView = {
    /** Where the page's box stands in the pages, in pixels, so it sits where it was seen at the scroll the turn sets. */
    top: number;

    /** How much of the page was seen, in pixels, which its box is no taller than. */
    height: number;

    /** How far the page was scrolled past its own top, in pixels, which it is scrolled to within its box. */
    scrolled: number;
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

    /**
     * The view a page out of front is drawn as; null while it is in front, or where its surface has
     * no shared scroller and each page keeps its own scroll inside it.
     *
     * @param entry The page; null for the surface's own first page.
     */
    viewOf: (entry: SurfacePageEntry | null) => SurfacePageView | null;

    /**
     * The surface's own first page gives its drawing, which a turn reads as it sets the page aside.
     *
     * @param element The drawn first page.
     */
    holdFirst: (element: () => HTMLElement | undefined) => void;

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
 * control the page marks data-autofocus, else to the page itself, which its title names, so a
 * screen reader says where the person now is; quietly, since the person reached for the row that
 * turned it. As the top page closes focus goes back to the control that turned it. A surface
 * closing closes its pages with it, and its own trap returns focus to its opener.
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

    /** The view each page out of front was last seen in, the first page under null. */
    const views = shallowReactive(new Map<SurfacePageEntry | null, SurfacePageView>());

    /** The surface's own first page as drawn. */
    let firstElement: () => HTMLElement | undefined = () => undefined;

    /** The surface's height change still playing, which the next turn takes over from. */
    let resizing: Animation | null = null;

    const top = computed((): SurfacePageEntry | null => stack[stack.length - 1] ?? null);

    /**
     * Reads the page in front as the person sees it, just before a turn takes it out of front:
     * how far its top has scrolled out of sight, how much of it the scroller shows, and where its
     * box must stand in the pages to sit where it was seen once the turn sets the scroller to its
     * new place. A page in front starts at the pages' top, so its box moves by the scroll the turn
     * takes away. Null where the pages share no scroller, or the surface is closing.
     *
     * @param place Where the turn sets the scroller.
     */
    function seenView(place: number): SurfacePageView | null {
        const scroller = parts.scroller?.();
        const element = top.value?.element() ?? firstElement();
        if (!scroller || !element || !parts.isOpen()) {
            return null;
        }

        const viewTop = scroller.getBoundingClientRect().top + scroller.clientTop;
        const page = element.getBoundingClientRect();
        const scrolled = Math.max(0, viewTop - page.top);
        const seen = Math.min(page.bottom, viewTop + scroller.clientHeight) - Math.max(page.top, viewTop);
        return { top: place - scroller.scrollTop + scrolled, height: Math.max(0, seen), scrolled };
    }

    /**
     * The page in front leaves front: it is drawn from now as the view it was seen in, or as its
     * own top where there is none to keep.
     */
    function holdView(entry: SurfacePageEntry | null, place: number): void {
        const view = seenView(place);
        if (view) {
            views.set(entry, view);
        }
        else {
            views.delete(entry);
        }
    }

    /**
     * The surface's height and side inset as the person sees them, mid-change included, so the
     * next change starts from there; null where the surface is not open to move. The inset moves
     * with the height: a page that runs to the surface's edges, as a picker does, takes it away.
     */
    function boxOf(): SurfaceBox | null {
        const surface = parts.surface();
        if (!surface || !parts.isOpen()) {
            return null;
        }
        const style = getComputedStyle(surface);
        return { height: surface.getBoundingClientRect().height, paddingLeft: style.paddingLeft, paddingRight: style.paddingRight };
    }

    /**
     * After the patch, puts the page on top at its place, then grows or shrinks the surface from
     * its old height and inset to the ones that page gives it, over the panel's own duration as
     * the pages slide. Where less motion is asked for both change at once and the pages only fade.
     */
    function settleTurn(before: SurfaceBox | null, place: number): void {
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

        const after = boxOf();
        const style = getComputedStyle(surface);
        const duration = parseFloat(style.getPropertyValue("--chat-t-panel"));
        const isSame = after !== null && after.height === before.height
            && after.paddingLeft === before.paddingLeft && after.paddingRight === before.paddingRight;
        if (!after || isSame || !duration) {
            return;
        }

        const frameOf = (box: SurfaceBox): Keyframe => ({ height: `${box.height}px`, paddingLeft: box.paddingLeft, paddingRight: box.paddingRight });
        resizing = surface.animate([frameOf(before), frameOf(after)], {
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
        const before = boxOf();
        // the page under slides away as it was seen, while the new page starts at its top
        holdView(top.value, 0);
        views.delete(entry);
        stack.push(entry);

        // After the patch, when the page and the head are drawn; the page starts at its top.
        void nextTick(() => {
            settleTurn(before, 0);
            const page = entry.element();
            const start = page?.querySelector<HTMLElement>("[data-autofocus]") ?? page;
            if (parts.isOpen() && top.value === entry && start) {
                focusQuietly(start, { preventScroll: true });
            }
        });
    }

    function turnFrom(entry: SurfacePageEntry): void {
        const at = stack.indexOf(entry);
        if (at < 0) {
            // a page gone for good, its last leave long over
            views.delete(entry);
            return;
        }

        const wasTop = at === stack.length - 1;
        const opener = openers.get(entry) ?? null;
        const place = places.get(entry) ?? 0;
        const before = wasTop ? boxOf() : null;
        // the page on top goes back as it was seen, over the page under back in front at its place;
        // a page closed while covered leaves as the view it was set aside in
        if (wasTop) {
            holdView(entry, place);
        }
        stack.splice(at, 1);
        if (wasTop) {
            views.delete(top.value);
        }
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

    // A surface closing closes every page on it, the top one first, by each page's owner, so no
    // owner is left holding a page open and the surface never opens again already turned.
    watch(parts.isOpen, isOpen => {
        if (!isOpen) {
            for (const entry of [...stack].reverse()) {
                entry.back();
            }
        }
    }, { flush: "sync" });

    const pages: SurfacePages = {
        isTurned: computed(() => stack.length > 0),
        title: computed(() => top.value?.title() ?? ""),
        height: computed(() => top.value?.height() ?? null),
        isCovered: entry => entry === null ? stack.length > 0 : stack.includes(entry) && top.value !== entry,
        viewOf: entry => views.get(entry) ?? null,
        holdFirst: element => {
            firstElement = element;
        },
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
