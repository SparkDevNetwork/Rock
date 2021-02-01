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
 * Is the value an empty string?
 * @param val
 */
export function isEmpty(val: unknown) {
    if (typeof val === 'string') {
        return val.length === 0;
    }

    return false;
}

/**
 * Is the value an empty string?
 * @param val
 */
export function isWhitespace(val: unknown) {
    if (typeof val === 'string') {
        return val.trim().length === 0;
    }

    return false;
}

/**
 * Is the value null, undefined or whitespace?
 * @param val
 */
export function isNullOrWhitespace(val: unknown) {
    return isWhitespace(val) || val === undefined || val === null;
}

/**
 * Turns "MyCamelCaseString" into "My Camel Case String"
 * @param val
 */
export function splitCamelCase(val: unknown) {
    if (typeof val === 'string') {
        return val.replace(/([a-z])([A-Z])/g, '$1 $2');
    }

    return val;
}

/**
 * Returns an English comma-and fragment.
 * Ex: ['a', 'b', 'c'] => 'a, b, and c'
 * @param strs
 */
export function asCommaAnd(strs: string[]) {
    if (strs.length === 0) {
        return '';
    }

    if (strs.length === 1) {
        return strs[0];
    }

    if (strs.length === 2) {
        return `${strs[0]} and ${strs[1]}`;
    }

    const last = strs.pop();
    return `${strs.join(', ')}, and ${last}`;
}