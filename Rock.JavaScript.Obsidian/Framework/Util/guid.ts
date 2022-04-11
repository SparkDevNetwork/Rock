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
export type Guid = string;

/** An empty unique identifier. */
export const emptyGuid = "00000000-0000-0000-0000-000000000000";

/**
* Generates a new Guid
*/
export function newGuid (): Guid {
    return "xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx".replace(/[xy]/g, (c) => {
        const r = Math.random() * 16 | 0;
        const v = c === "x" ? r : r & 0x3 | 0x8;
        return v.toString(16);
    });
}

/**
 * Returns a normalized Guid that can be compared with string equality (===)
 * @param a
 */
export function normalize (a: Guid | null | undefined): Guid | null {
    if (!a) {
        return null;
    }

    return a.toLowerCase();
}

/**
 * Checks if the given string is a valid Guid. To be considered valid it must
 * be a bare guid with hyphens. Bare means not enclosed in '{' and '}'.
 * 
 * @param guid The Guid to be checked.
 * @returns True if the guid is valid, otherwise false.
 */
export function isValidGuid(guid: Guid | string): boolean {
    return /^[0-9A-Fa-f]{8}-(?:[0-9A-Fa-f]{4}-){3}[0-9A-Fa-f]{12}$/.test(guid);
}

/**
 * Converts the string value to a Guid.
 * 
 * @param value The value to be converted.
 * @returns A Guid value or null is the string could not be parsed as a Guid.
 */
export function toGuidOrNull(value: string | null | undefined): Guid | null {
    if (value === null || value === undefined) {
        return null;
    }

    if (!isValidGuid(value)) {
        return null;
    }

    return value as Guid;
}

/**
 * Are the guids equal?
 * @param a
 * @param b
 */
export function areEqual (a: Guid | null | undefined, b: Guid | null | undefined): boolean {
    return normalize(a) === normalize(b);
}

export default {
    newGuid,
    normalize,
    areEqual
};

