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
// The thread drawer, in the panel host: the root, a divider that counts the replies or invites the
// first, the replies in the conversation's look, a Follow toggle in the header and the reply
// composer, whose foot offers to send the reply to the conversation too. Escape closes it, or goes
// back to the threads list it was opened from. A phone never focuses the composer on its own,
// since the keyboard would cover the thread.
//
// The component these tests are written to, threadDrawer.partial.obs:
//
//   props: root: TimelineMessage | null, replies: TimelineMessage[] (oldest first),
//          pending: PendingMessage[] (default []), isFollowing: boolean, conversationTitle: string,
//          isChannel: boolean, isDirectMessage: boolean, isPhone: boolean, isFromList: boolean,
//          personAliasGuid: string (required)
//   emits: close, back, follow(isFollowing: boolean), send(body: string, isShownInChannel: boolean),
//          seen(messageId: number), loadOlder, retry(localId: string), discard(localId: string)
//   marks: thread-divider, thread-follow (aria-pressed), and the composer's composer-input and
//          composer-also (the foot's label around its checkbox).
//
// It sits in the shared panel host, ui/panelHost.partial.obs:
//
//   props: title: string (required), subtitle: string | null, isPhone: boolean,
//          backLabel: string | null (a back chevron with this label), closeLabel: string
//   slots: actions (header buttons), top (above the scroller), default (the body)
//   emits: close, back; Escape inside it, and the back chevron, emit back when there is a back
//          label, else close
//   marks: panel-host (labelled by its title), panel-title, panel-subtitle, panel-close (the X,
//          desktop only), panel-back (the chevron: always on a phone, labelled "Back" there unless
//          a back label says otherwise)
//
// The toggle's toasts come from useThreads' followNotice, which the shell shows once the change
// is saved.
import { mount, VueWrapper } from "@vue/test-utils";
import { nextTick } from "vue";
import ThreadDrawer from "../../../../../src/Communication/Chat/ChatShell/components/threadDrawer.partial.obs";
import MessageRow from "../../../../../src/Communication/Chat/ChatShell/components/messageRow.partial.obs";
import { followNotice } from "../../../../../src/Communication/Chat/ChatShell/composables/useThreads.partial";
import { TimelineMessage } from "../../../../../src/Communication/Chat/ChatShell/types.partial";

const me = "a0000001-0000-4000-8000-000000000000";
const anna = "a0000002-0000-4000-8000-000000000000";
const ben = "a0000003-0000-4000-8000-000000000000";

/** The channel's name as the header shows it: plain, the hash being an icon, never a character. */
const channelTitle = "young-adults";

function message(id: number, extra: Record<string, unknown> = {}): TimelineMessage {
    return {
        id,
        parent_id: null,
        person_alias_guid: anna,
        sender_nick_name: "Anna",
        sender_last_name: "Reyes",
        sender_avatar_url: null,
        sender_listed: true,
        message_type: "text",
        body: `message ${id}`,
        pieces: null,
        blocks: null,
        created_at: new Date(2026, 9, 13, 9, id % 60).toISOString(),
        edited_at: null,
        deleted_at: null,
        visibility: "visible",
        visibility_notice: null,
        ...extra
    } as unknown as TimelineMessage;
}

function rootWith(replyCount: number): TimelineMessage {
    return message(40, {
        body: "Who's in for Saturday's hike?",
        reply_count: replyCount,
        last_reply_at: replyCount > 0 ? new Date(2026, 9, 13, 9, 41).toISOString() : null,
        repliers: replyCount > 0 ? [{ person_alias_guid: ben, nick_name: "Ben", last_name: "Okafor", avatar_url: null }] : null,
        thread_following: true,
        thread_unread: false
    });
}

function replies(count: number): TimelineMessage[] {
    return Array.from({ length: count }, (_, i) => message(41 + i, { parent_id: 40, person_alias_guid: ben, sender_nick_name: "Ben", sender_last_name: "Okafor" }));
}

let wrapper: VueWrapper | null = null;

function draw(props: Record<string, unknown> = {}): VueWrapper {
    wrapper = mount(ThreadDrawer, {
        props: {
            root: rootWith(3),
            replies: replies(3),
            pending: [],
            isFollowing: false,
            conversationTitle: channelTitle,
            isChannel: true,
            isDirectMessage: false,
            isPhone: false,
            isFromList: false,
            personAliasGuid: me,
            ...props
        },
        attachTo: document.body
    });
    return wrapper;
}

/** Text as a person reads it. */
function plain(text: string | undefined | null): string {
    return (text ?? "").replace(/\s+/g, " ").trim();
}

/** Longer than the drawer waits before it focuses the composer. */
async function afterOpen(): Promise<void> {
    await nextTick();
    await new Promise(resolve => setTimeout(resolve, 150));
    await nextTick();
}

beforeAll(() => {
    // jsdom has no ResizeObserver or IntersectionObserver; the feed and the tooltips use them.
    window.ResizeObserver ??= class {
        observe(): void { /* jsdom lays nothing out. */ }
        unobserve(): void { /* As above. */ }
        disconnect(): void { /* As above. */ }
    } as unknown as typeof ResizeObserver;
    window.IntersectionObserver ??= class {
        observe(): void { /* jsdom computes no intersections. */ }
        unobserve(): void { /* As above. */ }
        disconnect(): void { /* As above. */ }
        takeRecords(): IntersectionObserverEntry[] {
            return [];
        }
    } as unknown as typeof IntersectionObserver;
});

afterEach(() => {
    wrapper?.unmount();
    wrapper = null;
    document.body.innerHTML = "";
});

describe("the header", () => {
    test("titles the thread and names its conversation, labelled by its title", () => {
        const w = draw();

        expect(w.get("[data-testid='panel-title']").text()).toBe("Thread");
        expect(w.get("[data-testid='panel-subtitle']").text()).toBe(channelTitle);
        expect(w.get("[data-testid='panel-host']").attributes("aria-label")).toBe("Thread");
    });

    test("names a direct message's conversation by its title", () => {
        const w = draw({ conversationTitle: "Anna Reyes", isChannel: false, isDirectMessage: true });

        expect(w.get("[data-testid='panel-subtitle']").text()).toBe("Anna Reyes");
    });

    test("closes from its X on a wide block, with no back chevron", async () => {
        const w = draw();

        const close = w.get("[data-testid='panel-close']");
        expect(close.attributes("aria-label")).toBe("Close thread");
        expect(w.find("[data-testid='panel-back']").exists()).toBe(false);

        await close.trigger("click");
        expect(w.emitted("close")).toHaveLength(1);
    });
});

describe("the body", () => {
    test("draws the root, then the replies, in the channel's look", () => {
        const w = draw();

        const rows = w.findAllComponents(MessageRow);
        expect(rows.map(r => (r.props("message") as TimelineMessage | null)?.id)).toEqual([40, 41, 42, 43]);
        expect(rows.every(r => r.props("isChannel") === true)).toBe(true);
    });

    test("draws no thread pill under the root and no reply label over a reply also sent", () => {
        const w = draw({ replies: [...replies(2), message(43, { parent_id: 40, shown_in_channel: true })] });

        expect(w.find("[data-testid='thread-summary']").exists()).toBe(false);
        expect(w.find("[data-testid='thread-reply-label']").exists()).toBe(false);
    });

    test("draws this root's replies still on their way after the stored ones, and no other root's", () => {
        const sending = { localId: "mine", channelId: "c1", body: "On my way", status: "sending", errorCode: null, parentId: 40, isShownInChannel: false };
        const elsewhere = { ...sending, localId: "elsewhere", parentId: 99 };
        const plainSend = { ...sending, localId: "conversation", parentId: null };
        const w = draw({ pending: [sending, elsewhere, plainSend] });

        const rows = w.findAllComponents(MessageRow);
        expect(rows.map(r => (r.props("pending") as { localId: string } | null)?.localId ?? null)).toEqual([null, null, null, null, "mine"]);
    });

    test("draws a direct message's thread in its look", () => {
        const w = draw({ conversationTitle: "Anna Reyes", isChannel: false, isDirectMessage: true });

        expect(w.findAllComponents(MessageRow).every(r => r.props("isChannel") === false)).toBe(true);
    });
});

describe("the divider", () => {
    test.each([[3, "3 replies"], [1, "1 reply"]])("counts %i replies as %s", (count, words) => {
        const w = draw({ root: rootWith(count), replies: replies(count) });

        expect(plain(w.get("[data-testid='thread-divider']").text())).toBe(words);
    });

    test("counts every reply of the thread, not only the ones loaded", () => {
        const w = draw({ root: rootWith(50), replies: replies(3) });

        expect(plain(w.get("[data-testid='thread-divider']").text())).toBe("50 replies");
    });

    test.each([[99, "99 replies"], [100, "99+ replies"], [120, "99+ replies"]])("counts %i replies as %s, as the thread pill does", (count, words) => {
        const w = draw({ root: rootWith(count), replies: replies(3) });

        expect(plain(w.get("[data-testid='thread-divider']").text())).toBe(words);
    });

    test("invites the first reply when there is none", () => {
        const w = draw({ root: rootWith(0), replies: [] });

        expect(plain(w.get("[data-testid='thread-divider']").text())).toBe("No replies yet. Start the thread below.");
    });
});

describe("the Follow toggle", () => {
    test("reads Follow and asks to follow", async () => {
        const w = draw({ isFollowing: false });

        const toggle = w.get("[data-testid='thread-follow']");
        expect(plain(toggle.text())).toBe("Follow");
        expect(toggle.attributes("aria-pressed")).toBe("false");

        await toggle.trigger("click");
        expect(w.emitted("follow")).toEqual([[true]]);
    });

    test("reads Following and asks to stop", async () => {
        const w = draw({ isFollowing: true });

        const toggle = w.get("[data-testid='thread-follow']");
        expect(plain(toggle.text())).toBe("Following");
        expect(toggle.attributes("aria-pressed")).toBe("true");

        await toggle.trigger("click");
        expect(w.emitted("follow")).toEqual([[false]]);
    });

    test("is not offered while the thread's first page is on its way, since whether the reader follows is not known yet", () => {
        const w = draw({ root: null, replies: [], isLoading: true, isFollowing: false });

        expect(w.find("[data-testid='thread-follow']").exists()).toBe(false);
    });

    test("is offered once the first page has answered", async () => {
        const w = draw({ root: null, replies: [], isLoading: true });
        await w.setProps({ root: rootWith(3), replies: replies(3), isLoading: false, isFollowing: true });

        expect(plain(w.get("[data-testid='thread-follow']").text())).toBe("Following");
    });

    test("is not offered after the first page failed, since whether the reader follows is still not known", async () => {
        const w = draw({ root: null, replies: [], isLoading: true });
        await w.setProps({ isLoading: false, error: { code: "net.offline", severity: "degraded", text: null } });

        expect(w.find("[data-testid='thread-error']").exists()).toBe(true);
        expect(w.find("[data-testid='thread-follow']").exists()).toBe(false);
    });

    test("stays offered after a page of older replies failed, with that failure said", async () => {
        const w = draw({ root: rootWith(3), replies: replies(3), isLoading: false, isFollowing: true });
        await w.setProps({ olderError: { code: "net.offline", severity: "degraded", text: null } });

        expect(w.find("[data-testid='thread-error']").exists()).toBe(true);
        expect(plain(w.get("[data-testid='thread-follow']").text())).toBe("Following");
    });

    test("toasts the change in the design's words", () => {
        expect(followNotice(true).text).toBe("Following this thread");
        expect(followNotice(false).text).toBe("You won't be notified about new replies");
    });
});

describe("the reply composer", () => {
    test("asks for a reply in the thread", () => {
        const w = draw();

        expect(w.get("[data-testid='composer-input']").attributes("placeholder")).toBe("Reply in thread");
    });

    test("offers to send the reply to the channel too, by the channel's name", () => {
        const w = draw();

        expect(plain(w.get("[data-testid='composer-also']").text())).toBe(`Also send to ${channelTitle}`);
        expect(w.find("[data-testid='composer-hint']").exists()).toBe(false);
    });

    test("offers to send the reply to a direct message as the conversation", () => {
        const w = draw({ conversationTitle: "Anna Reyes", isChannel: false, isDirectMessage: true });

        expect(plain(w.get("[data-testid='composer-also']").text())).toBe("Also send to the conversation");
    });

    test("sends a reply to the thread alone unless the box is checked", async () => {
        const w = draw();

        await w.get("[data-testid='composer-input']").setValue("Count me in");
        await w.get("[data-testid='composer-input']").trigger("keydown", { key: "Enter" });

        expect(w.emitted("send")).toEqual([["Count me in", false]]);
    });

    test("sends a checked reply to the conversation too", async () => {
        const w = draw();

        await w.get("[data-testid='composer-also'] input[type='checkbox']").setValue(true);
        await w.get("[data-testid='composer-input']").setValue("Count me in");
        await w.get("[data-testid='composer-input']").trigger("keydown", { key: "Enter" });

        expect(w.emitted("send")).toEqual([["Count me in", true]]);
    });
});

describe("Escape", () => {
    test("closes a thread opened from the conversation", async () => {
        const w = draw();

        await w.get("[data-testid='panel-host']").trigger("keydown", { key: "Escape" });

        expect(w.emitted("close")).toHaveLength(1);
        expect(w.emitted("back")).toBeUndefined();
    });

    test("returns to the threads list a thread was opened from, as its back chevron does", async () => {
        const w = draw({ isFromList: true });

        const back = w.get("[data-testid='panel-back']");
        expect(back.attributes("aria-label")).toBe("Back to Threads");

        await w.get("[data-testid='panel-host']").trigger("keydown", { key: "Escape" });
        expect(w.emitted("back")).toHaveLength(1);
        expect(w.emitted("close")).toBeUndefined();

        await back.trigger("click");
        expect(w.emitted("back")).toHaveLength(2);
    });
});

describe("focus on open", () => {
    test("goes to the reply composer on a wide block", async () => {
        const w = draw();
        await afterOpen();

        expect(document.activeElement).toBe(w.get("[data-testid='composer-input']").element);
    });

    test("goes to the back chevron on a phone, never the composer, and the phone has no X", async () => {
        const w = draw({ isPhone: true });
        await afterOpen();

        expect(document.activeElement).not.toBe(w.get("[data-testid='composer-input']").element);
        expect(document.activeElement).toBe(w.get("[data-testid='panel-back']").element);
        expect(w.get("[data-testid='panel-back']").attributes("aria-label")).toBe("Back");
        expect(w.find("[data-testid='panel-close']").exists()).toBe(false);

        await w.get("[data-testid='panel-back']").trigger("click");
        expect(w.emitted("close")).toHaveLength(1);
    });
});
