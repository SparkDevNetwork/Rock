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

export function fromEntries(entries: Iterable<[PropertyKey, string]>): Record<string, unknown> {
    const res = {};
    for (const entry of entries) {
        res[entry[0]] = entry[1];
    }
    return res;
}

/**
 * Gets the value at the specified path within the object.
 *
 * @example
 * const object = {
 *     person: {
 *          name: "Ted Decker"
 *     }
 * };
 *
 * const value = getValueFromPath(object, "person.name"); // returns "Ted Decker"
 *
 * @param object The object containing the desired value.
 * @param path The dot-separated path name to the desired value.
 * @returns The value at the specified path within the object, or `undefined`
 * if no such path exists.
 */
export function getValueFromPath(object: Record<string, unknown>, path: string): unknown {
    if (!object || !path) {
        return;
    }

    const pathNames = path.split(".");

    for (let i = 0; i < pathNames.length; i++) {
        const pathName = pathNames[i].trim();

        // If the object doesn't have the specified path name as its own
        // property, return `undefined`.
        if (!pathName || !Object.prototype.hasOwnProperty.call(object, pathName)) {
            return;
        }

        const value = object[pathName];

        // If this is the last path name specified, return the current value.
        if (i === pathNames.length - 1) {
            return value;
        }

        // If the current value is not an object, but there are still
        // more path names to traverse, return `undefined`.
        if (typeof value !== "object") {
            return;
        }

        // Reassign `object` to the current value. This type assertion might
        // be incorrect, but will be caught on the next iteration if so,
        // in which case `undefined` will be returned.
        object = value as Record<string, unknown>;
    }

    // If we somehow got here, return `undefined`.
    return;
}
