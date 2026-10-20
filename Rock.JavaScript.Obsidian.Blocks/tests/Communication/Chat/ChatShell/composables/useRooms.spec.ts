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
// Rooms: Browse channels, the facts of each room opened, Join, and the row a previewed room keeps
// in the sidebar.
//
// The directory's search waits 200 ms after typing and drops an answer older than the newest
// query; its filters ask again at once; its next page loads after the last row it holds. A room's
// facts are read on every open and again on that room's membership change, the newest read kept.
// Join goes through Rock's door: joining, then joined, or pending until a sidebar reload shows the
// row as a member, or refused with the door's own sentence, or failed, saying why, with Join to
// press again. A room opened with no
// sidebar row keeps a muted row after the channels, across any sidebar reload, until another
// conversation opens or the reload brings its real row. Once stopped, nothing more is asked and no
// answer still on its way reaches the screen.
//
// The fakes answer in the platform's and the door's own shapes: chat_get_channel's object,
// chat_list_directory's {channels, total}, and ChatMembershipResultBag's {code, message,
// isPending, personAliasGuid}.
/* eslint-disable @typescript-eslint/naming-convention */
import { ChannelFactsResult, createRooms, DirectoryPageResult, DirectoryRequest, directoryPageSize, directorySearchPause } from "../../../../../src/Communication/Chat/ChatShell/composables/useRooms.partial";
import { JoinResult } from "../../../../../src/Communication/Chat/ChatShell/composables/useSend.partial";
import { createChannelStore } from "../../../../../src/Communication/Chat/ChatShell/stores/channelStore.partial";
import { ChannelFacts, DirectoryRow, SidebarRow } from "../../../../../src/Communication/Chat/ChatShell/types.partial";

// #region Fixtures

const volunteers = "c0000001-0000-4000-8000-000000000000";
const outreach = "c0000002-0000-4000-8000-000000000000";
const team = "c0000003-0000-4000-8000-000000000000";
const north = "ca000001-0000-4000-8000-000000000000";

type Deferred<T> = { promise: Promise<T>, resolve: (value: T) => void, reject: (error: unknown) => void };

function deferred<T>(): Deferred<T> {
    let resolve!: (value: T) => void;
    let reject!: (error: unknown) => void;
    const promise = new Promise<T>((res, rej) => {
        resolve = res;
        reject = rej;
    });
    return { promise, resolve, reject };
}

async function settle(): Promise<void> {
    for (let i = 0; i < 20; i++) {
        await Promise.resolve();
    }
}

/** A directory row in the exact shape chat_list_directory answers each channel. */
function directoryRow(channelId: string, name: string, extra: Partial<DirectoryRow> = {}): DirectoryRow {
    return {
        channel_id: channelId,
        name,
        icon_url: null,
        campus_id: null,
        is_member: false,
        can_post_code: null,
        description: `About ${name}`,
        group_type_name: "Serving Team",
        ...extra
    };
}

/** chat_get_channel's answer for a public channel the reader has not joined. */
function facts(channelId: string, name: string, extra: Partial<ChannelFacts> = {}): ChannelFacts {
    return {
        channel_id: channelId,
        name,
        icon_url: null,
        channel_type: "shared",
        is_member: false,
        can_post: false,
        can_post_code: "authz.join_required",
        can_post_reason: "Join this channel to post.",
        max_length: 4000,
        can_leave: false,
        description: `About ${name}`,
        group_type_name: "Serving Team",
        campus_id: null,
        is_public: true,
        member_count: 58,
        leaders: [{ person_alias_guid: "a0000009-0000-4000-8000-000000000000", nick_name: "Nathan", last_name: "Hughes", avatar_url: null }],
        people: [],
        ...extra
    };
}

/** A sidebar row in the exact shape chat_get_bootstrap answers. */
function sidebarRow(channelId: string, extra: Partial<SidebarRow> = {}): SidebarRow {
    return {
        channel_id: channelId,
        name: "Room",
        icon_url: null,
        channel_type: "shared",
        is_public: true,
        always_shown: false,
        is_member: true,
        is_favorite: false,
        notify_mode: "all",
        is_leader: false,
        can_post_announcements: false,
        can_mention_all: false,
        last_message_id: 10,
        last_message_at: "2026-10-19T09:00:00Z",
        last_message_type: "text",
        last_sender_nick_name: "Ada",
        last_sender_last_name: "Member",
        last_sender_listed: true,
        last_message_preview: "old",
        last_message_is_own: false,
        last_message_posted_by_rock: false,
        read_cursor: 10,
        is_unread: false,
        mention_count: 0,
        all_mention_count: 0,
        can_mention: true,
        dm_names: null,
        dm_other_count: null,
        ...extra
    };
}

/** The door's answer, as ChatMembershipResultBag reaches the browser. */
function door(code: string, message: string | null = null, isPending = false): JoinResult & { personAliasGuid: string | null } {
    return { code, message, isPending, personAliasGuid: code === "ok" ? "a0000001-0000-4000-8000-000000000000" : null };
}

function page(channels: DirectoryRow[], total = channels.length): DirectoryPageResult {
    return { ok: true, page: { channels, total } };
}

/** Rows a, b, c... of one page, named so they sort in the order made. */
function rows(count: number, from = 0): DirectoryRow[] {
    return Array.from({ length: count }, (_, i) => {
        const n = from + i;
        return directoryRow(`d0000000-0000-4000-8000-${String(n).padStart(12, "0")}`, `Room ${String(n).padStart(3, "0")}`);
    });
}

type Setup = {
    rooms: ReturnType<typeof createRooms>;
    channels: ReturnType<typeof createChannelStore>;
    directoryCalls: Array<{ request: DirectoryRequest, answer: Deferred<DirectoryPageResult> }>;
    channelCalls: Array<{ channelId: string, answer: Deferred<ChannelFactsResult> }>;
    joinCalls: Array<{ channelId: string, answer: Deferred<JoinResult> }>;
    toasts: Array<{ code: string, text: string, icon: string }>;
    active: { value: string | null };
    offline: { value: boolean };
};

function setup(options: { isCampusLimited?: boolean } = {}): Setup {
    const directoryCalls: Setup["directoryCalls"] = [];
    const channelCalls: Setup["channelCalls"] = [];
    const joinCalls: Setup["joinCalls"] = [];
    const toasts: Setup["toasts"] = [];
    const active = { value: null as string | null };
    const offline = { value: false };
    const channels = createChannelStore({ reloadSidebar: () => undefined });

    const rooms = createRooms({
        listDirectory: request => {
            const answer = deferred<DirectoryPageResult>();
            directoryCalls.push({ request, answer });
            return answer.promise;
        },
        getChannel: channelId => {
            const answer = deferred<ChannelFactsResult>();
            channelCalls.push({ channelId, answer });
            return answer.promise;
        },
        joinChannel: channelId => {
            const answer = deferred<JoinResult>();
            joinCalls.push({ channelId, answer });
            return answer.promise;
        },
        channels,
        activeChannelId: () => active.value,
        isOffline: () => offline.value,
        isCampusLimited: options.isCampusLimited ?? false,
        notify: notice => toasts.push(notice)
    });

    return { rooms, channels, directoryCalls, channelCalls, joinCalls, toasts, active, offline };
}

/** Opens Browse channels and answers its first page. */
async function opened(s: Setup, first: DirectoryRow[], total = first.length): Promise<void> {
    s.rooms.openDirectory();
    s.directoryCalls[s.directoryCalls.length - 1].answer.resolve(page(first, total));
    await settle();
}

// #endregion Fixtures

describe("the directory", () => {
    beforeEach(() => {
        jest.useFakeTimers();
    });

    afterEach(() => {
        jest.useRealTimers();
    });

    test("opening it asks the first page at once with no filters, loading until it lands", async () => {
        const s = setup();

        s.rooms.openDirectory();

        expect(s.directoryCalls.map(c => c.request)).toEqual([
            { query: null, campusId: null, wholeChurch: false, notJoined: false, after: null, limit: directoryPageSize }
        ]);
        expect(s.rooms.state.directory).toEqual(expect.objectContaining({ isOpen: true, isLoading: true, rows: [] }));

        s.directoryCalls[0].answer.resolve(page([directoryRow(volunteers, "Volunteers")], 7));
        await settle();

        expect(s.rooms.state.directory).toEqual(expect.objectContaining({ isLoading: false, total: 7 }));
        expect(s.rooms.state.directory.rows.map(r => r.name)).toEqual(["Volunteers"]);
    });

    test("the search waits for 200 ms after the last keystroke, searching meanwhile", async () => {
        const s = setup();
        await opened(s, rows(3));

        s.rooms.setQuery("vol");
        jest.advanceTimersByTime(directorySearchPause - 1);
        expect(s.directoryCalls).toHaveLength(1);
        expect(s.rooms.state.directory.isSearching).toBe(true);

        s.rooms.setQuery("volu");
        jest.advanceTimersByTime(directorySearchPause - 1);
        expect(s.directoryCalls).toHaveLength(1);

        jest.advanceTimersByTime(1);
        expect(s.directoryCalls).toHaveLength(2);
        expect(s.directoryCalls[1].request).toEqual(expect.objectContaining({ query: "volu", after: null }));
        expect(directorySearchPause).toBe(200);
    });

    test("an answer older than the newest query is dropped, whichever lands first", async () => {
        const s = setup();
        await opened(s, rows(3));

        s.rooms.setQuery("v");
        jest.advanceTimersByTime(directorySearchPause);
        s.rooms.setQuery("vo");
        jest.advanceTimersByTime(directorySearchPause);
        const [, older, newer] = s.directoryCalls;

        // the older answer lands first and changes nothing
        older.answer.resolve(page([directoryRow(outreach, "Valley Outreach"), directoryRow(volunteers, "Volunteers")]));
        await settle();
        expect(s.rooms.state.directory.rows.map(r => r.name)).toEqual(["Room 000", "Room 001", "Room 002"]);
        expect(s.rooms.state.directory.isSearching).toBe(true);

        newer.answer.resolve(page([directoryRow(volunteers, "Volunteers")]));
        await settle();
        expect(s.rooms.state.directory.rows.map(r => r.name)).toEqual(["Volunteers"]);
        expect(s.rooms.state.directory.isSearching).toBe(false);
    });

    test("the newer answer landing first is not overwritten by the older one landing after it", async () => {
        const s = setup();
        await opened(s, rows(3));

        s.rooms.setQuery("v");
        jest.advanceTimersByTime(directorySearchPause);
        s.rooms.setQuery("vo");
        jest.advanceTimersByTime(directorySearchPause);
        const [, older, newer] = s.directoryCalls;

        newer.answer.resolve(page([directoryRow(volunteers, "Volunteers")]));
        await settle();
        older.answer.resolve(page([directoryRow(outreach, "Valley Outreach")]));
        await settle();

        expect(s.rooms.state.directory.rows.map(r => r.name)).toEqual(["Volunteers"]);
    });

    test("a query typed after a pause began drops the answer already on its way", async () => {
        const s = setup();
        await opened(s, rows(3));

        s.rooms.setQuery("v");
        jest.advanceTimersByTime(directorySearchPause);
        s.rooms.setQuery("vx");
        s.directoryCalls[1].answer.resolve(page([directoryRow(volunteers, "Volunteers")]));
        await settle();

        expect(s.rooms.state.directory.rows.map(r => r.name)).toEqual(["Room 000", "Room 001", "Room 002"]);
        expect(s.rooms.state.directory.isSearching).toBe(true);
    });

    test("a search that lands announces how many channels it found", async () => {
        const s = setup();
        await opened(s, rows(3));

        s.rooms.setQuery("room");
        jest.advanceTimersByTime(directorySearchPause);
        s.directoryCalls[1].answer.resolve(page(rows(2), 2));
        await settle();
        expect(s.rooms.state.announcement).toEqual(expect.objectContaining({ text: "2 channels found", isAssertive: false }));

        s.rooms.setQuery("zzz");
        jest.advanceTimersByTime(directorySearchPause);
        s.directoryCalls[2].answer.resolve(page([], 0));
        await settle();
        expect(s.rooms.state.announcement).toEqual(expect.objectContaining({ text: "No channels found" }));
    });

    test("clearing the search asks again at once, without the pause", async () => {
        const s = setup();
        await opened(s, rows(3));
        s.rooms.setQuery("vol");

        s.rooms.clearQuery();

        expect(s.rooms.state.directory.query).toBe("");
        expect(s.directoryCalls).toHaveLength(2);
        expect(s.directoryCalls[1].request.query).toBeNull();
        jest.advanceTimersByTime(directorySearchPause);
        expect(s.directoryCalls).toHaveLength(2);
    });

    test("each filter asks again at once: one campus, the whole church, not joined", async () => {
        const s = setup();
        await opened(s, rows(3), 7);

        s.rooms.setCampus({ kind: "campus", campusId: north });
        expect(s.directoryCalls[1].request).toEqual(expect.objectContaining({ campusId: north, wholeChurch: false, notJoined: false }));
        s.directoryCalls[1].answer.resolve(page(rows(2), 2));
        await settle();
        expect(s.rooms.state.directory.total).toBe(2);

        s.rooms.setCampus({ kind: "wholeChurch" });
        expect(s.directoryCalls[2].request).toEqual(expect.objectContaining({ campusId: null, wholeChurch: true }));

        s.rooms.setShow("notJoined");
        expect(s.directoryCalls[3].request).toEqual(expect.objectContaining({ campusId: null, wholeChurch: true, notJoined: true }));

        s.rooms.setCampus({ kind: "all" });
        expect(s.directoryCalls[4].request).toEqual(expect.objectContaining({ campusId: null, wholeChurch: false, notJoined: true }));
        expect(s.rooms.state.directory).toEqual(expect.objectContaining({ campus: { kind: "all" }, show: "notJoined" }));
    });

    test("under the church's campus limit the chips give way to its note and no campus is asked for", async () => {
        const s = setup({ isCampusLimited: true });
        await opened(s, rows(3));

        s.rooms.setCampus({ kind: "campus", campusId: north });

        expect(s.rooms.state.directory.isCampusLimited).toBe(true);
        expect(s.directoryCalls).toHaveLength(1);
        expect(s.directoryCalls[0].request).toEqual(expect.objectContaining({ campusId: null, wholeChurch: false }));
    });

    test("the next page starts after the last row held, and a short page is the end", async () => {
        const s = setup();
        await opened(s, rows(directoryPageSize), directoryPageSize + 10);
        expect(s.rooms.state.directory.hasMore).toBe(true);

        void s.rooms.loadMore();
        void s.rooms.loadMore();
        expect(s.directoryCalls).toHaveLength(2);
        expect(s.directoryCalls[1].request).toEqual(expect.objectContaining({ after: rows(directoryPageSize)[directoryPageSize - 1].channel_id }));
        expect(s.rooms.state.directory.isLoadingMore).toBe(true);

        s.directoryCalls[1].answer.resolve(page(rows(10, directoryPageSize), directoryPageSize + 10));
        await settle();

        expect(s.rooms.state.directory.rows).toHaveLength(directoryPageSize + 10);
        expect(s.rooms.state.directory).toEqual(expect.objectContaining({ hasMore: false, isLoadingMore: false }));

        await s.rooms.loadMore();
        expect(s.directoryCalls).toHaveLength(2);
    });

    test("a next page asked under an older query is dropped once the query changes", async () => {
        const s = setup();
        await opened(s, rows(directoryPageSize), 100);

        void s.rooms.loadMore();
        s.rooms.setQuery("room 0");
        s.directoryCalls[1].answer.resolve(page(rows(10, directoryPageSize), 100));
        await settle();

        expect(s.rooms.state.directory.rows).toHaveLength(directoryPageSize);
        expect(s.rooms.state.directory.isLoadingMore).toBe(false);
    });

    test("a first page that fails says so and Try again asks it again", async () => {
        const s = setup();
        s.rooms.openDirectory();
        s.directoryCalls[0].answer.resolve({ ok: false, error: { code: "rpc.transport", severity: "failed" } });
        await settle();

        expect(s.rooms.state.directory).toEqual(expect.objectContaining({ isLoading: false, error: { code: "rpc.transport", severity: "failed" } }));

        void s.rooms.retry();
        expect(s.directoryCalls).toHaveLength(2);
        expect(s.rooms.state.directory.isLoading).toBe(true);
        s.directoryCalls[1].answer.resolve(page(rows(2)));
        await settle();
        expect(s.rooms.state.directory.error).toBeNull();
        expect(s.rooms.state.directory.rows).toHaveLength(2);
    });

    test("a next page that fails keeps the rows and says so", async () => {
        const s = setup();
        await opened(s, rows(directoryPageSize), 100);

        void s.rooms.loadMore();
        s.directoryCalls[1].answer.resolve({ ok: false, error: { code: "rpc.transport", severity: "failed" } });
        await settle();

        expect(s.rooms.state.directory.rows).toHaveLength(directoryPageSize);
        expect(s.rooms.state.directory.moreError).toEqual({ code: "rpc.transport", severity: "failed" });
    });

    test.each([
        ["nothing to browse", (): void => undefined, "none"],
        ["a search alone", (s: Setup): void => s.rooms.clearQuery(), "none"],
        ["a search", (s: Setup): void => {
            s.rooms.setQuery("zzz");
            jest.advanceTimersByTime(directorySearchPause);
        }, "query"],
        ["a search under a campus", (s: Setup): void => {
            s.rooms.setCampus({ kind: "campus", campusId: north });
            s.rooms.setQuery("zzz");
            jest.advanceTimersByTime(directorySearchPause);
        }, "filters"],
        ["not joined", (s: Setup): void => s.rooms.setShow("notJoined"), "allJoined"],
        ["a campus", (s: Setup): void => s.rooms.setCampus({ kind: "campus", campusId: north }), "campus"]
    ])("an empty answer for %s names its empty state", async (_name, act, kind) => {
        const s = setup();
        s.rooms.openDirectory();
        act(s);
        s.directoryCalls[s.directoryCalls.length - 1].answer.resolve(page([], 0));
        await settle();

        expect(s.rooms.state.directory.emptyKind).toBe(kind);
    });

    test("a page with rows has no empty state", async () => {
        const s = setup();
        await opened(s, rows(1));

        expect(s.rooms.state.directory.emptyKind).toBeNull();
    });

    test("opening it again is a fresh visit: no search, no filters, the first page asked again", async () => {
        const s = setup();
        await opened(s, rows(3));
        s.rooms.setCampus({ kind: "wholeChurch" });
        s.rooms.setQuery("vol");
        s.rooms.closeDirectory();

        s.rooms.openDirectory();

        expect(s.rooms.state.directory).toEqual(expect.objectContaining({ query: "", campus: { kind: "all" }, show: "all", isLoading: true, rows: [] }));
        expect(s.directoryCalls[s.directoryCalls.length - 1].request).toEqual(expect.objectContaining({ query: null, campusId: null, wholeChurch: false, notJoined: false }));
        // the search typed before closing never goes out
        jest.advanceTimersByTime(directorySearchPause);
        expect(s.directoryCalls.filter(c => c.request.query === "vol")).toHaveLength(0);
    });

    test("an answer still on its way when the directory closes is dropped", async () => {
        const s = setup();
        s.rooms.openDirectory();
        s.rooms.closeDirectory();

        s.directoryCalls[0].answer.resolve(page(rows(3)));
        await settle();

        expect(s.rooms.state.directory.rows).toEqual([]);
        expect(s.rooms.state.directory.isOpen).toBe(false);
    });
});

describe("a room's facts", () => {
    test("are read when a conversation opens and kept for each conversation", async () => {
        const s = setup();

        void s.rooms.channelOpened(volunteers);
        void s.rooms.channelOpened(team);
        expect(s.channelCalls.map(c => c.channelId)).toEqual([volunteers, team]);

        s.channelCalls[0].answer.resolve({ ok: true, facts: facts(volunteers, "Volunteers") });
        s.channelCalls[1].answer.resolve({ ok: true, facts: facts(team, "Tech Team", { is_member: true, member_count: 12 }) });
        await settle();

        expect(s.rooms.factsOf(volunteers)).toEqual(facts(volunteers, "Volunteers"));
        expect(s.rooms.factsOf(team)?.member_count).toBe(12);
        expect(s.rooms.factsOf(outreach)).toBeNull();
    });

    test("are read again on every open, and again on that room's membership change", async () => {
        const s = setup();
        void s.rooms.channelOpened(volunteers);
        s.channelCalls[0].answer.resolve({ ok: true, facts: facts(volunteers, "Volunteers") });
        await settle();

        void s.rooms.channelOpened(volunteers);
        s.rooms.membershipChanged(volunteers);
        s.rooms.membershipChanged(outreach);

        expect(s.channelCalls.map(c => c.channelId)).toEqual([volunteers, volunteers, volunteers]);
    });

    test("an older read answering after a newer one is dropped", async () => {
        const s = setup();
        void s.rooms.channelOpened(volunteers);
        s.rooms.membershipChanged(volunteers);
        const [older, newer] = s.channelCalls;

        newer.answer.resolve({ ok: true, facts: facts(volunteers, "Volunteers", { is_member: true, member_count: 59 }) });
        await settle();
        older.answer.resolve({ ok: true, facts: facts(volunteers, "Volunteers") });
        await settle();

        expect(s.rooms.factsOf(volunteers)).toEqual(expect.objectContaining({ is_member: true, member_count: 59 }));
    });

    test("a read that fails keeps the facts already held", async () => {
        const s = setup();
        void s.rooms.channelOpened(volunteers);
        s.channelCalls[0].answer.resolve({ ok: true, facts: facts(volunteers, "Volunteers") });
        await settle();

        s.rooms.membershipChanged(volunteers);
        s.channelCalls[1].answer.resolve({ ok: false, error: { code: "rpc.transport", severity: "failed" } });
        await settle();

        expect(s.rooms.factsOf(volunteers)?.name).toBe("Volunteers");
    });
});

describe("a previewed room", () => {
    test("opened with no sidebar row joins the sidebar after the channels as a room not joined, from its facts", async () => {
        const s = setup();
        s.channels.setSidebar([sidebarRow(team, { name: "Tech Team" })]);
        s.active.value = volunteers;

        void s.rooms.channelOpened(volunteers);
        s.channelCalls[0].answer.resolve({ ok: true, facts: facts(volunteers, "Volunteers") });
        await settle();

        expect(s.channels.rows.map(r => r.channel_id)).toEqual([team, volunteers]);
        expect(s.channels.rows[1]).toEqual(expect.objectContaining({ name: "Volunteers", is_member: false, is_public: true, channel_type: "shared", is_unread: false, mention_count: 0 }));
        expect(s.channels.previewChannelId).toBe(volunteers);
    });

    test("whose facts land after the person moved on puts no row in the sidebar", async () => {
        const s = setup();
        s.active.value = volunteers;
        void s.rooms.channelOpened(volunteers);
        s.active.value = team;

        s.channelCalls[0].answer.resolve({ ok: true, facts: facts(volunteers, "Volunteers") });
        await settle();

        expect(s.channels.rows).toEqual([]);
    });

    test("opened from a directory row shows its row at once, before its facts", async () => {
        jest.useFakeTimers();
        const s = setup();
        await opened(s, [directoryRow(volunteers, "Volunteers"), directoryRow(team, "Tech Team", { is_member: true })]);
        jest.useRealTimers();

        s.rooms.previewRoom(volunteers);

        expect(s.channels.rows).toEqual([expect.objectContaining({ channel_id: volunteers, name: "Volunteers", is_member: false })]);

        // a room the person is in has a row of its own to come, so it is not previewed
        s.rooms.previewRoom(team);
        expect(s.channels.rows.map(r => r.channel_id)).toEqual([volunteers]);
    });

    test("a room with a sidebar row is not previewed again", async () => {
        const s = setup();
        s.channels.setSidebar([sidebarRow(volunteers, { name: "Volunteers", is_member: false, always_shown: true })]);
        s.active.value = volunteers;

        void s.rooms.channelOpened(volunteers);
        s.channelCalls[0].answer.resolve({ ok: true, facts: facts(volunteers, "Volunteers") });
        await settle();

        expect(s.channels.rows).toHaveLength(1);
        expect(s.channels.previewChannelId).toBeNull();
    });

    test("is kept across a sidebar reload that does not hold it, and ends when the reload brings its row", async () => {
        const s = setup();
        s.channels.setSidebar([sidebarRow(team)]);
        s.active.value = volunteers;
        void s.rooms.channelOpened(volunteers);
        s.channelCalls[0].answer.resolve({ ok: true, facts: facts(volunteers, "Volunteers") });
        await settle();

        // a reload asked before the room was opened answers without it
        s.channels.setSidebar([sidebarRow(team)]);
        expect(s.channels.rows.map(r => r.channel_id)).toEqual([team, volunteers]);

        s.channels.setSidebar([sidebarRow(team), sidebarRow(volunteers, { name: "Volunteers" })]);
        expect(s.channels.rows.map(r => r.channel_id)).toEqual([team, volunteers]);
        expect(s.channels.rows[1].is_member).toBe(true);
        expect(s.channels.previewChannelId).toBeNull();
    });

    test("ends when another conversation opens, and stays while its own is shown", async () => {
        const s = setup();
        s.active.value = volunteers;
        void s.rooms.channelOpened(volunteers);
        s.channelCalls[0].answer.resolve({ ok: true, facts: facts(volunteers, "Volunteers") });
        await settle();

        s.rooms.conversationShown(volunteers);
        expect(s.channels.rows).toHaveLength(1);

        s.rooms.conversationShown(team);
        expect(s.channels.rows).toEqual([]);
        expect(s.channels.previewChannelId).toBeNull();
    });

    test("ends when a draft or nothing takes the conversation's place", async () => {
        const s = setup();
        s.active.value = volunteers;
        void s.rooms.channelOpened(volunteers);
        s.channelCalls[0].answer.resolve({ ok: true, facts: facts(volunteers, "Volunteers") });
        await settle();

        s.rooms.conversationShown(null);

        expect(s.channels.rows).toEqual([]);
    });
});

describe("Join", () => {
    /** A preview of Volunteers open from the directory, with its facts in. */
    async function previewing(): Promise<Setup> {
        jest.useFakeTimers();
        const s = setup();
        await opened(s, [directoryRow(volunteers, "Volunteers"), directoryRow(outreach, "Local Outreach")]);
        jest.useRealTimers();
        s.channels.setSidebar([sidebarRow(team)]);
        s.rooms.previewRoom(volunteers);
        s.active.value = volunteers;
        void s.rooms.channelOpened(volunteers);
        s.channelCalls[0].answer.resolve({ ok: true, facts: facts(volunteers, "Volunteers") });
        await settle();
        return s;
    }

    test("joining, then joined: the toast and the announcement name the room, and the room's rows turn member", async () => {
        const s = await previewing();

        void s.rooms.join(volunteers);
        expect(s.rooms.joinOf(volunteers)).toEqual({ kind: "joining" });
        expect(s.joinCalls.map(c => c.channelId)).toEqual([volunteers]);

        s.joinCalls[0].answer.resolve(door("ok"));
        await settle();

        expect(s.rooms.joinOf(volunteers)).toEqual({ kind: "joined", isFresh: true });
        expect(s.toasts).toEqual([{ code: "room.joined", text: "Joined Volunteers", icon: "check" }]);
        expect(s.rooms.state.announcement).toEqual(expect.objectContaining({ text: "You joined Volunteers", isAssertive: false }));
        expect(s.rooms.state.directory.rows.find(r => r.channel_id === volunteers)?.is_member).toBe(true);
        expect(s.channels.rows.find(r => r.channel_id === volunteers)?.is_member).toBe(true);
        expect(s.rooms.factsOf(volunteers)?.is_member).toBe(true);

        // what the room now allows is read again
        expect(s.channelCalls.map(c => c.channelId)).toEqual([volunteers, volunteers]);
    });

    test("a second press while joining asks the door once", async () => {
        const s = await previewing();

        void s.rooms.join(volunteers);
        void s.rooms.join(volunteers);

        expect(s.joinCalls).toHaveLength(1);
    });

    test("pending says it is finishing up until the sidebar reload shows the room as a member", async () => {
        const s = await previewing();
        void s.rooms.join(outreach);
        s.joinCalls[0].answer.resolve(door("ok", null, true));
        await settle();

        expect(s.rooms.joinOf(outreach)).toEqual({ kind: "pending" });
        expect(s.rooms.state.announcement).toEqual(expect.objectContaining({ text: "You're in Local Outreach. It can take a few seconds to show up." }));
        expect(s.toasts).toEqual([]);

        // a reload that does not hold the room yet leaves it pending
        s.channels.setSidebar([sidebarRow(team)]);
        s.rooms.sidebarLoaded();
        expect(s.rooms.joinOf(outreach)).toEqual({ kind: "pending" });

        // the reload membership.changed asks shows the row as a member
        s.channels.setSidebar([sidebarRow(team), sidebarRow(outreach, { name: "Local Outreach" })]);
        s.rooms.sidebarLoaded();

        expect(s.rooms.joinOf(outreach)).toEqual({ kind: "joined", isFresh: true });
        expect(s.toasts).toEqual([{ code: "room.joined", text: "Joined Local Outreach", icon: "check" }]);
        expect(s.rooms.state.directory.rows.find(r => r.channel_id === outreach)?.is_member).toBe(true);
    });

    test("a pending join of the room on screen keeps its preview not joined until the reload brings its row", async () => {
        const s = await previewing();
        void s.rooms.join(volunteers);
        s.joinCalls[0].answer.resolve(door("ok", null, true));
        await settle();

        expect(s.channels.rows.find(r => r.channel_id === volunteers)?.is_member).toBe(false);

        s.channels.setSidebar([sidebarRow(team), sidebarRow(volunteers, { name: "Volunteers" })]);
        s.rooms.sidebarLoaded();

        expect(s.rooms.joinOf(volunteers)).toEqual({ kind: "joined", isFresh: true });
        expect(s.channels.rows.filter(r => r.channel_id === volunteers)).toEqual([expect.objectContaining({ is_member: true })]);
    });

    test("refused shows the door's own sentence, announced assertively, and the door is not asked again", async () => {
        const s = await previewing();
        void s.rooms.join(volunteers);
        s.joinCalls[0].answer.resolve(door("door.not_allowed", "Tech Team is full right now. Ask Nathan Hughes about joining."));
        await settle();

        expect(s.rooms.joinOf(volunteers)).toEqual({ kind: "refused", text: "Tech Team is full right now. Ask Nathan Hughes about joining." });
        expect(s.rooms.state.announcement).toEqual(expect.objectContaining({ text: "Tech Team is full right now. Ask Nathan Hughes about joining.", isAssertive: true }));
        expect(s.channels.rows.find(r => r.channel_id === volunteers)?.is_member).toBe(false);

        void s.rooms.join(volunteers);
        expect(s.joinCalls).toHaveLength(1);
    });

    test("refused without a sentence says the person can't join", async () => {
        const s = await previewing();
        void s.rooms.join(volunteers);
        s.joinCalls[0].answer.resolve(door("door.not_found", null));
        await settle();

        expect(s.rooms.joinOf(volunteers)).toEqual({ kind: "refused", text: "You can't join this channel." });
    });

    test("offline asks no door, says so politely and leaves Join to press again", async () => {
        const s = await previewing();
        s.offline.value = true;

        await s.rooms.join(volunteers);

        expect(s.joinCalls).toEqual([]);
        expect(s.rooms.joinOf(volunteers)).toEqual({ kind: "failed", text: "Couldn't join. You're offline, so try again in a moment." });
        expect(s.rooms.state.announcement).toEqual(expect.objectContaining({ text: "Couldn't join. You're offline, so try again in a moment.", isAssertive: false }));

        s.offline.value = false;
        void s.rooms.join(volunteers);
        expect(s.joinCalls).toHaveLength(1);
    });

    test("a door that cannot be reached is offline too", async () => {
        const s = await previewing();
        void s.rooms.join(volunteers);
        s.joinCalls[0].answer.reject(new Error("network"));
        await settle();

        expect(s.rooms.joinOf(volunteers)).toEqual({ kind: "failed", text: "Couldn't join. You're offline, so try again in a moment." });
    });

    test("a fresh visit to the directory forgets a join that finished and keeps a refusal", async () => {
        const s = await previewing();
        void s.rooms.join(volunteers);
        void s.rooms.join(outreach);
        s.joinCalls[0].answer.resolve(door("ok"));
        s.joinCalls[1].answer.resolve(door("door.not_allowed", "You can't join this channel."));
        await settle();

        s.rooms.closeDirectory();
        s.rooms.openDirectory();

        expect(s.rooms.joinOf(volunteers)).toEqual({ kind: "idle" });
        expect(s.rooms.joinOf(outreach)).toEqual({ kind: "refused", text: "You can't join this channel." });
    });
});

describe("once stopped", () => {
    test("nothing still on its way reaches the screen and nothing more is asked", async () => {
        jest.useFakeTimers();
        const s = setup();
        s.active.value = volunteers;
        s.rooms.openDirectory();
        s.rooms.setQuery("vol");
        void s.rooms.channelOpened(volunteers);
        void s.rooms.join(volunteers);

        s.rooms.stop();
        jest.advanceTimersByTime(directorySearchPause);
        jest.useRealTimers();

        s.directoryCalls[0].answer.resolve(page(rows(3)));
        s.channelCalls[0].answer.resolve({ ok: true, facts: facts(volunteers, "Volunteers") });
        s.joinCalls[0].answer.resolve(door("ok"));
        await settle();

        expect(s.directoryCalls).toHaveLength(1);
        expect(s.rooms.state.directory.rows).toEqual([]);
        expect(s.rooms.factsOf(volunteers)).toBeNull();
        expect(s.channels.rows).toEqual([]);
        expect(s.toasts).toEqual([]);
        expect(s.rooms.joinOf(volunteers)).toEqual({ kind: "joining" });

        s.rooms.openDirectory();
        s.rooms.membershipChanged(volunteers);
        void s.rooms.channelOpened(volunteers);
        await s.rooms.join(outreach);
        await s.rooms.loadMore();
        expect(s.directoryCalls).toHaveLength(1);
        expect(s.channelCalls).toHaveLength(1);
        expect(s.joinCalls).toHaveLength(1);
    });
});
