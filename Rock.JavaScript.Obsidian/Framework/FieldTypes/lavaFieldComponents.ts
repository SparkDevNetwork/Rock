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

import { computed, defineComponent, ref, watch } from "vue";
import { getFieldConfigurationProps, getFieldEditorProps } from "./utils";
import DropDownList from "@Obsidian/Controls/dropDownList.obs";
import NumberBox from "@Obsidian/Controls/numberBox.obs";
import CodeEditor from "@Obsidian/Controls/codeEditor.obs";
import { ConfigurationPropertyKey, ConfigurationValueKey } from "./lavaField.partial";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { toNumber, toNumberOrNull } from "@Obsidian/Utility/numberUtils";

export const EditComponent = defineComponent({
    name: "LavaField.Edit",

    components: {
        CodeEditor
    },

    props: getFieldEditorProps(),

    setup(props, { emit }) {
        // The internal value used by the code editor.
        const minimumHeight = 200;
        const internalValue = ref<string>("");

        // The selected code editor mode from the field type's configuration.
        const editorMode = computed((): number => {
            return toNumber(props.configurationValues[ConfigurationValueKey.EditorMode]);
        });

        // The selected code editor theme from the field type's configuration.
        const editorTheme = computed((): number => {
            return toNumber(props.configurationValues[ConfigurationValueKey.EditorTheme]);
        });

        // The selected code editor theme from the field type's configuration.
        const editorHeight = computed((): number => {
            const editorHeight = toNumberOrNull(props.configurationValues[ConfigurationValueKey.EditorHeight]) ?? minimumHeight;
            return editorHeight;
        });

        // Watch for changes from the parent component and update the code editor.
        watch(() => props.modelValue, () => {
            internalValue.value = props.modelValue ?? "";
        }, {
            immediate: true
        });

        // Watch for changes from the code editor and update the parent component.
        watch(internalValue, () => {
            emit("update:modelValue", internalValue.value);
        });

        return {
            internalValue,
            editorMode,
            editorTheme,
            editorHeight
        };
    },

    template: `
    <CodeEditor v-model="internalValue" :theme="editorTheme" :mode="editorMode" :editorHeight="editorHeight" />
`
});

export const ConfigurationComponent = defineComponent({
    name: "LavaField.Configuration",

    components: {
        DropDownList,
        NumberBox,
    },

    props: getFieldConfigurationProps(),

    emits: ["update:modelValue", "updateConfiguration", "updateConfigurationValue"],

    setup(props, { emit }) {
        // Define the properties that will hold the current selections.
        const editorMode = ref<string>("");
        const editorTheme = ref<string>("");
        const editorHeight = ref<number | null>();

        // The options for the editor mode dropdown list
        const editorModeOptions = computed((): ListItemBag[] => {
            const editorModeOptions = props.configurationProperties[ConfigurationPropertyKey.EditorModeOptions];
            return editorModeOptions ? JSON.parse(editorModeOptions) as ListItemBag[] : [];
        });

        // The options for the editor theme dropdown list
        const editorThemeOptions = computed((): ListItemBag[] => {
            const editorThemeOptions = props.configurationProperties[ConfigurationPropertyKey.EditorThemeOptions];
            return editorThemeOptions ? JSON.parse(editorThemeOptions) as ListItemBag[] : [];
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
            newValue[ConfigurationValueKey.EditorMode] = editorMode.value ?? "";
            newValue[ConfigurationValueKey.EditorTheme] = editorTheme.value ?? "";
            newValue[ConfigurationValueKey.EditorHeight] = editorHeight.value?.toString() ?? "";

            // Compare the new value and the old value.
            const anyValueChanged = newValue[ConfigurationValueKey.EditorMode] !== (props.modelValue[ConfigurationValueKey.EditorMode])
                || newValue[ConfigurationValueKey.EditorTheme] !== (props.modelValue[ConfigurationValueKey.EditorTheme])
                || newValue[ConfigurationValueKey.EditorHeight] !== (props.modelValue[ConfigurationValueKey.EditorHeight]);

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
            editorMode.value = props.modelValue[ConfigurationValueKey.EditorMode] ?? 0;
            editorTheme.value = props.modelValue[ConfigurationValueKey.EditorTheme] ?? 0;
            editorHeight.value = toNumberOrNull(props.modelValue[ConfigurationValueKey.EditorHeight]);
        }, {
            immediate: true
        });

        // Watch for changes in properties that only require a local UI update.
        watch(editorMode, () => maybeUpdateConfiguration(ConfigurationValueKey.EditorMode, editorMode.value ?? ""));
        watch(editorTheme, () => maybeUpdateConfiguration(ConfigurationValueKey.EditorTheme, editorTheme.value ?? ""));
        watch(editorHeight, () => maybeUpdateConfiguration(ConfigurationValueKey.EditorHeight, editorHeight.value?.toString() ?? ""));

        return {
            editorMode,
            editorModeOptions,
            editorTheme,
            editorThemeOptions,
            editorHeight
        };
    },

    template: `
<DropDownList label="Editor Mode" v-model="editorMode" :items="editorModeOptions" :showBlankItem="false" help="The type of code that will be entered." />
<DropDownList label="Editor Theme" v-model="editorTheme" :items="editorThemeOptions" :showBlankItem="false" help="The styling theme to use for the code editor." />
<NumberBox label="Editor Height" v-model="editorHeight" :minimumValue="200" :showBlankItem="false" help="The height of the control in pixels (minimum of 200)" />
`
});
