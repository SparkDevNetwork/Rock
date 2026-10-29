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
import { churchTokenFromAction, createChatShell, PlatformClientLike, refusalMessage, ShellOptions } from "../../../../src/Communication/Chat/ChatShell/shell.partial";
import { channelTopic, personalTopic } from "../../../../src/Communication/Chat/ChatShell/composables/useRealtimeHub.partial";
import { rememberedChannelKey, revealCeilingMs } from "../../../../src/Communication/Chat/ChatShell/pageLoad.partial";
import { HistoryPage, MentionRow, SidebarRow, TimelineMessage, UnreadMention } from "../../../../src/Communication/Chat/ChatShell/types.partial";
import { DoorResult } from "../../../../src/Communication/Chat/ChatShell/composables/useDirectMessage.partial";
import { noKeySentence } from "../../../../src/Communication/Chat/ChatShell/composables/useSend.partial";
import { quoteKey } from "../../../../src/Communication/Chat/ChatShell/composables/useMessageActions.partial";
import { messageForError } from "../../../../src/Communication/Chat/ChatShell/errors.partial";
import { FakeRequest, limits as attachmentLimits, nextUpload } from "./attachmentFakes";

const tenant = "10000000-0000-4000-8000-000000000001";
const person = "a0000007-0000-4000-8000-000000000000";
const channelA = "c000000a-0000-4000-8000-000000000000";
const channelB = "c000000b-0000-4000-8000-000000000000";
const channelC = "c000000c-0000-4000-8000-000000000000";

// A body naming Ada, and the pieces the platform answers for it.
const mentionBody = "Hi <@a0000001-0000-4000-8000-000000000000>";
const mentionPieces = [
    { type: "text", text: "Hi " },
    { type: "mention", text: "@Ada Member", person_alias_guid: "a0000001-0000-4000-8000-000000000000" }
];

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

/**
 * The platform's limits as the token exchange answers them; the directory's page is a size no
 * client would choose, so only a size read from them is asked.
 */
const platformLimits = {
    dm_max_others: 8,
    dm_name_max_length: 100,
    body_max_length: 4000,
    history_page_size: 50,
    member_page_size: 50,
    mention_page_size: 30,
    thread_page_size: 30,
    directory_page_size: 45,
    people_search_size: 10,
    ...attachmentLimits
};

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

function build(options: {
    startDirectMessage?: (personAliasGuids: string[]) => Promise<DoorResult>,
    mentionViews?: ShellOptions["mentionViews"],
    joinChannel?: ShellOptions["joinChannel"],
    leaveChannel?: ShellOptions["leaveChannel"],
    renameConversation?: ShellOptions["renameConversation"],
    getMemberRights?: ShellOptions["getMemberRights"],
    addMembers?: ShellOptions["addMembers"],
    removeMember?: ShellOptions["removeMember"],
    isChatAdministrator?: boolean,
    savePersonSetting?: ShellOptions["savePersonSetting"],
    areSharedChannelsFilteredByCampus?: boolean,
    push?: ShellOptions["push"],
    pushSettings?: Record<string, unknown>,
    personAliasGuid?: string,
    stored?: Record<string, string>,
    // the channel a link names; channel A unless a test asks for a first visit with null
    linkedChannelId?: string | null
} = {}): {
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
    sent: Array<{ channelId: string, body: string, args: Record<string, unknown> }>,
    tokenPerson: { value: string | null },
    reloads: { count: number },
    heldSends: { value: Deferred<void>[] | null },
    heldBootstraps: { value: Deferred<Response>[] | null },
    heldFacts: Record<string, Deferred<Response>>,
    channelReads: string[],
    channelFacts: Record<string, unknown>,
    directoryCalls: Array<Record<string, unknown>>,
    sendRefusals: { value: Array<{ status: number, body: unknown }> },
    personCalls: Array<Record<string, unknown>>,
    personAnswer: { value: { status: number, body: unknown } },
    foundDm: { value: string | null },
    historyCalls: Array<Record<string, unknown>>,
    prefsCalls: Array<Record<string, unknown>>,
    prefsAnswer: { value: { status: number, body: unknown } },
    deviceCalls: Array<{ name: string, body: Record<string, unknown> }>,
    emojiCalls: Array<Record<string, unknown>>,
    deviceAnswer: { value: { status: number, body: unknown } },
    exchangeSettings: { value: Record<string, unknown> },
    heldExchange: { value: Deferred<void> | null },
    memberCalls: Array<Record<string, unknown>>,
    begins: Array<Record<string, unknown>>,
    uploads: FakeRequest[]
} {
    // Every attachment begun, and every upload to Storage, each answered by the test.
    const begins: Array<Record<string, unknown>> = [];
    const uploads: FakeRequest[] = [];
    const topics: LiveTopic[] = [];
    const counts = { mints: 0, clients: 0, setAuths: 0, bootstraps: 0 };
    const mintGate = { value: "ok" };
    const bootstrap: { value: unknown } = { value: [] };
    const history: Record<string, Deferred<Response>[]> = {};
    const saves: Array<{ channelId: string, messageId: number, answer: Deferred<Response>, token?: string | null }> = [];
    const joined: string[] = [];
    const removed: string[] = [];
    const stored: Record<string, string> = options.stored ?? {};
    const sent: Array<{ channelId: string, body: string, args: Record<string, unknown> }> = [];
    // The person the platform token names; null hands out a token that is not a JWT.
    const tokenPerson: { value: string | null } = { value: null };
    const reloads = { count: 0 };
    // Set to a list, each send waits for the test to answer it, in the order they were sent.
    const heldSends: { value: Deferred<void>[] | null } = { value: null };
    // Set to a list, each sidebar read waits for the test to answer it, in the order they were asked.
    const heldBootstraps: { value: Deferred<Response>[] | null } = { value: null };
    // A room listed here has its chat_get_channel answer wait for the test.
    const heldFacts: Record<string, Deferred<Response>> = {};
    // Each room's chat_get_channel answer by id; a room not listed answers as one the reader is in.
    const channelReads: string[] = [];
    const channelFacts: Record<string, unknown> = {};
    const directoryCalls: Array<Record<string, unknown>> = [];
    // Refusals the next sends answer, in order, before the sends that post.
    const sendRefusals: { value: Array<{ status: number, body: unknown }> } = { value: [] };
    // What chat_get_person answers next, and every call it was asked.
    const personCalls: Array<Record<string, unknown>> = [];
    const personAnswer: { value: { status: number, body: unknown } } = { value: { status: 200, body: null } };
    // The conversation chat_find_dm answers for any people, none by default; and every history read asked.
    const foundDm: { value: string | null } = { value: null };
    const historyCalls: Array<Record<string, unknown>> = [];
    // Every chat_set_prefs asked, and what it answers next.
    const prefsCalls: Array<Record<string, unknown>> = [];
    const prefsAnswer: { value: { status: number, body: unknown } } = { value: { status: 200, body: { channel: null, me: {} } } };
    // Every device register and unregister asked, and what an unregister answers.
    const deviceCalls: Array<{ name: string, body: Record<string, unknown> }> = [];
    const emojiCalls: Array<Record<string, unknown>> = [];
    const deviceAnswer: { value: { status: number, body: unknown } } = { value: { status: 200, body: {} } };
    const exchangeSettings = { value: { limits: platformLimits, ...(options.pushSettings ? { push: options.pushSettings } : {}) } as Record<string, unknown> };
    const heldExchange: { value: Deferred<void> | null } = { value: null };
    // Every roster page asked; each answers with nobody.
    const memberCalls: Array<Record<string, unknown>> = [];
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
        session: { gate: "ok", projectUrl: "http://platform", publishableKey: "k", tenantId: tenant, personAliasGuid: options.personAliasGuid ?? person, isChatAdministrator: options.isChatAdministrator },
        linkedChannelId: options.linkedChannelId === undefined ? channelA : options.linkedChannelId,
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
                await heldExchange.value?.promise;
                return fakeResponse(200, { access_token: tokenFor(tokenPerson.value), expires_in: 300, settings: exchangeSettings.value });
            }
            const body = JSON.parse(init.body as string);
            if (url.endsWith("/rest/v1/rpc/chat_attachment_begin")) {
                begins.push(body);
                return fakeResponse(200, (body.p_files as unknown[]).map(() => nextUpload()));
            }
            if (url.endsWith("/rest/v1/rpc/chat_register_device")) {
                deviceCalls.push({ name: "chat_register_device", body });
                return fakeResponse(200, {});
            }
            if (url.endsWith("/rest/v1/rpc/chat_unregister_device")) {
                deviceCalls.push({ name: "chat_unregister_device", body });
                return fakeResponse(deviceAnswer.value.status, deviceAnswer.value.body);
            }
            // Every platform call is a POST through the shell's one call path.
            if (url.endsWith("/rest/v1/rpc/chat_get_bootstrap")) {
                counts.bootstraps++;
                // an error here is a fetch that never reached the platform, as when offline
                if (bootstrap.value instanceof Error) {
                    throw bootstrap.value;
                }
                if (heldBootstraps.value) {
                    const answer = deferred<Response>();
                    heldBootstraps.value.push(answer);
                    return answer.promise;
                }
                return fakeResponse(200, bootstrap.value);
            }
            if (url.endsWith("/rest/v1/rpc/chat_get_history")) {
                historyCalls.push(body);
                const pending = deferred<Response>();
                (history[body.p_channel_id as string] ??= []).push(pending);
                return pending.promise;
            }
            if (url.endsWith("/rest/v1/rpc/chat_find_dm")) {
                return fakeResponse(200, { channel_id: foundDm.value });
            }
            if (url.endsWith("/rest/v1/rpc/chat_get_channel")) {
                channelReads.push(body.p_channel_id);
                if (heldFacts[body.p_channel_id]) {
                    return heldFacts[body.p_channel_id].promise;
                }
                return fakeResponse(200, channelFacts[body.p_channel_id] ?? memberFacts(body.p_channel_id));
            }
            if (url.endsWith("/rest/v1/rpc/chat_list_members")) {
                memberCalls.push(body);
                return fakeResponse(200, { members: [] });
            }
            if (url.endsWith("/rest/v1/rpc/chat_list_directory")) {
                directoryCalls.push(body);
                return fakeResponse(200, { channels: [], total: 0 });
            }
            if (url.endsWith("/rest/v1/rpc/chat_set_prefs")) {
                prefsCalls.push(body);
                return fakeResponse(prefsAnswer.value.status, prefsAnswer.value.body);
            }
            if (url.endsWith("/rest/v1/rpc/chat_list_emojis")) {
                emojiCalls.push(body);
                return fakeResponse(200, { emojis: [] });
            }
            if (url.endsWith("/rest/v1/rpc/chat_get_person")) {
                personCalls.push(body);
                return fakeResponse(personAnswer.value.status, personAnswer.value.body);
            }
            if (url.endsWith("/rest/v1/rpc/chat_send_message")) {
                sent.push({ channelId: body.p_channel_id, body: body.p_body, args: body });
                const refusal = sendRefusals.value.shift();
                if (refusal) {
                    return fakeResponse(refusal.status, refusal.body);
                }
                const id = 900 + sent.length;
                if (heldSends.value) {
                    const answer = deferred<void>();
                    heldSends.value.push(answer);
                    await answer.promise;
                }
                // chat_send_message's answer: a body naming Ada carries her mention piece, as the
                // platform builds it; a person's send never carries blocks
                return fakeResponse(200, {
                    id,
                    created_at: "2026-10-06T10:00:00Z",
                    notice: null,
                    pieces: body.p_body === mentionBody ? mentionPieces : null,
                    blocks: null
                });
            }
            const answer = deferred<Response>();
            const token = (init.headers as Record<string, string> | undefined)?.["Authorization"] ?? null;
            saves.push({ channelId: body.p_channel_id, messageId: body.p_message_id, answer, token });
            return answer.promise;
        },
        storage: {
            get length(): number {
                return Object.keys(stored).length;
            },
            key: index => Object.keys(stored)[index] ?? null,
            getItem: k => stored[k] ?? null,
            setItem: (k, v) => stored[k] = v,
            removeItem: k => delete stored[k]
        },
        pageTargets: {
            document: pageDocument,
            window: {
                addEventListener: (type, listener) => (windowListeners[type] ??= []).push(listener),
                removeEventListener: () => undefined
            }
        },
        mark: () => undefined,
        startDirectMessage: options.startDirectMessage,
        joinChannel: options.joinChannel,
        leaveChannel: options.leaveChannel,
        renameConversation: options.renameConversation,
        getMemberRights: options.getMemberRights,
        addMembers: options.addMembers,
        removeMember: options.removeMember,
        savePersonSetting: options.savePersonSetting,
        push: options.push,
        areSharedChannelsFilteredByCampus: options.areSharedChannelsFilteredByCampus,
        mentionViews: options.mentionViews,
        reloadPage: () => {
            reloads.count++;
        },
        newUploadRequest: () => {
            const request = new FakeRequest();
            uploads.push(request);
            return request as unknown as XMLHttpRequest;
        }
    });

    return {
        shell, history, saves, joined, removed, stored, topics, counts, mintGate, bootstrap, pageDocument, sent, tokenPerson, reloads, heldSends, heldBootstraps, heldFacts,
        channelReads, channelFacts, directoryCalls, sendRefusals, personCalls, personAnswer, foundDm, historyCalls, prefsCalls, prefsAnswer, deviceCalls, emojiCalls, deviceAnswer, exchangeSettings, heldExchange, memberCalls, begins, uploads,
        fireBlur: () => (windowListeners["blur"] ?? []).forEach(l => l())
    };
}

/** chat_get_channel's answer for a room the reader is in, in its exact shape. */
function memberFacts(channelId: string, extra: Record<string, unknown> = {}): Record<string, unknown> {
    return {
        channel_id: channelId,
        name: "Room",
        icon_url: null,
        channel_type: "shared",
        is_member: true,
        can_post: true,
        can_post_code: null,
        can_post_reason: null,
        max_length: 4000,
        can_leave: true,
        description: null,
        group_type_name: "Small Group",
        campus_id: null,
        is_public: false,
        member_count: 12,
        leaders: [],
        faces: [],
        people: [],
        ...extra
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
        // the remembered channel, and its look beside it
        expect(h.stored[rememberedChannelKey(tenant, person)]).toBe(channelC);
        expect(Object.keys(h.stored).sort()).toEqual([rememberedChannelKey(tenant, person), `${rememberedChannelKey(tenant, person)}:kind`]);
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

    test("a message seen while the open's catch-up is still out is saved on leaving", async () => {
        const h = build();
        const starting = h.shell.start();
        await settle();
        answerHistory(h, channelA, 10);
        await starting;
        await settle();

        // B's topic joins while its first page is read, so the page is followed by a catch-up,
        // and the page is on screen, its message seen, before that catch-up answers.
        const toB = h.shell.selectChannel(channelB);
        await settle();
        topicOf(h, channelTopic(tenant, channelB)).status?.("SUBSCRIBED");
        await settle();
        answerHistory(h, channelB, 20);
        await settle();
        expect(h.history[channelB]?.length).toBe(1);
        h.shell.seen(20);
        h.history[channelB]!.shift()!.resolve(fakeResponse(200, { messages: [], read_cursor: 19, unread_count: 0, has_more: false }));
        await toB;
        await settle();
        const before = h.saves.length;

        void h.shell.selectChannel(channelA);
        await settle();

        expect(h.saves.slice(before).map(s => `${s.channelId}:${s.messageId}`)).toEqual([`${channelB}:20`]);
    });

    test("a message seen in a channel shown again from what is held is saved on leaving, though its page is still on the way", async () => {
        const h = build();
        const starting = h.shell.start();
        await settle();
        answerHistory(h, channelA, 10);
        await starting;
        await settle();

        const toB = h.shell.selectChannel(channelB);
        await settle();
        answerHistory(h, channelB, 20);
        await toB;
        await settle();
        void h.shell.selectChannel(channelA);
        await settle();
        while (h.history[channelA]?.length) {
            answerHistory(h, channelA, 10);
            await settle();
        }

        // Back on B, its held messages are on screen at once and one is seen before its page answers.
        const backToB = h.shell.selectChannel(channelB);
        await settle();
        h.shell.seen(21);
        answerHistory(h, channelB, 21);
        await backToB;
        await settle();
        const before = h.saves.length;

        h.fireBlur();
        await settle();

        expect(h.saves.slice(before).map(s => `${s.channelId}:${s.messageId}`)).toEqual([`${channelB}:21`]);
    });

    test("Mark as read saves the open channel's position at the message named, without waiting for the page to lose focus", async () => {
        const h = build();
        const starting = h.shell.start();
        await settle();
        answerHistory(h, channelA, 10);
        await starting;
        const before = h.saves.length;

        h.shell.markRead(10);
        await settle();

        expect(h.saves.slice(before).map(s => `${s.channelId}:${s.messageId}`)).toEqual([`${channelA}:10`]);
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

// The block draws a loading state, never "Choose a conversation.", until the page load has chosen
// the first conversation or found none it can open.
describe("the page load", () => {
    test("is opening from the start, through a first visit's sidebar read, until the channel it names opens", async () => {
        const h = build({ linkedChannelId: null });
        h.heldBootstraps.value = [];
        expect(h.shell.state.isOpening).toBe(true);

        const starting = h.shell.start();
        await settle();
        expect(h.shell.state.phase).toBe("ready");
        expect(h.heldBootstraps.value).toHaveLength(1);
        expect(h.shell.state.isOpening).toBe(true);

        h.heldBootstraps.value[0].resolve(fakeResponse(200, { channels: [{ channel_id: channelB } as SidebarRow] }));
        await settle();
        answerHistory(h, channelB, 20);
        await starting;

        expect(h.shell.state.activeChannelId).toBe(channelB);
        expect(h.shell.state.isOpening).toBe(false);
    });

    test("a first sidebar that fails leaves the list failed, not loading, and the page no longer opening", async () => {
        const h = build({ linkedChannelId: null });
        h.bootstrap.value = new Error("offline");

        await h.shell.start();

        expect(h.shell.channels.loadState).toBe("failed");
        expect(h.shell.state.isOpening).toBe(false);
    });

    test("a Try again that loads the first sidebar goes on with the page load, opening what it would have", async () => {
        const h = build({ linkedChannelId: null });
        h.bootstrap.value = new Error("offline");
        await h.shell.start();

        h.bootstrap.value = { channels: [{ channel_id: channelB } as SidebarRow] };
        h.shell.channels.retryLoad();
        await settle();
        expect(h.shell.state.isOpening).toBe(true);

        answerHistory(h, channelB, 20);
        await settle();

        expect(h.shell.channels.loadState).toBe("loaded");
        expect(h.shell.state.activeChannelId).toBe(channelB);
        expect(h.shell.state.isOpening).toBe(false);
    });

    test("a Try again that fails again leaves the list failed and the page no longer opening", async () => {
        const h = build({ linkedChannelId: null });
        h.bootstrap.value = new Error("offline");
        await h.shell.start();

        h.shell.channels.retryLoad();
        await settle();

        expect(h.shell.channels.loadState).toBe("failed");
        expect(h.shell.state.isOpening).toBe(false);
    });

    test("a first visit's sidebar read overtaken by a newer one opens from the newer one's rows", async () => {
        const h = build({ linkedChannelId: null });
        h.heldBootstraps.value = [];
        const starting = h.shell.start();
        await settle();
        expect(h.heldBootstraps.value).toHaveLength(1);

        personal(h, "membership.changed", { channel_id: channelB });
        await settle();
        // the newer read is asked once the one out answers
        expect(h.heldBootstraps.value).toHaveLength(1);

        h.heldBootstraps.value[0].resolve(fakeResponse(200, { channels: [] }));
        await settle();
        expect(h.shell.state.isOpening).toBe(true);
        expect(h.heldBootstraps.value).toHaveLength(2);

        h.heldBootstraps.value[1].resolve(fakeResponse(200, { channels: [{ channel_id: channelB } as SidebarRow] }));
        await settle();
        answerHistory(h, channelB, 20);
        await starting;

        expect(h.shell.state.activeChannelId).toBe(channelB);
        expect(h.shell.state.isOpening).toBe(false);
    });

    test("a first sidebar that fails is told once, by the list's own line, not also by the error toast", async () => {
        const h = build({ linkedChannelId: null });
        h.bootstrap.value = new Error("offline");

        await h.shell.start();

        expect(h.shell.channels.loadState).toBe("failed");
        expect(h.shell.state.errors).toEqual([]);
    });

    test("a reload that fails once the list is drawn is still told by the error toast, as nothing else shows it", async () => {
        const h = build({ linkedChannelId: null });
        await h.shell.start();

        h.bootstrap.value = new Error("offline");
        personal(h, "membership.changed", { channel_id: channelB });
        await settle();

        expect(h.shell.channels.loadState).toBe("loaded");
        expect(h.shell.state.errors).toHaveLength(1);
    });

    test("a reload in flight after the first sidebar never takes the list back to loading", async () => {
        const h = build({ linkedChannelId: null });
        await h.shell.start();
        expect(h.shell.channels.loadState).toBe("loaded");

        h.heldBootstraps.value = [];
        personal(h, "membership.changed", { channel_id: channelB });
        await settle();

        expect(h.heldBootstraps.value).toHaveLength(1);
        expect(h.shell.channels.loadState).toBe("loaded");
    });

    test("stops opening when the sidebar names no conversation to open", async () => {
        const h = build({ linkedChannelId: null });

        await h.shell.start();

        expect(h.shell.state.activeChannelId).toBeNull();
        expect(h.shell.state.isOpening).toBe(false);
    });
});

// The first screen is revealed once, when the sidebar, the opening room's first page and its facts
// have all answered; the shell owns the answers, the reveal owns the rule (see the page reveal).
describe("the page load's reveal", () => {
    afterEach(() => {
        jest.useRealTimers();
    });

    test("shows nothing revealed while the opening room's first page is out, and everything when it lands", async () => {
        const h = build();
        const starting = h.shell.start();
        await settle();

        expect(h.shell.channels.loadState).toBe("loaded");
        expect(h.shell.reveal.state.isSidebarRevealed).toBe(false);
        expect(h.shell.reveal.state.isMainRevealed).toBe(false);

        answerHistory(h, channelA, 10);
        await starting;

        expect(h.shell.reveal.state.isSidebarRevealed).toBe(true);
        expect(h.shell.reveal.state.isMainRevealed).toBe(true);
    });

    test("at the ceiling reveals the sidebar that arrived and the column too, which draws its own loading state until its first page lands", async () => {
        jest.useFakeTimers({ doNotFake: ["queueMicrotask", "nextTick"] });
        const h = build();
        const starting = h.shell.start();
        await settle();

        jest.advanceTimersByTime(revealCeilingMs);

        expect(h.shell.reveal.state.isSidebarRevealed).toBe(true);
        expect(h.shell.reveal.state.isMainRevealed).toBe(true);

        answerHistory(h, channelA, 10);
        await starting;

        expect(h.shell.reveal.state.isMainRevealed).toBe(true);
    });

    test("a first visit with nothing to open reveals the column", async () => {
        const h = build({ linkedChannelId: null });
        h.bootstrap.value = { channels: [] };

        await h.shell.start();

        expect(h.shell.reveal.state.isSidebarRevealed).toBe(true);
        expect(h.shell.reveal.state.isMainRevealed).toBe(true);
    });

    test("a first page that fails reveals at once, the sidebar that arrived with it", async () => {
        const h = build();
        const starting = h.shell.start();
        await settle();

        h.history[channelA][0].resolve(fakeResponse(500, { message: "down" }));
        await starting;

        expect(h.shell.reveal.state.isSidebarRevealed).toBe(true);
        expect(h.shell.reveal.state.isMainRevealed).toBe(true);
    });

    test("a remembered room the person can no longer read falls back to the first row without revealing the column empty", async () => {
        const key = rememberedChannelKey(tenant, person);
        const h = build({ linkedChannelId: null, stored: { [key]: channelB } });
        h.bootstrap.value = { channels: [{ channel_id: channelA } as SidebarRow] };
        const starting = h.shell.start();
        await settle();

        h.history[channelB][0].resolve(fakeResponse(403, { code: "42501", message: "authz.not_readable" }));
        await settle();

        // The fallback room is what the first screen shows, so nothing is revealed before it answers.
        expect(h.shell.state.activeChannelId).toBe(channelA);
        expect(h.shell.reveal.state.isMainRevealed).toBe(false);
        expect(h.shell.reveal.state.isReleased).toBe(false);

        answerHistory(h, channelA, 10);
        await starting;
        await settle();

        expect(h.shell.reveal.state.isMainRevealed).toBe(true);
        expect(h.shell.reveal.state.isSidebarRevealed).toBe(true);
    });

    test("a remembered room that is refused with no row to fall back to reveals the column", async () => {
        const key = rememberedChannelKey(tenant, person);
        const h = build({ linkedChannelId: null, stored: { [key]: channelB } });
        const starting = h.shell.start();
        await settle();

        h.history[channelB][0].resolve(fakeResponse(403, { code: "42501", message: "authz.not_readable" }));
        await starting;
        await settle();

        expect(h.shell.reveal.state.isMainRevealed).toBe(true);
    });

    test("a room the person clicks mid-load answers the column's history and facts, not the superseded page load", async () => {
        const h = build();
        const starting = h.shell.start();
        await settle();

        const toB = h.shell.selectChannel(channelB);
        await settle();
        answerHistory(h, channelA, 10);
        await starting;
        await settle();

        // The load's own room has answered but was superseded: the column still waits for B.
        expect(h.shell.reveal.state.isMainRevealed).toBe(false);
        expect(h.shell.reveal.state.isReleased).toBe(false);

        answerHistory(h, channelB, 20);
        await toB;
        await settle();

        expect(h.shell.reveal.state.isMainRevealed).toBe(true);
    });

    test("a room the person clicks mid-load after the load's room answered waits for its own facts", async () => {
        const h = build();
        const starting = h.shell.start();
        await settle();

        // The load's room has answered its facts; the clicked room's are still out.
        h.heldFacts[channelB] = deferred<Response>();
        const toB = h.shell.selectChannel(channelB);
        await settle();
        answerHistory(h, channelB, 20);
        await settle();

        expect(h.shell.reveal.state.isMainRevealed).toBe(false);

        h.heldFacts[channelB].resolve(fakeResponse(200, memberFacts(channelB)));
        answerHistory(h, channelA, 10);
        await Promise.all([toB, starting]);
        await settle();

        expect(h.shell.reveal.state.isMainRevealed).toBe(true);
    });

    test("a refused remembered room's facts do not stand for the fallback room's", async () => {
        const key = rememberedChannelKey(tenant, person);
        const h = build({ linkedChannelId: null, stored: { [key]: channelB } });
        h.bootstrap.value = { channels: [{ channel_id: channelA } as SidebarRow] };
        h.heldFacts[channelA] = deferred<Response>();
        const starting = h.shell.start();
        await settle();

        h.history[channelB][0].resolve(fakeResponse(403, { code: "42501", message: "authz.not_readable" }));
        await settle();
        answerHistory(h, channelA, 10);
        await settle();

        // The fallback's first page is in, its facts are not: the header would pop in after the reveal.
        expect(h.shell.reveal.state.isMainRevealed).toBe(false);

        h.heldFacts[channelA].resolve(fakeResponse(200, memberFacts(channelA)));
        await starting;
        await settle();

        expect(h.shell.reveal.state.isMainRevealed).toBe(true);
    });

    test("a room the person clicks mid-load that is refused reveals the column at once", async () => {
        const h = build();
        const starting = h.shell.start();
        await settle();

        const toB = h.shell.selectChannel(channelB);
        await settle();
        answerHistory(h, channelA, 10);
        await starting;
        h.history[channelB][0].resolve(fakeResponse(403, { code: "42501", message: "authz.not_readable" }));
        await toB;
        await settle();

        expect(h.shell.reveal.state.isMainRevealed).toBe(true);
    });

    test("a sidebar that fails reveals the list at once, with its Try again line", async () => {
        const h = build({ linkedChannelId: null });
        h.bootstrap.value = new Error("offline");

        await h.shell.start();

        expect(h.shell.channels.loadState).toBe("failed");
        expect(h.shell.reveal.state.isSidebarRevealed).toBe(true);
    });

    test("a sidebar that fails releases the page at once, without waiting for a face still decoding", async () => {
        const h = build({ linkedChannelId: null });
        h.bootstrap.value = new Error("offline");
        h.shell.reveal.holdFace(new Promise(() => undefined));

        await h.shell.start();

        expect(h.shell.reveal.state.isReleased).toBe(true);
        expect(h.shell.reveal.state.isSidebarRevealed).toBe(true);
    });

    test("draws the look of the room opened last, a channel's on a first visit", async () => {
        const key = rememberedChannelKey(tenant, person);

        expect(build().shell.reveal.state.kind).toBe("channel");
        expect(build({ stored: { [`${key}:kind`]: "direct" } }).shell.reveal.state.kind).toBe("direct");
    });

    test("remembers a direct message's look beside its id once its facts are in", async () => {
        const h = build();
        h.channelFacts[channelA] = memberFacts(channelA, { channel_type: "dm" });
        const starting = h.shell.start();
        await settle();
        answerHistory(h, channelA, 10);
        await starting;
        await settle();

        const key = rememberedChannelKey(tenant, person);
        expect(h.stored[key]).toBe(channelA);
        expect(h.stored[`${key}:kind`]).toBe("direct");
    });

    test("stopped mid-load reveals nothing later and leaves no reveal timer", async () => {
        jest.useFakeTimers({ doNotFake: ["queueMicrotask", "nextTick"] });
        const h = build();
        const starting = h.shell.start();
        await settle();

        await h.shell.stop();
        answerHistory(h, channelA, 10);
        await starting;
        await settle();
        jest.advanceTimersByTime(revealCeilingMs * 2);

        expect(h.shell.reveal.state.isSidebarRevealed).toBe(false);
        expect(h.shell.reveal.state.isMainRevealed).toBe(false);
        expect(h.shell.reveal.state.isSkeletonShown).toBe(false);
    });
});

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

    test("a resync forgets the @ picker's held people, since who is in each room may have changed", async () => {
        const h = await started();
        const forget = jest.spyOn(h.shell.mentions, "forgetPeople");

        personal(h, "resync", {});

        expect(forget).toHaveBeenCalledTimes(1);
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

// While the connection is lost the banner says so, and a call that could not reach the platform
// for the same reason is part of that outage, not a failure of its own worth a toast.
describe("an outage", () => {
    test("a call that cannot reach the platform while the banner shows the outage adds no toast, during it or after", async () => {
        const h = await started();
        h.bootstrap.value = new TypeError("Failed to fetch");
        const open = topicOf(h, channelTopic(tenant, channelA));

        // the open topic drops, and a sidebar read asked meanwhile cannot reach the platform
        open.status?.("CHANNEL_ERROR", new Error("channel error: connection lost"));
        await settle();
        const before = h.counts.bootstraps;
        personal(h, "resync", {});
        await settle();

        expect(h.shell.state.connection).not.toBeNull();
        expect(h.counts.bootstraps).toBe(before + 1);
        expect(h.shell.state.errors).toEqual([]);

        h.bootstrap.value = [];
        open.status?.("SUBSCRIBED");
        await settle();

        expect(h.shell.state.connection).toBeNull();
        expect(h.shell.state.errors).toEqual([]);
    });

    test("one that failed just before the drop was noticed is taken back once the banner explains it", async () => {
        const h = await started();
        h.bootstrap.value = new TypeError("Failed to fetch");
        personal(h, "resync", {});
        await settle();
        expect(h.shell.state.errors.map(e => e.code)).toEqual(["rpc.transport"]);

        topicOf(h, channelTopic(tenant, channelA)).status?.("CHANNEL_ERROR");
        await settle();

        expect(h.shell.state.errors).toEqual([]);
    });

    test("with the connection healthy, a call that cannot reach the platform still toasts", async () => {
        const h = await started();
        h.bootstrap.value = new TypeError("Failed to fetch");

        personal(h, "resync", {});
        await settle();

        expect(h.shell.state.errors.map(e => e.code)).toEqual(["rpc.transport"]);
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

    /** The panes the frame says are out of view, which only the frame can see. */
    function hiddenPanesOf(shell: ChatShell): { conversation: boolean; thread: boolean } {
        return (shell.state as unknown as { hiddenPanes: { conversation: boolean; thread: boolean } }).hiddenPanes;
    }

    // A reply kept to its thread is on screen only while its thread is open in the drawer, so the
    // worker asks by the reply's root.
    type ThreadScreenShell = { isThreadOnScreen: (rootId: number) => boolean };

    test("a thread is on screen only while its root is open in the drawer and the page is visible", async () => {
        const h = await started();
        const shell = h.shell as unknown as ThreadScreenShell;

        expect(shell.isThreadOnScreen(40)).toBe(false);

        void h.shell.threads.openThread(channelA, 40);
        await settle();

        expect(shell.isThreadOnScreen(40)).toBe(true);
        expect(shell.isThreadOnScreen(41)).toBe(false);

        h.pageDocument.visibilityState = "hidden";
        expect(shell.isThreadOnScreen(40)).toBe(false);

        h.pageDocument.visibilityState = "visible";
        void h.shell.threads.closeThread();
        await settle();

        expect(shell.isThreadOnScreen(40)).toBe(false);
    });

    test("a thread open behind another pane, as a phone's list over it, is not on screen", async () => {
        const h = await started();
        const shell = h.shell as unknown as ThreadScreenShell;

        void h.shell.threads.openThread(channelA, 40);
        await settle();
        expect(shell.isThreadOnScreen(40)).toBe(true);

        // the frame says the pane that draws the thread is out of view
        hiddenPanesOf(h.shell).thread = true;
        expect(shell.isThreadOnScreen(40)).toBe(false);

        hiddenPanesOf(h.shell).thread = false;
        expect(shell.isThreadOnScreen(40)).toBe(true);
    });

    test("the open channel behind another pane, as a phone's list or thread over it, is not on screen", async () => {
        const h = await started();
        expect(h.shell.isChannelOnScreen(channelA)).toBe(true);

        // the frame says the pane that draws the conversation is out of view
        hiddenPanesOf(h.shell).conversation = true;
        expect(h.shell.isChannelOnScreen(channelA)).toBe(false);

        hiddenPanesOf(h.shell).conversation = false;
        expect(h.shell.isChannelOnScreen(channelA)).toBe(true);
    });

    /** A sidebar row of the given kind, read and named nowhere unless the cell says otherwise. */
    function badgeRow(channelId: string, roomKind: "channel" | "person" | "group", extra: Partial<SidebarRow> = {}): Partial<SidebarRow> {
        return { channel_id: channelId, name: channelId, room_kind: roomKind, is_member: true, is_unread: false, unread_count: 0, mention_count: 0, all_mention_count: 0, ...extra };
    }

    async function badgeOf(rows: Partial<SidebarRow>[]): Promise<number> {
        const h = build();
        h.bootstrap.value = { channels: rows };
        const starting = h.shell.start();
        await settle();
        answerHistory(h, channelA, 10);
        await starting;
        await settle();
        return h.shell.appBadge();
    }

    // What waits for the person: a conversation between people is addressed to them, so it counts
    // once however many messages it holds, as its dot shows; a channel counts only what named them.
    test("the badge counts each unread conversation between people once and each channel's mentions by name", async () => {
        const badge = await badgeOf([
            badgeRow(channelA, "channel"),
            badgeRow("youth", "channel", { is_unread: true, unread_count: 7, mention_count: 2 }),
            badgeRow("staff", "channel", { is_unread: true, unread_count: 3 }),
            badgeRow("ben", "person", { is_unread: true, unread_count: 5 }),
            badgeRow("trail-crew", "group", { is_unread: true, unread_count: 2, mention_count: 1 }),
            badgeRow("cara", "person", { unread_count: 0 })
        ]);

        expect(badge).toBe(4);
    });

    // A mention push's badge counts mentions by name, so the icon does too, or the two would
    // disagree; @all raises only the sidebar's own badge.
    test("the badge leaves out the @all mentions the sidebar's rows count", async () => {
        const badge = await badgeOf([
            badgeRow(channelA, "channel"),
            badgeRow("youth", "channel", { is_unread: true, mention_count: 2, all_mention_count: 4 }),
            badgeRow("staff", "channel", { is_unread: true, all_mention_count: 1 })
        ]);

        expect(badge).toBe(2);
    });

    test("the badge leaves out the conversation on screen, which is being read, and a room the person has not joined", async () => {
        const badge = await badgeOf([
            badgeRow(channelA, "person", { is_unread: true, unread_count: 4 }),
            badgeRow("youth", "channel", { is_member: false, is_public: true, mention_count: 3 }),
            badgeRow("ben", "person", { is_unread: true })
        ]);

        expect(badge).toBe(1);
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

        h.shell.directMessages.begin([other]);
        await settle();
        await h.shell.directMessages.sendFirst("hello");
        await settle();

        const toB = h.shell.selectChannel(channelB);
        await settle();
        answerHistory(h, channelB, 20);
        await toB;

        personal(h, "membership.changed", { channel_id: created });
        await settle();

        expect(starts.length).toBe(1);
        expect(h.sent.map(s => ({ channelId: s.channelId, body: s.body }))).toEqual([{ channelId: created, body: "hello" }]);
        expect(h.shell.state.activeChannelId).toBe(channelB);
    });

    test("a conversation the platform refuses to open keeps the draft and its text, and nothing is sent into it", async () => {
        const h = await started({
            startDirectMessage: async () => ({ code: "ok", channelGuid: created, isPending: false, message: null, personAliasGuid: null })
        });

        h.shell.directMessages.begin([other]);
        await settle();
        const sending = h.shell.directMessages.sendFirst("hello");
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

        h.shell.directMessages.begin([other]);
        await settle();
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

        h.shell.directMessages.begin([other]);
        await settle();
        const sending = h.shell.directMessages.sendFirst("hello");
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

    // New message is a full view over the conversation, so the conversation stays open under it,
    // as it does under Browse channels, until the first message's conversation takes its place.
    test("begun over an open conversation leaves it open, looking up the people with chat_find_dm", async () => {
        const h = await started();

        h.shell.directMessages.begin([other]);
        await settle();

        expect(h.shell.state.activeChannelId).toBe(channelA);
        expect(h.shell.directMessages.lookup).toEqual({ status: "new", channelId: null, isSlow: false });
        expect(h.shell.directMessages.draft?.people).toEqual([other]);
        await h.shell.stop();
    });

    test("reads its last 16 messages through its history, oldest first, opening nothing", async () => {
        const h = await started();
        h.foundDm.value = channelB;
        const before = h.historyCalls.length;

        h.shell.directMessages.begin([other]);
        await settle();
        expect(h.historyCalls.slice(before)).toEqual([expect.objectContaining({ p_channel_id: channelB, p_limit: 16, p_before_id: null, p_after_id: null })]);
        answerHistory(h, channelB, 20);
        await settle();

        expect(h.shell.state.activeChannelId).toBe(channelA);
        expect(h.shell.directMessages.lookup).toEqual({ status: "found", channelId: channelB, isSlow: false });
        expect(h.shell.directMessages.recent?.map(m => m.id)).toEqual([20]);
        await h.shell.stop();
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

describe("the send's answer", () => {
    test("the person's confirmed message carries the pieces the send answered", async () => {
        const h = await started();

        await h.shell.send(mentionBody);
        await settle();

        const confirmed = h.shell.timelines.state(channelA).messages.find(m => m.body === mentionBody);
        expect(confirmed?.pieces).toEqual(mentionPieces);
        expect(confirmed).toHaveProperty("blocks", null);
    });
});

describe("a thread's live counts", () => {
    test("reach the conversation's timeline, so a page read before them does not take them back", async () => {
        const h = await started();
        const live = topicOf(h, channelTopic(tenant, channelA));
        // the join's catch-up finds nothing newer, and a rejoin reads the held root again
        h.history[channelA].shift()?.resolve(fakeResponse(200, { messages: [], read_cursor: 9, unread_count: 0, has_more: false }));
        await settle();
        live.status?.("SUBSCRIBED");
        await settle();
        const rejoin = h.history[channelA].shift();
        expect(rejoin).toBeDefined();

        const ben = { person_alias_guid: "a0000003-0000-4000-8000-000000000000", nick_name: "Ben", last_name: "Okafor", avatar_url: null };
        live.listeners["broadcast"]?.({
            event: "thread.updated",
            payload: { channel_id: channelA, root_id: 10, reply_count: 1, last_reply_at: "2026-09-23T10:05:00Z", participant_count: 1, repliers: [ben] }
        });
        rejoin?.resolve(fakeResponse(200, {
            messages: [{ ...page(10).messages[0], reply_count: 0, last_reply_at: null, participant_count: 0, repliers: null }],
            read_cursor: 9,
            unread_count: 1,
            has_more: false
        }));
        await settle();

        const rootHeld = h.shell.timelines.state(channelA).messages.find(m => m.id === 10);
        expect(rootHeld?.reply_count).toBe(1);
        expect(rootHeld?.repliers).toEqual([ben]);
    });
});

// The person's own message reaches the page as the send's answer and as its live copy, in either
// order. The live copy carries the send's key, so it goes straight into the timeline, takes the
// place of the row still sending and settles the send at once, the answer merging into it later.
describe("the person's own live copy while their send is out", () => {
    test("settles the send before its answer, and the answer then merges into the one message", async () => {
        const h = await started();
        h.heldSends.value = [];
        let isSent = false;
        const sending = h.shell.send("on my way").then(() => isSent = true);
        await settle();
        const key = h.sent[0].args.p_client_key as string;

        topicOf(h, channelTopic(tenant, channelA)).listeners["broadcast"]?.({
            event: "message.created",
            payload: { id: 901, channel_id: channelA, parent_id: null, shown_in_channel: false, person_alias_guid: person, message_type: "text", body: "on my way", created_at: "2026-10-06T10:00:00Z", client_key: key }
        });
        await settle();
        expect(isSent).toBe(true);
        expect(h.shell.sender.pending(channelA)).toEqual([]);
        expect(h.shell.timelines.state(channelA).messages.map(m => [m.id, m.client_key ?? null])).toEqual([[10, null], [901, key]]);

        h.heldSends.value[0].resolve();
        await sending;
        await settle();
        expect(h.shell.timelines.state(channelA).messages.map(m => [m.id, m.client_key ?? null])).toEqual([[10, null], [901, key]]);
        expect(h.shell.sender.pending(channelA)).toEqual([]);
    });

    test("a reply shown only in its thread is settled by its live copy in the open thread, before its answer", async () => {
        const h = await started();
        const opening = h.shell.threads.openThread(channelA, 10);
        await settle();
        const root = { id: 10, person_alias_guid: "a0000001-0000-4000-8000-000000000000", message_type: "text", body: "m10", created_at: "2026-09-23T10:00:00Z" };
        h.saves[h.saves.length - 1].answer.resolve(fakeResponse(200, { root, messages: [], read_cursor: null, unread_count: 0, following: true, has_more: false }));
        await opening;

        h.heldSends.value = [];
        let isSent = false;
        const sending = (h.shell as unknown as { sendReply: (rootId: number, body: string, isShownInChannel: boolean) => Promise<void> })
            .sendReply(10, "just here", false).then(() => isSent = true);
        await settle();
        const key = h.sent[0].args.p_client_key as string;

        topicOf(h, channelTopic(tenant, channelA)).listeners["broadcast"]?.({
            event: "message.created",
            payload: { id: 901, channel_id: channelA, parent_id: 10, shown_in_channel: false, person_alias_guid: person, message_type: "text", body: "just here", created_at: "2026-10-06T10:00:00Z", client_key: key }
        });
        await settle();
        expect(isSent).toBe(true);
        expect(h.shell.sender.pending(channelA)).toEqual([]);
        expect(h.shell.threads.state.thread?.replies.map(r => r.id)).toEqual([901]);

        h.heldSends.value[0].resolve();
        await sending;
        await settle();
        expect(h.shell.threads.state.thread?.replies.map(r => r.id)).toEqual([901]);
        expect(h.shell.timelines.state(channelA).messages.map(m => m.id)).toEqual([10]);
    });

    // No view keeps a reply shown only in a thread that is not open, so the shell itself hears
    // every keyed copy on the channel topic; otherwise a lost answer leaves the send out for good.
    test("a reply shown only in a thread that is not open is settled by its live copy when its answer never comes", async () => {
        const h = await started();
        h.heldSends.value = [];
        let isSent = false;
        void (h.shell as unknown as { sendReply: (rootId: number, body: string, isShownInChannel: boolean) => Promise<void> })
            .sendReply(10, "just here", false).then(() => isSent = true);
        await settle();
        const key = h.sent[0].args.p_client_key as string;

        topicOf(h, channelTopic(tenant, channelA)).listeners["broadcast"]?.({
            event: "message.created",
            payload: { id: 901, channel_id: channelA, parent_id: 10, shown_in_channel: false, person_alias_guid: person, message_type: "text", body: "just here", created_at: "2026-10-06T10:00:00Z", client_key: key }
        });
        await settle();

        expect(isSent).toBe(true);
        expect(h.shell.sender.pending(channelA)).toEqual([]);
        expect(h.shell.timelines.state(channelA).messages.map(m => m.id)).toEqual([10]);
    });

    // A thread page that arrives after its thread was left is dropped by the thread, yet the reply
    // in it is posted, so the send it carries settles all the same.
    test("a reply in a thread page dropped because the thread was left settles its send", async () => {
        const h = await started();
        h.heldSends.value = [];
        const opening = h.shell.threads.openThread(channelA, 10);
        await settle();
        let isSent = false;
        void (h.shell as unknown as { sendReply: (rootId: number, body: string, isShownInChannel: boolean) => Promise<void> })
            .sendReply(10, "just here", false).then(() => isSent = true);
        await settle();
        const key = h.sent[0].args.p_client_key as string;
        await h.shell.threads.closeThread();

        const root = { id: 10, person_alias_guid: "a0000001-0000-4000-8000-000000000000", message_type: "text", body: "m10", created_at: "2026-09-23T10:00:00Z" };
        const reply = { id: 901, parent_id: 10, shown_in_channel: false, person_alias_guid: person, message_type: "text", body: "just here", created_at: "2026-10-06T10:00:00Z", client_key: key };
        h.saves.find(s => s.channelId === undefined && s.messageId === undefined)?.answer.resolve(fakeResponse(200, { root, messages: [reply], read_cursor: null, unread_count: 0, following: true, has_more: false }));
        await opening;
        await new Promise(resolve => setTimeout(resolve, 0));
        await settle();

        expect(h.shell.threads.state.thread).toBeNull();
        expect(isSent).toBe(true);
        expect(h.shell.sender.pending(channelA)).toEqual([]);
    });
});

// A key is a uuid, and the platform or a proxy may write a uuid back in upper case; the copy is
// still the send's, in the conversation and in the open thread.
describe("a send key on the wire in upper case", () => {
    test("settles the send in the conversation and is held in lower case", async () => {
        const h = await started();
        h.heldSends.value = [];
        let isSent = false;
        void h.shell.send("on my way").then(() => isSent = true);
        await settle();
        const key = h.sent[0].args.p_client_key as string;

        topicOf(h, channelTopic(tenant, channelA)).listeners["broadcast"]?.({
            event: "message.created",
            payload: { id: 901, channel_id: channelA, parent_id: null, shown_in_channel: false, person_alias_guid: person, message_type: "text", body: "on my way", created_at: "2026-10-06T10:00:00Z", client_key: key.toUpperCase() }
        });
        await settle();

        expect(isSent).toBe(true);
        expect(h.shell.sender.pending(channelA)).toEqual([]);
        expect(h.shell.timelines.state(channelA).messages.map(m => [m.id, m.client_key ?? null])).toEqual([[10, null], [901, key]]);
    });

    test("is held in lower case in the open thread, from its live copy and from a page", async () => {
        const h = await started();
        const opening = h.shell.threads.openThread(channelA, 10);
        await settle();
        const root = { id: 10, person_alias_guid: "a0000001-0000-4000-8000-000000000000", message_type: "text", body: "m10", created_at: "2026-09-23T10:00:00Z" };
        const older = { id: 800, parent_id: 10, shown_in_channel: false, person_alias_guid: person, message_type: "text", body: "before", created_at: "2026-10-06T09:00:00Z", client_key: "ABCDEF01-0000-4000-8000-000000000000" };
        h.saves[h.saves.length - 1].answer.resolve(fakeResponse(200, { root, messages: [older], read_cursor: null, unread_count: 0, following: true, has_more: false }));
        await opening;

        h.heldSends.value = [];
        let isSent = false;
        void (h.shell as unknown as { sendReply: (rootId: number, body: string, isShownInChannel: boolean) => Promise<void> })
            .sendReply(10, "just here", false).then(() => isSent = true);
        await settle();
        const key = h.sent[0].args.p_client_key as string;

        topicOf(h, channelTopic(tenant, channelA)).listeners["broadcast"]?.({
            event: "message.created",
            payload: { id: 901, channel_id: channelA, parent_id: 10, shown_in_channel: false, person_alias_guid: person, message_type: "text", body: "just here", created_at: "2026-10-06T10:00:00Z", client_key: key.toUpperCase() }
        });
        await settle();

        expect(isSent).toBe(true);
        expect(h.shell.threads.state.thread?.replies.map(r => [r.id, r.client_key])).toEqual([[800, "abcdef01-0000-4000-8000-000000000000"], [901, key]]);
    });
});

describe("a reply in a thread", () => {
    // The shell sends a reply to the open conversation as shell.sendReply(rootId, body,
    // isShownInChannel), through the same sender and retry key as any message.
    type ReplyingShell = { sendReply: (rootId: number, body: string, isShownInChannel: boolean) => Promise<void> };

    test("sends chat_send_message with its root, whether it is also shown in the conversation, and a key", async () => {
        const h = await started();
        const shell = h.shell as unknown as ReplyingShell;

        await shell.sendReply(10, "count me in", false);
        await shell.sendReply(10, "and everyone", true);
        await settle();

        expect(h.sent.map(s => s.args)).toEqual([
            expect.objectContaining({ p_channel_id: channelA, p_body: "count me in", p_parent_id: 10, p_shown_in_channel: false, p_client_key: expect.any(String) }),
            expect.objectContaining({ p_channel_id: channelA, p_body: "and everyone", p_parent_id: 10, p_shown_in_channel: true, p_client_key: expect.any(String) })
        ]);
    });

    test("an ordinary send still names no root", async () => {
        const h = await started();

        await h.shell.send("plain");
        await settle();

        expect(h.sent[0].args.p_parent_id ?? null).toBeNull();
        expect(h.sent[0].args.p_shown_in_channel ?? false).toBe(false);
    });
});

// A quote waits under its conversation's key, quoteKey(channelId), or its thread's,
// quoteKey(channelId, rootId), until the next send there. That send names it to the platform as
// p_quoted_message_id and uses it up, so the strip leaves as the text does.
describe("a quoted send", () => {
    type ReplyingShell = { sendReply: (rootId: number, body: string, isShownInChannel: boolean) => Promise<void> };

    test("carries the conversation's quote and uses it up", async () => {
        const h = await started();
        h.shell.messageActions.startQuote(quoteKey(channelA), page(10).messages[0]);

        await h.shell.send("count me in");
        await h.shell.send("and again");
        await settle();

        expect(h.sent[0].args.p_quoted_message_id).toBe(10);
        expect(h.sent[1].args.p_quoted_message_id ?? null).toBeNull();
        expect(h.shell.messageActions.state.quotes[quoteKey(channelA)]).toBeUndefined();
    });

    test("a thread's reply carries its thread's quote and leaves the conversation's", async () => {
        const h = await started();
        const conversationQuote = page(10).messages[0];
        const threadQuote = { ...conversationQuote, id: 11 };
        h.shell.messageActions.startQuote(quoteKey(channelA), conversationQuote);
        h.shell.messageActions.startQuote(quoteKey(channelA, 10), threadQuote);

        await (h.shell as unknown as ReplyingShell).sendReply(10, "count me in", false);
        await settle();

        expect(h.sent[0].args).toEqual(expect.objectContaining({ p_parent_id: 10, p_quoted_message_id: 11 }));
        expect(h.shell.messageActions.state.quotes[quoteKey(channelA, 10)]).toBeUndefined();
        expect(h.shell.messageActions.state.quotes[quoteKey(channelA)]).toEqual(conversationQuote);
    });
});

// Without the browser's cryptographic generator a send makes no key and so no row: nothing in the
// feed would say what became of the text, so the shell says it, in the direct message's words.
describe("a send in a browser that cannot make a send key", () => {
    type ReplyingShell = { sendReply: (rootId: number, body: string, isShownInChannel: boolean) => Promise<void> };

    async function withoutCrypto(run: () => Promise<void>): Promise<void> {
        const original = Object.getOwnPropertyDescriptor(globalThis, "crypto");
        Object.defineProperty(globalThis, "crypto", { value: undefined, configurable: true, writable: true });
        try {
            await run();
        }
        finally {
            if (original) {
                Object.defineProperty(globalThis, "crypto", original);
            }
        }
    }

    test("sends nothing and says it cannot send securely, for a message and a reply", async () => {
        const h = await started();

        await withoutCrypto(async () => {
            await h.shell.send("plain");
            await (h.shell as unknown as ReplyingShell).sendReply(10, "a reply", false);
        });
        await settle();

        expect(h.sent).toEqual([]);
        expect(h.shell.sender.pending(channelA)).toEqual([]);
        expect(noKeySentence).toEqual(expect.any(String));
        expect(h.shell.state.errors).toEqual([expect.objectContaining({ text: noKeySentence })]);
    });
});

// A live copy of a channel still loading waits for its page; so does the send it settles, or its
// row would go before the message that replaces it is on screen.
describe("the person's own live copy while the channel's first page loads", () => {
    test("leaves the row sending until the page is in, which then settles it", async () => {
        const h = await started();
        h.heldSends.value = [];
        const toB = h.shell.selectChannel(channelB);
        await settle();
        let isSent = false;
        void h.shell.send("on my way").then(() => isSent = true);
        await settle();
        const key = h.sent[0].args.p_client_key as string;

        topicOf(h, channelTopic(tenant, channelB)).listeners["broadcast"]?.({
            event: "message.created",
            payload: { id: 901, channel_id: channelB, parent_id: null, shown_in_channel: false, person_alias_guid: person, message_type: "text", body: "on my way", created_at: "2026-10-06T10:00:00Z", client_key: key }
        });
        await settle();
        expect(isSent).toBe(false);
        expect(h.shell.sender.pending(channelB).map(r => r.localId)).toEqual([key]);

        answerHistory(h, channelB, 20);
        await toB;
        await settle();
        expect(isSent).toBe(true);
        expect(h.shell.sender.pending(channelB)).toEqual([]);
        expect(h.shell.timelines.state(channelB).messages.map(m => [m.id, m.client_key ?? null])).toEqual([[20, null], [901, key]]);
    });
});

// The bootstrap carries the church's chat badges, which a profile card names a person's keys by.
describe("badges", () => {
    const serving = { badge_key: "b0000001-0000-4000-8000-000000000000", name: "Serving", icon_css: "fa fa-hands-helping", bg_color: "#E8F5E9", fg_color: "#1B5E20", sort_order: 1 };
    const baptized = { badge_key: "b0000002-0000-4000-8000-000000000000", name: "Baptized", icon_css: null, bg_color: null, fg_color: null, sort_order: 2 };

    test("the bootstrap's badges are held in the store, in the order it lists them", async () => {
        const h = build();
        h.bootstrap.value = { channels: [], reactions: [], badges: [serving, baptized] };
        const starting = h.shell.start();
        await settle();
        answerHistory(h, channelA, 10);
        await starting;
        await settle();

        expect(h.shell.channels.badges).toEqual([serving, baptized]);
    });

    test("a church with no badges holds none", async () => {
        const h = build();
        h.bootstrap.value = { channels: [], reactions: [], badges: [] };
        const starting = h.shell.start();
        await settle();
        answerHistory(h, channelA, 10);
        await starting;
        await settle();

        expect(h.shell.channels.badges).toEqual([]);
    });
});

// The bootstrap carries the church's favorites; a channel topic carries everyone's reactions.
describe("reactions", () => {
    const thumbsUp = { code: "thumbsup", label: "thumbs up", emoji: "👍", image_url: null };

    test("the bootstrap's reactions are the favorites the bar, the sheet and the picker draw", async () => {
        const h = build();
        h.bootstrap.value = { channels: [], reactions: [thumbsUp] };
        const starting = h.shell.start();
        await settle();
        answerHistory(h, channelA, 10);
        await starting;
        await settle();

        expect(h.shell.reactions.favorites).toEqual([thumbsUp]);
    });

    // The picker's quick tray draws a recent from the catalog by its glyph, and its full picker
    // the categories, so the catalog is on hand before the first open rather than fetched by it.
    test("the emoji catalog is fetched once, after the first conversation is on screen", async () => {
        const h = build();
        const starting = h.shell.start();
        await settle();
        expect(h.emojiCalls).toHaveLength(0);

        answerHistory(h, channelA, 10);
        await starting;
        await settle();

        expect(h.emojiCalls).toHaveLength(1);
        expect(h.shell.reactions.state.catalog).toEqual([]);
    });

    test.each(["reaction.added", "reaction.removed"])("%s on a channel topic moves the chip of the message it names", async event => {
        const h = await started();
        const held = h.shell.timelines.state(channelA).messages.find(m => m.id === 10);
        if (held) {
            held.reactions = [{ code: "pray", count: 2, mine: false, emoji: "🙏", image_url: null, label: "folded hands" }];
        }

        topicOf(h, channelTopic(tenant, channelA)).listeners["broadcast"]?.({
            event,
            payload: { message_id: 10, channel_id: channelA, code: "pray", person_alias_guid: "a0000001-0000-4000-8000-000000000000", emoji: "🙏", image_url: null, label: "folded hands" }
        });

        expect(h.shell.timelines.state(channelA).messages.find(m => m.id === 10)?.reactions?.[0]?.count).toBe(event === "reaction.added" ? 3 : 1);
    });

    // Reactions take the timeline's hold, as every other change to a message does.
    test("one arriving while the channel's first page is on its way is applied once the page lands", async () => {
        const h = build();
        const starting = h.shell.start();
        await settle();

        topicOf(h, channelTopic(tenant, channelA)).listeners["broadcast"]?.({
            event: "reaction.added",
            payload: { message_id: 10, channel_id: channelA, code: "pray", person_alias_guid: "a0000002-0000-4000-8000-000000000000", emoji: "🙏", image_url: null, label: "folded hands" }
        });
        answerHistory(h, channelA, 10);
        await starting;
        await settle();

        expect(h.shell.timelines.state(channelA).messages.find(m => m.id === 10)?.reactions?.map(r => [r.code, r.count])).toEqual([["pray", 1]]);
    });

    // The open thread has a page of its own, so its copies do not wait for the conversation's.
    test.each([
        ["root", 10],
        ["reply", 11]
    ])("one on the open thread's %s while the conversation's first page is on its way moves the thread's chip at once, and once", async (_what, messageId) => {
        const h = build();
        const starting = h.shell.start();
        await settle();

        const opening = h.shell.threads.openThread(channelA, 10);
        await settle();
        const root = { id: 10, person_alias_guid: "a0000001-0000-4000-8000-000000000000", message_type: "text", body: "m10", created_at: "2026-09-23T10:00:00Z" };
        const reply = { id: 11, parent_id: 10, shown_in_channel: false, person_alias_guid: "a0000001-0000-4000-8000-000000000000", message_type: "text", body: "m11", created_at: "2026-09-23T10:01:00Z" };
        h.saves[h.saves.length - 1].answer.resolve(fakeResponse(200, { root, messages: [reply], read_cursor: null, unread_count: 0, following: true, has_more: false }));
        await opening;
        const threadChips = (): unknown => {
            const thread = h.shell.threads.state.thread;
            const copy = messageId === 10 ? thread?.root : thread?.replies.find(r => r.id === messageId);
            return copy?.reactions?.map(r => [r.code, r.count]);
        };

        topicOf(h, channelTopic(tenant, channelA)).listeners["broadcast"]?.({
            event: "reaction.added",
            payload: { message_id: messageId, channel_id: channelA, code: "pray", person_alias_guid: "a0000002-0000-4000-8000-000000000000", emoji: "🙏", image_url: null, label: "folded hands" }
        });
        expect(threadChips()).toEqual([["pray", 1]]);

        answerHistory(h, channelA, 10);
        await starting;
        await settle();

        expect(threadChips()).toEqual([["pray", 1]]);
    });
});

describe("a page's report after the shell stops", () => {
    test("settles no send", async () => {
        const h = await started();
        h.heldSends.value = [];
        const opening = h.shell.threads.openThread(channelA, 10);
        await settle();
        let isSent = false;
        void (h.shell as unknown as { sendReply: (rootId: number, body: string, isShownInChannel: boolean) => Promise<void> })
            .sendReply(10, "just here", false).then(() => isSent = true);
        await settle();
        const key = h.sent[0].args.p_client_key as string;
        await h.shell.threads.closeThread();

        const root = { id: 10, person_alias_guid: "a0000001-0000-4000-8000-000000000000", message_type: "text", body: "m10", created_at: "2026-09-23T10:00:00Z" };
        const reply = { id: 901, parent_id: 10, shown_in_channel: false, person_alias_guid: person, message_type: "text", body: "just here", created_at: "2026-10-06T10:00:00Z", client_key: key };
        h.saves.find(s => s.channelId === undefined && s.messageId === undefined)?.answer.resolve(fakeResponse(200, { root, messages: [reply], read_cursor: null, unread_count: 0, following: true, has_more: false }));
        await opening;
        void h.shell.stop();
        await new Promise(resolve => setTimeout(resolve, 0));
        await settle();

        expect(isSent).toBe(false);
        expect(h.shell.sender.pending(channelA).map(r => r.status)).toEqual(["sending"]);
    });
});

// The editor and the delete confirm belong to the conversation they were opened in: a switch to
// another, or to a new direct message's conversation as its first message opens it, closes both,
// and an edit's answer still out when the person left has nothing left to touch.
describe("a conversation switch with a message's editor or delete confirm open", () => {
    const created = "c0000009-0000-4000-8000-000000000000";

    test.each([
        ["opening another channel", async (h: ReturnType<typeof build>) => void h.shell.selectChannel(channelB)],
        ["a new direct message's first message opening its conversation", async (h: ReturnType<typeof build>) => {
            h.shell.directMessages.begin([{ person_alias_guid: "a0000002-0000-4000-8000-000000000000", nick_name: "Bo", last_name: "Picked", avatar_url: null }]);
            await settle();
            void h.shell.directMessages.sendFirst("hello");
        }]
    ])("%s closes the editor, drops the edit's late answer and closes the delete confirm", async (_what, leave) => {
        const h = await started({
            startDirectMessage: async () => ({ code: "ok", channelGuid: created, isPending: false, message: null, personAliasGuid: null })
        });
        const message = h.shell.timelines.state(channelA).messages[0];

        h.shell.messageActions.startEdit(channelA, message.id);
        const saving = h.shell.messageActions.saveEdit("changed");
        await settle();
        const edit = h.saves.find(s => s.channelId === undefined && s.messageId === message.id);
        expect(edit).toBeDefined();
        h.shell.messageActions.askDelete(channelA, message);

        await leave(h);
        await settle();

        expect(h.shell.messageActions.state.editing).toBeNull();
        expect(h.shell.messageActions.state.confirmingDelete).toBeNull();

        edit?.answer.resolve(fakeResponse(403, { code: "42501", message: "authz.not_author" }));
        expect(await saving).toBe(false);
        expect(h.shell.messageActions.state.editing).toBeNull();
    });
});

describe("mentions", () => {
    // The shell holds the mentions as shell.mentions, built with its own call path, the church and
    // person, the page's storage and timelines, its open, and the frame's views for landing on a
    // message and opening a thread, which the shell cannot see and the frame hands in.

    /** One entry of the bootstrap's me.unread_mentions, exactly its keys. */
    function unread(channelId: string, messageId: number): UnreadMention {
        return { channel_id: channelId, parent_id: null, message_id: messageId, created_at: "2026-10-18T09:00:00Z", in_channel: true };
    }

    /** One row of chat_list_mentions, its message as a history page carries it. */
    function mentionRow(channelId: string, id: number, parentId: number | null = null): MentionRow {
        return {
            channel_id: channelId,
            parent_id: parentId,
            message: { id, parent_id: parentId, person_alias_guid: "a0000001-0000-4000-8000-000000000000", message_type: "text", body: `<@${person}> m${id}`, created_at: "2026-10-18T09:00:00Z" } as unknown as TimelineMessage
        };
    }

    /** Starts the shell with channel A open, after a bootstrap answering what the test gives it. */
    async function startedWith(bootstrap: unknown, options: Parameters<typeof build>[0] = {}): Promise<ReturnType<typeof build>> {
        const h = build(options);
        h.bootstrap.value = bootstrap;
        const starting = h.shell.start();
        await settle();
        answerHistory(h, channelA, 10);
        await starting;
        await settle();
        return h;
    }

    test("the bootstrap's unread mentions light the header's dot", async () => {
        const h = await startedWith({ channels: [], me: { unread_mentions: [unread(channelB, 50)] } });

        expect(h.shell.mentions.state.hasDot).toBe(true);
    });

    test("channel.mention on the person's topic reaches the mentions as well as the sidebar", async () => {
        const h = await started();
        expect(h.shell.mentions.state.hasDot).toBe(false);

        personal(h, "channel.mention", { channel_id: channelB, message_id: 60, parent_id: null, in_channel: true, all: false });

        expect(h.shell.mentions.state.hasDot).toBe(true);
    });

    test("channel.mention for a message the sidebar row has already read lights nothing", async () => {
        const h = await startedWith({ channels: [{ channel_id: channelB, name: "Youth", read_cursor: 70, mention_count: 0, all_mention_count: 0 }], me: { unread_mentions: [] } });

        personal(h, "channel.mention", { channel_id: channelB, message_id: 60, parent_id: null, in_channel: true, all: false });

        expect(h.shell.mentions.state.hasDot).toBe(false);
        expect(h.shell.channels.rows.find(row => row.channel_id === channelB)?.mention_count).toBe(0);
    });

    test("a read saved past an unread mention in the conversation takes it off the dot", async () => {
        const h = await startedWith({ channels: [], me: { unread_mentions: [unread(channelA, 10)] } });
        expect(h.shell.mentions.state.hasDot).toBe(true);

        h.shell.markRead(10);
        await settle();
        const save = h.saves.filter(s => s.channelId === channelA && s.messageId === 10).pop();
        save?.answer.resolve(fakeResponse(200, { read_cursor: 10, last_message_id: 10 }));
        await settle();

        expect(save).toBeDefined();
        expect(h.shell.mentions.state.hasDot).toBe(false);
    });

    test("opening the inbox keeps the visit in the page's storage, under the church and the person", async () => {
        const h = await started();

        void h.shell.mentions.openInbox();
        await settle();

        expect(Number(h.stored[`rock-chat:mentions-visit:${tenant}:${person}`])).toBeGreaterThan(0);
    });

    test("a jump from the inbox opens the mention's conversation and lands on the message there", async () => {
        const land = jest.fn();
        const h = await started({ mentionViews: { land, openThread: jest.fn(async () => undefined) } });

        const jumping = h.shell.mentions.jump(mentionRow(channelB, 20));
        await settle();
        answerHistory(h, channelB, 20);
        await jumping;

        expect(h.shell.state.activeChannelId).toBe(channelB);
        expect(land).toHaveBeenCalledWith(20);
    });

    test("a jump to a reply opens its thread at the reply through the frame", async () => {
        const openThread = jest.fn(async () => undefined);
        const h = await started({ mentionViews: { land: jest.fn(), openThread } });

        await h.shell.mentions.jump(mentionRow(channelB, 21, 15));

        expect(openThread).toHaveBeenCalledWith(channelB, 15, 21);
    });

    test("nothing reaches the mentions once the shell stops", async () => {
        const h = await started();

        await h.shell.stop();
        h.shell.mentions.search({ channel_id: channelA, can_mention: true, can_mention_all: true, is_member: true }, "a");

        expect(h.shell.mentions.state.picker.query).toBeNull();
    });
});

// The live sidebar. A dropped and rejoined personal topic may have lost signals, so the sidebar is
// read again, after a random wait of up to five seconds shared with any reload asked meanwhile, so
// a Realtime restart that rejoins every client at once does not read every sidebar in the same
// second. A reload in flight never lays its older snapshot over what arrived while it was out:
// every personal event and save that lands meanwhile is applied at once and replayed over the
// answer, and an answer older than the newest asked for is dropped.
describe("the live sidebar", () => {
    const dmD = "d000000d-0000-4000-8000-000000000000";
    const channelE = "c000000e-0000-4000-8000-000000000000";

    /** A sidebar row in the exact shape chat_get_bootstrap answers, read up to message 10. */
    function sidebarRow(channelId: string, extra: Partial<SidebarRow> = {}): SidebarRow {
        return {
            channel_id: channelId,
            name: "Room",
            icon_url: null,
            channel_type: "shared",
            is_public: false,
            always_shown: false,
            is_member: true,
            is_favorite: false,
            notify_mode: "all",
            is_leader: false,
            can_post_announcements: false,
            can_mention_all: false,
            last_message_id: 10,
            last_message_at: "2026-10-19T09:00:00Z",
            last_message_type: "text",
            last_sender_nick_name: "Ada",
            last_sender_last_name: "Member",
            last_sender_avatar_url: null,
            last_sender_listed: true,
            last_message_preview: "old",
            last_message_is_own: false,
            last_message_posted_by_rock: false,
            read_cursor: 10,
            is_unread: false,
            mention_count: 0,
            all_mention_count: 0,
            can_mention: true,
            dm_people: null,
            dm_other_count: null,
            room_kind: "channel",
            title: extra.name ?? "Room",
            preview_line: "",
            ...extra
        };
    }

    /** The sidebar as the page loaded it, and as a reload taken before any change answers it. */
    function snapshot(): { channels: SidebarRow[], reactions: [], me: { unread_mentions: UnreadMention[] } } {
        return {
            channels: [
                sidebarRow(channelA, { read_cursor: 9 }),
                sidebarRow(channelB),
                sidebarRow(dmD, { name: null, channel_type: "dm", dm_people: [{ person_alias_guid: "ben-okafor", nick_name: "Ben", last_name: "Okafor", avatar_url: null }], dm_other_count: 1, room_kind: "person", title: "Ben Okafor" }),
                sidebarRow(channelE, { last_message_id: 12, read_cursor: 10, is_unread: true, mention_count: 1 })
            ],
            reactions: [],
            me: { unread_mentions: [{ channel_id: channelE, parent_id: null, message_id: 11, created_at: "2026-10-19T09:00:00Z", in_channel: true }] }
        };
    }

    /** Starts the shell over the snapshot with channel A open and the personal topic joined. */
    async function startedLive(options: Parameters<typeof build>[0] = {}): Promise<ReturnType<typeof build>> {
        const h = build(options);
        h.bootstrap.value = snapshot();
        const starting = h.shell.start();
        await settle();
        answerHistory(h, channelA, 10);
        await starting;
        await settle();
        topicOf(h, personalTopic(tenant, person)).status?.("SUBSCRIBED");
        topicOf(h, channelTopic(tenant, channelA)).status?.("SUBSCRIBED");
        await settle();
        return h;
    }

    function rowOf(h: ReturnType<typeof build>, channelId: string): SidebarRow | undefined {
        return h.shell.channels.rows.find(r => r.channel_id === channelId);
    }

    /** Asks the shell for a reload, as a membership change in a room not on screen does. */
    function askReload(h: ReturnType<typeof build>): void {
        personal(h, "membership.changed", { channel_id: channelC, id: "5e0000c0-0000-4000-8000-000000000000" });
    }

    function personalStatus(h: ReturnType<typeof build>, status: string): void {
        topicOf(h, personalTopic(tenant, person)).status?.(status);
    }

    test("a channel whose unread newest message was deleted is read once its save answers with the message before", async () => {
        const h = await startedLive();
        personal(h, "channel.unread", { channel_id: channelB, message_id: 12, id: "5e000021-0000-4000-8000-000000000000" });
        await settle();
        expect(rowOf(h, channelB)).toEqual(expect.objectContaining({ last_message_id: 12, is_unread: true }));

        // 12 is deleted, which a channel's row hears nothing of; the person reads to 11 and leaves
        const toB = h.shell.selectChannel(channelB);
        await settle();
        answerHistory(h, channelB, 11);
        await toB;
        await settle();
        h.shell.seen(11);
        const before = h.saves.length;
        void h.shell.selectChannel(channelA);
        await settle();
        expect(h.saves.slice(before).map(s => `${s.channelId}:${s.messageId}`)).toEqual([`${channelB}:11`]);
        h.saves[before].answer.resolve(fakeResponse(200, { read_cursor: 11, last_message_id: 11 }));
        await settle();

        expect(rowOf(h, channelB)).toEqual(expect.objectContaining({ last_message_id: 11, read_cursor: 11, is_unread: false }));
    });

    test("a read revoked from the open room is reported with that room's kind, so a conversation is not called a channel", async () => {
        const h = build();
        const rows = snapshot();
        rows.channels[0] = { ...rows.channels[0], room_kind: "group", channel_type: "dm", name: null, title: "Ben, Cara" };
        h.bootstrap.value = rows;
        const starting = h.shell.start();
        await settle();
        answerHistory(h, channelA, 10);
        await starting;
        await settle();
        topicOf(h, channelTopic(tenant, channelA)).status?.("SUBSCRIBED");
        await settle();

        topicOf(h, channelTopic(tenant, channelA)).listeners["system"]?.({ status: "error", message: "You do not have permissions to read from this Channel topic: x" });
        await settle();

        const revoked = h.shell.state.errors.find(e => e.code === "rt.read_revoked");
        expect(revoked?.roomKind).toBe("group");
        expect(messageForError(revoked!)).toBe("You no longer have access to this conversation.");
    });

    test("a read revoked from an open channel keeps the channel's sentence", async () => {
        const h = await startedLive();

        topicOf(h, channelTopic(tenant, channelA)).listeners["system"]?.({ status: "error", message: "You do not have permissions to read from this Channel topic: x" });
        await settle();

        const revoked = h.shell.state.errors.find(e => e.code === "rt.read_revoked");
        expect(revoked?.roomKind).toBe("channel");
        expect(messageForError(revoked!)).toBe("You no longer have access to this channel.");
    });

    // The two reads say different things, so the person is told both, and closing one leaves the other.
    test("a channel's lost read and then a conversation's are each told in their own words", async () => {
        const h = await startedLive();
        const revokedText = "You do not have permissions to read from this Channel topic: x";

        topicOf(h, channelTopic(tenant, channelA)).listeners["system"]?.({ status: "error", message: revokedText });
        await settle();

        const opening = h.shell.selectChannel(dmD);
        await settle();
        answerHistory(h, dmD, 10);
        await opening;
        await settle();
        topicOf(h, channelTopic(tenant, dmD)).status?.("SUBSCRIBED");
        await settle();
        topicOf(h, channelTopic(tenant, dmD)).listeners["system"]?.({ status: "error", message: revokedText });
        await settle();

        const revoked = h.shell.state.errors.filter(e => e.code === "rt.read_revoked");
        expect(revoked.map(e => messageForError(e))).toEqual(["You no longer have access to this channel.", "You no longer have access to this conversation."]);

        h.shell.dismissError(revoked[1]);
        expect(h.shell.state.errors.filter(e => e.code === "rt.read_revoked").map(e => messageForError(e))).toEqual(["You no longer have access to this channel."]);
    });

    afterEach(() => {
        jest.useRealTimers();
        jest.restoreAllMocks();
    });

    describe("a rejoin of the personal topic", () => {
        test("reloads the sidebar once, after a random wait of up to five seconds", async () => {
            const h = await startedLive();
            jest.useFakeTimers({ doNotFake: ["queueMicrotask", "nextTick"] });
            jest.spyOn(Math, "random").mockReturnValue(0.5);
            const before = h.counts.bootstraps;

            personalStatus(h, "CHANNEL_ERROR");
            personalStatus(h, "SUBSCRIBED");
            await settle();
            jest.advanceTimersByTime(2_400);
            await settle();
            expect(h.counts.bootstraps).toBe(before);

            jest.advanceTimersByTime(200);
            await settle();
            expect(h.counts.bootstraps).toBe(before + 1);

            jest.advanceTimersByTime(10_000);
            await settle();
            expect(h.counts.bootstraps).toBe(before + 1);
        });

        test("is the only sidebar read of a socket lost under every topic, which fetches no token either", async () => {
            const h = await startedLive();
            jest.useFakeTimers({ doNotFake: ["queueMicrotask", "nextTick"] });
            jest.spyOn(Math, "random").mockReturnValue(0.5);
            const before = { ...h.counts };

            // as supabase-js reports a dropped socket on each joined topic
            const lost = new Error("socket closed: 1006 (network lost)");
            topicOf(h, personalTopic(tenant, person)).status?.("CHANNEL_ERROR", lost);
            topicOf(h, channelTopic(tenant, channelA)).status?.("CHANNEL_ERROR", lost);
            await settle();
            jest.advanceTimersByTime(10_000);
            await settle();
            expect(h.counts.mints).toBe(before.mints);
            expect(h.counts.bootstraps).toBe(before.bootstraps);

            personalStatus(h, "SUBSCRIBED");
            await settle();
            jest.advanceTimersByTime(10_000);
            await settle();
            expect(h.counts.bootstraps).toBe(before.bootstraps + 1);
        });

        test("tells the members a change to any room may have been missed, with the sidebar's reload", async () => {
            const h = await startedLive();
            jest.useFakeTimers({ doNotFake: ["queueMicrotask", "nextTick"] });
            jest.spyOn(Math, "random").mockReturnValue(0.5);
            const rejoined = jest.spyOn(h.shell.members, "rejoined");

            personalStatus(h, "CHANNEL_ERROR");
            personalStatus(h, "SUBSCRIBED");
            await settle();
            jest.advanceTimersByTime(2_600);
            await settle();

            expect(rejoined).toHaveBeenCalledTimes(1);
        });

        test("shares its reload with another rejoin and with a reload asked during the wait", async () => {
            const h = await startedLive();
            jest.useFakeTimers({ doNotFake: ["queueMicrotask", "nextTick"] });
            jest.spyOn(Math, "random").mockReturnValue(0.5);
            const before = h.counts.bootstraps;

            personalStatus(h, "TIMED_OUT");
            personalStatus(h, "SUBSCRIBED");
            jest.advanceTimersByTime(1_000);
            personalStatus(h, "CLOSED");
            personalStatus(h, "SUBSCRIBED");
            askReload(h);
            await settle();
            jest.advanceTimersByTime(10_000);
            await settle();

            expect(h.counts.bootstraps).toBe(before + 1);
        });

        test("the first join of the personal topic reloads nothing", async () => {
            const h = build();
            h.bootstrap.value = snapshot();
            const starting = h.shell.start();
            await settle();
            answerHistory(h, channelA, 10);
            await starting;
            await settle();
            jest.useFakeTimers({ doNotFake: ["queueMicrotask", "nextTick"] });
            const before = h.counts.bootstraps;

            personalStatus(h, "SUBSCRIBED");
            jest.advanceTimersByTime(10_000);
            await settle();

            expect(h.counts.bootstraps).toBe(before);
        });
    });

    describe("a reload in flight", () => {
        // Each event as the platform sends it, and what the row must show once the reload's older
        // answer has landed over it.
        const cases: Array<[string, string, Record<string, unknown>, (h: ReturnType<typeof build>) => void]> = [
            ["channel.unread", "channel.unread", { channel_id: channelB, message_id: 11, id: "5e000011-0000-4000-8000-000000000000" }, h => {
                expect(rowOf(h, channelB)).toEqual(expect.objectContaining({ is_unread: true, last_message_id: 11 }));
            }],
            ["channel.mention, for a message the answer did not count", "channel.mention", { channel_id: channelB, message_id: 11, parent_id: null, in_channel: true, all: false, id: "5e000012-0000-4000-8000-000000000000" }, h => {
                expect(rowOf(h, channelB)?.mention_count).toBe(1);
            }],
            ["dm.preview", "dm.preview", {
                channel_id: dmD,
                message_id: 11,
                preview: "new",
                sent_at: "2026-10-19T10:00:00.5+00:00",
                sender: { nick_name: "Ben", last_name: "Okafor" },
                is_own: false,
                posted_by_rock: false,
                preview_line: "new",
                id: "5e000013-0000-4000-8000-000000000000"
            }, h => {
                expect(rowOf(h, dmD)).toEqual(expect.objectContaining({ last_message_id: 11, last_message_preview: "new", preview_line: "new", last_sender_nick_name: "Ben", is_unread: true }));
            }],
            ["channel.read", "channel.read", { channel_id: channelE, read_cursor: 12, last_message_id: 12, id: "5e000014-0000-4000-8000-000000000000" }, h => {
                expect(rowOf(h, channelE)).toEqual(expect.objectContaining({ read_cursor: 12, is_unread: false, mention_count: 0 }));
                expect(h.shell.mentions.state.hasDot).toBe(false);
            }]
        ];

        test.each(cases)("keeps %s that landed before the answer", async (_name, event, payload, check) => {
            const h = await startedLive();
            h.heldBootstraps.value = [];
            askReload(h);
            await settle();
            expect(h.heldBootstraps.value).toHaveLength(1);

            personal(h, event, payload);
            await settle();
            h.heldBootstraps.value[0].resolve(fakeResponse(200, snapshot()));
            await settle();

            check(h);
        });

        test("does not count a mention twice when the answer already counts it", async () => {
            const h = await startedLive();
            h.heldBootstraps.value = [];
            askReload(h);
            await settle();

            personal(h, "channel.mention", { channel_id: channelB, message_id: 11, parent_id: null, in_channel: true, all: false, id: "5e000015-0000-4000-8000-000000000000" });
            await settle();
            const answer = snapshot();
            answer.channels[1] = sidebarRow(channelB, { last_message_id: 11, is_unread: true, mention_count: 1 });
            answer.me.unread_mentions.push({ channel_id: channelB, parent_id: null, message_id: 11, created_at: "2026-10-19T10:00:00Z", in_channel: true });
            h.heldBootstraps.value[0].resolve(fakeResponse(200, answer));
            await settle();

            expect(rowOf(h, channelB)?.mention_count).toBe(1);
        });

        test.each(["before", "after"])("keeps a save of the read position answered %s the sidebar", async order => {
            const h = await startedLive();
            h.shell.markRead(10);
            await settle();
            const save = h.saves.filter(s => s.channelId === channelA && s.messageId === 10).pop();
            expect(save).toBeDefined();

            h.heldBootstraps.value = [];
            askReload(h);
            await settle();

            if (order === "before") {
                save?.answer.resolve(fakeResponse(200, { read_cursor: 10, last_message_id: 10 }));
                await settle();
                h.heldBootstraps.value[0].resolve(fakeResponse(200, snapshot()));
            }
            else {
                h.heldBootstraps.value[0].resolve(fakeResponse(200, snapshot()));
                await settle();
                save?.answer.resolve(fakeResponse(200, { read_cursor: 10, last_message_id: 10 }));
            }
            await settle();

            expect(rowOf(h, channelA)?.read_cursor).toBe(10);
        });

        test("an answer older than the newest asked for is dropped, its unread mentions with it", async () => {
            const h = await startedLive();
            h.heldBootstraps.value = [];
            askReload(h);
            askReload(h);
            await settle();
            expect(h.heldBootstraps.value).toHaveLength(1);

            const older = snapshot();
            older.channels[1] = sidebarRow(channelB, { name: "Stale" });
            h.heldBootstraps.value[0].resolve(fakeResponse(200, older));
            await settle();
            expect(rowOf(h, channelB)?.name).toBe("Room");

            const newer = snapshot();
            newer.channels[1] = sidebarRow(channelB, { name: "Renamed" });
            newer.me.unread_mentions = [];
            h.heldBootstraps.value[1].resolve(fakeResponse(200, newer));
            await settle();

            expect(rowOf(h, channelB)?.name).toBe("Renamed");
            expect(h.shell.mentions.state.hasDot).toBe(false);
        });

        test("reloads asked while one is out make one more read once it answers, however many", async () => {
            const h = await startedLive();
            h.heldBootstraps.value = [];
            askReload(h);
            await settle();
            askReload(h);
            askReload(h);
            personal(h, "channel.updated", { channel_id: dmD, id: "5e0000c1-0000-4000-8000-000000000000" });
            await settle();
            expect(h.heldBootstraps.value).toHaveLength(1);

            h.heldBootstraps.value[0].resolve(fakeResponse(200, snapshot()));
            await settle();
            expect(h.heldBootstraps.value).toHaveLength(2);

            const newer = snapshot();
            newer.channels[1] = sidebarRow(channelB, { name: "Renamed" });
            h.heldBootstraps.value[1].resolve(fakeResponse(200, newer));
            await settle();

            expect(h.heldBootstraps.value).toHaveLength(2);
            expect(rowOf(h, channelB)?.name).toBe("Renamed");
        });

        test("a change that arrives while the read that follows is out is replayed over its answer", async () => {
            const h = await startedLive();
            h.heldBootstraps.value = [];
            askReload(h);
            askReload(h);
            await settle();
            h.heldBootstraps.value[0].resolve(fakeResponse(200, snapshot()));
            await settle();
            expect(h.heldBootstraps.value).toHaveLength(2);

            personal(h, "channel.unread", { channel_id: channelB, message_id: 11, id: "5e0000c2-0000-4000-8000-000000000000" });
            await settle();
            h.heldBootstraps.value[1].resolve(fakeResponse(200, snapshot()));
            await settle();

            expect(rowOf(h, channelB)).toEqual(expect.objectContaining({ is_unread: true, last_message_id: 11 }));
        });

        test("nothing lands once the shell stops, neither the answer nor a rejoin's wait", async () => {
            const h = await startedLive();
            jest.useFakeTimers({ doNotFake: ["queueMicrotask", "nextTick"] });
            h.heldBootstraps.value = [];
            askReload(h);
            personalStatus(h, "CHANNEL_ERROR");
            personalStatus(h, "SUBSCRIBED");
            await settle();
            const asked = h.heldBootstraps.value.length;

            void h.shell.stop();
            await settle();
            const changed = snapshot();
            changed.channels[1] = sidebarRow(channelB, { name: "Renamed" });
            h.heldBootstraps.value[0].resolve(fakeResponse(200, changed));
            await settle();
            jest.advanceTimersByTime(10_000);
            await settle();

            expect(rowOf(h, channelB)?.name).toBe("Room");
            expect(h.heldBootstraps.value).toHaveLength(asked);
        });
    });

    describe("rooms", () => {
        const volunteers = "c00000f1-0000-4000-8000-000000000000";

        /** chat_get_channel's answer for Volunteers, a public channel the reader has not joined. */
        function volunteersFacts(extra: Record<string, unknown> = {}): Record<string, unknown> {
            return memberFacts(volunteers, {
                name: "Volunteers",
                is_member: false,
                is_public: true,
                can_post: false,
                can_post_code: "authz.join_required",
                can_post_reason: "Join this channel to post.",
                can_leave: false,
                ...extra
            });
        }

        /** Opens Volunteers, which has no sidebar row, and answers its newest page. */
        async function openVolunteers(h: ReturnType<typeof build>): Promise<void> {
            h.channelFacts[volunteers] = volunteersFacts();
            const opening = h.shell.selectChannel(volunteers);
            await settle();
            answerHistory(h, volunteers, 30);
            await opening;
            await settle();
        }

        test("a conversation's facts are read beside its history on every open", async () => {
            const h = await startedLive();
            expect(h.channelReads).toEqual([channelA]);

            const toB = h.shell.selectChannel(channelB);
            await settle();
            // asked before its history answers, not after
            expect(h.channelReads).toEqual([channelA, channelB]);
            answerHistory(h, channelB, 11);
            await toB;
            await settle();

            expect(h.shell.rooms.factsOf(channelB)).toEqual(memberFacts(channelB));
        });

        test("a membership change for a room reads its facts again", async () => {
            const h = await startedLive();

            personal(h, "membership.changed", { channel_id: channelA, id: "5e0000f1-0000-4000-8000-000000000000" });
            await settle();

            expect(h.channelReads).toEqual([channelA, channelA]);
        });

        test("a room renamed elsewhere reads its row and its facts again, and asks no new token", async () => {
            const h = await startedLive();
            const before = { ...h.counts };
            const renamed = snapshot();
            renamed.channels = renamed.channels.map(row => row.channel_id === channelA ? { ...row, name: "Trail crew" } : row);
            h.bootstrap.value = renamed;

            personal(h, "channel.updated", { channel_id: channelA, id: "5e0000f2-0000-4000-8000-000000000000" });
            await settle();

            expect(h.counts.bootstraps).toBe(before.bootstraps + 1);
            expect(h.counts.mints).toBe(before.mints);
            expect(h.channelReads).toEqual([channelA, channelA]);
            expect(h.shell.channels.rows.find(r => r.channel_id === channelA)?.name).toBe("Trail crew");
        });

        test("a group that loses a person elsewhere becomes a two-person conversation in place, its row, facts and open roster read again", async () => {
            const h = await startedLive();
            const ben = { person_alias_guid: "ben-okafor", nick_name: "Ben", last_name: "Okafor", avatar_url: null };
            const cara = { person_alias_guid: "cara-lin", nick_name: "Cara", last_name: "Lin", avatar_url: null };
            const group = snapshot();
            group.channels = group.channels.map(row => row.channel_id === dmD ? { ...row, title: "Ben and Cara", room_kind: "group", dm_people: [ben, cara], dm_other_count: 2 } : row);
            h.bootstrap.value = group;
            personal(h, "channel.updated", { channel_id: dmD, id: "5e0000f4-0000-4000-8000-000000000000" });
            await settle();
            const toGroup = h.shell.selectChannel(dmD);
            await settle();
            answerHistory(h, dmD, 11);
            await toGroup;
            void h.shell.members.open(dmD);
            await settle();
            const row = rowOf(h, dmD);
            const reads = { facts: h.channelReads.length, roster: h.memberCalls.length };
            expect(row).toEqual(expect.objectContaining({ title: "Ben and Cara", room_kind: "group" }));

            h.bootstrap.value = snapshot();
            personal(h, "channel.updated", { channel_id: dmD, id: "5e0000f5-0000-4000-8000-000000000000" });
            await settle();

            expect(h.shell.state.activeChannelId).toBe(dmD);
            expect(rowOf(h, dmD)).toEqual(expect.objectContaining({ title: "Ben Okafor", room_kind: "person", dm_other_count: 1 }));
            expect(h.channelReads.slice(reads.facts)).toEqual([dmD]);
            expect(h.memberCalls.slice(reads.roster)).toEqual([expect.objectContaining({ p_channel_id: dmD, p_after: null })]);
        });

        test("a change to another room leaves an open members panel as it is", async () => {
            const h = await startedLive();
            void h.shell.members.open(channelA);
            await settle();
            const asked = h.memberCalls.length;

            personal(h, "channel.updated", { channel_id: channelB, id: "5e0000f6-0000-4000-8000-000000000000" });
            personal(h, "membership.changed", { channel_id: channelB, id: "5e0000f7-0000-4000-8000-000000000000" });
            await settle();

            expect(h.memberCalls).toHaveLength(asked);
        });

        test("the directory asks chat_list_directory with the church's campus limit and the chosen filters", async () => {
            const h = await startedLive({ areSharedChannelsFilteredByCampus: true });

            h.shell.rooms.openDirectory();
            await settle();

            expect(h.directoryCalls).toEqual([{
                p_query: null,
                p_campus_only: true,
                p_campus_id: null,
                p_whole_church: false,
                p_not_joined: false,
                p_after: null,
                p_limit: 45
            }]);
        });

        test("a room opened with no sidebar row is previewed after the channels, kept across a reload in flight and its replay", async () => {
            const h = await startedLive();
            h.heldBootstraps.value = [];
            askReload(h);
            await settle();

            await openVolunteers(h);
            expect(h.shell.channels.rows.map(r => r.channel_id)).toEqual([channelA, channelB, dmD, channelE, volunteers]);
            expect(rowOf(h, volunteers)).toEqual(expect.objectContaining({ name: "Volunteers", is_member: false }));

            personal(h, "channel.unread", { channel_id: channelB, message_id: 11, id: "5e0000f2-0000-4000-8000-000000000000" });
            await settle();
            h.heldBootstraps.value[0].resolve(fakeResponse(200, snapshot()));
            await settle();

            expect(rowOf(h, volunteers)).toEqual(expect.objectContaining({ name: "Volunteers", is_member: false }));
            expect(rowOf(h, channelB)).toEqual(expect.objectContaining({ is_unread: true, last_message_id: 11 }));
        });

        test("the previewed row goes once another conversation opens", async () => {
            const h = await startedLive();
            await openVolunteers(h);
            expect(rowOf(h, volunteers)).toBeDefined();

            void h.shell.selectChannel(channelA);
            await settle();

            expect(rowOf(h, volunteers)).toBeUndefined();
        });

        test("no read position is saved for a room only previewed", async () => {
            const h = await startedLive();
            await openVolunteers(h);
            h.shell.seen(30);

            void h.shell.selectChannel(channelA);
            await settle();

            expect(h.saves.filter(s => s.channelId === volunteers)).toEqual([]);
        });

        test("a pinned room the person has not joined still saves where they have read to", async () => {
            const h = build();
            const pinned = snapshot();
            pinned.channels[0] = sidebarRow(channelA, { read_cursor: 9, is_public: true, always_shown: true, is_member: false });
            h.bootstrap.value = pinned;
            const starting = h.shell.start();
            await settle();
            answerHistory(h, channelA, 10);
            await starting;
            await settle();

            h.shell.markRead(10);
            await settle();

            expect(h.saves.filter(s => s.channelId === channelA && s.messageId === 10)).toHaveLength(1);
        });

        test("a pending join settles on the reload its membership change asks, which brings the real row", async () => {
            const doorCalls: string[] = [];
            const h = await startedLive({
                joinChannel: async channelId => {
                    doorCalls.push(channelId);
                    return { code: "ok", message: null, isPending: true, personAliasGuid: person } as never;
                }
            });
            await openVolunteers(h);

            await h.shell.rooms.join(volunteers);
            await settle();
            expect(doorCalls).toEqual([volunteers]);
            expect(h.shell.rooms.joinOf(volunteers)).toEqual({ kind: "pending" });

            const joined = snapshot();
            joined.channels.push(sidebarRow(volunteers, { name: "Volunteers", is_public: true, last_message_id: 31, read_cursor: 30 }));
            h.bootstrap.value = joined;
            h.channelFacts[volunteers] = volunteersFacts({ is_member: true, can_post: true, can_post_code: null, can_post_reason: null });
            personal(h, "membership.changed", { channel_id: volunteers, id: "5e0000f3-0000-4000-8000-000000000000" });
            await settle();

            expect(h.shell.rooms.joinOf(volunteers)).toEqual({ kind: "joined", isFresh: true });
            expect(h.shell.channels.rows.filter(r => r.channel_id === volunteers)).toEqual([expect.objectContaining({ is_member: true, last_message_id: 31 })]);
            expect(h.shell.rooms.factsOf(volunteers)?.is_member).toBe(true);
        });

        test("a stale row's send refused as needing a join joins through the door and resends once under the same key", async () => {
            const doorCalls: string[] = [];
            const h = await startedLive({
                joinChannel: async channelId => {
                    doorCalls.push(channelId);
                    return { code: "ok", message: null, isPending: false, personAliasGuid: person } as never;
                }
            });
            // the row still says the person is in channel A, which they have since left in Rock
            h.sendRefusals.value = [{ status: 403, body: { code: "42501", message: "authz.join_required", hint: "Join this channel to post." } }];

            await h.shell.send("hello again");
            await settle();

            expect(doorCalls).toEqual([channelA]);
            expect(h.sent.map(s => s.channelId)).toEqual([channelA, channelA]);
            expect(h.sent[1].args.p_client_key).toBe(h.sent[0].args.p_client_key);
            expect(h.shell.sender.pending(channelA)).toEqual([]);
        });
    });

    // The conversations' doors and the profile card's read, through the shell: Leave and Rename
    // reach Rock's doors as the block hands them in, a leave of the conversation on screen opens
    // the next one in the sidebar, and the card reads chat_get_person through the one call path.
    describe("conversations", () => {
        const anna = "a0000002-0000-4000-8000-000000000000";

        /** chat_get_person's answer for Anna, exactly its keys. */
        const annaProfile = {
            person_alias_guid: anna,
            nick_name: "Anna",
            last_name: "Reyes",
            avatar_url: null,
            campus_id: null,
            is_self: false,
            is_leader: true,
            details: { badge_keys: [] },
            can_message: true,
            dm_channel_id: dmD
        };

        test("the card reads chat_get_person for the person and the room, and holds its answer", async () => {
            const h = await startedLive();
            h.personAnswer.value = { status: 200, body: annaProfile };

            h.shell.rooms.openCard(anna, channelA);
            await settle();

            expect(h.personCalls).toEqual([{ p_person_alias_guid: anna, p_channel_id: channelA }]);
            expect(h.shell.rooms.state.card).toEqual({ personAliasGuid: anna, channelId: channelA, person: annaProfile, rock: { kind: "failed" } });
        });

        test("a person the platform does not list opens no card", async () => {
            const h = await startedLive();
            h.personAnswer.value = { status: 400, body: { code: "P0001", message: "rpc.person_not_found", hint: "This person isn't available in chat." } };

            h.shell.rooms.openCard(anna, channelA);
            await settle();

            expect(h.personCalls).toHaveLength(1);
            expect(h.shell.rooms.state.card).toBeNull();
        });

        test("Leave goes through Rock's door, and leaving the conversation on screen opens the next one", async () => {
            const doorCalls: string[] = [];
            const h = await startedLive({
                leaveChannel: async channelId => {
                    doorCalls.push(channelId);
                    return { code: "ok", message: null, isPending: false, personAliasGuid: person } as never;
                }
            });

            await h.shell.rooms.leave(channelA);
            await settle();

            expect(doorCalls).toEqual([channelA]);
            expect(h.shell.rooms.leaveOf(channelA)).toEqual({ kind: "left" });
            expect(h.shell.state.activeChannelId).toBe(channelB);
            expect(h.history[channelB]).toHaveLength(1);
        });

        test("Rename goes through Rock's door with the room and the name", async () => {
            const doorCalls: Array<[string, string]> = [];
            const h = await startedLive({
                renameConversation: async (channelId, name) => {
                    doorCalls.push([channelId, name]);
                    return { code: "ok", message: null, isPending: true, personAliasGuid: person } as never;
                }
            });

            await h.shell.rooms.rename(dmD, "Trail crew");
            await settle();

            expect(doorCalls).toEqual([[dmD, "Trail crew"]]);
            expect(h.shell.rooms.renameOf(dmD)).toEqual({ kind: "idle" });
        });

        // Rock pushes a pending name on after its door answers; until the platform's own signal for
        // the room, a reload for anything else reads the old name, which must not replace the new.
        test("a pending name stays through another reload until the platform's signal for the room", async () => {
            const h = await startedLive({
                renameConversation: async () => ({ code: "ok", message: null, isPending: true, personAliasGuid: person }) as never
            });
            await h.shell.rooms.rename(dmD, "Trail crew");
            await settle();
            expect(rowOf(h, dmD)?.title).toBe("Trail crew");
            const reads = h.counts.bootstraps;

            // the platform's rows, read before it has the name
            h.bootstrap.value = snapshot();
            askReload(h);
            await settle();
            expect(h.counts.bootstraps).toBe(reads + 1);
            expect(rowOf(h, dmD)?.title).toBe("Trail crew");

            const renamed = snapshot();
            renamed.channels = renamed.channels.map(row => row.channel_id === dmD ? { ...row, name: "Hike crew", title: "Hike crew" } : row);
            h.bootstrap.value = renamed;
            personal(h, "channel.updated", { channel_id: dmD, id: "5e0000f9-0000-4000-8000-000000000000" });
            await settle();
            expect(rowOf(h, dmD)?.title).toBe("Hike crew");
        });

        test("the members panel asks Rock's doors: its rights on open, and a remove of someone on the roster", async () => {
            const doorCalls: unknown[][] = [];
            const h = await startedLive({
                isChatAdministrator: true,
                getMemberRights: async channelId => {
                    doorCalls.push(["rights", channelId]);
                    return { canManageMembers: true, canRename: false, lastLeaderPersonAliasGuid: null };
                },
                removeMember: async (channelId, personAliasGuid, replacement) => {
                    doorCalls.push(["remove", channelId, personAliasGuid, replacement]);
                    return { code: "ok", message: null, isPending: false };
                }
            });

            void h.shell.members.open(channelA);
            await settle();
            expect(h.shell.members.state.canManageMembers).toBe(true);
            expect(h.shell.members.state.isChatAdministrator).toBe(true);

            h.shell.members.state.rows = [{ person_alias_guid: anna, nick_name: "Anna", last_name: "Reyes", avatar_url: null, is_leader: false, details: null, can_message: true }];
            h.shell.members.askRemove(anna);
            await h.shell.members.remove();

            expect(doorCalls).toEqual([["rights", channelA], ["remove", channelA, anna, null]]);
            expect(h.shell.members.state.rows).toEqual([]);
        });

        test("without the doors, Leave and Rename are refused with a sentence and nothing else changes", async () => {
            const h = await startedLive();

            await h.shell.rooms.leave(channelA);
            await h.shell.rooms.rename(dmD, "Trail crew");

            expect(h.shell.rooms.leaveOf(channelA)).toEqual({ kind: "refused", text: expect.any(String) });
            expect(h.shell.rooms.renameOf(dmD)).toEqual({ kind: "refused", text: expect.any(String) });
            expect(h.shell.state.activeChannelId).toBe(channelA);
        });
    });

    // Rock's door makes the group and awaits the immediate sync, whose membership signal reloads
    // the sidebar before the door answers; the platform lists a direct message only once a message
    // is written in it, so that reload leaves it out. The first message is posted after, and its
    // dm.preview reaches every member's topic, the sender's own included.
    describe("a direct message the person starts", () => {
        const created = "d0000010-0000-4000-8000-000000000000";
        const ben = { person_alias_guid: "a0000002-0000-4000-8000-000000000000", nick_name: "Bo", last_name: "Picked", avatar_url: null };

        test("is open on a stand-in titled as the draft was, and listed once, at the top, as its first message posts", async () => {
            let door!: (result: DoorResult) => void;
            const h = await startedLive({ startDirectMessage: () => new Promise<DoorResult>(resolve => door = resolve) });
            h.channelFacts[created] = memberFacts(created, { name: null, channel_type: "dm", group_type_name: null, member_count: 2, people: [{ ...ben, details: null }] });
            const listed = (): string[] => h.shell.channels.listed.map(r => r.channel_id);
            const orders: string[][] = [];

            h.shell.directMessages.begin([ben]);
            await settle();
            const sending = h.shell.directMessages.sendFirst("hello");
            await settle();

            personal(h, "membership.changed", { channel_id: created, id: "5e0000d1-0000-4000-8000-000000000000" });
            await settle();
            orders.push(listed());

            door({ code: "ok", channelGuid: created, isPending: false, message: null, personAliasGuid: null });
            await settle();
            // the header draws from the stand-in while the conversation's first page is on its way
            expect(h.shell.channels.rows.find(r => r.channel_id === created)).toEqual(expect.objectContaining({ title: "Bo Picked", room_kind: "person" }));
            answerHistory(h, created, 0);
            await sending;
            await settle();
            orders.push(listed());
            expect(h.shell.channels.rows.find(r => r.channel_id === created)).toEqual(expect.objectContaining({ title: "Bo Picked", room_kind: "person", max_length: 4000 }));

            personal(h, "dm.preview", {
                channel_id: created, message_id: 901, preview: "hello", sent_at: "2026-10-19T10:00:00Z", sender: { nick_name: "Ada", last_name: "Member" },
                is_own: true, posted_by_rock: false, preview_line: "You: hello", id: "5e0000d2-0000-4000-8000-000000000000"
            });
            await settle();
            orders.push(listed());

            expect(h.sent.map(s => s.channelId)).toEqual([created]);
            expect(h.shell.state.activeChannelId).toBe(created);
            // not listed while nothing is written in it, then listed first, never drawn low and moved
            expect(orders.map(order => order.indexOf(created))).toEqual([-1, -1, 0]);
            expect(orders.map(order => order.length)).toEqual([4, 4, 5]);
        });
    });
});

// The person's settings ride the shell: the bootstrap's me is where the settings start, a room's
// star, hide and notify mode and the reaction switch go to chat_set_prefs through the one call
// path, the two privacy switches go to Rock's SavePersonSetting door, and stopping the shell stops
// them.
describe("settings", () => {
    async function startedWith(bootstrap: unknown, options: Parameters<typeof build>[0] = {}): Promise<ReturnType<typeof build>> {
        const h = build(options);
        h.bootstrap.value = bootstrap;
        const starting = h.shell.start();
        await settle();
        answerHistory(h, channelA, 10);
        await starting;
        await settle();
        return h;
    }

    test("a settings-only refresh carries read only independently of the unchanged posting row", async () => {
        const row = { channel_id: channelA, channel_type: "shared", is_member: true, can_post: true, can_post_code: null, can_post_reason: null };
        const h = await startedWith({ channels: [row], me: { unread_mentions: [] } });
        const refresh = deferred<void>();
        h.heldExchange.value = refresh;
        h.exchangeSettings.value = { limits: platformLimits, service: { state: "read_only", banner: "Chat is read only until 9." } };

        personal(h, "session.recheck", {});
        await settle();
        expect(h.shell.state.isComposerOpen).toBe(true);
        refresh.resolve();
        await settle();

        expect(h.shell.channels.rows[0]).toMatchObject({ can_post: true, can_post_code: null, can_post_reason: null });
        expect(h.shell.state).toMatchObject({
            isComposerOpen: false,
            service: { state: "read_only", banner: "Chat is read only until 9." }
        });
    });

    test("the bootstrap's me is where the person's own switches start", async () => {
        const h = await startedWith({ channels: [], me: { show_profile_details: false, is_open_dm_allowed: true, reaction_push: false, unread_mentions: [] } });

        expect(h.shell.prefs.state.me).toEqual({ show_profile_details: false, is_open_dm_allowed: true, reaction_push: false });
    });

    test("a room's notify mode is one chat_set_prefs call with the room and the patch", async () => {
        // the room in the sidebar, as the bootstrap lists it, where its mode shows
        const row = {
            channel_id: channelB, name: "Room", icon_url: null, channel_type: "shared", is_public: false, always_shown: false, is_member: true,
            is_favorite: false, notify_mode: "all", is_leader: false, can_post_announcements: false, can_mention_all: false, last_message_id: 10,
            last_message_at: "2026-10-19T09:00:00Z", last_message_type: "text", last_sender_nick_name: "Ada", last_sender_last_name: "Member",
            last_sender_listed: true, last_message_preview: "old", last_message_is_own: false, last_message_posted_by_rock: false, read_cursor: 10,
            is_unread: false, mention_count: 0, all_mention_count: 0, can_mention: true, dm_people: null, dm_other_count: null,
            room_kind: "channel", title: "Room", preview_line: ""
        };
        const h = await startedWith({ channels: [row], me: { unread_mentions: [] } });
        h.prefsAnswer.value = { status: 200, body: { channel: { channel_id: channelB, is_favorite: false, hidden: false, notify_mode: "silent", notify_mode_effective: "silent" }, me: {} } };

        await h.shell.prefs.setNotifyMode(channelB, "silent", false);

        expect(h.prefsCalls).toEqual([{ p_channel_id: channelB, p_changes: { notify_mode: "silent" } }]);
    });

    test("the reaction switch is chat_set_prefs with no room", async () => {
        const h = await startedWith({ channels: [], me: { show_profile_details: true, is_open_dm_allowed: false, reaction_push: true, unread_mentions: [] } });

        await h.shell.prefs.setReactionPush(false);

        expect(h.prefsCalls).toEqual([{ p_channel_id: null, p_changes: { reaction_push: false } }]);
    });

    test("a privacy switch goes to Rock's door, not the platform", async () => {
        const doorCalls: Array<[string, boolean]> = [];
        const h = await started({
            savePersonSetting: async (setting, value) => {
                doorCalls.push([setting, value]);
                return { code: "ok", isPending: false, message: null };
            }
        });

        await h.shell.prefs.setPersonSetting("profile_details", false);

        expect(doorCalls).toEqual([["profile_details", false]]);
        expect(h.prefsCalls).toEqual([]);
    });

    test("stopping the shell stops the settings", async () => {
        const h = await started();

        await h.shell.stop();
        await h.shell.prefs.setStarred(channelA, true, false);

        expect(h.prefsCalls).toEqual([]);
    });
});

// A browser's push as the shell is handed it: the permission and the token held by the test, and
// a worker already active. Firebase's values come from the token exchange's settings.
function fakeBrowserPush(permission: { value: string }, token: () => Promise<string> = async () => "fcm-1"): NonNullable<ShellOptions["push"]> {
    const cookie: { value: string | null } = { value: null };
    const worker = { state: "activated", addEventListener: () => undefined, removeEventListener: () => undefined } as unknown as ServiceWorker;
    return {
        pagePath: "/chat",
        permission: () => permission.value,
        requestPermission: async () => {
            permission.value = "granted";
            return permission.value;
        },
        registerWorker: async () => ({ installing: null, waiting: null, active: worker }),
        getToken: token,
        writeCookie: value => {
            cookie.value = value;
        },
        readCookie: () => cookie.value
    };
}

const firebaseWeb = { apiKey: "k", projectId: "p", messagingSenderId: "1", appId: "1:1:web:1" };

/** Push settings as the token exchange answers them, with the church's prompt timing. */
function pushSettings(prompt: string): Record<string, unknown> {
    return { web: firebaseWeb, vapid_key: "v", prompt };
}

/** The key a person's Turn off is kept under on this browser. */
function pushOffKey(personAliasGuid: string): string {
    return `rock-chat:push-off:${tenant}:${personAliasGuid}`;
}

// Chat settings' browser row reads the shell's push state; a browser with no push at all is not
// set up, and Turn on and Turn off go to push.
describe("the browser row", () => {
    test("a browser with no push at all is not set up", async () => {
        const h = await started();

        expect(h.shell.state.push).toBe("unavailable");
    });

    test("is drawn before a slow first registration settles, never as not set up", async () => {
        const token = deferred<string>();
        const h = build({ push: fakeBrowserPush({ value: "granted" }, () => token.promise), pushSettings: pushSettings("off") });
        const starting = h.shell.start();
        await settle();
        answerHistory(h, channelA, 10);
        await settle();

        // the token is still on its way, so the start has not settled
        expect(h.deviceCalls).toEqual([]);
        expect(h.shell.state.push).toBe("offered");

        token.resolve("fcm-1");
        await starting;
        expect(h.deviceCalls.map(c => c.name)).toEqual(["chat_register_device"]);
        expect(h.shell.state.push).toBe("on");
    });

    test("Turn on with the browser already allowing shows on only once the register settles", async () => {
        const token = deferred<string>();
        const h = await started({
            stored: { [pushOffKey(person)]: "true" },
            push: fakeBrowserPush({ value: "granted" }, () => token.promise),
            pushSettings: pushSettings("off")
        });
        expect(h.shell.state.push).toBe("offered");

        const turning = h.shell.turnOnPush();
        await settle();
        expect(h.shell.state.push).toBe("offered");

        token.resolve("fcm-1");
        await turning;
        expect(h.shell.state.push).toBe("on");
    });

    test("Turn off is kept per church and person, so another person on the browser still registers", async () => {
        const stored: Record<string, string> = {};
        const ada = await started({ stored, push: fakeBrowserPush({ value: "granted" }), pushSettings: pushSettings("off") });
        await ada.shell.turnOffPush();
        expect(ada.shell.state.push).toBe("offered");
        ada.shell.stop();

        const other = "a0000009-0000-4000-8000-000000000000";
        const ben = await started({ stored, personAliasGuid: other, push: fakeBrowserPush({ value: "granted" }), pushSettings: pushSettings("off") });

        expect(ben.deviceCalls.map(c => c.name)).toEqual(["chat_register_device"]);
        expect(ben.shell.state.push).toBe("on");
        expect(stored[pushOffKey(person)]).toBe("true");
    });

    test("an unregister the platform refuses is told, and the browser stays turned off", async () => {
        const h = await started({ push: fakeBrowserPush({ value: "granted" }), pushSettings: pushSettings("off") });
        h.deviceAnswer.value = { status: 500, body: { code: "rpc.unavailable", message: "x" } };

        await h.shell.turnOffPush();

        expect(h.deviceCalls.map(c => c.name)).toEqual(["chat_register_device", "chat_unregister_device"]);
        expect(h.shell.state.errors.map(e => e.code)).toContain("push.unregister_failed");
        expect(h.shell.state.push).toBe("offered");
    });
});

// The header offers push unasked as the church's prompt timing says: never with off, from the
// first load with offer, and after the person's first send with after_send. Chat settings offers
// it whatever the timing.
describe("the header's push offer", () => {
    async function withPrompt(prompt: string): Promise<ReturnType<typeof build>> {
        return started({ push: fakeBrowserPush({ value: "default" }), pushSettings: pushSettings(prompt) });
    }

    test("off never offers it, even after a send, while Chat settings still does", async () => {
        const h = await withPrompt("off");
        expect(h.shell.state.isPushOffered).toBe(false);

        await h.shell.send("on my way");
        expect(h.shell.state.isPushOffered).toBe(false);
        expect(h.shell.state.push).toBe("offered");
    });

    test("offer offers it from the first load", async () => {
        const h = await withPrompt("offer");

        expect(h.shell.state.isPushOffered).toBe(true);
    });

    test("after_send offers it only once the person has sent", async () => {
        const h = await withPrompt("after_send");
        expect(h.shell.state.isPushOffered).toBe(false);

        await h.shell.send("on my way");
        expect(h.shell.state.isPushOffered).toBe(true);
    });

    test("a yes at the browser takes the offer away", async () => {
        const h = await withPrompt("offer");

        await h.shell.turnOnPush();

        expect(h.shell.state.isPushOffered).toBe(false);
        expect(h.shell.state.push).toBe("on");
    });
});

// A place's files are uploaded before its send and handed over with its text, as the ids of their
// uploads; text left blank goes as none, so a message of files alone is stored with no body.
describe("a send with files", () => {
    test("takes the conversation's uploaded files with its text, and leaves the tray empty", async () => {
        const h = await started();
        const tray = h.shell.trayOf(channelA);
        tray.add([new File(["x"], "a.pdf", { type: "application/pdf" })]);
        await settle();
        expect(h.begins).toEqual([{ p_channel_id: channelA, p_files: [{ name: "a.pdf", type: "application/pdf", size: 1, width: null, height: null }] }]);
        expect(h.uploads[0].url).toMatch(/^http:\/\/platform\/storage\/v1\/object\/chat-attachments\//);
        h.uploads[0].respond(200);
        await settle();
        const uploadId = h.uploads[0].url.split("/").pop();

        await h.shell.send("");
        await settle();

        expect(h.sent[0].args).toMatchObject({ p_channel_id: channelA, p_body: null, p_attachments: [uploadId] });
        expect(tray.files).toEqual([]);
    });

    test("a reply takes its thread's files, never the conversation's", async () => {
        const h = await started();
        h.shell.trayOf(channelA).add([new File(["x"], "room.pdf", { type: "application/pdf" })]);
        h.shell.trayOf(channelA, 10).add([new File(["x"], "thread.pdf", { type: "application/pdf" })]);
        await settle();
        h.uploads.forEach(upload => upload.respond(200));
        await settle();

        await h.shell.sendReply(10, "see this", false);
        await settle();

        const threadUpload = h.uploads[1].url.split("/").pop();
        expect(h.sent[0].args).toMatchObject({ p_body: "see this", p_attachments: [threadUpload] });
        expect(h.shell.trayOf(channelA).files.map(f => f.name)).toEqual(["room.pdf"]);
    });
    test("a confirmed send lets the bytes kept for it go", async () => {
        const h = await started();
        h.shell.trayOf(channelA).add([new File(["x"], "a.pdf", { type: "application/pdf" })]);
        await settle();
        h.uploads[0].respond(200);
        await settle();
        const [uploadId, key] = [h.uploads[0].url.split("/").pop() as string, h.uploads[0].url.split("/chat-attachments/").pop() as string];

        await h.shell.send("");
        await settle();

        const item = { id: uploadId, key, kind: "file" as const, name: "a.pdf", size: 1, type: "application/pdf", width: null, height: null };
        expect(await h.shell.attachments.reupload(channelA, [item])).toBeNull();
        expect(h.begins).toHaveLength(1);
    });

    test("stopping the shell ends the uploads still going", async () => {
        const h = await started();
        h.shell.trayOf(channelA).add([new File(["x"], "a.pdf", { type: "application/pdf" })]);
        await settle();

        await h.shell.stop();

        expect(h.uploads[0].isAborted).toBe(true);
    });
});
