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

export const enum PreferenceKey {
    FilterUsername = "filter-username",

    FilterAuthenticationProvider = "filter-authentication-provider",

    FilterDateCreatedUpperValue = "filter-date-created-upper-value",

    FilterDateCreatedLowerValue=  "filter-date-created-lower-value",

    FilterLastLoginDateUpperValue = "filter-last-login-date-upper-value",

    FilterLastLoginDateLowerValue = "filter-last-login-date-lower-value",

    FilterIsConfirmed = "filter-is-confirmed",

    FilterIsLockedOut = "filter-is-locked-out"
}

export type GridSettingsOptions = {
    username?: string | null;

    authenticationProvider?: ListItemBag | null;

    dateCreatedUpperValue?: string | null;

    dateCreatedLowerValue?: string | null;

    lastLoginDateUpperValue?: string | null;

    lastLoginDateLowerValue?: string | null;

    isConfirmed?: string | null;

    isLockedOut?: string | null;
};