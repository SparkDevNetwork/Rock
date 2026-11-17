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
// The frame's landings on one message that may not be drawn yet: a reply a push's tap or a link
// names, opened in its thread, the original a quote jumps to, and a mention the inbox opens in its
// conversation. All of them fetch at most a few older pages through one loop, and each look checks
// that the view it started in is still there and that no landing has been asked for since, so only
// the newest lands.
//
// Kept apart from the frame, which opens and closes the drawer they land in, since a landing has a
// lifetime of its own: it outlives the move that asked for it, across the pages it waits on.

import { nextTick, ref, Ref } from "vue";
import { ChatShell } from "../shell.partial";
import type { ThreadOrigin } from "./useFrameView.partial";
import { ThreadPlace } from "./useThreads.partial";

// #region Types

/** A thread to open in a conversation, at a reply when it names one: from a push's tap, a link, or a message's own thread. */
export type ReplyLanding = ThreadPlace & { channelId: string };

/** Where a landing stands after a look: its message is held, an older page may hold it, or it is over. */
export type LandingCheck = "held" | "more" | "stop";

/** What the landings read of the frame, which opens and closes the drawer they land in. */
export type LandingDependencies = {
    /** The block's shell, read on every use since a birthdate replaces it. */
    shell: Readonly<Ref<ChatShell>>;

    activeChannelId: Readonly<Ref<string | null>>;

    isPhone: Readonly<Ref<boolean>>;

    /** Opens a thread in the drawer, keeping the list it was opened from. */
    openThreadDrawer: (channelId: string, rootId: number, origin: ThreadOrigin) => Promise<void>;

    closeThreadDrawer: () => void;

    /** Counts every thread opened or left, so a landing can tell the drawer it started in is gone. */
    threadOpenCount: () => number;
};

/** The landings, and the message each view scrolls to and flashes. */
export type Landings = {
    /** The reply the drawer scrolls to and flashes once it holds it, or null. */
    landingReplyId: Ref<number | null>;

    /** The message the feed scrolls to and flashes once its timeline holds it, or null. */
    landingMessageId: Ref<number | null>;

    landOnReply: (landing: ReplyLanding, origin?: ThreadOrigin) => Promise<void>;

    onJump: (messageId: number, isFromThread: boolean) => Promise<void>;

    /**
     * Opens a conversation, as a sidebar row does, and lands on a message in it. Answers "more"
     * when the pages a landing fetches ran out still looking, so the caller can say the message
     * is too far back; "held" once it landed; "stop" when it did not and is not still looking.
     */
    landInConversation: (channelId: string, messageId: number) => Promise<LandingCheck>;
};

// #endregion Types

/** A landing fetches at most this many older pages before it stays at the newest. */
const maxLandingOlderPages = 2;

/**
 * Creates the frame's landings.
 *
 * @param dependencies What the landings read of the frame.
 *
 * @returns The landings and what each view lands on.
 */
export function useLandings(dependencies: LandingDependencies): Landings {
    const { shell, activeChannelId, isPhone, openThreadDrawer, closeThreadDrawer, threadOpenCount } = dependencies;

    const landingReplyId = ref<number | null>(null);
    const landingMessageId = ref<number | null>(null);

    /** Counts every landing asked for, so one a newer landing replaced stops looking. */
    let landingCount = 0;

    /**
     * A quote asked to jump to the message it quotes. In a thread, a reply the drawer holds is
     * landed on there; anything else is in the conversation, which a phone brings back to the
     * front. The original is usually recent, so the timeline nearly always holds it; when not, a
     * few older pages are fetched, as for a reply a push opened, and past them the jump does nothing.
     */
    async function onJump(messageId: number, isFromThread: boolean): Promise<void> {
        const thisLanding = ++landingCount;
        const thread = shell.value.threads.state.thread;
        if (isFromThread && thread?.replies.some(reply => reply.id === messageId)) {
            // Let go first, so a second jump to the reply the drawer last landed on lands again.
            landingReplyId.value = null;
            await nextTick();
            if (thisLanding === landingCount) {
                landingReplyId.value = messageId;
            }
            return;
        }

        const channelId = activeChannelId.value;
        if (!channelId) {
            return;
        }

        if (isFromThread && isPhone.value) {
            closeThreadDrawer();
        }

        await landInFeed(channelId, messageId, thisLanding);
    }

    /**
     * Opens a thread at a reply. A push or a link is about a recent reply, so the newest page
     * nearly always holds it; when not, a few older pages are fetched before the drawer simply
     * stays at the newest. Each await checks the drawer is still the one this opened, so one
     * closed or replaced meanwhile asks for nothing more and flashes nothing. A reply opened from
     * a list keeps the list to go back to.
     */
    async function landOnReply(landing: ReplyLanding, origin: ThreadOrigin = null): Promise<void> {
        const thisLanding = ++landingCount;
        // Let go first, and let the drawer see it before landing, so a landing on the reply it
        // last landed on lands again, in the thread already open as in one newly opened.
        landingReplyId.value = null;
        const opened = openThreadDrawer(landing.channelId, landing.rootId, origin);
        const thisOpen = threadOpenCount();
        await opened;
        await nextTick();

        const threads = shell.value.threads;
        await loadUntilHeld(() => {
            const thread = threads.state.thread;
            if (thisLanding !== landingCount || thisOpen !== threadOpenCount() || !thread || thread.rootId !== landing.rootId || landing.messageId === null) {
                return "stop";
            }

            if (thread.replies.some(reply => reply.id === landing.messageId)) {
                landingReplyId.value = landing.messageId;
                return "held";
            }

            // A failed page is shown in the drawer; trying again here would only repeat it.
            return !thread.hasMore || thread.error || thread.olderError ? "stop" : "more";
        }, () => threads.loadOlderReplies());
    }

    /**
     * Looks for a message in the conversation on screen, fetching a few older pages while it may
     * be further back, and lands on it once held. Stops when the person has left the conversation
     * or asked for another landing meanwhile.
     *
     * @param thisLanding The landing's number, which must still be the newest at every look.
     */
    async function landInFeed(channelId: string, messageId: number, thisLanding: number): Promise<LandingCheck> {
        // Let go first, so a second landing on the message the feed last landed on lands again.
        landingMessageId.value = null;
        await nextTick();

        let oldestSeen: number | undefined;
        return loadUntilHeld(() => {
            const timeline = shell.value.timelines.state(channelId);
            const oldest = timeline.messages[0]?.id;
            if (thisLanding !== landingCount || activeChannelId.value !== channelId || oldest === undefined) {
                return "stop";
            }

            if (timeline.messages.some(message => message.id === messageId)) {
                landingMessageId.value = messageId;
                return "held";
            }

            // An older page that failed or was dropped moved nothing, and one past the message
            // without it means the conversation does not show it, as a reply kept to its thread
            // or a message since deleted: neither is too far back.
            const isStuck = oldest === oldestSeen || oldest < messageId;
            oldestSeen = oldest;
            return isStuck || !timeline.hasMore ? "stop" : "more";
        }, () => shell.value.loadOlder().catch(() => undefined)); // the shell has said why; the look above stops on it
    }

    /**
     * Opens a conversation as a sidebar row does, waiting for an open of it already under way,
     * then lands on a message in it. The landing is numbered as it is asked, before the open, so
     * one asked for later is never overtaken by this one's open finishing after it.
     */
    async function landInConversation(channelId: string, messageId: number): Promise<LandingCheck> {
        const thisLanding = ++landingCount;
        await shell.value.selectChannel(channelId);
        return thisLanding === landingCount ? landInFeed(channelId, messageId, thisLanding) : "stop";
    }

    return { landingReplyId, landingMessageId, landOnReply, onJump, landInConversation };
}

/**
 * Looks for a landing's message, fetching at most a few older pages while the check says one
 * may hold it. The check lands the message itself once it is held, in the same step that
 * found the view unchanged, so a view replaced meanwhile is never landed in.
 *
 * @param check Looks once and says where the landing stands.
 * @param loadOlder Fetches the next older page.
 *
 * @returns The last look's answer: "more" when the pages ran out still looking.
 */
async function loadUntilHeld(check: () => LandingCheck, loadOlder: () => Promise<void>): Promise<LandingCheck> {
    let last = check();
    for (let olderPages = 0; last === "more" && olderPages < maxLandingOlderPages; olderPages++) {
        await loadOlder();
        last = check();
    }

    return last;
}
