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
import { Component, computed, defineComponent, PropType, ref, watch, watchEffect } from "vue";
import { binaryComparisonTypes, ComparisonType, containsComparisonTypes, isCompareVisibleForComparisonFilter, isSingleComparisonType, stringComparisonTypes } from "../Reporting/comparisonType";
import { getFilteredComparisonTypeOptions } from "../Reporting/comparisonTypeOptions";
import { ComparisonValue } from "../Reporting/comparisonValue";
import { FilterMode } from "../Reporting/filterMode";
import { Guid, normalize, isValidGuid } from "../Util/guid";
import { IFieldType } from "./fieldType";
import DropDownList from "../Elements/dropDownList";
import FieldFilterContainer from "../Elements/fieldFilterContainer";
import { toNumberOrNull } from "../Services/number";
import { useVModelPassthrough } from "../Util/component";

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
 * The properties that all field filter components must support.
 */
type FieldFilterProps = {
    modelValue: {
        type: PropType<ComparisonValue>,
        required: true
    },

    configurationValues: {
        type: PropType<ConfigurationValues>,
        required: true
    },

    filterMode: {
        type: PropType<FilterMode>,
        required: true
    },

    required: {
        type: PropType<boolean>,
        required: true
    }
};

/**
 * The properties that all field filter components must support.
 */
export const fieldFilterProps: FieldFilterProps = {
    modelValue: {
        type: Object as PropType<ComparisonValue>,
        required: true
    },

    configurationValues: {
        type: Object as PropType<ConfigurationValues>,
        required: true
    },

    filterMode: {
        type: Number as PropType<FilterMode>,
        required: true
    },

    required: {
        type: Boolean as PropType<boolean>,
        required: true
    }
};

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
 * An alternative to `FieldFilterRule` that is used inside a Standard Filter
 * Component. This is done to allow the comparison type to be a string value
 * so that it can be selected via a `DropDownList` (they do not support
 * numeric values).
 */
type EditableFieldFilterRule = {
    comparisonType: string,
    value: any
}


/**
 * Gets a standard filter component that uses a constant string in place of the
 * comparison type picker. This component will always emit a ComparisonValue
 * with a null type.
 * 
 * @param compareLabel The string to display in place of a comparison picker.
 * @param valueComponent The component that will handle editing the value.
 *
 * @returns A component that will handle editing a filter value.
 */
export function getStandardFilterComponent(compareLabel: string, valueComponent: Component): Component;

/**
 * Gets a standard filter component that uses a picker to let the individual
 * select the comparison type to perform. If a single comparison type bit is
 * set then the picker will be hidden. If null is used for the comparison types
 * then no picker component will be shown and null will be emitted for the
 * type property.
 *
 * @param comparisonTypes One or more comparison types the user can select from.
 * @param valueComponent The component that will handle editing the value.
 *
 * @returns A component that will handle editing a filter value.
 */
export function getStandardFilterComponent(comparisonTypes: ComparisonType | null, valueComponent: Component): Component;

/**
 * Gets a standard filter component that can be used by field types to generate
 * their filter component without having to write one from scratch.
 * 
 * @param comparisonLabelOrTypes The comparison label or the comparison types that will be shown.
 * @param valueComponent The component that will handle value entry from the individual.
 *
 * @returns A component that will handle editing a filter value.
 */
export function getStandardFilterComponent(comparisonLabelOrTypes: ComparisonType | string | null, valueComponent: Component): Component {
    const comparisonTypes: ComparisonType | null = typeof comparisonLabelOrTypes === "number" ? comparisonLabelOrTypes : null;
    const compareLabel: string = typeof comparisonLabelOrTypes === "string" ? comparisonLabelOrTypes : "";

    const comparisonTypeOptions = comparisonTypes !== null ? getFilteredComparisonTypeOptions(comparisonTypes) : [];
    

    return defineComponent({
        name: "StandardFilterComponent",

        components: {
            DropDownList,
            FieldFilterContainer,
            ValueComponent: valueComponent
        },

        props: fieldFilterProps,

        emits: [
            "update:modelValue"
        ],

        setup(props, { emit }) {
            const internalValue = useVModelPassthrough(props, 'modelValue', emit);
            const comparisonType = ref<string | null>(`${internalValue.value.comparisonType}`);

            watchEffect(() => {
                let type: string | null;
                
                // For Simple Filters, set the appropriate comparison type based on the 
                // comparison types supported by the field type
                if (props.filterMode === FilterMode.Simple) {
                    if (comparisonTypes === binaryComparisonTypes) {
                        type = ComparisonType.EqualTo.toString();
                    }
                    else if (comparisonTypes === stringComparisonTypes || comparisonTypes === containsComparisonTypes) {
                        type = ComparisonType.Contains.toString();
                    }
                    else {
                        type = null;
                    }
                }
                // For non-simple filters, allow user to set the comparison type
                else {
                    type = internalValue.value.comparisonType?.toString() ?? null;
                }

                comparisonType.value = type;
            });

            watchEffect(() => {
                internalValue.value.comparisonType = toNumberOrNull(comparisonType.value);
            });

            /** True if the compare component should be visible. */
            const hasCompareComponent = computed(() => {
                return comparisonTypes !== null
                    && props.filterMode !== FilterMode.Simple
                    && !isSingleComparisonType(comparisonTypes)
                    && isCompareVisibleForComparisonFilter(comparisonTypes, props.filterMode);
            });

            /** True if the value component should be visible. */
            const hasValueComponent = computed((): boolean => {
                return comparisonType.value !== ComparisonType.IsBlank.toString()
                    && comparisonType.value !== ComparisonType.IsNotBlank.toString();
            });

            /** True if the comparison type is optional. */
            const isTypeOptional = computed(() => !props.required);

            return {
                compareLabel,
                comparisonTypeOptions,
                hasCompareComponent,
                hasValueComponent,
                isTypeOptional,
                internalValue,
                comparisonType
            };
        },

        template: `
<FieldFilterContainer :compareLabel="compareLabel">
    <template v-if="hasCompareComponent" #compare>
        <DropDownList v-model="comparisonType" :options="comparisonTypeOptions" :showBlankItem="!required" />
    </template>

    <ValueComponent v-if="hasValueComponent" v-model="internalValue.value" :configurationValues="configurationValues" />
</FieldFilterContainer>
`
    });
}

