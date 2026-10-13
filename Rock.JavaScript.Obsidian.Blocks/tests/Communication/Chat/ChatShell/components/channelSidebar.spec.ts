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
// The sidebar, mounted: the church at its top, a search over the rooms already loaded, and the
// rooms in three sections, starred first. A row says what needs the person's attention without
// shouting: a direct message waiting to be read carries a dot, a channel only goes bold unless it
// names them, a muted room's mention badge is the quiet one, and a screen reader hears the same
// facts as a suffix on the row's name. A two-person conversation is named by the other person,
// since the wire's count of others includes the people it already names.
import { mount, VueWrapper } from "@vue/test-utils";
import { nextTick } from "vue";
import ChannelSidebar from "../../../../../src/Communication/Chat/ChatShell/components/channelSidebar.partial.obs";
import { SidebarRow } from "../../../../../src/Communication/Chat/ChatShell/types.partial";

// #region Fixtures

/** A sidebar row in the exact shape chat_get_bootstrap answers, read and quiet by default. */
function row(overrides: Partial<SidebarRow> & { channel_id: string }): SidebarRow {
    return {
        name: null,
        icon_url: null,
        channel_type: "shared",
        is_public: false,
        always_shown: false,
        is_member: true,
        is_favorite: false,
        notify_mode: "all",
        is_leader: false,
        can_post_announcements: false,
        can_mention_all: false,
        last_message_id: 10,
        last_message_at: "2026-10-12T09:02:00Z",
        last_message_type: "text",
        last_sender_nick_name: "Marcus",
        last_sender_last_name: "Reed",
        last_sender_listed: true,
        last_message_preview: "Are we doing the extended bridge again?",
        read_cursor: 10,
        is_unread: false,
        mention_count: 0,
        dm_names: null,
        dm_other_count: null,
        ...overrides
    };
}

const team = row({ channel_id: "c0000001-0000-4000-8000-000000000000", name: "Worship Team" });
const youth = row({ channel_id: "c0000002-0000-4000-8000-000000000000", name: "Youth Leaders", is_unread: true, mention_count: 2 });
const starred = row({ channel_id: "c0000003-0000-4000-8000-000000000000", name: "Prayer", is_favorite: true });
const muted = row({ channel_id: "c0000004-0000-4000-8000-000000000000", name: "Announcements", notify_mode: "silent", is_unread: true, mention_count: 2 });

// A two-person direct message: the wire names the other person in full and counts them in
// dm_other_count; a group's names are first names.
const dmAnna = row({
    channel_id: "c0000005-0000-4000-8000-000000000000",
    channel_type: "dm",
    dm_names: ["Anna Kim"],
    dm_other_count: 1,
    last_sender_nick_name: "Anna",
    last_sender_last_name: "Kim",
    last_message_preview: "Do you have a minute after rehearsal?",
    is_unread: true
});

const dmGroup = row({
    channel_id: "c0000006-0000-4000-8000-000000000000",
    channel_type: "dm",
    dm_names: ["Anna", "Marcus"],
    dm_other_count: 2
});

// Six others, of whom the wire names the first four.
const dmBig = row({
    channel_id: "c0000007-0000-4000-8000-000000000000",
    channel_type: "dm",
    dm_names: ["Anna", "Ben", "Cara", "Dee"],
    dm_other_count: 6
});

const dmRead = row({
    channel_id: "c0000008-0000-4000-8000-000000000000",
    channel_type: "dm",
    dm_names: ["Priya Shah"],
    dm_other_count: 1,
    is_unread: false
});

const allRows = [team, youth, starred, muted, dmAnna, dmGroup, dmBig, dmRead];

// #endregion Fixtures

// #region Helpers

let wrapper: VueWrapper | null = null;

function mountSidebar(props: Record<string, unknown> = {}): VueWrapper {
    wrapper = mount(ChannelSidebar, {
        props: {
            rows: allRows,
            activeChannelId: null,
            isNewMessageShown: true,
            organizationName: "Sun Valley Church",
            layout: "conversational",
            ...props
        },
        attachTo: document.body
    });
    return wrapper;
}

function rowOf(w: VueWrapper, channelId: string): ReturnType<VueWrapper["get"]> {
    return w.get(`[data-testid='channel-row'][data-channel-id='${channelId}']`);
}

function rowIdsIn(w: VueWrapper, section: string): string[] {
    return w.get(`[data-testid='sidebar-section'][data-section='${section}']`)
        .findAll("[data-testid='channel-row']")
        .map(r => r.attributes("data-channel-id") ?? "");
}

function visibleRowIds(w: VueWrapper): string[] {
    return w.findAll("[data-testid='channel-row']").map(r => r.attributes("data-channel-id") ?? "");
}

// #endregion Helpers

afterEach(() => {
    wrapper?.unmount();
    wrapper = null;
    document.body.innerHTML = "";
});

describe("the sidebar's top", () => {
    test("shows the church's name and its initial in the mark", () => {
        const w = mountSidebar();

        expect(w.get("[data-testid='sidebar-org-name']").text()).toBe("Sun Valley Church");
        expect(w.get("[data-testid='sidebar-org-mark']").text()).toBe("S");
    });

    test("offers New message as a labelled icon button only with DM Access", async () => {
        const w = mountSidebar();
        const button = w.get("[data-testid='new-message']");

        expect(button.attributes("aria-label")).toBe("New message");
        await button.trigger("click");
        expect(w.emitted("newMessage")).toHaveLength(1);

        await w.setProps({ isNewMessageShown: false });
        expect(w.find("[data-testid='new-message']").exists()).toBe(false);
    });
});

describe("the sections", () => {
    test("are Starred, Channels and Direct messages, in that order", () => {
        const w = mountSidebar();

        const sections = w.findAll("[data-testid='sidebar-section']");
        expect(sections.map(s => s.attributes("data-section"))).toEqual(["starred", "channels", "dms"]);
        expect(sections.map(s => s.get("[data-testid='section-heading']").text())).toEqual(["Starred", "Channels", "Direct messages"]);
    });

    test("place a starred room under Starred only, and the rest by kind", () => {
        const w = mountSidebar();

        expect(rowIdsIn(w, "starred")).toEqual([starred.channel_id]);
        expect(rowIdsIn(w, "channels")).toEqual([team.channel_id, youth.channel_id, muted.channel_id]);
        expect(rowIdsIn(w, "dms")).toEqual([dmAnna.channel_id, dmGroup.channel_id, dmBig.channel_id, dmRead.channel_id]);
    });

    test("leave out Starred when nothing is starred", () => {
        const w = mountSidebar({ rows: [team, dmAnna] });

        expect(w.find("[data-testid='sidebar-section'][data-section='starred']").exists()).toBe(false);
    });

    test("give Direct messages its own New message plus, with DM Access", async () => {
        const w = mountSidebar();
        const plus = w.get("[data-testid='sidebar-section'][data-section='dms'] [data-testid='section-new-message']");

        expect(plus.attributes("aria-label")).toBe("New message");
        await plus.trigger("click");
        expect(w.emitted("newMessage")).toHaveLength(1);

        await w.setProps({ isNewMessageShown: false });
        expect(w.find("[data-testid='section-new-message']").exists()).toBe(false);
    });
});

describe("Find a conversation", () => {
    test("filters the loaded rows by name, in any letter case", async () => {
        const w = mountSidebar();
        const search = w.get("[data-testid='sidebar-search']");

        expect(search.attributes("placeholder")).toBe("Find a conversation");

        await search.setValue("wor");
        expect(visibleRowIds(w)).toEqual([team.channel_id]);

        // A direct message is found by the name it is drawn under.
        await search.setValue("ANNA");
        expect(visibleRowIds(w)).toEqual([dmAnna.channel_id, dmGroup.channel_id, dmBig.channel_id]);

        await search.setValue("");
        expect(visibleRowIds(w)).toHaveLength(allRows.length);
    });
});

describe("a direct message's name", () => {
    test("is the other person's full name in a two-person conversation, never 'and 1 more'", () => {
        const w = mountSidebar();

        expect(rowOf(w, dmAnna.channel_id).get("[data-testid='row-name']").text()).toBe("Anna Kim");
    });

    test("joins a group's members as 'A and B'", () => {
        const w = mountSidebar();

        expect(rowOf(w, dmGroup.channel_id).get("[data-testid='row-name']").text()).toBe("Anna and Marcus");
    });

    test("counts only the members the wire did not name", () => {
        const w = mountSidebar();

        expect(rowOf(w, dmBig.channel_id).get("[data-testid='row-name']").text()).toBe("Anna, Ben, Cara, Dee and 2 more");
    });

    test("is the group's own name when it has one", () => {
        const w = mountSidebar({ rows: [{ ...dmGroup, name: "Setlist crew" }] });

        expect(rowOf(w, dmGroup.channel_id).get("[data-testid='row-name']").text()).toBe("Setlist crew");
    });
});

describe("a row's states", () => {
    test("draws the dot only on an unread direct message, never on an unread channel", () => {
        const w = mountSidebar();

        expect(rowOf(w, dmAnna.channel_id).find("[data-testid='row-dot']").exists()).toBe(true);
        expect(rowOf(w, dmRead.channel_id).find("[data-testid='row-dot']").exists()).toBe(false);
        expect(rowOf(w, youth.channel_id).find("[data-testid='row-dot']").exists()).toBe(false);
        expect(rowOf(w, muted.channel_id).find("[data-testid='row-dot']").exists()).toBe(false);
    });

    test("draws the dot on an unread direct message in Community too", () => {
        const w = mountSidebar({ layout: "community" });

        expect(rowOf(w, dmAnna.channel_id).find("[data-testid='row-dot']").exists()).toBe(true);
        expect(rowOf(w, youth.channel_id).find("[data-testid='row-dot']").exists()).toBe(false);
    });

    test("marks an unread room, which the style draws bold", () => {
        const w = mountSidebar();

        expect(rowOf(w, youth.channel_id).attributes("data-unread")).toBe("true");
        expect(rowOf(w, team.channel_id).attributes("data-unread")).toBe("false");
    });

    test("shows a channel's mention count in the badge, the quiet one when the room is muted", () => {
        const w = mountSidebar();

        const loud = rowOf(w, youth.channel_id).get("[data-testid='row-mention-badge']");
        expect(loud.text()).toContain("2");
        expect(loud.classes()).toContain("chat-badge");
        expect(loud.classes()).not.toContain("is-quiet");
        expect(loud.attributes("aria-hidden")).toBe("true");

        const quiet = rowOf(w, muted.channel_id).get("[data-testid='row-mention-badge']");
        expect(quiet.classes()).toContain("is-quiet");

        expect(rowOf(w, team.channel_id).find("[data-testid='row-mention-badge']").exists()).toBe(false);
    });

    test("reads 99+ above 99 mentions", () => {
        const w = mountSidebar({ rows: [{ ...youth, mention_count: 120 }] });

        expect(rowOf(w, youth.channel_id).get("[data-testid='row-mention-badge']").text()).toContain("99+");
    });

    test("marks a muted room and draws its bell-off", () => {
        const w = mountSidebar();

        expect(rowOf(w, muted.channel_id).attributes("data-muted")).toBe("true");
        expect(rowOf(w, muted.channel_id).find("[data-testid='row-muted-icon']").exists()).toBe(true);
        expect(rowOf(w, team.channel_id).attributes("data-muted")).toBe("false");
    });

    test("marks a room holding a mention, so its time is drawn in the brand ink", () => {
        const w = mountSidebar();

        expect(rowOf(w, youth.channel_id).attributes("data-mentioned")).toBe("true");
        expect(rowOf(w, team.channel_id).attributes("data-mentioned")).toBe("false");
    });

    test("marks the open room as current and selects a room on a click", async () => {
        const w = mountSidebar({ activeChannelId: team.channel_id });

        expect(rowOf(w, team.channel_id).attributes("aria-current")).toBeTruthy();
        expect(rowOf(w, youth.channel_id).attributes("aria-current")).toBeUndefined();

        await rowOf(w, youth.channel_id).trigger("click");
        expect(w.emitted("select")).toEqual([[youth.channel_id]]);
    });

    test("flashes a row that arrives or moves, and not the rows already in place", async () => {
        const w = mountSidebar({ rows: [team, youth, dmAnna] });
        expect(w.findAll(".is-flash")).toHaveLength(0);

        // Youth moves above Team, as a new message moves a room up, and Prayer arrives.
        await w.setProps({ rows: [youth, team, dmAnna, starred] });
        await nextTick();

        expect(rowOf(w, youth.channel_id).classes()).toContain("is-flash");
        expect(rowOf(w, starred.channel_id).classes()).toContain("is-flash");
        expect(rowOf(w, dmAnna.channel_id).classes()).not.toContain("is-flash");
    });
});

describe("a row's look", () => {
    test("Conversational draws a preview, prefixed by who wrote it in a channel, and the time", () => {
        const w = mountSidebar();

        expect(rowOf(w, team.channel_id).get("[data-testid='row-preview']").text()).toBe("Marcus: Are we doing the extended bridge again?");
        expect(rowOf(w, team.channel_id).find("[data-testid='row-time']").exists()).toBe(true);
    });

    test("Conversational reads 'No messages yet' for an empty room", () => {
        const empty = { ...team, last_message_id: null, last_message_at: null, last_message_preview: null, last_sender_nick_name: null, last_sender_last_name: null };
        const w = mountSidebar({ rows: [empty] });

        expect(rowOf(w, team.channel_id).get("[data-testid='row-preview']").text()).toBe("No messages yet");
    });

    test("Community draws the name alone, with no preview and no time", () => {
        const w = mountSidebar({ layout: "community" });
        const r = rowOf(w, team.channel_id);

        expect(r.attributes("data-layout")).toBe("community");
        expect(r.get("[data-testid='row-name']").text()).toBe("Worship Team");
        expect(r.find("[data-testid='row-preview']").exists()).toBe(false);
        expect(r.find("[data-testid='row-time']").exists()).toBe(false);
    });
});

describe("a row's accessible name", () => {
    test("ends with a hidden suffix of mentions, unread and muted", () => {
        const w = mountSidebar();

        expect(rowOf(w, muted.channel_id).get("[data-testid='row-label-suffix']").text()).toBe(", 2 mentions, unread, muted");
        expect(rowOf(w, muted.channel_id).get("[data-testid='row-label-suffix']").classes()).toContain("chat-sr-only");
    });

    test("says one mention in the singular, and unread for a direct message", () => {
        const w = mountSidebar({ rows: [{ ...youth, mention_count: 1 }, dmAnna] });

        expect(rowOf(w, youth.channel_id).get("[data-testid='row-label-suffix']").text()).toBe(", 1 mention, unread");
        expect(rowOf(w, dmAnna.channel_id).get("[data-testid='row-label-suffix']").text()).toBe(", unread");
    });

    test("says 'not joined' for a room the person is not in", () => {
        const w = mountSidebar({ rows: [{ ...team, is_member: false, always_shown: true }] });

        expect(rowOf(w, team.channel_id).get("[data-testid='row-label-suffix']").text()).toBe(", not joined");
    });

    test("hides the dot from a screen reader, which hears the suffix instead", () => {
        const w = mountSidebar();

        expect(rowOf(w, dmAnna.channel_id).get("[data-testid='row-dot']").attributes("aria-hidden")).toBe("true");
    });
});

describe("the empty sidebar", () => {
    test("says there are no conversations yet and offers New message", async () => {
        const w = mountSidebar({ rows: [] });
        const empty = w.get("[data-testid='sidebar-empty']");

        expect(empty.text()).toContain("No conversations yet");
        expect(empty.text()).toContain("Message someone from your groups, or browse channels to join.");

        await empty.get("[data-testid='empty-new-message']").trigger("click");
        expect(w.emitted("newMessage")).toHaveLength(1);
    });

    test("offers no New message without DM Access", () => {
        const w = mountSidebar({ rows: [], isNewMessageShown: false });

        expect(w.find("[data-testid='empty-new-message']").exists()).toBe(false);
    });

    test("draws no sections while empty", () => {
        const w = mountSidebar({ rows: [] });

        expect(w.findAll("[data-testid='sidebar-section']")).toHaveLength(0);
    });
});
