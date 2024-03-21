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
import CheckBoxList from "@Obsidian/Controls/checkBoxList.obs";
import DropDownList from "@Obsidian/Controls/dropDownList.obs";
import ListBox from "@Obsidian/Controls/listBox.obs";
import NumberBox from "@Obsidian/Controls/numberBox.obs";
import TextBox from "@Obsidian/Controls/textBox.obs";
import { asBoolean, asBooleanOrNull, asTrueFalseOrNull } from "@Obsidian/Utility/booleanUtils";
import { toNumberOrNull } from "@Obsidian/Utility/numberUtils";
import { updateRefValue } from "@Obsidian/Utility/component";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { ConfigurationValueKey } from "./multiSelectField.partial";
import { getFieldConfigurationProps, getFieldEditorProps } from "./utils";

export const EditComponent = defineComponent({
    name: "MultiSelectField.Edit",

    components: {
        ListBox,
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
            internalValue: [] as string[]
        };
    },

    computed: {
        /** The options to choose from */
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

        /** Any additional attributes that will be assigned to the list box control */
        listBoxConfigAttributes(): Record<string, number | boolean> {
            const attributes: Record<string, number | boolean> = {};
            const enhancedSelection = this.configurationValues[ConfigurationValueKey.EnhancedSelection];

            if (asBoolean(enhancedSelection)) {
                attributes.enhanceForLongLists = true;
            }

            return attributes;
        },

        /** Any additional attributes that will be assigned to the check box list control */
        checkBoxListConfigAttributes(): Record<string, number | boolean> {
            const attributes: Record<string, number | boolean> = {};
            const repeatColumnsConfig = this.configurationValues[ConfigurationValueKey.RepeatColumns];
            const repeatDirection = this.configurationValues[ConfigurationValueKey.RepeatDirection];

            if (repeatColumnsConfig) {
                attributes["repeatColumns"] = toNumberOrNull(repeatColumnsConfig) || 0;
            }

            if (repeatDirection !== "1") {
                attributes["horizontal"] = true;
            }

            return attributes;
        },

        /** Is the control going to be list box? */
        isListBox(): boolean {
            const enhancedSelection = this.configurationValues[ConfigurationValueKey.EnhancedSelection];

            return asBoolean(enhancedSelection);
        }
    },

    watch: {
        internalValue() {
            this.$emit("update:modelValue", this.internalValue.join(","));
        },

        modelValue: {
            immediate: true,
            handler() {
                const value = this.modelValue || "";

                this.internalValue = value !== "" ? value.split(",") : [];
            }
        }
    },

    template: `
<ListBox v-if="isListBox && options.length > 0" v-model="internalValue" v-bind="listBoxConfigAttributes" :items="options" />
<CheckBoxList v-else-if="options.length > 0" v-model="internalValue" v-bind="checkBoxListConfigAttributes" :items="options" />
`
});

export const FilterComponent = defineComponent({
    name: "MultiSelectField.Filter",

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

const repeatDirectionOptions: ListItemBag[] = [
    {
        value: "0",
        text: "Horizontal"
    },
    {
        value: "1",
        text: "Vertical"
    }
];

export const ConfigurationComponent = defineComponent({
    name: "MultiSelectField.Configuration",

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
        const enhanceForLongLists = ref(false);
        const repeatColumns = ref<number | null>(null);
        const repeatDirection = ref("");

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
            newValue[ConfigurationValueKey.EnhancedSelection] = asTrueFalseOrNull(enhanceForLongLists.value) ?? "False";
            newValue[ConfigurationValueKey.RepeatColumns] = repeatColumns.value?.toString() ?? "";
            newValue[ConfigurationValueKey.RepeatDirection] = repeatDirection.value ?? "0";

            // Compare the new value and the old value.
            const anyValueChanged = newValue[ConfigurationValueKey.CustomValues] !== (props.modelValue[ConfigurationValueKey.CustomValues] ?? "")
                || newValue[ConfigurationValueKey.EnhancedSelection] !== (props.modelValue[ConfigurationValueKey.EnhancedSelection] ?? "False")
                || newValue[ConfigurationValueKey.RepeatColumns] !== (props.modelValue[ConfigurationValueKey.RepeatColumns] ?? "")
                || newValue[ConfigurationValueKey.RepeatDirection] !== (props.modelValue[ConfigurationValueKey.RepeatDirection] ?? "0");


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
            enhanceForLongLists.value = asBooleanOrNull(props.modelValue[ConfigurationValueKey.EnhancedSelection]) ?? false;
            repeatColumns.value = toNumberOrNull(props.modelValue[ConfigurationValueKey.RepeatColumns]);
            repeatDirection.value = props.modelValue[ConfigurationValueKey.RepeatDirection] ?? "0";
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
        watch(enhanceForLongLists, () => maybeUpdateConfiguration(ConfigurationValueKey.EnhancedSelection, asTrueFalseOrNull(enhanceForLongLists.value) ?? "False"));
        watch(repeatColumns, () => maybeUpdateConfiguration(ConfigurationValueKey.RepeatColumns, repeatColumns.value?.toString() ?? ""));
        watch(repeatDirection, () => maybeUpdateConfiguration(ConfigurationValueKey.RepeatDirection, repeatDirection.value ?? "0"));

        return {
            enhanceForLongLists,
            onBlur,
            rawValues,
            repeatColumns,
            repeatDirection,
            repeatDirectionOptions
        };
    },

    template: `
<div>
    <TextBox v-model="rawValues"
        label="Values"
        help="The source of the values to display in a list. Format is either 'value1,value2,value3,...', 'value1^text1,value2^text2,value3^text3,...', or a SQL Select statement that returns a result set with a 'Value' and 'Text' column <span class='tip tip-lava'></span>."
        textMode="multiline"
        @blur="onBlur" />

    <CheckBox v-model="enhanceForLongLists"
        label="Enhance For Long Lists"
        help="When set, will render a searchable selection of options." />

    <NumberBox
        v-model="repeatColumns"
        label="Columns"
        help="Select how many columns the list should use before going to the next row. If blank or 0 then 4 columns will be displayed. There is no enforced upper limit however the block this control is used in might add contraints due to available space." />

    <DropDownList v-model="repeatDirection"
        label="Repeat Direction"
        help="The direction that the list options will be displayed."
        :items="repeatDirectionOptions"
        :showBlankItem="false" />
</div>
`
});
