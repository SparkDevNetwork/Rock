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
// A root's thread, under it in the conversation: one button in two looks. A direct message draws
// the pill tucked under the bubble, with up to three faces; a channel draws the full-width row,
// with up to four faces and the last reply's time written out. Both count "1 reply", "3 replies"
// and "99+ replies" past 99, put the newest replier first, show unread with a dot, and say all of
// it in their label. A deleted root keeps its thread, since the replies are still there.
//
// The component these tests are written to, threadSummary.partial.obs:
//
//   props: message: TimelineMessage (required; a root with reply_count, last_reply_at, repliers
//          newest first and thread_unread), isCommunity: boolean (default false, the pill)
//   emits: open(rootId: number)
//   marks: root data-testid="thread-summary", class is-unread while unread; data-testid
//          thread-count, thread-faces (the faces drawn with the shared avatar), thread-unread-dot,
//          thread-last (Community's "Last reply ..."), thread-view (Community's "View thread").
//          The pill's tooltip is its title, "Last reply 9:41 AM".
//
// The message row draws the summary under any root with a live reply, a tombstone included, and
// re-emits its open as openThread(rootId).
import { mount, VueWrapper } from "@vue/test-utils";
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

function draw(message: TimelineMessage, isCommunity = false): VueWrapper {
    return mount(ThreadSummary, { props: { message, isCommunity } });
}

/** Text as a person reads it; some ICU builds put a narrow no-break space before AM and PM. */
function plain(text: string | undefined | null): string {
    return (text ?? "").replace(/\s+/g, " ").trim();
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
    ])("reads %i as %s, in both looks", (count, words) => {
        expect(draw(root({ reply_count: count })).get("[data-testid='thread-count']").text()).toBe(words);
        expect(draw(root({ reply_count: count }), true).get("[data-testid='thread-count']").text()).toBe(words);
    });
});

describe("the faces", () => {
    test("the pill draws the three newest repliers, newest first", () => {
        expect(faces(draw(root({ repliers: fivePeople })))).toBe("ABC");
    });

    test("the Community row draws four, newest first", () => {
        expect(faces(draw(root({ repliers: fivePeople }), true))).toBe("ABCD");
    });

    test("draw only as many as replied", () => {
        expect(faces(draw(root({ reply_count: 1, repliers: fivePeople.slice(0, 1) }), true))).toBe("A");
    });
});

describe("the pill, in a direct message", () => {
    test("is a button labelled with the count and the last reply's time", () => {
        const pill = draw(root());

        expect(pill.element.tagName).toBe("BUTTON");
        expect(pill.attributes("data-testid")).toBe("thread-summary");
        expect(plain(pill.attributes("aria-label"))).toBe("3 replies, last reply 9:41 AM. Open thread");
        expect(plain(pill.attributes("title"))).toBe("Last reply 9:41 AM");
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

describe("the thread row, in a channel", () => {
    test("writes the last reply's day and time, and offers View thread for hover and focus", () => {
        const row = draw(root(), true);

        expect(row.element.tagName).toBe("BUTTON");
        expect(plain(row.get("[data-testid='thread-last']").text())).toBe("Last reply today at 9:41 AM");
        expect(row.get("[data-testid='thread-view']").text()).toContain("View thread");
    });

    test("unread: marked, dotted and said so in its label", () => {
        const row = draw(root({ thread_unread: true }), true);

        expect(row.classes()).toContain("is-unread");
        expect(row.find("[data-testid='thread-unread-dot']").exists()).toBe(true);
        expect(plain(row.attributes("aria-label"))).toBe("3 replies, last reply today at 9:41 AM, new replies. Open thread");
    });
});

describe("in the message row", () => {
    test.each([["a direct message", false], ["a channel", true]] as const)("a root with replies draws its thread in %s", (_kind, isChannel) => {
        const row = mount(MessageRow, { props: { personAliasGuid: me, message: root(), isChannel, position: "single" } });

        const summary = row.findComponent(ThreadSummary);
        expect(summary.exists()).toBe(true);
        expect(summary.props("isCommunity")).toBe(isChannel);
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

    test("opening the thread from the row names the root", async () => {
        const row = mount(MessageRow, { props: { personAliasGuid: me, message: root(), position: "single" } });

        await row.get("[data-testid='thread-summary']").trigger("click");

        expect(row.emitted("openThread")).toEqual([[42]]);
    });
});
