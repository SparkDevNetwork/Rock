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

export default defineComponent({
    name: 'TextBox',
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
        placeholder: {
            type: String as PropType<string>,
            default: ''
        },
        inputClasses: {
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
    watch: {
        internalValue() {
            this.$emit('update:modelValue', this.internalValue);
        },
        modelValue() {
            this.internalValue = this.modelValue;
        }
    },
    template: `
<RockFormField
    v-model="internalValue"
    formGroupClasses="rock-text-box"
    name="textbox">
    <template #pre>
        <em v-if="showCountDown" class="pull-right badge" :class="countdownClass">
            {{charsRemaining}}
        </em>
    </template>
    <template #default="{uniqueId, field, errors, disabled, tabIndex}">
        <div class="control-wrapper">
            <input :id="uniqueId" :type="type" class="form-control" :class="inputClasses" v-bind="field" :disabled="disabled" :maxlength="maxLength" :placeholder="placeholder" :tabindex="tabIndex" />
        </div>
    </template>
</RockFormField>`
});
