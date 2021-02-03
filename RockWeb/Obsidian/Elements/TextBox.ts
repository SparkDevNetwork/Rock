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
import { Field } from 'vee-validate';
import RockLabel from './RockLabel';

export default defineComponent({
    name: 'TextBox1',
    components: {
        Field,
        RockLabel
    },
    props: {
        modelValue: {
            type: String as PropType<string>,
            required: true
        },
        label: {
            type: String as PropType<string>,
            default: ''
        },
        help: {
            type: String as PropType<string>,
            default: ''
        },
        type: {
            type: String as PropType<string>,
            default: 'text'
        },
        maxLength: {
            type: Number as PropType<number>,
            default: 524288
        },
        showCountDown: {
            type: Boolean as PropType<boolean>,
            default: false
        },
        rules: {
            type: String as PropType<string>,
            default: ''
        },
        disabled: {
            type: Boolean as PropType<boolean>,
            default: false
        },
        placeholder: {
            type: String as PropType<string>,
            default: ''
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
        },
        charsRemaining(): number {
            return this.maxLength - this.modelValue.length;
        },
        countdownClass(): string {
            if (this.charsRemaining >= 10) {
                return 'badge-default';
            }

            if (this.charsRemaining >= 0) {
                return 'badge-warning';
            }

            return 'badge-danger';
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
    <em v-if="showCountDown" class="pull-right badge" :class="countdownClass">
        {{charsRemaining}}
    </em>
    <div class="form-group rock-text-box" :class="{required: isRequired, 'has-error': Object.keys(errors).length}">
        <RockLabel v-if="label" :for="uniqueId" :help="help">
            {{label}}
        </RockLabel>
        <div class="control-wrapper">
            <input :id="uniqueId" :type="type" class="form-control" v-bind="field" :disabled="disabled" :maxlength="maxLength" :placeholder="placeholder" />
        </div>
    </div>
</Field>`
});
