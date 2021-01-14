import { getFieldTypeComponent } from '../Fields/Index.js';
import { Guid } from '../Util/Guid.js';
import { Component, defineComponent, PropType } from '../Vendor/Vue/vue.js';

// Import and assign TextField because it is the callback
import TextField from '../Fields/TextField.js';

// Import other field types so they are registered and available upon dynamic request
import '../Fields/BooleanField.js';
import '../Fields/DateField.js';

export default defineComponent({
    name: 'RockField',
    props: {
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
<component :is="fieldComponent" />`
});
