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

export const enum PreferenceKey {
    FilterAccountName = "filter-account-name",
    FilterCampus = "filter-campus",
    FilterIsPublic = "filter-is-public",
    FilterIsActive = "filter-is-active",
    FilterIsTaxDeductible = "filter-is-tax-deductible"
}

export type GridSettingsOptions = {
    name?: string | null;
    campus?: ListItemBag | null;
    isPublic?: string | null;
    isActive?: string | null;
    isTaxDeductible?: string | null;
};