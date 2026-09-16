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

export const enum NavigationUrlKey {
    // Per-config URLs:
    AreasAndGroupsPage = "AreasAndGroupsPage",
    ScheduleBuilderPage = "ScheduleBuilderPage",
    ConfigurationSettingsPage = "ConfigurationSettingsPage",

    // Related settings URLs:
    NamedLocationsPage = "NamedLocationsPage",
    SchedulesPage = "SchedulesPage",

    DevicesPage = "DevicesPage",
    LabelsPage = "LabelsPage",
    ClassicLabelsPage = "ClassicLabelsPage",
    CloudPrintPage = "CloudPrintPage",

    ClassicLabelMergeFields = "ClassicLabelMergeFields",
    AbilityLevels = "AbilityLevels",
    SearchType = "SearchType",

    // Public-Facing Docs URLs:
    CheckInManual = "CheckInManual",
}

export const enum PreferenceKey {
    SortBy = "sort-by"
}

export type SortItem = {
    value: string;
    text: string;
    iconCssClass: string;
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
