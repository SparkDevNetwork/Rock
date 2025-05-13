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
import DropDownList from "@Obsidian/Controls/dropDownList.obs";
import { ConfigurationPropertyKey, ConfigurationValueKey } from "./connectionStatusField.partial";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { asBoolean, asTrueOrFalseString } from "@Obsidian/Utility/booleanUtils";
import { isNullOrWhiteSpace } from "@Obsidian/Utility/stringUtils";

export const EditComponent = defineComponent({
    name: "ConnectionStatusField.Edit",

    components: {
        DropDownList,
    },

    props: getFieldEditorProps(),

    setup(props, { emit }) {
        // The internal value used by the text editor.
        const internalValue = ref<string>("");

        // If a filter has been selected then there is no need to group the options since they all belong to the same group.
        const grouped = computed((): boolean => {
            const connectionType = JSON.parse(props.configurationValues[ConfigurationValueKey.ConnectionTypeFilter] || "{}") as ListItemBag;
            return isNullOrWhiteSpace(connectionType.value);
        });

        // The ConnectionStatuses to choose from.
        const options = computed((): ListItemBag[] => {
            let clientValues = JSON.parse(props.configurationValues[ConfigurationValueKey.ClientValues] || "[]") as ListItemBag[];
            const connectionType = JSON.parse(props.configurationValues[ConfigurationValueKey.ConnectionTypeFilter] || "{}") as ListItemBag;

            if (connectionType.value) {
                clientValues = clientValues.filter(c => c.category === connectionType.text);
            }

            return clientValues;
        });

        // Watch for changes from the parent component and update the text editor.
        watch(() => props.modelValue, () => {
            internalValue.value = props.modelValue;
        }, {
            immediate: true
        });

        // Watch for changes from the text editor and update the parent component.
        watch(internalValue, () => {
            emit("update:modelValue", internalValue.value ?? "");
        });

        return {
            internalValue,
            options,
            grouped
        };
    },

    template: `
    <DropDownList v-model="internalValue" :items="options" showBlankItem :grouped="grouped" />
`
});

export const ConfigurationComponent = defineComponent({
    name: "ConnectionStatusField.Configuration",

    components: {
        CheckBox,
        DropDownList
    },

    props: getFieldConfigurationProps(),

    emits: ["update:modelValue", "updateConfiguration", "updateConfigurationValue"],

    setup(props, { emit }) {
        // Define the properties that will hold the current selections.
        const connectionTypeValue = ref<string>("");
        const includeInactive = ref<boolean>();

        // The options for the connection type dropdown list
        const options = computed((): ListItemBag[] => {
            const connectionTypeOptions = props.configurationProperties[ConfigurationPropertyKey.ConnectionTypeOptions];
            return connectionTypeOptions ? JSON.parse(connectionTypeOptions) as ListItemBag[] : [];
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
            const connectionType = options.value.find(o => o.value == connectionTypeValue.value);

            // Construct the new value that will be emitted if it is different
            // than the current value.
            newValue[ConfigurationValueKey.ConnectionTypeFilter] = JSON.stringify(connectionType) ?? "";
            newValue[ConfigurationValueKey.IncludeInactive] = asTrueOrFalseString(includeInactive.value);
            newValue[ConfigurationValueKey.ClientValues] = props.modelValue[ConfigurationValueKey.ClientValues];

            // Compare the new value and the old value.
            const anyValueChanged = newValue[ConfigurationValueKey.ConnectionTypeFilter] !== (props.modelValue[ConfigurationValueKey.ConnectionTypeFilter])
                || newValue[ConfigurationValueKey.IncludeInactive] !== (props.modelValue[ConfigurationValueKey.IncludeInactive]);

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
            const connectionType = JSON.parse(props.modelValue[ConfigurationValueKey.ConnectionTypeFilter] || "{}") as ListItemBag;
            connectionTypeValue.value = connectionType.value ?? "";
            includeInactive.value = asBoolean(props.modelValue[ConfigurationValueKey.IncludeInactive]);
        }, {
            immediate: true
        });

        // Watch for changes in properties that require new configuration
        // properties to be retrieved from the server.
        watch(() => includeInactive.value, () => {
            if (maybeUpdateModelValue()) {
                emit("updateConfiguration");
            }
        });

        // Watch for changes in properties that only require a local UI update.
        watch(connectionTypeValue, () => maybeUpdateConfiguration(ConfigurationValueKey.ConnectionTypeFilter, connectionTypeValue.value));
        watch(includeInactive, () => maybeUpdateConfiguration(ConfigurationValueKey.IncludeInactive, asTrueOrFalseString(connectionTypeValue.value)));

        return {
            connectionTypeValue,
            includeInactive,
            options
        };
    },

    template: `
    <CheckBox label="Include Inactive" v-model="includeInactive" help="When set, inactive connection statuses will be included in the list." />
    <DropDownList label="Connection Type"
        v-model="connectionTypeValue"
        :items="options"
        showBlankItem
        help="Select a Connection Type to limit selection to a specific connection type. Leave blank to allow selection of statuses from any connection type." />
`
});
