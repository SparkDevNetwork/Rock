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
import { updateRefValue } from "../Util/util";
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

        options: {
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

        const valueForOption = (option: ListItemBag): string => option.value ?? "";
        const textForOption = (option: ListItemBag): string => option.text ?? "";

        const uniqueIdForOption = (uniqueId: Guid, option: ListItemBag): string => `${uniqueId}-${(option.value ?? "").replace(" ", "-")}`;

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
            textForOption,
            uniqueIdForOption,
            valueForOption
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
                    <label v-for="option in options" class="checkbox-inline" :for="uniqueIdForOption(uniqueId, option)">
                        <input :id="uniqueIdForOption(uniqueId, option)" :name="uniqueId" type="checkbox" :value="valueForOption(option)" v-model="internalValue" />
                        <span class="label-text">{{textForOption(option)}}</span>
                    </label>
                </template>
                <template v-else>
                    <div v-for="option in options" class="checkbox">
                        <label :for="uniqueIdForOption(uniqueId, option)">
                            <input :id="uniqueIdForOption(uniqueId, option)" :name="uniqueId" type="checkbox" :value="valueForOption(option)" v-model="internalValue" />
                            <span class="label-text">{{textForOption(option)}}</span>
                        </label>
                    </div>
                </template>
            </div>
        </div>
    </template>
</RockFormField>
`
});
