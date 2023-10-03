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
import TimePicker from "@Obsidian/Controls/timePicker.obs";
import { TimePickerValue } from "@Obsidian/ViewModels/Controls/timePickerValue.d";
import { toNumber } from "@Obsidian/Utility/numberUtils";
import { padLeft } from "@Obsidian/Utility/stringUtils";

export const EditComponent = defineComponent({
    name: "TimeField.Edit",

    components: {
        TimePicker
    },

    props: getFieldEditorProps(),

    data() {
        return {
            internalTimeValue: {} as TimePickerValue,
            internalValue: ""
        };
    },

    computed: {
        displayValue(): string {
            if (this.internalTimeValue.hour === undefined || this.internalTimeValue.minute === undefined) {
                return "";
            }

            let hour = this.internalTimeValue.hour;
            const minute = this.internalTimeValue.minute;
            const meridiem = hour >= 12 ? "PM" : "AM";

            if (hour > 12) {
                hour -= 12;
            }

            return `${hour}:${padLeft(minute.toString(), 2, "0")} ${meridiem}`;
        },
    },

    watch: {
        internalValue(): void {
            this.$emit("update:modelValue", this.internalValue);
        },

        internalTimeValue(): void {
            if (this.internalTimeValue.hour === undefined || this.internalTimeValue.minute === undefined) {
                this.internalValue = "";
            }
            else {
                this.internalValue = `${this.internalTimeValue.hour}:${padLeft(this.internalTimeValue.minute.toString(), 2, "0")}:00`;
            }
        },

        modelValue: {
            immediate: true,
            handler(): void {
                const values = /^(\d+):(\d+)/.exec(this.modelValue ?? "");

                if (values !== null) {
                    this.internalTimeValue = {
                        hour: toNumber(values[1]),
                        minute: toNumber(values[2])
                    };
                }
                else {
                    this.internalTimeValue = {};
                }
            }
        }
    },

    template: `
<TimePicker v-model="internalTimeValue" />
`
});

export const ConfigurationComponent = defineComponent({
    name: "TimeField.Configuration",

    template: ``
});
