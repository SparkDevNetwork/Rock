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
// The members panel's roster: who is in a room, a page at a time in the platform's order (leaders
// first, then by name), and a search of the whole room.
//
// Nothing here is live: the roster is fetched on open, on Load more and on Try again, so a change
// made elsewhere shows at the next fetch. A query waits for a pause in typing and asks the
// platform for the whole room, while the rows already loaded that match show at once; an answer
// older than the newest query is dropped, as is a page meant for a room since closed. Once
// stopped, nothing more is fetched and no answer still on its way reaches the screen.
//
// It also changes who is in the room, through Rock's doors, which decide every change: the
// rights the panel offers are asked on each open, a remove is confirmed and then works on its row,
// and Add people searches the people the room may take. The doors answer in their own words,
// which are shown as they come.
/* eslint-disable @typescript-eslint/naming-convention */
import { reactive } from "vue";
import { CallResult } from "../platformCall.partial";
import { joinNames } from "../stores/channelStore.partial";
import { ChatError, PersonRow, RosterMember, RosterPage } from "../types.partial";
import { ActionNotice } from "./useMessageActions.partial";
import { PlatformLimits } from "./useSession.partial";

/** The people search's pause after the last keystroke before it asks the platform. */
export const memberSearchPause = 250;

/** The room's two searches: the panel's filter, and the last leader's dialog's "Find a member". */
type RoomSearchKey = "search" | "successor";

/** Why the platform will not list a room's people for the reader, each drawn as its own state. */
export type RosterRefusal = "not_member" | "no_roster_right";

/** The platform's answer for one query of the whole room. */
export type MemberSearchState = {
    /** The query the rows answer, trimmed; null when no search has been asked. */
    query: string | null;
    rows: RosterMember[];
    hasMore: boolean;
    isSearching: boolean;
    error: ChatError | null;
};

/** What the roster holds for the panel. */
export type MembersState = {
    /** The room whose roster this is, or null while the panel is closed. */
    channelId: string | null;

    /** The pages loaded so far, browsing with no query. */
    rows: RosterMember[];
    hasMore: boolean;
    isLoading: boolean;
    isLoadingMore: boolean;

    /** Why the first page failed, other than a refusal, or null. */
    error: ChatError | null;

    /** Why the last later page failed, or null. */
    moreError: ChatError | null;
    refusal: RosterRefusal | null;

    /** The filter as typed. */
    query: string;
    search: MemberSearchState;

    /** Whether Rock lets the reader add and remove people here, asked on every open; false until it answers. */
    canManageMembers: boolean;

    /** Whether the reader is a Chat Administrator, the only person who may remove a channel's leader. */
    isChatAdministrator: boolean;

    /**
     * The channel's one remaining leader by primary alias, as Rock answered on open; told only to a
     * Chat Administrator, and null where nobody leads or more than one still does.
     */
    lastLeaderPersonAliasGuid: string | null;

    /** Each row's removal while the door works or after it refused, by person. */
    removals: Record<string, MemberRemoval>;

    /** The person a remove confirm or the last leader's dialog asks about, or null. */
    confirming: RosterMember | null;

    /** The last leader's dialog: its search as typed, the platform's answer, and the door's refusal. */
    successorQuery: string;
    successor: MemberSearchState;
    successorRefusal: string | null;

    /**
     * The person asked about is the room's last leader, so the confirm is the dialog that asks who
     * leads next: Rock named them so on open, or refused their removal for want of a successor.
     */
    isSuccessorAsked: boolean;

    /** The Add people dialog. */
    adding: AddPeopleState;
};

/** A row's removal: the door at work, or its refusal, which for a person already gone leaves on dismiss. */
export type MemberRemoval =
    | { kind: "removing" }
    | { kind: "refused", text: string, isStale: boolean };

/** What the Add people dialog holds. */
export type AddPeopleState = {
    isOpen: boolean;

    /** The name as typed. */
    query: string;

    /** The people search's answer for the newest name, or null before one comes. */
    people: PersonRow[] | null;
    isSearching: boolean;
    error: ChatError | null;
    isAdding: boolean;

    /** Why the door added nobody, naming the person it would not take when it named one. */
    refusal: { text: string, personAliasGuid: string | null } | null;
};

/** What one of Rock's membership doors answers. */
export type MembershipAnswer = {
    code?: string | null;
    message?: string | null;
    isPending: boolean;

    /** The person the door would not add, when that is its refusal. */
    personAliasGuid?: string | null;
};

/** What Rock says the reader may change about a room, as its members door answers. */
export type MemberRights = {
    canManageMembers: boolean;
    canRename: boolean;

    /** The channel's one remaining leader by primary alias; null where nobody leads, more than one does, or the reader may not remove a leader. */
    lastLeaderPersonAliasGuid: string | null;
};

/** Rock's doors that change who is in a room, and the one that says whether the reader may; a page without one offers none of it. */
export type MemberDoors = {
    getRights?: (channelId: string) => Promise<MemberRights>;
    addMembers?: (channelId: string, personAliasGuids: string[]) => Promise<MembershipAnswer>;
    removeMember?: (channelId: string, personAliasGuid: string, replacementPersonAliasGuid: string | null) => Promise<MembershipAnswer>;
};

/** What the roster reaches outside itself. */
export type MembersDependencies = {
    call: (name: string, args: Record<string, unknown>) => Promise<CallResult>;

    /** Rock's doors; without them nothing is offered and nothing changes. */
    doors?: MemberDoors;

    /** Whether the connection is lost, so a door is not asked and the reason is said plainly. */
    isOffline?: () => boolean;

    /** Shows a toast that is not a failure. */
    notify?: (notice: ActionNotice) => void;

    /** A room's name as the screen shows it, for a toast. */
    titleOf?: (channelId: string) => string;

    isChatAdministrator?: boolean;

    /** Whether the people search keeps to the reader's campus, as the page's setting says. */
    campusOnly?: boolean;

    /** The platform's limits, from the latest token exchange. */
    limits: () => Pick<PlatformLimits, "member_page_size" | "people_search_size">;
};

/** The roster a shell holds. */
export type Members = {
    state: MembersState;
    open: (channelId: string) => Promise<void>;
    loadMore: () => Promise<void>;
    retry: () => Promise<void>;
    setQuery: (query: string) => void;
    retrySearch: () => Promise<void>;
    close: () => void;
    stop: () => void;

    /**
     * Who is in a room, or the room itself, changed on the person's topic: the panel holding that
     * room reads its roster and the reader's rights again, in place.
     */
    roomChanged: (channelId: string | null) => void;

    /** Opens the remove confirm, or the last leader's dialog, for a person on the roster. */
    askRemove: (personAliasGuid: string) => void;
    cancelRemove: () => void;

    /** Removes the person confirmed, naming who leads next when they are the last leader. */
    remove: (replacementPersonAliasGuid?: string | null) => Promise<void>;
    dismissRemoval: (personAliasGuid: string) => void;
    setSuccessorQuery: (query: string) => void;
    openAdd: () => void;
    closeAdd: () => void;
    setAddQuery: (query: string) => void;
    add: (people: PersonRow[]) => Promise<void>;
};

/** The refusals that are states of the panel rather than failures, by the platform's codes. */
const refusals: Record<string, RosterRefusal> = {
    "authz.not_member": "not_member",
    "authz.no_roster_right": "no_roster_right"
};

/** A search not asked. */
function noSearch(): MemberSearchState {
    return { query: null, rows: [], hasMore: false, isSearching: false, error: null };
}

/** The roster of no room, which the panel starts from and every open and close returns to. */
function emptyState(): MembersState {
    return {
        channelId: null,
        rows: [],
        hasMore: false,
        isLoading: false,
        isLoadingMore: false,
        error: null,
        moreError: null,
        refusal: null,
        query: "",
        search: noSearch(),
        canManageMembers: false,
        isChatAdministrator: false,
        lastLeaderPersonAliasGuid: null,
        removals: {},
        confirming: null,
        successorQuery: "",
        successor: noSearch(),
        successorRefusal: null,
        isSuccessorAsked: false,
        adding: noAdding()
    };
}

/** The search answered for a query: the matches the loaded rows hold until the platform's answer comes. */
function matchesFor(rows: RosterMember[], search: MemberSearchState, query: string): RosterMember[] {
    const trimmed = query.trim();
    if (!trimmed) {
        return rows;
    }

    return search.query === trimmed && !search.isSearching && !search.error ? search.rows : rows.filter(member => isMatch(member, trimmed));
}

/**
 * Who the last leader's dialog offers to lead next: the room's other members, under its search the
 * whole room's matches once the platform answers, and the loaded matches until then.
 *
 * @param state The roster.
 *
 * @returns The people to choose from.
 */
export function successorChoices(state: MembersState): RosterMember[] {
    const leaving = state.confirming?.person_alias_guid;
    return matchesFor(state.rows, state.successor, state.successorQuery).filter(member => member.person_alias_guid !== leaving);
}

/** A person's name as the panel writes it. */
function fullNameOf(person: { nick_name: string | null, last_name: string | null }): string {
    return `${person.nick_name ?? ""} ${person.last_name ?? ""}`.trim() || "Someone";
}

/** No Add people dialog. */
function noAdding(): AddPeopleState {
    return { isOpen: false, query: "", people: null, isSearching: false, error: null, isAdding: false, refusal: null };
}

/**
 * Whether a person matches a query by the platform's own rule: their nick name, their last name
 * or their whole name starts with it, in any letter case.
 *
 * @param member The person.
 * @param query The query, trimmed.
 *
 * @returns True when they match.
 */
function isMatch(member: RosterMember, query: string): boolean {
    const typed = query.toLowerCase();
    const nick = (member.nick_name ?? "").toLowerCase();
    const last = (member.last_name ?? "").toLowerCase();
    return nick.startsWith(typed) || last.startsWith(typed) || `${nick} ${last}`.startsWith(typed);
}

/**
 * What the panel lists: the loaded roster with no query; under one, the platform's answer once it
 * has come for that query, and until then the loaded rows that match.
 *
 * @param state The roster.
 *
 * @returns The rows to list.
 */
export function shownMembers(state: MembersState): RosterMember[] {
    return matchesFor(state.rows, state.search, state.query);
}

/**
 * Where the search for the query typed is.
 *
 * @param state The roster.
 *
 * @returns Searching while its answer has not come, answered or failed once it has, or null with no query.
 */
export function searchStatus(state: MembersState): "searching" | "answered" | "failed" | null {
    const query = state.query.trim();
    if (!query) {
        return null;
    }

    if (state.search.query !== query || state.search.isSearching) {
        return "searching";
    }

    return state.search.error ? "failed" : "answered";
}


/**
 * Creates the roster.
 *
 * @param dependencies What the roster reaches outside itself.
 *
 * @returns The roster.
 */
export function createMembers(dependencies: MembersDependencies): Members {
    const isChatAdministrator = dependencies.isChatAdministrator ?? false;
    const state = reactive<MembersState>({ ...emptyState(), isChatAdministrator }) as MembersState;
    const isOffline = (): boolean => dependencies.isOffline?.() ?? false;
    const notify = (notice: ActionNotice): void => dependencies.notify?.(notice);

    let isStopped = false;

    // Each open, close and stop moves this on, so a page, a right or a door's answer meant for a
    // room since left is dropped.
    let rosterGeneration = 0;

    // The panel's filter and the last leader's dialog each search the room, each with its own
    // pause; each query, close and stop moves its count on, so an answer to an older query is dropped.
    const searches: Record<RoomSearchKey, { generation: number, timer: ReturnType<typeof setTimeout> | null }> = {
        search: { generation: 0, timer: null },
        successor: { generation: 0, timer: null }
    };

    // The Add people dialog: its search's pause and count, and a count every open, close and stop
    // moves on, so an add answered after the dialog closed reaches nothing.
    let addSearchGeneration = 0;
    let addSearchTimer: ReturnType<typeof setTimeout> | null = null;
    let addGeneration = 0;

    /** How many people a page holds, as the platform sets it; a shorter page is the last. */
    function pageSize(): number {
        return dependencies.limits().member_page_size;
    }

    /** One page of the room's people after a person given, for a query or none. */
    function fetchPage(channelId: string, after: RosterMember | undefined, query: string | null): Promise<CallResult> {
        return dependencies.call("chat_list_members", {
            p_channel_id: channelId,
            p_after: after?.person_alias_guid ?? null,
            p_limit: pageSize(),
            p_query: query
        });
    }

    /** The people a successful answer holds. */
    function membersOf(result: CallResult & { ok: true }): RosterMember[] {
        return (result.data as Partial<RosterPage> | null)?.members ?? [];
    }

    /** Fetches the first page into the roster, unless the room was left meanwhile. */
    async function loadFirstPage(): Promise<void> {
        const channelId = state.channelId;
        if (isStopped || !channelId) {
            return;
        }

        const generation = rosterGeneration;
        state.isLoading = true;
        state.error = null;
        state.refusal = null;
        const result = await fetchPage(channelId, undefined, null);
        if (isStopped || generation !== rosterGeneration) {
            return;
        }

        state.isLoading = false;
        if (!result.ok) {
            const refusal = refusals[result.error.code];
            if (refusal) {
                state.refusal = refusal;
            }
            else {
                state.error = result.error;
            }
            return;
        }

        const members = membersOf(result);
        state.rows = members;
        state.hasMore = members.length >= pageSize();
    }

    /**
     * Reads the first page again in place, with no loading state, so the list and the Add people
     * row focus returns to stay drawn; a failure keeps what is shown.
     */
    async function refreshRoster(channelId: string): Promise<void> {
        const generation = rosterGeneration;
        const result = await fetchPage(channelId, undefined, null);
        if (isStopped || generation !== rosterGeneration || !result.ok) {
            return;
        }

        const members = membersOf(result);
        state.rows = members;
        state.hasMore = members.length >= pageSize();
    }

    /** Asks Rock whether the reader may change who is in the room; anything but a yes is a no. */
    async function loadRights(channelId: string): Promise<void> {
        const door = dependencies.doors?.getRights;
        if (!door) {
            return;
        }

        const generation = rosterGeneration;
        try {
            const rights = await door(channelId);
            if (!isStopped && generation === rosterGeneration) {
                state.canManageMembers = rights.canManageMembers === true;
                state.lastLeaderPersonAliasGuid = rights.lastLeaderPersonAliasGuid?.toLowerCase() ?? null;
            }
        }
        catch {
            // Intentionally ignored: with no answer nothing is offered, and the doors still decide.
        }
    }

    /** The query typed for one of the room's searches. */
    function typedQuery(key: RoomSearchKey): string {
        return key === "search" ? state.query : state.successorQuery;
    }

    /** Asks the platform for the whole room's matches to a query typed, if it is still the newest. */
    async function runSearch(key: RoomSearchKey): Promise<void> {
        const search = searches[key];
        search.timer = null;
        const channelId = state.channelId;
        const query = typedQuery(key).trim();
        if (isStopped || !channelId || !query) {
            return;
        }

        const generation = search.generation;
        state[key] = { query, rows: [], hasMore: false, isSearching: true, error: null };
        const result = await fetchPage(channelId, undefined, query);
        if (isStopped || generation !== search.generation) {
            return;
        }

        if (!result.ok) {
            state[key] = { query, rows: [], hasMore: false, isSearching: false, error: result.error };
            return;
        }

        const members = membersOf(result);
        state[key] = { query, rows: members, hasMore: members.length >= pageSize(), isSearching: false, error: null };
    }

    /** Drops a search's answer still on its way and its pause still running. */
    function cancelSearch(key: RoomSearchKey): void {
        const search = searches[key];
        search.generation++;
        if (search.timer !== null) {
            clearTimeout(search.timer);
            search.timer = null;
        }
    }

    /** Starts a search's pause for the query just typed, or empties it for a blank one. */
    function typeQuery(key: RoomSearchKey): void {
        cancelSearch(key);
        // A query of only spaces is the plain roster, which is already loaded.
        if (!typedQuery(key).trim() || !state.channelId) {
            state[key] = noSearch();
            return;
        }

        searches[key].timer = setTimeout(() => void runSearch(key), memberSearchPause);
    }

    /** The next page of the search answered, after its last row. */
    async function loadMoreMatches(channelId: string): Promise<void> {
        const search = state.search;
        const query = search.query;
        if (!query || !search.hasMore || search.isSearching || state.isLoadingMore) {
            return;
        }

        const generation = searches.search.generation;
        state.isLoadingMore = true;
        state.moreError = null;
        const result = await fetchPage(channelId, search.rows[search.rows.length - 1], query);
        if (isStopped || generation !== searches.search.generation) {
            return;
        }

        state.isLoadingMore = false;
        if (!result.ok) {
            state.moreError = result.error;
            return;
        }

        const members = membersOf(result);
        state.search = { ...search, rows: [...search.rows, ...members], hasMore: members.length >= pageSize() };
    }

    /** Drops the Add people search's answer still on its way and its pause still running. */
    function cancelAddSearch(): void {
        addSearchGeneration++;
        if (addSearchTimer !== null) {
            clearTimeout(addSearchTimer);
            addSearchTimer = null;
        }
    }

    /** Asks the people search for the name typed in Add people, naming the room so each person says whether they are in it. */
    async function runAddSearch(): Promise<void> {
        addSearchTimer = null;
        const channelId = state.channelId;
        const query = state.adding.query.trim();
        if (isStopped || !channelId || !query) {
            return;
        }

        const generation = addSearchGeneration;
        const result = await dependencies.call("chat_search_people", {
            p_query: query,
            p_limit: dependencies.limits().people_search_size,
            p_campus_only: dependencies.campusOnly ?? false,
            p_channel_id: channelId
        });
        if (isStopped || generation !== addSearchGeneration) {
            return;
        }

        state.adding.isSearching = false;
        if (!result.ok) {
            state.adding.error = result.error;
            state.adding.people = null;
            return;
        }

        state.adding.people = (result.data as { people?: PersonRow[] } | null)?.people ?? [];
    }

    /**
     * Asks one of Rock's doors. Rock answering without the door's result, or not at all, decided
     * nothing, so it is null: unreached, which the caller words as offline or failed.
     */
    async function askDoor(ask: () => Promise<MembershipAnswer>): Promise<MembershipAnswer | null> {
        try {
            const answer = await ask();
            return answer.code === "door.unavailable" ? null : answer;
        }
        catch {
            return null;
        }
    }

    /** Takes a person off the roster and any search showing them, as the door has taken them out of the room. */
    function dropRow(personAliasGuid: string): void {
        const isOther = (member: RosterMember): boolean => member.person_alias_guid !== personAliasGuid;
        state.rows = state.rows.filter(isOther);
        state.search = { ...state.search, rows: state.search.rows.filter(isOther) };
        delete state.removals[personAliasGuid];
    }

    /** Empties the roster and drops anything still on its way. */
    function reset(): void {
        rosterGeneration++;
        addGeneration++;
        cancelSearch("search");
        cancelSearch("successor");
        cancelAddSearch();
        Object.assign(state, emptyState(), { isChatAdministrator });
    }

    return {
        state,

        open: async (channelId): Promise<void> => {
            if (isStopped) {
                return;
            }

            // Every open reads the roster and the reader's rights afresh, side by side.
            reset();
            state.channelId = channelId;
            await Promise.all([loadFirstPage(), loadRights(channelId)]);
        },

        loadMore: async (): Promise<void> => {
            const channelId = state.channelId;
            if (isStopped || !channelId) {
                return;
            }

            if (state.query.trim()) {
                await loadMoreMatches(channelId);
                return;
            }

            if (!state.hasMore || state.isLoadingMore || state.isLoading) {
                return;
            }

            const generation = rosterGeneration;
            state.isLoadingMore = true;
            state.moreError = null;
            const result = await fetchPage(channelId, state.rows[state.rows.length - 1], null);
            if (isStopped || generation !== rosterGeneration) {
                return;
            }

            state.isLoadingMore = false;
            if (!result.ok) {
                state.moreError = result.error;
                return;
            }

            // A person who joined between pages can shift the keyset by a row; never list them twice.
            const held = new Set(state.rows.map(member => member.person_alias_guid));
            const members = membersOf(result);
            state.rows = [...state.rows, ...members.filter(member => !held.has(member.person_alias_guid))];
            state.hasMore = members.length >= pageSize();
        },

        retry: loadFirstPage,

        setQuery: (query): void => {
            if (isStopped) {
                return;
            }

            state.query = query;
            state.isLoadingMore = false;
            state.moreError = null;
            typeQuery("search");
        },

        retrySearch: async (): Promise<void> => {
            if (isStopped) {
                return;
            }

            cancelSearch("search");
            await runSearch("search");
        },

        close: reset,

        roomChanged: (channelId): void => {
            if (isStopped || !channelId || channelId !== state.channelId) {
                return;
            }

            // Read as after the panel's own add or remove: whoever came or went may have been a
            // leader, and the reader's own role may be what changed.
            void Promise.all([refreshRoster(channelId), loadRights(channelId)]);
        },

        stop: (): void => {
            isStopped = true;
            rosterGeneration++;
            addGeneration++;
            cancelSearch("search");
            cancelSearch("successor");
            cancelAddSearch();
        },

        askRemove: (personAliasGuid): void => {
            const member = [...state.rows, ...state.search.rows].find(m => m.person_alias_guid === personAliasGuid);
            if (isStopped || !member) {
                return;
            }

            cancelSearch("successor");
            state.confirming = member;
            state.successorQuery = "";
            state.successor = noSearch();
            state.successorRefusal = null;
            state.isSuccessorAsked = member.person_alias_guid.toLowerCase() === state.lastLeaderPersonAliasGuid;
        },

        cancelRemove: (): void => {
            cancelSearch("successor");
            state.confirming = null;
            state.successorQuery = "";
            state.successor = noSearch();
            state.successorRefusal = null;
            state.isSuccessorAsked = false;
        },

        remove: async (replacementPersonAliasGuid): Promise<void> => {
            const target = state.confirming;
            const channelId = state.channelId;
            if (isStopped || !target || !channelId) {
                return;
            }

            const personAliasGuid = target.person_alias_guid;
            if (state.removals[personAliasGuid]?.kind === "removing") {
                return;
            }

            // The plain confirm gives way to the row at once; the last leader's dialog stays open
            // until the door answers, since its refusal is said there.
            const replacement = replacementPersonAliasGuid ?? null;
            if (!replacement) {
                state.confirming = null;
            }
            state.successorRefusal = null;

            const firstName = target.nick_name || fullNameOf(target);
            const unreachedText = (): string => isOffline()
                ? `Couldn't remove ${firstName}. You're offline; try again when you're back.`
                : `Couldn't remove ${firstName}. Try again in a moment.`;
            const refuse = (text: string, isStale = false): void => {
                const isAskedInDialog = !!replacement && state.confirming?.person_alias_guid === personAliasGuid;
                if (isAskedInDialog) {
                    delete state.removals[personAliasGuid];
                    state.successorRefusal = text;
                }
                else {
                    state.removals[personAliasGuid] = { kind: "refused", text, isStale };
                }
            };

            const door = dependencies.doors?.removeMember;
            if (!door || isOffline()) {
                refuse(unreachedText());
                return;
            }

            const generation = rosterGeneration;
            state.removals[personAliasGuid] = { kind: "removing" };
            const answer = await askDoor(() => door(channelId, personAliasGuid, replacement));
            if (isStopped || generation !== rosterGeneration) {
                return;
            }

            if (!answer) {
                refuse(unreachedText());
                return;
            }

            // A leader removed with no one named, whom Rock did not name as the last on open, is
            // the last to Rock now: the confirm comes back as the dialog that asks who leads next.
            if (answer.code === "door.bad_request" && !replacement && target.is_leader) {
                delete state.removals[personAliasGuid];
                cancelSearch("successor");
                state.confirming = target;
                state.successorQuery = "";
                state.successor = noSearch();
                state.isSuccessorAsked = true;
                return;
            }

            if (answer.code !== "ok") {
                // The person was gone before the door looked: the row is stale and leaves once read.
                refuse(answer.message || `Couldn't remove ${firstName}. Try again in a moment.`, answer.code === "door.not_member");
                return;
            }

            // Pending is done as well: Rock has made the change, and the platform takes it in a moment.
            if (state.confirming?.person_alias_guid === personAliasGuid) {
                state.confirming = null;
                state.isSuccessorAsked = false;
            }
            dropRow(personAliasGuid);
            if (!replacement) {
                notify({ code: "member.removed", text: `Removed ${fullNameOf(target)}`, icon: "user-minus" });
                return;
            }

            const successor = [...state.rows, ...state.successor.rows].find(m => m.person_alias_guid === replacement);
            state.rows = state.rows.map(m => m.person_alias_guid === replacement ? { ...m, is_leader: true } : m);
            const title = dependencies.titleOf?.(channelId) ?? "";
            notify({ code: "member.leader", text: `${successor ? fullNameOf(successor) : "Someone"} is now the leader of ${title}`, icon: "users" });

            // Marked at once, then read again, since Rock may have given the role to more rows than
            // the one shown, and who leads last has changed.
            await Promise.all([refreshRoster(channelId), loadRights(channelId)]);
        },

        dismissRemoval: (personAliasGuid): void => {
            const removal = state.removals[personAliasGuid];
            if (removal?.kind === "refused" && removal.isStale) {
                dropRow(personAliasGuid);
            }
            else if (removal?.kind === "refused") {
                delete state.removals[personAliasGuid];
            }
        },

        setSuccessorQuery: (query): void => {
            if (isStopped) {
                return;
            }

            state.successorQuery = query;
            typeQuery("successor");
        },

        openAdd: (): void => {
            if (isStopped || !state.channelId) {
                return;
            }

            addGeneration++;
            cancelAddSearch();
            state.adding = { ...noAdding(), isOpen: true };
        },

        closeAdd: (): void => {
            addGeneration++;
            cancelAddSearch();
            state.adding = noAdding();
        },

        setAddQuery: (query): void => {
            if (isStopped) {
                return;
            }

            cancelAddSearch();
            state.adding.query = query;
            state.adding.error = null;
            if (!query.trim() || !state.channelId) {
                state.adding.people = null;
                state.adding.isSearching = false;
                return;
            }

            state.adding.isSearching = true;
            addSearchTimer = setTimeout(() => void runAddSearch(), memberSearchPause);
        },

        add: async (people): Promise<void> => {
            const channelId = state.channelId;
            if (isStopped || !channelId || state.adding.isAdding || people.length === 0) {
                return;
            }

            const unreachedText = (): string => isOffline()
                ? "Couldn't add people. You're offline; try again when you're back."
                : "Couldn't add people. Try again in a moment.";
            state.adding.refusal = null;
            const door = dependencies.doors?.addMembers;
            if (!door || isOffline()) {
                state.adding.refusal = { text: unreachedText(), personAliasGuid: null };
                return;
            }

            const generation = addGeneration;
            state.adding.isAdding = true;
            const answer = await askDoor(() => door(channelId, people.map(person => person.person_alias_guid)));
            if (isStopped || generation !== addGeneration) {
                return;
            }

            state.adding.isAdding = false;
            if (!answer) {
                state.adding.refusal = { text: unreachedText(), personAliasGuid: null };
                return;
            }

            if (answer.code !== "ok") {
                // A refusal naming one of the people chosen says who to take out, so the rest can go.
                const named = answer.personAliasGuid?.toLowerCase();
                const refused = named ? people.find(person => person.person_alias_guid.toLowerCase() === named) : undefined;
                const text = answer.message || unreachedText();
                state.adding.refusal = refused
                    ? { text: `${text} Remove ${refused.nick_name || fullNameOf(refused)} and try again.`, personAliasGuid: refused.person_alias_guid }
                    : { text, personAliasGuid: null };
                return;
            }

            state.adding = noAdding();
            const text = people.length === 1 ? `Added ${fullNameOf(people[0])}` : `Added ${joinNames(people.map(person => person.nick_name || fullNameOf(person)), 0)}`;
            notify({ code: "member.added", text, icon: "user-plus" });
            await refreshRoster(channelId);
        }
    };
}
