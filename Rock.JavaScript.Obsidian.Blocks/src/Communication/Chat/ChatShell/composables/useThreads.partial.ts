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
// Threads: the threads list, the open thread, following, the thread's read position and the
// live counts of a thread's root.
//
// The list is fetched when the panel opens, when the tab changes, when the person presses refresh
// and when it is scrolled to its end, and at no other time, so a busy room costs the platform
// nothing while the panel sits open. Its rows are held copies of their threads all the same, so
// the thread.updated a room already sends changes each row as it moves the conversation's pill:
// its counts, faces and marks, in its place. The order is the platform's and settles at the next
// fetch, so no row moves out from under the person's focus or pointer. The list keeps its own
// record of the changes it took, by revision, so a page read before one does not undo it. Every
// fetch is numbered, and an answer a newer fetch overtook is dropped. Once the threads are
// stopped nothing is fetched and no answer still on its way reaches the screen.
//
// The thread's read position is saved as the channel's is, on events only: closing the thread,
// the window losing focus, the page going to the background or closing. What is saved is the
// newest reply that was on screen, and nothing is sent when nothing newer than the stored
// position was seen. A root's counts and faces follow thread.updated wherever the root is held.
//
// Following saves by the one save rule the person's settings use: the bell changes at once
// everywhere the thread is drawn, the newest change wins, and a refusal puts it back with the
// reason, and Try again where another try can pass.
/* eslint-disable @typescript-eslint/naming-convention */
import { reactive } from "vue";
import { CallResult } from "../platformCall.partial";
import {
    ChatError,
    FailureWords,
    FollowThreadAnswer,
    MarkThreadReadAnswer,
    MessageCreatedEvent,
    Replier,
    ThreadListPage,
    ThreadListRow,
    ThreadPage,
    ThreadUpdatedEvent,
    TimelineMessage
} from "../types.partial";
import { defaultCatchUp, Timelines } from "./useHistory.partial";
import { applyMessageChange, holdMessage, maskHeld, MessageMask, withSendKey } from "../heldMessage.partial";
import type { MessageMotion } from "../messageMotion.partial";
import { changedFields, heldRevisions, layNewer, revOf, RevisionLedger, takeChange } from "../revisions.partial";
import { PageEventTargets } from "./useMarkRead.partial";
import { PlatformLimits } from "./useSession.partial";
import { createSaveRule, SaveOutcome } from "./saveRule.partial";
import { failureWords } from "../errors.partial";

/** A place in a thread: its root, and the reply to land on, or null to open it as it opens. */
export type ThreadPlace = { rootId: number, messageId: number | null };

/**
 * Where a message's thread opens. Only a root has a thread: a root opens its own, and a reply,
 * one also sent to its conversation included, is a message of its root's thread, which opens
 * landed on it.
 *
 * @param message The message.
 *
 * @returns Its thread's root, and the reply to land on.
 */
export function threadPlaceOf(message: Pick<TimelineMessage, "id" | "parent_id">): ThreadPlace {
    return message.parent_id ? { rootId: message.parent_id, messageId: message.id } : { rootId: message.id, messageId: null };
}

/** Which threads the list shows: the open conversation's, or every thread the person follows. */
export type ThreadListTab = "channel" | "all";

/** Where the list's next page starts: its last row's newest reply and root, as the platform listed them. */
type ListCursor = { last_reply_at: string | null, id: number };

/** A thread's counts and faces, as thread.updated states them and every held copy draws them. */
type ThreadCounts = Pick<ThreadUpdatedEvent, "reply_count" | "last_reply_at" | "participant_count" | "repliers">;

/** The reader's marks on a thread: whether they follow it and whether it is bold. */
type ThreadMarks = { isFollowing: boolean, isUnread: boolean };

/** The threads list as the panel draws it. */
export type ThreadListState = {
    tab: ThreadListTab;

    /** The conversation the list was opened from. */
    channelId: string | null;
    rows: ThreadListRow[];
    hasMore: boolean;

    /** A fresh list is on its way, with nothing of it on screen yet. */
    isLoading: boolean;

    /** The rows on screen are being fetched again. */
    isRefreshing: boolean;

    /** Why the last fetch failed, shown in place while the rows it could not replace stay. */
    error: ChatError | null;

};

/** The newest refused follow change: the thread, the change asked, which Try again asks again, and why. */
export type FollowError = { rootId: number, isFollowing: boolean, failure: FailureWords };

/** The open thread as the drawer draws it. */
export type OpenThreadState = {
    channelId: string;
    rootId: number;

    /** The root, null until the first page arrives. */
    root: TimelineMessage | null;

    /** The replies held, oldest first. */
    replies: TimelineMessage[];
    following: boolean;

    /** The reader's place in the thread as the platform last stored it. */
    readCursor: number | null;
    hasMore: boolean;
    isLoading: boolean;

    /** Why the first page failed; until it answers, whether the reader follows is not known. */
    error: ChatError | null;

    /** Why the last page of older replies failed, cleared by the next that answers. */
    olderError: ChatError | null;
};

/** What the threads hold for the screen. */
export type ThreadsState = {
    list: ThreadListState;
    thread: OpenThreadState | null;

    /** Shown beside the thread's bell in the list and the drawer, until the thread is changed again. */
    followError: FollowError | null;
};

/** What the threads reach outside themselves. */
export type ThreadsDependencies = {
    /** The shell's call path; keepalive for a save that must survive the page closing. */
    call: (name: string, args: Record<string, unknown>, options?: { keepalive?: boolean }) => Promise<CallResult>;

    /** The channel timelines, whose roots a thread's changes move. */
    timelines: Pick<Timelines, "state" | "keepThreadRead" | "isNewer">;

    /** The reader, so their own reply does not mark a thread unread. */
    personAliasGuid: string;

    /** The platform's limits, from the latest token exchange: a thread's page and the list's. */
    limits: () => Pick<PlatformLimits, "history_page_size" | "thread_page_size">;

    /** How a reread pages back through the replies held, as the timeline's catch-up does: each page, and at most how many in all. */
    catchUp?: () => { page: number, max: number };

    /**
     * Told of every copy of a reply sent under a key as it reaches the open thread, so the sender
     * can let that send's row go: a reply shown only in its thread never reaches a timeline.
     */
    onLanded?: (message: TimelineMessage) => void;

    /** Whether the live connection is lost, which decides whether a follow change that reached nothing says the person is offline. */
    isOffline?: () => boolean;

    /** What the reader may see of a message, applied to every root and reply as it is written. Required, so a holder wired without the people blocked does not compile. */
    mask: MessageMask;

    /** Whether an alias is someone the reader blocked, whose reply is not news to them. Required, for the same reason. */
    isBlocked: (personAliasGuid: string | null) => boolean;

    /** Told of each reply heard arriving live, each live change to what the drawer holds, and each thread's first reply, for the rows' motion. Required, so a store wired without the record does not compile rather than losing every motion. */
    motion: Pick<MessageMotion, "arrived" | "changed" | "threadStarted">;
};

/** The threads a shell holds. */
export type Threads = {
    state: ThreadsState;

    /** Opens the list for a conversation, on the tab last chosen unless one is named. */
    openList: (channelId: string, tab?: ThreadListTab) => Promise<void>;

    /** Shows the other tab, fetched afresh. */
    switchTab: (tab: ThreadListTab) => Promise<void>;

    /** Fetches the list on screen again, keeping its rows until the answer replaces them. */
    refresh: () => Promise<void>;

    /** Fetches the next page after the last row, when the list is scrolled to its end. */
    loadMore: () => Promise<void>;

    /** The panel closed: any list answer still on its way is dropped. */
    closeList: () => void;

    /** Follows or unfollows a thread. Resolves true when the platform took the change. */
    follow: (rootId: number, isFollowing: boolean) => Promise<boolean>;

    /**
     * Opens a thread at its newest replies, saving the position of any thread it replaces. The
     * thread open already is only waited on, unless its first page failed.
     */
    openThread: (channelId: string, rootId: number) => Promise<void>;

    /** Whether the thread is the one open, loading or loaded, which opening it again leaves as it is. */
    isOpen: (channelId: string, rootId: number) => boolean;

    /** Fetches the replies before the oldest held; asked again while that page is on its way, waits for it. */
    loadOlderReplies: () => Promise<void>;

    /** A reply of the open thread was on screen. */
    seen: (messageId: number) => void;

    /** Closes the thread and saves its position if a newer reply was seen. */
    closeThread: () => Promise<void>;

    /** Saves on background, lost focus and close. Returns what removes the listeners. */
    attach: (targets: PageEventTargets) => () => void;

    /** A live event on a channel topic. */
    applyEvent: (channelId: string, event: string, payload: unknown) => void;

    /** The person's own reply was confirmed: it goes into the open thread at once, and the thread is followed. */
    upsertReply: (channelId: string, message: TimelineMessage) => void;

    /**
     * Runs a change to the open thread's messages: at once when its page is in, or once it lands
     * when it is still on its way, so a page read before the change does not undo it. Nothing runs
     * when no thread of the channel is open.
     */
    whenLoaded: (channelId: string, change: () => void) => void;

    /**
     * What the threads hold on screen may be out of date in a way no event describes, as when the
     * people the person blocked change: the list, while shown, and the open thread read again.
     */
    resync: () => void;

    /** The room missed a live change: its open thread, if any, reads its newest page again. */
    repair: (channelId: string) => void;

    /** The people blocked gained someone: the open thread and the list's roots are masked again, in place. */
    remask: () => void;

    /** Stops for good: nothing more is fetched and no answer reaches the screen. */
    stop: () => void;
};

/** A toast that is not a failure, in the shape the toast draws. */
export type FollowNotice = {
    code: string;
    text: string;
    icon: string;
};

/**
 * The toast an unfollow shows once it lands, saying what it means as a notify mode's does.
 * Following says nothing: the bell shows it.
 */
export const unfollowNotice: FollowNotice = { code: "thread-unfollowed", text: "You won't be notified about new replies", icon: "bell-off" };

/** An answer's body as an object whose keys may each be missing. */
function body<T>(result: Extract<CallResult, { ok: true }>): Partial<T> {
    return result.data && typeof result.data === "object" ? result.data as Partial<T> : {};
}

/** Whether a live payload carries the message id every message event is keyed by. */
function hasId(payload: unknown): payload is { id: number } {
    return !!payload && typeof payload === "object" && typeof (payload as { id: unknown }).id === "number";
}

/**
 * Creates the threads.
 *
 * @param dependencies What the threads reach outside themselves.
 *
 * @returns The threads.
 */
export function createThreads(dependencies: ThreadsDependencies): Threads {
    const state = reactive<ThreadsState>({
        list: { tab: "channel", channelId: null, rows: [], hasMore: false, isLoading: false, isRefreshing: false, error: null },
        thread: null,
        followError: null
    }) as ThreadsState;
    const me = dependencies.personAliasGuid.toLowerCase();
    const mask = dependencies.mask;

    // The one stopped state every resumed await checks before it touches anything.
    let isStopped = false;

    // The open thread's copies by revision, per group, so a page read before a live change does not
    // undo it; begun again with each thread opened.
    let ledger: RevisionLedger = new Map();

    // The list's copies by revision, so a page read before a live change does not undo it and a
    // repeat or an older change moves nothing back. Kept for the threads' life, as a page lands into
    // it whichever fetch asked for it.
    const listLedger: RevisionLedger = new Map();

    // Where the next page starts, held from the page as read: a row's time changes live, and a
    // cursor read off the rows would then skip or repeat the rows between.
    let listCursor: ListCursor | null = null;

    // Bumped by every fetch that replaces the list and by closing it, so an older answer is dropped.
    let listFetch = 0;

    // Whether the list is on screen, from its open to its close, which a resync reads again.
    let isListShown = false;
    let isLoadingMore = false;

    // Bumped by every thread opened or closed, so a page of a thread left is dropped.
    let threadOpen = 0;

    // The open thread's first page on its way, or landed, which opening the same thread again
    // waits on rather than asking for it twice.
    let threadLoad: Promise<void> = Promise.resolve();

    // The open thread's older page on its way: the reply it is read before and the open and read
    // it was asked in, so a second ask for the same page, the drawer's top and a landing's, ends
    // with it, and an ask after a reread, whose page that reread will drop, asks afresh.
    let olderLoad: { beforeId: number, open: number, read: number, done: Promise<void> } | null = null;

    // Each root's follow saves. The bell can be pressed again before the first answer, and the
    // answers can arrive in either order, so only the newest says how it went.
    const followRule = createSaveRule(() => isStopped);

    // The open thread's read position: the newest reply on screen, and what is stored or on its
    // way to being stored, the same pair the channel's position keeps.
    let newestSeen: number | null = null;
    let lastSaved: number | null = null;

    // The newest thread.updated for the open thread while its root is still on its way, laid over
    // the root the page brings: the page may have been read before the reply it reports.
    let pendingRootUpdate: ThreadUpdatedEvent | null = null;

    // Bumped by every reread of the open thread and by every open, close and stop, so of two
    // rereads out at once only the newest asked lands.
    let threadRead = 0;

    // Changes to the open thread's messages made while its page is on its way, run once it lands.
    let whileLoading: Array<() => void> = [];

    /** The root the conversation's timeline holds, if any. */
    function timelineRoot(channelId: string, rootId: number): TimelineMessage | undefined {
        return dependencies.timelines.state(channelId).messages.find(m => m.id === rootId);
    }

    /**
     * The root a follow change moves in a timeline: on the All tab a row's conversation is not the
     * one the list was opened from, so it is looked up where the row says it lives.
     */
    function followedRoot(rootId: number): TimelineMessage | undefined {
        const thread = state.thread;
        const rowChannelId = state.list.rows.find(r => r.root.id === rootId)?.channel_id;
        const root = timelineRoot(thread?.rootId === rootId ? thread.channelId : rowChannelId ?? state.list.channelId ?? "", rootId);
        return root && root.thread_following !== null && root.thread_following !== undefined ? root : undefined;
    }

    /**
     * Draws a thread followed or not everywhere it is held. An unfollowed row stays, marked, until
     * the list is next fetched, so the person can take it back from where it was.
     */
    function showFollowing(rootId: number, following: boolean): void {
        for (const row of state.list.rows.filter(r => r.root.id === rootId)) {
            row.following = following;
            row.root.thread_following = following;
        }

        if (state.thread?.rootId === rootId) {
            state.thread.following = following;
        }

        const root = followedRoot(rootId);
        if (root) {
            root.thread_following = following;
        }
    }

    /**
     * Whether a page that just landed should draw a root followed. A page read while a change of
     * it is out is older than what the person chose, so the choice drawn stays until the change's
     * own answer settles it.
     */
    function landedFollowing(rootId: number, read: boolean): boolean {
        if (!followRule.isOut(`${rootId}`)) {
            return read;
        }

        return state.list.rows.find(r => r.root.id === rootId)?.following ?? followedRoot(rootId)?.thread_following ?? read;
    }

    /** Drops a thread's unread marks, which no longer apply once it is unfollowed. */
    function clearUnread(rootId: number): void {
        for (const row of state.list.rows.filter(r => r.root.id === rootId)) {
            markRow(row, { isFollowing: row.following, isUnread: false });
        }

        const root = followedRoot(rootId);
        if (root) {
            root.thread_unread = false;
        }
    }

    /**
     * Whether the thread is the one open. It is where the reader already is, loading or loaded, so
     * opening it again changes nothing; only a first page that failed is asked for again.
     */
    function isOpen(channelId: string, rootId: number): boolean {
        const open = state.thread;
        return open?.channelId === channelId && open.rootId === rootId && !open.error;
    }

    /** Opens a thread at its newest replies, saving the position of any thread it replaces. */
    async function loadThread(channelId: string, rootId: number): Promise<void> {
        if (state.thread) {
            void leaveThread();
        }

        const thisOpen = ++threadOpen;
        threadRead++;
        state.thread = { channelId, rootId, root: null, replies: [], following: false, readCursor: null, hasMore: false, isLoading: true, error: null, olderError: null };
        newestSeen = null;
        lastSaved = null;
        pendingRootUpdate = null;
        whileLoading = [];
        ledger = new Map();

        // Opening the thread is reading it, so its mark goes from the conversation and the list
        // at once rather than at the next save, and stays gone from a conversation whose
        // history arrives after it, as one opened from the All tab does.
        dependencies.timelines.keepThreadRead(rootId);
        const held = timelineRoot(channelId, rootId);
        if (held?.thread_unread) {
            held.thread_unread = false;
        }
        for (const row of state.list.rows.filter(r => r.root.id === rootId)) {
            markRow(row, { isFollowing: row.following, isUnread: false });
        }

        const result = await dependencies.call("chat_get_thread_history", { p_root_id: rootId, p_limit: dependencies.limits().history_page_size });
        if (isStopped || thisOpen !== threadOpen || !state.thread) {
            return;
        }

        const thread = state.thread;
        thread.isLoading = false;
        const changes = whileLoading;
        whileLoading = [];
        if (!result.ok) {
            thread.error = result.error;
            return;
        }

        const page = body<ThreadPage>(result);
        const pageRev = revOf(page);
        const pageRoot = page.root ? layNewer(ledger, page.root, pageRev) : null;
        // counts that arrived while the page was out move its root, unless the page was read after them
        const pendingRev = revOf(pendingRootUpdate);
        if (pageRoot && pendingRootUpdate && (pendingRev === null || pageRev === null || pendingRev > pageRev)) {
            moveRoot(pageRoot, pendingRootUpdate, true);
        }
        pendingRootUpdate = null;
        // masked after the counts are laid on, so a replier they name is masked too
        thread.root = pageRoot ? holdMessage(undefined, pageRoot, mask) : null;

        // A reply that arrived live while the page was on its way may be newer than the page,
        // so what is held joins the page by id rather than being replaced by it; a change that
        // arrived above the page's revision is laid over its copy.
        const live = thread.replies;
        thread.replies = (page.messages ?? []).map(message => holdMessage(undefined, layNewer(ledger, message, pageRev), mask)).sort((a, b) => a.id - b.id);
        reportLanded(thread.replies);
        for (const reply of live) {
            insertReply(reply);
        }
        thread.following = landedFollowing(rootId, page.following === true);
        thread.readCursor = page.read_cursor ?? null;
        thread.hasMore = page.has_more === true;
        thread.error = null;
        newestSeen = thread.readCursor;
        lastSaved = thread.readCursor;
        changes.forEach(change => change());
    }

    /**
     * Reads again what the open thread holds and lays it over what is held, by id, so the reader
     * stays where they are: the newest page, then pages before it back to the oldest reply held,
     * at most the catch-up bound's worth. Each copy read is the platform's newest say about it,
     * combined as the timeline combines a page; a live reply newer than the read stays. A held
     * reply older than what was read is let go, and comes back, as it now reads, when the drawer
     * is scrolled to it. Only the newest reread asked lands, so an answer read before a later
     * change never draws over that change's.
     */
    async function rereadThread(): Promise<void> {
        if (isStopped || !state.thread || state.thread.error) {
            return;
        }

        const thisRead = ++threadRead;
        if (state.thread.isLoading) {
            // the oldest reply held is known only once the first page is in
            await threadLoad;
        }
        const open = state.thread;
        if (isStopped || thisRead !== threadRead || !open || open.error) {
            return;
        }

        const { page: pageSize, max } = dependencies.catchUp?.() ?? defaultCatchUp;
        const oldestHeld = open.replies[0]?.id ?? null;
        const read: TimelineMessage[] = [];
        let root: TimelineMessage | null = null;
        let hasMore = false;
        let beforeId: number | null = null;
        do {
            const result: CallResult = await dependencies.call("chat_get_thread_history", beforeId === null
                ? { p_root_id: open.rootId, p_limit: dependencies.limits().history_page_size }
                : { p_root_id: open.rootId, p_before_id: beforeId, p_limit: pageSize });
            if (isStopped || thisRead !== threadRead || state.thread !== open || !result.ok) {
                // a failed read keeps what is on screen, as a failed resync of the conversation does
                return;
            }

            const page = body<ThreadPage>(result);
            const pageRev = revOf(page);
            const messages = (page.messages ?? []).filter(message => beforeId === null || message.id < (beforeId as number));
            root = root ?? (page.root ? layNewer(ledger, page.root, pageRev) : null);
            read.push(...messages.map(message => withSendKey(layNewer(ledger, message, pageRev))));
            hasMore = page.has_more === true;
            beforeId = messages.length > 0 ? Math.min(...messages.map(message => message.id)) : null;
        } while (hasMore && beforeId !== null && oldestHeld !== null && beforeId > oldestHeld && read.length < max);

        // masked as the set stands once every page is in, not as each was read
        if (root && open.root) {
            open.root = holdMessage(open.root, root, mask);
        }
        read.sort((a, b) => a.id - b.id);
        const oldest = read[0]?.id ?? null;
        const held = new Map(open.replies.map(reply => [reply.id, reply]));
        const readIds = new Set(read.map(message => message.id));
        const live = open.replies.filter(reply => (oldest === null || reply.id >= oldest) && !readIds.has(reply.id));
        open.replies = [...read.map(message => holdMessage(held.get(message.id), message, mask)), ...live]
            .sort((a, b) => a.id - b.id);
        open.hasMore = hasMore;
        reportLanded(read);
    }

    /** Fetches the list, replacing its rows, or adding the next page after them. */
    async function fetchList(mode: "fresh" | "refresh" | "more"): Promise<void> {
        const list = state.list;
        if (isStopped || list.channelId === null) {
            return;
        }

        const thisFetch = mode === "more" ? listFetch : ++listFetch;
        if (mode === "fresh") {
            // the rows are gone, so no page of them can follow
            listCursor = null;
        }
        const cursor = mode === "more" ? listCursor : null;
        // A change heard before the ask was committed before the page is read, so the page shows
        // it or something newer; only what is heard while the page is on its way is laid over it.
        const heardBefore = heldRevisions(listLedger, "thread");
        if (mode === "more") {
            isLoadingMore = true;
        }
        else {
            list.isLoading = mode === "fresh";
            list.isRefreshing = mode === "refresh";
        }

        let result: CallResult;
        try {
            result = await dependencies.call("chat_list_threads", {
                p_channel_id: list.tab === "channel" ? list.channelId : null,
                // both cursor values together, as the platform's keyset reads them
                p_before_last_reply_at: cursor?.last_reply_at ?? null,
                p_before_id: cursor?.id ?? null,
                p_limit: dependencies.limits().thread_page_size
            });
        }
        finally {
            if (mode === "more") {
                isLoadingMore = false;
            }
        }

        // overtaken by a newer fetch, the panel closed, or the threads stopped
        if (isStopped || thisFetch !== listFetch) {
            return;
        }
        // a page after a cursor the list has since replaced would leave a gap or repeat rows
        if (mode === "more" && listCursor !== cursor) {
            return;
        }

        list.isLoading = false;
        list.isRefreshing = false;
        if (!result.ok) {
            // the rows on screen stay, with the failure said beside them
            list.error = result.error;
            return;
        }

        const page = body<ThreadListPage>(result);
        const rows = page.threads ?? [];
        const last = rows[rows.length - 1];
        // A fetched list follows its own last row, or none when empty, so no page asked after the
        // old rows can land under it; a next page with no rows leaves the cursor where it was.
        if (mode !== "more" || last) {
            listCursor = last ? { last_reply_at: last.last_reply_at, id: last.root.id } : null;
        }
        // Rows hold their place while their times change, so the platform can list a thread held
        // above the cursor again below it; the row held stays.
        const held = new Set(mode === "more" ? list.rows.map(rowKey) : []);
        const landed = rows.filter(row => !held.has(rowKey(row)))
            .map(row => landRow(row, revOf(row) ?? heardBefore.get(row.root.id) ?? null))
            .filter(isListed);
        list.error = null;
        list.rows = mode === "more" ? [...list.rows, ...landed] : landed;
        list.hasMore = page.has_more === true;
    }

    /**
     * Whether a row's thread is listed: only with a live reply, as the platform lists it, so a
     * page read before its last reply went and a change heard after both leave it out.
     */
    function isListed(row: ThreadListRow): boolean {
        return row.reply_count > 0;
    }

    /** A row's thread, by its conversation and root. */
    function rowKey(row: ThreadListRow): string {
        return `${row.channel_id}:${row.root.id}`;
    }

    /**
     * Takes a row as a page lists it, read at a revision: masked, its follow as the person last
     * chose it, and the counts heard since laid over it with the marks they make.
     */
    function landRow(row: ThreadListRow, readAt: number | null): ThreadListRow {
        const read = { ...row.root, reply_count: row.reply_count, last_reply_at: row.last_reply_at, participant_count: row.participant_count };
        const root = holdMessage(undefined, layNewer(listLedger, read, readAt), mask);
        const counts = countsOf(root);
        const held = { count: row.reply_count, isFollowing: landedFollowing(root.id, row.following), isUnread: row.has_unread };
        const marks = marksAfter(counts, held, isReading(row.channel_id, root.id));
        row.root = root;
        setRowCounts(row, counts);
        markRow(row, marks);
        return row;
    }

    /**
     * Saves the open thread's position if a newer reply than the stored one was on screen. Taken
     * as saved before the answer, so a second event while the save is out sends nothing more.
     */
    async function flush(): Promise<void> {
        const thread = state.thread;
        const messageId = newestSeen;
        if (isStopped || !thread || messageId === null || (lastSaved !== null && messageId <= lastSaved)) {
            return;
        }

        const previous = lastSaved;
        const thisOpen = threadOpen;
        lastSaved = messageId;

        const result = await dependencies.call("chat_mark_thread_read", { p_root_id: thread.rootId, p_message_id: messageId }, saveOptions);
        if (isStopped) {
            return;
        }
        if (!result.ok) {
            // the next event tries again, unless the person has moved on since
            if (thisOpen === threadOpen && lastSaved === messageId) {
                lastSaved = previous;
            }
            return;
        }

        if (thisOpen === threadOpen && state.thread) {
            state.thread.readCursor = body<MarkThreadReadAnswer>(result).read_cursor ?? messageId;
        }
    }

    // Every save goes out with keepalive, as the channel's does, so one made as the page closes
    // or the shell stops still arrives.
    const saveOptions = { keepalive: true };

    /** Puts the thread's saved position on its root in the conversation and on its row in the list. */
    function applySaved(channelId: string, rootId: number, answer: Partial<MarkThreadReadAnswer>): void {
        // A reply that landed as the person left keeps the thread unread. The platform answers the
        // bold by the rule its pages read, since the newest reply may be by someone they blocked.
        const isUnread = answer.thread_unread;
        if (typeof isUnread !== "boolean") {
            return;
        }

        const root = timelineRoot(channelId, rootId);
        if (root && root.thread_following === true) {
            root.thread_unread = isUnread;
        }
        for (const row of state.list.rows.filter(r => r.channel_id === channelId && r.root.id === rootId && r.following)) {
            markRow(row, { isFollowing: true, isUnread });
        }
    }

    /**
     * Lets go of the open thread and saves its position without holding the screen: the drawer
     * closes at once, and a thread opened while the save is out is tracked from its own place.
     */
    async function leaveThread(): Promise<void> {
        const thread = state.thread;
        const messageId = newestSeen;
        const saved = lastSaved;

        threadOpen++;
        threadRead++;
        state.thread = null;
        newestSeen = null;
        lastSaved = null;
        pendingRootUpdate = null;
        whileLoading = [];
        dependencies.timelines.keepThreadRead(null);

        if (isStopped || !thread || messageId === null || (saved !== null && messageId <= saved)) {
            return;
        }

        const result = await dependencies.call("chat_mark_thread_read", { p_root_id: thread.rootId, p_message_id: messageId }, saveOptions);
        if (!isStopped && result.ok) {
            applySaved(thread.channelId, thread.rootId, body<MarkThreadReadAnswer>(result));
        }
    }

    /**
     * What a thread's new counts make of one held copy's marks, the one rule for every copy: the
     * reader's own reply makes them follow, someone else's, not blocked, makes a thread they follow
     * and are not reading unread, and a thread with no live reply left is read.
     */
    function marksAfter(counts: ThreadCounts, held: ThreadMarks & { count: number }, isOpen: boolean): ThreadMarks {
        const newest = counts.repliers?.[0]?.person_alias_guid?.toLowerCase() ?? null;
        const isNewReply = counts.reply_count > held.count;

        if (isNewReply && newest === me) {
            // the platform makes whoever replies follow the thread
            return { isFollowing: true, isUnread: held.isUnread };
        }
        if (isNewReply && held.isFollowing && !isOpen && !dependencies.isBlocked(newest)) {
            // a reply by someone the reader blocked is not news, as the platform's bold reads it
            return { isFollowing: held.isFollowing, isUnread: true };
        }
        return { isFollowing: held.isFollowing, isUnread: held.isUnread && counts.reply_count > 0 };
    }

    /** A held copy's counts and faces. */
    function countsOf(root: TimelineMessage): ThreadCounts {
        return { reply_count: root.reply_count ?? 0, last_reply_at: root.last_reply_at ?? null, participant_count: root.participant_count ?? 0, repliers: root.repliers ?? null };
    }

    /** Puts a thread's counts and faces on one held copy of its root. */
    function moveCounts(root: TimelineMessage, counts: ThreadCounts): void {
        root.reply_count = counts.reply_count;
        root.last_reply_at = counts.last_reply_at;
        root.participant_count = counts.participant_count;
        root.repliers = counts.repliers;
    }

    /** Moves one held copy of a root by a thread's new counts: its counts and faces, and its marks by the one rule. */
    function moveRoot(root: TimelineMessage, counts: ThreadCounts, isOpen: boolean): void {
        const marks = marksAfter(counts, { count: root.reply_count ?? 0, isFollowing: root.thread_following === true, isUnread: root.thread_unread === true }, isOpen);
        moveCounts(root, counts);

        // a mark the root does not carry stays unset until it changes
        if (marks.isFollowing && root.thread_following !== true) {
            root.thread_following = true;
        }
        if (marks.isUnread !== (root.thread_unread === true)) {
            root.thread_unread = marks.isUnread;
        }
    }

    /** Puts a thread's counts and faces on its row and the row's root, which the panel draws between them. */
    function setRowCounts(row: ThreadListRow, counts: ThreadCounts): void {
        row.reply_count = counts.reply_count;
        row.last_reply_at = counts.last_reply_at;
        row.participant_count = counts.participant_count;
        moveCounts(row.root, counts);
    }

    /**
     * Puts the reader's marks on a row and its root, one answer for both copies. The mention badge
     * is the list's count as fetched; a thread read has no unread mention left in it.
     */
    function markRow(row: ThreadListRow, marks: ThreadMarks): void {
        row.following = marks.isFollowing;
        row.root.thread_following = marks.isFollowing;
        row.has_unread = marks.isUnread;
        row.root.thread_unread = marks.isUnread;
        if (!marks.isUnread) {
            row.mention_count = 0;
        }
    }

    /** Whether the reader is in a thread now, its drawer open, loading or loaded. */
    function isReading(channelId: string, rootId: number): boolean {
        return state.thread?.channelId === channelId && state.thread.rootId === rootId;
    }

    /**
     * Takes a thread.updated into the list's record, and changes the thread's row in its place:
     * its counts, faces and marks, never its position, which settles at the next fetch. A thread
     * with no live reply leaves the list, as the platform lists it, which moves no other row.
     */
    function moveRow(channelId: string, rootId: number, counts: ThreadCounts, rev: number | null, isOpen: boolean): void {
        // taken whether or not the row is held, for a page on its way to have laid over it
        const newer = takeChange(listLedger, rootId, "thread", rev, counts);
        if (newer === null) {
            // a repeat, or older than what the list holds
            return;
        }
        const list = state.list;
        const row = list.rows.find(r => r.channel_id === channelId && r.root.id === rootId);
        if (!row) {
            return;
        }

        const now = { ...counts, ...newer } as ThreadCounts;
        const marks = marksAfter(now, { count: row.reply_count, isFollowing: row.following, isUnread: row.has_unread }, isOpen);
        setRowCounts(row, now);
        markRow(row, marks);
        if (!isListed(row)) {
            list.rows = list.rows.filter(r => r !== row);
        }
    }

    /** Moves a root's counts and faces wherever it is held, and keeps them for a drawer still loading it. */
    function applyThreadUpdated(channelId: string, update: ThreadUpdatedEvent): void {
        const isOpen = isReading(channelId, update.root_id);
        const rev = revOf(update);
        const counts: ThreadCounts = { reply_count: update.reply_count, last_reply_at: update.last_reply_at, participant_count: update.participant_count, repliers: update.repliers };

        // The conversation's root moves only when the counts are newer than what the conversation
        // holds, which the timelines record after the threads have taken the change; the list's
        // row by the list's own record.
        const held = timelineRoot(channelId, update.root_id);
        if (held && dependencies.timelines.isNewer(update.root_id, "thread", rev)) {
            // the conversation's root is the copy whose pill a first reply brings
            const isStarting = (held.reply_count ?? 0) === 0 && update.reply_count > 0;
            moveRoot(held, update, isOpen);
            if (isStarting) {
                dependencies.motion.threadStarted(held);
            }
        }
        moveRow(channelId, update.root_id, counts, rev, isOpen);
        const isNewerHere = isOpen && takeChange(ledger, update.root_id, "thread", rev, counts) !== null;
        if (isNewerHere && state.thread?.root) {
            moveRoot(state.thread.root, update, true);
        }
        else if (isNewerHere) {
            pendingRootUpdate = update;
        }

        // a reply that arrived before this, from someone nothing held named, is named now
        if (isOpen && state.thread) {
            for (const reply of state.thread.replies.filter(r => r.sender_nick_name === undefined)) {
                nameFromRepliers(reply, update.repliers);
            }
        }
    }

    /** The sender's name and photo from the root's repliers, for a reply whose sender nothing held names. */
    function nameFromRepliers(message: TimelineMessage, repliers: Replier[] | null | undefined): void {
        const guid = message.person_alias_guid?.toLowerCase();
        const replier = guid ? repliers?.find(r => r.person_alias_guid.toLowerCase() === guid) : undefined;
        if (replier) {
            message.sender_nick_name = replier.nick_name;
            message.sender_last_name = replier.last_name;
            message.sender_avatar_url = replier.avatar_url;
        }
    }

    /** Takes a live reply of the open thread, naming its sender from what the thread holds. */
    function applyReply(created: MessageCreatedEvent): void {
        const thread = state.thread;
        const heldCopy = thread?.replies.find(r => r.id === created.id);
        if (!thread || heldCopy) {
            if (heldCopy) {
                reportLanded([heldCopy]);
            }
            return;
        }

        // the live copy names its sender; one from a platform that does not takes the name and
        // photo from the sender's newest message already held here
        const held = [...thread.replies].reverse().concat(thread.root ? [thread.root] : []);
        // a change that arrived before it, above its revision, is laid over it
        const arrived = layNewer(ledger, created as TimelineMessage, revOf(created));
        const sender = created.sender_nick_name !== undefined
            ? created
            : held.find(m => m.person_alias_guid === created.person_alias_guid && m.sender_nick_name !== undefined);
        const message = {
            ...arrived,
            sender_nick_name: sender?.sender_nick_name,
            sender_last_name: sender?.sender_last_name,
            sender_avatar_url: sender?.sender_avatar_url,
            sender_listed: sender?.sender_listed
        } as TimelineMessage;

        // A first-time replier has nothing held here, but is among the root's repliers once
        // thread.updated has told of their reply. The platform sends that after the reply itself,
        // so usually the name is filled in when thread.updated lands instead.
        if (!sender) {
            nameFromRepliers(message, thread.root?.repliers ?? pendingRootUpdate?.repliers);
        }
        insertReply(message);
        dependencies.motion.arrived(message);
    }

    /**
     * Puts a reply in place by id, keeping what is held over a copy that carries less. A copy of
     * a reply already held is still reported, since it may be the first the sender hears of.
     */
    function insertReply(arrived: TimelineMessage): void {
        // held in the case the sender and the drawer compare keys in, in the one shape every copy has
        const message = holdMessage(undefined, arrived, mask);
        const replies = state.thread?.replies;
        if (!replies) {
            return;
        }

        const index = replies.findIndex(r => r.id >= message.id);
        if (index < 0 || replies[index].id !== message.id) {
            replies.splice(index < 0 ? replies.length : index, 0, message);
        }
        reportLanded([message]);
    }

    /** Tells the sender of each reply here sent under a key. */
    function reportLanded(messages: TimelineMessage[]): void {
        for (const message of messages) {
            if (message.client_key) {
                dependencies.onLanded?.(message);
            }
        }
    }

    return {
        state,

        openList: async (channelId: string, tab: ThreadListTab = state.list.tab): Promise<void> => {
            if (isStopped) {
                return;
            }
            const list = state.list;
            isListShown = true;
            // The tab is the person's choice for the page, kept across closing the panel and changing
            // conversation. All lists the same threads from any conversation, so it refreshes in place.
            const isSameList = list.tab === tab && (tab === "all" || list.channelId === channelId);
            list.channelId = channelId;
            list.tab = tab;
            state.followError = null;
            if (!isSameList) {
                list.rows = [];
                list.hasMore = false;
            }
            await fetchList(list.rows.length > 0 ? "refresh" : "fresh");
        },

        switchTab: async (tab: ThreadListTab): Promise<void> => {
            if (isStopped) {
                return;
            }
            const list = state.list;
            list.tab = tab;
            list.rows = [];
            list.hasMore = false;
            list.error = null;
            state.followError = null;
            await fetchList("fresh");
        },

        refresh: () => fetchList("refresh"),

        loadMore: async (): Promise<void> => {
            if (!state.list.hasMore || isLoadingMore || listCursor === null) {
                return;
            }
            await fetchList("more");
        },

        closeList: (): void => {
            isListShown = false;
            listFetch++;
            state.list.isLoading = false;
            state.list.isRefreshing = false;
        },

        follow: async (rootId: number, isFollowing: boolean): Promise<boolean> => {
            if (isStopped) {
                return false;
            }
            if (state.followError?.rootId === rootId) {
                state.followError = null;
            }

            const thread = state.thread;
            const was = thread?.rootId === rootId ? thread.following : state.list.rows.find(r => r.root.id === rootId)?.following ?? !isFollowing;
            return followRule.save(`${rootId}`, {
                was,
                value: isFollowing,
                show: following => showFollowing(rootId, following),
                ask: async (): Promise<SaveOutcome> => {
                    const result = await dependencies.call("chat_follow_thread", { p_root_id: rootId, p_follow: isFollowing });
                    return result.ok ? { isSaved: true, data: result } : { isSaved: false, failure: failureWords(result.error, dependencies.isOffline?.()) };
                },
                landed: data => {
                    const answer = body<FollowThreadAnswer>(data as Extract<CallResult, { ok: true }>);
                    const following = answer.following ?? isFollowing;
                    showFollowing(rootId, following);
                    // Unread is dropped only once an unfollow has landed, so a refused one loses nothing.
                    if (!following) {
                        clearUnread(rootId);
                    }

                    const open = state.thread;
                    if (open?.rootId === rootId && typeof answer.read_cursor === "number") {
                        open.readCursor = answer.read_cursor;
                        lastSaved = Math.max(lastSaved ?? 0, answer.read_cursor);
                    }
                },
                refused: failure => {
                    state.followError = { rootId, isFollowing, failure };
                }
            });
        },

        openThread: (channelId: string, rootId: number): Promise<void> => {
            if (isStopped || isOpen(channelId, rootId)) {
                return threadLoad;
            }

            threadLoad = loadThread(channelId, rootId);
            return threadLoad;
        },

        isOpen,

        loadOlderReplies: (): Promise<void> => {
            const thread = state.thread;
            const oldest = thread?.replies[0];
            if (isStopped || !thread || !oldest || !thread.hasMore) {
                return Promise.resolve();
            }

            const thisOpen = threadOpen;
            const thisRead = threadRead;
            if (olderLoad && olderLoad.beforeId === oldest.id && olderLoad.open === thisOpen && olderLoad.read === thisRead) {
                return olderLoad.done;
            }

            const load = { beforeId: oldest.id, open: thisOpen, read: thisRead, done: Promise.resolve() };
            olderLoad = load;
            load.done = (async (): Promise<void> => {
                try {
                    const result = await dependencies.call("chat_get_thread_history", { p_root_id: thread.rootId, p_before_id: oldest.id, p_limit: dependencies.limits().history_page_size });
                    // dropped when the thread was left, another older page got there first, or the thread was
                    // read again since the ask: the reread is the newer say about what is held, so the next
                    // scroll asks again after it
                    if (isStopped || thisOpen !== threadOpen || thisRead !== threadRead || !state.thread || state.thread.replies[0]?.id !== oldest.id) {
                        return;
                    }
                    if (!result.ok) {
                        state.thread.olderError = result.error;
                        return;
                    }

                    const page = body<ThreadPage>(result);
                    state.thread.olderError = null;
                    const older = (page.messages ?? []).filter(m => m.id < oldest.id).map(message => holdMessage(undefined, message, mask)).sort((a, b) => a.id - b.id);
                    state.thread.replies.unshift(...older);
                    reportLanded(older);
                    state.thread.hasMore = page.has_more === true;
                }
                finally {
                    if (olderLoad === load) {
                        olderLoad = null;
                    }
                }
            })();
            return load.done;
        },

        seen: (messageId: number): void => {
            if (state.thread && (newestSeen === null || messageId > newestSeen)) {
                newestSeen = messageId;
            }
        },

        closeThread: () => leaveThread(),

        attach: (targets: PageEventTargets): (() => void) => {
            const onVisibility = (): void => {
                if (targets.document.visibilityState === "hidden") {
                    void flush();
                }
            };
            const onLeave = (): void => {
                void flush();
            };

            targets.document.addEventListener("visibilitychange", onVisibility);
            targets.window.addEventListener("blur", onLeave);
            targets.window.addEventListener("pagehide", onLeave);

            return () => {
                targets.document.removeEventListener("visibilitychange", onVisibility);
                targets.window.removeEventListener("blur", onLeave);
                targets.window.removeEventListener("pagehide", onLeave);
            };
        },

        applyEvent: (channelId: string, event: string, payload: unknown): void => {
            if (isStopped || !payload || typeof payload !== "object") {
                return;
            }

            if (event === "thread.updated") {
                const update = payload as ThreadUpdatedEvent;
                if (typeof update.root_id === "number") {
                    applyThreadUpdated(channelId, update);
                }
                return;
            }

            const thread = state.thread;
            if (!thread || thread.channelId !== channelId || !hasId(payload)) {
                return;
            }

            if (event === "message.created") {
                const created = payload as MessageCreatedEvent;
                if (created.parent_id === thread.rootId) {
                    applyReply(created);
                }
                return;
            }

            // an edit, a delete or a hide reaches the root and the replies the drawer holds, when newer
            // than the copy, and waits in the ledger for a page still on its way
            const fields = changedFields(event, payload);
            const newer = fields ? takeChange(ledger, payload.id, "message", revOf(payload), fields) : null;
            if (!newer) {
                return;
            }
            const held = thread.root?.id === payload.id ? thread.root : thread.replies.find(r => r.id === payload.id);
            if (held) {
                applyMessageChange(held, event, payload, newer);
                // a change in place is a write like any other: its pieces may name someone blocked
                maskHeld([held], mask);
                dependencies.motion.changed(held, event);
            }
        },

        upsertReply: (channelId: string, message: TimelineMessage): void => {
            const rootId = message.parent_id;
            if (isStopped || rootId === null || rootId === undefined) {
                return;
            }

            // The platform makes whoever replies follow the thread, so every copy of it says so
            // now rather than at the next fetch.
            const thread = state.thread?.channelId === channelId && state.thread.rootId === rootId ? state.thread : null;
            if (thread) {
                thread.following = true;
                if (thread.root) {
                    thread.root.thread_following = true;
                }
            }
            for (const row of state.list.rows.filter(r => r.channel_id === channelId && r.root.id === rootId)) {
                row.following = true;
                row.root.thread_following = true;
            }
            const held = timelineRoot(channelId, rootId);
            if (held) {
                held.thread_following = true;
            }

            if (thread) {
                insertReply(message);
            }
        },

        whenLoaded: (channelId: string, change: () => void): void => {
            const thread = state.thread;
            if (isStopped || !thread || thread.channelId !== channelId) {
                return;
            }
            if (thread.isLoading) {
                whileLoading.push(change);
                return;
            }
            change();
        },

        resync: (): void => {
            if (isStopped) {
                return;
            }
            if (isListShown) {
                void fetchList("refresh");
            }
            void rereadThread();
        },

        repair: (channelId: string): void => {
            if (!isStopped && state.thread?.channelId === channelId) {
                void rereadThread();
            }
        },

        remask: (): void => {
            const thread = state.thread;
            if (thread) {
                maskHeld(thread.root ? [thread.root, ...thread.replies] : thread.replies, mask);
            }
            maskHeld(state.list.rows.map(row => row.root), mask);
        },

        stop: (): void => {
            isStopped = true;
            listFetch++;
            threadOpen++;
            threadRead++;
            whileLoading = [];
        }
    };
}
