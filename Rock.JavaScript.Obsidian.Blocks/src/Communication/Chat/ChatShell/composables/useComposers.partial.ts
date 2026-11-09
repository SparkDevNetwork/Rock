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
// The conversation's and the thread's composers, as the block wires them to the shell: each
// one's draft and the place being typed in, the files waiting and a drop on the feed, the strip
// of failed sends with Try again and Delete, the @ search, the emoji catalog and recents, and the
// reply's send. Quotes and the editor are the message actions' and stay there.

import { computed, ref, Ref, watch } from "vue";
import { MentionPickerSource } from "../components/mentionPicker.partial.obs";
import type { ChatShell } from "../shell.partial";
import { DisplayPiece, PendingMessage, SidebarRow } from "../types.partial";
import { AttachmentTray, canAttach } from "./useAttachments.partial";
import { ComposerDraft } from "./useDrafts.partial";
import { PanelKind } from "./useFrame.partial";
import { failedSends, isOwnedByConversation, lockedReason, retryableSends, sendFailure, SendFailure } from "./useSend.partial";
import { OpenThreadState } from "./useThreads.partial";

/** What the composers read from the block. */
export type ComposersDependencies = {
    /** The block's shell, read on every use since a birthdate replaces it. */
    shell: Readonly<Ref<ChatShell>>;

    activeChannelId: Readonly<Ref<string | null>>;

    /** The open conversation's sidebar row, which says whether its composer is locked. */
    activeRow: Readonly<Ref<SidebarRow | null>>;

    /** Whether the join bar stands in the conversation composer's place. */
    isJoinBarShown: Readonly<Ref<boolean>>;

    /** The thread the drawer draws, whose composer sends the reply. */
    drawnThread: Readonly<Ref<OpenThreadState | null>>;

    panelKind: Readonly<Ref<PanelKind | null>>;

    /** Whether the panel column is out of view, as a phone's list in front leaves it. */
    isPanelHidden: Readonly<Ref<boolean>>;

    /** Where the conversation composer's quote and draft are kept. */
    conversationQuoteKey: Readonly<Ref<string | null>>;

    /** Where the open thread's quote and draft are kept. */
    threadQuoteKey: Readonly<Ref<string | null>>;
};

/**
 * Creates what the two composers draw and the handlers they call.
 *
 * @param dependencies What the composers read from the block.
 *
 * @returns The composers' values and handlers.
 */
export function useComposers(dependencies: ComposersDependencies) {
    const { shell, activeChannelId, activeRow, isJoinBarShown, drawnThread, panelKind, isPanelHidden, conversationQuoteKey, threadQuoteKey } = dependencies;

    /** The open conversation's files waiting to go, or null where its composer takes none. */
    const conversationTray = computed((): AttachmentTray | null => isJoinBarShown.value ? null : trayOf(activeChannelId.value));

    /** Each box's text, kept by the shell for its place, so text typed in one never shows in another. */
    const conversationDraft = computed((): ComposerDraft | null => conversationQuoteKey.value ? shell.value.drafts.of(conversationQuoteKey.value) : null);
    const threadDraft = computed((): ComposerDraft | null => threadQuoteKey.value ? shell.value.drafts.of(threadQuoteKey.value) : null);

    /** The box focus is in, if either. */
    const focusedComposer = ref<"conversation" | "thread" | null>(null);

    // The drafts are told the place being typed in, as the room or thread under a focused box
    // changes or a birthdate builds a new shell, so another tab's write never replaces the text
    // the person is typing.
    watch([focusedComposer, conversationQuoteKey, threadQuoteKey, shell], () => {
        const place = focusedComposer.value === "conversation" ? conversationQuoteKey.value
            : focusedComposer.value === "thread" ? threadQuoteKey.value
                : null;
        shell.value.drafts.focus(place ?? null);
    });

    /**
     * The sends whose failure the conversation's strip speaks for: those it draws, and a reply
     * only when also sent there and not while the drawer shows its thread, whose own strip has it.
     */
    const conversationSends = computed((): PendingMessage[] => {
        const openRootId = panelKind.value === "thread" && !isPanelHidden.value ? drawnThread.value?.rootId ?? null : null;
        return activeChannelId.value ? shell.value.sender.pending(activeChannelId.value).filter(row => isOwnedByConversation(row, openRootId)) : [];
    });

    const conversationFailure = computed((): SendFailure | null => sendFailure(conversationSends.value));

    /** Who else is typing where each box types, drawn in its foot; null for no one. */
    const conversationTyping = computed((): string | null => shell.value.presence.lineFor(conversationQuoteKey.value));
    const threadTyping = computed((): string | null => shell.value.presence.lineFor(threadQuoteKey.value));

    /** A composer's @ query, searched among the people of the conversation it is in; null closes the search. */
    function onMentionQuery(row: SidebarRow | null, query: string | null): void {
        if (row && query !== null) {
            shell.value.mentions.search(row, query);
        }
        else {
            shell.value.mentions.closeSearch();
        }
    }

    /**
     * A composer took focus: the @ picker's first people are fetched now, so the first @ draws
     * them at once. Focus, not opening, so a person who only reads asks nothing.
     */
    function onComposerFocus(row: SidebarRow | null, box: "conversation" | "thread"): void {
        focusedComposer.value = box;
        if (row) {
            shell.value.mentions.warm(row);
        }
    }

    function onComposerBlur(): void {
        focusedComposer.value = null;
    }

    /**
     * A box's text changed: it is kept as its place's draft, and told as typing in that place. A box
     * handing back the text its draft already holds, as a send that keeps its text does, is not
     * the person typing.
     */
    function onDraftText(place: string | null, text: string): void {
        if (!place) {
            return;
        }
        const isChanged = shell.value.drafts.of(place).text !== text;
        shell.value.drafts.write(place, text);
        if (!isChanged) {
            return;
        }

        // the drawer's thread can be of a room other than the open one
        const thread = drawnThread.value;
        if (place === conversationQuoteKey.value && activeChannelId.value) {
            shell.value.presence.typed(activeChannelId.value, null, text);
        }
        else if (place === threadQuoteKey.value && thread) {
            shell.value.presence.typed(thread.channelId, thread.rootId, text);
        }
    }

    /** A thread reply's also-send choice, kept with that thread's draft. */
    function onDraftAlsoSent(place: string | null, isAlsoSent: boolean): void {
        if (place) {
            shell.value.drafts.setAlsoSent(place, isAlsoSent);
        }
    }

    /** A chip drawn in a box: its name and photo are kept with that place's draft. */
    function onDraftMention(place: string | null, piece: DisplayPiece): void {
        if (place) {
            shell.value.drafts.addMention(place, piece);
        }
    }

    /** A drag carrying files may be dropped anywhere on the conversation, where its composer takes files. */
    function onConversationDragOver(event: DragEvent): void {
        if (conversationTray.value && isConversationOpenToFiles() && event.dataTransfer?.types?.includes("Files")) {
            event.preventDefault();
        }
    }

    function onConversationDrop(event: DragEvent): void {
        const files = event.dataTransfer?.files;
        if (event.defaultPrevented || !conversationTray.value || !isConversationOpenToFiles() || !files?.length) {
            return;
        }

        event.preventDefault();
        conversationTray.value.add(Array.from(files));
    }

    function onSendReply(body: string, isShownInChannel: boolean): void {
        if (drawnThread.value) {
            void shell.value.sendReply(drawnThread.value.rootId, body, isShownInChannel);
        }
    }

    /** A composer's picker opened: its categories are fetched once for the page. */
    function onEmojiPickerOpen(): void {
        void shell.value.reactions.loadCatalog();
    }

    function onEmojiChosen(code: string): void {
        shell.value.reactions.recordRecent(code);
    }

    /** A strip's failed sends, oldest first, which the sender sends one at a time in that order. */
    function onRetry(localIds: string[]): void {
        void shell.value.sender.retryAll(localIds);
    }

    function onRetryFailed(sends: PendingMessage[]): void {
        onRetry(retryableSends(sends).map(row => row.localId));
    }

    function onDiscardFailed(sends: PendingMessage[]): void {
        failedSends(sends).forEach(row => shell.value.sender.discard(row.localId));
    }

    /** A place's files waiting to go, or null where the platform answered no limits for files. */
    function trayOf(channelId: string | null, rootId?: number): AttachmentTray | null {
        return channelId && canAttach(shell.value.state.limits) ? shell.value.trayOf(channelId, rootId) : null;
    }

    function isConversationOpenToFiles(): boolean {
        return shell.value.state.isComposerOpen && !lockedReason(activeRow.value, shell.value.state.service);
    }

    /** What a composer's @ picker lists: the people found for the newest query, and @all where offered; with no room, none. */
    function mentionSourceFor(row: SidebarRow | null): MentionPickerSource | null {
        const picker = shell.value.mentions.state.picker;
        return row
            ? { canMention: row.can_mention, people: picker.people, isAllOffered: picker.isAllOffered, isAwaitingAnswer: picker.isAwaitingAnswer, isAwaitingFirstAnswer: picker.isAwaitingFirstAnswer }
            : null;
    }

    return {
        conversationTray, conversationDraft, threadDraft, conversationSends, conversationFailure, conversationTyping, threadTyping,
        onMentionQuery, onComposerFocus, onComposerBlur, onDraftText, onDraftAlsoSent, onDraftMention, onConversationDragOver, onConversationDrop,
        onSendReply, onEmojiPickerOpen, onEmojiChosen, onRetry, onRetryFailed, onDiscardFailed, trayOf, mentionSourceFor
    };
}
