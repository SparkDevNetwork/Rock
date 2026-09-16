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

/** Navigation URL keys the Room List block emits to the frontend. */
export const enum NavigationUrlKey {
    RosterPage = "RosterPage"
}

/**
 * Shape of the value the filter modal round-trips with the host block. The
 * selected schedules are Guid-valued ListItemBags (emitted by SchedulePicker);
 * the block resolves them to integer identifiers server-side when it writes
 * the shared CheckinManager cookie.
 */
export type GridSettingsOptions = {
    selectedSchedules: ListItemBag[];
};
