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
// The bottom sheet and its scrim, what every menu, picker and card is on a phone. The sheet is
// modal: focus stays inside it, a screen reader hears only it, and the scrim behind it takes the
// whole of a tap that lands on it, so nothing under the scrim is pressed by the tap that closed it.
// Announcements made while it is open are spoken from inside it, since a screen reader ignores a
// live region outside a modal dialog.
import { readFileSync } from "fs";
import { mount, VueWrapper } from "@vue/test-utils";
import { defineComponent, h, nextTick, Ref, ref } from "vue";
import Sheet from "../../../../../src/Communication/Chat/ChatShell/ui/sheet.partial.obs";
import Scrim from "../../../../../src/Communication/Chat/ChatShell/ui/scrim.partial.obs";
import SurfacePage from "../../../../../src/Communication/Chat/ChatShell/ui/surfacePage.partial.obs";
import { provideAnnouncer, useAnnouncer } from "../../../../../src/Communication/Chat/ChatShell/ui/announcer.partial";
import { SheetHeight } from "../../../../../src/Communication/Chat/ChatShell/types.partial";
import { chatSheet } from "../statusStore";
import { ruleOf, styleOf } from "./partStyle";
import { panelMotion, standInHeights } from "./surfaceMotion";

async function settle(): Promise<void> {
    await nextTick();
    await nextTick();
}

function find(id: string): HTMLElement | null {
    return document.querySelector<HTMLElement>(`[data-testid='${id}']`);
}

describe("the scrim", () => {
    afterEach(() => {
        document.body.innerHTML = "";
    });

    it("takes the whole tap: it dismisses on the click that ends a press begun on it, never on the press alone", async () => {
        const wrapper = mount(Scrim, { props: { isShown: true }, attachTo: document.body });
        const scrim = find("scrim") as HTMLElement;
        scrim.dispatchEvent(new Event("pointerdown", { bubbles: true }));
        expect(wrapper.emitted("dismiss")).toBeUndefined();
        scrim.click();
        expect(wrapper.emitted("dismiss")).toHaveLength(1);
        wrapper.unmount();
    });

    it("ignores a click whose press began elsewhere, as a drag out of the sheet that ends on the scrim", () => {
        const wrapper = mount(Scrim, { props: { isShown: true }, attachTo: document.body });
        (find("scrim") as HTMLElement).click();
        expect(wrapper.emitted("dismiss")).toBeUndefined();
        wrapper.unmount();
    });
});

describe("the sheet", () => {
    let wrapper: VueWrapper | null = null;

    afterEach(() => {
        wrapper?.unmount();
        wrapper = null;
        document.body.innerHTML = "";
    });

    function sheet(props: Record<string, unknown> = {}): VueWrapper {
        wrapper = mount(Sheet, {
            props: { isOpen: true, label: "Message actions", ...props },
            slots: { default: () => [h("button", { "data-testid": "one" }, "one"), h("button", { "data-testid": "two" }, "two")] },
            attachTo: document.body
        });
        return wrapper;
    }

    it("is a modal dialog named for a screen reader, with its scrim", async () => {
        sheet();
        await settle();
        const drawn = find("sheet");
        expect(drawn?.getAttribute("role")).toBe("dialog");
        expect(drawn?.getAttribute("aria-modal")).toBe("true");
        expect(drawn?.getAttribute("aria-label")).toBe("Message actions");
        expect(find("scrim")).not.toBeNull();
    });

    it("takes focus to its first control as it opens", async () => {
        sheet();
        await settle();
        expect(document.activeElement).toBe(find("one"));
    });

    it("keeps Tab inside, wrapping from its last control to its first and back", async () => {
        sheet();
        await settle();
        (find("two") as HTMLElement).focus();
        const forward = new KeyboardEvent("keydown", { key: "Tab", bubbles: true, cancelable: true });
        document.activeElement?.dispatchEvent(forward);
        expect(document.activeElement).toBe(find("one"));
        const back = new KeyboardEvent("keydown", { key: "Tab", shiftKey: true, bubbles: true, cancelable: true });
        document.activeElement?.dispatchEvent(back);
        expect(document.activeElement).toBe(find("two"));
    });

    it("asks to close on Escape, and keeps the key from a panel under it", async () => {
        const outer = jest.fn();
        document.body.addEventListener("keydown", outer);
        const drawn = sheet();
        await settle();
        (find("one") as HTMLElement).dispatchEvent(new KeyboardEvent("keydown", { key: "Escape", bubbles: true, cancelable: true }));
        expect(drawn.emitted("dismiss")).toHaveLength(1);
        expect(outer).not.toHaveBeenCalled();
        document.body.removeEventListener("keydown", outer);
    });

    it("asks to close on a tap on its scrim", async () => {
        const drawn = sheet();
        await settle();
        const scrim = find("scrim") as HTMLElement;
        scrim.dispatchEvent(new Event("pointerdown", { bubbles: true }));
        scrim.click();
        expect(drawn.emitted("dismiss")).toHaveLength(1);
    });

    it("is drawn at one of two heights: as tall as its content, or the taller picker height", async () => {
        sheet({ height: "picker" });
        await settle();
        expect(find("sheet")?.classList.contains("is-picker")).toBe(true);
        wrapper?.unmount();
        sheet();
        await settle();
        expect(find("sheet")?.classList.contains("is-picker")).toBe(false);
    });

    it("keeps clear of a phone's home bar and slides up its whole height, a fade alone under reduced motion", () => {
        const source = readFileSync(require.resolve("../../../../../src/Communication/Chat/ChatShell/ui/sheet.partial.obs"), "utf8");
        expect(source).toMatch(/safe-area-inset-bottom/);
        expect(source).toMatch(/animation:\s*chat-slide-in/);

        // the shared slide is redefined as a fade where less motion is asked for
        const sheetCss = readFileSync(chatSheet, "utf8");
        const reduced = sheetCss.slice(sheetCss.indexOf("@media (prefers-reduced-motion: reduce)"));
        expect(reduced).toMatch(/@keyframes chat-slide-in\s*\{\s*from\s*\{\s*opacity:\s*0;\s*\}/);
    });

    it("speaks an announcement made while it is open from inside itself, where a screen reader still listens", async () => {
        let say: (text: string) => void = () => undefined;
        const Host = defineComponent({
            setup() {
                const root = ref<HTMLElement>();
                provideAnnouncer(root);
                return () => h("div", { ref: root, class: "chat-shell" }, [h(Child)]);
            }
        });
        const Child = defineComponent({
            setup() {
                say = useAnnouncer().announce;
                return () => h(Sheet, { isOpen: true, label: "Pick" }, () => h("button", "one"));
            }
        });

        wrapper = mount(Host, { attachTo: document.body });
        await settle();
        say("3 people found");
        await settle();
        const drawn = find("sheet") as HTMLElement;
        const live = Array.from(document.querySelectorAll<HTMLElement>("[aria-live]")).filter(region => region.textContent === "3 people found");
        expect(live).toHaveLength(1);
        expect(drawn.contains(live[0])).toBe(true);
    });
});

// A choice that leads to another, as the plus menu's GIF, turns the same sheet into the next page
// instead of stacking a second sheet: the handle stays, a head gains Back and the page's title,
// and Back or Escape returns one page with focus on the control that turned it.
describe("a sheet turned into another page", () => {
    let wrapper: VueWrapper | null = null;

    afterEach(() => {
        wrapper?.unmount();
        wrapper = null;
        document.body.innerHTML = "";
    });

    /** How the plus menu's sheet is drawn for a cell. */
    type Turning = {
        /** Whether the GIF page's field asks for focus. */
        hasAutofocus?: boolean;

        /** The height the GIF page asks the sheet for. */
        height?: SheetHeight;

        /** Whether Vue's own transitions run, as on a page, so a page going back is still drawn as it leaves. */
        isMoving?: boolean;
    };

    /** The plus menu's sheet, whose GIF row turns it into the GIF page. */
    function turning(options: Turning = {}): { isGif: Ref<boolean>, isOpen: Ref<boolean>, wrapper: VueWrapper } {
        const { hasAutofocus = false, height = "content", isMoving = false } = options;
        const isGif = ref(false);
        const isOpen = ref(true);
        const Host = defineComponent({
            setup() {
                return () => h(Sheet, { isOpen: isOpen.value, label: "Add to your message" }, {
                    default: () => [
                        h("button", { "data-testid": "to-gif", onClick: () => {
                            isGif.value = true;
                        } }, "GIF"),
                        h("button", { "data-testid": "photo" }, "Photo or file")
                    ],
                    pages: () => h(SurfacePage, { isOpen: isGif.value, title: "GIF", height, onBack: () => {
                        isGif.value = false;
                    } }, () => [
                        h("input", { "data-testid": "gif-search", "data-autofocus": hasAutofocus ? "" : undefined }),
                        h("button", { "data-testid": "gif-tile" }, "tile")
                    ])
                });
            }
        });

        wrapper = mount(Host, { attachTo: document.body, global: { stubs: { transition: !isMoving } } });
        return { isGif, isOpen, wrapper };
    }

    async function turn(): Promise<HTMLElement> {
        const opener = find("to-gif") as HTMLElement;
        opener.focus();
        opener.click();
        await settle();
        return opener;
    }

    it("turns in one sheet over one scrim, its head drawing Back and the page's title, the first page set aside", async () => {
        turning();
        await settle();
        expect(find("page-back")).toBeNull();

        await turn();

        expect(document.querySelectorAll("[data-testid='sheet']")).toHaveLength(1);
        expect(document.querySelectorAll("[data-testid='scrim']")).toHaveLength(1);
        const sheet = find("sheet") as HTMLElement;
        expect(sheet.querySelector(".chat-sheet-handle")).not.toBeNull();
        expect(sheet.contains(find("page-back"))).toBe(true);
        expect(find("page-back")?.getAttribute("aria-label")).toBe("Back");
        expect(find("page-title")?.textContent).toBe("GIF");
        expect(sheet.contains(find("gif-search"))).toBe(true);

        // the first page stays drawn under the turned one, out of reach and unheard
        const first = (find("to-gif") as HTMLElement).closest<HTMLElement>("[data-testid='surface-page']");
        expect(first?.hasAttribute("inert")).toBe(true);
        expect(first?.getAttribute("aria-hidden")).toBe("true");
    });

    // Focus on the page itself, named by its title, is what makes a screen reader say where the
    // person is now; on Back, outside the page, it would hear only "Back".
    it("puts focus on the page itself as it turns, named by its title, or on what the page marks to start on", async () => {
        turning();
        await settle();
        await turn();
        const page = document.activeElement as HTMLElement;
        expect(page.getAttribute("data-testid")).toBe("surface-page");
        expect(page.getAttribute("role")).toBe("group");
        expect(page.getAttribute("aria-label")).toBe("GIF");
        expect(page.contains(find("gif-search"))).toBe(true);

        // Tab from the page goes on to the head's Back, the first control, and stays inside
        page.dispatchEvent(new KeyboardEvent("keydown", { key: "Tab", bubbles: true, cancelable: true }));
        expect(document.activeElement).toBe(find("page-back"));

        // the page holding focus draws no ring around itself, as a dialog holding focus draws none
        expect(ruleOf(styleOf("surfacePage.partial.obs"), ".chat-surface-page:focus")).toMatch(/outline:\s*none/);

        wrapper?.unmount();
        document.body.innerHTML = "";
        turning({ hasAutofocus: true });
        await settle();
        await turn();
        expect(document.activeElement).toBe(find("gif-search"));
    });

    it("goes back one page on Back, with focus on the control that turned it", async () => {
        const { isGif } = turning();
        await settle();
        const opener = await turn();

        (find("page-back") as HTMLElement).click();
        await settle();

        expect(isGif.value).toBe(false);
        expect(find("page-back")).toBeNull();
        expect(document.activeElement).toBe(opener);
        expect(opener.closest("[inert]")).toBeNull();
    });

    it("goes back one page on Escape, and only the first page's Escape asks the sheet to close", async () => {
        const { isGif } = turning();
        await settle();
        const opener = await turn();
        const sheet = wrapper?.findComponent(Sheet);

        (find("gif-tile") as HTMLElement).dispatchEvent(new KeyboardEvent("keydown", { key: "Escape", bubbles: true, cancelable: true }));
        await settle();
        expect(isGif.value).toBe(false);
        expect(sheet?.emitted("dismiss")).toBeUndefined();
        expect(document.activeElement).toBe(opener);

        opener.dispatchEvent(new KeyboardEvent("keydown", { key: "Escape", bubbles: true, cancelable: true }));
        expect(sheet?.emitted("dismiss")).toHaveLength(1);
    });

    // Whatever is out of front is out of reach: the page going back lies over the uncovered one
    // for its whole leave, where it would catch a tap or a Tab meant for the page under.
    it("keeps the page going back out of reach and unheard while it leaves", async () => {
        turning({ isMoving: true });
        await settle();
        const opener = await turn();

        (find("page-back") as HTMLElement).click();
        await settle();

        const leaving = Array.from(document.querySelectorAll<HTMLElement>("[data-testid='surface-page']")).find(page => page.contains(find("gif-tile")));
        expect(leaving?.classList.contains("is-leaving")).toBe(true);
        expect(leaving?.hasAttribute("inert")).toBe(true);
        expect(leaving?.getAttribute("aria-hidden")).toBe("true");
        expect(opener.closest("[inert]")).toBeNull();
    });

    // The surface's close closes its pages with it, by their owners, top first, so it opens again
    // on its own first page rather than turned.
    it("closes every page as it closes, so it opens again on its first page", async () => {
        const { isGif, isOpen } = turning();
        await settle();
        await turn();

        isOpen.value = false;
        await settle();
        expect(isGif.value).toBe(false);

        isOpen.value = true;
        await settle();
        expect(find("page-back")).toBeNull();
        expect(find("to-gif")?.closest("[inert]")).toBeNull();
    });

    it("keeps Tab inside the turned page", async () => {
        turning();
        await settle();
        await turn();

        (find("gif-tile") as HTMLElement).focus();
        document.activeElement?.dispatchEvent(new KeyboardEvent("keydown", { key: "Tab", bubbles: true, cancelable: true }));

        expect(document.activeElement).toBe(find("page-back"));
    });

    it("slides the page in from the right while the page under moves a quarter left, a fade under reduced motion", () => {
        const style = styleOf("surfacePage.partial.obs");

        // the whole sheet's slide distance, which the reduced-motion block takes to nothing
        expect(ruleOf(style, ".chat-surface-page.is-beyond")).toMatch(/transform:\s*translateX\(var\(--chat-rise-sheet\)\)/);
        expect(ruleOf(style, ".chat-surface-page.is-beyond")).toMatch(/opacity:\s*0/);
        expect(ruleOf(style, ".chat-surface-page.is-covered")).toMatch(/transform:\s*translateX\(calc\(var\(--chat-rise-sheet\) \/ -4\)\)/);
        expect(ruleOf(style, ".chat-surface-page")).toMatch(/transition:[^;]*transform var\(--chat-t-panel\)/);
        expect(style).not.toMatch(/@keyframes/);
    });

    // Visibility steps with the slide over the panel's own duration, seen at every step but a hidden
    // end: a page set aside is seen as it slides away and hides once it is there, and a page coming
    // back is seen from its first frame. No delay of its own can drift from the slide's duration.
    it("hides a page set aside only once it has slid away, and shows it at once as it comes back", () => {
        const style = styleOf("surfacePage.partial.obs");

        expect(ruleOf(style, ".chat-surface-page")).toMatch(/transition:[^;]*visibility var\(--chat-t-panel\)/);
        expect(ruleOf(style, ".chat-surface-page.is-covered")).toMatch(/visibility:\s*hidden/);
        expect(ruleOf(style, ".chat-surface-page.is-covered")).not.toMatch(/transition/);
    });

    // A turn is one motion: as the pages slide, the same sheet grows or shrinks from the height of
    // the page it leaves to the height of the page it turns to, the handle riding on its top edge.
    describe("its height", () => {
        let heights: ReturnType<typeof standInHeights>;
        const original = window.matchMedia;

        beforeEach(() => {
            heights = standInHeights();
        });

        afterEach(() => {
            heights.restore();
            window.matchMedia = original;
        });

        it("grows to the picker height a page asks for over the panel's duration, and shrinks back to its content on Back", async () => {
            turning({ height: "picker" });
            await settle();
            const sheet = find("sheet") as HTMLElement;
            heights.size(sheet, () => sheet.classList.contains("is-picker") ? 600 : 137);

            await turn();

            expect(sheet.classList.contains("is-picker")).toBe(true);
            expect(heights.played).toEqual([{ element: sheet, from: "137px", to: "600px", ...panelMotion }]);
            expect(sheet.firstElementChild?.classList.contains("chat-sheet-handle")).toBe(true);

            (find("page-back") as HTMLElement).click();
            await settle();

            expect(sheet.classList.contains("is-picker")).toBe(false);
            expect(heights.played).toEqual([
                { element: sheet, from: "137px", to: "600px", ...panelMotion },
                { element: sheet, from: "600px", to: "137px", ...panelMotion }
            ]);
        });

        it("changes its height at once where less motion is asked for, the pages only fading", async () => {
            window.matchMedia = ((query: string) => ({ media: query, matches: query === "(prefers-reduced-motion: reduce)" })) as unknown as typeof window.matchMedia;
            turning({ height: "picker" });
            await settle();
            const sheet = find("sheet") as HTMLElement;
            heights.size(sheet, () => sheet.classList.contains("is-picker") ? 600 : 137);

            await turn();

            expect(sheet.classList.contains("is-picker")).toBe(true);
            expect(heights.played).toEqual([]);
        });
    });
});
