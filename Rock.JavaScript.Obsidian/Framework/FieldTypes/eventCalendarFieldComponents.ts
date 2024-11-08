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

import { defineComponent, ref, watch } from "vue";
import { getFieldEditorProps } from "./utils";
import EventCalendarPicker from "@Obsidian/Controls/eventCalendarPicker.obs";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";

export const EditComponent = defineComponent({
    name: "EventCalendarField.Edit",

    components: {
        EventCalendarPicker
    },

    props: getFieldEditorProps(),

    setup(props, { emit }) {
        // The internal value used by the text editor.
        const internalValue = ref({} as ListItemBag[]);

        // Watch for changes from the parent component and update the Event Calendar picker.
        watch(() => props.modelValue, () => {
            internalValue.value = JSON.parse(props.modelValue || "{}");
        }, { immediate: true });

        // Watch for changes from the Event Calendar picker and update the parent component.
        watch(() => internalValue.value, () => {
            emit("update:modelValue", internalValue.value ? JSON.stringify(internalValue.value) : "");
        });

        return {
            internalValue
        };
    },

    template: `
    <EventCalendarPicker v-model="internalValue" showBlankItem />
`
});


export const ConfigurationComponent = defineComponent({
    name: "EventCalendarField.Configuration",

    template: ``
});