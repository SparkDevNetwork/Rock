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

// export const enum NavigationUrlKey {
//     DetailPage = "DetailPage"
// }

export const enum PreferenceKey {
    PlacementConfigurationJSONRegistrationInstanceId = "PlacementConfigurationJSON_RegistrationInstanceId_{0}",
    PlacementConfigurationJSONRegistrationTemplateId = "PlacementConfigurationJSON_RegistrationTemplateId_{0}",
    SortOrderRegistrationInstanceId = "SortOrder_RegistrationInstanceId_{0}",
    SortOrderRegistrationTemplateId = "SortOrder_RegistrationTemplateId_{0}",
    IsGenderHighlightingRegistrationInstanceId = "IsGenderHighlighting_RegistrationInstanceId_{0}",
    IsGenderHighlightingRegistrationTemplateId = "IsGenderHighlighting_RegistrationTemplateId_{0}",
    PersonAttributeFilterRegistrationInstanceId = "PersonAttributeFilter_RegistrationInstanceId_{0}",
    PersonAttributeFilterRegistrationTemplateId = "PersonAttributeFilter_RegistrationTemplateId_{0}",
    GroupAttributeFilterGroupTypeId = "GroupAttributeFilter_GroupTypeId_{0}",
    GroupMemberAttributeFilterGroupTypeId = "GroupMemberAttributeFilter_GroupTypeId_{0}"
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