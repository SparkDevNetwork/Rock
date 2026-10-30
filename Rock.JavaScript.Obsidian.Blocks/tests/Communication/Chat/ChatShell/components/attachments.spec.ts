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
// A message's files, mounted: photos in a grid that holds its size before anything loads, other
// files as cards that download under their own names, and a viewer over the chat for the photos.
import { readFileSync } from "fs";
import { join } from "path";
import { mount, VueWrapper } from "@vue/test-utils";
import { nextTick, reactive } from "vue";
import Attachments from "../../../../../src/Communication/Chat/ChatShell/components/attachments.partial.obs";
import { AttachmentTray, attachmentViewsKey, createAttachments, TrayFile } from "../../../../../src/Communication/Chat/ChatShell/composables/useAttachments.partial";
import { AttachmentItem, FileItem, GifItem } from "../../../../../src/Communication/Chat/ChatShell/types.partial";
import { harness, Harness, projectUrl, settle, tenant } from "../attachmentFakes";

function image(n: number, extra: Partial<FileItem> = {}): FileItem {
    const id = `d0000000-0000-4000-8000-${String(n).padStart(12, "0")}`;
    return { id, key: `${tenant}/${id}`, kind: "image", name: `photo-${n}.jpg`, size: 1000, type: "image/jpeg", width: 800, height: 600, ...extra };
}

function file(n: number, name: string, size = 1_400_000): FileItem {
    const id = `d0000000-0000-4000-8000-${String(n).padStart(12, "0")}`;
    return { id, key: `${tenant}/${id}`, kind: "file", name, size, type: "application/pdf", width: null, height: null };
}

function gif(n: number, extra: Partial<GifItem> = {}): GifItem {
    const id = `e0000000-0000-4000-8000-${String(n).padStart(12, "0")}`;
    return { id, kind: "gif", slug: `slug-${n}`, title: `Thank you dance ${n}`, url: `https://static.klipy.com/ii/md/${n}.mp4?v=2`, still: `https://static.klipy.com/ii/md/${n}.jpg`, width: 498, height: 280, ...extra };
}

let wrapper: VueWrapper | null = null;
let shell: HTMLElement | null = null;

afterEach(() => {
    wrapper?.unmount();
    wrapper = null;
    shell?.remove();
    shell = null;
});

/** Mounts a message's files inside a chat shell, as a row draws them, with the session's link cache. */
async function mountFiles(items: AttachmentItem[], h: Harness = harness(), props: Record<string, unknown> = {}): Promise<{ wrapper: VueWrapper, h: Harness }> {
    shell = document.createElement("div");
    shell.className = "chat-shell";
    document.body.appendChild(shell);
    const attachments = createAttachments(h.dependencies);
    wrapper = mount(Attachments, {
        props: { items, ...props },
        attachTo: shell,
        global: { provide: { [attachmentViewsKey as symbol]: attachments } }
    });
    await settle();
    await nextTick();
    return { wrapper, h };
}

describe("the photo grid", () => {
    test.each([
        [1, "1"],
        [2, "2"],
        [3, "3"],
        [4, "4+"],
        [6, "4+"]
    ])("%i photos lay out as %s", async (count, layout) => {
        const { wrapper } = await mountFiles(Array.from({ length: count }, (_, i) => image(i + 1)));

        const grid = wrapper.get("[data-testid='attachment-grid']");
        expect(grid.attributes("data-layout")).toBe(layout);
        expect(wrapper.findAll("[data-testid='attachment-image']")).toHaveLength(Math.min(count, 4));
    });

    test("photos past the fourth are counted on the fourth", async () => {
        const { wrapper } = await mountFiles(Array.from({ length: 6 }, (_, i) => image(i + 1)));

        expect(wrapper.get("[data-testid='attachment-more']").text()).toBe("+2");
    });

    test("a single photo holds its declared shape before it loads", async () => {
        const { wrapper } = await mountFiles([image(1, { width: 1200, height: 900 })]);

        const tile = wrapper.get("[data-testid='attachment-image']");
        expect((tile.element as HTMLElement).style.aspectRatio).toBe("1200 / 900");
        const img = tile.get("img");
        expect(img.attributes("width")).toBe("1200");
        expect(img.attributes("height")).toBe("900");
        expect(img.attributes("alt")).toBe("photo-1.jpg");
    });

    // The image waits, hidden, in its held box until the browser has decoded it; one message's
    // photos reveal together.
    test("a message's photos stay hidden until all are decoded, then show together", async () => {
        const decodes: Array<() => void> = [];
        const original = Object.getOwnPropertyDescriptor(HTMLImageElement.prototype, "decode");
        HTMLImageElement.prototype.decode = () => new Promise<void>(resolve => decodes.push(resolve));

        try {
            const { wrapper } = await mountFiles([image(1), image(2)]);
            const images = wrapper.findAll("[data-testid='attachment-image'] img");

            expect(images).toHaveLength(2);
            expect(images.every(img => img.classes("chat-img-hold"))).toBe(true);

            decodes[0]();
            await settle();
            expect(images.every(img => img.classes("chat-img-hold"))).toBe(true);

            decodes[1]();
            await settle();
            expect(images.some(img => img.classes("chat-img-hold"))).toBe(false);
            expect(images.every(img => img.classes("chat-img-in"))).toBe(true);
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

    test("every photo on screen is linked in one batch, drawn from its signed link", async () => {
        const { wrapper, h } = await mountFiles([image(1), image(2)]);

        expect(h.signs).toHaveLength(1);
        expect(h.signs[0].body.paths).toEqual([image(1).key, image(2).key]);
        const [first] = wrapper.findAll("[data-testid='attachment-image'] img");
        expect(first.attributes("src")).toBe(`${projectUrl}/storage/v1/object/sign/chat-attachments/${image(1).key}?token=t-0001`);
    });

    test("a photo whose link was refused says it is unavailable rather than drawing a broken image", async () => {
        const h = harness();
        h.signAnswer.value = paths => paths.map(path => ({ path, signedURL: null, error: "not found" }));
        const { wrapper } = await mountFiles([image(1)], h);

        const tile = wrapper.get("[data-testid='attachment-image']");
        expect(tile.find("img").exists()).toBe(false);
        expect(tile.attributes("data-unavailable")).toBe("true");
        expect(tile.text()).toContain("Unavailable");
    });
});

describe("file cards", () => {
    test("a file shows its name and size and downloads under its own name", async () => {
        const { wrapper } = await mountFiles([file(1, "Q3 report #1.pdf")]);

        const card = wrapper.get("[data-testid='attachment-file']");
        expect(card.get("[data-testid='attachment-file-name']").text()).toBe("Q3 report #1.pdf");
        expect(card.get("[data-testid='attachment-file-size']").text()).toBe("1.3 MB");
        expect(card.element.tagName).toBe("A");
        expect(card.attributes("href")).toBe(`${projectUrl}/storage/v1/object/sign/chat-attachments/${file(1, "").key}?token=t-0001&download=Q3%20report%20%231.pdf`);
        expect(card.attributes("rel")).toBe("noopener");
        expect(card.attributes("aria-label")).toBe("Download Q3 report #1.pdf, 1.3 MB");
    });

    test("photos and files together draw the grid first, then the cards", async () => {
        const { wrapper } = await mountFiles([file(1, "a.pdf"), image(2), file(3, "b.pdf")]);

        const parts = wrapper.findAll("[data-testid='attachment-grid'], [data-testid='attachment-file']").map(part => part.attributes("data-testid"));
        expect(parts).toEqual(["attachment-grid", "attachment-file", "attachment-file"]);
    });
});

/**
 * Records each click reaching the document, with its link and whether the card held it back. jsdom
 * has no navigation, so each is held back once recorded; a card clicked over and over is taken off
 * the page after a few, so a loop ends and fails rather than hanging the run.
 */
function recordClicks(card: Element): Array<{ href: string | null, isPrevented: boolean }> & { stop: () => void } {
    const clicks: Array<{ href: string | null, isPrevented: boolean }> = [];
    const record = (event: Event): void => {
        clicks.push({ href: (event.target as Element).closest("a")?.getAttribute("href") ?? null, isPrevented: event.defaultPrevented });
        event.preventDefault();
        if (clicks.length > 5) {
            card.remove();
        }
    };
    document.addEventListener("click", record);
    return Object.assign(clicks, { stop: () => document.removeEventListener("click", record) });
}

describe("links over time", () => {
    test("a row's links are renewed before they end while it is mounted, so its photo and card keep working, and stop once it is gone", async () => {
        const { wrapper: row, h } = await mountFiles([image(1), file(2, "a.pdf")]);
        expect(h.signs).toHaveLength(1);

        h.signAnswer.value = paths => paths.map(path => ({ path, signedURL: `/object/sign/chat-attachments/${path}?token=renewed`, error: null }));
        await h.advance(9 * 60 * 1000 + 30 * 1000);
        await nextTick();

        expect(h.signs).toHaveLength(2);
        expect(h.signs[1].body.paths).toEqual([image(1).key, file(2, "").key]);
        expect(row.get("[data-testid='attachment-image'] img").attributes("src")).toContain("token=renewed");
        expect(row.get("[data-testid='attachment-file']").attributes("href")).toContain("token=renewed");

        row.unmount();
        wrapper = null;
        await h.advance(60 * 60 * 1000);
        expect(h.signs).toHaveLength(2);
        expect(h.timers).toEqual([]);
    });

    test("a card clicked as its link nears its end waits for a fresh link, then downloads from it", async () => {
        const h = harness();
        const { wrapper } = await mountFiles([file(1, "a.pdf")], h);
        const clicks: Array<{ href: string | null, isPrevented: boolean }> = [];
        const record = (event: Event): void => {
            clicks.push({ href: (event.target as Element).closest("a")?.getAttribute("href") ?? null, isPrevented: event.defaultPrevented });
            // jsdom has no navigation
            event.preventDefault();
        };
        document.addEventListener("click", record);

        try {
            // its renewal was not answered, so the link left is near its end
            const working = h.signAnswer.value;
            h.signAnswer.value = () => {
                throw new TypeError("Failed to fetch");
            };
            await h.advance(9 * 60 * 1000 + 5 * 1000);
            h.signAnswer.value = paths => working(paths).map(item => ({ ...item, signedURL: `${item.signedURL}-fresh` }));
            const signed = h.signs.length;

            wrapper.get("[data-testid='attachment-file']").element.dispatchEvent(new MouseEvent("click", { bubbles: true, cancelable: true }));
            await settle();
            await nextTick();
            await settle();

            expect(h.signs).toHaveLength(signed + 1);
            expect(clicks.map(c => c.isPrevented)).toEqual([true, false]);
            expect(clicks[1].href).toContain("-fresh&download=a.pdf");
        }
        finally {
            document.removeEventListener("click", record);
        }
    });

    test("a card double clicked as its link nears its end signs once and downloads once", async () => {
        const h = harness();
        const { wrapper } = await mountFiles([file(1, "a.pdf")], h);
        const card = wrapper.get("[data-testid='attachment-file']").element;
        const clicks = recordClicks(card);

        try {
            const working = h.signAnswer.value;
            h.signAnswer.value = () => {
                throw new TypeError("Failed to fetch");
            };
            await h.advance(9 * 60 * 1000 + 5 * 1000);
            h.signAnswer.value = paths => working(paths).map(item => ({ ...item, signedURL: `${item.signedURL}-fresh` }));
            const signed = h.signs.length;

            card.dispatchEvent(new MouseEvent("click", { bubbles: true, cancelable: true }));
            card.dispatchEvent(new MouseEvent("click", { bubbles: true, cancelable: true }));
            await settle();
            await nextTick();
            await settle();

            expect(h.signs).toHaveLength(signed + 1);
            // both of the person's clicks held back, then the one follow from the fresh link
            expect(clicks.map(c => c.isPrevented)).toEqual([true, true, false]);
            expect(clicks[2].href).toContain("-fresh&download=a.pdf");
        }
        finally {
            clicks.stop();
        }
    });

    test("a card clicked as its link nears its end, whose fresh signing fails, downloads once from the link it still has", async () => {
        const h = harness();
        const { wrapper } = await mountFiles([file(1, "a.pdf")], h);
        const card = wrapper.get("[data-testid='attachment-file']").element;
        const clicks = recordClicks(card);

        try {
            h.signAnswer.value = () => {
                throw new TypeError("Failed to fetch");
            };
            await h.advance(9 * 60 * 1000 + 5 * 1000);
            const signed = h.signs.length;

            card.dispatchEvent(new MouseEvent("click", { bubbles: true, cancelable: true }));
            await settle();
            await nextTick();
            await settle();

            expect(h.signs.length).toBeLessThanOrEqual(signed + 1);
            expect(clicks.map(c => c.isPrevented)).toEqual([true, false]);
            expect(clicks[1].href).toContain("token=t-0001&download=a.pdf");
        }
        finally {
            clicks.stop();
        }
    });

    test("a card clicked as its link nears its end, with no session to sign by, asks once and follows the link it still has once", async () => {
        const h = harness();
        const { wrapper } = await mountFiles([file(1, "a.pdf")], h);
        const card = wrapper.get("[data-testid='attachment-file']").element;
        const clicks = recordClicks(card);

        try {
            let asks = 0;
            h.dependencies.currentToken = () => {
                asks++;
                return null;
            };
            await h.advance(9 * 60 * 1000 + 5 * 1000);
            const asked = asks;

            card.dispatchEvent(new MouseEvent("click", { bubbles: true, cancelable: true }));
            await settle();
            await nextTick();
            await settle();

            expect(asks).toBeLessThanOrEqual(asked + 1);
            expect(clicks.map(c => c.isPrevented)).toEqual([true, false]);
        }
        finally {
            clicks.stop();
        }
    });

    test("a file card still being sent is plain text read by its visible name, with no label of its own", async () => {
        const { wrapper } = await mountFiles([file(1, "a.pdf")], harness(), { isPending: true });

        const card = wrapper.get("[data-testid='attachment-file']");
        expect(card.element.tagName).toBe("SPAN");
        expect(card.attributes("aria-label")).toBeUndefined();
        expect(card.attributes("rel")).toBeUndefined();
    });

    test("a file card whose link is refused is plain text, with no label and no link relation", async () => {
        const h = harness();
        h.signAnswer.value = paths => paths.map(path => ({ path, signedURL: null, error: "Either the object does not exist or you do not have access to it" }));
        const { wrapper } = await mountFiles([file(1, "a.pdf")], h);

        const card = wrapper.get("[data-testid='attachment-file']");
        expect(card.element.tagName).toBe("SPAN");
        expect(card.attributes("aria-label")).toBeUndefined();
        expect(card.attributes("rel")).toBeUndefined();
    });
});

describe("the viewer", () => {
    test("opens on the photo chosen, steps with the arrow keys, closes on Escape and gives focus back", async () => {
        const { wrapper } = await mountFiles([image(1), image(2), image(3)]);
        const tiles = wrapper.findAll("[data-testid='attachment-image']");
        (tiles[1].element as HTMLElement).focus();

        await tiles[1].trigger("click");
        await nextTick();
        await nextTick();

        const viewer = (): HTMLImageElement | null => document.querySelector("[data-testid='attachment-viewer-image']");
        expect(viewer()?.getAttribute("alt")).toBe("photo-2.jpg");
        expect(document.querySelector("[data-testid='dialog'] .chat-dialog-title")?.textContent).toBe("photo-2.jpg");
        expect(document.querySelector("[data-testid='attachment-viewer-count']")?.textContent).toBe("2 of 3");

        document.querySelector("[data-testid='dialog']")?.dispatchEvent(new KeyboardEvent("keydown", { key: "ArrowRight", bubbles: true }));
        await nextTick();
        expect(viewer()?.getAttribute("alt")).toBe("photo-3.jpg");

        // the last steps on to the first
        document.querySelector("[data-testid='dialog']")?.dispatchEvent(new KeyboardEvent("keydown", { key: "ArrowRight", bubbles: true }));
        await nextTick();
        expect(viewer()?.getAttribute("alt")).toBe("photo-1.jpg");

        document.querySelector("[data-testid='dialog']")?.dispatchEvent(new KeyboardEvent("keydown", { key: "ArrowLeft", bubbles: true }));
        await nextTick();
        expect(viewer()?.getAttribute("alt")).toBe("photo-3.jpg");

        document.querySelector("[data-testid='dialog']")?.dispatchEvent(new KeyboardEvent("keydown", { key: "Escape", bubbles: true }));
        await nextTick();
        await nextTick();
        expect(viewer()).toBeNull();
        expect(document.activeElement).toBe(tiles[1].element);
    });

    test("offers the photo's download under its name, and next and previous only with more than one", async () => {
        const { wrapper } = await mountFiles([image(1)]);

        await wrapper.get("[data-testid='attachment-image']").trigger("click");
        await nextTick();
        await nextTick();

        const download = document.querySelector<HTMLAnchorElement>("[data-testid='attachment-viewer-download']");
        expect(download?.getAttribute("href")).toBe(`${projectUrl}/storage/v1/object/sign/chat-attachments/${image(1).key}?token=t-0001&download=photo-1.jpg`);
        expect(download?.getAttribute("aria-label")).toBe("Download photo-1.jpg");
        expect(document.querySelector("[data-testid='attachment-viewer-next']")).toBeNull();
        expect(document.querySelector("[data-testid='attachment-viewer-prev']")).toBeNull();
    });

    test("a press on the scrim outside the photo closes it", async () => {
        const { wrapper } = await mountFiles([image(1), image(2)]);

        await wrapper.get("[data-testid='attachment-image']").trigger("click");
        await nextTick();
        await nextTick();
        const scrim = document.querySelector<HTMLElement>("[data-testid='dialog-scrim']");
        scrim?.dispatchEvent(new MouseEvent("mousedown", { bubbles: true }));
        scrim?.dispatchEvent(new MouseEvent("click", { bubbles: true }));
        await nextTick();
        await nextTick();

        expect(document.querySelector("[data-testid='attachment-viewer-image']")).toBeNull();
    });
});

/** The declarations a component's styles give a selector, comments left out. */
function ruleOf(file: string, selector: string): Record<string, string> {
    const source = readFileSync(join(__dirname, "../../../../../src/Communication/Chat/ChatShell", file), "utf8");
    const style = source.slice(source.indexOf("<style"), source.indexOf("</style>")).replace(/\/\*[\s\S]*?\*\//g, "");
    const declarations: Record<string, string> = {};
    for (const match of style.matchAll(/([^{}]+)\{([^{}]*)\}/g)) {
        if (match[1].split(",").some(part => part.trim() === selector)) {
            for (const declaration of match[2].split(";")) {
                const colon = declaration.indexOf(":");
                if (colon > 0) {
                    declarations[declaration.slice(0, colon).trim()] = declaration.slice(colon + 1).trim();
                }
            }
        }
    }
    return declarations;
}

// The viewer's body is the photo's frame. Its one cell has a definite size, so the photo's limits
// are read against it: a tall photo is drawn whole rather than running past the dialog's foot, a
// wide one whole across it, and a small one at its own size, never stretched.
describe("the viewer's photo", () => {
    test("is held whole within the dialog's body, centred, and never drawn past its own size", () => {
        const body = ruleOf("ui/dialog.partial.obs", ".chat-dialog.is-media .chat-dialog-scroll");
        expect(body["grid-template"]).toBe("minmax(0, 1fr) / minmax(0, 1fr)");
        expect(body["place-items"]).toBe("center");
        expect(body.overflow).toBe("hidden");

        const photo = ruleOf("components/attachments.partial.obs", ".chat-attach-viewer-image");
        expect(photo["max-width"]).toBe("100%");
        expect(photo["max-height"]).toBe("100%");
        expect(photo["object-fit"]).toBe("contain");
        expect(photo.width).toBeUndefined();
        expect(photo.height).toBeUndefined();
    });
});

// A failed file is a chip like the others: its name keeps the same room, and the line under it,
// where a file's size or progress stands, holds the failure and Try again on one line.
describe("the composer's strip", () => {
    function trayOf(files: Array<Partial<TrayFile>>): AttachmentTray {
        return reactive({
            files: files.map((file, index) => ({
                localId: `local-${index}`,
                name: `file-${index}.pdf`,
                kind: "file" as const,
                type: "application/pdf",
                size: 48_000,
                width: null,
                height: null,
                previewUrl: null,
                status: "uploaded" as const,
                progress: 1,
                failure: null,
                uploadId: null,
                key: null,
                ...file
            })),
            notice: null,
            add: jest.fn(),
            remove: jest.fn(),
            retry: jest.fn()
        });
    }

    test("a failed file keeps a chip's shape, its failure and Try again on the one line under its name", async () => {
        const tray = trayOf([
            { name: "Q4 budget.xlsx", status: "failed", progress: 0, failure: { alone: "Couldn't upload. Try again later.", besideRetry: "Couldn't upload.", canRetry: true } }
        ]);
        const { wrapper } = await mountFiles([], harness(), { tray });

        const chip = wrapper.get("[data-testid='attachment-tray-file']");
        const name = chip.get("[data-testid='attachment-tray-name']");
        const failure = chip.get("[data-testid='attachment-tray-failure']");
        const retry = chip.get("[data-testid='attachment-tray-retry']");
        expect(name.text()).toBe("Q4 budget.xlsx");
        expect(failure.text()).toBe("Couldn't upload.");

        // the line under the name, as a file's size stands, holds both
        const line = failure.element.parentElement;
        expect(retry.element.parentElement).toBe(line);
        expect(line?.parentElement).toBe(name.element.parentElement);
        expect(line?.previousElementSibling).toBe(name.element);

        // no wrapping into a taller chip; the words give way before Try again does
        expect(ruleOf("components/attachments.partial.obs", ".chat-attach-chip[data-status=\"failed\"]")["flex-wrap"]).toBeUndefined();
        expect(ruleOf("components/attachments.partial.obs", ".chat-attach-chip-line")["white-space"]).toBe("nowrap");
        const words = ruleOf("components/attachments.partial.obs", ".chat-attach-chip-failure");
        expect(words["text-overflow"]).toBe("ellipsis");
        expect(words["min-width"]).toBe("0");
        expect(ruleOf("components/attachments.partial.obs", ".chat-attach-chip-retry").flex).toBe("none");

        await retry.trigger("click");
        expect(tray.retry).toHaveBeenCalledWith("local-0");
    });

    test("a file refused for good says why alone on that line, with no Try again", async () => {
        const tray = trayOf([
            { name: "video.mov", status: "failed", progress: 0, failure: { alone: "Files can be up to 25 MB.", besideRetry: "Files can be up to 25 MB.", canRetry: false } }
        ]);
        const { wrapper } = await mountFiles([], harness(), { tray });

        const chip = wrapper.get("[data-testid='attachment-tray-file']");
        expect(chip.get("[data-testid='attachment-tray-failure']").text()).toBe("Files can be up to 25 MB.");
        expect(chip.get("[data-testid='attachment-tray-failure']").attributes("title")).toBe("Files can be up to 25 MB.");
        expect(chip.find("[data-testid='attachment-tray-retry']").exists()).toBe(false);
    });
});

// A row's column is sized to what it holds, in both looks, so a width in percent alone is read
// against nothing and the photos collapse until they load, then push the feed down under the
// reader. The grid and a file's card each ask for their own width, kept within the column.
describe("the room a message's files take", () => {
    test.each([".chat-attach-grid", ".chat-attach-file"])("%s asks for 320px of its own, never past its column", selector => {
        const declarations = ruleOf("components/attachments.partial.obs", selector);
        expect(declarations.width).toBe("320px");
        expect(declarations["max-width"]).toBe("100%");
    });
});

describe("a GIF in a message", () => {
    /** What the page reports for reduced motion, and the GIFs seen on screen, both driven by hand. */
    let isMotionReduced = false;
    let observed: Array<{ element: Element, report: (isOnScreen: boolean) => void }> = [];

    beforeEach(() => {
        isMotionReduced = false;
        observed = [];
        window.matchMedia = ((query: string) => ({ matches: query.includes("reduced-motion") && isMotionReduced, addEventListener: () => undefined, removeEventListener: () => undefined })) as unknown as typeof window.matchMedia;
        window.IntersectionObserver = class {
            private callback: IntersectionObserverCallback;
            constructor(callback: IntersectionObserverCallback) {
                this.callback = callback;
            }
            observe(element: Element): void {
                observed.push({ element, report: isOnScreen => this.callback([{ target: element, isIntersecting: isOnScreen } as unknown as IntersectionObserverEntry], this as unknown as IntersectionObserver) });
            }
            unobserve(): void {
                // nothing held
            }
            disconnect(): void {
                // nothing held
            }
        } as unknown as typeof IntersectionObserver;
        jest.spyOn(HTMLMediaElement.prototype, "play").mockImplementation(() => Promise.resolve());
        jest.spyOn(HTMLMediaElement.prototype, "pause").mockImplementation(() => undefined);
    });

    afterEach(() => {
        jest.restoreAllMocks();
    });

    test("draws in the single photo's box at its stored shape, within 320 by 360, with the GIF badge and its title for a reader", async () => {
        const { wrapper } = await mountFiles([gif(1, { width: 498, height: 280 })]);

        const box = wrapper.get("[data-testid='attachment-gif']");
        expect((box.element as HTMLElement).style.aspectRatio).toBe("498 / 280");
        expect(box.attributes("style")).toContain("width: 320px");
        expect(box.attributes("aria-label")).toBe("Thank you dance 1");
        expect(box.get("[data-testid='attachment-gif-badge']").text()).toBe("GIF");

        wrapper.unmount();
        const tall = await mountFiles([gif(2, { width: 360, height: 480 })]);
        // 360 high at 3:4 is 270 wide
        expect(tall.wrapper.get("[data-testid='attachment-gif']").attributes("style")).toContain("width: 270px");
    });

    test("a GIF with no title is still named for a reader", async () => {
        const { wrapper } = await mountFiles([gif(1, { title: "" })]);

        expect(wrapper.get("[data-testid='attachment-gif']").attributes("aria-label")).toBe("GIF");
    });

    test("plays KLIPY's mp4 as stored, muted and looping, with its still as the poster and no cross origin request", async () => {
        const item = gif(1);
        const { wrapper } = await mountFiles([item]);

        const video = wrapper.get("video");
        expect(video.attributes("src")).toBe(item.url);
        expect(video.attributes("poster")).toBe(item.still);
        expect((video.element as HTMLVideoElement).muted).toBe(true);
        expect(video.attributes("loop")).toBeDefined();
        expect(video.attributes("playsinline")).toBeDefined();
        expect(video.attributes("crossorigin")).toBeUndefined();
    });

    test("plays while on screen and pauses once it leaves", async () => {
        const play = HTMLMediaElement.prototype.play as jest.Mock;
        const pause = HTMLMediaElement.prototype.pause as jest.Mock;
        await mountFiles([gif(1)]);

        expect(play).not.toHaveBeenCalled();
        observed[0].report(true);
        expect(play).toHaveBeenCalledTimes(1);
        observed[0].report(false);
        expect(pause).toHaveBeenCalled();
    });

    test("with reduced motion it stays still, with a play button that plays it and a second press that pauses it", async () => {
        isMotionReduced = true;
        const play = HTMLMediaElement.prototype.play as jest.Mock;
        const pause = HTMLMediaElement.prototype.pause as jest.Mock;
        const { wrapper } = await mountFiles([gif(1)]);

        observed[0]?.report(true);
        expect(play).not.toHaveBeenCalled();

        const button = wrapper.get("[data-testid='attachment-gif-play']");
        expect(button.attributes("aria-label")).toBe("Play GIF");
        await button.trigger("click");
        expect(play).toHaveBeenCalledTimes(1);
        expect(wrapper.get("[data-testid='attachment-gif-play']").attributes("aria-label")).toBe("Pause GIF");

        await wrapper.get("[data-testid='attachment-gif-play']").trigger("click");
        expect(pause).toHaveBeenCalled();
    });

    test("a dead GIF is read as its words, not as a picture named for its title", async () => {
        const { wrapper } = await mountFiles([gif(1)]);

        await wrapper.get("video").trigger("error");

        const dead = wrapper.get("[data-testid='attachment-gif']");
        expect(dead.attributes("role")).toBeUndefined();
        expect(dead.attributes("aria-label")).toBeUndefined();
        expect(dead.text()).toContain("GIF unavailable");
    });

    test("a redraw of the message does not stop and start watching its GIFs", async () => {
        const watched = jest.spyOn(window.IntersectionObserver.prototype, "observe");
        const unwatched = jest.spyOn(window.IntersectionObserver.prototype, "unobserve");
        const { wrapper } = await mountFiles([gif(1), gif(2)]);
        expect(watched).toHaveBeenCalledTimes(2);

        await wrapper.setProps({ items: [gif(1), gif(2)] });
        await wrapper.setProps({ isPending: true });
        await wrapper.setProps({ isPending: false });

        expect(watched).toHaveBeenCalledTimes(2);
        expect(unwatched).not.toHaveBeenCalled();
    });

    test("a GIF's video asks for nothing until it plays, the poster holding its box", async () => {
        const { wrapper } = await mountFiles([gif(1)]);

        expect(wrapper.get("video").attributes("preload")).toBe("none");
    });

    test("a person who turns reduced motion on with the page open sees the GIF held still at once", async () => {
        const listeners: Array<(event: { matches: boolean }) => void> = [];
        window.matchMedia = ((query: string) => ({
            get matches() {
                return query.includes("reduced-motion") && isMotionReduced;
            },
            addEventListener: (_type: string, listener: (event: { matches: boolean }) => void) => listeners.push(listener),
            removeEventListener: () => undefined
        })) as unknown as typeof window.matchMedia;
        const { wrapper } = await mountFiles([gif(1)]);
        expect(wrapper.find("[data-testid='attachment-gif-play']").exists()).toBe(false);

        isMotionReduced = true;
        listeners.forEach(listener => listener({ matches: true }));
        await nextTick();

        expect(wrapper.find("[data-testid='attachment-gif-play']").exists()).toBe(true);
    });

    test("a GIF whose media is gone draws the unavailable tile at its stored size", async () => {
        const { wrapper } = await mountFiles([gif(1)]);

        await wrapper.get("video").trigger("error");

        const dead = wrapper.get("[data-testid='attachment-gif']");
        expect(dead.attributes("data-unavailable")).toBe("true");
        expect(dead.text()).toContain("GIF unavailable");
        expect((dead.element as HTMLElement).style.aspectRatio).toBe("498 / 280");
        expect(wrapper.find("video").exists()).toBe(false);
    });

    test("is drawn whatever the church's GIF switch says, and asks no signed link", async () => {
        // A GIF already sent stays in the conversation when the church turns GIFs off; nothing
        // here reads the switch, and KLIPY's media is never a key of ours to sign.
        const { wrapper, h } = await mountFiles([gif(1)]);

        expect(wrapper.find("[data-testid='attachment-gif']").exists()).toBe(true);
        expect(h.signs).toHaveLength(0);
    });

    test("a kind this client does not know is skipped, never drawn as a file card", async () => {
        const unknown = { ...file(3, "x.bin"), kind: "sticker" } as unknown as AttachmentItem;

        const { wrapper } = await mountFiles([unknown, file(4, "a.pdf")]);

        expect(wrapper.findAll("[data-testid='attachment-file']")).toHaveLength(1);
        expect(wrapper.get("[data-testid='attachment-file-name']").text()).toBe("a.pdf");
    });

    test("a GIF still being sent draws from its stored addresses at once", async () => {
        const item = gif(1);
        const { wrapper } = await mountFiles([item], harness(), { isPending: true });

        expect(wrapper.get("video").attributes("src")).toBe(item.url);
    });
});
