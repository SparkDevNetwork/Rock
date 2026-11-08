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
// Tells the platform what this client met and could not handle: a refusal it has no words for,
// Realtime words it does not know, a message type or block drawn by its fallback, a gap in what
// it heard live. A released client is in use for years, so this is how the platform learns what
// its older clients are missing.
//
// A report ends as a line in the platform's log, so it carries codes and never a message body, a
// name or another person's id: every word sent is held to the letters a code is made of. Each kind
// and code goes once a page load, and only a few a load, so a page that keeps failing does not
// keep reporting it; a report that fails is let go, since telling the platform must never become
// a failure the person sees or another report.
import { InjectionKey } from "vue";
import { CallResult } from "./platformCall.partial";

/** What the report reaches outside itself. */
export type ProblemReportOptions = {
    /** The shell's one call path to the platform. */
    call: (name: string, args: Record<string, unknown>) => Promise<CallResult>;

    /** How many reports a page load may send, read from the settings at each report. */
    cap: () => number;
};

/** What the components that draw a fallback reach the report by. */
export type ProblemReporter = {
    /**
     * Tells the platform about a problem, once per kind and code a page load.
     *
     * @param kind What sort of problem, one of the client's own names.
     * @param code Which one, as a code or a type's name; one not of a code's letters is sent as unreadable.
     * @param count How many, where it is a number; 0 by default.
     * @param detail The client's own words about it, sent only when they are of a code's letters.
     */
    report: (kind: string, code: string, count?: number, detail?: string) => void;
};

/** The report a shell holds. */
export type ProblemReport = ProblemReporter & {
    /** Sends nothing more; a report already out is let go when it answers. */
    stop: () => void;
};

/** The reporter the shell provides to the components that draw a fallback. */
export const problemReportKey: InjectionKey<ProblemReporter> = Symbol("chat-problem-report");

/** The letters the platform accepts in a kind or a code, so neither can carry free text. */
const codeLetters = /^[a-z0-9._-]+$/;

/** The longest kind, code and detail the platform keeps. */
const maxKindLength = 40;
const maxCodeLength = 64;
const maxDetailLength = 300;

/** The largest count the platform keeps, well inside its integer. */
const maxCount = 1000000;

/** What a code that is not of a code's letters is sent as, so it is still counted. */
const unreadableCode = "unreadable";

/**
 * Whether text is of a code's letters and no longer than the platform keeps.
 *
 * @param text The text.
 * @param maxLength The longest the platform keeps.
 *
 * @returns True when it may be sent as written.
 */
function isCodeText(text: unknown, maxLength: number): text is string {
    return typeof text === "string" && text.length <= maxLength && codeLetters.test(text);
}

/**
 * Creates the report, which lives as long as the page load.
 *
 * @param options What it reaches outside itself.
 *
 * @returns The report.
 */
export function createProblemReport(options: ProblemReportOptions): ProblemReport {
    /** Every kind and code already sent this load, a failed one included, so none goes twice. */
    const sent = new Set<string>();

    /** Set once the shell stops; nothing is sent after it. */
    let isStopped = false;

    return {
        report: (kind: string, code: string, count: number = 0, detail?: string): void => {
            // A kind is always one of the client's own names, so one that is not is a mistake in
            // the client and is not worth a line.
            if (isStopped || !isCodeText(kind, maxKindLength)) {
                return;
            }

            const sentCode = isCodeText(code, maxCodeLength) ? code : unreadableCode;
            const key = `${kind} ${sentCode}`;
            if (sent.has(key) || sent.size >= Math.floor(options.cap())) {
                return;
            }
            sent.add(key);

            const args = {
                p_kind: kind,
                p_code: sentCode,
                p_count: Number.isFinite(count) ? Math.min(maxCount, Math.max(0, Math.floor(count))) : 0,
                p_detail: isCodeText(detail, maxDetailLength) ? detail : null
            };

            // The platform answers nothing, so there is nothing to apply when it does, after a
            // stop or not; a refusal or a call that never left is dropped and never sent again.
            try {
                void options.call("chat_report_problem", args).catch(() => undefined);
            }
            catch {
                // the call path could not even ask, which is the same as a failed call
            }
        },

        stop: (): void => {
            isStopped = true;
        }
    };
}
