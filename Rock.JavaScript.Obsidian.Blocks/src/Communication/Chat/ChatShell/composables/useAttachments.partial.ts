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
import { InjectionKey } from "vue";
import { CallResult } from "../platformCall.partial";
import { AttachmentItem, FailureWords } from "../types.partial";
import { AttachmentTypes, PlatformLimits } from "./useSession.partial";

/* eslint-disable @typescript-eslint/naming-convention */
export type AttachmentLimits = Pick<PlatformLimits, "attachment_max_bytes" | "attachment_max_files"> & AttachmentTypes;
/* eslint-enable @typescript-eslint/naming-convention */

export const attachmentPreviewWords = { photo: "", photos: "", file: "", files: "" };

export function attachmentsPreview(_items: AttachmentItem[] | null | undefined): string {
    return "";
}

export function formatFileSize(_bytes: number): string {
    return "";
}

export function declaredType(_name: string, _reported: string): string {
    return "";
}

export type DecodedImage = { width: number, height: number, source: CanvasImageSource, close?: () => void };

export type ImageCodec = {
    decode: (blob: Blob) => Promise<DecodedImage>;
    encode: (image: DecodedImage, width: number, height: number, type: string, quality: number | undefined) => Promise<Blob>;
};

export type TrayFile = {
    localId: string;
    name: string;
    kind: AttachmentItem["kind"];
    type: string;
    size: number;
    width: number | null;
    height: number | null;
    previewUrl: string | null;
    status: "preparing" | "uploading" | "uploaded" | "failed";
    progress: number;
    failure: FailureWords | null;
    uploadId: string | null;
    key: string | null;
};

export type AttachmentTray = {
    files: TrayFile[];
    notice: string | null;
    add: (files: File[]) => void;
    remove: (localId: string) => void;
    retry: (localId: string) => void;
};

export function isTrayReady(_tray: AttachmentTray): boolean {
    return false;
}

export type AttachmentLink = { url: string | null, downloadUrl: string | null, isUnavailable: boolean };

export type AttachmentLinks = {
    request: (items: AttachmentItem[]) => void;
    linkOf: (item: AttachmentItem) => AttachmentLink | null;
};

export type Attachments = {
    trayFor: (placeKey: string, channelId: string) => AttachmentTray;
    take: (placeKey: string) => AttachmentItem[] | null;
    reupload: (channelId: string, items: AttachmentItem[]) => Promise<AttachmentItem[] | null>;
    previewOf: (id: string) => string | null;
    links: AttachmentLinks;
};

export type AttachmentDependencies = {
    call: (name: string, args: Record<string, unknown>) => Promise<CallResult>;
    fetch: (url: string, init: RequestInit) => Promise<Response>;
    projectUrl: string;
    publishableKey: string;
    currentToken: () => string | null;
    newRequest?: () => XMLHttpRequest;
    limits: () => AttachmentLimits;
    codec?: ImageCodec;
    objectUrls?: { create: (blob: Blob) => string, revoke: (url: string) => void };
    now?: () => number;
};

export function createAttachments(_dependencies: AttachmentDependencies): Attachments {
    return {
        trayFor: () => ({ files: [], notice: null, add: () => undefined, remove: () => undefined, retry: () => undefined }),
        take: () => null,
        reupload: async () => null,
        previewOf: () => null,
        links: { request: () => undefined, linkOf: () => null }
    };
}

export type AttachmentViews = Pick<Attachments, "previewOf" | "links">;

export const attachmentViewsKey: InjectionKey<AttachmentViews> = Symbol("chat-attachment-views");
