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
// The person's settings: each room's notify mode, star and hide, and their own three switches.
//
// Every change shows at once and writes at once, one save of a setting out at a time: a change
// made while one is out waits for its answer, and only the newest waiting is then asked. A room's
// settings and the reaction switch go to chat_set_prefs, the reaction switch with no room; the two
// privacy switches go to Rock's SavePersonSetting door, one setting a call, since Rock owns them
// and pushes them on. Star and hide pull against each other, so hiding unstars and starring
// unhides in the same patch. The control is the feedback: a save that lands shows nothing more, so
// a change is seamless. A save that fails puts the control back to the last value known saved and
// puts the server's sentence under it, with Try again where another try can pass, said aloud at
// once. Only the newest save of a setting says how it went, so quick changes never flicker through
// older answers. Her status is the one setting whose failed save is asked again rather than put
// back, unless the server answered about the choice itself, and the one her other tabs of chat in
// this browser share: a choice shows at once in every one of them and is saved by whichever still
// lives, one at a time under a Web Lock.
// A sidebar load while a room's save is out leaves that setting as the person chose it, as the
// person's own switches are left; its own answer settles it. Another device sees a change at its
// next load. Once stopped, nothing more is asked and no answer
// reaches the screen.
/* eslint-disable @typescript-eslint/naming-convention */
import { newGuid } from "@Obsidian/Utility/guid";
import { reactive } from "vue";
import { failureWords, finalWords, isOfflineFailure, offlineWords } from "../errors.partial";
import { CallResult } from "../platformCall.partial";
import { ChannelStore, RowPrefs } from "../stores/channelStore.partial";
import { ChatError, FailureWords, SidebarRow } from "../types.partial";
import { Note } from "../ui/settingNote.partial.obs";
import type { PageEventTargets } from "./useMarkRead.partial";
import { ActionNotice } from "./useMessageActions.partial";
import { createSaveRule, failureNote, SaveChange, saveFailed, SaveOutcome, SaveRule } from "./saveRule.partial";

/** What chat_set_prefs answered, through the shell's one call path. */
export type PrefsCallResult = { ok: true, data: unknown } | { ok: false, error: ChatError };

/** One of the two switches Rock owns, as its SavePersonSetting door names them. */
export type PersonSettingKey = "profile_details" | "open_dm";

/** What Rock's SavePersonSetting door answered, as ChatPersonSettingResultBag reaches the browser. */
export type PersonSettingResult = { code: string, isPending: boolean, message: string | null };

/** The person's own switches, as the bootstrap's me and chat_set_prefs's me carry them. */
export type MeSettings = {
    show_profile_details: boolean;
    is_open_dm_allowed: boolean;
    reaction_push: boolean;

    /** When her Busy ends, a server time; "infinity" for a Busy with no end; null when she is not Busy. */
    busy_until: string | null;

    /** Whether she chose Appear offline, on any of her aliases. */
    appear_offline: boolean;
};

/** Her status as she chose it, in the two fields her settings carry it in: Automatic, Busy to an end or "infinity", or Appear offline. */
export type StatusChoice = Pick<MeSettings, "busy_until" | "appear_offline">;

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

/** A room's notify mode, as the platform names them. */
export type NotifyMode = "all" | "mentions" | "silent";

/** A room's place in its details: the note under each of its controls. */
export type RoomPrefs = {
    notes: Partial<Record<"notify" | "star" | "hide", Note>>;
};

/** One of the person's own switches, by the key the dialog names it. */
export type MeKey = PersonSettingKey | "reaction_push";

/** What the settings hold for the screen. */
export type PrefsState = {
    me: MeSettings;

    /** Whether a me has been read: until then me holds this page's defaults, not what she chose. */
    isMeRead: boolean;

    meNotes: Partial<Record<MeKey, Note>>;

    /** Her newest input in her other tabs of chat in this browser, as they post it; 0 before any. */
    ownInputAt: number;

    /** Each room's place, by its id; a room not here has saved nothing this visit. */
    rooms: Record<string, RoomPrefs>;

    /** A sentence for screen readers; the count moves on with each, so the same words twice are read twice. */
    announcement: { text: string, isAssertive: boolean, count: number } | null;
};

/**
 * A choice from a room's controls: its details, or the header's menus, which a missing
 * isFromMenu means. Star with no value turns the room's star over.
 */
export type RoomChoice =
    | { kind: "notify", mode: NotifyMode, isFromMenu?: boolean }
    | { kind: "star", isStarred?: boolean, isFromMenu?: boolean }
    | { kind: "hide", isHidden: boolean };

/** What the settings reach outside themselves. */
export type PrefsDependencies = {
    /** chat_set_prefs through the shell's call path: a room, or null for the reaction switch, and the patch. */
    setPrefs: (channelId: string | null, changes: Record<string, boolean | string | null>) => Promise<PrefsCallResult>;

    /** Rock's SavePersonSetting door; without it, a privacy switch is refused. */
    savePersonSetting?: (setting: PersonSettingKey, value: boolean) => Promise<PersonSettingResult>;

    /** The sidebar, whose rows show a room's settings. */
    channels: Pick<ChannelStore, "rows" | "applyPrefs" | "setSidebar">;

    /** Whether the connection is lost, when the door would not be reached. */
    isOffline: () => boolean;

    /** Shows a toast for a change made from the header: what a notify mode means, or why a change failed. */
    notify: (notice: ActionNotice) => void;

    /** Her status's own reach; without it a status choice is not saved. */
    status?: StatusDependencies;
};

/** The settings a shell holds. */
export type Prefs = {
    state: PrefsState;

    /** The bootstrap's me, read at every sidebar load. */
    setMe: (me: Partial<MeSettings> | null | undefined) => void;

    /** The bootstrap's rooms, read at every sidebar load, into the sidebar. */
    setRooms: (rows: SidebarRow[]) => void;

    /** A room's place, idle when it has saved nothing. */
    roomOf: (channelId: string) => RoomPrefs;

    /** Writes a room's notify mode; from the header's menu it is told in a toast. */
    setNotifyMode: (channelId: string, mode: NotifyMode, isFromMenu: boolean) => Promise<void>;

    /** Stars or unstars a room, unhiding it as it is starred; the star itself shows it, and only a refusal is toasted from the header. */
    setStarred: (channelId: string, isStarred: boolean, isFromMenu: boolean) => Promise<void>;

    /** Hides or shows a room, unstarring it as it is hidden. */
    setHidden: (channelId: string, isHidden: boolean) => Promise<void>;

    /** Writes one of the two switches Rock owns, through its door. */
    setPersonSetting: (setting: PersonSettingKey, value: boolean) => Promise<void>;

    /** Writes whether reactions to the person's messages notify them. */
    setReactionPush: (value: boolean) => Promise<void>;

    /** Saves a choice for a room; with no room, nothing. */
    choose: (channelId: string | null, choice: RoomChoice) => Promise<void>;

    /** A Chat settings switch: the reaction switch is the platform's, the other two Rock's. */
    setSwitch: (key: MeKey, value: boolean) => Promise<void>;

    /**
     * Chooses her status: shown at once in every tab of hers, and saved by whichever tab still
     * lives, so it answers at once; a refusal is said in a toast.
     */
    setStatus: (choice: StatusChoice) => Promise<void>;

    /** Her status as her faces, her typing and her menu read it: her newest choice while it is not yet saved, else the last saved. */
    shownStatus: () => StatusChoice;

    /** Whether a choice of hers is shown that is not yet saved, which is hers whether or not her settings have been read. */
    isChoicePending: () => boolean;

    /** Her input on this page at a time, told to her other tabs at most every 30 seconds. */
    noteInput: (at: number) => void;

    stop: () => void;
};

/** Each mode's name and the toast a change from the header's menu shows. */
const notifyWords: Record<NotifyMode, { title: string, toast: string, icon: string }> = {
    all: { title: "All messages", toast: "You'll be notified about every message", icon: "bell" },
    mentions: { title: "Mentions only", toast: "You'll be notified only when mentioned", icon: "bell-at" },
    silent: { title: "Off", toast: "Muted. A mention by name still reaches you.", icon: "bell-off" }
};

/**
 * The glyph that draws a notify mode wherever the mode is shown: the header's bell, the details'
 * choices, a sidebar row's mark and the toast a change shows.
 *
 * @param mode The notify mode.
 *
 * @returns The name of the chat icon.
 */
export function notifyIconFor(mode: NotifyMode): string {
    return notifyWords[mode].icon;
}

/**
 * One notify mode as the header's menu draws it, by its title and description, and the details'
 * segmented control, by its short label and the one line said under it while it is chosen.
 */
export type NotifyOption = { value: NotifyMode, title: string, description: string, label: string, caption: string, icon: string };

/**
 * The three notify modes in the one wording the details and the header's menu share, so the two
 * never disagree: a name breaks Off and @all does not, as the push filter decides. A channel is a
 * channel; a direct message or group is a conversation.
 *
 * @param kind The room's kind.
 *
 * @returns The modes, in order.
 */
export function notifyOptionsFor(kind: string | null): NotifyOption[] {
    const room = kind === "channel" ? "channel" : "conversation";
    return [
        { value: "all", title: notifyWords.all.title, description: `Every new message in this ${room}.`, label: "All", caption: "Every new message.", icon: notifyWords.all.icon },
        { value: "mentions", title: notifyWords.mentions.title, description: "When someone mentions you by name or uses @all.", label: "Mentions", caption: "Your name or @all.", icon: notifyWords.mentions.icon },
        { value: "silent", title: notifyWords.silent.title, description: "Nothing, unless someone mentions you by name.", label: "Off", caption: "Only mentions of your name.", icon: notifyWords.silent.icon }
    ];
}

/** Each switch's label as Chat settings shows it. */
export const meTitles: Record<MeKey, string> = {
    profile_details: "Show public profile details",
    open_dm: "Anyone can message me",
    reaction_push: "Reactions to my messages"
};

/** The field of me each switch writes. */
const meFields: Record<MeKey, "show_profile_details" | "is_open_dm_allowed" | "reaction_push"> = {
    profile_details: "show_profile_details",
    open_dm: "is_open_dm_allowed",
    reaction_push: "reaction_push"
};

const noDoorSentence = "That setting can't be changed right now.";
const leavesListSentence = "It leaves your list when you open another conversation.";

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
 * How a status save's answer leaves her choice. Only the server's answer about the choice itself
 * refuses it: one of our codes for a 4xx, or read only, which the writer check refuses while her
 * pages still beat, so a choice asked again would show on her own faces and never on anyone
 * else's. Everything that is about the path rather than the choice is asked again on a backoff
 * spread by a random draw, the choice kept: no network or the save's deadline (status 0), a token
 * the refresh could not renew (401), a proxy giving up (408), and the platform failing or down
 * for maintenance (5xx). A 429 is a pace whoever set it, a gateway's carrying no code of ours, so
 * it waits the longer of the backoff and its Retry-After. Any other answer is refused, since
 * asking the same again would meet the same answer.
 *
 * @param result What the shell's call path answered.
 * @param failures How many tries of this choice have failed, this one included, from one.
 * @param random A number from 0 up to 1 that spreads the wait; the browser's by default.
 *
 * @returns The outcome the save rule acts on.
 */
export function statusSaveOutcome(result: CallResult, failures: number, random: () => number = Math.random): SaveOutcome {
    if (result.ok) {
        return { isSaved: true, data: result.data };
    }

    // read only's words are the banner's: the church's, or the server's own sentence, or ours
    const failure = failureWords(result.error);
    if (result.error.code === "rpc.read_only") {
        return { isSaved: false, failure };
    }

    const backoffMs = Math.round(Math.min(statusRetryCapMs, statusRetryFirstMs * 2 ** (Math.max(1, failures) - 1)) * (0.8 + 0.4 * random()));
    if (result.status === 429) {
        return { isSaved: false, failure, retryInMs: Math.max(backoffMs, (result.retryAfterSeconds ?? 0) * 1000) };
    }

    if (result.status === 0 || result.status === 401 || result.status === 408 || result.status >= 500) {
        return { isSaved: false, failure, retryInMs: backoffMs };
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

/** What her tabs say to each other on her channel. */
type StatusPost =
    | { kind: "choice", record: StatusRecord }
    | SavedPost
    | { kind: "hello" }
    | { kind: "held", covered: number, me: StatusChoice | null, meStamp: number }
    | { kind: "input", at: number };

/** Her status's half of the settings. */
type StatusTabs = Pick<Prefs, "setStatus" | "shownStatus" | "isChoicePending" | "noteInput" | "stop"> & {
    /** A sidebar load has written her status into me. */
    sidebarRead: () => void;
};

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
function createStatusTabs(state: PrefsState, rule: SaveRule, dependencies: PrefsDependencies, isStopped: () => boolean, announce: (text: string, isAssertive: boolean) => void): StatusTabs {
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

    /** How many tries of her choice have failed in a row here; any saved post ends the run. */
    let failures = 0;

    /**
     * The tab's visit: a stay in the back-forward cache ends one, and every timer, wait and answer
     * of the last one does nothing, since its records were forgotten.
     */
    let visit = 0;
    let isAway = false;

    /** The save out, aborted when the tab leaves; and the lock requests still waiting. */
    let callOut: AbortController | null = null;
    const lockWaits = new Set<AbortController>();

    let lastInputPostAt = -Infinity;
    let inputToPost = 0;
    let inputTimer: ReturnType<typeof setTimeout> | null = null;

    let channel: StatusChannel | null = null;
    try {
        channel = reach?.channel?.(`rock-chat:me:${names}`) ?? null;
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
        try {
            return await locks.request(lockName, { signal: waiting.signal }, ask) as SaveOutcome;
        }
        catch {
            // the request was given up as the tab left
            return nothingSent;
        }
        finally {
            lockWaits.delete(waiting);
        }
    }

    /** The lock's holder: sends the newest record once its pause has passed, unless a saved post covers it. */
    async function send(ofVisit: number): Promise<SaveOutcome> {
        const isGone = (): boolean => isStopped() || ofVisit !== visit;
        if (!reach || isGone() || reach.currentPerson() !== person) {
            return nothingSent;
        }

        // Read the newest again after each wait, so a record heard during it waits its own pause.
        for (let wait = pauseEnds - performance.now(); wait > 0; wait = pauseEnds - performance.now()) {
            await new Promise(resolve => setTimeout(resolve, wait));
            if (isGone()) {
                return nothingSent;
            }
        }
        const record = tab.newest;
        if (!record || record.at <= tab.covered) {
            return nothingSent;
        }

        // The deadline settles the race whatever the call does, so the lock is never held by a
        // call that hangs, and aborts the call by its signal.
        const call = new AbortController();
        callOut = call;
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

        const outcome = statusSaveOutcome(result, failures + 1);
        if (outcome.isSaved) {
            // the value sent is what the server now holds, whatever shape its answer took
            settleEverywhere({ kind: "saved", choiceAt: record.at, me: record.choice });
            return { ...outcome, sent: record };
        }
        if (outcome.retryInMs !== undefined || result.ok) {
            failures++;
            return outcome;
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
        failures = 0;
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

    /** A post from another tab of hers. */
    function heard(data: unknown): void {
        // a page in the back-forward cache is no channel's destination
        if (isStopped() || isAway || typeof data !== "object" || data === null) {
            return;
        }
        const message = data as StatusPost;
        if (message.kind === "choice") {
            hold(message.record, false);
        }
        else if (message.kind === "saved") {
            settle(message);
        }
        else if (message.kind === "hello") {
            post({ kind: "held", covered: tab.covered, me: tab.isMeKnown ? { busy_until: state.me.busy_until, appear_offline: state.me.appear_offline } : null, meStamp });
            if (isPending() && tab.newest) {
                post({ kind: "choice", record: tab.newest });
            }
        }
        else if (message.kind === "held") {
            if (!locks) {
                return;
            }
            tab.covered = Math.max(tab.covered, message.covered);
            if (message.me && (!tab.isMeKnown || message.meStamp > meStamp)) {
                state.me.busy_until = message.me.busy_until;
                state.me.appear_offline = message.me.appear_offline;
                meStamp = message.meStamp;
                tab.isMeKnown = true;
            }
        }
        else if (message.kind === "input" && typeof message.at === "number" && message.at > state.ownInputAt) {
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

/**
 * Creates the settings.
 *
 * @param dependencies What the settings reach outside themselves.
 *
 * @returns The settings.
 */
export function createPrefs(dependencies: PrefsDependencies): Prefs {
    const state = reactive<PrefsState>({
        me: { show_profile_details: false, is_open_dm_allowed: false, reaction_push: true, busy_until: null, appear_offline: false },
        isMeRead: false,
        meNotes: {},
        ownInputAt: 0,
        rooms: {},
        announcement: null
    }) as PrefsState;

    let isStopped = false;
    let announcementCount = 0;
    const rule = createSaveRule(() => isStopped);

    function announce(text: string, isAssertive = false): void {
        state.announcement = { text, isAssertive, count: ++announcementCount };
    }

    const status = createStatusTabs(state, rule, dependencies, () => isStopped, announce);

    function room(channelId: string): RoomPrefs {
        return state.rooms[channelId] ??= { notes: {} };
    }

    /** The room's sidebar row, whose star is always known; a save's patch reads it whole. */
    function rowOf(channelId: string): SidebarRow | undefined {
        return dependencies.channels.rows.find(row => row.channel_id === channelId);
    }

    /** A refusal in the server's own words; a lost connection says so. */
    function refusalWords(error: ChatError): FailureWords {
        return failureWords(error, dependencies.isOffline());
    }

    /** A refusal's note under its control, with Try again asking the same change once more where that can pass. */
    function refusalNote(failure: FailureWords, retry: () => Promise<void>): Note {
        return failureNote(failure, () => void retry());
    }

    /** Saves a setting by the one save rule; a refusal is said at once, wherever it is shown. */
    async function save<T>(key: string, change: SaveChange<T>): Promise<void> {
        await rule.save(key, {
            ...change,
            refused: failure => {
                change.refused(failure);
                announce(`Not saved. ${failure.alone}`, true);
            }
        });
    }

    /** A room's patch through chat_set_prefs, as the save rule asks it. */
    async function askRoom(channelId: string, patch: Record<string, boolean | string | null>): Promise<SaveOutcome> {
        const result = await dependencies.setPrefs(channelId, patch);
        return result.ok ? { isSaved: true, data: result.data } : { isSaved: false, failure: refusalWords(result.error) };
    }

    /** One of the person's own switches through the save rule. */
    function saveMe(key: MeKey, value: boolean, ask: () => Promise<SaveOutcome>): Promise<void> {
        const field = meFields[key];
        delete state.meNotes[key];
        return save(`me:${key}`, {
            was: state.me[field],
            value,
            show: shown => {
                state.me[field] = shown;
            },
            ask,
            refused: failure => {
                state.meNotes[key] = refusalNote(failure, () => prefs.setSwitch(key, value));
            }
        });
    }

    const prefs: Prefs = {
        state,

        setMe: (me: Partial<MeSettings> | null | undefined): void => {
            if (!me) {
                return;
            }

            const read: MeSettings = {
                show_profile_details: me.show_profile_details === true,
                is_open_dm_allowed: me.is_open_dm_allowed === true,
                reaction_push: me.reaction_push !== false,
                // a Busy with no end arrives as infinity, which her face reads as one
                busy_until: typeof me.busy_until === "string" ? me.busy_until : null,
                appear_offline: me.appear_offline === true
            };

            // A switch whose save is out keeps what the person chose; its own answer settles it.
            for (const key of Object.keys(meFields) as MeKey[]) {
                if (!rule.isOut(`me:${key}`)) {
                    state.me[meFields[key]] = read[meFields[key]];
                }
            }
            // Every load states her status as the platform holds it, a choice pending or not, since
            // what her faces show is the pending choice until it settles; a save landing later replaces it.
            state.me.busy_until = read.busy_until;
            state.me.appear_offline = read.appear_offline;
            status.sidebarRead();
            // last, so what reads her choice once it is read never sees the defaults as hers
            state.isMeRead = true;
        },

        setRooms: (rows: SidebarRow[]): void => {
            // What the person chose in each room whose save is out, as the rows show it now.
            const chosen = new Map<string, RowPrefs>();
            for (const row of dependencies.channels.rows) {
                const kept: RowPrefs = {};
                if (rule.isOut(`${row.channel_id}:notify`)) {
                    kept.notify_mode = row.notify_mode;
                }
                if (rule.isOut(`${row.channel_id}:sidebar`)) {
                    kept.is_favorite = row.is_favorite;
                    kept.hidden = row.hidden ?? false;
                }
                if (Object.keys(kept).length > 0) {
                    chosen.set(row.channel_id, kept);
                }
            }

            dependencies.channels.setSidebar(rows);
            chosen.forEach((kept, channelId) => dependencies.channels.applyPrefs(channelId, kept));
        },

        roomOf: (channelId: string): RoomPrefs => state.rooms[channelId] ?? { notes: {} },

        setNotifyMode: async (channelId: string, mode: NotifyMode, isFromMenu: boolean): Promise<void> => {
            const row = rowOf(channelId);
            if (isStopped || !row || row.notify_mode === mode) {
                return;
            }

            delete room(channelId).notes.notify;
            await save<string | null | undefined>(`${channelId}:notify`, {
                was: row.notify_mode,
                value: mode,
                show: shown => dependencies.channels.applyPrefs(channelId, { notify_mode: shown }),
                ask: () => askRoom(channelId, { notify_mode: mode }),
                landed: data => {
                    // the mode the room now has for the person, its default included where it is cleared
                    const effective = (data as { channel?: { notify_mode_effective?: unknown } } | null)?.channel?.notify_mode_effective;
                    if (typeof effective === "string") {
                        dependencies.channels.applyPrefs(channelId, { notify_mode: effective });
                    }

                    // A menu closes as it chooses, so the toast says what the choice means.
                    if (isFromMenu) {
                        dependencies.notify({ code: "notify-mode", text: notifyWords[mode].toast, icon: notifyWords[mode].icon });
                        announce(notifyWords[mode].toast);
                    }
                },
                refused: failure => {
                    if (isFromMenu) {
                        dependencies.notify({ code: "notify-mode", text: failure.alone, icon: "alert" });
                    }
                    else {
                        room(channelId).notes.notify = refusalNote(failure, () => prefs.setNotifyMode(channelId, mode, false));
                    }
                }
            });
        },

        setStarred: async (channelId: string, isStarred: boolean, isFromMenu: boolean): Promise<void> => {
            const row = rowOf(channelId);
            if (isStopped || !row) {
                return;
            }

            // starring a hidden room brings it back, in the same one save
            const isUnhiding = isStarred && row.hidden === true;
            const patch: Record<string, boolean> = isUnhiding ? { is_favorite: true, hidden: false } : { is_favorite: isStarred };
            const notes = room(channelId).notes;
            delete notes.star;
            delete notes.hide;
            if (isUnhiding && !isFromMenu) {
                notes.star = { kind: "info", text: "Starred, so it's back in your list." };
            }

            // Star and hide are one save's two halves, so they share one newest, and each save asks
            // for both: the newest then lands what is shown whatever an older one still out does.
            const was = { is_favorite: row.is_favorite, hidden: row.hidden ?? false };
            const value = { ...was, ...patch };
            await save<RowPrefs>(`${channelId}:sidebar`, {
                was,
                value,
                show: shown => dependencies.channels.applyPrefs(channelId, shown),
                ask: () => askRoom(channelId, value),
                refused: failure => {
                    if (isFromMenu) {
                        dependencies.notify({ code: "starred", text: failure.alone, icon: "alert" });
                    }
                    else {
                        delete notes.hide;
                        notes.star = refusalNote(failure, () => prefs.setStarred(channelId, isStarred, false));
                    }
                }
            });
        },

        setHidden: async (channelId: string, isHidden: boolean): Promise<void> => {
            const row = rowOf(channelId);
            if (isStopped || !row) {
                return;
            }

            // hiding a starred room takes it out of Starred, in the same one save
            const isUnstarring = isHidden && row.is_favorite === true;
            const patch: Record<string, boolean> = isUnstarring ? { hidden: true, is_favorite: false } : { hidden: isHidden };
            const notes = room(channelId).notes;
            delete notes.star;
            delete notes.hide;
            if (isHidden) {
                // what hiding means is said with the change, since the room stays until another opens
                notes.hide = { kind: "info", text: `${isUnstarring ? "Hidden, and taken out of Starred." : "Hidden."} ${leavesListSentence}` };
            }

            const was = { is_favorite: row.is_favorite, hidden: row.hidden ?? false };
            const value = { ...was, ...patch };
            await save<RowPrefs>(`${channelId}:sidebar`, {
                was,
                value,
                show: shown => dependencies.channels.applyPrefs(channelId, shown),
                ask: () => askRoom(channelId, value),
                refused: failure => {
                    delete notes.star;
                    notes.hide = refusalNote(failure, () => prefs.setHidden(channelId, isHidden));
                }
            });
        },

        setPersonSetting: async (setting: PersonSettingKey, value: boolean): Promise<void> => {
            if (isStopped) {
                return;
            }

            const door = dependencies.savePersonSetting;
            await saveMe(setting, value, async (): Promise<SaveOutcome> => {
                if (!door) {
                    return { isSaved: false, failure: finalWords(noDoorSentence) };
                }

                // Pending has saved too: Rock holds it and the platform hears of it shortly.
                const answer = await door(setting, value);
                if (answer.code === "ok") {
                    return { isSaved: true };
                }

                // Rock answering without its door decided nothing; the person may ask again.
                if (answer.code === "door.unavailable") {
                    return { isSaved: false, failure: isOfflineFailure(null, dependencies.isOffline()) ? offlineWords : saveFailed };
                }

                // Rock's door decided: the same value would be refused again.
                return { isSaved: false, failure: finalWords(answer.message?.trim() || noDoorSentence) };
            });
        },

        setReactionPush: async (value: boolean): Promise<void> => {
            if (isStopped) {
                return;
            }

            await saveMe("reaction_push", value, async (): Promise<SaveOutcome> => {
                const result = await dependencies.setPrefs(null, { reaction_push: value });
                return result.ok ? { isSaved: true } : { isSaved: false, failure: refusalWords(result.error) };
            });
        },

        choose: async (channelId: string | null, choice: RoomChoice): Promise<void> => {
            if (!channelId) {
                return;
            }

            const isFromMenu = choice.kind !== "hide" && choice.isFromMenu !== false;
            if (choice.kind === "notify") {
                await prefs.setNotifyMode(channelId, choice.mode, isFromMenu);
            }
            else if (choice.kind === "star") {
                const isStarred = choice.isStarred ?? !dependencies.channels.rows.find(r => r.channel_id === channelId)?.is_favorite;
                await prefs.setStarred(channelId, isStarred, isFromMenu);
            }
            else {
                await prefs.setHidden(channelId, choice.isHidden);
            }
        },

        setSwitch: (key: MeKey, value: boolean): Promise<void> => key === "reaction_push" ? prefs.setReactionPush(value) : prefs.setPersonSetting(key, value),

        setStatus: status.setStatus,
        shownStatus: status.shownStatus,
        isChoicePending: status.isChoicePending,
        noteInput: status.noteInput,

        stop: (): void => {
            isStopped = true;
            rule.stop();
            status.stop();
        }
    };

    return prefs;
}
