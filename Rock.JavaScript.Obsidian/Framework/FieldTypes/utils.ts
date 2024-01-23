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

import { Component, computed, defineComponent, PropType, ref, watch } from "vue";
import { binaryComparisonTypes, containsComparisonTypes, isCompareVisibleForComparisonFilter, isSingleComparisonType, stringComparisonTypes } from "@Obsidian/Core/Reporting/comparisonType";
import { getFilteredComparisonTypeOptions } from "@Obsidian/Core/Reporting/comparisonTypeOptions";
import { ComparisonValue } from "@Obsidian/Types/Reporting/comparisonValue";
import { ComparisonType } from "@Obsidian/Enums/Reporting/comparisonType";
import { FilterMode } from "@Obsidian/Core/Reporting/filterMode";
import DropDownList from "@Obsidian/Controls/dropDownList.obs";
import FieldFilterContainer from "@Obsidian/Controls/fieldFilterContainer.obs";
import { toNumberOrNull } from "@Obsidian/Utility/numberUtils";
import { DataEntryMode } from "@Obsidian/Utility/fieldTypes";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";

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

    /**
     * This is used internally by the fieldTypeEditor to allow controls to make adjustments based
     * on how it's being used (e.g. to define a default value vs to edit a value)
     */
    dataEntryMode: {
        type: PropType<DataEntryMode>;
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
        },

        dataEntryMode: {
            type: String as PropType<DataEntryMode>
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
 * Allows callers to modify the names of the comparison type options. The values
 * and the array itself should not be modified.
 */
export type UpdateComparisonTypeNamesCallback = (comparisonTypeOptions: ListItemBag[]) => void;

/**
 * Options that can be passed to the getStandardFilterComponent method which
 * will alter it's default behavior.
 */
export type StandardFilterComponentOptions = {
    updateComparisonTypeNames?: UpdateComparisonTypeNamesCallback;
};

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
export function getStandardFilterComponent(compareLabel: string, valueComponent: Component, options?: StandardFilterComponentOptions): Component;

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
export function getStandardFilterComponent(comparisonTypes: ComparisonType | null, valueComponent: Component, options?: StandardFilterComponentOptions): Component;

/**
 * Gets a standard filter component that can be used by field types to generate
 * their filter component without having to write one from scratch.
 *
 * @param comparisonLabelOrTypes The comparison label or the comparison types that will be shown.
 * @param valueComponent The component that will handle value entry from the individual.
 *
 * @returns A component that will handle editing a filter value.
 */
export function getStandardFilterComponent(comparisonLabelOrTypes: ComparisonType | string | null, valueComponent: Component, options?: StandardFilterComponentOptions): Component {
    const comparisonTypes: ComparisonType | null = typeof comparisonLabelOrTypes === "number" ? comparisonLabelOrTypes : null;
    const compareLabel: string = typeof comparisonLabelOrTypes === "string" ? comparisonLabelOrTypes : "";

    let comparisonTypeOptions = comparisonTypes !== null ? getFilteredComparisonTypeOptions(comparisonTypes) : [];

    if (options?.updateComparisonTypeNames) {
        // Create a new array with new objects so we don't modify the core
        // set for every component on the page.
        comparisonTypeOptions = comparisonTypeOptions.map(o => {
            return {
                value: o.value,
                text: o.text
            };
        });

        options.updateComparisonTypeNames(comparisonTypeOptions);
    }

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
            /** The comparison type currently selected in the UI. */
            const internalComparisonType = ref(props.modelValue.comparisonType?.toString() ?? "");
            const comparisonType = ref(props.modelValue.comparisonType ?? null);

            /** The comparison value currently entered in the UI. */
            const internalComparisonValue = ref(props.modelValue.value);

            /** True if the compare component should be visible. */
            const hasCompareComponent = computed(() => {
                return comparisonTypes !== null
                    && props.filterMode !== FilterMode.Simple
                    && !isSingleComparisonType(comparisonTypes)
                    && isCompareVisibleForComparisonFilter(comparisonTypes, props.filterMode);
            });

            /** True if the value component should be visible. */
            const hasValueComponent = computed((): boolean => {
                return internalComparisonType.value !== ComparisonType.IsBlank.toString()
                    && internalComparisonType.value !== ComparisonType.IsNotBlank.toString();
            });

            /** True if the comparison type is optional. */
            const isTypeOptional = computed(() => !props.required);

            /**
             * Constructs the new value and emits it if it has changed from
             * the current modelValue.
             */
            const emitValueIfChanged = (): void => {
                let type: ComparisonType | null | undefined;

                // Determine the comparison type.
                if (compareLabel || comparisonTypes === null) {
                    // No comparison type to be selected.
                    type = null;
                }
                else if (isSingleComparisonType(comparisonTypes)) {
                    // Only a single comparison type, so it is forced.
                    type = comparisonTypes;
                }
                else {
                    // If the filter mode is simple, then the comparison type is
                    // not shown so we come up with a sane default.
                    if (props.filterMode === FilterMode.Simple) {
                        if (comparisonTypes === binaryComparisonTypes) {
                            type = ComparisonType.EqualTo;
                        }
                        else if (comparisonTypes === stringComparisonTypes || comparisonTypes === containsComparisonTypes) {
                            type = ComparisonType.Contains;
                        }
                        else {
                            type = null;
                        }
                    }
                    else {
                        // Get the comparison type selected by the user if we are
                        // in advanced mode.
                        type = toNumberOrNull(internalComparisonType.value);
                    }
                }

                comparisonType.value = type;

                // Construct the new value to be emitted.
                const newValue: ComparisonValue = {
                    comparisonType: type,
                    value: internalComparisonValue.value
                };

                // Check if it has changed from the original value.
                if (newValue.comparisonType !== props.modelValue.comparisonType || newValue.value !== props.modelValue.value) {
                    emit("update:modelValue", newValue);
                }
            };

            // Watch for changes in our modelValue and update our internal
            // values to match.
            watch(() => props.modelValue, () => {
                internalComparisonType.value = props.modelValue.comparisonType?.toString() ?? "";
                comparisonType.value = props.modelValue.comparisonType ?? null;
                internalComparisonValue.value = props.modelValue.value;
            });

            // Watch for changes in our internal values and update the modelValue.
            watch([internalComparisonType, internalComparisonValue], () => {
                emitValueIfChanged();
            });

            // This is primarily here to sync up state with default values.
            emitValueIfChanged();

            return {
                compareLabel,
                comparisonType,
                comparisonTypeOptions,
                hasCompareComponent,
                hasValueComponent,
                internalComparisonType,
                internalComparisonValue,
                isTypeOptional
            };
        },

        template: `
<FieldFilterContainer :compareLabel="compareLabel">
    <template v-if="hasCompareComponent" #compare>
        <DropDownList v-model="internalComparisonType" :items="comparisonTypeOptions" :showBlankItem="isTypeOptional" />
    </template>

    <ValueComponent v-if="hasValueComponent" v-model="internalComparisonValue" :configurationValues="configurationValues" :comparisonType="comparisonType" />
</FieldFilterContainer>
`
    });
}

