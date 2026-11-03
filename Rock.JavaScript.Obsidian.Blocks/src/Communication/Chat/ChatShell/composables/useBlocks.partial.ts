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
// The people the person blocked: the set of their aliases every message is read through, the list
// Chat settings draws, and block and unblock with the one sentence that confirms a block.
//
// The platform hides a blocked person's messages in every page it answers, but a room's live
// events are one payload for every subscriber, and a page read before a block can land after it.
// So the set the client holds is the authority on what is withheld, applied where each message is
// written: every holder writes through mask, which collapses a message from someone in the set and
// blanks their face wherever it rides on someone else's (a quote, a mention, a replier, a reactor).
// A block masks again, in place, everything held, at once and with no read; what the client cannot
// work out itself (the sidebar, a room's facts and composer, counts) is read again, and so is what
// an unblock gives back, whose words the client never held. A copy the platform withheld for
// someone the set does not name is out of date, read before an unblock or after a block from
// another device not yet told; it asks, once for each set, that the list and then the screen be
// read again. The set comes with every sidebar load and with every block, unblock and list answer,
// so another device's change, told as blocks.changed on the person's own topic, is read the same
// way. The blocked person is never told, and nothing here is ever sent to them.
/* eslint-disable @typescript-eslint/naming-convention */
import { reactive } from "vue";
import { failureWords, messageForError } from "../errors.partial";
import { CallResult } from "../platformCall.partial";
import { ChatError, ChatPerson, ChipReactor, DisplayPiece, QuotedMessage, Reaction, Replier, TimelineMessage } from "../types.partial";
import { personName } from "../ui/face.partial";
import { Note } from "../ui/settingNote.partial.obs";
import { blockedCopy, isWithheld, MessageMask } from "./useHistory.partial";
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

    /**
     * The set changed, or a withheld copy showed it out of date, so what the client cannot mask
     * itself is read again: all of it, or, when the change came in a sidebar answer, all but the
     * sidebar, which that answer is. The aliases no longer blocked are named, since only a read
     * gives back what was withheld for them.
     */
    onChanged: (isInSidebarAnswer: boolean, removed: readonly string[]) => void;

    /** The set gained these aliases: every holder masks again, in place, what it holds. */
    remask: (added: readonly string[]) => void;

    /** Shows a toast, for a block or unblock asked from a card or a message. */
    notify: (notice: ActionNotice) => void;
};

/** The blocks a shell holds. */
export type Blocks = {
    state: BlocksState;

    /** The set as the sidebar's answer names it, in the bootstrap's me. */
    setAliases: (aliases: unknown) => void;

    /** Whether a message by this alias is hidden from the person. */
    isBlocked: (personAliasGuid: string | null | undefined) => boolean;

    /** A message as the person may see it, written by every holder; the same object when nothing applies. */
    mask: MessageMask;

    /** A room's live event as the person may see it: a blocked sender's message collapsed, their face blank. */
    maskEvent: (event: string, payload: unknown) => unknown;

    /** People who reacted, as the person may see them: the face of someone blocked blank. */
    maskFaces: <T extends CarriedFace & { reactor_person_alias_guid?: string }>(people: T[]) => T[];

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

/** A face as a message carries one: a replier, a reactor, or a mention chip. */
export type CarriedFace = { person_alias_guid?: unknown, nick_name?: unknown, last_name?: unknown, avatar_url?: unknown };

/**
 * A face blanked when it is someone the person blocked: the act stays counted, but whose it was
 * is not shown, as the platform blanks them on every list it answers the person. One already
 * blank is the same object, so nothing is written again.
 *
 * @param person The face as it was carried.
 * @param isBlocked Whether the face is someone blocked.
 *
 * @returns The face, or the same with no name or photo.
 */
function blankFace<T extends object>(person: T, isBlocked: boolean): T {
    const face = person as CarriedFace;
    const isShown = face.nick_name != null || face.last_name != null || face.avatar_url != null;
    return isBlocked && isShown ? { ...person, nick_name: null, last_name: null, avatar_url: null } : person;
}

/**
 * Each of a list blanked by blankFace, the same list when none changed.
 *
 * @param list The faces, or whatever the field held.
 * @param blank Blanks one face.
 *
 * @returns The list, or a new one with the faces blanked.
 */
function blankEach<T>(list: T[] | null | undefined, blank: (item: T) => T): T[] | null | undefined {
    if (!Array.isArray(list)) {
        return list;
    }
    // a copy only from the first face changed, so a list with nothing to blank allocates nothing
    let blanked: T[] | null = null;
    for (let i = 0; i < list.length; i++) {
        const item = blank(list[i]);
        if (item !== list[i] && !blanked) {
            blanked = list.slice(0, i);
        }
        blanked?.push(item);
    }
    return blanked ?? list;
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

    // Until the first set is known, a message is written as it came, since nobody is known to be
    // blocked; the first set masks in place whatever was let through.
    let hasSet = false;

    // Every change of the set has a number, and a heal is asked at most once for each, so a
    // platform that keeps withholding an alias the set never names asks no more than once.
    let setNumber = 0;
    let healedNumber = -1;
    let isHealQueued = false;

    // The aliases of copies the platform withheld that the set does not name, waiting for the heal.
    const stale = new Set<string>();

    // While what is held is masked again: a held copy still withheld for someone just unblocked is
    // being read again by that unblock, so it asks no heal of its own.
    let isRemasking = false;

    // Each list read and each block or unblock has a number; only the newest answer sets the set,
    // so a slow answer never takes back a newer one. The order is the asking's, not the platform's:
    // a list read asked after a block but read before it committed can win, and the block's own
    // blocks.changed then reads the list again, so no stale set stays.
    let asked = 0;
    let applied = 0;

    // Every alias already looked up and not in the set, as written, so masking again every row
    // held lowers no string twice; emptied whenever the set changes.
    let notBlocked = new Set<string>();

    function isBlocked(personAliasGuid: unknown): boolean {
        if (typeof personAliasGuid !== "string" || set.size === 0 || notBlocked.has(personAliasGuid)) {
            return false;
        }
        if (set.has(personAliasGuid) || set.has(personAliasGuid.toLowerCase())) {
            return true;
        }
        notBlocked.add(personAliasGuid);
        return false;
    }

    /**
     * Takes a set. Someone added is masked at once, in place, before anything else runs, so their
     * words go with no read; any change then asks the shell to read again what it cannot mask,
     * saying where the set came from and who was let go. The first set masks what was let through
     * before it, and reads nothing, since every page was already read through it.
     */
    function takeSet(aliases: unknown, isInSidebarAnswer: boolean): void {
        if (isStopped || !Array.isArray(aliases)) {
            return;
        }

        const next = new Set(aliases.filter((a): a is string => typeof a === "string").map(a => a.toLowerCase()));
        const removed = [...set].filter(alias => !next.has(alias));
        const added = [...next].filter(alias => !set.has(alias));
        const isFirst = !hasSet;
        if (!isFirst && added.length === 0 && removed.length === 0) {
            return;
        }

        hasSet = true;
        set = next;
        notBlocked = new Set();
        setNumber++;
        state.aliases = [...next];
        if (added.length > 0) {
            isRemasking = true;
            try {
                dependencies.remask(added);
            }
            finally {
                isRemasking = false;
            }
        }

        if (isFirst) {
            // a copy withheld before the set was known heals now if the set does not name it
            for (const alias of stale) {
                if (next.has(alias)) {
                    stale.delete(alias);
                }
            }
            queueHeal();
            return;
        }
        // this change reads again what is on screen, so whatever was waiting to heal is in it
        stale.clear();
        dependencies.onChanged(isInSidebarAnswer, removed);
    }

    /** A copy withheld for an alias the set does not name: noted, and healed once the write is done. */
    function noteWithheld(personAliasGuid: unknown): void {
        if (isRemasking || isStopped || typeof personAliasGuid !== "string" || isBlocked(personAliasGuid)) {
            return;
        }
        stale.add(personAliasGuid.toLowerCase());
        if (hasSet) {
            queueHeal();
        }
    }

    /** Asks the heal after the write that met the copy, so no read starts inside a holder's write. */
    function queueHeal(): void {
        if (isHealQueued || stale.size === 0 || healedNumber === setNumber) {
            return;
        }
        isHealQueued = true;
        queueMicrotask(() => void heal());
    }

    /**
     * Reads the list first: when it changes the set, that change is the heal. When it still does
     * not name them, the copies are out of date, read before an unblock, and what is held for
     * those aliases is read again as an unblock reads it.
     */
    async function heal(): Promise<void> {
        isHealQueued = false;
        if (isStopped || healedNumber === setNumber) {
            return;
        }
        const number = setNumber;
        healedNumber = number;
        await load();
        if (isStopped || setNumber !== number) {
            return;
        }
        const aliases = [...stale].filter(alias => !set.has(alias));
        stale.clear();
        if (aliases.length > 0) {
            dependencies.onChanged(false, aliases);
        }
    }

    /** A quote as the person may see it: withheld for someone blocked, its mention chips' faces blank. */
    function maskQuote(quoted: QuotedMessage): QuotedMessage {
        if (quoted.blocked && !isBlocked(quoted.person_alias_guid)) {
            noteWithheld(quoted.person_alias_guid);
        }
        if (quoted.blocked || isBlocked(quoted.person_alias_guid)) {
            const isAlreadyWithheld = quoted.blocked === true && [quoted.sender_nick_name, quoted.sender_last_name, quoted.sender_avatar_url, quoted.body, quoted.attachments, quoted.pieces]
                .every(field => field === null || field === undefined);
            return isAlreadyWithheld ? quoted : withheldQuote(quoted);
        }
        const pieces = blankEach(quoted.pieces, maskPiece);
        return pieces === quoted.pieces ? quoted : { ...quoted, pieces };
    }

    /** A mention chip keeps the name written in the message and loses the face of someone blocked, as the platform answers it. */
    function maskPiece(piece: DisplayPiece): DisplayPiece {
        return piece.type === "mention" ? blankFace(piece, isBlocked(piece.person_alias_guid)) : piece;
    }

    function maskReactor<T extends CarriedFace & { reactor_person_alias_guid?: string }>(reactor: T): T {
        return blankFace(reactor, isBlocked(reactor.reactor_person_alias_guid) || isBlocked(reactor.person_alias_guid));
    }

    function maskReaction(reaction: Reaction): Reaction {
        const reactors = blankEach(reaction.reactors, maskReactor);
        return reactors === reaction.reactors ? reaction : { ...reaction, reactors: reactors ?? undefined };
    }

    /** Whether a face of someone blocked is shown on any of these, read with no copy made. */
    function isAnyFaceShown(faces: readonly object[] | null | undefined, kind?: "mention"): boolean {
        if (!Array.isArray(faces)) {
            return false;
        }
        for (const item of faces) {
            const face = item as CarriedFace & { reactor_person_alias_guid?: unknown, type?: unknown };
            const isTheirs = isBlocked(face.person_alias_guid) || isBlocked(face.reactor_person_alias_guid);
            if (isTheirs && (kind === undefined || face.type === kind) && (face.nick_name != null || face.last_name != null || face.avatar_url != null)) {
                return true;
            }
        }
        return false;
    }

    /**
     * Whether the mask has anything to look at in a message: a mark of the platform's, or someone
     * blocked who wrote it, wrote its quote or shows a face on it. Most messages have none, and
     * this answers that with no copy made, so masking again every row held stays a quick walk.
     */
    function isMaskable(message: Partial<TimelineMessage>): boolean {
        const quoted = message.quoted;
        if (message.blocked || quoted?.blocked) {
            return true;
        }
        if (set.size === 0) {
            return false;
        }
        if (isBlocked(message.person_alias_guid) || (quoted && (isBlocked(quoted.person_alias_guid) || isAnyFaceShown(quoted.pieces, "mention")))) {
            return true;
        }
        if (isAnyFaceShown(message.pieces, "mention") || isAnyFaceShown(message.repliers)) {
            return true;
        }
        for (const reaction of message.reactions ?? []) {
            if (isAnyFaceShown(reaction.reactors)) {
                return true;
            }
        }
        return false;
    }

    /**
     * A message as the person may see it under the set as it stands now: by someone blocked, or
     * withheld by the platform, it is withheld; otherwise the faces of the blocked that ride on it
     * are blank. The same object when nothing applies, so a row already right is never written.
     */
    function mask<T extends Partial<TimelineMessage>>(message: T): T {
        if (!isMaskable(message)) {
            return message;
        }
        if (message.blocked && !isBlocked(message.person_alias_guid)) {
            noteWithheld(message.person_alias_guid);
        }

        let shown: T = message;
        const isTheirs = message.message_type !== "system" && (message.blocked === true || isBlocked(message.person_alias_guid));
        if (isTheirs) {
            shown = isWithheld(message) ? message : blockedCopy(message);
        }
        else {
            const quoted = message.quoted ? maskQuote(message.quoted) : message.quoted;
            const pieces = blankEach(message.pieces, maskPiece);
            if (quoted !== message.quoted || pieces !== message.pieces) {
                shown = { ...shown, quoted, pieces } as T;
            }
        }

        // the faces of the blocked among its repliers and reactors, on a withheld message too, whose counts stay
        const repliers = blankEach(shown.repliers, (r: Replier) => blankFace(r, isBlocked(r.person_alias_guid)));
        const reactions = blankEach(shown.reactions, maskReaction);
        if (repliers !== shown.repliers || reactions !== shown.reactions) {
            shown = { ...shown, repliers, reactions: reactions ?? undefined } as T;
        }
        return shown;
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
        takeSet(answer?.blocked_aliases, false);
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
        setAliases: aliases => takeSet(aliases, true),
        isBlocked,
        mask,

        // The one entry every live consumer sits behind, so a message is masked before anything
        // holds it; each holder masks again where it writes, which costs nothing on a masked copy.
        maskEvent: (event: string, payload: unknown): unknown => {
            if (set.size === 0 || !payload || typeof payload !== "object") {
                return payload;
            }

            if (event === "message.created") {
                return mask(payload as Partial<TimelineMessage>);
            }

            if (event === "reaction.added" || event === "reaction.removed") {
                const reaction = payload as { reactors?: ChipReactor[] };
                const reactors = blankEach(reaction.reactors, maskReactor);
                return reactors === reaction.reactors ? payload : { ...reaction, reactors };
            }

            if (event === "thread.updated") {
                const thread = payload as { repliers?: Replier[] | null };
                const repliers = blankEach(thread.repliers, (r: Replier) => blankFace(r, isBlocked(r.person_alias_guid)));
                return repliers === thread.repliers ? payload : { ...thread, repliers };
            }

            return payload;
        },

        maskFaces: people => blankEach(people, maskReactor) ?? people,

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
