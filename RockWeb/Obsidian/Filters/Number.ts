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
 * Get a formatted string.
 * Ex: 10001.2 => 10,001.2
 * @param num
 */
export function asFormattedString(num: number | null, digits = 2) {
    if (num === null) {
        return '';
    }

    return num.toLocaleString(
        'en-US',
        {
            minimumFractionDigits: digits,
            maximumFractionDigits: digits
        }
    );
}

/**
 * Get a number value from a formatted string.
 * Ex: $1,000.20 => 1000.2
 * @param str
 */
export function toNumberOrNull(str: string | null) {
    if (str === null) {
        return null;
    }

    const replaced = str.replace(/[$,]/g, '');
    return Number(replaced) || 0;
}