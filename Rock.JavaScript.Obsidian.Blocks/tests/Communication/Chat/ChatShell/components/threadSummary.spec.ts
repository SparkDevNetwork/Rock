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
// A root's thread, under it in the conversation: one pill in every conversation, so a thread
// reads the same wherever it is. A direct message tucks it under the bubble; a channel sets it
// under the text at the text column's left edge. It counts "1 reply", "3 replies" and "99+
// replies" past 99, draws up to three faces with the newest replier first, shows unread with a
// dot, keeps the last reply's time in its tooltip, and says all of it in its label. A deleted
// root keeps its thread, since the replies are still there.
//
// The component these tests are written to, threadSummary.partial.obs:
//
//   props: message: TimelineMessage (required; a root with reply_count, last_reply_at, repliers
//          newest first and thread_unread), isRowOnScreen: boolean (default false)
//   emits: open(rootId: number)
//   marks: root data-testid="thread-summary", class is-unread while unread; data-testid
//          thread-count, thread-faces (the faces drawn with the shared avatar), thread-unread-dot.
//          The pill's tooltip, role="tooltip" inside it once the pointer has rested, reads
//          "Last reply 9:41 AM".
//
// The message row draws the summary under any root with a live reply, a tombstone included, in
// its reactions row (.chat-reacts), and re-emits its open as openThread(rootId). In a channel
// that row sits in the text column (.chat-cm-main), after the text.
import { defineComponent, h, nextTick } from "vue";
import { DOMWrapper, mount, VueWrapper } from "@vue/test-utils";
import ThreadSummary from "../../../../../src/Communication/Chat/ChatShell/components/threadSummary.partial.obs";
import MessageRow from "../../../../../src/Communication/Chat/ChatShell/components/messageRow.partial.obs";
import { TimelineMessage } from "../../../../../src/Communication/Chat/ChatShell/types.partial";

const me = "a0000001-0000-4000-8000-000000000000";

/** 9:41 AM today in local time, so the time reads as today's. */
const todayAtNineFortyOne = ((): string => {
    const at = new Date();
    at.setHours(9, 41, 0, 0);
    return at.toISOString();
})();

/** One replier as chat_messages_json names them. */
function replier(nickName: string, lastName: string, index: number): Record<string, unknown> {
    return { person_alias_guid: `a00000${10 + index}-0000-4000-8000-000000000000`, nick_name: nickName, last_name: lastName, avatar_url: null };
}

/** Five people, newest replier first, each with a different first letter. */
const fivePeople = [
    replier("Anna", "Reyes", 1),
    replier("Ben", "Okafor", 2),
    replier("Cara", "Lin", 3),
    replier("Dev", "Patel", 4),
    replier("Eli", "Moss", 5)
];

/** A root with replies, as history carries it. */
function root(extra: Record<string, unknown> = {}): TimelineMessage {
    return {
        id: 42,
        parent_id: null,
        person_alias_guid: "a0000002-0000-4000-8000-000000000000",
        sender_nick_name: "Anna",
        sender_last_name: "Reyes",
        sender_avatar_url: null,
        sender_listed: true,
        message_type: "text",
        body: "Who's in for Saturday's hike?",
        pieces: null,
        blocks: null,
        created_at: new Date(2026, 9, 12, 9, 2).toISOString(),
        edited_at: null,
        deleted_at: null,
        visibility: "visible",
        visibility_notice: null,
        reply_count: 3,
        last_reply_at: todayAtNineFortyOne,
        participant_count: 3,
        repliers: fivePeople.slice(0, 3),
        thread_following: true,
        thread_unread: false,
        ...extra
    } as unknown as TimelineMessage;
}

function draw(message: TimelineMessage): VueWrapper {
    return mount(ThreadSummary, { props: { message } });
}

/** A message row holding the root, in a channel or a direct message. */
function drawRow(message: TimelineMessage, isChannel: boolean): VueWrapper {
    return mount(MessageRow, { props: { personAliasGuid: me, message, isChannel, position: "single" } });
}

/** Text as a person reads it; some ICU builds put a narrow no-break space before AM and PM. */
function plain(text: string | undefined | null): string {
    return (text ?? "").replace(/\s+/g, " ").trim();
}

/** The pill's tooltip as a resting pointer sees it, after the chat's half second delay. */
async function tooltipOf(pill: VueWrapper | DOMWrapper<Element>): Promise<string> {
    jest.useFakeTimers();
    try {
        await pill.trigger("mouseenter");
        jest.advanceTimersByTime(500);
        await nextTick();
        const tip = pill.find("[role='tooltip']");
        return tip.exists() ? plain(tip.text()) : "";
    }
    finally {
        jest.useRealTimers();
    }
}

/** The first letters of the faces drawn, in order. */
function faces(w: VueWrapper): string {
    return w.get("[data-testid='thread-faces']").findAll("[data-testid='avatar']").map(face => face.text()).join("");
}

describe("the count", () => {
    test.each([
        [1, "1 reply"],
        [3, "3 replies"],
        [99, "99 replies"],
        [100, "99+ replies"]
    ])("reads %i as %s", (count, words) => {
        expect(draw(root({ reply_count: count })).get("[data-testid='thread-count']").text()).toBe(words);
    });
});

describe("the faces", () => {
    test("the pill draws the three newest repliers, newest first", () => {
        expect(faces(draw(root({ repliers: fivePeople })))).toBe("ABC");
    });

    // A channel once drew four faces in a full-width row; every thread is now the one pill.
    test("a channel's thread draws the same three, newest first", () => {
        expect(faces(drawRow(root({ repliers: fivePeople }), true))).toBe("ABC");
    });

    test("draw only as many as replied", () => {
        expect(faces(draw(root({ reply_count: 1, repliers: fivePeople.slice(0, 1) })))).toBe("A");
    });
});

describe("the pill, in a direct message", () => {
    test("is a button labelled with the count and the last reply's time", async () => {
        const pill = draw(root());

        expect(pill.element.tagName).toBe("BUTTON");
        expect(pill.attributes("data-testid")).toBe("thread-summary");
        expect(plain(pill.attributes("aria-label"))).toBe("3 replies, last reply 9:41 AM. Open thread");
        expect(await tooltipOf(pill)).toBe("Last reply 9:41 AM");
    });

    test("unread: marked, dotted and said so in its label", () => {
        const pill = draw(root({ thread_unread: true }));

        expect(pill.classes()).toContain("is-unread");
        expect(pill.find("[data-testid='thread-unread-dot']").exists()).toBe(true);
        expect(plain(pill.attributes("aria-label"))).toBe("3 replies, last reply 9:41 AM, new replies. Open thread");
    });

    test("read: no mark and no dot", () => {
        const pill = draw(root({ thread_unread: false }));

        expect(pill.classes()).not.toContain("is-unread");
        expect(pill.find("[data-testid='thread-unread-dot']").exists()).toBe(false);
    });

    test("opens its thread", async () => {
        const pill = draw(root());

        await pill.trigger("click");

        expect(pill.emitted("open")).toEqual([[42]]);
    });
});

describe("the pill, in a channel", () => {
    test("is the same pill, with the same label and tooltip, and no written-out last reply", async () => {
        const pill = drawRow(root(), true).get("[data-testid='thread-summary']");

        expect(pill.element.tagName).toBe("BUTTON");
        expect(plain(pill.attributes("aria-label"))).toBe("3 replies, last reply 9:41 AM. Open thread");
        expect(await tooltipOf(pill)).toBe("Last reply 9:41 AM");
        expect(pill.classes()).not.toContain("is-community");
        expect(pill.find("[data-testid='thread-last']").exists()).toBe(false);
        expect(pill.find("[data-testid='thread-view']").exists()).toBe(false);
    });

    test("sits under the text, in the text column", () => {
        const row = drawRow(root(), true);

        const holder = row.get(".chat-cm-main > .chat-reacts");
        expect(holder.find("[data-testid='thread-summary']").exists()).toBe(true);
        const children = Array.from(row.get(".chat-cm-main").element.children);
        expect(children.indexOf(holder.element)).toBeGreaterThan(children.indexOf(row.get(".chat-cm-text").element));
    });

    test("unread: marked, dotted and said so in its label", () => {
        const pill = drawRow(root({ thread_unread: true }), true).get("[data-testid='thread-summary']");

        expect(pill.classes()).toContain("is-unread");
        expect(pill.find("[data-testid='thread-unread-dot']").exists()).toBe(true);
        expect(plain(pill.attributes("aria-label"))).toBe("3 replies, last reply 9:41 AM, new replies. Open thread");
    });
});

describe("in the message row", () => {
    test.each([["a direct message", false], ["a channel", true]] as const)("a root with replies draws its thread in %s", async (_kind, isChannel) => {
        const row = drawRow(root(), isChannel);

        const summary = row.findComponent(ThreadSummary);
        expect(summary.exists()).toBe(true);
        expect(await tooltipOf(summary)).toBe("Last reply 9:41 AM");
    });

    test("a root with no reply draws none", () => {
        const row = mount(MessageRow, { props: { personAliasGuid: me, message: root({ reply_count: 0, last_reply_at: null, repliers: null }), position: "single" } });

        expect(row.findComponent(ThreadSummary).exists()).toBe(false);
    });

    test.each([["a direct message", false], ["a channel", true]] as const)("a deleted root keeps its thread in %s", (_kind, isChannel) => {
        const deleted = root({ deleted_at: new Date(2026, 9, 12, 9, 30).toISOString(), body: null, thread_unread: true });
        const row = mount(MessageRow, { props: { personAliasGuid: me, message: deleted, isChannel, position: "single" } });

        expect(row.text()).toContain("This message was deleted.");
        const summary = row.findComponent(ThreadSummary);
        expect(summary.exists()).toBe(true);
        expect(plain(summary.attributes("aria-label"))).toContain("3 replies");
        expect(summary.attributes("disabled")).toBeUndefined();
    });

    // A thread that starts while the reader watches pops in; one already there when the
    // conversation draws does not, or a page load would pop every thread at once.
    test.each([["a direct message", false], ["a channel", true]] as const)("a root's first reply pops its thread in %s", async (_kind, isChannel) => {
        const row = mount(MessageRow, { props: { personAliasGuid: me, message: root({ reply_count: 0, last_reply_at: null, repliers: null }), isChannel, position: "single" } });

        await row.setProps({ message: root({ reply_count: 1, repliers: fivePeople.slice(0, 1) }) });

        expect(row.get("[data-testid='thread-summary']").classes()).toContain("is-popping");
    });

    test.each([["a direct message", false], ["a channel", true]] as const)("a thread already there when the row draws does not pop in %s", (_kind, isChannel) => {
        const row = mount(MessageRow, { props: { personAliasGuid: me, message: root(), isChannel, position: "single" } });

        expect(row.get("[data-testid='thread-summary']").classes()).not.toContain("is-popping");
    });

    // A page opened straight onto a conversation draws its rows before it knows the conversation's
    // kind, then redraws them in that kind's look; a thread that was there all along is not new.
    test.each([["a direct message", true], ["a channel", false]] as const)("a thread already there does not pop when its row is redrawn in %s's look", async (_kind, wasChannel) => {
        const row = mount(MessageRow, { props: { personAliasGuid: me, message: root(), isChannel: wasChannel, position: "single" } });

        await row.setProps({ isChannel: !wasChannel });

        expect(row.get("[data-testid='thread-summary']").classes()).not.toContain("is-popping");
    });

    // Whether the row was on screen is the row's to say, not read from whichever component is
    // the summary's parent.
    test("pops only when told its row was already on screen, whatever holds it", async () => {
        const host = defineComponent({
            props: { isShown: { type: Boolean, default: false }, isRowOnScreen: { type: Boolean, default: false } },
            setup: hostProps => () => hostProps.isShown ? h(ThreadSummary, { message: root(), isRowOnScreen: hostProps.isRowOnScreen }) : h("div")
        });
        const quiet = mount(host);
        const popping = mount(host);

        await quiet.setProps({ isShown: true });
        await popping.setProps({ isShown: true, isRowOnScreen: true });

        expect(quiet.get("[data-testid='thread-summary']").classes()).not.toContain("is-popping");
        expect(popping.get("[data-testid='thread-summary']").classes()).toContain("is-popping");
    });

    test("the pop ends with its animation, so a later redraw does not replay it", async () => {
        const row = mount(MessageRow, { props: { personAliasGuid: me, message: root({ reply_count: 0, last_reply_at: null, repliers: null }), position: "single" } });
        await row.setProps({ message: root({ reply_count: 1, repliers: fivePeople.slice(0, 1) }) });

        await row.get("[data-testid='thread-summary']").trigger("animationend", { animationName: "chat-tsum-pop" });

        expect(row.get("[data-testid='thread-summary']").classes()).not.toContain("is-popping");
    });

    test("opening the thread from the row names the root", async () => {
        const row = mount(MessageRow, { props: { personAliasGuid: me, message: root(), position: "single" } });

        await row.get("[data-testid='thread-summary']").trigger("click");

        expect(row.emitted("openThread")).toEqual([[42]]);
    });
});
