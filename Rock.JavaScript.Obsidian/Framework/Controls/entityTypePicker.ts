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
import { standardAsyncPickerProps, useStandardAsyncPickerProps, useVModelPassthrough } from "@Obsidian/Utility/component";
import { emptyGuid } from "@Obsidian/Utility/guid";
import { useHttp } from "@Obsidian/Utility/http";
import { EntityTypePickerGetEntityTypesOptionsBag } from "@Obsidian/ViewModels/Rest/Controls/entityTypePickerGetEntityTypesOptionsBag";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { computed, defineComponent, PropType, ref } from "vue";
import BaseAsyncPicker from "./baseAsyncPicker";

export default defineComponent({
    name: "EntityTypePicker",

    components: {
        BaseAsyncPicker
    },

    props: {
        modelValue: {
            type: Object as PropType<ListItemBag | ListItemBag[] | null>,
            required: false
        },

        includeGlobalOption: {
            type: Boolean as PropType<boolean>,
            default: false
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
        const http = useHttp();
        const loadedItems = ref<ListItemBag[] | null>(null);

        // #endregion

        // #region Computed Values

        /**
         * The actual items to make available to the picker. This allows us to do any
         * post-processing, such as adding additional items, and still be lazy loaded as well.
         */
        const actualItems = computed((): ListItemBag[] | (() => Promise<ListItemBag[]>) => {
            return loadedItems.value ? postProcessItems(loadedItems.value) : loadOptions;
        });

        // #endregion

        // #region Functions

        /**
         * Perform additional processing on the items based on our property
         * settings.
         *
         * @param items The items to be processed.
         *
         * @returns A new array of items that have been processed.
         */
        const postProcessItems = (items: ListItemBag[]): ListItemBag[] => {
            const processedItems = [...items];

            if (props.includeGlobalOption) {
                processedItems.splice(0, 0, {
                    value: emptyGuid,
                    text: "None (Global Attributes)"
                });
            }

            return processedItems;
        };

        /**
         * Loads the items from the server.
         */
        const loadOptions = async (): Promise<ListItemBag[]> => {
            const options: Partial<EntityTypePickerGetEntityTypesOptionsBag> = {
            };
            const result = await http.post<ListItemBag[]>("/api/v2/Controls/EntityTypePickerGetEntityTypes", undefined, options);

            if (result.isSuccess && result.data) {
                loadedItems.value = result.data;
                return postProcessItems(result.data);
            }
            else {
                console.error(result.errorMessage ?? "Unknown error while loading data.");
                loadedItems.value = [];
                return [];
            }
        };

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
    grouped
    :items="actualItems" />
`
});
