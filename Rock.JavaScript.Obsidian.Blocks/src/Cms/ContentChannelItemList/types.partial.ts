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

import { ContentChannelItemStatus } from "@Obsidian/Enums/Cms/contentChannelItemStatus";

export const enum NavigationUrlKey {
    DetailPage = "DetailPage",
    NewItemPage = "NewItemPage",
    LibraryDownloadPage = "LibraryDownloadPage",
    MediaElementPage = "MediaElementPage"
}

export type GridRowItem = {
    id: string,
    idKey: string,
    contentChannelId: number,
    title: string,
    startDateTime: string,
    expireDateTime: string,
    isScheduled: boolean,
    occurrences: boolean,
    status: ContentChannelItemStatus,
    priority: number,
    isContentLibraryOwner: boolean | null,
    contentLibrarySourceIdentifier: boolean | null,
    isDownloadedFromContentLibrary: boolean,
    isUploadedToContentLibrary: boolean,
    contentLibraryLicenseTypeGuid: string | null,
    isSecurityDisabled: boolean,
    allTimeViewsCount?: number | null,
    last28DaysViewsCount?: number | null,
    itemUrl?: string | null,
    hasLinkedMediaElements: boolean
};
