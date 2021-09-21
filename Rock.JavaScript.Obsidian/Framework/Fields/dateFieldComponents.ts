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
import { defineComponent } from "vue";
import { getFieldEditorProps } from "./utils";
import DatePicker from "../Elements/datePicker";
import { asBoolean } from "../Services/boolean";
import { toNumber } from "../Services/number";
import DatePartsPicker, { getDefaultDatePartsPickerModel } from "../Elements/datePartsPicker";
import { ConfigurationValueKey } from "./dateField";
import { RockDateTime } from "../Util/rockDateTime";

export const EditComponent = defineComponent({
    name: "DateField.Edit",

    components: {
        DatePicker,
        DatePartsPicker
    },

    props: getFieldEditorProps(),

    data() {
        return {
            internalValue: "",
            internalDateParts: getDefaultDatePartsPickerModel(),
            formattedString: ""
        };
    },

    setup() {
        return {
        };
    },

    computed: {
        datePartsAsDate(): RockDateTime | null {
            if (!this.internalDateParts?.day || !this.internalDateParts.month || !this.internalDateParts.year) {
                return null;
            }

            return RockDateTime.fromParts(this.internalDateParts.year, this.internalDateParts.month, this.internalDateParts.day) || null;
        },

        isDatePartsPicker(): boolean {
            const config = this.configurationValues[ConfigurationValueKey.DatePickerControlType];
            return config?.toLowerCase() === "date parts picker";
        },

        configAttributes(): Record<string, number | boolean> {
            const attributes: Record<string, number | boolean> = {};

            const displayCurrentConfig = this.configurationValues[ConfigurationValueKey.DisplayCurrentOption];
            const displayCurrent = asBoolean(displayCurrentConfig);
            attributes.displayCurrentOption = displayCurrent;
            attributes.isCurrentDateOffset = displayCurrent;

            const futureYearConfig = this.configurationValues[ConfigurationValueKey.FutureYearCount];
            const futureYears = toNumber(futureYearConfig);

            if (futureYears > 0) {
                attributes.futureYearCount = futureYears;
            }

            return attributes;
        }
    },

    methods: {
        syncModelValue(): void {
            this.internalValue = this.modelValue ?? "";
            const dateParts = /^(\d{4})-(\d{1,2})-(\d{1,2})/.exec(this.modelValue ?? "");

            if (dateParts != null) {
                this.internalDateParts.year = toNumber(dateParts[1]);
                this.internalDateParts.month = toNumber(dateParts[2]);
                this.internalDateParts.day = toNumber(dateParts[3]);
            }
            else {
                this.internalDateParts.year = 0;
                this.internalDateParts.month = 0;
                this.internalDateParts.day = 0;
            }
        }
    },

    watch: {
        datePartsAsDate(): void {
            if (this.isDatePartsPicker) {
                const d1 = this.datePartsAsDate;
                const d2 = RockDateTime.parseISO(this.modelValue ?? "");

                if (d1 === null || d2 === null || !d1.isEqualTo(d2)) {
                    this.$emit("update:modelValue", d1 !== null ? d1.toISOString().split("T")[0] : "");
                }
            }
        },

        internalValue(): void {
            if (!this.isDatePartsPicker) {
                const d1 = RockDateTime.parseISO(this.internalValue);
                const d2 = RockDateTime.parseISO(this.modelValue ?? "");

                if (d1 === null || d2 === null || !d1.isEqualTo(d2)) {
                    this.$emit("update:modelValue", this.internalValue);
                }
            }
        },

        modelValue: {
            immediate: true,
            async handler(): Promise<void> {
                await this.syncModelValue();
            }
        }
    },

    template: `
<DatePartsPicker v-if="isDatePartsPicker" v-model="internalDateParts" v-bind="configAttributes" />
<DatePicker v-else v-model="internalValue" v-bind="configAttributes" />
`
});
