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
// a bound the channel is opened fresh at its newest page. A row withheld for someone the reader has
// since unblocked comes back only by being read again: a row read before its author was let go is
// read again by the room's next caught-up pass, newest first, a bound's worth, and what is older
// is let go and read again by scrolling.
//
// Every history event on a room's topic carries the room's revision, one above the last, and every
// page carries the revision it was read at. A revision skipped while connected is a change lost on
// the way: once the hole has stayed open past a short reorder wait, the room is repaired after a
// random wait, so a change every viewer missed is not repaired by every viewer in the same second.
// The same numbers order a page against a live change, per group of fields a change states in
// full: a page loses to a change above its revision and wins over one at or below it.
import { reactive, toRaw } from "vue";
import { classifyPlatformError } from "../errors.partial";
import { computeBackoff } from "../platformCall.partial";
import { HistoryPage, MessageCreatedEvent, Reaction, ThreadUpdatedEvent, TimelineMessage } from "../types.partial";
import { applyMessageChange, holdMessage, isMessageChange, isTimelineMessage, MessageChangeEvent, MessageMask, maskHeld } from "../heldMessage.partial";
import { defaultLimits, PlatformLimits } from "./useSession.partial";
import { changedFields, FieldGroup, isNewerChange, layNewer, revOf, RevisionLedger, takeChange } from "../revisions.partial";
import type { MessageMotion } from "../messageMotion.partial";
import type { Presence } from "./usePresence.partial";

/** How catch-up pages forward, when the settings name nothing else: estimates, not measurements. */
export const defaultCatchUp = { page: 100, max: 500 };

/** How long a failed catch-up page waits before it is tried again, at first and at most: estimates. */
export const defaultRetryDelay = { baseMs: 1000, capMs: 30000 };

/** How a missed live change is noticed and repaired when nothing is passed: the settings' own defaults, so the two never differ. */
export const defaultRevRules = { reorderMs: defaultLimits.gap_reorder_ms, repairWaitMs: defaultLimits.gap_repair_wait_ms, staleMs: defaultLimits.stale_reload_ms };

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

    /** How many messages past that position are news to the reader, as the platform counted them for the unread strip. */
    unreadCount: number;

    /** The first of them, which the New divider stands above, as the platform found it; null for none. */
    firstUnreadId: number | null;

    /** Whether a catch-up failed and is waiting to try again, so messages may be missing. */
    isBehind: boolean;

    /**
     * Every timeline message from the oldest held up to this id is held. Only a page fetched
     * from the platform moves it, never a live copy or the person's own send, because a live copy
     * cannot prove that nothing before it was lost. Null until a first page arrives.
     */
    verifiedThrough: number | null;

    /**
     * The room's revision this timeline has applied with no hole below it: every history change up
     * to it is in what is held. Null until a page that carries one arrives.
     */
    rev: number | null;
};

/** What the timeline reaches outside itself. */
export type TimelineDependencies = {
    /** Fetches a page: the newest, the page before an id, or the page after one. */
    fetchPage: (channelId: string, options: { limit: number, beforeId?: number, afterId?: number }) => Promise<HistoryPage>;

    /** The platform's limits, from the latest token exchange: how many messages a page holds. */
    limits: () => Pick<PlatformLimits, "history_page_size">;

    /** How catch-up pages forward on a rejoin, read from the settings each time. */
    catchUp?: () => { page: number, max: number };

    /** How long a failed catch-up page waits before it is tried again, read from the settings each time. */
    retryDelay?: () => { baseMs: number, capMs: number };

    /** Waits, so a test can say when; a timer by default. */
    sleep?: (ms: number) => Promise<void>;

    /** Spreads the waits of many clients apart; Math.random by default. */
    random?: () => number;

    /**
     * Told of every copy of a message sent under a key as it lands, from a page, live or put in
     * place, so the sender can let that send's row go at the first of them.
     */
    onLanded?: (message: TimelineMessage) => void;

    /**
     * Moves the chips a reaction event names on the conversation's copy of its message. The
     * reactions own that arithmetic; the timelines decide when it runs, held like every other
     * change until the channel's first page is in.
     */
    applyReaction?: (channelId: string, event: "reaction.added" | "reaction.removed", payload: unknown, isLive: boolean) => void;

    /** What the reader may see of a message, applied to every row as it is written. Required, so a holder wired without the people blocked does not compile. */
    mask: MessageMask;

    /** How a missed live change is noticed and repaired, read from the settings each time. */
    revRules?: () => { reorderMs: number, repairWaitMs: number, staleMs: number };

    /** Told of each message heard arriving live and each live edit, delete or hide as it is applied, for the rows' motion. Required, so a store wired without the record does not compile rather than losing every motion. */
    motion: Pick<MessageMotion, "arrived" | "changed">;

    /**
     * Told what each page says of typing: every message it holds now, and who the platform says is
     * typing on a page that reaches the newest. A live message is told by the live router instead,
     * after the arrival is recorded, since it can be the typist's own send.
     */
    typing?: Pick<Presence, "heldThrough" | "stated">;

    /** Runs a wait and answers its cancel, so a test can say when; a timer by default. */
    schedule?: (run: () => void, ms: number) => () => void;

    /** The time now in milliseconds; Date.now by default. */
    now?: () => number;

    /** Told of each gap, a revision skipped and not filled within the reorder wait. */
    onGap?: (channelId: string) => void;

    /** Told as a room's repair starts, so the open thread of that room reads its newest page again. */
    onRepair?: (channelId: string) => void;
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

    /**
     * Takes the revision a live event on the channel's topic carries, whatever its name, before any
     * holder reads the event: a hole that stays open is repaired. An event with none moves nothing.
     */
    advance: (channelId: string, payload: unknown) => void;

    /** A live event on the channel's topic. */
    applyEvent: (channelId: string, event: string, payload: unknown) => void;

    /** The revision each room with a timeline has applied, read as a sidebar read is asked. */
    revsHeld: () => Map<string, number | null>;

    /**
     * A sidebar row's revision, against what its timeline held when the read was asked: the room on
     * screen is repaired when it moved, a room held but not on screen is marked stale, and a room
     * with no timeline is never read. One below what was held means the room's history started
     * again, and its next read reloads the newest page.
     */
    noteSidebar: (channelId: string, rev: number, heldAtAsk: number | null, isOpen: boolean) => void;

    /** Whether a change at this revision to a group of a message is newer than the conversation's copy. */
    isNewer: (messageId: number, group: FieldGroup, rev: number | null) => boolean;

    /** Puts one message in place by id, a person's own confirmed send for one. */
    upsert: (channelId: string, message: TimelineMessage) => void;

    /**
     * Records a message's reactions as they now stand after a live change, held or not, so a page
     * read before the change does not take it back.
     */
    recordReactions: (messageId: number, reactions: Reaction[]) => void;

    /** The root whose thread is open on screen, which every page shows read while it stays open; null once none is. */
    keepThreadRead: (rootId: number | null) => void;

    /**
     * The people blocked gained these aliases: every row of every room held is masked again, in
     * place, and nothing of theirs is read again for an earlier unblock.
     */
    remask: (added: readonly string[]) => void;

    /**
     * The people blocked lost these aliases, whose withheld rows only a read gives back. A row read
     * before now and withheld for one of them is read again by its room's next caught-up pass, so
     * the room on screen gets them back from the resync the unblock starts, and a room not on screen
     * from its next open.
     */
    release: (aliases: readonly string[]) => void;

    /** The channel is no longer open: its catch-up stops and any answer on its way is dropped. */
    leave: (channelId: string) => void;

    /** The session ended: every channel is let go. */
    stop: () => void;
};

/** Whether a live event is a reaction, which carries no revision and is ordered by when it arrived. */
function isReactionEvent(event: string): event is "reaction.added" | "reaction.removed" {
    return event === "reaction.added" || event === "reaction.removed";
}

/** Whether a live payload carries the message id every event is keyed by. */
function hasId(payload: unknown): payload is { id: number } {
    return !!payload && typeof payload === "object" && typeof (payload as { id: unknown }).id === "number";
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

/**
 * Whether a failed fetch is worth trying again. A refusal of access, or of a sign-in the call path
 * already tried to renew, will not change by waiting; anything else (an unreachable platform, an
 * overloaded one, a limit) may. Read through the same classifier the rest of the client uses, so
 * the decision follows the error the shell actually throws.
 */
function isTransient(error: unknown): boolean {
    const severity = classifyPlatformError(error).severity;
    return severity !== "permission" && severity !== "session";
}

/** What the timelines keep for a channel beyond what the screen draws. */
type ChannelSync = {
    /** Bumped whenever what is held is replaced or the channel is left, so a late answer is dropped. */
    epoch: number;

    /** The catch-up run that owns the channel; a newer run, a leave or a stop takes it over. */
    runToken: number;

    /** The catch-up in progress, which a request made while it runs extends by one more pass. */
    running: Promise<void> | null;
    isRerunAsked: boolean;

    /**
     * Whether a join has been caught up on what is held: the next one is a rejoin and reads the
     * held run again. Only a newest page that replaced what was held clears it, since only then is
     * there nothing held to read again.
     */
    hasJoined: boolean;

    /** Whether a newest page is being read; a join confirmed meanwhile is caught up after it. */
    isLoading: boolean;
    isJoinPending: boolean;

    /**
     * The older page on its way: the message it is read before and the epoch it was asked in, so
     * a second ask for the same page, the button's and the top of the feed's, ends with it.
     */
    olderLoad: { beforeId: number, epoch: number, done: Promise<void> } | null;

    /**
     * The revisions seen above the one applied, which a hole keeps from being applied, each with
     * when it arrived on the counter pages are read by, so a reload forgets only what came before it.
     */
    above: Map<number, number>;

    /** The highest revision the platform has named for the room, by an event or a sidebar row. */
    known: number | null;

    /** When the room was last confirmed current: a page, or an event applied in order. */
    confirmedAt: number;

    /** The reorder wait and the repair's random wait, each cancelled by a leave or a stop. */
    cancelReorder: (() => void) | null;
    cancelRepair: (() => void) | null;

    /** The lowest revision missing when the reorder wait started, so a hole opened later gets a whole wait. */
    reorderHole: number | null;

    /** Whether the next catch-up reads the held run again whatever the revision says: a repair or a reopen. */
    isRereadDue: boolean;

    /** Whether the next newest page replaces what is held: the revision went backwards. */
    isReloadDue: boolean;

    /**
     * Whether the next catch-up reads the newest page first: the room ran past the catch-up bound
     * or went unconfirmed too long. The page is merged with what is held when it joins it, as any
     * newest page is, so the reader keeps their place.
     */
    isNewestDue: boolean;
};

/** A page as it was fetched: when it was read on the client's clock, and the revision held when it was asked. */
/** A page as fetched: the live counter and the clock when it was asked, and the revision then held. */
type Fetched = { page: HistoryPage, readAt: number, askedAt: number, heldRev: number | null };

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
    const held = new Map<string, Array<{ event: string, payload: unknown, heldAt: number }>>();
    const sleep = dependencies.sleep ?? ((ms: number): Promise<void> => new Promise(resolve => setTimeout(resolve, ms)));
    const random = dependencies.random ?? ((): number => Math.random());
    const mask = dependencies.mask;
    const schedule = dependencies.schedule ?? ((run: () => void, ms: number): (() => void) => {
        const handle = setTimeout(run, ms);
        return () => clearTimeout(handle);
    });
    const now = dependencies.now ?? Date.now;
    const rules = (): { reorderMs: number, repairWaitMs: number, staleMs: number } => dependencies.revRules?.() ?? defaultRevRules;

    // One counter for every page read and every live change, across channels, so each can be told
    // apart in time: a page read before a change, or before another page, must not undo it.
    let seq = 0;
    let isStopped = false;

    // Each message's groups by revision, so a page read before a live change does not undo it.
    const ledger: RevisionLedger = new Map();

    // What no revision orders: a reaction and the reader's own thread marks, by arrival, so a page
    // read before one that lands after it gets it laid over it rather than undoing it.
    const liveChanges = new Map<number, { seq: number, fields: Partial<TimelineMessage> }>();

    // When the page that last set each message was read on the client's own clock: an older page
    // does not overwrite what no revision orders, and an unblock knows which rows predate it.
    const pageReadAt = new Map<number, number>();

    // When each alias was let go, on the same clock a page's read is taken from, so a row withheld
    // for one of them is known to be out of date by when it was read rather than by where it sits.
    const released = new Map<string, number>();

    // The root whose thread the reader has open. Its read position is saved only when the thread
    // closes, so until then any page, read before or after it opened, still says unread.
    let openThreadRootId: number | null = null;

    /** Records a change no revision orders to a message, whether or not it is held yet. */
    function recordLive(id: number, fields: Partial<TimelineMessage>): void {
        const previous = liveChanges.get(id);
        liveChanges.set(id, { seq: ++seq, fields: { ...previous?.fields, ...fields } });
    }

    /** The timeline of a channel, created empty on first ask. */
    function state(channelId: string): TimelineState {
        let timeline = states.get(channelId);

        if (!timeline) {
            timeline = reactive<TimelineState>({ messages: [], isLoaded: false, hasMore: false, readCursor: null, unreadCount: 0, firstUnreadId: null, isBehind: false, verifiedThrough: null, rev: null }) as TimelineState;
            states.set(channelId, timeline);
        }

        return timeline;
    }

    /** What the timelines keep for a channel, created on first ask. */
    function sync(channelId: string): ChannelSync {
        let channelSync = syncs.get(channelId);

        if (!channelSync) {
            channelSync = {
                epoch: 0, runToken: 0, running: null, isRerunAsked: false, hasJoined: false, isLoading: false, isJoinPending: false, olderLoad: null,
                above: new Map(), known: null, confirmedAt: 0, cancelReorder: null, reorderHole: null, cancelRepair: null, isRereadDue: false, isReloadDue: false, isNewestDue: false
            };
            syncs.set(channelId, channelSync);
        }

        return channelSync;
    }

    /**
     * Puts one message in place by id, masked as the people blocked now stand. The mask reads what
     * is combined, so a field the newer copy does not carry, kept from what is held, is masked too.
     */
    function upsert(channelId: string, message: TimelineMessage): void {
        const messages = state(channelId).messages;
        const index = positionOf(messages, message.id);

        if (index < messages.length && messages[index].id === message.id) {
            messages[index] = holdMessage(messages[index], message, mask);
        }
        else {
            messages.splice(index, 0, holdMessage(undefined, message, mask));
        }

        // Every way a message comes in passes here, so whichever copy of a send lands first is
        // the one the sender hears of.
        if (message.client_key) {
            dependencies.onLanded?.(message);
        }
    }

    /**
     * Merges a page read when the counter stood at readAt. A message a newer page already set is
     * left as that page set it. Each group a live change moved above the page's revision keeps the
     * change, so neither an edit, a delete nor a hide is undone by a page read before it, and a
     * reaction or a thread mark that arrived after the read is laid over it. A deleted, hidden or
     * removed message never carries its body.
     */
    function mergePage(channelId: string, page: HistoryPage, readAt: number, askedAt: number): void {
        const rev = revOf(page);
        for (const message of page.messages) {
            if ((pageReadAt.get(message.id) ?? 0) > readAt && find(channelId, message.id)) {
                continue;
            }
            pageReadAt.set(message.id, readAt);

            let incoming = layNewer(ledger, message, rev);
            const change = liveChanges.get(message.id);
            if (change && change.seq > readAt) {
                incoming = { ...incoming, ...change.fields };
                // each copy its own chips, so a later change to this one moves no record
                if (change.fields.reactions) {
                    incoming.reactions = change.fields.reactions.map(reaction => ({ ...reaction }));
                }
            }
            // a live edit laid over a page the platform withheld is withheld again by upsert's mask
            if (incoming.deleted_at || (incoming.visibility !== undefined && incoming.visibility !== "visible")) {
                incoming = { ...incoming, body: null, pieces: null, blocks: null, attachments: null, metadata: null };
            }
            if (incoming.id === openThreadRootId && incoming.thread_unread === true) {
                incoming = { ...incoming, thread_unread: false };
            }

            upsert(channelId, incoming);
            dependencies.typing?.heldThrough(channelId, incoming.parent_id ?? null, incoming.person_alias_guid, incoming.id);
        }
        // read in the same statement as the page, so a typist its messages overtook is left out
        if (page.typing !== undefined) {
            dependencies.typing?.stated(channelId, null, page.typing, askedAt);
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

    /**
     * Records a thread's live counts against its root, held or not, so a page read before them
     * does not take them back. The threads have already moved a root held here and set its
     * following and unread marks by rules only they know, so what those marks now say is recorded
     * with the counts, and a page's older mark does not undo them either.
     */
    function applyThreadCounts(channelId: string, update: ThreadUpdatedEvent, rev: number | null): void {
        const fields: Partial<TimelineMessage> = {
            reply_count: update.reply_count,
            last_reply_at: update.last_reply_at,
            participant_count: update.participant_count,
            repliers: update.repliers
        };
        const newer = takeChange(ledger, update.root_id, "thread", rev, fields);
        if (!newer) {
            return;
        }

        const root = find(channelId, update.root_id);
        if (root) {
            Object.assign(root, fields, newer);
            // the repliers are faces, so this write is masked as every other one is, whenever the event arrived
            maskHeld([root], mask);
            const marks: Partial<TimelineMessage> = {};
            if (root.thread_following !== undefined) {
                marks.thread_following = root.thread_following;
            }
            if (root.thread_unread !== undefined) {
                marks.thread_unread = root.thread_unread;
            }
            recordLive(update.root_id, marks);
        }
    }

    /**
     * Applies one live event to a loaded timeline. A live event never moves where catch-up resumes.
     * One held while the first page was read is applied as part of that page, so it is told to no
     * row as something that happened: the rows it lands in are drawn with it.
     */
    function apply(channelId: string, event: string, payload: unknown, isLive: boolean): void {
        if (isReactionEvent(event)) {
            dependencies.applyReaction?.(channelId, event, payload, isLive);
            return;
        }
        const rev = revOf(payload);
        if (event === "thread.updated") {
            if (payload && typeof (payload as Partial<ThreadUpdatedEvent>).root_id === "number") {
                applyThreadCounts(channelId, payload as ThreadUpdatedEvent, rev);
            }
            return;
        }
        if (!hasId(payload)) {
            return;
        }

        if (event === "message.created") {
            const created = payload as MessageCreatedEvent;
            // a copy a page already read at or past this revision holds is newer than this one
            const isRepeat = !!find(channelId, created.id) && !isNewerChange(ledger, created.id, "message", rev);
            if (isTimelineMessage(created) && !isRepeat) {
                // The live copy names its sender; one from a platform that does not takes the
                // name and photo from the sender's newest message already held.
                const messages = state(channelId).messages;
                let sender: Pick<TimelineMessage, "sender_nick_name" | "sender_last_name" | "sender_avatar_url" | "sender_listed"> | undefined
                    = created.sender_nick_name !== undefined ? created : undefined;
                for (let i = messages.length - 1; i >= 0 && !sender; i--) {
                    if (messages[i].person_alias_guid === created.person_alias_guid && messages[i].sender_nick_name !== undefined) {
                        sender = messages[i];
                    }
                }

                // Every field the payload carries is kept, not a list of the ones known today, so
                // whatever the platform adds to a message later reaches the row unchanged. A change
                // that arrived before it, above its revision, is laid over it.
                upsert(channelId, layNewer(ledger, {
                    ...created,
                    sender_nick_name: sender?.sender_nick_name,
                    sender_last_name: sender?.sender_last_name,
                    sender_avatar_url: sender?.sender_avatar_url,
                    sender_listed: sender?.sender_listed
                } as TimelineMessage, rev));
                if (isLive) {
                    dependencies.motion.arrived(created);
                }
            }
        }
        else if (isMessageChange(event)) {
            const fields = changedFields(event, payload);
            if (fields) {
                changeMessage(channelId, event, payload, rev, fields, isLive);
            }
        }
    }

    /**
     * Lays an edit, a delete or a change of visibility over the message held, when it is newer than
     * what is held, and records it, held or not, for a page read before it.
     */
    function changeMessage(channelId: string, event: MessageChangeEvent, payload: { id: number }, rev: number | null, fields: Partial<TimelineMessage>, isLive: boolean): void {
        const newer = takeChange(ledger, payload.id, "message", rev, fields);
        if (!newer) {
            return;
        }
        const message = find(channelId, payload.id);
        if (message) {
            applyMessageChange(message, event, payload, newer);
            // a change in place is a write like any other: its pieces may name someone blocked
            maskHeld([message], mask);
            if (isLive) {
                dependencies.motion.changed(message, event);
            }
        }
    }

    /**
     * Takes a newest page read when the live counter stood at readAt. When it joins what is held
     * (it reaches back to what was verified, or it is the whole channel) it is merged; when it
     * does not, it replaces what is held in one step, so the screen is never empty in between and
     * no gap is left in the middle. Messages above the page, a live copy or the person's own send
     * that landed after it was read, stay. A reload, for a history that started again, replaces what
     * is held whether it joins or not. A page that replaces what is held is where the room's revision
     * is counted from.
     */
    function takeNewest(channelId: string, page: HistoryPage, readAt: number, askedAt: number, isReload: boolean = false): boolean {
        const timeline = state(channelId);
        const channelSync = sync(channelId);
        const range = bounds(page);
        const verified = timeline.verifiedThrough;
        const isJoined = !isReload && verified !== null && (!page.has_more || (range !== null && range.oldest <= verified));
        // a newest page is in hand, which is what a room far behind was waiting for
        channelSync.isNewestDue = false;

        if (isJoined) {
            mergePage(channelId, page, readAt, askedAt);
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
            mergePage(channelId, page, readAt, askedAt);
            channelSync.epoch++;
            timeline.verifiedThrough = null;
            countFrom(channelId, revOf(page), isReload ? readAt : null);
        }

        timeline.verifiedThrough = Math.max(timeline.verifiedThrough ?? 0, range?.newest ?? 0);
        timeline.hasMore = page.has_more;
        timeline.readCursor = page.read_cursor;
        timeline.unreadCount = page.unread_count;
        timeline.firstUnreadId = page.first_unread_id ?? null;
        timeline.isLoaded = true;
        return !isJoined;
    }

    /**
     * Fetches a page for the channel, refusing to hand back an answer that arrived after what is
     * held was replaced or the channel was left: the answer belongs to a timeline that is gone.
     */
    async function fetchCurrent(channelId: string, options: { limit: number, beforeId?: number, afterId?: number }): Promise<Fetched | null> {
        const epoch = sync(channelId).epoch;
        const readAt = ++seq;
        // the clock as it is asked, before the request goes, so a typing signal heard while it is
        // out is known to be newer than the answer
        const askedAt = Date.now();
        // what was applied when asked, so a page is called backwards only against that, not against
        // live changes that arrived while it was out
        const heldRev = state(channelId).rev;
        const page = await dependencies.fetchPage(channelId, options);

        return sync(channelId).epoch === epoch ? { page, readAt, askedAt, heldRev } : null;
    }

    /**
     * One pass of catch-up: page forward until nothing newer is left. A repair, a reopen, and a
     * rejoin whose room moved start from the oldest held message, at most a bound's worth back, so
     * an edit, a delete or a hide made while the socket was down, and a message committed late under
     * a lower id, are read again; once that read is done, the room holds every change up to the
     * revision its first page was read at. A rejoin of the room on screen with nothing missed reads
     * forward first and reads the held run again only when that page's revision says the room moved
     * meanwhile: a Realtime restart rejoins every viewer at once, and most rooms have not moved. Past
     * the bound, or when the revision went backwards, the channel is reloaded at its newest page and
     * paging goes on from there. Once caught up it reads again the rows read before an unblock. A
     * failed page is tried again with growing, randomised waits while this run still owns the
     * channel; meanwhile the channel says it is behind.
     */
    async function catchUpPass(channelId: string, token: number): Promise<void> {
        const timeline = state(channelId);
        const channelSync = sync(channelId);
        const { page: pageSize, max } = dependencies.catchUp?.() ?? defaultCatchUp;
        const isOwner = (): boolean => channelSync.runToken === token;
        const isFirst = !channelSync.hasJoined;
        channelSync.hasJoined = true;

        // a room whose revision is unknown, or has a hole, cannot say it has not moved
        const isRevisionSilent = timeline.rev === null || channelSync.above.size > 0;
        let isReread = channelSync.isRereadDue || (!isFirst && isRevisionSilent);
        // a first join and a rejoin with nothing missed read forward first, and read the held run
        // again only when that page says the room moved since what was applied
        let isForwardFirst = !isReread;
        let isNewestFirst = channelSync.isNewestDue;
        channelSync.isRereadDue = false;
        channelSync.isNewestDue = false;

        const verified = timeline.verifiedThrough ?? 0;
        const settled = timeline.messages.filter(m => m.id <= verified);
        const rereadFrom = settled.length > 0 ? settled[Math.max(0, settled.length - max)].id - 1 : verified;
        let afterId = isReread ? rereadFrom : verified;
        let fetched = 0;
        let attempt = 0;
        // the lowest revision a page of the reread was read at, which the whole reread holds
        let rereadRev: number | null = null;

        while (isOwner()) {
            const isPastBound = fetched >= max || channelSync.isReloadDue || isNewestFirst;
            let answer: Fetched | null;
            try {
                answer = await fetchCurrent(channelId, isPastBound ? { limit: dependencies.limits().history_page_size } : { limit: pageSize, afterId });
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
                afterId = timeline.verifiedThrough ?? 0;
                continue;
            }
            if (!isOwner()) {
                return;
            }
            attempt = 0;

            if (isPastBound) {
                // Paging through more than this would keep the person waiting for history they
                // will mostly scroll past, so the channel opens fresh at its newest instead, and
                // only once the fresh page is in hand. A newest page asked first because the room is
                // far behind, when it joins what is held, is merged and the held run read again
                // behind it, so the reader keeps their place and the room still holds every change
                // up to what the reread was read at.
                const isReplaced = takeNewest(channelId, answer.page, answer.readAt, answer.askedAt, channelSync.isReloadDue);
                if (isReplaced) {
                    isReread = false;
                }
                afterId = !isReplaced && isNewestFirst && isReread ? rereadFrom : timeline.verifiedThrough ?? 0;
                isNewestFirst = false;
                fetched = 0;
                continue;
            }

            const rev = revOf(answer.page);
            if (rev !== null && answer.heldRev !== null && rev < answer.heldRev) {
                // the room's history started again: what is held is counted against numbers that
                // no longer mean anything, so the room is reloaded rather than merged
                channelSync.isReloadDue = true;
                continue;
            }
            if (isForwardFirst) {
                isForwardFirst = false;
                // A first join reads past its first page what was sent while the subscription came
                // up, so an edit made then is caught here rather than left for the live path to find
                // as a loss. A first join with no revision to compare reads forward only, as before.
                const isMoved = rev === null || answer.heldRev === null ? !isFirst : rev !== answer.heldRev;
                if (isMoved) {
                    isReread = true;
                    mergePage(channelId, answer.page, answer.readAt, answer.askedAt);
                    afterId = rereadFrom;
                    continue;
                }
            }
            if (isReread && rev !== null) {
                rereadRev = Math.min(rereadRev ?? rev, rev);
            }

            mergePage(channelId, answer.page, answer.readAt, answer.askedAt);
            const range = bounds(answer.page);
            if (range && afterId <= (timeline.verifiedThrough ?? 0)) {
                timeline.verifiedThrough = Math.max(timeline.verifiedThrough ?? 0, range.newest);
            }
            fetched += answer.page.messages.filter(m => m.id > verified).length;

            if (!answer.page.has_more || !range) {
                timeline.isBehind = false;
                caughtUp(channelId, isReread ? rereadRev : null);
                // inside the pass, so the run owns it and a join confirmed meanwhile gets a pass after it
                await rereadReleased(channelId, isOwner);
                return;
            }
            afterId = range.newest;
        }
    }

    /**
     * Where a room's revision is counted from: a page that replaced what is held. A reload forgets
     * the revisions seen before it was asked, which may belong to a history that started again, and
     * keeps those that arrived while it was out; any other page keeps every event above it.
     *
     * @param forgetBefore For a reload, when it was asked on the counter; null for any other page.
     */
    function countFrom(channelId: string, rev: number | null, forgetBefore: number | null): void {
        const timeline = state(channelId);
        const channelSync = sync(channelId);
        channelSync.isReloadDue = false;
        channelSync.isRereadDue = false;
        if (rev === null) {
            return;
        }
        if (forgetBefore !== null) {
            for (const [seen, arrivedAt] of [...channelSync.above]) {
                if (arrivedAt < forgetBefore) {
                    channelSync.above.delete(seen);
                }
            }
            channelSync.known = Math.max(rev, ...channelSync.above.keys());
        }
        timeline.rev = rev;
        channelSync.confirmedAt = now();
        settleRevisions(channelId);
    }

    /**
     * A catch-up pass reached the newest message. One that read the whole held run again holds every
     * change up to the revision its pages were read at.
     */
    function caughtUp(channelId: string, rereadRev: number | null): void {
        const timeline = state(channelId);
        const channelSync = sync(channelId);
        channelSync.confirmedAt = now();
        if (rereadRev !== null) {
            timeline.rev = Math.max(timeline.rev ?? rereadRev, rereadRev);
        }
        if (timeline.rev !== null) {
            settleRevisions(channelId);
        }
    }

    /**
     * Applies every revision seen in order above the one applied, and starts the reorder wait for a
     * hole still open: realtime may hand two changes over out of order, so a hole is a gap only once
     * it has stayed open that long.
     */
    function settleRevisions(channelId: string): void {
        const timeline = state(channelId);
        const channelSync = sync(channelId);
        let applied = timeline.rev ?? 0;
        for (const rev of [...channelSync.above.keys()]) {
            if (rev <= applied) {
                channelSync.above.delete(rev);
            }
        }
        while (channelSync.above.delete(applied + 1)) {
            applied++;
            channelSync.confirmedAt = now();
        }
        timeline.rev = applied;

        // each hole gets the whole wait: one that opens as an earlier one fills starts it again
        const hole = channelSync.above.size === 0 ? null : applied + 1;
        if (hole !== channelSync.reorderHole) {
            channelSync.cancelReorder?.();
            channelSync.cancelReorder = null;
            channelSync.reorderHole = null;
        }
        if (hole !== null && !channelSync.cancelReorder && !isStopped) {
            channelSync.reorderHole = hole;
            channelSync.cancelReorder = schedule(() => {
                channelSync.cancelReorder = null;
                channelSync.reorderHole = null;
                if (channelSync.above.size > 0) {
                    dependencies.onGap?.(channelId);
                    scheduleRepair(channelId);
                }
            }, rules().reorderMs);
        }
    }

    /**
     * Repairs the room once, after a random wait up to the setting's: every viewer of a room misses
     * the same lost change at once, and the wait spreads their reads. Gaps found meanwhile share it.
     */
    function scheduleRepair(channelId: string): void {
        const channelSync = sync(channelId);
        if (isStopped || channelSync.cancelRepair) {
            return;
        }
        channelSync.cancelRepair = schedule(() => {
            channelSync.cancelRepair = null;
            void repair(channelId);
        }, random() * rules().repairWaitMs);
    }

    /**
     * Whether the room's history started again, so what is held is replaced rather than merged: a
     * page or a sidebar row below what was applied when it was asked.
     *
     * @param pageRev The revision of a newest page in hand, if any, and what was applied when it was asked.
     */
    function isRestarted(channelId: string, pageRev: number | null = null, heldRev: number | null = null): boolean {
        return sync(channelId).isReloadDue || (pageRev !== null && heldRev !== null && pageRev < heldRev);
    }

    /**
     * Whether the room is far enough behind that its repair reads the newest page first: it moved
     * past the catch-up bound, or nothing has confirmed it for so long that paging through what
     * changed would cost more than starting at the newest.
     */
    function isFarBehind(channelId: string): boolean {
        const applied = state(channelId).rev;
        if (applied === null) {
            return false;
        }
        const channelSync = sync(channelId);
        const { max } = dependencies.catchUp?.() ?? defaultCatchUp;
        return (channelSync.known ?? applied) - applied > max || now() - channelSync.confirmedAt > rules().staleMs;
    }

    /**
     * Repairs a room that missed a change: the open thread reads its newest page again, and the
     * timeline reads its held run again by the catch-up it already has, a room far behind its newest
     * page first. A catch-up already going is the room's read, so the repair waits for it and asks
     * for one only if the room is still behind after it. A leave, a stop or a replace meanwhile ends
     * the repair, since the room it was for is gone and a hole still open is found again. A refusal
     * is not told: the join, the hub and the next open each tell the person of a room they lost.
     */
    async function repair(channelId: string): Promise<void> {
        const timeline = state(channelId);
        const channelSync = sync(channelId);
        const epoch = channelSync.epoch;
        if (channelSync.running) {
            await channelSync.running.catch(() => undefined);
        }
        const isCurrent = timeline.rev !== null && channelSync.above.size === 0 && (channelSync.known ?? 0) <= timeline.rev;
        if (isStopped || channelSync.epoch !== epoch || (isCurrent && !channelSync.isReloadDue)) {
            return;
        }
        dependencies.onRepair?.(channelId);
        channelSync.isNewestDue = isFarBehind(channelId);
        channelSync.isRereadDue = true;
        // the one await is the run's own, which a leave or a stop ends; a refusal, as above, is not told
        await requestCatchUp(channelId).catch(() => undefined);
    }

    /**
     * Runs catch-up for a channel, one run at a time: a request made while one runs asks it for one
     * more pass, so every confirmed join is caught up after it without two runs racing.
     */
    function requestCatchUp(channelId: string): Promise<void> {
        if (isStopped) {
            return Promise.resolve();
        }
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

    /** Lets go of every message held older than this id; the feed reads them again by scrolling. */
    function letGoBefore(channelId: string, id: number): void {
        const timeline = state(channelId);
        const count = positionOf(timeline.messages, id);
        if (count > 0) {
            timeline.messages.splice(0, count);
            timeline.hasMore = true;
        }
    }

    /** Whether a row is withheld for someone let go after it was read, so only a read now gives it back. */
    function isReadBeforeRelease(message: TimelineMessage): boolean {
        if (released.size === 0 || (!message.blocked && !message.quoted?.blocked)) {
            return false;
        }
        // a live row no page has read counts as read before any release
        const readAt = pageReadAt.get(message.id) ?? 0;
        const releasedAt = (alias: string | null | undefined): number => alias ? released.get(alias.toLowerCase()) ?? 0 : 0;
        return (!!message.blocked && releasedAt(message.person_alias_guid) > readAt)
            || (!!message.quoted?.blocked && releasedAt(message.quoted.person_alias_guid) > readAt);
    }

    /** The newest held row older than this id that was read before an unblock, if any. */
    function newestStaleBefore(channelId: string, id: number): TimelineMessage | undefined {
        const messages = toRaw(state(channelId).messages);
        for (let i = positionOf(messages, id) - 1; i >= 0; i--) {
            if (isReadBeforeRelease(messages[i])) {
                return messages[i];
            }
        }
        return undefined;
    }

    /**
     * The tail of a caught-up pass: reads again, newest first, the pages holding a row read before
     * an unblock, skipping pages that hold none, up to a bound's worth of rows; past the bound what
     * is older is let go and read by scrolling. A held row inside a page's span that the page does
     * not carry is no longer on the timeline, so it is let go rather than asked for by every later
     * pass. A failed read waits and tries again as the pass does, without saying the room is
     * behind, since it is caught up on what is new; a refusal ends it as it ends the pass. A leave,
     * a newer run, a replace or a stop ends it with the rows kept, still out of date, for the next
     * pass.
     */
    async function rereadReleased(channelId: string, isOwner: () => boolean): Promise<void> {
        const { page: pageSize, max } = dependencies.catchUp?.() ?? defaultCatchUp;
        const timeline = state(channelId);
        let before = Number.MAX_SAFE_INTEGER;
        let read = 0;
        let attempt = 0;

        for (let next = newestStaleBefore(channelId, before); next && isOwner(); next = newestStaleBefore(channelId, before)) {
            if (read >= max) {
                letGoBefore(channelId, before);
                return;
            }

            let answer: Fetched | null;
            try {
                answer = await fetchCurrent(channelId, { limit: pageSize, beforeId: next.id + 1 });
            }
            catch (error) {
                if (!isOwner()) {
                    return;
                }
                if (!isTransient(error)) {
                    throw error;
                }
                const delay = dependencies.retryDelay?.() ?? defaultRetryDelay;
                await sleep(computeBackoff(attempt++, delay.baseMs, delay.capMs, random));
                continue;
            }
            if (!answer || !isOwner()) {
                return;
            }
            attempt = 0;

            mergePage(channelId, answer.page, answer.readAt, answer.askedAt);
            read += answer.page.messages.length;
            // strictly falling, so whatever the platform answers the walk ends
            const oldest = Math.min(bounds(answer.page)?.oldest ?? next.id, next.id);
            const carried = new Set(answer.page.messages.map(m => m.id));
            const messages = toRaw(timeline.messages);
            for (let i = positionOf(messages, next.id + 1) - 1; i >= 0 && messages[i].id >= oldest; i--) {
                if (!carried.has(messages[i].id) && isReadBeforeRelease(messages[i])) {
                    timeline.messages.splice(i, 1);
                }
            }
            before = oldest;
        }
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
        // events queued for a first page that never came would replay over a later, newer one
        held.delete(channelId);
        // a room off screen hears nothing, so its waits stop, and the next open of a room it joined
        // reads it again, whatever its revision says, for what no channel event carries
        channelSync.cancelReorder?.();
        channelSync.cancelReorder = null;
        channelSync.reorderHole = null;
        channelSync.cancelRepair?.();
        channelSync.cancelRepair = null;
        channelSync.isRereadDue = channelSync.isRereadDue || channelSync.hasJoined;
    }

    /** Fetches the newest page and applies anything that arrived live while it was in flight. */
    async function loadNewest(channelId: string): Promise<void> {
            if (isStopped) {
                return;
            }
            const channelSync = sync(channelId);
            channelSync.isLoading = true;
            let failure: { error: unknown } | null = null;

            try {
                const answer = await fetchCurrent(channelId, { limit: dependencies.limits().history_page_size });
                if (answer && !isStopped) {
                    if (takeNewest(channelId, answer.page, answer.readAt, answer.askedAt, isRestarted(channelId, revOf(answer.page), answer.heldRev))) {
                        channelSync.hasJoined = false;
                    }

                    const waiting = held.get(channelId) ?? [];
                    held.delete(channelId);
                    for (const { event, payload, heldAt } of waiting) {
                        // A reaction carries no revision, so one that arrived before this page was
                        // read is older than the count the page shows; anything else its revision orders.
                        if (isReactionEvent(event) && heldAt < answer.readAt) {
                            continue;
                        }
                        apply(channelId, event, payload, false);
                    }
                }
            }
            catch (error) {
                failure = { error };
            }
            finally {
                channelSync.isLoading = false;
            }

            // A join confirmed while this page was read is caught up now, however the read ended:
            // from the page when it landed, and from what was already held when it failed or was
            // overtaken, so a reload that fails never strands a rejoin. A channel left meanwhile
            // has no join pending, and one with nothing on screen keeps it for its first page.
            const isCatchUpDue = channelSync.isJoinPending && state(channelId).isLoaded;
            if (isCatchUpDue) {
                channelSync.isJoinPending = false;
            }
            if (failure) {
                if (isCatchUpDue) {
                    // the reload's own failure is what the caller reports; this catch-up retries
                    // on its own and a refusal it meets is the one the reload already met
                    void requestCatchUp(channelId).catch(() => undefined);
                }
                throw failure.error;
            }
            if (isCatchUpDue) {
                await requestCatchUp(channelId);
            }
    }

    return {
        state,

        loadNewest,

        loadOlder: (channelId: string): Promise<void> => {
            const timeline = state(channelId);
            const oldest = timeline.messages[0];
            if (!oldest) {
                return Promise.resolve();
            }

            const channelSync = sync(channelId);
            const asked = channelSync.olderLoad;
            if (asked && asked.beforeId === oldest.id && asked.epoch === channelSync.epoch) {
                return asked.done;
            }

            const load = { beforeId: oldest.id, epoch: channelSync.epoch, done: Promise.resolve() };
            channelSync.olderLoad = load;
            load.done = (async (): Promise<void> => {
                try {
                    const answer = await fetchCurrent(channelId, { limit: dependencies.limits().history_page_size, beforeId: oldest.id });
                    // dropped when what is held changed underneath it, or another older page got there first
                    if (!answer || timeline.messages[0]?.id !== oldest.id) {
                        return;
                    }
                    mergePage(channelId, answer.page, answer.readAt, answer.askedAt);
                    timeline.hasMore = answer.page.has_more;
                }
                finally {
                    if (channelSync.olderLoad === load) {
                        channelSync.olderLoad = null;
                    }
                }
            })();
            return load.done;
        },

        onJoined: async (channelId: string): Promise<void> => {
            if (isStopped) {
                return;
            }
            const channelSync = sync(channelId);

            // Nothing on screen yet, or a newest page on its way that may replace it: the join is
            // caught up once that page is in, from it.
            if (channelSync.isLoading || !state(channelId).isLoaded) {
                channelSync.isJoinPending = true;
                return;
            }

            await requestCatchUp(channelId);
        },

        advance: (channelId: string, payload: unknown): void => {
            const rev = revOf(payload);
            if (rev === null || isStopped) {
                return;
            }
            const timeline = state(channelId);
            const channelSync = sync(channelId);
            channelSync.known = Math.max(channelSync.known ?? rev, rev);
            if (timeline.rev !== null && rev <= timeline.rev) {
                return;
            }
            if (!channelSync.above.has(rev)) {
                channelSync.above.set(rev, ++seq);
            }
            // until a page says where the room stands, what arrived waits for it
            if (timeline.rev !== null) {
                settleRevisions(channelId);
            }
        },

        revsHeld: (): Map<string, number | null> => new Map([...states].filter(([, timeline]) => timeline.isLoaded).map(([id, timeline]) => [id, timeline.rev])),

        noteSidebar: (channelId: string, rev: number, heldAtAsk: number | null, isOpen: boolean): void => {
            const timeline = states.get(channelId);
            if (isStopped || !timeline?.isLoaded) {
                return;
            }
            const channelSync = sync(channelId);
            if (heldAtAsk !== null && rev < heldAtAsk) {
                channelSync.isReloadDue = true;
                channelSync.known = rev;
            }
            else {
                channelSync.known = Math.max(channelSync.known ?? rev, rev);
                if (timeline.rev === null || rev <= timeline.rev) {
                    return;
                }
            }
            // a room held but not on screen reads itself again on its next open, from its newest page first
            // when the revision just recorded puts it far behind
            if (isOpen) {
                scheduleRepair(channelId);
            }
        },

        isNewer: (messageId: number, group: FieldGroup, rev: number | null): boolean => isNewerChange(ledger, messageId, group, rev),

        applyEvent: (channelId: string, event: string, payload: unknown): void => {
            if (!state(channelId).isLoaded) {
                const waiting = held.get(channelId) ?? [];
                waiting.push({ event, payload, heldAt: ++seq });
                held.set(channelId, waiting);
                return;
            }

            apply(channelId, event, payload, true);
        },

        upsert,

        recordReactions: (messageId: number, reactions: Reaction[]): void => {
            recordLive(messageId, { reactions: reactions.map(reaction => ({ ...reaction })) });
        },

        keepThreadRead: (rootId: number | null): void => {
            openThreadRootId = rootId;
        },

        remask: (added: readonly string[]): void => {
            // a person blocked again is withheld by the set from here on, so nothing is read for them
            for (const alias of added) {
                released.delete(alias.toLowerCase());
            }
            for (const timeline of states.values()) {
                maskHeld(timeline.messages, mask);
            }
        },

        release: (aliases: readonly string[]): void => {
            // one moment for all of them: a row read before it is out of date, one read after it is not
            const at = ++seq;
            for (const alias of aliases) {
                released.set(alias.toLowerCase(), at);
            }
        },

        leave,

        stop: (): void => {
            // for good: a join or a reload that fires while the session ends starts nothing
            isStopped = true;
            for (const channelId of syncs.keys()) {
                leave(channelId);
            }
        }
    };
}
