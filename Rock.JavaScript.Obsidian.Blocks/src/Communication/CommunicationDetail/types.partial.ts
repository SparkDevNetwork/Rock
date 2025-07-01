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

import { CommunicationRecipientStatus } from "@Obsidian/Enums/Communication/communicationRecipientStatus";
import { CommunicationType } from "@Obsidian/Enums/Communication/communicationType";
import { AgeClassification } from "@Obsidian/Enums/Crm/ageClassification";
import { Gender } from "@Obsidian/Enums/Crm/gender";
import { CommunicationRecipientGridSettingsBag } from "@Obsidian/ViewModels/Blocks/Communication/CommunicationDetail/communicationRecipientGridSettingsBag";
import { PersonFieldBag } from "@Obsidian/ViewModels/Core/Grid/personFieldBag";

export const enum TabItem {
    Analytics = "Analytics",
    MessageDetails = "Message Details",
    RecipientDetails = "Recipient Details"
}

export const enum PerformanceChartItem {
    Time = "Time",
    Flow = "Flow"
}

export const enum PerformanceChartTimeframe {
    First45Days = "First 45 Days",
    AllTime = "All Time"
}

export type ChartStyles = {
    fontFamily: string;
    fontColor: string;
    fontSize: number;
    fontWeight: string;
    legendBoxSize: number;
    fallbackColor: string;
};

export const enum PageParameterKey {
    Tab = "Tab"
}

export const enum PreferenceKey {
    RecipientListSettings = "RecipientListSettings"
}

export type GridSettingsOptions = {
    recipientListSettings: CommunicationRecipientGridSettingsBag;
};

export const enum PersonPropertyName {
    Age = "Age",
    AgeClassification = "AgeClassification",
    BirthDate = "BirthDate",
    Campus = "Campus",
    Email = "Email",
    Gender = "Gender",
    Grade = "Grade",
    IsDeceased = "IsDeceased"
}

export type RecipientGridRow = {
    personIdKey: string;
    communicationRecipientIdKey: string;
    person?: PersonFieldBag | null;
    lastActivityDateTime?: string | null;
    status: CommunicationRecipientStatus;
    statusNote?: string | null;
    sendDatetime?: string | null;
    deliveredDateTime?: string | null;
    medium?: CommunicationType | null;
    opensCount?: number | null;
    lastOpenedDateTime?: string | null;
    clicksCount?: number | null;
    lastClickedDateTime?: string | null;
    unsubscribeDateTime?: string | null;
    spamComplaintDateTime?: string | null;

    age?: number | null;
    ageClassification?: AgeClassification | null;
    birthdate?: string | null;
    campus?: string | null;
    email?: string | null;
    gender?: Gender | null;
    grade?: string | null;
    isDeceased?: boolean | null;
};
