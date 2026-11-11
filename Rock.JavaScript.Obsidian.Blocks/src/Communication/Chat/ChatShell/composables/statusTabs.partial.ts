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
// Her status as her tabs of chat in this browser share it: a choice shows at once in every one of
// them and is saved by whichever still lives, one at a time under a Web Lock, its answer told to
// the others on a BroadcastChannel; a failed save is asked again on one retry clock for the
// browser rather than put back, unless the server answered about the choice itself. The settings
// create and stop it, and own what it shows.
/* eslint-disable @typescript-eslint/naming-convention */
import { newGuid } from "@Obsidian/Utility/guid";
import { reactive } from "vue";
import { failureWords } from "../errors.partial";
import { CallResult } from "../platformCall.partial";
import { ChatError } from "../types.partial";
import type { PageEventTargets } from "./useMarkRead.partial";
import type { Prefs, PrefsDependencies, PrefsState, StatusChoice } from "./usePrefs.partial";
import { saveFailed, SaveOutcome, SaveRule } from "./saveRule.partial";

/** A status choice as chat_set_status takes it: a Busy's end a server time, left out for none. */
export type StatusChange =
    | { status: "auto" }
    | { status: "busy", busy_until?: string }
    | { status: "appear_offline" };

/**
 * One choice of her status, made in one of her tabs: when, by that tab's clock and always above
 * every choice the tab holds, so two choices of one machine always order, and which tab made it.
 */
export type StatusRecord = { choice: StatusChoice, at: number, page: string };

/** The browser's Web Locks, as her status's save takes them; a site without them has none. */
export type StatusLocks = {
    request: (name: string, options: { signal?: AbortSignal }, callback: () => Promise<unknown>) => Promise<unknown>;
};

/** One BroadcastChannel, which reaches her other tabs of chat in this browser and never the tab that posts. */
export type StatusChannel = {
    postMessage: (message: unknown) => void;
    close: () => void;
    onmessage: ((event: MessageEvent) => void) | null;
};

/** What her status reaches outside the settings. */
export type StatusDependencies = {
    tenantId: string;

    /** Her primary alias, which names her lock and her channel, and the person the lock may send for. */
    personAliasGuid: string;

    /** The person the session's token names now, lower-cased, or null. */
    currentPerson: () => string | null;

    /** chat_set_status through the shell's call path, which a deadline can end by the signal. */
    saveStatus: (change: StatusChange, signal: AbortSignal) => Promise<CallResult>;

    /** Puts a read only refusal of her own choice on this tab as its own refused call would, though another tab sent it. */
    applyRefusal: (error: ChatError) => void;

    /** The browser's Web Locks; without them each tab holds and saves only the choices it made. */
    locks?: StatusLocks;

    /** Opens a BroadcastChannel by name; without one the tab is alone. */
    channel?: (name: string) => StatusChannel;

    /** The window, whose pagehide and pageshow say the tab left for the back-forward cache and came back. */
    targets?: PageEventTargets["window"];
};

/**
 * The first wait before a status save that failed is asked again, doubling with each failure to
 * the cap. An estimate: long enough that a browser of tabs failing together through an outage
 * asks a few times a minute, short enough that a choice made offline goes soon after the network
 * returns, which also brings the next ask forward.
 */
const statusRetryFirstMs = 10_000;

/** The longest wait between asks of a status save that keeps failing. */
const statusRetryCapMs = 60_000;

/**
 * The longest a 429's Retry-After holds a status save, whatever a proxy names: a day's value would
 * park her choice a day. An estimate; her network or the platform coming back asks sooner.
 */
const statusRetryAfterCapMs = 300_000;

/** The longest wait the retry clock can name, the backoff's cap with its spread being shorter. */
const statusLongestWaitMs = statusRetryAfterCapMs;

/** A status save's outcome, a failure marked with whether its try reached the platform at all. */
export type StatusSaveOutcome = SaveOutcome & { isUnreached?: boolean };

/**
 * How a status save's answer leaves her choice. Only the server's answer about the choice itself
 * refuses it: one of our codes for a 4xx, or read only, which the writer check refuses while her
 * pages still beat, so a choice asked again would show on her own faces and never on anyone
 * else's. Everything that is about the path rather than the choice is asked again on a backoff
 * spread by a random draw, the choice kept: a try that reached nothing, which is no network or the
 * save's deadline (status 0), a proxy giving up (408), or the platform failing or down for
 * maintenance (5xx); and a token the refresh could not renew (401). Only a try that reached
 * nothing is marked so, since only its failure says nothing once the network or the platform is
 * back; the 401 already had its fresh token and second try inside its flight. A 429 is a pace whoever set it, a gateway's carrying no code of ours, so
 * it waits the longer of the backoff and its Retry-After. Any other answer is refused, since
 * asking the same again would meet the same answer.
 *
 * @param result What the shell's call path answered.
 * @param failures How many tries of this choice have failed, this one included, from one.
 * @param random A number from 0 up to 1 that spreads the wait; the browser's by default.
 *
 * @returns The outcome the save rule acts on, marked when the try reached nothing.
 */
export function statusSaveOutcome(result: CallResult, failures: number, random: () => number = Math.random): StatusSaveOutcome {
    if (result.ok) {
        return { isSaved: true, data: result.data };
    }

    // read only's words are the banner's: the church's, or the server's own sentence, or ours
    const failure = failureWords(result.error);
    if (result.error.code === "rpc.read_only") {
        return { isSaved: false, failure };
    }

    // a count another tab posted may be missing or malformed, and counts as the first failure
    const count = Number.isFinite(failures) && failures >= 1 ? Math.floor(failures) : 1;
    const backoffMs = Math.round(Math.min(statusRetryCapMs, statusRetryFirstMs * 2 ** (count - 1)) * (0.8 + 0.4 * random()));
    if (result.status === 429) {
        return { isSaved: false, failure, retryInMs: Math.max(backoffMs, Math.min(statusRetryAfterCapMs, (result.retryAfterSeconds ?? 0) * 1000)) };
    }

    const isUnreached = result.status === 0 || result.status === 408 || result.status >= 500;
    if (isUnreached || result.status === 401) {
        return { isSaved: false, failure, retryInMs: backoffMs, isUnreached };
    }

    return { isSaved: false, failure };
}

/** How long a choice waits after it was made before it is sent, so quick choices make one call. */
const statusPauseMs = 1000;

/**
 * How much longer than its maker a tab that took a choice from another waits before sending it,
 * so while the choosing tab lives its own save normally lands first and its post retires every
 * copy. An estimate.
 */
const statusHandOffMs = 2000;

/** How long a status save is waited for before it is given up as reaching nothing, and asked again. */
const statusDeadlineMs = 15_000;

/** The least time between two posts of her input to her other tabs, which only need it inside the idle line. */
const inputPostGapMs = 30_000;

/** A status save given up at its deadline, read as one that reached nothing. */
const unreached: CallResult = { ok: false, status: 0, error: { code: "rpc.transport", severity: "failed", text: null }, retryAfterSeconds: null };

/** The retry clock with no wait. */
const noWait: RetryClock = { retryAt: 0, failures: 0, retryPage: null };

/** What the lock's holder answers when it sent nothing: a saved post covered the choice, or this tab may not send it. */
const nothingSent: SaveOutcome = { isSaved: true, sent: null };

/**
 * A choice's answer as every tab of hers hears it: saved, with the value sent, which every tab then
 * holds as saved whatever the server's answer shaped; or refused, with the record's tab and the
 * refusal whole, which only that tab says.
 */
type SavedPost =
    | { kind: "saved", choiceAt: number, me: StatusChoice }
    | { kind: "saved", choiceAt: number, page: string, refused: true, error: ChatError };

/**
 * The browser's one retry clock for her choice: when it is next asked after a failure, on
 * Date.now()'s clock, which her tabs share; how many tries have failed in a row; and the tab that
 * failed last, which asks at that time while every other tab waits a hand-off more, so it sends
 * only if that tab has closed. No wait is 0.
 */
type RetryClock = { retryAt: number, failures: number, retryPage: string | null };

/** What her tabs say to each other on her channel. */
type StatusPost =
    | { kind: "choice", record: StatusRecord }
    | SavedPost
    | { kind: "failed", choiceAt: number, retryAt: number, failures: number, page: string }
    | { kind: "hello" }
    | ({ kind: "held", covered: number, me: StatusChoice | null, meStamp: number } & RetryClock)
    | { kind: "input", at: number };

/** Her status's half of the settings. */
export type StatusTabs = Pick<Prefs, "setStatus" | "shownStatus" | "isChoicePending" | "noteInput" | "statusWake" | "stop"> & {
    /** A sidebar load has written her status into me. */
    sidebarRead: () => void;
};

/** Whether a value is a number a post may carry: finite, so no comparison with it is ever false both ways. */
function isTime(value: unknown): value is number {
    return typeof value === "number" && Number.isFinite(value);
}

/** Whether a value is her status in her settings' two fields. */
function isChoice(value: unknown): value is StatusChoice {
    const choice = value as Partial<StatusChoice> | null;
    return typeof choice === "object" && choice !== null
        && (choice.busy_until === null || typeof choice.busy_until === "string")
        && typeof choice.appear_offline === "boolean";
}

/**
 * A post from another tab of hers as this release reads it, or null for one it cannot: a tab of
 * another release may still be open after an upgrade, and a field it left out or shaped another
 * way would otherwise stick in this tab for good, as a covered of NaN that no choice is above.
 *
 * @param data What the channel delivered.
 *
 * @returns The post, or null.
 */
function readPost(data: unknown): StatusPost | null {
    const post = data as Record<string, unknown> | null;
    if (typeof post !== "object" || post === null) {
        return null;
    }
    const record = post.record as Record<string, unknown> | null | undefined;
    const error = post.error as Record<string, unknown> | null | undefined;
    switch (post.kind) {
        case "choice":
            return typeof record === "object" && record !== null && isTime(record.at) && typeof record.page === "string" && isChoice(record.choice)
                ? post as StatusPost
                : null;
        case "saved":
            if (!isTime(post.choiceAt)) {
                return null;
            }
            return post.refused === true
                ? typeof post.page === "string" && typeof error === "object" && error !== null && typeof error.code === "string" ? post as StatusPost : null
                : isChoice(post.me) ? post as StatusPost : null;
        case "failed":
            return isTime(post.choiceAt) && isTime(post.retryAt) && isTime(post.failures) && typeof post.page === "string" ? post as StatusPost : null;
        case "hello":
            return post as StatusPost;
        case "held":
            return isTime(post.covered) && isTime(post.meStamp) && (post.me === null || isChoice(post.me))
                && isTime(post.retryAt) && isTime(post.failures) && (post.retryPage === null || typeof post.retryPage === "string")
                ? post as StatusPost
                : null;
        case "input":
            return isTime(post.at) ? post as StatusPost : null;
        default:
            return null;
    }
}

/**
 * A choice as chat_set_status takes it.
 *
 * @param choice The choice, in her settings' two fields.
 *
 * @returns The change.
 */
function statusChange(choice: StatusChoice): StatusChange {
    if (choice.appear_offline) {
        return { status: "appear_offline" };
    }
    if (choice.busy_until === null) {
        return { status: "auto" };
    }
    // a Busy with no end is sent with none, which the server holds as infinity
    return choice.busy_until === "infinity" ? { status: "busy" } : { status: "busy", busy_until: choice.busy_until };
}

/**
 * Creates her status's half of the settings: her choice, shown at once in every tab of hers in this
 * browser and saved by whichever of them still lives.
 *
 * A choice is a record, timed by the tab that made it and posted to the others. Every tab that
 * holds it queues one save of it, the choosing tab after the pause and the others a hand-off
 * later, so if the choosing tab closes first another sends it. One Web Lock lets one tab send at
 * a time; the holder waits out the newest record's pause, sends the newest only if no saved post
 * has covered it, and posts how it went before letting the lock go, which retires every other
 * copy. A tab shows its newest record while nothing has covered it, else the last saved value,
 * which a sidebar load and every landed save write. A tab opened, or come back from the
 * back-forward cache, says hello and is told what the others hold. Without Web Locks a tab holds
 * and sends only the records it made, and says no hello.
 *
 * @param state The settings' state, whose me holds her saved status.
 * @param rule The settings' save rule, which owns when the save goes.
 * @param dependencies What the settings reach outside themselves.
 * @param isStopped Whether the settings have stopped.
 * @param announce Says a sentence to screen readers.
 *
 * @returns Her status's half.
 */
export function createStatusTabs(state: PrefsState, rule: SaveRule, dependencies: PrefsDependencies, isStopped: () => boolean, announce: (text: string, isAssertive: boolean) => void): StatusTabs {
    const reach = dependencies.status;
    const person = (reach?.personAliasGuid ?? "").toLowerCase();
    const names = `${(reach?.tenantId ?? "").toLowerCase()}:${person}`;
    const lockName = `rock-chat:status:${names}`;
    const locks = reach?.locks;

    /** This tab's own id for the records it makes; minted once, as the page's client id is not. */
    const tabId = newGuid();

    /**
     * The newest record this tab made or heard, the newest choice any saved post covered, and
     * whether her saved status is known here. Reactive, since what her faces show reads them.
     */
    const tab = reactive({ newest: null as StatusRecord | null, covered: 0, isMeKnown: false });

    /** The choice time of what last wrote me: a saved post's, or what this tab had covered at a sidebar load. */
    let meStamp = 0;

    /** When the newest record's pause ends, on performance.now()'s clock: a second after this tab made or took it. */
    let pauseEnds = -Infinity;

    /** The retry clock as this tab knows it; any saved post ends the run. */
    const clock: RetryClock = { ...noWait };

    /**
     * When the clock's time comes in this tab, on performance.now()'s clock, read from the clock's
     * time once as it is set or taken: a system clock moved back during the wait never holds her
     * choice past it, and no time posted holds it longer than the clock's longest wait.
     */
    let clockDue = 0;

    /**
     * The tab's visit: a stay in the back-forward cache ends one, and every timer, wait and answer
     * of the last one does nothing, since its records were forgotten.
     */
    let visit = 0;
    let isAway = false;

    /** The save out, aborted when the tab leaves; and the lock requests and pause waits still waiting. */
    let callOut: AbortController | null = null;
    const lockWaits = new Set<AbortController>();

    /** How many wakes this tab has had, so a try out at one knows to ask again at once if it fails. */
    let wakes = 0;

    let lastInputPostAt = -Infinity;
    let inputToPost = 0;
    let inputTimer: ReturnType<typeof setTimeout> | null = null;

    let channel: StatusChannel | null = null;
    try {
        // the version of its posts is in its name, so a release that changes them talks on another
        channel = reach?.channel?.(`rock-chat:me:v1:${names}`) ?? null;
    }
    catch {
        // a browser that refuses the channel leaves this tab alone, as one without it
        channel = null;
    }

    function post(message: StatusPost): void {
        if (!channel || isStopped()) {
            return;
        }
        try {
            channel.postMessage(message);
        }
        catch {
            // Intentionally ignored: a channel the browser has closed reaches no tab.
        }
    }

    function isPending(): boolean {
        return tab.newest !== null && tab.newest.at > tab.covered;
    }

    function isNewer(record: StatusRecord, than: StatusRecord): boolean {
        return record.at > than.at || (record.at === than.at && record.page > than.page);
    }

    /** Takes a record this tab made or heard, if it is the newest, and queues its save while nothing covers it. */
    function hold(record: StatusRecord, isMine: boolean): void {
        // without Web Locks no other tab would send a record this one shows, so it shows only its own
        if ((!isMine && !locks) || (tab.newest && !isNewer(record, tab.newest))) {
            return;
        }
        tab.newest = record;
        if (record.at <= tab.covered) {
            return;
        }

        pauseEnds = performance.now() + statusPauseMs;
        const ofVisit = visit;
        void rule.save<null>("status", {
            was: null,
            value: null,
            // what her faces show is the record's, never the rule's
            show: () => undefined,
            settleMs: isMine ? statusPauseMs : statusPauseMs + statusHandOffMs,
            exclusive: ask => exclusively(ofVisit, ask),
            ask: () => send(ofVisit),
            // the refusal is said by the record's own tab, from its post
            refused: () => undefined
        });
    }

    /** Whether nothing is left to send for a visit: stopped, left, or covered. */
    function isDone(ofVisit: number): boolean {
        return isStopped() || ofVisit !== visit || !isPending();
    }

    /** Runs the holder under her lock, so one tab sends at a time; bare without Web Locks, one flight a page. */
    async function exclusively(ofVisit: number, ask: () => Promise<SaveOutcome>): Promise<SaveOutcome> {
        if (!locks) {
            return ask();
        }
        if (isDone(ofVisit)) {
            return nothingSent;
        }

        const waiting = new AbortController();
        lockWaits.add(waiting);
        let isGranted = false;
        try {
            return await locks.request(lockName, { signal: waiting.signal }, () => {
                isGranted = true;
                return ask();
            }) as SaveOutcome;
        }
        catch (error) {
            // the holder's own failure is the rule's to read
            if (isGranted) {
                throw error;
            }
            // given up as the tab left; a lock the browser refuses the page leaves it alone, as without Web Locks
            return waiting.signal.aborted ? nothingSent : ask();
        }
        finally {
            lockWaits.delete(waiting);
        }
    }

    /** The lock's holder: sends the newest record once its pause has passed, unless a saved post covers it. */
    async function send(ofVisit: number): Promise<SaveOutcome> {
        const isGone = (): boolean => isStopped() || ofVisit !== visit;
        if (!reach || isGone()) {
            return nothingSent;
        }
        // No token yet reaches nothing, as a request with no network does: the copy waits here,
        // counting no failure, and the session's first ok exchange asks it at once. A token of
        // another person sends nothing of hers, and the page reloads for them.
        const holder = reach.currentPerson();
        if (holder === null) {
            return { isSaved: false, failure: saveFailed, retryInMs: statusRetryFirstMs };
        }
        if (holder !== person) {
            return nothingSent;
        }

        // Read the newest again after each wait, so a record heard during it waits its own pause.
        // A tab leaving ends the wait at once: a page in the back-forward cache runs no timer, so
        // a wait left to its timer would hold the lock from her other tabs until it came back.
        for (let wait = pauseEnds - performance.now(); wait > 0; wait = pauseEnds - performance.now()) {
            const leaving = new AbortController();
            lockWaits.add(leaving);
            await new Promise<void>(resolve => {
                const timer = setTimeout(resolve, wait);
                leaving.signal.addEventListener("abort", () => {
                    clearTimeout(timer);
                    resolve();
                });
            });
            lockWaits.delete(leaving);
            if (isGone()) {
                return nothingSent;
            }
        }
        const record = tab.newest;
        if (!record || record.at <= tab.covered) {
            return nothingSent;
        }

        // Granted before the clock's time, the lock is let go with nothing sent, no failure
        // counted and nothing posted: the tab that failed asks at it, and this one a hand-off
        // later, so it sends only if that tab has closed.
        const dueIn = clockDue + (clock.retryPage === tabId ? 0 : statusHandOffMs) - performance.now();
        if (clock.retryAt > 0 && dueIn > 0) {
            return { isSaved: false, failure: saveFailed, retryInMs: dueIn };
        }

        // The deadline settles the race whatever the call does, so the lock is never held by a
        // call that hangs, and aborts the call by its signal.
        const call = new AbortController();
        callOut = call;
        const wakesAtSend = wakes;
        let deadline: ReturnType<typeof setTimeout> | undefined;
        let result: CallResult;
        try {
            result = await Promise.race([
                reach.saveStatus(statusChange(record.choice), call.signal),
                new Promise<CallResult>(resolve => {
                    deadline = setTimeout(() => {
                        call.abort();
                        resolve(unreached);
                    }, statusDeadlineMs);
                })
            ]);
        }
        catch {
            result = unreached;
        }
        finally {
            clearTimeout(deadline);
            if (callOut === call) {
                callOut = null;
            }
        }
        // a tab that left forgot the record, so an abort it made asks nothing again
        if (isGone()) {
            return nothingSent;
        }

        const outcome = statusSaveOutcome(result, clock.failures + 1);
        if (outcome.isSaved) {
            // the value sent is what the server now holds, whatever shape its answer took
            settleEverywhere({ kind: "saved", choiceAt: record.at, me: record.choice });
            return { ...outcome, sent: record };
        }
        if (outcome.retryInMs !== undefined || result.ok) {
            // a try that reached nothing left before the network or the platform came back, so it says nothing of now
            const retryInMs = wakes !== wakesAtSend && outcome.isUnreached === true ? 0 : outcome.retryInMs ?? 0;
            clock.failures++;
            clock.retryAt = Date.now() + retryInMs;
            clockDue = performance.now() + retryInMs;
            clock.retryPage = tabId;
            // told before the lock is let go, so a copy that asks it next waits the same clock
            post({ kind: "failed", choiceAt: record.at, retryAt: clock.retryAt, failures: clock.failures, page: tabId });
            return { ...outcome, retryInMs };
        }
        settleEverywhere({ kind: "saved", choiceAt: record.at, page: record.page, refused: true, error: result.error });
        return outcome;
    }

    /** Posts how a save went and settles it here, before the lock is let go. */
    function settleEverywhere(saved: SavedPost): void {
        post(saved);
        settle(saved);
    }

    /**
     * A save went, here or in another tab. Its choice and every older one are covered. A landing
     * writes the value sent into me; a refusal changes nothing the server holds, so it moves only
     * what is covered, and the record's own tab says why, unless a newer choice of its own is
     * pending, whose answer will speak.
     */
    function settle(saved: SavedPost): void {
        Object.assign(clock, noWait);
        const wasCovered = tab.covered;
        tab.covered = Math.max(tab.covered, saved.choiceAt);
        if (!("refused" in saved)) {
            if (saved.choiceAt >= meStamp) {
                state.me.busy_until = saved.me.busy_until;
                state.me.appear_offline = saved.me.appear_offline;
                meStamp = saved.choiceAt;
                tab.isMeKnown = true;
            }
            return;
        }

        // a refusal of a choice already settled here, sent again by a tab the post had not reached, was said once
        if (saved.page !== tabId || saved.choiceAt <= wasCovered) {
            return;
        }
        // read only is the church's state, not the choice's, so this tab takes it as its own refused call would
        if (saved.error.code === "rpc.read_only") {
            reach?.applyRefusal(saved.error);
        }
        if (isPending() && tab.newest?.page === tabId) {
            return;
        }
        const words = failureWords(saved.error);
        dependencies.notify({ code: "status", text: words.alone, icon: "alert" });
        announce(`Not saved. ${words.alone}`, true);
    }

    /**
     * A tab of hers failed, or a hello's answer tells the clock: the later time is the browser's,
     * with the tab that set it, and the larger count. Nothing without a later wait in it.
     */
    function takeClock(told: RetryClock): void {
        if (told.retryAt <= clock.retryAt) {
            return;
        }
        clock.retryAt = told.retryAt;
        clockDue = performance.now() + Math.min(statusLongestWaitMs, Math.max(0, told.retryAt - Date.now()));
        clock.retryPage = told.retryPage;
        clock.failures = Math.max(clock.failures, told.failures);
    }

    /** A post from another tab of hers. */
    function heard(data: unknown): void {
        // a page in the back-forward cache is no channel's destination
        const message = readPost(data);
        if (isStopped() || isAway || !message) {
            return;
        }
        if (message.kind === "choice") {
            hold(message.record, false);
        }
        else if (message.kind === "saved") {
            settle(message);
        }
        else if (message.kind === "failed") {
            // a failure of a choice already saved here starts no wait
            if (message.choiceAt > tab.covered) {
                takeClock({ retryAt: message.retryAt, failures: message.failures, retryPage: message.page });
            }
        }
        else if (message.kind === "hello") {
            post({ kind: "held", covered: tab.covered, me: tab.isMeKnown ? { busy_until: state.me.busy_until, appear_offline: state.me.appear_offline } : null, meStamp, ...clock });
            if (isPending() && tab.newest) {
                post({ kind: "choice", record: tab.newest });
            }
        }
        else if (message.kind === "held") {
            if (!locks) {
                return;
            }
            tab.covered = Math.max(tab.covered, message.covered);
            takeClock(message);
            if (message.me && (!tab.isMeKnown || message.meStamp > meStamp)) {
                state.me.busy_until = message.me.busy_until;
                state.me.appear_offline = message.me.appear_offline;
                meStamp = message.meStamp;
                tab.isMeKnown = true;
            }
        }
        else if (message.kind === "input" && message.at > state.ownInputAt) {
            state.ownInputAt = message.at;
        }
    }

    /**
     * The tab leaves for the back-forward cache, as for a close: it forgets the records not yet
     * saved, aborts its save out and its lock requests, and what its visit queued does nothing. A
     * record it kept could come back over a newer one it never heard.
     */
    function onPageHide(event: unknown): void {
        if ((event as { persisted?: unknown } | null)?.persisted !== true || isStopped()) {
            return;
        }
        visit++;
        isAway = true;
        if (isPending()) {
            tab.newest = null;
        }
        // its queued save and its wait go with the records, and the clock it holds on its return
        // is what its live tabs tell it
        rule.drop("status");
        Object.assign(clock, noWait);
        callOut?.abort();
        lockWaits.forEach(waiting => waiting.abort());
    }

    /** The tab is back from the back-forward cache: it holds again only what its live tabs tell it. */
    function onPageShow(event: unknown): void {
        if ((event as { persisted?: unknown } | null)?.persisted !== true || isStopped()) {
            return;
        }
        visit++;
        isAway = false;
        if (locks) {
            post({ kind: "hello" });
        }
    }

    /**
     * The network or the platform is back: the clock's time is now, its count kept, so the tab
     * that failed last asks at once and every other a hand-off later; and the rule's wait ends.
     */
    function wake(): void {
        if (isStopped()) {
            return;
        }
        wakes++;
        clock.retryAt = Math.min(clock.retryAt, Date.now());
        clockDue = Math.min(clockDue, performance.now());
        rule.wake("status");
    }

    function postInput(): void {
        inputTimer = null;
        if (isStopped()) {
            return;
        }
        lastInputPostAt = Date.now();
        post({ kind: "input", at: inputToPost });
    }

    if (channel) {
        channel.onmessage = event => heard(event.data);
        if (locks) {
            post({ kind: "hello" });
        }
    }
    reach?.targets?.addEventListener("pagehide", onPageHide);
    reach?.targets?.addEventListener("pageshow", onPageShow);
    reach?.targets?.addEventListener("online", wake);

    return {
        setStatus: async (choice: StatusChoice): Promise<void> => {
            if (isStopped() || !reach) {
                return;
            }
            // above every record held, so a clock moved back still orders this one newest
            const floor = Math.max(tab.covered, tab.newest?.at ?? 0);
            const record: StatusRecord = {
                choice: { busy_until: choice.busy_until, appear_offline: choice.appear_offline },
                at: Math.max(Date.now(), floor + 1),
                page: tabId
            };
            hold(record, true);
            post({ kind: "choice", record });
        },

        shownStatus: (): StatusChoice => isPending() && tab.newest
            ? { ...tab.newest.choice }
            : { busy_until: state.me.busy_until, appear_offline: state.me.appear_offline },

        isChoicePending: isPending,

        noteInput: (at: number): void => {
            if (isStopped() || !channel) {
                return;
            }
            inputToPost = Math.max(inputToPost, at);
            if (inputTimer !== null) {
                return;
            }
            const wait = lastInputPostAt + inputPostGapMs - Date.now();
            if (wait <= 0) {
                postInput();
            }
            else {
                inputTimer = setTimeout(postInput, wait);
            }
        },

        statusWake: wake,

        sidebarRead: (): void => {
            meStamp = tab.covered;
            tab.isMeKnown = true;
        },

        stop: (): void => {
            if (inputTimer !== null) {
                clearTimeout(inputTimer);
                inputTimer = null;
            }
            callOut?.abort();
            lockWaits.forEach(waiting => waiting.abort());
            reach?.targets?.removeEventListener("pagehide", onPageHide);
            reach?.targets?.removeEventListener("pageshow", onPageShow);
            reach?.targets?.removeEventListener("online", wake);
            if (channel) {
                channel.onmessage = null;
                try {
                    channel.close();
                }
                catch {
                    // Intentionally ignored: a channel already closed needs nothing more.
                }
                channel = null;
            }
        }
    };
}
