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

import { Guid } from "@Obsidian/Types";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { nextTick } from "vue";
import { tooltip } from "@Obsidian/Utility/tooltip";

export const enum PreferenceKey {
    PlacementConfigurationJSONRegistrationInstanceIdKey = "PlacementConfigurationJSON_RegistrationInstanceIdKey_{0}",
    PlacementConfigurationJSONRegistrationTemplateIdKey = "PlacementConfigurationJSON_RegistrationTemplateIdKey_{0}",
    PlacementConfigurationJSONSourceGroupIdKey = "PlacementConfigurationJSON_SourceGroupIdKey_{0}",
    PlacementConfigurationJSONEntitySetIdKey = "PlacementConfigurationJSON_EntitySetIdKey_{0}",
    PersonAttributeFilterRegistrationInstanceIdKey = "PersonAttributeFilter_RegistrationInstanceIdKey_{0}",
    PersonAttributeFilterRegistrationTemplateIdKey = "PersonAttributeFilter_RegistrationTemplateIdKey_{0}",
    PersonAttributeFilterSourceGroupIdKey = "PersonAttributeFilter_SourceGroupIdKey_{0}",
    GroupAttributeFilterGroupTypeIdKey = "GroupAttributeFilter_GroupTypeIdKey_{0}",
    GroupMemberAttributeFilterGroupTypeIdKey = "GroupMemberAttributeFilter_GroupTypeIdKey_{0}",
    RegistrantFeeItemValuesForFiltersJSONRegistrationInstanceIdKey = "RegistrantFeeItemValuesForFiltersJSON_RegistrationInstanceIdKey_{0}",
    RegistrantFeeItemValuesForFiltersJSONRegistrationTemplateIdKey = "RegistrantFeeItemValuesForFiltersJSON_RegistrationTemplateIdKey_{0}",
    SortOrderRegistrationInstanceIdKey = "SortOrder_RegistrationInstanceIdKey_{0}",
    SortOrderRegistrationTemplateIdKey = "SortOrder_RegistrationTemplateIdKey_{0}",
    SortOrderSourceGroupIdKey = "SortOrder_SourceGroupIdKey_{0}",
    SortOrderEntitySetIdKey = "SortOrder_EntitySetIdKey_{0}",
    IsGenderHighlightingRegistrationInstanceIdKey = "IsGenderHighlighting_RegistrationInstanceIdKey_{0}",
    IsGenderHighlightingRegistrationTemplateIdKey = "IsGenderHighlighting_RegistrationTemplateIdKey_{0}",
    IsGenderHighlightingSourceGroupIdKey = "IsGenderHighlighting_SourceGroupIdKey_{0}",
    IsGenderHighlightingEntitySetIdKey = "IsGenderHighlighting_EntitySetIdKey_{0}",
    FallbackRegistrationTemplatePlacement = "FallbackRegistrationTemplatePlacement",
}

export const enum NavigationUrlKey {
    GroupDetailPage = "GroupDetailPage",
    GroupMemberDetailPage = "GroupMemberDetailPage",
}

export type AvailablePlacementAttributes = {
    registrantAttributes: ListItemBag[];
    groupAttributes: ListItemBag[];
    groupMemberAttributes: ListItemBag[];
};

export enum SortTypes {
    SortByLastName = 0,
    SortByFirstName = 1,
    SortByDateAddedAsc = 2,
    SortByDateAddedDesc = 3
}

function isTextOverflowing(el: HTMLElement): boolean {
    return el.scrollWidth > el.clientWidth;
}

export function addTooltipIfOverflow(el: HTMLElement | null, tooltipText: string): void {
    if (!el) return;

    nextTick(() => {
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
}