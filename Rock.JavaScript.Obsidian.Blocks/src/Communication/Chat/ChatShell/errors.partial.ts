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
