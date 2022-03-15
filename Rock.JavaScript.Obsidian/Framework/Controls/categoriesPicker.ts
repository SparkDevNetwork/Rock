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
import { Guid } from "../Util/guid";
import { CategoryTreeItemProvider } from "../Util/treeItemProviders";
import { ListItem } from "../ViewModels";
import TreeItemPicker from "./treeItemPicker";

export default defineComponent({
    name: "CategoryPicker",

    components: {
        TreeItemPicker
    },

    props: {
        modelValue: {
            type: Array as PropType<ListItem[]>,
            default: [],
        },

        rootCategoryGuid: {
            type: String as PropType<Guid>
        },

        entityTypeGuid: {
            type: String as PropType<Guid>
        },

        entityTypeQualifierColumn: {
            type: String as PropType<string>
        },

        entityTypeQualifierValue: {
            type: String as PropType<string>
        }
    },

    emits: [
        "update:modelValue"
    ],

    setup(props, { emit }) {
        const internalValue = ref(props.modelValue);

        // Configure the item provider with our settings. These are not reactive
        // since we don't do lazy loading so there is no point.
        const itemProvider = new CategoryTreeItemProvider();
        itemProvider.rootCategoryGuid = props.rootCategoryGuid;
        itemProvider.entityTypeGuid = props.entityTypeGuid;
        itemProvider.entityTypeQualifierColumn = props.entityTypeQualifierColumn;
        itemProvider.entityTypeQualifierValue = props.entityTypeQualifierValue;

        watch(internalValue, () => {
            emit("update:modelValue", internalValue.value);
        });

        watch(() => props.modelValue, () => internalValue.value = props.modelValue);

        return {
            internalValue,
            itemProvider
        };
    },

    template: `
<TreeItemPicker v-model="internalValue"
    formGroupClasses="category-picker"
    iconCssClass="fa fa-folder-open"
    :provider="itemProvider"
    allowMultiple
/>
`
});
