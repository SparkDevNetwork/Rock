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
    messageForError
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

    test("a server sentence that is only the code itself, or blank, counts as no sentence", () => {
        // An Edge refusal whose message repeats its code would otherwise show the person "rpc.unavailable".
        expect(classifyPlatformError({ message: "rpc.unavailable", hint: "rpc.unavailable", status: 503 }))
            .toEqual({ code: "rpc.unavailable", severity: "failed" });
        expect(classifyPlatformError({ message: "rpc.bad_request", hint: "   ", status: 400 }))
            .toEqual({ code: "rpc.bad_request", severity: "failed" });
        expect(messageForError({ code: "rpc.unavailable", severity: "failed", text: "rpc.unavailable" }))
            .toBe("Something went wrong in chat. Try again in a moment.");
    });

    test("with no sentence from the server the severity's sentence is shown", () => {
        expect(messageForError({ code: "rpc.something_new", severity: "failed" }))
            .toBe("Something went wrong in chat. Try again in a moment.");
    });
});
