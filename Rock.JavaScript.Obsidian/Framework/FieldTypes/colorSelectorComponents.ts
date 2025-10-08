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
import { deserializeColors, deserializeValue, serializeValue } from "./colorSelectorField.utils.partial";
import { getFieldEditorProps, getFieldConfigurationProps } from "./utils";
import ColorSelector from "@Obsidian/Controls/colorSelector.obs";
import ListItems from "@Obsidian/Controls/listItems.obs";
import { useVModelPassthrough } from "@Obsidian/Utility/component";
import { newGuid } from "@Obsidian/Utility/guid";
import { KeyValueItem } from "@Obsidian/Types/Controls/keyValueItem";
import { safeParseJson } from "@Obsidian/Utility/stringUtils";

// Store the generated keys for the colors so that when we convert back and forth,
// we can keep the same key for the same color.
let keyMap: string[] = [];

function convertColorsToKeyValueItemString(colors: string): string {
    return JSON.stringify(deserializeColors(colors ?? "").map((color, i) => ({ key: keyMap[i] || newGuid(), value: color })));
}

function convertKeyValueItemStringToColors(value: string): string {
    const items = safeParseJson<KeyValueItem[]>(value ?? "[]") ?? [];
    keyMap = items.map(item => item.key ?? "");

    return items.map(item => item.value).join("|") ?? "";
}

export const ConfigurationComponent = defineComponent({
    components: {
        ColorSelector,
        ListItems
    },

    name: "ColorSelector.Configuration",

    props: getFieldConfigurationProps(),

    emits: ["update:modelValue", "updateConfiguration", "updateConfigurationValue"],

    setup(props, { emit }) {
        //#region Values

        // Define the properties that will hold the current selections with current config values.
        const colors = ref<string>("[]");

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

            // Construct the new value that will be emitted if it is different than the current value.
            newValue[ConfigurationValueKey.Colors] = convertKeyValueItemStringToColors(colors.value);

            // Compare the new value and the old value.
            const anyValueChanged = newValue[ConfigurationValueKey.Colors] !== props.modelValue[ConfigurationValueKey.Colors];

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
            colors.value = convertColorsToKeyValueItemString(props.modelValue[ConfigurationValueKey.Colors]);
        }, {
            immediate: true
        });

        // Watch for changes in properties that only require a local UI update.
        watch(colors, () => {
            maybeUpdateConfiguration(ConfigurationValueKey.Colors, convertKeyValueItemStringToColors(colors.value));
        });

        return {
            colors
        };
    },

    template: `<ListItems v-model="colors" help="The hex colors to select from." label="Colors" valuePrompt="#FFFFFF" />`
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
            return deserializeColors(props.configurationValues[ConfigurationValueKey.Colors] ?? "");
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
            selectedColors,
            items
        };
    },

    template: `<ColorSelector v-model="selectedColors" :items="items" label="Colors" />`
});
