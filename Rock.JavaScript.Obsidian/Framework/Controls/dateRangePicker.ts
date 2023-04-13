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
import { computed, defineComponent, PropType, ref, watch } from "vue";
import RockFormField from "./rockFormField";
import DatePickerBase from "./datePickerBase.obs";

export type DateRangeParts = {
    lowerValue?: string | null,
    upperValue?: string | null
};

export default defineComponent({
    name: "DateRangePicker",
    components: {
        RockFormField,
        DatePickerBase
    },

    props: {
        modelValue: {
            type: Object as PropType<DateRangeParts>,
            default: {}
        },

        /** Whether or not the user should be able to select dates in the past. NOT Reactive */
        disallowPastDateSelection: {
            type: Boolean as PropType<boolean>,
            default: false
        },
    },

    setup(props, { emit }) {
        const lowerValue = ref(props.modelValue.lowerValue ?? "");
        const upperValue = ref(props.modelValue.upperValue ?? "");

        const internalValue = computed(() => {
            if (lowerValue.value === "" && upperValue.value === "") {
                return "";
            }

            return `{lowerValue.value},{upperValue.value}`;
        });

        const basePickerProps = computed(() => {
            return {
                disallowPastDateSelection: props.disallowPastDateSelection
            };
        });

        watch(() => props.modelValue, () => {
            lowerValue.value = props.modelValue.lowerValue ?? "";
            upperValue.value = props.modelValue.upperValue ?? "";
        });

        watch(() => [lowerValue.value, upperValue.value], () => {
            emit("update:modelValue", {
                lowerValue: lowerValue.value,
                upperValue: upperValue.value
            } as DateRangeParts);
        });

        return {
            internalValue,
            lowerValue,
            upperValue,
            basePickerProps
        };
    },

    template: `
<RockFormField formGroupClasses="date-range-picker" #default="{uniqueId}" name="daterangepicker" v-model.lazy="internalValue">
    <div class="control-wrapper">
        <div class="picker-daterange">
            <div class="form-control-group">
                <DatePickerBase v-model="lowerValue" v-bind="basePickerProps" />
                <div class="input-group form-control-static"> to </div>
                <DatePickerBase v-model="upperValue" />
            </div>
        </div>
    </div>
</RockFormField>`
});
