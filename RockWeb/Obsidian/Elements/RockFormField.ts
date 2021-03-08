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
    name: 'RockFormField',
    components: {
        Field,
        RockLabel
    },
    props: {
        modelValue: {
            required: true
        },
        name: {
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
        rules: {
            type: String as PropType<string>,
            default: ''
        },
        disabled: {
            type: Boolean as PropType<boolean>,
            default: false
        },
        formGroupClasses: {
            type: String as PropType<string>,
            default: ''
        },
        validationTitle: {
            type: String as PropType<string>,
            default: ''
        }
    },
    emits: [
        'update:modelValue'
    ],
    data: function () {
        return {
            uniqueId: `rock-${this.name}-${newGuid()}`,
            internalValue: this.modelValue
        };
    },
    computed: {
        isRequired(): boolean {
            return this.rules.includes('required');
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
<Field v-model="internalValue" :name="validationTitle || label" :rules="rules" #default="{field, errors}">
    <slot name="pre" />
    <div class="form-group" :class="[formGroupClasses, isRequired ? 'required' : '', Object.keys(errors).length ? 'has-error' : '']">
        <RockLabel v-if="label || help" :for="uniqueId" :help="help">
            {{label}}
        </RockLabel>
        <slot v-bind="{uniqueId, field, errors, disabled}" />
    </div>
    <slot name="post" />
</Field>`
});
