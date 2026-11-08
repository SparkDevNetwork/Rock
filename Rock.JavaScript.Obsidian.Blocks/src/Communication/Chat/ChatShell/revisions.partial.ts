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
// The numbers that order a message's copies against the live changes to it. Every history event on a
// room's topic carries the room's revision, and every page the revision it was read at; each holder
// of copies, the timelines and the open thread, keeps one ledger of them, so a page read before a
// change that lands after it does not undo it, and a change older than what a page showed is not laid
// over it.
import { MessageDeletedEvent, MessageEditedEvent, MessageVisibilityEvent, TimelineMessage } from "./types.partial";

/**
 * The fields of a message each history event states in full, by the group one revision orders:
 * the message itself, and its thread's counts. A reaction carries no revision and the reader's own
 * thread marks no event carries, so neither is a group.
 */
const groupFields = {
    message: ["body", "pieces", "blocks", "attachments", "metadata", "edited_at", "deleted_at", "visibility", "visibility_notice"],
    thread: ["reply_count", "last_reply_at", "participant_count", "repliers"]
} as const;

/** A group of a message's fields that one revision orders. */
export type FieldGroup = keyof typeof groupFields;

/**
 * For each message held or heard of, the revision each group is held at, and for a live change
 * newer than any page, the fields it set, so a page read before it that lands after it gets them
 * laid over it. One per holder of copies: the timelines and the open thread.
 */
export type RevisionLedger = Map<number, Partial<Record<FieldGroup, { rev: number, fields?: Partial<TimelineMessage> }>>>;

/**
 * The revision a live payload or a page carries, or null when it carries none. A signal, a
 * reaction, or an older platform's answer carries none and is ordered as before.
 *
 * @param value The payload or page.
 *
 * @returns Its revision, or null.
 */
export function revOf(value: unknown): number | null {
    const rev = value && typeof value === "object" ? (value as { rev?: unknown }).rev : undefined;
    return typeof rev === "number" && Number.isFinite(rev) ? rev : null;
}

/**
 * Takes a live change to one group of a message: true when it is newer than what is held, and then
 * recorded with the fields it set; false for a change a page or an earlier event already holds. A
 * change with no revision is taken and recorded nowhere.
 *
 * @param ledger The holder's ledger.
 * @param id The message.
 * @param group The group the change states.
 * @param rev The change's revision, or null.
 * @param fields What it set.
 *
 * @returns Whether to apply it.
 */
export function takeChange(ledger: RevisionLedger, id: number, group: FieldGroup, rev: number | null, fields: Partial<TimelineMessage>): boolean {
    if (rev === null) {
        return true;
    }
    const entry = ledger.get(id) ?? {};
    const held = entry[group];
    if (held && held.rev >= rev) {
        return false;
    }
    // a later change of the group states what it changed in full, so it is laid over the earlier
    entry[group] = { rev, fields: { ...held?.fields, ...fields } };
    ledger.set(id, entry);
    return true;
}

/**
 * Whether a change at this revision to a group of a message is newer than what the holder has.
 *
 * @param ledger The holder's ledger.
 * @param id The message.
 * @param group The group.
 * @param rev The change's revision; one with none is always newer.
 *
 * @returns True when it should be applied.
 */
export function isNewerChange(ledger: RevisionLedger, id: number, group: FieldGroup, rev: number | null): boolean {
    const held = ledger.get(id)?.[group];
    return rev === null || !held || held.rev < rev;
}

/**
 * Takes a copy of a message read at a revision, a page's or a created event's: each group a live
 * change moved above that revision keeps what the change set, and every other group is now held at
 * the copy's revision. A copy with no revision is read as older than any numbered change.
 *
 * @param ledger The holder's ledger.
 * @param copy The copy as read.
 * @param rev The revision it was read at, or null.
 *
 * @returns The copy with the newer changes laid over it.
 */
export function layNewer(ledger: RevisionLedger, copy: TimelineMessage, rev: number | null): TimelineMessage {
    const readAt = rev ?? 0;
    const entry = ledger.get(copy.id) ?? {};
    let result = copy;
    for (const group of Object.keys(groupFields) as FieldGroup[]) {
        const held = entry[group];
        if (held && held.rev > readAt) {
            result = held.fields ? { ...result, ...held.fields } : result;
        }
        else {
            entry[group] = { rev: readAt };
        }
    }
    ledger.set(copy.id, entry);
    return result;
}

/**
 * What an edit, a delete or a change of visibility sets on a message, stated in full, for a copy
 * not held yet to have laid over it when its page lands; null for any other event.
 *
 * @param event The live event's name.
 * @param payload Its payload.
 *
 * @returns The fields it sets, or null.
 */
export function changedFields(event: string, payload: unknown): Partial<TimelineMessage> | null {
    if (event === "message.edited") {
        const edited = payload as MessageEditedEvent;
        return { body: edited.body, pieces: edited.pieces ?? null, edited_at: edited.edited_at };
    }
    if (event === "message.deleted") {
        return { deleted_at: (payload as MessageDeletedEvent).deleted_at, body: null, pieces: null, blocks: null, attachments: null, metadata: null };
    }
    if (event === "message.visibility") {
        const changed = payload as MessageVisibilityEvent;
        const withBody = changed.body !== undefined
            ? { body: changed.body, pieces: changed.pieces ?? null, blocks: changed.blocks ?? null, attachments: changed.attachments ?? null }
            : {};
        return {
            visibility: changed.state,
            visibility_notice: changed.notice,
            ...(changed.state === "visible" ? withBody : { body: null, pieces: null, blocks: null, attachments: null, metadata: null })
        };
    }
    return null;
}
