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
// or a reload takes a new number, so everything before it is history.
import { inject, InjectionKey, onBeforeUnmount, reactive, Ref, ref, toRaw, watch } from "vue";
import { rowKey, sendKey } from "./heldMessage.partial";
import { TimelineMessage } from "./types.partial";

/** How a row came to be drawn new: risen in as it was sent here or heard live. */
export type ArrivalMotion = "rise";

/** The reader's own reaction landing: a new chip pops in, joining others rolls the count. */
export type ReactionMotion = { seq: number, code: string, motion: "pop" | "roll" };

/** What happened to one message's row, the newest of each kind, as the stores applied it. */
export type RowEvents = {
    /** The message arrived: sent here, or heard live. Only the first is kept. */
    arrival?: { seq: number, how: "sent" | "live" };

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
    changed: (message: Pick<TimelineMessage, "id" | "client_key">, event: string) => void;

    /** The reader's own reaction landed on a held copy. */
    reacted: (message: Pick<TimelineMessage, "id" | "client_key">, code: string, motion: ReactionMotion["motion"]) => void;

    /** A held root's thread got its first reply. */
    threadStarted: (root: Pick<TimelineMessage, "id" | "client_key">) => void;

    /** The counter now, read once as something mounts; not reactive. */
    now: () => number;

    /** What happened to a row, reactive per key. */
    of: (key: string) => RowEvents | undefined;

    /** A new view's arrivals, from now. */
    scope: () => MotionScope;

    /** Ends the record for good: nothing more is recorded and nothing new rises. */
    stop: () => void;
};

/** How the shell hands the record to the rows and views it draws. */
export const messageMotionKey: InjectionKey<MessageMotion> = Symbol("chat-message-motion");

/**
 * Creates the record.
 *
 * @returns The record.
 */
export function createMessageMotion(): MessageMotion {
    // Vue tracks a reactive Map's get per key, so a write wakes only the rows that read that key.
    const rows = reactive(new Map<string, RowEvents>());
    const liveScopes = new Set<{ since: number }>();
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

    /**
     * Lets go of every row no view still open could play: everything at or below the oldest open
     * view's number. A send's mark is kept for the session, as it says the person's own message never
     * rises a second time however late its live copy comes; one short string a send.
     */
    function prune(): void {
        const floor = Math.min(seq, ...[...liveScopes].map(scope => scope.since));
        const held = toRaw(rows);
        for (const [key, events] of held) {
            const newest = Math.max(events.arrival?.seq ?? 0, events.content?.seq ?? 0, events.reaction?.seq ?? 0, events.thread?.seq ?? 0);
            if (newest <= floor && events.arrival?.how !== "sent") {
                rows.delete(key);
            }
        }
    }

    return {
        sent: localId => record(`k:${sendKey(localId)}`, held => held?.arrival ? null : { ...held, arrival: { seq: ++seq, how: "sent" } }),

        arrived: message => record(rowKey(message), held => held?.arrival ? null : { ...held, arrival: { seq: ++seq, how: "live" } }),

        changed: (message, event) => record(rowKey(message), held => ({ ...held, content: { seq: ++seq, isEdit: event === "message.edited" } })),

        reacted: (message, code, motion) => record(rowKey(message), held => ({ ...held, reaction: { seq: ++seq, code, motion } })),

        threadStarted: root => record(rowKey(root), held => ({ ...held, thread: { seq: ++seq } })),

        now: () => seq,

        of: key => rows.get(key),

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
                    return "rise";
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
            rows.clear();
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

/** A view's arrivals as its rows ask them, each read once as the row mounts. */
export type ViewArrivals = {
    /**
     * Whether a row rises as it is drawn: the first time its key is drawn after its message was
     * sent here or heard live. The later renders that answer false leave a row already drawn as it
     * is. The person's message from another tab is news here and rises like anyone's; one sent here
     * rose as its pending row and never rises again.
     */
    isRowArriving: (key: string) => boolean;

    /**
     * Whether a folded line of messages by people the reader blocked rises. Every message the line
     * draws is claimed, not only the one it is keyed by, so the messages an unblock draws again one
     * by one were already drawn here and none rises as if heard now.
     */
    isRunArriving: (messages: readonly Pick<TimelineMessage, "id" | "client_key">[]) => boolean;
};

/**
 * A view's arrivals, from the moment it mounts until it goes: the feed's and the thread drawer's.
 * A view drawn where no record is provided draws nothing new.
 *
 * @returns What the view's rows ask of their arrival.
 */
export function useViewArrivals(): ViewArrivals {
    const scope = inject(messageMotionKey, null)?.scope() ?? null;
    onBeforeUnmount(() => scope?.dispose());
    const isRowArriving = (key: string): boolean => scope?.arrives(key) === "rise";

    return {
        isRowArriving,
        // every key asked, never cut short at the first that rises, so each is claimed
        isRunArriving: messages => messages.map(message => isRowArriving(rowKey(message))).includes(true)
    };
}
