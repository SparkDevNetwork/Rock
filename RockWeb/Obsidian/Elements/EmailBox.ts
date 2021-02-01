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
import { ruleStringToArray, ruleArrayToString } from '../Rules/Index.js';
import { defineComponent, PropType } from '../Vendor/Vue/vue.js';
import TextBox from './TextBox.js';

export default defineComponent({
    name: 'EmailBox',
    components: {
        TextBox
    },
    props: {
        modelValue: {
            type: String as PropType<string>,
            required: true
        },
        label: {
            type: String as PropType<string>,
            default: 'Email'
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
<TextBox v-model.trim="internalValue" :label="label" :rules="computedRules" />`
});
