import { defineComponent, PropType } from '../Vendor/Vue/vue.js';
import { Guid } from '../Util/Guid.js';
import { registerFieldType } from './Index.js';
import TextBox from '../Elements/TextBox.js';

const fieldTypeGuid: Guid = '9C204CD0-1233-41C5-818A-C5DA439445AA';

export default registerFieldType(fieldTypeGuid, defineComponent({
    name: 'TextField',
    components: {
        TextBox
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
        safeValue(): string {
            return (this.modelValue || '').trim();
        },
        valueIsNull(): boolean {
            return !this.safeValue;
        }
    },
    watch: {
        internalValue() {
            this.$emit('update:modelValue', this.internalValue);
        }
    },
    template: `
<TextBox v-if="edit" v-model="internalValue" :label="label" :help="help" />
<span v-else>{{ safeValue }}</span>`
}));
