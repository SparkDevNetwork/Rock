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
// page: nothing exists anywhere until its first message, which asks Rock for the conversation,
// opens it and sends. Rock is the only one that creates a conversation, so a person cannot start
// one with somebody Rock would not let them reach. Once Rock has made it the message is the
// person's, so it is sent even if they leave the draft, and a platform slow to take the
// conversation is tried again with growing waits a few times before the message is left failed.
import { reactive } from "vue";
import { computeBackoff } from "../platformCall.partial";
import { ChatError, PersonRow } from "../types.partial";
import { defaultRetryDelay } from "./useHistory.partial";
import { newLocalId, SendTry } from "./useSend.partial";

export type { PersonRow };

/** The most people a person can start a conversation with, besides themselves. */
export const maxOthers = 8;

/**
 * How many times a first message is tried after Rock made its conversation and the platform had
 * not taken it. Each wait doubles, so five reach the reconnect cap; past that the platform is not
 * about to take it, and the person decides with the ordinary failed message.
 */
const maxRetriedSends = 5;

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

    /** The first message, kept while Rock answers and after a refusal. */
    body: string;

    /** "starting" from the first send until the message is on its way. */
    status: "draft" | "starting";

    /** The conversation Rock answered, while the platform has not taken it yet. */
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

    /** Opens a channel on screen. */
    openChannel: (channelId: string) => void | Promise<void>;

    /**
     * The ordinary send. Resolves true when the platform confirmed it. A first message tried
     * again on its own passes its try, which names the same row every time.
     */
    send: (channelId: string, body: string, attempt?: SendTry) => Promise<boolean>;

    /** The first wait before a try and the longest; the history's by default. */
    retryDelay?: () => { baseMs: number, capMs: number };

    /** Waits; a timer by default. */
    sleep?: (ms: number) => Promise<void>;

    /** Spreads the waits of many clients apart; Math.random by default. */
    random?: () => number;
};

/** The direct messages a shell holds. */
export type DirectMessages = {
    /** The people picked so far. */
    chosen: PersonRow[];

    /** The draft on screen, or null. */
    draft: DirectMessageDraft | null;

    /** The sentence for the last thing that failed, or null. */
    error: string | null;

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

    /** The person's membership changed; a conversation Rock made may now be on the platform. */
    membershipChanged: (channelId?: string | null) => Promise<void>;

    /** Closes the draft and forgets the picked people. A first message Rock has a conversation for is still sent. */
    close: () => void;

    /** Stops trying first messages again, because the shell is going away. */
    stop: () => void;
};

/** A first message whose conversation Rock made before the platform took it. */
type WaitingMessage = {
    /** The draft it came from, opened on its first try only while it is still on screen. */
    draft: DirectMessageDraft;

    channelId: string;

    body: string;

    /** The row its tries share, so they carry one key and show as one message. */
    localId: string;

    tries: number;

    isSending: boolean;

    /** Sent, or left to the sender as failed. */
    isDone: boolean;
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
        error: null as string | null
    });

    const sleep = dependencies.sleep ?? ((ms: number): Promise<void> => new Promise(resolve => setTimeout(resolve, ms)));
    const random = dependencies.random ?? Math.random;

    /** Conversations the person was added to while Rock had not answered yet. */
    const arrived = new Set<string>();

    /** Asks of Rock still on their way; arrivals are kept only while there is one. */
    let startsInFlight = 0;

    /** First messages waiting for the platform to take their conversation. */
    const waiting = new Set<WaitingMessage>();

    let isStopped = false;

    /**
     * Takes a draft still on screen off it and opens its conversation. A draft the person left
     * is not brought back: they chose to be somewhere else.
     */
    async function leaveDraftFor(draft: DirectMessageDraft, channelId: string): Promise<void> {
        if (state.draft !== draft) {
            return;
        }

        state.draft = null;
        await dependencies.openChannel(channelId);
    }

    /** Sends a waiting message once, unless it is done or a try is already on its way. */
    async function tryWaiting(message: WaitingMessage): Promise<void> {
        if (message.isDone || message.isSending || isStopped) {
            return;
        }

        // Marked before any await, so a signal and a wait ending together send it once.
        message.isSending = true;
        message.tries++;
        const isLast = message.tries >= maxRetriedSends;

        await leaveDraftFor(message.draft, message.channelId);
        const isSent = await dependencies.send(message.channelId, message.body, { localId: message.localId, isLast });
        message.isSending = false;

        // The last failure stays with the sender as an ordinary failed row to retry or discard.
        if (isSent || isLast) {
            message.isDone = true;
            waiting.delete(message);
        }
    }

    /**
     * Tries a waiting message after each of a run of growing waits until it is done. A membership
     * signal tries it sooner, and the wait ending then finds it done.
     */
    async function retryUntilDone(message: WaitingMessage): Promise<void> {
        let attempt = 0;

        while (!message.isDone && !isStopped) {
            const delay = dependencies.retryDelay?.() ?? defaultRetryDelay;
            await sleep(computeBackoff(attempt++, delay.baseMs, delay.capMs, random));
            await tryWaiting(message);
        }
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

            // Asking Rock now would create a conversation nobody has written in, so people with
            // none get a draft and Rock is asked only once there is a message to send.
            if (result.channelId) {
                state.draft = null;
                await dependencies.openChannel(result.channelId);
            }
            else {
                state.draft = { people, body: "", status: "draft", channelGuid: null };
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

            startsInFlight++;
            let answer: DoorResult;
            try {
                answer = await dependencies.startConversation(draft.people.map(p => p.person_alias_guid));
            }
            finally {
                startsInFlight--;
            }

            const channelId = answer.code === "ok" && answer.channelGuid ? answer.channelGuid.toLowerCase() : null;
            const hasArrived = channelId !== null && arrived.has(channelId);
            if (startsInFlight === 0) {
                arrived.clear();
            }

            // A refusal is Rock's answer and no wait changes it. A person still on the draft is
            // told; one who left it before Rock answered has nothing to send into.
            if (!channelId) {
                if (state.draft === draft) {
                    draft.status = "draft";
                    state.error = answer.message ?? "The conversation could not be started. Try again.";
                }
                return false;
            }

            if (isStopped) {
                return false;
            }

            // The platform has not taken the conversation yet, so a send now would be refused.
            // The person's membership signal says when it has, unless it came while Rock
            // answered; the waits try anyway, since a signal can be lost.
            if (answer.isPending && !hasArrived) {
                draft.channelGuid = channelId;
                const message: WaitingMessage = {
                    draft,
                    channelId,
                    body: draft.body,
                    localId: newLocalId(),
                    tries: 0,
                    isSending: false,
                    isDone: false
                };
                waiting.add(message);
                void retryUntilDone(message);
                return false;
            }

            await leaveDraftFor(draft, channelId);
            return dependencies.send(channelId, draft.body);
        },

        membershipChanged: async (channelId?: string | null): Promise<void> => {
            const changed = channelId?.toLowerCase() ?? null;

            if (changed && startsInFlight > 0) {
                arrived.add(changed);
            }

            // A signal for another conversation says nothing about a waiting one.
            const ready = [...waiting].filter(m => !changed || changed === m.channelId);
            await Promise.all(ready.map(tryWaiting));
        },

        close: (): void => {
            state.draft = null;
            state.chosen = [];
            state.error = null;
        },

        stop: (): void => {
            isStopped = true;
            waiting.clear();
        }
    });
}
