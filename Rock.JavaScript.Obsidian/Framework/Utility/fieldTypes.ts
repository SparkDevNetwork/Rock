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

import { Guid } from "@Obsidian/Types";
import { isValidGuid, normalize } from "./guid";
import { IFieldType } from "@Obsidian/Types/fieldType";
import { Ref, inject, provide, ref } from "vue";

const fieldTypeTable: Record<Guid, IFieldType> = {};
const fieldTypeAttributeGuidSymbol = Symbol();


/** Determines how the field type component is being used so it can adapt to different */
export type DataEntryMode = "defaultValue" | undefined;

/**
 * Register a new field type in the system. This must be called for all field
 * types a plugin registers.
 *
 * @param fieldTypeGuid The unique identifier of the field type.
 * @param fieldType The class instance that will handle the field type.
 */
export function registerFieldType(fieldTypeGuid: Guid, fieldType: IFieldType): void {
    const normalizedGuid = normalize(fieldTypeGuid);

    if (!isValidGuid(fieldTypeGuid) || normalizedGuid === null) {
        throw "Invalid guid specified when registering field type.";
    }

    if (fieldTypeTable[normalizedGuid] !== undefined) {
        throw "Invalid attempt to replace existing field type.";
    }

    fieldTypeTable[normalizedGuid] = fieldType;
}

/**
 * Get the field type handler for a given unique identifier.
 *
 * @param fieldTypeGuid The unique identifier of the field type.
 *
 * @returns The field type instance or null if not found.
 */
export function getFieldType(fieldTypeGuid: Guid): IFieldType | null {
    const normalizedGuid = normalize(fieldTypeGuid);

    if (normalizedGuid !== null) {
        const field = fieldTypeTable[normalizedGuid];

        if (field) {
            return field;
        }
    }

    console.warn(`Field type "${fieldTypeGuid}" was not found`);
    return null;
}

/**
 * Provides the attribute guid for a field type component to use.
 *
 * @param guid A reference to the attribute guid value.
 */
export function provideFieldTypeAttributeGuid(guid: Ref<Guid | undefined | null>): void {
    provide(fieldTypeAttributeGuidSymbol, guid);
}

/**
 * Gets a reference to the attribute guid value for this field. The value may be
 * undefined or null.
 *
 * @returns A function that will cause the block component to be reloaded.
 */
export function useFieldTypeAttributeGuid(): Ref<Guid | undefined | null> {
    return inject<Ref<Guid | undefined | null>>(fieldTypeAttributeGuidSymbol, ref());
}
