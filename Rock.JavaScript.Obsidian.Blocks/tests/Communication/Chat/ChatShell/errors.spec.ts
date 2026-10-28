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
// Every failure the shell meets becomes a stable code and a severity. The platform puts its own
// code in the error's message (authz.not_readable and the rest), so that is what is read; a
// message that is not one of our codes is never shown or branched on.
import {
    classifyActionFailure,
    classifyPlatformError,
    classifyRealtimeMessage,
    classifyRealtimeStatus,
    failedWords,
    failureWords,
    finalWords,
    isOfflineFailure,
    isRefusedForGood,
    isUnreached,
    maintenanceSentence,
    messageForError,
    messageOrOffline,
    offlineSentence,
    readOnlySentence
} from "../../../../src/Communication/Chat/ChatShell/errors.partial";

describe("classifyPlatformError", () => {
    test("a refusal carrying one of our codes keeps that code", () => {
        expect(classifyPlatformError({ message: "authz.not_readable", code: "42501", details: "the caller may not read this channel" }))
            .toEqual({ code: "authz.not_readable", severity: "permission" });
        expect(classifyPlatformError({ message: "rpc.bad_request", code: "PT400" }))
            .toEqual({ code: "rpc.bad_request", severity: "failed" });
    });

    test("an expired or refused platform token is a session failure", () => {
        expect(classifyPlatformError({ message: "JWT expired", code: "PGRST303", status: 401 }))
            .toEqual({ code: "auth.expired", severity: "session" });
        expect(classifyPlatformError({ message: "auth.stale_token", status: 401 }))
            .toEqual({ code: "auth.stale_token", severity: "session" });
    });

    test("a vendor message is never passed through as a code", () => {
        const classified = classifyPlatformError({ message: "duplicate key value violates unique constraint", code: "23505" });

        expect(classified).toEqual({ code: "rpc.unknown", severity: "unknown" });
    });

    test("a network failure is a transport failure, not unknown", () => {
        expect(classifyPlatformError(new TypeError("Failed to fetch")))
            .toEqual({ code: "rpc.transport", severity: "failed" });
    });

    test("anything else is unknown", () => {
        expect(classifyPlatformError(undefined)).toEqual({ code: "rpc.unknown", severity: "unknown" });
        expect(classifyPlatformError("boom")).toEqual({ code: "rpc.unknown", severity: "unknown" });
    });
});

describe("classifyActionFailure", () => {
    test("a Rock session that has ended asks the person to sign in", () => {
        expect(classifyActionFailure(401)).toEqual({ code: "door.sign_in_required", severity: "session" });
    });

    test("a refused action is a permission failure", () => {
        expect(classifyActionFailure(403)).toEqual({ code: "door.forbidden", severity: "permission" });
    });

    test("any other failed action is unknown", () => {
        expect(classifyActionFailure(500)).toEqual({ code: "door.unknown", severity: "unknown" });
    });
});

describe("classifyRealtimeStatus", () => {
    test("a refused or timed out join degrades the live connection rather than failing the shell", () => {
        expect(classifyRealtimeStatus("CHANNEL_ERROR")).toEqual({ code: "rt.channel_error", severity: "degraded" });
        expect(classifyRealtimeStatus("TIMED_OUT")).toEqual({ code: "rt.timed_out", severity: "degraded" });
        expect(classifyRealtimeStatus("CLOSED")).toEqual({ code: "rt.closed", severity: "degraded" });
    });

    test("a status it does not know is unknown", () => {
        expect(classifyRealtimeStatus("SOMETHING_NEW")).toEqual({ code: "rt.unknown", severity: "unknown" });
    });
});

describe("classifyRealtimeMessage", () => {
    // Worded as Realtime v2.130.0 sends them; the platform's live cut test asserts the same text
    // from the real server, so a Realtime upgrade that rewords one goes red there first.
    const notPermitted = "You do not have permissions to read from this Channel topic: t:10000000-0000-4000-8000-000000000001:c:c0000001-0000-4000-8000-000000000000";

    test("a channel that was joined and may no longer be read is a revoked read, for that channel only", () => {
        expect(classifyRealtimeMessage(notPermitted, true)).toEqual({ code: "rt.read_revoked", severity: "permission" });
    });

    test("the same words on a join that never succeeded are a refused join", () => {
        expect(classifyRealtimeMessage(`Unauthorized: ${notPermitted}`, false)).toEqual({ code: "rt.not_readable", severity: "permission" });
    });

    test("an expired token asks for a fresh one", () => {
        expect(classifyRealtimeMessage("Token has expired 3 seconds ago", true)).toEqual({ code: "rt.token_expired", severity: "session" });
    });

    test.each([
        "MalformedJWT: The token provided is not a valid JWT",
        "JwtSignatureError: Failed to validate JWT signature",
        "Fields `role` and `exp` are required in JWT",
        "InvalidJWTToken: Token expiration time is invalid"
    ])("a token Realtime cannot use is a session failure (%p)", message => {
        expect(classifyRealtimeMessage(message, false)).toEqual({ code: "rt.token_invalid", severity: "session" });
    });

    // Copied from the v2.130.0 source; a local stack cannot provoke them.
    test.each([
        ["ClientJoinRateLimitReached: Too many joins per second", "rt.too_many_joins"],
        ["ConnectionRateLimitReached: Too many connected users", "rt.too_many_connections"],
        ["ChannelRateLimitReached: Too many channels", "rt.too_many_channels"],
        ["Too many messages per second", "rt.too_many_messages"],
        ["IncreaseConnectionPool: Please increase your connection pool size", "rt.pool_exhausted"]
    ])("a project limit drops the whole socket (%p)", (message, code) => {
        expect(classifyRealtimeMessage(message, true)).toEqual({ code, severity: "quota" });
    });

    test.each([
        ["UnableToConnectToProject: Realtime was unable to connect to the project database", "rt.database_unavailable"],
        ["DatabaseLackOfConnections: Database can't accept more connections, Realtime won't connect", "rt.database_unavailable"],
        ["DatabaseConnectionRateLimitReached: Too many database connections attempts per second", "rt.database_unavailable"],
        ["Query was cancelled, please try again", "rt.database_unavailable"],
        ["InitializingProjectConnection: Realtime is initializing the project connection", "rt.database_unavailable"],
        ["TimeoutOnRpcCall: Node request timeout", "rt.database_unavailable"],
        ["RealtimeRestarting: Realtime is restarting, please standby", "rt.restarting"]
    ])("a Realtime that cannot answer for now degrades the live connection (%p)", (message, code) => {
        expect(classifyRealtimeMessage(message, true)).toEqual({ code, severity: "degraded" });
    });

    test("words it does not know are unknown, never passed through", () => {
        expect(classifyRealtimeMessage("Something Realtime has never said", true)).toEqual({ code: "rt.unknown", severity: "unknown" });
    });
});

describe("codes a released client was not built with", () => {
    // A Rock release that ships chat is in use for years while the platform keeps adding codes,
    // so a code from any family is kept, and the server's own sentence is what the person reads.
    test("a refusal keeps the server's sentence for it", () => {
        expect(classifyPlatformError({ message: "rpc.idempotency_mismatch", code: "PT409", hint: "That message was already sent with different text." }))
            .toEqual({ code: "rpc.idempotency_mismatch", severity: "failed", text: "That message was already sent with different text." });
    });

    test("a code from a family this client does not know is kept, not turned into rpc.unknown", () => {
        expect(classifyPlatformError({ message: "billing.over_quota", code: "PT402", hint: "This church's chat plan is full." }))
            .toEqual({ code: "billing.over_quota", severity: "unknown", text: "This church's chat plan is full." });
    });

    test("something that is not a code at all is still never passed through", () => {
        expect(classifyPlatformError({ message: "Duplicate Key", code: "23505", hint: "vendor words" }).code).toBe("rpc.unknown");
    });

    test("a code the client has no sentence of its own for shows the server's sentence", () => {
        expect(messageForError({ code: "billing.over_quota", severity: "unknown", text: "This church's chat plan is full." }))
            .toBe("This church's chat plan is full.");
        expect(messageForError({ code: "rpc.idempotency_mismatch", severity: "failed", text: "That message was already sent with different text." }))
            .toBe("That message was already sent with different text.");
    });

    test("a code the client handles itself keeps its own sentence", () => {
        expect(messageForError({ code: "rt.read_revoked", severity: "permission", text: "server words" }))
            .toBe("You no longer have access to this channel.");
        expect(messageForError({ code: "auth.expired", severity: "session", text: "server words" }))
            .toBe("Your chat session has ended. Refresh the page to continue.");
    });

    test("a read revoked from a conversation says conversation; from a channel, or a room of no known kind, channel", () => {
        expect(messageForError({ code: "rt.read_revoked", severity: "permission", roomKind: "group" }))
            .toBe("You no longer have access to this conversation.");
        expect(messageForError({ code: "rt.read_revoked", severity: "permission", roomKind: "person" }))
            .toBe("You no longer have access to this conversation.");
        expect(messageForError({ code: "rt.read_revoked", severity: "permission", roomKind: "channel" }))
            .toBe("You no longer have access to this channel.");
        expect(messageForError({ code: "rt.read_revoked", severity: "permission" }))
            .toBe("You no longer have access to this channel.");
    });

    test("a server sentence that is only the code itself, or blank, counts as no sentence", () => {
        // An Edge refusal whose message repeats its code would otherwise show the person "rpc.unavailable".
        expect(classifyPlatformError({ message: "rpc.unavailable", hint: "rpc.unavailable", status: 503 }))
            .toEqual({ code: "rpc.unavailable", severity: "failed" });
        expect(classifyPlatformError({ message: "rpc.bad_request", hint: "   ", status: 400 }))
            .toEqual({ code: "rpc.bad_request", severity: "failed" });
        expect(messageForError({ code: "rpc.unavailable", severity: "failed", text: "rpc.unavailable" }))
            .toBe("Chat is having trouble right now. Try again in a moment.");
    });

    test("with no sentence from the server the severity's sentence is shown", () => {
        expect(messageForError({ code: "rpc.something_new", severity: "failed" }))
            .toBe("Something went wrong in chat. Try again in a moment.");
    });
});

// One rule for a place that names a lost connection in its own words: only a call that never
// reached the platform is offline; any other failure is said in the server's words, or the
// severity's own sentence where the server sent none.
describe("messageOrOffline", () => {
    test("a call that never reached the platform says the place's offline sentence", () => {
        expect(messageOrOffline({ code: "rpc.transport", severity: "failed" }, "You're offline.")).toBe("You're offline.");
    });

    test("a refusal says the server's sentence", () => {
        expect(messageOrOffline({ code: "rpc.read_only", severity: "failed", text: "Chat is read only." }, "You're offline.")).toBe("Chat is read only.");
    });

    test("a failure that reached the platform with no sentence is not called offline", () => {
        expect(messageOrOffline({ code: "rpc.unknown", severity: "unknown" }, "You're offline.")).toBe("Something went wrong in chat. Try again in a moment.");
    });

    test("an ended session says so", () => {
        expect(messageOrOffline({ code: "auth.expired", severity: "session" }, "You're offline.")).toBe("Your chat session has ended. Refresh the page to continue.");
    });

    test("a Rock door that never answered is offline too", () => {
        expect(messageOrOffline({ code: "door.unreachable", severity: "failed" }, "You're offline.")).toBe("You're offline.");
    });

    test("with no sentence of the place's own, says the one offline sentence", () => {
        expect(messageOrOffline({ code: "rpc.transport", severity: "failed" })).toBe(offlineSentence);
        expect(offlineSentence).toBe("You're offline. Try again when you're back.");
    });
});

// One rule for a call that reached nothing, the platform or Rock's door before it, so every place
// that tells the person they are offline tells it for the same failures.
describe("isUnreached", () => {
    test("is a call that never reached the platform or Rock's door, and nothing else", () => {
        expect(isUnreached({ code: "rpc.transport", severity: "failed" })).toBe(true);
        expect(isUnreached({ code: "door.unreachable", severity: "failed" })).toBe(true);
        expect(isUnreached({ code: "door.unavailable", severity: "permission" })).toBe(false);
        expect(isUnreached({ code: "rpc.unknown", severity: "unknown" })).toBe(false);
    });
});

// One rule for when a failed change tells the person they are offline: the call reached nothing,
// and the live connection is lost where the place knows it. Reaching nothing while the connection
// holds is a failure to try again, as the banner says nothing is wrong; a place that does not know
// the connection goes by the failure alone.
describe("isOfflineFailure", () => {
    const transport = { code: "rpc.transport", severity: "failed" as const };

    test("a call that reached nothing while the connection is lost is offline", () => {
        expect(isOfflineFailure(transport, true)).toBe(true);
        expect(isOfflineFailure({ code: "door.unreachable", severity: "failed" }, true)).toBe(true);
    });

    test("a call not answered at all, or not asked, while the connection is lost is offline", () => {
        expect(isOfflineFailure(null, true)).toBe(true);
    });

    test("a call that reached nothing while the connection holds is not", () => {
        expect(isOfflineFailure(transport, false)).toBe(false);
        expect(isOfflineFailure(null, false)).toBe(false);
    });

    test("a call that was answered is never offline, connection or not", () => {
        expect(isOfflineFailure({ code: "rpc.read_only", severity: "failed", text: "Chat is read only." }, true)).toBe(false);
    });

    test("where the connection is not known, reaching nothing is offline", () => {
        expect(isOfflineFailure(transport)).toBe(true);
        expect(isOfflineFailure({ code: "rpc.unknown", severity: "unknown" })).toBe(false);
    });

    test("a place that knows the connection says its failures by it", () => {
        expect(messageOrOffline(transport, offlineSentence, true)).toBe(offlineSentence);
        expect(messageOrOffline(transport, offlineSentence, false)).toBe("Something went wrong in chat. Try again in a moment.");
    });
});

// A failure is said in two places: alone, where it asks the person to try again in its own words,
// and beside a Try again, where the button asks and the sentence only says what happened. Try
// again is offered only where another ask can pass: never in read only or maintenance, without a
// right, or once the session has ended.
describe("failureWords", () => {
    const transport = { code: "rpc.transport", severity: "failed" as const };

    test("a failure with no sentence of the server's says it went wrong, asking to try again only where it stands alone", () => {
        expect(failureWords({ code: "rpc.unknown", severity: "unknown" })).toEqual({
            alone: "Something went wrong in chat. Try again in a moment.",
            besideRetry: "Something went wrong in chat.",
            canRetry: true
        });
    });

    test("being offline says so, and when to try again only where it stands alone", () => {
        expect(failureWords(transport, true)).toEqual({ alone: offlineSentence, besideRetry: "You're offline.", canRetry: true });
    });

    test("a code this client does not know is said in the server's sentence alone, and in the client's beside Try again", () => {
        // The server's sentence may ask to try again itself, so beside the button it is never shown.
        expect(failureWords({ code: "rpc.busy", severity: "failed", text: "Chat is busy. Try again soon." }))
            .toEqual({ alone: "Chat is busy. Try again soon.", besideRetry: "Something went wrong in chat.", canRetry: true });
    });

    // The sentences the platform's catalog holds for these codes, word for word: each already asks
    // the person to try again, so beside a Try again the client says what happened in its own words.
    test.each([
        ["rpc.unavailable", "Chat is having trouble right now. Try again in a moment.", "Chat is having trouble right now."],
        ["rpc.rate_limited", "You're sending messages too quickly. Wait a moment and try again.", "You're sending messages too quickly."],
        ["rpc.system_author_missing", "Chat is still being set up for this organization. Try again shortly.", "Chat is still being set up for this organization."]
    ])("%s keeps the server's sentence alone and never asks twice beside Try again", (code, hint, beside) => {
        const words = failureWords(classifyPlatformError({ message: code, hint }));

        expect(words).toEqual({ alone: hint, besideRetry: beside, canRetry: true });
        expect(words.besideRetry).not.toMatch(/try/i);
    });

    test("a code another try can pass, with no sentence from the server, asks in the client's words alone", () => {
        expect(failureWords({ code: "rpc.rate_limited", severity: "failed" }))
            .toEqual({ alone: "You're sending messages too quickly. Try again in a moment.", besideRetry: "You're sending messages too quickly.", canRetry: true });
    });

    // Asked again, these only meet the same answer: the ask itself is wrong, or what it names is gone.
    test.each([
        ["rpc.bad_request", "Chat could not understand that request."],
        ["rpc.bad_body", "A message needs some text, up to 5,000 characters."],
        ["rpc.bad_parent", "That message can't be replied to."],
        ["rpc.message_not_found", "That message no longer exists."],
        ["rpc.message_deleted", "That message was deleted."],
        ["rpc.message_hidden", "That message is hidden and can't be changed."],
        ["rpc.channel_not_found", "That channel no longer exists."],
        ["rpc.not_editable", "This message can't be edited."],
        ["rpc.person_not_found", "That person isn't available in chat."],
        ["rpc.idempotency_mismatch", "This message could not be sent. Try sending it again."]
    ])("%s offers no Try again", (code, hint) => {
        expect(failureWords(classifyPlatformError({ message: code, hint }))).toEqual({ alone: hint, besideRetry: hint, canRetry: false });
        expect(isRefusedForGood(classifyPlatformError({ message: code, hint }))).toBe(true);
    });

    test.each(["door.not_allowed", "door.not_found", "door.bad_request", "door.not_member"])("Rock's door refusing as %s offers no Try again, whatever severity it was given", code => {
        expect(failureWords({ code, severity: "failed", text: "No." }).canRetry).toBe(false);
    });

    test("a send that needs a join can pass once the join lands, and Rock's door that could not be asked can pass later", () => {
        expect(failureWords({ code: "authz.join_required", severity: "permission", text: "Join this channel to post." }).canRetry).toBe(true);
        expect(failureWords({ code: "door.unavailable", severity: "permission" }).canRetry).toBe(true);
    });

    test("read only and maintenance are not refused for good, only closed until the service opens", () => {
        expect(isRefusedForGood({ code: "rpc.read_only", severity: "failed" })).toBe(false);
        expect(isRefusedForGood({ code: "rpc.maintenance", severity: "failed" })).toBe(false);
        expect(isRefusedForGood({ code: "auth.expired", severity: "session" })).toBe(true);
        expect(isRefusedForGood({ code: "authz.not_member", severity: "permission" })).toBe(true);
        expect(isRefusedForGood({ code: "rpc.transport", severity: "failed" })).toBe(false);
    });

    test("read only and maintenance with no sentence from the server say their own, not that something went wrong", () => {
        expect(failureWords({ code: "rpc.read_only", severity: "failed" })).toEqual(finalWords(readOnlySentence));
        expect(failureWords({ code: "rpc.maintenance", severity: "failed" })).toEqual(finalWords(maintenanceSentence));
        expect(readOnlySentence).toBe("Chat is read-only while we do some maintenance. You can read messages but not send them.");
        expect(maintenanceSentence).toBe("Chat is down for maintenance. Try again later.");
    });

    test("read only and maintenance offer no Try again, which cannot help until the service opens", () => {
        const readOnly = failureWords({ code: "rpc.read_only", severity: "failed", text: "Chat is read-only right now. You can read messages but not send them." });
        const maintenance = failureWords({ code: "rpc.maintenance", severity: "failed" });

        expect(readOnly).toEqual({ alone: "Chat is read-only right now. You can read messages but not send them.", besideRetry: "Chat is read-only right now. You can read messages but not send them.", canRetry: false });
        expect(maintenance.canRetry).toBe(false);
        expect(maintenance.alone).not.toMatch(/Try again in a moment/);
    });

    test("a refusal for want of a right or an ended session offers no Try again", () => {
        expect(failureWords({ code: "authz.not_member", severity: "permission" }).canRetry).toBe(false);
        expect(failureWords({ code: "auth.expired", severity: "session" }).canRetry).toBe(false);
    });

    test("a place's own words for a failure it may ask again, and for a refusal that is final", () => {
        expect(failedWords("Couldn't save.")).toEqual({ alone: "Couldn't save. Try again in a moment.", besideRetry: "Couldn't save.", canRetry: true });
        expect(finalWords("This conversation can't be renamed right now.")).toEqual({
            alone: "This conversation can't be renamed right now.",
            besideRetry: "This conversation can't be renamed right now.",
            canRetry: false
        });
    });
});
