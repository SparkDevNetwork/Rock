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
// one with somebody Rock would not let them reach.
import { reactive } from "vue";
import { ChatError, PersonRow } from "../types.partial";

export type { PersonRow };

/** The most people a person can start a conversation with, besides themselves. */
export const maxOthers = 8;

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

    /** The ordinary send. Resolves true when the platform confirmed the message. */
    send: (channelId: string, body: string) => Promise<boolean>;
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

    /** Closes the draft and forgets the picked people. */
    close: () => void;
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

    /** Conversations the person was added to while Rock had not answered yet. */
    const arrived = new Set<string>();

    /** Opens the conversation Rock made and sends the draft's message into it. */
    async function finish(channelId: string, body: string): Promise<boolean> {
        // Out of the draft first, so a second signal cannot send it twice.
        state.draft = null;
        arrived.clear();
        await dependencies.openChannel(channelId);

        return dependencies.send(channelId, body);
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

            const answer = await dependencies.startConversation(draft.people.map(p => p.person_alias_guid));

            // Closed or replaced while Rock answered; nothing is sent into a draft left behind.
            if (state.draft !== draft) {
                return false;
            }

            if (answer.code !== "ok" || !answer.channelGuid) {
                draft.status = "draft";
                state.error = answer.message ?? "The conversation could not be started. Try again.";
                return false;
            }

            const channelId = answer.channelGuid.toLowerCase();

            // The platform has not taken the conversation yet, so a send now would be refused.
            // The person's membership signal says when it has, unless it came while Rock answered.
            if (answer.isPending && !arrived.has(channelId)) {
                draft.channelGuid = channelId;
                return false;
            }

            return finish(channelId, draft.body);
        },

        membershipChanged: async (channelId?: string | null): Promise<void> => {
            const draft = state.draft;
            if (!draft || draft.status !== "starting") {
                return;
            }

            const changed = channelId?.toLowerCase() ?? null;

            if (!draft.channelGuid) {
                if (changed) {
                    arrived.add(changed);
                }
                return;
            }

            // A signal for another conversation says nothing about this one.
            if (changed && changed !== draft.channelGuid) {
                return;
            }

            await finish(draft.channelGuid, draft.body);
        },

        close: (): void => {
            state.draft = null;
            state.chosen = [];
            state.error = null;
            arrived.clear();
        }
    });
}
