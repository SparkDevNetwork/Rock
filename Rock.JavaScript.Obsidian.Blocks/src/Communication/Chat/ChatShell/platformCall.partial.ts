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
// The one way the chat client reaches the platform. Every call is a POST: straight to the
// database function, or, for the send when the platform's settings say so, to the platform's own
// send gate with the same arguments. A released client cannot be changed for years, so the route
// is the server's to choose, and a failure on one route is reported, never retried on the other:
// a client that picked for itself would undo the reason the server moved it.
//
// The token exchange lives here too, because it is the other half of how the client reaches the
// platform and reads the same refusals, waits and settings.
import { classifyPlatformError } from "./errors.partial";
import { ChatError, TokenExchangeResponse } from "./types.partial";

/** What a platform call answers: the data, or the refusal with how long to wait if it said. */
export type CallResult =
    | { ok: true, data: unknown }
    | { ok: false, status: number, error: ChatError, retryAfterSeconds: number | null };

/** What the call path reaches outside itself. */
export type PlatformCallOptions = {
    fetch: (url: string, init: RequestInit) => Promise<Response>;
    projectUrl: string;
    publishableKey: string;

    /** The current platform token, read at the moment of each call. */
    currentToken: () => string | null;

    /** The route of each operation that has more than one, from the settings. */
    routes: () => { send: string };

    /** The time now in milliseconds; the browser's clock unless a test stands in. */
    now?: () => number;
};

/** The call path a shell holds. */
export type PlatformCall = {
    /**
     * Calls a platform function with its own arguments.
     *
     * @param name The function, as the database names it.
     * @param args Its arguments, with their p_ names.
     * @param options keepalive for a call that must survive the page closing.
     */
    call: (name: string, args: Record<string, unknown>, options?: { keepalive?: boolean }) => Promise<CallResult>;
};

/** The operations a route in the settings can move; every other call goes straight to the database. */
const routedFunctions: Record<string, { setting: "send", gatePath: string }> = {
    chat_send_message: { setting: "send", gatePath: "/functions/v1/chat-send" }
};

/**
 * Reads a Retry-After header: a number of seconds, or an HTTP date.
 *
 * @param value The header, or null.
 * @param now The time now in milliseconds.
 *
 * @returns The seconds to wait, or null when the header says nothing usable.
 */
export function parseRetryAfter(value: string | null, now: number): number | null {
    if (!value) {
        return null;
    }

    if (/^\d+$/.test(value.trim())) {
        return Number(value.trim());
    }

    const at = Date.parse(value);
    if (Number.isNaN(at) || at <= now) {
        return null;
    }

    return Math.ceil((at - now) / 1000);
}

/**
 * How long to wait before trying again: between half and all of a ceiling that doubles from the
 * base with each failed try and never passes the cap. The random half spreads clients that
 * failed together, so they do not all come back in the same second.
 *
 * @param attempt How many tries have failed before this wait, from zero.
 * @param baseMs The first ceiling.
 * @param capMs The largest ceiling.
 * @param random A number from 0 up to 1.
 *
 * @returns The wait in milliseconds.
 */
export function computeBackoff(attempt: number, baseMs: number, capMs: number, random: () => number): number {
    const ceiling = Math.min(capMs, baseMs * Math.pow(2, attempt));

    return ceiling / 2 + random() * ceiling / 2;
}

/**
 * Turns a refused response's body into our code and the server's sentence. The database's API
 * carries our code in message and the sentence in hint; an Edge function carries both in its
 * error envelope.
 */
function refusal(status: number, body: unknown): ChatError {
    const record = body && typeof body === "object" ? body as Record<string, unknown> : {};
    const envelope = record.error && typeof record.error === "object" ? record.error as Record<string, unknown> : null;

    if (envelope) {
        return classifyPlatformError({ message: envelope.code as string, hint: envelope.message as string, status });
    }

    return classifyPlatformError({ ...record, status });
}

/**
 * Creates the call path.
 *
 * @param options What the call path reaches outside itself.
 *
 * @returns The call path.
 */
export function createPlatformCall(options: PlatformCallOptions): PlatformCall {
    const projectUrl = options.projectUrl.replace(/\/+$/, "");
    const now = options.now ?? ((): number => Date.now());

    return {
        call: async (name, args, callOptions): Promise<CallResult> => {
            const token = options.currentToken();
            if (!token) {
                return { ok: false, status: 401, error: { code: "auth.expired", severity: "session" }, retryAfterSeconds: null };
            }

            const routed = routedFunctions[name];
            const url = routed && options.routes()[routed.setting] === "edge"
                ? `${projectUrl}${routed.gatePath}`
                : `${projectUrl}/rest/v1/rpc/${name}`;

            let response: Response;
            try {
                response = await options.fetch(url, {
                    method: "POST",
                    keepalive: callOptions?.keepalive === true,
                    headers: {
                        "Authorization": `Bearer ${token}`,
                        "apikey": options.publishableKey,
                        "Content-Type": "application/json"
                    },
                    body: JSON.stringify(args)
                });
            }
            catch (error) {
                return { ok: false, status: 0, error: classifyPlatformError(error instanceof TypeError ? error : new TypeError(String(error))), retryAfterSeconds: null };
            }

            const body = await response.json().catch(() => null) as unknown;
            if (response.ok) {
                return { ok: true, data: body };
            }

            return {
                ok: false,
                status: response.status,
                error: refusal(response.status, body),
                retryAfterSeconds: parseRetryAfter(response.headers?.get("retry-after") ?? null, now())
            };
        }
    };
}

/** What the token exchange answers once read. */
export type ExchangeAnswer =
    | { ok: true, accessToken: string, expiresInSeconds: number, settings: unknown }
    | { ok: false, status: number, code: string, retryAfterSeconds: number | null };

/**
 * Exchanges a church token for a platform token and the settings beside it.
 *
 * @param fetch The fetch to use.
 * @param projectUrl The platform's address.
 * @param publishableKey The platform's publishable key.
 * @param churchToken The token Rock signed.
 * @param now The time now in milliseconds.
 *
 * @returns The token and settings, or the refusal.
 */
export async function exchangeChurchToken(
    fetch: PlatformCallOptions["fetch"],
    projectUrl: string,
    publishableKey: string,
    churchToken: string,
    now: number = Date.now()
): Promise<ExchangeAnswer> {
    try {
        const response = await fetch(`${projectUrl.replace(/\/+$/, "")}/functions/v1/token-exchange`, {
            method: "POST",
            headers: { "Authorization": `Bearer ${churchToken}`, "apikey": publishableKey }
        });
        const body = await response.json().catch(() => null) as TokenExchangeResponse | null;

        if (response.ok && body?.access_token && typeof body.expires_in === "number") {
            return { ok: true, accessToken: body.access_token, expiresInSeconds: body.expires_in, settings: body.settings };
        }

        return {
            ok: false,
            status: response.status,
            code: body?.error?.code ?? "auth.invalid_token",
            retryAfterSeconds: parseRetryAfter(response.headers?.get("retry-after") ?? null, now)
        };
    }
    catch {
        return { ok: false, status: 0, code: "rpc.transport", retryAfterSeconds: null };
    }
}
