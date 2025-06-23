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

import { SlidingDateRange } from "@Obsidian/Utility/slidingDateRange";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";

export const enum NavigationUrlKey {
    CommunicationDetailPage = "CommunicationDetailPage"
}

export type FilterSettings = {
    dateRange?: SlidingDateRange | null
    dataView?: ListItemBag
    connectionStatus?: ListItemBag | null
    medium?: string[]
    bulkOnly?: boolean
};

export const enum PreferenceKey {
    FilterDateRange = "FilterDateRange",
    FilterDataView = "FilterDataView",
    FilterConnectionStatus = "FilterConnectionStatus",
    FilterMedium = "FilterMedium",
    FilterBulkOnly = "FilterBulkOnly"
}