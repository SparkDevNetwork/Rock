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
import CategoryPicker from "@Obsidian/Controls/categoryPicker.obs";
import TextBox from "@Obsidian/Controls/textBox.obs";
import EntityTypePicker from "@Obsidian/Controls/entityTypePicker.obs";
import { ConfigurationValueKey } from "./categoryField.partial";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { Guid } from "@Obsidian/Types";
import { toGuidOrNull } from "@Obsidian/Utility/guid";

export const EditComponent = defineComponent({
    name: "CategoryField.Edit",

    components: {
        CategoryPicker
    },

    props: getFieldEditorProps(),

    setup(props, { emit }) {

        const internalValue = ref({} as ListItemBag);

        watch(() => props.modelValue, () => {
            internalValue.value = JSON.parse(props.modelValue || "{}");
        }, { immediate: true });

        watch(() => internalValue.value, () => {
            emit("update:modelValue", JSON.stringify(internalValue.value));
        });

        const entityTypeGuid = computed<Guid | null>(() => {
            const entityType = JSON.parse(props.configurationValues[ConfigurationValueKey.EntityTypeName] ?? "{}") as ListItemBag;
            return toGuidOrNull(entityType?.value);
        });

        return {
            internalValue,
            entityTypeGuid
        };
    },

    template: `
    <CategoryPicker v-model="internalValue" :entityTypeGuid="entityTypeGuid" enhanceForLongLists showBlankItem/>
`
});


export const ConfigurationComponent = defineComponent({
    name: "CategoryField.Configuration",

    components: {
        EntityTypePicker,
        TextBox
    },

    props: getFieldConfigurationProps(),

    emits: [
        "update:modelValue",
        "updateConfigurationValue"
    ],

    setup(props, { emit }) {
        const entityType = ref({} as ListItemBag);
        const qualifierColumn = ref("");
        const qualifierValue = ref("");

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
            newValue[ConfigurationValueKey.EntityTypeName] = JSON.stringify(entityType.value ?? "");
            newValue[ConfigurationValueKey.QualifierColumn] = qualifierColumn.value ?? "";
            newValue[ConfigurationValueKey.QualifierValue] = qualifierValue.value ?? "";

            // Compare the new value and the old value.
            const anyValueChanged = newValue[ConfigurationValueKey.EntityTypeName] !== (props.modelValue[ConfigurationValueKey.EntityTypeName] ?? "")
                || newValue[ConfigurationValueKey.QualifierColumn] !== (props.modelValue[ConfigurationValueKey.QualifierColumn] ?? "")
                || newValue[ConfigurationValueKey.QualifierValue] !== (props.modelValue[ConfigurationValueKey.QualifierValue] ?? "");

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
            entityType.value = JSON.parse(props.modelValue[ConfigurationValueKey.EntityTypeName] || "{}");
            qualifierColumn.value = props.modelValue[ConfigurationValueKey.QualifierColumn] || "";
            qualifierValue.value = props.modelValue[ConfigurationValueKey.QualifierValue] || "";
        }, {
            immediate: true
        });

        watch(entityType, val => maybeUpdateConfiguration(ConfigurationValueKey.EntityTypeName, JSON.stringify(val ?? "")));
        watch(qualifierColumn, val => maybeUpdateConfiguration(ConfigurationValueKey.QualifierColumn, val ?? ""));
        watch(qualifierValue, val => maybeUpdateConfiguration(ConfigurationValueKey.QualifierValue, val ?? ""));

        return {
            qualifierColumn,
            qualifierValue,
            entityType
        };
    },

    template: `
<div>
    <EntityTypePicker label="Entity Type" v-model="entityType" help="The type of entity to display categories for." />
    <TextBox v-model="qualifierColumn" label="Qualifier Column" help="Entity column qualifier."/>
    <TextBox v-model="qualifierValue" label="Qualifier Value" text="Yes" help="Entity column value." />
</div>
    `
});
