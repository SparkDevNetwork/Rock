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
// A message's files. Each file goes to the platform's storage before the message is sent: the
// platform first records the upload for the person and the room (begin), then the browser sends
// the bytes straight to Storage under the key begin answered, so no file passes through any
// server of ours. The send names the uploads, and the platform checks each against what begin
// recorded. Files wait in a tray per place a composer types into, uploading as they are picked,
// so the send itself never waits on bytes. Photos are always drawn again before they go, which
// is what strips their location, since a photo straight from a phone carries it.
//
// Reading a file needs a link Storage signs for the person, which it does only while they can
// read the message; links last ten minutes, so they are kept by file and asked again shortly
// before they expire, every file on screen asked in one call.
//
// This lives apart from the sender because its lifetime is not a send's: a tray outlives the
// sends that take from it, and links outlive both.
import { InjectionKey, markRaw, reactive } from "vue";
import { failedWords, failureWords, finalWords } from "../errors.partial";
import { CallResult } from "../platformCall.partial";
import { AttachmentItem, FailureWords } from "../types.partial";
import { AttachmentTypes, PlatformLimits } from "./useSession.partial";

/* eslint-disable @typescript-eslint/naming-convention */
/** The platform's limits on files, as the token exchange answers them. */
export type AttachmentLimits = Pick<PlatformLimits, "attachment_max_bytes" | "attachment_max_files"> & AttachmentTypes;
/* eslint-enable @typescript-eslint/naming-convention */

/**
 * How a message of files alone reads where only text is drawn: a quote, a list. The platform's
 * previews say the same, and the wire contract carries these words so the two cannot drift;
 * {count} and {name} are filled in.
 */
export const attachmentPreviewWords = {
    photo: "Photo",
    photos: "{count} photos",
    file: "File: {name}",
    files: "{count} files"
};

/** The bucket every chat file lives in. */
const bucket = "chat-attachments";

/** How long a signed link lasts, in seconds, and how long before its end it is asked again. */
const linkSeconds = 600;
const linkRenewMs = 60 * 1000;

/** The most keys Storage signs in one call. */
const signBatch = 50;

/** A photo's longest side once drawn again, and the JPEG quality it is drawn at. */
const maxPhotoSide = 2048;
const photoQuality = 0.82;

/**
 * Begin's record of an upload lasts two hours; a retry after most of that begins again rather
 * than sending to a key the platform may no longer take.
 */
const beginLifetimeMs = 110 * 60 * 1000;

/**
 * The type each extension declares. The browser's own type is not trusted for these: Windows
 * with Excel reports a CSV as Excel's, and a phone's HEIC photo often reports nothing.
 */
const extensionTypes: Record<string, string> = {
    jpg: "image/jpeg",
    jpeg: "image/jpeg",
    png: "image/png",
    gif: "image/gif",
    webp: "image/webp",
    heic: "image/heic",
    heif: "image/heic",
    pdf: "application/pdf",
    txt: "text/plain",
    csv: "text/csv",
    docx: "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
    xlsx: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
    pptx: "application/vnd.openxmlformats-officedocument.presentationml.presentation",
    mp4: "video/mp4",
    mov: "video/quicktime",
    mp3: "audio/mpeg",
    m4a: "audio/mp4"
};

/** The photo types drawn again before they go, and what each becomes. A GIF goes as it is. */
const redrawnTypes: Record<string, { type: string, extension: string }> = {
    "image/jpeg": { type: "image/jpeg", extension: "jpg" },
    "image/webp": { type: "image/jpeg", extension: "jpg" },
    "image/heic": { type: "image/jpeg", extension: "jpg" },
    "image/png": { type: "image/png", extension: "png" }
};

const typeRefused = "This type of file can't be sent.";
const photoUnreadable = "This photo can't be read in this browser. Save it as a JPEG and attach it again.";

// #region Words

/**
 * The words a message of files alone reads as.
 *
 * @param items The message's files, or nothing.
 *
 * @returns The words, or an empty string for no files.
 */
export function attachmentsPreview(items: AttachmentItem[] | null | undefined): string {
    if (!items || items.length === 0) {
        return "";
    }

    if (items.every(item => item.kind === "image")) {
        return items.length === 1 ? attachmentPreviewWords.photo : attachmentPreviewWords.photos.replace("{count}", String(items.length));
    }

    return items.length === 1
        ? attachmentPreviewWords.file.replace("{name}", items[0].name)
        : attachmentPreviewWords.files.replace("{count}", String(items.length));
}

/**
 * A file's size as a person writes it.
 *
 * @param bytes The size in bytes.
 *
 * @returns "812 bytes", "2 KB" or "1.3 MB".
 */
export function formatFileSize(bytes: number): string {
    if (bytes < 1024) {
        return `${bytes} bytes`;
    }

    if (bytes < 1024 * 1024) {
        return `${Math.round(bytes / 1024)} KB`;
    }

    const megabytes = bytes / (1024 * 1024);
    return `${megabytes >= 10 ? Math.round(megabytes) : Number(megabytes.toFixed(1))} MB`;
}

// #endregion Words

// #region Types and photos

/**
 * The type a file is declared as: its extension's, else the type the browser reported, bare. A
 * type with parameters would be stored with them and refused at the send, so none goes.
 *
 * @param name The file's name.
 * @param reported The type the browser reported, which may be empty.
 *
 * @returns The bare type, or an empty string where nothing says.
 */
export function declaredType(name: string, reported: string): string {
    const dot = name.lastIndexOf(".");
    const extension = dot > 0 ? name.slice(dot + 1).toLowerCase() : "";

    return extensionTypes[extension] ?? reported.split(";")[0].trim().toLowerCase();
}

/** A decoded image: its size after its orientation, and what a canvas draws it from. */
export type DecodedImage = { width: number, height: number, source: CanvasImageSource, close?: () => void };

/** How photos are read and drawn again; the browser's own unless a test stands in. */
export type ImageCodec = {
    /** Decodes a photo upright, or rejects where this browser cannot read it. */
    decode: (blob: Blob) => Promise<DecodedImage>;

    /** Draws a decoded photo at a size into a new file of a type. */
    encode: (image: DecodedImage, width: number, height: number, type: string, quality: number | undefined) => Promise<Blob>;
};

/** The browser's decoder and canvas. Drawing to a canvas carries none of the photo's metadata. */
export const browserImageCodec: ImageCodec = {
    decode: async (blob: Blob): Promise<DecodedImage> => {
        const bitmap = await createImageBitmap(blob, { imageOrientation: "from-image" });
        return { width: bitmap.width, height: bitmap.height, source: bitmap, close: () => bitmap.close() };
    },

    encode: (image, width, height, type, quality) => new Promise((resolve, reject) => {
        const canvas = document.createElement("canvas");
        canvas.width = width;
        canvas.height = height;
        const context = canvas.getContext("2d");
        if (!context) {
            reject(new Error("no canvas"));
            return;
        }

        // A JPEG has no transparency, and a transparent WebP would otherwise turn black.
        if (type === "image/jpeg") {
            context.fillStyle = "white";
            context.fillRect(0, 0, width, height);
        }
        context.drawImage(image.source, 0, 0, width, height);
        canvas.toBlob(blob => blob ? resolve(blob) : reject(new Error("not encoded")), type, quality);
    })
};

/** A file ready to begin: what it is declared as and the bytes that go. */
type PreparedFile = {
    blob: Blob;
    name: string;
    type: string;
    kind: AttachmentItem["kind"];
    width: number | null;
    height: number | null;
};

/**
 * Readies a picked file: typed, a photo drawn again within the longest side, a GIF measured.
 *
 * @returns The file, or the words it is refused with.
 */
async function prepare(file: File, limits: AttachmentLimits, codec: ImageCodec): Promise<PreparedFile | FailureWords> {
    const type = declaredType(file.name, file.type);
    const redrawn = redrawnTypes[type];
    if (!redrawn && !limits.attachment_types.includes(type)) {
        return finalWords(typeRefused);
    }

    if (!type.startsWith("image/")) {
        return { blob: file, name: file.name, type, kind: "file", width: null, height: null };
    }

    let image: DecodedImage;
    try {
        image = await codec.decode(file);
    }
    catch {
        return finalWords(photoUnreadable);
    }

    try {
        if (!redrawn) {
            return { blob: file, name: file.name, type, kind: "image", width: image.width, height: image.height };
        }

        if (!limits.attachment_types.includes(redrawn.type)) {
            return finalWords(typeRefused);
        }

        const scale = Math.min(1, maxPhotoSide / Math.max(image.width, image.height));
        const width = Math.max(1, Math.round(image.width * scale));
        const height = Math.max(1, Math.round(image.height * scale));
        const blob = await codec.encode(image, width, height, redrawn.type, redrawn.type === "image/jpeg" ? photoQuality : undefined);
        const dot = file.name.lastIndexOf(".");
        const name = `${dot > 0 ? file.name.slice(0, dot) : file.name}.${redrawn.extension}`;

        return { blob, name, type: redrawn.type, kind: "image", width, height };
    }
    catch {
        return finalWords(photoUnreadable);
    }
    finally {
        image.close?.();
    }
}

// #endregion Types and photos

// #region Trays

/** One file in a tray, as the strip over the composer draws it. */
export type TrayFile = {
    /** This page's own id for the file, until begin gives it an upload. */
    localId: string;
    name: string;
    kind: AttachmentItem["kind"];
    type: string;
    size: number;
    width: number | null;
    height: number | null;

    /** A photo's local copy, drawn before anything is uploaded; null for a file. */
    previewUrl: string | null;
    status: "preparing" | "uploading" | "uploaded" | "failed";

    /** How much of the file Storage has taken, from 0 to 1. */
    progress: number;

    /** Why it failed, with whether another try can pass; null unless failed. */
    failure: FailureWords | null;
    uploadId: string | null;
    key: string | null;
};

/** The files waiting to go with a place's next send. */
export type AttachmentTray = {
    /** In the order picked. */
    files: TrayFile[];

    /** What the tray said about the last pick as a whole, such as files past the most a message carries. */
    notice: string | null;

    /** Adds picked files, each uploading at once. */
    add: (files: File[]) => void;

    /** Takes a file out; its upload, if made, is left for storage to sweep. */
    remove: (localId: string) => void;

    /** Tries a failed file again. */
    retry: (localId: string) => void;
};

export function canAttach(_limits: AttachmentLimits): boolean {
    return false;
}

/**
 * Whether a tray's files can go: every one is up. A file still going, or failed, holds the send.
 *
 * @param tray The tray.
 *
 * @returns True when the tray holds files and each is uploaded.
 */
export function isTrayReady(tray: AttachmentTray): boolean {
    return tray.files.length > 0 && tray.files.every(file => file.status === "uploaded");
}

// #endregion Trays

/** A file's link: to draw it, and to download it under its name; or that it may not be read. */
export type AttachmentLink = { url: string | null, downloadUrl: string | null, isUnavailable: boolean };

/** The session's links, kept by file. */
export type AttachmentLinks = {
    /**
     * Asks for links to files, signed in one call with every other file asked for in the same
     * moment. A file with a link that is not near its end is not asked again.
     */
    request: (items: AttachmentItem[]) => void;

    /** A file's link, read reactively; null until signed, and once near its end. */
    linkOf: (item: AttachmentItem) => AttachmentLink | null;
};

/** Files for the session: each place's tray and every message's links. */
export type Attachments = {
    /** The tray of a place a composer types into, made the first time it is asked for. */
    trayFor: (placeKey: string, channelId: string) => AttachmentTray;

    /** Takes a place's files for its send, emptying the tray; null unless every one is up. */
    take: (placeKey: string) => AttachmentItem[] | null;

    /** Uploads files again from the copies kept, under new uploads; null when any cannot be. */
    reupload: (channelId: string, items: AttachmentItem[]) => Promise<AttachmentItem[] | null>;

    /** The local copy of a photo this person sent from this page, so it never waits on a link. */
    previewOf: (id: string) => string | null;
    links: AttachmentLinks;
};

/** What attachments reach outside themselves. */
export type AttachmentDependencies = {
    /** Calls a platform function, as every other call does. */
    call: (name: string, args: Record<string, unknown>) => Promise<CallResult>;
    fetch: (url: string, init: RequestInit) => Promise<Response>;
    projectUrl: string;
    publishableKey: string;

    /** The person's platform token, read at each request. */
    currentToken: () => string | null;

    /** A fresh request for an upload, the browser's own unless a test stands in; one has progress where fetch has none. */
    newRequest?: () => XMLHttpRequest;
    limits: () => AttachmentLimits;
    codec?: ImageCodec;
    objectUrls?: { create: (blob: Blob) => string, revoke: (url: string) => void };
    now?: () => number;
};

/** What a row and the viewer read: local copies and the session's links. */
export type AttachmentViews = Pick<Attachments, "previewOf" | "links">;

/** Where a row finds the session's attachment views, provided once by the chat. */
export const attachmentViewsKey: InjectionKey<AttachmentViews> = Symbol("chat-attachment-views");

/** What an upload's record is answered with by begin. */
type BeginAnswer = { upload_id?: string, key?: string };

/** A file kept for the session once uploaded: its bytes, so it can be uploaded again. */
type KeptFile = { blob: Blob, item: AttachmentItem };

/**
 * Creates the session's attachments.
 *
 * @param dependencies What attachments reach outside themselves.
 *
 * @returns The trays, the links and the session's own files.
 */
export function createAttachments(dependencies: AttachmentDependencies): Attachments {
    const projectUrl = dependencies.projectUrl.replace(/\/+$/, "");
    const codec = dependencies.codec ?? browserImageCodec;
    const objectUrls = dependencies.objectUrls ?? { create: (blob: Blob) => URL.createObjectURL(blob), revoke: (url: string) => URL.revokeObjectURL(url) };
    const now = dependencies.now ?? ((): number => Date.now());
    const newRequest = dependencies.newRequest ?? ((): XMLHttpRequest => new XMLHttpRequest());

    const trays = new Map<string, AttachmentTray>();

    // Per tray file, what is not drawn: its bytes, its room, and when begin recorded it.
    const pending = new Map<string, { blob: Blob, channelId: string, begunAt: number }>();

    // The person's own uploads by id, for the local copy and to upload again; held for the page.
    const kept = new Map<string, KeptFile>();
    const previews = reactive(new Map<string, string>());
    let localIds = 0;

    // #region Uploading

    /** Records uploads for files in a room; the answer is in the files' order. */
    async function begin(channelId: string, files: PreparedFile[]): Promise<{ ok: true, uploads: Array<{ uploadId: string, key: string }> } | { ok: false, failure: FailureWords }> {
        const result = await dependencies.call("chat_attachment_begin", {
            p_channel_id: channelId,
            p_files: files.map(file => ({ name: file.name, type: file.type, size: file.blob.size, width: file.width, height: file.height }))
        });
        if (!result.ok) {
            return { ok: false, failure: failureWords(result.error) };
        }

        const answers = Array.isArray(result.data) ? result.data as BeginAnswer[] : [];
        const uploads = answers.map(answer => ({ uploadId: answer.upload_id ?? "", key: answer.key ?? "" }));
        if (uploads.length !== files.length || uploads.some(upload => !upload.uploadId || !upload.key)) {
            return { ok: false, failure: failedWords("Couldn't upload.") };
        }

        return { ok: true, uploads };
    }

    /** Sends bytes to Storage under a key, telling progress as it goes. */
    function upload(key: string, blob: Blob, type: string, onProgress: (fraction: number) => void): Promise<boolean> {
        const token = dependencies.currentToken();
        if (!token) {
            return Promise.resolve(false);
        }

        return new Promise(resolve => {
            const request = newRequest();
            request.open("POST", `${projectUrl}/storage/v1/object/${bucket}/${key}`);
            request.setRequestHeader("apikey", dependencies.publishableKey);
            request.setRequestHeader("Authorization", `Bearer ${token}`);
            // A key is taken once; a second upload to it is refused rather than replacing the first.
            request.setRequestHeader("x-upsert", "false");
            // Bare, never with parameters, which Storage would keep and the send would refuse.
            request.setRequestHeader("content-type", type);
            request.setRequestHeader("cache-control", "max-age=3600");
            request.upload.onprogress = event => {
                if (event.lengthComputable && event.total > 0) {
                    onProgress(event.loaded / event.total);
                }
            };
            request.onload = () => resolve(request.status >= 200 && request.status < 300);
            request.onerror = () => resolve(false);
            request.onabort = () => resolve(false);
            request.send(blob);
        });
    }

    /** Begins a tray file where it has no live record, then uploads it. */
    async function send(tray: AttachmentTray, file: TrayFile): Promise<void> {
        const held = pending.get(file.localId);
        if (!held) {
            return;
        }

        file.status = "uploading";
        file.failure = null;
        file.progress = 0;

        const isBegun = !!file.key && now() - held.begunAt < beginLifetimeMs;
        if (!isBegun) {
            const begun = await begin(held.channelId, [{ blob: held.blob, name: file.name, type: file.type, kind: file.kind, width: file.width, height: file.height }]);
            if (!tray.files.includes(file)) {
                return;
            }
            if (!begun.ok) {
                file.status = "failed";
                file.failure = begun.failure;
                return;
            }
            file.uploadId = begun.uploads[0].uploadId;
            file.key = begun.uploads[0].key;
            held.begunAt = now();
        }

        const isUp = await upload(file.key as string, held.blob, file.type, fraction => file.progress = fraction);
        if (!tray.files.includes(file)) {
            return;
        }
        if (!isUp) {
            file.status = "failed";
            file.failure = failedWords("Couldn't upload.");
            return;
        }

        file.progress = 1;
        file.status = "uploaded";
    }

    /** Readies a picked file in its tray row, then sends it. */
    async function start(tray: AttachmentTray, file: TrayFile, picked: File, channelId: string): Promise<void> {
        const limits = dependencies.limits();
        const prepared = await prepare(picked, limits, codec);
        if (!tray.files.includes(file)) {
            return;
        }

        if ("alone" in prepared) {
            file.status = "failed";
            file.failure = prepared;
            return;
        }

        file.name = prepared.name;
        file.type = prepared.type;
        file.kind = prepared.kind;
        file.width = prepared.width;
        file.height = prepared.height;
        file.size = prepared.blob.size;
        if (prepared.blob.size > limits.attachment_max_bytes) {
            file.status = "failed";
            file.failure = finalWords(`Files can be up to ${formatFileSize(limits.attachment_max_bytes)}.`);
            return;
        }

        if (prepared.kind === "image") {
            file.previewUrl = objectUrls.create(prepared.blob);
        }
        pending.set(file.localId, { blob: markRaw(prepared.blob), channelId, begunAt: 0 });
        await send(tray, file);
    }

    function makeTray(channelId: string): AttachmentTray {
        const tray: AttachmentTray = reactive({
            files: [],
            notice: null,
            add: (picked: File[]): void => {
                const room = Math.max(0, dependencies.limits().attachment_max_files - tray.files.length);
                tray.notice = picked.length > room ? `A message can carry up to ${dependencies.limits().attachment_max_files} files.` : null;

                for (const one of picked.slice(0, room)) {
                    tray.files.push({
                        localId: `file-${++localIds}`,
                        name: one.name,
                        kind: "file",
                        type: declaredType(one.name, one.type),
                        size: one.size,
                        width: null,
                        height: null,
                        previewUrl: null,
                        status: "preparing",
                        progress: 0,
                        failure: null,
                        uploadId: null,
                        key: null
                    });
                    // the row read back is the reactive one, so the strip follows every change
                    void start(tray, tray.files[tray.files.length - 1], one, channelId);
                }
            },
            remove: (localId: string): void => {
                const index = tray.files.findIndex(file => file.localId === localId);
                if (index < 0) {
                    return;
                }
                const [file] = tray.files.splice(index, 1);
                pending.delete(localId);
                if (file.previewUrl) {
                    objectUrls.revoke(file.previewUrl);
                }
                tray.notice = null;
            },
            retry: (localId: string): void => {
                const file = tray.files.find(f => f.localId === localId);
                if (file?.status === "failed" && file.failure?.canRetry) {
                    void send(tray, file);
                }
            }
        }) as AttachmentTray;

        return tray;
    }

    // #endregion Uploading

    // #region Links

    // Each file's link by id, with when it ends; reactive, so a row draws its file once signed.
    const links = reactive(new Map<string, { link: AttachmentLink, endsAt: number }>());
    const asked = new Map<string, AttachmentItem>();
    const inFlight = new Set<string>();
    let isFlushQueued = false;

    function isFresh(id: string): boolean {
        const held = links.get(id);
        return !!held && held.endsAt - linkRenewMs > now();
    }

    /** Signs every key asked for in this moment, fifty to a call. */
    async function flush(): Promise<void> {
        isFlushQueued = false;
        const items = Array.from(asked.values());
        asked.clear();
        items.forEach(item => inFlight.add(item.id));

        const batches: AttachmentItem[][] = [];
        for (let i = 0; i < items.length; i += signBatch) {
            batches.push(items.slice(i, i + signBatch));
        }

        await Promise.all(batches.map(sign));
    }

    async function sign(items: AttachmentItem[]): Promise<void> {
        const startedAt = now();
        const token = dependencies.currentToken();
        let answer: Array<{ path?: string, signedURL?: string | null, error?: string | null }> | null = null;
        try {
            if (token) {
                const response = await dependencies.fetch(`${projectUrl}/storage/v1/object/sign/${bucket}`, {
                    method: "POST",
                    headers: { "Authorization": `Bearer ${token}`, "apikey": dependencies.publishableKey, "Content-Type": "application/json" },
                    body: JSON.stringify({ expiresIn: linkSeconds, paths: items.map(item => item.key) })
                });
                const body = await response.json().catch(() => null) as unknown;
                answer = response.ok && Array.isArray(body) ? body : null;
            }
        }
        catch {
            answer = null;
        }
        finally {
            items.forEach(item => inFlight.delete(item.id));
        }

        // A call that failed as a whole leaves its files unasked, so the next request asks again.
        if (!answer) {
            return;
        }

        const byPath = new Map(answer.map(entry => [entry.path, entry]));
        for (const item of items) {
            const signed = byPath.get(item.key);
            const endsAt = startedAt + linkSeconds * 1000;
            if (!signed?.signedURL || signed.error) {
                links.set(item.id, { link: { url: null, downloadUrl: null, isUnavailable: true }, endsAt });
                continue;
            }

            const url = `${projectUrl}/storage/v1${signed.signedURL}`;
            // Storage names the download only when asked; a file always downloads under its own name.
            const downloadUrl = `${url}${url.includes("?") ? "&" : "?"}download=${encodeURIComponent(item.name)}`;
            links.set(item.id, { link: { url, downloadUrl, isUnavailable: false }, endsAt });
        }
    }

    const linkCache: AttachmentLinks = {
        request: (items: AttachmentItem[]): void => {
            for (const item of items) {
                if (!isFresh(item.id) && !inFlight.has(item.id)) {
                    asked.set(item.id, item);
                }
            }

            if (asked.size > 0 && !isFlushQueued) {
                isFlushQueued = true;
                queueMicrotask(() => void flush());
            }
        },

        linkOf: (item: AttachmentItem): AttachmentLink | null => {
            const held = links.get(item.id);
            return held && held.endsAt - linkRenewMs > now() ? held.link : null;
        }
    };

    // #endregion Links

    return {
        trayFor: (placeKey: string, channelId: string): AttachmentTray => {
            let tray = trays.get(placeKey);
            if (!tray) {
                tray = makeTray(channelId);
                trays.set(placeKey, tray);
            }
            return tray;
        },

        take: (placeKey: string): AttachmentItem[] | null => {
            const tray = trays.get(placeKey);
            if (!tray || !isTrayReady(tray)) {
                return null;
            }

            const items = tray.files.map((file): AttachmentItem => {
                const item: AttachmentItem = {
                    id: file.uploadId as string,
                    key: file.key as string,
                    kind: file.kind,
                    name: file.name,
                    size: file.size,
                    type: file.type,
                    width: file.width,
                    height: file.height
                };
                const held = pending.get(file.localId);
                if (held) {
                    kept.set(item.id, { blob: held.blob, item });
                }
                if (file.previewUrl) {
                    previews.set(item.id, file.previewUrl);
                }
                pending.delete(file.localId);
                return item;
            });
            tray.files.splice(0);
            tray.notice = null;

            return items;
        },

        reupload: async (channelId: string, items: AttachmentItem[]): Promise<AttachmentItem[] | null> => {
            const copies = items.map(item => kept.get(item.id));
            if (copies.some(copy => !copy)) {
                return null;
            }

            const files = (copies as KeptFile[]).map(({ blob, item }) => ({ blob, name: item.name, type: item.type, kind: item.kind, width: item.width ?? null, height: item.height ?? null }));
            const begun = await begin(channelId, files);
            if (!begun.ok) {
                return null;
            }

            const renewed = items.map((item, index) => ({ ...item, id: begun.uploads[index].uploadId, key: begun.uploads[index].key }));
            const sent = await Promise.all(renewed.map((item, index) => upload(item.key, files[index].blob, item.type, () => undefined)));
            if (sent.some(isUp => !isUp)) {
                return null;
            }

            renewed.forEach((item, index) => {
                kept.set(item.id, { blob: files[index].blob, item });
                const preview = previews.get(items[index].id);
                if (preview) {
                    previews.set(item.id, preview);
                }
            });
            return renewed;
        },

        previewOf: (id: string): string | null => previews.get(id) ?? null,

        links: linkCache
    };
}
