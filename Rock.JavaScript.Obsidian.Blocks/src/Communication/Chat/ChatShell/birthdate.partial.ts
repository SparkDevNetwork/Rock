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

/**
 * Whether the shell shows the birthdate prompt rather than a refusal.
 *
 * @param _phase Where the shell stopped.
 * @param _gate The gate that refused, when one did.
 *
 * @returns True only when chat refused for want of a birthdate.
 */
export function isBirthdatePromptShown(_phase: string, _gate: string | null): boolean {
    throw new Error("not implemented");
}

/**
 * What the shell does with the save action's answer.
 *
 * @param _result What the action answered.
 *
 * @returns The next step.
 */
export function birthdateOutcome(_result: BirthdateSaveResult): BirthdateOutcome {
    throw new Error("not implemented");
}
