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

import { TimeIntervalBag } from "@Obsidian/ViewModels/Utility/timeIntervalBag";
import { TimeIntervalUnit } from "@Obsidian/Enums/Core/timeIntervalUnit";
import { isNullish } from "@Obsidian/Utility/util";

/**
 * Converts a minute count into a TimeIntervalBag using the largest
 * whole unit (days, hours, or minutes).
 *
 * @param minutes The number of minutes to convert.
 * @returns A TimeIntervalBag representing the interval.
 */
export function minutesToIntervalBag(minutes: number): TimeIntervalBag {
    if (minutes <= 0) {
        return { unit: TimeIntervalUnit.Days, value: 1 };
    }

    let value = minutes;
    let unit;

    if (minutes % 1440 === 0) {
        value = minutes / 1440;
        unit = TimeIntervalUnit.Days;
    }
    else if (minutes % 60 === 0) {
        value = minutes / 60;
        unit = TimeIntervalUnit.Hours;
    }
    else {
        value = minutes;
        unit = TimeIntervalUnit.Minutes;
    }

    return { unit, value };
}

/**
 * Converts a TimeIntervalBag back into a minute count.
 *
 * @param bag The interval bag to convert.
 * @returns The number of minutes represented by the bag.
 */
export function intervalBagToMinutes(bag: TimeIntervalBag | null): number {
    if (isNullish(bag) || isNullish(bag.value)) {
        return 0;
    }

    const value = bag.value;

    switch (bag.unit) {
        case TimeIntervalUnit.Days:
            return value * 1440;
        case TimeIntervalUnit.Hours:
            return value * 60;
        case TimeIntervalUnit.Minutes:
            return value;
        default:
            return 1440;
    }
}
