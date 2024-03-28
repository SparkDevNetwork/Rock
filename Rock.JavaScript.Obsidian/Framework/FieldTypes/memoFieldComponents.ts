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
import { defineComponent, ref, computed, watch } from "vue";
import { getFieldEditorProps, getFieldConfigurationProps } from "./utils";
import TextBox from "@Obsidian/Controls/textBox.obs";
import NumberBox from "@Obsidian/Controls/numberBox.obs";
import CheckBox from "@Obsidian/Controls/checkBox.obs";
import { asBoolean, asBooleanOrNull, asTrueFalseOrNull } from "@Obsidian/Utility/booleanUtils";
import { toNumberOrNull } from "@Obsidian/Utility/numberUtils";
import { toNumber } from "@Obsidian/Utility/numberUtils";
import { ConfigurationValueKey } from "./memoField.partial";
import { useVModelPassthrough } from "@Obsidian/Utility/component";

export const EditComponent = defineComponent({
    name: "MemoField.Edit",

    components: {
        TextBox
    },

    props: getFieldEditorProps(),

    emits: [
        "update:modelValue"
    ],

    setup(props, { emit }) {
        const internalValue = useVModelPassthrough(props, "modelValue", emit);

        const configAttributes = computed((): Record<string, number | boolean> => {
            const attributes: Record<string, number | boolean> = {};

            const maxCharsConfig = props.configurationValues[ConfigurationValueKey.MaxCharacters];
            const maxCharsValue = toNumber(maxCharsConfig);

            if (maxCharsValue) {
                attributes.maxLength = maxCharsValue;
            }

            const showCountDownConfig = props.configurationValues[ConfigurationValueKey.ShowCountDown];
            const showCountDownValue = asBooleanOrNull(showCountDownConfig) || false;

            const allowHtmlConfig = props.configurationValues[ConfigurationValueKey.AllowHtml];
            const allowHtmlValue = asBooleanOrNull(allowHtmlConfig) || false;
            if(allowHtmlValue){
                attributes.allowHtml = allowHtmlValue;
            }


            if (showCountDownValue) {
                attributes.showCountDown = showCountDownValue;
            }

            const rowsConfig = props.configurationValues[ConfigurationValueKey.NumberOfRows];
            const rows = toNumber(rowsConfig || null) || 3;

            if (rows > 0) {
                attributes.rows = rows;
            }

            return attributes;
        });

        return {
            internalValue,
            configAttributes
        };
    },

    template: `
<TextBox v-model="internalValue" v-bind="configAttributes" textMode="MultiLine" />
`
});

export const FilterComponent = defineComponent({
    name: "MemoField.Filter",

    components: {
        TextBox
    },

    props: getFieldEditorProps(),

    emits: [
        "update:modelValue"
    ],

    setup(props, { emit }) {
        const internalValue = useVModelPassthrough(props, "modelValue", emit);

        return {
            internalValue
        };
    },

    template: `
<TextBox v-model="internalValue" />
`
});

export const ConfigurationComponent = defineComponent({
    name: "MemoField.Configuration",

    components: {
        CheckBox,
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
        const numberOfRows = ref<number | null>(null);
        const allowHtml = ref(false);
        const maxCharacters = ref<number | null>(null);
        const showCountdown = ref(false);

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
            newValue[ConfigurationValueKey.NumberOfRows] = numberOfRows.value?.toString() ?? "";
            newValue[ConfigurationValueKey.AllowHtml] = asTrueFalseOrNull(allowHtml.value) ?? "False";
            newValue[ConfigurationValueKey.MaxCharacters] = maxCharacters.value?.toString() ?? "";
            newValue[ConfigurationValueKey.ShowCountDown] = asTrueFalseOrNull(showCountdown.value) ?? "False";

            // Compare the new value and the old value.
            const anyValueChanged = newValue[ConfigurationValueKey.NumberOfRows] !== (props.modelValue[ConfigurationValueKey.NumberOfRows] ?? "")
                || newValue[ConfigurationValueKey.AllowHtml] !== (props.modelValue[ConfigurationValueKey.AllowHtml] ?? "False")
                || newValue[ConfigurationValueKey.MaxCharacters] !== (props.modelValue[ConfigurationValueKey.MaxCharacters] ?? "")
                || newValue[ConfigurationValueKey.ShowCountDown] !== (props.modelValue[ConfigurationValueKey.ShowCountDown] ?? "False");

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
            numberOfRows.value = toNumberOrNull(props.modelValue[ConfigurationValueKey.NumberOfRows]);
            allowHtml.value = asBoolean(props.modelValue[ConfigurationValueKey.AllowHtml]);
            maxCharacters.value = toNumberOrNull(props.modelValue[ConfigurationValueKey.MaxCharacters]);
            showCountdown.value = asBoolean(props.modelValue[ConfigurationValueKey.ShowCountDown]);
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
        watch(numberOfRows, val => maybeUpdateConfiguration(ConfigurationValueKey.NumberOfRows, val?.toString() ?? ""));
        watch(allowHtml, val => maybeUpdateConfiguration(ConfigurationValueKey.AllowHtml, asTrueFalseOrNull(val) ?? "False"));
        watch(maxCharacters, val => maybeUpdateConfiguration(ConfigurationValueKey.MaxCharacters, val?.toString() ?? ""));
        watch(showCountdown, val => maybeUpdateConfiguration(ConfigurationValueKey.ShowCountDown, asTrueFalseOrNull(val) ?? "False"));

        return {
            numberOfRows,
            maxCharacters,
            allowHtml,
            showCountdown
        };
    },

    template: `
<div>
    <NumberBox v-model="numberOfRows" label="Rows" help="The number of rows to display (default is 3)" />
    <CheckBox v-model="allowHtml" label="Allow HTML" text="Yes" help="Controls whether server should prevent HTML from being entered in this field or not" />
    <NumberBox v-model="maxCharacters" label="Max Characters" help="The maximum number of characters to allow. Leave this field empty to allow for an unlimited amount of text" />
    <CheckBox v-model="showCountdown" label="Show Character Limit Countdown" text="Yes" help="When set, displays a countdown showing how many characters remain (for the Max Characters setting)" />
</div>
`
});
