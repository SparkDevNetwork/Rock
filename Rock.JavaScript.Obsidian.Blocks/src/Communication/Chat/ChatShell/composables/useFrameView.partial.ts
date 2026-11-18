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
// The block's frame, as one owner: what each column shows, how the block moves between them, and
// where focus goes as each move ends.
//
// The panel column holds one panel at a time (the threads list, the mentions inbox, the members
// panel or a thread in its drawer), with the way back from a thread to the list it came from and
// focus handed back to what opened a panel. The main column's full views (Browse channels, New
// message) take the conversation's place. On a phone one of the three panes is in front, worked
// out in one place from everything that can take a column.
//
// Focus has one owner here. A move asks where focus should go; once the frame has drawn the change,
// and every part fading out is out of reach, one settle puts it there, else leaves it where it is
// while that is still drawn, else gives it to the start of the column that last held it.
//
// The landings on one message that may not be drawn yet are their own, in useLandings, made here
// since they open and close the drawer the frame owns.
//
// The panels' wiring to the shell lives here too: the members panel and its two dialogs, the
// threads list and the drawer's older replies, since which panel shows and what it asks of the
// shell change together.

import { computed, ComputedRef, nextTick, onBeforeUnmount, ref, Ref, shallowRef, ShallowRef, StyleValue, watch, watchEffect } from "vue";
import { ToastNotice } from "../components/errorToast.partial.obs";
import { ChatShell } from "../shell.partial";
import { FocusStop, PersonRow } from "../types.partial";
import { tokenDuration, tokenLength } from "../ui/brand.partial";
import { EscapeLayer, focusQuietly, isDrawn, pullEscapeLayer, pushEscapeLayer } from "../ui/focusTrap.partial";
import { isMotionReduced } from "../ui/viewport.partial";
import { Landings, ReplyLanding, useLandings } from "./useLandings.partial";
import { searchStatus, shownMembers, successorChoices } from "./useMembers.partial";
import { useRoomsView } from "./useRoomsView.partial";
import { FollowError, OpenThreadState, ThreadListState, ThreadListTab, ThreadPlace, unfollowNotice } from "./useThreads.partial";

// #region Types

/**
 * What the panel column can hold: the open conversation's threads, Activity (the mentions inbox
 * and every thread followed, across conversations), the members panel, one thread in its drawer,
 * or a person's full profile.
 */
export type PanelKind = "threads" | "activity" | "members" | "thread" | "person";

/** The panels that list something, which a thread can be opened from or which take a thread's place. */
export type ListPanelKind = "threads" | "activity" | "members";

/** Activity's two lists: the mentions inbox, and every thread the reader follows. */
export type ActivityTab = "mentions" | "threads";

/** The list the column draws: Activity's shown tab, or the open panel itself. */
export type ShownList = "threads" | "mentions" | "members" | "thread" | "person";

/**
 * Where the sidebar is while a window too narrow for three columns holds a panel: folded away, or
 * opened over the conversation; null while it stands in its own column.
 */
export type SidebarFold = "folded" | "over";

/** A view that takes the conversation's place in the main column: Browse channels or New message. */
export type MainViewKind = "directory" | "newdm";

/** Which pane a phone shows in front. */
export type FramePane = "list" | "conversation" | "panel";

/** The list a thread was opened from, which its back chevron returns to, or null when it was opened from the conversation. */
export type ThreadOrigin = "threads" | "activity" | null;

/** The list a person's profile was opened from, which its back chevron returns to, or null for none. */
export type PersonOrigin = "members" | null;

/**
 * A place focus can go, tried in its order: a stop by its mark, a stop narrowed where it repeats
 * (a sidebar row by its conversation, a list's row by its root) or looked for inside one element,
 * or an element already in hand, as what opened a view. Null is a place not drawn, skipped.
 */
export type FocusTarget = FocusStop | { stop: FocusStop, match?: string, within?: HTMLElement } | HTMLElement | null | undefined;

/** A column of the frame, which focus lost from it goes back to the start of. */
type FocusRegion = "sidebar" | "main" | "panel";

/** Where the focus helper looks for its stops and how it puts focus there. */
export type FocusStopOptions = {
    /** The frame's column the stops are looked for in: the conversation's, the panel's, or the whole block's when not given. */
    within?: "main" | "panel";

    /** Leaves what holds the stop unscrolled, for a pane or column still sliding in. */
    preventScroll?: boolean;

    /** Without the tooltip a focused button shows, as a panel opening puts focus on its X only so the keyboard starts inside it. */
    isQuiet?: boolean;

    /**
     * The person asked to type there, as a quote or a closed editor hands typing back, so a
     * phone's keyboard rising is wanted and the composer is not skipped.
     */
    isTyping?: boolean;
};

/** What the frame reads from the block. */
export type FrameViewDependencies = {
    /** The block's shell, read on every use since a birthdate replaces it. */
    shell: Readonly<Ref<ChatShell>>;

    activeChannelId: Readonly<Ref<string | null>>;

    /** The block's root element, which focus is looked for in and the motion tokens are read from. */
    rootElement: Readonly<Ref<HTMLElement | undefined>>;

    /** The reply a link to this page named, which opens its thread over that conversation once it is the first to open. */
    linkedReply: ReplyLanding | null;

    /** The church's campuses, from the init box, which Add people's search names. */
    campuses: Array<{ guid: string, name?: string | null }>;

    /** Shows a toast that is not a failure, one per code. */
    showNotice: (notice: ToastNotice) => void;
};

/** What the template draws of the frame, and the ways the block moves between its columns. */
export type Frame = MembersPanelViews & ThreadsPanelWiring & Landings & {
    /** Whether the block itself is narrower than a phone's layout allows, wherever it sits in the window. */
    isPhone: Ref<boolean>;

    /** Turned on by the person's first move between panes, so the pane a page load opens does not slide in. */
    isPaneMotionOn: Ref<boolean>;

    pane: ComputedRef<FramePane>;

    isSidebarHidden: ComputedRef<boolean>;

    /**
     * Whether the conversation's column is out of reach: off a phone's screen, or under the
     * sidebar laid over it, where it stays in view, so push still counts it seen.
     */
    isMainInert: ComputedRef<boolean>;

    isPanelHidden: ComputedRef<boolean>;

    /** Whether the panel column is out of reach: hidden, or under the sidebar laid over the conversation. */
    isPanelInert: ComputedRef<boolean>;

    isSidebarGone: Ref<boolean>;

    isConversationGone: Ref<boolean>;

    isPanelGone: Ref<boolean>;

    goneStyle: StyleValue;

    /** The panel open in the right-hand column, or null while it is closed. */
    panelKind: Ref<PanelKind | null>;

    /**
     * The full view drawn in the conversation's place, or null. The conversation stays laid out
     * underneath and out of reach, so its place in the feed survives.
     */
    mainView: Ref<MainViewKind | null>;

    /**
     * The room Browse previews, which is never the room on screen: previewing it marks nothing
     * read, adds no sidebar row and keeps the link. Null for none.
     */
    previewId: Ref<string | null>;

    /** What the column draws: the open panel, kept through its close so it narrows over its content. */
    shownPanel: Ref<PanelKind | null>;

    /** Activity's shown tab, kept while it is closed, so it opens again where it was. */
    activityTab: Ref<ActivityTab>;

    /** The list the column draws, Activity's by its tab. */
    shownList: ComputedRef<ShownList | null>;

    /** Where the sidebar is while a narrow window holds a panel, or null while it has its column. */
    sidebarFold: ComputedRef<SidebarFold | null>;

    /** Whether the open conversation has Members to offer: a channel or a group, never a conversation of two. */
    isMembersOffered: ComputedRef<boolean>;

    panelElement: Ref<HTMLElement | undefined>;

    sidebarElement: Ref<HTMLElement | undefined>;

    /** The conversation's column, which holds its header, feed and composer. */
    mainElement: Ref<HTMLElement | undefined>;

    /** The full view drawn in the conversation's place, while one is. */
    mainViewElement: Ref<HTMLElement | undefined>;

    /** The thread the drawer draws, kept after the threads let it go so the drawer narrows over it. */
    drawnThread: ShallowRef<OpenThreadState | null>;

    /** The list the open thread came from, which its back chevron and Escape return to, or null. */
    threadOrigin: Ref<ThreadOrigin>;

    /** The list the profile in the column came from, which its back chevron returns to, or null. */
    personOrigin: Ref<PersonOrigin>;

    /** What the frame draws of rooms: the open conversation's, the open thread's and the one New message found. */
    rooms: ReturnType<typeof useRoomsView>;

    onResize: (entries: ResizeObserverEntry[]) => void;

    /** Focus came into the frame: the column it is in is where focus lost from it goes back to. */
    onFocusIn: (event: FocusEvent) => void;

    /** Focus left the frame for nowhere from a part still drawn: the person put it on the page, and no column is owed it. */
    onFocusOut: (event: FocusEvent) => void;

    onSelect: (channelId: string) => Promise<void>;

    onBack: () => Promise<void>;

    onListPanel: (kind: ListPanelKind) => Promise<void>;

    /** Activity's Segmented chose its other tab. */
    onActivityTab: (tab: ActivityTab) => void;

    /**
     * The column's crossfade began to fade a panel out: it is out of reach from then on, so the
     * keyboard and focus go only into the panel coming in.
     */
    onPanelLeave: (element: Element) => void;

    /** The header's sidebar button, while the sidebar is folded away: it opens over the conversation. */
    onSidebarOver: () => void;

    /** The sidebar's own button, while it is laid over the conversation: it folds away again. */
    onSidebarAway: () => void;

    /** The threads list asked for every thread followed, its empty state's way there: Activity on Threads. */
    onThreadsTab: (tab: ThreadListTab) => void;

    openThreadFromList: (rootId: number, channelId: string) => void;

    onOpenThread: (place: ThreadPlace) => void;

    onThreadBack: () => Promise<void>;

    onMentionOpen: (messageId: number) => void;

    openThreadFromMentions: (channelId: string, rootId: number, messageId: number) => Promise<void>;

    openFromTap: (channelId: string, rootId: number | null, messageId: number | null) => void;

    openThreadDrawer: (channelId: string, rootId: number, origin: ThreadOrigin) => Promise<void>;

    closeThreadDrawer: () => void;

    /**
     * Closes the panel column, with focus on what is given when it is still drawn, else on the
     * header button that opened the panel.
     */
    closePanel: (returnTo?: HTMLElement | null) => void;

    /**
     * The face or name a card was opened from. The column shows the open card's person, so focus
     * goes back to the opener of the person it shows, however the profile closes.
     */
    notePersonOpener: (opener: HTMLElement) => void;

    /**
     * The profile open as a card moves into the column in full. One opened from the members panel
     * keeps that list to go back to; any other panel gives the profile its place.
     */
    openPersonPanel: () => void;

    /** The profile's X: focus goes back to the face or name that opened the person it shows, while it is drawn. */
    closePersonPanel: () => void;

    /** Back from a profile to the members list it was opened from, with focus on its person's row. */
    onPersonBack: () => Promise<void>;

    /**
     * Opens a full view in the conversation's place, or brings the one open back to the front.
     * New message begins with the people given, as a profile card's Message chooses its person.
     */
    openMainView: (kind: MainViewKind, people?: PersonRow[]) => Promise<void>;

    /** Closes the full view: its X or Escape. The conversation underneath comes back. */
    closeMainView: () => void;

    /** A room chosen in the full view: previewed if it has no sidebar row, opened, and the view closed. */
    openFromMainView: (channelId: string) => Promise<void>;

    /** Browse previews a room beside its list, or none, reading the room's history for the preview. */
    onPreview: (channelId: string | null) => void;

    /** Join from Browse's preview; once it lands the preview reads the room again, for the door's line. */
    onPreviewJoin: (channelId: string) => Promise<void>;

    /** Add people for a room: its dialog, never for a conversation of two, which Rock keeps as it is. */
    openAddPeople: (channelId: string) => void;

    /** Counts every thread opened or left, so a landing can tell the drawer it started in is gone. */
    threadOpenCount: () => number;

    /**
     * Puts focus on the first place of an order that is drawn, each stop found by its mark. A
     * phone skips the composer, whose focus would raise the keyboard over what the person came to
     * read, unless they asked to type there.
     */
    focusStop: (order: FocusTarget[], options?: FocusStopOptions) => HTMLElement | null;
};

/** What the members panel, the remove confirm and Add people draw of the roster. */
type MembersPanelViews = ReturnType<typeof membersPanelViews>;

/** What the threads list and the drawer read of the shell's threads, and what they ask of it. */
type ThreadsPanelWiring = ReturnType<typeof threadsPanelWiring>;

// #endregion Types

/** Below this width the block shows one pane at a time, as on a phone. */
const phoneWidth = 760;

/** The design's pane slide, used when the sheet is not on the page. */
const fallbackPaneSlideMs = 300;

/** The sheet's sidebar and panel widths and the conversation's least width, used when the sheet is not on the page. */
const fallbackSidePx = 296;
const fallbackPanelPx = 392;
const fallbackMainMinPx = 480;

/**
 * Creates the block's frame. Its watches of the open conversation are made here in the order they
 * must run, so a thread a link opens is not closed by the same switch that brought its conversation.
 *
 * @param dependencies What the frame reads from the block.
 *
 * @returns What the template draws and the ways to move between the columns.
 */
export function useFrameView(dependencies: FrameViewDependencies): Frame {
    const { shell, activeChannelId, rootElement } = dependencies;

    // #region Values

    const isPhone = ref(false);

    /** Whether the block is narrower than the sidebar, a panel and the conversation's least width side by side. */
    const isNarrow = ref(false);

    /** Set by the header's sidebar button while the sidebar is folded, until a conversation is chosen. */
    const isSidebarOver = ref(false);

    /** Set by Back on a phone, so the list shows while the conversation stays open behind it. */
    const isListChosen = ref(false);

    const isPaneMotionOn = ref(false);

    // A pane that has slid out of view is also hidden, once its slide is over, because the oldest
    // Safari Rock supports ignores inert and would still let Tab walk into it.
    const isSidebarGone = ref(false);
    const isConversationGone = ref(false);
    const isPanelGone = ref(true);
    const goneStyle: StyleValue = { visibility: "hidden" };

    let paneGoneTimer: number | undefined;

    const panelKind = ref<PanelKind | null>(null);

    const shownPanel = ref<PanelKind | null>(null);

    const activityTab = ref<ActivityTab>("mentions");

    const panelElement = ref<HTMLElement>();

    const sidebarElement = ref<HTMLElement>();

    const mainElement = ref<HTMLElement>();

    const mainViewElement = ref<HTMLElement>();

    const mainView = ref<MainViewKind | null>(null);

    const previewId = ref<string | null>(null);

    /** What had focus as the full view opened, which its close and a phone's Back return to. */
    let mainViewOpener: HTMLElement | null = null;

    let panelClearTimer: number | undefined;

    const drawnThread = shallowRef<OpenThreadState | null>(null);

    const threadOrigin = ref<ThreadOrigin>(null);
    const personOrigin = ref<PersonOrigin>(null);

    /**
     * The face or name the open card's person was opened from, which focus goes back to when the
     * profile in the column closes, by its X or with its card. Every card notes it, so a second
     * face opened while the column shows a profile is the one focus goes back to.
     */
    let personReturn: HTMLElement | null = null;

    /** The conversation the open thread belongs to, which the drawer stays open over. */
    let threadChannelId: string | null = null;

    /**
     * The part of a message in the conversation that opened the drawer, its pill, its "Replied to
     * a thread" label or its bar's Reply in thread, which the drawer's close gives focus back to.
     * Null when the thread came from a list, a mention, a link or a push, or the click left focus
     * where it was.
     */
    let threadOpener: HTMLElement | null = null;

    /**
     * Counts every thread opened or left, so a landing on a reply that awaited an older page can
     * tell the drawer it started in is gone and stop.
     */
    let threadOpenCount = 0;

    /**
     * The reply a link to this page named, or a push's tap made before sign-in, until the first
     * conversation opens: its thread opens over that conversation, and over no other the page load
     * falls back to.
     */
    let linkedReply = dependencies.linkedReply;

    // Made from the drawer's open and close below, which are hoisted, and read by them in turn.
    const landings = useLandings({ shell, activeChannelId, isPhone, openThreadDrawer, closeThreadDrawer, threadOpenCount: () => threadOpenCount });
    const { landingReplyId, landOnReply } = landings;

    /** The sidebar laid over the conversation on the Escape stack, under anything opened over it since. */
    const sidebarLayer: EscapeLayer = { takeEscape: () => onSidebarAway() };

    /** Where the last move asked focus to go, which the next settle puts it once the move is drawn. */
    let askedFocus: { order: FocusTarget[], options: FocusStopOptions } | null = null;

    /** The column focus was last in, as the frame's focusin notes it. */
    let focusRegion: FocusRegion | null = null;

    /** The settle due after the next draw, one however many changes ask for it. */
    let settling: Promise<void> | null = null;

    // #endregion Values

    // #region Computed Values

    /**
     * Which pane a phone shows: the conversation once one is open, or a full view in its place,
     * until Back asks for the list, and an open panel over the conversation.
     */
    const pane = computed((): FramePane => {
        // Activity is opened from the list's head, and slides in over the list.
        if (panelKind.value === "activity" && !mainView.value) {
            return "panel";
        }

        if (!(activeChannelId.value || mainView.value) || isListChosen.value) {
            return "list";
        }

        return panelKind.value && !mainView.value ? "panel" : "conversation";
    });

    /**
     * A panel open in a window too narrow for three columns folds the sidebar away, and the header
     * offers it back over the conversation; a phone has panes instead, and a full view no panel.
     */
    const sidebarFold = computed((): SidebarFold | null => {
        if (isPhone.value || !isNarrow.value || !panelKind.value || mainView.value) {
            return null;
        }

        return isSidebarOver.value ? "over" : "folded";
    });

    const isSidebarHidden = computed((): boolean => (isPhone.value && pane.value !== "list") || sidebarFold.value === "folded");

    /**
     * Members is offered where a room has a roster to show: a channel or a group, never a
     * conversation of two, which Rock keeps as it is. The header offers it by this, and the panel
     * opens and follows a conversation change by it too.
     */
    const isMembersOffered = computed((): boolean => {
        const channelId = activeChannelId.value;
        return !!channelId && shell.value.channels.roomKindOf(channelId) !== "person";
    });

    const shownList = computed((): ShownList | null => shownPanel.value === "activity" ? activityTab.value : shownPanel.value);

    const isConversationHidden = computed((): boolean => isPhone.value && pane.value !== "conversation");

    /** A closed panel is out of reach everywhere; an open one only while a phone shows another pane. */
    const isPanelHidden = computed((): boolean => !panelKind.value || (isPhone.value && pane.value !== "panel"));

    // The sidebar laid over the conversation is modal, as a sheet is: what it covers is out of reach.
    const isMainInert = computed((): boolean => isConversationHidden.value || sidebarFold.value === "over");
    const isPanelInert = computed((): boolean => isPanelHidden.value || sidebarFold.value === "over");

    // #endregion Computed Values

    // #region Event Handlers

    /** The block observed its own width, which alone decides whether it shows phone panes. */
    function onResize(entries: ResizeObserverEntry[]): void {
        const width = entries[entries.length - 1]?.contentRect.width;
        if (width !== undefined) {
            isPhone.value = width < phoneWidth;
            isNarrow.value = width < foldWidth();
        }
    }

    function onFocusIn(event: FocusEvent): void {
        const target = event.target as Node;
        const columns = [["panel", panelElement], ["main", mainElement], ["sidebar", sidebarElement]] as const;
        // focus in a dialog or popover over the frame is in none of them, and leaves the last noted
        focusRegion = columns.find(([, element]) => element.value?.contains(target))?.[0] ?? focusRegion;
    }

    function onFocusOut(event: FocusEvent): void {
        const target = event.target;
        // a control removed or put out of reach drops focus too, and that is owed a place; a part
        // still drawn that lets go for nowhere is the person clicking out to the page
        if (!(event.relatedTarget instanceof Element) && target instanceof HTMLElement && target.isConnected && isDrawn(target)) {
            focusRegion = null;
        }
    }

    async function onSelect(channelId: string): Promise<void> {
        // Choosing the conversation already open behind the list or a full view brings it back too.
        const isFolding = sidebarFold.value === "over";
        leaveMainView();
        isListChosen.value = false;
        isSidebarOver.value = false;
        isPaneMotionOn.value = true;
        void shell.value.selectChannel(channelId);

        // The row that had focus has slid out of view and out of reach, on a phone or with the
        // sidebar folding away, so focus goes to what it opened: the conversation's title, never
        // its composer, which would raise a phone's keyboard. A phone's pane is still off to the
        // right as it starts to slide, so the focus must not scroll the frame to it, which a
        // browser that cannot clip the frame would do.
        if (isPhone.value || isFolding) {
            await askFocus(["header"], { preventScroll: true });
        }
    }

    async function onBack(): Promise<void> {
        const leftChannelId = activeChannelId.value;
        isListChosen.value = true;
        isPaneMotionOn.value = true;

        // Back on a phone hides the button that had focus, so focus returns to the row of the
        // conversation just left, where the person was in the list, or with none to the New
        // message button. A full view stays behind the list, as the prototype keeps it, and focus
        // goes to what opened it.
        if (isPhone.value) {
            await askFocus([
                mainView.value ? mainViewOpener : null,
                leftChannelId ? { stop: "row", match: `[data-channel-id='${leftChannelId}']` } : null,
                "new-message"
            ]);
        }
    }

    /**
     * The header's Threads opens the open conversation's threads and Members its roster; the
     * sidebar's Activity opens the inbox across conversations. Each closes its list when it is the
     * one open.
     */
    async function onListPanel(kind: ListPanelKind): Promise<void> {
        if (panelKind.value === kind) {
            closePanel();
            return;
        }

        // A mention waiting is what Activity is opened for, so it opens on Mentions; else where it was.
        await openPanel(kind, kind === "activity" && shell.value.mentions.state.hasDot ? "mentions" : activityTab.value);
    }

    /** Opens a list in the column, Activity on the tab given, with focus on its first control. */
    async function openPanel(kind: ListPanelKind, tab: ActivityTab): Promise<void> {
        if ((kind !== "activity" && !activeChannelId.value) || (kind === "members" && !isMembersOffered.value)) {
            return;
        }

        // The column holds one panel at a time, so the list takes an open thread's or list's place.
        releasePanel();
        window.clearTimeout(panelClearTimer);
        isPaneMotionOn.value = true;
        if (kind === "activity") {
            activityTab.value = tab;
        }
        panelKind.value = kind;
        shownPanel.value = kind;
        const opened = openList(kind);
        // The column may still be 0px wide, so a focus that scrolled would drag the content in
        // from its edge; a back chevron or X takes focus without its tooltip, as nobody reached for it.
        await askFocus(["panel-start"], { preventScroll: true, isQuiet: true });

        // A first control that comes with the list's first page, as a phone's first member, takes
        // focus once that page is drawn, while focus is still on the back chevron or X the panel
        // started on without it.
        const start = document.activeElement;
        if (start instanceof HTMLElement && panelElement.value?.contains(start) && !start.hasAttribute("data-autofocus")) {
            void opened.then(() => nextTick()).then(() => {
                if (document.activeElement === start) {
                    firstDrawn(panelElement.value, "[data-autofocus]")?.focus({ preventScroll: true });
                }
            });
        }
    }

    /** Activity's Segmented chose a tab: its list takes the other's place. */
    function onActivityTab(tab: ActivityTab): void {
        if (panelKind.value !== "activity" || tab === activityTab.value) {
            return;
        }

        releasePanel();
        activityTab.value = tab;
        void openList("activity");
    }

    /** The panel fading out is out of reach and unread while the next fades in over its place. */
    function onPanelLeave(element: Element): void {
        element.setAttribute("inert", "");
        element.setAttribute("aria-hidden", "true");
    }

    /** The conversation's empty threads list asked for every thread followed, which Activity's Threads lists. */
    function onThreadsTab(tab: ThreadListTab): void {
        if (tab === "all") {
            void openPanel("activity", "threads");
        }
    }

    /** The header's sidebar button, shown while the sidebar is folded: it opens over the conversation, with focus in it. */
    function onSidebarOver(): void {
        isSidebarOver.value = true;
        void askFocus([activeRow(), "new-message"]);
    }

    /**
     * The sidebar laid over the conversation goes back out of the way, as a sheet does: its own
     * button, Escape or a press on the shade over what it covers, focus going back to the
     * header's button, which brings it back.
     */
    function onSidebarAway(): void {
        isSidebarOver.value = false;
        void askFocus(["sidebar-button", "header"]);
    }

    /**
     * A row of a threads list chose its thread: its conversation opens behind the drawer, as a
     * sidebar row would open it, and the thread opens in the panel column in the list's place,
     * with a back chevron that returns to the list and the row that opened it.
     */
    function openThreadFromList(rootId: number, channelId: string): void {
        if (channelId !== activeChannelId.value) {
            void shell.value.selectChannel(channelId);
        }
        // Activity's list is opened from a phone's list, and the thread is over its conversation.
        isListChosen.value = false;
        void openThreadDrawer(channelId, rootId, panelKind.value === "activity" ? "activity" : "threads");
    }

    /**
     * A message's thread, from its thread pill, the "Replied to a thread" label over a reply or its
     * Reply in thread, in the open conversation: at the reply when the message is one.
     */
    function onOpenThread(place: ThreadPlace): void {
        if (activeChannelId.value) {
            void landOnReply({ channelId: activeChannelId.value, ...place });
        }
    }

    /** Back to the list the thread was opened from, with focus on the row that opened it. */
    async function onThreadBack(): Promise<void> {
        const rootId = drawnThread.value?.rootId;
        const origin = threadOrigin.value ?? "threads";
        leaveThread();
        panelKind.value = origin;
        shownPanel.value = origin;

        // The inbox marks the row opened as its current one.
        await askFocus([shownList.value === "threads"
            ? { stop: "threads-row", match: `[data-root-id='${rootId}']` }
            : { stop: "mentions-row", match: "[aria-current='true']" }], { within: "panel" });
    }

    /**
     * A row of the mentions inbox: the shell's jump marks it opened and lands on its message, in its
     * conversation beside the inbox, or in its thread with a way back. A phone shows one pane, so a
     * channel mention closes the inbox to bring its conversation forward.
     */
    function onMentionOpen(messageId: number): void {
        const row = shell.value.mentions.state.inbox.rows.find(r => r.message.id === messageId);
        if (!row) {
            return;
        }

        void shell.value.mentions.jump(row);
        if (row.parent_id === null && isPhone.value) {
            isListChosen.value = false;
            closePanel();
        }
    }

    /**
     * Opens a full view in the conversation's place, closing the panel column, since its buttons
     * sit in the header the view covers. The view open behind a phone's list comes back as it was.
     * Focus goes to the view's first field, except on a phone, where it would raise the keyboard;
     * New message's To field is the exception, since choosing people is all there is to do, and
     * the button that opened the view has slid away with the list.
     */
    async function openMainView(kind: MainViewKind, people?: PersonRow[]): Promise<void> {
        isListChosen.value = false;
        isPaneMotionOn.value = true;
        if (mainView.value !== kind) {
            // One full view taking another's place returns, when it closes, to what opened the first.
            const active = document.activeElement;
            const opener = mainView.value
                ? mainViewOpener
                : active instanceof HTMLElement && rootElement.value?.contains(active) ? active : null;
            leaveMainView();
            mainViewOpener = opener;
            if (panelKind.value) {
                endPanel();
            }
            mainView.value = kind;
            if (kind === "directory") {
                shell.value.rooms.openDirectory();
            }
            else {
                shell.value.directMessages.begin(people);
            }
        }
        else if (kind === "newdm" && people) {
            // A profile card's Message over New message begins it again with its person.
            shell.value.directMessages.begin(people);
        }

        // The view is still rising in, and on a phone its pane still sliding from the right, so
        // the focus leaves what holds it unscrolled; the first field is at the view's top.
        if (!isPhone.value || kind === "newdm") {
            await askFocus(["view-start"], { preventScroll: true });
        }
    }

    /**
     * Closes the full view. Focus goes to the composer of the conversation it gave back, or on a
     * phone or with none, to what opened the view, else the conversation's title. A phone that
     * gives a conversation back shows it in place of the list, so what opened the view is out of
     * reach there and focus goes to the conversation's title.
     */
    function closeMainView(): void {
        if (!mainView.value) {
            return;
        }

        const opener = mainViewOpener;
        leaveMainView();
        void askFocus(["composer", isPhone.value && pane.value !== "list" ? null : opener, "header"], { within: "main" });
    }

    /**
     * Opens a room chosen in the full view. One with no sidebar row is previewed before it opens,
     * so its row and its join bar are there at once; the view closes even when the room is the
     * one already underneath. Focus lands on Join, the preview's one action, else on the composer,
     * or on a phone the title, since a focused composer would raise the keyboard.
     */
    async function openFromMainView(channelId: string): Promise<void> {
        shell.value.rooms.previewRoom(channelId);
        const isPreviewed = mainView.value === "directory";
        leaveMainView(true);
        isListChosen.value = false;
        isPaneMotionOn.value = true;
        const opening = shell.value.selectChannel(channelId);

        // Let go only once the room is on screen: the preview's end keeps that room's page, so the
        // open draws what the person just read while its own read is out.
        if (isPreviewed) {
            onPreview(null);
        }

        await opening;
        await askFocus(["join", "composer", "header"], { within: "main" });
    }

    /**
     * Browse previews a room beside its list: the frame holds it apart from the room on screen,
     * and the shell reads its history for the preview's feed. None lets the last one go.
     */
    function onPreview(channelId: string | null): void {
        previewId.value = channelId;
        void shell.value.readPreview(channelId);
    }

    /** Join from Browse's preview; the watch below reads its history again once the join lands. */
    async function onPreviewJoin(channelId: string): Promise<void> {
        await shell.value.rooms.join(channelId);
    }

    // #endregion Event Handlers

    // #region Functions

    /**
     * Add people for a room. Rock keeps a conversation of two as it is, so one never opens there,
     * whatever asks.
     */
    function openAddPeople(channelId: string): void {
        if (shell.value.channels.roomKindOf(channelId) === "person") {
            return;
        }

        shell.value.members.openAdd();
    }

    /**
     * Lets go of the full view, without moving focus. Open holds Browse's preview to let it go
     * itself, once the room it opens is on screen.
     */
    function leaveMainView(isPreviewHeld: boolean = false): void {
        const kind = mainView.value;
        if (!kind) {
            return;
        }

        mainView.value = null;
        mainViewOpener = null;
        if (kind === "directory") {
            if (!isPreviewHeld) {
                onPreview(null);
            }
            shell.value.rooms.closeDirectory();
        }
        else {
            // A first message on its way carries on alone; the people chosen and the draft go.
            shell.value.directMessages.close();
        }
    }

    /** Opens a mention's thread at the reply over its conversation, with a way back to the inbox. */
    async function openThreadFromMentions(channelId: string, rootId: number, messageId: number): Promise<void> {
        if (channelId !== activeChannelId.value) {
            void shell.value.selectChannel(channelId);
        }
        isListChosen.value = false;
        await landOnReply({ channelId, rootId, messageId }, "activity");
    }

    /**
     * A push's tap brings what it was for to the front, even when its conversation is already the
     * one open behind a phone's list: the thread for a reply, else the conversation, which a
     * phone's panel would cover. A desktop shows a panel beside it, so keeps it.
     */
    function openFromTap(channelId: string, rootId: number | null, messageId: number | null): void {
        // Before chat has signed in nothing can be read, so the tap is a link that arrived late: its
        // room is the one the page load opens, and its reply, or none for a room's tap, the reply
        // that opens over it, where a link's own are held. The last tap wins.
        if (shell.value.state.phase === "starting") {
            shell.value.relink(channelId);
            linkedReply = rootId !== null ? { channelId, rootId, messageId } : null;
            return;
        }

        const isOpenConversation = channelId === activeChannelId.value;
        isListChosen.value = false;
        isPaneMotionOn.value = true;
        void shell.value.selectChannel(channelId);
        if (rootId !== null) {
            void landOnReply({ channelId, rootId, messageId });
        }
        else if (isPhone.value && panelKind.value === "thread") {
            // Only the open conversation's thread is closed here, so focus never aims at a root
            // in a feed about to be replaced, however late the switch lands; another
            // conversation's arrival closes the thread itself and moves focus.
            if (isOpenConversation) {
                closeThreadDrawer();
            }
        }
        else if (isPhone.value) {
            closePanel();
        }
    }

    /**
     * Asks the shell for a list the column now shows: Activity's by its tab, else the open
     * conversation's. Resolves once its first page is in, or with nothing to ask for at once; the
     * shell says a failure itself.
     */
    function openList(kind: ListPanelKind): Promise<void> {
        const channelId = activeChannelId.value;
        if (kind === "activity") {
            return activityTab.value === "mentions" ? shell.value.mentions.openInbox() : shell.value.threads.openList(channelId ?? "", "all");
        }

        if (channelId) {
            return kind === "threads" ? shell.value.threads.openList(channelId, "channel") : shell.value.members.open(channelId);
        }

        return Promise.resolve();
    }

    /**
     * Lets go of whatever the column holds: an open thread, and the list it was opened from or the
     * list open. The sidebar is laid over the conversation only for the panel it made room for,
     * so it folds again with that panel, whichever way the panel goes.
     */
    function releasePanel(): void {
        const kind = panelKind.value === "thread" ? threadOrigin.value : panelKind.value === "person" ? personOrigin.value : panelKind.value;
        // The profile in the column is the open profile, so it closes with the column.
        if (panelKind.value === "person") {
            personOrigin.value = null;
            shell.value.profile.closeCard();
        }
        const list = kind === "activity" ? activityTab.value : kind;
        isSidebarOver.value = false;
        leaveThread();
        if (list === "threads") {
            shell.value.threads.closeList();
        }
        else if (list === "mentions") {
            shell.value.mentions.closeInbox();
        }
        else if (list === "members") {
            shell.value.members.close();
        }
    }

    /** The width under which a panel folds the sidebar: the sidebar, the panel and the conversation's least width, as the sheet names them. */
    function foldWidth(): number {
        const root = rootElement.value;
        return root
            ? tokenLength(root, "--chat-w-side", fallbackSidePx) + tokenLength(root, "--chat-w-panel", fallbackPanelPx) + tokenLength(root, "--chat-w-main-min", fallbackMainMinPx)
            : fallbackSidePx + fallbackPanelPx + fallbackMainMinPx;
    }

    /**
     * Closes the panel column. Focus returns to the header button that opened it, or on a phone
     * to the overflow button it was chosen from, else the conversation's title.
     */
    function closePanel(returnTo: HTMLElement | null = null): void {
        if (!panelKind.value) {
            return;
        }

        const closed = panelKind.value;
        const isFromMembers = closed === "person" && personOrigin.value === "members";
        endPanel();

        // A thread or a profile is opened from a message or a list, never from a header button of
        // its own. A profile opened from Members took the list's place, so the row that opened it
        // went with the list and the header's Members button is the way back to it.
        const button: FocusStop | null = isFromMembers ? "members-button" : closed === "thread" || closed === "person" ? null : `${closed}-button`;
        void askFocus([returnTo?.isConnected ? returnTo : null, button, "more", "header"]);
    }

    function notePersonOpener(opener: HTMLElement): void {
        personReturn = opener;
    }

    function openPersonPanel(): void {
        const origin: PersonOrigin = panelKind.value === "members" ? "members" : null;
        // A thread or another list gives the profile its place; the members list stays to go back to.
        if (origin === null && panelKind.value !== "person") {
            releasePanel();
        }

        window.clearTimeout(panelClearTimer);
        isPaneMotionOn.value = true;
        personOrigin.value = origin;
        panelKind.value = "person";
        shownPanel.value = "person";
        void askFocus(["panel-start"], { within: "panel", preventScroll: true });
    }

    function closePersonPanel(): void {
        closePanel(personReturn);
    }

    async function onPersonBack(): Promise<void> {
        const personAliasGuid = shell.value.profile.state.card?.personAliasGuid ?? "";
        const origin = personOrigin.value;
        personOrigin.value = null;
        shell.value.profile.closeCard();
        if (origin !== "members") {
            closePanel();
            return;
        }

        panelKind.value = origin;
        shownPanel.value = origin;
        await askFocus([{ stop: "member", match: `[data-person='${personAliasGuid}']` }, "panel-start"], { within: "panel" });
    }

    /** Closes the panel column, which draws what it held while it narrows, without moving focus. */
    function endPanel(): void {
        releasePanel();
        panelKind.value = null;
        finishClosing();
    }

    /**
     * Opens a thread in the drawer. One opened from the conversation replaces whatever the column
     * held; one opened from a list keeps the list to go back to. Its mentions stop lighting the
     * header's dot, as the thread is where they are read. The thread drawn already is where the
     * reader is, so nothing about the column moves and only the page on its way is waited on; a
     * landing it carries is the landing's to make.
     */
    async function openThreadDrawer(channelId: string, rootId: number, origin: ThreadOrigin): Promise<void> {
        // Read before anything moves focus into the drawer: a message's part in the conversation
        // is where the reader was, and the newest one pressed is where they come back to.
        const active = document.activeElement;
        const opener = active instanceof HTMLElement && !!mainElement.value?.contains(active) && !!active.closest("[data-message-id]") ? active : null;

        if (panelKind.value === "thread" && shell.value.threads.isOpen(channelId, rootId)) {
            threadOpener = opener ?? threadOpener;
            return shell.value.threads.openThread(channelId, rootId);
        }

        threadOpener = opener;

        if (origin === null && (panelKind.value === "threads" || panelKind.value === "activity" || panelKind.value === "members")) {
            releasePanel();
        }
        shell.value.mentions.threadOpened(channelId, rootId);

        threadOpenCount++;
        // A thread opened afresh lands nothing old.
        landingReplyId.value = null;
        window.clearTimeout(panelClearTimer);
        isPaneMotionOn.value = true;
        threadOrigin.value = origin;
        threadChannelId = channelId;
        panelKind.value = "thread";
        shownPanel.value = "thread";
        // A phone skips the composer, whose keyboard would cover the thread, for the back chevron.
        void askFocus(["composer", "panel-back"], { within: "panel", preventScroll: true });
        await shell.value.threads.openThread(channelId, rootId);
    }

    /** Lets go of the open thread, which saves where the reader got to, without moving focus. */
    function leaveThread(): void {
        if (panelKind.value === "thread") {
            threadOpenCount++;
            panelKind.value = null;
            void shell.value.threads.closeThread();
        }
    }

    /**
     * Closes the drawer. Focus returns to what in the conversation opened it, else the root's
     * thread pill, else the root message, else the conversation's title. The conversation stays
     * where the reader left it: a reply also sent to it can sit a long way below its root, and
     * the focus coming back never scrolls the feed to reach the root.
     */
    function closeThreadDrawer(): void {
        const rootId = drawnThread.value?.rootId;
        const opener = threadOpener;
        threadOpener = null;
        releasePanel();
        finishClosing();

        const message = mainElement.value?.querySelector<HTMLElement>(`[data-message-id='${rootId}']`);
        void askFocus([opener, message ? { stop: "thread-summary", within: message } : null, message, "header"], { within: "main", preventScroll: true });
    }

    /** Keeps what the column drew until it has finished narrowing, then lets it go. */
    function finishClosing(): void {
        window.clearTimeout(panelClearTimer);
        panelClearTimer = window.setTimeout(() => {
            shownPanel.value = panelKind.value ? shownPanel.value : null;
        }, panelSlideMs());
    }

    /** How long a pane slides: none for a move the person did not make, or when they ask for reduced motion. */
    function paneSlideMs(): number {
        if (!isPaneMotionOn.value || isMotionReduced() || !rootElement.value) {
            return 0;
        }

        return tokenDuration(rootElement.value, "--chat-t-panel", fallbackPaneSlideMs);
    }

    /** How long the panel column takes to close, which its content stays drawn through. */
    function panelSlideMs(): number {
        if (isMotionReduced() || !rootElement.value) {
            return 0;
        }

        return tokenDuration(rootElement.value, "--chat-t-panel", fallbackPaneSlideMs);
    }

    /**
     * Puts focus on the first place of an order that is drawn. Each lookup keeps its own order,
     * since what is the next best place differs by what just closed or opened.
     *
     * @param order The places to try, first to last.
     * @param options Where the stops are looked for and how focus is put there.
     *
     * @returns The element focused, or null when nothing in the order is drawn.
     */
    function focusStop(order: FocusTarget[], options: FocusStopOptions = {}): HTMLElement | null {
        const place = options.within === "main" ? mainElement.value : options.within === "panel" ? panelElement.value : rootElement.value;
        for (const target of order) {
            // A place gone from the page or in a pane out of reach takes no focus, so the order goes on to the next.
            const element = target instanceof HTMLElement ? (target.isConnected && isDrawn(target) ? target : null) : stopElement(target, place, options);
            if (!element) {
                continue;
            }

            // with no scroll to hold, the focus is the plain one every other part of the chat makes
            const focusOptions = options.preventScroll ? { preventScroll: true } : undefined;
            if (options.isQuiet) {
                focusQuietly(element, focusOptions);
            }
            else {
                element.focus(focusOptions);
            }
            return element;
        }

        return null;
    }

    /**
     * The element a stop's mark is on, or the first button inside it when the mark is on what
     * wraps a part that draws its own button, as More's menu does.
     */
    function stopElement(target: Exclude<FocusTarget, HTMLElement>, place: HTMLElement | undefined, options: FocusStopOptions): HTMLElement | null {
        if (!target) {
            return null;
        }

        const { stop, match, within } = typeof target === "string" ? { stop: target, match: undefined, within: undefined } : target;
        if (stop === "composer" && isPhone.value && !options.isTyping) {
            return null;
        }

        // A panel's start is its first control, else its back chevron, drawn before its X, else
        // its X; a full view's is its first field. A part fading out is out of reach, so only
        // the one coming in is looked in.
        if (stop === "panel-start") {
            return firstDrawn(panelElement.value, "[data-autofocus]") ?? firstDrawn(panelElement.value, "[data-focus-stop='panel-back'], [data-focus-stop='panel-close']");
        }

        if (stop === "view-start") {
            return firstDrawn(mainViewElement.value, "[data-autofocus]");
        }

        const marked = firstDrawn(within ?? place, `[data-focus-stop='${stop}']${match ?? ""}`);
        if (!marked || marked.matches("button, input, textarea, select, a[href], [tabindex], [contenteditable]")) {
            return marked;
        }

        return marked.querySelector<HTMLElement>("button") ?? marked;
    }

    /** The sidebar's row of the open conversation, as a place for focus. */
    function activeRow(): FocusTarget {
        return activeChannelId.value ? { stop: "row", match: `[data-channel-id='${activeChannelId.value}']` } : null;
    }

    /**
     * Asks for focus at the first drawn place of an order, put there by the settle that follows
     * the move, once it is drawn. A move asks after it has changed what the frame shows.
     *
     * @returns Resolves once focus has been put.
     */
    function askFocus(order: FocusTarget[], options: FocusStopOptions = {}): Promise<void> {
        askedFocus = { order, options };
        return scheduleSettle();
    }

    /** Settles focus after the next draw, once however many changes ask before it. */
    function scheduleSettle(): Promise<void> {
        settling ??= nextTick(() => {
            settling = null;
            settleFocus();
        });
        return settling;
    }

    /**
     * Puts focus where a change of what the frame shows leaves it, once drawn, when every part
     * fading out has been put out of reach: where the move asked, else where it is while that is
     * still drawn, in the chat or out of it, else the start of the column that last held it,
     * or the conversation's title when that column has closed.
     */
    function settleFocus(): void {
        const asked = askedFocus;
        askedFocus = null;
        if (asked && focusStop(asked.order, asked.options)) {
            return;
        }

        const active = document.activeElement;
        const isLost = !(active instanceof HTMLElement) || active === document.body || (!!rootElement.value?.contains(active) && !isDrawn(active));
        if (!isLost || !focusRegion) {
            return;
        }

        const start: FocusTarget[] = focusRegion === "panel" ? ["panel-start"] : focusRegion === "sidebar" ? [activeRow(), "new-message"] : [];
        focusStop([...start, "header"], { preventScroll: true, isQuiet: true });
    }

    // #endregion Functions

    // #region Watchers

    // A pane coming into view is reachable at once; one leaving is hidden after it has slid away.
    watch([isSidebarHidden, isConversationHidden, isPanelHidden], () => {
        window.clearTimeout(paneGoneTimer);
        isSidebarGone.value &&= isSidebarHidden.value;
        isConversationGone.value &&= isConversationHidden.value;
        isPanelGone.value &&= isPanelHidden.value;

        const finish = (): void => {
            isSidebarGone.value = isSidebarHidden.value;
            isConversationGone.value = isConversationHidden.value;
            isPanelGone.value = isPanelHidden.value;
        };

        const slideMs = paneSlideMs();
        if (slideMs === 0) {
            finish();
        }
        else {
            paneGoneTimer = window.setTimeout(finish, slideMs);
        }
    });

    // The push worker's asks are answered by the shell, which cannot see the panes: a thread is
    // out of view unless the panel shows it, and a phone shows only one pane.
    watchEffect(() => {
        shell.value.state.hiddenPanes = {
            // a full view covers the conversation wherever it is drawn
            conversation: isConversationHidden.value || !!mainView.value,
            thread: isPanelHidden.value || panelKind.value !== "thread"
        };
    });

    // Any conversation that opens, from a row, a push or a first message, comes forward on a phone,
    // and takes the place of a full view. A New message draft is drawn inside its view, so it
    // coming or going changes nothing here.
    watch(activeChannelId, () => {
        isListChosen.value = false;
        leaveMainView();
    });

    // The profile in the column is the open card's person, so it follows the card: a card let go
    // of, by another room, a read that failed or a person the platform does not list, ends the
    // column, and with it the members list it was opened from. One rule for every way a card goes,
    // so nothing leaves the column standing empty.
    watch(() => shell.value.profile.state.card, card => {
        if (!card && panelKind.value === "person") {
            closePanel(personReturn);
        }
    });

    // A join of the previewed room that lands, at once or once a pending join is taken, reads its
    // history again, since the room's topic is not the one joined: the door's line, and what a
    // member may read, come with it. Moving the preview to a room joined earlier reads nothing more.
    watch(() => ({ channelId: previewId.value, kind: previewId.value ? shell.value.rooms.joinOf(previewId.value).kind : null }), (now, was) => {
        if (now.channelId && now.channelId === was.channelId && now.kind === "joined" && was.kind !== "joined") {
            void shell.value.readPreview(now.channelId);
        }
    });

    // The drawer keeps the thread it last drew, so a closing drawer still has its content.
    watch(() => shell.value.threads.state.thread, thread => {
        if (thread) {
            drawnThread.value = thread;
        }
    });

    // A link to a reply opens its thread once the conversation it names is the first to open; a
    // page load that fell back to another conversation, or a person who chose one first, drops it.
    // Made before the watch below, which would otherwise close the thread this opens.
    watch(activeChannelId, channelId => {
        const landing = linkedReply;
        if (!landing || !channelId) {
            return;
        }

        linkedReply = null;
        if (channelId === landing.channelId) {
            void landOnReply(landing);
        }
    });

    // A thread belongs to its own conversation, so another conversation closes it. A
    // thread opened from the list switches to its conversation first, which keeps it open.
    watch(activeChannelId, channelId => {
        if (panelKind.value === "thread" && channelId !== threadChannelId) {
            leaveThread();
            finishClosing();
        }
    });

    // The threads list and the roster are the open conversation's, so each follows it, re-pointing
    // at the conversation now open; a draft has neither, and a conversation of two no roster, so
    // they close there, as the header offers them. Activity spans every conversation, so it stays
    // as it is.
    watch(activeChannelId, channelId => {
        const kind = panelKind.value;
        if (kind !== "threads" && kind !== "members") {
            return;
        }

        if (!channelId || (kind === "members" && !isMembersOffered.value)) {
            closePanel();
            return;
        }

        void openList(kind);
    });

    // Focus is settled after every change of what the frame shows, the conversation included: a
    // closing drawer or a list read afresh for another conversation takes focus away with it.
    watch([pane, sidebarFold, mainView, panelKind, shownList, () => drawnThread.value?.rootId, activeChannelId], () => void scheduleSettle());

    // Laid over the conversation, the sidebar is on the Escape stack until it folds or takes its
    // column back; the shade the frame draws under it answers a press.
    watch(() => sidebarFold.value === "over", isOver => {
        if (isOver) {
            pushEscapeLayer(sidebarLayer);
        }
        else {
            pullEscapeLayer(sidebarLayer);
        }
    });

    // #endregion Watchers

    onBeforeUnmount(() => {
        window.clearTimeout(paneGoneTimer);
        window.clearTimeout(panelClearTimer);
        pullEscapeLayer(sidebarLayer);
    });

    const rooms = useRoomsView({ shell, activeChannelId, drawnThread });

    return {
        isPhone,
        isPaneMotionOn,
        pane,
        isSidebarHidden,
        isMainInert,
        isPanelHidden,
        isPanelInert,
        isSidebarGone,
        isConversationGone,
        isPanelGone,
        goneStyle,
        panelKind,
        mainView,
        previewId,
        shownPanel,
        activityTab,
        shownList,
        sidebarFold,
        isMembersOffered,
        panelElement,
        sidebarElement,
        mainElement,
        mainViewElement,
        drawnThread,
        threadOrigin,
        personOrigin,
        rooms,
        onResize,
        onFocusIn,
        onFocusOut,
        onSelect,
        onBack,
        onListPanel,
        onActivityTab,
        onPanelLeave,
        onSidebarOver,
        onSidebarAway,
        onThreadsTab,
        openThreadFromList,
        onOpenThread,
        onThreadBack,
        onMentionOpen,
        openThreadFromMentions,
        openFromTap,
        openThreadDrawer,
        closeThreadDrawer,
        closePanel,
        notePersonOpener,
        openPersonPanel,
        closePersonPanel,
        onPersonBack,
        openMainView,
        closeMainView,
        openFromMainView,
        onPreview,
        onPreviewJoin,
        openAddPeople,
        threadOpenCount: () => threadOpenCount,
        focusStop,
        ...landings,
        ...membersPanelViews(shell, rooms, dependencies.campuses),
        ...threadsPanelWiring(shell, activeChannelId, drawnThread, dependencies.showNotice)
    };
}

/**
 * The first element in a place that matches and is drawn for the keyboard, past any in a pane out
 * of reach, as a phone's panes behind the front one and a panel fading out are.
 *
 * @param place Where to look.
 * @param selector What to look for.
 *
 * @returns The element, or null when none is drawn.
 */
function firstDrawn(place: ParentNode | undefined, selector: string): HTMLElement | null {
    return Array.from(place?.querySelectorAll<HTMLElement>(selector) ?? []).find(isDrawn) ?? null;
}

/**
 * Works out what the members panel, the remove confirm and Add people draw. The two dialogs that
 * change who is in the room stand over the frame rather than in the panel, but all three read the
 * same roster and the same room, by its title, its kind and its head count.
 *
 * @param shell The block's shell, read on every draw since a birthdate replaces it.
 * @param rooms What the frame draws of rooms, for the room's title and kind.
 * @param campuses The church's campuses, for a person's campus in Add people's search.
 *
 * @returns Each part's props.
 */
function membersPanelViews(shell: Readonly<Ref<ChatShell>>, rooms: Pick<ReturnType<typeof useRoomsView>, "roomTitles" | "roomKinds">, campuses: Array<{ guid: string, name?: string | null }>) {
    const { roomTitles, roomKinds } = rooms;

    /** The church's campus names by guid in lower case, for a person's campus in Add people's search. */
    const campusNames: Record<string, string> = Object.fromEntries(campuses.map(campus => [campus.guid.toLowerCase(), campus.name ?? ""]));

    /** The room the roster is of: its name, whether it is a channel, and its head count once its facts are read. */
    const room = computed(() => {
        const channelId = shell.value.members.state.channelId ?? "";
        return {
            channelName: roomTitles.value[channelId] ?? "",
            isChannel: roomKinds.value[channelId] === "channel",
            memberCount: shell.value.rooms.factsOf(channelId)?.member_count ?? null
        };
    });

    const membersView = computed(() => {
        const members = shell.value.members.state;
        const isQuery = members.query.trim() !== "";
        return {
            ...room.value,
            members: shownMembers(members),
            hasMore: isQuery ? members.search.hasMore : members.hasMore,
            // The list never waits on Rock: only Add people's place is held until it answers.
            isLoading: members.isLoading,
            isAddPending: members.isReadingRights,
            isLoadingMore: members.isLoadingMore,
            error: members.error,
            moreError: members.moreError,
            refusal: members.refusal,
            query: members.query,
            searchStatus: searchStatus(members),
            canManageMembers: members.canManageMembers,
            isChatAdministrator: members.isChatAdministrator,
            removals: members.removals
        };
    });

    const removeView = computed(() => {
        const members = shell.value.members.state;
        const confirming = members.confirming;
        return {
            channelName: room.value.channelName,
            isChannel: room.value.isChannel,
            member: confirming,
            isLastLeader: !!confirming && members.isSuccessorAsked,
            choices: successorChoices(members),
            query: members.successorQuery,
            refusal: members.successorRefusal,
            isRemoving: !!confirming && members.removals[confirming.person_alias_guid]?.kind === "removing"
        };
    });

    const addView = computed(() => {
        const adding = shell.value.members.state.adding;
        return {
            ...room.value,
            campusNames,
            isOpen: adding.isOpen,
            query: adding.query,
            people: adding.people,
            isSearching: adding.isSearching,
            error: adding.error,
            isAdding: adding.isAdding,
            refusal: adding.refusal
        };
    });

    return { membersView, removeView, addView };
}

/**
 * The threads list and the thread drawer's wiring to the shell: the list's tab, refresh and
 * paging, a follow change from either and where its refusal is said, and the drawer's older
 * replies. Which panel shows is the frame's; this is what the two panels ask of the shell.
 *
 * @param shell The block's shell, read on every use since a birthdate replaces it.
 * @param activeChannelId The open conversation.
 * @param drawnThread The thread the drawer draws, whose follow change and replies are the drawer's.
 * @param showNotice Shows a toast that is not a failure, one per code.
 *
 * @returns The panels' values and handlers.
 */
function threadsPanelWiring(shell: Readonly<Ref<ChatShell>>, activeChannelId: Readonly<Ref<string | null>>, drawnThread: Readonly<Ref<OpenThreadState | null>>, showNotice: (notice: ToastNotice) => void) {
    /** The threads list the panel draws, as the shell's threads hold it. */
    const threadList = computed((): ThreadListState => shell.value.threads.state.list);

    /**
     * A refused follow change is said once: by the drawer while it holds that thread, the drawer
     * it went back from included, and otherwise by the list's row.
     */
    const drawerFollowError = computed((): FollowError | null => {
        const error = shell.value.threads.state.followError;
        return error && error.rootId === drawnThread.value?.rootId ? error : null;
    });
    const listFollowError = computed((): FollowError | null => drawerFollowError.value ? null : shell.value.threads.state.followError);

    function onThreadsRefresh(): void {
        void shell.value.threads.refresh();
    }

    /** The list's loader, whose promise the list's paging waits on. */
    function loadMoreThreads(): Promise<void> {
        return shell.value.threads.loadMore();
    }

    /**
     * A follow change from a row or the drawer: the bell changes at once and a refusal comes back
     * beside it. Following says nothing more; an unfollow that lands says what it means.
     */
    async function onFollowFromList(rootId: number, isFollowing: boolean): Promise<void> {
        if (!await shell.value.threads.follow(rootId, isFollowing) || isFollowing) {
            return;
        }

        showNotice(unfollowNotice);
    }

    /** The drawer's toggle, saved as the list's is. */
    function onFollowThread(isFollowing: boolean): void {
        if (drawnThread.value) {
            void onFollowFromList(drawnThread.value.rootId, isFollowing);
        }
    }

    return { threadList, drawerFollowError, listFollowError, onThreadsRefresh, loadMoreThreads, onFollowFromList, onFollowThread };
}
