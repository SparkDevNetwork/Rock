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
// The stand-ins the attachment specs share, each in the shape its real caller produces: the
// browser's XMLHttpRequest as an upload drives it, the platform's begin answer, Storage's sign
// answer, and an image codec standing in for the browser's decoder and canvas.
import { CallResult } from "../../../../src/Communication/Chat/ChatShell/platformCall.partial";
import { AttachmentDependencies, AttachmentLimits, DecodedImage, ImageCodec } from "../../../../src/Communication/Chat/ChatShell/composables/useAttachments.partial";

export const projectUrl = "https://chat.example";
export const publishableKey = "pk-1";
export const token = "person-token";
export const tenant = "10000000-0000-4000-8000-000000000001";

/** The platform's attachment limits as the token exchange answers them. */
export const limits: AttachmentLimits = {
    attachment_max_bytes: 26214400,
    attachment_max_files: 10,
    attachment_types: [
        "image/jpeg", "image/png", "image/gif", "image/webp",
        "application/pdf", "text/plain", "text/csv",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "application/vnd.openxmlformats-officedocument.presentationml.presentation",
        "video/mp4", "video/quicktime", "audio/mpeg", "audio/mp4"
    ]
};

/** Waits out every promise already queued, and those they queue. */
export async function settle(): Promise<void> {
    for (let i = 0; i < 30; i++) {
        await Promise.resolve();
    }
}

/** The browser's request, as an upload opens, heads, sends and hears it. */
export class FakeRequest {
    method = "";
    url = "";
    headers: Record<string, string> = {};
    body: unknown = null;
    status = 0;
    responseText = "";
    upload: { onprogress: ((event: { loaded: number, total: number, lengthComputable: boolean }) => void) | null } = { onprogress: null };
    onload: (() => void) | null = null;
    onerror: (() => void) | null = null;
    onabort: (() => void) | null = null;
    isAborted = false;

    open(method: string, url: string): void {
        this.method = method;
        this.url = url;
    }

    setRequestHeader(name: string, value: string): void {
        this.headers[name.toLowerCase()] = value;
    }

    send(body: unknown): void {
        this.body = body;
    }

    abort(): void {
        this.isAborted = true;
        this.onabort?.();
    }

    /** Storage reports bytes sent. */
    progress(loaded: number, total: number): void {
        this.upload.onprogress?.({ loaded, total, lengthComputable: true });
    }

    /** Storage answers, as its upload route does: a key on success, an error body on refusal. */
    respond(status: number, body: unknown = status < 300 ? { Key: "chat-attachments/x" } : { statusCode: String(status), error: "Bad", message: "refused" }): void {
        this.status = status;
        this.responseText = JSON.stringify(body);
        this.onload?.();
    }

    /** Nothing was reached. */
    fail(): void {
        this.onerror?.();
    }
}

/** A sign answer item, as Storage writes one: a relative link, or an error and no link. */
export type SignItem = { path: string, signedURL: string | null, error: string | null };

export type Harness = {
    dependencies: AttachmentDependencies;
    requests: FakeRequest[];
    begins: Array<Record<string, unknown>>;
    beginAnswers: Array<CallResult | ((args: Record<string, unknown>) => CallResult)>;
    signs: Array<{ url: string, init: RequestInit, body: { expiresIn: number, paths: string[] } }>;
    signAnswer: { value: (paths: string[]) => SignItem[] };
    now: { value: number };
    revoked: string[];
};

let uploadCounter = 0;

/** An upload id and key as begin makes them: lowercase, the church first. */
export function nextUpload(): { upload_id: string, key: string } {
    uploadCounter++;
    const id = `d0000000-0000-4000-8000-${String(uploadCounter).padStart(12, "0")}`;
    return { upload_id: id, key: `${tenant}/${id}` };
}

/**
 * Stands in for every call an attachment makes. Begin answers a fresh upload per file unless a
 * test queues an answer; sign answers every path with a link unless a test says otherwise.
 */
export function harness(codec: ImageCodec = fakeCodec()): Harness {
    const requests: FakeRequest[] = [];
    const begins: Array<Record<string, unknown>> = [];
    const beginAnswers: Harness["beginAnswers"] = [];
    const signs: Harness["signs"] = [];
    const signAnswer: Harness["signAnswer"] = {
        value: paths => paths.map(path => ({ path, signedURL: `/object/sign/chat-attachments/${path}?token=t-${path.slice(-4)}`, error: null }))
    };
    const now = { value: Date.parse("2026-10-28T10:00:00Z") };
    const revoked: string[] = [];
    let urls = 0;

    const dependencies: AttachmentDependencies = {
        call: async (name, args) => {
            if (name !== "chat_attachment_begin") {
                throw new Error(`unexpected call ${name}`);
            }
            begins.push(args);
            const queued = beginAnswers.shift();
            if (queued) {
                return typeof queued === "function" ? queued(args) : queued;
            }
            return { ok: true, data: (args.p_files as unknown[]).map(() => nextUpload()) };
        },
        fetch: async (url, init) => {
            const body = JSON.parse(init.body as string) as { expiresIn: number, paths: string[] };
            signs.push({ url, init, body });
            const answer = signAnswer.value(body.paths);
            return { ok: true, status: 200, headers: { get: () => null }, json: async () => answer } as unknown as Response;
        },
        projectUrl,
        publishableKey,
        currentToken: () => token,
        newRequest: () => {
            const request = new FakeRequest();
            requests.push(request);
            return request as unknown as XMLHttpRequest;
        },
        limits: () => limits,
        codec,
        objectUrls: { create: () => `blob:local-${++urls}`, revoke: url => revoked.push(url) },
        now: () => now.value
    };

    return { dependencies, requests, begins, beginAnswers, signs, signAnswer, now, revoked };
}

/** What the fake codec was asked to encode. */
export type Encoded = { width: number, height: number, type: string, quality: number | undefined, from: Blob };

/**
 * A codec standing in for the browser's: a blob decodes to the size its test gave it, a blob it
 * was never given cannot be decoded, and encoding records what it was asked for.
 */
export function fakeCodec(sizes: Map<Blob, { width: number, height: number }> = new Map(), encoded: Encoded[] = []): ImageCodec & { sizes: typeof sizes, encoded: Encoded[] } {
    return {
        sizes,
        encoded,
        decode: async (blob: Blob): Promise<DecodedImage> => {
            const size = sizes.get(blob);
            if (!size) {
                throw new DOMException("The source image cannot be decoded.", "InvalidStateError");
            }
            return { width: size.width, height: size.height, source: blob as unknown as CanvasImageSource };
        },
        encode: async (image, width, height, type, quality) => {
            encoded.push({ width, height, type, quality, from: image.source as unknown as Blob });
            return new Blob(["re-encoded"], { type });
        }
    };
}

/** A picked file of a size, with the type the browser reported. */
export function picked(name: string, type: string, size = 1000): File {
    return new File([new Uint8Array(size)], name, { type });
}
