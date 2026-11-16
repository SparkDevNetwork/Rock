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
// The segmented control: one choice of a few, as a radio group. One raised pill slides under the
// chosen option. Tab lands once, on the chosen option or the first where none is chosen; the
// arrows move and choose as they go, wrapping round. Each option may be described by a line of
// its own, which a screen reader hears as the arrows reach it.
import { mount, VueWrapper } from "@vue/test-utils";
import { ref } from "vue";
import Icon from "../../../../../src/Communication/Chat/ChatShell/ui/icon.partial.obs";
import Segmented from "../../../../../src/Communication/Chat/ChatShell/ui/segmented.partial.obs";
import { isPhoneKey } from "../../../../../src/Communication/Chat/ChatShell/ui/viewport.partial";
import { partRuleBreaks, ruleOf, styleOf } from "./partStyle";

const options = [
    { value: "all", label: "All", icon: "bell", description: "Every new message." },
    { value: "mentions", label: "Mentions", icon: "bell-at", description: "Your name or @all." },
    { value: "silent", label: "Off", icon: "bell-off", description: "Only mentions of your name." }
];

describe("the segmented control", () => {
    let wrapper: VueWrapper | null = null;

    afterEach(() => {
        wrapper?.unmount();
        wrapper = null;
    });

    function draw(modelValue: string | null, extra: Record<string, unknown> = {}, attrs: Record<string, unknown> = {}): VueWrapper {
        wrapper = mount(Segmented, { props: { options, modelValue, ...extra }, attrs, attachTo: document.body });
        return wrapper;
    }

    function radios(): HTMLElement[] {
        return wrapper!.findAll("[role='radio']").map(option => option.element as HTMLElement);
    }

    test("is a radio group of its options, each a button with its words, icon and value, the chosen one checked", () => {
        draw("mentions", {}, { "aria-label": "Notifications", "data-testid": "x-notify" });

        expect(wrapper!.attributes("role")).toBe("radiogroup");
        expect(wrapper!.attributes("aria-label")).toBe("Notifications");
        expect(wrapper!.attributes("data-testid")).toBe("x-notify");
        expect(radios().map(r => r.tagName)).toEqual(["BUTTON", "BUTTON", "BUTTON"]);
        expect(radios().map(r => r.getAttribute("type"))).toEqual(["button", "button", "button"]);
        expect(radios().map(r => r.textContent?.trim())).toEqual(["All", "Mentions", "Off"]);
        expect(radios().map(r => r.dataset.value)).toEqual(["all", "mentions", "silent"]);
        expect(wrapper!.findAllComponents(Icon).map(icon => icon.props("name"))).toEqual(["bell", "bell-at", "bell-off"]);
        expect(radios().map(r => r.getAttribute("aria-checked"))).toEqual(["false", "true", "false"]);
    });

    test("is one stop for Tab, on the chosen option", () => {
        draw("mentions");

        expect(radios().map(r => r.getAttribute("tabindex"))).toEqual(["-1", "0", "-1"]);
    });

    test("with nothing chosen, Tab lands on the first option, and none is checked", () => {
        draw(null);

        expect(radios().map(r => r.getAttribute("tabindex"))).toEqual(["0", "-1", "-1"]);
        expect(radios().map(r => r.getAttribute("aria-checked"))).toEqual(["false", "false", "false"]);
    });

    test("a click chooses the option, and choosing the chosen one again says nothing", async () => {
        draw("all");

        await wrapper!.findAll("[role='radio']")[2].trigger("click");
        await wrapper!.findAll("[role='radio']")[0].trigger("click");

        expect(wrapper!.emitted("update:modelValue")).toEqual([["silent"]]);
    });

    test.each([
        ["ArrowRight", "mentions", "silent"],
        ["ArrowDown", "mentions", "silent"],
        ["ArrowLeft", "mentions", "all"],
        ["ArrowUp", "mentions", "all"],
        ["ArrowRight", "silent", "all"],
        ["ArrowLeft", "all", "silent"]
    ])("%s from %s moves focus to %s and chooses it, wrapping round", async (key, from, to) => {
        draw(from);
        const at = options.findIndex(option => option.value === from);
        radios()[at].focus();

        await wrapper!.findAll("[role='radio']")[at].trigger("keydown", { key });

        expect(document.activeElement).toBe(radios()[options.findIndex(option => option.value === to)]);
        expect(wrapper!.emitted("update:modelValue")).toEqual([[to]]);
    });

    test("leaves any other key alone", async () => {
        draw("all");

        await wrapper!.findAll("[role='radio']")[0].trigger("keydown", { key: "a" });

        expect(wrapper!.emitted("update:modelValue")).toBeUndefined();
    });

    test("describes each option by its own line, read where the screen reader reaches it", () => {
        draw("all");

        expect(radios().map(r => document.getElementById(r.getAttribute("aria-describedby") ?? "")?.textContent?.trim())).toEqual([
            "Every new message.",
            "Your name or @all.",
            "Only mentions of your name."
        ]);
    });

    test("marks the option Tab lands on for a dialog to put focus on, when asked", () => {
        draw("mentions", { hasAutofocus: true });

        expect(radios().map(r => r.hasAttribute("data-autofocus"))).toEqual([false, true, false]);
    });

    test("marks no option for focus unless asked", () => {
        draw("mentions");

        expect(radios().some(r => r.hasAttribute("data-autofocus"))).toBe(false);
    });

    test("focuses the option Tab lands on when asked", () => {
        draw("silent");
        (wrapper!.vm as unknown as { focus: () => void }).focus();

        expect(document.activeElement).toBe(radios()[2]);
    });

    test("slides one pill under the chosen option, and draws none while nothing is chosen", async () => {
        draw("mentions");
        const pill = wrapper!.get(".chat-seg-pill").element as HTMLElement;

        expect(wrapper!.findAll(".chat-seg-pill")).toHaveLength(1);
        expect(pill.style.getPropertyValue("--chat-seg-i")).toBe("1");
        expect((wrapper!.element as HTMLElement).style.getPropertyValue("--chat-seg-n")).toBe("3");

        await wrapper!.setProps({ modelValue: null });
        expect(wrapper!.find(".chat-seg-pill").exists()).toBe(false);
    });

    test("gives each option a larger target on a phone, read from the session rather than handed in", () => {
        wrapper = mount(Segmented, { props: { options, modelValue: "all" }, global: { provide: { [isPhoneKey as symbol]: ref(true) } } });

        expect(wrapper.classes()).toContain("is-phone");
    });
});

describe("the segmented control's look", () => {
    const style = styleOf("segmented.partial.obs");

    test("is a pill track with a raised pill that slides on the thumb's time, which reduced motion makes instant", () => {
        expect(ruleOf(style, ".chat-seg")).toMatch(/background:\s*var\(--chat-tab-track\)/);
        expect(ruleOf(style, ".chat-seg")).toMatch(/border-radius:\s*var\(--chat-r-full\)/);
        expect(ruleOf(style, ".chat-seg-pill")).toMatch(/background:\s*var\(--chat-raised\)/);
        expect(ruleOf(style, ".chat-seg-pill")).toMatch(/transform:\s*translateX\(calc\(var\(--chat-seg-i\) \* 100%\)\)/);
        expect(ruleOf(style, ".chat-seg-pill")).toMatch(/transition:\s*transform var\(--chat-d-tab-thumb\)/);
    });

    test("each option is 32px tall, 40px on a phone", () => {
        expect(ruleOf(style, ".chat-seg-opt")).toMatch(/height:\s*32px/);
        expect(ruleOf(style, ".chat-seg.is-phone .chat-seg-opt")).toMatch(/height:\s*40px/);
    });

    test("holds no motion, priority, reach or capitals of its own", () => {
        expect(partRuleBreaks("segmented.partial.obs")).toEqual([]);
    });
});
