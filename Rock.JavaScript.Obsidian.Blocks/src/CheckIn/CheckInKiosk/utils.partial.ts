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

import { parse } from "@Obsidian/Libs/marked";
import { CheckInItemBag } from "@Obsidian/ViewModels/CheckIn/checkInItemBag";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";

export type CampusBag = CheckInItemBag & {
    kiosks?: CheckInItemBag[] | null;
};

export type CheckInKioskOptionsBag = {
    campuses?: CampusBag[] | null;

    themes?: ListItemBag[] | null;
};

export type SavedConfigurationBag = {
    name?: string | null;

    description?: string | null;
};

export function markdown(content: string | undefined | null): string {
    if (!content) {
        return "";
    }

    return parse(content) as string;
}
