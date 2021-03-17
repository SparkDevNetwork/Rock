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
import { Guid } from '../Util/Guid';
import { DropDownListOption } from './DropDownList';
import RockFormField from './RockFormField';

export default defineComponent({
    name: 'RadioButtonList',
    components: {
        RockFormField
    },
    props: {
        options: {
            type: Array as PropType<DropDownListOption[]>,
            default: []
        },
        modelValue: {
            type: String as PropType<string>,
            default: ''
        }
    },
    emits: [
        'update:modelValue'
    ],
    data() {
        return {
            internalValue: ''
        };
    },
    methods: {
        getOptionUniqueId(uniqueId: Guid, option: DropDownListOption): string {
            return `${uniqueId}-${option.key}`;
        }
    },
    watch: {
        internalValue() {
            this.$emit('update:modelValue', this.internalValue);
        },
        modelValue: {
            immediate: true,
            handler() {
                this.internalValue = this.modelValue;
            }
        }
    },
    template: `
<RockFormField formGroupClasses="rock-radio-button-list" #default="{uniqueId}" name="radiobuttonlist" v-model="internalValue">
    <div class="control-wrapper">
        <div class="controls rockradiobuttonlist rockradiobuttonlist-vertical">
            <span>
                <div v-for="option in options" class="radio">
                    <label class="" :for="getOptionUniqueId(uniqueId, option)">
                        <input :id="getOptionUniqueId(uniqueId, option)" :name="uniqueId" type="radio" :value="option.value" v-model="internalValue" />
                        <span class="label-text">{{option.text}}</span>
                    </label>
                </div>
            </span>
        </div>
    </div>
</RockFormField>`
});