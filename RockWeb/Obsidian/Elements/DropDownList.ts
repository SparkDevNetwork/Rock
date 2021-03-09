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
import { defineComponent, PropType } from 'vue';
import RockFormField from './RockFormField';

export type DropDownListOption = {
    key: string,
    value: string,
    text: string
};

export default defineComponent({
    name: 'DropDownList',
    components: {
        RockFormField
    },
    props: {
        modelValue: {
            type: String as PropType<string>,
            required: true
        },
        options: {
            type: Array as PropType<DropDownListOption[]>,
            required: true
        },
        showBlankItem: {
            type: Boolean as PropType<boolean>,
            default: true
        },
        blankValue: {
            type: String as PropType<string>,
            default: ''
        },
        formControlClasses: {
            type: String as PropType<string>,
            default: ''
        },
        placeholder: {
            type: String as PropType<string>,
            default: ''
        }
    },
    data: function () {
        return {
            internalValue: this.blankValue
        };
    },
    watch: {
        modelValue: {
            immediate: true,
            handler() {
                this.internalValue = this.modelValue;
            }
        },
        internalValue() {
            this.$emit('update:modelValue', this.internalValue);
        }
    },
    template: `
<RockFormField
    :modelValue="internalValue"
    formGroupClasses="rock-drop-down-list"
    name="dropdownlist">
    <template #default="{uniqueId, field, errors, disabled}">
        <div class="control-wrapper">
            <select :id="uniqueId" class="form-control" :class="formControlClasses" :disabled="disabled" v-model="internalValue">
                <option v-if="showBlankItem" :value="blankValue">{{placeholder}}</option>
                <option v-for="o in options" :key="o.key" :value="o.value">{{o.text}}</option>
            </select>
        </div>
    </template>
</RockFormField>`
});
