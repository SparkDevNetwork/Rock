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

import { IEntity } from "@Obsidian/ViewModels/entity";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";

export const enum NavigationUrlKey {
    ParentPage = "ParentPage"
}

export type CampusScheduleBag = {
    guid?: string | null;

    schedule?: ListItemBag | null;

    scheduleTypeValue?: ListItemBag | null;
}

export type CampusBag = IEntity & {
    campusSchedules?: CampusScheduleBag[] | null;

    campusStatusValue?: ListItemBag | null;

    campusTypeValue?: ListItemBag | null;

    description?: string | null;

    isActive?: boolean;

    isSystem: boolean;

    leaderPersonAlias?: ListItemBag | null;

    location?: ListItemBag | null;

    name?: string | null;

    phoneNumber?: string | null;

    serviceTimes?: ListItemBag[] | null;

    shortCode?: string | null;

    timeZoneId?: string | null;

    url?: string | null;
};

export type CampusDetailOptionsBag = {
    isMultiTimeZoneSupported?: boolean;

    timeZoneOptions?: ListItemBag[] | null;
};

// #region Core Types

export type DetailBlockBox<TPacket, TOptions> = {
    entity?: TPacket | null;

    isEditable?: boolean;

    errorMessage?: string | null;

    navigationUrls?: Record<string, string> | null;

    validProperties?: string[] | null;

    options?: TOptions | null;
};

// #endregion
