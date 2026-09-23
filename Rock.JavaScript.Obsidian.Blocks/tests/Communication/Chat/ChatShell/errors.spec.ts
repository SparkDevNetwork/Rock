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
    classifyRealtimeStatus
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
