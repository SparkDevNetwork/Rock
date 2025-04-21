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
import { ConfigurationValueKey, ConfigurationPropertyKey } from "./workflowField.partial";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import DropDownList from "@Obsidian/Controls/dropDownList.obs";
import { updateRefValue } from "@Obsidian/Utility/component";
import WorkflowPicker from "@Obsidian/Controls/workflowPicker.obs";

export const EditComponent = defineComponent({
    name: "WorkflowField.Edit",

    components: {
        WorkflowPicker
    },

    props: getFieldEditorProps(),

    setup(props, { emit }) {
        // The internal value used by the text editor.
        const internalValue = ref<ListItemBag>({});

        // The configured WorkflowType guid.
        const workflowTypeGuid = computed((): string => {
            const value = props.configurationValues[ConfigurationValueKey.WorkflowTypeGuid];
            return value ?? "";
        });

        // Watch for changes from the parent component and update the workflow picker.
        watch(() => props.modelValue, () => {
            updateRefValue(internalValue, JSON.parse(props.modelValue || "{}"));
        }, {
            immediate: true
        });

        // Watch for changes from the workflow picker and update the parent component.
        watch(internalValue, () => {
            emit("update:modelValue", JSON.stringify(internalValue.value));
        });

        return {
            internalValue,
            workflowTypeGuid
        };
    },

    template: `
    <WorkflowPicker v-model="internalValue" :workflowTypeGuid="workflowTypeGuid" showBlankItem />
`
});

export const ConfigurationComponent = defineComponent({
    name: "WorkflowField.Configuration",

    components: {
        DropDownList
    },

    props: getFieldConfigurationProps(),

    emits: ["update:modelValue", "updateConfiguration", "updateConfigurationValue"],

    setup(props, { emit }) {
        // Define the properties that will hold the current selections.
        const workflowType = ref<string>();

        // The options for the workflow type dropdown list
        const options = computed((): ListItemBag[] => {
            const workflowTypeOptions = props.configurationProperties[ConfigurationPropertyKey.WorkflowTypeOptions];
            return workflowTypeOptions ? JSON.parse(workflowTypeOptions) as ListItemBag[] : [];
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
            newValue[ConfigurationValueKey.WorkflowTypeGuid] = workflowType.value ?? "";

            // Compare the new value and the old value.
            const anyValueChanged = newValue[ConfigurationValueKey.WorkflowTypeGuid] !== (props.modelValue[ConfigurationValueKey.WorkflowTypeGuid]);

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
            workflowType.value = props.modelValue[ConfigurationValueKey.WorkflowTypeGuid];
        }, {
            immediate: true
        });

        // Watch for changes in properties that only require a local UI update.
        watch(workflowType, () => maybeUpdateConfiguration(ConfigurationValueKey.WorkflowTypeGuid, workflowType.value ?? ""));

        return {
            workflowType,
            options
        };
    },

    template: `
    <DropDownList v-model="workflowType"
        label="Workflow Type"
        :items="options"
        :showBlankItem="true"
        help="Workflow Type to select workflows from, if left blank any workflow type's workflows can be selected." />
`
});
