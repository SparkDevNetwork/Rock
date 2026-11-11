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

        // The first face is covered by the second, so it draws one initial; the last has nothing over it.
        expect(pile.map(face => face.textContent!.trim())).toEqual(["A", "BL"]);
        expect(hueOf(pile[1])).toBe(hueOf(alone));
    });

    // A face the next one overlaps shows 0.82 of its width less the ring, which two initials
    // centred in the whole face reach past: the header's 28px pile cut the R of "MR".
    test.each([26, 28, 36])("draws one initial on every covered face at %ipx and two only on the last", size => {
        const pile = faces(draw([{ name: "Marcus Reed" }, { name: "Anna Lee" }, { name: "Ben Cole" }], size));

        expect(pile.map(face => face.textContent!.trim())).toEqual(["M", "A", "BC"]);
    });

    test("a single face still draws two initials at any size over 22px", () => {
        expect(faces(draw([{ name: "Marcus Reed" }], 28))[0].textContent!.trim()).toBe("MR");
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

// A face's online status: a small disc at its lower right on the rim, a glyph per state so the
// shape says it as well as the colour, drawn beside the face inside the avatar's one root so no
// face without one changes. Only a single face of 32px or more carries it; the person's own face
// alone draws the hollow ring of Appear offline. It draws as state when it mounts and fades only
// for a change the store made after that.
describe("a face's status badge", () => {
    type Status = { state: "online" | "away" | "busy" | "hidden", label: string, tip: string, busyUntil: number | null, revision: number };

    const statuses: Record<"online" | "away" | "busy", Status> = {
        online: { state: "online", label: ", online", tip: "Online", busyUntil: null, revision: 4 },
        away: { state: "away", label: ", away", tip: "Away", busyUntil: null, revision: 4 },
        busy: { state: "busy", label: ", busy until 3:00 PM", tip: "Busy until 3:00 PM", busyUntil: Date.parse("2026-11-10T21:00:00Z"), revision: 4 }
    };
    const hidden: Status = { state: "hidden", label: "", tip: "Appear offline", busyUntil: null, revision: 4 };
    const anna = [{ name: "Anna Lee", photoUrl: null }];

    // Drawn with Vue's own transition, so the badge sits in the root as the page has it.
    const global = { stubs: { transition: false } };

    function withStatus(status: Status | null, extra: Record<string, unknown> = {}): VueWrapper {
        return mount(Avatar, { props: { people: anna, size: 36, status, ...extra }, attachTo: document.body, global });
    }

    function badgeOf(wrapper: VueWrapper): HTMLElement | null {
        return wrapper.element.querySelector<HTMLElement>("[data-testid='status-badge']");
    }

    afterEach(() => {
        document.body.replaceChildren();
        jest.useRealTimers();
    });

    test.each([
        ["online", "polyline"],
        ["away", "circle"],
        ["busy", "rect"]
    ] as const)("%s draws its disc and its own glyph beside the face, in the avatar's one root", (state, shape) => {
        const wrapper = withStatus(statuses[state]);
        const badge = badgeOf(wrapper)!;

        expect(badge.dataset.state).toBe(state);
        expect(badge.parentElement).toBe(wrapper.element);
        expect(wrapper.element.classList.contains("chat-pile")).toBe(true);
        expect(badge.previousElementSibling?.classList.contains("chat-av")).toBe(true);
        const shown = badge.querySelectorAll(".is-shown");
        expect(shown).toHaveLength(1);
        expect(shown[0].querySelector(shape)).not.toBeNull();
    });

    test("is inside the avatar's hidden root, so a screen reader hears the words its host puts in a label instead", () => {
        const wrapper = withStatus(statuses.online);

        expect(badgeOf(wrapper)!.closest("[aria-hidden='true']")).toBe(wrapper.element);
    });

    test("shows its words on a resting pointer", async () => {
        jest.useFakeTimers();
        const wrapper = withStatus(statuses.busy);

        badgeOf(wrapper)!.dispatchEvent(new MouseEvent("mouseenter"));
        jest.advanceTimersByTime(500);
        await nextTick();

        expect(document.querySelector("[role='tooltip']")?.textContent?.trim()).toBe("Busy until 3:00 PM");
        wrapper.unmount();
    });

    test("the person's own face under Appear offline draws a hollow ring and no glyph", () => {
        const badge = badgeOf(withStatus(hidden, { own: true }))!;

        expect(badge.dataset.state).toBe("hidden");
        expect(badge.querySelector(".chat-status-glyph")).toBeNull();
    });

    test("Appear offline on a face not marked as the person's own draws nothing, as others see her", () => {
        expect(badgeOf(withStatus(hidden))).toBeNull();
    });

    test("no status draws nothing and makes no badge, so a face that never shows one holds no tooltip", () => {
        const wrapper = withStatus(null);
        const bare = mount(Avatar, { props: { people: anna, size: 36 }, global });

        expect(badgeOf(wrapper)).toBeNull();
        expect(badgeOf(bare)).toBeNull();
        expect(wrapper.findComponent({ name: "statusBadge.partial" }).exists()).toBe(false);
    });

    test.each([24, 28, 31])("a face of %ipx draws no status, too small for it to read", size => {
        const wrapper = mount(Avatar, { props: { people: anna, size, status: statuses.online }, global });

        expect(badgeOf(wrapper)).toBeNull();
    });

    test("a pile draws no status, whoever it would be for", () => {
        const wrapper = mount(Avatar, { props: { people: [...anna, { name: "Sam Ray", photoUrl: null }], size: 36, status: statuses.online }, global });

        expect(badgeOf(wrapper)).toBeNull();
    });

    test.each([[32, -1.3], [36, -0.7], [44, 0.4], [56, 2.2]])("sits on the rim of a %ipx face, %fpx from its right and bottom edges", (size, offset) => {
        const wrapper = mount(Avatar, { props: { people: anna, size, status: statuses.online }, global });
        const badge = badgeOf(wrapper)!;

        expect(parseFloat(badge.style.right)).toBeCloseTo(offset, 5);
        expect(parseFloat(badge.style.bottom)).toBeCloseTo(offset, 5);
    });

    test("mounted, it draws as state; a change at a later revision crossfades, and a redraw at the same revision does not", async () => {
        const wrapper = withStatus(statuses.online);
        expect(badgeOf(wrapper)!.classList.contains("is-changing")).toBe(false);

        await wrapper.setProps({ status: { ...statuses.online, tip: "Online" } });
        expect(badgeOf(wrapper)!.classList.contains("is-changing")).toBe(false);

        await wrapper.setProps({ status: { ...statuses.away, revision: 5 } });
        expect(badgeOf(wrapper)!.dataset.state).toBe("away");
        expect(badgeOf(wrapper)!.classList.contains("is-changing")).toBe(true);
    });

    /** The classes of Vue's transition a badge carries now, entering or leaving. */
    function transitionClasses(badge: HTMLElement | null): string[] {
        return [...badge?.classList ?? []].filter(name => /^chat-status-(enter|leave|appear)-/.test(name));
    }

    test("a face mounted with a status draws it at once, with no fade in", async () => {
        const wrapper = withStatus(statuses.online);
        await nextTick();

        expect(transitionClasses(badgeOf(wrapper))).toEqual([]);
    });

    test("a status arriving on a face drawn with none fades in", async () => {
        const wrapper = withStatus(null);

        await wrapper.setProps({ status: { ...statuses.online, revision: 5 } });

        expect(transitionClasses(badgeOf(wrapper))).toContain("chat-status-enter-active");
    });

    test("a status going to nothing fades out, the badge kept until its fade is over", async () => {
        const wrapper = withStatus(statuses.online);

        await wrapper.setProps({ status: null });

        expect(transitionClasses(badgeOf(wrapper))).toContain("chat-status-leave-active");
    });

    // Where status shows is a short list; every other face of a person (message rows, the @
    // picker, reactions and who reacted, thread and mention faces, quotes, the typing bubble,
    // face stacks, remove member, blocked people) draws none, so none of them asks the store.
    // The composer asks it for the Busy note's words alone, so its faces still draw none.
    test("only the places that show status read it: every other face is passed none", () => {
        const hosts = ["channelSidebar", "chatHeader", "chatSettings", "conversationDetails", "membersPanel", "newMessage", "profileCard", "peoplePicker", "statusMenu"];
        const wordsOnly = ["composer"];
        const shell = join(__dirname, "../../../../../src/Communication/Chat/ChatShell");
        const parts = ["components", "ui"].flatMap(folder => readdirSync(join(shell, folder)).filter(name => name.endsWith(".obs")).map(name => join(shell, folder, name)));
        const isPart = (path: string, names: string[]): boolean => names.some(name => path.endsWith(`${name}.partial.obs`));
        const others = parts.filter(path => !isPart(path, hosts) && !path.endsWith("avatar.partial.obs"));

        expect(others.filter(path => /<Avatar\b/.test(readFileSync(path, "utf8"))).length).toBeGreaterThanOrEqual(12);
        for (const path of others) {
            const text = readFileSync(path, "utf8");
            const isRead = !isPart(path, wordsOnly) && text.includes("useFaceStatus");
            expect([path, /<Avatar\b[^>]*:status=/.test(text) || isRead]).toEqual([path, false]);
        }
    });

    // The first screen is drawn under its veil and revealed in one crossfade: a status that comes
    // or goes before then is part of what the reveal shows, so it draws as state, as photos wait
    // for the same release.
    describe("under the first screen's veil", () => {
        const veiled = (isReleased: boolean): Record<symbol, unknown> => ({
            [pageRevealKey as symbol]: { state: { kind: "channel", isSkeletonShown: false, isSidebarRevealed: isReleased, isMainRevealed: isReleased, isReleased }, holdFace: () => undefined }
        });

        function underVeil(status: Status | null, isReleased: boolean): VueWrapper {
            return mount(Avatar, { props: { people: anna, size: 36, status }, attachTo: document.body, global: { ...global, provide: veiled(isReleased) } });
        }

        test("a status arriving before the reveal is released draws as state", async () => {
            const wrapper = underVeil(null, false);

            await wrapper.setProps({ status: { ...statuses.online, revision: 5 } });

            expect(badgeOf(wrapper)).not.toBeNull();
            expect(transitionClasses(badgeOf(wrapper))).toEqual([]);
        });

        test("a status going before the reveal is released leaves at once", async () => {
            const wrapper = underVeil(statuses.online, false);

            await wrapper.setProps({ status: null });

            expect(badgeOf(wrapper)).toBeNull();
        });

        test("once released, a status arriving fades in as anywhere", async () => {
            const wrapper = underVeil(null, true);

            await wrapper.setProps({ status: { ...statuses.online, revision: 5 } });

            expect(transitionClasses(badgeOf(wrapper))).toContain("chat-status-enter-active");
        });
    });
});
