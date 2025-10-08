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

import { CommunicationFlowTriggerType } from "@Obsidian/Enums/Communication/communicationFlowTriggerType";
import { DeleteText, DeleteTextArgs, TemplateArgs } from "./types.partial";
import { nextTick, onBeforeUnmount, Ref, watch } from "vue";
import { isNullish } from "@Obsidian/Utility/util";

export function isEnumValue<T extends Record<string, number | string>>(enumObject: T, value: unknown): value is T[keyof T] {
    return Object.values(enumObject).includes(value as T[keyof T]);
}

/**
 * Returns a string like "an apple" or "a banana" based on the initial sound of the word.
 * Handles common English edge cases, including silent 'h' and hard 'u' sounds.
 */
export function withIndefiniteArticle(word: string): string {
    if (!word) return word;

    const lower = word.toLowerCase();

    // Common silent "h" words
    const silentH = ["honest", "hour", "honor", "heir"];
    if (silentH.includes(lower)) {
        return `an ${word}`;
    }

    // Words starting with hard "u" like "unicorn", "university"
    const hardURegex = /^(u[bcfhjkqrstn])/i; // "ubiquitous", "unicorn", etc.
    if (hardURegex.test(word)) {
        return `a ${word}`;
    }

    // Words starting with vowel sounds
    const vowelSound = /^[aeiou]/i;
    if (vowelSound.test(word)) {
        return `an ${word}`;
    }

    return `a ${word}`;
}

export function getPreheaderText(html: string | null | undefined): string | null | undefined {
    if (isNullish(html)) {
        return html;
    }

    const parser = new DOMParser();
    const doc = parser.parseFromString(html, "text/html");
    return getPreheaderTextFromDoc(doc);
}

export function getPreheaderTextFromDoc(doc: Document): string | null {
    const preheader = doc.querySelector("#preheader-text");
    return preheader?.textContent?.trim() ?? null;
}

function formatMessage<T extends string>(template: T, args: TemplateArgs<T>): string {
    return template.replace(/{(\w+\??)}/g, (_, rawKey) => {
        const key = rawKey.replace(/\?$/, "");
        return args[key as keyof typeof args] ?? "";
    });
}

/**
 * Formats a delete confirmation message using the template {@link DeleteText}.
 * @param args - Template arguments.
 * @param args.typeName - The type of item being deleted.
 * @param args.extra - Additional information to include in the message.
 */
export function formatDeleteConfirmation(args: DeleteTextArgs): string {
    return formatMessage(DeleteText, args);
}

export function triggerTypeToText(triggerType: CommunicationFlowTriggerType): string {
    switch (triggerType) {
        case CommunicationFlowTriggerType.OneTime:
            return "One-Time";
        case CommunicationFlowTriggerType.Recurring:
            return "Recurring";
        case CommunicationFlowTriggerType.OnDemand:
            return "On-Demand";
        default:
            return "Unknown";
    }
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