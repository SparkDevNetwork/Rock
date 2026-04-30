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

import { GroupLocationsBag } from "@Obsidian/ViewModels/Blocks/CheckIn/Configuration/CheckInScheduleBuilder/groupLocationsBag";

export const enum PreferenceKey {
    SelectedGroupType = "selected-group-type",
    SelectedArea = "selected-area",
    SelectedCategory = "selected-category",
    SelectedParentLocation = "selected-parent-location"
}

export const enum NavigationUrlKey {
    ParentPage = "ParentPage"
}

/** The grouped Group Locations Bag, grouped by area and group */
export type GroupedGroupLocationsBag = {
    /** The path to the area that contains the group. */
    areaPath?: string | null;

    /** The encrypted identifier of the group location to be modified. */
    groupLocationId?: string | null;

    /**
     * The path to the group that should be scheduled. This includes
     * any parent groups in the text.
     */
    groupPath?: string | null;

    /** Nested group locations under this area and group. */
    groupLocations: GroupLocationsBag[];
};
