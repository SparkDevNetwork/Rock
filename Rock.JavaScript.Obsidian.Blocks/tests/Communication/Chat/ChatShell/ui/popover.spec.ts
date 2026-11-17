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
// The popover, the one way the chat opens a floating surface beside what opened it: placed by
// floating-ui, closed by a press outside it, by Escape and by Tab past its end, grown from its
// anchor, and giving focus back to what opened it when the person leaves it by key. Every menu,
// picker, card and tip used to do this its own way, and each way differed a little; these cells
// hold the one way, including the cases the copies got wrong.
import { mount, VueWrapper } from "@vue/test-utils";
import { defineComponent, h, nextTick, PropType, ref } from "vue";
import { isFloatingLayerOpen, PopoverControl, PopoverOptions, usePopover } from "../../../../../src/Communication/Chat/ChatShell/ui/popover.partial";
import Popover from "../../../../../src/Communication/Chat/ChatShell/ui/popoverSurface.partial.obs";
import Dialog from "../../../../../src/Communication/Chat/ChatShell/ui/dialog.partial.obs";
import { ruleOf, styleOf } from "./partStyle";

/** A button that opens a popover holding two buttons, or none, and a button outside it. */
const Host = defineComponent({
    props: {
        name: { type: String, default: "a" },
        options: { type: Object as PropType<PopoverOptions>, default: () => ({}) },
        isEmpty: { type: Boolean, default: false }
    },
    setup(props, { expose }) {
        const anchor = ref<HTMLElement>();
        const floating = ref<HTMLElement>();
        const pop = usePopover(anchor, floating, props.options);
        expose({ pop });

        return () => [
            h("button", { ref: anchor, "data-testid": `${props.name}-anchor`, onClick: () => void pop.toggle() }, "open"),
            pop.isOpen.value
                ? h("div", { ref: floating, "data-testid": `${props.name}-floating`, tabindex: -1, style: pop.floatingStyles.value }, props.isEmpty ? [] : [
                    h("button", { "data-testid": `${props.name}-first` }, "first"),
                    h("button", { "data-testid": `${props.name}-last` }, "last")
                ])
                : null,
            h("button", { "data-testid": `${props.name}-outside` }, "outside")
        ];
    }
});

function find(id: string): HTMLElement {
    const element = document.querySelector<HTMLElement>(`[data-testid='${id}']`);
    if (!element) {
        throw new Error(`no ${id}`);
    }

    return element;
}

function press(element: Element): void {
    element.dispatchEvent(new Event("pointerdown", { bubbles: true, cancelable: true }));
}

function key(name: string, options: KeyboardEventInit = {}): KeyboardEvent {
    const event = new KeyboardEvent("keydown", { key: name, bubbles: true, cancelable: true, ...options });
    (document.activeElement ?? document.body).dispatchEvent(event);
    return event;
}

async function settle(): Promise<void> {
    await nextTick();
    await nextTick();
}

describe("the popover", () => {
    const wrappers: VueWrapper[] = [];

    function host(props: Record<string, unknown> = {}): PopoverControl {
        const wrapper = mount(Host, { props, attachTo: document.body });
        wrappers.push(wrapper);
        return (wrapper.vm as unknown as { pop: PopoverControl }).pop;
    }

    beforeAll(() => {
        if (typeof window.ResizeObserver === "undefined") {
            window.ResizeObserver = class {
                observe(): void { /* jsdom lays nothing out. */ }
                unobserve(): void { /* As above. */ }
                disconnect(): void { /* As above. */ }
            } as unknown as typeof ResizeObserver;
        }
    });

    afterEach(() => {
        wrappers.splice(0).forEach(wrapper => wrapper.unmount());
        document.body.innerHTML = "";
    });

    it("opens, and says so once its surface is drawn", async () => {
        const pop = host();
        expect(await pop.open()).toBe(true);
        expect(pop.isOpen.value).toBe(true);
        expect(document.querySelector("[data-testid='a-floating']")).not.toBeNull();
    });

    // A surface whose own size changes, as the @ picker's when its rows do, is placed again before
    // the frame is painted, rather than drawing one frame where the old size put it.
    it("is placed again at once when asked, from where its anchor is now", async () => {
        // a window 1000 by 800, which jsdom does not lay out
        Object.defineProperty(document.documentElement, "clientWidth", { configurable: true, value: 1000 });
        Object.defineProperty(document.documentElement, "clientHeight", { configurable: true, value: 800 });
        const pop = host({ options: { placement: "bottom-start", offset: 0 } });
        await pop.open();
        await settle();
        const anchor = find("a-anchor");
        anchor.getBoundingClientRect = (): DOMRect => ({ x: 120, y: 40, left: 120, top: 40, right: 160, bottom: 60, width: 40, height: 20, toJSON: () => ({}) }) as DOMRect;

        pop.update();
        await new Promise(resolve => setTimeout(resolve, 0));
        await settle();

        expect(pop.floatingStyles.value.left).toBe("120px");
        expect(pop.floatingStyles.value.top).toBe("60px");
    });

    it("closed while it was still opening, stays closed and says the open did not land", async () => {
        const pop = host();
        const opening = pop.open();
        pop.close();
        expect(await opening).toBe(false);
        await settle();
        expect(pop.isOpen.value).toBe(false);
        expect(document.querySelector("[data-testid='a-floating']")).toBeNull();

        // the listeners it took on opening went with it: a later press elsewhere throws nothing and opens nothing
        press(find("a-outside"));
        expect(pop.isOpen.value).toBe(false);
    });

    it("closes on a press outside it, leaving focus where the press put it", async () => {
        const pop = host();
        find("a-anchor").focus();
        await pop.open();
        find("a-first").focus();
        find("a-outside").focus();
        press(find("a-outside"));
        await settle();
        expect(pop.isOpen.value).toBe(false);
        expect(document.activeElement).toBe(find("a-outside"));
    });

    it("stays open on a press inside it, and leaves a press on its anchor to the anchor", async () => {
        const pop = host();
        await pop.open();
        press(find("a-first"));
        press(find("a-anchor"));
        await settle();
        expect(pop.isOpen.value).toBe(true);
    });

    it("listens for the press as it is captured, so a part that stops it on its way up cannot keep the popover open", async () => {
        const pop = host();
        await pop.open();
        const outside = find("a-outside");
        outside.addEventListener("pointerdown", event => event.stopPropagation());
        press(outside);
        await settle();
        expect(pop.isOpen.value).toBe(false);
    });

    it("closes on Escape and gives focus back to what opened it", async () => {
        const pop = host();
        find("a-anchor").focus();
        await pop.open();
        find("a-first").focus();
        const event = key("Escape");
        await settle();
        expect(pop.isOpen.value).toBe(false);
        expect(event.defaultPrevented).toBe(true);
        expect(document.activeElement).toBe(find("a-anchor"));
    });

    it("gives focus back to the element it was told to, over what had focus", async () => {
        const target = document.createElement("button");
        document.body.appendChild(target);
        const pop = host({ options: { returnTo: () => target } });
        find("a-anchor").focus();
        await pop.open();
        key("Escape");
        await settle();
        expect(document.activeElement).toBe(target);
    });

    it("gives focus to nothing that has left the page", async () => {
        const pop = host();
        const opener = document.createElement("button");
        document.body.appendChild(opener);
        opener.focus();
        await pop.open();
        find("a-first").focus();
        opener.remove();
        pop.close({ isFocusReturned: true });
        await settle();
        expect(document.activeElement).not.toBe(opener);
    });

    it("closes only the innermost of two open popovers on Escape, then the outer on the next", async () => {
        const outer = host({ name: "outer" });
        const inner = host({ name: "inner" });
        await outer.open();
        await inner.open();
        find("inner-first").focus();
        key("Escape");
        await settle();
        expect(inner.isOpen.value).toBe(false);
        expect(outer.isOpen.value).toBe(true);
        key("Escape");
        await settle();
        expect(outer.isOpen.value).toBe(false);
    });

    it("counts a press inside a popover opened from it as inside, so a nested popover's choice does not close its parent", async () => {
        const outer = host({ name: "outer" });
        const inner = host({ name: "inner" });
        await outer.open();
        await inner.open();
        press(find("inner-first"));
        await settle();
        expect(outer.isOpen.value).toBe(true);
        expect(inner.isOpen.value).toBe(true);
    });

    // Popovers and dialogs stand on one Escape stack: the newest open surface takes the key,
    // whichever kind it is, and the one under it keeps it for the next press.
    describe("beside a dialog", () => {
        function dialog(): VueWrapper {
            const wrapper = mount(Dialog, { props: { isOpen: true, title: "Leave Volunteers?" }, attachTo: document.body });
            wrappers.push(wrapper);
            return wrapper;
        }

        it("lets a dialog opened over an open popover take Escape alone, and leaves the popover the next", async () => {
            const pop = host();
            await pop.open();
            const confirm = dialog();
            await settle();
            find("dialog-cancel").focus();

            key("Escape");
            await settle();
            expect(confirm.emitted("close")).toHaveLength(1);
            expect(pop.isOpen.value).toBe(true);

            await confirm.setProps({ isOpen: false });
            await settle();
            find("a-first").focus();
            key("Escape");
            await settle();
            expect(pop.isOpen.value).toBe(false);
        });

        it("lets a popover opened over an open dialog take Escape alone", async () => {
            const confirm = dialog();
            await settle();
            const pop = host();
            await pop.open();
            find("a-first").focus();

            key("Escape");
            await settle();
            expect(pop.isOpen.value).toBe(false);
            expect(confirm.emitted("close")).toBeUndefined();
        });
    });

    it("leaves Escape alone while the person is composing a word", async () => {
        const pop = host();
        await pop.open();
        find("a-first").focus();
        key("Escape", { isComposing: true });
        await settle();
        expect(pop.isOpen.value).toBe(true);
    });

    it("on Tab from a popover with nothing to tab to, closes and hands focus to its anchor so the browser moves on from there", async () => {
        const pop = host({ isEmpty: true });
        await pop.open();
        find("a-floating").focus();
        const event = key("Tab");
        await settle();
        expect(pop.isOpen.value).toBe(false);
        expect(document.activeElement).toBe(find("a-anchor"));
        expect(event.defaultPrevented).toBe(false);
    });

    it("lets Tab move between its own controls, and closes on Tab past the last", async () => {
        const pop = host();
        await pop.open();
        find("a-first").focus();
        expect(key("Tab").defaultPrevented).toBe(false);
        expect(pop.isOpen.value).toBe(true);
        find("a-last").focus();
        key("Tab");
        await settle();
        expect(pop.isOpen.value).toBe(false);
        expect(document.activeElement).toBe(find("a-anchor"));
    });

    it("toggles: asked again by its anchor, it closes", async () => {
        const pop = host();
        await pop.toggle();
        expect(pop.isOpen.value).toBe(true);
        await pop.toggle();
        expect(pop.isOpen.value).toBe(false);
    });

    it("grows from its anchor: from below when it opens above, from above when it opens below", async () => {
        const above = host({ name: "above", options: { placement: "top-start" } });
        const below = host({ name: "below", options: { placement: "bottom-end" } });
        await above.open();
        await below.open();
        expect(above.origin.value["--chat-pop-toward"]).toBe("1");
        expect(above.origin.value.transformOrigin).toMatch(/ 100%$/);
        expect(below.origin.value["--chat-pop-toward"]).toBe("-1");
        expect(below.origin.value.transformOrigin).toMatch(/ 0$/);
    });

    it("as a sheet, is not placed beside its anchor and leaves a press outside to the sheet's scrim", async () => {
        const pop = host({ options: { isSheet: () => true } });
        await pop.open();
        expect(pop.floatingStyles.value).toEqual({});
        press(find("a-outside"));
        await settle();
        expect(pop.isOpen.value).toBe(true);
    });

    it("places beside a rect it is given rather than its anchor's own, as a card under a whole message row", async () => {
        const rect = { x: 0, y: 0, top: 0, left: 0, right: 10, bottom: 10, width: 10, height: 10, toJSON: () => ({}) } as DOMRect;
        const asked = jest.fn(() => rect);
        const pop = host({ options: { virtualAnchor: asked } });
        await pop.open();
        await new Promise(resolve => setTimeout(resolve, 0));
        expect(asked).toHaveBeenCalled();
    });
});

// One floating layer at a time, so two never cover each other: while any popover, menu or card is open, a message's bar
// rises for no hover. A tip, which shows on its own timers and takes no press, is not one.
describe("the open layer", () => {
    const wrappers: VueWrapper[] = [];

    afterEach(() => {
        wrappers.splice(0).forEach(wrapper => wrapper.unmount());
        document.body.innerHTML = "";
    });

    function host(props: Record<string, unknown> = {}): PopoverControl {
        const wrapper = mount(Host, { props, attachTo: document.body });
        wrappers.push(wrapper);
        return (wrapper.vm as unknown as { pop: PopoverControl }).pop;
    }

    test("is open while a popover is, and shut once each closes, however it closes", async () => {
        const menu = host({ name: "menu" });
        const card = host({ name: "card" });
        const tip = host({ name: "tip", options: { isDismissable: false } });
        expect(isFloatingLayerOpen()).toBe(false);

        await tip.open();
        expect(isFloatingLayerOpen()).toBe(false);

        await menu.open();
        expect(isFloatingLayerOpen()).toBe(true);
        await card.open();
        menu.close();
        expect(isFloatingLayerOpen()).toBe(true);

        key("Escape");
        await settle();
        expect(card.isOpen.value).toBe(false);
        expect(isFloatingLayerOpen()).toBe(false);
    });
});

describe("the popover surface", () => {
    beforeAll(() => {
        if (typeof window.ResizeObserver === "undefined") {
            window.ResizeObserver = class {
                observe(): void { /* jsdom lays nothing out. */ }
                unobserve(): void { /* As above. */ }
                disconnect(): void { /* As above. */ }
            } as unknown as typeof ResizeObserver;
        }
    });

    afterEach(() => {
        document.body.innerHTML = "";
    });

    const SurfaceHost = defineComponent({
        props: { isSheet: { type: Boolean, default: false } },
        setup(props, { expose }) {
            const anchor = ref<HTMLElement>();
            const floating = ref<HTMLElement>();
            const pop = usePopover(anchor, floating, { isSheet: () => props.isSheet });
            expose({ pop });
            return () => [
                h("button", { ref: anchor, "data-testid": "anchor" }, "open"),
                h(Popover, { popover: pop, label: "Things" }, () => h("button", { "data-testid": "inside" }, "inside"))
            ];
        }
    });

    // One family of floating surfaces, each with the surface's own corners: the emoji tray is now
    // the message box's own layer, so no surface is drawn as a pill any more: the box is one 12px surface.
    it("draws every surface with the surface's own corners, never a pill", async () => {
        const wrapper = mount(SurfaceHost, { attachTo: document.body });
        const pop = (wrapper.vm as unknown as { pop: PopoverControl }).pop;
        await pop.open();
        const surface = find("popover");
        const style = styleOf("popoverSurface.partial.obs");

        expect(surface.classList.contains("is-pill")).toBe(false);
        expect(ruleOf(style, ".chat-popover")).toMatch(/border-radius:\s*var\(--chat-r-surface\)/);
        expect(style).not.toMatch(/is-pill|--chat-r-full/);
        wrapper.unmount();
    });

    it("draws the surface beside its anchor, named for a screen reader, only while open", async () => {
        const wrapper = mount(SurfaceHost, { attachTo: document.body });
        const pop = (wrapper.vm as unknown as { pop: PopoverControl }).pop;
        expect(document.querySelector("[data-testid='popover']")).toBeNull();
        await pop.open();
        const surface = document.querySelector<HTMLElement>("[data-testid='popover']");
        expect(surface?.getAttribute("aria-label")).toBe("Things");
        expect(surface?.classList.contains("chat-popover")).toBe(true);
        wrapper.unmount();
    });

    it("on a phone draws a sheet over a scrim instead, modal, and a tap on the scrim closes it with focus back on the anchor", async () => {
        const wrapper = mount(SurfaceHost, { props: { isSheet: true }, attachTo: document.body });
        const pop = (wrapper.vm as unknown as { pop: PopoverControl }).pop;
        find("anchor").focus();
        await pop.open();
        await settle();
        expect(document.querySelector("[data-testid='popover']")).toBeNull();
        const sheet = find("sheet");
        expect(sheet.getAttribute("aria-modal")).toBe("true");
        const scrim = find("scrim");
        press(scrim);
        scrim.click();
        await settle();
        expect(pop.isOpen.value).toBe(false);
        expect(document.activeElement).toBe(find("anchor"));
        wrapper.unmount();
    });
});
