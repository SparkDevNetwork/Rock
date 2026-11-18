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
// Everything drawn over the frame, as one owner: a room's details and its choices, the person's
// Chat settings, a person's card and what it opens, a member row's Message and face, and the
// message actions' surfaces, which are the quote waiting in each composer, the editor open in a
// conversation or its thread, the delete confirm and the block confirmation. Each closes with what
// it belongs to, and focus goes somewhere the person can see as it does, through the frame's one
// focus helper.
//
// The shell's message actions, rooms, profile and blocks hold the state; this reads it for the
// place each part sits in, the conversation or the open thread, and moves focus as each one ends,
// so it needs the block's frame and elements, which the shell's half never sees.

import { computed, getCurrentScope, nextTick, onBeforeUnmount, ref, Ref, shallowRef, watch, WatchStopHandle } from "vue";
import { PickerOpening } from "../components/emojiPicker.partial.obs";
import { isOwnMessage } from "../feedLayout.partial";
import type { ChatShell } from "../shell.partial";
import { ChatError, ChatPerson, ColumnKind, PersonProfile, TimelineMessage } from "../types.partial";
import { personName } from "../ui/face.partial";
import { MessageActionRequest, MessageActionsState, quoteKey } from "./useMessageActions.partial";
import { shownMembers } from "./useMembers.partial";
import { RoomChoice } from "./usePrefs.partial";
import { Frame } from "./useFrameView.partial";

/** What the overlays read from the block: the shell and the open room, and the frame's panes, views and focus helper. */
export type OverlaysViewDependencies = {
    /** The block's shell, read on every use since a birthdate replaces it. */
    shell: Readonly<Ref<ChatShell>>;

    activeChannelId: Readonly<Ref<string | null>>;

    personAliasGuid: Readonly<Ref<string>>;

    /** The block's root, which holds every message and which focus goes back into as a dialog closes. */
    rootElement: Readonly<Ref<HTMLElement | undefined>>;

    /** The frame, whose panes and views the overlays open and close, and whose helper places focus. */
    frame: Pick<Frame, "panelKind" | "mainView" | "panelElement" | "mainElement" | "isPhone" | "drawnThread" | "rooms"
        | "closePanel" | "openPersonPanel" | "openFromMainView" | "openMainView" | "onListPanel" | "focusStop">;

    /** Opens the message box of the column that asked past its bar, for a message, beside what asked. */
    openReactionPicker: (column: ColumnKind, channelId: string, message: TimelineMessage, anchor: HTMLElement, opener?: HTMLElement, opening?: PickerOpening) => void;

    closeReactionPicker: () => void;
};

/** What the overlays draw and the handlers their parts call. */
export type OverlaysView = ReturnType<typeof useOverlaysView>;

/**
 * Creates the dialogs', the card's and the message actions' state and the handlers their parts call.
 *
 * @param dependencies What the overlays read from the block.
 *
 * @returns The overlays' values and handlers.
 */
export function useOverlaysView(dependencies: OverlaysViewDependencies) {
    const { shell, activeChannelId, personAliasGuid, rootElement, frame, closeReactionPicker } = dependencies;
    const { panelKind, mainView, panelElement, mainElement, isPhone, drawnThread, closePanel, openPersonPanel, openFromMainView, openMainView, onListPanel, focusStop } = frame;
    const { roomTitles, roomKinds } = frame.rooms;

    // #region Values

    /** Whether the open conversation's details dialog is open. */
    const isDetailsOpen = ref(false);

    const isSettingsOpen = ref(false);

    /** What asked for the profile card, which it is placed beside, and a count that draws each card afresh. */
    const cardAnchor = shallowRef<HTMLElement | null>(null);
    const cardOpenCount = ref(0);

    /** Moved on by every change a member row's Message may not outlive; see the sync watcher below. */
    let memberMessageGeneration = 0;

    /**
     * What the block confirmation open now was asked from: the card's opener, the message it sits
     * in or was asked on, and whether that is in the thread drawer or the conversation.
     */
    let blockReturn: { anchor: HTMLElement | null, messageId: number | null, isInThread: boolean } = { anchor: null, messageId: null, isInThread: false };

    /** The wait for a just-blocked message to be drawn again, which finds focus again there. */
    let stopBlockRefocus: WatchStopHandle | null = null;

    // The block's own scope, so a wait begun once a block lands still ends when the block goes.
    const viewScope = getCurrentScope();

    // #endregion Values

    // #region Computed Values

    /** Where the conversation composer's quote waits. */
    const conversationQuoteKey = computed((): string | null => activeChannelId.value ? quoteKey(activeChannelId.value) : null);

    /** Where the open thread's quote waits, apart from its conversation's. */
    const threadQuoteKey = computed((): string | null => drawnThread.value ? quoteKey(drawnThread.value.channelId, drawnThread.value.rootId) : null);

    const conversationQuote = computed((): TimelineMessage | null => quoteAt(conversationQuoteKey.value));

    const threadQuote = computed((): TimelineMessage | null => quoteAt(threadQuoteKey.value));

    /** The message the delete confirm is open for, or null. */
    const confirmingDelete = computed((): MessageActionsState["confirmingDelete"] => shell.value.messageActions.state.confirmingDelete);

    /** Where the deleted message is removed from, as the confirm says it: a channel by its name, written with no "#", or this conversation. */
    const deleteBody = computed((): string => {
        const channelId = confirmingDelete.value?.channelId;
        const where = channelId && roomKinds.value[channelId] === "channel" ? roomTitles.value[channelId] : "this conversation";
        return `It will be removed for everyone in ${where}. You can't undo this.`;
    });

    /** The block confirmation's title, which names the person as the card or the message did. */
    const blockTitle = computed((): string => {
        const person = shell.value.blocks.state.confirming;
        const name = person ? personName(person) : "";
        return name ? `Block ${name}?` : "Block this person?";
    });

    // #endregion Computed Values

    // #region Event Handlers

    /**
     * The card's Message: the conversation the two already have, else New message with the person
     * chosen. The card goes first, so it never stands over what opens.
     */
    function onCardMessage(person: PersonProfile): void {
        shell.value.profile.closeCard();
        closePanel();
        if (person.dm_channel_id) {
            void openFromMainView(person.dm_channel_id);
            return;
        }

        void openMainView("newdm", [{ person_alias_guid: person.person_alias_guid, nick_name: person.nick_name, last_name: person.last_name, avatar_url: person.avatar_url }]);
    }

    function onLeave(): void {
        if (activeChannelId.value) {
            void shell.value.rooms.leave(activeChannelId.value);
        }
    }

    function onRename(name: string): void {
        if (activeChannelId.value) {
            void shell.value.rooms.rename(activeChannelId.value, name);
        }
    }

    /** Focus goes to the title button, since the phone's overflow item that may have opened the details has left with its menu. */
    function closeDetails(): void {
        isDetailsOpen.value = false;
        void nextTick(() => focusStop(["header"], { within: "main" }));
    }

    /** The open room's notify mode, star or hide, from its details or its header. */
    function onRoomChoice(choice: RoomChoice): void {
        void shell.value.prefs.choose(activeChannelId.value, choice);
    }

    /** The card's Details: the profile moves into the panel column in full, and the card gives way to it. */
    function onCardDetails(): void {
        openPersonPanel();
    }

    /** The profile panel's X: focus goes back to the face or name that opened the profile, while it is drawn. */
    function closePersonPanel(): void {
        closePanel(cardAnchor.value);
    }

    /** The profile panel's Block asks as the card's does, the panel staying open under the confirmation. */
    function onPanelBlock(person: PersonProfile): void {
        askBlock(person, null, null);
    }

    /** Needs no confirmation, since everything comes back as it was; the panel stays on the person. */
    function onPanelUnblock(person: PersonProfile): void {
        void shell.value.blocks.unblock(person, false);
    }

    /** The person's own profile panel's Chat settings: the panel goes, so it never stands beside the dialog. */
    function onPanelSettings(): void {
        closePanel();
        isSettingsOpen.value = true;
    }

    /** A conversation the two share, from the profile panel: it opens in the conversation's place, the panel closing. */
    function onPanelOpenRoom(channelId: string): void {
        closePanel();
        void openFromMainView(channelId);
    }

    /** The card goes first, so the confirmation never stands under it, and focus comes back to what opened the card. */
    function onCardBlock(person: PersonProfile): void {
        const anchor = cardAnchor.value;
        shell.value.profile.closeCard();
        askBlock(person, anchor, null);
    }

    /** Needs no confirmation, since everything comes back as it was. */
    function onCardUnblock(person: PersonProfile): void {
        shell.value.profile.closeCard();
        void shell.value.blocks.unblock(person, false);
    }

    function onSettingsUnblock(person: ChatPerson): void {
        void shell.value.blocks.unblock(person, true);
    }

    /** The card goes first, so it never stands over the dialog. */
    function onCardSettings(): void {
        shell.value.profile.closeCard();
        isSettingsOpen.value = true;
    }

    /**
     * The dialog gives focus back to what opened it; a card that opened it has gone, so focus then
     * goes to the sidebar's foot, or on a phone the title.
     */
    function closeSettings(): void {
        isSettingsOpen.value = false;
        void nextTick(() => {
            const active = document.activeElement;
            if (active && active !== document.body && rootElement.value?.contains(active)) {
                return;
            }

            if (isPhone.value) {
                focusStop(["header"], { within: "main" });
            }
            else {
                focusStop(["sidebar-me"]);
            }
        });
    }

    /** A member row's Message: the conversation with that person, or a draft for them, as New message would open it. */
    async function onMemberMessage(personAliasGuid: string): Promise<void> {
        const askedShell = shell.value;
        const channelId = askedShell.members.state.channelId;
        const member = shownMembers(askedShell.members.state).find(m => m.person_alias_guid === personAliasGuid);
        if (!member || !channelId) {
            return;
        }

        const generation = ++memberMessageGeneration;
        const person = await askedShell.profile.readPerson(personAliasGuid, channelId);
        // A slow answer must not replace another conversation or a newer member's Message.
        if (generation !== memberMessageGeneration || shell.value !== askedShell || activeChannelId.value !== channelId || panelKind.value !== "members") {
            return;
        }

        if (person?.can_message) {
            onCardMessage(person);
        }
    }

    /** A member row's face or name: the card is anchored on the row, beside the panel, and Escape gives focus back to it. */
    function onMemberPerson(personAliasGuid: string): void {
        const channelId = shell.value.members.state.channelId;
        if (!channelId) {
            return;
        }

        // Safari leaves a pressed button unfocused, and the card hands focus back to what held it.
        const row = focusStop([{ stop: "member", match: `[data-person='${personAliasGuid}']` }], { within: "panel", preventScroll: true });
        if (row) {
            openCard(row, personAliasGuid, channelId);
        }
    }

    /** The details dialog's member count gives way to the members panel, which takes focus. */
    function onDetailsMembers(): void {
        isDetailsOpen.value = false;
        if (panelKind.value !== "members") {
            void onListPanel("members");
        }
    }

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

            // the confirmation names the sender as the message does
            case "block":
                askBlock({
                    person_alias_guid: request.message.person_alias_guid ?? "",
                    nick_name: request.message.sender_nick_name ?? null,
                    last_name: request.message.sender_last_name ?? null,
                    avatar_url: request.message.sender_avatar_url ?? null
                }, null, request.message.id, rootId !== null);
                break;

            case "server":
                void actions.runAction(channelId, request.message.id, request.action).finally(() => request.onSettled?.());
                break;

            case "react":
                void shell.value.reactions.toggleCode(channelId, request.message.id, request.code);
                break;

            case "openPicker":
                dependencies.openReactionPicker(rootId === null ? "conversation" : "thread", channelId, request.message, request.anchor, request.opener, request.opening);
                break;

            // the person is read for the room the author or chip was pressed in
            case "person":
                openCard(request.anchor, request.personAliasGuid, channelId);
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

    function onCancelBlock(): void {
        shell.value.blocks.cancel();
    }

    /**
     * Blocks the person. Their message is drawn again as a quiet line once the screen has been read
     * again, which replaces the row focus went back to, so focus is found again there if it fell.
     * The wait ends with the block, and once the reader has moved to another conversation or
     * thread, where taking focus later would pull them away from what they went to.
     */
    function onConfirmBlock(): void {
        const messageId = blockReturn.messageId;
        void shell.value.blocks.confirm().then(isTaken => {
            if (!isTaken || messageId === null || viewScope?.active === false) {
                return;
            }

            const channelId = activeChannelId.value;
            const rootId = drawnThread.value?.rootId ?? null;
            const isMoved = (): boolean => activeChannelId.value !== channelId || (drawnThread.value?.rootId ?? null) !== rootId;
            const isBlockedCopy = (): boolean => [
                ...(channelId ? shell.value.timelines.state(channelId).messages : []),
                ...(drawnThread.value?.replies ?? []),
                ...(drawnThread.value?.root ? [drawnThread.value.root] : [])
            ].some(message => message.id === messageId && message.blocked === true);

            // ended once: the first look, made at once, may already find the outcome
            let isEnded = false;
            const wait = (): WatchStopHandle => watch(() => isMoved() ? "moved" : isBlockedCopy() ? "drawn" : "waiting", outcome => {
                if (outcome === "waiting" || isEnded) {
                    return;
                }
                isEnded = true;
                stopBlockRefocus?.();
                stopBlockRefocus = null;
                if (outcome === "moved") {
                    return;
                }
                void nextTick(() => {
                    const active = document.activeElement;
                    if (!active || active === document.body || !active.isConnected) {
                        blockReturnTarget()?.focus({ preventScroll: true });
                    }
                });
            }, { immediate: true });
            const stop = viewScope ? viewScope.run(wait) ?? null : wait();
            if (isEnded) {
                stop?.();
            }
            else {
                stopBlockRefocus = stop;
            }
        });
    }

    // #endregion Event Handlers

    // #region Functions

    /** Opens a person's card for a room beside what asked, drawn afresh each time. */
    function openCard(anchor: HTMLElement, personAliasGuid: string, channelId: string): void {
        cardAnchor.value = anchor;
        cardOpenCount.value++;
        shell.value.profile.openCard(personAliasGuid, channelId);
        // A phone has no room for a glance beside the face, so the profile opens full screen at once.
        if (isPhone.value) {
            openPersonPanel();
        }
    }

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

    /**
     * Focuses the composer of the conversation, or of the thread drawer for a root, once it is
     * drawn. The person asked to type there, so a phone's keyboard rising is what they want.
     */
    function focusComposerIn(rootId: number | null): void {
        void nextTick(() => focusStop(["composer"], { within: rootId === null ? "main" : "panel", isTyping: true }));
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

    /**
     * The message's row by id where it was asked, the thread drawer or the conversation, or the
     * quiet line of the run of blocked messages it has folded into there, which names every id it
     * holds. A later blocked line, or the same person's in the other place, is never taken for it.
     */
    function messageElement(messageId: number, isInThread: boolean): HTMLElement | null {
        const place = isInThread ? panelElement.value : mainElement.value;
        return place?.querySelector<HTMLElement>(`[data-message-id='${messageId}'], [data-reply-id='${messageId}'], [data-state='blocked'][data-blocked-ids~='${messageId}']`) ?? null;
    }

    /**
     * Opens the block confirmation for a person, from their card or one of their messages, with
     * what focus goes back to: what opened the card, else the message.
     */
    function askBlock(person: ChatPerson, anchor: HTMLElement | null, messageId: number | null, isInThread = !!anchor && !!panelElement.value?.contains(anchor)): void {
        stopBlockRefocus?.();
        stopBlockRefocus = null;
        const row = anchor?.closest<HTMLElement>("[data-message-id], [data-reply-id]");
        const anchorMessage = row ? Number(row.dataset.messageId ?? row.dataset.replyId) : null;
        blockReturn = { anchor, messageId: messageId ?? anchorMessage, isInThread };
        shell.value.blocks.ask(person);
    }

    /** What the block confirmation gives focus back to as it closes. */
    function blockReturnTarget(): HTMLElement | null {
        const { anchor, messageId, isInThread } = blockReturn;
        if (anchor?.isConnected) {
            return anchor;
        }
        return messageId === null ? null : messageElement(messageId, isInThread);
    }

    // #endregion Functions

    // #region Watchers

    // The blocked people are read as Chat settings opens, so the list is never older than the dialog.
    watch(isSettingsOpen, isOpen => {
        if (isOpen) {
            void shell.value.blocks.load();
        }
    });

    // A Message belongs to this opening of Members, even if the same room's panel is closed and
    // reopened before Vue's next render, so it is invalidated at each change, not after batching.
    watch([shell, activeChannelId, panelKind], () => {
        memberMessageGeneration++;
    }, { flush: "sync" });

    // A picker or a card belongs to a message on screen, so another conversation or a full view
    // over it closes them; so are the details, which belong to the conversation.
    watch([activeChannelId, mainView], () => {
        closeReactionPicker();
        if (shell.value.profile.state.card) {
            shell.value.profile.closeCard();
        }
        isDetailsOpen.value = false;
    });

    // Rock is asked each time the details open whether the person may rename the room, as its
    // rename door would answer now.
    watch(isDetailsOpen, isOpen => {
        if (isOpen && activeChannelId.value) {
            void shell.value.rooms.detailsOpened(activeChannelId.value);
        }
    });

    // Once the person has left, the room's details have nothing left to show.
    watch(() => activeChannelId.value ? shell.value.rooms.leaveOf(activeChannelId.value).kind : null, kind => {
        if (kind === "left" && isDetailsOpen.value) {
            closeDetails();
        }
    });

    // #endregion Watchers

    onBeforeUnmount(() => {
        memberMessageGeneration++;
    });

    return {
        isDetailsOpen, isSettingsOpen, cardAnchor, cardOpenCount, onCardMessage, onLeave, onRename, closeDetails, onRoomChoice,
        onCardDetails, closePersonPanel, onPanelBlock, onPanelUnblock, onPanelSettings, onPanelOpenRoom, onCardBlock, onCardUnblock, onSettingsUnblock, onCardSettings, closeSettings, onMemberMessage, onMemberPerson, onDetailsMembers,
        conversationQuoteKey, threadQuoteKey, conversationQuote, threadQuote, confirmingDelete, deleteBody, isOwnQuote,
        editingIdIn, editErrorIn, isEditSavingIn, onEditLast, onEditLastReply, onCancelQuote, onMessageAction, onCancelDelete, onConfirmDelete,
        askBlock, blockTitle, blockReturnTarget, onCancelBlock, onConfirmBlock
    };
}
