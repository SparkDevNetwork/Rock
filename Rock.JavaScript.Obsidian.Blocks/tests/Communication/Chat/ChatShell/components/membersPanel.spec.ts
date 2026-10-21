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
// The members panel: who is in a room, in the shared panel host. A channel lists "Leaders" then
// "Members", a group conversation "In this conversation"; a heading counts its rows only once the
// loaded rows prove the count, since the roster carries no total. A row is the person's face and
// name, "(you)" for the reader, the Leader pill in a channel, the campus only when they show their
// details, and "Message" where they can be messaged. The filter searches the whole room: a line
// says it is searching, then how many it found, or that no one matches. Paging is "Load more",
// "Loading" while busy, "Couldn't load more." with Try again, and "That's everyone." at the end of
// a channel. A room the reader has not joined, one that keeps its list from them, a failed first
// page and the first load each draw a state of their own. A channel is named with no "#".
//
// The component these tests are written to, membersPanel.partial.obs:
//
//   props: channelName: string, memberCount: number | null (the room's facts), isChannel: boolean,
//          members: RosterMember[] (the rows to list),
//          hasMore, isLoading, isLoadingMore: boolean, error, moreError: ChatError | null,
//          refusal: "not_member" | "no_roster_right" | null, query: string,
//          searchStatus: "searching" | "answered" | "failed" | null, personAliasGuid: string,
//          campusNames: Record<string, string> (by campus guid in lower case), isPhone: boolean
//   emits: query(text), loadMore, retry, retrySearch, message(personAliasGuid),
//          person(personAliasGuid), close
//   marks: members-panel, members-filter, members-clear, members-section (with
//          members-section-count), member-row, member-who, member-you, member-leader,
//          member-campus, member-message, member-skeleton, members-state (data-state),
//          members-retry, members-more, members-more-error, members-end, members-hint,
//          members-search-retry, members-none
import { mount, VueWrapper } from "@vue/test-utils";
import { nextTick, reactive } from "vue";
import MembersPanel from "../../../../../src/Communication/Chat/ChatShell/components/membersPanel.partial.obs";
import { RosterMember } from "../../../../../src/Communication/Chat/ChatShell/types.partial";

// #region Fixtures

const me = "a0000001-0000-4000-8000-000000000000";
const downtown = "d0000001-0000-4000-8000-000000000000";

let next = 10;

/** One row of chat_list_members, exactly its keys. */
function member(nickName: string, lastName: string, extra: Partial<RosterMember> = {}): RosterMember {
    next++;
    return {
        person_alias_guid: `a00000${next}-0000-4000-8000-000000000000`,
        nick_name: nickName,
        last_name: lastName,
        avatar_url: null,
        is_leader: false,
        details: null,
        can_message: true,
        ...extra
    };
}

const tom = member("Tom", "Hale", { is_leader: true, details: { campus_id: downtown, badge_keys: [] } });
const grace = member("Grace", "Kim", { is_leader: true });
const anna = member("Anna", "Reyes", { details: { campus_id: downtown, badge_keys: [] } });
const ben = member("Ben", "Okafor", { can_message: false });
const self = member("Jordan", "Lee", { person_alias_guid: me });

let wrapper: VueWrapper | null = null;

function draw(props: Record<string, unknown> = {}): VueWrapper {
    wrapper = mount(MembersPanel, {
        props: {
            channelName: "young-adults",
            isChannel: true,
            members: [tom, grace, anna, ben, self],
            hasMore: false,
            isLoading: false,
            isLoadingMore: false,
            error: null,
            moreError: null,
            refusal: null,
            query: "",
            searchStatus: null,
            personAliasGuid: me,
            campusNames: { [downtown]: "Downtown" },
            isPhone: false,
            ...props
        },
        attachTo: document.body
    });
    return wrapper;
}

afterEach(() => {
    wrapper?.unmount();
    wrapper = null;
});

/** An element's text as a person reads it: each element's words apart from the next. */
function spaced(node: Node): string {
    return node.nodeType === Node.TEXT_NODE ? node.textContent ?? "" : Array.from(node.childNodes).map(child => ` ${spaced(child)} `).join("");
}

const read = (element: Element): string => spaced(element).replace(/\s+/g, " ").replace(/ ([.,])/g, "$1").trim();
const text = (w: VueWrapper, mark: string): string => read(w.get(`[data-testid='${mark}']`).element);
const all = (w: VueWrapper, mark: string): string[] => w.findAll(`[data-testid='${mark}']`).map(e => read(e.element));

// #endregion Fixtures

describe("sections", () => {
    test("a channel lists Leaders then Members, each counted once every row has loaded", () => {
        const w = draw();

        expect(all(w, "members-section")).toEqual(["Leaders 2", "Members 3"]);
        expect(w.findAll("[data-testid='member-row']").map(r => r.get("[data-testid='member-who']").attributes("aria-label")))
            .toEqual(["Tom Hale, leader, Downtown. View profile", "Grace Kim, leader. View profile", "Anna Reyes, Downtown. View profile", "Ben Okafor. View profile", "Jordan Lee, you. View profile"]);
    });

    test("with more to load, Members carries no count, and Leaders only once a member row has loaded", () => {
        expect(all(draw({ hasMore: true }), "members-section")).toEqual(["Leaders 2", "Members"]);
        wrapper?.unmount();
        expect(all(draw({ hasMore: true, members: [tom, grace] }), "members-section")).toEqual(["Leaders"]);
    });

    test("a group conversation lists everyone under In this conversation, with no Leader pill", () => {
        const w = draw({ isChannel: false, channelName: "Hike planning", members: [tom, anna, self] });

        expect(all(w, "members-section")).toEqual(["In this conversation 3"]);
        expect(w.find("[data-testid='member-leader']").exists()).toBe(false);
        expect(w.find("[data-testid='members-end']").exists()).toBe(false);
    });
});

describe("a row", () => {
    test("draws the name, (you), the Leader pill and the campus only when the person shows details", () => {
        const w = draw();
        const rows = w.findAll("[data-testid='member-row']");

        expect(rows[0].get("[data-testid='member-leader']").text()).toBe("Leader");
        expect(rows[0].get("[data-testid='member-leader']").classes()).toContain("chat-pill");
        expect(rows[0].get("[data-testid='member-campus']").text()).toBe("Downtown");
        expect(rows[1].find("[data-testid='member-campus']").exists()).toBe(false);
        expect(rows[2].find("[data-testid='member-leader']").exists()).toBe(false);
        expect(rows[4].get("[data-testid='member-you']").text()).toBe("(you)");
        expect(rows[0].find("[data-testid='member-you']").exists()).toBe(false);
    });

    test("a campus the init box does not name draws nothing", () => {
        const w = draw({ campusNames: {} });

        expect(w.find("[data-testid='member-campus']").exists()).toBe(false);
    });

    test("Message shows where the person can be messaged, never on the reader's own row, and asks for them", async () => {
        const w = draw();
        const rows = w.findAll("[data-testid='member-row']");

        expect(rows[3].find("[data-testid='member-message']").exists()).toBe(false);
        expect(rows[4].find("[data-testid='member-message']").exists()).toBe(false);
        const message = rows[2].get("[data-testid='member-message']");
        expect(message.attributes("aria-label")).toBe("Message Anna Reyes");

        await message.trigger("click");
        expect(w.emitted("message")).toEqual([[anna.person_alias_guid]]);
    });

    test("the row's name opens the person", async () => {
        const w = draw();

        await w.findAll("[data-testid='member-who']")[2].trigger("click");
        expect(w.emitted("person")).toEqual([[anna.person_alias_guid]]);
    });
});

describe("states", () => {
    test("loading draws seven skeleton rows and says so", () => {
        const w = draw({ isLoading: true, members: [] });

        expect(w.findAll("[data-testid='member-skeleton']")).toHaveLength(7);
        expect(w.get("[data-testid='members-panel']").attributes("aria-busy")).toBe("true");
        expect(w.text()).toContain("Loading members");
    });

    test("a room the reader has not joined asks them to join, with no filter", () => {
        const w = draw({ refusal: "not_member", members: [] });

        expect(w.get("[data-testid='members-state']").attributes("data-state")).toBe("not_member");
        expect(text(w, "members-state")).toBe("Join to see who's here Members of young-adults can see its member list.");
        expect(w.find("[data-testid='members-filter']").exists()).toBe(false);
    });

    test("a room that keeps its list from the reader says so", () => {
        const w = draw({ refusal: "no_roster_right", members: [] });

        expect(text(w, "members-state")).toBe("Only leaders can see who's here young-adults keeps its member list private. You can still message people you know.");
        expect(w.find("[data-testid='members-filter']").exists()).toBe(false);
    });

    test("a first page that failed offers Try again", async () => {
        const w = draw({ error: { code: "rpc.transport", severity: "failed" }, members: [] });

        expect(text(w, "members-state")).toBe("Couldn't load members Check your connection, then try again. Try again");
        await w.get("[data-testid='members-retry']").trigger("click");
        expect(w.emitted("retry")).toHaveLength(1);
    });
});

describe("paging", () => {
    test("Load more asks for the next page, and stays focusable while it loads", async () => {
        const w = draw({ hasMore: true });

        await w.get("[data-testid='members-more']").trigger("click");
        expect(w.emitted("loadMore")).toHaveLength(1);

        await w.setProps({ isLoadingMore: true });
        const busy = w.get("[data-testid='members-more']");
        expect(busy.text()).toBe("Loading");
        expect(busy.attributes("aria-disabled")).toBe("true");
        expect(busy.attributes("disabled")).toBeUndefined();
        await busy.trigger("click");
        expect(w.emitted("loadMore")).toHaveLength(1);
    });

    test("a page that failed says so with Try again", async () => {
        const w = draw({ hasMore: true, moreError: { code: "rpc.transport", severity: "failed" } });

        expect(text(w, "members-more-error")).toBe("Couldn't load more.");
        expect(w.get("[data-testid='members-more']").text()).toBe("Try again");
        await w.get("[data-testid='members-more']").trigger("click");
        expect(w.emitted("loadMore")).toHaveLength(1);
    });

    test("the end of a channel is That's everyone.", () => {
        expect(text(draw(), "members-end")).toBe("That's everyone.");
        wrapper?.unmount();
        expect(draw({ hasMore: true }).find("[data-testid='members-end']").exists()).toBe(false);
    });

    test("the rows a page brought rise in and the first of them takes focus", async () => {
        const w = draw({ hasMore: true, members: [tom, grace, anna] });
        expect(w.findAll("[data-testid='member-row'].is-in")).toHaveLength(0);

        await w.get("[data-testid='members-more']").trigger("focus");
        (w.get("[data-testid='members-more']").element as HTMLElement).focus();
        await w.setProps({ isLoadingMore: true });
        await w.setProps({ isLoadingMore: false, members: [tom, grace, anna, ben, self] });
        await nextTick();

        const fresh = w.findAll("[data-testid='member-row'].is-in");
        expect(fresh).toHaveLength(2);
        expect(document.activeElement).toBe(w.findAll("[data-testid='member-who']")[3].element);
    });
});

describe("the search", () => {
    test("the filter asks for what was typed, and its own X clears it", async () => {
        const w = draw();
        expect(w.find("[data-testid='members-clear']").exists()).toBe(false);

        await w.get("[data-testid='members-filter']").setValue("an");
        expect(w.emitted("query")).toEqual([["an"]]);

        await w.setProps({ query: "an", searchStatus: "searching", members: [anna] });
        await w.get("[data-testid='members-clear']").trigger("click");
        expect(w.emitted("query")?.[1]).toEqual([""]);
    });

    test("Escape clears a query before it closes the panel", async () => {
        const w = draw({ query: "an", searchStatus: "answered", members: [anna] });

        await w.get("[data-testid='members-filter']").trigger("keydown", { key: "Escape" });
        expect(w.emitted("query")).toEqual([[""]]);
        expect(w.emitted("close")).toBeUndefined();

        await w.setProps({ query: "", searchStatus: null });
        await w.get("[data-testid='members-filter']").trigger("keydown", { key: "Escape" });
        expect(w.emitted("close")).toHaveLength(1);
    });

    test("while it searches the whole room the line says so, over the loaded rows that match", () => {
        const w = draw({ query: "an", searchStatus: "searching", members: [anna] });

        expect(text(w, "members-hint")).toBe("Searching everyone in young-adults");
        expect(all(w, "members-section")).toEqual(["Members"]);
        expect(w.find("[data-testid='members-more']").exists()).toBe(false);
        expect(w.find("[data-testid='members-end']").exists()).toBe(false);
    });

    test("an answer says how many people it found, one as a person", () => {
        expect(text(draw({ query: "a", searchStatus: "answered", members: [tom, grace, anna] }), "members-hint")).toBe("3 people in young-adults");
        wrapper?.unmount();
        expect(text(draw({ query: "an", searchStatus: "answered", members: [anna] }), "members-hint")).toBe("1 person in young-adults");
    });

    test("no one matching says so", () => {
        const w = draw({ query: "zz", searchStatus: "answered", members: [] });

        expect(text(w, "members-hint")).toBe("Searched everyone in young-adults");
        expect(text(w, "members-none")).toBe("No one in young-adults matches “zz”.");
        expect(w.find("[data-testid='members-section']").exists()).toBe(false);
    });

    test("a search that failed offers Try again", async () => {
        const w = draw({ query: "an", searchStatus: "failed", members: [anna] });

        expect(text(w, "members-hint")).toBe("Couldn't search right now. Try again");
        await w.get("[data-testid='members-search-retry']").trigger("click");
        expect(w.emitted("retrySearch")).toHaveLength(1);
    });

    test("a full page of matches can load more under the query", () => {
        const w = draw({ query: "a", searchStatus: "answered", members: [tom, anna], hasMore: true });

        expect(w.find("[data-testid='members-more']").exists()).toBe(true);
    });
});

// Under "Members" the panel names the room and how many are in it, once the room's facts are read:
// a channel by name with no "#" and its members, a group by its title and its people.
describe("the subtitle", () => {
    test("names a channel and its members", () => {
        const w = draw({ memberCount: 42 });

        expect(text(w, "panel-subtitle")).toBe("young-adults · 42 members");
    });

    test("names a group and its people, one person as one", async () => {
        const w = draw({ channelName: "Hike planning", isChannel: false, memberCount: 4 });
        expect(text(w, "panel-subtitle")).toBe("Hike planning · 4 people");

        await w.setProps({ memberCount: 1 });
        expect(text(w, "panel-subtitle")).toBe("Hike planning · 1 person");
    });

    test("counts a large room the way the page writes numbers, and one member as one", async () => {
        const w = draw({ memberCount: 13536 });
        expect(text(w, "panel-subtitle")).toBe(`young-adults · ${(13536).toLocaleString()} members`);

        await w.setProps({ memberCount: 1 });
        expect(text(w, "panel-subtitle")).toBe("young-adults · 1 member");
    });

    test("is the name alone until the room's facts are read", () => {
        const w = draw({ memberCount: null });

        expect(text(w, "panel-subtitle")).toBe("young-adults");
    });
});

// Changing who is in the room. Where Rock lets the reader manage the room's people, the list opens
// with an Add people row (a group of nine draws it disabled, "Full, 9 of 9"), and each other row
// gains a More menu: View profile, Message where the person can be messaged, then Remove, or for a
// leader's row seen by someone who is not a Chat Administrator, a lock note in Remove's place. A
// row being removed dims with "Removing"; a refusal shows under the row in the door's words with a
// dismiss X.
//
//   props += canManageMembers: boolean, isChatAdministrator: boolean,
//            removals: Record<string, { kind: "removing" } | { kind: "refused", text, isStale }>
//   emits += addPeople, remove(personAliasGuid), dismissRemoval(personAliasGuid)
//   marks += members-add, members-add-full, member-more, member-removing, member-refusal,
//            member-refusal-dismiss; the menu's items by data-key (profile, message, remove) and
//            its menu-note

/** The open menu's items, as a person reads them. */
const menuItems = (): string[] => Array.from(document.querySelectorAll("[role='menuitem']")).map(e => read(e));

async function openMenu(w: VueWrapper, index: number): Promise<void> {
    await w.findAll("[data-testid='member-more']")[index].trigger("click");
    await nextTick();
}

describe("Add people", () => {
    test("the row opens the list for those who may add, and asks for the dialog", async () => {
        const w = draw({ canManageMembers: true });

        expect(text(w, "members-add")).toBe("Add people");
        await w.get("[data-testid='members-add']").trigger("click");
        expect(w.emitted("addPeople")).toHaveLength(1);
    });

    test("is not offered to someone who may not add, nor under a query", async () => {
        expect(draw().find("[data-testid='members-add']").exists()).toBe(false);
        wrapper?.unmount();

        const w = draw({ canManageMembers: true, query: "an", searchStatus: "answered" });
        expect(w.find("[data-testid='members-add']").exists()).toBe(false);
    });

    test("a full group draws it disabled, as Full, 9 of 9", () => {
        const w = draw({ canManageMembers: true, isChannel: false, channelName: "Hike planning", memberCount: 9, members: [anna, self] });

        expect(w.find("[data-testid='members-add']").exists()).toBe(false);
        const full = w.get("[data-testid='members-add-full']");
        expect(read(full.element)).toBe("Add people Full, 9 of 9");
        expect(full.attributes("aria-disabled")).toBe("true");
    });
});

describe("the row menu", () => {
    test("offers no More to someone who may not manage the room", () => {
        expect(draw().find("[data-testid='member-more']").exists()).toBe(false);
    });

    test("a manager's More on another person's row offers View profile, Message, then Remove from the channel", async () => {
        const w = draw({ canManageMembers: true, isChatAdministrator: true });

        const mores = w.findAll("[data-testid='member-more']");
        expect(mores.map(m => m.attributes("aria-label"))).toEqual(["More for Tom Hale", "More for Grace Kim", "More for Anna Reyes", "More for Ben Okafor"]);

        await openMenu(w, 2);
        expect(menuItems()).toEqual(["View profile", "Message", "Remove from young-adults"]);
        expect(document.querySelector("[data-key='remove']")?.classList.contains("is-danger")).toBe(true);
    });

    test("leaves out Message where the person cannot be messaged, and a group says this conversation", async () => {
        const w = draw({ canManageMembers: true, isChannel: false, channelName: "Hike planning", members: [ben, anna, self] });

        await openMenu(w, 0);
        expect(menuItems()).toEqual(["View profile", "Remove from this conversation"]);
    });

    test("each item asks for what it says", async () => {
        const w = draw({ canManageMembers: true });

        await openMenu(w, 2);
        document.querySelector<HTMLElement>("[data-key='profile']")!.click();
        expect(w.emitted("person")).toEqual([[anna.person_alias_guid]]);

        await openMenu(w, 2);
        document.querySelector<HTMLElement>("[data-key='message']")!.click();
        expect(w.emitted("message")).toEqual([[anna.person_alias_guid]]);

        await openMenu(w, 2);
        document.querySelector<HTMLElement>("[data-key='remove']")!.click();
        expect(w.emitted("remove")).toEqual([[anna.person_alias_guid]]);
    });

    test("a leader's row seen by someone who is not a Chat Administrator shows the lock note in Remove's place", async () => {
        const w = draw({ canManageMembers: true, isChatAdministrator: false });

        await openMenu(w, 0);
        expect(menuItems()).toEqual(["View profile", "Message"]);
        expect(document.querySelector("[data-key='remove']")).toBeNull();
        expect(read(document.querySelector("[data-testid='menu-note']")!)).toBe("Only Chat Administrators can remove a leader");
    });

    test("a Chat Administrator may remove a leader", async () => {
        const w = draw({ canManageMembers: true, isChatAdministrator: true });

        await openMenu(w, 0);
        expect(menuItems()).toEqual(["View profile", "Message", "Remove from young-adults"]);
        expect(document.querySelector("[data-testid='menu-note']")).toBeNull();
    });
});

describe("removing a row", () => {
    test("a row being removed dims, says Removing, and offers nothing else", () => {
        const w = draw({ canManageMembers: true, removals: { [anna.person_alias_guid]: { kind: "removing" } } });

        const row = w.findAll("[data-testid='member-row']")[2];
        expect(row.classes()).toContain("is-busy");
        expect(read(row.get("[data-testid='member-removing']").element)).toBe("Removing");
        expect(row.find("[data-testid='member-more']").exists()).toBe(false);
        expect(row.find("[data-testid='member-message']").exists()).toBe(false);
    });

    test("a refusal shows under the row in the door's words, and its X dismisses it", async () => {
        const text = "Couldn't remove Anna. You're offline; try again when you're back.";
        const w = draw({ canManageMembers: true, removals: { [anna.person_alias_guid]: { kind: "refused", text, isStale: false } } });

        const refusal = w.findAll("[data-testid='member-row']")[2].get("[data-testid='member-refusal']");
        expect(read(refusal.element)).toBe(text);
        expect(refusal.attributes("role")).toBe("alert");

        await refusal.get("[data-testid='member-refusal-dismiss']").trigger("click");
        expect(w.emitted("dismissRemoval")).toEqual([[anna.person_alias_guid]]);
    });

    test("once a removed row leaves, focus moves to the row after it", async () => {
        const w = draw({ canManageMembers: true });
        (w.findAll("[data-testid='member-who']")[2].element as HTMLElement).focus();

        await w.setProps({ removals: { [anna.person_alias_guid]: { kind: "removing" } } });
        await w.setProps({ removals: {}, members: [tom, grace, ben, self] });
        await nextTick();

        expect(document.activeElement?.getAttribute("data-person")).toBe(ben.person_alias_guid);
    });

    test("an in-place removal change remembers the next row before the removed row leaves", async () => {
        const removals = reactive<Record<string, { kind: "removing" }>>({});
        const w = draw({ canManageMembers: true, removals });
        (w.findAll("[data-testid='member-who']")[2].element as HTMLElement).focus();

        // The roster keeps this dictionary and mutates its entries while a door is running.
        removals[anna.person_alias_guid] = { kind: "removing" };
        await nextTick();
        delete removals[anna.person_alias_guid];
        await w.setProps({ members: [tom, grace, ben, self] });
        await nextTick();

        expect(document.activeElement?.getAttribute("data-person")).toBe(ben.person_alias_guid);
    });
});
