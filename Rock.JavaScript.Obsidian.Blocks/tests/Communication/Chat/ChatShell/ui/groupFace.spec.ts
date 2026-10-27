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
// A group conversation's face, mounted. Two other people are a diagonal pair of offset faces, the
// front one ringed; three or more are a triangle of bubbles, the third the third person when there
// are exactly three and otherwise a count of everyone past the first two. Each face is a photo, or
// one initial in the person's hue, and the whole mark keeps the slot a one-person face would fill.
import { mount, VueWrapper } from "@vue/test-utils";
import { readFileSync } from "fs";
import { join } from "path";
import GroupFace from "../../../../../src/Communication/Chat/ChatShell/ui/groupFace.partial.obs";

type Person = { name: string, photoUrl: string | null };

const anna: Person = { name: "Anna Reyes", photoUrl: "https://photos.example/anna.jpg" };
const marcus: Person = { name: "Marcus Bell", photoUrl: null };
const priya: Person = { name: "Priya Shah", photoUrl: "https://photos.example/priya.jpg" };
const dee: Person = { name: "Dee Moss", photoUrl: null };

function draw(people: Person[], count?: number | null, size?: number): VueWrapper {
    const props: Record<string, unknown> = { people };
    if (count !== undefined) {
        props.count = count;
    }
    if (size !== undefined) {
        props.size = size;
    }

    return mount(GroupFace, { props });
}

function root(wrapper: VueWrapper): HTMLElement {
    return wrapper.get("[data-testid='group-face']").element as HTMLElement;
}

function bubbles(wrapper: VueWrapper): HTMLElement[] {
    return wrapper.findAll(".chat-gf-bubble").map(bubble => bubble.element as HTMLElement);
}

function faceIn(bubble: HTMLElement): HTMLElement {
    return bubble.querySelector(".chat-av") as HTMLElement;
}

describe("a group's face with two other people", () => {
    test("is a diagonal pair of the two, back then front", () => {
        const w = draw([anna, marcus]);

        expect(root(w).dataset.shape).toBe("pair");
        const [back, front] = bubbles(w);
        expect(back.classList.contains("is-back")).toBe(true);
        expect(front.classList.contains("is-front")).toBe(true);
        expect(faceIn(back).querySelector("img")!.getAttribute("src")).toBe(anna.photoUrl);
        expect(faceIn(front).querySelector("img")).toBeNull();
    });

    test("draws each face 25 of the 36px slot, one initial in the face's hue where there is no photo", () => {
        const w = draw([marcus, dee]);

        for (const bubble of bubbles(w)) {
            expect(parseFloat(faceIn(bubble).style.width)).toBe(25);
        }

        expect(bubbles(w).map(bubble => faceIn(bubble).textContent!.trim())).toEqual(["M", "D"]);
        expect(faceIn(bubbles(w)[0]).style.getPropertyValue("--chat-av-hue")).not.toBe("");
    });

    test("marks a face that has a photo, so only an initial is nudged clear of the front face", () => {
        const [back, front] = bubbles(draw([anna, marcus]));

        expect(back.classList.contains("has-photo")).toBe(true);
        expect(front.classList.contains("has-photo")).toBe(false);
    });

    test("is never a count", () => {
        expect(draw([anna, marcus], 2).find("[data-testid='group-face-more']").exists()).toBe(false);
    });
});

describe("a group's face with three or more other people", () => {
    test("is a triangle of the three when there are exactly three", () => {
        const w = draw([anna, marcus, priya], 3);

        expect(root(w).dataset.shape).toBe("trio");
        expect(bubbles(w)).toHaveLength(3);
        expect(faceIn(bubbles(w)[2]).querySelector("img")!.getAttribute("src")).toBe(priya.photoUrl);
        expect(w.find("[data-testid='group-face-more']").exists()).toBe(false);
    });

    test("is the first two and a count of everyone else past them", () => {
        // Eight others, of whom the platform sends the first four.
        const w = draw([anna, marcus, priya, dee], 8);

        expect(root(w).dataset.shape).toBe("trio");
        expect(bubbles(w)).toHaveLength(3);
        expect(w.get("[data-testid='group-face-more']").text()).toBe("+6");
        expect(faceIn(bubbles(w)[0]).querySelector("img")!.getAttribute("src")).toBe(anna.photoUrl);
        expect(faceIn(bubbles(w)[1]).textContent!.trim()).toBe("M");
    });

    test("counts the people it was told of when it was given no count", () => {
        const w = draw([anna, marcus, priya, dee]);

        expect(w.get("[data-testid='group-face-more']").text()).toBe("+2");
    });

    test("draws each bubble 20 of the 36px slot", () => {
        const w = draw([anna, marcus, priya], 3);

        for (const bubble of bubbles(w)) {
            expect(parseFloat(faceIn(bubble).style.width)).toBe(20);
        }

        const more = draw([anna, marcus, priya, dee], 8).get("[data-testid='group-face-more']").element as HTMLElement;
        expect(parseFloat(more.style.width)).toBe(20);
    });
});

describe("a group's face with fewer people to show", () => {
    test("is the people icon in the whole slot when it knows none of them, as for a named group", () => {
        const w = draw([], null);

        expect(root(w).dataset.shape).toBe("none");
        const face = w.get(".chat-av").element as HTMLElement;
        expect(parseFloat(face.style.width)).toBe(36);
        expect(w.find(".chat-av svg").exists()).toBe(true);
    });

    test("still reads as a group when only one other person can be shown, the people icon behind them", () => {
        // A group of three where one person is on the Chat Ban List, so only one other is listed.
        const w = draw([anna], 1);

        expect(root(w).dataset.shape).toBe("pair");
        const [back, front] = bubbles(w);
        expect(back.querySelector("svg")).not.toBeNull();
        expect(faceIn(front).querySelector("img")!.getAttribute("src")).toBe(anna.photoUrl);
    });
});

describe("a group's face at another size", () => {
    test("fills the slot it is given, every part in proportion", () => {
        const pair = draw([anna, marcus], 2, 56);
        expect(root(pair).style.getPropertyValue("--chat-gf-s")).toBe("56px");
        expect(parseFloat(faceIn(bubbles(pair)[0]).style.width)).toBe(39);

        const trio = draw([anna, marcus, priya], 3, 56);
        expect(parseFloat(faceIn(bubbles(trio)[0]).style.width)).toBe(31);
    });

    test("is hidden from a screen reader, since the conversation's name says who is in it", () => {
        expect(root(draw([anna, marcus])).getAttribute("aria-hidden")).toBe("true");
    });
});

// The ring around a face must be the color behind the mark, or it shows as a band of another
// color. What a hovered or current row draws is its own fill over the sidebar, so the ring is two
// layers in the same order: the owner's fill over the owner's base. jsdom applies no stylesheet
// and has no hover, so these read the rules: every ring draws both layers, and every state of an
// owner that changes its background changes it only through the fill the ring reads.
describe("a group's face ring", () => {
    const shell = join(__dirname, "../../../../../src/Communication/Chat/ChatShell");

    /** Each rule of a component's style block whose selector matches, as its declarations. */
    function rules(file: string, selector: RegExp): string[] {
        const text = readFileSync(join(shell, file), "utf8");
        const style = text.slice(text.indexOf("<style"), text.indexOf("</style>"));
        return Array.from(style.matchAll(/([^{}]+)\{([^{}]*)\}/g))
            .filter(match => match[1].replace(/\/\*[\s\S]*?\*\//g, "").split(",").some(part => selector.test(part.trim())))
            .map(match => match[2]);
    }

    test("lays the owner's fill over its base in every ring", () => {
        const rings = rules("ui/groupFace.partial.obs", /./).filter(body => /box-shadow/.test(body));

        expect(rings.length).toBeGreaterThan(0);
        for (const body of rings) {
            expect(body).toMatch(/var\(--chat-av-ring-over, transparent\)[^;]*var\(--chat-av-ring, var\(--chat-surface\)\)/);
        }
    });

    test.each([
        ["a sidebar row", "components/channelSidebar.partial.obs", /^\.chat-row(\[|:|$)/, "--chat-row-fill"],
        ["the header's title button", "components/chatHeader.partial.obs", /^(button)?\.chat-header-id(\[|:|$)/, "--chat-header-id-fill"]
    ])("follows %s's hover and current state through its fill", (_name, file, selector, fill) => {
        const states = rules(file, selector);
        const backgrounds = states.flatMap(body => Array.from(body.matchAll(/(?:^|[;\s])background(?:-color)?:\s*([^;]+);/g)).map(match => match[1].trim()));

        expect(states.some(body => body.includes(`--chat-av-ring-over: var(${fill})`))).toBe(true);
        expect(backgrounds.length).toBeGreaterThan(0);
        expect(backgrounds.every(value => value === `var(${fill})`)).toBe(true);
    });
});
