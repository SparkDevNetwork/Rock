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
// The one reaction picker the block shows at a time, over the feed or the thread drawer: the
// message it reacts to, what it is placed beside, and what every message row reads of it. The
// block's frame holds it for the block's life and reads the shell through, since a birthdate save
// replaces the shell while the rows and the picker stay; the shell's reactions hold only the
// reactions themselves. The frame draws the picker and decides when another view closes it.

import { computed, ComputedRef, reactive, Ref, shallowRef, ShallowRef } from "vue";
import { PickerSource } from "../components/emojiPicker.partial.obs";
import { ChatShell } from "../shell.partial";
import { TimelineMessage } from "../types.partial";
import { ReactionHost } from "./useReactions.partial";

/**
 * The reaction picker while it is open: the message it reacts to, by id, what it is placed beside,
 * and the control that opened it, which is the anchor unless that control can leave with a choice.
 */
export type ReactionPicker = { key: number, channelId: string, messageId: number, anchor: HTMLElement, opener: HTMLElement };

/** The open picker, what the rows and every emoji picker read, and the ways to open, close and choose. */
export type ReactionPickerHost = {
    /** The picker while it is open; each opening is drawn afresh, so it is placed and pops in again. */
    reactionPicker: ShallowRef<ReactionPicker | null>;

    /**
     * What every message row reads of the reactions. Built once and read through, so a row follows
     * the shell a birthdate replaces and the picker opening, without every row being handed anew.
     */
    reactionHost: ReactionHost;

    /** What every emoji picker draws: the favorites at once, the catalog once fetched, the recents, the church's name. */
    emojiSource: ComputedRef<PickerSource>;

    /** The emoji the reader reacted to the picker's message with, which the picker marks as theirs. */
    pickerMine: ComputedRef<string[]>;

    /** Opens the picker for a message; asked again by what opened it, it closes instead. */
    openReactionPicker: (channelId: string, message: TimelineMessage, anchor: HTMLElement, opener?: HTMLElement) => void;

    closeReactionPicker: () => void;

    onPickerReact: (code: string) => void;
};

/**
 * Holds the block's one reaction picker.
 *
 * @param shell The block's shell, read on every use since a birthdate replaces it.
 * @param churchName The church's name, which the picker titles its own emoji with.
 *
 * @returns The picker host.
 */
export function useReactionPicker(shell: Readonly<Ref<ChatShell>>, churchName: string): ReactionPickerHost {
    const reactionPicker = shallowRef<ReactionPicker | null>(null);
    let reactionPickerCount = 0;

    const reactionHost = reactive<ReactionHost>({
        get favorites() {
            return shell.value.reactions.favorites;
        },
        listReactors: (messageId, code, after) => shell.value.reactions.listReactors(messageId, code, after),
        get pickerAnchor() {
            return reactionPicker.value?.anchor ?? null;
        }
    });

    const emojiSource = computed((): PickerSource => ({
        favorites: shell.value.reactions.favorites,
        catalog: shell.value.reactions.state.catalog,
        recents: shell.value.reactions.state.recents,
        churchName
    }));

    // Read through the page on every change rather than from the message the picker opened on,
    // since a page merge puts a new object in that message's place while the picker is up.
    const pickerMine = computed((): string[] => {
        const picker = reactionPicker.value;
        const message = picker ? heldMessage(picker.channelId, picker.messageId) : null;
        return (message?.reactions ?? []).filter(reaction => reaction.mine).map(reaction => reaction.code);
    });

    /**
     * The message as the client holds it now: on its conversation's timeline, else in the open
     * thread, where a reply shown only in its thread is the one copy there is.
     */
    function heldMessage(channelId: string, messageId: number): TimelineMessage | null {
        const onTimeline = shell.value.timelines.state(channelId).messages.find(message => message.id === messageId);
        if (onTimeline) {
            return onTimeline;
        }

        const thread = shell.value.threads.state.thread;
        if (!thread || thread.channelId !== channelId) {
            return null;
        }

        return thread.root?.id === messageId ? thread.root : thread.replies.find(message => message.id === messageId) ?? null;
    }

    /**
     * Opens the reaction picker for a message, closing any other first, and fetches the catalog it
     * draws. The control that opened the picker toggles it, as a press on More reactions does again.
     */
    function openReactionPicker(channelId: string, message: TimelineMessage, anchor: HTMLElement, opener: HTMLElement = anchor): void {
        const isSameOpener = reactionPicker.value?.opener === opener;
        closeReactionPicker();
        if (isSameOpener) {
            return;
        }

        opener.setAttribute("aria-expanded", "true");
        reactionPicker.value = { key: ++reactionPickerCount, channelId, messageId: message.id, anchor, opener };
        void shell.value.reactions.loadCatalog();
    }

    function closeReactionPicker(): void {
        reactionPicker.value?.opener.setAttribute("aria-expanded", "false");
        reactionPicker.value = null;
    }

    /** A choice in the reaction picker toggles the reader's reaction and is kept as recently used. */
    function onPickerReact(code: string): void {
        const picker = reactionPicker.value;
        if (!picker) {
            return;
        }

        shell.value.reactions.recordRecent(code);
        void shell.value.reactions.toggleCode(picker.channelId, picker.messageId, code);
    }

    return { reactionPicker, reactionHost, emojiSource, pickerMine, openReactionPicker, closeReactionPicker, onPickerReact };
}
