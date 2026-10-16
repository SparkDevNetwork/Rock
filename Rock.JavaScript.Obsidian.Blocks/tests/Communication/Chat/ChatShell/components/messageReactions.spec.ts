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
// thread pill. Resting on a chip shows who reacted, the count until the names arrive.
//
// The component these tests are written to, components/messageReactions.partial.obs:
//
//   props: {
//       message: TimelineMessage,          carries reactions: [{ code, count, mine, emoji, image_url, label }]
//       isPreviewing: boolean,             a room the person has not joined: chips drawn, not pressable
//       isCommunity: boolean,              a channel's flat look
//       isPhone: boolean,                  no add chip; the action sheet reacts instead
//       loadReactorNames: (reaction) => Promise<string[] | null>   who reacted, "You" for the reader
//   }
//   slots: default, the thread pill, drawn first in the row
//   emits: toggle(code: string), openPicker(anchor: HTMLElement)
//   marks: the row data-testid="message-reactions"; each chip data-testid="reaction-chip" with
//          data-code, aria-pressed and its count in data-testid="reaction-count"; the add chip
//          data-testid="reaction-add", "Add reaction". A chip's tooltip, role="tooltip" inside it,
//          shows after the pointer has rested 350ms.
import { mount, VueWrapper } from "@vue/test-utils";
import { nextTick } from "vue";
import MessageReactions from "../../../../../src/Communication/Chat/ChatShell/components/messageReactions.partial.obs";
import { TimelineMessage } from "../../../../../src/Communication/Chat/ChatShell/types.partial";

const anna = "a0000002-0000-4000-8000-000000000000";

type ReactionRow = { code: string, count: number, mine: boolean, emoji: string | null, image_url: string | null, label: string };

const heart = (count: number, mine = false): ReactionRow => ({ code: "heart", count, mine, emoji: "❤️", image_url: null, label: "red heart" });
const pray = (count: number, mine = false): ReactionRow => ({ code: "pray", count, mine, emoji: "🙏", image_url: null, label: "praying hands" });
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
    wrapper = mount(MessageReactions, { props: { loadReactorNames: async () => null, ...props }, slots, attachTo: document.body });
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

    test("for a person previewing the room are drawn but cannot be pressed, and say nothing of a press", async () => {
        const w = mountRow({ message: message([heart(5), pray(3)]), isPreviewing: true });

        expect(chips(w)).toHaveLength(2);
        expect(chips(w).every(c => c.attributes("disabled") !== undefined)).toBe(true);
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
            props: { loadReactorNames: async () => null, message: message([heart(1)]) },
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

describe("a chip's tooltip", () => {
    test("shows after the pointer rests 350ms, the count until the names arrive, then who reacted", async () => {
        jest.useFakeTimers();
        let answer!: (names: string[] | null) => void;
        const loadReactorNames = jest.fn(() => new Promise<string[] | null>(resolve => answer = resolve));
        const w = mountRow({ message: message([pray(3)]), loadReactorNames });
        const chip = chips(w)[0];

        await chip.trigger("mouseenter");
        jest.advanceTimersByTime(349);
        await settle();
        expect(chip.find("[role='tooltip']").exists()).toBe(false);

        jest.advanceTimersByTime(1);
        await settle();
        expect(chip.get("[role='tooltip']").text()).toBe("3 people reacted with 🙏");
        expect(loadReactorNames).toHaveBeenCalledWith(expect.objectContaining({ code: "pray", count: 3 }));

        answer(["Anna Reyes", "David Okafor"]);
        await settle();

        expect(chip.get("[role='tooltip']").text()).toBe("Anna Reyes, David Okafor and 1 more reacted with 🙏");
    });

    test("names the reader You", async () => {
        jest.useFakeTimers();
        const w = mountRow({ message: message([heart(2, true)]), loadReactorNames: async () => ["You", "Anna Reyes"] });
        const chip = chips(w)[0];

        await chip.trigger("mouseenter");
        jest.advanceTimersByTime(350);
        await settle();

        expect(chip.get("[role='tooltip']").text()).toBe("You and Anna Reyes reacted with ❤️");
    });

    test("a pointer passing over in less than 350ms asks for no names", async () => {
        jest.useFakeTimers();
        const loadReactorNames = jest.fn(async () => ["Anna Reyes"]);
        const w = mountRow({ message: message([pray(1)]), loadReactorNames });
        const chip = chips(w)[0];

        await chip.trigger("mouseenter");
        jest.advanceTimersByTime(200);
        await chip.trigger("mouseleave");
        jest.advanceTimersByTime(500);
        await settle();

        expect(loadReactorNames).not.toHaveBeenCalled();
        expect(chip.find("[role='tooltip']").exists()).toBe(false);
    });
});
