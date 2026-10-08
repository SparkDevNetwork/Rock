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
import { churchTokenFromAction, createChatShell, PlatformClientLike, refusalMessage } from "../../../../src/Communication/Chat/ChatShell/shell.partial";
import { channelTopic, personalTopic } from "../../../../src/Communication/Chat/ChatShell/composables/useRealtimeHub.partial";
import { HistoryPage } from "../../../../src/Communication/Chat/ChatShell/types.partial";
import { DoorResult } from "../../../../src/Communication/Chat/ChatShell/composables/useDirectMessage.partial";

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
    return { ok: status >= 200 && status < 300, status, headers: { get: () => null }, json: async () => body } as unknown as Response;
}

/** A topic the fake client joined, with the listeners the shell gave it. */
type LiveTopic = {
    topic: string;
    listeners: Record<string, (message: unknown) => void>;
    status: ((status: string, error?: unknown) => void) | null;
};

function build(options: { startDirectMessage?: (personAliasGuids: string[]) => Promise<DoorResult> } = {}): {
    shell: ReturnType<typeof createChatShell>,
    history: Record<string, Deferred<Response>[]>,
    saves: Array<{ channelId: string, messageId: number, answer: Deferred<Response>, token?: string | null }>,
    joined: string[],
    removed: string[],
    stored: Record<string, string>,
    fireBlur: () => void,
    topics: LiveTopic[],
    counts: { mints: number, clients: number, setAuths: number, bootstraps: number },
    mintGate: { value: string },
    bootstrap: { value: unknown },
    pageDocument: { visibilityState: string },
    sent: Array<{ channelId: string, body: string }>,
    tokenPerson: { value: string | null },
    reloads: { count: number }
} {
    const topics: LiveTopic[] = [];
    const counts = { mints: 0, clients: 0, setAuths: 0, bootstraps: 0 };
    const mintGate = { value: "ok" };
    const bootstrap: { value: unknown } = { value: [] };
    const history: Record<string, Deferred<Response>[]> = {};
    const saves: Array<{ channelId: string, messageId: number, answer: Deferred<Response>, token?: string | null }> = [];
    const joined: string[] = [];
    const removed: string[] = [];
    const stored: Record<string, string> = {};
    const sent: Array<{ channelId: string, body: string }> = [];
    // The person the platform token names; null hands out a token that is not a JWT.
    const tokenPerson: { value: string | null } = { value: null };
    const reloads = { count: 0 };
    const windowListeners: Record<string, Array<() => void>> = {};
    const pageDocument = { visibilityState: "visible", addEventListener: () => undefined, removeEventListener: () => undefined };

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
                return fakeResponse(200, { access_token: tokenFor(tokenPerson.value), expires_in: 300 });
            }
            const body = JSON.parse(init.body as string);
            // Every platform call is a POST through the shell's one call path.
            if (url.endsWith("/rest/v1/rpc/chat_get_bootstrap")) {
                counts.bootstraps++;
                return fakeResponse(200, bootstrap.value);
            }
            if (url.endsWith("/rest/v1/rpc/chat_get_history")) {
                const pending = deferred<Response>();
                (history[body.p_channel_id as string] ??= []).push(pending);
                return pending.promise;
            }
            if (url.endsWith("/rest/v1/rpc/chat_find_dm")) {
                return fakeResponse(200, { channel_id: null });
            }
            if (url.endsWith("/rest/v1/rpc/chat_send_message")) {
                sent.push({ channelId: body.p_channel_id, body: body.p_body });
                return fakeResponse(200, { id: 900 + sent.length, created_at: "2026-10-06T10:00:00Z", notice: null });
            }
            const answer = deferred<Response>();
            const token = (init.headers as Record<string, string> | undefined)?.["Authorization"] ?? null;
            saves.push({ channelId: body.p_channel_id, messageId: body.p_message_id, answer, token });
            return answer.promise;
        },
        storage: { getItem: k => stored[k] ?? null, setItem: (k, v) => stored[k] = v },
        pageTargets: {
            document: pageDocument,
            window: {
                addEventListener: (type, listener) => (windowListeners[type] ??= []).push(listener),
                removeEventListener: () => undefined
            }
        },
        mark: () => undefined,
        startDirectMessage: options.startDirectMessage,
        reloadPage: () => {
            reloads.count++;
        }
    });

    return {
        shell, history, saves, joined, removed, stored, topics, counts, mintGate, bootstrap, pageDocument, sent, tokenPerson, reloads,
        fireBlur: () => (windowListeners["blur"] ?? []).forEach(l => l())
    };
}

/** A platform token naming a person in its subject, as the token exchange signs one; unsigned here. */
function tokenFor(personAliasGuid: string | null): string {
    if (personAliasGuid === null) {
        return "platform";
    }
    const part = (value: unknown): string => Buffer.from(JSON.stringify(value)).toString("base64url");
    return `${part({ alg: "ES256" })}.${part({ sub: personAliasGuid, tid: tenant })}.signature`;
}

/** Answers the oldest pending history call for a channel. */
function answerHistory(h: ReturnType<typeof build>, channelId: string, id: number): void {
    const pending = h.history[channelId]?.shift();
    if (!pending) {
        throw new Error(`no history call pending for ${channelId}`);
    }
    pending.resolve(fakeResponse(200, page(id)));
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
async function started(options: Parameters<typeof build>[0] = {}): Promise<ReturnType<typeof build>> {
    const h = build(options);
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

    test("a refreshed token that names another person, after a merge, reloads the page", async () => {
        const h = await started();
        h.tokenPerson.value = "a0000099-0000-4000-8000-000000000000";

        personal(h, "session.recheck", {});
        await settle();

        expect(h.reloads.count).toBe(1);
    });

    test("a refreshed token that names the same person does not reload the page", async () => {
        const h = await started();
        h.tokenPerson.value = person;

        personal(h, "session.recheck", {});
        await settle();

        expect(h.reloads.count).toBe(0);
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

    test("an open still loading when its channel is revoked does not take the channel back", async () => {
        const h = await started();
        const toB = h.shell.selectChannel(channelB);
        await settle();
        const b = topicOf(h, channelTopic(tenant, channelB));
        b.status?.("SUBSCRIBED");
        await settle();

        b.listeners["system"]?.({ status: "error", message: "You do not have permissions to read from this Channel topic: x" });
        await settle();
        while (h.history[channelB]?.length) {
            answerHistory(h, channelB, 21);
            await settle();
        }
        h.saves.forEach(save => save.answer.resolve(fakeResponse(200, { read_cursor: save.messageId, last_message_id: save.messageId })));
        await toB;
        await settle();

        expect(h.shell.state.activeChannelId).toBeNull();
        expect(Object.values(h.stored)).not.toContain(channelB);
    });

    test("a shell stopped while it signs in joins nothing", async () => {
        const h = build();
        const starting = h.shell.start();
        await h.shell.stop();
        await settle();
        while (h.history[channelA]?.length) {
            answerHistory(h, channelA, 10);
            await settle();
        }
        await starting;

        expect(h.joined).toEqual([]);
    });

    test("closing stops refreshes before the last save, and the save still carries the token", async () => {
        const h = await started();
        h.shell.seen(10);
        const before = h.counts.mints;

        const stopping = h.shell.stop();
        await settle();
        personal(h, "session.recheck", {});
        await settle();
        h.saves.forEach(save => save.answer.resolve(fakeResponse(200, { read_cursor: save.messageId, last_message_id: save.messageId })));
        await stopping;

        expect(h.counts.mints).toBe(before);
        expect(h.saves.map(save => save.token)).toEqual(["Bearer platform"]);
    });
});

describe("what the platform may send a released client", () => {
    afterEach(() => {
        jest.restoreAllMocks();
    });

    test("the sidebar arrives as an object holding its rows", async () => {
        const row = { channel_id: channelA, name: "General" };
        const h = build();
        h.bootstrap.value = { channels: [row] };
        const starting = h.shell.start();
        await settle();
        answerHistory(h, channelA, 10);
        await starting;
        await settle();

        expect(h.shell.channels.rows).toEqual([row]);
    });

    test("an event name it does not know is ignored on either topic", async () => {
        const h = await started();
        const before = { ...h.counts };

        personal(h, "something.new", { id: 1 });
        topicOf(h, channelTopic(tenant, channelA)).listeners["broadcast"]?.({ event: "something.else", payload: { id: 2 } });
        await settle();

        expect(h.counts).toEqual(before);
        expect(h.shell.state.errors).toEqual([]);
    });

    test.each(["personal", "channel"])("a resync on the %s topic reloads the sidebar and the open channel", async where => {
        const h = await started();
        const before = { ...h.counts };
        const pendingBefore = h.history[channelA]?.length ?? 0;

        if (where === "personal") {
            personal(h, "resync", {});
        }
        else {
            topicOf(h, channelTopic(tenant, channelA)).listeners["broadcast"]?.({ event: "resync", payload: {} });
        }
        await settle();

        expect(h.counts.bootstraps).toBe(before.bootstraps + 1);
        expect(h.history[channelA]?.length ?? 0).toBe(pendingBefore + 1);
        expect(h.counts.mints).toBe(before.mints);
    });

    test("after a resync reloads the open channel, it catches up from that page", async () => {
        const h = await started();
        // the join's own catch-up first
        while (h.history[channelA]?.length) {
            answerHistory(h, channelA, 10);
            await settle();
        }

        personal(h, "resync", {});
        await settle();
        answerHistory(h, channelA, 10);
        await settle();

        expect(h.history[channelA]?.length ?? 0).toBe(1);
    });

    test("a resync whose channel was left while its page was on the way catches nothing up for it", async () => {
        const h = await started();
        while (h.history[channelA]?.length) {
            answerHistory(h, channelA, 10);
            await settle();
        }

        personal(h, "resync", {});
        await settle();
        // the person moves to another channel before the reload of the first one answers
        const toB = h.shell.selectChannel(channelB);
        await settle();
        answerHistory(h, channelA, 10);
        await settle();

        expect(h.history[channelA]?.length ?? 0).toBe(0);
        while (h.history[channelB]?.length) {
            answerHistory(h, channelB, 20);
            await settle();
        }
        await toB;
    });

    test("a Realtime error it cannot classify fetches a new token, reloads the sidebar and so rechecks access", async () => {
        const h = await started();
        const before = { ...h.counts };

        topicOf(h, channelTopic(tenant, channelA)).listeners["system"]?.({ status: "error", message: "PolicyChanged: you may not read this any more" });
        await settle();

        expect(h.counts.mints).toBe(before.mints + 1);
        expect(h.counts.setAuths).toBe(before.setAuths + 1);
        expect(h.counts.bootstraps).toBe(before.bootstraps + 1);
    });

    test("that refresh is shared with a recheck already asked for", async () => {
        const h = await started();
        const before = { ...h.counts };

        personal(h, "session.recheck", {});
        topicOf(h, channelTopic(tenant, channelA)).listeners["system"]?.({ status: "error", message: "PolicyChanged: you may not read this any more" });
        await settle();

        expect(h.counts.mints).toBe(before.mints + 1);
        expect(h.counts.bootstraps).toBe(before.bootstraps + 1);
    });
});

// The push worker asks a focused window whether the channel a push is for is already on screen,
// and hides the banner when it is; the app badge counts unread mentions in the rooms the sidebar
// lists, as the platform's mention push does, so reading a mention takes it off the icon.
describe("what the page tells push", () => {
    afterEach(() => {
        jest.restoreAllMocks();
    });

    test("a channel is on screen only when it is the open one and the page is visible", async () => {
        const h = await started();
        const shell = h.shell;

        expect(shell.isChannelOnScreen(channelA)).toBe(true);
        expect(shell.isChannelOnScreen("another-channel")).toBe(false);
    });

    test("a hidden page has nothing on screen", async () => {
        const h = await started();
        h.pageDocument.visibilityState = "hidden";

        expect(h.shell.isChannelOnScreen(channelA)).toBe(false);
    });

    test("the badge is the sidebar's unread mentions, never its unread messages", async () => {
        const h = build();
        h.bootstrap.value = {
            channels: [
                { channel_id: channelA, name: "General", unread_count: 40, mention_count: 2 },
                { channel_id: "another-channel", name: "Youth", unread_count: 7, mention_count: 1 },
                { channel_id: "third-channel", name: "Staff", unread_count: 3, mention_count: 0 }
            ]
        };
        const starting = h.shell.start();
        await settle();
        answerHistory(h, channelA, 10);
        await starting;
        await settle();

        expect(h.shell.mentionBadge()).toBe(3);
    });
});

describe("a direct message's first message", () => {
    const created = "c0000009-0000-4000-8000-000000000000";
    const other = { person_alias_guid: "a0000002-0000-4000-8000-000000000000", nick_name: "Bo", last_name: "Picked", avatar_url: null };

    test("is still sent once its conversation is ready, after the person has opened another channel", async () => {
        const starts: string[][] = [];
        const h = await started({
            startDirectMessage: async people => {
                starts.push(people);
                return { code: "ok", channelGuid: created, isPending: true, message: null, personAliasGuid: null };
            }
        });

        h.shell.directMessages.choose(other);
        await h.shell.openDirectMessage();
        await h.shell.send("hello");
        await settle();

        const toB = h.shell.selectChannel(channelB);
        await settle();
        answerHistory(h, channelB, 20);
        await toB;

        personal(h, "membership.changed", { channel_id: created });
        await settle();

        expect(starts.length).toBe(1);
        expect(h.sent).toEqual([{ channelId: created, body: "hello" }]);
        expect(h.shell.state.activeChannelId).toBe(channelB);
    });

    test("a conversation the platform refuses to open keeps the draft and its text, and nothing is sent into it", async () => {
        const h = await started({
            startDirectMessage: async () => ({ code: "ok", channelGuid: created, isPending: false, message: null, personAliasGuid: null })
        });

        h.shell.directMessages.choose(other);
        await h.shell.openDirectMessage();
        const sending = h.shell.send("hello");
        await settle();

        h.history[created]?.shift()?.resolve(fakeResponse(403, { code: "42501", message: "authz.not_readable" }));
        await sending;
        await settle();

        expect(h.shell.state.activeChannelId).toBeNull();
        expect(h.shell.directMessages.draft?.body).toBe("hello");
        expect(h.shell.directMessages.draft?.status).toBe("starting");
        expect(h.sent).toEqual([]);
        await h.shell.stop();
    });

    test("a draft nothing was sent from is dropped when another channel is chosen", async () => {
        const starts: string[][] = [];
        const h = await started({
            startDirectMessage: async people => {
                starts.push(people);
                return { code: "ok", channelGuid: created, isPending: false, message: null, personAliasGuid: null };
            }
        });

        h.shell.directMessages.choose(other);
        await h.shell.openDirectMessage();
        const toB = h.shell.selectChannel(channelB);
        await settle();
        answerHistory(h, channelB, 20);
        await toB;
        await settle();

        expect(h.shell.directMessages.draft).toBeNull();
        expect(starts).toEqual([]);
        expect(h.sent).toEqual([]);
    });

    test("a refusal that arrives after the person opened another channel shows in the error toast with Rock's sentence", async () => {
        let answer!: (result: DoorResult) => void;
        const h = await started({
            startDirectMessage: () => new Promise<DoorResult>(resolve => answer = resolve)
        });

        h.shell.directMessages.choose(other);
        await h.shell.openDirectMessage();
        const sending = h.shell.send("hello");
        await settle();

        const toB = h.shell.selectChannel(channelB);
        await settle();
        answerHistory(h, channelB, 20);
        await toB;

        answer({ code: "door.target_not_eligible", channelGuid: null, isPending: false, message: "Bo can't be messaged.", personAliasGuid: other.person_alias_guid });
        await sending;
        await settle();

        expect(h.shell.state.errors.filter(e => e.text?.includes("Bo can't be messaged.")).length).toBe(1);
        expect(h.sent).toEqual([]);
    });
});

describe("the error toast", () => {
    test("dismissing one of two toasts under one code removes exactly the one dismissed", () => {
        const { shell } = build();
        const first = { code: "door.first_message_failed", severity: "failed" as const, text: "Your message to Ada could not be sent." };
        const second = { code: "door.first_message_failed", severity: "failed" as const, text: "Your message to Bo could not be sent." };
        shell.state.errors.push(first, second);

        shell.dismissError(second);

        expect(shell.state.errors).toEqual([first]);
    });
});
