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
import DateRangePicker from "@Obsidian/Controls/dateRangePicker.obs";
import { DateRangeParts } from "@Obsidian/Types/Controls/dateRangePicker";

export const EditComponent = defineComponent({
    name: "DateRangeField.Edit",

    components: {
        DateRangePicker
    },

    props: getFieldEditorProps(),

    data() {
        return {
            internalValue: undefined as DateRangeParts | undefined
        };
    },

    setup() {
        return {
        };
    },

    watch: {
        internalValue(): void {
            if (!this.internalValue?.lowerValue && !this.internalValue?.upperValue) {
                this.$emit("update:modelValue", "");
            }
            else {
                this.$emit("update:modelValue", `${this.internalValue.lowerValue ?? ""},${this.internalValue.upperValue ?? ""}`);
            }
        },

        modelValue: {
            immediate: true,
            handler(): void {
                const components = (this.modelValue ?? "").split(",");

                if (components.length === 2) {
                    this.internalValue = {
                        lowerValue: components[0],
                        upperValue: components[1]
                    };
                }
                else {
                    this.internalValue = {};
                }
            }
        }
    },

    template: `
<DateRangePicker v-model="internalValue" />
`
});

export const ConfigurationComponent = defineComponent({
    name: "DateRangeField.Configuration",

    template: ``
});
