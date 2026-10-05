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
    ChurchTokenResult,
    createSession,
    ExchangeResult,
    SessionDependencies
} from "../../../../../src/Communication/Chat/ChatShell/composables/useSession.partial";
import * as sessionModule from "../../../../../src/Communication/Chat/ChatShell/composables/useSession.partial";
import { ChatError } from "../../../../../src/Communication/Chat/ChatShell/types.partial";

/** The settings the token exchange answers beside the token, every key filled. */
type ChatSettings = {
    service: { state: string, banner: string | null };
    client: { update_required: boolean };
    routes: { send: string };
    limits: { reconnect_base_ms: number, reconnect_cap_ms: number, catch_up_page: number, catch_up_max: number };
    flags: Record<string, unknown>;
};

/** What the session module offers for the settings, beside the session itself. */
const { readSettings, serviceBanner } = sessionModule as unknown as {
    readSettings: (raw: unknown) => ChatSettings,
    serviceBanner: (settings: ChatSettings) => string | null
};

/** The session with the settings it holds. */
type SettingsSession = ReturnType<typeof createSession> & {
    settings: () => ChatSettings,
    applyServiceRefusal: (error: ChatError & { text?: string | null }) => void
};

type Timer = { callback: () => void, milliseconds: number, cleared: boolean };

function fakes(overrides: Partial<SessionDependencies> = {}): { dependencies: SessionDependencies, timers: Timer[], mints: number, exchanges: string[], pushes: number } {
    const record = { timers: [] as Timer[], mints: 0, exchanges: [] as string[], pushes: 0 };

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

        expect(session.currentToken()).toBe("platform-for-church-3");
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
        routes: { send: "direct" },
        limits: { reconnect_base_ms: 1000, reconnect_cap_ms: 30000, catch_up_page: 100, catch_up_max: 500 },
        flags: {}
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
        expect(serviceBanner(readSettings({ client: { update_required: true } }))).toMatch(/update Rock/);
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
