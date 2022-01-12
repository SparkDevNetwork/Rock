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
import { asBoolean } from "../Services/boolean";
import { computed, defineComponent, ref, watch } from "vue";
import DropDownList from "../Elements/dropDownList";
import RockFormField from "../Elements/rockFormField";
import TextBox from "../Elements/textBox";
import { getFieldEditorProps } from "./utils";
import { ClientValue, ConfigurationValueKey, ValueItem } from "./keyValueListField";
import { ListItem } from "../ViewModels";

function parseModelValue(modelValue: string | undefined): ClientValue[] {
    try {
        return JSON.parse(modelValue ?? "[]") as ClientValue[];
    }
    catch {
        return [];
    }
}

export const EditComponent = defineComponent({
    name: "KeyValueListField.Edit",

    components: {
        RockFormField,
        DropDownList,
        TextBox
    },

    props: getFieldEditorProps(),

    setup(props, { emit }) {
        const internalValues = ref(parseModelValue(props.modelValue));

        const valueOptions = computed((): ValueItem[] => {
            try {
                return JSON.parse(props.configurationValues[ConfigurationValueKey.Values] ?? "[]") as ValueItem[];
            }
            catch {
                return [];
            }
        });

        /** The options to choose from in the drop down list */
        const options = computed((): ListItem[] => {
            const providedOptions: ListItem[] = valueOptions.value.map(v => {
                return {
                    text: v.text,
                    value: v.value
                };
            });

            return providedOptions;
        });

        const hasValues = computed((): boolean => valueOptions.value.length > 0);

        const keyPlaceholder = computed((): string => {
            return props.configurationValues[ConfigurationValueKey.KeyPrompt] ?? "";
        });

        const valuePlaceholder = computed((): string => {
            return props.configurationValues[ConfigurationValueKey.ValuePrompt] ?? "";
        });

        const displayValueFirst = computed((): boolean => {
            return asBoolean(props.configurationValues[ConfigurationValueKey.DisplayValueFirst] ?? "");
        });

        watch(() => props.modelValue, () => {
            internalValues.value = parseModelValue(props.modelValue);
        });

        watch(() => internalValues.value, () => {
            emit("update:modelValue", JSON.stringify(internalValues.value));
        }, {
            deep: true
        });

        const onAddClick = (): void => {
            let defaultValue = "";

            if (hasValues.value) {
                defaultValue = valueOptions.value[0].value;
            }

            internalValues.value.push({ key: "", value: defaultValue });
        };

        const onRemoveClick = (index: number): void => {
            internalValues.value.splice(index, 1);
        };

        return {
            internalValues,
            hasValues,
            displayValueFirst,
            options,
            keyPlaceholder,
            valuePlaceholder,
            onAddClick,
            onRemoveClick
        };
    },

    template: `
<RockFormField
    :modelValue="internalValues"
    formGroupClasses="key-value-list"
    name="key-value-list">
    <template #default="{uniqueId}">
        <div class="control-wrapper">
<span :id="uniqueId" class="key-value-list">
    <span class="key-value-rows">
        <div v-for="(value, valueIndex) in internalValues" class="controls controls-row form-control-group">
            <template v-if="!displayValueFirst">
                <input v-model="value.key" class="key-value-key form-control input-width-md" type="text" :placeholder="keyPlaceholder">

                <select v-if="hasValues" v-model="value.value" class="form-control input-width-lg">
                    <option v-for="option in options" :value="option.value" :key="option.value">{{ option.text }}</option>
                </select>
                <input v-else v-model="value.value" class="key-value-value form-control input-width-md" type="text" :placeholder="valuePlaceholder">
            </template>
            <template v-else>
                <select v-if="hasValues" v-model="value.value" class="form-control input-width-lg">
                    <option v-for="option in options" :value="option.value" :key="option.value">{{ option.text }}</option>
                </select>
                <input v-else v-model="value.value" class="key-value-value form-control input-width-md" type="text" :placeholder="valuePlaceholder">

                <input v-model="value.key" class="key-value-key form-control input-width-md" type="text" :placeholder="keyPlaceholder">
            </template>

            <a href="#" @click.prevent="onRemoveClick(valueIndex)" class="btn btn-sm btn-danger"><i class="fa fa-times"></i></a>
        </div>
    </span>
    <div class="control-actions">
        <a class="btn btn-action btn-square btn-xs" href="#" @click.prevent="onAddClick"><i class="fa fa-plus-circle"></i></a>
    </div>
</span>
        </div>
    </template>
</RockFormField>
`
});
