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
// read the message; links last ten minutes, so they are kept by file, and the files of every row
// on screen are signed again shortly before they expire, as many as can go in one call.
//
// This lives apart from the sender because its lifetime is not a send's: a tray outlives the
// sends that take from it, and links outlive both.
import { InjectionKey, markRaw, reactive, ref } from "vue";
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

/** How long after a signing that failed as a whole its files are asked again. */
const linkRetryMs = 15 * 1000;

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
const fileEmpty = "This file is empty.";
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

        // A JPEG has no transparency, so a transparent WebP is laid on white first, else it turns
        // black. The white is one pixel of full channels stretched over the canvas: a matte for the
        // photo's bytes, not a colour of the chat's, which only the token sheet names.
        if (type === "image/jpeg") {
            const matte = document.createElement("canvas");
            matte.width = 1;
            matte.height = 1;
            const pixel = matte.getContext("2d");
            const full = pixel?.createImageData(1, 1);
            if (pixel && full) {
                full.data.fill(255);
                pixel.putImageData(full, 0, 0);
                context.drawImage(matte, 0, 0, width, height);
            }
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
    // Said at once in its own words rather than as a failure from the platform later.
    if (file.size === 0) {
        return finalWords(fileEmpty);
    }

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

    /** Takes a file out, ending its upload if still going; one already made is left for storage to sweep. */
    remove: (localId: string) => void;

    /** Tries a failed file again. */
    retry: (localId: string) => void;
};

/**
 * Whether files can be offered: the platform answered its limits for them. A platform that has
 * not, or a session not started, answers none, and nothing is offered rather than guessed at.
 *
 * @param limits The platform's limits.
 *
 * @returns True when a file could be taken.
 */
export function canAttach(limits: AttachmentLimits): boolean {
    return limits.attachment_max_files > 0 && limits.attachment_max_bytes > 0 && limits.attachment_types.length > 0;
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
     * Keeps files linked while a row draws them: each without a good link is signed now, in one
     * call with every other file asked for in the same moment, and signed again shortly before
     * its link ends, until the row lets go by calling what this answers.
     */
    subscribe: (items: AttachmentItem[]) => () => void;

    /** A file's link, read reactively; null until signed, and once it has ended. */
    linkOf: (item: AttachmentItem) => AttachmentLink | null;

    /** Whether a file's link has time left to be followed. */
    isFresh: (item: AttachmentItem) => boolean;

    /** A file's link with time left to be followed, signed afresh when the one held is near its end. */
    fresh: (item: AttachmentItem) => Promise<AttachmentLink | null>;
};

/** Files for the session: each place's tray and every message's links. */
export type Attachments = {
    /** The tray of a place a composer types into, made the first time it is asked for. */
    trayFor: (placeKey: string, channelId: string) => AttachmentTray;

    /** Takes a place's files for its send, emptying the tray; null unless every one is up. */
    take: (placeKey: string) => AttachmentItem[] | null;

    /** Uploads files again from the copies kept, under new uploads; null when any cannot be. */
    reupload: (channelId: string, items: AttachmentItem[]) => Promise<AttachmentItem[] | null>;

    /**
     * The local copy of a photo this person sent from this page, so it never waits on a link; let
     * go once its link is signed, which draws it from then.
     */
    previewOf: (id: string) => string | null;
    links: AttachmentLinks;

    /** Lets go of the bytes kept for a send's files, once the platform has stored the message. */
    release: (items: AttachmentItem[]) => void;

    /** Ends every upload and renewal and lets every local copy go; nothing is written after. */
    stop: () => void;
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

    /** The browser's timers unless a test stands in, for renewing links before they end. */
    timers?: { set: (run: () => void, ms: number) => unknown, clear: (handle: unknown) => void };
};

/** What a row and the viewer read: local copies and the session's links. */
export type AttachmentViews = Pick<Attachments, "previewOf" | "links">;

/** Where a row finds the session's attachment views, provided once by the chat. */
export const attachmentViewsKey: InjectionKey<AttachmentViews> = Symbol("chat-attachment-views");

/* eslint-disable @typescript-eslint/naming-convention */
/** What an upload's record is answered with by begin. */
type BeginAnswer = { upload_id?: string, key?: string };
/* eslint-enable @typescript-eslint/naming-convention */

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
    const timers = dependencies.timers ?? {
        set: (run: () => void, ms: number): unknown => setTimeout(run, ms),
        clear: (handle: unknown): void => clearTimeout(handle as ReturnType<typeof setTimeout>)
    };

    // Once stopped, every await that resumes leaves without writing, and nothing new starts.
    let isStopped = false;

    const trays = new Map<string, AttachmentTray>();

    // Per tray file, what is not drawn: its bytes, its room, when begin recorded it, and its
    // upload while one is going, so taking the file out ends it.
    const pending = new Map<string, { blob: Blob, channelId: string, begunAt: number, request: XMLHttpRequest | null }>();

    // Every upload going, from a tray or sent again, so stopping ends them all.
    const uploading = new Set<XMLHttpRequest>();

    // The person's own uploads by id, to upload again while the send is unconfirmed, and the local
    // copy of each photo until its link draws it.
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

    /**
     * Whether Storage refused an upload because its key already holds a file. Begin makes each key
     * for one upload, so the file there is this one, from an earlier try whose answer was lost; the
     * send checks its size and type against begin's record all the same.
     */
    function isAlreadyStored(request: XMLHttpRequest): boolean {
        if (request.status === 409) {
            return true;
        }

        try {
            const body = JSON.parse(request.responseText) as { statusCode?: string } | null;
            return body?.statusCode === "409";
        }
        catch {
            // not Storage's error body, so not its answer for a stored key
            return false;
        }
    }

    /** Sends bytes to Storage under a key, telling progress as it goes; the request is held where asked. */
    function upload(key: string, blob: Blob, type: string, onProgress: (fraction: number) => void, holder?: { request: XMLHttpRequest | null }): Promise<boolean> {
        const token = dependencies.currentToken();
        if (!token) {
            return Promise.resolve(false);
        }

        return new Promise(resolve => {
            const request = newRequest();
            uploading.add(request);
            if (holder) {
                holder.request = request;
            }
            const finish = (isUp: boolean): void => {
                uploading.delete(request);
                if (holder?.request === request) {
                    holder.request = null;
                }
                resolve(isUp);
            };
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
            request.onload = () => finish((request.status >= 200 && request.status < 300) || isAlreadyStored(request));
            request.onerror = () => finish(false);
            request.onabort = () => finish(false);
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
            if (isStopped || !tray.files.includes(file)) {
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

        const isUp = await upload(file.key as string, held.blob, file.type, fraction => file.progress = fraction, held);
        if (isStopped || !tray.files.includes(file)) {
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
        if (isStopped || !tray.files.includes(file)) {
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
        pending.set(file.localId, { blob: markRaw(prepared.blob), channelId, begunAt: 0, request: null });
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
                pending.get(localId)?.request?.abort();
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

    // The moment links were last judged by, moved on as a watched link is renewed or ends, so a
    // row reading a link follows it past its end though time itself is not reactive.
    const clock = ref(now());

    // The files rows on screen draw, each with how many rows draw it; only these are renewed.
    const watched = new Map<string, { item: AttachmentItem, count: number }>();

    // When a file whose signing failed as a whole may be asked again.
    const retryAt = new Map<string, number>();
    const asked = new Map<string, AttachmentItem>();
    const inFlight = new Set<string>();
    let isFlushQueued = false;

    // The one timer for the next moment a watched file needs anything.
    let wake: unknown = null;

    function isFresh(id: string): boolean {
        const held = links.get(id);
        return !!held && held.endsAt - linkRenewMs > now();
    }

    /** Asks for each file without a good link and not already being signed, together in a moment. */
    function ask(items: AttachmentItem[]): void {
        for (const item of items) {
            if (!isFresh(item.id) && !inFlight.has(item.id)) {
                asked.set(item.id, item);
            }
        }

        if (asked.size > 0 && !isFlushQueued && !isStopped) {
            isFlushQueued = true;
            queueMicrotask(() => void flush());
        }
    }

    /** Signs every key asked for in this moment, fifty to a call. */
    async function flush(): Promise<void> {
        isFlushQueued = false;
        if (isStopped) {
            return;
        }
        const items = Array.from(asked.values());
        asked.clear();

        const batches: AttachmentItem[][] = [];
        for (let i = 0; i < items.length; i += signBatch) {
            batches.push(items.slice(i, i + signBatch));
        }

        await Promise.all(batches.map(sign));
        schedule();
    }

    async function sign(items: AttachmentItem[]): Promise<void> {
        items.forEach(item => inFlight.add(item.id));
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

        if (isStopped) {
            return;
        }

        // A call that failed as a whole leaves its files' links as they were, asked again shortly.
        if (!answer) {
            items.forEach(item => retryAt.set(item.id, now() + linkRetryMs));
            return;
        }

        const byPath = new Map(answer.map(entry => [entry.path, entry]));
        for (const item of items) {
            retryAt.delete(item.id);
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

            // The photo draws from its link from now on, so its local copy is let go.
            const preview = previews.get(item.id);
            if (preview) {
                previews.delete(item.id);
                objectUrls.revoke(preview);
            }
        }
    }

    /**
     * Sets the one timer for the next moment a watched file needs anything: its link signed again
     * a minute before its end, or once a failed signing may be tried again; or its link ending,
     * which moves the clock so the rows drawing it see it gone.
     */
    function schedule(): void {
        if (wake !== null) {
            timers.clear(wake);
            wake = null;
        }
        if (isStopped) {
            return;
        }

        const at = now();
        let next = Number.POSITIVE_INFINITY;
        for (const id of watched.keys()) {
            // its answer schedules again
            if (inFlight.has(id) || asked.has(id)) {
                continue;
            }
            const held = links.get(id);
            next = Math.min(next, Math.max(held ? held.endsAt - linkRenewMs : at, retryAt.get(id) ?? 0));
            if (held && held.endsAt > at) {
                next = Math.min(next, held.endsAt);
            }
        }

        if (next !== Number.POSITIVE_INFINITY) {
            wake = timers.set(onWake, Math.max(0, next - at));
        }
    }

    function onWake(): void {
        wake = null;
        const at = now();
        clock.value = at;
        ask(Array.from(watched.values(), entry => entry.item).filter(item => (retryAt.get(item.id) ?? 0) <= at));
        schedule();
    }

    const linkCache: AttachmentLinks = {
        subscribe: (items: AttachmentItem[]): (() => void) => {
            if (isStopped) {
                return () => undefined;
            }

            for (const item of items) {
                watched.set(item.id, { item, count: (watched.get(item.id)?.count ?? 0) + 1 });
            }
            ask(items);
            schedule();

            let isReleased = false;
            return (): void => {
                if (isReleased) {
                    return;
                }
                isReleased = true;
                for (const item of items) {
                    const entry = watched.get(item.id);
                    if (entry && entry.count > 1) {
                        entry.count--;
                    }
                    else {
                        watched.delete(item.id);
                    }
                }
                schedule();
            };
        },

        linkOf: (item: AttachmentItem): AttachmentLink | null => {
            // read so a row follows the clock past a link's end
            void clock.value;
            const held = links.get(item.id);
            return held && held.endsAt > now() ? held.link : null;
        },

        isFresh: (item: AttachmentItem): boolean => isFresh(item.id),

        fresh: async (item: AttachmentItem): Promise<AttachmentLink | null> => {
            if (isStopped) {
                return null;
            }
            if (!isFresh(item.id)) {
                await sign([item]);
                schedule();
            }

            return linkCache.linkOf(item);
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
            if (isStopped || !begun.ok) {
                return null;
            }

            const renewed = items.map((item, index) => ({ ...item, id: begun.uploads[index].uploadId, key: begun.uploads[index].key }));
            const sent = await Promise.all(renewed.map((item, index) => upload(item.key, files[index].blob, item.type, () => undefined)));
            if (isStopped || sent.some(isUp => !isUp)) {
                return null;
            }

            // The files move to their new uploads; the old ones are never sent.
            renewed.forEach((item, index) => {
                kept.delete(items[index].id);
                kept.set(item.id, { blob: files[index].blob, item });
                const preview = previews.get(items[index].id);
                if (preview) {
                    previews.delete(items[index].id);
                    previews.set(item.id, preview);
                }
            });
            return renewed;
        },

        previewOf: (id: string): string | null => previews.get(id) ?? null,

        links: linkCache,

        release: (items: AttachmentItem[]): void => {
            items.forEach(item => kept.delete(item.id));
        },

        stop: (): void => {
            isStopped = true;
            Array.from(uploading).forEach(request => request.abort());
            uploading.clear();
            schedule();
            asked.clear();
            watched.clear();

            const urls = new Set(previews.values());
            trays.forEach(tray => tray.files.forEach(file => {
                if (file.previewUrl) {
                    urls.add(file.previewUrl);
                }
            }));
            urls.forEach(url => objectUrls.revoke(url));
            previews.clear();
            kept.clear();
            pending.clear();
        }
    };
}
