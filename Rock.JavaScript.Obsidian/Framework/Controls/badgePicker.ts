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
import { standardAsyncPickerProps, useStandardAsyncPickerProps, useVModelPassthrough } from "@Obsidian/Utility/component";
import { post } from "@Obsidian/Utility/http";
import { BadgePickerGetBadgesOptionsBag } from "@Obsidian/ViewModels/Rest/Controls/badgePickerGetBadgesOptionsBag";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { computed, defineComponent, PropType, ref, watch } from "vue";
import BaseAsyncPicker from "./baseAsyncPicker";
import { useSecurityGrantToken } from "@Obsidian/Utility/block";

export default defineComponent({
    name: "BadgePicker",

    components: {
        BaseAsyncPicker
    },

    props: {
        modelValue: {
            type: Object as PropType<ListItemBag | ListItemBag[] | null>,
            required: false
        },

        entityTypeGuid: {
            type: String as PropType<Guid>,
            default: null
        },

        ...standardAsyncPickerProps
    },

    emits: {
        "update:modelValue": (_value: ListItemBag | ListItemBag[] | null) => true
    },

    setup(props, { emit }) {
        // #region Values

        const internalValue = useVModelPassthrough(props, "modelValue", emit);
        const standardProps = useStandardAsyncPickerProps(props);
        const loadedItems = ref<ListItemBag[] | null>(null);
        const securityGrantToken = useSecurityGrantToken();

        // #endregion

        // #region Computed Values

        /**
         * The actual items to make available to the picker. This allows us to do any
         * post-processing, such as adding additional items, and still be lazy loaded as well.
         */
        const actualItems = computed((): ListItemBag[] | (() => Promise<ListItemBag[]>) => {
            return loadedItems.value || loadOptions;
        });

        // #endregion

        // #region Functions

        /**
         * Loads the items from the server.
         */
        const loadOptions = async (): Promise<ListItemBag[]> => {
            const options: Partial<BadgePickerGetBadgesOptionsBag> = {
                securityGrantToken: securityGrantToken.value
            };
            const result = await post<ListItemBag[]>("/api/v2/Controls/BadgePickerGetBadges", undefined, options);

            if (result.isSuccess && result.data) {
                loadedItems.value = result.data;
                return result.data;
            }
            else {
                console.error(result.errorMessage ?? "Unknown error while loading data.");
                loadedItems.value = [];
                return [];
            }
        };

        // #endregion

        // #region Watchers

        watch(() => props.entityTypeGuid, () => {
            loadedItems.value = null;
        });

        // #endregion

        return {
            actualItems,
            internalValue,
            standardProps
        };
    },

    template: `
<BaseAsyncPicker v-model="internalValue"
    v-bind="standardProps"
    :items="actualItems" />
`
});
