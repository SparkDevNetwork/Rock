import { defineComponent, PropType } from '../Vendor/Vue/vue.js';
import { newGuid } from '../Util/Guid.js';
import { Field } from '../Vendor/VeeValidate/vee-validate.js';
import RockLabel from './RockLabel.js';

export default defineComponent({
    name: 'TextBox',
    components: {
        Field,
        RockLabel
    },
    props: {
        modelValue: {
            type: String as PropType<string>,
            required: true
        },
        label: {
            type: String as PropType<string>,
            default: ''
        },
        help: {
            type: String as PropType<string>,
            default: ''
        },
        type: {
            type: String as PropType<string>,
            default: 'text'
        },
        maxLength: {
            type: Number as PropType<number>,
            default: 524288
        },
        showCountDown: {
            type: Boolean as PropType<boolean>,
            default: false
        },
        rules: {
            type: String as PropType<string>,
            default: ''
        },
        disabled: {
            type: Boolean as PropType<boolean>,
            default: false
        },
        placeholder: {
            type: String as PropType<string>,
            default: ''
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
        },
        charsRemaining(): number {
            return this.maxLength - this.modelValue.length;
        },
        countdownClass(): string {
            if (this.charsRemaining >= 10) {
                return 'badge-default';
            }

            if (this.charsRemaining >= 0) {
                return 'badge-warning';
            }

            return 'badge-danger';
        }
    },
    methods: {
        handleInput() {
            this.$emit('update:modelValue', this.internalValue);
        }
    },
    watch: {
        modelValue: function () {
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
    <em v-if="showCountDown" class="pull-right badge" :class="countdownClass">
        {{charsRemaining}}
    </em>
    <div class="form-group rock-text-box" :class="{required: isRequired, 'has-error': Object.keys(errors).length}">
        <RockLabel v-if="label" :for="uniqueId" :help="help">
            {{label}}
        </RockLabel>
        <div class="control-wrapper">
            <input :id="uniqueId" :type="type" class="form-control" v-bind="field" :disabled="disabled" :maxlength="maxLength" :placeholder="placeholder" />
        </div>
    </div>
</Field>`
});
