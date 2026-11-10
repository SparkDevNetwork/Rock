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
// A message as every holder keeps it: one shape whichever path brought the copy, its send's key in
// one form, masked as the people blocked now stand, and what its row draws.
import { reactive, toRaw } from "vue";
import { DisplayPiece, MessageDeletedEvent, MessageEditedEvent, MessageVisibilityEvent, TimelineMessage } from "./types.partial";

/** What a message row draws: what kind of row, and its text. */
export type MessageDisplay = {
    kind: "message" | "system" | "deleted" | "notice" | "blocked";
    text: string | null;
};

/**
 * A send's key in the one form every comparison uses. The platform stores the key as a UUID, and
 * a UUID may be written back in either case, so a key is matched only once lowered; otherwise a
 * copy of the person's message would stand beside its pending row and never settle the send.
 *
 * @param key The key as it arrived, if any.
 *
 * @returns The key in lower case, or null when there is none.
 */
export function sendKey(key: string | null | undefined): string | null {
    return key ? key.toLowerCase() : null;
}

/**
 * A message as it is held: its send's key, if any, in the form every comparison uses. A message
 * already in that form is returned as it is, so most copies cost nothing.
 *
 * @param message The message as it arrived.
 *
 * @returns The message with its key lowered.
 */
export function withSendKey<T extends Pick<TimelineMessage, "client_key">>(message: T): T {
    const key = sendKey(message.client_key);
    return message.client_key && key !== message.client_key ? { ...message, client_key: key } : message;
}

/**
 * The key a message's row is drawn under, and what happened to it is recorded under: its send's key
 * when it has one, which its pending row was drawn under too, else its id. A send key is a UUID and
 * an id a number, so the two never meet.
 *
 * @param message The message.
 *
 * @returns The row's key.
 */
export function rowKey(message: Pick<TimelineMessage, "id" | "client_key">): string {
    return `k:${sendKey(message.client_key) ?? message.id}`;
}

/**
 * The state a message has until a change says otherwise: what a page states for a message never
 * edited, deleted, hidden, reacted to or replied to. The send's answer and the live copy state none
 * of it, so every copy is completed with it as it is held, and two copies of one message compare
 * equal whichever path brought them.
 */
export const newMessageState = {
    edited_at: null,
    deleted_at: null,
    visibility: "visible",
    visibility_notice: null,
    reactions: [],
    reply_count: 0,
    last_reply_at: null,
    participant_count: 0,
    repliers: null,
    thread_following: null,
    thread_unread: null
} as const satisfies Partial<Record<keyof TimelineMessage, unknown>>;

/**
 * Every other field a page states: what the message is and says, and who sent it, which a new
 * message has no one value for, so a copy keeps what it states and a field it leaves out keeps
 * what is held. Kept as data beside the state, so a field the platform adds is classed in one of
 * the two before it is relied on.
 */
export const keptAsStated: readonly (keyof TimelineMessage)[] = [
    "id", "parent_id", "quoted_message_id", "quoted", "shown_in_channel", "person_alias_guid", "client_key", "is_own",
    "sender_nick_name", "sender_last_name", "sender_avatar_url", "sender_listed", "sender_is_leader",
    "message_type", "system_kind", "body", "attachments", "pieces", "blocks", "metadata", "actions", "created_at"
];

/**
 * A copy of a message as every holder keeps it: combined with what is held, its key lowered, each
 * state field neither states completed with a new message's, and masked as the people blocked now
 * stand. Completed after combining, never before: inside the combine a field left out still means
 * "not said", which is how the send's answer landing after a hide keeps the hide.
 *
 * @param held The copy already held, if any.
 * @param arrived The copy as it arrived: a page's row, a live copy or the send's answer.
 * @param mask The mask as the set of people blocked now stands.
 *
 * @returns The copy to hold.
 */
export function holdMessage(held: TimelineMessage | undefined, arrived: TimelineMessage, mask: MessageMask): TimelineMessage {
    const keyed = withSendKey(arrived);
    const message = (held ? combine(held, keyed) : { ...keyed }) as Record<string, unknown>;

    for (const [field, value] of Object.entries(newMessageState)) {
        if (message[field] === undefined) {
            // each copy its own list, so a change to one copy's chips moves no other
            message[field] = Array.isArray(value) ? [] : value;
        }
    }

    return mask(message as TimelineMessage);
}

/**
 * What a row shows for a message. The server decides when a message is hidden or removed and
 * writes the sentence shown in its place, and a message type this client was not built with is
 * drawn by its text, so a released client still shows what later releases of the platform send.
 *
 * @param message The message.
 *
 * @returns The kind of row and its text.
 */
export function messageDisplay(message: TimelineMessage): MessageDisplay {
    // Before a delete or a hide, which would tell the reader what became of it.
    if (message.blocked) {
        return { kind: "blocked", text: blockedSentence };
    }

    if (message.deleted_at) {
        return { kind: "deleted", text: "This message was deleted." };
    }

    if (message.visibility === "hidden" || message.visibility === "removed") {
        return { kind: "notice", text: message.visibility_notice ?? "This message is not shown." };
    }

    return { kind: message.message_type === "system" ? "system" : "message", text: message.body };
}

/** What stands in place of a message by someone the reader blocked. */
export const blockedSentence = "Message from someone you blocked";

/**
 * A message by someone the reader blocked, as the platform answers it on a page: its row, id, time
 * and thread counts kept, and its sender and everything it says withheld. A live copy is one
 * payload for every subscriber, so the client makes it so itself; the one list of what is withheld.
 *
 * @param message The message as it arrived.
 *
 * @returns The message withheld and marked blocked.
 */
export function blockedCopy<T extends Partial<TimelineMessage>>(message: T): T {
    const copy: Record<string, unknown> = { ...message, blocked: true };
    for (const field of withheldFields) {
        copy[field] = null;
    }
    return copy as T;
}

/** What a message by someone the reader blocked withholds, the one list every copy is read by. */
const withheldFields = ["sender_nick_name", "sender_last_name", "sender_avatar_url", "body", "pieces", "blocks", "attachments", "metadata", "actions", "quoted"] as const;

/** Every field a mask can change on a message: what it withholds, its mark, and the faces it carries. */
const maskedFields: readonly string[] = [...withheldFields, "blocked", "repliers", "reactions"];

/**
 * Whether a message is already withheld as blockedCopy makes it, so masking it again writes
 * nothing and wakes nothing that draws it.
 *
 * @param message The message as it is held.
 *
 * @returns True when it is marked blocked and every withheld field is empty.
 */
export function isWithheld(message: Partial<TimelineMessage>): boolean {
    return message.blocked === true && withheldFields.every(field => message[field] === null || message[field] === undefined);
}

/**
 * Turns a message into what the reader may see under the people they blocked, as they stand when
 * it is written: the same object when nothing applies. Every holder of messages writes through
 * one, so an answer read before a block is masked by the set that holds when it lands.
 */
export type MessageMask = <T extends Partial<TimelineMessage>>(message: T) => T;

/**
 * The mask of a holder given none: a copy the platform withheld stays withheld, whatever is laid
 * over it, and anything else is as it came.
 */
export const platformMask: MessageMask = message => message.blocked && !isWithheld(message) ? blockedCopy(message) : message;

/**
 * Masks again the messages a holder keeps, writing on each row object itself rather than putting
 * a copy in its place, so a quote waiting in a composer, a drawer or an open sheet holding the row
 * shows the change too. The rows are read raw and only a row the mask changes is written, through
 * its reactive copy, so a room with nothing to withhold costs a walk and wakes nothing.
 *
 * @param messages The rows held.
 * @param mask The mask as the set now stands.
 */
export function maskHeld(messages: readonly Partial<TimelineMessage>[], mask: MessageMask): void {
    for (const held of toRaw(messages)) {
        const row = toRaw(held);
        const masked = mask(row) as Record<string, unknown>;
        if (masked === row) {
            continue;
        }
        // only the fields the mask changed go through the reactive copy, which wakes what draws them
        const shown = reactive(row) as Record<string, unknown>;
        const raw = row as Record<string, unknown>;
        for (const key of maskedFields) {
            if (masked[key] !== raw[key]) {
                shown[key] = masked[key];
            }
        }
    }
}

/** A mention token as a person types it: a person's alias guid, or the whole room, in any case. */
const mentionToken = /<@(all|[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12})>/gi;

/**
 * What a message not yet stored draws. Who may be mentioned is the platform's call, made when the
 * message is stored, so until then each token is plain text: the name this client already holds
 * for the person, else "someone". Never a mention piece, which would claim an admission the
 * platform has not made, and never the raw token.
 *
 * @param body The body as typed.
 * @param names Each person's name this client holds, by alias guid in lower case.
 *
 * @returns The body as text pieces.
 */
export function pendingPieces(body: string, names: ReadonlyMap<string, string>): DisplayPiece[] {
    const text = body.replace(mentionToken, (_token, target: string) => {
        const key = target.toLowerCase();
        return key === "all" ? "@all" : `@${names.get(key) ?? "someone"}`;
    });

    return [{ type: "text", text }];
}

/**
 * Whether a piece mentions the whole room. The platform marks the room's mention as such when it
 * admits it, and nothing else does, so a piece's text never makes one.
 *
 * @param piece A piece of a message's body.
 *
 * @returns True for the room's mention, @all.
 */
export function isRoomMention(piece: Readonly<Record<string, unknown>>): boolean {
    return piece.type === "mention" && piece.all === true;
}

/** A live change to a message held: an edit, a delete, or a moderator's change of visibility. */
export type MessageChangeEvent = "message.edited" | "message.deleted" | "message.visibility";

/**
 * Whether a live event is a change to a message held.
 *
 * @param event The live event's name.
 *
 * @returns True for an edit, a delete or a change of visibility.
 */
export function isMessageChange(event: string): event is MessageChangeEvent {
    return event === "message.edited" || event === "message.deleted" || event === "message.visibility";
}

/**
 * Lays an edit, a delete or a change of visibility over a message held on screen, wherever it is
 * held: the conversation's timeline or an open thread. A deleted message keeps no edit, and a
 * hidden or removed body never stays on screen. A change that arrived after a newer one of the
 * same message has the newer one's fields laid back over it, as a page read before a change does.
 *
 * @param message The message held, changed in place.
 * @param event The live event's name.
 * @param payload The live event's payload, about this message.
 * @param newer What newer changes to the message set, laid over this one.
 */
export function applyMessageChange(message: TimelineMessage, event: MessageChangeEvent, payload: unknown, newer: Partial<TimelineMessage> = {}): void {
    if (event === "message.edited") {
        const edited = payload as MessageEditedEvent;
        // an edit by someone the reader blocked moves its time and says nothing of its words
        if (message.blocked) {
            message.edited_at = edited.edited_at;
        }
        else if (!message.deleted_at) {
            message.body = edited.body;
            message.pieces = edited.pieces ?? null;
            message.edited_at = edited.edited_at;
        }
    }
    else if (event === "message.deleted") {
        message.deleted_at = (payload as MessageDeletedEvent).deleted_at;
        message.body = null;
        message.pieces = null;
        message.blocks = null;
        message.attachments = null;
        message.metadata = null;
    }
    else if (event === "message.visibility") {
        const changed = payload as MessageVisibilityEvent;
        message.visibility = changed.state;
        message.visibility_notice = changed.notice;

        // A hidden or removed body must not stay on screen. One shown again carries its body
        // back, so the row draws it without waiting for a page.
        if (changed.state !== "visible") {
            message.body = null;
            message.pieces = null;
            message.blocks = null;
            message.attachments = null;
            message.metadata = null;
        }
        else if (changed.body !== undefined && !message.deleted_at && !message.blocked) {
            // a message of files alone comes back with its body null and its files
            message.body = changed.body;
            message.pieces = changed.pieces ?? null;
            message.blocks = changed.blocks ?? null;
            message.attachments = changed.attachments ?? null;
        }
    }

    Object.assign(message, newer);
    if (message.deleted_at || (message.visibility !== undefined && message.visibility !== "visible")) {
        message.body = null;
        message.pieces = null;
        message.blocks = null;
        message.attachments = null;
        message.metadata = null;
    }
}

/**
 * Whether a message belongs on the channel's timeline: a root, or a reply also shown in the
 * channel. A reply shown only in its thread is the thread panel's.
 */
export function isTimelineMessage(message: Pick<TimelineMessage, "parent_id" | "shown_in_channel">): boolean {
    return message.parent_id === null || message.parent_id === undefined || message.shown_in_channel === true;
}

/**
 * Combines what is held for a message with a newer copy of it. A page can have been read
 * before an edit or a delete that has already arrived live, so an edit held is kept over an
 * older body and a delete held is never undone. A live copy lacks the sender's name, so a field
 * the newer copy does not carry keeps what is held.
 */
export function combine(held: TimelineMessage, incoming: TimelineMessage): TimelineMessage {
    const result: TimelineMessage = { ...held };

    for (const [key, value] of Object.entries(incoming)) {
        if (value !== undefined) {
            (result as Record<string, unknown>)[key] = value;
        }
    }

    // Whether a message is withheld for a block is each copy's own say, since the platform marks only
    // a withheld one: a newer copy without the mark, read after an unblock, shows it again.
    if (held.blocked && !incoming.blocked) {
        delete result.blocked;
    }

    // A copy withheld for a block is final about its words, as a delete is: an edit held from before
    // never puts them back.
    const isNewerEditHeld = !held.blocked && !incoming.blocked && !!held.edited_at && (!incoming.edited_at || incoming.edited_at < held.edited_at);
    if (isNewerEditHeld) {
        result.body = held.body;
        result.pieces = held.pieces;
        result.edited_at = held.edited_at;
    }

    if (held.deleted_at && !incoming.deleted_at) {
        result.deleted_at = held.deleted_at;
        result.body = null;
        result.pieces = null;
        result.blocks = null;
        result.attachments = null;
        result.metadata = null;
    }

    // The person's own send is confirmed with the text they typed, and that answer can arrive after
    // the platform hid the message, so a hide held is undone only by a copy that says so itself.
    const isHiddenHeld = held.visibility === "hidden" || held.visibility === "removed";
    if (isHiddenHeld && incoming.visibility === undefined) {
        result.visibility = held.visibility;
        result.visibility_notice = held.visibility_notice;
        result.body = null;
        result.pieces = null;
        result.blocks = null;
        result.attachments = null;
        result.metadata = null;
    }

    return result;
}

