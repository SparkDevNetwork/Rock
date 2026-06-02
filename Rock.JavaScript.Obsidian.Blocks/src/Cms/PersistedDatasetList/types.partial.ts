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


import { PersistedDatasetStatus } from "@Obsidian/Enums/Cms/persistedDatasetStatus";

export const enum NavigationUrlKey {
    DetailPage = "DetailPage"
}

export type Row = {
    idKey: string;
    name?: string | null;
    lastRefreshDateTime?: string | null;
    accessKey?: string | null;
    timeToBuildMS?: number | null;
    allowManualRefresh: boolean;
    resultSize?: number | null;
    isSystem: boolean;
    isActive: boolean;
    status: PersistedDatasetStatus;
};
