import { ruleStringToArray, ruleArrayToString } from '../Rules/Index.js';
import { defineComponent, PropType } from '../Vendor/Vue/vue.js';
import TextBox from './TextBox.js';

export default defineComponent({
    name: 'EmailInput',
    components: {
        TextBox
    },
    props: {
        modelValue: {
            type: String as PropType<string>,
            required: true
        },
        label: {
            type: String as PropType<string>,
            default: 'Email'
        },
        rules: {
            type: String as PropType<string>,
            default: ''
        }
    },
    emits: [
        'update:modelValue'
    ],
    data: function () {
        return {
            internalValue: this.modelValue
        };
    },
    computed: {
        computedRules() {
            const rules = ruleStringToArray(this.rules);

            if (rules.indexOf('email') === -1) {
                rules.push('email');
            }

            return ruleArrayToString(rules);
        }
    },
    watch: {
        internalValue() {
            this.$emit('update:modelValue', this.internalValue);
        },
        value () {
            this.internalValue = this.modelValue;
        }
    },
    template: `
<TextBox v-model.trim="internalValue" :label="label" :rules="computedRules" />`
});
