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
// The roster is fetched on open, on Load more and on Try again, never live. A page shorter than
// the size asked for is the end. A room the person has not joined, or one that keeps its member
// list from them, is refused by the platform with its own code, which the panel draws as a state
// of its own rather than a failure. A query waits for the people search's pause, then asks the
// platform for the whole room; until it answers, the rows already loaded that match show at once,
// by the platform's own rule (a nick name, a last name or the whole name starting with the text,
// in any letter case). An answer older than the newest query is dropped. Once stopped, nothing
// more is fetched and no answer still on its way reaches the screen.
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
//   memberSearchPause: 250, memberPageSize: 50
import { createMembers, memberPageSize, memberSearchPause, searchStatus, shownMembers } from "../../../../../src/Communication/Chat/ChatShell/composables/useMembers.partial";
import { CallResult } from "../../../../../src/Communication/Chat/ChatShell/platformCall.partial";

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
        }
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
