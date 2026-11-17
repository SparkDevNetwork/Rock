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
// an ask a newer query overtook, and pages on. Before anything is typed a picker shows the
// platform's topic words, each with one cover from its filtered search: the words live on the
// platform alone, and the browser asks for them once a page, on the first open.
//
// This lives apart from the attachments because its lifetime is the picker's, and nothing in it
// uploads or signs a file.
import { InjectionKey, reactive, ref } from "vue";
import { CallResult, PlatformCall } from "../platformCall.partial";
import { ChatError, GifResult } from "../types.partial";
import { useLatestSearch } from "./useLatestSearch.partial";

// #region Search

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

/**
 * One topic word and its cover: a still from the word's own filtered search, at its size, or no
 * still where KLIPY gave none, which the picker draws as a plain plate.
 */
export type GifCover = { word: string, still: string | null, width: number, height: number };

/** The topic words in the platform's order, or that they could not be had. */
export type GifCovers = { ok: true, words: GifCover[] } | { ok: false };

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

    /** The topic words and their covers, asked once a page; a failed ask is asked again next time. */
    covers: () => Promise<GifCovers>;

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

    // The covers are the same for every church and change only as KLIPY's results do, so one
    // answer serves the page's life; a failure is not kept, so Try again asks again.
    let coversAsked: Promise<GifCovers> | null = null;

    /** Asks the platform for the topic words once, and reads its answer into this client's shape. */
    async function askCovers(): Promise<GifCovers> {
        const result: CallResult = await dependencies.callFunction("gif-search", { covers: true });
        if (!result.ok) {
            if (result.error.code === gifsOffCode) {
                isRefused.value = true;
            }
            coversAsked = null;
            return { ok: false };
        }

        // eslint-disable-next-line @typescript-eslint/naming-convention
        const data = (result.data ?? {}) as { words?: unknown };
        const entries = Array.isArray(data.words) ? data.words as Array<Record<string, unknown>> : [];
        const words = entries.filter(entry => typeof entry?.word === "string").map((entry): GifCover => {
            const hasCover = typeof entry.still === "string" && typeof entry.w === "number" && typeof entry.h === "number";
            return {
                word: entry.word as string,
                still: hasCover ? entry.still as string : null,
                width: hasCover ? entry.w as number : 0,
                height: hasCover ? entry.h as number : 0
            };
        });

        // No word to draw is no first view, so it is a failure the picker offers Try again for,
        // and not kept, so Try again asks again.
        if (words.length === 0) {
            coversAsked = null;
            return { ok: false };
        }

        return { ok: true, words };
    }

    return {
        // The refusal is read first so a reader is always subscribed to it, even while the church
        // switch is off.
        isOn: () => !isRefused.value && dependencies.isEnabled(),

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

        covers: (): Promise<GifCovers> => {
            if (!coversAsked) {
                coversAsked = askCovers();
            }
            return coversAsked;
        },

        turnOff: () => {
            isRefused.value = true;
        }
    };
}

/** Where a picker's search stands: the topics, waiting, GIFs, none, a failure or busy. */
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
     * Searches for what was typed, once typing stops, or at once for a topic or Enter.
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

    /** Set once the picker has closed, after which nothing is asked or written. */
    let isStopped = false;

    // One pause after the last keystroke, the ask a newer query replaces cancelled, and only the
    // newest query's answers kept, its further pages included.
    const search = useLatestSearch((query, signal, page) => gifs.search(query, page, signal), {
        isSearchable: query => query.length >= minimumQueryLength,
        onAnswer: (answer, _query, page) => {
            state.isLoadingMore = false;
            if (answer.ok) {
                // A later page may repeat a GIF an earlier one drew; a tile is drawn once.
                const seen = new Set(state.items.map(item => item.slug));
                state.items = page === 1 ? answer.items : [...state.items, ...answer.items.filter(item => !seen.has(item.slug))];
                state.hasNext = answer.hasNext;
                state.status = state.items.length > 0 ? "results" : "empty";
                return;
            }

            // A further page that failed leaves the GIFs already drawn where they are.
            if (page > 1) {
                state.hasNext = false;
                return;
            }

            // A refusal as off ends GIFs for the session, and the switch that hides them closes the
            // picker; a Try again that can never work is not drawn meanwhile.
            if (answer.error.code === gifsOffCode) {
                return;
            }

            if (answer.error.code === busyCode) {
                state.status = "busy";
                state.busyText = answer.error.text ?? busySentence;
                return;
            }

            state.status = "error";
        }
    });

    /** Asks the first page of a query, the GIFs drawn for the last one cleared. */
    function start(query: string, isAtOnce: boolean): void {
        state.items = [];
        state.hasNext = false;
        state.isLoadingMore = false;
        state.busyText = null;
        state.status = "loading";
        search.set(query, { isAtOnce });
    }

    return Object.assign(state, {
        setQuery: (query: string, isAtOnce = false): void => {
            if (isStopped) {
                return;
            }

            state.query = query;
            const trimmed = query.trim();
            if (trimmed.length < minimumQueryLength) {
                // a shorter query is no search, so the topics come back
                search.set(trimmed);
                state.items = [];
                state.hasNext = false;
                state.isLoadingMore = false;
                state.status = "idle";
                return;
            }

            start(trimmed, isAtOnce);
        },

        loadMore: (): void => {
            if (isStopped || state.status !== "results" || !state.hasNext || state.isLoadingMore) {
                return;
            }

            state.isLoadingMore = true;
            search.more();
        },

        retry: (): void => {
            const trimmed = state.query.trim();
            if (!isStopped && trimmed.length >= minimumQueryLength) {
                start(trimmed, true);
            }
        },

        stop: (): void => {
            isStopped = true;
            search.stop();
        }
    }) as GifSearch;
}

// #endregion Search

// #region Masonry

/** One drawn tile: what it draws and its place in order, which the keys move through. */
export type MasonryTile<T = GifResult> = { item: T, index: number, top: number, height: number };

/** The space between tiles, as a share of a column's width, so a column's height is in widths. */
const tileGap = 0.03;

/**
 * A GIF's height as a share of its width, from its preview's size, which is the one drawn.
 *
 * @param item The GIF.
 *
 * @returns Its height in widths.
 */
export function gifHeight(item: GifResult): number {
    const width = item.preview?.width || item.width;
    return (item.preview?.height || item.height) / (width || 1);
}

/**
 * Lays tiles out in columns of equal width, each in the column that is shortest when it comes, so
 * every GIF keeps its own shape and nothing is cropped. The results and the topics share it.
 *
 * @param items The tiles in order.
 * @param heightOf A tile's height as a share of its width.
 * @param count How many columns.
 *
 * @returns The columns, each its tiles from the top, with where each sits in column widths.
 */
export function masonry<T>(items: T[], heightOf: (item: T) => number, count = 2): MasonryTile<T>[][] {
    const columns: MasonryTile<T>[][] = Array.from({ length: count }, () => []);
    const heights = Array<number>(count).fill(0);

    items.forEach((item, index) => {
        const shortest = heights.indexOf(Math.min(...heights));
        const height = heightOf(item);
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
export function nextInMasonry(columns: MasonryTile<unknown>[][], from: number, key: string): number | null {
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
