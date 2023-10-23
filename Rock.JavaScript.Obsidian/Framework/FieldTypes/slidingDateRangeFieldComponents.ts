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
import { defineComponent, ref, watch, computed } from "vue";
import { getFieldEditorProps, getFieldConfigurationProps } from "./utils";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import SlidingDateRangePicker from "@Obsidian/Controls/slidingDateRangePicker.obs";
import CheckBoxList from "@Obsidian/Controls/checkBoxList.obs";
import { ConfigurationValueKey } from "./slidingDateRangeField.partial";
import { parseSlidingDateRangeString, slidingDateRangeToString } from "@Obsidian/Utility/slidingDateRange";
import { updateRefValue } from "@Obsidian/Utility/component";
import { toNumber } from "@Obsidian/Utility/numberUtils";

export const EditComponent = defineComponent({
    name: "SlidingDateRangeField.Edit",

    components: {
        SlidingDateRangePicker
    },

    props: getFieldEditorProps(),
    setup(props, { emit }) {
        // Internal values
        const internalValue = ref(parseSlidingDateRangeString(props.modelValue || ""));

        /** The options to choose from in the drop down list */
        const enabledSlidingDateRangeTypes = computed((): string[] => {
            try {
                return props.configurationValues[ConfigurationValueKey.EnabledSlidingDateRangeTypes]?.split(",") ;
            }
            catch {
                return [];
            }
        });

        const enabledSlidingDateRangeUnits = computed((): number[] => {
            try {
                return props.configurationValues[ConfigurationValueKey.EnabledSlidingDateRangeUnits]?.split(",").filter(v => v !== "").map(a=>toNumber(a));
            }
            catch {
                return [];
            }
        });

        watch(() => props.modelValue, () => {
            updateRefValue(internalValue, parseSlidingDateRangeString(props.modelValue || ""));
        });

        watch(internalValue, () => {
            emit("update:modelValue", internalValue.value ? slidingDateRangeToString(internalValue.value) : "");
        });

        return {
            internalValue,
            enabledSlidingDateRangeUnits,
            enabledSlidingDateRangeTypes
        };
    },
    template: `
<SlidingDateRangePicker v-model="internalValue" label="Sliding Date Range" :enabledTimeUnits="enabledSlidingDateRangeUnits" :enabledSlidingDateRangeUnits="enabledSlidingDateRangeTypes" previewLocation="Right" />
`
});

export const ConfigurationComponent = defineComponent({
    name: "SlidingDateRangeField.Configuration",

    components: {
        CheckBoxList
    },

    props: getFieldConfigurationProps(),

    emits: [
        "update:modelValue",
        "updateConfiguration",
        "updateConfigurationValue"
    ],

    setup(props, { emit }) {
        const enabledSlidingDateRangeTypes = ref<string[]>([]);
        const enabledSlidingDateRangeUnits = ref<string[]>([]);


        /** The sliding Date Range types options to choose from in the check box list */
        const slidingDateRangeTypeOptions = computed((): ListItemBag[] => {
            try {
                return JSON.parse(props.configurationProperties[ConfigurationValueKey.SlidingDateRangeTypes] ?? "[]") as ListItemBag[];
            }
            catch {
                return [];
            }
        });

        /** The sliding Date Range units options to choose from in the check box list */
        const slidingDateRangeUnitOptions = computed((): ListItemBag[] => {
            try {
                return JSON.parse(props.configurationProperties[ConfigurationValueKey.TimeUnitTypes] ?? "[]") as ListItemBag[];
            }
            catch {
                return [];
            }
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
            newValue[ConfigurationValueKey.EnabledSlidingDateRangeTypes] = enabledSlidingDateRangeTypes.value.join(",");
            newValue[ConfigurationValueKey.EnabledSlidingDateRangeUnits] = enabledSlidingDateRangeUnits.value.join(",");

            // Compare the new value and the old value.
            const anyValueChanged = newValue[ConfigurationValueKey.EnabledSlidingDateRangeTypes] !== (props.modelValue[ConfigurationValueKey.EnabledSlidingDateRangeTypes] ?? "")
                || newValue[ConfigurationValueKey.EnabledSlidingDateRangeUnits] !== (props.modelValue[ConfigurationValueKey.EnabledSlidingDateRangeUnits] ?? "");

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
            enabledSlidingDateRangeTypes.value =  (props.modelValue[ConfigurationValueKey.EnabledSlidingDateRangeTypes]?.split(",") ?? []).filter(s => s !== "");
            enabledSlidingDateRangeUnits.value =  (props.modelValue[ConfigurationValueKey.EnabledSlidingDateRangeUnits]?.split(",") ?? []).filter(s => s !== "");
        }, {
            immediate: true
        });

        // Watch for changes in properties that require new configuration
        // properties to be retrieved from the server.
        // THIS IS JUST A PLACEHOLDER FOR COPYING TO NEW FIELDS THAT MIGHT NEED IT.
        // THIS FIELD DOES NOT NEED THIS
        watch([], () => {
            if (maybeUpdateModelValue()) {
                emit("updateConfiguration");
            }
        });

        // Watch for changes in properties that only require a local UI update.
        watch(enabledSlidingDateRangeTypes, () => maybeUpdateConfiguration(ConfigurationValueKey.EnabledSlidingDateRangeTypes, enabledSlidingDateRangeTypes.value.join(",")));
        watch(enabledSlidingDateRangeUnits, () => maybeUpdateConfiguration(ConfigurationValueKey.EnabledSlidingDateRangeUnits, enabledSlidingDateRangeUnits.value.join(",")));

        return {
            enabledSlidingDateRangeTypes,
            enabledSlidingDateRangeUnits,
            slidingDateRangeTypeOptions,
            slidingDateRangeUnitOptions
        };
    },
    template: `
<div>
<CheckBoxList v-model="enabledSlidingDateRangeTypes"
    label="Enabled Sliding Date Range Types"
    help="Select specific types or leave all blank to use all of them."
    :items="slidingDateRangeTypeOptions"
    horizontal />
<CheckBoxList v-model="enabledSlidingDateRangeUnits"
    label="Enabled Sliding Date Range Units"
    help="Select specific units or leave all blank to use all of them."
    :items="slidingDateRangeUnitOptions"
    horizontal />
</div>
`
});