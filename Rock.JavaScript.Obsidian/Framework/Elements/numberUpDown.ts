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
import RockFormField from "./rockFormField";

export const NumberUpDownInternal = defineComponent({
    name: "NumberUpDownInternal",
    props: {
        modelValue: {
            type: Number as PropType<number>,
            required: true
        },
        min: {
            type: Number as PropType<number>,
            default: 1
        },
        max: {
            type: Number as PropType<number>,
            default: 10
        },
        disabled: {
            type: Boolean as PropType<boolean>,
            default: false
        }
    },
    data() {
        return {
            internalValue: 0
        };
    },
    methods: {
        goUp() {
            if (!this.isUpDisabled) {
                this.internalValue++;
            }
        },
        goDown() {
            if (!this.isDownDisabled) {
                this.internalValue--;
            }
        }
    },
    computed: {
        isUpDisabled(): boolean {
            return this.internalValue >= this.max;
        },
        isDownDisabled(): boolean {
            return this.internalValue <= this.min;
        }
    },
    watch: {
        modelValue: {
            immediate: true,
            handler() {
                this.internalValue = this.modelValue;
            }
        },
        internalValue() {
            this.$emit("update:modelValue", this.internalValue);
        }
    },
    template: `
<div class="numberincrement">
    <a @click="goDown" class="numberincrement-down" :class="{disabled: disabled || isDownDisabled}" :disabled="disabled || isDownDisabled">
        <i class="fa fa-minus "></i>
    </a>
    <span class="numberincrement-value">{{modelValue}}</span>
    <a @click="goUp" class="numberincrement-up" :class="{disabled: disabled || isUpDisabled}" :disabled="disabled || isUpDisabled">
        <i class="fa fa-plus "></i>
    </a>
</div>`
});

export default defineComponent({
    name: "NumberUpDown",
    components: {
        RockFormField,
        NumberUpDownInternal
    },
    props: {
        modelValue: {
            type: Number as PropType<number>,
            required: true
        },
        min: {
            type: Number as PropType<number>,
            default: 1
        },
        max: {
            type: Number as PropType<number>,
            default: 10
        },
        numberIncrementClasses: {
            type: String as PropType<string>,
            default: ""
        }
    },
    data() {
        return {
            internalValue: 0
        };
    },
    watch: {
        modelValue: {
            immediate: true,
            handler() {
                this.internalValue = this.modelValue;
            }
        },
        internalValue() {
            this.$emit("update:modelValue", this.internalValue);
        }
    },
    methods: {
        additionalClasses(fieldLabel: string): string {
            if (fieldLabel !== "") {
                return `margin-t-sm ${this.numberIncrementClasses}`;
            }
            else {
                return this.numberIncrementClasses;
            }
        }
    },
    template: `
<RockFormField
    :modelValue="internalValue"
    formGroupClasses="number-up-down"
    name="numberupdown">
    <template #default="{uniqueId, field}">
        <div class="control-wrapper">
            <NumberUpDownInternal v-model="internalValue" :min="min" :max="max" :class="additionalClasses(fieldLabel)" />
        </div>
    </template>
</RockFormField>`
});
