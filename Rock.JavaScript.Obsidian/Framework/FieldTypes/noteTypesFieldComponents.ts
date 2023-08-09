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
import CheckBoxList from "@Obsidian/Controls/checkBoxList.obs";
import TextBox from "@Obsidian/Controls/textBox.obs";
import NumberBox from "@Obsidian/Controls/numberBox.obs";
import DropDownList from "@Obsidian/Controls/dropDownList.obs";
import { toNumberOrNull } from "@Obsidian/Utility/numberUtils";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { ConfigurationValueKey } from "./noteTypesField.partial";
import { getFieldConfigurationProps, getFieldEditorProps } from "./utils";

export const EditComponent = defineComponent({
    name: "NoteTypesField.Edit",

    components: {
        CheckBoxList
    },

    props: getFieldEditorProps(),

    setup() {
        return {
            isRequired: inject("isRequired") as boolean
        };
    },

    data() {
        return {
            internalValue: this.modelValue ? this.modelValue.split(",") : []
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
        },

        repeatColumns(): number {
            return Number(this.configurationValues[ConfigurationValueKey.RepeatColumns]) ?? 1;
        }
    },

    watch: {
        internalValue() {
            this.$emit("update:modelValue", this.internalValue.join(","));
        }
    },

    template: `
<CheckBoxList v-model="internalValue" :items="options" :repeatColumns="repeatColumns" horizontal />
`
});

export const ConfigurationComponent = defineComponent({
    name: "NoteTypesField.Configuration",

    components: {
        TextBox,
        DropDownList,
        NumberBox
    },

    props: getFieldConfigurationProps(),

    emits: [
        "update:modelValue",
        "updateConfiguration",
        "updateConfigurationValue"
    ],

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

    setup(props, { emit }) {
        // Define the properties that will hold the current selections.
        const repeatColumns = ref<number | null>(null);

        const entityTypeName = ref(props.modelValue[ConfigurationValueKey.EntityTypeName]);
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
            newValue[ConfigurationValueKey.RepeatColumns] = repeatColumns.value?.toString() ?? "";
            newValue[ConfigurationValueKey.EntityTypeName] = entityTypeName.value ?? "";
            newValue[ConfigurationValueKey.QualifierColumn] = qualifierColumn.value ?? "";
            newValue[ConfigurationValueKey.QualifierValue] = qualifierValue.value ?? "";

            // Compare the new value and the old value.
            const anyValueChanged = newValue[ConfigurationValueKey.RepeatColumns] !== (props.modelValue[ConfigurationValueKey.RepeatColumns] ?? "")
                || newValue[ConfigurationValueKey.EntityTypeName] !== (props.modelValue[ConfigurationValueKey.EntityTypeName] ?? "")
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

        // Watch for changes coming in from the parent component and update our
        // data to match the new information.
        watch(() => [props.modelValue, props.configurationProperties], () => {
            repeatColumns.value = toNumberOrNull(props.modelValue[ConfigurationValueKey.RepeatColumns]);
        }, {
            immediate: true
        });

        // Watch for changes in properties that require new configuration
        // properties to be retrieved from the server.
        watch([], () => {
            if (maybeUpdateModelValue()) {
                emit("updateConfiguration");
            }
        });

        // Watch for changes in properties that only require a local UI update.
        watch(repeatColumns, () => maybeUpdateConfiguration(ConfigurationValueKey.RepeatColumns, repeatColumns.value?.toString() ?? ""));
        watch(entityTypeName, () => maybeUpdateConfiguration(ConfigurationValueKey.EntityTypeName, entityTypeName.value ?? ""));
        watch(qualifierColumn, () => maybeUpdateConfiguration(ConfigurationValueKey.QualifierColumn, qualifierColumn.value ?? ""));
        watch(qualifierValue, () => maybeUpdateConfiguration(ConfigurationValueKey.QualifierValue, qualifierValue.value ?? ""));

        return {
            entityTypeName,
            qualifierColumn,
            qualifierValue,
            repeatColumns
        };
    },

    template: `
<div>
    <DropDownList v-model="entityTypeName" :items="options" label="Entity Type" help="The type of entity to display categories for." />
    <TextBox v-model="qualifierColumn" label="Qualifier Column" help="Entity column qualifier." />
    <TextBox v-model="qualifierValue" label="Qualifier Value" help="Entity column value." />
    <NumberBox v-model="repeatColumns" label="Columns" help="Select how many columns the list should use before going to the next row. If blank or 0 then 4 columns will be displayed. There is no upper limit enforced here however the block this is used in might add contraints due to available space." />
</div>
`
});
