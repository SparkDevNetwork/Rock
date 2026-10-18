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
// The mentions inbox: where people named the reader, newest first, under day headings. A row is
// one button whose label says the whole row; it draws the author's face with an @ badge, their
// name, the room's name from the sidebar, the time, the text in three lines as the message body
// draws it, and on a reply a "Reply in a thread" tag. A row newer than the last visit is New with
// a dot, until it is opened; the row just opened is marked current. A channel row jumps to the
// message and a reply opens its thread. Paging is "Load older mentions", "Loading" while busy,
// then "That's every mention"; a failed page and a failed refresh each say so in place.
//
// The component these tests are written to, mentionsPanel.partial.obs, in the shared panel host:
//
//   props: rows: MentionRow[] (chat_list_mentions' rows), hasMore, isLoading, isRefreshing,
//          isLoadingOlder: boolean, error: ChatError | null (a refresh's), olderError: ChatError |
//          null (an older page's), newIds: number[] (the messages New for this visit), currentId:
//          number | null (the row just opened), roomTitles, roomKinds, publicRooms (as the threads
//          panel takes them), isPhone: boolean, personAliasGuid: string
//   emits: jump(messageId: number, channelId: string), openThread(rootId: number, channelId: string,
//          messageId: number), refresh, loadOlder, close
//   marks: mentions-panel (the panel), mention-row (each row, class is-new and is-current),
//          mention-row-open (the row's button, labelled), mention-row-new (a New row's dot),
//          mention-row-thread (the reply tag), mentions-day (each day heading), mentions-more
//          (Load older mentions), mentions-end (the end note), mentions-error (a refresh's
//          failure, with Try again), mentions-older-error, mentions-empty, mentions-refresh,
//          mention-skeleton. The panel host's marks are threadDrawer.spec's.
import { mount, VueWrapper } from "@vue/test-utils";
import MentionsPanel from "../../../../../src/Communication/Chat/ChatShell/components/mentionsPanel.partial.obs";
import MessageBody from "../../../../../src/Communication/Chat/ChatShell/components/messageBody.partial.obs";
import Icon from "../../../../../src/Communication/Chat/ChatShell/ui/icon.partial.obs";
import { ChatError, MentionRow, TimelineMessage } from "../../../../../src/Communication/Chat/ChatShell/types.partial";

// #region Fixtures

const me = "a0000001-0000-4000-8000-000000000000";
const sam = "a0000004-0000-4000-8000-000000000000";
const channel = "c0000001-0000-4000-8000-000000000000";
const worship = "c0000002-0000-4000-8000-000000000000";

const roomTitles: Record<string, string> = { [channel]: "young-adults", [worship]: "Worship Team" };
const roomKinds: Record<string, "channel" | "person" | "group"> = { [channel]: "channel", [worship]: "channel" };

/** A time today, or yesterday, in local time, as the wire carries it. */
function at(hours: number, minutes: number, daysAgo = 0): string {
    const when = new Date();
    when.setDate(when.getDate() - daysAgo);
    when.setHours(hours, minutes, 0, 0);
    return when.toISOString();
}

const offline: ChatError = { code: "rpc.transport", severity: "failed" };

/** One row of chat_list_mentions, exactly its keys, its message as a history page carries it. */
function row(id: number, extra: { channel_id?: string, parent_id?: number | null, created_at?: string, body?: string } = {}): MentionRow {
    const parentId = extra.parent_id ?? null;
    const body = extra.body ?? "are you in for Saturday?";
    return {
        channel_id: extra.channel_id ?? channel,
        parent_id: parentId,
        message: {
            id,
            parent_id: parentId,
            quoted_message_id: null,
            quoted: null,
            shown_in_channel: false,
            person_alias_guid: sam,
            sender_nick_name: "Sam",
            sender_last_name: "Ortiz",
            sender_avatar_url: null,
            sender_listed: true,
            message_type: "text",
            body: `<@${me}> ${body}`,
            pieces: [{ type: "mention", text: "@Jordan Ellis", person_alias_guid: me }, { type: "text", text: ` ${body}` }],
            blocks: null,
            metadata: null,
            visibility: "visible",
            visibility_notice: null,
            reactions: [],
            reply_count: 0,
            last_reply_at: null,
            participant_count: 0,
            repliers: null,
            thread_following: null,
            thread_unread: null,
            created_at: extra.created_at ?? at(9, 12),
            edited_at: null,
            deleted_at: null
        } as unknown as TimelineMessage
    };
}

let wrapper: VueWrapper | null = null;

function draw(props: Record<string, unknown> = {}): VueWrapper {
    wrapper = mount(MentionsPanel, {
        props: {
            rows: [row(50)],
            hasMore: false,
            isLoading: false,
            isRefreshing: false,
            isLoadingOlder: false,
            error: null,
            olderError: null,
            newIds: [],
            currentId: null,
            roomTitles,
            roomKinds,
            publicRooms: { [channel]: true, [worship]: false },
            isPhone: false,
            personAliasGuid: me,
            ...props
        },
        attachTo: document.body
    });
    return wrapper;
}

/** Text as a person reads it; some ICU builds put a narrow no-break space before AM and PM. */
function plain(text: string | undefined | null): string {
    return (text ?? "").replace(/\s+/g, " ").trim();
}

function rowsOf(w: VueWrapper): ReturnType<VueWrapper["findAll"]> {
    return w.findAll("[data-testid='mention-row']");
}

function labelOf(w: VueWrapper, index = 0): string {
    return plain(w.findAll("[data-testid='mention-row-open']")[index]?.attributes("aria-label"));
}

beforeAll(() => {
    window.ResizeObserver ??= class {
        observe(): void { /* jsdom lays nothing out. */ }
        unobserve(): void { /* As above. */ }
        disconnect(): void { /* As above. */ }
    } as unknown as typeof ResizeObserver;
    Element.prototype.scrollIntoView ??= function scrollIntoView(): void { /* jsdom scrolls nothing. */ };
});

afterEach(() => {
    wrapper?.unmount();
    wrapper = null;
    document.body.innerHTML = "";
});

// #endregion Fixtures

describe("the header", () => {
    test("titles the panel Mentions, says where they come from, and refreshes", async () => {
        const w = draw();

        expect(w.find("[data-testid='mentions-panel']").exists()).toBe(true);
        expect(w.get("[data-testid='panel-title']").text()).toBe("Mentions");
        expect(w.get("[data-testid='panel-subtitle']").text()).toBe("Where people @mentioned you");

        const refresh = w.get("[data-testid='mentions-refresh']");
        expect(refresh.attributes("aria-label")).toBe("Refresh mentions");
        await refresh.trigger("click");
        expect(w.emitted("refresh")).toHaveLength(1);
    });
});

describe("the rows", () => {
    test("are grouped under Today and Yesterday, newest first", () => {
        const w = draw({ rows: [row(60, { created_at: at(9, 12) }), row(55, { created_at: at(8, 0) }), row(40, { created_at: at(16, 30, 1) })] });

        expect(w.findAll("[data-testid='mentions-day']").map(d => plain(d.text()))).toEqual(["Today", "Yesterday"]);
        expect(rowsOf(w)).toHaveLength(3);
    });

    test("draw the author's face with its @ badge, the name, the room, the time and the text through the message body", () => {
        const w = draw();
        expect(rowsOf(w)).toHaveLength(1);
        const drawn = rowsOf(w)[0];

        expect(drawn.find("[data-testid='avatar']").exists()).toBe(true);
        expect(drawn.findAllComponents(Icon).some(i => i.props("name") === "at")).toBe(true);
        expect(plain(drawn.text())).toContain("Sam Ortiz");
        expect(plain(drawn.text())).toContain("young-adults");
        expect(plain(drawn.text())).toContain("9:12 AM");

        const body = drawn.findComponent(MessageBody);
        expect(body.exists()).toBe(true);
        expect(plain(body.text())).toContain("@Jordan Ellis are you in for Saturday?");
    });

    test("tag a reply Reply in a thread, and only a reply", () => {
        const w = draw({ rows: [row(60, { parent_id: 30 }), row(50)] });

        expect(rowsOf(w)).toHaveLength(2);
        const [reply, inChannel] = rowsOf(w);
        expect(plain(reply.get("[data-testid='mention-row-thread']").text())).toBe("Reply in a thread");
        expect(inChannel.find("[data-testid='mention-row-thread']").exists()).toBe(false);
    });

    test("mark New the rows newer than the last visit, with a dot, and the row just opened as current", () => {
        const w = draw({ rows: [row(60), row(50), row(40)], newIds: [60], currentId: 50 });

        expect(rowsOf(w)).toHaveLength(3);
        const [fresh, opened, older] = rowsOf(w);
        expect(fresh.classes()).toContain("is-new");
        expect(fresh.find("[data-testid='mention-row-new']").exists()).toBe(true);
        expect(opened.classes()).toContain("is-current");
        expect(opened.classes()).not.toContain("is-new");
        expect(opened.get("[data-testid='mention-row-open']").attributes("aria-current")).toBe("true");
        expect(older.classes()).not.toContain("is-new");
        expect(older.classes()).not.toContain("is-current");
        expect(older.find("[data-testid='mention-row-new']").exists()).toBe(false);
    });
});

describe("a row's label", () => {
    test("says the whole row, New first, the room named plainly, then Go to message", () => {
        const w = draw({ newIds: [50] });

        expect(labelOf(w)).toBe("New. Sam Ortiz in young-adults, 9:12 AM: @Jordan Ellis are you in for Saturday? Go to message");
    });

    test("says in a thread and Open thread for a reply, and yesterday for yesterday's", () => {
        const w = draw({ rows: [row(60, { parent_id: 30, created_at: at(16, 30, 1), body: "bring the slides." })] });

        expect(labelOf(w)).toBe("Sam Ortiz in young-adults, in a thread, yesterday: @Jordan Ellis bring the slides. Open thread");
    });

    test("names another room by its own title", () => {
        const w = draw({ rows: [row(70, { channel_id: worship })] });

        expect(labelOf(w)).toMatch(/^Sam Ortiz in Worship Team, /);
    });
});

describe("opening a row", () => {
    test("jumps to a channel mention in its own conversation", async () => {
        const w = draw({ rows: [row(70, { channel_id: worship })] });

        await w.get("[data-testid='mention-row-open']").trigger("click");

        expect(w.emitted("jump")).toEqual([[70, worship]]);
        expect(w.emitted("openThread")).toBeUndefined();
    });

    test("opens the thread of a reply at the reply", async () => {
        const w = draw({ rows: [row(60, { parent_id: 30 })] });

        await w.get("[data-testid='mention-row-open']").trigger("click");

        expect(w.emitted("openThread")).toEqual([[30, channel, 60]]);
        expect(w.emitted("jump")).toBeUndefined();
    });
});

describe("paging", () => {
    test("offers Load older mentions while more wait", async () => {
        const w = draw({ hasMore: true });

        const more = w.get("[data-testid='mentions-more']");
        expect(plain(more.text())).toBe("Load older mentions");
        await more.trigger("click");
        expect(w.emitted("loadOlder")).toHaveLength(1);
        expect(w.find("[data-testid='mentions-end']").exists()).toBe(false);
    });

    test("says Loading, busy, while an older page is on its way, and asks nothing more", async () => {
        const w = draw({ hasMore: true, isLoadingOlder: true });

        const more = w.get("[data-testid='mentions-more']");
        expect(plain(more.text())).toBe("Loading");
        expect(more.attributes("aria-busy")).toBe("true");
        await more.trigger("click");
        expect(w.emitted("loadOlder")).toBeUndefined();
    });

    test("ends with That's every mention", () => {
        const w = draw({ hasMore: false });

        expect(plain(w.get("[data-testid='mentions-end']").text())).toBe("That's every mention");
        expect(w.find("[data-testid='mentions-more']").exists()).toBe(false);
    });

    test("says an older page failed offline, keeping the rows and the button", () => {
        const w = draw({ hasMore: true, olderError: offline });

        expect(plain(w.get("[data-testid='mentions-older-error']").text())).toBe("Couldn't load older mentions. You're offline.");
        expect(rowsOf(w)).toHaveLength(1);
        expect(w.find("[data-testid='mentions-more']").exists()).toBe(true);
    });
});

describe("loading, empty and failed", () => {
    test("draws skeleton rows while the inbox loads", () => {
        const w = draw({ isLoading: true, rows: [] });

        expect(w.findAll("[data-testid='mention-skeleton']").length).toBeGreaterThan(0);
        expect(rowsOf(w)).toHaveLength(0);
        expect(w.find("[data-testid='mentions-empty']").exists()).toBe(false);
    });

    test("says there are no mentions yet, and how they come", () => {
        const w = draw({ rows: [] });

        const empty = w.get("[data-testid='mentions-empty']");
        expect(plain(empty.text())).toContain("No mentions yet");
        expect(plain(empty.text())).toContain("When someone @mentions you by name, it shows up here so you can jump straight to it.");
        expect(w.find("[data-testid='mentions-end']").exists()).toBe(false);
    });

    test("a refresh that failed offline says so over the rows it kept, with Try again", async () => {
        const w = draw({ error: offline });

        const strip = w.get("[data-testid='mentions-error']");
        expect(plain(strip.text())).toContain("Couldn't refresh. You're offline, so this list may be out of date.");
        expect(rowsOf(w)).toHaveLength(1);

        const again = strip.findAll("button").find(b => plain(b.text()) === "Try again");
        expect(again).toBeDefined();
        await again!.trigger("click");
        expect(w.emitted("refresh")).toHaveLength(1);
    });

    test("a refused refresh shows the server's sentence", () => {
        const w = draw({ error: { code: "rpc.unavailable", severity: "failed", text: "Chat is busy. Try again in a minute." } });

        expect(plain(w.get("[data-testid='mentions-error']").text())).toContain("Chat is busy. Try again in a minute.");
    });
});
