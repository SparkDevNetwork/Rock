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
/* eslint-disable @typescript-eslint/naming-convention */
import { reactive } from "vue";
import { CallResult } from "../platformCall.partial";
import { ChatError, PersonRow, RosterMember, RosterPage } from "../types.partial";
import { ActionNotice } from "./useMessageActions.partial";

/** The people search's pause after the last keystroke before it asks the platform. */
export const memberSearchPause = 250;

/** How many people a page holds; a shorter page is the last. */
export const memberPageSize = 50;

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

    /** Each row's removal while the door works or after it refused, by person. */
    removals: Record<string, MemberRemoval>;

    /** The person a remove confirm or the last leader's dialog asks about, or null. */
    confirming: RosterMember | null;

    /** The last leader's dialog: its search as typed, the platform's answer, and the door's refusal. */
    successorQuery: string;
    successor: MemberSearchState;
    successorRefusal: string | null;

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

/** Rock's doors that change who is in a room, and the one that says whether the reader may. */
export type MemberDoors = {
    getRights: (channelId: string) => Promise<{ canManageMembers: boolean }>;
    addMembers: (channelId: string, personAliasGuids: string[]) => Promise<MembershipAnswer>;
    removeMember: (channelId: string, personAliasGuid: string, replacementPersonAliasGuid: string | null) => Promise<MembershipAnswer>;
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
        removals: {},
        confirming: null,
        successorQuery: "",
        successor: noSearch(),
        successorRefusal: null,
        adding: { isOpen: false, query: "", people: null, isSearching: false, error: null, isAdding: false, refusal: null }
    };
}

/** Whether a person is the room's only leader in the loaded roster, which lists every leader first. */
export function isLastLeader(_state: MembersState, _personAliasGuid: string): boolean {
    return false;
}

/** Who the last leader's dialog offers to lead next. */
export function successorChoices(_state: MembersState): RosterMember[] {
    return [];
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
    const query = state.query.trim();
    if (!query) {
        return state.rows;
    }

    return searchStatus(state) === "answered" ? state.search.rows : state.rows.filter(member => isMatch(member, query));
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
    const state = reactive<MembersState>(emptyState()) as MembersState;

    let isStopped = false;

    // Each open, close and stop moves this on, so a page meant for a room since left is dropped.
    let rosterGeneration = 0;

    // Each query, close and stop moves this on, so an answer to an older query is dropped.
    let searchGeneration = 0;
    let searchTimer: ReturnType<typeof setTimeout> | null = null;

    /** One page of the room's people after a person given, for a query or none. */
    function fetchPage(channelId: string, after: RosterMember | undefined, query: string | null): Promise<CallResult> {
        return dependencies.call("chat_list_members", {
            p_channel_id: channelId,
            p_after: after?.person_alias_guid ?? null,
            p_limit: memberPageSize,
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
        state.hasMore = members.length >= memberPageSize;
    }

    /** Asks the platform for the whole room's matches to the query typed, if it is still the newest. */
    async function runSearch(): Promise<void> {
        searchTimer = null;
        const channelId = state.channelId;
        const query = state.query.trim();
        if (isStopped || !channelId || !query) {
            return;
        }

        const generation = searchGeneration;
        state.search = { query, rows: [], hasMore: false, isSearching: true, error: null };
        const result = await fetchPage(channelId, undefined, query);
        if (isStopped || generation !== searchGeneration) {
            return;
        }

        if (!result.ok) {
            state.search = { query, rows: [], hasMore: false, isSearching: false, error: result.error };
            return;
        }

        const members = membersOf(result);
        state.search = { query, rows: members, hasMore: members.length >= memberPageSize, isSearching: false, error: null };
    }

    /** Drops a pause still running, so no search is asked for a query already replaced. */
    function cancelPause(): void {
        if (searchTimer !== null) {
            clearTimeout(searchTimer);
            searchTimer = null;
        }
    }

    /** The next page of the search answered, after its last row. */
    async function loadMoreMatches(channelId: string): Promise<void> {
        const search = state.search;
        const query = search.query;
        if (!query || !search.hasMore || search.isSearching || state.isLoadingMore) {
            return;
        }

        const generation = searchGeneration;
        state.isLoadingMore = true;
        state.moreError = null;
        const result = await fetchPage(channelId, search.rows[search.rows.length - 1], query);
        if (isStopped || generation !== searchGeneration) {
            return;
        }

        state.isLoadingMore = false;
        if (!result.ok) {
            state.moreError = result.error;
            return;
        }

        const members = membersOf(result);
        state.search = { ...search, rows: [...search.rows, ...members], hasMore: members.length >= memberPageSize };
    }

    /** Empties the roster and drops anything still on its way. */
    function reset(): void {
        rosterGeneration++;
        searchGeneration++;
        cancelPause();
        Object.assign(state, emptyState());
    }

    return {
        state,

        open: async (channelId): Promise<void> => {
            if (isStopped) {
                return;
            }

            // Every open reads the roster afresh.
            reset();
            state.channelId = channelId;
            await loadFirstPage();
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
            state.hasMore = members.length >= memberPageSize;
        },

        retry: loadFirstPage,

        setQuery: (query): void => {
            if (isStopped) {
                return;
            }

            searchGeneration++;
            cancelPause();
            state.query = query;
            state.isLoadingMore = false;
            state.moreError = null;

            // A query of only spaces is the plain roster, which is already loaded.
            if (!query.trim() || !state.channelId) {
                state.search = noSearch();
                return;
            }

            searchTimer = setTimeout(() => void runSearch(), memberSearchPause);
        },

        retrySearch: async (): Promise<void> => {
            if (isStopped) {
                return;
            }

            searchGeneration++;
            cancelPause();
            await runSearch();
        },

        close: reset,

        stop: (): void => {
            isStopped = true;
            rosterGeneration++;
            searchGeneration++;
            cancelPause();
        },

        askRemove: () => undefined,
        cancelRemove: () => undefined,
        remove: async () => undefined,
        dismissRemoval: () => undefined,
        setSuccessorQuery: () => undefined,
        openAdd: () => undefined,
        closeAdd: () => undefined,
        setAddQuery: () => undefined,
        add: async () => undefined
    };
}
