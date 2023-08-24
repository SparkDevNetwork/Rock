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
import { ConfigurationValueKey } from "./colorSelectorField.types.partial";
import { deserializeAllowMultiple, deserializeColors, deserializeValue, serializeAllowMultiple, serializeColors, serializeValue } from "./colorSelectorField.utils.partial";
import { getFieldEditorProps, getFieldConfigurationProps } from "./utils";
import ColorSelector from "@Obsidian/Controls/colorSelector.obs";
import ValueList from "@Obsidian/Controls/valueList.obs";
import { useVModelPassthrough } from "@Obsidian/Utility/component";

export const ConfigurationComponent = defineComponent({
    components: {
        ColorSelector,
        ValueList
    },

    name: "ColorSelector.Configuration",

    props: getFieldConfigurationProps(),

    emits: ["update:modelValue", "updateConfiguration", "updateConfigurationValue"],

    setup(props, { emit }) {
        //#region Computed Values

        const configColors = computed<string[]>(() => {
            return deserializeColors(props.modelValue[ConfigurationValueKey.Colors]);
        });

        const configAllowMultiple = computed<boolean>(() => {
            return deserializeAllowMultiple(props.modelValue[ConfigurationValueKey.AllowMultiple]);
        });

        //#endregion

        //#region Values

        // Define the properties that will hold the current selections with current config values.
        const colors = ref<string[]>(configColors.value);
        const allowMultiple = ref<boolean>(configAllowMultiple.value);

        //#endregion

        /**
         * Update the modelValue property if any value of the dictionary has
         * actually changed. This helps prevent unwanted postbacks if the value
         * didn't really change - which can happen if multiple values get updated
         * at the same time.
         *
         * @returns true if a new modelValue was emitted to the parent component.
         */
        function maybeUpdateModelValue(): boolean {
            const newValue: Record<string, string> = {};

            // Construct the new value that will be emitted if it is different
            // than the current value.
            newValue[ConfigurationValueKey.Colors] = serializeColors(colors.value);
            newValue[ConfigurationValueKey.AllowMultiple] = serializeAllowMultiple(allowMultiple.value);

            // Compare the new value and the old value.
            const anyValueChanged =
                newValue[ConfigurationValueKey.Colors] !== props.modelValue[ConfigurationValueKey.Colors]
                || newValue[ConfigurationValueKey.AllowMultiple] !== props.modelValue[ConfigurationValueKey.AllowMultiple];

            // If any value changed then emit the new model value.
            if (anyValueChanged) {
                emit("update:modelValue", newValue);
                return true;
            }
            else {
                return false;
            }
        }

        /**
         * Emits the updateConfigurationValue if the value has actually changed.
         *
         * @param key The key that was possibly modified.
         * @param value The new value.
         */
        function maybeUpdateConfiguration(key: string, value: string): void {
            if (maybeUpdateModelValue()) {
                emit("updateConfigurationValue", key, value);
            }
        }

        // Watch for changes coming in from the parent component and update our
        // data to match the new information.
        watch(() => [props.modelValue, props.configurationProperties], () => {
            colors.value = deserializeColors(props.modelValue[ConfigurationValueKey.Colors]);
            allowMultiple.value = deserializeAllowMultiple(props.modelValue[ConfigurationValueKey.AllowMultiple]);
        }, {
            immediate: true
        });

        // Watch for changes in properties that only require a local UI update.
        watch(colors, () => {
            maybeUpdateConfiguration(ConfigurationValueKey.Colors, serializeColors(colors.value) || "");
        });
        watch(allowMultiple, () => {
            maybeUpdateConfiguration(ConfigurationValueKey.AllowMultiple, serializeAllowMultiple(allowMultiple.value));
        });

        return {
            allowMultiple,
            colors
        };
    },

    template: `
<div>
    <ValueList v-model="colors"
               help="The hex colors to select from."
               label="Colors"
               valuePrompt="#FFFFFF" />
</div>
`
});

export const EditComponent = defineComponent({
    name: "ColorSelector.Edit",

    components: {
        ColorSelector
    },

    props: getFieldEditorProps(),

    emits: [
        "update:modelValue"
    ],

    setup(props, { emit }) {
        const items = computed<string[]>(() => {
            return deserializeColors(props.configurationValues[ConfigurationValueKey.Colors]);
        });

        const allowMultiple = computed<boolean>(() => {
            return deserializeAllowMultiple(props.configurationValues[ConfigurationValueKey.AllowMultiple]);
        });

        const internalValue = useVModelPassthrough(props, "modelValue", emit);

        const selectedColors = computed<string[]>({
            get(): string[] {
                return deserializeValue(internalValue.value);
            },
            set(newValue: string[]): void {
                internalValue.value = serializeValue(newValue);
            }
        });

        return {
            allowMultiple,
            selectedColors,
            items
        };
    },

    template: `
<ColorSelector v-model="selectedColors"
               :allowMultiple="allowMultiple"
               :items="items"
               label="Colors" />
`
});
