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

import { defineComponent, PropType } from "vue";
import { normalizeRules, rulesPropType, ValidationRule } from "../Rules/index";
import BasicTimePicker, { BasicTimePickerValue as TimePickerValue } from "./basicTimePicker";
import RockFormField from "./rockFormField";

export { BasicTimePickerValue as TimePickerValue } from "./basicTimePicker";

export default defineComponent({
    name: "TimePicker",
    components: {
        RockFormField,
        BasicTimePicker
    },
    props: {
        rules: rulesPropType,
        modelValue: {
            type: Object as PropType<TimePickerValue>,
            default: {}
        }
    },

    data() {
        return {
            internalValue: {} as TimePickerValue
        };
    },

    methods: {
    },

    computed: {
        computedRules(): ValidationRule[] {
            const rules = normalizeRules(this.rules);

            return rules;
        }
    },

    watch: {
        modelValue: {
            immediate: true,
            handler() {
                this.internalValue = this.modelValue;
            }
        },

        internalValue(): void {
            this.$emit("update:modelValue", this.internalValue);
        }
    },

    template: `
<RockFormField
    :modelValue="internalValue"
    formGroupClasses="timepicker-input"
    name="time-picker"
    :rules="computedRules">
    <template #default="{uniqueId, field}">
        <div class="control-wrapper">
            <div class="timepicker-input">
                <BasicTimePicker v-model="internalValue" />
            </div>
        </div>
    </template>
</RockFormField>`
});
