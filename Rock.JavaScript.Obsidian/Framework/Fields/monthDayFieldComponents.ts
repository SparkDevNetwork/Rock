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
import { toNumber } from "../Services/number";
import DatePartsPicker, { DatePartsPickerValue } from "../Elements/datePartsPicker";

export const EditComponent = defineComponent({
    name: "MonthDayField.Edit",

    components: {
        DatePartsPicker
    },

    props: getFieldEditorProps(),

    data() {
        return {
            /** The user input value. */
            internalValue: {
                year: 0,
                month: 0,
                day: 0
            } as DatePartsPickerValue
        };
    },

    watch: {
        /**
         * Watch for changes to internalValue and emit the new value out to
         * the consuming component.
         */
        internalValue(): void {
            const value = this.internalValue.month !== 0 && this.internalValue.day !== 0
                ? `${this.internalValue.month}/${this.internalValue.day}`
                : "";

            this.$emit("update:modelValue", value);
        },

        /**
         * Watch for changes to modelValue which indicate the component
         * using us has given us a new value to work with.
         */
        modelValue: {
            immediate: true,
            handler(): void {
                const components = (this.modelValue || "").split("/");

                if (components.length == 2) {
                    this.internalValue = {
                        year: 0,
                        month: toNumber(components[0]),
                        day: toNumber(components[1])
                    };
                }
                else {
                    this.internalValue = {
                        year: 0,
                        month: 0,
                        day: 0
                    };
                }
            }
        }
    },

    template: `
<DatePartsPicker v-model="internalValue" :showYear="false" />
`
});

export const ConfigurationComponent = defineComponent({
    name: "MonthDayField.Configuration",

    template: ``
});
