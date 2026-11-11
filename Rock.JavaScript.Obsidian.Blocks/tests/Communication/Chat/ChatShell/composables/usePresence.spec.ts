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
//
// And others' online status: one store by person, which keeps each person's newest decision
// however late the answer carrying it lands, draws from it by the server's clock without ever
// writing it again, moves a person's revision only when what is drawn changes, and answers
// nothing for someone blocked, for the person's own carried copy, or with the switch off.
import { computed, defineComponent, h as render, reactive, shallowRef, watchEffect } from "vue";
import { mount } from "@vue/test-utils";
import { createPresence, Presence, PresenceDependencies, typingWords } from "../../../../../src/Communication/Chat/ChatShell/composables/usePresence.partial";
import { presenceStatusKey, useFaceStatus } from "../../../../../src/Communication/Chat/ChatShell/composables/usePresenceStatus.partial";
import { quoteKey } from "../../../../../src/Communication/Chat/ChatShell/composables/useMessageActions.partial";
import { readSettings } from "../../../../../src/Communication/Chat/ChatShell/composables/useSession.partial";
import { classifyPlatformError } from "../../../../../src/Communication/Chat/ChatShell/errors.partial";
import { CallResult } from "../../../../../src/Communication/Chat/ChatShell/platformCall.partial";
import { ChatError, ChatPerson, SidebarRow } from "../../../../../src/Communication/Chat/ChatShell/types.partial";
import { asOf, away, busy, exchangeAnswer, heldNothingMs, nothing, online, personWithoutStatus, pgTime, readerPerson, rowLifeMs, serverNow, serverNowSeconds } from "../statusShapes";

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

/**
 * The settings a platform with the stop and the first signal's wait answers, typing on, at a pace of
 * its own: these cells time the pace a platform sends, not the client's fallback for one it leaves out.
 */
const current = { flags: { typing: true }, routes: { region: "us-east-1" }, limits: { typing_first_after_ms: 700, typing_interval_ms: 4000, typing_expiry_ms: 6000 } };

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
    // Her own settings as the sidebar last read them: her Busy and her Appear offline, read, as
    // they are on a page once the sidebar has answered.
    const prefs = reactive({ state: { me: { busy_until: null as string | null, appear_offline: false }, isMeRead: true } });
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
    // The page the status watches: each listener by event, with the options it was added with. As
    // a browser does, a removal finds a listener only by its capture, so one removed without the
    // capture it was added with stays.
    const listeners: Array<{ on: "window" | "document", type: string, listener: (event: unknown) => void, options: unknown }> = [];
    const isCapture = (options: unknown): boolean => typeof options === "boolean" ? options : (options as { capture?: unknown } | undefined)?.capture === true;
    const page = { visibilityState: "visible" };
    const targets = {
        document: {
            get visibilityState(): string {
                return page.visibilityState;
            },
            addEventListener: (type: string, listener: () => void) => listeners.push({ on: "document", type, listener, options: undefined }),
            removeEventListener: (type: string, listener: () => void) => {
                listeners.splice(listeners.findIndex(l => l.on === "document" && l.type === type && l.listener === listener) >>> 0, 1);
            }
        },
        window: {
            addEventListener: (type: string, listener: (event: unknown) => void, options?: unknown) => listeners.push({ on: "window", type, listener, options }),
            removeEventListener: (type: string, listener: (event: unknown) => void, options?: unknown) => {
                const index = listeners.findIndex(l => l.on === "window" && l.type === type && l.listener === listener && isCapture(l.options) === isCapture(options));
                if (index >= 0) {
                    listeners.splice(index, 1);
                }
            }
        }
    };
    // Every goodbye sent, by the id it named.
    const leaves: string[] = [];
    // The session as the status reads it: when the next refresh is due, whether one is out, and every refresh asked.
    const session = {
        next: Date.now() + 210_000 as number | null,
        out: null as string | null,
        life: 300,
        asked: [] as string[]
    };

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
        isStopped: () => stopped.value,
        prefs,
        targets,
        leave: clientId => leaves.push(clientId),
        session: {
            refresh: async reason => {
                session.asked.push(reason);
                return true;
            },
            nextRefreshAt: () => session.next,
            tokenLifeSeconds: () => session.life,
            inFlight: () => session.out as ReturnType<PresenceDependencies["session"]["inFlight"]>
        }
    };

    const presence: Presence = createPresence(dependencies);

    /** Fires a page event on whichever of the window or the document listens for it. */
    function fire(type: string, event: unknown = {}): void {
        for (const listener of listeners.filter(l => l.type === type)) {
            listener.listener(event);
        }
    }

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

    return { presence, calls, held, answers, rejections, settings, token, state, rows, people, blocked, reports, stopped, newest, behind, offScreen, records, hold, arrive, listeners, page, leaves, session, fire, me: prefs.state.me, prefs: prefs.state };
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
            body: { p_channel_id: group, p_parent_id: null, p_after: 10, p_client: h.presence.status.clientId() },
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
        expect(h.calls[0].body).toEqual({ p_channel_id: group, p_parent_id: null, p_client: h.presence.status.clientId() });
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

        expect(h.calls.map(c => c.body)).toEqual([{ p_channel_id: group, p_parent_id: null, p_after: 10, p_client: h.presence.status.clientId() }]);
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

        const client = h.presence.status.clientId();
        expect(h.calls.map(c => c.body)).toEqual([
            { p_channel_id: group, p_parent_id: null, p_after: 10, p_client: client },
            { p_channel_id: group, p_parent_id: null, p_after: 10, p_stopped: true, p_client: client }
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

        const client = h.presence.status.clientId();
        expect(h.calls.map(c => c.body)).toEqual([
            { p_channel_id: group, p_parent_id: null, p_after: 10, p_client: client },
            { p_channel_id: group, p_parent_id: 40, p_after: 44, p_client: client }
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

// #region Others' status

/** A presence with status on, on a page whose clock reads the fixtures' server now. */
function buildStatus(rawSettings: Record<string, unknown> = {}) {
    jest.setSystemTime(serverNow);
    const built = build({ flags: { typing: true, online: true }, ...rawSettings });
    return { ...built, status: built.presence.status };
}

/** What a face draws for someone, held as a view holds it: read again only when that person's revision moves. */
function face(status: Presence["status"], guid: string) {
    return computed(() => status.statusOf(guid));
}

describe("others' status, the newest decision", () => {
    test("an answer decided before a carried status changes nothing when it lands after it, and neither does a carried status decided before an answer", () => {
        const h = buildStatus();

        h.status.carried(anna, online(2000));
        h.status.exchanged([anna], exchangeAnswer({ [anna]: away(1000) }));
        expect(h.status.statusOf(anna)?.state).toBe("online");

        h.status.told([readerPerson(sam, busy(null, 5000))]);
        h.status.carried(sam, online(4000));
        expect(h.status.statusOf(sam)?.state).toBe("busy");
    });

    test("two decisions 300 microseconds apart inside one millisecond, landing in reverse, keep the later", () => {
        const h = buildStatus();

        h.status.told([readerPerson(anna, away(0, rowLifeMs, 700))]);
        h.status.carried(anna, online(0, rowLifeMs, 400));

        expect(h.status.statusOf(anna)?.state).toBe("away");
    });

    test("two answers of one decision time keep the one written later", () => {
        const h = buildStatus();

        h.status.told([readerPerson(anna, online(1000))]);
        h.status.carried(anna, away(1000));

        expect(h.status.statusOf(anna)?.state).toBe("away");
    });

    test("the decision time alone says she shows nothing from then, and an older one changes nothing", () => {
        const h = buildStatus();
        h.status.told([readerPerson(anna, online(0))]);

        h.status.told([readerPerson(anna, nothing(-1000))]);
        expect(h.status.statusOf(anna)?.state).toBe("online");

        h.status.told([readerPerson(anna, nothing(1000))]);
        expect(h.status.statusOf(anna)).toBeNull();
    });

    test("a person a reader sends with no status key tells nothing, and neither does a status the client cannot read", () => {
        const h = buildStatus();
        h.status.told([readerPerson(anna, online(0))]);

        h.status.told([personWithoutStatus(anna), null, "x"]);
        h.status.told([readerPerson(anna, { state: "online", until: pgTime(1000), as_of: String(asOf(5000)) })]);
        h.status.told([readerPerson(anna, { state: "dancing", until: pgTime(1000), as_of: asOf(5000) })]);
        h.status.told([readerPerson(anna, { state: "online", as_of: asOf(5000) })]);
        h.status.carried(anna, undefined);

        expect(h.status.statusOf(anna)?.state).toBe("online");
    });

    test("an exchange writes each person it asked about that its map names, and nothing for anyone else", () => {
        const h = buildStatus();
        h.status.told([readerPerson(sam, online(0))]);

        h.status.exchanged([anna, sam], exchangeAnswer({ [anna]: away(1000), [lee]: online(1000) }));

        expect(h.status.statusOf(anna)?.state).toBe("away");
        expect(h.status.statusOf(sam)?.state).toBe("online");
        expect(h.status.statusOf(lee)).toBeNull();
    });

    test("an exchange whose read did not answer in time, or that failed, changes nothing", () => {
        const h = buildStatus();
        h.status.told([readerPerson(anna, online(0))]);

        h.status.exchanged([anna], exchangeAnswer());
        h.status.exchanged([anna], null);

        expect(h.status.statusOf(anna)?.state).toBe("online");
    });

    test("a status carried for the person's own id writes nothing, in any case, though her own answer does", () => {
        const h = buildStatus();
        // her own answer says nothing of her, and with no idle line her input draws nothing, so only a write could draw her
        h.status.told([readerPerson(me, nothing(0))]);

        h.status.carried(me, online(1000));
        h.status.carried(me.toUpperCase(), online(1000));
        expect(h.status.statusOf(me)).toBeNull();

        h.status.told([readerPerson(me, online(2000))]);
        expect(h.status.statusOf(me)?.state).toBe("online");
    });

    test("keys are read in lower case wherever an id comes from", () => {
        const h = buildStatus();

        h.status.carried(anna.toUpperCase(), online(0));

        expect(h.status.statusOf(anna)?.state).toBe("online");
        expect(h.status.statusOf(anna.toUpperCase())?.state).toBe("online");
    });

    test("draws the words a label and a tip read", () => {
        const h = buildStatus();
        h.status.told([readerPerson(anna, online(0)), readerPerson(sam, busy(null, 0))]);

        expect(h.status.statusOf(anna)).toEqual({ state: "online", label: ", online", tip: "Online", busyUntil: null, revision: expect.any(Number) });
        expect(h.status.statusOf(sam)).toEqual({ state: "busy", label: ", busy", tip: "Busy", busyUntil: null, revision: expect.any(Number) });
    });

    test("carries Busy's end as a server time, so the note above the box words it from the store and nothing else", () => {
        const h = buildStatus();
        h.status.told([readerPerson(anna, busy(3_600_000, 0)), readerPerson(sam, busy(60_000, 0))]);

        expect(h.status.statusOf(anna)?.busyUntil).toBe(serverNow + 3_600_000);
        jest.advanceTimersByTime(60_000);
        expect(h.status.statusOf(sam)).toEqual(expect.objectContaining({ state: "away", busyUntil: null }));
    });
});

describe("others' status with the switch off", () => {
    test("nothing is written while it is off, and turning it off empties the store, so nothing from before draws when it comes back", () => {
        const h = buildStatus({ flags: { typing: true } });
        const on = readSettings({ ...current, flags: { typing: true, online: true } });
        const off = readSettings({ ...current, flags: { typing: true } });

        h.status.told([readerPerson(anna, online(0))]);
        h.settings.value = on;
        expect(h.status.statusOf(anna)).toBeNull();

        h.status.told([readerPerson(anna, online(0))]);
        expect(h.status.statusOf(anna)?.state).toBe("online");

        h.settings.value = off;
        expect(h.status.statusOf(anna)).toBeNull();
        h.settings.value = on;
        expect(h.status.statusOf(anna)).toBeNull();
    });
});

describe("others' status drawn over time", () => {
    test("Busy draws Away past its end and nothing past its row's end, by a server clock 30 s ahead of the page's, with no request", () => {
        const h = buildStatus();
        h.status.clockFrom(serverNowSeconds + 30);
        h.status.told([readerPerson(anna, busy(60_000, 0, 120_000))]);
        const drawn = face(h.status, anna);
        expect(drawn.value?.state).toBe("busy");

        jest.advanceTimersByTime(29_999);
        expect(drawn.value?.state).toBe("busy");
        jest.advanceTimersByTime(1);
        expect(drawn.value?.state).toBe("away");

        jest.advanceTimersByTime(59_999);
        expect(drawn.value?.state).toBe("away");
        jest.advanceTimersByTime(1);
        expect(drawn.value).toBeNull();
        expect(h.calls).toEqual([]);
    });

    test("what is drawn is worked out from the entry and the clock, never written over it, so a newer token's clock draws the entry again", () => {
        const h = buildStatus();
        h.status.told([readerPerson(anna, busy(60_000, 0))]);
        const drawn = face(h.status, anna);

        jest.advanceTimersByTime(60_000);
        expect(drawn.value?.state).toBe("away");

        // a token minted now says the server is 30 s behind the page, so her Busy has 30 s left
        h.status.clockFrom(Date.now() / 1000 - 30);
        expect(drawn.value?.state).toBe("busy");
    });

    test("reads a row's end as Postgres writes it, to the microsecond with its offset", () => {
        const h = buildStatus();
        const status = online(0, 10_000);
        expect(status.until).toMatch(/^2026-11-10T15:00:10\.000123\+00:00$/);
        h.status.told([readerPerson(anna, status)]);
        const drawn = face(h.status, anna);

        jest.advanceTimersByTime(9_999);
        expect(drawn.value?.state).toBe("online");
        jest.advanceTimersByTime(1);
        expect(drawn.value).toBeNull();
    });

    test("a person's revision moves only when what is drawn changes: a later end of the same state moves nothing", () => {
        const h = buildStatus();
        h.status.told([readerPerson(anna, online(0))]);
        let reads = 0;
        const drawn = computed(() => {
            reads++;
            return h.status.statusOf(anna);
        });
        const first = drawn.value!.revision;

        h.status.told([readerPerson(anna, online(1000, rowLifeMs + 1000))]);
        expect(drawn.value?.revision).toBe(first);
        expect(reads).toBe(1);

        h.status.carried(anna, away(2000));
        expect(drawn.value?.state).toBe("away");
        expect(drawn.value!.revision).toBeGreaterThan(first);
        expect(reads).toBe(2);
    });

    test("Busy's end words follow the viewer's day: told as tomorrow at 11:59 PM, a face held across midnight reads the time alone", () => {
        const h = buildStatus();
        const lateEvening = new Date(2026, 10, 10, 23, 59).getTime();
        jest.setSystemTime(lateEvening);
        // the server 30 s ahead of the page: the words turn at the viewer's midnight, by the page's clock
        h.status.clockFrom(lateEvening / 1000 + 30);
        const at = lateEvening + 30_000 - serverNow;
        const timers = jest.spyOn(global, "setTimeout");
        h.status.told([readerPerson(anna, busy(new Date(2026, 10, 11, 8, 0).getTime() - serverNow, at, at + rowLifeMs))]);
        const drawn = face(h.status, anna);
        const first = drawn.value!.revision;
        expect(drawn.value?.tip).toMatch(/^Busy until tomorrow, 8:00\sAM$/u);

        jest.advanceTimersByTime(59_999);
        expect(drawn.value!.revision).toBe(first);
        jest.advanceTimersByTime(1);

        expect(drawn.value?.tip).toMatch(/^Busy until 8:00\sAM$/u);
        expect(drawn.value?.label).toMatch(/^, busy until 8:00\sAM$/u);
        expect(drawn.value!.revision).toBeGreaterThan(first);
        expect(h.calls).toEqual([]);
        // one timer at the write, one when the midnight wake sets the next: a wake that came early would set one again and again
        expect(timers).toHaveBeenCalledTimes(2);
        timers.mockRestore();
    });

    test("an end two days on reads its day, then tomorrow, then the time alone, waking at each of the viewer's midnights", () => {
        const h = buildStatus();
        const evening = new Date(2026, 10, 10, 22, 0).getTime();
        jest.setSystemTime(evening);
        h.status.clockFrom(evening / 1000);
        const at = evening - serverNow;
        const end = new Date(2026, 10, 12, 8, 0).getTime() - serverNow;
        // a row that lasts past both midnights, so only the words can change
        h.status.told([readerPerson(anna, busy(end, at, end))]);
        const drawn = face(h.status, anna);
        expect(drawn.value?.tip).toMatch(/^Busy until Nov 12, 8:00\sAM$/u);
        const first = drawn.value!.revision;

        jest.advanceTimersByTime(7_200_000 - 1);
        expect(drawn.value!.revision).toBe(first);
        jest.advanceTimersByTime(1);
        expect(drawn.value?.tip).toMatch(/^Busy until tomorrow, 8:00\sAM$/u);

        jest.advanceTimersByTime(86_400_000);
        expect(drawn.value?.tip).toMatch(/^Busy until 8:00\sAM$/u);
        expect(drawn.value!.revision).toBeGreaterThan(first);
    });

    test("Busy's end moving is a change of what is drawn, since its words change", () => {
        const h = buildStatus();
        h.status.told([readerPerson(anna, busy(3_600_000, 0))]);
        const first = h.status.statusOf(anna)!.revision;

        h.status.told([readerPerson(anna, busy(7_200_000, 1000))]);

        expect(h.status.statusOf(anna)!.revision).toBeGreaterThan(first);
    });
});

describe("others' status and blocks", () => {
    test("someone blocked draws nothing at once, and a decision older than the block's never draws after the unblock", () => {
        const h = buildStatus();
        h.status.told([readerPerson(anna, online(0))]);
        const drawn = face(h.status, anna);
        expect(drawn.value?.state).toBe("online");

        h.blocked.add(anna);
        expect(h.status.statusOf(anna)).toBeNull();

        h.status.blockChanged([anna], asOf(1000));
        expect(drawn.value).toBeNull();
        h.status.carried(anna, online(500));
        h.blocked.delete(anna);
        expect(h.status.statusOf(anna)).toBeNull();

        h.status.told([readerPerson(anna, online(2000))]);
        expect(h.status.statusOf(anna)?.state).toBe("online");
    });

    test("a face stops drawing someone blocked though the block was decided before the status it holds, and draws that status again after the unblock", () => {
        const h = buildStatus();
        h.status.told([readerPerson(anna, online(2000))]);
        const drawn = face(h.status, anna);
        expect(drawn.value?.state).toBe("online");

        h.blocked.add(anna);
        h.status.blockChanged([anna], asOf(1000));
        expect(drawn.value).toBeNull();

        h.blocked.delete(anna);
        h.status.blockChanged([anna], asOf(1500));
        expect(drawn.value?.state).toBe("online");
    });

    test("a status carried for someone blocked is never written, so nothing of it draws after the unblock", () => {
        const h = buildStatus();
        h.blocked.add(anna);

        h.status.carried(anna, online(5000));
        h.blocked.delete(anna);

        expect(h.status.statusOf(anna)).toBeNull();
    });

    test("a block learned with no decision time clears her at her entry's own", () => {
        const h = buildStatus();
        h.status.told([readerPerson(anna, online(1000))]);

        h.status.blockChanged([anna.toUpperCase()], null);
        h.status.carried(anna, online(500));

        expect(h.status.statusOf(anna)).toBeNull();
    });
});

describe("the people watched", () => {
    test("the person first, then every face's people by rank and in each face's order, each once and in lower case", () => {
        const h = buildStatus();
        h.status.register(() => [sam.toUpperCase(), lee], 3);
        h.status.register(() => [anna], 1);
        h.status.register(() => [lee, anna, me], 2);

        expect(h.status.watched()).toEqual([me, anna, lee, sam]);
    });

    test("a face that unregisters leaves the list", () => {
        const h = buildStatus();
        const unregister = h.status.register(() => [anna], 1);

        unregister();

        expect(h.status.watched()).toEqual([me]);
    });

    test("is cut at 200, the platform's cap, keeping the higher ranks", () => {
        const h = buildStatus();
        const many = Array.from({ length: 250 }, (_, index) => `b${String(index).padStart(7, "0")}-0000-4000-8000-000000000000`);
        h.status.register(() => many, 3);
        h.status.register(() => [anna], 1);

        const watched = h.status.watched();

        expect(watched).toHaveLength(200);
        expect(watched.slice(0, 3)).toEqual([me, anna, many[0]]);
    });
});

describe("a face's status", () => {
    const statusFace = defineComponent({
        props: { people: { type: Array, required: true } },
        setup(props) {
            const statusOf = useFaceStatus(() => props.people as string[], 1);
            return () => render("span", statusOf((props.people as string[])[0])?.state ?? "none");
        }
    });

    test("registers its people while mounted and answers from the store", () => {
        const h = buildStatus();
        h.status.told([readerPerson(anna, online(0))]);

        const wrapper = mount(statusFace, { props: { people: [anna] }, global: { provide: { [presenceStatusKey as symbol]: h.status } } });
        expect(h.status.watched()).toEqual([me, anna]);
        expect(wrapper.text()).toBe("online");

        wrapper.unmount();
        expect(h.status.watched()).toEqual([me]);
    });

    test("with no provider, as a part drawn without the shell, registers nothing and draws no status", () => {
        const h = buildStatus();
        h.status.told([readerPerson(anna, online(0))]);

        const wrapper = mount(statusFace, { props: { people: [anna] } });

        expect(wrapper.text()).toBe("none");
        expect(h.status.watched()).toEqual([me]);
    });
});

describe("a stopped status", () => {
    test("its deadline bumps nothing and a late write changes nothing", () => {
        const h = buildStatus();
        h.status.told([readerPerson(anna, online(0, 10_000))]);
        const drawn = face(h.status, anna);
        expect(drawn.value?.state).toBe("online");

        h.stopped.value = true;
        h.presence.stop();
        expect(jest.getTimerCount()).toBe(0);

        h.status.told([readerPerson(sam, online(0))]);
        h.status.carried(lee, online(0));
        h.status.exchanged([anna], exchangeAnswer({ [anna]: away(1000) }));
        h.status.blockChanged([anna], asOf(2000));

        expect(drawn.value?.state).toBe("online");
        expect(h.status.statusOf(sam)).toBeNull();
        expect(h.status.statusOf(lee)).toBeNull();
        expect(jest.getTimerCount()).toBe(0);
    });

    test("an exchange's answer writes nothing, a settle asks nothing, and the page's events send nothing more than the stop's one goodbye", () => {
        const h = buildStatus({ limits: { presence_idle_seconds: 600 } });
        const body = h.status.exchangeBody("start");
        h.status.exchanged([], exchangeAnswer());
        h.session.next = Date.now() + 700_000;
        h.page.visibilityState = "hidden";

        h.stopped.value = true;
        h.presence.stop();
        h.status.exchanged([anna], exchangeAnswer({ [anna]: online(0) }));
        h.status.settled("scheduled", true);
        h.fire("pagehide");

        expect(body).not.toBeNull();
        expect(h.status.statusOf(anna)).toBeNull();
        expect(h.session.asked).toEqual([]);
        expect(h.leaves).toEqual([body?.client_id]);
        expect(h.listeners).toEqual([]);
    });

    test("a stop says goodbye only for an id a body carried, and never once the latest answer said status is off", () => {
        const never = buildStatus();
        never.stopped.value = true;
        never.presence.stop();

        const off = buildStatus();
        off.status.exchangeBody("start");
        off.status.exchanged([], exchangeAnswer());
        off.settings.value = readSettings({ ...current, flags: { typing: true, online: false } });
        off.stopped.value = true;
        off.presence.stop();

        expect([never.leaves, off.leaves]).toEqual([[], []]);
    });
});

// The page's side of each exchange: one client id a page until a goodbye spends it, a body on
// every exchange unless the latest answer said status is off, the people watched named only by a
// refresh that asks about them, and the person's idle time from what she last did on a visible page.
describe("the page's id and each exchange's body", () => {
    const uuid = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/;

    test("before any answer a body goes, even with the switch read as off, naming nobody; one id a page", () => {
        const h = buildStatus({ flags: { typing: true } });

        const first = h.status.exchangeBody("start");
        const second = h.status.exchangeBody("start");

        expect(first).toEqual({ client_id: expect.stringMatching(uuid), kind: "web", idle_seconds: 0 });
        expect(second?.client_id).toBe(first?.client_id);
        expect(h.status.clientId()).toBe(first?.client_id);
    });

    test("a latest answer saying status is off sends no body, and one saying on brings them back at the next exchange", () => {
        const h = buildStatus();
        h.status.exchangeBody("start");
        h.status.exchanged([], exchangeAnswer());
        h.settings.value = readSettings({ ...current, flags: { typing: true, online: false } });

        expect(h.status.exchangeBody("scheduled")).toBeNull();
        h.status.exchanged([], exchangeAnswer());
        h.settings.value = readSettings({ ...current, flags: { typing: true, online: true } });

        expect(h.status.exchangeBody("scheduled")).toMatchObject({ kind: "web", watched: [me] });
    });

    test("only a scheduled or early refresh names the people watched, and only their ids shaped as one, so one malformed id costs nobody else", () => {
        const h = buildStatus();
        h.status.register(() => ["", "not-a-guid", anna, `${sam}x`], 1);
        h.status.exchangeBody("start");
        h.status.exchanged([], exchangeAnswer());

        for (const reason of ["start", "recheck", "expired", "retry"] as const) {
            expect(h.status.exchangeBody(reason)).not.toHaveProperty("watched");
        }
        expect(h.status.exchangeBody("scheduled")?.watched).toEqual([me, anna]);
        expect(h.status.exchangeBody("early")?.watched).toEqual([me, anna]);
    });

    test("idle is whole seconds since the last input on a visible page, the clock running while it is hidden", () => {
        const h = buildStatus();

        jest.advanceTimersByTime(5000);
        h.fire("keydown");
        jest.advanceTimersByTime(3500);
        expect(h.status.exchangeBody("start")?.idle_seconds).toBe(3);

        h.page.visibilityState = "hidden";
        h.fire("pointerdown");
        jest.advanceTimersByTime(10_000);
        expect(h.status.exchangeBody("start")?.idle_seconds).toBe(13);
    });

    test("a scroll is not her input: the feed scrolls itself to the newest message as others post and type, and that scroll arrives as hers would", () => {
        const h = buildStatus();

        jest.advanceTimersByTime(5000);
        h.fire("scroll");

        expect(h.status.exchangeBody("start")?.idle_seconds).toBe(5);
    });

    test("one set of input listeners on the window, capturing and passive, every one gone at stop", () => {
        const h = buildStatus();

        const input = h.listeners.filter(l => !["visibilitychange", "pagehide", "pageshow"].includes(l.type));
        expect(input.map(l => `${l.on} ${l.type}`).sort()).toEqual(["keydown", "pointerdown", "pointermove", "touchstart", "wheel"].map(type => `window ${type}`));
        expect(input.every(l => (l.options as AddEventListenerOptions)?.capture === true && (l.options as AddEventListenerOptions)?.passive === true)).toBe(true);

        h.stopped.value = true;
        h.presence.stop();

        expect(h.listeners).toEqual([]);
    });

    test("closing the page says goodbye for an id a body carried and spends it, so the next body carries a new one; a hidden page says nothing", () => {
        const h = buildStatus();
        h.fire("pagehide");
        expect(h.leaves).toEqual([]);

        const first = h.status.exchangeBody("start")?.client_id;
        h.page.visibilityState = "hidden";
        h.fire("visibilitychange");
        expect(h.leaves).toEqual([]);

        h.fire("pagehide");
        h.fire("pagehide");
        const next = h.status.exchangeBody("start")?.client_id;

        expect(h.leaves).toEqual([first]);
        expect(next).toMatch(uuid);
        expect(next).not.toBe(first);
    });

    test("no goodbye once the latest answer said status is off, though a body carried the id", () => {
        const h = buildStatus();
        h.status.exchangeBody("start");
        h.status.exchanged([], exchangeAnswer());
        h.settings.value = readSettings({ ...current, flags: { typing: true, online: false } });

        h.fire("pagehide");

        expect(h.leaves).toEqual([]);
    });

    test("a page restored from the back-forward cache counts as her input and holds one ask for the recheck's settle, asking nothing itself; a fresh load's pageshow does neither", () => {
        const h = buildStatus({ limits: { presence_idle_seconds: 600 } });
        h.status.exchangeBody("start");
        h.status.exchanged([], exchangeAnswer());
        jest.advanceTimersByTime(400_000);

        h.fire("pageshow", { persisted: false });
        expect(h.status.exchangeBody("recheck")?.idle_seconds).toBe(400);
        h.status.exchanged([], exchangeAnswer());
        h.status.settled("recheck", true);
        expect(h.session.asked).toEqual([]);

        h.fire("pageshow", { persisted: true });
        expect(h.session.asked).toEqual([]);
        expect(h.status.exchangeBody("recheck")?.idle_seconds).toBe(0);
        h.status.exchanged([], exchangeAnswer());
        h.status.settled("recheck", true);

        expect(h.session.asked).toEqual(["early"]);
    });
});

// A sleeping machine fires no timer, and a page left visible across it hears neither of its own
// show events, so what is drawn is read again against the clock whenever the page comes back.
describe("others' status drawn again against the clock", () => {
    test("a clock that jumped past a row's end with no timer fired draws nothing once redrawn", () => {
        const h = buildStatus();
        h.status.told([readerPerson(anna, online(0, 10_000))]);
        const drawn = face(h.status, anna);
        expect(drawn.value?.state).toBe("online");

        jest.setSystemTime(serverNow + 20_000);
        expect(drawn.value?.state).toBe("online");
        h.status.rearm();

        expect(drawn.value).toBeNull();
    });

    test.each([
        ["shown again", (h: ReturnType<typeof buildStatus>) => h.fire("visibilitychange")],
        ["restored from the back-forward cache", (h: ReturnType<typeof buildStatus>) => h.fire("pageshow", { persisted: true })],
        ["the first input after a sleep", (h: ReturnType<typeof buildStatus>) => h.fire("keydown")]
    ])("%s, the page draws again against the clock", (_name, wake) => {
        const h = buildStatus();
        h.status.told([readerPerson(anna, online(0, 10_000))]);
        const drawn = face(h.status, anna);
        expect(drawn.value?.state).toBe("online");

        jest.setSystemTime(serverNow + 20_000);
        wake(h);

        expect(drawn.value).toBeNull();
    });
});

// Asking early, as the model does: an input asks when what the server holds of her runs out before
// the next refresh, and a page shown asks when its badges are older than a token's life; never
// while a refresh is out, which flags one ask again for the session to settle first; at most once
// a minute, and never before the first answer, with status off or with no idle line.
describe("asking early", () => {
    const idle = { limits: { presence_idle_seconds: 600 } };

    /** One exchange of a reason lands and settles, its next refresh due a time after now. */
    function land(h: ReturnType<typeof buildStatus>, reason: Parameters<Presence["status"]["exchangeBody"]>[0], nextAfterMs: number, presence?: Record<string, Record<string, unknown>>): void {
        const body = h.status.exchangeBody(reason);
        h.status.exchanged(body?.watched ?? [], exchangeAnswer(presence));
        h.session.next = Date.now() + nextAfterMs;
        h.session.out = null;
        h.status.settled(reason, true);
    }

    test("an input asks only when what the server holds of her runs out before the next refresh, and at most once a minute", () => {
        const h = buildStatus(idle);
        land(h, "start", 210_000);

        jest.advanceTimersByTime(1000);
        h.fire("keydown");
        expect(h.session.asked).toEqual([]);

        h.session.next = serverNow + 700_000;
        jest.advanceTimersByTime(1000);
        h.fire("keydown");
        jest.advanceTimersByTime(2000);
        h.fire("keydown");
        expect(h.session.asked).toEqual(["early"]);

        jest.advanceTimersByTime(59_000);
        h.fire("keydown");
        expect(h.session.asked).toEqual(["early", "early"]);
    });

    test("nothing asks before the first answer, with status off, or with no idle line", () => {
        const before = buildStatus(idle);
        before.session.next = serverNow + 700_000;
        jest.advanceTimersByTime(1000);
        before.fire("keydown");

        const noLine = buildStatus();
        land(noLine, "start", 700_000);
        jest.advanceTimersByTime(1000);
        noLine.fire("keydown");

        const off = buildStatus(idle);
        land(off, "start", 700_000);
        off.settings.value = readSettings({ flags: { typing: true, online: false }, ...idle });
        jest.advanceTimersByTime(1000);
        off.fire("keydown");

        expect([before.session.asked, noLine.session.asked, off.session.asked]).toEqual([[], [], []]);
    });

    test("an input while a refresh is out flags one ask, which its settle makes once the body that landed is older than the input", () => {
        const h = buildStatus(idle);
        land(h, "start", 210_000);
        jest.advanceTimersByTime(5000);

        h.session.out = "scheduled";
        const body = h.status.exchangeBody("scheduled");
        jest.advanceTimersByTime(1000);
        h.fire("keydown");
        jest.advanceTimersByTime(1000);
        h.fire("keydown");
        h.status.exchanged(body?.watched ?? [], exchangeAnswer({ [me]: online(0) }));
        expect(h.session.asked).toEqual([]);

        h.session.out = null;
        h.session.next = serverNow + 700_000;
        h.status.settled("scheduled", true);

        expect(h.session.asked).toEqual(["early"]);
    });

    test("a page shown while a refresh is out, with no input since its body, asks nothing once that refresh lands with presence", () => {
        const h = buildStatus(idle);
        land(h, "start", 210_000);

        h.session.out = "scheduled";
        const body = h.status.exchangeBody("scheduled");
        h.fire("visibilitychange");
        h.status.exchanged(body?.watched ?? [], exchangeAnswer({ [me]: online(0) }));
        h.session.out = null;
        h.session.next = serverNow + 700_000;
        h.status.settled("scheduled", true);

        expect(h.session.asked).toEqual([]);
    });

    test("a flag survives a failed settle, and the next landing asks once for a page still stale", () => {
        const h = buildStatus(idle);
        land(h, "start", 210_000);
        jest.advanceTimersByTime(301_000);

        h.session.out = "recheck";
        h.status.exchangeBody("recheck");
        h.fire("visibilitychange");
        h.status.exchanged([], null);
        h.session.out = null;
        h.session.next = null;
        h.status.settled("recheck", false);
        expect(h.session.asked).toEqual([]);

        land(h, "retry", 210_000);

        expect(h.session.asked).toEqual(["early"]);
    });

    test("a page shown inside the wait after a failed refresh flags, and the retry's settle asks once", () => {
        const h = buildStatus(idle);
        land(h, "start", 210_000);
        jest.advanceTimersByTime(301_000);
        h.session.next = null;

        h.fire("visibilitychange");
        expect(h.session.asked).toEqual([]);
        land(h, "retry", 210_000);

        expect(h.session.asked).toEqual(["early"]);
    });

    test("a page shown asks only once its badges are older than a token's life, the start's landing counting, and asks at once with no refresh out", () => {
        const h = buildStatus(idle);
        land(h, "start", 210_000);

        jest.advanceTimersByTime(120_000);
        h.fire("visibilitychange");
        expect(h.session.asked).toEqual([]);

        jest.advanceTimersByTime(181_000);
        h.fire("visibilitychange");
        expect(h.session.asked).toEqual(["early"]);
    });

    test("an answer with no presence map leaves the page as stale as it was; one with a map makes it fresh", () => {
        const h = buildStatus(idle);
        land(h, "start", 210_000);
        jest.advanceTimersByTime(250_000);
        land(h, "recheck", 210_000);
        jest.advanceTimersByTime(51_000);
        h.fire("visibilitychange");
        expect(h.session.asked).toEqual(["early"]);

        land(h, "early", 210_000, { [me]: online(0) });
        jest.advanceTimersByTime(61_000);
        h.fire("visibilitychange");
        expect(h.session.asked).toEqual(["early"]);
    });

    test("a flag set while a refresh was out starts no minute, so an input once it has settled asks at once", () => {
        const h = buildStatus(idle);
        land(h, "start", 210_000);

        h.session.out = "scheduled";
        h.status.exchangeBody("scheduled");
        jest.advanceTimersByTime(1000);
        h.fire("keydown");
        h.status.exchanged([], null);
        h.session.out = null;
        h.session.next = serverNow + 700_000;
        h.status.settled("scheduled", false);
        expect(h.session.asked).toEqual([]);

        jest.advanceTimersByTime(1000);
        h.fire("keydown");

        expect(h.session.asked).toEqual(["early"]);
    });

    // The real session never reaches this state: after a failed exchange it either waits, with no
    // next refresh, or mints again inside the same try. The cell guards the rule for a later reader
    // of the held input between a failure and the next landing.
    test("a failed exchange leaves the input the server holds as it was, so the next input still asks", () => {
        const h = buildStatus(idle);
        land(h, "start", 210_000);
        jest.advanceTimersByTime(70_000);
        h.fire("keydown");

        h.status.exchangeBody("scheduled");
        h.status.exchanged([], null);
        h.session.next = serverNow + 650_000;
        jest.advanceTimersByTime(2000);
        h.fire("keydown");

        expect(h.session.asked).toEqual(["early"]);
    });
});

describe("status carried on typing", () => {
    test("a start with status writes it as the dots are drawn, and so do a stop's and a stale row's", () => {
        const h = buildStatus();

        h.presence.heard("typing", signal(anna, { status: online(0) }), group);
        expect(h.presence.state.typists[group]).toEqual([anna]);
        expect(h.status.statusOf(anna)?.state).toBe("online");

        h.presence.heard("typing", signal(anna, { stopped: true, status: away(1000) }), group);
        expect(h.status.statusOf(anna)?.state).toBe("away");

        // a signal naming a message older than one of hers held was sent before it and draws no dots, but its status is newer
        h.hold(12, sam);
        h.presence.heard("typing", signal(sam, { after: 11, status: busy(null, 1000) }), group);
        expect(h.presence.state.typists[group]).toEqual([anna]);
        expect(h.status.statusOf(sam)?.state).toBe("busy");
    });

    test("a stop with no status key, and a start with none, write nothing", () => {
        const h = buildStatus();
        h.status.told([readerPerson(anna, online(0))]);
        const at = h.status.statusOf(anna)?.revision;

        h.presence.heard("typing", signal(anna, { stopped: true }), group);
        h.presence.heard("typing", signal(anna), group);

        expect(h.status.statusOf(anna)).toEqual(expect.objectContaining({ state: "online", revision: at }));
    });

    test("one for a blocked typist, or her own room row, writes nothing", () => {
        const h = buildStatus();
        h.status.exchanged([me], exchangeAnswer({ [me]: nothing(0) }));
        h.blocked.add(anna);

        h.presence.heard("typing", signal(anna, { status: online(1000) }), group);
        h.presence.heard("typing", signal(me, { status: online(1000) }), group);
        h.blocked.delete(anna);

        expect(h.status.statusOf(anna)).toBeNull();
        expect(h.status.statusOf(me)).toBeNull();
    });

    test("with typing off and status on a row still carries its status, though it draws no dots", () => {
        const h = buildStatus({ flags: { typing: false, online: true } });

        h.presence.heard("typing", signal(anna, { status: busy(null, 0) }), group);

        expect(h.presence.state.typists).toEqual({});
        expect(h.status.statusOf(anna)?.state).toBe("busy");
    });

    test("with status off a row writes nothing", () => {
        const h = buildStatus({ flags: { typing: true, online: false } });

        h.presence.heard("typing", signal(anna, { status: online(0) }), group);
        h.settings.value = readSettings({ ...current, flags: { typing: true, online: true } });

        expect(h.status.statusOf(anna)).toBeNull();
    });

    test("a row with every key but status changed, and no status, writes nothing", () => {
        const h = buildStatus();
        h.status.told([readerPerson(anna, online(0))]);
        const at = h.status.statusOf(anna)?.revision;

        h.presence.heard("typing", { ...signal(anna), after: 11, interval_ms: 9000, id: "sig-other", state: "away", until: pgTime(1000), as_of: asOf(5000) }, group);

        expect(h.status.statusOf(anna)).toEqual(expect.objectContaining({ state: "online", revision: at }));
    });
});

describe("nothing but status writes", () => {
    test("a reader's person whose status cannot be read writes nothing, whatever else it carries", () => {
        const h = buildStatus();
        h.status.told([readerPerson(anna, online(0))]);

        h.status.told([{ ...readerPerson(anna, { as_of: "soon" }), state: "away", until: pgTime(1000), as_of: asOf(5000) }]);

        expect(h.status.statusOf(anna)?.state).toBe("online");
    });
});

describe("the typing call's client", () => {
    test("every signal names the page's id, the one the exchange's body carries", async () => {
        const h = buildStatus();
        const id = h.status.exchangeBody("start")?.client_id;

        h.presence.typed(group, null, "a");
        await pass(700);
        h.presence.typed(group, null, "");
        await settle();

        expect(id).toEqual(expect.any(String));
        expect(h.calls.map(c => c.body.p_client)).toEqual([id, id]);
    });

    test("a page whose goodbye spent its id names a new one, minted for the signal, and never a client as null", async () => {
        const h = buildStatus();
        const id = h.status.exchangeBody("start")?.client_id;
        h.status.exchanged([], exchangeAnswer());
        h.fire("pagehide", { persisted: true });

        h.presence.typed(group, null, "a");
        await pass(700);

        expect(h.calls[0].body.p_client).toEqual(expect.any(String));
        expect(h.calls[0].body.p_client).not.toBe(id);
        expect(h.status.exchangeBody("recheck")?.client_id).toBe(h.calls[0].body.p_client);
    });
});

describe("Appear offline and typing", () => {
    test("Appear offline in her settings stops the next signal, and the switch being off changes nothing of it", async () => {
        const on = buildStatus();
        on.me.appear_offline = true;
        on.presence.typed(group, null, "a");
        await pass(700);

        const off = buildStatus({ flags: { typing: true, online: false } });
        off.me.appear_offline = true;
        off.presence.typed(group, null, "a");
        await pass(700);

        expect(on.calls).toEqual([]);
        expect(off.calls).toEqual([]);
    });
});

describe("her own face", () => {
    const idleLine = { limits: { presence_idle_seconds: 30 } };

    test("draws Online before any answer of hers, and from her input while it is fresh, whatever an answer says", () => {
        const h = buildStatus(idleLine);
        expect(h.status.statusOf(me)).toEqual(expect.objectContaining({ state: "online", tip: "Online", label: ", online" }));

        h.status.exchanged([me], exchangeAnswer({ [me]: away(0) }));
        expect(h.status.statusOf(me)?.state).toBe("online");

        // with her input past the idle line and no answer of hers yet, she has the page open
        const fresh = buildStatus(idleLine);
        jest.advanceTimersByTime(31_000);
        expect(fresh.status.statusOf(me)?.state).toBe("online");
    });

    test("draws her answer once her input is older than the idle line, nothing for a null one, and Online again at her next input, each by revision", () => {
        const h = buildStatus(idleLine);
        h.status.exchanged([me], exchangeAnswer({ [me]: nothing(0) }));
        const drawn = face(h.status, me);
        expect(drawn.value?.state).toBe("online");

        jest.advanceTimersByTime(30_000);
        expect(drawn.value).toBeNull();

        h.fire("keydown");
        expect(drawn.value?.state).toBe("online");

        h.status.exchanged([me], exchangeAnswer({ [me]: away(31_000) }));
        jest.advanceTimersByTime(30_000);
        expect(drawn.value?.state).toBe("away");
    });

    test("draws Busy from her settings at once, with her own words, and Busy with no end for infinity", () => {
        const h = buildStatus(idleLine);
        const drawn = face(h.status, me);
        expect(drawn.value?.state).toBe("online");
        const at = drawn.value?.revision ?? 0;

        h.me.busy_until = pgTime(3_600_000);
        expect(drawn.value).toEqual(expect.objectContaining({ state: "busy", busyUntil: serverNow + 3_600_000 }));
        // her revision moves, so her face fades to it rather than drawing it as state
        expect(drawn.value?.revision).toBeGreaterThan(at);
        expect(drawn.value?.tip).toMatch(/^Busy, notifications paused until /);

        h.me.busy_until = "infinity";
        expect(drawn.value).toEqual(expect.objectContaining({ state: "busy", tip: "Busy, notifications paused", busyUntil: null }));

        h.me.busy_until = pgTime(-1000);
        expect(drawn.value?.state).toBe("online");
    });

    test("turns from Busy to her automatic state at its end, with no request", () => {
        const h = buildStatus(idleLine);
        h.me.busy_until = pgTime(60_000);
        // her answer was decided while a longer Busy was on, so only her settings' end can turn her
        h.status.exchanged([me], exchangeAnswer({ [me]: { ...busy(120_000, 0), auto: "online" } }));
        const drawn = face(h.status, me);
        expect(drawn.value?.state).toBe("busy");
        const at = drawn.value?.revision ?? 0;

        // her input is older than the idle line by then, so what draws is her answer's automatic state
        jest.advanceTimersByTime(60_000);

        expect(drawn.value?.state).toBe("online");
        expect(drawn.value?.revision).toBeGreaterThan(at);
        expect(h.session.asked).toEqual([]);
    });

    test("draws Appear offline from her settings as hidden, before Busy and her input", () => {
        const h = buildStatus(idleLine);
        h.me.busy_until = "infinity";
        const drawn = face(h.status, me);

        h.me.appear_offline = true;

        expect(drawn.value).toEqual(expect.objectContaining({ state: "hidden", tip: "Appear offline" }));
    });

    test("draws nothing until her settings have been read, then what they say, with no Online between", () => {
        const h = buildStatus(idleLine);
        h.prefs.isMeRead = false;
        const states: Array<string | null> = [];
        const stopRecording = watchEffect(() => states.push(h.status.statusOf(me)?.state ?? null), { flush: "sync" });
        expect(h.status.statusOf(me)).toBeNull();

        // as the sidebar's me lands: her settings, then that they are read
        h.me.appear_offline = true;
        h.prefs.isMeRead = true;
        stopRecording();

        expect(h.status.statusOf(me)).toEqual(expect.objectContaining({ state: "hidden" }));
        expect(states).toEqual([null, "hidden"]);
    });

    test("an answer of hers that says Busy with no automatic state, her settings not Busy and her input old, draws Away", () => {
        const h = buildStatus(idleLine);
        jest.advanceTimersByTime(31_000);

        h.status.told([readerPerson(me, busy(600_000, 31_000))]);

        expect(h.status.statusOf(me)?.state).toBe("away");
    });

    test("draws nothing with the switch off, whatever her settings say", () => {
        const h = buildStatus({ flags: { typing: true, online: false }, ...idleLine });
        h.me.appear_offline = true;

        expect(h.status.statusOf(me)).toBeNull();
    });
});

describe("the status store's size", () => {
    test("drops a person who has drawn nothing for longer than any older decision of theirs could, so a write walks only who can still draw", () => {
        const h = buildStatus();
        const people = Array.from({ length: 300 }, (_, i) => `b${String(i).padStart(7, "0")}-0000-4000-8000-000000000000`);
        h.status.told(people.map(alias => readerPerson(alias, online(0))));

        // past every row's end, and past how long the store keeps a decision that draws nothing
        jest.advanceTimersByTime(heldNothingMs + 1000);
        h.status.told([readerPerson(anna, online(heldNothingMs, heldNothingMs + rowLifeMs))]);
        const parse = jest.spyOn(Date, "parse");
        h.status.told([readerPerson(sam, online(heldNothingMs, heldNothingMs + rowLifeMs))]);
        const parsed = parse.mock.calls.length;
        parse.mockRestore();

        expect(parsed).toBeLessThan(20);
        expect(h.status.statusOf(anna)?.state).toBe("online");
        expect(h.status.statusOf(people[0])).toBeNull();
    });

    test("keeps a decision that says nothing for as long as an older one could still draw", () => {
        const h = buildStatus();
        h.status.told([readerPerson(anna, nothing(0))]);

        jest.advanceTimersByTime(rowLifeMs - 10_000);
        h.status.told([readerPerson(sam, online(rowLifeMs - 10_000))]);
        h.status.carried(anna, online(-1000));

        expect(h.status.statusOf(anna)).toBeNull();
    });

    test("keeps a decision that says nothing past the platform's longest row, whatever the viewer's own token's life, with room for that row to grow", () => {
        const h = buildStatus();
        // a token minted from a church token held, which the platform cuts to two minutes
        h.session.life = 120;
        h.status.told([readerPerson(anna, nothing(0))]);

        jest.advanceTimersByTime(400_000);
        h.status.told([readerPerson(sam, online(400_000))]);
        // decided before the nothing, its row the platform's full token life and slack
        h.status.carried(anna, online(-10_000, rowLifeMs - 10_000));
        expect(h.status.statusOf(anna)).toBeNull();

        // a platform whose rows last half again as long
        jest.advanceTimersByTime(400_000);
        h.status.told([readerPerson(sam, online(800_000))]);
        h.status.carried(anna, online(-10_000, rowLifeMs * 1.5));
        expect(h.status.statusOf(anna)).toBeNull();
    });

    test("never drops her own answer, so a null one still draws nothing once her input is old", () => {
        const h = buildStatus({ limits: { presence_idle_seconds: 30 } });
        h.status.exchanged([me], exchangeAnswer({ [me]: nothing(0) }));

        jest.advanceTimersByTime(heldNothingMs + 1000);
        h.status.told([readerPerson(sam, online(heldNothingMs + 1000))]);

        expect(h.status.statusOf(me)).toBeNull();
    });
});

describe("what a face reads", () => {
    test("a token exchange that answers the same switch and idle line draws no face again, hers included; a switch flipped draws each", () => {
        const h = buildStatus({ limits: { presence_idle_seconds: 30 } });
        h.status.told([readerPerson(anna, online(0))]);
        const renders = { anna: 0, me: 0 };
        const stops = [
            watchEffect(() => void (renders.anna++, h.status.statusOf(anna)), { flush: "sync" }),
            watchEffect(() => void (renders.me++, h.status.statusOf(me)), { flush: "sync" })
        ];

        // as each landed exchange does, the settings replaced whole, here with a new token's pace too
        h.settings.value = readSettings({ flags: { typing: true, online: true }, limits: { presence_idle_seconds: 30, typing_interval_ms: 9000 } });
        h.settings.value = { ...h.settings.value };
        expect(renders).toEqual({ anna: 1, me: 1 });

        h.settings.value = readSettings({ flags: { typing: true, online: false }, limits: { presence_idle_seconds: 30 } });
        stops.forEach(stop => stop());

        expect(renders).toEqual({ anna: 2, me: 2 });
        expect(h.status.statusOf(anna)).toBeNull();
    });
});

// #endregion Others' status
