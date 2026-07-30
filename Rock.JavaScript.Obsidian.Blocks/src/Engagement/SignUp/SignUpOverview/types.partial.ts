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
    ProjectDetailPage = "ProjectDetailPage",
    SignUpOpportunityAttendeeListPage = "SignUpOpportunityAttendeeListPage"
}

export const enum PreferenceKey {
    FilterDateRange = "filter-date-range",
    FilterParentGroup = "filter-parent-group",
    FilterSlotsAvailableComparisonType = "filter-slots-available-comparison-type",
    FilterSlotsAvailableComparisonValue = "filter-slots-available-comparison-value"
}

export type GridSettingsOptions = {
    /** The schedule date range to limit the opportunities to. */
    dateRange?: SlidingDateRange | null;

    /** The project group to limit the opportunities to. */
    parentGroup?: ListItemBag | null;

    /** The comparison type (as an enum number string) of the slots available filter. */
    slotsAvailableComparisonType?: string | null;

    /** The value the slots available filter compares against. */
    slotsAvailableComparisonValue?: number | null;
};
