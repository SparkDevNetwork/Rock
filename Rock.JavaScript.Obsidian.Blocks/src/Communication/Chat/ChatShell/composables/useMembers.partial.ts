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
import { ChatError, RosterMember, RosterPage } from "../types.partial";

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
};

/** What the roster reaches outside itself. */
export type MembersDependencies = {
    call: (name: string, args: Record<string, unknown>) => Promise<CallResult>;
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
};

/** The refusals that are states of the panel rather than failures, by the platform's codes. */
const refusals: Record<string, RosterRefusal> = {
    "authz.not_member": "not_member",
    "authz.no_roster_right": "no_roster_right"
};

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
    const state = reactive<MembersState>({
        channelId: null,
        rows: [],
        hasMore: false,
        isLoading: false,
        isLoadingMore: false,
        error: null,
        moreError: null,
        refusal: null,
        query: "",
        search: { query: null, rows: [], hasMore: false, isSearching: false, error: null }
    }) as MembersState;

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
        Object.assign(state, {
            channelId: null,
            rows: [],
            hasMore: false,
            isLoading: false,
            isLoadingMore: false,
            error: null,
            moreError: null,
            refusal: null,
            query: "",
            search: { query: null, rows: [], hasMore: false, isSearching: false, error: null }
        });
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
                state.search = { query: null, rows: [], hasMore: false, isSearching: false, error: null };
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
        }
    };
}
