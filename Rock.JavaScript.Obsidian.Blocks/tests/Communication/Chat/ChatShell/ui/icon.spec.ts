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
import { mount } from "@vue/test-utils";
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
