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
// The first load's veil over a region. Until the region is revealed it lies over it, and the
// region under it is out of reach (the frame's own rule). From the moment the region is revealed
// the veil is leaving: it fades out over the content coming in, and takes no input while it does,
// so a first click or key on the revealed region reaches it rather than the veil.
//
// The component these tests are written to, components/pageVeil.partial.obs:
//
//   props: region: "sidebar" | "main", isRevealed: boolean, isMarksShown: boolean, kind: RoomLook
//   marks: page-veil, the veil, while it is drawn
import { readFileSync } from "fs";
import { resolve } from "path";
import { mount, VueWrapper } from "@vue/test-utils";
import { nextTick } from "vue";
import PageVeil from "../../../../../src/Communication/Chat/ChatShell/components/pageVeil.partial.obs";
import Skeleton from "../../../../../src/Communication/Chat/ChatShell/ui/skeleton.partial.obs";

let wrapper: VueWrapper | null = null;

afterEach(() => {
    wrapper?.unmount();
    wrapper = null;
});

/** The veil with Vue's own transition running, as it does on a page, so its way out is seen. */
function drawVeil(region: "sidebar" | "main"): VueWrapper {
    wrapper = mount(PageVeil, {
        props: { region, isRevealed: false, isMarksShown: true, kind: "channel" },
        attachTo: document.body,
        global: { stubs: { transition: false } }
    });
    return wrapper;
}

describe("the page veil", () => {
    test.each(["sidebar", "main"] as const)("over the %s, stands in until the region is revealed", region => {
        const w = drawVeil(region);

        const veil = w.find("[data-testid='page-veil']");
        expect(veil.exists()).toBe(true);
        expect(veil.attributes("inert")).toBeUndefined();
    });

    test.each(["sidebar", "main"] as const)("over the %s, takes no input from the moment the region is revealed, while it fades", async region => {
        const w = drawVeil(region);

        await w.setProps({ isRevealed: true });
        await nextTick();

        const leaving = document.querySelector("[data-testid='page-veil']");
        expect(leaving).not.toBeNull();
        expect(leaving?.hasAttribute("inert")).toBe(true);
    });

    test.each(["sidebar", "main"] as const)("over the %s, draws the status line it is given, hidden from screen readers, and changes its words in place", async region => {
        const w = drawVeil(region);
        expect(w.find("[data-testid='veil-status']").exists()).toBe(false);

        await w.setProps({ statusLine: "Connecting to chat\u2026" });
        const line = w.get("[data-testid='veil-status']");
        expect(line.attributes("aria-hidden")).toBe("true");
        expect(line.text()).toBe("Connecting to chat\u2026");
        expect(line.find(".chat-dot").exists()).toBe(true);

        await w.setProps({ statusLine: "Still connecting. Chat will open as soon as it can." });
        expect(w.get("[data-testid='veil-status']").text()).toBe("Still connecting. Chat will open as soon as it can.");
    });

    test("a channel's header drops its description mark while the status line takes the second line", async () => {
        const w = drawVeil("main");
        const headLines = (): number => w.get("[data-testid='veil-header']").findAll(".chat-skel-lines > .chat-skel-bone").length;
        expect(headLines()).toBe(2);

        await w.setProps({ statusLine: "Connecting to chat\u2026" });

        expect(headLines()).toBe(1);
        expect(w.find("[data-testid='veil-header'] .chat-skel-lines [data-testid='veil-status']").exists()).toBe(true);
    });

    // The marks are the shared still part, so the veil never draws a loading fill of its own: a
    // channel's head is a tile, a conversation's a face, each with its name.
    test("over the main column, draws its head and rows as skeletons, with no fill of its own", async () => {
        const w = drawVeil("main");
        const head = w.get("[data-testid='veil-header']").getComponent(Skeleton);

        expect(head.props("shape")).toBe("header");
        expect(head.props("isTile")).toBe(true);
        expect(w.find(".chat-skeleton").exists()).toBe(false);

        await w.setProps({ kind: "direct" });
        expect(w.get("[data-testid='veil-header']").getComponent(Skeleton).props("isTile")).toBe(false);
    });

    test("under reduced motion the status line's dot stands still, by the one rule every live dot shares", () => {
        const sheet = readFileSync(resolve(__dirname, "../../../../../../RockWeb/Styles/Blocks/Communication/Chat/chat.css"), "utf8").replace(/\/\*[\s\S]*?\*\//g, "");
        // each block runs to the brace at the line's start that closes it
        const reduced = Array.from(sheet.matchAll(/@media \(prefers-reduced-motion: reduce\)\s*\{([\s\S]*?)\r?\n\}/g)).map(m => m[1]);

        // the shared dot pulses through the loop stop, which reduced motion sets to none, so the pulse never runs
        const dot = readFileSync(resolve(__dirname, "../../../../../src/Communication/Chat/ChatShell/ui/dot.partial.obs"), "utf8");
        expect(dot).toMatch(/\n\.chat-dot\.is-pulse\s*\{[^}]*animation: var\(--chat-loop-stop, chat-pulse\) /);
        expect(reduced.some(block => /\.chat-shell\s*\{[^}]*--chat-loop-stop:\s*none;/.test(block))).toBe(true);
    });

    // The list arrives as headed sections, channels in 44px rows with tiles and direct messages in
    // 52px rows with faces, so the veil over it draws the same and nothing changes shape at reveal.
    test("over the sidebar, draws the list's headings with channel rows under Channels and conversation rows under Direct messages", () => {
        const veil = drawVeil("sidebar");
        const sections = veil.findAll("[data-testid='veil-section']");

        expect(sections.map(section => section.get("[data-testid='veil-heading']").text())).toEqual(["Channels", "Direct messages"]);
        const shapes = sections.map(section => [...new Set(section.findAllComponents(Skeleton).map(row => row.props("shape")))]);
        expect(shapes).toEqual([["channel"], ["person"]]);
    });

    test("the status line fades in on its way onto either veil, the column's header as the list's top", () => {
        const source = readFileSync(resolve(__dirname, "../../../../../src/Communication/Chat/ChatShell/components/pageVeil.partial.obs"), "utf8");
        const rule = source.slice(source.indexOf("<style")).match(/\n\.chat-veil-status \{([^}]*)\}/);

        expect(rule?.[1]).toMatch(/animation:\s*chat-reveal-in var\(--chat-t-base\)/);
    });

    // The marks are the first paint's own, so nothing fades them in: a fade would show an empty
    // column first, the flash the skeleton is there to prevent.
    test("draws the marks at once, with no fade in", () => {
        const source = readFileSync(resolve(__dirname, "../../../../../src/Communication/Chat/ChatShell/components/pageVeil.partial.obs"), "utf8");
        const styles = source.slice(source.indexOf("<style"));
        const rules = [...styles.matchAll(/\n([^{}\n]*\.chat-veil-(?:marks|header|feed)[^{}]*)\{([^}]*)\}/g)];

        expect(rules.length).toBeGreaterThan(0);
        expect(rules.filter(rule => /animation/.test(rule[2])).map(rule => rule[1].trim())).toEqual([]);
    });
});

