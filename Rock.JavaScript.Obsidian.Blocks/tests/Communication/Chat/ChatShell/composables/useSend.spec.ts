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
import { createSender, isComposerOpen, JoinResult, SendResult, textAfterSend } from "../../../../../src/Communication/Chat/ChatShell/composables/useSend.partial";
import { createTimelines } from "../../../../../src/Communication/Chat/ChatShell/composables/useHistory.partial";

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

    test("a later try of a row sends the text it is given, under the row's key, as the one row", async () => {
        const { sender, sent } = await setup([
            { ok: false, error: { code: "rpc.transport", severity: "failed" } },
            { ok: true, id: 48, createdAt: "2026-10-06T10:00:00Z" }
        ]);

        await sender.send(channel, "first words", { localId: "local-9", isLast: true });
        expect(await sender.send(channel, "edited words", { localId: "local-9", isLast: true })).toBe(true);

        // the person changed the text before trying again; the try sends what they see
        expect(sent).toEqual(["first words", "edited words"]);
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

// The person's own message reaches the page twice: once as the send's answer and once as its live
// copy, in either order. While a send of theirs waits on its answer in a channel, their own live
// copies there are held by the sender, so the in-flight row is the only one on screen; the answer
// then merges a copy of the same message into the message it confirmed, and lets any other copy
// in. Every order is driven by hand: the answer is a deferred the test resolves, and the live copy
// is handed to the sender as the shell hands it each live event.
describe("the person's own live copy while a send waits on its answer", () => {
    const anna = "a0000002-0000-4000-8000-000000000000";
    const otherChannel = "c0000002-0000-4000-8000-000000000000";

    /** A live message.created payload, as the channel topic carries it. */
    function liveCopy(id: number, sender = me, extra: Record<string, unknown> = {}): Record<string, unknown> {
        return {
            id,
            channel_id: channel,
            parent_id: null,
            shown_in_channel: false,
            person_alias_guid: sender,
            message_type: "text",
            body: `live ${id}`,
            pieces: null,
            blocks: null,
            created_at: "2026-10-14T10:00:00Z",
            ...extra
        };
    }

    /** Hands a live event to the sender first and to the timeline only when the sender does not hold it, as the shell does. */
    function deliverLive(sender: ReturnType<typeof createSender>, timelines: ReturnType<typeof createTimelines>, channelId: string, payload: unknown): boolean {
        const isHeld = sender.holdLive(channelId, "message.created", payload);
        if (!isHeld) {
            timelines.applyEvent(channelId, "message.created", payload);
        }
        return isHeld;
    }

    const ids = (timelines: ReturnType<typeof createTimelines>): number[] => timelines.state(channel).messages.map(m => m.id);

    test("the live copy first: it is held while the answer is out, then merged into the confirmed message, one message in all", async () => {
        const answer = deferred<SendResult>();
        const { sender, timelines } = await setup([answer]);
        const sending = sender.send(channel, "On my way");

        expect(deliverLive(sender, timelines, channel, liveCopy(41, me.toUpperCase(), { body: "On my way", pieces: [{ type: "text", text: "On my way" }] }))).toBe(true);
        expect(ids(timelines)).toEqual([]);
        expect(sender.pending(channel)).toHaveLength(1);

        answer.resolve({ ok: true, id: 41, createdAt: "2026-10-14T10:00:00Z" });
        await sending;

        expect(sender.pending(channel)).toEqual([]);
        expect(ids(timelines)).toEqual([41]);
        // the copy's own fields reach the one message, as a copy that came after the answer would
        expect(timelines.state(channel).messages[0].pieces).toEqual([{ type: "text", text: "On my way" }]);
    });

    test("the answer first: nothing is in flight, so the live copy goes straight to the timeline and merges by id", async () => {
        const { sender, timelines } = await setup([{ ok: true, id: 42, createdAt: "2026-10-14T10:00:00Z" }]);
        await sender.send(channel, "hello");

        expect(deliverLive(sender, timelines, channel, liveCopy(42))).toBe(false);
        expect(ids(timelines)).toEqual([42]);
    });

    test("a copy of another of the person's messages is held, and let in the moment the answer arrives", async () => {
        const answer = deferred<SendResult>();
        const { sender, timelines } = await setup([answer]);
        const sending = sender.send(channel, "from here");

        expect(deliverLive(sender, timelines, channel, liveCopy(50))).toBe(true);
        expect(ids(timelines)).toEqual([]);

        answer.resolve({ ok: true, id: 51, createdAt: "2026-10-14T10:00:01Z" });
        await sending;

        expect(ids(timelines)).toEqual([50, 51]);
    });

    test("a send that fails lets every held copy in", async () => {
        const answer = deferred<SendResult>();
        const { sender, timelines } = await setup([answer]);
        const sending = sender.send(channel, "lost");

        deliverLive(sender, timelines, channel, liveCopy(60));
        answer.resolve({ ok: false, error: { code: "rpc.transport", severity: "failed" } });
        await sending;

        expect(ids(timelines)).toEqual([60]);
        expect(sender.pending(channel)[0].status).toBe("failed");
    });

    test("stopping lets every held copy in, so nothing is left waiting on an answer that never comes", async () => {
        const answer = deferred<SendResult>();
        const { sender, timelines } = await setup([answer]);
        void sender.send(channel, "never answered");

        deliverLive(sender, timelines, channel, liveCopy(70));
        sender.stop();

        expect(ids(timelines)).toEqual([70]);
    });

    test("another person's message, a copy in another channel, and any event but a new message are never held", async () => {
        const answer = deferred<SendResult>();
        const { sender, timelines } = await setup([answer]);
        void sender.send(channel, "waiting");

        expect(sender.holdLive(channel, "message.created", liveCopy(80, anna))).toBe(false);
        expect(sender.holdLive(otherChannel, "message.created", liveCopy(81))).toBe(false);
        expect(sender.holdLive(channel, "message.edited", liveCopy(82))).toBe(false);
        expect(sender.holdLive(channel, "message.created", null)).toBe(false);
    });

    test("with nothing in flight the person's own copy is not held", async () => {
        const { sender } = await setup([]);

        expect(sender.holdLive(channel, "message.created", liveCopy(90))).toBe(false);
    });

    test("with two sends in flight, the first answer merges its own copy and keeps the second's held until the second answers", async () => {
        const first = deferred<SendResult>();
        const second = deferred<SendResult>();
        const { sender, timelines } = await setup([first, second]);
        const sendingFirst = sender.send(channel, "one");
        const sendingSecond = sender.send(channel, "two");

        deliverLive(sender, timelines, channel, liveCopy(101));
        deliverLive(sender, timelines, channel, liveCopy(102));

        // the second's copy would stand beside its own row still sending
        second.resolve({ ok: true, id: 102, createdAt: "2026-10-14T10:00:02Z" });
        await sendingSecond;
        expect(ids(timelines)).toEqual([102]);

        first.resolve({ ok: true, id: 101, createdAt: "2026-10-14T10:00:01Z" });
        await sendingFirst;
        expect(ids(timelines)).toEqual([101, 102]);
        expect(sender.pending(channel)).toEqual([]);
    });
});
