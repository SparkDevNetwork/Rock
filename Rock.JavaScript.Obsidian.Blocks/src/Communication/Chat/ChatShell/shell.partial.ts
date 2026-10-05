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
import { CallResult, createPlatformCall, exchangeChurchToken } from "./platformCall.partial";
import { createTimelines, Timelines } from "./composables/useHistory.partial";
import { createKeepaliveSave, createReadTracker, PageEventTargets, ReadTracker } from "./composables/useMarkRead.partial";
import { createRealtimeHub, RealtimeClientLike, RealtimeHub, realtimeOptions } from "./composables/useRealtimeHub.partial";
import { createSender, isComposerOpen, Sender } from "./composables/useSend.partial";
import { ChatSession, ChurchTokenResult, createSession, ExchangeResult, serviceBanner } from "./composables/useSession.partial";
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
import { ChannelUnreadEvent, ChatError, HistoryPage, SendAnswer, SidebarRow } from "./types.partial";

/**
 * The part of the platform client the shell uses: its live connection. Every call goes through
 * the shell's one call path instead, so the server can route it.
 */
export type PlatformClientLike = RealtimeClientLike;

/** The realtime client's options: the pinned protocol and the reconnect waits. */
export type RealtimeOptions = ReturnType<typeof realtimeOptions>;

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
    createPlatformClient: (url: string, key: string, accessToken: () => Promise<string>, realtime: RealtimeOptions) => PlatformClientLike;

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

    /** The platform's banner for the service state, or null when there is none to show. */
    banner: string | null;

    /** Whether the composer takes a message in the service state the platform last named. */
    isComposerOpen: boolean;
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
            return "Chat needs your birthdate before it can open.";
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
 * The sentence shown when chat does not open.
 *
 * @param phase Where the shell stopped.
 * @param gate The gate that refused, when one did.
 *
 * @returns The sentence.
 */
export function refusalMessage(phase: ShellState["phase"], gate: string | null): string {
    // Failed means Rock's gates passed and the platform did not finish signing in, so the
    // sentence is about chat not starting rather than about the person.
    return gateMessage(phase === "failed" ? "gate_unavailable" : gate);
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
 * A fresh identifier for a pending row. It is also the send's key, which the platform stores as
 * a UUID, so a browser without randomUUID still gets one of that shape.
 *
 * @returns A version 4 UUID.
 */
function newLocalId(): string {
    if (typeof globalThis.crypto?.randomUUID === "function") {
        return globalThis.crypto.randomUUID();
    }

    return "xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx".replace(/[xy]/g, c => {
        const value = Math.floor(Math.random() * 16);
        return (c === "x" ? value : (value & 0x3) | 0x8).toString(16);
    });
}

/**
 * Creates the shell.
 *
 * @param options Everything the shell reaches outside itself.
 *
 * @returns The shell.
 */
export function createChatShell(options: ShellOptions): ChatShell {
    const state = reactive<ShellState>({
        phase: "starting",
        gate: options.session.gate ?? null,
        connection: null,
        errors: [],
        activeChannelId: null,
        banner: null,
        isComposerOpen: true
    }) as ShellState;
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
            showSettings();
            await client?.realtime.setAuth();
        },
        random: Math.random,
        setTimer: (callback, milliseconds) => setTimeout(callback, milliseconds),
        clearTimer: handle => clearTimeout(handle as ReturnType<typeof setTimeout>),
        onEnded: () => void end()
    });

    /**
     * The longest a viewer waits before fetching a new token when the channel on screen changed,
     * an estimate. Everyone watching the channel is told at once, and the wait spreads their token
     * requests at Rock and their join checks at Realtime.
     */
    const channelChangedWaitMs = 5_000;

    /** Puts the session's latest settings on the page: the banner and whether the composer is open. */
    function showSettings(): void {
        const settings = session.settings();
        state.banner = serviceBanner(settings);
        state.isComposerOpen = isComposerOpen(settings.service.state);
    }

    /** The pending wait before a recheck, so a stopped or ended shell can cancel it. */
    let recheckTimer: ReturnType<typeof setTimeout> | null = null;

    /** Set once the shell is stopped or its session has ended; the page load goes no further. */
    let isStopped = false;

    /**
     * Asks again whether the person may still read what they have open. Realtime keeps a join's
     * answer until the socket is handed a new token, so a new token is fetched and handed over;
     * Realtime then closes whatever the person has lost. Refreshes already running are shared.
     */
    function recheck(): void {
        void session.refresh();
    }

    /** Cancels a recheck still waiting. */
    function cancelRecheck(): void {
        if (recheckTimer !== null) {
            clearTimeout(recheckTimer);
            recheckTimer = null;
        }
    }

    /**
     * Leaves every topic once the session holds no token, because Rock refused a refresh or the
     * token ran out, and shows why. Realtime would close the topics at the token's expiry anyway.
     */
    async function end(): Promise<void> {
        isStopped = true;
        cancelRecheck();
        session.stop();
        state.gate = session.state.gate;
        state.phase = session.state.gate === "ok" ? "failed" : "refused";
        await hub?.stop();
    }

    /** Exchanges a church token for a platform token and the settings beside it. */
    async function exchange(churchToken: string): Promise<ExchangeResult> {
        const answer = await exchangeChurchToken(options.fetch, projectUrl, publishableKey, churchToken);
        options.mark("chat:exchange");

        return answer;
    }

    const platform = createPlatformCall({
        fetch: options.fetch,
        projectUrl,
        publishableKey,
        currentToken: session.currentToken,
        routes: () => session.settings().routes
    });

    /**
     * Calls a platform function, refreshing once if the platform says the token has expired. A
     * refusal for read only or maintenance puts its banner on the page at once, since the state
     * changed after the last exchange.
     */
    async function call(name: string, args: Record<string, unknown>): Promise<CallResult> {
        const result = await session.withFreshToken(
            () => platform.call(name, args),
            answer => !answer.ok && answer.error.code === "auth.expired"
        );

        if (!result.ok && (result.error.code === "rpc.read_only" || result.error.code === "rpc.maintenance")) {
            session.applyServiceRefusal(result.error);
            showSettings();
        }

        return result;
    }

    /** A refusal in the shape the classifier reads, for callers that throw it on. */
    function thrown(result: Extract<CallResult, { ok: false }>): unknown {
        return { message: result.error.code, hint: result.error.text, status: result.status };
    }

    const timelines = createTimelines({
        fetchPage: async (channelId, page): Promise<HistoryPage> => {
            const result = await call("chat_get_history", {
                p_channel_id: channelId,
                p_limit: page.limit,
                p_before_id: page.beforeId ?? null,
                p_after_id: page.afterId ?? null
            });
            if (!result.ok) {
                throw thrown(result);
            }
            return result.data as HistoryPage;
        },
        catchUp: () => ({ page: session.settings().limits.catch_up_page, max: session.settings().limits.catch_up_max })
    });

    const channels = createChannelStore({ reloadSidebar: () => void loadSidebar() });

    const tracker: ReadTracker = createReadTracker({
        save: createKeepaliveSave({ fetch: options.fetch, projectUrl, publishableKey, currentToken: session.currentToken }),
        onSaved: (channelId, result) => channels.applyMarkRead(channelId, result)
    });

    const sender = createSender({
        send: async (channelId, body, key) => {
            const result = await call("chat_send_message", { p_channel_id: channelId, p_body: body, p_client_key: key ?? null });
            if (!result.ok) {
                return { ok: false, error: result.error };
            }
            const sent = (result.data ?? {}) as Partial<SendAnswer>;
            return { ok: true, id: sent.id as number, createdAt: sent.created_at ?? new Date().toISOString(), notice: sent.notice ?? null };
        },
        timelines,
        personAliasGuid,
        newLocalId
    });

    /** Fetches the sidebar and replaces the rows with it. */
    async function loadSidebar(): Promise<SidebarRow[]> {
        const result = await call("chat_get_bootstrap", {});
        options.mark("chat:sidebar");

        if (!result.ok) {
            report(result.error);
            throw thrown(result);
        }

        const rows = (result.data as { channels?: SidebarRow[] } | null)?.channels ?? [];
        channels.setSidebar(rows);
        return rows;
    }

    /**
     * The platform says what this client holds may be out of date in a way no other event
     * describes, so the sidebar and the open channel are read again. Nothing is reminted: what the
     * person may read has not changed, only what is on screen.
     */
    function resync(): void {
        void loadSidebar().catch(() => undefined);
        if (state.activeChannelId) {
            void timelines.loadNewest(state.activeChannelId).catch(error => report(classifyPlatformError(error)));
        }
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

            // Stopped while signing in, or ended by then: nothing is joined.
            if (isStopped) {
                return;
            }

            showSettings();
            client = options.createPlatformClient(projectUrl, publishableKey, async () => session.currentToken() ?? "",
                realtimeOptions(session.settings().limits, Math.random));

            hub = createRealtimeHub({
                client,
                tenantId,
                personAliasGuid,
                onChannelEvent: (channelId, event, payload) => {
                    if (event === "resync") {
                        resync();
                        return;
                    }
                    if (event === "channel.changed") {
                        if (channelId === state.activeChannelId) {
                            cancelRecheck();
                            recheckTimer = setTimeout(recheck, Math.random() * channelChangedWaitMs);
                        }
                        return;
                    }
                    timelines.applyEvent(channelId, event, payload);
                },
                onPersonalEvent: (event, payload) => {
                    if (event === "resync") {
                        resync();
                        return;
                    }
                    channels.applyPersonalEvent(event, payload);

                    // Only the channel on screen is joined, so only a change to it can need a cut.
                    const changed = (payload as Partial<Record<keyof ChannelUnreadEvent, unknown>> | null)?.channel_id;
                    if (event === "session.recheck" || (event === "membership.changed" && changed === state.activeChannelId)) {
                        recheck();
                    }
                },
                onJoined: channelId => {
                    options.mark("chat:join");
                    timelines.onJoined(channelId).catch(error => report(classifyPlatformError(error)));
                },
                onUnknownError: () => {
                    // Realtime words this client does not know may be a lost read it was not built
                    // to recognise. A new token makes Realtime decide again, and the sidebar read
                    // again shows what the person may still see; the refresh is shared with any
                    // already running.
                    recheck();
                    void loadSidebar().catch(() => undefined);
                },
                onStatus: error => {
                    // The hub has already left a channel whose read was revoked; the person is told,
                    // and the list is loaded again without it.
                    if (error?.code === "rt.read_revoked") {
                        // Nothing is open any more, so choosing the channel again, once the
                        // person may read it, opens it afresh.
                        // An open of it still loading is superseded, so it cannot take it back.
                        openCount++;
                        void tracker.leave();
                        state.activeChannelId = null;
                        channels.setActive(null);
                        report(error);
                        void loadSidebar();
                        return;
                    }
                    state.connection = error;
                }
            });

            // The token is on the socket before the first join, which the hub does first.
            await hub.start();
            if (isStopped) {
                return;
            }
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
            isStopped = true;
            cancelRecheck();

            // Stopped before the last save, so no refresh starts while it goes out; the token is
            // kept, and the save still carries it.
            session.stop();
            await tracker.leave();
            detachPage?.();
            await hub?.stop();
        }
    };
}
