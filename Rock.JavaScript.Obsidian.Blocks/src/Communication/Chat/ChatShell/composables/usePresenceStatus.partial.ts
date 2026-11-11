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
// Others' online status, the one store of it on the page: each person's newest decision, by
// the platform's decision time, whichever answer, reader or carried signal brings it and in
// whatever order they land, since an answer decided earlier can arrive later. What a face draws is
// worked out from that decision and the server's clock, never written back over it, and a person's
// revision moves only when what is drawn changes, so a face mounted at a revision draws as state
// and fades only for a change after it.
//
// Presence creates it beside typing and stops it with typing, so it has one owner, but the two
// share no state: this holds its own entries, revisions, timer and the faces registered, and reads
// only the settings, the blocks, the stopped state and the person's own id. It is a file of its own
// for that reason, so each half reads alone and neither grows past what one file should hold.

import { inject, InjectionKey, onMounted, onUnmounted, shallowReactive, watch } from "vue";
import { startOfDay } from "../feedLayout.partial";
import { FaceStatus, PersonStatus, RosterMember } from "../types.partial";
import { statusWords } from "../ui/face.partial";
import type { ChatSettings } from "./useSession.partial";

/** What status reaches outside itself. */
export type PresenceStatusDependencies = {
    /** The settings, read at every use, since the switch moves with each token. */
    settings: () => ChatSettings;

    /** The person's primary alias, whose own carried copy is never written. */
    personAliasGuid: string;

    isBlocked: (personAliasGuid: string) => boolean;

    /** Whether the shell has stopped or its session ended, read at each resumed step. */
    isStopped: () => boolean;
};

/**
 * How early a face's people are kept when the list of people watched is cut: 0 the person
 * themselves, 1 the open conversation's person and an open card or details, 2 the members, the
 * people pickers' results and New message's choice, 3 the sidebar's two-person rows.
 */
export type WatchRank = 0 | 1 | 2 | 3;

/** Others' online status on this page: the only store of it, and the only thing that writes it. */
export type PresenceStatus = {
    /**
     * A token exchange answered. Each person its body asked about that the answer's map names is
     * written; a map left out, as when the read did not answer in time, or a failed exchange,
     * writes nothing.
     *
     * @param asked The people the exchange's body watched.
     * @param outcome The exchange's answer, or null for one that failed.
     */
    exchanged: (asked: readonly string[], outcome: { presence?: unknown } | null) => void;

    /** A reader answered people, each with its status where it carries one; a person with no status key tells nothing. */
    told: (people: readonly unknown[]) => void;

    /** A typing row or a dm.preview carried its writer's status; never written for the person's own id, nor for someone they blocked. */
    carried: (alias: unknown, status: unknown) => void;

    /**
     * The people blocked changed: each named is drawn nothing from the block's decision time, or
     * from their own entry's when the block came with none, so nothing older draws after an unblock.
     */
    blockChanged: (aliases: readonly string[], asOf: number | null) => void;

    /** A token was minted at a server time, in seconds, which sets the server's clock as this page reads it. */
    clockFrom: (issuedAtSeconds: number) => void;

    /** What a face draws for a person, or null for nothing; reading it draws again when that person's revision moves. */
    statusOf: (guid: string) => FaceStatus | null;

    /** A face's people, watched until the returned function unregisters them. */
    register: (people: () => readonly string[], rank: WatchRank) => () => void;

    /** The people the next exchange asks about: the person, then every face's by rank, each once, cut at the platform's cap of 200. */
    watched: () => string[];

    stop: () => void;
};

/** The shell's status store, provided to every face that draws one. */
export const presenceStatusKey: InjectionKey<PresenceStatus> = Symbol("chat-presence-status");

// #region Status

/** The most people one exchange asks about: the platform's own cap, past which it asks about nobody. */
const watchedMax = 200;

/** The longest a timer waits, past which a browser fires it at once. */
const longestTimerMs = 2_147_483_647;

/** A person's newest decision: what it says of her, null for nothing, and when it was decided. */
type StatusEntry = { status: PersonStatus | null, asOf: number };

/** What a decision draws at a moment: her state, and when a Busy ends in server milliseconds, null for no end. */
type Drawn = { state: "online" | "away" | "busy", busyUntil: number | null };

const states: ReadonlyArray<string> = ["online", "away", "busy"];

/** When a Busy ends, in server milliseconds; null for no end, which the platform says by leaving it out or as infinity. */
function busyEnd(status: PersonStatus): number | null {
    const end = typeof status.busy_until === "string" ? Date.parse(status.busy_until) : NaN;
    return Number.isFinite(end) ? end : null;
}

/** What a decision draws at a server time: nothing past her row's end, and Away once a Busy has ended under a live row. */
function drawnAt(entry: StatusEntry | undefined, serverNow: number): Drawn | null {
    const status = entry?.status;
    if (!status?.state || !(Date.parse(status.until ?? "") > serverNow)) {
        return null;
    }
    if (status.state !== "busy") {
        return { state: status.state, busyUntil: null };
    }

    // Her page is still live, so she is there and only her activity is unknown, until the next
    // answer or her next signal says Online.
    const end = busyEnd(status);
    return end !== null && end <= serverNow ? { state: "away", busyUntil: null } : { state: "busy", busyUntil: end };
}

/**
 * Reads a status as the platform sends it: the decision time as the number it was sent as, never
 * parsed from text, where two decisions inside one millisecond would lose their order; a decision
 * time alone is nothing from then.
 *
 * @param raw The status as it arrived.
 *
 * @returns The entry, or null for something unreadable, which writes nothing.
 */
function readStatus(raw: unknown): StatusEntry | null {
    if (typeof raw !== "object" || raw === null || Array.isArray(raw)) {
        return null;
    }
    const sent = raw as Partial<PersonStatus>;
    if (typeof sent.as_of !== "number" || !Number.isSafeInteger(sent.as_of)) {
        return null;
    }
    if (sent.state === undefined) {
        return { status: null, asOf: sent.as_of };
    }
    if (!states.includes(sent.state) || typeof sent.until !== "string" || !Number.isFinite(Date.parse(sent.until))) {
        return null;
    }

    return {
        status: {
            as_of: sent.as_of,
            state: sent.state,
            until: sent.until,
            ...(typeof sent.busy_until === "string" ? { busy_until: sent.busy_until } : {}),
            ...(sent.auto === "online" || sent.auto === "away" ? { auto: sent.auto } : {})
        },
        asOf: sent.as_of
    };
}

/**
 * Creates the store of others' online status.
 *
 * @param dependencies What status reaches outside itself.
 *
 * @returns The store.
 */
export function createPresenceStatus(dependencies: PresenceStatusDependencies): PresenceStatus {
    const { settings, isBlocked, isStopped } = dependencies;
    const self = dependencies.personAliasGuid.toLowerCase();

    /** Each person's newest decision, by lower-case primary alias; kept when it says nothing, for its time. */
    const entries = new Map<string, StatusEntry>();

    /** What each person was last drawn as, so a write or a deadline knows whether that changed. */
    const shown = new Map<string, string>();

    /** Each person's revision, the page's count when what they draw last changed; the one thing a face reads to draw again. */
    const revisions = shallowReactive(new Map<string, number>());
    let revision = 0;

    /** The server's clock less the page's, from the newest token's mint time. */
    let offsetMs = 0;

    /** The one timer, for the soonest row's end or Busy's end. */
    let deadline: ReturnType<typeof setTimeout> | null = null;

    const registrations = new Set<{ people: () => readonly string[], rank: WatchRank }>();

    const isOn = (): boolean => settings().flags.online === true;
    const serverNow = (): number => Date.now() + offsetMs;

    /** What a person draws now, words and all, or null for nothing: what a face reads, and what a change is measured against. */
    function drawnNow(key: string): Omit<FaceStatus, "revision"> | null {
        if (!isOn() || isBlocked(key)) {
            return null;
        }
        const drawn = drawnAt(entries.get(key), serverNow());
        if (!drawn) {
            return null;
        }
        const words = statusWords(drawn.state, drawn.busyUntil !== null ? new Date(drawn.busyUntil) : null, new Date());
        return { state: drawn.state, ...words, busyUntil: drawn.busyUntil };
    }

    /**
     * What a person draws, as a key that changes exactly when the badge or its words would: the
     * words are the viewer's day's, so the same end reads "tomorrow" before midnight and not after,
     * and an end moved inside one shown minute changes nothing.
     */
    function shownAs(key: string): string {
        const drawn = drawnNow(key);
        return drawn ? `${drawn.state}|${drawn.tip}` : "";
    }

    /** Moves a person's revision if what they draw changed; the entry is never touched. */
    function redraw(key: string): void {
        const next = shownAs(key);
        if (next === (shown.get(key) ?? "")) {
            return;
        }
        if (next) {
            shown.set(key, next);
        }
        else {
            shown.delete(key);
        }
        revisions.set(key, ++revision);
    }

    /** Redraws everyone held or drawn, after the clock moved under them, and sets the timer again. */
    function redrawAll(): void {
        for (const key of new Set([...entries.keys(), ...shown.keys()])) {
            redraw(key);
        }
        arm();
    }

    /**
     * Sets the one timer for the soonest moment what is drawn can change with no write: a row's
     * end, a Busy's end, or the viewer's next midnight while a Busy drawn ends on a later day, since
     * its words count days from the viewer's today.
     */
    function arm(): void {
        if (deadline !== null) {
            clearTimeout(deadline);
            deadline = null;
        }
        if (isStopped()) {
            return;
        }

        const now = serverNow();
        const today = startOfDay(new Date());
        let soonest = Infinity;
        for (const entry of entries.values()) {
            const status = entry.status;
            if (!status) {
                continue;
            }
            for (const end of [Date.parse(status.until ?? ""), status.state === "busy" ? busyEnd(status) : null]) {
                if (end !== null && end > now) {
                    soonest = Math.min(soonest, end);
                }
            }

            const drawn = drawnAt(entry, now);
            if (drawn?.busyUntil != null && startOfDay(new Date(drawn.busyUntil)) > today) {
                // the next day's start by the calendar, not 24 hours on, so a daylight saving change holds
                const midnight = new Date(today.getFullYear(), today.getMonth(), today.getDate() + 1).getTime();
                soonest = Math.min(soonest, midnight + offsetMs);
            }
        }
        if (soonest !== Infinity) {
            deadline = setTimeout(() => {
                deadline = null;
                if (!isStopped()) {
                    redrawAll();
                }
            }, Math.min(longestTimerMs, soonest - now));
        }
    }

    /**
     * Keeps a decision if it is the person's newest. One of the same instant as the one held is the
     * page's later word on it, so it is kept too.
     */
    function write(key: string, entry: StatusEntry): void {
        const held = entries.get(key);
        if (held && entry.asOf < held.asOf) {
            return;
        }
        entries.set(key, entry);
        redraw(key);
    }

    /** Whether a writer may write now: not once the page has stopped, and nothing while status is off. */
    function mayWrite(): boolean {
        return !isStopped() && isOn();
    }

    // Status switched off empties the store, so nothing held comes back if it is switched on again.
    const stopWatchingSwitch = watch(isOn, on => {
        if (!on) {
            entries.clear();
            redrawAll();
        }
    }, { flush: "sync" });

    return {
        exchanged: (asked, outcome) => {
            const map = outcome?.presence;
            if (!mayWrite() || typeof map !== "object" || map === null) {
                return;
            }
            const answered = new Map(Object.entries(map).map(([alias, sent]) => [alias.toLowerCase(), sent as unknown]));
            for (const alias of asked) {
                const key = alias.toLowerCase();
                const entry = answered.has(key) ? readStatus(answered.get(key)) : null;
                if (entry) {
                    write(key, entry);
                }
            }
            arm();
        },

        told: people => {
            if (!mayWrite()) {
                return;
            }
            for (const person of people) {
                const told = person as Partial<Pick<RosterMember, "person_alias_guid" | "status">> | null;
                // a person with no status key was not covered, which says nothing of her
                if (typeof told !== "object" || told === null || typeof told.person_alias_guid !== "string" || !("status" in told)) {
                    continue;
                }
                const entry = readStatus(told.status);
                if (entry) {
                    write(told.person_alias_guid.toLowerCase(), entry);
                }
            }
            arm();
        },

        carried: (alias, carried) => {
            const key = typeof alias === "string" ? alias.toLowerCase() : null;
            // The person's own face is hers to draw from her own choice and her own answers; a copy
            // carried to her, as the room's typing row is, is never written for her.
            if (!mayWrite() || !key || key === self || isBlocked(key)) {
                return;
            }
            const entry = readStatus(carried);
            if (entry) {
                write(key, entry);
                arm();
            }
        },

        blockChanged: (aliases, asOf) => {
            if (!mayWrite()) {
                return;
            }
            for (const alias of aliases) {
                const key = alias.toLowerCase();
                const at = typeof asOf === "number" ? asOf : entries.get(key)?.asOf;
                if (at !== undefined) {
                    write(key, { status: null, asOf: at });
                }
                // A block is part of what is drawn whatever the write kept: a newer status than the
                // block's time is held, and draws nothing until the unblock, which draws it again.
                redraw(key);
            }
            arm();
        },

        clockFrom: issuedAtSeconds => {
            if (isStopped() || !Number.isFinite(issuedAtSeconds)) {
                return;
            }
            offsetMs = issuedAtSeconds * 1000 - Date.now();
            redrawAll();
        },

        statusOf: guid => {
            const key = guid.toLowerCase();
            // read first and always, so a face reading it draws again whenever it moves
            const at = revisions.get(key) ?? 0;
            const drawn = drawnNow(key);
            return drawn ? { ...drawn, revision: at } : null;
        },

        register: (people, rank) => {
            const registration = { people, rank };
            registrations.add(registration);
            return () => {
                registrations.delete(registration);
            };
        },

        watched: () => {
            const watched = [self];
            const seen = new Set(watched);
            // a sort keeps each rank's faces in the order they registered, and each face's people in its own order
            for (const { people } of [...registrations].sort((a, b) => a.rank - b.rank)) {
                for (const alias of people()) {
                    const key = alias.toLowerCase();
                    if (!seen.has(key)) {
                        seen.add(key);
                        watched.push(key);
                    }
                }
            }
            return watched.slice(0, watchedMax);
        },

        stop: () => {
            if (deadline !== null) {
                clearTimeout(deadline);
                deadline = null;
            }
            stopWatchingSwitch();
            registrations.clear();
        }
    };
}

/**
 * A face's status: registers the people a face draws while it is mounted, so the next exchange
 * asks about them, and answers what to draw for each. A part drawn without the shell has no store,
 * registers nothing and draws no status.
 *
 * @param people The people the face draws, by primary alias.
 * @param rank How early they are kept when the list is cut.
 *
 * @returns What to draw for a person, or null for nothing.
 */
export function useFaceStatus(people: () => readonly string[], rank: WatchRank): (guid: string) => FaceStatus | null {
    const status = inject(presenceStatusKey, null);
    if (!status) {
        return () => null;
    }

    let unregister: (() => void) | null = null;
    onMounted(() => {
        unregister = status.register(people, rank);
    });
    onUnmounted(() => {
        unregister?.();
        unregister = null;
    });

    return status.statusOf;
}

// #endregion Status
