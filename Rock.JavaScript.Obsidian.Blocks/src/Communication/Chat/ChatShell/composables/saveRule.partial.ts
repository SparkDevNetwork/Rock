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
// The one way the person's settings save: a room's notify mode, star and hide, their own
// switches, and a thread's follow. The control is the feedback: a change shows at once and a save
// that lands shows nothing more, so a change is seamless. Only the newest save of a setting says
// how it went, so quick changes never flicker through older answers; a refusal puts the control
// back to the last value known saved, and its owner shows why, with Try again where another try
// can pass.
import { failedWords } from "../errors.partial";
import { FailureWords } from "../types.partial";
import { Note } from "../ui/settingNote.partial.obs";

/** What a save that failed without the server's words says, as a call that threw. */
export const saveFailed: FailureWords = failedWords("Couldn't save.");

/** How a save went: landed, with what the server read back, or refused in words. */
export type SaveOutcome = { isSaved: true, data?: unknown } | { isSaved: false, failure: FailureWords };

/** One change to a setting. */
export type SaveChange<T> = {
    /** What the control showed before this change. */
    was: T;

    /** What the person chose. */
    value: T;

    /** Puts a value on the control. */
    show: (value: T) => void;

    /** Writes the value. */
    ask: () => Promise<SaveOutcome>;

    /** The newest save landed, with what the server read back. */
    landed?: (data: unknown) => void;

    /** The newest save was refused, with the words to show beside the control. */
    refused: (failure: FailureWords) => void;
};

/** The save rule an owner of settings holds. */
export type SaveRule = {
    /** Shows a change at once and writes it; true once it is the newest and has landed. */
    save: <T>(key: string, change: SaveChange<T>) => Promise<boolean>;

    /** Whether a save of a setting is still out, so a load leaves what the person chose. */
    isOut: (key: string) => boolean;
};

/**
 * One setting's saves: the value last known saved and the save that set it, the newest save
 * asked, how many are still out, and whether the newest was refused.
 */
type SaveTrack = { saved: unknown, savedCount: number, newest: number, out: number, isRefused: boolean };

/**
 * Creates a save rule.
 *
 * @param isStopped Whether its owner has stopped, after which no answer reaches the screen.
 *
 * @returns The save rule.
 */
export function createSaveRule(isStopped: () => boolean): SaveRule {
    /** Each setting's saves, by a key naming it. */
    const tracks = new Map<string, SaveTrack>();
    let saveCount = 0;

    return {
        save: async <T>(key: string, change: SaveChange<T>): Promise<boolean> => {
            let track = tracks.get(key);
            if (!track || track.out === 0) {
                // With nothing out, what the control shows is what is saved.
                track = { saved: change.was, savedCount: 0, newest: 0, out: 0, isRefused: false };
                tracks.set(key, track);
            }

            const count = ++saveCount;
            track.newest = count;
            track.out++;
            track.isRefused = false;
            change.show(change.value);

            // A call that throws is a failed save like any other, so the save is never left out.
            let outcome: SaveOutcome;
            try {
                outcome = await change.ask();
            }
            catch {
                outcome = { isSaved: false, failure: saveFailed };
            }
            if (isStopped()) {
                return false;
            }

            track.out--;
            if (outcome.isSaved && count > track.savedCount) {
                track.saved = change.value;
                track.savedCount = count;
            }

            if (count === track.newest) {
                if (outcome.isSaved) {
                    change.landed?.(outcome.data);
                    return true;
                }

                track.isRefused = true;
                change.show(track.saved as T);
                change.refused(outcome.failure);
                return false;
            }

            // An older save that lands after the newest was refused is now what is saved.
            if (track.isRefused && outcome.isSaved) {
                change.show(track.saved as T);
            }
            return false;
        },

        isOut: (key: string): boolean => (tracks.get(key)?.out ?? 0) > 0
    };
}

/**
 * A refused change as the note under its control: Try again, asking the same change once more,
 * only where another try can pass, and then a sentence that leaves the asking to the button.
 *
 * @param failure Why the change was refused.
 * @param retry Asks the same change again.
 *
 * @returns The note.
 */
export function failureNote(failure: FailureWords, retry: () => void): Note {
    return failure.canRetry
        ? { kind: "error", text: failure.besideRetry, retry }
        : { kind: "error", text: failure.alone };
}
