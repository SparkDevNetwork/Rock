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
import { computed, defineComponent, inject, ref, watch } from "vue";
import { getFieldConfigurationProps, getFieldEditorProps } from "./utils";
import CheckBoxList from "@Obsidian/Controls/checkBoxList.obs";
import DropDownList from "@Obsidian/Controls/dropDownList.obs";
import NumberBox from "@Obsidian/Controls/numberBox.obs";
import RadioButtonList from "@Obsidian/Controls/radioButtonList.obs";
import TextBox from "@Obsidian/Controls/textBox.obs";
import { toNumberOrNull } from "@Obsidian/Utility/numberUtils";
import { ConfigurationValueKey } from "./singleSelectField.partial";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { updateRefValue } from "@Obsidian/Utility/component";

export const EditComponent = defineComponent({
    name: "SingleSelectField.Edit",

    components: {
        DropDownList,
        RadioButtonList
    },

    props: getFieldEditorProps(),

    setup() {
        return {
            isRequired: inject("isRequired") as boolean
        };
    },

    data() {
        return {
            internalValue: ""
        };
    },

    computed: {
        /** The options to choose from in the drop down list */
        options(): ListItemBag[] {
            try {
                const providedOptions = JSON.parse(this.configurationValues[ConfigurationValueKey.Values] ?? "[]") as ListItemBag[];

                if (this.isRadioButtons && !this.isRequired) {
                    providedOptions.unshift({
                        text: "None",
                        value: ""
                    });
                }

                return providedOptions;
            }
            catch {
                return [];
            }
        },

        /** Any additional attributes that will be assigned to the drop down list control */
        ddlConfigAttributes(): Record<string, number | boolean> {
            const attributes: Record<string, number | boolean> = {};
            const fieldTypeConfig = this.configurationValues[ConfigurationValueKey.FieldType];

            if (fieldTypeConfig === "ddl_enhanced") {
                attributes.enhanceForLongLists = true;
            }

            return attributes;
        },

        /** Any additional attributes that will be assigned to the radio button control */
        rbConfigAttributes(): Record<string, number | boolean> {
            const attributes: Record<string, number | boolean> = {};
            const repeatColumnsConfig = this.configurationValues[ConfigurationValueKey.RepeatColumns];

            if (repeatColumnsConfig) {
                attributes["repeatColumns"] = toNumberOrNull(repeatColumnsConfig) || 0;
            }

            return attributes;
        },

        /** Is the control going to be radio buttons? */
        isRadioButtons(): boolean {
            const fieldTypeConfig = this.configurationValues[ConfigurationValueKey.FieldType];
            return fieldTypeConfig === "rb";
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
<RadioButtonList v-if="isRadioButtons" v-model="internalValue" v-bind="rbConfigAttributes" :items="options" horizontal />
<DropDownList v-else v-model="internalValue" v-bind="ddlConfigAttributes" :items="options" />
`
});

export const FilterComponent = defineComponent({
    name: "SingleSelectField.Filter",

    components: {
        CheckBoxList
    },

    props: getFieldEditorProps(),

    setup(props, { emit }) {
        const internalValue = ref(props.modelValue.split(",").filter(v => v !== ""));

        const options = computed((): ListItemBag[] => {
            try {
                const providedOptions = JSON.parse(props.configurationValues[ConfigurationValueKey.Values] ?? "[]") as ListItemBag[];

                return providedOptions;
            }
            catch {
                return [];
            }
        });

        watch(() => props.modelValue, () => {
            updateRefValue(internalValue, props.modelValue.split(",").filter(v => v !== ""));
        });

        watch(internalValue, () => {
            emit("update:modelValue", internalValue.value.join(","));
        });

        return {
            internalValue,
            options
        };
    },

    template: `
<CheckBoxList v-model="internalValue" :items="options" horizontal />
`
});

const controlTypeOptions: ListItemBag[] = [
    {
        value: "ddl",
        text: "Drop Down List"
    },
    {
        value: "ddl_enhanced",
        text: "Drop Down List (Enhanced for Long Lists)"
    },
    {
        value: "rb",
        text: "Radio Buttons"
    }
];

export const ConfigurationComponent = defineComponent({
    name: "SingleSelectField.Configuration",

    components: {
        DropDownList,
        TextBox,
        NumberBox
    },

    props: getFieldConfigurationProps(),

    emits: [
        "update:modelValue",
        "updateConfiguration",
        "updateConfigurationValue"
    ],

    setup(props, { emit }) {
        // Define the properties that will hold the current selections.
        const rawValues = ref("");
        const internalRawValues = ref("");
        const controlType = ref("");
        const repeatColumns = ref<number | null>(null);

        const isRadioList = computed((): boolean => {
            return controlType.value === "rb";
        });

        const onBlur = (): void => {
            internalRawValues.value = rawValues.value;
        };

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
            newValue[ConfigurationValueKey.CustomValues] = internalRawValues.value ?? "";
            newValue[ConfigurationValueKey.FieldType] = controlType.value ?? "";
            newValue[ConfigurationValueKey.RepeatColumns] = repeatColumns.value?.toString() ?? "";

            // Compare the new value and the old value.
            const anyValueChanged = newValue[ConfigurationValueKey.CustomValues] !== (props.modelValue[ConfigurationValueKey.CustomValues] ?? "")
                || newValue[ConfigurationValueKey.FieldType] !== (props.modelValue[ConfigurationValueKey.FieldType] ?? "")
                || newValue[ConfigurationValueKey.RepeatColumns] !== (props.modelValue[ConfigurationValueKey.RepeatColumns] ?? "");

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
            rawValues.value = props.modelValue[ConfigurationValueKey.CustomValues] ?? "";
            internalRawValues.value = rawValues.value;
            controlType.value = props.modelValue[ConfigurationValueKey.FieldType] ?? "ddl";
            repeatColumns.value = toNumberOrNull(props.modelValue[ConfigurationValueKey.RepeatColumns]);
        }, {
            immediate: true
        });

        // Watch for changes in properties that require new configuration
        // properties to be retrieved from the server.
        watch([internalRawValues], () => {
            if (maybeUpdateModelValue()) {
                emit("updateConfiguration");
            }
        });

        // Watch for changes in properties that only require a local UI update.
        watch(controlType, () => maybeUpdateConfiguration(ConfigurationValueKey.FieldType, controlType.value ?? "ddl"));
        watch(repeatColumns, () => maybeUpdateConfiguration(ConfigurationValueKey.RepeatColumns, repeatColumns.value?.toString() ?? ""));

        return {
            controlType,
            controlTypeOptions,
            isRadioList,
            onBlur,
            rawValues,
            repeatColumns
        };
    },

    template: `
<div>
    <TextBox v-model="rawValues"
        label="Values"
        help="The source of the values to display in a list.  Format is either 'value1,value2,value3,...', 'value1^text1,value2^text2,value3^text3,...', or a SQL Select statement that returns a result set with a 'Value' and 'Text' column <span class='tip tip-lava'></span>."
        textMode="multiline"
        @blur="onBlur" />

    <DropDownList v-model="controlType"
        label="Control Type"
        help="The type of control to use for selecting a single value from the list."
        :items="controlTypeOptions"
        :showBlankItem="false" />

    <NumberBox v-if="isRadioList"
        v-model="repeatColumns"
        label="Columns"
        help="Select how many columns the list should use before going to the next row. If blank or 0 then 4 columns will be displayed. There is no enforced upper limit however the block this control is used in might add contraints due to available space." />
</div>
`
});
