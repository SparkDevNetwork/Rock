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

import { CommunicationType } from "@Obsidian/Enums/Communication/communicationType";
import { Guid } from "@Obsidian/Types";
import { CommunicationFlowCommunicationBag } from "@Obsidian/ViewModels/Blocks/Communication/CommunicationFlowDetail/communicationFlowCommunicationBag";
import { CommunicationFlowDetailCommunicationTemplateBag } from "@Obsidian/ViewModels/Blocks/Communication/CommunicationFlowDetail/communicationFlowDetailCommunicationTemplateBag";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";

export const enum NavigationUrlKey {
    ParentPage = "ParentPage"
}

export type WizardItem = {
    value: WizardItemValue;
    text: string;
    iconCssClass: string;
};

export type WizardItemValue = "gettingStarted" | "conversionGoal" | "messageFlow";

export type TileListItemBag = ListItemBag & {
    iconCssClass?: string | null | undefined;
    subtext?: string | null | undefined;
};

export type BinaryFileAttachment = {
    fileName: string;
    binaryFileGuid: Guid;
    url: string;
};

type ExtractPlaceholders<T extends string, Optionals extends string = never> =
    T extends `${infer _Start}{${infer Param}}${infer Rest}`
    ? Param extends `${infer Key}?`
    ? ExtractPlaceholders<Rest, Optionals | Key> | Key
    : ExtractPlaceholders<Rest, Optionals> | Param
    : never;

type ExtractOptionalKeys<T extends string> =
    T extends `${infer _Start}{${infer Param}}${infer Rest}`
    ? Param extends `${infer Key}?`
    ? Key | ExtractOptionalKeys<Rest>
    : ExtractOptionalKeys<Rest>
    : never;

type ExtractRequiredKeys<T extends string> =
    Exclude<ExtractPlaceholders<T>, ExtractOptionalKeys<T>>;

export type TemplateArgs<T extends string> = {
    [K in ExtractRequiredKeys<T>]: string;
} & {
    [K in ExtractOptionalKeys<T>]?: string;
};

export const DeleteText = "Are you sure you want to delete this {typeName}? {additionalMessage?}" as const;

export type DeleteTextArgs = TemplateArgs<typeof DeleteText>;

export type SendTestOptions = BlockActionCallbacks & {
    communicationTemplate: CommunicationFlowDetailCommunicationTemplateBag;
    communicationType: CommunicationType;
    communication: CommunicationFlowCommunicationBag | null | undefined;
    testEmailAddress?: string | null | undefined;
    testSmsPhoneNumber?: string | null | undefined;
};

export type BlockActionCallbacks = {
    onSuccess?: (() => void) | undefined;
    onError?: ((error?: string | null | undefined) => void) | undefined;
};