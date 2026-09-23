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
import { createSender, SendResult } from "../../../../../src/Communication/Chat/ChatShell/composables/useSend.partial";
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
