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
import RockFormField from "@Obsidian/Controls/rockFormField.obs";
import { ClientValue, ConfigurationValueKey, ValueItem } from "./definedValueRangeField.partial";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { asBoolean } from "@Obsidian/Utility/booleanUtils";
import { List } from "@Obsidian/Utility/linq";

function parseModelValue(modelValue: string | undefined): string[] {
    try {
        const clientValue = JSON.parse(modelValue ?? "") as ClientValue;
        const splitValue = (clientValue.value ?? "").split(",");

        if (splitValue.length === 1) {
            return [splitValue[0], ""];
        }

        return splitValue;
    }
    catch {
        return ["", ""];
    }
}

function getClientValue(lowerValue: string, upperValue: string, valueOptions: ValueItem[], showDescription: boolean): ClientValue {
    const options = new List(valueOptions);
    const lv = options.firstOrUndefined(v => v.value === lowerValue);
    const uv = options.firstOrUndefined(v => v.value === upperValue);

    if (!lv && !uv) {
        return {
            value: "",
            text: "",
            description: ""
        };
    }

    return {
        value: `${lv?.value ?? ""},${uv?.value ?? ""}`,
        text: `${lv?.text ?? ""} to ${uv?.text ?? ""}`,
        description: showDescription ? `${lv?.description ?? ""} to ${uv?.description ?? ""}` : ""
    };
}

export const EditComponent = defineComponent({
    name: "DefinedValueRangeField.Edit",

    components: {
        RockFormField
    },

    props: getFieldEditorProps(),

    setup(props, { emit }) {
        const internalValues = parseModelValue(props.modelValue);
        const internalValue = ref(props.modelValue);
        const lowerValue = ref(internalValues[0]);
        const upperValue = ref(internalValues[1]);

        const valueOptions = computed((): ValueItem[] => {
            try {
                return JSON.parse(props.configurationValues[ConfigurationValueKey.Values] ?? "[]") as ValueItem[];
            }
            catch {
                return [];
            }
        });

        const showDescription = computed((): boolean => {
            return asBoolean(props.configurationValues[ConfigurationValueKey.DisplayDescription]);
        });

        /** The options to choose from in the drop down list */
        const options = computed((): ListItemBag[] => {
            const providedOptions: ListItemBag[] = valueOptions.value.map(v => {
                return {
                    text: showDescription.value ? v.description : v.text,
                    value: v.value
                };
            });

            return providedOptions;
        });

        watch(() => props.modelValue, () => {
            const internalValues = parseModelValue(props.modelValue);

            lowerValue.value = internalValues[0];
            upperValue.value = internalValues[1];
        });

        watch(() => [lowerValue.value, upperValue.value], () => {
            const clientValue = getClientValue(lowerValue.value, upperValue.value, valueOptions.value, showDescription.value);

            emit("update:modelValue", JSON.stringify(clientValue));
        });

        return {
            internalValue,
            lowerValue,
            upperValue,
            isRequired: inject("isRequired") as boolean,
            options,
            getKeyForOption(option: ListItemBag): string {
                return option.value ?? "";
            },
            getTextForOption(option: ListItemBag): string {
                return option.text ?? "";
            }
        };
    },

    template: `
<RockFormField
    v-model="internalValue"
    formGroupClasses="rock-defined-value-range"
    name="definedvaluerange"
    #default="{uniqueId}"
    :rules="computedRules">
    <div :id="uniqueId" class="form-control-group">
        <select class="input-width-md form-control" v-model="lowerValue">
            <option v-if="!isRequired" value=""></option>
            <option v-for="o in options" :key="o.value" :value="o.value">{{o.text}}</option>
        </select>
        <span class="to"> to </span>
        <select class="input-width-md form-control" v-model="upperValue">
            <option v-if="!isRequired" value=""></option>
            <option v-for="o in options" :key="o.value" :value="o.value">{{o.text}}</option>
        </select>
    </div>
</RockFormField>
`
});
