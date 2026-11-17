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
import { providedSession } from "../sessionShapes";
import { shellFolder } from "../statusStore";
import { ruleOf, styleOf } from "./partStyle";

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
    async function turned(props: Record<string, unknown>, isPhone = false): Promise<{ isBlocked: { value: boolean }, opener: HTMLElement }> {
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

        wrapper = mount(Host, { global: { provide: { ...providedSession({ isPhone }) } }, attachTo: document.body });
        await nextTick();
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
        expect(dialog.querySelector("[data-testid='settings-close']")).toBeNull();
        const title = document.getElementById(dialog.getAttribute("aria-labelledby") ?? "");
        expect(title?.textContent).toBe("Blocked people");
        expect(dialog.contains(document.querySelector("[data-testid='unblock']"))).toBe(true);
        expect(document.activeElement).toBe(back);

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
});
