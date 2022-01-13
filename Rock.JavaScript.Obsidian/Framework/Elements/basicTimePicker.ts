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

import { defineComponent, PropType, ref, Ref } from "vue";
import { toNumber } from "../Services/number";
import { padLeft } from "../Services/string";

/** The value expected by the TimePicker. */
export type BasicTimePickerValue = {
    /** Hour of the time, 0-23. */
    hour?: number;

    /** Minute of the time, 0-59. */
    minute?: number;
};

export default defineComponent({
    name: "BasicTimePicker",

    components: {
    },

    props: {
        modelValue: {
            type: Object as PropType<BasicTimePickerValue>,
            default: {}
        },
        disabled: {
            type: Boolean as PropType<boolean>,
            default: false
        }
    },

    setup() {
        const internalHour: Ref<number | null> = ref(null);
        const internalMinute: Ref<number | null> = ref(null);
        const internalMeridiem: Ref<"AM" | "PM"> = ref("AM");
        const internalValue: Ref<string> = ref("");

        return {
            internalHour,
            internalMinute,
            internalMeridiem,
            internalValue
        };
    },

    //data() {
    //    return {
    //        internalHour: null as number | null,
    //        internalMinute: null as number | null,
    //        internalMeridiem: "AM" as ("AM" | "PM"),
    //        internalValue: ""
    //    };
    //},

    methods: {
        keyPress(e: KeyboardEvent): boolean {
            if (e.key === "a" || e.key === "p" || e.key === "A" || e.key == "P") {
                this.internalMeridiem = e.key === "a" || e.key === "A" ? "AM" : "PM";
                this.maybeUpdateValue();

                e.preventDefault();
                return false;
            }

            if (/^[0-9:]$/.test(e.key) === false) {
                e.preventDefault();
                return false;
            }

            return true;
        },

        keyUp(e: KeyboardEvent): boolean {
            const area = this.$refs.area as HTMLInputElement;
            const group = this.$refs.group as HTMLInputElement;
            const serial = this.$refs.serial as HTMLInputElement;

            // Only move to next field if a number was pressed.
            if (/^[0-9]$/.test(e.key) === false) {
                return true;
            }

            if (area === e.target && area.selectionStart === 3) {
                this.$nextTick(() => {
                    group.focus();
                    group.setSelectionRange(0, 2);
                });
            }
            else if (group === e.target && group.selectionStart === 2) {
                this.$nextTick(() => {
                    serial.focus();
                    serial.setSelectionRange(0, 4);
                });
            }

            return true;
        },

        updateValue(): void {
            const values = /(\d+):(\d+)/.exec(this.internalValue);
            const value: BasicTimePickerValue = {};

            if (values !== null) {
                value.hour = toNumber(values[1]) + (this.internalMeridiem === "PM" ? 12 : 0);
                value.minute = toNumber(values[2]);
            }

            this.$emit("update:modelValue", value);
        },

        maybeUpdateValue(): void {
            const values = /(\d+):(\d+)/.exec(this.internalValue);

            if (values !== null) {
                this.updateValue();
            }
        },

        toggleMeridiem(e: Event): boolean {
            e.preventDefault();

            this.internalMeridiem = this.internalMeridiem === "AM" ? "PM" : "AM";
            this.maybeUpdateValue();

            return false;
        }
    },

    computed: {
    },

    watch: {
        modelValue: {
            immediate: true,
            handler(): void {
                ///^(\d{1,2})(?:\:(\d{2})(?: ?([aApP])[mM]?)?)?$/

                if (this.modelValue.hour) {
                    if (this.modelValue.hour > 12) {
                        this.internalHour = this.modelValue.hour - 12;
                    }
                    else {
                        this.internalHour = this.modelValue.hour;
                    }

                    if (this.modelValue.hour >= 12) {
                        this.internalMeridiem = "PM";
                    }
                }
                else {
                    this.internalHour = null;
                }

                if (this.modelValue.minute) {
                    this.internalMinute = this.modelValue.minute;
                }
                else if (this.internalHour != null) {
                    this.internalMinute = 0;
                }
                else {
                    this.internalMinute = null;
                }

                if (this.internalHour === null || this.internalMinute === null) {
                    return;
                }

                this.internalValue = `${this.internalHour}:${padLeft(this.internalMinute.toString(), 2, "0")}`;
            }
        }
    },

    template: `
<div class="input-group input-width-md">
    <input class="form-control" type="text" v-model="internalValue" v-on:change="updateValue" v-on:keypress="keyPress" :disabled="disabled" />
    <span class="input-group-btn"><button class="btn btn-default" v-on:click="toggleMeridiem" :disabled="disabled">{{ internalMeridiem }}</button></span>
</div>
`
});
