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
        expectNoHole(timelines.state(channel), 63);
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
        expectNoHole(timelines.state(channel), 106);
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
        const timelines = createTimelines(dependencies);
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

describe("catching up when fetches fail or are overtaken", () => {
    // A stand-in for the platform's history: messages by id, ids drawn from one sequence shared by
    // every channel, so this channel's ids skip. A page is read when it is asked for, as the
    // database reads it, so a page asked for before a change does not carry it.
    type Fetch = { limit: number, beforeId?: number, afterId?: number };

    function server(initial: number[]) {
        const rows = new Map<number, TimelineMessage>();
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
                return row;
            },
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
                return { messages, read_cursor: null, unread_count: 0, has_more: hasMore };
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
        const timelines = createTimelines({ fetchPage: fetches.fetchPage, sleep: backoff.sleep });
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
        await joining;

        expectCaughtUp(timelines.state(channel), platform.ids());
        expect(timelines.state(channel).isBehind).toBe(false);
    });

    test("an older page asked for before the channel was reloaded is dropped when it arrives", async () => {
        const platform = server(Array.from({ length: 200 }, (_, i) => i + 1));
        const fetches = controlled(platform, options => options.beforeId !== undefined);
        const timelines = createTimelines({ fetchPage: fetches.fetchPage, catchUp: () => ({ page: 25, max: 50 }) });
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
        const timelines = createTimelines({ fetchPage: fetches.fetchPage, sleep: backoff.sleep, catchUp: () => ({ page: 25, max: 50 }) });
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
        const timelines = createTimelines({ fetchPage: async (_c, options) => platform.read(options as Fetch) });
        await timelines.loadNewest(channel);
        await timelines.onJoined(channel);

        // the socket is down: none of these arrive live
        Object.assign(platform.rows.get(2) as TimelineMessage, { body: "edited", edited_at: "2026-09-23T11:00:00Z" });
        Object.assign(platform.rows.get(3) as TimelineMessage, { body: null, deleted_at: "2026-09-23T11:01:00Z" });
        Object.assign(platform.rows.get(4) as TimelineMessage, { body: null, visibility: "hidden", visibility_notice: "Hidden while it is reviewed." });

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
        const timelines = createTimelines({ fetchPage: fetches.fetchPage, catchUp: () => ({ page: 25, max: 500 }) });
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
        const timelines = createTimelines({ fetchPage: fetches.fetchPage, catchUp: () => ({ page: 5, max: 500 }) });
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
        const timelines = createTimelines({ fetchPage: fetches.fetchPage });
        await timelines.loadNewest(channel);

        const resyncing = timelines.loadNewest(channel);
        await settle();
        Object.assign(platform.rows.get(2) as TimelineMessage, { body: null, visibility: "hidden", visibility_notice: "Hidden." });
        timelines.applyEvent(channel, "message.visibility", { id: 2, channel_id: channel, state: "hidden", notice: "Hidden." });
        fetches.pending.shift()?.answer();
        await resyncing;

        const two = timelines.state(channel).messages[1];
        expect(two.body).toBeNull();
        expect(two.visibility).toBe("hidden");
    });

    test("joins in quick succession run one catch-up at a time, and one more after it", async () => {
        const platform = server([1, 2, 3]);
        const fetches = controlled(platform, options => options.afterId !== undefined);
        const timelines = createTimelines({ fetchPage: fetches.fetchPage });
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
        const timelines = createTimelines({ fetchPage: fetches.fetchPage, sleep: backoff.sleep });
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
        const timelines = createTimelines({ fetchPage: async (_c, options) => platform.read(options as Fetch) });
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
        const timelines = createTimelines({ fetchPage: fetches.fetchPage, sleep: backoff.sleep });
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
        const timelines = createTimelines({ fetchPage: fetches.fetchPage, sleep: backoff.sleep });
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
        const timelines = createTimelines({ fetchPage: fetches.fetchPage, sleep: backoff.sleep });
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
        const timelines = createTimelines({ fetchPage: fetches.fetchPage, sleep: backoff.sleep });
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
        const timelines = createTimelines({ fetchPage: async (_c, options) => platform.read(options as Fetch) });
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
        ["deleted", "message.deleted", { deleted_at: "2026-10-05T10:00:00Z" }],
        ["hidden", "message.visibility", { state: "hidden", notice: "Hidden." }]
    ])("a message %s live while a page carrying it is on its way stays that way when the page lands", async (_what, event, change) => {
        const platform = server([1, 2, 3]);
        const fetches = controlled(platform, options => options.afterId !== undefined);
        const timelines = createTimelines({ fetchPage: fetches.fetchPage });
        await timelines.loadNewest(channel);

        for (let id = 4; id <= 10; id++) {
            platform.add(id);
        }
        const joining = timelines.onJoined(channel);
        await settle();

        // the page was read with 7 as it was; the change to 7 arrives live before the page does
        timelines.applyEvent(channel, event, { id: 7, channel_id: channel, ...change });
        fetches.pending.shift()?.answer();
        await joining;

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
        const timelines = createTimelines({ fetchPage: fetches.fetchPage });
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
        const timelines = createTimelines({ fetchPage: fetches.fetchPage });
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
        const timelines = createTimelines({ fetchPage: async (_c, options) => platform.read(options as Fetch) });

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
        const timelines = createTimelines({ fetchPage: fetches.fetchPage });
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
