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
    DetailPage = "DetailPage"
}

export const enum PreferenceKey {
    FilterSite = "filter-site",
    FilterPage = "filter-page",
    FilterPerson = "filter-person",
    FilterExceptionTypeName = "filter-exception-type-name",
    FilterDateRange = "filter-date-range"
}

export type GridSettingsOptions = {
    site: string;
    page?: ListItemBag;
    person?: ListItemBag;
    exceptionTypeName: string;
    dateRange: SlidingDateRange | null;
};

export type Row = {
    idKey: string;
    lastExceptionDate?: string | null;
    exceptionTypeName?: string | null;
    description?: string | null;
    totalCount: number;
    subsetCount: number;
};

export type Breakpoint = "xs" | "sm" | "md" | "lg" | "xl" | "unknown";

export type BreakpointHelper = {
    breakpoint: Breakpoint;
    breakpoints: string;

    isXs: boolean;
    isSm: boolean;
    isMd: boolean;
    isLg: boolean;
    isXl: boolean;

    isXsOrSmaller: boolean;
    isSmOrSmaller: boolean;
    isMdOrSmaller: boolean;
    isLgOrSmaller: boolean;
    isXlOrSmaller: boolean;

    isXsOrLarger: boolean;
    isSmOrLarger: boolean;
    isMdOrLarger: boolean;
    isLgOrLarger: boolean;
    isXlOrLarger: boolean;
};
