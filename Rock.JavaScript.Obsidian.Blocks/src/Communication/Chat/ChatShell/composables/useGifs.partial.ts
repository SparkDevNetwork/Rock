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

// GIF search. The platform's own search function asks KLIPY on the church's behalf, so the key
// never reaches a browser, and signs each result for a send by this church alone. The session holds
// one search and the switch that says whether GIFs are offered: the church's, as the platform's
// settings carry it, until the platform refuses a search or a send as off, which hides them at
// once rather than at the next token. A picker holds its own search state while it is open: it
// waits for the person to stop typing, asks once for each query long enough to mean something, ends
// an ask a newer query overtook, and pages on.
//
// This lives apart from the attachments because its lifetime is the picker's, and nothing in it
// uploads or signs a file.
import { InjectionKey, reactive, ref } from "vue";
import { CallResult, PlatformCall } from "../platformCall.partial";
import { ChatError, GifResult } from "../types.partial";

// #region Search

/** The words offered before anything is typed, since the picker shows nothing trending. */
export const gifSuggestions = ["Thanks", "Praying", "Amen", "Celebrate", "Welcome", "Haha", "Wow", "Love it", "Yes", "Coffee"];

/** How long typing must stop before a search is asked, so each word typed is not its own search. */
const debounceMs = 300;

/** The shortest query asked; one letter matches too much to be worth KLIPY's answer. */
const minimumQueryLength = 2;

/** What busy says when the platform sent no sentence of its own. */
const busySentence = "GIF search is busy. Try again in a moment.";

/** The refusals the search acts on itself. */
const gifsOffCode = "rpc.gifs_off";
const busyCode = "rpc.gif_search_busy";

/** One page of a search: GIFs in KLIPY's order and whether more follow, or the refusal. */
export type GifSearchAnswer =
    | { ok: true, items: GifResult[], hasNext: boolean }
    | { ok: false, error: ChatError, retryAfterSeconds: number | null };

/** The session's GIF search, and whether GIFs are offered at all. */
export type Gifs = {
    /** Whether the church has GIFs on and the platform has not since refused them as off; reactive. */
    isOn: () => boolean;

    /**
     * Asks for a page of a query.
     *
     * @param query What was typed.
     * @param page From 1.
     * @param signal Ends an ask no longer wanted.
     */
    search: (query: string, page: number, signal?: AbortSignal) => Promise<GifSearchAnswer>;

    /** Hides GIFs for the rest of the page's life, as a refusal as off says to. */
    turnOff: () => void;
};

/** What the session's GIF search reaches outside itself. */
export type GifDependencies = {
    callFunction: PlatformCall["callFunction"];

    /** Whether the church has GIFs on, as the platform's settings last said. */
    isEnabled: () => boolean;
};

/** Where every composer finds the session's GIF search, provided once by the chat. */
export const gifsKey: InjectionKey<Gifs> = Symbol("chat-gifs");

/**
 * Creates the session's GIF search.
 *
 * @param dependencies What it reaches outside itself.
 *
 * @returns The search and its switch.
 */
export function createGifs(dependencies: GifDependencies): Gifs {
    // A refusal as off is the platform's live word, sooner than the next token's settings; it
    // holds until the page is loaded again, as a church turning GIFs back on within one visit is
    // rare enough to wait for that.
    const isRefused = ref(false);

    return {
        isOn: () => dependencies.isEnabled() && !isRefused.value,

        search: async (query, page, signal): Promise<GifSearchAnswer> => {
            const result: CallResult = await dependencies.callFunction("gif-search", { q: query, page }, signal ? { signal } : undefined);
            if (!result.ok) {
                if (result.error.code === gifsOffCode) {
                    isRefused.value = true;
                }
                return { ok: false, error: result.error, retryAfterSeconds: result.retryAfterSeconds };
            }

            // The platform answers its own shape; anything else reads as nothing found.
            // eslint-disable-next-line @typescript-eslint/naming-convention
            const data = (result.data ?? {}) as { items?: unknown, has_next?: unknown };
            return {
                ok: true,
                items: Array.isArray(data.items) ? data.items as GifResult[] : [],
                hasNext: data.has_next === true
            };
        },

        turnOff: () => {
            isRefused.value = true;
        }
    };
}

/** Where a picker's search stands: suggestions, waiting, GIFs, none, a failure or busy. */
export type GifSearchStatus = "idle" | "loading" | "results" | "empty" | "error" | "busy";

/** A picker's search, read reactively. */
export type GifSearch = {
    readonly query: string;
    readonly status: GifSearchStatus;
    readonly items: GifResult[];
    readonly hasNext: boolean;

    /** Whether a further page is on its way, which asks no other. */
    readonly isLoadingMore: boolean;

    /** What busy says: the platform's sentence, or this client's. */
    readonly busyText: string | null;

    /**
     * Searches for what was typed, once typing stops, or at once for a suggestion or Enter.
     *
     * @param query What the search box holds.
     * @param isAtOnce Whether to ask now rather than after the pause.
     */
    setQuery: (query: string, isAtOnce?: boolean) => void;

    /** Asks the next page, unless one is on its way or there is none. */
    loadMore: () => void;

    /** Asks the query again after a failure. */
    retry: () => void;

    /** Ends the ask in flight; nothing is asked or written after. */
    stop: () => void;
};

/**
 * Creates a picker's search.
 *
 * @param gifs The session's GIF search.
 *
 * @returns The search's state and what changes it.
 */
export function createGifSearch(gifs: Gifs): GifSearch {
    const state = reactive({
        query: "",
        status: "idle" as GifSearchStatus,
        items: [] as GifResult[],
        hasNext: false,
        isLoadingMore: false,
        busyText: null as string | null
    });

    let page = 0;
    let timer: ReturnType<typeof setTimeout> | null = null;
    let controller: AbortController | null = null;

    // Every ask is numbered; an answer to any but the newest is dropped.
    let asks = 0;
    let isStopped = false;

    function cancel(): void {
        if (timer !== null) {
            clearTimeout(timer);
            timer = null;
        }
        controller?.abort();
        controller = null;
    }

    async function ask(query: string, next: number): Promise<void> {
        const number = ++asks;
        controller = new AbortController();
        const answer = await gifs.search(query, next, controller.signal);
        if (isStopped || number !== asks) {
            return;
        }

        controller = null;
        state.isLoadingMore = false;
        if (answer.ok) {
            page = next;
            state.items = next === 1 ? answer.items : [...state.items, ...answer.items];
            state.hasNext = answer.hasNext;
            state.status = state.items.length > 0 ? "results" : "empty";
            return;
        }

        // A further page that failed leaves the GIFs already drawn where they are.
        if (next > 1) {
            state.hasNext = false;
            return;
        }

        if (answer.error.code === busyCode) {
            state.status = "busy";
            state.busyText = answer.error.text ?? busySentence;
            return;
        }

        state.status = "error";
    }

    function start(query: string): void {
        page = 0;
        state.items = [];
        state.hasNext = false;
        state.isLoadingMore = false;
        state.busyText = null;
        state.status = "loading";
        void ask(query, 1);
    }

    return Object.assign(state, {
        setQuery: (query: string, isAtOnce = false): void => {
            if (isStopped) {
                return;
            }

            cancel();
            state.query = query;
            const trimmed = query.trim();
            if (trimmed.length < minimumQueryLength) {
                // a shorter query is no search, so the suggestions come back
                asks++;
                state.items = [];
                state.hasNext = false;
                state.isLoadingMore = false;
                state.status = "idle";
                return;
            }

            if (isAtOnce) {
                start(trimmed);
                return;
            }

            state.status = "loading";
            state.items = [];
            timer = setTimeout(() => {
                timer = null;
                start(trimmed);
            }, debounceMs);
        },

        loadMore: (): void => {
            if (isStopped || state.status !== "results" || !state.hasNext || state.isLoadingMore) {
                return;
            }

            state.isLoadingMore = true;
            void ask(state.query.trim(), page + 1);
        },

        retry: (): void => {
            const trimmed = state.query.trim();
            if (!isStopped && trimmed.length >= minimumQueryLength) {
                cancel();
                start(trimmed);
            }
        },

        stop: (): void => {
            isStopped = true;
            cancel();
        }
    }) as GifSearch;
}

// #endregion Search

// #region Masonry

/** One drawn tile: the GIF and its place in KLIPY's order, which the keys move through. */
export type MasonryTile = { item: GifResult, index: number, top: number, height: number };

/** The space between tiles, as a share of a column's width, so a column's height is in widths. */
const tileGap = 0.03;

/**
 * Lays GIFs out in columns of equal width, each in the column that is shortest when it comes, so
 * every GIF keeps its own shape and nothing is cropped.
 *
 * @param items The GIFs in KLIPY's order.
 * @param count How many columns.
 *
 * @returns The columns, each its tiles from the top, with where each sits in column widths.
 */
export function masonry(items: GifResult[], count = 2): MasonryTile[][] {
    const columns: MasonryTile[][] = Array.from({ length: count }, () => []);
    const heights = Array<number>(count).fill(0);

    items.forEach((item, index) => {
        const shortest = heights.indexOf(Math.min(...heights));
        const width = item.preview?.width || item.width;
        const height = (item.preview?.height || item.height) / (width || 1);
        columns[shortest].push({ item, index, top: heights[shortest], height });
        heights[shortest] += height + tileGap;
    });

    return columns;
}

/**
 * Where an arrow key moves from a tile: Up and Down along its column, Left and Right to the tile
 * in the next column whose middle is nearest its own, since the columns' tiles do not line up.
 *
 * @param columns The laid out columns.
 * @param from The tile's place in KLIPY's order.
 * @param key The arrow pressed.
 *
 * @returns The place moved to, or null at an edge.
 */
export function nextInMasonry(columns: MasonryTile[][], from: number, key: string): number | null {
    const column = columns.findIndex(tiles => tiles.some(tile => tile.index === from));
    if (column < 0) {
        return null;
    }

    const row = columns[column].findIndex(tile => tile.index === from);
    if (key === "ArrowDown" || key === "ArrowUp") {
        return columns[column][row + (key === "ArrowDown" ? 1 : -1)]?.index ?? null;
    }

    if (key !== "ArrowLeft" && key !== "ArrowRight") {
        return null;
    }

    const beside = columns[column + (key === "ArrowRight" ? 1 : -1)];
    if (!beside?.length) {
        return null;
    }

    const tile = columns[column][row];
    const middle = tile.top + tile.height / 2;
    const nearest = beside.reduce((best, other) => Math.abs(other.top + other.height / 2 - middle) < Math.abs(best.top + best.height / 2 - middle) ? other : best);
    return nearest.index;
}

// #endregion Masonry
