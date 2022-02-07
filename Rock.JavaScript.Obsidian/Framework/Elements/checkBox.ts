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
import RockFormField from "./rockFormField";

export default defineComponent({
    name: "CheckBox",

    components: {
        RockFormField
    },

    props: {
        modelValue: {
            type: Boolean as PropType<boolean>,
            required: true
        },

        label: {
            type: String as PropType<string>,
            required: true
        },

        rules: {
            type: String as PropType<string>,
            default: ""
        },

        text: {
            type: String as PropType<string>,
            default: ""
        }
    },

    setup(props, { emit }) {
        const internalValue = ref(props.modelValue);

        const toggle = (): void => {
            internalValue.value = !internalValue.value;
        };

        watch(() => props.modelValue, () => {
            internalValue.value = props.modelValue;
        });

        watch(internalValue, () => {
            emit("update:modelValue", internalValue.value);
        });

        return {
            internalValue,
            toggle
        };
    },

    template: `
<RockFormField
    :modelValue="modelValue"
    :label="label"
    formGroupClasses="rock-check-box"
    name="checkbox">
    <template #default="{uniqueId, field}">
    <div class="control-wrapper">
        <div class="checkbox">
            <label class="rock-checkbox-icon">
                <input type="checkbox" v-bind="field" v-model="internalValue" :id="uniqueId" />
                <span v-if="text" class="label-text">&nbsp;{{ text }}</span>
            </label>
        </div>
    </div>
    </template>
</RockFormField>
`
});
