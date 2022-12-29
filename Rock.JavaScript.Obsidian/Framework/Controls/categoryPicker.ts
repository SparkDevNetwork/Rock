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

import { Guid } from "@Obsidian/Types";
import { defineComponent, PropType, ref, watch } from "vue";
import { CategoryTreeItemProvider } from "@Obsidian/Utility/treeItemProviders";
import { updateRefValue } from "@Obsidian/Utility/component";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import TreeItemPicker from "./treeItemPicker.obs";

export default defineComponent({
    name: "CategoryPicker",

    components: {
        TreeItemPicker
    },

    props: {
        modelValue: {
            type: Object as PropType<ListItemBag | ListItemBag[] | null>,
            required: false
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
        },

        securityGrantToken: {
            type: String as PropType<string | null>,
            required: false
        },

        multiple: {
            type: Boolean as PropType<boolean>,
            default: false
        }
    },

    emits: {
        "update:modelValue": (_value: ListItemBag | ListItemBag[] | null) => true
    },

    setup(props, { emit }) {
        const internalValue = ref(props.modelValue ?? null);

        // Configure the item provider with our settings.
        const itemProvider = ref(new CategoryTreeItemProvider());
        itemProvider.value.rootCategoryGuid = props.rootCategoryGuid;
        itemProvider.value.entityTypeGuid = props.entityTypeGuid;
        itemProvider.value.entityTypeQualifierColumn = props.entityTypeQualifierColumn;
        itemProvider.value.entityTypeQualifierValue = props.entityTypeQualifierValue;
        itemProvider.value.securityGrantToken = props.securityGrantToken;

        // Keep security token up to date, but don't need refetch data
        watch(() => props.securityGrantToken, () => {
            itemProvider.value.securityGrantToken = props.securityGrantToken;
        });

        // When this changes, we need to refetch the data, so reset the whole itemProvider
        watch(() => props.entityTypeGuid, () => {
            const oldProvider = itemProvider.value;
            const newProvider = new CategoryTreeItemProvider();

            // copy old provider's properties
            newProvider.rootCategoryGuid = oldProvider.rootCategoryGuid;
            newProvider.entityTypeQualifierColumn = oldProvider.entityTypeQualifierColumn;
            newProvider.entityTypeQualifierValue = oldProvider.entityTypeQualifierValue;
            newProvider.securityGrantToken = oldProvider.securityGrantToken;
            // Use new value
            newProvider.entityTypeGuid = props.entityTypeGuid;

            // Set the provider to the new one
            itemProvider.value = newProvider;
        });

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
    formGroupClasses="category-picker"
    iconCssClass="fa fa-folder-open"
    :provider="itemProvider"
    :multiple="multiple"
/>
`
});
