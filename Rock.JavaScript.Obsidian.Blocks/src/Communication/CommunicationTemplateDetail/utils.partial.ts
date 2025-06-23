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
import { CommunicationTemplateDetailBlockActionInvoker, CommunicationTemplateMessageUtils, Feature, RecordUtils, UpdateMessageOptions, UpdateMessageResult } from "./types.partial";
import { CommunicationTemplateVersion } from "@Obsidian/Enums/Communication/communicationTemplateVersion";
import { HttpResult } from "@Obsidian/Types/Utility/http";
import { ValidationResult, ValidationRuleFunction } from "@Obsidian/Types/validationRules";
import { toGuidOrNull } from "@Obsidian/Utility/guid";
import { useInvokeBlockAction } from "@Obsidian/Utility/block";
import { CommunicationTemplateDetailCommunicationTemplateBag } from "@Obsidian/ViewModels/Blocks/Communication/CommunicationTemplateDetail/communicationTemplateDetailCommunicationTemplateBag";
import { CommunicationTemplateDetailGetPreviewMessageRequestBag } from "@Obsidian/ViewModels/Blocks/Communication/CommunicationTemplateDetail/communicationTemplateDetailGetPreviewMessageRequestBag";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { isNullish } from "@Obsidian/Utility/util";
import { KeyValueItem } from "@Obsidian/Types/Controls/keyValueItem";

export function createMaxLengthValidator(maxLength: number): ValidationRuleFunction {
    return (value: unknown): ValidationResult => {
        if (value && typeof value === "string" && value.length > maxLength) {
            return `cannot be longer than ${maxLength} characters.`;
        }

        return true;
    };
}

export function useInvokeCommunicationTemplateDetailBlockAction(): CommunicationTemplateDetailBlockActionInvoker {
    const invokeBlockAction = useInvokeBlockAction();

    return {
        saveTemplate(bag: CommunicationTemplateDetailCommunicationTemplateBag): Promise<HttpResult<void>> {
            return invokeBlockAction("SaveTemplate", { bag });
        },
        getPreviewMessage(bag: CommunicationTemplateDetailGetPreviewMessageRequestBag): Promise<HttpResult<string>> {
            return invokeBlockAction("GetPreviewMessage", { bag });
        }
    };
}

export function useIframeOnLoad(
    iframeRef: Ref<HTMLIFrameElement | null | undefined>,
    onLoad: (iframe: HTMLIFrameElement) => void
): void {
    let cleanup: (() => void) | null = null;

    const attach = (iframe: HTMLIFrameElement): (() => void) => {
        const handler = (): void => onLoad(iframe);

        iframe.addEventListener("load", handler);

        // Immediate "ready"-style invocation
        if (iframe.contentDocument?.readyState === "complete") {
            // Use nextTick to ensure style/layout is fully settled
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

export function useIframeAutoHeight(iframeRef: Ref<HTMLIFrameElement | null | undefined>, { yOffsetPx }: { yOffsetPx: number; } = { yOffsetPx: 0 }): void {
    useIframeOnLoad(iframeRef, (iframe) => {
        const doc = iframe.contentDocument || iframe.contentWindow?.document;
        if (!doc) {
            return;
        }

        const height = doc.body.scrollHeight;
        iframe.style.height = `${height + yOffsetPx}px`;
    });
}

export function isFeatureEnabled(feature: Feature, version: CommunicationTemplateVersion): boolean {
    switch (feature) {
        case "LAVA_FIELDS_FEATURE":
            return version === CommunicationTemplateVersion.Legacy;
        case "EMAIL_BUILDER_FEATURE":
            return version === CommunicationTemplateVersion.Beta;
        default:
            // If we don't recognize the feature, we assume it is not available.
            return false;
    }
}

export function useCommunicationTemplateMessageUtils(): CommunicationTemplateMessageUtils {
    function getLavaFieldsFromHtmlMessage(templateHtml: string): Record<string, string> {
        const lavaFieldsTemplateDictionary: Record<string, string> = {};

        // Extract the contents of the <noscript id="lava-fields">...</noscript> manually
        // since DOMParser will ignore <noscript> tag content when run in browsers.
        const lavaFieldsMatch = templateHtml.match(/<noscript[^>]*id=["']lava-fields["'][^>]*>([\s\S]*?)<\/noscript>/i);
        if (!lavaFieldsMatch) {
            return lavaFieldsTemplateDictionary;
        }

        const lavaFieldContent = lavaFieldsMatch[1];

        const templateDocLavaFieldLines = lavaFieldContent
            .split(/\r?\n/)
            .map(line => line.trim())
            .filter(line => line.length > 0);

        // Regex pattern to match {% assign key = value %}
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

    function updateLavaFieldsInMessage(templateHtml: string, updatedFields: Record<string, string>): string {
        // Remove existing lava-fields blocks (both <noscript> and legacy <div>)
        templateHtml = templateHtml.replace(/<noscript[^>]*id=["']lava-fields["'][\s\S]*?<\/noscript>\s*/gi, "");
        templateHtml = templateHtml.replace(/<div[^>]*id=["']lava-fields["'][\s\S]*?<\/div>\s*/gi, "");

        // Build the replacement dictionary based on updatedFields only
        const lavaFieldsBlock = buildLavaFieldsNoscriptBlock(updatedFields);

        if (!lavaFieldsBlock) {
            return templateHtml;
        }

        // Insert Lava block after <head>, or <html>, or top of the document
        const headRegex = /<head[^>]*>/i;
        const htmlRegex = /<html[^>]*>/i;

        if (headRegex.test(templateHtml)) {
            templateHtml = templateHtml.replace(headRegex, match => `${match}\n${lavaFieldsBlock}`);
        }
        else if (htmlRegex.test(templateHtml)) {
            templateHtml = templateHtml.replace(htmlRegex, match => `${match}\n${lavaFieldsBlock}`);
        }
        else {
            // Insert at top of document
            templateHtml = `${lavaFieldsBlock}\n${templateHtml.trimStart()}`;
        }

        return templateHtml;
    }

    function buildLavaFieldsNoscriptBlock(fields: Record<string, string>): string {
        const fieldKeyValuePairs = Object.entries(fields);

        if (!fieldKeyValuePairs.length) {
            return ""; // No fields to include, return empty string
        }

        const lines: string[] = [];

        lines.push('<noscript id="lava-fields">');
        lines.push("  {% comment %}  Lava Fields: Code-Generated from Template Editor {% endcomment %}");
        for (const [key, value] of fieldKeyValuePairs) {
            lines.push(`  {% assign ${key} = '${value}' %}`);
        }
        lines.push("</noscript>");
        return lines.join("\n"); // No trailing newline
    }

    function updateLogoInMessage(templateHtml: string, binaryFile: ListItemBag | null | undefined): string {
        // Regex to find the template logo's `img` tag
        const templateLogoRegex = /<img[^>]+id=['"]template-logo['"][^>]*src=['"]([^">]+)['"][^>]*>/;
        const templateLogoMatch = templateHtml.match(templateLogoRegex);

        if (!templateLogoMatch || templateLogoMatch.length !== 2) {
            return templateHtml;
        }

        const originalTemplateLogoHtml = templateLogoMatch[0];
        const originalTemplateLogoSrc = templateLogoMatch[1];

        const binaryFileGuid = toGuidOrNull(binaryFile?.value);
        const newTemplateLogoSrc = binaryFileGuid ? `/GetImage.ashx?guid=${binaryFileGuid}` : "/Content/EmailTemplates/placeholder-logo.png";
        const newTemplateLogoHtml = originalTemplateLogoHtml.replace(originalTemplateLogoSrc, newTemplateLogoSrc);

        return templateHtml.replace(originalTemplateLogoHtml, newTemplateLogoHtml);
    }

    function updateMessage(options: UpdateMessageOptions): UpdateMessageResult {
        const {
            lavaFields,
            lavaFieldValues,
            message,
            logoBinaryFile
        } = options;

        const updatedLavaFields = { ...lavaFields };
        const updatedLavaFieldValues = { ...lavaFieldValues };

        // First, remove blank keys.
        for (const key of Object.keys(updatedLavaFields)) {
            if (!key) {
                delete updatedLavaFields[key];
            }
        }

        // Remove old lava fields.
        for (const key of Object.keys(updatedLavaFieldValues)) {
            if (!(key in updatedLavaFields)) {
                delete updatedLavaFieldValues[key];
            }
        }

        // Add new lava fields.
        for (const [key, value] of Object.entries(updatedLavaFields)) {
            if (!(key in updatedLavaFieldValues)) {
                updatedLavaFieldValues[key] = value;
            }
        }

        // Update lava field values with values from the HTML message.
        for (const key of Object.keys(updatedLavaFields)) {
            updatedLavaFields[key] = updatedLavaFieldValues[key];
        }

        // Update the lava fields in the HTML message.
        let updatedMessage = updateLavaFieldsInMessage(message, updatedLavaFieldValues);
        // Update the logo in the HTML message.
        updatedMessage = updateLogoInMessage(updatedMessage, logoBinaryFile);

        return {
            lavaFields: updatedLavaFields,
            lavaFieldValues: updatedLavaFieldValues,
            message: updatedMessage
        };
    }

    function hasLogoInMessage(templateHtml: string): boolean {
        // Check if the template HTML contains an <img> tag with id="template-logo"
        const logoRegex = /<img[^>]+id=['"]template-logo['"][^>]*>/i;
        return logoRegex.test(templateHtml);
    }

    return {
        getLavaFieldsFromHtmlMessage,
        hasLogoInMessage,
        updateMessage
    };
}

export function useRecordUtils(): RecordUtils {
    function recordAsKeyValueItems(record: Record<string, string>): KeyValueItem[] {
        const items: KeyValueItem[] = [];

        for (const key in record) {
            if (!isNullish(key)) {
                items.push({
                    key,
                    value: record[key]
                });
            }
        }

        return items;
    }

    function keyValueItemsAsRecord(items: KeyValueItem[]): Record<string, string> {
        const record: Record<string, string> = {};

        for (const { key, value } of items) {
            if (!isNullish(key) && !isNullish(value)) {
                record[key] = value;
            }
        }

        return record;
    }

    function areRecordsEqual(a: Record<string, string>, b: Record<string, string>): boolean {
        const aKeys = Object.keys(a);
        const bKeys = Object.keys(b);

        // Quick length check
        if (aKeys.length !== bKeys.length) {
            return false;
        }

        // Check for matching keys and values
        for (const key of aKeys) {
            if (!(key in b) || a[key] !== b[key]) {
                return false;
            }
        }

        return true;
    }

    return {
        recordAsKeyValueItems,
        keyValueItemsAsRecord,
        areRecordsEqual
    };
}