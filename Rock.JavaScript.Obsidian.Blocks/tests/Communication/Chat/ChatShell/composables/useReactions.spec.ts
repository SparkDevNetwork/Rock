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
// Reacting to a message: a toggle that adds or removes the person's reaction, drawn at once and
// then replaced by the platform's answer, which is the message's whole list as the server counts
// it; the live events of everyone reacting, applied by who reacted, so the reader's own reaction
// and its echo never count twice; the emoji catalog, fetched once for the page however often the
// picker opens; the recently used emoji, kept on the device; who reacted, the newest three each
// chip carries and the card's people drawn from them; and the full list behind See all. Nothing
// resolves into state once the reactions are stopped.
//
// The composable these tests are written to, in useReactions.partial.ts:
//
//   type Reaction = { code, count, mine, emoji, image_url, label, reactors? }   as every page message carries them;
//       reactors: the newest three, [{ person_alias_guid, nick_name, last_name, avatar_url, reacted_at, reactor_person_alias_guid }]
//   type ReactionFavorite = { code, label, emoji, image_url }        the bootstrap's reactions key, in order
//   type CatalogEmoji = { code, emoji, image_url, label, category, keywords }   chat_list_emojis, in picker order
//
//   createReactions(dependencies: {
//       call: (name: string, args: Record<string, unknown>) => Promise<CallResult>,  the shell's call path
//       timelines: Pick<Timelines, "state">,      the conversations' messages, whose reactions change in place
//       threads?: Pick<Threads, "state">,         the open thread, whose root and replies change too
//       personAliasGuid: string,                  the session's alias: an event under it is the reader's own
//       notify: (notice: { code, text, icon }) => void,   a toast
//       favorites: ReactionFavorite[],            the bootstrap's favorites
//       churchKey: string,                        the church, for the recents' storage key
//       storage?: Pick<Storage, "getItem" | "setItem"> | null   localStorage; may throw on either
//   }): Reactions
//
//   Reactions = {
//       state: { catalog: CatalogEmoji[] | null, recents: string[] },   recents are codes, newest first
//       toggle(channelId: string, messageId: number, emoji: { code, emoji, image_url, label }): Promise<void>,
//       toggleCode(channelId: string, messageId: number, code: string): Promise<void>,   the glyph from the chip, the catalog or the favorites
//       applyEvent(channelId: string, event: "reaction.added" | "reaction.removed", payload): void,
//       loadCatalog(): Promise<CatalogEmoji[] | null>,     one call for the page; null when it failed
//       recordRecent(code: string): void,
//       listReactors(messageId: number, code: string | null, after: { reacted_at, reactor_person_alias_guid, code } | null): Promise<ReactorsPage | null>,
//       stop(): void
//   }
//
//   whoReacted(reaction, reader: { personAliasGuid, name, photoUrl }): { people: [{ key, label, face }], rest, isMoreShown }   the card's rows
//   whoReactedLine(reaction, reader): string   the card in one sentence
//
// The add and the remove answer { message_id, reactions }, the message's whole list; the events
// carry { message_id, channel_id, code, person_alias_guid, emoji, image_url, label, reactors };
// chat_list_reactors answers { reactors: [{ person_alias_guid, nick_name, last_name, avatar_url,
// reacted_at, reactor_person_alias_guid, code }], has_more }; chat_list_emojis answers { emojis }.
// A refusal is the call path's own: { ok: false, status, error: { code, severity, text }, retryAfterSeconds }.
import { createReactions, whoReacted, whoReactedLine } from "../../../../../src/Communication/Chat/ChatShell/composables/useReactions.partial";
import { createTimelines } from "../../../../../src/Communication/Chat/ChatShell/composables/useHistory.partial";
import { createThreads } from "../../../../../src/Communication/Chat/ChatShell/composables/useThreads.partial";
import { CallResult } from "../../../../../src/Communication/Chat/ChatShell/platformCall.partial";
import { HistoryPage, TimelineMessage } from "../../../../../src/Communication/Chat/ChatShell/types.partial";

// #region Fixtures

const channel = "c0000001-0000-4000-8000-000000000000";
const church = "t0000001-0000-4000-8000-000000000000";
const me = "a0000001-0000-4000-8000-000000000000";
const anna = "a0000002-0000-4000-8000-000000000000";
const david = "a0000003-0000-4000-8000-000000000000";

type Deferred<T> = { promise: Promise<T>, resolve: (value: T) => void, reject: (error: Error) => void };

function deferred<T>(): Deferred<T> {
    let resolve!: (value: T) => void;
    let reject!: (error: Error) => void;
    const promise = new Promise<T>((r, j) => {
        resolve = r;
        reject = j;
    });
    return { promise, resolve, reject };
}

type ReactionRow = { code: string, count: number, mine: boolean, emoji: string | null, image_url: string | null, label: string };

const glyphs: Record<string, { emoji: string, label: string }> = {
    thumbsup: { emoji: "👍", label: "thumbs up" },
    heart: { emoji: "❤️", label: "red heart" },
    joy: { emoji: "😂", label: "face with tears of joy" },
    pray: { emoji: "🙏", label: "folded hands" },
    fire: { emoji: "🔥", label: "fire" },
    grinning: { emoji: "😀", label: "grinning face" }
};

function rx(code: string, count: number, mine = false): ReactionRow {
    return { code, count, mine, emoji: glyphs[code]?.emoji ?? null, image_url: null, label: glyphs[code]?.label ?? code };
}

function glyph(code: string): { code: string, emoji: string, image_url: null, label: string } {
    return { code, emoji: glyphs[code].emoji, image_url: null, label: glyphs[code].label };
}

/** Anna's message as a page carries it, with its reactions. */
function message(id: number, reactions: ReactionRow[] = [], extra: Record<string, unknown> = {}): TimelineMessage {
    return {
        id,
        parent_id: null,
        person_alias_guid: anna,
        sender_nick_name: "Anna",
        sender_last_name: "Reyes",
        message_type: "text",
        body: `message ${id}`,
        visibility: "visible",
        is_own: false,
        actions: [],
        reactions,
        created_at: "2026-10-16T09:00:00Z",
        edited_at: null,
        deleted_at: null,
        ...extra
    } as unknown as TimelineMessage;
}

/** The favorites in the bootstrap's order: thumbs up first, as the church's list has it. */
const favorites = ["thumbsup", "heart", "joy", "pray", "fire"].map(code => ({ code, label: glyphs[code].label, emoji: glyphs[code].emoji, image_url: null }));

/**
 * The catalog in picker order, which is not the favorites' order: the server orders a page's
 * chips by this, so a new chip must land by it too.
 */
const catalog = ["grinning", "joy", "heart", "thumbsup", "pray", "fire"].map(code => ({
    code, emoji: glyphs[code].emoji, image_url: null, label: glyphs[code].label, category: "smileys", keywords: []
}));

const ok = (data: unknown): CallResult => ({ ok: true, data });

const refused = (code: string, text: string): CallResult => ({
    ok: false,
    status: 400,
    error: { code, severity: "failed", text },
    retryAfterSeconds: null
});

const added = (code: string, by: string, messageId = 42): Record<string, unknown> => ({
    message_id: messageId, channel_id: channel, code, person_alias_guid: by, emoji: glyphs[code]?.emoji ?? null, image_url: null, label: glyphs[code]?.label ?? code
});

type Call = { name: string, args: Record<string, unknown> };

type FakeStorage = { getItem: jest.Mock, setItem: jest.Mock, items: Map<string, string> };

function memoryStorage(initial: Record<string, string> = {}): FakeStorage {
    const items = new Map(Object.entries(initial));
    return {
        items,
        getItem: jest.fn((key: string) => items.get(key) ?? null),
        setItem: jest.fn((key: string, value: string) => {
            items.set(key, value);
        })
    };
}

/**
 * The reactions over real timelines holding the given messages in the open channel, a call path
 * answering each call with the next answer, a deferred one when the test resolves it by hand.
 */
async function setup(messages: TimelineMessage[], answers: Array<CallResult | Deferred<CallResult>> = [], options: { threads?: unknown, storage?: unknown } = {}): Promise<{
    reactions: ReturnType<typeof createReactions>,
    timelines: ReturnType<typeof createTimelines>,
    calls: Call[],
    notices: jest.Mock
}> {
    const timelines = createTimelines({
        limits: () => ({ history_page_size: 50 }),
        fetchPage: async () => ({ messages, read_cursor: null, unread_count: 0, has_more: false })
    });
    await timelines.loadNewest(channel);

    const calls: Call[] = [];
    const notices = jest.fn();
    const reactions = createReactions({
        call: (name: string, args: Record<string, unknown>) => {
            calls.push({ name, args });
            const answer = answers.shift() ?? ok({});
            return "promise" in answer ? answer.promise : Promise.resolve(answer);
        },
        timelines,
        personAliasGuid: me,
        notify: notices,
        favorites,
        churchKey: church,
        storage: "storage" in options ? options.storage : memoryStorage(),
        ...("threads" in options ? { threads: options.threads } : {})
    } as unknown as Parameters<typeof createReactions>[0]);

    return { reactions, timelines, calls, notices };
}

function chipsOf(timelines: ReturnType<typeof createTimelines>, id = 42): Array<{ code: string, count: number, mine: boolean }> {
    const held = timelines.state(channel).messages.find(m => m.id === id) as unknown as { reactions?: ReactionRow[] };
    return (held.reactions ?? []).map(r => ({ code: r.code, count: r.count, mine: r.mine }));
}

async function flush(): Promise<void> {
    for (let i = 0; i < 5; i++) {
        await Promise.resolve();
    }
}

function noticeTexts(notices: jest.Mock): string[] {
    return notices.mock.calls.map(([notice]) => (notice as { text: string }).text);
}

// #endregion Fixtures

describe("a toggle", () => {
    test("on an emoji the reader has not used calls chat_add_reaction, and lays the answer's list over the message", async () => {
        const { reactions, timelines, calls } = await setup([message(42, [rx("heart", 4)])], [ok({ message_id: 42, reactions: [rx("heart", 5, true)] })]);

        await reactions.toggle(channel, 42, glyph("heart"));

        expect(calls).toEqual([{ name: "chat_add_reaction", args: { p_message_id: 42, p_reaction: "heart" } }]);
        expect(chipsOf(timelines)).toEqual([{ code: "heart", count: 5, mine: true }]);
    });

    test("on the reader's own reaction calls chat_remove_reaction", async () => {
        const { reactions, timelines, calls } = await setup([message(42, [rx("heart", 5, true), rx("pray", 1)])], [ok({ message_id: 42, reactions: [rx("heart", 4), rx("pray", 1)] })]);

        await reactions.toggle(channel, 42, glyph("heart"));

        expect(calls).toEqual([{ name: "chat_remove_reaction", args: { p_message_id: 42, p_reaction: "heart" } }]);
        expect(chipsOf(timelines)).toEqual([{ code: "heart", count: 4, mine: false }, { code: "pray", count: 1, mine: false }]);
    });

    test("is drawn at once, before the answer: the count and the reader's mark move together", async () => {
        const answer = deferred<CallResult>();
        const { reactions, timelines } = await setup([message(42, [rx("heart", 4)])], [answer]);

        const toggling = reactions.toggle(channel, 42, glyph("heart"));

        expect(chipsOf(timelines)).toEqual([{ code: "heart", count: 5, mine: true }]);
        answer.resolve(ok({ message_id: 42, reactions: [rx("heart", 5, true)] }));
        await toggling;
    });

    test("the answer is the authority: a count the server says moved further than the press wins", async () => {
        const { reactions, timelines } = await setup([message(42, [rx("heart", 4)])], [ok({ message_id: 42, reactions: [rx("heart", 7, true)] })]);

        await reactions.toggle(channel, 42, glyph("heart"));

        expect(chipsOf(timelines)).toEqual([{ code: "heart", count: 7, mine: true }]);
    });

    test("removing the last reaction of an emoji takes its chip away at once", async () => {
        const answer = deferred<CallResult>();
        const { reactions, timelines } = await setup([message(42, [rx("thumbsup", 2), rx("heart", 1, true)])], [answer]);

        const toggling = reactions.toggle(channel, 42, glyph("heart"));

        expect(chipsOf(timelines)).toEqual([{ code: "thumbsup", count: 2, mine: false }]);
        answer.resolve(ok({ message_id: 42, reactions: [rx("thumbsup", 2)] }));
        await toggling;
        expect(chipsOf(timelines)).toEqual([{ code: "thumbsup", count: 2, mine: false }]);
    });

    test.each([
        ["rpc.message_deleted", "This message was deleted."],
        ["rpc.bad_reaction", "That reaction isn't available."],
        ["rpc.message_hidden", "This message is hidden."]
    ])("a refusal, %s, toasts the server's sentence and puts the chips back as they were", async (code, text) => {
        const answer = deferred<CallResult>();
        const { reactions, timelines, notices } = await setup([message(42, [rx("heart", 4), rx("pray", 1, true)])], [answer]);

        const toggling = reactions.toggle(channel, 42, glyph("joy"));
        expect(chipsOf(timelines).map(c => c.code)).toContain("joy");

        answer.resolve(refused(code, text));
        await toggling;

        expect(noticeTexts(notices)).toEqual([text]);
        expect(chipsOf(timelines)).toEqual([{ code: "heart", count: 4, mine: false }, { code: "pray", count: 1, mine: true }]);
    });

    test("reaches the copy of the message the open thread holds as well", async () => {
        const root = message(42, [rx("heart", 4)]);
        const threads = { whenLoaded: (_channelId: string, change: () => void) => change(), state: { thread: { channelId: channel, rootId: 42, root: message(42, [rx("heart", 4)]), replies: [message(43, [], { parent_id: 42 })] } } };
        const { reactions } = await setup([root], [ok({ message_id: 42, reactions: [rx("heart", 5, true)] })], { threads });

        await reactions.toggle(channel, 42, glyph("heart"));

        const threadRoot = threads.state.thread.root as unknown as { reactions: ReactionRow[] };
        expect(threadRoot.reactions.map(r => [r.code, r.count, r.mine])).toEqual([["heart", 5, true]]);
    });

    test("on a reply held only in the open thread reacts to that reply", async () => {
        const reply = message(43, [], { parent_id: 42, shown_in_channel: false });
        const threads = { whenLoaded: (_channelId: string, change: () => void) => change(), state: { thread: { channelId: channel, rootId: 42, root: message(42), replies: [reply] } } };
        const { reactions, calls } = await setup([message(42)], [ok({ message_id: 43, reactions: [rx("pray", 1, true)] })], { threads });

        await reactions.toggle(channel, 43, glyph("pray"));

        expect(calls).toEqual([{ name: "chat_add_reaction", args: { p_message_id: 43, p_reaction: "pray" } }]);
        expect(((reply as unknown as { reactions: ReactionRow[] }).reactions).map(r => [r.code, r.count, r.mine])).toEqual([["pray", 1, true]]);
    });
});

// The reader's own reaction comes back to them twice: in the add's answer and as the event every
// viewer of the room hears. Whichever arrives first, the chip counts the reaction once.
// The bar, the sheet and the picker know only the code they were pressed with; the glyph a new
// chip draws comes from the message's own chip, the catalog once fetched, or the favorites.
describe("a toggle by code", () => {
    function heldChips(timelines: ReturnType<typeof createTimelines>): ReactionRow[] {
        const held = timelines.state(channel).messages.find(m => m.id === 42) as unknown as { reactions?: ReactionRow[] };
        return (held.reactions ?? []).map(r => ({ ...r }));
    }

    test("on a favorite the message has none of draws its chip with the favorite's glyph and label", async () => {
        const answer = deferred<CallResult>();
        const { reactions, timelines, calls } = await setup([message(42)], [answer]);

        const toggling = reactions.toggleCode(channel, 42, "pray");

        expect(calls).toEqual([{ name: "chat_add_reaction", args: { p_message_id: 42, p_reaction: "pray" } }]);
        expect(heldChips(timelines)).toEqual([rx("pray", 1, true)]);
        answer.resolve(ok({ message_id: 42, reactions: [rx("pray", 1, true)] }));
        await toggling;
    });

    test("on an emoji only the catalog holds takes the catalog's glyph", async () => {
        const answer = deferred<CallResult>();
        const { reactions, timelines } = await setup([message(42)], [ok({ emojis: catalog }), answer]);
        await reactions.loadCatalog();

        const toggling = reactions.toggleCode(channel, 42, "grinning");

        expect(heldChips(timelines)).toEqual([rx("grinning", 1, true)]);
        answer.resolve(ok({ message_id: 42, reactions: [rx("grinning", 1, true)] }));
        await toggling;
    });

    test("on the reader's own chip removes it", async () => {
        const { reactions, calls } = await setup([message(42, [rx("heart", 3, true)])], [ok({ message_id: 42, reactions: [rx("heart", 2)] })]);

        await reactions.toggleCode(channel, 42, "heart");

        expect(calls).toEqual([{ name: "chat_remove_reaction", args: { p_message_id: 42, p_reaction: "heart" } }]);
    });
});

describe("the reader's own reaction and its echo", () => {
    test.each([["the answer before the echo", "answer-first"], ["the echo before the answer", "echo-first"]] as const)(
        "count it once: %s",
        async (_order, order) => {
            const answer = deferred<CallResult>();
            const { reactions, timelines } = await setup([message(42, [rx("heart", 4)])], [answer]);

            const toggling = reactions.toggle(channel, 42, glyph("heart"));
            if (order === "answer-first") {
                answer.resolve(ok({ message_id: 42, reactions: [rx("heart", 5, true)] }));
                await toggling;
                reactions.applyEvent(channel, "reaction.added", added("heart", me));
            }
            else {
                reactions.applyEvent(channel, "reaction.added", added("heart", me));
                expect(chipsOf(timelines)).toEqual([{ code: "heart", count: 5, mine: true }]);
                answer.resolve(ok({ message_id: 42, reactions: [rx("heart", 5, true)] }));
                await toggling;
            }

            expect(chipsOf(timelines)).toEqual([{ code: "heart", count: 5, mine: true }]);
        }
    );

    test.each([["the answer before the echo", "answer-first"], ["the echo before the answer", "echo-first"]] as const)(
        "a removal counts once too: %s",
        async (_order, order) => {
            const answer = deferred<CallResult>();
            const { reactions, timelines } = await setup([message(42, [rx("heart", 5, true)])], [answer]);

            const toggling = reactions.toggle(channel, 42, glyph("heart"));
            if (order === "answer-first") {
                answer.resolve(ok({ message_id: 42, reactions: [rx("heart", 4)] }));
                await toggling;
                reactions.applyEvent(channel, "reaction.removed", added("heart", me));
            }
            else {
                reactions.applyEvent(channel, "reaction.removed", added("heart", me));
                answer.resolve(ok({ message_id: 42, reactions: [rx("heart", 4)] }));
                await toggling;
            }

            expect(chipsOf(timelines)).toEqual([{ code: "heart", count: 4, mine: false }]);
        }
    );

    test("a reaction from the reader's other device marks the chip theirs and counts it", async () => {
        const { reactions, timelines } = await setup([message(42, [rx("heart", 4)])]);

        reactions.applyEvent(channel, "reaction.added", added("heart", me));

        expect(chipsOf(timelines)).toEqual([{ code: "heart", count: 5, mine: true }]);
    });

    test("an event of the reader's that would not change their mark changes nothing", async () => {
        const { reactions, timelines } = await setup([message(42, [rx("heart", 5, true), rx("pray", 2)])]);

        reactions.applyEvent(channel, "reaction.added", added("heart", me));
        reactions.applyEvent(channel, "reaction.removed", added("pray", me));

        expect(chipsOf(timelines)).toEqual([{ code: "heart", count: 5, mine: true }, { code: "pray", count: 2, mine: false }]);
    });
});

describe("another person's reaction", () => {
    test("added moves the chip's count up by one and leaves the reader's mark", async () => {
        const { reactions, timelines } = await setup([message(42, [rx("heart", 4, true)])]);

        reactions.applyEvent(channel, "reaction.added", added("heart", anna));

        expect(chipsOf(timelines)).toEqual([{ code: "heart", count: 5, mine: true }]);
    });

    test("removed moves it down by one, and a chip at zero leaves", async () => {
        const { reactions, timelines } = await setup([message(42, [rx("thumbsup", 1), rx("heart", 2)])]);

        reactions.applyEvent(channel, "reaction.removed", added("heart", anna));
        expect(chipsOf(timelines)).toEqual([{ code: "thumbsup", count: 1, mine: false }, { code: "heart", count: 1, mine: false }]);

        reactions.applyEvent(channel, "reaction.removed", added("thumbsup", david));
        expect(chipsOf(timelines)).toEqual([{ code: "heart", count: 1, mine: false }]);
    });

    test("on an emoji the message has none of draws a new chip with the event's glyph and label", async () => {
        const { reactions, timelines } = await setup([message(42)]);

        reactions.applyEvent(channel, "reaction.added", added("pray", anna));

        const held = timelines.state(channel).messages.find(m => m.id === 42) as unknown as { reactions: ReactionRow[] };
        expect(held.reactions).toEqual([{ code: "pray", count: 1, mine: false, emoji: "🙏", image_url: null, label: "folded hands" }]);
    });

    test("on a message this page does not hold changes nothing and throws nothing", async () => {
        const { reactions, timelines } = await setup([message(42, [rx("heart", 1)])]);

        reactions.applyEvent(channel, "reaction.added", added("heart", anna, 999));

        expect(chipsOf(timelines)).toEqual([{ code: "heart", count: 1, mine: false }]);
    });

    // The server orders a page's chips by the catalog's picker order, which is not the favorites'
    // order, so a chip arriving live lands where a reload would draw it.
    test("a new chip lands in the catalog's picker order among the chips already there", async () => {
        const { reactions, timelines } = await setup([message(42, [rx("thumbsup", 2), rx("pray", 1)])], [ok({ emojis: catalog })]);
        await reactions.loadCatalog();

        reactions.applyEvent(channel, "reaction.added", added("heart", anna));
        expect(chipsOf(timelines).map(c => c.code)).toEqual(["heart", "thumbsup", "pray"]);

        reactions.applyEvent(channel, "reaction.added", added("fire", anna));
        expect(chipsOf(timelines).map(c => c.code)).toEqual(["heart", "thumbsup", "pray", "fire"]);
    });

    test("a new chip of an emoji outside the offered catalog lands last, as first use orders it", async () => {
        const { reactions, timelines } = await setup([message(42, [rx("thumbsup", 2), rx("pray", 1)])], [ok({ emojis: catalog })]);
        await reactions.loadCatalog();

        reactions.applyEvent(channel, "reaction.added", { ...added("thumbsup", anna), code: "smirk", emoji: "😏", label: "smirking face" });

        expect(chipsOf(timelines).map(c => c.code)).toEqual(["thumbsup", "pray", "smirk"]);
    });
});

describe("the catalog", () => {
    test("is fetched with chat_list_emojis once for the page, however often the picker opens", async () => {
        const { reactions, calls } = await setup([message(42)], [ok({ emojis: catalog })]);

        const first = await reactions.loadCatalog();
        const second = await reactions.loadCatalog();
        await reactions.loadCatalog();

        expect(calls).toEqual([{ name: "chat_list_emojis", args: {} }]);
        expect(first?.map(e => e.code)).toEqual(catalog.map(e => e.code));
        expect(second).toBe(first);
        expect(reactions.state.catalog?.map(e => e.code)).toEqual(catalog.map(e => e.code));
    });

    test("two opens racing share the one call", async () => {
        const answer = deferred<CallResult>();
        const { reactions, calls } = await setup([message(42)], [answer]);

        const first = reactions.loadCatalog();
        const second = reactions.loadCatalog();
        expect(calls).toHaveLength(1);

        answer.resolve(ok({ emojis: catalog }));

        expect((await first)?.length).toBe(catalog.length);
        expect((await second)?.length).toBe(catalog.length);
        expect(calls).toHaveLength(1);
    });

    test("a failed fetch is asked for again at the next open", async () => {
        const { reactions, calls } = await setup([message(42)], [refused("rt.unavailable", "Chat is busy right now."), ok({ emojis: catalog })]);

        expect(await reactions.loadCatalog()).toBeNull();
        expect(reactions.state.catalog).toBeNull();

        expect((await reactions.loadCatalog())?.length).toBe(catalog.length);
        expect(calls.map(c => c.name)).toEqual(["chat_list_emojis", "chat_list_emojis"]);
    });
});

// Kept on the device by church and person, read and written in try and catch, so a blocked
// store only costs the section on the next page.
describe("the recently used emoji", () => {
    test("keep the last eight codes, newest first, each once", async () => {
        const { reactions } = await setup([message(42)]);

        ["a", "b", "c", "a", "d", "e", "f", "g", "h", "i"].forEach(code => reactions.recordRecent(code));

        expect(reactions.state.recents).toEqual(["i", "h", "g", "f", "e", "d", "a", "c"]);
    });

    test("are written under a key naming the church and the person, and read back by the next page", async () => {
        const storage = memoryStorage();
        const first = await setup([message(42)], [], { storage });

        first.reactions.recordRecent("pray");
        first.reactions.recordRecent("heart");

        const [key, value] = storage.setItem.mock.calls[storage.setItem.mock.calls.length - 1] as [string, string];
        expect(key).toContain(church);
        expect(key).toContain(me);
        expect(JSON.parse(value)).toEqual(["heart", "pray"]);

        const next = await setup([message(42)], [], { storage });
        expect(next.reactions.state.recents).toEqual(["heart", "pray"]);
    });

    test("read nothing they cannot trust: a stored value that is not a list of codes is none", async () => {
        const storage = memoryStorage();
        storage.getItem.mockImplementation(() => "{\"not\":\"a list\"}");

        const { reactions } = await setup([message(42)], [], { storage });

        expect(reactions.state.recents).toEqual([]);
    });

    test("survive a store that throws on read and on write: none at first, and still kept for the page", async () => {
        const storage = {
            getItem: jest.fn(() => {
                throw new Error("SecurityError");
            }),
            setItem: jest.fn(() => {
                throw new Error("QuotaExceededError");
            })
        };

        const { reactions } = await setup([message(42)], [], { storage });
        expect(reactions.state.recents).toEqual([]);

        expect(() => reactions.recordRecent("pray")).not.toThrow();
        expect(reactions.state.recents).toEqual(["pray"]);
    });

    test("work with no store at all", async () => {
        const { reactions } = await setup([message(42)], [], { storage: null });

        reactions.recordRecent("pray");

        expect(reactions.state.recents).toEqual(["pray"]);
    });
});

// Each chip carries its newest three reactors, so who reacted is on the message before anyone
// rests on a chip: an event lays the chip's three as the server read them after the change, an
// answer copies the whole chip, and nothing is asked for on a hover.
describe("who reacted", () => {
    const person = (alias: string, nick: string | null, last: string | null, at = "2026-10-16T09:01:00Z"): Record<string, unknown> => ({
        person_alias_guid: alias, nick_name: nick, last_name: last, avatar_url: null, reacted_at: at, reactor_person_alias_guid: alias
    });

    function heldReactors(timelines: ReturnType<typeof createTimelines>, code: string): unknown {
        const held = timelines.state(channel).messages.find(m => m.id === 42) as unknown as { reactions?: Array<{ code: string, reactors?: unknown }> };
        return held.reactions?.find(r => r.code === code)?.reactors;
    }

    test("another person's reaction lays the event's three on the chip, the count still moved by one", async () => {
        const { reactions, timelines } = await setup([message(42, [{ ...rx("pray", 2, true), reactors: [person(me, "Jordan", "Ellis")] } as never])]);
        const three = [person(anna, "Anna", "Reyes"), person(me, "Jordan", "Ellis")];

        reactions.applyEvent(channel, "reaction.added", { ...added("pray", anna), reactors: three });

        expect(chipsOf(timelines)).toEqual([{ code: "pray", count: 3, mine: true }]);
        expect(heldReactors(timelines, "pray")).toEqual(three);
    });

    test("a new chip from an event carries the event's three", async () => {
        const { reactions, timelines } = await setup([message(42)]);

        reactions.applyEvent(channel, "reaction.added", { ...added("pray", anna), reactors: [person(anna, "Anna", "Reyes")] });

        expect(heldReactors(timelines, "pray")).toEqual([person(anna, "Anna", "Reyes")]);
    });

    test("a removal lays the three that are left", async () => {
        const { reactions, timelines } = await setup([message(42, [{ ...rx("pray", 2), reactors: [person(anna, "Anna", "Reyes"), person(david, "David", "Okafor")] } as never])]);

        reactions.applyEvent(channel, "reaction.removed", { ...added("pray", anna), reactors: [person(david, "David", "Okafor")] });

        expect(heldReactors(timelines, "pray")).toEqual([person(david, "David", "Okafor")]);
    });

    test("the reader's own echo lays the three too, though it leaves the count and mark as the press drew them", async () => {
        const { reactions, timelines } = await setup([message(42, [{ ...rx("pray", 2, true), reactors: [person(anna, "Anna", "Reyes")] } as never])]);
        const three = [person(me, "Jordan", "Ellis"), person(anna, "Anna", "Reyes")];

        reactions.applyEvent(channel, "reaction.added", { ...added("pray", me), reactors: three });

        expect(chipsOf(timelines)).toEqual([{ code: "pray", count: 2, mine: true }]);
        expect(heldReactors(timelines, "pray")).toEqual(three);
    });

    test("an event carrying no three leaves the chip's own", async () => {
        const held = [person(anna, "Anna", "Reyes")];
        const { reactions, timelines } = await setup([message(42, [{ ...rx("pray", 1), reactors: held } as never])]);

        reactions.applyEvent(channel, "reaction.added", added("pray", david));

        expect(heldReactors(timelines, "pray")).toEqual(held);
    });

    test("an answer read before an event for the message keeps the event's counts and faces, and lays only the pressed mark", async () => {
        const answer = deferred<CallResult>();
        const { reactions, timelines } = await setup([message(42, [{ ...rx("pray", 1), reactors: [person(anna, "Anna", "Reyes")] } as never])], [answer]);
        const prayThree = [person(david, "David", "Okafor"), person(anna, "Anna", "Reyes")];
        const heartThree = [person(david, "David", "Okafor"), person(me, "Jordan", "Ellis")];

        const pressing = reactions.toggle(channel, 42, glyph("heart"));
        reactions.applyEvent(channel, "reaction.added", { ...added("pray", david), reactors: prayThree });
        reactions.applyEvent(channel, "reaction.added", { ...added("heart", david), reactors: heartThree });
        answer.resolve(ok({ message_id: 42, reactions: [{ ...rx("pray", 1), reactors: [person(anna, "Anna", "Reyes")] }, { ...rx("heart", 1, true), reactors: [person(me, "Jordan", "Ellis")] }] }));
        await pressing;

        expect(chipsOf(timelines)).toEqual([{ code: "pray", count: 2, mine: false }, { code: "heart", count: 2, mine: true }]);
        expect(heldReactors(timelines, "pray")).toEqual(prayThree);
        expect(heldReactors(timelines, "heart")).toEqual(heartThree);
    });

    test("an answer's chip brings its three", async () => {
        const three = [person(me, "Jordan", "Ellis"), person(anna, "Anna", "Reyes")];
        const { reactions, timelines } = await setup([message(42, [rx("pray", 1)])], [ok({ message_id: 42, reactions: [{ ...rx("pray", 2, true), reactors: three }] })]);

        await reactions.toggle(channel, 42, glyph("pray"));

        expect(heldReactors(timelines, "pray")).toEqual(three);
    });
});

// Everyone who reacted, behind See all: one emoji or every emoji, a page at a time after the last
// row held, the emoji part of the cursor since one person can react with two at the same moment.
describe("the full list", () => {
    const row = { person_alias_guid: anna, nick_name: "Anna", last_name: "Reyes", avatar_url: null, reacted_at: "2026-10-16T09:01:00Z", reactor_person_alias_guid: anna, code: "pray" };

    test("asks chat_list_reactors for one emoji's first page, 50 at a time", async () => {
        const { reactions, calls } = await setup([message(42, [rx("pray", 3)])], [ok({ reactors: [row], has_more: false })]);

        const page = await reactions.listReactors(42, "pray", null);

        expect(calls).toEqual([{ name: "chat_list_reactors", args: { p_message_id: 42, p_reaction: "pray", p_after_at: null, p_after_alias: null, p_limit: 50, p_after_code: null } }]);
        expect(page).toEqual({ reactors: [row], has_more: false });
    });

    test("asks for every emoji with none named, and pages after a row by its time, alias and emoji", async () => {
        const { reactions, calls } = await setup([message(42, [rx("pray", 3)])], [ok({ reactors: [], has_more: false })]);

        await reactions.listReactors(42, null, { reacted_at: row.reacted_at, reactor_person_alias_guid: anna, code: "pray" });

        expect(calls[0].args).toEqual({ p_message_id: 42, p_reaction: null, p_after_at: row.reacted_at, p_after_alias: anna, p_limit: 50, p_after_code: "pray" });
    });

    test("a refusal answers null", async () => {
        const { reactions } = await setup([message(42, [rx("pray", 3)])], [refused("rpc.message_not_found", "That message is no longer here.")]);

        expect(await reactions.listReactors(42, "pray", null)).toBeNull();
    });
});

// The card's people: You first from the chip's mark, whatever the newest three hold, then the
// others the chip carries; a person the platform cannot name is counted, never drawn blank.
describe("the card's people", () => {
    const reader = { personAliasGuid: me, name: "Jordan Ellis", photoUrl: "https://rock.example.org/jordan.jpg" };
    const person = (alias: string, nick: string | null, last: string | null, avatar: string | null = null): Record<string, unknown> => ({
        person_alias_guid: alias, nick_name: nick, last_name: last, avatar_url: avatar, reacted_at: "2026-10-16T09:01:00Z", reactor_person_alias_guid: alias
    });
    const cara = "a0000004-0000-4000-8000-000000000000";
    const chip = (count: number, mine: boolean, reactors: unknown[]): never => ({ ...rx("pray", count, mine), reactors }) as never;

    test("names the newest three, and how many more", () => {
        const card = whoReacted(chip(12, false, [person(anna, "Anna", "Reyes", "https://rock.example.org/anna.jpg"), person(david, "David", "Okafor"), person(cara, "Cara", "Lin")]), reader);

        expect(card.people.map(p => [p.label, p.face.photoUrl])).toEqual([["Anna Reyes", "https://rock.example.org/anna.jpg"], ["David Okafor", null], ["Cara Lin", null]]);
        expect(card.rest).toBe(9);
        expect(card.isMoreShown).toBe(true);
    });

    test("leads with You from the chip's mark, with the reader's own photo, even when the reader is not among the three", () => {
        const card = whoReacted(chip(12, true, [person(anna, "Anna", "Reyes"), person(david, "David", "Okafor"), person(cara, "Cara", "Lin")]), reader);

        expect(card.people.map(p => p.label)).toEqual(["You", "Anna Reyes", "David Okafor"]);
        expect(card.people[0].face).toEqual({ name: "Jordan Ellis", photoUrl: "https://rock.example.org/jordan.jpg" });
        expect(card.rest).toBe(9);
    });

    test("never names the reader twice, under their primary or the alias they reacted with", () => {
        const underOther = { ...person(me, "Jordan", "Ellis"), person_alias_guid: "a0000009-0000-4000-8000-000000000000", reactor_person_alias_guid: me };
        const card = whoReacted(chip(3, true, [person(me, "Jordan", "Ellis"), underOther, person(anna, "Anna", "Reyes")]), reader);

        expect(card.people.map(p => p.label)).toEqual(["You", "Anna Reyes"]);
        expect(card.rest).toBe(1);
    });

    test("names a person once while two of their aliases are both carried, as during a merge", () => {
        const underOther = { ...person(anna, "Anna", "Reyes"), reactor_person_alias_guid: "a0000009-0000-4000-8000-000000000000" };
        const card = whoReacted(chip(4, false, [person(anna, "Anna", "Reyes"), underOther, person(david, "David", "Okafor")]), reader);

        expect(card.people.map(p => p.key)).toEqual([anna, david]);
    });

    test("leaves a nameless person to the count", () => {
        const card = whoReacted(chip(2, false, [person(david, null, null), person(anna, "Anna", "Reyes")]), reader);

        expect(card.people.map(p => p.label)).toEqual(["Anna Reyes"]);
        expect(card.rest).toBe(1);
        expect(card.isMoreShown).toBe(true);
    });

    test("draws no more row when everyone is named", () => {
        const card = whoReacted(chip(2, true, [person(me, "Jordan", "Ellis"), person(anna, "Anna", "Reyes")]), reader);

        expect(card.rest).toBe(0);
        expect(card.isMoreShown).toBe(false);
    });

    test("one nameless person is the count alone, with nothing more to see", () => {
        const card = whoReacted(chip(1, false, [person(david, null, null)]), reader);

        expect(card.people).toEqual([]);
        expect(card.isMoreShown).toBe(false);
    });

    test("nameless people the chip does not all carry still offer the rest", () => {
        const card = whoReacted(chip(5, false, [person(david, null, null), person(anna, null, null), person(cara, null, null)]), reader);

        expect(card.people).toEqual([]);
        expect(card.rest).toBe(5);
        expect(card.isMoreShown).toBe(true);
    });

    test("a chip from a platform that carries no three is You or the count", () => {
        expect(whoReacted({ ...rx("pray", 3, true) } as never, reader).people.map(p => p.label)).toEqual(["You"]);
        expect(whoReacted({ ...rx("pray", 3) } as never, reader).people).toEqual([]);
    });

    test.each([
        [chip(12, true, [person(anna, "Anna", "Reyes"), person(david, "David", "Okafor")]), "You, Anna Reyes, David Okafor and 9 others reacted with folded hands"],
        [chip(2, false, [person(anna, "Anna", "Reyes"), person(david, "David", "Okafor")]), "Anna Reyes and David Okafor reacted with folded hands"],
        [chip(2, false, [person(anna, "Anna", "Reyes")]), "Anna Reyes and 1 other reacted with folded hands"],
        [chip(1, true, []), "You reacted with folded hands"],
        [chip(1, false, [person(david, null, null)]), "1 person reacted with folded hands"],
        [chip(5, false, []), "5 people reacted with folded hands"]
    ])("reads as one sentence for a screen reader: %#", (reaction, sentence) => {
        expect(whoReactedLine(reaction, reader)).toBe(sentence);
    });
});

// A person can press again before the first answer is back, and answers can come back in either
// order. A press is answered for by its own emoji: a newer press of the same emoji overtakes it,
// a press of another emoji does not, and a refusal takes back only its own emoji's press.
describe("presses whose answers cross", () => {
    test("two quick toggles of one emoji, the first answer late: the second press stands", async () => {
        const first = deferred<CallResult>();
        const second = deferred<CallResult>();
        const { reactions, timelines } = await setup([message(42, [rx("heart", 4)])], [first, second]);

        const adding = reactions.toggle(channel, 42, glyph("heart"));
        const removing = reactions.toggle(channel, 42, glyph("heart"));
        expect(chipsOf(timelines)).toEqual([{ code: "heart", count: 4, mine: false }]);

        second.resolve(ok({ message_id: 42, reactions: [rx("heart", 4)] }));
        await removing;
        first.resolve(ok({ message_id: 42, reactions: [rx("heart", 5, true)] }));
        await adding;

        expect(chipsOf(timelines)).toEqual([{ code: "heart", count: 4, mine: false }]);
    });

    test("a double tap sends the remove only once the add is answered, so the server never sees them cross", async () => {
        const first = deferred<CallResult>();
        const second = deferred<CallResult>();
        const { reactions, timelines, calls } = await setup([message(42, [rx("heart", 4)])], [first, second]);

        const adding = reactions.toggle(channel, 42, glyph("heart"));
        const removing = reactions.toggle(channel, 42, glyph("heart"));
        await flush();
        expect(calls.map(c => c.name)).toEqual(["chat_add_reaction"]);
        expect(chipsOf(timelines)).toEqual([{ code: "heart", count: 4, mine: false }]);

        first.resolve(ok({ message_id: 42, reactions: [rx("heart", 5, true)] }));
        await flush();
        expect(calls.map(c => c.name)).toEqual(["chat_add_reaction", "chat_remove_reaction"]);
        // the add's answer is not what the person last asked for, so it is not drawn
        expect(chipsOf(timelines)).toEqual([{ code: "heart", count: 4, mine: false }]);

        second.resolve(ok({ message_id: 42, reactions: [rx("heart", 4)] }));
        await Promise.all([adding, removing]);
        expect(chipsOf(timelines)).toEqual([{ code: "heart", count: 4, mine: false }]);
    });

    test("three taps while the first is out send nothing more once it is answered, the reaction then as asked", async () => {
        const first = deferred<CallResult>();
        const { reactions, timelines, calls } = await setup([message(42, [rx("heart", 4)])], [first]);

        const pressing = [1, 2, 3].map(() => reactions.toggle(channel, 42, glyph("heart")));
        first.resolve(ok({ message_id: 42, reactions: [rx("heart", 5, true)] }));
        await Promise.all(pressing);

        expect(calls.map(c => c.name)).toEqual(["chat_add_reaction"]);
        expect(chipsOf(timelines)).toEqual([{ code: "heart", count: 5, mine: true }]);
    });

    test("two emoji on one message, answered out of order: neither answer wipes the other press", async () => {
        const thumbs = deferred<CallResult>();
        const heart = deferred<CallResult>();
        const { reactions, timelines } = await setup([message(42)], [thumbs, heart]);

        const pressingThumbs = reactions.toggle(channel, 42, glyph("thumbsup"));
        const pressingHeart = reactions.toggle(channel, 42, glyph("heart"));

        // the heart's answer was counted before the thumbs up reached the server
        heart.resolve(ok({ message_id: 42, reactions: [rx("heart", 1, true)] }));
        await pressingHeart;
        expect(chipsOf(timelines)).toEqual([{ code: "thumbsup", count: 1, mine: true }, { code: "heart", count: 1, mine: true }]);

        // and the thumbs up's was counted before the heart reached it
        thumbs.resolve(ok({ message_id: 42, reactions: [rx("thumbsup", 1, true)] }));
        await pressingThumbs;
        expect(chipsOf(timelines)).toEqual([{ code: "thumbsup", count: 1, mine: true }, { code: "heart", count: 1, mine: true }]);
    });

    test.each([["the newest refused first", "newest-first"], ["the oldest refused first", "oldest-first"]] as const)(
        "two emoji both refused, %s: each press is taken back",
        async (_order, order) => {
            const thumbs = deferred<CallResult>();
            const heart = deferred<CallResult>();
            const { reactions, timelines } = await setup([message(42, [rx("pray", 1)])], [thumbs, heart]);

            const pressingThumbs = reactions.toggle(channel, 42, glyph("thumbsup"));
            const pressingHeart = reactions.toggle(channel, 42, glyph("heart"));
            const [firstOut, secondOut] = order === "newest-first" ? [heart, thumbs] : [thumbs, heart];
            firstOut.resolve(refused("rpc.message_hidden", "This message is hidden."));
            secondOut.resolve(refused("rpc.message_hidden", "This message is hidden."));
            await pressingThumbs;
            await pressingHeart;

            expect(chipsOf(timelines)).toEqual([{ code: "pray", count: 1, mine: false }]);
        }
    );

    test("one emoji pressed twice and both refused: the chip is back as it was before either", async () => {
        const first = deferred<CallResult>();
        const second = deferred<CallResult>();
        const { reactions, timelines } = await setup([message(42, [rx("heart", 4)])], [first, second]);

        const adding = reactions.toggle(channel, 42, glyph("heart"));
        const removing = reactions.toggle(channel, 42, glyph("heart"));
        second.resolve(refused("rpc.message_hidden", "This message is hidden."));
        first.resolve(refused("rpc.message_hidden", "This message is hidden."));
        await removing;
        await adding;

        expect(chipsOf(timelines)).toEqual([{ code: "heart", count: 4, mine: false }]);
    });
});

// A page can be read before a reaction and land after it: a reload, a catch-up after a rejoin,
// the channel's first page, or an open thread's. What the reaction changed is laid over that page
// rather than undone by it, and a reaction a page read after it already counts is not counted again.
describe("a page read before a reaction", () => {
    type Paged = {
        reactions: ReturnType<typeof createReactions>,
        timelines: ReturnType<typeof createTimelines>,
        threads: ReturnType<typeof createThreads>,
        pages: Array<Deferred<HistoryPage>>,
        threadPage: Deferred<CallResult>,
        answers: Array<CallResult | Deferred<CallResult>>,
        deliver: (event: "reaction.added" | "reaction.removed", payload: unknown) => void
    };

    const pageOf = (messages: TimelineMessage[]): HistoryPage => ({ messages, read_cursor: null, unread_count: 0, has_more: false });

    /**
     * The reactions over real timelines and threads whose every page is answered by hand, with a
     * reaction event routed as the shell routes it: to the open thread's copies at once, and to the
     * conversation's through the timelines.
     */
    function paged(): Paged {
        const pages: Array<Deferred<HistoryPage>> = [];
        const threadPage = deferred<CallResult>();
        const answers: Array<CallResult | Deferred<CallResult>> = [];
        let reactions: ReturnType<typeof createReactions> | null = null;
        const timelines = createTimelines({
            limits: () => ({ history_page_size: 50 }),
            fetchPage: () => {
                const page = deferred<HistoryPage>();
                pages.push(page);
                return page.promise;
            },
            applyReaction: (channelId: string, event: string, payload: unknown) => reactions?.applyEvent(channelId, event as "reaction.added", payload)
        } as never);
        const threads = createThreads({ limits: () => ({ history_page_size: 50, thread_page_size: 30 }), call: () => threadPage.promise, timelines, personAliasGuid: me });
        reactions = createReactions({
            call: () => {
                const answer = answers.shift() ?? ok({});
                return "promise" in answer ? answer.promise : Promise.resolve(answer);
            },
            timelines,
            threads,
            personAliasGuid: me,
            notify: jest.fn(),
            favorites,
            churchKey: church,
            storage: null
        } as never);

        const deliver = (event: "reaction.added" | "reaction.removed", payload: unknown): void => {
            reactions?.applyThreadEvent(channel, event, payload);
            timelines.applyEvent(channel, event, payload);
        };

        return { reactions, timelines, threads, pages, threadPage, answers, deliver };
    }

    async function loaded(messages: TimelineMessage[]): Promise<Paged> {
        const p = paged();
        const loading = p.timelines.loadNewest(channel);
        p.pages[0].resolve(pageOf(messages));
        await loading;
        return p;
    }

    function threadChips(held: TimelineMessage | null | undefined): Array<{ code: string, count: number, mine: boolean }> {
        return ((held as unknown as { reactions?: ReactionRow[] } | null)?.reactions ?? []).map(r => ({ code: r.code, count: r.count, mine: r.mine }));
    }

    test("a toggle's answer is not undone by a newest page read before it", async () => {
        const p = await loaded([message(42, [rx("heart", 4)])]);
        p.answers.push(ok({ message_id: 42, reactions: [rx("heart", 5, true)] }));

        const reloading = p.timelines.loadNewest(channel);
        await p.reactions.toggle(channel, 42, glyph("heart"));
        p.pages[1].resolve(pageOf([message(42, [rx("heart", 4)])]));
        await reloading;

        expect(chipsOf(p.timelines)).toEqual([{ code: "heart", count: 5, mine: true }]);
    });

    test("another person's reaction is not undone by a newest page read before it", async () => {
        const p = await loaded([message(42, [rx("heart", 4)])]);

        const reloading = p.timelines.loadNewest(channel);
        p.deliver("reaction.added", added("heart", anna));
        p.pages[1].resolve(pageOf([message(42, [rx("heart", 4)])]));
        await reloading;

        expect(chipsOf(p.timelines)).toEqual([{ code: "heart", count: 5, mine: false }]);
    });

    test("nor by a catch-up page read before it, after a rejoin", async () => {
        const p = await loaded([message(42, [rx("heart", 4)])]);
        const joining = p.timelines.onJoined(channel);
        await flush();
        p.pages[1].resolve(pageOf([]));
        await joining;

        const rejoining = p.timelines.onJoined(channel);
        await flush();
        p.deliver("reaction.added", added("heart", anna));
        p.pages[2].resolve(pageOf([message(42, [rx("heart", 4)])]));
        await rejoining;

        expect(chipsOf(p.timelines)).toEqual([{ code: "heart", count: 5, mine: false }]);
    });

    test("a reaction arriving while the channel's first page is on its way is applied once it lands", async () => {
        const p = paged();

        const loading = p.timelines.loadNewest(channel);
        p.deliver("reaction.added", added("heart", anna));
        p.pages[0].resolve(pageOf([message(42, [rx("heart", 4)])]));
        await loading;

        expect(chipsOf(p.timelines)).toEqual([{ code: "heart", count: 5, mine: false }]);
    });

    test("a reaction held before the channel's first page was read is not counted again by it", async () => {
        const p = paged();

        p.deliver("reaction.added", added("heart", anna));
        const loading = p.timelines.loadNewest(channel);
        p.pages[0].resolve(pageOf([message(42, [rx("heart", 5)])]));
        await loading;

        expect(chipsOf(p.timelines)).toEqual([{ code: "heart", count: 5, mine: false }]);
    });

    test("a toggle's answer on a root reaches the open thread's root, whose page was read before it", async () => {
        const p = await loaded([message(42, [rx("heart", 4)])]);
        p.answers.push(ok({ message_id: 42, reactions: [rx("heart", 5, true)] }));

        const opening = p.threads.openThread(channel, 42);
        await p.reactions.toggle(channel, 42, glyph("heart"));
        p.threadPage.resolve(ok({ root: message(42, [rx("heart", 4)]), messages: [], read_cursor: null, following: false, has_more: false }));
        await opening;

        expect(threadChips(p.threads.state.thread?.root)).toEqual([{ code: "heart", count: 5, mine: true }]);
        expect(chipsOf(p.timelines)).toEqual([{ code: "heart", count: 5, mine: true }]);
    });

    test("a reaction to a reply only the thread holds, arriving while its page is on its way, is applied once it lands", async () => {
        const p = await loaded([message(42)]);

        const opening = p.threads.openThread(channel, 42);
        p.deliver("reaction.added", added("pray", anna, 43));
        p.threadPage.resolve(ok({ root: message(42), messages: [message(43, [], { parent_id: 42, shown_in_channel: false })], read_cursor: null, following: false, has_more: false }));
        await opening;

        expect(threadChips(p.threads.state.thread?.replies.find(r => r.id === 43))).toEqual([{ code: "pray", count: 1, mine: false }]);
    });

    test("after the conversation's first page failed and a retry read it, the open thread's root moves once", async () => {
        const p = paged();
        const opening = p.threads.openThread(channel, 42);
        p.threadPage.resolve(ok({ root: message(42, [rx("heart", 4)]), messages: [], read_cursor: null, following: false, has_more: false }));
        await opening;

        const failing = p.timelines.loadNewest(channel);
        p.deliver("reaction.added", added("heart", anna));
        p.pages[0].reject(new Error("offline"));
        await expect(failing).rejects.toThrow("offline");

        const retrying = p.timelines.loadNewest(channel);
        p.pages[1].resolve(pageOf([message(42, [rx("heart", 5)])]));
        await retrying;

        expect(threadChips(p.threads.state.thread?.root)).toEqual([{ code: "heart", count: 5, mine: false }]);
        expect(chipsOf(p.timelines)).toEqual([{ code: "heart", count: 5, mine: false }]);
    });
});

describe("after stop", () => {
    test("a toggle's answer resolves nothing into state and toasts nothing", async () => {
        const answer = deferred<CallResult>();
        const { reactions, timelines, notices } = await setup([message(42, [rx("heart", 4)])], [answer]);

        const toggling = reactions.toggle(channel, 42, glyph("heart"));
        const drawn = JSON.stringify(chipsOf(timelines));
        reactions.stop();
        answer.resolve(refused("rpc.message_deleted", "This message was deleted."));
        await toggling;
        await flush();

        expect(JSON.stringify(chipsOf(timelines))).toBe(drawn);
        expect(notices).not.toHaveBeenCalled();
    });

    test("the catalog's answer is not kept, and the full list answers none", async () => {
        const catalogAnswer = deferred<CallResult>();
        const namesAnswer = deferred<CallResult>();
        const { reactions } = await setup([message(42, [rx("pray", 1)])], [catalogAnswer, namesAnswer]);

        const loading = reactions.loadCatalog();
        const naming = reactions.listReactors(42, "pray", null);
        reactions.stop();
        catalogAnswer.resolve(ok({ emojis: catalog }));
        namesAnswer.resolve(ok({ reactors: [], has_more: false }));

        expect(await loading).toBeNull();
        expect(await naming).toBeNull();
        expect(reactions.state.catalog).toBeNull();
    });

    test("starts no new call and applies no event", async () => {
        const { reactions, timelines, calls } = await setup([message(42, [rx("heart", 4)])]);

        reactions.stop();
        await reactions.toggle(channel, 42, glyph("heart"));
        await reactions.loadCatalog();
        await reactions.listReactors(42, "heart", null);
        reactions.applyEvent(channel, "reaction.added", added("heart", anna));

        expect(calls).toEqual([]);
        expect(chipsOf(timelines)).toEqual([{ code: "heart", count: 4, mine: false }]);
    });
});
