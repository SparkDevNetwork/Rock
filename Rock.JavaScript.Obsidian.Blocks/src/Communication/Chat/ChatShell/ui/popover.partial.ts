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

// The one way every menu, picker, card and tip floats beside what opened it, so they all place,
// close and give focus back alike. A surface draws itself; this owns its coming and going.

import { computed, ComputedRef, nextTick, onScopeDispose, Ref, ref, shallowReactive, watch } from "vue";
import { autoUpdate, computePosition, flip, offset, Placement, shift } from "@Obsidian/Libs/floating-ui";
import { EscapeLayer, focusableSelector, focusQuietly, isDrawn, pullEscapeLayer, pushEscapeLayer } from "./focusTrap.partial";

/** How a popover is placed and closed. */
export type PopoverOptions = {
    /** Where it opens; a getter for a surface whose side follows what it is on, as the message box beside a bubble. */
    placement?: Placement | (() => Placement);

    /** How far off its anchor; a getter where that follows the placement, as a box moved inside a channel's row. */
    offset?: number | (() => number);

    /** Where it goes when its placement has no room, in order, read as it is placed. */
    fallbackPlacements?: () => Placement[];

    /**
     * What it is kept inside, 4px in on every edge, as the message box is kept in its column; the
     * window, 8px in along its side, where none is given.
     */
    boundary?: () => Element | null | undefined;

    /** A box to place beside instead of the anchor, as a card under the whole row a face opened. */
    virtualAnchor?: () => DOMRect | null;

    /** Drawn as a phone's sheet now: not placed, and a press outside is the scrim's to answer. */
    isSheet?: () => boolean;

    returnTo?: () => HTMLElement | null;

    /**
     * A control apart from the anchor that a press counts as inside, as the chip that opened the
     * message box: a press on it is its own to answer, which pressed again closes what it opened.
     */
    alsoInside?: () => Element | null | undefined;

    /** False for a tip, which shows and hides on its own timers and is only placed here. */
    isDismissable?: boolean;
};

/** How far inside a boundary a popover is kept, as the message box inside its column. */
export const boundaryInsetPx = 4;

/** One popover's state and what changes it. */
export type PopoverControl = {
    isOpen: Ref<boolean>;
    floating: Ref<HTMLElement | undefined>;
    anchor: Readonly<Ref<HTMLElement | undefined>>;

    /** Resolves false when it was closed before it was drawn, so no focus moves into nothing. */
    open: () => Promise<boolean>;

    close: (options?: { isFocusReturned?: boolean }) => void;
    toggle: () => Promise<boolean>;
    floatingStyles: ComputedRef<Record<string, string>>;
    origin: ComputedRef<Record<string, string>>;
    isSheet: ComputedRef<boolean>;

    /** Places it now, for a surface that just changed size: its size watcher would be a frame late. */
    update: () => void;

    /**
     * Places it now and resolves once the new place is drawn, for a surface that measures itself
     * where it lands, as the message box revealing its grown size from its old edges.
     */
    place: () => Promise<void>;
};

/**
 * Oldest first. A press inside a newer popover counts as inside every older one: one opened from
 * another is drawn apart from it at the chat's root, and would otherwise close its parent.
 */
const openStack: OpenPopover[] = shallowReactive([]);

/**
 * Whether any menu, picker, card or sheet is open. Only one floating layer shows at a time, so a
 * message's bar rises for no hover while this is true. A tip is never on the stack, so never counts.
 *
 * @returns True while one is open.
 */
export function isFloatingLayerOpen(): boolean {
    return openStack.length > 0;
}

/** Where a popover opens, for a surface that names its own sides, as the message box beside a bubble. */
export type PopoverPlacement = Placement;

/**
 * Tab past either end of a surface drawn apart from its place in the page, as a popover at the
 * chat's root or the message box beside its row: focus goes back to that place without the key
 * being taken, so the browser goes on from there. Tab between the surface's own controls is left
 * to the browser.
 *
 * @param event The key pressed inside the surface.
 * @param tabbable The surface's controls Tab reaches, in order.
 * @param leave Moves focus to the surface's place in the page.
 *
 * @returns True for a Tab, which nothing else should take.
 */
export function tabPastEnd(event: KeyboardEvent, tabbable: HTMLElement[], leave: () => void): boolean {
    if (event.key !== "Tab") {
        return false;
    }

    const edge = event.shiftKey ? tabbable[0] : tabbable[tabbable.length - 1];
    if (tabbable.length === 0 || document.activeElement === edge) {
        leave();
    }

    return true;
}

type OpenPopover = EscapeLayer & {
    contains: (node: Node) => boolean;
    close: (isFocusReturned: boolean) => void;
    onTab: (event: KeyboardEvent) => void;
    isSheet: () => boolean;
};

function onDocumentKeydown(event: KeyboardEvent): void {
    const top = openStack[openStack.length - 1];
    if (top && !event.isComposing && event.key === "Tab" && event.target instanceof Node && top.contains(event.target)) {
        top.onTab(event);
    }
}

/** Captured on its way down, so a part that stops the press on its way up cannot keep one open. */
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
    pushEscapeLayer(popover);
}

function pull(popover: OpenPopover): void {
    const at = openStack.indexOf(popover);
    if (at >= 0) {
        openStack.splice(at, 1);
    }

    pullEscapeLayer(popover);

    if (openStack.length === 0) {
        document.removeEventListener("keydown", onDocumentKeydown, true);
        document.removeEventListener("pointerdown", onDocumentPointerDown, true);
    }
}

/**
 * A floating surface beside an anchor, drawn by the caller or by the Popover part. Its alignment
 * never flips, which would throw a surface near the window's edge to the far side of its anchor;
 * it is placed by left and top, since its pop motion is a transform of its own.
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

    let opener: HTMLElement | null = null;

    /** Counts each open, so an open that was closed before its surface was drawn does not land. */
    let opening = 0;

    // A virtual anchor keeps the real one as its context, so it follows the anchor's own scroller.
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

    const placement = options.placement;
    const distance = options.offset ?? 8;
    const boundary = (): { padding: number, boundary?: Element } => {
        const element = options.boundary?.();
        return element ? { padding: boundaryInsetPx, boundary: element } : { padding: 8 };
    };

    // Kept inside a boundary it is given on both axes: the side it stands on as well as along it.
    // Placed under a point above its column, as a channel row's box whose row top has scrolled out
    // of view, it would otherwise stand over what is drawn above the column, the conversation's
    // header and its buttons. Inside the window alone it slides only along its side, as before.
    const shifting = (): { padding: number, boundary?: Element, crossAxis: boolean } => {
        const within = boundary();
        return { ...within, crossAxis: !!within.boundary };
    };

    const asked = computed((): Placement => (typeof placement === "function" ? placement() : placement) ?? "bottom-end");
    const middleware = [
        offset(() => typeof distance === "function" ? distance() : distance),
        flip(() => ({ crossAxis: false, flipAlignment: false, ...(options.fallbackPlacements ? { fallbackPlacements: options.fallbackPlacements() } : {}), ...boundary() })),
        shift(shifting)
    ];

    // Where it was last placed, and on which side its room put it. Held here rather than in the
    // library's own reactive placing, whose update promises nothing, so a surface can wait on it.
    const x = ref(0);
    const y = ref(0);
    const landed = ref<Placement>(asked.value);
    const isPlaced = ref(false);

    /** Counts each placing, so one overtaken by a later one never lands after it. */
    let placing = 0;

    /** The latest placing, which every earlier one it overtook resolves with. */
    let latest: Promise<void> = Promise.resolve();

    function place(): Promise<void> {
        const beside = reference.value;
        const element = floating.value;
        if (!beside || !element) {
            return latest;
        }

        const run = ++placing;
        latest = computePosition(beside, element, { placement: asked.value, strategy: "fixed", middleware }).then(position => {
            if (run !== placing) {
                return latest;
            }

            // Rounded to the device's pixels, so its edges stay sharp.
            const ratio = window.devicePixelRatio || 1;
            x.value = Math.round(position.x * ratio) / ratio;
            y.value = Math.round(position.y * ratio) / ratio;
            landed.value = position.placement;
            isPlaced.value = true;
            return nextTick();
        });

        return latest;
    }

    function update(): void {
        void place();
    }

    // Placed as it is drawn and kept beside its anchor through scrolls and resizes while both are
    // drawn; placed again whenever the side it asks for changes.
    watch([reference, floating], ([beside, element], _previous, onCleanup) => {
        if (beside && element) {
            onCleanup(autoUpdate(beside, element, update));
        }
    }, { flush: "sync" });
    watch(asked, update, { flush: "sync" });

    const floatingStyles = computed((): Record<string, string> => {
        if (isSheet.value) {
            return {};
        }

        return isPlaced.value ? { position: "fixed", left: `${x.value}px`, top: `${y.value}px` } : { position: "fixed", left: "0", top: "0" };
    });

    // Grown from the anchor's middle, so one flipped above grows up from below rather than from a
    // corner it does not touch; clamped, as an anchor can sit past the surface's end.
    const origin = computed((): Record<string, string> => {
        const isAbove = landed.value.startsWith("top");
        const box = (options.virtualAnchor?.() ?? null) ?? anchor.value?.getBoundingClientRect();
        const width = floating.value?.offsetWidth ?? 0;
        const middle = box ? box.left + box.width / 2 - x.value : width / 2;
        return {
            transformOrigin: `${Math.max(0, Math.min(width, middle))}px ${isAbove ? "100%" : "0"}`,
            "--chat-pop-toward": isAbove ? "1" : "-1"
        };
    });

    const entry: OpenPopover = {
        contains: node => !!floating.value?.contains(node) || !!anchor.value?.contains(node) || !!options.alsoInside?.()?.contains(node),
        close: isFocusReturned => close({ isFocusReturned }),
        takeEscape: () => close({ isFocusReturned: true }),
        onTab,
        isSheet: () => isSheet.value
    };

    /**
     * Tab past either end closes it and hands focus to its anchor without taking the key, so the
     * browser goes on from the anchor rather than from the chat's root, where the surface is drawn.
     * A sheet is modal and keeps Tab inside, through its own trap.
     */
    function onTab(event: KeyboardEvent): void {
        if (isSheet.value) {
            return;
        }

        const tabbable = Array.from(floating.value?.querySelectorAll<HTMLElement>(focusableSelector) ?? [])
            .filter(element => element.tabIndex >= 0 && isDrawn(element));
        tabPastEnd(event, tabbable, () => {
            close({ isFocusReturned: false });
            anchor.value?.focus({ preventScroll: true });
        });
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
        const openedFrom = opener;
        opener = null;
        if (!isFocusReturned) {
            return;
        }

        // Quietly: the person is leaving the surface, not reaching for its anchor, so no tip rises.
        // At once where the target is drawn, so a dialog a menu's choice opens takes the control
        // the menu gave focus back to as its own way back.
        const named = options.returnTo?.() ?? null;
        if (!options.returnTo || named?.isConnected) {
            const target = named ?? openedFrom ?? anchor.value;
            if (target?.isConnected) {
                focusQuietly(target, { preventScroll: true });
            }

            return;
        }

        // A surface that turns back into the control it names, as the message box's tray into its
        // bar's Add reaction, gives focus to that control once the patch the close causes draws it;
        // unless what the close led to took focus first.
        const surface = floating.value;
        const focusedAtClose = document.activeElement;
        void nextTick(() => {
            const active = document.activeElement;
            const isTaken = active !== focusedAtClose && active instanceof HTMLElement && active !== document.body && active.isConnected && !surface?.contains(active);
            if (isTaken) {
                return;
            }

            const target = options.returnTo?.() ?? openedFrom ?? anchor.value;
            if (target?.isConnected) {
                focusQuietly(target, { preventScroll: true });
            }
        });
    }

    async function toggle(): Promise<boolean> {
        if (isOpen.value) {
            close();
            return false;
        }

        return open();
    }

    onScopeDispose(() => pull(entry));

    return { isOpen, floating, anchor, open, close, toggle, floatingStyles, origin, isSheet, update, place };
}
