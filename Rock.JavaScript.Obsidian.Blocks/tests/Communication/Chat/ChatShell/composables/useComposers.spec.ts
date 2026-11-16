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

// The composers as the block wires them: a change the person makes to a box is kept as that
// place's draft and told to typing as the place it was typed in, and the conversation and the open
// thread are each given who else is typing there, for their typing bubbles. A box handing back the
// text it already holds is not the person typing. The drawer's reply composer is filled by the
// block, so its reply's send, its key and the strip of its failed replies are worked out here.
import { computed, ref } from "vue";
import { useComposers } from "../../../../../src/Communication/Chat/ChatShell/composables/useComposers.partial";
import { quoteKey } from "../../../../../src/Communication/Chat/ChatShell/composables/useMessageActions.partial";
import type { Typing } from "../../../../../src/Communication/Chat/ChatShell/composables/usePresence.partial";
import type { ChatShell } from "../../../../../src/Communication/Chat/ChatShell/shell.partial";
import type { OpenThreadState } from "../../../../../src/Communication/Chat/ChatShell/composables/useThreads.partial";
import type { PendingMessage } from "../../../../../src/Communication/Chat/ChatShell/types.partial";

const group = "c000000a-0000-4000-8000-000000000000";
const otherRoom = "c000000b-0000-4000-8000-000000000000";

function build() {
    const drafts: Record<string, string> = {};
    const typed: Array<[string, number | null, string]> = [];
    // the person's sends of the drawer's room, and the replies sent from its composer
    const pending: PendingMessage[] = [];
    const sent: Array<[number, string, boolean]> = [];
    const sender = { pending: (channelId: string) => channelId === otherRoom ? pending : [], retryAll: jest.fn(), discard: jest.fn() };
    const typing: Record<string, Typing> = {
        [group]: { typists: [{ alias: "anna", person: null }], names: "Anna", sentence: "Anna is typing…" },
        [quoteKey(otherRoom, 40)]: { typists: [{ alias: "sam", person: null }], names: "Sam", sentence: "Sam is typing…" }
    };
    const shell = ref({
        drafts: {
            of: (place: string) => ({ text: drafts[place] ?? "", mentions: [], isAlsoSent: false }),
            write: (place: string, text: string) => {
                drafts[place] = text;
            },
            focus: () => undefined
        },
        presence: {
            typed: (channelId: string, rootId: number | null, text: string) => typed.push([channelId, rootId, text]),
            typingIn: (place: string | null) => (place ? typing[place] : null) ?? null
        },
        sender,
        sendReply: async (rootId: number, body: string, isShownInChannel: boolean) => {
            sent.push([rootId, body, isShownInChannel]);
        }
    } as unknown as ChatShell);
    const activeChannelId = ref<string | null>(group);
    // a thread of another room, opened from the threads list
    const drawnThread = ref({ channelId: otherRoom, rootId: 40, root: { id: 40 } } as OpenThreadState);
    const composers = useComposers({
        shell,
        activeChannelId,
        activeRow: ref(null),
        isJoinBarShown: ref(false),
        drawnThread,
        panelKind: ref("thread"),
        isPanelHidden: ref(false),
        conversationQuoteKey: computed(() => activeChannelId.value ? quoteKey(activeChannelId.value) : null),
        threadQuoteKey: computed(() => quoteKey(drawnThread.value.channelId, drawnThread.value.rootId))
    });
    return { composers, drafts, typed, pending, sent, sender };
}

test("a change in the conversation's box is kept as its draft and told as typing in the open conversation", () => {
    const h = build();

    h.composers.onDraftText(quoteKey(group), "Hi");

    expect(h.drafts[group]).toBe("Hi");
    expect(h.typed).toEqual([[group, null, "Hi"]]);
});

test("a change in the drawer's box is told as typing in its thread, in the thread's own room", () => {
    const h = build();

    h.composers.onDraftText(quoteKey(otherRoom, 40), "On it");

    expect(h.drafts[quoteKey(otherRoom, 40)]).toBe("On it");
    expect(h.typed).toEqual([[otherRoom, 40, "On it"]]);
});

test("a box handing back the text its draft already holds, as a send that keeps its text does, is not typing", () => {
    const h = build();
    h.drafts[group] = "Kept";

    h.composers.onDraftText(quoteKey(group), "Kept");

    expect(h.typed).toEqual([]);
});

test("the conversation and the open thread are each given who else is typing in their place", () => {
    const h = build();

    expect(h.composers.conversationTyping.value?.sentence).toBe("Anna is typing…");
    expect(h.composers.threadTyping.value?.sentence).toBe("Sam is typing…");
});

// The drawer's reply composer, which the block fills as it fills the conversation's.
describe("the drawer's reply composer", () => {
    const failed = (localId: string, extra: Record<string, unknown> = {}): PendingMessage =>
        ({ localId, channelId: otherRoom, body: localId, status: "failed", errorCode: "rpc.transport", reason: { alone: "You're offline. Try again when you're back.", besideRetry: "You're offline.", canRetry: true }, parentId: 40, isShownInChannel: false, ...extra }) as unknown as PendingMessage;
    const refusal = { code: "authz.no_mention_all_right", severity: "permission", text: "You can't mention everyone in this channel." };

    test("sends a reply to the thread alone unless the switch is on", () => {
        const h = build();

        h.composers.onSendReply("Count me in");
        h.composers.onSendReply("And Sam", false);

        expect(h.sent).toEqual([[40, "Count me in", false], [40, "And Sam", false]]);
    });

    test("sends a reply to the conversation too when the switch is on", () => {
        const h = build();

        h.composers.onSendReply("Count me in", true);

        expect(h.sent).toEqual([[40, "Count me in", true]]);
    });

    // The reply composer types into the thread, not the conversation, so what it keeps for one place
    // (the names its chips draw, what it last said aloud) is the thread's.
    test("is keyed by its thread", () => {
        const h = build();

        expect(h.composers.threadComposerKey.value).toBe("thread:40");
    });

    test("counts the thread's failed replies in its strip, this thread's only", () => {
        const h = build();
        h.pending.push(failed("a"), failed("b"), failed("elsewhere", { parentId: 99 }), failed("plain", { parentId: null }));

        expect(h.composers.threadFailure.value?.count).toBe(2);
        expect(h.composers.threadFailure.value?.reason?.besideRetry).toBe("You're offline.");
    });

    test("Try again asks for each that can go, oldest first, in one go so they are sent in turn, and Delete drops each", () => {
        const h = build();
        h.pending.push(failed("a"), failed("refused", { refusal, reason: { alone: refusal.text, besideRetry: refusal.text, canRetry: false } }), failed("b"));

        h.composers.onRetryFailed(h.composers.threadSends.value);
        expect(h.sender.retryAll.mock.calls).toEqual([[["a", "b"]]]);

        h.composers.onDiscardFailed(h.composers.threadSends.value);
        expect(h.sender.discard.mock.calls).toEqual([["a"], ["refused"], ["b"]]);
    });

    test("draws no strip while every reply is on its way", () => {
        const h = build();
        h.pending.push(failed("a", { status: "sending", errorCode: null }));

        expect(h.composers.threadFailure.value).toBeNull();
    });
});
