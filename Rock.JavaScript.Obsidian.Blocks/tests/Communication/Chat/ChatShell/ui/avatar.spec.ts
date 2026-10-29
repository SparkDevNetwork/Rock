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
import { readdirSync, readFileSync } from "fs";
import { join } from "path";
import Avatar from "../../../../../src/Communication/Chat/ChatShell/ui/avatar.partial.obs";
import { nextTick } from "vue";
import { createPageReveal, pageRevealKey } from "../../../../../src/Communication/Chat/ChatShell/pageLoad.partial";
import { imageHoldClass, imageInClass } from "../../../../../src/Communication/Chat/ChatShell/ui/imageReveal.partial";

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

    // A face in a group's mark is partly covered or crowded by the others, so it reads by one letter.
    test.each([20, 25, 39])("draws one initial 0.42 of its size at %ipx when asked for one letter", size => {
        const [face] = faces(mount(Avatar, { props: { people: [{ name: "Anna Lee", photoUrl: null }], size, isOneLetter: true } }));

        expect(face.textContent!.trim()).toBe("A");
        expect(Math.abs(parseFloat(face.style.fontSize) - size * 0.42)).toBeLessThanOrEqual(0.01);
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

    // A neutral disc stands in while the photo loads, so a hue never flashes before a face; the
    // photo stays hidden until its decode lands.
    test("holds a neutral disc and a hidden photo until the photo is decoded", async () => {
        const decodes: Array<() => void> = [];
        const original = Object.getOwnPropertyDescriptor(HTMLImageElement.prototype, "decode");
        HTMLImageElement.prototype.decode = () => new Promise<void>(resolve => decodes.push(resolve));

        try {
            const [face] = faces(mount(Avatar, { props: { people: [{ name: "Anna Lee", photoUrl: "https://church.example/a.jpg" }] }, attachTo: document.body }));
            const image = face.querySelector("img")!;

            expect(face.classList.contains("is-photo")).toBe(true);
            expect(image.classList.contains("chat-img-hold")).toBe(true);

            decodes.forEach(resolve => resolve());
            await Promise.resolve();
            await Promise.resolve();

            expect(image.classList.contains("chat-img-hold")).toBe(false);
        }
        finally {
            if (original) {
                Object.defineProperty(HTMLImageElement.prototype, "decode", original);
            }
            else {
                delete (HTMLImageElement.prototype as { decode?: unknown }).decode;
            }
        }
    });

    test("draws initials on the hue at once, with no neutral disc, for a person with no photo", () => {
        const [face] = faces(draw([{ name: "Anna Lee", photoUrl: null }]));

        expect(face.classList.contains("is-photo")).toBe(false);
        expect(face.textContent!.trim()).toBe("AL");
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

// Every face the chat builds carries the person's photo beside their name, null only for someone
// with none: a face built from a name alone draws initials even for a person with a photo, which
// is how the sidebar's direct messages, the header and the @ picker once drew everyone. This reads
// every chat source that draws a face and fails on a face object written without photoUrl.
describe("every face the chat builds", () => {
    const chatFolder = join(__dirname, "../../../../../src/Communication/Chat");

    /** The chat's components and scripts, the block and everything under its folder. */
    function sources(folder: string): string[] {
        return readdirSync(folder, { withFileTypes: true }).flatMap(entry => {
            const path = join(folder, entry.name);
            if (entry.isDirectory()) {
                return sources(path);
            }
            return /\.(obs|ts)$/.test(entry.name) ? [path] : [];
        });
    }

    /**
     * The lines of each object in a face drawing source that opens with a name, as a face is
     * written, and lacks photoUrl. A source draws faces when it uses the avatar component or its
     * face type; the component itself is where a face is read, not built.
     */
    function facesWithoutPhoto(text: string): number[] {
        if (!/avatar\.partial\.obs|AvatarPerson|\bFace\b/.test(text)) {
            return [];
        }

        const lines: number[] = [];
        // a template's {{ name }} is text, not an object
        const opening = /(?<!\{)\{(?!\{)\s*name\s*[:,}]/g;
        let match: RegExpExecArray | null;
        while ((match = opening.exec(text)) !== null) {
            let depth = 0;
            let end = match.index;
            for (; end < text.length; end++) {
                depth += text[end] === "{" ? 1 : text[end] === "}" ? -1 : 0;
                if (depth === 0) {
                    break;
                }
            }

            if (!/\bphotoUrl\b/.test(text.slice(match.index, end))) {
                lines.push(text.slice(0, match.index).split("\n").length);
            }
        }

        return lines;
    }

    test("carries photoUrl beside its name", () => {
        const bare = sources(chatFolder)
            .filter(path => !path.endsWith("avatar.partial.obs"))
            .flatMap(path => facesWithoutPhoto(readFileSync(path, "utf8")).map(line => `${path.slice(chatFolder.length + 1)}:${line}`));

        expect(bare).toEqual([]);
    });

    test("is caught when written by name alone, and passes with a photo or an explicit none", () => {
        const source = [
            "import Avatar from \"../ui/avatar.partial.obs\";",
            "const alone = people.map(name => ({ name }));",
            "const named = [{ name: person.name }];",
            "const photo = { name: person.name, photoUrl: person.avatar_url };",
            "const none = { name: person.name, photoUrl: null };"
        ].join("\n");

        expect(facesWithoutPhoto(source)).toEqual([2, 3]);
        expect(facesWithoutPhoto("const icon = { name: \"users\" };")).toEqual([]);
        expect(facesWithoutPhoto("<Avatar :people=\"faces\" /><h2>{{ name }}</h2>")).toEqual([]);
    });
});

// On the first load the page's reveal owns when a face appears: it waits for the faces still
// decoding, and what has decoded shows with it, under the page's own crossfade and with no fade of
// its own.
describe("a photo avatar on the first load", () => {
    const photo = "https://church.example/GetImage.ashx?id=12";

    function mountOnPage(reveal: ReturnType<typeof createPageReveal>): VueWrapper {
        return mount(Avatar, {
            props: { people: [{ name: "Anna Lee", photoUrl: photo }] },
            global: { provide: { [pageRevealKey as symbol]: { state: reveal.state, holdFace: reveal.holdFace } } }
        });
    }

    afterEach(() => {
        jest.useRealTimers();
    });

    test("a face decoded before the page releases shows with it and does not fade a second time", async () => {
        jest.useFakeTimers();
        const reveal = createPageReveal({ kind: "channel" });
        const wrapper = mountOnPage(reveal);
        const image = wrapper.get("img").element as HTMLImageElement;

        expect(image.classList.contains(imageHoldClass)).toBe(true);

        image.dispatchEvent(new Event("load"));
        reveal.sidebarAnswered();
        reveal.historyAnswered();
        reveal.factsAnswered();

        // The face decoded, so the page has no grace to wait out and releases on the next tick.
        expect(image.classList.contains(imageHoldClass)).toBe(true);
        await nextTick();

        expect(reveal.state.isReleased).toBe(true);
        expect(image.classList.contains(imageHoldClass)).toBe(false);
        expect(image.classList.contains(imageInClass)).toBe(false);

        reveal.stop();
        wrapper.unmount();
    });

    test("holds the page for a face still decoding, then shows it with the release", async () => {
        jest.useFakeTimers();
        const reveal = createPageReveal({ kind: "channel" });
        const wrapper = mountOnPage(reveal);
        const image = wrapper.get("img").element as HTMLImageElement;

        reveal.sidebarAnswered();
        reveal.historyAnswered();
        reveal.factsAnswered();
        await nextTick();
        expect(reveal.state.isReleased).toBe(false);

        image.dispatchEvent(new Event("load"));
        await Promise.resolve();
        await Promise.resolve();

        expect(reveal.state.isReleased).toBe(true);
        expect(image.classList.contains(imageHoldClass)).toBe(false);
        expect(image.classList.contains(imageInClass)).toBe(false);

        reveal.stop();
        wrapper.unmount();
    });

    test("a face drawn after the release fades in on its own", () => {
        const reveal = createPageReveal({ kind: "channel" });
        reveal.failed();
        const wrapper = mountOnPage(reveal);
        const image = wrapper.get("img").element as HTMLImageElement;

        image.dispatchEvent(new Event("load"));

        expect(image.classList.contains(imageHoldClass)).toBe(false);
        expect(image.classList.contains(imageInClass)).toBe(true);

        reveal.stop();
        wrapper.unmount();
    });
});
