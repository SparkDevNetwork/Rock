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
import CheckBox from "@Obsidian/Controls/checkBox.obs";
import CheckBoxList from "@Obsidian/Controls/checkBoxList.obs";
import NumberBox from "@Obsidian/Controls/numberBox.obs";
import DropDownList from "@Obsidian/Controls/dropDownList.obs";
import { ConfigurationValueKey } from "./badgesField.partial";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { asBoolean, asTrueOrFalseString } from "@Obsidian/Utility/booleanUtils";
import { toNumberOrNull } from "@Obsidian/Utility/numberUtils";
import { updateRefValue } from "@Obsidian/Utility/component";

export const EditComponent = defineComponent({
    name: "BinaryFileTypesField.Edit",

    components: {
        CheckBoxList,
        DropDownList
    },

    props: getFieldEditorProps(),

    setup(props, { emit }) {
        // The internal value used by the text editor.
        const internalValue = ref<string[]>([]);

        // The options to choose from.
        const options = computed((): ListItemBag[] => {
            const selectedBadges = JSON.parse(props.configurationValues[ConfigurationValueKey.ClientValues] || "[]") as ListItemBag[];
            return selectedBadges;
        });

        // Enhance selection configuration value, sets control to a searchable dropdownlist when true.
        const enhancedSelection = computed((): boolean => {
            const enhancedselection = asBoolean(props.configurationValues[ConfigurationValueKey.EnhancedSelection]);
            return enhancedselection;
        });

        // Number of columns to use when in checkboxlist mode.
        const numberOfColumns = computed((): number => {
            const numberOfColumns = toNumberOrNull(props.configurationValues[ConfigurationValueKey.RepeatColumns]) ?? 4;
            return numberOfColumns;
        });

        // Watch for changes from the parent component and update the text editor.
        watch(() => props.modelValue, () => {
            updateRefValue(internalValue, props.modelValue ? props.modelValue.split(",") : []);
        }, {
            immediate: true
        });

        // Watch for changes from the text editor and update the parent component.
        watch(internalValue, () => {
            emit("update:modelValue", internalValue.value.join(","));
        });

        return {
            internalValue,
            options,
            enhancedSelection,
            numberOfColumns
        };
    },

    template: `
    <CheckBoxList v-if="!enhancedSelection" v-model="internalValue" :items="options" :horizontal="true" :repeatColumns="numberOfColumns" />
    <DropDownList v-else v-model="internalValue" :items="options" :showBlankItem="true" enhanceForLongLists multiple />
`
});

export const ConfigurationComponent = defineComponent({
    name: "BinaryFileTypesField.Configuration",

    components: {
        CheckBox,
        NumberBox
    },

    props: getFieldConfigurationProps(),

    emits: ["update:modelValue", "updateConfiguration", "updateConfigurationValue"],

    setup(props, { emit }) {
        // Define the properties that will hold the current selections.
        const enhancedSelection = ref<boolean>();
        const numberOfColumns = ref<number | null>();

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
            newValue[ConfigurationValueKey.EnhancedSelection] = asTrueOrFalseString(enhancedSelection.value);
            newValue[ConfigurationValueKey.RepeatColumns] = numberOfColumns.value?.toString() ?? "";
            newValue[ConfigurationValueKey.ClientValues] = props.modelValue[ConfigurationValueKey.ClientValues];

            // Compare the new value and the old value.
            const anyValueChanged = newValue[ConfigurationValueKey.EnhancedSelection] !== (props.modelValue[ConfigurationValueKey.EnhancedSelection])
                || newValue[ConfigurationValueKey.RepeatColumns] !== (props.modelValue[ConfigurationValueKey.RepeatColumns]);

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
            enhancedSelection.value = asBoolean(props.modelValue[ConfigurationValueKey.EnhancedSelection]);
            numberOfColumns.value = toNumberOrNull(props.modelValue[ConfigurationValueKey.RepeatColumns]);
        }, {
            immediate: true
        });

        // Watch for changes in properties that only require a local UI update.
        watch(enhancedSelection, () => maybeUpdateConfiguration(ConfigurationValueKey.EnhancedSelection, asTrueOrFalseString(enhancedSelection.value)));
        watch(numberOfColumns, () => maybeUpdateConfiguration(ConfigurationValueKey.RepeatColumns, numberOfColumns.value?.toString() ?? ""));

        return {
            enhancedSelection,
            numberOfColumns
        };
    },

    template: `
<CheckBox v-model="enhancedSelection" label="Enhance For Long Lists" help="When set, will render a searchable selection of options." />
<NumberBox v-if="!enhancedSelection" label="Number of Columns" v-model="numberOfColumns" help="Select how many columns the list should use before going to the next row. If blank or 0 then 4 columns will be displayed. There is no upper limit enforced here however the block this is used in might add contraints due to available space." />
`
});
