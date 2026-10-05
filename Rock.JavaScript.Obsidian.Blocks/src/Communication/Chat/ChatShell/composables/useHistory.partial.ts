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
// A channel's timeline on screen: history pages and live events merged into one list ordered by
// message id, the order the platform uses for history, the read position and the unread count.
//
// What is held is one unbroken run of messages, and the timelines know how far it is proven:
// every message up to "verified through" is held, and only a page fetched from the platform
// moves that point. A live copy can be lost outright and the person's own send is confirmed over
// HTTP even while the socket is down, so neither proves that nothing before it is missing. Every
// confirmed join pages forward from what is verified until nothing newer is left; a rejoin reads
// the held run again on the way, so what changed while the socket was down is redrawn. A newest
// page that does not join what is held replaces it in one step. Every fetch belongs to the
// channel as it was when asked, so an answer that arrives after the channel was replaced or left
// is dropped. A failed page is tried again with randomised, growing waits, and the channel says it
// is behind meanwhile. A socket gone for hours can miss more than is worth paging through, so past
// a bound the channel is opened fresh at its newest page.
import { reactive } from "vue";
import { computeBackoff } from "../platformCall.partial";
import { HistoryPage, MessageCreatedEvent, MessageDeletedEvent, MessageEditedEvent, MessageVisibilityEvent, TimelineMessage } from "../types.partial";

/** How many messages the first page of a channel holds. */
export const firstPageSize = 50;

/** How catch-up pages forward, when the settings name nothing else: estimates, not measurements. */
export const defaultCatchUp = { page: 100, max: 500 };

/** How long a failed catch-up page waits before it is tried again, at first and at most: estimates. */
export const defaultRetryDelay = { baseMs: 1000, capMs: 30000 };

/** What a message row draws: what kind of row, and its text. */
export type MessageDisplay = {
    kind: "message" | "system" | "deleted" | "notice";
    text: string | null;
};

/**
 * What a row shows for a message. The server decides when a message is hidden or removed and
 * writes the sentence shown in its place, and a message type this client was not built with is
 * drawn by its text, so a released client still shows what later releases of the platform send.
 *
 * @param message The message.
 *
 * @returns The kind of row and its text.
 */
export function messageDisplay(message: TimelineMessage): MessageDisplay {
    if (message.deleted_at) {
        return { kind: "deleted", text: "This message was deleted." };
    }

    if (message.visibility === "hidden" || message.visibility === "removed") {
        return { kind: "notice", text: message.visibility_notice ?? "This message is not shown." };
    }

    return { kind: message.message_type === "system" ? "system" : "message", text: message.body };
}

/** A channel's timeline. */
export type TimelineState = {
    /** The messages, oldest first, by id. */
    messages: TimelineMessage[];

    /** Whether the first page has arrived. */
    isLoaded: boolean;

    /** Whether older messages exist than the oldest here. */
    hasMore: boolean;

    /** The person's read position when the first page was read. */
    readCursor: number | null;

    /** How many messages were above that position, for the unread divider. */
    unreadCount: number;

    /** Whether a catch-up failed and is waiting to try again, so messages may be missing. */
    isBehind: boolean;
};

/** What the timeline reaches outside itself. */
export type TimelineDependencies = {
    /** Fetches a page: the newest, the page before an id, or the page after one. */
    fetchPage: (channelId: string, options: { limit: number, beforeId?: number, afterId?: number }) => Promise<HistoryPage>;

    /** How catch-up pages forward on a rejoin, read from the settings each time. */
    catchUp?: () => { page: number, max: number };

    /** How long a failed catch-up page waits before it is tried again, read from the settings each time. */
    retryDelay?: () => { baseMs: number, capMs: number };

    /** Waits, so a test can say when; a timer by default. */
    sleep?: (ms: number) => Promise<void>;

    /** Spreads the waits of many clients apart; Math.random by default. */
    random?: () => number;
};

/** The timelines a shell holds. */
export type Timelines = {
    /** The timeline of a channel, created empty on first ask. */
    state: (channelId: string) => TimelineState;

    /** Fetches the first page and applies anything that arrived live while it was in flight. */
    loadNewest: (channelId: string) => Promise<void>;

    /** Fetches the page before the oldest message held. */
    loadOlder: (channelId: string) => Promise<void>;

    /** The channel's live topic was joined: pages forward from what is verified until caught up. */
    onJoined: (channelId: string) => Promise<void>;

    /** A live event on the channel's topic. */
    applyEvent: (channelId: string, event: string, payload: unknown) => void;

    /** Puts one message in place by id, a person's own confirmed send for one. */
    upsert: (channelId: string, message: TimelineMessage) => void;

    /** The channel is no longer open: its catch-up stops and any answer on its way is dropped. */
    leave: (channelId: string) => void;

    /** The session ended: every channel is let go. */
    stop: () => void;
};

/**
 * Whether a message belongs on the channel's timeline: a root, or a reply also shown in the
 * channel. A reply shown only in its thread is the thread panel's.
 */
function isTimelineMessage(message: Pick<TimelineMessage, "parent_id" | "shown_in_channel">): boolean {
    return message.parent_id === null || message.parent_id === undefined || message.shown_in_channel === true;
}

/** Whether a live payload carries the message id every event is keyed by. */
function hasId(payload: unknown): payload is { id: number } {
    return !!payload && typeof payload === "object" && typeof (payload as { id: unknown }).id === "number";
}

/**
 * Combines what is held for a message with a newer copy of it. A page can have been read
 * before an edit or a delete that has already arrived live, so an edit held is kept over an
 * older body and a delete held is never undone. A live copy lacks the sender's name, so a field
 * the newer copy does not carry keeps what is held.
 */
function combine(held: TimelineMessage, incoming: TimelineMessage): TimelineMessage {
    const result: TimelineMessage = { ...held };

    for (const [key, value] of Object.entries(incoming)) {
        if (value !== undefined) {
            (result as Record<string, unknown>)[key] = value;
        }
    }

    const isNewerEditHeld = !!held.edited_at && (!incoming.edited_at || incoming.edited_at < held.edited_at);
    if (isNewerEditHeld) {
        result.body = held.body;
        result.edited_at = held.edited_at;
    }

    if (held.deleted_at && !incoming.deleted_at) {
        result.deleted_at = held.deleted_at;
        result.body = null;
        result.metadata = null;
    }

    return result;
}

/**
 * Finds where a message id is, or would go, in a list ordered by id.
 *
 * @returns The index of the id, or of the first message with a greater id.
 */
function positionOf(messages: TimelineMessage[], id: number): number {
    let low = 0;
    let high = messages.length;

    while (low < high) {
        const middle = (low + high) >>> 1;
        if (messages[middle].id < id) {
            low = middle + 1;
        }
        else {
            high = middle;
        }
    }

    return low;
}

/** Whether a failed fetch is worth trying again: a refusal of access or of the sign-in is not. */
function isTransient(error: unknown): boolean {
    const code = (error as { code?: unknown } | null)?.code;
    const severity = (error as { severity?: unknown } | null)?.severity;

    if (typeof code === "string" && (code.startsWith("auth.") || code.startsWith("authz."))) {
        return false;
    }

    return severity !== "permission" && severity !== "session";
}

/** What the timelines keep for a channel beyond what the screen draws. */
type ChannelSync = {
    /**
     * Every timeline message from the oldest held up to this id is held: only a page fetched
     * from the platform moves it, never a live copy or the person's own send, because a live
     * copy cannot prove that nothing before it was lost. Undefined until a first page arrives.
     */
    verifiedThrough: number | undefined;

    /** Bumped whenever what is held is replaced or the channel is left, so a late answer is dropped. */
    epoch: number;

    /** The catch-up run that owns the channel; a newer run, a leave or a stop takes it over. */
    runToken: number;

    /** The catch-up in progress, which a request made while it runs extends by one more pass. */
    running: Promise<void> | null;
    isRerunAsked: boolean;

    /** Whether a join has been caught up since the last newest page: the next one is a rejoin. */
    hasJoined: boolean;

    /** Whether a newest page is being read; a join confirmed meanwhile is caught up after it. */
    isLoading: boolean;
    isJoinPending: boolean;
};

/**
 * Creates the timelines.
 *
 * @param dependencies What the timelines reach outside themselves.
 *
 * @returns The timelines.
 */
export function createTimelines(dependencies: TimelineDependencies): Timelines {
    const states = new Map<string, TimelineState>();
    const syncs = new Map<string, ChannelSync>();
    const held = new Map<string, Array<{ event: string, payload: unknown }>>();
    const sleep = dependencies.sleep ?? ((ms: number): Promise<void> => new Promise(resolve => setTimeout(resolve, ms)));
    const random = dependencies.random ?? Math.random;

    // When each message was last changed by a live event, on one counter for every channel, so a
    // page read before that change cannot undo it.
    const touched = new Map<number, number>();
    let liveSeq = 0;

    /** The timeline of a channel, created empty on first ask. */
    function state(channelId: string): TimelineState {
        let timeline = states.get(channelId);

        if (!timeline) {
            timeline = reactive<TimelineState>({ messages: [], isLoaded: false, hasMore: false, readCursor: null, unreadCount: 0, isBehind: false }) as TimelineState;
            states.set(channelId, timeline);
        }

        return timeline;
    }

    /** What the timelines keep for a channel, created on first ask. */
    function sync(channelId: string): ChannelSync {
        let channelSync = syncs.get(channelId);

        if (!channelSync) {
            channelSync = { verifiedThrough: undefined, epoch: 0, runToken: 0, running: null, isRerunAsked: false, hasJoined: false, isLoading: false, isJoinPending: false };
            syncs.set(channelId, channelSync);
        }

        return channelSync;
    }

    /** Puts one message in place by id. */
    function upsert(channelId: string, message: TimelineMessage): void {
        const messages = state(channelId).messages;
        const index = positionOf(messages, message.id);

        if (index < messages.length && messages[index].id === message.id) {
            messages[index] = combine(messages[index], message);
        }
        else {
            messages.splice(index, 0, message);
        }
    }

    /**
     * Merges a page read when the live counter stood at readAt. A message changed live after
     * that keeps what the live change set: the page was read before it and would undo it.
     */
    function mergePage(channelId: string, page: HistoryPage, readAt: number): void {
        for (const message of page.messages) {
            const held = find(channelId, message.id);
            const isChangedSince = !!held && (touched.get(message.id) ?? 0) > readAt;

            upsert(channelId, isChangedSince && held
                ? {
                    ...message,
                    body: held.body,
                    metadata: held.metadata,
                    edited_at: held.edited_at,
                    deleted_at: held.deleted_at,
                    visibility: held.visibility,
                    visibility_notice: held.visibility_notice
                }
                : message);
        }
    }

    /** The message held with this id, if any. */
    function find(channelId: string, id: number): TimelineMessage | undefined {
        const messages = state(channelId).messages;
        const index = positionOf(messages, id);

        return index < messages.length && messages[index].id === id ? messages[index] : undefined;
    }

    /** The newest and oldest id of a page, whichever order it came in. */
    function bounds(page: HistoryPage): { oldest: number, newest: number } | null {
        if (page.messages.length === 0) {
            return null;
        }

        const pageIds = page.messages.map(m => m.id);
        return { oldest: Math.min(...pageIds), newest: Math.max(...pageIds) };
    }

    /** Applies one live event to a loaded timeline. A live event never moves where catch-up resumes. */
    function apply(channelId: string, event: string, payload: unknown): void {
        if (!hasId(payload)) {
            return;
        }

        if (event === "message.created") {
            const created = payload as MessageCreatedEvent;
            if (isTimelineMessage(created)) {
                // The live copy carries only the message row, so the sender's name and photo
                // come from their newest message already held, until a page brings the rest.
                const messages = state(channelId).messages;
                let sender: TimelineMessage | undefined;
                for (let i = messages.length - 1; i >= 0 && !sender; i--) {
                    if (messages[i].person_alias_guid === created.person_alias_guid && messages[i].sender_nick_name !== undefined) {
                        sender = messages[i];
                    }
                }

                // Every field the payload carries is kept, not a list of the ones known today, so
                // whatever the platform adds to a message later reaches the row unchanged.
                upsert(channelId, {
                    ...created,
                    sender_nick_name: sender?.sender_nick_name,
                    sender_last_name: sender?.sender_last_name,
                    sender_avatar_url: sender?.sender_avatar_url,
                    sender_listed: sender?.sender_listed
                } as TimelineMessage);
                touched.set(created.id, ++liveSeq);
            }
        }
        else if (event === "message.edited") {
            const edited = payload as MessageEditedEvent;
            const message = find(channelId, edited.id);
            if (message && !message.deleted_at) {
                message.body = edited.body;
                message.edited_at = edited.edited_at;
                touched.set(edited.id, ++liveSeq);
            }
        }
        else if (event === "message.deleted") {
            const deleted = payload as MessageDeletedEvent;
            const message = find(channelId, deleted.id);
            if (message) {
                message.deleted_at = deleted.deleted_at;
                message.body = null;
                message.metadata = null;
                touched.set(deleted.id, ++liveSeq);
            }
        }
        else if (event === "message.visibility") {
            const changed = payload as MessageVisibilityEvent;
            const message = find(channelId, changed.id);
            if (message) {
                message.visibility = changed.state;
                message.visibility_notice = changed.notice;

                // A hidden or removed body must not stay on screen. One shown again carries its
                // body back, so the row draws it without waiting for a page.
                if (changed.state !== "visible") {
                    message.body = null;
                    message.metadata = null;
                }
                else if (typeof changed.body === "string" && !message.deleted_at) {
                    message.body = changed.body;
                }
                touched.set(changed.id, ++liveSeq);
            }
        }
    }

    /**
     * Takes a newest page read when the live counter stood at readAt. When it joins what is held
     * (it reaches back to what was verified, or it is the whole channel) it is merged; when it
     * does not, it replaces what is held in one step, so the screen is never empty in between and
     * no gap is left in the middle. Messages above the page, a live copy or the person's own send
     * that landed after it was read, stay.
     */
    function takeNewest(channelId: string, page: HistoryPage, readAt: number): void {
        const timeline = state(channelId);
        const channelSync = sync(channelId);
        const range = bounds(page);
        const verified = channelSync.verifiedThrough;
        const isJoined = verified !== undefined && (!page.has_more || (range !== null && range.oldest <= verified));

        if (isJoined) {
            mergePage(channelId, page, readAt);
        }
        else {
            // an empty page replaces nothing: the channel has no messages yet, only what landed live
            const above = range === null ? [...timeline.messages] : timeline.messages.filter(m => m.id > range.newest);
            const previous = new Map(timeline.messages.map(m => [m.id, m]));
            timeline.messages.splice(0, timeline.messages.length, ...above);
            for (const message of page.messages) {
                const old = previous.get(message.id);
                if (old) {
                    // keep what a live change set on a message the page also carries
                    timeline.messages.splice(positionOf(timeline.messages, message.id), 0, old);
                }
            }
            mergePage(channelId, page, readAt);
            channelSync.epoch++;
            channelSync.verifiedThrough = undefined;
        }

        channelSync.verifiedThrough = Math.max(channelSync.verifiedThrough ?? 0, range?.newest ?? 0);
        timeline.hasMore = page.has_more;
        timeline.readCursor = page.read_cursor;
        timeline.unreadCount = page.unread_count;
        timeline.isLoaded = true;
    }

    /**
     * Fetches a page for the channel, refusing to hand back an answer that arrived after what is
     * held was replaced or the channel was left: the answer belongs to a timeline that is gone.
     */
    async function fetchCurrent(channelId: string, options: { limit: number, beforeId?: number, afterId?: number }): Promise<{ page: HistoryPage, readAt: number } | null> {
        const epoch = sync(channelId).epoch;
        const readAt = liveSeq;
        const page = await dependencies.fetchPage(channelId, options);

        return sync(channelId).epoch === epoch ? { page, readAt } : null;
    }

    /**
     * One pass of catch-up: page forward until nothing newer is left. A rejoin starts from the
     * oldest held message, at most a bound's worth back, so an edit, a delete or a hide made while
     * the socket was down, and a message committed late under a lower id, are read again. Past the
     * bound the channel is reloaded at its newest page and paging goes on from there. A failed
     * page is tried again with growing, randomised waits while this run still owns the channel;
     * meanwhile the channel says it is behind.
     */
    async function catchUpPass(channelId: string, token: number): Promise<void> {
        const timeline = state(channelId);
        const channelSync = sync(channelId);
        const { page: pageSize, max } = dependencies.catchUp?.() ?? defaultCatchUp;
        const isOwner = (): boolean => channelSync.runToken === token;
        const isRejoin = channelSync.hasJoined;
        channelSync.hasJoined = true;

        const verified = channelSync.verifiedThrough ?? 0;
        const settled = timeline.messages.filter(m => m.id <= verified);
        let afterId = isRejoin && settled.length > 0
            ? settled[Math.max(0, settled.length - max)].id - 1
            : verified;
        let fetched = 0;
        let attempt = 0;

        while (isOwner()) {
            const isPastBound = fetched >= max;
            let answer: { page: HistoryPage, readAt: number } | null;
            try {
                answer = await fetchCurrent(channelId, isPastBound ? { limit: firstPageSize } : { limit: pageSize, afterId });
            }
            catch (error) {
                // a run that no longer owns the channel goes quietly; a refusal that waiting cannot
                // change goes to whoever asked, which tells the person
                if (!isOwner()) {
                    return;
                }
                if (!isTransient(error)) {
                    timeline.isBehind = false;
                    throw error;
                }
                timeline.isBehind = true;
                const delay = dependencies.retryDelay?.() ?? defaultRetryDelay;
                await sleep(computeBackoff(attempt++, delay.baseMs, delay.capMs, random));
                continue;
            }

            // The channel was replaced while this page was on its way: start again from what is
            // verified now rather than merge a page that no longer joins anything.
            if (!answer) {
                afterId = channelSync.verifiedThrough ?? 0;
                continue;
            }
            if (!isOwner()) {
                return;
            }
            attempt = 0;

            if (isPastBound) {
                // Paging through more than this would keep the person waiting for history they
                // will mostly scroll past, so the channel opens fresh at its newest instead, and
                // only once the fresh page is in hand.
                takeNewest(channelId, answer.page, answer.readAt);
                afterId = channelSync.verifiedThrough ?? 0;
                fetched = 0;
                continue;
            }

            mergePage(channelId, answer.page, answer.readAt);
            const range = bounds(answer.page);
            if (range && afterId <= (channelSync.verifiedThrough ?? 0)) {
                channelSync.verifiedThrough = Math.max(channelSync.verifiedThrough ?? 0, range.newest);
            }
            fetched += answer.page.messages.filter(m => m.id > verified).length;

            if (!answer.page.has_more || !range) {
                timeline.isBehind = false;
                return;
            }
            afterId = range.newest;
        }
    }

    /**
     * Runs catch-up for a channel, one run at a time: a request made while one runs asks it for one
     * more pass, so every confirmed join is caught up after it without two runs racing.
     */
    function requestCatchUp(channelId: string): Promise<void> {
        const channelSync = sync(channelId);
        channelSync.isRerunAsked = true;

        if (channelSync.running) {
            return channelSync.running;
        }

        const token = ++channelSync.runToken;
        const run = (async (): Promise<void> => {
            try {
                while (channelSync.isRerunAsked && channelSync.runToken === token) {
                    channelSync.isRerunAsked = false;
                    await catchUpPass(channelId, token);
                }
            }
            finally {
                // a run a leave or a newer run took over has already handed the channel on
                if (channelSync.runToken === token) {
                    channelSync.running = null;
                }
            }
        })();

        channelSync.running = run;
        return run;
    }

    /** Lets go of a channel: any catch-up stops, and any answer still on its way is dropped. */
    function leave(channelId: string): void {
        const channelSync = sync(channelId);
        channelSync.runToken++;
        channelSync.epoch++;
        channelSync.running = null;
        channelSync.isRerunAsked = false;
        channelSync.isJoinPending = false;
        state(channelId).isBehind = false;
    }

    return {
        state,

        loadNewest: async (channelId: string): Promise<void> => {
            const channelSync = sync(channelId);
            channelSync.isLoading = true;

            try {
                const answer = await fetchCurrent(channelId, { limit: firstPageSize });
                if (!answer) {
                    return;
                }
                takeNewest(channelId, answer.page, answer.readAt);
                channelSync.hasJoined = false;

                const waiting = held.get(channelId) ?? [];
                held.delete(channelId);
                for (const { event, payload } of waiting) {
                    apply(channelId, event, payload);
                }
            }
            finally {
                channelSync.isLoading = false;
            }

            // A join confirmed while this page was read is caught up from it now, so catch-up
            // starts from the page on screen and not from what it replaced.
            if (channelSync.isJoinPending) {
                channelSync.isJoinPending = false;
                await requestCatchUp(channelId);
            }
        },

        loadOlder: async (channelId: string): Promise<void> => {
            const timeline = state(channelId);
            const oldest = timeline.messages[0];
            if (!oldest) {
                return;
            }

            const answer = await fetchCurrent(channelId, { limit: firstPageSize, beforeId: oldest.id });
            // dropped when what is held changed underneath it, or another older page got there first
            if (!answer || timeline.messages[0]?.id !== oldest.id) {
                return;
            }
            mergePage(channelId, answer.page, answer.readAt);
            timeline.hasMore = answer.page.has_more;
        },

        onJoined: async (channelId: string): Promise<void> => {
            const channelSync = sync(channelId);

            // Nothing on screen yet, or a newest page on its way that may replace it: the join is
            // caught up once that page is in, from it.
            if (channelSync.isLoading || !state(channelId).isLoaded) {
                channelSync.isJoinPending = true;
                return;
            }

            await requestCatchUp(channelId);
        },

        applyEvent: (channelId: string, event: string, payload: unknown): void => {
            if (!state(channelId).isLoaded) {
                const waiting = held.get(channelId) ?? [];
                waiting.push({ event, payload });
                held.set(channelId, waiting);
                return;
            }

            apply(channelId, event, payload);
        },

        upsert: (channelId: string, message: TimelineMessage): void => {
            upsert(channelId, message);
            touched.set(message.id, ++liveSeq);
        },

        leave,

        stop: (): void => {
            for (const channelId of syncs.keys()) {
                leave(channelId);
            }
        }
    };
}
