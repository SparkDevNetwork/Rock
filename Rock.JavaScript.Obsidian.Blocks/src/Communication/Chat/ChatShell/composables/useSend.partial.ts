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
// Sending. A message shows at once as the person's own pending row and becomes a timeline
// message when the platform confirms it with its id. A failed send keeps its text on the row so
// the person can send it again; nothing typed is lost to a failure. Each row's identifier goes
// with every try as the send's key, so a retry after a lost confirmation is answered with the
// message the first try posted rather than posting it twice; the live echo of a confirmed send
// is matched by its id. A send into a public or pinned room the person has not joined is refused
// until they join, which is Rock's door, so the sender joins through it and sends again under the
// same key. A reply names its root and whether it is also sent to the conversation, and keeps both
// on its row so every try sends the same reply; one shown only in its thread never enters the
// conversation's timeline. The send's key travels on every copy of the message, the answer, the
// live copy and each page, so the feeds draw the row and whichever copy arrives as one message.
import { reactive } from "vue";
import { ChatError, DisplayPiece, MessageBlock, PendingMessage, TimelineMessage } from "../types.partial";
import { Timelines } from "./useHistory.partial";

/** What Rock's Join door answers. */
export type JoinResult = {
    code?: string | null;
    message?: string | null;
    isPending: boolean;
};

/** The refusal of a send into a room the person may join and has not. */
const joinRequiredCode = "authz.join_required";

/**
 * How many times a send waits and tries again while a join is still reaching the platform, after
 * the first send that follows the join: five sends in all.
 */
const joinWaits = 4;

/** What the platform's send answers. */
export type SendResult =
    | { ok: true, id: number, createdAt: string, notice?: string | null, pieces?: DisplayPiece[] | null, blocks?: MessageBlock[] | null }
    | { ok: false, error: ChatError };

/**
 * Whether the composer takes a message in this service state. The database refuses a write in
 * read only and maintenance whatever the client does; closing the composer only saves the
 * person typing something that cannot be sent.
 *
 * @param serviceState The service state from the settings.
 *
 * @returns True when a message can be written.
 */
export function isComposerOpen(serviceState: string): boolean {
    return serviceState !== "read_only" && serviceState !== "maintenance";
}

/**
 * What the composer's box holds after it hands its text over. A failed ordinary send keeps its
 * text on its own row with a retry, so the box empties; a draft's first message has no row in a
 * feed yet, so the box keeps it while the shell says so.
 *
 * @param text The text handed over.
 * @param isTextKept Whether the shell keeps it in the box.
 *
 * @returns The text left in the box.
 */
export function textAfterSend(text: string, isTextKept: boolean): string {
    return isTextKept ? text : "";
}

/**
 * A fresh identifier for a pending row. It is also the send's key, which the platform stores as
 * a UUID and every copy of the message carries, live copies included, so other members of the
 * room see it. Only the browser's cryptographic generator makes one: a key anyone could predict
 * would let them take a person's next key first.
 *
 * @returns A version 4 UUID, or null when the browser has no cryptographic generator.
 */
export function newLocalId(): string | null {
    const crypto = globalThis.crypto;
    if (typeof crypto?.randomUUID === "function") {
        return crypto.randomUUID();
    }

    if (typeof crypto?.getRandomValues !== "function") {
        return null;
    }

    const bytes = crypto.getRandomValues(new Uint8Array(16));
    // The version and variant bits of a version 4 UUID, as RFC 9562 sets them.
    bytes[6] = (bytes[6] & 0x0f) | 0x40;
    bytes[8] = (bytes[8] & 0x3f) | 0x80;
    const hex = Array.from(bytes, b => b.toString(16).padStart(2, "0")).join("");

    return `${hex.slice(0, 8)}-${hex.slice(8, 12)}-${hex.slice(12, 16)}-${hex.slice(16, 20)}-${hex.slice(20)}`;
}

/** A reply's place: the root it answers, and whether it is also sent to the conversation. */
export type ReplyTarget = {
    parentId: number;
    isShownInChannel: boolean;
};

/** What the sender reaches outside itself. */
export type SenderDependencies = {
    /** Sends a text message to a channel, with the key that makes a retry safe, as a reply when one is named. */
    send: (channelId: string, body: string, key?: string, reply?: ReplyTarget) => Promise<SendResult>;

    /** The timelines a confirmed message is put into. */
    timelines: Pick<Timelines, "upsert">;

    /** A confirmed reply, for the open thread to put in place without waiting for its live copy. */
    onReplyConfirmed?: (channelId: string, message: TimelineMessage) => void;

    /**
     * Rock's Join door, for a send refused until the person joins a public or pinned room.
     * Without it the refusal stands.
     */
    join?: (channelId: string) => Promise<JoinResult>;

    /**
     * Waits before a send is tried again while a join is still reaching the platform; a short
     * doubling wait by default.
     */
    waitBeforeResend?: (attempt: number) => Promise<void>;

    /** The person sending, as the platform knows them. */
    personAliasGuid: string;

    /** A fresh identifier for a pending row, a UUID, since it is also the send's key; null when none can be made. */
    newLocalId: () => string | null;
};

/** How one try of a message that is tried again on its own is sent. */
export type SendTry = {
    /** The row every try shares, so they carry one key and the person sees one message. */
    localId: string;

    /** False while another try is still to come, so a failure keeps the row sending. */
    isLast: boolean;
};

/** The sender a shell holds. */
export type Sender = {
    /** The person's pending and failed rows in a channel, oldest first. */
    pending: (channelId: string) => PendingMessage[];

    /**
     * Sends a message. Resolves true when the platform confirmed it. Blank text sends nothing.
     * A try names the row it shares with the message's other tries; a reply names its root.
     */
    send: (channelId: string, body: string, attempt?: SendTry, reply?: ReplyTarget) => Promise<boolean>;

    /** Sends a failed row's text again. */
    retry: (localId: string) => Promise<boolean>;

    /** Drops a failed row. */
    discard: (localId: string) => void;

    /**
     * Every key this session has sent under, kept for the session: a few dozen bytes a message.
     * A message carrying one of them already rose in as its pending row, so a feed that draws it
     * again, in another group, does not raise it a second time.
     */
    sentKeys: ReadonlySet<string>;
};

/**
 * Creates the sender.
 *
 * @param dependencies What the sender reaches outside itself.
 *
 * @returns The sender.
 */
export function createSender(dependencies: SenderDependencies): Sender {
    const rows = reactive<PendingMessage[]>([]) as PendingMessage[];
    const sentKeys = new Set<string>();
    const waitBeforeResend = dependencies.waitBeforeResend
        ?? ((attempt: number): Promise<void> => new Promise(resolve => setTimeout(resolve, 250 * 2 ** attempt)));

    /** Sends a row's text once, under its key. */
    async function sendOnce(row: PendingMessage): Promise<SendResult> {
        // A send that rejects is a failure like any other, so its row ends failed with a Retry
        // and a Discard rather than sending for good.
        try {
            // read off the row, so a retry sends the same reply the first try did
            const reply = typeof row.parentId === "number" ? { parentId: row.parentId, isShownInChannel: row.isShownInChannel === true } : undefined;
            return await dependencies.send(row.channelId, row.body, row.localId, reply);
        }
        catch {
            return { ok: false, error: { code: "rpc.transport", severity: "failed" } };
        }
    }

    /**
     * Joins the row's room through Rock and sends again under the same key. A join Rock made
     * but the platform has not taken yet is refused as not joined, so the send waits and tries
     * again a few times before the row fails.
     */
    async function joinAndResend(row: PendingMessage, join: (channelId: string) => Promise<JoinResult>): Promise<SendResult> {
        let joined: JoinResult;
        try {
            joined = await join(row.channelId);
        }
        catch {
            return { ok: false, error: { code: "door.unreachable", severity: "failed" } };
        }

        if (joined.code !== "ok") {
            return { ok: false, error: { code: joined.code ?? "door.unavailable", severity: "permission", text: joined.message ?? null } };
        }

        let result = await sendOnce(row);
        for (let attempt = 0; attempt < joinWaits && !result.ok && result.error.code === joinRequiredCode; attempt++) {
            await waitBeforeResend(attempt);
            result = await sendOnce(row);
        }

        return result;
    }

    /** Sends a row's text and settles the row by the answer. */
    async function deliver(row: PendingMessage, isLast = true): Promise<boolean> {
        row.status = "sending";
        row.errorCode = null;

        let result = await sendOnce(row);
        if (!result.ok && result.error.code === joinRequiredCode && dependencies.join) {
            result = await joinAndResend(row, dependencies.join);
        }

        if (!result.ok) {
            // Another try is coming on its own, so the row is not offered for a retry yet.
            if (!isLast) {
                return false;
            }
            row.status = "failed";
            row.errorCode = result.error.code;
            return false;
        }

        // The confirmed message goes in before the row comes out, so the person never sees
        // their message disappear between the two.
        const isReply = typeof row.parentId === "number";
        const confirmed: TimelineMessage = {
            id: result.id,
            ...(isReply ? { parent_id: row.parentId, shown_in_channel: row.isShownInChannel === true } : {}),
            person_alias_guid: dependencies.personAliasGuid,
            sender_listed: true,
            message_type: "text",
            body: row.body,
            // the pieces the server built, so a mention shows as one before the event arrives
            pieces: result.pieces ?? null,
            blocks: result.blocks ?? null,
            created_at: result.createdAt,
            notice: result.notice ?? null,
            client_key: row.localId
        };
        if (isReply) {
            dependencies.onReplyConfirmed?.(row.channelId, confirmed);
        }
        // a reply shown only in its thread is the thread's, never a row of the conversation
        if (!isReply || row.isShownInChannel === true) {
            dependencies.timelines.upsert(row.channelId, confirmed);
        }

        // Named on the row as it goes, since the row's going is the only sign of which message
        // it became.
        row.confirmedId = result.id;
        const index = rows.indexOf(row);
        if (index >= 0) {
            rows.splice(index, 1);
        }

        return true;
    }

    return {
        pending: (channelId: string): PendingMessage[] => rows.filter(row => row.channelId === channelId),

        send: async (channelId: string, body: string, attempt?: SendTry, reply?: ReplyTarget): Promise<boolean> => {
            if (body.trim() === "") {
                return false;
            }

            // A later try sends the same row again, with the text it is given, since a person
            // retrying from a draft may have changed it; a row the person discarded or sent from
            // in the meantime is gone, and its key still keeps a repeat from posting twice.
            const earlier = attempt ? rows.find(r => r.localId === attempt.localId) : undefined;
            if (earlier) {
                earlier.body = body;
                return deliver(earlier, attempt?.isLast);
            }

            // Nothing is sent without a key no one else could guess.
            const localId = attempt?.localId ?? dependencies.newLocalId();
            if (!localId) {
                return false;
            }
            sentKeys.add(localId);

            rows.push({
                localId,
                channelId,
                body,
                status: "sending",
                errorCode: null,
                ...(reply ? { parentId: reply.parentId, isShownInChannel: reply.isShownInChannel } : {})
            });

            // The row read back from the list is the reactive one, so the status the person
            // sees follows every change made to it.
            return deliver(rows[rows.length - 1], attempt?.isLast);
        },

        retry: async (localId: string): Promise<boolean> => {
            const row = rows.find(r => r.localId === localId);

            if (!row || row.status !== "failed") {
                return false;
            }

            return deliver(row);
        },

        discard: (localId: string): void => {
            const index = rows.findIndex(r => r.localId === localId && r.status === "failed");
            if (index >= 0) {
                rows.splice(index, 1);
            }
        },

        sentKeys
    };
}
