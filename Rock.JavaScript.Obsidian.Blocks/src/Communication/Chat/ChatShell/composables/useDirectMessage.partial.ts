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
// Starting a direct message. The person picks people from the platform's search; people who
// already share a conversation open it, and otherwise a draft opens. A draft is only on this
// page: nothing exists anywhere until its first message, which asks Rock for the conversation.
// Rock is the only one that creates a conversation, so a person cannot start one with somebody
// Rock would not let them reach.
//
// Each first message moves through one phase at a time: Rock is asked once; a refusal is final;
// a conversation the platform does not hold yet is waited for, tried on the person's membership
// signal or after growing waits, under one row and one send key so it posts once. The draft stays
// on screen holding the text until its conversation is: a draft goes only when an open succeeds
// or the message is posted. A first message that ends failed is never left where the person
// cannot see it: in its conversation as a failed row, on its draft with a sentence, or in the
// error toast with the text kept for their next draft to the same people.
//
// A failed first message went to the platform, which may have posted it before the answer was
// lost, so it is only ever tried again as itself, under its own row and key. A refused one never
// left Rock, so its text is the only copy and goes back in the box for the person to send.
import { reactive, toRaw } from "vue";
import { computeBackoff } from "../platformCall.partial";
import { ChatError, PersonRow } from "../types.partial";
import { defaultRetryDelay } from "./useHistory.partial";
import { newLocalId, SendTry } from "./useSend.partial";

export type { PersonRow };

/** The most people a person can start a conversation with, besides themselves. */
export const maxOthers = 8;

/**
 * How many times a first message is tried after Rock made its conversation and the platform had
 * not taken it. Each wait's ceiling doubles from the reconnect base, so with the default 1 s base
 * the fifth is 16 s and the five take about half a minute at most; past that the platform is not
 * about to take it, and the person decides with the ordinary failed message.
 */
const maxRetriedSends = 5;

/** Shown on a draft whose message could not be sent because the platform does not hold its conversation. */
const notReadySentence = "The conversation is not ready yet, so your message was not sent. Send it again in a moment.";

/** What Rock's door answers when asked for a conversation. */
export type DoorResult = {
    /** "ok", or the refusal's code. */
    code: string;

    /** The conversation, when there is one. */
    channelGuid: string | null;

    /** True when Rock made the conversation but the platform has not taken it yet. */
    isPending: boolean;

    /** The sentence to show for a refusal. */
    message: string | null;

    /** The person a refusal is about, when it is about one. */
    personAliasGuid: string | null;
};

/** A conversation that has not been sent its first message yet. */
export type DirectMessageDraft = {
    people: PersonRow[];

    /** The first message, kept while it is on its way and after it is refused or fails. */
    body: string;

    /** "starting" from a send until the message is posted, refused or failed. */
    status: "draft" | "starting";

    /** The conversation Rock answered, while the platform has not shown it to the person yet. */
    channelGuid: string | null;
};

/** What the direct messages reach outside themselves. */
export type DirectMessageDependencies = {
    /** The platform's people search. */
    search: (query: string) => Promise<{ ok: true, people: PersonRow[] } | { ok: false, error: ChatError }>;

    /** The platform's lookup of the conversation these people already share, by alias guid. */
    findConversation: (personAliasGuids: string[]) => Promise<{ ok: true, channelId: string | null } | { ok: false, error: ChatError }>;

    /** Rock's door, which finds or creates the conversation. */
    startConversation: (personAliasGuids: string[]) => Promise<DoorResult>;

    /** Opens a channel on screen. False when the platform refused it or it could not load. */
    openChannel: (channelId: string) => boolean | Promise<boolean>;

    /**
     * The ordinary send. Resolves true when the platform confirmed it. A first message passes its
     * try, which names the same row every time.
     */
    send: (channelId: string, body: string, attempt?: SendTry) => Promise<boolean>;

    /** The first wait before a try and the longest; the history's by default. */
    retryDelay?: () => { baseMs: number, capMs: number };

    /** Waits; a timer by default. */
    sleep?: (ms: number) => Promise<void>;

    /** Spreads the waits of many clients apart; Math.random by default. */
    random?: () => number;

    /** Tells the person about a failure that has no draft on screen to show it. */
    report?: (error: ChatError) => void;
};

/** The direct messages a shell holds. */
export type DirectMessages = {
    /** The people picked so far. */
    chosen: PersonRow[];

    /** The draft on screen, or null. */
    draft: DirectMessageDraft | null;

    /** The sentence for the last thing that failed, or null. */
    error: string | null;

    /**
     * Text Rock refused while the person was elsewhere, for the box of the conversation those
     * people now share, or null. A new object each time, so the box takes it even when the
     * same text comes back.
     */
    held: { body: string } | null;

    /** The people the platform's search answers for the text typed. */
    search: (query: string) => Promise<PersonRow[]>;

    /** Picks a person. False when eight are already picked. */
    choose: (person: PersonRow) => boolean;

    /** Takes a picked person back out. */
    unchoose: (personAliasGuid: string) => void;

    /** Opens the conversation the picked people share, or a draft for them. */
    open: () => Promise<void>;

    /** Sends a draft's first message. True when it was sent. */
    sendFirst: (body: string) => Promise<boolean>;

    /** The text in the composer's box changed, so the draft holds what the person sees. */
    edit: (body: string) => void;

    /** The person's membership changed; a conversation Rock made may now be on the platform. */
    membershipChanged: (channelId?: string | null) => Promise<void>;

    /** Closes the draft and forgets the picked people. A first message Rock has a conversation for is still sent. */
    close: () => void;

    /** Stops everything a first message would still do, because the shell is going away. */
    stop: () => void;
};

/**
 * Where a first message is. Asking: Rock has not answered. Waiting: Rock made the conversation
 * and the platform has not taken it, so a try comes on a signal or a wait. Trying: a send is on
 * its way. Sent, refused and failed are where it ends, though a failed one is sent again from its
 * draft as its retry.
 */
type FirstMessagePhase = "asking" | "waiting" | "trying" | "sent" | "refused" | "failed";

/** One first message, from the send that asks Rock to where it ends. */
type FirstMessage = {
    phase: FirstMessagePhase;

    people: PersonRow[];

    body: string;

    /** The row its tries share, so they carry one key and show as one message. */
    localId: string;

    /** The conversation Rock answered, once it has. */
    channelId: string | null;

    /** Retried sends made while waiting for the platform. */
    tries: number;

    /** The draft it shows on, which is on screen only while it is the shell's draft. */
    draft: DirectMessageDraft | null;

    /** True once its conversation replaced its draft, so its row is in front of the person. */
    isShown: boolean;
};

/**
 * Whether the sidebar shows the New message button. Rock checks the same right again when the
 * conversation is asked for, so this only spares a person a button that would be refused.
 *
 * @param session The part of the session that says whether the person may start one.
 *
 * @returns True when the button shows.
 */
export function isNewMessageShown(session: { canStartDm?: boolean | null }): boolean {
    return session.canStartDm === true;
}

/**
 * Whether the composer keeps the text in its box when it hands it over. A draft's first message
 * that Rock refuses has no failed row in a feed to keep it on, so the box keeps it until the draft
 * goes away; an ordinary send's failure stays on its own row with a retry.
 *
 * @param draft The draft on screen, or null.
 *
 * @returns True while a draft is on screen.
 */
export function isTextKeptOnSend(draft: DirectMessageDraft | null): boolean {
    return draft !== null;
}

/**
 * Creates the direct messages.
 *
 * @param dependencies What they reach outside themselves.
 *
 * @returns The direct messages.
 */
export function createDirectMessages(dependencies: DirectMessageDependencies): DirectMessages {
    const state = reactive({
        chosen: [] as PersonRow[],
        draft: null as DirectMessageDraft | null,
        error: null as string | null,
        held: null as { body: string } | null
    });

    const sleep = dependencies.sleep ?? ((ms: number): Promise<void> => new Promise(resolve => setTimeout(resolve, ms)));
    const random = dependencies.random ?? Math.random;

    /** Conversations the person was added to while Rock had not answered yet. */
    const arrived = new Set<string>();

    /** Asks of Rock still on their way; arrivals are kept only while there is one. */
    let startsInFlight = 0;

    /** First messages waiting for the platform to take their conversation, or being tried. */
    const waiting = new Set<FirstMessage>();

    /** The first message each draft carries, by the draft's raw object, since the state hands out proxies. */
    const carriedBy = new WeakMap<DirectMessageDraft, FirstMessage>();

    /**
     * Failed first messages a draft tries again before its own, oldest first, by the draft's raw
     * object. Their text is not in the box, which holds the draft's own message.
     */
    const queuedOn = new WeakMap<DirectMessageDraft, FirstMessage[]>();

    /**
     * First messages the person could not see end, by the people they were for and in the order
     * they ended, so the next draft to those people takes them up instead of losing them: refused
     * text in its box, failed ones tried again under their own rows. A list, since two first
     * messages to the same people can both be on their way when the person leaves.
     */
    const kept = new Map<string, FirstMessage[]>();

    let isStopped = false;

    /** The same key for the same people in any order. */
    function peopleKey(people: PersonRow[]): string {
        return people.map(p => p.person_alias_guid.toLowerCase()).sort().join(",");
    }

    /** Whether a first message's draft is the one on screen. */
    function isOnScreen(message: FirstMessage): boolean {
        return message.draft !== null && state.draft !== null && toRaw(state.draft) === toRaw(message.draft);
    }

    /** Ties a first message to a draft, so a send from that draft finds it. */
    function carry(draft: DirectMessageDraft, message: FirstMessage): void {
        carriedBy.set(toRaw(draft), message);
        message.draft = draft;
    }

    /** Keeps a first message's text for the next draft to its people. */
    function keep(message: FirstMessage): void {
        const key = peopleKey(message.people);
        const list = kept.get(key) ?? [];
        if (!list.includes(message)) {
            list.push(message);
        }
        kept.set(key, list);
    }

    /** Tells the person in the toast about a first message whose draft they had left. */
    function reportLeft(message: FirstMessage, error: Omit<ChatError, "text">, reason: string): void {
        keep(message);
        const names = message.people.map(p => p.nick_name).join(", ");
        dependencies.report?.({ ...error, text: `Your message to ${names} could not be sent. ${reason}` });
    }

    /**
     * Takes the draft off the screen. A failed first message the person read on it still has its
     * text and its row, so they wait for the next draft to the same people, and so do the failed
     * ones it was to try first; a first message on its way carries on alone; a plain draft, or one
     * Rock refused in front of the person, is dropped.
     */
    function leaveDraft(): void {
        const draft = state.draft;
        if (!draft) {
            return;
        }

        // Older than the draft's own message, so they are kept ahead of it.
        for (const queued of queuedOn.get(toRaw(draft)) ?? []) {
            keep(queued);
        }

        // The text kept is the box's, edits never sent included, since that is what the person
        // last saw and would have to type again otherwise.
        const message = carriedBy.get(toRaw(draft));
        if (message?.phase === "failed") {
            message.body = draft.body;
            keep(message);
        }

        state.draft = null;
    }

    /**
     * Opens a first message's conversation in place of its draft. The draft goes only once the
     * conversation is on screen, so a refused open leaves the person on the draft with the text.
     *
     * @returns True when the conversation replaced the draft.
     */
    async function showConversation(message: FirstMessage): Promise<boolean> {
        const isOpened = await dependencies.openChannel(message.channelId as string);
        if (isStopped) {
            return false;
        }

        // An open the person replaced by going elsewhere leaves nothing to take off the screen.
        if (isOpened && isOnScreen(message)) {
            state.draft = null;
            message.isShown = true;
        }

        return message.isShown;
    }

    /** Settles a first message whose send has answered, so the person can see where it ended. */
    async function finish(message: FirstMessage, isSent: boolean): Promise<void> {
        waiting.delete(message);

        if (isSent) {
            message.phase = "sent";

            // Posted, so the draft goes whatever the open answers: kept, it would offer to send
            // the message a second time.
            if (isOnScreen(message)) {
                await showConversation(message);
                if (!isStopped && isOnScreen(message)) {
                    state.draft = null;
                }
            }
            return;
        }

        message.phase = "failed";

        // Its failed row, with its Retry, is in the conversation the person was taken to.
        if (message.isShown) {
            return;
        }

        if (isOnScreen(message)) {
            const isShown = await showConversation(message);
            if (isStopped || isShown) {
                return;
            }

            // The platform still does not hold it: the draft keeps the text and is its retry.
            if (isOnScreen(message) && message.draft) {
                message.draft.status = "draft";
                state.error = notReadySentence;
                return;
            }
        }

        reportLeft(message, { code: "door.first_message_failed", severity: "failed" }, "The conversation is not ready yet. Your text is kept for your next message to them.");
    }

    /**
     * Tries a waiting first message once, unless a try is already on its way or it has ended.
     * A held conversation, one the membership signal says the platform has, opens before the send
     * so the row shows sending in it; a refused open leaves the draft and the try goes on.
     */
    async function tryWaiting(message: FirstMessage, isHeld: boolean): Promise<void> {
        if (message.phase !== "waiting" || isStopped) {
            return;
        }

        // Marked before any await, so a signal and a wait ending together send it once.
        message.phase = "trying";
        message.tries++;
        const isLast = message.tries >= maxRetriedSends;

        if (isHeld && isOnScreen(message)) {
            await showConversation(message);
            if (isStopped) {
                return;
            }
        }

        const isSent = await sendTry(message, isLast);
        if (isStopped) {
            return;
        }

        if (isSent || isLast) {
            await finish(message, isSent);
        }
        else {
            message.phase = "waiting";
        }
    }

    /**
     * Waits for the platform to take a first message's conversation: a try after each of a run of
     * growing waits until it ends. A membership signal tries it sooner, and the wait ending then
     * finds it ended or on its way.
     */
    async function waitForConversation(message: FirstMessage): Promise<void> {
        message.phase = "waiting";
        waiting.add(message);

        let attempt = 0;
        while ((message.phase === "waiting" || message.phase === "trying") && !isStopped) {
            const delay = dependencies.retryDelay?.() ?? defaultRetryDelay;
            await sleep(computeBackoff(attempt++, delay.baseMs, delay.capMs, random));
            await tryWaiting(message, false);
        }
    }

    /**
     * Sends one try of a first message's row. A send that rejects is a failed try, so the message
     * still ends where the person can see it rather than leaving its draft starting for good.
     */
    async function sendTry(message: FirstMessage, isLast: boolean): Promise<boolean> {
        try {
            return await dependencies.send(message.channelId as string, message.body, { localId: message.localId, isLast });
        }
        catch {
            return false;
        }
    }

    /**
     * Tries a draft's queued failed messages again, oldest first, each as itself. Stops at the
     * first that fails, since the ones after it would fail the same way and must not post ahead
     * of it.
     *
     * @returns True when none is left.
     */
    async function retryQueued(draft: DirectMessageDraft): Promise<boolean> {
        const queued = queuedOn.get(toRaw(draft)) ?? [];
        while (queued.length > 0) {
            const message = queued[0];
            message.phase = "trying";
            const isSent = await sendTry(message, true);
            if (isStopped) {
                return false;
            }

            if (!isSent) {
                message.phase = "failed";
                if (state.draft && toRaw(state.draft) === toRaw(draft)) {
                    draft.status = "draft";
                    state.error = notReadySentence;
                }
                else {
                    reportLeft(message, { code: "door.first_message_failed", severity: "failed" }, "The conversation is not ready yet. Your text is kept for your next message to them.");
                }
                return false;
            }

            message.phase = "sent";
            queued.shift();
        }

        return true;
    }

    /** Sends a first message once, as an ordinary send of its row, and settles it by the answer. */
    async function sendOnce(message: FirstMessage): Promise<boolean> {
        message.phase = "trying";
        const isSent = await sendTry(message, true);
        if (isStopped) {
            return false;
        }

        await finish(message, isSent);
        return isSent;
    }

    /** Asks Rock for a first message's conversation and takes it from there by the answer. */
    async function start(draft: DirectMessageDraft, message: FirstMessage): Promise<boolean> {
        startsInFlight++;
        let answer: DoorResult;
        try {
            answer = await dependencies.startConversation(message.people.map(p => p.person_alias_guid));
        }
        catch {
            // Rock could not be reached, so nothing was made: it ends like a refusal, on the
            // draft or in the toast with the text kept, and a send from the draft asks again.
            answer = { code: "door.unreachable", channelGuid: null, isPending: false, message: null, personAliasGuid: null };
        }
        finally {
            startsInFlight--;
        }

        const channelId = answer.code === "ok" && answer.channelGuid ? answer.channelGuid.toLowerCase() : null;
        const hasArrived = channelId !== null && arrived.has(channelId);
        if (startsInFlight === 0) {
            arrived.clear();
        }

        // The shell is going away, so there is nobody left to tell or to keep the text for.
        if (isStopped) {
            return false;
        }

        // A refusal is Rock's answer and no wait changes it. A person still on the draft is told
        // there; one who left it is told in the toast, and the text waits for their next draft.
        if (!channelId) {
            message.phase = "refused";
            const reason = answer.message ?? "The conversation could not be started. Try again.";
            if (isOnScreen(message)) {
                draft.status = "draft";
                state.error = reason;
            }
            else {
                reportLeft(message, { code: "door.first_message_refused", severity: "permission" }, reason);
            }
            return false;
        }

        message.channelId = channelId;
        draft.channelGuid = channelId;

        // Not held yet: a send now would be refused. The membership signal says when it is,
        // unless it came while Rock answered; the waits try anyway, since a signal can be lost.
        if (answer.isPending && !hasArrived) {
            void waitForConversation(message);
            return false;
        }

        // Held, so it opens first and the row shows sending in it. An open refused all the same
        // means the platform does not hold it for this person yet, so it is waited for like one
        // Rock answered pending, rather than sent into a conversation that is not on screen.
        if (isOnScreen(message)) {
            const isShown = await showConversation(message);
            if (isStopped) {
                return false;
            }
            if (!isShown && isOnScreen(message)) {
                void waitForConversation(message);
                return false;
            }
        }

        return sendOnce(message);
    }

    return Object.assign(state, {
        search: async (query: string): Promise<PersonRow[]> => {
            const result = await dependencies.search(query);
            if (!result.ok) {
                state.error = result.error.text ?? "People could not be searched. Try again.";
                return [];
            }

            return result.people;
        },

        choose: (person: PersonRow): boolean => {
            if (state.chosen.some(p => p.person_alias_guid === person.person_alias_guid)) {
                return true;
            }
            if (state.chosen.length >= maxOthers) {
                return false;
            }

            state.chosen.push(person);
            return true;
        },

        unchoose: (personAliasGuid: string): void => {
            state.chosen = state.chosen.filter(p => p.person_alias_guid !== personAliasGuid);
        },

        open: async (): Promise<void> => {
            const people = [...state.chosen];
            if (people.length === 0) {
                return;
            }

            state.error = null;
            const result = await dependencies.findConversation(people.map(p => p.person_alias_guid));
            if (!result.ok) {
                state.error = result.error.text ?? "The conversation could not be opened. Try again.";
                return;
            }

            state.chosen = [];
            leaveDraft();
            state.held = null;

            // Kept messages for these people are taken either way, into their new draft or the
            // conversation they share now. One posted since it was kept needs nothing more.
            const key = peopleKey(people);
            const keptMessages = (kept.get(key) ?? []).filter(m => m.phase !== "sent");
            kept.delete(key);
            const refusedText = keptMessages.filter(m => m.phase === "refused").map(m => m.body).join("\n\n");
            const failed = keptMessages.filter(m => m.phase !== "refused");

            // Asking Rock now would create a conversation nobody has written in, so people with
            // none get a draft and Rock is asked only once there is a message to send.
            if (result.channelId) {
                const isOpened = await dependencies.openChannel(result.channelId);
                if (isStopped) {
                    return;
                }

                // A failed message is a row in that conversation with its Retry already; refused
                // text has no row anywhere, so it goes in the box. An open that did not happen
                // keeps everything for the next try.
                if (!isOpened) {
                    keptMessages.forEach(keep);
                }
                else if (refusedText !== "") {
                    state.held = { body: refusedText };
                }
                return;
            }

            // Refused text needs the box, so the draft's own message is a new one carrying it and
            // every failed message is tried first. Otherwise the newest failed message is the
            // draft's own, its text in the box and its row and key the retry, and older ones go
            // first.
            const carried = refusedText === "" ? failed[failed.length - 1] : undefined;
            const queued = failed.filter(m => m !== carried);

            state.draft = { people, body: carried?.body ?? refusedText, status: "draft", channelGuid: carried?.channelId ?? null };
            if (carried) {
                carry(state.draft, carried);
            }
            if (queued.length > 0) {
                queuedOn.set(toRaw(state.draft), queued);
            }
        },

        sendFirst: async (body: string): Promise<boolean> => {
            const draft = state.draft;

            // A draft already starting has its message; a second send would ask Rock twice.
            if (!draft || draft.status === "starting" || body.trim() === "") {
                return false;
            }

            draft.body = body;
            draft.status = "starting";
            state.error = null;

            // Failed messages kept with this draft are older than its own, so they go first. Only
            // awaited when there are some, so an ordinary first message asks Rock at once.
            if (queuedOn.get(toRaw(draft))?.length && !await retryQueued(draft)) {
                return false;
            }

            // A failed first message's draft is its retry: the same row and key to the
            // conversation Rock already made, so it posts once and Rock is not asked again.
            const carried = carriedBy.get(toRaw(draft));
            if (carried?.phase === "failed") {
                carried.body = body;
                return sendOnce(carried);
            }

            const message: FirstMessage = {
                phase: "asking",
                people: draft.people,
                body,
                localId: newLocalId(),
                channelId: null,
                tries: 0,
                draft: null,
                isShown: false
            };
            carry(draft, message);

            return start(draft, message);
        },

        edit: (body: string): void => {
            if (state.draft) {
                state.draft.body = body;
            }
        },

        membershipChanged: async (channelId?: string | null): Promise<void> => {
            const changed = channelId?.toLowerCase() ?? null;

            if (changed && startsInFlight > 0) {
                arrived.add(changed);
            }

            // A signal for another conversation says nothing about a waiting one.
            const ready = [...waiting].filter(m => !changed || changed === m.channelId);
            await Promise.all(ready.map(m => tryWaiting(m, true)));
        },

        close: (): void => {
            leaveDraft();
            state.chosen = [];
            state.error = null;
        },

        stop: (): void => {
            isStopped = true;
            waiting.clear();
        }
    });
}
