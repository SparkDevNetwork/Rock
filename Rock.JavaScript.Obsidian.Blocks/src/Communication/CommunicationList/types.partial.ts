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

import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import type { DateRangeParts } from "@Obsidian/Types/Controls/dateRangePicker";
import { NumberRangeModelValue } from "@Obsidian/Types/Controls/numberRangeBox";

export const enum NavigationUrlKey {
    DetailPage = "DetailPage",
    RootUrl = "RootUrl"
}

export const enum PreferenceKey {
    FilterSubject = "filter-subject",
    FilterCommunicationType = "filter-communication-type",
    FilterStatus = "filter-status",
    FilterCreatedBy = "filter-created-by",
    FilterCreatedDateRangeFrom = "filter-created-date-from",
    FilterCreatedDateRangeTo = "filter-created-date-to",
    FilterSentDateRangeFrom = "filter-sent-date-range-from",
    FilterSentDateRangeTo = "filter-sent-date-range-to",
    FilterContent = "filter-content",
    FilterRecipientCountRangeFrom = "filter-recipient-count-from",
    FilterRecipientCountRangeTo = "filter-recipient-count-to",
}

export type GridSettingsOptions = {
    subject?: string | null;
    communicationType?: string | null;
    status?: string | null;
    createdBy?: ListItemBag | undefined;
    createdDateRange?: DateRangeParts | null;
    sentDateRange?: DateRangeParts | null;
    content?: string | null;
    recipientCount?: NumberRangeModelValue | undefined;
};
