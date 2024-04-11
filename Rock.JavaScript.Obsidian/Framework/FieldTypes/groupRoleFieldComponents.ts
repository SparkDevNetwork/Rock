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
import GroupRolePicker from "@Obsidian/Controls/groupRolePicker.obs";
import GroupTypePicker from "@Obsidian/Controls/groupTypePicker.obs";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { ConfigurationValueKey } from "./groupRoleField.partial";
import { toGuidOrNull } from "@Obsidian/Utility/guid";
import { Guid } from "@Obsidian/Types";

export const EditComponent = defineComponent({
    name: "GroupRoleField.Edit",

    components: {
        GroupRolePicker
    },

    props: getFieldEditorProps(),

    setup(props, { emit }) {
        const internalValue = ref<ListItemBag>({});
        const groupTypeValue = ref<ListItemBag>({});

        const groupTypeGuid = computed<Guid | null>(() => toGuidOrNull(groupTypeValue.value?.value));

        watch(() => props.modelValue, () => {
            internalValue.value = JSON.parse(props.modelValue || "{}");
        }, { immediate: true });

        watch(() => props.configurationValues, () => {
            groupTypeValue.value = JSON.parse(props.configurationValues[ConfigurationValueKey.GroupType] || "{}");
        }, { immediate: true });

        watch(() => internalValue.value, () => {
            emit("update:modelValue", JSON.stringify(internalValue.value));
        });

        return {
            internalValue,
            groupTypeValue,
            groupTypeGuid
        };
    },

    template: `
<GroupRolePicker v-model="internalValue" :groupTypeGuid="groupTypeGuid" />
`
});


export const ConfigurationComponent = defineComponent({
    name: "GroupRoleField.Configuration",

    components: {
        GroupTypePicker
    },

    props: getFieldConfigurationProps(),

    emits: [
        "update:modelValue",
        "updateConfigurationValue"
    ],

    setup(props, { emit }) {
        const groupType = ref({});

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
            newValue[ConfigurationValueKey.GroupType] = JSON.stringify(groupType.value ?? "");

            // Compare the new value and the old value.
            const anyValueChanged = newValue[ConfigurationValueKey.GroupType] !== (props.modelValue[ConfigurationValueKey.GroupType] ?? "");

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
            groupType.value = JSON.parse(props.modelValue[ConfigurationValueKey.GroupType] || "{}");
        }, {
            immediate: true
        });

        watch(groupType, val => maybeUpdateConfiguration(ConfigurationValueKey.GroupType, JSON.stringify(val ?? "")));

        return {
            groupType
        };
    },

    template: `
<GroupTypePicker v-model="groupType"
    label="Group Type" help="Type of group to select roles from, if left blank any group type's role can be selected."
    showBlankItem="true" />
    `
});
