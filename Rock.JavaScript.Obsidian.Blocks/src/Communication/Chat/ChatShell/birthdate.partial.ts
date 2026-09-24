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
// When the shell asks a person for their birthdate, and what it does with Rock's answer once
// they give it. The prompt's markup is a component; every decision it makes is here.
import { ChatBirthdateResultBag } from "@Obsidian/ViewModels/Blocks/Communication/Chat/ChatShell/chatBirthdateResultBag";
import { ChatShellSessionBag } from "@Obsidian/ViewModels/Blocks/Communication/Chat/ChatShell/chatShellSessionBag";

/** What the save action answered. */
export type BirthdateSaveResult = {
    isSuccess: boolean;
    statusCode: number;
    data?: ChatBirthdateResultBag | null;
};

/** What the shell does next. */
export type BirthdateOutcome =
    /** Chat may open now: start again from this session. */
    | { kind: "start", session: ChatShellSessionBag }
    /** Chat still refuses, for this gate, and there is nothing more to ask. */
    | { kind: "refused", gate: string }
    /** Keep the prompt open and show this under it. */
    | { kind: "retry", message: string };

const birthdateGate = "age_verification_required";

const retryMessages: Record<string, string> = {
    invalid_date: "That date is incomplete, does not exist, or is after today. Check it and try again.",
    birthdate_recorded: "Rock already has part of your birthdate, and this date does not match it. Ask your church to correct it on your profile."
};

const unsavedMessage = "Your birthdate could not be saved just now. Try again.";

/**
 * Whether the shell shows the birthdate prompt rather than a refusal.
 *
 * @param phase Where the shell stopped.
 * @param gate The gate that refused, when one did.
 *
 * @returns True only when chat refused for want of a birthdate.
 */
export function isBirthdatePromptShown(phase: string, gate: string | null): boolean {
    return phase === "refused" && gate === birthdateGate;
}

/**
 * What the shell does with the save action's answer. The session Rock returns is followed
 * whenever chat is no longer asking for a birthdate, whatever the save itself answered, because
 * another tab or a member of staff may have settled it first.
 *
 * @param result What the action answered.
 *
 * @returns The next step.
 */
export function birthdateOutcome(result: BirthdateSaveResult): BirthdateOutcome {
    if (!result.isSuccess) {
        // Rock answers 401 when the person's Rock sign-in has ended, which no date can fix.
        return result.statusCode === 401
            ? { kind: "refused", gate: "sign_in_required" }
            : { kind: "retry", message: unsavedMessage };
    }

    const session = result.data?.session;
    const gate = session?.gate;

    if (!session || !gate) {
        return { kind: "retry", message: unsavedMessage };
    }

    if (gate === "ok") {
        return { kind: "start", session };
    }

    if (gate !== birthdateGate) {
        return { kind: "refused", gate };
    }

    return { kind: "retry", message: retryMessages[result.data?.code ?? ""] ?? unsavedMessage };
}
