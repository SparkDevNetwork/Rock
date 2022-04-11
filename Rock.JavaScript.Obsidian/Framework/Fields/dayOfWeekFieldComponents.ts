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
import DropDownList from "../Elements/dropDownList";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { useVModelPassthrough } from "../Util/component";

export const EditComponent = defineComponent({
    name: "DayOfWeekField.Edit",
    components: {
        DropDownList
    },
    props: getFieldEditorProps(),

    setup(props, { emit }) {
        const internalValue = useVModelPassthrough(props, "modelValue", emit);

        const options: ListItemBag[] = [
            { text: "Sunday", value: DayOfWeek.Sunday.toString() },
            { text: "Monday", value: DayOfWeek.Monday.toString() },
            { text: "Tuesday", value: DayOfWeek.Tuesday.toString() },
            { text: "Wednesday", value: DayOfWeek.Wednesday.toString() },
            { text: "Thursday", value: DayOfWeek.Thursday.toString() },
            { text: "Friday", value: DayOfWeek.Friday.toString() },
            { text: "Saturday", value: DayOfWeek.Saturday.toString() }
        ];

        return {
            internalValue,
            options
        };
    },
    template: `
<DropDownList v-model="internalValue" :options="options" />
`
});

export const FilterComponent = defineComponent({
    name: "DayOfWeekField.Filter",
    components: {
        DropDownList
    },
    props: getFieldEditorProps(),

    setup(props, { emit }) {
        const internalValue = useVModelPassthrough(props, "modelValue", emit);

        const options: ListItemBag[] = [
            { text: "Sunday", value: DayOfWeek.Sunday.toString() },
            { text: "Monday", value: DayOfWeek.Monday.toString() },
            { text: "Tuesday", value: DayOfWeek.Tuesday.toString() },
            { text: "Wednesday", value: DayOfWeek.Wednesday.toString() },
            { text: "Thursday", value: DayOfWeek.Thursday.toString() },
            { text: "Friday", value: DayOfWeek.Friday.toString() },
            { text: "Saturday", value: DayOfWeek.Saturday.toString() }
        ];

        return {
            internalValue,
            options
        };
    },
    template: `
<DropDownList v-model="internalValue" :options="options" />
`
});

export const ConfigurationComponent = defineComponent({
    name: "DayOfWeekField.Configuration",

    template: ``
});

