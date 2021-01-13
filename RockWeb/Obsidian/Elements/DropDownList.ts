import { defineComponent, PropType } from '../Vendor/Vue/vue.js';
import { newGuid } from '../Util/Guid.js';
import { Field } from '../Vendor/VeeValidate/vee-validate.js';
import RockLabel from './RockLabel.js';

export type DropDownListOption = {
    key: string,
    value: string,
    text: string
};

export default defineComponent({
    name: 'DropDownList',
    components: {
        Field,
        RockLabel
    },
    props: {
        modelValue: {
            type: String,
            required: true
        },
        label: {
            type: String,
            required: true
        },
        disabled: {
            type: Boolean,
            default: false
        },
        options: {
            type: Array as PropType<DropDownListOption[]>,
            required: true
        },
        rules: {
            type: String as PropType<string>,
            default: ''
        },
        help: {
            type: String as PropType<string>,
            default: ''
        }
    },
    emits: [
        'update:modelValue'
    ],
    data: function () {
        return {
            uniqueId: `rock-dropdownlist-${newGuid()}`,
            internalValue: this.modelValue
        };
    },
    computed: {
        isRequired(): boolean {
            return this.rules.includes('required');
        }
    },
    methods: {
        onInput: function () {
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
    @input="onInput"
    :name="label"
    :rules="rules"
    #default="{field, errors}">
    <div class="form-group rock-drop-down-list" :class="{required: isRequired, 'has-error': Object.keys(errors).length}">
        <RockLabel :for="uniqueId" :help="help">{{label}}</RockLabel>
        <div class="control-wrapper">
            <select :id="uniqueId" class="form-control" :disabled="disabled" v-bind="field">
                <option value=""></option>
                <option v-for="o in options" :key="o.key" :value="o.value">{{o.text}}</option>
            </select>
        </div>
    </div>
</Field>`
});
