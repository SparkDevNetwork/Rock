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

export const enum NavigationUrlKey {
    DetailPage = "DetailPage"
}

export type GridSettingsOptions = {
    purpose?: ListItemBag | null;
    systemGroupType?: string | null;
    showInNavigation?: string | null;
};

export const enum PreferenceKey {
    FilterPurpose = "filter-purpose",
    FilterSystemGroupTypes = "filter-system-group-types",
    FilterShowInNavigation = "filter-show-in-navigation"
}
