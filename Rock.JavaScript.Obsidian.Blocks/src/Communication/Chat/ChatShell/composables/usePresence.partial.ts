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
// Signals that say what someone is doing now rather than change anything held: typing.
//
// The person's own, per place, as one turn from the first key to the send: nothing goes until the
// box has held text for the first signal's wait, so a quick message never shows typing; then a
// signal at most once an interval while the text keeps changing, and one last one an interval after
// the last, if the text changed since, so the others see the typing until a reread is over. A send
// ends the turn, so the next message's typing shows as soon as the first did; clearing an announced
// box sends one stop. Leaving the place ends its turn with nothing sent.
//
// The others', per place and typist, kept whatever room is open until they expire, stop, are
// overtaken by a message of theirs, send, or are blocked; every end goes through one function, which
// records why the last typist in a place went, so the views move the bubble by what happened. A
// typist drawn stays at least a moment, so a word typed and deleted never blinks.
//
// This lives apart from the timelines because what it holds expires on its own clock and is never
// history: a signal lost costs the bubble for a few seconds, and nothing is caught up or repaired.
//
// Others' online status is composed here, created and stopped with typing, and kept in its own file
// beside this one.

import { reactive, shallowReactive, watch } from "vue";
import type { CallResult, PlatformCall } from "../platformCall.partial";
import { isReportedRefusal } from "../errors.partial";
import type { MessageMotion, TypingEnd } from "../messageMotion.partial";
import type { ShellState } from "../shell.partial";
import { ChatError, ChatPerson, MessageCreatedEvent, SidebarRow, TypingEvent, TypingSnapshot } from "../types.partial";
import { personName } from "../ui/face.partial";
import { quoteKey } from "./useMessageActions.partial";
import { createPresenceStatus, PresenceStatus, PresenceStatusDependencies } from "./usePresenceStatus.partial";
import type { Prefs } from "./usePrefs.partial";
import type { ChatSettings } from "./useSession.partial";

/** What typing reaches outside itself. */
export type PresenceDependencies = {
    /** The shell's call to a platform function, which refreshes an expired token once. */
    callFunction: PlatformCall["callFunction"];

    /** The settings, read at every use, since the pace moves with each token. */
    settings: () => ChatSettings;

    /** The token held now, which a refusal as off silences until the next. */
    currentToken: () => string | null;

    state: Pick<ShellState, "activeChannelId" | "connection" | "isComposerOpen">;

    /** A room's sidebar row, for its kind and whether the person may post there. */
    rowOf: (channelId: string) => SidebarRow | null;

    /** The room's people as its facts last named them, for the typists' names. */
    peopleOf: (channelId: string) => readonly ChatPerson[];

    /** The newest message the client holds in a place, which the person's signal names as followed. */
    newestHeld: (channelId: string, rootId: number | null) => number | null;

    /**
     * The newest message the client holds from someone in a place, by their lower-case alias,
     * whether a page or a live copy brought it; a signal naming an older one was sent before it.
     */
    newestFrom: (channelId: string, rootId: number | null, alias: string) => number | null;

    /**
     * Whether what the client holds of a place may be behind what the person has written there: a
     * send of theirs still out, or a thread's first page still on its way.
     */
    isBehind: (channelId: string, rootId: number | null) => boolean;

    /** Whether a place is on screen now: the open conversation, or the thread open in its drawer. */
    isOnScreen: (channelId: string, rootId: number | null) => boolean;

    /** The person's primary alias, which the platform writes as a signal's typist. */
    personAliasGuid: string;

    isBlocked: (personAliasGuid: string) => boolean;

    /** Told where someone starts being drawn typing and why the last typist in a place went, for the views' motion. */
    motion: Pick<MessageMotion, "typingStarted" | "typingEnded">;

    /** Tells the platform of a refusal its one rule for reporting picks out. */
    report: (code: string) => void;

    /** Whether the shell has stopped or its session ended, read at each resumed step. */
    isStopped: () => boolean;

    /** Her own settings, read for her Busy and Appear offline. */
    prefs: { state: { me: Pick<Prefs["state"]["me"], "busy_until" | "appear_offline">, isMeRead: boolean } };
} & Pick<PresenceStatusDependencies, "targets" | "leave" | "session">;

/** Typing, as the shell holds it. */
export type Presence = {
    /** Each place's typists by alias, in the order first heard, so the faces and names do not reorder as they refresh; reactive. */
    state: { typists: Record<string, string[]> };

    /** Whether a live event is the presence's alone. */
    owns: (event: string) => boolean;

    /**
     * A live signal arrived.
     *
     * @param event The event's name.
     * @param payload Its payload.
     * @param topicChannelId The conversation whose topic carried it; left out for the personal topic.
     */
    heard: (event: string, payload: unknown, topicChannelId?: string) => void;

    /** The person changed a box's text in a place: a conversation, or a thread when a root is named. */
    typed: (channelId: string, rootId: number | null, text: string) => void;

    /** The person sent a message in a place, which ends their turn there: the next message is a new one. */
    sent: (channelId: string, rootId: number | null) => void;

    /** A send of the person's landed, so a signal that waited for their history to catch up goes. */
    caughtUp: () => void;

    /** Who else is typing in a place, as its feed's typing bubble draws them, or null for no one. */
    typingIn: (place: string | null) => Typing | null;

    /** A message arrived live, which ends its sender's typing in its place: the last typist's turns into it. */
    messageArrived: (channelId: string, created: Partial<MessageCreatedEvent>) => void;

    /**
     * A holder now holds a message from someone in a place, from a page, a catch-up or a thread's
     * read: their typing that signal named an older message for is over.
     */
    heldThrough: (channelId: string, rootId: number | null, alias: string | null | undefined, id: number) => void;

    /**
     * The platform's answer of who is typing in a place, read with its newest page and asked at
     * askedAt. It is what is, so nothing moves for it: a typist it names is drawn, one the page's
     * own messages overtook is left out, and one held whom it leaves out has gone, unless a signal
     * of hers was heard after the read was asked.
     */
    stated: (channelId: string, rootId: number | null, typists: unknown, askedAt: number) => void;

    /** The people blocked gained someone: their typing goes at once. */
    remask: () => void;

    /** Others' online status. */
    status: PresenceStatus;

    stop: () => void;
};

/** Who is typing in a place: their faces, the name line a group shows, and the sentence a screen reader hears. */
export type Typing = {
    /** Each typist in the order first heard, by lower-case alias, with their person where the room's facts name them. */
    typists: ReadonlyArray<{ alias: string, person: ChatPerson | null }>;
} & TypingWords;

/** What is said of the people typing in a place. */
export type TypingWords = {
    /** Who is typing, as a group's name line says it: "Anna Park", "Anna and Sam", or "Several people". */
    names: string;

    /** The whole sentence: "Anna is typing…". */
    sentence: string;
};

/**
 * One typist drawn in a place: the newest message their signal followed, when they were first
 * drawn, when a signal of theirs was last heard, when their typing expires, and when a stop heard
 * takes them, if one was.
 */
type Typist = {
    after: number;
    shownAt: number;
    heardAt: number;
    expiresAt: number;

    /** When a stop takes her, held to the minimum show: a stop heard, or a page's answer leaving her out. */
    stopAt: number | null;

    /** Whether that stop is a page's answer leaving her out, which a later answer naming her undoes. */
    isLeftOut: boolean;
};

/**
 * The person's turn in one place, from its first key to its send: the box's text, when its last
 * signal went (null while nothing is announced), whether the text changed since, whether a signal
 * waits for the history or a call out, and the one timer, for the first signal or the last.
 */
type Turn = {
    channelId: string;
    rootId: number | null;
    text: string;
    sentAt: number | null;
    isChanged: boolean;
    isDue: boolean;
    timer: ReturnType<typeof setTimeout> | null;
};

/**
 * The words for the people typing in a place.
 *
 * @param people Each typist in the order first heard, or null for one whose name is not held.
 *
 * @returns The words, or null for none.
 */
export function typingWords(people: ReadonlyArray<Pick<ChatPerson, "nick_name" | "last_name"> | null>): TypingWords | null {
    if (people.length === 0) {
        return null;
    }
    if (people.length > 2) {
        return { names: "Several people", sentence: "Several people are typing…" };
    }

    const names = people.map(person => person?.nick_name || person?.last_name || null);

    // Two of one nick name are told apart by their last initial, only then, so the usual names
    // stay as short as they are.
    if (names.length === 2 && names[0] !== null && names[0] === names[1]) {
        people.forEach((person, index) => {
            const initial = person?.nick_name && person.last_name ? ` ${person.last_name.charAt(0)}.` : "";
            names[index] = `${names[index]}${initial}`;
        });
    }

    if (names.length === 1) {
        const one = names[0] ?? "Someone";
        // one typist's name line is her message's, as the bubble is drawn as her next message would be
        return { names: (people[0] && personName(people[0])) || one, sentence: `${one} is typing…` };
    }

    const two = `${names[0] ?? "Someone"} and ${names[1] ?? "someone"}`;
    return { names: two, sentence: `${two} are typing…` };
}

/**
 * Creates the session's typing.
 *
 * @param dependencies What it reaches outside itself.
 *
 * @returns The typing state and its handlers.
 */
export function createPresence(dependencies: PresenceDependencies): Presence {
    const { state: shellState, settings, isStopped, motion } = dependencies;
    const self = dependencies.personAliasGuid.toLowerCase();

    const state = reactive({ typists: {} as Record<string, string[]> });

    const status = createPresenceStatus({ ...dependencies, me: () => dependencies.prefs.state.isMeRead ? dependencies.prefs.state.me : null });

    // #region Send

    /** The person's turn in each place that has one; its places are watched for leaving the screen. */
    const turns = shallowReactive(new Map<string, Turn>());

    /** Whether a signal is out; the next waits for it, so a slow route never stacks calls. */
    let isCallOut = false;

    /** The token typing was refused as off under; silent until the session holds another. */
    let offToken: string | null = null;

    /** Places the platform refused for good this page: a room that is not a conversation, a thread whose root is gone. */
    const refusedPlaces = new Set<string>();

    /** Whether the person may be shown typing from this place at all; the one rule every send condition lives in. */
    function mayType(channelId: string, place: string): boolean {
        const row = dependencies.rowOf(channelId);
        // a channel never carries typing: only a direct message or a group does
        if (row?.room_kind !== "person" && row?.room_kind !== "group") {
            return false;
        }
        // Appear offline hides her typing too, whether or not status is on: the platform drops it anyway
        if (dependencies.prefs.state.me.appear_offline) {
            return false;
        }
        if (!settings().flags.typing || (offToken !== null && offToken === dependencies.currentToken())) {
            return false;
        }
        // nothing could come of a signal while the banner shows or the box cannot send
        if (shellState.connection !== null || !shellState.isComposerOpen || row.can_post === false) {
            return false;
        }
        return !refusedPlaces.has(channelId) && !refusedPlaces.has(place);
    }

    /** Ends a place's turn with nothing sent: its timer goes with it. */
    function forget(place: string): void {
        const turn = turns.get(place);
        if (turn?.timer) {
            clearTimeout(turn.timer);
        }
        turns.delete(place);
    }

    /** Sets a turn's one timer, for its first signal or its last. */
    function arm(place: string, turn: Turn, ms: number): void {
        if (turn.timer) {
            clearTimeout(turn.timer);
        }
        turn.timer = setTimeout(() => {
            turn.timer = null;
            due(place, turn);
        }, Math.max(0, ms));
    }

    /**
     * A turn's signal is due: its first once the wait has passed, a keep-alive, or the last after
     * the person stopped. One that comes due while the person's own send is out, or while a call
     * is, waits for it rather than being dropped.
     */
    function due(place: string, turn: Turn): void {
        if (isStopped() || turns.get(place) !== turn) {
            return;
        }
        if (!mayType(turn.channelId, place)) {
            return;
        }
        // A send still out, or a thread page still coming, may hold a message of the person's
        // that this client's history lacks, so a signal now would name a message older than it
        // and viewers would drop it; it goes as the history catches up.
        if (isCallOut || dependencies.isBehind(turn.channelId, turn.rootId)) {
            turn.isDue = true;
            return;
        }

        turn.isDue = false;
        turn.sentAt = Date.now();
        turn.isChanged = false;
        void signal(turn.channelId, turn.rootId, place, false);
    }

    /** Sends every signal that waited for the history or a call out, now that one may go. */
    function sendDue(): void {
        for (const [place, turn] of turns) {
            if (turn.isDue) {
                due(place, turn);
            }
        }
    }

    function typed(channelId: string, rootId: number | null, text: string): void {
        if (isStopped()) {
            return;
        }
        const place = quoteKey(channelId, rootId);
        const turn = turns.get(place);

        if (text.trim() === "") {
            forget(place);
            // An announced draft cleared says so once, so the others' bubble goes within a moment
            // rather than lingering to its expiry.
            if (turn?.sentAt != null && mayType(channelId, place)) {
                void signal(channelId, rootId, place, true);
            }
            return;
        }
        if (!mayType(channelId, place)) {
            return;
        }

        if (!turn) {
            // A message sent within the wait never shows typing, and costs nothing.
            const composing: Turn = { channelId, rootId, text, sentAt: null, isChanged: false, isDue: false, timer: null };
            turns.set(place, composing);
            arm(place, composing, settings().limits.typing_first_after_ms);
            return;
        }

        turn.text = text;
        turn.isChanged = true;
        if (turn.isDue) {
            due(place, turn);
            return;
        }
        // still within the first wait, or the last signal is already set for its moment
        if (turn.sentAt === null || turn.timer) {
            return;
        }
        const wait = turn.sentAt + settings().limits.typing_interval_ms - Date.now();
        if (wait <= 0) {
            due(place, turn);
        }
        else {
            arm(place, turn, wait);
        }
    }

    /** Sends one signal, a start or a stop, and acts on a refusal; every failure is quiet. */
    async function signal(channelId: string, rootId: number | null, place: string, isStop: boolean): Promise<void> {
        const after = dependencies.newestHeld(channelId, rootId);
        const region = settings().routes.region;
        // the token the call goes with, so a refresh while it is out is never what gets silenced
        const token = dependencies.currentToken();
        // the page whose row her typing marks active; a page with none sends no key, since the route refuses a null
        const client = status.clientId();

        // a stop is one call a turn, so it never waits behind a start
        if (!isStop) {
            isCallOut = true;
        }
        let result: CallResult | null;
        try {
            result = await dependencies.callFunction(
                "chat-typing",
                { p_channel_id: channelId, p_parent_id: rootId, ...(after !== null ? { p_after: after } : {}), ...(isStop ? { p_stopped: true } : {}), ...(client !== null ? { p_client: client } : {}) },
                region ? { headers: { "x-region": region } } : undefined
            );
        }
        catch {
            // a call that rejects rather than answering is dropped like any other failure
            result = null;
        }
        finally {
            // however the call ended the next may go, unless stop has already cleared everything
            if (!isStopped() && !isStop) {
                isCallOut = false;
                sendDue();
            }
        }

        // A pass and a throttled call answer alike, as success with nothing more to do. A stop is
        // the turn's last word, so one refused, as a platform that does not know it refuses it, is
        // simply dropped: the others' bubble goes at its expiry instead.
        if (isStopped() || !result || result.ok || isStop) {
            return;
        }
        refused(result.error, channelId, place, token);
    }

    function refused(error: ChatError, channelId: string, place: string, token: string | null): void {
        if (error.code === "rpc.typing_off") {
            offToken = token;
        }
        else if (error.code === "rpc.typing_not_here") {
            refusedPlaces.add(channelId);
        }
        else if (error.code === "rpc.bad_parent") {
            refusedPlaces.add(place);
        }
        // every other refusal is dropped quietly, and told by the shell's one rule for reporting
        else if (isReportedRefusal(error)) {
            dependencies.report(error.code);
        }
    }

    // Typing switched off ends everyone drawn as cleared, so nothing held comes back if it is
    // switched on again within an expiry; what is heard while it is off draws no one.
    const stopWatchingSwitch = watch(() => settings().flags.typing, isOn => {
        if (!isOn && typists.size > 0) {
            for (const [place, held] of [...typists]) {
                for (const alias of [...held.keys()]) {
                    end(place, alias, "cleared");
                }
            }
            changed();
        }
    }, { flush: "sync" });

    // A place that leaves the screen ends its turn there with nothing sent, whichever way it goes:
    // a room switch, the drawer closing, or the drawer moving to another thread. Watched at once,
    // so a timer set for it never fires for a room the person has left.
    const stopWatchingScreen = watch(
        () => [...turns].filter(([, turn]) => !dependencies.isOnScreen(turn.channelId, turn.rootId)).map(([place]) => place),
        left => left.forEach(forget),
        { flush: "sync" }
    );

    // #endregion Send

    // #region Receive

    /** The typists drawn in each place, by lower-case alias, whatever room is open. */
    const typists = new Map<string, Map<string, Typist>>();

    /** The one timer, set for the soonest typist due to go. */
    let endTimer: ReturnType<typeof setTimeout> | null = null;

    /** What each place last drew, handed out again while its typists and their people are the same. */
    const drawn = new Map<string, Typing>();

    /** Puts what is held on the screen, each place's typists in the order first heard. */
    function publish(): void {
        const next: Record<string, string[]> = {};
        for (const [place, held] of typists) {
            next[place] = [...held.keys()];
        }
        state.typists = next;
    }

    /** What is held changed: it is drawn, and the one timer set for the soonest typist due to go. */
    function changed(): void {
        publish();
        schedule();
    }

    /**
     * When a typist goes by their own clock: at a stop heard, or at their expiry, and never before
     * they have been drawn for the minimum, so a word typed and deleted never blinks.
     */
    function endsAt(typist: Typist): number {
        return typist.stopAt ?? Math.max(typist.expiresAt, typist.shownAt + settings().limits.typing_min_show_ms);
    }

    /** Sets the one timer for the soonest typist due to go, replacing any set before. */
    function schedule(): void {
        if (endTimer !== null) {
            clearTimeout(endTimer);
            endTimer = null;
        }
        let soonest = Infinity;
        for (const held of typists.values()) {
            for (const typist of held.values()) {
                soonest = Math.min(soonest, endsAt(typist));
            }
        }
        if (soonest !== Infinity) {
            endTimer = setTimeout(tick, Math.max(0, soonest - Date.now()));
        }
    }

    /** Ends every typist now due, a stop or an expiry, and sets the one timer for the next. */
    function tick(): void {
        endTimer = null;
        if (isStopped()) {
            return;
        }
        const now = Date.now();
        for (const [place, held] of typists) {
            for (const [alias, typist] of held) {
                // a stop, heard or learned from an answer, goes as a stop does, sliding out
                if (endsAt(typist) <= now) {
                    end(place, alias, typist.stopAt !== null ? "stopped" : "expired");
                }
            }
        }
        changed();
    }

    /**
     * The one way a typist stops being drawn. When the place is left with no one, why is recorded,
     * with her message where it was her send, so the views move the bubble once, by what happened.
     * The caller draws the change.
     */
    function end(place: string, alias: string, cause: TypingEnd, message?: Pick<MessageCreatedEvent, "id" | "client_key">): void {
        const held = typists.get(place);
        if (!held?.delete(alias)) {
            return;
        }
        if (held.size === 0) {
            typists.delete(place);
            drawn.delete(place);
            motion.typingEnded(place, cause, message);
        }
    }

    /**
     * Draws a typist, or holds one drawn on. A signal heard for a place where no one was drawn is
     * typing starting, which the views move for; one the platform states is simply there.
     */
    function draw(place: string, alias: string, after: number, expiresAt: number, isHeard: boolean): void {
        let held = typists.get(place);
        const isFirst = !held;
        if (!held) {
            held = new Map();
            typists.set(place, held);
        }

        const typist = held.get(alias);
        if (typist) {
            typist.after = Math.max(typist.after, after);
            typist.expiresAt = Math.max(typist.expiresAt, expiresAt);
            // still typing after a stop, or named again after an answer left her out: the stop is past
            if (isHeard) {
                typist.heardAt = Date.now();
            }
            if (isHeard || typist.isLeftOut) {
                typist.stopAt = null;
                typist.isLeftOut = false;
            }
            return;
        }
        held.set(alias, { after, shownAt: Date.now(), heardAt: isHeard ? Date.now() : 0, expiresAt, stopAt: null, isLeftOut: false });
        if (isFirst && isHeard) {
            motion.typingStarted(place);
        }
    }

    /**
     * How long a typist is shown typing: the viewer's own expiry, or where the signal names the pace
     * its typist was held to, that pace with the same margin the viewer's expiry has over its own
     * interval, since tokens minted at different moments can name different paces.
     */
    function expiryOf(intervalMs: unknown): number {
        const { typing_interval_ms: ownInterval, typing_expiry_ms: ownExpiry } = settings().limits;
        return typeof intervalMs === "number" && intervalMs > 0 && ownInterval > 0
            ? intervalMs * ownExpiry / ownInterval
            : ownExpiry;
    }

    /** Whether someone's typing could be drawn at all: not the person's own, and not someone they blocked. */
    function isOther(alias: string): boolean {
        return alias !== self && !dependencies.isBlocked(alias);
    }

    function heard(event: string, payload: unknown, topicChannelId?: string): void {
        if (isStopped() || event !== "typing") {
            return;
        }

        const typing = payload as Partial<TypingEvent> | null;
        const channelId = typing?.channel_id;
        const alias = typeof typing?.person_alias_guid === "string" ? typing.person_alias_guid.toLowerCase() : null;
        // a conversation topic carries its own room only, and the person's own echo is not someone else typing
        if (typeof channelId !== "string" || !alias || (topicChannelId !== undefined && channelId !== topicChannelId) || !isOther(alias)) {
            return;
        }

        // Her status rides the row whatever becomes of its dots: a stop, a signal older than her
        // message, or typing switched off still say where she is, decided when the row was. It is
        // written before the dots are drawn, so both show in one pass.
        if (typing !== null && "status" in typing) {
            status.carried(alias, typing.status);
        }
        if (!settings().flags.typing) {
            return;
        }

        const rootId = typeof typing?.parent_id === "number" ? typing.parent_id : null;
        const place = quoteKey(channelId, rootId);
        const typist = typists.get(place)?.get(alias);
        if (typing?.stopped === true) {
            if (typist) {
                typist.stopAt = Math.max(Date.now(), typist.shownAt + settings().limits.typing_min_show_ms);
                typist.isLeftOut = false;
                changed();
            }
            return;
        }

        // A signal that names a message older than one of theirs the history holds was sent before
        // that message, however late it arrives and however the message came; it ended that typing.
        const after = typeof typing?.after === "number" ? typing.after : 0;
        if (after < (dependencies.newestFrom(channelId, rootId, alias) ?? 0)) {
            return;
        }

        draw(place, alias, after, Date.now() + expiryOf(typing?.interval_ms), true);
        changed();
    }

    function messageArrived(channelId: string, created: Partial<MessageCreatedEvent>): void {
        const alias = typeof created.person_alias_guid === "string" ? created.person_alias_guid.toLowerCase() : null;
        if (isStopped() || !alias || typeof created.id !== "number") {
            return;
        }

        const place = quoteKey(channelId, typeof created.parent_id === "number" ? created.parent_id : null);
        // a signal that already follows this message overtook it, and stays
        const typist = typists.get(place)?.get(alias);
        if (typist && typist.after < created.id) {
            end(place, alias, "sent", { id: created.id, client_key: created.client_key ?? null });
            changed();
        }
    }

    function heldThrough(channelId: string, rootId: number | null, alias: string | null | undefined, id: number): void {
        const place = quoteKey(channelId, rootId);
        // nothing is drawn in most places, so a page's every message costs one lookup
        const held = typists.get(place);
        const key = alias?.toLowerCase();
        const typist = held && key ? held.get(key) : undefined;
        if (typist && key && typist.after < id) {
            end(place, key, "overtaken");
            changed();
        }
    }

    function stated(channelId: string, rootId: number | null, answered: unknown, askedAt: number): void {
        // an answer with no list says nothing of typing, as an older platform's or a channel's; while
        // typing is off nothing is held, so switching it on again draws no one from before
        if (isStopped() || !Array.isArray(answered) || !settings().flags.typing) {
            return;
        }
        const place = quoteKey(channelId, rootId);
        const now = Date.now();
        const named = new Set<string>();
        for (const entry of answered as Partial<TypingSnapshot>[]) {
            const alias = typeof entry?.person_alias_guid === "string" ? entry.person_alias_guid.toLowerCase() : null;
            if (!alias || !isOther(alias)) {
                continue;
            }
            // the page and the typists are read together, so one of hers newer than what the signal followed ended it
            const after = typeof entry.after === "number" ? entry.after : 0;
            if (after < (dependencies.newestFrom(channelId, rootId, alias) ?? 0)) {
                continue;
            }
            const age = typeof entry.age_ms === "number" && entry.age_ms > 0 ? entry.age_ms : 0;
            const expiresAt = now + expiryOf(entry.interval_ms) - age;
            if (expiresAt > now) {
                draw(place, alias, after, expiresAt, false);
                named.add(alias);
            }
        }
        // The answer read the typists in the same statement as the page, so one held here that it
        // leaves out stopped, expired or was blocked before the read; a signal heard after the
        // read was asked is newer than the answer, and stays. Past her minimum she ends with the
        // page's render, so nothing moves; under it she is held to it, then goes as a stop does,
        // and a stop already heard is kept as it is.
        for (const [alias, typist] of [...typists.get(place) ?? []]) {
            if (!named.has(alias) && typist.heardAt < askedAt) {
                const at = typist.shownAt + settings().limits.typing_min_show_ms;
                if (at <= now) {
                    end(place, alias, "overtaken");
                }
                else if (typist.stopAt === null) {
                    typist.stopAt = at;
                    typist.isLeftOut = true;
                }
            }
        }
        changed();
    }

    // #endregion Receive

    return {
        state,

        owns: event => event === "typing",

        heard,

        typed,

        sent: (channelId, rootId) => forget(quoteKey(channelId, rootId)),

        caughtUp: () => {
            if (!isStopped()) {
                sendDue();
            }
        },

        typingIn: place => {
            const aliases = place ? state.typists[place] : undefined;
            // switched off, no one is drawn typing, whatever is still held until it expires
            if (!place || !aliases?.length || !settings().flags.typing) {
                return null;
            }
            // a place key starts with its conversation's id
            const people = dependencies.peopleOf(place.split("/")[0]);
            const typing = aliases.map(alias => ({ alias, person: people.find(person => person.person_alias_guid.toLowerCase() === alias) ?? null }));
            const last = drawn.get(place);
            if (last && last.typists.length === typing.length && last.typists.every((typist, index) => typist.alias === typing[index].alias && typist.person === typing[index].person)) {
                return last;
            }

            const words = typingWords(typing.map(typist => typist.person));
            const result = words ? { typists: typing, ...words } : null;
            if (result) {
                drawn.set(place, result);
            }
            return result;
        },

        messageArrived,

        heldThrough,

        stated,

        remask: () => {
            for (const [place, held] of typists) {
                for (const alias of held.keys()) {
                    if (dependencies.isBlocked(alias)) {
                        end(place, alias, "blocked");
                    }
                }
            }
            changed();
        },

        status,

        stop: () => {
            status.stop();
            stopWatchingScreen();
            stopWatchingSwitch();
            for (const place of [...turns.keys()]) {
                forget(place);
            }
            typists.clear();
            drawn.clear();
            changed();
            refusedPlaces.clear();
            isCallOut = false;
        }
    };
}
