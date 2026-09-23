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
// The shell's wiring: one place that builds the session, the live connection, the timelines,
// the sender, the read tracker and the sidebar, and joins them. Each of those is its own tested
// module; what is here decides only which one hears what.
//
// Each step marks the page's performance timeline (chat:mint, chat:exchange, chat:history,
// chat:join, chat:sidebar, chat:first-message), so the time from opening the page to the first
// message on screen can be split into the steps that make it up.
import { reactive } from "vue";
import { classifyActionFailure, classifyPlatformError } from "./errors.partial";
import { createTimelines, Timelines } from "./composables/useHistory.partial";
import { createKeepaliveSave, createReadTracker, PageEventTargets, ReadTracker } from "./composables/useMarkRead.partial";
import { createRealtimeHub, RealtimeClientLike, RealtimeHub } from "./composables/useRealtimeHub.partial";
import { createSender, Sender } from "./composables/useSend.partial";
import { ChatSession, ChurchTokenResult, createSession, ExchangeResult } from "./composables/useSession.partial";
import {
    openChannel,
    OpenOutcome,
    readRememberedChannel,
    rememberedChannelKey,
    startPageLoad,
    StorageLike,
    writeRememberedChannel
} from "./pageLoad.partial";
import { ChannelStore, createChannelStore } from "./stores/channelStore.partial";
import { ChatError, HistoryPage, SidebarRow, TokenExchangeResponse } from "./types.partial";

/** What a platform call answers, as the platform client returns it. */
export type RpcResult = {
    data: unknown;
    error: { message?: string | null, code?: string | null, details?: string | null } | null;
    status?: number;
};

/** The part of the platform client the shell uses. */
export type PlatformClientLike = RealtimeClientLike & {
    rpc: (name: string, args: Record<string, unknown>) => PromiseLike<RpcResult>;
};

/** What the Rock token action answered. */
export type MintActionResult = {
    isSuccess: boolean;
    statusCode: number;
    data?: { gate?: string | null, churchToken?: string | null } | null;
};

/** Everything the shell reaches outside itself. */
export type ShellOptions = {
    /** The gate outcome and public settings Rock sent when the block loaded. */
    session: {
        gate?: string | null;
        projectUrl?: string | null;
        publishableKey?: string | null;
        tenantId?: string | null;
        personAliasGuid?: string | null;
    };

    /** The channel a link named, or null. */
    linkedChannelId: string | null;

    /** Asks Rock for a church token. */
    mintChurchToken: () => Promise<MintActionResult>;

    /** Creates the platform client, whose every request reads the token through the callback. */
    createPlatformClient: (url: string, key: string, accessToken: () => Promise<string>) => PlatformClientLike;

    fetch: (url: string, init: RequestInit) => Promise<Response>;
    storage: StorageLike | null;
    pageTargets: PageEventTargets;
    mark: (name: string) => void;
};

/** What the page shows. */
export type ShellState = {
    /** Where the shell has got to. */
    phase: "starting" | "ready" | "refused" | "failed";

    /** The gate that refused, when one did. */
    gate: string | null;

    /** The live connection's trouble, or null when it is healthy. */
    connection: ChatError | null;

    /** Failures worth telling the person about, newest last, one per code. */
    errors: ChatError[];

    /** The channel on screen. */
    activeChannelId: string | null;
};

/** The shell a block holds. */
export type ChatShell = {
    state: ShellState;
    channels: ChannelStore;
    timelines: Timelines;
    sender: Sender;

    /** Signs in and runs the page load. */
    start: () => Promise<void>;

    /** Opens a channel the person chose. */
    selectChannel: (channelId: string) => Promise<void>;

    /** A message of the open channel was on screen. */
    seen: (messageId: number) => void;

    /** The first message of the page load reached the screen. */
    firstMessageShown: () => void;

    /** Sends a message to the open channel. */
    send: (body: string) => Promise<void>;

    /** Fetches older messages of the open channel. */
    loadOlder: () => Promise<void>;

    /** Dismisses a failure. */
    dismissError: (code: string) => void;

    /** Saves the read position and leaves every topic. */
    stop: () => Promise<void>;
};

/**
 * The sentence shown for a gate that refused. The reason a person is refused is not always
 * theirs to know, so the sentences that could reveal a ban say no more than that chat is not
 * available to them.
 *
 * @param gate The gate code.
 *
 * @returns The sentence.
 */
export function gateMessage(gate: string | null): string {
    switch (gate) {
        case "sign_in_required":
            return "Sign in to use chat.";
        case "not_configured":
            return "Chat is not set up for this organization yet.";
        case "age_verification_required":
            return "Chat needs your birthdate before it can open. Add it to your profile, then come back.";
        case "age_restricted":
            return "Chat is not available at your age.";
        case "invalid_key":
        case "gate_unavailable":
            return "Chat could not start. Try again later, and let your church know if this keeps happening.";
        default:
            return "Chat is not available for your account. If you think this is wrong, let your church know.";
    }
}

/**
 * What the Rock token action's answer means to the session.
 *
 * @param result What the action answered.
 *
 * @returns The church token, or the reason there is none.
 */
export function churchTokenFromAction(result: MintActionResult): ChurchTokenResult {
    if (result.isSuccess && result.data) {
        return { gate: result.data.gate ?? "gate_unavailable", churchToken: result.data.churchToken ?? null };
    }

    // Rock answers 401 when the person's Rock sign-in has ended.
    if (result.statusCode === 401) {
        return { gate: "sign_in_required", churchToken: null };
    }

    // No answer, too many requests, or a server fault may pass; anything else is Rock refusing.
    const isPassing = result.statusCode === 0 || result.statusCode === 429 || result.statusCode >= 500;

    return isPassing
        ? { gate: "gate_unavailable", churchToken: null, isUnreachable: true }
        : { gate: "gate_unavailable", churchToken: null };
}

/**
 * Creates the shell.
 *
 * @param options Everything the shell reaches outside itself.
 *
 * @returns The shell.
 */
export function createChatShell(options: ShellOptions): ChatShell {
    const state = reactive<ShellState>({ phase: "starting", gate: options.session.gate ?? null, connection: null, errors: [], activeChannelId: null }) as ShellState;
    const projectUrl = (options.session.projectUrl ?? "").replace(/\/+$/, "");
    const publishableKey = options.session.publishableKey ?? "";
    const tenantId = options.session.tenantId ?? "";
    const personAliasGuid = options.session.personAliasGuid ?? "";

    let client: PlatformClientLike | null = null;
    let hub: RealtimeHub | null = null;
    let detachPage: (() => void) | null = null;
    let isFirstMessageMarked = false;

    /** Tells the person about a failure, once per code until it is dismissed. */
    function report(error: ChatError): void {
        if (!state.errors.some(e => e.code === error.code)) {
            state.errors.push(error);
        }
    }

    const session: ChatSession = createSession({
        mintChurchToken: async (): Promise<ChurchTokenResult> => {
            const result = await options.mintChurchToken();
            options.mark("chat:mint");

            if (!result.isSuccess && !churchTokenFromAction(result).isUnreachable) {
                report(classifyActionFailure(result.statusCode));
            }

            return churchTokenFromAction(result);
        },
        exchange: (churchToken: string) => exchange(churchToken),
        pushTokenToConnection: async (): Promise<void> => {
            await client?.realtime.setAuth();
        },
        random: Math.random,
        setTimer: (callback, milliseconds) => setTimeout(callback, milliseconds),
        clearTimer: handle => clearTimeout(handle as ReturnType<typeof setTimeout>)
    });

    /** Exchanges a church token for a platform token. */
    async function exchange(churchToken: string): Promise<ExchangeResult> {
        try {
            const response = await options.fetch(`${projectUrl}/functions/v1/token-exchange`, {
                method: "POST",
                headers: { "Authorization": `Bearer ${churchToken}`, "apikey": publishableKey }
            });
            const body = await response.json().catch(() => null) as TokenExchangeResponse | null;
            options.mark("chat:exchange");

            if (response.ok && body?.access_token && typeof body.expires_in === "number") {
                return { ok: true, accessToken: body.access_token, expiresInSeconds: body.expires_in };
            }

            return { ok: false, status: response.status, code: body?.error?.code ?? "auth.invalid_token" };
        }
        catch {
            return { ok: false, status: 0, code: "rpc.transport" };
        }
    }

    /** Calls a platform function, refreshing once if the platform says the token has expired. */
    async function call(name: string, args: Record<string, unknown>): Promise<RpcResult> {
        if (!client) {
            return { data: null, error: { message: "auth.expired" } };
        }

        const platform = client;
        return session.withFreshToken(
            async () => await platform.rpc(name, args),
            result => !!result.error && classifyPlatformError({ ...result.error, status: result.status }).code === "auth.expired"
        );
    }

    const timelines = createTimelines({
        fetchPage: async (channelId, page): Promise<HistoryPage> => {
            const result = await call("chat_get_history", { p_channel_id: channelId, p_limit: page.limit, p_before_id: page.beforeId ?? null });
            if (result.error) {
                throw { ...result.error, status: result.status };
            }
            return result.data as HistoryPage;
        }
    });

    const channels = createChannelStore({ reloadSidebar: () => void loadSidebar() });

    const tracker: ReadTracker = createReadTracker({
        save: createKeepaliveSave({ fetch: options.fetch, projectUrl, publishableKey, currentToken: session.currentToken }),
        onSaved: (channelId, result) => channels.applyMarkRead(channelId, result)
    });

    let localIds = 0;
    const sender = createSender({
        send: async (channelId, body) => {
            const result = await call("chat_send_message", { p_channel_id: channelId, p_body: body });
            if (result.error) {
                return { ok: false, error: classifyPlatformError({ ...result.error, status: result.status }) };
            }
            return { ok: true, id: result.data as number, createdAt: new Date().toISOString() };
        },
        timelines,
        personAliasGuid,
        newLocalId: () => `pending-${++localIds}`
    });

    /** Fetches the sidebar and replaces the rows with it. */
    async function loadSidebar(): Promise<SidebarRow[]> {
        const result = await call("chat_get_bootstrap", {});
        options.mark("chat:sidebar");

        if (result.error) {
            report(classifyPlatformError({ ...result.error, status: result.status }));
            throw result.error;
        }

        const rows = (result.data as SidebarRow[]) ?? [];
        channels.setSidebar(rows);
        return rows;
    }

    const storageKey = rememberedChannelKey(tenantId, personAliasGuid);

    /** Counts opens, so an open that a later one has replaced stops touching anything. */
    let openCount = 0;

    /**
     * Opens a channel: lets go of the one being left and saves its position without waiting,
     * then joins and fetches together. A person can open channels faster than the platform
     * answers; whatever an earlier open was waiting on, the channel chosen last is the one on
     * screen, joined, tracked and remembered.
     */
    async function open(channelId: string): Promise<OpenOutcome> {
        const thisOpen = ++openCount;
        const isCurrent = (): boolean => thisOpen === openCount;

        if (state.activeChannelId && state.activeChannelId !== channelId) {
            void tracker.leave();
        }

        state.activeChannelId = channelId;
        channels.setActive(channelId);

        const outcome = await openChannel({
            join: id => hub?.openChannel(id),
            loadNewest: async id => {
                await timelines.loadNewest(id);
                if (isCurrent()) {
                    options.mark("chat:history");
                    tracker.open(id, timelines.state(id).readCursor);
                }
            },
            isRefusal: error => classifyPlatformError(error).severity === "permission",
            remember: id => {
                if (isCurrent()) {
                    writeRememberedChannel(options.storage, storageKey, id);
                }
            }
        }, channelId);

        if (!isCurrent()) {
            return "superseded";
        }

        if (outcome !== "opened") {
            state.activeChannelId = null;
            channels.setActive(null);
        }

        return outcome;
    }

    return {
        state,
        channels,
        timelines,
        sender,

        start: async (): Promise<void> => {
            if (state.gate !== "ok") {
                state.phase = "refused";
                return;
            }

            if (!await session.start()) {
                state.gate = session.state.gate;
                state.phase = session.state.gate === "ok" ? "failed" : "refused";
                return;
            }

            client = options.createPlatformClient(projectUrl, publishableKey, async () => session.currentToken() ?? "");

            hub = createRealtimeHub({
                client,
                tenantId,
                personAliasGuid,
                onChannelEvent: (channelId, event, payload) => timelines.applyEvent(channelId, event, payload),
                onPersonalEvent: (event, payload) => channels.applyPersonalEvent(event, payload),
                onJoined: channelId => {
                    options.mark("chat:join");
                    timelines.onJoined(channelId).catch(error => report(classifyPlatformError(error)));
                },
                onStatus: error => state.connection = error
            });

            // The token is on the socket before the first join, which the hub does first.
            await hub.start();
            detachPage = tracker.attach(options.pageTargets);
            state.phase = "ready";

            await startPageLoad({
                linkedChannelId: options.linkedChannelId?.toLowerCase() ?? null,
                rememberedChannelId: readRememberedChannel(options.storage, storageKey),
                loadSidebar,
                open
            });
        },

        selectChannel: async (channelId: string): Promise<void> => {
            if (channelId !== state.activeChannelId) {
                await open(channelId);
            }
        },

        seen: (messageId: number): void => tracker.seen(messageId),

        firstMessageShown: (): void => {
            if (!isFirstMessageMarked) {
                isFirstMessageMarked = true;
                options.mark("chat:first-message");
            }
        },

        send: async (body: string): Promise<void> => {
            if (state.activeChannelId) {
                await sender.send(state.activeChannelId, body);
            }
        },

        loadOlder: async (): Promise<void> => {
            if (state.activeChannelId) {
                await timelines.loadOlder(state.activeChannelId).catch(error => report(classifyPlatformError(error)));
            }
        },

        dismissError: (code: string): void => {
            const index = state.errors.findIndex(e => e.code === code);
            if (index >= 0) {
                state.errors.splice(index, 1);
            }
        },

        stop: async (): Promise<void> => {
            await tracker.leave();
            detachPage?.();
            session.stop();
            await hub?.stop();
        }
    };
}
