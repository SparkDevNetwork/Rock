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
import { getFieldEditorProps, getFieldConfigurationProps } from "./utils";
import DefinedValuePicker from "@Obsidian/Controls/definedValuePicker.obs";
import DropDownList from "@Obsidian/Controls/dropDownList.obs";
import { ConfigurationValueKey } from "./groupTypeField.partial";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";

export const EditComponent = defineComponent({

    name: "GroupTypeField.Edit",

    components: {
        DropDownList
    },

    props: getFieldEditorProps(),

    data() {
        return {
            internalValue: this.modelValue ?? ""
        };
    },

    computed: {
        options(): ListItemBag[] {
            try {
                return JSON.parse(this.configurationValues[ConfigurationValueKey.Values] ?? "[]") as ListItemBag[];
            }
            catch {
                return [];
            }
        }
    },

    watch: {
        internalValue() {
            this.$emit("update:modelValue", this.internalValue);
        }
    },

    template: `
<DropDownList v-model="internalValue" :items="options" enhanceForLongLists />
`
});

export const ConfigurationComponent = defineComponent({
    name: "GroupTypeField.Configuration",

    components: {
        DefinedValuePicker,
        DropDownList
    },

    props: getFieldConfigurationProps(),

    emits: [
        "update:modelValue",
        "updateConfiguration",
        "updateConfigurationValue"
    ],

    setup(props, { emit }) {
        const groupTypePurposeValueGuid = ref(props.modelValue[ConfigurationValueKey.GroupTypePurposeValueGuid]);

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
            newValue[ConfigurationValueKey.GroupTypePurposeValueGuid] = groupTypePurposeValueGuid.value;

            // Compare the new value and the old value.
            const anyValueChanged = newValue[ConfigurationValueKey.GroupTypePurposeValueGuid] !== (props.modelValue[ConfigurationValueKey.GroupTypePurposeValueGuid]);

            // If any value changed then emit the new model value.
            if (anyValueChanged) {
                emit("update:modelValue", newValue);
                return true;
            }
            else {
                return false;
            }
        };

        const options = computed((): ListItemBag[] => {
            try {
                return JSON.parse(props.modelValue[ConfigurationValueKey.GroupTypePurposes] ?? "[]") as ListItemBag[];
            }
            catch {
                return [];
            }
        });

        const internalValue = computed(() : string => props.modelValue.value);

        /**
         * Emits the updateConfigurationValue if the value has actually changed.
         *
         * @param key The key that was possibly modified.
         * @param value The new value.
         */
        const maybeUpdateConfiguration = (key: string, value: string): void => {
            if (maybeUpdateModelValue()) {
                emit("updateConfigurationValue", key, value);
                emit("updateConfiguration");
            }
        };

        // Watch for changes in properties that only require a local UI update.
        watch(groupTypePurposeValueGuid, () => maybeUpdateConfiguration(ConfigurationValueKey.GroupTypePurposeValueGuid, groupTypePurposeValueGuid.value));

        return { groupTypePurposeValueGuid, options, internalValue };
    },

    template: `
<div>
    <DropDownList v-model="groupTypePurposeValueGuid" label="Purpose" :items="options" help="An optional setting to limit the selection of group types to those that have the selected purpose." />
</div>
`
});
