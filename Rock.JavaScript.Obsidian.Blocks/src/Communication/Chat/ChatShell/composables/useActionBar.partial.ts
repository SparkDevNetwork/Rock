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
// A message's one box, and when it shows: its actions as a bar, the quick tray Add reaction turns
// it into, and the emoji picker All emoji grows it into. A scrolling column of messages owns a
// single box, so two can never be on screen in it: each row that offers one only lends it a dock,
// and the box is placed against the dock of the message it is on, drawn in the chat's root so the
// column's edges never cut it. It first shows once the pointer rests on a message, then follows
// the pointer from message to message at once, with no fade, and hides a moment after the pointer
// leaves the messages and the box. A menu or the tray open from it, or a keyboard's focus inside
// it, holds it where it is; a keyboard reaching a message shows it there at once. While any other
// floating layer is open, a menu, a card or a picker anywhere, the pointer raises no box.
//
// The block holds which box has grown past its bar, one at a time across the conversation and
// the thread drawer, with what every message row reads of it. The conversation's feed and the
// thread drawer each keep one column, binding the handlers below on their scroller, whose rows
// each mark themselves with data-hot-id; the box marks itself with its message's, so the pointer
// or focus on it counts as on its message.

import { computed, ComputedRef, inject, InjectionKey, onScopeDispose, provide, reactive, Ref, ref, shallowReactive, shallowRef, ShallowRef, watch } from "vue";
import { PickerOpening, PickerSource } from "../components/emojiPicker.partial.obs";
import { ChatShell } from "../shell.partial";
import { BoxLayer, ColumnKind, TimelineMessage } from "../types.partial";
import { isFloatingLayerOpen } from "../ui/popover.partial";
import { isKeyboardFocus } from "../ui/tooltip.partial";
import { useHoverIntent } from "./useHoverIntent.partial";
import { ReactionHost } from "./useReactions.partial";

/** What a row lends the column's box: where it is placed, what its bar shows and where its choices go. */
export type ActionBarDock = {
    /** The element the box is placed against, beside the message's bubble or on a channel's row. */
    element: HTMLElement;

    /** The bar's props for the row's message. */
    props: ComputedRef<Record<string, unknown>>;

    /** The bar's handlers for the row's message. */
    handlers: Record<string, (...args: never[]) => void>;
};

/** One column's box, and the handlers its scroller binds. */
export type ColumnActionBar = {
    /** The dock the box is on: the message it shows on, or the one it last showed on while it fades. */
    dock: ComputedRef<ActionBarDock | null>;

    /** Whether the box shows. */
    isShown: Ref<boolean>;

    /**
     * Whether the box shows on a message, read by that message's row: kept per message, so the box
     * moving wakes the row it leaves and the row it reaches, never every row of the column.
     */
    isOn: (messageId: number) => boolean;

    /** Whether the last show or hide skips the fade, as a scroll's or a keyboard leaving does. */
    isInstant: Ref<boolean>;

    /** The drawn box, which the chat's root holds apart from the column; what the box sets as it draws. */
    box: Ref<HTMLElement | undefined>;

    /** A row lends its dock while its message offers a box, and takes it back when it no longer does. */
    offer: (messageId: number, dock: ActionBarDock) => void;
    withdraw: (messageId: number, dock: ActionBarDock) => void;

    onPointerOver: (event: PointerEvent) => void;
    onPointerLeave: () => void;
    onPointerMove: (event: PointerEvent) => void;
    onScroll: () => void;
    onFocusIn: (event: FocusEvent) => void;
    onFocusOut: (event: FocusEvent) => void;

    /** The box reports its menu or its tray holding it, and letting go. */
    onHoldChange: (isHeld: boolean) => void;

    /** Stops every timer, as the column goes. */
    stop: () => void;
};

/**
 * A box grown past its bar: the message it reacts to, by id, in which column, the tray or the
 * picker, what it is placed beside, the control that opened it, the quick reactions that control
 * already shows, which the tray leaves out, and whether a keyboard opened it.
 */
export type HeldBox = {
    key: number;
    column: ColumnKind;
    channelId: string;
    messageId: number;
    layer: Exclude<BoxLayer, "bar">;
    anchor: HTMLElement;
    opener: HTMLElement;
    offered: string[];
    isKeyboard: boolean;
};

/** The block's boxes: the one grown past its bar, what the rows and every emoji picker read, and the ways to open, grow, close and choose. */
export type ActionBars = {
    /** The box grown past its bar, at most one in the block; each opening is placed and drawn afresh. */
    held: ShallowRef<HeldBox | null>;

    /**
     * What every message row reads of the reactions. Built once and read through, so a row follows
     * the shell a birthdate replaces and the box growing, without every row being handed anew.
     */
    reactionHost: ReactionHost;

    /** What every emoji picker draws: the favorites at once, the catalog once fetched, the recents, the church's name. */
    emojiSource: ComputedRef<PickerSource>;

    /** The emoji the reader reacted to the held box's message with, which its tray and picker mark as theirs. */
    pickerMine: ComputedRef<string[]>;

    /** Which column an element is drawn in. */
    kindOf: (element: Element | null | undefined) => ColumnKind;

    /** Opens a column's box for a message, on its tray unless told; asked again by what opened it, it closes instead. */
    openReactionPicker: (column: ColumnKind, channelId: string, message: TimelineMessage, anchor: HTMLElement, opener?: HTMLElement, opening?: PickerOpening) => void;

    /** All emoji: the held tray grows into the picker. */
    showAllEmoji: () => void;

    closeReactionPicker: () => void;

    /**
     * Marks a message held by its box, grown or with its menu open, so the message's who-reacted
     * card stays shut under it; the release given back clears the mark while it is still its own.
     */
    holdMessage: (messageId: number, anchor: HTMLElement) => () => void;

    onPickerReact: (code: string) => void;
};

const actionBarKey: InjectionKey<ColumnActionBar> = Symbol("chat-action-bar");
const actionBarsKey: InjectionKey<ActionBars> = Symbol("chat-action-bars");

/**
 * The message a row of the column stands for, from the element an event reached: a row, or the
 * box, which carries its message's mark.
 *
 * @param target The event's target.
 *
 * @returns The message's id, or null outside any row that offers a box.
 */
function hotIdAt(target: EventTarget | null): number | null {
    const row = target instanceof Element ? target.closest<HTMLElement>("[data-hot-id]") : null;
    const id = Number(row?.dataset.hotId);
    return row && Number.isSafeInteger(id) ? id : null;
}

/**
 * Holds the block's boxes for its life and hands them to every column and row. The frame reads
 * the shell through, since a birthdate save replaces the shell while the rows and the boxes stay;
 * the shell's reactions hold only the reactions themselves.
 *
 * @param shell The block's shell, read on every use since a birthdate replaces it.
 * @param churchName The church's name, which the picker titles its own emoji with.
 * @param threadPane The pane the thread drawer is drawn in, which tells its column from the conversation's.
 *
 * @returns The block's boxes.
 */
export function provideActionBars(shell: Readonly<Ref<ChatShell>>, churchName: string, threadPane: () => HTMLElement | undefined): ActionBars {
    const held = shallowRef<HeldBox | null>(null);
    let heldCount = 0;

    // The anchor of the box holding a message, grown or with its menu open, by the message, which
    // every row asks about its own message alone: a
    // reactive map tracks each message apart, so the box growing or closing wakes the rows of that
    // message and leaves the rest of the feed still. One reference read by every row would redraw
    // every row on each open and close.
    const anchorsByMessage = shallowReactive(new Map<number, HTMLElement>());

    /** Lets go of the grown box's mark on its message. */
    let releaseHeld: (() => void) | null = null;

    const reactionHost = reactive<ReactionHost>({
        get favorites() {
            return shell.value.reactions.favorites;
        },
        listReactors: (messageId, code, after) => shell.value.reactions.listReactors(messageId, code, after),
        pickerAnchorOf: messageId => anchorsByMessage.get(messageId) ?? null
    });

    const emojiSource = computed((): PickerSource => ({
        favorites: shell.value.reactions.favorites,
        catalog: shell.value.reactions.state.catalog,
        recents: shell.value.reactions.state.recents,
        churchName
    }));

    // Read through the page on every change rather than from the message the box opened on,
    // since a page merge puts a new object in that message's place while the box is up.
    const pickerMine = computed((): string[] => {
        const box = held.value;
        const message = box ? heldMessage(box.channelId, box.messageId) : null;
        return (message?.reactions ?? []).filter(reaction => reaction.mine).map(reaction => reaction.code);
    });

    /**
     * The message as the client holds it now: on its conversation's timeline, else in the open
     * thread, where a reply shown only in its thread is the one copy there is.
     */
    function heldMessage(channelId: string, messageId: number): TimelineMessage | null {
        const onTimeline = shell.value.timelines.state(channelId).messages.find(message => message.id === messageId);
        if (onTimeline) {
            return onTimeline;
        }

        const thread = shell.value.threads.state.thread;
        if (!thread || thread.channelId !== channelId) {
            return null;
        }

        return thread.root?.id === messageId ? thread.root : thread.replies.find(message => message.id === messageId) ?? null;
    }

    function kindOf(element: Element | null | undefined): ColumnKind {
        return element && threadPane()?.contains(element) ? "thread" : "conversation";
    }

    /**
     * Opens a column's box for a message past its bar, closing any other first. The shell fetches
     * the catalog once the first conversation is drawn; asking here too covers a fetch that failed.
     * The control that opened it toggles it, as a press on a chip's smile-plus does again.
     */
    function openReactionPicker(column: ColumnKind, channelId: string, message: TimelineMessage, anchor: HTMLElement, opener: HTMLElement = anchor, opening: PickerOpening = {}): void {
        const isSameOpener = held.value?.opener === opener;
        closeReactionPicker();
        if (isSameOpener) {
            return;
        }

        opener.setAttribute("aria-expanded", "true");
        held.value = {
            key: ++heldCount,
            column,
            channelId,
            messageId: message.id,
            layer: opening.startsOn === "full" ? "picker" : "tray",
            anchor,
            opener,
            offered: opening.offered ?? [],
            isKeyboard: isKeyboardFocus(document.activeElement)
        };
        releaseHeld = holdMessage(message.id, anchor);
        void shell.value.reactions.loadCatalog();
    }

    function showAllEmoji(): void {
        if (held.value?.layer === "tray") {
            held.value = { ...held.value, layer: "picker" };
        }
    }

    function closeReactionPicker(): void {
        const box = held.value;
        if (!box) {
            return;
        }

        box.opener.setAttribute("aria-expanded", "false");
        releaseHeld?.();
        releaseHeld = null;
        held.value = null;
    }

    function holdMessage(messageId: number, anchor: HTMLElement): () => void {
        anchorsByMessage.set(messageId, anchor);
        return () => {
            if (anchorsByMessage.get(messageId) === anchor) {
                anchorsByMessage.delete(messageId);
            }
        };
    }

    /** A choice in the tray or the picker toggles the reader's reaction and is kept as recently used. */
    function onPickerReact(code: string): void {
        const box = held.value;
        if (!box) {
            return;
        }

        shell.value.reactions.recordRecent(code);
        void shell.value.reactions.toggleCode(box.channelId, box.messageId, code);
    }

    const bars: ActionBars = { held, reactionHost, emojiSource, pickerMine, kindOf, openReactionPicker, showAllEmoji, closeReactionPicker, holdMessage, onPickerReact };
    provide(actionBarsKey, bars);
    return bars;
}

/**
 * The block's boxes, for a column or a box drawn inside the block.
 *
 * @returns The boxes, or null for a part drawn alone.
 */
export function useActionBars(): ActionBars | null {
    return inject(actionBarsKey, null);
}

/**
 * Creates one column's box and hands it to the column's rows.
 *
 * @param timingElement The column's scroller: the rest and grace tokens are read on it once it is drawn, and the box is kept inside it.
 *
 * @returns The box and its handlers.
 */
export function useActionBar(timingElement: () => Element | null | undefined): ColumnActionBar {
    const docks = shallowReactive(new Map<number, ActionBarDock>());
    const bars = useActionBars();

    // The timing the box shares with the members list's Message button; the message the box is on
    // is kept after it hides, so it fades where it was.
    const { shownKey: barId, isShown, isInstant, follow, show, hide, stop } = useHoverIntent<number>(timingElement);

    const dock = computed((): ActionBarDock | null => barId.value === null ? null : docks.get(barId.value) ?? null);

    // The message the box shows on, as a set a row asks of its own message only: a computed of the
    // one id would wake every row that reads it each time it changed.
    const shownOn = shallowReactive(new Set<number>());
    watch(() => isShown.value ? barId.value : null, (now, was) => {
        if (was !== null && was !== undefined) {
            shownOn.delete(was);
        }

        if (now !== null) {
            shownOn.add(now);
        }
    }, { immediate: true, flush: "sync" });

    function isOn(messageId: number): boolean {
        return shownOn.has(messageId);
    }

    const box = ref<HTMLElement>();

    /** The message the pointer is over, or the one a keyboard reached. */
    let hotId: number | null = null;

    /** The hot message came from a keyboard's focus, which the pointer leaving does not let go of. */
    let isHotFromFocus = false;

    /** A menu or the tray open from the box, as the box reports, keeps it on its message. */
    let isOpenHeld = false;

    /** A keyboard's focus inside the box keeps it on its message too. */
    let isFocusHeld = false;

    /** Set by a scroll and cleared by the next move, so the rows passing under a still pointer stay cold. */
    let isScrollMuted = false;

    function isHeld(): boolean {
        return isOpenHeld || isFocusHeld;
    }

    /** Whether a node is inside the box or its dock, which holds the box's focus guard. */
    function isInBar(node: EventTarget | null): boolean {
        const element = dock.value?.element;
        return node instanceof Node && (!!element?.contains(node) || !!box.value?.contains(node));
    }

    /** The pointer's choice of message, which raises nothing while another floating layer is open. */
    function raise(id: number | null): void {
        follow(id !== null && isFloatingLayerOpen() ? null : id);
    }

    /** The pointer reached a row, a gap between rows, or the box. */
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
        if (!isHeld()) {
            raise(id);
        }
    }

    function onPointerLeave(): void {
        if (isHotFromFocus || hotId === null) {
            return;
        }

        hotId = null;
        if (!isHeld()) {
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
        if (!isHeld()) {
            raise(hotId);
        }
    }

    /** A scroll hides the box at once, since its message is sliding away under it. */
    function onScroll(): void {
        isScrollMuted = true;
        hotId = null;
        isHotFromFocus = false;
        if (!isHeld()) {
            hide(true);
        }
    }

    /**
     * A keyboard reaching a message's links or buttons shows its box at once, so Tab can go on into
     * it. Only a keyboard's focus inside the box holds it: a click leaves focus on the tool it
     * pressed, and a box kept for that would stay after the pointer has gone.
     */
    function onFocusIn(event: FocusEvent): void {
        const isKeyboard = isKeyboardFocus(event.target);
        if (isInBar(event.target) && isFocusHeld !== isKeyboard) {
            isFocusHeld = isKeyboard;
            if (!isHeld()) {
                letGo();
            }
        }

        const id = hotIdAt(event.target);
        if (id === null || !isKeyboard || (isHeld() && id !== barId.value)) {
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
        const isLeavingBar = isFocusHeld && isInBar(event.target) && !isInBar(event.relatedTarget);
        if (isLeavingBar) {
            isFocusHeld = false;
        }

        if (!isHotFromFocus || hotIdAt(event.relatedTarget) === hotId) {
            if (isLeavingBar && !isHeld()) {
                letGo();
            }

            return;
        }

        hotId = null;
        isHotFromFocus = false;
        if (!isHeld()) {
            hide(true);
        }
    }

    function onHoldChange(isHolding: boolean): void {
        isOpenHeld = isHolding;
        if (!isHeld()) {
            letGo();
        }
    }

    /** Let go, the box goes where the pointer is now, or starts its grace if it is on none. */
    function letGo(): void {
        if (hotId !== barId.value) {
            raise(hotId);
        }
    }

    function offer(messageId: number, offered: ActionBarDock): void {
        docks.set(messageId, offered);
    }

    /** Which column this is, read while the column is drawn and kept for when it is going. */
    let kind: ColumnKind | null = null;

    /** Whether the block's held box is this column's, on a message, or on any message when none is named. */
    function isHeldHere(messageId: number | null): boolean {
        const element = timingElement();
        if (bars && element?.isConnected) {
            kind = bars.kindOf(element);
        }

        const heldBox = bars?.held.value;
        return !!heldBox && heldBox.column === kind && (messageId === null || heldBox.messageId === messageId);
    }

    /** A message that no longer offers a box takes it away at once, held or grown; focus is the frame's to place. */
    function withdraw(messageId: number, offered: ActionBarDock): void {
        if (docks.get(messageId) !== offered) {
            return;
        }

        docks.delete(messageId);
        if (hotId === messageId) {
            hotId = null;
        }

        if (isHeldHere(messageId)) {
            bars?.closeReactionPicker();
        }

        if (barId.value === messageId) {
            isOpenHeld = false;
            isFocusHeld = false;
            hide(true);
        }
    }

    // A box grown on another of this column's messages, as by a chip's add reaction, brings the
    // box's place in the row to that message, which the box is placed against. Which column holds
    // it is read as it changes, never as the column is set up, before its scroller exists.
    watch(() => bars?.held.value ?? null, box => {
        if (box && box.messageId !== barId.value && isHeldHere(box.messageId)) {
            show(box.messageId);
        }
    });

    // A column going takes its grown box with it.
    onScopeDispose(() => {
        if (isHeldHere(null)) {
            bars?.closeReactionPicker();
        }
    });

    const bar: ColumnActionBar = { dock, isShown, isOn, isInstant, box, offer, withdraw, onPointerOver, onPointerLeave, onPointerMove, onScroll, onFocusIn, onFocusOut, onHoldChange, stop };
    provide(actionBarKey, bar);
    return bar;
}

/**
 * The box of the column a row is drawn in.
 *
 * @returns The column's box, or null for a row drawn outside any column.
 */
export function useColumnActionBar(): ColumnActionBar | null {
    return inject(actionBarKey, null);
}
