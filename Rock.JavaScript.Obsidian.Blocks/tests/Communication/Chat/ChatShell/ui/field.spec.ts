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
// The text field, the one look for every search box and name field: a bordered box with 12px
// corners, an optional icon before the words, a close that clears it, and a helper line under it
// that a screen reader hears with the field. What the screen hands it (a name, a test id, keys)
// lands on the input; a class lands on the box, for where the screen lays it out.
import { mount, VueWrapper } from "@vue/test-utils";
import Field from "../../../../../src/Communication/Chat/ChatShell/ui/field.partial.obs";
import Icon from "../../../../../src/Communication/Chat/ChatShell/ui/icon.partial.obs";
import { mediaOf, outsideMedia, partRuleBreaks, ruleOf, styleOf } from "./partStyle";

describe("the text field", () => {
    let wrapper: VueWrapper | null = null;

    afterEach(() => {
        wrapper?.unmount();
        wrapper = null;
    });

    function draw(props: Record<string, unknown> = {}, attrs: Record<string, unknown> = {}): VueWrapper {
        wrapper = mount(Field, { props: { modelValue: "", ...props }, attrs, attachTo: document.body });
        return wrapper;
    }

    test("shows its value and placeholder in a text input, and says each change", async () => {
        const input = draw({ modelValue: "gr", placeholder: "Search" }).get("input");

        expect((input.element as HTMLInputElement).value).toBe("gr");
        expect(input.attributes("type")).toBe("text");
        expect(input.attributes("placeholder")).toBe("Search");

        await input.setValue("gra");

        expect(wrapper!.emitted("update:modelValue")).toEqual([["gra"]]);
    });

    test("takes another input type", () => {
        expect(draw({ type: "search" }).get("input").attributes("type")).toBe("search");
    });

    test("draws its leading icon before the input, and none unless given one", () => {
        expect(draw().findComponent(Icon).exists()).toBe(false);
        wrapper!.unmount();

        const icon = draw({ leadingIcon: "search" }).findComponent(Icon);
        expect(icon.props("name")).toBe("search");
    });

    test("puts the screen's name, test id and key handlers on the input, and its class on the box", async () => {
        const onKeydown = jest.fn();
        draw({}, { class: "chat-x-search", "aria-label": "Find a conversation", "data-testid": "x-search", onKeydown });
        const input = wrapper!.get("input");

        await input.trigger("keydown", { key: "ArrowDown" });

        expect(input.attributes("aria-label")).toBe("Find a conversation");
        expect(input.attributes("data-testid")).toBe("x-search");
        expect(input.classes()).not.toContain("chat-x-search");
        expect(wrapper!.classes()).toContain("chat-x-search");
        expect(onKeydown).toHaveBeenCalledTimes(1);
    });

    describe("clearable", () => {
        test("shows its close only while it holds words", async () => {
            draw({ clearable: true });
            expect(wrapper!.find("button").exists()).toBe(false);

            await wrapper!.setProps({ modelValue: "gr" });

            expect(wrapper!.get("button").attributes("aria-label")).toBe("Clear");
        });

        test("names its close as the screen asks", () => {
            draw({ clearable: true, modelValue: "gr", clearLabel: "Clear search" });

            expect(wrapper!.get("button").attributes("aria-label")).toBe("Clear search");
        });

        test("clearing empties the value, says so, and puts focus back in the input", async () => {
            draw({ clearable: true, modelValue: "gr" });

            await wrapper!.get("button").trigger("click");

            expect(wrapper!.emitted("update:modelValue")).toEqual([[""]]);
            expect(wrapper!.emitted("clear")).toHaveLength(1);
            expect(document.activeElement).toBe(wrapper!.get("input").element);
        });
    });

    test("when invalid says so to a screen reader and marks the box", () => {
        draw({ invalid: true });

        expect(wrapper!.get("input").attributes("aria-invalid")).toBe("true");
        expect(wrapper!.classes()).toContain("is-invalid");
    });

    test("is not marked invalid unless told", () => {
        expect(draw().get("input").attributes("aria-invalid")).toBeUndefined();
    });

    test("draws its helper line under the box, which describes the input", () => {
        draw({ helper: "Up to 50 characters." });
        const helper = wrapper!.get(".chat-fld-helper");

        expect(helper.text()).toBe("Up to 50 characters.");
        expect(wrapper!.get("input").attributes("aria-describedby")).toBe(helper.attributes("id"));
    });

    test("keeps a description the screen gives beside its own helper line", () => {
        draw({ helper: "Up to 50 characters." }, { "aria-describedby": "x-error" });
        const helperId = wrapper!.get(".chat-fld-helper").attributes("id");

        expect(wrapper!.get("input").attributes("aria-describedby")).toBe(`x-error ${helperId}`);
    });

    test("holds what a screen puts in its content in the input's place, such as a chip editor", () => {
        wrapper = mount(Field, { props: { modelValue: "" }, slots: { content: "<div data-testid='chips'>Ann</div>" } });

        expect(wrapper.find("input").exists()).toBe(false);
        expect(wrapper.get("[data-testid='chips']").text()).toBe("Ann");
    });

    test("focuses its input when asked", () => {
        draw();
        (wrapper!.vm as unknown as { focus: () => void }).focus();

        expect(document.activeElement).toBe(wrapper!.get("input").element);
    });

    test("is 36px tall, or 44px at the large size", () => {
        draw({ size: "lg" });

        expect(wrapper!.classes()).toContain("is-lg");
    });
});

describe("the text field's look", () => {
    const style = styleOf("field.partial.obs");

    test("is a box with a one-pixel line and 12px corners, 36px tall, 44px large, in the interface's type size", () => {
        const box = ruleOf(style, ".chat-fld-box");

        expect(box).toMatch(/border:\s*1px solid var\(--chat-line-strong\)/);
        expect(box).toMatch(/border-radius:\s*var\(--chat-r-field\)/);
        expect(box).toMatch(/min-height:\s*36px/);
        expect(box).toMatch(/font-size:\s*var\(--chat-fs-ui\)/);
        expect(ruleOf(style, ".chat-fld.is-lg .chat-fld-box")).toMatch(/min-height:\s*44px/);
    });

    test("rings the box in the church's focus color while the input has focus, and in danger while invalid", () => {
        expect(ruleOf(style, ".chat-fld-box:focus-within")).toMatch(/border-color:\s*var\(--chat-focus-line\)/);
        expect(ruleOf(style, ".chat-fld-box:focus-within")).toMatch(/box-shadow:\s*0 0 0 3px var\(--chat-brand-ring\)/);
        expect(ruleOf(style, ".chat-fld.is-invalid .chat-fld-box")).toMatch(/border-color:\s*var\(--chat-danger\)/);
    });

    test("darkens its line under a pointer that can hover, so a tap on a touch screen leaves no hover behind", () => {
        expect(ruleOf(mediaOf(style, "(hover: hover)"), ".chat-fld-box:hover")).toMatch(/border-color:\s*var\(--chat-icon\)/);
        expect(ruleOf(outsideMedia(style), ".chat-fld-box:hover")).toBe("");
    });

    test("holds no motion, priority, reach or capitals of its own", () => {
        expect(partRuleBreaks("field.partial.obs")).toEqual([]);
    });
});
