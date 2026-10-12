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
// where each day starts and what it is called, where the New divider sits, and the words of the
// unread strip and Jump to latest. These are pure rules, read without mounting, so each layout
// draws the same groups. The clock and the locale are fixed here, so a test never depends on the
// day it runs or the machine it runs on. Dates are built in local time, the time a reader sees.
import {
    dayLabel,
    groupWindowMs,
    jumpLabel,
    layoutFeed,
    newDividerBeforeId,
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

function lay(messages: TimelineMessage[], options: LayoutOptions = {}): ReturnType<typeof layoutFeed> {
    return layoutFeed({
        messages,
        pending: [],
        personAliasGuid: me,
        newDividerBeforeId: null,
        layout: "conversational",
        now,
        locale,
        ...options
    });
}

/** Every item of every section, in order. */
function items(messages: TimelineMessage[], options: LayoutOptions = {}): ReturnType<typeof layoutFeed>[number]["items"] {
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

describe("the New divider", () => {
    test("sits above the first message past the read position", () => {
        const list = [message(9, anna, at(9, 0)), message(10, anna, at(9, 1)), message(11, ben, at(9, 2)), message(12, anna, at(9, 3))];

        expect(newDividerBeforeId(list, 10, 2, me)).toBe(11);
    });

    test("skips a system line past the read position", () => {
        const list = [message(10, anna, at(9, 0)), message(11, null, at(9, 1), { message_type: "system" }), message(12, anna, at(9, 2))];

        expect(newDividerBeforeId(list, 10, 1, me)).toBe(12);
    });

    test("skips the reader's own message, which is never new to them", () => {
        const list = [message(10, anna, at(9, 0)), message(11, me, at(9, 1)), message(12, anna, at(9, 2))];

        expect(newDividerBeforeId(list, 10, 1, me)).toBe(12);
    });

    test("is not drawn with nothing unread or no read position", () => {
        const list = [message(10, anna, at(9, 0)), message(11, anna, at(9, 1))];

        expect(newDividerBeforeId(list, 10, 0, me)).toBeNull();
        expect(newDividerBeforeId(list, null, 3, me)).toBeNull();
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
