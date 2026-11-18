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
// A message's action bar, mounted: three quick reactions and Add reaction, then Reply in thread,
// Quote and More. A column keeps one and says when it shows and on which message (the feed's
// tests hold the timing); a menu, the picker or a keyboard's focus inside holds it, and it tells
// the column so. It is one stop for the keyboard, on the first quick reaction, the arrows
// moving between its tools. The quick reactions are the church's first three favorites, pressed
// when the reader reacted with one; in a narrow column only the first stays. More
// opens the message's menu: Edit (the person's own message, never a block post), Copy, the actions
// the server listed, then Delete (their own). Whether a message can be acted on at all is its row's
// call: the row lends the column's bar a dock only while it can, so the bar draws for the message
// it is given and nothing while it is given none.
//
// The component these tests are written to, components/actionBar.partial.obs:
//
//   props: {
//       message: TimelineMessage | null,   the stored message, carrying actions: [{ id, label, call }]; null on none
//       isOwn: boolean,
//       isInThread: boolean,               inside the thread drawer, where Reply in thread has no place
//       isShown: boolean                   the column shows its bar on this message
//       isInstant: boolean                 the column showed or hid it without a fade
//       favorites: [{ code, label, emoji, image_url }]   the bootstrap's favorites; the first three are quick
//       pickerAnchor: HTMLElement | null   what the open picker is placed beside; the bar holding it stays
//   }
//   emits: replyInThread(message), quote(message), edit(message), copy(message),
//          delete(message), serverAction(action), react(message, code), openPicker(message, anchor),
//          holdChange(isHeld)
//
// The bar is the element marked data-testid="action-bar", with role="toolbar"; it is in the page
// only while it shows. A quick reaction is read "React with thumbs up" or "Remove your thumbs up";
// Add reaction is read "More reactions" and opens the picker; the separator between the reactions
// and the tools is data-testid="action-bar-separator". The column is the nearest element marked
// data-chat-scroller, narrow at 560px wide or less.
import { mount, VueWrapper } from "@vue/test-utils";
import { readFileSync } from "fs";
import { join } from "path";
import { defineComponent, effect, h, nextTick, reactive, ref, shallowRef } from "vue";
import ActionBar from "../../../../../src/Communication/Chat/ChatShell/components/actionBar.partial.obs";
import Icon from "../../../../../src/Communication/Chat/ChatShell/ui/icon.partial.obs";
import { ActionBars, ColumnActionBar, provideActionBars, useActionBar } from "../../../../../src/Communication/Chat/ChatShell/composables/useActionBar.partial";
import { isActionCall } from "../../../../../src/Communication/Chat/ChatShell/composables/useMessageActions.partial";
import { ChatShell } from "../../../../../src/Communication/Chat/ChatShell/shell.partial";
import { TimelineMessage } from "../../../../../src/Communication/Chat/ChatShell/types.partial";
import { PopoverControl, usePopover } from "../../../../../src/Communication/Chat/ChatShell/ui/popover.partial";

const me = "a0000001-0000-4000-8000-000000000000";
const anna = "a0000002-0000-4000-8000-000000000000";

/** 9:02 AM on the day the tests are about, in local time. */
const nineOhTwo = new Date(2026, 9, 15, 9, 2).toISOString();

const approve = { id: "approve", label: "Approve", call: "chat_action_approve" };

/** The church's favorites as the bootstrap carries them, in order; the bar draws the first three. */
const favorites = [
    { code: "thumbsup", label: "thumbs up", emoji: "👍", image_url: null },
    { code: "heart", label: "red heart", emoji: "❤️", image_url: null },
    { code: "joy", label: "face with tears of joy", emoji: "😂", image_url: null },
    { code: "open_mouth", label: "face with open mouth", emoji: "😮", image_url: null },
    { code: "cry", label: "crying face", emoji: "😢", image_url: null },
    { code: "pray", label: "folded hands", emoji: "🙏", image_url: null },
    { code: "tada", label: "party popper", emoji: "🎉", image_url: null }
];

/** The bar's quick reactions and its Add reaction, as the toolbar reads them. */
const reactionLabels = ["React with thumbs up", "React with red heart", "React with face with tears of joy", "More reactions"];

/** A stored message as a page carries it, Anna's unless told otherwise. */
function message(extra: Record<string, unknown> = {}): TimelineMessage {
    return {
        id: 42,
        parent_id: null,
        person_alias_guid: anna,
        sender_nick_name: "Anna",
        sender_last_name: "Reyes",
        sender_avatar_url: null,
        sender_listed: true,
        message_type: "text",
        body: "Morning all",
        pieces: null,
        blocks: null,
        created_at: nineOhTwo,
        edited_at: null,
        deleted_at: null,
        visibility: "visible",
        visibility_notice: null,
        is_own: false,
        actions: [],
        ...extra
    } as unknown as TimelineMessage;
}

function own(extra: Record<string, unknown> = {}): TimelineMessage {
    return message({ person_alias_guid: me, sender_nick_name: "Jordan", sender_last_name: "Ellis", is_own: true, ...extra });
}

type BarProps = {
    message?: TimelineMessage | null;
    isOwn?: boolean;
    isInThread?: boolean;
    isShown?: boolean;
    favorites?: unknown[];
    pickerAnchor?: HTMLElement | null;
};

let wrapper: VueWrapper | null = null;

async function settle(): Promise<void> {
    await nextTick();
    await nextTick();
}

/** Mounts the bar as its column shows it, so it is drawn if it may be. */
async function shown(props: BarProps): Promise<VueWrapper> {
    wrapper = mount(ActionBar, { props: { message: null, isShown: true, favorites, ...props }, attachTo: document.body });
    await settle();
    return wrapper;
}

function bar(w: VueWrapper): ReturnType<VueWrapper["find"]> {
    return w.find("[data-testid='action-bar']");
}

function toolLabels(w: VueWrapper): string[] {
    return w.get("[data-testid='action-bar']").findAll("button").map(button => button.attributes("aria-label") ?? "");
}

/** Opens More and reads the menu in order: each item's label without its key hint, "---" for a separator. */
async function menuOf(w: VueWrapper): Promise<string[]> {
    const more = w.get("[data-testid='action-bar'] button[aria-label='More actions']").element as HTMLElement;
    more.click();
    await settle();

    return Array.from(document.querySelectorAll<HTMLElement>("[role='menu'] [role='menuitem'], [role='menu'] [role='separator']")).map(item => {
        if (item.getAttribute("role") === "separator") {
            return "---";
        }
        const copy = item.cloneNode(true) as HTMLElement;
        copy.querySelectorAll("kbd").forEach(kbd => kbd.remove());
        return (copy.textContent ?? "").trim();
    });
}

function menuItem(label: string): HTMLElement {
    const item = Array.from(document.querySelectorAll<HTMLElement>("[role='menu'] [role='menuitem']")).find(i => (i.textContent ?? "").includes(label));
    if (!item) {
        throw new Error(`No menu item ${label}`);
    }
    return item;
}

beforeAll(() => {
    // jsdom has no ResizeObserver, and floating placement keeps its menu beside the anchor with one.
    if (typeof window.ResizeObserver === "undefined") {
        window.ResizeObserver = class {
            observe(): void { /* jsdom lays nothing out, so there is nothing to watch. */ }
            unobserve(): void { /* As above. */ }
            disconnect(): void { /* As above. */ }
        } as unknown as typeof ResizeObserver;
    }
});

beforeEach(() => {
    jest.useFakeTimers();
});

afterEach(() => {
    wrapper?.unmount();
    wrapper = null;
    document.body.innerHTML = "";
    jest.useRealTimers();
});

describe("the bar's tools", () => {
    test("are the quick reactions and Add reaction, then Reply in thread, Quote and More, in a toolbar named for the message", async () => {
        const w = await shown({ message: message() });

        expect(bar(w).attributes("role")).toBe("toolbar");
        expect(bar(w).attributes("aria-label")?.replace(/\s+/g, " ")).toBe("Actions for Anna Reyes's message, 9:02 AM");
        expect(toolLabels(w)).toEqual([...reactionLabels, "Reply in thread", "Quote", "More actions"]);
    });

    test("More is read as More actions, and its tooltip says More", async () => {
        const w = await shown({ message: message() });
        const more = w.get("[data-testid='action-bar'] button[aria-label='More actions']");

        await more.trigger("mouseenter");
        jest.advanceTimersByTime(500);
        await settle();

        expect(Array.from(document.querySelectorAll("[role='tooltip']")).map(tip => (tip.textContent ?? "").trim())).toEqual(["More"]);
    });

    test("leave out Reply in thread inside a thread", async () => {
        const w = await shown({ message: message({ parent_id: 40 }), isInThread: true });

        expect(toolLabels(w)).toEqual([...reactionLabels, "Quote", "More actions"]);
    });

    test("ask to reply in thread and to quote, each with the message", async () => {
        const target = message();
        const w = await shown({ message: target });

        await w.get("[data-testid='action-bar'] button[aria-label='Reply in thread']").trigger("click");
        await w.get("[data-testid='action-bar'] button[aria-label='Quote']").trigger("click");

        expect(w.emitted("replyInThread")).toEqual([[target]]);
        expect(w.emitted("quote")).toEqual([[target]]);
    });
});

describe("the menu", () => {
    test("on the person's own message: Edit, Copy, the server's actions, then Delete, each group apart", async () => {
        const w = await shown({ message: own({ actions: [approve] }), isOwn: true });

        expect(await menuOf(w)).toEqual(["Edit message", "Copy text", "---", "Approve", "---", "Delete message"]);
    });

    test("on someone else's message: Copy, the server's actions, then Block apart, never Mark unread", async () => {
        const w = await shown({ message: message({ actions: [approve] }) });

        const items = await menuOf(w);

        expect(items).toEqual(["Copy text", "---", "Approve", "---", "Block"]);
        expect(items).not.toContain("Mark unread from here");
    });

    test("with no server actions, no empty group is drawn", async () => {
        const w = await shown({ message: own(), isOwn: true });

        expect(await menuOf(w)).toEqual(["Edit message", "Copy text", "---", "Delete message"]);
    });

    test("leaves Edit off the person's own block post", async () => {
        const card = own({ message_type: "system", blocks: [{ type: "text", text: "Signups open", fallback: { type: "text", text: "Signups open" } }] });
        const w = await shown({ message: card, isOwn: true });

        expect(await menuOf(w)).toEqual(["Copy text", "---", "Delete message"]);
    });

    test("Edit carries its key hint, Up", async () => {
        const w = await shown({ message: own(), isOwn: true });
        await menuOf(w);

        expect(menuItem("Edit message").querySelector("kbd")?.textContent?.trim()).toBe("Up");
    });

    test("an action this client was not built with is drawn from its label, with the bolt", async () => {
        const closePoll = { id: "close_poll", label: "Close the poll", call: "chat_action_close_poll" };
        const w = await shown({ message: message({ actions: [closePoll] }) });
        await menuOf(w);

        const item = menuItem("Close the poll");
        const icons = w.findAllComponents(Icon).filter(icon => item.contains(icon.element));
        expect(icons.map(icon => icon.props("name"))).toEqual(["bolt"]);
    });

    test("Delete is drawn as dangerous", async () => {
        const w = await shown({ message: own(), isOwn: true });
        await menuOf(w);

        expect(menuItem("Delete message").classList.contains("is-danger")).toBe(true);
    });

    test.each([
        ["Edit message", "edit"],
        ["Copy text", "copy"],
        ["Delete message", "delete"]
    ])("%s asks for its action with the message", async (label, event) => {
        const target = own();
        const w = await shown({ message: target, isOwn: true });
        await menuOf(w);

        menuItem(label).click();
        await settle();

        expect(w.emitted(event)).toEqual([[target]]);
    });

    test("a server action asks for itself", async () => {
        const w = await shown({ message: message({ actions: [approve] }) });
        await menuOf(w);

        menuItem("Approve").click();
        await settle();

        expect(w.emitted("serverAction")).toEqual([[approve]]);
    });
});

// The row decides whether its message can be acted on, and lends the bar a dock only then; the
// cases it refuses are the feed's tests of the bar.
describe("the bar's message", () => {
    test("is drawn for the message it is given, and nothing while it is given none", async () => {
        const w = await shown({ message: message() });
        expect(bar(w).exists()).toBe(true);

        await w.setProps({ message: null });
        await settle();

        expect(bar(w).exists()).toBe(false);
    });
});

// The toolbar pattern: one tab stop, the arrows moving it between the tools.
describe("the bar's keyboard", () => {
    function tools(w: VueWrapper): HTMLElement[] {
        return w.get("[data-testid='action-bar']").findAll("button").map(b => b.element as HTMLElement);
    }

    test("is one tab stop, the first quick reaction", async () => {
        const w = await shown({ message: message() });

        expect(tools(w)[0].getAttribute("aria-label")).toBe("React with thumbs up");
        expect(tools(w).map(t => t.getAttribute("tabindex"))).toEqual(["0", "-1", "-1", "-1", "-1", "-1", "-1"]);
    });

    // Tab past the bar's end hides the box once focus leaves its message, but the box's place in
    // the row keeps its stop, so Shift+Tab back from what follows the message finds the bar again:
    // reaching the place shows the box there, and focus goes on into it once it is drawn.
    test("its place in the row stays a stop while the box is hidden on its message, and focus there goes into the box once it shows", async () => {
        wrapper = mount(ActionBar, { props: { message: message(), isShown: false, favorites }, attachTo: document.body });
        await settle();
        const guard = wrapper.get("[data-testid='action-bar-guard']").element as HTMLElement;
        expect(bar(wrapper).exists()).toBe(false);
        expect(guard.getAttribute("tabindex")).toBe("0");

        guard.focus();
        await wrapper.setProps({ isShown: true });
        await settle();

        expect(document.activeElement?.getAttribute("aria-label")).toBe("React with thumbs up");
    });

    test("its place in the row is no stop while the box is on no message", async () => {
        wrapper = mount(ActionBar, { props: { message: null, isShown: false, favorites }, attachTo: document.body });
        await settle();

        expect(wrapper.get("[data-testid='action-bar-guard']").attributes("tabindex")).toBe("-1");
    });

    test("Right moves to the next tool and takes the tab stop with it; Left moves back", async () => {
        const w = await shown({ message: message() });
        const [thumbsUp, heart] = tools(w);
        thumbsUp.focus();

        thumbsUp.dispatchEvent(new KeyboardEvent("keydown", { key: "ArrowRight", bubbles: true }));
        await settle();

        expect(document.activeElement).toBe(heart);
        expect(tools(w).map(t => t.getAttribute("tabindex"))).toEqual(["-1", "0", "-1", "-1", "-1", "-1", "-1"]);

        heart.dispatchEvent(new KeyboardEvent("keydown", { key: "ArrowLeft", bubbles: true }));
        await settle();

        expect(document.activeElement).toBe(thumbsUp);
    });

    test("Right moves from Add reaction across the separator to the tools", async () => {
        const w = await shown({ message: message() });
        const all = tools(w);
        all[3].focus();

        all[3].dispatchEvent(new KeyboardEvent("keydown", { key: "ArrowRight", bubbles: true }));
        await settle();

        expect(document.activeElement).toBe(all[4]);
        expect(all[4].getAttribute("aria-label")).toBe("Reply in thread");
    });

    test("Right on the last tool wraps to the first", async () => {
        const w = await shown({ message: message() });
        const all = tools(w);
        all[all.length - 1].focus();

        all[all.length - 1].dispatchEvent(new KeyboardEvent("keydown", { key: "ArrowRight", bubbles: true }));
        await settle();

        expect(document.activeElement).toBe(all[0]);
    });
});

// The column decides when its bar shows; the bar draws at once as told, and says when it is held.
describe("showing and holding", () => {
    test("is drawn the moment its column shows it, and gone the moment it hides it, with no wait of its own", async () => {
        wrapper = mount(ActionBar, { props: { message: message(), isShown: false }, attachTo: document.body });
        await settle();
        expect(bar(wrapper).exists()).toBe(false);

        await wrapper.setProps({ isShown: true });
        await settle();
        expect(bar(wrapper).exists()).toBe(true);

        await wrapper.setProps({ isShown: false });
        await settle();
        expect(bar(wrapper).exists()).toBe(false);
    });

    test("tells its column while its menu holds it, and when it lets go", async () => {
        // A mouse's focus never matches :focus-visible in a browser, so it holds no bar; jsdom
        // cannot tell, and is told.
        const matches = Element.prototype.matches;
        const mouseFocus = jest.spyOn(Element.prototype, "matches").mockImplementation(function (this: Element, selector: string) {
            return selector === ":focus-visible" ? false : matches.call(this, selector);
        });
        const w = await shown({ message: message() });

        (w.get("[data-testid='action-bar'] button[aria-label='More actions']").element as HTMLElement).click();
        await settle();
        expect(w.emitted("holdChange")).toEqual([[true]]);

        await w.setProps({ isShown: false });
        await settle();
        expect(bar(w).exists()).toBe(true);

        document.body.dispatchEvent(new Event("pointerdown", { bubbles: true }));
        await settle();
        expect(w.emitted("holdChange")).toEqual([[true], [false]]);
        expect(bar(w).exists()).toBe(false);
        mouseFocus.mockRestore();
    });
});

// A listed action may only call a chat action function. One naming anything else is still drawn,
// so the menu matches what the server listed, but it cannot be chosen, as a block post's button
// with such a call cannot be pressed.
describe("a server action whose call is not a chat action", () => {
    const stray = { id: "stray", label: "Wipe the room", call: "chat_delete_everything" };

    test("is drawn disabled and asks for nothing when chosen", async () => {
        expect(isActionCall(stray.call)).toBe(false);
        const w = await shown({ message: message({ actions: [approve, stray] }) });
        await menuOf(w);

        expect(menuItem("Wipe the room").getAttribute("aria-disabled")).toBe("true");
        expect(menuItem("Approve").getAttribute("aria-disabled")).toBeNull();

        menuItem("Wipe the room").click();
        await settle();

        expect(w.emitted("serverAction")).toBeUndefined();
    });

    test("Enter on it asks for nothing and leaves the menu open, as a click on it does", async () => {
        const w = await shown({ message: message({ actions: [approve, stray] }) });
        await menuOf(w);

        const item = menuItem("Wipe the room");
        item.focus();
        item.dispatchEvent(new KeyboardEvent("keydown", { key: "Enter", bubbles: true, cancelable: true }));
        await settle();

        expect(w.emitted("serverAction")).toBeUndefined();
        expect(document.querySelector("[role='menu']")).not.toBeNull();
        expect(document.activeElement).toBe(item);
    });
});

// The church's first three favorites, each a toggle of the reader's reaction to the message.
describe("the bar's quick reactions", () => {
    const yourHeart = { code: "heart", count: 3, mine: true, emoji: "❤️", image_url: null, label: "red heart" };
    const othersJoy = { code: "joy", count: 1, mine: false, emoji: "😂", image_url: null, label: "face with tears of joy" };

    function quick(w: VueWrapper): HTMLElement[] {
        return w.get("[data-testid='action-bar']").findAll("button")
            .map(b => b.element as HTMLElement)
            .filter(b => /^(React with|Remove your) /.test(b.getAttribute("aria-label") ?? ""));
    }

    test("are the first three favorites, the reader's own pressed and read as a removal", async () => {
        const w = await shown({ message: message({ reactions: [othersJoy, yourHeart] }) });

        expect(quick(w).map(b => b.textContent?.trim())).toEqual(["👍", "❤️", "😂"]);
        expect(quick(w).map(b => b.getAttribute("aria-label"))).toEqual(["React with thumbs up", "Remove your red heart", "React with face with tears of joy"]);
        expect(quick(w).map(b => b.getAttribute("aria-pressed"))).toEqual(["false", "true", "false"]);
    });

    test("a press asks to react to the message with that emoji", async () => {
        const target = message({ reactions: [yourHeart] });
        const w = await shown({ message: target });

        quick(w)[0].click();
        quick(w)[1].click();
        await settle();

        expect(w.emitted("react")).toEqual([[target, "thumbsup"], [target, "heart"]]);
    });

    // The box is placed against its place in the message's row, a guard of no size, which the
    // grown box stays beside, so the bar can turn into it in place; so Add reaction asks from there.
    test("Add reaction opens the picker from the box's own place, its tooltip saying Add reaction", async () => {
        const target = message();
        const w = await shown({ message: target });
        const add = w.get("[data-testid='action-bar'] button[aria-label='More reactions']");

        expect(add.attributes("aria-haspopup")).toBe("dialog");
        await add.trigger("mouseenter");
        jest.advanceTimersByTime(500);
        await settle();
        expect(Array.from(document.querySelectorAll("[role='tooltip']")).map(tip => (tip.textContent ?? "").trim())).toEqual(["Add reaction"]);

        await add.trigger("click");
        const guard = document.querySelector("[data-testid='action-bar-guard']");
        expect(guard).not.toBeNull();
        expect(w.emitted("openPicker")).toEqual([[target, guard, { offered: favorites.slice(0, 3).map(favorite => favorite.code) }]]);
    });

    /**
     * Mounts the bar inside a block's boxes, as its column shows it, and hands back both and the
     * message's chip row. Given a row, it draws the row's smile-plus beside the bar; a row "taken"
     * by a choice is drawn no more once a reaction is toggled, as a choice removing the last one.
     */
    async function shownInBlock(chipRow?: "kept" | "taken"): Promise<{ w: VueWrapper, bars: ActionBars, row: { isDrawn: boolean } }> {
        let bars: ActionBars | null = null;
        const row = reactive({ isDrawn: !!chipRow });
        const toggleCode = jest.fn(async () => {
            row.isDrawn = chipRow === "kept";
        });
        const shell = shallowRef({
            reactions: { favorites, state: reactive({ catalog: null, recents: [] }), loadCatalog: jest.fn(async () => null), recordRecent: jest.fn(), toggleCode },
            timelines: { state: () => ({ messages: [] }) },
            threads: { state: { thread: null } }
        } as unknown as ChatShell);
        const Host = defineComponent({
            props: { isShown: { type: Boolean, default: true } },
            setup(props) {
                bars = provideActionBars(shell, "Cedar Hill", () => undefined);
                return () => [
                    h(ActionBar, { message: message(), isShown: props.isShown, favorites }),
                    row.isDrawn ? h("button", { "data-testid": "chip-add" }, "Add reaction") : null
                ];
            }
        });
        wrapper = mount(Host, { attachTo: document.body });
        await settle();
        return { w: wrapper, bars: bars as unknown as ActionBars, row };
    }

    function box(): HTMLElement | null {
        return document.querySelector<HTMLElement>("[data-testid='message-box']");
    }

    test("the box stays while the tray its Add reaction opened is up, and goes once it closes", async () => {
        const { w, bars } = await shownInBlock();
        const guard = document.querySelector<HTMLElement>("[data-testid='action-bar-guard']") as HTMLElement;
        const add = document.querySelector<HTMLElement>("[data-testid='action-bar'] button[aria-label='More reactions']") as HTMLElement;

        bars.openReactionPicker("conversation", "c1", message(), guard, add, { offered: ["thumbsup", "heart", "joy"] });
        await w.setProps({ isShown: false });
        jest.advanceTimersByTime(500);
        await settle();
        expect(box()?.getAttribute("data-layer")).toBe("tray");

        bars.closeReactionPicker();
        await settle();
        expect(box()).toBeNull();
    });

    test("a box grown in the other column does not hold this one", async () => {
        const { w, bars } = await shownInBlock();
        const elsewhere = document.createElement("button");

        bars.openReactionPicker("thread", "c1", message(), elsewhere, elsewhere);
        await w.setProps({ isShown: false });
        jest.advanceTimersByTime(500);
        await settle();

        expect(box()).toBeNull();
    });

    // A chip's smile-plus opens the box from outside it. Pressed again it closes the box: the press
    // on it counts as inside the box, not as a press elsewhere that would close the box first and
    // leave the click to open a fresh one.
    test("the control outside the box that opened it closes it when pressed again", async () => {
        const { bars } = await shownInBlock();
        const guard = document.querySelector<HTMLElement>("[data-testid='action-bar-guard']") as HTMLElement;
        const chipAdd = document.createElement("button");
        document.body.append(chipAdd);

        bars.openReactionPicker("conversation", "c1", message(), guard, chipAdd);
        await settle();
        expect(box()?.getAttribute("data-layer")).toBe("tray");

        chipAdd.dispatchEvent(new Event("pointerdown", { bubbles: true }));
        await settle();
        bars.openReactionPicker("conversation", "c1", message(), guard, chipAdd);
        await settle();

        expect(bars.held.value).toBeNull();
        expect(box()?.getAttribute("data-layer")).toBe("bar");
    });

    /** Opens the box from the chip smile-plus outside it, focused as a press leaves it, and hands it back. */
    async function openedFromChip(bars: ActionBars): Promise<HTMLElement> {
        const guard = document.querySelector<HTMLElement>("[data-testid='action-bar-guard']") as HTMLElement;
        const chipAdd = document.querySelector<HTMLElement>("[data-testid='chip-add']") as HTMLElement;
        chipAdd.focus();

        bars.openReactionPicker("conversation", "c1", message(), guard, chipAdd);
        await settle();
        await settle();
        return chipAdd;
    }

    function trayGlyph(code: string): HTMLElement {
        return document.querySelector<HTMLElement>(`[data-testid='message-box'] [role='toolbar'][aria-label='Quick reactions'] [data-code='${code}']`) as HTMLElement;
    }

    // The picker drawn in the box only asks to react; the box is what closes, and gives focus back
    // to the control that opened it, as the picker's own popover once did.
    test("a choice in the tray reacts, gives the bar back, and gives focus to the control outside the box that opened it", async () => {
        const { bars } = await shownInBlock("kept");
        const chipAdd = await openedFromChip(bars);
        expect(box()?.getAttribute("data-layer")).toBe("tray");

        trayGlyph("pray").click();
        await settle();
        await settle();

        expect(bars.held.value).toBeNull();
        expect(box()?.getAttribute("data-layer")).toBe("bar");
        expect(document.activeElement).toBe(chipAdd);
    });

    test("Escape gives the bar back and focus to the control outside the box that opened it", async () => {
        const { bars } = await shownInBlock("kept");
        const chipAdd = await openedFromChip(bars);
        expect(document.activeElement).toBe(trayGlyph("thumbsup"));

        document.activeElement?.dispatchEvent(new KeyboardEvent("keydown", { key: "Escape", bubbles: true }));
        await settle();
        await settle();

        expect(bars.held.value).toBeNull();
        expect(document.activeElement).toBe(chipAdd);
    });

    // A choice can take the chip row away as the page redraws, the smile-plus with it. The bar is
    // drawn again on the message, so its Add reaction is where focus goes, never the page's body.
    test("a choice that takes the control that opened it away gives focus to the bar's Add reaction", async () => {
        const { bars } = await shownInBlock("taken");
        await openedFromChip(bars);

        trayGlyph("pray").click();
        await settle();
        await settle();

        expect(document.querySelector("[data-testid='chip-add']")).toBeNull();
        expect(bars.held.value).toBeNull();
        expect(document.activeElement).toBe(document.querySelector("[data-testid='action-bar'] button[aria-label='More reactions']"));
    });

    test("stays when the control outside the box that opened it goes, and Escape then gives focus to the bar's Add reaction", async () => {
        const { bars, row } = await shownInBlock("kept");
        await openedFromChip(bars);

        row.isDrawn = false;
        await settle();
        expect(box()?.getAttribute("data-layer")).toBe("tray");

        trayGlyph("thumbsup").dispatchEvent(new KeyboardEvent("keydown", { key: "Escape", bubbles: true }));
        await settle();
        await settle();

        expect(bars.held.value).toBeNull();
        expect(document.activeElement).toBe(document.querySelector("[data-testid='action-bar'] button[aria-label='More reactions']"));
    });

    // The box grows from the bar's edges to the tray's over the block's one move, the
    // approved grow (220ms, easing in); asked for less motion, the token sheet takes that move to
    // nothing and the tray is simply there.
    describe("growing", () => {
        const animate = jest.fn();
        let sheet: HTMLStyleElement;

        beforeEach(() => {
            animate.mockClear();
            (HTMLElement.prototype as unknown as { animate: unknown }).animate = animate;
            sheet = document.createElement("style");
            document.head.append(sheet);
        });

        afterEach(() => {
            delete (HTMLElement.prototype as unknown as { animate?: unknown }).animate;
            sheet.remove();
        });

        /** Opens the tray from a bar whose box reads the durations given. */
        async function grownWith(durations: string): Promise<void> {
            sheet.textContent = `.chat-box { --chat-t-base: 180ms; ${durations} }`;
            const { bars } = await shownInBlock();
            const guard = document.querySelector<HTMLElement>("[data-testid='action-bar-guard']") as HTMLElement;
            bars.openReactionPicker("conversation", "c1", message(), guard, guard, { offered: ["thumbsup", "heart", "joy"] });
            // The grow is revealed once the grown box is placed, a chain of promises settled a turn at a time.
            for (let i = 0; i < 40; i++) {
                await settle();
            }
            expect(box()?.getAttribute("data-layer")).toBe("tray");
        }

        test("over the one move", async () => {
            await grownWith("--chat-d-move: 220ms;");

            expect(animate).toHaveBeenCalledTimes(1);
            expect(animate.mock.calls[0][1]).toMatchObject({ duration: 220 });
        });

        test("at once where the one move is nothing, as under reduced motion", async () => {
            await grownWith("--chat-d-move: 0ms;");

            expect(animate).not.toHaveBeenCalled();
        });
    });

    test("are set apart from the tools by a separator", async () => {
        const w = await shown({ message: message() });

        expect(w.find("[data-testid='action-bar-separator']").exists()).toBe(true);
    });
});

// The thread drawer, or a feed squeezed by a panel: the bar keeps one quick reaction and drops
// the separator, so it fits the narrower column.
describe("the bar in a narrow column", () => {
    async function shownIn(columnWidth: number): Promise<VueWrapper> {
        const scroller = document.createElement("div");
        scroller.setAttribute("data-chat-scroller", "");
        scroller.getBoundingClientRect = () => ({ x: 0, y: 0, top: 0, left: 0, right: columnWidth, bottom: 600, width: columnWidth, height: 600, toJSON: () => ({}) }) as DOMRect;
        const host = document.createElement("div");
        scroller.append(host);
        document.body.append(scroller);

        wrapper = mount(ActionBar, { props: { message: message(), isShown: true, favorites }, attachTo: host });
        await settle();
        await settle();
        return wrapper;
    }

    test("of 560px or less keeps only the first quick reaction and no separator", async () => {
        const w = await shownIn(560);

        expect(toolLabels(w)).toEqual(["React with thumbs up", "More reactions", "Reply in thread", "Quote", "More actions"]);
        expect(w.find("[data-testid='action-bar-separator']").exists()).toBe(false);
    });

    test("wider than 560px keeps all three", async () => {
        const w = await shownIn(561);

        expect(toolLabels(w)).toEqual([...reactionLabels, "Reply in thread", "Quote", "More actions"]);
        expect(w.find("[data-testid='action-bar-separator']").exists()).toBe(true);
    });
});

/** The bar's style rules outside any at-rule, by selector, each as its declarations. */
function barRules(): Map<string, Record<string, string>> {
    const source = readFileSync(join(__dirname, "../../../../../src/Communication/Chat/ChatShell/components/actionBar.partial.obs"), "utf8").replace(/\r\n/g, "\n");
    const style = source.slice(source.indexOf(">", source.indexOf("<style")) + 1, source.indexOf("</style>")).replace(/\/\*[\s\S]*?\*\//g, "");
    const rules = new Map<string, Record<string, string>>();
    for (const rule of style.matchAll(/(?:^|\n)([^\s@{}][^{}]*)\{([^{}]*)\}/g)) {
        const body = Object.fromEntries(rule[2].split(";").map(d => d.trim()).filter(d => d).map(d => [d.slice(0, d.indexOf(":")).trim(), d.slice(d.indexOf(":") + 1).trim()]));
        for (const selector of rule[1].split(",")) {
            rules.set(selector.trim(), { ...rules.get(selector.trim()), ...body });
        }
    }
    return rules;
}

// A direct message's bar sits beside the bubble it acts on, its top on the bubble's top, on the
// side away from the face: right of another person's bubble, left of the reader's own. It takes the
// widest form that fits there: three quick reactions and the separator, then the first quick
// reaction alone, then the tools alone. Only where even that does not fit does it stand over the
// bubble's top, at the bubble's own edge, in the column's form; and under the bubble where the
// column's top leaves no room over it. It is never drawn over its own message's text. The box is
// kept 4px inside its column, as every box drawn in its own layer is kept inside the feed,
// so the room it fits in is measured from 4px inside the column's edges.
describe("the bar beside a bubble", () => {
    /** The bar's width as these tests lay it out: 6px of padding, 32px a quick reaction, 28px a tool, 9px the separator, 2px between. */
    function widthOf(glyphs: number, tools: number, hasSeparator: boolean): number {
        const parts = glyphs + tools + (hasSeparator ? 1 : 0);
        return 6 + glyphs * 32 + tools * 28 + (hasSeparator ? 9 : 0) + 2 * (parts - 1);
    }

    /** The bar's three forms; each keeps the four tools: Add reaction, Reply in thread, Quote and More. */
    const full = widthOf(3, 4, true);
    const one = widthOf(1, 4, false);
    const bare = widthOf(0, 4, false);

    /** How far the bar stands off the bubble beside it, and over or under it, its height, and how far inside its column it is kept. */
    const gap = 6;
    const overGap = 4;
    const height = 34;
    const inset = 4;

    /** The bubbles these tests draw are this tall. */
    const bubbleHeight = 60;

    const offsetWidth = Object.getOwnPropertyDescriptor(HTMLElement.prototype, "offsetWidth");
    const offsetHeight = Object.getOwnPropertyDescriptor(HTMLElement.prototype, "offsetHeight");

    beforeEach(() => {
        // jsdom lays nothing out, so the box is given the width its drawn tools would take.
        Object.defineProperty(HTMLElement.prototype, "offsetWidth", {
            configurable: true,
            get(this: HTMLElement): number {
                const tools = this.getAttribute("data-testid") === "message-box" ? this.querySelector("[data-testid='action-bar']") : null;
                if (!tools) {
                    return 0;
                }
                const glyphs = tools.querySelectorAll(".chat-tool-quick").length;
                return widthOf(glyphs, tools.querySelectorAll("button").length - glyphs, !!tools.querySelector("[data-testid='action-bar-separator']"));
            }
        });
        Object.defineProperty(HTMLElement.prototype, "offsetHeight", {
            configurable: true,
            get(this: HTMLElement): number {
                return this.getAttribute("data-testid") === "message-box" ? height : 0;
            }
        });

        // The window, which a box is kept inside as well as its column.
        Object.defineProperty(document.documentElement, "clientWidth", { configurable: true, value: 1200 });
        Object.defineProperty(document.documentElement, "clientHeight", { configurable: true, value: 800 });
    });

    afterEach(() => {
        Object.defineProperty(HTMLElement.prototype, "offsetWidth", offsetWidth as PropertyDescriptor);
        Object.defineProperty(HTMLElement.prototype, "offsetHeight", offsetHeight as PropertyDescriptor);
        delete (document.documentElement as unknown as Record<string, unknown>).clientWidth;
        delete (document.documentElement as unknown as Record<string, unknown>).clientHeight;
    });

    type Bubble = { left: number, right: number, top: number };

    /** Mounts the bar in a bubble's dock, the bubble and the column at the boxes given. */
    async function shownBeside(bubble: Bubble, isOwn = false, columnWidth = 900): Promise<VueWrapper> {
        const rect = (left: number, right: number, top: number, bottom: number): DOMRect => ({ x: left, y: top, top, left, right, bottom, width: right - left, height: bottom - top, toJSON: () => ({}) }) as DOMRect;
        const scroller = document.createElement("div");
        scroller.setAttribute("data-chat-scroller", "");
        scroller.getBoundingClientRect = () => rect(0, columnWidth, 0, 600);
        // The column's inner box too, which the box is kept inside.
        Object.defineProperty(scroller, "clientWidth", { value: columnWidth });
        Object.defineProperty(scroller, "clientHeight", { value: 600 });
        const host = document.createElement("div");
        scroller.append(host);
        document.body.append(scroller);

        wrapper = mount(ActionBar, { props: { message: isOwn ? own() : message(), isOwn, isShown: true, favorites }, attachTo: host });
        await settle();

        // The row's dock draws no box, so the bar is placed against the box holding its dock, the
        // bubble's; the bar is placed again as it moves to the next message.
        const guard = wrapper.get("[data-testid='action-bar-guard']").element as HTMLElement;
        const anchor = guard.parentElement?.parentElement as HTMLElement;
        anchor.getBoundingClientRect = () => rect(bubble.left, bubble.right, bubble.top, bubble.top + bubbleHeight);
        await wrapper.setProps({ message: isOwn ? own({ id: 43 }) : message({ id: 43 }) });
        // Placing is a chain of promises, settled a turn at a time.
        for (let i = 0; i < 40; i++) {
            await settle();
        }
        return wrapper;
    }

    /** Where the box stands against the bubble, read from where it is drawn. */
    function standing(w: VueWrapper, bubble: Bubble, isOwn = false): "beside" | "over" | "under" | string {
        const box = w.get("[data-testid='message-box']").element as HTMLElement;
        const left = parseFloat(box.style.left);
        const top = parseFloat(box.style.top);
        const width = box.offsetWidth;
        const isBeside = isOwn ? left + width === bubble.left - gap : left === bubble.right + gap;
        const isAtOwnEdge = isOwn ? left === bubble.left : left + width === bubble.right;
        if (isBeside && top === bubble.top) {
            return "beside";
        }
        if (isAtOwnEdge && top + height === bubble.top - overGap) {
            return "over";
        }
        if (isAtOwnEdge && top === bubble.top + bubbleHeight + overGap) {
            return "under";
        }
        return `at ${left}, ${top}`;
    }

    /** Another person's bubble leaving this much room to the column's right edge. */
    function leaving(room: number, top = 200): Bubble {
        return { left: 60, right: 900 - room, top };
    }

    test("is drawn at the bubble's top beside it, away from the face, on either side", async () => {
        const others = leaving(full + gap + inset + 100);
        const beside = await shownBeside(others);
        expect(standing(beside, others)).toBe("beside");
        beside.unmount();
        document.body.innerHTML = "";

        const yours = { left: full + gap + inset + 100, right: 900, top: 200 };
        const ownBeside = await shownBeside(yours, true);
        expect(standing(ownBeside, yours, true)).toBe("beside");
    });

    test("keeps all three quick reactions where they fit beside the bubble, to the pixel", async () => {
        const bubble = leaving(full + gap + inset);
        const w = await shownBeside(bubble);

        expect(toolLabels(w)).toEqual([...reactionLabels, "Reply in thread", "Quote", "More actions"]);
        expect(standing(w, bubble)).toBe("beside");
    });

    test("a pixel short of that keeps the first quick reaction alone, with no separator, still beside the bubble", async () => {
        const bubble = leaving(full + gap + inset - 1);
        const w = await shownBeside(bubble);

        expect(toolLabels(w)).toEqual(["React with thumbs up", "More reactions", "Reply in thread", "Quote", "More actions"]);
        expect(w.find("[data-testid='action-bar-separator']").exists()).toBe(false);
        expect(standing(w, bubble)).toBe("beside");
    });

    test("keeps that one where it just fits, and the tools alone a pixel short of it", async () => {
        const roomy = leaving(one + gap + inset);
        const fits = await shownBeside(roomy);
        expect(toolLabels(fits)[0]).toBe("React with thumbs up");
        expect(standing(fits, roomy)).toBe("beside");
        fits.unmount();
        document.body.innerHTML = "";

        const tight = leaving(one + gap + inset - 1);
        const short = await shownBeside(tight);
        expect(toolLabels(short)).toEqual(["More reactions", "Reply in thread", "Quote", "More actions"]);
        expect(standing(short, tight)).toBe("beside");
    });

    test("the tools alone stay beside the bubble where they just fit", async () => {
        const bubble = leaving(bare + gap + inset);
        const w = await shownBeside(bubble);

        expect(toolLabels(w)).toEqual(["More reactions", "Reply in thread", "Quote", "More actions"]);
        expect(standing(w, bubble)).toBe("beside");
    });

    test("where even the tools alone do not fit, it stands over the bubble's top with all three quick reactions", async () => {
        const bubble = leaving(bare + gap + inset - 1);
        const w = await shownBeside(bubble);

        expect(standing(w, bubble)).toBe("over");
        expect(toolLabels(w)).toEqual([...reactionLabels, "Reply in thread", "Quote", "More actions"]);
    });

    test("over a bubble too near the column's top for it, it stands under the bubble instead, never over its text", async () => {
        const roomy = leaving(inset, height + overGap + inset);
        const room = await shownBeside(roomy);
        expect(standing(room, roomy)).toBe("over");
        room.unmount();
        document.body.innerHTML = "";

        const tight = leaving(inset, height + overGap + inset - 1);
        const near = await shownBeside(tight);
        expect(standing(near, tight)).toBe("under");
    });

    test("does the same on the reader's own side, by the room left of the bubble", async () => {
        const roomy = { left: one + gap + inset, right: 900, top: 200 };
        const beside = await shownBeside(roomy, true);
        expect(toolLabels(beside)[0]).toBe("React with thumbs up");
        expect(toolLabels(beside)[1]).toBe("More reactions");
        expect(standing(beside, roomy, true)).toBe("beside");
        beside.unmount();
        document.body.innerHTML = "";

        const tight = { left: bare + gap + inset - 1, right: 900, top: 200 };
        const above = await shownBeside(tight, true);
        expect(standing(above, tight, true)).toBe("over");
    });

    test("in a narrow column never takes more than the first quick reaction, beside or over", async () => {
        const roomy = { left: 60, right: 200, top: 200 };
        const beside = await shownBeside(roomy, false, 560);
        expect(toolLabels(beside)[0]).toBe("React with thumbs up");
        expect(toolLabels(beside)[1]).toBe("More reactions");
        expect(standing(beside, roomy)).toBe("beside");
        beside.unmount();
        document.body.innerHTML = "";

        const tight = { left: 60, right: 560 - inset, top: 200 };
        const above = await shownBeside(tight, false, 560);
        expect(standing(above, tight)).toBe("over");
        expect(toolLabels(above)[1]).toBe("More reactions");
    });

    test("a bubble beside which it fits is not dropped near the column's top, since it never rises over the bubble", async () => {
        const bubble = leaving(full + gap + inset, inset);
        const w = await shownBeside(bubble);

        expect(standing(w, bubble)).toBe("beside");
    });

    // A channel's flat row has no bubble to stand beside: its box stands at the row's top right,
    // 14px in from the row's end and lifted 18px over the row's top, so it straddles the row's top
    // edge and covers the least of the words; only where the column's top leaves no room for that
    // lift does it move inside the row, 4px under the row's top. The approved design places it
    // so, where the box has always stood on a channel's row: the row's top less 18, or the top plus 4
    // near the feed's top edge, kept 4px inside the column as every box is.
    describe("on a channel's row", () => {
        /** How far the box is lifted over a channel row's top, in from the row's end, and inside the row near the column's top. */
        const lift = 18;
        const endInset = 14;
        const insideGap = 4;

        /** How near the column's inset a row's top must come before the box moves inside it. */
        const insideBelow = 30;

        type Row = { left: number, right: number, top: number };

        /** Mounts the box on a channel's row at the box given, its column 900px wide from the window's top. */
        async function shownOnRow(row: Row): Promise<VueWrapper> {
            const rect = (left: number, right: number, top: number, bottom: number): DOMRect => ({ x: left, y: top, top, left, right, bottom, width: right - left, height: bottom - top, toJSON: () => ({}) }) as DOMRect;
            const scroller = document.createElement("div");
            scroller.setAttribute("data-chat-scroller", "");
            scroller.getBoundingClientRect = () => rect(0, 900, 0, 600);
            Object.defineProperty(scroller, "clientWidth", { value: 900 });
            Object.defineProperty(scroller, "clientHeight", { value: 600 });
            const host = document.createElement("div");
            scroller.append(host);
            document.body.append(scroller);

            wrapper = mount(ActionBar, { props: { message: message(), isCommunity: true, isShown: true, favorites }, attachTo: host });
            await settle();

            // The dock draws no box, so the box is placed against the row holding it.
            const guard = wrapper.get("[data-testid='action-bar-guard']").element as HTMLElement;
            const anchor = guard.parentElement?.parentElement as HTMLElement;
            anchor.getBoundingClientRect = () => rect(row.left, row.right, row.top, row.top + 44);
            await wrapper.setProps({ message: message({ id: 43 }) });
            for (let i = 0; i < 40; i++) {
                await settle();
            }
            return wrapper;
        }

        /** The box's top and its end's distance in from the row's end, as drawn. */
        function placed(w: VueWrapper, row: Row): { top: number, endIn: number } {
            const box = w.get("[data-testid='message-box']").element as HTMLElement;
            return { top: parseFloat(box.style.top), endIn: row.right - (parseFloat(box.style.left) + box.offsetWidth) };
        }

        test("stands at the row's top right, lifted over the row's top edge and in from its end", async () => {
            const row = { left: 0, right: 900, top: 200 };
            const w = await shownOnRow(row);

            expect(placed(w, row)).toEqual({ top: row.top - lift, endIn: endInset });
            expect(toolLabels(w)).toEqual([...reactionLabels, "Reply in thread", "Quote", "More actions"]);
        });

        // As the approved demo places it: a row whose top is less than 30px under the column's 4px
        // inset has the box inside it, so a box over the first row never presses on the feed's top.
        test("keeps the lift on a row 30px under the column's 4px inset, and moves inside a row a pixel nearer", async () => {
            const roomy = { left: 0, right: 900, top: inset + insideBelow };
            const lifted = await shownOnRow(roomy);
            expect(placed(lifted, roomy).top).toBe(roomy.top - lift);
            lifted.unmount();
            document.body.innerHTML = "";

            const tight = { left: 0, right: 900, top: inset + insideBelow - 1 };
            const inside = await shownOnRow(tight);
            expect(placed(inside, tight)).toEqual({ top: tight.top + insideGap, endIn: endInset });
        });
    });
});

// Asked for less motion, an entrance becomes a short fade rather than nothing: the bar fades in
// and out where it stands, without its 3px rise and sink.
describe("the bar under reduced motion", () => {
    /** Each rule the bar's styles give under reduced motion, by selector. */
    function reducedMotionRules(): Map<string, string> {
        const source = readFileSync(join(__dirname, "../../../../../src/Communication/Chat/ChatShell/components/actionBar.partial.obs"), "utf8");
        const style = source.slice(source.indexOf("<style"), source.indexOf("</style>")).replace(/\/\*[\s\S]*?\*\//g, "");
        const start = style.indexOf("@media (prefers-reduced-motion: reduce)");
        let depth = 0;
        let end = style.indexOf("{", start);
        do {
            depth += style[end] === "{" ? 1 : style[end] === "}" ? -1 : 0;
            end++;
        } while (depth > 0 && end < style.length);
        const inner = style.slice(style.indexOf("{", start) + 1, end - 1);
        const rules = new Map<string, string>();
        for (const rule of inner.matchAll(/([^{}]+)\{([^{}]*)\}/g)) {
            for (const selector of rule[1].split(",")) {
                rules.set(selector.trim(), rule[2].trim());
            }
        }
        return rules;
    }

    test("fades in and out, with no rise or sink", () => {
        const source = readFileSync(join(__dirname, "../../../../../src/Communication/Chat/ChatShell/components/actionBar.partial.obs"), "utf8");
        const style = source.slice(source.indexOf("<style"), source.indexOf("</style>")).replace(/\/\*[\s\S]*?\*\//g, "");
        const sheet = readFileSync(join(__dirname, "../../../../../../RockWeb/Styles/Blocks/Communication/Chat/chat.css"), "utf8");

        // The rise and sink are the set's slides, by the small rise, which the token sheet takes to a fade under reduced motion.
        expect(style).toMatch(/\.chat-tools-enter-active\s*\{[^}]*--chat-slide-from:\s*var\(--chat-rise-sm\);[^}]*animation:\s*chat-slide-in var\(--chat-d-tools-move\) var\(--chat-ease-in\), chat-fade-in var\(--chat-d-tools-show\) linear;/);
        expect(style).toMatch(/\.chat-tools-leave-active\s*\{[^}]*--chat-slide-to:\s*var\(--chat-rise-sm\);[^}]*animation:\s*chat-slide-out var\(--chat-d-tools-move\) var\(--chat-ease-in\) forwards, chat-fade-out var\(--chat-d-tools-hide\) linear forwards;/);
        expect(style).not.toContain("prefers-reduced-motion");
        expect(sheet).toMatch(/@media \(prefers-reduced-motion: reduce\) \{\s*@keyframes chat-slide-in \{\s*from \{\s*opacity: 0;/);
    });
});

// One look for every tool: each is the shared icon button at its one size, a quick reaction
// its glyph inside one, so the bar restyles nothing from outside its parts.
describe("the bar's parts", () => {
    test("every tool is the shared icon button, and the bar's sheet reaches into no part", async () => {
        const w = await shown({ message: message() });
        const source = readFileSync(join(__dirname, "../../../../../src/Communication/Chat/ChatShell/components/actionBar.partial.obs"), "utf8");
        const style = source.slice(source.indexOf("<style"), source.indexOf("</style>"));

        const tools = w.get("[data-testid='action-bar']").findAll(":scope > button, :scope > [role='group'] > button");
        expect(tools.length).toBe(7);
        expect(tools.filter(tool => !tool.classes("chat-icon-btn"))).toHaveLength(0);
        expect(style).not.toContain(":deep(");
    });

    // A tool's focus outline is drawn outside the tool, past the box's edge, and the box's hit pad
    // reaches 6px past it; a box that cut what overflows it would cut both. Only the grown picker's
    // foot has a fill that meets the box's corners, so the foot takes the box's corners as its own.
    test("the box cuts nothing that overflows it, and the grown picker's foot rounds its own corners", () => {
        const sheetOf = (file: string): string => {
            const source = readFileSync(join(__dirname, `../../../../../src/Communication/Chat/ChatShell/components/${file}`), "utf8");
            return source.slice(source.indexOf("<style"), source.indexOf("</style>")).replace(/\/\*[\s\S]*?\*\//g, "");
        };
        const ruleOf = (style: string, selector: string): string => style.match(new RegExp(`(?:^|\\n)${selector.replace(/[.]/g, "\\.")}\\s*\\{([^}]*)\\}`))?.[1] ?? "";

        expect(ruleOf(sheetOf("actionBar.partial.obs"), ".chat-box")).not.toContain("overflow");
        const picker = sheetOf("emojiPicker.partial.obs");
        expect(ruleOf(picker, ".chat-picker-host")).toMatch(/border-radius:\s*inherit;/);
        expect(ruleOf(picker, ".chat-picker-host > .chat-picker")).toMatch(/border-radius:\s*inherit;/);
        expect(ruleOf(picker, ".chat-picker-foot")).toMatch(/border-bottom-right-radius:\s*inherit;[\s\S]*border-bottom-left-radius:\s*inherit;/);
    });
});

// The block's one held box: where it opens, the quick tray unless the control that asked has
// offered the quick reactions already, and what the message rows read of it while it is up.
// Every row asks about its own message, so opening and closing it reaches the rows of that
// message and no other. The shell is faked with only what the box reads. Moved from the old
// picker host's own tests, which the box took over.
describe("the block's boxes", () => {
    const team = "c0000001-0000-4000-8000-000000000000";

    function setUp(): ActionBars {
        const shell = shallowRef({
            reactions: {
                favorites: [],
                state: reactive({ catalog: null, recents: [] }),
                loadCatalog: jest.fn(async () => null),
                reactorNames: jest.fn(),
                recordRecent: jest.fn(),
                toggleCode: jest.fn()
            },
            timelines: { state: () => ({ messages: [] }) },
            threads: { state: { thread: null } }
        } as unknown as ChatShell);

        let bars: ActionBars | null = null;
        wrapper = mount(defineComponent({
            setup() {
                bars = provideActionBars(shell, "Cedar Hill", () => undefined);
                return () => null;
            }
        }));
        return bars as unknown as ActionBars;
    }

    function stored(id: number): TimelineMessage {
        return { id, body: "Hello" } as TimelineMessage;
    }

    test("opens on the quick tray, or on the full picker when the control that asked says so", () => {
        const picker = setUp();
        const anchor = document.createElement("button");
        const sheetPlus = document.createElement("button");

        picker.openReactionPicker("conversation", team, stored(7), anchor);
        expect(picker.held.value?.layer).toBe("tray");

        picker.openReactionPicker("conversation", team, stored(7), sheetPlus, sheetPlus, { startsOn: "full" });
        expect(picker.held.value?.layer).toBe("picker");
    });

    test("keeps the quick reactions the control that asked already shows, for its tray to leave out", () => {
        const picker = setUp();
        const add = document.createElement("button");

        picker.openReactionPicker("conversation", team, stored(7), add, add, { offered: ["thumbsup", "heart", "joy"] });
        expect(picker.held.value).toMatchObject({ layer: "tray", offered: ["thumbsup", "heart", "joy"] });

        picker.closeReactionPicker();
        picker.openReactionPicker("conversation", team, stored(7), add);
        expect(picker.held.value?.offered).toEqual([]);
    });

    // Only a control says whether what it opens is open; the box's place in the row, which the
    // bar's Add reaction asks from, is no control, so it carries no such state.
    test("marks the control that opened it expanded while it is open, and never the box's place in the row", () => {
        const picker = setUp();
        const place = document.createElement("span");

        picker.openReactionPicker("conversation", team, stored(7), place);
        expect(place.hasAttribute("aria-expanded")).toBe(false);
        picker.closeReactionPicker();
        expect(place.hasAttribute("aria-expanded")).toBe(false);

        const chipAdd = document.createElement("button");
        picker.openReactionPicker("conversation", team, stored(7), place, chipAdd);
        expect(chipAdd.getAttribute("aria-expanded")).toBe("true");
        picker.closeReactionPicker();
        expect(chipAdd.getAttribute("aria-expanded")).toBe("false");
    });

    test("opening and closing reaches only the rows of the message it is open on", () => {
        const picker = setUp();
        const anchor = document.createElement("button");
        const runs = { 7: 0, 8: 0 };
        const seen: Record<number, HTMLElement | null> = {};
        for (const id of [7, 8] as const) {
            effect(() => {
                seen[id] = picker.reactionHost.pickerAnchorOf(id);
                runs[id]++;
            });
        }

        picker.openReactionPicker("conversation", team, stored(7), anchor);
        expect(seen[7]).toBe(anchor);
        expect(seen[8]).toBeNull();

        picker.closeReactionPicker();
        expect(seen[7]).toBeNull();

        expect(runs).toEqual({ 7: 3, 8: 1 });
    });
});

// Every row asks whether the column's box is on its own message, so the box moving from one
// message to the next wakes the row it leaves and the row it reaches, never the rest of the feed.
describe("the column's box on a message", () => {
    test("is read per message, so a move wakes only the row it leaves and the row it reaches", () => {
        const scroller = document.createElement("div");
        const rows = new Map<number, HTMLElement>();
        for (const id of [1, 2, 3, 4, 5]) {
            const row = document.createElement("div");
            row.dataset.hotId = String(id);
            scroller.append(row);
            rows.set(id, row);
        }
        document.body.append(scroller);

        let column: ColumnActionBar | null = null;
        wrapper = mount(defineComponent({
            setup() {
                column = useActionBar(() => scroller);
                return () => h("div");
            }
        }), { attachTo: document.body });
        const bar = column as unknown as ColumnActionBar;
        const hover = (id: number): void => {
            bar.onPointerOver({ pointerType: "mouse", target: rows.get(id) } as unknown as PointerEvent);
            jest.advanceTimersByTime(1000);
        };

        const runs: Record<number, number> = { 1: 0, 2: 0, 3: 0, 4: 0, 5: 0 };
        const seen: Record<number, boolean> = {};
        for (const id of [1, 2, 3, 4, 5]) {
            effect(() => {
                seen[id] = bar.isOn(id);
                runs[id]++;
            });
        }

        hover(1);
        expect(seen).toEqual({ 1: true, 2: false, 3: false, 4: false, 5: false });
        hover(2);
        expect(seen).toEqual({ 1: false, 2: true, 3: false, 4: false, 5: false });

        expect(runs).toEqual({ 1: 3, 2: 2, 3: 1, 4: 1, 5: 1 });
    });
});

// Another floating layer open keeps the box down under the pointer. Once it closes, the box rises
// on the message the pointer rests on, with no move needed to wake it.
describe("the column's box under a closing layer", () => {
    test("rises on the message the pointer rests on once the floating layer that kept it down closes", async () => {
        const scroller = document.createElement("div");
        const row = document.createElement("div");
        row.dataset.hotId = "1";
        scroller.append(row);
        document.body.append(scroller);

        let column: ColumnActionBar | null = null;
        let layer: PopoverControl | null = null;
        wrapper = mount(defineComponent({
            setup() {
                column = useActionBar(() => scroller);
                layer = usePopover(ref<HTMLElement>(), ref<HTMLElement>());
                return () => h("div");
            }
        }), { attachTo: document.body });
        const bar = column as unknown as ColumnActionBar;
        const card = layer as unknown as PopoverControl;

        await card.open();
        bar.onPointerOver({ pointerType: "mouse", target: row } as unknown as PointerEvent);
        jest.advanceTimersByTime(1000);
        expect(bar.isOn(1)).toBe(false);

        card.close();
        await settle();
        jest.advanceTimersByTime(1000);

        expect(bar.isOn(1)).toBe(true);
    });
});

// The box grows from where the bar stood. It is placed at the tray's size first and only then
// revealed from the bar's edges, so beside the reader's own bubble, where the box keeps its edge
// by the bubble, it opens out away from the bubble and never jumps sideways as it grows.
describe("growing beside the reader's own bubble", () => {
    const animate = jest.fn();
    let sheet: HTMLStyleElement;

    /** The box's width and height for each layer, as these tests lay it out. */
    const sizes: Record<string, [number, number]> = { bar: [200, 38], tray: [320, 44] };

    const offsetWidth = Object.getOwnPropertyDescriptor(HTMLElement.prototype, "offsetWidth");
    const offsetHeight = Object.getOwnPropertyDescriptor(HTMLElement.prototype, "offsetHeight");
    const rectOf = Element.prototype.getBoundingClientRect;

    function sizeOf(element: HTMLElement): [number, number] {
        return element.getAttribute("data-testid") === "message-box" ? sizes[element.getAttribute("data-layer") ?? "bar"] : [0, 0];
    }

    function rect(left: number, right: number, top: number, bottom: number): DOMRect {
        return { x: left, y: top, top, left, right, bottom, width: right - left, height: bottom - top, toJSON: () => ({}) } as DOMRect;
    }

    beforeEach(() => {
        animate.mockClear();
        (HTMLElement.prototype as unknown as { animate: unknown }).animate = animate;
        sheet = document.createElement("style");
        sheet.textContent = ".chat-box { --chat-d-move: 220ms; }";
        document.head.append(sheet);
        Object.defineProperty(HTMLElement.prototype, "offsetWidth", { configurable: true, get(this: HTMLElement) { return sizeOf(this)[0]; } });
        Object.defineProperty(HTMLElement.prototype, "offsetHeight", { configurable: true, get(this: HTMLElement) { return sizeOf(this)[1]; } });
        // The box is where its floating styles put it, at its layer's size.
        Element.prototype.getBoundingClientRect = function (this: Element): DOMRect {
            if (this instanceof HTMLElement && this.getAttribute("data-testid") === "message-box") {
                const [width, height] = sizeOf(this);
                const left = parseFloat(this.style.left) || 0;
                const top = parseFloat(this.style.top) || 0;
                return rect(left, left + width, top, top + height);
            }
            return rectOf.call(this);
        };
        Object.defineProperty(document.documentElement, "clientWidth", { configurable: true, value: 1200 });
        Object.defineProperty(document.documentElement, "clientHeight", { configurable: true, value: 800 });
    });

    afterEach(() => {
        delete (HTMLElement.prototype as unknown as { animate?: unknown }).animate;
        sheet.remove();
        Object.defineProperty(HTMLElement.prototype, "offsetWidth", offsetWidth as PropertyDescriptor);
        Object.defineProperty(HTMLElement.prototype, "offsetHeight", offsetHeight as PropertyDescriptor);
        Element.prototype.getBoundingClientRect = rectOf;
        delete (document.documentElement as unknown as Record<string, unknown>).clientWidth;
        delete (document.documentElement as unknown as Record<string, unknown>).clientHeight;
    });

    test("reveals the tray from the bar's edges, opening away from the bubble", async () => {
        const scroller = document.createElement("div");
        scroller.setAttribute("data-chat-scroller", "");
        scroller.getBoundingClientRect = () => rect(0, 900, 0, 600);
        Object.defineProperty(scroller, "clientWidth", { value: 900 });
        Object.defineProperty(scroller, "clientHeight", { value: 600 });
        const host = document.createElement("div");
        scroller.append(host);
        document.body.append(scroller);

        let bars: ActionBars | null = null;
        const shell = shallowRef({
            reactions: { favorites, state: reactive({ catalog: null, recents: [] }), loadCatalog: jest.fn(async () => null), recordRecent: jest.fn(), toggleCode: jest.fn() },
            timelines: { state: () => ({ messages: [] }) },
            threads: { state: { thread: null } }
        } as unknown as ChatShell);
        const Host = defineComponent({
            props: { id: { type: Number, default: 42 } },
            setup(props) {
                bars = provideActionBars(shell, "Cedar Hill", () => undefined);
                return () => h(ActionBar, { message: own({ id: props.id }), isOwn: true, isShown: true, favorites });
            }
        });
        wrapper = mount(Host, { attachTo: host });
        await settle();

        // The reader's bubble, at the column's right; the bar stands 6px left of it, level with its top.
        const guard = document.querySelector<HTMLElement>("[data-testid='action-bar-guard']") as HTMLElement;
        (guard.parentElement?.parentElement as HTMLElement).getBoundingClientRect = () => rect(600, 880, 200, 260);
        await wrapper.setProps({ id: 43 });
        for (let i = 0; i < 40; i++) {
            await settle();
        }
        const drawn = document.querySelector<HTMLElement>("[data-testid='message-box']") as HTMLElement;
        expect([drawn.style.left, drawn.style.top]).toEqual(["394px", "200px"]);

        (bars as unknown as ActionBars).openReactionPicker("conversation", "c1", own({ id: 43 }), guard, guard, { offered: ["thumbsup", "heart", "joy"] });
        for (let i = 0; i < 40; i++) {
            await settle();
        }

        // Placed with its right edge where the bar's was, then revealed from the bar's 200 by 38:
        // 120px to its left and 6px at its foot grow in, nothing on its right.
        expect([drawn.style.left, drawn.style.top]).toEqual(["274px", "200px"]);
        expect(animate).toHaveBeenCalledTimes(1);
        expect(animate.mock.calls[0][0][0]).toEqual({ clipPath: "inset(0px 0px 6px 120px)" });
    });
});

// The box's tray opened by a control outside it, as the smile-plus under a message's chips,
// stands at that control, from its start, over it where there is room; the hover bar's own Add
// reaction keeps growing the box where the bar stands. A control that goes while the tray is up
// leaves the tray by its message rather than at the window's corner.
describe("grown from a control outside the box", () => {
    const animate = jest.fn();
    let sheet: HTMLStyleElement;
    const sizes: Record<string, [number, number]> = { bar: [200, 38], tray: [320, 44] };

    const offsetWidth = Object.getOwnPropertyDescriptor(HTMLElement.prototype, "offsetWidth");
    const offsetHeight = Object.getOwnPropertyDescriptor(HTMLElement.prototype, "offsetHeight");
    const rectOf = Element.prototype.getBoundingClientRect;

    function sizeOf(element: HTMLElement): [number, number] {
        return element.getAttribute("data-testid") === "message-box" ? sizes[element.getAttribute("data-layer") ?? "bar"] : [0, 0];
    }

    function rect(left: number, right: number, top: number, bottom: number): DOMRect {
        return { x: left, y: top, top, left, right, bottom, width: right - left, height: bottom - top, toJSON: () => ({}) } as DOMRect;
    }

    beforeEach(() => {
        animate.mockClear();
        (HTMLElement.prototype as unknown as { animate: unknown }).animate = animate;
        sheet = document.createElement("style");
        sheet.textContent = ".chat-box { --chat-d-move: 220ms; }";
        document.head.append(sheet);
        Object.defineProperty(HTMLElement.prototype, "offsetWidth", { configurable: true, get(this: HTMLElement) { return sizeOf(this)[0]; } });
        Object.defineProperty(HTMLElement.prototype, "offsetHeight", { configurable: true, get(this: HTMLElement) { return sizeOf(this)[1]; } });
        Element.prototype.getBoundingClientRect = function (this: Element): DOMRect {
            if (this instanceof HTMLElement && this.getAttribute("data-testid") === "message-box") {
                const [width, height] = sizeOf(this);
                const left = parseFloat(this.style.left) || 0;
                const top = parseFloat(this.style.top) || 0;
                return rect(left, left + width, top, top + height);
            }
            return rectOf.call(this);
        };
        Object.defineProperty(document.documentElement, "clientWidth", { configurable: true, value: 1200 });
        Object.defineProperty(document.documentElement, "clientHeight", { configurable: true, value: 800 });
    });

    afterEach(() => {
        delete (HTMLElement.prototype as unknown as { animate?: unknown }).animate;
        sheet.remove();
        Object.defineProperty(HTMLElement.prototype, "offsetWidth", offsetWidth as PropertyDescriptor);
        Object.defineProperty(HTMLElement.prototype, "offsetHeight", offsetHeight as PropertyDescriptor);
        Element.prototype.getBoundingClientRect = rectOf;
        delete (document.documentElement as unknown as Record<string, unknown>).clientWidth;
        delete (document.documentElement as unknown as Record<string, unknown>).clientHeight;
    });

    async function settled(): Promise<void> {
        for (let i = 0; i < 40; i++) {
            await settle();
        }
    }

    /**
     * Draws another person's bubble at 60 to 500 across and 200 to 260 down a 900 by 600 column,
     * the bar beside it, and the smile-plus under its chips at 70 to 98 across, 266 to 290 down.
     */
    async function drawn(): Promise<{ bars: ActionBars, guard: HTMLElement, chipAdd: HTMLElement }> {
        const scroller = document.createElement("div");
        scroller.setAttribute("data-chat-scroller", "");
        scroller.getBoundingClientRect = () => rect(0, 900, 0, 600);
        Object.defineProperty(scroller, "clientWidth", { value: 900 });
        Object.defineProperty(scroller, "clientHeight", { value: 600 });
        const host = document.createElement("div");
        const chipAdd = document.createElement("button");
        chipAdd.getBoundingClientRect = () => rect(70, 98, 266, 290);
        scroller.append(host, chipAdd);
        document.body.append(scroller);

        let bars: ActionBars | null = null;
        const shell = shallowRef({
            reactions: { favorites, state: reactive({ catalog: null, recents: [] }), loadCatalog: jest.fn(async () => null), recordRecent: jest.fn(), toggleCode: jest.fn() },
            timelines: { state: () => ({ messages: [] }) },
            threads: { state: { thread: null } }
        } as unknown as ChatShell);
        const Host = defineComponent({
            props: { id: { type: Number, default: 42 } },
            setup(props) {
                bars = provideActionBars(shell, "Cedar Hill", () => undefined);
                return () => h(ActionBar, { message: message({ id: props.id }), isShown: true, favorites });
            }
        });
        wrapper = mount(Host, { attachTo: host });
        await settle();

        const guard = document.querySelector<HTMLElement>("[data-testid='action-bar-guard']") as HTMLElement;
        (guard.parentElement?.parentElement as HTMLElement).getBoundingClientRect = () => rect(60, 500, 200, 260);
        await wrapper.setProps({ id: 43 });
        await settled();
        return { bars: bars as unknown as ActionBars, guard, chipAdd };
    }

    function boxAt(): [string, string] {
        const box = document.querySelector<HTMLElement>("[data-testid='message-box']") as HTMLElement;
        return [box.style.left, box.style.top];
    }

    test("stands over that control, from its start, 4px off it", async () => {
        const { bars, chipAdd } = await drawn();
        expect(boxAt()).toEqual(["506px", "200px"]);

        bars.openReactionPicker("conversation", "c1", message({ id: 43 }), chipAdd, chipAdd);
        await settled();

        expect(boxAt()).toEqual(["70px", `${266 - 4 - 44}px`]);
    });

    // Revealed from the bar's edges beside the bubble, a box now far from them would sweep in
    // from nowhere; it moved, so it is simply there, and so is the bar it gives back.
    test("appears there whole, as the bar it gives back does beside the bubble, neither revealed from the other's edges", async () => {
        const { bars, chipAdd } = await drawn();

        bars.openReactionPicker("conversation", "c1", message({ id: 43 }), chipAdd, chipAdd);
        await settled();
        bars.closeReactionPicker();
        await settled();

        expect(boxAt()).toEqual(["506px", "200px"]);
        expect(animate).not.toHaveBeenCalled();
    });

    test("the bar's own Add reaction still grows the box where the bar stands", async () => {
        const { bars, guard } = await drawn();

        bars.openReactionPicker("conversation", "c1", message({ id: 43 }), guard, guard);
        await settled();

        expect(boxAt()).toEqual(["506px", "200px"]);
    });

    test("stays by its message, not at the window's corner, once that control goes", async () => {
        const { bars, chipAdd } = await drawn();
        bars.openReactionPicker("conversation", "c1", message({ id: 43 }), chipAdd, chipAdd);
        await settled();

        chipAdd.remove();
        window.dispatchEvent(new Event("resize"));
        await settled();

        expect(boxAt()).toEqual(["60px", `${200 - 4 - 44}px`]);
    });
});
