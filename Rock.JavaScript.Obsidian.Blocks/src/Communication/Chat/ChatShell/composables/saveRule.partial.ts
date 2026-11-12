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
// switches, her status, and a thread's follow. The control is the feedback: a change shows at once
// and a save that lands shows nothing more, so a change is seamless. One save of a setting is out
// at a time, so two answers of one setting never land crossed: a change made while one is out, or
// while its pause runs, shows at once and takes the place of any change still waiting, and the
// newest waiting is asked once the one out is answered and its pause has passed. Only the newest
// says how it went, so quick changes never flicker through older answers; a refusal puts the
// control back to the value last known saved, and its owner shows why, with Try again where
// another try can pass. A save whose owner names a wait is asked again after it, the choice kept.
import { failedWords } from "../errors.partial";
import { FailureWords } from "../types.partial";
import { Note } from "../ui/settingNote.partial.obs";

/** What a save that failed without the server's words says, as a call that threw. */
export const saveFailed: FailureWords = failedWords("Couldn't save.");

/**
 * How a save went: landed, with what the server read back and what was sent; or failed in words,
 * with how long to wait before asking it again when its owner would rather ask again than give up.
 */
export type SaveOutcome =
    | {
        isSaved: true;
        data?: unknown;

        /**
         * What the ask sent, when that was not the change's own value (a newer value its owner
         * held); null when it sent nothing, the server already holding the value. Left out, the
         * change's value was sent.
         */
        sent?: unknown;
    }
    | {
        isSaved: false;
        failure: FailureWords;

        /** Milliseconds before the same save is asked again; left out, the failure is final and the control goes back. */
        retryInMs?: number;
    };

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

    /**
     * The least time from when the change was made to when it is asked, so quick changes inside
     * it make one call; none by default.
     */
    settleMs?: number;

    /**
     * When the change was made, on performance.now()'s clock, which a system clock moved back
     * never moves; when save is called by default.
     */
    madeAt?: number;

    /** Runs the ask, as under a lock shared with the person's other pages; the ask alone by default. */
    exclusive?: (ask: () => Promise<SaveOutcome>) => Promise<SaveOutcome>;

    /** The newest save landed, with what the server read back. */
    landed?: (data: unknown) => void;

    /** The newest save was refused, with the words to show beside the control. */
    refused: (failure: FailureWords) => void;
};

/** The save rule an owner of settings holds. */
export type SaveRule = {
    /** Shows a change at once and writes it; true once it is the newest and has landed. */
    save: <T>(key: string, change: SaveChange<T>) => Promise<boolean>;

    /** Whether a save of a setting is out or waiting, so a load leaves what the person chose. */
    isOut: (key: string) => boolean;

    /**
     * Ends a setting's named wait now, as when the network or the platform is back: what waits is
     * asked at once, its own pause still kept. With no named wait, nothing.
     */
    wake: (key: string) => void;

    /**
     * Ends a setting's pause now, as when something waits on its answer: the change waiting is
     * asked at once unless a save is out or a named wait holds it, which still hold.
     */
    hasten: (key: string) => void;

    /**
     * Drops a setting's change waiting, resolving false, so its named wait ends with it, as when
     * its owner forgets the change; one out is still answered.
     */
    drop: (key: string) => void;

    /** Asks nothing more: what waits is dropped, each resolving false. */
    stop: () => void;
};

/** A change waiting to be asked, with the promise its save answers. */
type Waiting = { change: SaveChange<unknown>, madeAt: number, resolve: (isNewestLanded: boolean) => void };

/**
 * One setting's saves: the value last known saved, whether a save is out, the newest change
 * waiting, the time before which nothing is asked, which a named wait sets, and the timer that
 * asks once the waiting change is due.
 */
type SaveTrack = {
    saved: unknown;
    isFlying: boolean;
    waiting: Waiting | null;
    notBefore: number;
    timer: ReturnType<typeof setTimeout> | null;
};

/** The longest timer a browser holds, past which it fires at once; a longer wait is kept by timers no longer than it. */
const longestTimerMs = 2 ** 31 - 1;

/**
 * Whether an owner's named wait is one the rule can keep: a finite number of milliseconds from
 * zero up. Anything else is read as no wait, so the failure is final; a wait of NaN would
 * otherwise be due at once and ask in a loop.
 *
 * @param retryInMs The wait an outcome named, or nothing.
 *
 * @returns Whether it is a wait.
 */
function isWait(retryInMs: number | undefined): retryInMs is number {
    return typeof retryInMs === "number" && Number.isFinite(retryInMs) && retryInMs >= 0;
}

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

    /** Drops a setting's waiting change, resolving false, and clears its timer. */
    function dropWaiting(track: SaveTrack): void {
        if (track.timer !== null) {
            clearTimeout(track.timer);
            track.timer = null;
        }
        track.waiting?.resolve(false);
        track.waiting = null;
    }

    /** Asks the waiting change once nothing is out and it is due, or sets a timer for when it is. */
    function next(track: SaveTrack): void {
        if (track.timer !== null) {
            clearTimeout(track.timer);
            track.timer = null;
        }
        const waiting = track.waiting;
        if (isStopped() || track.isFlying || !waiting) {
            return;
        }

        const dueIn = Math.max(waiting.madeAt + (waiting.change.settleMs ?? 0), track.notBefore) - performance.now();
        if (dueIn > 0) {
            track.timer = setTimeout(() => next(track), Math.min(dueIn, longestTimerMs));
            return;
        }

        track.waiting = null;
        track.isFlying = true;
        void fly(track, waiting);
    }

    /** Asks one change and settles what its answer says, then asks the next waiting. */
    async function fly(track: SaveTrack, asked: Waiting): Promise<void> {
        const change = asked.change;

        // A call that throws is a failed save like any other, so the save is never left out.
        let outcome: SaveOutcome;
        try {
            outcome = await (change.exclusive ? change.exclusive(change.ask) : change.ask());
        }
        catch {
            outcome = { isSaved: false, failure: saveFailed };
        }
        track.isFlying = false;
        if (isStopped()) {
            asked.resolve(false);
            return;
        }

        const isNewest = track.waiting === null;
        if (outcome.isSaved) {
            // what the server now holds is what was sent, which may be newer than this change
            const sent = "sent" in outcome ? outcome.sent : change.value;
            if (sent !== null) {
                track.saved = sent;
            }
            track.notBefore = -Infinity;
            if (isNewest) {
                change.landed?.(outcome.data);
            }
            asked.resolve(isNewest);
        }
        else if (isWait(outcome.retryInMs)) {
            // The wait is outside the flight, so a lock the ask runs under is free through it and
            // a change made meanwhile is what the next ask sends.
            track.notBefore = performance.now() + outcome.retryInMs;
            if (isNewest) {
                track.waiting = asked;
            }
            else {
                asked.resolve(false);
            }
        }
        else {
            track.notBefore = -Infinity;
            if (isNewest) {
                change.show(track.saved);
                change.refused(outcome.failure);
            }
            asked.resolve(false);
        }

        next(track);
    }

    return {
        save: <T>(key: string, change: SaveChange<T>): Promise<boolean> => {
            let track = tracks.get(key);
            if (!track || (!track.isFlying && !track.waiting)) {
                // With nothing out or waiting, what the control shows is what is saved.
                track = { saved: change.was, isFlying: false, waiting: null, notBefore: -Infinity, timer: null };
                tracks.set(key, track);
            }
            const current = track;

            change.show(change.value);
            return new Promise<boolean>(resolve => {
                current.waiting?.resolve(false);
                current.waiting = { change: change as SaveChange<unknown>, madeAt: change.madeAt ?? performance.now(), resolve };
                next(current);
            });
        },

        isOut: (key: string): boolean => {
            const track = tracks.get(key);
            return !!track && (track.isFlying || track.waiting !== null);
        },

        wake: (key: string): void => {
            const track = tracks.get(key);
            if (!track || track.notBefore === -Infinity) {
                return;
            }
            track.notBefore = -Infinity;
            next(track);
        },

        hasten: (key: string): void => {
            const track = tracks.get(key);
            if (!track?.waiting) {
                return;
            }
            track.waiting.madeAt = -Infinity;
            next(track);
        },

        drop: (key: string): void => {
            const track = tracks.get(key);
            // a setting with nothing out or waiting starts afresh at its next change, its wait with it
            if (track) {
                dropWaiting(track);
            }
        },

        stop: (): void => {
            for (const track of tracks.values()) {
                dropWaiting(track);
            }
        }
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
