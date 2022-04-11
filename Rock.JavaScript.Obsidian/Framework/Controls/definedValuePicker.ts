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
import { computed, defineComponent, PropType, ref } from "vue";
import BasePicker from "./basePicker";
import RockFormField from "../Elements/rockFormField";
import { useVModelPassthrough } from "../Util/component";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { get } from "../Util/http";

export default defineComponent({
    name: "DefinedTypePicker",

    components: {
        BasePicker,
        RockFormField
    },

    props: {
        modelValue: {
            type: Object as PropType<ListItemBag>,
            required: false
        },

        definedTypeGuid: {
            type: String as PropType<Guid>,
            required: false
        }
    },

    emits: [
        "update:modelValue"
    ],

    setup(props, { emit }) {
        const internalValue = useVModelPassthrough(props, "modelValue", emit);
        const items = ref<ListItemBag[]>([]);
        const isPickerOpen = ref(false);

        const validationValue = computed((): string => internalValue.value?.value ?? "");
        const selectedText = computed((): string => internalValue.value?.text ?? "");

        const onSelectItem = (item: ListItemBag): void => {
            internalValue.value = item;
            isPickerOpen.value = false;
        };

        const onLoadData = async (): Promise<void> => {
            const url = `/api/v2/Controls/DefinedValuePicker/definedValues/${props.definedTypeGuid}`;
            const result = await get<ListItemBag[]>(url);

            if (result.isSuccess && result.data) {
                items.value = result.data;
            }
        };

        return {
            isPickerOpen,
            items,
            onSelectItem,
            onLoadData,
            selectedText,
            validationValue
        };
    },
    template: `
<RockFormField
    :modelValue="internalValue"
    formGroupClasses="rock-defined-type-picker"
    name="defined-type-picker">
    <template #default="{uniqueId, field}">
        <div class="control-wrapper">
            <BasePicker v-model="isPickerOpen"
                :id="uniqueId"
                v-bind="field"
                :text="selectedText"
                saveText=""
                @load="onLoadData">
                <div>
                    <div v-for="item in items" @click="onSelectItem(item)" style="padding: 8px; border-bottom: 1px solid gray;">{{ item.text }}</div>
                </div>
            </BasePicker>
        </div>
    </template>
</RockFormField>`
});
