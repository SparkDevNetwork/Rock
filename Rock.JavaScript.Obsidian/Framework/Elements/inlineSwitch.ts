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
import { computed, defineComponent, PropType } from "vue";
import { useVModelPassthrough } from "../Util/component";
import { newGuid } from "../Util/guid";

export default defineComponent({
    name: "InlineSwitch",

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
        },

        isBold: {
            type: Boolean as PropType<boolean>,
            default: false
        },

        uniqueId: {
            type: String as PropType<string>,
            default: ""
        }
    },

    emits: [
        "update:modelValue"
    ],

    setup(props, { emit }) {
        const internalValue = useVModelPassthrough(props, "modelValue", emit);
        const internalUniqueId = `inline-switch-${newGuid()}`;

        const uniqueId = computed((): string => props.uniqueId || internalUniqueId);

        const labelClass = computed((): string[] => {
            const classes = ["custom-control-label"];

            if (props.isBold) {
                classes.push("custom-control-label-bold");
            }

            return classes;
        });

        return {
            labelClass,
            internalValue,
            uniqueId
        };
    },

    template: `
<div class="custom-control custom-switch">
    <input v-model="internalValue" :id="uniqueId" class="custom-control-input" type="checkbox" />
    <label :class="labelClass" :for="uniqueId">
        <template v-if="label">{{ label }}</template>
        <template v-else>&nbsp;</template>
    </label>
</div>
`
});
