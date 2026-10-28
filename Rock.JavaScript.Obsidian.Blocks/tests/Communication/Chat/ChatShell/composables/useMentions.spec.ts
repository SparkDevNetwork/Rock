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
// The opening @ asks at once; a later keystroke waits for a pause in typing, and an answer older
// than the newest query is dropped. Each conversation's bare-@ answer is held for the page, so the
// next open draws it at once while a fresh answer replaces it.
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
//       activeChannelId: () => string | null,              the conversation on screen now
//       readCursor: (channelId) => number | null,          the sidebar row's read position
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
//   mentionHeldConversations: how many conversations' opening people are held.
//   mentionHeldAge: how long, in milliseconds, held people may still be drawn after they were asked.
//   mentionAskShared: how long, in milliseconds, an ask still out is shared before an open asks afresh.
//   warm(channel): asks a conversation's opening people ahead of its first @.
//   forgetPeople(): forgets every conversation's held people, as a resync does.
import { createMentions, mentionAskShared, mentionHeldAge, mentionHeldConversations, mentionSearchPause } from "../../../../../src/Communication/Chat/ChatShell/composables/useMentions.partial";
import { createTimelines } from "../../../../../src/Communication/Chat/ChatShell/composables/useHistory.partial";
import { CallResult } from "../../../../../src/Communication/Chat/ChatShell/platformCall.partial";
import { HistoryPage, MentionRow, TimelineMessage, UnreadMention } from "../../../../../src/Communication/Chat/ChatShell/types.partial";

// #region Fixtures

const tenant = "70000001-0000-4000-8000-000000000000";
const me = "a0000001-0000-4000-8000-000000000000";
const anna = "a0000002-0000-4000-8000-000000000000";
const ben = "a0000003-0000-4000-8000-000000000000";
const channel = "c0000001-0000-4000-8000-000000000000";

/** The platform's page of the inbox, a size no client would choose, so only the limits' is asked. */
const mentionLimits = (): { mention_page_size: number } => ({ mention_page_size: 12 });
const otherChannel = "c0000002-0000-4000-8000-000000000000";

const annaPerson = { person_alias_guid: anna, nick_name: "Anna", last_name: "Reyes" };
const benPerson = { person_alias_guid: ben, nick_name: "Ben", last_name: "Okafor" };

/** A row of the sidebar as the picker reads it. */
const room = { channel_id: channel, can_mention: true, can_mention_all: false, is_member: true };

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
    toasts: Array<{ code: string, text: string, icon: string }>,

    /** The conversation on screen, which the test can change as the person would. */
    active: { channelId: string | null },

    /** Each sidebar row's read position, by channel. */
    readCursors: Map<string, number>
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
    const timelines = createTimelines({ limits: () => ({ history_page_size: 50 }), fetchPage });
    const opened: string[] = [];
    const landed: number[] = [];
    const threads: Array<[string, number, number]> = [];
    const toasts: Array<{ code: string, text: string, icon: string }> = [];
    const active: { channelId: string | null } = { channelId: null };
    const readCursors = new Map<string, number>();

    const refusing: Pick<Storage, "getItem" | "setItem"> = {
        getItem: () => {
            throw new Error("SecurityError");
        },
        setItem: () => {
            throw new Error("QuotaExceededError");
        }
    };

    const mentions = createMentions({
        limits: mentionLimits,
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
            active.channelId = channelId;
            await timelines.loadNewest(channelId);
            return true;
        },
        activeChannelId: () => active.channelId,
        readCursor: channelId => readCursors.get(channelId) ?? null,
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
        toasts,
        active,
        readCursors
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

    test("asks for the opening query at once, with no pause, since it has no keystrokes to gather", () => {
        const h = setup();

        h.mentions.search(room, "");
        jest.advanceTimersByTime(0);

        const searches = h.callsTo("chat_search_mentionable");
        expect(searches).toHaveLength(1);
        expect(searches[0].args).toEqual(expect.objectContaining({ p_channel_id: channel, p_query: "" }));
        expect(typeof searches[0].args.p_limit).toBe("number");
    });

    test("asks for a later keystroke only once typing pauses, for the newest query", async () => {
        const h = setup();
        h.mentions.search(room, "");
        await settle();

        h.mentions.search(room, "a");
        jest.advanceTimersByTime(100);
        h.mentions.search(room, "an");
        jest.advanceTimersByTime(149);
        expect(h.callsTo("chat_search_mentionable")).toHaveLength(1);

        jest.advanceTimersByTime(1);
        await settle();

        const searches = h.callsTo("chat_search_mentionable");
        expect(searches).toHaveLength(2);
        expect(searches[1].args).toEqual(expect.objectContaining({ p_channel_id: channel, p_query: "an" }));
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

    describe("while an answer is on its way", () => {
        const priya = "a0000005-0000-4000-8000-000000000000";
        const priyaPerson = { person_alias_guid: priya, nick_name: "Priya", last_name: "Shah" };
        const annaKim = { person_alias_guid: "a0000006-0000-4000-8000-000000000000", nick_name: "Anna", last_name: "Kim" };

        test("narrows the people it holds to each keystroke at once, by the platform's rule, so the first row always matches what was typed", async () => {
            const h = setup();
            h.answers.chat_search_mentionable = [peopleAnswer([annaKim, benPerson, priyaPerson])];
            h.mentions.search(room, "");
            jest.advanceTimersByTime(mentionSearchPause);
            await settle();

            h.mentions.search(room, "pr");
            expect(h.mentions.state.picker.people.map(p => p.person_alias_guid)).toEqual([priya]);
            expect(h.mentions.state.picker.isAwaitingAnswer).toBe(true);

            // a last name counts too, in any letter case
            const byLast = setup();
            byLast.answers.chat_search_mentionable = [peopleAnswer([annaKim, benPerson, priyaPerson])];
            byLast.mentions.search(room, "");
            jest.advanceTimersByTime(mentionSearchPause);
            await settle();
            byLast.mentions.search(room, "OK");
            expect(byLast.mentions.state.picker.people.map(p => p.person_alias_guid)).toEqual([ben]);

            h.mentions.search(room, "x");
            expect(h.mentions.state.picker.people).toEqual([]);
            expect(h.mentions.state.picker.isAwaitingAnswer).toBe(true);
        });

        test("is answered once the newest query's answer lands", async () => {
            const h = setup();
            h.answers.chat_search_mentionable = [peopleAnswer([priyaPerson])];

            h.mentions.search(room, "pr");
            expect(h.mentions.state.picker.isAwaitingAnswer).toBe(true);
            jest.advanceTimersByTime(mentionSearchPause);
            await settle();

            expect(h.mentions.state.picker.isAwaitingAnswer).toBe(false);
            expect(h.mentions.state.picker.people.map(p => p.person_alias_guid)).toEqual([priya]);
        });

        test("waits for the first answer of each open, and not again until the picker closes", async () => {
            const h = setup();
            h.answers.chat_search_mentionable = [peopleAnswer([annaPerson]), peopleAnswer([annaPerson]), peopleAnswer([benPerson])];

            h.mentions.search(room, "");
            expect(h.mentions.state.picker.isAwaitingFirstAnswer).toBe(true);
            jest.advanceTimersByTime(mentionSearchPause);
            await settle();
            expect(h.mentions.state.picker.isAwaitingFirstAnswer).toBe(false);

            h.mentions.search(room, "a");
            expect(h.mentions.state.picker.isAwaitingFirstAnswer).toBe(false);
            jest.advanceTimersByTime(mentionSearchPause);
            await settle();

            h.mentions.closeSearch();
            expect(h.mentions.state.picker.isAwaitingFirstAnswer).toBe(false);
            h.mentions.search(room, "b");
            expect(h.mentions.state.picker.isAwaitingFirstAnswer).toBe(true);
        });

        test("waits for nothing where the platform is not asked", () => {
            const h = setup();

            h.mentions.search({ ...room, can_mention: false, can_mention_all: true }, "");

            expect(h.mentions.state.picker.isAwaitingFirstAnswer).toBe(false);
            expect(h.mentions.state.picker.isAwaitingAnswer).toBe(false);
        });
    });

    describe("in a conversation it has opened on before", () => {
        const priyaPerson = { person_alias_guid: "a0000005-0000-4000-8000-000000000000", nick_name: "Priya", last_name: "Shah", avatar_url: "https://example.org/priya.jpg" };
        const anotherRoom = { channel_id: otherChannel, can_mention: true, can_mention_all: false, is_member: true };

        test("a bare @ in a conversation warmed by its composer shows its people at once, photos and all", async () => {
            const h = setup();
            h.answers.chat_search_mentionable = [peopleAnswer([annaPerson, priyaPerson]), deferred<CallResult>()];

            h.mentions.warm(room);
            await settle();
            expect(h.callsTo("chat_search_mentionable")[0]?.args).toEqual(expect.objectContaining({ p_channel_id: channel, p_query: "" }));

            h.mentions.search(room, "");

            expect(h.mentions.state.picker.people).toEqual([annaPerson, priyaPerson]);
            expect(h.mentions.state.picker.isAwaitingFirstAnswer).toBe(false);

            // and still asks, after the pause since rows are on screen, so a person who joined or
            // was renamed since is drawn once it answers
            expect(h.mentions.state.picker.isAwaitingAnswer).toBe(true);
            jest.advanceTimersByTime(mentionSearchPause);
            expect(h.callsTo("chat_search_mentionable")).toHaveLength(2);
        });

        test("with held rows drawn, a fast @ and letter ask once, for the letter", async () => {
            const h = setup();
            h.answers.chat_search_mentionable = [peopleAnswer([annaPerson, benPerson])];
            h.mentions.warm(room);
            await settle();

            h.mentions.search(room, "");
            jest.advanceTimersByTime(60);
            h.mentions.search(room, "b");
            expect(h.mentions.state.picker.people.map(p => p.person_alias_guid)).toEqual([ben]);
            jest.advanceTimersByTime(mentionSearchPause);

            const searches = h.callsTo("chat_search_mentionable");
            expect(searches).toHaveLength(2);
            expect(searches[1].args).toEqual(expect.objectContaining({ p_query: "b" }));
        });

        test("a room no longer a member's draws nothing held", async () => {
            const h = setup();
            h.answers.chat_search_mentionable = [peopleAnswer([annaPerson])];
            h.mentions.warm(room);
            await settle();

            h.mentions.search({ ...room, is_member: false }, "");

            expect(h.mentions.state.picker.people).toEqual([]);
            expect(h.mentions.state.picker.isAwaitingFirstAnswer).toBe(true);
        });

        test.each(["membership.changed", "channel.updated"])("%s for the room forgets its people, and an answer asked before it is not held", async event => {
            const h = setup();
            const late = deferred<CallResult>();
            h.answers.chat_search_mentionable = [peopleAnswer([annaPerson]), late];
            h.mentions.warm(room);
            h.mentions.warm(anotherRoom);
            await settle();

            h.mentions.applyPersonalEvent(event, { channel_id: channel });
            h.mentions.search(room, "");
            expect(h.mentions.state.picker.isAwaitingFirstAnswer).toBe(true);
            h.mentions.closeSearch();

            // the other room's ask, still out when that room changes too, lists people from before
            h.mentions.applyPersonalEvent(event, { channel_id: otherChannel });
            late.resolve(peopleAnswer([benPerson]));
            await settle();
            h.mentions.search(anotherRoom, "");
            expect(h.mentions.state.picker.isAwaitingFirstAnswer).toBe(true);
        });

        test("a room whose roster is now hidden forgets its people", async () => {
            const h = setup();
            h.answers.chat_search_mentionable = [peopleAnswer([annaPerson])];
            h.mentions.warm(room);
            await settle();

            h.mentions.search({ ...room, can_mention: false }, "");
            h.mentions.closeSearch();
            h.mentions.search(room, "");

            expect(h.mentions.state.picker.isAwaitingFirstAnswer).toBe(true);
        });

        test("forgets every room's people when asked, as on a resync", async () => {
            const h = setup();
            h.answers.chat_search_mentionable = [peopleAnswer([annaPerson])];
            h.mentions.warm(room);
            await settle();

            h.mentions.forgetPeople();
            h.mentions.search(room, "");

            expect(h.mentions.state.picker.isAwaitingFirstAnswer).toBe(true);
        });

        test("the next open draws the last answer at once, and the new answer replaces it in place", async () => {
            const h = setup();
            const renamed = { ...annaPerson, nick_name: "Annie" };
            const fresh = deferred<CallResult>();
            h.answers.chat_search_mentionable = [peopleAnswer([annaPerson, benPerson]), fresh];
            h.mentions.search(room, "");
            await settle();
            h.mentions.closeSearch();

            h.mentions.search(room, "");
            expect(h.mentions.state.picker.people.map(p => p.person_alias_guid)).toEqual([anna, ben]);
            expect(h.mentions.state.picker.isAwaitingFirstAnswer).toBe(false);

            jest.advanceTimersByTime(mentionSearchPause);
            fresh.resolve(peopleAnswer([renamed, benPerson]));
            await settle();
            expect(h.mentions.state.picker.people.map(p => p.nick_name)).toEqual(["Annie", "Ben"]);
            expect(h.mentions.state.picker.isAwaitingAnswer).toBe(false);

            // the renamed list is what the next open draws
            h.mentions.closeSearch();
            h.mentions.search(room, "");
            expect(h.mentions.state.picker.people.map(p => p.nick_name)).toEqual(["Annie", "Ben"]);
        });

        test("an answer the same as the rows drawn leaves them as they are, so the row the person moved to stays", async () => {
            const h = setup();
            h.answers.chat_search_mentionable = [peopleAnswer([annaPerson, benPerson]), peopleAnswer([annaPerson, benPerson])];
            h.mentions.search(room, "");
            await settle();
            h.mentions.closeSearch();

            h.mentions.search(room, "");
            const drawn = h.mentions.state.picker.people;
            jest.advanceTimersByTime(mentionSearchPause);
            await settle();

            expect(h.mentions.state.picker.people).toBe(drawn);
            expect(h.mentions.state.picker.isAwaitingAnswer).toBe(false);
        });

        test("holds each conversation's own people", async () => {
            const h = setup();
            h.answers.chat_search_mentionable = [peopleAnswer([annaPerson]), peopleAnswer([benPerson])];
            h.mentions.warm(room);
            h.mentions.warm(anotherRoom);
            await settle();

            h.mentions.search(anotherRoom, "");
            expect(h.mentions.state.picker.people.map(p => p.person_alias_guid)).toEqual([ben]);
        });

        test("a query typed at the opening still waits for its answer, since the people held are only the first by name", async () => {
            const h = setup();
            h.answers.chat_search_mentionable = [peopleAnswer([annaPerson, benPerson])];
            h.mentions.warm(room);
            await settle();

            h.mentions.search(room, "b");

            expect(h.mentions.state.picker.isAwaitingFirstAnswer).toBe(true);
            expect(h.callsTo("chat_search_mentionable")).toHaveLength(2);
        });

        test("warms each conversation once, and an open while the warm is on its way shares its answer", async () => {
            const h = setup();
            const answer = deferred<CallResult>();
            h.answers.chat_search_mentionable = [answer];

            h.mentions.warm(room);
            h.mentions.warm(room);
            h.mentions.search(room, "");
            expect(h.callsTo("chat_search_mentionable")).toHaveLength(1);
            expect(h.mentions.state.picker.isAwaitingFirstAnswer).toBe(true);

            answer.resolve(peopleAnswer([annaPerson]));
            await settle();
            expect(h.mentions.state.picker.people.map(p => p.person_alias_guid)).toEqual([anna]);
            expect(h.mentions.state.picker.isAwaitingFirstAnswer).toBe(false);

            h.mentions.closeSearch();
            h.mentions.warm(room);
            expect(h.callsTo("chat_search_mentionable")).toHaveLength(1);
        });

        test("a refusal forgets the people held, so they are not drawn on the next open", async () => {
            const h = setup();
            h.answers.chat_search_mentionable = [peopleAnswer([annaPerson]), noRosterRight];
            h.mentions.warm(room);
            await settle();
            h.mentions.search(room, "");
            jest.advanceTimersByTime(mentionSearchPause);
            await settle();
            expect(h.mentions.state.picker.people).toEqual([]);
            h.mentions.closeSearch();

            h.mentions.search(room, "");
            expect(h.mentions.state.picker.isAwaitingFirstAnswer).toBe(true);
        });

        test("a call that never reached the platform keeps the people held", async () => {
            const h = setup();
            h.answers.chat_search_mentionable = [peopleAnswer([annaPerson]), offline];
            h.mentions.warm(room);
            await settle();
            h.mentions.search(room, "");
            jest.advanceTimersByTime(mentionSearchPause);
            await settle();
            h.mentions.closeSearch();

            h.mentions.search(room, "");
            expect(h.mentions.state.picker.people.map(p => p.person_alias_guid)).toEqual([anna]);
        });

        test("keeps a bounded number of conversations, dropping the one used longest ago", async () => {
            const h = setup();
            const rooms = Array.from({ length: mentionHeldConversations + 1 }, (_, i) => ({ channel_id: `c1000000-0000-4000-8000-${String(i).padStart(12, "0")}`, can_mention: true, can_mention_all: false, is_member: true }));
            for (const each of rooms) {
                h.answers.chat_search_mentionable = [peopleAnswer([annaPerson])];
                h.mentions.warm(each);
                await settle();
            }

            h.mentions.search(rooms[0], "");
            expect(h.mentions.state.picker.isAwaitingFirstAnswer).toBe(true);
            h.mentions.closeSearch();
            h.mentions.search(rooms[1], "");
            expect(h.mentions.state.picker.isAwaitingFirstAnswer).toBe(false);
        });

        // Nothing tells the reader when someone else is put on the Ban List or the room starts
        // hiding its roster, so a held list is drawn only while young.
        test("a held list past its age is not drawn, even one drawn since, and the open asks at once", async () => {
            const h = setup();
            h.answers.chat_search_mentionable = [peopleAnswer([annaPerson, benPerson])];
            h.mentions.warm(room);
            await settle();

            // drawn, then a fast letter cancels the bare refresh within its pause
            h.mentions.search(room, "");
            expect(h.mentions.state.picker.isAwaitingFirstAnswer).toBe(false);
            h.mentions.search(room, "b");
            jest.advanceTimersByTime(mentionSearchPause);
            await settle();
            h.mentions.closeSearch();

            jest.advanceTimersByTime(mentionHeldAge);
            const asked = h.callsTo("chat_search_mentionable").length;
            h.mentions.search(room, "");

            expect(h.mentions.state.picker.people).toEqual([]);
            expect(h.mentions.state.picker.isAwaitingFirstAnswer).toBe(true);
            expect(h.callsTo("chat_search_mentionable")).toHaveLength(asked + 1);
        });

        test("a warm asks again once the list held is past its age", async () => {
            const h = setup();
            h.answers.chat_search_mentionable = [peopleAnswer([annaPerson]), peopleAnswer([benPerson])];
            h.mentions.warm(room);
            await settle();
            jest.advanceTimersByTime(mentionHeldAge - 1);
            h.mentions.warm(room);
            expect(h.callsTo("chat_search_mentionable")).toHaveLength(1);

            jest.advanceTimersByTime(1);
            h.mentions.warm(room);
            await settle();
            expect(h.callsTo("chat_search_mentionable")).toHaveLength(2);

            h.mentions.search(room, "");
            expect(h.mentions.state.picker.people.map(p => p.person_alias_guid)).toEqual([ben]);
        });

        test("an ask that never answers is shared only for a while, so a later open asks afresh and its answer is held", async () => {
            const h = setup();
            h.answers.chat_search_mentionable = [deferred<CallResult>(), peopleAnswer([benPerson])];
            h.mentions.warm(room);

            jest.advanceTimersByTime(mentionAskShared);
            h.mentions.search(room, "");
            expect(h.callsTo("chat_search_mentionable")).toHaveLength(2);
            await settle();
            expect(h.mentions.state.picker.people.map(p => p.person_alias_guid)).toEqual([ben]);

            h.mentions.closeSearch();
            h.mentions.search(room, "");
            expect(h.mentions.state.picker.isAwaitingFirstAnswer).toBe(false);
        });

        test("warms nothing where the person may not see who is in the room, and nothing once stopped", async () => {
            const h = setup();

            h.mentions.warm({ ...room, can_mention: false });
            h.mentions.stop();
            h.mentions.warm(room);
            await settle();

            expect(h.callsTo("chat_search_mentionable")).toHaveLength(0);
        });
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
        expect(fetches[0].args.p_limit).toBe(12);
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

    test("asks for no older mentions while a refresh is on its way, so no old page lands after the refreshed rows", async () => {
        const h = setup();
        const refreshed = deferred<CallResult>();
        const older = deferred<CallResult>();
        h.answers.chat_list_mentions = [inboxAnswer([mentionRow(50, "2026-10-18T09:00:00Z")], true), refreshed, older];
        await h.mentions.openInbox();

        const refreshing = h.mentions.refreshInbox();
        const loading = h.mentions.loadOlder();
        refreshed.resolve(inboxAnswer([mentionRow(70, "2026-10-18T09:40:00Z"), mentionRow(60, "2026-10-18T09:30:00Z")], true));
        older.resolve(inboxAnswer([mentionRow(40, "2026-10-18T08:00:00Z")], false));
        await refreshing;
        await loading;

        expect(h.callsTo("chat_list_mentions")).toHaveLength(2);
        expect(h.mentions.state.inbox.rows.map(r => r.message.id)).toEqual([70, 60]);
        expect(h.mentions.state.inbox.hasMore).toBe(true);
        expect(h.mentions.state.inbox.isLoadingOlder).toBe(false);
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

    test("does not rise for a message in the conversation already read, as an edit naming the person brings", () => {
        const h = setup();
        h.mentions.setUnread([]);
        h.readCursors.set(channel, 80);

        h.mentions.applyPersonalEvent("channel.mention", { channel_id: channel, message_id: 80, parent_id: null, in_channel: true, all: false });
        expect(h.mentions.state.hasDot).toBe(false);

        // a reply kept to its thread is read in the thread, so the conversation's position says nothing of it
        h.mentions.applyPersonalEvent("channel.mention", { channel_id: channel, message_id: 79, parent_id: 30, in_channel: false, all: false });
        expect(h.mentions.state.hasDot).toBe(true);

        const later = setup();
        later.mentions.setUnread([]);
        later.readCursors.set(channel, 80);
        later.mentions.applyPersonalEvent("channel.mention", { channel_id: channel, message_id: 81, parent_id: null, in_channel: true, all: false });
        expect(later.mentions.state.hasDot).toBe(true);
    });

    test("rises for @all, which the inbox lists beside the mentions by name", () => {
        const h = setup();
        h.mentions.setUnread([]);

        h.mentions.applyPersonalEvent("channel.mention", { channel_id: channel, message_id: 80, parent_id: null, in_channel: true, all: true });
        expect(h.mentions.state.hasDot).toBe(true);
    });

    test("does not rise for @all on a message already read in the conversation", () => {
        const h = setup();
        h.mentions.setUnread([]);
        h.readCursors.set(channel, 80);

        h.mentions.applyPersonalEvent("channel.mention", { channel_id: channel, message_id: 80, parent_id: null, in_channel: true, all: true });
        expect(h.mentions.state.hasDot).toBe(false);
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

    describe("overtaken by the person opening another conversation", () => {
        /** Holds each older page the jump asks for until the test lets it through. */
        function holdOlderPages(h: Harness): Array<Deferred<void>> {
            const held: Array<Deferred<void>> = [];
            const loadOlder = h.timelines.loadOlder.bind(h.timelines);
            h.timelines.loadOlder = async (channelId: string): Promise<void> => {
                const gate = deferred<void>();
                held.push(gate);
                await gate.promise;
                await loadOlder(channelId);
            };
            return held;
        }

        test("never lands in the conversation now on screen, even once the page holding the message comes", async () => {
            const h = setup();
            const held = holdOlderPages(h);

            const jumping = h.mentions.jump(mentionRow(85, "2026-10-18T09:00:00Z"));
            await settle();
            expect(held).toHaveLength(1);

            h.active.channelId = otherChannel;
            held[0].resolve();
            await jumping;

            expect(h.landed).toEqual([]);
            expect(h.toasts).toEqual([]);
            expect(held).toHaveLength(1);
        });

        test("never says a message is too far back over the conversation now on screen", async () => {
            const h = setup();
            const held = holdOlderPages(h);

            const jumping = h.mentions.jump(mentionRow(5, "2026-10-18T09:00:00Z"));
            await settle();
            held[0].resolve();
            await settle();
            expect(held).toHaveLength(2);

            h.active.channelId = otherChannel;
            held[1].resolve();
            await jumping;

            expect(h.landed).toEqual([]);
            expect(h.toasts).toEqual([]);
        });
    });

    test("marks the row it opened as the current one", async () => {
        const h = setup();

        await h.mentions.jump(mentionRow(95, "2026-10-18T09:00:00Z"));

        expect(h.mentions.state.inbox.currentId).toBe(95);
    });
});

// A read saved on another of the person's devices arrives as channel.read, the save's own answer,
// and takes the conversation's mentions it read off the dot here as a save of this device's does.
describe("channel.read from another device", () => {
    test("drops the conversation's mentions at or below its place, and not the ones past it", () => {
        const h = setup();
        h.mentions.setUnread([unread(50, "2026-10-18T09:00:00Z")]);

        h.mentions.applyPersonalEvent("channel.read", { channel_id: channel, read_cursor: 49, last_message_id: 60, id: "5e000001-0000-4000-8000-000000000000" });
        expect(h.mentions.state.hasDot).toBe(true);

        h.mentions.applyPersonalEvent("channel.read", { channel_id: channel, read_cursor: 50, last_message_id: 60, id: "5e000002-0000-4000-8000-000000000000" });
        expect(h.mentions.state.hasDot).toBe(false);
    });

    test("leaves a mention kept to a thread, and another conversation's", () => {
        const h = setup();
        h.mentions.setUnread([unread(55, "2026-10-18T09:00:00Z", { parent_id: 30, in_channel: false })]);

        h.mentions.applyPersonalEvent("channel.read", { channel_id: channel, read_cursor: 500, last_message_id: 500, id: "5e000003-0000-4000-8000-000000000000" });
        expect(h.mentions.state.hasDot).toBe(true);

        const other = setup();
        other.mentions.setUnread([unread(50, "2026-10-18T09:00:00Z")]);
        other.mentions.applyPersonalEvent("channel.read", { channel_id: otherChannel, read_cursor: 500, last_message_id: 500, id: "5e000004-0000-4000-8000-000000000000" });
        expect(other.mentions.state.hasDot).toBe(true);
    });

    test("a mention replayed over a reload's answer that already lists it is held once, so one read takes it", () => {
        const h = setup();
        h.mentions.setUnread([unread(50, "2026-10-18T09:00:00Z")]);

        h.mentions.applyPersonalEvent("channel.mention", { channel_id: channel, message_id: 50, parent_id: null, in_channel: true, all: false, id: "5e000005-0000-4000-8000-000000000000" });
        h.mentions.applyPersonalEvent("channel.read", { channel_id: channel, read_cursor: 50, last_message_id: 50, id: "5e000006-0000-4000-8000-000000000000" });

        expect(h.mentions.state.hasDot).toBe(false);
    });
});
