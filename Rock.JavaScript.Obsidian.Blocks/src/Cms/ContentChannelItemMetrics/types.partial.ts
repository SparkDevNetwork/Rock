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

/** The block person preference keys. */
export const enum PreferenceKey {
    /** The sliding date range, shared between the Overview panel and the Viewer Details grid. */
    DateRange = "DateRange",

    /** The Viewer Details campus filter, stored as a campus guid. */
    FilterCampus = "filter-campus",

    /** The Viewer Details connection status filter, stored as a defined value guid. */
    FilterConnectionStatus = "filter-connection-status",

    /** The Viewer Details original source filter, stored as the source label. */
    FilterSource = "filter-source"
}

/** The Viewer Details grid settings, edited in the grid settings modal. */
export type GridSettingsOptions = {
    /** The sliding date range, shared with the Overview panel. */
    dateRange?: SlidingDateRange | null;

    /** The selected campus guid, or empty for all campuses. */
    campus?: string;

    /** The selected connection status defined value guid, or empty for all. */
    connectionStatus?: string;

    /** The selected source label, or empty for all sources. */
    source?: string;
};
