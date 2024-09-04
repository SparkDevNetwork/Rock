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
    DetailPage = "DetailPage"
}

export const enum PreferenceKey {
    FilterBusinessName = "filter-business-name",

    FilterActive = "filter-active",
}

export type GridSettingsOptions = {
    businessName?: string | null;

    active?: string | null;
};

export type Row = {
    id: number;
    businessName: string;
    phoneNumber: string;
    email: string;
    street: string;
    city: string;
    state: string;
    zip: string;
    contacts: string;
    campus: string;
};