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
// Signals that say what someone is doing now rather than change anything held: typing. The
// person's own go out at once on the first change to a box and then at most once an interval while
// the text keeps changing; there is no stop signal, and the send side holds no timer, so nothing is
// sent when the person stops, sends, leaves or hides the page. The others' arrive on either topic
// and are kept for the open conversation's places until they expire, a message of theirs follows
// them, or the person blocks them.
//
// This lives apart from the timelines because what it holds expires on its own clock and is never
// history: a signal lost costs a line for a few seconds, and nothing is caught up or repaired.

import { reactive } from "vue";
import type { CallResult, PlatformCall } from "../platformCall.partial";
import { isReportedRefusal } from "../errors.partial";
import type { ShellState } from "../shell.partial";
import { ChatError, ChatPerson, MessageCreatedEvent, SidebarRow, TypingEvent } from "../types.partial";
import { quoteKey } from "./useMessageActions.partial";
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

    /** Whether one of the person's own sends to a place is still out. */
    isSending: (channelId: string, rootId: number | null) => boolean;

    /** The person's primary alias, which the platform writes as a signal's typist. */
    personAliasGuid: string;

    isBlocked: (personAliasGuid: string) => boolean;

    /** Tells the platform of a refusal its one rule for reporting picks out. */
    report: (code: string) => void;

    /** Whether the shell has stopped or its session ended, read at each resumed step. */
    isStopped: () => boolean;
};

/** Typing, as the shell holds it. */
export type Presence = {
    /** Each place's typists by alias, in the order first heard, so the line does not reorder as they refresh; reactive. */
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

    /** The line a place's composer draws, or null when no one else is typing there. */
    lineFor: (place: string | null) => string | null;

    /** A message arrived live, which ends its sender's typing in its place. */
    messageArrived: (channelId: string, created: Partial<MessageCreatedEvent>) => void;

    /** A conversation's topic was joined, first or again: whatever was heard before is stale. */
    joined: (channelId: string) => void;

    /** The people blocked gained someone: their line goes at once. */
    remask: () => void;

    stop: () => void;
};

/** One typist in a place: when their line expires, and the newest message their signal followed. */
type Typist = { expiresAt: number, after: number };

/**
 * The words for the people typing in a place.
 *
 * @param people Each typist in the order first heard, or null for one whose name is not held.
 *
 * @returns The line, or null for none.
 */
export function typingLine(people: ReadonlyArray<Pick<ChatPerson, "nick_name" | "last_name"> | null>): string | null {
    if (people.length === 0) {
        return null;
    }
    if (people.length > 2) {
        return "Several people are typing…";
    }

    const names = people.map(person => person?.nick_name || person?.last_name || null);

    // Two of one nick name are told apart by their last initial, only then, so the usual line
    // stays as short as the names.
    if (names.length === 2 && names[0] !== null && names[0] === names[1]) {
        people.forEach((person, index) => {
            const initial = person?.nick_name && person.last_name ? ` ${person.last_name.charAt(0)}.` : "";
            names[index] = `${names[index]}${initial}`;
        });
    }

    if (names.length === 1) {
        return `${names[0] ?? "Someone"} is typing…`;
    }

    return `${names[0] ?? "Someone"} and ${names[1] ?? "someone"} are typing…`;
}

/**
 * Creates the session's typing.
 *
 * @param dependencies What it reaches outside itself.
 *
 * @returns The typing state and its handlers.
 */
export function createPresence(dependencies: PresenceDependencies): Presence {
    const { state: shellState, settings, isStopped } = dependencies;
    const self = dependencies.personAliasGuid.toLowerCase();

    const state = reactive({ typists: {} as Record<string, string[]> });

    // #region Send

    /** When each place's last signal went, so the next goes once the interval has passed. */
    const lastSentAt = new Map<string, number>();

    /** Whether a signal is out; the next waits for it, so a slow route never stacks calls. */
    let isCallOut = false;

    /** The token typing was refused as off under; silent until the session holds another. */
    let offToken: string | null = null;

    /** Places the platform refused for good this page: a room that is not a conversation, a thread whose root is gone. */
    const refusedPlaces = new Set<string>();

    /** Whether a signal may go from this place now; the one rule every send condition lives in. */
    function canSignal(channelId: string, rootId: number | null, place: string): boolean {
        const row = dependencies.rowOf(channelId);
        // a channel never carries typing: only a direct message or a group does
        if (row?.room_kind !== "person" && row?.room_kind !== "group") {
            return false;
        }
        if (!settings().flags.typing || (offToken !== null && offToken === dependencies.currentToken())) {
            return false;
        }
        // nothing could come of a signal while the banner shows or the box cannot send
        if (shellState.connection !== null || !shellState.isComposerOpen || row.can_post === false) {
            return false;
        }
        if (refusedPlaces.has(channelId) || refusedPlaces.has(place) || isCallOut) {
            return false;
        }
        // A send still out has not reached this client's history, so a signal now would name a
        // message older than it and viewers would drop it; the next change after it lands goes.
        if (dependencies.isSending(channelId, rootId)) {
            return false;
        }

        const last = lastSentAt.get(place);
        return last === undefined || Date.now() - last >= settings().limits.typing_interval_ms;
    }

    /** Sends one signal and acts on a refusal; every failure is quiet. */
    async function signal(channelId: string, rootId: number | null, place: string): Promise<void> {
        const after = dependencies.newestHeld(channelId, rootId);
        const region = settings().routes.region;
        // the token the call goes with, so a refresh while it is out is never what gets silenced
        const token = dependencies.currentToken();

        isCallOut = true;
        lastSentAt.set(place, Date.now());
        let result: CallResult | null;
        try {
            result = await dependencies.callFunction(
                "chat-typing",
                { p_channel_id: channelId, p_parent_id: rootId, ...(after !== null ? { p_after: after } : {}) },
                region ? { headers: { "x-region": region } } : undefined
            );
        }
        catch {
            // a call that rejects rather than answering is dropped like any other failure
            result = null;
        }
        finally {
            // however the call ended the next may go, unless stop has already cleared everything
            if (!isStopped()) {
                isCallOut = false;
            }
        }

        // a pass and a throttled call answer alike, as success with nothing more to do
        if (isStopped() || !result || result.ok) {
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

    // #endregion Send

    // #region Receive

    /** The typists held for each place of the open conversation, by lower-case alias. */
    const typists = new Map<string, Map<string, Typist>>();

    /** The newest message heard live from each person in each place, by place and lower-case alias. */
    const newestHeard = new Map<string, Map<string, number>>();

    /** The one timer, set for the soonest expiry. */
    let expiryTimer: ReturnType<typeof setTimeout> | null = null;

    /** Puts what is held on the screen, each place's typists in the order first heard. */
    function publish(): void {
        const next: Record<string, string[]> = {};
        for (const [place, held] of typists) {
            if (held.size > 0) {
                next[place] = [...held.keys()];
            }
        }
        state.typists = next;
    }

    /** What is held changed: it is drawn, and the one timer set for the soonest expiry left. */
    function changed(): void {
        publish();
        schedule();
    }

    /** Removes every typist due and sets the one timer for the next. */
    function expire(): void {
        expiryTimer = null;
        const now = Date.now();
        for (const [place, held] of typists) {
            for (const [alias, typist] of held) {
                if (typist.expiresAt <= now) {
                    held.delete(alias);
                }
            }
            if (held.size === 0) {
                typists.delete(place);
            }
        }
        changed();
    }

    /** Sets the one timer for the soonest expiry held, replacing any set before. */
    function schedule(): void {
        if (expiryTimer !== null) {
            clearTimeout(expiryTimer);
            expiryTimer = null;
        }
        let soonest = Infinity;
        for (const held of typists.values()) {
            for (const typist of held.values()) {
                soonest = Math.min(soonest, typist.expiresAt);
            }
        }
        if (soonest !== Infinity) {
            expiryTimer = setTimeout(expire, Math.max(0, soonest - Date.now()));
        }
    }

    /** Forgets every typist and message heard, and the timer. */
    function clear(): void {
        typists.clear();
        newestHeard.clear();
        changed();
    }

    function heard(event: string, payload: unknown, topicChannelId?: string): void {
        if (isStopped() || event !== "typing") {
            return;
        }

        const typing = payload as Partial<TypingEvent> | null;
        const channelId = typing?.channel_id;
        const alias = typeof typing?.person_alias_guid === "string" ? typing.person_alias_guid.toLowerCase() : null;
        if (typeof channelId !== "string" || !alias) {
            return;
        }
        // A conversation topic carries its own room only, and only the open conversation's
        // typists are kept; the person's own echo and other tabs are not someone else typing.
        if ((topicChannelId !== undefined && channelId !== topicChannelId) || channelId !== shellState.activeChannelId) {
            return;
        }
        if (alias === self || dependencies.isBlocked(alias)) {
            return;
        }

        // A signal that names a message older than one of theirs already heard was sent before
        // that message, however late it arrives; their message ended that typing.
        const place = quoteKey(channelId, typeof typing?.parent_id === "number" ? typing.parent_id : null);
        const after = typeof typing?.after === "number" ? typing.after : 0;
        if (after < (newestHeard.get(place)?.get(alias) ?? 0)) {
            return;
        }

        let held = typists.get(place);
        if (!held) {
            held = new Map();
            typists.set(place, held);
        }
        held.set(alias, { expiresAt: Date.now() + settings().limits.typing_expiry_ms, after });
        changed();
    }

    function messageArrived(channelId: string, created: Partial<MessageCreatedEvent>): void {
        const alias = typeof created.person_alias_guid === "string" ? created.person_alias_guid.toLowerCase() : null;
        if (isStopped() || !alias || typeof created.id !== "number" || channelId !== shellState.activeChannelId) {
            return;
        }

        const place = quoteKey(channelId, typeof created.parent_id === "number" ? created.parent_id : null);
        let heardHere = newestHeard.get(place);
        if (!heardHere) {
            heardHere = new Map();
            newestHeard.set(place, heardHere);
        }
        heardHere.set(alias, Math.max(heardHere.get(alias) ?? 0, created.id));

        // a signal that already follows this message overtook it, and stays
        const typist = typists.get(place)?.get(alias);
        if (typist && typist.after < created.id) {
            typists.get(place)?.delete(alias);
            changed();
        }
    }

    // #endregion Receive

    return {
        state,

        owns: event => event === "typing",

        heard,

        typed: (channelId, rootId, text) => {
            const place = quoteKey(channelId, rootId);
            if (isStopped() || text.trim() === "" || !canSignal(channelId, rootId, place)) {
                return;
            }
            void signal(channelId, rootId, place);
        },

        lineFor: place => {
            const aliases = place ? state.typists[place] : undefined;
            if (!place || !aliases?.length) {
                return null;
            }
            // a place key starts with its conversation's id
            const people = dependencies.peopleOf(place.split("/")[0]);
            return typingLine(aliases.map(alias => people.find(person => person.person_alias_guid.toLowerCase() === alias) ?? null));
        },

        messageArrived,

        // Only the open conversation's places are ever held, and its topic joined again means
        // signals and messages were missed meanwhile.
        joined: () => clear(),

        remask: () => {
            for (const held of typists.values()) {
                for (const alias of held.keys()) {
                    if (dependencies.isBlocked(alias)) {
                        held.delete(alias);
                    }
                }
            }
            changed();
        },

        stop: () => {
            clear();
            lastSentAt.clear();
            refusedPlaces.clear();
            isCallOut = false;
        }
    };
}
