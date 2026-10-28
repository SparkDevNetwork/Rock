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
// conversation opens or the reload brings its real row. Leave and a group's Rename go through
// Rock's doors too, and the profile card reads its person for the room it was opened from. Once
// stopped, nothing more is asked and no answer still on its way reaches the screen.
//
// The fakes answer in the platform's and the door's own shapes: chat_get_channel's object,
// chat_list_directory's {channels, total}, chat_get_person's object, and
// ChatMembershipResultBag's {code, message, isPending, personAliasGuid}.
/* eslint-disable @typescript-eslint/naming-convention */
import { ChatProfileDetailsBag } from "@Obsidian/ViewModels/Blocks/Communication/Chat/ChatShell/chatProfileDetailsBag";
import { ChannelFactsResult, createRooms, DirectoryPageResult, DirectoryRequest, directorySearchPause, PersonProfileResult, profileDetailsWait } from "../../../../../src/Communication/Chat/ChatShell/composables/useRooms.partial";
import { MemberRights } from "../../../../../src/Communication/Chat/ChatShell/composables/useMembers.partial";
import { JoinResult } from "../../../../../src/Communication/Chat/ChatShell/composables/useSend.partial";
import { createChannelStore } from "../../../../../src/Communication/Chat/ChatShell/stores/channelStore.partial";
import { ChannelFacts, DirectoryRow, PersonProfile, SidebarRow } from "../../../../../src/Communication/Chat/ChatShell/types.partial";

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

/** A page of the directory as the platform's limits set it, a size no client would choose. */
const directoryPageSize = 35;

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
        last_sender_avatar_url: null,
        last_sender_listed: true,
        last_message_preview: "old",
        last_message_is_own: false,
        last_message_posted_by_rock: false,
        read_cursor: 10,
        is_unread: false,
        mention_count: 0,
        all_mention_count: 0,
        can_mention: true,
        dm_people: null,
        dm_other_count: null,
        room_kind: "channel",
        title: extra.name ?? "Room",
        preview_line: "",
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
    leaveCalls: Array<{ channelId: string, answer: Deferred<JoinResult> }>;
    renameCalls: Array<{ channelId: string, name: string, answer: Deferred<JoinResult> }>;
    personCalls: Array<{ personAliasGuid: string, channelId: string, answer: Deferred<PersonProfileResult> }>;
    rightsCalls: Array<{ channelId: string, answer: Deferred<MemberRights> }>;
    detailsCalls: Array<{ personAliasGuid: string, answer: Deferred<ChatProfileDetailsBag | null> }>;
    reloads: { value: number };
    opened: string[];
    toasts: Array<{ code: string, text: string, icon: string }>;
    active: { value: string | null };
    offline: { value: boolean };
};

function setup(options: { isCampusLimited?: boolean, hasDetailsDoor?: boolean } = {}): Setup {
    const directoryCalls: Setup["directoryCalls"] = [];
    const channelCalls: Setup["channelCalls"] = [];
    const joinCalls: Setup["joinCalls"] = [];
    const leaveCalls: Setup["leaveCalls"] = [];
    const renameCalls: Setup["renameCalls"] = [];
    const personCalls: Setup["personCalls"] = [];
    const rightsCalls: Setup["rightsCalls"] = [];
    const detailsCalls: Setup["detailsCalls"] = [];
    const reloads = { value: 0 };
    const opened: string[] = [];
    const toasts: Setup["toasts"] = [];
    const active = { value: null as string | null };
    const offline = { value: false };
    const channels = createChannelStore({ reloadSidebar: () => undefined, retryFirstLoad: () => undefined });

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
        leaveChannel: channelId => {
            const answer = deferred<JoinResult>();
            leaveCalls.push({ channelId, answer });
            return answer.promise;
        },
        renameConversation: (channelId, name) => {
            const answer = deferred<JoinResult>();
            renameCalls.push({ channelId, name, answer });
            return answer.promise;
        },
        getPerson: (personAliasGuid, channelId) => {
            const answer = deferred<PersonProfileResult>();
            personCalls.push({ personAliasGuid, channelId, answer });
            return answer.promise;
        },
        getMemberRights: channelId => {
            const answer = deferred<MemberRights>();
            rightsCalls.push({ channelId, answer });
            return answer.promise;
        },
        getProfileDetails: options.hasDetailsDoor === false
            ? undefined
            : personAliasGuid => {
                const answer = deferred<ChatProfileDetailsBag | null>();
                detailsCalls.push({ personAliasGuid, answer });
                return answer.promise;
            },
        reloadSidebar: () => {
            reloads.value++;
        },
        limits: () => ({ directory_page_size: directoryPageSize }),
        openChannel: channelId => {
            opened.push(channelId);
        },
        channels,
        activeChannelId: () => active.value,
        isOffline: () => offline.value,
        isCampusLimited: options.isCampusLimited ?? false,
        notify: notice => toasts.push(notice)
    });

    return { rooms, channels, directoryCalls, channelCalls, joinCalls, leaveCalls, renameCalls, personCalls, rightsCalls, detailsCalls, reloads, opened, toasts, active, offline };
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
        s.rooms.roomChanged(volunteers);
        s.rooms.roomChanged(outreach);

        expect(s.channelCalls.map(c => c.channelId)).toEqual([volunteers, volunteers, volunteers]);
    });

    test("an older read answering after a newer one is dropped", async () => {
        const s = setup();
        void s.rooms.channelOpened(volunteers);
        s.rooms.roomChanged(volunteers);
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

        s.rooms.roomChanged(volunteers);
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

// The platform lists a direct message only once a message is written in it, so the conversation a
// first message is starting opens with no row of its own.
describe("a direct message not yet written in", () => {
    const started = "d0000005-0000-4000-8000-000000000000";
    const bo = { person_alias_guid: "a0000002-0000-4000-8000-000000000000", nick_name: "Bo", last_name: "Picked", avatar_url: null };
    const cy = { person_alias_guid: "a0000003-0000-4000-8000-000000000000", nick_name: "Cy", last_name: "Second", avatar_url: null };
    const dmFacts = (people: typeof bo[]): ChannelFacts => facts(started, "", {
        name: null, channel_type: "dm", is_member: true, is_public: false, can_post: true, can_post_code: null, can_post_reason: null, max_length: 5000,
        leaders: [], people: people.map(person => ({ ...person, details: null }))
    });

    test("opens on a stand-in titled as New message titled it, held for the header but not listed", () => {
        const s = setup();
        s.channels.setSidebar([sidebarRow(team)]);

        s.rooms.conversationStarted(started, [bo]);

        expect(s.channels.rows.find(r => r.channel_id === started)).toEqual(expect.objectContaining({ title: "Bo Picked", room_kind: "person", is_member: true, channel_type: "dm", dm_other_count: 1 }));
        expect(s.channels.listed.map(r => r.channel_id)).toEqual([team]);

        const group = setup();
        group.rooms.conversationStarted(started, [bo, cy]);
        expect(group.channels.rows).toEqual([expect.objectContaining({ title: "Bo and Cy", room_kind: "group", dm_other_count: 2 })]);
        expect(group.channels.listed).toEqual([]);
    });

    test("takes what its facts say once they land, and one opened with no row by a link stands in from its facts alone", async () => {
        const s = setup();
        s.active.value = started;
        s.rooms.conversationStarted(started, [bo]);

        void s.rooms.channelOpened(started);
        s.channelCalls[0].answer.resolve({ ok: true, facts: dmFacts([bo]) });
        await settle();
        expect(s.channels.rows).toEqual([expect.objectContaining({ title: "Bo Picked", room_kind: "person", max_length: 5000, can_post: true })]);
        expect(s.channels.listed).toEqual([]);

        const linked = setup();
        linked.active.value = started;
        void linked.rooms.channelOpened(started);
        linked.channelCalls[0].answer.resolve({ ok: true, facts: dmFacts([bo, cy]) });
        await settle();
        expect(linked.channels.rows).toEqual([expect.objectContaining({ title: "Bo and Cy", room_kind: "group" })]);
        expect(linked.channels.listed).toEqual([]);
    });

    test("a conversation the sidebar holds is never stood in for", () => {
        const s = setup();
        s.channels.setSidebar([sidebarRow(started, { channel_type: "dm", room_kind: "person", title: "Bo Picked" })]);

        s.rooms.conversationStarted(started, [bo]);

        expect(s.channels.rows).toHaveLength(1);
        expect(s.channels.previewChannelId).toBeNull();
    });

    test("a leave lands on a listed room, never on the stand-in after it", async () => {
        const s = setup();
        s.channels.setSidebar([sidebarRow(team, { name: "Tech Team" }), sidebarRow(volunteers, { name: "Volunteers" })]);
        s.rooms.conversationStarted(started, [bo]);
        s.active.value = volunteers;

        void s.rooms.leave(volunteers);
        s.leaveCalls[0].answer.resolve(door("ok"));
        await settle();

        expect(s.opened).toEqual([team]);
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

    test("a door that cannot be reached once the connection is lost is offline too", async () => {
        const s = await previewing();
        void s.rooms.join(volunteers);
        s.offline.value = true;
        s.joinCalls[0].answer.reject(new Error("network"));
        await settle();

        expect(s.rooms.joinOf(volunteers)).toEqual({ kind: "failed", text: "Couldn't join. You're offline, so try again in a moment." });
    });

    test("Rock failing to answer while online is a failure to press Join again after, not a refusal", async () => {
        const s = await previewing();
        void s.rooms.join(volunteers);
        s.joinCalls[0].answer.resolve(door("door.unavailable", "An error occurred."));
        await settle();

        expect(s.rooms.joinOf(volunteers)).toEqual({ kind: "failed", text: "Couldn't join. Try again in a moment." });
        expect(s.rooms.state.announcement).toEqual(expect.objectContaining({ text: "Couldn't join. Try again in a moment.", isAssertive: false }));

        void s.rooms.join(volunteers);
        expect(s.joinCalls).toHaveLength(2);
    });

    test("a door that throws while online says to try again, not that the person is offline", async () => {
        const s = await previewing();
        void s.rooms.join(volunteers);
        s.joinCalls[0].answer.reject(new Error("server"));
        await settle();

        expect(s.rooms.joinOf(volunteers)).toEqual({ kind: "failed", text: "Couldn't join. Try again in a moment." });
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
        s.rooms.roomChanged(volunteers);
        void s.rooms.channelOpened(volunteers);
        await s.rooms.join(outreach);
        await s.rooms.loadMore();
        expect(s.directoryCalls).toHaveLength(1);
        expect(s.channelCalls).toHaveLength(1);
        expect(s.joinCalls).toHaveLength(1);
    });
});

// The membership doors the details dialog asks: Leave and a group's Rename, each through Rock's
// door and answered in ChatMembershipResultBag's shape. A leave that went through, pending
// included since Rock has made the change, toasts and announces the room by its title and opens
// the next conversation in the sidebar when the one left was on screen. A refusal keeps the door's
// sentence, and the person may ask again.
const group = "c0000004-0000-4000-8000-000000000000";

/** The sidebar as the leave tests hold it: two channels, then a group with no name of its own. */
function sidebarOfThree(s: Setup): void {
    s.channels.setSidebar([
        sidebarRow(team, { name: "Tech Team" }),
        sidebarRow(volunteers, { name: "Volunteers" }),
        sidebarRow(group, { name: null, channel_type: "dm", is_public: false, dm_people: [{ person_alias_guid: "caleb", nick_name: "Caleb", last_name: null, avatar_url: null }, { person_alias_guid: "hannah", nick_name: "Hannah", last_name: null, avatar_url: null }], dm_other_count: 2, room_kind: "group", title: "Caleb and Hannah" })
    ]);
}

describe("Leave", () => {
    test("leaving, then left: the toast and the announcement name the room, and the next conversation opens", async () => {
        const s = setup();
        sidebarOfThree(s);
        s.active.value = volunteers;

        void s.rooms.leave(volunteers);
        expect(s.rooms.leaveOf(volunteers)).toEqual({ kind: "leaving" });
        expect(s.leaveCalls.map(c => c.channelId)).toEqual([volunteers]);

        s.leaveCalls[0].answer.resolve(door("ok"));
        await settle();

        expect(s.rooms.leaveOf(volunteers)).toEqual({ kind: "left" });
        expect(s.toasts).toEqual([{ code: "room.left", text: "You left Volunteers", icon: "door" }]);
        expect(s.rooms.state.announcement).toEqual(expect.objectContaining({ text: "You left Volunteers.", isAssertive: false }));
        expect(s.opened).toEqual([group]);
    });

    test("the next conversation is the one beside it as the leave was asked, though the platform's signal took its row and closed it first", async () => {
        const s = setup();
        sidebarOfThree(s);
        s.active.value = group;

        void s.rooms.leave(group);
        // The door's own sync reaches the person's topic before its answer: the sidebar reloads
        // without the group, and the recheck that follows closes the room on screen.
        s.channels.setSidebar(s.channels.rows.filter(row => row.channel_id !== group));
        s.active.value = null;
        s.leaveCalls[0].answer.resolve(door("ok"));
        await settle();

        expect(s.opened).toEqual([volunteers]);
        expect(s.toasts).toEqual([{ code: "room.left", text: "You left Caleb and Hannah", icon: "door" }]);
    });

    test("leaving the last conversation in the sidebar opens the one before it", async () => {
        const s = setup();
        sidebarOfThree(s);
        s.active.value = group;

        void s.rooms.leave(group);
        s.leaveCalls[0].answer.resolve(door("ok"));
        await settle();

        expect(s.opened).toEqual([volunteers]);
    });

    test("names a group with no name of its own by its people, as the sidebar does", async () => {
        const s = setup();
        sidebarOfThree(s);
        s.active.value = group;

        void s.rooms.leave(group);
        s.leaveCalls[0].answer.resolve(door("ok"));
        await settle();

        expect(s.toasts).toEqual([{ code: "room.left", text: "You left Caleb and Hannah", icon: "door" }]);
    });

    test("a room left while another is on screen opens nothing", async () => {
        const s = setup();
        sidebarOfThree(s);
        s.active.value = team;

        void s.rooms.leave(volunteers);
        s.leaveCalls[0].answer.resolve(door("ok"));
        await settle();

        expect(s.opened).toEqual([]);
        expect(s.rooms.leaveOf(volunteers)).toEqual({ kind: "left" });
    });

    test("pending is left too, since Rock has made the change", async () => {
        const s = setup();
        sidebarOfThree(s);
        s.active.value = volunteers;

        void s.rooms.leave(volunteers);
        s.leaveCalls[0].answer.resolve(door("ok", null, true));
        await settle();

        expect(s.rooms.leaveOf(volunteers)).toEqual({ kind: "left" });
        expect(s.toasts).toEqual([{ code: "room.left", text: "You left Volunteers", icon: "door" }]);
        expect(s.opened).toEqual([group]);
    });

    test("a second press while leaving asks the door once", () => {
        const s = setup();
        sidebarOfThree(s);

        void s.rooms.leave(volunteers);
        void s.rooms.leave(volunteers);

        expect(s.leaveCalls).toHaveLength(1);
    });

    test("refused keeps the door's own sentence, announced assertively, opens nothing, and may be asked again", async () => {
        const s = setup();
        sidebarOfThree(s);
        s.active.value = volunteers;

        void s.rooms.leave(volunteers);
        s.leaveCalls[0].answer.resolve(door("door.not_allowed", "Leaving Volunteers is turned off. A leader can remove you if you need to step away."));
        await settle();

        expect(s.rooms.leaveOf(volunteers)).toEqual({ kind: "refused", text: "Leaving Volunteers is turned off. A leader can remove you if you need to step away." });
        expect(s.rooms.state.announcement).toEqual(expect.objectContaining({ isAssertive: true }));
        expect(s.toasts).toEqual([]);
        expect(s.opened).toEqual([]);

        void s.rooms.leave(volunteers);
        expect(s.leaveCalls).toHaveLength(2);
    });

    test("offline asks no door, and a door that cannot be reached once the connection is lost is offline too", async () => {
        const s = setup();
        sidebarOfThree(s);
        s.offline.value = true;

        await s.rooms.leave(volunteers);
        expect(s.leaveCalls).toEqual([]);
        expect(s.rooms.leaveOf(volunteers)).toEqual({ kind: "offline" });

        s.offline.value = false;
        void s.rooms.leave(volunteers);
        s.offline.value = true;
        s.leaveCalls[0].answer.reject(new Error("network"));
        await settle();
        expect(s.rooms.leaveOf(volunteers)).toEqual({ kind: "offline" });
    });

    test("a door that throws while online failed, and says so without saying the person is offline", async () => {
        const s = setup();
        sidebarOfThree(s);

        void s.rooms.leave(volunteers);
        s.leaveCalls[0].answer.reject(new Error("server"));
        await settle();

        expect(s.rooms.leaveOf(volunteers)).toEqual({ kind: "failed" });
    });

    test("a door Rock answered without its result once the connection is lost is offline", async () => {
        const s = setup();
        sidebarOfThree(s);

        void s.rooms.leave(volunteers);
        s.offline.value = true;
        s.leaveCalls[0].answer.resolve(door("door.unavailable"));
        await settle();

        expect(s.rooms.leaveOf(volunteers)).toEqual({ kind: "offline" });
    });

    test("a door Rock answered without its result while online failed, and may be asked again", async () => {
        const s = setup();
        sidebarOfThree(s);
        s.active.value = volunteers;

        void s.rooms.leave(volunteers);
        s.leaveCalls[0].answer.resolve(door("door.unavailable"));
        await settle();

        expect(s.rooms.leaveOf(volunteers)).toEqual({ kind: "failed" });
        expect(s.opened).toEqual([]);
        void s.rooms.leave(volunteers);
        expect(s.leaveCalls).toHaveLength(2);
    });

    test("a room never asked about is idle", () => {
        const s = setup();

        expect(s.rooms.leaveOf(volunteers)).toEqual({ kind: "idle" });
    });
});

describe("Rename", () => {
    test("the door is asked for the trimmed name; once saved the rename is idle again, and the new name is announced", async () => {
        const s = setup();
        sidebarOfThree(s);

        void s.rooms.rename(group, "  Trail crew ");
        expect(s.rooms.renameOf(group)).toEqual({ kind: "saving" });
        expect(s.renameCalls.map(c => [c.channelId, c.name])).toEqual([[group, "Trail crew"]]);

        s.renameCalls[0].answer.resolve(door("ok"));
        await settle();

        expect(s.rooms.renameOf(group)).toEqual({ kind: "idle" });
        expect(s.rooms.state.announcement).toEqual(expect.objectContaining({ text: "Renamed to Trail crew.", isAssertive: false }));
    });

    test("saved names the group at once in the sidebar row and the facts, which the header and the dialog draw", async () => {
        const s = setup();
        sidebarOfThree(s);
        s.active.value = group;
        void s.rooms.channelOpened(group);
        s.channelCalls[0].answer.resolve({ ok: true, facts: facts(group, null as unknown as string, { name: null, channel_type: "dm", is_public: false }) });
        await settle();

        void s.rooms.rename(group, "Trail crew");
        await settle();
        expect(s.channels.rows.find(r => r.channel_id === group)?.name).toBeNull();
        s.renameCalls[0].answer.resolve(door("ok"));
        await settle();

        const row = s.channels.rows.find(r => r.channel_id === group)!;
        expect(row.name).toBe("Trail crew");
        expect(row.title).toBe("Trail crew");
        expect(s.reloads.value).toBe(0);
        expect(s.rooms.factsOf(group)?.name).toBe("Trail crew");
    });

    test("a name removed and saved reads the sidebar again, whose title of the group by its people is the platform's", async () => {
        const s = setup();
        s.channels.setSidebar([sidebarRow(group, { name: "Trail crew", channel_type: "dm", is_public: false, dm_people: null, dm_other_count: 0, room_kind: "group", title: "Trail crew" })]);
        s.active.value = group;
        void s.rooms.channelOpened(group);
        s.channelCalls[0].answer.resolve({
            ok: true,
            facts: facts(group, "Trail crew", {
                channel_type: "dm",
                is_public: false,
                member_count: 3,
                people: [
                    { person_alias_guid: "a0000005-0000-4000-8000-000000000000", nick_name: "Caleb", last_name: "Brooks", avatar_url: null, details: null },
                    { person_alias_guid: "a0000006-0000-4000-8000-000000000000", nick_name: "Hannah", last_name: "Diaz", avatar_url: null, details: null }
                ]
            })
        });
        await settle();

        void s.rooms.rename(group, "");
        s.renameCalls[0].answer.resolve(door("ok"));
        await settle();

        const row = s.channels.rows.find(r => r.channel_id === group)!;
        expect(row.name).toBeNull();
        expect(s.reloads.value).toBe(1);
        expect(s.rooms.factsOf(group)?.name).toBeNull();
    });

    // The platform has a pending name only once Rock's push lands, which its channel.updated says;
    // until then any other sidebar load or facts read brings the old name back.
    test("a saved name stays through sidebar loads and facts reads until the platform says the room changed", async () => {
        const s = setup();
        sidebarOfThree(s);
        s.active.value = group;
        void s.rooms.channelOpened(group);
        s.channelCalls[0].answer.resolve({ ok: true, facts: facts(group, "Old name", { channel_type: "dm", is_public: false }) });
        await settle();

        void s.rooms.rename(group, "Trail crew");
        s.renameCalls[0].answer.resolve(door("ok", null, true));
        await settle();

        // A load and a facts read that the platform answered before it had the name.
        const old = s.channels.rows.find(r => r.channel_id === group)!;
        s.channels.setSidebar(s.channels.rows.map(r => r.channel_id === group ? { ...old, name: "Old name", title: "Old name" } : r));
        s.rooms.sidebarLoaded();
        s.rooms.roomChanged(group);
        s.channelCalls[1].answer.resolve({ ok: true, facts: facts(group, "Old name", { channel_type: "dm", is_public: false }) });
        await settle();

        expect(s.channels.rows.find(r => r.channel_id === group)?.name).toBe("Trail crew");
        expect(s.channels.rows.find(r => r.channel_id === group)?.title).toBe("Trail crew");
        expect(s.rooms.factsOf(group)?.name).toBe("Trail crew");

        // The platform has it now; what it reads next is taken as read, another's rename included.
        s.rooms.channelUpdated(group);
        s.channelCalls[2].answer.resolve({ ok: true, facts: facts(group, "Hike crew", { channel_type: "dm", is_public: false }) });
        await settle();
        s.channels.setSidebar(s.channels.rows.map(r => r.channel_id === group ? { ...old, name: "Hike crew", title: "Hike crew" } : r));
        s.rooms.sidebarLoaded();

        expect(s.channels.rows.find(r => r.channel_id === group)?.title).toBe("Hike crew");
        expect(s.rooms.factsOf(group)?.name).toBe("Hike crew");
    });

    test("pending has saved too, so it names the group at once, with no reload before the platform has it", async () => {
        const s = setup();
        sidebarOfThree(s);

        void s.rooms.rename(group, "Trail crew");
        s.renameCalls[0].answer.resolve(door("ok", null, true));
        await settle();

        expect(s.channels.rows.find(r => r.channel_id === group)?.name).toBe("Trail crew");
        expect(s.reloads.value).toBe(0);
    });

    test("a name removed and pending is no name at once, with no reload that would read the old one back", async () => {
        const s = setup();
        s.channels.setSidebar([sidebarRow(group, { name: "Trail crew", channel_type: "dm", is_public: false, dm_people: null, dm_other_count: 0, room_kind: "group", title: "Trail crew" })]);

        void s.rooms.rename(group, "");
        s.renameCalls[0].answer.resolve(door("ok", null, true));
        await settle();

        expect(s.channels.rows.find(r => r.channel_id === group)?.name).toBeNull();
        expect(s.reloads.value).toBe(0);
    });

    test("a blank name goes back to everyone's names, and says so", async () => {
        const s = setup();
        sidebarOfThree(s);

        void s.rooms.rename(group, "   ");
        expect(s.renameCalls.map(c => c.name)).toEqual([""]);
        s.renameCalls[0].answer.resolve(door("ok"));
        await settle();

        expect(s.rooms.state.announcement).toEqual(expect.objectContaining({ text: "Name removed. Showing everyone's names." }));
    });

    test("pending shows nothing more than a save that landed", async () => {
        const s = setup();
        sidebarOfThree(s);

        void s.rooms.rename(group, "Trail crew");
        s.renameCalls[0].answer.resolve(door("ok", null, true));
        await settle();

        expect(s.rooms.renameOf(group)).toEqual({ kind: "idle" });
        expect(s.rooms.state.announcement).toEqual(expect.objectContaining({ text: "Renamed to Trail crew.", isAssertive: false }));
    });

    test("refused keeps the door's own sentence", async () => {
        const s = setup();
        sidebarOfThree(s);

        void s.rooms.rename(group, "Trail crew");
        s.renameCalls[0].answer.resolve(door("door.not_allowed", "Chat is read only for a few minutes while Cedar Hill updates it. Try again soon."));
        await settle();

        expect(s.rooms.renameOf(group)).toEqual({ kind: "refused", text: "Chat is read only for a few minutes while Cedar Hill updates it. Try again soon." });
    });

    test("offline asks no door, and a door that cannot be reached once the connection is lost is offline too", async () => {
        const s = setup();
        sidebarOfThree(s);
        s.offline.value = true;

        await s.rooms.rename(group, "Trail crew");
        expect(s.renameCalls).toEqual([]);
        expect(s.rooms.renameOf(group)).toEqual({ kind: "offline" });

        s.offline.value = false;
        void s.rooms.rename(group, "Trail crew");
        s.offline.value = true;
        s.renameCalls[0].answer.reject(new Error("network"));
        await settle();
        expect(s.rooms.renameOf(group)).toEqual({ kind: "offline" });
    });

    test("a door that throws while online failed, and says so without saying the person is offline", async () => {
        const s = setup();
        sidebarOfThree(s);

        void s.rooms.rename(group, "Trail crew");
        s.renameCalls[0].answer.reject(new Error("server"));
        await settle();

        expect(s.rooms.renameOf(group)).toEqual({ kind: "failed" });
    });

    test("a door Rock answered without its result once the connection is lost is offline", async () => {
        const s = setup();
        sidebarOfThree(s);

        void s.rooms.rename(group, "Trail crew");
        s.offline.value = true;
        s.renameCalls[0].answer.resolve(door("door.unavailable"));
        await settle();

        expect(s.rooms.renameOf(group)).toEqual({ kind: "offline" });
    });

    test("a door Rock answered without its result while online failed", async () => {
        const s = setup();
        sidebarOfThree(s);

        void s.rooms.rename(group, "Trail crew");
        s.renameCalls[0].answer.resolve(door("door.unavailable"));
        await settle();

        expect(s.rooms.renameOf(group)).toEqual({ kind: "failed" });
    });

    test("a second save while one is out asks the door once", () => {
        const s = setup();

        void s.rooms.rename(group, "Trail crew");
        void s.rooms.rename(group, "Trail crew");

        expect(s.renameCalls).toHaveLength(1);
    });
});

// The profile card's person, read by chat_get_person for the room the card was opened from. One
// card is open at a time, so a card opened over another drops the first's answer whenever it
// lands; a person the platform does not list draws no card at all.
// Whether the details offer Rename is Rock's answer, asked each time they open: the members door's
// CanRename, by the rule its rename door applies. No answer, or a failed one, offers nothing.
describe("Rename offered", () => {
    test("opening the details asks Rock, and Rename is offered only once it says yes", async () => {
        const s = setup();

        void s.rooms.detailsOpened(group);
        expect(s.rightsCalls.map(c => c.channelId)).toEqual([group]);
        expect(s.rooms.canRenameOf(group)).toBe(false);

        s.rightsCalls[0].answer.resolve({ canManageMembers: true, canRename: true, lastLeaderPersonAliasGuid: null });
        await settle();
        expect(s.rooms.canRenameOf(group)).toBe(true);
        expect(s.rooms.canRenameOf(team)).toBe(false);
    });

    test("each open asks again, and a no takes Rename away", async () => {
        const s = setup();
        void s.rooms.detailsOpened(group);
        s.rightsCalls[0].answer.resolve({ canManageMembers: true, canRename: true, lastLeaderPersonAliasGuid: null });
        await settle();

        void s.rooms.detailsOpened(group);
        s.rightsCalls[1].answer.resolve({ canManageMembers: false, canRename: false, lastLeaderPersonAliasGuid: null });
        await settle();

        expect(s.rightsCalls).toHaveLength(2);
        expect(s.rooms.canRenameOf(group)).toBe(false);
    });

    test("a door that fails offers nothing, and nothing lands once stopped", async () => {
        const s = setup();
        void s.rooms.detailsOpened(group);
        s.rightsCalls[0].answer.reject(new Error("unreachable"));
        await settle();
        expect(s.rooms.canRenameOf(group)).toBe(false);

        void s.rooms.detailsOpened(team);
        s.rooms.stop();
        s.rightsCalls[s.rightsCalls.length - 1].answer.resolve({ canManageMembers: true, canRename: true, lastLeaderPersonAliasGuid: null });
        await settle();
        expect(s.rooms.canRenameOf(team)).toBe(false);
    });
});

describe("the profile card", () => {
    const anna = "a0000002-0000-4000-8000-000000000000";
    const ben = "a0000003-0000-4000-8000-000000000000";

    /** chat_get_person's answer, exactly its keys. */
    function profile(personAliasGuid: string, nickName: string, extra: Partial<PersonProfile> = {}): PersonProfile {
        return {
            person_alias_guid: personAliasGuid,
            nick_name: nickName,
            last_name: "Reyes",
            avatar_url: null,
            campus_id: null,
            is_self: false,
            is_leader: false,
            details: null,
            can_message: true,
            dm_channel_id: null,
            ...extra
        };
    }

    test("opening asks for the person in that room, and the card is drawn once they are read", async () => {
        const s = setup();

        s.rooms.openCard(anna, team);
        expect(s.personCalls.map(c => [c.personAliasGuid, c.channelId])).toEqual([[anna, team]]);
        expect(s.rooms.state.card).toEqual({ personAliasGuid: anna, channelId: team, person: null, rock: { kind: "loading" } });

        s.personCalls[0].answer.resolve({ ok: true, person: profile(anna, "Anna", { is_leader: true }) });
        await settle();

        expect(s.rooms.state.card).toEqual({ personAliasGuid: anna, channelId: team, person: profile(anna, "Anna", { is_leader: true }), rock: { kind: "loading" } });
    });

    test("a card opened over another drops the first one's answer, whichever lands first", async () => {
        const s = setup();
        s.rooms.openCard(anna, team);
        s.rooms.openCard(ben, team);
        const [first, second] = s.personCalls;

        first.answer.resolve({ ok: true, person: profile(anna, "Anna") });
        await settle();
        expect(s.rooms.state.card).toEqual({ personAliasGuid: ben, channelId: team, person: null, rock: { kind: "loading" } });

        second.answer.resolve({ ok: true, person: profile(ben, "Ben") });
        await settle();
        expect(s.rooms.state.card?.person?.nick_name).toBe("Ben");
    });

    test("an older card's answer landing after the newer one's changes nothing", async () => {
        const s = setup();
        s.rooms.openCard(anna, team);
        s.rooms.openCard(ben, team);
        const [first, second] = s.personCalls;

        second.answer.resolve({ ok: true, person: profile(ben, "Ben") });
        await settle();
        first.answer.resolve({ ok: true, person: profile(anna, "Anna") });
        await settle();

        expect(s.rooms.state.card?.person?.nick_name).toBe("Ben");
    });

    test("a person the platform does not list draws no card", async () => {
        const s = setup();
        s.rooms.openCard(anna, team);

        s.personCalls[0].answer.resolve({ ok: false, error: { code: "rpc.person_not_found", severity: "failed", text: "This person isn't available." } });
        await settle();

        expect(s.rooms.state.card).toBeNull();
    });

    test("a read that fails draws no card either", async () => {
        const s = setup();
        s.rooms.openCard(anna, team);

        s.personCalls[0].answer.resolve({ ok: false, error: { code: "rpc.transport", severity: "failed" } });
        await settle();

        expect(s.rooms.state.card).toBeNull();
    });

    test("closing drops an answer still on its way", async () => {
        const s = setup();
        s.rooms.openCard(anna, team);

        s.rooms.closeCard();
        s.personCalls[0].answer.resolve({ ok: true, person: profile(anna, "Anna") });
        await settle();

        expect(s.rooms.state.card).toBeNull();
    });
});

// What Rock adds to the card, its email, phone and template, is read beside the platform's person
// and never copied to chat. A person showing details waits a moment for Rock's answer, so the card
// usually draws once, whole; past that it draws with Rock's rows reserved. Each person's answer is
// kept for the visit, so a card opened again draws whole at once.
describe("the profile card's details from Rock", () => {
    const anna = "a0000002-0000-4000-8000-000000000000";
    const ben = "a0000003-0000-4000-8000-000000000000";

    /** chat_get_person's answer for someone showing details, exactly its keys. */
    function profile(personAliasGuid: string, nickName: string, extra: Partial<PersonProfile> = {}): PersonProfile {
        return {
            person_alias_guid: personAliasGuid,
            nick_name: nickName,
            last_name: "Reyes",
            avatar_url: null,
            campus_id: null,
            is_self: false,
            is_leader: false,
            details: { badge_keys: [] },
            can_message: true,
            dm_channel_id: null,
            ...extra
        };
    }

    function shown(email: string): ChatProfileDetailsBag {
        return { isShown: true, email, mobilePhone: "(602) 555-0142", mobilePhoneUri: "tel:+16025550142", lavaHtml: null };
    }

    beforeEach(() => {
        jest.useFakeTimers();
    });

    afterEach(() => {
        jest.useRealTimers();
    });

    test("opening asks the platform and Rock together", () => {
        const s = setup();

        s.rooms.openCard(anna, team);

        expect(s.personCalls.map(c => c.personAliasGuid)).toEqual([anna]);
        expect(s.detailsCalls.map(c => c.personAliasGuid)).toEqual([anna]);
    });

    test("Rock answering first is kept, and the card draws once, whole", async () => {
        const s = setup();
        s.rooms.openCard(anna, team);

        s.detailsCalls[0].answer.resolve(shown("anna@example.org"));
        await settle();
        expect(s.rooms.state.card?.person).toBeNull();

        s.personCalls[0].answer.resolve({ ok: true, person: profile(anna, "Anna") });
        await settle();

        expect(s.rooms.state.card).toEqual({ personAliasGuid: anna, channelId: team, person: profile(anna, "Anna"), rock: { kind: "answered", details: shown("anna@example.org") } });
    });

    test("someone showing details waits for Rock, and Rock answering in time draws the card once, whole", async () => {
        const s = setup();
        s.rooms.openCard(anna, team);

        s.personCalls[0].answer.resolve({ ok: true, person: profile(anna, "Anna") });
        await settle();
        expect(s.rooms.state.card?.person).toBeNull();

        jest.advanceTimersByTime(profileDetailsWait - 1);
        s.detailsCalls[0].answer.resolve(shown("anna@example.org"));
        await settle();

        expect(s.rooms.state.card?.person?.nick_name).toBe("Anna");
        expect(s.rooms.state.card?.rock).toEqual({ kind: "answered", details: shown("anna@example.org") });
    });

    test("past the wait the card draws with Rock's rows still loading, and fills them when it answers", async () => {
        const s = setup();
        s.rooms.openCard(anna, team);
        s.personCalls[0].answer.resolve({ ok: true, person: profile(anna, "Anna") });
        await settle();

        jest.advanceTimersByTime(profileDetailsWait);
        await settle();
        expect(s.rooms.state.card?.person?.nick_name).toBe("Anna");
        expect(s.rooms.state.card?.rock).toEqual({ kind: "loading" });

        s.detailsCalls[0].answer.resolve(shown("anna@example.org"));
        await settle();
        expect(s.rooms.state.card?.rock).toEqual({ kind: "answered", details: shown("anna@example.org") });
    });

    test("the wait runs from the open, so a slow platform is not waited on twice", async () => {
        const s = setup();
        s.rooms.openCard(anna, team);

        jest.advanceTimersByTime(profileDetailsWait);
        s.personCalls[0].answer.resolve({ ok: true, person: profile(anna, "Anna") });
        await settle();

        expect(s.rooms.state.card?.person?.nick_name).toBe("Anna");
        expect(s.rooms.state.card?.rock).toEqual({ kind: "loading" });
    });

    test("someone keeping details private draws at once, Rock or not", async () => {
        const s = setup();
        s.rooms.openCard(anna, team);

        s.personCalls[0].answer.resolve({ ok: true, person: profile(anna, "Anna", { details: null }) });
        await settle();

        expect(s.rooms.state.card?.person?.nick_name).toBe("Anna");
    });

    test("Rock failing, or answering nothing, leaves its rows out", async () => {
        const s = setup();
        s.rooms.openCard(anna, team);
        s.detailsCalls[0].answer.reject(new Error("unreachable"));
        s.personCalls[0].answer.resolve({ ok: true, person: profile(anna, "Anna") });
        await settle();
        expect(s.rooms.state.card?.person?.nick_name).toBe("Anna");
        expect(s.rooms.state.card?.rock).toEqual({ kind: "failed" });

        s.rooms.openCard(ben, team);
        s.detailsCalls[1].answer.resolve(null);
        s.personCalls[1].answer.resolve({ ok: true, person: profile(ben, "Ben") });
        await settle();
        expect(s.rooms.state.card?.person?.nick_name).toBe("Ben");
        expect(s.rooms.state.card?.rock).toEqual({ kind: "failed" });
    });

    test("an older card's answer from Rock never lands on a newer card", async () => {
        const s = setup();
        s.rooms.openCard(anna, team);
        s.rooms.openCard(ben, team);

        s.detailsCalls[0].answer.resolve(shown("anna@example.org"));
        await settle();

        expect(s.rooms.state.card?.personAliasGuid).toBe(ben);
        expect(s.rooms.state.card?.rock).toEqual({ kind: "loading" });
    });

    test("each person's answer is kept for the visit, so their card opens whole without asking again", async () => {
        const s = setup();
        s.rooms.openCard(anna, team);
        s.detailsCalls[0].answer.resolve(shown("anna@example.org"));
        s.personCalls[0].answer.resolve({ ok: true, person: profile(anna, "Anna") });
        await settle();
        s.rooms.closeCard();

        s.rooms.openCard(anna.toUpperCase(), team);
        expect(s.detailsCalls).toHaveLength(1);
        expect(s.rooms.state.card?.rock).toEqual({ kind: "answered", details: shown("anna@example.org") });

        s.personCalls[1].answer.resolve({ ok: true, person: profile(anna, "Anna") });
        await settle();
        expect(s.rooms.state.card?.person?.nick_name).toBe("Anna");
    });

    test("a failed answer is not kept, so the next open asks again", async () => {
        const s = setup();
        s.rooms.openCard(anna, team);
        s.detailsCalls[0].answer.reject(new Error("unreachable"));
        await settle();
        s.rooms.closeCard();

        s.rooms.openCard(anna, team);

        expect(s.detailsCalls).toHaveLength(2);
    });

    test("after the person's own switch is saved, their open card asks Rock again and keeps the new answer", async () => {
        const s = setup();
        s.rooms.openCard(anna, team);
        s.detailsCalls[0].answer.resolve({ isShown: false });
        s.personCalls[0].answer.resolve({ ok: true, person: profile(anna, "Anna", { is_self: true, details: null }) });
        await settle();

        const saved = deferred<void>();
        s.rooms.refreshCardDetails(saved.promise);
        expect(s.rooms.state.card?.rock).toEqual({ kind: "loading" });
        expect(s.detailsCalls).toHaveLength(1);

        saved.resolve();
        await settle();
        expect(s.detailsCalls).toHaveLength(2);

        s.detailsCalls[1].answer.resolve(shown("anna@example.org"));
        await settle();
        expect(s.rooms.state.card?.rock).toEqual({ kind: "answered", details: shown("anna@example.org") });

        s.rooms.closeCard();
        s.rooms.openCard(anna, team);
        expect(s.rooms.state.card?.rock).toEqual({ kind: "answered", details: shown("anna@example.org") });
    });

    test("an answer that shows nothing is not kept, so a person who turns details on is asked again at the next open", async () => {
        const s = setup();
        s.rooms.openCard(anna, team);
        s.detailsCalls[0].answer.resolve({ isShown: false });
        s.personCalls[0].answer.resolve({ ok: true, person: profile(anna, "Anna", { details: null }) });
        await settle();
        s.rooms.closeCard();

        s.rooms.openCard(anna, team);

        expect(s.detailsCalls).toHaveLength(2);
        expect(s.rooms.state.card?.rock).toEqual({ kind: "loading" });
    });

    test("only the newest ask after quick changes of the switch is drawn and kept, however the answers arrive", async () => {
        const s = setup();
        s.rooms.openCard(anna, team);
        s.detailsCalls[0].answer.resolve(shown("anna@example.org"));
        s.personCalls[0].answer.resolve({ ok: true, person: profile(anna, "Anna", { is_self: true }) });
        await settle();

        s.rooms.refreshCardDetails(Promise.resolve());
        await settle();
        s.rooms.refreshCardDetails(Promise.resolve());
        await settle();
        expect(s.detailsCalls).toHaveLength(3);

        s.detailsCalls[2].answer.resolve({ isShown: false });
        await settle();
        s.detailsCalls[1].answer.resolve(shown("old@example.org"));
        await settle();

        expect(s.rooms.state.card?.rock).toEqual({ kind: "answered", details: { isShown: false } });

        s.rooms.closeCard();
        s.rooms.openCard(anna, team);
        expect(s.detailsCalls).toHaveLength(4);
    });

    test("an answer to an open asked before the switch changed is neither drawn nor kept", async () => {
        const s = setup();
        s.rooms.openCard(anna, team);
        s.personCalls[0].answer.resolve({ ok: true, person: profile(anna, "Anna", { is_self: true }) });
        jest.advanceTimersByTime(profileDetailsWait);
        await settle();

        s.rooms.refreshCardDetails(Promise.resolve());
        await settle();
        s.detailsCalls[0].answer.resolve(shown("old@example.org"));
        await settle();

        expect(s.rooms.state.card?.rock).toEqual({ kind: "loading" });

        s.detailsCalls[1].answer.resolve({ isShown: false });
        await settle();
        s.rooms.closeCard();
        s.rooms.openCard(anna, team);
        expect(s.detailsCalls).toHaveLength(3);
    });

    test("after the person's own switch is saved, their card reads them again from the platform, so their badges show", async () => {
        const s = setup();
        s.rooms.openCard(anna, team);
        s.detailsCalls[0].answer.resolve({ isShown: false });
        s.personCalls[0].answer.resolve({ ok: true, person: profile(anna, "Anna", { is_self: true, details: null }) });
        await settle();

        const saved = deferred<void>();
        s.rooms.refreshCardDetails(saved.promise);
        await settle();
        expect(s.personCalls).toHaveLength(1);

        saved.resolve();
        await settle();
        expect(s.personCalls.map(c => [c.personAliasGuid, c.channelId])).toEqual([[anna, team], [anna, team]]);

        s.personCalls[1].answer.resolve({ ok: true, person: profile(anna, "Anna", { is_self: true, details: { badge_keys: ["serving"] } }) });
        await settle();
        expect(s.rooms.state.card?.person?.details).toEqual({ badge_keys: ["serving"] });
    });

    test("only the newest save's platform read is drawn, however the answers arrive", async () => {
        const s = setup();
        s.rooms.openCard(anna, team);
        s.detailsCalls[0].answer.resolve({ isShown: false });
        s.personCalls[0].answer.resolve({ ok: true, person: profile(anna, "Anna", { is_self: true, details: null }) });
        await settle();

        s.rooms.refreshCardDetails(Promise.resolve());
        await settle();
        s.rooms.refreshCardDetails(Promise.resolve());
        await settle();
        expect(s.personCalls).toHaveLength(3);

        s.personCalls[2].answer.resolve({ ok: true, person: profile(anna, "Anna", { is_self: true, details: null }) });
        await settle();
        s.personCalls[1].answer.resolve({ ok: true, person: profile(anna, "Anna", { is_self: true, details: { badge_keys: ["serving"] } }) });
        await settle();

        expect(s.rooms.state.card?.person?.details).toBeNull();
    });

    test("a failed read after the save keeps the person the card already shows", async () => {
        const s = setup();
        s.rooms.openCard(anna, team);
        s.detailsCalls[0].answer.resolve({ isShown: false });
        s.personCalls[0].answer.resolve({ ok: true, person: profile(anna, "Anna", { is_self: true, details: null }) });
        await settle();

        s.rooms.refreshCardDetails(Promise.resolve());
        await settle();
        s.personCalls[1].answer.resolve({ ok: false, error: { code: "rpc.transport", severity: "failed" } });
        await settle();

        expect(s.rooms.state.card?.person?.nick_name).toBe("Anna");
    });

    test("without Rock's door the card draws at once with nothing from Rock", async () => {
        const s = setup({ hasDetailsDoor: false });
        s.rooms.openCard(anna, team);
        s.personCalls[0].answer.resolve({ ok: true, person: profile(anna, "Anna") });
        await settle();

        expect(s.rooms.state.card?.person?.nick_name).toBe("Anna");
        expect(s.rooms.state.card?.rock).toEqual({ kind: "failed" });
    });

    test("once stopped Rock is not asked again and its answer never lands", async () => {
        const s = setup();
        s.rooms.openCard(anna, team);
        s.rooms.stop();
        s.detailsCalls[0].answer.resolve(shown("anna@example.org"));
        jest.advanceTimersByTime(profileDetailsWait);
        await settle();

        expect(s.rooms.state.card?.rock).toEqual({ kind: "loading" });

        s.rooms.openCard(ben, team);
        s.rooms.refreshCardDetails(Promise.resolve());
        await settle();
        expect(s.detailsCalls).toHaveLength(1);
    });
});

describe("the doors and the card once stopped", () => {
    test("nothing still on its way reaches the screen and nothing more is asked", async () => {
        const s = setup();
        sidebarOfThree(s);
        s.active.value = volunteers;
        void s.rooms.leave(volunteers);
        void s.rooms.rename(group, "Trail crew");
        s.rooms.openCard("a0000002-0000-4000-8000-000000000000", team);

        s.rooms.stop();
        s.leaveCalls[0].answer.resolve(door("ok"));
        s.renameCalls[0].answer.resolve(door("ok"));
        s.personCalls[0].answer.resolve({
            ok: true,
            person: { person_alias_guid: "a0000002-0000-4000-8000-000000000000", nick_name: "Anna", last_name: "Reyes", avatar_url: null, campus_id: null, is_self: false, is_leader: false, details: null, can_message: true, dm_channel_id: null }
        });
        await settle();

        expect(s.toasts).toEqual([]);
        expect(s.opened).toEqual([]);
        expect(s.rooms.leaveOf(volunteers)).toEqual({ kind: "leaving" });
        expect(s.rooms.renameOf(group)).toEqual({ kind: "saving" });
        expect(s.rooms.state.card?.person ?? null).toBeNull();

        await s.rooms.leave(team);
        await s.rooms.rename(group, "Hike crew");
        s.rooms.openCard("a0000003-0000-4000-8000-000000000000", team);
        expect(s.leaveCalls).toHaveLength(1);
        expect(s.renameCalls).toHaveLength(1);
        expect(s.personCalls).toHaveLength(1);
    });
});
