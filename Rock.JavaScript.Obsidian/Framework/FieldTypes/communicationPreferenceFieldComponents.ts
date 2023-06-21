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
import { defineComponent, ref, watch } from "vue";
import { getFieldEditorProps, getFieldConfigurationProps } from "./utils";
import RadioButtonList from "@Obsidian/Controls/radioButtonList";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import NumberBox from "@Obsidian/Controls/numberBox";
import { toNumberOrNull } from "@Obsidian/Utility/numberUtils";
import { ConfigurationValueKey } from "./communicationPreferenceField.partial";

export const EditComponent = defineComponent({
    name: "CommunicationPreferenceField.Edit",
    components: {
        RadioButtonList
    },
    props: getFieldEditorProps(),
    data() {
        return {
            /** The user input value as a number of null if it isn't valid. */
            internalValue: ""
        };
    },
    computed: {
        /** The options to choose from in the drop down list */
        options(): ListItemBag[] {
            try {
                const providedOptions = JSON.parse(this.configurationValues[ConfigurationValueKey.RepeatColumns] ?? "[]") as ListItemBag[];


                return providedOptions;
            }
            catch {
                return [];
            }
        },

        /** Any additional attributes that will be assigned to the radio button control */
        rbConfigAttributes(): Record<string, number | boolean> {
            const attributes: Record<string, number | boolean> = {};
            const repeatColumnsConfig = this.configurationValues[ConfigurationValueKey.RepeatColumns];

            if (repeatColumnsConfig) {
                attributes["repeatColumns"] = toNumberOrNull(repeatColumnsConfig) || 0;
            }

            return attributes;
        },
    },
    watch: {
        internalValue() {
            this.$emit("update:modelValue", this.internalValue);
        },

        modelValue: {
            immediate: true,
            handler() {
                this.internalValue = this.modelValue || "";
            }
        }
    },
    template: `
<RadioButtonList v-model="internalValue" v-bind="rbConfigAttributes" :items="options" horizontal />
`
});


export const ConfigurationComponent = defineComponent({
    name: "CommunicationPreferenceField.Configuration",

    components: {
        NumberBox
    },

    props: getFieldConfigurationProps(),

    emits: [
        "update:modelValue",
        "updateConfigurationValue"
    ],

    setup(props, { emit }) {
        // Define the properties that will hold the current selections.
        const repeatColumns = ref<number | null>(null);

        /**
         * Update the modelValue property if any value of the dictionary has
         * actually changed. This helps prevent unwanted postbacks if the value
         * didn't really change - which can happen if multiple values get updated
         * at the same time.
         *
         * @returns true if a new modelValue was emitted to the parent component.
         */
        const maybeUpdateModelValue = (): boolean => {
            const newValue: Record<string, string> = { ...props.modelValue };

            // Construct the new value that will be emitted if it is different
            // than the current value.
            newValue[ConfigurationValueKey.RepeatColumns] = repeatColumns.value?.toString() ?? "";

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
            repeatColumns.value = toNumberOrNull(props.modelValue[ConfigurationValueKey.RepeatColumns]);
        }, {
            immediate: true
        });

        watch(repeatColumns, () => maybeUpdateConfiguration(ConfigurationValueKey.RepeatColumns, repeatColumns.value?.toString() ?? ""));

        return {
            repeatColumns
        };
    },

    template: `
<div>
    <NumberBox v-if="isRadioList"
        v-model="repeatColumns"
        label="Columns"
        help="Select how many columns the list should use before going to the next row. If blank or 0 then 4 columns will be displayed. There is no enforced upper limit however the block this control is used in might add contraints due to available space." />
</div>
`
});
