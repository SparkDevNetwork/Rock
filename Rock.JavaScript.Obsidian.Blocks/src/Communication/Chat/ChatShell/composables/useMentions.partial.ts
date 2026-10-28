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
// Mentions: the @ picker's search, the mentions inbox, the inbox's last visit on this device and
// the header's dot.
//
// The search waits for a pause in typing and drops an answer older than the newest query, so a
// slow answer never overwrites a newer one. The @ that opens the picker draws the conversation's
// last bare-@ answer, while it is young, and asks again; with nothing held it asks at once. The
// composer's focus warms that answer before the first @.
//
// The inbox's last visit is kept on the device by church and person; a browser that refuses its
// storage still works, the dot then showing for any unread mention. Once stopped, nothing more is
// fetched and no answer still on its way reaches the screen.
/* eslint-disable @typescript-eslint/naming-convention */
import { reactive } from "vue";
import { CallResult } from "../platformCall.partial";
import { ChannelMentionEvent, ChatError, MentionListPage, MentionPerson, MentionRow, MentionSearchAnswer, SidebarRow, UnreadMention } from "../types.partial";
import { Timelines } from "./useHistory.partial";
import { LandingCheck, loadUntilHeld } from "./useJump.partial";
import { PlatformLimits } from "./useSession.partial";

/**
 * How long the picker waits after the last keystroke before it searches. An estimate, not a
 * measurement: about the gap between keystrokes of steady typing, so a typed name asks once.
 * The @ that opens the picker has no keystrokes to gather, so it asks at once.
 */
export const mentionSearchPause = 150;

/** How many people the picker asks for: the six rows it draws. */
export const mentionPickerSize = 6;

/**
 * How many conversations' bare-@ answers are held for the page, the one used longest ago dropped
 * first. An estimate: more rooms than a person types in at one sitting, at six people each.
 */
export const mentionHeldConversations = 20;

/**
 * How long a held bare-@ answer may still be drawn after it was asked. An estimate: nothing tells
 * the reader when someone else is put on the Ban List or a room starts hiding its roster, so this
 * bounds how long such a person can be offered, at one small call a minute in a room typed in.
 */
export const mentionHeldAge = 60_000;

/**
 * How long a bare-@ ask still out is shared with an open, before the open asks afresh. An
 * estimate: well past a normal answer, so a warm and the @ just after it ask once, while an ask
 * stalled on a dropped connection never holds the picker for good.
 */
export const mentionAskShared = 5_000;

/** What the picker draws for the newest query. */
export type MentionPickerState = {
    /** The query typed after the @, or null when the picker is closed. */
    query: string | null;
    people: MentionPerson[];

    /** Whether the @all row leads the list. */
    isAllOffered: boolean;

    /**
     * Whether the answer for the newest query is still on its way; the people are then those
     * already held, narrowed to the query, so every row drawn matches what was typed.
     */
    isAwaitingAnswer: boolean;

    /** Whether no answer has come since the picker opened, when it has nothing to draw yet. */
    isAwaitingFirstAnswer: boolean;
};

/** The inbox as the panel draws it. */
export type MentionInboxState = {
    rows: MentionRow[];
    hasMore: boolean;
    isLoading: boolean;
    isRefreshing: boolean;
    isLoadingOlder: boolean;
    error: ChatError | null;
    olderError: ChatError | null;

    /** The messages newer than the last visit and not opened here, marked New for this visit. */
    newIds: number[];

    /** The message whose row was just opened. */
    currentId: number | null;
};

/** What the mentions hold for the screen. */
export type MentionsState = {
    picker: MentionPickerState;
    inbox: MentionInboxState;

    /** Whether the header's Mentions button shows its dot. */
    hasDot: boolean;
};

/** A toast that is not a failure, in the shape the toast draws. */
export type MentionNotice = {
    code: string;
    text: string;
    icon: string;
};

/** What the mentions reach outside themselves. */
export type MentionsDependencies = {
    call: (name: string, args: Record<string, unknown>) => Promise<CallResult>;
    tenantId: string;
    personAliasGuid: string;

    /** The device's storage, which may be missing or refuse every read and write. */
    storage: () => Pick<Storage, "getItem" | "setItem">;

    /** The channel timelines a jump looks for its message in. */
    timelines: Pick<Timelines, "state" | "loadOlder">;

    /** The platform's limits, from the latest token exchange: how many rows a page of the inbox holds. */
    limits: () => Pick<PlatformLimits, "mention_page_size">;

    /** Opens a conversation; resolves true once it is on screen. */
    openChannel: (channelId: string) => Promise<boolean>;

    /** The conversation on screen now, or null. */
    activeChannelId: () => string | null;

    /** How far the person has read a conversation, from its sidebar row; null when unknown. */
    readCursor: (channelId: string) => number | null;

    /** Scrolls the open conversation to a message it holds and flashes it. */
    land: (messageId: number) => void;

    /** Opens a thread at a reply. */
    openThread: (channelId: string, rootId: number, messageId: number) => Promise<void>;

    toast: (notice: MentionNotice) => void;
};

/** The mentions a shell holds. */
export type Mentions = {
    state: MentionsState;
    search: (channel: Pick<SidebarRow, "channel_id" | "can_mention" | "can_mention_all" | "is_member">, query: string) => void;
    closeSearch: () => void;

    /** Asks a conversation's bare-@ people ahead of its first @, as its composer takes focus. */
    warm: (channel: Pick<SidebarRow, "channel_id" | "can_mention" | "is_member">) => void;

    /** Forgets every conversation's held people, as a resync says what this client holds may be out of date. */
    forgetPeople: () => void;
    openInbox: () => Promise<void>;
    refreshInbox: () => Promise<void>;
    loadOlder: () => Promise<void>;
    closeInbox: () => void;
    jump: (row: MentionRow) => Promise<void>;
    setUnread: (entries: UnreadMention[]) => void;
    applyPersonalEvent: (event: string, payload: unknown) => void;
    channelRead: (channelId: string, readCursor: number) => void;
    threadOpened: (channelId: string, rootId: number) => void;
    stop: () => void;
};

/** The toast a jump gives for a mention further back than it fetches. */
export const tooFarBackNotice: MentionNotice = { code: "mention-too-far-back", text: "That message is too far back to jump to.", icon: "alert" };

/**
 * An unread mention as the dot weighs it. One that arrived live carries no time, and counts as
 * newer than the last visit until the inbox is opened after it.
 */
type DotEntry = Omit<UnreadMention, "created_at"> & { createdAt: number | null };

/**
 * Whether @all is what the person may be typing: nothing yet, or the start of "all" or "everyone".
 *
 * @param query The text after the @.
 *
 * @returns True when the @all row leads the picker.
 */
function isAllQuery(query: string): boolean {
    const typed = query.toLowerCase();
    return "all".startsWith(typed) || "everyone".startsWith(typed);
}

/**
 * The people held who still match a longer or different query, by the platform's own rule: a
 * nick or last name starting with the text, in any letter case.
 *
 * @param people The people held.
 * @param query The text after the @.
 *
 * @returns The people who match it.
 */
function narrowPeople(people: MentionPerson[], query: string): MentionPerson[] {
    const typed = query.trim().toLowerCase();
    return people.filter(person => (person.nick_name ?? "").toLowerCase().startsWith(typed) || (person.last_name ?? "").toLowerCase().startsWith(typed));
}

/**
 * Whether two lists draw the same rows: the same people in the same order, by the same names and photos.
 *
 * @param drawn The people drawn.
 * @param answered The people an answer lists.
 *
 * @returns True when nothing on screen would change.
 */
function isSamePeople(drawn: MentionPerson[], answered: MentionPerson[]): boolean {
    return drawn.length === answered.length && drawn.every((person, index) => {
        const other = answered[index];
        return person.person_alias_guid === other.person_alias_guid
            && person.nick_name === other.nick_name
            && person.last_name === other.last_name
            && (person.avatar_url ?? null) === (other.avatar_url ?? null);
    });
}

/**
 * Reads a channel.mention payload, which arrives untyped off the wire.
 *
 * @param payload The event's payload.
 *
 * @returns The event, or null when it cannot be read.
 */
function readMentionEvent(payload: unknown): ChannelMentionEvent | null {
    const event = payload as Partial<Record<keyof ChannelMentionEvent, unknown>> | null;
    if (!event || typeof event.channel_id !== "string" || typeof event.message_id !== "number") {
        return null;
    }

    return {
        channel_id: event.channel_id,
        message_id: event.message_id,
        parent_id: typeof event.parent_id === "number" ? event.parent_id : null,
        in_channel: event.in_channel === true,
        all: event.all === true
    };
}

/**
 * Creates the mentions.
 *
 * @param dependencies What the mentions reach outside themselves.
 *
 * @returns The mentions.
 */
export function createMentions(dependencies: MentionsDependencies): Mentions {
    const state = reactive<MentionsState>({
        picker: { query: null, people: [], isAllOffered: false, isAwaitingAnswer: false, isAwaitingFirstAnswer: false },
        inbox: { rows: [], hasMore: false, isLoading: false, isRefreshing: false, isLoadingOlder: false, error: null, olderError: null, newIds: [], currentId: null },
        hasDot: false
    }) as MentionsState;

    const visitKey = `rock-chat:mentions-visit:${dependencies.tenantId.toLowerCase()}:${dependencies.personAliasGuid.toLowerCase()}`;

    let isStopped = false;

    // Each search, close and stop moves this on, so an answer to an older query is dropped.
    let searchGeneration = 0;
    let searchTimer: ReturnType<typeof setTimeout> | null = null;

    // Each conversation's latest bare-@ answer and when it was asked, the one used longest ago
    // first. Drawn at an open only while young and only until that open's own answer replaces it,
    // so a person who joined, left or was renamed since is put right within one round trip.
    const heldOpenings = new Map<string, { people: MentionPerson[], askedAt: number }>();

    // The bare-@ answer asked for a conversation and not yet in, shared by a warm and an open
    // that comes soon after, so they ask once.
    const askingOpenings = new Map<string, { answer: Promise<CallResult>, askedAt: number }>();

    // Each open, refresh, close and stop moves this on, so a page meant for an inbox since
    // replaced or closed never lands in the one on screen.
    let inboxGeneration = 0;

    let unread: DotEntry[] = [];
    let lastVisit = readLastVisit();

    // The visit before this one, which decides New while the inbox stays open; null when never.
    let newSince: number | null = null;
    const openedIds = new Set<number>();

    // #region The last visit

    /** The last visit on this device, or null when there was none or the storage refuses. */
    function readLastVisit(): number | null {
        try {
            const at = Number(dependencies.storage().getItem(visitKey));
            return at > 0 ? at : null;
        }
        catch {
            // Intentionally ignored: a browser that refuses its storage only forgets the visit.
            return null;
        }
    }

    /** Keeps the visit on the device, for the next page load. */
    function writeLastVisit(at: number): void {
        try {
            dependencies.storage().setItem(visitKey, String(at));
        }
        catch {
            // Intentionally ignored: blocked or full storage only forgets the visit; this page keeps it.
        }
    }

    // #endregion The last visit

    /** The dot: any unread mention newer than the last visit, or every one when there was none. */
    function refreshDot(): void {
        state.hasDot = unread.some(entry => entry.createdAt === null || lastVisit === null || entry.createdAt > lastVisit);
    }

    /** Drops the conversation's mentions at or below a saved read position. */
    function channelRead(channelId: string, readCursor: number): void {
        // A reply kept to its thread is read in the thread, not by reading the conversation.
        unread = unread.filter(entry => entry.channel_id !== channelId || !entry.in_channel || entry.message_id > readCursor);
        refreshDot();
    }

    // #region The picker

    /** The people in a search's answer; a refusal lists nobody. */
    function answeredPeople(result: CallResult): MentionPerson[] {
        return result.ok ? ((result.data as Partial<MentionSearchAnswer> | null)?.people ?? []) : [];
    }

    /**
     * Holds a conversation's bare-@ entry as the one used last, dropping the one used longest ago
     * past the bound. Using an entry keeps it from being dropped, never makes it younger.
     */
    function holdOpening(channelId: string, entry: { people: MentionPerson[], askedAt: number }): void {
        heldOpenings.delete(channelId);
        heldOpenings.set(channelId, entry);
        if (heldOpenings.size > mentionHeldConversations) {
            heldOpenings.delete(heldOpenings.keys().next().value as string);
        }
    }

    /** A conversation's held bare-@ entry while it is young enough to draw. */
    function youngOpening(channelId: string): { people: MentionPerson[], askedAt: number } | undefined {
        const entry = heldOpenings.get(channelId);
        return entry && Date.now() - entry.askedAt < mentionHeldAge ? entry : undefined;
    }

    /**
     * Asks a conversation's bare-@ people, or joins an ask for them made moments ago, and holds the
     * answer. An older ask still out may have stalled, so it is replaced, and its answer, should it
     * come, is not held. A refusal forgets what was held, since the platform no longer lists those
     * people here; a call that never reached it keeps them.
     */
    function askOpening(channelId: string): Promise<CallResult> {
        const asking = askingOpenings.get(channelId);
        if (asking && Date.now() - asking.askedAt < mentionAskShared) {
            return asking.answer;
        }

        const askedAt = Date.now();
        // Read only once the answer is in, by which time it holds this ask.
        let ask: { answer: Promise<CallResult>, askedAt: number } | null = null;
        ask = {
            askedAt,
            answer: (async (): Promise<CallResult> => {
                const result = await dependencies.call("chat_search_mentionable", { p_channel_id: channelId, p_query: "", p_limit: mentionPickerSize });

                // An answer to an ask since replaced, or one the conversation was forgotten after,
                // as on a membership change, may list the people from before it, so it is not held.
                if (askingOpenings.get(channelId) !== ask) {
                    return result;
                }

                askingOpenings.delete(channelId);
                if (result.ok) {
                    holdOpening(channelId, { people: answeredPeople(result), askedAt });
                }
                else if (result.status !== 0) {
                    heldOpenings.delete(channelId);
                }
                return result;
            })()
        };
        askingOpenings.set(channelId, ask);
        return ask.answer;
    }

    /** Forgets a conversation's held people and any ask for them still on its way. */
    function forgetConversation(channelId: string): void {
        heldOpenings.delete(channelId);
        askingOpenings.delete(channelId);
    }

    /** Forgets every conversation's held people. */
    function forgetPeople(): void {
        heldOpenings.clear();
        askingOpenings.clear();
    }

    /** Asks the platform for the people matching a query, if it is still the newest. */
    async function runSearch(generation: number, channelId: string, query: string): Promise<void> {
        searchTimer = null;
        const result = query === ""
            ? await askOpening(channelId)
            : await dependencies.call("chat_search_mentionable", { p_channel_id: channelId, p_query: query, p_limit: mentionPickerSize });
        if (isStopped || generation !== searchGeneration) {
            return;
        }

        // A refusal lists nobody rather than the people an earlier query found. An answer the
        // same as the rows drawn, as the held ones mostly are, leaves them be, so the row the
        // person moved to stays where it is.
        const people = answeredPeople(result);
        if (!isSamePeople(state.picker.people, people)) {
            state.picker.people = people;
        }
        state.picker.isAwaitingAnswer = false;
        state.picker.isAwaitingFirstAnswer = false;
    }

    /** Drops a pause still running, so no search is asked for a query already replaced. */
    function cancelPause(): void {
        if (searchTimer !== null) {
            clearTimeout(searchTimer);
            searchTimer = null;
        }
    }

    // #endregion The picker

    // #region The inbox

    /** One page of the inbox from every conversation: the newest, or the one after a row. */
    function fetchInboxPage(after: MentionRow | null): Promise<CallResult> {
        return dependencies.call("chat_list_mentions", {
            p_channel_id: null,
            p_before_created_at: after?.message.created_at ?? null,
            p_before_id: after?.message.id ?? null,
            p_limit: dependencies.limits().mention_page_size
        });
    }

    /** Marks New the rows newer than the visit before this one, leaving out any opened since. */
    function markNew(): void {
        const since = newSince;
        state.inbox.newIds = state.inbox.rows
            .filter(row => !openedIds.has(row.message.id) && (since === null || Date.parse(row.message.created_at) > since))
            .map(row => row.message.id);
    }

    /**
     * Fetches the newest page into the inbox. A failure keeps the rows on screen and is handed
     * back; a page meant for an inbox since replaced is dropped.
     */
    async function loadFirstPage(generation: number): Promise<ChatError | null> {
        const result = await fetchInboxPage(null);
        if (isStopped || generation !== inboxGeneration) {
            return null;
        }

        if (!result.ok) {
            return result.error;
        }

        const page = result.data as Partial<MentionListPage> | null;
        state.inbox.rows = page?.mentions ?? [];
        state.inbox.hasMore = page?.has_more === true;
        state.inbox.olderError = null;
        markNew();
        return null;
    }

    /** Empties the inbox and drops any page still on its way. */
    function resetInbox(): void {
        inboxGeneration++;
        Object.assign(state.inbox, { rows: [], hasMore: false, isLoading: false, isRefreshing: false, isLoadingOlder: false, error: null, olderError: null, newIds: [], currentId: null });
    }

    /**
     * Lands on a channel mention in its conversation, fetching at most two older pages as any
     * landing does; past them the conversation stays at the newest and says so.
     */
    async function jumpInConversation(row: MentionRow): Promise<void> {
        const isOpen = await dependencies.openChannel(row.channel_id);
        if (!isOpen) {
            return;
        }

        // Opening another conversation meanwhile ends the jump: it must not land or warn there.
        const isOnScreen = (): boolean => !isStopped && dependencies.activeChannelId() === row.channel_id;
        const messageId = row.message.id;
        let oldestSeen: number | undefined;
        // written inside the check, so it is declared wide rather than narrowed to its first value
        let lastCheck = "stop" as LandingCheck;
        await loadUntilHeld(() => {
            const timeline = dependencies.timelines.state(row.channel_id);
            const oldest = timeline.messages[0]?.id;
            if (!isOnScreen() || oldest === undefined) {
                return lastCheck = "stop";
            }

            if (timeline.messages.some(message => message.id === messageId)) {
                dependencies.land(messageId);
                return lastCheck = "held";
            }

            // A failed page moved nothing, and one already past the message without it means the
            // conversation no longer shows it, as when it was deleted: neither is too far back.
            const isStuck = oldest === oldestSeen || oldest < messageId;
            oldestSeen = oldest;
            return lastCheck = isStuck || !timeline.hasMore ? "stop" : "more";
        }, () => dependencies.timelines.loadOlder(row.channel_id));

        // Still looking once the pages ran out: the message is older than a jump fetches.
        if (lastCheck === "more" && isOnScreen()) {
            dependencies.toast(tooFarBackNotice);
        }
    }

    // #endregion The inbox

    return {
        state,

        search: (channel, query): void => {
            if (isStopped) {
                return;
            }

            searchGeneration++;
            cancelPause();
            const isOpening = state.picker.query === null;
            state.picker.query = query;
            state.picker.isAllOffered = channel.can_mention_all && isAllQuery(query);

            // Where the roster is hidden from the person the platform would only refuse, so it is
            // not asked; @all, when they hold the right, is offered alone.
            if (!channel.can_mention) {
                forgetConversation(channel.channel_id);
                state.picker.people = [];
                state.picker.isAwaitingAnswer = false;
                state.picker.isAwaitingFirstAnswer = false;
                return;
            }

            // A bare @ opens on the conversation's last bare-@ answer, the same list its own answer
            // will be unless the room changed, so the picker shows at once and its rows stay put.
            // A query typed at the opening is not drawn from it: the people held are only the
            // first by name, and the rows would grow as the answer came. Only a member's room
            // draws it, since the people held are the ones they could see while one, and only a
            // young one, since a fast letter cancels the refresh and no event says someone else
            // was banned or the roster hidden.
            const held = isOpening && query === "" && channel.is_member ? youngOpening(channel.channel_id) : undefined;
            if (held) {
                holdOpening(channel.channel_id, held);
                state.picker.people = held.people;
            }
            else {
                // Until the answer comes the rows held are narrowed to what was typed, so Enter
                // after a fast keystroke never takes a person the query no longer matches.
                state.picker.people = narrowPeople(state.picker.people, query);
            }
            state.picker.isAwaitingAnswer = true;
            if (isOpening && !held) {
                state.picker.isAwaitingFirstAnswer = true;
            }

            // With nothing on screen the opening @ asks at once, having no keystrokes to gather.
            // Held rows already drawn, the refresh waits the pause like any keystroke, so a fast
            // @ and letter still ask once.
            const generation = searchGeneration;
            if (isOpening && !held) {
                void runSearch(generation, channel.channel_id, query);
            }
            else {
                searchTimer = setTimeout(() => void runSearch(generation, channel.channel_id, query), mentionSearchPause);
            }
        },

        warm: (channel): void => {
            // Only where nothing young is held: a held answer is kept fresh by its opens, and asked
            // again here once it is too old to draw.
            if (!isStopped && channel.can_mention && channel.is_member && !youngOpening(channel.channel_id)) {
                void askOpening(channel.channel_id);
            }
        },

        forgetPeople,

        closeSearch: (): void => {
            searchGeneration++;
            cancelPause();
            state.picker.query = null;
            state.picker.people = [];
            state.picker.isAllOffered = false;
            state.picker.isAwaitingAnswer = false;
            state.picker.isAwaitingFirstAnswer = false;
        },

        openInbox: async (): Promise<void> => {
            if (isStopped) {
                return;
            }

            resetInbox();
            openedIds.clear();

            // Opening is the visit: what came after the one before is New for this visit, and
            // every mention already arrived, live ones included, stops lighting the dot.
            newSince = lastVisit;
            lastVisit = Date.now();
            writeLastVisit(lastVisit);
            unread = unread.map(entry => entry.createdAt === null ? { ...entry, createdAt: lastVisit } : entry);
            refreshDot();

            state.inbox.isLoading = true;
            const generation = inboxGeneration;
            const error = await loadFirstPage(generation);
            if (generation === inboxGeneration && !isStopped) {
                state.inbox.isLoading = false;
                state.inbox.error = error;
            }
        },

        refreshInbox: async (): Promise<void> => {
            if (isStopped) {
                return;
            }

            // A refresh replaces the rows, so an older page still on its way would land after them.
            const generation = ++inboxGeneration;
            state.inbox.isRefreshing = true;
            state.inbox.isLoadingOlder = false;
            const error = await loadFirstPage(generation);
            if (generation === inboxGeneration && !isStopped) {
                state.inbox.isRefreshing = false;
                state.inbox.error = error;
            }
        },

        loadOlder: async (): Promise<void> => {
            const last = state.inbox.rows[state.inbox.rows.length - 1];
            // A refresh on its way replaces the rows, so a page after the current last row would land
            // past a gap; the refreshed rows decide what is older.
            if (isStopped || !last || !state.inbox.hasMore || state.inbox.isLoadingOlder || state.inbox.isRefreshing) {
                return;
            }

            const generation = inboxGeneration;
            state.inbox.isLoadingOlder = true;
            state.inbox.olderError = null;
            const result = await fetchInboxPage(last);
            if (isStopped || generation !== inboxGeneration) {
                return;
            }

            state.inbox.isLoadingOlder = false;
            if (!result.ok) {
                state.inbox.olderError = result.error;
                return;
            }

            // A mention written between pages can shift the keyset by a row; never draw it twice.
            const page = result.data as Partial<MentionListPage> | null;
            const held = new Set(state.inbox.rows.map(row => row.message.id));
            state.inbox.rows = [...state.inbox.rows, ...(page?.mentions ?? []).filter(row => !held.has(row.message.id))];
            state.inbox.hasMore = page?.has_more === true;
            markNew();
        },

        closeInbox: resetInbox,

        jump: async (row): Promise<void> => {
            if (isStopped) {
                return;
            }

            // The row opened is no longer New, even if the inbox refreshes while it stays open.
            openedIds.add(row.message.id);
            state.inbox.currentId = row.message.id;
            state.inbox.newIds = state.inbox.newIds.filter(id => id !== row.message.id);

            if (row.parent_id !== null) {
                await dependencies.openThread(row.channel_id, row.parent_id, row.message.id);
                return;
            }

            await jumpInConversation(row);
        },

        setUnread: (entries): void => {
            unread = entries.map(({ created_at, ...entry }) => ({ ...entry, createdAt: Date.parse(created_at) }));
            refreshDot();
        },

        applyPersonalEvent: (event, payload): void => {
            if (isStopped) {
                return;
            }

            // a read saved on another of the person's devices reads what a save here would
            const read = payload as { channel_id?: unknown, read_cursor?: unknown } | null;
            if (event === "channel.read" && typeof read?.channel_id === "string" && typeof read.read_cursor === "number") {
                channelRead(read.channel_id, read.read_cursor);
                return;
            }

            // A membership or a room that changed may list other people now, or hide its roster,
            // so its held people are not drawn again: someone just banned must not be offered.
            if ((event === "membership.changed" || event === "channel.updated") && typeof read?.channel_id === "string") {
                forgetConversation(read.channel_id);
                return;
            }
            if (event !== "channel.mention") {
                return;
            }

            // By name or with @all, as the inbox lists both. One already held, as a mention replayed
            // over a reload's answer is, is held once.
            const mention = readMentionEvent(payload);
            if (!mention || unread.some(entry => entry.message_id === mention.message_id)) {
                return;
            }

            // An edit naming the person in a message of the conversation they have already read
            // sends a mention too; only a reply kept to its thread is read elsewhere, and of those
            // the platform tells only the thread's followers of an @all, as a reload counts them.
            const readCursor = mention.in_channel ? dependencies.readCursor(mention.channel_id) : null;
            if (readCursor !== null && mention.message_id <= readCursor) {
                return;
            }

            unread = [{ channel_id: mention.channel_id, parent_id: mention.parent_id, message_id: mention.message_id, in_channel: mention.in_channel, createdAt: null }, ...unread];
            refreshDot();
        },

        channelRead,

        threadOpened: (channelId, rootId): void => {
            unread = unread.filter(entry => entry.channel_id !== channelId || entry.parent_id !== rootId);
            refreshDot();
        },

        stop: (): void => {
            isStopped = true;
            searchGeneration++;
            inboxGeneration++;
            cancelPause();
            forgetPeople();
        }
    };
}
