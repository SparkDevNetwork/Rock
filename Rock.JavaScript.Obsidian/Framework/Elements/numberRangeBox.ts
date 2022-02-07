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
import { normalizeRules, rulesPropType, ValidationRule } from "../Rules/index";
import { asFormattedString, toNumberOrNull } from "../Services/number";
import RockFormField from "./rockFormField";

/**
 * The model value used by the NumberRangeBox element.
 */
export type NumberRangeModelValue = {
    /** The lower number of the range. */
    lower: number | null;

    /** The upper number of the range. */
    upper: number | null;
};

export default defineComponent({
    name: "NumberRangeBox",

    components: {
        RockFormField
    },

    props: {
        modelValue: {
            type: Object as PropType<NumberRangeModelValue>,
            default: { lower: null, upper: null }
        },

        /** The number of decimal places allowed. */
        decimalCount: {
            type: Number as PropType<number | null>,
            default: null
        },

        inputClasses: {
            type: String as PropType<string>,
            default: ""
        },

        rules: rulesPropType
    },

    emits: [
        "update:modelValue"
    ],

    data: function () {
        return {
            internalValue: {
                lower: "",
                upper: ""
            },
            validationValue: ""
        };
    },

    methods: {
        onChange(): void {
            this.internalValue = {
                lower: asFormattedString(this.modelValue.lower, this.internalDecimalCount ?? undefined),
                upper: asFormattedString(this.modelValue.upper, this.internalDecimalCount ?? undefined)
            };
        }
    },

    computed: {
        computedValue(): NumberRangeModelValue {
            return {
                lower: toNumberOrNull(this.internalValue.lower),
                upper: toNumberOrNull(this.internalValue.upper)
            };
        },

        internalDecimalCount(): number | null {
            return this.decimalCount;
        },

        internalStep(): string {
            return this.internalDecimalCount === null ? "any" : (1 / Math.pow(10, this.internalDecimalCount)).toString();
        },

        computedRules(): ValidationRule[] {
            const rules = normalizeRules(this.rules);

            return rules;
        },
    },

    watch: {
        computedValue(): void {
            this.$emit("update:modelValue", this.computedValue);
        },

        internalValue(): void {
            const value = `${this.internalValue.lower ?? ""},${this.internalValue.upper ?? ""}`;
            this.validationValue = value;

            const emitValue = {
                lower: toNumberOrNull(this.internalValue.lower),
                upper: toNumberOrNull(this.internalValue.upper)
            } as NumberRangeModelValue;

            this.$emit("update:modelValue", emitValue);
        },

        internalStep(): string {
            return this.decimalCount === null ? "any" : (1 / Math.pow(10, this.decimalCount)).toString();
        },

        modelValue: {
            immediate: true,
            handler(): void {
                const lower = this.modelValue.lower !== null ? this.modelValue.lower.toString() : "";
                const upper = this.modelValue.upper !== null ? this.modelValue.upper.toString() : "";

                if (this.internalValue.lower !== lower || this.internalValue.upper !== upper) {
                    this.internalValue = {
                        lower: lower,
                        upper: upper
                    };
                }
            }
        }
    },
    template: `
<RockFormField
    v-model="validationValue"
    formGroupClasses="number-range-editor"
    name="number-range-box"
    :rules="computedRules">
    <template #default="{uniqueId, field}">
        <div class="control-wrapper">
            <div class="form-control-group">
                <input
                    :id="uniqueId + '_lower'"
                    @change="onChange"
                    type="number"
                    class="input-width-md form-control"
                    :class="inputClasses"
                    v-model="internalValue.lower"
                    :step="internalStep" />
                <span class="to">to</span>
                <input
                    :id="uniqueId + '_upper'"
                    @change="onChange"
                    type="number"
                    class="input-width-md form-control"
                    :class="inputClasses"
                    v-model="internalValue.upper"
                    :step="internalStep" />
            </div>
        </div>
    </template>
</RockFormField>`
});
