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
import { useSecurityGrantToken } from "@Obsidian/Utility/block";
import { GroupTreeItemProvider } from "@Obsidian/Utility/treeItemProviders";
import { updateRefValue } from "@Obsidian/Utility/component";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import TreeItemPicker from "./treeItemPicker";
import { Guid } from "@Obsidian/Types";
import InlineCheckBox from "./inlineCheckBox";

export default defineComponent({
    name: "GroupPicker",

    components: {
        TreeItemPicker,
        InlineCheckBox
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

        /** GUID of the group you want to use as the root. */
        rootGroupGuid: {
            type: Object as PropType<Guid | null>,
            default: null
        },

        /** List of group types GUIDs to limit to groups of those types. */
        includedGroupTypeGuids: {
            type: Object as PropType<Guid[]>,
            default: []
        },

        /** Whether to limit to only groups that have scheduling enabled. */
        limitToSchedulingEnabled: {
            type: Object as PropType<boolean>,
            default: false
        },

        /** Whether to limit to only groups that have RSVPs enabled. */
        limitToRSVPEnabled: {
            type: Object as PropType<boolean>,
            default: false
        },
    },

    emits: {
        "update:modelValue": (_value: ListItemBag | ListItemBag[] | null) => true
    },

    setup(props, { emit }) {
        const internalValue = ref(props.modelValue ?? null);
        const securityGrantToken = useSecurityGrantToken();

        /** Whether to include inactive groups in the results or not. */
        const includeInactiveGroups = ref(false);

        const itemProvider = ref<GroupTreeItemProvider>();

        // Configure the item provider with our settings.
        function refreshItemProvider(): void {
            const provider = new GroupTreeItemProvider();
            provider.rootGroupGuid = props.rootGroupGuid;
            provider.includedGroupTypeGuids = props.includedGroupTypeGuids;
            provider.includeInactiveGroups = includeInactiveGroups.value;
            provider.limitToSchedulingEnabled = props.limitToSchedulingEnabled;
            provider.limitToRSVPEnabled = props.limitToRSVPEnabled;
            provider.securityGrantToken = securityGrantToken.value;

            itemProvider.value = provider;
        }

        refreshItemProvider();

        watch(() => [
            props.rootGroupGuid,
            includeInactiveGroups.value,
            props.includedGroupTypeGuids,
            props.limitToRSVPEnabled,
            props.limitToSchedulingEnabled
        ], refreshItemProvider);

        // Keep security token up to date, but don't need refetch data
        watch(securityGrantToken, () => {
            if (itemProvider.value) {
                itemProvider.value.securityGrantToken = securityGrantToken.value;
            }
        });

        watch(internalValue, () => {
            emit("update:modelValue", internalValue.value);
        });

        watch(() => props.modelValue, () => {
            updateRefValue(internalValue, props.modelValue ?? null);
        });

        return {
            internalValue,
            includeInactiveGroups,
            itemProvider
        };
    },

    template: `
<TreeItemPicker v-model="internalValue"
    formGroupClasses="group-item-picker"
    iconCssClass="fa fa-home"
    :provider="itemProvider"
    :multiple="multiple">

    <template #customPickerActions>
        <label class="rock-checkbox-icon">
            <i :class="['fa', includeInactiveGroups ? 'fa-check-square-o' : 'fa-square-o']"></i> Show Inactive
            <span style="display:none"><input type="checkbox" v-model="includeInactiveGroups"></span>
        </label>
    </template>
</TreeItemPicker>
`
});
