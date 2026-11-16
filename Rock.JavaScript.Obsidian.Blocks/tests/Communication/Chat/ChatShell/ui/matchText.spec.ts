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
// The one match mark: a name drawn with the letters a search matched underlined in the church's
// colour. It was a wash in the row's own tint, invisible on the row about to be picked, in one
// place, and a padded box that split the word in another; now every search marks a match alike.
import { mount, VueWrapper } from "@vue/test-utils";
import { h } from "vue";
import MatchText from "../../../../../src/Communication/Chat/ChatShell/ui/matchText.partial.obs";
import { partRuleBreaks, ruleOf, styleOf } from "./partStyle";

/** The mark inside a name's own element, as a row draws it. */
function draw(text: string, query: string): VueWrapper {
    return mount({ render: () => h("b", [h(MatchText, { text, query })]) });
}

describe("the match mark", () => {
    test("marks the letters matched at the name's start, their case kept, as one span", () => {
        const w = draw("Prayer Requests", "pray");
        const marks = w.findAll("mark");

        expect(w.text()).toBe("Prayer Requests");
        expect(marks).toHaveLength(1);
        expect(marks[0].text()).toBe("Pray");
        expect(marks[0].classes()).toContain("chat-match");
    });

    test("marks a word's start before the same letters inside a word", () => {
        const w = draw("Tea Team", "tea");

        expect(w.get("mark").text()).toBe("Tea");
        expect(draw("Bread Team", "team").get("mark").text()).toBe("Team");
        expect(draw("Instead", "tea").get("mark").text()).toBe("tea");
    });

    test("draws the text alone when nothing is typed or nothing matches", () => {
        expect(draw("Prayer Requests", "").find("mark").exists()).toBe(false);
        expect(draw("Prayer Requests", "zz").find("mark").exists()).toBe(false);
        expect(draw("Prayer Requests", "zz").text()).toBe("Prayer Requests");
    });

    test("reads the typed text trimmed and in any case", () => {
        expect(draw("Prayer Requests", " REQ ").get("mark").text()).toBe("Req");
    });
});

describe("the match mark's look", () => {
    const style = styleOf("matchText.partial.obs");

    test("is the church's colour underlined under the letters, with no fill and no padding, so it reads on any row", () => {
        const mark = ruleOf(style, ".chat-match");

        expect(mark).toMatch(/text-decoration:\s*underline[^;]*var\(--chat-brand-fill\)/);
        expect(mark).toMatch(/color:\s*var\(--chat-brand-ink\)/);
        expect(mark).toMatch(/background:\s*none/);
        expect(mark).toMatch(/padding:\s*0/);
        expect(mark).not.toMatch(/border-radius|box-shadow/);
    });

    test("holds no motion, priority, reach or capitals of its own", () => {
        expect(partRuleBreaks("matchText.partial.obs")).toEqual([]);
    });
});
