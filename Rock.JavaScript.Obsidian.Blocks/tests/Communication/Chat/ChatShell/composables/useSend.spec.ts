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
