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
import DropDownList from "@Obsidian/Controls/dropDownList.obs";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { ConfigurationValueKey, ReminderTypeFieldItem } from "./reminderTypeField.partial";
import { useVModelPassthrough } from "@Obsidian/Utility/component";

export const EditComponent = defineComponent({
    name: "ReminderTypeField.Edit",

    components: {
        DropDownList
    },

    props: getFieldEditorProps(),

    emits: {
        "update:modelValue": (_v: string) => true
    },

    setup(props, { emit }) {
        const internalValue = useVModelPassthrough(props, "modelValue", emit);

        /** The options to choose from in the drop down list */
        const options = computed((): ListItemBag[] => {
            try {
                const items = JSON.parse(props.configurationValues[ConfigurationValueKey.Values] ?? "[]") as ReminderTypeFieldItem[];
                return items.map(rt => ({ value: rt.guid, text: `${rt.name} (${rt.entityTypeName})` }));
            }
            catch {
                return [];
            }
        });

        return {
            internalValue,
            options
        };
    },

    template: `<DropDownList v-model="internalValue" :items="options" />`
});

export const ConfigurationComponent = defineComponent({
    name: "ReminderTypeField.Configuration",

    template: ``
});
