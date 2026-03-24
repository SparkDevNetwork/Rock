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

import { nextTick, onBeforeUnmount, Ref, watch } from "vue";
import { SystemCommunicationDetailBlockActionInvoker } from "./types.partial";
import { HttpResult } from "@Obsidian/Types/Utility/http";
import { useInvokeBlockAction } from "@Obsidian/Utility/block";
import { SystemCommunicationBag } from "@Obsidian/ViewModels/Blocks/Communication/SystemCommunicationDetail/systemCommunicationBag";
import { SystemCommunicationDetailGetPreviewMessageRequestBag } from "@Obsidian/ViewModels/Blocks/Communication/SystemCommunicationDetail/systemCommunicationDetailGetPreviewMessageRequestBag";
import { isNullish } from "@Obsidian/Utility/util";
import { KeyValueItem } from "@Obsidian/Types/Controls/keyValueItem";

/**
 * Creates a typed block action invoker for the System Communication Detail block.
 */
export function useInvokeSystemCommunicationDetailBlockAction(): SystemCommunicationDetailBlockActionInvoker {
    const invokeBlockAction = useInvokeBlockAction();

    return {
        save(bag: SystemCommunicationBag): Promise<HttpResult<void>> {
            return invokeBlockAction("Save", { bag });
        },
        getPreviewMessage(bag: SystemCommunicationDetailGetPreviewMessageRequestBag): Promise<HttpResult<string>> {
            return invokeBlockAction("GetPreviewMessage", { bag });
        }
    };
}

/**
 * Extracts Lava fields from the HTML message template's noscript block.
 * Parses {% assign key = 'value' %} patterns.
 */
export function getLavaFieldsFromHtmlMessage(templateHtml: string): Record<string, string> {
    const lavaFieldsTemplateDictionary: Record<string, string> = {};

    const lavaFieldsMatch = templateHtml.match(/<noscript[^>]*id=["']lava-fields["'][^>]*>([\s\S]*?)<\/noscript>/i);
    if (!lavaFieldsMatch) {
        return lavaFieldsTemplateDictionary;
    }

    const lavaFieldContent = lavaFieldsMatch[1];
    const templateDocLavaFieldLines = lavaFieldContent
        .split(/\r?\n/)
        .map(line => line.trim())
        .filter(line => line.length > 0);

    const lavaFieldRegex = /{% assign (.*?)\s*=\s*(.*?) %}/;

    for (const line of templateDocLavaFieldLines) {
        const match = line.match(lavaFieldRegex);
        if (match && match.length === 3) {
            const key = match[1].trim().replace(/\s+/g, "");
            const value = match[2].trim().replace(/^'|'$/g, "");
            lavaFieldsTemplateDictionary[key] = value;
        }
    }

    return lavaFieldsTemplateDictionary;
}

/**
 * Checks if the HTML message template contains an element with id="template-logo".
 */
export function hasLogoInMessage(templateHtml: string): boolean {
    const logoRegex = /<img(?=[^>]*id=['"]template-logo['"])[^>]*>/i;
    return logoRegex.test(templateHtml);
}

/**
 * Replaces the src attribute of the template-logo img tag with the
 * uploaded image URL, or reverts to the placeholder if no image is set.
 */
export function updateLogoInMessage(templateHtml: string, binaryFileGuid: string | null): string {
    const templateLogoRegex = /<img(?=[^>]*id=['"]template-logo['"])(?=[^>]*src=['"]([^"'>]+)['"])[^>]*>/i;
    const match = templateHtml.match(templateLogoRegex);

    if (!match || match.length !== 2) {
        return templateHtml;
    }

    const originalHtml = match[0];
    const originalSrc = match[1];
    const newSrc = binaryFileGuid
        ? `/GetImage.ashx?guid=${binaryFileGuid}`
        : "/Content/EmailTemplates/placeholder-logo.png";

    return templateHtml.replace(originalHtml, originalHtml.replace(originalSrc, newSrc));
}

/**
 * Extracts the BinaryFile GUID from an existing template-logo img src,
 * returning null if no uploaded logo is present.
 */
export function getLogoBinaryFileGuidFromMessage(templateHtml: string): string | null {
    const regex = /<img(?=[^>]*id=['"]template-logo['"])(?=[^>]*src=['"]\/GetImage\.ashx\?guid=([a-f0-9-]+)['"])[^>]*>/i;
    const match = templateHtml.match(regex);
    return match ? match[1] : null;
}

/**
 * Attaches a load event handler to an iframe ref, automatically handling
 * cleanup when the ref changes or the component unmounts.
 */
export function useIframeOnLoad(
    iframeRef: Ref<HTMLIFrameElement | null | undefined>,
    onLoad: (iframe: HTMLIFrameElement) => void
): void {
    let cleanup: (() => void) | null = null;

    const attach = (iframe: HTMLIFrameElement): (() => void) => {
        const handler = (): void => onLoad(iframe);
        iframe.addEventListener("load", handler);

        if (iframe.contentDocument?.readyState === "complete") {
            nextTick(() => onLoad(iframe));
        }

        return (): void => {
            iframe.removeEventListener("load", handler);
        };
    };

    watch(iframeRef, (newIframe, _, onCleanup) => {
        cleanup?.();
        if (newIframe) {
            cleanup = attach(newIframe);
            onCleanup(() => cleanup?.());
        }
    }, { immediate: true });

    onBeforeUnmount(() => {
        cleanup?.();
    });
}

/**
 * Automatically adjusts iframe height to match its content height.
 */
export function useIframeAutoHeight(
    iframeRef: Ref<HTMLIFrameElement | null | undefined>,
    { yOffsetPx }: { yOffsetPx: number } = { yOffsetPx: 0 }
): void {
    useIframeOnLoad(iframeRef, (iframe) => {
        const doc = iframe.contentDocument || iframe.contentWindow?.document;
        if (!doc) {
            return;
        }

        const height = doc.body?.scrollHeight ?? 0;
        iframe.style.height = `${height + yOffsetPx}px`;
    });
}

/**
 * Converts a Record<string, string> to an array of KeyValueItem objects.
 */
export function recordAsKeyValueItems(record: Record<string, string>): KeyValueItem[] {
    const items: KeyValueItem[] = [];

    for (const key in record) {
        if (!isNullish(key)) {
            items.push({ key, value: record[key] });
        }
    }

    return items;
}

/**
 * Converts an array of KeyValueItem objects to a Record<string, string>.
 */
export function keyValueItemsAsRecord(items: KeyValueItem[]): Record<string, string> {
    const record: Record<string, string> = {};

    for (const { key, value } of items) {
        if (!isNullish(key) && !isNullish(value)) {
            record[key] = value;
        }
    }

    return record;
}
