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
import { computed, defineComponent, ref, SetupContext, watch } from "vue";
import { getFieldConfigurationProps, getFieldEditorProps } from "./utils";
import CheckBox from "@Obsidian/Controls/checkBox.obs";
import NumberBox from "@Obsidian/Controls/numberBox.obs";
import CheckBoxList from "@Obsidian/Controls/checkBoxList.obs";
import DropDownList from "@Obsidian/Controls/dropDownList.obs";
import { toNumberOrNull } from "@Obsidian/Utility/numberUtils";
import { ConfigurationValueKey } from "./reminderTypesField.partial";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { asBoolean, asTrueFalseOrNull } from "@Obsidian/Utility/booleanUtils";
import { updateRefValue } from "@Obsidian/Utility/component";

export const EditComponent = defineComponent({
    name: "ReminderTypesField.Edit",

    components: {
        CheckBoxList,
        DropDownList
    },

    props: getFieldEditorProps(),

    setup(props, context: SetupContext) {
        const internalValue = ref(props.modelValue ? props.modelValue.split(",") : []);

        /** The options to choose from in the drop down list */
        const options = computed((): ListItemBag[] => {
            try {
                return JSON.parse(props.configurationValues[ConfigurationValueKey.Values] ?? "[]") as ListItemBag[];
            }
            catch {
                return [];
            }
        });

        const enhance = computed(() => {
            return props.configurationValues[ConfigurationValueKey.EnhancedSelection] == "True";
        });

        const repeatColumns = computed(() => {
            const repeatColumnsConfig = props.configurationValues[ConfigurationValueKey.RepeatColumns];

            return toNumberOrNull(repeatColumnsConfig) ?? 4;
        });

        watch(() => props.modelValue, () => {
            updateRefValue(internalValue, props.modelValue ? props.modelValue.split(",") : []);
        });

        watch(internalValue, () => {
            context.emit("update:modelValue", internalValue.value.join(","));
        });

        return {
            internalValue,
            options,
            repeatColumns,
            enhance
        };
    },

    template: `
<DropDownList v-if="enhance" v-model="internalValue" enhanceForLongLists multiple :items="options" />
<CheckBoxList v-else v-model="internalValue" horizontal :items="options" :repeatColumns="repeatColumns" />
`
});

export const ConfigurationComponent = defineComponent({
    name: "ReminderTypesField.Configuration",

    components: {
        CheckBox,
        NumberBox
    },

    props: getFieldConfigurationProps(),

    emit: {
        "update:modelValue": (_v: Record<string, string>) => true,
        "updateConfigurationValue": (_k: string, _v: string) => true,
        "updateConfiguration": () => true
    },

    setup(props, { emit }) {
        // Define the properties that will hold the current selections.
        const enhancedSelection = ref(false);
        const numberOfColumns = ref<number | null>(null);



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
            newValue[ConfigurationValueKey.EnhancedSelection] = asTrueFalseOrNull(enhancedSelection.value) ?? "False";
            newValue[ConfigurationValueKey.RepeatColumns] = numberOfColumns.value?.toString() ?? "";
            newValue[ConfigurationValueKey.Values] = props.modelValue[ConfigurationValueKey.Values] ?? "";

            // Compare the new value and the old value.
            const anyValueChanged = newValue[ConfigurationValueKey.EnhancedSelection] !== (props.modelValue[ConfigurationValueKey.EnhancedSelection] ?? "False")
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
            enhancedSelection.value = asBoolean(props.modelValue[ConfigurationValueKey.EnhancedSelection]);
            numberOfColumns.value = toNumberOrNull(props.modelValue[ConfigurationValueKey.RepeatColumns]);
        }, {
            immediate: true
        });

        // Watch for changes in properties that require new configuration
        // properties to be retrieved from the server.
        watch([], () => {
            if (maybeUpdateModelValue()) {
                emit("updateConfiguration");
            }
        });

        // Watch for changes in properties that only require a local UI update.
        watch(enhancedSelection, () => maybeUpdateConfiguration(ConfigurationValueKey.EnhancedSelection, asTrueFalseOrNull(enhancedSelection.value) ?? "False"));
        watch(numberOfColumns, () => maybeUpdateConfiguration(ConfigurationValueKey.RepeatColumns, numberOfColumns.value?.toString() ?? ""));

        return {
            enhancedSelection,
            numberOfColumns
        };
    },

    template: `
<div>
    <CheckBox v-model="enhancedSelection"
        label="Enhanced For Long Lists"
        help="When set, will render a searchable selection of options." />

    <NumberBox v-if="!enhancedSelection"
        v-model="numberOfColumns"
        label="Number of Columns"
        help="Select how many columns the list should use before going to the next row. If blank or 0 then 4 columns will be displayed. There is no upper limit enforced here however the block this is used in might add contraints due to available space." />
</div>
`
});
