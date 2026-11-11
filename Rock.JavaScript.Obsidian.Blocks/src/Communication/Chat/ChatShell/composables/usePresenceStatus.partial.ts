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
// Online status, the one store of it on the page, others' and her own face's: each person's
// newest decision, by the platform's decision time, whichever answer, reader or carried signal
// brings it and in whatever order they land, since an answer decided earlier can arrive later.
// What a face draws is worked out from that decision and the server's clock, never written back
// over it, and a person's revision moves only when what is drawn changes, so a face mounted at a
// revision draws as state and fades only for a change after it.
//
// The page's side of the exchange lives here too, since it is what keeps this store current: the
// page's client id, the body every token exchange carries (her heartbeat, and on a refresh the
// people to ask about), her input, when to ask before the next refresh, and the goodbye as the page
// closes. The session alone asks the platform; this only says what to ask and when it is worth
// asking early.
//
// Her own face is drawn here too, from her own Busy and Appear offline as her settings hold them,
// her input on this page and her own answer, so it moves by the same revisions as everyone else's.
//
// Presence creates it beside typing and stops it with typing, so it has one owner, but the two
// share no state: this holds its own entries, revisions, timer, faces registered and page
// listeners, and reads only the settings, her own Busy and Appear offline, the blocks, the
// session, the page, the stopped state and the person's own id. It is a file of its own for that
// reason, so each half reads alone and neither grows past what one file should hold.

import { newGuid } from "@Obsidian/Utility/guid";
import { inject, InjectionKey, onMounted, onUnmounted, shallowReactive, watch } from "vue";
import { startOfDay } from "../feedLayout.partial";
import { FaceStatus, PersonStatus, RosterMember } from "../types.partial";
import { ownStatusWords, statusWords } from "../ui/face.partial";
import type { PageEventTargets } from "./useMarkRead.partial";
import type { MeSettings } from "./usePrefs.partial";
import type { ChatSession, ChatSettings, RefreshReason } from "./useSession.partial";

/** What status reaches outside itself. */
export type PresenceStatusDependencies = {
    /** The settings, watched, so it must be reactive: the switch and the idle line are kept as they change, since a face reads only its revision. */
    settings: () => ChatSettings;

    /** The person's primary alias, whose own carried copy is never written. */
    personAliasGuid: string;

    /**
     * Her own Busy and Appear offline as her settings hold them, which her own face draws before
     * any answer; null until her settings have been read, when the page knows neither.
     */
    me: () => Pick<MeSettings, "busy_until" | "appear_offline"> | null;

    isBlocked: (personAliasGuid: string) => boolean;

    /** Whether the shell has stopped or its session ended, read at each resumed step. */
    isStopped: () => boolean;

    /** The page, whose input, hiding, closing and showing the status watches. */
    targets: PageEventTargets;

    /** Says goodbye for a client id as the page closes, by a call that outlives the page and never asks for a token. */
    leave: (clientId: string) => void;

    /** The session, which alone asks the platform: status reads when it refreshes next and asks it to refresh early. */
    session: Pick<ChatSession, "refresh" | "nextRefreshAt" | "tokenLifeSeconds" | "inFlight">;
};

/**
 * What a page tells and asks the platform with each token exchange: its client and how long the
 * person has done nothing, which beat her online, and on a refresh the people to ask about.
 */
/* eslint-disable @typescript-eslint/naming-convention */
export type ExchangeBody = {
    client_id: string;
    kind: "web";
    idle_seconds: number;
    watched?: string[];
};
/* eslint-enable @typescript-eslint/naming-convention */

/**
 * How early a face's people are kept when the list of people watched is cut: 0 the person
 * themselves, 1 the open conversation's person and an open card or details, 2 the members, the
 * people pickers' results and New message's choice, 3 the sidebar's two-person rows.
 */
export type WatchRank = 0 | 1 | 2 | 3;

/** Online status on this page, others' and her own face's: the only store of it, and the only thing that writes it. */
export type PresenceStatus = {
    /**
     * A token exchange answered. Each person its body asked about is written as of the answer's
     * one decision time: what its told says of her, or nothing when it leaves her out. A presence
     * left out, as when the read did not answer in time, or a failed exchange, writes nothing. A
     * landed body is what the platform now holds of her input, and a landed presence, or the
     * start, makes the badges fresh; a failed exchange changes neither.
     *
     * @param asked The people the exchange's body watched.
     * @param outcome The exchange's answer, or null for one that failed.
     */
    exchanged: (asked: readonly string[], outcome: { presence?: unknown } | null) => void;

    /** The page's client id, which every body names: one a page, and a new one once a goodbye has spent it; none once stopped. */
    clientId: () => string | null;

    /**
     * The body an exchange asked for a reason carries, built as its request goes so it reports her
     * newest input: the people watched only on a refresh that asks about them, and no body at all
     * once the latest answer said status is off.
     */
    exchangeBody: (reason: RefreshReason) => ExchangeBody | null;

    /** A refresh, or a try of the start, has settled: an early ask held while it was out is made now if it is still due. */
    settled: (reason: RefreshReason, ok: boolean) => void;

    /** Draws everyone again against the clock now and sets the timer again, for a clock that moved with no timer firing. */
    rearm: () => void;

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

/** What a face reads of the status store: what to draw, and who it draws so they are asked about. */
export type FaceStatusSource = Pick<PresenceStatus, "statusOf" | "register">;

/** The shell's status store, provided to every face that draws one, read only. */
export const presenceStatusKey: InjectionKey<FaceStatusSource> = Symbol("chat-presence-status");

// #region Status

/** The most people one exchange asks about: the platform's own cap, past which it asks about nobody. */
const watchedMax = 200;

/** The longest a timer waits, past which a browser fires it at once. */
const longestTimerMs = 2_147_483_647;

/** An id the platform takes: one id not shaped as one would make it refuse the whole body, beat included. */
const uuidPattern = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;

/**
 * What counts as the person doing something on the page. Never a scroll: the feed scrolls itself
 * to keep her at the newest message as others post and type, and a scroll set by script arrives as
 * trusted as hers, while every scroll she makes starts with a wheel, a touch, a key or a pointer.
 */
const inputEvents = ["keydown", "pointerdown", "pointermove", "wheel", "touchstart"];

/** How often an input does more than note its time, since a moving pointer fires many a second. */
const inputWorkGapMs = 1000;

/** The least time between two early asks for one cause, so a page in steady use costs at most one a minute. */
const earlyAskGapMs = 60_000;

/**
 * How long a decision that draws nothing is kept, which must outlast any status row: once it has
 * passed, no decision older than the one kept can still be live, so none arriving late can draw.
 * The platform does not send its row life, which is the token's life, five minutes at most, plus
 * the slack it gives a page whose timers a browser holds back, under ten minutes in all today.
 * Twenty minutes is about twice that, so the platform can lengthen either without a page here
 * letting go of a nothing that still keeps an older status from drawing; holding longer costs
 * only the people written in that time.
 */
const heldNothingSeconds = 1200;

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

/**
 * Her own Busy as her settings hold it, at a server time: its end in server milliseconds, null
 * for no end (infinity), or undefined when she is not Busy, its end passed included.
 */
function ownBusyEnd(busyUntil: string | null, serverNow: number): number | null | undefined {
    if (busyUntil === "infinity") {
        return null;
    }
    const end = busyUntil === null ? NaN : Date.parse(busyUntil);
    return end > serverNow ? end : undefined;
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

/** Whether a value is a JSON object, not null or an array. */
function isRecord(value: unknown): value is Record<string, unknown> {
    return typeof value === "object" && value !== null && !Array.isArray(value);
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
    if (!isRecord(raw)) {
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
 * Reads the token exchange's presence: one decision time for the whole answer, sent as the number
 * it was decided as, and told, each watched person it tells something of by primary alias, in
 * whatever case it arrived in.
 *
 * @param raw The presence as it arrived.
 *
 * @returns The answer, or null for something unreadable, which writes nothing for anyone.
 */
function readWatchAnswer(raw: unknown): { asOf: number, told: Map<string, unknown> } | null {
    if (!isRecord(raw) || typeof raw.as_of !== "number" || !Number.isSafeInteger(raw.as_of) || !isRecord(raw.told)) {
        return null;
    }

    return {
        asOf: raw.as_of,
        told: new Map(Object.entries(raw.told).map(([alias, sent]) => [alias.toLowerCase(), sent]))
    };
}

/**
 * Creates the store of online status, others' and her own face's.
 *
 * @param dependencies What status reaches outside itself.
 *
 * @returns The store.
 */
export function createPresenceStatus(dependencies: PresenceStatusDependencies): PresenceStatus {
    const { settings, isBlocked, isStopped, targets, leave, session, me } = dependencies;
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

    /** The one timer, for the soonest row's end or Busy's end, and when it is due by the page's clock. */
    let deadline: ReturnType<typeof setTimeout> | null = null;
    let deadlineAt: number | null = null;

    const registrations = new Set<{ people: () => readonly string[], rank: WatchRank }>();

    /**
     * The switch, the idle line and her own Busy and Appear offline, each kept as a plain value
     * that moves only when it changes, by the watches below, which redraw whoever that changes. A
     * face reads only its revision: every token exchange replaces the settings whole, so a face
     * that read them would draw again at each exchange with nothing changed.
     */
    const switchOf = (): boolean => settings().flags.online === true;
    const idleLineOf = (): number => settings().limits.presence_idle_seconds * 1000;
    const mineOf = (): Pick<MeSettings, "busy_until" | "appear_offline"> | null => {
        const read = me();
        return read ? { busy_until: read.busy_until, appear_offline: read.appear_offline } : null;
    };
    let isSwitchOn = switchOf();
    let idleLine = idleLineOf();
    let mine = mineOf();
    const isOn = (): boolean => isSwitchOn;
    const idleLineMs = (): number => idleLine;
    const serverNow = (): number => Date.now() + offsetMs;

    /** When she last did something while the page was visible, its load counting, and when an input last did more than note it. */
    let inputAt = Date.now();
    let inputWorkedAt = -Infinity;

    /** What a person draws now, words and all, or null for nothing: what a face reads, and what a change is measured against. */
    function drawnNow(key: string): Omit<FaceStatus, "revision"> | null {
        if (key === self) {
            return ownDrawnNow();
        }
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
     * Her own face. What she chose comes first, from her settings, so it shows the moment she
     * chooses it: Appear offline, then Busy. Then her input, which this page sees before any
     * answer could say it: Online while it is inside the idle line. Then the platform's answer for
     * her, her automatic state where it says Busy and her settings say it has ended, and nothing
     * once it says nothing; and Online before it has said anything, since she has the page open.
     * Nothing before her settings are read, since they may say Appear offline or Busy.
     */
    function ownDrawnNow(): Omit<FaceStatus, "revision"> | null {
        if (!isOn() || !mine) {
            return null;
        }
        if (mine.appear_offline) {
            return own("hidden", null);
        }
        const busyUntil = ownBusyEnd(mine.busy_until, serverNow());
        if (busyUntil !== undefined) {
            return own("busy", busyUntil);
        }
        if (isInputFresh()) {
            return own("online", null);
        }
        const entry = entries.get(self);
        if (!entry) {
            return own("online", null);
        }
        const drawn = drawnAt(entry, serverNow());
        if (!drawn) {
            return null;
        }
        // Busy by an answer decided before her own settings ended it: what she is underneath
        return own(entry.status?.state === "busy" ? entry.status.auto ?? "away" : drawn.state, null);
    }

    /** Her own face in her own words, which say what a Busy does to her. */
    function own(state: FaceStatus["state"], busyUntil: number | null): Omit<FaceStatus, "revision"> {
        const tip = ownStatusWords(state, busyUntil !== null ? new Date(busyUntil) : null, new Date());
        // the label continues her name, as others' does
        return { state, label: `, ${tip.charAt(0).toLowerCase()}${tip.slice(1)}`, tip, busyUntil };
    }

    /** Whether her newest input on this page is inside the idle line, past which the platform counts her Away. */
    function isInputFresh(): boolean {
        return idleLineMs() > 0 && Date.now() - inputAt < idleLineMs();
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

    /** Redraws everyone held or drawn, her own face always, after the clock moved under them, and sets the timer again. */
    function redrawAll(): void {
        for (const key of new Set([self, ...entries.keys(), ...shown.keys()])) {
            redraw(key);
        }
        arm();
    }

    /**
     * Sets the one timer for the soonest moment what is drawn can change with no write: a row's
     * end, a Busy's end, or the viewer's next midnight while a Busy drawn ends on a later day, since
     * its words count days from the viewer's today; and for her own face, her Busy's end and her
     * input passing the idle line.
     *
     * It walks every entry, so it also lets go of the people who can draw nothing again from what
     * is held: the roster, the searches and the readers write whoever they show, and a page left
     * open would otherwise walk them all on every write. An entry may go once it draws nothing and
     * the server's clock is past its decision time plus a bound longer than any row lasts, since
     * any older decision's row ended before then and could not draw if it arrived late. Her own is
     * kept: it says what her face draws once her input is old.
     */
    function arm(): void {
        if (deadline !== null) {
            clearTimeout(deadline);
            deadline = null;
            deadlineAt = null;
        }
        if (isStopped()) {
            return;
        }

        const now = serverNow();
        const today = startOfDay(new Date());
        let soonest = Infinity;
        for (const [key, entry] of entries) {
            if (key !== self && entry.asOf / 1000 + heldNothingSeconds * 1000 < now && !shown.has(key) && !drawnAt(entry, now)) {
                entries.delete(key);
                continue;
            }
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
        if (isOn() && mine && !mine.appear_offline) {
            const busyUntil = ownBusyEnd(mine.busy_until, now);
            if (busyUntil !== undefined) {
                soonest = Math.min(soonest, busyUntil ?? Infinity);
            }
            else if (isInputFresh()) {
                soonest = Math.min(soonest, inputAt + idleLineMs() + offsetMs);
            }
        }
        if (soonest !== Infinity) {
            const wait = Math.min(longestTimerMs, soonest - now);
            deadlineAt = Date.now() + wait;
            deadline = setTimeout(() => {
                deadline = null;
                deadlineAt = null;
                if (!isStopped()) {
                    redrawAll();
                }
            }, wait);
        }
    }

    /** Draws everyone again against the clock now, unless the page has stopped. */
    function rearm(): void {
        if (!isStopped()) {
            redrawAll();
        }
    }

    /** The people the next exchange asks about: the person, then every face's by rank, each once, only ids shaped as one, cut at the platform's cap. */
    function watchedNow(): string[] {
        const watched: string[] = [];
        const seen = new Set<string>();
        const add = (alias: string): void => {
            const key = alias.toLowerCase();
            if (!seen.has(key) && uuidPattern.test(key)) {
                seen.add(key);
                watched.push(key);
            }
        };
        add(self);
        // a sort keeps each rank's faces in the order they registered, and each face's people in its own order
        for (const { people } of [...registrations].sort((a, b) => a.rank - b.rank)) {
            people().forEach(add);
        }
        return watched.slice(0, watchedMax);
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

    // Status switched off empties the store, so nothing held comes back if it is switched on again;
    // either way her own face, which draws before any answer, is drawn again.
    const stopWatchingSwitch = watch(switchOf, on => {
        isSwitchOn = on;
        if (!on) {
            entries.clear();
        }
        redrawAll();
    }, { flush: "sync" });

    // The idle line moved: only her own face reads it, and her next deadline with it.
    const stopWatchingIdleLine = watch(idleLineOf, line => {
        idleLine = line;
        if (!isStopped()) {
            redraw(self);
            arm();
        }
    }, { flush: "sync" });

    // Her own Busy or Appear offline changed in her settings, or they were read: her face draws it at once.
    const stopWatchingMe = watch(() => [me() !== null, me()?.busy_until, me()?.appear_offline], () => {
        mine = mineOf();
        if (!isStopped()) {
            redraw(self);
            arm();
        }
    }, { flush: "sync" });

    // What her face draws from the start is what it was last drawn as, so the first change from
    // it, the switch turned off included, moves her revision.
    redraw(self);

    // #region The page

    /** The page's client id, null once a goodbye has spent it, and the id a body last carried, the only one a goodbye names. */
    let pageId: string | null = null;
    let carriedId: string | null = null;

    /** The body out, so its answer knows the reason it went for, the id and input it reported and whether it asked about people. */
    let out: { reason: RefreshReason, inputAt: number, isSent: boolean, isWatched: boolean, id: string | null } | null = null;

    /**
     * Whether any exchange has landed, the input the latest landed body reported (what the
     * platform holds of her; -Infinity for nothing held), and when the badges were last made
     * fresh: the start's landing, or an answer that carried status.
     */
    let hasLanded = false;
    let heldInputAt: number | null = null;
    let freshAt: number | null = null;

    /**
     * Whether the latest landed body asked about people. An answer to one with no status is the
     * platform saying not now (its watch ran past its wait, its breaker is open, or it answered her
     * row within its pace), so asking again at once would be refused the same way.
     */
    let hasLandedWatched = false;

    /** One early ask held while a refresh was out, when the last early ask went, and when the last her input made went. */
    let isAskHeld = false;
    let earlyAskedAt = -Infinity;
    let inputAskedAt = -Infinity;

    /** Whether a body goes: before any answer, which the start's first try is, and while the latest answer says status is on. */
    const isBodySent = (): boolean => !hasLanded || isOn();

    /** The page's client id, minted afresh after a goodbye spent the last. */
    function ownId(): string {
        pageId ??= newGuid().toLowerCase();
        return pageId;
    }

    /**
     * Whether what the platform holds of her runs out before the next refresh would tell it
     * more: her held input plus the idle line falls before it. Asked again once a refresh lands,
     * only an input newer than the body that landed is news.
     */
    function isInputDue(isNewerNeeded: boolean): boolean {
        const next = session.nextRefreshAt();
        return heldInputAt !== null && next !== null && heldInputAt + idleLineMs() < next && (!isNewerNeeded || inputAt > heldInputAt);
    }

    /** Whether the page is visible with badges older than a token's life. */
    function isStale(): boolean {
        return targets.document.visibilityState === "visible" && freshAt !== null && Date.now() - freshAt > session.tokenLifeSeconds() * 1000;
    }

    /** Whether asking early can mean anything: after an answer, with status on and an idle line to measure against. */
    function mayAskEarly(): boolean {
        return hasLanded && isOn() && idleLineMs() > 0;
    }

    /**
     * Whether a cause may ask early now: her input a minute after the last ask her input made, a
     * stale page a minute after any early ask. A stale page's ask never spends her input's
     * minute, which would refuse the first move she makes on her return and leave the platform
     * holding the idle time she came back from.
     */
    function isMinuteOver(isHerInput: boolean): boolean {
        return Date.now() - (isHerInput ? inputAskedAt : earlyAskedAt) >= earlyAskGapMs;
    }

    function askEarly(isHerInput: boolean): void {
        earlyAskedAt = Date.now();
        if (isHerInput) {
            inputAskedAt = earlyAskedAt;
        }
        void session.refresh("early");
    }

    /**
     * Something happened that may be worth an early refresh. While a refresh is out, or the
     * session waits after a failure, nothing is asked: one ask is held for the session's settle,
     * which knows what that refresh carried. Otherwise the ask goes if it is due, once a minute.
     */
    function trigger(isDue: () => boolean, isHerInput: boolean): void {
        if (isStopped()) {
            return;
        }
        if (session.inFlight() !== null || session.nextRefreshAt() === null) {
            isAskHeld = true;
            return;
        }
        if (mayAskEarly() && isMinuteOver(isHerInput) && isDue()) {
            askEarly(isHerInput);
        }
    }

    /** An input on a visible page: its time is kept every time, and once a second it may ask early. */
    function onInput(): void {
        if (targets.document.visibilityState !== "visible") {
            return;
        }
        const at = Date.now();
        inputAt = at;
        if (at - inputWorkedAt < inputWorkGapMs) {
            return;
        }
        inputWorkedAt = at;

        // A timer due before now has not fired: the machine slept with the page visible, which
        // fires none of the page's own events. Otherwise only her own face can have moved, back
        // to Online from past the idle line.
        if (deadlineAt !== null && at >= deadlineAt) {
            rearm();
        }
        else if (shownAs(self) !== (shown.get(self) ?? "")) {
            redraw(self);
            arm();
        }
        trigger(() => isInputDue(false), true);
    }

    function onVisibility(): void {
        if (targets.document.visibilityState === "visible") {
            rearm();
            trigger(isStale, false);
        }
    }

    /**
     * The page is put away or closed: a goodbye for the id a body carried, so she leaves the
     * others' faces in seconds rather than at her row's end, and never with status off, when the
     * platform keeps no row. The id is spent either way; a page restored goes on under a new one.
     */
    function onPageHide(): void {
        if (pageId !== null && pageId === carriedId && isBodySent() && !isStopped()) {
            leave(pageId);
        }
        pageId = null;
    }

    /**
     * A page restored from the back-forward cache is a page she asked for with Back, so it counts
     * as her input as a load does, and holds one ask for the sign-in's recheck to settle, which
     * decides it against what landed whether or not a show follows. It draws again against the
     * clock, which moved while the page ran no timer.
     */
    function onPageShow(event: unknown): void {
        if ((event as { persisted?: unknown } | null)?.persisted === true && !isStopped()) {
            inputAt = Date.now();
            isAskHeld = true;
            rearm();
        }
    }

    for (const type of inputEvents) {
        targets.window.addEventListener(type, onInput, { capture: true, passive: true });
    }
    targets.document.addEventListener("visibilitychange", onVisibility);
    targets.window.addEventListener("pagehide", onPageHide);
    targets.window.addEventListener("pageshow", onPageShow);

    // #endregion The page

    return {
        exchanged: (asked, outcome) => {
            if (isStopped()) {
                return;
            }
            const body = out;
            out = null;
            // a failed exchange reported nothing, so what the platform holds of her is as it was
            if (outcome === null) {
                return;
            }
            hasLanded = true;
            hasLandedWatched = body?.isWatched ?? false;
            // A body that named an id a goodbye has spent since it left beat a row already left,
            // which counts for nothing: the platform holds nothing of her, so her next ask is due
            // at once, under the page's new id.
            if (body?.isSent) {
                heldInputAt = body.id === pageId ? body.inputAt : -Infinity;
            }
            if (body?.reason === "start" || "presence" in outcome) {
                freshAt = Date.now();
            }

            // The switch read here is the answer before this one's, since the session applies an
            // answer's settings only after this returns. The platform answers status only with the
            // switch on in the same answer, and a body names people only after an answer said on.
            const answer = readWatchAnswer(outcome.presence);
            if (!mayWrite() || answer === null) {
                return;
            }
            // The answer's one decision time is every entry's, and a person asked about that its
            // told leaves out is told nothing as of then, as a reader's time alone says.
            for (const alias of asked) {
                const key = alias.toLowerCase();
                const sent = answer.told.has(key) ? answer.told.get(key) : {};
                const entry = isRecord(sent) ? readStatus({ ...sent, as_of: answer.asOf }) : null;
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

        watched: watchedNow,

        clientId: () => isStopped() ? null : ownId(),

        exchangeBody: reason => {
            if (isStopped()) {
                return null;
            }
            out = { reason, inputAt, isSent: false, isWatched: false, id: null };
            if (!isBodySent()) {
                return null;
            }
            const id = ownId();
            carriedId = id;
            out.isSent = true;
            out.id = id;
            // A refresh names the people to ask about once an answer said status is on; the start,
            // a recheck, a call refused as expired and a retry only beat, and are answered at once.
            const watched = hasLanded && (reason === "scheduled" || reason === "early") ? watchedNow() : [];
            out.isWatched = watched.length > 0;
            return {
                client_id: id,
                kind: "web",
                idle_seconds: Math.max(0, Math.floor((Date.now() - inputAt) / 1000)),
                ...(watched.length > 0 ? { watched } : {})
            };
        },

        settled: (_reason, ok) => {
            if (isStopped()) {
                isAskHeld = false;
                return;
            }
            // a failed one keeps the ask held for the next landing, whose body and answer decide it
            if (!ok || !isAskHeld) {
                return;
            }
            isAskHeld = false;

            if (!mayAskEarly() || session.inFlight() !== null || session.nextRefreshAt() === null) {
                return;
            }

            // Against what landed, each cause on its own minute. An input newer than its body asks
            // once her input's minute is over: whether one landing makes the next due rests on the
            // platform's idle line and token life, and with a line shorter than the refresh window
            // every landing would ask again. A page still stale asks once the minute is over, and
            // never after a body that asked about people, whose answer with no status is the
            // platform's not now; otherwise a refused ask would ask again at every landing.
            if (isInputDue(true) && isMinuteOver(true)) {
                askEarly(true);
            }
            else if (!hasLandedWatched && isMinuteOver(false) && isStale()) {
                askEarly(false);
            }
        },

        rearm,

        stop: () => {
            // A page stopped in place, as a block torn down without the page closing, says goodbye
            // too, so her row ends now rather than running on beside the next page's; the session
            // still holds its token here, and a goodbye never mints.
            if (pageId !== null && pageId === carriedId && isBodySent()) {
                leave(pageId);
            }
            pageId = null;
            if (deadline !== null) {
                clearTimeout(deadline);
                deadline = null;
                deadlineAt = null;
            }
            stopWatchingSwitch();
            stopWatchingIdleLine();
            stopWatchingMe();
            registrations.clear();
            isAskHeld = false;
            for (const type of inputEvents) {
                targets.window.removeEventListener(type, onInput, { capture: true });
            }
            targets.document.removeEventListener("visibilitychange", onVisibility);
            targets.window.removeEventListener("pagehide", onPageHide);
            targets.window.removeEventListener("pageshow", onPageShow);
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
