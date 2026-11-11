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
// The people a person blocked. A room's live events are one payload for every subscriber, so a
// message by someone blocked arrives collapsed and their face is blank where they reacted or
// replied; the set comes with the sidebar and with every block, unblock and list answer, and a
// change asks the shell to read again what is on screen. Blocking asks one sentence first;
// unblocking asks nothing, and a refusal is told where it was asked.
/* eslint-disable @typescript-eslint/naming-convention */
import { blockSentence, BlocksChange, createBlocks } from "../../../../../src/Communication/Chat/ChatShell/composables/useBlocks.partial";
import { CallResult } from "../../../../../src/Communication/Chat/ChatShell/platformCall.partial";
import { ChatPerson, MessageCreatedEvent, TimelineMessage } from "../../../../../src/Communication/Chat/ChatShell/types.partial";

const sam = "a0000005-0000-4000-8000-000000000000";
const samOther = "a0000015-0000-4000-8000-000000000000";
const ada = "a0000001-0000-4000-8000-000000000000";
const channel = "c0000001-0000-4000-8000-000000000000";

const samPerson: ChatPerson = { person_alias_guid: sam, nick_name: "Sam", last_name: "Ortiz", avatar_url: "https://example.org/sam.jpg" };

type Deferred<T> = { promise: Promise<T>, resolve: (value: T) => void };

async function settle(): Promise<void> {
    for (let i = 0; i < 10; i++) {
        await Promise.resolve();
    }
}

function deferred<T>(): Deferred<T> {
    let resolve!: (value: T) => void;
    const promise = new Promise<T>(r => resolve = r);
    return { promise, resolve };
}

/** A live message.created as a room topic carries it, from Sam unless told otherwise. */
function created(extra: Partial<MessageCreatedEvent> = {}): MessageCreatedEvent {
    return {
        id: 7,
        channel_id: channel,
        parent_id: null,
        shown_in_channel: false,
        person_alias_guid: sam,
        message_type: "text",
        body: "hello",
        pieces: [{ type: "text", text: "hello" }],
        blocks: null,
        attachments: [{ id: "f1", key: "k", kind: "file", name: "a.pdf", size: 10, type: "application/pdf" }],
        actions: [{ id: "x", label: "X", call: "chat_action_x" }],
        sender_nick_name: "Sam",
        sender_last_name: "Ortiz",
        sender_avatar_url: "https://example.org/sam.jpg",
        sender_listed: true,
        created_at: "2026-11-03T10:00:00Z",
        client_key: null,
        ...extra
    };
}

function build(answers: CallResult[] = []): {
    blocks: ReturnType<typeof createBlocks>,
    calls: Array<{ name: string, args: Record<string, unknown> }>,
    changes: { count: number },
    notices: string[],
    order: string[],
    removed: string[][],
    remasks: Array<{ added: string[], asOf: number | null }>,
    changeArgs: Array<{ removed: string[], change: BlocksChange }>
} {
    const calls: Array<{ name: string, args: Record<string, unknown> }> = [];
    const changes = { count: 0 };
    const notices: string[] = [];
    // what the blocks asked of the shell, in order: what is held masked again, or read again
    const order: string[] = [];
    const removed: string[][] = [];
    // what each remask and each read again was told of the change: its decision time, or that it is the heal
    const remasks: Array<{ added: string[], asOf: number | null }> = [];
    const changeArgs: Array<{ removed: string[], change: BlocksChange }> = [];
    const blocks = createBlocks({
        call: async (name, args) => {
            calls.push({ name, args });
            return answers.shift() ?? { ok: true, data: { people: [], blocked_aliases: [] } };
        },
        onChanged: (_isInSidebarAnswer, aliases, change) => {
            changes.count++;
            order.push("onChanged");
            removed.push([...(aliases ?? [])]);
            changeArgs.push({ removed: [...aliases], change });
        },
        remask: (added, asOf) => {
            order.push("remask");
            remasks.push({ added: [...added], asOf });
        },
        notify: notice => notices.push(notice.text)
    });
    return { blocks, calls, changes, notices, order, removed, remasks, changeArgs };
}

const refusal: CallResult = { ok: false, status: 400, error: { code: "rpc.block_limit", severity: "failed", text: "You've blocked as many people as chat allows. Unblock someone to block another." }, retryAfterSeconds: null };

describe("the blocked set", () => {
    test("the first set is what the page is read through, so it asks nothing to be read again", () => {
        const { blocks, changes } = build();

        blocks.setAliases([sam.toUpperCase()]);

        expect(changes.count).toBe(0);
        expect(blocks.isBlocked(sam)).toBe(true);
        expect(blocks.isBlocked(ada)).toBe(false);
    });

    test("a later set that differs asks the shell to read again what is on screen, and the same set does not", () => {
        const { blocks, changes } = build();
        blocks.setAliases([]);

        blocks.setAliases([sam, samOther]);
        blocks.setAliases([samOther, sam]);
        blocks.setAliases([]);

        expect(changes.count).toBe(2);
    });

    test("a live message let through before the first set is known is masked in place once that set names someone, with nothing read again", () => {
        const { blocks, changes, order } = build();

        // the room's live copy arrives before the sidebar has said who is blocked
        expect(blocks.maskEvent("message.created", created())).toMatchObject({ body: "hello" });
        blocks.setAliases([sam]);

        expect(order).toEqual(["remask"]);
        expect(changes.count).toBe(0);
    });

    test("a first set naming nobody, or one taken before any live event, asks nothing read again even after events", () => {
        const empty = build();
        empty.blocks.maskEvent("reaction.added", { reactors: [] });
        empty.blocks.setAliases([]);

        const early = build();
        early.blocks.setAliases([sam]);
        early.blocks.maskEvent("message.created", created());

        expect(empty.changes.count).toBe(0);
        expect(empty.order).toEqual([]);
        expect(early.changes.count).toBe(0);
        expect(early.order).toEqual(["remask"]);
    });

    test("a later addition masks what is held at once, then asks the rest read again; a removal alone only asks it read again, naming who", () => {
        const { blocks, order, removed } = build();
        blocks.setAliases([]);

        blocks.setAliases([sam, samOther]);
        expect(order).toEqual(["remask", "onChanged"]);

        blocks.setAliases([sam]);
        expect(order).toEqual(["remask", "onChanged", "onChanged"]);
        expect(removed.at(-1)).toEqual([samOther]);
    });

    test("an answer with no set, as an older platform's sidebar, leaves the set as it was", () => {
        const { blocks } = build();
        blocks.setAliases([sam]);

        blocks.setAliases(undefined);

        expect(blocks.isBlocked(sam)).toBe(true);
    });
});

describe("live events", () => {
    test("a message by someone blocked arrives collapsed: its row, id, time and key kept, everything it says and who sent it withheld", () => {
        const { blocks } = build();
        blocks.setAliases([sam]);

        const shown = blocks.maskEvent("message.created", created()) as Record<string, unknown>;

        expect(shown).toMatchObject({ id: 7, channel_id: channel, person_alias_guid: sam, created_at: "2026-11-03T10:00:00Z", blocked: true });
        for (const field of ["body", "pieces", "blocks", "attachments", "metadata", "actions", "quoted", "sender_nick_name", "sender_last_name", "sender_avatar_url"]) {
            expect(shown[field]).toBeNull();
        }
    });

    test("a message from any alias of someone blocked collapses, in any case", () => {
        const { blocks } = build();
        blocks.setAliases([sam, samOther]);

        const shown = blocks.maskEvent("message.created", created({ person_alias_guid: samOther.toUpperCase() })) as Record<string, unknown>;

        expect(shown.blocked).toBe(true);
    });

    test("a system line is nobody's message and is never collapsed", () => {
        const { blocks } = build();
        blocks.setAliases([sam]);

        const line = created({ message_type: "system", body: "Sam joined" });

        expect(blocks.maskEvent("message.created", line)).toBe(line);
    });

    test("someone else's message quoting a blocked person keeps its own words and withholds the quote's", () => {
        const { blocks } = build();
        blocks.setAliases([sam]);

        const shown = blocks.maskEvent("message.created", created({
            person_alias_guid: ada,
            sender_nick_name: "Ada",
            quoted_message_id: 3,
            quoted: { id: 3, person_alias_guid: sam, sender_nick_name: "Sam", sender_last_name: "Ortiz", sender_avatar_url: "x", body: "secret", pieces: null, attachments: null, created_at: "2026-11-03T09:00:00Z" }
        })) as MessageCreatedEvent;

        expect(shown.body).toBe("hello");
        expect(shown.sender_nick_name).toBe("Ada");
        expect(shown.quoted).toMatchObject({ id: 3, body: null, sender_nick_name: null, sender_last_name: null, sender_avatar_url: null, blocked: true });
    });

    test("a reaction's chips blank the face of someone blocked and keep everyone else's, the count whole", () => {
        const { blocks } = build();
        blocks.setAliases([sam]);
        const payload = {
            message_id: 7, channel_id: channel, person_alias_guid: sam, code: "heart", count: 2,
            reactors: [
                { person_alias_guid: sam, reactor_person_alias_guid: sam, nick_name: "Sam", last_name: "Ortiz", avatar_url: "x", reacted_at: "t" },
                { person_alias_guid: ada, reactor_person_alias_guid: ada, nick_name: "Ada", last_name: "Lane", avatar_url: "y", reacted_at: "t" }
            ]
        };

        const shown = blocks.maskEvent("reaction.added", payload) as typeof payload;

        expect(shown.count).toBe(2);
        expect(shown.reactors[0]).toMatchObject({ person_alias_guid: sam, nick_name: null, last_name: null, avatar_url: null });
        expect(shown.reactors[1]).toEqual(payload.reactors[1]);
    });

    test("a thread's repliers blank the face of someone blocked, the counts whole", () => {
        const { blocks } = build();
        blocks.setAliases([sam]);
        const payload = {
            channel_id: channel, root_id: 7, reply_count: 3, last_reply_at: null, participant_count: 2,
            repliers: [{ person_alias_guid: sam, nick_name: "Sam", last_name: "Ortiz", avatar_url: "x" }, { person_alias_guid: ada, nick_name: "Ada", last_name: "Lane", avatar_url: null }]
        };

        const shown = blocks.maskEvent("thread.updated", payload) as typeof payload;

        expect(shown.reply_count).toBe(3);
        expect(shown.participant_count).toBe(2);
        expect(shown.repliers[0]).toEqual({ person_alias_guid: sam, nick_name: null, last_name: null, avatar_url: null });
        expect(shown.repliers[1]).toEqual(payload.repliers[1]);
    });

    test("with nobody blocked, every event is the same payload, untouched", () => {
        const { blocks } = build();
        blocks.setAliases([]);
        const message = created();

        expect(blocks.maskEvent("message.created", message)).toBe(message);
    });
});

describe("a message as the person may see it", () => {
    const samFace = { person_alias_guid: sam, nick_name: "Sam", last_name: "Ortiz", avatar_url: "https://example.org/sam.jpg" };
    const adaFace = { person_alias_guid: ada, nick_name: "Ada", last_name: "Lane", avatar_url: null };
    const mention = (guid: string, name: string): Record<string, unknown> => ({ type: "mention", text: `@${name}`, person_alias_guid: guid, nick_name: name, last_name: "X", avatar_url: "https://example.org/face.jpg" });

    /** Ada's held message: quoting Sam, mentioning Sam, replied to and reacted to by Sam. */
    function adaMessage(): TimelineMessage {
        return {
            id: 8, person_alias_guid: ada, sender_nick_name: "Ada", sender_last_name: "Lane", message_type: "text", body: "hi", created_at: "2026-11-03T10:00:00Z",
            pieces: [{ type: "text", text: "hi " }, mention(sam, "Sam"), mention(ada, "Ada")],
            quoted: { id: 3, person_alias_guid: sam, sender_nick_name: "Sam", sender_last_name: "Ortiz", sender_avatar_url: "x", body: "secret", pieces: null, attachments: null, created_at: "2026-11-03T09:00:00Z" },
            repliers: [samFace, adaFace],
            reactions: [{ code: "heart", count: 2, mine: false, emoji: null, image_url: null, label: "heart", reactors: [{ ...samFace, reactor_person_alias_guid: samOther, reacted_at: "t" }, { ...adaFace, reactor_person_alias_guid: ada, reacted_at: "t" }] }]
        } as TimelineMessage;
    }

    test("a message by someone in the set is withheld, its id, time and counts kept", () => {
        const { blocks } = build();
        blocks.setAliases([sam]);

        const shown = blocks.mask({ ...adaMessage(), id: 9, person_alias_guid: sam, sender_nick_name: "Sam", body: "secret", reply_count: 2 }) as unknown as Record<string, unknown>;

        expect(shown).toMatchObject({ id: 9, reply_count: 2, blocked: true });
        for (const field of ["body", "pieces", "blocks", "attachments", "metadata", "actions", "quoted", "sender_nick_name", "sender_last_name", "sender_avatar_url"]) {
            expect(shown[field]).toBeNull();
        }
    });

    test("someone else's message keeps its words and loses the face of someone in the set: the quote, the mention chip, the repliers and the chips' reactors", () => {
        const { blocks } = build();
        blocks.setAliases([sam, samOther]);

        const shown = blocks.mask(adaMessage());

        expect(shown.body).toBe("hi");
        expect(shown.quoted).toMatchObject({ id: 3, body: null, pieces: null, sender_nick_name: null, sender_last_name: null, sender_avatar_url: null, blocked: true });
        expect(shown.pieces?.[1]).toEqual({ type: "mention", text: "@Sam", person_alias_guid: sam, nick_name: null, last_name: null, avatar_url: null });
        expect(shown.pieces?.[2]).toEqual(adaMessage().pieces?.[2]);
        expect(shown.repliers).toEqual([{ ...samFace, nick_name: null, last_name: null, avatar_url: null }, adaFace]);
        expect(shown.reactions?.[0].reactors?.[0]).toMatchObject({ reactor_person_alias_guid: samOther, nick_name: null, last_name: null, avatar_url: null });
        expect(shown.reactions?.[0].reactors?.[1]).toMatchObject(adaFace);
        expect(shown.reactions?.[0].count).toBe(2);
    });

    test("a quote's own mention chip of someone in the set loses its face too", () => {
        const { blocks } = build();
        blocks.setAliases([sam]);
        const message = { ...adaMessage(), quoted: { id: 3, person_alias_guid: ada, sender_nick_name: "Ada", body: "hey Sam", pieces: [mention(sam, "Sam")], attachments: null, created_at: "t" } } as TimelineMessage;

        const shown = blocks.mask(message);

        expect(shown.quoted).toMatchObject({ body: "hey Sam", sender_nick_name: "Ada" });
        expect(shown.quoted?.pieces?.[0]).toMatchObject({ text: "@Sam", nick_name: null, avatar_url: null });
    });

    test("a live message from someone else mentioning a person in the set draws no face in the chip", () => {
        const { blocks } = build();
        blocks.setAliases([sam]);

        const shown = blocks.maskEvent("message.created", created({ person_alias_guid: ada, pieces: [mention(sam, "Sam")] as never })) as MessageCreatedEvent;

        expect(shown.pieces?.[0]).toMatchObject({ text: "@Sam", nick_name: null, last_name: null, avatar_url: null });
    });

    test("a copy the platform withheld is kept withheld, every field it withholds emptied, even for an alias the set does not name", () => {
        const { blocks } = build();
        blocks.setAliases([sam]);

        const shown = blocks.mask({ ...adaMessage(), id: 9, person_alias_guid: samOther, body: "laid over", blocked: true });

        expect(shown).toMatchObject({ blocked: true, body: null, pieces: null, quoted: null, sender_nick_name: null });
    });

    test("the same object comes back when nothing applies, when it is already withheld, and when its faces are already blank, so nothing is written again", () => {
        const { blocks } = build();
        blocks.setAliases([sam]);
        const plain = { ...adaMessage(), quoted: null, pieces: null, repliers: [adaFace], reactions: [] } as TimelineMessage;
        const withheld = blocks.mask({ ...adaMessage(), person_alias_guid: sam });
        const blanked = blocks.mask(adaMessage());

        expect(blocks.mask(plain)).toBe(plain);
        expect(blocks.mask(withheld)).toBe(withheld);
        expect(blocks.mask(blanked)).toBe(blanked);
        expect(blocks.mask(blocks.mask(adaMessage()))).toEqual(blocks.mask(adaMessage()));
    });

    test("a system line is never withheld", () => {
        const { blocks } = build();
        blocks.setAliases([sam]);
        const line = { ...adaMessage(), person_alias_guid: sam, message_type: "system", quoted: null, pieces: null, repliers: null, reactions: [] } as TimelineMessage;

        expect(blocks.mask(line)).toBe(line);
    });
});

describe("a copy withheld for someone the set does not name", () => {
    const withheldBy = (guid: string): TimelineMessage => ({ id: 9, person_alias_guid: guid, message_type: "text", body: null, created_at: "t", blocked: true } as TimelineMessage);

    test("reads the list first, then asks once what is held read again for that person, after the write", async () => {
        const { blocks, calls, order, removed } = build([{ ok: true, data: { people: [], blocked_aliases: [] } }]);
        blocks.setAliases([]);

        blocks.mask(withheldBy(sam.toUpperCase()));
        expect(calls).toEqual([]);
        await settle();

        expect(calls.map(c => c.name)).toEqual(["chat_list_blocks"]);
        expect(order).toEqual(["onChanged"]);
        expect(removed).toEqual([[sam]]);
    });

    test("asks nothing more for a second such copy until the set changes", async () => {
        const { blocks, calls, changes } = build();
        blocks.setAliases([]);
        blocks.mask(withheldBy(sam));
        await settle();

        blocks.mask(withheldBy(sam));
        blocks.mask({ ...withheldBy(ada), id: 10 });
        await settle();
        expect(calls.length).toBe(1);
        expect(changes.count).toBe(1);

        blocks.setAliases([ada]);
        blocks.setAliases([]);
        await settle();
        blocks.mask(withheldBy(sam));
        await settle();
        expect(calls.length).toBe(2);
    });

    test("a quote withheld for someone the set does not name heals as a row does", async () => {
        const { blocks, removed } = build();
        blocks.setAliases([]);

        blocks.mask({ id: 8, person_alias_guid: ada, message_type: "text", body: "hi", created_at: "t", quoted: { id: 3, person_alias_guid: sam, body: null, created_at: "t", blocked: true } } as TimelineMessage);
        await settle();

        expect(removed).toEqual([[sam]]);
    });

    test("when the list read names someone new, that change is the heal, and nothing more is asked", async () => {
        const { blocks, order } = build([{ ok: true, data: { people: [], blocked_aliases: [sam] } }]);
        blocks.setAliases([]);

        blocks.mask(withheldBy(sam));
        await settle();

        expect(order).toEqual(["remask", "onChanged"]);
        expect(blocks.isBlocked(sam)).toBe(true);
    });

    test("asks nothing for a copy of someone the set names, nor once stopped", async () => {
        const named = build();
        named.blocks.setAliases([sam]);
        named.blocks.mask(withheldBy(sam));
        const stopped = build();
        stopped.blocks.setAliases([]);
        stopped.blocks.mask(withheldBy(sam));
        stopped.blocks.stop();
        await settle();

        expect(named.calls).toEqual([]);
        expect(stopped.calls).toEqual([]);
        expect(stopped.changes.count).toBe(0);
    });

    test("one seen before the first set heals once that set lands not naming its person, and not when it does", async () => {
        // the list read the heal asks names the same set, so the copy is out of date
        const unnamed = build([{ ok: true, data: { people: [], blocked_aliases: [ada] } }]);
        unnamed.blocks.mask(withheldBy(sam));
        await settle();
        expect(unnamed.calls).toEqual([]);
        unnamed.blocks.setAliases([ada]);
        await settle();

        const named = build();
        named.blocks.mask(withheldBy(sam));
        named.blocks.setAliases([sam]);
        await settle();

        expect(unnamed.calls.map(c => c.name)).toEqual(["chat_list_blocks"]);
        expect(unnamed.removed).toEqual([[sam]]);
        expect(named.calls).toEqual([]);
    });
});

describe("when a change of the set was decided", () => {
    test("a block's and an unblock's answer pass their decision time to what masks and what is read again; the sidebar's set, which carries none, passes none", async () => {
        const { blocks, remasks, changeArgs } = build([
            { ok: true, data: { people: [samPerson], blocked_aliases: [sam], as_of: 1_700_000_000_000_100 } },
            { ok: true, data: { people: [], blocked_aliases: [], as_of: 1_700_000_000_000_200 } }
        ]);
        blocks.setAliases([ada]);
        blocks.ask(samPerson);
        await blocks.confirm();
        await blocks.unblock(samPerson, true);
        blocks.setAliases([]);

        expect(remasks).toEqual([{ added: [ada], asOf: null }, { added: [sam], asOf: 1_700_000_000_000_100 }]);
        expect(changeArgs).toEqual([
            { removed: [ada], change: { asOf: 1_700_000_000_000_100 } },
            { removed: [sam], change: { asOf: 1_700_000_000_000_200 } }
        ]);
    });

    test("a list read whose decision time is not a whole number passes none", async () => {
        const { blocks, remasks } = build([{ ok: true, data: { people: [], blocked_aliases: [sam], as_of: "1700000000000100" } }]);
        blocks.setAliases([]);

        await blocks.changed();

        expect(remasks).toEqual([{ added: [sam], asOf: null }]);
    });

    test("the heal says it is one, since it changes no block", async () => {
        const { blocks, changeArgs } = build([{ ok: true, data: { people: [], blocked_aliases: [], as_of: 1_700_000_000_000_100 } }]);
        blocks.setAliases([]);

        blocks.mask({ id: 9, person_alias_guid: sam, message_type: "text", body: null, created_at: "t", blocked: true } as TimelineMessage);
        await settle();

        expect(changeArgs).toEqual([{ removed: [sam], change: { heal: true } }]);
    });
});

describe("block and unblock", () => {
    test("Block asks the one sentence first and calls nothing until it is confirmed", async () => {
        const { blocks, calls } = build();

        blocks.ask(samPerson);

        expect(blocks.state.confirming).toEqual(samPerson);
        expect(calls).toEqual([]);
        expect(blockSentence).toBe("They won't be told. You won't see their messages, and they can't message you.");

        blocks.cancel();
        expect(blocks.state.confirming).toBeNull();
        expect(calls).toEqual([]);
    });

    test("a confirmed block calls the platform for that person, takes the answer's list and set, and says so", async () => {
        const { blocks, calls, changes, notices } = build([{ ok: true, data: { people: [samPerson], blocked_aliases: [sam, samOther] } }]);
        blocks.setAliases([]);
        blocks.ask(samPerson);

        const isTaken = await blocks.confirm();

        expect(isTaken).toBe(true);
        expect(calls).toEqual([{ name: "chat_block_person", args: { p_person_alias_guid: sam } }]);
        expect(blocks.state.people).toEqual([samPerson]);
        expect(blocks.isBlocked(samOther)).toBe(true);
        expect(changes.count).toBe(1);
        expect(notices).toEqual(["Blocked Sam Ortiz"]);
    });

    test("a refused block says the server's sentence and changes nothing", async () => {
        const { blocks, changes, notices } = build([refusal]);
        blocks.setAliases([]);
        blocks.ask(samPerson);

        expect(await blocks.confirm()).toBe(false);

        expect(notices).toEqual(["You've blocked as many people as chat allows. Unblock someone to block another."]);
        expect(changes.count).toBe(0);
        expect(blocks.isBlocked(sam)).toBe(false);
    });

    test("Unblock from the list asks nothing first, and a refusal another try can pass is noted on the row with Try again", async () => {
        const unavailable: CallResult = { ok: false, status: 503, error: { code: "rpc.unavailable", severity: "failed" }, retryAfterSeconds: null };
        const { blocks, calls } = build([unavailable, { ok: true, data: { people: [], blocked_aliases: [] } }]);
        blocks.setAliases([sam]);

        expect(await blocks.unblock(samPerson, true)).toBe(false);

        expect(calls).toEqual([{ name: "chat_unblock_person", args: { p_person_alias_guid: sam } }]);
        const note = blocks.state.notes[sam];
        expect(note.kind).toBe("error");
        expect(note.retry).toBeDefined();

        note.retry?.();
        await Promise.resolve();
        await Promise.resolve();
        expect(calls.length).toBe(2);
    });

    test("a block or unblock already out for a person is not asked again", async () => {
        const answer = deferred<CallResult>();
        const calls: string[] = [];
        const blocks = createBlocks({ call: (name) => {
            calls.push(name); return answer.promise;
        }, onChanged: () => undefined, remask: () => undefined, notify: () => undefined });

        const first = blocks.unblock(samPerson, true);
        expect(blocks.state.busy[sam]).toBe(true);
        expect(await blocks.unblock(samPerson, true)).toBe(false);

        answer.resolve({ ok: true, data: { people: [], blocked_aliases: [] } });
        await first;
        expect(calls).toEqual(["chat_unblock_person"]);
        expect(blocks.state.busy[sam]).toBeUndefined();
    });

    test("an older answer that lands after a newer one never takes it back", async () => {
        const list = deferred<CallResult>();
        const unblocked = deferred<CallResult>();
        const answers = [list.promise, unblocked.promise];
        const blocks = createBlocks({ call: () => answers.shift() as Promise<CallResult>, onChanged: () => undefined, remask: () => undefined, notify: () => undefined });
        blocks.setAliases([sam]);

        const loading = blocks.load();
        const unblocking = blocks.unblock(samPerson, true);
        unblocked.resolve({ ok: true, data: { people: [], blocked_aliases: [] } });
        await unblocking;
        list.resolve({ ok: true, data: { people: [samPerson], blocked_aliases: [sam] } });
        await loading;

        expect(blocks.isBlocked(sam)).toBe(false);
        expect(blocks.state.people).toEqual([]);
    });

    test("blocks.changed reads the list again, and its set is taken like any other", async () => {
        const { blocks, calls, changes } = build([{ ok: true, data: { people: [samPerson], blocked_aliases: [sam] } }]);
        blocks.setAliases([]);

        await blocks.changed();

        expect(calls.map(c => c.name)).toEqual(["chat_list_blocks"]);
        expect(blocks.isBlocked(sam)).toBe(true);
        expect(changes.count).toBe(1);
    });

    test("once stopped, a list, block or unblock answering late changes nothing and tells nothing", async () => {
        const list = deferred<CallResult>();
        const blocked = deferred<CallResult>();
        const unblocked = deferred<CallResult>();
        const answers = [list.promise, blocked.promise, unblocked.promise];
        const changes = { count: 0 };
        const notices: string[] = [];
        const blocks = createBlocks({ call: () => answers.shift() as Promise<CallResult>, onChanged: () => changes.count++, remask: () => undefined, notify: notice => notices.push(notice.text) });
        blocks.setAliases([]);

        const loading = blocks.load();
        blocks.ask(samPerson);
        const blocking = blocks.confirm();
        const unblocking = blocks.unblock({ ...samPerson, person_alias_guid: samOther }, false);
        blocks.stop();
        list.resolve({ ok: true, data: { people: [samPerson], blocked_aliases: [sam] } });
        blocked.resolve({ ok: true, data: { people: [samPerson], blocked_aliases: [sam] } });
        unblocked.resolve(refusal);

        expect(await blocking).toBe(false);
        expect(await unblocking).toBe(false);
        await loading;
        expect(blocks.isBlocked(sam)).toBe(false);
        expect(blocks.state.people).toEqual([]);
        expect(blocks.state.listState).not.toBe("loaded");
        expect(changes.count).toBe(0);
        expect(notices).toEqual([]);
        expect(blocks.state.notes).toEqual({});
    });

    test("a list that fails to load says so until it loads", async () => {
        const { blocks } = build([refusal]);

        await blocks.load();

        expect(blocks.state.listState).toBe("failed");
    });
});
