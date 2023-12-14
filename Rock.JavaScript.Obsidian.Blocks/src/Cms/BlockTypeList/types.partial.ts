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

export type Row = {
    id: number;

    name: string;

    category?: string | null;

    description?: string | null;

    entityTypeId?: number | null;

    isSystem: boolean;

    path?: string | null;

    blockCount?: number | null;

    status?: string | null;

};

export const enum PreferenceKey {
    FilterName = "filter-name",

    FilterPath = "filter-path",

    FilterCategory = "filter-category",

    FilterSystemTypes = "filter-system-types"
}

export type GridSettingsOptions = {
    name?: string | null;

    path?: string | null;

    category?: string | null;

    excludeSystemTypes: boolean;
};
