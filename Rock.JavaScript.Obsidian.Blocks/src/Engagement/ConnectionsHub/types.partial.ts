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

import { nextTick } from "vue";
import { tooltip } from "@Obsidian/Utility/tooltip";

export const enum PreferenceKey {
    ConnectionmOpportunityFilterConnectionTypeIdKey = "ConnectionOpportunityFilter_ConnectionTypeIdKey_{0}",
    SelectedGroupByMode = "SelectedGroupByMode",
    SelectedConnector = "SelectedConnector",
    AreOnlyMyRequestsVisible = "AreOnlyMyRequestsVisible",
    IsAutoCollapseEnabled = "IsAutoCollapseEnabled",
    FilterSortByConnectionTypIdKey = "FilterSortBy_ConnectionTypeIdKey_{0}",
    FilterIsAssignedToMeConnectionTypeIdKey = "FilterIsAssignedToMe_ConnectionTypeIdKey_{0}",
    FilterGridDataToShowConnectionTypIdKey = "FilterGridDataToShow_ConnectionTypeIdKey_{0}",
    FilterIsRequestSourceShownConnectionTypIdKey = "FilterIsRequestSourceShown_ConnectionTypeIdKey_{0}",
    FilterStateConnectionTypIdKey = "FilterState_ConnectionTypeIdKey_{0}",
    FilterStatusConnectionTypIdKey = "FilterStatus_ConnectionTypeIdKey_{0}",
    FilterDueConnectionTypIdKey = "FilterDue_ConnectionTypeIdKey_{0}",
}

export const enum NavigationUrlKey {
    PersonProfilePage = "PersonProfilePage",
    GroupDetailPage = "GroupDetailPage",
}

export type ViewOptions = {
    sortBy?: string | null,
    gridDataToShow?: string[] | null,
    isRequestSourceShown: boolean,
    stateFilter?: string[] | null,
    statusFilter?: string[] | null,
    dueFilter?: string[] | null
};

function isTextOverflowing(el: HTMLElement): boolean {
    return el.scrollWidth > el.clientWidth;
}

export function addTooltipIfOverflow(el: HTMLElement | null, tooltipText: string): void {
    if (!el) return;

    nextTick(() => {
        requestAnimationFrame(() => {
            el.removeAttribute("title");
            el.removeAttribute("data-original-title");
            el.removeAttribute("data-toggle");

            if (isTextOverflowing(el)) {
                el.setAttribute("title", tooltipText);
                el.setAttribute("data-original-title", tooltipText);
                el.setAttribute("data-toggle", "tooltip");
                tooltip(el);
            }
        });
    });
}