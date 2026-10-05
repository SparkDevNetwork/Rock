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
//
// Every exchange also answers the platform's settings for this church, which is how a client
// released years ago still learns the service state, the route of the send and its limits: it is
// the one call every client already makes at load and every few minutes.
import { computeBackoff } from "../platformCall.partial";
import { ChatError } from "../types.partial";

/** The settings the token exchange answers, every key filled. Names are the platform's own. */
/* eslint-disable @typescript-eslint/naming-convention */
export type ChatSettings = {
    service: { state: string, banner: string | null };
    client: { update_required: boolean };
    routes: { send: string };
    limits: { reconnect_base_ms: number, reconnect_cap_ms: number, catch_up_page: number, catch_up_max: number };
    flags: Record<string, unknown>;
};
/* eslint-enable @typescript-eslint/naming-convention */

/** The service states this client knows; any other is read as normal. */
const serviceStates = ["normal", "degraded", "read_only", "maintenance"];

/** The routes this client can take; any other is read as direct, the route every release has. */
const sendRoutes = ["direct", "edge"];

/**
 * The defaults when the platform sends nothing for a key. The reconnect and catch-up figures are
 * estimates, not measurements; the platform can change them without a release.
 */
const defaultLimits: ChatSettings["limits"] = {
    reconnect_base_ms: 1000,
    reconnect_cap_ms: 30000,
    catch_up_page: 100,
    catch_up_max: 500
};

/** An object's own value for a key, or undefined when the value is not an object. */
function section(value: unknown, key: string): Record<string, unknown> {
    const outer = value && typeof value === "object" ? (value as Record<string, unknown>)[key] : undefined;

    return outer && typeof outer === "object" && !Array.isArray(outer) ? outer as Record<string, unknown> : {};
}

/**
 * Reads the settings the exchange answered. Every key may be missing or of a shape this client
 * does not know, since the platform keeps changing after a release; each falls back to its
 * default on its own, so one bad key never costs the rest.
 *
 * @param raw The settings as the exchange answered them.
 *
 * @returns The settings with every key filled.
 */
export function readSettings(raw: unknown): ChatSettings {
    const service = section(raw, "service");
    const client = section(raw, "client");
    const routes = section(raw, "routes");
    const limits = section(raw, "limits");
    const flags = section(raw, "flags");

    const limit = (key: keyof ChatSettings["limits"]): number => {
        const value = limits[key];
        return typeof value === "number" && value > 0 ? value : defaultLimits[key];
    };

    return {
        service: {
            state: typeof service.state === "string" && serviceStates.includes(service.state) ? service.state : "normal",
            banner: typeof service.banner === "string" ? service.banner : null
        },
        client: { update_required: client.update_required === true },
        routes: { send: typeof routes.send === "string" && sendRoutes.includes(routes.send) ? routes.send : "direct" },
        limits: {
            reconnect_base_ms: limit("reconnect_base_ms"),
            reconnect_cap_ms: limit("reconnect_cap_ms"),
            // history serves 1 to 100 messages a page and refuses anything else, so a setting
            // outside that would fail every catch-up
            catch_up_page: Math.min(100, Math.max(1, Math.round(limit("catch_up_page")))),
            catch_up_max: limit("catch_up_max")
        },
        flags: { ...flags }
    };
}

/**
 * The banner the page shows, or null for none. The platform's own sentence wins; this client's
 * sentence is only for a state the platform sent without one.
 *
 * @param settings The settings.
 *
 * @returns The sentence, or null.
 */
export function serviceBanner(settings: ChatSettings): string | null {
    if (settings.client.update_required) {
        return "This version of Rock is too old for chat. Please ask your church to update Rock.";
    }

    if (settings.service.state === "normal") {
        return null;
    }

    if (settings.service.banner) {
        return settings.service.banner;
    }

    switch (settings.service.state) {
        case "read_only":
            return "Chat is read only for now. You can read messages but not send them.";
        case "maintenance":
            return "Chat is down for maintenance. Try again later.";
        default:
            return "Chat is running slowly right now.";
    }
}

/** What the Rock token action answers. */
export type ChurchTokenResult = {
    gate: string;
    churchToken: string | null;

    /**
     * True when Rock could not be asked at all, so the gate is unknown rather than refused. A
     * session that still holds a token keeps it and asks again.
     */
    isUnreachable?: boolean;
};

/** What the platform's token exchange answers. */
export type ExchangeResult =
    | { ok: true, accessToken: string, expiresInSeconds: number, settings?: unknown }
    | { ok: false, status: number, code: string, retryAfterSeconds?: number | null };

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

    /** The time now in milliseconds; the browser's clock unless a test stands in. */
    now?: () => number;

    /**
     * Called when a session that held a token no longer does: Rock refused a refresh, or the
     * token ran out before one succeeded. Whatever the token opened should be closed.
     */
    onEnded?: () => void;
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

    /** Stops the session for good: no refresh runs, is scheduled or is handed over after this. */
    stop: () => void;

    /** The settings the latest exchange answered. */
    settings: () => ChatSettings;

    /**
     * A refusal for read only or maintenance means the service state changed since the last
     * exchange, and its sentence is the banner, so the page shows it without waiting for the
     * next refresh.
     */
    applyServiceRefusal: (error: ChatError) => void;

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
    let expiresAt = 0;
    let settings = readSettings(undefined);

    // Failed refreshes in a row, which sets how long the next wait is, and how long the platform
    // asked to be left alone after the last one, when it said.
    let failures = 0;
    let retryAfterSeconds: number | null = null;
    let refreshing: Promise<boolean> | null = null;
    let isStopped = false;
    const now = dependencies.now ?? ((): number => Date.now());

    /**
     * Mints and exchanges, reminting once when the platform calls the church token stale. A gate
     * that refused, or a platform that refused the church token, is final; a Rock or a platform
     * that could not answer this time is not, and says nothing about the token already held.
     */
    async function acquire(): Promise<"ok" | "refused" | "unavailable"> {
        for (let attempt = 0; attempt < 2; attempt++) {
            const minted = await dependencies.mintChurchToken();

            // Stopped while Rock answered: nothing more is asked, and nothing it said is kept.
            if (isStopped) {
                return "unavailable";
            }

            if (minted.isUnreachable) {
                // Only a session with nothing yet shows it; one that holds a token carries on.
                if (token === null) {
                    state.gate = minted.gate;
                }
                return "unavailable";
            }

            state.gate = minted.gate;
            if (minted.gate !== "ok" || !minted.churchToken) {
                return "refused";
            }

            const exchanged = await dependencies.exchange(minted.churchToken);
            if (isStopped) {
                return "unavailable";
            }
            if (exchanged.ok) {
                token = exchanged.accessToken;
                expiresAt = now() + exchanged.expiresInSeconds * 1000;
                settings = readSettings(exchanged.settings);
                state.errorCode = null;
                failures = 0;
                schedule(exchanged.expiresInSeconds);
                return "ok";
            }

            state.errorCode = exchanged.code;
            retryAfterSeconds = exchanged.retryAfterSeconds ?? null;

            // A stale church token only means Rock signed it too long ago; a fresh one is
            // worth one more try at once.
            if (exchanged.code === "auth.stale_token") {
                continue;
            }

            // No answer, too many requests, or a server fault may pass; any other refusal is
            // the platform saying no to this church token, and asking again would only repeat it.
            const isPassing = exchanged.status === 0 || exchanged.status === 429 || exchanged.status >= 500;
            return isPassing ? "unavailable" : "refused";
        }

        return "unavailable";
    }

    /**
     * Mints and exchanges, and settles what the session holds by the outcome: a refusal ends the
     * session; a failure that may pass keeps a token that is still alive and asks again soon,
     * since the token has at least a tenth of its life left when a refresh runs. Once the token
     * has expired there is nothing left to keep.
     */
    async function renew(): Promise<boolean> {
        const outcome = await acquire();

        // A stopped session has already been closed by whoever stopped it; it is not ended again.
        if (isStopped) {
            return false;
        }

        if (outcome === "ok") {
            return true;
        }

        if (outcome === "refused" || token === null || now() >= expiresAt) {
            const wasHeld = token !== null;
            token = null;
            clear();
            if (wasHeld) {
                dependencies.onEnded?.();
            }
            return false;
        }

        retryLater();
        return false;
    }

    /**
     * Schedules the next refresh at a random point in the permitted window of the token's
     * life, so that everyone who opened chat at the same moment does not refresh together.
     */
    function schedule(expiresInSeconds: number): void {
        clear();
        if (isStopped) {
            return;
        }
        const fraction = refreshWindowStart + (refreshWindowEnd - refreshWindowStart) * dependencies.random();
        timer = dependencies.setTimer(() => void refresh(), expiresInSeconds * 1000 * fraction);
    }

    /**
     * Schedules another refresh after a failed one: when the platform said to come back, or else
     * after a random wait that grows with each failure in a row, so the clients of every church
     * that failed together do not all ask again in the same second.
     */
    function retryLater(): void {
        clear();
        if (isStopped) {
            return;
        }
        const wait = retryAfterSeconds !== null
            ? retryAfterSeconds * 1000
            : computeBackoff(failures, settings.limits.reconnect_base_ms, settings.limits.reconnect_cap_ms, dependencies.random);
        failures++;
        timer = dependencies.setTimer(() => void refresh(), wait);
    }

    /** Stops the pending refresh, if there is one. */
    function clear(): void {
        if (timer !== null) {
            dependencies.clearTimer(timer);
            timer = null;
        }
    }

    /**
     * Mints and exchanges again and hands the new token to the open connection. A refresh asked
     * for while one is running shares it, so the timer and an expired call never race each other
     * to decide what the session holds.
     */
    function refresh(): Promise<boolean> {
        // A stopped session asks Rock for nothing more, whatever still calls it: an expired call
        // in flight, or a timer that fired late.
        if (isStopped) {
            return Promise.resolve(false);
        }

        refreshing ??= (async (): Promise<boolean> => {
            if (!await renew() || isStopped) {
                return false;
            }

            await dependencies.pushTokenToConnection();
            return true;
        })().finally(() => refreshing = null);

        return refreshing;
    }

    return {
        start: renew,
        currentToken: () => token,
        refresh,
        withFreshToken: async <T>(call: () => Promise<T>, isExpired: (result: T) => boolean): Promise<T> => {
            const result = await call();
            if (!isExpired(result) || !await refresh()) {
                return result;
            }

            return call();
        },
        stop: (): void => {
            isStopped = true;
            clear();
        },
        settings: () => settings,
        applyServiceRefusal: (error: ChatError): void => {
            if (error.code === "rpc.read_only" || error.code === "rpc.maintenance") {
                settings = { ...settings, service: { state: error.code.slice("rpc.".length), banner: error.text ?? null } };
            }
        },
        state
    };
}
