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
import { newGuid } from '../Util/Guid';
import { ruleStringToArray } from '../Rules/Index';

export default defineComponent({
    name: 'CheckBox',
    props: {
        modelValue: {
            type: Boolean as PropType<boolean>,
            required: true
        },
        label: {
            type: String as PropType<string>,
            required: true
        },
        inline: {
            type: Boolean as PropType<boolean>,
            default: true
        },
        rules: {
            type: String as PropType<string>,
            default: ''
        }
    },
    data: function () {
        return {
            uniqueId: `rock-checkbox-${newGuid()}`,
            internalValue: this.modelValue
        };
    },
    methods: {
        toggle() {
            if (!this.isRequired) {
                this.internalValue = !this.internalValue;
            }
            else {
                this.internalValue = true;
            }
        }
    },
    computed: {
        isRequired() {
            const rules = ruleStringToArray(this.rules);
            return rules.indexOf('required') !== -1;
        }
    },
    watch: {
        modelValue() {
            this.internalValue = this.modelValue;
        },
        internalValue() {
            this.$emit('update:modelValue', this.internalValue);
        },
        isRequired: {
            immediate: true,
            handler() {
                if (this.isRequired) {
                    this.internalValue = true;
                }
            }
        }
    },
    template: `
<div v-if="inline" class="checkbox">
    <label title="">
        <input type="checkbox" v-model="internalValue" />
        <span class="label-text ">{{label}}</span>
    </label>
</div>
<div v-else class="form-group rock-check-box" :class="isRequired ? 'required' : ''">
    <label class="control-label" :for="uniqueId">{{label}}</label>
    <div class="control-wrapper">
        <div class="rock-checkbox-icon" @click="toggle" :class="isRequired ? 'text-muted' : ''">
            <i v-if="modelValue" class="fa fa-check-square-o fa-lg"></i>
            <i v-else class="fa fa-square-o fa-lg"></i>
        </div>
    </div>
</div>`
});
