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

/** The reaction picker while it is open: the message it reacts to and what it is placed beside. */
export type ReactionPicker = { key: number, channelId: string, message: TimelineMessage, anchor: HTMLElement };

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

    openReactionPicker: (channelId: string, message: TimelineMessage, anchor: HTMLElement) => void;

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
        reactorNames: (messageId, reaction) => shell.value.reactions.reactorNames(messageId, reaction),
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

    const pickerMine = computed((): string[] => (reactionPicker.value?.message.reactions ?? [])
        .filter(reaction => reaction.mine)
        .map(reaction => reaction.code));

    /**
     * Opens the reaction picker for a message, closing any other first, and fetches the catalog it
     * draws. The message is held as the page holds it, so its marks follow while the picker is up.
     */
    function openReactionPicker(channelId: string, message: TimelineMessage, anchor: HTMLElement): void {
        closeReactionPicker();
        anchor.setAttribute("aria-expanded", "true");
        reactionPicker.value = { key: ++reactionPickerCount, channelId, message: reactive(message) as TimelineMessage, anchor };
        void shell.value.reactions.loadCatalog();
    }

    function closeReactionPicker(): void {
        reactionPicker.value?.anchor.setAttribute("aria-expanded", "false");
        reactionPicker.value = null;
    }

    /** A choice in the reaction picker toggles the reader's reaction and is kept as recently used. */
    function onPickerReact(code: string): void {
        const picker = reactionPicker.value;
        if (!picker) {
            return;
        }

        shell.value.reactions.recordRecent(code);
        void shell.value.reactions.toggleCode(picker.channelId, picker.message.id, code);
    }

    return { reactionPicker, reactionHost, emojiSource, pickerMine, openReactionPicker, closeReactionPicker, onPickerReact };
}
