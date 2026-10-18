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
// Mentions: the @ picker's search, the mentions inbox, the inbox's last visit on this device and
// the header's dot.
//
// The search waits for a pause in typing, and an answer older than the newest query is dropped.
// @all is offered, without asking the platform, only where the person holds the right and what
// they typed is nothing yet or the start of "all" or "everyone". Where the person may not see who
// is in the room, the platform is not asked at all.
//
// The inbox is fetched on open, on refresh and on "Load older mentions". Opening it is the visit:
// rows newer than the last visit are New for this visit, and the visit is kept on the device by
// church and person. A browser that refuses its storage still works, the dot then showing for any
// unread mention. The dot is any mention by name still unread where it lives and newer than the
// last visit; reading the conversation or opening the thread drops it, and channel.mention raises
// it. A jump to a channel mention opens the conversation and fetches at most two older pages
// before it says the message is too far back.
//
// Once stopped, nothing more is fetched and no answer still on its way reaches the screen.
//
// The composable these tests are written to, in useMentions.partial.ts:
//
//   createMentions(dependencies: {
//       call: (name, args) => Promise<CallResult>,         the shell's call path
//       tenantId: string, personAliasGuid: string,         who the last visit is kept for
//       storage: () => Pick<Storage, "getItem" | "setItem">,   may throw, as may each method
//       timelines: Pick<Timelines, "state" | "loadOlder">, where a jump looks for its message
//       openChannel: (channelId) => Promise<boolean>,      puts a conversation on screen
//       land: (messageId) => void,                         scrolls to and flashes a held message
//       openThread: (channelId, rootId, messageId) => Promise<void>,
//       toast: (notice: { code, text, icon }) => void
//   }): Mentions
//
//   Mentions = {
//       state: {
//           picker: { query: string | null, people: MentionPerson[], isAllOffered: boolean },
//           inbox: { rows: MentionRow[], hasMore, isLoading, isRefreshing, isLoadingOlder,
//                    error: ChatError | null, olderError: ChatError | null,
//                    newIds: number[], currentId: number | null },
//           hasDot: boolean
//       },
//       search(channel: { channel_id, can_mention, can_mention_all }, query: string): void,
//       closeSearch(): void,
//       openInbox(): Promise<void>, refreshInbox(): Promise<void>, loadOlder(): Promise<void>,
//       closeInbox(): void,
//       jump(row: MentionRow): Promise<void>,
//       setUnread(entries: UnreadMention[]): void,          the bootstrap's me.unread_mentions
//       applyPersonalEvent(event: string, payload: unknown): void,
//       channelRead(channelId: string, readCursor: number): void,
//       threadOpened(channelId: string, rootId: number): void,
//       stop(): void
//   }
//
//   mentionSearchPause: 150, the pause in milliseconds.
import { createMentions, mentionSearchPause } from "../../../../../src/Communication/Chat/ChatShell/composables/useMentions.partial";
import { createTimelines } from "../../../../../src/Communication/Chat/ChatShell/composables/useHistory.partial";
import { CallResult } from "../../../../../src/Communication/Chat/ChatShell/platformCall.partial";
import { HistoryPage, MentionRow, TimelineMessage, UnreadMention } from "../../../../../src/Communication/Chat/ChatShell/types.partial";

// #region Fixtures

const tenant = "70000001-0000-4000-8000-000000000000";
const me = "a0000001-0000-4000-8000-000000000000";
const anna = "a0000002-0000-4000-8000-000000000000";
const ben = "a0000003-0000-4000-8000-000000000000";
const channel = "c0000001-0000-4000-8000-000000000000";
const otherChannel = "c0000002-0000-4000-8000-000000000000";

const annaPerson = { person_alias_guid: anna, nick_name: "Anna", last_name: "Reyes" };
const benPerson = { person_alias_guid: ben, nick_name: "Ben", last_name: "Okafor" };

/** A row of the sidebar as the picker reads it. */
const room = { channel_id: channel, can_mention: true, can_mention_all: false };

/** A message in the shape a history page carries. */
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
        body: `<@${me}> message ${id}`,
        pieces: [{ type: "mention", text: "@Jordan Ellis", person_alias_guid: me }, { type: "text", text: ` message ${id}` }],
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
        created_at: "2026-10-18T09:00:00Z",
        edited_at: null,
        deleted_at: null,
        ...extra
    } as unknown as TimelineMessage;
}

/** One row of chat_list_mentions, exactly its keys. */
function mentionRow(id: number, createdAt: string, extra: { channel_id?: string, parent_id?: number | null } = {}): MentionRow {
    const parentId = extra.parent_id ?? null;
    return {
        channel_id: extra.channel_id ?? channel,
        parent_id: parentId,
        message: message(id, { created_at: createdAt, parent_id: parentId })
    };
}

/** chat_list_mentions' answer. */
function inboxAnswer(rows: MentionRow[], hasMore = false): CallResult {
    return { ok: true, data: { mentions: rows, has_more: hasMore } };
}

/** chat_search_mentionable's answer. */
function peopleAnswer(people: Array<Record<string, unknown>>): CallResult {
    return { ok: true, data: { people } };
}

/** One entry of the bootstrap's me.unread_mentions, exactly its keys. */
function unread(messageId: number, createdAt: string, extra: Partial<UnreadMention> = {}): UnreadMention {
    return { channel_id: channel, parent_id: null, message_id: messageId, created_at: createdAt, in_channel: true, ...extra };
}

/** A call that never reached the platform. */
const offline: CallResult = { ok: false, status: 0, error: { code: "rpc.transport", severity: "failed" }, retryAfterSeconds: null };

/** The platform's refusal when the person may not see who is in the room. */
const noRosterRight: CallResult = {
    ok: false,
    status: 400,
    error: { code: "authz.no_roster_right", severity: "permission", text: "You can't see who is in this channel." },
    retryAfterSeconds: null
};

type Deferred<T> = { promise: Promise<T>, resolve: (value: T) => void };

function deferred<T>(): Deferred<T> {
    let resolve!: (value: T) => void;
    const promise = new Promise<T>(r => resolve = r);
    return { promise, resolve };
}

type Call = { name: string, args: Record<string, unknown> };

/** A device store the test can read, or make refuse every use. */
function memoryStorage(): Pick<Storage, "getItem" | "setItem"> & { items: Map<string, string> } {
    const items = new Map<string, string>();
    return {
        items,
        getItem: (key: string) => items.get(key) ?? null,
        setItem: (key: string, value: string) => {
            items.set(key, value);
        }
    };
}

/** A conversation's history: ids 100 down to 1, the newest page first, ten to a page. */
function fetchPage(_channelId: string, options: { limit: number, beforeId?: number }): Promise<HistoryPage> {
    const top = options.beforeId !== undefined ? options.beforeId - 1 : 100;
    const ids: number[] = [];
    for (let id = top; id > top - 10 && id > 0; id--) {
        ids.push(id);
    }
    return Promise.resolve({ messages: ids.map(id => message(id)), read_cursor: 100, unread_count: 0, has_more: top - 10 > 0 });
}

type Harness = {
    mentions: ReturnType<typeof createMentions>,
    calls: Call[],
    answers: Record<string, Array<CallResult | Deferred<CallResult>>>,
    callsTo: (name: string) => Call[],
    storage: ReturnType<typeof memoryStorage>,
    timelines: ReturnType<typeof createTimelines>,
    opened: string[],
    landed: number[],
    threads: Array<[string, number, number]>,
    toasts: Array<{ code: string, text: string, icon: string }>
};

/**
 * Mentions over a fake call path. Each call takes the next answer queued for its function, a
 * result or a deferred the test resolves by hand; a function with nothing queued answers an
 * empty success.
 */
function setup(options: { storage?: ReturnType<typeof memoryStorage>, storageThrows?: "getter" | "methods" } = {}): Harness {
    const calls: Call[] = [];
    const answers: Record<string, Array<CallResult | Deferred<CallResult>>> = {};
    const storage = options.storage ?? memoryStorage();
    const timelines = createTimelines({ fetchPage });
    const opened: string[] = [];
    const landed: number[] = [];
    const threads: Array<[string, number, number]> = [];
    const toasts: Array<{ code: string, text: string, icon: string }> = [];

    const refusing: Pick<Storage, "getItem" | "setItem"> = {
        getItem: () => {
            throw new Error("SecurityError");
        },
        setItem: () => {
            throw new Error("QuotaExceededError");
        }
    };

    const mentions = createMentions({
        call: (name, args) => {
            calls.push({ name, args });
            const next = answers[name]?.shift();
            if (!next) {
                return Promise.resolve({ ok: true, data: {} });
            }
            return "promise" in next ? next.promise : Promise.resolve(next);
        },
        tenantId: tenant,
        personAliasGuid: me,
        storage: () => {
            if (options.storageThrows === "getter") {
                throw new Error("SecurityError");
            }
            return options.storageThrows === "methods" ? refusing : storage;
        },
        timelines,
        openChannel: async channelId => {
            opened.push(channelId);
            await timelines.loadNewest(channelId);
            return true;
        },
        land: messageId => {
            landed.push(messageId);
        },
        openThread: async (channelId, rootId, messageId) => {
            threads.push([channelId, rootId, messageId]);
        },
        toast: notice => {
            toasts.push(notice);
        }
    });

    return {
        mentions,
        calls,
        answers,
        callsTo: name => calls.filter(c => c.name === name),
        storage,
        timelines,
        opened,
        landed,
        threads,
        toasts
    };
}

/** Lets every answer already given reach the state. */
async function settle(): Promise<void> {
    for (let i = 0; i < 5; i++) {
        await Promise.resolve();
    }
}

// #endregion Fixtures

describe("the picker's search", () => {
    beforeEach(() => {
        jest.useFakeTimers();
    });

    afterEach(() => {
        jest.useRealTimers();
    });

    test("asks the platform only once typing pauses, for the newest query", async () => {
        const h = setup();

        h.mentions.search(room, "a");
        jest.advanceTimersByTime(100);
        h.mentions.search(room, "an");
        jest.advanceTimersByTime(149);
        expect(h.callsTo("chat_search_mentionable")).toHaveLength(0);

        jest.advanceTimersByTime(1);
        await settle();

        const searches = h.callsTo("chat_search_mentionable");
        expect(searches).toHaveLength(1);
        expect(searches[0].args).toEqual(expect.objectContaining({ p_channel_id: channel, p_query: "an" }));
        expect(typeof searches[0].args.p_limit).toBe("number");
    });

    test("searches a bare @ too, which lists the room's first people", async () => {
        const h = setup();
        h.answers.chat_search_mentionable = [peopleAnswer([annaPerson, benPerson])];

        h.mentions.search(room, "");
        jest.advanceTimersByTime(mentionSearchPause);
        await settle();

        expect(h.callsTo("chat_search_mentionable")[0]?.args).toEqual(expect.objectContaining({ p_query: "" }));
        expect(h.mentions.state.picker.people.map(p => p.person_alias_guid)).toEqual([anna, ben]);
        expect(h.mentions.state.picker.query).toBe("");
    });

    test("drops an answer older than the newest query, whatever order they arrive in", async () => {
        const h = setup();
        const first = deferred<CallResult>();
        const second = deferred<CallResult>();
        h.answers.chat_search_mentionable = [first, second];

        h.mentions.search(room, "a");
        jest.advanceTimersByTime(mentionSearchPause);
        await settle();
        h.mentions.search(room, "an");
        jest.advanceTimersByTime(mentionSearchPause);
        await settle();
        expect(h.callsTo("chat_search_mentionable")).toHaveLength(2);

        second.resolve(peopleAnswer([annaPerson]));
        await settle();
        first.resolve(peopleAnswer([annaPerson, benPerson]));
        await settle();

        expect(h.mentions.state.picker.people.map(p => p.person_alias_guid)).toEqual([anna]);
        expect(h.mentions.state.picker.query).toBe("an");
    });

    test("puts nothing on screen once stopped, and asks nothing for a pause still running", async () => {
        const h = setup();
        const answer = deferred<CallResult>();
        h.answers.chat_search_mentionable = [answer];

        h.mentions.search(room, "a");
        jest.advanceTimersByTime(mentionSearchPause);
        await settle();
        h.mentions.search(room, "an");
        h.mentions.stop();
        jest.advanceTimersByTime(mentionSearchPause * 2);
        answer.resolve(peopleAnswer([annaPerson]));
        await settle();

        expect(h.callsTo("chat_search_mentionable")).toHaveLength(1);
        expect(h.mentions.state.picker.people).toEqual([]);
    });

    test("closing the picker drops an answer still on its way", async () => {
        const h = setup();
        const answer = deferred<CallResult>();
        h.answers.chat_search_mentionable = [peopleAnswer([benPerson]), answer];
        h.mentions.search(room, "b");
        jest.advanceTimersByTime(mentionSearchPause);
        await settle();
        expect(h.mentions.state.picker.people.map(p => p.person_alias_guid)).toEqual([ben]);

        h.mentions.search(room, "an");
        jest.advanceTimersByTime(mentionSearchPause);
        await settle();
        h.mentions.closeSearch();
        answer.resolve(peopleAnswer([annaPerson]));
        await settle();

        expect(h.mentions.state.picker.query).toBeNull();
        expect(h.mentions.state.picker.people).toEqual([]);
    });

    test("a refusal lists nobody, not the people an earlier query found, and throws nothing", async () => {
        const h = setup();
        h.answers.chat_search_mentionable = [peopleAnswer([annaPerson]), noRosterRight];

        h.mentions.search(room, "a");
        jest.advanceTimersByTime(mentionSearchPause);
        await settle();
        expect(h.mentions.state.picker.people.map(p => p.person_alias_guid)).toEqual([anna]);

        h.mentions.search(room, "an");
        jest.advanceTimersByTime(mentionSearchPause);
        await settle();

        expect(h.mentions.state.picker.people).toEqual([]);
    });

    test("never asks where the person may not see who is in the room", async () => {
        const h = setup();

        h.mentions.search({ ...room, can_mention: false }, "an");
        jest.advanceTimersByTime(mentionSearchPause * 2);
        await settle();

        expect(h.callsTo("chat_search_mentionable")).toHaveLength(0);
        expect(h.mentions.state.picker.people).toEqual([]);
        expect(h.mentions.state.picker.query).toBe("an");

        // the same query where they may
        h.mentions.search(room, "an");
        jest.advanceTimersByTime(mentionSearchPause);
        await settle();
        expect(h.callsTo("chat_search_mentionable")).toHaveLength(1);
    });

    describe("offers @all", () => {
        test.each([
            ["nothing typed", ""],
            ["the start of all", "al"],
            ["all itself", "all"],
            ["all in capitals", "ALL"],
            ["the start of everyone", "ev"],
            ["everyone itself", "everyone"]
        ])("with the right and %s", async (_what, query) => {
            const h = setup();

            h.mentions.search({ ...room, can_mention_all: true }, query);
            jest.advanceTimersByTime(mentionSearchPause);
            await settle();

            expect(h.mentions.state.picker.isAllOffered).toBe(true);
        });

        test.each([
            ["a name", "an"],
            ["more than all", "allx"],
            ["more than everyone", "everyones"]
        ])("not for %s", async (_what, query) => {
            const h = setup();
            h.mentions.search({ ...room, can_mention_all: true }, "");
            jest.advanceTimersByTime(mentionSearchPause);
            await settle();
            expect(h.mentions.state.picker.isAllOffered).toBe(true);

            h.mentions.search({ ...room, can_mention_all: true }, query);
            jest.advanceTimersByTime(mentionSearchPause);
            await settle();

            expect(h.mentions.state.picker.isAllOffered).toBe(false);
        });

        test("never without the right", async () => {
            const h = setup();

            h.mentions.search({ ...room, can_mention_all: true }, "");
            jest.advanceTimersByTime(mentionSearchPause);
            await settle();
            expect(h.mentions.state.picker.isAllOffered).toBe(true);

            h.mentions.search(room, "");
            jest.advanceTimersByTime(mentionSearchPause);
            await settle();

            expect(h.mentions.state.picker.isAllOffered).toBe(false);
        });

        test("alone where the person may not see who is in the room but holds the right", async () => {
            const h = setup();

            h.mentions.search({ ...room, can_mention: false, can_mention_all: true }, "");
            jest.advanceTimersByTime(mentionSearchPause);
            await settle();

            expect(h.callsTo("chat_search_mentionable")).toHaveLength(0);
            expect(h.mentions.state.picker.isAllOffered).toBe(true);
            expect(h.mentions.state.picker.people).toEqual([]);
        });
    });
});

describe("the inbox", () => {
    test("fetches the newest page on open, from every conversation", async () => {
        const h = setup();
        h.answers.chat_list_mentions = [inboxAnswer([mentionRow(50, "2026-10-18T09:00:00Z"), mentionRow(40, "2026-10-17T09:00:00Z")], true)];

        await h.mentions.openInbox();

        const fetches = h.callsTo("chat_list_mentions");
        expect(fetches).toHaveLength(1);
        expect(fetches[0].args).toEqual(expect.objectContaining({ p_channel_id: null, p_before_created_at: null, p_before_id: null }));
        expect(typeof fetches[0].args.p_limit).toBe("number");
        expect(h.mentions.state.inbox.rows.map(r => r.message.id)).toEqual([50, 40]);
        expect(h.mentions.state.inbox.hasMore).toBe(true);
    });

    test("says it is loading until the first page answers", async () => {
        const h = setup();
        const answer = deferred<CallResult>();
        h.answers.chat_list_mentions = [answer];

        const opening = h.mentions.openInbox();
        expect(h.mentions.state.inbox.isLoading).toBe(true);

        answer.resolve(inboxAnswer([mentionRow(50, "2026-10-18T09:00:00Z")]));
        await opening;
        expect(h.mentions.state.inbox.isLoading).toBe(false);
    });

    test("loads older mentions after the last row, by its time and id, until the end", async () => {
        const h = setup();
        h.answers.chat_list_mentions = [
            inboxAnswer([mentionRow(50, "2026-10-18T09:00:00Z"), mentionRow(40, "2026-10-17T09:00:00Z")], true),
            inboxAnswer([mentionRow(30, "2026-10-16T09:00:00Z")], false)
        ];
        await h.mentions.openInbox();

        await h.mentions.loadOlder();

        expect(h.callsTo("chat_list_mentions")).toHaveLength(2);
        const older = h.callsTo("chat_list_mentions")[1];
        expect(older.args).toEqual(expect.objectContaining({ p_channel_id: null, p_before_created_at: "2026-10-17T09:00:00Z", p_before_id: 40 }));
        expect(h.mentions.state.inbox.rows.map(r => r.message.id)).toEqual([50, 40, 30]);
        expect(h.mentions.state.inbox.hasMore).toBe(false);

        await h.mentions.loadOlder();
        expect(h.callsTo("chat_list_mentions")).toHaveLength(2);
    });

    test("is busy while older mentions load", async () => {
        const h = setup();
        const older = deferred<CallResult>();
        h.answers.chat_list_mentions = [inboxAnswer([mentionRow(50, "2026-10-18T09:00:00Z")], true), older];
        await h.mentions.openInbox();

        const loading = h.mentions.loadOlder();
        expect(h.mentions.state.inbox.isLoadingOlder).toBe(true);

        older.resolve(inboxAnswer([], false));
        await loading;
        expect(h.mentions.state.inbox.isLoadingOlder).toBe(false);
    });

    test("keeps its rows and says why when older mentions fail", async () => {
        const h = setup();
        h.answers.chat_list_mentions = [inboxAnswer([mentionRow(50, "2026-10-18T09:00:00Z")], true), offline];
        await h.mentions.openInbox();

        await h.mentions.loadOlder();

        expect(h.mentions.state.inbox.rows.map(r => r.message.id)).toEqual([50]);
        expect(h.mentions.state.inbox.olderError?.code).toBe("rpc.transport");
        expect(h.mentions.state.inbox.hasMore).toBe(true);
    });

    test("keeps its rows and says why when a refresh fails", async () => {
        const h = setup();
        h.answers.chat_list_mentions = [inboxAnswer([mentionRow(50, "2026-10-18T09:00:00Z")]), offline];
        await h.mentions.openInbox();

        await h.mentions.refreshInbox();

        expect(h.mentions.state.inbox.rows.map(r => r.message.id)).toEqual([50]);
        expect(h.mentions.state.inbox.error?.code).toBe("rpc.transport");
    });

    test("drops a page on its way once stopped", async () => {
        const h = setup();
        const answer = deferred<CallResult>();
        h.answers.chat_list_mentions = [answer];

        const opening = h.mentions.openInbox();
        h.mentions.stop();
        answer.resolve(inboxAnswer([mentionRow(50, "2026-10-18T09:00:00Z")]));
        await opening;

        expect(h.mentions.state.inbox.rows).toEqual([]);
        await h.mentions.refreshInbox();
        expect(h.callsTo("chat_list_mentions")).toHaveLength(1);
    });

    test("drops a page on its way once closed", async () => {
        const h = setup();
        const answer = deferred<CallResult>();
        h.answers.chat_list_mentions = [answer];

        const opening = h.mentions.openInbox();
        h.mentions.closeInbox();
        answer.resolve(inboxAnswer([mentionRow(50, "2026-10-18T09:00:00Z")]));
        await opening;
        expect(h.mentions.state.inbox.rows).toEqual([]);

        h.answers.chat_list_mentions = [inboxAnswer([mentionRow(60, "2026-10-18T09:30:00Z")])];
        await h.mentions.openInbox();
        expect(h.mentions.state.inbox.rows.map(r => r.message.id)).toEqual([60]);
    });
});

describe("the last visit and New", () => {
    beforeEach(() => {
        jest.useFakeTimers();
        jest.setSystemTime(new Date("2026-10-18T10:00:00Z"));
    });

    afterEach(() => {
        jest.useRealTimers();
    });

    test("is kept on the device for this church and this person", async () => {
        const h = setup();
        h.answers.chat_list_mentions = [inboxAnswer([mentionRow(50, "2026-10-18T09:00:00Z")])];

        await h.mentions.openInbox();

        const keys = Array.from(h.storage.items.keys());
        expect(keys).toHaveLength(1);
        expect(keys[0]).toContain(tenant);
        expect(keys[0]).toContain(me);
    });

    test("marks New only the rows newer than the last visit, until one is opened", async () => {
        const storage = memoryStorage();
        const before = setup({ storage });
        before.answers.chat_list_mentions = [inboxAnswer([mentionRow(40, "2026-10-18T09:00:00Z")])];
        await before.mentions.openInbox();

        jest.setSystemTime(new Date("2026-10-18T12:00:00Z"));
        const h = setup({ storage });
        const fresh = mentionRow(60, "2026-10-18T11:00:00Z");
        h.answers.chat_list_mentions = [inboxAnswer([fresh, mentionRow(40, "2026-10-18T09:00:00Z")])];
        await h.mentions.openInbox();

        expect(h.mentions.state.inbox.newIds).toEqual([60]);

        await h.mentions.jump(fresh);
        expect(h.mentions.state.inbox.currentId).toBe(60);
        expect(h.mentions.state.inbox.newIds).toEqual([]);
    });
});

describe("the header's dot", () => {
    beforeEach(() => {
        jest.useFakeTimers();
        jest.setSystemTime(new Date("2026-10-18T10:00:00Z"));
    });

    afterEach(() => {
        jest.useRealTimers();
    });

    test("shows for an unread mention when the inbox has never been visited here", () => {
        const h = setup();

        h.mentions.setUnread([unread(50, "2026-10-18T09:00:00Z")]);

        expect(h.mentions.state.hasDot).toBe(true);
    });

    test("follows the unread list it is given, off once nothing is unread", () => {
        const h = setup();

        h.mentions.setUnread([unread(50, "2026-10-18T09:00:00Z")]);
        expect(h.mentions.state.hasDot).toBe(true);

        h.mentions.setUnread([]);
        expect(h.mentions.state.hasDot).toBe(false);
    });

    test("clears when the inbox is opened, and the visit outlasts the page", async () => {
        const storage = memoryStorage();
        const h = setup({ storage });
        h.mentions.setUnread([unread(50, "2026-10-18T09:00:00Z")]);
        h.answers.chat_list_mentions = [inboxAnswer([mentionRow(50, "2026-10-18T09:00:00Z")])];

        await h.mentions.openInbox();
        expect(h.mentions.state.hasDot).toBe(false);

        // the page loads again: the same unread mention is no longer newer than the visit
        const reloaded = setup({ storage });
        reloaded.mentions.setUnread([unread(50, "2026-10-18T09:00:00Z")]);
        expect(reloaded.mentions.state.hasDot).toBe(false);

        // one that came after the visit is
        reloaded.mentions.setUnread([unread(70, "2026-10-18T11:00:00Z"), unread(50, "2026-10-18T09:00:00Z")]);
        expect(reloaded.mentions.state.hasDot).toBe(true);
    });

    test("rises on a mention by name that arrives live, in the conversation or in a thread", async () => {
        const h = setup();
        h.mentions.setUnread([]);
        await h.mentions.openInbox();

        h.mentions.applyPersonalEvent("channel.mention", { channel_id: channel, message_id: 80, parent_id: null, in_channel: true, all: false });
        expect(h.mentions.state.hasDot).toBe(true);

        const other = setup();
        other.mentions.setUnread([]);
        other.mentions.applyPersonalEvent("channel.mention", { channel_id: channel, message_id: 81, parent_id: 30, in_channel: false, all: false });
        expect(other.mentions.state.hasDot).toBe(true);
    });

    test("does not rise for @all, which the inbox never lists", () => {
        const h = setup();
        h.mentions.setUnread([]);

        h.mentions.applyPersonalEvent("channel.mention", { channel_id: channel, message_id: 80, parent_id: null, in_channel: true, all: true });
        expect(h.mentions.state.hasDot).toBe(false);

        h.mentions.applyPersonalEvent("channel.mention", { channel_id: channel, message_id: 81, parent_id: null, in_channel: true, all: false });
        expect(h.mentions.state.hasDot).toBe(true);
    });

    test("ignores an event it does not know and a payload it cannot read", () => {
        const h = setup();
        h.mentions.setUnread([]);

        h.mentions.applyPersonalEvent("channel.unread", { channel_id: channel, message_id: 80 });
        h.mentions.applyPersonalEvent("channel.mention", null);
        expect(h.mentions.state.hasDot).toBe(false);

        h.mentions.applyPersonalEvent("channel.mention", { channel_id: channel, message_id: 81, parent_id: null, in_channel: true, all: false });
        expect(h.mentions.state.hasDot).toBe(true);
    });

    test("goes once the conversation is read past the mention, and not before", () => {
        const h = setup();
        h.mentions.setUnread([unread(50, "2026-10-18T09:00:00Z")]);

        h.mentions.channelRead(channel, 49);
        expect(h.mentions.state.hasDot).toBe(true);

        h.mentions.channelRead(otherChannel, 500);
        expect(h.mentions.state.hasDot).toBe(true);

        h.mentions.channelRead(channel, 50);
        expect(h.mentions.state.hasDot).toBe(false);
    });

    test("is not cleared by reading the conversation when the mention lives only in a thread", () => {
        const h = setup();
        h.mentions.setUnread([unread(55, "2026-10-18T09:00:00Z", { parent_id: 30, in_channel: false })]);

        h.mentions.channelRead(channel, 500);

        expect(h.mentions.state.hasDot).toBe(true);
    });

    test("goes once the thread holding the mention is opened", () => {
        const h = setup();
        h.mentions.setUnread([unread(55, "2026-10-18T09:00:00Z", { parent_id: 30, in_channel: false })]);

        h.mentions.threadOpened(channel, 31);
        expect(h.mentions.state.hasDot).toBe(true);

        h.mentions.threadOpened(channel, 30);
        expect(h.mentions.state.hasDot).toBe(false);
    });

    test("a live mention in a thread goes when that thread is opened", () => {
        const h = setup();
        h.mentions.setUnread([]);
        h.mentions.applyPersonalEvent("channel.mention", { channel_id: channel, message_id: 81, parent_id: 30, in_channel: false, all: false });
        expect(h.mentions.state.hasDot).toBe(true);

        h.mentions.threadOpened(channel, 30);

        expect(h.mentions.state.hasDot).toBe(false);
    });

    describe("on a device whose storage refuses", () => {
        test.each([["the storage itself", "getter"], ["each read and write", "methods"]] as const)("by %s, shows for any unread mention and the inbox still works", async (_what, how) => {
            const h = setup({ storageThrows: how });

            h.mentions.setUnread([unread(50, "2026-10-18T09:00:00Z")]);
            expect(h.mentions.state.hasDot).toBe(true);

            h.answers.chat_list_mentions = [inboxAnswer([mentionRow(50, "2026-10-18T09:00:00Z")])];
            await expect(h.mentions.openInbox()).resolves.toBeUndefined();
            expect(h.mentions.state.inbox.rows.map(r => r.message.id)).toEqual([50]);
            expect(h.mentions.state.hasDot).toBe(false);
        });
    });
});

describe("a jump from the inbox", () => {
    test("opens the conversation of a channel mention and lands on the message", async () => {
        const h = setup();

        await h.mentions.jump(mentionRow(95, "2026-10-18T09:00:00Z", { channel_id: otherChannel }));

        expect(h.opened).toEqual([otherChannel]);
        expect(h.landed).toEqual([95]);
        expect(h.toasts).toEqual([]);
    });

    test("fetches an older page for a mention just past the newest", async () => {
        const h = setup();

        await h.mentions.jump(mentionRow(85, "2026-10-18T09:00:00Z"));

        expect(h.landed).toEqual([85]);
        expect(h.toasts).toEqual([]);
    });

    test("past two older pages, stays at the newest and says the message is too far back", async () => {
        const h = setup();

        await h.mentions.jump(mentionRow(5, "2026-10-18T09:00:00Z"));

        expect(h.landed).toEqual([]);
        expect(h.toasts.map(t => t.text)).toEqual(["That message is too far back to jump to."]);
        // the newest page and two older ones, ten each
        expect(h.timelines.state(channel).messages.length).toBe(30);
    });

    test("opens the thread of a reply at the reply, leaving the conversation alone", async () => {
        const h = setup();

        await h.mentions.jump(mentionRow(55, "2026-10-18T09:00:00Z", { parent_id: 30 }));

        expect(h.threads).toEqual([[channel, 30, 55]]);
        expect(h.opened).toEqual([]);
        expect(h.mentions.state.inbox.currentId).toBe(55);
    });

    test("marks the row it opened as the current one", async () => {
        const h = setup();

        await h.mentions.jump(mentionRow(95, "2026-10-18T09:00:00Z"));

        expect(h.mentions.state.inbox.currentId).toBe(95);
    });
});
