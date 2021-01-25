import { asFormattedString, toNumberOrNull } from '../Filters/Currency.js';
import { defineComponent, PropType } from '../Vendor/Vue/vue.js';
import TextBox from './TextBox.js';

export default defineComponent({
    name: 'CurrencyBox',
    components: {
        TextBox
    },
    props: {
        modelValue: {
            type: Number as PropType<number | null>,
            default: null
        }
    },
    emits: [
        'update:modelValue'
    ],
    data: function () {
        return {
            internalValue: asFormattedString(this.modelValue)
        };
    },
    watch: {
        internalValue() {
            this.$emit('update:modelValue', toNumberOrNull(this.internalValue));
        },
        value () {
            this.internalValue = asFormattedString(this.modelValue);
        }
    },
    template: `
<TextBox v-model.lazy="internalValue">
    <template #preFormControl>
        <span class="input-group-addon">$</span>
    </template>
</TextBox>`
});
