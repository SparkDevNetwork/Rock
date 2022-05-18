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
import { computed, defineComponent, PropType, ref, watch } from "vue";
import { updateRefValue } from "@Obsidian/Utility/component";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import RockFormField from "./rockFormField";

export default defineComponent({
    name: "CheckBoxList",

    components: {
        RockFormField
    },

    props: {
        modelValue: {
            type: Array as PropType<Array<string>>,
            default: []
        },

        items: {
            type: Array as PropType<Array<ListItemBag>>,
            required: true
        },

        repeatColumns: {
            type: Number as PropType<number>,
            default: 0
        },

        horizontal: {
            type: Boolean as PropType<boolean>,
            default: false
        }
    },

    setup(props, { emit }) {
        const internalValue = ref([...props.modelValue]);

        watch(() => props.modelValue, () => updateRefValue(internalValue, props.modelValue));
        watch(internalValue, () => emit("update:modelValue", internalValue.value));

        const valueForItem = (item: ListItemBag): string => item.value ?? "";
        const textForItem = (item: ListItemBag): string => item.text ?? "";

        const uniqueIdForItem = (uniqueId: Guid, item: ListItemBag): string => `${uniqueId}-${(item.value ?? "").replace(" ", "-")}`;

        const containerClasses = computed(() => {
            const classes: string[] = [];

            if (props.horizontal) {
                classes.push("rockcheckboxlist-horizontal");

                if (props.repeatColumns > 0) {
                    classes.push(`in-columns in-columns-${props.repeatColumns}`);
                }
            }
            else {
                classes.push("rockcheckboxlist-vertical");
            }

            return classes.join(" ");
        });

        return {
            containerClasses,
            internalValue,
            textForItem,
            uniqueIdForItem,
            valueForItem
        };
    },

    template: `
<RockFormField
    :modelValue="internalValue"
    formGroupClasses="check-box-list"
    name="check-box-list">
    <template #default="{uniqueId}">
        <div class="control-wrapper">
            <div class="controls rockcheckboxlist" :class="containerClasses">
                <template v-if="horizontal">
                    <label v-for="item in items" class="checkbox-inline" :for="uniqueIdForItem(uniqueId, item)">
                        <input :id="uniqueIdForItem(uniqueId, item)" :name="uniqueId" type="checkbox" :value="valueForItem(item)" v-model="internalValue" />
                        <span class="label-text">{{textForItem(item)}}</span>
                    </label>
                </template>
                <template v-else>
                    <div v-for="item in items" class="checkbox">
                        <label :for="uniqueIdForItem(uniqueId, item)">
                            <input :id="uniqueIdForItem(uniqueId, item)" :name="uniqueId" type="checkbox" :value="valueForItem(item)" v-model="internalValue" />
                            <span class="label-text">{{textForItem(item)}}</span>
                        </label>
                    </div>
                </template>
            </div>
        </div>
    </template>
</RockFormField>
`
});
