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
import { toNumberOrNull } from "@Obsidian/Utility/numberUtils";
import NumberRangeBox from "@Obsidian/Controls/numberRangeBox.obs";
import { NumberRangeModelValue } from "@Obsidian/Types/Controls/numberRangeBox";

export const EditComponent = defineComponent({
    name: "IntegerRangeField.Edit",

    components: {
        NumberRangeBox
    },

    props: getFieldEditorProps(),

    data() {
        return {
            /** The user input value as a number of null if it isn't valid. */
            internalValue: undefined as NumberRangeModelValue | undefined
        };
    },

    watch: {
        /**
         * Watch for changes to internalValue and emit the new value out to
         * the consuming component.
         */
        internalValue(): void {
            const value = `${this.internalValue?.lower ?? ""},${this.internalValue?.upper ?? ""}`;

            this.$emit("update:modelValue", value !== "," ? value : "");
        },

        /**
         * Watch for changes to modelValue which indicate the component
         * using us has given us a new value to work with.
         */
        modelValue: {
            immediate: true,
            handler(): void {
                const values = (this.modelValue ?? "").split(",");
                const lower = toNumberOrNull(values[0]);
                const upper = values.length >= 2 ? toNumberOrNull(values[1]) : null;

                if (lower === null && upper === null) {
                    this.internalValue = undefined;
                }
                else if (lower !== (this.internalValue?.lower ?? null) || upper !== (this.internalValue?.upper ?? null)) {
                    this.internalValue = {
                        lower: lower,
                        upper: upper
                    };
                }
            }
        }
    },

    template: `
<NumberRangeBox v-model="internalValue" :decimal-count="0" />
`
});

export const ConfigurationComponent = defineComponent({
    name: "IntegerRangeField.Configuration",

    template: ``
});
