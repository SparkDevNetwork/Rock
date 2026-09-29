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
import { churchTokenFromAction, createChatShell, PlatformClientLike, refusalMessage, RpcResult } from "../../../../src/Communication/Chat/ChatShell/shell.partial";
import { channelTopic, personalTopic } from "../../../../src/Communication/Chat/ChatShell/composables/useRealtimeHub.partial";
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

/** A topic the fake client joined, with the listeners the shell gave it. */
type LiveTopic = {
    topic: string;
    listeners: Record<string, (message: unknown) => void>;
    status: ((status: string, error?: unknown) => void) | null;
};

function build(): {
    shell: ReturnType<typeof createChatShell>,
    history: Record<string, Deferred<RpcResult>[]>,
    saves: Array<{ channelId: string, messageId: number, answer: Deferred<Response> }>,
    joined: string[],
    removed: string[],
    stored: Record<string, string>,
    fireBlur: () => void,
    topics: LiveTopic[],
    counts: { mints: number, clients: number, setAuths: number, bootstraps: number },
    mintGate: { value: string }
} {
    const topics: LiveTopic[] = [];
    const counts = { mints: 0, clients: 0, setAuths: 0, bootstraps: 0 };
    const mintGate = { value: "ok" };
    const history: Record<string, Deferred<RpcResult>[]> = {};
    const saves: Array<{ channelId: string, messageId: number, answer: Deferred<Response> }> = [];
    const joined: string[] = [];
    const removed: string[] = [];
    const stored: Record<string, string> = {};
    const windowListeners: Record<string, Array<() => void>> = {};

    const client: PlatformClientLike = {
        realtime: {
            setAuth: async () => {
                counts.setAuths++;
            }
        },
        channel: (topic) => {
            joined.push(topic);
            const live: LiveTopic = { topic, listeners: {}, status: null };
            topics.push(live);
            const channel = {
                topic,
                on: (type: string, _filter: unknown, callback: (message: unknown) => void) => {
                    live.listeners[type] = callback;
                    return channel;
                },
                subscribe: (callback: (status: string, error?: unknown) => void) => {
                    live.status = callback;
                    return channel;
                }
            };
            return channel as never;
        },
        removeChannel: async (channel) => {
            removed.push((channel as unknown as { topic: string }).topic);
            return "ok";
        },
        rpc: (name, args) => {
            if (name === "chat_get_bootstrap") {
                counts.bootstraps++;
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
        mintChurchToken: async () => {
            counts.mints++;
            const isOk = mintGate.value === "ok";
            return { isSuccess: true, statusCode: 200, data: { gate: mintGate.value, churchToken: isOk ? `church-${counts.mints}` : null } };
        },
        createPlatformClient: () => {
            counts.clients++;
            return client;
        },
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

    return {
        shell, history, saves, joined, removed, stored, topics, counts, mintGate,
        fireBlur: () => (windowListeners["blur"] ?? []).forEach(l => l())
    };
}

/** Answers the oldest pending history call for a channel. */
function answerHistory(h: ReturnType<typeof build>, channelId: string, id: number): void {
    const pending = h.history[channelId]?.shift();
    if (!pending) {
        throw new Error(`no history call pending for ${channelId}`);
    }
    pending.resolve({ data: page(id), error: null, status: 200 });
}

describe("churchTokenFromAction", () => {
    test("an ended Rock sign-in is a refusal", () => {
        expect(churchTokenFromAction({ isSuccess: false, statusCode: 401 })).toEqual({ gate: "sign_in_required", churchToken: null });
    });

    test("a Rock that refused the action is a refusal, not an outage", () => {
        expect(churchTokenFromAction({ isSuccess: false, statusCode: 403 })).toEqual({ gate: "gate_unavailable", churchToken: null });
    });

    test.each([0, 429, 500, 503])("a Rock that could not answer (%p) is only unreachable", status => {
        expect(churchTokenFromAction({ isSuccess: false, statusCode: status })).toEqual({ gate: "gate_unavailable", churchToken: null, isUnreachable: true });
    });

    test("a token Rock signed passes through", () => {
        expect(churchTokenFromAction({ isSuccess: true, statusCode: 200, data: { gate: "ok", churchToken: "t" } })).toEqual({ gate: "ok", churchToken: "t" });
    });
});

describe("refusalMessage", () => {
    test("a platform that could not start chat is not blamed on the person's account", () => {
        expect(refusalMessage("failed", "ok")).toBe(refusalMessage("refused", "gate_unavailable"));
    });

    test("a refused gate is told by its gate", () => {
        expect(refusalMessage("refused", "age_restricted")).toBe("Chat is not available at your age.");
    });
});

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


/** The last topic joined under this name. */
function topicOf(h: ReturnType<typeof build>, topic: string): LiveTopic {
    const found = [...h.topics].reverse().find(t => t.topic === topic);
    if (!found) {
        throw new Error(`${topic} was never joined`);
    }
    return found;
}

/** Starts the shell with channel A open and joined. */
async function started(): Promise<ReturnType<typeof build>> {
    const h = build();
    const starting = h.shell.start();
    await settle();
    answerHistory(h, channelA, 10);
    await starting;
    await settle();
    topicOf(h, channelTopic(tenant, channelA)).status?.("SUBSCRIBED");
    await settle();
    return h;
}

/** Delivers an event on the person's own topic. */
function personal(h: ReturnType<typeof build>, event: string, payload: unknown): void {
    topicOf(h, personalTopic(tenant, person)).listeners["broadcast"]?.({ event, payload });
}

describe("the live cut", () => {
    afterEach(() => {
        jest.useRealTimers();
        jest.restoreAllMocks();
    });

    test("a membership change in the channel on screen fetches a new token for the open socket", async () => {
        const h = await started();
        const before = { ...h.counts };

        personal(h, "membership.changed", { channel_id: channelA });
        await settle();

        expect(h.counts.mints).toBe(before.mints + 1);
        expect(h.counts.setAuths).toBe(before.setAuths + 1);
    });

    test("a recheck and a membership change together fetch one token, not two", async () => {
        const h = await started();
        const before = h.counts.mints;

        personal(h, "membership.changed", { channel_id: channelA });
        personal(h, "session.recheck", {});
        await settle();

        expect(h.counts.mints).toBe(before + 1);
    });

    test("a membership change in another channel only reloads the list", async () => {
        const h = await started();
        const before = { ...h.counts };

        personal(h, "membership.changed", { channel_id: channelB });
        await settle();

        expect(h.counts.mints).toBe(before.mints);
        expect(h.counts.bootstraps).toBe(before.bootstraps + 1);
    });

    test("a change to the channel on screen fetches a new token after a random wait of up to five seconds", async () => {
        const h = await started();
        jest.useFakeTimers({ doNotFake: ["queueMicrotask", "nextTick"] });
        jest.spyOn(Math, "random").mockReturnValue(0.5);
        const before = h.counts.mints;

        topicOf(h, channelTopic(tenant, channelA)).listeners["broadcast"]?.({ event: "channel.changed", payload: { channel_id: channelA } });
        await settle();
        jest.advanceTimersByTime(2_400);
        await settle();
        expect(h.counts.mints).toBe(before);

        jest.advanceTimersByTime(200);
        await settle();
        expect(h.counts.mints).toBe(before + 1);
    });

    test("a refresh Rock refuses leaves both topics and shows the gate", async () => {
        const h = await started();
        h.mintGate.value = "banned";

        personal(h, "session.recheck", {});
        await settle();

        expect(h.removed.sort()).toEqual([channelTopic(tenant, channelA), personalTopic(tenant, person)].sort());
        expect(h.shell.state.phase).toBe("refused");
        expect(h.shell.state.gate).toBe("banned");
    });

    test("a refresh keeps the one platform client and its socket", async () => {
        const h = await started();
        const before = h.counts.mints;

        personal(h, "session.recheck", {});
        await settle();

        expect(h.counts.mints).toBe(before + 1);
        expect(h.counts.clients).toBe(1);
        expect(h.removed).toEqual([]);
    });

    test("a channel Realtime closes as no longer readable tells the person and reloads the list", async () => {
        const h = await started();
        const before = h.counts.bootstraps;
        const open = topicOf(h, channelTopic(tenant, channelA));

        open.listeners["system"]?.({ status: "error", message: "You do not have permissions to read from this Channel topic: x" });
        open.status?.("CLOSED");
        await settle();

        expect(h.shell.state.errors.map(e => e.code)).toContain("rt.read_revoked");
        expect(h.counts.bootstraps).toBe(before + 1);
    });

    test("a wait begun before the shell stops never fetches a token after it", async () => {
        const h = await started();
        jest.useFakeTimers({ doNotFake: ["queueMicrotask", "nextTick"] });
        jest.spyOn(Math, "random").mockReturnValue(0.5);
        const before = h.counts.mints;

        topicOf(h, channelTopic(tenant, channelA)).listeners["broadcast"]?.({ event: "channel.changed", payload: { channel_id: channelA } });
        await settle();
        await h.shell.stop();
        jest.advanceTimersByTime(10_000);
        await settle();

        expect(h.counts.mints).toBe(before);
    });

    test("a channel whose read was revoked is no longer the open one, so choosing it again opens it", async () => {
        const h = await started();
        const open = topicOf(h, channelTopic(tenant, channelA));

        open.listeners["system"]?.({ status: "error", message: "You do not have permissions to read from this Channel topic: x" });
        await settle();
        expect(h.shell.state.activeChannelId).toBeNull();

        const joinsBefore = h.joined.length;
        const reopening = h.shell.selectChannel(channelA);
        await settle();
        while (h.history[channelA]?.length) {
            answerHistory(h, channelA, 11);
            await settle();
        }
        await reopening;

        expect(h.shell.state.activeChannelId).toBe(channelA);
        expect(h.joined.slice(joinsBefore)).toEqual([channelTopic(tenant, channelA)]);
    });

    test("once Rock refuses a refresh, choosing a channel joins nothing", async () => {
        const h = await started();
        h.mintGate.value = "banned";
        personal(h, "session.recheck", {});
        await settle();
        const joinsBefore = h.joined.length;

        void h.shell.selectChannel(channelB);
        await settle();

        expect(h.joined.slice(joinsBefore)).toEqual([]);
    });
});

