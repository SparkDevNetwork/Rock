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
import { useHttp } from "@Obsidian/Utility/http";
import { FinancialGatewayPickerGetFinancialGatewaysOptionsBag } from "@Obsidian/ViewModels/Rest/Controls/financialGatewayPickerGetFinancialGatewaysOptionsBag";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { computed, defineComponent, PropType, ref, watch } from "vue";
import BaseAsyncPicker from "./baseAsyncPicker";

export default defineComponent({
    name: "FinancialGatewayPicker",

    components: {
        BaseAsyncPicker
    },

    props: {
        modelValue: {
            type: Object as PropType<ListItemBag | ListItemBag[] | null>,
            required: false
        },

        includeInactive: {
            type: Boolean as PropType<boolean>,
            default: false
        },

        showAllGatewayComponents: {
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
            return loadedItems.value || loadOptions;
        });

        // #endregion

        // #region Functions

        /**
         * Loads the items from the server.
         */
        const loadOptions = async (): Promise<ListItemBag[]> => {
            const options: Partial<FinancialGatewayPickerGetFinancialGatewaysOptionsBag> = {
                includeInactive: props.includeInactive,
                showAllGatewayComponents: props.showAllGatewayComponents
            };

            const result = await http.post<ListItemBag[]>("/api/v2/Controls/FinancialGatewayPickerGetFinancialGateways", undefined, options);

            if (result.isSuccess && result.data) {
                let items = result.data;

                // If we have some selected values, add them to the list if not already present.
                if (internalValue.value && Array.isArray(internalValue.value)) {
                    // Get the selected values that don't already exist in the list and add them
                    items = internalValue.value.filter(gateway => !items.some(item => item.value === gateway.value)).concat(items);
                }
                // If we have a single selected value & isn't already in the list, add it to the list.
                else if (internalValue.value && !Array.isArray(internalValue.value) && internalValue.value.value && !items.some(item => item.value === (internalValue.value as ListItemBag).value)) {
                    // Add it in at the front if not already in the list
                    items.unshift(internalValue.value);
                }

                loadedItems.value = items;
                return items;
            }
            else {
                console.error(result.errorMessage ?? "Unknown error while loading data.");
                loadedItems.value = [];
                return [];
            }
        };

        // #endregion

        // #region Watchers

        watch(() => props.includeInactive, () => {
            loadedItems.value = null;
        });

        watch(() => props.showAllGatewayComponents, () => {
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
