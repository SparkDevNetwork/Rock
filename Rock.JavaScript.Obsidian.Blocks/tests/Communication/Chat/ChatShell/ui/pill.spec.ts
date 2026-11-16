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
// The chip: a 30px pill that is pressed to filter or to fill in a word. Off it is outlined; on it
// takes the church's tint, and says so as a pressed button. A chip that only acts, a suggested
// word, is never pressed and so carries no pressed state at all.
import { mount } from "@vue/test-utils";
import Pill from "../../../../../src/Communication/Chat/ChatShell/ui/pill.partial.obs";
import { partRuleBreaks, ruleOf, styleOf } from "./partStyle";

describe("the chip", () => {
    test("is a button of type button that shows its words", () => {
        const chip = mount(Pill, { slots: { default: "North campus" } });

        expect(chip.element.tagName).toBe("BUTTON");
        expect(chip.attributes("type")).toBe("button");
        expect(chip.text()).toBe("North campus");
        expect(chip.classes()).toContain("chat-chip");
    });

    test("says whether it is pressed when it filters", () => {
        expect(mount(Pill, { props: { selected: true } }).attributes("aria-pressed")).toBe("true");
        expect(mount(Pill, { props: { selected: false } }).attributes("aria-pressed")).toBe("false");
        expect(mount(Pill, { props: { selected: true } }).classes()).toContain("is-selected");
    });

    test("carries no pressed state when it only acts", () => {
        expect(mount(Pill).attributes("aria-pressed")).toBeUndefined();
    });

    test("passes a click on", async () => {
        const chip = mount(Pill);

        await chip.trigger("click");

        expect(chip.emitted("click")).toHaveLength(1);
    });

    test("takes its test id and data from where it is drawn", () => {
        const chip = mount(Pill, { attrs: { "data-testid": "x-chip", "data-campus": "north" } });

        expect(chip.attributes("data-testid")).toBe("x-chip");
        expect(chip.attributes("data-campus")).toBe("north");
    });
});

describe("the chip's look", () => {
    const style = styleOf("pill.partial.obs");

    test("is a 30px pill, outlined in one pixel off and tinted in the church's colour on, in the interface's type size", () => {
        expect(ruleOf(style, ".chat-chip")).toMatch(/height:\s*30px/);
        expect(ruleOf(style, ".chat-chip")).toMatch(/border-radius:\s*var\(--chat-r-full\)/);
        expect(ruleOf(style, ".chat-chip")).toMatch(/border:\s*1px solid var\(--chat-line-strong\)/);
        expect(ruleOf(style, ".chat-chip")).toMatch(/font-size:\s*var\(--chat-fs-ui\)/);
        expect(ruleOf(style, ".chat-chip.is-selected")).toMatch(/background:\s*var\(--chat-brand-tint\)/);
        expect(ruleOf(style, ".chat-chip.is-selected")).toMatch(/border-color:\s*var\(--chat-brand-line\)/);
    });

    test("holds no motion, priority, reach or capitals of its own", () => {
        expect(partRuleBreaks("pill.partial.obs")).toEqual([]);
    });
});
