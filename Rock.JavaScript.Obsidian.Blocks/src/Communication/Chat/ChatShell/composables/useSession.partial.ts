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
// The person's session on the chat platform. Rock signs a short church token after running its
// gates; the platform exchanges it for its own token; the platform token is refreshed the same
// way before it expires, so every refresh passes through Rock's gates again.

/** What the Rock token action answers. */
export type ChurchTokenResult = {
    gate: string;
    churchToken: string | null;
};

/** What the platform's token exchange answers. */
export type ExchangeResult =
    | { ok: true, accessToken: string, expiresInSeconds: number }
    | { ok: false, status: number, code: string };

/** Everything the session reaches outside itself, so a test can stand in for each. */
export type SessionDependencies = {
    /** Asks Rock for a church token. Every ask re-runs the gates. */
    mintChurchToken: () => Promise<ChurchTokenResult>;

    /** Exchanges a church token for a platform token. */
    exchange: (churchToken: string) => Promise<ExchangeResult>;

    /**
     * Hands the current token to the live connection that is already open. Called after every
     * refresh; the connection reads the token back through the client's own token callback.
     */
    pushTokenToConnection: () => Promise<void>;

    /** A number from 0 up to 1, where to refresh within the permitted window. */
    random: () => number;

    /** Starts a timer. */
    setTimer: (callback: () => void, milliseconds: number) => unknown;

    /** Stops a timer. */
    clearTimer: (handle: unknown) => void;
};

/** Why the session is not running, when it is not. */
export type SessionState = {
    gate: string | null;
    errorCode: string | null;
};

/** The session a shell holds. */
export type ChatSession = {
    /** Mints, exchanges and schedules the first refresh. Resolves false when a gate refused. */
    start: () => Promise<boolean>;

    /** The current platform token, for the client's token callback and a save on close. */
    currentToken: () => string | null;

    /** Mints and exchanges again and hands the new token to the open connection. */
    refresh: () => Promise<boolean>;

    /**
     * Runs a platform call, and if the platform refuses the token as expired, refreshes once
     * and runs it again.
     */
    withFreshToken: <T>(call: () => Promise<T>, isExpired: (result: T) => boolean) => Promise<T>;

    /** Stops the refresh timer. */
    stop: () => void;

    /** Why the session is not running. */
    state: SessionState;
};

/** The earliest point in a token's life at which it is refreshed. */
export const refreshWindowStart = 0.5;

/** The latest point in a token's life at which it is refreshed. */
export const refreshWindowEnd = 0.9;

/**
 * Creates the session.
 *
 * @param dependencies What the session reaches outside itself.
 *
 * @returns The session.
 */
export function createSession(dependencies: SessionDependencies): ChatSession {
    const state: SessionState = { gate: null, errorCode: null };
    let token: string | null = null;
    let timer: unknown = null;

    /** Mints and exchanges, reminting once when the platform calls the church token stale. */
    async function acquire(): Promise<boolean> {
        for (let attempt = 0; attempt < 2; attempt++) {
            const minted = await dependencies.mintChurchToken();
            state.gate = minted.gate;

            if (minted.gate !== "ok" || !minted.churchToken) {
                token = null;
                return false;
            }

            const exchanged = await dependencies.exchange(minted.churchToken);
            if (exchanged.ok) {
                token = exchanged.accessToken;
                state.errorCode = null;
                schedule(exchanged.expiresInSeconds);
                return true;
            }

            state.errorCode = exchanged.code;

            // A stale church token only means Rock signed it too long ago; a fresh one is
            // worth one more try, and anything else is not.
            if (exchanged.code !== "auth.stale_token") {
                break;
            }
        }

        token = null;
        return false;
    }

    /**
     * Schedules the next refresh at a random point in the permitted window of the token's
     * life, so that everyone who opened chat at the same moment does not refresh together.
     */
    function schedule(expiresInSeconds: number): void {
        clear();
        const fraction = refreshWindowStart + (refreshWindowEnd - refreshWindowStart) * dependencies.random();
        timer = dependencies.setTimer(() => {
            timer = null;
            void refresh();
        }, expiresInSeconds * 1000 * fraction);
    }

    /** Stops the pending refresh, if there is one. */
    function clear(): void {
        if (timer !== null) {
            dependencies.clearTimer(timer);
            timer = null;
        }
    }

    /** Mints and exchanges again and hands the new token to the open connection. */
    async function refresh(): Promise<boolean> {
        if (!await acquire()) {
            clear();
            return false;
        }

        await dependencies.pushTokenToConnection();
        return true;
    }

    return {
        start: acquire,
        currentToken: () => token,
        refresh,
        withFreshToken: async <T>(call: () => Promise<T>, isExpired: (result: T) => boolean): Promise<T> => {
            const result = await call();
            if (!isExpired(result) || !await refresh()) {
                return result;
            }

            return call();
        },
        stop: clear,
        state
    };
}
