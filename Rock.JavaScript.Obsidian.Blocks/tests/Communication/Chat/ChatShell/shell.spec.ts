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
// The shell's wiring, with every outside call stood in for. The case here is a person who opens
// channels faster than the platform answers: the channel chosen last is the one on screen, its
// topic is the one joined, it is the one remembered, and what is seen in it is saved against it,
// however the earlier opens and saves settle.
import { createChatShell, PlatformClientLike, RpcResult } from "../../../../src/Communication/Chat/ChatShell/shell.partial";
import { channelTopic } from "../../../../src/Communication/Chat/ChatShell/composables/useRealtimeHub.partial";
import { HistoryPage } from "../../../../src/Communication/Chat/ChatShell/types.partial";

const tenant = "10000000-0000-4000-8000-000000000001";
const person = "a0000007-0000-4000-8000-000000000000";
const channelA = "c000000a-0000-4000-8000-000000000000";
const channelB = "c000000b-0000-4000-8000-000000000000";
const channelC = "c000000c-0000-4000-8000-000000000000";

type Deferred<T> = { promise: Promise<T>, resolve: (value: T) => void };

function deferred<T>(): Deferred<T> {
    let resolve!: (value: T) => void;
    const promise = new Promise<T>(r => resolve = r);
    return { promise, resolve };
}

async function settle(): Promise<void> {
    for (let i = 0; i < 30; i++) {
        await Promise.resolve();
    }
}

function page(id: number): HistoryPage {
    return {
        messages: [{ id, person_alias_guid: "a0000001-0000-4000-8000-000000000000", sender_nick_name: "Ada", message_type: "text", body: `m${id}`, created_at: "2026-09-23T10:00:00Z" }],
        read_cursor: id - 1,
        unread_count: 1,
        has_more: false
    };
}

function fakeResponse(status: number, body: unknown): Response {
    return { ok: status >= 200 && status < 300, status, json: async () => body } as unknown as Response;
}

function build(): {
    shell: ReturnType<typeof createChatShell>,
    history: Record<string, Deferred<RpcResult>[]>,
    saves: Array<{ channelId: string, messageId: number, answer: Deferred<Response> }>,
    joined: string[],
    removed: string[],
    stored: Record<string, string>,
    fireBlur: () => void
} {
    const history: Record<string, Deferred<RpcResult>[]> = {};
    const saves: Array<{ channelId: string, messageId: number, answer: Deferred<Response> }> = [];
    const joined: string[] = [];
    const removed: string[] = [];
    const stored: Record<string, string> = {};
    const windowListeners: Record<string, Array<() => void>> = {};

    const client: PlatformClientLike = {
        realtime: { setAuth: async () => undefined },
        channel: (topic) => {
            joined.push(topic);
            const channel = {
                topic,
                on: () => channel,
                subscribe: () => channel
            };
            return channel;
        },
        removeChannel: async (channel) => {
            removed.push((channel as unknown as { topic: string }).topic);
            return "ok";
        },
        rpc: (name, args) => {
            if (name === "chat_get_bootstrap") {
                return Promise.resolve({ data: [], error: null, status: 200 });
            }
            const answer = deferred<RpcResult>();
            (history[args.p_channel_id as string] ??= []).push(answer);
            return answer.promise;
        }
    };

    const shell = createChatShell({
        session: { gate: "ok", projectUrl: "http://platform", publishableKey: "k", tenantId: tenant, personAliasGuid: person },
        linkedChannelId: channelA,
        mintChurchToken: async () => ({ isSuccess: true, statusCode: 200, data: { gate: "ok", churchToken: "church" } }),
        createPlatformClient: () => client,
        fetch: async (url, init) => {
            if (url.endsWith("/functions/v1/token-exchange")) {
                return fakeResponse(200, { access_token: "platform", expires_in: 300 });
            }
            const body = JSON.parse(init.body as string);
            const answer = deferred<Response>();
            saves.push({ channelId: body.p_channel_id, messageId: body.p_message_id, answer });
            return answer.promise;
        },
        storage: { getItem: k => stored[k] ?? null, setItem: (k, v) => stored[k] = v },
        pageTargets: {
            document: { visibilityState: "visible", addEventListener: () => undefined, removeEventListener: () => undefined },
            window: {
                addEventListener: (type, listener) => (windowListeners[type] ??= []).push(listener),
                removeEventListener: () => undefined
            }
        },
        mark: () => undefined
    });

    return { shell, history, saves, joined, removed, stored, fireBlur: () => (windowListeners["blur"] ?? []).forEach(l => l()) };
}

/** Answers the oldest pending history call for a channel. */
function answerHistory(h: ReturnType<typeof build>, channelId: string, id: number): void {
    const pending = h.history[channelId]?.shift();
    if (!pending) {
        throw new Error(`no history call pending for ${channelId}`);
    }
    pending.resolve({ data: page(id), error: null, status: 200 });
}

describe("createChatShell", () => {
    test("the channel chosen last stays open, however the earlier opens and saves settle", async () => {
        const h = build();
        const starting = h.shell.start();
        await settle();
        answerHistory(h, channelA, 10);
        await starting;

        // A is open and something new was seen in it, so leaving A saves.
        h.shell.seen(10);
        const toB = h.shell.selectChannel(channelB);
        const toC = h.shell.selectChannel(channelC);
        await settle();

        // C's history returns, then B's, then A's save.
        answerHistory(h, channelC, 30);
        await settle();
        if (h.history[channelB]?.length) {
            answerHistory(h, channelB, 20);
        }
        await settle();
        h.saves.forEach(s => s.answer.resolve(fakeResponse(200, { read_cursor: s.messageId, last_message_id: s.messageId })));
        await Promise.all([toB, toC]);
        await settle();

        expect(h.shell.state.activeChannelId).toBe(channelC);
        expect(h.joined[h.joined.length - 1]).toBe(channelTopic(tenant, channelC));
        expect(h.removed).not.toContain(channelTopic(tenant, channelC));
        expect(Object.values(h.stored)).toEqual([channelC]);
    });

    test("what is seen in the channel on screen is saved against that channel", async () => {
        const h = build();
        const starting = h.shell.start();
        await settle();
        answerHistory(h, channelA, 10);
        await starting;
        h.shell.seen(10);

        const toB = h.shell.selectChannel(channelB);
        await settle();
        // A's history, fetched again late, must not take the tracker back to A.
        const toA = h.shell.selectChannel(channelA);
        const toBAgain = h.shell.selectChannel(channelB);
        await settle();
        while (h.history[channelB]?.length) {
            answerHistory(h, channelB, 20);
            await settle();
        }
        while (h.history[channelA]?.length) {
            answerHistory(h, channelA, 10);
            await settle();
        }
        h.saves.forEach(s => s.answer.resolve(fakeResponse(200, { read_cursor: s.messageId, last_message_id: s.messageId })));
        await Promise.all([toB, toA, toBAgain]);
        await settle();
        const before = h.saves.length;

        h.shell.seen(20);
        h.fireBlur();
        await settle();

        expect(h.shell.state.activeChannelId).toBe(channelB);
        expect(h.saves.slice(before).map(s => `${s.channelId}:${s.messageId}`)).toEqual([`${channelB}:20`]);
    });
});
