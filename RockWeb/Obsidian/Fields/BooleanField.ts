import { defineComponent, PropType } from '../Vendor/Vue/vue.js';
import { Guid } from '../Util/Guid.js';
import { registerFieldType } from './Index.js';
import { asYesNoOrNull } from '../Filters/Boolean.js';

const fieldTypeGuid: Guid = '1EDAFDED-DFE6-4334-B019-6EECBA89E05A';

export default registerFieldType(fieldTypeGuid, defineComponent({
    name: 'BooleanField',
    props: {
        modelValue: {
            type: String as PropType<string>,
            required: true
        }
    },
    computed: {
        valueAsYesNoOrNull() {
            return asYesNoOrNull(this.modelValue);
        }
    },
    template: `
<span>{{ valueAsYesNoOrNull }}</span>`
}));
