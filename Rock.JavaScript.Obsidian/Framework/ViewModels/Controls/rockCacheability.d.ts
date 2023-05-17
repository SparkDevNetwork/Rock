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

import { RockCacheabilityType } from "@Obsidian/Enums/Controls/rockCacheabilityType";
import { TimeInterval } from "@Obsidian/ViewModels/Utility/timeInterval";

/**
 * Represents a setting for how to cache a resource. Based on Rock.Utility.RockCacheability
 */
export type RockCacheability = {
    /** Cache header type */
    rockCacheabilityType: RockCacheabilityType;

    /** Max amount of time to cache */
    maxAge: TimeInterval;

    /** Max amount of time to cache in a shared cache */
    sharedMaxAge: TimeInterval;
};
