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
import { ruleArrayToString, ruleStringToArray } from "../Rules/index";
import { asFormattedString, toNumberOrNull } from "../Services/number";
import RockFormField from "./rockFormField";

export default defineComponent({
    name: "NumberBox",
    components: {
        RockFormField
    },
    props: {
        modelValue: {
            type: Number as PropType<number | null>,
            default: null
        },
        placeholder: {
            type: String as PropType<string>,
            default: ""
        },
        /** The minimum allowed value to be entered. */
        minimumValue: {
            type: Number as PropType<number | null>
        },
        maximumValue: {
            type: Number as PropType<number | null>
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
        rules: {
            type: String as PropType<string>,
            default: ""
        }
    },
    emits: [
        "update:modelValue"
    ],
    data: function () {
        return {
            internalValue: "",
        };
    },
    methods: {
        onChange(): void {
            this.internalValue = asFormattedString(this.modelValue, this.internalDecimalCount ?? undefined);
        }
    },
    computed: {
        internalNumberValue(): number | null {
            return toNumberOrNull(this.internalValue);
        },
        internalDecimalCount(): number | null {
            return this.decimalCount;
        },
        internalStep(): string {
            return this.internalDecimalCount === null ? "any" : (1 / Math.pow(10, this.internalDecimalCount)).toString();
        },
        computedRules(): string {
            const rules = ruleStringToArray(this.rules);

            if (this.maximumValue !== null && this.maximumValue !== undefined) {
                rules.push(`lte:${this.maximumValue}`);
            }

            if (this.minimumValue !== null && this.minimumValue !== undefined) {
                rules.push(`gte:${this.minimumValue}`);
            }

            return ruleArrayToString(rules);
        },

        isGrouped(): boolean {
            return this.$slots.prepend !== undefined || this.$slots.append !== undefined;
        }
    },
    watch: {
        internalNumberValue(): void {
            this.$emit("update:modelValue", this.internalNumberValue);
        },
        modelValue: {
            immediate: true,
            handler(): void {
                if (this.modelValue !== this.internalNumberValue) {
                    this.internalValue = asFormattedString(this.modelValue, this.internalDecimalCount ?? undefined);
                }
            }
        }
    },
    template: `
<RockFormField
    v-model="internalValue"
    @change="onChange"
    formGroupClasses="rock-number-box"
    name="numberbox"
    :rules="computedRules">
    <template #default="{uniqueId, field, errors, disabled, tabIndex, inputGroupClasses}">
        <div class="control-wrapper">
            <div class="input-group" :class="inputGroupClasses" v-if="isGrouped">
                <slot name="prepend"></slot>
                <input
                    :id="uniqueId"
                    type="number"
                    class="form-control"
                    :class="inputClasses"
                    v-bind="field"
                    :disabled="disabled"
                    :placeholder="placeholder"
                    :tabindex="tabIndex"
                    :step="internalStep"
                    :min="minimumValue"
                    :max="maximumValue" />
                <slot name="append"></slot>
            </div>

            <input v-else
                :id="uniqueId"
                type="number"
                class="form-control"
                :class="inputClasses"
                v-bind="field"
                :disabled="disabled"
                :placeholder="placeholder"
                :tabindex="tabIndex"
                :step="internalStep"
                :min="minimumValue"
                :max="maximumValue" />
        </div>
    </template>
</RockFormField>`
});
