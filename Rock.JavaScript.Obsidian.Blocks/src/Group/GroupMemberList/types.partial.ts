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
    AddPage = "AddPage",
    DetailPage = "DetailPage",
    RegistrationPage = "RegistrationPage"
}

export const enum PreferenceKey {
    FilterCampus = "filter-campus",
    FilterGender = "filter-gender",
    FilterRegistrationInstance = "filter-registration-instance",
    FilterSignedDocument = "filter-signed-document"
}

/** The filter modal's filters, every one of which narrows the query. */
export type GridSettingsOptions = {
    /** The campus whose families the list is limited to. */
    campus?: ListItemBag;

    /** The genders the list is limited to, as `Gender` enum values. */
    genders?: string[];

    /** The unique identifier of the registration instance the list is limited to registrants of. */
    registrationInstance?: string | null;

    /** "Yes" limits the list to people who have signed, "No" to people who have not. */
    signedDocument?: string | null;
};
