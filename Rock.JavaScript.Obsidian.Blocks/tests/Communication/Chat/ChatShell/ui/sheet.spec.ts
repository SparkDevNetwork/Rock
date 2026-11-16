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
import { defineComponent, h, nextTick, ref } from "vue";
import Sheet from "../../../../../src/Communication/Chat/ChatShell/ui/sheet.partial.obs";
import Scrim from "../../../../../src/Communication/Chat/ChatShell/ui/scrim.partial.obs";
import { provideAnnouncer, useAnnouncer } from "../../../../../src/Communication/Chat/ChatShell/ui/announcer.partial";
import { chatSheet } from "../statusStore";

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
        const wrapper = mount(Scrim, { attachTo: document.body });
        const scrim = find("scrim") as HTMLElement;
        scrim.dispatchEvent(new PointerEvent("pointerdown", { bubbles: true }));
        expect(wrapper.emitted("dismiss")).toBeUndefined();
        scrim.click();
        expect(wrapper.emitted("dismiss")).toHaveLength(1);
        wrapper.unmount();
    });

    it("ignores a click whose press began elsewhere, as a drag out of the sheet that ends on the scrim", () => {
        const wrapper = mount(Scrim, { attachTo: document.body });
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
        scrim.dispatchEvent(new PointerEvent("pointerdown", { bubbles: true }));
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
