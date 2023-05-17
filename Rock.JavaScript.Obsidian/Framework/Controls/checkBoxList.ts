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
import { defaultControlCompareValue } from "@Obsidian/Utility/stringUtils";

export default defineComponent({
    name: "CheckBoxList",

    components: {
        RockFormField
    },

    props: {
        modelValue: {
            type: Array as PropType<string[]>,
            default: []
        },

        disabled: {
            type: Boolean as PropType<boolean>,
            required: false,
            default: false
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
        },

        compareValue: {
            type: Function as PropType<((value: string, itemValue: string) => boolean)>,
            default: defaultControlCompareValue
        }
    },

    emits: {
        "update:modelValue": (_value: string[]) => true
    },

    setup(props, { emit }) {
        const internalValue = ref([...props.modelValue]);

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
                classes.push("rockcheckboxlist-vertical input-group");
            }

            return classes.join(" ");
        });

        const syncInternalValue = (): void => {
            let value = [...props.modelValue];

            // Ensure they are all valid values and make sure they are the
            // correct matching value from the item rather than what was
            // originally provided.
            value = props.items
                .filter(o => value.some(v => props.compareValue(v, o.value ?? "")))
                .map(o => o.value ?? "");

            updateRefValue(internalValue, value);
        };

        watch([() => props.modelValue, () => props.items], () => {
            syncInternalValue();
        });

        watch(internalValue, () => {
            emit("update:modelValue", internalValue.value);
        });

        syncInternalValue();

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
            <slot name="prepend" :isInputGroupSupported="false" />
            <div class="controls rockcheckboxlist" :class="containerClasses">
                <template v-if="horizontal">
                    <label v-for="item in items" class="checkbox-inline" :for="uniqueIdForItem(uniqueId, item)">
                        <input :disabled="disabled" :id="uniqueIdForItem(uniqueId, item)" :name="uniqueId" type="checkbox" :value="valueForItem(item)" v-model="internalValue" />
                        <span class="label-text">{{textForItem(item)}}</span>
                    </label>
                </template>
                <template v-else>
                    <div v-for="item in items" class="checkbox">
                        <label :for="uniqueIdForItem(uniqueId, item)">
                            <input :disabled="disabled" :id="uniqueIdForItem(uniqueId, item)" :name="uniqueId" type="checkbox" :value="valueForItem(item)" v-model="internalValue" />
                            <span class="label-text">{{textForItem(item)}}</span>
                        </label>
                    </div>
                </template>
            </div>
            <slot name="append" :isInputGroupSupported="false" />
        </div>
    </template>
</RockFormField>
`
});
