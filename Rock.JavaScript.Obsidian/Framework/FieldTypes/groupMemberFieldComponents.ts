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
import GroupPicker from "@Obsidian/Controls/groupPicker.obs";
import GroupMemberPicker from "@Obsidian/Controls/groupMemberPicker.obs";
import CheckBox from "@Obsidian/Controls/checkBox.obs";
import { asBoolean, asTrueFalseOrNull } from "@Obsidian/Utility/booleanUtils";
import { ConfigurationValueKey } from "./groupMemberField.partial";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { toGuidOrNull } from "@Obsidian/Utility/guid";
import { Guid } from "@Obsidian/Types";

export const EditComponent = defineComponent({
    name: "GroupMemberField.Edit",

    components: {
        GroupMemberPicker
    },

    props: getFieldEditorProps(),

    setup(props, { emit }) {
        const internalValue = ref({} as ListItemBag[]);

        watch(() => props.modelValue, () => {
            internalValue.value = JSON.parse(props.modelValue || "{}");
        }, { immediate: true });

        const groupGuid = computed<Guid | null>(() => {
            const groupListItemBag: ListItemBag | null = JSON.parse(props.configurationValues[ConfigurationValueKey.Group] || "{}") as ListItemBag;
            return toGuidOrNull(groupListItemBag?.value);
        });

        const allowMultipleValues = computed((): boolean => {
            return asBoolean(props.configurationValues[ConfigurationValueKey.AllowMultipleValues]);
        });

        const enhanceForLongLists = computed((): boolean => {
            return  asBoolean(props.configurationValues[ConfigurationValueKey.EnhanceForLongLists]);
        });

        watch(() => internalValue.value, () => {
            emit("update:modelValue", JSON.stringify(internalValue.value));
        });

        return {
            internalValue,
            groupGuid,
            allowMultipleValues,
            enhanceForLongLists
        };
    },

    template: `
    <GroupMemberPicker v-model="internalValue"
        :groupGuid="groupGuid"
        :multiple="allowMultipleValues"
        :enhanceForLongLists="enhanceForLongLists"
        showBlankItem="true"/>
`
});


export const ConfigurationComponent = defineComponent({
    name: "GroupMemberField.Configuration",

    components: {
        CheckBox,
        GroupPicker
    },

    props: getFieldConfigurationProps(),

    emits: [
        "update:modelValue",
        "updateConfigurationValue"
    ],

    setup(props, { emit }) {
        const allowMultipleValues = ref(false);
        const enhanceForLongLists = ref(false);
        const group = ref({});

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
            newValue[ConfigurationValueKey.AllowMultipleValues] = asTrueFalseOrNull(allowMultipleValues.value) ?? "False";
            newValue[ConfigurationValueKey.EnhanceForLongLists] = asTrueFalseOrNull(enhanceForLongLists.value) ?? "False";
            newValue[ConfigurationValueKey.Group] = JSON.stringify(group.value ?? "");

            // Compare the new value and the old value.
            const anyValueChanged = newValue[ConfigurationValueKey.AllowMultipleValues] !== (props.modelValue[ConfigurationValueKey.AllowMultipleValues] ?? "False")
                || newValue[ConfigurationValueKey.EnhanceForLongLists] !== (props.modelValue[ConfigurationValueKey.EnhanceForLongLists] ?? "False")
                || newValue[ConfigurationValueKey.Group] !== (props.modelValue[ConfigurationValueKey.Group] ?? "");

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
            allowMultipleValues.value = asBoolean(props.modelValue[ConfigurationValueKey.AllowMultipleValues]);
            enhanceForLongLists.value = asBoolean(props.modelValue[ConfigurationValueKey.EnhanceForLongLists]);
            group.value = JSON.parse(props.modelValue[ConfigurationValueKey.Group] || "{}");
        }, {
            immediate: true
        });

        watch(allowMultipleValues, val => maybeUpdateConfiguration(ConfigurationValueKey.AllowMultipleValues, asTrueFalseOrNull(val) ?? "False"));
        watch(enhanceForLongLists, val => maybeUpdateConfiguration(ConfigurationValueKey.EnhanceForLongLists, asTrueFalseOrNull(val) ?? "False"));
        watch(group, val => maybeUpdateConfiguration(ConfigurationValueKey.Group, JSON.stringify(val ?? "")));

        return {
            allowMultipleValues,
            enhanceForLongLists,
            group
        };
    },

    template: `
<div>
    <GroupPicker v-model="group" label="Group" help="The group to select the member(s) from."/>
    <CheckBox v-model="allowMultipleValues" label="Allow Multiple Values" text="Yes" help="When set, allows multiple group members to be selected." />
    <CheckBox v-model="enhanceForLongLists" label="Enhance For Long Lists" text="Yes" help="When set, will render a searchable selection of options." />
</div>
    `
});