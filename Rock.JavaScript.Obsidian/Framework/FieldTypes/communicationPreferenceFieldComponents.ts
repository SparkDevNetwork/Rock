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

import { computed, defineComponent, ref, watch } from "vue";
import { getFieldEditorProps, getFieldConfigurationProps } from "./utils";
import TextBox from "@Obsidian/Controls/textBox.obs";
import RadioButtonList from "@Obsidian/Controls/radioButtonList.obs";
import { ConfigurationValueKey } from "./communicationPreferenceField.partial";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";

export const EditComponent = defineComponent({
    name: "CommunicationPreferenceField.Edit",

    components: {
        RadioButtonList
    },

    props: getFieldEditorProps(),

    setup(props, { emit }) {

        const internalValue = ref("");

        watch(() => props.modelValue, () => {
            internalValue.value = props.modelValue;
        }, { immediate: true });

        watch(() => internalValue.value, () => {
            emit("update:modelValue", internalValue.value);
        });

        const options = computed((): ListItemBag[] | null | undefined => {
            return JSON.parse(props.configurationValues[ConfigurationValueKey.Options] ?? "[]") as ListItemBag[];
        });

        const repeatColumns = computed((): string => {
            return props.configurationValues[ConfigurationValueKey.RepeatColumns];
        });

        return {
            internalValue,
            options,
            repeatColumns
        };
    },

    template: `
    <RadioButtonList v-model="internalValue" :items="options" horizontal :repeatColumns="repeatColumns" />
`
});


export const ConfigurationComponent = defineComponent({
    name: "CommunicationPreferenceField.Configuration",

    components: {
        TextBox
    },

    props: getFieldConfigurationProps(),

    emits: [
        "update:modelValue",
        "updateConfigurationValue"
    ],

    setup(props, { emit }) {
        const columns = ref("");
        const options = ref("");

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
            newValue[ConfigurationValueKey.RepeatColumns] = columns.value;
            newValue[ConfigurationValueKey.Options] = options.value;

            // Compare the new value and the old value.
            const anyValueChanged = newValue[ConfigurationValueKey.RepeatColumns] !== (props.modelValue[ConfigurationValueKey.RepeatColumns] ?? "");

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
            columns.value = props.modelValue[ConfigurationValueKey.RepeatColumns];
            options.value = props.modelValue[ConfigurationValueKey.Options];
        }, {
            immediate: true
        });

        watch(columns, val => maybeUpdateConfiguration(ConfigurationValueKey.RepeatColumns, val ?? ""));

        return {
            columns
        };
    },

    template: `
    <TextBox v-model="columns" label="Columns" help="Select how many columns the list should use before going to the next row. If blank or 0 then 4 columns will be displayed. There is no upper limit enforced here however the block this is used in might add contraints due to available space." />
`
});
