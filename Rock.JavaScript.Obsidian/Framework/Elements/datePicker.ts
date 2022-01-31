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
import { newGuid } from "../Util/guid";
import TextBox from "./textBox";

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

export const DatePickerBase = defineComponent({
    name: "DatePickerBase",

    props: {
        modelValue: {
            type: String as PropType<string | null>,
            default: null
        },

        id: {
            type: String as PropType<string>,
            default: ""
        },

        disabled: {
            type: Boolean as PropType<boolean>,
            default: false
        }
    },

    emits: [
        "update:modelValue"
    ],

    data: function () {
        return {
            internalValue: null as string | null,
            defaultId: `datepicker-${newGuid()}`
        };
    },

    computed: {
        computedId(): string {
            return this.id || this.defaultId;
        },

        asRockDateOrNull(): string | null {
            const match = /^(\d+)\/(\d+)\/(\d+)/.exec(this.internalValue ?? "");

            if (match !== null) {
                return `${match[3]}-${match[1]}-${match[2]}`;
            }
            else {
                return null;
            }
        }
    },

    watch: {
        asRockDateOrNull(): void {
            this.$emit("update:modelValue", this.asRockDateOrNull);
        },

        modelValue: {
            immediate: true,
            handler(): void {
                if (!this.modelValue) {
                    this.internalValue = null;

                    return;
                }

                const match = /^(\d+)-(\d+)-(\d+)/.exec(this.modelValue);

                if (match !== null) {
                    this.internalValue = `${match[2]}/${match[3]}/${match[1]}`;
                }
                else {
                    this.internalValue = null;
                }
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
                this.internalValue = input.value;
            }
        });
    },

    template: `
<div class="input-group input-width-md js-date-picker date">
    <input ref="input" type="text" :id="computedId" class="form-control" v-model.lazy="internalValue" :disabled="isCurrent" />
    <span class="input-group-addon">
        <i class="fa fa-calendar"></i>
    </span>
</div>
`
});

export default defineComponent({
    name: "DatePicker",

    components: {
        RockFormField,
        DatePickerBase,
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
            internalValue: null as string | null,
            isCurrent: false,
            currentDiff: "0"
        };
    },

    computed: {
        asCurrentDateValue (): string {
            const plusMinus = `${toNumber(this.currentDiff)}`;
            return `CURRENT:${plusMinus}`;
        },

        valueToEmit (): string | string | null {
            if (this.isCurrent) {
                return this.asCurrentDateValue;
            }

            return this.internalValue;
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

        isCurrent: {
            immediate: true,
            handler(): void {
                if (this.isCurrent) {
                    this.internalValue = "Current";
                }
                else {
                    this.internalValue = null;
                }
            }
        },

        valueToEmit(): void {
            this.$emit("update:modelValue", this.valueToEmit);
        },

        modelValue: {
            immediate: true,
            handler(): void {
                if (!this.modelValue) {
                    this.internalValue = null;
                    this.isCurrent = false;
                    this.currentDiff = "0";

                    return;
                }

                if (this.modelValue.indexOf("CURRENT") === 0) {
                    this.isCurrent = true;
                    const parts = this.modelValue.split(":");

                    if (parts.length === 2) {
                        this.currentDiff = `${toNumber(parts[1])}`;
                    }

                    return;
                }

                this.internalValue = this.modelValue;
            }
        },

        displayCurrentOption() {
            // clear out the "current" data this option is disabled so we can actually set a new value
            if (!this.displayCurrentOption && this.isCurrent) {
                this.internalValue = null;
                this.isCurrent = false;
                this.currentDiff = "0";
            }
        }
    },

    template: `
<RockFormField formGroupClasses="date-picker" #default="{uniqueId}" name="datepicker" v-model.lazy="internalValue">
    <div class="control-wrapper">
        <div v-if="displayCurrentOption" class="form-control-group">
            <div class="form-row">
                <DatePickerBase v-model.lazy="internalValue" :id="uniqueId" :disabled="isCurrent" />
                <div v-if="displayCurrentOption || isCurrent" class="input-group">
                    <div class="checkbox">
                        <label title="">
                        <input type="checkbox" v-model="isCurrent" />
                        <span class="label-text">Current Date</span></label>
                    </div>
                </div>
            </div>
            <div v-if="isCurrent && isCurrentDateOffset" class="form-row">
                <TextBox label="+- Days" v-model="currentDiff" inputClasses="input-width-md" help="Enter the number of days after the current date to use as the date. Use a negative number to specify days before." />
            </div>
        </div>
        <DatePickerBase v-else v-model.lazy="internalValue" :id="uniqueId" :disabled="isCurrent" />
    </div>
</RockFormField>`
});
