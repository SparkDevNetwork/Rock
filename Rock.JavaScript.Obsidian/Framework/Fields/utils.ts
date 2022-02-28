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
import { PropType } from "vue";
import { Guid, normalize, isValidGuid } from "../Util/guid";
import { IFieldType } from "./fieldType";

const fieldTypeTable: Record<Guid, IFieldType> = {};

export type ConfigurationValues = Record<string, string>;

/**
 * The basic properties that all field editor components must support.
 */
type FieldEditorBaseProps = {
    modelValue: {
        type: PropType<string>,
        required: true
    };

    configurationValues: {
        type: PropType<ConfigurationValues>;
        default: () => ConfigurationValues;
    };
};

/**
 * The basic properties that all field configuration components must support.
 */
type FieldConfigurationBaseProps = {
    modelValue: {
        type: PropType<Record<string, string>>,
        required: true
    };

    configurationProperties: {
        type: PropType<Record<string, string>>,
        required: true
    };
};

/**
 * Get the standard properties that all field editor components must support.
 */
export function getFieldEditorProps(): FieldEditorBaseProps {
    return {
        modelValue: {
            type: String as PropType<string>,
            required: true
        },

        configurationValues: {
            type: Object as PropType<ConfigurationValues>,
            default: () => ({})
        }
    };
}

/**
 * Gets the standard field configuration properties that all field configuration
 * components must support.
 */
export function getFieldConfigurationProps(): FieldConfigurationBaseProps {
    return {
        modelValue: {
            type: Object as PropType<Record<string, string>>,
            required: true
        },
        configurationProperties: {
            type: Object as PropType<Record<string, string>>,
            required: true
        }
    };
}

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
