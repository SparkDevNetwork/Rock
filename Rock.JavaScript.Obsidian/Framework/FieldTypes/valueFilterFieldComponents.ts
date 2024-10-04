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

import { computed, defineComponent, ref, watch } from "vue";
import { getFieldConfigurationProps, getFieldEditorProps } from "./utils";
import { ConfigurationPropertyKey, ConfigurationValueKey } from "./valueFilterField.partial";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import ValueFilter from "@Obsidian/Controls/valueFilter.obs";
import CheckBoxList from "@Obsidian/Controls/checkBoxList.obs";
import CheckBox from "@Obsidian/Controls/checkBox.obs";
import { asBoolean, asTrueOrFalseString } from "@Obsidian/Utility/booleanUtils";
import { CompoundFilterExpression } from "@Obsidian/ViewModels/Controls/valueFilter.d";

export const EditComponent = defineComponent({
    name: "ValueFilterField.Edit",

    components: {
        ValueFilter
    },

    props: getFieldEditorProps(),

    setup(props, { emit }) {
        // The internal value used by the text editor.
        const internalValue = ref<CompoundFilterExpression>({} as CompoundFilterExpression);

        const hideFilterMode = computed((): boolean => {
            return asBoolean(props.configurationValues[ConfigurationValueKey.HideFilterMode]);
        });

        // Watch for changes from the parent component and update the text editor.
        watch(() => props.modelValue, () => {
            internalValue.value = JSON.parse(props.modelValue || "{}") as CompoundFilterExpression;
        }, {
            immediate: true
        });

        // Watch for changes from the text editor and update the parent component.
        watch(internalValue, () => {
            emit("update:modelValue", JSON.stringify(internalValue.value));
        });

        return {
            internalValue,
            hideFilterMode,
        };
    },

    template: `
    <ValueFilter v-model="internalValue" :hideFilterMode="hideFilterMode" />
`
});

export const ConfigurationComponent = defineComponent({
    name: "ValueFilterField.Configuration",

    components: {
        CheckBox,
        CheckBoxList,
    },

    props: getFieldConfigurationProps(),

    emits: ["update:modelValue", "updateConfiguration", "updateConfigurationValue"],

    setup(props, { emit }) {
        // Define the properties that will hold the current selections.
        const hideFilterMode = ref<boolean>();
        const comparisonTypes = ref<string[]>([]);

        // The options for the comparison types dropdown list
        const comparisonTypesOptions = computed((): ListItemBag[] => {
            const comparisonTypesOptions = props.configurationProperties[ConfigurationPropertyKey.ComparisonTypesOptions];
            return comparisonTypesOptions ? JSON.parse(comparisonTypesOptions) as ListItemBag[] : [];
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
            const newValue: Record<string, string> = {};

            // Construct the new value that will be emitted if it is different
            // than the current value.
            newValue[ConfigurationValueKey.HideFilterMode] = asTrueOrFalseString(hideFilterMode.value);
            newValue[ConfigurationValueKey.ComparisonTypes] = JSON.stringify(comparisonTypes.value);

            // Compare the new value and the old value.
            const anyValueChanged = newValue[ConfigurationValueKey.ComparisonTypes] !== (props.modelValue[ConfigurationValueKey.ComparisonTypes])
                || newValue[ConfigurationValueKey.HideFilterMode] !== (props.modelValue[ConfigurationValueKey.HideFilterMode]);

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
            hideFilterMode.value = asBoolean(props.modelValue[ConfigurationValueKey.HideFilterMode]);
            comparisonTypes.value = JSON.parse(props.modelValue[ConfigurationValueKey.ComparisonTypes] || "[]");
        }, {
            immediate: true
        });

        // Watch for changes in properties that only require a local UI update.
        watch(hideFilterMode, () => maybeUpdateConfiguration(ConfigurationValueKey.HideFilterMode, asTrueOrFalseString(hideFilterMode.value)));
        watch(comparisonTypes, () => maybeUpdateConfiguration(ConfigurationValueKey.ComparisonTypes, JSON.stringify(comparisonTypes.value)));

        return {
            hideFilterMode,
            comparisonTypes,
            comparisonTypesOptions
        };
    },

    template: `
    <CheckBox v-model="hideFilterMode" label="Hide Filter mode" help="When set, filter mode will be hidden." />
    <CheckBoxList v-model="comparisonTypes" label="Comparison Types" rules="required" :items="comparisonTypesOptions" :repeatColumns="2" help="The comparison types the user can select from." />
`
});
