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
// Attachments: a picked file is typed, checked against the platform's limits, a photo re-encoded,
// then begun and uploaded straight to Storage with its progress; a message's files are linked in
// batches, each link kept until shortly before it expires.
import { readFileSync } from "fs";
import { resolve } from "path";
import { attachmentPreviewWords, attachmentsPreview, canAttach, createAttachments, declaredType, formatFileSize, isTrayReady } from "../../../../../src/Communication/Chat/ChatShell/composables/useAttachments.partial";
import { AttachmentItem } from "../../../../../src/Communication/Chat/ChatShell/types.partial";
import { fakeCodec, harness, limits, picked, projectUrl, publishableKey, settle, tenant, token } from "../attachmentFakes";

const channel = "c0000001-0000-4000-8000-000000000000";

function image(id: string, extra: Partial<AttachmentItem> = {}): AttachmentItem {
    return { id, key: `${tenant}/${id}`, kind: "image", name: `${id}.jpg`, size: 1000, type: "image/jpeg", width: 400, height: 300, ...extra };
}

function file(id: string, name: string, extra: Partial<AttachmentItem> = {}): AttachmentItem {
    return { id, key: `${tenant}/${id}`, kind: "file", name, size: 2048, type: "application/pdf", width: null, height: null, ...extra };
}

describe("declaredType", () => {
    test("takes the type from the extension, whatever the browser reported", () => {
        // Windows with Excel reports a CSV as Excel's own type
        expect(declaredType("report.CSV", "application/vnd.ms-excel")).toBe("text/csv");
        // a HEIC photo often reports nothing
        expect(declaredType("IMG_0001.HEIC", "")).toBe("image/heic");
        expect(declaredType("notes.txt", "text/plain; charset=utf-8")).toBe("text/plain");
        expect(declaredType("deck.pptx", "")).toBe("application/vnd.openxmlformats-officedocument.presentationml.presentation");
        expect(declaredType("clip.MOV", "")).toBe("video/quicktime");
        expect(declaredType("song.m4a", "audio/x-m4a")).toBe("audio/mp4");
    });

    test("falls back to the browser's type, bare, only for a name with no extension it knows", () => {
        expect(declaredType("pasted", "image/png")).toBe("image/png");
        expect(declaredType("readme", "text/plain;charset=UTF-8")).toBe("text/plain");
        expect(declaredType("archive.zip", "application/zip")).toBe("application/zip");
        expect(declaredType("mystery", "")).toBe("");
    });
});

describe("attachment previews", () => {
    test("a message of files alone reads as its files, in the words the platform's previews use", () => {
        expect(attachmentsPreview([image("a")])).toBe("Photo");
        expect(attachmentsPreview([image("a"), image("b")])).toBe("2 photos");
        expect(attachmentsPreview([file("a", "Q3.pdf")])).toBe("File: Q3.pdf");
        expect(attachmentsPreview([image("a"), file("b", "Q3.pdf"), file("c", "Q4.pdf")])).toBe("3 files");
        expect(attachmentsPreview(null)).toBe("");
        expect(attachmentsPreview([])).toBe("");
    });

    test("the words are the wire contract's, so the platform and this client say the same", () => {
        const contractPath = resolve(__dirname, "../../../../../../Rock/Communication/Chat/Platform/Contract/chat-wire-contract.json");
        const contract = JSON.parse(readFileSync(contractPath, "utf8")) as { attachment_previews?: unknown };

        expect(contract.attachment_previews).toEqual(attachmentPreviewWords);
    });

    test("sizes read as a person writes them", () => {
        expect(formatFileSize(812)).toBe("812 bytes");
        expect(formatFileSize(2048)).toBe("2 KB");
        expect(formatFileSize(1_400_000)).toBe("1.3 MB");
        expect(formatFileSize(26214400)).toBe("25 MB");
    });
});

describe("a tray's files", () => {
    test("a picked file is begun with its declared type and size, then uploaded to its key with the person's token and the bare type", async () => {
        const h = harness();
        const attachments = createAttachments(h.dependencies);
        const tray = attachments.trayFor(channel, channel);

        tray.add([picked("notes.txt", "text/plain; charset=utf-8", 1200)]);
        await settle();

        expect(h.begins).toEqual([{
            p_channel_id: channel,
            p_files: [{ name: "notes.txt", type: "text/plain", size: 1200, width: null, height: null }]
        }]);
        const [request] = h.requests;
        const key = request.url.slice(`${projectUrl}/storage/v1/object/chat-attachments/`.length);
        expect(request.method).toBe("POST");
        expect(request.url).toBe(`${projectUrl}/storage/v1/object/chat-attachments/${key}`);
        expect(key).toMatch(new RegExp(`^${tenant}/[0-9a-f-]{36}$`));
        expect(request.headers).toEqual({
            "apikey": publishableKey,
            "authorization": `Bearer ${token}`,
            "x-upsert": "false",
            "content-type": "text/plain",
            "cache-control": "max-age=3600"
        });
        expect(request.body).toBeInstanceOf(Blob);
        expect(tray.files.map(f => [f.name, f.status])).toEqual([["notes.txt", "uploading"]]);
    });

    test("an upload reports its progress, and once Storage takes it the tray is ready and hands it over in the order picked", async () => {
        const h = harness();
        const attachments = createAttachments(h.dependencies);
        const tray = attachments.trayFor(channel, channel);

        tray.add([picked("a.pdf", "application/pdf"), picked("b.pdf", "application/pdf")]);
        await settle();
        expect(isTrayReady(tray)).toBe(false);

        h.requests[0].progress(250, 1000);
        expect(tray.files[0].progress).toBe(0.25);

        h.requests[1].respond(200);
        h.requests[0].respond(200);
        await settle();

        expect(tray.files.map(f => f.status)).toEqual(["uploaded", "uploaded"]);
        expect(isTrayReady(tray)).toBe(true);

        const taken = attachments.take(channel);
        expect(taken?.map(item => [item.name, item.kind, item.type])).toEqual([["a.pdf", "file", "application/pdf"], ["b.pdf", "file", "application/pdf"]]);
        expect(taken?.map(item => item.key)).toEqual(h.begins.map(() => expect.any(String)));
        expect(tray.files).toEqual([]);
        expect(attachments.take(channel)).toBeNull();
    });

    test("a type the platform does not list, a file over the size, and files past the count are refused before anything is begun", async () => {
        const h = harness();
        const attachments = createAttachments(h.dependencies);
        const tray = attachments.trayFor(channel, channel);
        const big = picked("big.pdf", "application/pdf");
        Object.defineProperty(big, "size", { value: limits.attachment_max_bytes + 1 });

        tray.add([picked("site.zip", "application/zip"), picked("page.svg", "image/svg+xml"), big]);
        await settle();

        expect(h.begins).toEqual([]);
        expect(tray.files.map(f => [f.name, f.status, f.failure?.alone, f.failure?.canRetry])).toEqual([
            ["site.zip", "failed", "This type of file can't be sent.", false],
            ["page.svg", "failed", "This type of file can't be sent.", false],
            ["big.pdf", "failed", "Files can be up to 25 MB.", false]
        ]);
        expect(isTrayReady(tray)).toBe(false);

        tray.remove(tray.files[0].localId);
        tray.remove(tray.files[0].localId);
        tray.remove(tray.files[0].localId);
        tray.add(Array.from({ length: 11 }, (_, i) => picked(`f${i}.pdf`, "application/pdf")));
        await settle();

        expect(tray.files).toHaveLength(10);
        expect(tray.notice).toBe("A message can carry up to 10 files.");
    });

    test("a begin the platform refuses fails that file with the server's sentence, and Try again begins it again", async () => {
        const h = harness();
        h.beginAnswers.push({ ok: false, status: 429, error: { code: "rpc.rate_limited", severity: "failed", text: "You're attaching files too quickly. Try again in a minute." }, retryAfterSeconds: null });
        const attachments = createAttachments(h.dependencies);
        const tray = attachments.trayFor(channel, channel);

        tray.add([picked("a.pdf", "application/pdf")]);
        await settle();

        expect(tray.files[0].status).toBe("failed");
        expect(tray.files[0].failure).toEqual({ alone: "You're attaching files too quickly. Try again in a minute.", besideRetry: "You're sending messages too quickly.", canRetry: true });
        expect(h.requests).toEqual([]);

        tray.retry(tray.files[0].localId);
        await settle();
        expect(h.begins).toHaveLength(2);
        h.requests[0].respond(200);
        await settle();
        expect(tray.files[0].status).toBe("uploaded");
    });

    test("an upload that fails fails its file alone, and Try again uploads it again to the same key", async () => {
        const h = harness();
        const attachments = createAttachments(h.dependencies);
        const tray = attachments.trayFor(channel, channel);

        tray.add([picked("a.pdf", "application/pdf"), picked("b.pdf", "application/pdf")]);
        await settle();
        h.requests[0].fail();
        h.requests[1].respond(200);
        await settle();

        expect(tray.files.map(f => f.status)).toEqual(["failed", "uploaded"]);
        expect(tray.files[0].failure).toEqual({ alone: "Couldn't upload. Try again in a moment.", besideRetry: "Couldn't upload.", canRetry: true });

        tray.retry(tray.files[0].localId);
        await settle();
        expect(h.begins).toHaveLength(2);
        expect(h.requests[2].url).toBe(h.requests[0].url);
        h.requests[2].respond(200);
        await settle();
        expect(isTrayReady(tray)).toBe(true);
    });

    test("an empty file is refused with its own sentence before anything is begun", async () => {
        const h = harness();
        const attachments = createAttachments(h.dependencies);
        const tray = attachments.trayFor(channel, channel);

        tray.add([picked("empty.txt", "text/plain", 0)]);
        await settle();

        expect(h.begins).toEqual([]);
        expect(tray.files.map(f => [f.status, f.failure?.alone, f.failure?.canRetry])).toEqual([["failed", "This file is empty.", false]]);
    });

    test("Try again after an upload whose answer was lost takes Storage's already-stored answer for its own key as uploaded", async () => {
        const h = harness();
        const attachments = createAttachments(h.dependencies);
        const tray = attachments.trayFor(channel, channel);

        tray.add([picked("a.pdf", "application/pdf")]);
        await settle();
        // the bytes landed, but the answer never came back
        h.requests[0].fail();
        await settle();
        expect(tray.files[0].status).toBe("failed");

        tray.retry(tray.files[0].localId);
        await settle();
        expect(h.requests[1].url).toBe(h.requests[0].url);
        // Storage's answer to a second upload to a stored key, as its upload route writes it
        h.requests[1].respond(400, { statusCode: "409", error: "Duplicate", message: "The resource already exists", code: "KeyAlreadyExists" });
        await settle();

        expect(tray.files[0].status).toBe("uploaded");
        expect(isTrayReady(tray)).toBe(true);
    });

    test("removing a file stops its upload", async () => {
        const h = harness();
        const attachments = createAttachments(h.dependencies);
        const tray = attachments.trayFor(channel, channel);

        tray.add([picked("a.pdf", "application/pdf")]);
        await settle();
        tray.remove(tray.files[0].localId);

        expect(h.requests[0].isAborted).toBe(true);
    });

    test("each place keeps its own tray, so files picked in one room never go with a message in another", async () => {
        const h = harness();
        const attachments = createAttachments(h.dependencies);
        const other = "c0000002-0000-4000-8000-000000000000";

        attachments.trayFor(channel, channel).add([picked("a.pdf", "application/pdf")]);
        await settle();

        expect(attachments.trayFor(other, other).files).toEqual([]);
        expect(attachments.trayFor(channel, channel).files).toHaveLength(1);
        expect(attachments.trayFor(`${channel}:42`, channel).files).toEqual([]);
    });

    test("removing a file takes it out of the tray and lets its preview go", async () => {
        const sizes = new Map<Blob, { width: number, height: number }>();
        const h = harness(fakeCodec(sizes));
        const attachments = createAttachments(h.dependencies);
        const tray = attachments.trayFor(channel, channel);
        const photo = picked("p.jpg", "image/jpeg");
        sizes.set(photo, { width: 400, height: 300 });

        tray.add([photo]);
        await settle();
        const preview = tray.files[0].previewUrl;
        expect(preview).toMatch(/^blob:/);

        tray.remove(tray.files[0].localId);
        expect(tray.files).toEqual([]);
        expect(h.revoked).toEqual([preview]);
    });
});

describe("photos", () => {
    test("a JPEG is always re-encoded, even under the longest side, which strips its location", async () => {
        const codec = fakeCodec();
        const h = harness(codec);
        const photo = picked("small.jpg", "image/jpeg");
        codec.sizes.set(photo, { width: 400, height: 300 });
        const attachments = createAttachments(h.dependencies);

        attachments.trayFor(channel, channel).add([photo]);
        await settle();

        expect(codec.encoded.map(e => [e.width, e.height, e.type, e.quality])).toEqual([[400, 300, "image/jpeg", 0.82]]);
        expect(h.begins[0].p_files).toEqual([{ name: "small.jpg", type: "image/jpeg", size: "re-encoded".length, width: 400, height: 300 }]);
        expect(h.requests[0].body).not.toBe(photo);
        expect(h.requests[0].headers["content-type"]).toBe("image/jpeg");
    });

    test("a WebP becomes a JPEG and a PNG stays a PNG, each brought within 2048 on its longest side", async () => {
        const codec = fakeCodec();
        const h = harness(codec);
        const webp = picked("wide.webp", "image/webp");
        const png = picked("tall.png", "image/png");
        codec.sizes.set(webp, { width: 4096, height: 1024 });
        codec.sizes.set(png, { width: 1000, height: 3000 });
        const attachments = createAttachments(h.dependencies);

        attachments.trayFor(channel, channel).add([webp, png]);
        await settle();

        expect(codec.encoded.map(e => [e.width, e.height, e.type, e.quality])).toEqual([
            [2048, 512, "image/jpeg", 0.82],
            [683, 2048, "image/png", undefined]
        ]);
        expect(h.begins.map(b => (b.p_files as Array<Record<string, unknown>>)[0])).toEqual([
            { name: "wide.jpg", type: "image/jpeg", size: "re-encoded".length, width: 2048, height: 512 },
            { name: "tall.png", type: "image/png", size: "re-encoded".length, width: 683, height: 2048 }
        ]);
    });

    test("a GIF goes as it is, with its size read for the space it holds", async () => {
        const codec = fakeCodec();
        const h = harness(codec);
        const gif = picked("party.gif", "image/gif", 5000);
        codec.sizes.set(gif, { width: 320, height: 240 });
        const attachments = createAttachments(h.dependencies);

        attachments.trayFor(channel, channel).add([gif]);
        await settle();

        expect(codec.encoded).toEqual([]);
        expect(h.begins[0].p_files).toEqual([{ name: "party.gif", type: "image/gif", size: 5000, width: 320, height: 240 }]);
        expect(h.requests[0].body).toBe(gif);
    });

    test("a HEIC photo this browser can decode goes as a JPEG; one it cannot is refused, never sent raw", async () => {
        const codec = fakeCodec();
        const h = harness(codec);
        const readable = picked("IMG_1.HEIC", "");
        const unreadable = picked("IMG_2.heic", "image/heic");
        codec.sizes.set(readable, { width: 3024, height: 4032 });
        const attachments = createAttachments(h.dependencies);
        const tray = attachments.trayFor(channel, channel);

        tray.add([readable, unreadable]);
        await settle();

        expect(codec.encoded.map(e => [e.width, e.height, e.type])).toEqual([[1536, 2048, "image/jpeg"]]);
        expect(h.begins.map(b => (b.p_files as Array<Record<string, unknown>>)[0].name)).toEqual(["IMG_1.jpg"]);
        expect(tray.files.map(f => [f.name, f.status, f.failure?.alone ?? null])).toEqual([
            ["IMG_1.jpg", "uploading", null],
            ["IMG_2.heic", "failed", "This photo can't be read in this browser. Save it as a JPEG and attach it again."]
        ]);
    });
});

describe("download links", () => {
    test("links asked for together are signed in one call, for ten minutes, and read as full addresses", async () => {
        const h = harness();
        const attachments = createAttachments(h.dependencies);
        const photo = image("d0000000-0000-4000-8000-0000000000a1");
        const pdf = file("d0000000-0000-4000-8000-0000000000a2", "Q3 report #1 & notes.pdf");

        attachments.links.subscribe([photo]);
        attachments.links.subscribe([pdf]);
        await settle();

        expect(h.signs).toHaveLength(1);
        expect(h.signs[0].url).toBe(`${projectUrl}/storage/v1/object/sign/chat-attachments`);
        expect(h.signs[0].init.method).toBe("POST");
        expect((h.signs[0].init.headers as Record<string, string>)["Authorization"]).toBe(`Bearer ${token}`);
        expect((h.signs[0].init.headers as Record<string, string>)["apikey"]).toBe(publishableKey);
        expect(h.signs[0].body).toEqual({ expiresIn: 600, paths: [photo.key, pdf.key] });

        const photoLink = attachments.links.linkOf(photo);
        expect(photoLink?.url).toBe(`${projectUrl}/storage/v1/object/sign/chat-attachments/${photo.key}?token=t-00a1`);
        expect(photoLink?.downloadUrl).toBe(`${photoLink?.url}&download=${encodeURIComponent(photo.name)}`);
        // a file card always downloads, under its own name
        expect(attachments.links.linkOf(pdf)?.downloadUrl).toBe(`${projectUrl}/storage/v1/object/sign/chat-attachments/${pdf.key}?token=t-00a2&download=Q3%20report%20%231%20%26%20notes.pdf`);
    });

    test("a link is kept by id, drawn until it ends, and asked again for a row only once it is near its end", async () => {
        const h = harness();
        const attachments = createAttachments(h.dependencies);
        const photo = image("d0000000-0000-4000-8000-0000000000b1");

        attachments.links.subscribe([photo])();
        await settle();
        attachments.links.subscribe([photo])();
        await settle();
        expect(h.signs).toHaveLength(1);

        // eight minutes into its life, the link held is still good for a row
        h.now.value += 8 * 60 * 1000;
        attachments.links.subscribe([photo])();
        await settle();
        expect(h.signs).toHaveLength(1);

        // released, so no renewal runs; the link still works, and is drawn, until it ends
        h.now.value += 60 * 1000 + 30 * 1000;
        expect(attachments.links.linkOf(photo)?.url).toContain(photo.key);
        attachments.links.subscribe([photo]);
        await settle();
        expect(h.signs).toHaveLength(2);
        expect(attachments.links.linkOf(photo)?.url).toContain(photo.key);
    });

    test("a row's links are signed again about a minute before they end, fifty to a call, and stop once released", async () => {
        const h = harness();
        const attachments = createAttachments(h.dependencies);
        const items = Array.from({ length: 60 }, (_, i) => image(`d0000000-0000-4000-8000-${String(i + 500).padStart(12, "0")}`));

        const release = attachments.links.subscribe(items);
        await settle();
        expect(h.signs.map(s => s.body.paths.length)).toEqual([50, 10]);

        await h.advance(8 * 60 * 1000);
        expect(h.signs).toHaveLength(2);
        await h.advance(70 * 1000);
        expect(h.signs.map(s => s.body.paths.length)).toEqual([50, 10, 50, 10]);
        expect(items.every(item => attachments.links.linkOf(item)?.url)).toBe(true);

        release();
        await h.advance(30 * 60 * 1000);
        expect(h.signs).toHaveLength(4);
        expect(h.timers).toEqual([]);
        // past its end, nothing is drawn from it
        expect(attachments.links.linkOf(items[0])).toBeNull();
    });

    test("a renewal that fails is tried again, and the link is drawn until it ends", async () => {
        const h = harness();
        const attachments = createAttachments(h.dependencies);
        const photo = image("d0000000-0000-4000-8000-0000000000b2");
        attachments.links.subscribe([photo]);
        await settle();

        const working = h.signAnswer.value;
        h.signAnswer.value = () => {
            throw new TypeError("Failed to fetch");
        };
        await h.advance(9 * 60 * 1000 + 1000);
        expect(h.signs).toHaveLength(2);
        expect(attachments.links.linkOf(photo)?.url).toContain(photo.key);

        h.signAnswer.value = working;
        await h.advance(30 * 1000);
        expect(h.signs.length).toBeGreaterThan(2);
        await h.advance(5 * 60 * 1000);
        expect(attachments.links.linkOf(photo)?.url).toContain(photo.key);
    });

    test("a link at its end is signed afresh for a click, which waits on it", async () => {
        const h = harness();
        const attachments = createAttachments(h.dependencies);
        const pdf = file("d0000000-0000-4000-8000-0000000000b3", "a.pdf");
        attachments.links.subscribe([pdf])();
        await settle();

        expect((await attachments.links.fresh(pdf))?.downloadUrl).toContain("download=a.pdf");
        expect(h.signs).toHaveLength(1);

        h.now.value += 9 * 60 * 1000 + 30 * 1000;
        h.signAnswer.value = paths => paths.map(path => ({ path, signedURL: `/object/sign/chat-attachments/${path}?token=again`, error: null }));
        const link = await attachments.links.fresh(pdf);
        expect(h.signs).toHaveLength(2);
        expect(link?.downloadUrl).toContain("token=again");
    });

    test("more than fifty at once go in calls of fifty", async () => {
        const h = harness();
        const attachments = createAttachments(h.dependencies);
        const items = Array.from({ length: 60 }, (_, i) => image(`d0000000-0000-4000-8000-${String(i).padStart(12, "0")}`));

        attachments.links.subscribe(items);
        await settle();

        expect(h.signs.map(s => s.body.paths.length)).toEqual([50, 10]);
        expect(items.every(item => attachments.links.linkOf(item)?.url)).toBe(true);
    });

    test("a key refused at signing is unavailable, never a failed batch, and the others still link", async () => {
        const h = harness();
        const refused = image("d0000000-0000-4000-8000-0000000000c1");
        const fine = image("d0000000-0000-4000-8000-0000000000c2");
        h.signAnswer.value = paths => paths.map(path => path === refused.key
            ? { path, signedURL: null, error: "Either the object does not exist or you do not have access to it" }
            : { path, signedURL: `/object/sign/chat-attachments/${path}?token=ok`, error: null });
        const attachments = createAttachments(h.dependencies);

        attachments.links.subscribe([refused, fine]);
        await settle();

        expect(attachments.links.linkOf(refused)).toEqual({ url: null, downloadUrl: null, isUnavailable: true });
        expect(attachments.links.linkOf(fine)?.isUnavailable).toBe(false);
    });

    test("the person's own photo shows from the copy they picked, before and after the send, with no link asked", async () => {
        const codec = fakeCodec();
        const h = harness(codec);
        const photo = picked("p.jpg", "image/jpeg");
        codec.sizes.set(photo, { width: 400, height: 300 });
        const attachments = createAttachments(h.dependencies);
        const tray = attachments.trayFor(channel, channel);

        tray.add([photo]);
        await settle();
        h.requests[0].respond(200);
        await settle();
        const preview = tray.files[0].previewUrl;
        const [item] = attachments.take(channel) ?? [];

        expect(attachments.previewOf(item.id)).toBe(preview);
    });
});

describe("uploading again", () => {
    test("files whose uploads are gone are begun and uploaded again from the copies kept, under new ids", async () => {
        const h = harness();
        const attachments = createAttachments(h.dependencies);
        const tray = attachments.trayFor(channel, channel);
        tray.add([picked("a.pdf", "application/pdf")]);
        await settle();
        h.requests[0].respond(200);
        await settle();
        const items = attachments.take(channel) ?? [];

        const again = attachments.reupload(channel, items);
        await settle();
        expect(h.begins).toHaveLength(2);
        expect(h.begins[1]).toEqual(h.begins[0]);
        h.requests[1].respond(200);

        const renewed = await again;
        expect(renewed).toHaveLength(1);
        expect(renewed?.[0].id).not.toBe(items[0].id);
        expect(renewed?.[0]).toEqual({ ...items[0], id: renewed?.[0].id, key: renewed?.[0].key });
    });

    test("an upload that cannot be made again answers nothing, so the send keeps its failure", async () => {
        const h = harness();
        const attachments = createAttachments(h.dependencies);
        const tray = attachments.trayFor(channel, channel);
        tray.add([picked("a.pdf", "application/pdf")]);
        await settle();
        h.requests[0].respond(200);
        await settle();
        const items = attachments.take(channel) ?? [];

        const again = attachments.reupload(channel, items);
        await settle();
        h.requests[1].respond(403);

        expect(await again).toBeNull();
    });
});

describe("letting go", () => {
    test("a confirmed send's kept bytes are let go, and a photo's local copy once its link draws it", async () => {
        const codec = fakeCodec();
        const h = harness(codec);
        const photo = picked("p.jpg", "image/jpeg");
        codec.sizes.set(photo, { width: 400, height: 300 });
        const attachments = createAttachments(h.dependencies);
        attachments.trayFor(channel, channel).add([photo]);
        await settle();
        h.requests[0].respond(200);
        await settle();
        const items = attachments.take(channel) ?? [];
        const preview = attachments.previewOf(items[0].id);
        expect(preview).toMatch(/^blob:/);

        attachments.release(items);
        // nothing is left to upload again from
        expect(await attachments.reupload(channel, items)).toBeNull();
        expect(h.begins).toHaveLength(1);
        // the photo still draws from its local copy until its link can
        expect(attachments.previewOf(items[0].id)).toBe(preview);

        attachments.links.subscribe(items);
        await settle();
        expect(attachments.previewOf(items[0].id)).toBeNull();
        expect(h.revoked).toEqual([preview]);
    });

    test("stop ends every upload and renewal, lets every local copy go, and nothing is written after", async () => {
        const codec = fakeCodec();
        const h = harness(codec);
        const photo = picked("p.jpg", "image/jpeg");
        codec.sizes.set(photo, { width: 400, height: 300 });
        const attachments = createAttachments(h.dependencies);
        const tray = attachments.trayFor(channel, channel);
        tray.add([photo, picked("a.pdf", "application/pdf")]);
        await settle();
        const preview = tray.files[0].previewUrl;
        const linked = image("d0000000-0000-4000-8000-0000000000e1");
        attachments.links.subscribe([linked]);
        await settle();
        const unsigned = image("d0000000-0000-4000-8000-0000000000e2");
        attachments.links.subscribe([unsigned]);

        attachments.stop();
        await settle();

        expect(h.requests.map(r => r.isAborted)).toEqual([true, true]);
        expect(tray.files.map(f => [f.status, f.failure])).toEqual([["uploading", null], ["uploading", null]]);
        expect(h.revoked).toEqual([preview]);
        expect(attachments.links.linkOf(unsigned)).toBeNull();
        expect(h.timers).toEqual([]);

        // a late answer changes nothing
        h.requests[1].respond(200);
        await h.advance(20 * 60 * 1000);
        expect(tray.files.map(f => f.status)).toEqual(["uploading", "uploading"]);
        expect(h.signs).toHaveLength(1);
    });
});

describe("canAttach", () => {
    test("files are offered only once the platform has answered its limits for them", () => {
        expect(canAttach(limits)).toBe(true);
        expect(canAttach({ ...limits, attachment_types: [] })).toBe(false);
        expect(canAttach({ ...limits, attachment_max_files: 0 })).toBe(false);
        expect(canAttach({ ...limits, attachment_max_bytes: 0 })).toBe(false);
    });
});
