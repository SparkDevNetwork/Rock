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
// The profile: the person a card or the profile panel shows, read for the room it was opened
// from, keeping only the newest card's answer. Rock's half of the card, the email, phone and
// template it alone holds, is asked beside the platform's person; a person who shows details is
// drawn once both have answered or a short wait has passed, so the card rarely grows after it
// appears, and each person's answer is kept for the visit. A member's Message reads the same
// person, for their messaging rights. Once stopped, nothing more is asked and no answer still on
// its way reaches the screen.
import { reactive } from "vue";
import { ChatProfileDetailsBag } from "@Obsidian/ViewModels/Blocks/Communication/Chat/ChatShell/chatProfileDetailsBag";
import { ChatError, PersonProfile } from "../types.partial";

/** The profile card's person, or the refusal; a person the platform does not list is a refusal. */
export type PersonProfileResult = { ok: true, person: PersonProfile } | { ok: false, error: ChatError };

/** Rock's half of a profile card: asked, answered, or not to be had. */
export type ProfileRockState =
    | { kind: "loading" }
    | { kind: "answered", details: ChatProfileDetailsBag }
    | { kind: "failed" };

/** The open profile card: whom it is for, the room it was opened from, the person once read, and Rock's half. */
export type ProfileCardState = {
    personAliasGuid: string;
    channelId: string;
    person: PersonProfile | null;
    rock: ProfileRockState;
};

/**
 * How long a card for someone showing details waits for Rock's half after it is opened, in ms,
 * so the card usually appears whole; past it the card draws with Rock's rows reserved. Short
 * enough that the wait is not felt beside the card's own entrance.
 */
export const profileDetailsWait = 150;

/** What the profile holds for the screen. */
export type ProfileState = {
    /** The profile card open now, or null; drawn only once its person is read. */
    card: ProfileCardState | null;
};

/** What the profile reaches outside itself. */
export type ProfileDependencies = {
    /** Reads one person for the profile card, chat_get_person through the shell's call path. */
    getPerson: (personAliasGuid: string, channelId: string) => Promise<PersonProfileResult>;

    /** Rock's half of a profile card, or null when Rock gave no answer; without it, the card has none. */
    getProfileDetails?: (personAliasGuid: string) => Promise<ChatProfileDetailsBag | null>;
};

/** The profile a shell holds. */
export type Profile = {
    state: ProfileState;

    /** Opens the profile card for a person, read for the room it was opened from. */
    openCard: (personAliasGuid: string, channelId: string) => void;

    /** Reads a person's current profile and the conversation they already share with the reader. */
    readPerson: (personAliasGuid: string, channelId: string) => Promise<PersonProfile | null>;

    /** Closes the profile card; an answer still on its way is dropped. */
    closeCard: () => void;

    /** Reads the open card again, from the platform and from Rock, once a change to the person's own switch has been saved. */
    refreshCardDetails: (saved: Promise<unknown>) => void;

    stop: () => void;
};

/**
 * Creates the profile.
 *
 * @param dependencies What the profile reaches outside itself.
 *
 * @returns The profile.
 */
export function createProfile(dependencies: ProfileDependencies): Profile {
    const state = reactive<ProfileState>({ card: null }) as ProfileState;

    let isStopped = false;

    // Each card opened or closed moves this on, so a person read for a card no longer open never lands.
    let cardGeneration = 0;
    // Each refresh of the open card moves this on, so of quick changes of the person's own switch
    // only the newest refresh's reads land, the platform's and Rock's alike.
    let cardRefresh = 0;
    let cardWaitTimer: ReturnType<typeof setTimeout> | null = null;

    // Rock's answer for each person showing details this visit, by alias in lower case, so a card
    // opened again is whole at once. An answer showing nothing is never kept: the person may turn
    // their details on, and a refusal for reading too many people in a minute looks the same.
    const rockAnswers = new Map<string, ChatProfileDetailsBag>();

    // Each ask of Rock moves this on, so only the newest is drawn or kept: after quick changes of
    // the person's own switch, an older answer arriving last would show what they no longer chose.
    let rockAsk = 0;

    /** The card and a member's Message share the platform's current profile and messaging rights. */
    async function readPerson(personAliasGuid: string, channelId: string): Promise<PersonProfile | null> {
        if (isStopped) {
            return null;
        }

        const result = await dependencies.getPerson(personAliasGuid, channelId)
            .catch((): PersonProfileResult => ({ ok: false, error: { code: "rpc.transport", severity: "failed" } }));
        return !isStopped && result.ok ? result.person : null;
    }

    /**
     * Rock's half for a person, asked of its door: null when the door is absent, failed or gave
     * nothing, and undefined when a newer ask has started since, whose answer is the one that counts.
     */
    async function askRock(personAliasGuid: string): Promise<ChatProfileDetailsBag | null | undefined> {
        const door = dependencies.getProfileDetails;
        if (!door) {
            return null;
        }

        const ask = ++rockAsk;
        const details = await door(personAliasGuid).catch(() => null);
        if (ask !== rockAsk) {
            return undefined;
        }

        if (details?.isShown) {
            rockAnswers.set(personAliasGuid.toLowerCase(), details);
        }

        return details;
    }

    function cancelCardWait(): void {
        if (cardWaitTimer !== null) {
            clearTimeout(cardWaitTimer);
            cardWaitTimer = null;
        }
    }

    return {
        state,

        openCard: (personAliasGuid: string, channelId: string): void => {
            if (isStopped) {
                return;
            }

            cancelCardWait();
            const generation = ++cardGeneration;
            const kept = rockAnswers.get(personAliasGuid.toLowerCase());
            const card: ProfileCardState = {
                personAliasGuid,
                channelId,
                person: null,
                rock: kept ? { kind: "answered", details: kept } : dependencies.getProfileDetails ? { kind: "loading" } : { kind: "failed" }
            };
            state.card = card;

            let read: PersonProfile | null | undefined;
            let isWaitOver = false;

            /**
             * Draws the person once read: at once when Rock has answered or there is nothing of
             * Rock's to wait for, else once Rock answers or the wait from the open is over.
             */
            const draw = (): void => {
                if (isStopped || generation !== cardGeneration || read === undefined) {
                    return;
                }

                // A person the platform does not list, or a read that failed, draws no card: the
                // opener stays as it was with nothing to show.
                if (!read) {
                    cancelCardWait();
                    state.card = null;
                    return;
                }

                const current = state.card as ProfileCardState;
                if (current.rock.kind === "loading" && read.details !== null && !isWaitOver) {
                    return;
                }

                cancelCardWait();
                state.card = { ...current, person: read };
            };

            if (card.rock.kind === "loading") {
                cardWaitTimer = setTimeout(() => {
                    cardWaitTimer = null;
                    isWaitOver = true;
                    draw();
                }, profileDetailsWait);

                void askRock(personAliasGuid).then(details => {
                    if (details === undefined || isStopped || generation !== cardGeneration || !state.card) {
                        return;
                    }

                    state.card = { ...state.card, rock: details ? { kind: "answered", details } : { kind: "failed" } };
                    draw();
                });
            }

            void readPerson(personAliasGuid, channelId).then(person => {
                read = person;
                draw();
            });
        },

        readPerson,

        closeCard: (): void => {
            cancelCardWait();
            cardGeneration++;
            state.card = null;
        },

        refreshCardDetails: (saved: Promise<unknown>): void => {
            const card = state.card;
            if (isStopped || !card || !dependencies.getProfileDetails) {
                return;
            }

            // What Rock answered before the change no longer holds, here or at the next open.
            rockAnswers.delete(card.personAliasGuid.toLowerCase());
            state.card = { ...card, rock: { kind: "loading" } };

            const generation = cardGeneration;
            const refresh = ++cardRefresh;
            const isCurrent = (): boolean => !isStopped && generation === cardGeneration && refresh === cardRefresh && !!state.card;
            void saved.catch(() => undefined).then(() => {
                if (!isCurrent()) {
                    return;
                }

                // The platform is read again too: the save waits for the change to reach it, and
                // until then its answer, the badges among it, is the one from before the change.
                void readPerson(card.personAliasGuid, card.channelId).then(person => {
                    if (person && isCurrent()) {
                        state.card = { ...state.card as ProfileCardState, person };
                    }
                });

                void askRock(card.personAliasGuid).then(details => {
                    if (details !== undefined && isCurrent()) {
                        state.card = { ...state.card as ProfileCardState, rock: details ? { kind: "answered", details } : { kind: "failed" } };
                    }
                });
            });
        },

        stop: (): void => {
            isStopped = true;
            cardGeneration++;
            cancelCardWait();
        }
    };
}
