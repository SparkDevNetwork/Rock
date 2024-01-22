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
export function clone<T>(obj:T) : T {
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
export function toListItemBagList (obj: Record<string, unknown>): ListItemBag[] {
    return Object.entries(obj).map(([key, value]) => {
        return {
            text: key,
            value: `${value}`,
        } as ListItemBag;
    });
}