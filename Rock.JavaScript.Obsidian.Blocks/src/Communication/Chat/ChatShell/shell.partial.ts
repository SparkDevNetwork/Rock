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
// the sender, the read tracker, the threads, the mentions, the rooms, the person's settings and the
// sidebar, and joins them. Each of those is its own tested
// module; what is here decides only which one hears what.
//
// Each step marks the page's performance timeline (chat:mint, chat:exchange, chat:history,
// chat:join, chat:sidebar, chat:first-message), so the time from opening the page to the first
// message on screen can be split into the steps that make it up.
import { reactive } from "vue";
import { classifyActionFailure, classifyPlatformError } from "./errors.partial";
import { CallResult, computeBackoff, createPlatformCall, exchangeChurchToken } from "./platformCall.partial";
import { createDirectMessages, DirectMessages, DoorResult, isNewMessageShown } from "./composables/useDirectMessage.partial";
import { createTimelines, isTimelineMessage, Timelines } from "./composables/useHistory.partial";
import { createKeepaliveSave, createReadTracker, PageEventTargets, ReadTracker } from "./composables/useMarkRead.partial";
import { createRealtimeHub, RealtimeClientLike, RealtimeHub, realtimeOptions } from "./composables/useRealtimeHub.partial";
import { createPushRegistration, PushDependencies, PushRegistration, PushState } from "./composables/usePush.partial";
import { createPrefs, MeSettings, PersonSettingKey, PersonSettingResult, Prefs } from "./composables/usePrefs.partial";
import { canMakeSendKey, createSender, isComposerOpen, JoinResult, newLocalId, noKeySentence, Sender } from "./composables/useSend.partial";
import { ChatSession, ChatSettings, ChurchTokenResult, createSession, ExchangeResult, PlatformLimits, readSettings, serviceBanner } from "./composables/useSession.partial";
import { createThreads, Threads } from "./composables/useThreads.partial";
import { ActionNotice, createMessageActions, MessageActions, quoteKey } from "./composables/useMessageActions.partial";
import { createReactions, Reactions } from "./composables/useReactions.partial";
import { createMentions, Mentions, MentionsDependencies } from "./composables/useMentions.partial";
import { ChannelFactsResult, createRooms, DirectoryPageResult, DirectoryRequest, PersonProfileResult, Rooms } from "./composables/useRooms.partial";
import { createMembers, MemberDoors, Members } from "./composables/useMembers.partial";
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
import { ChannelFacts, ChannelUnreadEvent, ChatBadge, ChatError, DirectoryPage, FindDmAnswer, HistoryPage, MarkReadResult, MessageCreatedEvent, PeopleSearchAnswer, PersonProfile, ReactionFavorite, SendAnswer, SidebarRow, TimelineMessage, UnreadMention } from "./types.partial";

/**
 * The part of the platform client the shell uses: its live connection. Every call goes through
 * the shell's one call path instead, so the server can route it.
 */
export type PlatformClientLike = RealtimeClientLike;

/** The realtime client's options: the pinned protocol and the reconnect waits. */
export type RealtimeOptions = ReturnType<typeof realtimeOptions>;

/** How many of the found conversation's newest messages New message shows, as the design draws them. */
const foundHistoryLimit = 16;

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
        canStartDm?: boolean | null;
        isChatAdministrator?: boolean | null;
    };

    /** The channel a link named, or null. */
    linkedChannelId: string | null;

    /** Asks Rock for a church token. */
    mintChurchToken: () => Promise<MintActionResult>;

    /** Asks Rock's door for the conversation with these people; without it, none can start. */
    startDirectMessage?: (personAliasGuids: string[]) => Promise<DoorResult>;

    /** Asks Rock's door to join a public or pinned room; without it, a send never joins. */
    joinChannel?: (channelId: string) => Promise<JoinResult>;

    /** Asks Rock's door to take the person out of a room; without it, Leave is refused. */
    leaveChannel?: (channelId: string) => Promise<JoinResult>;

    /** Asks Rock's door to rename a group; a blank name goes back to its people's names. Without it, Rename is refused. */
    renameConversation?: (channelId: string, name: string) => Promise<JoinResult>;

    /** Asks Rock whether the person may add and remove people in a room; without it, they may not. */
    getMemberRights?: MemberDoors["getRights"];

    /** Asks Rock's door to add people to a room; without it, nobody is added. */
    addMembers?: MemberDoors["addMembers"];

    /** Asks Rock's door to remove someone from a room, naming who leads next for a last leader. */
    removeMember?: MemberDoors["removeMember"];
    /** Asks Rock's door to save one of the person's two privacy switches; without it, they are refused. */
    savePersonSetting?: (setting: PersonSettingKey, value: boolean) => Promise<PersonSettingResult>;

    /**
     * Whether the page narrows the sidebar's pinned rooms and the people search to the person's
     * campus, as its block setting says.
     */
    areSharedChannelsFilteredByCampus?: boolean;

    /** Creates the platform client, whose every request reads the token through the callback. */
    createPlatformClient: (url: string, key: string, accessToken: () => Promise<string>, realtime: RealtimeOptions) => PlatformClientLike;

    fetch: (url: string, init: RequestInit) => Promise<Response>;
    storage: StorageLike | null;
    pageTargets: PageEventTargets;
    mark: (name: string) => void;

    /** The browser's push, or nothing where this browser has none. */
    push?: Omit<PushDependencies, "settings" | "call" | "offFlag">;

    /** Loads the page again; the browser's own reload when not given. */
    reloadPage?: () => void;

    /** Shows a toast that is not a failure, such as a message deleted or text copied. */
    notify?: (notice: ActionNotice) => void;

    /**
     * How a jump from the mentions inbox reaches the frame's views, which the shell cannot see:
     * landing on a message in the open conversation, and opening a thread at a reply.
     */
    mentionViews?: Pick<MentionsDependencies, "land" | "openThread">;
};

/**
 * The person a platform token names, or null where it names none this client can read. The
 * token's signature is the platform's to check; this only reads whom it is for.
 */
export function tokenPerson(token: string | null): string | null {
    const payload = token?.split(".")[1];
    if (!payload) {
        return null;
    }

    try {
        const claims = JSON.parse(atob(payload.replace(/-/g, "+").replace(/_/g, "/")));
        return typeof claims?.sub === "string" ? claims.sub.toLowerCase() : null;
    }
    catch {
        // Not a token this client can read, so it says nothing about whom it is for.
        return null;
    }
}

/** What the page shows. */
export type ShellState = {
    /** Where the shell has got to. */
    phase: "starting" | "ready" | "refused" | "failed";

    /**
     * Whether the page load is still choosing the first conversation, which it does after the
     * shell is ready, so the page draws a loading state rather than asking the person to choose.
     */
    isOpening: boolean;

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

    /** The latest service state and its own banner, independent of an older sidebar row. */
    service: ChatSettings["service"];

    /** Whether the banner says this Rock is too old for the platform, which is told in its own tone. */
    isUpdateRequired: boolean;

    /** Whether the composer takes a message in the service state the platform last named. */
    isComposerOpen: boolean;

    /** The platform's limits as the latest token exchange answered them, which the screens draw. */
    limits: PlatformLimits;

    /** Whether the church's prompt timing would offer push now. */
    isPushOffered: boolean;

    /** This browser's push as Chat settings draws it. */
    push: PushState;

    /**
     * Which of the panes that draw the open conversation and the open thread are out of view, as
     * when a phone shows its list in front; written by the frame, which alone knows its panes.
     */
    hiddenPanes: { conversation: boolean; thread: boolean };
};

/** The shell a block holds. */
export type ChatShell = {
    state: ShellState;
    channels: ChannelStore;
    timelines: Timelines;
    sender: Sender;
    directMessages: DirectMessages;

    /** The threads list, the open thread and the live counts of thread roots. */
    threads: Threads;

    /** Editing, deleting, copying and quoting a message, and the actions the server lists on it. */
    messageActions: MessageActions;

    /** Reacting to messages: the toggle, the live events, the catalog, the recents and who reacted. */
    reactions: Reactions;

    /** The @ picker's search, the mentions inbox and the header's dot. */
    mentions: Mentions;

    /** Browse channels, each open room's facts, Join, and a previewed room's sidebar row. */
    rooms: Rooms;
    /** The members panel's roster and its search. */
    members: Members;

    /** Each room's notify mode, star and hide, and the person's own switches. */
    prefs: Prefs;

    /** Whether the sidebar shows the New message button. */
    isNewMessageShown: boolean;

    /** Signs in and runs the page load. */
    start: () => Promise<void>;

    /** Opens a channel the person chose. */
    selectChannel: (channelId: string) => Promise<void>;

    /** A message of the open channel was on screen. */
    seen: (messageId: number) => void;

    /** The person asked to mark the open channel read up to a message: saved at once. */
    markRead: (messageId: number) => void;

    /** The first message of the page load reached the screen. */
    firstMessageShown: () => void;

    /** Sends a message to the open channel, or a draft's first message. */
    send: (body: string) => Promise<void>;

    /**
     * Sends a reply to a root: in the open thread's conversation when the root is the open
     * thread's, else in the open conversation. Also shown in the conversation when asked.
     */
    sendReply: (rootId: number, body: string, isShownInChannel: boolean) => Promise<void>;

    /** The person turned on notifications for this browser. */
    turnOnPush: () => Promise<void>;

    /** The person turned off notifications for this browser. */
    turnOffPush: () => Promise<void>;

    /** Whether a channel is the one on screen while the page is visible, for the push worker. */
    isChannelOnScreen: (channelId: string) => boolean;

    /** Whether a thread is the one open in the drawer while the page is visible, for the push worker. */
    isThreadOnScreen: (rootId: number) => boolean;

    /** The unread mentions in the rooms the sidebar lists, for the app badge. */
    mentionBadge: () => number;

    /** Fetches older messages of the open channel. */
    loadOlder: () => Promise<void>;

    /** Dismisses the failure the person closed. */
    dismissError: (error: ChatError) => void;

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
 * Creates the shell.
 *
 * @param options Everything the shell reaches outside itself.
 *
 * @returns The shell.
 */
export function createChatShell(options: ShellOptions): ChatShell {
    const state = reactive<ShellState>({
        phase: "starting",
        isOpening: true,
        gate: options.session.gate ?? null,
        connection: null,
        errors: [],
        activeChannelId: null,
        banner: null,
        service: { state: "normal", banner: null },
        isUpdateRequired: false,
        isComposerOpen: true,
        // the platform's, from the first exchange; nothing is drawn from them before it answers
        limits: readSettings(undefined).limits,
        isPushOffered: false,
        // known once push starts after the first channel is on screen
        push: "unavailable",
        hiddenPanes: { conversation: false, thread: false }
    }) as ShellState;
    const projectUrl = (options.session.projectUrl ?? "").replace(/\/+$/, "");
    const publishableKey = options.session.publishableKey ?? "";
    const tenantId = options.session.tenantId ?? "";
    const personAliasGuid = options.session.personAliasGuid ?? "";

    let client: PlatformClientLike | null = null;
    let hub: RealtimeHub | null = null;
    let detachPage: (() => void) | null = null;
    let detachThreads: (() => void) | null = null;
    let push: PushRegistration | null = null;
    let isFirstMessageMarked = false;

    /**
     * Whether a failure is the connection itself: a call that never reached the platform. While
     * the connection is lost the banner already says so, and a toast beside it would outlive it.
     */
    function isConnectionFailure(error: ChatError): boolean {
        return error.code === "rpc.transport";
    }

    /** Tells the person about a failure, once per code until it is dismissed. */
    function report(error: ChatError): void {
        if (state.connection && isConnectionFailure(error)) {
            return;
        }

        // The same failure twice shows once; two refused messages under one code are two
        // different sentences, so the text is part of what makes an error the same one.
        if (!state.errors.some(e => e.code === error.code && e.text === error.text)) {
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

            // A token for someone other than the person the page loaded for means a merge made
            // them part of another person. The page listens on a personal topic that is no longer
            // theirs, so it loads again rather than move every topic under a live connection.
            const person = tokenPerson(session.currentToken());
            if (person !== null && personAliasGuid !== "" && person !== personAliasGuid.toLowerCase()) {
                (options.reloadPage ?? (() => globalThis.location?.reload()))();
                return;
            }

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
        state.service = { ...settings.service };
        state.isUpdateRequired = settings.client.update_required;
        state.isComposerOpen = isComposerOpen(settings.service.state);
        state.limits = settings.limits;
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
        cancelSidebarWait();
        timelines.stop();
        threads.stop();
        messageActions.stop();
        reactions.stop();
        mentions.stop();
        rooms.stop();
        members.stop();
        prefs.stop();
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
    async function call(name: string, args: Record<string, unknown>, callOptions?: { keepalive?: boolean }): Promise<CallResult> {
        const result = await session.withFreshToken(
            () => platform.call(name, args, callOptions),
            answer => !answer.ok && answer.error.code === "auth.expired"
        );

        if (!result.ok && (result.error.code === "rpc.read_only" || result.error.code === "rpc.maintenance")) {
            session.applyServiceRefusal(result.error);
            showSettings();
        }

        if (result.ok) {
            reportPage(result.data);
        }

        return result;
    }

    /**
     * Tells the sender of every keyed message in a page, whether or not a view keeps the page: a
     * page dropped because its thread was left, or its channel was left, still shows the message
     * was posted. It waits a turn, so a page a view does keep has already put the message in
     * place, and the pending row is never taken out before the message that replaces it is in.
     */
    function reportPage(data: unknown): void {
        const messages = (data as { messages?: unknown } | null)?.messages;
        if (!Array.isArray(messages)) {
            return;
        }

        const keyed = (messages as TimelineMessage[]).filter(m => typeof m?.id === "number" && !!m.client_key);
        if (keyed.length > 0) {
            // The turn can end after the shell stopped, when nothing it settles is on screen.
            setTimeout(() => {
                if (!isStopped) {
                    keyed.forEach(m => sender.landed(m));
                }
            }, 0);
        }
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
        limits: () => session.settings().limits,
        catchUp: () => ({ page: session.settings().limits.catch_up_page, max: session.settings().limits.catch_up_max }),
        retryDelay: () => ({ baseMs: session.settings().limits.reconnect_base_ms, capMs: session.settings().limits.reconnect_cap_ms }),
        // whichever copy of the person's send lands first, from a page, live or its answer, settles it
        onLanded: message => sender.landed(message),
        // A reaction moves the conversation's copy of its message when the timeline lets it
        // through, held like any other change until the channel's first page is in; the open
        // thread's copies are moved as the event arrives.
        applyReaction: (channelId, event, payload) => reactions.applyEvent(channelId, event, payload)
    });

    // A reload asked while a rejoin's wait is pending is the one that wait will make. A failed
    // read is reported where it fails, so nothing is left to catch here or at any other reload.
    const channels = createChannelStore({
        reloadSidebar: () => sidebarWait === null ? void loadSidebar().catch(() => undefined) : undefined,
        // A first visit's page load waits on the sidebar to choose its conversation, so a retry
        // with nothing open runs the page load again; with a conversation open, only the list is owed.
        retryFirstLoad: () => state.activeChannelId === null ? void runPageLoad() : void loadSidebar().catch(() => undefined)
    });

    /** A save of the read position, applied to the row and the unread mentions. */
    function applySave(channelId: string, result: MarkReadResult, sentAt: number | undefined): void {
        channels.applyMarkRead(channelId, result, sentAt);
        // a mention at or below the saved cursor has been read where it lives
        if (result.read_cursor !== null) {
            mentions.channelRead(channelId, result.read_cursor);
        }
    }

    const keepaliveSave = createKeepaliveSave({ fetch: options.fetch, projectUrl, publishableKey, currentToken: session.currentToken });
    // When each save set out, by its answer, so the row can tell a message that reached it since
    // from one the save already ran past
    const savesSentAt = new WeakMap<MarkReadResult, number>();
    const tracker: ReadTracker = createReadTracker({
        save: async (channelId, messageId) => {
            // A room the person is only previewing has no read position of theirs to keep, until
            // they join it; a pinned room they have not joined is theirs to read, so it keeps one.
            if (channels.previewChannelId === channelId && !channels.rows.some(row => row.channel_id === channelId && row.is_member)) {
                return null;
            }

            const sentAt = channels.saveSent();
            const result = await keepaliveSave(channelId, messageId);
            if (result) {
                savesSentAt.set(result, sentAt);
            }
            return result;
        },
        onSaved: (channelId, result) => {
            const sentAt = savesSentAt.get(result);
            applySave(channelId, result, sentAt);
            sidebarJournal?.push({ channelId, result, sentAt });
        }
    });

    // a reply shown only in its thread lands nowhere else, so the open thread settles its send
    const threads = createThreads({ call, timelines, personAliasGuid, limits: () => session.settings().limits, onLanded: message => sender.landed(message) });

    const messageActions = createMessageActions({
        call,
        timelines,
        threads,
        personAliasGuid,
        notify: notice => options.notify?.(notice),
        // the clipboard is only there on a secure page; without it copy says it isn't available
        writeText: globalThis.navigator?.clipboard ? text => globalThis.navigator.clipboard.writeText(text) : undefined
    });

    // The favorites come with the sidebar, after the reactions are made, so the list is filled in place.
    const favorites = reactive<ReactionFavorite[]>([]);

    const reactions = createReactions({
        call,
        timelines,
        threads,
        personAliasGuid,
        notify: notice => options.notify?.(notice),
        favorites,
        churchKey: tenantId,
        storage: options.storage
    });

    const mentions = createMentions({
        call,
        tenantId,
        personAliasGuid,
        // A browser that blocks its storage refuses here, and the mentions only forget the visit.
        storage: () => {
            if (!options.storage) {
                throw new Error("The browser's storage is not available.");
            }

            return options.storage;
        },
        timelines,
        limits: () => session.settings().limits,
        // A jump opens its conversation as a sidebar row does, and one already open stays as it is.
        openChannel: async channelId => {
            directMessages.close();
            return channelId === state.activeChannelId || await open(channelId) === "opened";
        },
        activeChannelId: () => state.activeChannelId,
        readCursor: channelId => channels.rows.find(row => row.channel_id === channelId)?.read_cursor ?? null,
        land: messageId => options.mentionViews?.land(messageId),
        openThread: async (channelId, rootId, messageId) => options.mentionViews?.openThread(channelId, rootId, messageId),
        toast: notice => options.notify?.(notice)
    });

    /** One room's facts, read when it opens and again on its membership change. */
    async function getChannel(channelId: string): Promise<ChannelFactsResult> {
        const result = await call("chat_get_channel", { p_channel_id: channelId });
        return result.ok ? { ok: true, facts: result.data as ChannelFacts } : { ok: false, error: result.error };
    }

    /**
     * A page of Browse channels. The church's own campus limit goes with every page, as it goes
     * with the people search and the sidebar, so a room hidden there is hidden here too.
     */
    async function listDirectory(asked: DirectoryRequest): Promise<DirectoryPageResult> {
        const result = await call("chat_list_directory", {
            p_query: asked.query,
            p_campus_only: options.areSharedChannelsFilteredByCampus ?? false,
            p_campus_id: asked.campusId,
            p_whole_church: asked.wholeChurch,
            p_not_joined: asked.notJoined,
            p_after: asked.after,
            p_limit: asked.limit
        });
        if (!result.ok) {
            return { ok: false, error: result.error };
        }

        const page = (result.data ?? {}) as Partial<DirectoryPage>;
        return { ok: true, page: { channels: page.channels ?? [], total: page.total ?? 0 } };
    }

    /** One person for the profile card, as the room the card was opened from shows them. */
    async function getPerson(personAliasGuid: string, channelId: string): Promise<PersonProfileResult> {
        const result = await call("chat_get_person", { p_person_alias_guid: personAliasGuid, p_channel_id: channelId });
        return result.ok ? { ok: true, person: result.data as PersonProfile } : { ok: false, error: result.error };
    }

    const rooms = createRooms({
        getChannel,
        listDirectory,
        getPerson,
        joinChannel: options.joinChannel,
        leaveChannel: options.leaveChannel,
        renameConversation: options.renameConversation,
        getMemberRights: options.getMemberRights,
        // The next conversation after a leave opens as a sidebar row does.
        openChannel: channelId => {
            directMessages.close();
            if (channelId !== state.activeChannelId) {
                void open(channelId);
            }
        },
        channels,
        // a reload asked while a rejoin's wait is pending is the one that wait will make
        reloadSidebar: () => sidebarWait === null ? void loadSidebar().catch(() => undefined) : undefined,
        limits: () => session.settings().limits,
        activeChannelId: () => state.activeChannelId,
        // a lost connection is told by its banner, and a door asked then would not be reached
        isOffline: () => state.connection !== null,
        isCampusLimited: options.areSharedChannelsFilteredByCampus ?? false,
        notify: notice => options.notify?.(notice)
    });
    const members = createMembers({
        call,
        doors: { getRights: options.getMemberRights, addMembers: options.addMembers, removeMember: options.removeMember },
        // a lost connection is told by its banner, and a door asked then would not be reached
        isOffline: () => state.connection !== null,
        notify: notice => options.notify?.(notice),
        titleOf: channelId => channels.rows.find(r => r.channel_id === channelId)?.title ?? "",
        isChatAdministrator: options.session.isChatAdministrator ?? false,
        campusOnly: options.areSharedChannelsFilteredByCampus ?? false,
        limits: () => session.settings().limits
    });

    const prefs = createPrefs({
        setPrefs: async (channelId, changes) => {
            const result = await call("chat_set_prefs", { p_channel_id: channelId, p_changes: changes });
            return result.ok ? { ok: true, data: result.data } : { ok: false, error: result.error };
        },
        savePersonSetting: options.savePersonSetting,
        channels,
        isOffline: () => state.connection !== null,
        notify: notice => options.notify?.(notice)
    });

    const sender = createSender({
        send: async (channelId, body, key, reply, quotedMessageId) => {
            const result = await call("chat_send_message", {
                p_channel_id: channelId,
                p_body: body,
                p_client_key: key ?? null,
                ...(reply ? { p_parent_id: reply.parentId, p_shown_in_channel: reply.isShownInChannel } : {}),
                ...(typeof quotedMessageId === "number" ? { p_quoted_message_id: quotedMessageId } : {})
            });
            if (!result.ok) {
                return { ok: false, error: result.error };
            }
            const sent = (result.data ?? {}) as Partial<SendAnswer>;
            return {
                ok: true,
                id: sent.id as number,
                createdAt: sent.created_at ?? new Date().toISOString(),
                notice: sent.notice ?? null,
                pieces: sent.pieces ?? null,
                blocks: sent.blocks ?? null
            };
        },
        timelines,
        onReplyConfirmed: (channelId, message) => threads.upsertReply(channelId, message),
        join: options.joinChannel,
        waitBeforeResend: attempt => new Promise(resolve => setTimeout(resolve,
            computeBackoff(attempt, session.settings().limits.reconnect_base_ms, session.settings().limits.reconnect_cap_ms, Math.random))),
        personAliasGuid,
        newLocalId
    });

    /**
     * Whether a send can be keyed, saying why not when it cannot. The sender makes no row without
     * a key, so no failed row in the feed would explain it; the composer keeps the text.
     */
    function isSendKeyed(): boolean {
        if (canMakeSendKey()) {
            return true;
        }
        report({ code: "send.no_key", severity: "failed", text: noKeySentence });
        return false;
    }

    /**
     * Uses up the quote waiting under a key, for the send about to go. The strip leaves with the
     * text; the quote stays on the send's row, so every retry of it names the same message.
     */
    function takeQuote(key: string): number | undefined {
        const quoted = messageActions.state.quotes[key];
        messageActions.cancelQuote(key);
        return quoted?.id;
    }

    /** Counts a send toward offering notifications, which waits for the person's first send. */
    function notePushSend(): void {
        push?.noteSend();
        state.isPushOffered = push?.isOffered() ?? false;
    }

    const directMessages = createDirectMessages({
        search: async query => {
            const result = await call("chat_search_people", { p_query: query, p_limit: session.settings().limits.people_search_size, p_campus_only: options.areSharedChannelsFilteredByCampus ?? false });
            return result.ok
                ? { ok: true, people: (result.data as Partial<PeopleSearchAnswer> | null)?.people ?? [] }
                : { ok: false, error: result.error };
        },
        findConversation: async personAliasGuids => {
            const result = await call("chat_find_dm", { p_person_alias_guids: personAliasGuids });
            return result.ok
                ? { ok: true, channelId: (result.data as Partial<FindDmAnswer> | null)?.channel_id ?? null }
                : { ok: false, error: result.error };
        },
        // New message shows the conversation found as a read-only look at its newest messages.
        recentMessages: async channelId => {
            const result = await call("chat_get_history", { p_channel_id: channelId, p_limit: foundHistoryLimit, p_before_id: null, p_after_id: null });
            if (!result.ok) {
                throw thrown(result);
            }
            return [...((result.data as Partial<HistoryPage> | null)?.messages ?? [])].sort((a, b) => a.id - b.id);
        },
        startConversation: async personAliasGuids => options.startDirectMessage
            ? options.startDirectMessage(personAliasGuids)
            : { code: "door.unavailable", channelGuid: null, isPending: false, message: "A conversation cannot be started here.", personAliasGuid: null },
        // Only an open that put the conversation on screen lets a draft go; a refused one keeps
        // the person on the draft with their text.
        openChannel: async (channelId, startedWith) => {
            if (startedWith) {
                rooms.conversationStarted(channelId, startedWith);
            }
            return (await open(channelId)) === "opened";
        },
        send: async (channelId, body, attempt) => {
            const isSent = await sender.send(channelId, body, attempt);
            notePushSend();
            return isSent;
        },
        retryDelay: () => ({ baseMs: session.settings().limits.reconnect_base_ms, capMs: session.settings().limits.reconnect_cap_ms }),
        limits: () => session.settings().limits,
        report
    });

    // A sidebar read is a snapshot taken some time after it is asked for. Each read takes the next
    // generation, and only the newest asked for may land; every change that arrives while it is
    // out is applied at once and kept here, then replayed over its answer, which may predate it.
    // Null while no read is out.
    let sidebarGeneration = 0;

    // The newest read's answer, which a read it overtook answers with, so a caller waiting on the
    // sidebar, the page load above all, always gets the sidebar that landed.
    let newestSidebarRead: Promise<SidebarRow[]> | null = null;
    let sidebarJournal: Array<{ event: string, payload: unknown } | { channelId: string, result: MarkReadResult, sentAt: number | undefined }> | null = null;

    /** The live signals that state a row's value, and so can be replayed over a sidebar's answer. */
    const replayedEvents = new Set(["channel.unread", "channel.mention", "dm.preview", "channel.read"]);

    /**
     * Opens the first conversation: the one a link names or this browser opened last, else the
     * first the sidebar can open. The page says chat is opening until it settles.
     */
    async function runPageLoad(): Promise<void> {
        if (isStopped) {
            return;
        }

        state.isOpening = true;
        try {
            await startPageLoad({
                linkedChannelId: options.linkedChannelId?.toLowerCase() ?? null,
                rememberedChannelId: readRememberedChannel(options.storage, storageKey),
                loadSidebar,
                open
            });
        }
        finally {
            state.isOpening = false;
        }
    }

    /**
     * Fetches the sidebar and replaces the rows with it. A read a newer one overtook answers with
     * the newer one's sidebar.
     */
    function loadSidebar(): Promise<SidebarRow[]> {
        const read = readSidebar();
        newestSidebarRead = read;
        return read;
    }

    /** One sidebar read, which lands only if no newer read was asked for meanwhile. */
    async function readSidebar(): Promise<SidebarRow[]> {
        const generation = ++sidebarGeneration;
        sidebarJournal = [];
        const result = await call("chat_get_bootstrap", { p_campus_only: options.areSharedChannelsFilteredByCampus ?? false });
        if (isStopped) {
            return channels.rows;
        }
        if (generation !== sidebarGeneration && newestSidebarRead) {
            return newestSidebarRead;
        }
        const journal = sidebarJournal;
        sidebarJournal = null;
        options.mark("chat:sidebar");

        if (!result.ok) {
            // Before the first sidebar has landed, the list's own line with Try again tells the
            // failure, so the toast is kept for a session that ended, which the line cannot mend,
            // and for a later read's failure, which nothing else shows.
            if (channels.loadState === "loaded" || result.error.severity === "session") {
                report(result.error);
            }
            channels.setLoadFailed();
            throw thrown(result);
        }

        // the bootstrap's own key names
        /* eslint-disable-next-line @typescript-eslint/naming-convention */
        const answer = result.data as { channels?: SidebarRow[], reactions?: ReactionFavorite[], badges?: ChatBadge[], me?: Partial<MeSettings> & { unread_mentions?: UnreadMention[] } } | null;
        const rows = answer?.channels ?? [];
        channels.setSidebar(rows);
        // every load restates the unread mentions, so one read elsewhere stops lighting the dot
        mentions.setUnread(answer?.me?.unread_mentions ?? []);
        // the person's own switches, as another device may have changed them since
        prefs.setMe(answer?.me);
        if (Array.isArray(answer?.reactions)) {
            favorites.splice(0, favorites.length, ...answer.reactions);
        }
        // a profile card names a person's badge keys by these
        channels.setBadges(Array.isArray(answer?.badges) ? answer.badges : []);

        // In arrival order, through the same handlers; each states a value, and a mention the
        // answer already counted is not counted again.
        for (const change of journal) {
            if ("event" in change) {
                channels.applyPersonalEvent(change.event, change.payload, true);
                mentions.applyPersonalEvent(change.event, change.payload);
            }
            else {
                applySave(change.channelId, change.result, change.sentAt);
            }
        }

        // a join still settling is settled by the reload that shows the room as the person's
        rooms.sidebarLoaded();
        return channels.rows;
    }

    /** The wait before a rejoin's sidebar read, pending; null when none is. */
    let sidebarWait: ReturnType<typeof setTimeout> | null = null;

    /**
     * Reads the sidebar again after the personal topic was joined again, after a random wait so a
     * Realtime restart that rejoins every client at once does not read every sidebar in the same
     * second. Live signals still apply meanwhile, and any reload asked during the wait is this one.
     */
    function reloadAfterRejoin(): void {
        if (sidebarWait !== null || isStopped) {
            return;
        }
        sidebarWait = setTimeout(() => {
            sidebarWait = null;
            void loadSidebar().catch(() => undefined);
        }, Math.random() * channelChangedWaitMs);
    }

    /** Cancels a rejoin's wait still pending. */
    function cancelSidebarWait(): void {
        if (sidebarWait !== null) {
            clearTimeout(sidebarWait);
            sidebarWait = null;
        }
    }

    /**
     * The platform says what this client holds may be out of date in a way no other event
     * describes, so the sidebar and the open channel are read again. Nothing is reminted: what the
     * person may read has not changed, only what is on screen.
     */
    function resync(): void {
        void loadSidebar().catch(() => undefined);
        mentions.forgetPeople();
        const channelId = state.activeChannelId;
        if (channelId) {
            // The newest page alone redraws only the latest messages, so the channel then catches up
            // from it, reading again what it holds, as a rejoin does.
            // Only while it is still the channel on screen: one left, or whose access was revoked,
            // while its page was on the way has nobody to catch up for.
            void timelines.loadNewest(channelId)
                .then(() => state.activeChannelId === channelId ? timelines.onJoined(channelId) : undefined)
                .catch(error => report(classifyPlatformError(error)));
        }
    }

    const storageKey = rememberedChannelKey(tenantId, personAliasGuid);
    const pushOffKey = `rock-chat:push-off:${tenantId.toLowerCase()}:${personAliasGuid.toLowerCase()}`;

    /**
     * Takes the open channel off the screen: its position is saved, its catch-up stops, and an
     * open of it still loading is superseded, so it cannot take the screen back.
     */
    function closeActive(): void {
        openCount++;
        void tracker.leave();
        if (state.activeChannelId) {
            timelines.leave(state.activeChannelId);
        }
        showChannel(null);
    }

    /**
     * Puts a channel on screen, or none. Every switch passes through here, so a message's editor
     * and delete confirm close with the conversation they were opened in, and an edit's answer
     * still out lands nowhere.
     */
    function showChannel(channelId: string | null): void {
        if (state.activeChannelId !== channelId) {
            messageActions.switchConversation(channelId);
            rooms.conversationShown(channelId);
        }
        state.activeChannelId = channelId;
        channels.setActive(channelId);
    }

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
            // its catch-up stops and its answers still on the way are dropped
            timelines.leave(state.activeChannelId);
        }

        if (state.activeChannelId !== channelId) {
            tracker.show(channelId);
        }
        showChannel(channelId);

        // The room's facts are read beside its history, so neither waits for the other.
        void rooms.channelOpened(channelId);

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
            showChannel(null);
        }

        return outcome;
    }

    return {
        state,
        channels,
        timelines,
        sender,
        directMessages,
        threads,
        messageActions,
        reactions,
        mentions,
        rooms,
        members,
        prefs,
        isNewMessageShown: isNewMessageShown(options.session),

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
                    // A thread's counts move its root wherever it is held, and a reply reaches the
                    // open thread. The threads go first: they set the root's marks, which the
                    // timeline then records with the counts so a page read earlier cannot undo them.
                    // The person's own new message goes straight in: it carries its send's key, so
                    // it takes the place of the row still sending rather than standing beside it.
                    // A reaction reaches the open thread's copies at once, by the thread's own page;
                    // only the conversation's copy waits for the conversation's.
                    threads.applyEvent(channelId, event, payload);
                    if (event === "reaction.added" || event === "reaction.removed") {
                        reactions.applyThreadEvent(channelId, event, payload);
                    }
                    timelines.applyEvent(channelId, event, payload);

                    // Any copy settles the send, after the views have taken theirs: a reply shown
                    // only in a thread that is not open is kept by no view, and with its answer
                    // lost nothing else would settle it. A timeline message of a channel still
                    // loading is held until its page is in and settles the send there; settling it
                    // now would take the row away before the message that replaces it is drawn.
                    const created = payload as Partial<MessageCreatedEvent> | null;
                    if (event === "message.created" && typeof created?.id === "number" && created.client_key
                        && (timelines.state(channelId).isLoaded || !isTimelineMessage(created))) {
                        sender.landed({ id: created.id, client_key: created.client_key });
                    }
                },
                onPersonalEvent: (event, payload) => {
                    if (event === "resync") {
                        resync();
                        return;
                    }
                    channels.applyPersonalEvent(event, payload);
                    mentions.applyPersonalEvent(event, payload);
                    if (replayedEvents.has(event)) {
                        sidebarJournal?.push({ event, payload });
                    }

                    // Only the channel on screen is joined, so only a change to it can need a cut.
                    const changed = (payload as Partial<Record<keyof ChannelUnreadEvent, unknown>> | null)?.channel_id;

                    // A conversation Rock made before the platform had taken it may have arrived.
                    if (event === "membership.changed") {
                        void directMessages.membershipChanged(typeof changed === "string" ? changed : null);
                    }
                    // A room's facts change with its members and with its own name, description or kind.
                    if (event === "membership.changed" || event === "channel.updated") {
                        rooms.roomChanged(typeof changed === "string" ? changed : null);
                    }
                    if (event === "session.recheck" || (event === "membership.changed" && changed === state.activeChannelId)) {
                        recheck();
                    }
                },
                onPersonalRejoined: reloadAfterRejoin,
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
                        // Only the shell knows which room was open, so it names the room's kind
                        // before closing it.
                        const roomKind = channels.rows.find(row => row.channel_id === state.activeChannelId)?.room_kind ?? null;

                        // Nothing is open any more, so choosing the channel again, once the
                        // person may read it, opens it afresh.
                        closeActive();
                        report({ ...error, roomKind });
                        void loadSidebar().catch(() => undefined);
                        return;
                    }
                    state.connection = error;

                    // A call that failed just before the drop was noticed was the same outage,
                    // which the banner now explains.
                    if (error) {
                        state.errors = state.errors.filter(e => !isConnectionFailure(e));
                    }
                }
            });

            // The token is on the socket before the first join, which the hub does first.
            await hub.start();
            if (isStopped) {
                return;
            }
            detachPage = tracker.attach(options.pageTargets);
            detachThreads = threads.attach(options.pageTargets);
            state.phase = "ready";

            await runPageLoad();

            // After the first channel is on screen, so push never delays it; a stop during the
            // page load leaves it unstarted.
            if (options.push && !isStopped) {
                push = createPushRegistration({
                    ...options.push,
                    settings: () => session.settings().push,
                    call: (name, args) => platform.call(name, args),
                    // per church and person, so each person on a shared browser keeps their own Turn off
                    offFlag: {
                        read: () => options.storage?.getItem(pushOffKey) === "true",
                        write: isOff => options.storage?.setItem(pushOffKey, isOff ? "true" : "false")
                    }
                });
                // drawn now, as a browser's first registration can take half a minute
                state.isPushOffered = push.isOffered();
                state.push = push.state();
                await push.start();
                state.isPushOffered = push.isOffered();
                state.push = push.state();
            }
        },

        selectChannel: async (channelId: string): Promise<void> => {
            directMessages.close();
            if (channelId !== state.activeChannelId) {
                await open(channelId);
            }
        },

        seen: (messageId: number): void => tracker.seen(messageId),

        // The same save as reading the message on screen, sent now rather than when the page
        // next loses focus, because the person asked for it.
        markRead: (messageId: number): void => {
            tracker.seen(messageId);
            void tracker.flush();
        },

        firstMessageShown: (): void => {
            if (!isFirstMessageMarked) {
                isFirstMessageMarked = true;
                options.mark("chat:first-message");
            }
        },

        send: async (body: string): Promise<void> => {
            // While New message has people chosen, the box is theirs, never the conversation's
            // left open underneath.
            if (directMessages.draft || directMessages.lookup.status !== "idle") {
                await directMessages.sendFirst(body);
            }
            else if (state.activeChannelId && isSendKeyed()) {
                await sender.send(state.activeChannelId, body, undefined, undefined, takeQuote(quoteKey(state.activeChannelId)));
                notePushSend();
            }
        },

        sendReply: async (rootId: number, body: string, isShownInChannel: boolean): Promise<void> => {
            // A thread opened from the All tab can belong to a conversation other than the open one.
            const thread = threads.state.thread;
            const channelId = thread?.rootId === rootId ? thread.channelId : state.activeChannelId;
            if (channelId && isSendKeyed()) {
                // the thread's own quote, never the conversation's, which waits for a send there
                await sender.send(channelId, body, undefined, { parentId: rootId, isShownInChannel }, takeQuote(quoteKey(channelId, rootId)));
                notePushSend();
            }
        },

        turnOnPush: async (): Promise<void> => {
            if (!push) {
                return;
            }

            // only the browser's own box shows at once; on waits for the register to settle
            const turning = push.turnOn();
            if (push.state() === "asking") {
                state.push = "asking";
            }
            await turning;
            state.isPushOffered = push.isOffered();
            state.push = push.state();
        },

        turnOffPush: async (): Promise<void> => {
            const failure = await push?.turnOff() ?? null;
            state.push = push?.state() ?? "unavailable";
            if (failure) {
                report(failure);
            }
        },

        // Only while its pane is the one in view: a phone's list or thread can cover it.
        isChannelOnScreen: (channelId: string): boolean => {
            return channelId === state.activeChannelId
                && !state.hiddenPanes.conversation
                && options.pageTargets.document.visibilityState === "visible";
        },

        // A reply kept to its thread is seen only in the drawer, so only the open thread counts,
        // whatever conversation is behind it, and only while its pane is the one in view.
        isThreadOnScreen: (rootId: number): boolean => {
            return threads.state.thread?.rootId === rootId
                && !state.hiddenPanes.thread
                && options.pageTargets.document.visibilityState === "visible";
        },

        // Mentions, never messages: the icon counts what was meant for the person, as the
        // platform's mention push does, and the sidebar lists the same rooms that push counts.
        mentionBadge: (): number => channels.rows.reduce((sum, row) => sum + (row.mention_count ?? 0), 0),

        loadOlder: async (): Promise<void> => {
            if (state.activeChannelId) {
                await timelines.loadOlder(state.activeChannelId).catch(error => report(classifyPlatformError(error)));
            }
        },

        // Two failures can share a code with different sentences, so the one closed is matched
        // the way the toast keys it, by code and text, never by the code alone.
        dismissError: (error: ChatError): void => {
            const index = state.errors.findIndex(e => e.code === error.code && e.text === error.text);
            if (index >= 0) {
                state.errors.splice(index, 1);
            }
        },

        stop: async (): Promise<void> => {
            isStopped = true;
            push?.stop();
            state.isPushOffered = false;
            cancelRecheck();
            cancelSidebarWait();
            timelines.stop();
            directMessages.stop();

            // The thread's save leaves before the threads stop, so it still goes out; nothing it
            // answers reaches the screen.
            const threadSaved = threads.closeThread();
            threads.stop();
            messageActions.stop();
            reactions.stop();
            mentions.stop();
            rooms.stop();
            members.stop();
            prefs.stop();

            // Stopped before the last save, so no refresh starts while it goes out; the token is
            // kept, and the save still carries it.
            session.stop();
            await Promise.all([tracker.leave(), threadSaved]);
            detachPage?.();
            detachThreads?.();
            await hub?.stop();
        }
    };
}
