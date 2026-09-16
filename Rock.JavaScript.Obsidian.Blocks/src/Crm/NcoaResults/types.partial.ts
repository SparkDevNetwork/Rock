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

export const enum PreferenceKey {
    FilterProcessed = "filter-processed",
    FilterMoveDate = "filter-move-date",
    FilterNcoaProcessedDate = "filter-ncoa-processed-date",
    FilterMoveType = "filter-move-type",
    FilterAddressStatus = "filter-address-status",
    FilterInvalidReason = "filter-invalid-reason",
    FilterMoveDistance = "filter-move-distance",
    FilterLastName = "filter-last-name",
    FilterCampus = "filter-campus"
}

export type GridSettingsOptions = {
    filterProcessed: string;
    filterMoveDate: SlidingDateRange | null;
    filterNcoaProcessedDate: SlidingDateRange | null;
    filterMoveType: string;
    filterAddressStatus: string;
    filterInvalidReason: string;
    filterMoveDistance: number | null;
    filterLastName: string;
    filterCampus?: ListItemBag | null;
};
