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
import { defineComponent, ref, watch } from "vue";
import { getFieldEditorProps, getFieldConfigurationProps } from "./utils";
import { useHttp } from "@Obsidian/Utility/http";
import DropDownList from "@Obsidian/Controls/dropDownList.obs";
import GroupTypeGroupPicker from "@Obsidian/Controls/groupTypeGroupPicker.obs";
import RockLabel from "@Obsidian/Controls/rockLabel.obs";
import TextBox from "@Obsidian/Controls/textBox.obs";
import { ConfigurationValueKey, GroupAndRoleValue } from "./groupAndRoleField.partial";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { GroupRolePickerGetGroupRolesOptionsBag } from "@Obsidian/ViewModels/Rest/Controls/groupRolePickerGetGroupRolesOptionsBag";
import { areEqual, emptyGuid, toGuidOrNull } from "@Obsidian/Utility/guid";

const http = useHttp();

export const EditComponent = defineComponent({
    name: "GroupAndRoleField.Edit",

    components: {
        DropDownList,
        RockLabel,
        GroupTypeGroupPicker
    },

    props: getFieldEditorProps(),

    setup(props, { emit }) {

        const internalValue = ref({} as GroupAndRoleValue);
        const group = ref({} as ListItemBag);
        const groupType = ref({} as ListItemBag);
        const groupRoleValue = ref("");
        const roleOptions = ref([] as ListItemBag[]);
        const groupLabel = ref("");

        watch(() => props.modelValue, () => {
            if (props.modelValue) {
                internalValue.value = JSON.parse(props.modelValue || "{}");
                group.value = internalValue.value.group;
                groupType.value = internalValue.value.groupType;
                groupRoleValue.value = internalValue.value.groupRole?.value ?? "";
                roleOptions.value = internalValue.value.groupTypeRoles;
            }
            groupLabel.value = props.configurationValues[ConfigurationValueKey.GroupRolePickerLabel] ?? "Group";
        }, { immediate: true });

        watch(() => [group.value, groupType.value, groupRoleValue], () => {

            const newValue = {
                groupType: groupType.value,
                group: group.value,
                groupRole: { value: groupRoleValue.value },
                groupTypeRoles: roleOptions.value
            };
            emit("update:modelValue", JSON.stringify(newValue));
        }, { deep: true });

        // Watch for changes to the groupType value and get the corresponding roles
        watch(() => groupType.value, async () => {
            const groupTypeGuid = toGuidOrNull(groupType.value?.value);

            if (groupTypeGuid && !areEqual(groupTypeGuid, emptyGuid)) {
                const options: GroupRolePickerGetGroupRolesOptionsBag = {
                    groupTypeGuid,
                };

                const result = await http.post<ListItemBag[]>("/api/v2/Controls/GroupRolePickerGetGroupRoles", null, options);

                if (result.isSuccess && result.data) {
                    roleOptions.value = result.data ?? [];
                }
                else {
                    roleOptions.value = [];
                }
            }
            else{
                roleOptions.value = [];
            }

        });

        return {
            group,
            groupType,
            groupRoleValue,
            roleOptions,
            groupLabel
        };
    },

    template: `
    <div>
        <RockLabel>Default Value</RockLabel>
        <GroupTypeGroupPicker :groupLabel="groupLabel" v-model="group" v-model:groupType="groupType" />
        <DropDownList v-model="groupRoleValue" label="Role" :items="roleOptions" :multiple="false" />
    </div>
`
});

export const ConfigurationComponent = defineComponent({
    name: "GroupAndRoleField.Configuration",

    components: {
        TextBox
    },

    props: getFieldConfigurationProps(),

    emits: [
        "update:modelValue",
        "updateConfigurationValue"
    ],

    setup(props, { emit }) {
        const groupAndRolePickerLabel = ref("");

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
            newValue[ConfigurationValueKey.GroupRolePickerLabel] = groupAndRolePickerLabel.value ?? "";

            const anyValueChanged = newValue[ConfigurationValueKey.GroupRolePickerLabel] !== props.modelValue[ConfigurationValueKey.GroupRolePickerLabel];

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
            groupAndRolePickerLabel.value = props.modelValue[ConfigurationValueKey.GroupRolePickerLabel] ?? "";
        }, {
            immediate: true
        });

        watch(groupAndRolePickerLabel, val => maybeUpdateConfiguration(ConfigurationValueKey.GroupRolePickerLabel, val ?? ""));

        return {
            groupAndRolePickerLabel
        };
    },

    template: `
    <TextBox label="Group/Role Picker Label" v-model="groupAndRolePickerLabel" help="The label for the group/role picker." />
    `
});
