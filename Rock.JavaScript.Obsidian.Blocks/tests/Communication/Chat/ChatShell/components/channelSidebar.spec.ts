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
// rooms in three sections, starred first. A channel is one dense line and a direct message carries
// its face, preview and time, whichever section it sits in. A row says what needs the person's attention without
// shouting: a direct message waiting to be read carries a dot, a channel only goes bold unless it
// names them, a muted room's mention badge is the quiet one, and a screen reader hears the same
// facts as a suffix on the row's name. A room's kind, its title and the line under it are the
// platform's, drawn as sent: the platform's own tests hold how it works each of them out.
import { mount, VueWrapper } from "@vue/test-utils";
import { nextTick } from "vue";
import ChannelSidebar from "../../../../../src/Communication/Chat/ChatShell/components/channelSidebar.partial.obs";
import Avatar from "../../../../../src/Communication/Chat/ChatShell/ui/avatar.partial.obs";
import { avatarHue } from "../../../../../src/Communication/Chat/ChatShell/ui/brand.partial";
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
        last_sender_avatar_url: null,
        last_sender_listed: true,
        last_message_preview: "Are we doing the extended bridge again?",
        last_message_is_own: false,
        last_message_posted_by_rock: false,
        read_cursor: 10,
        is_unread: false,
        mention_count: 0,
        all_mention_count: 0,
        can_mention: true,
        dm_people: null,
        dm_other_count: null,
        room_kind: "channel",
        title: overrides.name ?? "",
        preview_line: "",
        ...overrides
    };
}

const team = row({ channel_id: "c0000001-0000-4000-8000-000000000000", name: "Worship Team" });
const youth = row({ channel_id: "c0000002-0000-4000-8000-000000000000", name: "Youth Leaders", is_unread: true, mention_count: 2 });
const starred = row({ channel_id: "c0000003-0000-4000-8000-000000000000", name: "Prayer", is_favorite: true });
const muted = row({ channel_id: "c0000004-0000-4000-8000-000000000000", name: "Announcements", notify_mode: "silent", is_unread: true, mention_count: 2 });

// A two-person direct message: the wire names the other person in full and counts them in
// dm_other_count; a group's names are first names. Each carries the kind, title and line the
// platform answers for it.
const dmAnna = row({
    channel_id: "c0000005-0000-4000-8000-000000000000",
    channel_type: "dm",
    dm_people: [{ person_alias_guid: "anna-kim", nick_name: "Anna", last_name: "Kim", avatar_url: null }],
    dm_other_count: 1,
    room_kind: "person",
    title: "Anna Kim",
    preview_line: "Do you have a minute after rehearsal?",
    last_sender_nick_name: "Anna",
    last_sender_last_name: "Kim",
    last_sender_avatar_url: null,
    last_message_preview: "Do you have a minute after rehearsal?",
    is_unread: true
});

const dmGroup = row({
    channel_id: "c0000006-0000-4000-8000-000000000000",
    channel_type: "dm",
    dm_people: [{ person_alias_guid: "anna", nick_name: "Anna", last_name: null, avatar_url: null }, { person_alias_guid: "marcus", nick_name: "Marcus", last_name: null, avatar_url: null }],
    dm_other_count: 2,
    room_kind: "group",
    title: "Anna and Marcus",
    preview_line: "Marcus: Are we doing the extended bridge again?"
});

// Six others, of whom the wire names the first four.
const dmBig = row({
    channel_id: "c0000007-0000-4000-8000-000000000000",
    channel_type: "dm",
    dm_people: [{ person_alias_guid: "anna", nick_name: "Anna", last_name: null, avatar_url: null }, { person_alias_guid: "ben", nick_name: "Ben", last_name: null, avatar_url: null }, { person_alias_guid: "cara", nick_name: "Cara", last_name: null, avatar_url: null }, { person_alias_guid: "dee", nick_name: "Dee", last_name: null, avatar_url: null }],
    dm_other_count: 6,
    room_kind: "group",
    title: "Anna, Ben, Cara, Dee and 2 more",
    preview_line: "Marcus: Are we doing the extended bridge again?"
});

const dmRead = row({
    channel_id: "c0000008-0000-4000-8000-000000000000",
    channel_type: "dm",
    dm_people: [{ person_alias_guid: "priya-shah", nick_name: "Priya", last_name: "Shah", avatar_url: null }],
    dm_other_count: 1,
    room_kind: "person",
    title: "Priya Shah",
    preview_line: "Are we doing the extended bridge again?",
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
    // The page above the block already names the church and Rock's rail shows its logo, so the head
    // is one row for finding a conversation and starting one.
    test("draws no church name or mark: the head is the search beside the compose button", () => {
        const w = mountSidebar();
        const top = w.get("[data-testid='sidebar-top']");

        expect(w.find("[data-testid='sidebar-org-name']").exists()).toBe(false);
        expect(w.find("[data-testid='sidebar-org-mark']").exists()).toBe(false);
        expect(top.find("[data-testid='sidebar-search']").exists()).toBe(true);
        expect(top.find("[data-testid='new-message']").exists()).toBe(true);
        expect(w.findAll(".chat-side-search")).toHaveLength(1);
        expect(top.get(".chat-side-search").classes()).toContain("chat-field");
    });

    test("is not given the church's name at all", () => {
        expect((ChannelSidebar as unknown as { props: Record<string, unknown> }).props).not.toHaveProperty("organizationName");
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

    // New message is a full view over the conversation, as Browse channels is, so the row of the
    // conversation under it is not the current one while it is open.
    test("takes the current mark from the conversation under it while New message is open, and gives it back", async () => {
        const w = mountSidebar({ activeChannelId: team.channel_id, isNewMessageOpen: true });

        expect(rowOf(w, team.channel_id).attributes("aria-current")).toBeUndefined();
        expect(w.get("[data-testid='new-message']").attributes("aria-pressed")).toBe("true");

        await w.setProps({ isNewMessageOpen: false });
        expect(rowOf(w, team.channel_id).attributes("aria-current")).toBe("true");
        expect(w.get("[data-testid='new-message']").attributes("aria-pressed")).toBe("false");
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

        expect(search.attributes("placeholder")).toBe("Search");
        expect(search.attributes("aria-label")).toBe("Find a conversation");

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
    test("is the title the platform sent, which names people the faces do not", () => {
        // A person on the Chat Ban List is in the room's title count but not among its names.
        const w = mountSidebar({ rows: [{ ...dmGroup, title: "Anna, Marcus and 1 more" }] });

        expect(rowOf(w, dmGroup.channel_id).get("[data-testid='row-name']").text()).toBe("Anna, Marcus and 1 more");
    });

    test("is the title the platform sent for a named group, not its name field", () => {
        const w = mountSidebar({ rows: [{ ...dmGroup, name: "Chat Direct Message", title: "Anna and Marcus" }] });

        expect(rowOf(w, dmGroup.channel_id).get("[data-testid='row-name']").text()).toBe("Anna and Marcus");
    });

    test("is found by Find a conversation under the title the platform sent", async () => {
        const w = mountSidebar({ rows: [team, { ...dmBig, title: "Setlist crew" }] });

        await w.get("[data-testid='sidebar-search']").setValue("setlist");
        expect(visibleRowIds(w)).toEqual([dmBig.channel_id]);
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

    test("draws the dot on an unread direct message's row beside the channels' Community rows", () => {
        const w = mountSidebar();

        expect(rowOf(w, dmAnna.channel_id).attributes("data-layout")).toBe("conversational");
        expect(rowOf(w, dmAnna.channel_id).find("[data-testid='row-dot']").exists()).toBe(true);
        expect(rowOf(w, youth.channel_id).attributes("data-layout")).toBe("community");
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

    test("flashes a row that arrives, and not the rows a fresh sidebar puts in a new order", async () => {
        const w = mountSidebar({ rows: [team, youth, dmAnna] });
        expect(w.findAll(".is-flash")).toHaveLength(0);

        // A reread sidebar puts Youth above Team, by its newer message, and Prayer arrives.
        await w.setProps({ rows: [youth, team, dmAnna, starred] });
        await nextTick();

        expect(rowOf(w, starred.channel_id).classes()).toContain("is-flash");
        expect(rowOf(w, youth.channel_id).classes()).not.toContain("is-flash");
        expect(rowOf(w, team.channel_id).classes()).not.toContain("is-flash");
        expect(rowOf(w, dmAnna.channel_id).classes()).not.toContain("is-flash");
    });

    test("flashes nothing when a room leaves, though every row below it climbs a place", async () => {
        const w = mountSidebar({ rows: [team, youth, starred] });

        await w.setProps({ rows: [youth, starred] });
        await nextTick();

        expect(w.findAll(".is-flash")).toHaveLength(0);
    });

    test("flashes nothing when the top room is unstarred, though every room it drops past climbs a place", async () => {
        const prayer = { ...starred, is_favorite: false };
        const w = mountSidebar({ rows: [{ ...team, is_favorite: true }, youth, prayer] });

        await w.setProps({ rows: [youth, team, prayer] });
        await nextTick();

        expect(w.findAll(".is-flash")).toHaveLength(0);
    });

    test("a group that gains a person keeps its row, its title and faces changing in place with no flash", async () => {
        const w = mountSidebar({ rows: [team, dmGroup] });
        const before = rowOf(w, dmGroup.channel_id).element;

        const dee = { person_alias_guid: "dee", nick_name: "Dee", last_name: null, avatar_url: null };
        await w.setProps({ rows: [team, { ...dmGroup, dm_people: [...(dmGroup.dm_people ?? []), dee], dm_other_count: 3, title: "Anna, Dee and Marcus" }] });
        await nextTick();

        expect(rowOf(w, dmGroup.channel_id).element).toBe(before);
        expect(rowOf(w, dmGroup.channel_id).get("[data-testid='row-name']").text()).toBe("Anna, Dee and Marcus");
        expect(w.findAll(".is-flash")).toHaveLength(0);
    });

    test("a group that loses a person becomes a two-person row in place, the person's face for the group's", async () => {
        const w = mountSidebar({ rows: [team, dmGroup] });
        const before = rowOf(w, dmGroup.channel_id).element;

        await w.setProps({ rows: [team, { ...dmGroup, dm_people: [dmGroup.dm_people![1]], dm_other_count: 1, room_kind: "person", title: "Marcus Reed" }] });
        await nextTick();

        expect(rowOf(w, dmGroup.channel_id).element).toBe(before);
        expect(rowOf(w, dmGroup.channel_id).get("[data-testid='row-name']").text()).toBe("Marcus Reed");
        expect(w.findAll(".is-flash")).toHaveLength(0);
    });

    test("flashes only the room the person stars, which moves up into Starred", async () => {
        const prayer = { ...starred, is_favorite: false };
        const w = mountSidebar({ rows: [team, youth, prayer] });

        await w.setProps({ rows: [{ ...prayer, is_favorite: true }, team, youth] });
        await nextTick();

        expect(rowOf(w, prayer.channel_id).classes()).toContain("is-flash");
        expect(rowOf(w, team.channel_id).classes()).not.toContain("is-flash");
        expect(rowOf(w, youth.channel_id).classes()).not.toContain("is-flash");
    });
});

describe("a row's look, by the room's kind", () => {
    test("a channel draws the Community row and a direct message the Conversational row, in one list, Starred included", () => {
        const starredDm = { ...dmRead, is_favorite: true };
        const w = mountSidebar({ rows: [team, youth, starred, muted, dmAnna, dmGroup, dmBig, starredDm] });

        for (const channel of [team, youth, starred, muted]) {
            expect(rowOf(w, channel.channel_id).attributes("data-layout")).toBe("community");
            expect(rowOf(w, channel.channel_id).classes()).toContain("is-cs");
        }
        for (const direct of [dmAnna, dmGroup, dmBig, starredDm]) {
            expect(rowOf(w, direct.channel_id).attributes("data-layout")).toBe("conversational");
            expect(rowOf(w, direct.channel_id).classes()).not.toContain("is-cs");
        }
        expect(rowIdsIn(w, "starred")).toEqual([starred.channel_id, starredDm.channel_id]);
    });

    test("Conversational draws the line the platform sent under the title, and the time", () => {
        // The line names a sender the raw fields do not, so only the platform's line reads this way.
        const w = mountSidebar({ rows: [{ ...dmGroup, preview_line: "Priya: See you Sunday" }] });

        expect(rowOf(w, dmGroup.channel_id).get("[data-testid='row-preview']").text()).toBe("Priya: See you Sunday");
        expect(rowOf(w, dmGroup.channel_id).find("[data-testid='row-time']").exists()).toBe(true);
    });

    test("draws a direct message by the kind the platform sent: a group showing one other person still draws a group's faces", () => {
        // Three people, one of them on the Chat Ban List, so the names show only one other.
        const banListGroup = { ...dmGroup, dm_people: [{ person_alias_guid: "anna", nick_name: "Anna", last_name: null, avatar_url: null }], dm_other_count: 1, room_kind: "group" as const };
        const w = mountSidebar({ rows: [banListGroup, dmRead] });

        expect(rowOf(w, dmGroup.channel_id).get("[data-testid='group-face']").attributes("data-shape")).toBe("pair");
        expect(rowOf(w, dmRead.channel_id).find("[data-testid='group-face']").exists()).toBe(false);
        expect(rowOf(w, dmRead.channel_id).findComponent(Avatar).props("size")).toBe(36);
    });

    test("Conversational draws the platform's empty line for an empty room", () => {
        const empty = { ...dmRead, last_message_id: null, last_message_at: null, last_message_preview: null, last_sender_nick_name: null, last_sender_last_name: null, preview_line: "No messages yet" };
        const w = mountSidebar({ rows: [empty] });

        expect(rowOf(w, dmRead.channel_id).get("[data-testid='row-preview']").text()).toBe("No messages yet");
    });

    test("Community draws the name alone, with no preview and no time", () => {
        const w = mountSidebar();
        const r = rowOf(w, team.channel_id);

        expect(r.attributes("data-layout")).toBe("community");
        expect(r.get("[data-testid='row-name']").text()).toBe("Worship Team");
        expect(r.find("[data-testid='row-preview']").exists()).toBe(false);
        expect(r.find("[data-testid='row-time']").exists()).toBe(false);
    });
});

describe("a channel's tile", () => {
    const pictured = row({ channel_id: "c0000010-0000-4000-8000-000000000000", name: "Greeters", is_public: true, icon_url: "https://church.example/GetImage.ashx?guid=team" });

    // The letter tile is always the base layer, so the photo fades in over it and never into an empty box.
    test("shows the channel's own image over its letter tile", () => {
        const w = mountSidebar({ rows: [pictured, team] });
        const image = rowOf(w, pictured.channel_id).get(".chat-ch-tile [data-testid='channel-mark-image']");

        expect(image.attributes("src")).toBe("https://church.example/GetImage.ashx?guid=team");
        expect(rowOf(w, pictured.channel_id).get("[data-testid='channel-mark-letter']").text()).toBe("G");
    });

    test("shows its first letter on the colour of its name's hue when it has no image", () => {
        const w = mountSidebar({ rows: [pictured, team] });
        const tile = rowOf(w, team.channel_id).get(".chat-ch-tile");

        expect(tile.get("[data-testid='channel-mark-letter']").text()).toBe("W");
        expect(tile.find("[data-testid='channel-mark-image']").exists()).toBe(false);
        // the people avatar's hash, so a channel and a person of one name share a hue
        expect((tile.element as HTMLElement).style.getPropertyValue("--chat-av-hue")).toBe(String(avatarHue("Worship Team")));
    });

    test("keeps a letter when the image fails to load", async () => {
        const w = mountSidebar({ rows: [pictured] });

        await w.get("[data-testid='channel-mark-image']").trigger("error");

        expect(w.get("[data-testid='channel-mark-letter']").text()).toBe("G");
    });

    test("a private channel shows a lock after its name, a public one none", () => {
        const w = mountSidebar({ rows: [pictured, team] });
        const title = rowOf(w, team.channel_id).get(".chat-row-title");

        expect(rowOf(w, pictured.channel_id).find("[data-testid='row-lock']").exists()).toBe(false);
        expect(title.element.children[0].getAttribute("data-testid")).toBe("row-name");
        expect(title.element.children[1].getAttribute("data-testid")).toBe("row-lock");
    });

    test("sits in the column a direct message's face does, in Starred beside one", () => {
        const w = mountSidebar({ rows: [starred, { ...dmRead, is_favorite: true }] });

        expect(rowOf(w, starred.channel_id).find(".chat-row-face .chat-ch-tile").exists()).toBe(true);
        expect(rowOf(w, dmRead.channel_id).find(".chat-row-face").exists()).toBe(true);
    });
});

describe("a channel's @ badge with @all", () => {
    test("counts the mentions by name and the @all mentions together", () => {
        const w = mountSidebar({ rows: [{ ...youth, mention_count: 2, all_mention_count: 1 }] });

        expect(rowOf(w, youth.channel_id).get("[data-testid='row-mention-badge']").text()).toContain("3");
        expect(rowOf(w, youth.channel_id).get("[data-testid='row-label-suffix']").text()).toBe(", private, 3 mentions, unread");
    });

    test("shows for @all alone", () => {
        const w = mountSidebar({ rows: [{ ...team, is_unread: true, mention_count: 0, all_mention_count: 2 }] });

        expect(rowOf(w, team.channel_id).get("[data-testid='row-mention-badge']").text()).toContain("2");
        expect(rowOf(w, team.channel_id).get("[data-testid='row-label-suffix']").text()).toBe(", private, 2 mentions, unread");
    });

    test("reads 99+ when the two together pass 99", () => {
        const w = mountSidebar({ rows: [{ ...youth, mention_count: 60, all_mention_count: 40 }] });

        expect(rowOf(w, youth.channel_id).get("[data-testid='row-mention-badge']").text()).toContain("99+");
    });
});

describe("a row's accessible name", () => {
    test("ends with a hidden suffix of mentions, unread and muted", () => {
        const w = mountSidebar();

        expect(rowOf(w, muted.channel_id).get("[data-testid='row-label-suffix']").text()).toBe(", private, 2 mentions, unread, muted");
        expect(rowOf(w, muted.channel_id).get("[data-testid='row-label-suffix']").classes()).toContain("chat-sr-only");
    });

    test("says one mention in the singular, and unread for a direct message", () => {
        const w = mountSidebar({ rows: [{ ...youth, mention_count: 1 }, dmAnna] });

        expect(rowOf(w, youth.channel_id).get("[data-testid='row-label-suffix']").text()).toBe(", private, 1 mention, unread");
        expect(rowOf(w, dmAnna.channel_id).get("[data-testid='row-label-suffix']").text()).toBe(", unread");
    });

    test("says 'private' for a private channel, whose lock is hidden, and not for a public one or a direct message", () => {
        const locked = row({ channel_id: "c0000008-0000-4000-8000-000000000000", name: "Leaders", is_public: false, is_member: true });
        const w = mountSidebar({ rows: [locked, { ...team, is_public: true, is_unread: false, mention_count: 0, all_mention_count: 0 }, dmAnna] });

        expect(rowOf(w, locked.channel_id).get("[data-testid='row-label-suffix']").text()).toContain("private");
        expect(rowOf(w, team.channel_id).get("[data-testid='row-label-suffix']").text()).not.toContain("private");
        expect(rowOf(w, dmAnna.channel_id).get("[data-testid='row-label-suffix']").text()).not.toContain("private");
    });

    test("says 'not joined' for a room the person is not in", () => {
        const w = mountSidebar({ rows: [{ ...team, is_member: false, always_shown: true }] });

        expect(rowOf(w, team.channel_id).get("[data-testid='row-label-suffix']").text()).toBe(", not joined, private");
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

    // Centred and calm: a glyph with no box, the line, one solid button and one link.
    test("offers New message as the one solid button and Browse channels as a link after it", async () => {
        const w = mountSidebar({ rows: [] });
        const empty = w.get("[data-testid='sidebar-empty']");

        expect(empty.find(".chat-side-empty-ic").exists()).toBe(false);
        expect(empty.find(".chat-side-empty-glyph svg").exists()).toBe(true);
        const buttons = empty.get(".chat-side-empty-actions").findAll("button");
        expect(buttons.map(button => button.text())).toEqual(["New message", "Browse channels"]);
        expect(buttons[0].attributes("data-testid")).toBe("empty-new-message");
        expect(buttons[0].classes()).toContain("is-solid");
        expect(buttons[1].attributes("data-testid")).toBe("empty-browse");
        expect(buttons[1].classes()).toContain("is-link");
        expect(buttons[1].classes()).not.toContain("is-solid");

        await buttons[1].trigger("click");
        expect(w.emitted("browse")).toHaveLength(1);
    });

    test("offers Browse channels without DM Access too, since browsing needs none", () => {
        const w = mountSidebar({ rows: [], isNewMessageShown: false });

        expect(w.get("[data-testid='empty-browse']").text()).toBe("Browse channels");
    });

    // With no New message beside it, the way in is the one thing to press, so it is the solid one.
    test("makes Browse channels the solid button when it is the only way in", () => {
        const w = mountSidebar({ rows: [], isNewMessageShown: false });

        expect(w.get("[data-testid='empty-browse']").classes()).toContain("is-solid");
    });
});

// Until the sidebar's first page arrives the list is loading, not empty: it draws skeleton rows,
// never the empty state, which would tell a person with conversations that they have none. A first
// page that failed says so with Try again, rather than loading forever.
describe("the loading sidebar", () => {
    test("draws skeleton rows, not the empty state, before the sidebar has loaded", () => {
        const w = mountSidebar({ rows: [], loadState: "loading" });

        expect(w.findAll("[data-testid='sidebar-skeleton']").length).toBeGreaterThan(0);
        expect(w.find("[data-testid='sidebar-empty']").exists()).toBe(false);
        expect(w.findAll("[data-testid='sidebar-section']")).toHaveLength(0);
    });

    test("tells assistive technology the list is loading, and hides the skeleton from it", () => {
        const w = mountSidebar({ rows: [], loadState: "loading" });
        const list = w.get(".chat-side-list");

        expect(list.attributes("aria-busy")).toBe("true");
        expect(list.get(".chat-sr-only").text()).toBe("Loading conversations");
        expect(w.get("[data-testid='sidebar-skeleton']").attributes("aria-hidden")).toBe("true");
        expect(w.get("[data-testid='sidebar-skeleton']").findAll(".chat-skeleton").length).toBeGreaterThan(0);
    });

    test("draws only the skeleton while loading, though a previewed room already has a row", () => {
        const w = mountSidebar({ rows: [team], loadState: "loading" });

        expect(w.find("[data-testid='sidebar-skeleton']").exists()).toBe(true);
        expect(w.findAll("[data-testid='channel-row']")).toHaveLength(0);
    });

    test("draws the empty state once loaded with no rows, and is no longer busy", async () => {
        const w = mountSidebar({ rows: [], loadState: "loading" });

        await w.setProps({ loadState: "loaded" });

        expect(w.find("[data-testid='sidebar-skeleton']").exists()).toBe(false);
        expect(w.find("[data-testid='sidebar-empty']").exists()).toBe(true);
        expect(w.get(".chat-side-list").attributes("aria-busy")).toBeUndefined();
    });

    test("says a failed first page could not load, with Try again, never the empty state", async () => {
        const w = mountSidebar({ rows: [], loadState: "failed" });
        const error = w.get("[data-testid='sidebar-error']");

        expect(error.classes()).toContain("chat-ls-err");
        expect(error.text()).toContain("Couldn't load your conversations.");
        expect(w.find("[data-testid='sidebar-empty']").exists()).toBe(false);
        expect(w.find("[data-testid='sidebar-skeleton']").exists()).toBe(false);

        await error.get("button").trigger("click");
        expect(w.emitted("retry")).toHaveLength(1);
    });

    test("glows nothing when the first sidebar lands after a previewed room's row", async () => {
        const w = mountSidebar({ rows: [team], loadState: "loading" });

        await w.setProps({ rows: [youth, team, dmAnna], loadState: "loaded" });

        expect(w.findAll(".is-flash")).toHaveLength(0);
    });
});

describe("Browse channels", () => {
    test("is a row at the end of the channels, after a room the person has not joined", () => {
        const preview = row({ channel_id: "c0000009-0000-4000-8000-000000000000", name: "Volunteers", is_member: false, is_public: true });
        const w = mountSidebar({ rows: [team, preview, youth, dmAnna] });

        const section = w.get("[data-testid='sidebar-section'][data-section='channels']");
        const browse = section.get("[data-testid='sidebar-browse']");
        const rows = section.findAll("[data-testid='channel-row']").map(r => r.element);
        expect(rowIdsIn(w, "channels")).toEqual([team.channel_id, youth.channel_id, preview.channel_id]);
        expect(rows[rows.length - 1].compareDocumentPosition(browse.element) & Node.DOCUMENT_POSITION_FOLLOWING).toBeTruthy();
        expect(browse.text()).toBe("Browse channels");
        expect(browse.classes()).toEqual(expect.arrayContaining(["chat-row", "is-cs", "is-browse"]));
        // a dashed tile in the channels' column, as every room above it has a tile
        expect(browse.find(".chat-row-face .chat-ch-tile.is-browse svg").exists()).toBe(true);
    });

    test("opens the directory from its row, and is current while the directory is open", async () => {
        const w = mountSidebar();

        expect(w.get("[data-testid='sidebar-browse']").attributes("aria-current")).toBeUndefined();
        await w.get("[data-testid='sidebar-browse']").trigger("click");
        expect(w.emitted("browse")).toHaveLength(1);

        await w.setProps({ isBrowseOpen: true });
        expect(w.get("[data-testid='sidebar-browse']").attributes("aria-current")).toBe("true");
    });

    test("takes the current mark from the conversation under it while the directory is open, and gives it back", async () => {
        const w = mountSidebar({ activeChannelId: team.channel_id, isBrowseOpen: true });

        expect(rowOf(w, team.channel_id).attributes("aria-current")).toBeUndefined();
        expect(w.get("[data-testid='sidebar-browse']").attributes("aria-current")).toBe("true");

        await w.setProps({ isBrowseOpen: false });
        expect(rowOf(w, team.channel_id).attributes("aria-current")).toBe("true");
    });

    test("is the Channels heading's 26px plus too, and only that heading's", async () => {
        const w = mountSidebar();

        const plus = w.get("[data-testid='sidebar-section'][data-section='channels'] [data-testid='section-browse']");
        expect(plus.attributes("aria-label")).toBe("Browse channels");
        expect(plus.classes()).toContain("chat-sec-btn");
        expect(w.findAll("[data-testid='section-browse']")).toHaveLength(1);

        await plus.trigger("click");
        expect(w.emitted("browse")).toHaveLength(1);
    });

    test("stays while a search narrows the rooms, as the way to find one not listed", async () => {
        const w = mountSidebar();

        await w.get("[data-testid='sidebar-search']").setValue("zzz");

        expect(w.find("[data-testid='sidebar-browse']").exists()).toBe(true);
    });
});

describe("a previewed room", () => {
    test("is muted among the channels and says it is not joined", () => {
        const preview = row({ channel_id: "c0000009-0000-4000-8000-000000000000", name: "Volunteers", is_member: false, is_public: true, last_message_id: null });
        const w = mountSidebar({ rows: [team, preview], activeChannelId: preview.channel_id });

        expect(rowOf(w, preview.channel_id).classes()).toContain("is-out");
        expect(rowOf(w, preview.channel_id).get("[data-testid='row-label-suffix']").text()).toBe(", not joined");
        expect(rowOf(w, preview.channel_id).attributes("aria-current")).toBe("true");
    });
});

// The live sidebar: the reader's own last message reads "You: " in any direct message, and a row
// glows once when a message from someone else reaches it, as the design's incoming message does.
describe("the reader's own last message", () => {
    test("reads as the platform's line says, its 'You: ' included, the row adding nothing of its own", () => {
        const ownPair = { ...dmAnna, last_message_is_own: true, last_sender_nick_name: "Jordan", last_sender_last_name: "Ellis", is_unread: false, preview_line: "You: Do you have a minute after rehearsal?" };
        const ownSystem = { ...dmGroup, last_message_is_own: true, last_message_type: "system", last_message_preview: "Jordan added Priya", preview_line: "Jordan added Priya" };
        const w = mountSidebar({ rows: [ownPair, ownSystem] });

        expect(rowOf(w, dmAnna.channel_id).get("[data-testid='row-preview']").text()).toBe("You: Do you have a minute after rehearsal?");
        expect(rowOf(w, dmGroup.channel_id).get("[data-testid='row-preview']").text()).toBe("Jordan added Priya");
    });
});

describe("the flash on a live message", () => {
    test("a direct message's row glows on a message from someone else, even where it does not move", async () => {
        const w = mountSidebar({ rows: [dmRead, dmGroup] });

        await w.setProps({ rows: [{ ...dmRead, last_message_id: 11, last_message_preview: "On my way", is_unread: true }, dmGroup] });
        await nextTick();

        expect(rowOf(w, dmRead.channel_id).classes()).toContain("is-flash");
        expect(rowOf(w, dmGroup.channel_id).classes()).not.toContain("is-flash");
    });

    test("not on the reader's own send, though it moves the row up", async () => {
        const w = mountSidebar({ rows: [dmGroup, dmRead] });

        await w.setProps({ rows: [{ ...dmRead, last_message_id: 11, last_message_preview: "On my way", last_message_is_own: true }, dmGroup] });
        await nextTick();

        expect(rowOf(w, dmRead.channel_id).classes()).not.toContain("is-flash");
    });

    test("a conversation new to the list glows with someone else's first message, not with the reader's own, as from another of their devices", async () => {
        const w = mountSidebar({ rows: [dmGroup] });

        await w.setProps({ rows: [{ ...dmRead, last_message_is_own: true }, { ...dmAnna, is_unread: true }, dmGroup] });
        await nextTick();

        expect(rowOf(w, dmRead.channel_id).classes()).not.toContain("is-flash");
        expect(rowOf(w, dmAnna.channel_id).classes()).toContain("is-flash");
    });

    test("on a post Rock made in the reader's name, which is news to them as its bold says", async () => {
        const w = mountSidebar({ rows: [dmGroup, dmRead] });

        await w.setProps({
            rows: [{ ...dmRead, last_message_id: 11, last_message_preview: "Your class starts at 7", last_message_is_own: true, last_message_posted_by_rock: true, is_unread: true }, dmGroup]
        });
        await nextTick();

        expect(rowOf(w, dmRead.channel_id).classes()).toContain("is-flash");
    });

    test("not on an edit of the newest message, nor a walk-back after a delete", async () => {
        const w = mountSidebar({ rows: [dmRead, dmGroup] });

        await w.setProps({ rows: [{ ...dmRead, last_message_preview: "Edited text" }, { ...dmGroup, last_message_id: 9, last_message_preview: "The one before" }] });
        await nextTick();

        expect(rowOf(w, dmRead.channel_id).classes()).not.toContain("is-flash");
        expect(rowOf(w, dmGroup.channel_id).classes()).not.toContain("is-flash");
    });

    test("glows for its own whole length, not ending when the dot it gains finishes its own animation", async () => {
        const w = mountSidebar({ rows: [dmRead, dmGroup] });

        await w.setProps({ rows: [{ ...dmRead, last_message_id: 11, last_message_preview: "On my way", is_unread: true }, dmGroup] });
        await nextTick();
        // the dot's animation ends first, and its event bubbles up through the row
        rowOf(w, dmRead.channel_id).get("[data-testid='row-dot']").element.dispatchEvent(new Event("animationend", { bubbles: true }));
        await nextTick();
        expect(rowOf(w, dmRead.channel_id).classes()).toContain("is-flash");

        // the row's own glow ending is what clears it
        rowOf(w, dmRead.channel_id).element.dispatchEvent(new Event("animationend", { bubbles: true }));
        await nextTick();
        expect(rowOf(w, dmRead.channel_id).classes()).not.toContain("is-flash");
    });

    test("not the conversation on screen, which a message from someone else reaches without glowing", async () => {
        const w = mountSidebar({ rows: [dmRead, dmGroup, team], activeChannelId: dmRead.channel_id });

        await w.setProps({ rows: [{ ...dmRead, last_message_id: 11, last_message_preview: "On my way" }, team, { ...dmGroup, last_message_id: 12, last_message_preview: "Hello", is_unread: true }] });
        await nextTick();

        expect(rowOf(w, dmRead.channel_id).classes()).not.toContain("is-flash");
        // another conversation reached at the same time still glows
        expect(rowOf(w, dmGroup.channel_id).classes()).toContain("is-flash");
    });

    test("a direct message a reread sidebar shows newer messages for does not glow, as after a reconnect; a live one after it does", async () => {
        const w = mountSidebar({ rows: [dmRead, dmGroup], sidebarReads: 1 });

        await w.setProps({
            rows: [{ ...dmRead, last_message_id: 11, last_message_preview: "On my way", is_unread: true }, { ...dmGroup, last_message_id: 12, is_unread: true }, dmAnna],
            sidebarReads: 2
        });
        await nextTick();

        expect(rowOf(w, dmRead.channel_id).classes()).not.toContain("is-flash");
        expect(rowOf(w, dmGroup.channel_id).classes()).not.toContain("is-flash");
        // a room the reread brings that was not listed is still news
        expect(rowOf(w, dmAnna.channel_id).classes()).toContain("is-flash");

        await w.setProps({ rows: [{ ...dmRead, last_message_id: 13, last_message_preview: "Here", is_unread: true }, { ...dmGroup, last_message_id: 12, is_unread: true }, dmAnna] });
        await nextTick();

        expect(rowOf(w, dmRead.channel_id).classes()).toContain("is-flash");
    });

    test("a direct message glows on its first live message after the page's first sidebar, which glows nothing", async () => {
        const w = mountSidebar({ rows: [], sidebarReads: 0, loadState: "loading" });

        await w.setProps({ rows: [dmRead, dmGroup], sidebarReads: 1, loadState: "loaded" });
        await nextTick();
        expect(w.findAll(".is-flash")).toHaveLength(0);

        await w.setProps({ rows: [{ ...dmRead, last_message_id: 11, last_message_preview: "On my way", is_unread: true }, dmGroup] });
        await nextTick();

        expect(rowOf(w, dmRead.channel_id).classes()).toContain("is-flash");
    });

    test("a channel's row does not glow on turning unread where it does not move: its bold and darker mark say it", async () => {
        const w = mountSidebar({ rows: [team, dmRead] });

        await w.setProps({ rows: [{ ...team, last_message_id: 11, is_unread: true }, dmRead] });
        await nextTick();

        expect(rowOf(w, team.channel_id).classes()).toContain("is-unread");
        expect(rowOf(w, team.channel_id).classes()).not.toContain("is-flash");
    });

    test("nor do channels a reload after a reconnect puts in a new order by their newer messages", async () => {
        const prayer = { ...starred, is_favorite: false };
        const w = mountSidebar({ rows: [team, youth, prayer, dmRead] });

        await w.setProps({
            rows: [
                { ...prayer, last_message_id: 12, last_message_at: "2026-10-12T10:00:00Z", is_unread: true },
                { ...youth, last_message_id: 11, last_message_at: "2026-10-12T09:30:00Z", is_unread: true },
                team,
                dmRead
            ]
        });
        await nextTick();

        expect(w.findAll(".is-flash")).toHaveLength(0);
    });
});

// The sidebar's foot is one button for who the person is and their settings: their face, name and
// campus, and a gear, opening Chat settings.
describe("the sidebar's foot", () => {
    test("names the person, their campus, and opens Chat settings", async () => {
        const w = mountSidebar({ personName: "Jordan Lee", personPhotoUrl: null, personCampusName: "Downtown Campus" });

        const me = w.get("[data-testid='sidebar-me']");
        expect(me.element.tagName).toBe("BUTTON");
        expect(me.attributes("aria-label")).toBe("Chat settings, Jordan Lee");
        expect(me.attributes("aria-haspopup")).toBe("dialog");
        expect(me.get("[data-testid='sidebar-me-name']").text()).toBe("Jordan Lee");
        expect(me.get("[data-testid='sidebar-me-campus']").text()).toBe("Downtown Campus");

        await me.trigger("click");
        expect(w.emitted("settings")).toHaveLength(1);
    });

    test("leaves the campus line out for a person with none", () => {
        const w = mountSidebar({ personName: "Jordan Lee", personCampusName: null });

        expect(w.find("[data-testid='sidebar-me-campus']").exists()).toBe(false);
    });

    test("is below the list, with the empty sidebar too", () => {
        const w = mountSidebar({ rows: [], personName: "Jordan Lee" });

        const side = w.get("[data-testid='channel-sidebar']").element;
        expect(side.lastElementChild?.contains(w.get("[data-testid='sidebar-me']").element)).toBe(true);
    });
});

// A direct message's row draws its people's own faces: the other person's photo, or a group's
// first two, each their initials only when they have no photo.
describe("a direct message's faces", () => {
    test("is the other person's photo", () => {
        const withPhoto = { ...dmAnna, dm_people: [{ person_alias_guid: "anna-kim", nick_name: "Anna", last_name: "Kim", avatar_url: "https://photos.example/anna.jpg" }] };
        const w = mountSidebar({ rows: [withPhoto] });

        expect(rowOf(w, dmAnna.channel_id).get(".chat-av img").attributes("src")).toBe("https://photos.example/anna.jpg");
    });

    test("is a group's first two people's photos, initials for one with none", () => {
        const withPhotos = {
            ...dmGroup,
            dm_people: [
                { person_alias_guid: "anna", nick_name: "Anna", last_name: "Reyes", avatar_url: "https://photos.example/anna.jpg" },
                { person_alias_guid: "marcus", nick_name: "Marcus", last_name: "Reed", avatar_url: null }
            ]
        };
        const w = mountSidebar({ rows: [withPhotos] });

        const faces = rowOf(w, dmGroup.channel_id).findAll(".chat-av");
        expect(faces).toHaveLength(2);
        expect(faces[0].get("img").attributes("src")).toBe("https://photos.example/anna.jpg");
        expect(faces[1].find("img").exists()).toBe(false);
        expect(faces[1].text()).toBe("M");
    });
});

// A group's mark: two other people a diagonal pair, three or more a triangle whose third bubble
// counts everyone past the first two, by the head count the row carries rather than the four people
// it names.
describe("a group direct message's mark", () => {
    test("is a diagonal pair for two other people", () => {
        const w = mountSidebar({ rows: [dmGroup] });

        expect(rowOf(w, dmGroup.channel_id).get("[data-testid='group-face']").attributes("data-shape")).toBe("pair");
        expect(rowOf(w, dmGroup.channel_id).find("[data-testid='group-face-more']").exists()).toBe(false);
    });

    test("is a triangle of the three for three other people", () => {
        const three = { ...dmBig, dm_people: dmBig.dm_people!.slice(0, 3), dm_other_count: 3, title: "Anna, Ben and Cara" };
        const w = mountSidebar({ rows: [three] });

        const mark = rowOf(w, dmBig.channel_id).get("[data-testid='group-face']");
        expect(mark.attributes("data-shape")).toBe("trio");
        expect(mark.findAll(".chat-av").map(face => face.text())).toEqual(["A", "B", "C"]);
    });

    test("counts everyone past the first two from the row's head count", () => {
        const w = mountSidebar({ rows: [dmBig] });

        const mark = rowOf(w, dmBig.channel_id).get("[data-testid='group-face']");
        expect(mark.attributes("data-shape")).toBe("trio");
        expect(mark.get("[data-testid='group-face-more']").text()).toBe("+4");
    });

    test("is the people icon for a named group, which the platform sends without its people", () => {
        const named = { ...dmBig, name: "Trail crew", title: "Trail crew", dm_people: null, dm_names: null, dm_other_count: null };
        const w = mountSidebar({ rows: [named] });

        const mark = rowOf(w, dmBig.channel_id).get("[data-testid='group-face']");
        expect(mark.attributes("data-shape")).toBe("none");
        expect(mark.find(".chat-av svg").exists()).toBe(true);
    });
});

// A platform older than this client sends a direct message's names without its people; the row
// still draws their initials.
describe("a direct message from an older platform", () => {
    test("draws its names' initials when it carries no people", () => {
        const older = { ...dmGroup, dm_people: undefined, dm_names: ["Anna", "Marcus"] } as unknown as typeof dmGroup;
        const w = mountSidebar({ rows: [older] });

        const faces = rowOf(w, dmGroup.channel_id).findAll(".chat-av");
        expect(faces.map(face => face.text())).toEqual(["A", "M"]);
    });
});
