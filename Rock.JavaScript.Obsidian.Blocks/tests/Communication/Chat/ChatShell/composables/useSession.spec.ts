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
        expect(retry.milliseconds).toBe(300_000 * 0.05);

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
        expect(f.timers[f.timers.length - 1].milliseconds).toBe(300_000 * 0.05);
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
});
