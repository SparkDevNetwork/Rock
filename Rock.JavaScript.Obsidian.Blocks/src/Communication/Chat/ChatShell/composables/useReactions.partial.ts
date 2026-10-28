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
// Reacting to a message: the toggle, the live events of everyone reacting, the emoji catalog the
// picker draws, the emoji the person used recently, and the names of who reacted.
//
// A toggle is drawn at once and then replaced by the platform's answer, which is the message's
// whole list as the server counts it. The reader's own reaction reaches them twice, in that answer
// and as the event every viewer hears, so an event under the reader's alias moves the chip only
// when it would change their mark; whichever arrives first, the reaction counts once. Every change
// to the conversation's copy is recorded with the timelines, so a page read before it does not
// take it back, and the open thread's copies change once its own page is in, never waiting for
// the conversation's. Once the reactions are
// stopped nothing new is called and no answer still on its way reaches the screen.
/* eslint-disable @typescript-eslint/naming-convention */
import { reactive } from "vue";
import { messageForError } from "../errors.partial";
import { CallResult } from "../platformCall.partial";
import { CatalogEmoji, EmojiGlyph, Reaction, ReactionAnswer, ReactionEvent, ReactionFavorite, ReactorsPage, TimelineMessage } from "../types.partial";
import { Timelines } from "./useHistory.partial";
import { Threads } from "./useThreads.partial";

export type { CatalogEmoji, EmojiGlyph, Reaction, ReactionFavorite } from "../types.partial";

/** A toast the reactions ask the shell to show. */
export type ReactionNotice = {
    code: string;
    text: string;
    icon: string;
};

/** What the reactions reach outside themselves. */
export type ReactionsDependencies = {
    /** The shell's one call path. */
    call: (name: string, args: Record<string, unknown>) => Promise<CallResult>;

    /** The conversations' messages, whose reactions change in place and are recorded as they do. */
    timelines: Pick<Timelines, "state" | "recordReactions">;

    /**
     * The open thread. A reply shown only in its thread is on no timeline, and the drawer's root
     * is a copy of its own, so both change too.
     */
    threads?: Pick<Threads, "state" | "whenLoaded">;

    /** The session's alias: an event under it is the reader's own. */
    personAliasGuid: string;

    /** Shows a toast. */
    notify: (notice: ReactionNotice) => void;

    /** The bootstrap's favorites, in order. */
    favorites: ReactionFavorite[];

    /** The church, which with the alias keys the recents on the device. */
    churchKey: string;

    /** Where the recents are kept; absent or null where there is none. Either call may throw. */
    storage?: Pick<Storage, "getItem" | "setItem"> | null;
};

/** What the reactions hold. */
export type ReactionsState = {
    /** The church's offered emoji in picker order, once fetched; null until then. */
    catalog: CatalogEmoji[] | null;

    /** The codes the person used recently, newest first. */
    recents: string[];
};

/** The reactions a shell holds. */
export type Reactions = {
    state: ReactionsState;

    /** The bootstrap's favorites, in order, for the bar, the sheet and the picker's section. */
    favorites: ReactionFavorite[];

    /** Adds the reader's reaction with an emoji, or removes it when it is already theirs. */
    toggle: (channelId: string, messageId: number, emoji: EmojiGlyph) => Promise<void>;

    /**
     * The toggle for a press that knows only the code, as the bar, the sheet and the picker do;
     * the glyph comes from the message's own chip, the catalog, or the favorites.
     */
    toggleCode: (channelId: string, messageId: number, code: string) => Promise<void>;

    /**
     * Applies a reaction event a channel topic carried to the conversation's copy of its message,
     * once the conversation's timeline lets it through.
     */
    applyEvent: (channelId: string, event: "reaction.added" | "reaction.removed", payload: unknown) => void;

    /**
     * Applies a reaction event a channel topic carried to the open thread's copies of its message
     * as it arrives: the thread has a page of its own, so they never wait for the conversation's.
     */
    applyThreadEvent: (channelId: string, event: "reaction.added" | "reaction.removed", payload: unknown) => void;

    /** The catalog, fetched once for the page; null when the fetch failed, to be asked again. */
    loadCatalog: () => Promise<CatalogEmoji[] | null>;

    /** Records an emoji the person reacted with or inserted. */
    recordRecent: (code: string) => void;

    /**
     * The full names of the others who reacted with a chip, newest first: never the reader, whose
     * own mark the chip carries, nor a person the platform cannot name. Null when not known.
     */
    reactorNames: (messageId: number, reaction: Reaction) => Promise<string[] | null>;

    /** Ends the reactions for good. */
    stop: () => void;
};

/**
 * What a message row reads of the reactions: the favorites its bar and sheet draw, who reacted
 * for a chip's tooltip, and what the open picker is placed beside when it is open on the row's
 * own message, so the bar holding it stays. Asked by message, so the picker opening reaches the
 * rows of its message and no other.
 */
export type ReactionHost = {
    favorites: ReactionFavorite[];
    reactorNames: Reactions["reactorNames"];
    pickerAnchorOf: (messageId: number) => HTMLElement | null;
};

/** How many recently used emoji the picker shows. */
const recentLimit = 8;

/** How many names a chip's tooltip shows before it counts the rest. */
const reactorLimit = 3;

/**
 * The tooltip line of a chip: "You" first when the reader reacted, then the names of the others
 * and how many more; before the names arrive, or with none to show, the count alone.
 *
 * @param reaction The chip.
 * @param names The others who reacted, as reactorNames answers; null while unknown.
 *
 * @returns The line.
 */
export function reactorLine(reaction: Reaction, names: string[] | null): string {
    // An image emoji has no glyph to put in a sentence, so it is named by the code a person types.
    const glyph = reaction.emoji ?? `:${reaction.code}:`;

    // The reader leads whether or not they were among the newest few the names were read from,
    // so the line still names no more people than the limit.
    const others = names?.slice(0, reaction.mine ? reactorLimit - 1 : reactorLimit) ?? [];
    if (!names || (!reaction.mine && others.length === 0)) {
        return `${reaction.count} ${reaction.count === 1 ? "person" : "people"} reacted with ${glyph}`;
    }

    const named = reaction.mine ? ["You", ...others] : others;
    const rest = reaction.count - named.length;
    const parts = rest > 0 ? [...named, `${rest} more`] : named;
    const who = parts.length === 1 ? parts[0] : `${parts.slice(0, -1).join(", ")} and ${parts[parts.length - 1]}`;

    return `${who} reacted with ${glyph}`;
}

/**
 * Reads the recents a store holds, trusting only a list of codes.
 *
 * @param storage The store.
 * @param key The recents' key.
 *
 * @returns The codes, newest first; none when the store is absent, blocked or holds anything else.
 */
function readRecents(storage: Pick<Storage, "getItem"> | null | undefined, key: string): string[] {
    try {
        const parsed: unknown = JSON.parse(storage?.getItem(key) ?? "[]");
        if (Array.isArray(parsed) && parsed.every(code => typeof code === "string")) {
            return parsed.slice(0, recentLimit);
        }
    }
    catch {
        // A blocked store or a value that is not JSON costs only the section, never the page.
    }

    return [];
}

/**
 * Moves the reader's own reaction with an emoji on one copy of a message: the mark and the count
 * together, a new chip where there was none, and a chip at zero taken away.
 *
 * @param message The copy.
 * @param emoji The emoji.
 * @param isMine Whether the reader's reaction should now be there.
 * @param place Where a new chip goes among the rest.
 */
function setOwnReaction(message: TimelineMessage, emoji: EmojiGlyph, isMine: boolean, place: (reactions: Reaction[], code: string) => number): void {
    const reactions = message.reactions ?? [];
    const index = reactions.findIndex(reaction => reaction.code === emoji.code);
    const chip = index >= 0 ? reactions[index] : null;

    if ((chip?.mine ?? false) === isMine) {
        return;
    }

    if (isMine) {
        if (chip) {
            chip.count++;
            chip.mine = true;
        }
        else {
            addChip(message, emoji, true, place);
        }

        return;
    }

    // Only a chip marked the reader's reaches here, so it exists.
    chip!.mine = false;
    chip!.count--;
    if (chip!.count <= 0) {
        reactions.splice(index, 1);
    }
}

/**
 * Lays one emoji's chip as an answer counts it over a copy of a message, leaving its other chips:
 * the chip replaced, put in place, or taken away when the answer has none of that emoji.
 *
 * @param message The copy.
 * @param code The emoji.
 * @param chip The answer's chip of it, if any.
 * @param place Where a new chip goes among the rest.
 */
function layChip(message: TimelineMessage, code: string, chip: Reaction | undefined, place: (reactions: Reaction[], code: string) => number): void {
    const reactions = message.reactions ?? [];
    const index = reactions.findIndex(reaction => reaction.code === code);

    if (!chip) {
        if (index >= 0) {
            reactions.splice(index, 1);
        }
        return;
    }

    if (index >= 0) {
        reactions.splice(index, 1, { ...chip });
    }
    else if (!message.reactions) {
        message.reactions = [{ ...chip }];
    }
    else {
        reactions.splice(place(reactions, code), 0, { ...chip });
    }
}

/**
 * Draws a chip of one reaction on a copy of a message that has none of that emoji.
 *
 * @param message The copy.
 * @param emoji The emoji, its glyph as the server sent it.
 * @param isMine Whether the reaction is the reader's.
 * @param place Where the chip goes among the rest.
 */
function addChip(message: TimelineMessage, emoji: EmojiGlyph, isMine: boolean, place: (reactions: Reaction[], code: string) => number): void {
    const chip: Reaction = { code: emoji.code, count: 1, mine: isMine, emoji: emoji.emoji ?? null, image_url: emoji.image_url ?? null, label: emoji.label ?? emoji.code };

    if (!message.reactions) {
        message.reactions = [chip];
        return;
    }

    message.reactions.splice(place(message.reactions, emoji.code), 0, chip);
}

/**
 * Creates the reactions.
 *
 * @param dependencies What the reactions reach outside themselves.
 *
 * @returns The reactions.
 */
export function createReactions(dependencies: ReactionsDependencies): Reactions {
    const recentsKey = `rock-chat-recent-reactions:${dependencies.churchKey}:${dependencies.personAliasGuid}`;

    const state = reactive<ReactionsState>({
        catalog: null,
        recents: readRecents(dependencies.storage, recentsKey)
    }) as ReactionsState;

    let isStopped = false;

    // The one fetch of the catalog still out, which every open while it is out shares.
    let catalogLoad: Promise<CatalogEmoji[] | null> | null = null;

    // Each press takes the next number, kept by message and emoji; its answer counts only while
    // its number is still the newest for that emoji, so a late answer never undoes a press of it
    // made since, and a press of another emoji is neither overtaken nor taken back by it. With
    // each, whether the reader's reaction is there as the server last answered, which a refused
    // newest press puts back.
    const presses = new Map<string, { turn: number, out: number, confirmed: boolean }>();
    let toggleTurn = 0;

    // The presses of each message still out, and whether two were ever out at once. Crossing
    // answers can come back in either order and each lists the message as the server counted it
    // then, so while presses cross an answer speaks only for its own emoji.
    const outstanding = new Map<number, { count: number, isCrossed: boolean }>();

    // The names asked for per message and emoji, held while the chip's count and mark are the
    // ones they were asked for.
    const names = new Map<string, { count: number, mine: boolean, names: Promise<string[] | null> }>();

    /** The conversation's copy of a message, if its timeline holds it. */
    function timelineCopy(channelId: string, messageId: number): TimelineMessage | undefined {
        return dependencies.timelines.state(channelId).messages.find(message => message.id === messageId);
    }

    /** The open thread's copies of a message: its root, or a reply it holds. */
    function threadCopies(channelId: string, messageId: number): TimelineMessage[] {
        const thread = dependencies.threads?.state.thread;
        if (!thread || thread.channelId !== channelId) {
            return [];
        }

        return [thread.root, thread.replies.find(message => message.id === messageId)]
            .filter((copy): copy is TimelineMessage => !!copy && copy.id === messageId);
    }

    /** Every copy of a message the client holds now, the conversation's first, each once. */
    function copiesOf(channelId: string, messageId: number): TimelineMessage[] {
        return [...new Set([timelineCopy(channelId, messageId), ...threadCopies(channelId, messageId)])]
            .filter((copy): copy is TimelineMessage => !!copy);
    }

    /**
     * Changes the conversation's copy of a message at once, its reactions then recorded so a page
     * read before the change does not take it back.
     */
    function changeTimeline(channelId: string, messageId: number, apply: (copy: TimelineMessage) => void): void {
        const onTimeline = timelineCopy(channelId, messageId);
        if (onTimeline) {
            apply(onTimeline);
            dependencies.timelines.recordReactions(messageId, onTimeline.reactions ?? []);
        }
    }

    /**
     * Changes the open thread's copies of a message once its page is in, leaving one the
     * conversation also holds to the conversation's change, so it is changed once.
     */
    function changeThread(channelId: string, messageId: number, apply: (copy: TimelineMessage) => void): void {
        dependencies.threads?.whenLoaded(channelId, () => {
            if (isStopped) {
                return;
            }
            const onTimeline = timelineCopy(channelId, messageId);
            threadCopies(channelId, messageId).filter(copy => copy !== onTimeline).forEach(apply);
        });
    }

    /** Changes every copy of a message: the conversation's at once and the open thread's once its page is in. */
    function change(channelId: string, messageId: number, apply: (copy: TimelineMessage) => void): void {
        changeTimeline(channelId, messageId, apply);
        changeThread(channelId, messageId, apply);
    }

    /**
     * What a reaction event does to a copy of its message, by who reacted; null for a payload
     * that names no message or emoji.
     */
    function eventChange(event: "reaction.added" | "reaction.removed", payload: unknown): { messageId: number, apply: (copy: TimelineMessage) => void } | null {
        const reaction = payload as Partial<ReactionEvent> | null;
        if (typeof reaction?.message_id !== "number" || typeof reaction.code !== "string") {
            return null;
        }

        const isAdded = event === "reaction.added";
        const emoji: EmojiGlyph = { code: reaction.code, emoji: reaction.emoji ?? null, image_url: reaction.image_url ?? null, label: reaction.label ?? reaction.code };
        const isReaders = reaction.person_alias_guid === dependencies.personAliasGuid;

        return {
            messageId: reaction.message_id,
            apply: copy => {
                // The reader's own moves only a mark it would change: their toggle already drew
                // it, and another device of theirs has not.
                if (isReaders) {
                    setOwnReaction(copy, emoji, isAdded, placeOf);
                    return;
                }

                const chip = copy.reactions?.find(existing => existing.code === emoji.code);
                if (isAdded) {
                    if (chip) {
                        chip.count++;
                    }
                    else {
                        addChip(copy, emoji, false, placeOf);
                    }
                }
                else if (chip) {
                    chip.count--;
                    if (chip.count <= 0) {
                        copy.reactions!.splice(copy.reactions!.indexOf(chip), 1);
                    }
                }
            }
        };
    }

    /**
     * Where a new chip lands among a message's chips: by the catalog's picker order, as the
     * server orders a page, before any chip the catalog does not offer. An emoji outside the
     * catalog, or any emoji before the catalog is fetched, goes last; the next page orders it.
     */
    function placeOf(reactions: Reaction[], code: string): number {
        const catalog = state.catalog;
        const order = catalog ? catalog.findIndex(emoji => emoji.code === code) : -1;
        if (!catalog || order < 0) {
            return reactions.length;
        }

        const after = reactions.findIndex(reaction => {
            const theirs = catalog.findIndex(emoji => emoji.code === reaction.code);
            return theirs < 0 || theirs > order;
        });

        return after < 0 ? reactions.length : after;
    }

    /** The glyph of a code: the message's own chip first, then the catalog, then the favorites. */
    function glyphOf(channelId: string, messageId: number, code: string): EmojiGlyph {
        const known = copiesOf(channelId, messageId)[0]?.reactions?.find(reaction => reaction.code === code)
            ?? state.catalog?.find(emoji => emoji.code === code)
            ?? dependencies.favorites.find(favorite => favorite.code === code);

        // An unknown code still toggles; the answer brings the server's glyph for it.
        return known
            ? { code, emoji: known.emoji, image_url: known.image_url, label: known.label }
            : { code, emoji: null, image_url: null, label: code };
    }

    /**
     * Lays an answer over every copy, each copy its own chips so no two share one: the whole list,
     * or while presses of the message cross, only the pressed emoji's chip.
     */
    function applyAnswer(channelId: string, messageId: number, code: string, reactions: Reaction[], isWhole: boolean): void {
        const chip = reactions.find(reaction => reaction.code === code);
        const apply = isWhole
            ? (copy: TimelineMessage): void => {
                copy.reactions = reactions.map(reaction => ({ ...reaction }));
            }
            : (copy: TimelineMessage): void => layChip(copy, code, chip, placeOf);

        change(channelId, messageId, apply);

        // A message the conversation is still reloading is not held yet, but its next page may
        // have been read before this answer, so the answer is recorded all the same.
        if (isWhole && !timelineCopy(channelId, messageId)) {
            dependencies.timelines.recordReactions(messageId, reactions);
        }
    }

    /** Adds or removes the reader's reaction with an emoji, drawn at once and replaced by the answer. */
    async function toggle(channelId: string, messageId: number, emoji: EmojiGlyph): Promise<void> {
        if (isStopped) {
            return;
        }

        const isMine = copiesOf(channelId, messageId)[0]?.reactions?.find(reaction => reaction.code === emoji.code)?.mine ?? false;
        const turn = ++toggleTurn;
        const key = `${messageId}/${emoji.code}`;
        const press = presses.get(key) ?? { turn, out: 0, confirmed: isMine };
        press.turn = turn;
        press.out++;
        presses.set(key, press);
        const pending = outstanding.get(messageId) ?? { count: 0, isCrossed: false };
        pending.isCrossed ||= pending.count > 0;
        pending.count++;
        outstanding.set(messageId, pending);

        // Drawn at once, the mark and the count together, before the platform answers.
        change(channelId, messageId, copy => setOwnReaction(copy, emoji, !isMine, placeOf));

        const result = await dependencies.call(isMine ? "chat_remove_reaction" : "chat_add_reaction", { p_message_id: messageId, p_reaction: emoji.code });
        if (isStopped) {
            return;
        }

        const isNewest = press.turn === turn;
        if (result.ok) {
            press.confirmed = !isMine;
        }
        press.out -= 1;
        if (press.out === 0) {
            presses.delete(key);
        }
        const isWhole = !pending.isCrossed;
        pending.count -= 1;
        if (pending.count === 0) {
            outstanding.delete(messageId);
        }

        if (!result.ok) {
            dependencies.notify({ code: result.error.code, text: messageForError(result.error), icon: "alert" });

            // Only the press is taken back, to where the server last left the reader's reaction,
            // so a reaction someone else made meanwhile stays. A newer press of the same emoji
            // answers for its chip itself.
            if (isNewest) {
                change(channelId, messageId, copy => setOwnReaction(copy, emoji, press.confirmed, placeOf));
            }
            return;
        }

        const answer = result.data as Partial<ReactionAnswer> | null;
        if (isNewest && Array.isArray(answer?.reactions)) {
            applyAnswer(channelId, messageId, emoji.code, answer.reactions, isWhole);
        }
    }

    return {
        state,

        favorites: dependencies.favorites,

        toggle,

        toggleCode: (channelId: string, messageId: number, code: string): Promise<void> => toggle(channelId, messageId, glyphOf(channelId, messageId, code)),

        applyEvent: (channelId: string, event: "reaction.added" | "reaction.removed", payload: unknown): void => {
            const moved = isStopped ? null : eventChange(event, payload);
            if (moved) {
                changeTimeline(channelId, moved.messageId, moved.apply);
            }
        },

        applyThreadEvent: (channelId: string, event: "reaction.added" | "reaction.removed", payload: unknown): void => {
            const moved = isStopped ? null : eventChange(event, payload);
            if (moved) {
                changeThread(channelId, moved.messageId, moved.apply);
            }
        },

        loadCatalog: (): Promise<CatalogEmoji[] | null> => {
            if (isStopped) {
                return Promise.resolve(null);
            }

            if (state.catalog) {
                return Promise.resolve(state.catalog);
            }

            if (catalogLoad) {
                return catalogLoad;
            }

            catalogLoad = (async (): Promise<CatalogEmoji[] | null> => {
                const result = await dependencies.call("chat_list_emojis", {});

                // A failed fetch is forgotten, so the next open asks again.
                catalogLoad = null;
                const emojis = result.ok ? (result.data as { emojis?: unknown } | null)?.emojis : null;
                if (isStopped || !Array.isArray(emojis)) {
                    return null;
                }

                state.catalog = emojis as CatalogEmoji[];
                return state.catalog;
            })();

            return catalogLoad;
        },

        recordRecent: (code: string): void => {
            if (isStopped || !code) {
                return;
            }

            state.recents = [code, ...state.recents.filter(recent => recent !== code)].slice(0, recentLimit);

            try {
                dependencies.storage?.setItem(recentsKey, JSON.stringify(state.recents));
            }
            catch {
                // A full or blocked store keeps the recents for this page only.
            }
        },

        reactorNames: (messageId: number, reaction: Reaction): Promise<string[] | null> => {
            if (isStopped) {
                return Promise.resolve(null);
            }

            const key = `${messageId}/${reaction.code}`;
            const held = names.get(key);
            if (held && held.count === reaction.count && held.mine === reaction.mine) {
                return held.names;
            }

            const entry = { count: reaction.count, mine: reaction.mine, names: Promise.resolve<string[] | null>(null) };
            entry.names = (async (): Promise<string[] | null> => {
                const result = await dependencies.call("chat_list_reactors", {
                    p_message_id: messageId,
                    p_reaction: reaction.code,
                    p_after_at: null,
                    p_after_alias: null,
                    p_limit: reactorLimit
                });

                if (isStopped) {
                    return null;
                }

                const page = result.ok ? result.data as Partial<ReactorsPage> | null : null;
                if (!Array.isArray(page?.reactors)) {
                    // Forgotten, so the next hover asks again; the tooltip keeps its count line.
                    if (names.get(key) === entry) {
                        names.delete(key);
                    }
                    return null;
                }

                // The reader may have reacted under an alias other than the session's primary one.
                // Someone the platform cannot name, gone or never synced, is counted, not drawn blank.
                return page.reactors
                    .filter(reactor => reactor.person_alias_guid !== dependencies.personAliasGuid && reactor.reactor_person_alias_guid !== dependencies.personAliasGuid)
                    .map(reactor => [reactor.nick_name, reactor.last_name].filter(part => !!part).join(" "))
                    .filter(name => name.length > 0);
            })();

            names.set(key, entry);
            return entry.names;
        },

        stop: (): void => {
            // for good: what is on screen stays as it was, and nothing still out lands in it
            isStopped = true;
        }
    };
}
