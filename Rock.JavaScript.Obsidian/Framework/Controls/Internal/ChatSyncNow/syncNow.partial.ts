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
import { ChatSyncNowStatusBag } from "@Obsidian/ViewModels/Blocks/Communication/Chat/ChatSyncNow/chatSyncNowStatusBag";

/** What a block action comes back as, as far as Sync Now needs to know. */
export type SyncNowActionResult = {
    isSuccess: boolean;
    data?: ChatSyncNowStatusBag | null;
    errorMessage?: string | null;
};

/** What Sync Now is given, so that the press and the checks can be driven without a browser. */
export type SyncNowDependencies = {
    /** Asks the block to start a sync. */
    request: () => Promise<SyncNowActionResult>;

    /** Asks the block where the press with this marker has got to. */
    check: (runMarker: number) => Promise<SyncNowActionResult>;

    /** Waits the given number of milliseconds. */
    wait: (milliseconds: number) => Promise<void>;

    /** The current time in milliseconds. */
    now: () => number;

    /** Told what to show each time the press moves on. */
    onProgress?: (message: string) => void;
};

/** What a press came to. */
export type SyncNowOutcome = {
    isFinished: boolean;
    isFailure: boolean;
    message: string;
};

/** A Sync Now button's behaviour. */
export type SyncNow = {
    /** Starts a sync and follows it, or does nothing and returns null when one is already being followed. */
    press: () => Promise<SyncNowOutcome | null>;

    /** Whether a press is being followed right now. */
    isInFlight: () => boolean;
};

/**
 * How often a press is checked on. The same cadence the sync job uses when a person is waiting on
 * the chat platform's answer, so the screen learns of the end of the run about as soon as the job
 * does; each check is one read of the job's own history. An estimate, like the budget below.
 */
export const syncNowCheckIntervalMilliseconds = 3000;

/**
 * How long a press is followed before the screen stops waiting for it. Long enough to cover the
 * job's own wait for the chat platform's answer, which is a minute for a person's run, plus reading
 * the church and sending it at a typical church; an estimate, since how long that takes depends on
 * the church. A large church can outlast it, and then the screen says where the result will appear
 * rather than keep a person waiting.
 */
export const syncNowBudgetMilliseconds = 120000;

/** Said when the budget is spent and the run has not ended. */
const stillRunningMessage = "The sync is still running. Its result will appear on the Chat Platform Sync job in Jobs Administration.";

/** Said when a refusal carries no reason of its own. */
const unexplainedRefusalMessage = "Sync Now could not be started.";

/**
 * Builds a Sync Now button's behaviour.
 *
 * @param dependencies The block calls, the clock and the wait.
 *
 * @returns The behaviour.
 */
export function createSyncNow(dependencies: SyncNowDependencies): SyncNow {
    let isInFlight = false;

    /**
     * Turns a refused block action into what the press came to.
     *
     * @param result The refused action.
     *
     * @returns The outcome.
     */
    function refused(result: SyncNowActionResult): SyncNowOutcome {
        return {
            isFinished: false,
            isFailure: true,
            message: result.errorMessage || unexplainedRefusalMessage
        };
    }

    /**
     * Starts a sync and checks on it until it ends or the budget is spent.
     *
     * @returns What the press came to.
     */
    async function follow(): Promise<SyncNowOutcome> {
        const requested = await dependencies.request();

        if (!requested.isSuccess || !requested.data) {
            return refused(requested);
        }

        let status = requested.data;
        dependencies.onProgress?.(status.message ?? "");

        const started = dependencies.now();

        while (!status.isFinished) {
            if (dependencies.now() - started >= syncNowBudgetMilliseconds) {
                return { isFinished: false, isFailure: false, message: stillRunningMessage };
            }

            await dependencies.wait(syncNowCheckIntervalMilliseconds);

            const checked = await dependencies.check(status.runMarker);

            if (!checked.isSuccess || !checked.data) {
                return refused(checked);
            }

            status = checked.data;
            dependencies.onProgress?.(status.message ?? "");
        }

        return {
            isFinished: true,
            isFailure: status.isFailure,
            message: status.message ?? ""
        };
    }

    return {
        press: async (): Promise<SyncNowOutcome | null> => {
            // One press at a time. The job would refuse a second run anyway, but a second request
            // would start a second poll reporting the same run twice.
            if (isInFlight) {
                return null;
            }

            isInFlight = true;

            try {
                return await follow();
            }
            finally {
                isInFlight = false;
            }
        },

        isInFlight: (): boolean => isInFlight
    };
}
