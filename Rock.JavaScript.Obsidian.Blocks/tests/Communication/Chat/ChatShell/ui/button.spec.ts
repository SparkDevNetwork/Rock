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
// The text button, the one look for every button with words: four kinds (the church's fill, a
// line, a ghost and danger), two heights, 8px corners. A busy button keeps its focus and says it
// is busy, rather than being disabled, since a browser drops focus from a disabled button.
import { mount, VueWrapper } from "@vue/test-utils";
import Button from "../../../../../src/Communication/Chat/ChatShell/ui/button.partial.obs";
import Icon from "../../../../../src/Communication/Chat/ChatShell/ui/icon.partial.obs";
import { partRuleBreaks, ruleOf, styleOf } from "./partStyle";

function draw(props: Record<string, unknown> = {}, label = "Join channel"): VueWrapper {
    return mount(Button, { props, slots: { default: label }, attachTo: document.body });
}

describe("the text button", () => {
    let wrapper: VueWrapper | null = null;

    afterEach(() => {
        wrapper?.unmount();
        wrapper = null;
    });

    test("is a plain button of type button that shows its words, so it never submits a form", () => {
        wrapper = draw();
        const button = wrapper.get("button");

        expect(button.attributes("type")).toBe("button");
        expect(button.text()).toBe("Join channel");
    });

    test.each(["brand", "line", "ghost", "danger"])("draws the %s kind", kind => {
        wrapper = draw({ kind });

        expect(wrapper.get("button").classes()).toContain(`is-${kind}`);
    });

    test("is the line kind at the middle height unless told otherwise", () => {
        wrapper = draw();

        expect(wrapper.get("button").classes()).toEqual(expect.arrayContaining(["chat-btn", "is-line", "is-md"]));
    });

    // The way forward from a failure, such as a failed send's Try again, beside a plain Delete: a
    // line button in the danger ink, where the filled danger kind would say it destroys something.
    test("draws its words in the danger ink in the danger tone, keeping its kind's line", () => {
        wrapper = draw({ tone: "danger" }, "Try again");

        expect(wrapper.get("button").classes()).toEqual(expect.arrayContaining(["is-line", "is-tone-danger"]));
        expect(ruleOf(styleOf("button.partial.obs"), ".chat-btn.is-tone-danger")).toMatch(/color:\s*var\(--chat-danger-ink\)/);
    });

    test("draws an icon before its words when given one, at the small icon size", () => {
        wrapper = draw({ icon: "search" });
        const icon = wrapper.findComponent(Icon);

        expect(icon.props("name")).toBe("search");
        expect(icon.props("size")).toBe("sm");
        expect(wrapper.get("button").element.firstElementChild).toBe(icon.element);
    });

    test("passes a click on", async () => {
        wrapper = draw();

        await wrapper.get("button").trigger("click");

        expect(wrapper.emitted("click")).toHaveLength(1);
    });

    test("while busy keeps its focus, says it is busy, shows a spinner in its icon's place, and passes no click on", async () => {
        wrapper = draw({ busy: true, icon: "search" });
        const button = wrapper.get("button");
        (button.element as HTMLButtonElement).focus();

        await button.trigger("click");

        expect(button.attributes("disabled")).toBeUndefined();
        expect(button.attributes("aria-disabled")).toBe("true");
        expect(button.attributes("aria-busy")).toBe("true");
        expect(button.find(".chat-spinner").exists()).toBe(true);
        expect(wrapper.findComponent(Icon).exists()).toBe(false);
        expect(document.activeElement).toBe(button.element);
        expect(wrapper.emitted("click")).toBeUndefined();
    });

    // "Join channel" turning to a spinner and "Joining" was 14px narrower, so its end moved while
    // the door was out: busy holds the width the button had at rest, and lets it go when done.
    test("keeps the width it had at rest while busy, however short its busy words, and lets it go after", async () => {
        wrapper = draw({}, "Join channel");
        const button = wrapper.get("button");
        jest.spyOn(button.element, "getBoundingClientRect").mockReturnValue({ width: 115.5, height: 32 } as DOMRect);

        await wrapper.setProps({ busy: true });

        expect((button.element as HTMLElement).style.minWidth).toBe("115.5px");

        await wrapper.setProps({ busy: false });

        expect((button.element as HTMLElement).style.minWidth).toBe("");
    });

    test("when disabled is a disabled button, and not busy", () => {
        wrapper = draw({ disabled: true });
        const button = wrapper.get("button");

        expect(button.attributes("disabled")).toBeDefined();
        expect(button.attributes("aria-busy")).toBeUndefined();
    });

    test("is reached by Tab and pressed by Enter or Space, as a native button is", () => {
        wrapper = draw();

        expect(wrapper.get("button").attributes("tabindex")).toBeUndefined();
        expect(wrapper.get("button").element.tagName).toBe("BUTTON");
    });

    test("fills its row when block, and takes a class and test id from where it is drawn", () => {
        wrapper = mount(Button, { props: { block: true }, attrs: { class: "chat-x-foot", "data-testid": "x-save" }, slots: { default: "Save" } });
        const button = wrapper.get("button");

        expect(button.classes()).toEqual(expect.arrayContaining(["is-block", "chat-x-foot"]));
        expect(button.attributes("data-testid")).toBe("x-save");
    });

    test("focuses itself when asked, for a dialog landing on it", () => {
        wrapper = draw();
        (wrapper.vm as unknown as { focus: () => void }).focus();

        expect(document.activeElement).toBe(wrapper.get("button").element);
    });
});

describe("the text button's look", () => {
    const style = styleOf("button.partial.obs");

    test("has 8px corners, 32px at the middle height and 28px small, and the interface's type size", () => {
        expect(ruleOf(style, ".chat-btn")).toMatch(/border-radius:\s*var\(--chat-r-control\)/);
        expect(ruleOf(style, ".chat-btn")).toMatch(/height:\s*32px/);
        expect(ruleOf(style, ".chat-btn.is-sm")).toMatch(/height:\s*28px/);
        expect(ruleOf(style, ".chat-btn")).toMatch(/font-size:\s*var\(--chat-fs-ui\)/);
    });

    // The design's third height, a dialog footer's: 36px with 16px sides.
    test("is 36px large, for a dialog's footer", () => {
        const w = mount(Button, { props: { size: "lg" }, slots: { default: "Leave" } });

        expect(w.classes()).toContain("is-lg");
        expect(ruleOf(style, ".chat-btn.is-lg")).toMatch(/height:\s*36px/);
        expect(ruleOf(style, ".chat-btn.is-lg")).toMatch(/padding:\s*0 16px/);
        w.unmount();
    });

    test("draws the church's fill, a one-pixel line, a ghost and danger from the token sheet", () => {
        expect(ruleOf(style, ".chat-btn.is-brand")).toMatch(/background:\s*var\(--chat-brand-fill\)/);
        expect(ruleOf(style, ".chat-btn.is-line")).toMatch(/border-color:\s*var\(--chat-line-strong\)/);
        expect(ruleOf(style, ".chat-btn")).toMatch(/border:\s*1px solid transparent/);
        expect(ruleOf(style, ".chat-btn.is-danger")).toMatch(/background:\s*var\(--chat-danger\)/);
        expect(ruleOf(style, ".chat-btn.is-ghost")).toMatch(/color:\s*var\(--chat-text-2\)/);
    });

    test("holds no motion, priority, reach or capitals of its own", () => {
        expect(partRuleBreaks("button.partial.obs")).toEqual([]);
    });
});
