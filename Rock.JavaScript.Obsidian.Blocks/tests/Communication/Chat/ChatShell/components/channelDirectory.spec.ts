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
import { readFileSync } from "fs";
import { join } from "path";
import { mount, VueWrapper } from "@vue/test-utils";
import { nextTick, reactive } from "vue";
import ChannelDirectory from "../../../../../src/Communication/Chat/ChatShell/components/channelDirectory.partial.obs";
import ChannelMark from "../../../../../src/Communication/Chat/ChatShell/ui/channelMark.partial.obs";
import JoinBar from "../../../../../src/Communication/Chat/ChatShell/components/joinBar.partial.obs";
import ListRow from "../../../../../src/Communication/Chat/ChatShell/ui/listRow.partial.obs";
import LoadMoreRow from "../../../../../src/Communication/Chat/ChatShell/ui/loadMoreRow.partial.obs";
import { DirectoryState, JoinState, LeaveState } from "../../../../../src/Communication/Chat/ChatShell/composables/useRooms.partial";
import { DirectoryRow } from "../../../../../src/Communication/Chat/ChatShell/types.partial";
import { providedSession } from "../sessionShapes";


/** The rules a component's styles give under reduced motion, selector and body, read from its source. */
function reducedMotionRules(relativePath: string): Array<{ selector: string, body: string }> {
    const source = readFileSync(join(__dirname, relativePath), "utf8");
    const style = source.slice(source.indexOf("<style"), source.indexOf("</style>")).replace(/\/\*[\s\S]*?\*\//g, "");
    const rules: Array<{ selector: string, body: string }> = [];
    for (const media of style.matchAll(/@media \(prefers-reduced-motion: reduce\)\s*\{/g)) {
        // the block runs to the brace that closes the one the query opened
        let depth = 1;
        let end = (media.index ?? 0) + media[0].length;
        for (; end < style.length && depth > 0; end++) {
            depth += style[end] === "{" ? 1 : style[end] === "}" ? -1 : 0;
        }
        const inner = style.slice((media.index ?? 0) + media[0].length, end - 1);
        rules.push(...Array.from(inner.matchAll(/([^{}]+)\{([^{}]*)\}/g)).map(m => ({ selector: m[1].trim(), body: m[2] })));
    }
    return rules;
}

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

const volunteers = room({ channel_id: "c0000001-0000-4000-8000-000000000000", name: "Volunteers", description: "Serve teams across the church", group_type_name: "Serving Team", member_count: 1248 });
const outreach = room({ channel_id: "c0000002-0000-4000-8000-000000000000", name: "Local Outreach", campus_id: downtown.guid, group_type_name: "Small Group", member_count: 1 });
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

/** Mounts the view; a phone's width comes from the session, as the shell provides it. */
function mountDirectory(props: Record<string, unknown> = {}): VueWrapper {
    const { isPhone = false, organizationName = "Cedar Hill Church", slots, ...rest } = props;
    wrapper = mount(ChannelDirectory, {
        slots: slots as Record<string, string> | undefined,
        props: {
            directory: directory(),
            joins: {},
            campuses: [downtown, north],
            personCampusGuid: downtown.guid,
            ...rest
        },
        global: { provide: providedSession({ isPhone: isPhone as boolean, churchName: organizationName as string }) },
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

/** The observers the view made, so a test can say what came into view; jsdom computes none. */
const observers: FakeObserver[] = [];

class FakeObserver {
    public watched: Element[] = [];
    constructor(public callback: IntersectionObserverCallback, public options?: IntersectionObserverInit) {
        observers.push(this);
    }
    observe(element: Element): void {
        this.watched.push(element);
    }
    unobserve(element: Element): void {
        this.watched = this.watched.filter(one => one !== element);
    }
    disconnect(): void {
        this.watched = [];
    }
    takeRecords(): IntersectionObserverEntry[] {
        return [];
    }
    see(isIntersecting: boolean): void {
        this.callback(this.watched.map(target => ({ target, isIntersecting }) as IntersectionObserverEntry), this as unknown as IntersectionObserver);
    }
}

/** The list's end comes into view, as a reader scrolling near it would bring it. */
async function reachEnd(): Promise<void> {
    await nextTick();
    observers.filter(observer => observer.watched.length > 0).forEach(observer => observer.see(true));
    await nextTick();
}

/** The view's width observers, so a test can lay the view out at a width; jsdom lays nothing out. */
const resizers: Array<{ callback: ResizeObserverCallback, watched: Element[] }> = [];

/** Lays the view out at a width, as a browser would report it, and lets it redraw. */
async function layOut(w: VueWrapper, width: number): Promise<void> {
    const view = w.get("[data-testid='channel-directory']").element as HTMLElement;
    Object.defineProperty(view, "clientWidth", { configurable: true, get: () => width });
    resizers.filter(resizer => resizer.watched.includes(view)).forEach(resizer => resizer.callback([], resizer as unknown as ResizeObserver));
    await nextTick();
    await nextTick();
}

/** The list's row buttons, in order. */
function rowButtons(w: VueWrapper): ReturnType<VueWrapper["findAll"]> {
    return w.findAll("[data-testid='directory-row-open']");
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

const originalObserver = window.IntersectionObserver;

const originalResizeObserver = window.ResizeObserver;

beforeEach(() => {
    observers.length = 0;
    window.IntersectionObserver = FakeObserver as unknown as typeof IntersectionObserver;
    resizers.length = 0;
    window.ResizeObserver = class {
        private resizer: { callback: ResizeObserverCallback, watched: Element[] };
        constructor(callback: ResizeObserverCallback) {
            this.resizer = { callback, watched: [] };
            resizers.push(this.resizer);
        }
        observe(element: Element): void {
            this.resizer.watched.push(element);
        }
        unobserve(element: Element): void {
            this.resizer.watched = this.resizer.watched.filter(one => one !== element);
        }
        disconnect(): void {
            this.resizer.watched = [];
        }
    } as unknown as typeof ResizeObserver;
});

afterEach(() => {
    wrapper?.unmount();
    wrapper = null;
    document.body.innerHTML = "";
    window.IntersectionObserver = originalObserver;
    window.ResizeObserver = originalResizeObserver;
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

    // Browse's own rule pulls Back out past the header's gutter; a scoped rule reaches the button
    // only when the button carries Browse's scope. The test build compiles no scope, so Browse is
    // given one here.
    test("places Back by its own rule, which reaches the button", () => {
        const scoped = ChannelDirectory as unknown as { __scopeId?: string };
        scoped.__scopeId = "data-v-fd";
        try {
            const back = mountDirectory({ isPhone: true }).get("[data-testid='directory-back']");

            expect(back.element.tagName).toBe("BUTTON");
            expect(back.classes()).toContain("chat-fd-back");
            expect(back.attributes()).toHaveProperty("data-v-fd");
        }
        finally {
            delete scoped.__scopeId;
        }
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

    test("shows no spinner in the search while a filter alone is asked again, with nothing typed", () => {
        const w = mountDirectory({ directory: directory({ query: "", show: "notJoined", isSearching: true }) });

        expect(w.find("[data-testid='directory-search-busy']").exists()).toBe(false);
    });

    test("shows no spinner for a search of spaces, which asks for every room", () => {
        const w = mountDirectory({ directory: directory({ query: "   ", isSearching: true }) });

        expect(w.find("[data-testid='directory-search-busy']").exists()).toBe(false);
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

// A row of campus chips ran off the edge once a church had more than three campuses; the filter is
// one button naming the campus chosen, which opens the block's own menu of every campus.
describe("the campus menu", () => {
    /** The menu, looked for in the whole document, since it may be drawn outside the view. */
    function campusMenu(): HTMLElement | null {
        return document.querySelector("[role='menu']");
    }

    function choices(): HTMLElement[] {
        return Array.from(document.querySelectorAll<HTMLElement>("[role='menu'] [role='menuitemradio']"));
    }

    async function settle(): Promise<void> {
        await nextTick();
        await nextTick();
    }

    async function press(key: string): Promise<void> {
        (document.activeElement as HTMLElement).dispatchEvent(new KeyboardEvent("keydown", { key, bubbles: true }));
        await settle();
    }

    async function openMenu(w: VueWrapper): Promise<HTMLElement> {
        const button = w.get("[data-testid='directory-campus']").element as HTMLElement;
        button.focus();
        button.click();
        await settle();
        return button;
    }

    test("is one button naming the campus chosen, All campuses by default, that opens a menu", () => {
        const w = mountDirectory();
        const button = w.get("[data-testid='directory-campus']");

        expect(button.element.tagName).toBe("BUTTON");
        expect(button.text()).toBe("All campuses");
        expect(button.attributes("aria-label")).toBe("Campus: All campuses");
        expect(button.attributes("aria-haspopup")).toBe("menu");
        expect(button.attributes("aria-expanded")).toBe("false");
        expect(button.attributes("aria-pressed")).toBeUndefined();
        expect(w.find("[data-testid='directory-chip']").exists()).toBe(false);
        expect(campusMenu()).toBeNull();
    });

    // The rest of the block says "Downtown Campus"; a short code here was a second name for it.
    test("names on its button the campus chosen, or Whole church for the rooms with no campus", () => {
        const at = mountDirectory({ directory: directory({ campus: { kind: "campus", campusId: north.guid } }) });
        expect(at.get("[data-testid='directory-campus']").text()).toBe("North Campus");
        at.unmount();

        const whole = mountDirectory({ directory: directory({ campus: { kind: "wholeChurch" } }) });
        expect(whole.get("[data-testid='directory-campus']").text()).toBe("Whole church");
    });

    test("offers every campus by its name, between All campuses and Whole church, the one chosen checked and focused", async () => {
        const w = mountDirectory({ directory: directory({ campus: { kind: "campus", campusId: north.guid } }) });
        const button = await openMenu(w);

        expect(campusMenu()?.getAttribute("aria-label")).toBe("Campus");
        expect(choices().map(choice => choice.textContent?.trim())).toEqual(["All campuses", "Downtown Campus", "North Campus", "Whole church"]);
        expect(choices().map(choice => choice.getAttribute("aria-checked"))).toEqual(["false", "false", "true", "false"]);
        expect(choices().map(choice => choice.getAttribute("data-key"))).toEqual(["all", downtown.guid, north.guid, "whole-church"]);
        expect(document.activeElement).toBe(choices()[2]);
        expect(button.getAttribute("aria-expanded")).toBe("true");
    });

    test("asks for the campus picked, closing with focus back on its button", async () => {
        const w = mountDirectory();
        const button = await openMenu(w);

        choices()[1].click();
        await settle();

        expect(w.emitted("campus")).toEqual([[{ kind: "campus", campusId: downtown.guid }]]);
        expect(campusMenu()).toBeNull();
        expect(document.activeElement).toBe(button);
    });

    test("opens from its button on Down, moves with the arrows and picks with Enter", async () => {
        const w = mountDirectory();
        const button = w.get("[data-testid='directory-campus']").element as HTMLElement;
        button.focus();

        await press("ArrowDown");
        expect(document.activeElement).toBe(choices()[0]);
        await press("ArrowDown");
        await press("ArrowDown");
        await press("ArrowDown");
        expect(document.activeElement).toBe(choices()[3]);
        await press("Enter");

        expect(w.emitted("campus")).toEqual([[{ kind: "wholeChurch" }]]);
        expect(campusMenu()).toBeNull();
        expect(document.activeElement).toBe(button);
    });

    // Browse closes on Escape too; the menu over it takes the key first, so only the menu closes.
    test("closes on Escape with focus back on its button, leaving Browse channels open", async () => {
        const w = mountDirectory();
        const button = await openMenu(w);

        await press("Escape");

        expect(campusMenu()).toBeNull();
        expect(document.activeElement).toBe(button);
        expect(button.getAttribute("aria-expanded")).toBe("false");
        expect(w.emitted("campus")).toBeUndefined();
        expect(w.emitted("close")).toBeUndefined();
    });

    test("opens on a phone as a sheet, as every menu there does", async () => {
        const w = mountDirectory({ isPhone: true });
        await openMenu(w);

        expect(campusMenu()?.closest("[data-testid='sheet']")).not.toBeNull();
        expect(choices()).toHaveLength(4);
    });

    // A long campus name at a phone's width shortens with an ellipsis rather than pushing the
    // button past the row's edge.
    test("never runs past the row: the button holds to its width and its words shorten", () => {
        const source = readFileSync(join(__dirname, "../../../../../src/Communication/Chat/ChatShell/components/channelDirectory.partial.obs"), "utf8");
        const style = source.slice(source.indexOf("<style"), source.indexOf("</style>"));

        expect(style).toMatch(/\.chat-fd-campus\s*\{[^}]*max-width:\s*100%/);
        expect(style).toMatch(/\.chat-fd-campus-words\s*\{[^}]*overflow:\s*hidden[^}]*text-overflow:\s*ellipsis/);
    });

    test("give way to the church's note when it shows each person only their campus's rooms", () => {
        const w = mountDirectory({ directory: directory({ isCampusLimited: true }) });

        expect(w.find("[data-testid='directory-campus']").exists()).toBe(false);
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

    // All and Not joined are one choice of two, so they are the segmented part: a radio group
    // whose chosen option is checked, rather than two pressed buttons.
    test("carries All and Not joined, the one chosen checked, each asking for its own set", async () => {
        const w = mountDirectory({ directory: directory({ show: "notJoined" }) });
        const option = (value: string): ReturnType<typeof w.get> => w.get(`[data-testid='directory-show-option'][data-value='${value}']`);

        const show = w.get("[data-testid='directory-show']");
        expect(show.attributes("role")).toBe("radiogroup");
        expect(show.attributes("aria-label")).toBe("Show");
        expect(option("all").text()).toBe("All");
        expect(option("all").attributes("aria-checked")).toBe("false");
        expect(option("notJoined").text()).toBe("Not joined");
        expect(option("notJoined").attributes("aria-checked")).toBe("true");

        await option("all").trigger("click");
        await w.setProps({ directory: directory({ show: "all" }) });
        await option("notJoined").trigger("click");
        expect(w.emitted("show")).toEqual([["all"], ["notJoined"]]);
    });
});

describe("a row", () => {
    // Rock's group type ("Chat Shared Channel") said nothing a person picks a channel by; how many
    // are in it does, counted as the room's header counts them.
    test("draws the room's name, its description, its campus and how many are in it, with no group type", () => {
        const w = mountDirectory();

        const row = rowOf(w, outreach.channel_id);
        expect(row.get("[data-testid='directory-row-name']").text()).toBe("Local Outreach");
        expect(row.findAll("[data-testid='directory-row-meta'] > span").map(part => part.text())).toEqual(["Downtown Campus", "1 member"]);
        expect(row.text()).not.toContain("Small Group");
        expect(rowOf(w, volunteers.channel_id).findAll("[data-testid='directory-row-meta'] > span").map(part => part.text())).toEqual(["Whole church", "1,248 members"]);
        expect(rowOf(w, volunteers.channel_id).get("[data-testid='directory-row-desc']").text()).toBe("Serve teams across the church");
    });

    test("draws its campus alone from a platform that sends no count", () => {
        const w = mountDirectory();

        expect(rowOf(w, worship.channel_id).findAll("[data-testid='directory-row-meta'] > span").map(part => part.text())).toEqual(["Whole church"]);
    });

    // A room made since the platform last counted (up to a minute) is sent with no count yet, null,
    // which says nothing about how many are in it, so it is never drawn as 0.
    test("draws its campus alone, in the row and the preview, for a new room the platform has not counted yet", () => {
        // the row type still reads a count as present or absent; null is the platform's not-yet
        const fresh = room({ channel_id: "c0000004-0000-4000-8000-000000000000", name: "Young Adults", member_count: null });
        const w = mountDirectory({ directory: directory({ rows: [fresh, volunteers] }), previewId: fresh.channel_id });

        expect(rowOf(w, fresh.channel_id).findAll("[data-testid='directory-row-meta'] > span").map(part => part.text())).toEqual(["Whole church"]);
        expect(w.get("[data-testid='directory-preview-meta']").text()).toBe("Whole church");
        expect(rowOf(w, fresh.channel_id).get("[data-testid='directory-row-open']").attributes("aria-label")).toBe("Young Adults, Whole church. Preview channel");
    });

    test("says Whole church for a room with no campus, and draws no description line without one", () => {
        const w = mountDirectory();

        expect(rowOf(w, volunteers.channel_id).findAll("[data-testid='directory-row-meta'] > span").map(part => part.text())).toEqual(["Whole church", "1,248 members"]);
        expect(rowOf(w, outreach.channel_id).find("[data-testid='directory-row-desc']").exists()).toBe(false);
    });

    // A channel with a photo showed it everywhere but here, where a plain hash stood in.
    test("draws the channel's own photo through the channel mark, and its letter tile without one, as the sidebar does", () => {
        const pictured = { ...volunteers, icon_url: "https://church.example/volunteers.jpg" };
        const w = mountDirectory({ directory: directory({ rows: [pictured, outreach] }) });

        const photo = rowOf(w, volunteers.channel_id).getComponent(ChannelMark);
        expect(photo.props("imageUrl")).toBe("https://church.example/volunteers.jpg");
        expect(photo.props("tileName")).toBe("Volunteers");
        expect(photo.get("[data-testid='channel-mark-image']").attributes("src")).toBe("https://church.example/volunteers.jpg");

        const letter = rowOf(w, outreach.channel_id).getComponent(ChannelMark);
        expect(letter.props("imageUrl")).toBeNull();
        expect(letter.get("[data-testid='channel-mark-letter']").text()).toBe("L");
    });

    test("marks the part of the name the search matched, in any letter case", () => {
        const w = mountDirectory({ directory: directory({ query: "  team ", rows: [worship], total: 1 }) });

        const name = rowOf(w, worship.channel_id).get("[data-testid='directory-row-name']");
        expect(name.text()).toBe("Worship Team");
        expect(name.get("mark").text()).toBe("Team");
        // the one shared mark, underlined, never a padded box that splits the word
        const source = readFileSync(join(__dirname, "../../../../../src/Communication/Chat/ChatShell/components/channelDirectory.partial.obs"), "utf8");
        expect(name.get("mark").classes()).toContain("chat-match");
        expect(source.slice(source.indexOf("<style"), source.indexOf("</style>"))).not.toMatch(/mark/);
    });

    test("marks nothing without a search", () => {
        const w = mountDirectory();

        expect(rowOf(w, worship.channel_id).find("mark").exists()).toBe(false);
    });

    // Looking at a channel no longer leaves Browse: the row asks for its preview beside the list.
    test("asks for the preview of the room it names, and opens nothing", async () => {
        const w = mountDirectory();

        await rowOf(w, volunteers.channel_id).get("[data-testid='directory-row-open']").trigger("click");

        expect(w.emitted("preview")).toEqual([[volunteers.channel_id]]);
        expect(w.emitted("open")).toBeUndefined();
    });

    test("tells a screen reader the room, whether it is joined, its facts and that it shows a preview", () => {
        const w = mountDirectory();

        expect(rowOf(w, volunteers.channel_id).get("[data-testid='directory-row-open']").attributes("aria-label")).toBe("Volunteers, Whole church, 1,248 members. Preview channel");
        expect(rowOf(w, worship.channel_id).get("[data-testid='directory-row-open']").attributes("aria-label")).toBe("Worship Team, joined, Whole church. Preview channel");
    });

    test("marks a room the person is in with a check, and the row previewed as the current one", () => {
        const w = mountDirectory({ previewId: volunteers.channel_id });

        expect(rowOf(w, worship.channel_id).find("[data-testid='directory-row-joined']").exists()).toBe(true);
        expect(rowOf(w, volunteers.channel_id).find("[data-testid='directory-row-joined']").exists()).toBe(false);
        expect(rowOf(w, volunteers.channel_id).get("[data-testid='directory-row-open']").attributes("aria-current")).toBe("true");
        expect(rowOf(w, worship.channel_id).get("[data-testid='directory-row-open']").attributes("aria-current")).toBeUndefined();
    });

    test("is a list of channels", () => {
        const w = mountDirectory();

        expect(w.get("[data-testid='directory-list']").element.tagName).toBe("UL");
        expect(w.get("[data-testid='directory-list']").attributes("aria-label")).toBe("Channels");
    });

    // A Browse row drew its own hover, chosen fill and rules between rows, so it looked and moved
    // unlike every other list row. The one list row draws it now: its hover, its press, its ring,
    // and the brand wash as the chosen fill.
    test("is the one list row, the previewed one chosen, with no fill or rule of its own", () => {
        const w = mountDirectory({ previewId: volunteers.channel_id });

        const chosen = rowOf(w, volunteers.channel_id).getComponent(ListRow);
        expect(chosen.element.tagName).toBe("LI");
        expect(chosen.props("interactive")).toBe(true);
        expect(chosen.props("selected")).toBe(true);
        expect(rowOf(w, worship.channel_id).getComponent(ListRow).props("selected")).toBe(false);
        expect(chosen.get("[data-testid='directory-row-open']").classes()).toContain("chat-lrow-hit");

        const source = readFileSync(join(__dirname, "../../../../../src/Communication/Chat/ChatShell/components/channelDirectory.partial.obs"), "utf8");
        const style = source.slice(source.indexOf("<style"), source.indexOf("</style>"));
        expect(style).not.toMatch(/\.chat-fd-row/);
        expect(style).not.toMatch(/\.chat-fd-open/);
    });
});

// Join, Open and Leave are the preview's footer, the join bar in its footer place for the room
// previewed; the row says only that the person is in, with its check, which pops in once.
describe("Join in the preview's footer", () => {
    function footerOf(w: VueWrapper): ReturnType<VueWrapper["getComponent"]> {
        return w.get("[data-testid='directory-preview']").getComponent(JoinBar);
    }

    test("offers Join channel for a room the person is not in, which asks the door for that room", async () => {
        const w = mountDirectory({ previewId: volunteers.channel_id });

        const footer = footerOf(w);
        expect(footer.props("place")).toBe("preview");
        expect(footer.props("name")).toBe("Volunteers");
        expect(footer.props("isMember")).toBe(false);
        await footer.get("[data-testid='join-channel']").trigger("click");

        expect(w.emitted("join")).toEqual([[volunteers.channel_id]]);
        expect(w.emitted("open")).toBeUndefined();
    });

    test("says the room previewed is joined, with Open channel, for a room the person is in, without the row's pop", () => {
        const w = mountDirectory({ previewId: worship.channel_id });

        expect(footerOf(w).props("isMember")).toBe(true);
        expect(footerOf(w).find("[data-testid='join-open']").exists()).toBe(true);
        expect(rowOf(w, worship.channel_id).get("[data-testid='directory-row-joined']").classes()).not.toContain("is-fresh");
        expect(rowOf(w, worship.channel_id).classes()).toContain("is-member");
    });

    test("hands the footer the room's Join while the door is out, which holds Join channel busy", () => {
        const joins: Record<string, JoinState> = { [volunteers.channel_id]: { kind: "joining" } };
        const w = mountDirectory({ joins, previewId: volunteers.channel_id });

        expect(footerOf(w).props("state")).toEqual({ kind: "joining" });
        expect(footerOf(w).get("[data-testid='join-channel']").attributes("aria-busy")).toBe("true");
    });

    test("asks nothing of the door while Joining is pressed again", async () => {
        const joins: Record<string, JoinState> = { [volunteers.channel_id]: { kind: "joining" } };
        const w = mountDirectory({ joins, previewId: volunteers.channel_id });

        await footerOf(w).get("[data-testid='join-channel']").trigger("click");

        expect(w.emitted("join")).toBeUndefined();
    });

    test("pops a fresh check in on the row, marked member, once the door and the platform agree", () => {
        const joins: Record<string, JoinState> = { [volunteers.channel_id]: { kind: "joined", isFresh: true } };
        const w = mountDirectory({ joins, previewId: volunteers.channel_id });

        expect(rowOf(w, volunteers.channel_id).get("[data-testid='directory-row-joined']").classes()).toContain("is-fresh");
        expect(rowOf(w, volunteers.channel_id).classes()).toContain("is-member");
        expect(footerOf(w).props("isMember")).toBe(true);
    });

    test("says Setting up in the footer while the platform catches up, with no check yet", () => {
        const joins: Record<string, JoinState> = { [volunteers.channel_id]: { kind: "pending" } };
        const w = mountDirectory({ joins, previewId: volunteers.channel_id });

        expect(footerOf(w).get("[data-testid='join-bar']").attributes("data-state")).toBe("pending");
        expect(rowOf(w, volunteers.channel_id).find("[data-testid='directory-row-joined']").exists()).toBe(false);
    });

    test("shows the door's own sentence in the footer when refused", () => {
        const joins: Record<string, JoinState> = { [volunteers.channel_id]: { kind: "refused", text: "Volunteers is full right now. Ask Nathan Hughes about joining." } };
        const w = mountDirectory({ joins, previewId: volunteers.channel_id });

        expect(footerOf(w).get("[data-testid='join-bar-text']").text()).toBe("Volunteers is full right now. Ask Nathan Hughes about joining.");
    });

    test("keeps focus in the footer, on the footer itself, when a Join turns refused", async () => {
        const joins = reactive<Record<string, JoinState>>({ [volunteers.channel_id]: { kind: "joining" } });
        const w = mountDirectory({ joins, previewId: volunteers.channel_id });
        (footerOf(w).get("[data-testid='join-channel']").element as HTMLElement).focus();

        joins[volunteers.channel_id] = { kind: "refused", text: "You can't join this channel." };
        await nextTick();
        await nextTick();

        expect(document.activeElement).toBe(footerOf(w).get("[data-testid='join-bar']").element);
    });

    test("says why in the footer when the door could not be reached, and offers Join channel again", () => {
        const joins: Record<string, JoinState> = { [volunteers.channel_id]: { kind: "failed", text: "You're offline. Try again when you're back." } };
        const w = mountDirectory({ joins, previewId: volunteers.channel_id });

        expect(footerOf(w).get("[data-testid='join-bar-text']").text()).toBe("You're offline. Try again when you're back.");
        expect(footerOf(w).get("[data-testid='join-channel']").attributes("aria-disabled")).toBeUndefined();
    });

    test("keeps focus on Join channel as it turns to Joining", async () => {
        const joins = reactive<Record<string, JoinState>>({});
        const w = mountDirectory({ joins, previewId: volunteers.channel_id });
        (footerOf(w).get("[data-testid='join-channel']").element as HTMLElement).focus();

        joins[volunteers.channel_id] = { kind: "joining" };
        await nextTick();
        await nextTick();

        expect(document.activeElement).toBe(footerOf(w).get("[data-testid='join-channel']").element);
    });

    test("opens the room previewed from Open channel, and leaves it from Leave once asked", async () => {
        const leaves: Record<string, LeaveState> = { [worship.channel_id]: { kind: "idle" } };
        const w = mountDirectory({ leaves, previewId: worship.channel_id });

        await footerOf(w).get("[data-testid='join-open']").trigger("click");
        expect(w.emitted("open")).toEqual([[worship.channel_id]]);

        await footerOf(w).get("[data-testid='join-leave']").trigger("click");
        await footerOf(w).get("[data-testid='leave-go']").trigger("click");
        expect(w.emitted("leave")).toEqual([[worship.channel_id]]);
        expect(footerOf(w).props("leave")).toEqual({ kind: "idle" });
    });
});

// The approved Browse: the list with the chosen channel's preview beside it. The preview is the
// channel's own feed, read only, under its summary, which stays pinned while only the feed
// scrolls; the feed is the shell's, drawn into the preview's slot.
describe("the preview", () => {
    test("draws the room previewed: its mark, name, campus and member count, and description, pinned over the feed", () => {
        const pictured = { ...volunteers, icon_url: "https://church.example/volunteers.jpg" };
        const w = mountDirectory({
            directory: directory({ rows: [pictured, outreach, worship] }),
            previewId: volunteers.channel_id,
            slots: { feed: "<div data-testid='the-feed'>messages</div>" }
        });

        const preview = w.get("[data-testid='directory-preview']");
        const summary = preview.get("[data-testid='directory-preview-summary']");
        expect(summary.getComponent(ChannelMark).props("imageUrl")).toBe("https://church.example/volunteers.jpg");
        expect(summary.get("[data-testid='directory-preview-name']").text()).toBe("Volunteers");
        expect(summary.findAll("[data-testid='directory-preview-meta'] > span").map(part => part.text())).toEqual(["Whole church", "1,248 members"]);
        expect(summary.get("[data-testid='directory-preview-desc']").text()).toBe("Serve teams across the church");

        // the summary stands outside the feed's own scroller, before it, so only the feed scrolls
        const feed = preview.get("[data-testid='directory-preview-feed']");
        expect(feed.get("[data-testid='the-feed']").text()).toBe("messages");
        expect(summary.element.contains(feed.element)).toBe(false);
        expect(summary.element.compareDocumentPosition(feed.element) & Node.DOCUMENT_POSITION_FOLLOWING).toBeTruthy();
        expect(preview.attributes("aria-label")).toBe("Preview of Volunteers");
    });

    test("hands its feed slot the room previewed", () => {
        const w = mountDirectory({
            previewId: outreach.channel_id,
            slots: { feed: "<template #feed=\"{ channelId }\"><b data-testid='slot-room'>{{ channelId }}</b></template>" }
        });

        expect(w.get("[data-testid='slot-room']").text()).toBe(outreach.channel_id);
    });

    test("draws no preview without a room chosen, or for a room the list no longer holds", async () => {
        const w = mountDirectory();
        expect(w.find("[data-testid='directory-preview']").exists()).toBe(false);

        await w.setProps({ previewId: "c9999999-0000-4000-8000-000000000000" });
        expect(w.find("[data-testid='directory-preview']").exists()).toBe(false);
    });

    test("swaps the preview for another row's, keeping the list, its search and its scroller", async () => {
        const w = mountDirectory({ directory: directory({ query: "e" }), previewId: volunteers.channel_id });
        const scroller = w.get("[data-testid='directory-scroll']").element;
        const search = w.get("[data-testid='directory-search']").element as HTMLInputElement;
        scrollTo(w, 120);

        await rowOf(w, outreach.channel_id).get("[data-testid='directory-row-open']").trigger("click");
        await w.setProps({ previewId: outreach.channel_id });

        expect(w.get("[data-testid='directory-preview-name']").text()).toBe("Local Outreach");
        expect(w.get("[data-testid='directory-scroll']").element).toBe(scroller);
        expect(w.get("[data-testid='directory-search']").element).toBe(search);
        expect(search.value).toBe("e");
        expect((scroller as HTMLElement).scrollTop).toBe(120);
        expect(w.findAll("[data-testid='directory-row']")).toHaveLength(3);
    });

    test("sits beside the list in a wide view, choosing the first row while none is chosen", async () => {
        const w = mountDirectory();

        await layOut(w, 1200);

        expect(w.get("[data-testid='channel-directory']").classes()).toContain("is-split");
        expect(w.emitted("preview")).toEqual([[volunteers.channel_id]]);
    });

    test("chooses the first row again in a wide view when a search or filter leaves the chosen one out", async () => {
        const w = mountDirectory({ previewId: volunteers.channel_id });
        await layOut(w, 1200);
        expect(w.emitted("preview")).toBeUndefined();

        await w.setProps({ directory: directory({ rows: [outreach, worship], total: 2 }) });
        await nextTick();

        expect(w.emitted("preview")).toEqual([[outreach.channel_id]]);
    });

    test("in a wide view, previews each row the arrows move to, the list one tab stop", async () => {
        const w = mountDirectory({ previewId: volunteers.channel_id });
        await layOut(w, 1200);
        const rows = rowButtons(w);
        expect(rows.map(row => row.attributes("tabindex"))).toEqual(["0", "-1", "-1"]);

        (rows[0].element as HTMLElement).focus();
        await rows[0].trigger("keydown", { key: "ArrowDown" });

        expect(document.activeElement).toBe(rows[1].element);
        expect(w.emitted("preview")).toEqual([[outreach.channel_id]]);
    });

    test("previews the row Enter is pressed on", async () => {
        const w = mountDirectory();
        const rows = rowButtons(w);
        (rows[1].element as HTMLElement).focus();

        await rows[1].trigger("keydown", { key: "Enter" });

        expect(w.emitted("preview")).toEqual([[outreach.channel_id]]);
    });

    test("lies over the list in a narrow view, with Back in its head, which closes it", async () => {
        const w = mountDirectory({ previewId: volunteers.channel_id });
        await layOut(w, 800);

        expect(w.get("[data-testid='channel-directory']").classes()).not.toContain("is-split");
        const preview = w.get("[data-testid='directory-preview']");
        expect(preview.classes()).toContain("is-over");
        // the list under it is out of reach while it covers it
        expect(w.get("[data-testid='directory-list-pane']").attributes("inert")).toBeDefined();
        expect(w.emitted("preview")).toBeUndefined();

        const back = preview.get("[data-testid='directory-preview-back']");
        expect(back.attributes("aria-label")).toBe("Back to Browse channels");
        await back.trigger("click");
        expect(w.emitted("preview")).toEqual([[null]]);
    });

    test("is its own screen on a phone, its head the channel's name and Back", async () => {
        const w = mountDirectory({ isPhone: true, previewId: outreach.channel_id });
        await layOut(w, 390);

        const preview = w.get("[data-testid='directory-preview']");
        expect(preview.classes()).toContain("is-over");
        expect(preview.get("[data-testid='directory-preview-head']").text()).toContain("Local Outreach");
        expect(w.emitted("preview")).toBeUndefined();
    });

    // Over the list, the list goes out of reach: focus on the row pressed moves into the preview, to
    // its Back, and Back or Escape gives it back to that row, rather than leaving it on the page.
    test.each([["in a narrow view", false, 800], ["on a phone", true, 390]])("%s, moves focus from the row pressed to the preview's Back, and back to the row as Back closes it", async (_, isPhone, width) => {
        const w = mountDirectory({ isPhone });
        await layOut(w, width as number);
        const row = rowOf(w, outreach.channel_id).get("[data-testid='directory-row-open']");
        (row.element as HTMLElement).focus();
        await row.trigger("click");
        expect(w.emitted("preview")).toEqual([[outreach.channel_id]]);

        await w.setProps({ previewId: outreach.channel_id });
        await nextTick();
        const back = w.get("[data-testid='directory-preview-back']");
        expect(document.activeElement).toBe(back.element);

        await back.trigger("click");
        expect(w.emitted("preview")).toEqual([[outreach.channel_id], [null]]);
        await w.setProps({ previewId: null });
        await nextTick();
        expect(document.activeElement).toBe(rowOf(w, outreach.channel_id).get("[data-testid='directory-row-open']").element);
    });

    test("gives focus back to the row pressed when Escape closes the preview lying over the list", async () => {
        const w = mountDirectory();
        await layOut(w, 800);
        const row = rowOf(w, worship.channel_id).get("[data-testid='directory-row-open']");
        (row.element as HTMLElement).focus();
        await row.trigger("click");
        await w.setProps({ previewId: worship.channel_id });
        await nextTick();

        await w.get("[data-testid='directory-preview-back']").trigger("keydown", { key: "Escape" });
        await w.setProps({ previewId: null });
        await nextTick();

        expect(document.activeElement).toBe(rowOf(w, worship.channel_id).get("[data-testid='directory-row-open']").element);
    });

    test("leaves focus on the row beside the preview in a wide view, where the list stays in reach", async () => {
        const w = mountDirectory();
        await layOut(w, 1200);
        const row = rowOf(w, outreach.channel_id).get("[data-testid='directory-row-open']");
        (row.element as HTMLElement).focus();
        await row.trigger("click");
        await w.setProps({ previewId: outreach.channel_id });
        await nextTick();

        expect(document.activeElement).toBe(row.element);
    });

    test("Escape in a preview lying over the list closes the preview, not the view", async () => {
        const w = mountDirectory({ previewId: volunteers.channel_id });
        await layOut(w, 800);

        await w.get("[data-testid='directory-preview-back']").trigger("keydown", { key: "Escape" });

        expect(w.emitted("preview")).toEqual([[null]]);
        expect(w.emitted("close")).toBeUndefined();
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
        const w = mountDirectory({ directory: directory({ rows: [], total: 0, error: { code: "rpc.transport", severity: "failed" } }) });

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
    // The list's end is the shared load-more row, watched against the list's own scroller.
    test("is asked as the list nears its end, while there is more, through the shared load-more row", async () => {
        const w = mountDirectory({ directory: directory({ hasMore: true }) });
        await nextTick();
        expect(w.emitted("loadMore")).toBeUndefined();

        await reachEnd();

        expect(w.emitted("loadMore")).toHaveLength(1);
        expect(w.getComponent(LoadMoreRow).props("name")).toBe("directory");
        const watching = observers.find(observer => observer.watched.length > 0);
        expect(watching?.options?.root).toBe(w.get("[data-testid='directory-scroll']").element);
    });

    // A page that lands short leaves the row in view, which an observer never reports again.
    test("asks again once a short page lands with the row still in view", async () => {
        const state = directory({ hasMore: true });
        const w = mountDirectory({ directory: state });
        await reachEnd();
        expect(w.emitted("loadMore")).toHaveLength(1);

        state.isLoadingMore = true;
        await nextTick();
        state.isLoadingMore = false;
        await nextTick();
        await nextTick();

        expect(w.emitted("loadMore")).toHaveLength(2);
    });

    test("asks nothing more after a failed page until Try again, which asks for it", async () => {
        const state = directory({ hasMore: true });
        const w = mountDirectory({ directory: state });
        await reachEnd();
        state.isLoadingMore = true;
        await nextTick();
        state.isLoadingMore = false;
        state.moreError = { code: "rpc.transport", severity: "failed" };
        await nextTick();
        await reachEnd();
        expect(w.emitted("loadMore")).toHaveLength(1);

        await w.get("[data-testid='directory-more']").trigger("click");

        expect(w.emitted("loadMore")).toHaveLength(2);
        expect(w.emitted("retry")).toBeUndefined();
    });

    test("is never asked once the list has ended", async () => {
        mountDirectory({ directory: directory({ hasMore: false }) });

        await reachEnd();

        expect(wrapper?.emitted("loadMore")).toBeUndefined();
    });

    test("shows that it is loading under the rows", () => {
        const w = mountDirectory({ directory: directory({ hasMore: true, isLoadingMore: true }) });

        expect(w.get("[data-testid='directory-more']").find(".chat-spinner").exists()).toBe(true);
    });

    test("keeps the rows and offers Try again, saying why, when it did not come", async () => {
        const w = mountDirectory({ directory: directory({ hasMore: true, moreError: { code: "rpc.transport", severity: "failed" } }) });

        expect(w.findAll("[data-testid='directory-row']")).toHaveLength(3);
        expect(w.get("[data-testid='directory-more-error']").text()).toContain("Couldn't load more channels.");
        expect(w.get("[data-testid='directory-more']").text()).toBe("Try again");
        await w.get("[data-testid='directory-more']").trigger("click");
        expect(w.emitted("loadMore")).toHaveLength(1);
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
        expect(empty.get(".chat-empty-title").text()).toBe("No channels to browse yet");
        expect(empty.text()).toContain("When Cedar Hill Church opens channels to everyone, they'll show up here.");
        expect(empty.find("button").exists()).toBe(false);
        expect(w.find("[data-testid='directory-count']").exists()).toBe(false);
    });

    test("to a search says what it looked at and offers Clear search", async () => {
        const w = mountDirectory({ directory: directory({ query: "zzz", rows: [], total: 0, emptyKind: "query" }) });

        const empty = w.get("[data-testid='directory-empty']");
        expect(empty.get(".chat-empty-title").text()).toBe("No channels match “zzz”");
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
        expect(empty.get(".chat-empty-title").text()).toBe("No channels match “zzz”");
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
        expect(empty.get(".chat-empty-title").text()).toBe("You're in every channel here");
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
        expect(empty.get(".chat-empty-title").text()).toBe("No channels for this campus yet");
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

// A search's count, a join and a refusal are the rooms' to say, through the chat's one voice, whose
// own cells hold politeness and a sentence said twice; the view says only what it draws itself.
describe("what is spoken", () => {
    test("draws no live region of its own, so its words never talk over the chat's one voice", () => {
        const w = mountDirectory({ directory: directory({ moreError: { code: "rpc.transport", severity: "failed" }, hasMore: true }) });

        expect(w.findAll("[aria-live], [role='status'], [role='alert']")).toHaveLength(0);
    });

    test("a first page that did not come is said as it is drawn", async () => {
        const state = directory({ rows: [], isLoading: true });
        mountDirectory({ directory: state });

        state.isLoading = false;
        state.error = { code: "rpc.transport", severity: "failed" };
        await nextTick();

        expect(document.querySelector("[data-testid='announcer-polite']")?.textContent).toContain("Couldn't load channels.");
    });

    test("a next page that did not come is said as it is drawn, the rows held", async () => {
        const state = directory({ hasMore: true, isLoadingMore: true });
        mountDirectory({ directory: state });

        state.isLoadingMore = false;
        state.moreError = { code: "rpc.transport", severity: "failed" };
        await nextTick();

        expect(document.querySelector("[data-testid='announcer-polite']")?.textContent).toContain("Couldn't load more channels.");
    });
});

describe("under reduced motion", () => {
    const rules = (): Array<{ selector: string, body: string }> => reducedMotionRules("../../../../../src/Communication/Chat/ChatShell/components/channelDirectory.partial.obs");

    // Reduced motion is honoured once, in the token sheet: the enter's rise is nothing there, so
    // the note only fades, as every entrance does, and the view keeps no reduced rule of its own.
    test("the church's campus note only fades in, by the one reduced-motion rule every entrance shares", () => {
        const source = readFileSync(join(__dirname, "../../../../../src/Communication/Chat/ChatShell/components/channelDirectory.partial.obs"), "utf8");
        const rule = /\n\.chat-fd-note\s*\{([^}]*)\}/.exec(source)?.[1] ?? "";

        expect(rule).toMatch(/animation:\s*chat-enter /);
        expect(rules()).toEqual([]);
    });

    // A refusal or a failure is said in the preview's footer now, which the join bar's cells hold;
    // what enters here is the preview laid over the list in a narrow view.
    test("a preview laid over the list only fades in", () => {
        // It rises by the sheet's rise, which is nothing under reduced motion, so the one
        // enter motion is a fade there and the preview keeps no reduced rule of its own.
        const source = readFileSync(join(__dirname, "../../../../../src/Communication/Chat/ChatShell/components/channelDirectory.partial.obs"), "utf8");
        const rule = /\n\.chat-fd-preview\.is-over\s*\{([^}]*)\}/.exec(source)?.[1] ?? "";

        expect(rule).not.toMatch(/--chat-rise:/);
        expect(rule).toMatch(/animation:\s*chat-enter /);
        expect(rules().some(r => /\.chat-fd-preview/.test(r.selector))).toBe(false);
    });
});
