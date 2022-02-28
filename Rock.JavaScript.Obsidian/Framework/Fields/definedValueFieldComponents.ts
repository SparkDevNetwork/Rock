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
import { getFieldEditorProps } from "./utils";
import CheckBoxList from "../Elements/checkBoxList";
import DropDownList, { DropDownListOption } from "../Elements/dropDownList";
import { asBoolean } from "../Services/boolean";
import { ClientValue, ConfigurationValueKey, ValueItem } from "./definedValueField";
import { ListItem } from "../ViewModels";
import { toNumber } from "../Services/number";

function parseModelValue(modelValue: string | undefined): string {
    try {
        const clientValue = JSON.parse(modelValue ?? "") as ClientValue;

        return clientValue.value;
    }
    catch {
        return "";
    }
}

function getClientValue(value: string | string[], valueOptions: ValueItem[]): ClientValue {
    const values = Array.isArray(value) ? value : [value];
    const selectedValues = valueOptions.filter(v => values.includes(v.value));

    if (selectedValues.length >= 1) {
        return {
            value: selectedValues.map(v => v.value).join(","),
            text: selectedValues.map(v => v.text).join(", "),
            description: selectedValues.map(v => v.description).join(", ")
        };
    }
    else {
        return {
            value: "",
            text: "",
            description: ""
        };
    }
}

export const EditComponent = defineComponent({
    name: "DefinedValueField.Edit",

    components: {
        DropDownList,
        CheckBoxList
    },

    props: getFieldEditorProps(),

    setup(props, { emit }) {
        const internalValue = ref(parseModelValue(props.modelValue));
        const internalValues = ref(parseModelValue(props.modelValue).split(",").filter(v => v !== ""));

        const valueOptions = computed((): ValueItem[] => {
            try {
                return JSON.parse(props.configurationValues[ConfigurationValueKey.Values] ?? "[]") as ValueItem[];
            }
            catch {
                return [];
            }
        });

        /** The options to choose from in the drop down list */
        const options = computed((): DropDownListOption[] => {
            const providedOptions: DropDownListOption[] = valueOptions.value.map(v => {
                return {
                    text: v.text,
                    value: v.value
                };
            });

            return providedOptions;
        });

        /** The options to choose from in the drop down list */
        const optionsMultiple = computed((): ListItem[] => {
            return valueOptions.value.map(v => {
                return {
                    text: v.text,
                    value: v.value
                } as ListItem;
            });
        });

        const isMultiple = computed((): boolean => asBoolean(props.configurationValues[ConfigurationValueKey.AllowMultiple]));

        const configAttributes = computed((): Record<string, unknown> => {
            const attributes: Record<string, unknown> = {};

            const enhancedConfig = props.configurationValues[ConfigurationValueKey.EnhancedSelection];
            if (enhancedConfig) {
                attributes.enhanceForLongLists = asBoolean(enhancedConfig);
            }

            return attributes;
        });

        /** The number of columns wide the checkbox list will be. */
        const repeatColumns = computed((): number => toNumber(props.configurationValues[ConfigurationValueKey.RepeatColumns]));

        watch(() => props.modelValue, () => {
            internalValue.value = parseModelValue(props.modelValue);
            internalValues.value = parseModelValue(props.modelValue).split(",").filter(v => v !== "");
        });

        watch(() => internalValue.value, () => {
            if (!isMultiple.value) {
                const clientValue = getClientValue(internalValue.value, valueOptions.value);

                emit("update:modelValue", JSON.stringify(clientValue));
            }
        });

        watch(() => internalValues.value, () => {
            if (isMultiple.value) {
                const clientValue = getClientValue(internalValues.value, valueOptions.value);

                emit("update:modelValue", JSON.stringify(clientValue));
            }
        });

        return {
            configAttributes,
            internalValue,
            internalValues,
            isMultiple,
            isRequired: inject("isRequired") as boolean,
            options,
            optionsMultiple,
            repeatColumns
        };
    },

    template: `
<DropDownList v-if="!isMultiple" v-model="internalValue" v-bind="configAttributes" :options="options" :showBlankItem="!isRequired" />
<CheckBoxList v-else v-model="internalValues" :options="optionsMultiple" horizontal :repeatColumns="repeatColumns" />
`
});
