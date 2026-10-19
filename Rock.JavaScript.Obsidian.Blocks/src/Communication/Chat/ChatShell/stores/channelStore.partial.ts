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
// The sidebar. Only this store writes it: the rows arrive whole from the platform's sidebar
// call, and after that they change only through the person's own signals and their own saves.
//
// The person's topic carries a short signal the first time a channel gets something they have
// not read, and never again until they read it. A signal for a channel the sidebar does not
// hold, a hidden channel coming back or one they were just added to, reloads the sidebar, since
// the signal carries no name to draw a row with.
//
// A mention, unlike that signal, arrives for every message that names the person, so the row's
// @ count keeps rising while they stay behind.
//
// A direct message's row is kept whole instead: its preview, time and order follow every change
// to the room's last message, on the sender's own devices too. A channel's row only bolds.
import { reactive } from "vue";
import { isUnreadAfterSave } from "../composables/useMarkRead.partial";
import { ChannelMentionEvent, ChannelReadEvent, ChannelUnreadEvent, DmPreviewEvent, MarkReadResult, SidebarRow } from "../types.partial";

/** What the store reaches outside itself. */
export type ChannelStoreDependencies = {
    /** Fetches the sidebar again and hands it to setSidebar. */
    reloadSidebar: () => void;
};

/** The sidebar a shell holds. */
export type ChannelStore = {
    /** The rows in the platform's order. */
    rows: SidebarRow[];

    /** The channel on screen, or null. */
    activeChannelId: string | null;

    /** Replaces every row with a fresh sidebar. */
    setSidebar: (rows: SidebarRow[]) => void;

    /** Sets the channel on screen. */
    setActive: (channelId: string | null) => void;

    /**
     * An event on the person's own topic; replayed when a sidebar reload that was in flight as it
     * arrived has since landed.
     */
    applyPersonalEvent: (event: string, payload: unknown, isReplay?: boolean) => void;

    /** A save of the read position came back. */
    applyMarkRead: (channelId: string, result: MarkReadResult) => void;
};

/**
 * Creates the sidebar store.
 *
 * @param dependencies What the store reaches outside itself.
 *
 * @returns The store.
 */
export function createChannelStore(dependencies: ChannelStoreDependencies): ChannelStore {
    const store = reactive({
        rows: [] as SidebarRow[],
        activeChannelId: null as string | null
    });

    // Each row's last message as the latest sidebar answer read it. A mention replayed over that
    // answer was already counted in it when its message is at or below this, wherever the row's
    // pointer has moved since; a plain map, since nothing draws it.
    let answerPointers = new Map<string, number | null>();

    /** The row of a channel, if the sidebar holds it. */
    function find(channelId: string): SidebarRow | undefined {
        return store.rows.find(row => row.channel_id === channelId);
    }

    /** A signal that a channel has something the person has not read. */
    function applyUnread(payload: unknown): void {
        const signal = payload as Partial<Record<keyof ChannelUnreadEvent, unknown>> | null;
        if (!signal || typeof signal.channel_id !== "string") {
            return;
        }

        // The person is looking at it; their save when they leave settles it.
        if (signal.channel_id === store.activeChannelId) {
            return;
        }

        const row = find(signal.channel_id);
        if (!row) {
            dependencies.reloadSidebar();
            return;
        }

        row.is_unread = true;
        if (typeof signal.message_id === "number" && (row.last_message_id === null || signal.message_id > row.last_message_id)) {
            row.last_message_id = signal.message_id;
        }
    }

    /** A message named the person, by name or with @all; replayed, only one the answer missed counts. */
    function applyMention(payload: unknown, isReplay: boolean): void {
        const signal = payload as Partial<Record<keyof ChannelMentionEvent, unknown>> | null;
        // The sidebar counts what the conversation shows; a reply kept to its thread counts in
        // the thread. The conversation on screen is read as it arrives, so it counts nothing.
        if (!signal || typeof signal.channel_id !== "string" || signal.in_channel !== true || signal.channel_id === store.activeChannelId) {
            return;
        }

        // A room the sidebar does not hold yet comes with channel.unread, whose reload counts it.
        const row = find(signal.channel_id);
        if (!row) {
            return;
        }

        // An edit naming the person in a message they have already read sends a mention too;
        // the count is of what they have not read.
        if (typeof signal.message_id === "number" && row.read_cursor !== null && signal.message_id <= row.read_cursor) {
            return;
        }

        const answered = answerPointers.get(row.channel_id);
        if (isReplay && typeof signal.message_id === "number" && typeof answered === "number" && signal.message_id <= answered) {
            return;
        }

        // Kept apart so the app's icon, which counts mentions by name only, still reads its own.
        if (signal.all === true) {
            row.all_mention_count++;
        }
        else {
            row.mention_count++;
        }
    }

    /**
     * A direct message's last timeline message changed: a send, an edit of the newest, or a walk
     * back after a delete or hide, to an older message or to none.
     */
    function applyDmPreview(payload: unknown): void {
        const signal = payload as Partial<Record<keyof DmPreviewEvent, unknown>> | null;
        if (!signal || typeof signal.channel_id !== "string") {
            return;
        }

        const messageId = typeof signal.message_id === "number" ? signal.message_id : null;
        const row = find(signal.channel_id);
        if (!row) {
            // A hidden conversation comes back on a new message, as the sidebar's hide rule reads;
            // one emptied out of sight has nothing to draw.
            if (messageId !== null) {
                dependencies.reloadSidebar();
            }
            return;
        }

        const sender = signal.sender as Partial<Record<"nick_name" | "last_name", unknown>> | null | undefined;
        const isOwn = signal.is_own === true;
        row.last_message_id = messageId;
        row.last_message_at = messageId !== null && typeof signal.sent_at === "string" ? signal.sent_at : null;
        row.last_message_preview = messageId !== null && typeof signal.preview === "string" ? signal.preview : null;
        // the pointer only ever rests on a timeline message, never a system line
        row.last_message_type = messageId !== null ? "text" : null;
        row.last_sender_nick_name = typeof sender?.nick_name === "string" ? sender.nick_name : null;
        row.last_sender_last_name = typeof sender?.last_name === "string" ? sender.last_name : null;
        row.last_message_is_own = isOwn;

        // Worked out again from the new pointer rather than only set, so a walk-back past an unread
        // message that was deleted leaves the row read. The person's own message, or one in the
        // room on screen, is read already.
        row.is_unread = messageId !== null && !isOwn && row.channel_id !== store.activeChannelId
            && (row.read_cursor === null || messageId > row.read_cursor);

        sortRows();
    }

    /**
     * Puts the rows back in the order the platform's sidebar call sorts them by: starred first,
     * then the newest last message (none last), then the room's id. Only a direct message's time
     * moves live, so this is the one place the client reorders.
     */
    function sortRows(): void {
        const at = (row: SidebarRow): number => row.last_message_at ? Date.parse(row.last_message_at) : Number.NEGATIVE_INFINITY;
        const byId = (a: SidebarRow, b: SidebarRow): number => a.channel_id < b.channel_id ? -1 : a.channel_id > b.channel_id ? 1 : 0;
        store.rows = [...store.rows].sort((a, b) => Number(b.is_favorite) - Number(a.is_favorite) || at(b) - at(a) || byId(a, b));
    }

    /** A save of the read position, from this device or, as channel.read, from another of the person's. */
    function applyMarkRead(channelId: string, result: MarkReadResult): void {
        const row = find(channelId);
        if (!row) {
            return;
        }

        row.read_cursor = result.read_cursor;
        if (result.last_message_id !== null) {
            row.last_message_id = result.last_message_id;
        }
        // A save that comes back after the person has returned to the channel leaves it read:
        // they are looking at it, and their next save settles it.
        row.is_unread = channelId !== store.activeChannelId && isUnreadAfterSave(result);

        // A save that reached the newest message read every mention in it; one behind may not
        // have, and the next save settles it.
        if (!isUnreadAfterSave(result)) {
            row.mention_count = 0;
            row.all_mention_count = 0;
        }
    }

    /**
     * A read saved on another of the person's devices. It carries the room's pointer as a save's
     * answer does, so a row here that heard of fewer messages stays bold where the server holds it so.
     */
    function applyRead(payload: unknown): void {
        const signal = payload as Partial<Record<keyof ChannelReadEvent, unknown>> | null;
        if (!signal || typeof signal.channel_id !== "string") {
            return;
        }

        applyMarkRead(signal.channel_id, {
            read_cursor: typeof signal.read_cursor === "number" ? signal.read_cursor : null,
            last_message_id: typeof signal.last_message_id === "number" ? signal.last_message_id : null
        });
    }

    return Object.assign(store, {
        setSidebar: (rows: SidebarRow[]): void => {
            store.rows = rows;
            answerPointers = new Map(rows.map(row => [row.channel_id, row.last_message_id]));
        },

        setActive: (channelId: string | null): void => {
            store.activeChannelId = channelId;
        },

        applyPersonalEvent: (event: string, payload: unknown, isReplay?: boolean): void => {
            if (event === "channel.unread") {
                applyUnread(payload);
            }
            else if (event === "channel.mention") {
                applyMention(payload, isReplay === true);
            }
            else if (event === "dm.preview") {
                applyDmPreview(payload);
            }
            else if (event === "channel.read") {
                applyRead(payload);
            }
            else if (event === "membership.changed") {
                dependencies.reloadSidebar();
            }
        },

        applyMarkRead
    });
}

/**
 * People's names as one title: "Anna", "Anna and Ben", "Anna, Ben and Cara", or "Anna, Ben, Cara,
 * Dee and 2 more" when the platform names only the first few of a group.
 *
 * @param names The names the platform sent.
 * @param unnamed How many more people there are than names.
 *
 * @returns The title.
 */
export function joinNames(names: string[], unnamed: number): string {
    if (unnamed > 0) {
        return `${names.join(", ")} and ${unnamed} more`;
    }

    if (names.length <= 1) {
        return names[0] ?? "";
    }

    return `${names.slice(0, -1).join(", ")} and ${names[names.length - 1]}`;
}

/**
 * Whether a direct message is between the person and one other. Only a group can be named, and
 * the platform counts a room's people only when it has no name, so a named one, sent with a count
 * of 0, is a group.
 *
 * @param row The sidebar row.
 *
 * @returns True for a two-person direct message.
 */
export function isTwoPersonDirectMessage(row: SidebarRow): boolean {
    return row.channel_type === "dm" && !row.name && (row.dm_other_count ?? 1) <= 1;
}

/**
 * A room's title as the sidebar and the header draw it: its own name, or for a direct message
 * without one the people in it. The platform's count of others includes the people it names, so
 * only the rest are "more".
 *
 * @param row The sidebar row.
 *
 * @returns The title.
 */
export function roomTitle(row: SidebarRow): string {
    if (row.name) {
        return row.name;
    }

    const names = row.dm_names ?? [];
    return joinNames(names, (row.dm_other_count ?? names.length) - names.length);
}
