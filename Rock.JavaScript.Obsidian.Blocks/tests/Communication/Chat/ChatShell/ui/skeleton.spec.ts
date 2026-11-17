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
import { providedSession } from "../sessionShapes";

function bones(shape: string, extra: Record<string, unknown> = {}): HTMLElement[] {
    return mount(Skeleton, { props: { shape, ...extra } }).findAll(".chat-skel-bone").map(bone => bone.element as HTMLElement);
}

describe("the skeleton", () => {
    test.each(["channel", "person", "group", "bubble", "panel", "reactor", "media", "contact", "text", "header"])("a %s row is hidden from a screen reader and carries its shape", shape => {
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

    // A two-person conversation draws no faces beside its bubbles, so neither does its skeleton.
    test("a bubble has no face, the reader's own or the other person's", () => {
        expect(bones("bubble").some(bone => bone.classList.contains("is-face"))).toBe(false);
        expect(bones("bubble", { isOwn: true }).some(bone => bone.classList.contains("is-face"))).toBe(false);
        expect(mount(Skeleton, { props: { shape: "bubble", isOwn: true } }).classes()).toContain("is-own");
    });

    // The feed's rows narrow their face on a phone, so the skeleton reads the session as the
    // list row does and narrows with them; nothing jumps when the page lands.
    test("a feed group's face is the row's own, 36px on a desktop and 32px on a phone, and the row says which", () => {
        const desktop = mount(Skeleton, { props: { shape: "group" }, global: { provide: providedSession({ isPhone: false }) } });
        const phone = mount(Skeleton, { props: { shape: "group" }, global: { provide: providedSession({ isPhone: true }) } });

        expect((desktop.find(".chat-skel-bone.is-face").element as HTMLElement).style.width).toBe("36px");
        expect(desktop.classes()).not.toContain("is-phone");
        expect((phone.find(".chat-skel-bone.is-face").element as HTMLElement).style.width).toBe("32px");
        expect((phone.find(".chat-skel-bone.is-face").element as HTMLElement).style.height).toBe("32px");
        expect(phone.classes()).toContain("is-phone");
    });

    test("a reactor row is a 28px face and a name, as the reactions list draws a person", () => {
        const drawn = bones("reactor");

        expect(drawn.map(bone => bone.classList.contains("is-face"))).toEqual([true, false]);
        expect(drawn[0].style.width).toBe("28px");
        expect(drawn[0].style.height).toBe("28px");
    });

    // A picture still to come, a GIF in the picker's grid, holds its place at the picture's own shape.
    test("a media row is the place of a picture at the shape it is given, with no face or lines", () => {
        const row = mount(Skeleton, { props: { shape: "media", ratio: "16 / 9" } });

        expect((row.element as HTMLElement).style.aspectRatio).toBe("16 / 9");
        expect(row.findAll(".chat-skel-bone")).toHaveLength(0);
        expect(row.findAll(".chat-skel-lines")).toHaveLength(0);
    });

    test("draws its lines at the widths it is given, so a column of rows reads as text rather than stripes", () => {
        const lines = bones("panel", { widths: [46, 92, 60] }).filter(bone => !bone.classList.contains("is-face"));

        expect(lines.map(line => line.style.width)).toEqual(["46%", "92%", "60%"]);
    });

    // A profile's contact row: its icon and one line, 32px tall, as the panel draws a phone or an email.
    test("a contact row is a small icon and one line", () => {
        const drawn = bones("contact", { widths: [64] });

        expect(drawn.map(bone => bone.classList.contains("is-icon"))).toEqual([true, false]);
        expect(drawn[0].style.width).toBe("15px");
        expect(drawn[1].style.width).toBe("64%");
    });

    // Words a church or a person wrote, still to come: lines alone, one for each width given.
    test("a text block is lines alone, one for each width it is given", () => {
        const drawn = bones("text", { widths: [30, 88, 72] });

        expect(drawn.some(bone => bone.classList.contains("is-face") || bone.classList.contains("is-tile"))).toBe(false);
        expect(drawn.map(bone => bone.style.width)).toEqual(["30%", "88%", "72%"]);
    });

    // A room's head: its mark, then its name and, for a channel, its description.
    test("a room's head is a tile for a channel or a face for a person, its name, then its second line", () => {
        const channel = bones("header", { isTile: true, widths: [22, 40] });
        const person = bones("header", { widths: [22] });

        expect(channel.map(bone => bone.classList.contains("is-tile"))).toEqual([true, false, false]);
        expect(channel[0].style.width).toBe("36px");
        expect(channel.slice(1).map(bone => [bone.style.width, bone.style.height])).toEqual([["22%", "12px"], ["40%", "9px"]]);
        expect(person.map(bone => bone.classList.contains("is-face"))).toEqual([true, false]);
    });

    // What waits in a row's last line, such as the start's status, stands in the row's own lines.
    test("draws what it holds after its lines, in the lines' column", () => {
        const row = mount(Skeleton, { props: { shape: "header", widths: [22] }, slots: { default: "<span data-testid='held'>Connecting</span>" } });

        expect(row.find(".chat-skel-lines [data-testid='held']").exists()).toBe(true);
    });

    test("takes its test id and the element the list asks for from where it is drawn", () => {
        const row = mount(Skeleton, { props: { shape: "panel", as: "li" }, attrs: { "data-testid": "x-skeleton" } });

        expect(row.element.tagName).toBe("LI");
        expect(row.attributes("data-testid")).toBe("x-skeleton");
    });
});

describe("the skeleton's look", () => {
    const style = styleOf("skeleton.partial.obs");

    test("a channel row is 44px, a conversation row 52px, a reactor row 40px", () => {
        expect(ruleOf(style, ".chat-skel.is-channel")).toMatch(/height:\s*44px/);
        expect(ruleOf(style, ".chat-skel.is-person")).toMatch(/height:\s*52px/);
        expect(ruleOf(style, ".chat-skel.is-reactor")).toMatch(/height:\s*40px/);
        expect(ruleOf(style, ".chat-skel.is-contact")).toMatch(/height:\s*32px/);
    });

    test("a contact row's icon takes the sheet's inline corners", () => {
        expect(ruleOf(style, ".chat-skel-bone.is-icon")).toMatch(/border-radius:\s*var\(--chat-r-inline\)/);
    });

    // The message row's gutter, gap and sides: 36, 12 and 20 on a desktop, 32, 10 and 12 on a
    // phone; a two-person conversation's bubbles stand 20 in, 12 on a phone.
    test("a feed group keeps the message row's gap and sides on a desktop and on a phone, and a bubble its sides", () => {
        expect(ruleOf(style, ".chat-skel.is-group")).toMatch(/gap:\s*12px/);
        expect(ruleOf(style, ".chat-skel.is-group")).toMatch(/padding:\s*6px 20px/);
        expect(ruleOf(style, ".chat-skel.is-group.is-phone")).toMatch(/gap:\s*10px/);
        expect(ruleOf(style, ".chat-skel.is-group.is-phone")).toMatch(/padding:\s*6px 12px/);
        expect(ruleOf(style, ".chat-skel.is-bubble")).toMatch(/padding:\s*2px 20px/);
        expect(ruleOf(style, ".chat-skel.is-bubble.is-phone")).toMatch(/padding:\s*2px 12px/);
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

    test("a media row is one fill from the token sheet in the sheet's control corners", () => {
        expect(ruleOf(style, ".chat-skel.is-media")).toMatch(/background:\s*var\(--chat-bubble\)/);
        expect(ruleOf(style, ".chat-skel.is-media")).toMatch(/border-radius:\s*var\(--chat-r-control\)/);
    });

    test("holds no motion, priority, reach or capitals of its own", () => {
        expect(partRuleBreaks("skeleton.partial.obs")).toEqual([]);
    });
});
