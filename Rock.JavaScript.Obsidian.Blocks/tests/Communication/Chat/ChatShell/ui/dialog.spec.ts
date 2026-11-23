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
// Where the dialog stands in the chat's frame. A question stands centred; a dialog whose body
// grows and shrinks while it is open, as settings does when a note comes and goes, is anchored at
// the top, so its title and its first rows never move under the person's eye.
import { readdirSync, readFileSync } from "fs";
import { join } from "path";
import { mount, VueWrapper } from "@vue/test-utils";
import { defineComponent, h, nextTick, ref } from "vue";
import Button from "../../../../../src/Communication/Chat/ChatShell/ui/button.partial.obs";
import Dialog from "../../../../../src/Communication/Chat/ChatShell/ui/dialog.partial.obs";
import SurfacePage from "../../../../../src/Communication/Chat/ChatShell/ui/surfacePage.partial.obs";
import { providedSession, recordingBackStack } from "../sessionShapes";
import { shellFolder } from "../statusStore";
import { ruleOf, styleOf } from "./partStyle";
import { panelMotion, standInHeights } from "./surfaceMotion";

let wrapper: VueWrapper | null = null;

function draw(props: Record<string, unknown> = {}): VueWrapper {
    wrapper = mount(Dialog, {
        props: { isOpen: true, title: "Chat settings", ...props },
        global: { provide: { ...providedSession() } },
        attachTo: document.body
    });
    return wrapper;
}

afterEach(() => {
    wrapper?.unmount();
    wrapper = null;
});

describe("the dialog's place", () => {
    test("stands centred unless told otherwise", () => {
        const scrim = draw().get("[data-testid='dialog-scrim']");

        expect(scrim.classes()).not.toContain("is-placed-top");
        expect(ruleOf(styleOf("dialog.partial.obs"), ".chat-modal")).toMatch(/place-items:\s*center/);
    });

    test("anchored at the top, keeps its top edge where it opened however tall its body grows", () => {
        const scrim = draw({ placement: "top" }).get("[data-testid='dialog-scrim']");

        expect(scrim.classes()).toContain("is-placed-top");
        expect(ruleOf(styleOf("dialog.partial.obs"), ".chat-modal.is-placed-top")).toMatch(/place-items:\s*start center/);
    });

    test("rises from the bottom as a sheet on a phone wherever it would stand on a wider screen", () => {
        wrapper = mount(Dialog, {
            props: { isOpen: true, title: "Chat settings", placement: "top", sheet: "phone" },
            global: { provide: { ...providedSession({ isPhone: true }) } },
            attachTo: document.body
        });
        const scrim = wrapper.get("[data-testid='dialog-scrim']");

        expect(scrim.classes()).toContain("is-sheet");
        expect(scrim.classes()).not.toContain("is-placed-top");
    });
});

// A dialog's footer is the design's large row of buttons, 36px, whether the dialog draws its own
// Cancel and confirm or a screen hands it its own actions.
describe("the dialog's footer", () => {
    test("draws its Cancel and confirm large", () => {
        const w = draw({ confirmLabel: "Delete", isDestructive: true });

        expect(w.findAllComponents(Button).map(button => button.props("size"))).toEqual(["lg", "lg"]);
    });

    test("is handed large buttons by every screen that fills it", () => {
        const files = [...readdirSync(join(shellFolder, "components")).map(file => join(shellFolder, "components", file)), join(shellFolder, "../chatShell.obs")]
            .filter(path => path.endsWith(".obs"));
        const footers = files.flatMap(path => {
            const text = readFileSync(path, "utf8");
            return text.includes("<Dialog") ? Array.from(text.matchAll(/<template #actions>([\s\S]*?)<\/template>\s*<\/Dialog>/g), match => match[1]) : [];
        });
        const buttons = footers.flatMap(footer => Array.from(footer.matchAll(/<Button\b[^>]*>/g), match => match[0]));

        expect(buttons.length).toBeGreaterThan(3);
        expect(buttons.filter(button => !/\bsize="lg"/.test(button))).toEqual([]);
    });
});

// A panel whose row leads to another page, as settings' Blocked people, turns in place: one
// dialog, its head showing Back and the page's title, which also names the dialog while it shows.
// It turns the same way centred, anchored at the top, and risen as a phone's sheet.
describe("a panel turned into another page", () => {
    /**
     * Settings, turned to Blocked people from its row.
     *
     * @param props The dialog's own props.
     * @param isPhone Whether the session reads a phone.
     * @param beforeTurn What the person did in the dialog before the row turned it, as scrolling down to it.
     * @param isMoving Whether Vue's own transitions run, as on a page, so a page going back is still drawn as it leaves.
     */
    async function turned(props: Record<string, unknown>, isPhone = false, beforeTurn?: (dialog: HTMLElement) => void, isMoving = false): Promise<{ isBlocked: { value: boolean }, opener: HTMLElement }> {
        const isBlocked = ref(false);
        const Host = defineComponent({
            setup() {
                return () => h(Dialog, { isOpen: true, title: "Chat settings", isPanel: true, ...props }, {
                    tools: () => h("button", { "data-testid": "settings-close" }, "Close"),
                    default: () => h("button", { "data-testid": "to-blocked", onClick: () => {
                        isBlocked.value = true;
                    } }, "Blocked people"),
                    pages: () => h(SurfacePage, { isOpen: isBlocked.value, title: "Blocked people", onBack: () => {
                        isBlocked.value = false;
                    } }, () => h("button", { "data-testid": "unblock" }, "Unblock"))
                });
            }
        });

        wrapper = mount(Host, { global: { provide: { ...providedSession({ isPhone }) }, stubs: { transition: !isMoving } }, attachTo: document.body });
        await nextTick();
        beforeTurn?.(document.querySelector<HTMLElement>("[data-testid='dialog']") as HTMLElement);
        const opener = document.querySelector<HTMLElement>("[data-testid='to-blocked']") as HTMLElement;
        opener.focus();
        opener.click();
        await nextTick();
        await nextTick();
        return { isBlocked, opener };
    }

    test.each([
        ["centred", { placement: "center" }, false],
        ["at the top", { placement: "top" }, false],
        ["as a phone's sheet", { placement: "top", sheet: "phone" }, true]
    ])("turns %s in one dialog, its head Back and the page's title, and Escape goes back", async (_where, props, isPhone) => {
        const { isBlocked, opener } = await turned(props, isPhone);

        expect(document.querySelectorAll("[data-testid='dialog']")).toHaveLength(1);
        const dialog = document.querySelector<HTMLElement>("[data-testid='dialog']") as HTMLElement;
        const back = dialog.querySelector<HTMLElement>("[data-testid='page-back']");
        expect(back?.getAttribute("aria-label")).toBe("Back");
        const title = document.getElementById(dialog.getAttribute("aria-labelledby") ?? "");
        expect(title?.textContent).toBe("Blocked people");

        // the approved settings design keeps the dialog's Close beside the title while it is
        // turned: the head reads Back, the page's title, then Close, so one tap still leaves
        const close = dialog.querySelector<HTMLElement>("[data-testid='settings-close']");
        expect(Array.from(title?.parentElement?.children ?? [])).toEqual([back, title, close]);
        expect(dialog.contains(document.querySelector("[data-testid='unblock']"))).toBe(true);
        // focus is on the page itself, named by its title, so a screen reader says where the person is
        expect(document.activeElement).toBe(dialog.querySelector("[role='group'][aria-label='Blocked people']"));
        expect(document.activeElement?.contains(document.querySelector("[data-testid='unblock']"))).toBe(true);

        back?.dispatchEvent(new KeyboardEvent("keydown", { key: "Escape", bubbles: true, cancelable: true }));
        await nextTick();
        await nextTick();

        expect(isBlocked.value).toBe(false);
        expect(wrapper?.findComponent(Dialog).emitted("close")).toBeUndefined();
        expect(document.getElementById(dialog.getAttribute("aria-labelledby") ?? "")?.textContent).toBe("Chat settings");
        expect(document.activeElement).toBe(opener);
    });

    // A page set aside or going back is placed over the one on top; placed against the padded body
    // itself, it would jump into the padding as it starts to move.
    test("places its pages inside the body's padding, never against the padded body itself", async () => {
        await turned({});

        const page = document.querySelector<HTMLElement>("[data-testid='surface-page']") as HTMLElement;
        expect(page.parentElement?.classList.contains("chat-dialog-pages")).toBe(true);
        const style = styleOf("dialog.partial.obs");
        expect(ruleOf(style, ".chat-dialog-pages")).toMatch(/position:\s*relative/);
        expect(ruleOf(style, ".chat-dialog-scroll")).not.toMatch(/position:\s*relative/);
        expect(ruleOf(style, ".chat-dialog-scroll")).toMatch(/overflow-x:\s*hidden/);
    });

    // A turn is one motion: as the pages slide, the dialog grows or shrinks from the height of the
    // page it leaves to the height of the page it turns to.
    test("shrinks to the page it turns to over the panel's duration, and grows back to the first page on Back", async () => {
        const heights = standInHeights();
        try {
            await turned({}, false, dialog => heights.size(dialog, () => dialog.querySelector("[data-testid='page-back']") ? 180 : 520));
            const dialog = document.querySelector<HTMLElement>("[data-testid='dialog']") as HTMLElement;

            expect(heights.played).toEqual([{ element: dialog, from: "520px", to: "180px", ...panelMotion }]);

            dialog.querySelector<HTMLElement>("[data-testid='page-back']")?.click();
            await nextTick();
            await nextTick();

            expect(heights.played[1]).toEqual({ element: dialog, from: "180px", to: "520px", ...panelMotion });
        }
        finally {
            heights.restore();
        }
    });

    // Settings scrolled down to its Blocked people row turns to a page that starts at its own top,
    // never in blank space below a short page, and Back finds settings where the person left it.
    // A page out of front is clipped to the one in front, so it adds nothing to the body's scroll.
    test("starts the page it turns to at its top, and gives the first page back its place on Back", async () => {
        await turned({}, false, dialog => {
            (dialog.querySelector(".chat-dialog-scroll") as HTMLElement).scrollTop = 400;
        });
        const scroll = document.querySelector<HTMLElement>(".chat-dialog-scroll") as HTMLElement;

        expect(scroll.scrollTop).toBe(0);

        document.querySelector<HTMLElement>("[data-testid='page-back']")?.click();
        await nextTick();
        await nextTick();

        expect(scroll.scrollTop).toBe(400);
        const page = styleOf("surfacePage.partial.obs");
        for (const outOfFront of [".chat-surface-page.is-covered", ".chat-surface-page.is-leaving"]) {
            expect(ruleOf(page, outOfFront)).toMatch(/position:\s*absolute/);
            expect(ruleOf(page, outOfFront)).toMatch(/inset:\s*0\s*(;|$)/);
            expect(ruleOf(page, outOfFront)).toMatch(/overflow:\s*hidden/);
        }
    });

    // The page sliding out of front, set aside or going back, is drawn as the person last saw it:
    // where the body's view was, as much of it as was seen, and scrolled to where it was. Drawn
    // from its own top it would flash settings' first rows on a turn from far down, and on Back
    // the page going back would stand at the restored place, below its own end, and show nothing.
    test.each([
        ["centred", {}, false],
        ["as a phone's sheet", { placement: "top", sheet: "phone" }, true]
    ])("draws the page sliding out %s as the person last saw it, at the view and scrolled where it was", async (_where, props, isPhone) => {
        // jsdom lays nothing out: the body's view is 300 tall and 100 down the frame, and the page in
        // front starts under the body's 18px padding, moving up as the body scrolls
        const view = { top: 100, height: 300, padding: 18 };
        let scroll = document.body;
        const standIn = (page: HTMLElement, height: number): void => {
            page.getBoundingClientRect = () => {
                const top = view.top + view.padding - scroll.scrollTop;
                return { top, bottom: top + height, height } as DOMRect;
            };
        };

        await turned(props, isPhone, dialog => {
            scroll = dialog.querySelector(".chat-dialog-scroll") as HTMLElement;
            scroll.getBoundingClientRect = () => ({ top: view.top, bottom: view.top + view.height, height: view.height }) as DOMRect;
            Object.defineProperty(scroll, "clientHeight", { value: view.height });
            standIn(dialog.querySelector("[data-testid='surface-page']") as HTMLElement, 1200);
            scroll.scrollTop = 400;
        }, true);
        const pageOf = (id: string): HTMLElement => Array.from(document.querySelectorAll<HTMLElement>("[data-testid='surface-page']"))
            .find(page => page.contains(document.querySelector(`[data-testid='${id}']`))) as HTMLElement;
        const first = pageOf("to-blocked");
        const blocked = pageOf("unblock");

        // settings, seen from 382px down its rows at the view's top, stays there as it slides away:
        // its box stands at the view's top over the body's padding, 300 tall, scrolled 382 within
        expect(scroll.scrollTop).toBe(0);
        expect(first.classList.contains("is-covered")).toBe(true);
        expect([first.style.top, first.style.maxHeight, first.scrollTop]).toEqual(["-18px", "300px", 382]);
        // the page in front is drawn in the flow as ever
        expect([blocked.style.top, blocked.style.maxHeight]).toEqual(["", ""]);

        // Blocked people, scrolled 50 down, goes back: drawn at the restored place, scrolled 32
        // within, so it leaves showing what was seen, over settings back at its place in front
        standIn(blocked, 600);
        scroll.scrollTop = 50;
        (document.querySelector("[data-testid='page-back']") as HTMLElement).click();
        await nextTick();
        await nextTick();

        expect(scroll.scrollTop).toBe(400);
        expect(blocked.isConnected && blocked.classList.contains("is-leaving")).toBe(true);
        expect([blocked.style.top, blocked.style.maxHeight, blocked.scrollTop]).toEqual(["382px", "300px", 32]);
        expect([first.style.top, first.style.maxHeight]).toEqual(["", ""]);
    });
});

// On a phone every dialog, a confirm, a panel or the photo viewer, holds one history entry while
// open, so the system Back closes it as its Cancel or X would.
describe("a dialog on a phone's Back", () => {
    test.each([
        ["a confirm", { confirmLabel: "Delete", isDestructive: true }],
        ["a panel drawn as a sheet", { isPanel: true, sheet: "phone" }],
        ["the photo viewer", { isPanel: true, isMedia: true }]
    ])("%s holds one entry while open, and Back closes it", async (_what, props) => {
        const recorded = recordingBackStack();
        const isOpen = ref(true);
        const Host = defineComponent({
            setup() {
                return () => h(Dialog, { isOpen: isOpen.value, title: "Photo", ...props, onClose: () => {
                    isOpen.value = false;
                } });
            }
        });
        wrapper = mount(Host, { global: { provide: { ...providedSession({ isPhone: true, backStack: recorded.stack }) } }, attachTo: document.body });
        await nextTick();
        expect(recorded.held).toHaveLength(1);

        recorded.back();
        await nextTick();

        expect(isOpen.value).toBe(false);
        expect(recorded.held).toHaveLength(0);
    });
});

// A phone's panel drawn as a sheet is held at the frame's bottom, so a change of its content's
// height within a page, a list loading, an ask opening or a row leaving, moves its top edge and
// head. The sheet glides its edge with the content's own move instead of jumping; a frame change,
// as a keyboard rising, changes the height at once.
describe("a panel drawn as a sheet following its content's height", () => {
    type Observed = { callback: ResizeObserverCallback, targets: Element[] };
    let observers: Observed[] = [];
    const originalObserver = (globalThis as { ResizeObserver?: unknown }).ResizeObserver;

    beforeEach(() => {
        observers = [];
        (globalThis as { ResizeObserver?: unknown }).ResizeObserver = class {
            private readonly entry: Observed;
            constructor(callback: ResizeObserverCallback) {
                this.entry = { callback, targets: [] };
                observers.push(this.entry);
            }
            observe(target: Element): void {
                this.entry.targets.push(target);
            }
            unobserve(target: Element): void {
                this.entry.targets = this.entry.targets.filter(t => t !== target);
            }
            disconnect(): void {
                this.entry.targets = [];
            }
        };
    });

    afterEach(() => {
        (globalThis as { ResizeObserver?: unknown }).ResizeObserver = originalObserver;
    });

    /** Tells the observers a box changed size, as the browser's layout would. */
    function resized(target: Element, height: number): void {
        observers.filter(o => o.targets.includes(target)).forEach(o => o.callback([{ target, contentRect: { height } as DOMRectReadOnly } as ResizeObserverEntry], {} as ResizeObserver));
    }

    async function drawSheet(isPhone: boolean): Promise<{ dialog: HTMLElement, content: HTMLElement | null, heights: ReturnType<typeof standInHeights>, setHeight: (h: number) => void }> {
        const heights = standInHeights();
        let height = 300;
        wrapper = mount(Dialog, {
            props: { isOpen: true, title: "Chat settings", isPanel: true, sheet: "phone" },
            slots: { default: () => h("div", { "data-testid": "rows" }, "rows") },
            global: { provide: { ...providedSession({ isPhone }) } },
            attachTo: document.body
        });
        await nextTick();
        await nextTick();
        const dialog = wrapper.get("[data-testid='dialog']").element as HTMLElement;
        heights.size(dialog, () => height);
        dialog.style.setProperty("--chat-d-move", "240ms");
        const content = document.querySelector<HTMLElement>("[data-testid='rows']")?.parentElement ?? null;
        return { dialog, content, heights, setHeight: h => {
            height = h;
        } };
    }

    test("glides from its old height to its content's new one, over the content's own move", async () => {
        const { dialog, content, heights, setHeight } = await drawSheet(true);
        try {
            resized(dialog.parentElement as Element, 800);
            setHeight(360);
            resized(content as Element, 300);

            expect(heights.played).toEqual([expect.objectContaining({ element: dialog, from: "300px", to: "360px", duration: 240 })]);
        }
        finally {
            heights.restore();
        }
    });

    test("changes at once for a frame change, and the next content change glides from there", async () => {
        const { dialog, content, heights, setHeight } = await drawSheet(true);
        try {
            setHeight(250);
            resized(dialog.parentElement as Element, 500);
            expect(heights.played).toEqual([]);

            setHeight(290);
            resized(content as Element, 280);
            expect(heights.played).toEqual([expect.objectContaining({ from: "250px", to: "290px" })]);
        }
        finally {
            heights.restore();
        }
    });

    test("changes at once under reduced motion", async () => {
        const original = window.matchMedia;
        window.matchMedia = ((query: string) => ({ media: query, matches: query === "(prefers-reduced-motion: reduce)" })) as unknown as typeof window.matchMedia;
        const { dialog, content, heights, setHeight } = await drawSheet(true);
        try {
            resized(dialog.parentElement as Element, 800);
            setHeight(360);
            resized(content as Element, 300);

            expect(heights.played).toEqual([]);
        }
        finally {
            heights.restore();
            window.matchMedia = original;
        }
    });

    // A turn plays its own height change over the panel's duration; the content box's change it
    // brings is that same change, so the sheet does not play it a second time.
    test("plays a turn's own content change once", async () => {
        const heights = standInHeights();
        try {
            const isBlocked = ref(false);
            const Host = defineComponent({
                setup() {
                    return () => h(Dialog, { isOpen: true, title: "Chat settings", isPanel: true, placement: "top", sheet: "phone" }, {
                        default: () => h("button", { "data-testid": "to-blocked", onClick: () => {
                            isBlocked.value = true;
                        } }, "Blocked people"),
                        pages: () => h(SurfacePage, { isOpen: isBlocked.value, title: "Blocked people", onBack: () => {
                            isBlocked.value = false;
                        } }, () => h("p", "No one blocked"))
                    });
                }
            });
            wrapper = mount(Host, { global: { provide: { ...providedSession({ isPhone: true }) } }, attachTo: document.body });
            await nextTick();
            await nextTick();
            const dialog = document.querySelector<HTMLElement>("[data-testid='dialog']") as HTMLElement;
            const isTurned = (): boolean => !!dialog.querySelector("[data-testid='page-back']");
            heights.size(dialog, () => isTurned() ? 180 : 520);
            dialog.style.setProperty("--chat-d-move", "240ms");
            const content = dialog.querySelector<HTMLElement>(".chat-dialog-pages") as HTMLElement;
            content.getBoundingClientRect = (): DOMRect => ({ height: isTurned() ? 140 : 480 }) as DOMRect;
            resized(dialog.parentElement as Element, 800);

            document.querySelector<HTMLElement>("[data-testid='to-blocked']")?.click();
            await nextTick();
            await nextTick();
            resized(content, 140);

            expect(heights.played).toEqual([expect.objectContaining({ element: dialog, from: "520px", to: "180px", duration: panelMotion.duration })]);
        }
        finally {
            heights.restore();
        }
    });

    test("a desktop panel observes nothing", async () => {
        const { heights } = await drawSheet(false);
        heights.restore();

        expect(observers.flatMap(o => o.targets)).toEqual([]);
    });
});
