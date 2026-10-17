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
// The emoji picker, mounted: a search over a grid of sections, Recently used when there are any,
// the church's favorites, then eight categories, with a tab per section under it and a foot naming
// the emoji in focus. One picker serves two modes: from a message it reacts, the reader's own
// reactions marked; from the composer it inserts, a Unicode emoji as its glyph and an image emoji
// as its :code:. The grid is one tab stop moved by the arrows across section edges, the tabs a
// tablist, and typing on the grid goes to the search.
//
// The component these tests are written to, components/emojiPicker.partial.obs:
//
//   props: {
//       mode: "react" | "insert",
//       source: {
//           favorites: [{ code, label, emoji, image_url }],      the bootstrap's 24, in order
//           catalog: [{ code, emoji, image_url, label, category, keywords }] | null,   null while it loads
//           recents: string[],                                   codes, newest first
//           churchName: string
//       },
//       mine: string[],                    the codes the reader reacted with; marked in react mode only
//       anchor: HTMLElement | null         what it is placed beside; what opened it unless opener is given
//       opener: HTMLElement | null         what opened it, where focus returns; its row once it has gone
//   }
//   emits: react(code), insert(text, code), close()
//
// A category's id is the catalog's category: smileys, people, symbols, nature, food, activities,
// travel, objects. The picker is drawn while mounted. It is role="dialog", "Add reaction" in react
// mode; its search is the input "Search emoji"; the grid role="grid", each section a
// role="rowgroup" named for its heading, rows of eight role="row", each emoji a role="gridcell"
// button with data-code; the tabs role="tablist" "Emoji categories", one role="tab" per section
// named as it is. The live line is data-testid="emoji-live", the empty line data-testid="emoji-empty",
// the foot data-testid="emoji-preview". A cell left out by a search is not drawn or is hidden.
import { mount, VueWrapper } from "@vue/test-utils";
import { nextTick } from "vue";
import EmojiPicker from "../../../../../src/Communication/Chat/ChatShell/components/emojiPicker.partial.obs";

// #region Fixtures

type Emoji = { code: string, emoji: string | null, image_url: string | null, label: string, category: string, keywords: string[] };

function emoji(code: string, glyph: string, label: string, category: string, keywords: string[] = []): Emoji {
    return { code, emoji: glyph, image_url: null, label, category, keywords };
}

/** The design's 24 favorites, in its order. */
const favoriteRows: Array<[string, string, string, string[]]> = [
    ["thumbsup", "👍", "thumbs up", ["like", "yes", "agree"]],
    ["heart", "❤️", "red heart", ["love"]],
    ["joy", "😂", "face with tears of joy", ["laugh"]],
    ["open_mouth", "😮", "face with open mouth", ["wow"]],
    ["cry", "😢", "crying face", ["sad"]],
    ["pray", "🙏", "folded hands", ["pray", "prayer", "please", "thanks", "amen"]],
    ["tada", "🎉", "party popper", ["celebrate"]],
    ["raised_hands", "🙌", "raising hands", ["praise"]],
    ["clap", "👏", "clapping hands", ["applause"]],
    ["blush", "😊", "smiling face with smiling eyes", ["smile"]],
    ["smiling_face_with_3_hearts", "🥰", "smiling face with hearts", ["adore"]],
    ["hugging", "🤗", "hugging face", ["hug"]],
    ["innocent", "😇", "smiling face with halo", ["angel"]],
    ["sparkles", "✨", "sparkles", ["shine"]],
    ["fire", "🔥", "fire", ["lit"]],
    ["100", "💯", "hundred points", ["perfect"]],
    ["white_check_mark", "✅", "check mark button", ["done"]],
    ["muscle", "💪", "flexed biceps", ["strong"]],
    ["notes", "🎶", "musical notes", ["music", "worship"]],
    ["book", "📖", "open book", ["bible", "scripture"]],
    ["church", "⛪", "church", ["worship", "sunday"]],
    ["dove", "🕊️", "dove", ["peace"]],
    ["coffee", "☕", "hot beverage", ["tea", "fellowship"]],
    ["thinking", "🤔", "thinking face", ["hmm"]]
];

const favorites = favoriteRows.map(([code, glyph, label]) => ({ code, label, emoji: glyph, image_url: null }));

/**
 * A small catalog, in picker order: Smileys two rows (8 and 2), People one short row of three,
 * Hearts and symbols with the red heart again and an orange one, then one or two of each other
 * category. The favorites are in it too, as the server's catalog holds every offered emoji, and
 * their keywords come from it.
 */
const smileys = [
    emoji("grinning", "😀", "grinning face", "smileys", ["happy"]),
    emoji("smiley", "😃", "grinning face with big eyes", "smileys"),
    emoji("smile", "😄", "grinning face with smiling eyes", "smileys"),
    emoji("grin", "😁", "beaming face", "smileys"),
    emoji("laughing", "😆", "grinning squinting face", "smileys"),
    emoji("sweat_smile", "😅", "grinning face with sweat", "smileys"),
    emoji("rofl", "🤣", "rolling on the floor laughing", "smileys"),
    emoji("slight_smile", "🙂", "slightly smiling face", "smileys"),
    emoji("upside_down", "🙃", "upside down face", "smileys"),
    emoji("wink", "😉", "winking face", "smileys")
];
const people = [
    emoji("wave", "👋", "waving hand", "people"),
    emoji("ok_hand", "👌", "ok hand", "people"),
    emoji("v", "✌️", "victory hand", "people")
];
const symbols = [
    emoji("heart", "❤️", "red heart", "symbols", ["love"]),
    emoji("orange_heart", "🧡", "orange heart", "symbols", ["love"])
];
const rest = [
    emoji("sunflower", "🌻", "sunflower", "nature"),
    emoji("apple", "🍎", "red apple", "food"),
    emoji("balloon", "🎈", "balloon", "activities"),
    emoji("airplane", "✈️", "airplane", "travel"),
    emoji("bulb", "💡", "light bulb", "objects"),
    { code: "cedar_hill", emoji: null, image_url: "https://cdn.example.org/cedar.png", label: "cedar hill", category: "objects", keywords: ["church"] }
];
/** The favorites other than the red heart, kept last under Objects so the sections above keep the rows the keyboard cells count on. */
const favoritesInCatalog = favoriteRows
    .filter(([code]) => code !== "heart")
    .map(([code, glyph, label, keywords]) => emoji(code, glyph, label, "objects", keywords));

const catalog: Emoji[] = [...smileys, ...people, ...symbols, ...rest, ...favoritesInCatalog];

const categoryLabels = ["Smileys", "People and gestures", "Hearts and symbols", "Nature", "Food and drink", "Activities and celebrations", "Travel and places", "Objects"];

// #endregion Fixtures

// #region Helpers

let wrapper: VueWrapper | null = null;
let anchor: HTMLButtonElement;

type PickerProps = { mode?: "react" | "insert", recents?: string[], catalog?: Emoji[] | null, mine?: string[] };

async function settle(): Promise<void> {
    for (let i = 0; i < 3; i++) {
        await Promise.resolve();
        await nextTick();
    }
}

async function open(props: PickerProps = {}): Promise<VueWrapper> {
    anchor = document.createElement("button");
    anchor.textContent = "Add reaction";
    document.body.append(anchor);
    anchor.focus();

    wrapper = mount(EmojiPicker, {
        props: {
            mode: props.mode ?? "react",
            source: { favorites, catalog: props.catalog === undefined ? catalog : props.catalog, recents: props.recents ?? [], churchName: "Cedar Hill" },
            mine: props.mine ?? [],
            anchor
        },
        attachTo: document.body
    });
    await settle();
    return wrapper;
}

function search(): HTMLInputElement {
    const input = document.querySelector<HTMLInputElement>("input[aria-label='Search emoji']");
    if (!input) {
        throw new Error("No search");
    }
    return input;
}

async function typeSearch(text: string): Promise<void> {
    const input = search();
    input.value = text;
    input.dispatchEvent(new Event("input", { bubbles: true }));
    await settle();
}

function isShown(element: Element): boolean {
    return !element.closest("[hidden]");
}

function sectionLabels(): string[] {
    return Array.from(document.querySelectorAll("[role='grid'] [role='rowgroup']")).filter(isShown).map(s => s.getAttribute("aria-label") ?? "");
}

function section(label: string): Element {
    const found = Array.from(document.querySelectorAll("[role='grid'] [role='rowgroup']")).find(s => s.getAttribute("aria-label") === label);
    if (!found) {
        throw new Error(`No section ${label}`);
    }
    return found;
}

function cellsOf(container: ParentNode): HTMLElement[] {
    return Array.from(container.querySelectorAll<HTMLElement>("[role='gridcell']")).filter(isShown);
}

function cells(): HTMLElement[] {
    return cellsOf(document.querySelector("[role='grid']") as Element);
}

function codesOf(list: HTMLElement[]): string[] {
    return list.map(c => c.getAttribute("data-code") ?? "");
}

/** A section's cell by its row and column, rows of eight. */
function cellAt(label: string, row: number, column: number): HTMLElement {
    const rows = Array.from(section(label).querySelectorAll("[role='row']"));
    const cell = cellsOf(rows[row])[column];
    if (!cell) {
        throw new Error(`No cell ${label} ${row} ${column}`);
    }
    return cell;
}

async function press(target: HTMLElement, key: string, extra: KeyboardEventInit = {}): Promise<KeyboardEvent> {
    const event = new KeyboardEvent("keydown", { key, bubbles: true, cancelable: true, ...extra });
    target.dispatchEvent(event);
    await settle();
    return event;
}

function tabs(): HTMLElement[] {
    return Array.from(document.querySelectorAll<HTMLElement>("[role='tablist'] [role='tab']"));
}

beforeAll(() => {
    // jsdom has no ResizeObserver, and floating placement keeps the popover beside its anchor with one.
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
    document.body.replaceChildren();
    jest.useRealTimers();
});

// #endregion Helpers

describe("the sections", () => {
    test("are the church's favorites, then the eight categories, in order, with no Recently used before any", async () => {
        await open();

        expect(sectionLabels()).toEqual(["Cedar Hill favorites", ...categoryLabels]);
    });

    test("Recently used comes first once there are any, drawn newest first", async () => {
        await open({ recents: ["fire", "grinning"] });

        expect(sectionLabels()[0]).toBe("Recently used");
        expect(codesOf(cellsOf(section("Recently used")))).toEqual(["fire", "grinning"]);
    });

    test("the favorites are the 24 in the church's order, three rows of eight", async () => {
        await open();

        expect(codesOf(cellsOf(section("Cedar Hill favorites")))).toEqual(favorites.map(f => f.code));
        expect(section("Cedar Hill favorites").querySelectorAll("[role='row']")).toHaveLength(3);
    });

    test("a category holds the catalog's emoji of that category, in the catalog's order", async () => {
        await open();

        expect(codesOf(cellsOf(section("Smileys")))).toEqual(smileys.map(e => e.code));
        expect(codesOf(cellsOf(section("Hearts and symbols")))).toEqual(["heart", "orange_heart"]);
    });

    test("while the catalog loads, the favorites are drawn at once", async () => {
        await open({ catalog: null });

        expect(sectionLabels()[0]).toBe("Cedar Hill favorites");
        expect(cellsOf(section("Cedar Hill favorites"))).toHaveLength(24);
    });

    test("is a dialog named Add reaction, its grid a grid, its search focused on open", async () => {
        await open();

        expect(document.querySelector("[role='dialog']")?.getAttribute("aria-label")).toBe("Add reaction");
        expect(document.querySelector("[role='grid']")).not.toBeNull();
        expect(document.activeElement).toBe(search());
    });
});

describe("the search", () => {
    test("matches names, each glyph once though it sits in two sections, and says how many", async () => {
        await open();

        await typeSearch("heart");

        expect(codesOf(cells())).toEqual(["heart", "smiling_face_with_3_hearts", "orange_heart"]);
        expect(document.querySelector("[data-testid='emoji-live']")?.textContent?.trim()).toBe("3 results");
    });

    test("matches keywords", async () => {
        await open();

        await typeSearch("pray");

        expect(codesOf(cells())).toEqual(["pray"]);
        expect(document.querySelector("[data-testid='emoji-live']")?.textContent?.trim()).toBe("1 result");
    });

    test("Enter takes the first result", async () => {
        const w = await open();
        await typeSearch("pray");

        await press(search(), "Enter");

        expect(w.emitted("react")).toEqual([["pray"]]);
    });

    test("with no match says so, in the grid and on the live line, and Enter takes nothing", async () => {
        const w = await open();

        await typeSearch("zzz");

        expect(cells()).toHaveLength(0);
        expect(document.querySelector("[data-testid='emoji-empty']")?.textContent?.trim()).toMatch(/^No emoji match ["“]zzz["”]$/);
        expect(document.querySelector("[data-testid='emoji-live']")?.textContent?.trim()).toBe("No emoji match");

        await press(search(), "Enter");
        expect(w.emitted("react")).toBeUndefined();
    });

    test("hides the tabs while searching, and brings every section back once cleared", async () => {
        await open();

        await typeSearch("heart");
        expect(tabs().filter(isShown)).toHaveLength(0);

        await typeSearch("");
        expect(tabs().filter(isShown).length).toBeGreaterThan(0);
        expect(sectionLabels()).toEqual(["Cedar Hill favorites", ...categoryLabels]);
    });
});

describe("the grid's keyboard", () => {
    test("is one tab stop", async () => {
        await open();

        expect(cells().filter(c => c.getAttribute("tabindex") === "0")).toHaveLength(1);
    });

    test("Down or Tab from the search enters the grid", async () => {
        await open();

        await press(search(), "ArrowDown");
        expect(document.activeElement).toBe(cellAt("Cedar Hill favorites", 0, 0));

        search().focus();
        const tab = await press(search(), "Tab");
        expect(tab.defaultPrevented).toBe(true);
        expect(document.activeElement).toBe(cellAt("Cedar Hill favorites", 0, 0));
    });

    test("Right and Left cross from one section into the next and back", async () => {
        await open();
        const lastFavorite = cellAt("Cedar Hill favorites", 2, 7);
        lastFavorite.focus();

        await press(lastFavorite, "ArrowRight");
        expect(document.activeElement).toBe(cellAt("Smileys", 0, 0));

        await press(document.activeElement as HTMLElement, "ArrowLeft");
        expect(document.activeElement).toBe(lastFavorite);
    });

    test("Down keeps the column across a section's edge, and lands on a short row's last cell", async () => {
        await open();
        const start = cellAt("Cedar Hill favorites", 2, 5);
        start.focus();

        await press(start, "ArrowDown");
        expect(document.activeElement).toBe(cellAt("Smileys", 0, 5));

        await press(document.activeElement as HTMLElement, "ArrowDown");
        expect(document.activeElement).toBe(cellAt("Smileys", 1, 1));

        await press(document.activeElement as HTMLElement, "ArrowDown");
        expect(document.activeElement).toBe(cellAt("People and gestures", 0, 1));
    });

    test("Up crosses back into the section above, and from the top row returns to the search", async () => {
        await open();
        const start = cellAt("People and gestures", 0, 2);
        start.focus();

        await press(start, "ArrowUp");
        expect(document.activeElement).toBe(cellAt("Smileys", 1, 1));

        const top = cellAt("Cedar Hill favorites", 0, 3);
        top.focus();
        await press(top, "ArrowUp");
        expect(document.activeElement).toBe(search());
    });

    test("Home and End go to the grid's first and last emoji", async () => {
        await open();
        const middle = cellAt("Smileys", 0, 3);
        middle.focus();

        await press(middle, "End");
        expect(document.activeElement).toBe(cells()[cells().length - 1]);

        await press(document.activeElement as HTMLElement, "Home");
        expect(document.activeElement).toBe(cellAt("Cedar Hill favorites", 0, 0));
    });

    test("Page Down and Page Up move by section, to its first emoji", async () => {
        await open();
        const start = cellAt("Cedar Hill favorites", 1, 4);
        start.focus();

        await press(start, "PageDown");
        expect(document.activeElement).toBe(cellAt("Smileys", 0, 0));

        await press(document.activeElement as HTMLElement, "PageDown");
        expect(document.activeElement).toBe(cellAt("People and gestures", 0, 0));

        await press(document.activeElement as HTMLElement, "PageUp");
        expect(document.activeElement).toBe(cellAt("Smileys", 0, 0));
    });

    test("the arrows carry the one tab stop with them", async () => {
        await open();
        const start = cellAt("Cedar Hill favorites", 0, 0);
        start.focus();

        await press(start, "ArrowRight");

        const stops = cells().filter(c => c.getAttribute("tabindex") === "0");
        expect(stops).toEqual([cellAt("Cedar Hill favorites", 0, 1)]);
    });

    test("typing a letter on the grid goes to the search", async () => {
        await open();
        const start = cellAt("Smileys", 0, 2);
        start.focus();

        await press(start, "p");

        expect(document.activeElement).toBe(search());
    });
});

describe("the tabs", () => {
    test("are a tablist, one per section, the favorites selected at first", async () => {
        await open({ recents: ["fire"] });

        expect(document.querySelector("[role='tablist']")?.getAttribute("aria-label")).toBe("Emoji categories");
        expect(tabs().map(t => t.getAttribute("aria-label"))).toEqual(["Recently used", "Cedar Hill favorites", ...categoryLabels]);
        expect(tabs().map(t => t.getAttribute("aria-selected"))).toEqual(["false", "true", ...categoryLabels.map(() => "false")]);
    });

    test("a tab chosen is selected", async () => {
        await open();
        const nature = tabs().find(t => t.getAttribute("aria-label") === "Nature") as HTMLElement;

        nature.click();
        await settle();

        expect(nature.getAttribute("aria-selected")).toBe("true");
        expect(tabs().filter(t => t.getAttribute("aria-selected") === "true")).toEqual([nature]);
    });

    test("Right, Left, Home and End move between the tabs", async () => {
        await open();
        const all = tabs();
        all[0].focus();

        await press(all[0], "ArrowRight");
        expect(document.activeElement).toBe(all[1]);

        await press(all[1], "ArrowLeft");
        expect(document.activeElement).toBe(all[0]);

        await press(all[0], "End");
        expect(document.activeElement).toBe(all[all.length - 1]);

        await press(all[all.length - 1], "Home");
        expect(document.activeElement).toBe(all[0]);
    });

    // jsdom lays nothing out, so each section is given a place 100px below the one before, in a
    // body 200px high, as a browser would measure them.
    function layOut(): HTMLElement {
        const body = document.querySelector<HTMLElement>("[role='grid']") as HTMLElement;
        Array.from(body.querySelectorAll<HTMLElement>("[data-sec]")).forEach((section, index) => {
            Object.defineProperty(section, "offsetTop", { configurable: true, get: () => index * 100 });
        });
        Object.defineProperty(body, "clientHeight", { configurable: true, value: 200 });
        Object.defineProperty(body, "scrollHeight", { configurable: true, value: 2000 });
        return body;
    }

    async function scrollBody(body: HTMLElement, top: number): Promise<void> {
        body.scrollTop = top;
        body.dispatchEvent(new Event("scroll"));
        await settle();
    }

    function selectedTab(): string | null {
        return tabs().find(t => t.getAttribute("aria-selected") === "true")?.getAttribute("aria-label") ?? null;
    }

    test("follow the scroll: the section at the top, and the last at the end", async () => {
        await open();
        const body = layOut();

        await scrollBody(body, 300);
        expect(selectedTab()).toBe("Hearts and symbols");

        await scrollBody(body, 108);
        expect(selectedTab()).toBe("Smileys");

        await scrollBody(body, 1800);
        expect(selectedTab()).toBe("Objects");

        await scrollBody(body, 0);
        expect(selectedTab()).toBe("Cedar Hill favorites");
    });

    test("a tab chosen stays selected while its smooth scroll passes the sections between, then the tabs follow again", async () => {
        jest.useFakeTimers();
        await open();
        const body = layOut();
        body.scrollTo = jest.fn() as unknown as typeof body.scrollTo;
        const nature = tabs().find(t => t.getAttribute("aria-label") === "Nature") as HTMLElement;

        nature.click();
        await settle();
        await scrollBody(body, 100);
        await scrollBody(body, 300);
        expect(selectedTab()).toBe("Nature");

        // The reader takes the scroll over before it arrives; once the hold is up the tabs follow them.
        jest.advanceTimersByTime(450);
        await scrollBody(body, 100);
        expect(selectedTab()).toBe("Smileys");
        jest.useRealTimers();
    });

    test("once a search is cleared, select the section the grid is back at", async () => {
        await open();
        const body = layOut();
        await scrollBody(body, 300);
        expect(selectedTab()).toBe("Hearts and symbols");

        await typeSearch("c");
        await typeSearch("");

        expect(body.scrollTop).toBe(0);
        expect(selectedTab()).toBe("Cedar Hill favorites");
    });
});

describe("where it is placed", () => {
    /** Opens the picker beside an anchor at a given box, in a window 1000 by 800. */
    async function openAt(box: { left: number, top: number, width: number, height: number }, isOwn = false): Promise<HTMLElement> {
        Object.defineProperty(document.documentElement, "clientWidth", { configurable: true, value: 1000 });
        Object.defineProperty(document.documentElement, "clientHeight", { configurable: true, value: 800 });
        const row = document.createElement("div");
        row.className = isOwn ? "chat-msg is-conversational is-own" : "chat-msg is-conversational";
        anchor = document.createElement("button");
        anchor.getBoundingClientRect = () => ({ ...box, x: box.left, y: box.top, right: box.left + box.width, bottom: box.top + box.height, toJSON: () => box }) as DOMRect;
        row.append(anchor);
        document.body.append(row);

        const sized = jest.spyOn(HTMLElement.prototype, "offsetWidth", "get").mockImplementation(function (this: HTMLElement) {
            return this.classList.contains("chat-picker") ? 328 : 0;
        });
        const tall = jest.spyOn(HTMLElement.prototype, "offsetHeight", "get").mockImplementation(function (this: HTMLElement) {
            return this.classList.contains("chat-picker") ? 380 : 0;
        });

        wrapper = mount(EmojiPicker, {
            props: { mode: "react", source: { favorites, catalog, recents: [], churchName: "Cedar Hill" }, mine: [], anchor },
            attachTo: document.body
        });
        await settle();
        await new Promise(resolve => setTimeout(resolve, 0));
        await settle();
        sized.mockRestore();
        tall.mockRestore();

        return document.querySelector<HTMLElement>(".chat-picker") as HTMLElement;
    }

    test("starts at another's message's anchor, and with no room there keeps that alignment and shifts inside the window", async () => {
        const picker = await openAt({ left: 900, top: 500, width: 28, height: 28 });

        expect(picker.style.left).toBe(`${1000 - 328 - 8}px`);
        expect(picker.style.top).toBe(`${500 - 8 - 380}px`);
    });

    test("ends at the reader's own message's anchor, and with no room there keeps that alignment and shifts inside the window", async () => {
        const picker = await openAt({ left: 20, top: 500, width: 28, height: 28 }, true);

        expect(picker.style.left).toBe("8px");
    });

    test("with room, starts at the anchor's left edge", async () => {
        const picker = await openAt({ left: 300, top: 500, width: 28, height: 28 });

        expect(picker.style.left).toBe("300px");
    });
});

describe("closing", () => {
    test("closes once what opened it leaves the page, as when its row or drawer goes", async () => {
        const w = await open();

        anchor.remove();
        await settle();

        expect(w.emitted("close")).toHaveLength(1);
    });

    // Opened from a message's add chip, the picker is placed on the message's bubble, since a
    // choice that takes the message's last chip away takes the add chip with it.
    test("placed on a message's bubble and opened from its add chip, stays when the add chip goes, and gives focus to the row once it has", async () => {
        const row = document.createElement("div");
        row.className = "chat-msg is-conversational";
        row.tabIndex = -1;
        const bubble = document.createElement("div");
        bubble.className = "chat-bubble";
        const add = document.createElement("button");
        add.textContent = "Add reaction";
        row.append(bubble, add);
        document.body.append(row);
        add.focus();

        const w = mount(EmojiPicker, {
            props: { mode: "react", source: { favorites, catalog, recents: [], churchName: "Cedar Hill" }, mine: [], anchor: bubble, opener: add },
            attachTo: document.body
        });
        wrapper = w;
        await settle();

        add.remove();
        await settle();
        expect(w.emitted("close")).toBeUndefined();

        await press(search(), "Escape");

        expect(w.emitted("close")).toHaveLength(1);
        expect(document.activeElement).toBe(row);
    });

    test("a choice that takes its add chip away as the page redraws gives focus to the message's row", async () => {
        const row = document.createElement("div");
        row.className = "chat-msg is-conversational";
        row.tabIndex = -1;
        const bubble = document.createElement("div");
        bubble.className = "chat-bubble";
        const add = document.createElement("button");
        add.textContent = "Add reaction";
        row.append(bubble, add);
        document.body.append(row);
        add.focus();

        const w = mount(EmojiPicker, {
            // The toggle takes the last chip back at once, and the row is redrawn without it, add
            // chip and all, before the next tick.
            props: { mode: "react", source: { favorites, catalog, recents: [], churchName: "Cedar Hill" }, mine: ["grinning"], anchor: bubble, opener: add, onReact: () => queueMicrotask(() => add.remove()) },
            attachTo: document.body
        });
        wrapper = w;
        await settle();

        cellAt("Smileys", 0, 0).click();
        await settle();

        expect(w.emitted("react")).toEqual([["grinning"]]);
        expect(document.activeElement).toBe(row);
    });

    test("does not wait on a way out that a hidden tab never plays, closing after its length", async () => {
        // jsdom plays no animations; this one never finishes, as in a tab the browser has hidden.
        const prototype = HTMLElement.prototype as unknown as { getAnimations?: () => unknown[] };
        prototype.getAnimations = () => [{ finished: new Promise(() => undefined) }];
        try {
            const w = await open();

            await press(search(), "Escape");
            expect(w.emitted("close")).toBeUndefined();

            await new Promise(resolve => setTimeout(resolve, 200));
            await settle();

            expect(w.emitted("close")).toHaveLength(1);
        }
        finally {
            delete prototype.getAnimations;
        }
    });
});

describe("choosing in react mode", () => {
    test("asks to react with the emoji's code, closes, and gives focus back to what opened it", async () => {
        const w = await open();

        cellAt("Smileys", 0, 0).click();
        await settle();

        expect(w.emitted("react")).toEqual([["grinning"]]);
        expect(w.emitted("close")).toHaveLength(1);
        expect(document.activeElement).toBe(anchor);
    });

    test("marks the reader's own reactions, in the cell's name and the foot", async () => {
        await open({ mine: ["pray"] });

        const pray = cellAt("Cedar Hill favorites", 0, 5);
        expect(pray.getAttribute("aria-pressed")).toBe("true");
        expect(pray.getAttribute("aria-label")).toBe("Folded hands, you reacted");
        expect(cellAt("Cedar Hill favorites", 0, 0).getAttribute("aria-pressed")).not.toBe("true");

        pray.focus();
        pray.dispatchEvent(new FocusEvent("focusin", { bubbles: true }));
        await settle();

        const foot = document.querySelector("[data-testid='emoji-preview']")?.textContent ?? "";
        expect(foot).toContain("Folded hands");
        expect(foot).toContain("(yours)");
        expect(foot).toContain(":pray:");
    });

    test("Escape closes and gives focus back", async () => {
        const w = await open();

        await press(search(), "Escape");

        expect(w.emitted("close")).toHaveLength(1);
        expect(document.activeElement).toBe(anchor);
    });
});

describe("choosing in insert mode", () => {
    test("asks to insert a Unicode emoji as its glyph", async () => {
        const w = await open({ mode: "insert" });

        cellAt("Cedar Hill favorites", 0, 5).click();
        await settle();

        expect(w.emitted("insert")).toEqual([["🙏", "pray"]]);
        expect(w.emitted("react")).toBeUndefined();
        expect(w.emitted("close")).toHaveLength(1);
    });

    test("asks to insert an image emoji as its code, which the message draws", async () => {
        const w = await open({ mode: "insert" });

        const custom = cellsOf(section("Objects")).find(c => c.getAttribute("data-code") === "cedar_hill") as HTMLElement;
        custom.click();
        await settle();

        expect(w.emitted("insert")).toEqual([[":cedar_hill:", "cedar_hill"]]);
    });

    test("marks nothing as the reader's", async () => {
        await open({ mode: "insert", mine: ["pray"] });

        expect(cells().filter(c => c.getAttribute("aria-pressed") === "true")).toHaveLength(0);
    });
});
