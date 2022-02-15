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
import { DayOfWeek } from "./dayOfWeekField";
import CheckBoxList from "../Elements/checkBoxList";
import { toNumber } from "../Services/number";
import { ListItem } from "../ViewModels";

export const EditComponent = defineComponent({
    name: "DaysOfWeekField.Edit",
    components: {
        CheckBoxList
    },
    props: getFieldEditorProps(),

    data() {
        return {
            /** The currently selected values. */
            internalValue: [] as Array<string>,
        };
    },

    methods: {
        /**
         * Builds a list of the drop down options that are used to display
         * in the drop down list.
         */
        options(): Array<ListItem> {
            return [
                { text: "Sunday", value: DayOfWeek.Sunday.toString() },
                { text: "Monday", value: DayOfWeek.Monday.toString() },
                { text: "Tuesday", value: DayOfWeek.Tuesday.toString() },
                { text: "Wednesday", value: DayOfWeek.Wednesday.toString() },
                { text: "Thursday", value: DayOfWeek.Thursday.toString() },
                { text: "Friday", value: DayOfWeek.Friday.toString() },
                { text: "Saturday", value: DayOfWeek.Saturday.toString() }
            ];
        },
    },

    watch: {
        /**
         * Watch for changes to internalValue and emit the new value out to
         * the consuming component.
         */
        internalValue(): void {
            this.$emit("update:modelValue", this.internalValue.sort((a, b) => toNumber(a) - toNumber(b)).join(","));
        },

        /**
         * Watch for changes to modelValue which indicate the component
         * using us has given us a new value to work with.
         */
        modelValue: {
            immediate: true,
            handler(): void {
                const value = this.modelValue ?? "";

                this.internalValue = value !== "" ? value.split(",") : [];
            }
        }
    },
    template: `
<CheckBoxList v-model="internalValue" :options="options()" />
`
});

export const ConfigurationComponent = defineComponent({
    name: "DaysOfWeekField.Configuration",

    template: ``
});
