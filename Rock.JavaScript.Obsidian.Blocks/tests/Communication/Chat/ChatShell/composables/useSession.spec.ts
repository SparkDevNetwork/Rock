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
// The session: Rock mints, the platform exchanges, and the platform token is refreshed the same
// way at a random point between half and nine tenths of its life, so a church's Sunday morning
// opens do not all refresh together. A refresh is handed to the connection already open rather
// than opening a new one, because a new token on the open connection is what makes the platform
// re-check which channels the person may still hear.
import {
    ChatSettings,
    churchTokenReuseMs,
    ChurchTokenResult,
    createSession,
    ExchangeResult,
    readSettings,
    serviceBanner,
    SessionDependencies,
    tryTimeoutMs
} from "../../../../../src/Communication/Chat/ChatShell/composables/useSession.partial";
import { exchangeChurchToken } from "../../../../../src/Communication/Chat/ChatShell/platformCall.partial";
import { ChatError } from "../../../../../src/Communication/Chat/ChatShell/types.partial";

/** The session with the settings it holds. */
type SettingsSession = ReturnType<typeof createSession> & {
    settings: () => ChatSettings,
    applyServiceRefusal: (error: ChatError & { text?: string | null }) => void
};

type Timer = { callback: () => void, milliseconds: number, cleared: boolean };

function fakes(overrides: Partial<SessionDependencies> = {}): { dependencies: SessionDependencies, timers: Timer[], mints: number, exchanges: string[], pushes: number } {
    const record = { timers: [] as Timer[], deadlines: [] as Timer[], mints: 0, exchanges: [] as string[], pushes: 0 };

    const dependencies: SessionDependencies = {
        mintChurchToken: async (): Promise<ChurchTokenResult> => {
            record.mints++;
            return { gate: "ok", churchToken: `church-${record.mints}` };
        },
        exchange: async (churchToken: string): Promise<ExchangeResult> => {
            record.exchanges.push(churchToken);
            return { ok: true, accessToken: `platform-for-${churchToken}`, expiresInSeconds: 300 };
        },
        pushTokenToConnection: async (): Promise<void> => {
            record.pushes++;
        },
        random: () => 0.5,
        setTimer: (callback: () => void, milliseconds: number): unknown => {
            const timer = { callback, milliseconds, cleared: false };
            record.timers.push(timer);
            return timer;
        },
        // Every try sets a deadline of its own and clears it when the try answers; those are kept
        // apart by who set them, so the timers a test reads are the refreshes and waits the
        // session schedules, whatever their length.
        setDeadline: (callback: () => void, milliseconds: number): unknown => {
            const timer = { callback, milliseconds, cleared: false };
            record.deadlines.push(timer);
            return timer;
        },
        clearTimer: (handle: unknown): void => {
            (handle as Timer).cleared = true;
        },
        ...overrides
    };

    return {
        dependencies,
        get timers() {
            return record.timers;
        },
        get mints() {
            return record.mints;
        },
        get exchanges() {
            return record.exchanges;
        },
        get pushes() {
            return record.pushes;
        }
    };
}

async function settle(): Promise<void> {
    for (let i = 0; i < 10; i++) {
        await Promise.resolve();
    }
}

describe("createSession", () => {
    test("start mints, exchanges and holds the platform token", async () => {
        const f = fakes();
        const session = createSession(f.dependencies);

        expect(await session.start()).toBe(true);

        expect(f.mints).toBe(1);
        expect(f.exchanges).toEqual(["church-1"]);
        expect(session.currentToken()).toBe("platform-for-church-1");
        expect(session.state.gate).toBe("ok");
    });

    test("a refused gate stops before the exchange and says which gate", async () => {
        const f = fakes({ mintChurchToken: async () => ({ gate: "banned", churchToken: null }) });
        const session = createSession(f.dependencies);

        expect(await session.start()).toBe(false);

        expect(f.exchanges).toEqual([]);
        expect(session.currentToken()).toBeNull();
        expect(session.state.gate).toBe("banned");
        expect(f.timers).toHaveLength(0);
    });

    test.each([
        [0, 0.5],
        [0.5, 0.7],
        [0.999, 0.8996]
    ])("with random %p the refresh is scheduled at %p of the token's life", async (random, fraction) => {
        const f = fakes({ random: () => random });
        const session = createSession(f.dependencies);

        await session.start();

        expect(f.timers).toHaveLength(1);
        expect(f.timers[0].milliseconds).toBeCloseTo(300_000 * fraction, -1);
    });

    test("the refresh mints and exchanges again and hands the token to the open connection", async () => {
        const f = fakes();
        const session = createSession(f.dependencies);
        await session.start();

        f.timers[0].callback();
        await settle();

        expect(f.mints).toBe(2);
        expect(session.currentToken()).toBe("platform-for-church-2");
        expect(f.pushes).toBe(1);
        expect(f.timers).toHaveLength(2);
    });

    test("the first token is not pushed, because the connection reads it before its first join", async () => {
        const f = fakes();
        const session = createSession(f.dependencies);

        await session.start();

        expect(f.pushes).toBe(0);
    });

    test("a gate that refuses at refresh keeps no token and schedules nothing more", async () => {
        let calls = 0;
        const f = fakes({
            mintChurchToken: async () => {
                calls++;
                return calls === 1 ? { gate: "ok", churchToken: "church-1" } : { gate: "banned", churchToken: null };
            }
        });
        const session = createSession(f.dependencies);
        await session.start();

        expect(await session.refresh()).toBe(false);

        expect(session.currentToken()).toBeNull();
        expect(session.state.gate).toBe("banned");
        expect(f.timers).toHaveLength(1);
    });

    test("a refresh the exchange cannot complete keeps the token it has and tries again soon", async () => {
        let exchanges = 0;
        const f = fakes({
            exchange: async (churchToken: string): Promise<ExchangeResult> => {
                exchanges++;
                return exchanges === 2
                    ? { ok: false, status: 0, code: "rpc.transport" }
                    : { ok: true, accessToken: `platform-for-${churchToken}`, expiresInSeconds: 300 };
            }
        });
        const session = createSession(f.dependencies);
        await session.start();

        expect(await session.refresh()).toBe(false);

        // The token still has life left; throwing it away would silence every save until expiry.
        expect(session.currentToken()).toBe("platform-for-church-1");
        const retry = f.timers[f.timers.length - 1];
        expect(retry.cleared).toBe(false);
        // The first retry waits between half and all of the default one second base, here at the
        // middle of it, rather than a fixed share of the token's life that every client shares.
        expect(retry.milliseconds).toBe(750);

        retry.callback();
        await settle();

        // the retry exchanges the church token the failed try left, still young, rather than minting
        expect(session.currentToken()).toBe("platform-for-church-2");
    });

    test("a refresh that cannot reach Rock keeps the token it has and tries again soon", async () => {
        let mints = 0;
        const f = fakes({
            mintChurchToken: async (): Promise<ChurchTokenResult> => {
                mints++;
                return mints === 2
                    ? { gate: "gate_unavailable", churchToken: null, isUnreachable: true }
                    : { gate: "ok", churchToken: `church-${mints}` };
            }
        });
        const session = createSession(f.dependencies);
        await session.start();

        expect(await session.refresh()).toBe(false);

        expect(session.currentToken()).toBe("platform-for-church-1");
        expect(f.timers[f.timers.length - 1].cleared).toBe(false);
        expect(f.timers[f.timers.length - 1].milliseconds).toBe(750);
    });

    test("a refresh the platform refuses outright ends the session rather than asking again", async () => {
        let exchanges = 0;
        const f = fakes({
            exchange: async (churchToken: string): Promise<ExchangeResult> => {
                exchanges++;
                return exchanges === 2
                    ? { ok: false, status: 401, code: "auth.invalid_token" }
                    : { ok: true, accessToken: `platform-for-${churchToken}`, expiresInSeconds: 300 };
            }
        });
        const session = createSession(f.dependencies);
        await session.start();

        expect(await session.refresh()).toBe(false);

        expect(session.currentToken()).toBeNull();
        expect(f.timers.every(t => t.cleared)).toBe(true);
    });

    test.each([
        [0, true], [429, true], [500, true], [503, true],
        [400, false], [401, false], [403, false], [499, false]
    ])("an exchange answering %p at refresh is asked again: %p", async (status, isKept) => {
        let exchanges = 0;
        const f = fakes({
            exchange: async (churchToken: string): Promise<ExchangeResult> => {
                exchanges++;
                return exchanges === 2
                    ? { ok: false, status, code: "auth.invalid_token" }
                    : { ok: true, accessToken: `platform-for-${churchToken}`, expiresInSeconds: 300 };
            }
        });
        const session = createSession(f.dependencies);
        await session.start();

        await session.refresh();

        expect(session.currentToken() !== null).toBe(isKept);
    });

    test("a refresh the platform could not serve is asked again", async () => {
        let exchanges = 0;
        const f = fakes({
            exchange: async (churchToken: string): Promise<ExchangeResult> => {
                exchanges++;
                return exchanges === 2
                    ? { ok: false, status: 503, code: "auth.invalid_token" }
                    : { ok: true, accessToken: `platform-for-${churchToken}`, expiresInSeconds: 300 };
            }
        });
        const session = createSession(f.dependencies);
        await session.start();

        expect(await session.refresh()).toBe(false);

        expect(session.currentToken()).toBe("platform-for-church-1");
        expect(f.timers[f.timers.length - 1].cleared).toBe(false);
    });

    test("once the held token has expired, a failing refresh ends the session", async () => {
        let clock = 0;
        let exchanges = 0;
        const f = fakes({
            now: () => clock,
            exchange: async (churchToken: string): Promise<ExchangeResult> => {
                exchanges++;
                return exchanges === 1
                    ? { ok: true, accessToken: `platform-for-${churchToken}`, expiresInSeconds: 300 }
                    : { ok: false, status: 0, code: "rpc.transport" };
            }
        });
        const session = createSession(f.dependencies);
        await session.start();

        clock = 250_000;
        expect(await session.refresh()).toBe(false);
        expect(session.currentToken()).toBe("platform-for-church-1");

        // A second failure while the token is alive still keeps it, so the rule is the expiry
        // and not a count of failures.
        clock = 275_000;
        f.timers[f.timers.length - 1].callback();
        await settle();
        expect(session.currentToken()).toBe("platform-for-church-1");

        clock = 300_000;
        f.timers[f.timers.length - 1].callback();
        await settle();

        expect(session.currentToken()).toBeNull();
        expect(f.timers.filter(t => !t.cleared)).toHaveLength(0);
    });

    test("refreshes asked for together share one mint and exchange", async () => {
        const f = fakes();
        const session = createSession(f.dependencies);
        await session.start();

        const results = await Promise.all([session.refresh(), session.refresh(), session.refresh()]);

        expect(results).toEqual([true, true, true]);
        expect(f.mints).toBe(2);
        expect(f.pushes).toBe(1);
    });

    test("a church token the platform calls stale is minted again once", async () => {
        let exchanges = 0;
        const f = fakes({
            exchange: async (churchToken: string): Promise<ExchangeResult> => {
                exchanges++;
                return exchanges === 1
                    ? { ok: false, status: 401, code: "auth.stale_token" }
                    : { ok: true, accessToken: `platform-for-${churchToken}`, expiresInSeconds: 300 };
            }
        });
        const session = createSession(f.dependencies);

        expect(await session.start()).toBe(true);

        expect(f.mints).toBe(2);
        expect(session.currentToken()).toBe("platform-for-church-2");
    });

    test("a second stale answer is a failure, not a loop", async () => {
        const f = fakes({ exchange: async () => ({ ok: false, status: 401, code: "auth.stale_token" }) });
        const session = createSession(f.dependencies);

        expect(await session.start()).toBe(false);

        expect(f.mints).toBe(2);
        expect(session.state.errorCode).toBe("auth.stale_token");
    });

    test("a call refused as expired refreshes once and runs again", async () => {
        const f = fakes();
        const session = createSession(f.dependencies);
        await session.start();
        const answers = ["expired", "fine"];

        const result = await session.withFreshToken(async () => answers.shift() as string, r => r === "expired");

        expect(result).toBe("fine");
        expect(f.mints).toBe(2);
        expect(f.pushes).toBe(1);
    });

    test("a call refused as expired twice returns the second refusal", async () => {
        const f = fakes();
        const session = createSession(f.dependencies);
        await session.start();

        const result = await session.withFreshToken(async () => "expired", r => r === "expired");

        expect(result).toBe("expired");
        expect(f.mints).toBe(2);
    });

    test("stop clears the pending refresh", async () => {
        const f = fakes();
        const session = createSession(f.dependencies);
        await session.start();

        session.stop();

        expect(f.timers[0].cleared).toBe(true);
    });

    test("a refresh still running when the session stops schedules nothing after it", async () => {
        let answer!: (result: ChurchTokenResult) => void;
        let calls = 0;
        const f = fakes({
            mintChurchToken: async (): Promise<ChurchTokenResult> => {
                calls++;
                if (calls === 1) {
                    return { gate: "ok", churchToken: "church-1" };
                }
                return new Promise(resolve => answer = resolve);
            }
        });
        const session = createSession(f.dependencies);
        await session.start();

        const refreshing = session.refresh();
        await settle();
        session.stop();
        answer({ gate: "ok", churchToken: "church-2" });
        await refreshing;

        expect(f.timers.filter(t => !t.cleared)).toEqual([]);
        expect(f.pushes).toBe(0);
        expect(f.exchanges).toEqual(["church-1"]);
    });

    test("once stopped, a call the platform calls expired does not mint again", async () => {
        const f = fakes();
        const session = createSession(f.dependencies);
        await session.start();
        session.stop();
        const before = f.mints;

        const result = await session.withFreshToken(async () => "expired", () => true);

        expect(result).toBe("expired");
        expect(f.mints).toBe(before);
    });

    test("a refusal that arrives after the session stopped does not end it a second time", async () => {
        let answer!: (result: ChurchTokenResult) => void;
        let calls = 0;
        let ended = 0;
        const f = fakes({
            mintChurchToken: async (): Promise<ChurchTokenResult> => {
                calls++;
                if (calls === 1) {
                    return { gate: "ok", churchToken: "church-1" };
                }
                return new Promise(resolve => answer = resolve);
            },
            onEnded: () => ended++
        });
        const session = createSession(f.dependencies);
        await session.start();

        const refreshing = session.refresh();
        await settle();
        session.stop();
        answer({ gate: "banned", churchToken: null });
        await refreshing;

        expect(ended).toBe(0);
    });

    test("a platform token that arrives after the session stopped is not kept", async () => {
        let answer!: (result: ExchangeResult) => void;
        let calls = 0;
        const f = fakes({
            exchange: async (churchToken: string): Promise<ExchangeResult> => {
                calls++;
                if (calls === 1) {
                    return { ok: true, accessToken: `platform-for-${churchToken}`, expiresInSeconds: 300 };
                }
                return new Promise(resolve => answer = resolve);
            }
        });
        const session = createSession(f.dependencies);
        await session.start();

        const refreshing = session.refresh();
        await settle();
        session.stop();
        answer({ ok: true, accessToken: "platform-late", expiresInSeconds: 300 });
        await refreshing;

        expect(session.currentToken()).toBe("platform-for-church-1");
    });
});

describe("settings from the token exchange", () => {
    const defaults: ChatSettings = {
        service: { state: "normal", banner: null },
        client: { update_required: false },
        // no region named, so calls run wherever the platform's edge takes them
        routes: { send: "direct", region: null },
        // the platform's own limits have no default here: one not sent reads as 0
        limits: {
            reconnect_base_ms: 1000,
            reconnect_cap_ms: 30000,
            catch_up_page: 100,
            catch_up_max: 500,
            problem_reports_per_load: 10,
            tally_reports_per_load: 10,
            gap_reorder_ms: 500,
            gap_repair_wait_ms: 5000,
            stale_reload_ms: 900000,
            typing_interval_ms: 4000,
            typing_expiry_ms: 6000,
            typing_first_after_ms: 700,
            typing_min_show_ms: 1000,
            dm_max_others: 0,
            dm_name_max_length: 0,
            body_max_length: 0,
            history_page_size: 0,
            member_page_size: 0,
            mention_page_size: 0,
            thread_page_size: 0,
            directory_page_size: 0,
            people_search_size: 0,
            attachment_max_bytes: 0,
            attachment_max_files: 0,
            // no list takes no files
            attachment_types: []
        },
        // GIFs and typing are off unless the platform says the church has them on
        flags: { gifs: false, typing: false },
        push: { web: null, vapid_key: null, prompt: "after_send" }
    };

    function exchangeWith(...settings: unknown[]): Partial<SessionDependencies> {
        let calls = 0;
        return {
            exchange: async (churchToken: string): Promise<ExchangeResult> => {
                const answer = settings[Math.min(calls++, settings.length - 1)];
                return { ok: true, accessToken: `platform-for-${churchToken}`, expiresInSeconds: 300, settings: answer } as ExchangeResult;
            }
        };
    }

    test("a missing key, or no settings at all, falls back to the built-in default", () => {
        expect(readSettings(undefined)).toEqual(defaults);
        expect(readSettings({})).toEqual(defaults);
        expect(readSettings("not an object")).toEqual(defaults);
        expect(readSettings({ limits: { reconnect_cap_ms: 60000 } }).limits).toEqual({ ...defaults.limits, reconnect_cap_ms: 60000 });
    });

    test("the platform's limits are read as it sent them, and one missing or not a positive whole number reads as 0", () => {
        const platform = {
            dm_max_others: 8,
            dm_name_max_length: 100,
            body_max_length: 4000,
            history_page_size: 50,
            member_page_size: 50,
            mention_page_size: 30,
            thread_page_size: 30,
            directory_page_size: 50,
            people_search_size: 10,
            attachment_max_bytes: 26214400,
            attachment_max_files: 10,
            attachment_types: ["image/jpeg", "application/pdf"]
        };

        expect(readSettings({ limits: platform }).limits).toEqual({ ...defaults.limits, ...platform });
        // a list holding anything but types is no list, so no file is taken on a guess
        expect(readSettings({ limits: { ...platform, attachment_types: ["image/jpeg", 7] } }).limits.attachment_types).toEqual([]);
        expect(readSettings({ limits: { ...platform, attachment_types: "image/jpeg" } }).limits.attachment_types).toEqual([]);
        const odd = readSettings({ limits: { ...platform, dm_max_others: "8", history_page_size: 2.5, people_search_size: -1, member_page_size: undefined } }).limits;
        expect([odd.dm_max_others, odd.history_page_size, odd.people_search_size, odd.member_page_size]).toEqual([0, 0, 0, 0]);
    });

    // The platform sets the flag from the church's switch and Spark's; only the JSON value true
    // turns GIFs on, so a text "true" or anything else this client cannot read leaves them off.
    test("GIFs are on only when the platform's flag is the boolean true", () => {
        expect(readSettings({ flags: { gifs: true } }).flags.gifs).toBe(true);
        expect(readSettings({ flags: { gifs: "true" } }).flags.gifs).toBe(false);
        expect(readSettings({ flags: { gifs: 1 } }).flags.gifs).toBe(false);
        expect(readSettings({ flags: {} }).flags.gifs).toBe(false);
        expect(readSettings({ flags: { gifs: true, other: "kept" } }).flags).toEqual({ gifs: true, typing: false, other: "kept" });
    });

    test("the first wait and the minimum show are read as sent, else 0.7 and 1 second, and no setting says whether a stop is taken", () => {
        expect(readSettings({ limits: { typing_first_after_ms: 500, typing_min_show_ms: 1500 } }))
            .toMatchObject({ limits: { typing_first_after_ms: 500, typing_min_show_ms: 1500 } });
        expect(readSettings({ limits: { typing_first_after_ms: "500", typing_min_show_ms: -1 } }))
            .toMatchObject({ limits: { typing_first_after_ms: 700, typing_min_show_ms: 1000 } });
        expect(readSettings({ limits: {} }).flags).not.toHaveProperty("typing_stop");
    });

    test("typing is on only when the platform's flag is the boolean true", () => {
        expect(readSettings({ flags: { typing: true } }).flags.typing).toBe(true);
        expect(readSettings({ flags: { typing: "true" } }).flags.typing).toBe(false);
        expect(readSettings({ flags: {} }).flags.typing).toBe(false);
    });

    // A platform that sends no pace sends no typing flag either, so the defaults only keep a
    // missing key from becoming a signal on every key.
    test("the typing pace and expiry are read as sent, else 4 and 6 seconds", () => {
        expect(readSettings({ limits: { typing_interval_ms: 6860, typing_expiry_ms: 9000 } }).limits)
            .toMatchObject({ typing_interval_ms: 6860, typing_expiry_ms: 9000 });
        expect(readSettings({ limits: { typing_interval_ms: "6860", typing_expiry_ms: 0 } }).limits)
            .toMatchObject({ typing_interval_ms: 4000, typing_expiry_ms: 6000 });
    });

    test("the database's region is read when it is a region's name, else there is none", () => {
        expect(readSettings({ routes: { region: "us-east-1" } }).routes.region).toBe("us-east-1");
        expect(readSettings({ routes: { region: "ap-southeast-2" } }).routes.region).toBe("ap-southeast-2");
        expect(readSettings({ routes: { region: "us-gov-west-1" } }).routes.region).toBe("us-gov-west-1");
        for (const region of [undefined, "", 7, "US-EAST-1", "us-east-1; x", "https://example.com"]) {
            expect(readSettings({ routes: { region } }).routes.region).toBeNull();
        }
    });

    test("the problem reports a page load may send are read as sent, else 10", () => {
        expect(readSettings({ limits: { problem_reports_per_load: 3 } }).limits.problem_reports_per_load).toBe(3);
        expect(readSettings({ limits: { problem_reports_per_load: "3" } }).limits.problem_reports_per_load).toBe(10);
        expect(readSettings({ limits: { problem_reports_per_load: 0 } }).limits.problem_reports_per_load).toBe(10);
    });

    test("the tallies a page load may send are read as sent, else 10, apart from the one-off reports' cap", () => {
        expect(readSettings({ limits: { tally_reports_per_load: 4 } }).limits).toMatchObject({ tally_reports_per_load: 4, problem_reports_per_load: 10 });
        expect(readSettings({ limits: { problem_reports_per_load: 3 } }).limits).toMatchObject({ tally_reports_per_load: 10, problem_reports_per_load: 3 });
        expect(readSettings({ limits: { tally_reports_per_load: "4" } }).limits.tally_reports_per_load).toBe(10);
        expect(readSettings({ limits: { tally_reports_per_load: -1 } }).limits.tally_reports_per_load).toBe(10);
    });

    test("the waits of a missed live change are read as sent, else their estimates", () => {
        expect(readSettings({ limits: { gap_reorder_ms: 800, gap_repair_wait_ms: 20000, stale_reload_ms: 60000 } }).limits)
            .toMatchObject({ gap_reorder_ms: 800, gap_repair_wait_ms: 20000, stale_reload_ms: 60000 });
        expect(readSettings({ limits: { gap_reorder_ms: "800", gap_repair_wait_ms: 0, stale_reload_ms: -1 } }).limits)
            .toMatchObject({ gap_reorder_ms: 500, gap_repair_wait_ms: 5000, stale_reload_ms: 900000 });
    });

    test("a catch-up page is never larger than history will serve", () => {
        // history refuses a page of more than 100, so a larger setting would fail every catch-up
        expect(readSettings({ limits: { catch_up_page: 500 } }).limits.catch_up_page).toBe(100);
        expect(readSettings({ limits: { catch_up_page: 0.5 } }).limits.catch_up_page).toBe(1);
        expect(readSettings({ limits: { catch_up_page: 40 } }).limits.catch_up_page).toBe(40);
    });

    // Push is offered only when the platform sent both Firebase values; a church's prompt timing
    // this client does not know is read as the default, after the person's first send.
    test("push is available only with a web config and a key, and its prompt falls back to after a send", () => {
        const push = (raw: unknown): unknown => readSettings(raw).push;
        const web = { apiKey: "k", projectId: "p", messagingSenderId: "1", appId: "1:1:web:1" };

        expect(push({ push: { web, vapid_key: "v", prompt: "offer" } })).toEqual({ web, vapid_key: "v", prompt: "offer" });
        expect(push({ push: { web, vapid_key: "v", prompt: "off" } })).toEqual({ web, vapid_key: "v", prompt: "off" });
        expect(push({ push: { web, prompt: "offer" } })).toEqual({ web: null, vapid_key: null, prompt: "offer" });
        expect(push({ push: { web: "not an object", vapid_key: "v" } })).toEqual({ web: null, vapid_key: null, prompt: "after_send" });
        expect(push({ push: { web, vapid_key: "v", prompt: "when_the_moon_is_full" } })).toEqual({ web, vapid_key: "v", prompt: "after_send" });
    });

    test("a route or a state this client does not know is read as the safe default", () => {
        expect(readSettings({ routes: { send: "carrier_pigeon" } }).routes.send).toBe("direct");
        expect(readSettings({ service: { state: "something_new", banner: "x" } }).service).toEqual({ state: "normal", banner: "x" });
    });

    test("the settings are read at sign-in", async () => {
        const f = fakes(exchangeWith({ service: { state: "degraded", banner: "Chat is slow today." }, routes: { send: "edge" } }));
        const session = createSession(f.dependencies) as SettingsSession;

        await session.start();

        expect(session.settings().service).toEqual({ state: "degraded", banner: "Chat is slow today." });
        expect(session.settings().routes.send).toBe("edge");
    });

    test("every refresh reads them again", async () => {
        const f = fakes(exchangeWith({ service: { state: "normal" } }, { service: { state: "read_only", banner: "Read only tonight." } }));
        const session = createSession(f.dependencies) as SettingsSession;
        await session.start();

        await session.refresh();

        expect(session.settings().service).toEqual({ state: "read_only", banner: "Read only tonight." });
    });

    test("a refusal for read only or maintenance takes its banner from the refusal's sentence", async () => {
        const f = fakes();
        const session = createSession(f.dependencies) as SettingsSession;
        await session.start();

        session.applyServiceRefusal({ code: "rpc.read_only", severity: "failed", text: "Chat is read only for maintenance tonight." });
        expect(session.settings().service).toEqual({ state: "read_only", banner: "Chat is read only for maintenance tonight." });

        session.applyServiceRefusal({ code: "rpc.maintenance", severity: "failed", text: "Chat is down for maintenance." });
        expect(session.settings().service).toEqual({ state: "maintenance", banner: "Chat is down for maintenance." });

        session.applyServiceRefusal({ code: "rpc.bad_body", severity: "failed", text: "Write something first." });
        expect(session.settings().service.state).toBe("maintenance");
    });

    test("a Rock older than the platform's minimum is asked to update", () => {
        expect(serviceBanner(readSettings({ client: { update_required: true } }))).toBe("Chat needs a newer version of Rock. Ask your church's Rock administrator to update.");
        expect(serviceBanner(readSettings({}))).toBeNull();
    });

    test("a service state other than normal shows its banner, or a sentence of the client's own", () => {
        expect(serviceBanner(readSettings({ service: { state: "read_only", banner: "Read only tonight." } }))).toBe("Read only tonight.");
        expect(serviceBanner(readSettings({ service: { state: "maintenance" } }))).not.toBeNull();
        expect(serviceBanner(readSettings({ service: { state: "normal", banner: "ignored" } }))).toBeNull();
    });
});

describe("retrying a refresh that could not complete", () => {
    function failingOnce(failure: ExchangeResult): Partial<SessionDependencies> {
        let exchanges = 0;
        return {
            exchange: async (churchToken: string): Promise<ExchangeResult> => {
                exchanges++;
                return exchanges === 2
                    ? failure
                    : { ok: true, accessToken: `platform-for-${churchToken}`, expiresInSeconds: 300 };
            }
        };
    }

    test("a platform that says when to come back is asked again then", async () => {
        const f = fakes(failingOnce({ ok: false, status: 503, code: "rpc.unavailable", retryAfterSeconds: 7 } as ExchangeResult));
        const session = createSession(f.dependencies);
        await session.start();

        await session.refresh();

        expect(f.timers[f.timers.length - 1].milliseconds).toBe(7000);
    });

    test("without that, each failed try waits longer, from the base the settings name", async () => {
        let exchanges = 0;
        const f = fakes({
            exchange: async (churchToken: string): Promise<ExchangeResult> => {
                exchanges++;
                return exchanges === 1
                    ? { ok: true, accessToken: `platform-for-${churchToken}`, expiresInSeconds: 300, settings: { limits: { reconnect_base_ms: 2000 } } } as ExchangeResult
                    : { ok: false, status: 0, code: "rpc.transport" };
            }
        });
        const session = createSession(f.dependencies);
        await session.start();

        await session.refresh();
        const first = f.timers[f.timers.length - 1].milliseconds;
        f.timers[f.timers.length - 1].callback();
        await settle();
        const second = f.timers[f.timers.length - 1].milliseconds;

        expect(first).toBe(1500);
        expect(second).toBe(3000);
    });

    test("the wait is random, so clients that failed together do not all ask again together", async () => {
        const waits: number[] = [];
        for (const draw of [0.1, 0.9]) {
            const f = fakes({ ...failingOnce({ ok: false, status: 0, code: "rpc.transport" }), random: () => draw });
            const session = createSession(f.dependencies);
            await session.start();
            await session.refresh();
            waits.push(f.timers[f.timers.length - 1].milliseconds);
        }

        expect(waits[0]).not.toBe(waits[1]);
    });
});

// A failed exchange is asked again after the longer of the platform's Retry-After and the growing,
// jittered wait, never past the cap, and nothing asks before that time: the platform answering 503
// while it struggles is exactly when every refused call would otherwise start an exchange of its own.
describe("waiting after a refresh the platform could not serve", () => {
    /** A session whose exchanges after the first all fail as given, with a clock the test moves. */
    function failing(failure: ExchangeResult): { f: ReturnType<typeof fakes>, clock: { now: number }, ended: { count: number }, asked: { exchanges: number } } {
        const asked = { exchanges: 0 };
        const clock = { now: 0 };
        const ended = { count: 0 };
        const f = fakes({
            now: () => clock.now,
            onEnded: () => {
                ended.count++;
            },
            exchange: async (churchToken: string): Promise<ExchangeResult> => {
                asked.exchanges++;
                return asked.exchanges === 1
                    ? { ok: true, accessToken: `platform-for-${churchToken}`, expiresInSeconds: 300 }
                    : failure;
            }
        });
        return { f, clock, ended, asked };
    }

    const unavailable = (retryAfterSeconds: number | null): ExchangeResult => ({ ok: false, status: 503, code: "rpc.unavailable", retryAfterSeconds });

    test("a Retry-After longer than the first backoff is the wait", async () => {
        const { f } = failing(unavailable(8));
        const session = createSession(f.dependencies);
        await session.start();

        await session.refresh();

        expect(f.timers[f.timers.length - 1].milliseconds).toBe(8000);
    });

    test("once the backoff has grown past a short Retry-After, the backoff is the wait", async () => {
        const { f, clock } = failing(unavailable(2));
        const session = createSession(f.dependencies);
        await session.start();

        await session.refresh();
        for (let i = 0; i < 4; i++) {
            const timer = f.timers[f.timers.length - 1];
            clock.now += timer.milliseconds;
            timer.callback();
            await settle();
        }

        // The fifth failure's ceiling is 16 s from the 1 s base, and the random draw sits at its middle.
        expect(f.timers.slice(-5).map(t => t.milliseconds)).toEqual([2000, 2000, 3000, 6000, 12000]);
    });

    test("a Retry-After past the cap waits the cap", async () => {
        const { f } = failing(unavailable(99999));
        const session = createSession(f.dependencies);
        await session.start();

        await session.refresh();

        expect(f.timers[f.timers.length - 1].milliseconds).toBe(30000);
    });

    test("a failure at Rock's mint after the platform's 503 waits the backoff, not that old Retry-After", async () => {
        // The Retry-After was the platform's answer to one exchange; a later refresh that never
        // reached the platform has nothing from it to honour.
        let mints = 0;
        let exchanges = 0;
        const clock = { now: 0 };
        const f = fakes({
            now: () => clock.now,
            mintChurchToken: async (): Promise<ChurchTokenResult> => {
                mints++;
                return mints === 3
                    ? { gate: "gate_unavailable", churchToken: null, isUnreachable: true }
                    : { gate: "ok", churchToken: `church-${mints}` };
            },
            exchange: async (churchToken: string): Promise<ExchangeResult> => {
                exchanges++;
                return exchanges === 1
                    ? { ok: true, accessToken: `platform-for-${churchToken}`, expiresInSeconds: 300 }
                    : unavailable(8);
            }
        });
        const session = createSession(f.dependencies);
        await session.start();

        await session.refresh();
        const first = f.timers[f.timers.length - 1];
        expect(first.milliseconds).toBe(8000);
        // late enough that the church token the failed try left is too old to exchange, so the retry asks Rock
        clock.now += churchTokenReuseMs;
        first.callback();
        await settle();

        // The second failure's ceiling is 2 s from the 1 s base, and the random draw sits at its middle.
        expect(mints).toBe(3);
        expect(f.timers[f.timers.length - 1].milliseconds).toBe(1500);
    });

    test("a call refused as expired inside the wait does not mint or exchange", async () => {
        const { f, clock, asked } = failing(unavailable(8));
        const session = createSession(f.dependencies);
        await session.start();
        await session.refresh();
        const mints = f.mints;
        const exchanges = asked.exchanges;

        clock.now += 7999;
        const result = await session.withFreshToken(async () => "expired", r => r === "expired");
        expect(await session.refresh()).toBe(false);

        expect(result).toBe("expired");
        expect([f.mints, asked.exchanges]).toEqual([mints, exchanges]);

        // The wait's own timer asks again when it ends, with the church token the failed try left.
        f.timers[f.timers.length - 1].callback();
        await settle();
        expect([f.mints, asked.exchanges]).toEqual([mints, exchanges + 1]);
    });

    test("a run of failures keeps the live token until it expires, then ends the session once", async () => {
        const { f, clock, ended } = failing(unavailable(8));
        const session = createSession(f.dependencies);
        await session.start();

        clock.now = 150_000;
        await session.refresh();
        while (clock.now < 300_000) {
            expect(session.currentToken()).toBe("platform-for-church-1");
            const timer = f.timers[f.timers.length - 1];
            clock.now += timer.milliseconds;
            timer.callback();
            await settle();
        }

        expect(session.currentToken()).toBeNull();
        expect(ended.count).toBe(1);
        expect(f.timers.filter(t => !t.cleared)).toHaveLength(0);
    });
});

// A start the platform cannot answer yet keeps trying on the same wait a refresh takes, rather
// than giving up: a church opening chat during a short outage gets chat when it passes. Only a
// refusal ends a start. The exchange here is the real one over a fake network, so what the
// platform's 503, its Retry-After and a dead socket look like on the wire is what is read.
describe("a start the platform cannot answer yet", () => {
    /** What the network answers one exchange: a status, a request that never reaches anything, or one that never answers. */
    type Answer = { status: number, retryAfter?: number, code?: string } | "unreachable" | "hang";

    /** A mint that is Rock's answer, or one that never answers, or one the test answers itself. */
    type Mint = ChurchTokenResult | "hang" | Promise<ChurchTokenResult>;

    beforeEach(() => {
        jest.useFakeTimers({ now: 0 });
    });

    afterEach(() => {
        jest.useRealTimers();
    });

    function answer(reply: Answer, signal: AbortSignal | null | undefined): Promise<Response> {
        if (reply === "unreachable") {
            return Promise.reject(new TypeError("Failed to fetch"));
        }
        if (reply === "hang") {
            // a socket that never answers gives up only when it is aborted, as fetch does
            return new Promise((_, reject) => signal?.addEventListener("abort", () => reject(new DOMException("aborted", "AbortError"))));
        }
        const body = reply.status === 200
            ? { access_token: "platform-token", expires_in: 300, settings: {} }
            : { error: { code: reply.code ?? "rpc.unavailable" } };
        const headers: Record<string, string> = reply.retryAfter !== undefined ? { "retry-after": String(reply.retryAfter) } : {};
        return Promise.resolve({
            ok: reply.status >= 200 && reply.status < 300,
            status: reply.status,
            headers: { get: (name: string) => headers[name.toLowerCase()] ?? null },
            json: async () => body
        } as unknown as Response);
    }

    /** The platform answering each exchange with the next answer, then 200; Rock answering each mint with the next mint, then ok. */
    function platform(answers: Answer[], options: { random?: number, mints?: Mint[], onEnded?: () => void } = {}): {
        dependencies: SessionDependencies,
        exchanges: Array<{ churchToken: string, at: number, signal: AbortSignal | null }>,
        mints: number[],
        waits: number[],
        away: { value: boolean }
    } {
        // the person is away (hidden or offline) unless a test says they are watching
        const away = { value: true };
        const exchanges: Array<{ churchToken: string, at: number, signal: AbortSignal | null }> = [];
        const mints: number[] = [];
        const waits: number[] = [];
        const dependencies: SessionDependencies = {
            mintChurchToken: (): Promise<ChurchTokenResult> => {
                mints.push(Date.now());
                const next = options.mints?.shift();
                if (next === "hang") {
                    return new Promise(() => undefined);
                }
                return Promise.resolve(next ?? { gate: "ok", churchToken: `church-${mints.length}` });
            },
            exchange: (churchToken: string, options: { signal: AbortSignal }): Promise<ExchangeResult> => {
                exchanges.push({ churchToken, at: Date.now(), signal: options.signal });
                const reply = answers.shift() ?? { status: 200 };
                return exchangeChurchToken((_url, init) => answer(reply, init.signal), "http://platform", "k", churchToken, { signal: options.signal, now: Date.now() });
            },
            pushTokenToConnection: async () => undefined,
            random: () => options.random ?? 0.5,
            setTimer: (callback, milliseconds) => setTimeout(callback, milliseconds),
            clearTimer: handle => clearTimeout(handle as ReturnType<typeof setTimeout>),
            now: () => Date.now(),
            onWaiting: until => waits.push(until),
            onEnded: options.onEnded,
            isAway: () => away.value
        };
        return { dependencies, exchanges, mints, waits, away };
    }

    /** Starts, keeping how the start resolved, or null while it has not. */
    function begin(session: ReturnType<typeof createSession>): { result: boolean | null } {
        const outcome: { result: boolean | null } = { result: null };
        void session.start().then(result => outcome.result = result);
        return outcome;
    }

    const busy = (retryAfter?: number): Answer => ({ status: 503, retryAfter, code: "rpc.unavailable" });

    test("a start the exchange answers 503 with a Retry-After waits exactly that long and signs in on the next answer", async () => {
        const p = platform([busy(4)]);
        const session = createSession(p.dependencies);

        const started = begin(session);
        await jest.advanceTimersByTimeAsync(3999);

        // the backoff of a first failure (half to all of one second) is shorter, so the server's wait is the one
        expect(started.result).toBeNull();
        expect(p.exchanges).toHaveLength(1);

        await jest.advanceTimersByTimeAsync(1);

        expect(p.exchanges.map(e => e.at)).toEqual([0, 4000]);
        expect(started.result).toBe(true);
        expect(session.currentToken()).toBe("platform-token");
    });

    test("each wait is the longer of the Retry-After and a backoff that grows, never past the cap", async () => {
        const p = platform([busy(2), busy(2), busy(2), busy(2)]);
        const session = createSession(p.dependencies);

        begin(session);
        await jest.advanceTimersByTimeAsync(20000);

        // backoffs at the draw's middle are 0.75, 1.5, 3 and 6 s; a 2 s floor lifts the first two
        expect(p.exchanges.map(e => e.at)).toEqual([0, 2000, 4000, 7000, 13000]);

        const capped = platform([busy(99)]);
        begin(createSession(capped.dependencies));
        await jest.advanceTimersByTimeAsync(30000);

        expect(capped.exchanges.map(e => e.at - capped.exchanges[0].at)).toEqual([0, 30000]);
    });

    test("nothing asks for a token inside a start's wait, however often a refresh or a call refused as expired asks", async () => {
        const p = platform([busy(8)]);
        const session = createSession(p.dependencies);

        begin(session);
        await jest.advanceTimersByTimeAsync(1000);
        const refreshes = await Promise.all([session.refresh(), session.refresh()]);
        const call = await session.withFreshToken(async () => "expired", r => r === "expired");

        expect(refreshes).toEqual([false, false]);
        expect(call).toBe("expired");
        expect(p.mints).toHaveLength(1);
        expect(p.exchanges).toHaveLength(1);
    });

    test("a start the platform refuses resolves false after one exchange and leaves nothing scheduled", async () => {
        const p = platform([{ status: 401, code: "auth.invalid_token" }]);
        const session = createSession(p.dependencies);

        const started = begin(session);
        await jest.advanceTimersByTimeAsync(0);

        expect(started.result).toBe(false);
        expect(p.exchanges).toHaveLength(1);
        expect(jest.getTimerCount()).toBe(0);
    });

    test("a refresh whose fresh church token is called stale twice in a row ends the session and schedules nothing", async () => {
        let ended = 0;
        const p = platform([{ status: 200 }, { status: 401, code: "auth.stale_token" }, { status: 401, code: "auth.stale_token" }], { onEnded: () => ended++ });
        const session = createSession(p.dependencies);
        begin(session);
        await jest.advanceTimersByTimeAsync(0);

        // asking a third time cannot mend a clock or a key Rock signs with wrongly
        expect(await session.refresh()).toBe(false);

        expect(ended).toBe(1);
        expect(session.currentToken()).toBeNull();
        expect(jest.getTimerCount()).toBe(0);
    });

    test("a start that cannot reach Rock waits the backoff and asks Rock again", async () => {
        const p = platform([], { mints: [{ gate: "gate_unavailable", churchToken: null, isUnreachable: true }] });
        const session = createSession(p.dependencies);

        const started = begin(session);
        await jest.advanceTimersByTimeAsync(749);
        expect(p.mints).toEqual([0]);
        expect(started.result).toBeNull();

        await jest.advanceTimersByTimeAsync(1);

        expect(p.mints).toEqual([0, 750]);
        expect(started.result).toBe(true);
    });

    test("a stop inside a start's wait resolves the start false, clears its timer and asks for nothing after", async () => {
        const p = platform([busy(4)]);
        const session = createSession(p.dependencies);
        const started = begin(session);
        await jest.advanceTimersByTimeAsync(1000);

        session.stop();
        await jest.advanceTimersByTimeAsync(0);

        expect(started.result).toBe(false);
        expect(jest.getTimerCount()).toBe(0);
        await jest.advanceTimersByTimeAsync(60000);
        expect(p.mints).toHaveLength(1);
        expect(p.exchanges).toHaveLength(1);
    });

    test("a stop while a try is out aborts its exchange and resolves the start false", async () => {
        const p = platform(["hang"]);
        const session = createSession(p.dependencies);
        const started = begin(session);
        await jest.advanceTimersByTimeAsync(1000);

        session.stop();
        await jest.advanceTimersByTimeAsync(0);

        expect(p.exchanges[0].signal?.aborted).toBe(true);
        expect(started.result).toBe(false);
        expect(jest.getTimerCount()).toBe(0);
    });

    test("a nudge past the platform's floor brings the next try to half a second to a second away", async () => {
        const p = platform(["unreachable", "unreachable", "unreachable", "unreachable"]);
        const session = createSession(p.dependencies);
        begin(session);

        // waits of 0.75, 1.5 and 3 s, then a fourth failure at 5.25 s waits 6 s, to 11.25 s
        await jest.advanceTimersByTimeAsync(6250);
        expect(p.exchanges).toHaveLength(4);
        session.nudge();
        await jest.advanceTimersByTimeAsync(749);
        expect(p.exchanges).toHaveLength(4);
        await jest.advanceTimersByTimeAsync(1);

        expect(p.exchanges.map(e => e.at)).toEqual([0, 750, 2250, 5250, 7000]);
    });

    test("a nudge inside the platform's Retry-After waits for it to end, not past it", async () => {
        const p = platform([busy(2), busy(2), busy(2), busy(2)]);
        const session = createSession(p.dependencies);
        begin(session);

        // the fourth failure at 7 s waits the 6 s backoff, though the platform asked for only 2 s
        await jest.advanceTimersByTimeAsync(7500);
        session.nudge();
        await jest.advanceTimersByTimeAsync(1499);
        expect(p.exchanges).toHaveLength(4);
        await jest.advanceTimersByTimeAsync(1);

        expect(p.exchanges[4].at).toBe(9000);
    });

    test("every failed try of a start is told with the time of the next", async () => {
        const p = platform([busy(4), busy(4)]);
        const session = createSession(p.dependencies);

        begin(session);
        await jest.advanceTimersByTimeAsync(10000);

        expect(p.waits).toEqual([4000, 8000]);
    });

    test("an exchange that never answers is given up at ten seconds and tried again on the backoff", async () => {
        const p = platform(["hang"]);
        const session = createSession(p.dependencies);

        const started = begin(session);
        await jest.advanceTimersByTimeAsync(tryTimeoutMs - 1);
        expect(p.exchanges[0].signal?.aborted).toBe(false);
        await jest.advanceTimersByTimeAsync(1);
        expect(p.exchanges[0].signal?.aborted).toBe(true);

        await jest.advanceTimersByTimeAsync(750);

        expect(p.exchanges.map(e => e.at)).toEqual([0, tryTimeoutMs + 750]);
        expect(started.result).toBe(true);
    });

    test("a mint that never answers is given up the same way, and its late answer changes nothing", async () => {
        let late!: (result: ChurchTokenResult) => void;
        const p = platform([], { mints: [new Promise<ChurchTokenResult>(resolve => late = resolve)] });
        const session = createSession(p.dependencies);

        const started = begin(session);
        await jest.advanceTimersByTimeAsync(tryTimeoutMs + 750);

        expect(p.mints).toEqual([0, tryTimeoutMs + 750]);
        expect(started.result).toBe(true);

        late({ gate: "ok", churchToken: "church-late" });
        await jest.advanceTimersByTimeAsync(0);

        expect(p.exchanges.map(e => e.churchToken)).toEqual(["church-2"]);
    });

    test("a later try exchanges the church token Rock signed while it is young, and mints anew once it is not", async () => {
        const p = platform([busy(30), busy(30), busy(30), busy(30), busy(30), busy(30)]);
        const session = createSession(p.dependencies);

        begin(session);
        await jest.advanceTimersByTimeAsync(60000);

        expect(p.mints).toEqual([0]);
        expect(p.exchanges.map(e => e.churchToken)).toEqual(["church-1", "church-1", "church-1"]);

        await jest.advanceTimersByTimeAsync(120000);

        // at 150 s the first is too old to trust at the exchange, so the try mints
        expect(p.mints).toEqual([0, churchTokenReuseMs]);
        expect(p.exchanges.filter(e => e.at >= churchTokenReuseMs).map(e => e.churchToken)).toEqual(["church-2", "church-2"]);
    });

    test("a church token held over and called stale is minted again at once in the same try", async () => {
        const p = platform([busy(4), { status: 401, code: "auth.stale_token" }]);
        const session = createSession(p.dependencies);

        const started = begin(session);
        await jest.advanceTimersByTimeAsync(4000);

        expect(p.mints).toEqual([0, 4000]);
        expect(p.exchanges.map(e => [e.churchToken, e.at])).toEqual([["church-1", 0], ["church-1", 4000], ["church-2", 4000]]);
        expect(started.result).toBe(true);
    });

    test("a fresh church token called stale straight after a held one was is a refusal", async () => {
        const p = platform([busy(4), { status: 401, code: "auth.stale_token" }, { status: 401, code: "auth.stale_token" }]);
        const session = createSession(p.dependencies);

        const started = begin(session);
        await jest.advanceTimersByTimeAsync(4000);

        expect(started.result).toBe(false);
        expect(p.exchanges).toHaveLength(3);
        expect(jest.getTimerCount()).toBe(0);
    });

    test("a nudge while a try is out starts no second try", async () => {
        const p = platform(["hang"]);
        const session = createSession(p.dependencies);
        begin(session);
        await jest.advanceTimersByTimeAsync(1000);

        session.nudge();
        await jest.advanceTimersByTimeAsync(2000);

        expect(p.exchanges).toHaveLength(1);
    });

    test("two nudges in one wait move it once", async () => {
        const draws = [0.5, 0.5, 0.5, 0.5, 0.5, 0];
        const p = platform(["unreachable", "unreachable", "unreachable", "unreachable"]);
        p.dependencies.random = () => draws.shift() ?? 0;
        const session = createSession(p.dependencies);
        begin(session);
        await jest.advanceTimersByTimeAsync(5300);

        // the first nudge draws the middle, to 6.05 s; a second, drawing the least, would bring it to 5.9 s
        session.nudge();
        await jest.advanceTimersByTimeAsync(100);
        session.nudge();
        await jest.advanceTimersByTimeAsync(649);
        expect(p.exchanges).toHaveLength(4);
        await jest.advanceTimersByTimeAsync(1);

        expect(p.exchanges.map(e => e.at)).toEqual([0, 750, 2250, 5250, 6050]);
    });

    test("a person flicking between tabs while every try fails in front of them moves none of the waits that began while they watched", async () => {
        const p = platform(["unreachable", "unreachable", "unreachable", "unreachable"]);
        p.away.value = false;
        const session = createSession(p.dependencies);
        begin(session);

        // three waits, each nudged twice as the page is hidden and shown again
        for (const at of [100, 1000, 2400]) {
            await jest.advanceTimersByTimeAsync(at - Date.now());
            session.nudge();
            session.nudge();
        }
        await jest.advanceTimersByTimeAsync(5250 - Date.now());

        expect(p.exchanges.map(e => e.at)).toEqual([0, 750, 2250, 5250]);
    });

    test("a wait that began while the person was away is brought forward when they come back", async () => {
        const p = platform(["unreachable", "unreachable", "unreachable", "unreachable"]);
        p.away.value = false;
        const session = createSession(p.dependencies);
        begin(session);
        await jest.advanceTimersByTimeAsync(2249);

        // the third try fails while the page is hidden, so its 3 s wait may be cut short
        p.away.value = true;
        await jest.advanceTimersByTimeAsync(1);
        expect(p.exchanges).toHaveLength(3);
        await jest.advanceTimersByTimeAsync(250);
        p.away.value = false;
        session.nudge();
        await jest.advanceTimersByTimeAsync(750);

        expect(p.exchanges.map(e => e.at)).toEqual([0, 750, 2250, 3250]);
    });

    test("a gateway's 408 is no answer, which may pass, not a refusal", async () => {
        const p = platform([{ status: 408, code: "rpc.timeout" }]);
        const session = createSession(p.dependencies);

        const started = begin(session);
        await jest.advanceTimersByTimeAsync(750);

        expect(p.exchanges).toHaveLength(2);
        expect(started.result).toBe(true);
    });
});
