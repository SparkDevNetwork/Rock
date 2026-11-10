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
// What happened to each message's row, for its motion. A row moves only for something that
// happened while it was on screen: a message sent here or heard arriving live, an edit, a delete or
// a hide, the reader's own reaction, a thread's first reply. The stores record each as they apply
// it; a page, a re-read, a catch-up or a remount only says what is and records nothing, so nothing
// can play for it. Each event carries a number from one counter, and a row or a view takes the
// counter as it mounts: it plays only what is above that. No clock and no timer: a switch, a remount
// or a reload takes a new number, so everything before it is history. Typing is recorded the same
// way, by place: where someone starts being drawn typing, and where the last typist stops and why.
import { inject, InjectionKey, onBeforeUnmount, reactive, Ref, ref, toRaw, watch } from "vue";
import { rowKey, sendKey } from "./heldMessage.partial";
import { TimelineMessage } from "./types.partial";
import type { MessageChangeEvent } from "./heldMessage.partial";

/**
 * How a row came to be drawn new: risen in as it was sent here or heard live, or in place, grown out
 * of its sender's typing bubble.
 */
export type ArrivalMotion = "rise" | "inPlace";

/** Why the last typist in a place stopped being drawn; only some of these are motion. */
export type TypingEnd = "sent" | "stopped" | "expired" | "overtaken" | "blocked" | "cleared";

/** Typing in one place: where its bubble last came and last went, and why it went. */
export type PlaceTyping = {
    started?: { seq: number };
    ended?: { seq: number, cause: TypingEnd };
};

/** The reader's own reaction landing: a new chip pops in, joining others rolls the count. */
export type ReactionMotion = { seq: number, code: string, motion: "pop" | "roll" };

/** What happened to one message's row, the newest of each kind, as the stores applied it. */
export type RowEvents = {
    /**
     * The message arrived: sent here, or heard live. Only the first is kept, except that her message
     * ending the last typing in its place turns its live arrival into the handoff.
     */
    arrival?: { seq: number, how: "sent" | "live" | "handoff" };

    /** Its words were replaced: an edit, a delete or a moderator's hide or show. */
    content?: { seq: number, isEdit: boolean };

    reaction?: ReactionMotion;

    /** Its thread got its first reply. */
    thread?: { seq: number };
};

/** A view's arrivals: what this feed or drawer has drawn new since it mounted. */
export type MotionScope = {
    /** Whether a row drawn under this key rises, answered once per key: the first ask claims it. */
    arrives: (key: string) => ArrivalMotion | null;

    /** The view is gone: what no view still open could play is let go. */
    dispose: () => void;
};

/** The shell's one record of what happened to each row. */
export type MessageMotion = {
    /** The person sent a message here, under its send's key. */
    sent: (localId: string) => void;

    /** A message was heard arriving live. One already sent here or heard keeps its first arrival. */
    arrived: (message: Pick<TimelineMessage, "id" | "client_key">) => void;

    /** A live edit, delete or change of visibility, by its event's name, was applied to a held copy. */
    changed: (message: Pick<TimelineMessage, "id" | "client_key">, event: MessageChangeEvent) => void;

    /** The reader's own reaction landed on a held copy. */
    reacted: (message: Pick<TimelineMessage, "id" | "client_key">, code: string, motion: ReactionMotion["motion"]) => void;

    /** A held root's thread got its first reply. */
    threadStarted: (root: Pick<TimelineMessage, "id" | "client_key">) => void;

    /** Someone is drawn typing in a place where no one was. */
    typingStarted: (place: string) => void;

    /**
     * The last typist drawn in a place stopped being drawn. Her own message ending it is named, and
     * its arrival, recorded just before in the same pass, becomes the handoff.
     */
    typingEnded: (place: string, cause: TypingEnd, message?: Pick<TimelineMessage, "id" | "client_key">) => void;

    /** The counter now, read once as something mounts; not reactive. */
    now: () => number;

    /** What happened to a row, reactive per key. */
    of: (key: string) => RowEvents | undefined;

    /** Typing in a place, reactive per place. */
    typingOf: (place: string) => PlaceTyping | undefined;

    /**
     * A count that moves with every event that changes a view's tail: a send, an arrival, typing
     * starting or ending. Reactive, so a view watching it before the render reads where its tail
     * stands on screen before anything moves; a row's own changes never move it.
     */
    tail: () => number;

    /** A new view's arrivals, from now. */
    scope: () => MotionScope;

    /** Ends the record for good: nothing more is recorded and nothing new rises. */
    stop: () => void;
};

/** What the rows and views read of the record. Only the stores record, so a component never holds a writer. */
export type MotionReader = Pick<MessageMotion, "now" | "of" | "typingOf" | "tail" | "scope">;

/** How the shell hands the record to the rows and views it draws, to read only. */
export const messageMotionKey: InjectionKey<MotionReader> = Symbol("chat-message-motion");

/**
 * Creates the record.
 *
 * @returns The record.
 */
export function createMessageMotion(): MessageMotion {
    // Vue tracks a reactive Map's get per key, so a write wakes only the rows that read that key.
    const rows = reactive(new Map<string, RowEvents>());
    const places = reactive(new Map<string, PlaceTyping>());
    const tail = ref(0);
    const liveScopes = new Set<{ since: number }>();
    // Sends whose live copy has not been heard yet, by row key: until it is, the mark is what keeps
    // that copy from rising as heard, however late it comes. Plain, since no view reads it.
    const unheardSends = new Set<string>();
    let seq = 0;
    let isStopped = false;

    /** Writes one kind of event on a row, in a new entry so every reader of the key sees it. */
    function record(key: string, events: (held: RowEvents | undefined) => RowEvents | null): void {
        if (isStopped) {
            return;
        }
        const next = events(toRaw(rows).get(key));
        if (next) {
            rows.set(key, next);
        }
    }

    /** Writes typing in a place, in a new entry so every reader of the place sees it, and moves the tail's count. */
    function recordTyping(place: string, typing: (held: PlaceTyping | undefined) => PlaceTyping): void {
        if (isStopped) {
            return;
        }
        places.set(place, typing(toRaw(places).get(place)));
        tail.value++;
    }

    /** A row came to a view's tail, as a new arrival or a copy of one recorded already. */
    function arrivedAtTail(): void {
        if (!isStopped) {
            tail.value++;
        }
    }

    /**
     * Lets go of every row no view still open could play: everything at or below the oldest open
     * view's number, as a view goes. A send's mark stays until its live copy is heard, since it is
     * what keeps that copy from rising as heard however late it comes. While one room stays open
     * nothing newer than its feed is let go: about one small entry for each message heard live in
     * it, the same count of messages its timeline holds, and let go at the next switch (an estimate:
     * 10,000 messages heard in a day at about 150 bytes each is 1.5 MB).
     */
    function prune(): void {
        const floor = Math.min(seq, ...[...liveScopes].map(scope => scope.since));
        const held = toRaw(rows);
        for (const [key, events] of held) {
            const newest = Math.max(events.arrival?.seq ?? 0, events.content?.seq ?? 0, events.reaction?.seq ?? 0, events.thread?.seq ?? 0);
            if (newest <= floor && !unheardSends.has(key)) {
                rows.delete(key);
            }
        }
        for (const [place, typing] of toRaw(places)) {
            if (Math.max(typing.started?.seq ?? 0, typing.ended?.seq ?? 0) <= floor) {
                places.delete(place);
            }
        }
    }

    return {
        sent: localId => {
            record(`k:${sendKey(localId)}`, held => {
                if (held?.arrival) {
                    return null;
                }
                unheardSends.add(`k:${sendKey(localId)}`);
                return { ...held, arrival: { seq: ++seq, how: "sent" } };
            });
            arrivedAtTail();
        },

        // A copy of a message already arrived still moves the tail's count: its row may change size,
        // as a send's row does once its live copy stands in it, and the view glides that too.
        arrived: message => {
            record(rowKey(message), held => {
                if (held?.arrival) {
                    unheardSends.delete(rowKey(message));
                    return null;
                }
                return { ...held, arrival: { seq: ++seq, how: "live" } };
            });
            arrivedAtTail();
        },

        changed: (message, event) => record(rowKey(message), held => ({ ...held, content: { seq: ++seq, isEdit: event === "message.edited" } })),

        reacted: (message, code, motion) => record(rowKey(message), held => ({ ...held, reaction: { seq: ++seq, code, motion } })),

        threadStarted: root => record(rowKey(root), held => ({ ...held, thread: { seq: ++seq } })),

        typingStarted: place => recordTyping(place, held => ({ ...held, started: { seq: ++seq } })),

        typingEnded: (place, cause, message) => {
            // Her arrival was recorded just before, in the same live pass, so the view reads the two
            // side by side: the arrival's number one below the end's.
            if (cause === "sent" && message) {
                record(rowKey(message), held => held?.arrival?.how === "live" ? { ...held, arrival: { ...held.arrival, how: "handoff" } } : null);
            }
            recordTyping(place, held => ({ ...held, ended: { seq: ++seq, cause } }));
        },

        now: () => seq,

        of: key => rows.get(key),

        typingOf: place => places.get(place),

        tail: () => tail.value,

        scope: (): MotionScope => {
            const scope = { since: seq };
            const claimed = new Set<string>();
            if (!isStopped) {
                liveScopes.add(scope);
            }

            return {
                arrives: key => {
                    // read raw: a view asks as it draws, and an event on a row must not redraw the view
                    const arrival = toRaw(rows).get(key)?.arrival;
                    if (isStopped || claimed.has(key) || !arrival || arrival.seq <= scope.since) {
                        return null;
                    }
                    claimed.add(key);
                    return arrival.how === "handoff" ? "inPlace" : "rise";
                },

                dispose: () => {
                    if (liveScopes.delete(scope)) {
                        prune();
                    }
                }
            };
        },

        stop: () => {
            isStopped = true;
            liveScopes.clear();
            unheardSends.clear();
            rows.clear();
            places.clear();
        }
    };
}

/** A row's own motion: what the look draws, each set by an event on the row since it mounted. */
export type RowMotion = {
    /** An edit, a delete or a hide just replaced the text, so it crossfades once. */
    isCrossfading: Ref<boolean>;

    /** An edit landed while the row was on screen, so its "Edited" slides in once. */
    isEditedNow: Ref<boolean>;

    /** The message's thread started while the row was on screen, so its pill arrives. */
    isThreadStartedHere: Ref<boolean>;

    /** The reader's own reaction that landed while the row was on screen, which its chip plays. */
    reaction: Readonly<Ref<ReactionMotion | null>>;
};

/**
 * A row's motion from the shell's record, from the moment it mounts: only an event recorded after
 * that plays. A row drawn where no record is provided plays nothing.
 *
 * @param key The row's key: its send's key, or its message's id.
 *
 * @returns What the row's look draws of its own motion.
 */
export function useRowMotion(key: () => string | null): RowMotion {
    const motion = inject(messageMotionKey, null);
    const isCrossfading = ref(false);
    const isEditedNow = ref(false);
    const isThreadStartedHere = ref(false);
    const reaction = ref<ReactionMotion | null>(null);
    if (!motion) {
        return { isCrossfading, isEditedNow, isThreadStartedHere, reaction };
    }

    const since = motion.now();
    const events = (): RowEvents | undefined => {
        const rowAt = key();
        return rowAt ? motion.of(rowAt) : undefined;
    };

    // Before the render, so a thread's pill drawn by the same change already knows it is new.
    watch(() => events()?.content, content => {
        if (content && content.seq > since) {
            isCrossfading.value = true;
            isEditedNow.value = content.isEdit;
        }
    });
    watch(() => events()?.thread?.seq, threadAt => {
        if (threadAt !== undefined && threadAt > since) {
            isThreadStartedHere.value = true;
        }
    });
    watch(() => events()?.reaction, latest => {
        if (latest && latest.seq > since) {
            reaction.value = latest;
        }
    });

    return { isCrossfading, isEditedNow, isThreadStartedHere, reaction };
}

/** A view's arrivals as its tail claims them, each once, as a row is first drawn after a change. */
export type ViewArrivals = {
    /**
     * How a row arrives: the first time its key is drawn after its message was sent here or heard
     * live it rises, or grows in place out of its sender's typing bubble; asked again, it is
     * already drawn and arrives no more. The person's message from another tab is news here and
     * rises like anyone's; one sent here rose as its pending row and never rises again.
     */
    arrives: (key: string) => ArrivalMotion | null;

    /**
     * How a folded line of messages by people the reader blocked arrives. Every message the line
     * draws is claimed, not only the one it is keyed by, so the messages an unblock draws again one
     * by one were already drawn here and none arrives as if heard now.
     */
    arrivesRun: (messages: readonly Pick<TimelineMessage, "id" | "client_key">[]) => ArrivalMotion | null;
};

/**
 * A view's arrivals, from the moment it mounts until it goes: the feed's and the thread drawer's.
 * A view drawn where no record is provided draws nothing new.
 *
 * @returns What the view's tail asks of each row's arrival.
 */
export function useViewArrivals(): ViewArrivals {
    const scope = inject(messageMotionKey, null)?.scope() ?? null;
    onBeforeUnmount(() => scope?.dispose());
    const arrives = (key: string): ArrivalMotion | null => scope?.arrives(key) ?? null;

    return {
        arrives,
        // every key asked, never cut short at the first that arrives, so each is claimed
        arrivesRun: messages => messages.map(message => arrives(rowKey(message))).includes("rise") ? "rise" : null
    };
}
