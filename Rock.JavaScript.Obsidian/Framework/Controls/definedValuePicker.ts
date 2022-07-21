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
import { useSecurityGrantToken } from "@Obsidian/Utility/block";
import { standardAsyncPickerProps, useStandardAsyncPickerProps, useVModelPassthrough } from "@Obsidian/Utility/component";
import { useHttp } from "@Obsidian/Utility/http";
import { DefinedValuePickerGetDefinedValuesOptionsBag } from "@Obsidian/ViewModels/Rest/Controls/definedValuePickerGetDefinedValuesOptionsBag";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { defineComponent, PropType, ref, watch } from "vue";
import BaseAsyncPicker from "./baseAsyncPicker";
import RockFormField from "./rockFormField";

export default defineComponent({
    name: "DefinedValuePicker",

    components: {
        BaseAsyncPicker,
        RockFormField
    },

    props: {
        modelValue: {
            type: Object as PropType<ListItemBag | ListItemBag[] | null>,
            required: false
        },

        ...standardAsyncPickerProps,

        definedTypeGuid: {
            type: String as PropType<Guid>,
            required: false
        }
    },

    emits: {
        "update:modelValue": (_value: ListItemBag | ListItemBag[] | null) => true
    },

    setup(props, { emit }) {
        const standardProps = useStandardAsyncPickerProps(props);
        const securityGrantToken = useSecurityGrantToken();
        const http = useHttp();
        const internalValue = useVModelPassthrough(props, "modelValue", emit);
        const itemsSource = ref<(() => Promise<ListItemBag[]>) | null>(null);

        const loadItems = async (): Promise<ListItemBag[]> => {
            const options: Partial<DefinedValuePickerGetDefinedValuesOptionsBag> = {
                definedTypeGuid: props.definedTypeGuid,
                securityGrantToken: securityGrantToken.value
            };
            const url = "/api/v2/Controls/DefinedValuePickerGetDefinedValues";
            const result = await http.post<ListItemBag[]>(url, undefined, options);

            if (result.isSuccess && result.data) {
                return result.data;
            }
            else {
                console.error(result.errorMessage ?? "Unknown error while loading data.");
                return [];
            }
        };

        watch(() => props.definedTypeGuid, () => {
            // Pass as a wrapped function to ensure lazy loading works.
            itemsSource.value = () => loadItems();
        });

        itemsSource.value = () => loadItems();

        return {
            internalValue,
            itemsSource,
            standardProps
        };
    },
    template: `
<BaseAsyncPicker v-model="internalValue"
    v-bind="standardProps"
    :items="itemsSource" />
`
});
