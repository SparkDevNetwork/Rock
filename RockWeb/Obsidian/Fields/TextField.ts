import { defineComponent, PropType } from '../Vendor/Vue/vue.js';
import { Guid } from '../Util/Guid.js';
import { registerFieldType } from './Index.js';

const fieldTypeGuid: Guid = '9C204CD0-1233-41C5-818A-C5DA439445AA';

export default registerFieldType(fieldTypeGuid, defineComponent({
    name: 'TextField',
    props: {
        modelValue: {
            type: String as PropType<string>,
            required: true
        }
    },
    computed: {
        safeValue(): string {
            return (this.modelValue || '').trim();
        },
        valueIsNull(): boolean {
            return !this.safeValue;
        }
    },
    template: `
<span>{{ safeValue }}</span>`
}));
