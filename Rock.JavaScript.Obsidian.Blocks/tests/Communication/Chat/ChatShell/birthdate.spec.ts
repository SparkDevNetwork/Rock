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
// When the shell asks for a birthdate, and what it does with Rock's answer. The prompt itself is
// markup and is not mounted here; every choice it makes is this module.
import { birthdateOutcome, isBirthdatePromptShown } from "../../../../src/Communication/Chat/ChatShell/birthdate.partial";

const everyOtherGate = [
    "ok",
    "sign_in_required",
    "not_configured",
    "deceased",
    "inactive",
    "banned",
    "age_restricted",
    "no_primary_alias",
    "invalid_key",
    "gate_unavailable"
];

function saved(gate: string, code = "saved"): Parameters<typeof birthdateOutcome>[0] {
    return {
        isSuccess: true,
        statusCode: 200,
        data: {
            code,
            session: gate === "ok"
                ? { gate, projectUrl: "http://127.0.0.1:54321", publishableKey: "sb_publishable_test", canStartDm: true }
                : { gate, canStartDm: false }
        }
    };
}

describe("isBirthdatePromptShown", () => {
    it("shows the prompt when chat refused for want of a birthdate", () => {
        expect(isBirthdatePromptShown("refused", "age_verification_required")).toBe(true);
    });

    it("shows no prompt for any other gate, a person under age included", () => {
        for (const gate of everyOtherGate) {
            expect([gate, isBirthdatePromptShown("refused", gate)]).toEqual([gate, false]);
        }
    });

    it("shows no prompt while chat is starting, ready, or failed on the platform's side", () => {
        for (const phase of ["starting", "ready", "failed"]) {
            expect([phase, isBirthdatePromptShown(phase, "age_verification_required")]).toEqual([phase, false]);
        }
    });
});

describe("birthdateOutcome", () => {
    it("starts chat from the session Rock returned when the birthdate let the person in", () => {
        const result = saved("ok");

        expect(birthdateOutcome(result)).toEqual({ kind: "start", session: result.data?.session });
    });

    it("shows the refusal when the birthdate puts the person under age", () => {
        expect(birthdateOutcome(saved("age_restricted"))).toEqual({ kind: "refused", gate: "age_restricted" });
    });

    it("follows the session whenever chat is no longer asking, whatever the door answered", () => {
        // Another tab saved first, or a birthdate was recorded meanwhile: Rock's session is the truth.
        expect(birthdateOutcome(saved("ok", "not_asked"))).toEqual({ kind: "start", session: saved("ok").data?.session });
        expect(birthdateOutcome(saved("age_restricted", "birthdate_recorded"))).toEqual({ kind: "refused", gate: "age_restricted" });
        expect(birthdateOutcome(saved("banned", "not_asked"))).toEqual({ kind: "refused", gate: "banned" });
    });

    it("keeps the prompt open with its own message for each reason the date was not taken", () => {
        const invalid = birthdateOutcome(saved("age_verification_required", "invalid_date"));
        const recorded = birthdateOutcome(saved("age_verification_required", "birthdate_recorded"));

        expect(invalid.kind).toBe("retry");
        expect(recorded.kind).toBe("retry");
        const invalidMessage = invalid.kind === "retry" ? invalid.message : "";
        const recordedMessage = recorded.kind === "retry" ? recorded.message : "";
        expect(invalidMessage).not.toBe("");
        expect(recordedMessage).not.toBe("");
        expect(invalidMessage).not.toBe(recordedMessage);
    });

    it("asks the person to sign in when Rock's sign-in has ended", () => {
        expect(birthdateOutcome({ isSuccess: false, statusCode: 401 })).toEqual({ kind: "refused", gate: "sign_in_required" });
    });

    it("keeps the prompt open to try again when Rock could not be reached or failed", () => {
        for (const statusCode of [0, 429, 500, 503, 400]) {
            const outcome = birthdateOutcome({ isSuccess: false, statusCode });

            expect([statusCode, outcome.kind]).toEqual([statusCode, "retry"]);
        }

        expect(birthdateOutcome({ isSuccess: true, statusCode: 200, data: null }).kind).toBe("retry");
    });
});
