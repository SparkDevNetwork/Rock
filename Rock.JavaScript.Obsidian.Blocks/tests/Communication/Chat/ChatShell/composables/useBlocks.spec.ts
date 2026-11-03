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
import { blockSentence, createBlocks } from "../../../../../src/Communication/Chat/ChatShell/composables/useBlocks.partial";
import { CallResult } from "../../../../../src/Communication/Chat/ChatShell/platformCall.partial";
import { ChatPerson, MessageCreatedEvent } from "../../../../../src/Communication/Chat/ChatShell/types.partial";

const sam = "a0000005-0000-4000-8000-000000000000";
const samOther = "a0000015-0000-4000-8000-000000000000";
const ada = "a0000001-0000-4000-8000-000000000000";
const channel = "c0000001-0000-4000-8000-000000000000";

const samPerson: ChatPerson = { person_alias_guid: sam, nick_name: "Sam", last_name: "Ortiz", avatar_url: "https://example.org/sam.jpg" };

type Deferred<T> = { promise: Promise<T>, resolve: (value: T) => void };

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

function build(answers: CallResult[] = []): { blocks: ReturnType<typeof createBlocks>, calls: Array<{ name: string, args: Record<string, unknown> }>, changes: { count: number }, notices: string[] } {
    const calls: Array<{ name: string, args: Record<string, unknown> }> = [];
    const changes = { count: 0 };
    const notices: string[] = [];
    const blocks = createBlocks({
        call: async (name, args) => {
            calls.push({ name, args });
            return answers.shift() ?? { ok: true, data: { people: [], blocked_aliases: [] } };
        },
        onChanged: () => changes.count++,
        notify: notice => notices.push(notice.text)
    });
    return { blocks, calls, changes, notices };
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

    test("Unblock from the list asks nothing first, and a refusal is noted on the row with Try again", async () => {
        const { blocks, calls } = build([refusal, { ok: true, data: { people: [], blocked_aliases: [] } }]);
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
        }, onChanged: () => undefined, notify: () => undefined });

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
        const blocks = createBlocks({ call: () => answers.shift() as Promise<CallResult>, onChanged: () => undefined, notify: () => undefined });
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

    test("a list that fails to load says so until it loads", async () => {
        const { blocks } = build([refusal]);

        await blocks.load();

        expect(blocks.state.listState).toBe("failed");
    });
});
