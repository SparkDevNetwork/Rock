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
import {  getFieldConfigurationProps, getFieldEditorProps } from "./utils";
import DropDownList from "@Obsidian/Controls/dropDownList.obs";
import { ConfigurationValueKey } from "./systemCommunicationField.partial";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import CheckBox from "@Obsidian/Controls/checkBox.obs";
import { asBoolean, asTrueFalseOrNull } from "@Obsidian/Utility/booleanUtils";

export const EditComponent = defineComponent({
    name: "SystemCommunication.Edit",

    components: {
        DropDownList
    },

    props: getFieldEditorProps(),

    setup(props, { emit }) {
        // The internal value used by the dropdownlist control.
        const internalValue = ref<string>("");

        // The available options to choose from.
        const options = computed((): ListItemBag[] => {
            const options = JSON.parse(props.configurationValues[ConfigurationValueKey.ClientValues] || "[]") as ListItemBag[];
            return options;
        });

        // Watch for changes from the parent component and update the client UI.
        watch(() => props.modelValue, () => {
            internalValue.value = props.modelValue;
        }, {
            immediate: true
        });

        // Watch for changes from the client and update the parent component.
        watch(internalValue, () => {
            emit("update:modelValue", internalValue.value);
        });

        return {
            internalValue,
            options
        };
    },

    template: `
    <DropDownList v-model="internalValue" :items="options" :showBlankItem="true" :enhanceForLongLists="true" />
`
});

export const ConfigurationComponent = defineComponent({
    name: "SystemCommunicationField.Configuration",
    components: {
        CheckBox
    },

    props: getFieldConfigurationProps(),

    setup(props, { emit }) {
        // Define the properties that will hold the current selections.
        const includeInactive = ref(false);

        /**
         * Update the modelValue property if any value of the dictionary has
         * actually changed. This helps prevent unwanted postbacks if the value
         * didn't really change - which can happen if multiple values get updated
         * at the same time.
         *
         * @returns true if a new modelValue was emitted to the parent component.
         */
        const maybeUpdateModelValue = (): boolean => {
            const newValue: Record<string, string> = {
                ...props.modelValue
            };

            // Construct the new value that will be emitted if it is different
            // than the current value.
            newValue[ConfigurationValueKey.IncludeInactive] = asTrueFalseOrNull(includeInactive.value) ?? "False";

            // Compare the new value and the old value.
            const anyValueChanged = newValue[ConfigurationValueKey.IncludeInactive] !== (props.modelValue[ConfigurationValueKey.IncludeInactive]);

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
            includeInactive.value = asBoolean(props.modelValue[ConfigurationValueKey.IncludeInactive]);
        }, {
            immediate: true
        });

        // Watch for changes in properties that require new configuration
        // properties to be retrieved from the server.
        watch([includeInactive], () => {
            if (maybeUpdateModelValue()) {
                emit("updateConfiguration");
            }
        });

        // Watch for changes in properties that only require a local UI update.
        watch(includeInactive, () => maybeUpdateConfiguration(ConfigurationValueKey.IncludeInactive, asTrueFalseOrNull(includeInactive.value) ?? "False"));

        return {
            includeInactive,
        };
    },
    template: `
    <div>
    <CheckBox v-model="includeInactive"
        label="Include Inactive"
        help="When set, inactive system communications will be included in the list." />
</div>
    `
});
