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
// The list row: a lead (a face or a mark), what the row says, a meta at its end (a time, a count,
// an emoji) and actions that show while the row is hovered or holds focus. A row that opens
// something is one button under the whole row, what is drawn sitting over it and letting every
// press through, so its actions stay buttons of their own rather than buttons inside a button.
import { mount, VueWrapper } from "@vue/test-utils";
import ListRow from "../../../../../src/Communication/Chat/ChatShell/ui/listRow.partial.obs";
import { mediaOf, outsideMedia, partRuleBreaks, ruleOf, styleOf } from "./partStyle";

const slots = {
    lead: "<span data-testid='lead'>A</span>",
    default: "<span data-testid='main'>Anna Lee</span>",
    meta: "<span data-testid='meta'>2m</span>",
    actions: "<button type='button' data-testid='action'>Message</button>"
};

describe("the list row", () => {
    let wrapper: VueWrapper | null = null;

    afterEach(() => {
        wrapper?.unmount();
        wrapper = null;
    });

    test("draws its lead, what it says, its meta and its actions in that order", () => {
        wrapper = mount(ListRow, { slots });
        const order = ["lead", "main", "meta", "action"].map(id => wrapper!.get(`[data-testid='${id}']`).element);

        for (let i = 1; i < order.length; i++) {
            expect(order[i - 1].compareDocumentPosition(order[i]) & Node.DOCUMENT_POSITION_FOLLOWING).toBeTruthy();
        }
        expect(wrapper.get("[data-testid='action']").element.closest(".chat-lrow-actions")).not.toBeNull();
    });

    test("leaves out the places it is given nothing for", () => {
        wrapper = mount(ListRow, { slots: { default: "Anna Lee" } });

        expect(wrapper.find(".chat-lrow-lead").exists()).toBe(false);
        expect(wrapper.find(".chat-lrow-meta").exists()).toBe(false);
        expect(wrapper.find(".chat-lrow-actions").exists()).toBe(false);
    });

    test("a row that only shows is no button and takes no hover", () => {
        wrapper = mount(ListRow, { slots });

        expect(wrapper.find(".chat-lrow-hit").exists()).toBe(false);
        expect(wrapper.classes()).not.toContain("is-interactive");
    });

    test("a row that opens something is one button under the whole row, named by what the row says", async () => {
        wrapper = mount(ListRow, { props: { interactive: true }, slots, attachTo: document.body });
        const hit = wrapper.get(".chat-lrow-hit");

        expect(hit.element.tagName).toBe("BUTTON");
        expect(hit.attributes("type")).toBe("button");
        expect(document.getElementById(hit.attributes("aria-labelledby") ?? "")?.textContent?.trim()).toBe("Anna Lee");
        expect(wrapper.classes()).toContain("is-interactive");

        await hit.trigger("click");
        expect(wrapper.emitted("press")).toHaveLength(1);
    });

    test("names its button as the screen asks, where what it says is not the whole name", () => {
        wrapper = mount(ListRow, { props: { interactive: true, label: "Anna Lee, 2 new" }, slots });

        expect(wrapper.get(".chat-lrow-hit").attributes("aria-label")).toBe("Anna Lee, 2 new");
        expect(wrapper.get(".chat-lrow-hit").attributes("aria-labelledby")).toBeUndefined();
    });

    test("an action's press is the action's, not the row's", async () => {
        wrapper = mount(ListRow, { props: { interactive: true }, slots });

        await wrapper.get("[data-testid='action']").trigger("click");

        expect(wrapper.emitted("press")).toBeUndefined();
    });

    test("a selected row is marked, and its button says it is the current one", () => {
        wrapper = mount(ListRow, { props: { interactive: true, selected: true }, slots });

        expect(wrapper.classes()).toContain("is-selected");
        expect(wrapper.get(".chat-lrow-hit").attributes("aria-current")).toBe("true");
    });

    test("is drawn as the element the list asks for, with its test id", () => {
        wrapper = mount(ListRow, { props: { as: "li" }, attrs: { "data-testid": "x-row" }, slots });

        expect(wrapper.element.tagName).toBe("LI");
        expect(wrapper.attributes("data-testid")).toBe("x-row");
    });
});

describe("the list row's look", () => {
    const style = styleOf("listRow.partial.obs");

    test("one hover tint for a row that opens something, and the church's wash for the selected one", () => {
        expect(ruleOf(style, ".chat-lrow.is-interactive:hover")).toMatch(/--chat-lrow-fill:\s*var\(--chat-hover\)/);
        expect(ruleOf(style, ".chat-lrow.is-selected")).toMatch(/--chat-lrow-fill:\s*var\(--chat-brand-wash\)/);
        expect(ruleOf(style, ".chat-lrow")).toMatch(/background:\s*var\(--chat-lrow-fill\)/);
        expect(ruleOf(style, ".chat-lrow")).toMatch(/border-radius:\s*var\(--chat-r-row\)/);
    });

    test("tints under a pointer that can hover, so a tap on a touch screen leaves no tint behind", () => {
        expect(ruleOf(mediaOf(style, "(hover: hover)"), ".chat-lrow.is-interactive:hover")).toMatch(/--chat-lrow-fill:\s*var\(--chat-hover\)/);
        expect(ruleOf(outsideMedia(style), ".chat-lrow.is-interactive:hover")).toBe("");
    });

    test("a face in the row rings itself in the row's own fill, so the ring matches what is behind it", () => {
        expect(ruleOf(style, ".chat-lrow")).toMatch(/--chat-av-ring-over:\s*var\(--chat-lrow-fill\)/);
    });

    test("its actions show while it is hovered or holds focus, and always where nothing can hover", () => {
        expect(ruleOf(style, ".chat-lrow-actions")).toMatch(/opacity:\s*0/);
        expect(ruleOf(style, ".chat-lrow:hover .chat-lrow-actions")).toMatch(/opacity:\s*1/);
        expect(ruleOf(style, ".chat-lrow:focus-within .chat-lrow-actions")).toMatch(/opacity:\s*1/);
        expect(style).toMatch(/@media \(hover: none\)\s*\{\s*\.chat-lrow-actions\s*\{\s*opacity:\s*1;?\s*\}/);
    });

    test("holds no motion, priority, reach or capitals of its own", () => {
        expect(partRuleBreaks("listRow.partial.obs")).toEqual([]);
    });
});
