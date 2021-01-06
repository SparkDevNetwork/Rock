import { defineComponent, PropType } from '../Vendor/Vue/vue.js';
import { newGuid } from '../Util/Guid.js';
import { Field } from '../Vendor/VeeValidate/vee-validate.js';

export default defineComponent({
    name: 'TextBox',
    components: {
        Field
    },
    props: {
        modelValue: {
            type: String as PropType<string>,
            required: true
        },
        label: {
            type: String as PropType<string>,
            required: true
        },
        type: {
            type: String as PropType<string>,
            default: 'text'
        },
        rules: {
            type: String as PropType<string>,
            default: ''
        },
        disabled: {
            type: Boolean,
            default: false
        }
    },
    emits: [
        'update:modelValue'
    ],
    data: function () {
        return {
            uniqueId: `rock-textbox-${newGuid()}`,
            internalValue: this.modelValue
        };
    },
    computed: {
        isRequired(): boolean {
            return this.rules.includes('required');
        }
    },
    methods: {
        handleInput() {
            this.$emit('update:modelValue', this.internalValue);
        }
    },
    watch: {
        value: function () {
            this.internalValue = this.modelValue;
        }
    },
    template: `
<Field
    v-model="internalValue"
    @input="handleInput"
    :name="label"
    :rules="rules"
    #default="{field, errors}">
    <div class="form-group rock-text-box" :class="{required: isRequired, 'has-error': Object.keys(errors).length}">
        <label class="control-label" :for="uniqueId">
            {{label}}
        </label>
        <div class="control-wrapper">
            <input :id="uniqueId" :type="type" class="form-control" v-bind="field" :disabled="disabled" />
        </div>
    </div>
</Field>`
});
