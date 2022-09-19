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
import { computed, defineComponent, PropType, ref, watch } from "vue";
import DropDownList from "./dropDownList";
import RockFormField from "./rockFormField";
import TextBox from "./textBox";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";

export type KeyValueItem = {
    key?: string | null;

    value?: string | null;
};

export default defineComponent({
    name: "KeyValueListField.Edit",

    components: {
        RockFormField,
        DropDownList,
        TextBox
    },

    props: {
        modelValue: {
            type: Array as PropType<KeyValueItem[]>,
            required: false
        },

        valueOptions: {
            type: Array as PropType<ListItemBag[]>,
            required: false
        },

        keyPlaceholder: {
            type: String as PropType<string>,
            required: false
        },

        valuePlaceholder: {
            type: String as PropType<string>,
            required: false
        },

        displayValueFirst: {
            type: Boolean as PropType<boolean>,
            default: false
        }
    },

    emits: {
        "update:modelValue": (_value: KeyValueItem[]) => true
    },

    setup(props, { emit }) {
        const internalValues = ref(props.modelValue ?? []);

        /** The options to choose from in the drop down list */
        const options = computed((): ListItemBag[] => props.valueOptions ?? []);

        const hasValues = computed((): boolean => options.value.length > 0);

        watch(() => props.modelValue, () => {
            internalValues.value = props.modelValue ?? [];
        });

        watch(() => internalValues.value, () => {
            emit("update:modelValue", internalValues.value);
        }, {
            deep: true
        });

        const onAddClick = (): void => {
            let defaultValue = "";

            if (hasValues.value) {
                defaultValue = options.value[0].value ?? "";
            }

            internalValues.value.push({ key: "", value: defaultValue });
        };

        const onRemoveClick = (index: number): void => {
            internalValues.value.splice(index, 1);
        };

        return {
            internalValues,
            hasValues,
            options,
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
