import { defineComponent, PropType } from '../Vendor/Vue/vue.js';
import { Guid } from '../Util/Guid.js';
import { registerFieldType } from './Index.js';
import { asDateString } from '../Filters/Date.js';
import DatePicker from '../Elements/DatePicker.js';

const fieldTypeGuid: Guid = '6B6AA175-4758-453F-8D83-FCD8044B5F36';

export default registerFieldType(fieldTypeGuid, defineComponent({
    name: 'DateField',
    components: {
        DatePicker
    },
    props: {
        modelValue: {
            type: String as PropType<string>,
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
        valueAsDateString() {
            return asDateString(this.modelValue);
        }
    },
    watch: {
        internalValue() {
            this.$emit('update:modelValue', this.internalValue);
        }
    },
    template: `
<DatePicker v-if="edit" v-model="internalValue" :label="label" :help="help" />
<span v-else>{{ valueAsDateString }}</span>`
}));
