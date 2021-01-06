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
        }
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
    template: `
<component :is="fieldComponent" v-model="modelValue" />`
});
