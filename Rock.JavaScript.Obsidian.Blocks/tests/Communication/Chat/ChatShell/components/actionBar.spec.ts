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
// A message's action bar, mounted: Reply in thread, Quote and More, docked on the message the
// pointer rests on. It waits a moment before it shows, so a pointer passing over the feed does not
// flicker bars along its path, and lingers a moment after the pointer leaves, so moving onto the
// bar does not lose it. It is one stop for the keyboard, the arrows moving between its tools. More
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
//   }
//   emits: replyInThread(messageId), quote(message), edit(message), copy(message),
//          delete(message), serverAction(action)
//
// The bar is the element marked data-testid="action-bar", with role="toolbar"; it is in the page
// only while it shows.
import { mount, VueWrapper } from "@vue/test-utils";
import { nextTick } from "vue";
import ActionBar from "../../../../../src/Communication/Chat/ChatShell/components/actionBar.partial.obs";
import Icon from "../../../../../src/Communication/Chat/ChatShell/ui/icon.partial.obs";
import { PendingMessage, TimelineMessage } from "../../../../../src/Communication/Chat/ChatShell/types.partial";

const me = "a0000001-0000-4000-8000-000000000000";
const anna = "a0000002-0000-4000-8000-000000000000";

/** 9:02 AM on the day the tests are about, in local time. */
const nineOhTwo = new Date(2026, 9, 15, 9, 2).toISOString();

const approve = { id: "approve", label: "Approve", call: "chat_action_approve" };

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
};

let wrapper: VueWrapper | null = null;

async function settle(): Promise<void> {
    await nextTick();
    await nextTick();
}

/** Mounts the bar with the pointer on its message and lets the rest pass, so it shows if it may. */
async function shown(props: BarProps): Promise<VueWrapper> {
    wrapper = mount(ActionBar, { props: { message: null, pending: null, isPointerInside: true, ...props }, attachTo: document.body });
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
    const more = w.get("[data-testid='action-bar'] button[aria-label='More']").element as HTMLElement;
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
    test("are Reply in thread, Quote and More, in a toolbar named for the message", async () => {
        const w = await shown({ message: message() });

        expect(bar(w).attributes("role")).toBe("toolbar");
        expect(bar(w).attributes("aria-label")?.replace(/\s+/g, " ")).toBe("Actions for Anna Reyes's message, 9:02 AM");
        expect(toolLabels(w)).toEqual(["Reply in thread", "Quote", "More"]);
    });

    test("leave out Reply in thread inside a thread", async () => {
        const w = await shown({ message: message({ parent_id: 40 }), isInThread: true });

        expect(toolLabels(w)).toEqual(["Quote", "More"]);
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

    test("is one tab stop, the first tool", async () => {
        const w = await shown({ message: message() });

        expect(tools(w).map(t => t.getAttribute("tabindex"))).toEqual(["0", "-1", "-1"]);
    });

    test("Right moves to the next tool and takes the tab stop with it; Left moves back", async () => {
        const w = await shown({ message: message() });
        const [reply, quote] = tools(w);
        reply.focus();

        reply.dispatchEvent(new KeyboardEvent("keydown", { key: "ArrowRight", bubbles: true }));
        await settle();

        expect(document.activeElement).toBe(quote);
        expect(tools(w).map(t => t.getAttribute("tabindex"))).toEqual(["-1", "0", "-1"]);

        quote.dispatchEvent(new KeyboardEvent("keydown", { key: "ArrowLeft", bubbles: true }));
        await settle();

        expect(document.activeElement).toBe(reply);
    });

    test("Right on the last tool wraps to the first", async () => {
        const w = await shown({ message: message() });
        const all = tools(w);
        all[2].focus();

        all[2].dispatchEvent(new KeyboardEvent("keydown", { key: "ArrowRight", bubbles: true }));
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
