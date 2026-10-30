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
// search reading "Search KLIPY", suggestions before anything is typed, two columns of GIFs as KLIPY
// shapes them, each blurred until it loads, a state for loading, nothing found, a failure and a
// busy search, and KLIPY named at its foot. A pick hands the GIF to the composer to stage; Escape
// closes it and hands focus back. On a phone it is a bottom sheet.
import { flushPromises, mount, VueWrapper } from "@vue/test-utils";
import { nextTick } from "vue";
import GifPicker from "../../../../../src/Communication/Chat/ChatShell/components/gifPicker.partial.obs";
import { GifSearchAnswer, Gifs, gifSuggestions } from "../../../../../src/Communication/Chat/ChatShell/composables/useGifs.partial";
import { GifResult } from "../../../../../src/Communication/Chat/ChatShell/types.partial";

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

function fakeGifs(): Gifs & { asked: Asked[] } {
    const asked: Asked[] = [];
    return {
        asked,
        isOn: () => true,
        search: (query, page) => new Promise(resolve => asked.push({ query, page, answer: resolve })),
        turnOff: () => undefined
    };
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

async function open(gifs: Gifs, props: Record<string, unknown> = {}): Promise<VueWrapper> {
    anchor = document.createElement("button");
    document.body.appendChild(anchor);
    wrapper = mount(GifPicker, { props: { gifs, anchor, ...props }, attachTo: document.body });
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
    test("opens on its search, \"Search KLIPY\", with the suggestions before anything is typed, and KLIPY named at its foot", async () => {
        const gifs = fakeGifs();
        await open(gifs);

        expect(picker().getAttribute("role")).toBe("dialog");
        expect(search().getAttribute("placeholder")).toBe("Search KLIPY");
        expect(document.activeElement).toBe(search());
        const chips = Array.from(picker().querySelectorAll("[data-testid='gif-suggestion']")).map(chip => chip.textContent?.trim());
        expect(chips).toEqual(gifSuggestions);
        expect(picker().querySelector("[data-testid='gif-foot']")?.textContent?.replace(/\s+/g, " ").trim()).toBe("Powered by KLIPY");
        expect(gifs.asked).toHaveLength(0);
    });

    test("a suggestion searches at once", async () => {
        const gifs = fakeGifs();
        await open(gifs);

        picker().querySelector<HTMLElement>("[data-testid='gif-suggestion']")?.click();
        await nextTick();

        expect(gifs.asked.map(a => a.query)).toEqual([gifSuggestions[0]]);
        expect(search().value).toBe(gifSuggestions[0]);
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

    test("draws results in two columns, each GIF's preview with no referrer, blurred until it loads", async () => {
        const gifs = fakeGifs();
        await open(gifs);
        picker().querySelector<HTMLElement>("[data-testid='gif-suggestion']")?.click();
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

        picker().querySelector<HTMLElement>("[data-testid='gif-suggestion']")?.click();
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
        const live = (): string => picker().querySelector("[aria-live]")?.textContent?.trim() ?? "";

        picker().querySelector<HTMLElement>("[data-testid='gif-suggestion']")?.click();
        gifs.asked[0].answer({ ok: false, error: { code: "rpc.unavailable", severity: "failed" }, retryAfterSeconds: null });
        await flushPromises();
        expect(live()).toBe("GIFs can't load right now");

        picker().querySelector<HTMLElement>("[data-testid='gif-retry']")?.click();
        await nextTick();
        gifs.asked[1].answer({ ok: false, error: { code: "rpc.gif_search_busy", severity: "failed", text: "GIF search is busy. Try again in a moment." }, retryAfterSeconds: 4 });
        await flushPromises();
        expect(live()).toBe("GIF search is busy. Try again in a moment.");
    });

    test("the search ends as the picker starts to leave, so nothing is asked or written on its way out", async () => {
        jest.useFakeTimers();
        const gifs = fakeGifs();
        const w = await open(gifs);

        search().value = "thanks";
        search().dispatchEvent(new Event("input"));
        await nextTick();
        (w.findComponent({ ref: "shell" }).vm as unknown as { close: (isFocusReturned: boolean) => void }).close(true);
        await nextTick();
        jest.advanceTimersByTime(1000);

        expect(gifs.asked).toHaveLength(0);
    });

    test("a pick hands the GIF over and closes, giving focus back to what opened it", async () => {
        const gifs = fakeGifs();
        const w = await open(gifs);
        picker().querySelector<HTMLElement>("[data-testid='gif-suggestion']")?.click();
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
        picker().querySelector<HTMLElement>("[data-testid='gif-suggestion']")?.click();
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
        await open(fakeGifs(), { isSheet: true });
        expect(picker().getAttribute("aria-modal")).toBe("true");
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

    test("on a phone it is a bottom sheet over a scrim", async () => {
        const gifs = fakeGifs();
        await open(gifs, { isSheet: true });

        expect(picker().classList).toContain("is-sheet");
        expect(document.querySelector("[data-testid='picker-scrim']")).not.toBeNull();
    });
});
