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
// Every room put on screen, the first above all. A room's history, its live join and its facts
// start at once, and the one chosen last wins however the answers land. On the first screen the
// sidebar starts beside them: history needs nothing from it, so the time to the first message is
// sign-in plus one history call. The channel that opens first is the one a link names, else the
// last one this browser opened for this person, else, only on a first visit, the sidebar's first
// row once it arrives.
//
// What the person sees meanwhile is one thing, owned here: the frame stands at mount, and its
// regions crossfade in together once the first screen is ready, rather than each popping in as
// its own answer lands.
import { InjectionKey, nextTick, reactive } from "vue";
import { classifyPlatformError } from "./errors.partial";
import type { ReadTracker } from "./composables/useMarkRead.partial";
import type { ChatShell, ShellState } from "./shell.partial";
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
 * How long after sign-in the reveal waits at most for the first screen's data. An estimate to
 * measure on the sandbox: past it, whatever has arrived is revealed and a region still waiting
 * keeps its skeleton and crossfades in when its data lands. It counts from sign-in, as nothing can
 * answer before it: a sign-in that waits out an outage is the veil's to carry, never a reveal of
 * an empty column over loading rows.
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
 * Sign-in comes first, and the ceiling starts there: nothing here can answer before it.
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

    /** The session signed in, so the page load is under way and the ceiling starts. */
    signedIn: () => void;

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
 * Creates the first load's reveal; the skeleton's wait runs from now, the ceiling from sign-in.
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
    let ceilingTimer: ReturnType<typeof setTimeout> | null = null;

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
        signedIn: (): void => {
            if (isStopped || state.isReleased || ceilingTimer !== null) {
                return;
            }
            ceilingTimer = setTimeout(() => {
                ceilingTimer = null;
                answered.sidebar = answered.history = answered.facts = true;
                release();
            }, revealCeilingMs);
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

/** What putting rooms on screen reads from the shell. */
export type RoomOpenerDependencies = {
    state: ShellState;

    /** The shell's modules a room's opening and closing reach, each made before this. */
    modules: Pick<ChatShell, "reveal" | "channels" | "timelines" | "messageActions" | "rooms" | "directMessages">;

    /** The read tracker, which follows the room on screen. */
    tracker: ReadTracker;

    storage: StorageLike | null;

    /** Where this person's last room and its look are remembered. */
    storageKey: string;

    /** The channel a link named, or null. */
    linkedChannelId: string | null;

    /** Marks a step on the page's performance timeline. */
    mark: (name: string) => void;

    /** Joins a channel's live topic, once the live connection is there. */
    join: (channelId: string) => void;

    /** Fetches the sidebar, through the shell's one read. */
    loadSidebar: () => Promise<SidebarRow[]>;

    /** Whether the shell has stopped or its session ended, read at each resumed step. */
    isStopped: () => boolean;
};

/** Every room put on screen: the page load's first, the person's choices, and closing the one open. */
export type RoomOpener = {
    /** Opens a channel, kept as the last open asked for. */
    open: (channelId: string, isPageLoad?: boolean) => Promise<OpenOutcome>;

    /** Opens a channel the person chose, or waits for the open of it already under way. */
    select: (channelId: string) => Promise<void>;

    /** Takes the open channel off the screen. */
    closeActive: () => void;

    /** Opens the first conversation, saying chat is opening until it settles. */
    runPageLoad: () => Promise<void>;

    /** A room asked for before sign-in becomes the room the page load opens; after it, nothing changes. */
    relink: (channelId: string) => void;
};

/**
 * Creates what puts rooms on screen.
 *
 * @param dependencies What it reads from the shell.
 *
 * @returns The opener.
 */
export function createRoomOpener(dependencies: RoomOpenerDependencies): RoomOpener {
    const { state, modules, tracker, storage, storageKey, mark, isStopped } = dependencies;
    const { reveal, channels, timelines, messageActions, rooms, directMessages } = modules;

    /** Counts opens, so an open that a later one has replaced stops touching anything. */
    let openCount = 0;

    /** The open asked for last and the room it opens, which selecting that room again waits for. */
    let lastOpen: { channelId: string, outcome: Promise<OpenOutcome> } | null = null;

    /**
     * The room the page load opens first: the one the page's link named, or one a push's tap asked
     * for before sign-in, the last asked winning, as a link that arrived late.
     */
    let linkedChannelId = dependencies.linkedChannelId;

    /**
     * Puts a channel on screen, or none. Every switch passes through here, so a message's editor
     * and delete confirm close with the conversation they were opened in.
     */
    function showChannel(channelId: string | null): void {
        if (state.activeChannelId !== channelId) {
            messageActions.switchConversation(channelId);
            rooms.conversationShown(channelId);
        }
        state.activeChannelId = channelId;
        channels.setActive(channelId);
    }

    /**
     * Its position is saved, its catch-up stops, and an open of it still loading is superseded,
     * so it cannot take the screen back.
     */
    function closeActive(): void {
        openCount++;
        void tracker.leave();
        if (state.activeChannelId) {
            timelines.leave(state.activeChannelId);
        }
        showChannel(null);
    }

    /** A second ask for the same room while it loads waits for this open rather than finding it empty. */
    function open(channelId: string, isPageLoad: boolean = false): Promise<OpenOutcome> {
        const outcome = runOpen(channelId, isPageLoad);
        lastOpen = { channelId, outcome };
        return outcome;
    }

    /**
     * Lets go of the room being left without waiting, then joins and fetches together. A person
     * can open rooms faster than the platform answers; the one chosen last is on screen, joined,
     * tracked and remembered.
     *
     * @param isPageLoad Whether the first load is opening this room, which then decides what a
     * refusal means for the reveal; a room the person chose answers it for itself.
     */
    async function runOpen(channelId: string, isPageLoad: boolean): Promise<OpenOutcome> {
        const thisOpen = ++openCount;
        // a stopped shell's open touches nothing more, however its answers land
        const isCurrent = (): boolean => thisOpen === openCount && !isStopped();

        // The first screen's column waits for the room now on screen, not one it replaces.
        reveal.roomOpening();

        if (state.activeChannelId && state.activeChannelId !== channelId) {
            void tracker.leave();
            timelines.leave(state.activeChannelId);
        }

        if (state.activeChannelId !== channelId) {
            tracker.show(channelId);
        }
        showChannel(channelId);

        // The facts are read beside the history, neither waiting for the other; both answers, a
        // refusal too, are what the first load waits on.
        void rooms.channelOpened(channelId).then(() => {
            if (isCurrent()) {
                // The look is the room's own, so the next load's skeleton has its shape.
                const facts = rooms.factsOf(channelId);
                if (facts) {
                    writeRememberedKind(storage, storageKey, facts.channel_type === "dm" ? "direct" : "channel");
                }
                reveal.factsAnswered();
            }
        });

        const outcome = await openChannel({
            join: id => dependencies.join(id),
            loadNewest: async id => {
                await timelines.loadNewest(id);
                if (isCurrent()) {
                    reveal.historyAnswered();
                    mark("chat:history");
                    tracker.open(id, timelines.state(id).readCursor);
                }
            },
            isRefusal: error => classifyPlatformError(error).severity === "permission",
            remember: id => {
                if (isCurrent()) {
                    writeRememberedChannel(storage, storageKey, id);
                }
            }
        }, channelId);

        if (!isCurrent()) {
            return "superseded";
        }

        if (outcome !== "opened") {
            showChannel(null);

            // A room the person chose has nothing more to load; what has arrived shows now. The
            // first load's own room leaves that to the load.
            if (!isPageLoad) {
                reveal.nothingToOpen();
                reveal.failed();
            }
        }

        return outcome;
    }

    /**
     * Opens the first conversation: the one a link names or this browser opened last, else the
     * first the sidebar can open. The page says chat is opening until it settles.
     */
    async function runPageLoad(): Promise<void> {
        if (isStopped()) {
            return;
        }

        state.isOpening = true;
        try {
            const last: { outcome: OpenOutcome | null } = { outcome: null };
            const opened = await startPageLoad({
                linkedChannelId: linkedChannelId?.toLowerCase() ?? null,
                rememberedChannelId: readRememberedChannel(storage, storageKey),
                loadSidebar: () => dependencies.loadSidebar(),
                // The load decides the opening room, so one it opens answers nothing to the reveal
                // when refused: the next row may still open, and the column must not reveal empty.
                open: async id => (last.outcome = await open(id, true))
            });

            // The person opened a room themselves meanwhile, and that open answers the column.
            if (opened !== null || last.outcome === "superseded") {
                return;
            }

            // Nothing could open: the column waits no longer, and a room that failed or was
            // refused shows what has arrived now.
            reveal.nothingToOpen();
            if (last.outcome !== null) {
                reveal.failed();
            }
        }
        finally {
            state.isOpening = false;
        }
    }

    /** Opens a channel the person chose, or waits for the open of it already under way. */
    async function select(channelId: string): Promise<void> {
        directMessages.close();
        if (channelId !== state.activeChannelId) {
            await open(channelId);
        }
        else if (lastOpen?.channelId === channelId) {
            await lastOpen.outcome;
        }
    }

    return {
        open,
        select,
        closeActive,
        runPageLoad,
        relink: (channelId: string): void => {
            // Read once the session has signed in; a tap after that opens the room itself.
            if (state.phase === "starting") {
                linkedChannelId = channelId;
            }
        }
    };
}
