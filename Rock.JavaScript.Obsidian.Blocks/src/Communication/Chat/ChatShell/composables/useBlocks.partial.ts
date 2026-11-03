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
// The people the person blocked: the set of their aliases every live event is read through, the
// list Chat settings draws, and block and unblock with the one sentence that confirms a block.
//
// The platform hides a blocked person's messages in every page it answers, but a room's live
// events are one payload for every subscriber, so the client hides them itself: a message from
// someone in the set arrives collapsed, and their face is blanked where they reacted or replied.
// The set comes with every sidebar load and with every block, unblock and list answer, so another
// device's change, told as blocks.changed on the person's own topic, is read the same way. When
// the set changes, what is on screen was read through the old one, so the shell reads it again.
// The blocked person is never told, and nothing here is ever sent to them.
/* eslint-disable @typescript-eslint/naming-convention */
import { reactive } from "vue";
import { failureWords, messageForError } from "../errors.partial";
import { CallResult } from "../platformCall.partial";
import { ChatError, ChatPerson, ChipReactor, MessageCreatedEvent, QuotedMessage, Replier } from "../types.partial";
import { personName } from "../ui/face.partial";
import { Note } from "../ui/settingNote.partial.obs";
import { blockedCopy } from "./useHistory.partial";
import { ActionNotice } from "./useMessageActions.partial";
import { failureNote } from "./saveRule.partial";

/** What block, unblock and the list answer: the people, and every alias of theirs to hide. */
export type BlocksAnswer = {
    people: ChatPerson[];
    blocked_aliases: string[];
};

/** The sentence the block's confirmation says, wherever it was asked. */
export const blockSentence = "They won't be told. You won't see their messages, and they can't message you.";

/** A person on the list the church no longer lists, who has no name to show. */
export const unnamedBlocked = "Someone no longer in chat";

/** What the blocks hold for the screen. */
export type BlocksState = {
    /** Every alias of every person blocked, in lower case. */
    aliases: string[];

    /** The list Chat settings draws, once read. */
    people: ChatPerson[];

    /** Where the list has got to: not asked, on its way, read, or failed. */
    listState: "idle" | "loading" | "loaded" | "failed";

    /** The person the block's confirmation is open for. */
    confirming: ChatPerson | null;

    /** The people whose block or unblock is out, by alias guid in lower case. */
    busy: Record<string, boolean>;

    /** A failed unblock's note on the list, by alias guid in lower case. */
    notes: Record<string, Note>;
};

/** What the blocks reach outside themselves. */
export type BlocksDependencies = {
    /** The shell's one call path. */
    call: (name: string, args: Record<string, unknown>) => Promise<CallResult>;

    /** The set changed, so what is on screen, read through the old one, is read again. */
    onChanged: () => void;

    /** Shows a toast, for a block or unblock asked from a card or a message. */
    notify: (notice: ActionNotice) => void;
};

/** The blocks a shell holds. */
export type Blocks = {
    state: BlocksState;

    /** The set as an answer names it: the bootstrap's me, or a block, unblock or list answer. */
    setAliases: (aliases: unknown) => void;

    /** Whether a message by this alias is hidden from the person. */
    isBlocked: (personAliasGuid: string | null | undefined) => boolean;

    /** A room's live event as the person may see it: a blocked sender's message collapsed, their face blank. */
    maskEvent: (event: string, payload: unknown) => unknown;

    /** Reads the list Chat settings draws. */
    load: () => Promise<void>;

    /** Opens the block's confirmation for a person. */
    ask: (person: ChatPerson) => void;

    /** Closes the confirmation, blocking no one. */
    cancel: () => void;

    /** Blocks the person the confirmation is open for; true once the platform took it. */
    confirm: () => Promise<boolean>;

    /** Unblocks a person, with no confirmation, as everything comes back as it was; true once taken. */
    unblock: (person: ChatPerson, isFromList: boolean) => Promise<boolean>;

    /** blocks.changed: another page or device changed the set, so it is read again. */
    changed: () => Promise<void>;

    /** The session ended: nothing more is asked, and no answer reaches the screen. */
    stop: () => void;
};

/** The live events that can carry a blocked person's words or face. */
const maskedEvents = new Set(["message.created", "reaction.added", "reaction.removed", "thread.updated"]);

/**
 * A face blanked when it is someone the person blocked: the act stays counted, but whose it was
 * is not shown, as the platform blanks them on every list it answers the person.
 *
 * @param person The face as the payload carried it.
 * @param isBlocked Whether an alias is blocked.
 *
 * @returns The face, or the same with no name or photo.
 */
function blankFace<T extends { nick_name: string | null, last_name: string | null, avatar_url: string | null }>(person: T, isBlocked: boolean): T {
    return isBlocked ? { ...person, nick_name: null, last_name: null, avatar_url: null } : person;
}

/**
 * A quote of someone the person blocked, withheld as the platform withholds it on a page.
 *
 * @param quoted The quote as the payload carried it.
 *
 * @returns The quote with nothing of what it says and no name.
 */
function withheldQuote(quoted: QuotedMessage): QuotedMessage {
    return { ...quoted, sender_nick_name: null, sender_last_name: null, sender_avatar_url: null, body: null, attachments: null, pieces: null, blocked: true };
}

/**
 * Creates the blocks.
 *
 * @param dependencies The call path, what the shell reads again, and the toast.
 *
 * @returns The blocks.
 */
export function createBlocks(dependencies: BlocksDependencies): Blocks {
    const state = reactive<BlocksState>({ aliases: [], people: [], listState: "idle", confirming: null, busy: {}, notes: {} }) as BlocksState;
    let set = new Set<string>();
    let isStopped = false;

    // Until the first set is known, a live event is let through as it came, since nobody is known
    // to be blocked. The first set then changes what is on screen only if such an event was held
    // and the set names someone; after it, any change does.
    let hasSet = false;
    let isHeldUnmasked = false;

    // Each list read and each block or unblock has a number; only the newest answer sets the set,
    // so a slow answer never takes back a newer one. The order is the asking's, not the platform's:
    // a list read asked after a block but read before it committed can win, and the block's own
    // blocks.changed then reads the list again, so no stale set stays.
    let asked = 0;
    let applied = 0;

    function isBlocked(personAliasGuid: string | null | undefined): boolean {
        return !!personAliasGuid && set.has(personAliasGuid.toLowerCase());
    }

    /** Takes a set; a change asks the shell to read again what it shows. */
    function setAliases(aliases: unknown): void {
        if (isStopped || !Array.isArray(aliases)) {
            return;
        }

        const next = new Set(aliases.filter((a): a is string => typeof a === "string").map(a => a.toLowerCase()));
        const isChanged = hasSet
            ? next.size !== set.size || [...next].some(a => !set.has(a))
            : isHeldUnmasked && next.size > 0;
        hasSet = true;
        set = next;
        state.aliases = [...next];
        if (isChanged) {
            dependencies.onChanged();
        }
    }

    /** Takes an answer, unless a newer ask has already answered. */
    function take(number: number, data: unknown): void {
        if (number < applied) {
            return;
        }
        applied = number;
        const answer = data as Partial<BlocksAnswer> | null;
        if (Array.isArray(answer?.people)) {
            state.people = answer.people;
            state.listState = "loaded";
        }
        setAliases(answer?.blocked_aliases);
    }

    function key(person: ChatPerson): string {
        return person.person_alias_guid.toLowerCase();
    }

    async function load(): Promise<void> {
        if (isStopped) {
            return;
        }
        const number = ++asked;
        if (state.listState !== "loaded") {
            state.listState = "loading";
        }
        const result = await dependencies.call("chat_list_blocks", {});
        if (isStopped) {
            return;
        }
        if (result.ok) {
            take(number, result.data);
        }
        else if (number >= applied && state.listState !== "loaded") {
            state.listState = "failed";
        }
    }

    /** Blocks or unblocks a person; a refusal is told beside the list's row or in a toast. */
    async function change(name: "chat_block_person" | "chat_unblock_person", person: ChatPerson, isFromList: boolean): Promise<boolean> {
        if (isStopped || state.busy[key(person)]) {
            return false;
        }
        const number = ++asked;
        state.busy[key(person)] = true;
        delete state.notes[key(person)];
        const result = await dependencies.call(name, { p_person_alias_guid: person.person_alias_guid });
        delete state.busy[key(person)];
        if (isStopped) {
            return false;
        }
        if (result.ok) {
            take(number, result.data);
            // the list's row says so itself; a card or a message has closed, so a toast tells it
            if (!isFromList) {
                const named = personName(person) || "this person";
                dependencies.notify(name === "chat_block_person"
                    ? { code: "blocked", text: `Blocked ${named}`, icon: "ban" }
                    : { code: "unblocked", text: `Unblocked ${named}`, icon: "check" });
            }
            return true;
        }

        refused(result.error, person, name, isFromList);
        return false;
    }

    function refused(error: ChatError, person: ChatPerson, name: "chat_block_person" | "chat_unblock_person", isFromList: boolean): void {
        if (isFromList) {
            state.notes[key(person)] = failureNote(failureWords(error), () => void change(name, person, true));
            return;
        }
        dependencies.notify({ code: error.code, text: messageForError(error), icon: "alert" });
    }

    return {
        state,
        setAliases,
        isBlocked,

        maskEvent: (event: string, payload: unknown): unknown => {
            if (!hasSet && maskedEvents.has(event)) {
                isHeldUnmasked = true;
            }
            if (set.size === 0 || !payload || typeof payload !== "object") {
                return payload;
            }

            if (event === "message.created") {
                const created = payload as MessageCreatedEvent;
                if (created.message_type !== "system" && isBlocked(created.person_alias_guid)) {
                    return blockedCopy(created);
                }
                if (created.quoted && isBlocked(created.quoted.person_alias_guid)) {
                    return { ...created, quoted: withheldQuote(created.quoted) };
                }
                return payload;
            }

            if (event === "reaction.added" || event === "reaction.removed") {
                const reaction = payload as { reactors?: ChipReactor[] };
                return Array.isArray(reaction.reactors)
                    ? { ...reaction, reactors: reaction.reactors.map(r => blankFace(r, isBlocked(r.reactor_person_alias_guid) || isBlocked(r.person_alias_guid))) }
                    : payload;
            }

            if (event === "thread.updated") {
                const thread = payload as { repliers?: Replier[] | null };
                return Array.isArray(thread.repliers)
                    ? { ...thread, repliers: thread.repliers.map(r => blankFace(r, isBlocked(r.person_alias_guid))) }
                    : payload;
            }

            return payload;
        },

        load,

        ask: (person: ChatPerson): void => {
            if (!isStopped) {
                state.confirming = person;
            }
        },

        cancel: (): void => {
            state.confirming = null;
        },

        confirm: async (): Promise<boolean> => {
            const person = state.confirming;
            state.confirming = null;
            return person ? change("chat_block_person", person, false) : false;
        },

        unblock: (person: ChatPerson, isFromList: boolean): Promise<boolean> => change("chat_unblock_person", person, isFromList),

        changed: load,

        stop: (): void => {
            isStopped = true;
            state.confirming = null;
        }
    };
}
