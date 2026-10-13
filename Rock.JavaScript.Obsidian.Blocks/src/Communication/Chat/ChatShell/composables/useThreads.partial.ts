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
// The list has no live signal. It is fetched when the panel opens, when the tab changes, when the
// person presses refresh and when it is scrolled to its end, and at no other time, so a busy room
// costs the platform nothing while the panel sits open. Every fetch is numbered, and an answer a
// newer fetch overtook is dropped. Once the threads are stopped nothing is fetched and no answer
// still on its way reaches the screen.
//
// The thread's read position is saved as the channel's is, on events only: closing the thread,
// the window losing focus, the page going to the background or closing. What is saved is the
// newest reply that was on screen, and nothing is sent when nothing newer than the stored
// position was seen. A root's counts and faces follow thread.updated wherever the root is held.
/* eslint-disable @typescript-eslint/naming-convention */
import { reactive } from "vue";
import { CallResult } from "../platformCall.partial";
import {
    ChatError,
    FollowThreadAnswer,
    MarkThreadReadAnswer,
    MessageCreatedEvent,
    ThreadListPage,
    ThreadListRow,
    ThreadPage,
    ThreadUpdatedEvent,
    TimelineMessage
} from "../types.partial";
import { applyMessageChange, firstPageSize, Timelines } from "./useHistory.partial";
import { PageEventTargets } from "./useMarkRead.partial";

/** How many rows a page of the threads list holds. */
export const threadListPageSize = 30;

/** Which threads the list shows: the open conversation's, or every thread the person follows. */
export type ThreadListTab = "channel" | "all";

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

    /** The row whose follow change is on its way. */
    busyRootId: number | null;

    /** The row whose follow change was refused, and why. */
    rowError: { rootId: number, error: ChatError } | null;
};

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
    error: ChatError | null;
};

/** What the threads hold for the screen. */
export type ThreadsState = {
    list: ThreadListState;
    thread: OpenThreadState | null;
};

/** What the threads reach outside themselves. */
export type ThreadsDependencies = {
    /** The shell's call path; keepalive for a save that must survive the page closing. */
    call: (name: string, args: Record<string, unknown>, options?: { keepalive?: boolean }) => Promise<CallResult>;

    /** The channel timelines, whose roots a thread's changes move. */
    timelines: Pick<Timelines, "state">;

    /** The reader, so their own reply does not mark a thread unread. */
    personAliasGuid: string;
};

/** The threads a shell holds. */
export type Threads = {
    state: ThreadsState;

    /** Opens the list for a conversation, on a tab, the conversation's own by default. */
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

    /** Opens a thread at its newest replies, saving the position of any thread it replaces. */
    openThread: (channelId: string, rootId: number) => Promise<void>;

    /** Fetches the replies before the oldest held. */
    loadOlderReplies: () => Promise<void>;

    /** A reply of the open thread was on screen. */
    seen: (messageId: number) => void;

    /** Closes the thread and saves its position if a newer reply was seen. */
    closeThread: () => Promise<void>;

    /** Saves on background, lost focus and close. Returns what removes the listeners. */
    attach: (targets: PageEventTargets) => () => void;

    /** A live event on a channel topic. */
    applyEvent: (channelId: string, event: string, payload: unknown) => void;

    /** The person's own reply was confirmed: it goes into the open thread at once. */
    upsertReply: (channelId: string, message: TimelineMessage) => void;

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
 * The toast a follow change shows once the platform took it.
 *
 * @param isFollowing Whether the person now follows the thread.
 *
 * @returns The toast.
 */
export function followNotice(isFollowing: boolean): FollowNotice {
    return isFollowing
        ? { code: "thread-followed", text: "Following this thread", icon: "bell" }
        : { code: "thread-unfollowed", text: "You won't be notified about new replies", icon: "bell-off" };
}

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
        list: { tab: "channel", channelId: null, rows: [], hasMore: false, isLoading: false, isRefreshing: false, error: null, busyRootId: null, rowError: null },
        thread: null
    }) as ThreadsState;
    const me = dependencies.personAliasGuid.toLowerCase();

    // The one stopped state every resumed await checks before it touches anything.
    let isStopped = false;

    // Bumped by every fetch that replaces the list and by closing it, so an older answer is dropped.
    let listFetch = 0;
    let isLoadingMore = false;

    // Bumped by every thread opened or closed, so a page of a thread left is dropped.
    let threadOpen = 0;

    // The open thread's read position: the newest reply on screen, and what is stored or on its
    // way to being stored, the same pair the channel's position keeps.
    let newestSeen: number | null = null;
    let lastSaved: number | null = null;

    /** The root the conversation's timeline holds, if any. */
    function timelineRoot(channelId: string, rootId: number): TimelineMessage | undefined {
        return dependencies.timelines.state(channelId).messages.find(m => m.id === rootId);
    }

    /** Fetches the list, replacing its rows, or adding the next page after them. */
    async function fetchList(mode: "fresh" | "refresh" | "more"): Promise<void> {
        const list = state.list;
        if (isStopped || list.channelId === null) {
            return;
        }

        const thisFetch = mode === "more" ? listFetch : ++listFetch;
        const last = mode === "more" ? list.rows[list.rows.length - 1] : undefined;
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
                p_before_last_reply_at: last?.last_reply_at ?? null,
                p_before_id: last?.root.id ?? null,
                p_limit: threadListPageSize
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
        if (mode === "more" && list.rows[list.rows.length - 1] !== last) {
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
        list.error = null;
        list.rows = mode === "more" ? [...list.rows, ...rows] : rows;
        list.hasMore = page.has_more === true;
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

    /** Puts the thread's saved position on its root in the conversation. */
    function applySaved(channelId: string, rootId: number, answer: Partial<MarkThreadReadAnswer>): void {
        const root = timelineRoot(channelId, rootId);
        if (root && root.thread_following === true && typeof answer.read_cursor === "number" && typeof answer.last_reply_id === "number") {
            // a reply that landed as the person left keeps the root unread
            root.thread_unread = answer.last_reply_id > answer.read_cursor;
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
        state.thread = null;
        newestSeen = null;
        lastSaved = null;

        if (isStopped || !thread || messageId === null || (saved !== null && messageId <= saved)) {
            return;
        }

        const result = await dependencies.call("chat_mark_thread_read", { p_root_id: thread.rootId, p_message_id: messageId }, saveOptions);
        if (!isStopped && result.ok) {
            applySaved(thread.channelId, thread.rootId, body<MarkThreadReadAnswer>(result));
        }
    }

    /** Moves a root's counts and faces, and marks it unread when someone else replied. */
    function applyThreadUpdated(channelId: string, update: ThreadUpdatedEvent): void {
        const newest = update.repliers?.[0]?.person_alias_guid?.toLowerCase() ?? null;
        const isOpen = state.thread?.channelId === channelId && state.thread.rootId === update.root_id;

        const move = (root: TimelineMessage): void => {
            const isNewReply = update.reply_count > (root.reply_count ?? 0);
            root.reply_count = update.reply_count;
            root.last_reply_at = update.last_reply_at;
            root.participant_count = update.participant_count;
            root.repliers = update.repliers;

            if (isNewReply && newest === me) {
                // the platform makes whoever replies follow the thread
                root.thread_following = true;
            }
            else if (isNewReply && root.thread_following === true && !isOpen) {
                root.thread_unread = true;
            }
            if (update.reply_count === 0 && root.thread_unread === true) {
                root.thread_unread = false;
            }
        };

        const held = timelineRoot(channelId, update.root_id);
        if (held) {
            move(held);
        }
        if (isOpen && state.thread?.root) {
            move(state.thread.root);
        }
    }

    /** Takes a live reply of the open thread, naming its sender from what the thread holds. */
    function applyReply(created: MessageCreatedEvent): void {
        const thread = state.thread;
        if (!thread || thread.replies.some(r => r.id === created.id)) {
            return;
        }

        // the live copy carries only the message row, so the name and photo come from the
        // sender's newest message already held here
        const held = [...thread.replies].reverse().concat(thread.root ? [thread.root] : []);
        const sender = held.find(m => m.person_alias_guid === created.person_alias_guid && m.sender_nick_name !== undefined);
        insertReply({
            ...created,
            sender_nick_name: sender?.sender_nick_name,
            sender_last_name: sender?.sender_last_name,
            sender_avatar_url: sender?.sender_avatar_url,
            sender_listed: sender?.sender_listed
        } as TimelineMessage);
    }

    /** Puts a reply in place by id, keeping what is held over a copy that carries less. */
    function insertReply(message: TimelineMessage): void {
        const replies = state.thread?.replies;
        if (!replies) {
            return;
        }

        const index = replies.findIndex(r => r.id >= message.id);
        if (index >= 0 && replies[index].id === message.id) {
            return;
        }
        replies.splice(index < 0 ? replies.length : index, 0, message);
    }

    return {
        state,

        openList: async (channelId: string, tab: ThreadListTab = "channel"): Promise<void> => {
            if (isStopped) {
                return;
            }
            const list = state.list;
            const isSameList = list.channelId === channelId && list.tab === tab;
            list.channelId = channelId;
            list.tab = tab;
            list.busyRootId = null;
            list.rowError = null;
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
            list.rowError = null;
            await fetchList("fresh");
        },

        refresh: () => fetchList("refresh"),

        loadMore: async (): Promise<void> => {
            if (!state.list.hasMore || isLoadingMore || state.list.rows.length === 0) {
                return;
            }
            await fetchList("more");
        },

        closeList: (): void => {
            listFetch++;
            state.list.isLoading = false;
            state.list.isRefreshing = false;
            state.list.busyRootId = null;
            state.list.rowError = null;
        },

        follow: async (rootId: number, isFollowing: boolean): Promise<boolean> => {
            if (isStopped) {
                return false;
            }
            const list = state.list;
            list.busyRootId = rootId;
            list.rowError = null;

            const result = await dependencies.call("chat_follow_thread", { p_root_id: rootId, p_follow: isFollowing });
            if (isStopped) {
                return false;
            }
            if (list.busyRootId === rootId) {
                list.busyRootId = null;
            }
            if (!result.ok) {
                list.rowError = { rootId, error: result.error };
                return false;
            }

            const answer = body<FollowThreadAnswer>(result);
            const following = answer.following ?? isFollowing;

            // An unfollowed row stays, marked, until the list is next fetched, so the person can
            // take it back from where it was.
            for (const row of list.rows.filter(r => r.root.id === rootId)) {
                row.following = following;
                row.root.thread_following = following;
                if (!following) {
                    row.has_unread = false;
                }
            }

            const thread = state.thread;
            if (thread?.rootId === rootId) {
                thread.following = following;
                if (typeof answer.read_cursor === "number") {
                    thread.readCursor = answer.read_cursor;
                    lastSaved = Math.max(lastSaved ?? 0, answer.read_cursor);
                }
            }

            const root = timelineRoot(thread?.rootId === rootId ? thread.channelId : list.channelId ?? "", rootId);
            if (root && root.thread_following !== null && root.thread_following !== undefined) {
                root.thread_following = following;
                if (!following) {
                    root.thread_unread = false;
                }
            }

            return true;
        },

        openThread: async (channelId: string, rootId: number): Promise<void> => {
            if (isStopped) {
                return;
            }
            if (state.thread) {
                void leaveThread();
            }

            const thisOpen = ++threadOpen;
            state.thread = { channelId, rootId, root: null, replies: [], following: false, readCursor: null, hasMore: false, isLoading: true, error: null };
            newestSeen = null;
            lastSaved = null;

            // Opening the thread is reading it, so its mark goes from the conversation and the list
            // at once rather than at the next save.
            const held = timelineRoot(channelId, rootId);
            if (held?.thread_unread) {
                held.thread_unread = false;
            }
            for (const row of state.list.rows.filter(r => r.root.id === rootId)) {
                row.has_unread = false;
            }

            const result = await dependencies.call("chat_get_thread_history", { p_root_id: rootId, p_limit: firstPageSize });
            if (isStopped || thisOpen !== threadOpen || !state.thread) {
                return;
            }

            const thread = state.thread;
            thread.isLoading = false;
            if (!result.ok) {
                thread.error = result.error;
                return;
            }

            const page = body<ThreadPage>(result);
            thread.root = page.root ?? null;
            thread.replies = [...(page.messages ?? [])].sort((a, b) => a.id - b.id);
            thread.following = page.following === true;
            thread.readCursor = page.read_cursor ?? null;
            thread.hasMore = page.has_more === true;
            thread.error = null;
            newestSeen = thread.readCursor;
            lastSaved = thread.readCursor;
        },

        loadOlderReplies: async (): Promise<void> => {
            const thread = state.thread;
            const oldest = thread?.replies[0];
            if (isStopped || !thread || !oldest || !thread.hasMore) {
                return;
            }

            const thisOpen = threadOpen;
            const result = await dependencies.call("chat_get_thread_history", { p_root_id: thread.rootId, p_before_id: oldest.id, p_limit: firstPageSize });
            // dropped when the thread was left, or another older page got there first
            if (isStopped || thisOpen !== threadOpen || !state.thread || state.thread.replies[0]?.id !== oldest.id) {
                return;
            }
            if (!result.ok) {
                state.thread.error = result.error;
                return;
            }

            const page = body<ThreadPage>(result);
            const older = (page.messages ?? []).filter(m => m.id < oldest.id).sort((a, b) => a.id - b.id);
            state.thread.replies.unshift(...older);
            state.thread.hasMore = page.has_more === true;
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

            // an edit, a delete or a hide reaches the root and the replies the drawer holds
            const held = thread.root?.id === payload.id ? thread.root : thread.replies.find(r => r.id === payload.id);
            if (held) {
                applyMessageChange(held, event, payload);
            }
        },

        upsertReply: (channelId: string, message: TimelineMessage): void => {
            if (!isStopped && state.thread?.channelId === channelId && state.thread.rootId === message.parent_id) {
                insertReply(message);
            }
        },

        stop: (): void => {
            isStopped = true;
            listFetch++;
            threadOpen++;
        }
    };
}
