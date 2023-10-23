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
import { defineComponent, inject, ref, watch } from "vue";
import TextBox from "@Obsidian/Controls/textBox.obs";
import NumberBox from "@Obsidian/Controls/numberBox.obs";
import DropDownList from "@Obsidian/Controls/dropDownList.obs";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { ConfigurationValueKey } from "./noteTypeField.partial";
import { getFieldConfigurationProps, getFieldEditorProps } from "./utils";

export const EditComponent = defineComponent({
    name: "NoteTypeField.Edit",

    components: {
        DropDownList
    },

    props: getFieldEditorProps(),

    setup() {
        return {
            isRequired: inject("isRequired") as boolean
        };
    },

    data() {
        return {
            internalValue: this.modelValue ?? ""
        };
    },

    computed: {
        options(): ListItemBag[] {
            try {
                const valuesConfig = JSON.parse(this.configurationValues[ConfigurationValueKey.Values] ?? "[]") as ListItemBag[];
                return valuesConfig.map(v => {
                    return {
                        text: v.text,
                        value: v.value
                    } as ListItemBag;
                });
            }
            catch {
                return [];
            }
        }
    },

    watch: {
        internalValue() {
            this.$emit("update:modelValue", this.internalValue);
        },
        modelValue: {
            immediate: true,
            handler() {
                this.internalValue = this.modelValue || "";
            }
        }
    },

    template: `
        <DropDownList v-model="internalValue" :items="options" :repeatColumns="repeatColumns" horizontal />
    `
});

export const ConfigurationComponent = defineComponent({
    name: "NoteTypeField.Configuration",

    components: {
        TextBox,
        DropDownList,
        NumberBox
    },

    props: getFieldConfigurationProps(),

    computed: {
        options(): ListItemBag[] {
            try {
                const valuesConfig = JSON.parse(this.modelValue[ConfigurationValueKey.EntityTypes] ?? "[]") as ListItemBag[];
                return valuesConfig.map(v => {
                    return {
                        text: v.text,
                        value: v.value
                    } as ListItemBag;
                });
            }
            catch {
                return [];
            }
        }
    },

    emits: [
        "update:modelValue",
        "updateConfiguration",
        "updateConfigurationValue"
    ],

    setup(props, { emit }) {
        const entityTypeName = ref(props.modelValue[ConfigurationValueKey.EntityTypeName]?? {});
        const qualifierColumn = ref(props.modelValue[ConfigurationValueKey.QualifierColumn]);
        const qualifierValue = ref(props.modelValue[ConfigurationValueKey.QualifierValue]);

        /**
         * Update the modelValue property if any value of the dictionary has
         * actually changed. This helps prevent unwanted postbacks if the value
         * didn't really change - which can happen if multiple values get updated
         * at the same time.
         *
         * @returns true if a new modelValue was emitted to the parent component.
         */
        const maybeUpdateModelValue = (): boolean => {
            const newValue: Record<string, string> = {...props.modelValue};

            // Construct the new value that will be emitted if it is different
            // than the current value.
            newValue[ConfigurationValueKey.EntityTypeName] = entityTypeName.value ?? "";
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
                emit("updateConfiguration");
            }
        };

        // Watch for changes in properties that only require a local UI update.
        watch(entityTypeName, () => maybeUpdateConfiguration(ConfigurationValueKey.EntityTypeName, entityTypeName.value ?? ""));
        watch(qualifierColumn, () => maybeUpdateConfiguration(ConfigurationValueKey.QualifierColumn, qualifierColumn.value ?? ""));
        watch(qualifierValue, () => maybeUpdateConfiguration(ConfigurationValueKey.QualifierValue, qualifierValue.value ?? ""));

        return {
            entityTypeName,
            qualifierColumn,
            qualifierValue
        };
    },

    template: `
<div>
    <DropDownList v-model="entityTypeName" :items="options" label="Entity Type" help="The type of entity to display categories for." />
    <TextBox v-model="qualifierColumn" label="Qualifier Column" help="Entity column qualifier." />
    <TextBox v-model="qualifierValue" label="Qualifier Value" help="Entity column value." />
</div>
`
});
