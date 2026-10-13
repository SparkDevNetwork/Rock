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
// Threads: the threads list, the thread page, following, the thread's read position and the live
// counts of a thread's root. The list has no live signal, so it is fetched when the panel opens,
// when the person switches tabs and when they press refresh, and at no other time. An answer a
// newer fetch overtook is dropped, and nothing resolves into state once the threads are stopped.
// The thread's read position is saved as the channel's is, on events only: closing the thread,
// the window losing focus and the page going to the background, and only when a newer reply was
// on screen. A root's counts and faces follow thread.updated, and a followed root shows unread
// when someone else replied and the thread is not open.
//
// The composable these tests are written to, in useThreads.partial.ts:
//
//   createThreads(dependencies: {
//       call: (name: string, args: Record<string, unknown>) => Promise<CallResult>,  the shell's call path
//       timelines: Pick<Timelines, "state">,   the channel timelines, whose roots thread.updated moves
//       personAliasGuid: string                the reader, as repliers name people
//   }): Threads
//
//   Threads = {
//       state: {
//           list: { tab: "channel" | "all", channelId: string | null, rows: ThreadListRow[], hasMore: boolean,
//                   isLoading: boolean, isRefreshing: boolean, error: ChatError | null,
//                   busyRootId: number | null, rowError: { rootId: number, error: ChatError } | null },
//           thread: { channelId: string, rootId: number, root: TimelineMessage | null,
//                     replies: TimelineMessage[] (oldest first), following: boolean, readCursor: number | null,
//                     hasMore: boolean, isLoading: boolean, error: ChatError | null } | null
//       },
//       openList(channelId: string, tab?: "channel" | "all"): Promise<void>,
//       switchTab(tab: "channel" | "all"): Promise<void>,
//       refresh(): Promise<void>,
//       loadMore(): Promise<void>,           the next 30 when the list is scrolled to its end
//       closeList(): void,
//       follow(rootId: number, isFollowing: boolean): Promise<boolean>,
//       openThread(channelId: string, rootId: number): Promise<void>,
//       loadOlderReplies(): Promise<void>,
//       seen(messageId: number): void,       a reply of the open thread was on screen
//       closeThread(): Promise<void>,        saves the thread's read position
//       attach(targets: PageEventTargets): () => void,   saves on blur and on hidden
//       applyEvent(channelId: string, event: string, payload: unknown): void,
//       stop(): void
//   }
//
//   followNotice(isFollowing: boolean): ToastNotice, the toast a follow change shows.
//
// ThreadListRow is chat_list_threads' row; TimelineMessage gains repliers, thread_following and
// thread_unread, as chat_messages_json now answers them.
import { createThreads, followNotice } from "../../../../../src/Communication/Chat/ChatShell/composables/useThreads.partial";
import { createTimelines } from "../../../../../src/Communication/Chat/ChatShell/composables/useHistory.partial";
import { PageEventTargets } from "../../../../../src/Communication/Chat/ChatShell/composables/useMarkRead.partial";
import { CallResult } from "../../../../../src/Communication/Chat/ChatShell/platformCall.partial";
import { HistoryPage, TimelineMessage } from "../../../../../src/Communication/Chat/ChatShell/types.partial";

// #region Fixtures

const channel = "c0000001-0000-4000-8000-000000000000";
const otherChannel = "c0000002-0000-4000-8000-000000000000";
const me = "a0000001-0000-4000-8000-000000000000";
const anna = "a0000002-0000-4000-8000-000000000000";
const ben = "a0000003-0000-4000-8000-000000000000";

type Replier = { person_alias_guid: string, nick_name: string, last_name: string, avatar_url: string | null };

const annaReplier: Replier = { person_alias_guid: anna, nick_name: "Anna", last_name: "Reyes", avatar_url: null };
const benReplier: Replier = { person_alias_guid: ben, nick_name: "Ben", last_name: "Okafor", avatar_url: null };
const meReplier: Replier = { person_alias_guid: me, nick_name: "Jordan", last_name: "Ellis", avatar_url: null };

/** A message in the shape chat_messages_json answers, with the thread keys it now carries. */
function message(id: number, extra: Record<string, unknown> = {}): TimelineMessage {
    return {
        id,
        parent_id: null,
        quoted_message_id: null,
        quoted: null,
        shown_in_channel: false,
        person_alias_guid: anna,
        sender_nick_name: "Anna",
        sender_last_name: "Reyes",
        sender_avatar_url: null,
        sender_listed: true,
        message_type: "text",
        body: `message ${id}`,
        pieces: null,
        blocks: null,
        metadata: null,
        visibility: "visible",
        visibility_notice: null,
        reactions: [],
        reply_count: 0,
        last_reply_at: null,
        participant_count: 0,
        repliers: null,
        thread_following: null,
        thread_unread: null,
        created_at: "2026-10-13T09:00:00Z",
        edited_at: null,
        deleted_at: null,
        ...extra
    } as unknown as TimelineMessage;
}

/** A root with replies, as history and the thread page carry it. */
function root(id: number, extra: Record<string, unknown> = {}): TimelineMessage {
    return message(id, {
        reply_count: 2,
        last_reply_at: "2026-10-13T09:41:00Z",
        participant_count: 2,
        repliers: [annaReplier, benReplier],
        thread_following: true,
        thread_unread: false,
        ...extra
    });
}

/** A reply of a root. */
function reply(id: number, rootId: number, extra: Record<string, unknown> = {}): TimelineMessage {
    return message(id, { parent_id: rootId, ...extra });
}

/** One row of chat_list_threads, exactly its keys. */
function listRow(rootId: number, extra: Record<string, unknown> = {}): Record<string, unknown> {
    return {
        channel_id: channel,
        root: root(rootId),
        reply_count: 2,
        last_reply_at: "2026-10-13T09:41:00Z",
        last_reply_id: rootId + 2,
        participant_count: 2,
        following: true,
        has_unread: false,
        mention_count: 0,
        ...extra
    };
}

/** chat_list_threads' answer. */
function listAnswer(rows: Array<Record<string, unknown>>, hasMore = false): CallResult {
    return { ok: true, data: { threads: rows, has_more: hasMore } };
}

/** chat_get_thread_history's answer: the root, the replies newest first, and the reader's standing. */
function threadAnswer(rootMessage: TimelineMessage, newestFirst: TimelineMessage[], extra: Record<string, unknown> = {}): CallResult {
    return {
        ok: true,
        data: { root: rootMessage, messages: newestFirst, read_cursor: null, unread_count: 0, following: true, has_more: false, ...extra }
    };
}

/** A refusal as the shell's call path hands it over: a fetch that never reached the platform. */
const offline: CallResult = { ok: false, status: 0, error: { code: "rpc.transport", severity: "failed" }, retryAfterSeconds: null };

type Deferred<T> = { promise: Promise<T>, resolve: (value: T) => void };

function deferred<T>(): Deferred<T> {
    let resolve!: (value: T) => void;
    const promise = new Promise<T>(r => resolve = r);
    return { promise, resolve };
}

type Call = { name: string, args: Record<string, unknown> };

/**
 * Threads over a fake call path. Each call takes the next answer queued for its function, a
 * result or a deferred the test resolves by hand; a function with nothing queued answers an
 * empty success.
 */
async function setup(historyPage?: HistoryPage): Promise<{
    threads: ReturnType<typeof createThreads>,
    timelines: ReturnType<typeof createTimelines>,
    calls: Call[],
    answers: Record<string, Array<CallResult | Deferred<CallResult>>>,
    callsTo: (name: string) => Call[]
}> {
    const calls: Call[] = [];
    const answers: Record<string, Array<CallResult | Deferred<CallResult>>> = {};
    const timelines = createTimelines({
        fetchPage: async () => historyPage ?? { messages: [], read_cursor: null, unread_count: 0, has_more: false }
    });
    await timelines.loadNewest(channel);

    const threads = createThreads({
        call: (name, args) => {
            calls.push({ name, args });
            const next = answers[name]?.shift();
            if (!next) {
                return Promise.resolve({ ok: true, data: {} } as CallResult);
            }
            return "promise" in next ? next.promise : Promise.resolve(next);
        },
        timelines,
        personAliasGuid: me
    });

    return { threads, timelines, calls, answers, callsTo: (name: string) => calls.filter(c => c.name === name) };
}

/** Lets every promise already settled run its continuations. */
async function settle(): Promise<void> {
    for (let i = 0; i < 10; i++) {
        await Promise.resolve();
    }
}

type Listeners = Record<string, Array<() => void>>;

/** The page's document and window, whose events the test fires. */
function page(): { targets: PageEventTargets, fire: (type: string) => void } {
    const onDocument: Listeners = {};
    const onWindow: Listeners = {};
    const add = (map: Listeners) => (type: string, listener: () => void): void => {
        (map[type] ??= []).push(listener);
    };
    const remove = (map: Listeners) => (type: string, listener: () => void): void => {
        map[type] = (map[type] ?? []).filter(l => l !== listener);
    };
    const targets: PageEventTargets = {
        document: { visibilityState: "visible", addEventListener: add(onDocument), removeEventListener: remove(onDocument) },
        window: { addEventListener: add(onWindow), removeEventListener: remove(onWindow) }
    };
    return {
        targets,
        fire: (type: string) => {
            if (type === "hidden" || type === "visible") {
                targets.document.visibilityState = type;
                (onDocument["visibilitychange"] ?? []).forEach(l => l());
            }
            else {
                (onWindow[type] ?? []).forEach(l => l());
            }
        }
    };
}

/** thread.updated as the channel topic carries it, exactly its keys. */
function threadUpdated(rootId: number, repliers: Replier[] | null, replyCount: number): Record<string, unknown> {
    return {
        channel_id: channel,
        root_id: rootId,
        reply_count: replyCount,
        last_reply_at: "2026-10-13T10:05:00Z",
        participant_count: repliers?.length ?? 0,
        repliers
    };
}

/** The root a channel's timeline holds. */
function timelineRoot(timelines: ReturnType<typeof createTimelines>, id: number): Record<string, unknown> {
    return timelines.state(channel).messages.find(m => m.id === id) as unknown as Record<string, unknown>;
}

// #endregion Fixtures

describe("the threads list is fetched", () => {
    test("when the panel opens, for the open conversation", async () => {
        const { threads, callsTo, answers } = await setup();
        answers.chat_list_threads = [listAnswer([listRow(40)])];

        await threads.openList(channel);

        expect(callsTo("chat_list_threads")).toEqual([
            { name: "chat_list_threads", args: expect.objectContaining({ p_channel_id: channel, p_limit: 30 }) }
        ]);
        expect(threads.state.list.rows.map(r => r.root.id)).toEqual([40]);
        expect(threads.state.list.tab).toBe("channel");
    });

    test("when the tab switches, the All tab asking for every followed thread", async () => {
        const { threads, callsTo } = await setup();
        await threads.openList(channel);

        await threads.switchTab("all");

        const listCalls = callsTo("chat_list_threads");
        expect(listCalls).toHaveLength(2);
        expect(listCalls[1].args).toEqual(expect.objectContaining({ p_channel_id: null }));
        expect(threads.state.list.tab).toBe("all");
    });

    test("when the person presses refresh", async () => {
        const { threads, callsTo } = await setup();
        await threads.openList(channel);

        await threads.refresh();

        expect(callsTo("chat_list_threads")).toHaveLength(2);
    });

    test("and at no other time: no event, thread or follow fetches it again", async () => {
        const { threads, timelines, callsTo, answers } = await setup({ messages: [root(40)], read_cursor: null, unread_count: 0, has_more: false });
        answers.chat_list_threads = [listAnswer([listRow(40)])];
        await threads.openList(channel);

        threads.applyEvent(channel, "thread.updated", threadUpdated(40, [benReplier, annaReplier], 3));
        threads.applyEvent(channel, "message.created", {
            id: 43, channel_id: channel, parent_id: 40, shown_in_channel: false, person_alias_guid: ben,
            message_type: "text", body: "hi", created_at: "2026-10-13T10:05:00Z"
        });
        threads.applyEvent(channel, "message.deleted", { id: 41, channel_id: channel, deleted_at: "2026-10-13T10:06:00Z" });
        await threads.follow(40, false);
        await threads.openThread(channel, 40);
        await threads.closeThread();
        await settle();

        expect(callsTo("chat_list_threads")).toHaveLength(1);
        expect(timelines.state(channel).messages).toHaveLength(1);
    });

    test("the next 30 when the list is scrolled to its end, after the last row by both cursor values", async () => {
        const { threads, callsTo, answers } = await setup();
        answers.chat_list_threads = [
            listAnswer([listRow(40), listRow(30, { last_reply_at: "2026-10-13T08:00:00Z" })], true),
            listAnswer([listRow(20)])
        ];
        await threads.openList(channel);

        await threads.loadMore();

        expect(callsTo("chat_list_threads")[1].args).toEqual(expect.objectContaining({
            p_channel_id: channel,
            p_before_last_reply_at: "2026-10-13T08:00:00Z",
            p_before_id: 30,
            p_limit: 30
        }));
        expect(threads.state.list.rows.map(r => r.root.id)).toEqual([40, 30, 20]);
        expect(threads.state.list.hasMore).toBe(false);
    });
});

describe("an answer overtaken by a newer fetch", () => {
    test("is dropped when it arrives after the newer one", async () => {
        const { threads, answers } = await setup();
        const first = deferred<CallResult>();
        const second = deferred<CallResult>();
        answers.chat_list_threads = [first, second];

        const opening = threads.openList(channel);
        const switching = threads.switchTab("all");
        second.resolve(listAnswer([listRow(70, { channel_id: otherChannel })]));
        await switching;
        first.resolve(listAnswer([listRow(40)]));
        await opening;

        expect(threads.state.list.tab).toBe("all");
        expect(threads.state.list.rows.map(r => r.root.id)).toEqual([70]);
    });

    test("is dropped when it arrives before the newer one, which then replaces the rows", async () => {
        const { threads, answers } = await setup();
        const first = deferred<CallResult>();
        const second = deferred<CallResult>();
        answers.chat_list_threads = [first, second];

        const opening = threads.openList(channel);
        const refreshing = threads.refresh();
        first.resolve(listAnswer([listRow(40)]));
        await opening;

        // the older answer never reached the screen, so the list still waits for the newer one
        expect(threads.state.list.rows).toEqual([]);

        second.resolve(listAnswer([listRow(50)]));
        await refreshing;
        expect(threads.state.list.rows.map(r => r.root.id)).toEqual([50]);
    });

    test("a thread page overtaken by opening another thread is dropped", async () => {
        const { threads, answers } = await setup();
        const first = deferred<CallResult>();
        const second = deferred<CallResult>();
        answers.chat_get_thread_history = [first, second];

        const openingFirst = threads.openThread(channel, 40);
        const openingSecond = threads.openThread(channel, 50);
        second.resolve(threadAnswer(root(50), [reply(52, 50), reply(51, 50)]));
        await openingSecond;
        first.resolve(threadAnswer(root(40), [reply(41, 40)]));
        await openingFirst;

        expect(threads.state.thread?.rootId).toBe(50);
        expect(threads.state.thread?.root?.id).toBe(50);
        expect(threads.state.thread?.replies.map(r => r.id)).toEqual([51, 52]);
    });
});

describe("once the threads are stopped", () => {
    test("no list answer resolves into state", async () => {
        const { threads, answers } = await setup();
        const answer = deferred<CallResult>();
        answers.chat_list_threads = [answer];

        const opening = threads.openList(channel);
        threads.stop();
        answer.resolve(listAnswer([listRow(40)]));
        await opening;

        expect(threads.state.list.rows).toEqual([]);
    });

    test("no thread page resolves into state", async () => {
        const { threads, answers } = await setup();
        const answer = deferred<CallResult>();
        answers.chat_get_thread_history = [answer];

        const opening = threads.openThread(channel, 40);
        threads.stop();
        answer.resolve(threadAnswer(root(40), [reply(41, 40)]));
        await opening;

        expect(threads.state.thread?.root ?? null).toBeNull();
        expect(threads.state.thread?.replies ?? []).toEqual([]);
    });

    test("no follow answer resolves into state, and nothing more is fetched", async () => {
        const { threads, answers, calls } = await setup();
        answers.chat_list_threads = [listAnswer([listRow(40)])];
        await threads.openList(channel);
        const answer = deferred<CallResult>();
        answers.chat_follow_thread = [answer];

        const following = threads.follow(40, false);
        threads.stop();
        answer.resolve({ ok: true, data: { following: false, read_cursor: null } });
        await following;
        const callsAtStop = calls.length;
        await threads.refresh();
        await threads.openList(channel);

        expect(threads.state.list.rows[0].following).toBe(true);
        expect(calls).toHaveLength(callsAtStop);
    });
});

describe("the thread page", () => {
    test("reads the newest replies and holds them oldest first, with the reader's follow", async () => {
        const { threads, callsTo, answers } = await setup();
        answers.chat_get_thread_history = [threadAnswer(root(40), [reply(43, 40), reply(42, 40), reply(41, 40)], { following: false, read_cursor: 41, has_more: true })];

        await threads.openThread(channel, 40);

        expect(callsTo("chat_get_thread_history")[0].args).toEqual(expect.objectContaining({ p_root_id: 40, p_limit: 50 }));
        expect(threads.state.thread).toEqual(expect.objectContaining({ channelId: channel, rootId: 40, following: false, readCursor: 41, hasMore: true }));
        expect(threads.state.thread?.replies.map(r => r.id)).toEqual([41, 42, 43]);
    });

    test("clears the root's unread mark in the conversation when it opens", async () => {
        const { threads, timelines } = await setup({ messages: [root(40, { thread_unread: true })], read_cursor: null, unread_count: 0, has_more: false });

        await threads.openThread(channel, 40);

        expect(timelineRoot(timelines, 40).thread_unread).toBe(false);
    });

    test("reads older replies before the oldest held", async () => {
        const { threads, callsTo, answers } = await setup();
        answers.chat_get_thread_history = [
            threadAnswer(root(40), [reply(43, 40), reply(42, 40)], { has_more: true }),
            threadAnswer(root(40), [reply(41, 40)], { has_more: false })
        ];
        await threads.openThread(channel, 40);

        await threads.loadOlderReplies();

        expect(callsTo("chat_get_thread_history")[1].args).toEqual(expect.objectContaining({ p_root_id: 40, p_before_id: 42 }));
        expect(threads.state.thread?.replies.map(r => r.id)).toEqual([41, 42, 43]);
        expect(threads.state.thread?.hasMore).toBe(false);
    });

    test("takes a live reply of the open thread, and no other thread's", async () => {
        const { threads, answers } = await setup();
        answers.chat_get_thread_history = [threadAnswer(root(40), [reply(41, 40)])];
        await threads.openThread(channel, 40);

        threads.applyEvent(channel, "message.created", {
            id: 42, channel_id: channel, parent_id: 40, shown_in_channel: false, person_alias_guid: ben,
            message_type: "text", body: "on my way", created_at: "2026-10-13T10:05:00Z"
        });
        threads.applyEvent(channel, "message.created", {
            id: 61, channel_id: channel, parent_id: 60, shown_in_channel: false, person_alias_guid: ben,
            message_type: "text", body: "elsewhere", created_at: "2026-10-13T10:06:00Z"
        });

        expect(threads.state.thread?.replies.map(r => r.id)).toEqual([41, 42]);
    });
});

describe("the thread's read position", () => {
    async function opened(readCursor: number | null): Promise<Awaited<ReturnType<typeof setup>>> {
        const harness = await setup();
        harness.answers.chat_get_thread_history = [threadAnswer(root(40), [reply(43, 40), reply(42, 40), reply(41, 40)], { read_cursor: readCursor })];
        await harness.threads.openThread(channel, 40);
        return harness;
    }

    test("is saved on close with the newest reply that was on screen", async () => {
        const { threads, callsTo } = await opened(41);
        threads.seen(42);
        threads.seen(43);
        threads.seen(42);

        await threads.closeThread();

        expect(callsTo("chat_mark_thread_read")).toEqual([
            { name: "chat_mark_thread_read", args: { p_root_id: 40, p_message_id: 43 } }
        ]);
        expect(threads.state.thread).toBeNull();
    });

    test("is not saved on close when nothing newer than the stored position was seen", async () => {
        const { threads, callsTo } = await opened(43);
        threads.seen(42);
        threads.seen(43);

        await threads.closeThread();

        expect(callsTo("chat_mark_thread_read")).toEqual([]);
    });

    test("is not saved when no reply was on screen at all", async () => {
        const { threads, callsTo } = await opened(null);

        await threads.closeThread();

        expect(callsTo("chat_mark_thread_read")).toEqual([]);
    });

    test("is saved when the window loses focus, once for what was seen", async () => {
        const { threads, callsTo } = await opened(41);
        const { targets, fire } = page();
        threads.attach(targets);
        threads.seen(43);

        fire("blur");
        await settle();
        fire("blur");
        await settle();

        expect(callsTo("chat_mark_thread_read")).toEqual([
            { name: "chat_mark_thread_read", args: { p_root_id: 40, p_message_id: 43 } }
        ]);
    });

    test("is saved when the page goes to the background, and not when it comes back", async () => {
        const { threads, callsTo } = await opened(41);
        const { targets, fire } = page();
        threads.attach(targets);
        threads.seen(42);

        fire("visible");
        await settle();
        expect(callsTo("chat_mark_thread_read")).toEqual([]);

        fire("hidden");
        await settle();
        expect(callsTo("chat_mark_thread_read")).toEqual([
            { name: "chat_mark_thread_read", args: { p_root_id: 40, p_message_id: 42 } }
        ]);
    });

    test("is not saved on blur when no thread is open", async () => {
        const { threads, callsTo } = await setup();
        const { targets, fire } = page();
        threads.attach(targets);

        fire("blur");
        fire("hidden");
        await settle();

        expect(callsTo("chat_mark_thread_read")).toEqual([]);
    });

    test("stops listening once detached", async () => {
        const { threads, callsTo } = await opened(41);
        const { targets, fire } = page();
        const detach = threads.attach(targets);
        threads.seen(43);

        detach();
        fire("blur");
        await settle();

        expect(callsTo("chat_mark_thread_read")).toEqual([]);
    });
});

describe("thread.updated", () => {
    test("moves a root's count, last reply and faces in the conversation", async () => {
        const { threads, timelines } = await setup({ messages: [root(40)], read_cursor: null, unread_count: 0, has_more: false });

        threads.applyEvent(channel, "thread.updated", threadUpdated(40, [benReplier, annaReplier], 3));

        const held = timelineRoot(timelines, 40);
        expect(held.reply_count).toBe(3);
        expect(held.last_reply_at).toBe("2026-10-13T10:05:00Z");
        expect(held.participant_count).toBe(2);
        expect(held.repliers).toEqual([benReplier, annaReplier]);
    });

    test("gives a root its first reply, so a new pill appears", async () => {
        const { threads, timelines } = await setup({ messages: [message(40)], read_cursor: null, unread_count: 0, has_more: false });

        threads.applyEvent(channel, "thread.updated", threadUpdated(40, [annaReplier], 1));

        expect(timelineRoot(timelines, 40).reply_count).toBe(1);
        expect(timelineRoot(timelines, 40).repliers).toEqual([annaReplier]);
    });

    test("moves the open thread's root in the drawer", async () => {
        const { threads, answers } = await setup();
        answers.chat_get_thread_history = [threadAnswer(root(40), [reply(41, 40)])];
        await threads.openThread(channel, 40);

        threads.applyEvent(channel, "thread.updated", threadUpdated(40, [benReplier, annaReplier], 3));

        const drawerRoot = threads.state.thread?.root as unknown as Record<string, unknown>;
        expect(drawerRoot.reply_count).toBe(3);
        expect(drawerRoot.repliers).toEqual([benReplier, annaReplier]);
    });

    test("marks a followed root unread when someone else replied", async () => {
        const { threads, timelines } = await setup({ messages: [root(40, { thread_following: true, thread_unread: false })], read_cursor: null, unread_count: 0, has_more: false });

        threads.applyEvent(channel, "thread.updated", threadUpdated(40, [benReplier, annaReplier], 3));

        expect(timelineRoot(timelines, 40).thread_unread).toBe(true);
    });

    test("leaves a followed root read when the newest replier is the reader", async () => {
        const { threads, timelines } = await setup({ messages: [root(40, { thread_following: true, thread_unread: false })], read_cursor: null, unread_count: 0, has_more: false });

        threads.applyEvent(channel, "thread.updated", threadUpdated(40, [meReplier, benReplier], 3));

        expect(timelineRoot(timelines, 40).thread_unread).toBe(false);
    });

    test("leaves a followed root read while its thread is open", async () => {
        const { threads, timelines, answers } = await setup({ messages: [root(40, { thread_following: true, thread_unread: false })], read_cursor: null, unread_count: 0, has_more: false });
        answers.chat_get_thread_history = [threadAnswer(root(40), [reply(41, 40)])];
        await threads.openThread(channel, 40);

        threads.applyEvent(channel, "thread.updated", threadUpdated(40, [benReplier, annaReplier], 3));

        expect(timelineRoot(timelines, 40).thread_unread).toBe(false);
    });

    test("never marks a root the reader does not follow", async () => {
        const { threads, timelines } = await setup({ messages: [root(40, { thread_following: false, thread_unread: false })], read_cursor: null, unread_count: 0, has_more: false });

        threads.applyEvent(channel, "thread.updated", threadUpdated(40, [benReplier, annaReplier], 3));

        expect(timelineRoot(timelines, 40).reply_count).toBe(3);
        expect(timelineRoot(timelines, 40).thread_unread).toBe(false);
    });
});

describe("following", () => {
    test("an unfollowed row stays in the list, not following, until the next fetch", async () => {
        const { threads, callsTo, answers } = await setup();
        answers.chat_list_threads = [listAnswer([listRow(40), listRow(30)]), listAnswer([listRow(30)])];
        answers.chat_follow_thread = [{ ok: true, data: { following: false, read_cursor: null } }];
        await threads.openList(channel, "all");

        expect(await threads.follow(40, false)).toBe(true);

        expect(callsTo("chat_follow_thread")[0].args).toEqual({ p_root_id: 40, p_follow: false });
        expect(threads.state.list.rows.map(r => [r.root.id, r.following])).toEqual([[40, false], [30, true]]);

        await threads.refresh();
        expect(threads.state.list.rows.map(r => r.root.id)).toEqual([30]);
    });

    test("names the row it is working on while the call is out", async () => {
        const { threads, answers } = await setup();
        answers.chat_list_threads = [listAnswer([listRow(40)])];
        await threads.openList(channel);
        const answer = deferred<CallResult>();
        answers.chat_follow_thread = [answer];

        const following = threads.follow(40, false);
        expect(threads.state.list.busyRootId).toBe(40);

        answer.resolve({ ok: true, data: { following: false, read_cursor: null } });
        await following;
        expect(threads.state.list.busyRootId).toBeNull();
    });

    test("a refused change leaves the row as it was and puts the error on it", async () => {
        const { threads, answers } = await setup();
        answers.chat_list_threads = [listAnswer([listRow(40)])];
        answers.chat_follow_thread = [offline];
        await threads.openList(channel);

        expect(await threads.follow(40, false)).toBe(false);

        expect(threads.state.list.rows[0].following).toBe(true);
        expect(threads.state.list.rowError).toEqual({ rootId: 40, error: expect.objectContaining({ code: "rpc.transport" }) });
    });

    test("the open thread's toggle follows the answer", async () => {
        const { threads, answers } = await setup();
        answers.chat_get_thread_history = [threadAnswer(root(40), [reply(41, 40)], { following: false })];
        answers.chat_follow_thread = [{ ok: true, data: { following: true, read_cursor: 41 } }];
        await threads.openThread(channel, 40);

        await threads.follow(40, true);

        expect(threads.state.thread?.following).toBe(true);
    });

    test("a list that fails to refresh keeps its rows and says so", async () => {
        const { threads, answers } = await setup();
        answers.chat_list_threads = [listAnswer([listRow(40)]), offline];
        await threads.openList(channel);

        await threads.refresh();

        expect(threads.state.list.rows.map(r => r.root.id)).toEqual([40]);
        expect(threads.state.list.error).toEqual(expect.objectContaining({ code: "rpc.transport" }));
    });

    test("tells the person what changed, in the design's words", () => {
        expect(followNotice(true)).toEqual(expect.objectContaining({ text: "Following this thread", icon: "bell" }));
        expect(followNotice(false)).toEqual(expect.objectContaining({ text: "You won't be notified about new replies", icon: "bell-off" }));
        expect(followNotice(true).code).not.toBe("");
    });
});
