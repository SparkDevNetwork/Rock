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
import ListBox from "@Obsidian/Controls/listBox.obs";
import NumberBox from "@Obsidian/Controls/numberBox.obs";
import ListItems from "@Obsidian/Controls/listItems.obs";
import { toNumberOrNull } from "@Obsidian/Utility/numberUtils";
import { updateRefValue } from "@Obsidian/Utility/component";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { ConfigurationValueKey } from "./checkListField.partial";
import { getFieldConfigurationProps, getFieldEditorProps } from "./utils";
import { KeyValueItem } from "@Obsidian/Types/Controls/keyValueItem";

export const EditComponent = defineComponent({
    name: "CheckListField.Edit",

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
                const listItems = JSON.parse(this.configurationValues[ConfigurationValueKey.ListItems] ?? "[]") as KeyValueItem[];

                return listItems.map(v => {
                    return {
                        text: v.value,
                        value: v.key
                    } as ListItemBag;
                });
            }
            catch {
                return [];
            }
        },

        /** Any additional attributes that will be assigned to the check box list control */
        checkBoxListConfigAttributes(): Record<string, number | boolean> {
            const attributes: Record<string, number | boolean> = {};
            const repeatColumnsConfig = this.configurationValues[ConfigurationValueKey.RepeatColumns];

            if (repeatColumnsConfig) {
                attributes["repeatColumns"] = toNumberOrNull(repeatColumnsConfig) || 0;
            }

            return attributes;
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
<CheckBoxList v-model="internalValue" v-bind="checkBoxListConfigAttributes" displayAsCheckList="true" :items="options" />
`
});

export const FilterComponent = defineComponent({
    name: "CheckListField.Filter",

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

export const ConfigurationComponent = defineComponent({
    name: "CheckListField.Configuration",

    components: {
        ListItems,
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
        const listItems = ref("");
        const repeatColumns = ref<number | null>(null);

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
            newValue[ConfigurationValueKey.ListItems] = listItems.value ?? "[]";
            newValue[ConfigurationValueKey.RepeatColumns] = repeatColumns.value?.toString() ?? "";

            // Compare the new value and the old value.
            const anyValueChanged = newValue[ConfigurationValueKey.ListItems] !== (props.modelValue[ConfigurationValueKey.ListItems] ?? "[]")
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
            listItems.value = props.modelValue[ConfigurationValueKey.ListItems] ?? "[]";
            repeatColumns.value = toNumberOrNull(props.modelValue[ConfigurationValueKey.RepeatColumns]);
        }, {
            immediate: true
        });

        // Watch for changes in properties that require new configuration
        // properties to be retrieved from the server.
        watch([listItems], () => {
            if (maybeUpdateModelValue()) {
                emit("updateConfiguration");
            }
        });

        // Watch for changes in properties that only require a local UI update.
        watch(repeatColumns, () => maybeUpdateConfiguration(ConfigurationValueKey.RepeatColumns, repeatColumns.value?.toString() ?? ""));

        return {
            listItems,
            repeatColumns
        };
    },

    template: `
<div>
    <ListItems
        v-model="listItems"
        label="ListItems"
        help="The list of the values to display." />

    <NumberBox
        v-model="repeatColumns"
        label="Columns"
        help="Select how many columns the list should use before going to the next row. If blank or 0 then 4 columns will be displayed. There is no enforced upper limit however the block this control is used in might add contraints due to available space." />
</div>
`
});