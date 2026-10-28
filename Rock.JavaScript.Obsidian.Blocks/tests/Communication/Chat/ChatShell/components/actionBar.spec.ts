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
//   emits: replyInThread(messageId), quote(message), edit(message), copy(message),
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
import { nextTick } from "vue";
import ActionBar from "../../../../../src/Communication/Chat/ChatShell/components/actionBar.partial.obs";
import Icon from "../../../../../src/Communication/Chat/ChatShell/ui/icon.partial.obs";
import { isActionCall } from "../../../../../src/Communication/Chat/ChatShell/composables/useMessageActions.partial";
import { TimelineMessage } from "../../../../../src/Communication/Chat/ChatShell/types.partial";

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
        expect(w.emitted("openPicker")).toEqual([[target, add.element, { offered: favorites.slice(0, 3).map(favorite => favorite.code) }]]);
    });

    test("the bar stays while the picker its More reactions opened is up, and goes once it closes", async () => {
        const w = await shown({ message: message() });
        const add = w.get("[data-testid='action-bar'] button[aria-label='More reactions']").element as HTMLElement;

        await w.setProps({ pickerAnchor: add, isShown: false });
        jest.advanceTimersByTime(500);
        await settle();
        expect(bar(w).exists()).toBe(true);

        await w.setProps({ pickerAnchor: null });
        await settle();
        expect(bar(w).exists()).toBe(false);
    });

    test("a picker placed beside something else does not hold the bar", async () => {
        const w = await shown({ message: message() });

        await w.setProps({ pickerAnchor: document.createElement("button"), isShown: false });
        jest.advanceTimersByTime(500);
        await settle();

        expect(bar(w).exists()).toBe(false);
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
// side away from the face: right of another person's bubble, left of the reader's own. It never
// rises over the message above. Where the column has no room beside the bubble, it lies over the
// bubble's own top corner instead, still within the bubble's own height.
describe("the bar beside a bubble", () => {
    /** Mounts the bar in a bubble's dock, the bubble and the column at the boxes given. */
    async function shownBeside(bubble: { left: number, right: number, top: number }, isOwn = false): Promise<VueWrapper> {
        const box = (left: number, right: number, top: number, bottom: number): DOMRect => ({ x: left, y: top, top, left, right, bottom, width: right - left, height: bottom - top, toJSON: () => ({}) }) as DOMRect;
        const scroller = document.createElement("div");
        scroller.setAttribute("data-chat-scroller", "");
        scroller.getBoundingClientRect = () => box(0, 900, 0, 600);
        const host = document.createElement("div");
        scroller.append(host);
        document.body.append(scroller);

        wrapper = mount(ActionBar, { props: { message: isOwn ? own() : message(), isOwn, isShown: true, favorites }, attachTo: host });
        await settle();

        // The row's dock draws no box, so the bar is placed against the box holding its dock, the
        // bubble's; the bar is placed again as it moves to the next message.
        const anchor = (bar(wrapper).element as HTMLElement).parentElement?.parentElement as HTMLElement;
        anchor.getBoundingClientRect = () => box(bubble.left, bubble.right, bubble.top, bubble.top + 34);
        await wrapper.setProps({ message: isOwn ? own({ id: 43 }) : message({ id: 43 }) });
        await settle();
        await settle();
        return wrapper;
    }

    test("is drawn at the bubble's top, never above it, and beside it rather than over its corner", () => {
        const rules = barRules();

        expect(rules.get(".chat-tools")?.top).toBe("0");
        expect(rules.get(".chat-tools:not(.is-own)")).toMatchObject({ left: "100%", right: "auto" });
        expect(rules.get(".chat-tools.is-own")).toMatchObject({ right: "100%", left: "auto" });
        expect(rules.get(".chat-tools.is-low")).toBeUndefined();
    });

    test("stays beside another person's bubble while the column has room past its right edge", async () => {
        const w = await shownBeside({ left: 60, right: 400, top: 200 });

        expect(bar(w).classes()).not.toContain("is-flush");
    });

    test("lies over the bubble's own top corner when the bubble reaches the column's edge", async () => {
        const w = await shownBeside({ left: 60, right: 900, top: 200 });

        expect(bar(w).classes()).toContain("is-flush");
    });

    test("does the same on the reader's own side, by the room left of the bubble", async () => {
        const beside = await shownBeside({ left: 500, right: 900, top: 200 }, true);
        expect(bar(beside).classes()).not.toContain("is-flush");
        beside.unmount();
        document.body.innerHTML = "";

        const over = await shownBeside({ left: 0, right: 900, top: 200 }, true);
        expect(bar(over).classes()).toContain("is-flush");
    });

    test("is not dropped near the column's top, since it never rises above the bubble", async () => {
        const w = await shownBeside({ left: 60, right: 400, top: 4 });

        expect(bar(w).classes()).not.toContain("is-low");
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
        const rules = reducedMotionRules();

        expect(rules.get(".chat-tools-enter-active")).toMatch(/animation:\s*chat-tools-fade-in var\(--chat-d-tools-show\) linear;?$/);
        expect(rules.get(".chat-tools-leave-active")).toMatch(/animation:\s*chat-tools-fade-out var\(--chat-d-tools-hide\) linear forwards;?$/);
    });
});
