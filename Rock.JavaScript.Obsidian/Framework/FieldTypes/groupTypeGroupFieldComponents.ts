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
import TextBox from "@Obsidian/Controls/textBox.obs";
import DropDownList from "@Obsidian/Controls/dropDownList.obs";
import GroupTypeGroupPicker from "@Obsidian/Controls/groupTypeGroupPicker.obs";
import { ConfigurationValueKey, GroupAndGroupType } from "./groupTypeGroupField.partial";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";

export const EditComponent = defineComponent({

    name: "GroupTypeGroupField.Edit",

    components: {
        DropDownList,
        GroupTypeGroupPicker
    },

    props: getFieldEditorProps(),

    setup(props, { emit }) {
        const internalValue = ref({} as GroupAndGroupType);
        const group = ref({} as ListItemBag);
        const groupType = ref({} as ListItemBag);

        const groupPickerLabel = computed((): string => props.configurationValues[ConfigurationValueKey.GroupPickerLabel]);

        watch(() => props.modelValue, () => {
            if (props.modelValue) {
                internalValue.value = JSON.parse(props.modelValue || "{}");
                group.value = internalValue.value.group;
                groupType.value = internalValue.value.groupType;
            }
        }, { immediate: true });

        watch(() => [group.value, groupType.value], () => {
            const newValue = {
                groupType: groupType.value ?? null,
                group: group.value ?? null
            };
            emit("update:modelValue", JSON.stringify(newValue));
        }, { deep: true });

        return {
            groupType,
            group,
            groupPickerLabel
        };
    },

    template: `<GroupTypeGroupPicker v-model="group" v-model:groupType="groupType" :groupLabel="groupPickerLabel"/>`
});

export const ConfigurationComponent = defineComponent({
    name: "GroupTypeGroupField.Configuration",

    components: {
        TextBox
    },

    props: getFieldConfigurationProps(),

    emits: [
        "update:modelValue",
        "updateConfiguration",
        "updateConfigurationValue"
    ],

    setup(props, { emit }) {
        const groupPickerLabel = ref(props.modelValue[ConfigurationValueKey.GroupPickerLabel]);

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
            newValue[ConfigurationValueKey.GroupPickerLabel] = groupPickerLabel.value;

            // Compare the new value and the old value.
            const anyValueChanged = newValue[ConfigurationValueKey.GroupPickerLabel] !== (props.modelValue[ConfigurationValueKey.GroupPickerLabel]);

            // If any value changed then emit the new model value.
            if (anyValueChanged) {
                emit("update:modelValue", newValue);
            }
            return anyValueChanged;
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
                emit("updateConfiguration");
            }
        };

        // Watch for changes in properties that only require a local UI update.
        watch(groupPickerLabel, () => maybeUpdateConfiguration(ConfigurationValueKey.GroupPickerLabel, groupPickerLabel.value));

        return { groupPickerLabel };
    },

    template: `
<div>
    <TextBox v-model="groupPickerLabel" label="Group Picker Label" help="The label for the group picker" />
</div>
`
});
