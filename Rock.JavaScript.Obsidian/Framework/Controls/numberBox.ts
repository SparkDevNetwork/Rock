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

import { computed, defineComponent, PropType, ref, watch } from "vue";
import { normalizeRules, rulesPropType, ValidationRule } from "@Obsidian/ValidationRules";
import { asFormattedString, toNumberOrNull } from "@Obsidian/Utility/numberUtils";
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
        inputGroupClasses: {
            type: String as PropType<string>,
            default: ""
        },
        rules: rulesPropType
    },

    emits: {
        "update:modelValue": (_value: number | null) => true
    },

    setup(props, ctx) {
        const internalValue = ref(asFormattedString(props.modelValue, props.decimalCount ?? undefined, { useGrouping: false }));

        const internalNumberValue = computed((): number | null => {
            return toNumberOrNull(internalValue.value);
        });

        const internalStep = computed((): string => {
            return props.decimalCount === null ? "any" : (1 / Math.pow(10, props.decimalCount)).toString();
        });

        const isInputGroup = computed((): boolean => {
            return !!ctx.slots.prepend || !!ctx.slots.append;
        });

        const controlContainerClass = computed((): string => {
            return isInputGroup.value ? `input-group ${props.inputGroupClasses}` : "";
        });

        const computedRules = computed((): ValidationRule[] => {
            const rules = normalizeRules(props.rules);

            if (props.maximumValue !== null && props.maximumValue !== undefined) {
                rules.push(`lte:${props.maximumValue}`);
            }

            if (props.minimumValue !== null && props.minimumValue !== undefined) {
                rules.push(`gte:${props.minimumValue}`);
            }

            return rules;
        });

        watch(() => props.modelValue, () => {
            if (props.modelValue !== internalNumberValue.value) {
                internalValue.value = asFormattedString(props.modelValue, props.decimalCount ?? undefined, { useGrouping: false });
            }
        });

        watch(internalNumberValue, () => {
            ctx.emit("update:modelValue", internalNumberValue.value);
        });

        return {
            computedRules,
            controlContainerClass,
            internalStep,
            internalValue
        };
    },

    template: `
<RockFormField
    :modelValue="internalValue"
    formGroupClasses="rock-number-box"
    name="numberbox"
    :rules="computedRules">
    <template #default="{uniqueId, field}">
        <div class="control-wrapper">
            <div :class="controlContainerClass">
                <slot name="prepend" />
                <input
                    v-model="internalValue"
                    :id="uniqueId"
                    type="number"
                    class="form-control"
                    :class="inputClasses"
                    v-bind="field"
                    :placeholder="placeholder"
                    :step="internalStep"
                    :min="minimumValue"
                    :max="maximumValue" />
                <slot name="append" />
            </div>
        </div>
    </template>
</RockFormField>`
});
