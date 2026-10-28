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
import { mount, VueWrapper } from "@vue/test-utils";
import { nextTick } from "vue";
import Attachments from "../../../../../src/Communication/Chat/ChatShell/components/attachments.partial.obs";
import { attachmentViewsKey, createAttachments } from "../../../../../src/Communication/Chat/ChatShell/composables/useAttachments.partial";
import { AttachmentItem } from "../../../../../src/Communication/Chat/ChatShell/types.partial";
import { harness, Harness, projectUrl, settle, tenant } from "../attachmentFakes";

function image(n: number, extra: Partial<AttachmentItem> = {}): AttachmentItem {
    const id = `d0000000-0000-4000-8000-${String(n).padStart(12, "0")}`;
    return { id, key: `${tenant}/${id}`, kind: "image", name: `photo-${n}.jpg`, size: 1000, type: "image/jpeg", width: 800, height: 600, ...extra };
}

function file(n: number, name: string, size = 1_400_000): AttachmentItem {
    const id = `d0000000-0000-4000-8000-${String(n).padStart(12, "0")}`;
    return { id, key: `${tenant}/${id}`, kind: "file", name, size, type: "application/pdf", width: null, height: null };
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
async function mountFiles(items: AttachmentItem[], h: Harness = harness()): Promise<{ wrapper: VueWrapper, h: Harness }> {
    shell = document.createElement("div");
    shell.className = "chat-shell";
    document.body.appendChild(shell);
    const attachments = createAttachments(h.dependencies);
    wrapper = mount(Attachments, {
        props: { items },
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
