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
import { getFieldConfigurationProps, getFieldEditorProps } from "./utils";
import TextBox from "@Obsidian/Controls/textBox.obs";
import CheckBox from "@Obsidian/Controls/checkBox.obs";
import NumberBox from "@Obsidian/Controls/numberBox.obs";
import { asBoolean, asBooleanOrNull, asTrueFalseOrNull } from "@Obsidian/Utility/booleanUtils";
import { toNumberOrNull } from "@Obsidian/Utility/numberUtils";

// We can't import the ConfigurationValueKey from textField.partial.ts
// because it causes a recursive import back to this file by way of
// the fieldType.ts import in textField.partial.ts.
export const enum ConfigurationValueKey {
    /** Contains "True" if the text field is designed for password entry. */
    IsPassword = "ispassword",

    /** The maximum number of characters allowed in the text entry field. */
    MaxCharacters = "maxcharacters",

    /** Contains "True" if the text field should show the character countdown. */
    ShowCountdown = "showcountdown"
}

export const EditComponent = defineComponent({
    name: "TextField.Edit",

    components: {
        TextBox
    },

    props: getFieldEditorProps(),

    setup(props, { emit }) {
        // The internal value used by the text editor.
        const internalValue = ref("");

        // Configuration attributes passed to the text editor.
        const configAttributes = computed((): Record<string, number | boolean> => {
            const attributes: Record<string, number | boolean> = {};

            const maxCharsConfig = props.configurationValues[ConfigurationValueKey.MaxCharacters];
            if (maxCharsConfig) {
                const maxCharsValue = Number(maxCharsConfig);

                if (maxCharsValue) {
                    attributes.maxLength = maxCharsValue;
                }
            }

            const showCountDownConfig = props.configurationValues[ConfigurationValueKey.ShowCountdown];
            if (showCountDownConfig && showCountDownConfig) {
                const showCountDownValue = asBooleanOrNull(showCountDownConfig) || false;

                if (showCountDownValue) {
                    attributes.showCountDown = showCountDownValue;
                }
            }

            return attributes;
        });

        // The type of text input field to use on the text editor.
        const textType = computed((): string => {
            const isPasswordConfig = props.configurationValues[ConfigurationValueKey.IsPassword];
            const isPassword = asBooleanOrNull(isPasswordConfig) ?? false;

            return isPassword ? "password" : "";

        });

        // Watch for changes from the parent component and update the text editor.
        watch(() => props.modelValue, () => {
            internalValue.value = props.modelValue;
        }, {
            immediate: true
        });

        // Watch for changes from the text editor and update the parent component.
        watch(internalValue, () => {
            emit("update:modelValue", internalValue.value);
        });

        return {
            configAttributes,
            internalValue,
            textType
        };
    },

    template: `
<TextBox v-model="internalValue" v-bind="configAttributes" :type="textType" />
`
});

export const ConfigurationComponent = defineComponent({
    name: "TextField.Configuration",

    components: {
        CheckBox,
        NumberBox
    },

    props: getFieldConfigurationProps(),

    emits: ["update:modelValue", "updateConfiguration", "updateConfigurationValue" ],

    setup(props, { emit }) {
        // Define the properties that will hold the current selections.
        const passwordField = ref(false);
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
            newValue[ConfigurationValueKey.IsPassword] = asTrueFalseOrNull(passwordField.value) ?? "False";
            newValue[ConfigurationValueKey.MaxCharacters] = maxCharacters.value?.toString() ?? "";
            newValue[ConfigurationValueKey.ShowCountdown] = asTrueFalseOrNull(showCountdown.value) ?? "False";

            // Compare the new value and the old value.
            const anyValueChanged = newValue[ConfigurationValueKey.IsPassword] !== (props.modelValue[ConfigurationValueKey.IsPassword] ?? "False")
                || newValue[ConfigurationValueKey.MaxCharacters] !== (props.modelValue[ConfigurationValueKey.MaxCharacters] ?? "")
                || newValue[ConfigurationValueKey.ShowCountdown] !== (props.modelValue[ConfigurationValueKey.ShowCountdown] ?? "False");

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
            passwordField.value = asBoolean(props.modelValue[ConfigurationValueKey.IsPassword]);
            maxCharacters.value = toNumberOrNull(props.modelValue[ConfigurationValueKey.MaxCharacters]);
            showCountdown.value = asBoolean(props.modelValue[ConfigurationValueKey.ShowCountdown]);
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
        watch(passwordField, () => maybeUpdateConfiguration(ConfigurationValueKey.IsPassword, asTrueFalseOrNull(passwordField.value) ?? "False"));
        watch(maxCharacters, () => maybeUpdateConfiguration(ConfigurationValueKey.MaxCharacters, maxCharacters.value?.toString() ?? ""));
        watch(showCountdown, () => maybeUpdateConfiguration(ConfigurationValueKey.ShowCountdown, asTrueFalseOrNull(showCountdown.value) ?? "False"));

        return {
            maxCharacters,
            passwordField,
            showCountdown
        };
    },

    template: `
<div>
    <CheckBox v-model="passwordField" label="Password Field" help="When set, edit field will be masked." />
    <NumberBox v-model="maxCharacters" label="Max Characters" help="The maximum number of characters to allow. Leave this field empty to allow for an unlimited amount of text." />
    <CheckBox v-model="showCountdown" label="Show Character Limit Countdown" help="When set, displays a countdown showing how many characters remain (for the Max Characters setting)." />
</div>
`
});
