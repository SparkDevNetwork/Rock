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
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import RockFormField from "./rockFormField";
import { updateRefValue } from "@Obsidian/Utility/component";
import { defaultControlCompareValue } from "@Obsidian/Utility/stringUtils";

export default defineComponent({
    name: "RadioButtonList",
    components: {
        RockFormField
    },
    props: {
        items: {
            type: Array as PropType<ListItemBag[]>,
            default: []
        },

        modelValue: {
            type: String as PropType<string>,
            default: ""
        },

        disabled: {
            type: Boolean as PropType<boolean>,
            required: false,
            default: false
        },

        formGroupClasses: {
            type: String as PropType<string>,
            default: ""
        },

        repeatColumns: {
            type: Number as PropType<number>,
            default: 0
        },

        horizontal: {
            type: Boolean as PropType<boolean>,
            default: false
        },

        showBlankItem: {
            type: Boolean as PropType<boolean>,
            default: false
        },

        compareValue: {
            type: Function as PropType<((value: string, itemValue: string) => boolean)>,
            default: defaultControlCompareValue
        }
    },

    emits: {
        "update:modelValue": (_value: string) => true
    },

    setup(props, { emit }) {
        const internalValue = ref(props.modelValue);

        const containerClasses = computed((): string => {
            const classes: string[] = [];

            if (props.repeatColumns > 0) {
                classes.push(`in-columns in-columns-${props.repeatColumns}`);
            }

            if (props.horizontal) {
                classes.push("rockradiobuttonlist-horizontal");
            }
            else {
                classes.push("rockradiobuttonlist-vertical");
            }

            return classes.join(" ");
        });

        const actualItems = computed((): ListItemBag[] => {
            const items = [...props.items];

            if (props.showBlankItem) {
                items.splice(0, 0, {
                    value: "",
                    text: "None"
                });
            }

            return items;
        });

        function isItemDisabled(item: ListItemBag): boolean {
            return item.category === "disabled" || props.disabled;
        }

        const getItemUniqueId = (uniqueId: Guid, item: ListItemBag): string => {
            const key = (item.value ?? "").replace(" ", "-");

            return `${uniqueId}-${key}`;
        };

        const syncInternalValue = (): void => {
            let value = props.modelValue;

            if (value) {
                // Ensure it is a valid value, if not then set it to blank.
                const selectedOption = props.items.find(o => props.compareValue(value as string, o.value ?? "")) || null;

                if (!selectedOption) {
                    value = "";
                }
                else {
                    value = selectedOption.value ?? "";
                }
            }

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
            actualItems,
            containerClasses,
            getItemUniqueId,
            internalValue,
            isItemDisabled
        };
    },

    template: `
<RockFormField
    :formGroupClasses="'rock-radio-button-list ' + formGroupClasses"
     #default="{uniqueId}" name="radiobuttonlist" v-model="internalValue">
    <div class="control-wrapper">
        <slot name="prepend" :isInputGroupSupported="false" />
        <div class="controls rockradiobuttonlist" :class="containerClasses">
            <span>
                <template v-if="horizontal">
                    <label v-for="item in actualItems" class="radio-inline" :for="getItemUniqueId(uniqueId, item)" :key="item.value">
                        <input :id="getItemUniqueId(uniqueId, item)" :name="uniqueId" type="radio" :value="item.value" v-model="internalValue" :disabled="isItemDisabled(item)" />
                        <span class="label-text">{{item.text}}</span>
                    </label>
                </template>
                <template v-else>
                    <div v-for="item in actualItems" class="radio" :key="item.value">
                        <label :for="getItemUniqueId(uniqueId, item)">
                            <input :id="getItemUniqueId(uniqueId, item)" :name="uniqueId" type="radio" :value="item.value" v-model="internalValue" :disabled="isItemDisabled(item)" />
                            <span class="label-text">{{item.text}}</span>
                        </label>
                    </div>
                </template>
            </span>
        </div>
        <slot name="append" :isInputGroupSupported="false" />
    </div>
</RockFormField>`
});
