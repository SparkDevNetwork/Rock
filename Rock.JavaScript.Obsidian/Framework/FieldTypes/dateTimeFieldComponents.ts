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
import { computed, defineComponent, PropType, ref, watch } from "vue";
import { getFieldEditorProps, getFieldConfigurationProps } from "./utils";
import { asBoolean, asTrueFalseOrNull } from "@Obsidian/Utility/booleanUtils";
import { ConfigurationKey } from "./dateTimeField.partial";
import SlidingDateRangePicker from "@Obsidian/Controls/slidingDateRangePicker.obs";
import DateTimePicker from "@Obsidian/Controls/dateTimePicker.obs";
import TextBox from "@Obsidian/Controls/textBox.obs";
import CheckBox from "@Obsidian/Controls/checkBox.obs";
import { ComparisonType } from "@Obsidian/Enums/Reporting/comparisonType";
import { parseSlidingDateRangeString, slidingDateRangeToString } from "@Obsidian/Utility/slidingDateRange";
import { updateRefValue } from "@Obsidian/Utility/component";

export const EditComponent = defineComponent({
    name: "DateTimeField.Edit",

    components: {
        DateTimePicker
    },

    props: getFieldEditorProps(),

    setup() {
        return {
        };
    },

    data() {
        return {
            internalValue: "",
            formattedString: ""
        };
    },

    methods: {
        async syncModelValue(): Promise<void> {
            this.internalValue = this.modelValue ?? "";
        },
    },

    computed: {
        dateFormatTemplate(): string {
            const formatConfig = this.configurationValues[ConfigurationKey.Format];
            return formatConfig || "MM/dd/yyyy";
        },

        configAttributes(): Record<string, number | boolean> {
            const attributes: Record<string, number | boolean> = {};

            const displayCurrentConfig = this.configurationValues[ConfigurationKey.DisplayCurrentOption];
            const displayCurrent = asBoolean(displayCurrentConfig);
            attributes.displayCurrentOption = displayCurrent;
            attributes.isCurrentDateOffset = displayCurrent;

            return attributes;
        }
    },

    watch: {
        internalValue(): void {
            if (this.internalValue !== this.modelValue) {
                const d1 = Date.parse(this.internalValue);
                const d2 = Date.parse(this.modelValue ?? "");

                if (isNaN(d1) || isNaN(d2) || d1 !== d2) {
                    this.$emit("update:modelValue", this.internalValue);
                }
            }
        },

        modelValue: {
            immediate: true,
            async handler(): Promise<void> {
                await this.syncModelValue();
            }
        }
    },
    template: `
<DateTimePicker v-model="internalValue" v-bind="configAttributes" />
`
});

export const FilterComponent = defineComponent({
    name: "DateField.Filter",

    components: {
        EditComponent,
        SlidingDateRangePicker
    },

    props: {
        ...getFieldEditorProps(),
        comparisonType: {
            type: Number as PropType<ComparisonType | null>,
            required: true
        }
    },

    emits: [
        "update:modelValue"
    ],

    setup(props, { emit }) {
        // The internal values that make up the model value.
        const internalValue = ref(props.modelValue);
        const internalValueSegments = internalValue.value.split("\t");
        const dateValue = ref(internalValueSegments[0]);
        const rangeValue = ref(parseSlidingDateRangeString(internalValueSegments.length > 1 ? internalValueSegments[1] : ""));

        // Get the configuration values and force the DisplayCurrentOption to True.
        const configurationValues = ref({ ...props.configurationValues });
        configurationValues.value[ConfigurationKey.DisplayCurrentOption] = "True";

        /** True if the comparison type is of type Between. */
        const isComparisonTypeBetween = computed((): boolean => props.comparisonType === ComparisonType.Between);

        // Watch for changes in the configuration values and update our own list.
        watch(() => props.configurationValues, () => {
            configurationValues.value = { ...props.configurationValues };
            configurationValues.value[ConfigurationKey.DisplayCurrentOption] = "True";
        });

        // Watch for changes from the standard DatePicker.
        watch(dateValue, () => {
            if (props.comparisonType !== ComparisonType.Between) {
                internalValue.value = `${dateValue.value}\t`;
            }
        });

        // Watch for changes from the SlidingDateRangePicker.
        watch(rangeValue, () => {
            if (props.comparisonType === ComparisonType.Between) {
                internalValue.value = `\t${rangeValue.value ? slidingDateRangeToString(rangeValue.value) : ""}`;
            }
        });

        // Watch for changes to the model value and update our internal values.
        watch(() => props.modelValue, () => {
            internalValue.value = props.modelValue;
            const segments = internalValue.value.split("\t");
            dateValue.value = segments[0];
            updateRefValue(rangeValue, parseSlidingDateRangeString(segments.length > 1 ? segments[1] : ""));
        });

        // Watch for changes to our internal value and update the model value.
        watch(internalValue, () => {
            emit("update:modelValue", internalValue.value);
        });

        return {
            configurationValues,
            dateValue,
            isComparisonTypeBetween,
            rangeValue
        };
    },

    template: `
<SlidingDateRangePicker v-if="isComparisonTypeBetween" v-model="rangeValue" />
<EditComponent v-else v-model="dateValue" :configurationValues="configurationValues" />
`
});

const defaults = {
    [ConfigurationKey.Format]: "",
    [ConfigurationKey.DisplayAsElapsedTime]: "False",
    [ConfigurationKey.DisplayCurrentOption]: "False",
};

export const ConfigurationComponent = defineComponent({
    name: "DateTimeField.Configuration",

    components: {
        TextBox,
        CheckBox
    },

    props: getFieldConfigurationProps(),

    emits: [
        "update:modelValue",
        "updateConfiguration",
        "updateConfigurationValue"
    ],

    setup(props, { emit }) {
        // Define the properties that will hold the current selections.
        const format = ref("");
        const displayAsElapsedTime = ref(false);
        const displayCurrentOption = ref(false);

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
            newValue[ConfigurationKey.Format] = format.value ?? defaults[ConfigurationKey.Format];
            newValue[ConfigurationKey.DisplayAsElapsedTime] = asTrueFalseOrNull(displayAsElapsedTime.value) ?? defaults[ConfigurationKey.DisplayAsElapsedTime];
            newValue[ConfigurationKey.DisplayCurrentOption] = asTrueFalseOrNull(displayCurrentOption.value) ?? defaults[ConfigurationKey.DisplayCurrentOption];

            // Compare the new value and the old value.
            const anyValueChanged = newValue[ConfigurationKey.Format] !== (props.modelValue[ConfigurationKey.Format] ?? defaults[ConfigurationKey.Format])
                || newValue[ConfigurationKey.DisplayAsElapsedTime] !== (props.modelValue[ConfigurationKey.DisplayAsElapsedTime] ?? defaults[ConfigurationKey.DisplayAsElapsedTime])
                || newValue[ConfigurationKey.DisplayCurrentOption] !== (props.modelValue[ConfigurationKey.DisplayCurrentOption] ?? defaults[ConfigurationKey.DisplayCurrentOption]);

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
            format.value = props.modelValue[ConfigurationKey.Format] ?? "";
            displayAsElapsedTime.value = asBoolean(props.modelValue[ConfigurationKey.DisplayAsElapsedTime]);
            displayCurrentOption.value = asBoolean(props.modelValue[ConfigurationKey.DisplayCurrentOption]);
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
        watch(format, (val) => maybeUpdateConfiguration(ConfigurationKey.Format, val ?? defaults[ConfigurationKey.Format]));
        watch(displayAsElapsedTime, (val) => maybeUpdateConfiguration(ConfigurationKey.DisplayAsElapsedTime, asTrueFalseOrNull(val) ?? defaults[ConfigurationKey.DisplayAsElapsedTime]));
        watch(displayCurrentOption, (val) => maybeUpdateConfiguration(ConfigurationKey.DisplayCurrentOption, asTrueFalseOrNull(val) ?? defaults[ConfigurationKey.DisplayCurrentOption]));

        return {
            format,
            displayAsElapsedTime,
            displayCurrentOption,
        };
    },

    template: `
<div>
    <TextBox v-model="format" label="Date Time Format" help="The format string to use for date (default is system short date and time)" />
    <CheckBox v-model="displayAsElapsedTime" label="Display as Elapsed Time" help="Display value as an elapsed time" />
    <CheckBox v-model="displayCurrentOption" label="Display Current Option" help="Include option to specify value as the current time" />
</div>
`
});
