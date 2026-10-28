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
// same key. A reply names its root and whether it is also sent to the conversation, and a quote the
// message it quotes; the row keeps both so every try sends the same message, since the platform
// compares a retry with the key's first try. A reply shown only in its thread never enters the
// conversation's timeline. The send's key travels on every copy of the message, the answer, the
// live copy and each page, so the feeds draw the row and whichever copy arrives as one message,
// and whichever copy lands first settles the send: its row goes and the send resolves, even when
// the answer is lost. A key stands for one text from its first try, since that try may have been
// stored with its answer lost; changed words are a new message under a new key.
import { onBeforeUnmount, reactive, Ref, watch } from "vue";
import { AttachmentItem, ChatError, DisplayPiece, FailureWords, MessageBlock, PendingMessage, SidebarRow, TimelineMessage } from "../types.partial";
import { sentHoldMs } from "../feedLayout.partial";
import { tokenDuration } from "../ui/brand.partial";
import { failureWords, finalWords, isOfflineFailure, isRefusedForGood, messageForError, offlineWords, readOnlySentence } from "../errors.partial";
import { sendKey, Timelines } from "./useHistory.partial";
import { ChatSettings } from "./useSession.partial";

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

/**
 * Whether a failed send can never pass on another try.
 *
 * @param error The failure that ended the try.
 * @param isFirstMessage Whether the row is a direct message's first message.
 *
 * @returns True when the row should offer Delete alone.
 */
export function isFinalRefusal(error: ChatError, isFirstMessage: boolean): boolean {
    // A conversation just started may not be on the platform yet, so "not a member" there can
    // still pass once it arrives.
    if (isFirstMessage && error.code === "authz.not_member") {
        return false;
    }

    // Read only and maintenance are not final: the box is closed until the service opens, and
    // the message can then be tried again.
    return isRefusedForGood(error);
}

/**
 * Why a try failed, in words the person can read, or null where nothing was said. A refusal for
 * good always has its sentence; the person being offline says so; any other failure says only
 * what the server wrote for it, or that the session ended.
 *
 * @param error The failure that ended the try.
 * @param isFinal Whether the same text can only be refused again.
 * @param isConnectionLost Whether the live connection is lost, where the sender is told.
 *
 * @returns The words, or null.
 */
function failureReason(error: ChatError, isFinal: boolean, isConnectionLost: boolean | undefined): FailureWords | null {
    if (isFinal) {
        return finalWords(messageForError(error));
    }

    if (isOfflineFailure(error, isConnectionLost)) {
        return offlineWords;
    }

    return error.text || error.severity === "session" ? failureWords(error) : null;
}

/** What the strip over a composer says for the person's failed sends in the place it types into. */
export type SendFailure = {
    /** How many sends failed there. */
    count: number;

    /**
     * Why, or null where nothing was said: the newest refusal's sentence, since a message refused
     * for good can only be deleted and the person must read why, else the newest failure's. Each
     * message's own reason is also on its Not sent marker.
     */
    reason: FailureWords | null;

    /** Whether any of them can be tried again; one refused for good offers only Delete. */
    canRetry: boolean;

    /** The failed sends' keys, oldest first, so the strip can tell a failure it holds as it happens. */
    localIds: string[];
};

/** The latest send to fail: its key, and how many failures the sender has had, so each is news once. */
export type SendFailureNews = {
    localId: string;
    count: number;
};

/**
 * The sends a strip's Delete drops: every one that failed.
 *
 * @param rows The pending and failed sends in one place, oldest first.
 *
 * @returns The failed ones, oldest first.
 */
export function failedSends(rows: PendingMessage[]): PendingMessage[] {
    return rows.filter(row => row.status === "failed");
}

/**
 * The sends a strip's Try again resends: every failed one not refused for good, oldest first, so
 * they land in the order they were written.
 *
 * @param rows The pending and failed sends in one place, oldest first.
 *
 * @returns The ones to send again.
 */
export function retryableSends(rows: PendingMessage[]): PendingMessage[] {
    return failedSends(rows).filter(row => !row.refusal);
}

/**
 * What the strip over a composer says, from the person's sends in the place its box types into.
 *
 * @param rows The pending and failed sends there, oldest first.
 *
 * @returns The strip's words, or null while nothing failed.
 */
export function sendFailure(rows: PendingMessage[]): SendFailure | null {
    const failed = failedSends(rows);
    if (failed.length === 0) {
        return null;
    }

    const refused = failed.filter(row => row.refusal);
    const telling = refused.length > 0 ? refused : failed;

    return {
        count: failed.length,
        reason: telling[telling.length - 1].reason ?? null,
        canRetry: retryableSends(rows).length > 0,
        localIds: failed.map(row => row.localId)
    };
}

/**
 * Whether the conversation draws a send: every message, and a reply only when it was also sent
 * there. The thread draws its replies whatever they chose.
 *
 * @param row The send.
 *
 * @returns True when the conversation's feed draws it.
 */
export function isInConversation(row: PendingMessage): boolean {
    return typeof row.parentId !== "number" || row.isShownInChannel === true;
}

/**
 * Whether the conversation's strip speaks for a send: each one the conversation draws, except a
 * reply whose thread is open in the drawer, which speaks for it there. One failure has one strip,
 * so it is said once and its Delete is in one place.
 *
 * @param row The send.
 * @param openThreadRootId The root of the thread open in the drawer, or null while none is.
 *
 * @returns True when the conversation's strip holds it.
 */
export function isOwnedByConversation(row: PendingMessage, openThreadRootId: number | null): boolean {
    return isInConversation(row) && (openThreadRootId === null || row.parentId !== openThreadRootId);
}

/** What the platform's send answers. */
export type SendResult =
    | { ok: true, id: number, createdAt: string, notice?: string | null, pieces?: DisplayPiece[] | null, blocks?: MessageBlock[] | null, attachments?: AttachmentItem[] | null }
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
 * Why the person cannot post in a room, in the server's words, which locks its composer; null
 * where they can. Refreshed service settings can close every composer before its sidebar row
 * catches up. The join bar and maintenance keep their own sentences.
 *
 * @param row The room's sidebar row, or null with no room.
 * @param service The latest service state and its banner, including for a new conversation.
 *
 * @returns The sentence, or null.
 */
export function lockedReason(row: SidebarRow | null, service?: ChatSettings["service"]): string | null {
    const code = row?.can_post_code ?? null;
    if (row?.can_post === false && code !== "authz.join_required" && code !== "rpc.maintenance") {
        return row.can_post_reason?.trim() || (code === "rpc.read_only" ? readOnlySentence : "You can't post in this conversation.");
    }

    return service?.state === "read_only" ? service.banner?.trim() || readOnlySentence : null;
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

/** Shown when a browser cannot make a send key no one else could guess, so nothing was sent. */
export const noKeySentence = "This browser cannot send messages securely. Try a current browser.";

/**
 * Whether this browser can make a send key. Without one a send makes no row, so the text has
 * nowhere to wait but the composer's box.
 *
 * @returns True when the browser has a cryptographic generator.
 */
export function canMakeSendKey(): boolean {
    const crypto = globalThis.crypto;
    return typeof crypto?.randomUUID === "function" || typeof crypto?.getRandomValues === "function";
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
        return sendKey(crypto.randomUUID());
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
    /**
     * Sends a text message to a channel, with the key that makes a retry safe, as a reply when one
     * is named, quoting a message when one is named, carrying the files uploaded for it.
     */
    send: (channelId: string, body: string, key?: string, reply?: ReplyTarget, quotedMessageId?: number, attachments?: AttachmentItem[]) => Promise<SendResult>;

    /**
     * Uploads a send's files again when the platform no longer holds an upload, answering the
     * files under their new uploads, or null when they could not be. Without it the refusal stands.
     */
    reupload?: (channelId: string, attachments: AttachmentItem[]) => Promise<AttachmentItem[] | null>;

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

    /** Whether the live connection is lost, which decides whether a send that reached nothing says the person is offline. */
    isOffline?: () => boolean;

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
     * Sends a message. Resolves true as soon as any copy of it reaches the page, or the platform
     * answers it. Blank text sends nothing. A try names the row it shares with the message's other
     * tries, and sends that row's own text whatever text it is given; a reply names its root, and
     * a quote the message it quotes.
     */
    send: (channelId: string, body: string, attempt?: SendTry, reply?: ReplyTarget, quotedMessageId?: number, attachments?: AttachmentItem[]) => Promise<boolean>;

    /**
     * Sends failed rows again, one at a time and oldest first, each once the one before it is
     * posted, so they land in the order they were written. Every one shows as sending at once; a
     * failure stops there, and the rows after it stay failed as they were, since sending them
     * would land them ahead of it. A message written in the same room meanwhile waits until they
     * are done. A row refused for good, or not failed, is left out. Resolves true when every one
     * went, false when one failed or none could go.
     */
    retryAll: (localIds: string[]) => Promise<boolean>;

    /** The latest send to fail, or null before any has. */
    lastFailure: () => SendFailureNews | null;

    /** Drops a failed row. */
    discard: (localId: string) => void;

    /**
     * Every key this session has sent under, kept for the session: a few dozen bytes a message.
     * A message carrying one of them already rose in as its pending row, so a feed that draws it
     * again, in another group, does not raise it a second time.
     */
    sentKeys: ReadonlySet<string>;

    /**
     * A copy of a message reached the page: put in a timeline, or in the open thread. One sent
     * under a key of this session settles that send.
     */
    landed: (message: Pick<TimelineMessage, "id" | "client_key">) => void;
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
    const failures = reactive<{ last: SendFailureNews | null }>({ last: null });

    // The id each key's message landed under. A key in here is posted, whatever an answer still
    // on its way says: the answer may have been lost, or refused a replay of a different text.
    const landedKeys = new Map<string, number>();

    // The try in flight under each key, ended the moment a copy of its message lands.
    const inFlight = new Map<string, () => void>();

    // Each room's failed messages going again, in order, while they go: a message written then
    // waits behind them, so it lands after them as it was written after them.
    const retrying = new Map<string, Promise<unknown>>();
    const waitBeforeResend = dependencies.waitBeforeResend
        ?? ((attempt: number): Promise<void> => new Promise(resolve => setTimeout(resolve, 250 * 2 ** attempt)));

    /** Sends a row's text once, under its key. */
    async function sendOnce(row: PendingMessage): Promise<SendResult> {
        // A send that rejects is a failure like any other, so its row ends failed with a Retry
        // and a Discard rather than sending for good.
        try {
            // read off the row, so a retry sends the same reply the first try did
            const reply = typeof row.parentId === "number" ? { parentId: row.parentId, isShownInChannel: row.isShownInChannel === true } : undefined;
            return typeof row.quotedMessageId === "number"
                ? await dependencies.send(row.channelId, row.body, row.localId, reply, row.quotedMessageId)
                : await dependencies.send(row.channelId, row.body, row.localId, reply);
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

    /**
     * Lets a row go once its message is on the page, naming the id it became, since the row's
     * going is the only sign of which message it became.
     */
    function settle(row: PendingMessage, messageId: number): void {
        row.confirmedId = messageId;
        const index = rows.indexOf(row);
        if (index >= 0) {
            rows.splice(index, 1);
        }
    }

    /**
     * Sends a row's text and settles the row at whichever comes first: the answer, or a copy of
     * its message landing. The answer still goes on after a copy settled the row, so what only
     * it carries, the server's notice, reaches the message.
     */
    async function deliver(row: PendingMessage, isLast = true): Promise<boolean> {
        row.status = "sending";
        row.errorCode = null;
        delete row.refusal;
        delete row.reason;

        let endTry!: () => void;
        const copyLanded = new Promise<boolean>(resolve => endTry = () => resolve(true));
        inFlight.set(row.localId, endTry);

        try {
            return await Promise.race([answer(row, isLast), copyLanded]);
        }
        finally {
            // a later try under the same key has its own
            if (inFlight.get(row.localId) === endTry) {
                inFlight.delete(row.localId);
            }
        }
    }

    /** Sends a row's text and settles the row by the platform's answer. */
    async function answer(row: PendingMessage, isLast: boolean): Promise<boolean> {
        let result = await sendOnce(row);
        if (!result.ok && result.error.code === joinRequiredCode && dependencies.join) {
            result = await joinAndResend(row, dependencies.join);
        }

        if (!result.ok) {
            // A copy already landed, so the message is posted: the answer was lost after the
            // commit, or a replay was refused, and the row has already gone.
            if (landedKeys.has(row.localId)) {
                return true;
            }
            // Another try is coming on its own, so the row is not offered for a retry yet.
            if (!isLast) {
                return false;
            }
            row.status = "failed";
            row.errorCode = result.error.code;
            failures.last = { localId: row.localId, count: (failures.last?.count ?? 0) + 1 };
            const isFinal = isFinalRefusal(result.error, row.isFirstMessage === true);
            if (isFinal) {
                row.refusal = result.error;
            }
            const reason = failureReason(result.error, isFinal, dependencies.isOffline?.());
            if (reason) {
                row.reason = reason;
            }
            return false;
        }

        // The confirmed message goes in before the row comes out, so the person never sees
        // their message disappear between the two.
        const isReply = typeof row.parentId === "number";
        const confirmed: TimelineMessage = {
            id: result.id,
            ...(isReply ? { parent_id: row.parentId, shown_in_channel: row.isShownInChannel === true } : {}),
            // named at once, so the quote draws from what is held before the live copy brings its preview
            ...(typeof row.quotedMessageId === "number" ? { quoted_message_id: row.quotedMessageId } : {}),
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

        // Where the timelines and the thread report what lands this has settled already; it is
        // said here too, so a send with no thread open to hold its reply still settles.
        landed(confirmed);
        return true;
    }

    /** A copy of a message reached the page; one of this session's sends settles. */
    function landed(message: Pick<TimelineMessage, "id" | "client_key">): void {
        const key = sendKey(message.client_key);
        if (!key || !sentKeys.has(key)) {
            return;
        }

        if (!landedKeys.has(key)) {
            landedKeys.set(key, message.id);
        }
        const row = rows.find(r => r.localId === key);
        if (row) {
            settle(row, message.id);
        }
        inFlight.get(key)?.();
    }

    return {
        pending: (channelId: string): PendingMessage[] => rows.filter(row => row.channelId === channelId),

        send: async (channelId: string, body: string, attempt?: SendTry, reply?: ReplyTarget, quotedMessageId?: number): Promise<boolean> => {
            if (body.trim() === "") {
                return false;
            }

            // The key a caller kept is matched in the case the sender stores it in.
            const attemptKey = sendKey(attempt?.localId);

            // A key already on the page is posted; another try would only ask for it again.
            if (attemptKey && landedKeys.has(attemptKey)) {
                return true;
            }

            // A later try sends the same row again with its own text: the first try may have been
            // stored with its answer lost, and other words under its key would be refused or posted
            // beside it. A row the person discarded in the meantime is gone, and its key still
            // keeps a repeat from posting twice.
            const earlier = attemptKey ? rows.find(r => r.localId === attemptKey) : undefined;
            if (earlier) {
                return deliver(earlier, attempt?.isLast);
            }

            // Nothing is sent without a key no one else could guess.
            const localId = attemptKey ?? sendKey(dependencies.newLocalId());
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
                // only a direct message's first message is sent with a try of its own
                ...(attempt ? { isFirstMessage: true } : {}),
                ...(reply ? { parentId: reply.parentId, isShownInChannel: reply.isShownInChannel } : {}),
                ...(typeof quotedMessageId === "number" ? { quotedMessageId } : {})
            });

            // The row read back from the list is the reactive one, so the status the person
            // sees follows every change made to it. It shows as sending while it waits its turn.
            const row = rows[rows.length - 1];
            const ahead = retrying.get(channelId);
            if (ahead) {
                await ahead;
            }
            return deliver(row, attempt?.isLast);
        },

        retryAll: async (localIds: string[]): Promise<boolean> => {
            // read in the rows' own order, which is the order they were written
            const keys = new Set(localIds.map(sendKey));
            const queue = rows.filter(r => keys.has(r.localId) && r.status === "failed" && !r.refusal);
            if (queue.length === 0) {
                return false;
            }

            // Each waiting row keeps its last reason until its own try clears it.
            queue.forEach(row => row.status = "sending");
            const drain = (async (): Promise<boolean> => {
                for (let index = 0; index < queue.length; index++) {
                    const row = queue[index];

                    // a copy of its first try landed while it waited, so it is posted already
                    if (!rows.includes(row)) {
                        continue;
                    }

                    if (!await deliver(row)) {
                        queue.slice(index + 1).filter(r => rows.includes(r)).forEach(r => r.status = "failed");
                        return false;
                    }
                }

                return true;
            })();

            const channelIds = new Set(queue.map(row => row.channelId));
            channelIds.forEach(channelId => retrying.set(channelId, drain));
            try {
                return await drain;
            }
            finally {
                channelIds.forEach(channelId => {
                    if (retrying.get(channelId) === drain) {
                        retrying.delete(channelId);
                    }
                });
            }
        },

        lastFailure: (): SendFailureNews | null => failures.last,

        discard: (localId: string): void => {
            const index = rows.findIndex(r => r.localId === sendKey(localId) && r.status === "failed");
            if (index >= 0) {
                rows.splice(index, 1);
            }
        },

        sentKeys,
        landed
    };
}

/**
 * The keys of the person's sends that settled while a view was open, each held for a moment so
 * its message shows "Sent" before it settles to its time. The conversation and the thread drawer
 * each hold their own, over the rows they draw. By key, since every copy of the message carries
 * it, and from the first copy to land, so a later copy neither restarts nor ends the hold: a row
 * goes with the id of the message it became, and one discarded goes with none.
 *
 * @param pending The view's pending rows, read again as they change.
 * @param element An element inside the chat, whose sheet names the hold's length.
 *
 * @returns The keys showing "Sent" now.
 */
export function useSentHold(pending: () => PendingMessage[], element: Ref<HTMLElement | null>): ReadonlySet<string> {
    const keys = reactive(new Set<string>());
    const timers = new Set<ReturnType<typeof setTimeout>>();

    watch(pending, (rows, previous) => {
        const kept = new Set(rows.map(row => row.localId));
        for (const row of previous ?? []) {
            if (kept.has(row.localId) || row.confirmedId === undefined) {
                continue;
            }

            keys.add(row.localId);
            const hold = element.value ? tokenDuration(element.value, "--chat-d-sent-hold", sentHoldMs) : sentHoldMs;
            const timer = setTimeout(() => {
                timers.delete(timer);
                keys.delete(row.localId);
            }, hold);
            timers.add(timer);
        }
    }, { flush: "sync" });

    onBeforeUnmount(() => timers.forEach(timer => clearTimeout(timer)));

    return keys;
}
