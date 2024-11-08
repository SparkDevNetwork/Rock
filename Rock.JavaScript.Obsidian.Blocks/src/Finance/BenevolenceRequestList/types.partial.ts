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
    DetailPage = "DetailPage",
    ConfigurationPage = "ConfigurationPage"
}

export const enum PreferenceKey {
    FilterFirstName = "filter-first-name",
    FilterLastName = "filter-last-name",
    FilterResult = "filter-result",
    FilterBenevolenceTypes = "filter-benevolence-types"
}

export type GridSettingsOptions = {

    firstName?: string | null;

    lastName?: string | null;

    result?: ListItemBag | null;

    benevolenceTypes: string[];
};

export const enum ColumnKey {
    AssignedTo = "Assigned To",

    GovernmentId = "Government Id",

    TotalAmount = "Total Amount",

    TotalResults = "Total Results",

    Campus = "Campus"
}
