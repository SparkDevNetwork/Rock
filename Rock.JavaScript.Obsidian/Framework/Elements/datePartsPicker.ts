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
import DateKey from "../Services/dateKey";
import { toNumber, toNumberOrNull } from "../Services/number";
import { RockDateTime } from "../Util/rockDateTime";
import RockFormField from "./rockFormField";

export type DatePartsPickerValue = {
    year: number;
    month: number;
    day: number;
};

export function getDefaultDatePartsPickerModel(): DatePartsPickerValue {
    return {
        year: 0,
        month: 0,
        day: 0
    };
}

export default defineComponent({
    name: "DatePartsPicker",
    components: {
        RockFormField
    },
    props: {
        rules: rulesPropType,
        modelValue: {
            type: Object as PropType<DatePartsPickerValue>,
            required: true
        },
        requireYear: {
            type: Boolean as PropType<boolean>,
            default: true
        },
        showYear: {
            type: Boolean as PropType<boolean>,
            default: true
        },
        allowFutureDates: {
            type: Boolean as PropType<boolean>,
            default: true
        },
        futureYearCount: {
            type: Number as PropType<number>,
            default: 50
        },
        startYear: {
            type: Number as PropType<number>,
            default: 1900
        },
        disabled: {
            type: String as PropType<string>
        }
    },

    data() {
        return {
            internalDay: "0",
            internalMonth: "0",
            internalYear: "0",
            days: [] as Array<string>
        };
    },

    methods: {
        getValue(): DatePartsPickerValue {
            return {
                day: toNumber(this.internalDay),
                month: toNumber(this.internalMonth),
                year: toNumber(this.internalYear)
            };
        },

        updateDays(): void {
            let dayCount = 31;

            const year = toNumber(this.internalYear);
            const month = toNumber(this.internalMonth);
            if (this.showYear && year > 0 && month > 0) {
                dayCount = RockDateTime.fromParts(year, month, 1)?.addMonths(1)?.addDays(-1)?.day ?? 31;
            }
            else if ([1, 3, 5, 7, 8, 10, 12].indexOf(month) !== -1) {
                dayCount = 31;
            }
            else if ([4, 6, 9, 11].indexOf(month) !== -1) {
                dayCount = 30;
            }
            else if (month === 2) {
                dayCount = 29;
            }

            const days: Array<string> = [];
            for (let day = 1; day <= dayCount; day++) {
                days.push(day.toString());
            }

            this.days = days;
        }
    },

    computed: {
        computedRequireYear(): boolean {
            return this.showYear && this.requireYear;
        },
        internalDateKey (): string {
            if (!this.modelValue.year && !this.computedRequireYear) {
                const dateKey = DateKey.toNoYearDateKey(this.modelValue.month, this.modelValue.day);
                return dateKey;
            }

            const dateKey = DateKey.toDateKey(this.modelValue.year, this.modelValue.month, this.modelValue.day);
            return dateKey;
        },
        computedRules (): ValidationRule[] {
            const rules = normalizeRules(this.rules);

            if (rules.indexOf("required") !== -1 && rules.indexOf("datekey") === -1) {
                rules.push("datekey");
            }

            return rules;
        },
        years (): string[] {
            const years: string[] = [];
            let year = RockDateTime.now().year;

            if (this.futureYearCount > 0 && this.allowFutureDates) {
                year += this.futureYearCount;
            }

            while (year >= 1900) {
                years.push(year.toString());
                year--;
            }

            return years;
        },
    },

    watch: {
        modelValue: {
            immediate: true,
            handler(): void {
                this.internalDay = this.modelValue.day.toString();
                this.internalMonth = this.modelValue.month.toString();
                this.internalYear = this.modelValue.year.toString();
                this.updateDays();
            }
        },

        showYear: {
            immediate: true,
            handler(): void {
                this.updateDays();
            }
        },

        internalDay(): void {
            this.$emit("update:modelValue", this.getValue());
        },

        internalMonth(): void {
            const day = toNumberOrNull(this.internalDay);

            this.updateDays();

            if (day != null && day >= this.days.length + 1) {
                this.internalDay = this.days.length.toString();
            }
            else {
                this.$emit("update:modelValue", this.getValue());
            }
        },

        internalYear(): void {
            const day = toNumberOrNull(this.internalDay);

            this.updateDays();

            if (day != null && day >= this.days.length + 1) {
                this.internalDay = this.days.length.toString();
            }
            else {
                this.$emit("update:modelValue", this.getValue());
            }
        },
    },

    template: `
<RockFormField
    :modelValue="internalDateKey"
    formGroupClasses="birthday-picker"
    name="birthday"
    :rules="computedRules">
    <template #default="{uniqueId, field}">
        <div class="control-wrapper">
            <div class="form-control-group">
                <select :id="uniqueId + '-month'" class="form-control input-width-sm" :disabled="disabled" v-model="internalMonth">
                    <option value="0"></option>
                    <option value="1">Jan</option>
                    <option value="2">Feb</option>
                    <option value="3">Mar</option>
                    <option value="4">Apr</option>
                    <option value="5">May</option>
                    <option value="6">Jun</option>
                    <option value="7">Jul</option>
                    <option value="8">Aug</option>
                    <option value="9">Sep</option>
                    <option value="10">Oct</option>
                    <option value="11">Nov</option>
                    <option value="12">Dec</option>
                </select>
                <span class="separator">/</span>
                <select :id="uniqueId + '-day'" class="form-control input-width-sm" v-model="internalDay">
                    <option value="0"></option>
                    <option v-for="day in days" :key="day" :value="day">{{day}}</option>
                </select>
                <span v-if="showYear" class="separator">/</span>
                <select v-if="showYear" :id="uniqueId + '-year'" class="form-control input-width-sm" v-model="internalYear">
                    <option value="0"></option>
                    <option v-for="year in years" :value="year">{{year}}</option>
                </select>
            </div>
        </div>
    </template>
</RockFormField>`
});
