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

export type RockDateType = string;

/**
 * Adjust for the timezone offset so early morning times don't appear as the previous local day.
 * @param val
 */
export function stripTimezone(val: Date) {
    const asUtc = new Date(val.getTime() + val.getTimezoneOffset() * 60000);
    return asUtc;
}

/**
 * Convert a date to a RockDate
 * @param d
 */
export function toRockDate(d: Date): RockDateType {
    if (d instanceof Date && !isNaN(d as unknown as number)) {
        return stripTimezone(d).toISOString().split('T')[0];
    }

    return '';
}

/**
* Generates a new Rock Date
*/
export function newDate(): RockDateType {
    return toRockDate(new Date());
}

/**
 * Returns the day.
 * Ex: 1/2/2000 => 2
 * @param d
 */
export function getDay(d: RockDateType | null): number | null {
    if (!d) {
        return null;
    }

    const asDate = stripTimezone(new Date(d));
    return asDate.getDate();
}

/**
 * Returns the month.
 * Ex: 1/2/2000 => 1
 * @param d
 */
export function getMonth(d: RockDateType | null): number | null {
    if (!d) {
        return null;
    }

    const asDate = stripTimezone(new Date(d));
    return asDate.getMonth() + 1;
}

/**
 * Returns the year.
 * Ex: 1/2/2000 => 2000
 * @param d
 */
export function getYear(d: RockDateType | null): number | null {
    if (!d) {
        return null;
    }

    const asDate = stripTimezone(new Date(d));
    return asDate.getFullYear();
}

export default {
    newDate,
    toRockDate,
    getDay,
    getMonth,
    getYear,
    stripTimezone
};