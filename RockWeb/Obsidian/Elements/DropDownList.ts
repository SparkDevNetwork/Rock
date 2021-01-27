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
        },
        showBlankItem: {
            type: Boolean as PropType<boolean>,
            default: true
        }
    },
    emits: [
        'update:modelValue'
    ],
    data: function () {
        return {
            uniqueId: `rock-dropdownlist-${newGuid()}`,
            internalValue: ''
        };
    },
    computed: {
        isRequired(): boolean {
            return this.rules.includes('required');
        }
    },
    methods: {
        syncValue() {
            this.internalValue = this.modelValue;
            if (!this.showBlankItem && !this.internalValue && this.options.length) {
                this.internalValue = this.options[0].value;
                this.$emit('update:modelValue', this.internalValue);
            }
        }
    },
    watch: {
        modelValue: {
            immediate: true,
            handler() {
                this.syncValue();
            }
        },
        options: {
            immediate: true,
            handler() {
                this.syncValue();
            }
        },
        internalValue() {
            this.$emit('update:modelValue', this.internalValue);
        }
    },
    template: `
<Field
    v-model="internalValue"
    :name="label"
    :rules="rules"
    #default="{field, errors}">
    <div class="form-group rock-drop-down-list" :class="{required: isRequired, 'has-error': Object.keys(errors).length}">
        <RockLabel :for="uniqueId" :help="help">{{label}}</RockLabel>
        <div class="control-wrapper">
            <select :id="uniqueId" class="form-control" :disabled="disabled" v-bind="field">
                <option v-if="showBlankItem" value=""></option>
                <option v-for="o in options" :key="o.key" :value="o.value">{{o.text}}</option>
            </select>
        </div>
    </div>
</Field>`
});
