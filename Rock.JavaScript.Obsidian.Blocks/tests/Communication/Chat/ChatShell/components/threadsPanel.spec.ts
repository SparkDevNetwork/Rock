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
// The threads panel: the open conversation's threads, or every thread the person follows, in two
// tabs. Each row is one button with a label that says the whole row; the @ badge counts mentions
// of the reader and shows only when there are any; a row just unfollowed on the All tab stays,
// dimmed, until the list is fetched again. Both tabs have an empty state, a failed refresh and a
// failed follow each say so in place, and the list is one tab stop that the arrow keys move
// through, Right reaching a row's follow toggle.
//
// The component these tests are written to, threadsPanel.partial.obs, in the shared panel host:
//
//   props: tab: "channel" | "all", rows: ThreadListRow[] (chat_list_threads' rows), isLoading: boolean,
//          isRefreshing: boolean, error: ChatError | null (the list's), rowError: { rootId: number,
//          error: ChatError } | null, busyRootId: number | null, channelId: string (the open
//          conversation), kind: "channel" | "person" | "group" (the open conversation's),
//          roomTitles: Record<string, string> (each conversation's title by channel id, as the
//          header shows it), isPhone: boolean, personAliasGuid: string
//   emits: open(rootId: number, channelId: string), tab(tab: "channel" | "all"), refresh,
//          follow(rootId: number, isFollowing: boolean), loadMore, close
//   marks: thread-row (each row, class is-unread and is-off), thread-row-open (the row's button,
//          labelled), thread-row-follow (aria-pressed, aria-busy while busy), thread-row-mentions,
//          thread-row-status ("Following" or "Not following"), thread-row-error, thread-skeleton,
//          threads-empty, threads-empty-title, threads-empty-body, threads-error, threads-refresh;
//          the tabs are role="tab" with aria-selected. The panel host's marks are threadDrawer.spec's.
//
// A refusal shows the server's sentence; a call that never reached the platform has none, and
// shows the design's offline sentence.
import { readFileSync } from "fs";
import { join } from "path";
import { mount, VueWrapper } from "@vue/test-utils";
import { nextTick } from "vue";
import ThreadsPanel from "../../../../../src/Communication/Chat/ChatShell/components/threadsPanel.partial.obs";
import { ChatError } from "../../../../../src/Communication/Chat/ChatShell/types.partial";

const me = "a0000001-0000-4000-8000-000000000000";
const anna = "a0000002-0000-4000-8000-000000000000";
const ben = "a0000003-0000-4000-8000-000000000000";
const channel = "c0000001-0000-4000-8000-000000000000";
const worship = "c0000002-0000-4000-8000-000000000000";

const roomTitles: Record<string, string> = { [channel]: "young-adults", [worship]: "Worship Team" };

/** 9:41 AM today in local time. */
const todayAtNineFortyOne = ((): string => {
    const at = new Date();
    at.setHours(9, 41, 0, 0);
    return at.toISOString();
})();

const offline: ChatError = { code: "rpc.transport", severity: "failed" };

/** One row of chat_list_threads, exactly its keys, its root as chat_messages_json answers it. */
function row(rootId: number, extra: Record<string, unknown> = {}, rootExtra: Record<string, unknown> = {}): Record<string, unknown> {
    return {
        channel_id: channel,
        root: {
            id: rootId,
            parent_id: null,
            person_alias_guid: anna,
            sender_nick_name: "Anna",
            sender_last_name: "Reyes",
            sender_avatar_url: null,
            sender_listed: true,
            message_type: "text",
            body: "Who's in for Saturday's hike?",
            pieces: null,
            blocks: null,
            created_at: todayAtNineFortyOne,
            edited_at: null,
            deleted_at: null,
            visibility: "visible",
            visibility_notice: null,
            reply_count: 3,
            last_reply_at: todayAtNineFortyOne,
            participant_count: 2,
            repliers: [{ person_alias_guid: ben, nick_name: "Ben", last_name: "Okafor", avatar_url: null }],
            thread_following: true,
            thread_unread: false,
            ...rootExtra
        },
        reply_count: 3,
        last_reply_at: todayAtNineFortyOne,
        last_reply_id: rootId + 3,
        participant_count: 2,
        following: true,
        has_unread: false,
        mention_count: 0,
        ...extra
    };
}

let wrapper: VueWrapper | null = null;

function draw(props: Record<string, unknown> = {}): VueWrapper {
    wrapper = mount(ThreadsPanel, {
        props: {
            tab: "channel",
            rows: [row(40)],
            isLoading: false,
            isRefreshing: false,
            error: null,
            rowError: null,
            busyRootId: null,
            channelId: channel,
            kind: "channel",
            roomTitles,
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
    return w.findAll("[data-testid='thread-row']");
}

function openButtons(w: VueWrapper): HTMLElement[] {
    return w.findAll("[data-testid='thread-row-open']").map(b => b.element as HTMLElement);
}

async function press(element: HTMLElement, key: string): Promise<void> {
    element.dispatchEvent(new KeyboardEvent("keydown", { key, bubbles: true }));
    await nextTick();
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

describe("the header", () => {
    test("titles the panel Threads and names the open conversation on its own tab", () => {
        const w = draw();

        expect(w.get("[data-testid='panel-title']").text()).toBe("Threads");
        expect(w.get("[data-testid='panel-subtitle']").text()).toBe("young-adults");
    });

    test("says Threads you follow on the All tab", () => {
        const w = draw({ tab: "all" });

        expect(w.get("[data-testid='panel-subtitle']").text()).toBe("Threads you follow");
    });

    test("refreshes from its button, which says it is busy while it is", async () => {
        const w = draw();

        const refresh = w.get("[data-testid='threads-refresh']");
        expect(refresh.attributes("aria-label")).toBe("Refresh threads");
        await refresh.trigger("click");
        expect(w.emitted("refresh")).toHaveLength(1);

        await w.setProps({ isRefreshing: true });
        expect(w.get("[data-testid='threads-refresh']").attributes("aria-busy")).toBe("true");
    });
});

describe("the tabs", () => {
    test.each([
        ["channel", "This channel"],
        ["person", "This conversation"],
        ["group", "This conversation"]
    ])("name a %s's tab %s, beside All", (kind, words) => {
        const w = draw({ kind });

        expect(w.findAll("[role='tab']").map(t => t.text())).toEqual([words, "All"]);
    });

    test("mark the tab shown and ask for the other", async () => {
        const w = draw();
        const [here, all] = w.findAll("[role='tab']");

        expect(here.attributes("aria-selected")).toBe("true");
        expect(all.attributes("aria-selected")).toBe("false");

        await all.trigger("click");
        expect(w.emitted("tab")).toEqual([["all"]]);
    });
});

describe("a row", () => {
    test("is one button saying the whole row, as the design writes it", () => {
        const w = draw({ rows: [row(40, { has_unread: true, mention_count: 1 })] });

        expect(plain(w.get("[data-testid='thread-row-open']").attributes("aria-label"))).toBe(
            "New replies. Anna Reyes in young-adults: Who's in for Saturday's hike? 3 replies, last reply 9:41 AM, 1 mention of you, following. Open thread");
    });

    test("on the All tab names its own room, and a row just unfollowed says so", () => {
        const w = draw({
            tab: "all",
            rows: [row(70, { channel_id: worship, following: false, reply_count: 2, mention_count: 2 }, { person_alias_guid: ben, sender_nick_name: "Ben", sender_last_name: "Okafor", body: "Bring snacks" })]
        });

        expect(plain(w.get("[data-testid='thread-row-open']").attributes("aria-label"))).toBe(
            "Ben Okafor in Worship Team: Bring snacks. 2 replies, last reply 9:41 AM, 2 mentions of you, not following. Open thread");
    });

    test("names the reader's own root You", () => {
        const w = draw({ rows: [row(40, {}, { person_alias_guid: me, sender_nick_name: "Jordan", sender_last_name: "Ellis" })] });

        expect(plain(w.get("[data-testid='thread-row-open']").attributes("aria-label"))).toMatch(/^You in young-adults: /);
    });

    test("opens its thread in its own conversation", async () => {
        const w = draw({ tab: "all", rows: [row(70, { channel_id: worship })] });

        await w.get("[data-testid='thread-row-open']").trigger("click");

        expect(w.emitted("open")).toEqual([[70, worship]]);
    });

    test("is bold when it has new replies, and only then", () => {
        const w = draw({ rows: [row(40, { has_unread: true }), row(30)] });

        expect(rowsOf(w).map(r => r.classes().includes("is-unread"))).toEqual([true, false]);
    });

    test("shows the @ badge with the count only when the reader is mentioned", () => {
        const w = draw({ rows: [row(40, { mention_count: 2 }), row(30, { mention_count: 0 })] });

        const [mentioned, quiet] = rowsOf(w);
        expect(mentioned.get("[data-testid='thread-row-mentions']").text()).toBe("2");
        expect(quiet.find("[data-testid='thread-row-mentions']").exists()).toBe(false);
    });

    test("says Following on the conversation's tab", () => {
        const w = draw();

        expect(w.get("[data-testid='thread-row-status']").text()).toContain("Following");
    });
});

describe("following from a row", () => {
    test("the toggle asks to change the row's follow", async () => {
        const w = draw();

        const toggle = w.get("[data-testid='thread-row-follow']");
        expect(toggle.attributes("aria-pressed")).toBe("true");
        expect(toggle.attributes("aria-label")).toBe("Unfollow thread by Anna Reyes");

        await toggle.trigger("click");
        expect(w.emitted("follow")).toEqual([[40, false]]);
    });

    test("the toggle is busy while its change is out", () => {
        const w = draw({ busyRootId: 40 });

        expect(w.get("[data-testid='thread-row-follow']").attributes("aria-busy")).toBe("true");
    });

    test("a row unfollowed on the All tab stays, dimmed, saying Not following", () => {
        const w = draw({ tab: "all", rows: [row(40, { following: false }), row(30)] });

        const [unfollowed, followed] = rowsOf(w);
        expect(unfollowed.classes()).toContain("is-off");
        expect(unfollowed.get("[data-testid='thread-row-status']").text()).toBe("Not following");
        expect(followed.classes()).not.toContain("is-off");
    });

    test("a change that failed offline says so on its row", () => {
        const w = draw({ rows: [row(40), row(30)], rowError: { rootId: 40, error: offline } });

        const [failed, other] = rowsOf(w);
        expect(plain(failed.get("[data-testid='thread-row-error']").text())).toBe("Couldn't change this. You're offline, so try again in a moment.");
        expect(other.find("[data-testid='thread-row-error']").exists()).toBe(false);
    });

    test("a refused change shows the server's sentence on its row", () => {
        const w = draw({ rowError: { rootId: 40, error: { code: "authz.not_member", severity: "permission", text: "You're no longer in this channel." } } });

        expect(plain(w.get("[data-testid='thread-row-error']").text())).toBe("You're no longer in this channel.");
    });
});

describe("loading, empty and failed", () => {
    test("draws three skeleton rows while the list loads", () => {
        const w = draw({ isLoading: true, rows: [] });

        expect(w.findAll("[data-testid='thread-skeleton']")).toHaveLength(3);
        expect(rowsOf(w)).toHaveLength(0);
    });

    test.each([
        ["channel", "young-adults", "No threads in young-adults yet"],
        ["person", "Marcus Bell", "No threads with Marcus Bell yet"],
        ["group", "Anna, Ben and Marcus", "No threads in this conversation yet"]
    ])("an empty %s tab says so and offers every followed thread", async (kind, conversation, title) => {
        const w = draw({ kind, rows: [], roomTitles: { [channel]: conversation } });

        expect(plain(w.get("[data-testid='threads-empty-title']").text())).toBe(title);
        expect(plain(w.get("[data-testid='threads-empty-body']").text())).toBe(
            "Reply to any message to start one. Threads you follow from every conversation are under All.");

        const seeAll = w.findAll("[data-testid='threads-empty'] button").find(b => plain(b.text()) === "See all followed threads");
        expect(seeAll).toBeDefined();
        await seeAll!.trigger("click");
        expect(w.emitted("tab")).toEqual([["all"]]);
    });

    test("an empty All tab says how a thread comes to be followed", () => {
        const w = draw({ tab: "all", rows: [] });

        expect(plain(w.get("[data-testid='threads-empty-title']").text())).toBe("You're not following any threads");
        expect(plain(w.get("[data-testid='threads-empty-body']").text())).toBe(
            "You follow a thread when you start it, reply in it or are mentioned in it. New replies to those threads collect here.");
    });

    test("a refresh that failed offline says so over the rows it kept, with Try again", async () => {
        const w = draw({ error: offline });

        const strip = w.get("[data-testid='threads-error']");
        expect(plain(strip.text())).toContain("Couldn't refresh. You're offline, so this list may be out of date.");
        expect(rowsOf(w)).toHaveLength(1);

        const again = strip.findAll("button").find(b => plain(b.text()) === "Try again");
        expect(again).toBeDefined();
        await again!.trigger("click");
        expect(w.emitted("refresh")).toHaveLength(1);
    });

    test("a refused refresh shows the server's sentence", () => {
        const w = draw({ error: { code: "rpc.unavailable", severity: "failed", text: "Chat is busy. Try again in a minute." } });

        expect(plain(w.get("[data-testid='threads-error']").text())).toContain("Chat is busy. Try again in a minute.");
    });
});

describe("the keyboard", () => {
    test("the list is one tab stop, the first row until another is focused", async () => {
        const w = draw({ rows: [row(40), row(30), row(20)] });

        expect(openButtons(w).map(b => b.tabIndex)).toEqual([0, -1, -1]);

        openButtons(w)[1].focus();
        await nextTick();
        expect(openButtons(w).map(b => b.tabIndex)).toEqual([-1, 0, -1]);
    });

    test("Up and Down move between rows, Home and End to the ends", async () => {
        const w = draw({ rows: [row(40), row(30), row(20)] });
        const [first, second, third] = openButtons(w);
        first.focus();

        await press(first, "ArrowDown");
        expect(document.activeElement).toBe(second);

        await press(second, "End");
        expect(document.activeElement).toBe(third);

        await press(third, "ArrowDown");
        expect(document.activeElement).toBe(third);

        await press(third, "Home");
        expect(document.activeElement).toBe(first);

        await press(first, "ArrowUp");
        expect(document.activeElement).toBe(first);
    });

    test("Right reaches the row's follow toggle and Left returns to the row", async () => {
        const w = draw({ rows: [row(40), row(30)] });
        const [first] = openButtons(w);
        const toggle = rowsOf(w)[0].get("[data-testid='thread-row-follow']").element as HTMLElement;
        first.focus();

        await press(first, "ArrowRight");
        expect(document.activeElement).toBe(toggle);

        await press(toggle, "ArrowLeft");
        expect(document.activeElement).toBe(first);
    });

    test("the follow toggle is outside the tab order, reached by Right alone", () => {
        const w = draw();

        expect((w.get("[data-testid='thread-row-follow']").element as HTMLElement).tabIndex).toBe(-1);
    });
});

describe("a row's text", () => {
    // A mention in a row's text is quiet, an inline pill whose padding takes no room in the line,
    // so two lines of a root holding a chip are as tall as two without. In the conversation a
    // chip is an inline block that may grow its line; the list must not.

    /** The panel's own style rules, selector and body, read from the component's source. */
    function panelRules(): Array<{ selector: string, body: string }> {
        const source = readFileSync(join(__dirname, "../../../../../src/Communication/Chat/ChatShell/components/threadsPanel.partial.obs"), "utf8");
        const style = source.slice(source.indexOf("<style"), source.indexOf("</style>")).replace(/\/\*[\s\S]*?\*\//g, "");
        return Array.from(style.matchAll(/([^{}]+)\{([^{}]*)\}/g)).map(m => ({ selector: m[1].trim(), body: m[2] }));
    }

    test("draws a mention chip inline, so a chip never makes its line taller", () => {
        const chip = panelRules().find(r => r.selector === ".chat-ls-text :deep(.chat-mn)");

        expect(chip?.body).toMatch(/display:\s*inline;/);
        expect(chip?.body).toMatch(/box-decoration-break:\s*clone;/);
    });
});
