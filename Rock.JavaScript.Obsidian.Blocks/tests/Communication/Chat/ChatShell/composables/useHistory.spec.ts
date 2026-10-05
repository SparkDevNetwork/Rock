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
// A channel's timeline. History and the live join start together, so live events can arrive
// before the first page, and a message committed between the page's read and the join reaches
// neither; the newest page is fetched again on every confirmed join and merged by id.
import {
    createTimelines,
    firstPageSize,
    messageDisplay
} from "../../../../../src/Communication/Chat/ChatShell/composables/useHistory.partial";
import { HistoryPage, TimelineMessage } from "../../../../../src/Communication/Chat/ChatShell/types.partial";

const channel = "c0000001-0000-4000-8000-000000000000";

function message(id: number, body = `m${id}`): TimelineMessage {
    return {
        id,
        person_alias_guid: "a0000001-0000-4000-8000-000000000000",
        sender_nick_name: "Ada",
        message_type: "text",
        body,
        created_at: `2026-09-23T10:00:${String(id % 60).padStart(2, "0")}Z`
    };
}

function page(ids: number[], extra: Partial<HistoryPage> = {}): HistoryPage {
    // The platform returns the newest page newest first.
    return { messages: [...ids].sort((a, b) => b - a).map(id => message(id)), read_cursor: null, unread_count: 0, has_more: false, ...extra };
}

type Deferred<T> = { promise: Promise<T>, resolve: (value: T) => void };

function deferred<T>(): Deferred<T> {
    let resolve!: (value: T) => void;
    const promise = new Promise<T>(r => resolve = r);
    return { promise, resolve };
}

function ids(messages: TimelineMessage[]): number[] {
    return messages.map(m => m.id);
}

describe("createTimelines", () => {
    test("the first page is held oldest first with the read position and unread count", async () => {
        const fetches: { limit: number, beforeId?: number }[] = [];
        const timelines = createTimelines({
            fetchPage: async (_c, options) => {
                fetches.push(options);
                return page([3, 1, 2], { read_cursor: 2, unread_count: 1, has_more: true });
            }
        });

        await timelines.loadNewest(channel);

        const state = timelines.state(channel);
        expect(fetches).toEqual([{ limit: firstPageSize }]);
        expect(ids(state.messages)).toEqual([1, 2, 3]);
        expect(state.isLoaded).toBe(true);
        expect(state.readCursor).toBe(2);
        expect(state.unreadCount).toBe(1);
        expect(state.hasMore).toBe(true);
    });

    test("a live message before the first page is held, then applied after it, once", async () => {
        const first = deferred<HistoryPage>();
        const timelines = createTimelines({ fetchPage: () => first.promise });

        const loading = timelines.loadNewest(channel);
        timelines.applyEvent(channel, "message.created", { ...message(5), channel_id: channel, parent_id: null, shown_in_channel: false });
        timelines.applyEvent(channel, "message.created", { ...message(4), channel_id: channel, parent_id: null, shown_in_channel: false });
        expect(timelines.state(channel).messages).toEqual([]);

        first.resolve(page([3, 4]));
        await loading;

        expect(ids(timelines.state(channel).messages)).toEqual([3, 4, 5]);
    });

    test("a confirmed join fetches what is newer than the newest held and fills the gap by id", async () => {
        const answers = [page([1, 2, 3]), page([4, 5])];
        const fetches: { limit: number, afterId?: number }[] = [];
        const timelines = createTimelines({
            fetchPage: async (_c, options) => {
                fetches.push(options);
                return answers.shift() as HistoryPage;
            }
        });

        await timelines.loadNewest(channel);
        await timelines.onJoined(channel);

        expect(fetches).toEqual([{ limit: firstPageSize }, { limit: 100, afterId: 3 }]);
        expect(ids(timelines.state(channel).messages)).toEqual([1, 2, 3, 4, 5]);
    });

    test("a rejoin after a drop fetches again too", async () => {
        const answers = [page([1]), page([1, 2]), page([1, 2, 3])];
        const timelines = createTimelines({ fetchPage: async () => answers.shift() as HistoryPage });

        await timelines.loadNewest(channel);
        await timelines.onJoined(channel);
        await timelines.onJoined(channel);

        expect(ids(timelines.state(channel).messages)).toEqual([1, 2, 3]);
    });

    test("a join confirmed before the first page arrives still merges cleanly", async () => {
        const first = deferred<HistoryPage>();
        const calls: number[] = [];
        const timelines = createTimelines({
            fetchPage: (_c, options) => {
                calls.push(options.limit);
                return options.limit === firstPageSize ? first.promise : Promise.resolve(page([4, 5]));
            }
        });

        const loading = timelines.loadNewest(channel);
        await timelines.onJoined(channel);
        first.resolve(page([1, 2, 3, 4]));
        await loading;

        expect(ids(timelines.state(channel).messages)).toEqual([1, 2, 3, 4, 5]);
        expect(timelines.state(channel).isLoaded).toBe(true);
    });

    test("an edit and a delete that arrived live are not undone by a page read before them", async () => {
        const answers = [page([1, 2]), page([1, 2])];
        const timelines = createTimelines({ fetchPage: async () => answers.shift() as HistoryPage });
        await timelines.loadNewest(channel);

        timelines.applyEvent(channel, "message.edited", { id: 1, channel_id: channel, body: "changed", edited_at: "2026-09-23T10:05:00Z" });
        timelines.applyEvent(channel, "message.deleted", { id: 2, channel_id: channel, deleted_at: "2026-09-23T10:06:00Z" });
        await timelines.onJoined(channel);

        const [one, two] = timelines.state(channel).messages;
        expect(one.body).toBe("changed");
        expect(two.deleted_at).toBe("2026-09-23T10:06:00Z");
        expect(two.body).toBeNull();
    });

    test("a live event for a message not held is ignored for an edit and a delete", async () => {
        const timelines = createTimelines({ fetchPage: async () => page([1]) });
        await timelines.loadNewest(channel);

        timelines.applyEvent(channel, "message.edited", { id: 9, channel_id: channel, body: "x", edited_at: "2026-09-23T10:05:00Z" });
        timelines.applyEvent(channel, "message.deleted", { id: 9, channel_id: channel, deleted_at: "2026-09-23T10:05:00Z" });

        expect(ids(timelines.state(channel).messages)).toEqual([1]);
    });

    test("an event name it does not know is ignored", async () => {
        const timelines = createTimelines({ fetchPage: async () => page([1]) });
        await timelines.loadNewest(channel);

        timelines.applyEvent(channel, "message.reacted", { id: 1 });

        expect(ids(timelines.state(channel).messages)).toEqual([1]);
    });

    test("an older page is fetched before the oldest held and merged below it", async () => {
        const fetches: { limit: number, beforeId?: number }[] = [];
        const answers = [page([4, 5], { has_more: true }), page([2, 3], { has_more: false })];
        const timelines = createTimelines({
            fetchPage: async (_c, options) => {
                fetches.push(options);
                return answers.shift() as HistoryPage;
            }
        });

        await timelines.loadNewest(channel);
        await timelines.loadOlder(channel);

        expect(fetches[1]).toEqual({ limit: firstPageSize, beforeId: 4 });
        expect(ids(timelines.state(channel).messages)).toEqual([2, 3, 4, 5]);
        expect(timelines.state(channel).hasMore).toBe(false);
    });

    test("upsert keeps one row per id", async () => {
        const timelines = createTimelines({ fetchPage: async () => page([1]) });
        await timelines.loadNewest(channel);

        timelines.upsert(channel, message(2, "mine"));
        timelines.applyEvent(channel, "message.created", { ...message(2, "mine"), channel_id: channel, parent_id: null, shown_in_channel: false });

        expect(ids(timelines.state(channel).messages)).toEqual([1, 2]);
    });

    test("a live message takes its sender's name and photo from that person's earlier messages", async () => {
        const timelines = createTimelines({
            fetchPage: async () => ({
                messages: [{ ...message(1), sender_last_name: "Lovelace", sender_avatar_url: "/ada.png", sender_listed: true }],
                read_cursor: null, unread_count: 0, has_more: false
            })
        });
        await timelines.loadNewest(channel);

        // The live copy carries no name: the channel topic sends only what the message row holds.
        timelines.applyEvent(channel, "message.created", {
            id: 2, channel_id: channel, parent_id: null, shown_in_channel: false,
            person_alias_guid: "a0000001-0000-4000-8000-000000000000", message_type: "text", body: "hi", created_at: "2026-09-23T10:01:00Z"
        });
        timelines.applyEvent(channel, "message.created", {
            id: 3, channel_id: channel, parent_id: null, shown_in_channel: false,
            person_alias_guid: "a0000009-0000-4000-8000-000000000000", message_type: "text", body: "new here", created_at: "2026-09-23T10:02:00Z"
        });

        const [, second, third] = timelines.state(channel).messages;
        expect(second.sender_nick_name).toBe("Ada");
        expect(second.sender_last_name).toBe("Lovelace");
        expect(second.sender_avatar_url).toBe("/ada.png");
        expect(second.sender_listed).toBe(true);
        expect(third.sender_nick_name).toBeUndefined();
    });

    test("a thread-only reply on the channel topic is not a timeline message", async () => {
        const timelines = createTimelines({ fetchPage: async () => page([1]) });
        await timelines.loadNewest(channel);

        timelines.applyEvent(channel, "message.created", { ...message(2), channel_id: channel, parent_id: 1, shown_in_channel: false });
        timelines.applyEvent(channel, "message.created", { ...message(3), channel_id: channel, parent_id: 1, shown_in_channel: true });

        expect(ids(timelines.state(channel).messages)).toEqual([1, 3]);
    });
});

describe("catching up after a rejoin", () => {
    // A live copy can be lost, and a socket can be gone for hours while a laptop sleeps, so a
    // rejoin pages forward from the newest message held until nothing newer is left. Past the
    // bound, the channel is opened fresh at its newest page instead, as opening it does.
    type CatchUpDependencies = Parameters<typeof createTimelines>[0] & { catchUp: () => { page: number, max: number } };
    type Fetch = { limit: number, beforeId?: number, afterId?: number };

    function history(newest: number): (options: Fetch) => HistoryPage {
        return options => {
            if (options.afterId !== undefined) {
                const from = options.afterId + 1;
                const to = Math.min(newest, options.afterId + options.limit);
                const found = Array.from({ length: Math.max(0, to - from + 1) }, (_, i) => from + i);
                return { messages: found.map(id => message(id)), read_cursor: null, unread_count: 0, has_more: to < newest };
            }
            const from = Math.max(1, newest - options.limit + 1);
            return page(Array.from({ length: newest - from + 1 }, (_, i) => from + i), { has_more: from > 1 });
        };
    }

    test("a rejoin pages forward with after until caught up, filling a gap of 60", async () => {
        let newest = 3;
        const fetches: Fetch[] = [];
        const dependencies: CatchUpDependencies = {
            fetchPage: async (_c, options) => {
                fetches.push(options as Fetch);
                return history(newest)(options as Fetch);
            },
            catchUp: () => ({ page: 25, max: 500 })
        };
        const timelines = createTimelines(dependencies);
        await timelines.loadNewest(channel);

        newest = 63;
        await timelines.onJoined(channel);

        expect(fetches.slice(1)).toEqual([
            { limit: 25, afterId: 3 },
            { limit: 25, afterId: 28 },
            { limit: 25, afterId: 53 }
        ]);
        expect(ids(timelines.state(channel).messages)).toEqual(Array.from({ length: 63 }, (_, i) => i + 1));
    });

    test("a person's own send while the socket was down does not hide the messages others sent before it", async () => {
        let newest = 100;
        const fetches: Fetch[] = [];
        const dependencies: CatchUpDependencies = {
            fetchPage: async (_c, options) => {
                fetches.push(options as Fetch);
                return history(newest)(options as Fetch);
            },
            catchUp: () => ({ page: 25, max: 500 })
        };
        const timelines = createTimelines(dependencies);
        await timelines.loadNewest(channel);

        // The socket drops; others post 101 to 105, which no live copy brings. The person's own
        // send still goes through over HTTP and its confirmation puts 106 on screen.
        newest = 106;
        timelines.upsert(channel, message(106, "my own"));

        await timelines.onJoined(channel);

        expect(fetches[fetches.length - 1]).toEqual({ limit: 25, afterId: 100 });
        expect(ids(timelines.state(channel).messages).slice(-6)).toEqual([101, 102, 103, 104, 105, 106]);
    });

    test("past the bound it drops what it held and opens the channel fresh at the newest page", async () => {
        let newest = 3;
        const fetches: Fetch[] = [];
        const dependencies: CatchUpDependencies = {
            fetchPage: async (_c, options) => {
                fetches.push(options as Fetch);
                return history(newest)(options as Fetch);
            },
            catchUp: () => ({ page: 25, max: 50 })
        };
        const timelines = createTimelines(dependencies);
        await timelines.loadNewest(channel);

        newest = 1000;
        await timelines.onJoined(channel);

        const state = timelines.state(channel);
        expect(fetches.slice(1)).toEqual([
            { limit: 25, afterId: 3 },
            { limit: 25, afterId: 28 },
            { limit: firstPageSize }
        ]);
        expect(ids(state.messages)).toEqual(Array.from({ length: firstPageSize }, (_, i) => 1000 - firstPageSize + 1 + i));
        expect(state.hasMore).toBe(true);
    });

    // What is on screen is always one run of messages with nothing missing in the middle: either
    // everything back to the first message, or a run whose older end says there is more to load.
    function expectNoHole(state: { messages: TimelineMessage[], hasMore: boolean }, newest: number): void {
        const held = ids(state.messages);
        expect(held[held.length - 1]).toBe(newest);
        expect(held).toEqual(Array.from({ length: held.length }, (_, i) => held[0] + i));
        expect(held[0] === 1 || state.hasMore).toBe(true);
    }

    test("reopening a channel after 60 new messages leaves no hole in what it holds", async () => {
        let newest = 3;
        const dependencies: CatchUpDependencies = {
            fetchPage: async (_c, options) => history(newest)(options as Fetch),
            catchUp: () => ({ page: 25, max: 500 })
        };
        const timelines = createTimelines(dependencies);
        await timelines.loadNewest(channel);

        // the person left the channel; 60 arrive; opening it again reads its newest page
        newest = 63;
        await timelines.loadNewest(channel);
        await timelines.onJoined(channel);

        expectNoHole(timelines.state(channel), 63);
    });

    test("a resync after 70 messages it never heard live leaves no hole either", async () => {
        let newest = 3;
        const dependencies: CatchUpDependencies = {
            fetchPage: async (_c, options) => history(newest)(options as Fetch),
            catchUp: () => ({ page: 25, max: 500 })
        };
        const timelines = createTimelines(dependencies);
        await timelines.loadNewest(channel);
        newest = 5;
        timelines.applyEvent(channel, "message.created", { ...message(4) });
        timelines.applyEvent(channel, "message.created", { ...message(5) });

        // the server asks for a resync, which reads the newest page again
        newest = 75;
        await timelines.loadNewest(channel);
        await timelines.onJoined(channel);

        expectNoHole(timelines.state(channel), 75);
    });

    test("a channel whose first page was empty still reaches every message a rejoin finds", async () => {
        let newest = 0;
        const dependencies: CatchUpDependencies = {
            fetchPage: async (_c, options) => history(newest)(options as Fetch),
            catchUp: () => ({ page: 25, max: 500 })
        };
        const timelines = createTimelines(dependencies);
        await timelines.loadNewest(channel);

        newest = 80;
        await timelines.onJoined(channel);

        expectNoHole(timelines.state(channel), 80);
    });
});

describe("what the server adds later", () => {
    test("a live message keeps every field its payload carries, including ones this client does not know", async () => {
        const timelines = createTimelines({ fetchPage: async () => page([1]) });
        await timelines.loadNewest(channel);

        timelines.applyEvent(channel, "message.created", {
            id: 2, channel_id: channel, parent_id: null, shown_in_channel: false,
            person_alias_guid: "a0000001-0000-4000-8000-000000000000", message_type: "text", body: "hi", created_at: "2026-09-23T10:01:00Z",
            pieces: [{ kind: "text", text: "hi" }]
        });

        const [, second] = timelines.state(channel).messages;
        expect((second as Record<string, unknown>).pieces).toEqual([{ kind: "text", text: "hi" }]);
        expect(second.sender_nick_name).toBe("Ada");
    });

    test("a message type this client does not know is shown as its text", () => {
        expect(messageDisplay({ ...message(1, "Lunch on Sunday?"), message_type: "poll" })).toEqual({ kind: "message", text: "Lunch on Sunday?" });
        expect(messageDisplay({ ...message(2, "Ada joined"), message_type: "system" })).toEqual({ kind: "system", text: "Ada joined" });
        expect(messageDisplay({ ...message(3), deleted_at: "2026-09-23T10:06:00Z", body: null })).toEqual({ kind: "deleted", text: "This message was deleted." });
    });

    test("a message hidden live shows the server's sentence in place of its body", async () => {
        const timelines = createTimelines({ fetchPage: async () => page([1, 2]) });
        await timelines.loadNewest(channel);

        timelines.applyEvent(channel, "message.visibility", { id: 2, channel_id: channel, state: "hidden", notice: "This message is under review." });

        const [, two] = timelines.state(channel).messages;
        expect(two.body).toBeNull();
        expect(messageDisplay(two)).toEqual({ kind: "notice", text: "This message is under review." });
    });

    test("a message hidden live and then shown again live draws its body again", async () => {
        const timelines = createTimelines({ fetchPage: async () => page([1, 2]) });
        await timelines.loadNewest(channel);

        timelines.applyEvent(channel, "message.visibility", { id: 2, channel_id: channel, state: "hidden", notice: "Under review." });
        timelines.applyEvent(channel, "message.visibility", { id: 2, channel_id: channel, state: "visible", notice: null, body: "m2" });

        const [, two] = timelines.state(channel).messages;
        expect(messageDisplay(two)).toEqual({ kind: "message", text: "m2" });
    });

    test("a message history returns as removed shows the server's sentence, and one shown again shows its body", async () => {
        const removed = { ...message(1), body: null, visibility: "removed", visibility_notice: "Removed by a moderator." } as TimelineMessage;
        const answers = [
            { messages: [removed], read_cursor: null, unread_count: 0, has_more: false },
            { messages: [{ ...message(1, "back again"), visibility: "visible", visibility_notice: null } as TimelineMessage], read_cursor: null, unread_count: 0, has_more: false }
        ];
        const timelines = createTimelines({ fetchPage: async () => answers.shift() as HistoryPage });
        await timelines.loadNewest(channel);

        expect(messageDisplay(timelines.state(channel).messages[0])).toEqual({ kind: "notice", text: "Removed by a moderator." });

        await timelines.loadNewest(channel);
        expect(messageDisplay(timelines.state(channel).messages[0])).toEqual({ kind: "message", text: "back again" });
    });
});
