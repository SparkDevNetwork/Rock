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

// The one way the chat opens a floating surface beside what opened it, for every menu, picker,
// card and tip: where it is placed, what closes it, where it grows from and where focus goes as it
// closes. A surface draws itself; this decides everything about its coming and going.

import { computed, ComputedRef, nextTick, onScopeDispose, Ref, ref } from "vue";
import { autoUpdate, flip, offset, Placement, shift, useFloating } from "@Obsidian/Libs/floating-ui";
import { focusableSelector, focusQuietly, isDrawn } from "./focusTrap.partial";

/** How a popover is placed and closed. */
export type PopoverOptions = {
    /** Where it opens from its anchor; below at its end by default. */
    placement?: Placement;

    /** The gap to its anchor, in pixels; 8 by default. */
    offset?: number;

    /**
     * A box to place beside instead of the anchor's own, as a card under a whole message row that
     * a face opened. Read at every placement; null falls back to the anchor.
     */
    virtualAnchor?: () => DOMRect | null;

    /**
     * Whether it is drawn as a bottom sheet now, as on a phone: then it is not placed, and a press
     * outside it is the sheet's scrim's to answer.
     */
    isSheet?: () => boolean;

    /** What focus goes back to as it closes by key, when not what had focus as it opened. */
    returnTo?: () => HTMLElement | null;

    /**
     * Whether a press outside, Escape and Tab past its end close it; true by default. A tip that
     * shows on its own timers is placed here and closed by its own rules.
     */
    isDismissable?: boolean;
};

/** One popover's state and what changes it. */
export type PopoverControl = {
    isOpen: Ref<boolean>;

    /** The surface element, set by the surface as it is drawn. */
    floating: Ref<HTMLElement | undefined>;

    /** The element it opens beside. */
    anchor: Readonly<Ref<HTMLElement | undefined>>;

    /**
     * Opens it, resolving once the surface is drawn: true when it is still open then, false when
     * it was closed before, so the caller moves no focus into a surface that never came.
     */
    open: () => Promise<boolean>;

    /** Closes it; with isFocusReturned, focus goes back to what opened it, when that is still on the page. */
    close: (options?: { isFocusReturned?: boolean }) => void;

    /** Opens it, or closes it when open, as its anchor's press does. */
    toggle: () => Promise<boolean>;

    /** Where the surface is placed; empty while it is a sheet. */
    floatingStyles: ComputedRef<Record<string, string>>;

    /** Where the surface grows from and which way it moves, toward its anchor. */
    origin: ComputedRef<Record<string, string>>;

    /** Whether it is drawn as a bottom sheet now. */
    isSheet: ComputedRef<boolean>;

    /**
     * Places it again now, for a surface whose own size has just changed, as the @ picker's does
     * when its rows do: the watcher on its size would place it a frame late, a jump as it shows.
     */
    update: () => void;
};

/**
 * The popovers open now, oldest first. Escape closes the newest alone, and a press inside a newer
 * one counts as inside every older one, since a popover opened from another is drawn apart from it
 * at the chat's root and would otherwise close its parent when chosen in.
 */
const openStack: OpenPopover[] = [];

type OpenPopover = {
    contains: (node: Node) => boolean;
    close: (isFocusReturned: boolean) => void;
    onTab: (event: KeyboardEvent) => void;
    isSheet: () => boolean;
};

/** Escape closes the newest popover only, before any part under it sees the key. */
function onDocumentKeydown(event: KeyboardEvent): void {
    const top = openStack[openStack.length - 1];
    if (!top || event.isComposing) {
        return;
    }

    if (event.key === "Escape") {
        event.preventDefault();
        event.stopPropagation();
        top.close(true);
    }
    else if (event.key === "Tab" && event.target instanceof Node && top.contains(event.target)) {
        top.onTab(event);
    }
}

/**
 * A press outside every open popover that does not count it as inside closes it, newest first.
 * Captured on its way down, so a part that stops the press on its way up cannot keep one open.
 */
function onDocumentPointerDown(event: Event): void {
    const target = event.target as Node;
    for (let at = openStack.length - 1; at >= 0; at--) {
        const popover = openStack[at];
        const isInsideNewer = openStack.slice(at + 1).some(newer => newer.contains(target));
        if (popover.isSheet() || popover.contains(target) || isInsideNewer) {
            continue;
        }

        // focus stays where the press put it, as the person pressed something else
        popover.close(false);
    }
}

function push(popover: OpenPopover): void {
    if (openStack.length === 0) {
        document.addEventListener("keydown", onDocumentKeydown, true);
        document.addEventListener("pointerdown", onDocumentPointerDown, true);
    }

    openStack.push(popover);
}

function pull(popover: OpenPopover): void {
    const at = openStack.indexOf(popover);
    if (at >= 0) {
        openStack.splice(at, 1);
    }

    if (openStack.length === 0) {
        document.removeEventListener("keydown", onDocumentKeydown, true);
        document.removeEventListener("pointerdown", onDocumentPointerDown, true);
    }
}

/**
 * A floating surface beside an anchor. The caller draws the surface while isOpen, binds its
 * element to floating and its style to floatingStyles and origin, or draws it with the Popover
 * part, which does all three.
 *
 * Placed below at the anchor's end unless told otherwise, flipped to the other side when there is
 * no room and slid along inside the window 8px clear of its edge; the alignment never flips, which
 * would throw a surface near the window's edge to the far side of what opened it. Placed by left
 * and top, never a transform, since the surface's pop motion is a transform of its own.
 *
 * @param anchor What it opens beside, and gives focus back to.
 * @param floating The surface's element, while drawn.
 * @param options How it is placed and closed.
 *
 * @returns The popover's state and what changes it.
 */
export function usePopover(anchor: Readonly<Ref<HTMLElement | undefined>>, floating: Ref<HTMLElement | undefined>, options: PopoverOptions = {}): PopoverControl {
    const isOpen = ref(false);
    const isSheet = computed((): boolean => options.isSheet?.() ?? false);
    const isDismissable = options.isDismissable ?? true;

    /** What had focus as it opened, given focus back on a close by key. */
    let opener: HTMLElement | null = null;

    /** Counts each open, so an open that was closed before its surface was drawn does not land. */
    let opening = 0;

    // A sheet is placed by its own rules, so nothing is floated for it. A virtual anchor keeps the
    // real one as its context, so the placement follows the anchor's own scrolling container.
    const reference = computed(() => {
        const element = anchor.value;
        if (isSheet.value || !element) {
            return undefined;
        }

        const virtual = options.virtualAnchor;
        return virtual
            ? { getBoundingClientRect: (): DOMRect => virtual() ?? element.getBoundingClientRect(), contextElement: element }
            : element;
    });

    const floated = useFloating(reference, floating, {
        placement: options.placement ?? "bottom-end",
        strategy: "fixed",
        transform: false,
        middleware: [offset(options.offset ?? 8), flip({ crossAxis: false, flipAlignment: false }), shift({ padding: 8 })],
        whileElementsMounted: autoUpdate
    });

    const floatingStyles = computed((): Record<string, string> => isSheet.value ? {} : floated.floatingStyles.value as Record<string, string>);

    /**
     * Grown from the anchor's middle along the surface's edge nearest it, and moving toward it, so a
     * surface flipped above its anchor grows up from below rather than down from a corner it does
     * not touch. Clamped to the surface, as an anchor can sit past its end.
     */
    const origin = computed((): Record<string, string> => {
        const isAbove = floated.placement.value.startsWith("top");
        const box = (options.virtualAnchor?.() ?? null) ?? anchor.value?.getBoundingClientRect();
        const width = floating.value?.offsetWidth ?? 0;
        const middle = box ? box.left + box.width / 2 - (floated.x.value ?? 0) : width / 2;
        return {
            transformOrigin: `${Math.max(0, Math.min(width, middle))}px ${isAbove ? "100%" : "0"}`,
            "--chat-pop-toward": isAbove ? "1" : "-1"
        };
    });

    const entry: OpenPopover = {
        contains: node => !!floating.value?.contains(node) || !!anchor.value?.contains(node),
        close: isFocusReturned => close({ isFocusReturned }),
        onTab,
        isSheet: () => isSheet.value
    };

    /**
     * Tab past either end of the surface closes it and hands focus to its anchor without taking
     * the key, so the browser moves on from the anchor as if the surface had never been there:
     * it is drawn at the chat's root, away from its anchor, where Tab would go on from instead.
     * A surface with nothing to tab to, as a menu whose rows the arrows move between, closes on
     * any Tab. A sheet is modal and keeps Tab inside, through its own trap.
     */
    function onTab(event: KeyboardEvent): void {
        if (isSheet.value) {
            return;
        }

        const tabbable = Array.from(floating.value?.querySelectorAll<HTMLElement>(focusableSelector) ?? [])
            .filter(element => element.tabIndex >= 0 && isDrawn(element));
        const edge = event.shiftKey ? tabbable[0] : tabbable[tabbable.length - 1];
        if (tabbable.length > 0 && document.activeElement !== edge) {
            return;
        }

        close({ isFocusReturned: false });
        anchor.value?.focus({ preventScroll: true });
    }

    async function open(): Promise<boolean> {
        if (isOpen.value) {
            return true;
        }

        const at = ++opening;
        opener = document.activeElement instanceof HTMLElement && document.activeElement !== document.body ? document.activeElement : null;
        isOpen.value = true;
        if (isDismissable) {
            push(entry);
        }

        await nextTick();
        return at === opening && isOpen.value;
    }

    function close({ isFocusReturned = false }: { isFocusReturned?: boolean } = {}): void {
        opening++;
        pull(entry);
        if (!isOpen.value) {
            return;
        }

        isOpen.value = false;
        if (isFocusReturned) {
            // quietly: the person is leaving the surface, not reaching for its anchor, so no tip rises
            const target = options.returnTo?.() ?? opener ?? anchor.value;
            if (target?.isConnected) {
                focusQuietly(target, { preventScroll: true });
            }
        }

        opener = null;
    }

    async function toggle(): Promise<boolean> {
        if (isOpen.value) {
            close();
            return false;
        }

        return open();
    }

    onScopeDispose(() => pull(entry));

    return { isOpen, floating, anchor, open, close, toggle, floatingStyles, origin, isSheet, update: floated.update };
}
