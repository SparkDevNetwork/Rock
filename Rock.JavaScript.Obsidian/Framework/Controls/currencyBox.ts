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

import { defineComponent, PropType } from "vue";
import NumberBox from "./numberBox";

export default defineComponent({
    name: "CurrencyBox",
    components: {
        NumberBox
    },
    props: {
        modelValue: {
            type: Number as PropType<number | null>,
            default: null
        },

        /** The minimum allowed value to be entered. */
        minimumValue: {
            type: Number as PropType<number | null>
        },

        maximumValue: {
            type: Number as PropType<number | null>
        },
    },
    emits: [
        "update:modelValue"
    ],
    data: function () {
        return {
            internalValue: null as number | null
        };
    },
    computed: {
        placeholder(): string {
            return "0.00";
        }
    },
    watch: {
        internalValue() {
            this.$emit("update:modelValue", this.internalValue);
        },
        modelValue: {
            immediate: true,
            handler() {
                if (this.modelValue !== this.internalValue) {
                    this.internalValue = this.modelValue;
                }
            }
        }
    },
    template: `
<NumberBox v-model="internalValue"
    :placeholder="placeholder"
    :minimum-value="minimumValue"
    :maximum-value="maximumValue"
    :decimal-count="2"
    rules="decimal">
    <template v-slot:prepend>
        <span class="input-group-addon">$</span>
    </template>
</NumberBox>
`
});
