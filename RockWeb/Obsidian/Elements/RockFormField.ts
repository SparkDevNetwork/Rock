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

import { defineComponent, inject, PropType } from 'vue';
import { newGuid } from '../Util/Guid';
import { Field } from 'vee-validate';
import RockLabel from './RockLabel';
import { FormState } from '../Controls/RockForm';

export default defineComponent({
    name: 'RockFormField',
    components: {
        Field,
        RockLabel
    },
    setup() {
        const formState = inject<FormState | null>('formState', null); 

        return {
            formState
        };
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
        inputGroupClasses: {
            type: String as PropType<string>,
            default: ''
        },
        validationTitle: {
            type: String as PropType<string>,
            default: ''
        },
        'class': {
            type: String as PropType<string>,
            default: ''
        },
        tabIndex: {
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
        },
        classAttr(): string {
            return this.class;
        },
        errorClasses(): (formState: FormState | null, errors: Record<string, string>) => string {
            return (formState: FormState | null, errors: Record<string, string>) => {
                if (!formState || formState.submitCount < 1) {
                    return '';
                }

                return Object.keys(errors).length ? 'has-error' : '';
            };
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
    <div class="form-group" :class="[classAttr, formGroupClasses, isRequired ? 'required' : '', errorClasses(formState, errors)]">
        <RockLabel v-if="label || help" :for="uniqueId" :help="help">
            {{label}}
        </RockLabel>
        <slot v-bind="{uniqueId, field, errors, disabled, inputGroupClasses, tabIndex}" />
    </div>
    <slot name="post" />
</Field>`
});
