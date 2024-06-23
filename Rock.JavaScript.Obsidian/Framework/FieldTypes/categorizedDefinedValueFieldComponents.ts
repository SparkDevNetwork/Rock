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
import CheckBoxList from "@Obsidian/Controls/checkBoxList.obs";
import DropDownList from "@Obsidian/Controls/dropDownList.obs";
import CategorizedValuePicker from "@Obsidian/Controls/categorizedValuePicker.obs";
import RockLabel from "@Obsidian/Controls/rockLabel.obs";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { ConfigurationValueKey } from "./categorizedDefinedValueField.partial";
import { Guid } from "@Obsidian/Types";
import { emptyGuid, toGuidOrNull } from "@Obsidian/Utility/guid";

export const EditComponent = defineComponent({
    name: "CategorizedDefinedValueField.Edit",

    components: {
        CategorizedValuePicker,
        RockLabel
    },

    props: getFieldEditorProps(),

    setup(props, { emit }) {
        const internalValue = ref<ListItemBag>({});
        const definedTypeValue = computed<Guid>(() => {
            const definedType = JSON.parse(props.configurationValues[ConfigurationValueKey.DefinedType] || "{}") as ListItemBag;
            return toGuidOrNull(definedType.value) ?? emptyGuid;
        });

        const selectableValues = computed((): ListItemBag[] => {
            try {
                return JSON.parse(props.configurationValues[ConfigurationValueKey.SelectableDefinedValues] || "[]") as ListItemBag[];
            }
            catch {
                return [];
            }
        });

        const isEditMode = computed((): boolean => {
            return props.configurationValues[ConfigurationValueKey.ConfigurationMode] === "Edit";
        });

        watch(() => props.modelValue, () => {
            internalValue.value = JSON.parse(props.modelValue || "{}");
        }, { immediate: true });

        watch(() => internalValue.value, () => {
            emit("update:modelValue", JSON.stringify(internalValue.value));
        }, { deep: true });

        return {
            internalValue,
            definedTypeValue,
            selectableValues,
            isEditMode
        };
    },

    template: `
    <CategorizedValuePicker v-if="isEditMode" label="Categorized Defined Value" v-model="internalValue" :definedTypeGuid="definedTypeValue" :onlyIncludeGuids="selectableValues" />
    `
});


export const ConfigurationComponent = defineComponent({
    name: "CategorizedDefinedValueField.Configuration",

    components: {
        DropDownList,
        CheckBoxList
    },

    props: getFieldConfigurationProps(),

    emits: [
        "update:modelValue",
        "updateConfigurationValue"
    ],

    setup(props, { emit }) {
        const definedType = ref<ListItemBag>({});
        const selectableDefinedValues = ref<string[]>([]);

        const definedTypes = computed((): ListItemBag[] => {
            return JSON.parse(props.modelValue[ConfigurationValueKey.DefinedTypes] || "[]");
        });

        /** The defined value options to choose from in the check box list */
        const options = computed((): ListItemBag[] => {
            try {
                const definedTypeValue = definedType.value;

                if (definedTypeValue.value) {
                    const definedTypeValues = JSON.parse(props.modelValue[ConfigurationValueKey.DefinedTypeValues] || "{}");
                    return JSON.parse(definedTypeValues[definedTypeValue.value] || "[]") as ListItemBag[];
                }
                else {
                    return [];
                }
            }
            catch {
                return [];
            }
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
            newValue[ConfigurationValueKey.DefinedType] = JSON.stringify(definedType.value ?? "");
            newValue[ConfigurationValueKey.SelectableDefinedValues] = JSON.stringify(selectableDefinedValues.value ?? "");
            newValue[ConfigurationValueKey.DefinedTypeValues] = props.modelValue[ConfigurationValueKey.DefinedTypeValues];
            newValue[ConfigurationValueKey.DefinedTypes] = props.modelValue[ConfigurationValueKey.DefinedTypes];
            newValue[ConfigurationValueKey.ConfigurationMode] = props.modelValue[ConfigurationValueKey.ConfigurationMode];

            // Compare the new value and the old value.
            const anyValueChanged = newValue[ConfigurationValueKey.DefinedType] !== (props.modelValue[ConfigurationValueKey.DefinedType] ?? "")
            || newValue[ConfigurationValueKey.SelectableDefinedValues] !== (props.modelValue[ConfigurationValueKey.SelectableDefinedValues] ?? "");

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
            definedType.value = JSON.parse(props.modelValue[ConfigurationValueKey.DefinedType] || "{}");
            selectableDefinedValues.value = JSON.parse(props.modelValue[ConfigurationValueKey.SelectableDefinedValues] || "[]");
        }, {
            immediate: true
        });

        watch(definedType, val => maybeUpdateConfiguration(ConfigurationValueKey.DefinedType, JSON.stringify(val ?? "")));
        watch(selectableDefinedValues, val => maybeUpdateConfiguration(ConfigurationValueKey.SelectableDefinedValues, JSON.stringify(val ?? "")));

        return {
            definedType,
            definedTypes,
            selectableDefinedValues,
            options
        };
    },

    template: `
    <DropDownList label="Defined Type" v-model="definedType.value" :items="definedTypes" help="A Defined Type that is configured to support categorized values." :showBlankItem="true" :enhanceForLongLists="true" />
    <CheckBoxList v-if="definedType.value" label="Selectable Values" v-model="selectableDefinedValues" :items="options" horizontal repeatColumns="4" />
    `
});
