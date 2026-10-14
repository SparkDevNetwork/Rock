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
// Sending: a pending row at once, a timeline message once confirmed, and a failed send that
// keeps its text so it can be sent again.
import { createSender, isComposerOpen, JoinResult, newLocalId, SendResult, textAfterSend } from "../../../../../src/Communication/Chat/ChatShell/composables/useSend.partial";
import { createTimelines } from "../../../../../src/Communication/Chat/ChatShell/composables/useHistory.partial";
import { TimelineMessage } from "../../../../../src/Communication/Chat/ChatShell/types.partial";

const channel = "c0000001-0000-4000-8000-000000000000";
const me = "a0000001-0000-4000-8000-000000000000";

type Deferred<T> = { promise: Promise<T>, resolve: (value: T) => void };

function deferred<T>(): Deferred<T> {
    let resolve!: (value: T) => void;
    const promise = new Promise<T>(r => resolve = r);
    return { promise, resolve };
}

async function setup(answers: Array<SendResult | Deferred<SendResult>>): Promise<{
    sender: ReturnType<typeof createSender>,
    timelines: ReturnType<typeof createTimelines>,
    sent: string[]
}> {
    const sent: string[] = [];
    let next = 0;
    const timelines = createTimelines({ fetchPage: async () => ({ messages: [], read_cursor: null, unread_count: 0, has_more: false }) });
    await timelines.loadNewest(channel);

    const sender = createSender({
        send: (_c, body) => {
            sent.push(body);
            const answer = answers.shift() as SendResult | Deferred<SendResult>;
            return "promise" in answer ? answer.promise : Promise.resolve(answer);
        },
        timelines,
        personAliasGuid: me,
        newLocalId: () => `local-${++next}`
    });

    return { sender, timelines, sent };
}

describe("createSender", () => {
    test("a send shows a pending row until the platform confirms it", async () => {
        const answer = deferred<SendResult>();
        const { sender, timelines } = await setup([answer]);

        const sending = sender.send(channel, "hello");

        expect(sender.pending(channel)).toEqual([
            { localId: "local-1", channelId: channel, body: "hello", status: "sending", errorCode: null }
        ]);
        expect(timelines.state(channel).messages).toEqual([]);

        answer.resolve({ ok: true, id: 41, createdAt: "2026-09-23T10:00:00Z" });
        expect(await sending).toBe(true);

        expect(sender.pending(channel)).toEqual([]);
        const [confirmed] = timelines.state(channel).messages;
        expect(confirmed.id).toBe(41);
        expect(confirmed.body).toBe("hello");
        expect(confirmed.person_alias_guid).toBe(me);
    });

    test("a failed send keeps its text on a failed row with the reason", async () => {
        const { sender, timelines } = await setup([{ ok: false, error: { code: "rpc.transport", severity: "failed" } }]);

        expect(await sender.send(channel, "keep me")).toBe(false);

        expect(sender.pending(channel)).toEqual([
            { localId: "local-1", channelId: channel, body: "keep me", status: "failed", errorCode: "rpc.transport" }
        ]);
        expect(timelines.state(channel).messages).toEqual([]);
    });

    test("a send that rejects instead of answering leaves its row failed with its text, so it can be retried or discarded", async () => {
        const timelines = createTimelines({ fetchPage: async () => ({ messages: [], read_cursor: null, unread_count: 0, has_more: false }) });
        await timelines.loadNewest(channel);
        const sender = createSender({
            send: () => Promise.reject(new Error("network")),
            timelines,
            personAliasGuid: me,
            newLocalId: () => "local-1"
        });

        expect(await sender.send(channel, "keep me")).toBe(false);

        expect(sender.pending(channel)).toEqual([
            { localId: "local-1", channelId: channel, body: "keep me", status: "failed", errorCode: "rpc.transport" }
        ]);
        sender.discard("local-1");
        expect(sender.pending(channel)).toEqual([]);
    });

    test("retry sends the same text again and replaces the row with the confirmed message", async () => {
        const { sender, timelines, sent } = await setup([
            { ok: false, error: { code: "rpc.transport", severity: "failed" } },
            { ok: true, id: 42, createdAt: "2026-09-23T10:00:00Z" }
        ]);
        await sender.send(channel, "again");

        expect(await sender.retry("local-1")).toBe(true);

        expect(sent).toEqual(["again", "again"]);
        expect(sender.pending(channel)).toEqual([]);
        expect(timelines.state(channel).messages.map(m => m.id)).toEqual([42]);
    });

    test("a retry shows as sending while it is in flight", async () => {
        const second = deferred<SendResult>();
        const { sender } = await setup([{ ok: false, error: { code: "rpc.transport", severity: "failed" } }, second]);
        await sender.send(channel, "again");

        const retrying = sender.retry("local-1");

        expect(sender.pending(channel)[0].status).toBe("sending");
        second.resolve({ ok: true, id: 43, createdAt: "2026-09-23T10:00:00Z" });
        await retrying;
    });

    test("the live echo of the person's own message leaves one row, whichever arrives first", async () => {
        const answer = deferred<SendResult>();
        const { sender, timelines } = await setup([answer]);

        const sending = sender.send(channel, "echo");
        timelines.applyEvent(channel, "message.created", {
            id: 44, channel_id: channel, parent_id: null, shown_in_channel: false,
            person_alias_guid: me, message_type: "text", body: "echo", created_at: "2026-09-23T10:00:00Z"
        });
        answer.resolve({ ok: true, id: 44, createdAt: "2026-09-23T10:00:00Z" });
        await sending;
        timelines.applyEvent(channel, "message.created", {
            id: 44, channel_id: channel, parent_id: null, shown_in_channel: false,
            person_alias_guid: me, message_type: "text", body: "echo", created_at: "2026-09-23T10:00:00Z"
        });

        expect(timelines.state(channel).messages.map(m => m.id)).toEqual([44]);
        expect(sender.pending(channel)).toEqual([]);
    });

    test("blank text sends nothing", async () => {
        const { sender, sent } = await setup([]);

        expect(await sender.send(channel, "   ")).toBe(false);

        expect(sent).toEqual([]);
        expect(sender.pending(channel)).toEqual([]);
    });

    test("discard drops a failed row", async () => {
        const { sender } = await setup([{ ok: false, error: { code: "rpc.transport", severity: "failed" } }]);
        await sender.send(channel, "gone");

        sender.discard("local-1");

        expect(sender.pending(channel)).toEqual([]);
    });

    test("pending rows are per channel", async () => {
        const { sender } = await setup([{ ok: false, error: { code: "rpc.transport", severity: "failed" } }]);
        await sender.send(channel, "here");

        expect(sender.pending("c0000002-0000-4000-8000-000000000000")).toEqual([]);
    });
});

describe("a send the platform can recognise again", () => {
    // A confirmation can be lost after the platform committed the message, so a retry carries
    // the same key as the first try, and the platform answers the first message instead of
    // posting a second.
    test("a send carries its row's key, and a retry carries the same key", async () => {
        const keys: Array<string | undefined> = [];
        const answers: SendResult[] = [
            { ok: false, error: { code: "rpc.transport", severity: "failed" } },
            { ok: true, id: 45, createdAt: "2026-10-05T10:00:00Z" }
        ];
        const timelines = createTimelines({ fetchPage: async () => ({ messages: [], read_cursor: null, unread_count: 0, has_more: false }) });
        await timelines.loadNewest(channel);
        const sender = createSender({
            send: (_c: string, _body: string, key?: string) => {
                keys.push(key);
                return Promise.resolve(answers.shift() as SendResult);
            },
            timelines,
            personAliasGuid: me,
            newLocalId: () => "3f0c7a52-8a4e-4c5e-9d1a-2b7f6e0c1d22"
        });

        await sender.send(channel, "once");
        await sender.retry("3f0c7a52-8a4e-4c5e-9d1a-2b7f6e0c1d22");

        expect(keys).toEqual(["3f0c7a52-8a4e-4c5e-9d1a-2b7f6e0c1d22", "3f0c7a52-8a4e-4c5e-9d1a-2b7f6e0c1d22"]);
    });

    // The first try may have been stored with its answer lost, so a key stands for one text: a
    // later try sending other words would either be refused as a different message under the same
    // key or post them beside the first. Changed words are a new message under a new key.
    test("a later try of a row sends the row's own text under its key, whatever text it is given", async () => {
        const { sender, sent } = await setup([
            { ok: false, error: { code: "rpc.transport", severity: "failed" } },
            { ok: true, id: 48, createdAt: "2026-10-06T10:00:00Z" }
        ]);

        await sender.send(channel, "first words", { localId: "local-9", isLast: true });
        expect(await sender.send(channel, "edited words", { localId: "local-9", isLast: true })).toBe(true);

        expect(sent).toEqual(["first words", "first words"]);
        expect(sender.pending(channel)).toEqual([]);
    });

    test("a notice the server sends with the confirmation stays with the person's message", async () => {
        const { sender, timelines } = await setup([{ ok: true, id: 46, createdAt: "2026-10-05T10:00:00Z", notice: "Your message is waiting for review." } as SendResult]);

        await sender.send(channel, "please look");

        const [confirmed] = timelines.state(channel).messages;
        expect((confirmed as Record<string, unknown>).notice).toBe("Your message is waiting for review.");
    });

    test("a confirmation with no notice leaves none", async () => {
        const { sender, timelines } = await setup([{ ok: true, id: 47, createdAt: "2026-10-05T10:00:00Z", notice: null } as SendResult]);

        await sender.send(channel, "plain");

        expect((timelines.state(channel).messages[0] as Record<string, unknown>).notice ?? null).toBeNull();
    });
});

describe("the person's confirmed message draws what the server built", () => {
    // The send answers the pieces the stored row draws, so the person's own message shows its
    // mention as soon as it is confirmed rather than a raw token until its event arrives. The
    // answer here is chat_send_message's, a person's send never carrying blocks.
    const lee = "a0000007-0000-4000-8000-000000000000";
    const pieces = [
        { type: "text", text: "Hi " },
        { type: "mention", text: "@Lee Leader", person_alias_guid: lee },
        { type: "text", text: " there" }
    ];

    test("the confirmed message carries the answer's pieces and blocks", async () => {
        const { sender, timelines } = await setup([
            { ok: true, id: 49, createdAt: "2026-10-12T10:00:00Z", notice: null, pieces, blocks: null } as SendResult
        ]);

        await sender.send(channel, `Hi <@${lee}> there`);

        const [confirmed] = timelines.state(channel).messages;
        expect(confirmed.body).toBe(`Hi <@${lee}> there`);
        expect(confirmed.pieces).toEqual(pieces);
        expect(confirmed).toHaveProperty("blocks", null);
    });

    test("a confirmation arriving after the message was hidden never brings its body or pieces back", async () => {
        // The live copy and the hide can both arrive before the send's own answer.
        const answer = deferred<SendResult>();
        const { sender, timelines } = await setup([answer]);
        const sending = sender.send(channel, `Hi <@${lee}> there`);

        timelines.applyEvent(channel, "message.created", {
            id: 51, channel_id: channel, person_alias_guid: me, message_type: "text", body: `Hi <@${lee}> there`,
            pieces, blocks: null, parent_id: null, shown_in_channel: false, created_at: "2026-10-12T10:00:00Z"
        });
        timelines.applyEvent(channel, "message.visibility", { id: 51, channel_id: channel, state: "hidden", notice: "Hidden while it is reviewed." });
        answer.resolve({ ok: true, id: 51, createdAt: "2026-10-12T10:00:00Z", notice: "Hidden while it is reviewed.", pieces: null, blocks: null } as SendResult);
        await sending;

        const [held] = timelines.state(channel).messages;
        expect(held.visibility).toBe("hidden");
        expect(held.body).toBeNull();
        expect(held.pieces ?? null).toBeNull();
    });

    test("a plain body's confirmation carries no pieces", async () => {
        const { sender, timelines } = await setup([
            { ok: true, id: 50, createdAt: "2026-10-12T10:00:00Z", notice: null, pieces: null, blocks: null } as SendResult
        ]);

        await sender.send(channel, "plain words");

        const [confirmed] = timelines.state(channel).messages;
        expect(confirmed).toHaveProperty("pieces", null);
        expect(confirmed).toHaveProperty("blocks", null);
    });
});

describe("a send into a room the person has not joined", () => {
    // The refusal as the shell's call path hands it over: the platform's code, classified as a
    // permission, with the sentence the database gives it.
    const joinRequired: SendResult = {
        ok: false,
        error: { code: "authz.join_required", severity: "permission", text: "Join this channel to post." }
    };

    type Sent = { body: string, key: string | undefined };

    async function joinSetup(
        answers: Array<SendResult | Deferred<SendResult>>,
        joins: Array<JoinResult | Deferred<JoinResult> | Error>
    ): Promise<{
        sender: ReturnType<typeof createSender>,
        timelines: ReturnType<typeof createTimelines>,
        sent: Sent[],
        joined: string[],
        waits: Array<Deferred<void>>
    }> {
        const sent: Sent[] = [];
        const joined: string[] = [];
        const waits: Array<Deferred<void>> = [];
        let next = 0;
        const timelines = createTimelines({ fetchPage: async () => ({ messages: [], read_cursor: null, unread_count: 0, has_more: false }) });
        await timelines.loadNewest(channel);

        const sender = createSender({
            send: (_c, body, key) => {
                sent.push({ body, key });
                const answer = answers.shift() as SendResult | Deferred<SendResult>;
                return "promise" in answer ? answer.promise : Promise.resolve(answer);
            },
            join: channelId => {
                joined.push(channelId);
                const answer = joins.shift() as JoinResult | Deferred<JoinResult> | Error;
                if (answer instanceof Error) {
                    return Promise.reject(answer);
                }
                return "promise" in answer ? answer.promise : Promise.resolve(answer);
            },
            waitBeforeResend: () => {
                const wait = deferred<void>();
                waits.push(wait);
                return wait.promise;
            },
            timelines,
            personAliasGuid: me,
            newLocalId: () => `local-${++next}`
        });

        return { sender, timelines, sent, joined, waits };
    }

    /** Lets every promise already settled run its continuations. */
    async function settle(): Promise<void> {
        for (let i = 0; i < 10; i++) {
            await Promise.resolve();
        }
    }

    test("joins through Rock's door and sends again under the same key", async () => {
        const { sender, timelines, sent, joined } = await joinSetup(
            [joinRequired, { ok: true, id: 51, createdAt: "2026-10-07T10:00:00Z" }],
            [{ code: "ok", message: null, isPending: false }]);

        expect(await sender.send(channel, "hello room")).toBe(true);

        expect(joined).toEqual([channel]);
        expect(sent.map(s => s.body)).toEqual(["hello room", "hello room"]);
        expect(sent[1].key).toBe(sent[0].key);
        expect(sender.pending(channel)).toEqual([]);
        expect(timelines.state(channel).messages[0].id).toBe(51);
    });

    test("a join the door refuses fails the message with the door's code and sends nothing more", async () => {
        const { sender, sent } = await joinSetup(
            [joinRequired],
            [{ code: "door.not_allowed", message: "You can't join this channel.", isPending: false }]);

        expect(await sender.send(channel, "hello room")).toBe(false);

        expect(sent).toHaveLength(1);
        expect(sender.pending(channel)).toEqual([
            { localId: "local-1", channelId: channel, body: "hello room", status: "failed", errorCode: "door.not_allowed" }
        ]);
    });

    test("a join that does not reach Rock fails the message so it can be tried again", async () => {
        const { sender, sent } = await joinSetup([joinRequired], [new Error("network")]);

        expect(await sender.send(channel, "hello room")).toBe(false);

        expect(sent).toHaveLength(1);
        expect(sender.pending(channel)[0]).toMatchObject({ status: "failed", errorCode: "door.unreachable" });
    });

    test("a join still reaching the platform waits and sends again until the membership is there", async () => {
        const { sender, sent, joined, waits } = await joinSetup(
            [joinRequired, joinRequired, { ok: true, id: 52, createdAt: "2026-10-07T10:00:01Z" }],
            [{ code: "ok", message: null, isPending: true }]);

        const sending = sender.send(channel, "hello room");
        await settle();

        // The resend found the membership not there yet, so the sender waits before the next.
        expect(sent).toHaveLength(2);
        expect(waits).toHaveLength(1);
        expect(sender.pending(channel)[0].status).toBe("sending");

        waits[0].resolve();
        expect(await sending).toBe(true);

        expect(joined).toEqual([channel]);
        expect(sent).toHaveLength(3);
        expect(new Set(sent.map(s => s.key)).size).toBe(1);
    });

    test("a membership that never arrives fails the message as needing a join after five sends", async () => {
        const { sender, sent, waits } = await joinSetup(
            [joinRequired, joinRequired, joinRequired, joinRequired, joinRequired, joinRequired],
            [{ code: "ok", message: null, isPending: true }]);

        const sending = sender.send(channel, "hello room");
        for (let i = 0; i < 4; i++) {
            await settle();
            waits[i].resolve();
        }

        expect(await sending).toBe(false);
        expect(sent).toHaveLength(6);
        expect(sender.pending(channel)[0]).toMatchObject({ status: "failed", errorCode: "authz.join_required" });
    });

    test("without a join door the refusal fails the message as before", async () => {
        const { sender } = await setup([joinRequired]);

        expect(await sender.send(channel, "hello room")).toBe(false);

        expect(sender.pending(channel)[0]).toMatchObject({ status: "failed", errorCode: "authz.join_required" });
    });
});

// A send's key is published on its live copy, so a guessable key would let another member of the
// room take the next one first. Keys come only from the browser's cryptographic generator, and a
// browser without one sends nothing rather than a predictable key.
describe("a send's key", () => {
    const original = Object.getOwnPropertyDescriptor(globalThis, "crypto");

    /** Puts a stand-in for the browser's crypto object in place for one test. */
    function useCrypto(value: unknown): void {
        Object.defineProperty(globalThis, "crypto", { value, configurable: true, writable: true });
    }

    afterEach(() => {
        if (original) {
            Object.defineProperty(globalThis, "crypto", original);
        }
        else {
            delete (globalThis as { crypto?: unknown }).crypto;
        }
        jest.restoreAllMocks();
    });

    const uuidV4 = /^[0-9a-f]{8}-[0-9a-f]{4}-4[0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/;

    test("is the browser's randomUUID when it has one", () => {
        useCrypto({ randomUUID: () => "11111111-2222-4333-8444-555555555555" });

        expect(newLocalId()).toBe("11111111-2222-4333-8444-555555555555");
    });

    test("without randomUUID, is a version 4 UUID built from getRandomValues, never from Math.random", () => {
        const random = jest.spyOn(Math, "random");
        useCrypto({
            getRandomValues: (bytes: Uint8Array) => {
                bytes.forEach((_b, i) => bytes[i] = (i * 37 + 11) & 0xff);
                return bytes;
            }
        });

        const key = newLocalId();

        expect(key).toMatch(uuidV4);
        expect(random).not.toHaveBeenCalled();
    });

    test("with neither, there is no key", () => {
        useCrypto(undefined);

        expect(newLocalId()).toBeNull();
    });

    test("a sender with no key to give a message sends nothing and draws no row", async () => {
        const sent: string[] = [];
        const timelines = createTimelines({ fetchPage: async () => ({ messages: [], read_cursor: null, unread_count: 0, has_more: false }) });
        const sender = createSender({
            send: async (_c, body) => {
                sent.push(body);
                return { ok: true, id: 1, createdAt: "2026-10-14T10:00:00Z" };
            },
            timelines,
            personAliasGuid: me,
            newLocalId: () => null
        });

        expect(await sender.send(channel, "hello")).toBe(false);

        expect(sent).toEqual([]);
        expect(sender.pending(channel)).toEqual([]);
    });
});

describe("the composer and the service state", () => {
    test("read only and maintenance close the composer; normal and degraded leave it open", () => {
        expect(isComposerOpen("read_only")).toBe(false);
        expect(isComposerOpen("maintenance")).toBe(false);
        expect(isComposerOpen("normal")).toBe(true);
        expect(isComposerOpen("degraded")).toBe(true);
    });
});

describe("the composer's text after a send", () => {
    test("is kept when the shell keeps it, and emptied otherwise", () => {
        expect(textAfterSend("hello", true)).toBe("hello");
        expect(textAfterSend("hello", false)).toBe("");
    });
});

describe("a reply in a thread", () => {
    // A reply names its root and whether it is also sent to the conversation. The sender passes
    // both to the platform on every try, beside the row's key, as the fourth argument of its send:
    // send(channelId, body, key, { parentId, isShownInChannel }), which the shell turns into
    // chat_send_message's p_parent_id and p_shown_in_channel. The person asks for a reply with the
    // same fourth argument: sender.send(channelId, body, attempt?, { parentId, isShownInChannel }).
    type Reply = { parentId: number, isShownInChannel: boolean };
    type Sent = { body: string, key: string | undefined, reply: Reply | undefined };

    async function replySetup(answers: SendResult[]): Promise<{
        sender: ReturnType<typeof createSender>,
        timelines: ReturnType<typeof createTimelines>,
        sent: Sent[]
    }> {
        const sent: Sent[] = [];
        const timelines = createTimelines({ fetchPage: async () => ({ messages: [], read_cursor: null, unread_count: 0, has_more: false }) });
        await timelines.loadNewest(channel);
        const sender = createSender({
            send: ((_c: string, body: string, key?: string, reply?: Reply) => {
                sent.push({ body, key, reply });
                return Promise.resolve(answers.shift() as SendResult);
            }) as Parameters<typeof createSender>[0]["send"],
            timelines,
            personAliasGuid: me,
            newLocalId: () => "5b1e2c3d-4f50-4a6b-8c7d-9e0f1a2b3c4d"
        });
        return { sender, timelines, sent };
    }

    /** Sends a reply through the sender's fourth argument. */
    function sendReply(sender: ReturnType<typeof createSender>, body: string, reply: Reply): Promise<boolean> {
        return (sender.send as (channelId: string, body: string, attempt?: unknown, reply?: Reply) => Promise<boolean>)(channel, body, undefined, reply);
    }

    test("sends its root and whether it is also sent to the conversation", async () => {
        const { sender, sent } = await replySetup([
            { ok: true, id: 60, createdAt: "2026-10-13T10:00:00Z" },
            { ok: true, id: 61, createdAt: "2026-10-13T10:01:00Z" }
        ]);

        await sendReply(sender, "count me in", { parentId: 40, isShownInChannel: false });
        await sendReply(sender, "and me", { parentId: 40, isShownInChannel: true });

        expect(sent.map(s => s.reply)).toEqual([
            { parentId: 40, isShownInChannel: false },
            { parentId: 40, isShownInChannel: true }
        ]);
    });

    test("a retry keeps the root, the choice and the key", async () => {
        const { sender, sent } = await replySetup([
            { ok: false, error: { code: "rpc.transport", severity: "failed" } },
            { ok: true, id: 62, createdAt: "2026-10-13T10:02:00Z" }
        ]);
        await sendReply(sender, "count me in", { parentId: 40, isShownInChannel: true });

        expect(await sender.retry("5b1e2c3d-4f50-4a6b-8c7d-9e0f1a2b3c4d")).toBe(true);

        expect(sent).toEqual([
            { body: "count me in", key: "5b1e2c3d-4f50-4a6b-8c7d-9e0f1a2b3c4d", reply: { parentId: 40, isShownInChannel: true } },
            { body: "count me in", key: "5b1e2c3d-4f50-4a6b-8c7d-9e0f1a2b3c4d", reply: { parentId: 40, isShownInChannel: true } }
        ]);
    });

    test("a failed reply's row names its root, so the thread draws it and the conversation does not", async () => {
        const { sender } = await replySetup([{ ok: false, error: { code: "rpc.transport", severity: "failed" } }]);

        await sendReply(sender, "count me in", { parentId: 40, isShownInChannel: false });

        expect(sender.pending(channel)[0]).toEqual(expect.objectContaining({ status: "failed", parentId: 40, isShownInChannel: false }));
    });

    test("a confirmed reply shown only in its thread stays out of the conversation's timeline", async () => {
        const { sender, timelines } = await replySetup([{ ok: true, id: 63, createdAt: "2026-10-13T10:03:00Z" }]);

        await sendReply(sender, "just here", { parentId: 40, isShownInChannel: false });

        expect(timelines.state(channel).messages).toEqual([]);
        expect(sender.pending(channel)).toEqual([]);
    });

    test("a confirmed reply also sent to the conversation enters it as a reply, so its label draws", async () => {
        const { sender, timelines } = await replySetup([{ ok: true, id: 64, createdAt: "2026-10-13T10:04:00Z" }]);

        await sendReply(sender, "everyone", { parentId: 40, isShownInChannel: true });

        const [confirmed] = timelines.state(channel).messages;
        expect(confirmed.id).toBe(64);
        expect(confirmed.parent_id).toBe(40);
        expect(confirmed.shown_in_channel).toBe(true);
    });

    test("an ordinary message sends no reply", async () => {
        const { sender, sent } = await replySetup([{ ok: true, id: 65, createdAt: "2026-10-13T10:05:00Z" }]);

        await sender.send(channel, "plain");

        expect(sent[0].reply ?? null).toBeNull();
    });
});

// The person's own message is one message from the moment it is sent: its send's key is its
// identity. The message the answer builds carries the key, as every live copy and page of it does,
// and the sender keeps every key it sent under for the session, so a feed can tell its own sends
// from the person's sends in another tab.
describe("a send's identity", () => {
    test("the message the answer builds carries the send's key", async () => {
        const { sender, timelines } = await setup([{ ok: true, id: 41, createdAt: "2026-10-14T10:00:00Z" }]);

        await sender.send(channel, "hello");

        expect(timelines.state(channel).messages[0].client_key).toBe("local-1");
    });

    test("the session keeps every key it sent under, failed and discarded ones included", async () => {
        const { sender } = await setup([
            { ok: true, id: 41, createdAt: "2026-10-14T10:00:00Z" },
            { ok: false, error: { code: "rpc.transport", severity: "failed" } }
        ]);

        await sender.send(channel, "one");
        await sender.send(channel, "two");
        sender.discard("local-2");

        expect([...sender.sentKeys]).toEqual(["local-1", "local-2"]);
    });
});


// Three copies of a person's message reach the page in any order: the send's answer, the live copy
// and any page that holds it (history, catch-up, a resync). Whichever comes first settles the send:
// its row goes and the awaited send resolves, even if the answer never comes. Every order is driven
// by hand: each answer is a deferred the test resolves, each live copy is applied as the shell
// applies a live event, and the timelines report what lands to the sender, as the shell wires it.
describe("a send settles on the first copy of its message", () => {
    const anna = "a0000002-0000-4000-8000-000000000000";
    const transport: SendResult = { ok: false, error: { code: "rpc.transport", severity: "failed" } };

    type Harness = {
        sender: ReturnType<typeof createSender>,
        timelines: ReturnType<typeof createTimelines>,
        sent: Array<{ body: string, key: string | undefined }>,
        pages: Array<{ messages: TimelineMessage[] }>
    };

    async function harness(answers: Array<SendResult | Deferred<SendResult>>): Promise<Harness> {
        const sent: Array<{ body: string, key: string | undefined }> = [];
        const pages: Array<{ messages: TimelineMessage[] }> = [{ messages: [] }];
        let next = 0;
        let sender: ReturnType<typeof createSender> | null = null;
        const timelines = createTimelines({
            fetchPage: async () => ({ messages: pages.shift()?.messages ?? [], read_cursor: null, unread_count: 0, has_more: false }),
            onLanded: message => sender?.landed(message)
        });
        await timelines.loadNewest(channel);
        sender = createSender({
            send: (_c, body, key) => {
                sent.push({ body, key });
                const answer = answers.shift() as SendResult | Deferred<SendResult>;
                return "promise" in answer ? answer.promise : Promise.resolve(answer);
            },
            timelines,
            personAliasGuid: me,
            newLocalId: () => `local-${++next}`
        });
        return { sender, timelines, sent, pages };
    }

    /** A live message.created as the channel topic carries it, under a send's key. */
    function live(id: number, key: string | null, extra: Record<string, unknown> = {}): Record<string, unknown> {
        return {
            id, channel_id: channel, parent_id: null, shown_in_channel: false, person_alias_guid: me,
            message_type: "text", body: `live ${id}`, pieces: null, blocks: null, created_at: "2026-10-14T10:00:00Z",
            client_key: key, ...extra
        };
    }

    /** Lets every promise already settled run its continuations. */
    async function settle(): Promise<void> {
        for (let i = 0; i < 10; i++) {
            await Promise.resolve();
        }
    }

    /** Whether a promise has settled yet, without waiting for it. */
    async function isSettled(promise: Promise<unknown>): Promise<boolean> {
        let isDone = false;
        void promise.then(() => isDone = true);
        await settle();
        return isDone;
    }

    const ids = (h: Harness): number[] => h.timelines.state(channel).messages.map(m => m.id);

    test("the live copy first: the row goes and the send resolves at once, and the answer after it merges into the one message", async () => {
        const answer = deferred<SendResult>();
        const h = await harness([answer]);
        const sending = h.sender.send(channel, "On my way");

        h.timelines.applyEvent(channel, "message.created", live(41, "local-1", { body: "On my way" }));

        expect(h.sender.pending(channel)).toEqual([]);
        expect(await sending).toBe(true);
        expect(ids(h)).toEqual([41]);

        answer.resolve({ ok: true, id: 41, createdAt: "2026-10-14T10:00:00Z", notice: "Waiting for review." } as SendResult);
        await settle();
        expect(ids(h)).toEqual([41]);
        expect(h.timelines.state(channel).messages[0].notice).toBe("Waiting for review.");
    });

    test("the answer first: the row goes at the answer, and the live copy after it merges by id", async () => {
        const h = await harness([{ ok: true, id: 42, createdAt: "2026-10-14T10:00:00Z" }]);

        expect(await h.sender.send(channel, "hello")).toBe(true);
        h.timelines.applyEvent(channel, "message.created", live(42, "local-1"));

        expect(ids(h)).toEqual([42]);
        expect(h.sender.pending(channel)).toEqual([]);
    });

    test("an answer that never comes: the live copy alone settles the send", async () => {
        const h = await harness([deferred<SendResult>()]);
        const sending = h.sender.send(channel, "hello");

        h.timelines.applyEvent(channel, "message.created", live(43, "local-1"));

        expect(await isSettled(sending)).toBe(true);
        expect(await sending).toBe(true);
        expect(h.sender.pending(channel)).toEqual([]);
    });

    test("a page copy first, as a resync or a catch-up brings it: it settles the send", async () => {
        const h = await harness([deferred<SendResult>()]);
        const sending = h.sender.send(channel, "hello");
        h.pages.push({ messages: [{ id: 44, person_alias_guid: me, message_type: "text", body: "hello", created_at: "2026-10-14T10:00:00Z", client_key: "local-1" }] });

        await h.timelines.loadNewest(channel);

        expect(await sending).toBe(true);
        expect(h.sender.pending(channel)).toEqual([]);
        expect(ids(h)).toEqual([44]);
    });

    test("an answer that fails after the platform stored the message: the row fails, and a later copy settles it", async () => {
        const h = await harness([transport]);

        expect(await h.sender.send(channel, "hello")).toBe(false);
        expect(h.sender.pending(channel)[0].status).toBe("failed");

        h.timelines.applyEvent(channel, "message.created", live(45, "local-1"));

        expect(h.sender.pending(channel)).toEqual([]);
        expect(ids(h)).toEqual([45]);
    });

    test("an answer that fails after the live copy landed leaves the send settled, with no failed row", async () => {
        const answer = deferred<SendResult>();
        const h = await harness([answer]);
        const sending = h.sender.send(channel, "hello");
        h.timelines.applyEvent(channel, "message.created", live(46, "local-1"));

        answer.resolve(transport);
        await settle();

        expect(await sending).toBe(true);
        expect(h.sender.pending(channel)).toEqual([]);
    });

    test("an answer refusing the key as another message's, for a key the session saw land, counts as settled", async () => {
        const answer = deferred<SendResult>();
        const h = await harness([answer]);
        const sending = h.sender.send(channel, "hello");
        h.timelines.applyEvent(channel, "message.created", live(47, "local-1"));

        answer.resolve({ ok: false, error: { code: "rpc.idempotency_mismatch", severity: "failed" } });
        await settle();

        expect(await sending).toBe(true);
        expect(h.sender.pending(channel)).toEqual([]);
    });

    test("a try under a key whose message already landed sends nothing and resolves sent", async () => {
        const h = await harness([transport]);
        await h.sender.send(channel, "hello", { localId: "first-key", isLast: true });
        h.timelines.applyEvent(channel, "message.created", live(48, "first-key"));

        expect(await h.sender.send(channel, "hello", { localId: "first-key", isLast: true })).toBe(true);

        expect(h.sent).toHaveLength(1);
        expect(h.sender.pending(channel)).toEqual([]);
    });

    test("a retry goes under the same key, and a copy landing while it is out settles it", async () => {
        const second = deferred<SendResult>();
        const h = await harness([transport, second]);
        await h.sender.send(channel, "again");

        const retrying = h.sender.retry("local-1");
        h.timelines.applyEvent(channel, "message.created", live(49, "local-1"));

        expect(await retrying).toBe(true);
        expect(h.sent.map(s => s.key)).toEqual(["local-1", "local-1"]);
        expect(h.sender.pending(channel)).toEqual([]);
    });

    test("two sends in flight: each copy settles its own row, whatever the order", async () => {
        const h = await harness([deferred<SendResult>(), deferred<SendResult>()]);
        const first = h.sender.send(channel, "one");
        const second = h.sender.send(channel, "two");

        h.timelines.applyEvent(channel, "message.created", live(51, "local-2"));
        expect(h.sender.pending(channel).map(r => r.localId)).toEqual(["local-1"]);
        expect(await second).toBe(true);
        expect(await isSettled(first)).toBe(false);

        h.timelines.applyEvent(channel, "message.created", live(50, "local-1"));
        expect(await first).toBe(true);
        expect(ids(h)).toEqual([50, 51]);
    });

    test("the same text sent twice is two messages, each settled only by its own copy", async () => {
        const h = await harness([deferred<SendResult>(), deferred<SendResult>()]);
        void h.sender.send(channel, "amen");
        void h.sender.send(channel, "amen");

        h.timelines.applyEvent(channel, "message.created", live(52, "local-1", { body: "amen" }));

        expect(h.sender.pending(channel).map(r => r.localId)).toEqual(["local-2"]);
        expect(h.sent.map(s => s.key)).toEqual(["local-1", "local-2"]);
    });

    test("a reply also sent to the conversation is settled by its copy there", async () => {
        const h = await harness([deferred<SendResult>()]);
        const sending = h.sender.send(channel, "everyone", undefined, { parentId: 40, isShownInChannel: true });

        h.timelines.applyEvent(channel, "message.created", live(53, "local-1", { parent_id: 40, shown_in_channel: true }));

        expect(await sending).toBe(true);
        expect(h.sender.pending(channel)).toEqual([]);
    });

    test("a reply shown only in its thread is settled by the copy the open thread reports", async () => {
        const h = await harness([deferred<SendResult>()]);
        const sending = h.sender.send(channel, "just here", undefined, { parentId: 40, isShownInChannel: false });

        h.sender.landed({ id: 54, client_key: "local-1" });

        expect(await sending).toBe(true);
        expect(h.sender.pending(channel)).toEqual([]);
        expect(ids(h)).toEqual([]);
    });

    test("the person's message from another tab, under a key this session never sent, settles nothing", async () => {
        const h = await harness([deferred<SendResult>()]);
        const sending = h.sender.send(channel, "from here");

        h.timelines.applyEvent(channel, "message.created", live(55, "another-tab"));

        expect(h.sender.pending(channel).map(r => r.localId)).toEqual(["local-1"]);
        expect(await isSettled(sending)).toBe(false);
        expect(ids(h)).toEqual([55]);
    });

    test("an edit and a delete right after the live copy survive the answer that follows", async () => {
        const first = deferred<SendResult>();
        const second = deferred<SendResult>();
        const h = await harness([first, second]);
        void h.sender.send(channel, "typo");
        void h.sender.send(channel, "oops");

        h.timelines.applyEvent(channel, "message.created", live(56, "local-1", { body: "typo" }));
        h.timelines.applyEvent(channel, "message.edited", { id: 56, channel_id: channel, body: "fixed", pieces: null, edited_at: "2026-10-14T10:00:05Z" });
        h.timelines.applyEvent(channel, "message.created", live(57, "local-2", { body: "oops" }));
        h.timelines.applyEvent(channel, "message.deleted", { id: 57, channel_id: channel, deleted_at: "2026-10-14T10:00:06Z" });

        first.resolve({ ok: true, id: 56, createdAt: "2026-10-14T10:00:00Z" });
        second.resolve({ ok: true, id: 57, createdAt: "2026-10-14T10:00:00Z" });
        await settle();

        const [edited, deleted] = h.timelines.state(channel).messages;
        expect([edited.body, edited.edited_at]).toEqual(["fixed", "2026-10-14T10:00:05Z"]);
        expect([deleted.body, deleted.deleted_at]).toEqual([null, "2026-10-14T10:00:06Z"]);
        expect(h.sender.pending(channel)).toEqual([]);
    });

    test("another person's newer message before the answer: one message each, in id order", async () => {
        const answer = deferred<SendResult>();
        const h = await harness([answer]);
        const sending = h.sender.send(channel, "mine");

        h.timelines.applyEvent(channel, "message.created", live(59, null, { person_alias_guid: anna }));
        expect(h.sender.pending(channel)).toHaveLength(1);
        answer.resolve({ ok: true, id: 58, createdAt: "2026-10-14T10:00:00Z" });
        expect(await sending).toBe(true);
        h.timelines.applyEvent(channel, "message.created", live(58, "local-1"));

        expect(ids(h)).toEqual([58, 59]);
        expect(h.sender.pending(channel)).toEqual([]);
    });
});
