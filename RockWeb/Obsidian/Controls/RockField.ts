import { getFieldTypeComponent } from '../Fields/Index.js';
import { Guid } from '../Util/Guid.js';
import { Component, defineComponent, PropType } from '../Vendor/Vue/vue.js';

import TextField from '../Fields/TextField.js';
import '../Fields/BooleanField.js';
import '../Fields/DateField.js';

export default defineComponent({
    name: 'RockField',
    props: {
        modelValue: {
            type: String as PropType<string>,
            required: true
        },
        fieldTypeGuid: {
            type: String as PropType<Guid>,
            required: true
        },
        edit: {
            type: Boolean as PropType<boolean>,
            default: false
        },
        label: {
            type: String as PropType<string>,
            default: ''
        }
    },
    data() {
        return {
            internalValue: this.modelValue
        };
    },
    computed: {
        fieldComponent(): Component | null {
            const field = getFieldTypeComponent(this.fieldTypeGuid);

            if (!field) {
                // Fallback to text field
                return TextField.component;
            }

            return field;
        }
    },
    watch: {
        internalValue() {
            this.$emit('update:modelValue', this.internalValue);
        }
    },
    template: `
<component :is="fieldComponent" v-model="internalValue" :edit="edit" :label="label" />`
});
