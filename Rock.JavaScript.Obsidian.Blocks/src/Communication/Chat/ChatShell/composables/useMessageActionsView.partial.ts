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
// What the block draws of the message actions, and where it sends a message's bar and menu: the
// quote waiting in each composer, the editor open in a conversation or its thread, and the delete
// confirm. The shell's message actions hold the state; this reads it for the place each part sits
// in, the conversation or the open thread, and moves focus as each action ends, so it needs the
// block's frame and elements, which the shell's half never sees.

import { computed, ComputedRef, nextTick, Ref } from "vue";
import { PickerOpening } from "../components/emojiPicker.partial.obs";
import { isOwnMessage } from "../feedLayout.partial";
import { ChatShell } from "../shell.partial";
import { ChatError, TimelineMessage } from "../types.partial";
import { MessageActionRequest, MessageActionsState, quoteKey } from "./useMessageActions.partial";
import { OpenThreadState } from "./useThreads.partial";

/** What the message actions read from the block. */
export type MessageActionsViewDependencies = {
    /** The block's shell, read on every call since a birthdate replaces it. */
    shell: Readonly<Ref<ChatShell>>;

    activeChannelId: Readonly<Ref<string | null>>;

    /** The thread the drawer draws, whose composer and replies are the thread's place. */
    drawnThread: Readonly<Ref<OpenThreadState | null>>;

    personAliasGuid: Readonly<Ref<string>>;

    /** Each room's title by channel id, as the header writes it. */
    roomTitles: Readonly<Ref<Record<string, string>>>;

    /** Each room's kind by channel id: a channel, a person, or a group. */
    roomKinds: Readonly<Ref<Record<string, string>>>;

    /** The block's root, which holds the conversation's composer and every message. */
    rootElement: Readonly<Ref<HTMLElement | undefined>>;

    /** The panel column, which holds the thread's composer. */
    panelElement: Readonly<Ref<HTMLElement | undefined>>;

    /** Opens the reaction picker for a message, beside what asked. */
    openReactionPicker: (channelId: string, message: TimelineMessage, anchor: HTMLElement, opener?: HTMLElement, opening?: PickerOpening) => void;

    /** Opens a person's card for a room, beside what asked. */
    openCard: (anchor: HTMLElement, personAliasGuid: string, channelId: string) => void;
};

/** What the conversation, the thread drawer and the delete confirm draw and send. */
export type MessageActionsView = {
    /** Where the conversation composer's quote waits. */
    conversationQuoteKey: ComputedRef<string | null>;

    /** Where the open thread's quote waits, apart from its conversation's. */
    threadQuoteKey: ComputedRef<string | null>;

    conversationQuote: ComputedRef<TimelineMessage | null>;
    threadQuote: ComputedRef<TimelineMessage | null>;

    /** The message the delete confirm is open for, or null. */
    confirmingDelete: ComputedRef<MessageActionsState["confirmingDelete"]>;

    /** Where the deleted message is removed from, as the confirm says it. */
    deleteBody: ComputedRef<string>;

    isOwnQuote: (quote: TimelineMessage | null) => boolean;
    editingIdIn: (channelId: string | null) => number | null;
    editErrorIn: (channelId: string | null) => ChatError | null;
    isEditSavingIn: (channelId: string | null) => boolean;
    onEditLast: () => void;
    onEditLastReply: () => void;
    onCancelQuote: (key: string | null) => void;
    onMessageAction: (request: MessageActionRequest, rootId: number | null) => void;
    onCancelDelete: () => void;
    onConfirmDelete: () => void;
};

/**
 * Creates what the block draws of the message actions and the handlers its parts call.
 *
 * @param dependencies What the actions read from the block.
 *
 * @returns The view's values and handlers.
 */
export function useMessageActionsView(dependencies: MessageActionsViewDependencies): MessageActionsView {
    const { shell, activeChannelId, drawnThread, personAliasGuid, roomTitles, roomKinds, rootElement, panelElement } = dependencies;

    // #region Computed Values

    const conversationQuoteKey = computed((): string | null => activeChannelId.value ? quoteKey(activeChannelId.value) : null);

    const threadQuoteKey = computed((): string | null => drawnThread.value ? quoteKey(drawnThread.value.channelId, drawnThread.value.rootId) : null);

    const conversationQuote = computed((): TimelineMessage | null => quoteAt(conversationQuoteKey.value));

    const threadQuote = computed((): TimelineMessage | null => quoteAt(threadQuoteKey.value));

    const confirmingDelete = computed((): MessageActionsState["confirmingDelete"] => shell.value.messageActions.state.confirmingDelete);

    /** A channel by its name, written with no "#", or this conversation. */
    const deleteBody = computed((): string => {
        const channelId = confirmingDelete.value?.channelId;
        const where = channelId && roomKinds.value[channelId] === "channel" ? roomTitles.value[channelId] : "this conversation";
        return `It will be removed for everyone in ${where}. You can't undo this.`;
    });

    // #endregion Computed Values

    // #region Event Handlers

    /** Up in the empty conversation composer: the newest message the person could edit opens in place. */
    function onEditLast(): void {
        if (activeChannelId.value) {
            shell.value.messageActions.startEditNewest(activeChannelId.value);
        }
    }

    /** Up in the empty thread composer: the newest of the person's replies in that thread opens in place. */
    function onEditLastReply(): void {
        const thread = drawnThread.value;
        if (thread) {
            shell.value.messageActions.startEditNewest(thread.channelId, thread.rootId);
        }
    }

    function onCancelQuote(key: string | null): void {
        if (key) {
            shell.value.messageActions.cancelQuote(key);
        }
    }

    /**
     * A message's bar or menu asked for an action, in the conversation or, with its root, the open
     * thread. A quote waits in the composer of the place it was asked in, which takes focus.
     */
    function onMessageAction(request: MessageActionRequest, rootId: number | null): void {
        const channelId = rootId === null ? activeChannelId.value : drawnThread.value?.channelId ?? null;
        if (!channelId) {
            if (request.kind === "server") {
                request.onSettled?.();
            }
            return;
        }

        const actions = shell.value.messageActions;
        switch (request.kind) {
            case "quote":
                actions.startQuote(quoteKey(channelId, rootId), request.message);
                focusComposerIn(rootId);
                break;

            case "edit":
                actions.startEdit(channelId, request.message.id);
                break;

            // A closed editor hands focus back to the composer it was opened beside, so typing
            // carries on there; a refused save keeps the editor and its focus.
            case "saveEdit":
                void actions.saveEdit(request.body).then(isSaved => {
                    if (isSaved) {
                        focusComposerIn(rootId);
                    }
                });
                break;

            case "cancelEdit":
                actions.cancelEdit();
                focusComposerIn(rootId);
                break;

            case "copy":
                void actions.copy(request.message);
                break;

            case "delete":
                actions.askDelete(channelId, request.message);
                break;

            case "server":
                void actions.runAction(channelId, request.message.id, request.action).finally(() => request.onSettled?.());
                break;

            case "react":
                void shell.value.reactions.toggleCode(channelId, request.message.id, request.code);
                break;

            case "openPicker":
                dependencies.openReactionPicker(channelId, request.message, request.anchor, request.opener, request.opening);
                break;

            // the person is read for the room the author or chip was pressed in
            case "person":
                dependencies.openCard(request.anchor, request.personAliasGuid, channelId);
                break;
        }
    }

    function onCancelDelete(): void {
        const messageId = confirmingDelete.value?.message.id ?? null;
        shell.value.messageActions.cancelDelete();
        refocusMessage(messageId);
    }

    function onConfirmDelete(): void {
        const messageId = confirmingDelete.value?.message.id ?? null;
        void shell.value.messageActions.confirmDelete();
        refocusMessage(messageId);
    }

    // #endregion Event Handlers

    // #region Functions

    /** The message open in the editor when it belongs to a conversation, else null. */
    function editingIdIn(channelId: string | null): number | null {
        const editing = shell.value.messageActions.state.editing;
        return editing && editing.channelId === channelId ? editing.messageId : null;
    }

    /** The refusal the open editor's last save met, when the editor belongs to a conversation. */
    function editErrorIn(channelId: string | null): ChatError | null {
        const editing = shell.value.messageActions.state.editing;
        return editing && editing.channelId === channelId ? editing.error : null;
    }

    /** Whether the open editor's save is out, when the editor belongs to a conversation. */
    function isEditSavingIn(channelId: string | null): boolean {
        const editing = shell.value.messageActions.state.editing;
        return !!editing && editing.channelId === channelId && editing.isSaving;
    }

    /** The message quoted under a key, if any. */
    function quoteAt(key: string | null): TimelineMessage | null {
        return key ? shell.value.messageActions.state.quotes[key] ?? null : null;
    }

    /** Whether a quote is the person's own, under any of their aliases when the page said so. */
    function isOwnQuote(quote: TimelineMessage | null): boolean {
        return !!quote && isOwnMessage(quote, personAliasGuid.value);
    }

    /** Focuses the composer of the conversation, or of the thread drawer for a root, once it is drawn. */
    function focusComposerIn(rootId: number | null): void {
        void nextTick(() => {
            const place = rootId === null ? rootElement.value?.querySelector("[data-testid='frame-main']") : panelElement.value;
            place?.querySelector<HTMLElement>("[data-testid='composer-input']")?.focus();
        });
    }

    /**
     * The dialog gives focus back to the More button that opened it, but that button leaves with its
     * bar, at once when focus moved into the dialog, or once a mouse's pointer has gone from the
     * message, so focus goes to the message itself instead. Focus on the dialog's own buttons or on
     * a message's bar is not kept: the dialog stays on the page while it fades out, the bar may
     * leave a moment later, and either leaving would drop focus to the page.
     */
    function refocusMessage(messageId: number | null): void {
        void nextTick(() => {
            const root = rootElement.value;
            const active = document.activeElement;
            const isFocusKept = !!active && root?.contains(active) && !active.closest("[role='dialog'], [role='toolbar']");
            if (messageId === null || !root || isFocusKept) {
                return;
            }

            root.querySelector<HTMLElement>(`[data-message-id='${messageId}'], [data-reply-id='${messageId}']`)?.focus({ preventScroll: true });
        });
    }

    // #endregion Functions

    return {
        conversationQuoteKey,
        threadQuoteKey,
        conversationQuote,
        threadQuote,
        confirmingDelete,
        deleteBody,
        isOwnQuote,
        editingIdIn,
        editErrorIn,
        isEditSavingIn,
        onEditLast,
        onEditLastReply,
        onCancelQuote,
        onMessageAction,
        onCancelDelete,
        onConfirmDelete
    };
}
