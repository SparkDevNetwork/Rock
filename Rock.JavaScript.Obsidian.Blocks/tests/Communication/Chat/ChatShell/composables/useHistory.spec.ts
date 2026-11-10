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
// neither; every confirmed join pages forward from what a fetched page proved is held.
import {
    createTimelines
} from "../../../../../src/Communication/Chat/ChatShell/composables/useHistory.partial";
import {
    applyMessageChange,
    holdMessage,
    keptAsStated,
    messageDisplay,
    newMessageState,
    platformMask
} from "../../../../../src/Communication/Chat/ChatShell/heldMessage.partial";
import { createBlocks } from "../../../../../src/Communication/Chat/ChatShell/composables/useBlocks.partial";
import { AttachmentItem, HistoryPage, TimelineMessage } from "../../../../../src/Communication/Chat/ChatShell/types.partial";
import { anna, channelId as shapesChannel, liveCreated, pageRow, sendAnswer } from "../messageShapes";
import { createThreads } from "../../../../../src/Communication/Chat/ChatShell/composables/useThreads.partial";

const channel = "c0000001-0000-4000-8000-000000000000";

/** A page of history as the platform's limits set it. */
const firstPageSize = 50;
const limits = (): { history_page_size: number } => ({ history_page_size: firstPageSize });

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
    return { messages: [...ids].sort((a, b) => b - a).map(id => message(id)), read_cursor: null, unread_count: 0, first_unread_id: null, has_more: false, ...extra };
}

type Deferred<T> = { promise: Promise<T>, resolve: (value: T) => void };

function deferred<T>(): Deferred<T> {
    let resolve!: (value: T) => void;
    const promise = new Promise<T>(r => resolve = r);
    return { promise, resolve };
}

/** A page of the platform's own rows, newest first as it answers. */
function pageOf(rows: TimelineMessage[]): HistoryPage {
    return { messages: [...rows].reverse(), read_cursor: null, unread_count: 0, first_unread_id: null, has_more: false, rev: 1 };
}

/** Lets every promise the stores started settle. */
async function flushPromisesOf(): Promise<void> {
    for (let i = 0; i < 10; i++) {
        await Promise.resolve();
    }
}

function ids(messages: TimelineMessage[]): number[] {
    return messages.map(m => m.id);
}

describe("createTimelines", () => {
    test("the first page is held oldest first with the read position and unread count", async () => {
        const fetches: { limit: number, beforeId?: number }[] = [];
        const timelines = createTimelines({
            mask: platformMask,
            limits,
            fetchPage: async (_c, options) => {
                fetches.push(options);
                return page([3, 1, 2], { read_cursor: 2, unread_count: 1, first_unread_id: 3, has_more: true });
            }
        });

        await timelines.loadNewest(channel);

        const state = timelines.state(channel);
        expect(fetches).toEqual([{ limit: firstPageSize }]);
        expect(ids(state.messages)).toEqual([1, 2, 3]);
        expect(state.isLoaded).toBe(true);
        expect(state.readCursor).toBe(2);
        expect(state.unreadCount).toBe(1);
        expect(state.firstUnreadId).toBe(3);
        expect(state.hasMore).toBe(true);
    });

    test("pages ask for the size the platform's limits set, read as each page is asked", async () => {
        const fetches: { limit: number, beforeId?: number }[] = [];
        let size = 7;
        const timelines = createTimelines({
            mask: platformMask,
            limits: () => ({ history_page_size: size }),
            fetchPage: async (_c, options) => {
                fetches.push(options);
                return page(options.beforeId ? [1] : [3, 2], { has_more: !options.beforeId });
            }
        });

        await timelines.loadNewest(channel);
        size = 9;
        await timelines.loadOlder(channel);

        expect(fetches).toEqual([{ limit: 7 }, { limit: 9, beforeId: 2 }]);
    });

    test("a live message before the first page is held, then applied after it, once", async () => {
        const first = deferred<HistoryPage>();
        const timelines = createTimelines({ mask: platformMask, limits, fetchPage: () => first.promise });

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
            mask: platformMask,
            limits,
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
        const timelines = createTimelines({ mask: platformMask, limits, fetchPage: async () => answers.shift() as HistoryPage });

        await timelines.loadNewest(channel);
        await timelines.onJoined(channel);
        await timelines.onJoined(channel);

        expect(ids(timelines.state(channel).messages)).toEqual([1, 2, 3]);
    });

    test("a join confirmed before the first page arrives still merges cleanly", async () => {
        const first = deferred<HistoryPage>();
        const calls: number[] = [];
        const timelines = createTimelines({
            mask: platformMask,
            limits,
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
        const timelines = createTimelines({ mask: platformMask, limits, fetchPage: async () => answers.shift() as HistoryPage });
        await timelines.loadNewest(channel);

        timelines.applyEvent(channel, "message.edited", { id: 1, channel_id: channel, body: "changed", edited_at: "2026-09-23T10:05:00Z" });
        timelines.applyEvent(channel, "message.deleted", { id: 2, channel_id: channel, deleted_at: "2026-09-23T10:06:00Z" });
        await timelines.onJoined(channel);

        const [one, two] = timelines.state(channel).messages;
        expect(one.body).toBe("changed");
        expect(two.deleted_at).toBe("2026-09-23T10:06:00Z");
        expect(two.body).toBeNull();
    });

    test("a thread's live counts, faces and unread mark are not undone by a page read before them", async () => {
        const anna = { person_alias_guid: "a0000002-0000-4000-8000-000000000000", nick_name: "Anna", last_name: "Reyes", avatar_url: null };
        const ben = { person_alias_guid: "a0000003-0000-4000-8000-000000000000", nick_name: "Ben", last_name: "Okafor", avatar_url: null };
        // the root as chat_get_history carries it, before Ben's reply
        const rootBefore = (): TimelineMessage => ({
            ...message(10), reply_count: 1, last_reply_at: "2026-09-23T10:01:00Z", participant_count: 1,
            repliers: [anna], thread_following: true, thread_unread: false
        });
        const rejoinPage = deferred<HistoryPage>();
        const answers: Array<HistoryPage | Deferred<HistoryPage>> = [
            { messages: [rootBefore()], read_cursor: null, unread_count: 0, has_more: false, rev: 1 },
            page([], { rev: 1 }),
            rejoinPage
        ];
        const timelines = createTimelines({
            mask: platformMask,
            limits,
            fetchPage: () => {
                const next = answers.shift() as HistoryPage | Deferred<HistoryPage>;
                return "promise" in next ? next.promise : Promise.resolve(next);
            }
        });
        await timelines.loadNewest(channel);
        await timelines.onJoined(channel);

        // a rejoin reads the root again; Ben replies while that page is on its way, and the
        // threads have marked the followed root unread before the timeline records the counts
        const rejoining = timelines.onJoined(channel);
        timelines.state(channel).messages[0].thread_unread = true;
        timelines.applyEvent(channel, "thread.updated", {
            channel_id: channel, root_id: 10, reply_count: 2, last_reply_at: "2026-09-23T10:05:00Z", participant_count: 2, repliers: [ben, anna], rev: 2
        });
        rejoinPage.resolve({ messages: [rootBefore()], read_cursor: null, unread_count: 0, has_more: false, rev: 1 });
        await rejoining;

        const held = timelines.state(channel).messages[0];
        expect(held.reply_count).toBe(2);
        expect(held.last_reply_at).toBe("2026-09-23T10:05:00Z");
        expect(held.participant_count).toBe(2);
        expect(held.repliers).toEqual([ben, anna]);
        expect(held.thread_unread).toBe(true);
    });

    test("a live event for a message not held is ignored for an edit and a delete", async () => {
        const timelines = createTimelines({ mask: platformMask, limits, fetchPage: async () => page([1]) });
        await timelines.loadNewest(channel);

        timelines.applyEvent(channel, "message.edited", { id: 9, channel_id: channel, body: "x", edited_at: "2026-09-23T10:05:00Z" });
        timelines.applyEvent(channel, "message.deleted", { id: 9, channel_id: channel, deleted_at: "2026-09-23T10:05:00Z" });

        expect(ids(timelines.state(channel).messages)).toEqual([1]);
    });

    test("an event name it does not know is ignored", async () => {
        const timelines = createTimelines({ mask: platformMask, limits, fetchPage: async () => page([1]) });
        await timelines.loadNewest(channel);

        timelines.applyEvent(channel, "message.reacted", { id: 1 });

        expect(ids(timelines.state(channel).messages)).toEqual([1]);
    });

    test("an older page is fetched before the oldest held and merged below it", async () => {
        const fetches: { limit: number, beforeId?: number }[] = [];
        const answers = [page([4, 5], { has_more: true }), page([2, 3], { has_more: false })];
        const timelines = createTimelines({
            mask: platformMask,
            limits,
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
        const timelines = createTimelines({ mask: platformMask, limits, fetchPage: async () => page([1]) });
        await timelines.loadNewest(channel);

        timelines.upsert(channel, message(2, "mine"));
        timelines.applyEvent(channel, "message.created", { ...message(2, "mine"), channel_id: channel, parent_id: null, shown_in_channel: false });

        expect(ids(timelines.state(channel).messages)).toEqual([1, 2]);
    });

    test("a live message takes its sender's name and photo from that person's earlier messages", async () => {
        const timelines = createTimelines({
            mask: platformMask,
            limits,
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

    test("a live message that names its sender draws that name and photo, a newcomer's included", async () => {
        const timelines = createTimelines({ mask: platformMask, limits, fetchPage: async () => page([1]) });
        await timelines.loadNewest(channel);

        timelines.applyEvent(channel, "message.created", {
            id: 3, channel_id: channel, parent_id: null, shown_in_channel: false,
            person_alias_guid: "a0000009-0000-4000-8000-000000000000", message_type: "text", body: "new here", created_at: "2026-09-23T10:02:00Z",
            sender_nick_name: "Nan", sender_last_name: "Newcomer", sender_avatar_url: "/nan.png", sender_listed: true
        });

        const [, live] = timelines.state(channel).messages;
        expect(live.sender_nick_name).toBe("Nan");
        expect(live.sender_last_name).toBe("Newcomer");
        expect(live.sender_avatar_url).toBe("/nan.png");
        expect(live.sender_listed).toBe(true);
    });

    test("a thread-only reply on the channel topic is not a timeline message", async () => {
        const timelines = createTimelines({ mask: platformMask, limits, fetchPage: async () => page([1]) });
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

    // What is on screen is always one run of messages with nothing missing in the middle: either
    // everything back to the first message, or a run whose older end says there is more to load.
    function expectNoHole(state: { messages: TimelineMessage[], hasMore: boolean }, newest: number): void {
        const held = ids(state.messages);
        expect(held[held.length - 1]).toBe(newest);
        expect(held).toEqual(Array.from({ length: held.length }, (_, i) => held[0] + i));
        expect(held[0] === 1 || state.hasMore).toBe(true);
    }

    test("a rejoin pages forward with after until caught up, filling a gap of 60", async () => {
        let newest = 3;
        const fetches: Fetch[] = [];
        const dependencies: CatchUpDependencies = {
            mask: platformMask,
            fetchPage: async (_c, options) => {
                fetches.push(options as Fetch);
                return history(newest)(options as Fetch);
            },
            catchUp: () => ({ page: 25, max: 500 })
        };
        const timelines = createTimelines({ ...dependencies, limits });
        await timelines.loadNewest(channel);

        newest = 63;
        await timelines.onJoined(channel);

        expect(fetches.slice(1)).toEqual([
            { limit: 25, afterId: 3 },
            { limit: 25, afterId: 28 },
            { limit: 25, afterId: 53 }
        ]);
        expect(ids(timelines.state(channel).messages)).toEqual(Array.from({ length: 63 }, (_, i) => i + 1));
        expectNoHole(timelines.state(channel), 63);
    });

    test("a person's own send while the socket was down does not hide the messages others sent before it", async () => {
        let newest = 100;
        const fetches: Fetch[] = [];
        const dependencies: CatchUpDependencies = {
            mask: platformMask,
            fetchPage: async (_c, options) => {
                fetches.push(options as Fetch);
                return history(newest)(options as Fetch);
            },
            catchUp: () => ({ page: 25, max: 500 })
        };
        const timelines = createTimelines({ ...dependencies, limits });
        await timelines.loadNewest(channel);

        // The socket drops; others post 101 to 105, which no live copy brings. The person's own
        // send still goes through over HTTP and its confirmation puts 106 on screen.
        newest = 106;
        timelines.upsert(channel, message(106, "my own"));

        await timelines.onJoined(channel);

        expect(fetches[fetches.length - 1]).toEqual({ limit: 25, afterId: 100 });
        expect(ids(timelines.state(channel).messages).slice(-6)).toEqual([101, 102, 103, 104, 105, 106]);
        expectNoHole(timelines.state(channel), 106);
    });

    test("past the bound it drops what it held and opens the channel fresh at the newest page", async () => {
        let newest = 3;
        const fetches: Fetch[] = [];
        const dependencies: CatchUpDependencies = {
            mask: platformMask,
            fetchPage: async (_c, options) => {
                fetches.push(options as Fetch);
                return history(newest)(options as Fetch);
            },
            catchUp: () => ({ page: 25, max: 50 })
        };
        const timelines = createTimelines({ ...dependencies, limits });
        await timelines.loadNewest(channel);

        newest = 1000;
        await timelines.onJoined(channel);

        const state = timelines.state(channel);
        // the fresh page is read before anything is let go, and paging goes on from it
        expect(fetches.slice(1)).toEqual([
            { limit: 25, afterId: 3 },
            { limit: 25, afterId: 28 },
            { limit: firstPageSize },
            { limit: 25, afterId: 1000 }
        ]);
        expect(ids(state.messages)).toEqual(Array.from({ length: firstPageSize }, (_, i) => 1000 - firstPageSize + 1 + i));
        expect(state.hasMore).toBe(true);
        expectNoHole(state, 1000);
    });

    test("loading older messages after a fresh reload keeps one run back to the first message", async () => {
        let newest = 3;
        const dependencies: CatchUpDependencies = {
            mask: platformMask,
            fetchPage: async (_c, options) => {
                const fetch = options as Fetch;
                if (fetch.beforeId !== undefined) {
                    const to = fetch.beforeId - 1;
                    const from = Math.max(1, to - fetch.limit + 1);
                    return page(Array.from({ length: to - from + 1 }, (_, i) => from + i), { has_more: from > 1 });
                }
                return history(newest)(fetch);
            },
            catchUp: () => ({ page: 25, max: 50 })
        };
        const timelines = createTimelines({ ...dependencies, limits });
        await timelines.loadNewest(channel);
        newest = 200;
        await timelines.onJoined(channel);

        while (timelines.state(channel).hasMore) {
            await timelines.loadOlder(channel);
            expectNoHole(timelines.state(channel), 200);
        }
        expect(ids(timelines.state(channel).messages)[0]).toBe(1);
    });

    test("reopening a channel after 60 new messages leaves no hole in what it holds", async () => {
        let newest = 3;
        const dependencies: CatchUpDependencies = {
            mask: platformMask,
            fetchPage: async (_c, options) => history(newest)(options as Fetch),
            catchUp: () => ({ page: 25, max: 500 })
        };
        const timelines = createTimelines({ ...dependencies, limits });
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
            mask: platformMask,
            fetchPage: async (_c, options) => history(newest)(options as Fetch),
            catchUp: () => ({ page: 25, max: 500 })
        };
        const timelines = createTimelines({ ...dependencies, limits });
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
            mask: platformMask,
            fetchPage: async (_c, options) => history(newest)(options as Fetch),
            catchUp: () => ({ page: 25, max: 500 })
        };
        const timelines = createTimelines({ ...dependencies, limits });
        await timelines.loadNewest(channel);

        newest = 80;
        await timelines.onJoined(channel);

        expectNoHole(timelines.state(channel), 80);
    });
});

describe("what the server adds later", () => {
    test("a live message keeps every field its payload carries, including ones this client does not know", async () => {
        const timelines = createTimelines({ mask: platformMask, limits, fetchPage: async () => page([1]) });
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
        const timelines = createTimelines({ mask: platformMask, limits, fetchPage: async () => page([1, 2]) });
        await timelines.loadNewest(channel);

        timelines.applyEvent(channel, "message.visibility", { id: 2, channel_id: channel, state: "hidden", notice: "This message is under review." });

        const [, two] = timelines.state(channel).messages;
        expect(two.body).toBeNull();
        expect(messageDisplay(two)).toEqual({ kind: "notice", text: "This message is under review." });
    });

    test("a message hidden live and then shown again live draws its body again", async () => {
        const timelines = createTimelines({ mask: platformMask, limits, fetchPage: async () => page([1, 2]) });
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
        const timelines = createTimelines({ mask: platformMask, limits, fetchPage: async () => answers.shift() as HistoryPage });
        await timelines.loadNewest(channel);

        expect(messageDisplay(timelines.state(channel).messages[0])).toEqual({ kind: "notice", text: "Removed by a moderator." });

        await timelines.loadNewest(channel);
        expect(messageDisplay(timelines.state(channel).messages[0])).toEqual({ kind: "message", text: "back again" });
    });
});

describe("catching up when fetches fail or are overtaken", () => {
    // A stand-in for the platform's history: messages by id, ids drawn from one sequence shared by
    // every channel, so this channel's ids skip. A page is read when it is asked for, as the
    // database reads it, so a page asked for before a change does not carry it.
    type Fetch = { limit: number, beforeId?: number, afterId?: number };

    function server(initial: number[]) {
        const rows = new Map<number, TimelineMessage>();
        // the room's revision, moved by each change as the platform moves it
        let rev = 0;
        for (const id of initial) {
            rows.set(id, { ...message(id), visibility: "visible" });
        }

        function sorted(): number[] {
            return [...rows.keys()].sort((a, b) => a - b);
        }

        return {
            rows,
            add: (id: number): TimelineMessage => {
                const row = { ...message(id), visibility: "visible" };
                rows.set(id, row);
                rev++;
                return row;
            },
            bump: (): number => ++rev,
            ids: sorted,
            read: (options: Fetch): HistoryPage => {
                const all = sorted();
                let chosen: number[];
                let hasMore: boolean;
                if (options.afterId !== undefined) {
                    const newer = all.filter(id => id > (options.afterId as number));
                    chosen = newer.slice(0, options.limit);
                    hasMore = newer.length > options.limit;
                }
                else if (options.beforeId !== undefined) {
                    const older = all.filter(id => id < (options.beforeId as number));
                    chosen = older.slice(-options.limit);
                    hasMore = older.length > options.limit;
                }
                else {
                    chosen = all.slice(-options.limit);
                    hasMore = all.length > options.limit;
                }
                const messages = chosen.map(id => ({ ...(rows.get(id) as TimelineMessage) }));
                // The newest page comes newest first; a page after an id comes oldest first.
                if (options.afterId === undefined) {
                    messages.reverse();
                }
                return { messages, read_cursor: null, unread_count: 0, has_more: hasMore, rev };
            }
        };
    }

    // A fetch the test answers by hand: the page is read when asked, and handed over (or refused)
    // when the test says so.
    type Pending = { options: Fetch, answer: () => void, refuse: (error?: unknown) => void };

    function controlled(platform: ReturnType<typeof server>, holdWhen: (options: Fetch) => boolean) {
        const pending: Pending[] = [];
        const asked: Fetch[] = [];
        let inFlight = 0;
        let mostInFlight = 0;

        const fetchPage = (_c: string, options: Fetch): Promise<HistoryPage> => {
            asked.push(options);
            const read = platform.read(options);
            if (!holdWhen(options)) {
                return Promise.resolve(read);
            }
            inFlight++;
            mostInFlight = Math.max(mostInFlight, inFlight);
            return new Promise<HistoryPage>((resolve, reject) => {
                pending.push({
                    options,
                    answer: () => {
                        inFlight--;
                        resolve(read);
                    },
                    refuse: error => {
                        inFlight--;
                        // the shape the shell throws: our code as the message, the HTTP status beside it
                        reject(error ?? { message: "rpc.unavailable", status: 503 });
                    }
                });
            });
        };

        return { fetchPage, pending, asked, mostInFlight: (): number => mostInFlight };
    }

    // Waits stand in for the retry's backoff; each resolves when the test lets it.
    function waits() {
        const list: Array<Deferred<void>> = [];
        return {
            list,
            sleep: (): Promise<void> => {
                const wait = deferred<void>();
                list.push(wait);
                return wait.promise;
            }
        };
    }

    async function settle(): Promise<void> {
        for (let i = 0; i < 20; i++) {
            await new Promise(r => setTimeout(r, 0));
        }
    }

    function live(row: TimelineMessage): Record<string, unknown> {
        return { ...row, channel_id: channel, parent_id: null, shown_in_channel: false };
    }

    // Everything from the first message held up to the newest the server has is held: one run,
    // nothing missing in the middle, and older ones reachable when it does not start at the first.
    function expectCaughtUp(state: { messages: TimelineMessage[], hasMore: boolean }, serverIds: number[]): void {
        const held = ids(state.messages);
        const from = held[0];
        expect(held).toEqual(serverIds.filter(id => id >= from));
        expect(from === serverIds[0] || state.hasMore).toBe(true);
    }

    // Everything held from the first message up to the point the timelines say is proven is
    // exactly what the server has in that range.
    function expectVerified(state: { messages: TimelineMessage[], verifiedThrough: number | null }, serverIds: number[]): void {
        const held = ids(state.messages);
        if (held.length === 0 || state.verifiedThrough === null) {
            return;
        }
        const through = state.verifiedThrough;
        expect(held.filter(id => id <= through)).toEqual(serverIds.filter(id => id >= held[0] && id <= through));
    }

    test("a catch-up page that fails is tried again, and a live message meanwhile cannot carry it past the gap", async () => {
        const platform = server([1, 2, 3]);
        const fetches = controlled(platform, options => options.afterId !== undefined);
        const backoff = waits();
        const timelines = createTimelines({ mask: platformMask, limits, fetchPage: fetches.fetchPage, sleep: backoff.sleep });
        await timelines.loadNewest(channel);

        for (let id = 4; id <= 30; id++) {
            platform.add(id);
        }
        const joining = timelines.onJoined(channel);
        await settle();
        fetches.pending.shift()?.refuse();
        await settle();
        expect(timelines.state(channel).isBehind).toBe(true);

        // a live copy lands while the retry waits
        timelines.applyEvent(channel, "message.created", live(platform.add(31)));
        backoff.list.shift()?.resolve();
        await settle();
        fetches.pending.shift()?.answer();
        // the room moved since its first page, so the join reads the held run again behind it
        await settle();
        fetches.pending.shift()?.answer();
        await joining;

        expectCaughtUp(timelines.state(channel), platform.ids());
        expect(timelines.state(channel).isBehind).toBe(false);
    });

    test("an older page asked for again while it is on its way is asked once, both asks ending with it", async () => {
        const platform = server(Array.from({ length: 200 }, (_, i) => i + 1));
        const fetches = controlled(platform, options => options.beforeId !== undefined);
        const timelines = createTimelines({ mask: platformMask, limits, fetchPage: fetches.fetchPage });
        await timelines.loadNewest(channel);
        const oldest = ids(timelines.state(channel).messages)[0];

        const first = timelines.loadOlder(channel);
        const second = timelines.loadOlder(channel);
        await settle();
        expect(fetches.pending).toHaveLength(1);

        fetches.pending.shift()?.answer();
        await Promise.all([first, second]);

        expect(ids(timelines.state(channel).messages)[0]).toBeLessThan(oldest);
    });

    test("an older page asked for before the channel was reloaded is dropped when it arrives", async () => {
        const platform = server(Array.from({ length: 200 }, (_, i) => i + 1));
        const fetches = controlled(platform, options => options.beforeId !== undefined);
        const timelines = createTimelines({ mask: platformMask, limits, fetchPage: fetches.fetchPage, catchUp: () => ({ page: 25, max: 50 }) });
        await timelines.loadNewest(channel);

        const older = timelines.loadOlder(channel);
        await settle();
        for (let id = 201; id <= 1000; id++) {
            platform.add(id);
        }
        await timelines.onJoined(channel);
        fetches.pending.shift()?.answer();
        await older;

        expectCaughtUp(timelines.state(channel), platform.ids());
        expect(ids(timelines.state(channel).messages)[0]).toBe(1000 - firstPageSize + 1);
    });

    test("when the reload past the bound fails, what was on screen stays", async () => {
        const platform = server([1, 2, 3]);
        const fetches = controlled(platform, options => options.afterId === undefined && options.beforeId === undefined && platform.ids().length > 3);
        const backoff = waits();
        const timelines = createTimelines({ mask: platformMask, limits, fetchPage: fetches.fetchPage, sleep: backoff.sleep, catchUp: () => ({ page: 25, max: 50 }) });
        await timelines.loadNewest(channel);

        for (let id = 4; id <= 1000; id++) {
            platform.add(id);
        }
        void timelines.onJoined(channel);
        await settle();
        fetches.pending.shift()?.refuse();
        await settle();

        const held = ids(timelines.state(channel).messages);
        expect(held.length).toBeGreaterThan(0);
        expect(held[0]).toBe(1);
        expect(timelines.state(channel).isBehind).toBe(true);
        timelines.leave(channel);
    });

    test("a rejoin shows the edits, deletes and hides made while the socket was down", async () => {
        const platform = server([1, 2, 3, 4, 5]);
        const timelines = createTimelines({ mask: platformMask, limits, fetchPage: async (_c, options) => platform.read(options as Fetch) });
        await timelines.loadNewest(channel);
        await timelines.onJoined(channel);

        // the socket is down: none of these arrive live
        Object.assign(platform.rows.get(2) as TimelineMessage, { body: "edited", edited_at: "2026-09-23T11:00:00Z" });
        Object.assign(platform.rows.get(3) as TimelineMessage, { body: null, deleted_at: "2026-09-23T11:01:00Z" });
        Object.assign(platform.rows.get(4) as TimelineMessage, { body: null, visibility: "hidden", visibility_notice: "Hidden while it is reviewed." });
        // each change moves the room's revision, as the platform's commit does
        platform.bump();
        platform.bump();
        platform.bump();

        await timelines.onJoined(channel);

        const [, two, three, four] = timelines.state(channel).messages;
        expect(two.body).toBe("edited");
        expect(three.deleted_at).toBe("2026-09-23T11:01:00Z");
        expect(four.body).toBeNull();
        expect(messageDisplay(four)).toEqual({ kind: "notice", text: "Hidden while it is reviewed." });
    });

    test("a channel opened again replaces a stale run even when a live message arrives before its newest page", async () => {
        const platform = server([1, 2, 3]);
        const fetches = controlled(platform, options => options.afterId === undefined && options.beforeId === undefined && platform.ids().length > 3);
        const timelines = createTimelines({ mask: platformMask, limits, fetchPage: fetches.fetchPage, catchUp: () => ({ page: 25, max: 500 }) });
        await timelines.loadNewest(channel);
        await timelines.onJoined(channel);

        for (let id = 4; id <= 63; id++) {
            platform.add(id);
        }
        const reopening = timelines.loadNewest(channel);
        await settle();
        timelines.applyEvent(channel, "message.created", live(platform.add(64)));
        fetches.pending.shift()?.answer();
        await reopening;

        expectCaughtUp(timelines.state(channel), platform.ids());
    });

    test("a join confirmed while a channel is being opened again catches up from the fresh page, not under it", async () => {
        const platform = server([1, 2, 3]);
        const fetches = controlled(platform, options => options.afterId === undefined && options.beforeId === undefined && platform.ids().length > 3);
        const timelines = createTimelines({ mask: platformMask, limits, fetchPage: fetches.fetchPage, catchUp: () => ({ page: 5, max: 500 }) });
        await timelines.loadNewest(channel);
        await timelines.onJoined(channel);

        for (let id = 4; id <= 63; id++) {
            platform.add(id);
        }
        const reopening = timelines.loadNewest(channel);
        const joining = timelines.onJoined(channel);
        await settle();
        fetches.pending.shift()?.answer();
        await reopening;
        await joining;

        expect(ids(timelines.state(channel).messages)[0]).toBe(63 - firstPageSize + 1);
        expectCaughtUp(timelines.state(channel), platform.ids());
    });

    test("a page asked for before a live hide does not bring the hidden body back", async () => {
        const platform = server([1, 2, 3]);
        const fetches = controlled(platform, options => options.afterId === undefined && options.beforeId === undefined && fetches.asked.length > 1);
        const timelines = createTimelines({ mask: platformMask, limits, fetchPage: fetches.fetchPage });
        await timelines.loadNewest(channel);

        const resyncing = timelines.loadNewest(channel);
        await settle();
        Object.assign(platform.rows.get(2) as TimelineMessage, { body: null, visibility: "hidden", visibility_notice: "Hidden." });
        timelines.applyEvent(channel, "message.visibility", { id: 2, channel_id: channel, state: "hidden", notice: "Hidden.", rev: platform.bump() });
        fetches.pending.shift()?.answer();
        await resyncing;

        const two = timelines.state(channel).messages[1];
        expect(two.body).toBeNull();
        expect(two.visibility).toBe("hidden");
    });

    test("joins in quick succession run one catch-up at a time, and one more after it", async () => {
        const platform = server([1, 2, 3]);
        const fetches = controlled(platform, options => options.afterId !== undefined);
        const timelines = createTimelines({ mask: platformMask, limits, fetchPage: fetches.fetchPage });
        await timelines.loadNewest(channel);

        const first = timelines.onJoined(channel);
        const second = timelines.onJoined(channel);
        const third = timelines.onJoined(channel);
        await settle();
        while (fetches.pending.length > 0) {
            fetches.pending.shift()?.answer();
            await settle();
        }
        await Promise.all([first, second, third]);

        expect(fetches.mostInFlight()).toBe(1);
        expect(fetches.asked.filter(f => f.afterId !== undefined)).toHaveLength(2);
    });

    test("leaving a channel stops a catch-up waiting to try again", async () => {
        const platform = server([1, 2, 3]);
        const fetches = controlled(platform, options => options.afterId !== undefined);
        const backoff = waits();
        const timelines = createTimelines({ mask: platformMask, limits, fetchPage: fetches.fetchPage, sleep: backoff.sleep });
        await timelines.loadNewest(channel);

        const joining = timelines.onJoined(channel);
        await settle();
        fetches.pending.shift()?.refuse();
        await settle();
        const askedBefore = fetches.asked.length;

        timelines.leave(channel);
        backoff.list.shift()?.resolve();
        await joining;
        await settle();

        expect(fetches.asked).toHaveLength(askedBefore);
    });

    test("a message committed late under a lower id than one held is picked up by the next rejoin", async () => {
        const platform = server([1, 2, 3, 5]);
        const timelines = createTimelines({ mask: platformMask, limits, fetchPage: async (_c, options) => platform.read(options as Fetch) });
        await timelines.loadNewest(channel);
        await timelines.onJoined(channel);

        // 4 was taken before 5 but committed after it
        platform.add(4);
        await timelines.onJoined(channel);

        expect(ids(timelines.state(channel).messages)).toEqual([1, 2, 3, 4, 5]);
    });

    test("a live copy that lands while a catch-up page is failing does not count as proven", async () => {
        const platform = server([1, 2, 3]);
        const fetches = controlled(platform, options => options.afterId !== undefined);
        const backoff = waits();
        const timelines = createTimelines({ mask: platformMask, limits, fetchPage: fetches.fetchPage, sleep: backoff.sleep });
        await timelines.loadNewest(channel);

        for (let id = 4; id <= 30; id++) {
            platform.add(id);
        }
        void timelines.onJoined(channel);
        await settle();
        fetches.pending.shift()?.refuse();
        await settle();
        timelines.applyEvent(channel, "message.created", live(platform.add(31)));

        expect(timelines.state(channel).verifiedThrough).toBe(3);
        timelines.leave(channel);
    });

    test("a refusal of access during catch-up stops it and reaches whoever asked", async () => {
        const platform = server([1, 2, 3]);
        const fetches = controlled(platform, options => options.afterId !== undefined);
        const backoff = waits();
        const timelines = createTimelines({ mask: platformMask, limits, fetchPage: fetches.fetchPage, sleep: backoff.sleep });
        await timelines.loadNewest(channel);

        const joining = timelines.onJoined(channel);
        await settle();
        fetches.pending.shift()?.refuse({ message: "authz.not_member", status: 403 });

        await expect(joining).rejects.toEqual({ message: "authz.not_member", status: 403 });
        expect(timelines.state(channel).isBehind).toBe(false);
        expect(backoff.list).toHaveLength(0);
    });

    test("a platform that is unavailable during catch-up is tried again", async () => {
        const platform = server([1, 2, 3]);
        const fetches = controlled(platform, options => options.afterId !== undefined);
        const backoff = waits();
        const timelines = createTimelines({ mask: platformMask, limits, fetchPage: fetches.fetchPage, sleep: backoff.sleep });
        await timelines.loadNewest(channel);

        const joining = timelines.onJoined(channel);
        await settle();
        fetches.pending.shift()?.refuse({ message: "rpc.unavailable", status: 503 });
        await settle();

        expect(timelines.state(channel).isBehind).toBe(true);
        expect(backoff.list).toHaveLength(1);
        backoff.list.shift()?.resolve();
        await settle();
        fetches.pending.shift()?.answer();
        await joining;
        expect(timelines.state(channel).isBehind).toBe(false);
    });

    test("a join confirmed while a reload is read is still caught up when the reload fails", async () => {
        const platform = server([1, 2, 3]);
        let holdNewest = false;
        const fetches = controlled(platform, options => holdNewest && options.afterId === undefined && options.beforeId === undefined);
        const backoff = waits();
        const timelines = createTimelines({ mask: platformMask, limits, fetchPage: fetches.fetchPage, sleep: backoff.sleep });
        await timelines.loadNewest(channel);
        await timelines.onJoined(channel);

        for (let id = 4; id <= 9; id++) {
            platform.add(id);
        }
        holdNewest = true;
        const reloading = timelines.loadNewest(channel);
        await settle();
        void timelines.onJoined(channel);
        const askedBefore = fetches.asked.length;
        fetches.pending.shift()?.refuse();
        await expect(reloading).rejects.toBeDefined();
        await settle();

        expect(fetches.asked.length).toBeGreaterThan(askedBefore);
        expectCaughtUp(timelines.state(channel), platform.ids());
    });

    test("a channel opened again re-reads what it holds on the next join, so a removal well back is shown", async () => {
        const platform = server(Array.from({ length: 130 }, (_, i) => i + 1));
        const timelines = createTimelines({ mask: platformMask, limits, fetchPage: async (_c, options) => platform.read(options as Fetch) });
        await timelines.loadNewest(channel);
        await timelines.onJoined(channel);
        while (timelines.state(channel).hasMore) {
            await timelines.loadOlder(channel);
        }
        timelines.leave(channel);

        // away from the channel, a message 80 back is removed and ten more arrive
        const removed = platform.rows.get(50) as TimelineMessage;
        platform.rows.set(50, { ...removed, body: null, visibility: "removed", visibility_notice: "Removed." });
        for (let id = 131; id <= 140; id++) {
            platform.add(id);
        }

        await timelines.loadNewest(channel);
        await timelines.onJoined(channel);

        const held = timelines.state(channel).messages.find(m => m.id === 50);
        expect(held?.visibility).toBe("removed");
        expect(held?.body).toBeNull();
    });

    test.each([
        ["deleted", "message.deleted", { deleted_at: "2026-10-05T10:00:00Z" }, { deleted_at: "2026-10-05T10:00:00Z", body: null }],
        ["hidden", "message.visibility", { state: "hidden", notice: "Hidden." }, { visibility: "hidden", visibility_notice: "Hidden.", body: null }]
    ])("a message %s live while a page carrying it is on its way stays that way when the page lands", async (_what, event, change, stored) => {
        const platform = server([1, 2, 3]);
        const fetches = controlled(platform, options => options.afterId !== undefined);
        const timelines = createTimelines({ mask: platformMask, limits, fetchPage: fetches.fetchPage });
        await timelines.loadNewest(channel);

        for (let id = 4; id <= 6; id++) {
            platform.add(id);
        }
        const joining = timelines.onJoined(channel);
        await settle();
        // 7 to 10 are written after the forward page was read, so the page that brings 7 is the
        // reread the join makes because the room moved since its first page
        for (let id = 7; id <= 10; id++) {
            platform.add(id);
        }
        fetches.pending.shift()?.answer();
        await settle();
        expect(fetches.pending.map(fetch => fetch.options.afterId)).toEqual([0]);
        expect(timelines.state(channel).messages.find(m => m.id === 7)).toBeUndefined();

        // the reread was read with 7 as it was; the change to 7 is stored after, and arrives live
        // while 7 is not held yet, so only the change laid over the page keeps 7 as it now is
        Object.assign(platform.rows.get(7) as TimelineMessage, stored);
        timelines.applyEvent(channel, event, { id: 7, channel_id: channel, ...change, rev: platform.bump() });
        fetches.pending.shift()?.answer();
        await joining;
        expect(fetches.pending).toEqual([]);

        const held = timelines.state(channel).messages.find(m => m.id === 7);
        expect(held?.body).toBeNull();
        if (event === "message.deleted") {
            expect(held?.deleted_at).toBe("2026-10-05T10:00:00Z");
        }
        else {
            expect(held?.visibility).toBe("hidden");
        }
    });

    test("an older newest page that lands after a newer one does not bring back what the newer one hid", async () => {
        const platform = server([1, 2, 3]);
        let holdNext = false;
        const fetches = controlled(platform, options => {
            const isHeld = holdNext && options.afterId === undefined && options.beforeId === undefined;
            holdNext = false;
            return isHeld;
        });
        const timelines = createTimelines({ mask: platformMask, limits, fetchPage: fetches.fetchPage });
        await timelines.loadNewest(channel);

        holdNext = true;
        const older = timelines.loadNewest(channel);
        await settle();
        const hidden = platform.rows.get(2) as TimelineMessage;
        platform.rows.set(2, { ...hidden, body: null, visibility: "hidden", visibility_notice: "Hidden." });
        await timelines.loadNewest(channel);
        fetches.pending.shift()?.answer();
        await older;

        const held = timelines.state(channel).messages.find(m => m.id === 2);
        expect(held?.visibility).toBe("hidden");
        expect(held?.body).toBeNull();
    });

    test("an older page asked for before the channel was left is dropped even when nothing held changed", async () => {
        const platform = server(Array.from({ length: 200 }, (_, i) => i + 1));
        const fetches = controlled(platform, options => options.beforeId !== undefined);
        const timelines = createTimelines({ mask: platformMask, limits, fetchPage: fetches.fetchPage });
        await timelines.loadNewest(channel);
        const before = ids(timelines.state(channel).messages);

        const older = timelines.loadOlder(channel);
        await settle();
        timelines.leave(channel);
        fetches.pending.shift()?.answer();
        await older;

        expect(ids(timelines.state(channel).messages)).toEqual(before);
    });

    test("leaving a channel before its first page lets go of the live events waiting for it", async () => {
        const platform = server([1, 2, 3]);
        const timelines = createTimelines({ mask: platformMask, limits, fetchPage: async (_c, options) => platform.read(options as Fetch) });

        timelines.applyEvent(channel, "message.edited", { id: 2, channel_id: channel, body: "stale", edited_at: "2026-10-05T09:00:00Z" });
        timelines.leave(channel);
        const fresh = platform.rows.get(2) as TimelineMessage;
        platform.rows.set(2, { ...fresh, body: "fresh", edited_at: "2026-10-05T11:00:00Z" });
        await timelines.loadNewest(channel);

        expect(timelines.state(channel).messages.find(m => m.id === 2)?.body).toBe("fresh");
    });

    test("once stopped, a join or a load starts nothing", async () => {
        const platform = server([1, 2, 3]);
        const fetches = controlled(platform, () => false);
        const timelines = createTimelines({ mask: platformMask, limits, fetchPage: fetches.fetchPage });
        await timelines.loadNewest(channel);
        const askedBefore = fetches.asked.length;

        timelines.stop();
        await timelines.onJoined(channel);
        await timelines.loadNewest(channel);
        await settle();

        expect(fetches.asked).toHaveLength(askedBefore);
    });

    test("any mix of failures, delays, drops and rejoins ends caught up once the network is back", async () => {
        for (let seed = 1; seed <= 25; seed++) {
            let state = seed;
            const random = (): number => {
                state = (Math.imul(state, 1103515245) + 12345) & 0x7fffffff;
                return state / 2147483648;
            };

            const platform = server([]);
            let nextId = 1;
            const grow = (): TimelineMessage => {
                // other channels take ids too, so this channel's skip
                nextId += 1 + Math.floor(random() * 3);
                return platform.add(nextId);
            };
            for (let i = 0; i < 40; i++) {
                grow();
            }

            let failRate = 0.3;
            const timelines = createTimelines({
                mask: platformMask,
                limits,
                fetchPage: async (_c, options) => {
                    const read = platform.read(options as Fetch);
                    for (let i = Math.floor(random() * 4); i > 0; i--) {
                        await Promise.resolve();
                    }
                    if (random() < failRate) {
                        throw { message: "rpc.unavailable", status: 503 };
                    }
                    return read;
                },
                sleep: () => Promise.resolve(),
                catchUp: () => ({ page: 10, max: 60 })
            });

            const running: Array<Promise<unknown>> = [];
            for (let step = 0; step < 60; step++) {
                const roll = random();
                if (roll < 0.4) {
                    const row = grow();
                    // a live copy is sometimes lost
                    if (random() < 0.6) {
                        timelines.applyEvent(channel, "message.created", live(row));
                    }
                }
                else if (roll < 0.6) {
                    running.push(timelines.onJoined(channel).catch(() => undefined));
                }
                else if (roll < 0.75) {
                    running.push(timelines.loadNewest(channel).catch(() => undefined));
                }
                else if (roll < 0.85) {
                    running.push(timelines.loadOlder(channel).catch(() => undefined));
                }
                await Promise.resolve();
            }

            // Before anything heals it: whatever is held up to the point the timelines say is
            // proven is exactly what the server has there, so no hole hides below that point.
            await Promise.all(running);
            await settle();
            expectVerified(timelines.state(channel), platform.ids());

            // the network is back and the socket rejoins
            failRate = 0;
            await timelines.loadNewest(channel).catch(() => undefined);
            await timelines.onJoined(channel);
            await settle();

            expectCaughtUp(timelines.state(channel), platform.ids());
        }
    // 25 interleavings take several seconds; each is still a few hundred steps
    }, 30000);
});

describe("a message's pieces and blocks, live", () => {
    const linked = [{ type: "text", text: "see " }, { type: "link", text: "https://x.org", url: "https://x.org" }];
    const blocks = [{ type: "divider", fallback: { type: "text", text: "---" } }];

    test("an edit replaces the pieces, and a page read before it does not undo that", async () => {
        const answers = [page([1]), page([1])];
        const timelines = createTimelines({ mask: platformMask, limits, fetchPage: async () => answers.shift() as HistoryPage });
        await timelines.loadNewest(channel);

        timelines.applyEvent(channel, "message.edited", { id: 1, channel_id: channel, body: "see https://x.org", pieces: linked, edited_at: "2026-10-09T10:05:00Z" });
        await timelines.onJoined(channel);

        expect(timelines.state(channel).messages[0].pieces).toEqual(linked);

        timelines.applyEvent(channel, "message.edited", { id: 1, channel_id: channel, body: "plain", pieces: null, edited_at: "2026-10-09T10:06:00Z" });
        expect(timelines.state(channel).messages[0].pieces).toBeNull();
    });

    test("a delete and a hide clear the pieces and blocks with the body, and showing again brings them back", async () => {
        const held = { ...message(1, "see https://x.org"), pieces: linked, blocks } as TimelineMessage;
        const timelines = createTimelines({ mask: platformMask, limits, fetchPage: async () => ({ messages: [held, { ...message(2), pieces: linked, blocks } as TimelineMessage], read_cursor: null, unread_count: 0, has_more: false }) });
        await timelines.loadNewest(channel);

        timelines.applyEvent(channel, "message.deleted", { id: 1, channel_id: channel, deleted_at: "2026-10-09T10:06:00Z" });
        timelines.applyEvent(channel, "message.visibility", { id: 2, channel_id: channel, state: "hidden", notice: "Under review." });

        const [one, two] = timelines.state(channel).messages;
        expect([one.pieces, one.blocks, two.pieces, two.blocks]).toEqual([null, null, null, null]);

        timelines.applyEvent(channel, "message.visibility", { id: 2, channel_id: channel, state: "visible", notice: null, body: "m2", pieces: linked, blocks });
        expect([timelines.state(channel).messages[1].pieces, timelines.state(channel).messages[1].blocks]).toEqual([linked, blocks]);
    });
    test("a late copy of a message does not pair an edit's body with older pieces, nor bring pieces back to a deleted one", async () => {
        const timelines = createTimelines({ mask: platformMask, limits, fetchPage: async () => page([1, 2]) });
        await timelines.loadNewest(channel);
        const created = (id: number): Record<string, unknown> => ({
            id, channel_id: channel, parent_id: null, shown_in_channel: false, person_alias_guid: "a0000001-0000-4000-8000-000000000000",
            message_type: "text", body: `m${id}`, pieces: [{ type: "text", text: "old" }], blocks, created_at: "2026-09-23T10:00:01Z"
        });

        timelines.applyEvent(channel, "message.edited", { id: 1, channel_id: channel, body: "see https://x.org", pieces: linked, edited_at: "2026-10-09T10:05:00Z" });
        timelines.applyEvent(channel, "message.deleted", { id: 2, channel_id: channel, deleted_at: "2026-10-09T10:06:00Z" });
        timelines.applyEvent(channel, "message.created", created(1));
        timelines.applyEvent(channel, "message.created", created(2));

        const [one, two] = timelines.state(channel).messages;
        expect([one.body, one.pieces]).toEqual(["see https://x.org", linked]);
        expect([two.body, two.pieces, two.blocks]).toEqual([null, null, null]);
    });

    test("a page copy of a deleted or hidden message never carries pieces or blocks", async () => {
        const gone = { ...message(1), deleted_at: "2026-10-09T10:06:00Z", pieces: linked, blocks } as TimelineMessage;
        const hidden = { ...message(2), visibility: "hidden", pieces: linked, blocks } as TimelineMessage;
        const timelines = createTimelines({ mask: platformMask, limits, fetchPage: async () => ({ messages: [hidden, gone], read_cursor: null, unread_count: 0, has_more: false }) });
        await timelines.loadNewest(channel);

        const [one, two] = timelines.state(channel).messages;
        expect([one.pieces, one.blocks, two.pieces, two.blocks]).toEqual([null, null, null, null]);
    });
});

// A message the person sent carries its send's key on every copy. The timelines report each such
// copy as it lands, however it came (the send's answer, the live copy, a history page or a live
// copy that waited for the first page), so the sender can let its row go at the first of them.
describe("a copy carrying a send's key, as it lands", () => {
    const keyed = (id: number, key: string | null): TimelineMessage => ({ ...message(id), client_key: key });

    test("is reported whether it comes live, on a page, or put in place by the sender, and a message with no key is not", async () => {
        const landed: Array<[number, string | null | undefined]> = [];
        const timelines = createTimelines({
            mask: platformMask,
            limits,
            fetchPage: async () => ({ messages: [keyed(2, "from-page"), keyed(1, null)], read_cursor: null, unread_count: 0, has_more: false }),
            onLanded: m => landed.push([m.id, m.client_key])
        });

        await timelines.loadNewest(channel);
        timelines.applyEvent(channel, "message.created", { ...keyed(3, "from-live"), channel_id: channel, parent_id: null, shown_in_channel: false });
        timelines.upsert(channel, keyed(4, "from-answer"));

        expect(landed).toEqual([[2, "from-page"], [3, "from-live"], [4, "from-answer"]]);
    });

    test("a live copy that came before the first page is reported once the page is in and it lands", async () => {
        const first = deferred<HistoryPage>();
        const landed: number[] = [];
        const timelines = createTimelines({ mask: platformMask, limits, fetchPage: () => first.promise, onLanded: m => landed.push(m.id) });

        const loading = timelines.loadNewest(channel);
        timelines.applyEvent(channel, "message.created", { ...keyed(5, "early"), channel_id: channel, parent_id: null, shown_in_channel: false });
        expect(landed).toEqual([]);

        first.resolve(page([4]));
        await loading;

        expect(landed).toEqual([5]);
    });
});

// A message's files are carried wherever its body is and withheld wherever its body is, so a
// deleted, hidden or removed message never keeps them on screen, and one shown again brings them back.
describe("files follow the body", () => {
    const photo: AttachmentItem = { id: "d0000000-0000-4000-8000-000000000001", key: "t/d0000000-0000-4000-8000-000000000001", kind: "image", name: "p.jpg", size: 900, type: "image/jpeg", width: 400, height: 300 };

    function held(): TimelineMessage {
        return { id: 5, person_alias_guid: "a0000001-0000-4000-8000-000000000000", message_type: "text", body: null, attachments: [photo], created_at: "2026-10-28T10:00:00Z" };
    }

    test("a delete and a hide take the files away with the body", () => {
        const deleted = held();
        applyMessageChange(deleted, "message.deleted", { id: 5, channel_id: channel, deleted_at: "2026-10-28T10:01:00Z" });
        expect(deleted.attachments).toBeNull();

        const hidden = held();
        applyMessageChange(hidden, "message.visibility", { id: 5, channel_id: channel, state: "hidden", notice: "Hidden by a leader." });
        expect(hidden.attachments).toBeNull();
    });

    test("a message of files alone shown again brings its files back with its null body", () => {
        const message = held();
        applyMessageChange(message, "message.visibility", { id: 5, channel_id: channel, state: "hidden", notice: "Hidden." });

        applyMessageChange(message, "message.visibility", { id: 5, channel_id: channel, state: "visible", notice: null, body: null, pieces: null, blocks: null, attachments: [photo] });

        expect(message.visibility).toBe("visible");
        expect(message.body).toBeNull();
        expect(message.attachments).toEqual([photo]);
    });

    test("a live copy of a message held deleted does not bring its files back", async () => {
        const timelines = createTimelines({ mask: platformMask, limits, fetchPage: async () => ({ messages: [held()], read_cursor: null, unread_count: 0, has_more: false }) });
        await timelines.loadNewest(channel);
        timelines.applyEvent(channel, "message.deleted", { id: 5, channel_id: channel, deleted_at: "2026-10-28T10:01:00Z" });

        timelines.upsert(channel, held());

        expect(timelines.state(channel).messages[0].attachments).toBeNull();
    });
});

describe("a message by someone the reader blocked", () => {
    /** A page's row of a blocked person's message, as the platform withholds it. */
    function blockedRow(id: number): TimelineMessage {
        return { ...message(id), sender_nick_name: null, body: null, pieces: null, blocked: true };
    }

    test("is drawn as the one blocked sentence, whatever became of it, before a delete or a hide", () => {
        expect(messageDisplay(blockedRow(1))).toEqual({ kind: "blocked", text: "Message from someone you blocked" });
        expect(messageDisplay({ ...blockedRow(1), deleted_at: "2026-11-03T10:00:00Z" }).kind).toBe("blocked");
    });

    test("an edit moves its time and brings back none of its words", () => {
        const held = blockedRow(1);

        applyMessageChange(held, "message.edited", { id: 1, channel_id: channel, body: "edited secret", pieces: [{ type: "text", text: "edited secret" }], edited_at: "2026-11-03T11:00:00Z" });

        expect(held.body).toBeNull();
        expect(held.pieces).toBeNull();
        expect(held.edited_at).toBe("2026-11-03T11:00:00Z");
    });

    test("shown again after a hide, it stays withheld", () => {
        const held = blockedRow(1);

        applyMessageChange(held, "message.visibility", { id: 1, channel_id: channel, state: "visible", notice: null, body: "secret", pieces: null });

        expect(held.body).toBeNull();
    });

    test("a live edit laid over a page read before it brings back nothing the page withheld", async () => {
        const answer = deferred<HistoryPage>();
        const timelines = createTimelines({ mask: platformMask, limits, fetchPage: () => answer.promise });
        const loading = timelines.loadNewest(channel);

        // the edit lands while the page is on its way, so it is laid over the page
        timelines.applyEvent(channel, "message.edited", { id: 1, channel_id: channel, body: "edited secret", pieces: null, edited_at: "2026-11-03T11:00:00Z" });
        answer.resolve({ messages: [blockedRow(1)], read_cursor: null, unread_count: 0, first_unread_id: null, has_more: false });
        await loading;

        const held = timelines.state(channel).messages[0];
        expect(held.blocked).toBe(true);
        expect(held.body).toBeNull();
    });

    test("a visible message edited later than the page that then withholds it keeps none of its words", async () => {
        const pages = [
            page([1]),
            { messages: [{ ...blockedRow(1), edited_at: null }], read_cursor: null, unread_count: 0, first_unread_id: null, has_more: false }
        ];
        const timelines = createTimelines({ mask: platformMask, limits, fetchPage: async () => pages.shift() as HistoryPage });
        await timelines.loadNewest(channel);
        // an edit held from before the block, newer than anything the next page says of it
        timelines.applyEvent(channel, "message.edited", { id: 1, channel_id: channel, body: "edited words", pieces: [{ type: "text", text: "edited words" }], edited_at: "2026-11-03T11:00:00Z" });

        await timelines.loadNewest(channel);

        const held = timelines.state(channel).messages[0];
        expect(held.blocked).toBe(true);
        expect(held.body).toBeNull();
        expect(held.pieces).toBeNull();
    });

    test("a page read after an unblock, which marks nothing, shows the message again in place", async () => {
        const pages = [
            { messages: [blockedRow(1)], read_cursor: null, unread_count: 0, first_unread_id: null, has_more: false },
            page([1])
        ];
        const timelines = createTimelines({ mask: platformMask, limits, fetchPage: async () => pages.shift() as HistoryPage });

        await timelines.loadNewest(channel);
        await timelines.loadNewest(channel);

        const held = timelines.state(channel).messages[0];
        expect(held.blocked).toBeUndefined();
        expect(held.body).toBe("m1");
        expect(held.sender_nick_name).toBe("Ada");
    });
});

describe("a block, as what is held is masked where it is written", () => {
    const dee = "a0000009-0000-4000-8000-000000000000";
    const otherChannel = "c0000002-0000-4000-8000-000000000000";

    /** Dee's message, as a page read before the block carries it. */
    function deeMessage(id: number): TimelineMessage {
        return { ...message(id, `dee ${id}`), person_alias_guid: dee, sender_nick_name: "Dee" };
    }

    /** Dee's message as the platform withholds it once the reader blocked him. */
    function deeWithheld(id: number): TimelineMessage {
        return { ...deeMessage(id), sender_nick_name: null, body: null, blocked: true };
    }

    /** Timelines over a fetch the test answers, masked through the blocks as the shell wires them. */
    function masked(fetchPage: (channelId: string, options: { limit: number, beforeId?: number, afterId?: number }) => Promise<HistoryPage>, catchUp = { page: 100, max: 500 }, extra: Partial<Parameters<typeof createTimelines>[0]> = {}): {
        timelines: ReturnType<typeof createTimelines>,
        blocks: ReturnType<typeof createBlocks>,
        changes: string[][]
    } {
        const changes: string[][] = [];
        // eslint-disable-next-line prefer-const
        let blocks: ReturnType<typeof createBlocks>;
        const timelines = createTimelines({
            limits,
            fetchPage,
            catchUp: () => catchUp,
            mask: m => blocks.mask(m),
            ...extra
        });
        blocks = createBlocks({
            call: async () => ({ ok: true, data: { people: [], blocked_aliases: [] } }),
            onChanged: (_isInSidebarAnswer, removed) => changes.push([...(removed ?? [])]),
            remask: added => timelines.remask(added),
            notify: () => undefined
        });
        blocks.setAliases([]);
        return { timelines, blocks, changes };
    }

    const said = (messages: TimelineMessage[]): string[] => messages.filter(m => m.person_alias_guid === dee).map(m => `${m.id}:${m.body}:${m.sender_nick_name}`);

    test("an older page asked before the block and answered after it with his words lands collapsed, nothing dropped", async () => {
        const older = deferred<HistoryPage>();
        const { timelines, blocks } = masked(async (_c, options) => options.beforeId ? older.promise : page([10, 11], { has_more: true }));
        await timelines.loadNewest(channel);

        const loading = timelines.loadOlder(channel);
        blocks.setAliases([dee]);
        older.resolve({ messages: [deeMessage(9), message(8)], read_cursor: null, unread_count: 0, first_unread_id: null, has_more: false });
        await loading;

        const state = timelines.state(channel);
        expect(ids(state.messages)).toEqual([8, 9, 10, 11]);
        expect(said(state.messages)).toEqual(["9:null:null"]);
        expect(state.messages[1].blocked).toBe(true);
        expect(state.hasMore).toBe(false);
    });

    test("more held than the catch-up bound: every one of his messages collapses at the block, with no read", async () => {
        const fetches: unknown[] = [];
        const { timelines, blocks } = masked(async (_c, options) => {
            fetches.push(options);
            return { messages: Array.from({ length: 6 }, (_, i) => deeMessage(i + 1)), read_cursor: null, unread_count: 0, first_unread_id: null, has_more: false };
        }, { page: 2, max: 3 });
        await timelines.loadNewest(channel);
        fetches.length = 0;

        blocks.setAliases([dee]);

        expect(said(timelines.state(channel).messages)).toEqual(["1:null:null", "2:null:null", "3:null:null", "4:null:null", "5:null:null", "6:null:null"]);
        expect(fetches).toEqual([]);
    });

    test("a room not on screen whose timeline is held shows none of his words the moment he is blocked", async () => {
        const { timelines, blocks } = masked(async channelId => ({ messages: [deeMessage(channelId === channel ? 1 : 2)], read_cursor: null, unread_count: 0, first_unread_id: null, has_more: false }));
        await timelines.loadNewest(channel);
        await timelines.loadNewest(otherChannel);
        timelines.leave(otherChannel);

        blocks.setAliases([dee]);

        expect(said(timelines.state(otherChannel).messages)).toEqual(["2:null:null"]);
        expect(said(timelines.state(channel).messages)).toEqual(["1:null:null"]);
    });

    test("the row object held is the one masked, so whatever holds a reference to it shows the withheld copy", async () => {
        const { timelines, blocks } = masked(async () => page([1]));
        timelines.upsert(channel, deeMessage(5));
        const reference = timelines.state(channel).messages.find(m => m.id === 5) as TimelineMessage;

        blocks.setAliases([dee]);

        expect(timelines.state(channel).messages.find(m => m.id === 5)).toBe(reference);
        expect(reference).toMatchObject({ blocked: true, body: null, sender_nick_name: null });
    });

    test("a live edit or a showing again of someone else's message that mentions him lays no face of his in the chip", async () => {
        const { timelines, blocks } = masked(async () => page([1, 2]));
        await timelines.loadNewest(channel);
        blocks.setAliases([dee]);
        const mentionsDee = [{ type: "text", text: "hi " }, { type: "mention", text: "@Dee Vance", person_alias_guid: dee, nick_name: "Dee", last_name: "Vance", avatar_url: "https://example.org/dee.jpg" }];

        // straight to the timeline, as the edit's own answer comes: the holder is the guard, not the live entry
        timelines.applyEvent(channel, "message.edited", { id: 1, channel_id: channel, body: "hi <@dee>", pieces: mentionsDee, edited_at: "t" });
        timelines.applyEvent(channel, "message.visibility", { id: 2, channel_id: channel, state: "visible", notice: null, body: "hi <@dee>", pieces: mentionsDee });

        const chip = { type: "mention", text: "@Dee Vance", person_alias_guid: dee, nick_name: null, last_name: null, avatar_url: null };
        expect(timelines.state(channel).messages.map(m => m.pieces?.[1])).toEqual([chip, chip]);
        expect(timelines.state(channel).messages.map(m => m.body)).toEqual(["hi <@dee>", "hi <@dee>"]);
    });

    test("a thread's counts that arrived before the first page, his face among them, land blank when he is blocked meanwhile", async () => {
        const first = deferred<HistoryPage>();
        const { timelines, blocks } = masked(() => first.promise);
        const loading = timelines.loadNewest(channel);
        timelines.applyEvent(channel, "thread.updated", { channel_id: channel, root_id: 1, reply_count: 1, last_reply_at: null, participant_count: 1, repliers: [{ person_alias_guid: dee, nick_name: "Dee", last_name: "Vance", avatar_url: "x" }] });

        blocks.setAliases([dee]);
        first.resolve(page([1]));
        await loading;

        expect(timelines.state(channel).messages[0].repliers).toEqual([{ person_alias_guid: dee, nick_name: null, last_name: null, avatar_url: null }]);
    });

    test("a thread's counts laid live on a held root, his face among them, land blank", async () => {
        const { timelines, blocks } = masked(async () => page([1]));
        await timelines.loadNewest(channel);
        blocks.setAliases([dee]);

        // straight to the timeline, past the live entry: the holder is the guard
        timelines.applyEvent(channel, "thread.updated", { channel_id: channel, root_id: 1, reply_count: 1, last_reply_at: null, participant_count: 1, repliers: [{ person_alias_guid: dee, nick_name: "Dee", last_name: "Vance", avatar_url: "x" }] });

        expect(timelines.state(channel).messages[0].repliers).toEqual([{ person_alias_guid: dee, nick_name: null, last_name: null, avatar_url: null }]);
    });

    test("an unblock leaves a room not on screen as it is, and its next open reads his rows again, newest first, a bound's worth, letting go of what is older", async () => {
        const room = platformRoom(() => dee);
        const { timelines, blocks } = masked((channelId, options) => channelId === otherChannel ? room.fetchPage(channelId, options) : Promise.resolve(page([1])), { page: 2, max: 3 });
        blocks.setAliases([dee]);
        await timelines.loadNewest(channel);
        await timelines.loadNewest(otherChannel);
        timelines.leave(otherChannel);

        room.unblock(blocks, dee);
        timelines.release([dee]);

        expect(ids(timelines.state(otherChannel).messages)).toEqual([1, 2, 3, 4, 5, 6]);
        expect(withheldIds(timelines, otherChannel)).toEqual([1, 2, 3, 4, 5, 6]);
        expect(timelines.state(otherChannel).hasMore).toBe(false);
        expect(ids(timelines.state(channel).messages)).toEqual([1]);

        room.newest = 1;
        await openAsShell(timelines, otherChannel);

        expect(withheldIds(timelines, otherChannel)).toEqual([]);
        expect(readsBefore(room.reads)).toEqual([6, 4]);
        expect(ids(timelines.state(otherChannel).messages)).toEqual([2, 3, 4, 5, 6]);
        expect(timelines.state(otherChannel).hasMore).toBe(true);
    });

    /**
     * Timelines of one open room holding rows 1 to count, each withheld when it is Dee's, answered
     * as the platform answers after the unblock: every row with its words. The catch-up's page is
     * 2 and its bound 3; every read is recorded.
     */
    async function heldPastBound(count: number, isDees: (id: number) => boolean): Promise<{ timelines: ReturnType<typeof createTimelines>, blocks: ReturnType<typeof createBlocks>, fetches: Array<{ limit: number, beforeId?: number, afterId?: number }>, unblock: () => void }> {
        const fetches: Array<{ limit: number, beforeId?: number, afterId?: number }> = [];
        let isUnblocked = false;
        const row = (id: number): TimelineMessage => !isDees(id) ? message(id) : isUnblocked ? deeMessage(id) : deeWithheld(id);
        const all = (): TimelineMessage[] => Array.from({ length: count }, (_, i) => row(i + 1));
        const { timelines, blocks } = masked(async (_c, options) => {
            fetches.push(options);
            let run: TimelineMessage[];
            if (options.beforeId !== undefined) {
                run = all().filter(m => m.id < (options.beforeId as number)).slice(-options.limit);
            }
            else if (options.afterId !== undefined) {
                run = all().filter(m => m.id > (options.afterId as number)).slice(0, options.limit);
            }
            else {
                run = all();
            }
            return { messages: run, read_cursor: null, unread_count: 0, first_unread_id: null, has_more: options.afterId !== undefined && run.length > 0 && run[run.length - 1].id < count };
        }, { page: 2, max: 3 });
        blocks.setAliases([dee]);
        await timelines.loadNewest(channel);
        await timelines.onJoined(channel);
        fetches.length = 0;
        return {
            timelines, blocks, fetches, unblock: () => {
                isUnblocked = true;
                blocks.setAliases([]);
                timelines.release([dee]);
            }
        };
    }

    /** Lets every answer already given land, and what each asks in turn. */
    async function settle(): Promise<void> {
        for (let i = 0; i < 30; i++) {
            await Promise.resolve();
        }
    }

    const readsBefore = (fetches: Array<{ beforeId?: number }>): number[] => fetches.filter(f => f.beforeId !== undefined).map(f => f.beforeId as number);

    test("an unblock in the open room keeps the reader's place: what is past the bound is read again, newest first, and the catch-up reads the rest", async () => {
        const h = await heldPastBound(6, () => true);

        h.unblock();
        await h.timelines.onJoined(channel);
        await settle();

        expect(ids(h.timelines.state(channel).messages)).toEqual([1, 2, 3, 4, 5, 6]);
        expect(said(h.timelines.state(channel).messages)).toEqual(["1:dee 1:Dee", "2:dee 2:Dee", "3:dee 3:Dee", "4:dee 4:Dee", "5:dee 5:Dee", "6:dee 6:Dee"]);
        expect(readsBefore(h.fetches)).toEqual([4, 2]);
        expect(h.timelines.state(channel).hasMore).toBe(false);
    });

    test("an unblock in the open room far back reads again at most a bound's worth past it, and lets go of what is older, however much is held", async () => {
        const h = await heldPastBound(12, () => true);

        h.unblock();
        await h.timelines.onJoined(channel);
        await settle();

        // the bound is 3 at a page of 2: two pages past it, never the five that reach row 1
        expect(readsBefore(h.fetches)).toEqual([10, 8]);
        expect(ids(h.timelines.state(channel).messages)).toEqual([6, 7, 8, 9, 10, 11, 12]);
        expect(said(h.timelines.state(channel).messages).every(line => !line.includes("null"))).toBe(true);
        expect(h.timelines.state(channel).hasMore).toBe(true);
    });

    test("an unblock in the open room reads only the pages that hold one of his rows, skipping those between", async () => {
        const h = await heldPastBound(12, id => id === 2 || id === 9);

        h.unblock();
        await h.timelines.onJoined(channel);
        await settle();

        expect(readsBefore(h.fetches)).toEqual([10, 3]);
        expect(ids(h.timelines.state(channel).messages)).toEqual([1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12]);
        expect(said(h.timelines.state(channel).messages)).toEqual(["2:dee 2:Dee", "9:dee 9:Dee"]);
    });

    /**
     * Timelines of the open room holding Dee's rows 1 to 6, verified, then Ada's live 7 and 8, held
     * and not yet verified by a page; answered as the platform answers after the unblock, with every
     * row's words, the newest page being the newest three. The catch-up's page is 2 and its bound 3.
     */
    async function heldWithLiveRows(): Promise<{ timelines: ReturnType<typeof createTimelines>, blocks: ReturnType<typeof createBlocks>, fetches: Array<{ limit: number, beforeId?: number, afterId?: number }>, unblock: () => void }> {
        let isUnblocked = false;
        const fetches: Array<{ limit: number, beforeId?: number, afterId?: number }> = [];
        const row = (id: number): TimelineMessage => id > 6 ? message(id) : isUnblocked ? deeMessage(id) : deeWithheld(id);
        const { timelines, blocks } = masked(async (_c, options) => {
            fetches.push(options);
            const all = Array.from({ length: isUnblocked ? 8 : 6 }, (_, i) => row(i + 1));
            let run: TimelineMessage[];
            if (options.beforeId !== undefined) {
                run = all.filter(m => m.id < (options.beforeId as number)).slice(-options.limit);
                return { messages: run, read_cursor: null, unread_count: 0, first_unread_id: null, has_more: run.length > 0 && run[0].id > 1 };
            }
            if (options.afterId !== undefined) {
                run = all.filter(m => m.id > (options.afterId as number)).slice(0, options.limit);
                return { messages: run, read_cursor: null, unread_count: 0, first_unread_id: null, has_more: run.length > 0 && run[run.length - 1].id < all.length };
            }
            run = isUnblocked ? all.slice(-3) : all;
            return { messages: run, read_cursor: null, unread_count: 0, first_unread_id: null, has_more: isUnblocked };
        }, { page: 2, max: 3 });
        blocks.setAliases([dee]);
        await timelines.loadNewest(channel);
        await timelines.onJoined(channel);
        timelines.applyEvent(channel, "message.created", { ...message(7), channel_id: channel });
        timelines.applyEvent(channel, "message.created", { ...message(8), channel_id: channel });
        fetches.length = 0;
        return {
            timelines, blocks, fetches, unblock: () => {
                isUnblocked = true;
                blocks.setAliases([]);
            }
        };
    }

    /** Opens a room as the shell does: the newest page, then the join's catch-up from it. */
    async function openAsShell(timelines: ReturnType<typeof createTimelines>, channelId: string): Promise<void> {
        await timelines.loadNewest(channelId);
        await timelines.onJoined(channelId);
        await settle();
    }

    const withheldLeft = (timelines: ReturnType<typeof createTimelines>, channelId: string): string[] => said(timelines.state(channelId).messages).filter(line => line.includes("null"));

    test("an unblock in the open room gives back the rows the catch-up's span leaves when the resync's newest page verifies live rows held since the last page: read again newest first, a bound's worth, and what is older let go", async () => {
        const h = await heldWithLiveRows();

        h.unblock();
        h.timelines.release([dee]);
        await openAsShell(h.timelines, channel);

        // the live rows 7 and 8, verified by the newest page, move the catch-up's start past 4 and 5,
        // which are read again after it with 2 and 3; row 1 is past the reread's bound of 3
        expect(withheldLeft(h.timelines, channel)).toEqual([]);
        expect(readsBefore(h.fetches)).toEqual([6, 4]);
        expect(ids(h.timelines.state(channel).messages)).toEqual([2, 3, 4, 5, 6, 7, 8]);
        expect(h.timelines.state(channel).hasMore).toBe(true);
    });

    test("an unblock's rows are read once: the rejoin after the catch-up that read them starts where its own bound does", async () => {
        const h = await heldWithLiveRows();
        h.unblock();
        h.timelines.release([dee]);
        await openAsShell(h.timelines, channel);
        h.fetches.length = 0;

        await h.timelines.onJoined(channel);
        await settle();

        // seven settled rows, 2 to 8, at a bound of 3: from row 6 on
        expect(h.fetches[0]).toEqual({ limit: 2, afterId: 5 });
    });

    test("an unblock in the open room, left before its catch-up ran, gives back every row within the bound when it is opened again", async () => {
        const h = await heldWithLiveRows();

        h.unblock();
        h.timelines.release([dee]);
        h.timelines.leave(channel);
        await settle();
        await openAsShell(h.timelines, channel);

        expect(withheldLeft(h.timelines, channel)).toEqual([]);
    });

    test("an unblock gives back, in a room not on screen opened later, the rows its catch-up's span leaves when its newest page verifies live rows held since its last page: read again newest first, a bound's worth, and what is older let go", async () => {
        const h = await heldWithLiveRows();
        h.timelines.leave(channel);

        h.unblock();
        h.timelines.release([dee]);
        await openAsShell(h.timelines, channel);

        // nothing is let go at the unblock; the open's reread lets go only of row 1, past its bound of 3
        expect(withheldLeft(h.timelines, channel)).toEqual([]);
        expect(readsBefore(h.fetches)).toEqual([6, 4]);
        expect(ids(h.timelines.state(channel).messages)).toEqual([2, 3, 4, 5, 6, 7, 8]);
        expect(h.timelines.state(channel).hasMore).toBe(true);
    });

    test("an unblock whose open room is then replaced by a newest page that does not join reads nothing of what was let go", async () => {
        const fetches: Array<{ limit: number, beforeId?: number, afterId?: number }> = [];
        const reread = deferred<HistoryPage>();
        let isReplaced = false;
        const held = Array.from({ length: 6 }, (_, i) => deeWithheld(i + 1));
        const { timelines, blocks } = masked(async (_c, options) => {
            fetches.push(options);
            if (options.beforeId !== undefined) {
                return reread.promise;
            }
            if (isReplaced) {
                // the newest page, far past what was held, and the catch-up after it
                const newest = [deeMessage(20), deeMessage(21)];
                return { messages: options.afterId === undefined ? newest : [], read_cursor: null, unread_count: 0, first_unread_id: null, has_more: options.afterId === undefined };
            }
            const run = options.afterId === undefined ? held : held.filter(m => m.id > (options.afterId as number)).slice(0, 2);
            return { messages: run, read_cursor: null, unread_count: 0, first_unread_id: null, has_more: options.afterId !== undefined && run.length > 0 && run[run.length - 1].id < 6 };
        }, { page: 2, max: 3 });
        blocks.setAliases([dee]);
        await timelines.loadNewest(channel);
        await timelines.onJoined(channel);
        fetches.length = 0;

        blocks.setAliases([]);
        timelines.release([dee]);
        isReplaced = true;
        await timelines.loadNewest(channel);
        reread.resolve({ messages: [deeMessage(2), deeMessage(3)], read_cursor: null, unread_count: 0, first_unread_id: null, has_more: true });
        await timelines.onJoined(channel);
        await settle();

        expect(ids(timelines.state(channel).messages)).toEqual([20, 21]);
        expect(fetches.filter(f => f.afterId !== undefined && f.afterId < 20)).toEqual([]);
    });

    const ada = "a0000001-0000-4000-8000-000000000000";
    const sam = "a0000008-0000-4000-8000-000000000000";

    type Read = { limit: number, beforeId?: number, afterId?: number };

    /**
     * A room on the platform as the test changes it: rows 1 to count, each by whoever authorOf
     * names, withheld while that person is blocked or while the row is one the platform still
     * withholds for another reason, and a quote of Dee's withheld while he is blocked. A page is
     * answered as the platform stands when it is served, the newest page being the newest `newest`
     * rows. Every read and the rows it carried are recorded; the next read `hold` names waits until
     * the test lets it go, and a read `fail` names is refused with the error it gives.
     */
    type PlatformRoom = {
        count: number,
        newest: number,
        blocked: Set<string>,
        stillWithheld: Set<number>,
        gone: Set<number>,
        reads: Read[],
        served: number,
        held: Array<() => void>,
        hold: (options: Read) => boolean,
        fail: (options: Read) => unknown,
        row: (id: number) => TimelineMessage,
        answer: (options: Read) => HistoryPage,
        fetchPage: (channelId: string, options: Read) => Promise<HistoryPage>,
        unblock: (blocks: ReturnType<typeof createBlocks>, who: string) => void
    };

    function platformRoom(authorOf: (id: number) => string, quotesDee: (id: number) => boolean = () => false): PlatformRoom {
        const room: PlatformRoom = {
            count: 6,
            newest: firstPageSize,
            blocked: new Set<string>([dee]),
            stillWithheld: new Set<number>(),
            gone: new Set<number>(),
            reads: [] as Read[],
            served: 0,
            held: [] as Array<() => void>,
            hold: (_options: Read): boolean => false,
            fail: (_options: Read): unknown => undefined,

            row(id: number): TimelineMessage {
                const who = authorOf(id);
                const shown: TimelineMessage = who === dee ? deeMessage(id) : { ...message(id), person_alias_guid: who, sender_nick_name: who === sam ? "Sam" : "Ada" };
                let row: TimelineMessage = room.blocked.has(who) || room.stillWithheld.has(id) ? { ...shown, sender_nick_name: null, body: null, blocked: true } : shown;
                if (quotesDee(id)) {
                    const quote = { id: 100 + id, person_alias_guid: dee, sender_nick_name: "Dee", body: "dee said", created_at: row.created_at };
                    row = { ...row, quoted_message_id: quote.id, quoted: room.blocked.has(dee) ? { ...quote, sender_nick_name: null, body: null, blocked: true } : quote };
                }
                return row;
            },

            answer(options: Read): HistoryPage {
                const all = Array.from({ length: room.count }, (_, i) => i + 1).filter(id => !room.gone.has(id)).map(id => room.row(id));
                const oldest = all[0]?.id ?? 0;
                const newest = all[all.length - 1]?.id ?? 0;
                let run: TimelineMessage[];
                let hasMore: boolean;
                if (options.beforeId !== undefined) {
                    run = all.filter(m => m.id < (options.beforeId as number)).slice(-options.limit);
                    hasMore = run.length > 0 && run[0].id > oldest;
                }
                else if (options.afterId !== undefined) {
                    run = all.filter(m => m.id > (options.afterId as number)).slice(0, options.limit);
                    hasMore = run.length > 0 && run[run.length - 1].id < newest;
                }
                else {
                    run = all.slice(-room.newest);
                    hasMore = all.length > room.newest;
                }
                return { messages: run, read_cursor: null, unread_count: 0, first_unread_id: null, has_more: hasMore };
            },

            fetchPage: async (_channelId: string, options: Read): Promise<HistoryPage> => {
                room.reads.push(options);
                if (room.hold(options)) {
                    room.hold = () => false;
                    await new Promise<void>(resolve => room.held.push(resolve));
                }
                const error = room.fail(options);
                if (error) {
                    throw error;
                }
                const answer = room.answer(options);
                room.served += answer.messages.length;
                return answer;
            },

            /** The platform lets the person go and the client hears the set, as an unblock lands. */
            unblock(blocks: ReturnType<typeof createBlocks>, who: string): void {
                room.blocked.delete(who);
                blocks.setAliases([...room.blocked]);
            }
        };
        return room;
    }

    /** Every held row withheld, its own words or its quote's. */
    function withheldIds(timelines: ReturnType<typeof createTimelines>, channelId: string): number[] {
        return timelines.state(channelId).messages.filter(m => m.blocked || m.quoted?.blocked).map(m => m.id);
    }

    /** Lets every answer given land, through waits of any length the timelines chain. */
    async function flush(): Promise<void> {
        for (let i = 0; i < 20; i++) {
            await new Promise(resolve => setTimeout(resolve, 0));
        }
    }

    function liveRow(row: TimelineMessage): Record<string, unknown> {
        return { ...row, channel_id: channel };
    }

    test("a second unblock while the first one's catch-up is on its way gives back the rows of the second person that pass read while they were still blocked", async () => {
        const room = platformRoom(id => id === 4 ? dee : id === 6 ? sam : ada);
        room.blocked.add(sam);
        room.newest = 3;
        const { timelines, blocks } = masked(room.fetchPage, { page: 2, max: 3 });
        blocks.setAliases([dee, sam]);
        await openAsShell(timelines, channel);
        room.count = 8;
        timelines.applyEvent(channel, "message.created", liveRow(room.row(7)));
        timelines.applyEvent(channel, "message.created", liveRow(room.row(8)));

        // Dee let go, and the resync's catch-up held at its second page
        room.hold = options => options.afterId === 7;
        room.unblock(blocks, dee);
        timelines.release([dee]);
        await timelines.loadNewest(channel);
        const first = timelines.onJoined(channel);
        await flush();

        // while it waits, two live rows, and Sam let go
        room.count = 10;
        timelines.applyEvent(channel, "message.created", liveRow(room.row(9)));
        timelines.applyEvent(channel, "message.created", liveRow(room.row(10)));
        room.unblock(blocks, sam);
        timelines.release([sam]);
        await timelines.loadNewest(channel);
        const second = timelines.onJoined(channel);
        room.held.shift()?.();
        await first;
        await second;
        await flush();

        expect(withheldIds(timelines, channel)).toEqual([]);
    });

    test("an unblock in a room left again and again before its catch-up reads, on its next open, no more than a bound and a reread's worth", async () => {
        const room = platformRoom(id => id <= 6 ? dee : ada);
        const { timelines, blocks } = masked(room.fetchPage, { page: 2, max: 3 });
        blocks.setAliases([dee]);
        await openAsShell(timelines, channel);
        timelines.leave(channel);
        room.newest = 3;
        room.unblock(blocks, dee);
        timelines.release([dee]);

        // each time: two live rows, a newest page that joins what is held, and left before its catch-up
        for (let i = 0; i < 6; i++) {
            room.count += 2;
            timelines.applyEvent(channel, "message.created", liveRow(room.row(room.count - 1)));
            timelines.applyEvent(channel, "message.created", liveRow(room.row(room.count)));
            await timelines.loadNewest(channel);
            timelines.leave(channel);
        }
        room.served = 0;
        await openAsShell(timelines, channel);

        // the newest page's 3, the catch-up's bound of 3 and the reread's 4: two pages past a bound of 3
        expect(room.served).toBeLessThanOrEqual(10);
        expect(withheldIds(timelines, channel)).toEqual([]);
    });

    test("an unblock whose reread is on its way when the room is left lets nothing go, and the next open gives every row back", async () => {
        const room = platformRoom(() => dee);
        const { timelines, blocks } = masked(room.fetchPage, { page: 2, max: 3 });
        blocks.setAliases([dee]);
        await openAsShell(timelines, channel);
        room.newest = 1;
        room.hold = options => options.beforeId !== undefined;
        room.unblock(blocks, dee);
        timelines.release([dee]);
        await timelines.loadNewest(channel);
        const joining = timelines.onJoined(channel);
        await flush();
        expect(readsBefore(room.reads)).toEqual([4]);

        timelines.leave(channel);
        room.held.shift()?.();
        await joining;
        await flush();

        expect(ids(timelines.state(channel).messages)).toEqual([1, 2, 3, 4, 5, 6]);
        expect(withheldIds(timelines, channel)).toEqual([1, 2, 3]);

        await openAsShell(timelines, channel);

        expect(ids(timelines.state(channel).messages)).toEqual([1, 2, 3, 4, 5, 6]);
        expect(withheldIds(timelines, channel)).toEqual([]);
    });

    test("an unblock's reread that fails for a moment waits and tries again, without saying the room is behind, and gives every row back", async () => {
        const room = platformRoom(() => dee);
        const waiting: Array<() => void> = [];
        const { timelines, blocks } = masked(room.fetchPage, { page: 2, max: 3 }, { sleep: () => new Promise<void>(resolve => waiting.push(resolve)) });
        blocks.setAliases([dee]);
        await openAsShell(timelines, channel);
        room.newest = 1;
        let failures = 1;
        room.fail = options => options.beforeId !== undefined && failures-- > 0 ? { message: "rpc.unavailable", status: 503 } : undefined;
        room.unblock(blocks, dee);
        timelines.release([dee]);
        await timelines.loadNewest(channel);
        const joining = timelines.onJoined(channel);
        await flush();

        // caught up on what is new, so the room is not behind while the reread waits
        expect(waiting).toHaveLength(1);
        expect(timelines.state(channel).isBehind).toBe(false);
        waiting.shift()?.();
        await joining;
        await flush();

        expect(readsBefore(room.reads)).toEqual([4, 4, 2]);
        expect(ids(timelines.state(channel).messages)).toEqual([1, 2, 3, 4, 5, 6]);
        expect(withheldIds(timelines, channel)).toEqual([]);
    });

    test("an unblock's reread refused ends the catch-up as a refusal does, and lets nothing go", async () => {
        const room = platformRoom(() => dee);
        const waiting: Array<() => void> = [];
        const { timelines, blocks } = masked(room.fetchPage, { page: 2, max: 3 }, { sleep: () => new Promise<void>(resolve => waiting.push(resolve)) });
        blocks.setAliases([dee]);
        await openAsShell(timelines, channel);
        room.newest = 1;
        room.fail = options => options.beforeId !== undefined ? { message: "authz.not_member", status: 403 } : undefined;
        room.unblock(blocks, dee);
        timelines.release([dee]);
        await timelines.loadNewest(channel);

        await expect(timelines.onJoined(channel)).rejects.toEqual({ message: "authz.not_member", status: 403 });

        expect(waiting).toHaveLength(0);
        expect(ids(timelines.state(channel).messages)).toEqual([1, 2, 3, 4, 5, 6]);
        expect(withheldIds(timelines, channel)).toEqual([1, 2, 3]);
    });

    test("an unblock lets go of a withheld row the platform no longer has, so no later catch-up reads for it again", async () => {
        const room = platformRoom(() => dee);
        const { timelines, blocks } = masked(room.fetchPage, { page: 2, max: 3 });
        blocks.setAliases([dee]);
        await openAsShell(timelines, channel);
        room.newest = 1;
        room.gone = new Set([2, 3]);
        room.unblock(blocks, dee);
        timelines.release([dee]);
        await openAsShell(timelines, channel);

        expect(ids(timelines.state(channel).messages)).toEqual([1, 4, 5, 6]);
        expect(withheldIds(timelines, channel)).toEqual([]);

        const asked = room.reads.length;
        await timelines.onJoined(channel);
        await flush();
        expect(readsBefore(room.reads.slice(asked))).toEqual([]);
    });

    test("a person blocked again while their rows are read again is not read for: answers with their words land withheld, and no later catch-up rereads", async () => {
        const room = platformRoom(id => id <= 6 || id === 7 ? dee : ada);
        const { timelines, blocks } = masked(room.fetchPage, { page: 2, max: 3 });
        blocks.setAliases([dee]);
        await openAsShell(timelines, channel);
        room.newest = 1;
        room.hold = options => options.beforeId !== undefined;
        room.unblock(blocks, dee);
        timelines.release([dee]);
        await timelines.loadNewest(channel);
        const joining = timelines.onJoined(channel);
        await flush();
        expect(readsBefore(room.reads)).toEqual([4]);

        // the client hears the block first; the page on its way was served before the platform's
        blocks.setAliases([dee]);
        const asked = room.reads.length;
        room.held.shift()?.();
        await joining;
        await flush();
        room.blocked.add(dee);
        room.count = 7;
        timelines.applyEvent(channel, "message.created", liveRow(deeMessage(7)));
        await timelines.onJoined(channel);
        await flush();

        expect(readsBefore(room.reads.slice(asked))).toEqual([]);
        expect(said(timelines.state(channel).messages).every(line => line.endsWith(":null:null"))).toBe(true);
        expect(withheldIds(timelines, channel)).toEqual(ids(timelines.state(channel).messages).filter(id => id <= 7));
    });

    test("a row the platform still withholds after an unblock, for an alias still blocked, is read again once, not by every catch-up", async () => {
        const room = platformRoom(() => dee);
        const { timelines, blocks } = masked(room.fetchPage, { page: 2, max: 3 });
        blocks.setAliases([dee]);
        await openAsShell(timelines, channel);
        room.newest = 1;
        room.stillWithheld.add(2);
        room.unblock(blocks, dee);
        timelines.release([dee]);
        await openAsShell(timelines, channel);

        expect(readsBefore(room.reads)).toEqual([4, 2]);
        expect(withheldIds(timelines, channel)).toEqual([2]);

        const asked = room.reads.length;
        await timelines.onJoined(channel);
        await flush();
        expect(readsBefore(room.reads.slice(asked))).toEqual([]);
    });

    test("an unblock gives back his quote in someone else's row past the catch-up's span", async () => {
        const room = platformRoom(() => ada, id => id === 2);
        const { timelines, blocks } = masked(room.fetchPage, { page: 2, max: 3 });
        blocks.setAliases([dee]);
        await openAsShell(timelines, channel);
        expect(withheldIds(timelines, channel)).toEqual([2]);
        room.newest = 1;
        room.unblock(blocks, dee);
        timelines.release([dee]);
        await openAsShell(timelines, channel);

        expect(readsBefore(room.reads)).toEqual([3]);
        expect(withheldIds(timelines, channel)).toEqual([]);
        expect(timelines.state(channel).messages[1].quoted).toMatchObject({ body: "dee said", sender_nick_name: "Dee" });
    });

    test("an unblock gives back his live row the client withheld, which no page had read", async () => {
        const room = platformRoom(id => id === 7 ? dee : ada);
        const { timelines, blocks } = masked(room.fetchPage, { page: 2, max: 3 });
        blocks.setAliases([dee]);
        await openAsShell(timelines, channel);
        room.count = 8;
        timelines.applyEvent(channel, "message.created", liveRow(deeMessage(7)));
        timelines.applyEvent(channel, "message.created", liveRow(message(8)));
        expect(withheldIds(timelines, channel)).toEqual([7]);

        room.unblock(blocks, dee);
        timelines.release([dee]);
        await timelines.onJoined(channel);
        await flush();

        expect(withheldIds(timelines, channel)).toEqual([]);
        expect(said(timelines.state(channel).messages)).toEqual(["7:dee 7:Dee"]);
    });

    test("a join confirmed while an unblock's reread is on its way is caught up after it", async () => {
        const room = platformRoom(id => id <= 6 ? dee : ada);
        const { timelines, blocks } = masked(room.fetchPage, { page: 2, max: 3 });
        blocks.setAliases([dee]);
        await openAsShell(timelines, channel);
        room.newest = 1;
        room.hold = options => options.beforeId !== undefined;
        room.unblock(blocks, dee);
        timelines.release([dee]);
        await timelines.loadNewest(channel);
        const joining = timelines.onJoined(channel);
        await flush();
        expect(readsBefore(room.reads)).toEqual([4]);

        // a message committed while the socket was away, which only that join's catch-up reads
        room.count = 7;
        const rejoining = timelines.onJoined(channel);
        room.held.shift()?.();
        await joining;
        await rejoining;
        await flush();

        expect(ids(timelines.state(channel).messages)).toEqual([1, 2, 3, 4, 5, 6, 7]);
        expect(withheldIds(timelines, channel)).toEqual([]);
    });

    test("a stop while an unblock's reread is on its way reads nothing more and writes nothing", async () => {
        const room = platformRoom(() => dee);
        const { timelines, blocks } = masked(room.fetchPage, { page: 2, max: 3 });
        blocks.setAliases([dee]);
        await openAsShell(timelines, channel);
        room.newest = 1;
        room.hold = options => options.beforeId !== undefined;
        room.unblock(blocks, dee);
        timelines.release([dee]);
        await timelines.loadNewest(channel);
        const joining = timelines.onJoined(channel);
        await flush();
        expect(readsBefore(room.reads)).toEqual([4]);

        timelines.stop();
        const asked = room.reads.length;
        room.held.shift()?.();
        await joining;
        await flush();

        expect(room.reads.length).toBe(asked);
        expect(ids(timelines.state(channel).messages)).toEqual([1, 2, 3, 4, 5, 6]);
        expect(withheldIds(timelines, channel)).toEqual([1, 2, 3]);
    });
});

// A room's revision: every history event on its topic carries the room's new revision, and pages
// and the sidebar carry the revision they were read at, so a client that sees a revision skip
// knows it missed a change and repairs that room alone. Reactions carry none.
describe("the room's revision", () => {
    /** A page as the platform answers it, with the revision read before its rows. */
    function revPage(pageIds: number[], rev: number, extra: Partial<HistoryPage> = {}): HistoryPage {
        return page(pageIds, { rev, ...extra });
    }

    /** The timers the timelines start, run or cancelled by hand. */
    type Timer = { run: () => void, ms: number, isCancelled: boolean };

    function timers(): { list: Timer[], schedule: (run: () => void, ms: number) => () => void, live: () => Timer[], fire: (ms: number) => void, runAll: () => void } {
        const list: Timer[] = [];
        return {
            list,
            schedule: (run, ms) => {
                const timer: Timer = { run, ms, isCancelled: false };
                list.push(timer);
                return () => {
                    timer.isCancelled = true;
                };
            },
            live: () => list.filter(timer => !timer.isCancelled),
            fire: ms => {
                for (const timer of list.filter(t => !t.isCancelled && t.ms === ms)) {
                    timer.isCancelled = true;
                    timer.run();
                }
            },
            runAll: () => {
                for (const timer of list.filter(t => !t.isCancelled)) {
                    timer.isCancelled = true;
                    timer.run();
                }
            }
        };
    }

    type Fetch = { limit: number, beforeId?: number, afterId?: number };

    /**
     * Timelines over a queue of answers, each a page or a deferred one, with the timers, the clock
     * and the random draw in the test's hands. The rules are the cells' own, so no cell moves when
     * a default does.
     */
    function setup(answers: Array<HistoryPage | Deferred<HistoryPage>>, options: { max?: number, page?: number, now?: () => number, random?: () => number, onGap?: (channelId: string) => void } = {}): {
        timelines: ReturnType<typeof createTimelines>,
        fetches: Fetch[],
        clock: ReturnType<typeof timers>,
        reactions: Array<{ event: string, payload: unknown }>
    } {
        const fetches: Fetch[] = [];
        const clock = timers();
        const reactions: Array<{ event: string, payload: unknown }> = [];
        const timelines = createTimelines({
            mask: platformMask,
            limits,
            catchUp: () => ({ page: options.page ?? 100, max: options.max ?? 500 }),
            revRules: () => ({ reorderMs: 500, repairWaitMs: 5000, staleMs: 900000 }),
            schedule: clock.schedule,
            now: options.now ?? (() => 0),
            random: options.random ?? (() => 0.5),
            onGap: options.onGap,
            applyReaction: (_c, event, payload) => reactions.push({ event, payload }),
            fetchPage: (_c, asked) => {
                fetches.push(asked);
                const next = answers.shift();
                if (!next) {
                    return Promise.reject(new Error("no answer queued"));
                }
                return "promise" in next ? next.promise : Promise.resolve(next);
            }
        });
        return { timelines, fetches, clock, reactions };
    }

    /** What the shell's router does with each channel event: the revision first, then the event. */
    function deliver(timelines: ReturnType<typeof createTimelines>, event: string, payload: Record<string, unknown>): void {
        timelines.advance(channel, payload);
        timelines.applyEvent(channel, event, payload);
    }

    function edited(id: number, body: string, rev?: number): Record<string, unknown> {
        return { id, channel_id: channel, body, pieces: null, edited_at: `2026-09-23T11:00:${String(id % 60).padStart(2, "0")}Z`, ...(rev === undefined ? {} : { rev }) };
    }

    async function settle(): Promise<void> {
        for (let i = 0; i < 10; i++) {
            await Promise.resolve();
        }
    }

    test("an event one above what is applied is applied and advances the room's revision", async () => {
        const { timelines, clock } = setup([revPage([1, 2, 3], 5)]);
        await timelines.loadNewest(channel);
        expect(timelines.state(channel).rev).toBe(5);

        deliver(timelines, "message.edited", edited(2, "changed", 6));

        expect(timelines.state(channel).messages[1].body).toBe("changed");
        expect(timelines.state(channel).rev).toBe(6);
        expect(clock.live()).toEqual([]);
    });

    test("a hole that fills within the reorder wait repairs nothing", async () => {
        const { timelines, fetches, clock } = setup([revPage([1, 2, 3], 5)]);
        await timelines.loadNewest(channel);

        deliver(timelines, "message.edited", edited(3, "second", 7));
        expect(timelines.state(channel).rev).toBe(5);
        expect(clock.live().map(timer => timer.ms)).toEqual([500]);

        deliver(timelines, "message.edited", edited(2, "first", 6));
        expect(timelines.state(channel).rev).toBe(7);
        expect(clock.live()).toEqual([]);
        clock.runAll();
        await settle();

        expect(fetches.length).toBe(1);
    });

    test("a hole that opens as an earlier one fills gets a whole reorder wait of its own", async () => {
        const { timelines, clock } = setup([revPage([1, 2, 3], 5)]);
        await timelines.loadNewest(channel);

        deliver(timelines, "message.edited", edited(2, "x", 7));
        deliver(timelines, "message.edited", edited(3, "y", 9));
        deliver(timelines, "message.edited", edited(1, "z", 6));

        expect(timelines.state(channel).rev).toBe(7);
        expect(clock.list.map(timer => timer.isCancelled)).toEqual([true, false]);
    });

    test("a hole still open after the reorder wait is repaired once, after a random wait, and later gaps share the repair", async () => {
        const gaps: string[] = [];
        const { timelines, fetches, clock } = setup([revPage([1, 2, 3], 5), revPage([1, 2, 3, 4], 9)], { onGap: id => gaps.push(id) });
        await timelines.loadNewest(channel);

        deliver(timelines, "message.edited", edited(2, "x", 7));
        clock.fire(500);
        // the random wait: half of the longest, as the injected random says
        expect(clock.live().map(timer => timer.ms)).toEqual([2500]);

        deliver(timelines, "message.edited", edited(3, "y", 9));
        clock.fire(500);
        expect(clock.live().map(timer => timer.ms)).toEqual([2500]);

        clock.fire(2500);
        await settle();

        // the repair is the catch-up the timeline has: the held run read again from its oldest
        expect(fetches).toEqual([{ limit: firstPageSize }, { limit: 100, afterId: 0 }]);
        expect(ids(timelines.state(channel).messages)).toEqual([1, 2, 3, 4]);
        expect(timelines.state(channel).rev).toBe(9);
        expect(gaps).toEqual([channel, channel]);
    });

    test("the wait before a repair is drawn afresh each time across the whole window, not fixed within it", async () => {
        const draws = [0, 0.999];
        const { timelines, clock } = setup([revPage([1, 2, 3], 5), revPage([1, 2, 3], 7), revPage([1, 2, 3], 9)], { random: () => draws.shift() ?? 0.5 });
        await timelines.loadNewest(channel);

        deliver(timelines, "message.edited", edited(2, "x", 7));
        clock.fire(500);
        expect(clock.live().map(timer => timer.ms)).toEqual([0]);
        clock.fire(0);
        await settle();

        deliver(timelines, "message.edited", edited(3, "y", 9));
        clock.fire(500);
        expect(clock.live().map(timer => timer.ms)).toEqual([4995]);
    });

    test("a page at a revision does not undo a group changed above it, and overwrites one at or below it, for the message and its thread counts", async () => {
        const root = (body: string, replies: number): TimelineMessage => ({ ...message(1, body), reply_count: replies, participant_count: 1, last_reply_at: "2026-09-23T10:00:00Z", repliers: [] });
        const older = { messages: [message(2, "old"), root("root", 1)], read_cursor: null, unread_count: 0, has_more: false, rev: 6 };
        const newer = { messages: [{ ...message(2, "newest"), edited_at: "2026-09-23T12:00:00Z" }, root("root", 4)], read_cursor: null, unread_count: 0, has_more: false, rev: 9 };
        const { timelines } = setup([revPage([1, 2], 5), older, newer]);
        await timelines.loadNewest(channel);

        deliver(timelines, "message.edited", edited(2, "new", 7));
        deliver(timelines, "thread.updated", { channel_id: channel, root_id: 1, reply_count: 3, participant_count: 2, last_reply_at: "2026-09-23T10:05:00Z", repliers: [], rev: 8 });

        // read at 6, before both changes: neither is undone
        await timelines.loadNewest(channel);
        let [first, second] = timelines.state(channel).messages;
        expect(second.body).toBe("new");
        expect(first.reply_count).toBe(3);

        // read at 9, after both: it wins
        await timelines.loadNewest(channel);
        [first, second] = timelines.state(channel).messages;
        expect(second.body).toBe("newest");
        expect(first.reply_count).toBe(4);
    });

    test("an event at or below what a page set for a message is not laid over it", async () => {
        const { timelines } = setup([revPage([1, 2], 8)]);
        await timelines.loadNewest(channel);

        deliver(timelines, "message.edited", edited(2, "late", 7));

        expect(timelines.state(channel).messages[1].body).toBe("m2");
    });

    test("a message created before an edit that arrived first still lands, with the edit laid over it", async () => {
        const { timelines } = setup([revPage([1], 5)]);
        await timelines.loadNewest(channel);

        deliver(timelines, "message.edited", edited(2, "edited", 7));
        deliver(timelines, "message.created", { ...message(2, "first words"), channel_id: channel, parent_id: null, rev: 6 });

        expect(timelines.state(channel).messages[1].body).toBe("edited");
        expect(timelines.state(channel).rev).toBe(7);
    });

    test("a reaction carries no revision: held before a page it is skipped by arrival time, after it it passes to the reactions, and it never moves the revision", async () => {
        const { timelines, reactions, clock } = setup([revPage([1], 5)]);
        const reaction = { message_id: 1, channel_id: channel, code: "thumbsup", person_alias_guid: "a0000009-0000-4000-8000-000000000000", reactors: [], count: 1 };
        deliver(timelines, "reaction.added", reaction);
        await timelines.loadNewest(channel);
        expect(reactions).toEqual([]);

        deliver(timelines, "reaction.added", { ...reaction, count: 2 });
        expect(reactions).toEqual([{ event: "reaction.added", payload: { ...reaction, count: 2 } }]);
        expect(timelines.state(channel).rev).toBe(5);
        expect(clock.live()).toEqual([]);
    });

    test("an event name it does not know, with a revision, advances and never leaves a gap", async () => {
        const { timelines, clock } = setup([revPage([1], 5)]);
        await timelines.loadNewest(channel);

        deliver(timelines, "pin.added", { id: 1, channel_id: channel, rev: 6 });
        deliver(timelines, "message.edited", edited(1, "after", 7));

        expect(timelines.state(channel).rev).toBe(7);
        expect(clock.live()).toEqual([]);
    });

    test("an event with no revision moves nothing", async () => {
        const { timelines, clock } = setup([revPage([1], 5)]);
        await timelines.loadNewest(channel);

        deliver(timelines, "message.edited", edited(1, "unnumbered"));

        expect(timelines.state(channel).rev).toBe(5);
        expect(clock.live()).toEqual([]);
    });

    test("a jump above the catch-up bound reloads the newest page instead of reading the held run again", async () => {
        const { timelines, fetches, clock } = setup([revPage([1, 2, 3], 5), revPage([40, 41], 30, { has_more: true }), revPage([], 30)], { max: 10 });
        await timelines.loadNewest(channel);

        deliver(timelines, "message.edited", edited(41, "far ahead", 30));
        clock.fire(500);
        clock.fire(2500);
        await settle();

        expect(fetches.slice(0, 2)).toEqual([{ limit: firstPageSize }, { limit: firstPageSize }]);
        expect(ids(timelines.state(channel).messages)).toEqual([40, 41]);
        expect(timelines.state(channel).rev).toBe(30);
    });

    test("a page whose revision went backwards reloads the newest page, which is taken as it stands", async () => {
        const { timelines, fetches, clock } = setup([revPage([1, 2, 3], 50), revPage([1, 2, 3], 40), revPage([7, 8], 41, { has_more: true }), revPage([], 41)]);
        await timelines.loadNewest(channel);

        deliver(timelines, "message.edited", edited(2, "x", 52));
        clock.fire(500);
        clock.fire(2500);
        await settle();

        expect(fetches.slice(0, 3)).toEqual([{ limit: firstPageSize }, { limit: 100, afterId: 0 }, { limit: firstPageSize }]);
        expect(ids(timelines.state(channel).messages)).toEqual([7, 8]);
        expect(timelines.state(channel).rev).toBe(41);
    });

    test("a room with nothing confirmed for longer than the stale wait reloads the newest page on a gap", async () => {
        let now = 0;
        const { timelines, fetches, clock } = setup([revPage([1, 2, 3], 5), revPage([9], 12, { has_more: true }), revPage([], 12)], { now: () => now });
        await timelines.loadNewest(channel);

        now = 16 * 60 * 1000;
        deliver(timelines, "message.edited", edited(9, "much later", 12));
        clock.fire(500);
        clock.fire(2500);
        await settle();

        expect(fetches[1]).toEqual({ limit: firstPageSize });
        expect(ids(timelines.state(channel).messages)).toEqual([9]);
    });

    test("a sidebar read before live events that arrived meanwhile is not backwards, and one below what was held when asked is", async () => {
        const { timelines, fetches, clock } = setup([revPage([1, 2, 3], 5), revPage([4], 3, { has_more: true }), revPage([], 3)]);
        await timelines.loadNewest(channel);
        const asked = timelines.revsHeld();

        deliver(timelines, "message.edited", edited(2, "meanwhile", 6));
        timelines.noteSidebar(channel, 5, asked.get(channel) ?? null, true);
        expect(clock.live()).toEqual([]);

        timelines.noteSidebar(channel, 3, asked.get(channel) ?? null, true);
        clock.runAll();
        await settle();
        expect(fetches[1]).toEqual({ limit: firstPageSize });
        expect(timelines.state(channel).rev).toBe(3);
    });

    test("a sidebar row ahead of the open room repairs it, leaves a held room not on screen to read itself again on its next open, and never reads a room with no timeline", async () => {
        const other = "c0000002-0000-4000-8000-000000000000";
        const never = "c0000003-0000-4000-8000-000000000000";
        const { timelines, fetches, clock } = setup([revPage([1, 2], 5), revPage([1, 2], 4), revPage([], 4), revPage([1, 2, 3], 7), revPage([1, 2], 9), revPage([1, 2], 9)]);
        await timelines.loadNewest(channel);
        await timelines.loadNewest(other);
        await timelines.onJoined(other);
        timelines.leave(other);
        const asked = timelines.revsHeld();

        timelines.noteSidebar(other, 9, asked.get(other) ?? null, false);
        timelines.noteSidebar(never, 3, asked.get(never) ?? null, false);
        expect(fetches.length).toBe(3);
        expect(clock.live()).toEqual([]);

        timelines.noteSidebar(channel, 7, asked.get(channel) ?? null, true);
        clock.runAll();
        await settle();
        expect(fetches[3]).toEqual({ limit: 100, afterId: 0 });
        expect(timelines.state(channel).rev).toBe(7);

        // the room not on screen, opened again, reads its newest page and then its held run
        await timelines.loadNewest(other);
        await timelines.onJoined(other);
        expect(fetches.slice(4)).toEqual([{ limit: firstPageSize }, { limit: 100, afterId: 0 }]);
        expect(timelines.state(other).rev).toBe(9);
    });

    test("a rejoin of the open room whose forward page answers the revision applied does not read the held run again", async () => {
        const { timelines, fetches } = setup([revPage([1, 2, 3, 4, 5], 5), revPage([], 5), revPage([], 5)], { page: 2 });
        await timelines.loadNewest(channel);
        await timelines.onJoined(channel);

        await timelines.onJoined(channel);

        expect(fetches).toEqual([{ limit: firstPageSize }, { limit: 2, afterId: 5 }, { limit: 2, afterId: 5 }]);
    });

    test("a rejoin of the open room whose revision moved reads the held run again", async () => {
        const { timelines, fetches } = setup([revPage([1, 2, 3], 5), revPage([], 5), revPage([], 6), revPage([1, 2, 3], 6)]);
        await timelines.loadNewest(channel);
        await timelines.onJoined(channel);

        await timelines.onJoined(channel);

        expect(fetches).toEqual([{ limit: firstPageSize }, { limit: 100, afterId: 3 }, { limit: 100, afterId: 3 }, { limit: 100, afterId: 0 }]);
        expect(timelines.state(channel).rev).toBe(6);
    });

    test("a reopen of a visited room reads the held run again even when its revision has not moved", async () => {
        const { timelines, fetches } = setup([revPage([1, 2, 3], 5), revPage([], 5), revPage([1, 2, 3], 5), revPage([1, 2, 3], 5)]);
        await timelines.loadNewest(channel);
        await timelines.onJoined(channel);
        timelines.leave(channel);

        await timelines.loadNewest(channel);
        await timelines.onJoined(channel);

        expect(fetches).toEqual([{ limit: firstPageSize }, { limit: 100, afterId: 3 }, { limit: firstPageSize }, { limit: 100, afterId: 0 }]);
    });

    test("a reopen after the room's revision ran past the catch-up bound reloads the newest page rather than read the held run", async () => {
        const other = "c0000002-0000-4000-8000-000000000000";
        const { timelines, fetches } = setup([revPage([1, 2, 3], 5), revPage([90, 91], 600, { has_more: true }), revPage([], 600)]);
        await timelines.loadNewest(other);
        timelines.leave(other);
        timelines.noteSidebar(other, 600, 5, false);

        await timelines.loadNewest(other);
        await timelines.onJoined(other);

        expect(ids(timelines.state(other).messages)).toEqual([90, 91]);
        expect(fetches[2]).toEqual({ limit: 100, afterId: 91 });
        expect(timelines.state(other).rev).toBe(600);
    });

    test("leaving the room cancels the reorder wait, and a stop cancels the repair's", async () => {
        const { timelines, clock } = setup([revPage([1, 2, 3], 5)]);
        await timelines.loadNewest(channel);

        deliver(timelines, "message.edited", edited(2, "x", 7));
        expect(clock.live().length).toBe(1);
        timelines.leave(channel);
        expect(clock.live()).toEqual([]);
    });

    test("a stop cancels a repair waiting to run", async () => {
        const { timelines, fetches, clock } = setup([revPage([1, 2, 3], 5)]);
        await timelines.loadNewest(channel);

        deliver(timelines, "message.edited", edited(2, "x", 7));
        clock.fire(500);
        expect(clock.live().map(timer => timer.ms)).toEqual([2500]);
        timelines.stop();

        expect(clock.live()).toEqual([]);
        expect(fetches.length).toBe(1);
    });

    test("a repair reading the newest page, overtaken by a leave, starts nothing on the room left, and the reopen reads the held run again", async () => {
        const newest = deferred<HistoryPage>();
        const { timelines, fetches, clock } = setup([revPage([1, 2, 3], 5), newest, revPage([1, 2, 3], 30), revPage([1, 2, 3], 30)], { max: 10 });
        await timelines.loadNewest(channel);
        deliver(timelines, "message.edited", edited(3, "far ahead", 30));
        clock.fire(500);
        clock.fire(2500);
        await settle();
        expect(fetches).toEqual([{ limit: firstPageSize }, { limit: firstPageSize }]);

        timelines.leave(channel);
        newest.resolve(revPage([40, 41], 30, { has_more: true }));
        await settle();
        expect(fetches.length).toBe(2);

        await timelines.loadNewest(channel);
        await timelines.onJoined(channel);
        expect(fetches.slice(2)).toEqual([{ limit: firstPageSize }, { limit: 100, afterId: 0 }]);
        expect(timelines.state(channel).rev).toBe(30);
    });

    test("a repair waiting on a rejoin's catch-up, overtaken by a leave, starts nothing on the room left", async () => {
        const reread = deferred<HistoryPage>();
        const { timelines, fetches, clock } = setup([revPage([1, 2, 3], 5), revPage([], 5), reread, revPage([1, 2, 3], 7)]);
        await timelines.loadNewest(channel);
        await timelines.onJoined(channel);
        deliver(timelines, "message.edited", edited(2, "x", 7));
        clock.fire(500);

        const rejoining = timelines.onJoined(channel);
        await settle();
        clock.fire(2500);
        await settle();
        timelines.leave(channel);
        reread.resolve(revPage([1, 2, 3], 7));
        await rejoining;
        await settle();

        expect(fetches.length).toBe(3);
    });

    test("a newest page read for a jump keeps the revisions that arrived while it was out, so the next event finds no hole", async () => {
        const newest = deferred<HistoryPage>();
        const { timelines, clock } = setup([revPage([1, 2, 3], 5), newest, revPage([], 601)]);
        await timelines.loadNewest(channel);
        deliver(timelines, "message.edited", edited(3, "far ahead", 600));
        clock.fire(500);
        clock.fire(2500);
        await settle();

        deliver(timelines, "message.edited", edited(90, "meanwhile", 601));
        newest.resolve(revPage([90, 91], 600, { has_more: true }));
        await settle();
        deliver(timelines, "message.edited", edited(91, "after", 602));

        expect(timelines.state(channel).rev).toBe(602);
        expect(clock.live()).toEqual([]);
    });

    test("a reload for a history that started again forgets the revisions seen before it was asked and keeps those that arrived while it was out", async () => {
        const newest = deferred<HistoryPage>();
        const { timelines, clock } = setup([revPage([1, 2, 3], 50), revPage([1, 2, 3], 40), newest, revPage([], 40)]);
        await timelines.loadNewest(channel);
        deliver(timelines, "message.edited", edited(2, "before", 52));
        clock.fire(500);
        clock.fire(2500);
        await settle();

        deliver(timelines, "message.edited", edited(3, "meanwhile", 53));
        newest.resolve(revPage([7, 8], 40, { has_more: true }));
        await settle();
        for (let rev = 41; rev <= 52; rev++) {
            deliver(timelines, "pin.added", { id: 7, channel_id: channel, rev });
        }

        expect(timelines.state(channel).rev).toBe(53);
    });

    test("a repair that fires while a rejoin reads the held run again waits for it, and reads nothing more once the room is current", async () => {
        const reread = deferred<HistoryPage>();
        const { timelines, fetches, clock } = setup([revPage([1, 2, 3], 5), revPage([], 5), revPage([], 8), reread, revPage([1, 2, 3], 8)]);
        await timelines.loadNewest(channel);
        await timelines.onJoined(channel);
        const asked = timelines.revsHeld();

        const rejoining = timelines.onJoined(channel);
        await settle();
        timelines.noteSidebar(channel, 8, asked.get(channel) ?? null, true);
        clock.fire(2500);
        await settle();
        reread.resolve(revPage([1, 2, 3], 8));
        await rejoining;
        await settle();

        expect(fetches).toEqual([{ limit: firstPageSize }, { limit: 100, afterId: 3 }, { limit: 100, afterId: 3 }, { limit: 100, afterId: 0 }]);
        expect(timelines.state(channel).rev).toBe(8);
    });

    test("a repair waiting to run reads nothing when a rejoin's reread has caught the room up past the hole", async () => {
        const { timelines, fetches, clock } = setup([revPage([1, 2, 3], 5), revPage([], 5), revPage([1, 2, 3], 7), revPage([1, 2, 3], 7)]);
        await timelines.loadNewest(channel);
        await timelines.onJoined(channel);
        deliver(timelines, "message.edited", edited(2, "x", 7));
        clock.fire(500);
        expect(clock.live().map(timer => timer.ms)).toEqual([2500]);

        await timelines.onJoined(channel);
        expect(timelines.state(channel).rev).toBe(7);
        clock.fire(2500);
        await settle();

        expect(fetches.length).toBe(3);
    });

    test("a first join whose forward page says the room moved since the first page reads the held run again, so nothing is left for the live path to find", async () => {
        const gaps: string[] = [];
        const { timelines, fetches, clock } = setup([revPage([1, 2, 3], 5), revPage([4], 6), revPage([1, 2, 3, 4], 6)], { onGap: id => gaps.push(id) });
        await timelines.loadNewest(channel);

        await timelines.onJoined(channel);
        deliver(timelines, "message.edited", edited(4, "x", 7));

        expect(fetches).toEqual([{ limit: firstPageSize }, { limit: 100, afterId: 3 }, { limit: 100, afterId: 0 }]);
        expect(timelines.state(channel).rev).toBe(7);
        expect(clock.live()).toEqual([]);
        expect(gaps).toEqual([]);
    });

    test("a room unconfirmed past the stale wait reads its newest page first, and one that joins what is held keeps every row and reads the held run again", async () => {
        let now = 0;
        const { timelines, fetches, clock } = setup([revPage([1, 2, 3], 5), revPage([2, 3, 4], 7, { has_more: true }), revPage([1, 2, 3, 4], 7)], { now: () => now });
        await timelines.loadNewest(channel);

        now = 16 * 60 * 1000;
        deliver(timelines, "message.edited", edited(2, "x", 7));
        clock.fire(500);
        clock.fire(2500);
        await settle();

        expect(fetches.slice(1)).toEqual([{ limit: firstPageSize }, { limit: 100, afterId: 0 }]);
        expect(ids(timelines.state(channel).messages)).toEqual([1, 2, 3, 4]);
        expect(timelines.state(channel).rev).toBe(7);
    });

    test("an older change that arrives after a newer one of the same message still sets the fields the newer one did not: a late hide after an edit hides", async () => {
        const { timelines } = setup([revPage([1, 2], 10)]);
        await timelines.loadNewest(channel);

        deliver(timelines, "message.edited", edited(2, "later words", 12));
        deliver(timelines, "message.visibility", { id: 2, channel_id: channel, state: "hidden", notice: "Hidden.", rev: 11 });

        const held = timelines.state(channel).messages[1];
        expect(held.visibility).toBe("hidden");
        expect(held.body).toBeNull();
        expect(timelines.state(channel).rev).toBe(12);
    });

    test("a late hide after an unhide sets only what the unhide did not: the unhide's visibility and words are laid back over it", async () => {
        const { timelines } = setup([revPage([1, 2], 10)]);
        await timelines.loadNewest(channel);

        deliver(timelines, "message.visibility", { id: 2, channel_id: channel, state: "visible", notice: null, body: "back", rev: 12 });
        // the hide also states the metadata the unhide does not, so it is taken, under what the unhide set
        deliver(timelines, "message.visibility", { id: 2, channel_id: channel, state: "hidden", notice: "Hidden.", rev: 11 });

        const held = timelines.state(channel).messages[1];
        expect(held.visibility).toBe("visible");
        expect(held.body).toBe("back");
        expect(timelines.state(channel).rev).toBe(12);
    });

    test("a thread's unread and following marks, which no event's revision covers, keep the order they arrived in", async () => {
        const rootPage = (replies: number, rev: number): HistoryPage => ({ messages: [{ ...message(10), reply_count: replies, thread_following: true, thread_unread: false }], read_cursor: null, unread_count: 0, has_more: false, rev });
        const rejoin = deferred<HistoryPage>();
        const { timelines } = setup([rootPage(1, 5), revPage([], 5), revPage([], 6), rejoin]);
        await timelines.loadNewest(channel);
        await timelines.onJoined(channel);

        const rejoining = timelines.onJoined(channel);
        await settle();
        // the threads mark the root unread before the timeline records the counts
        timelines.state(channel).messages[0].thread_unread = true;
        deliver(timelines, "thread.updated", { channel_id: channel, root_id: 10, reply_count: 2, participant_count: 2, last_reply_at: "2026-09-23T10:05:00Z", repliers: [], rev: 6 });
        // a page read at the counts' revision, but before the threads' mark was recorded
        rejoin.resolve(rootPage(2, 6));
        await rejoining;

        expect(timelines.state(channel).messages[0].thread_unread).toBe(true);
        expect(timelines.state(channel).messages[0].reply_count).toBe(2);
    });
});

// One message reaches the client in three shapes: the send's answer and the live copy state none of
// its state, a page states all of it. A copy held is the same message whichever path brought it,
// so a page that only states it again changes nothing a row could read as a change.
describe("one shape for every copy of a message", () => {
    const stateOf = (held: TimelineMessage): unknown[] => [held.edited_at, held.deleted_at, held.visibility, held.visibility_notice, held.reactions, held.reply_count, held.last_reply_at, held.participant_count, held.repliers, held.thread_following, held.thread_unread];

    test("a send answered and heard live, then read again on a return as a page states it, changes no state field", async () => {
        const pages: HistoryPage[] = [{ ...pageOf([pageRow({ id: 1, sender: anna })]) }];
        const timelines = createTimelines({ mask: platformMask, limits, fetchPage: async () => pages.shift() as HistoryPage });
        await timelines.loadNewest(shapesChannel);
        timelines.upsert(shapesChannel, await sendAnswer({ id: 2, key: "k1" }));
        timelines.applyEvent(shapesChannel, "message.created", liveCreated({ id: 2, key: "k1" }));
        const before = stateOf(timelines.state(shapesChannel).messages[1]);

        timelines.leave(shapesChannel);
        pages.push(pageOf([pageRow({ id: 1, sender: anna }), pageRow({ id: 2, key: "k1" })]));
        await timelines.loadNewest(shapesChannel);

        expect(stateOf(timelines.state(shapesChannel).messages[1])).toEqual(before);
    });

    test("every key a page row states is either a state field a new message has, or one a copy keeps as stated, never both", () => {
        const stated = Object.keys(pageRow({ id: 1 })).sort();
        const classed = [...Object.keys(newMessageState), ...keptAsStated].sort();

        expect(classed).toEqual(stated);
    });

    test("each of the three shapes alone is held with the same state as a page states it", async () => {
        const shapes = [await sendAnswer({ id: 2, key: "k1" }), liveCreated({ id: 2, key: "k1" }), pageRow({ id: 2, key: "k1" })];

        const held = shapes.map(shape => stateOf(holdMessage(undefined, shape, platformMask)));

        expect(held[0]).toEqual(stateOf(pageRow({ id: 2 })));
        expect(held[1]).toEqual(held[0]);
        expect(held[2]).toEqual(held[0]);
    });

    test("a copy's chips are its own list, so a change to one copy's chips moves no other", () => {
        const first = holdMessage(undefined, liveCreated({ id: 2 }), platformMask);
        const second = holdMessage(undefined, liveCreated({ id: 2 }), platformMask);

        first.reactions?.push({ code: "heart", count: 1, mine: true, emoji: null, image_url: null, label: "Heart" });

        expect(second.reactions).toEqual([]);
    });

    test("the send's answer landing after a moderator hid the message keeps it hidden, in the timeline and in the open thread", async () => {
        const timelines = createTimelines({ mask: platformMask, limits, fetchPage: async () => pageOf([pageRow({ id: 40, sender: anna }, { reply_count: 1 })]) });
        await timelines.loadNewest(shapesChannel);
        const threadPage = { root: pageRow({ id: 40, sender: anna }, { reply_count: 1 }), messages: [], has_more: false, following: true, read_cursor: null, unread_count: 0, rev: 1 };
        const threads = createThreads({ call: async () => ({ ok: true, data: threadPage }) as never, timelines, personAliasGuid: anna, limits: () => ({ history_page_size: 50, thread_page_size: 25 }), mask: platformMask, isBlocked: () => false });
        await threads.openThread(shapesChannel, 40);
        timelines.applyEvent(shapesChannel, "message.created", liveCreated({ id: 2, key: "k1" }));
        threads.applyEvent(shapesChannel, "message.created", liveCreated({ id: 41, key: "k2", parentId: 40 }));
        const hidden = (id: number): Record<string, unknown> => ({ id, channel_id: shapesChannel, state: "hidden", notice: "Hidden by a moderator", rev: 3 });
        timelines.applyEvent(shapesChannel, "message.visibility", hidden(2));
        threads.applyEvent(shapesChannel, "message.visibility", hidden(41));

        timelines.upsert(shapesChannel, await sendAnswer({ id: 2, key: "k1" }));
        threads.upsertReply(shapesChannel, await sendAnswer({ id: 41, key: "k2", parentId: 40 }));

        expect(timelines.state(shapesChannel).messages.find(m => m.id === 2)).toMatchObject({ visibility: "hidden", body: null });
        expect(threads.state.thread?.replies.find(m => m.id === 41)).toMatchObject({ visibility: "hidden", body: null });
    });

    test("every way a copy enters the threads is held in the one shape", async () => {
        const timelines = createTimelines({ mask: platformMask, limits, fetchPage: async () => pageOf([pageRow({ id: 40, sender: anna }, { reply_count: 1 })]) });
        await timelines.loadNewest(shapesChannel);
        const reply = (id: number): Record<string, unknown> => ({ ...liveCreated({ id, sender: anna, parentId: 40 }) });
        const held = (): TimelineMessage[] => [threads.state.thread?.root as TimelineMessage, ...threads.state.thread?.replies ?? []];
        const pages = [
            { root: liveCreated({ id: 40, sender: anna }), messages: [reply(41)], has_more: true, following: true, read_cursor: null, unread_count: 0, rev: 1 },
            { root: liveCreated({ id: 40, sender: anna }), messages: [reply(39)], has_more: false, following: true, read_cursor: null, unread_count: 0, rev: 1 },
            { root: liveCreated({ id: 40, sender: anna }), messages: [reply(39), reply(41)], has_more: false, following: true, read_cursor: null, unread_count: 0, rev: 2 }
        ];
        const threads = createThreads({ call: async () => ({ ok: true, data: pages.shift() }) as never, timelines, personAliasGuid: anna, limits: () => ({ history_page_size: 50, thread_page_size: 25 }), mask: platformMask, isBlocked: () => false });
        // every state field stated, whatever path brought the copy
        const unstated = (): unknown[] => held().flatMap(stateOf).filter(value => value === undefined);

        await threads.openThread(shapesChannel, 40);
        await threads.loadOlderReplies();
        threads.applyEvent(shapesChannel, "message.created", reply(42));
        threads.upsertReply(shapesChannel, await sendAnswer({ id: 43, key: "k3", parentId: 40 }));
        expect(held()).toHaveLength(5);
        expect(unstated()).toEqual([]);

        threads.repair(shapesChannel);
        await flushPromisesOf();
        expect(unstated()).toEqual([]);
    });
});
