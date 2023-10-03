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
import CheckBox from "@Obsidian/Controls/checkBox.obs";
import PersonPicker from "@Obsidian/Controls/personPicker.obs";
import { ConfigurationValueKey } from "./personField.partial";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { asBoolean, asTrueFalseOrNull } from "@Obsidian/Utility/booleanUtils";

export const EditComponent = defineComponent({
    name: "Person.Edit",

    components: {
        PersonPicker
    },

    props: getFieldEditorProps(),

    setup(props, { emit }) {
        const internalValue = ref({} as ListItemBag);

        const includeBusinesses = computed((): boolean => {
            return asBoolean(props.configurationValues[ConfigurationValueKey.IncludeBusinesses] ?? "");
        });

        const enableSelfSelection = computed(() => asTrueFalseOrNull(props.configurationValues[ConfigurationValueKey.EnableSelfSelection]));

        watch(() => props.modelValue, () => {
            internalValue.value = JSON.parse(props.modelValue || "{}");
        }, { immediate: true });

        watch(() => internalValue.value, () => {
            emit("update:modelValue", JSON.stringify(internalValue.value));
        });

        return {
            internalValue,
            includeBusinesses,
            enableSelfSelection
        };
    },

    template: `
<PersonPicker v-model="internalValue" :includeBusinesses="includeBusinesses" :enableSelfSelection="enableSelfSelection" />
`
});

export const ConfigurationComponent = defineComponent({
    name: "Person.Configuration",

    components: {
        CheckBox
    },

    props: getFieldConfigurationProps(),

    setup(props, { emit }) {
        // Define the properties that will hold the current selections.
        const enableSelfSelection = ref(false);
        const includeBusinesses = ref(false);

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
            newValue[ConfigurationValueKey.EnableSelfSelection] = asTrueFalseOrNull(enableSelfSelection.value) ?? "False";
            newValue[ConfigurationValueKey.IncludeBusinesses] = asTrueFalseOrNull(includeBusinesses.value) ?? "False";

            const anyValueChanged = newValue[ConfigurationValueKey.EnableSelfSelection] !== (props.modelValue[ConfigurationValueKey.EnableSelfSelection] ?? "False")
                || newValue[ConfigurationValueKey.IncludeBusinesses] !== (props.modelValue[ConfigurationValueKey.IncludeBusinesses] ?? "False");

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
            enableSelfSelection.value = asBoolean(props.modelValue[ConfigurationValueKey.EnableSelfSelection]);
            includeBusinesses.value = asBoolean(props.modelValue[ConfigurationValueKey.IncludeBusinesses]);
        }, {
            immediate: true
        });

        // Watch for changes in properties that require new configuration
        // properties to be retrieved from the server.
        watch([], () => {
            if (maybeUpdateModelValue()) {
                emit("updateConfiguration");
            }
        });

        // Watch for changes in properties that only require a local UI update.
        watch(enableSelfSelection, () => maybeUpdateConfiguration(ConfigurationValueKey.EnableSelfSelection, asTrueFalseOrNull(enableSelfSelection.value) ?? "False"));
        watch(includeBusinesses, () => maybeUpdateConfiguration(ConfigurationValueKey.IncludeBusinesses, asTrueFalseOrNull(includeBusinesses.value) ?? "False"));

        return {
            enableSelfSelection,
            includeBusinesses
        };
    },

    template: `
<div>
    <CheckBox v-model="enableSelfSelection" label="Enable Self Selection"
        help="When using Person Picker, show the self selection option" />

    <CheckBox v-model="includeBusinesses" label="Include Businesses"
        help="When using Person Picker, include businesses in the search results" />
</div>
`
});
