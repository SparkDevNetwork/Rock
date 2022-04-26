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
import { computed, defineComponent, PropType } from "vue";
import JavaScriptAnchor from "./javaScriptAnchor";
import RockFormField from "./rockFormField";

export default defineComponent({
    name: "Toggle",

    components: {
        JavaScriptAnchor,
        RockFormField
    },

    props: {
        modelValue: {
            type: Boolean as PropType<boolean>,
            required: true
        },

        trueText: {
            type: String as PropType<string>,
            default: "On"
        },

        falseText: {
            type: String as PropType<string>,
            default: "Off"
        },

        btnSize: {
            type: String as PropType<string>,
            default: ""
        }
    },

    setup(props, { emit }) {
        const getButtonGroupClass = computed((): string[] => {
            const classes = ["btn-group", "btn-toggle"];

            if (props.btnSize) {
                classes.push(`btn-group-${props.btnSize}`);
            }

            return classes;
        });

        const onClick = (isOn: boolean): void => {
            if (isOn !== props.modelValue) {
                emit("update:modelValue", isOn);
            }
        };

        return {
            getButtonGroupClass,
            onClick,
            selectedClasses: "active btn btn-primary",
            unselectedClasses: "btn btn-default"
        };
    },

    template: `
<RockFormField
    :modelValue="modelValue"
    formGroupClasses="toggle"
    name="toggle">
    <template #default="{uniqueId, field}">
        <div class="control-wrapper">
            <div class="toggle-container">
                <div :class="getButtonGroupClass">
                    <JavaScriptAnchor :class="modelValue ? unselectedClasses : selectedClasses" @click="onClick(false)">
                        <slot name="off">{{falseText}}</slot>
                    </JavaScriptAnchor>
                    <JavaScriptAnchor :class="modelValue ? selectedClasses : unselectedClasses" @click="onClick(true)">
                        <slot name="on">{{trueText}}</slot>
                    </JavaScriptAnchor>
                </div>
            </div>
        </div>
    </template>
</RockFormField>`
});
