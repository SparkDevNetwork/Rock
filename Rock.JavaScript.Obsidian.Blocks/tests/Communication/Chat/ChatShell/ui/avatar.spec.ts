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
// The avatar and the pile, mounted. A person without a photo is drawn as initials in a hue of
// their own, and the hue comes from the name the chat shows, because the sidebar knows people only
// by name: keyed on anything else, one person would wear two colors on one screen. A pile is the
// same faces overlapped, each ringed so no initials are cut.
import { mount, VueWrapper } from "@vue/test-utils";
import Avatar from "../../../../../src/Communication/Chat/ChatShell/ui/avatar.partial.obs";

type Person = { name: string, photoUrl?: string | null };

function draw(people: Person[], size?: number): VueWrapper {
    return mount(Avatar, { props: size === undefined ? { people } : { people, size } });
}

function faces(wrapper: VueWrapper): HTMLElement[] {
    return wrapper.findAll("[data-testid='avatar']").map(face => face.element as HTMLElement);
}

/** The hue the avatar carries for the sheet's colour recipe to read. */
function hueOf(face: HTMLElement): number {
    return Number(face.style.getPropertyValue("--chat-av-hue").trim());
}

describe("an initials avatar", () => {
    test("draws two initials at the default size", () => {
        const [face] = faces(draw([{ name: "Anna Lee" }]));

        expect(face.textContent!.trim()).toBe("AL");
    });

    test("draws one initial for a one-word name", () => {
        const [face] = faces(draw([{ name: "Cher" }], 32));

        expect(face.textContent!.trim()).toBe("C");
    });

    // 16px is drawn only inside a mention chip or a reply line, where two letters would not read either.
    test.each([16, 18, 20, 22])("draws one initial at %ipx, where two would not fit", size => {
        const [face] = faces(draw([{ name: "Anna Lee" }], size));

        expect(face.textContent!.trim()).toBe("A");
    });

    test.each([28, 34, 36])("draws two initials at %ipx", size => {
        const [face] = faces(draw([{ name: "Anna Lee" }], size));

        expect(face.textContent!.trim()).toBe("AL");
    });

    test.each([20, 32, 34, 48])("is %ipx round with initials 0.37 of its size", size => {
        const [face] = faces(draw([{ name: "Anna Lee" }], size));

        expect(parseFloat(face.style.width)).toBe(size);
        expect(parseFloat(face.style.height)).toBe(size);
        expect(Math.abs(parseFloat(face.style.fontSize) - size * 0.37)).toBeLessThanOrEqual(0.01);
    });

    test("gives the same name the same hue on every draw", () => {
        const first = hueOf(faces(draw([{ name: "Nathan Hughes" }]))[0]);
        const again = hueOf(faces(draw([{ name: "Nathan Hughes" }], 20))[0]);

        expect(Number.isInteger(first)).toBe(true);
        expect(first).toBeGreaterThanOrEqual(0);
        expect(first).toBeLessThan(360);
        expect(again).toBe(first);
    });

    test("spreads different names around the color wheel", () => {
        // A room of a dozen people should read as a dozen people, not three colors.
        const names = [
            "Anna Lee", "Ben Lee", "Carla Diaz", "Dev Patel", "Erin Moss", "Femi Ade",
            "Grace Kim", "Hank Ortiz", "Ivy Chen", "Jon Park", "Kara Webb", "Liam Shaw"
        ];
        const hues = names.map(name => hueOf(faces(draw([{ name }]))[0]));

        expect(new Set(hues).size).toBeGreaterThanOrEqual(10);
        expect(Math.max(...hues) - Math.min(...hues)).toBeGreaterThan(180);
    });

    test("is never an empty disc, even for a person with no name to draw", () => {
        for (const name of ["", "   "]) {
            const [face] = faces(draw([{ name }]));

            const hasInitials = face.textContent!.trim() !== "";
            const hasIcon = face.querySelector("svg") !== null;
            expect(hasInitials || hasIcon).toBe(true);
        }
    });
});

describe("a photo avatar", () => {
    test("draws the photo at the same size in place of the initials", () => {
        const [face] = faces(draw([{ name: "Anna Lee", photoUrl: "https://church.example/GetImage.ashx?id=12" }], 34));

        const image = face.querySelector("img");
        expect(image).not.toBeNull();
        expect(image!.getAttribute("src")).toBe("https://church.example/GetImage.ashx?id=12");
        expect(face.textContent!.trim()).toBe("");
        expect(parseFloat(face.style.width)).toBe(34);
    });

    test("falls back to initials when the person has no photo", () => {
        const [face] = faces(draw([{ name: "Anna Lee", photoUrl: null }]));

        expect(face.querySelector("img")).toBeNull();
        expect(face.textContent!.trim()).toBe("AL");
    });
});

describe("a pile", () => {
    test("overlaps each face after the first by 0.18 of the size", () => {
        const pile = faces(draw([{ name: "Anna Lee" }, { name: "Ben Lee" }, { name: "Carla Diaz" }], 20));

        expect(pile).toHaveLength(3);
        expect(parseFloat(pile[0].style.marginLeft || "0")).toBe(0);
        for (const face of pile.slice(1)) {
            expect(Math.abs(parseFloat(face.style.marginLeft) + 20 * 0.18)).toBeLessThanOrEqual(0.01);
        }
    });

    test("keeps each person's own hue and initials", () => {
        const pile = faces(draw([{ name: "Anna Lee" }, { name: "Ben Lee" }], 32));
        const alone = faces(draw([{ name: "Ben Lee" }], 32))[0];

        expect(pile.map(face => face.textContent!.trim())).toEqual(["AL", "BL"]);
        expect(hueOf(pile[1])).toBe(hueOf(alone));
    });
});
