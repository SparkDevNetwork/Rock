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
import { toNumber } from "../Services/number";
import RockFormField from "./rockFormField";
import TextBox from "./textBox";
import BasicTimePicker from "./basicTimePicker";
import { TimePickerValue } from "./timePicker";
import { padLeft } from "../Services/string";
import { RockDateTime } from "../Util/rockDateTime";

type Rock = {
    controls: {
        datePicker: {
            initialize: (args: Record<string, unknown>) => void;
        };
    };
};

declare global {
    /* eslint-disable @typescript-eslint/naming-convention */
    interface Window {
        Rock: Rock;
    }
    /* eslint-enable @typescript-eslint/naming-convention */
}

export default defineComponent({
    name: "DateTimePicker",

    components: {
        RockFormField,
        BasicTimePicker,
        TextBox
    },

    props: {
        modelValue: {
            type: String as PropType<string | null>,
            default: null
        },
        displayCurrentOption: {
            type: Boolean as PropType<boolean>,
            default: false
        },
        isCurrentDateOffset: {
            type: Boolean as PropType<boolean>,
            default: false
        }
    },

    emits: [
        "update:modelValue"
    ],

    data: function () {
        return {
            internalDateValue: null as string | null,
            internalTimeValue: {} as TimePickerValue,
            isCurrent: false,
            currentDiff: "0",
            validationValue: "",
            skipEmit: false
        };
    },

    computed: {
        asRockDateTimeOrNull(): string | null {
            if (this.internalDateValue) {
                const dateMatch = /^(\d+)\/(\d+)\/(\d+)/.exec(this.internalDateValue ?? "");

                if (dateMatch === null) {
                    return null;
                }

                let date = RockDateTime.fromParts(toNumber(dateMatch[3]), toNumber(dateMatch[1]), toNumber(dateMatch[2]));

                if (date === null) {
                    return null;
                }

                if (this.internalTimeValue.hour !== undefined && this.internalTimeValue.minute !== undefined) {
                    date = date?.addHours(this.internalTimeValue.hour).addMinutes(this.internalTimeValue.minute);
                }

                const year = date.year.toString();
                const month = padLeft(date.month.toString(), 2, "0");
                const day = padLeft(date.day.toString(), 2, "0");
                const hour = padLeft(date.hour.toString(), 2, "0");
                const minute = padLeft(date.minute.toString(), 2, "0");
                const second = padLeft(date.second.toString(), 2, "0");
                const millisecond = padLeft(date.millisecond.toString(), 3, "0");

                // Construct it manually so it doesn't get converted to UTC.
                return `${year}-${month}-${day}T${hour}:${minute}:${second}.${millisecond}`;
            }
            else {
                return null;
            }
        },

        asCurrentDateValue(): string {
            const plusMinus = `${toNumber(this.currentDiff)}`;
            return `CURRENT:${plusMinus}`;
        },

        valueToEmit(): string | string | null {
            if (this.isCurrent) {
                return this.asCurrentDateValue;
            }

            return this.asRockDateTimeOrNull ?? "";
        }
    },

    watch: {
        isCurrentDateOffset: {
            immediate: true,
            handler(): void {
                if (!this.isCurrentDateOffset) {
                    this.currentDiff = "0";
                }
            }
        },

        valueToEmit(): void {
            if (!this.skipEmit) {
                this.$emit("update:modelValue", this.valueToEmit);
            }
        },

        modelValue: {
            immediate: true,
            handler(): void {
                if (!this.modelValue) {
                    this.internalDateValue = null;
                    this.internalTimeValue = {};
                    this.isCurrent = false;
                    this.currentDiff = "0";
                    return;
                }

                if (this.modelValue.indexOf("CURRENT") === 0) {
                    const parts = this.modelValue.split(":");

                    if (parts.length === 2) {
                        this.currentDiff = `${toNumber(parts[1])}`;
                    }

                    this.isCurrent = true;

                    return;
                }

                const date = RockDateTime.parseISO(this.modelValue);

                /*
                 * This is an anti-pattern, but I couldn't find a quick way
                 * around this. Without this, we would set the date and then
                 * emit just the date part before we had a chance to set the
                 * time. There is likely a better way to do this. -dsh.
                 */
                this.skipEmit = true;
                if (date === null) {
                    this.internalDateValue = null;
                    this.internalTimeValue = {};
                }
                else {
                    this.internalDateValue = `${date.month}/${date.day}/${date.year}`;
                    this.internalTimeValue = {
                        hour: date.hour,
                        minute: date.minute
                    };
                }
                this.skipEmit = false;
            }
        },

        displayCurrentOption() {
            if (!this.displayCurrentOption && this.isCurrent) {
                this.internalDateValue = null;
                this.internalTimeValue = {};
                this.isCurrent = false;
                this.currentDiff = "0";
            }
        }
    },

    mounted() {
        const input = this.$refs["input"] as HTMLInputElement;
        const inputId = input.id;

        window.Rock.controls.datePicker.initialize({
            id: inputId,
            startView: 0,
            showOnFocus: true,
            format: "mm/dd/yyyy",
            todayHighlight: true,
            forceParse: true,
            onChangeScript: () => {
                if (!this.isCurrent) {
                    this.internalDateValue = input.value;
                }
            }
        });
    },

    template: `
<RockFormField formGroupClasses="date-picker" #default="{uniqueId}" name="datepicker" v-model.lazy="internalDateValue">
    <div class="control-wrapper">
        <div class="form-control-group">
            <div class="form-row">
                <div class="input-group input-width-md js-date-picker date">
                    <input ref="input" type="text" :id="uniqueId" class="form-control" v-model.lazy="internalDateValue" :disabled="isCurrent" />
                    <span class="input-group-addon">
                        <i class="fa fa-calendar"></i>
                    </span>
                </div>
                <BasicTimePicker v-model="internalTimeValue" :disabled="isCurrent" />
                <div v-if="displayCurrentOption" class="input-group">
                    <div class="checkbox">
                        <label title="">
                        <input type="checkbox" v-model="isCurrent" />
                        <span class="label-text">Current Time</span></label>
                    </div>
                </div>
            </div>
            <div v-if="isCurrent && isCurrentDateOffset" class="form-row">
                <TextBox label="+- Minutes" v-model="currentDiff" inputClasses="input-width-md" help="Enter the number of minutes after the current time to use as the date. Use a negative number to specify minutes before." />
            </div>
        </div>
    </div>
</RockFormField>`
});
