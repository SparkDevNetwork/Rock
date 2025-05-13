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
import { NumberRangeModelValue } from "@Obsidian/Types/Controls/numberRangeBox";

export const enum NavigationUrlKey {
    ViewPage = "ViewPage",
    AddPage = "AddPage"
}

export const enum PreferenceKey {
    FilterAccount = "filter-account",
    FilterDateRangeUpper = "filter-date-range-upper",
    FilterDateRangeLower = "filter-date-range-lower",
    FilterIncludeInctiveSchedules = "filter-include-inctive-schedules",
    FilterFrequency = "filter-frequency",
    FilterAmountRangeFrom = "filter-amount-range-from",
    FilterAmountRangeTo = "filter-amount-range-to"
}

export type Row = {
    accounts: AccountAmount[];
};

export type AccountAmount = {
    idKey: string;

    name: string;

    amount: number;
};

export type GridSettingsOptions = {
    dateRange?: string | null;

    account: ListItemBag;

    frequency?: ListItemBag | null;

    includeInactiveSchedules: boolean;

    startDate?: string | null;

    endDate?: string | null;

    amountRange?: NumberRangeModelValue | undefined;
};