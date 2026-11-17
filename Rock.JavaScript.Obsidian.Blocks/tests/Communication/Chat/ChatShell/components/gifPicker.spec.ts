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

// The GIF picker, mounted: a popover the emoji picker's size opened from the composer's "+", its
// search reading "Search KLIPY", the topic tiles before anything is typed, two columns of GIFs as KLIPY
// shapes them, each blurred until it loads, a state for loading, nothing found, a failure and a
// busy search, and KLIPY named at its foot. A pick hands the GIF to the composer to stage; Escape
// closes it and hands focus back. On a phone it is a bottom sheet.
import { flushPromises, mount, VueWrapper } from "@vue/test-utils";
import { defineComponent, h, nextTick, ref } from "vue";
import GifPicker from "../../../../../src/Communication/Chat/ChatShell/components/gifPicker.partial.obs";
import Sheet from "../../../../../src/Communication/Chat/ChatShell/ui/sheet.partial.obs";
import SurfacePage from "../../../../../src/Communication/Chat/ChatShell/ui/surfacePage.partial.obs";
import { readFileSync } from "fs";
import { join } from "path";
import { GifCovers, GifSearchAnswer, Gifs } from "../../../../../src/Communication/Chat/ChatShell/composables/useGifs.partial";
import { GifResult } from "../../../../../src/Communication/Chat/ChatShell/types.partial";
import { providedSession } from "../sessionShapes";

function result(slug: string, width = 200, height = 112): GifResult {
    return {
        token: `token-${slug}`,
        slug,
        title: `Title ${slug}`,
        url: `https://static.klipy.com/ii/md/${slug}.mp4`,
        still: `https://static.klipy.com/ii/md/${slug}.jpg`,
        preview: { url: `https://static.klipy.com/ii/sm/${slug}.webp`, width, height },
        blur: "data:image/jpeg;base64,AAAA",
        width: 498,
        height: 280
    };
}

type Asked = { query: string, page: number, answer: (value: GifSearchAnswer) => void };

/** The platform's ten topic words, each with a cover of its own shape. */
const words = ["Thanks", "Praying", "Amen", "Celebrate", "Welcome", "Haha", "Wow", "Love it", "Yes", "Coffee"];
const coversOf = (list = words): GifCovers => ({ ok: true, words: list.map((word, index) => ({ word, still: `https://static.klipy.com/ii/sm/${index}.webp`, width: 90, height: 50 + index * 4 })) });

/** A search whose asks the test answers by hand, and whose covers come as given, once asked. */
function fakeGifs(covers: () => Promise<GifCovers> = async () => coversOf()): Gifs & { asked: Asked[], coversAsked: number } {
    const asked: Asked[] = [];
    const fake = {
        asked,
        coversAsked: 0,
        isOn: () => true,
        search: (query: string, page: number) => new Promise<GifSearchAnswer>(resolve => asked.push({ query, page, answer: resolve })),
        covers: () => {
            fake.coversAsked++;
            return covers();
        },
        turnOff: () => undefined
    };
    return fake;
}

let wrapper: VueWrapper | null = null;
let anchor: HTMLButtonElement | null = null;

afterEach(() => {
    wrapper?.unmount();
    wrapper = null;
    anchor?.remove();
    anchor = null;
    jest.useRealTimers();
});

async function open(gifs: Gifs, { isPhone = false }: { isPhone?: boolean } = {}): Promise<VueWrapper> {
    anchor = document.createElement("button");
    document.body.appendChild(anchor);
    wrapper = mount(GifPicker, { props: { gifs, anchor }, attachTo: document.body, global: { provide: providedSession({ isPhone }) } });
    await flushPromises();
    return wrapper;
}

function picker(): HTMLElement {
    return document.querySelector<HTMLElement>("[data-testid='gif-picker']") as HTMLElement;
}

function search(): HTMLInputElement {
    return picker().querySelector<HTMLInputElement>("input") as HTMLInputElement;
}

async function answered(gifs: ReturnType<typeof fakeGifs>, items: GifResult[], hasNext = false): Promise<void> {
    gifs.asked[gifs.asked.length - 1].answer({ ok: true, items, hasNext });
    await flushPromises();
}

describe("the GIF picker", () => {
    test("opens on its search, \"Search KLIPY\", with the topic tiles before anything is typed, and KLIPY named at its foot", async () => {
        const gifs = fakeGifs();
        await open(gifs);

        expect(picker().getAttribute("role")).toBe("dialog");
        expect(search().getAttribute("placeholder")).toBe("Search KLIPY");
        expect(document.activeElement).toBe(search());
        // ten tiles in the results' two columns, each its cover still with its word on a plate
        const columns = Array.from(picker().querySelectorAll("[data-testid='gif-topic-column']"));
        expect(columns).toHaveLength(2);
        const tiles = Array.from(picker().querySelectorAll<HTMLElement>("[data-testid='gif-topic']"));
        expect(tiles.map(tile => tile.querySelector("[data-testid='gif-topic-plate']")?.textContent?.trim()).sort()).toEqual([...words].sort());
        expect(tiles.every(tile => tile.querySelector("img")?.getAttribute("referrerpolicy") === "no-referrer")).toBe(true);
        expect(tiles.find(tile => tile.textContent?.trim() === "Thanks")?.getAttribute("aria-label")).toBe("Search Thanks");
        expect(picker().querySelector("[data-testid='gif-foot']")?.textContent?.replace(/\s+/g, " ").trim()).toBe("Powered by KLIPY");
        expect(gifs.coversAsked).toBe(1);
        expect(gifs.asked).toHaveLength(0);
    });

    test("a topic tile searches its word at once", async () => {
        const gifs = fakeGifs();
        await open(gifs);

        picker().querySelector<HTMLElement>("[data-testid='gif-topic']")?.click();
        await nextTick();

        expect(gifs.asked.map(a => a.query)).toEqual(["Thanks"]);
        expect(search().value).toBe("Thanks");
    });

    test("a word with no cover is a plain plate, still a tile that searches", async () => {
        const gifs = fakeGifs(async () => ({ ok: true, words: [{ word: "Amen", still: null, width: 0, height: 0 }, ...(coversOf(["Yes"]) as { words: [] }).words] }));
        await open(gifs);

        const amen = Array.from(picker().querySelectorAll<HTMLElement>("[data-testid='gif-topic']")).find(tile => tile.textContent?.trim() === "Amen");
        expect(amen?.querySelector("img")).toBeNull();
        expect(amen?.querySelector("[data-testid='gif-topic-plate']")?.textContent?.trim()).toBe("Amen");
        amen?.click();
        await nextTick();
        expect(gifs.asked.map(a => a.query)).toEqual(["Amen"]);
    });

    test("covers that could not load show the error with Try again, which asks for them again", async () => {
        let isFailing = true;
        const gifs = fakeGifs(async () => isFailing ? { ok: false } : coversOf());
        await open(gifs);

        expect(picker().querySelector("[data-testid='gif-topic']")).toBeNull();
        expect(picker().querySelector("[data-testid='gif-error']")).not.toBeNull();
        isFailing = false;
        picker().querySelector<HTMLElement>("[data-testid='gif-retry']")?.click();
        await flushPromises();

        expect(gifs.coversAsked).toBe(2);
        expect(picker().querySelectorAll("[data-testid='gif-topic']")).toHaveLength(10);
        expect(gifs.asked).toHaveLength(0);
    });

    test("Down from the search goes to the first topic, the arrows move by the columns, and Enter searches it", async () => {
        const gifs = fakeGifs();
        await open(gifs);

        search().dispatchEvent(new KeyboardEvent("keydown", { key: "ArrowDown", bubbles: true }));
        await nextTick();
        expect(document.activeElement?.textContent?.trim()).toBe("Thanks");

        document.activeElement?.dispatchEvent(new KeyboardEvent("keydown", { key: "ArrowRight", bubbles: true }));
        await nextTick();
        expect(document.activeElement?.textContent?.trim()).toBe("Praying");

        document.activeElement?.dispatchEvent(new KeyboardEvent("keydown", { key: "Enter", bubbles: true }));
        await nextTick();
        expect(gifs.asked.map(a => a.query)).toEqual(["Praying"]);
    });

    // The topics give way to the results, and clearing the search brings them back, by the one
    // entrance every view takes: a rise, which the token sheet takes to nothing under reduced motion.
    test("each view comes in by the sheet's enter, a fade under reduced motion", () => {
        const source = readFileSync(join(__dirname, "../../../../../src/Communication/Chat/ChatShell/components/gifPicker.partial.obs"), "utf8");
        const style = source.slice(source.indexOf("<style"), source.indexOf("</style>"));

        expect(/\.chat-gif-masonry\s*\{([^}]*)\}/.exec(style)?.[1]).toMatch(/animation:\s*chat-enter var\(--chat-t-base\) var\(--chat-ease-in\)/);
        expect(style).not.toMatch(/prefers-reduced-motion/);
    });

    // A cover can be white where the plate sits, so the word must read at 4.5 to 1 over white, in
    // the colours the plate itself names, in every theme the token sheet sets them for.
    test("a topic's word reads 4.5 to 1 on its plate over a white cover", () => {
        const source = readFileSync(join(__dirname, "../../../../../src/Communication/Chat/ChatShell/components/gifPicker.partial.obs"), "utf8");
        const sheet = readFileSync(join(__dirname, "../../../../../../RockWeb/Styles/Blocks/Communication/Chat/chat.css"), "utf8");
        const plate = /\.chat-gif-plate\s*\{([^}]*)\}/.exec(source)?.[1] ?? "";
        const tokenIn = (property: string): string => new RegExp(`(?:^|;|\\s)${property}:\\s*var\\(--([\\w-]+)\\)`).exec(plate)?.[1] ?? "";
        const valuesOf = (token: string): string[] => [...sheet.matchAll(new RegExp(`--${token}:\\s*([^;]+);`, "g"))].map(match => match[1].trim());
        const rgbaOf = (value: string): number[] => {
            const rgba = /rgba?\(([\d.\s,]+)\)/.exec(value);
            if (rgba) {
                const [r, g, b, a = 1] = rgba[1].split(",").map(Number);
                return [r, g, b, a];
            }
            const hex = /#([0-9a-f]{6})/i.exec(value)?.[1] ?? "000000";
            return [0, 2, 4].map(at => parseInt(hex.slice(at, at + 2), 16)).concat(1);
        };
        const luminance = (rgb: number[]): number => {
            const [r, g, b] = rgb.map(channel => {
                const c = channel / 255;
                return c <= 0.03928 ? c / 12.92 : Math.pow((c + 0.055) / 1.055, 2.4);
            });
            return 0.2126 * r + 0.7152 * g + 0.0722 * b;
        };

        const backgrounds = valuesOf(tokenIn("background"));
        const inks = valuesOf(tokenIn("color"));
        expect(backgrounds.length).toBeGreaterThan(0);
        expect(inks.length).toBeGreaterThan(0);
        for (const background of backgrounds) {
            const [r, g, b, a] = rgbaOf(background);
            const ground = [r, g, b].map(channel => channel * a + 255 * (1 - a));
            for (const ink of inks) {
                const [high, low] = [luminance(rgbaOf(ink)), luminance(ground)].sort((x, y) => y - x);
                expect((high + 0.05) / (low + 0.05)).toBeGreaterThanOrEqual(4.5);
            }
        }
    });

    test("typing asks once after the person stops, and shows the loading tiles meanwhile", async () => {
        jest.useFakeTimers();
        const gifs = fakeGifs();
        await open(gifs);

        search().value = "thanks";
        search().dispatchEvent(new Event("input"));
        await nextTick();
        expect(picker().querySelector("[data-testid='gif-loading']")).not.toBeNull();
        expect(gifs.asked).toHaveLength(0);

        jest.advanceTimersByTime(300);
        expect(gifs.asked.map(a => a.query)).toEqual(["thanks"]);
    });

    // A wait is the still skeleton (no pulse): the shared part at GIF-like shapes in the grid's two
    // columns; a GIF whose preview is on its way shows KLIPY's blur, still, until it sharpens.
    test("the loading tiles are still skeletons at GIF-like shapes, and nothing in the picker pulses", async () => {
        jest.useFakeTimers();
        const gifs = fakeGifs();
        await open(gifs);

        search().value = "thanks";
        search().dispatchEvent(new Event("input"));
        await nextTick();

        const loading = picker().querySelector("[data-testid='gif-loading']") as HTMLElement;
        const tiles = Array.from(loading.querySelectorAll<HTMLElement>(".chat-skel.is-media"));
        expect(tiles).toHaveLength(6);
        expect(tiles.every(tile => tile.getAttribute("aria-hidden") === "true" && tile.style.aspectRatio !== "")).toBe(true);

        const source = readFileSync(join(__dirname, "../../../../../src/Communication/Chat/ChatShell/components/gifPicker.partial.obs"), "utf8");
        const style = source.slice(source.indexOf("<style"), source.indexOf("</style>"));
        expect(style).not.toMatch(/chat-skeleton-pulse|--chat-loop-stop|--chat-d-skeleton/);
    });

    test("draws results in two columns, each GIF's preview with no referrer, blurred until it loads", async () => {
        const gifs = fakeGifs();
        await open(gifs);
        picker().querySelector<HTMLElement>("[data-testid='gif-topic']")?.click();
        await answered(gifs, [result("a", 100, 100), result("b", 100, 50), result("c", 100, 50)]);

        const columns = picker().querySelectorAll("[data-testid='gif-column']");
        expect(columns).toHaveLength(2);
        expect(Array.from(columns[0].querySelectorAll("[data-testid='gif-tile']")).map(tile => tile.getAttribute("data-slug"))).toEqual(["a"]);
        expect(Array.from(columns[1].querySelectorAll("[data-testid='gif-tile']")).map(tile => tile.getAttribute("data-slug"))).toEqual(["b", "c"]);

        const tile = picker().querySelector<HTMLElement>("[data-slug='a']") as HTMLElement;
        const image = tile.querySelector("img") as HTMLImageElement;
        expect(image.getAttribute("src")).toBe("https://static.klipy.com/ii/sm/a.webp");
        expect(image.getAttribute("referrerpolicy")).toBe("no-referrer");
        expect(tile.getAttribute("aria-label")).toBe("Title a");
        expect(tile.style.backgroundImage).toContain("data:image/jpeg;base64,AAAA");
        expect(tile.classList).toContain("is-loading");

        image.dispatchEvent(new Event("load"));
        await nextTick();
        expect(tile.classList).not.toContain("is-loading");
    });

    test("nothing found, a failure with Try again, and a busy search each say so", async () => {
        const gifs = fakeGifs();
        await open(gifs);

        picker().querySelector<HTMLElement>("[data-testid='gif-topic']")?.click();
        await answered(gifs, []);
        expect(picker().querySelector("[data-testid='gif-empty']")?.textContent).toContain("No GIFs found");

        search().value = "thanks";
        search().dispatchEvent(new KeyboardEvent("keydown", { key: "Enter" }));
        await nextTick();
        gifs.asked[gifs.asked.length - 1].answer({ ok: false, error: { code: "rpc.unavailable", severity: "failed" }, retryAfterSeconds: null });
        await flushPromises();
        expect(picker().querySelector("[data-testid='gif-error']")?.textContent).toContain("GIFs can't load right now");

        picker().querySelector<HTMLElement>("[data-testid='gif-retry']")?.click();
        await nextTick();
        gifs.asked[gifs.asked.length - 1].answer({ ok: false, error: { code: "rpc.gif_search_busy", severity: "failed", text: "GIF search is busy. Try again in a moment." }, retryAfterSeconds: 4 });
        await flushPromises();
        expect(picker().querySelector("[data-testid='gif-error']")?.textContent).toContain("GIF search is busy. Try again in a moment.");
    });

    test("a screen reader hears a failure and a busy search, not only results", async () => {
        const gifs = fakeGifs();
        await open(gifs);
        const live = (): string => document.querySelector("[data-testid='announcer-polite']")?.textContent?.trim() ?? "";

        picker().querySelector<HTMLElement>("[data-testid='gif-topic']")?.click();
        gifs.asked[0].answer({ ok: false, error: { code: "rpc.unavailable", severity: "failed" }, retryAfterSeconds: null });
        await flushPromises();
        expect(live()).toBe("GIFs can't load right now");

        picker().querySelector<HTMLElement>("[data-testid='gif-retry']")?.click();
        await nextTick();
        gifs.asked[1].answer({ ok: false, error: { code: "rpc.gif_search_busy", severity: "failed", text: "GIF search is busy. Try again in a moment." }, retryAfterSeconds: 4 });
        await flushPromises();
        expect(live()).toBe("GIF search is busy. Try again in a moment.");
    });

    // Both failures draw the same error, so both are heard alike and both are reached alike: Down
    // from the search goes into the body, to Try again where there are no tiles to move through.
    test("covers that could not load are heard as a failed search is, and Down from the search reaches Try again in either", async () => {
        const gifs = fakeGifs(async () => ({ ok: false }));
        await open(gifs);
        const live = (): string => document.querySelector("[data-testid='announcer-polite']")?.textContent?.trim() ?? "";

        expect(live()).toBe("GIFs can't load right now");
        search().dispatchEvent(new KeyboardEvent("keydown", { key: "ArrowDown", bubbles: true }));
        await nextTick();
        expect(document.activeElement?.getAttribute("data-testid")).toBe("gif-retry");

        search().focus();
        search().value = "thanks";
        search().dispatchEvent(new KeyboardEvent("keydown", { key: "Enter" }));
        await nextTick();
        gifs.asked[0].answer({ ok: false, error: { code: "rpc.unavailable", severity: "failed" }, retryAfterSeconds: null });
        await flushPromises();
        search().dispatchEvent(new KeyboardEvent("keydown", { key: "ArrowDown", bubbles: true }));
        await nextTick();
        expect(document.activeElement?.getAttribute("data-testid")).toBe("gif-retry");
    });

    test("the search ends as the picker starts to leave, so nothing is asked or written on its way out", async () => {
        jest.useFakeTimers();
        const gifs = fakeGifs();
        await open(gifs);

        search().value = "thanks";
        search().dispatchEvent(new Event("input"));
        await nextTick();
        search().dispatchEvent(new KeyboardEvent("keydown", { key: "Escape", bubbles: true }));
        await nextTick();
        jest.advanceTimersByTime(1000);

        expect(gifs.asked).toHaveLength(0);
    });

    test("a pick hands the GIF over and closes, giving focus back to what opened it", async () => {
        const gifs = fakeGifs();
        const w = await open(gifs);
        picker().querySelector<HTMLElement>("[data-testid='gif-topic']")?.click();
        await answered(gifs, [result("a")]);

        picker().querySelector<HTMLElement>("[data-slug='a']")?.click();
        await flushPromises();

        expect(w.emitted("pick")).toEqual([[result("a")]]);
        expect(w.emitted("close")).toHaveLength(1);
        expect(document.activeElement).toBe(anchor);
    });

    test("Down from the search goes to the first GIF, the arrows move by the columns, and Enter picks", async () => {
        const gifs = fakeGifs();
        const w = await open(gifs);
        picker().querySelector<HTMLElement>("[data-testid='gif-topic']")?.click();
        await answered(gifs, [result("a", 100, 100), result("b", 100, 50), result("c", 100, 50)]);

        search().dispatchEvent(new KeyboardEvent("keydown", { key: "ArrowDown", bubbles: true }));
        await nextTick();
        expect(document.activeElement?.getAttribute("data-slug")).toBe("a");

        document.activeElement?.dispatchEvent(new KeyboardEvent("keydown", { key: "ArrowRight", bubbles: true }));
        await nextTick();
        expect(document.activeElement?.getAttribute("data-slug")).toBe("b");

        document.activeElement?.dispatchEvent(new KeyboardEvent("keydown", { key: "ArrowDown", bubbles: true }));
        await nextTick();
        expect(document.activeElement?.getAttribute("data-slug")).toBe("c");

        document.activeElement?.dispatchEvent(new KeyboardEvent("keydown", { key: "Enter", bubbles: true }));
        await flushPromises();
        expect(w.emitted("pick")).toEqual([[result("c", 100, 50)]]);
    });

    test("a sheet over a scrim is modal for a screen reader, a popover is not", async () => {
        await open(fakeGifs(), { isPhone: true });
        expect(picker().closest("[data-testid='sheet']")?.getAttribute("aria-modal")).toBe("true");
        wrapper?.unmount();
        wrapper = null;

        await open(fakeGifs());
        expect(picker().getAttribute("aria-modal")).toBeNull();
    });

    test("Escape closes and gives focus back to what opened it", async () => {
        const gifs = fakeGifs();
        const w = await open(gifs);

        search().dispatchEvent(new KeyboardEvent("keydown", { key: "Escape", bubbles: true }));
        await flushPromises();

        expect(w.emitted("close")).toHaveLength(1);
        expect(w.emitted("pick")).toBeUndefined();
        expect(document.activeElement).toBe(anchor);
    });

    // The shared paging trigger: a sentinel at the end of the GIFs asks the next page as it comes into view.
    test("asks the next page as the end of the GIFs comes into view, and not while the last page is all there is", async () => {
        const seen: Array<{ callback: IntersectionObserverCallback, watched: Element[] }> = [];
        const original = window.IntersectionObserver;
        window.IntersectionObserver = class {
            public watched: Element[] = [];
            constructor(public callback: IntersectionObserverCallback) {
                seen.push(this);
            }
            observe(element: Element): void {
                this.watched.push(element);
            }
            unobserve(): void { /* Nothing is watched apart. */ }
            disconnect(): void {
                this.watched = [];
            }
        } as unknown as typeof IntersectionObserver;
        try {
            const gifs = fakeGifs();
            await open(gifs);
            picker().querySelector<HTMLElement>("[data-testid='gif-topic']")?.click();
            await answered(gifs, [result("a"), result("b")], true);

            const end = seen.find(observer => observer.watched.length > 0);
            end?.callback(end.watched.map(target => ({ target, isIntersecting: true }) as IntersectionObserverEntry), {} as IntersectionObserver);
            await flushPromises();
            expect(gifs.asked.map(a => a.page)).toEqual([1, 2]);

            await answered(gifs, [result("c")], false);
            const last = seen.filter(observer => observer.watched.length > 0).pop();
            last?.callback(last.watched.map(target => ({ target, isIntersecting: true }) as IntersectionObserverEntry), {} as IntersectionObserver);
            await flushPromises();
            expect(gifs.asked.map(a => a.page)).toEqual([1, 2]);
        }
        finally {
            window.IntersectionObserver = original;
        }
    });

    test("on a phone it is a bottom sheet over a scrim", async () => {
        const gifs = fakeGifs();
        await open(gifs, { isPhone: true });

        expect(picker().closest("[data-testid='sheet']")).not.toBeNull();
        expect(document.querySelector("[data-testid='scrim']")).not.toBeNull();
    });
});

// On a phone the plus menu's sheet turns into the picker, so the picker is a page of that sheet:
// it draws no surface of its own, the sheet's head carries Back and the page's title, and closing
// is the sheet owner's, as a pick hands the GIF over.
describe("the GIF picker as a page of a turned sheet", () => {
    const original = window.matchMedia;

    afterEach(() => {
        window.matchMedia = original;
        document.body.innerHTML = "";
    });

    /** The plus menu's sheet, turned to the GIF page as the GIF row would turn it. */
    async function turned(gifs: Gifs, isTouch = false): Promise<VueWrapper> {
        window.matchMedia = ((query: string) => ({ media: query, matches: isTouch && query === "(hover: none)" })) as unknown as typeof window.matchMedia;
        const isGif = ref(false);
        const Host = defineComponent({
            setup() {
                return () => h(Sheet, { isOpen: true, label: "Add to your message", height: "picker" }, {
                    default: () => h("button", { "data-testid": "to-gif", onClick: () => {
                        isGif.value = true;
                    } }, "GIF"),
                    pages: () => h(SurfacePage, { isOpen: isGif.value, title: "GIF", onBack: () => {
                        isGif.value = false;
                    } }, () => isGif.value ? h(GifPicker, { gifs, isPage: true }) : null)
                });
            }
        });

        wrapper = mount(Host, { attachTo: document.body, global: { provide: providedSession({ isPhone: true }) } });
        await flushPromises();
        const opener = document.querySelector<HTMLElement>("[data-testid='to-gif']") as HTMLElement;
        opener.focus();
        opener.click();
        await flushPromises();
        return wrapper;
    }

    test("draws no surface of its own: one sheet over one scrim, its search under the sheet's Back and GIF title", async () => {
        await turned(fakeGifs());

        expect(document.querySelectorAll("[data-testid='sheet']")).toHaveLength(1);
        expect(document.querySelectorAll("[data-testid='scrim']")).toHaveLength(1);
        expect(document.querySelector("[data-testid='page-title']")?.textContent).toBe("GIF");
        expect(picker().closest("[data-testid='sheet']")).not.toBeNull();
        expect(picker().getAttribute("role")).toBeNull();
        expect(search().getAttribute("placeholder")).toBe("Search KLIPY");
        expect(document.activeElement).toBe(search());
    });

    test("on a touch screen leaves focus to the page's Back, so no keyboard rises over the GIFs", async () => {
        await turned(fakeGifs(), true);

        expect(document.activeElement).toBe(document.querySelector("[data-testid='page-back']"));
    });

    test("a pick hands the GIF over and leaves closing to the sheet's owner", async () => {
        const gifs = fakeGifs();
        const w = await turned(gifs);
        picker().querySelector<HTMLElement>("[data-testid='gif-topic']")?.click();
        await answered(gifs, [result("a")]);

        picker().querySelector<HTMLElement>("[data-slug='a']")?.click();
        await flushPromises();

        const page = w.findComponent(GifPicker);
        expect(page.emitted("pick")).toEqual([[result("a")]]);
        expect(page.emitted("close")).toBeUndefined();
        expect(document.querySelectorAll("[data-testid='sheet']")).toHaveLength(1);
    });

    test("Back returns to the plus menu and ends the search", async () => {
        jest.useFakeTimers();
        const gifs = fakeGifs();
        await turned(gifs);
        search().value = "thanks";
        search().dispatchEvent(new Event("input"));
        await nextTick();

        document.querySelector<HTMLElement>("[data-testid='page-back']")?.click();
        await nextTick();
        jest.advanceTimersByTime(1000);

        expect(document.querySelector("[data-testid='gif-picker']")).toBeNull();
        expect(gifs.asked).toHaveLength(0);
    });
});
