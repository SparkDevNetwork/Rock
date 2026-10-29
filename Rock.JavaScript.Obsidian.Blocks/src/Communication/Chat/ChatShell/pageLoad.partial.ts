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
// What happens between sign-in and the first message on screen. The open channel's history, its
// live join and the sidebar all start at once: history needs nothing from the sidebar, so the
// time to the first message is sign-in plus one history call. The channel that opens is the one
// a link names, else the last one this browser opened for this person, else, only on a first
// visit, the sidebar's first row once it arrives.
//
// What the person sees meanwhile is one thing, owned here: the frame stands at mount, and its
// regions crossfade in together once the first screen is ready, rather than each popping in as
// its own answer lands.
import { InjectionKey, nextTick, reactive } from "vue";
import { SidebarRow } from "./types.partial";

/** The part of browser storage the shell uses. */
export type StorageLike = {
    getItem: (key: string) => string | null;
    setItem: (key: string, value: string) => void;
};

/**
 * The storage key for the last channel a person opened in a church, so that one person on a
 * shared computer never opens on another person's channel.
 *
 * @param tenantId The church.
 * @param personAliasGuid The person.
 *
 * @returns The key.
 */
export function rememberedChannelKey(tenantId: string, personAliasGuid: string): string {
    return `rock-chat:last-channel:${tenantId.toLowerCase()}:${personAliasGuid.toLowerCase()}`;
}

/**
 * Reads the remembered channel. Storage that is blocked or missing only loses the memory.
 *
 * @param storage The browser storage, or null when there is none.
 * @param key The key.
 *
 * @returns The channel, or null.
 */
export function readRememberedChannel(storage: StorageLike | null, key: string): string | null {
    try {
        return storage?.getItem(key) || null;
    }
    catch {
        // Intentionally ignored: storage blocked by the browser only loses the memory.
        return null;
    }
}

/**
 * Remembers the channel. Storage that is blocked or missing only loses the memory.
 *
 * @param storage The browser storage, or null when there is none.
 * @param key The key.
 * @param channelId The channel.
 */
export function writeRememberedChannel(storage: StorageLike | null, key: string, channelId: string): void {
    try {
        storage?.setItem(key, channelId);
    }
    catch {
        // Intentionally ignored: storage blocked or full only loses the memory.
    }
}

/**
 * The look the first screen's skeleton takes: a channel's (mark, title and description, flat
 * rows) or a direct message's (face and name, bubbles).
 */
export type RoomLook = "channel" | "direct";

/**
 * The key beside the remembered channel that holds its look, so a reload draws the right
 * skeleton before anything is known. A key of its own keeps the channel's key reading as it
 * always has.
 *
 * @param channelKey The remembered channel's key.
 *
 * @returns The key.
 */
function rememberedKindKey(channelKey: string): string {
    return `${channelKey}:kind`;
}

/**
 * Reads the remembered room's look. A first visit, blocked storage or a value this reader does
 * not know draws a channel's.
 *
 * @param storage The browser storage, or null when there is none.
 * @param channelKey The remembered channel's key.
 *
 * @returns The look.
 */
export function readRememberedKind(storage: StorageLike | null, channelKey: string): RoomLook {
    try {
        return storage?.getItem(rememberedKindKey(channelKey)) === "direct" ? "direct" : "channel";
    }
    catch {
        // Intentionally ignored: storage blocked by the browser only loses the memory.
        return "channel";
    }
}

/**
 * Remembers the room's look. Storage that is blocked or missing only loses the memory.
 *
 * @param storage The browser storage, or null when there is none.
 * @param channelKey The remembered channel's key.
 * @param kind The look.
 */
export function writeRememberedKind(storage: StorageLike | null, channelKey: string, kind: RoomLook): void {
    try {
        storage?.setItem(rememberedKindKey(channelKey), kind);
    }
    catch {
        // Intentionally ignored: storage blocked or full only loses the memory.
    }
}

/**
 * How long a first load waits before drawing the skeleton's marks. An estimate to measure on the
 * sandbox: a load faster than this never shows them, so a quick load does not flash grey.
 */
export const skeletonWaitMs = 150;

/**
 * How long the faces of the first screen get to decode once its data is in, before the reveal
 * goes without them. An estimate to measure on the sandbox. Faces held for the reveal show with
 * it as soon as they decode, never waiting on the image batch's own cap, so this one figure
 * governs the first load; a face later than it fades in on its own, through the batch.
 */
export const faceGraceMs = 250;

/**
 * How long after mount the reveal waits at most for the first screen's data. An estimate to
 * measure on the sandbox: past it, whatever has arrived is revealed and a region still waiting
 * keeps its skeleton and crossfades in when its data lands.
 */
export const revealCeilingMs = 2500;

/** What the first load shows. */
export type PageRevealState = {
    /** The look the skeleton draws, from the room opened last. */
    kind: RoomLook;

    /** Whether the skeleton's marks are drawn: the first load has run past the wait. */
    isSkeletonShown: boolean;

    /** Whether the sidebar's list has been revealed over its skeleton. */
    isSidebarRevealed: boolean;

    /** Whether the conversation's column (header, feed and composer) has been revealed. */
    isMainRevealed: boolean;

    /**
     * Whether the first screen's reveal has been released. Until then every photo and face waits,
     * and from then on they draw as they arrive; the page's reveal is their one owner.
     */
    isReleased: boolean;
};

/**
 * The first load's reveal. The block is ready when the sidebar has answered, the opening room's
 * first page has answered and its facts have answered (or there is nothing to open), after which
 * the faces on the first screen get a short grace to decode; then every region is revealed in
 * one tick, which the frame draws as one crossfade. A failure releases it early: what has arrived
 * is revealed, and a region still waiting is revealed on its own when it lands. The ceiling
 * releases it and reveals every region: one still waiting hands over to its own loading state, so
 * a call that never answers cannot lock the column.
 * Sign-in comes first without being tracked: nothing here can answer before it.
 *
 * Once stopped, every timer is gone and every later answer is refused, so a load that outlives
 * the page writes nothing.
 */
export type PageReveal = {
    /** What the frame draws. */
    state: PageRevealState;

    /** The sidebar answered, with rows or a failure. */
    sidebarAnswered: () => void;

    /** The opening room's first page answered, or the room was refused. */
    historyAnswered: () => void;

    /** The opening room's facts answered, or were refused. */
    factsAnswered: () => void;

    /**
     * A room is being opened, the page load's or one the person chose: the answers of the room
     * before it no longer count, so the column waits for the room on screen. Ignored once the
     * page is released, when the column no longer waits.
     */
    roomOpening: () => void;

    /** The page load found no room to open, so the column has nothing more to wait for. */
    nothingToOpen: () => void;

    /** Something failed: whatever has arrived is revealed now, without the face grace. */
    failed: () => void;

    /**
     * Holds the reveal for a face on the first screen that is still decoding, up to the grace.
     * Ignored once the reveal has been released.
     *
     * @param decoded Settles when the face has decoded, or failed to.
     */
    holdFace: (decoded: Promise<unknown>) => void;

    /** Cancels every timer and refuses every later answer. */
    stop: () => void;
};

/** What the frame offers to what draws images: the reveal's state and the face hold. */
export type PageRevealView = Pick<PageReveal, "state" | "holdFace">;

/**
 * Provided by the block, so a face on the first screen joins the reveal: inject it (it may be
 * absent where a component is drawn alone), read `state` to hold the face's own fade until its
 * region is revealed, and call `holdFace` with the decode so the reveal waits for it, within the
 * grace. A face drawn after the reveal has been released is not held.
 */
export const pageRevealKey: InjectionKey<PageRevealView> = Symbol("chatPageReveal");

/**
 * Creates the first load's reveal; the ceiling and the skeleton's wait run from now.
 *
 * @param options The look to draw before anything is known.
 *
 * @returns The reveal.
 */
export function createPageReveal(options: { kind: RoomLook }): PageReveal {
    const state = reactive<PageRevealState>({
        kind: options.kind,
        isSkeletonShown: false,
        isSidebarRevealed: false,
        isMainRevealed: false,
        isReleased: false
    });

    const answered = { sidebar: false, history: false, facts: false };
    let isStopped = false;
    let isGraceStarted = false;
    let isGraceOn = false;
    let heldFaces = 0;
    let graceTimer: ReturnType<typeof setTimeout> | null = null;

    function clearTimer(timer: ReturnType<typeof setTimeout> | null): null {
        if (timer !== null) {
            clearTimeout(timer);
        }
        return null;
    }

    // The marks wait so a load that is quick never shows them.
    let skeletonTimer: ReturnType<typeof setTimeout> | null = setTimeout(() => {
        skeletonTimer = null;
        state.isSkeletonShown = !(state.isSidebarRevealed && state.isMainRevealed);
    }, skeletonWaitMs);

    // Past the ceiling nothing more is waited for: a region whose data has not come is revealed
    // all the same and shows its own loading state, so no call can keep the column veiled.
    let ceilingTimer: ReturnType<typeof setTimeout> | null = setTimeout(() => {
        ceilingTimer = null;
        answered.sidebar = answered.history = answered.facts = true;
        release();
    }, revealCeilingMs);

    /** Reveals every region that has its data, once the reveal is released. */
    function reveal(): void {
        if (!state.isReleased) {
            return;
        }

        state.isSidebarRevealed ||= answered.sidebar;
        state.isMainRevealed ||= answered.history && answered.facts;

        if (state.isSidebarRevealed && state.isMainRevealed) {
            // Nothing is left to wait for or to draw.
            skeletonTimer = clearTimer(skeletonTimer);
            ceilingTimer = clearTimer(ceilingTimer);
            state.isSkeletonShown = false;
        }
    }

    function release(): void {
        graceTimer = clearTimer(graceTimer);
        state.isReleased = true;
        reveal();
    }

    /**
     * With every answer in, releases once the held faces have decoded or the grace is spent. The
     * faces of the region that answered last mount in the render its answer caused, so they are
     * counted after it, not before: counting at once would find none and skip the grace.
     */
    async function startGrace(): Promise<void> {
        if (isGraceStarted || state.isReleased) {
            return;
        }

        isGraceStarted = true;
        await nextTick();

        // Stopped, or released by a failure or the ceiling, while the render was pending.
        if (isStopped || state.isReleased) {
            return;
        }

        isGraceOn = true;
        if (heldFaces === 0) {
            release();
            return;
        }

        graceTimer = setTimeout(() => {
            graceTimer = null;
            release();
        }, faceGraceMs);
    }

    function answer(gate: keyof typeof answered): void {
        if (isStopped) {
            return;
        }

        answered[gate] = true;
        if (state.isReleased) {
            reveal();
        }
        else if (answered.sidebar && answered.history && answered.facts) {
            void startGrace();
        }
    }

    return {
        state,
        sidebarAnswered: () => answer("sidebar"),
        historyAnswered: () => answer("history"),
        factsAnswered: () => answer("facts"),
        roomOpening: (): void => {
            if (!isStopped && !state.isReleased) {
                answered.history = answered.facts = false;
            }
        },
        nothingToOpen: (): void => {
            answer("history");
            answer("facts");
        },
        failed: (): void => {
            if (!isStopped) {
                release();
            }
        },
        holdFace: (decoded: Promise<unknown>): void => {
            if (isStopped || state.isReleased) {
                return;
            }

            heldFaces++;
            const settled = (): void => {
                heldFaces--;
                if (!isStopped && isGraceOn && heldFaces === 0) {
                    release();
                }
            };
            decoded.then(settled, settled);
        },
        stop: (): void => {
            isStopped = true;
            skeletonTimer = clearTimer(skeletonTimer);
            ceilingTimer = clearTimer(ceilingTimer);
            graceTimer = clearTimer(graceTimer);
        }
    };
}

/** How opening a channel went. Superseded: the person opened another before this one finished. */
export type OpenOutcome = "opened" | "refused" | "failed" | "superseded";

/** What opening a channel reaches. */
export type ChannelOpenerDependencies = {
    /** Joins the channel's live topic. Returns at once; the join confirms later. */
    join: (channelId: string) => void;

    /** Fetches the channel's first page. Rejects with the platform's refusal when it refuses. */
    loadNewest: (channelId: string) => Promise<void>;

    /** Whether a failure is the platform refusing the channel to this person. */
    isRefusal: (error: unknown) => boolean;

    /** Remembers the channel once its history has loaded. */
    remember: (channelId: string) => void;
};

/**
 * Opens a channel: its live join and its history start together, neither waiting on the other.
 *
 * @param dependencies What opening a channel reaches.
 * @param channelId The channel.
 *
 * @returns How it went.
 */
export async function openChannel(dependencies: ChannelOpenerDependencies, channelId: string): Promise<OpenOutcome> {
    dependencies.join(channelId);
    const history = dependencies.loadNewest(channelId);

    try {
        await history;
    }
    catch (error) {
        return dependencies.isRefusal(error) ? "refused" : "failed";
    }

    dependencies.remember(channelId);
    return "opened";
}

/** What the page load reaches. */
export type PageLoadDependencies = {
    /** The channel a link named, or null. */
    linkedChannelId: string | null;

    /** The channel this browser opened last for this person, or null. */
    rememberedChannelId: string | null;

    /** Fetches the sidebar. */
    loadSidebar: () => Promise<SidebarRow[]>;

    /** Opens a channel. */
    open: (channelId: string) => Promise<OpenOutcome>;
};

/**
 * Runs the page load. The sidebar and the first channel start together when the channel is
 * known; otherwise the sidebar names it.
 *
 * @param dependencies What the page load reaches.
 *
 * @returns The channel that opened, or null when none could.
 */
export async function startPageLoad(dependencies: PageLoadDependencies): Promise<string | null> {
    const known = dependencies.linkedChannelId ?? dependencies.rememberedChannelId;
    const tried = new Set<string>();

    // Both start before either is awaited: the first message on screen waits on history
    // alone, never on the sidebar.
    const sidebar = dependencies.loadSidebar();

    // When the channel opens without the sidebar, nothing here waits on it, and its failure is
    // the sidebar's own to show; this keeps it from surfacing as an unhandled rejection.
    sidebar.catch(() => undefined);

    const opening = known ? dependencies.open(known) : null;

    if (known && opening) {
        tried.add(known);
        const outcome = await opening;

        if (outcome === "opened") {
            return known;
        }

        // A failure is shown where it happened; a channel the person has already moved away
        // from means they chose one themselves, and the page load has nothing more to open.
        if (outcome !== "refused") {
            return null;
        }
    }

    // Only a first visit, or a channel the person can no longer read, waits for the sidebar.
    let rows: SidebarRow[];
    try {
        rows = await sidebar;
    }
    catch {
        return null;
    }

    for (const row of rows) {
        if (tried.has(row.channel_id)) {
            continue;
        }

        tried.add(row.channel_id);
        const outcome = await dependencies.open(row.channel_id);

        if (outcome === "opened") {
            return row.channel_id;
        }

        if (outcome !== "refused") {
            return null;
        }
    }

    return null;
}
