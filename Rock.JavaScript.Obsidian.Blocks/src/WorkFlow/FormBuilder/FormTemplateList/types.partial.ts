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
    FilterActiveStatus = "filter-active-status"
}

/** The stored values for the active status filter, mirroring the block's C# ActiveStatus constants. */
export const enum ActiveStatus {
    All = "All",
    Active = "Active",
    Inactive = "Inactive"
}

export type GridSettingsOptions = {
    activeStatus: string;
};
