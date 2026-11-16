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

// Search while typing, the one way every search box does it: one pause after the last keystroke,
// one ask, the ask a newer query replaces cancelled, and only the newest answer kept however late
// an older one lands. A box keeps what its answers mean; this keeps when to ask and which answer counts.

import { getCurrentScope, onScopeDispose, reactive } from "vue";

/**
 * How long every search box waits after the last keystroke before it asks: about the gap between
 * keystrokes of steady typing, so a typed name asks once. One pause for every box, so they all
 * feel the same.
 */
export const searchPause = 200;

/** How a box searches. */
export type LatestSearchOptions<T> = {
    /** The wait after the last keystroke; the shared pause unless a box has a reason. */
    pause?: number;

    /** Whether a query is a search at all; one that is not asks nothing and clears the last answer. */
    isSearchable?: (query: string) => boolean;

    /** Told of each answer kept, with the query it answers, for a box that holds its answers in its own state. */
    onAnswer?: (answer: T, query: string) => void;
};

/** A box's search: what it holds now, and what changes it. */
export type LatestSearch<T> = {
    /** The query the answer held answers, or the one waited on. */
    query: string;

    /** The newest answer, or null before one or after a query that is no search. */
    results: T | null;

    /** Whether an answer is being waited for, from the keystroke on. */
    busy: boolean;

    /** Why the newest ask failed, or null. */
    error: unknown;

    /** Searches for a query after the pause, or at once when told to. */
    set: (query: string, options?: { isAtOnce?: boolean }) => void;

    /** Drops the pause and the ask out; the answer held stays. */
    cancel: () => void;

    /** Ends the search for good, as when its part goes. */
    stop: () => void;
};

/**
 * A search box's search. Made in a component's setup or another scope, it stops with it.
 *
 * @param fetch Asks for a query's answer; the signal is aborted when a newer query replaces it.
 * @param options How the box searches.
 *
 * @returns The search.
 */
export function useLatestSearch<T>(fetch: (query: string, signal: AbortSignal) => Promise<T>, options: LatestSearchOptions<T> = {}): LatestSearch<T> {
    const pause = options.pause ?? searchPause;

    let timer: ReturnType<typeof setTimeout> | null = null;
    let controller: AbortController | null = null;

    // Every ask is numbered; an answer to any but the newest is dropped.
    let asks = 0;
    let isStopped = false;

    async function ask(query: string): Promise<void> {
        const number = ++asks;
        controller = new AbortController();
        try {
            const answer = await fetch(query, controller.signal);
            if (isStopped || number !== asks) {
                return;
            }

            controller = null;
            search.results = answer;
            search.error = null;
            search.busy = false;
            options.onAnswer?.(answer, query);
        }
        catch (error) {
            // A cancelled ask is stale from that moment, its failure most of all.
            if (isStopped || number !== asks) {
                return;
            }

            controller = null;
            search.error = error;
            search.busy = false;
        }
    }

    function cancel(): void {
        if (timer !== null) {
            clearTimeout(timer);
            timer = null;
        }

        controller?.abort();
        controller = null;
        asks++;
        search.busy = false;
    }

    function set(query: string, { isAtOnce = false }: { isAtOnce?: boolean } = {}): void {
        if (isStopped) {
            return;
        }

        cancel();
        search.query = query;
        search.error = null;
        if (options.isSearchable && !options.isSearchable(query)) {
            search.results = null;
            return;
        }

        search.busy = true;
        if (isAtOnce) {
            void ask(query);
            return;
        }

        timer = setTimeout(() => {
            timer = null;
            void ask(query);
        }, pause);
    }

    function stop(): void {
        cancel();
        isStopped = true;
    }

    const search = reactive({
        query: "",
        results: null,
        busy: false,
        error: null,
        set,
        cancel,
        stop
    }) as LatestSearch<T>;

    if (getCurrentScope()) {
        onScopeDispose(stop);
    }

    return search;
}
