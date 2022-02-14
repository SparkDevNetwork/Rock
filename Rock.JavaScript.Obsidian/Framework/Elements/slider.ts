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
import { defineComponent, PropType } from "vue";
import { useVModelPassthrough } from "../Util/component";
import InlineSlider from "./inlineSlider";
import RockFormField from "./rockFormField";

export default defineComponent({
    name: "Slider",

    components: {
        InlineSlider,
        RockFormField
    },

    props: {
        modelValue: {
            type: Number as PropType<number>,
            required: true
        },

        isIntegerOnly: {
            type: Boolean as PropType<boolean>,
            default: false
        },

        min: {
            type: Number as PropType<number>,
            default: 0
        },

        max: {
            type: Number as PropType<number>,
            default: 100
        },

        showValueBar: {
            type: Boolean as PropType<boolean>,
            default: false
        }
    },

    emits: [
        "update:modelValue"
    ],

    setup(props, { emit }) {
        const internalValue = useVModelPassthrough(props, "modelValue", emit);

        return {
            internalValue
        };
    },

    template: `
<RockFormField
    :modelValue="internalValue"
    formGroupClasses="rock-switch"
    name="switch">
    <template #default="{uniqueId, field}">
        <div class="control-wrapper">
            <InlineSlider v-model="internalValue" :uniqueId="uniqueId" v-bind="field" :isIntegerOnly="isIntegerOnly" :min="min" :max="max" :showValueBar="showValueBar" />
        </div>
    </template>
</RockFormField>
`
});
