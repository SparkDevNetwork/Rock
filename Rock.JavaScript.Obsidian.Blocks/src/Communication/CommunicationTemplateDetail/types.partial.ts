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

import { KeyValueItem } from "@Obsidian/Types/Controls/keyValueItem";
import { HttpResult } from "@Obsidian/Types/Utility/http";
import { CommunicationTemplateDetailCommunicationTemplateBag } from "@Obsidian/ViewModels/Blocks/Communication/CommunicationTemplateDetail/communicationTemplateDetailCommunicationTemplateBag";
import { CommunicationTemplateDetailGetPreviewMessageRequestBag } from "@Obsidian/ViewModels/Blocks/Communication/CommunicationTemplateDetail/communicationTemplateDetailGetPreviewMessageRequestBag";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";

export const enum NavigationUrlKey {
    ParentPage = "ParentPage"
}

export type CommunicationTemplateDetailBlockActionInvoker = {
    saveTemplate(bag: CommunicationTemplateDetailCommunicationTemplateBag): Promise<HttpResult<void>>;
    getPreviewMessage(bag: CommunicationTemplateDetailGetPreviewMessageRequestBag): Promise<HttpResult<string>>;
};

export type Feature = "EMAIL_BUILDER_FEATURE" | "LAVA_FIELDS_FEATURE";

export type UpdateMessageOptions = {
    lavaFields: Record<string, string>;
    lavaFieldValues: Record<string, string>;
    message: string;
    logoBinaryFile: ListItemBag | null | undefined;
};

export type UpdateMessageResult = {
    lavaFields: Record<string, string>;
    lavaFieldValues: Record<string, string>;
    message: string;
};

export type CommunicationTemplateMessageUtils = {
    getLavaFieldsFromHtmlMessage(templateHtml: string): Record<string, string>;
    hasLogoInMessage(templateHtml: string): boolean;
    updateMessage(options: UpdateMessageOptions): UpdateMessageResult;
};

export type RecordUtils = {
    recordAsKeyValueItems(record: Record<string, string>): KeyValueItem[];
    keyValueItemsAsRecord(items: KeyValueItem[]): Record<string, string>;
    areRecordsEqual(a: Record<string, string>, b: Record<string, string>): boolean;
};