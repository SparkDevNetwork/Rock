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
import { ruleStringToArray, ruleArrayToString } from '../Rules/Index';
import { defineComponent, PropType } from 'vue';
import RockFormField from './RockFormField';

export default defineComponent({
    name: 'EmailBox',
    components: {
        RockFormField
    },
    props: {
        modelValue: {
            type: String as PropType<string>,
            required: true
        },
        rules: {
            type: String as PropType<string>,
            default: ''
        }
    },
    emits: [
        'update:modelValue'
    ],
    data: function () {
        return {
            internalValue: this.modelValue
        };
    },
    computed: {
        computedRules() {
            const rules = ruleStringToArray(this.rules);

            if (rules.indexOf('email') === -1) {
                rules.push('email');
            }

            return ruleArrayToString(rules);
        }
    },
    watch: {
        internalValue() {
            this.$emit('update:modelValue', this.internalValue);
        },
        modelValue () {
            this.internalValue = this.modelValue;
        }
    },
    template: `
<RockFormField
    v-model="internalValue"
    formGroupClasses="rock-text-box"
    name="textbox"
    :rules="computedRules">
    <template #default="{uniqueId, field, errors, tabIndex, disabled}">
        <div class="control-wrapper">
            <div class="input-group">
                <span class="input-group-addon">
                    <i class="fa fa-envelope"></i>
                </span>
                <input :id="uniqueId" class="form-control" v-bind="field" :disabled="disabled" :tabindex="tabIndex" />
            </div>
        </div>
    </template>
</RockFormField>`
});
