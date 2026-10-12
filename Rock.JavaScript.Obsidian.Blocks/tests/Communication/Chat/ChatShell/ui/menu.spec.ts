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
