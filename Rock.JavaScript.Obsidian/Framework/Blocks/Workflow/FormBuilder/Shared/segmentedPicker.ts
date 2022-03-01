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

import { defineComponent, PropType } from "vue";
import { useVModelPassthrough } from "../../../../Util/component";
import { ListItem } from "../../../../ViewModels";

export default defineComponent({
    name: "SegmentedPicker",

    props: {
        modelValue: {
            type: String as PropType<string>,
            default: ""
        },

        options: {
            type: Array as PropType<ListItem[]>,
            default: []
        }
    },

    emits: [
        "update:modelValue"
    ],

    setup(props, { emit }) {
        const internalValue = useVModelPassthrough(props, "modelValue", emit);

        /**
         * Gets the classes to apply to the button.
         * 
         * @param item The ListItem that represents the button.
         *
         * @returns A collection of CSS class names.
         */
        const getButtonClass = (item: ListItem): string[] => {
            return ["btn", item.value === internalValue.value ? "btn-primary" : "btn-default"];
        };

        /**
         * Event handler for then a button item is clicked.
         * 
         * @param item The ListItem that represents the button that was clicked.
         */
        const onItemClick = (item: ListItem): void => {
            internalValue.value = item.value;
        };

        return {
            getButtonClass,
            internalValue,
            onItemClick
        };
    },

    template: `
<div class="btn-group btn-group-xs mb-2" role="group">
    <button v-for="item in options" :class="getButtonClass(item)" :key="item.value" type="button" @click="onItemClick(item)">{{ item.text }}</button>
</div>
`
});
