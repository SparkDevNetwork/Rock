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
// The chat's frame, mounted as the block: three columns side by side while the block is wide, and
// one pane at a time once the block itself is narrower than 760px, whatever the window's width,
// since a block in a narrow zone of a wide screen has no more room than a phone. Opening a
// conversation on a phone brings its pane in over the list, Back returns to the list, and nothing
// focuses the composer on its own there, because that would raise the keyboard over the messages.
//
// The shell behind the block is replaced with a plain reactive stand-in, so the frame is tested
// without a platform. jsdom lays nothing out, so the block's width comes from a ResizeObserver the
// test reports through.
import { readFileSync } from "fs";
import { join } from "path";
import { mount, VueWrapper } from "@vue/test-utils";
import { nextTick, reactive } from "vue";
import { ChannelFacts, MentionRow, SidebarRow, TimelineMessage } from "../../../../src/Communication/Chat/ChatShell/types.partial";
import { ShellOptions } from "../../../../src/Communication/Chat/ChatShell/shell.partial";
import ComposerComponent from "../../../../src/Communication/Chat/ChatShell/components/composer.partial.obs";
import MessageFeedComponent from "../../../../src/Communication/Chat/ChatShell/components/messageFeed.partial.obs";
import ProfileCardComponent from "../../../../src/Communication/Chat/ChatShell/components/profileCard.partial.obs";
import ThreadDrawerComponent from "../../../../src/Communication/Chat/ChatShell/components/threadDrawer.partial.obs";

// #region Stand-ins

const team: SidebarRow = sidebarRow({ channel_id: "c0000001-0000-4000-8000-000000000000", name: "Worship Team" });
const dmAnna: SidebarRow = sidebarRow({
    channel_id: "c0000005-0000-4000-8000-000000000000",
    channel_type: "dm",
    dm_names: ["Anna Kim"],
    dm_other_count: 1
});

/** A sidebar row in the exact shape chat_get_bootstrap answers. */
function sidebarRow(overrides: Partial<SidebarRow> & { channel_id: string }): SidebarRow {
    return {
        name: null,
        icon_url: null,
        channel_type: "shared",
        is_public: false,
        always_shown: false,
        is_member: true,
        is_favorite: false,
        notify_mode: "all",
        is_leader: false,
        can_post_announcements: false,
        can_mention_all: false,
        last_message_id: null,
        last_message_at: null,
        last_message_type: null,
        last_sender_nick_name: null,
        last_sender_last_name: null,
        last_sender_listed: false,
        last_message_preview: null,
        last_message_is_own: false,
        last_message_posted_by_rock: false,
        read_cursor: null,
        is_unread: false,
        mention_count: 0,
        all_mention_count: 0,
        can_mention: true,
        dm_names: null,
        dm_other_count: null,
        ...overrides
    };
}

/**
 * The thread a test holds for a push to land in, by reply id: its newest page, and the older pages
 * each loadOlderReplies call answers, in order, when the test answers it.
 */
let mockThreadPages: { newest: number[], older: number[][] } | null = null;

/** The loadOlderReplies calls waiting for the test to answer them, oldest first. */
const mockOlderAnswers: Array<() => void> = [];

/** The shell the test mounted last, so a test can open a draft on it. */
let mockShell: Record<string, unknown> | null = null;

/** What the block built its last shell with, which carries the frame's views for a mention's jump. */
let mockShellOptions: ShellOptions | null = null;

/** The block's actions, as the page asks Rock for them; answered by each test that asks. */
const mockInvoke = jest.fn();

/** Each conversation's timeline as the stand-in holds it, so a test can put messages in one. */
const mockTimelines = new Map<string, { messages: TimelineMessage[] }>();

/** A conversation's timeline, made once and kept, as the shell's timelines are. */
function timelineOf(channelId: string): { messages: TimelineMessage[] } {
    let timeline = mockTimelines.get(channelId);
    if (!timeline) {
        timeline = reactive({ messages: [] as TimelineMessage[], isLoaded: true, hasMore: false, readCursor: null, unreadCount: 0, isBehind: false, verifiedThrough: null });
        mockTimelines.set(channelId, timeline);
    }

    return timeline;
}

/** A stored text message in the shape history answers, from Anna unless a test says otherwise. */
function storedMessage(id: number, extra: Partial<TimelineMessage> = {}): TimelineMessage {
    return {
        id,
        parent_id: null,
        person_alias_guid: "a0000002-0000-4000-8000-000000000000",
        sender_nick_name: "Anna",
        sender_last_name: "Kim",
        sender_avatar_url: null,
        sender_listed: true,
        message_type: "text",
        body: `message ${id}`,
        pieces: null,
        blocks: null,
        created_at: new Date().toISOString(),
        edited_at: null,
        deleted_at: null,
        visibility: "visible",
        visibility_notice: null,
        ...extra
    } as unknown as TimelineMessage;
}

/**
 * The init box the block reads. A test may add a field an older Rock sent, to show the block no
 * longer reads it.
 */
const mockConfig: Record<string, unknown> = {};

function resetConfig(): void {
    for (const key of Object.keys(mockConfig)) {
        delete mockConfig[key];
    }

    Object.assign(mockConfig, {
        session: { gate: "ok", personAliasGuid: "a0000001-0000-4000-8000-000000000000", personNickName: "Jordan", personLastName: "Ellis", canStartDm: true },
        channelGuid: null,
        areSharedChannelsFilteredByCampus: false,
        organizationName: "Sun Valley Church"
    });
}

resetConfig();

/** The shell the block drives, reduced to what the frame reads, ready with two rooms loaded. */
function fakeShell(): Record<string, unknown> {
    const state = reactive({
        phase: "ready",
        gate: "ok",
        connection: null,
        errors: [],
        activeChannelId: null as string | null,
        banner: null,
        isComposerOpen: true,
        isPushOffered: false,
        push: "offered"
    });

    const threads = fakeThreads();
    const mentions = fakeMentions(state);
    const rooms = fakeRooms();

    mockShell = {
        state,
        channels: reactive({ rows: [team, dmAnna] }),
        timelines: {
            state: timelineOf
        },
        sender: { pending: () => [], retry: jest.fn(), discard: jest.fn() },
        directMessages: reactive({
            draft: null,
            held: null,
            chosen: [],
            error: null,
            refused: null,
            lookup: { status: "idle", channelId: null, isSlow: false },
            recent: null,
            search: jest.fn(async () => []),
            choose: jest.fn(),
            unchoose: jest.fn(),
            edit: jest.fn(),
            begin: jest.fn(),
            close: jest.fn(),
            sendFirst: jest.fn(async () => true),
            discardFailed: jest.fn()
        }),
        isNewMessageShown: true,
        openDirectMessage: jest.fn(),
        // The page load opens the channel a link named, as the shell's start does.
        start: jest.fn(async () => {
            if (mockConfig.channelGuid) {
                state.activeChannelId = (mockConfig.channelGuid as string).toLowerCase();
            }
        }),
        stop: jest.fn(async () => undefined),
        selectChannel: jest.fn(async (channelId: string) => {
            state.activeChannelId = channelId;
        }),
        seen: jest.fn(),
        markRead: jest.fn(),
        firstMessageShown: jest.fn(),
        send: jest.fn(),
        turnOnPush: jest.fn(),
        turnOffPush: jest.fn(),
        isChannelOnScreen: () => false,
        // as the shell answers it: the thread open in the drawer, on a page the test calls visible
        isThreadOnScreen: (rootId: number) => (threads.state as { thread: { rootId: number } | null }).thread?.rootId === rootId,
        mentionBadge: () => 0,
        loadOlder: jest.fn(),
        dismissError: jest.fn(),
        sendReply: jest.fn(),
        threads,
        mentions,
        rooms,
        members: fakeMembers(),
        prefs: fakePrefs(),
        // the quotes the composers draw and the Up key's edit, as useMessageActions holds them
        messageActions: {
            state: reactive({ editing: null, confirmingDelete: null, quotes: {} }),
            startEditNewest: jest.fn(() => false),
            cancelQuote: jest.fn(),
            saveEdit: jest.fn(async () => true),
            cancelEdit: jest.fn()
        },
        // the reactions the rows, the composers and the picker read, as useReactions holds them
        reactions: {
            state: reactive({ catalog: null as unknown[] | null, recents: [] as string[] }),
            favorites: [{ code: "thumbsup", label: "thumbs up", emoji: "👍", image_url: null }, { code: "pray", label: "folded hands", emoji: "🙏", image_url: null }],
            toggleCode: jest.fn(async () => undefined),
            loadCatalog: jest.fn(async () => null),
            recordRecent: jest.fn(),
            reactorNames: jest.fn(async () => null)
        }
    };

    return mockShell;
}

/**
 * The person's settings the shell holds, as usePrefs' createPrefs answers them, reduced to the
 * state the block draws and the saves it asks for.
 */
function fakePrefs(): Record<string, unknown> {
    const state = reactive({
        me: { show_profile_details: true, is_open_dm_allowed: false, reaction_push: true },
        meSave: null,
        meNotes: {},
        rooms: {} as Record<string, unknown>,
        announcement: null
    });

    return {
        state,
        roomOf: (channelId: string) => state.rooms[channelId] ?? { save: null, notes: {} },
        setMe: jest.fn(),
        setNotifyMode: jest.fn(async () => undefined),
        setStarred: jest.fn(async () => undefined),
        setHidden: jest.fn(async () => undefined),
        setPersonSetting: jest.fn(async () => undefined),
        setReactionPush: jest.fn(async () => undefined),
        stop: jest.fn()
    };
}

/**
 * The rooms the shell holds, as useRooms' createRooms answers them, reduced to the state the block
 * draws and the calls it makes: opening and closing the directory, Join, and each room's facts.
 */
function fakeRooms(): Record<string, unknown> {
    const state = reactive({
        directory: {
            isOpen: false,
            query: "",
            campus: { kind: "all" },
            show: "all",
            rows: [] as unknown[],
            total: 0,
            hasMore: false,
            isLoading: false,
            isSearching: false,
            isLoadingMore: false,
            error: null,
            moreError: null,
            isCampusLimited: false,
            emptyKind: "none" as string | null
        },
        facts: {} as Record<string, ChannelFacts>,
        joins: {} as Record<string, unknown>,
        leaves: {} as Record<string, unknown>,
        renames: {} as Record<string, unknown>,
        card: null as Record<string, unknown> | null,
        announcement: null
    });

    return {
        state,
        openDirectory: jest.fn(() => {
            state.directory.isOpen = true;
        }),
        closeDirectory: jest.fn(() => {
            state.directory.isOpen = false;
        }),
        setQuery: jest.fn(),
        clearQuery: jest.fn(),
        setCampus: jest.fn(),
        setShow: jest.fn(),
        clearFilters: jest.fn(),
        loadMore: jest.fn(async () => undefined),
        retry: jest.fn(async () => undefined),
        join: jest.fn(async () => undefined),
        joinOf: (channelId: string) => state.joins[channelId] ?? { kind: "idle" },
        factsOf: (channelId: string) => state.facts[channelId] ?? null,
        previewRoom: jest.fn(),
        leave: jest.fn(async () => undefined),
        leaveOf: (channelId: string) => state.leaves[channelId] ?? { kind: "idle" },
        rename: jest.fn(async () => undefined),
        renameOf: (channelId: string) => state.renames[channelId] ?? { kind: "idle" },
        openCard: jest.fn(),
        readPerson: jest.fn(async () => null),
        closeCard: jest.fn(() => {
            state.card = null;
        }),
        stop: jest.fn()
    };
}

/**
 * The roster the shell holds, as useMembers' createMembers answers it, reduced to state the panel
 * reads and the calls the block makes.
 */
function fakeMembers(): Record<string, unknown> {
    const state = reactive({
        channelId: null as string | null, rows: [], hasMore: false, isLoading: false, isLoadingMore: false,
        error: null, moreError: null, refusal: null, query: "",
        search: { query: null, rows: [], hasMore: false, isSearching: false, error: null },
        canManageMembers: false, isChatAdministrator: false, removals: {}, confirming: null as unknown,
        successorQuery: "", successor: { query: null, rows: [], hasMore: false, isSearching: false, error: null }, successorRefusal: null,
        adding: { isOpen: false, query: "", people: null, isSearching: false, error: null, isAdding: false, refusal: null }
    });
    return {
        state,
        open: jest.fn(async (channelId: string) => {
            state.channelId = channelId;
        }),
        loadMore: jest.fn(async () => undefined),
        retry: jest.fn(async () => undefined),
        setQuery: jest.fn(),
        retrySearch: jest.fn(async () => undefined),
        close: jest.fn(() => {
            state.channelId = null;
        }),
        stop: jest.fn(),
        askRemove: jest.fn(),
        cancelRemove: jest.fn(),
        remove: jest.fn(async () => undefined),
        dismissRemoval: jest.fn(),
        setSuccessorQuery: jest.fn(),
        openAdd: jest.fn(),
        closeAdd: jest.fn(),
        setAddQuery: jest.fn(),
        add: jest.fn(async () => undefined)
    };
}

/**
 * The threads the shell holds, as useThreads' createThreads answers them, reduced to state the
 * block reads and calls it makes: opening the list names the conversation and its tab.
 */
function fakeThreads(): Record<string, unknown> {
    const state = reactive({
        list: { tab: "channel", channelId: null as string | null, rows: [], hasMore: false, isLoading: false, isRefreshing: false, error: null, busyRootId: null, rowError: null },
        thread: null as Record<string, unknown> | null
    });

    return {
        state,
        openList: jest.fn(async (channelId: string, tab?: string) => {
            state.list.channelId = channelId;
            state.list.tab = tab ?? "channel";
        }),
        switchTab: jest.fn(async (tab: string) => {
            state.list.tab = tab;
        }),
        refresh: jest.fn(async () => undefined),
        loadMore: jest.fn(async () => undefined),
        closeList: jest.fn(),
        follow: jest.fn(async () => true),
        // The open thread as the drawer draws it, its root taken from the conversation that holds it.
        openThread: jest.fn(async (channelId: string, rootId: number) => {
            const root = timelineOf(channelId).messages.find(m => m.id === rootId) ?? storedMessage(rootId);
            const replies = (mockThreadPages?.newest ?? []).map(id => storedMessage(id, { parent_id: rootId }));
            const hasMore = (mockThreadPages?.older.length ?? 0) > 0;
            state.thread = { channelId, rootId, root, replies, following: false, readCursor: null, hasMore, isLoading: false, error: null, olderError: null };
        }),
        // Each older page waits for the test to answer it, then goes in above what is held, as
        // useThreads' loadOlderReplies does.
        loadOlderReplies: jest.fn(() => {
            const thread = state.thread as { rootId: number, replies: TimelineMessage[], hasMore: boolean } | null;
            if (!mockThreadPages || !thread) {
                return Promise.resolve();
            }

            return new Promise<void>(resolve => {
                mockOlderAnswers.push(() => {
                    const page = mockThreadPages?.older.shift() ?? [];
                    thread.replies.unshift(...page.map(id => storedMessage(id, { parent_id: thread.rootId })));
                    thread.hasMore = (mockThreadPages?.older.length ?? 0) > 0;
                    resolve();
                });
            });
        }),
        seen: jest.fn(),
        closeThread: jest.fn(async () => {
            state.thread = null;
        }),
        attach: jest.fn(() => () => undefined),
        applyEvent: jest.fn(),
        stop: jest.fn()
    };
}

/**
 * The mentions the shell holds, as useMentions' createMentions answers them, reduced to state the
 * block reads and calls it makes. A jump marks its row as the one opened and goes where
 * createMentions' jump goes: a reply's thread through the frame's view, else its conversation,
 * opened as the shell opens one, and the message landed on through the frame's view.
 */
function fakeMentions(shellState: { activeChannelId: string | null }): Record<string, unknown> {
    const state = reactive({
        picker: { query: null as string | null, people: [] as unknown[], isAllOffered: false },
        inbox: { rows: [] as MentionRow[], hasMore: false, isLoading: false, isRefreshing: false, isLoadingOlder: false, error: null, olderError: null, newIds: [] as number[], currentId: null as number | null },
        hasDot: false
    });

    return {
        state,
        search: jest.fn(),
        closeSearch: jest.fn(),
        openInbox: jest.fn(async () => undefined),
        refreshInbox: jest.fn(async () => undefined),
        loadOlder: jest.fn(async () => undefined),
        closeInbox: jest.fn(),
        jump: jest.fn(async (row: MentionRow) => {
            state.inbox.currentId = row.message.id;
            if (row.parent_id !== null) {
                await mockShellOptions?.mentionViews?.openThread(row.channel_id, row.parent_id, row.message.id);
                return;
            }

            shellState.activeChannelId = row.channel_id;
            await nextTick();
            mockShellOptions?.mentionViews?.land(row.message.id);
        }),
        setUnread: jest.fn(),
        applyPersonalEvent: jest.fn(),
        channelRead: jest.fn(),
        threadOpened: jest.fn(),
        stop: jest.fn()
    };
}

jest.mock("@Obsidian/Utility/block", () => ({
    useConfigurationValues: () => mockConfig,
    useInvokeBlockAction: () => mockInvoke
}));

jest.mock("@Obsidian/Libs/supabase", () => ({ createClient: jest.fn() }));

jest.mock("../../../../src/Communication/Chat/ChatShell/shell.partial", () => ({
    ...jest.requireActual("../../../../src/Communication/Chat/ChatShell/shell.partial"),
    createChatShell: (options: ShellOptions) => {
        mockShellOptions = options;
        return fakeShell();
    }
}));

/** The block's width as the test says it is, read by the ResizeObserver stand-in and by any size read. */
let blockWidth = 1200;

/** A ResizeObserver the test reports through, since jsdom computes no sizes. */
class FakeResizeObserver {
    static all: FakeResizeObserver[] = [];

    readonly watched = new Set<Element>();

    constructor(private readonly callback: ResizeObserverCallback) {
        FakeResizeObserver.all.push(this);
    }

    observe(element: Element): void {
        this.watched.add(element);
        sizeElement(element);
        // A browser reports a first size as soon as an element is observed.
        this.callback([entryFor(element)], this as unknown as ResizeObserver);
    }

    unobserve(element: Element): void {
        this.watched.delete(element);
    }

    disconnect(): void {
        this.watched.clear();
    }

    /** Sets the block's width and tells every observer of every element it watches. */
    static resize(width: number): void {
        blockWidth = width;
        for (const observer of FakeResizeObserver.all) {
            const entries = Array.from(observer.watched).map(element => entryFor(element));
            if (entries.length > 0) {
                observer.callback(entries, observer as unknown as ResizeObserver);
            }
        }
    }
}

/** Gives an element the test's width wherever a size might be read from it. */
function sizeElement(element: Element): void {
    for (const property of ["clientWidth", "offsetWidth", "scrollWidth"]) {
        Object.defineProperty(element, property, { configurable: true, get: () => blockWidth });
    }
    element.getBoundingClientRect = (): DOMRect => ({ x: 0, y: 0, top: 0, left: 0, right: blockWidth, bottom: 800, width: blockWidth, height: 800, toJSON: () => ({}) }) as DOMRect;
}

function entryFor(element: Element): ResizeObserverEntry {
    const rect = { x: 0, y: 0, top: 0, left: 0, right: blockWidth, bottom: 800, width: blockWidth, height: 800, toJSON: () => ({}) } as DOMRectReadOnly;
    const size = [{ inlineSize: blockWidth, blockSize: 800 }];
    return { target: element, contentRect: rect, borderBoxSize: size, contentBoxSize: size, devicePixelContentBoxSize: size } as unknown as ResizeObserverEntry;
}

// #endregion Stand-ins

// #region Helpers

let wrapper: VueWrapper | null = null;

/** The rules a component's styles give under reduced motion, selector and body, read from its source. */
function reducedMotionRules(relativePath: string): Array<{ selector: string, body: string }> {
    const source = readFileSync(join(__dirname, relativePath), "utf8");
    const style = source.slice(source.indexOf("<style"), source.indexOf("</style>")).replace(/\/\*[\s\S]*?\*\//g, "");
    const rules: Array<{ selector: string, body: string }> = [];
    for (const media of style.matchAll(/@media \(prefers-reduced-motion: reduce\)\s*\{/g)) {
        // the block runs to the brace that closes the one the query opened
        let depth = 1;
        let end = (media.index ?? 0) + media[0].length;
        for (; end < style.length && depth > 0; end++) {
            depth += style[end] === "{" ? 1 : style[end] === "}" ? -1 : 0;
        }
        const inner = style.slice((media.index ?? 0) + media[0].length, end - 1);
        rules.push(...Array.from(inner.matchAll(/([^{}]+)\{([^{}]*)\}/g)).map(m => ({ selector: m[1].trim(), body: m[2] })));
    }
    return rules;
}

async function settle(): Promise<void> {
    for (let i = 0; i < 4; i++) {
        await nextTick();
    }
    await Promise.resolve();
}

/**
 * Mounts the block. A test about what happens while a part fades out keeps Vue's real Transition,
 * which holds a leaving element on the page until the frames after its fade, as a browser does.
 */
async function mountBlock(width: number, isTransitionKept = false): Promise<VueWrapper> {
    blockWidth = width;
    const ChatShell = (await import("../../../../src/Communication/Chat/chatShell.obs")).default;
    wrapper = mount(ChatShell, { attachTo: document.body, global: isTransitionKept ? { stubs: { transition: false } } : {} });
    await settle();
    return wrapper;
}

function shellRoot(w: VueWrapper): HTMLElement {
    return w.get("[data-testid='chat-shell']").element as HTMLElement;
}

/** Whether a pane is taken out of view and out of reach, as a phone's off-screen pane must be. */
function isPaneHidden(w: VueWrapper, testId: string): boolean {
    const pane = w.get(`[data-testid='${testId}']`).element as HTMLElement;
    return pane.getAttribute("aria-hidden") === "true" || pane.hasAttribute("inert");
}

/** The rooms the mounted block's shell holds. */
function roomsOf(): { state: { facts: Record<string, ChannelFacts>, joins: Record<string, unknown>, directory: Record<string, unknown> } } & Record<string, jest.Mock> {
    return mockShell!.rooms as ReturnType<typeof roomsOf>;
}

/** A room's facts in the exact shape chat_get_channel answers. */
function roomFacts(overrides: Partial<ChannelFacts> & { channel_id: string }): ChannelFacts {
    return {
        name: null,
        icon_url: null,
        channel_type: "shared",
        is_member: true,
        can_post: true,
        can_post_code: null,
        can_post_reason: null,
        max_length: 5000,
        can_leave: true,
        description: null,
        group_type_name: null,
        campus_id: null,
        is_public: true,
        member_count: 0,
        leaders: [],
        people: [],
        ...overrides
    };
}

async function openRow(w: VueWrapper, channelId: string): Promise<void> {
    await w.get(`[data-testid='channel-row'][data-channel-id='${channelId}']`).trigger("click");
    await settle();
}

// #endregion Helpers

beforeAll(() => {
    window.ResizeObserver = FakeResizeObserver as unknown as typeof ResizeObserver;
    window.IntersectionObserver ??= class {
        observe(): void { /* jsdom computes no intersections. */ }
        unobserve(): void { /* As above. */ }
        disconnect(): void { /* As above. */ }
        takeRecords(): IntersectionObserverEntry[] {
            return [];
        }
    } as unknown as typeof IntersectionObserver;
});

afterEach(() => {
    wrapper?.unmount();
    wrapper = null;
    mockShell = null;
    mockShellOptions = null;
    mockInvoke.mockReset();
    mockThreadPages = null;
    mockOlderAnswers.length = 0;
    mockTimelines.clear();
    resetConfig();
    FakeResizeObserver.all = [];
    document.body.innerHTML = "";
});

describe("the frame on a wide block", () => {
    test("draws the sidebar, the conversation and an empty panel column side by side", async () => {
        const w = await mountBlock(1200);

        expect(shellRoot(w).getAttribute("data-phone")).toBe("false");
        const frame = w.get("[data-testid='chat-frame']");
        expect(frame.find("[data-testid='frame-sidebar']").exists()).toBe(true);
        expect(frame.find("[data-testid='frame-main']").exists()).toBe(true);
        expect(frame.find("[data-testid='frame-panel']").exists()).toBe(true);
        expect(isPaneHidden(w, "frame-sidebar")).toBe(false);
        expect(isPaneHidden(w, "frame-main")).toBe(false);
    });

    test("keeps both panes and draws no Back once a conversation is open", async () => {
        const w = await mountBlock(1200);
        await openRow(w, team.channel_id);

        expect(isPaneHidden(w, "frame-sidebar")).toBe(false);
        expect(isPaneHidden(w, "frame-main")).toBe(false);
        expect(w.find("[data-testid='header-back']").exists()).toBe(false);
    });

    test("titles a two-person conversation by the other person's full name, as the sidebar does", async () => {
        const w = await mountBlock(1200);
        await openRow(w, dmAnna.channel_id);

        expect(w.get("[data-testid='header-title']").text()).toBe("Anna Kim");
    });
});

describe("a conversation switch", () => {
    /** Whether the open conversation's feed plays the switch's entrance. */
    function isFeedEntering(w: VueWrapper): boolean {
        return w.get("[data-testid='message-feed']").classes().includes("is-entering");
    }

    test("the first conversation opened is simply there, and the next one enters in its place", async () => {
        const w = await mountBlock(1200);

        await openRow(w, team.channel_id);
        expect(isFeedEntering(w)).toBe(false);

        await openRow(w, dmAnna.channel_id);
        expect(isFeedEntering(w)).toBe(true);
    });
});

describe("the switch to phone panes", () => {
    test("follows the block's own width below 760px, not the window's", async () => {
        const w = await mountBlock(1200);
        expect(shellRoot(w).getAttribute("data-phone")).toBe("false");

        FakeResizeObserver.resize(759);
        await settle();
        expect(shellRoot(w).getAttribute("data-phone")).toBe("true");

        FakeResizeObserver.resize(760);
        await settle();
        expect(shellRoot(w).getAttribute("data-phone")).toBe("false");
    });

    test("starts in phone panes when the block is narrow from the first size", async () => {
        const w = await mountBlock(390);

        expect(shellRoot(w).getAttribute("data-phone")).toBe("true");
    });
});

describe("the phone panes", () => {
    test("show the list alone until a conversation is opened", async () => {
        const w = await mountBlock(390);

        expect(w.get("[data-testid='chat-frame']").attributes("data-pane")).toBe("list");
        expect(isPaneHidden(w, "frame-sidebar")).toBe(false);
        expect(isPaneHidden(w, "frame-main")).toBe(true);
    });

    test("show the conversation alone once opened, without focusing the composer", async () => {
        const w = await mountBlock(390);
        await openRow(w, team.channel_id);

        expect(w.get("[data-testid='chat-frame']").attributes("data-pane")).toBe("conversation");
        expect(isPaneHidden(w, "frame-sidebar")).toBe(true);
        expect(isPaneHidden(w, "frame-main")).toBe(false);
        expect(document.activeElement?.getAttribute("data-testid")).not.toBe("composer-input");
    });

    test("return to the list from the header's Back", async () => {
        const w = await mountBlock(390);
        await openRow(w, team.channel_id);

        await w.get("[data-testid='header-back']").trigger("click");
        await settle();

        expect(w.get("[data-testid='chat-frame']").attributes("data-pane")).toBe("list");
        expect(isPaneHidden(w, "frame-sidebar")).toBe(false);
        expect(isPaneHidden(w, "frame-main")).toBe(true);
    });

    test("keep the open conversation when the block widens again, with both panes shown", async () => {
        const w = await mountBlock(390);
        await openRow(w, team.channel_id);

        FakeResizeObserver.resize(1200);
        await settle();

        expect(isPaneHidden(w, "frame-sidebar")).toBe(false);
        expect(isPaneHidden(w, "frame-main")).toBe(false);
        expect(w.get(`[data-testid='channel-row'][data-channel-id='${team.channel_id}']`).attributes("aria-current")).toBeTruthy();
    });
});

describe("focus across the phone panes", () => {
    test("goes to the conversation's title when a row opens it, never to its composer", async () => {
        const w = await mountBlock(390);
        (w.get(`[data-testid='channel-row'][data-channel-id='${team.channel_id}']`).element as HTMLElement).focus();

        await openRow(w, team.channel_id);

        expect(document.activeElement).toBe(w.get("[data-testid='header-details']").element);
    });

    test("returns on Back to the row of the conversation just left", async () => {
        const w = await mountBlock(390);
        await openRow(w, dmAnna.channel_id);

        await w.get("[data-testid='header-back']").trigger("click");
        await settle();

        expect(document.activeElement).toBe(w.get(`[data-testid='channel-row'][data-channel-id='${dmAnna.channel_id}']`).element);
    });

    test("returns on Back from New message, which has no row, to the New message button", async () => {
        const w = await mountBlock(390);
        const button = w.get("[data-testid='new-message']");
        (button.element as HTMLElement).focus();
        await button.trigger("click");
        await settle();
        expect(w.get("[data-testid='chat-frame']").attributes("data-pane")).toBe("conversation");

        const back = w.get("[data-testid='nd-back']");
        (back.element as HTMLElement).focus();
        await back.trigger("click");
        await settle();

        expect(w.get("[data-testid='chat-frame']").attributes("data-pane")).toBe("list");
        expect(document.activeElement).toBe(w.get("[data-testid='new-message']").element);
    });

    test("stays on the row a wide block's person clicked", async () => {
        const w = await mountBlock(1200);
        const row = w.get(`[data-testid='channel-row'][data-channel-id='${team.channel_id}']`).element as HTMLElement;
        row.focus();

        await openRow(w, team.channel_id);

        expect(document.activeElement).toBe(row);
    });
});

describe("with nothing open", () => {
    test("draws no header bar and no composer, and asks for a conversation", async () => {
        const w = await mountBlock(1200);

        expect(w.find("[data-testid='chat-header']").exists()).toBe(false);
        expect(w.find("[data-testid='composer-input']").exists()).toBe(false);
        expect(w.get("[data-testid='frame-main']").text()).toContain("Choose a conversation.");
    });

    test("still shows the service banner, which is about chat as a whole", async () => {
        const w = await mountBlock(1200);
        (mockShell!.state as { banner: string | null }).banner = "Chat is read-only while we do some maintenance.";
        await settle();

        expect(w.find("[data-testid='service-banner']").exists()).toBe(true);
    });

    test("draws the header and the composer once a conversation opens", async () => {
        const w = await mountBlock(1200);
        await openRow(w, team.channel_id);

        expect(w.find("[data-testid='chat-header']").exists()).toBe(true);
        expect(w.find("[data-testid='composer-input']").exists()).toBe(true);
    });
});

describe("the reader's own send in a channel", () => {
    test("is headed with the session's name and photo", async () => {
        mockConfig.session = { ...(mockConfig.session as object), personPhotoUrl: "/GetImage.ashx?id=7" };
        const w = await mountBlock(1200);
        (mockShell!.sender as { pending: () => unknown[] }).pending = () => [{ localId: "l1", channelId: team.channel_id, body: "On my way", status: "failed", errorCode: "rpc.transport" }];

        await openRow(w, team.channel_id);

        const row = w.get("[data-testid='pending-row']");
        expect(row.get("[data-testid='sender-name']").text()).toBe("Jordan Ellis");
        expect(row.find("img").attributes("src")).toBe("/GetImage.ashx?id=7");
    });
});

describe("a phone pane out of view", () => {
    /** Whether a pane is taken out of the page's tab order by its style, which every browser honours, unlike inert. */
    function isOutOfReach(w: VueWrapper, testId: string): boolean {
        return (w.get(`[data-testid='${testId}']`).element as HTMLElement).style.visibility === "hidden";
    }

    /** Longer than any pane's slide, so a pane that slid away has finished. */
    async function afterSlide(): Promise<void> {
        await new Promise(resolve => setTimeout(resolve, 450));
        await settle();
    }

    afterEach(() => {
        delete (window as { matchMedia?: unknown }).matchMedia;
    });

    test("is out of reach once the conversation has slid over the list, and not before", async () => {
        const w = await mountBlock(390);
        await openRow(w, team.channel_id);

        // Still sliding: the list stays drawn until the conversation covers it.
        expect(isOutOfReach(w, "frame-sidebar")).toBe(false);

        await afterSlide();
        expect(isOutOfReach(w, "frame-sidebar")).toBe(true);
        expect(isOutOfReach(w, "frame-main")).toBe(false);
    });

    test("comes back into reach at once on Back, and the conversation leaves reach after its slide", async () => {
        const w = await mountBlock(390);
        await openRow(w, team.channel_id);
        await afterSlide();

        await w.get("[data-testid='header-back']").trigger("click");
        await settle();
        expect(isOutOfReach(w, "frame-sidebar")).toBe(false);

        await afterSlide();
        expect(isOutOfReach(w, "frame-main")).toBe(true);
    });

    test("is out of reach at once when the person asks for reduced motion", async () => {
        window.matchMedia = ((query: string) => ({ matches: query.includes("reduce"), media: query })) as unknown as typeof window.matchMedia;
        const w = await mountBlock(390);
        await openRow(w, team.channel_id);

        expect(isOutOfReach(w, "frame-sidebar")).toBe(true);
    });

    test("never applies to a wide block, where both panes show", async () => {
        const w = await mountBlock(1200);
        await openRow(w, team.channel_id);
        await afterSlide();

        expect(isOutOfReach(w, "frame-sidebar")).toBe(false);
        expect(isOutOfReach(w, "frame-main")).toBe(false);
    });
});

describe("the header's title", () => {
    test("is a channel's name as Rock writes it, with no '#' before it", async () => {
        const w = await mountBlock(1200);
        await openRow(w, team.channel_id);

        expect(w.get("[data-testid='header-title']").text()).toBe("Worship Team");
    });
});

describe("the look, by the open conversation's kind", () => {
    /** The look the open conversation is drawn in, which the feed and the frame's main column carry. */
    function lookOf(w: VueWrapper): string | undefined {
        return w.get("[data-testid='frame-main']").attributes("data-look");
    }

    function isFeedCommunity(w: VueWrapper): boolean {
        return w.get(".chat-feed-frame").classes().includes("is-community");
    }

    test("a channel draws Community and a direct message Conversational", async () => {
        const w = await mountBlock(1200);

        await openRow(w, team.channel_id);
        expect(lookOf(w)).toBe("community");
        expect(isFeedCommunity(w)).toBe(true);

        await openRow(w, dmAnna.channel_id);
        expect(lookOf(w)).toBe("conversational");
        expect(isFeedCommunity(w)).toBe(false);
    });

    test("New message draws its first message in Conversational, and the conversation under it keeps its own look", async () => {
        const w = await mountBlock(1200);
        await openRow(w, team.channel_id);

        await w.get("[data-testid='new-message']").trigger("click");
        const anna = { person_alias_guid: "a0000002-0000-4000-8000-000000000000", nick_name: "Anna", last_name: "Kim", avatar_url: null };
        Object.assign(mockShell!.directMessages as Record<string, unknown>, {
            chosen: [anna],
            lookup: { status: "new", channelId: null, isSlow: false },
            draft: { people: [anna], status: "starting", body: "hello", channelGuid: null, phase: "asking" }
        });
        await settle();

        expect(w.find("[data-testid='dm-draft']").exists()).toBe(false);
        expect(w.get("[data-testid='new-message-view']").attributes("data-look")).toBe("conversational");
        expect(w.find("[data-testid='nd-pending']").exists()).toBe(true);
        expect(lookOf(w)).toBe("community");
    });

    test("the sidebar draws each row by its room's kind, the channel's Community and the direct message's Conversational", async () => {
        const w = await mountBlock(1200);

        expect(w.get(`[data-testid='channel-row'][data-channel-id='${team.channel_id}']`).attributes("data-layout")).toBe("community");
        expect(w.get(`[data-testid='channel-row'][data-channel-id='${dmAnna.channel_id}']`).attributes("data-layout")).toBe("conversational");
    });

    test("reads no layout from the init box: a direct message stays Conversational beside an older box's layout field", async () => {
        mockConfig.layout = "community";
        const w = await mountBlock(1200);

        await openRow(w, dmAnna.channel_id);
        expect(lookOf(w)).toBe("conversational");
        expect(isFeedCommunity(w)).toBe(false);
        expect(w.get(`[data-testid='channel-row'][data-channel-id='${dmAnna.channel_id}']`).attributes("data-layout")).toBe("conversational");
    });
});

describe("the panel column", () => {
    // The header's Threads entry opens the threads list in the right-hand panel: a third column
    // on a wide block, opening from 0 to 392px, and a third pane on a phone, left by its back
    // chevron. The frame marks the column open with data-panel-open, which its styles widen.
    // The shell holds the threads as shell.threads, the list opened with openList(channelId, tab).

    /** The frame's own style rules, selector and body, read from the block's source. */
    function frameRules(): Array<{ selector: string, body: string }> {
        const source = readFileSync(join(__dirname, "../../../../src/Communication/Chat/chatShell.obs"), "utf8");
        const style = source.slice(source.indexOf("<style"), source.indexOf("</style>")).replace(/\/\*[\s\S]*?\*\//g, "");
        return Array.from(style.matchAll(/([^{}]+)\{([^{}]*)\}/g)).map(m => ({ selector: m[1].trim(), body: m[2] }));
    }

    function panelOpen(w: VueWrapper): string | undefined {
        return w.get("[data-testid='chat-frame']").attributes("data-panel-open");
    }

    test("is 0px closed and 392px open in the frame's styles", () => {
        const rules = frameRules();

        expect(rules.some(r => /(^|,)\s*\.chat-frame\s*$/.test(r.selector) && /grid-template-columns:\s*296px minmax\(0, 1fr\) 0px/.test(r.body))).toBe(true);
        expect(rules.some(r => r.selector.includes("data-panel-open=\"true\"") && /grid-template-columns:\s*296px minmax\(0, 1fr\) 392px/.test(r.body))).toBe(true);
    });

    test("draws its dividing line only while open, so the closed column is 0px wide", () => {
        const rules = frameRules();
        const closedOrAlways = rules.filter(r => /\.chat-frame-panel\s*$/.test(r.selector) && !r.selector.includes("data-panel-open=\"true\"") && !r.selector.includes("data-phone"));

        expect(closedOrAlways.some(r => /border-left:\s*1px/.test(r.body))).toBe(false);
        expect(rules.some(r => r.selector === ".chat-frame[data-panel-open=\"true\"] .chat-frame-panel" && /border-left:\s*1px solid var\(--chat-line\)/.test(r.body))).toBe(true);
    });

    test("opens from the header's Threads button on a wide block, for the open conversation", async () => {
        const w = await mountBlock(1200);
        await openRow(w, team.channel_id);

        const threadsButton = w.get("[data-testid='header-threads']");
        expect(threadsButton.attributes("aria-label")).toBe("Threads");
        expect(threadsButton.attributes("aria-expanded")).toBe("false");
        expect(panelOpen(w)).toBe("false");
        expect(isPaneHidden(w, "frame-panel")).toBe(true);

        await threadsButton.trigger("click");
        await settle();

        expect(panelOpen(w)).toBe("true");
        expect(isPaneHidden(w, "frame-panel")).toBe(false);
        expect(w.get("[data-testid='frame-panel'] [data-testid='panel-title']").text()).toBe("Threads");
        expect(w.get("[data-testid='header-threads']").attributes("aria-expanded")).toBe("true");
        const openList = (mockShell!.threads as { openList: jest.Mock }).openList;
        expect(openList).toHaveBeenCalledTimes(1);
        expect(openList.mock.calls[0][0]).toBe(team.channel_id);
    });

    test("closes to nothing from the panel's X", async () => {
        const w = await mountBlock(1200);
        await openRow(w, team.channel_id);
        await w.get("[data-testid='header-threads']").trigger("click");
        await settle();

        await w.get("[data-testid='panel-close']").trigger("click");
        await settle();

        expect(panelOpen(w)).toBe("false");
        expect(isPaneHidden(w, "frame-panel")).toBe(true);
        expect(w.get("[data-testid='header-threads']").attributes("aria-expanded")).toBe("false");
    });

    test("gives focus back to the header button that opened it when its X closes it", async () => {
        const w = await mountBlock(1200);
        await openRow(w, team.channel_id);
        await w.get("[data-testid='header-threads']").trigger("click");
        await settle();

        await w.get("[data-testid='panel-close']").trigger("click");
        await settle();

        expect(document.activeElement).toBe(w.get("[data-testid='header-threads']").element);
    });

    test("closes again from the header button that opened it, letting its list go", async () => {
        const w = await mountBlock(1200);
        await openRow(w, team.channel_id);
        await w.get("[data-testid='header-threads']").trigger("click");
        await settle();

        await w.get("[data-testid='header-threads']").trigger("click");
        await settle();

        expect(panelOpen(w)).toBe("false");
        expect((mockShell!.threads as { closeList: jest.Mock }).closeList).toHaveBeenCalledTimes(1);
    });

    test("keeps drawing a closed panel while the column narrows, then lets it go", async () => {
        const w = await mountBlock(1200);
        await openRow(w, team.channel_id);
        await w.get("[data-testid='header-threads']").trigger("click");
        await settle();

        await w.get("[data-testid='panel-close']").trigger("click");
        await settle();

        expect(panelOpen(w)).toBe("false");
        expect(w.find("[data-testid='frame-panel'] [data-testid='panel-title']").exists()).toBe(true);

        // jsdom has no token sheet, so the column's close takes the design's 300ms fallback.
        await new Promise(resolve => setTimeout(resolve, 350));
        await settle();

        expect(w.find("[data-testid='frame-panel'] [data-testid='panel-title']").exists()).toBe(false);
    });

    test("lets an open list go when a thread pill opens a thread in its place, with no way back to it", async () => {
        const w = await mountBlock(1200);
        await openRow(w, team.channel_id);
        timelineOf(team.channel_id).messages.push(storedMessage(40, { reply_count: 2, last_reply_at: new Date().toISOString(), repliers: [] } as Partial<TimelineMessage>));
        await settle();
        await w.get("[data-testid='header-threads']").trigger("click");
        await settle();

        await w.get("[data-testid='frame-main'] [data-testid='thread-summary']").trigger("click");
        await settle();

        expect((mockShell!.threads as { closeList: jest.Mock }).closeList).toHaveBeenCalledTimes(1);
        expect(w.get("[data-testid='frame-panel'] [data-testid='panel-title']").text()).toBe("Thread");
        expect(w.find("[data-testid='frame-panel'] [data-testid='panel-back']").exists()).toBe(false);
    });

    test("on a phone, Threads is in the header's overflow menu, not a button of its own", async () => {
        const w = await mountBlock(390);
        await openRow(w, team.channel_id);

        expect(w.find("[data-testid='header-threads']").exists()).toBe(false);

        await w.get("[data-testid='header-more'] button").trigger("click");
        await settle();
        const items = Array.from(document.querySelectorAll<HTMLElement>("[role='menu'] [role='menuitem']"));
        expect(items.map(i => i.textContent?.trim())).toContain("Threads");
    });

    test("on a phone, the panel is the third pane, left by its back chevron", async () => {
        const w = await mountBlock(390);
        await openRow(w, team.channel_id);

        await w.get("[data-testid='header-more'] button").trigger("click");
        await settle();
        Array.from(document.querySelectorAll<HTMLElement>("[role='menu'] [role='menuitem']")).find(i => i.textContent?.trim() === "Threads")!.click();
        await settle();

        expect(w.get("[data-testid='chat-frame']").attributes("data-pane")).toBe("panel");
        expect(isPaneHidden(w, "frame-sidebar")).toBe(true);
        expect(isPaneHidden(w, "frame-main")).toBe(true);
        expect(isPaneHidden(w, "frame-panel")).toBe(false);
        expect(w.find("[data-testid='panel-close']").exists()).toBe(false);

        const back = w.get("[data-testid='panel-back']");
        expect(back.attributes("aria-label")).toBe("Back");
        await back.trigger("click");
        await settle();

        expect(w.get("[data-testid='chat-frame']").attributes("data-pane")).toBe("conversation");
        expect(isPaneHidden(w, "frame-main")).toBe(false);
        expect(isPaneHidden(w, "frame-panel")).toBe(true);
    });
});

describe("the thread drawer", () => {
    // A thread pill, the "Replied to a thread" label over a reply and a row of the threads list
    // each open the thread drawer in the panel column at the thread's root, through
    // shell.threads.openThread(channelId, rootId). Closing it lets the thread go through
    // closeThread and returns focus to what opened it; one opened from the list goes back to the
    // list by its "Back to Threads" chevron, to the row that opened it.

    const threadsOf = (): Record<string, jest.Mock> => mockShell!.threads as Record<string, jest.Mock>;

    function panelOpen(w: VueWrapper): string | undefined {
        return w.get("[data-testid='chat-frame']").attributes("data-panel-open");
    }

    async function openWithRoot(w: VueWrapper, extra: TimelineMessage[] = []): Promise<void> {
        await openRow(w, team.channel_id);
        timelineOf(team.channel_id).messages.push(storedMessage(40, { reply_count: 2, last_reply_at: new Date().toISOString(), repliers: [] } as Partial<TimelineMessage>), ...extra);
        await settle();
    }

    test("opens from a thread pill at its root, titled by the conversation, and closes back to the pill", async () => {
        const w = await mountBlock(1200);
        await openWithRoot(w);

        const pill = w.get("[data-testid='frame-main'] [data-testid='thread-summary']");
        await pill.trigger("click");
        await settle();

        expect(threadsOf().openThread).toHaveBeenCalledWith(team.channel_id, 40);
        expect(panelOpen(w)).toBe("true");
        expect(w.get("[data-testid='frame-panel'] [data-testid='panel-title']").text()).toBe("Thread");
        expect(w.get("[data-testid='frame-panel'] [data-testid='panel-subtitle']").text()).toBe("Worship Team");

        await w.get("[data-testid='frame-panel'] [data-testid='panel-close']").trigger("click");
        await settle();

        expect(threadsOf().closeThread).toHaveBeenCalledTimes(1);
        expect(panelOpen(w)).toBe("false");
        expect(document.activeElement).toBe(w.get("[data-testid='frame-main'] [data-testid='thread-summary']").element);
    });

    test("opens from the \"Replied to a thread\" label over a reply also sent to the conversation", async () => {
        const w = await mountBlock(1200);
        await openWithRoot(w, [storedMessage(41, { parent_id: 40, shown_in_channel: true } as Partial<TimelineMessage>)]);

        await w.get("[data-testid='frame-main'] [data-testid='thread-reply-label']").trigger("click");
        await settle();

        expect(threadsOf().openThread).toHaveBeenCalledWith(team.channel_id, 40);
        expect(panelOpen(w)).toBe("true");
        expect(w.get("[data-testid='frame-panel'] [data-testid='panel-title']").text()).toBe("Thread");
    });

    test("sends a reply to the root through the shell, as asked for the conversation", async () => {
        const w = await mountBlock(1200);
        await openWithRoot(w);
        await w.get("[data-testid='frame-main'] [data-testid='thread-summary']").trigger("click");
        await settle();

        const input = w.get("[data-testid='frame-panel'] [data-testid='composer-input']");
        await w.get("[data-testid='frame-panel'] [data-testid='composer-also'] input").setValue(true);
        // typed as a browser types into the editor: the text, then input
        input.element.textContent = "Count me in";
        input.element.dispatchEvent(new Event("input", { bubbles: true }));
        await nextTick();
        await input.trigger("keydown", { key: "Enter" });

        expect(mockShell!.sendReply).toHaveBeenCalledWith(40, "Count me in", true);
    });

    test("opens from a row of the threads list with a way back to the list and the row", async () => {
        const w = await mountBlock(1200);
        await openWithRoot(w);
        await w.get("[data-testid='header-threads']").trigger("click");
        await settle();

        const root = timelineOf(team.channel_id).messages[0];
        (threadsOf().state as unknown as { list: { rows: unknown[] } }).list.rows = [{
            channel_id: team.channel_id, root, reply_count: 2, last_reply_at: root.created_at, last_reply_id: 42,
            participant_count: 2, following: true, has_unread: false, mention_count: 0
        }];
        await settle();

        await w.get("[data-testid='thread-row-open'][data-root-id='40']").trigger("click");
        await settle();

        expect(threadsOf().openThread).toHaveBeenCalledWith(team.channel_id, 40);
        const back = w.get("[data-testid='frame-panel'] [data-testid='panel-back']");
        expect(back.attributes("aria-label")).toBe("Back to Threads");

        await back.trigger("click");
        await settle();

        expect(threadsOf().closeThread).toHaveBeenCalledTimes(1);
        expect(panelOpen(w)).toBe("true");
        expect(w.get("[data-testid='frame-panel'] [data-testid='panel-title']").text()).toBe("Threads");
        expect(document.activeElement).toBe(w.get("[data-testid='thread-row-open'][data-root-id='40']").element);
    });

    /** Opens the threads list on its All tab over the team, holding one thread of the direct message with Anna. */
    async function openAllTabWithAnnasThread(w: VueWrapper): Promise<void> {
        await openWithRoot(w);
        if (w.find("[data-testid='header-threads']").exists()) {
            await w.get("[data-testid='header-threads']").trigger("click");
        }
        else {
            await w.get("[data-testid='header-more'] button").trigger("click");
            await settle();
            Array.from(document.querySelectorAll<HTMLElement>("[role='menu'] [role='menuitem']")).find(i => i.textContent?.trim() === "Threads")!.click();
        }
        await settle();

        const root = storedMessage(70, { reply_count: 1 } as Partial<TimelineMessage>);
        timelineOf(dmAnna.channel_id).messages.push(root);
        const list = (threadsOf().state as unknown as { list: { tab: string, rows: unknown[] } }).list;
        list.tab = "all";
        list.rows = [{
            channel_id: dmAnna.channel_id, root, reply_count: 1, last_reply_at: root.created_at, last_reply_id: 71,
            participant_count: 2, following: true, has_unread: false, mention_count: 0
        }];
        await settle();
    }

    test("opens a thread of another conversation from the All tab over that conversation, and goes back to the All tab", async () => {
        const w = await mountBlock(1200);
        await openAllTabWithAnnasThread(w);

        await w.get("[data-testid='thread-row-open'][data-root-id='70']").trigger("click");
        await settle();

        expect(mockShell!.selectChannel).toHaveBeenLastCalledWith(dmAnna.channel_id);
        expect((mockShell!.state as { activeChannelId: string }).activeChannelId).toBe(dmAnna.channel_id);
        expect(w.get("[data-testid='header-title']").text()).toContain("Anna Kim");
        expect(threadsOf().openThread).toHaveBeenCalledWith(dmAnna.channel_id, 70);
        expect(threadsOf().closeThread).not.toHaveBeenCalled();
        expect(panelOpen(w)).toBe("true");
        expect(w.get("[data-testid='frame-panel'] [data-testid='panel-title']").text()).toBe("Thread");

        await w.get("[data-testid='frame-panel'] [data-testid='panel-back']").trigger("click");
        await settle();

        expect(w.get("[data-testid='frame-panel'] [data-testid='panel-title']").text()).toBe("Threads");
        expect((threadsOf().state as unknown as { list: { tab: string } }).list.tab).toBe("all");
        expect(document.activeElement).toBe(w.get("[data-testid='thread-row-open'][data-root-id='70']").element);

        // The conversation behind the list is now Anna's, so its own tab lists her threads.
        await w.findAll("[data-testid='frame-panel'] [role='tab']").find(t => t.attributes("aria-selected") === "false")!.trigger("click");
        await settle();

        expect(threadsOf().openList).toHaveBeenLastCalledWith(dmAnna.channel_id, "channel");
    });

    test("marks a private channel's row on the All tab with its lock, as the sidebar does", async () => {
        const w = await mountBlock(1200);
        await openAllTabWithAnnasThread(w);
        const root = timelineOf(team.channel_id).messages[0];
        (threadsOf().state as unknown as { list: { rows: unknown[] } }).list.rows = [{
            channel_id: team.channel_id, root, reply_count: 2, last_reply_at: root.created_at, last_reply_id: 42,
            participant_count: 2, following: true, has_unread: false, mention_count: 0
        }];
        await settle();

        const sub = w.get("[data-testid='thread-row'] .chat-ls-sub");
        expect(team.is_public).toBe(false);
        // The same shape as the sidebar row's icon; only its size differs.
        const sidebarIcon = w.get(`[data-testid='channel-row'][data-channel-id='${team.channel_id}'] .chat-cs-slot svg`);
        expect(sub.get("svg").element.innerHTML).toBe(sidebarIcon.element.innerHTML);
    });

    test("on a phone, a thread of another conversation opened from the All tab keeps the panel pane shown", async () => {
        const w = await mountBlock(390);
        await openAllTabWithAnnasThread(w);

        await w.get("[data-testid='thread-row-open'][data-root-id='70']").trigger("click");
        await settle();

        expect((mockShell!.state as { activeChannelId: string }).activeChannelId).toBe(dmAnna.channel_id);
        expect(w.get("[data-testid='chat-frame']").attributes("data-pane")).toBe("panel");
        expect(isPaneHidden(w, "frame-panel")).toBe(false);
        expect(w.get("[data-testid='frame-panel'] [data-testid='panel-title']").text()).toBe("Thread");
    });

    test("on a phone, takes focus into the drawer at its back chevron when a pill opens it, never the composer", async () => {
        const w = await mountBlock(390);
        await openWithRoot(w);

        (w.get("[data-testid='frame-main'] [data-testid='thread-summary']").element as HTMLElement).focus();
        await w.get("[data-testid='frame-main'] [data-testid='thread-summary']").trigger("click");
        await settle();
        await new Promise(resolve => setTimeout(resolve, 150));
        await settle();

        expect(document.activeElement).toBe(w.get("[data-testid='frame-panel'] [data-testid='panel-back']").element);
    });

    test("on a phone, takes focus into the drawer at its back chevron when a threads list row opens it", async () => {
        const w = await mountBlock(390);
        await openAllTabWithAnnasThread(w);

        (w.get("[data-testid='thread-row-open'][data-root-id='70']").element as HTMLElement).focus();
        await w.get("[data-testid='thread-row-open'][data-root-id='70']").trigger("click");
        await settle();
        await new Promise(resolve => setTimeout(resolve, 150));
        await settle();

        expect(document.activeElement).toBe(w.get("[data-testid='frame-panel'] [data-testid='panel-back']").element);
    });

    test("closes when another conversation is chosen in the sidebar", async () => {
        const w = await mountBlock(1200);
        await openWithRoot(w);
        await w.get("[data-testid='frame-main'] [data-testid='thread-summary']").trigger("click");
        await settle();

        await openRow(w, dmAnna.channel_id);

        expect(threadsOf().closeThread).toHaveBeenCalledTimes(1);
        expect(panelOpen(w)).toBe("false");
    });

    test("leaves out the drawer's reply still on its way from the conversation", async () => {
        const w = await mountBlock(1200);
        await openWithRoot(w);
        const sending = { localId: "mine", channelId: team.channel_id, body: "On my way", status: "sending", errorCode: null, parentId: 40, isShownInChannel: false };
        (mockShell!.sender as { pending: () => unknown[] }).pending = () => [sending];
        await w.get("[data-testid='frame-main'] [data-testid='thread-summary']").trigger("click");
        await settle();

        expect(w.findAll("[data-testid='frame-main'] [data-testid='pending-row']")).toHaveLength(0);
        expect(w.findAll("[data-testid='frame-panel'] [data-testid='pending-row']")).toHaveLength(1);
    });
});

describe("mentions", () => {
    // The header's Mentions opens the shell's inbox, shell.mentions, in the panel column beside
    // the conversation, or as the third pane on a phone. A row is opened through the shell's
    // jump, which lands a channel mention in its conversation through the frame's view and opens a
    // reply's thread through it, with a way back to the inbox. The composers' @ queries search
    // the people of the conversation they are in, from the room's sidebar row.

    type MentionsStandIn = Record<string, jest.Mock> & { state: { inbox: { rows: MentionRow[] }, hasDot: boolean, picker: { people: unknown[], isAllOffered: boolean } } };

    const mentionsOf = (): MentionsStandIn => mockShell!.mentions as MentionsStandIn;

    function panelTitle(w: VueWrapper): string {
        return w.get("[data-testid='frame-panel'] [data-testid='panel-title']").text();
    }

    function mention(channelId: string, id: number, parentId: number | null = null): MentionRow {
        return { channel_id: channelId, parent_id: parentId, message: storedMessage(id, { parent_id: parentId }) };
    }

    async function openInbox(w: VueWrapper, rows: MentionRow[] = []): Promise<void> {
        mentionsOf().state.inbox.rows = rows;
        if (w.find("[data-testid='header-mentions']").exists()) {
            await w.get("[data-testid='header-mentions']").trigger("click");
        }
        else {
            await w.get("[data-testid='header-more'] button").trigger("click");
            await settle();
            Array.from(document.querySelectorAll<HTMLElement>("[role='menu'] [role='menuitem']")).find(i => i.textContent?.trim() === "Mentions")!.click();
        }
        await settle();
    }

    test("the header's Mentions opens the inbox beside the conversation and closes it again", async () => {
        const w = await mountBlock(1200);
        await openRow(w, team.channel_id);

        await openInbox(w);

        expect(mentionsOf().openInbox).toHaveBeenCalledTimes(1);
        expect(w.get("[data-testid='chat-frame']").attributes("data-panel-open")).toBe("true");
        expect(panelTitle(w)).toBe("Mentions");
        expect(w.get("[data-testid='header-mentions']").attributes("aria-expanded")).toBe("true");

        await w.get("[data-testid='header-mentions']").trigger("click");
        await settle();

        expect(mentionsOf().closeInbox).toHaveBeenCalled();
        expect(w.get("[data-testid='chat-frame']").attributes("data-panel-open")).toBe("false");
    });

    test("on a phone, opened from the overflow menu, puts focus on its back chevron with no tooltip, as the threads list shows none", async () => {
        const w = await mountBlock(390);
        await openRow(w, team.channel_id);

        await openInbox(w);
        await new Promise(resolve => setTimeout(resolve, 700));
        await settle();

        expect(panelTitle(w)).toBe("Mentions");
        expect(document.activeElement).toBe(w.get("[data-testid='frame-panel'] [data-testid='panel-back']").element);
        expect(Array.from(document.querySelectorAll("[role='tooltip']")).map(tip => tip.textContent?.trim())).toEqual([]);
    });

    test("the header's Mentions shows the shell's dot", async () => {
        const w = await mountBlock(1200);
        await openRow(w, team.channel_id);
        expect(w.find("[data-testid='header-mentions-dot']").exists()).toBe(false);

        mentionsOf().state.hasDot = true;
        await settle();

        expect(w.find("[data-testid='header-mentions-dot']").exists()).toBe(true);
    });

    test("the threads list takes the inbox's place, letting the inbox go", async () => {
        const w = await mountBlock(1200);
        await openRow(w, team.channel_id);
        await openInbox(w);

        await w.get("[data-testid='header-threads']").trigger("click");
        await settle();

        expect(mentionsOf().closeInbox).toHaveBeenCalled();
        expect(panelTitle(w)).toBe("Threads");
    });

    test("a channel mention's row lands on the message in its conversation, the inbox staying beside it", async () => {
        const w = await mountBlock(1200);
        await openRow(w, team.channel_id);
        const row = mention(dmAnna.channel_id, 30);
        await openInbox(w, [row]);

        await w.get("[data-testid='mention-row-open']").trigger("click");
        await settle();

        expect(mentionsOf().jump).toHaveBeenCalledWith(row);
        expect((mockShell!.state as { activeChannelId: string }).activeChannelId).toBe(dmAnna.channel_id);
        expect(w.findComponent(MessageFeedComponent).props("landOnId")).toBe(30);
        expect(panelTitle(w)).toBe("Mentions");
    });

    test("on a phone, a channel mention's row closes the inbox and shows the conversation", async () => {
        const w = await mountBlock(390);
        await openRow(w, team.channel_id);
        await openInbox(w, [mention(dmAnna.channel_id, 30)]);
        expect(w.get("[data-testid='chat-frame']").attributes("data-pane")).toBe("panel");

        await w.get("[data-testid='mention-row-open']").trigger("click");
        await settle();

        expect(w.get("[data-testid='chat-frame']").attributes("data-pane")).toBe("conversation");
        expect(w.findComponent(MessageFeedComponent).props("landOnId")).toBe(30);
    });

    test("a reply's row opens its thread with a way back to the inbox and the row", async () => {
        const w = await mountBlock(1200);
        await openRow(w, team.channel_id);
        await openInbox(w, [mention(team.channel_id, 41, 40)]);

        await w.get("[data-testid='mention-row-open']").trigger("click");
        await settle();

        const threads = mockShell!.threads as Record<string, jest.Mock>;
        expect(threads.openThread).toHaveBeenCalledWith(team.channel_id, 40);
        expect(mentionsOf().threadOpened).toHaveBeenCalledWith(team.channel_id, 40);
        expect(panelTitle(w)).toBe("Thread");
        const back = w.get("[data-testid='frame-panel'] [data-testid='panel-back']");
        expect(back.attributes("aria-label")).toBe("Back to Mentions");

        await back.trigger("click");
        await settle();

        expect(threads.closeThread).toHaveBeenCalledTimes(1);
        expect(mentionsOf().closeInbox).not.toHaveBeenCalled();
        expect(panelTitle(w)).toBe("Mentions");
        expect(document.activeElement).toBe(w.get("[data-testid='mention-row-open']").element);
    });

    test("a thread opened from the conversation tells the mentions, so its replies stop lighting the dot", async () => {
        const w = await mountBlock(1200);
        await openRow(w, team.channel_id);
        timelineOf(team.channel_id).messages.push(storedMessage(40, { reply_count: 2, last_reply_at: new Date().toISOString(), repliers: [] } as Partial<TimelineMessage>));
        await settle();

        await w.get("[data-testid='frame-main'] [data-testid='thread-summary']").trigger("click");
        await settle();

        expect(mentionsOf().threadOpened).toHaveBeenCalledWith(team.channel_id, 40);
    });

    test("the composer's @ query searches the conversation's people from its row, and its end closes the search", async () => {
        const w = await mountBlock(1200);
        await openRow(w, team.channel_id);
        mentionsOf().state.picker.isAllOffered = true;
        await settle();

        const composer = w.findComponent(ComposerComponent);
        expect(composer.props("mentionSource")).toEqual({ canMention: true, people: [], isAllOffered: true });

        composer.vm.$emit("mentionQuery", "an");
        expect(mentionsOf().search).toHaveBeenCalledWith(team, "an");

        composer.vm.$emit("mentionQuery", null);
        expect(mentionsOf().closeSearch).toHaveBeenCalledTimes(1);
    });

    test("the composer's picker is told while its answer is on its way, so it draws nothing stale", async () => {
        const w = await mountBlock(1200);
        await openRow(w, team.channel_id);
        Object.assign(mentionsOf().state.picker, { isAwaitingAnswer: true, isAwaitingFirstAnswer: true });
        await settle();

        const composer = w.findComponent(ComposerComponent);
        expect(composer.props("mentionSource")).toEqual(expect.objectContaining({ isAwaitingAnswer: true, isAwaitingFirstAnswer: true }));

        Object.assign(mentionsOf().state.picker, { isAwaitingAnswer: false, isAwaitingFirstAnswer: false });
        await settle();
        expect(composer.props("mentionSource")).toEqual(expect.objectContaining({ isAwaitingAnswer: false, isAwaitingFirstAnswer: false }));
    });

    test("the thread's reply composer searches the people of the thread's conversation", async () => {
        const w = await mountBlock(1200);
        await openRow(w, team.channel_id);
        timelineOf(team.channel_id).messages.push(storedMessage(40, { reply_count: 2, last_reply_at: new Date().toISOString(), repliers: [] } as Partial<TimelineMessage>));
        await settle();
        await w.get("[data-testid='frame-main'] [data-testid='thread-summary']").trigger("click");
        await settle();

        const drawer = w.findComponent(ThreadDrawerComponent);
        expect(drawer.props("mentionSource")).toEqual({ canMention: true, people: [], isAllOffered: false });

        drawer.vm.$emit("mentionQuery", "b");
        expect(mentionsOf().search).toHaveBeenCalledWith(team, "b");
    });
});

describe("a named group direct message", () => {
    // The platform names a direct message by its people only when it has no name, so a named group
    // arrives with no people and a count of others of 0. Only a group can be named, so it is still
    // a group: each run of messages is headed by its sender, in the conversation and its threads.

    const hike: SidebarRow = sidebarRow({
        channel_id: "c0000009-0000-4000-8000-000000000000",
        channel_type: "dm",
        name: "Hike planning",
        dm_names: [],
        dm_other_count: 0
    });

    async function openHike(w: VueWrapper): Promise<void> {
        (mockShell!.channels as { rows: SidebarRow[] }).rows.push(hike);
        timelineOf(hike.channel_id).messages.push(storedMessage(50, { reply_count: 1, last_reply_at: new Date().toISOString(), repliers: [] } as Partial<TimelineMessage>));
        await settle();
        await openRow(w, hike.channel_id);
    }

    test("heads each run of messages with its sender's name", async () => {
        const w = await mountBlock(1200);
        await openHike(w);

        expect(w.get("[data-testid='frame-main'] [data-testid='sender-name']").text()).toBe("Anna Kim");
    });

    test("writes no head count under its name until the room's facts are read, then counts its members", async () => {
        const w = await mountBlock(1200);
        await openHike(w);

        expect(w.get("[data-testid='frame-main'] [data-testid='header-title']").text()).toContain("Hike planning");
        expect(w.find("[data-testid='frame-main'] [data-testid='header-subtitle']").exists()).toBe(false);

        roomsOf().state.facts[hike.channel_id] = roomFacts({ channel_id: hike.channel_id, channel_type: "dm", name: "Hike planning", member_count: 4 });
        await settle();

        expect(w.get("[data-testid='frame-main'] [data-testid='header-subtitle']").text()).toBe("4 people");
    });

    test("an unnamed group still counts its people with the reader under its title", async () => {
        const w = await mountBlock(1200);
        const trio = sidebarRow({
            channel_id: "c0000010-0000-4000-8000-000000000000",
            channel_type: "dm",
            dm_names: ["Anna Kim", "Ben Ortiz"],
            dm_other_count: 2
        });
        (mockShell!.channels as { rows: SidebarRow[] }).rows.push(trio);
        await settle();
        await openRow(w, trio.channel_id);

        expect(w.get("[data-testid='frame-main'] [data-testid='header-subtitle']").text()).toBe("3 people");
    });

    test("heads each run of replies in its thread drawer with its sender's name", async () => {
        const w = await mountBlock(1200);
        await openHike(w);
        await w.get("[data-testid='frame-main'] [data-testid='thread-summary']").trigger("click");
        await settle();
        const thread = (mockShell!.threads as { state: { thread: { replies: TimelineMessage[] } } }).state.thread;
        thread.replies.push(storedMessage(51, { parent_id: 50, person_alias_guid: "a0000003-0000-4000-8000-000000000000", sender_nick_name: "Ben", sender_last_name: "Ortiz" }));
        await settle();

        const names = w.findAll("[data-testid='frame-panel'] .chat-grp [data-testid='sender-name']").map(n => n.text());
        expect(names).toContain("Ben Ortiz");
    });
});

describe("a push about a thread reply", () => {
    // The push worker posts to a window of the site through its service worker container, in the
    // worker's own names and with every value a string, as the push carried it: a tap posts
    // chat.push.open with the channel, the reply's root and the reply; the ask before a banner
    // posts chat.push.is-open with the channel and, for a reply, its root, and reads the answer on
    // the port it hands over. A page opened by a tap with no chat window open carries the same
    // three in its link, which reach the block through the init box.

    const threadsOf = (): Record<string, jest.Mock> => mockShell!.threads as Record<string, jest.Mock>;

    /** The block's listeners on the page's service worker container. */
    let workerListeners: Array<(event: MessageEvent) => void> = [];

    let scrolledTo: Element[] = [];
    const scrollIntoViewBefore = Element.prototype.scrollIntoView;

    beforeEach(() => {
        workerListeners = [];
        Object.defineProperty(navigator, "serviceWorker", {
            configurable: true,
            value: {
                addEventListener: (type: string, listener: (event: MessageEvent) => void) => {
                    if (type === "message") {
                        workerListeners.push(listener);
                    }
                },
                removeEventListener: (_type: string, listener: (event: MessageEvent) => void) => {
                    workerListeners = workerListeners.filter(l => l !== listener);
                }
            }
        });

        // jsdom lays nothing out and has no scrollIntoView, so where the block scrolls to is recorded.
        scrolledTo = [];
        Element.prototype.scrollIntoView = function (this: Element): void {
            scrolledTo.push(this);
        };
    });

    afterEach(() => {
        delete (navigator as unknown as Record<string, unknown>).serviceWorker;
        Element.prototype.scrollIntoView = scrollIntoViewBefore;
    });

    /** Posts a message from the worker, with a port for an ask's answer. */
    function fromWorker(data: Record<string, string>): jest.Mock {
        const port = { postMessage: jest.fn() };
        for (const listener of workerListeners) {
            listener({ data, ports: [port] } as unknown as MessageEvent);
        }

        return port.postMessage;
    }

    /** The drawer's row of a reply, or null when it is not drawn. */
    function replyRow(w: VueWrapper, id: number): Element | null {
        return w.element.querySelector(`[data-testid='frame-panel'] [data-reply-id='${id}']`);
    }

    function isFlashing(w: VueWrapper, id: number): boolean {
        return !!replyRow(w, id)?.classList.contains("is-flashing");
    }

    /** The panes the frame told the shell are out of view. */
    function hiddenPanes(): { conversation: boolean; thread: boolean } {
        return (mockShell!.state as unknown as { hiddenPanes: { conversation: boolean; thread: boolean } }).hiddenPanes;
    }

    function flashingRows(w: VueWrapper): Element[] {
        return Array.from(w.element.querySelectorAll("[data-testid='frame-panel'] .is-flashing"));
    }

    /** Answers the oldest older-page call the block made. */
    async function answerOlder(): Promise<void> {
        const answer = mockOlderAnswers.shift();
        if (!answer) {
            throw new Error("no older page asked for");
        }
        answer();
        await settle();
    }

    test("a tap opens the reply's conversation and the drawer at its root, scrolled to the reply and flashing it", async () => {
        mockThreadPages = { newest: [41, 42, 43], older: [] };
        const w = await mountBlock(1200);

        fromWorker({ type: "chat.push.open", channel_id: team.channel_id, root_id: "40", message_id: "42" });
        await settle();
        await settle();

        expect(mockShell!.selectChannel).toHaveBeenCalledWith(team.channel_id);
        expect(threadsOf().openThread).toHaveBeenCalledWith(team.channel_id, 40);
        expect(w.get("[data-testid='chat-frame']").attributes("data-panel-open")).toBe("true");
        expect(w.get("[data-testid='frame-panel'] [data-testid='panel-title']").text()).toBe("Thread");
        expect(isFlashing(w, 42)).toBe(true);
        expect(flashingRows(w)).toHaveLength(1);
        expect(scrolledTo).toContain(replyRow(w, 42));
        expect(threadsOf().loadOlderReplies).not.toHaveBeenCalled();
    });

    test("a reply older than the newest page is found by loading older pages until it appears", async () => {
        mockThreadPages = { newest: [50, 51], older: [[44, 45], [41, 42, 43]] };
        const w = await mountBlock(1200);

        fromWorker({ type: "chat.push.open", channel_id: team.channel_id, root_id: "40", message_id: "42" });
        await settle();
        await settle();

        expect(threadsOf().loadOlderReplies).toHaveBeenCalledTimes(1);
        expect(flashingRows(w)).toHaveLength(0);

        await answerOlder();
        await settle();

        expect(threadsOf().loadOlderReplies).toHaveBeenCalledTimes(2);
        expect(flashingRows(w)).toHaveLength(0);

        await answerOlder();
        await settle();

        expect(threadsOf().loadOlderReplies).toHaveBeenCalledTimes(2);
        expect(isFlashing(w, 42)).toBe(true);
        expect(scrolledTo).toContain(replyRow(w, 42));
    });

    test("a reply not found in two older pages leaves the drawer at its newest, flashing nothing", async () => {
        mockThreadPages = { newest: [50], older: [[47, 48], [45, 46], [43, 44], [41, 42]] };
        const w = await mountBlock(1200);

        fromWorker({ type: "chat.push.open", channel_id: team.channel_id, root_id: "40", message_id: "42" });
        await settle();
        await settle();
        await answerOlder();
        await settle();
        await answerOlder();
        await settle();
        await settle();

        expect(threadsOf().loadOlderReplies).toHaveBeenCalledTimes(2);
        expect(mockOlderAnswers).toHaveLength(0);
        expect(flashingRows(w)).toHaveLength(0);
        expect(scrolledTo.filter(e => e.hasAttribute("data-reply-id"))).toEqual([]);
        expect(w.get("[data-testid='chat-frame']").attributes("data-panel-open")).toBe("true");
    });

    test("a drawer closed while an older page is on its way asks for no more and flashes nothing", async () => {
        mockThreadPages = { newest: [50], older: [[44, 45], [41, 42, 43]] };
        const w = await mountBlock(1200);

        fromWorker({ type: "chat.push.open", channel_id: team.channel_id, root_id: "40", message_id: "42" });
        await settle();
        await settle();
        expect(threadsOf().loadOlderReplies).toHaveBeenCalledTimes(1);

        await w.get("[data-testid='frame-panel'] [data-testid='panel-close']").trigger("click");
        await settle();
        await answerOlder();
        await settle();

        expect(threadsOf().loadOlderReplies).toHaveBeenCalledTimes(1);
        expect(flashingRows(w)).toHaveLength(0);
    });

    test("a second tap on the same reply while its thread is open scrolls to it and flashes it again", async () => {
        mockThreadPages = { newest: [41, 42, 43], older: [] };
        const w = await mountBlock(1200);

        fromWorker({ type: "chat.push.open", channel_id: team.channel_id, root_id: "40", message_id: "42" });
        await settle();
        await settle();
        expect(isFlashing(w, 42)).toBe(true);

        // the first flash plays out, as a browser ends it
        replyRow(w, 42)!.dispatchEvent(Object.assign(new Event("animationend", { bubbles: true }), { animationName: "chat-cm-flash" }));
        await settle();
        expect(flashingRows(w)).toHaveLength(0);

        fromWorker({ type: "chat.push.open", channel_id: team.channel_id, root_id: "40", message_id: "42" });
        await settle();
        await settle();

        expect(isFlashing(w, 42)).toBe(true);
        expect(scrolledTo.filter(e => e === replyRow(w, 42))).toHaveLength(2);
    });

    test("a tap on another thread while an older page is on its way lands the new one, and the first asks for no more and flashes nothing", async () => {
        mockThreadPages = { newest: [50, 62], older: [[44, 45], [41, 42, 43]] };
        const w = await mountBlock(1200);

        fromWorker({ type: "chat.push.open", channel_id: team.channel_id, root_id: "40", message_id: "42" });
        await settle();
        await settle();
        expect(threadsOf().loadOlderReplies).toHaveBeenCalledTimes(1);

        fromWorker({ type: "chat.push.open", channel_id: team.channel_id, root_id: "60", message_id: "62" });
        await settle();
        await settle();

        expect(threadsOf().openThread).toHaveBeenLastCalledWith(team.channel_id, 60);
        expect(isFlashing(w, 62)).toBe(true);
        expect(scrolledTo).toContain(replyRow(w, 62));

        await answerOlder();
        await settle();

        expect(threadsOf().loadOlderReplies).toHaveBeenCalledTimes(1);
        expect(mockOlderAnswers).toHaveLength(0);
        expect(flashingRows(w).map(row => row.getAttribute("data-reply-id"))).toEqual(["62"]);
        expect(replyRow(w, 42)).toBeNull();
    });

    test("tells the shell the thread's pane is out of view while a phone shows the list over an open thread", async () => {
        mockThreadPages = { newest: [41, 42, 43], older: [] };
        const w = await mountBlock(390);
        const paneHidden = (): unknown => hiddenPanes().thread;
        await openRow(w, team.channel_id);

        fromWorker({ type: "chat.push.open", channel_id: team.channel_id, root_id: "40", message_id: "42" });
        await settle();
        await settle();
        expect(w.get("[data-testid='chat-frame']").attributes("data-pane")).toBe("panel");
        expect(paneHidden()).toBe(false);

        // Back to the list, the thread still open behind it
        await w.get("[data-testid='header-back']").trigger("click");
        await settle();
        expect(w.get("[data-testid='chat-frame']").attributes("data-pane")).toBe("list");
        expect(paneHidden()).toBe(true);

        FakeResizeObserver.resize(1200);
        await settle();
        expect(paneHidden()).toBe(false);
    });

    test("tells the shell the conversation's pane is out of view while a phone shows the list or a thread over it", async () => {
        mockThreadPages = { newest: [41, 42, 43], older: [] };
        const w = await mountBlock(390);
        await openRow(w, team.channel_id);
        expect(w.get("[data-testid='chat-frame']").attributes("data-pane")).toBe("conversation");
        expect(hiddenPanes().conversation).toBe(false);

        fromWorker({ type: "chat.push.open", channel_id: team.channel_id, root_id: "40", message_id: "42" });
        await settle();
        await settle();
        expect(w.get("[data-testid='chat-frame']").attributes("data-pane")).toBe("panel");
        expect(hiddenPanes().conversation).toBe(true);

        // on a desktop the conversation and the drawer beside it are both on screen
        FakeResizeObserver.resize(1200);
        await settle();
        expect(hiddenPanes()).toEqual({ conversation: false, thread: false });

        FakeResizeObserver.resize(390);
        await settle();
        await w.get("[data-testid='header-back']").trigger("click");
        await settle();
        expect(w.get("[data-testid='chat-frame']").attributes("data-pane")).toBe("list");
        expect(hiddenPanes()).toEqual({ conversation: true, thread: true });
    });

    test("on a phone, a reply's tap for the conversation open behind the list brings its thread to the front", async () => {
        mockThreadPages = { newest: [41, 42, 43], older: [] };
        const w = await mountBlock(390);
        await openRow(w, team.channel_id);
        await w.get("[data-testid='header-back']").trigger("click");
        await settle();
        expect(w.get("[data-testid='chat-frame']").attributes("data-pane")).toBe("list");

        fromWorker({ type: "chat.push.open", channel_id: team.channel_id, root_id: "40", message_id: "42" });
        await settle();
        await settle();

        expect(w.get("[data-testid='chat-frame']").attributes("data-pane")).toBe("panel");
        expect(w.get("[data-testid='frame-panel'] [data-testid='panel-title']").text()).toBe("Thread");
        expect(isPaneHidden(w, "frame-panel")).toBe(false);
        expect(isFlashing(w, 42)).toBe(true);
    });

    test("on a phone, a root's tap for the conversation open behind the list brings the conversation to the front", async () => {
        const w = await mountBlock(390);
        await openRow(w, team.channel_id);
        await w.get("[data-testid='header-back']").trigger("click");
        await settle();
        expect(w.get("[data-testid='chat-frame']").attributes("data-pane")).toBe("list");

        fromWorker({ type: "chat.push.open", channel_id: team.channel_id });
        await settle();

        expect(w.get("[data-testid='chat-frame']").attributes("data-pane")).toBe("conversation");
        expect(isPaneHidden(w, "frame-main")).toBe(false);
    });

    test("on a phone, a root's tap for the conversation under an open thread brings the conversation to the front", async () => {
        mockThreadPages = { newest: [41, 42, 43], older: [] };
        const w = await mountBlock(390);
        await openRow(w, team.channel_id);
        fromWorker({ type: "chat.push.open", channel_id: team.channel_id, root_id: "40", message_id: "42" });
        await settle();
        await settle();
        expect(w.get("[data-testid='chat-frame']").attributes("data-pane")).toBe("panel");

        fromWorker({ type: "chat.push.open", channel_id: team.channel_id });
        await settle();

        expect(w.get("[data-testid='chat-frame']").attributes("data-pane")).toBe("conversation");
        expect(threadsOf().closeThread).toHaveBeenCalled();
    });

    test("on a phone, a root's tap for another conversation over an open thread brings that conversation to the front, focus on its title", async () => {
        mockThreadPages = { newest: [41, 42, 43], older: [] };
        const w = await mountBlock(390);
        await openRow(w, team.channel_id);
        timelineOf(team.channel_id).messages.push(storedMessage(40, { reply_count: 3, last_reply_at: new Date().toISOString(), repliers: [] } as Partial<TimelineMessage>));
        fromWorker({ type: "chat.push.open", channel_id: team.channel_id, root_id: "40", message_id: "42" });
        await settle();
        await settle();
        expect(w.get("[data-testid='chat-frame']").attributes("data-pane")).toBe("panel");
        (w.get("[data-testid='frame-panel'] [data-testid='panel-back']").element as HTMLElement).focus();

        // The shell today names the new conversation before its first await; this one names it
        // only after the tap is handled, so where focus lands is shown not to depend on when the
        // swap comes.
        let swap: (() => void) | null = null;
        mockShell!.selectChannel = jest.fn((channelId: string) => new Promise<void>(resolve => {
            swap = (): void => {
                (mockShell!.state as { activeChannelId: string | null }).activeChannelId = channelId;
                resolve();
            };
        }));

        fromWorker({ type: "chat.push.open", channel_id: dmAnna.channel_id });
        await settle();
        swap!();
        await settle();
        await settle();

        expect(mockShell!.selectChannel).toHaveBeenCalledWith(dmAnna.channel_id);
        expect(w.get("[data-testid='chat-frame']").attributes("data-pane")).toBe("conversation");
        expect(w.get("[data-testid='chat-frame']").attributes("data-panel-open")).toBe("false");
        expect(threadsOf().closeThread).toHaveBeenCalled();
        expect(document.activeElement).not.toBe(document.body);
        expect(document.activeElement).toBe(w.get("[data-testid='header-details']").element);
    });

    test("on a desktop, a root's tap leaves an open thread's drawer beside the conversation", async () => {
        mockThreadPages = { newest: [41, 42, 43], older: [] };
        const w = await mountBlock(1200);
        await openRow(w, team.channel_id);
        fromWorker({ type: "chat.push.open", channel_id: team.channel_id, root_id: "40", message_id: "42" });
        await settle();
        await settle();

        fromWorker({ type: "chat.push.open", channel_id: team.channel_id });
        await settle();

        expect(w.get("[data-testid='chat-frame']").attributes("data-panel-open")).toBe("true");
        expect(threadsOf().closeThread).not.toHaveBeenCalled();
    });

    test("a tap on a root's push opens its conversation and no thread", async () => {
        await mountBlock(1200);

        fromWorker({ type: "chat.push.open", channel_id: team.channel_id });
        await settle();

        expect(mockShell!.selectChannel).toHaveBeenCalledWith(team.channel_id);
        expect(threadsOf().openThread).not.toHaveBeenCalled();
    });

    test("a page opened at a reply's link opens the drawer at its root, scrolled to the reply and flashing it", async () => {
        mockThreadPages = { newest: [41, 42, 43], older: [] };
        Object.assign(mockConfig, { channelGuid: team.channel_id.toUpperCase(), threadRootId: 40, messageId: 42 });
        const w = await mountBlock(1200);
        await settle();

        expect(threadsOf().openThread).toHaveBeenCalledWith(team.channel_id, 40);
        expect(w.get("[data-testid='chat-frame']").attributes("data-panel-open")).toBe("true");
        expect(isFlashing(w, 42)).toBe(true);
        expect(scrolledTo).toContain(replyRow(w, 42));
    });

    test("the ask with a root answers open only while that thread's drawer is shown", async () => {
        const w = await mountBlock(1200);
        await openRow(w, team.channel_id);
        timelineOf(team.channel_id).messages.push(storedMessage(40, { reply_count: 2, last_reply_at: new Date().toISOString(), repliers: [] } as Partial<TimelineMessage>));
        await settle();
        // the conversation itself is on screen throughout
        mockShell!.isChannelOnScreen = (channelId: string): boolean => channelId === team.channel_id;

        expect(fromWorker({ type: "chat.push.is-open", channel_id: team.channel_id, root_id: "40" })).toHaveBeenCalledWith({ open: false });

        await w.get("[data-testid='frame-main'] [data-testid='thread-summary']").trigger("click");
        await settle();

        expect(fromWorker({ type: "chat.push.is-open", channel_id: team.channel_id, root_id: "40" })).toHaveBeenCalledWith({ open: true });
        expect(fromWorker({ type: "chat.push.is-open", channel_id: team.channel_id, root_id: "41" })).toHaveBeenCalledWith({ open: false });

        await w.get("[data-testid='frame-panel'] [data-testid='panel-close']").trigger("click");
        await settle();

        expect(fromWorker({ type: "chat.push.is-open", channel_id: team.channel_id, root_id: "40" })).toHaveBeenCalledWith({ open: false });
    });

    test("the ask with no root still answers by the conversation on screen", async () => {
        const w = await mountBlock(1200);
        await openRow(w, team.channel_id);
        mockShell!.isChannelOnScreen = (channelId: string): boolean => channelId === team.channel_id;

        expect(fromWorker({ type: "chat.push.is-open", channel_id: team.channel_id })).toHaveBeenCalledWith({ open: true });
        expect(fromWorker({ type: "chat.push.is-open", channel_id: dmAnna.channel_id })).toHaveBeenCalledWith({ open: false });
    });
});

// A toast reports on the conversation, so it centres over the conversation's column, not the whole
// block: with a panel open beside it, a toast over the block would sit off the conversation. The
// toast's status region is drawn inside that column.
describe("the toasts", () => {
    test("are drawn inside the conversation's column, with a conversation open", async () => {
        const w = await mountBlock(1200);
        await openRow(w, team.channel_id);

        expect(w.find("[data-testid='error-toast']").exists()).toBe(true);
        expect(w.find("[data-testid='frame-main'] [data-testid='error-toast']").exists()).toBe(true);
    });

    test("stay inside the conversation's column with the threads panel open beside it", async () => {
        const w = await mountBlock(1200);
        await openRow(w, team.channel_id);
        await w.get("[data-testid='header-threads']").trigger("click");
        await settle();

        expect(w.find("[data-testid='frame-panel'] [data-testid='error-toast']").exists()).toBe(false);
        expect(w.find("[data-testid='frame-main'] [data-testid='error-toast']").exists()).toBe(true);
    });
});

// An editor closed, saved or cancelled, gives focus back to the composer of the place it was
// opened in, so the person carries on typing there; Up in the thread's composer edits the newest
// of their replies in that thread.
describe("edit in place", () => {
    const actionsOf = (): Record<string, jest.Mock> => mockShell!.messageActions as Record<string, jest.Mock>;

    test.each([["cancelled", "cancelEdit"], ["saved", "saveEdit"]] as const)("an editor %s gives focus back to the conversation's composer", async (_how, kind) => {
        const w = await mountBlock(1200);
        await openRow(w, team.channel_id);
        const target = storedMessage(7);

        w.findComponent(MessageFeedComponent).vm.$emit("messageAction", kind === "saveEdit" ? { kind, message: target, body: "fixed" } : { kind, message: target });
        await settle();
        await settle();

        if (kind === "saveEdit") {
            expect(actionsOf().saveEdit).toHaveBeenCalledWith("fixed");
        }
        else {
            expect(actionsOf().cancelEdit).toHaveBeenCalledTimes(1);
        }
        expect(document.activeElement).toBe(w.get("[data-testid='frame-main'] [data-testid='composer-input']").element);
    });

    test("Up in the thread's empty composer edits the newest of the person's replies in that thread", async () => {
        const w = await mountBlock(1200);
        await openRow(w, team.channel_id);
        timelineOf(team.channel_id).messages.push(storedMessage(40, { reply_count: 2, last_reply_at: new Date().toISOString(), repliers: [] } as Partial<TimelineMessage>));
        await settle();
        await w.get("[data-testid='frame-main'] [data-testid='thread-summary']").trigger("click");
        await settle();

        await w.get("[data-testid='frame-panel'] [data-testid='composer-input']").trigger("keydown", { key: "ArrowUp" });

        expect(actionsOf().startEditNewest).toHaveBeenCalledWith(team.channel_id, 40);
    });
});

// A reaction asked for anywhere in the block reaches the shell's reactions with the conversation
// it was asked in, and the picker opens beside what asked for it, one at a time.
describe("reactions", () => {
    const reactionsOf = (): Record<string, jest.Mock> => mockShell!.reactions as Record<string, jest.Mock>;

    test("a press of a message's reaction toggles it in the open conversation", async () => {
        const w = await mountBlock(1200);
        await openRow(w, team.channel_id);

        w.findComponent(MessageFeedComponent).vm.$emit("messageAction", { kind: "react", message: storedMessage(7), code: "pray" });
        await settle();

        expect(reactionsOf().toggleCode).toHaveBeenCalledWith(team.channel_id, 7, "pray");
    });

    test("the picker opens beside what asked for it, fetches the catalog, and a choice reacts, is kept as recent and closes", async () => {
        const w = await mountBlock(1200);
        await openRow(w, team.channel_id);
        const anchor = document.createElement("button");
        shellRoot(w).append(anchor);

        w.findComponent(MessageFeedComponent).vm.$emit("messageAction", { kind: "openPicker", message: storedMessage(7), anchor });
        await settle();

        expect(reactionsOf().loadCatalog).toHaveBeenCalledTimes(1);
        expect(anchor.getAttribute("aria-expanded")).toBe("true");
        const cell = document.querySelector<HTMLElement>("[role='dialog'][aria-label='Add reaction'] [role='gridcell'][data-code='pray']");
        expect(cell).not.toBeNull();

        cell?.click();
        await settle();
        await settle();

        expect(reactionsOf().toggleCode).toHaveBeenCalledWith(team.channel_id, 7, "pray");
        expect(reactionsOf().recordRecent).toHaveBeenCalledWith("pray");
        expect(document.querySelector("[role='dialog'][aria-label='Add reaction']")).toBeNull();
        expect(anchor.getAttribute("aria-expanded")).toBe("false");
    });

    const pickerOf = (): Element | null => document.querySelector("[role='dialog'][aria-label='Add reaction']");

    test("asked for again by what opened it, closes", async () => {
        const w = await mountBlock(1200);
        await openRow(w, team.channel_id);
        const anchor = document.createElement("button");
        shellRoot(w).append(anchor);
        const feed = w.findComponent(MessageFeedComponent);

        feed.vm.$emit("messageAction", { kind: "openPicker", message: storedMessage(7), anchor });
        await settle();
        expect(pickerOf()).not.toBeNull();

        feed.vm.$emit("messageAction", { kind: "openPicker", message: storedMessage(7), anchor });
        await settle();

        expect(pickerOf()).toBeNull();
        expect(anchor.getAttribute("aria-expanded")).toBe("false");
    });

    test("asked for by an add chip and placed on its message, marks the add chip open and closes when the add chip asks again", async () => {
        const w = await mountBlock(1200);
        await openRow(w, team.channel_id);
        const bubble = document.createElement("div");
        const opener = document.createElement("button");
        shellRoot(w).append(bubble, opener);
        const feed = w.findComponent(MessageFeedComponent);

        feed.vm.$emit("messageAction", { kind: "openPicker", message: storedMessage(7), anchor: bubble, opener });
        await settle();
        expect(pickerOf()).not.toBeNull();
        expect(opener.getAttribute("aria-expanded")).toBe("true");
        expect(bubble.hasAttribute("aria-expanded")).toBe(false);

        feed.vm.$emit("messageAction", { kind: "openPicker", message: storedMessage(7), anchor: bubble, opener });
        await settle();

        expect(pickerOf()).toBeNull();
        expect(opener.getAttribute("aria-expanded")).toBe("false");
    });

    test("marks the reader's reactions as the page holds the message now, after a page replaced it", async () => {
        const w = await mountBlock(1200);
        await openRow(w, team.channel_id);
        const timeline = timelineOf(team.channel_id);
        timeline.messages.push(storedMessage(7));
        await settle();
        const anchor = document.createElement("button");
        shellRoot(w).append(anchor);

        w.findComponent(MessageFeedComponent).vm.$emit("messageAction", { kind: "openPicker", message: timeline.messages[0], anchor });
        await settle();
        expect(document.querySelector("[role='gridcell'][data-code='pray']")?.getAttribute("aria-pressed")).not.toBe("true");

        // A page merge puts a new object in the message's place, carrying the reader's reaction.
        timeline.messages.splice(0, 1, storedMessage(7, { reactions: [{ code: "pray", count: 1, mine: true, emoji: "🙏", image_url: null, label: "folded hands" }] } as Partial<TimelineMessage>));
        await settle();

        expect(document.querySelector("[role='gridcell'][data-code='pray']")?.getAttribute("aria-pressed")).toBe("true");
    });

    test("closes with the thread drawer it was opened in", async () => {
        const w = await mountBlock(1200);
        await openRow(w, team.channel_id);
        timelineOf(team.channel_id).messages.push(storedMessage(40, { reply_count: 2, last_reply_at: new Date().toISOString(), repliers: [] } as Partial<TimelineMessage>));
        await settle();
        await w.get("[data-testid='frame-main'] [data-testid='thread-summary']").trigger("click");
        await settle();
        const anchor = document.createElement("button");
        w.get("[data-testid='frame-panel']").element.append(anchor);

        w.findComponent(MessageFeedComponent).vm.$emit("messageAction", { kind: "openPicker", message: storedMessage(40), anchor });
        await settle();
        expect(pickerOf()).not.toBeNull();

        // Focus back on what opened it, as after a quick look, so only the drawer's closing can close the picker.
        anchor.focus();
        await settle();
        expect(pickerOf()).not.toBeNull();

        await w.get("[data-testid='frame-panel'] [data-testid='panel-close']").trigger("click");
        await settle();

        expect(pickerOf()).toBeNull();
    });
});

// The delete confirm, however it closes, gives focus to the message it was opened for, the
// tombstone after a delete, since the More button that opened it left with its bar. The dialog
// fades out, so its buttons are still on the page while focus is put back.
describe("delete confirm", () => {
    const messages = (): Record<string, jest.Mock> => mockShell!.messageActions as Record<string, jest.Mock>;

    /** Opens the confirm on message 7 from a button that then leaves, as the action bar's More does. */
    async function openConfirm(): Promise<{ w: VueWrapper, row: Element }> {
        const w = await mountBlock(1200, true);
        await openRow(w, team.channel_id);
        const target = storedMessage(7);
        timelineOf(team.channel_id).messages.push(target);
        const actions = mockShell!.messageActions as Record<string, unknown>;
        const state = actions.state as { confirmingDelete: unknown };
        actions.cancelDelete = jest.fn(() => {
            state.confirmingDelete = null;
        });
        actions.confirmDelete = jest.fn(async () => {
            state.confirmingDelete = null;
            target.deleted_at = new Date().toISOString();
            return true;
        });
        await settle();

        const more = document.createElement("button");
        shellRoot(w).appendChild(more);
        more.focus();
        state.confirmingDelete = { channelId: team.channel_id, message: target };
        await settle();
        more.remove();

        expect(document.activeElement).toBe(w.get("[data-testid='dialog-cancel']").element);
        return { w, row: w.get("[data-testid='frame-main'] [data-message-id='7']").element };
    }

    /** Lets the dialog's fade finish and the frames after it run, as a browser would. */
    async function fadeOut(): Promise<void> {
        await settle();
        await new Promise(resolve => setTimeout(resolve, 50));
        await settle();
    }

    test("Escape gives focus to the message", async () => {
        const { w, row } = await openConfirm();

        await w.get("[data-testid='dialog']").trigger("keydown", { key: "Escape" });
        await fadeOut();

        expect(messages().cancelDelete).toHaveBeenCalledTimes(1);
        expect(document.activeElement).toBe(row);
    });

    test("Cancel gives focus to the message", async () => {
        const { w, row } = await openConfirm();

        await w.get("[data-testid='dialog-cancel']").trigger("click");
        await fadeOut();

        expect(messages().cancelDelete).toHaveBeenCalledTimes(1);
        expect(document.activeElement).toBe(row);
    });

    test("Delete pressed from the keyboard gives focus to the message's tombstone", async () => {
        const { w } = await openConfirm();
        const confirm = w.get("[data-testid='dialog-confirm']").element as HTMLElement;
        confirm.focus();

        // A key on a focused button reaches it as a click.
        await w.get("[data-testid='dialog-confirm']").trigger("click");
        await fadeOut();

        expect(messages().confirmDelete).toHaveBeenCalledTimes(1);
        expect(document.activeElement).toBe(w.get("[data-testid='frame-main'] [data-message-id='7']").element);
        expect(document.activeElement).not.toBe(document.body);
    });

    /**
     * Opens the confirm on the person's own message 7 by pointer, from its action bar's real More
     * menu, then moves the pointer off the message onto the dialog, as a mouse does. The bar's
     * More is still on the page, in its grace, when the dialog closes, and leaves after.
     */
    let mouseFocus: jest.SpyInstance | null = null;

    afterEach(() => {
        mouseFocus?.mockRestore();
        mouseFocus = null;
    });

    async function openConfirmByPointer(): Promise<VueWrapper> {
        const w = await mountBlock(1200, true);
        await openRow(w, team.channel_id);
        const target = storedMessage(7, { person_alias_guid: "a0000001-0000-4000-8000-000000000000", is_own: true } as Partial<TimelineMessage>);
        timelineOf(team.channel_id).messages.push(target);
        const actions = mockShell!.messageActions as Record<string, unknown>;
        const state = actions.state as { confirmingDelete: unknown };
        actions.askDelete = jest.fn((channelId: string, message: TimelineMessage) => {
            state.confirmingDelete = { channelId, message };
        });
        actions.cancelDelete = jest.fn(() => {
            state.confirmingDelete = null;
        });
        actions.confirmDelete = jest.fn(async () => {
            state.confirmingDelete = null;
            target.deleted_at = new Date().toISOString();
            return true;
        });
        await settle();

        // A mouse's focus never matches :focus-visible in a browser, so it holds no bar; jsdom
        // cannot tell, and is told.
        const matches = Element.prototype.matches;
        mouseFocus = jest.spyOn(Element.prototype, "matches").mockImplementation(function (this: Element, selector: string) {
            return selector === ":focus-visible" ? false : matches.call(this, selector);
        });

        // The pointer rests on the message until its bar shows, then picks Delete from More.
        await w.get("[data-testid='frame-main'] [data-hot-id='7']").trigger("pointerover");
        await new Promise(resolve => setTimeout(resolve, 80));
        await settle();
        (w.get("[data-testid='frame-main'] [data-testid='action-bar'] button[aria-label='More actions']").element as HTMLElement).click();
        await settle();
        const item = Array.from(document.querySelectorAll<HTMLElement>("[role='menu'] [role='menuitem']")).find(i => i.textContent?.includes("Delete message"));
        item!.click();
        await settle();
        expect(document.activeElement).toBe(w.get("[data-testid='dialog-cancel']").element);

        // The pointer leaves the message for the dialog; the bar is still up for its grace.
        await w.get("[data-testid='frame-main'] [data-testid='message-feed']").trigger("pointerleave");
        await settle();
        expect(w.find("[data-testid='frame-main'] [data-testid='action-bar']").exists()).toBe(true);
        return w;
    }

    /** Lets the bar's grace run out and the bar leave, after the dialog's fade. */
    async function barLeaves(): Promise<void> {
        await fadeOut();
        await new Promise(resolve => setTimeout(resolve, 300));
        await settle();
    }

    test("a Cancel click gives focus to the message", async () => {
        const w = await openConfirmByPointer();

        await w.get("[data-testid='dialog-cancel']").trigger("click");
        await barLeaves();

        expect(messages().cancelDelete).toHaveBeenCalledTimes(1);
        expect(w.find("[data-testid='frame-main'] [data-testid='action-bar']").exists()).toBe(false);
        expect(document.activeElement).toBe(w.get("[data-testid='frame-main'] [data-message-id='7']").element);
    });

    test("a Delete click gives focus to the message's tombstone", async () => {
        const w = await openConfirmByPointer();

        await w.get("[data-testid='dialog-confirm']").trigger("click");
        await barLeaves();

        expect(messages().confirmDelete).toHaveBeenCalledTimes(1);
        expect(document.activeElement).toBe(w.get("[data-testid='frame-main'] [data-message-id='7']").element);
    });
});

describe("Browse channels", () => {
    const volunteers: SidebarRow = sidebarRow({ channel_id: "c0000011-0000-4000-8000-000000000000", name: "Volunteers", is_public: true, is_member: false });

    /** A directory row of the room above, in the shape chat_list_directory answers. */
    function listVolunteers(): void {
        const directory = roomsOf().state.directory;
        directory.rows = [{ channel_id: volunteers.channel_id, name: "Volunteers", icon_url: null, campus_id: null, is_member: false, can_post_code: null, description: null, group_type_name: null }];
        directory.total = 1;
        directory.emptyKind = null;
    }

    test("opens from the sidebar's Browse row in the conversation's place, the conversation inert under it", async () => {
        const w = await mountBlock(1200);
        await openRow(w, team.channel_id);

        await w.get("[data-testid='sidebar-browse']").trigger("click");
        await settle();

        const main = w.get("[data-testid='frame-main']");
        const view = main.get("[data-testid='main-view']");
        expect(view.attributes("data-kind")).toBe("directory");
        expect(view.find("[data-testid='channel-directory']").exists()).toBe(true);
        expect(main.get("[data-testid='frame-conversation']").attributes("inert")).toBeDefined();
        expect(roomsOf().openDirectory).toHaveBeenCalledTimes(1);
        expect(w.get("[data-testid='sidebar-browse']").attributes("aria-current")).toBe("true");
        expect(document.activeElement).toBe(view.get("[data-testid='directory-search']").element);
    });

    test("opens from the Channels heading's plus too", async () => {
        const w = await mountBlock(1200);

        await w.get("[data-testid='section-browse']").trigger("click");
        await settle();

        expect(w.find("[data-testid='main-view']").exists()).toBe(true);
    });

    test("closes from its X, giving the conversation back", async () => {
        const w = await mountBlock(1200);
        await openRow(w, team.channel_id);
        await w.get("[data-testid='sidebar-browse']").trigger("click");
        await settle();

        await w.get("[data-testid='directory-close']").trigger("click");
        await settle();

        expect(w.find("[data-testid='main-view']").exists()).toBe(false);
        expect(w.get("[data-testid='frame-conversation']").attributes("inert")).toBeUndefined();
        expect(roomsOf().closeDirectory).toHaveBeenCalled();
    });

    test("hands the search, the filters and Join on to the rooms", async () => {
        const w = await mountBlock(1200);
        await w.get("[data-testid='sidebar-browse']").trigger("click");
        listVolunteers();
        await settle();

        await w.get("[data-testid='directory-search']").setValue("vol");
        await w.get("[data-testid='directory-show-not-joined']").trigger("click");
        await w.get("[data-testid='directory-join']").trigger("click");

        expect(roomsOf().setQuery).toHaveBeenCalledWith("vol");
        expect(roomsOf().setShow).toHaveBeenCalledWith("notJoined");
        expect(roomsOf().join).toHaveBeenCalledWith(volunteers.channel_id);
    });

    test("opening a row previews its room and closes the view", async () => {
        const w = await mountBlock(1200);
        await w.get("[data-testid='sidebar-browse']").trigger("click");
        listVolunteers();
        await settle();

        await w.get("[data-testid='directory-row-open']").trigger("click");
        await settle();

        expect(roomsOf().previewRoom).toHaveBeenCalledWith(volunteers.channel_id);
        expect(w.find("[data-testid='main-view']").exists()).toBe(false);
    });
});

describe("a channel the person has not joined", () => {
    const volunteers: SidebarRow = sidebarRow({ channel_id: "c0000011-0000-4000-8000-000000000000", name: "Volunteers", is_public: true, is_member: false });

    async function openVolunteers(): Promise<VueWrapper> {
        const w = await mountBlock(1200);
        (mockShell!.channels as { rows: SidebarRow[] }).rows.push(volunteers);
        await settle();
        await openRow(w, volunteers.channel_id);
        return w;
    }

    test("shows the join bar in the composer's place, which joins through the rooms", async () => {
        const w = await openVolunteers();

        const main = w.get("[data-testid='frame-main']");
        expect(main.find("[data-testid='composer-input']").exists()).toBe(false);
        expect(main.get("[data-testid='join-bar-title']").text()).toBe("You're previewing Volunteers");

        await main.get("[data-testid='join-channel']").trigger("click");
        expect(roomsOf().join).toHaveBeenCalledWith(volunteers.channel_id);
    });

    test("draws the join bar in the state the room's Join is in", async () => {
        const w = await openVolunteers();

        roomsOf().state.joins[volunteers.channel_id] = { kind: "pending" };
        await settle();

        expect(w.get("[data-testid='join-bar']").attributes("data-state")).toBe("pending");
    });

    test("gives the composer back once joined, which enters from below", async () => {
        const w = await openVolunteers();

        const rows = (mockShell!.channels as { rows: SidebarRow[] }).rows;
        rows.splice(rows.findIndex(r => r.channel_id === volunteers.channel_id), 1, { ...volunteers, is_member: true });
        await settle();

        const main = w.get("[data-testid='frame-main']");
        expect(main.find("[data-testid='join-bar']").exists()).toBe(false);
        expect(main.get(".chat-composer").classes()).toContain("is-entering");
    });

    test("moves focus from the join bar to the composer once joined, as the design does on a wide screen", async () => {
        const w = await openVolunteers();
        (w.get("[data-testid='join-channel']").element as HTMLElement).focus();

        const rows = (mockShell!.channels as { rows: SidebarRow[] }).rows;
        rows.splice(rows.findIndex(r => r.channel_id === volunteers.channel_id), 1, { ...volunteers, is_member: true });
        await settle();

        expect(document.activeElement).toBe(w.get("[data-testid='frame-main'] [data-testid='composer-input']").element);
    });

    test("on a phone moves focus from the join bar to the conversation's title, never raising the keyboard", async () => {
        const w = await mountBlock(390);
        (mockShell!.channels as { rows: SidebarRow[] }).rows.push(volunteers);
        await settle();
        await openRow(w, volunteers.channel_id);
        (w.get("[data-testid='join-channel']").element as HTMLElement).focus();

        const rows = (mockShell!.channels as { rows: SidebarRow[] }).rows;
        rows.splice(rows.findIndex(r => r.channel_id === volunteers.channel_id), 1, { ...volunteers, is_member: true });
        await settle();

        expect(document.activeElement).toBe(w.get("[data-testid='frame-main'] [data-testid='header-details']").element);
    });

    test("leaves focus alone after a join when the person was elsewhere", async () => {
        const w = await openVolunteers();
        const search = w.get("[data-testid='sidebar-search']").element as HTMLElement;
        search.focus();

        const rows = (mockShell!.channels as { rows: SidebarRow[] }).rows;
        rows.splice(rows.findIndex(r => r.channel_id === volunteers.channel_id), 1, { ...volunteers, is_member: true });
        await settle();

        expect(document.activeElement).toBe(search);
    });

    test("speaks what the rooms say about a join while Browse channels is closed", async () => {
        const w = await openVolunteers();
        expect(w.find("[data-testid='main-view']").exists()).toBe(false);

        (roomsOf().state as unknown as { announcement: unknown }).announcement = { text: "You're in Volunteers. It can take a few seconds to show up.", isAssertive: false, count: 1 };
        await settle();

        expect(w.get("[data-testid='frame-main'] [data-testid='join-bar-announcement']").text()).toBe("You're in Volunteers. It can take a few seconds to show up.");
    });

    test("the composer's re-entry only fades in under reduced motion", () => {
        const rules = reducedMotionRules("../../../../src/Communication/Chat/chatShell.obs");

        expect(rules.some(r => /(^|,)\s*\.chat-composer\.is-entering\s*(,|$)/.test(r.selector) && /animation-name:\s*chat-crossfade/.test(r.body))).toBe(true);
    });

    test("never shows the join bar in a channel the person is in, whose composer is simply there", async () => {
        const w = await mountBlock(1200);
        await openRow(w, team.channel_id);

        expect(w.find("[data-testid='join-bar']").exists()).toBe(false);
        expect(w.get("[data-testid='frame-main'] .chat-composer").classes()).not.toContain("is-entering");
    });

    test("tells the feed when the room's Join was refused, for its intro", async () => {
        const w = await openVolunteers();
        expect(w.getComponent(MessageFeedComponent).props("isJoinRefused")).toBe(false);

        roomsOf().state.joins[volunteers.channel_id] = { kind: "refused", text: "You can't join this channel." };
        await settle();

        expect(w.getComponent(MessageFeedComponent).props("isJoinRefused")).toBe(true);
    });

    test("a refusal's Browse channels opens the directory", async () => {
        const w = await openVolunteers();
        roomsOf().state.joins[volunteers.channel_id] = { kind: "refused", text: "You can't join this channel." };
        await settle();

        await w.get("[data-testid='join-bar-browse']").trigger("click");
        await settle();

        expect(w.find("[data-testid='main-view']").exists()).toBe(true);
    });
});

describe("the header's subtitle, from the room's facts", () => {
    test("is the open channel's description", async () => {
        const w = await mountBlock(1200);
        roomsOf().state.facts[team.channel_id] = roomFacts({ channel_id: team.channel_id, description: "Sunday teams" });
        await openRow(w, team.channel_id);

        expect(w.get("[data-testid='frame-main'] [data-testid='header-subtitle']").text()).toBe("Sunday teams");
    });

    test("is the other person's campus, named from the init box's campuses", async () => {
        mockConfig.campuses = [{ guid: "d0000001-0000-4000-8000-000000000000", name: "Downtown Campus", shortCode: "DT" }];
        const w = await mountBlock(1200);
        roomsOf().state.facts[dmAnna.channel_id] = roomFacts({
            channel_id: dmAnna.channel_id,
            channel_type: "dm",
            member_count: 2,
            people: [{ person_alias_guid: "a0000002-0000-4000-8000-000000000000", nick_name: "Anna", last_name: "Kim", avatar_url: null, details: { campus_id: "d0000001-0000-4000-8000-000000000000", badge_keys: [] } }]
        });
        await openRow(w, dmAnna.channel_id);

        expect(w.get("[data-testid='frame-main'] [data-testid='header-subtitle']").text()).toBe("Downtown Campus");
    });
});

describe("the channel intro", () => {
    test("is drawn at the top of a channel's history from its facts", async () => {
        const w = await mountBlock(1200);
        roomsOf().state.facts[team.channel_id] = roomFacts({ channel_id: team.channel_id, name: "Worship Team", description: "Sunday teams", member_count: 12 });
        await openRow(w, team.channel_id);

        expect(w.get("[data-testid='frame-main'] [data-testid='intro-title']").text()).toBe("Worship Team");
    });

    test("is never drawn in a direct message", async () => {
        const w = await mountBlock(1200);
        roomsOf().state.facts[dmAnna.channel_id] = roomFacts({ channel_id: dmAnna.channel_id, channel_type: "dm", member_count: 2 });
        await openRow(w, dmAnna.channel_id);

        expect(w.find("[data-testid='channel-intro']").exists()).toBe(false);
    });
});

/** The direct messages the mounted block's shell holds, as useDirectMessage keeps them. */
function directMessagesOf(): Record<string, unknown> & Record<"begin" | "close" | "choose" | "unchoose" | "sendFirst" | "discardFailed", jest.Mock> {
    return mockShell!.directMessages as ReturnType<typeof directMessagesOf>;
}

/** Anna as the people search answers her. */
const annaPerson = { person_alias_guid: "a0000002-0000-4000-8000-000000000000", nick_name: "Anna", last_name: "Kim", avatar_url: null };

// New message is a full view in the conversation's place, opened from the sidebar, never a dialog
// over the page; it hands what the person does to the shell's direct messages.
describe("New message", () => {
    async function openNewMessage(w: VueWrapper): Promise<void> {
        await w.get("[data-testid='new-message']").trigger("click");
        await settle();
    }

    test("opens from the sidebar in the conversation's place, beginning afresh, the conversation inert under it", async () => {
        const w = await mountBlock(1200);
        await openRow(w, team.channel_id);

        await openNewMessage(w);

        const view = w.get("[data-testid='frame-main'] [data-testid='main-view']");
        expect(view.attributes("data-kind")).toBe("newdm");
        expect(view.find("[data-testid='new-message-view']").exists()).toBe(true);
        expect(w.get("[data-testid='frame-conversation']").attributes("inert")).toBeDefined();
        expect(directMessagesOf().begin).toHaveBeenCalledTimes(1);
        expect(document.querySelector("[data-testid='picker-search']")).toBeNull();
        expect(w.get("[data-testid='new-message']").attributes("aria-pressed")).toBe("true");
        expect(document.activeElement).toBe(view.get("[data-testid='nd-to-input']").element);
    });

    test("on a phone opens with focus in the To field, since the button that opened it has slid away", async () => {
        const w = await mountBlock(390);
        const button = w.get("[data-testid='new-message']");
        (button.element as HTMLElement).focus();

        await button.trigger("click");
        await settle();

        expect(document.activeElement).toBe(w.get("[data-testid='main-view'] [data-testid='nd-to-input']").element);
    });

    test("on a phone, Escape closing the empty view over a conversation leaves focus on that conversation's header", async () => {
        const w = await mountBlock(390);
        await openRow(w, team.channel_id);
        await w.get("[data-testid='header-back']").trigger("click");
        await settle();
        const button = w.get("[data-testid='new-message']");
        (button.element as HTMLElement).focus();
        await button.trigger("click");
        await settle();

        const input = w.get("[data-testid='nd-to-input']");
        (input.element as HTMLElement).focus();
        await input.trigger("keydown", { key: "Escape" });
        await settle();

        expect(w.find("[data-testid='main-view']").exists()).toBe(false);
        expect(w.get("[data-testid='chat-frame']").attributes("data-pane")).toBe("conversation");
        expect(document.activeElement).toBe(w.get("[data-testid='frame-main'] [data-testid='header-details']").element);
    });

    test("opens from the Direct messages heading's plus too", async () => {
        const w = await mountBlock(1200);

        await w.get("[data-testid='section-new-message']").trigger("click");
        await settle();

        expect(w.get("[data-testid='main-view']").attributes("data-kind")).toBe("newdm");
    });

    test("closes from its X, letting go of New message and giving the conversation back", async () => {
        const w = await mountBlock(1200);
        await openRow(w, team.channel_id);
        await openNewMessage(w);

        await w.get("[data-testid='nd-close']").trigger("click");
        await settle();

        expect(w.find("[data-testid='main-view']").exists()).toBe(false);
        expect(directMessagesOf().close).toHaveBeenCalled();
        expect(w.get("[data-testid='frame-conversation']").attributes("inert")).toBeUndefined();
    });

    test("hands a chip's remove and the message sent to the direct messages", async () => {
        const w = await mountBlock(1200);
        await openNewMessage(w);
        Object.assign(directMessagesOf(), {
            chosen: [annaPerson],
            lookup: { status: "new", channelId: null, isSlow: false },
            draft: { people: [annaPerson], status: "draft", body: "", channelGuid: null, phase: null }
        });
        await settle();

        await w.get("[data-testid='nd-chip-remove']").trigger("click");
        expect(directMessagesOf().unchoose).toHaveBeenCalledWith(annaPerson.person_alias_guid);

        const editor = w.get("[data-testid='new-message-view'] [data-testid='composer-input']").element as HTMLElement;
        editor.replaceChildren(document.createTextNode("hello"));
        editor.dispatchEvent(new Event("input", { bubbles: true }));
        await settle();
        await w.get("[data-testid='new-message-view'] form").trigger("submit");
        await settle();

        expect(directMessagesOf().sendFirst).toHaveBeenCalledWith("hello");
    });

    test("a failed first message's Try again sends its text again, and Delete discards it", async () => {
        const w = await mountBlock(1200);
        await openNewMessage(w);
        Object.assign(directMessagesOf(), {
            chosen: [annaPerson],
            lookup: { status: "new", channelId: null, isSlow: false },
            draft: { people: [annaPerson], status: "draft", body: "hello", channelGuid: "c0000009-0000-4000-8000-000000000000", phase: "failed" }
        });
        await settle();

        await w.get("[data-testid='nd-retry']").trigger("click");
        await w.get("[data-testid='nd-discard']").trigger("click");

        expect(directMessagesOf().sendFirst).toHaveBeenCalledWith("hello");
        expect(directMessagesOf().discardFailed).toHaveBeenCalledTimes(1);
    });

    test.each([
        ["a wide block", 1200, "[data-testid='frame-main'] [data-testid='composer-input']"],
        ["a phone", 390, "[data-testid='frame-main'] [data-testid='header-details']"]
    ])("on %s, the first message opening its new conversation takes focus there, from the view going away", async (_kind, width, target) => {
        const w = await mountBlock(width);
        await openNewMessage(w);
        Object.assign(directMessagesOf(), {
            chosen: [annaPerson],
            lookup: { status: "new", channelId: null, isSlow: false },
            draft: { people: [annaPerson], status: "starting", body: "hello", channelGuid: null, phase: "asking" }
        });
        await settle();
        (w.get("[data-testid='new-message-view'] [data-testid='composer-input']").element as HTMLElement).focus();

        // as the first message's send does: the conversation Rock made opens in the view's place
        mockShell!.state.activeChannelId = dmAnna.channel_id;
        await settle();

        expect(w.find("[data-testid='main-view']").exists()).toBe(false);
        expect(document.activeElement).toBe(w.get(target).element);
    });

    test("the first message's conversation takes the focus its locked composer dropped while it started", async () => {
        const w = await mountBlock(1200);
        await openNewMessage(w);
        Object.assign(directMessagesOf(), {
            chosen: [annaPerson],
            lookup: { status: "new", channelId: null, isSlow: false },
            draft: { people: [annaPerson], status: "draft", body: "", channelGuid: null, phase: null }
        });
        await settle();
        const editor = w.get("[data-testid='new-message-view'] [data-testid='composer-input']").element as HTMLElement;
        editor.focus();

        // the box locks as the message starts, and a browser drops the focus it held to the page
        Object.assign(directMessagesOf().draft as Record<string, unknown>, { status: "starting", phase: "asking" });
        await settle();
        editor.dispatchEvent(new FocusEvent("focusout", { bubbles: true, relatedTarget: null }));
        editor.blur();
        mockShell!.state.activeChannelId = dmAnna.channel_id;
        await settle();

        expect(document.activeElement).toBe(w.get("[data-testid='frame-main'] [data-testid='composer-input']").element);
    });

    test("the conversation found shows its last messages under its strip, drawn as its feed is and out of reach", async () => {
        const w = await mountBlock(1200);
        await openNewMessage(w);
        Object.assign(directMessagesOf(), {
            chosen: [annaPerson],
            lookup: { status: "found", channelId: dmAnna.channel_id, isSlow: false },
            recent: [
                { id: 41, person_alias_guid: annaPerson.person_alias_guid, sender_nick_name: "Anna", sender_last_name: "Kim", body: "See you Saturday", message_type: "text", created_at: "2026-10-20T10:00:00Z" },
                { id: 42, person_alias_guid: "a0000001-0000-4000-8000-000000000000", body: "Sounds good", message_type: "text", created_at: "2026-10-20T10:01:00Z", is_own: true }
            ]
        });
        await settle();

        const history = w.get("[data-testid='main-view'] [data-testid='nd-history']");
        expect(history.attributes("inert")).toBeDefined();
        expect(history.findAll("[data-message-id]").map(row => row.attributes("data-message-id"))).toEqual(["41", "42"]);
        expect(history.text()).toContain("See you Saturday");
    });

    test("the conversation found opens from its strip, closing the view", async () => {
        const w = await mountBlock(1200);
        await openNewMessage(w);
        Object.assign(directMessagesOf(), {
            chosen: [annaPerson],
            lookup: { status: "found", channelId: dmAnna.channel_id, isSlow: false }
        });
        await settle();

        expect(w.get("[data-testid='nd-found']").text()).toContain("Anna Kim. What you send goes there.");
        await w.get("[data-testid='nd-open']").trigger("click");
        await settle();

        expect(mockShell!.selectChannel).toHaveBeenCalledWith(dmAnna.channel_id);
        expect(w.find("[data-testid='main-view']").exists()).toBe(false);
    });
});

// A message's author or a person's chip asks for that person's card, in the room it was opened
// from; the card is drawn once the shell's rooms have read the person, and its Message opens the
// conversation the two already have, or New message with that person chosen.
describe("the profile card", () => {
    /** chat_get_person's answer for Anna, exactly its keys. */
    function annaProfile(extra: Record<string, unknown> = {}): Record<string, unknown> {
        return {
            person_alias_guid: annaPerson.person_alias_guid,
            nick_name: "Anna",
            last_name: "Kim",
            avatar_url: null,
            is_self: false,
            is_leader: false,
            details: null,
            can_message: true,
            dm_channel_id: null,
            ...extra
        };
    }

    async function cardFor(w: VueWrapper, profile: Record<string, unknown>): Promise<HTMLElement> {
        const anchor = w.get("[data-testid='header-title']").element as HTMLElement;
        w.findComponent(MessageFeedComponent).vm.$emit("messageAction", { kind: "person", personAliasGuid: annaPerson.person_alias_guid, anchor });
        await settle();
        roomsOf().state.card = { personAliasGuid: annaPerson.person_alias_guid, channelId: team.channel_id, person: profile } as never;
        await settle();
        const card = document.querySelector<HTMLElement>("[data-testid='profile-card']");
        if (!card) {
            throw new Error("No profile card was drawn");
        }
        return card;
    }

    test("is asked for in the room open when an author or a chip asks, and drawn once the person is read", async () => {
        const w = await mountBlock(1200);
        await openRow(w, team.channel_id);

        const card = await cardFor(w, annaProfile());

        expect(roomsOf().openCard).toHaveBeenCalledWith(annaPerson.person_alias_guid, team.channel_id);
        expect(card.getAttribute("aria-label")).toBe("Profile, Anna Kim");
    });

    test("is not drawn while the person is still being read", async () => {
        const w = await mountBlock(1200);
        await openRow(w, team.channel_id);
        const anchor = w.get("[data-testid='header-title']").element as HTMLElement;

        w.findComponent(MessageFeedComponent).vm.$emit("messageAction", { kind: "person", personAliasGuid: annaPerson.person_alias_guid, anchor });
        roomsOf().state.card = { personAliasGuid: annaPerson.person_alias_guid, channelId: team.channel_id, person: null } as never;
        await settle();

        expect(roomsOf().openCard).toHaveBeenCalledTimes(1);
        expect(document.querySelector("[data-testid='profile-card']")).toBeNull();
    });

    test("Message opens the conversation the two already have, closing the card", async () => {
        const w = await mountBlock(1200);
        await openRow(w, team.channel_id);
        const card = await cardFor(w, annaProfile({ dm_channel_id: dmAnna.channel_id }));

        card.querySelector<HTMLElement>("[data-testid='profile-message']")!.click();
        await settle();

        expect(mockShell!.selectChannel).toHaveBeenCalledWith(dmAnna.channel_id);
        expect(roomsOf().closeCard).toHaveBeenCalled();
        expect(document.querySelector("[data-testid='profile-card']")).toBeNull();
    });

    test("Message opens New message with the person chosen when they have no conversation yet", async () => {
        const w = await mountBlock(1200);
        await openRow(w, team.channel_id);
        const card = await cardFor(w, annaProfile());

        card.querySelector<HTMLElement>("[data-testid='profile-message']")!.click();
        await settle();

        expect(w.get("[data-testid='main-view']").attributes("data-kind")).toBe("newdm");
        expect(directMessagesOf().begin).toHaveBeenCalledWith([annaPerson]);
        expect(roomsOf().closeCard).toHaveBeenCalled();
    });

    test("closes on Escape, letting go of the person", async () => {
        const w = await mountBlock(1200);
        await openRow(w, team.channel_id);
        const card = await cardFor(w, annaProfile());

        card.dispatchEvent(new KeyboardEvent("keydown", { key: "Escape", bubbles: true }));
        await settle();

        expect(roomsOf().closeCard).toHaveBeenCalled();
        expect(document.querySelector("[data-testid='profile-card']")).toBeNull();
    });
});

// The header's title opens the conversation's details, as does the phone's overflow; Leave and a
// group's Rename go to the shell's rooms, and the dialog closes once the person has left.
describe("the conversation's details", () => {
    const hike: SidebarRow = sidebarRow({ channel_id: "c0000006-0000-4000-8000-000000000000", channel_type: "dm", dm_names: ["Caleb", "Hannah"], dm_other_count: 2 });

    function detailsDialog(): HTMLElement | null {
        return document.querySelector<HTMLElement>("[data-testid='details-close']")?.closest<HTMLElement>("[role='dialog']") ?? null;
    }

    function dialogName(dialog: HTMLElement): string {
        const labelledBy = dialog.getAttribute("aria-labelledby");
        return labelledBy ? document.getElementById(labelledBy)?.textContent?.trim() ?? "" : dialog.getAttribute("aria-label") ?? "";
    }

    async function openTeamDetails(w: VueWrapper): Promise<void> {
        roomsOf().state.facts[team.channel_id] = roomFacts({ channel_id: team.channel_id, name: "Worship Team", member_count: 12 });
        await openRow(w, team.channel_id);
        await w.get("[data-testid='header-details']").trigger("click");
        await settle();
    }

    test("open from the header's title, titled by the room's kind", async () => {
        const w = await mountBlock(1200);
        await openTeamDetails(w);

        const dialog = detailsDialog();
        expect(dialog).not.toBeNull();
        expect(dialogName(dialog!)).toBe("Channel details");
    });

    test("open from the phone's overflow menu too", async () => {
        const w = await mountBlock(390);
        roomsOf().state.facts[dmAnna.channel_id] = roomFacts({ channel_id: dmAnna.channel_id, channel_type: "dm", can_leave: false, member_count: 2 });
        await openRow(w, dmAnna.channel_id);

        await w.get("[data-testid='header-more'] button").trigger("click");
        await settle();
        const item = Array.from(document.querySelectorAll<HTMLElement>("[role='menu'] [role='menuitem']")).find(i => i.textContent?.trim() === "Conversation details");
        item!.click();
        await settle();

        expect(dialogName(detailsDialog()!)).toBe("Conversation details");
    });

    test("Leave goes to the rooms, and the dialog closes once the person has left", async () => {
        const w = await mountBlock(1200);
        await openTeamDetails(w);

        document.querySelector<HTMLElement>("[data-testid='details-leave']")!.click();
        await settle();
        document.querySelector<HTMLElement>("[data-testid='leave-go']")!.click();
        await settle();
        expect(roomsOf().leave).toHaveBeenCalledWith(team.channel_id);

        roomsOf().state.leaves[team.channel_id] = { kind: "left" };
        await settle();
        expect(detailsDialog()).toBeNull();
    });

    test.each([
        ["a wide block", 1200, "[data-testid='frame-main'] [data-testid='composer-input']"],
        ["a phone", 390, "[data-testid='frame-main'] [data-testid='header-details']"]
    ])("on %s, the next conversation a leave opens takes focus, from the dialog going away", async (_kind, width, target) => {
        const w = await mountBlock(width);
        roomsOf().state.facts[team.channel_id] = roomFacts({ channel_id: team.channel_id, name: "Worship Team", member_count: 12 });
        await openRow(w, team.channel_id);
        await w.get("[data-testid='header-details']").trigger("click");
        await settle();
        document.querySelector<HTMLElement>("[data-testid='details-leave']")!.click();
        await settle();
        document.querySelector<HTMLElement>("[data-testid='leave-go']")!.focus();

        // as the rooms' leave does: left, and the next conversation opened in the same turn
        roomsOf().state.leaves[team.channel_id] = { kind: "left" };
        mockShell!.state.activeChannelId = dmAnna.channel_id;
        await settle();

        expect(detailsDialog()).toBeNull();
        expect(document.activeElement).toBe(w.get(target).element);
    });

    test("the next conversation a leave opens takes focus though the platform's signal closed the room first", async () => {
        const w = await mountBlock(1200);
        roomsOf().state.facts[team.channel_id] = roomFacts({ channel_id: team.channel_id, name: "Worship Team", member_count: 12 });
        await openRow(w, team.channel_id);
        await w.get("[data-testid='header-details']").trigger("click");
        await settle();
        document.querySelector<HTMLElement>("[data-testid='details-leave']")!.click();
        await settle();
        document.querySelector<HTMLElement>("[data-testid='leave-go']")!.focus();

        // the door's own sync reaches the person's topic first, and its recheck closes the room
        mockShell!.state.activeChannelId = null;
        await settle();
        // then the door answers, and the rooms open the conversation beside it
        roomsOf().state.leaves[team.channel_id] = { kind: "left" };
        mockShell!.state.activeChannelId = dmAnna.channel_id;
        await settle();

        expect(document.activeElement).toBe(w.get("[data-testid='frame-main'] [data-testid='composer-input']").element);
    });

    test("the next conversation a leave opens takes focus though the signal took the room's row, and its details with it, first", async () => {
        const w = await mountBlock(1200);
        roomsOf().state.facts[team.channel_id] = roomFacts({ channel_id: team.channel_id, name: "Worship Team", member_count: 12 });
        await openRow(w, team.channel_id);
        await w.get("[data-testid='header-details']").trigger("click");
        await settle();
        document.querySelector<HTMLElement>("[data-testid='details-leave']")!.click();
        await settle();
        document.querySelector<HTMLElement>("[data-testid='leave-go']")!.focus();

        // the sidebar reload the door's own sync asks comes back without the room
        const rows = (mockShell!.channels as { rows: SidebarRow[] }).rows;
        rows.splice(rows.findIndex(r => r.channel_id === team.channel_id), 1);
        await settle();
        expect(detailsDialog()).toBeNull();
        mockShell!.state.activeChannelId = null;
        await settle();
        roomsOf().state.leaves[team.channel_id] = { kind: "left" };
        mockShell!.state.activeChannelId = dmAnna.channel_id;
        await settle();

        expect(document.activeElement).toBe(w.get("[data-testid='frame-main'] [data-testid='composer-input']").element);
    });

    test("a group's Rename goes to the rooms with the name typed", async () => {
        const w = await mountBlock(1200);
        (mockShell!.channels as { rows: SidebarRow[] }).rows.push(hike);
        roomsOf().state.facts[hike.channel_id] = roomFacts({
            channel_id: hike.channel_id,
            channel_type: "dm",
            is_public: false,
            member_count: 3,
            people: [
                { person_alias_guid: "a0000005-0000-4000-8000-000000000000", nick_name: "Caleb", last_name: "Brooks", avatar_url: null, details: null },
                { person_alias_guid: "a0000006-0000-4000-8000-000000000000", nick_name: "Hannah", last_name: "Diaz", avatar_url: null, details: null }
            ]
        });
        await settle();
        await openRow(w, hike.channel_id);
        await w.get("[data-testid='header-details']").trigger("click");
        await settle();

        const input = document.querySelector<HTMLInputElement>("[data-testid='rename-input']")!;
        input.value = "Trail crew";
        input.dispatchEvent(new Event("input", { bubbles: true }));
        await settle();
        document.querySelector<HTMLElement>("[data-testid='rename-save']")!.click();
        await settle();

        expect(roomsOf().rename).toHaveBeenCalledWith(hike.channel_id, "Trail crew");
    });

    test("close from their close button, giving focus back to the title", async () => {
        const w = await mountBlock(1200);
        await openTeamDetails(w);

        document.querySelector<HTMLElement>("[data-testid='details-close']")!.click();
        await settle();

        expect(detailsDialog()).toBeNull();
        expect(document.activeElement).toBe(w.get("[data-testid='header-details']").element);
    });
});

// The members panel opens for the open conversation from a channel's facepile, a group's Members
// button, the intro's Members and the details dialog's member count; a second press closes it.
// A member row opens the person's card beside the panel, and Escape gives focus back to the row.
describe("the members panel", () => {
    const hike: SidebarRow = sidebarRow({ channel_id: "c0000006-0000-4000-8000-000000000000", channel_type: "dm", name: "Hike planning", dm_names: ["Caleb", "Hannah"], dm_other_count: 2 });

    /** Anna as chat_list_members answers her, exactly its keys. */
    const annaMember = { person_alias_guid: "a0000002-0000-4000-8000-000000000000", nick_name: "Anna", last_name: "Kim", avatar_url: null, is_leader: false, details: { campus_id: null, badge_keys: ["serving"] }, can_message: true };

    function membersOf(): { state: { channelId: string | null, rows: unknown[] }, open: jest.Mock, close: jest.Mock } {
        return mockShell!.members as ReturnType<typeof membersOf>;
    }

    async function openTeam(w: VueWrapper): Promise<void> {
        roomsOf().state.facts[team.channel_id] = roomFacts({ channel_id: team.channel_id, name: "Worship Team", member_count: 12 });
        await openRow(w, team.channel_id);
    }

    test("opens from a channel's facepile, named with its count, and closes on a second press", async () => {
        const w = await mountBlock(1200);
        await openTeam(w);

        const pile = w.get("[data-testid='frame-main'] [data-testid='header-members']");
        await pile.trigger("click");
        await settle();
        expect(membersOf().open).toHaveBeenCalledWith(team.channel_id);
        expect(w.get("[data-testid='panel-host'] [data-testid='panel-subtitle']").text()).toBe("Worship Team · 12 members");
        expect(w.get("[data-testid='chat-frame']").attributes("data-panel-open")).toBe("true");
        expect(w.get("[data-testid='frame-main'] [data-testid='header-members']").attributes("aria-expanded")).toBe("true");

        await w.get("[data-testid='frame-main'] [data-testid='header-members']").trigger("click");
        await settle();
        expect(membersOf().close).toHaveBeenCalled();
        expect(w.get("[data-testid='chat-frame']").attributes("data-panel-open")).toBe("false");
    });

    test("opens from a group's Members button, counting its people", async () => {
        const w = await mountBlock(1200);
        (mockShell!.channels as { rows: SidebarRow[] }).rows.push(hike);
        roomsOf().state.facts[hike.channel_id] = roomFacts({ channel_id: hike.channel_id, channel_type: "dm", name: "Hike planning", member_count: 3 });
        await settle();
        await openRow(w, hike.channel_id);

        await w.get("[data-testid='frame-main'] [data-testid='header-members']").trigger("click");
        await settle();
        expect(membersOf().open).toHaveBeenCalledWith(hike.channel_id);
        expect(w.get("[data-testid='panel-host'] [data-testid='panel-subtitle']").text()).toBe("Hike planning · 3 people");
    });

    test("is not offered in a conversation of two", async () => {
        const w = await mountBlock(1200);
        roomsOf().state.facts[dmAnna.channel_id] = roomFacts({ channel_id: dmAnna.channel_id, channel_type: "dm", member_count: 2 });
        await openRow(w, dmAnna.channel_id);

        expect(w.find("[data-testid='header-members']").exists()).toBe(false);
    });

    test("opens from the intro's Members and closes on a second press", async () => {
        const w = await mountBlock(1200);
        await openTeam(w);

        await w.get("[data-testid='intro-members-button']").trigger("click");
        await settle();
        expect(membersOf().open).toHaveBeenCalledWith(team.channel_id);
        expect(w.find("[data-testid='members-panel']").exists()).toBe(true);

        await w.get("[data-testid='intro-members-button']").trigger("click");
        await settle();
        expect(w.get("[data-testid='chat-frame']").attributes("data-panel-open")).toBe("false");
    });

    test("opens from the details dialog's member count, closing the dialog", async () => {
        const w = await mountBlock(1200);
        await openTeam(w);
        await w.get("[data-testid='header-details']").trigger("click");
        await settle();

        document.querySelector<HTMLElement>("[data-testid='details-members']")!.click();
        await settle();

        expect(membersOf().open).toHaveBeenCalledWith(team.channel_id);
        expect(w.find("[data-testid='members-panel']").exists()).toBe(true);
        expect(document.querySelector("[data-testid='details-close']")).toBeNull();
    });

    test("opens a member's card in the panel's room, beside the panel, with the church's badges, and Escape gives focus back to the row", async () => {
        const w = await mountBlock(1200);
        (mockShell!.channels as Record<string, unknown>).badges = [{ badge_key: "serving", name: "Serving", icon_css: null, bg_color: null, fg_color: null, sort_order: 1 }];
        await openTeam(w);
        await w.get("[data-testid='frame-main'] [data-testid='header-members']").trigger("click");
        await settle();
        membersOf().state.rows = [annaMember];
        await settle();

        const who = w.get("[data-testid='members-panel'] [data-testid='member-who']");
        await who.trigger("click");
        await settle();
        expect(roomsOf().openCard).toHaveBeenCalledWith(annaMember.person_alias_guid, team.channel_id);

        roomsOf().state.card = { personAliasGuid: annaMember.person_alias_guid, channelId: team.channel_id, person: { ...annaMember, is_self: false, dm_channel_id: null } } as never;
        await settle();
        const card = document.querySelector<HTMLElement>("[data-testid='profile-card']")!;
        expect(card).not.toBeNull();
        // the row is the card's anchor, inside the panel, which is what sets the card beside it
        expect(w.findComponent(ProfileCardComponent).props("anchor")).toBe(who.element);
        expect(who.element.closest(".chat-panel")).not.toBeNull();
        expect(Array.from(card.querySelectorAll("[data-testid='profile-badge']")).map(pill => pill.textContent?.trim())).toEqual(["Serving"]);

        card.dispatchEvent(new KeyboardEvent("keydown", { key: "Escape", bubbles: true }));
        await settle();
        expect(document.activeElement).toBe(who.element);
    });

    test.each([1200, 390])("a member's Message opens New message with that person chosen at width %i", async width => {
        const w = await mountBlock(width);
        await openTeam(w);
        await w.get("[data-testid='intro-members-button']").trigger("click");
        await settle();
        membersOf().state.rows = [annaMember];
        roomsOf().readPerson.mockResolvedValue({ ...annaMember, is_self: false, dm_channel_id: null });
        await settle();

        await w.get("[data-testid='member-message']").trigger("click");
        await settle();

        expect(directMessagesOf().begin).toHaveBeenCalledWith([{
            person_alias_guid: annaMember.person_alias_guid,
            nick_name: annaMember.nick_name,
            last_name: annaMember.last_name,
            avatar_url: annaMember.avatar_url
        }]);
        expect(w.get("[data-testid='main-view']").attributes("data-kind")).toBe("newdm");
        expect(w.get("[data-testid='chat-frame']").attributes("data-panel-open")).toBe("false");
    });

    test.each([1200, 390])("a member's Message opens their existing conversation at width %i", async width => {
        const w = await mountBlock(width);
        await openTeam(w);
        await w.get("[data-testid='intro-members-button']").trigger("click");
        await settle();
        membersOf().state.rows = [annaMember];
        roomsOf().readPerson.mockResolvedValue({ ...annaMember, is_self: false, dm_channel_id: dmAnna.channel_id });
        await settle();

        await w.get("[data-testid='member-message']").trigger("click");
        await settle();

        expect(mockShell!.selectChannel).toHaveBeenLastCalledWith(dmAnna.channel_id);
        expect(roomsOf().readPerson).toHaveBeenCalledWith(annaMember.person_alias_guid, team.channel_id);
        expect(directMessagesOf().begin).not.toHaveBeenCalled();
        expect(w.find("[data-testid='main-view']").exists()).toBe(false);
        expect(w.get("[data-testid='chat-frame']").attributes("data-panel-open")).toBe("false");
    });

    test("a member's Message drops a profile answer after another conversation was selected", async () => {
        const w = await mountBlock(1200);
        await openTeam(w);
        await w.get("[data-testid='intro-members-button']").trigger("click");
        await settle();
        membersOf().state.rows = [annaMember];
        let resolvePerson!: (person: unknown) => void;
        roomsOf().readPerson.mockReturnValue(new Promise(resolve => { resolvePerson = resolve; }));
        await settle();

        await w.get("[data-testid='member-message']").trigger("click");
        await openRow(w, dmAnna.channel_id);
        resolvePerson({ ...annaMember, is_self: false, dm_channel_id: "c0000009-0000-4000-8000-000000000000" });
        await settle();

        expect(mockShell!.selectChannel).toHaveBeenLastCalledWith(dmAnna.channel_id);
        expect(directMessagesOf().begin).not.toHaveBeenCalled();
    });

    test("a member's Message drops a profile answer after the same members panel was closed and reopened", async () => {
        const w = await mountBlock(1200);
        await openTeam(w);
        await w.get("[data-testid='intro-members-button']").trigger("click");
        await settle();
        membersOf().state.rows = [annaMember];
        let resolvePerson!: (person: unknown) => void;
        roomsOf().readPerson.mockReturnValue(new Promise(resolve => { resolvePerson = resolve; }));
        await settle();

        await w.get("[data-testid='member-message']").trigger("click");
        await w.get("[data-testid='panel-close']").trigger("click");
        await settle();
        await w.get("[data-testid='intro-members-button']").trigger("click");
        await settle();
        resolvePerson({ ...annaMember, is_self: false, dm_channel_id: dmAnna.channel_id });
        await settle();

        expect(mockShell!.selectChannel).toHaveBeenLastCalledWith(team.channel_id);
        expect(directMessagesOf().begin).not.toHaveBeenCalled();
        expect(w.get("[data-testid='chat-frame']").attributes("data-panel-open")).toBe("true");
    });

    test("no longer opens from a keyboard shortcut", async () => {
        const w = await mountBlock(1200);
        await openTeam(w);

        await w.get("[data-testid='chat-shell']").trigger("keydown", { key: "M", altKey: true, shiftKey: true });
        await settle();
        expect(membersOf().open).not.toHaveBeenCalled();
        expect(w.find("[data-testid='members-panel']").exists()).toBe(false);
    });

    /** Opens the members panel on the team channel, with Anna loaded and the rights given. */
    async function openManaged(w: VueWrapper): Promise<Record<string, unknown> & { state: Record<string, unknown> }> {
        await openTeam(w);
        await w.get("[data-testid='frame-main'] [data-testid='header-members']").trigger("click");
        await settle();
        const members = mockShell!.members as Record<string, unknown> & { state: Record<string, unknown> };
        members.state.rows = [annaMember];
        members.state.canManageMembers = true;
        await settle();
        return members;
    }

    test("the panel's Add people opens the dialog over the frame, which searches, adds and closes through the roster", async () => {
        const w = await mountBlock(1200);
        const members = await openManaged(w);

        await w.get("[data-testid='members-add']").trigger("click");
        await settle();
        expect(members.openAdd).toHaveBeenCalled();
        (members.state.adding as Record<string, unknown>).isOpen = true;
        await settle();

        const dialog = w.get("[data-testid='add-people']");
        expect(dialog.element.closest("[data-testid='chat-frame']")).not.toBeNull();
        expect(dialog.element.closest(".chat-panel")).toBeNull();
        expect(w.get("[data-testid='add-people'] .chat-dialog-title").text()).toBe("Add people to Worship Team");

        await w.get("[data-testid='ap-input']").setValue("aa");
        expect(members.setAddQuery).toHaveBeenCalledWith("aa");

        await w.get("[data-testid='ap-cancel']").trigger("click");
        expect(members.closeAdd).toHaveBeenCalled();
    });

    test("Remove in a row's menu opens the confirm over the frame, which removes and cancels through the roster", async () => {
        const w = await mountBlock(1200);
        const members = await openManaged(w);

        await w.get("[data-testid='member-more']").trigger("click");
        await settle();
        document.querySelector<HTMLElement>("[data-key='remove']")!.click();
        await settle();
        expect(members.askRemove).toHaveBeenCalledWith(annaMember.person_alias_guid);

        members.state.confirming = annaMember;
        await settle();
        const confirm = document.querySelector<HTMLElement>("[data-testid='remove-member'] [data-testid='dialog-confirm']")!;
        expect(confirm.closest(".chat-panel")).toBeNull();
        confirm.click();
        await settle();
        expect(members.remove).toHaveBeenCalledWith(null);

        document.querySelector<HTMLElement>("[data-testid='remove-member'] [data-testid='dialog-cancel']")!.click();
        await settle();
        expect(members.cancelRemove).toHaveBeenCalled();
    });

    test("a row's refusal is dismissed through the roster", async () => {
        const w = await mountBlock(1200);
        const members = await openManaged(w);
        members.state.removals = { [annaMember.person_alias_guid]: { kind: "refused", text: "That person isn't in this conversation.", isStale: true } };
        await settle();

        await w.get("[data-testid='member-refusal-dismiss']").trigger("click");
        expect(members.dismissRemoval).toHaveBeenCalledWith(annaMember.person_alias_guid);
    });

    test("the page asks Rock's doors for the rights, an add and a remove, and a door Rock answered without is unreached", async () => {
        await mountBlock(1200);
        const options = mockShellOptions!;
        mockInvoke.mockResolvedValueOnce({ isSuccess: true, data: { canManageMembers: true } });
        mockInvoke.mockResolvedValueOnce({ isSuccess: true, data: { code: "ok", message: null, isPending: true } });
        mockInvoke.mockResolvedValueOnce({ isSuccess: false, errorMessage: "Request failed", data: null });

        expect(await options.getMemberRights!(team.channel_id)).toEqual({ canManageMembers: true });
        expect(await options.addMembers!(team.channel_id, [annaMember.person_alias_guid])).toEqual({ code: "ok", message: null, isPending: true });
        expect(await options.removeMember!(team.channel_id, annaMember.person_alias_guid, null)).toEqual({ code: "door.unavailable", message: "Request failed", isPending: false });

        expect(mockInvoke.mock.calls).toEqual([
            ["GetMemberRights", { channelGuid: team.channel_id }],
            ["AddMembers", { channelGuid: team.channel_id, personAliasGuids: [annaMember.person_alias_guid] }],
            ["RemoveMember", { channelGuid: team.channel_id, personAliasGuid: annaMember.person_alias_guid, replacementPersonAliasGuid: null }]
        ]);
    });

    test("rights Rock answered without are none", async () => {
        await mountBlock(1200);
        mockInvoke.mockResolvedValueOnce({ isSuccess: false, errorMessage: "Request failed", data: null });

        expect(await mockShellOptions!.getMemberRights!(team.channel_id)).toEqual({ canManageMembers: false });
    });

});

// The person's settings across the block: the details' notify mode, Star and Hide, the header's
// bell and Star, and Chat settings from the sidebar's foot or the person's own card, each asking
// the shell's settings to save; the composer locked with the row's sentence where the person
// cannot post, and counting down to the row's limit.
describe("settings", () => {
    function prefsOf(): Record<string, jest.Mock> & { state: Record<string, unknown> } {
        return mockShell!.prefs as ReturnType<typeof prefsOf>;
    }

    function settingsDialog(): HTMLElement | null {
        return document.querySelector<HTMLElement>("[data-testid='settings-close']")?.closest<HTMLElement>("[role='dialog']") ?? null;
    }

    async function openTeam(w: VueWrapper): Promise<void> {
        roomsOf().state.facts[team.channel_id] = roomFacts({ channel_id: team.channel_id, name: "Worship Team", member_count: 12 });
        await openRow(w, team.channel_id);
    }

    test("the details' notify mode, Star and Hide ask the settings to save, for the open room", async () => {
        const w = await mountBlock(1200);
        await openTeam(w);
        await w.get("[data-testid='header-details']").trigger("click");
        await settle();

        document.querySelectorAll<HTMLElement>("[data-testid='notify-option']")[2].click();
        document.querySelector<HTMLElement>("[data-testid='switch-star'] [role='switch']")!.click();
        document.querySelector<HTMLElement>("[data-testid='switch-hide'] [role='switch']")!.click();
        await settle();

        expect(prefsOf().setNotifyMode).toHaveBeenCalledWith(team.channel_id, "silent", false);
        expect(prefsOf().setStarred).toHaveBeenCalledWith(team.channel_id, true, false);
        expect(prefsOf().setHidden).toHaveBeenCalledWith(team.channel_id, true);
    });

    test("the details show the room's mode and star from its sidebar row", async () => {
        const w = await mountBlock(1200);
        const rows = (mockShell!.channels as { rows: SidebarRow[] }).rows;
        rows.splice(0, 1, { ...team, notify_mode: "mentions", is_favorite: true });
        await settle();
        await openTeam(w);
        await w.get("[data-testid='header-details']").trigger("click");
        await settle();

        const checked = Array.from(document.querySelectorAll<HTMLElement>("[data-testid='notify-option']")).map(o => o.getAttribute("aria-checked"));
        expect(checked).toEqual(["false", "true", "false"]);
        expect(document.querySelector("[data-testid='switch-star'] [role='switch']")?.getAttribute("aria-checked")).toBe("true");
    });

    test("the header's bell and Star ask the settings to save, as from the header", async () => {
        const w = await mountBlock(1200);
        await openTeam(w);

        await w.get("[data-testid='header-notify'] button").trigger("click");
        await settle();
        document.querySelectorAll<HTMLElement>("[role='menu'] [role='menuitemradio']")[1].click();
        await settle();
        await w.get("[data-testid='header-star']").trigger("click");
        await settle();

        expect(prefsOf().setNotifyMode).toHaveBeenCalledWith(team.channel_id, "mentions", true);
        expect(prefsOf().setStarred).toHaveBeenCalledWith(team.channel_id, true, true);
    });

    test("the header offers Turn on notifications while the shell says the church's timing offers it", async () => {
        const w = await mountBlock(1200);
        await openTeam(w);
        expect(w.find("[data-testid='push-offer']").exists()).toBe(false);

        mockShell!.state.isPushOffered = true;
        await settle();
        await w.get("[data-testid='push-offer']").trigger("click");

        expect(mockShell!.turnOnPush).toHaveBeenCalled();
    });

    test("Chat settings opens from the sidebar's foot, and its switches and browser row reach the shell", async () => {
        const w = await mountBlock(1200);

        await w.get("[data-testid='sidebar-me']").trigger("click");
        await settle();

        const dialog = settingsDialog();
        expect(dialog).not.toBeNull();
        expect(document.getElementById(dialog!.getAttribute("aria-labelledby") ?? "")?.textContent?.trim()).toBe("Chat settings");

        document.querySelector<HTMLElement>("[data-testid='switch-open_dm'] [role='switch']")!.click();
        document.querySelector<HTMLElement>("[data-testid='switch-reaction_push'] [role='switch']")!.click();
        document.querySelector<HTMLElement>("[data-testid='push-row'] button")!.click();
        await settle();

        expect(prefsOf().setPersonSetting).toHaveBeenCalledWith("open_dm", true);
        expect(prefsOf().setReactionPush).toHaveBeenCalledWith(false);
        expect(mockShell!.turnOnPush).toHaveBeenCalled();

        mockShell!.state.push = "on";
        await settle();
        document.querySelector<HTMLElement>("[data-testid='push-row'] button")!.click();
        await settle();
        expect(mockShell!.turnOffPush).toHaveBeenCalled();

        document.querySelector<HTMLElement>("[data-testid='settings-close']")!.click();
        await settle();
        expect(settingsDialog()).toBeNull();
        expect(document.activeElement).toBe(w.get("[data-testid='sidebar-me']").element);
    });

    test("Chat settings opens from the person's own card", async () => {
        const w = await mountBlock(1200);
        await openTeam(w);
        roomsOf().state.card = {
            personAliasGuid: "a0000001-0000-4000-8000-000000000000",
            channelId: team.channel_id,
            person: {
                person_alias_guid: "a0000001-0000-4000-8000-000000000000",
                nick_name: "Jordan",
                last_name: "Ellis",
                avatar_url: null,
                is_self: true,
                is_leader: false,
                details: null,
                can_message: false,
                dm_channel_id: null
            }
        };
        await settle();

        document.querySelector<HTMLElement>("[data-testid='profile-settings']")!.click();
        await settle();

        expect(roomsOf().closeCard).toHaveBeenCalled();
        expect(settingsDialog()).not.toBeNull();
    });

    test("the composer gives way to the locked bar with the row's sentence where the person cannot post", async () => {
        const w = await mountBlock(1200);
        const rows = (mockShell!.channels as { rows: SidebarRow[] }).rows;
        rows.splice(0, 1, { ...team, can_post: false, can_post_code: "rpc.not_allowed", can_post_reason: "Only leaders can post in Worship Team." });
        await settle();
        await openTeam(w);

        expect(w.get("[data-testid='frame-main'] [data-testid='composer-locked']").text()).toBe("Only leaders can post in Worship Team.");
    });

    test("the composer counts down to the row's limit, and a send over it is toasted", async () => {
        const w = await mountBlock(1200);
        const rows = (mockShell!.channels as { rows: SidebarRow[] }).rows;
        rows.splice(0, 1, { ...team, max_length: 5000 });
        await settle();
        await openTeam(w);

        const composer = w.findComponent(ComposerComponent);
        expect(composer.props("maxLength")).toBe(5000);

        composer.vm.$emit("overLimit", 5000);
        await settle();

        expect(w.text()).toContain("This message is over the 5,000 character limit.");
    });
});
