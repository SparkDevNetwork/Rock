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
// The landings these tests are written to, the frame's in useLandings.partial.ts, reached through
// useFrameView.partial.ts and mounted in a bare host since the frame lives as long as the block does:
//
//   useFrameView(dependencies: { shell, activeChannelId, rootElement, linkedReply, campuses, showNotice }): {
//       landingReplyId: Ref<number | null>, landingMessageId: Ref<number | null>,
//       landOnReply(landing, origin?): Promise<void>,
//       onJump(messageId, isFromThread): Promise<void>,
//       landInConversation(channelId, messageId): Promise<"held" | "more" | "stop">
//   }
import { mount, VueWrapper } from "@vue/test-utils";
import { defineComponent, h, reactive, ref, shallowRef, watch } from "vue";
import { Frame, useFrameView } from "../../../../../src/Communication/Chat/ChatShell/composables/useFrameView.partial";
import { createTimelines } from "../../../../../src/Communication/Chat/ChatShell/composables/useHistory.partial";
import { platformMask } from "../../../../../src/Communication/Chat/ChatShell/heldMessage.partial";
import { ChatShell } from "../../../../../src/Communication/Chat/ChatShell/shell.partial";
import { HistoryPage, TimelineMessage } from "../../../../../src/Communication/Chat/ChatShell/types.partial";
import { createMessageMotion } from "../../../../../src/Communication/Chat/ChatShell/messageMotion.partial";

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

/**
 * A conversation's history, the newest page first, ten to a page: ids 100 down to 1, and in the
 * other conversation 200 down to 101, since a message's id is unique across conversations.
 */
function fetchPage(channelId: string, options: { limit: number, beforeId?: number }): Promise<HistoryPage> {
    const newest = channelId === otherChannel ? 200 : 100;
    const top = options.beforeId !== undefined ? options.beforeId - 1 : newest;
    const ids: number[] = [];
    for (let id = top; id > top - 10 && id > newest - 100; id--) {
        ids.push(id);
    }
    const messages = ids.map(id => ({ id, parent_id: null, person_alias_guid: "a0000002-0000-4000-8000-000000000000", message_type: "text", body: `m${id}`, created_at: "2026-10-18T09:00:00Z" }) as unknown as TimelineMessage);
    return Promise.resolve({ messages, read_cursor: newest, unread_count: 0, has_more: top - 10 > newest - 100 });
}


let host: VueWrapper | null = null;

afterEach(() => {
    host?.unmount();
    host = null;
});

/**
 * The landings over a stand-in shell with real timelines: selecting a conversation puts it on
 * screen and reads its newest page, as the shell's open does, and each older page waits for the
 * test to let it through while it holds them.
 */
async function setup(): Promise<{
    jump: Frame,
    timelines: ReturnType<typeof createTimelines>,
    active: { value: string | null },
    landed: number[],
    namedAtOpen: Array<number | null>,
    olderPages: Array<Deferred<void>>,
    holdOlderPages: () => void
}> {
    const timelines = createTimelines({ motion: createMessageMotion(), mask: platformMask, limits: () => ({ history_page_size: 50 }), fetchPage });
    const active = ref<string | null>(channel);
    await timelines.loadNewest(channel);

    const olderPages: Array<Deferred<void>> = [];
    let isHolding = false;
    let opening: Promise<void> = Promise.resolve();
    // the message named for the feed as each open starts, which the room's feed is drawn with
    const namedAtOpen: Array<number | null> = [];
    let jump!: Frame;
    const shell = {
        state: reactive({ hiddenPanes: { conversation: false, thread: false } }),
        timelines,
        // a second select of the conversation still opening waits for that open, as the shell's does
        selectChannel: async (channelId: string): Promise<void> => {
            namedAtOpen.push(jump.landingMessageId.value);
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

    host = mount(defineComponent({
        setup() {
            jump = useFrameView({
                shell: shallowRef(shell as unknown as ChatShell),
                activeChannelId: active,
                rootElement: ref(),
                linkedReply: null,
                campuses: [],
                showNotice: () => undefined
            });
            return () => h("div");
        }
    }));

    // Landed is named and held in the conversation on screen: a landing is named as its room opens,
    // before the room holds it, and the feed lands on it once it does.
    const landed: number[] = [];
    watch(() => {
        const id = jump.landingMessageId.value;
        const channelId = active.value;
        return id !== null && channelId !== null && timelines.state(channelId).messages.some(message => message.id === id) ? id : null;
    }, id => {
        if (id !== null) {
            landed.push(id);
        }
    }, { flush: "sync" });

    return { jump, timelines, active, landed, namedAtOpen, olderPages, holdOlderPages: () => isHolding = true };
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

        const check = await h.jump.landInConversation(otherChannel, 195);

        expect(h.active.value).toBe(otherChannel);
        expect(h.landed).toEqual([195]);
        expect(check).toBe("held");
    });

    // The room's feed is drawn with the message it opens at, so it keeps its skeleton until the
    // message is held rather than painting the newest page first and leaving it.
    test("names its message before the conversation opens", async () => {
        const h = await setup();

        await h.jump.landInConversation(otherChannel, 195);

        expect(h.namedAtOpen).toEqual([195]);
    });

    test("past two older pages, stays at the newest and answers that it was still looking", async () => {
        const h = await setup();

        const check = await h.jump.landInConversation(channel, 5);

        expect(check).toBe("more");
        expect(h.landed).toEqual([]);
        // let go, so the feed paints the newest page
        expect(h.jump.landingMessageId.value).toBeNull();
        // the newest page and two older ones, ten each
        expect(h.timelines.state(channel).messages.length).toBe(30);
    });

    // A message the conversation does not show, as a reply kept to its thread, ends the landing.
    test("that stops lets its message go, so the feed paints the newest page", async () => {
        const h = await setup();

        const check = await h.jump.landInConversation(otherChannel, 1000);

        expect(check).toBe("stop");
        expect(h.jump.landingMessageId.value).toBeNull();
    });

    // A feed drawn again for the conversation later would otherwise hold its skeleton for it.
    test("lets its message go once the person leaves its conversation", async () => {
        const h = await setup();
        await h.jump.landInConversation(otherChannel, 195);
        expect(h.jump.landingMessageId.value).toBe(195);

        h.active.value = channel;
        await settle();

        expect(h.jump.landingMessageId.value).toBeNull();
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

        const first = h.jump.landInConversation(otherChannel, 195);
        const second = h.jump.landInConversation(otherChannel, 195);

        expect(await second).toBe("held");
        expect(await first).toBe("stop");
        expect(h.jump.landingMessageId.value).toBe(195);

    });
});
