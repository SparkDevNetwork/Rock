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

import { Guid } from "@Obsidian/Types";
import { HttpResult } from "@Obsidian/Types/Utility/http";
import { CommunicationEntryWizardSaveMetricsReminderRequestBag } from "@Obsidian/ViewModels/Blocks/Communication/CommunicationEntryWizard/communicationEntryWizardSaveMetricsReminderRequestBag";
import { ICancellationToken } from "@Obsidian/Utility/cancellation";
import { CommunicationEntryWizardCommunicationBag } from "@Obsidian/ViewModels/Blocks/Communication/CommunicationEntryWizard/communicationEntryWizardCommunicationBag";
import { CommunicationEntryWizardCommunicationTemplateDetailBag } from "@Obsidian/ViewModels/Blocks/Communication/CommunicationEntryWizard/communicationEntryWizardCommunicationTemplateDetailBag";
import { CommunicationEntryWizardSaveCommunicationTemplateResponseBag } from "@Obsidian/ViewModels/Blocks/Communication/CommunicationEntryWizard/communicationEntryWizardSaveCommunicationTemplateResponseBag";
import { CommunicationEntryWizardGetEmailPreviewHtmlBag } from "@Obsidian/ViewModels/Blocks/Communication/CommunicationEntryWizard/communicationEntryWizardGetEmailPreviewHtmlBag";
import { CommunicationEntryWizardRecipientBag } from "@Obsidian/ViewModels/Blocks/Communication/CommunicationEntryWizard/communicationEntryWizardRecipientBag";
import { CommunicationEntryWizardSaveResponseBag } from "@Obsidian/ViewModels/Blocks/Communication/CommunicationEntryWizard/communicationEntryWizardSaveResponseBag";
import { CommunicationEntryWizardSendResponseBag } from "@Obsidian/ViewModels/Blocks/Communication/CommunicationEntryWizard/communicationEntryWizardSendResponseBag";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";

export type SendTimePreference = "now" | "later";

export type Breakpoint = "xs" | "sm" | "md" | "lg" | "xl" | "unknown";

export type BreakpointHelper = {
    breakpoint: Breakpoint;

    isXs: boolean;
    isSm: boolean;
    isMd: boolean;
    isLg: boolean;
    isXl: boolean;

    isXsOrSmaller: boolean;
    isSmOrSmaller: boolean;
    isMdOrSmaller: boolean;
    isLgOrSmaller: boolean;
    isXlOrSmaller: boolean;

    isXsOrLarger: boolean;
    isSmOrLarger: boolean;
    isMdOrLarger: boolean;
    isLgOrLarger: boolean;
    isXlOrLarger: boolean;
};

export type BinaryFileAttachment = {
    fileName: string;
    binaryFileGuid: Guid;
    url: string;
};

export type BlockActionCallbacks = {
    onSuccess(): void;
    onError(error?: string | null | undefined): void;
};

export type PersonPreferencesHelper = {
    getCommunicationTemplateGuid(): Guid | null | undefined;
    setCommuncationTemplateGuid(value: Guid | null | undefined): Promise<void>;
};

export type InvokeBlockActionHelper = {
    cancelMetricsReminder(communicationGuid: Guid): Promise<HttpResult<void>>;
    getCommunicationTemplate(communicationTemplateGuid: Guid): Promise<HttpResult<CommunicationEntryWizardCommunicationTemplateDetailBag>>;
    getEmailPreviewHtml(bag: CommunicationEntryWizardCommunicationBag): Promise<HttpResult<CommunicationEntryWizardGetEmailPreviewHtmlBag | null | undefined>>;
    getRecipient(personAliasGuid: Guid): Promise<HttpResult<CommunicationEntryWizardRecipientBag>>;
    getRecipients(bag: CommunicationEntryWizardCommunicationBag, cancellationToken: ICancellationToken): Promise<HttpResult<CommunicationEntryWizardRecipientBag[]>>;
    getSegmentDataViews(communicationListGroupGuid: Guid | null | undefined): Promise<HttpResult<ListItemBag[]>>;
    sendTest(bag: CommunicationEntryWizardCommunicationBag): Promise<HttpResult<void>>;
    save(bag: CommunicationEntryWizardCommunicationBag): Promise<HttpResult<CommunicationEntryWizardSaveResponseBag>>;
    saveMetricsReminder(bag: CommunicationEntryWizardSaveMetricsReminderRequestBag): Promise<HttpResult<void>>;
    send(bag: CommunicationEntryWizardCommunicationBag): Promise<HttpResult<CommunicationEntryWizardSendResponseBag>>;
    subscribeToRealTime(request: {
        connectionId: string | null;
        communicationGuid: Guid;
    }): Promise<HttpResult<void>>;
    saveAsCommunicationTemplate(bag: CommunicationEntryWizardCommunicationTemplateDetailBag): Promise<HttpResult<CommunicationEntryWizardSaveCommunicationTemplateResponseBag>>;
    saveCommunicationTemplate(bag: CommunicationEntryWizardCommunicationTemplateDetailBag): Promise<HttpResult<CommunicationEntryWizardSaveCommunicationTemplateResponseBag>>;
};