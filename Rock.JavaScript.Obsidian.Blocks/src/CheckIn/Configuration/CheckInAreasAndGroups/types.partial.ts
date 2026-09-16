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

export const enum PreferenceKey {
    DisableAutoCollapse = "disable-auto-collapse",

    /**
     * The selected-area key. Scoped to the check-in configuration GroupType
     * entity (via getEntityPreferences) and shared with other check-in config
     * blocks; not block-scoped like the others here.
     */
    SelectedArea = "checkin-config-selected-area",
    ShowInactive = "show-inactive"
}

export const enum NavigationUrlKey {
    CreateCheckInLabel = "CreateCheckInLabel",
    CreateClassicCheckInLabel = "CreateClassicCheckInLabel"
}

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
