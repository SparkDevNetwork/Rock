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

/** How often a press is checked on. */
export const syncNowCheckIntervalMilliseconds = 3000;

/** How long a press is followed before the screen stops waiting for it. */
export const syncNowBudgetMilliseconds = 120000;

/**
 * Builds a Sync Now button's behaviour.
 *
 * @param dependencies The block calls, the clock and the wait.
 *
 * @returns The behaviour.
 */
export function createSyncNow(dependencies: SyncNowDependencies): SyncNow {
    throw new Error("not implemented");
}
