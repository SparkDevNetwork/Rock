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
// How a channel's messages are laid out before anything is drawn: which messages share a group,
// where each day starts and what it is called, and the words of the unread strip and Jump to
// latest; which message the New divider stands above is the platform's answer. These are pure
// rules, read without mounting, so each layout draws the same groups. The clock and the locale are fixed here, so a test never depends on the
// day it runs or the machine it runs on. Dates are built in local time, the time a reader sees.
import {
    blockedRunText,
    dayLabel,
    FeedSection,
    groupWindowMs,
    jumpLabel,
    layoutFeed,
    nearBottomPx,
    newMessagesText,
    sentHoldMs,
    skeletonDelayMs,
    timeLabel,
    unreadStripText
} from "../../../../src/Communication/Chat/ChatShell/feedLayout.partial";
import { PendingMessage, TimelineMessage } from "../../../../src/Communication/Chat/ChatShell/types.partial";

const me = "a0000001-0000-4000-8000-000000000000";
const anna = "a0000002-0000-4000-8000-000000000000";
const ben = "a0000003-0000-4000-8000-000000000000";

const locale = "en-US";

/** Monday, October 12, 2026, 3 PM, in local time. */
const now = new Date(2026, 9, 12, 15, 0);

/** A local time on the fixed day, or a number of days before it. */
function at(hour: number, minute: number, second = 0, daysBefore = 0): string {
    return new Date(2026, 9, 12 - daysBefore, hour, minute, second).toISOString();
}

/** A stored message, in the shape chat_get_history returns. */
function message(id: number, sender: string | null, createdAt: string, extra: Partial<TimelineMessage> = {}): TimelineMessage {
    return {
        id,
        person_alias_guid: sender,
        sender_nick_name: sender === anna ? "Anna" : sender === ben ? "Ben" : "Jordan",
        sender_last_name: sender === anna ? "Reyes" : sender === ben ? "Ortiz" : "Lee",
        sender_listed: true,
        message_type: "text",
        body: `m${id}`,
        pieces: null,
        blocks: null,
        created_at: createdAt,
        edited_at: null,
        deleted_at: null,
        visibility: "visible",
        visibility_notice: null,
        ...extra
    };
}

function pendingRow(localId: string, status: "sending" | "failed" = "sending"): PendingMessage {
    return { localId, channelId: "c1", body: `pending ${localId}`, status, errorCode: status === "failed" ? "network" : null };
}

type LayoutOptions = Partial<Parameters<typeof layoutFeed>[0]>;

function lay(messages: TimelineMessage[], options: LayoutOptions = {}): FeedSection[] {
    return layoutFeed({
        messages,
        pending: [],
        personAliasGuid: me,
        newDividerBeforeId: null,
        layout: "conversational",
        now,
        locale,
        ...options
    }).sections;
}

/** Every item of every section, in order. */
function items(messages: TimelineMessage[], options: LayoutOptions = {}): FeedSection["items"] {
    return lay(messages, options).flatMap(section => section.items);
}

/** Each item as a short word: a group as its message ids, else its kind and id. */
function shape(messages: TimelineMessage[], options: LayoutOptions = {}): string[] {
    return items(messages, options).map(item => {
        if (item.kind === "group") {
            return item.rows.map(row => row.message ? String(row.message.id) : `p:${row.pending?.localId}`).join(",");
        }

        if (item.kind === "divider") {
            return "new";
        }

        return `${item.kind}:${item.message.id}`;
    });
}

/** Times as a person reads them; some ICU builds put a narrow no-break space before AM and PM. */
function plain(text: string): string {
    return text.replace(/\s+/g, " ");
}

describe("the rules' constants", () => {
    test("a group spans five minutes, Sent is held 2.2 seconds, the skeleton waits one second", () => {
        expect(groupWindowMs).toBe(5 * 60 * 1000);
        expect(sentHoldMs).toBe(2200);
        expect(skeletonDelayMs).toBe(1000);
    });
});

// A group's typing bubble is 70 px (its gap, name line and bubble), so a reader within it is at
// the newest message and the bubble's settling away never moves one counted as scrolled up.
test("the near-bottom band is 72 px, a group's typing bubble rounded up", () => {
    expect(nearBottomPx).toBe(72);
});

describe("grouping", () => {
    test("one sender's messages on one day within five minutes of each other are one group", () => {
        const list = [message(1, anna, at(9, 0)), message(2, anna, at(9, 2)), message(3, anna, at(9, 6, 59))];

        expect(shape(list)).toEqual(["1,2,3"]);
    });

    test("five minutes are measured from the message before, not from the group's first", () => {
        const list = [message(1, anna, at(9, 0)), message(2, anna, at(9, 4)), message(3, anna, at(9, 8))];

        expect(shape(list)).toEqual(["1,2,3"]);
    });

    test("a gap of more than five minutes starts a new group", () => {
        const list = [message(1, anna, at(9, 0)), message(2, anna, at(9, 5, 1))];

        expect(shape(list)).toEqual(["1", "2"]);
    });

    test("another sender starts a new group", () => {
        const list = [message(1, anna, at(9, 0)), message(2, ben, at(9, 1)), message(3, anna, at(9, 2))];

        expect(shape(list)).toEqual(["1", "2", "3"]);
    });

    test("a new day starts a new group, even a minute later", () => {
        const list = [message(1, anna, at(23, 59, 30, 1)), message(2, anna, at(0, 0, 30))];

        expect(shape(list)).toEqual(["1", "2"]);
    });

    test("the New divider breaks a group and sits between its halves", () => {
        const list = [message(1, anna, at(9, 0)), message(2, anna, at(9, 1)), message(3, anna, at(9, 2))];

        expect(shape(list, { newDividerBeforeId: 2 })).toEqual(["1", "new", "2,3"]);
    });

    test("a system line breaks a group and is drawn on its own", () => {
        const list = [
            message(1, anna, at(9, 0)),
            message(2, null, at(9, 1), { message_type: "system", body: "Grace Kim added Sam Ortiz to the conversation." }),
            message(3, anna, at(9, 2))
        ];

        expect(shape(list)).toEqual(["1", "system:2", "3"]);
    });

    test("a block post breaks a group and is drawn as a card", () => {
        const blocks = [{ type: "text", text: "Easter signups are open", fallback: { type: "text", text: "Easter signups are open" } }];
        const list = [
            message(1, anna, at(9, 0)),
            message(2, anna, at(9, 1), { message_type: "system", blocks }),
            message(3, anna, at(9, 2))
        ];

        expect(shape(list)).toEqual(["1", "card:2", "3"]);
    });

    test("in Conversational an announcement stays in its sender's group", () => {
        const list = [message(1, anna, at(9, 0)), message(2, anna, at(9, 1), { message_type: "announcement" }), message(3, anna, at(9, 2))];

        expect(shape(list)).toEqual(["1,2,3"]);
    });

    test("in Community an announcement always starts a group of its own, and the next message starts another", () => {
        const list = [message(1, anna, at(9, 0)), message(2, anna, at(9, 1), { message_type: "announcement" }), message(3, anna, at(9, 2))];

        expect(shape(list, { layout: "community" })).toEqual(["1", "2", "3"]);
    });

    test("in Community a quote starts a new group; in Conversational it does not", () => {
        const list = [message(1, anna, at(9, 0)), message(2, anna, at(9, 1), { quoted_message_id: 1 })];

        expect(shape(list, { layout: "community" })).toEqual(["1", "2"]);
        expect(shape(list)).toEqual(["1,2"]);
    });

    test("a deleted or hidden message keeps its place in its sender's group", () => {
        const list = [
            message(1, anna, at(9, 0)),
            message(2, anna, at(9, 1), { deleted_at: at(9, 3), body: null }),
            message(3, anna, at(9, 2), { visibility: "hidden", visibility_notice: "Hidden by a moderator.", body: null })
        ];

        expect(shape(list)).toEqual(["1,2,3"]);
    });

    test("each row knows its place in its group, which decides its corners, name and avatar", () => {
        const list = [message(1, anna, at(9, 0)), message(2, anna, at(9, 1)), message(3, anna, at(9, 2)), message(4, ben, at(9, 3))];
        const groups = items(list).filter(item => item.kind === "group");

        expect(groups.map(group => group.kind === "group" ? group.rows.map(row => row.position) : [])).toEqual([["first", "middle", "last"], ["single"]]);
    });

    test("a group knows whether it is the reader's own", () => {
        const groups = items([message(1, me, at(9, 0)), message(2, anna, at(9, 1))]);

        expect(groups.map(group => group.kind === "group" && group.isOwn)).toEqual([true, false]);
    });
});

describe("pending and failed sends", () => {
    test("are rows of the timeline after the newest stored message, in the reader's own group", () => {
        const list = [message(1, anna, at(14, 50))];
        const laid = items(list, { pending: [pendingRow("a"), pendingRow("b", "failed")] });

        expect(shape(list, { pending: [pendingRow("a"), pendingRow("b", "failed")] })).toEqual(["1", "p:a,p:b"]);
        const own = laid[1];
        expect(own.kind === "group" && own.isOwn).toBe(true);
    });

    test("join the reader's last group when it is today and within five minutes", () => {
        const list = [message(1, me, at(14, 58))];

        expect(shape(list, { pending: [pendingRow("a")] })).toEqual(["1,p:a"]);
    });

    test("sit in today's section even when the newest stored message is older", () => {
        const list = [message(1, anna, at(9, 0, 0, 1))];
        const sections = lay(list, { pending: [pendingRow("a")] });

        expect(sections.map(section => section.label)).toEqual(["Yesterday", "Today"]);
        expect(sections[1].items).toHaveLength(1);
    });

    // A message is keyed by its send's key whenever it carries one, and the pending row of that send
    // by the same key, so whichever copy reaches the page the browser patches the one element in
    // place rather than drawing a new one: a new element would cut its rise short and play it again.
    test("a sent message has the key its pending row had, as a row and as the head of its group", () => {
        const pendingKeys = lay([message(1, anna, at(14, 50))], { pending: [pendingRow("a")] })
            .flatMap(section => section.items)
            .map(item => item.kind === "group" ? [item.key, ...item.rows.map(row => row.key)] : [item.key]);

        const confirmed = lay([message(1, anna, at(14, 50)), message(2, me, at(15, 0), { client_key: "a" })])
            .flatMap(section => section.items);
        const keys = confirmed.map(item => item.kind === "group" ? [item.key, ...item.rows.map(row => row.key)] : [item.key]);

        expect(keys).toEqual(pendingKeys);
        expect(keys[1]).toEqual(["k:a", "k:a"]);
    });

    // A key read back from the platform may come in either case; it is still the pending row's key.
    test("a message whose key differs from its pending row's only in case is the same row", () => {
        const rows = lay([message(1, anna, at(14, 50)), message(2, me, at(15, 0), { client_key: "A" })], { pending: [pendingRow("a")] })
            .flatMap(section => section.items)
            .flatMap(item => item.kind === "group" ? item.rows : []);

        expect(rows.map(row => [row.key, row.message?.id ?? null, row.pending?.localId ?? null])).toEqual([
            ["k:1", 1, null],
            ["k:a", 2, null]
        ]);
    });

    test("a message with no send key, a system line or one sent before keys existed, is keyed by its id", () => {
        const keys = items([message(1, anna, at(14, 50)), message(2, me, at(15, 0), { client_key: null })])
            .map(item => item.key);

        expect(keys).toEqual(["k:1", "k:2"]);
    });

    // The live copy, a page or the answer can each bring the message while its row is still out;
    // the message stands for the send from then on, so the row is never drawn beside it.
    test("a pending row whose key is already among the messages is not drawn", () => {
        const rows = lay([message(1, anna, at(14, 50)), message(2, me, at(15, 0), { client_key: "a" })], { pending: [pendingRow("a"), pendingRow("b")] })
            .flatMap(section => section.items)
            .flatMap(item => item.kind === "group" ? item.rows : []);

        expect(rows.map(row => [row.key, row.message?.id ?? null, row.pending?.localId ?? null])).toEqual([
            ["k:1", 1, null],
            ["k:a", 2, null],
            ["k:b", null, "b"]
        ]);
    });
});

describe("day sections", () => {
    test("each day is its own section, oldest first, so only that day's pill sticks", () => {
        const list = [message(1, anna, at(9, 0, 0, 2)), message(2, anna, at(9, 0, 0, 1)), message(3, ben, at(9, 0, 0, 1)), message(4, anna, at(9, 0))];
        const sections = lay(list);

        expect(sections.map(section => section.label)).toEqual(["Saturday", "Yesterday", "Today"]);
        expect(sections.map(section => section.items.length)).toEqual([1, 2, 1]);
    });

    test("no messages, no sections", () => {
        expect(lay([])).toEqual([]);
    });

    test.each([
        ["today", new Date(2026, 9, 12, 0, 1), "Today"],
        ["yesterday", new Date(2026, 9, 11, 23, 59), "Yesterday"],
        ["two days ago", new Date(2026, 9, 10, 12, 0), "Saturday"],
        ["six days ago", new Date(2026, 9, 6, 12, 0), "Tuesday"],
        ["seven days ago, this year", new Date(2026, 9, 5, 12, 0), "October 5"],
        ["earlier this year", new Date(2026, 0, 2, 12, 0), "January 2"],
        ["a year before", new Date(2025, 9, 3, 12, 0), "October 3, 2025"]
    ])("a day %s is labelled %s", (_when, day, label) => {
        expect(dayLabel(day, now, locale)).toBe(label);
    });
});

describe("the words", () => {
    test("a time reads as the hour and minute", () => {
        expect(plain(timeLabel(new Date(2026, 9, 12, 9, 2), locale))).toBe("9:02 AM");
    });

    test.each([
        [1, "1 new message"],
        [7, "7 new messages"],
        [99, "99 new messages"],
        [100, "99+ new messages"]
    ])("%i unread reads %s", (count, text) => {
        expect(newMessagesText(count)).toBe(text);
    });

    test("the unread strip says how many arrived since when", () => {
        const since = new Date(2026, 9, 12, 11, 30);

        expect(plain(unreadStripText(7, since, locale))).toBe("7 new messages since 11:30 AM");
        expect(plain(unreadStripText(1, since, locale))).toBe("1 new message since 11:30 AM");
        expect(plain(unreadStripText(100, since, locale))).toBe("99+ new messages since 11:30 AM");
    });

    test("Jump to latest counts what arrived while the reader was scrolled up", () => {
        expect(jumpLabel(0)).toBe("Jump to latest");
        expect(jumpLabel(1)).toBe("1 new message");
        expect(jumpLabel(3)).toBe("3 new messages");
        expect(jumpLabel(150)).toBe("99+ new messages");
    });
});

describe("messages by people the reader blocked", () => {
    const blocked = (id: number, sender: string, extra: Partial<TimelineMessage> = {}): TimelineMessage =>
        message(id, sender, at(9, id), { body: null, sender_nick_name: null, sender_last_name: null, blocked: true, ...extra });

    function items(messages: TimelineMessage[], newDividerBeforeId: number | null = null): Array<{ kind: string, ids: number[] }> {
        return layoutFeed({ messages, pending: [], personAliasGuid: me, newDividerBeforeId, layout: "community", now, locale }).sections[0].items
            .map(item => ({ kind: item.kind, ids: item.kind === "blocked" ? item.messages.map(m => m.id) : item.kind === "group" ? item.rows.map(r => r.message?.id ?? 0) : item.kind === "divider" ? [] : [item.message.id] }));
    }

    test("a run of them folds into one line between the groups either side, keyed by its first message", () => {
        const sections = layoutFeed({ messages: [message(1, anna, at(9, 0)), blocked(2, ben), blocked(3, ben), message(4, anna, at(9, 5))], pending: [], personAliasGuid: me, newDividerBeforeId: null, layout: "community", now, locale }).sections;

        expect(sections[0].items.map(item => item.kind)).toEqual(["group", "blocked", "group"]);
        const grown = layoutFeed({ messages: [message(1, anna, at(9, 0)), blocked(2, ben)], pending: [], personAliasGuid: me, newDividerBeforeId: null, layout: "community", now, locale }).sections;
        expect(sections[0].items[1].key).toBe(grown[0].items[1].key);
        expect(items([message(1, anna, at(9, 0)), blocked(2, ben), blocked(3, ben), message(4, anna, at(9, 5))])[1]).toEqual({ kind: "blocked", ids: [2, 3] });
    });

    test("the New divider parts a run, so the line never hides where new messages start", () => {
        expect(items([blocked(2, ben), blocked(3, ben)], 3).map(i => i.kind)).toEqual(["blocked", "divider", "blocked"]);
    });

    test("a root with replies stands alone, so its thread can still be opened from under it", () => {
        expect(items([blocked(2, ben), blocked(3, ben, { reply_count: 2 }), blocked(4, ben)]).map(i => i.ids)).toEqual([[2], [3], [4]]);
    });

    test("the line says how many and whether from one person or several, never who", () => {
        expect(blockedRunText([blocked(2, ben)])).toBe("Message from someone you blocked");
        expect(blockedRunText([blocked(2, ben), blocked(3, ben.toUpperCase())])).toBe("2 messages from someone you blocked");
        expect(blockedRunText([blocked(2, ben), blocked(3, anna)])).toBe("2 messages from people you blocked");
    });
});

// Someone else typing is laid out as the typist's next row, by the rule her message will be: it
// joins her run when she sent the newest group today within five minutes, and otherwise starts a
// group of its own. Joining, it takes the run's face and corners from the row above, so her
// message lands where it stood; the time stays under her newest message, the typing row having none.
describe("someone else typing", () => {
    /** The rows of the newest group, each as its position and whether typing follows it. */
    function lastRun(messages: TimelineMessage[], options: LayoutOptions = {}): Array<[string, boolean]> {
        const groups = items(messages, options).filter(item => item.kind === "group");
        const last = groups[groups.length - 1];
        return last?.kind === "group" ? last.rows.map(row => [row.position, row.isTypingNext]) : [];
    }

    function typingOf(messages: TimelineMessage[], options: LayoutOptions = {}): ReturnType<typeof layoutFeed>["typing"] {
        return layoutFeed({ messages, pending: [], personAliasGuid: me, newDividerBeforeId: null, layout: "conversational", now, locale, ...options }).typing;
    }

    test("with no one typing there is no typing row and the rows are as they were", () => {
        const list = [message(1, anna, at(14, 58))];

        expect(typingOf(list)).toBeNull();
        expect(typingOf(list, { typists: [] })).toBeNull();
        expect(lastRun(list)).toEqual([["single", false]]);
    });

    test("the one typist who sent the newest group within five minutes is her run's next row", () => {
        const list = [message(1, ben, at(14, 50)), message(2, anna, at(14, 57)), message(3, anna, at(14, 58))];

        expect(typingOf(list, { typists: [anna] })).toEqual({ isInRun: true, endsAt: new Date(2026, 9, 12, 15, 3, 0, 1).getTime() });
        expect(lastRun(list, { typists: [anna] })).toEqual([["first", false], ["middle", true]]);
    });

    test("a run of one becomes the run's head, keeping its name line, and gives its face to the typing row", () => {
        expect(lastRun([message(1, anna, at(14, 58))], { typists: [anna] })).toEqual([["first", true]]);
    });

    test("is a group of its own when someone else sent the newest group, and leaves that group as it was", () => {
        const list = [message(1, anna, at(14, 57)), message(2, ben, at(14, 58))];

        expect(typingOf(list, { typists: [anna] })).toEqual({ isInRun: false, endsAt: null });
        expect(lastRun(list, { typists: [anna] })).toEqual([["single", false]]);
    });

    test("is a group of its own after the reader's own sends, a stored one or one still pending", () => {
        expect(typingOf([message(1, anna, at(14, 57)), message(2, me, at(14, 58))], { typists: [anna] })).toEqual({ isInRun: false, endsAt: null });
        expect(typingOf([message(1, anna, at(14, 58))], { typists: [anna], pending: [pendingRow("a")] })).toEqual({ isInRun: false, endsAt: null });
    });

    test("is a group of its own past five minutes, on a day after her run, or after a system line or a block post", () => {
        expect(typingOf([message(1, anna, at(14, 54, 59))], { typists: [anna] })).toEqual({ isInRun: false, endsAt: null });
        expect(typingOf([message(1, anna, at(23, 59, 0, 1))], { typists: [anna], now: new Date(2026, 9, 12, 0, 1) })).toEqual({ isInRun: false, endsAt: null });
        expect(typingOf([message(1, anna, at(14, 58)), message(2, ben, at(14, 59), { message_type: "system" })], { typists: [anna] })).toEqual({ isInRun: false, endsAt: null });
        expect(typingOf([message(1, anna, at(14, 58), { blocks: [{ type: "text" }] as unknown as TimelineMessage["blocks"] })], { typists: [anna] })).toEqual({ isInRun: false, endsAt: null });
    });

    // The layout reads the clock only as its inputs change, so it says when the run stops holding
    // the typing row: the first moment past the five minutes, or the day's end if that is sooner.
    test("in her run it says when the run ends: five minutes after her message, or at midnight if sooner", () => {
        expect(typingOf([message(1, anna, at(14, 58))], { typists: [anna] })?.endsAt).toBe(new Date(2026, 9, 12, 15, 3, 0, 1).getTime());
        expect(typingOf([message(1, anna, at(23, 57))], { typists: [anna], now: new Date(2026, 9, 12, 23, 58) })?.endsAt).toBe(new Date(2026, 9, 13).getTime());
    });

    test("two typing are a group of their own, even when one of them sent the newest group", () => {
        const list = [message(1, anna, at(14, 58))];

        expect(typingOf(list, { typists: [anna, ben] })).toEqual({ isInRun: false, endsAt: null });
        expect(lastRun(list, { typists: [anna, ben] })).toEqual([["single", false]]);
    });

    test("her run is found under the alias in any case", () => {
        expect(typingOf([message(1, anna.toUpperCase(), at(14, 58))], { typists: [anna] })).toEqual({ isInRun: true, endsAt: new Date(2026, 9, 12, 15, 3, 0, 1).getTime() });
    });

    test("with no messages at all it is a group of its own", () => {
        expect(typingOf([], { typists: [anna] })).toEqual({ isInRun: false, endsAt: null });
    });
});
