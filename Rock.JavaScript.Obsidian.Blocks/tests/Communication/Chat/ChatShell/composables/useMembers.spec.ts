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
// The members panel's roster: who is in a room, a page at a time, leaders first as the platform
// orders them, and a search of the whole room.
//
// The roster is fetched on open, on Load more, on Try again and when the room changes elsewhere,
// never from a live event's payload. A page shorter than the size asked for is the end. A room the
// person has not joined, or one that keeps its member list from them, is refused by the platform
// with its own code, which the panel draws as a state of its own rather than a failure. A query
// waits for the people search's pause, then asks the platform for the whole room; until it answers,
// the rows already loaded that match show at once, by the platform's own rule (a nick name, a last
// name or the whole name starting with the text, in any letter case). An answer older than the
// newest query is dropped. Once stopped, nothing more is fetched and no answer still on its way
// reaches the screen.
//
// The composable these tests are written to, in useMembers.partial.ts:
//
//   createMembers(dependencies: { call: (name, args) => Promise<CallResult> }): Members
//
//   Members = {
//       state: {
//           channelId: string | null, rows: RosterMember[], hasMore, isLoading, isLoadingMore,
//           error: ChatError | null, moreError: ChatError | null,
//           refusal: "not_member" | "no_roster_right" | null,
//           query: string,
//           search: { query: string | null, rows: RosterMember[], hasMore, isSearching,
//                     error: ChatError | null }
//       },
//       open(channelId): Promise<void>, loadMore(): Promise<void>, retry(): Promise<void>,
//       setQuery(query: string): void, retrySearch(): Promise<void>, close(): void, stop(): void
//   }
//
//   shownMembers(state): RosterMember[]      what the panel lists for the query typed
//   searchStatus(state): "searching" | "answered" | "failed" | null
//   memberSearchPause: 250; the page and search sizes are the platform's limits
import { createMembers, MemberRights, memberSearchPause, searchStatus, shownMembers, successorChoices } from "../../../../../src/Communication/Chat/ChatShell/composables/useMembers.partial";
import { CallResult } from "../../../../../src/Communication/Chat/ChatShell/platformCall.partial";
import { PersonRow } from "../../../../../src/Communication/Chat/ChatShell/types.partial";

// #region Fixtures

const channel = "c0000001-0000-4000-8000-000000000000";
const otherChannel = "c0000002-0000-4000-8000-000000000000";

/** A person's guid from a number, so a page can be made of many. */
function guid(n: number): string {
    return `a${String(n).padStart(7, "0")}-0000-4000-8000-000000000000`;
}

/** One row of chat_list_members, exactly its keys. */
function member(n: number, nickName: string, lastName: string, extra: Record<string, unknown> = {}): Record<string, unknown> {
    return {
        person_alias_guid: guid(n),
        nick_name: nickName,
        last_name: lastName,
        avatar_url: null,
        is_leader: false,
        details: null,
        can_message: true,
        ...extra
    };
}

const tom = member(1, "Tom", "Hale", { is_leader: true });
const anna = member(2, "Anna", "Reyes");
const ben = member(3, "Ben", "Okafor");
const annie = member(4, "Annie", "Lopez");

/** chat_list_members' answer. */
function page(members: Array<Record<string, unknown>>): CallResult {
    return { ok: true, data: { members } };
}

/** A full page of people, so another follows. */
function fullPage(from: number): Array<Record<string, unknown>> {
    return Array.from({ length: memberPageSize }, (_, i) => member(from + i, `Person${from + i}`, "Zed"));
}

/** A call that never reached the platform. */
const offline: CallResult = { ok: false, status: 0, error: { code: "rpc.transport", severity: "failed" }, retryAfterSeconds: null };

/** The platform's refusals, as the call path classifies a raised 42501. */
function refused(code: string, text: string): CallResult {
    return { ok: false, status: 403, error: { code, severity: "permission", text }, retryAfterSeconds: null };
}

const notMember = refused("authz.not_member", "the caller is not a member of this channel");
const noRosterRight = refused("authz.no_roster_right", "only a leader may see who is in this channel");

type Deferred<T> = { promise: Promise<T>, resolve: (value: T) => void };

function deferred<T>(): Deferred<T> {
    let resolve!: (value: T) => void;
    const promise = new Promise<T>(r => resolve = r);
    return { promise, resolve };
}

type Call = { name: string, args: Record<string, unknown> };

/**
 * A page of the roster and the people search's answer, as the platform's limits set them; sizes
 * no client would choose for itself, so only a size read from the limits is asked.
 */
const memberPageSize = 40;
const peopleSearchSize = 7;
const limits = (): { member_page_size: number, people_search_size: number } => ({ member_page_size: memberPageSize, people_search_size: peopleSearchSize });

/** The roster with a call path the test answers in order, each answer given or held. */
function harness(...answers: Array<CallResult | Deferred<CallResult>>): { members: ReturnType<typeof createMembers>, calls: Call[], answers: Array<CallResult | Deferred<CallResult>> } {
    const calls: Call[] = [];
    const queue = [...answers];
    const members = createMembers({
        call: (name, args) => {
            calls.push({ name, args });
            const next = queue.shift();
            if (!next) {
                return Promise.resolve(page([]));
            }
            return "promise" in next ? next.promise : Promise.resolve(next);
        },
        limits
    });
    return { members, calls, answers: queue };
}

/** Lets every answer already given reach the state. */
async function settle(): Promise<void> {
    for (let i = 0; i < 5; i++) {
        await Promise.resolve();
    }
}

const names = (rows: Array<{ nick_name: string }>): string[] => rows.map(r => r.nick_name);

// #endregion Fixtures

describe("the roster's pages", () => {
    test("opening asks for the first page of the room with no query, and a short page is the end", async () => {
        const { members, calls } = harness(page([tom, anna, ben]));

        const opened = members.open(channel);
        expect(members.state.isLoading).toBe(true);
        await opened;

        expect(calls).toEqual([{ name: "chat_list_members", args: { p_channel_id: channel, p_after: null, p_limit: memberPageSize, p_query: null } }]);
        expect(names(members.state.rows)).toEqual(["Tom", "Anna", "Ben"]);
        expect(members.state.isLoading).toBe(false);
        expect(members.state.hasMore).toBe(false);
        expect(members.state.channelId).toBe(channel);
    });

    test("a full page leaves more to load, and Load more asks after the last person given", async () => {
        const first = fullPage(10);
        const { members, calls } = harness(page(first), page([anna]));

        await members.open(channel);
        expect(members.state.hasMore).toBe(true);

        const more = members.loadMore();
        expect(members.state.isLoadingMore).toBe(true);
        await more;

        expect(calls[1].args).toEqual({ p_channel_id: channel, p_after: first[first.length - 1].person_alias_guid, p_limit: memberPageSize, p_query: null });
        expect(members.state.rows).toHaveLength(memberPageSize + 1);
        expect(members.state.isLoadingMore).toBe(false);
        expect(members.state.hasMore).toBe(false);
    });

    test("Load more does nothing at the end or while a page is already on its way", async () => {
        const held = deferred<CallResult>();
        const { members, calls } = harness(page(fullPage(10)), held);

        await members.open(channel);
        void members.loadMore();
        void members.loadMore();
        expect(calls).toHaveLength(2);

        held.resolve(page([]));
        await settle();
        void members.loadMore();
        expect(calls).toHaveLength(2);
    });

    test("a room the person has not joined is its own state, not a failure", async () => {
        const { members } = harness(notMember);

        await members.open(channel);

        expect(members.state.refusal).toBe("not_member");
        expect(members.state.error).toBeNull();
        expect(members.state.rows).toEqual([]);
        expect(members.state.isLoading).toBe(false);
    });

    test("a room that keeps its member list from the person is its own state", async () => {
        const { members } = harness(noRosterRight);

        await members.open(channel);

        expect(members.state.refusal).toBe("no_roster_right");
        expect(members.state.error).toBeNull();
    });

    test("a first page that fails says so, and Try again asks again", async () => {
        const { members, calls } = harness(offline, page([anna]));

        await members.open(channel);
        expect(members.state.error?.code).toBe("rpc.transport");
        expect(members.state.refusal).toBeNull();

        await members.retry();
        expect(calls).toHaveLength(2);
        expect(members.state.error).toBeNull();
        expect(names(members.state.rows)).toEqual(["Anna"]);
    });

    test("a later page that fails keeps the rows, and Load more tries again", async () => {
        const { members, calls } = harness(page(fullPage(10)), offline, page([anna]));

        await members.open(channel);
        await members.loadMore();
        expect(members.state.moreError?.code).toBe("rpc.transport");
        expect(members.state.rows).toHaveLength(memberPageSize);
        expect(members.state.hasMore).toBe(true);

        await members.loadMore();
        expect(calls).toHaveLength(3);
        expect(members.state.moreError).toBeNull();
        expect(members.state.rows).toHaveLength(memberPageSize + 1);
    });

    test("opening another room drops a page still on its way for the first", async () => {
        const held = deferred<CallResult>();
        const { members } = harness(held, page([ben]));

        void members.open(channel);
        await members.open(otherChannel);
        held.resolve(page([tom, anna]));
        await settle();

        expect(members.state.channelId).toBe(otherChannel);
        expect(names(members.state.rows)).toEqual(["Ben"]);
    });
});

describe("the search", () => {
    beforeEach(() => {
        jest.useFakeTimers();
    });

    afterEach(() => {
        jest.useRealTimers();
    });

    test("a query waits for the pause, then asks the whole room for it, trimmed", async () => {
        const { members, calls } = harness(page([tom, anna, ben]), page([anna, annie]));
        await members.open(channel);

        members.setQuery(" ann ");
        jest.advanceTimersByTime(memberSearchPause - 1);
        expect(calls).toHaveLength(1);
        expect(searchStatus(members.state)).toBe("searching");

        jest.advanceTimersByTime(1);
        expect(calls[1]).toEqual({ name: "chat_list_members", args: { p_channel_id: channel, p_after: null, p_limit: memberPageSize, p_query: "ann" } });

        await settle();
        expect(searchStatus(members.state)).toBe("answered");
        expect(names(shownMembers(members.state))).toEqual(["Anna", "Annie"]);
    });

    test("until the platform answers, the loaded rows that match show at once, by nick, last or whole name", async () => {
        const { members } = harness(page([tom, anna, ben, annie]));
        await members.open(channel);

        members.setQuery("an");
        expect(names(shownMembers(members.state))).toEqual(["Anna", "Annie"]);

        members.setQuery("LOP");
        expect(names(shownMembers(members.state))).toEqual(["Annie"]);

        members.setQuery("tom h");
        expect(names(shownMembers(members.state))).toEqual(["Tom"]);

        members.setQuery("om");
        expect(shownMembers(members.state)).toEqual([]);
    });

    test("an answer to an older query is dropped when it arrives after the newest", async () => {
        const forA = deferred<CallResult>();
        const forAn = deferred<CallResult>();
        const { members } = harness(page([tom, anna, ben]), forA, forAn);
        await members.open(channel);

        members.setQuery("a");
        jest.advanceTimersByTime(memberSearchPause);
        members.setQuery("an");
        jest.advanceTimersByTime(memberSearchPause);

        forAn.resolve(page([anna]));
        await settle();
        forA.resolve(page([anna, annie, member(9, "Al", "Ames")]));
        await settle();

        expect(members.state.search.query).toBe("an");
        expect(names(shownMembers(members.state))).toEqual(["Anna"]);
        expect(searchStatus(members.state)).toBe("answered");
    });

    test("typing again within the pause asks once, for the last query", async () => {
        const { members, calls } = harness(page([tom]), page([anna]));
        await members.open(channel);

        members.setQuery("a");
        jest.advanceTimersByTime(100);
        members.setQuery("an");
        jest.advanceTimersByTime(memberSearchPause);

        expect(calls.slice(1).map(c => c.args.p_query)).toEqual(["an"]);
    });

    test("clearing the query shows the loaded roster and drops a search still on its way", async () => {
        const held = deferred<CallResult>();
        const { members } = harness(page([tom, anna, ben]), held);
        await members.open(channel);

        members.setQuery("an");
        jest.advanceTimersByTime(memberSearchPause);
        members.setQuery("");
        held.resolve(page([anna]));
        await settle();

        expect(searchStatus(members.state)).toBeNull();
        expect(names(shownMembers(members.state))).toEqual(["Tom", "Anna", "Ben"]);
    });

    test("a query of only spaces is the plain roster and asks nothing", async () => {
        const { members, calls } = harness(page([tom, anna]));
        await members.open(channel);

        members.setQuery("   ");
        jest.advanceTimersByTime(memberSearchPause);

        expect(calls).toHaveLength(1);
        expect(searchStatus(members.state)).toBeNull();
        expect(names(shownMembers(members.state))).toEqual(["Tom", "Anna"]);
    });

    test("a search that fails says so, and Try again asks again at once", async () => {
        const { members, calls } = harness(page([tom, anna]), offline, page([anna]));
        await members.open(channel);

        members.setQuery("an");
        jest.advanceTimersByTime(memberSearchPause);
        await settle();
        expect(searchStatus(members.state)).toBe("failed");
        expect(names(shownMembers(members.state))).toEqual(["Anna"]);

        const again = members.retrySearch();
        expect(calls).toHaveLength(3);
        await again;
        expect(searchStatus(members.state)).toBe("answered");
    });

    test("Load more under a query follows the search's own keyset", async () => {
        const matches = fullPage(10);
        const { members, calls } = harness(page([tom]), page(matches), page([anna]));
        await members.open(channel);

        members.setQuery("person");
        jest.advanceTimersByTime(memberSearchPause);
        await settle();
        expect(members.state.search.hasMore).toBe(true);

        await members.loadMore();
        expect(calls[2].args).toEqual({ p_channel_id: channel, p_after: matches[matches.length - 1].person_alias_guid, p_limit: memberPageSize, p_query: "person" });
        expect(shownMembers(members.state)).toHaveLength(memberPageSize + 1);
        expect(members.state.search.hasMore).toBe(false);
        expect(members.state.rows).toHaveLength(1);
    });
});

describe("stopping", () => {
    test("nothing resolves into the state after stop", async () => {
        const held = deferred<CallResult>();
        const { members } = harness(held);

        void members.open(channel);
        members.stop();
        held.resolve(page([tom, anna]));
        await settle();

        expect(members.state.rows).toEqual([]);
    });

    test("no search is asked after stop, and nothing is fetched", async () => {
        jest.useFakeTimers();
        try {
            const { members, calls } = harness(page([tom]));
            await members.open(channel);

            members.setQuery("an");
            members.stop();
            jest.advanceTimersByTime(memberSearchPause);
            await members.loadMore();
            await members.open(otherChannel);

            expect(calls).toHaveLength(1);
        }
        finally {
            jest.useRealTimers();
        }
    });

    test("closing drops what is on its way and empties the roster", async () => {
        const held = deferred<CallResult>();
        const { members } = harness(held);

        void members.open(channel);
        members.close();
        held.resolve(page([tom]));
        await settle();

        expect(members.state.rows).toEqual([]);
        expect(members.state.channelId).toBeNull();
        expect(members.state.isLoading).toBe(false);
    });
});

// #region Changing who is in a room

// The panel also changes who is in the room through Rock's doors. Opening it asks Rock, beside the
// roster, whether the reader may add and remove people there, and Add people and Remove are offered
// only where they may. A remove is confirmed first; the row is busy while the door works; on
// success the row leaves and a toast names the person, and a leader removed in favour of someone
// named to lead next makes that person the room's leader on the roster. A refusal stays under the
// row in the door's own words, and a row whose person had already gone leaves once its refusal is
// dismissed. A door not reached is told as offline or as failed, as a Leave is. Adding searches
// the people the room may take, marking who is in it already, and adds up to eight at a time.
//
//   createMembers({ call, doors?: { getRights, addMembers, removeMember }, isOffline?, notify?,
//                   titleOf?, isChatAdministrator?, campusOnly? })
//   state += canManageMembers, isChatAdministrator, removals: { [guid]: { kind: "removing" } |
//            { kind: "refused", text, isStale } }, confirming: RosterMember | null,
//            successorQuery, successor: MemberSearchState, successorRefusal: string | null, isSuccessorAsked,
//            adding: { isOpen, query, people: PersonRow[] | null, isSearching, error, isAdding,
//                      refusal: { text, personAliasGuid } | null }
//   askRemove(guid), cancelRemove(), remove(replacementGuid?), dismissRemoval(guid),
//   setSuccessorQuery(query), openAdd(), closeAdd(), setAddQuery(query), add(people)
//   successorChoices(state); state.lastLeaderPersonAliasGuid is Rock's answer on open, and
//   state.isSuccessorAsked says the confirm asks who leads next

type Answer = { code?: string | null, message?: string | null, isPending: boolean, personAliasGuid?: string | null };

type DoorCall = { door: string, args: unknown[] };

/** Rock's answer, its other rights none unless a test gives them. */
type Rights = Partial<MemberRights> & { canManageMembers: boolean };

/** The roster with Rock's doors, each answering what the test hands it, in order, given or held. */
function doorHarness(options: {
    calls?: Array<CallResult | Deferred<CallResult>>,
    rights?: Array<Rights | Deferred<Rights>>,
    answers?: Array<Answer | Deferred<Answer> | Error>,
    isOffline?: boolean,
    isChatAdministrator?: boolean
}): { members: ReturnType<typeof createMembers>, calls: Call[], doorCalls: DoorCall[], notices: Array<{ code: string, text: string, icon: string }>, offline: { value: boolean } } {
    const calls: Call[] = [];
    const doorCalls: DoorCall[] = [];
    const notices: Array<{ code: string, text: string, icon: string }> = [];
    const callQueue = [...(options.calls ?? [])];
    const rightsQueue = [...(options.rights ?? [])];
    const answerQueue = [...(options.answers ?? [])];
    const offline = { value: options.isOffline ?? false };

    const answerNext = (): Promise<Answer> => {
        const next = answerQueue.shift() ?? { code: "ok", message: null, isPending: false };
        if (next instanceof Error) {
            return Promise.reject(next);
        }
        return "promise" in next ? next.promise : Promise.resolve(next);
    };

    const members = createMembers({
        call: (name, args) => {
            calls.push({ name, args });
            const next = callQueue.shift() ?? page([]);
            return "promise" in next ? next.promise : Promise.resolve(next);
        },
        doors: {
            getRights: channelId => {
                doorCalls.push({ door: "getRights", args: [channelId] });
                const next = rightsQueue.shift() ?? { canManageMembers: false };
                const answer = "promise" in next ? next.promise : Promise.resolve(next);
                return answer.then(rights => ({ canRename: false, lastLeaderPersonAliasGuid: null, ...rights }));
            },
            addMembers: (channelId, personAliasGuids) => {
                doorCalls.push({ door: "addMembers", args: [channelId, personAliasGuids] });
                return answerNext();
            },
            removeMember: (channelId, personAliasGuid, replacementPersonAliasGuid) => {
                doorCalls.push({ door: "removeMember", args: [channelId, personAliasGuid, replacementPersonAliasGuid] });
                return answerNext();
            }
        },
        isOffline: () => offline.value,
        notify: notice => notices.push(notice),
        titleOf: id => id === channel ? "young-adults" : "Hike planning",
        isChatAdministrator: options.isChatAdministrator ?? false,
        campusOnly: false,
        limits
    });

    return { members, calls, doorCalls, notices, offline };
}

/** The door's answers, as ChatMembershipResultBag carries them. */
const ok: Answer = { code: "ok", message: null, isPending: false };
const pendingOk: Answer = { code: "ok", message: null, isPending: true };
const unreached: Answer = { code: "door.unavailable", message: null, isPending: false };
const notMemberAnswer: Answer = { code: "door.not_member", message: "That person isn't in this conversation.", isPending: false };
const notAllowed: Answer = { code: "door.not_allowed", message: "Only Chat Administrators can remove a leader.", isPending: false };

/** One person as chat_search_people answers them for a room, exactly its keys. */
function found(n: number, nickName: string, lastName: string, inRoom: boolean): PersonRow {
    return { person_alias_guid: guid(n), nick_name: nickName, last_name: lastName, avatar_url: null, campus_id: null, in_room: inRoom } as PersonRow;
}

const aaron = found(20, "Aaron", "Park", false);
const luis = found(21, "Luis", "Romero", false);
const annaFound = found(2, "Anna", "Reyes", true);

describe("the reader's rights in the room", () => {
    test("opening asks Rock beside the roster, and draws Add and Remove only where it says so", async () => {
        const { members, calls, doorCalls } = doorHarness({ calls: [page([tom, anna])], rights: [{ canManageMembers: true }] });

        const opened = members.open(channel);
        expect(members.state.canManageMembers).toBe(false);
        expect(calls).toHaveLength(1);
        expect(doorCalls).toEqual([{ door: "getRights", args: [channel] }]);
        await opened;
        await settle();

        expect(members.state.canManageMembers).toBe(true);
    });

    test("following to another room asks again, and nothing carries over from the first", async () => {
        const { members, doorCalls } = doorHarness({ calls: [page([tom]), page([ben])], rights: [{ canManageMembers: true }, { canManageMembers: false }] });

        await members.open(channel);
        await settle();
        expect(members.state.canManageMembers).toBe(true);

        const opened = members.open(otherChannel);
        expect(members.state.canManageMembers).toBe(false);
        await opened;
        await settle();

        expect(doorCalls.map(c => c.args[0])).toEqual([channel, otherChannel]);
        expect(members.state.canManageMembers).toBe(false);
    });

    test("an answer for a room since left is dropped", async () => {
        const first = deferred<Rights>();
        const { members } = doorHarness({ calls: [page([tom]), page([ben])], rights: [first, { canManageMembers: false }] });

        void members.open(channel);
        await members.open(otherChannel);
        first.resolve({ canManageMembers: true });
        await settle();

        expect(members.state.canManageMembers).toBe(false);
    });

    test("nothing resolves into the state after stop", async () => {
        const held = deferred<Rights>();
        const { members } = doorHarness({ calls: [page([tom])], rights: [held] });

        void members.open(channel);
        members.stop();
        held.resolve({ canManageMembers: true });
        await settle();

        expect(members.state.canManageMembers).toBe(false);
    });

    test("while nothing is held for the room, the open says its rights are still being read", async () => {
        const held = deferred<Rights>();
        const { members } = doorHarness({ calls: [page([tom])], rights: [held] });

        const opened = members.open(channel);
        expect(members.state.isReadingRights).toBe(true);
        held.resolve({ canManageMembers: true });
        await opened;
        await settle();

        expect(members.state.isReadingRights).toBe(false);
        expect(members.state.canManageMembers).toBe(true);
    });

    test("a failed read is a no, and no longer being read", async () => {
        const failing = createMembers({
            call: () => Promise.resolve(page([tom])),
            doors: { getRights: () => Promise.reject(new Error("unreached")) },
            limits
        });

        await failing.open(channel);
        await settle();

        expect(failing.state.isReadingRights).toBe(false);
        expect(failing.state.canManageMembers).toBe(false);
    });

    test("rights read once are kept for the room, so a reopen draws them on its first frame and asks nothing", async () => {
        const { members, doorCalls } = doorHarness({ calls: [page([tom]), page([tom])], rights: [{ canManageMembers: true, lastLeaderPersonAliasGuid: guid(1) }] });
        await members.open(channel);
        await settle();
        members.close();

        void members.open(channel);
        expect(members.state.canManageMembers).toBe(true);
        expect(members.state.lastLeaderPersonAliasGuid).toBe(guid(1));
        expect(members.state.isReadingRights).toBe(false);
        await settle();

        expect(doorCalls).toEqual([{ door: "getRights", args: [channel] }]);
    });

    test("reading ahead, as the person reaches for the panel, has the rights there when it opens", async () => {
        const { members, doorCalls } = doorHarness({ calls: [page([tom])], rights: [{ canManageMembers: true }] });

        members.readAhead(channel);
        members.readAhead(channel);
        await settle();
        expect(members.state.channelId).toBeNull();

        void members.open(channel);
        expect(members.state.canManageMembers).toBe(true);
        expect(members.state.isReadingRights).toBe(false);
        await settle();

        expect(doorCalls).toHaveLength(1);
    });

    test("an open while a read ahead is still on its way waits for that answer rather than asking again", async () => {
        const held = deferred<Rights>();
        const { members, doorCalls } = doorHarness({ calls: [page([tom])], rights: [held] });

        members.readAhead(channel);
        const opened = members.open(channel);
        expect(members.state.isReadingRights).toBe(true);
        held.resolve({ canManageMembers: true });
        await opened;
        await settle();

        expect(doorCalls).toHaveLength(1);
        expect(members.state.canManageMembers).toBe(true);
    });

    test("a room changed elsewhere drops what is held for it, so its next open asks again", async () => {
        const { members, doorCalls } = doorHarness({ calls: [page([tom]), page([tom])], rights: [{ canManageMembers: true }, { canManageMembers: false }] });
        await members.open(channel);
        await settle();
        members.close();

        members.roomChanged(channel);
        const opened = members.open(channel);
        expect(members.state.isReadingRights).toBe(true);
        expect(members.state.canManageMembers).toBe(false);
        await opened;
        await settle();

        expect(doorCalls.map(c => c.args[0])).toEqual([channel, channel]);
        expect(members.state.canManageMembers).toBe(false);
    });

    test("an answer asked before the room changed is not kept", async () => {
        const first = deferred<Rights>();
        const { members, doorCalls } = doorHarness({ calls: [page([tom])], rights: [first, { canManageMembers: false }] });

        members.readAhead(channel);
        members.roomChanged(channel);
        first.resolve({ canManageMembers: true });
        await settle();

        await members.open(channel);
        await settle();

        expect(doorCalls).toHaveLength(2);
        expect(members.state.canManageMembers).toBe(false);
    });

    test("a page with no doors offers nothing, and whether the reader is a Chat Administrator comes from Rock's page load", async () => {
        const bare = createMembers({ call: () => Promise.resolve(page([tom])), limits });
        await bare.open(channel);
        expect(bare.state.canManageMembers).toBe(false);
        expect(bare.state.isChatAdministrator).toBe(false);

        const { members } = doorHarness({ isChatAdministrator: true });
        expect(members.state.isChatAdministrator).toBe(true);
    });
});

describe("removing someone", () => {
    test("the confirm names the person, and confirming asks the door with no one to lead next", async () => {
        const held = deferred<Answer>();
        const { members, doorCalls } = doorHarness({ calls: [page([tom, anna, ben])], answers: [held] });
        await members.open(channel);

        members.askRemove(guid(2));
        expect(members.state.confirming?.nick_name).toBe("Anna");

        const removing = members.remove();
        expect(members.state.confirming).toBeNull();
        expect(members.state.removals[guid(2)]).toEqual({ kind: "removing" });
        expect(doorCalls.filter(c => c.door === "removeMember")).toEqual([{ door: "removeMember", args: [channel, guid(2), null] }]);

        held.resolve(ok);
        await removing;
        expect(names(members.state.rows)).toEqual(["Tom", "Ben"]);
        expect(members.state.removals[guid(2)]).toBeUndefined();
    });

    test("a removal the platform has yet to take is done too: the row leaves and the toast names the person", async () => {
        const { members, notices } = doorHarness({ calls: [page([tom, anna])], answers: [pendingOk] });
        await members.open(channel);

        members.askRemove(guid(2));
        await members.remove();

        expect(names(members.state.rows)).toEqual(["Tom"]);
        expect(notices).toEqual([{ code: "member.removed", text: "Removed Anna Reyes", icon: "user-minus" }]);
    });

    test("a person removed under a query leaves the search's rows as well", async () => {
        jest.useFakeTimers();
        try {
            const { members } = doorHarness({ calls: [page([tom, anna]), page([anna, annie])] });
            await members.open(channel);
            members.setQuery("an");
            jest.advanceTimersByTime(memberSearchPause);
            await settle();

            members.askRemove(guid(4));
            await members.remove();

            expect(names(shownMembers(members.state))).toEqual(["Anna"]);
        }
        finally {
            jest.useRealTimers();
        }
    });

    test("cancelling the confirm asks nothing", async () => {
        const { members, doorCalls } = doorHarness({ calls: [page([tom, anna])] });
        await members.open(channel);

        members.askRemove(guid(2));
        members.cancelRemove();

        expect(members.state.confirming).toBeNull();
        expect(doorCalls.filter(c => c.door === "removeMember")).toEqual([]);
    });

    test("a refusal stays under the row in the door's words, and dismissing it keeps the row", async () => {
        const { members, notices } = doorHarness({ calls: [page([tom, anna])], answers: [notAllowed] });
        await members.open(channel);

        members.askRemove(guid(1));
        await members.remove();

        expect(members.state.removals[guid(1)]).toEqual({ kind: "refused", text: "Only Chat Administrators can remove a leader.", isStale: false });
        expect(names(members.state.rows)).toEqual(["Tom", "Anna"]);
        expect(notices).toEqual([]);

        members.dismissRemoval(guid(1));
        expect(members.state.removals[guid(1)]).toBeUndefined();
        expect(names(members.state.rows)).toEqual(["Tom", "Anna"]);
    });

    test("a person who had already gone is a stale row, which leaves once its refusal is dismissed", async () => {
        const { members } = doorHarness({ calls: [page([tom, anna])], answers: [notMemberAnswer] });
        await members.open(channel);

        members.askRemove(guid(2));
        await members.remove();
        expect(members.state.removals[guid(2)]).toEqual({ kind: "refused", text: "That person isn't in this conversation.", isStale: true });
        expect(names(members.state.rows)).toEqual(["Tom", "Anna"]);

        members.dismissRemoval(guid(2));
        expect(names(members.state.rows)).toEqual(["Tom"]);
    });

    test("offline, the door is not asked and the row says so", async () => {
        const { members, doorCalls } = doorHarness({ calls: [page([tom, anna])], isOffline: true });
        await members.open(channel);

        members.askRemove(guid(2));
        await members.remove();

        expect(doorCalls.filter(c => c.door === "removeMember")).toEqual([]);
        expect(members.state.removals[guid(2)]).toEqual({ kind: "refused", text: "Couldn't remove Anna. You're offline; try again when you're back.", isStale: false });
    });

    test("a door Rock answered without, or one that threw, is offline once the connection is lost and otherwise a failure", async () => {
        const { members, offline } = doorHarness({ calls: [page([tom, anna, ben])], answers: [unreached, new Error("lost")] });
        await members.open(channel);

        members.askRemove(guid(2));
        await members.remove();
        expect(members.state.removals[guid(2)]).toEqual({ kind: "refused", text: "Couldn't remove Anna. Try again in a moment.", isStale: false });

        members.askRemove(guid(3));
        const removing = members.remove();
        offline.value = true;
        await removing;
        expect(members.state.removals[guid(3)]).toEqual({ kind: "refused", text: "Couldn't remove Ben. You're offline; try again when you're back.", isStale: false });
    });

    test("an answer that comes after the panel stopped reaches nothing", async () => {
        const held = deferred<Answer>();
        const { members, notices } = doorHarness({ calls: [page([tom, anna])], answers: [held] });
        await members.open(channel);

        members.askRemove(guid(2));
        const removing = members.remove();
        members.stop();
        held.resolve(ok);
        await removing;

        expect(notices).toEqual([]);
        expect(names(members.state.rows)).toEqual(["Tom", "Anna"]);
    });
});

describe("removing a leader", () => {
    const grace = member(5, "Grace", "Kim", { is_leader: true });

    test("the last leader is the one Rock names on open, whatever leaders the loaded roster shows", async () => {
        // Grace leads too by the roster, but Rock leaves her out (on the Chat Ban List, say).
        const { members } = doorHarness({ calls: [page([tom, grace, anna])], rights: [{ canManageMembers: true, lastLeaderPersonAliasGuid: guid(1).toUpperCase() }], isChatAdministrator: true });
        await members.open(channel);
        await settle();

        members.askRemove(guid(1));
        expect(members.state.isSuccessorAsked).toBe(true);
        members.askRemove(guid(5));
        expect(members.state.isSuccessorAsked).toBe(false);
        members.askRemove(guid(2));
        expect(members.state.isSuccessorAsked).toBe(false);

        // The roster's only leader is not the last one when Rock names nobody.
        const alone = doorHarness({ calls: [page([tom, anna])], rights: [{ canManageMembers: true, lastLeaderPersonAliasGuid: null }] });
        await alone.members.open(channel);
        await settle();
        alone.members.askRemove(guid(1));
        expect(alone.members.state.isSuccessorAsked).toBe(false);
    });

    test("the last leader's dialog lists the room's other members, and its search asks the roster for the whole room", async () => {
        jest.useFakeTimers();
        try {
            const { members, calls } = doorHarness({ calls: [page([tom, anna, ben]), page([tom, annie])] });
            await members.open(channel);
            members.askRemove(guid(1));
            expect(names(successorChoices(members.state))).toEqual(["Anna", "Ben"]);

            members.setSuccessorQuery("an");
            expect(names(successorChoices(members.state))).toEqual(["Anna"]);
            jest.advanceTimersByTime(memberSearchPause);
            expect(calls[1]).toEqual({ name: "chat_list_members", args: { p_channel_id: channel, p_after: null, p_limit: memberPageSize, p_query: "an" } });

            await settle();
            expect(names(successorChoices(members.state))).toEqual(["Annie"]);
        }
        finally {
            jest.useRealTimers();
        }
    });

    test("removing with someone to lead next sends them, keeps the dialog while the door works, then makes them leader and says so", async () => {
        const held = deferred<Answer>();
        const { members, doorCalls, notices } = doorHarness({ calls: [page([tom, anna, ben]), page([{ ...anna, is_leader: true }, ben])], answers: [held], isChatAdministrator: true });
        await members.open(channel);
        members.askRemove(guid(1));

        const removing = members.remove(guid(2));
        expect(members.state.confirming?.nick_name).toBe("Tom");
        expect(members.state.removals[guid(1)]).toEqual({ kind: "removing" });
        expect(doorCalls.filter(c => c.door === "removeMember")).toEqual([{ door: "removeMember", args: [channel, guid(1), guid(2)] }]);

        held.resolve(ok);
        await removing;
        expect(members.state.confirming).toBeNull();
        expect(names(members.state.rows)).toEqual(["Anna", "Ben"]);
        expect(members.state.rows[0].is_leader).toBe(true);
        expect(notices).toEqual([{ code: "member.leader", text: "Anna Reyes is now the leader of young-adults", icon: "users" }]);
    });

    test("after a leader is removed for someone to lead next, the roster is read again, so its leaders are Rock's", async () => {
        const { members, calls } = doorHarness({ calls: [page([tom, anna, ben]), page([{ ...anna, is_leader: true }, { ...ben, is_leader: true }])], isChatAdministrator: true });
        await members.open(channel);
        members.askRemove(guid(1));

        await members.remove(guid(2));

        expect(calls.filter(c => c.name === "chat_list_members")).toHaveLength(2);
        expect(members.state.rows.filter(m => m.is_leader).map(m => m.nick_name)).toEqual(["Anna", "Ben"]);
    });

    test("a refusal shows in the dialog in the door's words, and the row stays as it was", async () => {
        const refused: Answer = { code: "door.bad_request", message: "Choose who leads this channel next before removing its last leader.", isPending: false };
        const { members } = doorHarness({ calls: [page([tom, anna])], answers: [refused] });
        await members.open(channel);
        members.askRemove(guid(1));

        await members.remove(guid(2));

        expect(members.state.confirming?.nick_name).toBe("Tom");
        expect(members.state.successorRefusal).toBe("Choose who leads this channel next before removing its last leader.");
        expect(members.state.removals[guid(1)]).toBeUndefined();

        members.cancelRemove();
        expect(members.state.successorRefusal).toBeNull();
    });

    test("a leader the roster showed beside another, whom Rock calls the last, gets the dialog that asks who leads next", async () => {
        const lastLeader: Answer = { code: "door.bad_request", message: "Choose who leads this channel next before removing its last leader.", isPending: false };
        const { members, doorCalls } = doorHarness({ calls: [page([tom, grace, anna]), page([grace, { ...anna, is_leader: true }])], answers: [lastLeader, ok], isChatAdministrator: true });
        await members.open(channel);
        members.askRemove(guid(1));
        expect(members.state.isSuccessorAsked).toBe(false);

        await members.remove();

        expect(members.state.confirming?.nick_name).toBe("Tom");
        expect(members.state.isSuccessorAsked).toBe(true);
        expect(members.state.removals[guid(1)]).toBeUndefined();
        expect(members.state.successorRefusal).toBeNull();

        await members.remove(guid(2));
        expect(doorCalls.filter(c => c.door === "removeMember").map(c => c.args[2])).toEqual([null, guid(2)]);
        expect(members.state.confirming).toBeNull();

        members.askRemove(guid(5));
        expect(members.state.isSuccessorAsked).toBe(false);
    });

    test("a refusal of a member who does not lead stays under their row", async () => {
        const refused: Answer = { code: "door.bad_request", message: "Choose who leads this channel next before removing its last leader.", isPending: false };
        const { members } = doorHarness({ calls: [page([tom, anna])], answers: [refused] });
        await members.open(channel);
        members.askRemove(guid(2));

        await members.remove();

        expect(members.state.confirming).toBeNull();
        expect(members.state.removals[guid(2)]).toEqual({ kind: "refused", text: "Choose who leads this channel next before removing its last leader.", isStale: false });
    });
});

describe("adding people", () => {
    beforeEach(() => {
        jest.useFakeTimers();
    });

    afterEach(() => {
        jest.useRealTimers();
    });

    test("a name waits for the pause, then asks the people search for this room, which marks who is in it", async () => {
        const { members, calls } = doorHarness({ calls: [page([tom, anna]), { ok: true, data: { people: [aaron, annaFound] } }] });
        await members.open(channel);
        members.openAdd();
        expect(members.state.adding.isOpen).toBe(true);

        members.setAddQuery(" a ");
        expect(members.state.adding.isSearching).toBe(true);
        jest.advanceTimersByTime(memberSearchPause);
        expect(calls[1]).toEqual({ name: "chat_search_people", args: { p_query: "a", p_limit: peopleSearchSize, p_campus_only: false, p_channel_id: channel } });

        await settle();
        expect(members.state.adding.people?.map(p => [p.nick_name, p.in_room])).toEqual([["Aaron", false], ["Anna", true]]);
        expect(members.state.adding.isSearching).toBe(false);
    });

    test("an answer to an older name is dropped, and clearing the name asks nothing", async () => {
        const forA = deferred<CallResult>();
        const { members, calls } = doorHarness({ calls: [page([tom]), forA, { ok: true, data: { people: [luis] } }] });
        await members.open(channel);
        members.openAdd();

        members.setAddQuery("a");
        jest.advanceTimersByTime(memberSearchPause);
        members.setAddQuery("lu");
        jest.advanceTimersByTime(memberSearchPause);
        await settle();
        forA.resolve({ ok: true, data: { people: [aaron] } });
        await settle();
        expect(members.state.adding.people?.map(p => p.nick_name)).toEqual(["Luis"]);

        members.setAddQuery("");
        jest.advanceTimersByTime(memberSearchPause);
        expect(calls).toHaveLength(3);
        expect(members.state.adding.people).toBeNull();
    });

    test("adding asks the door for everyone chosen, busy until it answers, then closes, says who was added and reads the roster again", async () => {
        const held = deferred<Answer>();
        const { members, calls, doorCalls, notices } = doorHarness({ calls: [page([tom]), page([tom, member(20, "Aaron", "Park")])], answers: [held] });
        await members.open(channel);
        members.openAdd();

        const adding = members.add([aaron]);
        expect(members.state.adding.isAdding).toBe(true);
        expect(doorCalls.filter(c => c.door === "addMembers")).toEqual([{ door: "addMembers", args: [channel, [guid(20)]] }]);

        held.resolve(ok);
        await adding;
        await settle();
        expect(members.state.adding.isOpen).toBe(false);
        expect(notices).toEqual([{ code: "member.added", text: "Added Aaron Park", icon: "user-plus" }]);
        expect(calls[1].name).toBe("chat_list_members");
        expect(names(members.state.rows)).toEqual(["Tom", "Aaron"]);
        expect(members.state.isLoading).toBe(false);
    });

    test("several people added are named by first name", async () => {
        const { members, notices } = doorHarness({ calls: [page([tom])], answers: [pendingOk] });
        await members.open(channel);
        members.openAdd();

        await members.add([aaron, luis, found(22, "Mia", "Chen", false)]);

        expect(notices[0].text).toBe("Added Aaron, Luis and Mia");
    });

    test("a refusal naming someone chosen asks for them to be taken out, and the dialog stays open", async () => {
        const refusal: Answer = { code: "door.target_not_eligible", message: "Luis Romero cannot be added.", isPending: false, personAliasGuid: guid(21) };
        const { members, notices } = doorHarness({ calls: [page([tom])], answers: [refusal] });
        await members.open(channel);
        members.openAdd();

        await members.add([aaron, luis]);

        expect(members.state.adding.isOpen).toBe(true);
        expect(members.state.adding.isAdding).toBe(false);
        expect(members.state.adding.refusal).toEqual({ text: "Luis Romero cannot be added. Remove Luis and try again.", personAliasGuid: guid(21) });
        expect(notices).toEqual([]);
    });

    test("any other refusal is the door's sentence, and a door not reached says offline or failed", async () => {
        const full: Answer = { code: "door.bad_request", message: "A conversation holds at most nine people.", isPending: false };
        const { members, offline } = doorHarness({ calls: [page([tom])], answers: [full, unreached] });
        await members.open(channel);
        members.openAdd();

        await members.add([aaron]);
        expect(members.state.adding.refusal).toEqual({ text: "A conversation holds at most nine people.", personAliasGuid: null });

        await members.add([aaron]);
        expect(members.state.adding.refusal?.text).toBe("Couldn't add people. Try again in a moment.");

        offline.value = true;
        await members.add([aaron]);
        expect(members.state.adding.refusal?.text).toBe("Couldn't add people. You're offline; try again when you're back.");
    });

    test("nothing resolves after stop", async () => {
        const held = deferred<Answer>();
        const { members, notices } = doorHarness({ calls: [page([tom])], answers: [held] });
        await members.open(channel);
        members.openAdd();

        const adding = members.add([aaron]);
        members.stop();
        held.resolve(ok);
        await adding;

        expect(notices).toEqual([]);
        expect(members.state.adding.isOpen).toBe(true);
    });
});

// #endregion Changing who is in a room

// #region A room changed elsewhere

// Someone else added or removed a person, or the room changed: the platform tells the person's
// topic, and the panel holding that room reads who is in it, and what the reader may do there,
// again in place, as it does after its own add and remove.
describe("a room changed elsewhere", () => {
    test("the panel holding the room reads its first page and the reader's rights again, with no loading state", async () => {
        const { members, calls, doorCalls } = doorHarness({ calls: [page([tom, anna]), page([tom, anna, ben])], rights: [{ canManageMembers: false }, { canManageMembers: true }] });
        await members.open(channel);
        await settle();

        members.roomChanged(channel);
        expect(members.state.isLoading).toBe(false);
        expect(names(members.state.rows)).toEqual(["Tom", "Anna"]);
        await settle();

        expect(calls.map(c => c.args)).toEqual([
            { p_channel_id: channel, p_after: null, p_limit: memberPageSize, p_query: null },
            { p_channel_id: channel, p_after: null, p_limit: memberPageSize, p_query: null }
        ]);
        expect(doorCalls.map(c => c.args[0])).toEqual([channel, channel]);
        expect(names(members.state.rows)).toEqual(["Tom", "Anna", "Ben"]);
        expect(members.state.canManageMembers).toBe(true);
    });

    test("another room, a closed panel or a stopped one asks nothing", async () => {
        const { members, calls, doorCalls } = doorHarness({ calls: [page([tom])] });

        members.roomChanged(channel);
        await members.open(channel);
        members.roomChanged(otherChannel);
        members.roomChanged(null);
        await settle();
        expect(calls).toHaveLength(1);

        members.stop();
        members.roomChanged(channel);
        await settle();

        expect(calls).toHaveLength(1);
        expect(doorCalls).toHaveLength(1);
    });

    test("an answer for a room since left is dropped", async () => {
        const held = deferred<CallResult>();
        const { members } = doorHarness({ calls: [page([tom]), held, page([ben])] });
        await members.open(channel);

        members.roomChanged(channel);
        await members.open(otherChannel);
        held.resolve(page([tom, anna]));
        await settle();

        expect(members.state.channelId).toBe(otherChannel);
        expect(names(members.state.rows)).toEqual(["Ben"]);
    });
});

// #endregion A room changed elsewhere
