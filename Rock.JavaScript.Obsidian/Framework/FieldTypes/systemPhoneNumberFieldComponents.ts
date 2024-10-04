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
import { ConfigurationValueKey } from "./systemPhoneNumberField.partial";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { asBoolean, asTrueOrFalseString } from "@Obsidian/Utility/booleanUtils";
import { toNumberOrNull } from "@Obsidian/Utility/numberUtils";
import { updateRefValue } from "@Obsidian/Utility/component";

export const EditComponent = defineComponent({
    name: "SystemPhoneNumberField.Edit",

    components: {
        CheckBoxList,
        DropDownList
    },

    props: getFieldEditorProps(),

    setup(props, { emit }) {
        // The internal value used by the text editor.
        const internalValue = ref<string[] | string>([]);

        // The options to choose from.
        const options = computed((): ListItemBag[] => {
            const selectedPhoneNumbers = JSON.parse(props.configurationValues[ConfigurationValueKey.Values] || "[]") as ListItemBag[];
            return selectedPhoneNumbers;
        });

        // Allow Multiple configuration value, sets control to a multi select check box list when true.
        const allowMultiple = computed((): boolean => {
            const allowMultiple = asBoolean(props.configurationValues[ConfigurationValueKey.AllowMultiple]);
            return allowMultiple;
        });

        // Number of columns to use when in checkboxlist mode.
        const numberOfColumns = computed((): number => {
            const numberOfColumns = toNumberOrNull(props.configurationValues[ConfigurationValueKey.RepeatColumns]) ?? 4;
            return numberOfColumns;
        });

        // Watch for changes from the parent component and update the client UI.
        watch(() => props.modelValue, () => {
            if (allowMultiple.value) {
                updateRefValue(internalValue, props.modelValue ? props.modelValue.split(",") : []);
            }
            else {
                updateRefValue(internalValue, props.modelValue);
            }
        }, {
            immediate: true
        });

        // Watch for changes from the client UI and update the parent component.
        watch(internalValue, () => {
            if (Array.isArray(internalValue.value)) {
                emit("update:modelValue", internalValue.value.join(","));
            }
            else {
                emit("update:modelValue", internalValue.value);
            }
        });

        return {
            internalValue,
            options,
            allowMultiple,
            numberOfColumns
        };
    },

    template: `
    <CheckBoxList v-if="allowMultiple" v-model="internalValue" :items="options" :horizontal="true" :repeatColumns="numberOfColumns" />
    <DropDownList v-else v-model="internalValue" :items="options" :showBlankItem="true" enhanceForLongLists />
`
});

export const ConfigurationComponent = defineComponent({
    name: "SystemPhoneNumberField.Configuration",

    components: {
        CheckBox,
        NumberBox
    },

    props: getFieldConfigurationProps(),

    emits: ["update:modelValue", "updateConfiguration", "updateConfigurationValue"],

    setup(props, { emit }) {
        // Define the properties that will hold the current selections.
        const allowMultiple = ref<boolean>();
        const includeInactive = ref<boolean>();
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
            newValue[ConfigurationValueKey.AllowMultiple] = asTrueOrFalseString(allowMultiple.value);
            newValue[ConfigurationValueKey.IncludeInactive] = asTrueOrFalseString(includeInactive.value);
            newValue[ConfigurationValueKey.RepeatColumns] = numberOfColumns.value?.toString() ?? "";
            newValue[ConfigurationValueKey.Values] = props.modelValue[ConfigurationValueKey.Values];

            // Compare the new value and the old value.
            const anyValueChanged = newValue[ConfigurationValueKey.AllowMultiple] !== (props.modelValue[ConfigurationValueKey.AllowMultiple])
                || newValue[ConfigurationValueKey.IncludeInactive] !== (props.modelValue[ConfigurationValueKey.IncludeInactive])
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
            allowMultiple.value = asBoolean(props.modelValue[ConfigurationValueKey.AllowMultiple]);
            includeInactive.value = asBoolean(props.modelValue[ConfigurationValueKey.IncludeInactive]);
            numberOfColumns.value = toNumberOrNull(props.modelValue[ConfigurationValueKey.RepeatColumns]);
        }, {
            immediate: true
        });

        // Watch for changes in properties that require new configuration
        // properties to be retrieved from the server.
        watch([includeInactive], () => {
            if (maybeUpdateModelValue()) {
                emit("updateConfiguration");
            }
        });

        // Watch for changes in properties that only require a local UI update.
        watch(allowMultiple, () => maybeUpdateConfiguration(ConfigurationValueKey.AllowMultiple, asTrueOrFalseString(allowMultiple.value)));
        watch(includeInactive, () => maybeUpdateConfiguration(ConfigurationValueKey.IncludeInactive, asTrueOrFalseString(includeInactive.value)));
        watch(numberOfColumns, () => maybeUpdateConfiguration(ConfigurationValueKey.RepeatColumns, numberOfColumns.value?.toString() ?? ""));

        return {
            allowMultiple,
            includeInactive,
            numberOfColumns
        };
    },

    template: `
<CheckBox v-model="allowMultiple" label="Allow Multiple Values" help="When set, allows multiple system phone numbers to be selected." />
<CheckBox v-model="includeInactive" label="Include Inactive" help="When set, inactive system phone numbers will be included in the list." />
<NumberBox v-model="numberOfColumns" label="Repeat Columns" help="Select how many columns the list should use before going to the next row. If blank or 0 then 4 columns will be displayed. There is no upper limit enforced here however the block this is used in might add constraints due to available space." />
`
});
