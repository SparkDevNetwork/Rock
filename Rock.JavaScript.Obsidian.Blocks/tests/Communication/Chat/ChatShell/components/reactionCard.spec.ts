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
// A chip's card of who reacted, drawn from the chip alone: the emoji, its label and how many
// people, then up to three faces and names, "You" first on the reader's own chip with their own
// photo, then a fourth row on the same grid for the rest, which is See all.
//
// The component these tests are written to, components/reactionCard.partial.obs:
//
//   props: {
//       reaction: Reaction,                     { code, count, mine, emoji, image_url, label, reactors }
//       reader: { personAliasGuid, name, photoUrl }
//   }
//   emits: seeAll()
//   marks: data-testid="reaction-card", its count "reaction-card-count", each person
//          "reaction-card-person", the rest's row "reaction-card-more", "See all N reactions".
import { mount, VueWrapper } from "@vue/test-utils";
import ReactionCard from "../../../../../src/Communication/Chat/ChatShell/components/reactionCard.partial.obs";

const me = "a0000001-0000-4000-8000-000000000000";
const anna = "a0000002-0000-4000-8000-000000000000";
const david = "a0000003-0000-4000-8000-000000000000";

const reader = { personAliasGuid: me, name: "Jordan Ellis", photoUrl: "https://rock.example.org/jordan.jpg" };

const person = (alias: string, nick: string, last: string, avatar: string | null = null): Record<string, unknown> => ({
    person_alias_guid: alias, nick_name: nick, last_name: last, avatar_url: avatar, reacted_at: "2026-10-16T09:01:00Z", reactor_person_alias_guid: alias
});

function pray(count: number, mine: boolean, reactors: unknown[]): Record<string, unknown> {
    return { code: "pray", count, mine, emoji: "🙏", image_url: null, label: "folded hands", reactors };
}

function draw(reaction: Record<string, unknown>): VueWrapper {
    return mount(ReactionCard, { props: { reaction, reader } as never });
}

function names(w: VueWrapper): string[] {
    return w.findAll("[data-testid='reaction-card-person'] .chat-rx-card-name").map(name => name.text());
}

describe("the card", () => {
    test("heads with the emoji, its label and how many people", () => {
        const w = draw(pray(12, false, []));

        expect(w.text()).toContain("🙏");
        expect(w.text()).toContain("folded hands");
        expect(w.get("[data-testid='reaction-card-count']").text()).toBe("12 people");
    });

    test("leads with You and the reader's photo on the reader's chip, then two others", () => {
        const w = draw(pray(12, true, [person(anna, "Anna", "Reyes"), person(david, "David", "Okafor"), person("a0000004-0000-4000-8000-000000000000", "Cara", "Lin")]));

        expect(names(w)).toEqual(["You", "Anna Reyes", "David Okafor"]);
        expect(w.findAll("[data-testid='reaction-card-person']")[0].get("img").attributes("src")).toBe("https://rock.example.org/jordan.jpg");
    });

    test("draws the rest as a fourth row: a disc with the count, how many others, and See all", async () => {
        const w = draw(pray(12, true, [person(anna, "Anna", "Reyes"), person(david, "David", "Okafor")]));

        const more = w.get("[data-testid='reaction-card-more']");
        expect(more.element.tagName).toBe("BUTTON");
        expect(more.attributes("aria-label")).toBe("See all 12 reactions");
        expect(more.text()).toContain("+9");
        expect(more.text()).toContain("9 others");
        expect(more.text()).toContain("See all");

        await more.trigger("click");
        expect(w.emitted("seeAll")).toHaveLength(1);
    });

    test("draws no fourth row when everyone is named", () => {
        const w = draw(pray(2, false, [person(anna, "Anna", "Reyes"), person(david, "David", "Okafor")]));

        expect(names(w)).toEqual(["Anna Reyes", "David Okafor"]);
        expect(w.find("[data-testid='reaction-card-more']").exists()).toBe(false);
    });

    test("with no one named, the header's count says how many and See all stands alone, never a second count", async () => {
        const nameless = (alias: string): Record<string, unknown> => ({ ...person(alias, "", ""), nick_name: null, last_name: null });
        const w = draw(pray(48, false, [nameless(anna), nameless(david), nameless("a0000004-0000-4000-8000-000000000000")]));

        expect(w.get("[data-testid='reaction-card-count']").text()).toBe("48 people");
        expect(w.findAll("[data-testid='reaction-card-person']")).toHaveLength(0);
        expect(w.text().match(/48 people/g)).toHaveLength(1);
        expect(w.find(".chat-rx-card-disc").exists()).toBe(false);

        const more = w.get("[data-testid='reaction-card-more']");
        expect(more.text()).toBe("See all");
        expect(more.attributes("aria-label")).toBe("See all 48 reactions");
        await more.trigger("click");
        expect(w.emitted("seeAll")).toHaveLength(1);
    });

    test("one nameless person is the count alone", () => {
        const w = draw(pray(1, false, [{ ...person(david, "", ""), nick_name: null, last_name: null }]));

        expect(w.get("[data-testid='reaction-card-count']").text()).toBe("1 person");
        expect(w.findAll("[data-testid='reaction-card-person']")).toHaveLength(0);
        expect(w.find("[data-testid='reaction-card-more']").exists()).toBe(false);
    });
});
