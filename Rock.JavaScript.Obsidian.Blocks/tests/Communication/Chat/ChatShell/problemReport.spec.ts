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
// The client tells the platform what it met and could not handle: a refusal it has no words for,
// Realtime words it does not know, something drawn by its fallback, a gap in what it heard live.
// A report is a line in a log, so it carries codes only, each once a page load and a few a load,
// and a report that fails is let go: telling the platform must never become a problem of its own.
import { createProblemReport, ProblemReport } from "../../../../src/Communication/Chat/ChatShell/problemReport.partial";
import { CallResult } from "../../../../src/Communication/Chat/ChatShell/platformCall.partial";
import { PageEventTargets } from "../../../../src/Communication/Chat/ChatShell/composables/useMarkRead.partial";

type Deferred<T> = { promise: Promise<T>, resolve: (value: T) => void, reject: (error: unknown) => void };

function deferred<T>(): Deferred<T> {
    let resolve!: (value: T) => void;
    let reject!: (error: unknown) => void;
    const promise = new Promise<T>((a, b) => {
        resolve = a;
        reject = b;
    });
    return { promise, resolve, reject };
}

/** What the platform answers an accepted report: nothing. */
const accepted: CallResult = { ok: true, data: null };

/** A refusal as the shell's call path hands it back. */
const refused: CallResult = { ok: false, status: 400, error: { code: "rpc.bad_request", severity: "failed" }, retryAfterSeconds: null };

type Sent = { name: string, args: Record<string, unknown> };

/** A report over a call path that answers each call with what the test hands it. */
function reporter(reportCap: () => number = () => 10, answer: () => Promise<CallResult> = async () => accepted, countCap: () => number = () => 10): { problems: ProblemReport, sent: Sent[] } {
    const sent: Sent[] = [];
    const problems = createProblemReport({
        call: (name, args) => {
            sent.push({ name, args });
            return answer();
        },
        reportCap,
        countCap
    });
    return { problems, sent };
}

/** A page whose hiding and leaving the test does by hand. */
function page(): { targets: PageEventTargets, hide: () => void, leave: () => void } {
    const listeners = new Map<string, Array<() => void>>();
    const add = (type: string, listener: () => void): void => {
        listeners.set(type, [...(listeners.get(type) ?? []), listener]);
    };
    const remove = (type: string, listener: () => void): void => {
        listeners.set(type, (listeners.get(type) ?? []).filter(l => l !== listener));
    };
    const document = { visibilityState: "visible", addEventListener: add, removeEventListener: remove };
    const fire = (type: string): void => (listeners.get(type) ?? []).forEach(listener => listener());
    return {
        targets: { document, window: { addEventListener: add, removeEventListener: remove } } as unknown as PageEventTargets,
        hide: () => {
            document.visibilityState = "hidden";
            fire("visibilitychange");
        },
        leave: () => fire("pagehide")
    };
}

/** Lets every answer already given settle. */
async function settle(): Promise<void> {
    await new Promise(resolve => setTimeout(resolve, 0));
}

describe("the problem report", () => {
    test("sends a kind and code to the platform's report, with the count and the client's own detail", () => {
        const { problems, sent } = reporter();

        problems.report("live", "gap", 3);
        problems.report("refusal", "rpc.new_thing", 0, "rpc.new_thing");

        expect(sent).toEqual([
            { name: "chat_report_problem", args: { p_kind: "live", p_code: "gap", p_count: 3, p_detail: null } },
            { name: "chat_report_problem", args: { p_kind: "refusal", p_code: "rpc.new_thing", p_count: 0, p_detail: "rpc.new_thing" } }
        ]);
    });

    test("each kind and code is sent once a page load, however often it is met", () => {
        const { problems, sent } = reporter();

        problems.report("block", "poll");
        problems.report("block", "poll", 5);
        problems.report("message_type", "poll");
        problems.report("block", "survey");

        expect(sent.map(s => `${s.args.p_kind} ${s.args.p_code}`)).toEqual(["block poll", "message_type poll", "block survey"]);
    });

    test("at most the cap are sent a load, the cap read from the settings at each report", () => {
        let cap = 2;
        const { problems, sent } = reporter(() => cap);

        problems.report("block", "a");
        problems.report("block", "b");
        problems.report("block", "c");
        expect(sent).toHaveLength(2);

        // the platform raised it on a later exchange; what was over the old cap was not kept
        cap = 3;
        problems.report("block", "d");
        problems.report("block", "c");
        expect(sent.map(s => s.args.p_code)).toEqual(["a", "b", "d"]);
    });

    test("no body, name or other person's id is sent: only words of the codes' own letters reach the call", () => {
        const { problems, sent } = reporter();

        problems.report("block", "Sam Lee said hi", 0, "Pick up the kids at 5");
        problems.report("message_type", "8F2C1B9A-0D4E-4A7B-9C3D-1E2F3A4B5C6D", 0, "Sam Lee");
        // a kind is the client's own constant; one that is not is a mistake, never sent
        problems.report("Live Gap", "gap");

        expect(sent.map(s => s.args)).toEqual([
            { p_kind: "block", p_code: "unreadable", p_count: 0, p_detail: null },
            { p_kind: "message_type", p_code: "unreadable", p_count: 0, p_detail: null }
        ]);
    });

    test("a code or detail longer than the platform keeps is not sent as written", () => {
        const { problems, sent } = reporter();

        problems.report("block", "a".repeat(65), 0, "b".repeat(301));
        problems.report("k".repeat(41), "gap");

        expect(sent.map(s => s.args)).toEqual([{ p_kind: "block", p_code: "unreadable", p_count: 0, p_detail: null }]);
    });

    test("a count is sent as a whole number the platform's integer holds", () => {
        const { problems, sent } = reporter();

        problems.report("live", "a", 2.7);
        problems.report("live", "b", -4);
        problems.report("live", "c", Number.NaN);
        problems.report("live", "d", 5e12);

        expect(sent.map(s => s.args.p_count)).toEqual([2, 0, 0, 1000000]);
    });

    test("a failed call is dropped: not thrown, not sent again, and it still counts toward the cap", async () => {
        const answers = [deferred<CallResult>(), deferred<CallResult>()];
        let next = 0;
        const { problems, sent } = reporter(() => 3, () => answers[next++].promise);

        problems.report("refusal", "rpc.new_thing");
        problems.report("realtime", "rt.unknown");
        answers[0].resolve(refused);
        answers[1].reject(new TypeError("Failed to fetch"));
        await settle();

        problems.report("refusal", "rpc.new_thing");
        problems.report("realtime", "rt.unknown");
        expect(sent).toHaveLength(2);
    });

    test("nothing is sent once stopped, and a call answering after the stop changes nothing", async () => {
        const answer = deferred<CallResult>();
        const { problems, sent } = reporter(() => 10, () => answer.promise);

        problems.report("live", "gap", 1);
        problems.stop();
        answer.resolve(accepted);
        await settle();

        problems.report("live", "gap", 1);
        problems.report("block", "poll");
        expect(sent).toHaveLength(1);
    });

    test("a counted problem is sent as the page is hidden or left, with how many since the last send, and kept alive as the page closes", () => {
        const keptAlive: boolean[] = [];
        const sent: Sent[] = [];
        const problems = createProblemReport({
            call: (name, args, options) => {
                sent.push({ name, args });
                keptAlive.push(options?.keepalive === true);
                return Promise.resolve(accepted);
            },
            reportCap: () => 10,
            countCap: () => 10
        });
        const shown = page();
        problems.attach(shown.targets);

        problems.tally("live", "gap");
        problems.tally("live", "gap");
        problems.tally("live", "gap");
        expect(sent).toEqual([]);

        shown.hide();
        problems.tally("live", "gap");
        problems.tally("live", "gap");
        shown.leave();
        shown.leave();

        expect(sent.map(s => s.args)).toEqual([
            { p_kind: "live", p_code: "gap", p_count: 3, p_detail: null },
            { p_kind: "live", p_code: "gap", p_count: 2, p_detail: null }
        ]);
        expect(keptAlive).toEqual([true, true]);
    });

    test("counts held are sent as the report stops, each send held to the load's cap for counts, and none once stopped", () => {
        const { problems, sent } = reporter(() => 1, undefined, () => 1);

        problems.report("refusal", "rpc.new_thing");
        problems.tally("live", "gap");
        problems.tally("realtime", "rt.unknown");
        problems.stop();
        problems.tally("live", "gap");
        problems.stop();

        expect(sent.map(s => [s.args.p_kind, s.args.p_count])).toEqual([["refusal", 0], ["live", 1]]);
    });

    test("sends of counts have their own cap a load: a gappy load that keeps being hidden never spends the one-off reports' cap", () => {
        let countCap = 2;
        const { problems, sent } = reporter(() => 2, undefined, () => countCap);
        const shown = page();
        problems.attach(shown.targets);

        for (let i = 0; i < 4; i++) {
            problems.tally("live", "gap");
            shown.hide();
        }
        problems.report("refusal", "rpc.new_thing");
        problems.report("realtime", "rt.unknown");
        problems.report("block", "poll");

        // the platform raised the counts' cap on a later exchange; what was over the old cap was not kept
        countCap = 3;
        problems.tally("live", "gap");
        shown.leave();

        expect(sent.map(s => `${s.args.p_kind} ${s.args.p_code} ${s.args.p_count}`)).toEqual([
            "live gap 1", "live gap 1", "refusal rpc.new_thing 0", "realtime rt.unknown 0", "live gap 1"
        ]);
    });

    test("one-off reports that use up their cap leave the counts' sends alone", () => {
        const { problems, sent } = reporter(() => 1, undefined, () => 1);
        const shown = page();
        problems.attach(shown.targets);

        problems.report("refusal", "rpc.new_thing");
        problems.report("realtime", "rt.unknown");
        problems.tally("live", "gap");
        problems.tally("live", "gap");
        shown.leave();

        expect(sent.map(s => `${s.args.p_kind} ${s.args.p_count}`)).toEqual(["refusal 0", "live 2"]);
    });

    test("a call path that throws as it is asked is dropped the same way", () => {
        const problems = createProblemReport({
            call: () => {
                throw new Error("no token");
            },
            reportCap: () => 10,
            countCap: () => 10
        });

        expect(() => problems.report("live", "gap")).not.toThrow();
    });
});
