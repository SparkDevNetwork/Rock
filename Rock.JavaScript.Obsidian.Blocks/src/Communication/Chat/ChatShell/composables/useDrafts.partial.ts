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
// Drafts: what the person typed and has not sent, one for each place a composer types into, a
// conversation or a thread. The box draws its place's draft and writes every change back here,
// so text typed in one room never shows in another, and coming back finds it as it was left.
//
// A draft is the box's wire text and the chips drawn in it, each with the name and photo it was
// chosen with, so the box draws it again exactly. It is kept on this device for the signed-in
// person across a reload: read the first time its place is asked for, so it is there on the
// box's first frame, and written once typing pauses or the page goes, never on each keystroke.
//
// A shared computer must not keep the last person's words. Rock's sign-out takes away a mark the
// block sets, and a block that finds the mark gone forgets every draft on the device before it
// reads any. A browser closed or a sign-in left to expire signs out with no sign-out Rock sees,
// so a block also forgets every draft not its own person's: someone else signed in since.
// Storage that is missing, blocked or full keeps drafts for the page only.
//
// Two tabs of the same person share the device's drafts. Each tab hears the other's writes and
// shows them in any box not being typed in, so a draft sent in one is never left to send again
// in the other, and the bound on how many are kept is counted from what the device holds.
import { reactive } from "vue";
import { DisplayPiece } from "../types.partial";

// #region Types

/** What a box holds for one place. */
export type ComposerDraft = {
    /** The wire text, a mention written as its token, <@guid> or <@all>. */
    text: string;

    /** A mention piece for each chip drawn in the box, which the box draws its token with. */
    mentions: DisplayPiece[];
};

/** The part of the browser's storage drafts use, every call of which may throw. */
export type DraftStorage = {
    readonly length: number;
    key: (index: number) => string | null;
    getItem: (key: string) => string | null;
    setItem: (key: string, value: string) => void;
    removeItem: (key: string) => void;
};

/** Everything drafts reach outside themselves. */
export type DraftsDependencies = {
    /** The device's storage, or null where the browser has none. */
    storage: DraftStorage | null;

    tenantId: string;
    personAliasGuid: string;

    /**
     * The mark Rock's sign-out takes away: present while nobody has signed out on this browser
     * since chat last ran in it. Without one, nothing tells a sign-out, so nothing is kept.
     */
    signInMark: { isSet: () => boolean, set: () => void } | null;

    /**
     * The page, whose leaving writes what typing has not yet paused for, and whose storage
     * events carry another tab's writes.
     */
    window?: { addEventListener: (type: string, listener: (event: unknown) => void) => void, removeEventListener: (type: string, listener: (event: unknown) => void) => void };

    now?: () => number;
};

/** The drafts a shell holds. */
export type Drafts = {
    /** The draft of a place, read from the device the first time the place is asked for. */
    of: (place: string) => ComposerDraft;

    /** The box's text changed: kept at once, written to the device once typing pauses. */
    write: (place: string, text: string) => void;

    /** A chip was drawn in a place's box, with the name and photo it is drawn with. */
    addMention: (place: string, piece: DisplayPiece) => void;

    /** Puts held text after what a place's box holds, a blank line between, as refused text comes back. */
    append: (place: string, text: string) => void;

    /** The place whose box has focus, or null: another tab's writes never replace what is being typed. */
    focus: (place: string | null) => void;

    /** Writes what is waiting and stops listening to the page. */
    stop: () => void;
};

/** A draft as the device keeps it. */
type StoredDraft = ComposerDraft & {
    /** When it was last written, in milliseconds. */
    at: number;
};

// #endregion Types

/** How long typing must pause before a draft is written; an estimate, a little past a keystroke gap. */
const writeDelayMs = 300;

/** How long a draft is kept on the device, so a room never visited again does not hold it forever. */
const keptForMs = 30 * 24 * 60 * 60 * 1000;

/** How many drafts a person keeps on the device; the oldest go first. */
const keptAtMost = 50;

/** Every draft's storage key starts with this, so a sign-out can find them all. */
const draftPrefix = "rock-chat:draft:";

/** A mention token in any letter case, as the box writes it. */
const tokenPattern = /<@([0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}|all)>/gi;

/**
 * Makes the drafts of one person in one church.
 *
 * @param dependencies What drafts reach outside themselves.
 *
 * @returns The drafts.
 */
export function createDrafts(dependencies: DraftsDependencies): Drafts {
    const now = dependencies.now ?? Date.now;

    // Without the mark a sign-out cannot be told, so the device keeps nothing.
    const storage = dependencies.signInMark ? dependencies.storage : null;
    const personPrefix = `${draftPrefix}${dependencies.tenantId.toLowerCase()}:${dependencies.personAliasGuid.toLowerCase()}:`;

    /** Each place's draft, once asked for or written; each one reactive, so its box follows it. */
    const drafts = new Map<string, ComposerDraft>();

    /** The places changed since the last write to the device. */
    const waiting = new Set<string>();

    let writeTimer: ReturnType<typeof setTimeout> | null = null;

    /** The place whose box has focus, whose text another tab's write leaves alone. */
    let focusedPlace: string | null = null;

    // Set again on every load, so the mark lasts as long as nobody signs out.
    if (storage && dependencies.signInMark) {
        const isSignedOut = !dependencies.signInMark.isSet();
        keysStartingWith(draftPrefix)
            .filter(key => isSignedOut || !key.startsWith(personPrefix))
            .forEach(remove);
        dependencies.signInMark.set();
    }

    dependencies.window?.addEventListener("pagehide", writeWaiting);
    dependencies.window?.addEventListener("storage", onStorage);

    // #region Functions

    function draftOf(place: string): ComposerDraft {
        let draft = drafts.get(place);
        if (!draft) {
            draft = reactive(read(place));
            drafts.set(place, draft);
        }

        return draft;
    }

    /** A place's draft as the device holds it, or an empty one. One too old is removed as it is read. */
    function read(place: string): ComposerDraft {
        const empty: ComposerDraft = { text: "", mentions: [] };
        let value: string | null = null;
        try {
            value = storage?.getItem(personPrefix + place) ?? null;
        }
        catch {
            // Intentionally ignored: blocked storage only means the draft is not on this device.
        }

        if (!value) {
            return empty;
        }

        const draft = parse(value);
        if (!draft || draft.at < now() - keptForMs) {
            remove(personPrefix + place);
            return empty;
        }

        return { text: draft.text, mentions: draft.mentions };
    }

    /** A stored draft, or null for anything else under its key. */
    function parse(value: string): StoredDraft | null {
        try {
            const draft = JSON.parse(value) as Partial<StoredDraft> | null;
            if (typeof draft?.text !== "string" || typeof draft.at !== "number") {
                return null;
            }

            return { text: draft.text, mentions: Array.isArray(draft.mentions) ? draft.mentions : [], at: draft.at };
        }
        catch {
            return null;
        }
    }

    function scheduleWrite(place: string): void {
        if (!storage) {
            return;
        }

        waiting.add(place);
        if (writeTimer !== null) {
            clearTimeout(writeTimer);
        }
        writeTimer = setTimeout(writeWaiting, writeDelayMs);
    }

    /** Writes every changed draft to the device: an empty one is removed, the rest kept within the bounds. */
    function writeWaiting(): void {
        if (writeTimer !== null) {
            clearTimeout(writeTimer);
            writeTimer = null;
        }

        if (!storage || waiting.size === 0) {
            return;
        }

        const kept = storedDrafts();
        for (const place of waiting) {
            const draft = drafts.get(place);
            const key = personPrefix + place;
            if (!draft || draft.text.trim() === "") {
                remove(key);
                kept.delete(key);
                continue;
            }

            const at = now();
            const value: StoredDraft = { text: draft.text, mentions: mentionsIn(draft), at };
            try {
                storage.setItem(key, JSON.stringify(value));
                kept.set(key, at);
            }
            catch {
                // Intentionally ignored: full or blocked storage keeps the draft for this page only.
            }
        }
        waiting.clear();

        // The oldest go first once there are too many.
        const byAge = Array.from(kept.entries()).sort((a, b) => a[1] - b[1]);
        for (const [key] of byAge.slice(0, Math.max(0, byAge.length - keptAtMost))) {
            remove(key);
            kept.delete(key);
        }
    }

    /**
     * The person's drafts on the device and when each was written, read at each write because
     * another tab of the person's writes there too, dropping any too old to keep.
     */
    function storedDrafts(): Map<string, number> {
        const stored = new Map<string, number>();
        const oldest = now() - keptForMs;
        for (const key of keysStartingWith(personPrefix)) {
            let at: number | undefined;
            try {
                at = parse(storage?.getItem(key) ?? "")?.at;
            }
            catch {
                // Intentionally ignored: an unreadable draft is removed below like a stale one.
            }

            if (at === undefined || at < oldest) {
                remove(key);
            }
            else {
                stored.set(key, at);
            }
        }

        return stored;
    }

    /** The chips the text still holds, so a chip deleted from the box is not kept. */
    function mentionsIn(draft: ComposerDraft): DisplayPiece[] {
        const tokens = new Set(Array.from(draft.text.matchAll(tokenPattern), match => match[1].toLowerCase()));
        return draft.mentions.filter(piece => tokens.has(mentionKey(piece)));
    }

    /**
     * Another tab wrote the device's storage. A draft of this person's this tab holds takes the
     * other's text, or empties as the other sent or cleared it, unless its box is being typed in
     * or this tab has a change of its own still to write: that write comes after and wins.
     */
    function onStorage(event: unknown): void {
        const { key = null, newValue = null } = event as { key?: string | null, newValue?: string | null };
        if (key !== null && !key.startsWith(personPrefix)) {
            return;
        }

        // A null key is the whole storage cleared, every draft with it.
        const fresh = newValue !== null ? parse(newValue) : null;
        const places = key === null ? Array.from(drafts.keys()) : [key.slice(personPrefix.length)];
        for (const place of places) {
            const draft = drafts.get(place);
            if (!draft || place === focusedPlace || waiting.has(place)) {
                continue;
            }

            draft.text = fresh?.text ?? "";
            draft.mentions = fresh?.mentions ?? [];
        }
    }

    function keysStartingWith(prefix: string): string[] {
        const keys: string[] = [];
        try {
            for (let index = 0; storage && index < storage.length; index++) {
                const key = storage.key(index);
                if (key?.startsWith(prefix)) {
                    keys.push(key);
                }
            }
        }
        catch {
            // Intentionally ignored: storage that cannot be listed holds nothing this can reach.
        }

        return keys;
    }

    function remove(key: string): void {
        try {
            storage?.removeItem(key);
        }
        catch {
            // Intentionally ignored: blocked storage holds nothing this can reach.
        }
    }

    // #endregion Functions

    return {
        of: draftOf,

        write: (place: string, text: string): void => {
            const draft = draftOf(place);
            if (draft.text !== text) {
                draft.text = text;
                scheduleWrite(place);
            }
        },

        addMention: (place: string, piece: DisplayPiece): void => {
            const draft = draftOf(place);
            const key = mentionKey(piece);
            const index = draft.mentions.findIndex(m => mentionKey(m) === key);
            if (index < 0) {
                draft.mentions.push(piece);
            }
            else if (draft.mentions[index].text !== piece.text || draft.mentions[index].avatar_url !== piece.avatar_url) {
                draft.mentions[index] = piece;
            }
        },

        append: (place: string, text: string): void => {
            const draft = draftOf(place);
            draft.text = [draft.text, text].filter(part => part.trim() !== "").join("\n\n");
            scheduleWrite(place);
        },

        focus: (place: string | null): void => {
            focusedPlace = place;
        },

        stop: (): void => {
            writeWaiting();
            dependencies.window?.removeEventListener("pagehide", writeWaiting);
            dependencies.window?.removeEventListener("storage", onStorage);
        }
    };
}

/** A mention piece's token inside its brackets, in lower case: the guid, or "all". */
function mentionKey(piece: DisplayPiece): string {
    return piece.all === true ? "all" : String(piece.person_alias_guid ?? "").toLowerCase();
}
