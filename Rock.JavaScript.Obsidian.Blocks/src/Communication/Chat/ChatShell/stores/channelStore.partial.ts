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
//
// The person's own star, hide and notify mode show on the row the moment they are chosen. A room
// hidden from the conversation on screen stays while it is open, since the person is still in it,
// and leaves once another opens, as the platform's sidebar leaves it out from then on.
import { reactive } from "vue";
import { isUnreadAfterSave } from "../composables/useMarkRead.partial";
import { ChannelMentionEvent, ChannelReadEvent, ChannelUnreadEvent, ChatBadge, DmPreviewEvent, MarkReadResult, SidebarRow } from "../types.partial";

/** What the store reaches outside itself. */
export type ChannelStoreDependencies = {
    /** Fetches the sidebar again and hands it to setSidebar. */
    reloadSidebar: () => void;
};

/** The sidebar a shell holds. */
export type ChannelStore = {
    /** The rows in the platform's order. */
    rows: SidebarRow[];

    /**
     * Where the first sidebar stands: on its way, arrived, or failed. Until it has arrived, no rows
     * says nothing about whether the person has conversations, and a previewed room's row may be
     * all the rows hold. Once arrived it stays so through every reload after it.
     */
    loadState: SidebarLoadState;

    /** The channel on screen, or null. */
    activeChannelId: string | null;

    /** The church's chat badges, in the order the bootstrap lists them. */
    badges: ChatBadge[];

    /** The room the person is previewing without a sidebar row of its own, whose stand-in row the rows hold; or null. */
    previewChannelId: string | null;

    /**
     * How many fresh sidebars have been set. A row changed by a reread is not news the moment it
     * shows: a reconnect or a membership change reads every message that landed since at once.
     */
    sidebarReads: number;

    /**
     * Replaces every row with a fresh sidebar. A previewed room's row is kept after them, unless
     * the fresh sidebar holds the room itself, which ends the preview.
     */
    setSidebar: (rows: SidebarRow[]) => void;

    /**
     * Shows a room the person is previewing as a row after the others, or takes the row away with
     * null. A room the sidebar already holds is never previewed.
     */
    setPreview: (row: SidebarRow | null) => void;

    /** A sidebar read failed; only the first sidebar's failure is the list's to show, a later one keeps the rows. */
    setLoadFailed: () => void;

    /** Reads the first sidebar again after it failed, loading once more meanwhile. */
    retryLoad: () => void;

    /** Replaces the church's badges with the ones a fresh bootstrap listed. */
    setBadges: (badges: ChatBadge[]) => void;

    /** Sets the channel on screen. */
    setActive: (channelId: string | null) => void;

    /**
     * An event on the person's own topic; replayed when a sidebar reload that was in flight as it
     * arrived has since landed.
     */
    applyPersonalEvent: (event: string, payload: unknown, isReplay?: boolean) => void;

    /**
     * A save of the read position came back. `sentAt` is what `saveSent` answered as the save set
     * out; without it, a newer pointer the row holds is always kept.
     */
    applyMarkRead: (channelId: string, result: MarkReadResult, sentAt?: number) => void;

    /** A save of the read position is setting out; the answer to hand back with its result. */
    saveSent: () => number;

    /** The person's own star, hide or notify mode on a room, shown at once and sorted as the platform sorts. */
    applyPrefs: (channelId: string, changes: RowPrefs) => void;
};

/** Where the first sidebar stands. */
export type SidebarLoadState = "loading" | "loaded" | "failed";

/** What the person's own settings change on a row. */
export type RowPrefs = Partial<Pick<SidebarRow, "is_favorite" | "hidden" | "notify_mode">>;

/**
 * Whether the row's last message is one the reader typed. Their send never moves their read
 * position, so without this their own last message would bold the row; a post Rock made in
 * their name reads "You: " but is still news to them, as the platform's sidebar holds. The bold
 * and the sidebar's glow both read it, so they never disagree on what is news.
 *
 * @param row The row, or the parts of it this reads.
 *
 * @returns True when the reader typed the row's last message.
 */
export function isTypedByReader(row: Pick<SidebarRow, "last_message_is_own" | "last_message_posted_by_rock">): boolean {
    return row.last_message_is_own && !row.last_message_posted_by_rock;
}

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
        loadState: "loading" as SidebarLoadState,
        activeChannelId: null as string | null,
        previewChannelId: null as string | null,
        badges: [] as ChatBadge[],
        sidebarReads: 0
    });

    /**
     * A room opened from Browse channels or a link has no row in the platform's sidebar until the
     * person joins it, so a stand-in is kept here and put back after every fresh sidebar, which
     * may have been read before the room was opened.
     */
    function setPreview(row: SidebarRow | null): void {
        const previous = store.previewChannelId;
        const others = previous ? store.rows.filter(r => r.channel_id !== previous) : store.rows;
        if (!row || others.some(r => r.channel_id === row.channel_id)) {
            store.previewChannelId = null;
            store.rows = others;
            return;
        }

        store.previewChannelId = row.channel_id;
        store.rows = [...others, row];
    }

    // Each row's last message as the latest sidebar answer read it. A mention replayed over that
    // answer was already counted in it when its message is at or below this, wherever the row's
    // pointer has moved since; a plain map, since nothing draws it.
    let answerPointers = new Map<string, number | null>();

    // When each row's pointer was last moved by a signal or a fresh sidebar, as a count of those
    // moves, so a save's answer can tell a message that reached the row after the save set out
    // from one it was sent knowing of; a plain map, since nothing draws it.
    let moves = 0;
    const movedAt = new Map<string, number>();

    /** The row of a channel, if the sidebar holds it. */
    function find(channelId: string): SidebarRow | undefined {
        return store.rows.find(row => row.channel_id === channelId);
    }

    /**
     * A message newer than the hide brings a hidden room back, as the platform's sidebar reads its
     * hide, so a row kept while it was open stays once another opens.
     */
    function unhideOnNew(row: SidebarRow | undefined): void {
        if (row?.hidden) {
            row.hidden = false;
        }
    }

    /** A signal that a channel has something the person has not read. */
    function applyUnread(payload: unknown): void {
        const signal = payload as Partial<Record<keyof ChannelUnreadEvent, unknown>> | null;
        if (!signal || typeof signal.channel_id !== "string") {
            return;
        }

        const row = find(signal.channel_id);
        unhideOnNew(row);

        // The person is looking at it; their save when they leave settles it.
        if (signal.channel_id === store.activeChannelId) {
            return;
        }

        if (!row) {
            dependencies.reloadSidebar();
            return;
        }

        row.is_unread = true;
        if (typeof signal.message_id === "number" && (row.last_message_id === null || signal.message_id > row.last_message_id)) {
            row.last_message_id = signal.message_id;
            movedAt.set(row.channel_id, ++moves);
            // the signal never announces a message the reader typed
            row.last_message_is_own = false;
            row.last_message_posted_by_rock = false;
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

        // A room the sidebar does not hold yet comes with channel.unread, or for a direct message
        // with dm.preview, whose reload counts it.
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
        if (messageId !== null && messageId > (row?.last_message_id ?? 0)) {
            unhideOnNew(row);
        }

        if (!row) {
            // A hidden conversation comes back on a new message, as the sidebar's hide rule reads;
            // one emptied out of sight has nothing to draw.
            if (messageId !== null) {
                dependencies.reloadSidebar();
            }
            return;
        }

        const sender = signal.sender as Partial<Record<"nick_name" | "last_name" | "avatar_url", unknown>> | null | undefined;
        const isOwn = signal.is_own === true;
        row.last_message_id = messageId;
        movedAt.set(row.channel_id, ++moves);
        row.last_message_at = messageId !== null && typeof signal.sent_at === "string" ? signal.sent_at : null;
        row.last_message_preview = messageId !== null && typeof signal.preview === "string" ? signal.preview : null;
        // the pointer only ever rests on a timeline message, never a system line
        row.last_message_type = messageId !== null ? "text" : null;
        row.last_sender_nick_name = typeof sender?.nick_name === "string" ? sender.nick_name : null;
        row.last_sender_last_name = typeof sender?.last_name === "string" ? sender.last_name : null;
        row.last_sender_avatar_url = typeof sender?.avatar_url === "string" ? sender.avatar_url : null;
        row.last_message_is_own = isOwn;
        row.last_message_posted_by_rock = signal.posted_by_rock === true;
        // the line the platform worked out for this reader, so the row never composes its own
        row.preview_line = typeof signal.preview_line === "string" ? signal.preview_line : "";

        // Worked out again from the new pointer rather than only set, so a walk-back past an unread
        // message that was deleted leaves the row read. A message the person typed, or one in the
        // room on screen, is read already.
        row.is_unread = messageId !== null && !isTypedByReader(row) && row.channel_id !== store.activeChannelId
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
    function applyMarkRead(channelId: string, result: MarkReadResult, sentAt?: number): void {
        const row = find(channelId);
        if (!row) {
            return;
        }

        // An answer that comes back after a newer message reached the row carries the pointer as
        // it stood when the save ran, so the row keeps the newer one and the bold is worked out from
        // it. A newer pointer the row already held when the save set out is one the save ran past:
        // a deleted message walked back from, which a channel hears of no other way, so the
        // answer's is taken. Without the moment the save set out, as for a read from another
        // device, the newer is kept: a stale bold the next open clears, never a row left plain
        // over a message the platform holds unread.
        const isRowNewer = row.last_message_id !== null
            && (result.last_message_id === null || row.last_message_id > result.last_message_id);
        const isKept = isRowNewer && (sentAt === undefined || (movedAt.get(channelId) ?? 0) > sentAt);
        // A message the reader typed is read already; one the answer names instead is nobody's
        // the row knows of.
        const isTyped = (isKept || row.last_message_id === result.last_message_id) && isTypedByReader(row);
        if (!isKept && result.last_message_id !== null) {
            row.last_message_id = result.last_message_id;
        }
        row.read_cursor = result.read_cursor;
        const saved: MarkReadResult = { read_cursor: result.read_cursor, last_message_id: row.last_message_id };

        // A save that comes back after the person has returned to the channel leaves it read:
        // they are looking at it, and their next save settles it.
        row.is_unread = channelId !== store.activeChannelId && !isTyped && isUnreadAfterSave(saved);

        // A save that reached the newest message read every mention in it; one behind may not
        // have, and the next save settles it.
        if (!isUnreadAfterSave(saved)) {
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
            const preview = store.rows.find(row => row.channel_id === store.previewChannelId);
            // A room hidden from the screen it is open on is still the person's to look at, though
            // the fresh rows leave it out.
            const hidden = store.rows.find(row => row.hidden && row.channel_id === store.activeChannelId && row.channel_id !== preview?.channel_id);
            const kept = [preview, hidden].filter((row): row is SidebarRow => !!row && !rows.some(fresh => fresh.channel_id === row.channel_id));
            // the fresh rows' own copy of the room, once it has one, ends the preview
            if (!preview || !kept.includes(preview)) {
                store.previewChannelId = null;
            }
            store.rows = [...rows, ...kept];
            store.loadState = "loaded";
            store.sidebarReads++;
            answerPointers = new Map(rows.map(row => [row.channel_id, row.last_message_id]));
            const at = ++moves;
            movedAt.clear();
            rows.forEach(row => movedAt.set(row.channel_id, at));
        },

        setLoadFailed: (): void => {
            if (store.loadState !== "loaded") {
                store.loadState = "failed";
            }
        },

        retryLoad: (): void => {
            if (store.loadState === "failed") {
                store.loadState = "loading";
                dependencies.reloadSidebar();
            }
        },

        setBadges: (badges: ChatBadge[]): void => {
            store.badges = badges;
        },

        saveSent: (): number => moves,

        setActive: (channelId: string | null): void => {
            store.activeChannelId = channelId;
            // a room hidden while it was open leaves once another opens
            if (store.rows.some(row => row.hidden && row.channel_id !== channelId)) {
                store.rows = store.rows.filter(row => !row.hidden || row.channel_id === channelId);
            }
        },

        applyPrefs: (channelId: string, changes: RowPrefs): void => {
            const row = find(channelId);
            if (!row) {
                return;
            }

            Object.assign(row, changes);
            if (changes.is_favorite !== undefined) {
                sortRows();
            }
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
            // A membership, or a room's name, description or kind, changed: the signal carries
            // only the room's id, so its row is read again with the rest.
            else if (event === "membership.changed" || event === "channel.updated") {
                dependencies.reloadSidebar();
            }
        },

        setPreview,

        applyMarkRead
    });
}

/**
 * People's names as one line of text that is not a room's title, which the platform sends: who was
 * added, who leads a channel, the people a new conversation is with. "Anna", "Anna and Ben",
 * "Anna, Ben and Cara", or "Anna, Ben, Cara, Dee and 2 more".
 *
 * @param names The names.
 * @param unnamed How many more people there are than names.
 *
 * @returns The line.
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
