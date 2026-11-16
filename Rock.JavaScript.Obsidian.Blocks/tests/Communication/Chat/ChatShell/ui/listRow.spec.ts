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
import { providedSession } from "../sessionShapes";

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

    // What a list's rows need of the row: their button's marks and the list's keys on it, words
    // that run on under the first line, the list leading when the actions show, a row kept faded,
    // and a phone, where the actions are always there.
    test("puts on its button what the list hands it, its keys among them, and hands the list the button", async () => {
        const onFocus = jest.fn();
        const hitRef = jest.fn();
        wrapper = mount(ListRow, {
            props: { interactive: true, hitAttrs: { "data-testid": "x-open", "aria-describedby": "x-hint", tabindex: -1, onFocus }, hitRef },
            slots,
            attachTo: document.body
        });
        const hit = wrapper.get(".chat-lrow-hit");

        expect(hit.attributes("data-testid")).toBe("x-open");
        expect(hit.attributes("aria-describedby")).toBe("x-hint");
        expect(hit.attributes("tabindex")).toBe("-1");
        (hit.element as HTMLElement).focus();
        expect(onFocus).toHaveBeenCalledTimes(1);
        expect(hitRef).toHaveBeenCalledWith(hit.element);
    });

    test("draws the lines that run on under its first line beneath it, and is marked stacked", () => {
        wrapper = mount(ListRow, { slots: { ...slots, detail: "<p data-testid='detail'>Two lines of the message</p>" } });

        expect(wrapper.get("[data-testid='detail']").element.closest(".chat-lrow-detail")).not.toBeNull();
        expect(wrapper.classes()).toContain("is-stacked");
        expect(wrapper.classes()).toContain("has-actions");
    });

    test("a row its button names is drawn for the eye alone: nothing drawn is read or reached", () => {
        wrapper = mount(ListRow, { props: { interactive: true, label: "Anna Lee, 2m" }, slots: { ...slots, detail: "<a href='#x'>a link</a>" } });

        for (const place of [".chat-lrow-lead", ".chat-lrow-main", ".chat-lrow-meta", ".chat-lrow-detail"]) {
            expect(wrapper.get(place).attributes("aria-hidden")).toBe("true");
            expect(wrapper.get(place).attributes("inert")).toBeDefined();
        }
        expect(wrapper.get(".chat-lrow-actions").attributes("aria-hidden")).toBeUndefined();
    });

    test("a row named by what it says keeps it readable", () => {
        wrapper = mount(ListRow, { props: { interactive: true }, slots });

        expect(wrapper.get(".chat-lrow-main").attributes("aria-hidden")).toBeUndefined();
        expect(wrapper.get(".chat-lrow-main").attributes("inert")).toBeUndefined();
    });

    test.each([
        [null, []],
        [true, ["is-led", "is-held"]],
        [false, ["is-led"]]
    ])("with its actions held out as %s, leaves them to its own hover or follows the list", (actionsShown, marks) => {
        wrapper = mount(ListRow, { props: { actionsShown }, slots });

        expect(wrapper.classes().filter(name => ["is-led", "is-held"].includes(name))).toEqual(marks);
    });

    test("its actions come and go at once while the list passes them from row to row", () => {
        wrapper = mount(ListRow, { props: { actionsShown: true, instant: true }, slots });

        expect(wrapper.classes()).toContain("is-instant");
    });

    test("a row kept in place while it is off or on its way out is drawn faded", () => {
        wrapper = mount(ListRow, { props: { dimmed: true }, slots });

        expect(wrapper.classes()).toContain("is-dimmed");
    });

    test.each([true, false])("on a phone (%s) its actions are always there", isPhone => {
        wrapper = mount(ListRow, { slots, global: { provide: providedSession({ isPhone }) } });

        expect(wrapper.classes().includes("is-phone")).toBe(isPhone);
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
        expect(ruleOf(style, ".chat-lrow:not(.is-led):hover .chat-lrow-actions")).toMatch(/opacity:\s*1/);
        expect(ruleOf(style, ".chat-lrow:focus-within .chat-lrow-actions")).toMatch(/opacity:\s*1/);
        expect(style).toMatch(/@media \(hover: none\)\s*\{\s*\.chat-lrow-actions\s*\{\s*opacity:\s*1;?\s*\}/);
    });

    test("its actions show on its own hover only where a pointer can hover and the list leaves them to it", () => {
        expect(ruleOf(mediaOf(style, "(hover: hover)"), ".chat-lrow:not(.is-led):hover .chat-lrow-actions")).toMatch(/opacity:\s*1/);
        expect(ruleOf(outsideMedia(style), ".chat-lrow:not(.is-led):hover .chat-lrow-actions")).toBe("");
    });

    test("its actions show while the list holds them out, a menu of the row is open, or on a phone, and pass at once when asked", () => {
        expect(ruleOf(style, ".chat-lrow.is-held .chat-lrow-actions")).toMatch(/opacity:\s*1/);
        expect(ruleOf(style, ".chat-lrow:has([aria-expanded=\"true\"]) .chat-lrow-actions")).toMatch(/opacity:\s*1/);
        expect(ruleOf(style, ".chat-lrow.is-phone .chat-lrow-actions")).toMatch(/opacity:\s*1/);
        expect(ruleOf(style, ".chat-lrow.is-instant .chat-lrow-actions")).toMatch(/transition:\s*none/);
    });

    test("the current row keeps the church's wash under the pointer", () => {
        expect(ruleOf(style, ".chat-lrow.is-selected:hover")).toMatch(/--chat-lrow-fill:\s*var\(--chat-brand-wash\)/);
    });

    test("a stacked row lays its lead beside every line, its meta beside the first and its lines under it as wide as both", () => {
        expect(ruleOf(style, ".chat-lrow.is-stacked")).toMatch(/display:\s*grid/);
        expect(ruleOf(style, ".chat-lrow.is-stacked > .chat-lrow-lead")).toMatch(/grid-row:\s*1 \/ span 2/);
        expect(ruleOf(style, ".chat-lrow.is-stacked > .chat-lrow-detail")).toMatch(/grid-column:\s*2 \/ -1/);
    });

    test("a stacked row's actions sit over its meta, which gives way while they show", () => {
        expect(ruleOf(style, ".chat-lrow.is-stacked > .chat-lrow-actions")).toMatch(/position:\s*absolute/);
        for (const shown of [".chat-lrow.is-stacked.has-actions:focus-within > .chat-lrow-meta", ".chat-lrow.is-stacked.has-actions.is-held > .chat-lrow-meta", ".chat-lrow.is-stacked.has-actions.is-phone > .chat-lrow-meta"]) {
            expect(ruleOf(style, shown)).toMatch(/opacity:\s*0/);
        }
        expect(ruleOf(mediaOf(style, "(hover: hover)"), ".chat-lrow.is-stacked.has-actions:not(.is-led):hover > .chat-lrow-meta")).toMatch(/opacity:\s*0/);
        expect(ruleOf(mediaOf(style, "(hover: none)"), ".chat-lrow.is-stacked.has-actions > .chat-lrow-meta")).toMatch(/opacity:\s*0/);
    });

    test("a faded row fades what it shows and keeps its actions whole", () => {
        expect(ruleOf(style, ".chat-lrow.is-dimmed > .chat-lrow-main")).toMatch(/opacity:\s*\.62/);
        expect(ruleOf(style, ".chat-lrow.is-dimmed > .chat-lrow-actions")).toBe("");
    });

    test("holds no motion, priority, reach or capitals of its own", () => {
        expect(partRuleBreaks("listRow.partial.obs")).toEqual([]);
    });
});
