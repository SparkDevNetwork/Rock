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
// Acting on a message: editing the person's own message, deleting it behind a confirm, copying
// its text, quoting it into the next message, and running an action the server listed on it.
//
// Each call answers in its own time. An edit's answer that a newer edit or a conversation switch
// overtook is dropped, so a late answer never undoes what the person sees now or closes an editor
// they opened since. Once the actions are stopped nothing new is called and no answer still on
// its way reaches the screen. The database refuses what it must whatever the client offers; the
// client only keeps from offering what would be refused.
/* eslint-disable @typescript-eslint/naming-convention */
import { reactive } from "vue";
import { messageForError } from "../errors.partial";
import { isOwnMessage } from "../feedLayout.partial";
import { CallResult } from "../platformCall.partial";
import { ChatError, MessageAction, TimelineMessage } from "../types.partial";
import { isTimelineMessage, Timelines } from "./useHistory.partial";

export type { MessageAction } from "../types.partial";

/** A toast the actions ask the shell to show. */
export type ActionNotice = {
    code: string;
    text: string;
    icon: string;
};

/** What the actions reach outside themselves. */
export type MessageActionsDependencies = {
    /** The shell's one call path. */
    call: (name: string, args: Record<string, unknown>) => Promise<CallResult>;

    /** Where an answer is laid over the message it changed. */
    timelines: Pick<Timelines, "state" | "applyEvent">;

    /** The session's alias, for the own check of a live copy, which carries no is_own. */
    personAliasGuid: string;

    /** Shows a toast. */
    notify: (notice: ActionNotice) => void;

    /** Writes to the clipboard; absent where there is none. */
    writeText?: (text: string) => Promise<void>;
};

/** What the actions hold. */
export type MessageActionsState = {
    /** The message being edited in place, whether its save is out, and the refusal it met. */
    editing: { channelId: string, messageId: number, isSaving: boolean, error: ChatError | null } | null;

    /** The message the delete confirm is open for. */
    confirmingDelete: { channelId: string, message: TimelineMessage } | null;

    /** The message being quoted into the next send, by conversation or thread key. */
    quotes: Record<string, TimelineMessage>;
};

/** The actions a shell holds. */
export type MessageActions = {
    state: MessageActionsState;

    /** Opens the editor on a message. */
    startEdit: (channelId: string, messageId: number) => void;

    /** Opens the editor on the newest message the person could edit; false when there is none. */
    startEditNewest: (channelId: string) => boolean;

    /** Closes the editor without saving. */
    cancelEdit: () => void;

    /** Saves the open editor's text; true once the platform took it. */
    saveEdit: (body: string) => Promise<boolean>;

    /** Opens the delete confirm; nothing is called until it is confirmed. */
    askDelete: (channelId: string, message: TimelineMessage) => void;

    /** Closes the delete confirm. */
    cancelDelete: () => void;

    /** Deletes the message the confirm is open for; true once the platform took it. */
    confirmDelete: () => Promise<boolean>;

    /** Runs an action the server listed on a message and toasts its sentence. */
    runAction: (channelId: string, messageId: number, action: MessageAction) => Promise<void>;

    /** Copies a message's text to the clipboard. */
    copy: (message: TimelineMessage) => Promise<void>;

    /** Quotes a message into the next send of a conversation or thread. */
    startQuote: (key: string, message: TimelineMessage) => void;

    /** Drops the quote of a conversation or thread. */
    cancelQuote: (key: string) => void;

    /** The open conversation changed: the editor and the confirm belong to the one left. */
    switchConversation: (channelId: string | null) => void;

    /** Ends the actions for good. */
    stop: () => void;
};

/**
 * The only functions a server-listed action may call. A listed action names its own function, so
 * without this a message could make the client call any function the platform exposes to it.
 */
const actionCallPattern = /^chat_action_[a-z0-9_]+$/;

/**
 * Whether the person could edit a message in place: their own, on the timeline, live, shown and
 * not laid out in blocks or a system line, which the database refuses to edit.
 *
 * @param message The message.
 * @param personAliasGuid The session's alias.
 *
 * @returns True when Edit is offered.
 */
export function canEditMessage(message: TimelineMessage, personAliasGuid: string): boolean {
    return isOwnMessage(message, personAliasGuid)
        && isTimelineMessage(message)
        && !message.deleted_at
        && (message.visibility ?? "visible") === "visible"
        && message.message_type !== "system"
        && !(message.blocks && message.blocks.length > 0);
}

/**
 * The key a quote waits under until the next send there: the conversation's id, or for a
 * thread its conversation and root, so a thread's quote never leaves with a conversation send.
 *
 * @param channelId The conversation.
 * @param rootId The thread's root, for a quote in its reply composer.
 *
 * @returns The key.
 */
export function quoteKey(channelId: string, rootId?: number | null): string {
    return typeof rootId === "number" ? `${channelId}/${rootId}` : channelId;
}

/**
 * The text a copy writes: what the person reads, so a mention copies as its name rather than its
 * token.
 *
 * @param message The message.
 *
 * @returns The text.
 */
function copyText(message: TimelineMessage): string {
    return message.pieces ? message.pieces.map(piece => piece.text).join("") : message.body ?? "";
}

/**
 * Creates the actions.
 *
 * @param dependencies What the actions reach outside themselves.
 *
 * @returns The actions.
 */
export function createMessageActions(dependencies: MessageActionsDependencies): MessageActions {
    const state = reactive<MessageActionsState>({ editing: null, confirmingDelete: null, quotes: {} }) as MessageActionsState;

    let isStopped = false;

    // Each editor opened, save sent and conversation switch takes the next number; an edit's
    // answer counts only while its number is still the newest.
    let editTurn = 0;

    /** Toasts a refusal: the server's sentence, or the client's own for its severity. */
    function notifyRefusal(error: ChatError): void {
        dependencies.notify({ code: error.code, text: messageForError(error), icon: "alert" });
    }

    /** Opens the editor on a message, overtaking any save still out. */
    function startEdit(channelId: string, messageId: number): void {
        if (isStopped) {
            return;
        }

        editTurn++;
        state.editing = { channelId, messageId, isSaving: false, error: null };
    }

    return {
        state,

        startEdit,

        startEditNewest: (channelId: string): boolean => {
            if (isStopped) {
                return false;
            }

            const messages = dependencies.timelines.state(channelId).messages;
            for (let i = messages.length - 1; i >= 0; i--) {
                if (canEditMessage(messages[i], dependencies.personAliasGuid)) {
                    startEdit(channelId, messages[i].id);
                    return true;
                }
            }

            return false;
        },

        cancelEdit: (): void => {
            editTurn++;
            state.editing = null;
        },

        saveEdit: async (body: string): Promise<boolean> => {
            const editing = state.editing;
            if (isStopped || !editing) {
                return false;
            }

            const turn = ++editTurn;
            editing.isSaving = true;
            editing.error = null;

            const result = await dependencies.call("chat_edit_message", { p_message_id: editing.messageId, p_body: body });
            if (isStopped || turn !== editTurn) {
                return false;
            }

            if (!result.ok) {
                // The editor stays with the text the person typed, so they can fix it or cancel.
                editing.isSaving = false;
                editing.error = result.error;
                return false;
            }

            // Laid over the message as its live edit would be; the live copy follows with the
            // pieces the server built, which the answer does not carry.
            const answer = (result.data ?? {}) as { edited_at?: string | null };
            dependencies.timelines.applyEvent(editing.channelId, "message.edited", {
                id: editing.messageId,
                channel_id: editing.channelId,
                body,
                pieces: null,
                edited_at: answer.edited_at ?? new Date().toISOString()
            });
            state.editing = null;
            return true;
        },

        askDelete: (channelId: string, message: TimelineMessage): void => {
            if (isStopped) {
                return;
            }

            state.confirmingDelete = { channelId, message };
        },

        cancelDelete: (): void => {
            state.confirmingDelete = null;
        },

        confirmDelete: async (): Promise<boolean> => {
            const confirming = state.confirmingDelete;
            if (isStopped || !confirming) {
                return false;
            }

            // The dialog closes on the press; the tombstone comes with the answer. A delete is
            // not overtaken by a switch: it took place, and the timeline it lands in is held.
            state.confirmingDelete = null;
            const result = await dependencies.call("chat_delete_message", { p_message_id: confirming.message.id });
            if (isStopped) {
                return false;
            }

            if (!result.ok) {
                notifyRefusal(result.error);
                return false;
            }

            const answer = (result.data ?? {}) as { deleted_at?: string };
            dependencies.timelines.applyEvent(confirming.channelId, "message.deleted", {
                id: confirming.message.id,
                channel_id: confirming.channelId,
                deleted_at: answer.deleted_at ?? new Date().toISOString()
            });
            dependencies.notify({ code: "message-deleted", text: "Message deleted", icon: "trash" });
            return true;
        },

        runAction: async (_channelId: string, messageId: number, action: MessageAction): Promise<void> => {
            if (isStopped || !actionCallPattern.test(action.call)) {
                return;
            }

            const result = await dependencies.call(action.call, { p_message_id: messageId, p_action_id: action.id });
            if (isStopped) {
                return;
            }

            if (!result.ok) {
                notifyRefusal(result.error);
                return;
            }

            // The function's answer is the sentence the person is shown; one without says nothing.
            const notice = (result.data as { notice?: unknown } | null)?.notice;
            if (typeof notice === "string" && notice !== "") {
                dependencies.notify({ code: `action ${action.id}`, text: notice, icon: "bolt" });
            }
        },

        copy: async (message: TimelineMessage): Promise<void> => {
            if (isStopped) {
                return;
            }

            const unavailable: ActionNotice = { code: "copy-unavailable", text: "Copy isn't available here", icon: "alert" };
            if (!dependencies.writeText) {
                dependencies.notify(unavailable);
                return;
            }

            try {
                await dependencies.writeText(copyText(message));
            }
            catch {
                // The browser refused the clipboard, as it does outside a secure page or without focus.
                if (!isStopped) {
                    dependencies.notify(unavailable);
                }
                return;
            }

            if (!isStopped) {
                dependencies.notify({ code: "copied", text: "Copied to clipboard", icon: "copy" });
            }
        },

        startQuote: (key: string, message: TimelineMessage): void => {
            if (!isStopped) {
                state.quotes[key] = message;
            }
        },

        cancelQuote: (key: string): void => {
            delete state.quotes[key];
        },

        // The quotes stay, each kept to its own conversation, so going back finds it still there.
        switchConversation: (_channelId: string | null): void => {
            editTurn++;
            state.editing = null;
            state.confirmingDelete = null;
        },

        stop: (): void => {
            // for good: what is on screen stays as it was, and nothing still out lands in it
            isStopped = true;
        }
    };
}
