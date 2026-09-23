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
    rejoinPageSize
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

    test("a confirmed join fetches the newest page again and fills the gap by id", async () => {
        const answers = [page([1, 2, 3]), page([2, 3, 4, 5])];
        const fetches: { limit: number }[] = [];
        const timelines = createTimelines({
            fetchPage: async (_c, options) => {
                fetches.push(options);
                return answers.shift() as HistoryPage;
            }
        });

        await timelines.loadNewest(channel);
        await timelines.onJoined(channel);

        expect(fetches.map(f => f.limit)).toEqual([firstPageSize, rejoinPageSize]);
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

    test("a thread-only reply on the channel topic is not a timeline message", async () => {
        const timelines = createTimelines({ fetchPage: async () => page([1]) });
        await timelines.loadNewest(channel);

        timelines.applyEvent(channel, "message.created", { ...message(2), channel_id: channel, parent_id: 1, shown_in_channel: false });
        timelines.applyEvent(channel, "message.created", { ...message(3), channel_id: channel, parent_id: 1, shown_in_channel: true });

        expect(ids(timelines.state(channel).messages)).toEqual([1, 3]);
    });
});
