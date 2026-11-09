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
// of copies, the timelines, the open thread and the threads list, keeps one ledger of them, so a page
// read before a change that lands after it does not undo it, and a change older than what a page
// showed is not laid over it.
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
 * One group of one message as a holder has it: the revision a copy last showed the whole group at,
 * and each field a live change set above that, with the revision of the change that set it. Kept
 * per field because two changes of one group state different fields: a hide that arrives after a
 * later edit still hides, though the edit's words stay newer than its.
 */
type GroupRecord = { copyRev: number, changes: Partial<Record<keyof TimelineMessage, { rev: number, value: unknown }>> };

/**
 * For each message held or heard of, each group as the holder has it, so a page read before a live
 * change that lands after it gets the change laid over it. One per holder of copies: the timelines,
 * the open thread and the threads list.
 */
export type RevisionLedger = Map<number, Partial<Record<FieldGroup, GroupRecord>>>;

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
 * The revision a field of a group is held at: the copy's, or a live change's above it.
 */
function fieldRev(record: GroupRecord, key: keyof TimelineMessage): number {
    return Math.max(record.copyRev, record.changes[key]?.rev ?? 0);
}

/**
 * What live changes above a revision set in a group, to lay over anything older.
 */
function setAbove(record: GroupRecord, rev: number): Partial<TimelineMessage> {
    const fields: Record<string, unknown> = {};
    for (const [key, change] of Object.entries(record.changes)) {
        if (change && change.rev > rev) {
            fields[key] = change.value;
        }
    }
    return fields as Partial<TimelineMessage>;
}

/**
 * Takes a live change to one group of a message. Each field it states that nothing newer set is
 * recorded at its revision; when none is, the change is older than what is held throughout and is
 * refused. A change with no revision is taken and recorded nowhere.
 *
 * @param ledger The holder's ledger.
 * @param id The message.
 * @param group The group the change states.
 * @param rev The change's revision, or null.
 * @param fields What it set.
 *
 * @returns Null to refuse it; otherwise what newer changes set, to lay over it once it is applied.
 */
export function takeChange(ledger: RevisionLedger, id: number, group: FieldGroup, rev: number | null, fields: Partial<TimelineMessage>): Partial<TimelineMessage> | null {
    if (rev === null) {
        return {};
    }
    const entry = ledger.get(id) ?? {};
    const record: GroupRecord = entry[group] ?? { copyRev: 0, changes: {} };
    let isTaken = false;
    for (const [key, value] of Object.entries(fields)) {
        if (fieldRev(record, key as keyof TimelineMessage) < rev) {
            record.changes[key as keyof TimelineMessage] = { rev, value };
            isTaken = true;
        }
    }
    if (!isTaken) {
        return null;
    }
    entry[group] = record;
    ledger.set(id, entry);
    return setAbove(record, rev);
}

/**
 * Whether a change at this revision to a group of a message is newer than everything the holder
 * has of that group.
 *
 * @param ledger The holder's ledger.
 * @param id The message.
 * @param group The group.
 * @param rev The change's revision; one with none is always newer.
 *
 * @returns True when it should be applied.
 */
export function isNewerChange(ledger: RevisionLedger, id: number, group: FieldGroup, rev: number | null): boolean {
    const record = ledger.get(id)?.[group];
    if (rev === null || !record) {
        return true;
    }
    return Object.values(record.changes).every(change => !change || change.rev < rev) && record.copyRev < rev;
}

/**
 * The revision each message's group is held at, the copy's or its newest live change's, for a
 * page that carries no revision of its own: a change heard before the page was asked for was
 * committed before it was read, so the page shows it or something newer.
 *
 * @param ledger The holder's ledger.
 * @param group The group.
 *
 * @returns Each message held or heard of, by id, and the revision its group is held at.
 */
export function heldRevisions(ledger: RevisionLedger, group: FieldGroup): Map<number, number> {
    const revs = new Map<number, number>();
    for (const [id, entry] of ledger) {
        const record = entry[group];
        if (record) {
            revs.set(id, Math.max(record.copyRev, ...Object.values(record.changes).map(change => change?.rev ?? 0)));
        }
    }
    return revs;
}

/**
 * Takes a copy of a message read at a revision, a page's or a created event's: each field a live
 * change set above that revision keeps what the change set, and the rest of each group is now held
 * at the copy's revision. A copy with no revision is read as older than any numbered change.
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
        const record = entry[group];
        if (!record) {
            entry[group] = { copyRev: readAt, changes: {} };
            continue;
        }
        result = { ...result, ...setAbove(record, readAt) };
        record.copyRev = Math.max(record.copyRev, readAt);
        // a change the copy already shows is held by the copy's revision
        for (const [key, change] of Object.entries(record.changes)) {
            if (change && change.rev <= record.copyRev) {
                delete record.changes[key as keyof TimelineMessage];
            }
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
