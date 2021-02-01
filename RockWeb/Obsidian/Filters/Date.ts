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
/**
 * Adjust for the timezone offset so early morning times don't appear as the previous local day.
 * @param val
 */
function stripTimezone(val: Date) {
    const asUtc = new Date(val.getTime() + val.getTimezoneOffset() * 60000);
    return asUtc;
}

/**
 * Transform the value into a date or null
 * @param val
 */
export function asDateOrNull(val: unknown) {
    if (val === undefined || val === null) {
        return null;
    }

    if (val instanceof Date) {
        return val;
    }

    if (typeof val === 'string') {
        const ms = Date.parse(val);

        if (isNaN(ms)) {
            return null;
        }

        return stripTimezone(new Date(ms));
    }

    return null;
}

/**
 * To a date picker value.  Mon Dec 2 => 2000-12-02
 * @param val
 */
export function toDatePickerValue(val: unknown) {
    const date = asDateOrNull(val);

    if (date === null) {
        return '';
    }

    return date.toISOString().split('T')[0];
}

/**
 * Transforms the value into a string like '9/13/2001'
 * @param val
 */
export function asDateString(val: unknown) {
    const dateOrNull = asDateOrNull(val);

    if (!dateOrNull) {
        return '';
    }

    return dateOrNull.toLocaleDateString();
}