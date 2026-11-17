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
// The badge: a count, or a short label such as Leader or GIF. A mention count is the church's soft
// tint, a "+N" neutral, a tag such as Leader on the pill fill, a muted room's count quiet, a label
// over a photo on the photo's scrim. What it says is read from the row or button it sits in, so the badge itself is hidden
// from a screen reader.
import { mount } from "@vue/test-utils";
import Badge from "../../../../../src/Communication/Chat/ChatShell/ui/badge.partial.obs";
import Icon from "../../../../../src/Communication/Chat/ChatShell/ui/icon.partial.obs";
import { partRuleBreaks, ruleOf, styleOf } from "./partStyle";

describe("the badge", () => {
    test("draws its count in the church's tint unless told otherwise", () => {
        const badge = mount(Badge, { props: { count: 3 } });

        expect(badge.text()).toBe("3");
        expect(badge.classes()).toEqual(expect.arrayContaining(["chat-badge", "is-brand", "is-count"]));
    });

    test.each([[99, 99, "99"], [100, 99, "99+"], [12, 9, "9+"]])("draws %i with a most of %i as %s", (count, max, shown) => {
        expect(mount(Badge, { props: { count, max } }).text()).toBe(shown);
    });

    test("caps at 99 unless told otherwise", () => {
        expect(mount(Badge, { props: { count: 250 } }).text()).toBe("99+");
    });

    test.each(["brand", "neutral", "muted", "media", "tag"])("draws the %s tone", tone => {
        expect(mount(Badge, { props: { count: 2, tone } }).classes()).toContain(`is-${tone}`);
    });

    test("draws a label in place of a count, the label's shape and no count's", () => {
        const badge = mount(Badge, { props: { label: "GIF", tone: "media" } });

        expect(badge.text()).toBe("GIF");
        expect(badge.classes()).toContain("is-label");
        expect(badge.classes()).not.toContain("is-count");
    });

    test("draws an icon before what it says when given one", () => {
        const badge = mount(Badge, { props: { count: 2, icon: "at" } });

        expect(badge.findComponent(Icon).props("name")).toBe("at");
        expect(badge.text()).toBe("2");
    });

    test("is hidden from a screen reader, which hears the count from what holds it", () => {
        expect(mount(Badge, { props: { count: 2 } }).attributes("aria-hidden")).toBe("true");
    });

    // A role such as Leader or Announcement is words nothing else on the row says, so it is read
    // where it stands; a count or a GIF mark repeats what holds it, and stays hidden.
    test("reads its words when they say what nothing else does, as a role label's do", () => {
        const role = mount(Badge, { props: { label: "Leader", tone: "tag", isSpoken: true } });

        expect(role.attributes("aria-hidden")).toBeUndefined();
        expect(role.text()).toBe("Leader");
        expect(mount(Badge, { props: { label: "GIF", tone: "media" } }).attributes("aria-hidden")).toBe("true");
    });

    test("springs in as it appears when asked, the set's spring that reduced motion turns to a short fade", () => {
        expect(mount(Badge, { props: { count: 2, isPopping: true } }).classes()).toContain("is-pop");
        expect(ruleOf(styleOf("badge.partial.obs"), ".chat-badge.is-pop")).toMatch(/animation:\s*chat-spring var\(--chat-d-spring\)/);
    });

    test("takes a class and test id from where it is drawn", () => {
        const badge = mount(Badge, { props: { label: "GIF" }, attrs: { class: "chat-x-corner", "data-testid": "x-badge" } });

        expect(badge.classes()).toContain("chat-x-corner");
        expect(badge.attributes("data-testid")).toBe("x-badge");
    });
});

describe("the badge's look", () => {
    const style = styleOf("badge.partial.obs");

    test("a count is 20px tall in the badge's type size; a label 18px", () => {
        expect(ruleOf(style, ".chat-badge")).toMatch(/font-size:\s*var\(--chat-fs-badge\)/);
        expect(ruleOf(style, ".chat-badge.is-count")).toMatch(/height:\s*20px/);
        expect(ruleOf(style, ".chat-badge.is-label")).toMatch(/height:\s*18px/);
    });

    // A text badge, a count or a tag such as Leader, has the badge's square-ish corner from the
    // token sheet; the "+N" beside a pile of faces stays a circle, matching the faces it counts.
    test("every tone but the +N takes the badge corner; the +N stays round", () => {
        expect(ruleOf(style, ".chat-badge")).toMatch(/border-radius:\s*var\(--chat-r-badge\)/);
        expect(ruleOf(style, ".chat-badge.is-neutral")).toMatch(/border-radius:\s*var\(--chat-r-full\)/);
        for (const tone of [".chat-badge.is-brand", ".chat-badge.is-muted", ".chat-badge.is-tag", ".chat-badge.is-media", ".chat-badge.is-count", ".chat-badge.is-label"]) {
            expect(ruleOf(style, tone)).not.toMatch(/border-radius/);
        }
    });

    test("the church's tint for what is yours, one neutral for a +N, a quiet muted count, the photo's scrim", () => {
        expect(ruleOf(style, ".chat-badge.is-brand")).toMatch(/background:\s*var\(--chat-brand-tint\)/);
        expect(ruleOf(style, ".chat-badge.is-neutral")).toMatch(/background:\s*var\(--chat-line-strong\)/);
        expect(ruleOf(style, ".chat-badge.is-muted")).toMatch(/background:\s*var\(--chat-hover\)/);
        expect(ruleOf(style, ".chat-badge.is-media")).toMatch(/background:\s*var\(--chat-media-scrim\)/);
    });

    // The approved look draws a tag such as Leader on the pill fill, lighter than the "+N" disc's
    // line fill; one neutral for both made Leader the disc's darker grey.
    test("a tag such as Leader on the pill fill in the second text colour, apart from the +N disc's fill", () => {
        expect(ruleOf(style, ".chat-badge.is-tag")).toMatch(/background:\s*var\(--chat-pill-bg\)/);
        expect(ruleOf(style, ".chat-badge.is-tag")).toMatch(/color:\s*var\(--chat-text-2\)/);
        expect(ruleOf(style, ".chat-badge.is-neutral")).not.toMatch(/--chat-pill-bg/);
    });

    // The approved tag's words are semibold, a step under a count's bold digits, so a tag reads as
    // a quiet word beside a name; the "+N" keeps the count's bold, as the approved look draws it.
    test("a tag's words are semibold, under a count's bold", () => {
        expect(ruleOf(style, ".chat-badge")).toMatch(/font-weight:\s*700/);
        expect(ruleOf(style, ".chat-badge.is-tag")).toMatch(/font-weight:\s*600/);
        expect(ruleOf(style, ".chat-badge.is-neutral")).not.toMatch(/font-weight/);
    });

    test("holds no motion, priority, reach or capitals of its own", () => {
        expect(partRuleBreaks("badge.partial.obs")).toEqual([]);
    });
});
