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

import { inject, InjectionKey, provide, Ref } from "vue";
import { ComparisonValue } from "@Obsidian/Types/Reporting/comparisonValue";
import { GroupFinderInitializationBox } from "@Obsidian/ViewModels/Blocks/Group/GroupFinder/groupFinderInitializationBox";

/**
 * The shared filter state provided by the Group Finder block and consumed by the
 * filter-section partials, so the desktop bar and the mobile drawer bind to one
 * source of truth.
 */
export type GroupFinderFilterState = {
    config: GroupFinderInitializationBox;
    selectedCampuses: Ref<string[]>;
    selectedMeetingStyles: Ref<string[]>;
    selectedDaysOfWeek: Ref<string[]>;
    selectedTimeOfDay: Ref<string>;
    searchTerm: Ref<string>;
    origin: Ref<string>;
    /** Whether the origin is the visitor's current location (coordinates), so the Where filter shows a friendly label instead of the raw "lat,lng". */
    isCurrentLocation: Ref<boolean>;
    /** Whether the search is scoped to the map area the visitor chose via "Search this area", so the Where filter shows that label instead of a typed address. */
    isMapAreaLocation: Ref<boolean>;
    /** Enters current-location mode: prefers device geolocation and falls back to the server's best-guess location when the browser blocks or denies it. Sets the origin ready for the next Search. */
    useCurrentLocation: () => void;
    /** Featured attribute pill selections, keyed by attribute key. */
    featuredAttributeSelections: Ref<Record<string, string[]>>;
    /** More Filters modal comparison values, keyed by attribute key. */
    modalAttributeValues: Ref<Record<string, ComparisonValue>>;
};

const filterStateKey: InjectionKey<GroupFinderFilterState> = Symbol("group-finder-filters");

/** Provides the shared filter state to descendant filter-section partials. */
export function provideGroupFinderFilters(state: GroupFinderFilterState): void {
    provide(filterStateKey, state);
}

/** Gets the shared filter state provided by the Group Finder block. */
export function useGroupFinderFilters(): GroupFinderFilterState {
    const state = inject(filterStateKey);
    if (!state) {
        throw new Error("Group Finder filter state was not provided.");
    }
    return state;
}
