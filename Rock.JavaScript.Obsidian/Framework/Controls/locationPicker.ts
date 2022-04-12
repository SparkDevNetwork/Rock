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

import { defineComponent, PropType, ref, watch } from "vue";
import { LocationTreeItemProvider } from "../Util/treeItemProviders";
import { updateRefValue } from "../Util/util";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import TreeItemPicker from "./treeItemPicker";

export default defineComponent({
    name: "LocationPicker",

    components: {
        TreeItemPicker
    },

    props: {
        modelValue: {
            type: Object as PropType<ListItemBag | null>
        }
    },

    setup(props, { emit }) {
        const internalValue = ref(props.modelValue ? [props.modelValue] : []);

        // Configure the item provider with our settings. These are not reactive
        // since we don't do lazy loading so there is no point.
        const itemProvider = new LocationTreeItemProvider();

        watch(internalValue, () => {
            emit("update:modelValue", internalValue.value.length > 0 ? internalValue.value[0] : undefined);
        });

        watch(() => props.modelValue, () => {
            updateRefValue(internalValue, props.modelValue ? [props.modelValue] : []);
        });

        return {
            internalValue,
            itemProvider
        };
    },

    template: `
<TreeItemPicker v-model="internalValue"
    formGroupClasses="location-item-picker"
    iconCssClass="fa fa-home"
    :provider="itemProvider"
/>
`
});
