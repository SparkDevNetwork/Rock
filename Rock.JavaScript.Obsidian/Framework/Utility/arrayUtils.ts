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
 * Flatten a nested array down by the given number of levels.
 * Meant to be a replacement for the official Array.prototype.flat, which isn't supported by all browsers we support.
 * Adapted from Polyfill: https://github.com/behnammodi/polyfill/blob/master/array.polyfill.js#L591
 *
 * @param arr (potentially) nested array to be flattened
 * @param depth The depth level specifying how deep a nested array structure should be flattened. Defaults to 1.
 *
 * @returns A new array with the sub-array elements concatenated into it.
 */
export const flatten = <T>(arr: T[][], depth: number = 1): T[] => {
    const result: T[] = [];
    const forEach = result.forEach;

    const flatDeep = function (arr, depth): void {
        forEach.call(arr, function (val) {
            if (depth > 0 && Array.isArray(val)) {
                flatDeep(val, depth - 1);
            }
            else {
                result.push(val);
            }
        });
    };

    flatDeep(arr, depth);
    return result;
};