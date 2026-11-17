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
// The question Leave asks before it goes, shared by every Leave the chat offers: a room's details
// and the footer of a channel's Browse preview. Each draws the question in its own place; this
// owns what the question says and where focus stands through it. Asking puts focus on Cancel, so
// Enter never leaves by accident; Cancel gives it back to the Leave that asked; a refusal swaps
// Leave for its sentence and puts focus on OK, so the sentence is not missed; and a leave that
// lands moves focus to what the view offers next, unless the person had moved it elsewhere.

import { computed, ComputedRef, nextTick, Ref, ref, watch } from "vue";
import { failedWords, offlineWords } from "../errors.partial";
import { LeaveState } from "./useRooms.partial";

/** Something focus can be handed to: an element, or a part that focuses its own. */
type Focusable = { focus: () => void } | null | undefined;

/** What one place that asks the question gives it. */
export type LeaveQuestionOptions = {
    /** The room's Leave, as the rooms hold it. */
    leave: () => LeaveState;

    /** Asks the door, once the person answers the question with Leave. */
    go: () => void;

    /** The question's Cancel, which reads OK once the leave is stopped. */
    cancelButton: () => Focusable;

    /** The Leave that asked, which Cancel gives focus back to. */
    askButton: () => Focusable;

    /**
     * Where the view goes on once the leave lands: the question ends, and focus that was in its
     * region moves to what the view offers next, or to the region where that is not drawn. Left
     * out where the view closes with the leave and gives focus back itself.
     */
    afterLeft?: {
        region: () => HTMLElement | null | undefined;
        next: () => Focusable;
    };
};

/** The question as one place draws it. */
export type LeaveQuestion = {
    /** Whether the question is asked, in Leave's place. */
    isAsking: Ref<boolean>;

    /** Whether the door is out, which holds Leave busy and Cancel still. */
    isLeaving: ComputedRef<boolean>;

    /** What stopped the leave, in the door's own words, in the question's place; null while none did. */
    error: ComputedRef<string | null>;

    /** Leave was pressed: the question is asked. */
    ask: () => Promise<void>;

    /** Cancel, OK or Escape: the question is taken back, unless the door is out. */
    cancel: () => Promise<void>;

    /** The question was answered with Leave. */
    confirm: () => void;

    /**
     * The person is no longer in the room, by this leave or any other way: the question ends, and
     * focus that was in it goes where the view goes on. Called before the view redraws, while what
     * held focus is still drawn.
     */
    end: () => Promise<void>;
};

/** What a failure to reach the door says. */
const leaveFailed = failedWords("Couldn't leave.");

/**
 * What leaving means for a room, said once under the question: a public channel can be joined
 * again, a private one or a group cannot.
 *
 * @param isGroup Whether the room is a conversation between people rather than a channel.
 * @param isPublic Whether the channel is one anyone may join from Browse channels.
 * @param churchName The church the room's Rock group is at.
 *
 * @returns The sentence.
 */
export function leaveSentence(isGroup: boolean, isPublic: boolean, churchName: string): string {
    if (isGroup) {
        return "The others will see that you left. To come back, someone in the conversation will need to add you.";
    }

    const leaving = `You'll stop getting its messages and leave the group at ${churchName}.`;
    return isPublic ? `${leaving} You can join again from Browse channels.` : `${leaving} To come back, a leader will need to add you.`;
}

/**
 * The question for one place that asks it.
 *
 * @param options What that place gives the question.
 *
 * @returns The question's state and its answers.
 */
export function useLeaveQuestion(options: LeaveQuestionOptions): LeaveQuestion {
    const isAsking = ref(false);

    /** Whether Leave was answered in this question, so an answer to an earlier one is never shown in it. */
    const hasAsked = ref(false);

    /** The leave as this question sees it: idle until it asks. */
    const shown = computed((): LeaveState => hasAsked.value ? options.leave() : { kind: "idle" });

    const isLeaving = computed((): boolean => shown.value.kind === "leaving");

    const error = computed((): string | null => {
        const state = shown.value;
        if (state.kind === "refused") {
            return state.text;
        }

        if (state.kind === "failed") {
            return leaveFailed.alone;
        }

        return state.kind === "offline" ? offlineWords.alone : null;
    });

    async function ask(): Promise<void> {
        hasAsked.value = false;
        isAsking.value = true;
        await nextTick();
        options.cancelButton()?.focus();
    }

    async function cancel(): Promise<void> {
        if (isLeaving.value) {
            return;
        }

        isAsking.value = false;
        await nextTick();
        options.askButton()?.focus();
    }

    function confirm(): void {
        if (!isLeaving.value) {
            hasAsked.value = true;
            options.go();
        }
    }

    async function end(): Promise<void> {
        if (!isAsking.value) {
            return;
        }

        const after = options.afterLeft;
        const region = after?.region();
        const hadFocus = !!region && region.contains(document.activeElement);
        isAsking.value = false;
        hasAsked.value = false;
        if (!after || !hadFocus) {
            return;
        }

        await nextTick();
        (after.next() ?? after.region())?.focus();
    }

    // A refusal takes Leave's place with OK, which takes focus so the sentence is not missed.
    watch(error, async now => {
        if (now && isAsking.value) {
            await nextTick();
            options.cancelButton()?.focus();
        }
    });

    // A view that goes on past the leave ends the question as it lands; one that closes with it
    // keeps the question as it was through its exit.
    watch(() => shown.value.kind, kind => {
        if (kind === "left" && options.afterLeft) {
            void end();
        }
    }, { flush: "pre" });

    return { isAsking, isLeaving, error, ask, cancel, confirm, end };
}
