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
import DayOfWeekPicker from "@Obsidian/Controls/dayOfWeekPicker.obs";

export const EditComponent = defineComponent({
    name: "DaysOfWeekField.Edit",
    components: {
        DayOfWeekPicker
    },
    props: getFieldEditorProps(),

    emits: {
        "update:modelValue": (_value: string) => true
    },

    setup(props, { emit }) {
        const internalValue = computed<string[]>({
            get() {
                const value = props.modelValue ?? "";
                return value !== "" ? value.split(",") : [];
            },
            set(newVal) {
                emit("update:modelValue", newVal.sort().join(","));
            }
        });

        return { internalValue };
    },
    template: `
<DayOfWeekPicker v-model="internalValue" multiple />
`
});

export const ConfigurationComponent = defineComponent({
    name: "DaysOfWeekField.Configuration",

    template: ``
});
