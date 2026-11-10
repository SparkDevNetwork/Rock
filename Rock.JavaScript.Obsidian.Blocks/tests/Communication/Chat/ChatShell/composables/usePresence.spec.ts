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

// Typing: the person's own turn in a place, which sends nothing until the box has held text for
// the first wait, then at most once an interval while the text keeps changing and once more after
// it stops, starts again with each send, stops once when an announced box is cleared, and ends with
// nothing sent when the place leaves the screen; never in a channel and never while nothing could
// come of a signal. And the others' typing, drawn at once and kept per place whatever room is open
// until it expires, stops, is overtaken, sent or blocked, each end recorded once with its cause and
// never before a typist has been shown for the minimum; and the platform's answer of who is typing,
// drawn as what is.
import { reactive, shallowRef } from "vue";
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

/** Time passes, then whatever it set going settles. */
async function pass(ms: number): Promise<void> {
    jest.advanceTimersByTime(ms);
    await settle();
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

/** The settings a platform with the stop and the first signal's wait answers, typing on. */
const current = { flags: { typing: true }, routes: { region: "us-east-1" }, limits: { typing_first_after_ms: 700 } };

/** A presence with every outside piece stood in for; a test changes what it reads through the handles returned. */
function build(rawSettings: Record<string, unknown> = {}) {
    const calls: Call[] = [];
    // Answered at once with a pass unless a test holds the next answers to settle them by hand.
    const held: { value: Deferred<CallResult>[] | null } = { value: null };
    const answers: CallResult[] = [];
    // A call that rejects rather than answering, as a token refresh that throws would.
    const rejections: Error[] = [];
    // a shallow ref, as the session holds them, so a change of the switch is seen as it lands
    const settings = shallowRef(readSettings({ ...current, ...rawSettings }));
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
    // Places the person has left: every other is on screen.
    const offScreen = reactive(new Set<string>());
    // The newest message the client holds from each person in each place, by place key and
    // lower-case alias, whether a page or a live copy brought it.
    const sent: Record<string, Record<string, number>> = {};
    // What the presence told the record, in order.
    const records: string[] = [];

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
        isOnScreen: (channelId, rootId) => !offScreen.has(quoteKey(channelId, rootId)),
        newestFrom: (channelId, rootId, alias) => sent[quoteKey(channelId, rootId)]?.[alias] ?? null,
        personAliasGuid: me,
        isBlocked: alias => blocked.has(alias.toLowerCase()),
        motion: {
            typingStarted: place => records.push(`started ${place}`),
            typingEnded: (place, cause, message) => records.push(`ended ${place} ${cause}${message ? ` ${message.id}` : ""}`)
        },
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

    return { presence, calls, held, answers, rejections, settings, token, state, rows, people, blocked, reports, stopped, newest, behind, offScreen, records, hold, arrive };
}

/** A typing signal as chat_typing writes it. */
function signal(alias: string, extra: Record<string, unknown> = {}): Record<string, unknown> {
    return { channel_id: group, parent_id: null, person_alias_guid: alias, id: `sig-${alias}`, after: 10, ...extra };
}

/** The stop chat_typing writes when the typist clears an announced box. */
function stop(alias: string, extra: Record<string, unknown> = {}): Record<string, unknown> {
    return signal(alias, { stopped: true, ...extra });
}

beforeEach(() => {
    jest.useFakeTimers();
});

afterEach(() => {
    jest.useRealTimers();
});

describe("the person's own signals", () => {
    test("nothing goes until the box has held text for the first wait, then one call naming the place, the newest message held and the database's region", async () => {
        const h = build();

        h.presence.typed(group, null, "H");
        await pass(699);
        expect(h.calls).toHaveLength(0);
        await pass(1);

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
        await pass(700);

        expect(h.calls).toHaveLength(1);
        expect(h.calls[0].options?.headers?.["x-region"]).toBeUndefined();
        expect(h.calls[0].body).toEqual({ p_channel_id: group, p_parent_id: null });
    });

    test("the first wait is the platform's, read as the turn starts", async () => {
        const h = build({ limits: { typing_first_after_ms: 300 } });

        h.presence.typed(group, null, "H");
        await pass(300);

        expect(h.calls).toHaveLength(1);
    });

    // A quick "hi" never shows typing and costs nothing.
    test("a box emptied or sent within the first wait sends nothing at all", async () => {
        const h = build();

        h.presence.typed(group, null, "h");
        await pass(300);
        h.presence.typed(group, null, "");
        h.presence.typed(group, 40, "hi");
        await pass(300);
        h.presence.sent(group, 40);
        await pass(5000);

        expect(h.calls).toHaveLength(0);
        expect(jest.getTimerCount()).toBe(0);
    });

    // The signal after the first goes at the interval while the text changes, and one more an
    // interval after the last if it changed since: it is what keeps the others' bubble up while
    // the person rereads before sending.
    test.each([
        [4000, [700, 4700, 8700, 12700]],
        [8000, [700, 8700, 16700]]
    ])("eight changes a second for ten seconds at an interval of %i ms call at %j ms, the last one interval after the one before, and none after", async (interval, expected) => {
        const h = build({ limits: { typing_first_after_ms: 700, typing_interval_ms: interval } });
        const start = Date.now();
        const at: number[] = [];
        h.held.value = null;
        const record = (): void => {
            while (at.length < h.calls.length) {
                at.push(Date.now() - start);
            }
        };

        for (let i = 0; i < 80; i++) {
            h.presence.typed(group, null, "x".repeat(i + 1));
            for (let step = 0; step < 5; step++) {
                await pass(25);
                record();
            }
        }
        for (let step = 0; step < 1000; step++) {
            await pass(25);
            record();
        }

        expect(at).toEqual(expected);
    });

    test("an announced turn whose text did not change since its last signal sends nothing more", async () => {
        const h = build();
        h.presence.typed(group, null, "a");
        await pass(700);

        await pass(20000);

        expect(h.calls).toHaveLength(1);
        expect(jest.getTimerCount()).toBe(0);
    });

    test("the interval is read at each change, so a new token's pace applies on the next key", async () => {
        const h = build();
        h.presence.typed(group, null, "a");
        await pass(700);

        h.settings.value = readSettings({ ...current, limits: { ...current.limits, typing_interval_ms: 1000 } });
        jest.advanceTimersByTime(1000);
        h.presence.typed(group, null, "ab");
        await settle();

        expect(h.calls).toHaveLength(2);
    });

    // A send starts a new turn: the next message's typing shows as soon as the first's did, and the
    // platform passes it as a new turn rather than holding it to the interval.
    test("a send ends the turn: no last signal goes for it, and the next message's first goes after the wait, not the interval", async () => {
        const h = build();
        h.presence.typed(group, null, "a");
        await pass(700);
        h.presence.typed(group, null, "ab");

        h.presence.sent(group, null);
        await pass(5000);
        expect(h.calls).toHaveLength(1);

        h.newest[group] = 11;
        h.presence.typed(group, null, "n");
        await pass(700);
        expect(h.calls.map(c => c.body.p_after)).toEqual([10, 11]);
    });

    // The composer clears the box once it has handed the text to the send, which ends the turn
    // first, so the clear is not someone giving up: her message takes the bubble's place.
    test("a send that clears an announced box sends no stop", async () => {
        const h = build();
        h.presence.typed(group, null, "a");
        await pass(700);

        h.presence.sent(group, null);
        h.presence.typed(group, null, "");
        await pass(5000);

        expect(h.calls.map(c => c.body)).toEqual([{ p_channel_id: group, p_parent_id: null, p_after: 10 }]);
    });

    test("clearing an announced box sends one stop; a box never announced, or cleared again, sends none", async () => {
        const h = build();
        h.presence.typed(group, null, "a");
        await pass(700);

        h.presence.typed(group, null, "");
        h.presence.typed(group, null, "   ");
        await settle();
        h.presence.typed(group, 40, "b");
        h.presence.typed(group, 40, "");
        await pass(5000);

        expect(h.calls.map(c => c.body)).toEqual([
            { p_channel_id: group, p_parent_id: null, p_after: 10 },
            { p_channel_id: group, p_parent_id: null, p_after: 10, p_stopped: true }
        ]);
        expect(jest.getTimerCount()).toBe(0);
    });

    test("a stop goes whatever the platform's settings name, and a turn still waits the client's own first wait", async () => {
        const h = build({ limits: {} });
        h.presence.typed(group, null, "a");
        await pass(699);
        expect(h.calls).toHaveLength(0);
        await pass(1);

        h.presence.typed(group, null, "");
        await settle();

        expect(h.calls.map(c => c.body.p_stopped)).toEqual([undefined, true]);
    });

    test("a stop the platform refuses is dropped quietly: nothing reported, nothing asked again, and the turn stays ended", async () => {
        const h = build();
        h.presence.typed(group, null, "a");
        await pass(700);
        h.answers.push(refused("rpc.unknown_option"));

        h.presence.typed(group, null, "");
        await pass(10000);

        expect(h.calls).toHaveLength(2);
        expect(h.reports).toEqual([]);
        expect(jest.getTimerCount()).toBe(0);
    });

    test("a place that leaves the screen ends its turn with nothing sent: no first signal and no last", async () => {
        const h = build();
        h.presence.typed(group, null, "a");
        await pass(700);
        h.presence.typed(group, null, "ab");
        h.presence.typed(group, 40, "c");

        h.offScreen.add(group);
        h.offScreen.add(quoteKey(group, 40));
        // ended as the place goes, not when a timer finds it gone
        expect(jest.getTimerCount()).toBe(0);
        await pass(10000);

        expect(h.calls).toHaveLength(1);
        expect(jest.getTimerCount()).toBe(0);

        // back in the room, typing is a new turn that waits the first wait again
        h.offScreen.delete(group);
        h.presence.typed(group, null, "abc");
        await pass(699);
        expect(h.calls).toHaveLength(1);
        await pass(1);
        expect(h.calls).toHaveLength(2);
    });

    test("a place left and come back to before its last signal was due sends nothing for the turn it left", async () => {
        const h = build();
        h.presence.typed(group, null, "a");
        await pass(700);
        h.presence.typed(group, null, "ab");

        h.offScreen.add(group);
        await pass(100);
        h.offScreen.delete(group);
        await pass(5000);

        expect(h.calls).toHaveLength(1);
    });

    test("a thread left for another thread ends its turn with nothing sent", async () => {
        const h = build();
        h.presence.typed(group, 40, "a");
        await pass(700);
        h.presence.typed(group, 40, "ab");

        h.offScreen.add(quoteKey(group, 40));
        await pass(5000);

        expect(h.calls).toHaveLength(1);
    });

    test("a turn holds one timer however many changes it hears", async () => {
        const h = build();
        for (const text of ["a", "ab", "abc"]) {
            h.presence.typed(group, null, text);
        }
        expect(jest.getTimerCount()).toBe(1);

        await pass(700);
        for (const text of ["abcd", "abcde"]) {
            h.presence.typed(group, null, text);
        }
        expect(jest.getTimerCount()).toBe(1);
    });

    test("a thread sends its root and is paced apart from the conversation", async () => {
        const h = build();
        h.newest[quoteKey(group, 40)] = 44;

        h.presence.typed(group, null, "a");
        h.presence.typed(group, 40, "b");
        await pass(700);
        h.presence.typed(group, 40, "bc");
        await settle();

        expect(h.calls.map(c => c.body)).toEqual([
            { p_channel_id: group, p_parent_id: null, p_after: 10 },
            { p_channel_id: group, p_parent_id: 40, p_after: 44 }
        ]);
    });

    test("a signal due while another call is out waits for it, and goes as it ends", async () => {
        const h = build();
        h.held.value = [];

        h.presence.typed(group, null, "a");
        await pass(700);
        h.presence.typed(group, 40, "b");
        await pass(700);
        expect(h.calls).toHaveLength(1);

        h.held.value[0].resolve({ ok: true, data: { sent: true } });
        await settle();
        expect(h.calls.map(c => c.body.p_parent_id)).toEqual([null, 40]);
    });

    test("a signal due while the person's own send to that place is out waits for it to land, and goes then naming it", async () => {
        const h = build();
        h.behind.add(group);

        h.presence.typed(group, null, "a");
        await pass(700);
        expect(h.calls).toHaveLength(0);

        h.behind.delete(group);
        h.newest[group] = 12;
        h.presence.caughtUp();
        await settle();
        expect(h.calls.map(c => c.body.p_after)).toEqual([12]);

        h.presence.caughtUp();
        await settle();
        expect(h.calls).toHaveLength(1);
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
            await pass(5000);
            expect(h.calls).toHaveLength(0);
        }
    });

    test("the connection lost when the first wait ends sends nothing then", async () => {
        const h = build();
        h.presence.typed(group, null, "a");
        h.state.connection = { code: "rpc.transport", severity: "transient" } as ChatError;

        await pass(700);

        expect(h.calls).toHaveLength(0);
    });

    test("typing refused as off is not asked again until the session holds another token", async () => {
        const h = build();
        h.answers.push(refused("rpc.typing_off", 403, "Typing indicators are turned off in this church's chat."));

        h.presence.typed(group, null, "a");
        await pass(700);
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
        await pass(700);
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
        await pass(700);
        jest.advanceTimersByTime(5000);
        h.presence.typed(group, 40, "ab");
        h.presence.typed(group, null, "a");
        h.presence.typed(group, 41, "a");
        h.presence.typed(otherGroup, null, "a");
        await pass(700);

        expect(h.calls.map(c => c.body.p_channel_id)).toEqual([group, otherGroup]);
    });

    test("a thread whose root is gone is not asked again this page, and its conversation still is", async () => {
        const h = build();
        h.answers.push(refused("rpc.bad_parent"));

        h.presence.typed(group, 40, "a");
        await pass(700);
        jest.advanceTimersByTime(5000);
        h.presence.typed(group, 40, "ab");
        h.presence.typed(group, null, "a");
        await pass(700);

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
        await pass(700);
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
        await pass(700);
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
        await pass(700);

        expect(h.reports).toEqual(["rpc.typing_new_thing"]);
    });

    // The route could not read what this client sent: the client and the contract have drifted,
    // which no words the person might be shown make any less worth telling.
    test("a request the route could not read is reported, with the route's sentence or without", async () => {
        for (const text of ["Chat could not understand that request.", undefined]) {
            const h = build();
            h.answers.push(refused("rpc.bad_request", 400, text));

            h.presence.typed(group, null, "a");
            await pass(700);

            expect(h.reports).toEqual(["rpc.bad_request"]);
        }
    });
});

describe("the others' signals", () => {
    test("a signal shows its typist in its place at once, from either topic, and typing starting is recorded once for the place", () => {
        const h = build();

        h.presence.heard("typing", signal(anna), group);
        h.presence.heard("typing", signal(sam));
        h.presence.heard("typing", signal(anna), group);

        expect(h.presence.state.typists[group]).toEqual([anna, sam]);
        expect(h.records).toEqual([`started ${group}`]);
        expect(jest.getTimerCount()).toBe(1);
    });

    test("only the typing event is the presence's", () => {
        const h = build();

        expect(h.presence.owns("typing")).toBe(true);
        expect(h.presence.owns("message.created")).toBe(false);
    });

    test("a typist shows until the expiry and goes at it, recorded as expired, and a fresh signal holds them on", () => {
        const h = build();
        h.presence.heard("typing", signal(anna), group);

        jest.advanceTimersByTime(5999);
        expect(h.presence.state.typists[group]).toEqual([anna]);
        jest.advanceTimersByTime(1);
        expect(h.presence.state.typists[group]).toBeUndefined();
        expect(h.records).toEqual([`started ${group}`, `ended ${group} expired`]);

        h.presence.heard("typing", signal(anna), group);
        jest.advanceTimersByTime(5000);
        h.presence.heard("typing", signal(anna), group);
        jest.advanceTimersByTime(5999);
        expect(h.presence.state.typists[group]).toEqual([anna]);
        jest.advanceTimersByTime(1);
        expect(h.presence.state.typists[group]).toBeUndefined();
    });

    // The typist paces by their own token, which can name a longer interval than the viewer's
    // while the platform's cap is moving; the typing lasts by their pace, with the viewer's own
    // margin of expiry over interval (6,000 over 4,000 here).
    test("a signal naming its typist's interval keeps the typing for that interval with the expiry's margin", () => {
        const h = build();
        h.presence.heard("typing", signal(anna, { interval_ms: 8000 }), group);

        jest.advanceTimersByTime(11999);
        expect(h.presence.state.typists[group]).toEqual([anna]);
        jest.advanceTimersByTime(1);
        expect(h.presence.state.typists[group]).toBeUndefined();
    });

    test("a signal naming no interval, or one that is not a positive number, keeps the typing for the viewer's own expiry", () => {
        const h = build();
        h.presence.heard("typing", signal(anna), group);
        h.presence.heard("typing", signal(sam, { interval_ms: "8000" }), group);
        h.presence.heard("typing", signal(lee, { interval_ms: 0 }), group);

        jest.advanceTimersByTime(5999);
        expect(h.presence.state.typists[group]).toEqual([anna, sam, lee]);
        jest.advanceTimersByTime(1);
        expect(h.presence.state.typists[group]).toBeUndefined();
    });

    test("however many typists are held, one timer waits for the soonest", () => {
        const h = build();

        h.presence.heard("typing", signal(anna), group);
        jest.advanceTimersByTime(1000);
        h.presence.heard("typing", signal(sam, { parent_id: 40 }), group);

        expect(jest.getTimerCount()).toBe(1);
        jest.advanceTimersByTime(5000);
        expect(h.presence.state.typists[group]).toBeUndefined();
        expect(h.presence.state.typists[quoteKey(group, 40)]).toEqual([sam]);
        expect(jest.getTimerCount()).toBe(1);
    });

    // A word typed and deleted never blinks: a typist drawn stays at least the minimum.
    test("a stop heard takes its typist once they have been shown for the minimum, recorded as stopped", () => {
        const h = build();
        h.presence.heard("typing", signal(anna), group);
        jest.advanceTimersByTime(200);

        h.presence.heard("typing", stop(anna), group);
        jest.advanceTimersByTime(799);
        expect(h.presence.state.typists[group]).toEqual([anna]);
        jest.advanceTimersByTime(1);
        expect(h.presence.state.typists[group]).toBeUndefined();
        expect(h.records).toEqual([`started ${group}`, `ended ${group} stopped`]);
    });

    test("a stop heard past the minimum takes its typist at once; one for someone not drawn draws nothing", () => {
        const h = build();
        h.presence.heard("typing", signal(anna), group);
        jest.advanceTimersByTime(2700);

        h.presence.heard("typing", stop(anna), group);
        h.presence.heard("typing", stop(sam), group);
        jest.advanceTimersByTime(0);

        expect(h.presence.state.typists[group]).toBeUndefined();
        expect(h.records).toEqual([`started ${group}`, `ended ${group} stopped`]);
    });

    test("typing heard again after a stop holds the typist on to a fresh expiry", () => {
        const h = build();
        h.presence.heard("typing", signal(anna), group);
        h.presence.heard("typing", stop(anna), group);
        jest.advanceTimersByTime(500);
        h.presence.heard("typing", signal(anna), group);

        jest.advanceTimersByTime(5999);
        expect(h.presence.state.typists[group]).toEqual([anna]);
        jest.advanceTimersByTime(1);
        expect(h.presence.state.typists[group]).toBeUndefined();
        expect(h.records).toEqual([`started ${group}`, `ended ${group} expired`]);
    });

    // Her message is the end of typing everywhere: it ends her at once, within the minimum too, and
    // when she was the last typist the end names it, so the view grows the bubble into it.
    test("a message by the last typist in its place ends her at once as sent, naming it; one by a typist with others still typing records nothing", () => {
        const h = build();
        h.presence.heard("typing", signal(anna), group);
        h.presence.heard("typing", signal(sam), group);
        h.presence.heard("typing", signal(anna, { parent_id: 40 }), group);

        h.arrive(11, anna);
        expect(h.presence.state.typists[group]).toEqual([sam]);
        expect(h.presence.state.typists[quoteKey(group, 40)]).toEqual([anna]);
        expect(h.records).toEqual([`started ${group}`, `started ${quoteKey(group, 40)}`]);

        jest.advanceTimersByTime(100);
        h.arrive(12, sam);
        expect(h.presence.state.typists[group]).toBeUndefined();
        expect(h.records).toEqual([`started ${group}`, `started ${quoteKey(group, 40)}`, `ended ${group} sent 12`]);
    });

    test("a message arriving in a place no one types in records nothing", () => {
        const h = build();

        h.arrive(11, anna);

        expect(h.records).toEqual([]);
    });

    test("a signal that follows an older message than one of theirs already held is dropped, as it was sent before that message", () => {
        const h = build();
        h.arrive(11, anna);

        h.presence.heard("typing", signal(anna, { after: 10 }), group);
        expect(h.presence.state.typists[group]).toBeUndefined();

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

        expect(h.presence.state.typists[group]).toBeUndefined();
    });

    test("ignored: a conversation topic's signal naming another, the person's own in any case, someone blocked, and a signal with no one", () => {
        const h = build();
        h.blocked.add(lee);

        h.presence.heard("typing", signal(sam), otherGroup);
        h.presence.heard("typing", signal(me.toUpperCase()), group);
        h.presence.heard("typing", signal(me));
        h.presence.heard("typing", signal(lee), group);
        h.presence.heard("typing", { channel_id: group, parent_id: null, id: "x" }, group);
        h.presence.heard("typing", null, group);

        expect(h.presence.state.typists).toEqual({});
        expect(h.records).toEqual([]);
    });

    test("a reply typed in a thread shows only under the thread's place", () => {
        const h = build();

        h.presence.heard("typing", signal(anna, { parent_id: 40 }), group);

        expect(h.presence.state.typists[quoteKey(group, 40)]).toEqual([anna]);
        expect(h.presence.state.typists[group]).toBeUndefined();
        expect(h.presence.typingIn(group)).toBeNull();
        expect(h.presence.typingIn(quoteKey(group, 40))).toMatchObject({ names: "Anna Reyes", sentence: "Anna is typing…" });
    });

    // A typist held when the person switched away is still true for the rest of her expiry, so the
    // room drawn again shows her from its first frame; a rejoin keeps her too, since a missed signal
    // costs at most one expiry and her message brought by the catch-up ends her.
    test("typists are kept per place whatever room is open, and a signal for a room not open is kept under its place", () => {
        const h = build();
        h.presence.heard("typing", signal(anna), group);

        h.state.activeChannelId = otherGroup;
        h.presence.heard("typing", signal(sam, { channel_id: otherGroup }));
        expect(h.presence.state.typists[group]).toEqual([anna]);
        expect(h.presence.state.typists[otherGroup]).toEqual([sam]);

        h.state.activeChannelId = group;
        expect(h.presence.typingIn(group)?.names).toBe("Anna Reyes");
        jest.advanceTimersByTime(6000);
        expect(h.presence.state.typists).toEqual({});
    });

    // After a reconnect or a reopen her message comes back in a page, never heard live; a signal
    // she sent before it, arriving late, would otherwise draw her typing under her own message.
    test("a signal sent before a message of theirs that a page brought, not a live copy, is dropped", () => {
        const h = build();
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

    // A page or a catch-up that brings her newer message ends her typing as what is: the bubble
    // leaves with no motion of its own, since nothing happened on screen.
    test("a holder now holding a newer message of a typist's ends her as overtaken; an older one, or one in another place, keeps her", () => {
        const h = build();
        h.presence.heard("typing", signal(anna, { after: 10 }), group);
        h.presence.heard("typing", signal(sam, { after: 10 }), group);

        h.presence.heldThrough(group, null, anna.toUpperCase(), 10);
        h.presence.heldThrough(group, 40, anna, 11);
        h.presence.heldThrough(otherGroup, null, null, 11);
        expect(h.presence.state.typists[group]).toEqual([anna, sam]);

        h.presence.heldThrough(group, null, anna.toUpperCase(), 11);
        h.presence.heldThrough(group, null, sam, 12);
        expect(h.presence.state.typists[group]).toBeUndefined();
        expect(h.records).toEqual([`started ${group}`, `ended ${group} overtaken`]);
    });

    test("someone just blocked leaves at once, recorded as blocked once the place has no one", () => {
        const h = build();
        h.presence.heard("typing", signal(anna), group);
        h.presence.heard("typing", signal(sam), group);

        h.blocked.add(anna);
        h.presence.remask();
        expect(h.presence.state.typists[group]).toEqual([sam]);
        expect(h.records).toEqual([`started ${group}`]);

        h.blocked.add(sam);
        h.presence.remask();
        expect(h.presence.state.typists[group]).toBeUndefined();
        expect(h.records).toEqual([`started ${group}`, `ended ${group} blocked`]);
    });
});

// The newest page answers who is typing from the platform's own record of signals passed, so a
// conversation opened while someone types shows them with no Realtime event. What is answered is
// drawn as what is: nothing moves for it, and nothing is recorded.
describe("the typing a page states", () => {
    const stated = (alias: string, extra: Record<string, unknown> = {}): Record<string, unknown> => ({ person_alias_guid: alias, after: 10, age_ms: 2000, ...extra });

    test("each typist answered is drawn with what is left of their expiry, and nothing is recorded", () => {
        const h = build();

        h.presence.stated(group, null, [stated(anna), stated(sam, { parent_id: 40, interval_ms: 8000, age_ms: 1000 })], Date.now());

        expect(h.presence.state.typists[group]).toEqual([anna, sam]);
        expect(h.records).toEqual([]);
        jest.advanceTimersByTime(3999);
        expect(h.presence.state.typists[group]).toEqual([anna, sam]);
        jest.advanceTimersByTime(1);
        expect(h.presence.state.typists[group]).toEqual([sam]);
        jest.advanceTimersByTime(7000);
        expect(h.presence.state.typists[group]).toBeUndefined();
    });

    test("a typist answered for a thread is drawn under the thread's place", () => {
        const h = build();

        h.presence.stated(group, 40, [stated(anna)], Date.now());

        expect(h.presence.state.typists[quoteKey(group, 40)]).toEqual([anna]);
        expect(h.presence.state.typists[group]).toBeUndefined();
    });

    test("left out: one the page's own messages overtook, the person, someone blocked, one already expired, and an answer that is not a list", () => {
        const h = build();
        h.blocked.add(lee);
        h.hold(11, anna);

        h.presence.stated(group, null, [stated(anna), stated(me), stated(lee), stated(sam, { age_ms: 6000 }), { after: 10 }, null], Date.now());
        h.presence.stated(group, null, { typing: [stated(sam)] }, Date.now());
        h.presence.stated(group, null, undefined, Date.now());

        expect(h.presence.state.typists).toEqual({});
    });

    // An older platform leaves a null out of its answer rather than writing it, so a typist whose
    // signal followed no message is read the same whether her after is null or missing.
    test("a typist's after, null or left out, is read alike: drawn while no message of hers is held, left out once one is", () => {
        const h = build();
        h.presence.stated(group, null, [stated(anna, { after: null }), { person_alias_guid: sam, age_ms: 2000 }], Date.now());
        expect(h.presence.state.typists[group]).toEqual([anna, sam]);

        const later = build();
        later.hold(11, anna);
        later.hold(12, sam);
        later.presence.stated(group, null, [stated(anna, { after: null }), { person_alias_guid: sam, age_ms: 2000 }], Date.now());
        expect(later.presence.state.typists).toEqual({});
    });

    test("the answer is what is: a typist held in its place whom it leaves out ends as overtaken, unless heard after the read was asked", () => {
        const h = build();
        h.presence.heard("typing", signal(anna));
        h.presence.heard("typing", signal(sam));
        jest.advanceTimersByTime(1000);
        const askedAt = Date.now();
        jest.advanceTimersByTime(50);
        // a keep-alive heard while the read was out
        h.presence.heard("typing", signal(sam));

        h.presence.stated(group, null, [], askedAt);

        expect(h.presence.state.typists[group]).toEqual([sam]);
        expect(h.records).toEqual([`started ${group}`]);

        // a read asked after her last signal was heard
        jest.advanceTimersByTime(1);
        h.presence.stated(group, null, [], Date.now());
        expect(h.presence.state.typists[group]).toBeUndefined();
        expect(h.records).toEqual([`started ${group}`, `ended ${group} overtaken`]);
    });

    // A word typed and deleted never blinks, whoever says it is over: an answer leaving out someone
    // drawn under the minimum ends her only once she has been shown for it. That end lands on its
    // own, long after the page, so it is a stop learned late and goes as a stop does, sliding out.
    test("a typist the answer leaves out before her minimum show is drawn until it, then ends as stopped", () => {
        const h = build();
        h.presence.heard("typing", signal(anna));
        jest.advanceTimersByTime(300);

        h.presence.stated(group, null, [], Date.now() + 1);
        expect(h.presence.state.typists[group]).toEqual([anna]);

        jest.advanceTimersByTime(699);
        expect(h.presence.state.typists[group]).toEqual([anna]);
        jest.advanceTimersByTime(1);
        expect(h.presence.state.typists[group]).toBeUndefined();
        expect(h.records).toEqual([`started ${group}`, `ended ${group} stopped`]);
    });

    test("held to her minimum by an answer that left her out, a later answer naming her keeps her drawn", () => {
        const h = build();
        h.presence.heard("typing", signal(anna));
        jest.advanceTimersByTime(300);
        h.presence.stated(group, null, [], Date.now() + 1);
        jest.advanceTimersByTime(100);

        h.presence.stated(group, null, [stated(anna, { age_ms: 0 })], Date.now());
        jest.advanceTimersByTime(1000);

        expect(h.presence.state.typists[group]).toEqual([anna]);
        expect(h.records).toEqual([`started ${group}`]);
    });

    test("an answer leaving out a typist whose stop was heard keeps the stop: a later answer naming her does not keep her", () => {
        const h = build();
        h.presence.heard("typing", signal(anna));
        jest.advanceTimersByTime(200);
        h.presence.heard("typing", stop(anna));
        jest.advanceTimersByTime(100);

        h.presence.stated(group, null, [], Date.now() + 1);
        h.presence.stated(group, null, [stated(anna, { age_ms: 0 })], Date.now() + 1);
        jest.advanceTimersByTime(700);

        expect(h.presence.state.typists[group]).toBeUndefined();
        expect(h.records).toEqual([`started ${group}`, `ended ${group} stopped`]);
    });

    test("an answer with no list of typists removes no one, and an answer for one place leaves another's typists", () => {
        const h = build();
        h.presence.heard("typing", signal(anna));
        h.presence.heard("typing", signal(sam, { parent_id: 40 }));
        jest.advanceTimersByTime(100);

        h.presence.stated(group, null, undefined, Date.now());
        h.presence.stated(group, 40, [stated(sam)], Date.now());

        expect(h.presence.state.typists[group]).toEqual([anna]);
        expect(h.presence.state.typists[quoteKey(group, 40)]).toEqual([sam]);
    });

    test("a typist already drawn is stated again with nothing recorded, keeping when she was first shown", () => {
        const h = build();
        h.presence.heard("typing", signal(anna), group);
        jest.advanceTimersByTime(500);

        h.presence.stated(group, null, [stated(anna, { age_ms: 5800 })], Date.now());
        h.presence.heard("typing", stop(anna), group);

        expect(h.records).toEqual([`started ${group}`]);
        jest.advanceTimersByTime(499);
        expect(h.presence.state.typists[group]).toEqual([anna]);
        jest.advanceTimersByTime(1);
        expect(h.presence.state.typists[group]).toBeUndefined();
    });

    test("one nearly expired is still drawn for the minimum, never a blink", () => {
        const h = build();

        h.presence.stated(group, null, [stated(anna, { age_ms: 5800 })], Date.now());

        jest.advanceTimersByTime(999);
        expect(h.presence.state.typists[group]).toEqual([anna]);
        jest.advanceTimersByTime(1);
        expect(h.presence.state.typists[group]).toBeUndefined();
        expect(h.records).toEqual([`ended ${group} expired`]);
    });

    test("her message heard live ends a stated typist as sent, at once", () => {
        const h = build();
        h.presence.stated(group, null, [stated(anna)], Date.now());

        h.arrive(11, anna);

        expect(h.presence.state.typists[group]).toBeUndefined();
        expect(h.records).toEqual([`ended ${group} sent 11`]);
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

    // The bubble is drawn as her next message would be, so one typist's name line reads the name
    // her message's name line will, and a group hand-off never swaps one name for another.
    test("one typist's name line reads her name as her message draws it; the sentence keeps her nick name", () => {
        expect(typingWords([person(anna, "Anna", "Park")])).toEqual({ names: "Anna Park", sentence: "Anna is typing…" });
        expect(typingWords([person(anna, "Anna", "Park"), person(sam, "Sam", "Kim")])).toEqual({ names: "Anna and Sam", sentence: "Anna and Sam are typing…" });
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
            names: "Sam Okafor",
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

describe("typing switched off", () => {
    test("a signal draws no one, and anyone drawn before ends as cleared", () => {
        const h = build();
        h.presence.heard("typing", signal(anna));
        expect(h.presence.typingIn(group)).not.toBeNull();

        h.settings.value = readSettings({ ...current, flags: { typing: false } });
        h.presence.heard("typing", signal(sam));

        expect(h.presence.typingIn(group)).toBeNull();
        expect(h.presence.state.typists[group]).toBeUndefined();
        expect(h.records).toEqual([`started ${group}`, `ended ${group} cleared`]);

        // switched on again, no one held from before comes back
        h.settings.value = readSettings({ ...current, flags: { typing: true } });
        expect(h.presence.typingIn(group)).toBeNull();
    });

    test("a page's answer while it is off holds no one, so switching it on again within an expiry draws no one", () => {
        const h = build();
        h.settings.value = readSettings({ ...current, flags: { typing: false } });

        h.presence.stated(group, null, [{ person_alias_guid: anna, after: 10, age_ms: 0 }], Date.now());
        h.settings.value = readSettings({ ...current, flags: { typing: true } });

        expect(h.presence.typingIn(group)).toBeNull();
        expect(h.presence.state.typists[group]).toBeUndefined();
    });
});

describe("what a place last drew", () => {
    test("goes with its last typist, so typing starting again there is a new value", () => {
        const h = build();
        h.presence.heard("typing", signal(anna));
        const first = h.presence.typingIn(group);
        jest.advanceTimersByTime(6000);
        expect(h.presence.typingIn(group)).toBeNull();

        h.presence.heard("typing", signal(anna));

        expect(h.presence.typingIn(group)).not.toBe(first);
    });
});

describe("a stopped presence", () => {
    test("forgets every typist, turn and timer, sends nothing, records nothing, and writes nothing an answer brings after", async () => {
        const h = build();
        h.held.value = [];
        h.presence.typed(group, null, "a");
        await pass(700);
        h.presence.typed(group, null, "ab");
        h.presence.heard("typing", signal(anna), group);

        h.stopped.value = true;
        h.presence.stop();
        expect(h.presence.state.typists).toEqual({});
        expect(jest.getTimerCount()).toBe(0);

        h.held.value[0].resolve(refused("rpc.typing_new_thing"));
        await settle();
        expect(h.reports).toEqual([]);

        jest.advanceTimersByTime(10000);
        h.presence.typed(group, null, "abc");
        h.presence.typed(group, null, "");
        h.presence.heard("typing", signal(sam), group);
        h.presence.stated(group, null, [{ person_alias_guid: lee, after: 10, age_ms: 0 }], Date.now());
        h.arrive(11, sam);
        h.presence.caughtUp();
        await pass(10000);
        expect(h.calls).toHaveLength(1);
        expect(h.presence.state.typists).toEqual({});
        expect(h.records).toEqual([`started ${group}`]);
        expect(jest.getTimerCount()).toBe(0);
    });
});
