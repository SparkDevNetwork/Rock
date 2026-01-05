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

import { InferredCommunicationStatus } from "@Obsidian/Enums/Communication/inferredCommunicationStatus";
import { NumberRangeModelValue } from "@Obsidian/Types/Controls/numberRangeBox";
import { SlidingDateRange } from "@Obsidian/Utility/slidingDateRange";
import { PersonFieldBag } from "@Obsidian/ViewModels/Core/Grid/personFieldBag";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";

export const enum NavigationUrlKey {
    DetailPage = "DetailPage",
    LegacyDetailPage = "LegacyDetailPage",
}

export const enum PreferenceKey {
    FilterSentBy = "filter-sent-by",
    FilterCommunicationTypes = "filter-communication-types",
    FilterHideDrafts = "filter-hide-drafts",
    FilterSendDateRange = "filter-send-date-range",
    FilterRecipientCountLower = "filter-recipient-count-lower",
    FilterRecipientCountUpper = "filter-recipient-count-upper",
    FilterTopic = "filter-topic",
    FilterName = "filter-name",
    FilterContent = "filter-content"
}

export type GridSettingsOptions = {
    sentBy?: ListItemBag;
    hideDrafts: boolean;
    slidingDateRange: SlidingDateRange | null;
    recipientCountRange?: NumberRangeModelValue;
    topic: string;
    name: string;
    content: string;
};

export type Row = {
    idKey: string;
    type: number;
    name?: string | null;
    summary?: string | null;
    inferredStatus: InferredCommunicationStatus;
    recipientCount: number;
    deliveredCount: number;
    openedCount: number;
    failedCount: number;
    unsubscribedCount: number;
    topic?: string | null;
    createdDateTime?: string | null;
    sendDateTime?: string | null;
    futureSendDateTime?: string | null;
    sentByPersonFullName?: string | null;
    sentByPerson?: PersonFieldBag | null;
    reviewedDateTime?: string | null;
    reviewedByPersonFullName?: string | null;
    isDeleteDisabled: boolean;
    isLegacyCommunication: boolean;
};
