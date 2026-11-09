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
// place's draft and told to typing as the place it was typed in, and each box is given the line of
// who else is typing there. A box handing back the text it already holds is not the person typing.
import { computed, ref } from "vue";
import { useComposers } from "../../../../../src/Communication/Chat/ChatShell/composables/useComposers.partial";
import { quoteKey } from "../../../../../src/Communication/Chat/ChatShell/composables/useMessageActions.partial";
import type { ChatShell } from "../../../../../src/Communication/Chat/ChatShell/shell.partial";
import type { OpenThreadState } from "../../../../../src/Communication/Chat/ChatShell/composables/useThreads.partial";

const group = "c000000a-0000-4000-8000-000000000000";
const otherRoom = "c000000b-0000-4000-8000-000000000000";

function build() {
    const drafts: Record<string, string> = {};
    const typed: Array<[string, number | null, string]> = [];
    const lines: Record<string, string> = { [group]: "Anna is typing…", [quoteKey(otherRoom, 40)]: "Sam is typing…" };
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
            lineFor: (place: string | null) => (place ? lines[place] : null) ?? null
        }
    } as unknown as ChatShell);
    const activeChannelId = ref<string | null>(group);
    // a thread of another room, opened from the threads list
    const drawnThread = ref({ channelId: otherRoom, rootId: 40 } as OpenThreadState);
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
    return { composers, drafts, typed };
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

test("each box is given the line of who else is typing in its place", () => {
    const h = build();

    expect(h.composers.conversationTyping.value).toBe("Anna is typing…");
    expect(h.composers.threadTyping.value).toBe("Sam is typing…");
});
