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
import { LocationTreeItemProvider } from "@Obsidian/Utility/treeItemProviders";
import { updateRefValue } from "@Obsidian/Utility/component";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import TreeItemPicker from "./treeItemPicker.obs";

export default defineComponent({
    name: "LocationPicker",

    components: {
        TreeItemPicker
    },

    props: {
        modelValue: {
            type: Object as PropType<ListItemBag | ListItemBag[] | null>,
            required: false
        },

        multiple: {
            type: Boolean as PropType<boolean>,
            default: false
        },

        securityGrantToken: {
            type: String as PropType<string | null>,
            required: false
        }
    },

    emits: {
        "update:modelValue": (_value: ListItemBag | ListItemBag[] | null) => true
    },

    setup(props, { emit }) {
        const internalValue = ref(props.modelValue ?? null);

        // Configure the item provider with our settings. These are not reactive
        // since we don't do lazy loading so there is no point.
        const itemProvider = new LocationTreeItemProvider();
        itemProvider.securityGrantToken = props.securityGrantToken;

        watch(internalValue, () => {
            emit("update:modelValue", internalValue.value);
        });

        watch(() => props.modelValue, () => {
            updateRefValue(internalValue, props.modelValue ?? null);
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
    :multiple="multiple">

    <template #pickerContentSuperHeader v-if="$slots.pickerContentSuperHeader">
        <slot name="pickerContentSuperHeader" />
    </template>
    <template #prepend="{ isInputGroupSupported }" v-if="$slots.prepend">
        <slot name="prepend" :isInputGroupSupported="isInputGroupSupported" />
    </template>
    <template #inputGroupPrepend="{ isInputGroupSupported }" v-if="$slots.inputGroupPrepend">
        <slot name="inputGroupPrepend" :isInputGroupSupported="isInputGroupSupported" />
    </template>
    <template #append="{ isInputGroupSupported }" v-if="$slots.append">
        <slot name="append" :isInputGroupSupported="isInputGroupSupported" />
    </template>
</TreeItemPicker>
`
});
