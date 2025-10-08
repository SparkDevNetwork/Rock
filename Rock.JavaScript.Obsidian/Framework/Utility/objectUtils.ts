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

import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";

export function fromEntries(entries: Iterable<[PropertyKey, string]>): Record<string, unknown> {
    const res = {};
    for (const entry of entries) {
        res[entry[0]] = entry[1];
    }
    return res;
}

/**
 * Clone an object/array. Only works with values that can be converted to JSON, which means no
 * functions, self-/circular references, etc.
 */
export function clone<T>(obj: T): T {
    return JSON.parse(JSON.stringify(obj));
}

/**
 * Take an object and convert it to a list of ListItemBag objects.
 * Each property is a ListItemBag where the key becomes the text property
 * and the value becomes the value property.
 *
 * @param obj The object to convert. Each property value should be able to be converted to a string.
 *
 * @return A list of ListItemBags.
 */
export function toListItemBagList(obj: Record<string, unknown>): ListItemBag[] {
    return Object.entries(obj).map(([key, value]) => {
        return {
            text: key,
            value: `${value}`,
        } as ListItemBag;
    });
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

/**
 * Gets the member name of a type.
 *
 * This is useful for ensuring that the property name is valid and exists on the type.
 *
 * @param memberName The member name to get. This should be a key of the type `T`.
 * @template T The type to get the member name from.
 * @returns The member name as a key of the type `T`.
 */
export function getTypeMemberName<T, K extends keyof T>(memberName: K): K {
    return memberName;
}

/**
 * Returns a function to get the member name of a type.
 *
 * @example
 * const getMemberName = useGetTypeMemberName<MyType>();
 * const memberName = getMemberName("myProperty"); // returns "myProperty" as a key of MyType
 *
 * @returns A function that takes a member name and returns it as a key of the type `T`.
 */
export function useGetTypeMemberName<T>(): <K extends keyof T>(memberName: K) => K {
    return <K extends keyof T>(memberName: K): K => {
        return getTypeMemberName<T, K>(memberName);
    };
}