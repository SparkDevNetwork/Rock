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
// The skeleton: one row of the shape the real row will have, drawn at once and still, so nothing
// jumps when the real row lands and nothing moves while it waits. A channel row is 44px with a
// tile, a conversation row 52px with a round face, a feed group a face with a name and lines (or
// a bubble in a two-person conversation), a panel row a face over three lines. It is decoration,
// hidden from a screen reader; what holds it says that it is loading.
import { mount } from "@vue/test-utils";
import Skeleton from "../../../../../src/Communication/Chat/ChatShell/ui/skeleton.partial.obs";
import { partRuleBreaks, ruleOf, styleOf } from "./partStyle";

function bones(shape: string, extra: Record<string, unknown> = {}): HTMLElement[] {
    return mount(Skeleton, { props: { shape, ...extra } }).findAll(".chat-skel-bone").map(bone => bone.element as HTMLElement);
}

describe("the skeleton", () => {
    test.each(["channel", "person", "group", "bubble", "panel"])("a %s row is hidden from a screen reader and carries its shape", shape => {
        const row = mount(Skeleton, { props: { shape } });

        expect(row.attributes("aria-hidden")).toBe("true");
        expect(row.classes()).toEqual(expect.arrayContaining(["chat-skel", `is-${shape}`]));
    });

    test("a channel row is a tile and one line", () => {
        const drawn = bones("channel");

        expect(drawn.map(bone => bone.classList.contains("is-tile"))).toEqual([true, false]);
    });

    test("a conversation row is a round face, a name and a line, and its time", () => {
        expect(bones("person").map(bone => bone.classList.contains("is-face"))).toEqual([true, false, false, false]);
    });

    test("a panel row is a round face over three lines", () => {
        expect(bones("panel").map(bone => bone.classList.contains("is-face"))).toEqual([true, false, false, false]);
    });

    test("a feed group is a face, a name and its time, and its lines", () => {
        const drawn = bones("group");

        expect(drawn[0].classList.contains("is-face")).toBe(true);
        expect(drawn.length).toBeGreaterThanOrEqual(4);
    });

    test("a bubble has a face beside it, and none when it is the reader's own", () => {
        expect(bones("bubble").some(bone => bone.classList.contains("is-face"))).toBe(true);
        expect(bones("bubble", { isOwn: true }).some(bone => bone.classList.contains("is-face"))).toBe(false);
        expect(mount(Skeleton, { props: { shape: "bubble", isOwn: true } }).classes()).toContain("is-own");
    });

    test("draws its lines at the widths it is given, so a column of rows reads as text rather than stripes", () => {
        const lines = bones("panel", { widths: [46, 92, 60] }).filter(bone => !bone.classList.contains("is-face"));

        expect(lines.map(line => line.style.width)).toEqual(["46%", "92%", "60%"]);
    });

    test("takes its test id and the element the list asks for from where it is drawn", () => {
        const row = mount(Skeleton, { props: { shape: "panel", as: "li" }, attrs: { "data-testid": "x-skeleton" } });

        expect(row.element.tagName).toBe("LI");
        expect(row.attributes("data-testid")).toBe("x-skeleton");
    });
});

describe("the skeleton's look", () => {
    const style = styleOf("skeleton.partial.obs");

    test("a channel row is 44px, a conversation row 52px", () => {
        expect(ruleOf(style, ".chat-skel.is-channel")).toMatch(/height:\s*44px/);
        expect(ruleOf(style, ".chat-skel.is-person")).toMatch(/height:\s*52px/);
    });

    test("is drawn still, with no wait and no fade in: no animation at all", () => {
        expect(style).not.toMatch(/animation/);
        expect(style).not.toMatch(/transition/);
    });

    test("its bones are one fill from the token sheet, faces round, tiles and lines in the sheet's corners", () => {
        expect(ruleOf(style, ".chat-skel-bone")).toMatch(/background:\s*var\(--chat-bubble\)/);
        expect(ruleOf(style, ".chat-skel-bone.is-face")).toMatch(/border-radius:\s*var\(--chat-r-full\)/);
        expect(ruleOf(style, ".chat-skel-bone.is-tile")).toMatch(/border-radius:\s*var\(--chat-r-control\)/);
    });

    test("holds no motion, priority, reach or capitals of its own", () => {
        expect(partRuleBreaks("skeleton.partial.obs")).toEqual([]);
    });
});
