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
// What a message row shows, worked out once for the row and the look it draws in: its state, its
// text and pieces, who sent it, its time, and whether it carries a thread, a quote or chips. The
// row reads these for its own element and lends them to its look as one view, beside what can be
// done to the message and the handlers that ask for it, so the two looks draw the same facts and
// neither works any of them out again. A fact only one look draws lives in that look.

import { ComponentPublicInstance, computed, inject, Ref, UnwrapNestedRefs, watch } from "vue";
import { isRoomMention, MessageDisplay, messageDisplay, pendingPieces } from "./useHistory.partial";
import { MessageRowRequests } from "./useMessageActions.partial";
import { ReactionReader } from "./useReactions.partial";
import type { ReactionMotion } from "../messageMotion.partial";
import { RowPosition, timeLabel } from "../feedLayout.partial";
import { problemReportKey } from "../problemReport.partial";
import { AttachmentItem, ChatError, DisplayPiece, PendingMessage, ReactorsCursor, ReactorsPage, TimelineMessage } from "../types.partial";

/** What the row is drawing, which is also the marker the styles and the screenshots key on. */
export type RowState = "message" | "sending" | "sent" | "failed" | "deleted" | "hidden" | "removed" | "unknown" | "system" | "card" | "blocked";

/** One face, in the avatar's own shape. */
export type Face = { name: string, photoUrl: string | null };

/** The row's props the facts are worked out from. */
export type MessageRowViewSource = {
    /** The stored message, or null for a send that is not stored yet. */
    message: TimelineMessage | null;

    /** The person's own send that is not stored yet, or null for a stored message. */
    pending: PendingMessage | null;

    isOwn: boolean;

    /** The reader's alias guid, for the "you" mention chip. */
    personAliasGuid: string;

    /** The reader's own name, for their messages the platform sent no name with. */
    personName: string;

    /** The reader's own photo, for their face. */
    personPhotoUrl: string | null;

    /** The church's name, the sender of a block post nobody signed. */
    organizationName: string;

    /** A channel's message is a flat Community row; a direct message's is a Conversational bubble. */
    isChannel: boolean;

    /** The row's place in its group. */
    position: RowPosition;

    /** The person's own send was just confirmed, so "Sent" shows for a moment. */
    isJustSent: boolean;

    /** A send still going has taken long enough to say "Sending…". */
    isSendingSlow: boolean;

    /** Each person's name this client holds, by alias guid in lower case, for a pending body's mentions. */
    senderNames: ReadonlyMap<string, string>;

    /** Drawn in the thread drawer, where a thread pill or label would only open the thread on screen. */
    isInThread: boolean;

    /** A room the person has not joined. */
    isPreviewing: boolean;
};

/** The reactions row's props, the same in both looks. */
export type MessageRowReactionsProps = {
    message: TimelineMessage;
    isPreviewing: boolean;
    isPhone: boolean;
    reader: ReactionReader;
    listReactors: (code: string | null, after: ReactorsCursor | null) => Promise<ReactorsPage | null>;
    isCardDisabled: boolean;
    motion: ReactionMotion | null;
};

/** What the row lends its look of what can be done to the message and of the row's own motion. */
export type MessageRowSurface = {
    /** The editor takes the text's place. */
    isEditorShown: Readonly<Ref<boolean>>;

    /** The row lends the column's bar a dock, so the look draws one. */
    isBarOffered: Readonly<Ref<boolean>>;

    /** The refusal the editor's last save met. */
    editError: Readonly<Ref<ChatError | null>>;

    /** The editor's save is out, so Save is held. */
    isEditSaving: Readonly<Ref<boolean>>;

    reactionsProps: Readonly<Ref<MessageRowReactionsProps>>;

    /** An edit, a delete or a hide just replaced the text, so it crossfades once. */
    isCrossfading: Readonly<Ref<boolean>>;

    /** An edit landed while the row was on screen, so its "Edited" slides in once. */
    isEditedNow: Readonly<Ref<boolean>>;

    /** The message's thread started while the row was on screen, so its pill arrives. */
    isThreadStartedHere: Readonly<Ref<boolean>>;
};

/** Everything a look reads: the facts and what the row lends it, each read as a plain value. */
export type MessageRowView = Readonly<UnwrapNestedRefs<ReturnType<typeof useMessageRowView> & MessageRowSurface>>;

/** What a look calls, every request wired once by the row. */
export type MessageRowHandlers = {
    /** Asks for the author's card beside the name or face pressed. */
    author: (event: MouseEvent) => void;

    /** Asks for a person's card, from a chip in the body, beside what was pressed. */
    person: (personAliasGuid: string, anchor: HTMLElement) => void;

    /** Opens the thread the message belongs to, from its pill or its "Replied to a thread" label. */
    openThread: () => void;

    /** Tries the failed send again. */
    retry: () => void;

    /** Drops the failed send. */
    discard: () => void;

    /** Goes to the message a quote names. */
    jump: (quotedMessageId: number) => void;

    editor: MessageRowRequests["editor"];

    reactions: MessageRowRequests["reactions"];

    runBlockAction: MessageRowRequests["runBlockAction"];

    /** Where the look draws the bar's dock, handed to the row as the element mounts and goes. */
    setDock: (element: Element | ComponentPublicInstance | null) => void;
};

/** One to six emoji and nothing else, joined sequences and skin tones included. */
const jumboPattern = new RegExp("^(?:\\p{Extended_Pictographic}|\\p{Emoji_Modifier}|\\u{FE0F}|\\u{200D})+$", "u");
const pictograph = new RegExp("\\p{Extended_Pictographic}", "gu");

/**
 * Works out what a message row shows, each fact its own computed value, so a change wakes only
 * the parts that read it, as it would were each declared in the row.
 *
 * @param props The row's props.
 *
 * @returns The facts, each a computed value.
 */
// eslint-disable-next-line @typescript-eslint/explicit-function-return-type
export function useMessageRowView(props: Readonly<MessageRowViewSource>) {
    /** When a pending row was first drawn, the time it shows until the platform gives it one. */
    const drawnAt = new Date();

    const message = computed((): TimelineMessage | null => props.message);

    const isPending = computed((): boolean => !!props.pending);

    const isOwn = computed((): boolean => props.isOwn);

    const personAliasGuid = computed((): string => props.personAliasGuid);

    /** What the server decided the message shows; a pending send is its own text. */
    const display = computed((): MessageDisplay => props.message
        ? messageDisplay(props.message)
        : { kind: "message", text: props.pending?.body ?? "" });

    const text = computed((): string | null => display.value.text);

    /** The files the message carries, none where its text is withheld; a send still going, those it was given. */
    const files = computed((): AttachmentItem[] => {
        if (props.pending) {
            return props.pending.attachments ?? [];
        }

        return display.value.kind === "message" ? props.message?.attachments ?? [] : [];
    });

    const isBlockPost = computed((): boolean => Array.isArray(props.message?.blocks) && (props.message?.blocks?.length ?? 0) > 0);

    /**
     * Why a failed send failed, where anything was said. The strip over the composer says one
     * reason for all of a place's failures, so each message keeps its own on its marker, for a
     * screen reader and a pointer resting on it.
     */
    const failedReason = computed((): string | null => props.pending?.status === "failed" ? props.pending.reason?.alone ?? null : null);

    /** Try again is offered unless the send was refused for good, which the same text under the same key can only meet again. */
    const canRetryFailed = computed((): boolean => !props.pending?.refusal);

    const state = computed((): RowState => {
        if (props.pending) {
            return props.pending.status === "failed" ? "failed" : "sending";
        }

        const message = props.message;
        if (!message) {
            return "message";
        }

        if (display.value.kind === "blocked") {
            return "blocked";
        }

        if (display.value.kind === "deleted") {
            return "deleted";
        }

        if (display.value.kind === "notice") {
            return message.visibility === "removed" ? "removed" : "hidden";
        }

        if (isBlockPost.value) {
            return "card";
        }

        if (message.message_type === "system") {
            return "system";
        }

        // A type a newer platform sends is drawn by the text it carries, marked so it reads as such.
        if (message.message_type !== "text" && message.message_type !== "announcement") {
            return "unknown";
        }

        return props.isJustSent ? "sent" : "message";
    });

    const isTomb = computed((): boolean => state.value === "deleted" || state.value === "hidden" || state.value === "removed");

    const isAnnouncement = computed((): boolean => !isTomb.value && props.message?.message_type === "announcement");

    const isEdited = computed((): boolean => !isTomb.value && !props.pending && !!props.message?.edited_at);

    const isHead = computed((): boolean => props.position === "first" || props.position === "single");

    /** A send says "Sending…" only once it has gone quiet; a quick one goes straight to "Sent". */
    const isSendingShown = computed((): boolean => state.value === "sending" && props.isSendingSlow);

    /** The server's pieces, which only a stored message that is shown has. */
    const serverPieces = computed((): DisplayPiece[] | null => {
        const kind = display.value.kind;
        return props.message && (kind === "message" || kind === "system") ? props.message.pieces ?? null : null;
    });

    /**
     * What the body draws. A send not stored yet has no pieces from the server, so its mention
     * tokens are drawn as the names this client holds, never as chips and never raw.
     */
    const pieces = computed((): DisplayPiece[] | null => props.pending
        ? pendingPieces(props.pending.body, props.senderNames)
        : serverPieces.value);

    /** The sender's name from the message, else the reader's own name for their own message. */
    const displayName = computed((): string => {
        const message = props.message;
        const name = [message?.sender_nick_name, message?.sender_last_name].filter(part => !!part).join(" ");

        if (name) {
            return name;
        }

        // The reader's own row, a send not stored yet above all, carries no sender of its own and
        // is never the church's, whatever the session left out.
        if (props.isOwn) {
            return props.personName || "You";
        }

        if (!message?.person_alias_guid && props.organizationName) {
            return props.organizationName;
        }

        return "Someone";
    });

    /**
     * The author whose card the name and face open: a stored message's sender. A send not stored
     * yet, a line from the church and a sender the church no longer lists name no one to open.
     */
    const authorGuid = computed((): string | null => {
        const message = props.message;
        return message?.person_alias_guid && message.sender_listed !== false && !message.blocked ? message.person_alias_guid : null;
    });

    const face = computed((): Face => ({
        name: displayName.value,
        photoUrl: props.message?.sender_avatar_url ?? (props.isOwn ? props.personPhotoUrl : null)
    }));

    /**
     * When the message was sent, or when a send not stored yet was first drawn. Kept here rather
     * than in a look, since a look is drawn again when the conversation's kind becomes known and
     * a send's first instant must survive it.
     */
    const at = computed((): Date => {
        const created = props.message ? new Date(props.message.created_at) : drawnAt;
        return isNaN(created.getTime()) ? drawnAt : created;
    });

    const time = computed((): string => timeLabel(at.value));

    /**
     * Whom the message names that reaches the reader: "you" by name, else "everyone" by @all, else
     * nothing. Never the reader's own message, nor one deleted; and never "everyone" for a person
     * only previewing the room, who is not among them until they join.
     */
    const mentionOfReader = computed((): "you" | "everyone" | null => {
        const me = props.personAliasGuid.toLowerCase();
        if (props.isOwn || isTomb.value) {
            return null;
        }

        const mentions = (serverPieces.value ?? []).filter(piece => piece.type === "mention");
        if (!!me && mentions.some(piece => typeof piece.person_alias_guid === "string" && piece.person_alias_guid.toLowerCase() === me)) {
            return "you";
        }

        return !props.isPreviewing && mentions.some(isRoomMention) ? "everyone" : null;
    });

    /**
     * Community's "Mentioned you" or "Mentioned everyone", on a message only. A direct message's
     * bubbles draw neither it nor the wash: the whole conversation reaches the reader, and the
     * chip already says who was named. A system line is a quiet note of what happened and a card a
     * post from the church, neither a message to the reader, so they draw neither either.
     */
    const readerMention = computed((): "you" | "everyone" | null => props.isChannel
        && state.value !== "system"
        && state.value !== "card"
        ? mentionOfReader.value
        : null);

    /** One to six emoji and nothing else draws large, from the text the body would draw. */
    const isJumbo = computed((): boolean => {
        if (isTomb.value || state.value === "system" || state.value === "card") {
            return false;
        }

        const drawn = pieces.value;
        const text = (drawn && drawn.length > 0
            ? drawn.map(piece => piece.type === "emoji" && typeof piece.emoji === "string" ? piece.emoji : piece.text).join("")
            : display.value.text ?? "").trim();

        if (!text || !jumboPattern.test(text)) {
            return false;
        }

        const count = text.match(pictograph)?.length ?? 0;
        return count >= 1 && count <= 6;
    });

    /**
     * A root with a reply draws its thread under it, a deleted or hidden root included, since
     * its replies are still there.
     */
    const hasThread = computed((): boolean => {
        const message = props.message;
        return !!message && !props.isInThread && !message.parent_id && (message.reply_count ?? 0) > 0
            && state.value !== "system" && state.value !== "card";
    });

    /**
     * A message that can carry chips: a live one, never a system line, a card or a tombstone. Its
     * reactions part is mounted before the first chip, so the reader's first reaction, added in
     * place, is seen arriving and pops rather than being drawn as a page draws one.
     */
    const isReactable = computed((): boolean => !!props.message && !isTomb.value && state.value !== "system" && state.value !== "card" && state.value !== "blocked");

    /** A reply also sent to the conversation, which says so above it and opens its thread. */
    const isThreadReply = computed((): boolean => !props.isInThread && !!props.message?.parent_id && !!props.message.shown_in_channel);

    /** A message that quotes another draws the quote above it, unless it is a tombstone itself. */
    const isQuoteShown = computed((): boolean => !!props.message?.quoted_message_id && !isTomb.value
        && state.value !== "system" && state.value !== "card" && state.value !== "blocked");

    const tombIcon = computed((): string => state.value === "deleted" ? "trash" : "eye-off");

    // The platform learns which message types its released clients draw by their text, by type
    // alone; the report keeps each to once a page load, however many rows draw it. Absent where
    // nothing provides it, as a row drawn outside the shell.
    const problems = inject(problemReportKey, null);
    watch(state, drawn => {
        if (drawn === "unknown" && props.message) {
            problems?.report("message_type", props.message.message_type);
        }
    }, { immediate: true });

    return {
        message,
        isPending,
        isOwn,
        personAliasGuid,
        state,
        text,
        pieces,
        files,
        isTomb,
        tombIcon,
        isEdited,
        isAnnouncement,
        isJumbo,
        hasThread,
        isHead,
        displayName,
        authorGuid,
        face,
        at,
        time,
        readerMention,
        isReactable,
        isThreadReply,
        isQuoteShown,
        isSendingShown,
        failedReason,
        canRetryFailed
    };
}
