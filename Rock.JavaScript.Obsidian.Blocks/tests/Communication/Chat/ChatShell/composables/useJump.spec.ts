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
// Landing on a message in the conversation: a quote's jump to the original it quotes, and a
// mention's row in the inbox, which opens its conversation first. A landing fetches at most two
// older pages, and answers whether it is still looking once they ran out, so the inbox can say
// the message is too far back. The newest landing asked for is the only one that lands: one
// asked before it, still fetching, never scrolls or flashes once the newer has, and nor does one
// whose conversation the person left.
//
// The composable these tests are written to, in useJump.partial.ts:
//
//   useJump(dependencies: { shell, activeChannelId, isPhone, threadOpenCount, openThreadDrawer, closeThreadDrawer }): {
//       landingReplyId: Ref<number | null>, landingMessageId: Ref<number | null>,
//       landOnReply(landing, origin?): Promise<void>,
//       onJump(messageId, isFromThread): Promise<void>,
//       landInConversation(channelId, messageId): Promise<"held" | "more" | "stop">
//   }
import { ref, shallowRef, watch } from "vue";
import { useJump } from "../../../../../src/Communication/Chat/ChatShell/composables/useJump.partial";
import { createTimelines } from "../../../../../src/Communication/Chat/ChatShell/composables/useHistory.partial";
import { ChatShell } from "../../../../../src/Communication/Chat/ChatShell/shell.partial";
import { HistoryPage, TimelineMessage } from "../../../../../src/Communication/Chat/ChatShell/types.partial";

// #region Fixtures

const channel = "c0000001-0000-4000-8000-000000000000";
const otherChannel = "c0000002-0000-4000-8000-000000000000";

type Deferred<T> = { promise: Promise<T>, resolve: (value: T) => void };

function deferred<T>(): Deferred<T> {
    let resolve!: (value: T) => void;
    const promise = new Promise<T>(r => resolve = r);
    return { promise, resolve };
}

async function settle(): Promise<void> {
    for (let i = 0; i < 10; i++) {
        await Promise.resolve();
    }
}

/** A conversation's history: ids 100 down to 1, the newest page first, ten to a page. */
function fetchPage(_channelId: string, options: { limit: number, beforeId?: number }): Promise<HistoryPage> {
    const top = options.beforeId !== undefined ? options.beforeId - 1 : 100;
    const ids: number[] = [];
    for (let id = top; id > top - 10 && id > 0; id--) {
        ids.push(id);
    }
    const messages = ids.map(id => ({ id, parent_id: null, person_alias_guid: "a0000002-0000-4000-8000-000000000000", message_type: "text", body: `m${id}`, created_at: "2026-10-18T09:00:00Z" }) as unknown as TimelineMessage);
    return Promise.resolve({ messages, read_cursor: 100, unread_count: 0, has_more: top - 10 > 0 });
}

/**
 * The landings over a stand-in shell with real timelines: selecting a conversation puts it on
 * screen and reads its newest page, as the shell's open does, and each older page waits for the
 * test to let it through while it holds them.
 */
async function setup(): Promise<{
    jump: ReturnType<typeof useJump>,
    timelines: ReturnType<typeof createTimelines>,
    active: { value: string | null },
    landed: number[],
    olderPages: Array<Deferred<void>>,
    holdOlderPages: () => void
}> {
    const timelines = createTimelines({ limits: () => ({ history_page_size: 50 }), fetchPage });
    const active = ref<string | null>(channel);
    await timelines.loadNewest(channel);

    const olderPages: Array<Deferred<void>> = [];
    let isHolding = false;
    let opening: Promise<void> = Promise.resolve();
    const shell = {
        timelines,
        // a second select of the conversation still opening waits for that open, as the shell's does
        selectChannel: async (channelId: string): Promise<void> => {
            if (channelId !== active.value) {
                active.value = channelId;
                opening = timelines.loadNewest(channelId);
            }
            await opening;
        },
        loadOlder: async (): Promise<void> => {
            const channelId = active.value!;
            if (isHolding) {
                const gate = deferred<void>();
                olderPages.push(gate);
                await gate.promise;
            }
            await timelines.loadOlder(channelId);
        },
        threads: { state: { thread: null }, loadOlderReplies: async () => undefined }
    };

    const jump = useJump({
        shell: shallowRef(shell as unknown as ChatShell),
        activeChannelId: active,
        isPhone: ref(false),
        threadOpenCount: () => 0,
        openThreadDrawer: async () => undefined,
        closeThreadDrawer: () => undefined
    });

    const landed: number[] = [];
    watch(jump.landingMessageId, id => {
        if (id !== null) {
            landed.push(id);
        }
    }, { flush: "sync" });

    return { jump, timelines, active, landed, olderPages, holdOlderPages: () => isHolding = true };
}

// #endregion Fixtures

describe("a quote's jump", () => {
    test("lands on an original the conversation holds", async () => {
        const h = await setup();

        await h.jump.onJump(95, false);

        expect(h.landed).toEqual([95]);
    });

    test("fetches an older page for an original just past the newest", async () => {
        const h = await setup();

        await h.jump.onJump(85, false);

        expect(h.landed).toEqual([85]);
    });

    test("overtaken by a second jump while it fetches, never lands once the second has", async () => {
        const h = await setup();
        h.holdOlderPages();

        const first = h.jump.onJump(85, false);
        await settle();
        expect(h.olderPages).toHaveLength(1);

        await h.jump.onJump(95, false);
        expect(h.landed).toEqual([95]);

        h.olderPages[0].resolve();
        await first;

        expect(h.landed).toEqual([95]);
        expect(h.jump.landingMessageId.value).toBe(95);
    });
});

describe("a landing in a conversation from elsewhere", () => {
    test("opens the conversation and lands on the message", async () => {
        const h = await setup();

        const check = await h.jump.landInConversation(otherChannel, 95);

        expect(h.active.value).toBe(otherChannel);
        expect(h.landed).toEqual([95]);
        expect(check).toBe("held");
    });

    test("past two older pages, stays at the newest and answers that it was still looking", async () => {
        const h = await setup();

        const check = await h.jump.landInConversation(channel, 5);

        expect(check).toBe("more");
        expect(h.landed).toEqual([]);
        // the newest page and two older ones, ten each
        expect(h.timelines.state(channel).messages.length).toBe(30);
    });

    test("overtaken by the person opening another conversation, never lands there, even once its page comes", async () => {
        const h = await setup();
        h.holdOlderPages();

        const landing = h.jump.landInConversation(channel, 85);
        await settle();
        expect(h.olderPages).toHaveLength(1);

        h.active.value = otherChannel;
        h.olderPages[0].resolve();

        expect(await landing).toBe("stop");
        expect(h.landed).toEqual([]);
        expect(h.olderPages).toHaveLength(1);
    });

    test("overtaken by a quote's jump while it fetches, never lands and is not still looking", async () => {
        const h = await setup();
        h.holdOlderPages();

        const landing = h.jump.landInConversation(channel, 5);
        await settle();
        await h.jump.onJump(95, false);

        h.olderPages[0].resolve();
        await settle();
        h.olderPages[1]?.resolve();

        expect(await landing).toBe("stop");
        expect(h.landed).toEqual([95]);
    });

    test("asked twice for the same message, as a double click asks, lands once the conversation is in", async () => {
        const h = await setup();

        const first = h.jump.landInConversation(otherChannel, 95);
        const second = h.jump.landInConversation(otherChannel, 95);

        expect(await second).toBe("held");
        expect(await first).toBe("stop");
        expect(h.jump.landingMessageId.value).toBe(95);
    });
});
