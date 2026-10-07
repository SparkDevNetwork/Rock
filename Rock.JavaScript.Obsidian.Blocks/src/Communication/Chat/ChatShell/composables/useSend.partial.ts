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
// is matched by its id.
import { reactive } from "vue";
import { ChatError, PendingMessage } from "../types.partial";
import { Timelines } from "./useHistory.partial";

/** What the platform's send answers. */
export type SendResult =
    | { ok: true, id: number, createdAt: string, notice?: string | null }
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
 * a UUID, so a browser without randomUUID still gets one of that shape.
 *
 * @returns A version 4 UUID.
 */
export function newLocalId(): string {
    if (typeof globalThis.crypto?.randomUUID === "function") {
        return globalThis.crypto.randomUUID();
    }

    return "xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx".replace(/[xy]/g, c => {
        const value = Math.floor(Math.random() * 16);
        return (c === "x" ? value : (value & 0x3) | 0x8).toString(16);
    });
}

/** What the sender reaches outside itself. */
export type SenderDependencies = {
    /** Sends a text message to a channel, with the key that makes a retry safe. */
    send: (channelId: string, body: string, key?: string) => Promise<SendResult>;

    /** The timelines a confirmed message is put into. */
    timelines: Pick<Timelines, "upsert">;

    /** The person sending, as the platform knows them. */
    personAliasGuid: string;

    /** A fresh identifier for a pending row, a UUID, since it is also the send's key. */
    newLocalId: () => string;
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
     * A try names the row it shares with the message's other tries.
     */
    send: (channelId: string, body: string, attempt?: SendTry) => Promise<boolean>;

    /** Sends a failed row's text again. */
    retry: (localId: string) => Promise<boolean>;

    /** Drops a failed row. */
    discard: (localId: string) => void;
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

    /** Sends a row's text and settles the row by the answer. */
    async function deliver(row: PendingMessage, isLast = true): Promise<boolean> {
        row.status = "sending";
        row.errorCode = null;

        // A send that rejects is a failure like any other, so its row ends failed with a Retry
        // and a Discard rather than sending for good.
        let result: SendResult;
        try {
            result = await dependencies.send(row.channelId, row.body, row.localId);
        }
        catch {
            result = { ok: false, error: { code: "rpc.transport", severity: "failed" } };
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
        dependencies.timelines.upsert(row.channelId, {
            id: result.id,
            person_alias_guid: dependencies.personAliasGuid,
            sender_listed: true,
            message_type: "text",
            body: row.body,
            created_at: result.createdAt,
            notice: result.notice ?? null
        });

        const index = rows.indexOf(row);
        if (index >= 0) {
            rows.splice(index, 1);
        }

        return true;
    }

    return {
        pending: (channelId: string): PendingMessage[] => rows.filter(row => row.channelId === channelId),

        send: async (channelId: string, body: string, attempt?: SendTry): Promise<boolean> => {
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

            rows.push({ localId: attempt?.localId ?? dependencies.newLocalId(), channelId, body, status: "sending", errorCode: null });

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
        }
    };
}
