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
// A message's reactions row, mounted: the thread pill first, then one chip per emoji in the order
// the page carries them, each a toggle with its count, the reader's own drawn pressed, and an add
// chip after them that opens the picker. A person previewing a room sees the chips but cannot
// press them; a system line and a deleted message draw none, though a deleted root keeps its
// thread pill. Resting on a chip shows its card of who reacted, drawn from the people the chip
// carries, whole the first time; See all, or a long press on a touch screen, opens the full list.
//
// The component these tests are written to, components/messageReactions.partial.obs:
//
//   props: {
//       message: TimelineMessage,          carries reactions: [{ code, count, mine, emoji, image_url, label }]
//       isPreviewing: boolean,             a room the person has not joined: chips drawn, not pressable
//       isCommunity: boolean,              a channel's flat look
//       isPhone: boolean,                  no add chip; the action sheet reacts instead; the list is a sheet
//       reader: { personAliasGuid, name, photoUrl }   named "You" on a card
//       listReactors: (code | null, after) => Promise<ReactorsPage | null>   the full list, a page at a time
//       isCardDisabled: boolean            no card while the picker is open
//   }
//   slots: default, the thread pill, drawn first in the row
//   emits: toggle(code: string), openPicker(anchor: HTMLElement)
//   marks: the row data-testid="message-reactions"; each chip data-testid="reaction-chip" with
//          data-code, aria-pressed and its count in data-testid="reaction-count"; the add chip
//          data-testid="reaction-add", "Add reaction". A chip's card, data-testid="reaction-card",
//          shows after a pointer that can hover has rested 350ms, or a keyboard focus, and stays
//          for the hover grace (220ms) after the pointer leaves, long enough to reach See all,
//          data-testid="reaction-card-more". The full list is a dialog, data-testid="dialog".
import { mount, VueWrapper } from "@vue/test-utils";
import { nextTick, reactive } from "vue";
import MessageReactions from "../../../../../src/Communication/Chat/ChatShell/components/messageReactions.partial.obs";
import { TimelineMessage } from "../../../../../src/Communication/Chat/ChatShell/types.partial";

const anna = "a0000002-0000-4000-8000-000000000000";

const me = "a0000001-0000-4000-8000-000000000000";
const david = "a0000003-0000-4000-8000-000000000000";

type Person = { person_alias_guid: string, nick_name: string | null, last_name: string | null, avatar_url: string | null, reacted_at: string, reactor_person_alias_guid: string };
type ReactionRow = { code: string, count: number, mine: boolean, emoji: string | null, image_url: string | null, label: string, reactors?: Person[] };

const person = (alias: string, nick: string, last: string, avatar: string | null = null): Person => ({
    person_alias_guid: alias, nick_name: nick, last_name: last, avatar_url: avatar, reacted_at: "2026-10-16T09:01:00Z", reactor_person_alias_guid: alias
});

const reader = { personAliasGuid: me, name: "Jordan Ellis", photoUrl: "https://rock.example.org/jordan.jpg" };

const heart = (count: number, mine = false, reactors?: Person[]): ReactionRow => ({ code: "heart", count, mine, emoji: "❤️", image_url: null, label: "red heart", ...(reactors ? { reactors } : {}) });
const pray = (count: number, mine = false, reactors?: Person[]): ReactionRow => ({ code: "pray", count, mine, emoji: "🙏", image_url: null, label: "praying hands", ...(reactors ? { reactors } : {}) });
const thumbsup = (count: number, mine = false): ReactionRow => ({ code: "thumbsup", count, mine, emoji: "👍", image_url: null, label: "thumbs up" });

function message(reactions: ReactionRow[], extra: Record<string, unknown> = {}): TimelineMessage {
    return {
        id: 42,
        parent_id: null,
        person_alias_guid: anna,
        sender_nick_name: "Anna",
        sender_last_name: "Reyes",
        message_type: "text",
        body: "Morning all",
        visibility: "visible",
        reactions,
        created_at: "2026-10-16T09:02:00Z",
        edited_at: null,
        deleted_at: null,
        ...extra
    } as unknown as TimelineMessage;
}

let wrapper: VueWrapper | null = null;

function mountRow(props: Record<string, unknown>, slots: Record<string, string> = {}): VueWrapper {
    wrapper = mount(MessageReactions, { props: { reader, listReactors: async () => null, ...props }, slots, attachTo: document.body });
    return wrapper;
}

function chips(w: VueWrapper): ReturnType<VueWrapper["findAll"]> {
    return w.findAll("[data-testid='reaction-chip']");
}

async function settle(): Promise<void> {
    for (let i = 0; i < 5; i++) {
        await Promise.resolve();
        await nextTick();
    }
}

afterEach(() => {
    wrapper?.unmount();
    wrapper = null;
    document.body.innerHTML = "";
    jest.useRealTimers();
});

describe("the chips", () => {
    test("are drawn in the order the page carries them, each with its glyph and count", () => {
        const w = mountRow({ message: message([thumbsup(2), heart(5, true), pray(3)]) });

        expect(chips(w).map(c => c.attributes("data-code"))).toEqual(["thumbsup", "heart", "pray"]);
        expect(chips(w).map(c => c.get("[data-testid='reaction-count']").text())).toEqual(["2", "5", "3"]);
        expect(chips(w)[1].text()).toContain("❤️");
    });

    test("the reader's own is pressed, and each label says the count and what a press does", () => {
        const w = mountRow({ message: message([heart(5, true), pray(3)]) });

        expect(chips(w).map(c => c.attributes("aria-pressed"))).toEqual(["true", "false"]);
        expect(chips(w).map(c => c.attributes("aria-label"))).toEqual([
            "red heart, 5, including you. Remove yours",
            "praying hands, 3. Add yours"
        ]);
    });

    test("an image emoji is drawn from its image", () => {
        const custom = { code: "cedar_hill", count: 1, mine: false, emoji: null, image_url: "https://cdn.example.org/cedar.png", label: "cedar hill" };
        const w = mountRow({ message: message([custom]) });

        expect(chips(w)[0].find("img").attributes("src")).toBe("https://cdn.example.org/cedar.png");
    });

    test("a press asks to toggle that emoji", async () => {
        const w = mountRow({ message: message([heart(5, true), pray(3)]) });

        await chips(w)[1].trigger("click");
        await chips(w)[0].trigger("click");

        expect(w.emitted("toggle")).toEqual([["pray"], ["heart"]]);
    });

    // Not disabled, which would take the pointer and the keyboard from the chip and its card with them.
    test("for a person previewing the room are drawn but cannot be pressed, and say nothing of a press", async () => {
        const w = mountRow({ message: message([heart(5), pray(3)]), isPreviewing: true });

        expect(chips(w)).toHaveLength(2);
        expect(chips(w).every(c => c.attributes("aria-disabled") === "true" && c.attributes("disabled") === undefined)).toBe(true);
        expect(chips(w).map(c => c.attributes("aria-label"))).toEqual(["red heart, 5", "praying hands, 3"]);

        await chips(w)[0].trigger("click");
        expect(w.emitted("toggle")).toBeUndefined();
    });
});

describe("the add chip", () => {
    test("follows the chips, named Add reaction, and opens the picker from itself", async () => {
        const w = mountRow({ message: message([heart(1)]) });

        const add = w.get("[data-testid='reaction-add']");
        expect(add.attributes("aria-label")).toBe("Add reaction");
        await add.trigger("click");

        expect(w.emitted("openPicker")).toEqual([[add.element]]);
    });

    test.each([
        ["for a person previewing", { isPreviewing: true }],
        ["on a phone", { isPhone: true }]
    ])("is not drawn %s", (_what, props) => {
        const drawn = mountRow({ message: message([heart(1)]) });
        expect(drawn.find("[data-testid='reaction-add']").exists()).toBe(true);
        drawn.unmount();

        const w = mountRow({ message: message([heart(1)]), ...props });

        expect(w.find("[data-testid='reaction-add']").exists()).toBe(false);
    });
});

describe("where the row is drawn", () => {
    // The message row places the row under its bubble with its own scoped rules, which reach a
    // child only through a single root element; a fragment root drops them and every attribute.
    test("is one root element, the row itself, carrying what the message row passes it", () => {
        wrapper = mount(MessageReactions, {
            props: { reader, listReactors: async () => null, message: message([heart(1)]) },
            attrs: { "data-placed": "under-bubble" },
            attachTo: document.body
        });

        expect(wrapper.element.nodeType).toBe(Node.ELEMENT_NODE);
        expect((wrapper.element as HTMLElement).classList.contains("chat-reacts")).toBe(true);
        expect(wrapper.element.getAttribute("data-placed")).toBe("under-bubble");
    });

    test("the thread pill comes first, before the chips", () => {
        const w = mountRow({ message: message([heart(1)]) }, { default: "<button data-testid='thread-pill'>3 replies</button>" });

        const row = w.get("[data-testid='message-reactions']").element;
        const first = row.querySelector("button");
        expect(first?.getAttribute("data-testid")).toBe("thread-pill");
        expect(chips(w)).toHaveLength(1);
    });

    test("a system line draws no chips", () => {
        const w = mountRow({ message: message([heart(2)], { message_type: "system", body: "Priya Shah joined the channel." }) });

        expect(chips(w)).toHaveLength(0);
        expect(w.find("[data-testid='reaction-add']").exists()).toBe(false);
    });

    test("a deleted message draws no chips, but keeps its thread pill", () => {
        const w = mountRow(
            { message: message([heart(2)], { deleted_at: "2026-10-16T09:05:00Z", body: null }) },
            { default: "<button data-testid='thread-pill'>3 replies</button>" }
        );

        expect(chips(w)).toHaveLength(0);
        expect(w.find("[data-testid='reaction-add']").exists()).toBe(false);
        expect(w.find("[data-testid='thread-pill']").exists()).toBe(true);
    });

    test("with no reactions and no thread, nothing is drawn", () => {
        const w = mountRow({ message: message([]) });

        expect(w.find("[data-testid='message-reactions']").exists()).toBe(false);
    });
});

async function rest(chip: ReturnType<VueWrapper["get"]>, pointerType = "mouse"): Promise<void> {
    await chip.trigger("pointerenter", { pointerType });
    jest.advanceTimersByTime(350);
    await settle();
}

function card(): HTMLElement | null {
    return document.querySelector("[data-testid='reaction-card']");
}

describe("a chip's card", () => {
    test("shows after the pointer rests 350ms, whole the first time, from the people the chip carries, asking for nothing", async () => {
        jest.useFakeTimers();
        const listReactors = jest.fn(async () => null);
        const w = mountRow({ message: message([pray(12, true, [person(anna, "Anna", "Reyes"), person(david, "David", "Okafor")])]), listReactors });
        const chip = chips(w)[0];

        await chip.trigger("pointerenter", { pointerType: "mouse" });
        jest.advanceTimersByTime(349);
        await settle();
        expect(card()).toBeNull();

        jest.advanceTimersByTime(1);
        await settle();
        expect(Array.from(card()!.querySelectorAll("[data-testid='reaction-card-person'] .chat-rx-card-name")).map(row => row.textContent)).toEqual(["You", "Anna Reyes", "David Okafor"]);
        expect(card()!.querySelector("[data-testid='reaction-card-count']")?.textContent).toBe("12 people");
        expect(listReactors).not.toHaveBeenCalled();
    });

    test("is never shown by a finger, which has no hover", async () => {
        jest.useFakeTimers();
        const w = mountRow({ message: message([pray(3)]) });

        await rest(chips(w)[0], "touch");

        expect(card()).toBeNull();
    });

    test("a pointer passing over in less than 350ms shows nothing", async () => {
        jest.useFakeTimers();
        const w = mountRow({ message: message([pray(1)]) });
        const chip = chips(w)[0];

        await chip.trigger("pointerenter", { pointerType: "mouse" });
        jest.advanceTimersByTime(200);
        await chip.trigger("pointerleave");
        jest.advanceTimersByTime(500);
        await settle();

        expect(card()).toBeNull();
    });

    test("stays for the hover grace after the pointer leaves, and for as long as the pointer is on it", async () => {
        jest.useFakeTimers();
        const w = mountRow({ message: message([pray(12, false, [person(anna, "Anna", "Reyes")])]) });
        const chip = chips(w)[0];
        await rest(chip);

        await chip.trigger("pointerleave");
        jest.advanceTimersByTime(219);
        await settle();
        expect(card()).not.toBeNull();

        card()!.parentElement!.dispatchEvent(new Event("pointerenter"));
        jest.advanceTimersByTime(1000);
        await settle();
        expect(card()).not.toBeNull();

        card()!.parentElement!.dispatchEvent(new Event("pointerleave"));
        jest.advanceTimersByTime(220);
        await settle();
        expect(card()).toBeNull();
    });

    test("goes once the grace is out when the pointer never reaches it", async () => {
        jest.useFakeTimers();
        const w = mountRow({ message: message([pray(3)]) });
        const chip = chips(w)[0];
        await rest(chip);

        await chip.trigger("pointerleave");
        jest.advanceTimersByTime(220);
        await settle();

        expect(card()).toBeNull();
    });

    test("is not shown while the picker is open, and goes when it opens", async () => {
        jest.useFakeTimers();
        const w = mountRow({ message: message([pray(3)]), isCardDisabled: true });
        await rest(chips(w)[0]);
        expect(card()).toBeNull();

        await w.setProps({ isCardDisabled: false });
        await rest(chips(w)[0]);
        expect(card()).not.toBeNull();

        await w.setProps({ isCardDisabled: true });
        await settle();
        expect(card()).toBeNull();
    });

    test("is shown to a person previewing the room, who sees who reacted", async () => {
        jest.useFakeTimers();
        const w = mountRow({ message: message([pray(2, false, [person(anna, "Anna", "Reyes")])]), isPreviewing: true });

        await rest(chips(w)[0]);

        expect(card()).not.toBeNull();
    });

    test("starts fetching the faces it will draw as the pointer arrives", async () => {
        jest.useFakeTimers();
        const made: Array<{ src: string }> = [];
        const images = jest.spyOn(window, "Image").mockImplementation(() => {
            const image = { src: "", decode: async () => undefined };
            made.push(image);
            return image as unknown as HTMLImageElement;
        });
        try {
            const w = mountRow({ message: message([pray(2, false, [person(anna, "Anna", "Reyes", "https://rock.example.org/anna-card.jpg")])]) });
            await chips(w)[0].trigger("pointerenter", { pointerType: "mouse" });
        }
        finally {
            images.mockRestore();
        }

        expect(made.map(image => image.src)).toEqual(expect.arrayContaining(["https://rock.example.org/anna-card.jpg"]));
    });

    test("each chip is described by who reacted with it in one sentence", () => {
        const w = mountRow({ message: message([pray(12, true, [person(anna, "Anna", "Reyes"), person(david, "David", "Okafor")])]) });

        const id = chips(w)[0].attributes("aria-describedby");
        expect(document.getElementById(id!)?.textContent).toBe("You, Anna Reyes, David Okafor and 9 others reacted with praying hands");
    });
});

// The card holds See all, so the keyboard reaches it: Tab from a chip whose card shows moves in,
// Shift+Tab and Escape come back, and Tab past it goes on to what follows the chip.
describe("a chip's card from the keyboard", () => {
    async function focusCard(w: VueWrapper): Promise<void> {
        await chips(w)[0].trigger("focus");
        jest.advanceTimersByTime(350);
        await settle();
    }

    test("shows on a keyboard focus, and Tab moves into See all", async () => {
        jest.useFakeTimers();
        const w = mountRow({ message: message([pray(12, false, [person(anna, "Anna", "Reyes")]), heart(1)]) });
        await focusCard(w);
        expect(card()).not.toBeNull();

        await chips(w)[0].trigger("keydown", { key: "Tab" });

        expect(document.activeElement?.getAttribute("data-testid")).toBe("reaction-card-more");
    });

    test("Escape in the card closes it and gives focus back to the chip", async () => {
        jest.useFakeTimers();
        const w = mountRow({ message: message([pray(12, false, [person(anna, "Anna", "Reyes")])]) });
        await focusCard(w);
        await chips(w)[0].trigger("keydown", { key: "Tab" });

        document.activeElement!.dispatchEvent(new KeyboardEvent("keydown", { key: "Escape", bubbles: true }));
        await settle();

        expect(card()).toBeNull();
        expect(document.activeElement).toBe(chips(w)[0].element);
    });

    test("Tab past See all goes on to the next chip", async () => {
        jest.useFakeTimers();
        const w = mountRow({ message: message([pray(12, false, [person(anna, "Anna", "Reyes")]), heart(1)]) });
        await focusCard(w);
        await chips(w)[0].trigger("keydown", { key: "Tab" });

        document.activeElement!.dispatchEvent(new KeyboardEvent("keydown", { key: "Tab", bubbles: true }));
        await settle();

        expect(document.activeElement).toBe(chips(w)[1].element);
    });

    test("Tab past See all skips what is not drawn", async () => {
        jest.useFakeTimers();
        const host = document.createElement("div");
        document.body.append(host);
        const hidden = document.createElement("div");
        hidden.style.display = "none";
        hidden.innerHTML = "<button type='button'>Hidden</button>";
        const after = document.createElement("button");
        after.type = "button";
        document.body.append(hidden, after);
        wrapper = mount(MessageReactions, { props: { reader, listReactors: async () => null, isPhone: true, message: message([pray(12, false, [person(anna, "Anna", "Reyes")])]) }, attachTo: host });
        await focusCard(wrapper);
        await chips(wrapper)[0].trigger("keydown", { key: "Tab" });

        document.activeElement!.dispatchEvent(new KeyboardEvent("keydown", { key: "Tab", bubbles: true }));
        await settle();

        expect(document.activeElement).toBe(after);
    });

    test("the chip that holds the card says so, and names it", async () => {
        jest.useFakeTimers();
        const w = mountRow({ message: message([pray(12, false, [person(anna, "Anna", "Reyes")]), heart(1)]) });
        expect(chips(w)[0].attributes("aria-expanded")).toBe("false");
        expect(chips(w)[0].attributes("aria-controls")).toBeUndefined();

        await focusCard(w);

        const controls = chips(w)[0].attributes("aria-controls");
        expect(chips(w)[0].attributes("aria-expanded")).toBe("true");
        expect(controls && document.getElementById(controls)?.querySelector("[data-testid='reaction-card']")).toBeTruthy();
        expect(chips(w)[1].attributes("aria-expanded")).toBe("false");
    });
});

describe("the full list", () => {
    test("opens from See all on the card's chip, asking for the people after those the chip carries", async () => {
        jest.useFakeTimers();
        const listReactors = jest.fn(async () => ({ reactors: [], has_more: false }));
        const carried = [person(anna, "Anna", "Reyes"), person(david, "David", "Okafor")];
        const w = mountRow({ message: message([heart(1), pray(12, false, carried)]), listReactors });
        await rest(chips(w)[1]);

        (card()!.querySelector("[data-testid='reaction-card-more']") as HTMLElement).click();
        await settle();

        expect(document.querySelector("[data-testid='dialog']")).not.toBeNull();
        expect(document.querySelector("[role='tab'][aria-selected='true']")?.getAttribute("data-code")).toBe("pray");
        expect(listReactors).toHaveBeenCalledWith("pray", { reacted_at: carried[1].reacted_at, reactor_person_alias_guid: david, code: "pray" });
    });

    test("opens from a long press, and the release does not toggle", async () => {
        jest.useFakeTimers();
        const w = mountRow({ message: message([pray(2, true, [person(anna, "Anna", "Reyes")])]) });
        const chip = chips(w)[0];

        await chip.trigger("pointerdown", { pointerType: "touch", clientX: 10, clientY: 10 });
        jest.advanceTimersByTime(450);
        await settle();
        await chip.trigger("pointerup", { pointerType: "touch" });
        await chip.trigger("click");

        expect(document.querySelector("[data-testid='dialog']")).not.toBeNull();
        expect(w.emitted("toggle")).toBeUndefined();
    });

    test("a short tap toggles and opens nothing", async () => {
        jest.useFakeTimers();
        const w = mountRow({ message: message([pray(2)]) });
        const chip = chips(w)[0];

        await chip.trigger("pointerdown", { pointerType: "touch", clientX: 10, clientY: 10 });
        jest.advanceTimersByTime(200);
        await chip.trigger("pointerup", { pointerType: "touch" });
        await chip.trigger("click");
        jest.advanceTimersByTime(500);
        await settle();

        expect(document.querySelector("[data-testid='dialog']")).toBeNull();
        expect(w.emitted("toggle")).toEqual([["pray"]]);
    });

    test("a finger that moves away is scrolling, not holding", async () => {
        jest.useFakeTimers();
        const w = mountRow({ message: message([pray(2)]) });
        const chip = chips(w)[0];

        await chip.trigger("pointerdown", { pointerType: "touch", clientX: 10, clientY: 10 });
        await chip.trigger("pointermove", { pointerType: "touch", clientX: 10, clientY: 19 });
        jest.advanceTimersByTime(500);
        await settle();

        expect(document.querySelector("[data-testid='dialog']")).toBeNull();
    });

    test("a held chip keeps the phone's own menu away", async () => {
        jest.useFakeTimers();
        const w = mountRow({ message: message([pray(2)]) });
        const chip = chips(w)[0];

        await chip.trigger("pointerdown", { pointerType: "touch", clientX: 10, clientY: 10 });
        const menu = new Event("contextmenu", { cancelable: true });
        chip.element.dispatchEvent(menu);

        expect(menu.defaultPrevented).toBe(true);
    });

    // Closing the list gives focus back to the chip without the card rising over it again.
    test("closed after See all, gives focus back to the chip and no card follows", async () => {
        jest.useFakeTimers();
        const w = mountRow({ message: message([pray(12, false, [person(anna, "Anna", "Reyes")])]), listReactors: async () => ({ reactors: [], has_more: false }) });
        await rest(chips(w)[0]);
        (card()!.querySelector("[data-testid='reaction-card-more']") as HTMLElement).click();
        await settle();
        await chips(w)[0].trigger("pointerleave");

        (document.querySelector("[data-testid='reactors-close']") as HTMLElement).click();
        await settle();
        jest.advanceTimersByTime(1000);
        await settle();

        expect(document.activeElement).toBe(chips(w)[0].element);
        expect(card()).toBeNull();
    });

    test("closed after a long press, gives focus back to the chip and no card follows", async () => {
        jest.useFakeTimers();
        const w = mountRow({ message: message([pray(2, true, [person(anna, "Anna", "Reyes")])]), listReactors: async () => ({ reactors: [], has_more: false }) });
        const chip = chips(w)[0];
        await chip.trigger("pointerdown", { pointerType: "touch", clientX: 10, clientY: 10 });
        jest.advanceTimersByTime(450);
        await settle();
        await chip.trigger("pointerup", { pointerType: "touch" });
        await chip.trigger("click");

        (document.querySelector("[data-testid='reactors-close']") as HTMLElement).click();
        await settle();
        jest.advanceTimersByTime(1000);
        await settle();

        expect(document.activeElement).toBe(chip.element);
        expect(card()).toBeNull();
    });
});

// The page changes a message's reactions in place, as the reactions do on a press, an answer or a
// live event, so the row tells a change apart by the values it last drew, never by the objects.
describe("the reader's own change", () => {
    test("pops a chip the reader just added beside others' chips, and only that chip", async () => {
        const held = reactive(message([heart(2)]));
        const w = mountRow({ message: held });

        held.reactions!.push(pray(1, true));
        await settle();

        const added = chips(w).find(c => c.attributes("data-code") === "pray");
        expect(added?.classes()).toContain("is-popping");
        expect(chips(w).find(c => c.attributes("data-code") === "heart")?.classes()).not.toContain("is-popping");
    });

    test("pops the first chip of a message that had none", async () => {
        const held = reactive(message([]));
        const w = mountRow({ message: held });

        held.reactions!.push(thumbsup(1, true));
        await settle();

        expect(chips(w)[0].classes()).toContain("is-popping");
    });

    test("rolls the count when the reader joins others' reaction, and pops nothing", async () => {
        const held = reactive(message([pray(1)]));
        const w = mountRow({ message: held });

        held.reactions![0].count++;
        held.reactions![0].mine = true;
        await settle();

        expect(chips(w)[0].get("[data-testid='reaction-count']").classes()).toContain("is-rolling");
        expect(chips(w)[0].classes()).not.toContain("is-popping");
    });

    test("others' changes move nothing", async () => {
        const held = reactive(message([pray(1)]));
        const w = mountRow({ message: held });

        held.reactions![0].count++;
        held.reactions!.push(heart(1));
        await settle();

        expect(w.findAll(".is-popping")).toHaveLength(0);
        expect(w.findAll(".is-rolling")).toHaveLength(0);
    });
});

// One card for the row, moved between chips: drawn outside the chip, since a lifted chip would
// hold a fixed card to its own box, and always drawing the chip it now belongs to.
describe("the one card", () => {
    test("is not drawn inside the chip, and in the chat's root when there is one", async () => {
        jest.useFakeTimers();
        const root = document.createElement("div");
        root.className = "chat-shell";
        document.body.append(root);
        wrapper = mount(MessageReactions, { props: { reader, listReactors: async () => null, message: message([pray(3)]) }, attachTo: root });
        const chip = chips(wrapper)[0];

        await rest(chip);

        expect(card()).not.toBeNull();
        expect(chip.element.contains(card())).toBe(false);
        expect(card()!.parentElement?.parentElement).toBe(root);
    });

    test("moved from a focused chip to a hovered one draws the second chip", async () => {
        jest.useFakeTimers();
        const w = mountRow({ message: message([pray(1, false, [person(anna, "Anna", "Reyes")]), heart(1, false, [person(david, "David", "Okafor")])]) });

        await chips(w)[0].trigger("focus");
        jest.advanceTimersByTime(350);
        await settle();
        expect(card()?.textContent).toContain("Anna Reyes");

        await rest(chips(w)[1]);

        expect(card()?.textContent).toContain("David Okafor");
    });

    test("goes when its chip is taken away while it shows", async () => {
        jest.useFakeTimers();
        const held = reactive(message([pray(1), heart(1)]));
        const w = mountRow({ message: held });

        await rest(chips(w)[0]);
        expect(card()).not.toBeNull();

        held.reactions!.splice(0, 1);
        await settle();

        expect(card()).toBeNull();
    });
});
