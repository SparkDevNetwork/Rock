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
import { defineComponent, PropType, ref, watch } from "vue";
import { NumberUpDownInternal } from "./numberUpDown";
import RockFormField from "./rockFormField";
import { updateRefValue } from "@Obsidian/Utility/component";

export type NumberUpDownGroupOption = {
    key: string,
    label: string,
    min: number,
    max: number
};

export default defineComponent({
    name: "NumberUpDownGroup",
    components: {
        RockFormField,
        NumberUpDownInternal
    },
    props: {
        modelValue: {
            type: Object as PropType<Record<string, number>>,
            required: true
        },
        options: {
            type: Array as PropType<NumberUpDownGroupOption[]>,
            required: true
        }
    },
    setup(props, { emit }) {
        const internalValue = ref({...props.modelValue});

        function setValue(key: string, value: number): void {
            const newValue: Record<string, number> = {};

            for (const option of props.options) {
                newValue[option.key] = key === option.key
                    ? value
                    : internalValue.value[option.key] ?? 0;
            }

            internalValue.value = newValue;
        }

        watch(() => props.modelValue, () => {
            updateRefValue(internalValue, props.modelValue);
        });

        watch(internalValue, () => {
            emit("update:modelValue", internalValue.value);
        });

        return {
            internalValue,
            setValue
        };
    },
    computed: {
        total(): number {
            let total = 0;

            for (const option of this.options) {
                total += (this.internalValue[option.key] || 0);
            }

            return total;
        }
    },
    template: `
<RockFormField
    :modelValue="total"
    formGroupClasses="margin-b-md number-up-down-group"
    name="numberupdowngroup">
    <template #default="{uniqueId, field}">
        <div class="control-wrapper">
            <div v-for="option in options" :key="option.key" class="margin-l-sm margin-b-sm">
                <div v-if="option.label" class="margin-b-sm">
                    {{option.label}}
                </div>
                <NumberUpDownInternal :modelValue="internalValue[option.key] ?? 0" @update:modelValue="setValue(option.key, $event)" :min="option.min" :max="option.max" class="margin-t-sm" />
            </div>
        </div>
    </template>
</RockFormField>`
});
