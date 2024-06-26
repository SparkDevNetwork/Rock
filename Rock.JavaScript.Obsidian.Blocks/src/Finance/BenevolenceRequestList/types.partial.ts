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
    FilterStartDateUpperValue = "filter-start-date-upper-value",
    FilterStartDateLowerValue = "filter-start-date-lower-value",
    FilterCampus = "filter-campus",
    FilterFirstName = "filter-first-name",
    FilterLastName = "filter-last-name",
    FilterGovernmentId = "filter-government-id",
    FilterCaseWorker = "filter-case-worker",
    FilterResult = "filter-result",
    FilterRequestStatus = "filter-request-status",
    FilterBenevolenceTypes = "filter-benevolence-types"
}

export type GridSettingsOptions = {
    startDateUpperValue?: string | null;

    startDateLowerValue?: string | null;

    campus?: ListItemBag | null;

    firstName?: string | null;

    lastName?: string | null;

    governmentId?: string | null;

    caseWorker?: string | null;

    result?: ListItemBag | null;

    requestStatus?: ListItemBag | null;

    benevolenceTypes: string[];
};

export const enum ColumnKey {
    AssignedTo = "Assigned To",

    GovernmentId = "Government Id",

    TotalAmount = "Total Amount",

    TotalResults = "Total Results"
}
