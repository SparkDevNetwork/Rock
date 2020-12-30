import { getFieldTypeComponent } from '../Fields/Index.js';
import { Guid } from '../Util/Guid.js';
import { Component, defineComponent, PropType } from '../Vendor/Vue/vue.js';

import './BooleanField.js';
import './TextField.js';

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
            return getFieldTypeComponent(this.fieldTypeGuid);
        }
    },
    template: `
<component :is="fieldComponent" v-model="modelValue" />`
});
