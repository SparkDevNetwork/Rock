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
// The menu, mounted: an icon button that opens a list of actions beside it. A keyboard user must be
// able to open it, move through it and leave it without losing their place, so focus goes to the
// first item on open, the arrows cycle, and Escape closes it and puts focus back on the button
// that opened it. The button says whether its menu is open, which is how a screen reader knows.
import { readFileSync } from "fs";
import { join } from "path";
import { mount, VueWrapper } from "@vue/test-utils";
import { nextTick } from "vue";
import Menu from "../../../../../src/Communication/Chat/ChatShell/ui/menu.partial.obs";

const items = [
    { key: "notifications", label: "Turn on notifications", icon: "bell" },
    { key: "settings", label: "Chat settings", icon: "settings" },
    { key: "leave", label: "Leave", icon: "door", isDanger: true }
];

/** The menu is looked for in the whole document, since it may be drawn outside the chat so nothing clips it. */
function menu(): HTMLElement | null {
    return document.querySelector("[role='menu']");
}

function menuItems(): HTMLElement[] {
    return Array.from(document.querySelectorAll<HTMLElement>("[role='menu'] [role='menuitem']"));
}

async function settle(): Promise<void> {
    await nextTick();
    await nextTick();
}

async function press(key: string): Promise<void> {
    (document.activeElement as HTMLElement).dispatchEvent(new KeyboardEvent("keydown", { key, bubbles: true }));
    await settle();
}

describe("the menu", () => {
    let wrapper: VueWrapper | null = null;

    beforeAll(() => {
        // jsdom has no ResizeObserver, and floating placement keeps its menu beside the anchor with one.
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

    async function open(): Promise<HTMLElement> {
        wrapper = mount(Menu, { props: { icon: "dots", label: "More", items }, attachTo: document.body });
        const anchor = wrapper.get("button").element as HTMLElement;
        anchor.focus();
        anchor.click();
        await settle();
        return anchor;
    }

    test("is placed by its position, so its own entrance and exit can move it", async () => {
        // A transform the placement set would override the pop motion's, and the menu would only fade.
        await open();
        for (let i = 0; i < 10; i++) {
            await Promise.resolve();
        }
        await settle();

        expect(menu()?.style.transform ?? "").toBe("");
        expect(menu()?.style.left).not.toBe("");
    });

    test("is closed until its button is pressed", () => {
        wrapper = mount(Menu, { props: { icon: "dots", label: "More", items }, attachTo: document.body });

        expect(menu()).toBeNull();
        expect(wrapper.get("button").attributes("aria-expanded")).not.toBe("true");
        expect(wrapper.get("button").attributes("aria-label")).toBe("More");
    });

    test("opens as a menu of its items with the first one focused", async () => {
        const anchor = await open();

        expect(menu()).not.toBeNull();
        expect(menuItems().map(item => item.textContent!.trim())).toEqual(["Turn on notifications", "Chat settings", "Leave"]);
        expect(document.activeElement).toBe(menuItems()[0]);
        expect(anchor.getAttribute("aria-expanded")).toBe("true");
    });

    test("says an item's dot as visually hidden text, since a label on a plain span is never read", async () => {
        wrapper = mount(Menu, { props: { icon: "dots", label: "More", items: [{ key: "mentions", label: "Mentions", icon: "at", hasDot: true }, ...items] }, attachTo: document.body });
        (wrapper.get("button").element as HTMLElement).click();
        await settle();

        const dot = menuItems()[0].querySelector(".chat-menu-dot");
        expect(dot).not.toBeNull();
        expect(dot?.hasAttribute("aria-label")).toBe(false);
        expect(dot?.querySelector(".chat-sr-only")?.textContent).toBe("new");
        expect(menuItems()[1].querySelector(".chat-menu-dot")).toBeNull();
    });

    test("moves down with the down arrow and wraps from the last item to the first", async () => {
        await open();

        await press("ArrowDown");
        expect(document.activeElement).toBe(menuItems()[1]);
        await press("ArrowDown");
        expect(document.activeElement).toBe(menuItems()[2]);
        await press("ArrowDown");
        expect(document.activeElement).toBe(menuItems()[0]);
    });

    test("moves up with the up arrow and wraps from the first item to the last", async () => {
        await open();

        await press("ArrowUp");
        expect(document.activeElement).toBe(menuItems()[2]);
        await press("ArrowUp");
        expect(document.activeElement).toBe(menuItems()[1]);
    });

    test("opens from its button on Down at the first item, and on Up at the last", async () => {
        wrapper = mount(Menu, { props: { icon: "dots", label: "More", items }, attachTo: document.body });
        const anchor = wrapper.get("button").element as HTMLElement;
        anchor.focus();

        await press("ArrowDown");
        expect(document.activeElement).toBe(menuItems()[0]);

        await press("Escape");
        await press("ArrowUp");
        expect(document.activeElement).toBe(menuItems()[2]);
    });

    test("goes to the next item that starts with a letter typed", async () => {
        await open();

        await press("l");
        expect(document.activeElement).toBe(menuItems()[2]);
        await press("T");
        expect(document.activeElement).toBe(menuItems()[0]);
    });

    test("closes on Escape and gives focus back to its button", async () => {
        const anchor = await open();

        await press("Escape");

        expect(menu()).toBeNull();
        expect(document.activeElement).toBe(anchor);
        expect(anchor.getAttribute("aria-expanded")).not.toBe("true");
    });

    test("closes on a click outside it", async () => {
        const anchor = await open();

        document.body.dispatchEvent(new MouseEvent("pointerdown", { bubbles: true }));
        document.body.dispatchEvent(new MouseEvent("mousedown", { bubbles: true }));
        document.body.click();
        await settle();

        expect(menu()).toBeNull();
        expect(anchor.getAttribute("aria-expanded")).not.toBe("true");
    });

    test("chooses an item with Enter, says which, and closes", async () => {
        await open();

        await press("ArrowDown");
        await press("Enter");

        expect(wrapper!.emitted("select")).toEqual([["settings"]]);
        expect(menu()).toBeNull();
    });

    test("chooses an item with a click", async () => {
        await open();

        menuItems()[2].click();
        await settle();

        expect(wrapper!.emitted("select")).toEqual([["leave"]]);
        expect(menu()).toBeNull();
    });
});

// A menu asked to be a sheet is the phone's form of the same list: a scrim and a bottom sheet with
// a grab bar and full width rows, not a popover beside its button.
describe("the menu as a sheet", () => {
    let wrapper: VueWrapper | null = null;

    afterEach(() => {
        wrapper?.unmount();
        wrapper = null;
        document.body.innerHTML = "";
    });

    async function openSheet(): Promise<HTMLElement> {
        wrapper = mount(Menu, { props: { icon: "plus", label: "Add", items, isSheet: true }, attachTo: document.body });
        const anchor = wrapper.get("button").element as HTMLElement;
        anchor.focus();
        anchor.click();
        await settle();
        return anchor;
    }

    test("draws a scrim and a sheet with a grab bar and one sheet row for each item, and no floating placement", async () => {
        await openSheet();

        expect(document.querySelector(".chat-sheet-scrim")).not.toBeNull();
        expect(menu()?.classList.contains("chat-sheet")).toBe(true);
        expect(menu()?.querySelector(".chat-sheet-grab")).not.toBeNull();
        expect(document.querySelectorAll("[role='menu'] .chat-sheet-item")).toHaveLength(3);
        expect(menu()?.style.left ?? "").toBe("");
        expect(menu()?.style.top ?? "").toBe("");
        expect(document.activeElement).toBe(menuItems()[0]);
    });

    test("a press on the scrim closes it with focus back on the button", async () => {
        const anchor = await openSheet();

        document.querySelector(".chat-sheet-scrim")!.dispatchEvent(new Event("pointerdown", { bubbles: true }));
        await settle();

        expect(menu()).toBeNull();
        expect(document.activeElement).toBe(anchor);
    });

    test("Tab and Shift+Tab go round the rows and never leave the sheet", async () => {
        await openSheet();
        const rows = menuItems();
        const tab = async (isShift: boolean): Promise<void> => {
            (document.activeElement as HTMLElement).dispatchEvent(new KeyboardEvent("keydown", { key: "Tab", shiftKey: isShift, bubbles: true, cancelable: true }));
            await settle();
        };

        rows[rows.length - 1].focus();
        await tab(false);
        expect(document.activeElement).toBe(rows[0]);

        await tab(true);
        expect(document.activeElement).toBe(rows[rows.length - 1]);
        expect(menu()).not.toBeNull();
    });

    test("Shift+Tab with focus on no row starts from the last row", async () => {
        await openSheet();
        const rows = menuItems();
        const menuElement = menu()!;
        menuElement.tabIndex = -1;
        menuElement.focus();

        menuElement.dispatchEvent(new KeyboardEvent("keydown", { key: "Tab", shiftKey: true, bubbles: true, cancelable: true }));
        await settle();

        expect(document.activeElement).toBe(rows[rows.length - 1]);
    });

    test("a choice is emitted and closes it, and Escape closes it with focus back", async () => {
        const anchor = await openSheet();
        menuItems()[1].click();
        await settle();
        expect(wrapper!.emitted("select")).toEqual([["settings"]]);
        expect(menu()).toBeNull();

        anchor.click();
        await settle();
        await press("Escape");
        expect(menu()).toBeNull();
        expect(document.activeElement).toBe(anchor);
    });
});

// How a focused item looks. jsdom computes no styles, so these read the menu's own rules: they show
// the item never hides the chat's focus ring and is tinted only under the pointer, not that a
// browser draws either; a browser check proves that.
describe("the menu's focused item", () => {
    const source = readFileSync(join(__dirname, "../../../../../src/Communication/Chat/ChatShell/ui/menu.partial.obs"), "utf8");
    const styles = (/<style[^>]*>([\s\S]*?)<\/style>/.exec(source)?.[1] ?? "").replace(/\/\*[\s\S]*?\*\//g, "");

    /** Each rule that names a menu item, as its selector and its declarations. */
    const itemRules = Array.from(styles.matchAll(/([^{}]+)\{([^{}]*)\}/g))
        .map(m => ({ selector: m[1].trim().replace(/\s+/g, " "), body: m[2] }))
        .filter(rule => rule.selector.includes(".chat-menu-item"));

    test("keeps the chat's focus ring: no rule takes the outline away", () => {
        expect(itemRules.filter(rule => /outline\s*:\s*(none|0)/.test(rule.body)).map(rule => rule.selector)).toEqual([]);
    });

    test("is tinted under the pointer only, never under focus", () => {
        const tinted = itemRules.filter(rule => /background\s*:\s*var\(--chat-hover\)/.test(rule.body)).flatMap(rule => rule.selector.split(",").map(part => part.trim()));

        expect(tinted).toEqual([".chat-menu-item:hover"]);
    });
});
