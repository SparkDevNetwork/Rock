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
// The dot: one 8px mark. Something new for you is the church's colour (red is kept for failure);
// a connection being worked out is the warning colour, or the info colour while nothing is wrong
// yet, and pulses. A dot over a button or a pill is ringed in what it sits on. The words beside
// it, or the name of what holds it, say what it means, so it is hidden from a screen reader.
import { mount } from "@vue/test-utils";
import Dot from "../../../../../src/Communication/Chat/ChatShell/ui/dot.partial.obs";
import { partRuleBreaks, ruleOf, styleOf } from "./partStyle";

describe("the dot", () => {
    test("is the church's colour unless told otherwise, still, unringed and hidden from a screen reader", () => {
        const dot = mount(Dot);

        expect(dot.classes()).toEqual(expect.arrayContaining(["chat-dot", "is-brand"]));
        expect(dot.classes()).not.toContain("is-pulse");
        expect(dot.classes()).not.toContain("is-ringed");
        expect(dot.attributes("aria-hidden")).toBe("true");
    });

    test.each(["brand", "warning", "info"])("draws the %s tone", tone => {
        expect(mount(Dot, { props: { tone } }).classes()).toContain(`is-${tone}`);
    });

    test("pulses when asked, and is ringed when asked", () => {
        const dot = mount(Dot, { props: { pulse: true, ringed: true } });

        expect(dot.classes()).toEqual(expect.arrayContaining(["is-pulse", "is-ringed"]));
    });

    test("takes a class and test id from where it is drawn, for where the screen places it", () => {
        const dot = mount(Dot, { attrs: { class: "chat-x-corner", "data-testid": "x-dot" } });

        expect(dot.classes()).toContain("chat-x-corner");
        expect(dot.attributes("data-testid")).toBe("x-dot");
    });
});

describe("the dot's look", () => {
    const style = styleOf("dot.partial.obs");

    test("is 8px round, in the church's fill, the warning or the info colour", () => {
        expect(ruleOf(style, ".chat-dot")).toMatch(/width:\s*8px/);
        expect(ruleOf(style, ".chat-dot")).toMatch(/height:\s*8px/);
        expect(ruleOf(style, ".chat-dot")).toMatch(/border-radius:\s*var\(--chat-r-full\)/);
        expect(ruleOf(style, ".chat-dot.is-brand")).toMatch(/background:\s*var\(--chat-brand-fill\)/);
        expect(ruleOf(style, ".chat-dot.is-warning")).toMatch(/background:\s*var\(--chat-warning\)/);
        expect(ruleOf(style, ".chat-dot.is-info")).toMatch(/background:\s*var\(--chat-info\)/);
    });

    test("pulses with the set's pulse through the loop stop, which reduced motion sets to none", () => {
        expect(ruleOf(style, ".chat-dot.is-pulse")).toMatch(/animation:\s*var\(--chat-loop-stop, chat-pulse\) var\(--chat-d-pulse\)/);
    });

    test("a ring follows the fill under it, as a face's does", () => {
        expect(ruleOf(style, ".chat-dot.is-ringed")).toMatch(/box-shadow:\s*0 0 0 2px var\(--chat-av-ring, var\(--chat-surface\)\)/);
    });

    test("holds no motion, priority, reach or capitals of its own", () => {
        expect(partRuleBreaks("dot.partial.obs")).toEqual([]);
    });
});
