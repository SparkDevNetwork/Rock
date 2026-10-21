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
// What each column of the block's frame shows: the panel column's one panel at a time (the
// threads list, the mentions inbox or a thread in its drawer), the way back from a thread to the
// list it came from, focus handed back to what opened a panel, the main column's full views (such
// as Browse channels) that take the conversation's place, and on a phone which of the three
// panes is in front. The block keeps the template and the wiring; this keeps the rules, so the
// phone's pane is worked out in one place from everything that can take a column.
//
// The landings on a message are useJump's, which takes this frame's drawer open and close, so
// the frame takes its one landing back lazily, called only once the block is set up.

import { computed, ComputedRef, nextTick, onBeforeUnmount, ref, Ref, shallowRef, ShallowRef, StyleValue, watch, watchEffect } from "vue";
import { ChatShell } from "../shell.partial";
import { PersonRow } from "../types.partial";
import { tokenDuration } from "../ui/brand.partial";
import { focusQuietly } from "../ui/tooltip.partial";
import { ReplyLanding, ThreadOrigin } from "./useJump.partial";
import { OpenThreadState } from "./useThreads.partial";

/** What the panel column can hold: the threads list, the mentions inbox, or one thread in its drawer. */
export type PanelKind = "threads" | "mentions" | "thread";

/** A view that takes the conversation's place in the main column: Browse channels or New message. */
export type MainViewKind = "directory" | "newdm";

/** Which pane a phone shows in front. */
export type FramePane = "list" | "conversation" | "panel";

/** What the frame reads from the block. */
export type FrameDependencies = {
    /** The block's shell, read on every use since a birthdate replaces it. */
    shell: Readonly<Ref<ChatShell>>;

    activeChannelId: Readonly<Ref<string | null>>;

    /** The block's root element, which focus is looked for in and the motion tokens are read from. */
    rootElement: Readonly<Ref<HTMLElement | undefined>>;

    /** The reply a link to this page named, which opens its thread over that conversation once it is the first to open. */
    linkedReply: ReplyLanding | null;

    /** Opens a thread at a reply, through the block's landings. */
    landOnReply: (landing: ReplyLanding, origin?: ThreadOrigin) => Promise<void>;

    /** Lets go of the reply the drawer last landed on, so a thread opened afresh lands nothing old. */
    forgetReplyLanding: () => void;
};

/** What the template draws of the frame, and the ways the block moves between its columns. */
export type Frame = {
    /** Whether the block itself is narrower than a phone's layout allows, wherever it sits in the window. */
    isPhone: Ref<boolean>;

    /** Turned on by the person's first move between panes, so the pane a page load opens does not slide in. */
    isPaneMotionOn: Ref<boolean>;

    pane: ComputedRef<FramePane>;

    isSidebarHidden: ComputedRef<boolean>;

    isConversationHidden: ComputedRef<boolean>;

    isPanelHidden: ComputedRef<boolean>;

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

    /** What the column draws: the open panel, kept through its close so it narrows over its content. */
    shownPanel: Ref<PanelKind | null>;

    panelElement: Ref<HTMLElement | undefined>;

    /** The thread the drawer draws, kept after the threads let it go so the drawer narrows over it. */
    drawnThread: ShallowRef<OpenThreadState | null>;

    /** The list the open thread came from, which its back chevron and Escape return to, or null. */
    threadOrigin: Ref<ThreadOrigin>;

    /** Counts every thread opened or left, so a landing can tell the drawer it started in is gone. */
    threadOpenCount: () => number;

    onResize: (entries: ResizeObserverEntry[]) => void;

    onSelect: (channelId: string) => Promise<void>;

    onBack: () => Promise<void>;

    onListPanel: (kind: "threads" | "mentions") => Promise<void>;

    openThreadFromList: (rootId: number, channelId: string) => void;

    onOpenThread: (rootId: number) => void;

    onThreadBack: () => Promise<void>;

    onMentionOpen: (messageId: number) => void;

    openThreadFromMentions: (channelId: string, rootId: number, messageId: number) => Promise<void>;

    openFromTap: (channelId: string, rootId: number | null, messageId: number | null) => void;

    openThreadDrawer: (channelId: string, rootId: number, origin: ThreadOrigin) => Promise<void>;

    closeThreadDrawer: () => void;

    closePanel: () => void;

    /**
     * Opens a full view in the conversation's place, or brings the one open back to the front.
     * New message begins with the people given, as a profile card's Message chooses its person.
     */
    openMainView: (kind: MainViewKind, people?: PersonRow[]) => Promise<void>;

    /** Closes the full view: its X or Escape. The conversation underneath comes back. */
    closeMainView: () => void;

    /** A room chosen in the full view: previewed if it has no sidebar row, opened, and the view closed. */
    openFromMainView: (channelId: string) => Promise<void>;
};

/** Below this width the block shows one pane at a time, as on a phone. */
const phoneWidth = 760;

/** The design's pane slide, used when the sheet is not on the page. */
const fallbackPaneSlideMs = 300;

/**
 * Creates the block's frame. Its watches of the open conversation are made here in the order they
 * must run, so a thread a link opens is not closed by the same switch that brought its conversation.
 *
 * @param dependencies What the frame reads from the block.
 *
 * @returns What the template draws and the ways to move between the columns.
 */
export function useFrame(dependencies: FrameDependencies): Frame {
    const { shell, activeChannelId, rootElement } = dependencies;

    const isPhone = ref(false);

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

    const panelElement = ref<HTMLElement>();

    const mainView = ref<MainViewKind | null>(null);

    /** What had focus as the full view opened, which its close and a phone's Back return to. */
    let mainViewOpener: HTMLElement | null = null;

    let panelClearTimer: number | undefined;

    const drawnThread = shallowRef<OpenThreadState | null>(null);

    const threadOrigin = ref<ThreadOrigin>(null);

    /** The conversation the open thread belongs to, which the drawer stays open over. */
    let threadChannelId: string | null = null;

    /**
     * Counts every thread opened or left, so a landing on a reply that awaited an older page can
     * tell the drawer it started in is gone and stop.
     */
    let threadOpenCount = 0;

    /**
     * The reply a link to this page named, until the first conversation opens: its thread opens
     * over that conversation, and over no other the page load falls back to.
     */
    let linkedReply = dependencies.linkedReply;

    /**
     * Which pane a phone shows: the conversation once one is open, or a full view in its place,
     * until Back asks for the list, and an open panel over the conversation.
     */
    const pane = computed((): FramePane => {
        if (!(activeChannelId.value || mainView.value) || isListChosen.value) {
            return "list";
        }

        return panelKind.value && !mainView.value ? "panel" : "conversation";
    });

    const isSidebarHidden = computed((): boolean => isPhone.value && pane.value !== "list");

    const isConversationHidden = computed((): boolean => isPhone.value && pane.value !== "conversation");

    /** A closed panel is out of reach everywhere; an open one only while a phone shows another pane. */
    const isPanelHidden = computed((): boolean => !panelKind.value || (isPhone.value && pane.value !== "panel"));

    // #region Event Handlers

    /** The block observed its own width, which alone decides whether it shows phone panes. */
    function onResize(entries: ResizeObserverEntry[]): void {
        const width = entries[entries.length - 1]?.contentRect.width;
        if (width !== undefined) {
            isPhone.value = width < phoneWidth;
        }
    }

    async function onSelect(channelId: string): Promise<void> {
        // Choosing the conversation already open behind the list or a full view brings it back too.
        leaveMainView();
        isListChosen.value = false;
        isPaneMotionOn.value = true;
        void shell.value.selectChannel(channelId);

        // The row that had focus has slid out of view and out of reach, so focus goes to what
        // slid in: the conversation's title, never its composer, which would raise the keyboard.
        if (isPhone.value) {
            await nextTick();
            rootElement.value?.querySelector<HTMLElement>("[data-header-focus]")?.focus();
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
            await nextTick();
            if (mainView.value && mainViewOpener?.isConnected) {
                mainViewOpener.focus();
                return;
            }

            const row = leftChannelId
                ? rootElement.value?.querySelector<HTMLElement>(`[data-testid='channel-row'][data-channel-id='${leftChannelId}']`)
                : null;
            (row ?? rootElement.value?.querySelector<HTMLElement>("[data-testid='new-message']"))?.focus();
        }
    }

    /**
     * The header's Threads opens the open conversation's threads, and its Mentions the inbox; each
     * closes its list when it is the one open.
     */
    async function onListPanel(kind: "threads" | "mentions"): Promise<void> {
        if (panelKind.value === kind) {
            closePanel();
            return;
        }

        const channelId = activeChannelId.value;
        if (kind === "threads" && !channelId) {
            return;
        }

        // The column holds one panel at a time, so the list takes an open thread's or list's place.
        releasePanel();
        window.clearTimeout(panelClearTimer);
        isPaneMotionOn.value = true;
        panelKind.value = kind;
        shownPanel.value = kind;
        void (channelId && kind === "threads" ? shell.value.threads.openList(channelId) : shell.value.mentions.openInbox());

        // Focus goes to the panel's first control, or its X or back chevron when it has none yet,
        // without that button's tooltip, as a list whose first control is a tab shows none.
        await nextTick();
        const panel = panelElement.value;
        const first = panel?.querySelector<HTMLElement>("[data-autofocus]");
        const exit = panel?.querySelector<HTMLElement>("[data-testid='panel-close'], [data-testid='panel-back']");
        if (first) {
            first.focus();
        }
        else if (exit) {
            focusQuietly(exit);
        }
    }

    /**
     * A row of the threads list chose its thread: its conversation opens behind the drawer, as a
     * sidebar row would open it, and the thread opens in the panel column in the list's place,
     * with a back chevron that returns to the list and the row that opened it.
     */
    function openThreadFromList(rootId: number, channelId: string): void {
        if (channelId !== activeChannelId.value) {
            void shell.value.selectChannel(channelId);
        }
        void openThreadDrawer(channelId, rootId, "threads");
    }

    /** A thread pill, or the "Replied to a thread" label over a reply, in the open conversation. */
    function onOpenThread(rootId: number): void {
        if (activeChannelId.value) {
            void openThreadDrawer(activeChannelId.value, rootId, null);
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
        await nextTick();
        panelElement.value?.querySelector<HTMLElement>(origin === "threads"
            ? `[data-testid='thread-row-open'][data-root-id='${rootId}']`
            : "[data-testid='mention-row-open'][aria-current='true']")?.focus();
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
                releasePanel();
                panelKind.value = null;
                finishClosing();
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

        await nextTick();
        if (!isPhone.value || kind === "newdm") {
            rootElement.value?.querySelector<HTMLElement>("[data-testid='main-view'] [data-autofocus]")?.focus();
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

        void nextTick(() => {
            const root = rootElement.value;
            const composer = isPhone.value ? null : root?.querySelector<HTMLElement>("[data-testid='frame-main'] [data-testid='composer-input']");
            const isOpenerShown = !!opener?.isConnected && !(isPhone.value && pane.value !== "list");
            (composer
                ?? (isOpenerShown ? opener : null)
                ?? root?.querySelector<HTMLElement>("[data-header-focus]"))?.focus();
        });
    }

    /**
     * Opens a room chosen in the full view. One with no sidebar row is previewed before it opens,
     * so its row and its join bar are there at once; the view closes even when the room is the
     * one already underneath. Focus lands on Join, the preview's one action, else on the composer,
     * or on a phone the title, since a focused composer would raise the keyboard.
     */
    async function openFromMainView(channelId: string): Promise<void> {
        shell.value.rooms.previewRoom(channelId);
        leaveMainView();
        isListChosen.value = false;
        isPaneMotionOn.value = true;
        await shell.value.selectChannel(channelId);

        await nextTick();
        const main = rootElement.value?.querySelector<HTMLElement>("[data-testid='frame-main']");
        (main?.querySelector<HTMLElement>("[data-testid='join-channel']")
            ?? (isPhone.value ? null : main?.querySelector<HTMLElement>("[data-testid='composer-input']"))
            ?? main?.querySelector<HTMLElement>("[data-header-focus]"))?.focus();
    }

    // #endregion Event Handlers

    // #region Functions

    /** Lets go of the full view, without moving focus. */
    function leaveMainView(): void {
        const kind = mainView.value;
        if (!kind) {
            return;
        }

        mainView.value = null;
        mainViewOpener = null;
        if (kind === "directory") {
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
        await dependencies.landOnReply({ channelId, rootId, messageId }, "mentions");
    }

    /**
     * A push's tap brings what it was for to the front, even when its conversation is already the
     * one open behind a phone's list: the thread for a reply, else the conversation, which a
     * phone's panel would cover. A desktop shows a panel beside it, so keeps it.
     */
    function openFromTap(channelId: string, rootId: number | null, messageId: number | null): void {
        const isOpenConversation = channelId === activeChannelId.value;
        isListChosen.value = false;
        isPaneMotionOn.value = true;
        void shell.value.selectChannel(channelId);
        if (rootId !== null) {
            void dependencies.landOnReply({ channelId, rootId, messageId });
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

    /** Lets go of whatever the column holds: an open thread, and the list it was opened from or the list open. */
    function releasePanel(): void {
        const list = panelKind.value === "thread" ? threadOrigin.value : panelKind.value;
        leaveThread();
        if (list === "threads") {
            shell.value.threads.closeList();
        }
        else if (list === "mentions") {
            shell.value.mentions.closeInbox();
        }
    }

    /**
     * Closes the panel column. Focus returns to the header button that opened it, or on a phone
     * to the overflow button it was chosen from, else the conversation's title.
     */
    function closePanel(): void {
        if (!panelKind.value) {
            return;
        }

        const closed = panelKind.value;
        releasePanel();
        panelKind.value = null;
        finishClosing();

        void nextTick(() => {
            const root = rootElement.value;
            (root?.querySelector<HTMLElement>(`[data-testid='header-${closed}']`)
                ?? root?.querySelector<HTMLElement>("[data-testid='header-more'] button")
                ?? root?.querySelector<HTMLElement>("[data-header-focus]"))?.focus();
        });
    }

    /**
     * Opens a thread in the drawer. One opened from the conversation replaces whatever the column
     * held; one opened from a list keeps the list to go back to. Its mentions stop lighting the
     * header's dot, as the thread is where they are read.
     */
    async function openThreadDrawer(channelId: string, rootId: number, origin: ThreadOrigin): Promise<void> {
        if (origin === null && (panelKind.value === "threads" || panelKind.value === "mentions")) {
            releasePanel();
        }
        shell.value.mentions.threadOpened(channelId, rootId);

        threadOpenCount++;
        dependencies.forgetReplyLanding();
        window.clearTimeout(panelClearTimer);
        isPaneMotionOn.value = true;
        threadOrigin.value = origin;
        threadChannelId = channelId;
        panelKind.value = "thread";
        shownPanel.value = "thread";
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
     * Closes the drawer. Focus returns to the thread pill that opened it, else the root message,
     * else the conversation's title.
     */
    function closeThreadDrawer(): void {
        const rootId = drawnThread.value?.rootId;
        releasePanel();
        finishClosing();

        void nextTick(() => {
            const root = rootElement.value;
            const message = root?.querySelector<HTMLElement>(`[data-testid='frame-main'] [data-message-id='${rootId}']`);
            (message?.querySelector<HTMLElement>("[data-testid='thread-summary']")
                ?? message
                ?? root?.querySelector<HTMLElement>("[data-header-focus]"))?.focus();
        });
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
        const isReduced = window.matchMedia?.("(prefers-reduced-motion: reduce)").matches ?? false;
        if (!isPaneMotionOn.value || isReduced || !rootElement.value) {
            return 0;
        }

        return tokenDuration(rootElement.value, "--chat-t-panel", fallbackPaneSlideMs);
    }

    /** How long the panel column takes to close, which its content stays drawn through. */
    function panelSlideMs(): number {
        const isReduced = window.matchMedia?.("(prefers-reduced-motion: reduce)").matches ?? false;
        if (isReduced || !rootElement.value) {
            return 0;
        }

        return tokenDuration(rootElement.value, "--chat-t-panel", fallbackPaneSlideMs);
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
            void dependencies.landOnReply(landing);
        }
    });

    // A thread belongs to its own conversation, so another conversation closes it. A
    // thread opened from the list switches to its conversation first, which keeps it open. Focus
    // inside the closing drawer would drop to the page as it leaves reach, so it goes to the title
    // of what replaced it, as a phone's row does when it opens a conversation.
    watch(activeChannelId, channelId => {
        if (panelKind.value === "thread" && channelId !== threadChannelId) {
            const hadFocus = !!panelElement.value?.contains(document.activeElement);
            leaveThread();
            finishClosing();

            if (hadFocus) {
                void nextTick(() => rootElement.value?.querySelector<HTMLElement>("[data-header-focus]")?.focus());
            }
        }
    });

    // The threads list follows the open conversation, and closes with none open.
    watch(activeChannelId, channelId => {
        if (panelKind.value !== "threads") {
            return;
        }

        if (channelId) {
            void shell.value.threads.openList(channelId);
        }
        else {
            closePanel();
        }
    });

    // #endregion Watchers

    onBeforeUnmount(() => {
        window.clearTimeout(paneGoneTimer);
        window.clearTimeout(panelClearTimer);
    });

    return {
        isPhone,
        isPaneMotionOn,
        pane,
        isSidebarHidden,
        isConversationHidden,
        isPanelHidden,
        isSidebarGone,
        isConversationGone,
        isPanelGone,
        goneStyle,
        panelKind,
        mainView,
        shownPanel,
        panelElement,
        drawnThread,
        threadOrigin,
        threadOpenCount: () => threadOpenCount,
        onResize,
        onSelect,
        onBack,
        onListPanel,
        openThreadFromList,
        onOpenThread,
        onThreadBack,
        onMentionOpen,
        openThreadFromMentions,
        openFromTap,
        openThreadDrawer,
        closeThreadDrawer,
        closePanel,
        openMainView,
        closeMainView,
        openFromMainView
    };
}
