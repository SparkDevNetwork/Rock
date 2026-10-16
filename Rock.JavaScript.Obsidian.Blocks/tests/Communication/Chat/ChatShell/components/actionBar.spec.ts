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
// Quote and More, docked on the message the pointer rests on. It waits a moment before it shows, so a pointer passing over the feed does not
// flicker bars along its path, and lingers a moment after the pointer leaves, so moving onto the
// bar does not lose it. It is one stop for the keyboard, on the first quick reaction, the arrows
// moving between its tools. The quick reactions are the church's first three favorites, pressed
// when the reader reacted with one; in a narrow column only the first stays. More
// opens the message's menu: Edit (the person's own message, never a block post), Copy, the actions
// the server listed, then Delete (their own). A message nothing can be done to draws no bar.
//
// The component these tests are written to, components/actionBar.partial.obs:
//
//   props: {
//       message: TimelineMessage | null,   the stored message, carrying actions: [{ id, label, call }]
//       pending: PendingMessage | null,    the person's own row still sending, or failed
//       isOwn: boolean,
//       isInThread: boolean,               inside the thread drawer, where Reply in thread has no place
//       isPreviewing: boolean,             a room the person has not joined
//       isEditing: boolean,                the message is open in the editor
//       isPointerInside: boolean           the pointer is over the message or its bar
//       favorites: [{ code, label, emoji, image_url }]   the bootstrap's favorites; the first three are quick
//   }
//   emits: replyInThread(messageId), quote(message), edit(message), copy(message),
//          delete(message), serverAction(action), react(message, code), openPicker(message, anchor)
//
// The bar is the element marked data-testid="action-bar", with role="toolbar"; it is in the page
// only while it shows. A quick reaction is read "React with thumbs up" or "Remove your thumbs up";
// Add reaction is read "More reactions" and opens the picker; the separator between the reactions
// and the tools is data-testid="action-bar-separator". The column is the nearest element marked
// data-chat-scroller, narrow at 560px wide or less.
import { mount, VueWrapper } from "@vue/test-utils";
import { nextTick } from "vue";
import ActionBar from "../../../../../src/Communication/Chat/ChatShell/components/actionBar.partial.obs";
import Icon from "../../../../../src/Communication/Chat/ChatShell/ui/icon.partial.obs";
import { isActionCall } from "../../../../../src/Communication/Chat/ChatShell/composables/useMessageActions.partial";
import { PendingMessage, TimelineMessage } from "../../../../../src/Communication/Chat/ChatShell/types.partial";

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
    pending?: PendingMessage | null;
    isOwn?: boolean;
    isInThread?: boolean;
    isPreviewing?: boolean;
    isEditing?: boolean;
    isPointerInside?: boolean;
    favorites?: unknown[];
};

let wrapper: VueWrapper | null = null;

async function settle(): Promise<void> {
    await nextTick();
    await nextTick();
}

/** Mounts the bar with the pointer on its message and lets the rest pass, so it shows if it may. */
async function shown(props: BarProps): Promise<VueWrapper> {
    wrapper = mount(ActionBar, { props: { message: null, pending: null, isPointerInside: true, favorites, ...props }, attachTo: document.body });
    jest.advanceTimersByTime(60);
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

    test("ask to reply in thread with the message's id, and to quote the message", async () => {
        const target = message();
        const w = await shown({ message: target });

        await w.get("[data-testid='action-bar'] button[aria-label='Reply in thread']").trigger("click");
        await w.get("[data-testid='action-bar'] button[aria-label='Quote']").trigger("click");

        expect(w.emitted("replyInThread")).toEqual([[42]]);
        expect(w.emitted("quote")).toEqual([[target]]);
    });
});

describe("the menu", () => {
    test("on the person's own message: Edit, Copy, the server's actions, then Delete, each group apart", async () => {
        const w = await shown({ message: own({ actions: [approve] }), isOwn: true });

        expect(await menuOf(w)).toEqual(["Edit message", "Copy text", "---", "Approve", "---", "Delete message"]);
    });

    test("on someone else's message: Copy and the server's actions only, never Mark unread", async () => {
        const w = await shown({ message: message({ actions: [approve] }) });

        const items = await menuOf(w);

        expect(items).toEqual(["Copy text", "---", "Approve"]);
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

// Each case is held beside a message the same bar does draw, so the case fails for its own reason.
describe("a message nothing can be done to draws no bar", () => {
    const pendingRow = (status: "sending" | "failed"): PendingMessage => ({ localId: "local-1", channelId: "c1", body: "On my way", status, errorCode: null });

    test.each([
        ["a deleted message", { message: message({ deleted_at: nineOhTwo, body: null }) }],
        ["a hidden message", { message: message({ visibility: "hidden", visibility_notice: "Hidden by a moderator.", body: null }) }],
        ["a removed message", { message: message({ visibility: "removed", visibility_notice: "Removed by Cedar Hill staff.", body: null }) }],
        ["a message still sending", { pending: pendingRow("sending"), isOwn: true }],
        ["a message that failed to send", { pending: pendingRow("failed"), isOwn: true }],
        ["a message open in the editor", { message: own(), isOwn: true, isEditing: true }],
        ["a room the person is previewing", { message: message(), isPreviewing: true }]
    ] as Array<[string, BarProps]>)("%s", async (_what, props) => {
        const live = await shown({ message: message() });
        expect(bar(live).exists()).toBe(true);
        live.unmount();
        wrapper = null;

        const none = await shown(props);

        expect(bar(none).exists()).toBe(false);
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

describe("hover intent", () => {
    test("the bar shows once the pointer has rested 60ms on the message, not before", async () => {
        wrapper = mount(ActionBar, { props: { message: message(), pending: null, isPointerInside: false }, attachTo: document.body });
        await wrapper.setProps({ isPointerInside: true });

        jest.advanceTimersByTime(59);
        await settle();
        expect(bar(wrapper).exists()).toBe(false);

        jest.advanceTimersByTime(1);
        await settle();
        expect(bar(wrapper).exists()).toBe(true);
    });

    test("the bar stays 220ms after the pointer leaves, then hides", async () => {
        const w = await shown({ message: message() });
        expect(bar(w).exists()).toBe(true);

        await w.setProps({ isPointerInside: false });
        jest.advanceTimersByTime(219);
        await settle();
        expect(bar(w).exists()).toBe(true);

        jest.advanceTimersByTime(1);
        await settle();
        expect(bar(w).exists()).toBe(false);
    });

    test("a pointer back inside the grace keeps the bar", async () => {
        const w = await shown({ message: message() });

        await w.setProps({ isPointerInside: false });
        jest.advanceTimersByTime(150);
        await w.setProps({ isPointerInside: true });
        jest.advanceTimersByTime(500);
        await settle();

        expect(bar(w).exists()).toBe(true);
    });

    test("a pointer passing over in less than 60ms never shows the bar", async () => {
        wrapper = mount(ActionBar, { props: { message: message(), pending: null, isPointerInside: false }, attachTo: document.body });

        await wrapper.setProps({ isPointerInside: true });
        jest.advanceTimersByTime(40);
        await wrapper.setProps({ isPointerInside: false });
        jest.advanceTimersByTime(500);
        await settle();
        expect(bar(wrapper).exists()).toBe(false);

        await wrapper.setProps({ isPointerInside: true });
        jest.advanceTimersByTime(60);
        await settle();
        expect(bar(wrapper).exists()).toBe(true);
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

    test("Add reaction opens the picker from itself, its tooltip saying Add reaction", async () => {
        const target = message();
        const w = await shown({ message: target });
        const add = w.get("[data-testid='action-bar'] button[aria-label='More reactions']");

        expect(add.attributes("aria-haspopup")).toBe("dialog");
        await add.trigger("mouseenter");
        jest.advanceTimersByTime(500);
        await settle();
        expect(Array.from(document.querySelectorAll("[role='tooltip']")).map(tip => (tip.textContent ?? "").trim())).toEqual(["Add reaction"]);

        await add.trigger("click");
        expect(w.emitted("openPicker")).toEqual([[target, add.element]]);
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

        wrapper = mount(ActionBar, { props: { message: message(), pending: null, isPointerInside: true, favorites }, attachTo: host });
        jest.advanceTimersByTime(60);
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
