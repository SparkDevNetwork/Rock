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

import { ComputedRef, inject, InjectionKey, provide } from "vue";
import { BreakpointHelper, Cache, CacheOptions, InvokeBlockActionHelper, PersonPreferencesHelper } from "./types.partial";
import { Guid } from "@Obsidian/Types";
import { HttpResult } from "@Obsidian/Types/Utility/http";
import { useInvokeBlockAction, usePersonPreferences } from "@Obsidian/Utility/block";
import { ICancellationToken } from "@Obsidian/Utility/cancellation";
import { toGuidOrNull } from "@Obsidian/Utility/guid";
import { CommunicationEntryWizardCheckShortLinkTokenBag } from "@Obsidian/ViewModels/Blocks/Communication/CommunicationEntryWizard/communicationEntryWizardCheckShortLinkTokenBag";
import { CommunicationEntryWizardCommunicationBag } from "@Obsidian/ViewModels/Blocks/Communication/CommunicationEntryWizard/communicationEntryWizardCommunicationBag";
import { CommunicationEntryWizardCommunicationTemplateDetailBag } from "@Obsidian/ViewModels/Blocks/Communication/CommunicationEntryWizard/communicationEntryWizardCommunicationTemplateDetailBag";
import { CommunicationEntryWizardGetPreviewBag } from "@Obsidian/ViewModels/Blocks/Communication/CommunicationEntryWizard/communicationEntryWizardGetPreviewBag";
import { CommunicationEntryWizardRecipientBag } from "@Obsidian/ViewModels/Blocks/Communication/CommunicationEntryWizard/communicationEntryWizardRecipientBag";
import { CommunicationEntryWizardSaveCommunicationTemplateResponseBag } from "@Obsidian/ViewModels/Blocks/Communication/CommunicationEntryWizard/communicationEntryWizardSaveCommunicationTemplateResponseBag";
import { CommunicationEntryWizardSaveMetricsReminderRequestBag } from "@Obsidian/ViewModels/Blocks/Communication/CommunicationEntryWizard/communicationEntryWizardSaveMetricsReminderRequestBag";
import { CommunicationEntryWizardSaveResponseBag } from "@Obsidian/ViewModels/Blocks/Communication/CommunicationEntryWizard/communicationEntryWizardSaveResponseBag";
import { CommunicationEntryWizardSendResponseBag } from "@Obsidian/ViewModels/Blocks/Communication/CommunicationEntryWizard/communicationEntryWizardSendResponseBag";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";

const breakpointHelperInjectionKey: InjectionKey<ComputedRef<BreakpointHelper>> = Symbol("breakpoint-helper");

/**
 * Sets the readonly, reactive breakpoint helper.
 *
 * It can be injected as a dependency into child components with `useBreakpointHelper()`.
 */
export function provideBreakpointHelper(value: ComputedRef<BreakpointHelper>): void {
    provide(breakpointHelperInjectionKey, value);
}

/**
 * Injects a provided value.
 * Throws an exception if the value is undefined or not yet provided.
 */
function use<T>(key: string | InjectionKey<T>): T {
    const result = inject<T>(key);

    if (result === undefined) {
        throw `Attempted to access ${key.toString()} before a value was provided.`;
    }

    return result;
}

/**
 * Gets the breakpoint helper that can be used to provide responsive behavior.
 */
export function useBreakpointHelper(): ComputedRef<BreakpointHelper> {
    return use(breakpointHelperInjectionKey);
}

export function usePersonPreferencesHelper(): PersonPreferencesHelper {
    const personPreferences = usePersonPreferences().blockPreferences;
    const personPreferenceKey = {
        communicationTemplateGuid: "CommunicationTemplateGuid"
    } as const;

    return {
        getCommunicationTemplateGuid(): Guid | null | undefined {
            return toGuidOrNull(personPreferences.getValue(personPreferenceKey.communicationTemplateGuid));
        },
        async setCommuncationTemplateGuid(value: Guid | null | undefined): Promise<void> {
            personPreferences.setValue(personPreferenceKey.communicationTemplateGuid, value ?? "");
            await personPreferences.save();
        }
    };
}

export function useInvokeBlockActionHelper(): InvokeBlockActionHelper {
    const invokeBlockAction = useInvokeBlockAction();

    return {
        cancelMetricsReminder(communicationGuid: Guid): Promise<HttpResult<void>> {
            return invokeBlockAction("CancelMetricsReminder", {
                communicationGuid: communicationGuid
            });
        },

        checkShortLinkToken(bag: CommunicationEntryWizardCheckShortLinkTokenBag): Promise<HttpResult<string>> {
            return invokeBlockAction<string>("CheckShortLinkToken", { bag });
        },

        getShortLinkPageId(pageGuid: Guid): Promise<HttpResult<number | null | undefined>> {
            return invokeBlockAction<number | null | undefined>("GetShortLinkPageId", { pageGuid });
        },

        saveMetricsReminder(bag: CommunicationEntryWizardSaveMetricsReminderRequestBag): Promise<HttpResult<void>> {
            return invokeBlockAction("SaveMetricsReminder", { bag });
        },

        getCommunicationTemplate(communicationTemplateGuid: Guid): Promise<HttpResult<CommunicationEntryWizardCommunicationTemplateDetailBag>> {
            return invokeBlockAction<CommunicationEntryWizardCommunicationTemplateDetailBag>(
                "GetCommunicationTemplate",
                {
                    communicationTemplateGuid
                }
            );
        },

        getEmailPreviewHtml(bag: CommunicationEntryWizardCommunicationBag, previewAsPersonAliasGuid?: Guid | null | undefined, previewAsPersonalizationSegmentId?: number | null | undefined): Promise<HttpResult<CommunicationEntryWizardGetPreviewBag | null | undefined>> {
            return invokeBlockAction<CommunicationEntryWizardGetPreviewBag | null | undefined>(
                "GetEmailPreviewHtml",
                {
                    bag,
                    previewAsPersonAliasGuid,
                    previewAsPersonalizationSegmentId
                });
        },

        getPushPreview(bag: CommunicationEntryWizardCommunicationBag, previewAsPersonAliasGuid?: Guid | null | undefined, previewAsPersonalizationSegmentId?: number | null | undefined): Promise<HttpResult<CommunicationEntryWizardGetPreviewBag | null | undefined>> {
            return invokeBlockAction<CommunicationEntryWizardGetPreviewBag | null | undefined>(
                "GetPushPreview",
                {
                    bag,
                    previewAsPersonAliasGuid,
                    previewAsPersonalizationSegmentId
                });
        },

        getSmsPreview(bag: CommunicationEntryWizardCommunicationBag, previewAsPersonAliasGuid?: Guid | null | undefined, previewAsPersonalizationSegmentId?: number | null | undefined): Promise<HttpResult<CommunicationEntryWizardGetPreviewBag | null | undefined>> {
            return invokeBlockAction<CommunicationEntryWizardGetPreviewBag | null | undefined>(
                "GetSmsPreview",
                {
                    bag,
                    previewAsPersonAliasGuid,
                    previewAsPersonalizationSegmentId
                });
        },

        getRecipient(personAliasGuid: Guid): Promise<HttpResult<CommunicationEntryWizardRecipientBag>> {
            return invokeBlockAction<CommunicationEntryWizardRecipientBag>("GetRecipient", { personAliasGuid });
        },

        getRecipients(bag: CommunicationEntryWizardCommunicationBag, cancellationToken: ICancellationToken) {
            return invokeBlockAction<CommunicationEntryWizardRecipientBag[]>(
                "GetRecipients",
                { bag },
                undefined,
                cancellationToken
            );
        },

        getSegmentDataViews(communicationListGroupGuid: Guid | null | undefined): Promise<HttpResult<ListItemBag[]>> {
            return invokeBlockAction<ListItemBag[]>(
                "GetSegmentDataViews",
                { communicationListGroupGuid }
            );
        },

        subscribeToRealTime(request: {
            connectionId: string | null,
            communicationGuid: Guid
        }): Promise<HttpResult<void>> {
            return invokeBlockAction("SubscribeToRealTime", request);
        },

        sendTest(bag: CommunicationEntryWizardCommunicationBag): Promise<HttpResult<void>> {
            return invokeBlockAction("SendTest", { bag });
        },

        save(bag: CommunicationEntryWizardCommunicationBag): Promise<HttpResult<CommunicationEntryWizardSaveResponseBag>> {
            return invokeBlockAction<CommunicationEntryWizardSaveResponseBag>("Save", { bag });
        },

        send(bag: CommunicationEntryWizardCommunicationBag) {
            return invokeBlockAction<CommunicationEntryWizardSendResponseBag>("Send", { bag });
        },

        saveAsCommunicationTemplate(bag: CommunicationEntryWizardCommunicationTemplateDetailBag) {
            return invokeBlockAction<CommunicationEntryWizardSaveCommunicationTemplateResponseBag>("SaveAsCommunicationTemplate", { bag });
        },

        saveCommunicationTemplate(bag: CommunicationEntryWizardCommunicationTemplateDetailBag) {
            return invokeBlockAction<CommunicationEntryWizardSaveCommunicationTemplateResponseBag>("SaveCommunicationTemplate", { bag });
        }
    };
}

export function get<T>(value: T): T {
    return value;
}

export function useCache<T>({ maxSize }: CacheOptions): Cache<T> {
    const cache = new Map<string, T>();
    const usageQueue = new Set<string>();

    function updateUsage(key: string): void {
        usageQueue.delete(key);
        usageQueue.add(key);
    }

    function has(key: string): boolean {
        return cache.has(key);
    }

    function get(key: string): T | undefined {
        if (!cache.has(key)) {
            return;
        }

        // Ensure this key is the most recently used.
        updateUsage(key);
        return cache.get(key);
    }

    function set(key: string, value: T): void {
        if (cache.size >= maxSize && !cache.has(key)) {
            const oldestKey = usageQueue.values().next().value;

            if (oldestKey !== undefined) {
                cache.delete(oldestKey);
                usageQueue.delete(oldestKey);
            }
        }

        cache.set(key, value);
        updateUsage(key);
    }

    function remove(key: string): void {
        cache.delete(key);
        usageQueue.delete(key);
    }

    function clear(): void {
        cache.clear();
        usageQueue.clear();
    }

    return {
        has,
        get,
        set,
        remove,
        clear
    };
}