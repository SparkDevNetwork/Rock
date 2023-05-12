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
import { computed } from "vue";
import { defineComponent, PropType } from "vue";
import { useVModelPassthrough } from "@Obsidian/Utility/component";
import RockFormField from "./rockFormField";

export default defineComponent({
    name: "TextBox",
    components: {
        RockFormField
    },
    props: {
        modelValue: {
            type: String as PropType<string>,
            required: true
        },
        type: {
            type: String as PropType<string>,
            default: "text"
        },
        maxLength: {
            type: Number as PropType<number>,
            default: 524288
        },
        showCountDown: {
            type: Boolean as PropType<boolean>,
            default: false
        },
        placeholder: {
            type: String as PropType<string>,
            default: ""
        },
        inputClasses: {
            type: String as PropType<string>,
            default: ""
        },
        formGroupClasses: {
            type: String as PropType<string>,
            default: ""
        },
        rows: {
            type: Number as PropType<number>,
            default: 3
        },
        textMode: {
            type: String as PropType<string>,
            default: ""
        },
        size: {
            type: String as PropType<"small" | "medium" | "large">,
            default: "medium"
        }
    },
    emits: [
        "update:modelValue"
    ],
    setup(props, ctx) {
        const internalValue = useVModelPassthrough(props, "modelValue", ctx.emit);

        const isTextarea = computed((): boolean => {
            return props.textMode?.toLowerCase() === "multiline";
        });

        const charsRemaining = computed((): number => {
            return props.maxLength - internalValue.value.length;
        });

        const countdownClass = computed((): string => {
            if (charsRemaining.value >= 10) {
                return "badge-default";
            }

            if (charsRemaining.value >= 0) {
                return "badge-warning";
            }

            return "badge-danger";
        });

        const isInputGroup = computed((): boolean => {
            return !!ctx.slots.inputGroupPrepend || !!ctx.slots.inputGroupAppend;
        });

        const controlContainerClass = computed((): Record<string, boolean> => {
            return {
                "input-group": isInputGroup.value,
                "input-group-sm": isInputGroup.value && props.size == "small",
                "input-group-lg": isInputGroup.value && props.size == "large"
            };
        });

        const formControlClass = computed((): Record<string, boolean> => {
            return {
                "form-control": true,
                [props.inputClasses]: true,
                "input-sm": props.size == "small",
                "input-lg": props.size == "large"
            };
        });

        return {
            controlContainerClass,
            formControlClass,
            internalValue,
            isTextarea,
            charsRemaining,
            countdownClass
        };
    },
    template: `
<RockFormField
    v-model="internalValue"
    :formGroupClasses="'rock-text-box ' + formGroupClasses"
    name="textbox">
    <template #pre>
        <em v-if="showCountDown" class="pull-right badge" :class="countdownClass">
            {{charsRemaining}}
        </em>
    </template>
    <template #default="{uniqueId, field}">
        <div class="control-wrapper">
            <slot name="prepend" :isInputGroupSupported="true" />
            <div :class="controlContainerClass">
                <slot name="inputGroupPrepend" :isInputGroupSupported="true" />
                <textarea v-if="isTextarea" v-model="internalValue" :rows="rows" cols="20" :maxlength="maxLength" :id="uniqueId" class="form-control" v-bind="field"></textarea>
                <input v-else v-model="internalValue" :id="uniqueId" :type="type" :class="formControlClass" v-bind="field" :maxlength="maxLength" :placeholder="placeholder" />
                <slot name="inputGroupAppend" :isInputGroupSupported="true" />
            </div>
            <slot name="append" :isInputGroupSupported="true" />
        </div>
    </template>
</RockFormField>`
});
