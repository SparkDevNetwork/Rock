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
import CheckBox from "@Obsidian/Controls/checkBox.obs";
import EntityTypePicker from "@Obsidian/Controls/entityTypePicker.obs";
import { ConfigurationValueKey } from "./entityTypeField.partial";
import { asBoolean, asTrueOrFalseString } from "@Obsidian/Utility/booleanUtils";
import { updateRefValue } from "@Obsidian/Utility/component";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";

export const EditComponent = defineComponent({
    name: "EntityTypeField.Edit",

    components: {
        EntityTypePicker
    },

    props: getFieldEditorProps(),

    setup(props, { emit }) {
        // The internal value used by the text editor.
        const internalValue = ref<ListItemBag>();

        // Include Global configuration value, includes the 'Global Attributes' option when set to true.
        const includeGlobalOption = computed((): boolean => {
            const includeGlobal = asBoolean(props.configurationValues[ConfigurationValueKey.IncludeGlobal]);
            return includeGlobal;
        });

        // Watch for changes from the parent component and update the text editor.
        watch(() => props.modelValue, () => {
            updateRefValue(internalValue, JSON.parse(props.modelValue || "{}"));
        }, {
            immediate: true
        });

        // Watch for changes from the text editor and update the parent component.
        watch(internalValue, () => {
            emit("update:modelValue", JSON.stringify(internalValue.value));
        });

        return {
            internalValue,
            includeGlobalOption,
        };
    },

    template: `
    <EntityTypePicker v-model="internalValue" :multiple="false" :includeGlobalOption="includeGlobalOption" showBlankItem />
`
});

export const ConfigurationComponent = defineComponent({
    name: "EntityTypeField.Configuration",

    components: {
        CheckBox
    },

    props: getFieldConfigurationProps(),

    emits: ["update:modelValue", "updateConfiguration", "updateConfigurationValue"],

    setup(props, { emit }) {
        // Define the properties that will hold the current selections.
        const includeGlobalOption = ref<boolean>();

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
            newValue[ConfigurationValueKey.IncludeGlobal] = asTrueOrFalseString(includeGlobalOption.value);

            // Compare the new value and the old value.
            const anyValueChanged = newValue[ConfigurationValueKey.IncludeGlobal] !== props.modelValue[ConfigurationValueKey.IncludeGlobal];

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
            includeGlobalOption.value = asBoolean(props.modelValue[ConfigurationValueKey.IncludeGlobal]);
        }, {
            immediate: true
        });

        // Watch for changes in properties that only require a local UI update.
        watch(includeGlobalOption, () => maybeUpdateConfiguration(ConfigurationValueKey.IncludeGlobal, asTrueOrFalseString(includeGlobalOption.value)));

        return {
            includeGlobalOption
        };
    },

    template: `
<CheckBox v-model="includeGlobalOption" label="Include Global Attributes Option" text="Yes" help="Should the 'Global Attributes' entity option be included." />
`
});