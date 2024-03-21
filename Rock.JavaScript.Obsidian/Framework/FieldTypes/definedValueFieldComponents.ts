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
import { computed, defineComponent, inject, PropType, ref, watch } from "vue";
import CheckBox from "@Obsidian/Controls/checkBox.obs";
import CheckBoxList from "@Obsidian/Controls/checkBoxList.obs";
import DropDownList from "@Obsidian/Controls/dropDownList.obs";
import NumberBox from "@Obsidian/Controls/numberBox.obs";
import { asBoolean, asTrueFalseOrNull } from "@Obsidian/Utility/booleanUtils";
import { toNumber, toNumberOrNull } from "@Obsidian/Utility/numberUtils";
import { useVModelPassthrough } from "@Obsidian/Utility/component";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { ClientValue, ConfigurationPropertyKey, ConfigurationValueKey, ValueItem } from "./definedValueField.partial";
import { getFieldEditorProps } from "./utils";

function parseModelValue(modelValue: string | undefined): string {
    try {
        const clientValue = JSON.parse(modelValue ?? "") as ClientValue;

        return clientValue.value ?? "";
    }
    catch {
        return "";
    }
}

function getClientValue(value: string | string[], valueOptions: ValueItem[]): ClientValue {
    const values = Array.isArray(value) ? value : [value];
    const selectedValues = valueOptions.filter(v => values.includes(v.value));

    if (selectedValues.length >= 1) {
        return {
            value: selectedValues.map(v => v.value).join(","),
            text: selectedValues.map(v => v.text).join(", "),
            description: selectedValues.map(v => v.description).join(", ")
        };
    }
    else {
        return {
            value: "",
            text: "",
            description: ""
        };
    }
}

export const EditComponent = defineComponent({
    name: "DefinedValueField.Edit",

    components: {
        DropDownList,
        CheckBoxList
    },

    props: getFieldEditorProps(),

    setup(props, { emit }) {
        const internalValue = ref(parseModelValue(props.modelValue));
        const internalValues = ref(parseModelValue(props.modelValue).split(",").filter(v => v !== ""));

        const valueOptions = computed((): ValueItem[] => {
            try {
                return JSON.parse(props.configurationValues[ConfigurationValueKey.Values] ?? "[]") as ValueItem[];
            }
            catch {
                return [];
            }
        });

        const displayDescription = computed((): boolean => asBoolean(props.configurationValues[ConfigurationValueKey.DisplayDescription]));

        /** The options to choose from */
        const options = computed((): ListItemBag[] => {
            return valueOptions.value.map(v => {
                return {
                    text: displayDescription.value ? (v.description || v.text) : v.text,
                    value: v.value
                };
            });
        });

        const isMultiple = computed((): boolean => asBoolean(props.configurationValues[ConfigurationValueKey.AllowMultiple]));

        const configAttributes = computed((): Record<string, unknown> => {
            const attributes: Record<string, unknown> = {};

            const enhancedConfig = props.configurationValues[ConfigurationValueKey.EnhancedSelection];
            if (enhancedConfig) {
                attributes.enhanceForLongLists = asBoolean(enhancedConfig);
            }

            attributes.multiple = isMultiple.value;

            return attributes;
        });

        /** The number of columns wide the checkbox list will be. */
        const repeatColumns = computed((): number => toNumber(props.configurationValues[ConfigurationValueKey.RepeatColumns]));

        watch(() => props.modelValue, () => {
            internalValue.value = parseModelValue(props.modelValue);
            internalValues.value = parseModelValue(props.modelValue).split(",").filter(v => v !== "");
        });

        watch(() => internalValue.value, () => {
            if (!isMultiple.value) {
                const clientValue = getClientValue(internalValue.value, valueOptions.value);

                emit("update:modelValue", JSON.stringify(clientValue));
            }
        });

        watch(() => internalValues.value, () => {
            if (isMultiple.value) {
                const clientValue = getClientValue(internalValues.value, valueOptions.value);

                emit("update:modelValue", JSON.stringify(clientValue));
            }
        });

        return {
            configAttributes,
            internalValue,
            internalValues,
            isMultiple,
            isRequired: inject("isRequired") as boolean,
            options,
            repeatColumns
        };
    },

    template: `
<CheckBoxList v-if="isMultiple && !configAttributes.enhanceForLongLists" v-model="internalValues" :items="options" horizontal :repeatColumns="repeatColumns" />
<DropDownList v-else-if="isMultiple && configAttributes.enhanceForLongLists" v-model="internalValues" v-bind="configAttributes" :items="options" />
<DropDownList v-else v-model="internalValue" v-bind="configAttributes" :items="options" />
`
});

export const FilterComponent = defineComponent({
    name: "DefinedValueField.Filter",

    components: {
        EditComponent
    },

    props: getFieldEditorProps(),

    setup(props, { emit }) {
        const internalValue = useVModelPassthrough(props, "modelValue", emit);

        const configurationValues = ref({ ...props.configurationValues });
        configurationValues.value[ConfigurationValueKey.AllowMultiple] = "True";

        watch(() => props.configurationValues, () => {
            configurationValues.value = { ...props.configurationValues };
            configurationValues.value[ConfigurationValueKey.AllowMultiple] = "True";
        });

        return {
            internalValue,
            configurationValues
        };
    },

    template: `
<EditComponent v-model="internalValue" :configurationValues="configurationValues" />
`
});

export const ConfigurationComponent = defineComponent({
    name: "DefinedValueField.Configuration",

    components: {
        DropDownList,
        CheckBoxList,
        CheckBox,
        NumberBox
    },

    props: {
        modelValue: {
            type: Object as PropType<Record<string, string>>,
            required: true
        },
        configurationProperties: {
            type: Object as PropType<Record<string, string>>,
            required: true
        }
    },

    setup(props, { emit }) {
        // Define the properties that will hold the current selections.
        const definedTypeValue = ref("");
        const allowMultipleValues = ref(false);
        const displayDescriptions = ref(false);
        const enhanceForLongLists = ref(false);
        const includeInactive = ref(false);
        const repeatColumns = ref<number | null>(null);
        const selectableValues = ref<string[]>([]);

        /** The defined types that are available to be selected from. */
        const definedTypeItems = ref<ListItemBag[]>([]);

        /** The defined values that are available to be selected from. */
        const definedValueItems = ref<ListItemBag[]>([]);

        /** The options to show in the defined type picker. */
        const definedTypeOptions = computed((): ListItemBag[] => {
            return definedTypeItems.value;
        });

        /** The options to show in the selectable values picker. */
        const definedValueOptions = computed((): ListItemBag[] => definedValueItems.value);

        /** Determines if we have any defined values to show. */
        const hasValues = computed((): boolean => {
            return definedValueItems.value.length > 0;
        });

        /**
         * Update the modelValue property if any value of the dictionary has
         * actually changed. This helps prevent unwanted postbacks if the value
         * didn't really change - which can happen if multiple values get updated
         * at the same time.
         *
         * @returns true if a new modelValue was emitted to the parent component.
         */
        const maybeUpdateModelValue = (): boolean => {
            const newValue: Record<string, string> = {
                ...props.modelValue
            };

            // Construct the new value that will be emitted if it is different
            // than the current value.
            newValue[ConfigurationValueKey.DefinedType] = definedTypeValue.value;
            newValue[ConfigurationValueKey.SelectableValues] = selectableValues.value.join(",");
            newValue[ConfigurationValueKey.AllowMultiple] = asTrueFalseOrNull(allowMultipleValues.value) ?? "False";
            newValue[ConfigurationValueKey.DisplayDescription] = asTrueFalseOrNull(displayDescriptions.value) ?? "False";
            newValue[ConfigurationValueKey.EnhancedSelection] = asTrueFalseOrNull(enhanceForLongLists.value) ?? "False";
            newValue[ConfigurationValueKey.IncludeInactive] = asTrueFalseOrNull(includeInactive.value) ?? "False";
            newValue[ConfigurationValueKey.RepeatColumns] = repeatColumns.value?.toString() ?? "";

            // Compare the new value and the old value.
            const anyValueChanged = newValue[ConfigurationValueKey.DefinedType] !== props.modelValue[ConfigurationValueKey.DefinedType]
                || newValue[ConfigurationValueKey.SelectableValues] !== (props.modelValue[ConfigurationValueKey.SelectableValues] ?? "")
                || newValue[ConfigurationValueKey.AllowMultiple] !== (props.modelValue[ConfigurationValueKey.AllowMultiple] ?? "False")
                || newValue[ConfigurationValueKey.DisplayDescription] !== (props.modelValue[ConfigurationValueKey.DisplayDescription] ?? "False")
                || newValue[ConfigurationValueKey.EnhancedSelection] !== (props.modelValue[ConfigurationValueKey.EnhancedSelection] ?? "False")
                || newValue[ConfigurationValueKey.IncludeInactive] !== (props.modelValue[ConfigurationValueKey.IncludeInactive] ?? "False")
                || newValue[ConfigurationValueKey.RepeatColumns] !== (props.modelValue[ConfigurationValueKey.RepeatColumns] ?? "");

            // If any value changed then emit the new model value.
            if (anyValueChanged) {
                emit("update:modelValue", newValue);
                return true;
            }
            else {
                return false;
            }
        };

        /**
         * Emits the updateConfigurationValue if the value has actually changed.
         *
         * @param key The key that was possibly modified.
         * @param value The new value.
         */
        const maybeUpdateConfiguration = (key: string, value: string): void => {
            if (maybeUpdateModelValue()) {
                emit("updateConfigurationValue", key, value);
            }
        };

        // Watch for changes coming in from the parent component and update our
        // data to match the new information.
        watch(() => [props.modelValue, props.configurationProperties], () => {
            const definedTypes = props.configurationProperties[ConfigurationPropertyKey.DefinedTypes];
            const definedValues = props.configurationProperties[ConfigurationPropertyKey.DefinedValues];

            definedTypeItems.value = definedTypes ? JSON.parse(props.configurationProperties.definedTypes) as ListItemBag[] : [];
            definedValueItems.value = definedValues ? JSON.parse(props.configurationProperties.definedValues) as ListItemBag[] : [];

            definedTypeValue.value = props.modelValue.definedtype;
            allowMultipleValues.value = asBoolean(props.modelValue[ConfigurationValueKey.AllowMultiple]);
            displayDescriptions.value = asBoolean(props.modelValue[ConfigurationValueKey.DisplayDescription]);
            enhanceForLongLists.value = asBoolean(props.modelValue[ConfigurationValueKey.EnhancedSelection]);
            includeInactive.value = asBoolean(props.modelValue[ConfigurationValueKey.IncludeInactive]);
            repeatColumns.value = toNumberOrNull(props.modelValue[ConfigurationValueKey.RepeatColumns]);
            selectableValues.value = (props.modelValue[ConfigurationValueKey.SelectableValues]?.split(",") ?? []).filter(s => s !== "");
        }, {
            immediate: true
        });

        // Watch for changes in properties that require new configuration
        // properties to be retrieved from the server.
        watch([definedTypeValue, selectableValues, displayDescriptions, includeInactive], () => {
            if (maybeUpdateModelValue()) {
                emit("updateConfiguration");
            }
        });

        // Watch for changes in properties that only require a local UI update.
        watch(allowMultipleValues, () => maybeUpdateConfiguration(ConfigurationValueKey.AllowMultiple, asTrueFalseOrNull(allowMultipleValues.value) ?? "False"));
        watch(enhanceForLongLists, () => maybeUpdateConfiguration(ConfigurationValueKey.EnhancedSelection, asTrueFalseOrNull(enhanceForLongLists.value) ?? "False"));
        watch(repeatColumns, () => maybeUpdateConfiguration(ConfigurationValueKey.RepeatColumns, repeatColumns.value?.toString() ?? ""));

        return {
            allowMultipleValues,
            definedTypeValue,
            definedTypeOptions,
            definedTypeItems,
            definedValueOptions,
            displayDescriptions,
            enhanceForLongLists,
            hasValues,
            includeInactive,
            repeatColumns,
            selectableValues
        };
    },

    template: `
<div>
    <DropDownList v-model="definedTypeValue" label="Defined Type" :items="definedTypeOptions" :showBlankItem="false" />
    <CheckBox v-model="allowMultipleValues" label="Allow Multiple Values" text="Yes" help="When set, allows multiple defined type values to be selected." />
    <CheckBox v-model="displayDescriptions" label="Display Descriptions" text="Yes" help="When set, the defined value descriptions will be displayed instead of the values." />
    <CheckBox v-model="enhanceForLongLists" label="Enhance For Long Lists" text="Yes" />
    <CheckBox v-model="includeInactive" label="Include Inactive" text="Yes" />
    <NumberBox v-model="repeatColumns" label="Repeat Columns" />
    <CheckBoxList v-if="hasValues" v-model="selectableValues" label="Selectable Values" :items="definedValueOptions" :horizontal="true" />
</div>
`
});
