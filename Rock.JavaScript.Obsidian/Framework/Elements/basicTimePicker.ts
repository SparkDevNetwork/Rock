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

import { watch } from "vue";
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

    emits: [ "update:modelValue" ],

    setup(props, { emit }) {
        const internalHour: Ref<number | null> = ref(null);
        const internalMinute: Ref<number | null> = ref(null);
        const internalMeridiem: Ref<"AM" | "PM"> = ref("AM");
        const internalValue: Ref<string> = ref("");

        function keyPress(e: KeyboardEvent): boolean {
            if (e.key === "a" || e.key === "p" || e.key === "A" || e.key == "P") {
                internalMeridiem.value = e.key === "a" || e.key === "A" ? "AM" : "PM";
                maybeUpdateValue();

                e.preventDefault();
                return false;
            }

            if (/^[0-9:]$/.test(e.key) === false) {
                e.preventDefault();
                return false;
            }

            return true;
        }

        function updateValue(): void {
            const values = /(\d+):(\d+)/.exec(internalValue.value);
            const value: BasicTimePickerValue = {};

            if(values !== null) {
                value.hour = toNumber(values[1]) + (internalMeridiem.value === "PM" ? 12 : 0);
                value.minute = toNumber(values[2]);
            }

            emit("update:modelValue", value);
        }

        function maybeUpdateValue(): void {
            const values = /(\d+):(\d+)/.exec(internalValue.value);

            if(values !== null) {
                updateValue();
            }
        }

        function toggleMeridiem(e: Event): boolean {
            e.preventDefault();

            internalMeridiem.value = internalMeridiem.value === "AM" ? "PM" : "AM";
            maybeUpdateValue();

            return false;
        }

        watch(() => props.modelValue, (): void => {
            ///^(\d{1,2})(?:\:(\d{2})(?: ?([aApP])[mM]?)?)?$/
            if (props.modelValue.hour) {
                if (props.modelValue.hour > 12) {
                    internalHour.value = props.modelValue.hour - 12;
                }
                else {
                    internalHour.value = props.modelValue.hour;
                }

                if (props.modelValue.hour >= 12) {
                    internalMeridiem.value = "PM";
                }
            }
            else {
                internalHour.value = null;
            }

            if (props.modelValue.minute) {
                internalMinute.value = props.modelValue.minute;
            }
            else if (internalHour.value != null) {
                internalMinute.value = 0;
            }
            else {
                internalMinute.value = null;
            }

            if (internalHour.value === null || internalMinute.value === null) {
                return;
            }

            internalValue.value = `${internalHour.value}:${padLeft(internalMinute.value.toString(), 2, "0")}`;
        }, { immediate: true });

        return {
            internalHour,
            internalMinute,
            internalMeridiem,
            internalValue,
            keyPress,
            updateValue,
            maybeUpdateValue,
            toggleMeridiem
        };
    },

    template: `
<div class="input-group input-width-md">
    <input class="form-control" type="text" v-model="internalValue" v-on:change="updateValue" v-on:keypress="keyPress" :disabled="disabled" />
    <span class="input-group-btn"><button class="btn btn-default" v-on:click="toggleMeridiem" :disabled="disabled">{{ internalMeridiem }}</button></span>
</div>
`
});
