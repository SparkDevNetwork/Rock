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

export default defineComponent({
    name: "InlineCheckBox",

    components: {
    },

    props: {
        modelValue: {
            type: Boolean as PropType<boolean>,
            required: true
        },

        label: {
            type: String as PropType<string>,
            required: true
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
            label: props.label,
            toggle
        };
    },

    template: `
<div class="checkbox">
    <label title="">
        <input type="checkbox" v-model="internalValue" />
        <span class="label-text ">{{label}}</span>
    </label>
</div>
`
});
