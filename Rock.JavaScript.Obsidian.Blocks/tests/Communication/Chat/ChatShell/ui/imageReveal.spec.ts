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
// The image reveal owner. An image stays hidden until its decode resolves; images tracked together
// show together, once all are decoded or at the cap after the first; a cached image shows at once
// without a fade; a rejected decode falls back to the load event; a stopped tracker writes nothing
// and leaves no timer; and the page's hold signal delays every reveal.
import { ref } from "vue";
import { createImageReveal, imageHoldClass, imageInClass } from "../../../../../src/Communication/Chat/ChatShell/ui/imageReveal.partial";

type Decode = { resolve: () => void, reject: () => void };

/** An image whose decode the test settles by hand. */
function makeImage(options: { cached?: boolean, decodes?: boolean } = {}): { img: HTMLImageElement, decode: Decode } {
    const img = document.createElement("img");
    const decode: Decode = { resolve: () => undefined, reject: () => undefined };

    Object.defineProperty(img, "complete", { value: options.cached === true, configurable: true });
    Object.defineProperty(img, "naturalWidth", { value: options.cached === true ? 40 : 0, configurable: true });

    if (options.decodes === false) {
        (img as { decode?: unknown }).decode = undefined;
    }
    else {
        img.decode = () => new Promise<void>((resolve, reject) => {
            decode.resolve = resolve;
            decode.reject = reject;
        });
    }

    return { img, decode };
}

async function settle(): Promise<void> {
    await Promise.resolve();
    await Promise.resolve();
}

const isHidden = (img: HTMLImageElement): boolean => img.classList.contains(imageHoldClass);
const isFading = (img: HTMLImageElement): boolean => img.classList.contains(imageInClass);

beforeEach(() => jest.useFakeTimers());
afterEach(() => jest.useRealTimers());

describe("the image reveal", () => {
    test("keeps an image hidden until its decode resolves, then fades it in", async () => {
        const reveal = createImageReveal();
        const { img, decode } = makeImage();

        reveal.track(img);
        expect(isHidden(img)).toBe(true);

        await settle();
        expect(isHidden(img)).toBe(true);

        decode.resolve();
        await settle();

        expect(isHidden(img)).toBe(false);
        expect(isFading(img)).toBe(true);
    });

    test("reveals a batch together once every image is decoded", async () => {
        const reveal = createImageReveal();
        const first = makeImage();
        const second = makeImage();

        reveal.track(first.img);
        reveal.track(second.img);

        first.decode.resolve();
        await settle();
        expect(isHidden(first.img)).toBe(true);

        second.decode.resolve();
        await settle();
        expect(isHidden(first.img)).toBe(false);
        expect(isHidden(second.img)).toBe(false);
        expect(jest.getTimerCount()).toBe(0);
    });

    test("reveals what is decoded at the cap after the first, and a slower one alone", async () => {
        const reveal = createImageReveal();
        const first = makeImage();
        const slow = makeImage();

        reveal.track(first.img);
        reveal.track(slow.img);

        first.decode.resolve();
        await settle();

        jest.advanceTimersByTime(299);
        expect(isHidden(first.img)).toBe(true);

        jest.advanceTimersByTime(1);
        expect(isHidden(first.img)).toBe(false);
        expect(isHidden(slow.img)).toBe(true);

        slow.decode.resolve();
        await settle();
        expect(isHidden(slow.img)).toBe(false);
        expect(isFading(slow.img)).toBe(true);
    });

    test("faces drawn in the same frame share a batch, and the next frame opens another", async () => {
        const a = createImageReveal({ isShared: true });
        const b = createImageReveal({ isShared: true });
        const first = makeImage();
        const second = makeImage();

        a.track(first.img);
        b.track(second.img);
        first.decode.resolve();
        await settle();
        expect(isHidden(first.img)).toBe(true);

        second.decode.resolve();
        await settle();
        expect(isHidden(first.img)).toBe(false);
        expect(isHidden(second.img)).toBe(false);

        const later = makeImage();
        const other = makeImage();
        a.track(later.img);
        later.decode.resolve();
        await settle();
        expect(isHidden(later.img)).toBe(false);

        b.track(other.img);
        expect(isHidden(other.img)).toBe(true);
    });

    test("shows a cached image at once, with no fade", () => {
        const reveal = createImageReveal();
        const { img } = makeImage({ cached: true });

        reveal.track(img);

        expect(isHidden(img)).toBe(false);
        expect(isFading(img)).toBe(false);
    });

    test("falls back to the load event when decode rejects on an image still loading", async () => {
        const reveal = createImageReveal();
        const { img, decode } = makeImage();

        reveal.track(img);
        decode.reject();
        await settle();
        expect(isHidden(img)).toBe(true);

        img.dispatchEvent(new Event("load"));
        expect(isHidden(img)).toBe(false);
    });

    test("uses the load event where decode does not exist", () => {
        const reveal = createImageReveal();
        const { img } = makeImage({ decodes: false });

        reveal.track(img);
        expect(isHidden(img)).toBe(true);

        img.dispatchEvent(new Event("load"));
        expect(isHidden(img)).toBe(false);
    });

    test("shows a broken image at once and never makes the others wait for it", async () => {
        const reveal = createImageReveal();
        const broken = makeImage();
        const good = makeImage();

        reveal.track(broken.img);
        reveal.track(good.img);

        good.decode.resolve();
        await settle();
        expect(isHidden(good.img)).toBe(true);

        broken.decode.reject();
        await settle();
        broken.img.dispatchEvent(new Event("error"));

        expect(isHidden(broken.img)).toBe(false);
        expect(isHidden(good.img)).toBe(false);
    });

    test("a stopped tracker writes nothing and leaves no timer", async () => {
        const reveal = createImageReveal();
        const first = makeImage();
        const second = makeImage();

        reveal.track(first.img);
        reveal.track(second.img);
        first.decode.resolve();
        await settle();
        expect(jest.getTimerCount()).toBe(1);

        reveal.stop();
        expect(jest.getTimerCount()).toBe(0);

        second.decode.resolve();
        await settle();
        jest.advanceTimersByTime(1000);

        expect(isHidden(first.img)).toBe(true);
        expect(isHidden(second.img)).toBe(true);
        expect(isFading(first.img)).toBe(false);

        const late = makeImage();
        reveal.track(late.img);
        expect(late.img.className).toBe("");
    });

    test("an image that leaves does not hold the rest of its batch", async () => {
        const reveal = createImageReveal();
        const gone = makeImage();
        const stays = makeImage();

        reveal.track(gone.img);
        reveal.track(stays.img);
        stays.decode.resolve();
        await settle();

        reveal.untrack(gone.img);

        expect(isHidden(stays.img)).toBe(false);
    });

    test("waits for the page's hold, then shows what was ready", async () => {
        const hold = ref(true);
        const reveal = createImageReveal({ hold });
        const cached = makeImage({ cached: true });
        const loaded = makeImage();

        reveal.track(cached.img);
        reveal.track(loaded.img);
        loaded.decode.resolve();
        await settle();

        expect(isHidden(cached.img)).toBe(true);
        expect(isHidden(loaded.img)).toBe(true);

        hold.value = false;

        expect(isHidden(cached.img)).toBe(false);
        expect(isFading(cached.img)).toBe(false);
        expect(isHidden(loaded.img)).toBe(false);
    });
});
