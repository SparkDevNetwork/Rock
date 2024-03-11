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
import { defineComponent, inject, ref, watch } from "vue";
import CheckBoxList from "@Obsidian/Controls/checkBoxList.obs";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import NumberBox from "@Obsidian/Controls/numberBox.obs";
import DropDownList from "@Obsidian/Controls/dropDownList.obs";
import CheckBox from "@Obsidian/Controls/checkBox.obs";
import { ConfigurationValueKey } from "./connectionTypesField.partial";
import { getFieldEditorProps, getFieldConfigurationProps } from "./utils";
import { asBoolean, asTrueFalseOrNull } from "@Obsidian/Utility/booleanUtils";
import { toNumberOrNull } from "@Obsidian/Utility/numberUtils";

export const EditComponent = defineComponent({
    name: "CheckinConfigurationTypeField.Edit",

    components: {
        CheckBoxList,
        DropDownList
    },

    props: getFieldEditorProps(),

    setup() {
        return {
            isRequired: inject("isRequired") as boolean
        };
    },

    data() {
        return {
            internalValue: this.modelValue ? this.modelValue.split(",") : []
        };
    },

    computed: {
        options(): ListItemBag[] {
            try {
                const valuesConfig = JSON.parse(this.configurationValues[ConfigurationValueKey.Values] ?? "[]") as ListItemBag[];
                return valuesConfig.map(v => {
                    return {
                        text: v.text,
                        value: v.value
                    } as ListItemBag;
                });
            }
            catch {
                return [];
            }
        },

        repeatColumns(): number {
            return Number(this.configurationValues[ConfigurationValueKey.RepeatColumns]) ?? 1;
        },
        enhance(): boolean {
            return this.configurationValues[ConfigurationValueKey.EnhancedForLongLists] == "True";
        }
    },

    watch: {
        internalValue() {
            this.$emit("update:modelValue", this.internalValue.join(","));
        }
    },

    template: `
    <DropDownList v-if="enhance" v-model="internalValue" enhanceForLongLists multiple :items="options" />
<CheckBoxList v-else v-model="internalValue" :items="options" :repeatColumns="repeatColumns" horizontal />
`
});

export const ConfigurationComponent = defineComponent({
    name: "CheckinConfigurationTypeField.Configuration",
    components: {
        CheckBox,
        NumberBox
    },

    props: getFieldConfigurationProps(),

    setup(props, { emit }) {
        // Define the properties that will hold the current selections.
        const enhanceForLongLists = ref(true);
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
            const newValue: Record<string, string> = {
                ...props.modelValue
            };

            // Construct the new value that will be emitted if it is different
            // than the current value.
            newValue[ConfigurationValueKey.EnhancedForLongLists] = asTrueFalseOrNull(enhanceForLongLists.value) ?? "True";
            newValue[ConfigurationValueKey.RepeatColumns] = repeatColumns.value?.toString() ?? "";
            // Compare the new value and the old value.
            const anyValueChanged = newValue[ConfigurationValueKey.EnhancedForLongLists] !== (props.modelValue[ConfigurationValueKey.EnhancedForLongLists] ?? "True")
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
            enhanceForLongLists.value = asBoolean(props.modelValue[ConfigurationValueKey.EnhancedForLongLists]);
            repeatColumns.value = toNumberOrNull(props.modelValue[ConfigurationValueKey.RepeatColumns]);
        }, {
            immediate: true
        });

        // Watch for changes in properties that require new configuration
        // properties to be retrieved from the server.
        watch([enhanceForLongLists, repeatColumns], () => {
            if(repeatColumns.value === null) {
                // The value of the repeatColumns field happens to be null when it is being updated by the individual in the remote device.
                // Updating the configuration now might interfere with the update being made in the remote device.
                return;
            }
            if (maybeUpdateModelValue()) {
                emit("updateConfiguration");
            }
        });

        // Watch for changes in properties that only require a local UI update.
        watch(enhanceForLongLists, () => maybeUpdateConfiguration(ConfigurationValueKey.EnhancedForLongLists, asTrueFalseOrNull(enhanceForLongLists.value) ?? "True"));
        watch(repeatColumns, () => maybeUpdateConfiguration(ConfigurationValueKey.RepeatColumns, repeatColumns.value?.toString() ?? ""));

        return {
            enhanceForLongLists,
            repeatColumns
        };
    },

    template: `
<div>
    <CheckBox v-model="enhanceForLongLists" label="Enhance For Long Lists" text="Yes" help="When set, will render a searchable selection of options." />
    <NumberBox v-if="!enhanceForLongLists" v-model="repeatColumns" label="Number of Columns" help="Select how many columns the list should use before going to the next row. If blank or 0 then 4 columns will be displayed. There is no upper limit enforced here however the block this is used in might add contraints due to available space." />
</div>
`
});