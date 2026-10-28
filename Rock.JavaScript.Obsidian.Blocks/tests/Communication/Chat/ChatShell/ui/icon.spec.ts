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
// The chat's icon set, mounted, for the notify modes: every message is the bell, mentions only the
// bell with an @ in its lower corner, and off the slashed bell. The @ notches the bell's outline the
// way the bell's other badged forms do, so the outline is cut short rather than drawn whole under
// the badge, and each mode is drawn from the one place that names its glyph.
//
// An icon is drawn at the size it is given unless what holds it sets --chat-ic-size or
// --chat-ic-stroke, which the icon's rules read before their own. A holder's rule on the icon
// itself would weigh the same as the icon's and win or lose by the order the built styles load
// in: the @all mark's glyph once lost that way and drew at the icon's 13px, not its share of the disc.
import { mount } from "@vue/test-utils";
import { readFileSync } from "fs";
import { resolve } from "path";
import Icon from "../../../../../src/Communication/Chat/ChatShell/ui/icon.partial.obs";
import { notifyIconFor } from "../../../../../src/Communication/Chat/ChatShell/composables/usePrefs.partial";

/** Each shape the icon draws: a path by its data, a circle by its centre and radius. */
function drawn(name: string): string[] {
    const w = mount(Icon, { props: { name } });
    const shapes = Array.from(w.element.querySelectorAll("path, circle"))
        .map(shape => shape.getAttribute("d") ?? `circle ${shape.getAttribute("cx")} ${shape.getAttribute("cy")} ${shape.getAttribute("r")}`);
    w.unmount();
    return shapes;
}

describe("the notify mode icons", () => {
    test("name one glyph per mode", () => {
        expect(notifyIconFor("all")).toBe("bell");
        expect(notifyIconFor("mentions")).toBe("bell-at");
        expect(notifyIconFor("silent")).toBe("bell-off");
    });

    test("draw mentions only as a bell cut short where its @ sits, not the whole bell under a mark", () => {
        const bell = drawn("bell");
        const mentions = drawn("bell-at");

        // The bell's outline and clapper, then the @'s ring and tail.
        expect(mentions).toHaveLength(4);
        expect(mentions.some(shape => bell.includes(shape))).toBe(false);
        expect(mentions[2]).toMatch(/^circle /);
    });

    test("no longer draw the bell with a dot", () => {
        expect(drawn("at-bell")).toEqual([]);
    });
});

describe("the icon's size", () => {
    const source = readFileSync(resolve(__dirname, "../../../../../src/Communication/Chat/ChatShell/ui/icon.partial.obs"), "utf8");
    const style = source.slice(source.indexOf("<style"), source.indexOf("</style>")).replace(/\/\*[\s\S]*?\*\//g, "");
    const sheet = readFileSync(resolve(__dirname, "../../../../../../RockWeb/Styles/Blocks/Communication/Chat/chat.css"), "utf8").replace(/\/\*[\s\S]*?\*\//g, "");

    /** A rule's declarations, by property, for the selector given. */
    function rule(css: string, selector: string): Record<string, string> {
        const lines = css.replace(/\r\n/g, "\n");
        const start = lines.indexOf(`\n${selector} {`);
        const body = start < 0 ? "" : lines.slice(lines.indexOf("{", start) + 1, lines.indexOf("}", start));
        return Object.fromEntries(body.split(";").map(d => d.trim()).filter(d => d).map(d => [d.slice(0, d.indexOf(":")).trim(), d.slice(d.indexOf(":") + 1).trim()]));
    }

    test("is its holder's --chat-ic-size and --chat-ic-stroke where set, else its own at each size", () => {
        expect(rule(style, ".chat-ic")).toMatchObject({ "width": "var(--chat-ic-size, 18px)", "height": "var(--chat-ic-size, 18px)", "stroke-width": "var(--chat-ic-stroke, 1.75)" });
        expect(rule(style, ".chat-ic.is-sm")).toMatchObject({ width: "var(--chat-ic-size, 15px)", height: "var(--chat-ic-size, 15px)" });
        expect(rule(style, ".chat-ic.is-xs")).toMatchObject({ "width": "var(--chat-ic-size, 13px)", "height": "var(--chat-ic-size, 13px)", "stroke-width": "var(--chat-ic-stroke, 2)" });
    });

    test("in the @all mark is a share of the disc, set on the mark for the glyph it holds", () => {
        expect(rule(sheet, ".chat-all-mark")).toMatchObject({ "--chat-ic-size": "calc(var(--chat-all-mark-s, 18px) * 11 / 18)", "--chat-ic-stroke": "2.3" });
    });
});
