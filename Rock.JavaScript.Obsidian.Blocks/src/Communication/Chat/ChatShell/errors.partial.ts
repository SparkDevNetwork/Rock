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
//
// A released client is in use for years while the platform keeps adding codes, so a code from
// any family is kept, and the sentence the server writes for it travels with it: a code this
// client was never built to know still reaches the person as something they can read.
import { ChatError, FailureWords, RoomKind } from "./types.partial";

/**
 * An error as the platform's API returns it: our code in message, the reason in details, and the
 * sentence a person can read in hint.
 */
export type PlatformErrorLike = {
    message?: string | null;
    code?: string | null;
    details?: string | null;
    hint?: string | null;
    status?: number | null;
};

/** The shape every one of our codes has: a family, a dot, a snake_case name. */
const ourCode = /^[a-z][a-z0-9_]*\.[a-z][a-z0-9_]*$/;

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
 * Whether server text is a sentence worth showing: blank text, or text that only repeats the code,
 * would show the person a bare code, so the client's own sentence is used instead.
 */
function isSentence(text: string | null | undefined, code: string): text is string {
    return typeof text === "string" && text.trim().length > 0 && text.trim() !== code;
}

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
        const classified: ChatError = { code: message, severity: severityByFamily[family] ?? "unknown" };
        if (isSentence(platformError.hint, message)) {
            classified.text = platformError.hint;
        }
        return classified;
    }

    // The API gateway refuses an expired or unreadable token before our code runs, so it has
    // no code of ours; its status is what says the session needs a fresh token.
    if (platformError.status === 401 || platformError.code === "PGRST301" || platformError.code === "PGRST303") {
        return { code: "auth.expired", severity: "session" };
    }

    return { code: "rpc.unknown", severity: "unknown" };
}

/**
 * What the person is told for a failure. The few codes this client acts on itself keep its own
 * sentence, since the client knows what it did about them; any other code shows the server's
 * sentence when it sent one, so a code added after this release still says something useful.
 *
 * @param error The classified failure.
 *
 * @returns The sentence.
 */
export function messageForError(error: ChatError): string {
    return answeredWords(error).alone;
}

/**
 * A refusal stamped with the kind of room it met, so it is worded for a person, a group or a
 * channel. A room of unknown kind leaves the refusal as it was, with the server's words.
 *
 * @param error The failure.
 * @param roomKind The room's kind, where the shell knows it.
 *
 * @returns The failure, carrying the kind when there is one.
 */
export function inRoomKind(error: ChatError, roomKind: RoomKind | null | undefined): ChatError {
    return roomKind ? { ...error, roomKind } : error;
}

/** What asks the person to try a failure again where no Try again stands beside it. */
const tryAgainLater = "Try again in a moment.";

/** What a failure says that this client has no better words for. */
const wentWrong = "Something went wrong in chat.";

/** What a change says when it could not be made because the person is offline, wherever it was asked. */
export const offlineSentence = "You're offline. Try again when you're back.";

/** What read only says where the church set no banner of its own and the server sent no sentence. */
export const readOnlySentence = "Chat is read-only while we do some maintenance. You can read messages but not send them.";

/** What maintenance says where the church set no banner of its own and the server sent no sentence. */
export const maintenanceSentence = "Chat is down for maintenance. Try again later.";

/** Being offline, which another try can pass once the person is back. */
export const offlineWords: FailureWords = { alone: offlineSentence, besideRetry: "You're offline.", canRetry: true };

/**
 * What the wire contract publishes about refusals, carried as a constant the way the attachment
 * preview words are: a spec holds it to the contract's own copy in both directions, so the lists
 * are one fact the platform states and every client reads, never a hand list of this client's.
 * codes are the refusals another ask of the same can only meet again, whatever severity they came
 * with: the ask itself is wrong, what it names is gone or will not change, or a right is missing.
 * passable are the authz refusals another ask can pass, as a send into a room the person may join
 * once the join lands. roomRefusals are the ones that close a room to posting until its posting
 * fields say otherwise. A code in none of them, including one added after this release, can be
 * tried again.
 */
export const finalRefusals = {
    codes: [
        "authz.wrong_scope",
        "authz.wrong_tenant",
        "authz.dm_unavailable",
        "authz.no_announcement_right",
        "authz.no_mention_all_right",
        "authz.no_roster_right",
        "authz.not_author",
        "authz.not_eligible",
        "authz.not_live_person",
        "authz.not_member",
        "authz.not_readable",
        "authz.sender_not_allowed",
        "rpc.bad_request",
        "rpc.bad_body",
        "rpc.bad_message_type",
        "rpc.bad_parent",
        "rpc.bad_quote",
        "rpc.bad_blocks",
        "rpc.bad_reaction",
        "rpc.channel_not_found",
        "rpc.message_not_found",
        "rpc.message_deleted",
        "rpc.message_hidden",
        "rpc.not_editable",
        "rpc.person_not_found",
        "rpc.attachment_type",
        "rpc.attachment_size",
        "rpc.attachment_count",
        "rpc.bad_attachments",
        "rpc.bad_gif",
        "rpc.gifs_off",
        "rpc.idempotency_mismatch",
        "door.bad_request",
        "door.not_allowed",
        "door.not_found",
        "door.not_member",
        "door.self",
        "door.target_not_eligible"
    ],
    passable: ["authz.join_required"],
    roomRefusals: ["authz.dm_unavailable"]
};

/**
 * The refusal this client makes for itself, which no platform call answers and the contract does
 * not list: Rock refusing one of the block's own actions with 403. The same ask is refused again.
 */
const finalRefusalCodes = new Set([...finalRefusals.codes, "door.forbidden"]);

/**
 * The sentences a direct message or a group is refused in, from the wire contract: the server's
 * say channel because it does not know the kind of room at every place it refuses, and a
 * conversation is never called a channel. Held to the contract's copy by the same spec.
 */
export const conversationTexts: Record<string, string> = {
    "authz.no_announcement_right": "You can't post announcements in this conversation.",
    "authz.no_mention_all_right": "You can't mention everyone in this conversation.",
    "authz.not_member": "You're not in this conversation.",
    "authz.not_readable": "You don't have access to this conversation.",
    "rt.read_revoked": "You no longer have access to this conversation."
};

/**
 * The client's own words beside a Try again for the failures another try can pass. The server's
 * sentence for these already asks the person to try again, which the button beside it does, so it
 * is kept for where the sentence stands alone.
 */
const besideRetryWords: Record<string, string> = {
    "rpc.rate_limited": "You're sending messages too quickly.",
    "rpc.unavailable": "Chat is having trouble right now.",
    "rpc.system_author_missing": "Chat is still being set up for this organization.",
    "authz.join_required": "Join this channel to post."
};

/**
 * Whether no later ask of the same can pass: the contract's final refusals, and an ended session,
 * which needs the page refreshed. The one rule every place asks before it offers Try again, and
 * the one a send asks before it ends its message.
 *
 * @param error The classified failure.
 *
 * @returns True when asking again can only be refused again.
 */
export function isRefusedForGood(error: ChatError): boolean {
    return finalRefusalCodes.has(error.code) || error.severity === "session";
}

/**
 * Whether the service refused the ask because it is closed to changes. Read only and maintenance
 * refuse every change until the service opens, which its banner says, so no Try again helps until
 * then; the ask itself is not wrong, and passes once it opens.
 *
 * @param error The classified failure.
 *
 * @returns True for read only and maintenance.
 */
function isServiceClosed(error: ChatError): boolean {
    return error.code === "rpc.read_only" || error.code === "rpc.maintenance";
}

/**
 * A failure of a call that was answered, in words. A failure another try can pass says the
 * server's sentence alone, which may ask to try again, and the client's own beside the Try again
 * that asks it; any other says the server's sentence in both, or the client's where it sent none.
 *
 * @param error The classified failure.
 *
 * @returns The words.
 */
function answeredWords(error: ChatError): FailureWords {
    // A direct message or group conversation is never called a channel; a room of unknown kind
    // keeps the channel's wording, which is what most rooms are.
    const isConversation = error.roomKind === "person" || error.roomKind === "group";
    const conversationSentence = isConversation ? conversationTexts[error.code] : undefined;
    if (conversationSentence) {
        return finalWords(conversationSentence);
    }

    if (error.code === "rt.read_revoked") {
        return finalWords("You no longer have access to this channel.");
    }

    if (error.severity === "session") {
        return finalWords("Your chat session has ended. Refresh the page to continue.");
    }

    const serverSentence = isSentence(error.text, error.code) ? error.text : null;
    if (!isServiceClosed(error) && !isRefusedForGood(error)) {
        const words = failedWords(besideRetryWords[error.code] ?? wentWrong);
        return serverSentence ? { ...words, alone: serverSentence } : words;
    }

    if (serverSentence) {
        return finalWords(serverSentence);
    }

    if (error.code === "rpc.read_only") {
        return finalWords(readOnlySentence);
    }

    if (error.code === "rpc.maintenance") {
        return finalWords(maintenanceSentence);
    }

    return finalWords(error.severity === "permission" ? "You do not have access to that." : wentWrong);
}

/**
 * A failure in words, wherever it is told: being offline where nothing was reached and the
 * connection is lost, else what the call was answered with.
 *
 * @param error The classified failure.
 * @param isConnectionLost Whether the shell's live connection is lost, where the place knows it.
 *
 * @returns The words.
 */
export function failureWords(error: ChatError, isConnectionLost?: boolean): FailureWords {
    return isOfflineFailure(error, isConnectionLost) ? offlineWords : answeredWords(error);
}

/**
 * A place's own words for a failure that another try can pass, as "Couldn't save.": alone, it
 * also asks the person to try again in a moment.
 *
 * @param whatFailed What failed, in a sentence that does not ask anything.
 *
 * @returns The words.
 */
export function failedWords(whatFailed: string): FailureWords {
    return { alone: `${whatFailed} ${tryAgainLater}`, besideRetry: whatFailed, canRetry: true };
}

/**
 * A refusal that another try would only meet again, in the words given, with no Try again.
 *
 * @param sentence Why it was refused.
 *
 * @returns The words.
 */
export function finalWords(sentence: string): FailureWords {
    return { alone: sentence, besideRetry: sentence, canRetry: false };
}

/**
 * Whether a call reached nothing: not the platform, nor Rock's door in front of it.
 *
 * @param error The classified failure.
 *
 * @returns True when nothing was reached.
 */
export function isUnreached(error: ChatError): boolean {
    return error.code === "rpc.transport" || error.code === "door.unreachable";
}

/**
 * Whether a failed call is the person being offline, the one rule every place that says so asks:
 * the call reached nothing, and the live connection is lost where the place knows it. Reaching
 * nothing while the connection holds is a failure to try again, since the banner says nothing is
 * wrong, and a call that was answered was reached whatever the connection says.
 *
 * @param error The classified failure, or null for a call that was not answered or not asked.
 * @param isConnectionLost Whether the shell's live connection is lost; left out where the place
 * does not know it, and the failure alone then decides.
 *
 * @returns True when the person is offline.
 */
export function isOfflineFailure(error: ChatError | null, isConnectionLost?: boolean): boolean {
    return (error === null || isUnreached(error)) && (isConnectionLost ?? true);
}

/**
 * What the person is told for a failure where a call that reached nothing while offline says so
 * in the words of the place it failed, and any other failure in the server's.
 *
 * @param error The classified failure.
 * @param offline The place's sentence for being offline; the plain one by default.
 * @param isConnectionLost Whether the shell's live connection is lost, where the place knows it.
 *
 * @returns The sentence.
 */
export function messageOrOffline(error: ChatError, offline: string = offlineSentence, isConnectionLost?: boolean): string {
    return isOfflineFailure(error, isConnectionLost) ? offline : messageForError(error);
}

/**
 * Which failure this is to the person: its code and the sentence they are shown. Two failures
 * under one code that read differently, as two refused messages or a lost read from a channel and
 * one from a conversation, are each told and each closed on their own; the same one twice is one.
 *
 * @param error The classified failure.
 *
 * @returns The key the shell and the toast both match a failure by.
 */
export function errorKey(error: ChatError): string {
    return `${error.code} ${messageForError(error)}`;
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
 * How the client library words a socket that closed or failed, as supabase-js 2.x reports it on
 * every joined topic at once. These are not Realtime's words about the person's access but a lost
 * connection, which the next join heals. A close carrying any other reason is the server's own
 * words, left to the classification above or taken as unknown.
 */
const lostConnectionWords = [/^socket closed: \d+( \(network lost\))?$/, /^channel error: transport failure$/, /^channel error: connection lost$/];

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
    if (known) {
        return { code: known.code, severity: known.severity };
    }

    // Checked after Realtime's own words, which a socket's close reason can carry. Taken for
    // unknown words, a lost connection would mint a token and read the sidebar while nothing can
    // be heard live, ahead of the rejoin's own read.
    if (lostConnectionWords.some(words => words.test(message))) {
        return classifyRealtimeStatus("CHANNEL_ERROR");
    }

    return { code: "rt.unknown", severity: "unknown" };
}
