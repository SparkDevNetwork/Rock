import { defineComponent, PropType } from '../Vendor/Vue/vue.js';
import { Guid } from '../Util/Guid.js';
import { registerFieldType } from './Index.js';
import { asDateString } from '../Filters/Date.js';

const fieldTypeGuid: Guid = '6B6AA175-4758-453F-8D83-FCD8044B5F36';

export default registerFieldType(fieldTypeGuid, defineComponent({
    name: 'DateField',
    props: {
        modelValue: {
            type: String as PropType<string>,
            required: true
        }
    },
    computed: {
        valueAsDateString() {
            return asDateString(this.modelValue);
        }
    },
    template: `
<span>{{modelValue}} => {{ valueAsDateString }}</span>`
}));
