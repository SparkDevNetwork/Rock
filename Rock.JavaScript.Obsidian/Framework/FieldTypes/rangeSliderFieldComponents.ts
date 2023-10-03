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
import { toNumberOrNull } from "@Obsidian/Utility/numberUtils";
import RangeSlider from "@Obsidian/Controls/rangeSlider.obs";
import NumberBox from "@Obsidian/Controls/numberBox.obs";
import { ConfigurationValueKey } from "./rangeSliderField.partial";

export const EditComponent = defineComponent({
    name: "RangeSliderField.Edit",

    components: {
        RangeSlider
    },

    props: getFieldEditorProps(),

    data() {
        return {
            /** The user input value as a number of null if it isn't valid. */
            internalValue: null as number | null
        };
    },
    computed: {
        minValue(): number {
            const minValueConfig = this.configurationValues[ConfigurationValueKey.MinValue];

            return toNumberOrNull(minValueConfig) || 0;
        },
        maxValue(): number {
            const maxValueConfig = this.configurationValues[ConfigurationValueKey.MaxValue];

            return toNumberOrNull(maxValueConfig) || 100;
        },

    },
    watch: {
        /**
         * Watch for changes to internalValue and emit the new value out to
         * the consuming component.
         */
        internalValue(): void {
            this.$emit("update:modelValue", this.internalValue !== null ? this.internalValue.toString() : "");
        },

        /**
         * Watch for changes to modelValue which indicate the component
         * using us has given us a new value to work with.
         */
        modelValue: {
            immediate: true,
            handler(): void {
                this.internalValue = toNumberOrNull(this.modelValue || "");
            }
        }
    },

    template: `
<RangeSlider v-model="internalValue" rules="integer" :min="minValue" :max="maxValue" />
`
});

export const ConfigurationComponent = defineComponent({
    name: "RangeSliderField.Configuration",

    components: {
        NumberBox
    },

    props: getFieldConfigurationProps(),

    emits: [
        "update:modelValue",
        "updateConfiguration",
        "updateConfigurationValue"
    ],

    setup(props, { emit }) {
        // Define the properties that will hold the current selections.
        const minValue = ref<number | null>(null);
        const maxValue = ref<number | null>(null);

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
            newValue[ConfigurationValueKey.MinValue] = minValue.value?.toString() ?? "";
            newValue[ConfigurationValueKey.MaxValue] = maxValue.value?.toString() ?? "";

            // Compare the new value and the old value.
            const anyValueChanged = newValue[ConfigurationValueKey.MinValue] !== (props.modelValue[ConfigurationValueKey.MinValue] ?? "")
                || newValue[ConfigurationValueKey.MaxValue] !== (props.modelValue[ConfigurationValueKey.MaxValue] ?? "");

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
            minValue.value = toNumberOrNull(props.modelValue[ConfigurationValueKey.MinValue]);
            maxValue.value = toNumberOrNull(props.modelValue[ConfigurationValueKey.MaxValue]);
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
        watch(minValue, () => maybeUpdateConfiguration(ConfigurationValueKey.MinValue, minValue.value?.toString() ?? ""));
        watch(maxValue, () => maybeUpdateConfiguration(ConfigurationValueKey.MaxValue, maxValue.value?.toString() ?? ""));

        return {
            minValue,
            maxValue
        };
    },

    template: `
<div>
    <NumberBox v-model="minValue" label="Min Value" help="The minimum value allowed for the slider range." />
    <NumberBox v-model="maxValue" label="Max Value" help="The maximum value allowed for the slider range." />
</div>
`
});

