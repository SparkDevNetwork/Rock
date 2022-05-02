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
import { useVModelPassthrough } from "@Obsidian/Utility/component";
import { get } from "@Obsidian/Utility/http";
import { containsRequiredRule, rulesPropType } from "@Obsidian/ValidationRules";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { computed, defineComponent, PropType, ref, watch } from "vue";
import BaseAsyncPicker from "./baseAsyncPicker";
import RockFormField from "./rockFormField";

export default defineComponent({
    name: "DefinedTypePicker",

    components: {
        BaseAsyncPicker,
        RockFormField
    },

    props: {
        modelValue: {
            type: Object as PropType<ListItemBag | ListItemBag[] | null>,
            required: false
        },

        definedTypeGuid: {
            type: String as PropType<Guid>,
            required: false
        },

        rules: rulesPropType,

        securityKey: {
            type: String as PropType<string>,
            required: false
        },

        multiple: {
            type: Boolean as PropType<boolean>,
            default: false
        },

        showBlankItem: {
            type: Boolean as PropType<boolean>,
            default: false
        }
    },

    emits: {
        "update:modelValue": (_value: ListItemBag | ListItemBag[] | null) => true
    },

    setup(props, { emit }) {
        const internalValue = useVModelPassthrough(props, "modelValue", emit);
        const itemsSource = ref<(() => Promise<ListItemBag[]>) | null>(null);

        const computedShowBlankItem = computed((): boolean => {
            return props.showBlankItem || containsRequiredRule(props.rules);
        });

        const loadItems = async (): Promise<ListItemBag[]> => {
            const url = `/api/v2/Controls/DefinedValuePicker/definedValues/${props.definedTypeGuid}`;
            const result = await get<ListItemBag[]>(url);

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
            computedShowBlankItem,
            internalValue,
            itemsSource
        };
    },
    template: `
<BaseAsyncPicker v-model="internalValue"
    :items="itemsSource"
    :multiple="multiple"
    :rules="rules"
    :showBlankItem="computedShowBlankItem" />
`
});
