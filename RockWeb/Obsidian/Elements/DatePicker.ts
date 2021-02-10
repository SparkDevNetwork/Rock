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
import { defineComponent, PropType } from '../Vendor/Vue/vue.js';
import { newGuid } from '../Util/Guid.js';
import { Field } from '../Vendor/VeeValidate/vee-validate.js';
import RockLabel from './RockLabel.js';
import { RockDateType } from '../Util/RockDate.js';

export default defineComponent({
    name: 'DatePicker',
    components: {
        Field,
        RockLabel
    },
    props: {
        modelValue: {
            type: String as PropType<RockDateType>,
            required: true
        },
        label: {
            type: String as PropType<string>,
            required: true
        },
        help: {
            type: String as PropType<string>,
            default: ''
        },
        rules: {
            type: String as PropType<string>,
            default: ''
        },
        disabled: {
            type: Boolean,
            default: false
        }
    },
    emits: [
        'update:modelValue'
    ],
    data: function () {
        return {
            uniqueId: `rock-textbox-${newGuid()}`,
            internalValue: this.modelValue
        };
    },
    computed: {
        isRequired(): boolean {
            return this.rules.includes('required');
        }
    },
    methods: {
        handleInput() {
            this.$emit('update:modelValue', this.internalValue);
        }
    },
    watch: {
        modelValue: function () {
            this.internalValue = this.modelValue;
        }
    },
    template: `
<Field
    v-model="internalValue"
    @input="handleInput"
    :name="label"
    :rules="rules"
    #default="{field, errors}">
    <div class="form-group date-picker" :class="{required: isRequired, 'has-error': Object.keys(errors).length}">
        <RockLabel :for="uniqueId" :help="help">
            {{label}}
        </RockLabel>
        <div class="control-wrapper">
            <div class="input-group input-width-md date">
                <input :id="uniqueId" type="text" class="form-control" :disabled="disabled" v-bind="field" onfocus="(this.type='date')" onblur="(this.type='text')" />
                <label :for="uniqueId" class="input-group-addon" :disabled="disabled">
                    <i class="fa fa-calendar"></i>
                </label>
            </div>
        </div>
    </div>
</Field>`
});
