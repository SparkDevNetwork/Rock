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
// The button itself is not mounted here, for the reason chatConfiguration.spec.ts gives: mounting
// any component in this project pulls packages no build step installs. What a press does is this
// module, which the one button the Chat Configuration and Group Type Detail blocks share is built on.
// It sits with the framework's internal controls because the two blocks are in different folders and
// each folder is its own TypeScript project, which may not import from another.
import { ChatSyncNowStatusBag } from "@Obsidian/ViewModels/Blocks/Communication/Chat/ChatSyncNow/chatSyncNowStatusBag";
import {
    createSyncNow,
    syncNowBudgetMilliseconds,
    syncNowCheckIntervalMilliseconds,
    SyncNowActionResult,
    SyncNowDependencies
} from "@Obsidian/Controls/Internal/ChatSyncNow/syncNow.partial";

function status(runMarker: number, isFinished: boolean, isFailure: boolean, message: string): SyncNowActionResult {
    const data: ChatSyncNowStatusBag = { runMarker, isFinished, isFailure, message };

    return { isSuccess: true, data };
}

/** A clock that moves only when the press waits, so the budget is spent exactly as written. */
function harness(request: () => Promise<SyncNowActionResult>, check: (runMarker: number) => Promise<SyncNowActionResult>): {
    dependencies: SyncNowDependencies;
    waits: number[];
    checks: number[];
    requests: () => number;
} {
    let time = 1000000;
    let requestCount = 0;
    const waits: number[] = [];
    const checks: number[] = [];

    return {
        dependencies: {
            request: () => {
                requestCount++;
                return request();
            },
            check: (runMarker: number) => {
                checks.push(runMarker);
                return check(runMarker);
            },
            wait: (milliseconds: number) => {
                waits.push(milliseconds);
                time += milliseconds;
                return Promise.resolve();
            },
            now: () => time
        },
        waits,
        checks,
        requests: () => requestCount
    };
}

describe("Sync Now", () => {
    it("sends one request, checks on that run every three seconds until it ends, and reports what it came to", async () => {
        const answers = [
            status(900, false, false, "The sync is running."),
            status(900, false, false, "The sync is running."),
            status(900, true, true, "Submission 1: the chat platform refused this restatement: sync.bad_counts")
        ];
        const h = harness(
            () => Promise.resolve(status(900, false, false, "Waiting for the sync to start.")),
            () => Promise.resolve(answers.shift() as SyncNowActionResult));

        const outcome = await createSyncNow(h.dependencies).press();

        expect(h.requests()).toBe(1);
        expect(h.checks).toEqual([900, 900, 900]);
        expect(h.waits).toEqual([3000, 3000, 3000]);
        expect(syncNowCheckIntervalMilliseconds).toBe(3000);
        expect(outcome).toEqual({
            isFinished: true,
            isFailure: true,
            message: "Submission 1: the chat platform refused this restatement: sync.bad_counts"
        });
    });

    it("reports a refused request's reason and never checks on anything", async () => {
        const h = harness(
            () => Promise.resolve({ isSuccess: false, errorMessage: "Chat is not set up for this church." }),
            () => Promise.resolve(status(0, true, false, "should not be asked")));

        const outcome = await createSyncNow(h.dependencies).press();

        expect(h.checks).toEqual([]);
        expect(h.waits).toEqual([]);
        expect(outcome?.isFinished).toBe(false);
        expect(outcome?.isFailure).toBe(true);
        expect(outcome?.message).toBe("Chat is not set up for this church.");
    });

    it("stops checking when a check is refused, and reports why", async () => {
        const h = harness(
            () => Promise.resolve(status(900, false, false, "Waiting for the sync to start.")),
            () => Promise.resolve({ isSuccess: false, errorMessage: "You are not authorized to sync chat from here." }));

        const outcome = await createSyncNow(h.dependencies).press();

        expect(h.checks).toEqual([900]);
        expect(outcome?.isFailure).toBe(true);
        expect(outcome?.message).toBe("You are not authorized to sync chat from here.");
    });

    it("says it could not check, not that it could not start, when a check fails with no reason", async () => {
        // By the time a check fails the run has been asked for and goes ahead, so "could not be
        // started" would be untrue and would invite a second press that is then refused.
        const h = harness(
            () => Promise.resolve(status(900, false, false, "Waiting for the sync to start.")),
            () => Promise.resolve({ isSuccess: false, errorMessage: null }));

        const outcome = await createSyncNow(h.dependencies).press();

        expect(outcome?.isFailure).toBe(true);
        expect(outcome?.message).not.toContain("could not be started");
        expect(outcome?.message).toContain("Chat Platform Sync");
    });

    it("stops waiting once its budget is spent and says the run is still going and where its result will appear", async () => {
        const h = harness(
            () => Promise.resolve(status(900, false, false, "Waiting for the sync to start.")),
            () => Promise.resolve(status(900, false, false, "The sync is running.")));

        const outcome = await createSyncNow(h.dependencies).press();

        expect(syncNowBudgetMilliseconds).toBe(120000);
        expect(h.checks).toHaveLength(syncNowBudgetMilliseconds / syncNowCheckIntervalMilliseconds);
        expect(outcome?.isFinished).toBe(false);
        expect(outcome?.isFailure).toBe(false);
        expect(outcome?.message).toContain("still running");
        expect(outcome?.message).toContain("Jobs Administration");
    });

    it("sends nothing for a second press while the first is still being followed", async () => {
        let release: (value: SyncNowActionResult) => void = () => undefined;
        const h = harness(
            () => new Promise<SyncNowActionResult>(resolve => {
                release = resolve;
            }),
            () => Promise.resolve(status(900, true, false, "done")));
        const syncNow = createSyncNow(h.dependencies);

        const first = syncNow.press();
        const second = await syncNow.press();

        expect(second).toBeNull();
        expect(syncNow.isInFlight()).toBe(true);

        release(status(900, false, false, "Waiting for the sync to start."));
        await first;

        expect(h.requests()).toBe(1);
        expect(syncNow.isInFlight()).toBe(false);
    });
});
