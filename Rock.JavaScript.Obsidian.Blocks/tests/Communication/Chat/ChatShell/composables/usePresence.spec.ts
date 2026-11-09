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

// Typing: the person's own signals, sent at once on the first change and then at most once an
// interval while the text keeps changing, never in a channel and never while nothing could come of
// one; and the others' signals, kept for the open conversation's places until they expire, a
// message of theirs follows them, or the person blocks them, and given with their faces and words.
import { reactive } from "vue";
import { createPresence, Presence, PresenceDependencies, typingWords } from "../../../../../src/Communication/Chat/ChatShell/composables/usePresence.partial";
import { quoteKey } from "../../../../../src/Communication/Chat/ChatShell/composables/useMessageActions.partial";
import { readSettings } from "../../../../../src/Communication/Chat/ChatShell/composables/useSession.partial";
import { classifyPlatformError } from "../../../../../src/Communication/Chat/ChatShell/errors.partial";
import { CallResult } from "../../../../../src/Communication/Chat/ChatShell/platformCall.partial";
import { ChatError, ChatPerson, SidebarRow } from "../../../../../src/Communication/Chat/ChatShell/types.partial";

const me = "a0000007-0000-4000-8000-000000000000";
const anna = "a0000001-0000-4000-8000-000000000000";
const sam = "a0000002-0000-4000-8000-000000000000";
const lee = "a0000003-0000-4000-8000-000000000000";
const group = "c000000a-0000-4000-8000-000000000000";
const otherGroup = "c000000b-0000-4000-8000-000000000000";
const channel = "c000000c-0000-4000-8000-000000000000";

type Deferred<T> = { promise: Promise<T>, resolve: (value: T) => void };

function deferred<T>(): Deferred<T> {
    let resolve!: (value: T) => void;
    const promise = new Promise<T>(r => resolve = r);
    return { promise, resolve };
}

async function settle(): Promise<void> {
    for (let i = 0; i < 10; i++) {
        await Promise.resolve();
    }
}

function person(alias: string, nick: string | null, last: string | null = null): ChatPerson {
    return { person_alias_guid: alias, nick_name: nick, last_name: last, avatar_url: null };
}

/** A refusal as the shell's call path classifies one: its severity by its family, and the route's sentence when it sent one. */
function refused(code: string, status = 400, text?: string): CallResult {
    const error = code === "rpc.transport" ? classifyPlatformError(new TypeError("Failed to fetch")) : classifyPlatformError({ message: code, hint: text, status });
    return { ok: false, status, error, retryAfterSeconds: null };
}

type Call = { path: string, body: Record<string, unknown>, options: { headers?: Record<string, string> } | undefined };

/** A presence with every outside piece stood in for; a test changes what it reads through the handles returned. */
function build(rawSettings: Record<string, unknown> = {}) {
    const calls: Call[] = [];
    // Answered at once with a pass unless a test holds the next answers to settle them by hand.
    const held: { value: Deferred<CallResult>[] | null } = { value: null };
    const answers: CallResult[] = [];
    // A call that rejects rather than answering, as a token refresh that throws would.
    const rejections: Error[] = [];
    const settings = {
        value: readSettings({ flags: { typing: true }, routes: { region: "us-east-1" }, ...rawSettings })
    };
    const token = { value: "token-1" as string | null };
    const state = reactive({ activeChannelId: group as string | null, connection: null as ChatError | null, isComposerOpen: true });
    const rows: Record<string, Partial<SidebarRow>> = {
        [group]: { channel_id: group, room_kind: "group", can_post: true },
        [otherGroup]: { channel_id: otherGroup, room_kind: "person", can_post: true },
        [channel]: { channel_id: channel, room_kind: "channel", can_post: true }
    };
    const people = reactive<Record<string, ChatPerson[]>>({
        [group]: [person(anna, "Anna", "Reyes"), person(sam, "Sam", "Okafor"), person(lee, "Lee", "Park")]
    });
    const blocked = new Set<string>();
    const reports: string[] = [];
    const stopped = { value: false };
    // The newest message the person's client holds for each place, by place key.
    const newest: Record<string, number | null> = { [group]: 10 };
    // Places whose history may be behind the person, as a send still out leaves one.
    const behind = new Set<string>();
    // The newest message the client holds from each person in each place, by place key and
    // lower-case alias, whether a page or a live copy brought it.
    const sent: Record<string, Record<string, number>> = {};

    const dependencies: PresenceDependencies = {
        callFunction: (path, body, options) => {
            calls.push({ path, body, options: options as Call["options"] });
            const rejection = rejections.shift();
            if (rejection) {
                return Promise.reject(rejection);
            }
            if (held.value) {
                const answer = deferred<CallResult>();
                held.value.push(answer);
                return answer.promise;
            }
            return Promise.resolve(answers.shift() ?? { ok: true, data: { sent: true } });
        },
        settings: () => settings.value,
        currentToken: () => token.value,
        state,
        rowOf: channelId => (rows[channelId] ?? null) as SidebarRow | null,
        peopleOf: channelId => people[channelId] ?? [],
        newestHeld: (channelId, rootId) => newest[quoteKey(channelId, rootId)] ?? null,
        isBehind: (channelId, rootId) => behind.has(quoteKey(channelId, rootId)),
        newestFrom: (channelId, rootId, alias) => sent[quoteKey(channelId, rootId)]?.[alias] ?? null,
        personAliasGuid: me,
        isBlocked: alias => blocked.has(alias.toLowerCase()),
        report: code => reports.push(code),
        isStopped: () => stopped.value
    };

    const presence: Presence = createPresence(dependencies);

    /** A page or a live copy brings someone's message into the client's history. */
    function hold(id: number, alias: string, parentId: number | null = null): void {
        const place = quoteKey(group, parentId);
        sent[place] = { ...sent[place], [alias.toLowerCase()]: Math.max(sent[place]?.[alias.toLowerCase()] ?? 0, id) };
    }

    /** A message arrives live: the history takes it, then the presence hears it, as the shell runs them. */
    function arrive(id: number, alias: string, parentId: number | null = null): void {
        hold(id, alias, parentId);
        presence.messageArrived(group, { id, channel_id: group, parent_id: parentId, person_alias_guid: alias });
    }

    return { presence, calls, held, answers, rejections, settings, token, state, rows, people, blocked, reports, stopped, newest, behind, hold, arrive };
}

/** A typing signal as chat_typing writes it. */
function signal(alias: string, extra: Record<string, unknown> = {}): Record<string, unknown> {
    return { channel_id: group, parent_id: null, person_alias_guid: alias, id: `sig-${alias}`, after: 10, ...extra };
}

beforeEach(() => {
    jest.useFakeTimers();
});

afterEach(() => {
    jest.useRealTimers();
});

describe("the person's own signals", () => {
    test("the first change to a non-empty box calls the typing route once, naming the place, the newest message held and the database's region", async () => {
        const h = build();

        h.presence.typed(group, null, "H");
        await settle();

        expect(h.calls).toEqual([{
            path: "chat-typing",
            body: { p_channel_id: group, p_parent_id: null, p_after: 10 },
            options: { headers: { "x-region": "us-east-1" } }
        }]);
    });

    test("with no region in the settings the call carries no header, and with no message held it names none followed", async () => {
        const h = build({ routes: {} });
        h.newest[group] = null;

        h.presence.typed(group, null, "H");
        await settle();

        expect(h.calls).toHaveLength(1);
        expect(h.calls[0].options?.headers?.["x-region"]).toBeUndefined();
        expect(h.calls[0].body).toEqual({ p_channel_id: group, p_parent_id: null });
    });

    test.each([
        [4000, 3],
        [8000, 2]
    ])("eight changes a second for ten seconds at an interval of %i ms call %i times, the first at once and none after the person stops", async (interval, expected) => {
        const h = build({ flags: { typing: true }, limits: { typing_interval_ms: interval } });

        for (let i = 0; i < 80; i++) {
            h.presence.typed(group, null, "x".repeat(i + 1));
            await settle();
            jest.advanceTimersByTime(125);
        }
        jest.advanceTimersByTime(20000);
        await settle();

        expect(h.calls).toHaveLength(expected);
    });

    test("the interval is read at each change, so a new token's pace applies on the next key", async () => {
        const h = build();
        h.presence.typed(group, null, "a");
        await settle();

        h.settings.value = readSettings({ flags: { typing: true }, routes: { region: "us-east-1" }, limits: { typing_interval_ms: 1000 } });
        jest.advanceTimersByTime(1000);
        h.presence.typed(group, null, "ab");
        await settle();

        expect(h.calls).toHaveLength(2);
    });

    test("a thread sends its root and is paced apart from the conversation", async () => {
        const h = build();
        h.newest[quoteKey(group, 40)] = 44;

        h.presence.typed(group, null, "a");
        await settle();
        h.presence.typed(group, 40, "b");
        await settle();
        h.presence.typed(group, 40, "bc");
        await settle();

        expect(h.calls.map(c => c.body)).toEqual([
            { p_channel_id: group, p_parent_id: null, p_after: 10 },
            { p_channel_id: group, p_parent_id: 40, p_after: 44 }
        ]);
    });

    test("no second call goes while one is out, and the next change after it settles and the interval passes goes", async () => {
        const h = build();
        h.held.value = [];

        h.presence.typed(group, null, "a");
        await settle();
        h.presence.typed(group, 40, "b");
        jest.advanceTimersByTime(5000);
        h.presence.typed(group, null, "abc");
        await settle();
        expect(h.calls).toHaveLength(1);

        h.held.value[0].resolve({ ok: true, data: { sent: true } });
        await settle();
        h.presence.typed(group, null, "abcd");
        await settle();
        expect(h.calls).toHaveLength(2);
    });

    test("nothing is sent while the person's own send to that place is still out, and the next change after it lands is", async () => {
        const h = build();
        h.behind.add(group);

        h.presence.typed(group, null, "a");
        await settle();
        expect(h.calls).toHaveLength(0);

        h.behind.delete(group);
        h.newest[group] = 12;
        h.presence.typed(group, null, "ab");
        await settle();
        expect(h.calls.map(c => c.body.p_after)).toEqual([12]);
    });

    test("nothing is sent in a channel, for an empty or blank box, with typing off, with the connection lost, with the composer closed, or where the person cannot post", async () => {
        const cases: Array<(h: ReturnType<typeof build>) => [string, string]> = [
            () => [channel, "a"],
            () => [group, ""],
            () => [group, "   \n "],
            h => {
                h.settings.value = readSettings({ flags: { typing: false } });
                return [group, "a"];
            },
            h => {
                h.state.connection = { code: "rpc.transport", severity: "transient" } as ChatError;
                return [group, "a"];
            },
            h => {
                h.state.isComposerOpen = false;
                return [group, "a"];
            },
            h => {
                h.rows[group] = { ...h.rows[group], can_post: false };
                return [group, "a"];
            }
        ];

        for (const arrange of cases) {
            const h = build();
            const [channelId, text] = arrange(h);
            h.presence.typed(channelId, null, text);
            await settle();
            expect(h.calls).toHaveLength(0);
        }
    });

    test("typing refused as off is not asked again until the session holds another token", async () => {
        const h = build();
        h.answers.push(refused("rpc.typing_off", 403, "Typing indicators are turned off in this church's chat."));

        h.presence.typed(group, null, "a");
        await settle();
        jest.advanceTimersByTime(10000);
        h.presence.typed(group, null, "ab");
        await settle();
        expect(h.calls).toHaveLength(1);

        h.token.value = "token-2";
        h.presence.typed(group, null, "abc");
        await settle();
        expect(h.calls).toHaveLength(2);
        expect(h.reports).toEqual([]);
    });

    test("the token silenced is the one the refused call went with, so a token fetched while it was out is not", async () => {
        const h = build();
        h.held.value = [];

        h.presence.typed(group, null, "a");
        await settle();
        h.token.value = "token-2";
        h.held.value[0].resolve(refused("rpc.typing_off", 403, "Typing indicators are turned off in this church's chat."));
        await settle();
        h.held.value = null;

        jest.advanceTimersByTime(5000);
        h.presence.typed(group, null, "ab");
        await settle();
        expect(h.calls).toHaveLength(2);
    });

    test("a conversation refused as not one typing shows in is not asked again this page, nor are its threads, and another still is", async () => {
        const h = build();
        h.answers.push(refused("rpc.typing_not_here", 400, "Typing indicators show only in direct messages and groups."));

        // refused from a thread, so the silence is the conversation's and not only that thread's
        h.presence.typed(group, 40, "a");
        await settle();
        jest.advanceTimersByTime(5000);
        h.presence.typed(group, 40, "ab");
        h.presence.typed(group, null, "a");
        h.presence.typed(group, 41, "a");
        h.presence.typed(otherGroup, null, "a");
        await settle();

        expect(h.calls.map(c => c.body.p_channel_id)).toEqual([group, otherGroup]);
    });

    test("a thread whose root is gone is not asked again this page, and its conversation still is", async () => {
        const h = build();
        h.answers.push(refused("rpc.bad_parent"));

        h.presence.typed(group, 40, "a");
        await settle();
        jest.advanceTimersByTime(5000);
        h.presence.typed(group, 40, "ab");
        h.presence.typed(group, null, "a");
        await settle();

        expect(h.calls.map(c => c.body.p_parent_id)).toEqual([40, null]);
    });

    test.each([
        ["authz.not_member", 403, "You're not in this conversation."],
        ["rpc.transport", 0, undefined],
        ["rpc.unavailable", 503, undefined],
        ["auth.expired", 401, "Your chat session expired. Reconnecting."]
    ])("%s is dropped quietly, unreported, and the next change after the interval calls again", async (code, status, text) => {
        const h = build();
        h.answers.push(refused(code, status, text));

        h.presence.typed(group, null, "a");
        await settle();
        jest.advanceTimersByTime(4000);
        h.presence.typed(group, null, "ab");
        await settle();

        expect(h.calls).toHaveLength(2);
        expect(h.reports).toEqual([]);
    });

    test("a call that rejects is dropped quietly like any failure, and the next change after the interval calls again", async () => {
        const h = build();
        h.rejections.push(new Error("the refresh threw"));

        h.presence.typed(group, null, "a");
        await settle();
        jest.advanceTimersByTime(4000);
        h.presence.typed(group, null, "ab");
        await settle();

        expect(h.calls).toHaveLength(2);
        expect(h.reports).toEqual([]);
    });

    test("a code this release has no words for is reported by its code", async () => {
        const h = build();
        h.answers.push(refused("rpc.typing_new_thing"));

        h.presence.typed(group, null, "a");
        await settle();

        expect(h.reports).toEqual(["rpc.typing_new_thing"]);
    });

    // The route could not read what this client sent: the client and the contract have drifted,
    // which no words the person might be shown make any less worth telling.
    test("a request the route could not read is reported, with the route's sentence or without", async () => {
        for (const text of ["Chat could not understand that request.", undefined]) {
            const h = build();
            h.answers.push(refused("rpc.bad_request", 400, text));

            h.presence.typed(group, null, "a");
            await settle();

            expect(h.reports).toEqual(["rpc.bad_request"]);
        }
    });
});

describe("the others' signals", () => {
    test("a signal for the open conversation shows its typist in the conversation's place, from either topic", () => {
        const h = build();

        h.presence.heard("typing", signal(anna), group);
        h.presence.heard("typing", signal(sam));

        expect(h.presence.state.typists[group]).toEqual([anna, sam]);
    });

    test("only the typing event is the presence's", () => {
        const h = build();

        expect(h.presence.owns("typing")).toBe(true);
        expect(h.presence.owns("message.created")).toBe(false);
    });

    test("a typist shows until the expiry and goes at it, and a fresh signal holds them on", () => {
        const h = build();
        h.presence.heard("typing", signal(anna), group);

        jest.advanceTimersByTime(5999);
        expect(h.presence.state.typists[group]).toEqual([anna]);
        jest.advanceTimersByTime(1);
        expect(h.presence.state.typists[group] ?? []).toEqual([]);

        h.presence.heard("typing", signal(anna), group);
        jest.advanceTimersByTime(5000);
        h.presence.heard("typing", signal(anna), group);
        jest.advanceTimersByTime(5999);
        expect(h.presence.state.typists[group]).toEqual([anna]);
        jest.advanceTimersByTime(1);
        expect(h.presence.state.typists[group] ?? []).toEqual([]);
    });

    // The typist paces by their own token, which can name a longer interval than the viewer's
    // while the platform's cap is moving; the line lasts by their pace, with the viewer's own
    // margin of expiry over interval (6,000 over 4,000 here).
    test("a signal naming its typist's interval keeps the line for that interval with the expiry's margin", () => {
        const h = build();
        h.presence.heard("typing", signal(anna, { interval_ms: 8000 }), group);

        jest.advanceTimersByTime(11999);
        expect(h.presence.state.typists[group]).toEqual([anna]);
        jest.advanceTimersByTime(1);
        expect(h.presence.state.typists[group] ?? []).toEqual([]);
    });

    test("a signal naming no interval, or one that is not a positive number, keeps the line for the viewer's own expiry", () => {
        const h = build();
        h.presence.heard("typing", signal(anna), group);
        h.presence.heard("typing", signal(sam, { interval_ms: "8000" }), group);
        h.presence.heard("typing", signal(lee, { interval_ms: 0 }), group);

        jest.advanceTimersByTime(5999);
        expect(h.presence.state.typists[group]).toEqual([anna, sam, lee]);
        jest.advanceTimersByTime(1);
        expect(h.presence.state.typists[group] ?? []).toEqual([]);
    });

    test("however many typists are held, one timer waits for the soonest", () => {
        const h = build();

        h.presence.heard("typing", signal(anna), group);
        jest.advanceTimersByTime(1000);
        h.presence.heard("typing", signal(sam, { parent_id: 40 }), group);

        expect(jest.getTimerCount()).toBe(1);
        jest.advanceTimersByTime(5000);
        expect(h.presence.state.typists[group] ?? []).toEqual([]);
        expect(h.presence.state.typists[quoteKey(group, 40)]).toEqual([sam]);
        expect(jest.getTimerCount()).toBe(1);
    });

    test("a message by the typist in that place takes them off it at once, and leaves the others and the other places", () => {
        const h = build();
        h.presence.heard("typing", signal(anna), group);
        h.presence.heard("typing", signal(sam), group);
        h.presence.heard("typing", signal(anna, { parent_id: 40 }), group);

        h.arrive(11, anna);

        expect(h.presence.state.typists[group]).toEqual([sam]);
        expect(h.presence.state.typists[quoteKey(group, 40)]).toEqual([anna]);
    });

    test("a signal that follows an older message than one of theirs already held is dropped, as it was sent before that message", () => {
        const h = build();
        h.arrive(11, anna);

        h.presence.heard("typing", signal(anna, { after: 10 }), group);
        expect(h.presence.state.typists[group] ?? []).toEqual([]);

        h.presence.heard("typing", signal(anna, { after: 11 }), group);
        expect(h.presence.state.typists[group]).toEqual([anna]);
    });

    test("a signal that already follows a message stays when that message arrives after it", () => {
        const h = build();

        h.presence.heard("typing", signal(anna, { after: 11 }), group);
        h.arrive(11, anna);

        expect(h.presence.state.typists[group]).toEqual([anna]);
    });

    test("a signal naming no message followed, or from a platform that names none, counts as following none", () => {
        const h = build();
        h.arrive(11, anna);
        h.arrive(12, sam);
        const older = signal(sam);
        delete older.after;

        h.presence.heard("typing", signal(anna, { after: null }), group);
        h.presence.heard("typing", older, group);

        expect(h.presence.state.typists[group] ?? []).toEqual([]);
    });

    test("ignored: another conversation, a conversation topic's signal naming another, the person's own in any case, someone blocked, and a signal with no one", () => {
        const h = build();
        h.blocked.add(lee);

        h.presence.heard("typing", signal(anna, { channel_id: otherGroup }));
        h.presence.heard("typing", signal(sam), otherGroup);
        h.presence.heard("typing", signal(me.toUpperCase()), group);
        h.presence.heard("typing", signal(me));
        h.presence.heard("typing", signal(lee), group);
        h.presence.heard("typing", { channel_id: group, parent_id: null, id: "x" }, group);
        h.presence.heard("typing", null, group);

        expect(h.presence.state.typists).toEqual({});
    });

    test("a reply typed in a thread shows only under the thread's place", () => {
        const h = build();

        h.presence.heard("typing", signal(anna, { parent_id: 40 }), group);

        expect(h.presence.state.typists[quoteKey(group, 40)]).toEqual([anna]);
        expect(h.presence.state.typists[group]).toBeUndefined();
        expect(h.presence.typingIn(group)).toBeNull();
        expect(h.presence.typingIn(quoteKey(group, 40))).toMatchObject({ names: "Anna", sentence: "Anna is typing…" });
    });

    // Signals missed meanwhile cannot be caught up, so every typist goes; what was sent is the
    // history's, which a rejoin keeps or pages back in, so a late signal is still judged by it.
    test("a topic joined again forgets every typist, and a signal sent before a message the history still holds stays dropped", () => {
        const h = build();
        h.presence.heard("typing", signal(anna), group);
        h.presence.heard("typing", signal(sam, { parent_id: 40 }), group);
        h.arrive(11, lee);

        h.presence.joined(group);
        expect(h.presence.state.typists).toEqual({});
        expect(jest.getTimerCount()).toBe(0);

        h.presence.heard("typing", signal(lee, { after: 10 }), group);
        expect(h.presence.state.typists[group] ?? []).toEqual([]);
        h.presence.heard("typing", signal(lee, { after: 11 }), group);
        expect(h.presence.state.typists[group]).toEqual([lee]);
    });

    // After a reconnect or a reopen her message comes back in a page, never heard live; a signal
    // she sent before it, arriving late, would otherwise draw her typing under her own message.
    test("a signal sent before a message of theirs that a page brought, not a live copy, is dropped", () => {
        const h = build();
        h.presence.joined(group);
        h.hold(100, anna);
        h.hold(50, sam, 40);

        h.presence.heard("typing", signal(anna, { after: 99 }), group);
        h.presence.heard("typing", signal(sam, { parent_id: 40, after: 49 }), group);
        expect(h.presence.state.typists).toEqual({});

        h.presence.heard("typing", signal(anna, { after: 100 }), group);
        h.presence.heard("typing", signal(sam, { parent_id: 40, after: 50 }), group);
        expect(h.presence.state.typists[group]).toEqual([anna]);
        expect(h.presence.state.typists[quoteKey(group, 40)]).toEqual([sam]);
    });

    test("someone just blocked leaves the line at once", () => {
        const h = build();
        h.presence.heard("typing", signal(anna), group);
        h.presence.heard("typing", signal(sam), group);

        h.blocked.add(anna);
        h.presence.remask();

        expect(h.presence.state.typists[group]).toEqual([sam]);
    });
});

describe("the typing words", () => {
    test("one, two, and three or more typists", () => {
        expect(typingWords([])).toBeNull();
        expect(typingWords([person(anna, "Anna")])).toEqual({ names: "Anna", sentence: "Anna is typing…" });
        expect(typingWords([person(anna, "Anna"), person(sam, "Sam")])).toEqual({ names: "Anna and Sam", sentence: "Anna and Sam are typing…" });
        expect(typingWords([person(anna, "Anna"), person(sam, "Sam"), person(lee, "Lee")])).toEqual({ names: "Several people", sentence: "Several people are typing…" });
        expect(typingWords([person(anna, "Anna"), person(sam, "Sam"), person(lee, "Lee"), null, null])).toEqual({ names: "Several people", sentence: "Several people are typing…" });
    });

    test("someone not held reads as someone, and a person with no nick name by their last name", () => {
        expect(typingWords([null])).toEqual({ names: "Someone", sentence: "Someone is typing…" });
        expect(typingWords([person(anna, "Anna"), null])).toEqual({ names: "Anna and someone", sentence: "Anna and someone are typing…" });
        expect(typingWords([null, person(anna, "Anna")])).toEqual({ names: "Someone and Anna", sentence: "Someone and Anna are typing…" });
        expect(typingWords([person(anna, null, "Reyes")])).toEqual({ names: "Reyes", sentence: "Reyes is typing…" });
    });

    test("two typists of one nick name are told apart by their last initial", () => {
        expect(typingWords([person(anna, "Anna", "Baker"), person(sam, "Anna", "Kim")])).toEqual({ names: "Anna B. and Anna K.", sentence: "Anna B. and Anna K. are typing…" });
    });

    test("names and faces come from the room's people, here a named group of five", () => {
        const h = build();
        const five = "c000000f-0000-4000-8000-000000000000";
        h.state.activeChannelId = five;
        h.people[five] = [person(anna, "Anna", "Reyes"), person(sam, "Sam", "Okafor"), person(lee, "Lee", "Park"), person("a0000004-0000-4000-8000-000000000000", "Kai", "Lu")];

        h.presence.heard("typing", signal(sam, { channel_id: five }), five);
        expect(h.presence.typingIn(five)).toEqual({
            typists: [{ alias: sam, person: person(sam, "Sam", "Okafor") }],
            names: "Sam",
            sentence: "Sam is typing…"
        });

        // someone the facts do not name is still a typist, drawn with no face of their own
        const stranger = "a000000f-0000-4000-8000-000000000000";
        h.presence.heard("typing", signal(stranger, { channel_id: five }), five);
        expect(h.presence.typingIn(five)).toEqual({
            typists: [{ alias: sam, person: person(sam, "Sam", "Okafor") }, { alias: stranger, person: null }],
            names: "Sam and someone",
            sentence: "Sam and someone are typing…"
        });
        expect(h.presence.typingIn(null)).toBeNull();
    });
});

// A signal refreshes its typist every interval and an expiry anywhere republishes every place,
// so the bubble is handed the same value until who it draws changes, and draws nothing again.
describe("the typing a place draws", () => {
    test("is the same value while its typists and their people are unchanged, and a new one when they change", () => {
        const h = build();
        h.presence.heard("typing", signal(anna), group);
        const first = h.presence.typingIn(group);

        h.presence.heard("typing", signal(anna), group);
        h.presence.heard("typing", signal(sam, { parent_id: 40 }), group);
        expect(h.presence.typingIn(group)).toBe(first);

        h.presence.heard("typing", signal(sam), group);
        const two = h.presence.typingIn(group);
        expect(two).not.toBe(first);
        expect(two?.names).toBe("Anna and Sam");

        h.people[group] = [person(anna, "Ann", "Reyes"), person(sam, "Sam", "Okafor")];
        expect(h.presence.typingIn(group)).not.toBe(two);
        expect(h.presence.typingIn(group)?.names).toBe("Ann and Sam");
    });
});

describe("a stopped presence", () => {
    test("forgets every typist and timer, sends nothing, keeps nothing heard, and writes nothing an answer brings after", async () => {
        const h = build();
        h.held.value = [];
        h.presence.typed(group, null, "a");
        await settle();
        h.presence.heard("typing", signal(anna), group);

        h.stopped.value = true;
        h.presence.stop();
        expect(h.presence.state.typists).toEqual({});
        expect(jest.getTimerCount()).toBe(0);

        h.held.value[0].resolve(refused("rpc.typing_new_thing"));
        await settle();
        expect(h.reports).toEqual([]);

        jest.advanceTimersByTime(10000);
        h.presence.typed(group, null, "ab");
        h.presence.heard("typing", signal(sam), group);
        h.arrive(11, sam);
        await settle();
        expect(h.calls).toHaveLength(1);
        expect(h.presence.state.typists).toEqual({});
        expect(jest.getTimerCount()).toBe(0);
    });
});
