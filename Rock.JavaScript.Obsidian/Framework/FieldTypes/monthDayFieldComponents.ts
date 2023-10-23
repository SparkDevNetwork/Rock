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
import { defineComponent, computed } from "vue";
import { getFieldEditorProps } from "./utils";
import { toNumber } from "@Obsidian/Utility/numberUtils";
import { MonthDayValue } from "@Obsidian/ViewModels/Controls/monthDayValue";
import MonthDayPicker from "@Obsidian/Controls/monthDayPicker.obs";

export const EditComponent = defineComponent({
    name: "MonthDayField.Edit",

    components: {
        MonthDayPicker
    },

    props: getFieldEditorProps(),

    emit: {
        "update:modelValue": (_value: string) => true
    },

    setup(props, { emit }) {
        const internalValue = computed<MonthDayValue | undefined>({
            get() {
                const components = (props.modelValue || "").split("/");

                if (components.length == 2) {
                    return {
                        month: toNumber(components[0]),
                        day: toNumber(components[1])
                    };
                }
                else {
                    return undefined;
                }
            },
            set(newVal: MonthDayValue | undefined) {
                const value = newVal && newVal.month !== 0 && newVal.day !== 0
                    ? `${newVal.month}/${newVal.day}`
                    : "";

                emit("update:modelValue", value);
            }
        });

        return { internalValue };
    },

    template: `
<MonthDayPicker v-model="internalValue" :showYear="false" />
`
});

export const ConfigurationComponent = defineComponent({
    name: "MonthDayField.Configuration",

    template: ``
});