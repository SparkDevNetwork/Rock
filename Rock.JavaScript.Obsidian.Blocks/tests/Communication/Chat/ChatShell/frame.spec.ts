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
import { SidebarRow, TimelineMessage } from "../../../../src/Communication/Chat/ChatShell/types.partial";

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
        read_cursor: null,
        is_unread: false,
        mention_count: 0,
        dm_names: null,
        dm_other_count: null,
        ...overrides
    };
}

/** The shell the test mounted last, so a test can open a draft on it. */
let mockShell: Record<string, unknown> | null = null;

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
        isPushOffered: false
    });

    mockShell = {
        state,
        channels: reactive({ rows: [team, dmAnna] }),
        timelines: {
            state: timelineOf
        },
        sender: { pending: () => [], retry: jest.fn(), discard: jest.fn() },
        directMessages: reactive({ draft: null, held: null, chosen: [], error: null, search: jest.fn(), choose: jest.fn(), unchoose: jest.fn(), edit: jest.fn() }),
        isNewMessageShown: true,
        openDirectMessage: jest.fn(),
        start: jest.fn(async () => undefined),
        stop: jest.fn(async () => undefined),
        selectChannel: jest.fn(async (channelId: string) => {
            state.activeChannelId = channelId;
        }),
        seen: jest.fn(),
        markRead: jest.fn(),
        firstMessageShown: jest.fn(),
        send: jest.fn(),
        turnOnPush: jest.fn(),
        isChannelOnScreen: () => false,
        mentionBadge: () => 0,
        loadOlder: jest.fn(),
        dismissError: jest.fn(),
        sendReply: jest.fn(),
        threads: fakeThreads()
    };

    return mockShell;
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
            state.thread = { channelId, rootId, root, replies: [], following: false, readCursor: null, hasMore: false, isLoading: false, error: null };
        }),
        loadOlderReplies: jest.fn(async () => undefined),
        seen: jest.fn(),
        closeThread: jest.fn(async () => {
            state.thread = null;
        }),
        attach: jest.fn(() => () => undefined),
        applyEvent: jest.fn(),
        stop: jest.fn()
    };
}

jest.mock("@Obsidian/Utility/block", () => ({
    useConfigurationValues: () => mockConfig,
    useInvokeBlockAction: () => jest.fn()
}));

jest.mock("@Obsidian/Libs/supabase", () => ({ createClient: jest.fn() }));

jest.mock("../../../../src/Communication/Chat/ChatShell/shell.partial", () => ({
    ...jest.requireActual("../../../../src/Communication/Chat/ChatShell/shell.partial"),
    createChatShell: () => fakeShell()
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

async function settle(): Promise<void> {
    for (let i = 0; i < 4; i++) {
        await nextTick();
    }
    await Promise.resolve();
}

async function mountBlock(width: number): Promise<VueWrapper> {
    blockWidth = width;
    const ChatShell = (await import("../../../../src/Communication/Chat/chatShell.obs")).default;
    wrapper = mount(ChatShell, { attachTo: document.body });
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

        expect(document.activeElement).toBe(w.get("[data-testid='header-title']").element);
    });

    test("returns on Back to the row of the conversation just left", async () => {
        const w = await mountBlock(390);
        await openRow(w, dmAnna.channel_id);

        await w.get("[data-testid='header-back']").trigger("click");
        await settle();

        expect(document.activeElement).toBe(w.get(`[data-testid='channel-row'][data-channel-id='${dmAnna.channel_id}']`).element);
    });

    test("returns on Back from a New message draft, which has no row, to the New message button", async () => {
        const w = await mountBlock(390);
        (mockShell!.directMessages as { draft: unknown }).draft = { people: [{ nick_name: "Anna", last_name: "Kim" }], status: "draft", body: "" };
        await settle();
        expect(w.get("[data-testid='chat-frame']").attributes("data-pane")).toBe("conversation");

        const back = w.get("[data-testid='header-back']");
        (back.element as HTMLElement).focus();
        await back.trigger("click");
        await settle();

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

    test("a New message draft is Conversational", async () => {
        const w = await mountBlock(1200);
        await openRow(w, team.channel_id);

        (mockShell!.state as { activeChannelId: string | null }).activeChannelId = null;
        (mockShell!.directMessages as { draft: unknown }).draft = { people: [{ nick_name: "Anna", last_name: "Kim" }], status: "draft", body: "" };
        await settle();

        expect(w.find("[data-testid='dm-draft']").exists()).toBe(true);
        expect(lookOf(w)).toBe("conversational");
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
        await input.setValue("Count me in");
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
