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
import DropDownList from "./dropDownList";
import DatePicker from "./datePicker.obs";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { toNumber, toNumberOrNull } from "@Obsidian/Utility/numberUtils";
import { useHttp } from "@Obsidian/Utility/http";
import { SlidingDateRange, rangeTypeOptions, timeUnitOptions, TimeUnit } from "@Obsidian/Utility/slidingDateRange";

export default defineComponent({
    name: "SlidingDateRangePicker",

    components: {
        DatePicker,
        DropDownList,
        RockFormField
    },

    props: {
        modelValue: {
            type: Object as PropType<SlidingDateRange | null>,
            required: true
        }
    },

    emits: [
        "update:modelValue"
    ],

    setup(props, { emit }) {
        const internalValue = ref(props.modelValue);
        const http = useHttp();

        const rangeType = ref(internalValue.value?.rangeType?.toString() ?? "");
        const timeValue = ref(internalValue.value?.timeValue?.toString() ?? "");
        const timeUnit = ref(internalValue.value?.timeUnit?.toString() ?? "0");
        const lowDate = ref(internalValue.value?.lowerDate ?? "");
        const highDate = ref(internalValue.value?.upperDate ?? "");

        /** Contains the text that describes the currently selected range. */
        const dateRangeText = ref("");

        /** True if the selected range type is two dates that make up the range. */
        const isDateRange = computed((): boolean => {
            return rangeType.value === "2";
        });

        /** True if the selected range type is one based on time units. */
        const isTimeUnit = computed((): boolean => {
            return rangeType.value === "0" || rangeType.value === "1" || rangeType.value === "4" || rangeType.value === "8" || rangeType.value === "16";
        });

        /** True if the selected range type is one that takes a numerical value. */
        const isNumberVisible = computed((): boolean => {
            return rangeType.value === "0" || rangeType.value === "4" || rangeType.value === "8" || rangeType.value === "16";
        });

        /** The time unit options that will be made available to the user. */
        const computedTimeUnitOptions = computed((): ListItemBag[] => {
            if (!isNumberVisible.value || toNumber(timeValue.value) === 1) {
                return timeUnitOptions;
            }

            // Pluralize the time unit options if the time value isn't singular.
            return timeUnitOptions.map(o => {
                return {
                    value: o.value,
                    text: `${o.text}s`
                };
            });
        });

        /**
         * Updates the dateRangeText value to reflect the current selections
         * made by the user.
         */
        const updateDateRangeText = async (): Promise<void> => {
            const parameters: Record<string, string> = {
                slidingDateRangeType: rangeType.value || "0",
                timeUnitType: timeUnit.value || "0",
                number: timeValue.value || "1"
            };

            if (lowDate.value && highDate.value) {
                parameters["startDate"] = lowDate.value;
                parameters["endDate"] = highDate.value;
            }

            const result = await http.get<string>("/api/Utility/CalculateSlidingDateRange", parameters);

            if (result.isSuccess && result.data) {
                dateRangeText.value = result.data;
            }
            else {
                dateRangeText.value = "";
            }
        };

        // Watch for changes in our user interface values and update our internal
        // value with the computed information.
        watch([rangeType, timeUnit, timeValue, lowDate, highDate], () => {
            updateDateRangeText();

            const internalRangeType = toNumberOrNull(rangeType.value);

            if (internalRangeType === null) {
                internalValue.value = null;
                return;
            }

            const newValue: SlidingDateRange = {
                rangeType: internalRangeType
            };

            // These two checks could probably use isTimeUnit and isNumberVisible,
            // but I'm not sure if watch() runs before or after computed().
            if (rangeType.value === "0" || rangeType.value === "1" || rangeType.value === "4" || rangeType.value === "8" || rangeType.value === "16") {
                newValue.timeUnit = toNumberOrNull(timeUnit.value) as TimeUnit ?? undefined;
            }

            if (rangeType.value === "0" || rangeType.value === "4" || rangeType.value === "8" || rangeType.value === "16") {
                newValue.timeValue = toNumberOrNull(timeValue.value) ?? 1;
            }

            if (rangeType.value == "2") {
                newValue.lowerDate = lowDate.value;
                newValue.upperDate = highDate.value;
            }

            internalValue.value = newValue;
        });

        // Watch for changes in the model value and update our internal values.
        watch(() => props.modelValue, () => {
            internalValue.value = props.modelValue;
            rangeType.value = internalValue.value?.rangeType?.toString() ?? "";
            timeValue.value = internalValue.value?.timeValue?.toString() ?? "";
            timeUnit.value = internalValue.value?.timeUnit?.toString() ?? "";
            lowDate.value = internalValue.value?.lowerDate ?? "";
            highDate.value = internalValue.value?.upperDate ?? "";
        });

        // Watch for changes in our internal value and update the model value.
        watch(internalValue, () => {
            emit("update:modelValue", internalValue.value);
        });

        // Set the initial date range text on load.
        updateDateRangeText();

        return {
            dateRangeText,
            highDate,
            internalValue,
            isDateRange,
            isNumberVisible,
            isTimeUnit,
            lowDate,
            rangeType,
            rangeTypeOptions,
            timeUnit,
            timeUnitOptions: computedTimeUnitOptions,
            timeValue
        };
    },
    template: `
<RockFormField
    :modelValue="internalValue"
    formGroupClasses="slidingdaterange"
    name="slidingdaterange">
    <template #default="{uniqueId}">
        <div :id="uniqueId" class="form-control-group">
            <DropDownList v-model="rangeType" :items="rangeTypeOptions" showBlankItem class="input-width-md slidingdaterange-select" />

            <input v-if="isNumberVisible" v-model="timeValue" class="form-control input-width-sm slidingdaterange-number" type="number" pattern="[0-9]*]" />

            <template v-if="isTimeUnit">
                <DropDownList v-model="timeUnit" :items="timeUnitOptions" class="form-control input-width-md slidingdaterange-timeunits-plural" :showBlankItem="false" />

                <div class="label label-info slidingdaterange-info">{{ dateRangeText }}</div>
            </template>

            <div v-if="isDateRange" class="picker-daterange slidingdaterange-daterange pull-left">
                <div class="input-group input-group-lower input-width-md date">
                    <DatePicker v-model="lowDate" />
                </div>

                <div class="input-group form-control-static">to</div>

                <div class="input-group input-group-lower input-width-md date">
                    <DatePicker v-model="highDate" />
                </div>
            </div>
        </div>
    </template>
</RockFormField>`
});
