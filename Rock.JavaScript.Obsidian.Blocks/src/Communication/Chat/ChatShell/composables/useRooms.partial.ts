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
// Rooms: Browse channels, the facts of each room opened, Join, and the row a previewed room keeps
// in the sidebar.
//
// The directory's search waits for a pause in typing and drops an answer older than the newest
// query, so a slow answer never overwrites a newer one; a filter asks again at once. A room's
// facts are read on every open and again on that room's membership change, and only the newest
// read is kept. Join goes through Rock's door. A join Rock made that the platform has not taken
// yet is pending until a sidebar reload shows the room as a member, since that reload is the
// platform saying it holds the membership. A room opened with no sidebar row keeps a stand-in row
// in the sidebar store until another conversation opens or a reload brings its real row. Once
// stopped, nothing more is asked and no answer still on its way reaches the screen.
/* eslint-disable @typescript-eslint/naming-convention */
import { reactive } from "vue";
import { ChannelStore } from "../stores/channelStore.partial";
import { ChannelFacts, ChatError, DirectoryPage, DirectoryRow, SidebarRow } from "../types.partial";
import { ActionNotice } from "./useMessageActions.partial";
import { JoinResult } from "./useSend.partial";

/** How long the directory waits after the last keystroke before it searches, as the design sets it. */
export const directorySearchPause = 200;

/** How many rows a page of the directory asks for: the platform's own default page. */
export const directoryPageSize = 50;

/** The sentence a refusal shows when the door gave none of its own, as the door words its refusal. */
const refusedSentence = "You can't join this channel.";

/** What a Join that could not reach the door says, as the chat says any change made while offline. */
const offlineSentence = "Couldn't join. You're offline, so try again in a moment.";

/** Which campus chip is pressed: every campus, one campus, or the rooms with no campus. */
export type DirectoryCampus =
    | { kind: "all" }
    | { kind: "campus", campusId: string }
    | { kind: "wholeChurch" };

/** The segmented control under the search: every room, or only those the person is not in. */
export type DirectoryShow = "all" | "notJoined";

/**
 * Which empty state an empty answer draws: nothing public at all, nothing matching a search,
 * nothing matching a search under a campus, every room joined, or nothing at a campus.
 */
export type DirectoryEmptyKind = "none" | "query" | "filters" | "allJoined" | "campus";

/** What the directory asks the platform for, in the client's names. */
export type DirectoryRequest = {
    query: string | null;
    campusId: string | null;
    wholeChurch: boolean;
    notJoined: boolean;
    after: string | null;
    limit: number;
};

/** A directory page, or the refusal. */
export type DirectoryPageResult = { ok: true, page: DirectoryPage } | { ok: false, error: ChatError };

/** A room's facts, or the refusal. */
export type ChannelFactsResult = { ok: true, facts: ChannelFacts } | { ok: false, error: ChatError };

/** Browse channels as the view draws it. */
export type DirectoryState = {
    isOpen: boolean;

    /** What is typed in the search, untrimmed. */
    query: string;
    campus: DirectoryCampus;
    show: DirectoryShow;
    rows: DirectoryRow[];

    /** Every room the answer's query and filters match, counted past the page. */
    total: number;
    hasMore: boolean;

    /** The first page of this visit is on its way, with nothing to draw yet: the skeleton rows. */
    isLoading: boolean;

    /** A search or a filter is waiting for its pause or its answer; the rows held stay drawn. */
    isSearching: boolean;
    isLoadingMore: boolean;

    /** Why the first page did not come, or null. */
    error: ChatError | null;

    /** Why the next page did not come, or null; the rows held stay. */
    moreError: ChatError | null;

    /** The church shows each person only their campus's rooms, so a note stands in for the chips. */
    isCampusLimited: boolean;

    /** The empty state the latest answer draws, or null while it has rows or none has landed. */
    emptyKind: DirectoryEmptyKind | null;
};

/**
 * Where Join is for one room: not asked, the door on its way, joined (fresh until the next visit,
 * so the check pops in once), pending while the platform takes the membership, refused with the
 * door's own sentence, or failed without the door's answer, saying why, with Join to press again.
 */
export type JoinState =
    | { kind: "idle" }
    | { kind: "joining" }
    | { kind: "joined", isFresh: boolean }
    | { kind: "pending" }
    | { kind: "refused", text: string }
    | { kind: "failed", text: string };

/** A sentence for screen readers; the count moves on with each, so the same words twice are read twice. */
export type RoomAnnouncement = {
    text: string;
    isAssertive: boolean;
    count: number;
};

/** What the rooms hold for the screen. */
export type RoomsState = {
    directory: DirectoryState;

    /** Each room's facts by its id, as last read this session. */
    facts: Record<string, ChannelFacts>;

    /** Each room's Join, by its id; a room not here is idle. */
    joins: Record<string, JoinState>;
    announcement: RoomAnnouncement | null;
};

/** What the rooms reach outside themselves. */
export type RoomsDependencies = {
    /** Reads one room's facts, chat_get_channel through the shell's call path. */
    getChannel: (channelId: string) => Promise<ChannelFactsResult>;

    /** Reads a page of the directory, chat_list_directory through the shell's call path. */
    listDirectory: (request: DirectoryRequest) => Promise<DirectoryPageResult>;

    /** Rock's Join door; without it, Join is refused. */
    joinChannel?: (channelId: string) => Promise<JoinResult>;

    /** The sidebar, which a previewed room's stand-in row joins. */
    channels: Pick<ChannelStore, "rows" | "previewChannelId" | "setPreview">;

    /** The conversation on screen now, or null. */
    activeChannelId: () => string | null;

    /** Whether the connection is lost, when the door would not be reached. */
    isOffline: () => boolean;

    /** Whether the church shows each person only their own campus's rooms. */
    isCampusLimited: boolean;

    /** Shows a toast that is not a failure. */
    notify: (notice: ActionNotice) => void;
};

/** The rooms a shell holds. */
export type Rooms = {
    state: RoomsState;

    /** Opens Browse channels afresh: no search, no filters, the first page asked. */
    openDirectory: () => void;

    /** Closes Browse channels; an answer still on its way is dropped. */
    closeDirectory: () => void;

    /** The search changed; it is asked after the pause. */
    setQuery: (query: string) => void;

    /** Clears the search and asks again at once. */
    clearQuery: () => void;

    setCampus: (campus: DirectoryCampus) => void;

    setShow: (show: DirectoryShow) => void;

    /** The empty state's Clear filters: every campus and every room, the search kept. */
    clearFilters: () => void;

    /** The next page, as the list nears its end; nothing once it has ended. */
    loadMore: () => Promise<void>;

    /** Try again after a page failed. */
    retry: () => Promise<void>;

    /** Joins a room through Rock's door, from its directory row or its join bar. */
    join: (channelId: string) => Promise<void>;

    joinOf: (channelId: string) => JoinState;

    /** A room's facts as last read, or null before the first read lands. */
    factsOf: (channelId: string) => ChannelFacts | null;

    /** A directory row is about to open: its room shows in the sidebar at once if it has no row. */
    previewRoom: (channelId: string) => void;

    /** A conversation was opened: its facts are read beside its history. */
    channelOpened: (channelId: string) => Promise<void>;

    /** The conversation on screen changed: a preview of any other room ends. */
    conversationShown: (channelId: string | null) => void;

    /** A membership changed on the person's topic: the facts held for that room are read again. */
    membershipChanged: (channelId: string | null) => void;

    /** A sidebar reload landed: a pending join it shows as a member is joined. */
    sidebarLoaded: () => void;

    stop: () => void;
};

/**
 * A sidebar row standing in for a room the person is previewing: a room they are not in, with
 * nothing unread, which the sidebar draws after the channels with its name muted.
 *
 * @param source The room as the directory or its facts describe it.
 *
 * @returns The row.
 */
function previewRow(source: Pick<ChannelFacts, "channel_id" | "name" | "icon_url" | "is_member"> & Partial<Pick<ChannelFacts, "channel_type" | "is_public">>): SidebarRow {
    return {
        channel_id: source.channel_id,
        name: source.name,
        icon_url: source.icon_url,
        // the directory lists only public shared channels, and says no more than that
        channel_type: source.channel_type ?? "shared",
        is_public: source.is_public ?? true,
        always_shown: false,
        is_member: source.is_member,
        is_favorite: false,
        notify_mode: null,
        is_leader: false,
        can_post_announcements: false,
        can_mention_all: false,
        last_message_id: null,
        last_message_at: null,
        last_message_type: null,
        last_sender_nick_name: null,
        last_sender_last_name: null,
        last_sender_listed: false,
        last_message_preview: null,
        last_message_is_own: false,
        last_message_posted_by_rock: false,
        read_cursor: null,
        is_unread: false,
        mention_count: 0,
        all_mention_count: 0,
        can_mention: false,
        dm_names: null,
        dm_other_count: null
    };
}

/**
 * Creates the rooms.
 *
 * @param dependencies What the rooms reach outside themselves.
 *
 * @returns The rooms.
 */
export function createRooms(dependencies: RoomsDependencies): Rooms {
    const { channels } = dependencies;

    const state = reactive<RoomsState>({
        directory: {
            isOpen: false,
            query: "",
            campus: { kind: "all" },
            show: "all",
            rows: [],
            total: 0,
            hasMore: false,
            isLoading: false,
            isSearching: false,
            isLoadingMore: false,
            error: null,
            moreError: null,
            isCampusLimited: dependencies.isCampusLimited,
            emptyKind: null
        },
        facts: {},
        joins: {},
        announcement: null
    }) as RoomsState;

    let isStopped = false;

    // Each query, filter, open, close and stop moves this on, so a page asked for an older
    // directory never lands in the one on screen.
    let directoryGeneration = 0;
    let searchTimer: ReturnType<typeof setTimeout> | null = null;

    // Each room's newest facts read, so an older read answering last is dropped.
    const factsGeneration = new Map<string, number>();

    let announcements = 0;

    /** Puts a sentence in the live region. */
    function announce(text: string, isAssertive = false): void {
        state.announcement = { text, isAssertive, count: ++announcements };
    }

    // #region The directory

    /** What the directory asks for, from its filters and a starting row. */
    function request(after: string | null): DirectoryRequest {
        const directory = state.directory;
        // under the church's campus limit there are no chips, and the platform applies the limit
        const campus = directory.isCampusLimited ? { kind: "all" } as DirectoryCampus : directory.campus;
        return {
            query: directory.query.trim() || null,
            campusId: campus.kind === "campus" ? campus.campusId : null,
            wholeChurch: campus.kind === "wholeChurch",
            notJoined: directory.show === "notJoined",
            after,
            limit: directoryPageSize
        };
    }

    /** Which empty state an empty answer to a request draws. */
    function emptyKindFor(asked: DirectoryRequest): DirectoryEmptyKind {
        const hasCampus = asked.campusId !== null || asked.wholeChurch;
        if (asked.query !== null) {
            return hasCampus || asked.notJoined ? "filters" : "query";
        }
        if (hasCampus) {
            return "campus";
        }

        return asked.notJoined ? "allJoined" : "none";
    }

    /** Drops a pause still running, so no search is asked for a query already replaced. */
    function cancelPause(): void {
        if (searchTimer !== null) {
            clearTimeout(searchTimer);
            searchTimer = null;
        }
    }

    /** Asks the first page for what the directory shows now, if it is still the newest. */
    async function fetchFirst(generation: number): Promise<void> {
        searchTimer = null;
        const asked = request(null);
        const result = await dependencies.listDirectory(asked);
        if (isStopped || generation !== directoryGeneration) {
            return;
        }

        const directory = state.directory;
        directory.isLoading = false;
        directory.isSearching = false;
        if (!result.ok) {
            directory.error = result.error;
            return;
        }

        directory.error = null;
        directory.moreError = null;
        directory.rows = result.page.channels;
        directory.total = result.page.total;
        // a short page is the end; a full one may be followed by an empty one, which ends it too
        directory.hasMore = result.page.channels.length >= directoryPageSize;
        directory.emptyKind = directory.rows.length === 0 ? emptyKindFor(asked) : null;

        if (asked.query !== null) {
            const count = result.page.total;
            announce(count === 0 ? "No channels found" : `${count} ${count === 1 ? "channel" : "channels"} found`);
        }
    }

    /** Asks again at once for the directory's search and filters as they stand, rows held drawn meanwhile. */
    function askNow(): void {
        if (isStopped || !state.directory.isOpen) {
            return;
        }

        cancelPause();
        state.directory.isSearching = true;
        state.directory.isLoadingMore = false;
        void fetchFirst(++directoryGeneration);
    }

    /** Asks the page after the last row held, unless the list has ended or another ask is on its way. */
    async function fetchMore(): Promise<void> {
        const directory = state.directory;
        const last = directory.rows[directory.rows.length - 1];
        if (isStopped || !directory.isOpen || !directory.hasMore || !last
            || directory.isLoadingMore || directory.isLoading || directory.isSearching) {
            return;
        }

        directory.isLoadingMore = true;
        directory.moreError = null;
        const generation = directoryGeneration;
        const result = await dependencies.listDirectory(request(last.channel_id));
        if (isStopped || generation !== directoryGeneration) {
            return;
        }

        directory.isLoadingMore = false;
        if (!result.ok) {
            directory.moreError = result.error;
            return;
        }

        // a row already held, as one renamed between the pages, is not drawn twice
        const held = new Set(directory.rows.map(row => row.channel_id));
        directory.rows.push(...result.page.channels.filter(row => !held.has(row.channel_id)));
        directory.total = result.page.total;
        directory.hasMore = result.page.channels.length >= directoryPageSize;
    }

    // #endregion The directory

    // #region Facts and the preview

    /** Reads a room's facts, keeping the newest read, and shows a room on screen with no row as previewed. */
    async function readFacts(channelId: string): Promise<void> {
        if (isStopped) {
            return;
        }

        const generation = (factsGeneration.get(channelId) ?? 0) + 1;
        factsGeneration.set(channelId, generation);
        const result = await dependencies.getChannel(channelId);
        if (isStopped || factsGeneration.get(channelId) !== generation || !result.ok) {
            // a failed read keeps what was held, which the next open reads again
            return;
        }

        state.facts[channelId] = result.facts;

        // Only the room still on screen is previewed; one left meanwhile is not the person's to see in the sidebar.
        const held = channels.rows.find(row => row.channel_id === channelId);
        const isPreview = channels.previewChannelId === channelId;
        if (dependencies.activeChannelId() === channelId && (isPreview || (!held && !result.facts.is_member))) {
            channels.setPreview(previewRow(result.facts));
        }
    }

    // #endregion Facts and the preview

    // #region Join

    /** The room's name as anything this client holds names it, for the sentences Join says. */
    function nameOf(channelId: string): string {
        return state.facts[channelId]?.name
            ?? state.directory.rows.find(row => row.channel_id === channelId)?.name
            ?? channels.rows.find(row => row.channel_id === channelId)?.name
            ?? "this channel";
    }

    /** Whether the person is in a room as far as this client knows. */
    function isMember(channelId: string): boolean {
        const row = channels.rows.find(r => r.channel_id === channelId);
        return state.facts[channelId]?.is_member === true || (!!row && row.is_member);
    }

    /** Join did not reach the door's answer: it says so where it was pressed and aloud, and may be pressed again. */
    function failJoin(channelId: string): void {
        state.joins[channelId] = { kind: "failed", text: offlineSentence };
        announce(offlineSentence);
    }

    /** The room is joined and the platform holds it: every copy of it here says so. */
    function finishJoin(channelId: string): void {
        state.joins[channelId] = { kind: "joined", isFresh: true };

        // A joined row stays where it is under "Not joined", so it never jumps from under the pointer.
        const row = state.directory.rows.find(r => r.channel_id === channelId);
        if (row) {
            row.is_member = true;
        }
        const facts = state.facts[channelId];
        if (facts) {
            facts.is_member = true;
        }

        // The stand-in turns member, so the composer enters before the reload brings the real row.
        const preview = channels.previewChannelId === channelId ? channels.rows.find(r => r.channel_id === channelId) : undefined;
        if (preview) {
            channels.setPreview({ ...preview, is_member: true });
        }

        const name = nameOf(channelId);
        dependencies.notify({ code: "room.joined", text: `Joined ${name}`, icon: "check" });
        announce(`You joined ${name}`);
    }

    // #endregion Join

    return {
        state,

        openDirectory: (): void => {
            if (isStopped) {
                return;
            }

            // A fresh visit forgets every join that finished; a refusal stays, since the door
            // would refuse again.
            for (const [channelId, join] of Object.entries(state.joins)) {
                if (join.kind === "joined") {
                    delete state.joins[channelId];
                }
            }

            cancelPause();
            Object.assign(state.directory, {
                isOpen: true,
                query: "",
                campus: { kind: "all" },
                show: "all",
                rows: [],
                total: 0,
                hasMore: false,
                isLoading: true,
                isSearching: false,
                isLoadingMore: false,
                error: null,
                moreError: null,
                emptyKind: null
            });
            void fetchFirst(++directoryGeneration);
        },

        closeDirectory: (): void => {
            cancelPause();
            directoryGeneration++;
            Object.assign(state.directory, { isOpen: false, isLoading: false, isSearching: false, isLoadingMore: false });
        },

        setQuery: (query: string): void => {
            if (isStopped || !state.directory.isOpen) {
                return;
            }

            // Moved on as the person types, so an answer already on its way for the words before
            // is dropped even while this pause runs.
            state.directory.query = query;
            state.directory.isSearching = true;
            state.directory.isLoadingMore = false;
            cancelPause();
            const generation = ++directoryGeneration;
            searchTimer = setTimeout(() => void fetchFirst(generation), directorySearchPause);
        },

        clearQuery: (): void => {
            state.directory.query = "";
            askNow();
        },

        setCampus: (campus: DirectoryCampus): void => {
            if (state.directory.isCampusLimited) {
                return;
            }
            state.directory.campus = campus;
            askNow();
        },

        setShow: (show: DirectoryShow): void => {
            state.directory.show = show;
            askNow();
        },

        clearFilters: (): void => {
            state.directory.campus = { kind: "all" };
            state.directory.show = "all";
            askNow();
        },

        loadMore: fetchMore,

        retry: async (): Promise<void> => {
            if (isStopped || !state.directory.isOpen) {
                return;
            }

            if (state.directory.error) {
                cancelPause();
                state.directory.isLoading = state.directory.rows.length === 0;
                state.directory.error = null;
                await fetchFirst(++directoryGeneration);
            }
            else if (state.directory.moreError) {
                await fetchMore();
            }
        },

        join: async (channelId: string): Promise<void> => {
            const current = state.joins[channelId]?.kind;
            if (isStopped || current === "joining" || current === "pending" || current === "refused" || isMember(channelId)) {
                return;
            }

            if (!dependencies.joinChannel) {
                state.joins[channelId] = { kind: "refused", text: refusedSentence };
                return;
            }
            if (dependencies.isOffline()) {
                failJoin(channelId);
                return;
            }

            state.joins[channelId] = { kind: "joining" };
            let answer: JoinResult;
            try {
                answer = await dependencies.joinChannel(channelId);
            }
            catch {
                if (!isStopped) {
                    failJoin(channelId);
                }
                return;
            }

            if (isStopped) {
                return;
            }

            // Rock answered without its door's result: no sentence of its own, so the person may
            // press Join again once the connection is back.
            if (answer.code === "door.unavailable" && dependencies.isOffline()) {
                failJoin(channelId);
                return;
            }

            if (answer.code !== "ok") {
                const text = answer.message || refusedSentence;
                state.joins[channelId] = { kind: "refused", text };
                announce(text, true);
                return;
            }

            if (answer.isPending) {
                state.joins[channelId] = { kind: "pending" };
                announce(`You're in ${nameOf(channelId)}. It can take a few seconds to show up.`);
                return;
            }

            finishJoin(channelId);
            // what the room now allows, such as posting, is read again
            void readFacts(channelId);
        },

        joinOf: (channelId: string): JoinState => state.joins[channelId] ?? { kind: "idle" },

        factsOf: (channelId: string): ChannelFacts | null => state.facts[channelId] ?? null,

        previewRoom: (channelId: string): void => {
            const row = state.directory.rows.find(r => r.channel_id === channelId);
            // a room the person is in has its own row coming, and one with a row needs none
            if (isStopped || !row || row.is_member || channels.rows.some(r => r.channel_id === channelId)) {
                return;
            }

            channels.setPreview(previewRow(row));
        },

        channelOpened: (channelId: string): Promise<void> => readFacts(channelId),

        conversationShown: (channelId: string | null): void => {
            if (channels.previewChannelId !== null && channels.previewChannelId !== channelId) {
                channels.setPreview(null);
            }
        },

        membershipChanged: (channelId: string | null): void => {
            // Only a room this session has asked about is held, its first read perhaps still on
            // its way; any other is read when it opens.
            if (channelId && factsGeneration.has(channelId)) {
                void readFacts(channelId);
            }
        },

        sidebarLoaded: (): void => {
            if (isStopped) {
                return;
            }

            for (const [channelId, join] of Object.entries(state.joins)) {
                const row = channels.rows.find(r => r.channel_id === channelId);
                if (join.kind === "pending" && row && row.is_member && channels.previewChannelId !== channelId) {
                    finishJoin(channelId);
                }
            }
        },

        stop: (): void => {
            isStopped = true;
            cancelPause();
            directoryGeneration++;
        }
    };
}
