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
// The emoji picker, mounted. Reacting opens on a quick tray of eight glyphs, the reader's recents
// then the church's favorites, with the full picker one press away; the composer's insert opens
// straight on the full picker. The full picker is a search, a tab per section under it, and one
// section drawn at a time, Recently used when there are any, the church's favorites first, then
// eight categories, with a foot naming the emoji in focus. Only what is on screen is in the page:
// eight cells for the tray, one section's cells for the full picker, at most ten rows for a search.
// From a message it reacts, the reader's own reactions marked; from the composer it inserts, a
// Unicode emoji as its glyph and an image emoji as its :code:. The grid is one tab stop moved by
// the arrows, the tabs a tablist, the tray a toolbar, and typing on the grid goes to the search.
//
// The component these tests are written to, components/emojiPicker.partial.obs:
//
//   props: {
//       mode: "react" | "insert",
//       startsOn: "tray" | "full",          where it opens; "full" by default, the tray in react mode only
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
// mode. The tray is role="toolbar" "Quick reactions", each glyph a button with data-code, then the
// button "All emoji". The full picker's search is the input "Search emoji"; the tabs role="tablist"
// "Emoji categories", one role="tab" per section named as it is, above the grid; the grid
// role="grid", its one section a role="rowgroup" named for its heading, rows of eight role="row",
// each emoji a role="gridcell" button with data-code. The live line is data-testid="emoji-live",
// the empty line data-testid="emoji-empty", a search cut short data-testid="emoji-more", the foot
// data-testid="emoji-preview".
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

/** A catalog the size the church offers, 439 emoji, its largest category 77, every one named "sample face". */
const fullSizes: Array<[string, number]> = [["smileys", 77], ["people", 70], ["symbols", 50], ["nature", 60], ["food", 50], ["activities", 40], ["travel", 50], ["objects", 42]];
const fullCatalog: Emoji[] = fullSizes.flatMap(([category, size]) => Array.from({ length: size }, (_, i) => emoji(`${category}_${i}`, "😀", `sample face ${category} ${i}`, category)));

// #endregion Fixtures

// #region Helpers

let wrapper: VueWrapper | null = null;
let anchor: HTMLButtonElement;

type PickerProps = { mode?: "react" | "insert", startsOn?: "tray" | "full", recents?: string[], catalog?: Emoji[] | null, mine?: string[] };

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
            ...(props.startsOn ? { startsOn: props.startsOn } : {}),
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

/** Every emoji button in the page, tray or grid, which is what the browser has to lay out and paint. */
function drawnGlyphs(): number {
    return document.querySelectorAll("[role='dialog'] button[data-code]").length;
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

function tab(label: string): HTMLElement {
    const found = tabs().find(t => t.getAttribute("aria-label") === label);
    if (!found) {
        throw new Error(`No tab ${label}`);
    }
    return found;
}

async function showSection(label: string): Promise<void> {
    tab(label).click();
    await settle();
}

function selectedTab(): string | null {
    return tabs().find(t => t.getAttribute("aria-selected") === "true")?.getAttribute("aria-label") ?? null;
}

function trayGlyphs(): HTMLElement[] {
    return Array.from(document.querySelectorAll<HTMLElement>("[role='toolbar'][aria-label='Quick reactions'] button[data-code]"));
}

function allEmoji(): HTMLElement {
    const button = document.querySelector<HTMLElement>("[role='toolbar'][aria-label='Quick reactions'] button[aria-label='All emoji']");
    if (!button) {
        throw new Error("No All emoji");
    }
    return button;
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

describe("the quick tray", () => {
    test("draws eight glyphs, the recents newest first then the favorites, each once, and nothing else of the picker", async () => {
        await open({ startsOn: "tray", recents: ["fire", "grinning", "thumbsup"] });

        expect(codesOf(trayGlyphs())).toEqual(["fire", "grinning", "thumbsup", "heart", "joy", "open_mouth", "cry", "pray"]);
        expect(drawnGlyphs()).toBe(8);
        expect(document.querySelector("[role='grid']")).toBeNull();
        expect(document.querySelector("input[aria-label='Search emoji']")).toBeNull();
    });

    test("with no recents is the first eight favorites, and leaves out a recent it no longer knows", async () => {
        await open({ startsOn: "tray", recents: ["retired_code"] });

        expect(codesOf(trayGlyphs())).toEqual(favorites.slice(0, 8).map(f => f.code));
    });

    test("draws a recent from the catalog by its glyph, and a recent the catalog has not brought yet is left out", async () => {
        await open({ startsOn: "tray", recents: ["wave", "pray"], catalog: null });

        expect(codesOf(trayGlyphs())).toEqual(["pray", "thumbsup", "heart", "joy", "open_mouth", "cry", "tada", "raised_hands"]);
    });

    test("is a toolbar in a dialog named Add reaction, its first glyph focused on open and the one tab stop", async () => {
        await open({ startsOn: "tray" });

        expect(document.querySelector("[role='dialog']")?.getAttribute("aria-label")).toBe("Add reaction");
        expect(document.activeElement).toBe(trayGlyphs()[0]);
        const stops = Array.from(document.querySelectorAll("[role='toolbar'] button")).filter(b => b.getAttribute("tabindex") === "0");
        expect(stops).toEqual([trayGlyphs()[0]]);
    });

    test("Right, Left, Home and End move along it, All emoji last", async () => {
        await open({ startsOn: "tray" });
        const glyphs = trayGlyphs();

        await press(glyphs[0], "ArrowRight");
        expect(document.activeElement).toBe(glyphs[1]);

        await press(glyphs[1], "ArrowLeft");
        expect(document.activeElement).toBe(glyphs[0]);

        await press(glyphs[0], "End");
        expect(document.activeElement).toBe(allEmoji());
        expect(allEmoji().getAttribute("tabindex")).toBe("0");

        await press(allEmoji(), "Home");
        expect(document.activeElement).toBe(glyphs[0]);
    });

    test("names each glyph and marks the reader's own", async () => {
        await open({ startsOn: "tray", mine: ["heart"] });

        expect(trayGlyphs()[0].getAttribute("aria-label")).toBe("Thumbs up");
        expect(trayGlyphs()[1].getAttribute("aria-label")).toBe("Red heart, you reacted");
        expect(trayGlyphs()[1].getAttribute("aria-pressed")).toBe("true");
        expect(trayGlyphs()[0].getAttribute("aria-pressed")).not.toBe("true");
    });

    test("a glyph reacts with its code, closes, and gives focus back to what opened it", async () => {
        const w = await open({ startsOn: "tray" });

        trayGlyphs()[5].click();
        await settle();

        expect(w.emitted("react")).toEqual([["pray"]]);
        expect(w.emitted("close")).toHaveLength(1);
        expect(document.activeElement).toBe(anchor);
    });

    test("Escape closes and gives focus back", async () => {
        const w = await open({ startsOn: "tray" });

        await press(trayGlyphs()[0], "Escape");

        expect(w.emitted("close")).toHaveLength(1);
        expect(document.activeElement).toBe(anchor);
    });

    test("All emoji opens the full picker in its place, on the favorites, the search focused, nothing chosen or closed", async () => {
        const w = await open({ startsOn: "tray" });

        allEmoji().click();
        await settle();

        expect(trayGlyphs()).toHaveLength(0);
        expect(sectionLabels()).toEqual(["Cedar Hill favorites"]);
        expect(drawnGlyphs()).toBe(24);
        expect(document.activeElement).toBe(search());
        expect(document.querySelector("[role='dialog']")?.getAttribute("aria-label")).toBe("Add reaction");
        expect(w.emitted("react")).toBeUndefined();
        expect(w.emitted("close")).toBeUndefined();
    });

    test("is never drawn in insert mode, which opens on the full picker", async () => {
        await open({ mode: "insert", startsOn: "tray" });

        expect(trayGlyphs()).toHaveLength(0);
        expect(sectionLabels()).toEqual(["Cedar Hill favorites"]);
    });
});

describe("the full picker's sections", () => {
    test("opens on the church's favorites alone, the 24 in order, three rows of eight", async () => {
        await open();

        expect(sectionLabels()).toEqual(["Cedar Hill favorites"]);
        expect(codesOf(cellsOf(section("Cedar Hill favorites")))).toEqual(favorites.map(f => f.code));
        expect(section("Cedar Hill favorites").querySelectorAll("[role='row']")).toHaveLength(3);
    });

    test("draws one section at a time, never the whole catalog: 24 cells on open, a category's own on its tab", async () => {
        await open({ catalog: fullCatalog });
        expect(drawnGlyphs()).toBe(24);

        for (const [index, [, size]] of fullSizes.entries()) {
            await showSection(categoryLabels[index]);
            expect(sectionLabels()).toEqual([categoryLabels[index]]);
            expect(drawnGlyphs()).toBe(size);
        }
    });

    test("a category holds the catalog's emoji of that category, in the catalog's order", async () => {
        await open();

        await showSection("Smileys");
        expect(codesOf(cells())).toEqual(smileys.map(e => e.code));

        await showSection("Hearts and symbols");
        expect(codesOf(cells())).toEqual(["heart", "orange_heart"]);
    });

    test("Recently used is drawn newest first on its tab, which comes first", async () => {
        await open({ recents: ["fire", "grinning"] });

        await showSection("Recently used");

        expect(sectionLabels()).toEqual(["Recently used"]);
        expect(codesOf(cells())).toEqual(["fire", "grinning"]);
    });

    test("while the catalog loads, the favorites are drawn at once and a category holds its place until it comes", async () => {
        const w = await open({ catalog: null });
        expect(cellsOf(section("Cedar Hill favorites"))).toHaveLength(24);

        await showSection("Nature");
        expect(cells()).toHaveLength(0);
        expect(document.querySelector("[role='grid']")?.getAttribute("aria-busy")).toBe("true");

        await w.setProps({ source: { favorites, catalog, recents: [], churchName: "Cedar Hill" } });
        await settle();
        expect(sectionLabels()).toEqual(["Nature"]);
        expect(codesOf(cells())).toEqual(["sunflower"]);
    });

    test("is a dialog named Add reaction, its grid a grid, its search focused on open", async () => {
        await open();

        expect(document.querySelector("[role='dialog']")?.getAttribute("aria-label")).toBe("Add reaction");
        expect(document.querySelector("[role='grid']")).not.toBeNull();
        expect(document.activeElement).toBe(search());
    });
});

describe("the search", () => {
    test("matches names across every section, each glyph once though it sits in two, and says how many", async () => {
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

    test("draws at most ten rows, counts every match, and asks for more letters past them", async () => {
        await open({ catalog: fullCatalog });

        await typeSearch("sample");

        expect(drawnGlyphs()).toBe(80);
        expect(document.querySelector("[data-testid='emoji-live']")?.textContent?.trim()).toBe("439 results");
        expect(document.querySelector("[data-testid='emoji-more']")?.textContent?.trim()).toBe("Showing the first 80. Keep typing to narrow it down.");

        await typeSearch("sample face travel 4");
        expect(document.querySelector("[data-testid='emoji-more']")).toBeNull();
    });

    test("hides the tabs while searching, and once cleared draws the chosen tab's section again", async () => {
        await open();
        await showSection("Nature");

        await typeSearch("heart");
        expect(tabs().filter(isShown)).toHaveLength(0);

        await typeSearch("");
        expect(tabs().filter(isShown).length).toBeGreaterThan(0);
        expect(sectionLabels()).toEqual(["Nature"]);
        expect(selectedTab()).toBe("Nature");
    });
});

describe("the grid's keyboard", () => {
    test("is one tab stop", async () => {
        await open();

        expect(cells().filter(c => c.getAttribute("tabindex") === "0")).toHaveLength(1);
    });

    test("Down from the search enters the grid", async () => {
        await open();

        await press(search(), "ArrowDown");
        expect(document.activeElement).toBe(cellAt("Cedar Hill favorites", 0, 0));
    });

    test("Right and Left run in reading order from one row into the next and back", async () => {
        await open();
        const endOfRow = cellAt("Cedar Hill favorites", 0, 7);
        endOfRow.focus();

        await press(endOfRow, "ArrowRight");
        expect(document.activeElement).toBe(cellAt("Cedar Hill favorites", 1, 0));

        await press(document.activeElement as HTMLElement, "ArrowLeft");
        expect(document.activeElement).toBe(endOfRow);
    });

    test("Down keeps the column, and lands on a short row's last cell", async () => {
        await open();
        await showSection("Smileys");
        const start = cellAt("Smileys", 0, 5);
        start.focus();

        await press(start, "ArrowDown");
        expect(document.activeElement).toBe(cellAt("Smileys", 1, 1));
    });

    test("Up moves a row, and from the top row returns to the search", async () => {
        await open();
        const start = cellAt("Cedar Hill favorites", 1, 2);
        start.focus();

        await press(start, "ArrowUp");
        expect(document.activeElement).toBe(cellAt("Cedar Hill favorites", 0, 2));

        await press(document.activeElement as HTMLElement, "ArrowUp");
        expect(document.activeElement).toBe(search());
    });

    test("Home and End go to the section's first and last emoji", async () => {
        await open();
        const middle = cellAt("Cedar Hill favorites", 1, 3);
        middle.focus();

        await press(middle, "End");
        expect(document.activeElement).toBe(cellAt("Cedar Hill favorites", 2, 7));

        await press(document.activeElement as HTMLElement, "Home");
        expect(document.activeElement).toBe(cellAt("Cedar Hill favorites", 0, 0));
    });

    test("Page Down and Page Up draw the next and the previous section, focus on its first emoji", async () => {
        await open();
        const start = cellAt("Cedar Hill favorites", 1, 4);
        start.focus();

        await press(start, "PageDown");
        expect(sectionLabels()).toEqual(["Smileys"]);
        expect(selectedTab()).toBe("Smileys");
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
        const start = cellAt("Cedar Hill favorites", 0, 2);
        start.focus();

        await press(start, "p");

        expect(document.activeElement).toBe(search());
    });
});

describe("the tabs", () => {
    test("are a tablist above the grid, one per section, the favorites selected at first", async () => {
        await open({ recents: ["fire"] });

        const tablist = document.querySelector("[role='tablist']") as Element;
        expect(tablist.getAttribute("aria-label")).toBe("Emoji categories");
        expect(tablist.compareDocumentPosition(document.querySelector("[role='grid']") as Element) & Node.DOCUMENT_POSITION_FOLLOWING).toBeTruthy();
        expect(tabs().map(t => t.getAttribute("aria-label"))).toEqual(["Recently used", "Cedar Hill favorites", ...categoryLabels]);
        expect(tabs().map(t => t.getAttribute("aria-selected"))).toEqual(["false", "true", ...categoryLabels.map(() => "false")]);
    });

    test("a tab chosen is selected, draws its section in place of the one before, and starts it at its top", async () => {
        await open();
        const body = document.querySelector<HTMLElement>("[role='grid']") as HTMLElement;
        body.scrollTop = 60;

        await showSection("Nature");

        expect(tabs().filter(t => t.getAttribute("aria-selected") === "true")).toEqual([tab("Nature")]);
        expect(sectionLabels()).toEqual(["Nature"]);
        expect(body.scrollTop).toBe(0);
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
            props: { mode: "react", source: { favorites, catalog, recents: [], churchName: "Cedar Hill" }, mine: ["thumbsup"], anchor: bubble, opener: add, onReact: () => queueMicrotask(() => add.remove()) },
            attachTo: document.body
        });
        wrapper = w;
        await settle();

        cellAt("Cedar Hill favorites", 0, 0).click();
        await settle();

        expect(w.emitted("react")).toEqual([["thumbsup"]]);
        expect(document.activeElement).toBe(row);
    });

    // The way out starts a frame after the close is asked for, so it is still playing when its
    // length has passed; the close waits a little past it before giving up on it.
    test("waits on a way out still playing at its length, and closes by 50ms past it at the latest", async () => {
        const prototype = HTMLElement.prototype as unknown as { getAnimations?: () => unknown[] };
        prototype.getAnimations = () => [{ finished: new Promise(() => undefined) }];
        try {
            const w = await open();
            document.querySelector<HTMLElement>(".chat-picker")?.style.setProperty("--chat-t-fast", "120ms");
            jest.useFakeTimers();

            await press(search(), "Escape");
            await jest.advanceTimersByTimeAsync(120);
            expect(w.emitted("close")).toBeUndefined();

            await jest.advanceTimersByTimeAsync(50);
            expect(w.emitted("close")).toHaveLength(1);
        }
        finally {
            delete prototype.getAnimations;
        }
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
        await showSection("Smileys");

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

        await showSection("Objects");
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
