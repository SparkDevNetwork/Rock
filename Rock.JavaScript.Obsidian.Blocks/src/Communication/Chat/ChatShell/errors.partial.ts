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
// One place that turns every kind of failure into a stable code and a severity, so the rest of
// the shell branches on codes and never on a vendor's message text.
import { ChatError } from "./types.partial";

/** An error as the platform's API returns it: our code in message, the reason in details. */
export type PlatformErrorLike = {
    message?: string | null;
    code?: string | null;
    details?: string | null;
    status?: number | null;
};

/** The shape every one of our codes has: a family, a dot, a snake_case name. */
const ourCode = /^(auth|authz|rpc|sync|rt|push|door)\.[a-z][a-z0-9_]*$/;

/** How bad each family's refusals are, where the family alone decides it. */
const severityByFamily: Record<string, ChatError["severity"]> = {
    auth: "session",
    authz: "permission",
    rpc: "failed",
    sync: "failed",
    rt: "degraded",
    push: "failed",
    door: "permission"
};

/**
 * Classifies a refusal or failure from a platform call.
 *
 * @param error The error the platform client returned, or what a fetch threw.
 *
 * @returns The code and severity.
 */
export function classifyPlatformError(error: PlatformErrorLike | unknown): ChatError {
    // A fetch that never reached the platform throws a TypeError in every browser.
    if (error instanceof TypeError) {
        return { code: "rpc.transport", severity: "failed" };
    }

    if (!error || typeof error !== "object") {
        return { code: "rpc.unknown", severity: "unknown" };
    }

    const platformError = error as PlatformErrorLike;
    const message = platformError.message ?? "";

    if (ourCode.test(message)) {
        const family = message.slice(0, message.indexOf("."));
        return { code: message, severity: severityByFamily[family] ?? "unknown" };
    }

    // The API gateway refuses an expired or unreadable token before our code runs, so it has
    // no code of ours; its status is what says the session needs a fresh token.
    if (platformError.status === 401 || platformError.code === "PGRST301" || platformError.code === "PGRST303") {
        return { code: "auth.expired", severity: "session" };
    }

    return { code: "rpc.unknown", severity: "unknown" };
}

/**
 * Classifies a failed call to one of the block's own actions on the Rock server.
 *
 * @param statusCode The HTTP status the action returned.
 *
 * @returns The code and severity.
 */
export function classifyActionFailure(statusCode: number): ChatError {
    // Rock answers 401 when the person's Rock sign-in has ended. No chat token can fix that,
    // so the person is asked to sign in again rather than the shell retrying.
    if (statusCode === 401) {
        return { code: "door.sign_in_required", severity: "session" };
    }

    if (statusCode === 403) {
        return { code: "door.forbidden", severity: "permission" };
    }

    return { code: "door.unknown", severity: "unknown" };
}

/** The realtime client's statuses for a channel that is no longer joined. */
const realtimeCodes: Record<string, string> = {
    CHANNEL_ERROR: "rt.channel_error",
    TIMED_OUT: "rt.timed_out",
    CLOSED: "rt.closed"
};

/**
 * Classifies a live channel's status once it is no longer joined.
 *
 * @param status The status the realtime client reported.
 *
 * @returns The code and severity.
 */
export function classifyRealtimeStatus(status: string): ChatError {
    const code = realtimeCodes[status];

    // A dropped live channel never stops the shell: history still works, and the next
    // confirmed join fetches what was missed.
    return code ? { code, severity: "degraded" } : { code: "rt.unknown", severity: "unknown" };
}

/**
 * Realtime's words for each way it refuses or closes a channel, as v2.130.0 sends them, first
 * match wins. Realtime prefixes most with a name of its own ("Unauthorized: ..."), so each is
 * matched anywhere in the message. Quota refusals drop the whole socket, not one channel, so they
 * carry their own severity: retrying them at once is what keeps a project over its limits.
 */
const realtimeMessages: Array<{ words: string, code: string, severity: ChatError["severity"] }> = [
    { words: "Token has expired", code: "rt.token_expired", severity: "session" },
    { words: "not a valid JWT", code: "rt.token_invalid", severity: "session" },
    { words: "Failed to validate JWT signature", code: "rt.token_invalid", severity: "session" },
    { words: "Fields `role` and `exp` are required in JWT", code: "rt.token_invalid", severity: "session" },
    { words: "Token expiration time is invalid", code: "rt.token_invalid", severity: "session" },
    { words: "Too many joins per second", code: "rt.too_many_joins", severity: "quota" },
    { words: "Too many connected users", code: "rt.too_many_connections", severity: "quota" },
    { words: "Too many channels", code: "rt.too_many_channels", severity: "quota" },
    { words: "Too many messages per second", code: "rt.too_many_messages", severity: "quota" },
    { words: "Please increase your connection pool size", code: "rt.pool_exhausted", severity: "quota" },
    { words: "unable to connect to the project database", code: "rt.database_unavailable", severity: "degraded" },
    { words: "Database can't accept more connections", code: "rt.database_unavailable", severity: "degraded" },
    { words: "Too many database connections attempts", code: "rt.database_unavailable", severity: "degraded" },
    { words: "Query was cancelled", code: "rt.database_unavailable", severity: "degraded" },
    { words: "initializing the project connection", code: "rt.database_unavailable", severity: "degraded" },
    { words: "Node request timeout", code: "rt.database_unavailable", severity: "degraded" },
    { words: "Realtime is restarting", code: "rt.restarting", severity: "degraded" }
];

/**
 * Classifies the words Realtime closes or refuses a channel with.
 *
 * @param message The message Realtime sent.
 * @param wasJoined True when the channel had been joined before this arrived.
 *
 * @returns The code and severity.
 */
export function classifyRealtimeMessage(message: string, wasJoined: boolean): ChatError {
    // Realtime words a read revoked from a joined channel exactly as it words a refused join, so
    // only whether the channel had been joined tells the two apart.
    if (message.includes("You do not have permissions to read from this Channel topic")) {
        return { code: wasJoined ? "rt.read_revoked" : "rt.not_readable", severity: "permission" };
    }

    const known = realtimeMessages.find(m => message.includes(m.words));

    return known ? { code: known.code, severity: known.severity } : { code: "rt.unknown", severity: "unknown" };
}
