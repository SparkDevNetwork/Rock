import { defineComponent, PropType } from '../Vendor/Vue/vue.js';
import { Guid } from '../Util/Guid.js';
import { registerFieldType } from './Index.js';
import { asYesNoOrNull, asTrueFalseOrNull } from '../Filters/Boolean.js';
import DropDownList, { DropDownListOption } from '../Elements/DropDownList.js';

const fieldTypeGuid: Guid = '1EDAFDED-DFE6-4334-B019-6EECBA89E05A';

export default registerFieldType(fieldTypeGuid, defineComponent({
    name: 'BooleanField',
    components: {
        DropDownList
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
        },
        help: {
            type: String as PropType<string>,
            default: ''
        }
    },
    data() {
        const trueVal = asTrueFalseOrNull(true);
        const falseVal = asTrueFalseOrNull(false);
        const yesVal = asYesNoOrNull(true);
        const noVal = asYesNoOrNull(false);

        return {
            internalValue: '',
            dropDownListOptions: [
                { key: falseVal, text: noVal, value: falseVal },
                { key: trueVal, text: yesVal, value: trueVal }
            ] as DropDownListOption[]
        };
    },
    computed: {
        valueAsYesNoOrNull() {
            return asYesNoOrNull(this.modelValue);
        }
    },
    watch: {
        internalValue() {
            this.$emit('update:modelValue', this.internalValue);
        },
        modelValue: {
            immediate: true,
            handler() {
                this.internalValue = asTrueFalseOrNull(this.modelValue) || '';
            }
        }
    },
    template: `
<DropDownList v-if="edit" v-model="internalValue" :label="label" :options="dropDownListOptions" :help="help" />
<span v-else>{{ valueAsYesNoOrNull }}</span>`
}));
