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
// Browse channels, mounted: the church's public channels by name, the search that marks what it
// matched, the campus chips or the church's own campus note, the count with All and Not joined,
// and each row's Join in every state the door can leave it in. Every empty answer says why it is
// empty and offers the one way out. The keys move from the search into the rows and back, and
// Escape clears the search before it closes the view. What happens out of sight is spoken.
//
// The view draws the directory as the rooms hold it, so each test hands it a state in the exact
// shape useRooms keeps, and reads what it asks for from its events.
import { mount, VueWrapper } from "@vue/test-utils";
import { nextTick, reactive } from "vue";
import ChannelDirectory from "../../../../../src/Communication/Chat/ChatShell/components/channelDirectory.partial.obs";
import { DirectoryState, JoinState, RoomAnnouncement } from "../../../../../src/Communication/Chat/ChatShell/composables/useRooms.partial";
import { DirectoryRow } from "../../../../../src/Communication/Chat/ChatShell/types.partial";

// #region Fixtures

const downtown = { guid: "d0000001-0000-4000-8000-000000000000", name: "Downtown Campus", shortCode: "Downtown" };
const north = { guid: "d0000002-0000-4000-8000-000000000000", name: "North Campus", shortCode: null };

/** A row in the exact shape chat_list_directory answers. */
function room(overrides: Partial<DirectoryRow> & { channel_id: string, name: string }): DirectoryRow {
    return {
        icon_url: null,
        campus_id: null,
        is_member: false,
        can_post_code: null,
        description: null,
        group_type_name: null,
        ...overrides
    };
}

const volunteers = room({ channel_id: "c0000001-0000-4000-8000-000000000000", name: "Volunteers", description: "Serve teams across the church", group_type_name: "Serving Team" });
const outreach = room({ channel_id: "c0000002-0000-4000-8000-000000000000", name: "Local Outreach", campus_id: downtown.guid, group_type_name: "Small Group" });
const worship = room({ channel_id: "c0000003-0000-4000-8000-000000000000", name: "Worship Team", is_member: true, group_type_name: "Serving Team" });

/** Browse channels as useRooms holds it with a first page landed. */
function directory(overrides: Partial<DirectoryState> = {}): DirectoryState {
    return reactive({
        isOpen: true,
        query: "",
        campus: { kind: "all" },
        show: "all",
        rows: [volunteers, outreach, worship],
        total: 3,
        hasMore: false,
        isLoading: false,
        isSearching: false,
        isLoadingMore: false,
        error: null,
        moreError: null,
        isCampusLimited: false,
        emptyKind: null,
        ...overrides
    }) as DirectoryState;
}

// #endregion Fixtures

// #region Helpers

let wrapper: VueWrapper | null = null;

function mountDirectory(props: Record<string, unknown> = {}): VueWrapper {
    wrapper = mount(ChannelDirectory, {
        props: {
            directory: directory(),
            joins: {},
            announcement: null,
            campuses: [downtown, north],
            personCampusGuid: downtown.guid,
            organizationName: "Cedar Hill Church",
            isPhone: false,
            ...props
        },
        attachTo: document.body
    });
    return wrapper;
}

function rowOf(w: VueWrapper, channelId: string): ReturnType<VueWrapper["get"]> {
    return w.get(`[data-testid='directory-row'][data-channel-id='${channelId}']`);
}

function opens(w: VueWrapper): HTMLElement[] {
    return w.findAll("[data-testid='directory-row-open']").map(open => open.element as HTMLElement);
}

/** Gives the list's scroller the sizes a browser would, puts it at a position and says it scrolled. */
function scrollTo(w: VueWrapper, scrollTop: number, scrollHeight = 2000, clientHeight = 500): void {
    const scroller = w.get("[data-testid='directory-scroll']").element as HTMLElement;
    Object.defineProperty(scroller, "scrollHeight", { configurable: true, get: () => scrollHeight });
    Object.defineProperty(scroller, "clientHeight", { configurable: true, get: () => clientHeight });
    Object.defineProperty(scroller, "scrollTop", { configurable: true, get: () => scrollTop });
    scroller.dispatchEvent(new Event("scroll"));
}

// #endregion Helpers

beforeAll(() => {
    // jsdom has no ResizeObserver, and the tooltips keep their place with one.
    if (typeof window.ResizeObserver === "undefined") {
        window.ResizeObserver = class {
            observe(): void { /* jsdom lays nothing out, so there is nothing to watch. */ }
            unobserve(): void { /* As above. */ }
            disconnect(): void { /* As above. */ }
        } as unknown as typeof ResizeObserver;
    }
});

afterEach(() => {
    wrapper?.unmount();
    wrapper = null;
    document.body.innerHTML = "";
});

describe("the header", () => {
    test("names the view and the church it lists, with a close X on a wide block and no Back", () => {
        const w = mountDirectory();

        expect(w.get("[data-testid='directory-title']").text()).toBe("Browse channels");
        expect(w.get("[data-testid='directory-subtitle']").text()).toBe("Public channels at Cedar Hill Church");
        expect(w.get("[data-testid='directory-close']").attributes("aria-label")).toBe("Close Browse channels");
        expect(w.find("[data-testid='directory-back']").exists()).toBe(false);
        expect(w.get("[data-testid='channel-directory']").attributes("role")).toBe("region");
        expect(w.get("[data-testid='channel-directory']").attributes("aria-labelledby")).toBe(w.get("[data-testid='directory-title']").attributes("id"));
    });

    test("leads with Back on a phone and draws no X, each asking for its own way out", async () => {
        const w = mountDirectory({ isPhone: true });

        expect(w.find("[data-testid='directory-close']").exists()).toBe(false);
        await w.get("[data-testid='directory-back']").trigger("click");
        expect(w.emitted("back")).toHaveLength(1);
        expect(w.emitted("close")).toBeUndefined();
    });

    test("closes from its X", async () => {
        const w = mountDirectory();

        await w.get("[data-testid='directory-close']").trigger("click");

        expect(w.emitted("close")).toHaveLength(1);
    });
});

describe("the search", () => {
    test("is the field focus lands on as the view opens, by name, with no clear X while empty", () => {
        const w = mountDirectory();
        const search = w.get("[data-testid='directory-search']");

        expect(search.attributes("data-autofocus")).toBeDefined();
        expect(search.attributes("placeholder")).toBe("Search channels by name");
        expect(search.attributes("aria-label")).toBe("Search channels");
        expect(w.find("[data-testid='directory-search-clear']").exists()).toBe(false);
        expect(w.find("[data-testid='directory-search-busy']").exists()).toBe(false);
    });

    test("hands each keystroke to the rooms, which wait for the pause", async () => {
        const w = mountDirectory();

        await w.get("[data-testid='directory-search']").setValue("vol");

        expect(w.emitted("query")).toEqual([["vol"]]);
    });

    test("shows the spinner while a search waits for its pause or its answer", () => {
        const w = mountDirectory({ directory: directory({ query: "vol", isSearching: true }) });

        expect(w.get("[data-testid='directory-search-busy']").find(".chat-spinner").exists()).toBe(true);
    });

    test("offers a clear X once something is typed, which clears and keeps focus in the field", async () => {
        const w = mountDirectory({ directory: directory({ query: "vol" }) });

        const clear = w.get("[data-testid='directory-search-clear']");
        expect(clear.attributes("aria-label")).toBe("Clear search");
        await clear.trigger("click");

        expect(w.emitted("clearQuery")).toHaveLength(1);
        expect(document.activeElement).toBe(w.get("[data-testid='directory-search']").element);
    });
});

describe("the campus chips", () => {
    test("offer every campus by its short name, else its name, between All campuses and Whole church", () => {
        const w = mountDirectory();

        const chips = w.findAll("[data-testid='directory-chip']");
        expect(chips.map(chip => chip.text())).toEqual(["All campuses", "Downtown", "North Campus", "Whole church"]);
        expect(w.get("[data-testid='directory-chips']").attributes("role")).toBe("group");
        expect(w.get("[data-testid='directory-chips']").attributes("aria-label")).toBe("Campus");
    });

    test("press the chip of the campus chosen and no other", () => {
        const w = mountDirectory({ directory: directory({ campus: { kind: "campus", campusId: north.guid } }) });

        const pressed = w.findAll("[data-testid='directory-chip']").map(chip => chip.attributes("aria-pressed"));
        expect(pressed).toEqual(["false", "false", "true", "false"]);
    });

    test("press All campuses with nothing chosen, and Whole church for the rooms with no campus", () => {
        const all = mountDirectory();
        expect(all.findAll("[data-testid='directory-chip']").map(chip => chip.attributes("aria-pressed"))).toEqual(["true", "false", "false", "false"]);
        all.unmount();

        const whole = mountDirectory({ directory: directory({ campus: { kind: "wholeChurch" } }) });
        expect(whole.findAll("[data-testid='directory-chip']").map(chip => chip.attributes("aria-pressed"))).toEqual(["false", "false", "false", "true"]);
    });

    test("ask for the campus each one names", async () => {
        const w = mountDirectory();
        const chips = w.findAll("[data-testid='directory-chip']");

        await chips[1].trigger("click");
        await chips[3].trigger("click");
        await chips[0].trigger("click");

        expect(w.emitted("campus")).toEqual([[{ kind: "campus", campusId: downtown.guid }], [{ kind: "wholeChurch" }], [{ kind: "all" }]]);
    });

    test("give way to the church's note when it shows each person only their campus's rooms", () => {
        const w = mountDirectory({ directory: directory({ isCampusLimited: true }) });

        expect(w.find("[data-testid='directory-chips']").exists()).toBe(false);
        const note = w.get("[data-testid='directory-campus-note']");
        expect(note.text()).toBe("Showing channels for Downtown Campus and the whole church.");
        expect(note.get("b").text()).toBe("Downtown Campus");
    });
});

describe("the count line", () => {
    test("counts every channel the answer holds, past the page", () => {
        const w = mountDirectory({ directory: directory({ total: 7 }) });

        expect(w.get("[data-testid='directory-count']").text()).toBe("7 channels");
    });

    test("counts one channel in the singular", () => {
        const w = mountDirectory({ directory: directory({ rows: [volunteers], total: 1 }) });

        expect(w.get("[data-testid='directory-count']").text()).toBe("1 channel");
    });

    test("counts what a search matched", () => {
        const two = mountDirectory({ directory: directory({ query: "team", rows: [volunteers, worship], total: 2 }) });
        expect(two.get("[data-testid='directory-count']").text()).toBe("2 channels match");
        two.unmount();

        const one = mountDirectory({ directory: directory({ query: "vol", rows: [volunteers], total: 1 }) });
        expect(one.get("[data-testid='directory-count']").text()).toBe("1 channel matches");
    });

    test("follows the answer as it changes", async () => {
        const state = directory({ total: 7 });
        const w = mountDirectory({ directory: state });

        state.total = 12;
        await nextTick();

        expect(w.get("[data-testid='directory-count']").text()).toBe("12 channels");
    });

    test("carries All and Not joined, the one chosen pressed, each asking for its own set", async () => {
        const w = mountDirectory({ directory: directory({ show: "notJoined" }) });

        const show = w.get("[data-testid='directory-show']");
        expect(show.attributes("role")).toBe("group");
        expect(show.attributes("aria-label")).toBe("Show");
        expect(w.get("[data-testid='directory-show-all']").text()).toBe("All");
        expect(w.get("[data-testid='directory-show-all']").attributes("aria-pressed")).toBe("false");
        expect(w.get("[data-testid='directory-show-not-joined']").text()).toBe("Not joined");
        expect(w.get("[data-testid='directory-show-not-joined']").attributes("aria-pressed")).toBe("true");

        await w.get("[data-testid='directory-show-all']").trigger("click");
        await w.get("[data-testid='directory-show-not-joined']").trigger("click");
        expect(w.emitted("show")).toEqual([["all"], ["notJoined"]]);
    });
});

describe("a row", () => {
    test("draws the room's name, its description and its group type and campus, with no member count", () => {
        const w = mountDirectory();

        const row = rowOf(w, outreach.channel_id);
        expect(row.get("[data-testid='directory-row-name']").text()).toBe("Local Outreach");
        expect(row.findAll("[data-testid='directory-row-meta'] > span").map(part => part.text())).toEqual(["Small Group", "Downtown Campus"]);
        expect(row.text()).not.toContain("members");
        expect(rowOf(w, volunteers.channel_id).get("[data-testid='directory-row-desc']").text()).toBe("Serve teams across the church");
    });

    test("says Whole church for a room with no campus, and draws no description line without one", () => {
        const w = mountDirectory();

        expect(rowOf(w, volunteers.channel_id).findAll("[data-testid='directory-row-meta'] > span").map(part => part.text())).toEqual(["Serving Team", "Whole church"]);
        expect(rowOf(w, outreach.channel_id).find("[data-testid='directory-row-desc']").exists()).toBe(false);
    });

    test("marks the part of the name the search matched, in any letter case", () => {
        const w = mountDirectory({ directory: directory({ query: "  team ", rows: [worship], total: 1 }) });

        const name = rowOf(w, worship.channel_id).get("[data-testid='directory-row-name']");
        expect(name.text()).toBe("Worship Team");
        expect(name.get("mark").text()).toBe("Team");
    });

    test("marks nothing without a search", () => {
        const w = mountDirectory();

        expect(rowOf(w, worship.channel_id).find("mark").exists()).toBe(false);
    });

    test("opens the room it names, to preview it", async () => {
        const w = mountDirectory();

        await rowOf(w, volunteers.channel_id).get("[data-testid='directory-row-open']").trigger("click");

        expect(w.emitted("open")).toEqual([[volunteers.channel_id]]);
    });

    test("tells a screen reader the room, whether it is joined, its facts and what opening does", () => {
        const w = mountDirectory();

        expect(rowOf(w, volunteers.channel_id).get("[data-testid='directory-row-open']").attributes("aria-label")).toBe("Volunteers, Serving Team, Whole church. Preview channel");
        expect(rowOf(w, worship.channel_id).get("[data-testid='directory-row-open']").attributes("aria-label")).toBe("Worship Team, joined, Serving Team, Whole church. Open channel");
    });

    test("is a list of channels", () => {
        const w = mountDirectory();

        expect(w.get("[data-testid='directory-list']").element.tagName).toBe("UL");
        expect(w.get("[data-testid='directory-list']").attributes("aria-label")).toBe("Channels");
    });
});

describe("Join on a row", () => {
    test("offers Join for a room the person is not in, which asks the door for that room", async () => {
        const w = mountDirectory();

        const join = rowOf(w, volunteers.channel_id).get("[data-testid='directory-join']");
        expect(join.text()).toBe("Join");
        expect(join.attributes("aria-label")).toBe("Join Volunteers");
        await join.trigger("click");

        expect(w.emitted("join")).toEqual([[volunteers.channel_id]]);
        expect(w.emitted("open")).toBeUndefined();
    });

    test("says Joined with its check for a room the person is in, without the pop", () => {
        const w = mountDirectory();

        const joined = rowOf(w, worship.channel_id).get("[data-testid='directory-joined']");
        expect(joined.text()).toBe("Joined");
        expect(joined.classes()).not.toContain("is-fresh");
        expect(rowOf(w, worship.channel_id).classes()).toContain("is-member");
        expect(rowOf(w, worship.channel_id).find("[data-testid='directory-join']").exists()).toBe(false);
    });

    test("shows Joining with a spinner, busy and not pressable, while the door is out", () => {
        const joins: Record<string, JoinState> = { [volunteers.channel_id]: { kind: "joining" } };
        const w = mountDirectory({ joins });

        const joining = rowOf(w, volunteers.channel_id).get("[data-testid='directory-joining']");
        expect(joining.text()).toBe("Joining");
        expect(joining.find(".chat-spinner").exists()).toBe(true);
        // held by aria-disabled rather than disabled, since a browser drops focus from a disabled button
        expect(joining.attributes("aria-disabled")).toBe("true");
        expect(joining.attributes("aria-busy")).toBe("true");
    });

    test("asks nothing of the door while Joining is pressed again", async () => {
        const joins: Record<string, JoinState> = { [volunteers.channel_id]: { kind: "joining" } };
        const w = mountDirectory({ joins });

        await rowOf(w, volunteers.channel_id).get("[data-testid='directory-joining']").trigger("click");

        expect(w.emitted("join")).toBeUndefined();
    });

    test("pops a fresh Joined in, the row's icon turning member, once the door and the platform agree", () => {
        const joins: Record<string, JoinState> = { [volunteers.channel_id]: { kind: "joined", isFresh: true } };
        const w = mountDirectory({ joins });

        const joined = rowOf(w, volunteers.channel_id).get("[data-testid='directory-joined']");
        expect(joined.text()).toBe("Joined");
        expect(joined.classes()).toContain("is-fresh");
        expect(rowOf(w, volunteers.channel_id).classes()).toContain("is-member");
    });

    test("says Finishing up with a spinner, and Setting up under the name, while the platform catches up", () => {
        const joins: Record<string, JoinState> = { [volunteers.channel_id]: { kind: "pending" } };
        const w = mountDirectory({ joins });

        const row = rowOf(w, volunteers.channel_id);
        expect(row.get("[data-testid='directory-pending']").text()).toBe("Finishing up");
        expect(row.get("[data-testid='directory-pending']").find(".chat-spinner").exists()).toBe(true);
        expect(row.get("[data-testid='directory-row-wait']").text()).toBe("Setting up, a few seconds");
        expect(row.find("[data-testid='directory-join']").exists()).toBe(false);
    });

    test("holds Join disabled and shows the door's own sentence under the name when refused", () => {
        const joins: Record<string, JoinState> = { [volunteers.channel_id]: { kind: "refused", text: "Volunteers is full right now. Ask Nathan Hughes about joining." } };
        const w = mountDirectory({ joins });

        const row = rowOf(w, volunteers.channel_id);
        expect(row.get("[data-testid='directory-join']").attributes("disabled")).toBeDefined();
        expect(row.get("[data-testid='directory-row-refused']").text()).toBe("Volunteers is full right now. Ask Nathan Hughes about joining.");
    });

    test("moves focus from a Join turned refused to the row's own button, rather than to the page", async () => {
        const joins = reactive<Record<string, JoinState>>({ [volunteers.channel_id]: { kind: "joining" } });
        const w = mountDirectory({ joins });
        (rowOf(w, volunteers.channel_id).get("[data-testid='directory-joining']").element as HTMLElement).focus();

        joins[volunteers.channel_id] = { kind: "refused", text: "You can't join this channel." };
        await nextTick();
        await nextTick();

        expect(document.activeElement).toBe(rowOf(w, volunteers.channel_id).get("[data-testid='directory-row-open']").element);
    });

    test("says why under the name when the door could not be reached, and offers Join again", () => {
        const joins: Record<string, JoinState> = { [volunteers.channel_id]: { kind: "failed", text: "Couldn't join. You're offline, so try again in a moment." } };
        const w = mountDirectory({ joins });

        const row = rowOf(w, volunteers.channel_id);
        const join = row.get("[data-testid='directory-join']");
        expect(join.attributes("disabled")).toBeUndefined();
        expect(join.attributes("aria-disabled")).toBeUndefined();
        expect(row.find("[data-testid='directory-row-refused']").exists()).toBe(false);
        const line = row.get("[data-testid='directory-row-failed']");
        expect(line.text()).toBe("Couldn't join. You're offline, so try again in a moment.");
        expect(line.classes()).toContain("chat-fd-err");
        expect(row.get("[data-testid='directory-row-open']").element.contains(line.element)).toBe(true);
    });

    test("keeps focus in a row whose Join turns to Joining, on the row's own button", async () => {
        const joins = reactive<Record<string, JoinState>>({});
        const w = mountDirectory({ joins });
        (rowOf(w, volunteers.channel_id).get("[data-testid='directory-join']").element as HTMLElement).focus();

        joins[volunteers.channel_id] = { kind: "joining" };
        await nextTick();
        await nextTick();

        expect(rowOf(w, volunteers.channel_id).element.contains(document.activeElement)).toBe(true);
    });
});

describe("loading", () => {
    test("draws five skeleton rows of a real row's shape and says it is loading", () => {
        const w = mountDirectory({ directory: directory({ isLoading: true, rows: [], total: 0 }) });

        expect(w.findAll("[data-testid='directory-skeleton-row']")).toHaveLength(5);
        expect(w.get("[data-testid='directory-results']").attributes("aria-busy")).toBe("true");
        expect(w.get("[data-testid='directory-count']").text()).toBe("Loading channels…");
        expect(w.find("[data-testid='directory-list']").exists()).toBe(false);
    });

    test("is no longer busy once the page lands", () => {
        const w = mountDirectory();

        expect(w.get("[data-testid='directory-results']").attributes("aria-busy")).toBe("false");
        expect(w.find("[data-testid='directory-skeleton-row']").exists()).toBe(false);
    });

    test("says why the first page did not come, with Try again", async () => {
        const w = mountDirectory({ directory: directory({ rows: [], total: 0, error: { code: "network", severity: "degraded", text: null } }) });

        expect(w.get("[data-testid='directory-error']").text()).toContain("Couldn't load channels.");
        await w.get("[data-testid='directory-retry']").trigger("click");
        expect(w.emitted("retry")).toHaveLength(1);
    });

    test("shows the server's own sentence when the first page was refused", () => {
        const w = mountDirectory({ directory: directory({ rows: [], total: 0, error: { code: "authz.not_allowed", severity: "permission", text: "Chat is turned off for your account." } }) });

        expect(w.get("[data-testid='directory-error']").text()).toContain("Chat is turned off for your account.");
    });
});

describe("the next page", () => {
    test("is asked as the list nears its end, while there is more", () => {
        const w = mountDirectory({ directory: directory({ hasMore: true }) });

        scrollTo(w, 100);
        expect(w.emitted("loadMore")).toBeUndefined();

        scrollTo(w, 1350);
        expect(w.emitted("loadMore")).toHaveLength(1);
    });

    test("is never asked once the list has ended", () => {
        const w = mountDirectory({ directory: directory({ hasMore: false }) });

        scrollTo(w, 1500);

        expect(w.emitted("loadMore")).toBeUndefined();
    });

    test("shows that it is loading under the rows", () => {
        const w = mountDirectory({ directory: directory({ hasMore: true, isLoadingMore: true }) });

        expect(w.get("[data-testid='directory-more']").find(".chat-spinner").exists()).toBe(true);
    });

    test("keeps the rows and offers Try again when it did not come", async () => {
        const w = mountDirectory({ directory: directory({ hasMore: true, moreError: { code: "network", severity: "degraded", text: null } }) });

        expect(w.findAll("[data-testid='directory-row']")).toHaveLength(3);
        await w.get("[data-testid='directory-more-error'] button").trigger("click");
        expect(w.emitted("retry")).toHaveLength(1);
    });
});

describe("the scroll hairline", () => {
    test("appears under the tools once rows pass beneath them, and goes at the top", async () => {
        const w = mountDirectory();
        const scroller = w.get("[data-testid='directory-scroll']");

        scrollTo(w, 40);
        await nextTick();
        expect(scroller.classes()).toContain("is-scrolled");

        scrollTo(w, 0);
        await nextTick();
        expect(scroller.classes()).not.toContain("is-scrolled");
    });
});

describe("an empty answer", () => {
    test("with nothing public at all says so, naming the church, with no count and no button", () => {
        const w = mountDirectory({ directory: directory({ rows: [], total: 0, emptyKind: "none" }) });

        const empty = w.get("[data-testid='directory-empty']");
        expect(empty.attributes("data-kind")).toBe("none");
        expect(empty.get("b").text()).toBe("No channels to browse yet");
        expect(empty.text()).toContain("When Cedar Hill Church opens channels to everyone, they'll show up here.");
        expect(empty.find("button").exists()).toBe(false);
        expect(w.find("[data-testid='directory-count']").exists()).toBe(false);
    });

    test("to a search says what it looked at and offers Clear search", async () => {
        const w = mountDirectory({ directory: directory({ query: "zzz", rows: [], total: 0, emptyKind: "query" }) });

        const empty = w.get("[data-testid='directory-empty']");
        expect(empty.get("b").text()).toBe("No channels match “zzz”");
        expect(empty.text()).toContain("Search looks at channel names. Private groups don't show here; a leader can add you.");
        expect(w.get("[data-testid='directory-count']").text()).toBe("0 channels match");
        const action = empty.get("[data-testid='directory-empty-action']");
        expect(action.text()).toBe("Clear search");
        await action.trigger("click");
        expect(w.emitted("clearQuery")).toHaveLength(1);
    });

    test("to a search under filters names the filters and offers Clear filters", async () => {
        const w = mountDirectory({ directory: directory({ query: "zzz", campus: { kind: "campus", campusId: north.guid }, show: "notJoined", rows: [], total: 0, emptyKind: "filters" }) });

        const empty = w.get("[data-testid='directory-empty']");
        expect(empty.get("b").text()).toBe("No channels match “zzz”");
        expect(empty.text()).toContain("You're only looking at North Campus, and channels you haven't joined. Clear the filters to search every public channel.");
        const action = empty.get("[data-testid='directory-empty-action']");
        expect(action.text()).toBe("Clear filters");
        await action.trigger("click");
        expect(w.emitted("clearFilters")).toHaveLength(1);
    });

    test("to a search under the whole church says so", () => {
        const w = mountDirectory({ directory: directory({ query: "zzz", campus: { kind: "wholeChurch" }, rows: [], total: 0, emptyKind: "filters" }) });

        expect(w.get("[data-testid='directory-empty']").text()).toContain("You're only looking at the whole church. Clear the filters to search every public channel.");
    });

    test("with every room joined says nothing is left and offers Show all", async () => {
        const w = mountDirectory({ directory: directory({ show: "notJoined", rows: [], total: 0, emptyKind: "allJoined" }) });

        const empty = w.get("[data-testid='directory-empty']");
        expect(empty.get("b").text()).toBe("You're in every channel here");
        expect(empty.text()).toContain("Nothing left to join.");
        const action = empty.get("[data-testid='directory-empty-action']");
        expect(action.text()).toBe("Show all");
        await action.trigger("click");
        expect(w.emitted("show")).toEqual([["all"]]);
    });

    test("with every room joined at a campus says so for this campus", () => {
        const w = mountDirectory({ directory: directory({ show: "notJoined", campus: { kind: "campus", campusId: north.guid }, rows: [], total: 0, emptyKind: "allJoined" }) });

        expect(w.get("[data-testid='directory-empty']").text()).toContain("Nothing left to join for this campus.");
    });

    test("at a campus with no rooms offers All campuses", async () => {
        const w = mountDirectory({ directory: directory({ campus: { kind: "campus", campusId: north.guid }, rows: [], total: 0, emptyKind: "campus" }) });

        const empty = w.get("[data-testid='directory-empty']");
        expect(empty.get("b").text()).toBe("No channels for this campus yet");
        expect(empty.text()).toContain("Try another campus, or the whole church.");
        const action = empty.get("[data-testid='directory-empty-action']");
        expect(action.text()).toBe("All campuses");
        await action.trigger("click");
        expect(w.emitted("campus")).toEqual([[{ kind: "all" }]]);
    });
});

describe("the keys", () => {
    test("Down from the search goes to the first row", async () => {
        const w = mountDirectory();
        const search = w.get("[data-testid='directory-search']");
        (search.element as HTMLElement).focus();

        await search.trigger("keydown", { key: "ArrowDown" });

        expect(document.activeElement).toBe(opens(w)[0]);
    });

    test("Up and Down move between rows, and Up from the first row goes back to the search", async () => {
        const w = mountDirectory();
        const rows = opens(w);
        rows[0].focus();

        await w.findAll("[data-testid='directory-row-open']")[0].trigger("keydown", { key: "ArrowDown" });
        expect(document.activeElement).toBe(rows[1]);

        await w.findAll("[data-testid='directory-row-open']")[1].trigger("keydown", { key: "ArrowDown" });
        expect(document.activeElement).toBe(rows[2]);

        // the last row has nowhere further to go
        await w.findAll("[data-testid='directory-row-open']")[2].trigger("keydown", { key: "ArrowDown" });
        expect(document.activeElement).toBe(rows[2]);

        await w.findAll("[data-testid='directory-row-open']")[2].trigger("keydown", { key: "ArrowUp" });
        expect(document.activeElement).toBe(rows[1]);

        await w.findAll("[data-testid='directory-row-open']")[1].trigger("keydown", { key: "ArrowUp" });
        await w.findAll("[data-testid='directory-row-open']")[0].trigger("keydown", { key: "ArrowUp" });
        expect(document.activeElement).toBe(w.get("[data-testid='directory-search']").element);
    });

    test("Escape in a search that holds words clears it, and does not close", async () => {
        const w = mountDirectory({ directory: directory({ query: "vol" }) });
        const search = w.get("[data-testid='directory-search']");
        (search.element as HTMLElement).focus();

        await search.trigger("keydown", { key: "Escape" });

        expect(w.emitted("clearQuery")).toHaveLength(1);
        expect(w.emitted("close")).toBeUndefined();
    });

    test("Escape with the search empty closes the view", async () => {
        const w = mountDirectory();
        const search = w.get("[data-testid='directory-search']");
        (search.element as HTMLElement).focus();

        await search.trigger("keydown", { key: "Escape" });

        expect(w.emitted("close")).toHaveLength(1);
    });

    test("Escape from a row closes the view", async () => {
        const w = mountDirectory({ directory: directory({ query: "o" }) });

        await w.findAll("[data-testid='directory-row-open']")[0].trigger("keydown", { key: "Escape" });

        expect(w.emitted("close")).toHaveLength(1);
        expect(w.emitted("clearQuery")).toBeUndefined();
    });
});

describe("what is spoken", () => {
    test("a search's count and a join are read politely", () => {
        const announcement: RoomAnnouncement = { text: "2 channels found", isAssertive: false, count: 1 };
        const w = mountDirectory({ announcement });

        const polite = w.get("[data-testid='directory-announcement']");
        expect(polite.attributes("aria-live")).toBe("polite");
        expect(polite.text()).toBe("2 channels found");
        expect(w.get("[data-testid='directory-alert']").text()).toBe("");
    });

    test("a refused join is read assertively", () => {
        const announcement: RoomAnnouncement = { text: "Volunteers is full right now.", isAssertive: true, count: 2 };
        const w = mountDirectory({ announcement });

        const alert = w.get("[data-testid='directory-alert']");
        expect(alert.attributes("aria-live")).toBe("assertive");
        expect(alert.text()).toBe("Volunteers is full right now.");
        expect(w.get("[data-testid='directory-announcement']").text()).toBe("");
    });

    test("the same words again are put in afresh, so they are read again", async () => {
        const w = mountDirectory({ announcement: { text: "You joined Volunteers", isAssertive: false, count: 1 } });
        const before = w.get("[data-testid='directory-announcement'] span").element;

        await w.setProps({ announcement: { text: "You joined Volunteers", isAssertive: false, count: 2 } });

        expect(w.get("[data-testid='directory-announcement'] span").element).not.toBe(before);
    });
});
